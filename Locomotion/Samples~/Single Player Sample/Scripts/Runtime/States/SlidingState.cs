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


using Locomotion.Runtime.Components;
using Unity.Mathematics;
using Unity.Profiling;
using Wayn.Locomotion.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    internal struct
        SlidingState : ILocomotionState<BasicLocomotionContext, BasicLocomotionProfile, BasicLocomotionInput,
        BasicLocomotionStateData>
    {
        static readonly ProfilerMarker KStateMachineIntegrateStatePerfMarker = new("SlidingState.OnIntegrateState");

        static readonly ProfilerMarker KStateMachineResolveStateCollisionsPerfMarker =
            new("SlidingState.OnResolveStateCollisions");

        internal static void OnEnterState(ref BasicLocomotionContext ctx)
        {
        }

        internal static void OnIntegrateState(ref BasicLocomotionContext ctx)
        {
            KStateMachineIntegrateStatePerfMarker.Begin();
            var fallTerminalVelocity = ctx.ProfileValue.Falling.TerminalVelocity;
            var fallGravityMultiplier = ctx.ProfileValue.Falling.GravityMultiplier;
            FallingState.ComputeGravityAcceleration(ref ctx, fallTerminalVelocity, out float3 newVerticalAcceleration,
                fallGravityMultiplier);

            ref BaseLocomotionContext baseContext = ref ctx.BaseContext;
            baseContext.UpdateVelocitiesAndComputeDisplacements(float3.zero, newVerticalAcceleration);
            KStateMachineIntegrateStatePerfMarker.End();
        }

        internal static void OnResolveStateCollisions(ref BasicLocomotionContext ctx)
        {
            KStateMachineResolveStateCollisionsPerfMarker.Begin();
            CharacterPhysicsProfile profilePropertyPhysicsProfile = ctx.ProfileValue.PhysicsProfileValue;
            MathematicsHelpers.SimpleSlideProjection(ref ctx.BaseContext);
            PhysicsOperations.MoveUpToNextObstacle(ref ctx.BaseContext, in profilePropertyPhysicsProfile);
            KStateMachineResolveStateCollisionsPerfMarker.End();
        }

        internal static void EvaluateTransitions(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            if (SlidingToFalling(ref ctx, out newState)) return;
            if (SlidingToGrounded(ref ctx, out newState)) return;
        }

        static void OnExitState(ref BasicLocomotionContext ctx)
        {
        }

        static bool SlidingToFalling(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Sliding;
            if (ctx.BaseContext.IsInContact) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Falling;
            FallingState.OnEnterState(ref ctx);
            return true;
        }

        static bool SlidingToGrounded(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Sliding;
            if (ctx.IsSlopeAboveMax) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Moving;
            MovingState.OnEnterState(ref ctx);
            return true;
        }
    }
}