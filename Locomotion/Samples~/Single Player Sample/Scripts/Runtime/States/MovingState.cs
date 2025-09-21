// /*
//  * Copyright (c) 2025 WAYN Games.
//  *
//  * All rights reserved. This software and associated documentation files
//  * (the "Software") are the exclusive property of WAYN Games.
//  * Unauthorized copying of this file, via any medium, is strictly prohibited.
//  *
//  * This software may not be reproduced, distributed, or used in any manner
//  * whatsoever without the express written permission of the author, except for
//  * the use of brief quotations in a review or other non-commercial uses permitted
//  * by copyright law.
//  *
//  * For permissions, contact: contact@wayn.games
//  */

using System;
using Locomotion.Runtime.Components;
using Unity.Mathematics;
using Unity.Profiling;
using Wayn.Locomotion.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    ///     Settings governing basic movement behavior (speed, acceleration, stepping, turning).
    /// </summary>
    [Serializable]
    public struct MovingProfile
    {
        /// <summary>Maximum horizontal speed.</summary>
        public float MaxSpeed;

        /// <summary>Maximum slope angle (in degrees) the character can climb.</summary>
        public float MaxSlope;

        /// <summary>Time taken to accelerate from 0 to full speed.</summary>
        public float TimeToMaxSpeed;

        /// <summary>Time taken to decelerate from full speed to a stop.</summary>
        public float TimeToStop;

        /// <summary>Maximum height of obstacles the character can step over.</summary>
        public float MaxStepHeight;

        /// <summary>How sharply the character reaches target speed or direction.</summary>
        public float Sharpness;
    }


    internal struct MovingState : ILocomotionState<BasicLocomotionContext, BasicLocomotionProfile, BasicLocomotionInput,
        BasicLocomotionStateData>
    {
        static readonly ProfilerMarker KStateMachineIntegrateStatePerfMarker = new("MovingState.OnIntegrateState");

        static readonly ProfilerMarker KStateMachineResolveStateCollisionsPerfMarker =
            new("MovingState.OnResolveStateCollisions");

        internal static void OnEnterState(ref BasicLocomotionContext ctx)
        {
            // The moving state is a grounded state therefore there should be no vertical velocity,
            // we enforce that by setting the vertical velocity to zero upon state entry,
            // cancelling any momentum we could have from falling or jumping.
            ctx.BaseContext.VerticalVelocity = float3.zero;
        }

        internal static void OnIntegrateState(ref BasicLocomotionContext ctx)
        {
            KStateMachineIntegrateStatePerfMarker.Begin();
            ref BaseLocomotionContext baseContext = ref ctx.BaseContext;

            ComputeHorizontalMovementAcceleration(ref baseContext, ctx.ProfileValue.Moving,
                ctx.InputValue.MovementDirection, ctx.InputValue.MovementMagnitude,
                out float3 newHorizontalAcceleration);

            baseContext.UpdateVelocitiesAndComputeDisplacements(newHorizontalAcceleration);

            // We make the character rotate toward the horizontal velocity to always have it face the movement direction.
            ctx.BaseContext.CharacterPosition.ValueRW.Rotation =
                quaternion.LookRotationSafe(ctx.BaseContext.HorizontalVelocity, -ctx.BaseContext.GravityDirection);
            KStateMachineIntegrateStatePerfMarker.End();
        }

        /// Computes the horizontal movement acceleration based on the current locomotion context, movement profile,
        /// and input movement direction and magnitude, while ensuring a smooth transition to the target velocity.
        /// <param name="baseContext">
        ///     The base context containing locomotion state, including current horizontal velocity and
        ///     timestep.
        /// </param>
        /// <param name="movingProfile">
        ///     Profile data that defines the movement behavior, such as maximum speed and time to reach
        ///     target speed or stop.
        /// </param>
        /// <param name="movementDirection">The normalized direction vector representing the desired movement direction.</param>
        /// <param name="movementMagnitude">
        ///     The scalar value representing the magnitude of movement input (e.g., joystick
        ///     intensity).
        /// </param>
        /// <param name="newHorizontalAcceleration">The resulting computed horizontal acceleration to achieve the desired velocity.</param>
        public static void ComputeHorizontalMovementAcceleration(ref BaseLocomotionContext baseContext,
            MovingProfile movingProfile, float3 movementDirection, float movementMagnitude,
            out float3 newHorizontalAcceleration)
        {
            var wantsToMove = movementMagnitude > math.EPSILON;

            float3 horizontalVelocity = baseContext.HorizontalVelocity;
            var horizontalVelocityMagnitude = math.length(horizontalVelocity);
            float3 horizontalVelocityDirection = horizontalVelocityMagnitude == 0
                ? horizontalVelocityMagnitude
                : horizontalVelocity / horizontalVelocityMagnitude;


            if (wantsToMove)
            {
                // This allows to make the character turn sharper than its natural turn speed
                baseContext.HorizontalVelocity -= horizontalVelocity;
                baseContext.HorizontalVelocity += horizontalVelocityMagnitude * math.lerp(horizontalVelocityDirection,
                    movementDirection, movingProfile.Sharpness);
            }


            float3 desiredVelocity =
                movementDirection * movementMagnitude * movingProfile.MaxSpeed;
            var accel = math.select(
                movingProfile.MaxSpeed / movingProfile.TimeToStop,
                movingProfile.MaxSpeed / movingProfile.TimeToMaxSpeed,
                wantsToMove);
            MathematicsHelpers.ComputeAccelerationToTargetVelocity(out newHorizontalAcceleration,
                baseContext.HorizontalVelocity, desiredVelocity, baseContext.TimeStep, accel);
        }

        internal static void OnResolveStateCollisions(ref BasicLocomotionContext ctx)
        {
            using (KStateMachineResolveStateCollisionsPerfMarker.Auto())
            {
                CharacterPhysicsProfile characterPhysicsProfile = ctx.ProfileValue.PhysicsProfileValue;
                MovingProfile movingProfile = ctx.ProfileValue.Moving;

                if (!ctx.IsSlopeAboveMax)
                {
                    MathematicsHelpers.SimpleGroundProjection(ref ctx.BaseContext);
                    PhysicsOperations.MoveUpToNextObstacle(ref ctx.BaseContext, in characterPhysicsProfile);
                    return;
                }


                if (ctx.CouldBeAStep(out float3 stepOffset))
                    if (PhysicsOperations.TryStep(ref ctx.BaseContext, in characterPhysicsProfile, stepOffset,
                            movingProfile.MaxStepHeight))
                        return;


                if (ctx.BaseContext.IsMovingAgainstSlope)
                {
                    MathematicsHelpers.WallProjection(ref ctx.BaseContext);
                    PhysicsOperations.MoveUpToNextObstacle(ref ctx.BaseContext, in characterPhysicsProfile);
                    return;
                }

                var distanceToCheck = movingProfile.MaxStepHeight +
                                      characterPhysicsProfile.ShellWidth;

                PhysicsOperations.CheckForStepDown(ref ctx.BaseContext, distanceToCheck);
                if (ctx.IsSlopeAboveMax) return;
                PhysicsOperations.MoveUpToNextObstacle(ref ctx.BaseContext, in characterPhysicsProfile);
            }
        }


        internal static void EvaluateTransitions(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            if (GroundedToIdle(ref ctx, out newState)) return;
            if (GroundedToFalling(ref ctx, out newState)) return;
            if (GroundedToSliding(ref ctx, out newState)) return;
            if (GroundedToJumping(ref ctx, out newState)) return;
        }

        static void OnExitState(ref BasicLocomotionContext ctx)
        {
        }

        static bool GroundedToSliding(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Moving;
            if (!ctx.IsSlopeAboveMax) return false;
            if (ctx.BaseContext.IsCeiling) return false;
            if (ctx.BaseContext.IsMovingAgainstSlope) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Sliding;
            SlidingState.OnEnterState(ref ctx);
            return true;
        }

        static bool GroundedToIdle(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Moving;
            if (ctx.BaseContext.IsMoving || ctx.WantsToMove) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Idle;
            IdleState.OnEnterState(ref ctx);
            return true;
        }

        static bool GroundedToFalling(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Moving;
            if (ctx.BaseContext.IsInContact) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Falling;
            FallingState.OnEnterState(ref ctx);
            return true;
        }

        static bool GroundedToJumping(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Moving;
            if (!ctx.BaseContext.IsInContact) return false;
            if (ctx.InputValue.JumpTrigger <= 0) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Jumping;
            JumpingState.OnEnterState(ref ctx);
            return true;
        }
    }
}