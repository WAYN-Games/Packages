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
    /// Defines the properties associated with jumping behavior in a locomotion system.
    /// </summary>
    [Serializable]
    public struct JumpingProfile
    {
        /// <summary>Maximum vertical speed.</summary>
        public float Force;

        /// <summary>Maximum number of jumps.</summary>
        public int MaxJumps;
    }

    [Serializable]
    public struct JumpData
    {
        public int JumpsCount;
    }

    internal struct
        JumpingState : ILocomotionState<BasicLocomotionContext, BasicLocomotionProfile, BasicLocomotionInput,
        BasicLocomotionStateData>
    {
        static readonly ProfilerMarker KStateMachineIntegrateStatePerfMarker = new("JumpingState.OnIntegrateState");

        static readonly ProfilerMarker KStateMachineResolveStateCollisionsPerfMarker =
            new("JumpingState.OnResolveStateCollisions");

        internal static void OnEnterState(ref BasicLocomotionContext ctx)
        {
            ctx.BaseContext.LocomotionContact.ValueRW = default;
        }

        internal static void OnIntegrateState(ref BasicLocomotionContext ctx)
        {
            KStateMachineIntegrateStatePerfMarker.Begin();
            if (ctx.InputValue.JumpTrigger > 0 && ctx.StateData.JumpData.JumpsCount < ctx.ProfileValue.Jumping.MaxJumps)
            {
                Jump(ref ctx);
                ConsumeJumpTrigger(ref ctx);
            }

            var fallTerminalVelocity = ctx.ProfileValue.Falling.TerminalVelocity;
            var fallGravityMultiplier = ctx.ProfileValue.Falling.GravityMultiplier;
            FallingState.ComputeGravityAcceleration(ref ctx, fallTerminalVelocity, out float3 newVerticalAcceleration,
                fallGravityMultiplier);

            ref BaseLocomotionContext baseContext = ref ctx.BaseContext;

            MovingState.ComputeHorizontalMovementAcceleration(ref baseContext, ctx.ProfileValue.Moving,
                ctx.InputValue.MovementDirection, ctx.InputValue.MovementMagnitude,
                out float3 newHorizontalAcceleration);

            baseContext.UpdateVelocitiesAndComputeDisplacements(newHorizontalAcceleration, newVerticalAcceleration);
            KStateMachineIntegrateStatePerfMarker.End();
        }

        static void Jump(ref BasicLocomotionContext ctx)
        {
            ctx.BaseContext.VerticalVelocity = ctx.ProfileValue.Jumping.Force * -ctx.BaseContext.GravityDirection;
            ctx.StateData.JumpData.JumpsCount++;
        }

        static void ConsumeJumpTrigger(ref BasicLocomotionContext ctx)
        {
            BasicLocomotionInput value = ctx.InputValue;
            value.JumpTrigger = 0;
            ctx.InputValue = value;
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
            if (JumpingToSliding(ref ctx, out newState)) return;
            if (JumpingToGrounded(ref ctx, out newState)) return;
        }

        static void OnExitState(ref BasicLocomotionContext ctx)
        {
            ctx.StateData.JumpData.JumpsCount = 0;
        }

        static bool JumpingToGrounded(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Jumping;
            if (!ctx.BaseContext.IsInContact) return false;
            if (ctx.IsSlopeAboveMax) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Moving;
            MovingState.OnEnterState(ref ctx);
            return true;
        }

        static bool JumpingToSliding(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Jumping;
            if (!ctx.BaseContext.IsInContact) return false;
            if (!ctx.IsSlopeAboveMax) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Sliding;
            SlidingState.OnEnterState(ref ctx);
            return true;
        }
    }
}