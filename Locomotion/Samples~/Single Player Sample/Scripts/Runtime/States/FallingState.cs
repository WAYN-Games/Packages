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
    /// Contains parameters related to the falling behavior of a character in the locomotion system.
    /// </summary>
    [Serializable]
    public struct FallingProfile
    {
        /// <summary>Maximum vertical speed.</summary>
        public float TerminalVelocity;

        /// <summary>Acceleration multiplayer to the standard gravity.</summary>
        public float GravityMultiplier;
    }

    
    internal struct
        FallingState : ILocomotionState<BasicLocomotionContext, BasicLocomotionProfile, BasicLocomotionInput,
        BasicLocomotionStateData>
    {
        static readonly ProfilerMarker KStateMachineIntegrateStatePerfMarker = new("FallingState.OnIntegrateState");

        static readonly ProfilerMarker KStateMachineResolveStateCollisionsPerfMarker =
            new("FallingState.OnResolveStateCollisions");

        internal static void OnEnterState(ref BasicLocomotionContext ctx)
        {
        }

        internal static void OnIntegrateState(ref BasicLocomotionContext ctx)
        {
            KStateMachineIntegrateStatePerfMarker.Begin();
            var fallTerminalVelocity = ctx.ProfileValue.Falling.TerminalVelocity;
            var fallGravityMultiplier = ctx.ProfileValue.Falling.GravityMultiplier;
            ComputeGravityAcceleration(ref ctx, fallTerminalVelocity, out float3 newVerticalAcceleration,
                fallGravityMultiplier);


            ref BaseLocomotionContext baseContext = ref ctx.BaseContext;

            MovingState.ComputeHorizontalMovementAcceleration(ref baseContext, ctx.ProfileValue.Moving,
                ctx.InputValue.MovementDirection, ctx.InputValue.MovementMagnitude,
                out float3 newHorizontalAcceleration);

            baseContext.UpdateVelocitiesAndComputeDisplacements(newHorizontalAcceleration, newVerticalAcceleration);
            KStateMachineIntegrateStatePerfMarker.End();
        }

        public static void ComputeGravityAcceleration(ref BasicLocomotionContext ctx, float fallTerminalVelocity,
            out float3 newVerticalAcceleration, float fallGravityMultiplier = 1f)
        {
            ref BaseLocomotionContext baseContext = ref ctx.BaseContext;
            var timeStep = baseContext.TimeStep;
            float3 desiredVelocity = baseContext.GravityDirection * fallTerminalVelocity;
            var maxGravitationalAcceleration = baseContext.GravityStrength * fallGravityMultiplier;
            MathematicsHelpers.ComputeAccelerationToTargetVelocity(out newVerticalAcceleration,
                in baseContext.VerticalVelocity, in desiredVelocity, timeStep, in maxGravitationalAcceleration);
        }

        internal static void OnResolveStateCollisions(ref BasicLocomotionContext ctx)
        {
            KStateMachineResolveStateCollisionsPerfMarker.Begin();
            CharacterPhysicsProfile characterPhysicsProfile = ctx.ProfileValue.PhysicsProfileValue;
            PhysicsOperations.MoveUpToNextObstacle(ref ctx.BaseContext, in characterPhysicsProfile);
            KStateMachineResolveStateCollisionsPerfMarker.End();
        }

        internal static void EvaluateTransitions(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            if (FallingToSliding(ref ctx, out newState)) return;
            if (FallingToGrounded(ref ctx, out newState)) return;
        }

        static void OnExitState(ref BasicLocomotionContext ctx)
        {
        }

        static bool FallingToGrounded(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Falling;
            if (!ctx.BaseContext.IsInContact) return false;
            if (ctx.IsSlopeAboveMax) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Moving;
            MovingState.OnEnterState(ref ctx);
            return true;
        }

        static bool FallingToSliding(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Falling;
            if (!ctx.BaseContext.IsInContact) return false;
            if (!ctx.IsSlopeAboveMax) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Sliding;
            SlidingState.OnEnterState(ref ctx);
            return true;
        }
    }
}