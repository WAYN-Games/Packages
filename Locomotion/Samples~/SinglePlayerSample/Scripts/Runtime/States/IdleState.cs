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


using WAYNGames.Locomotion.Runtime.Components;
using Unity.Mathematics;
using Unity.Profiling;
using WAYNGames.Locomotion.Runtime.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    internal struct IdleState : ILocomotionState<BasicLocomotionContext, BasicLocomotionProfile, BasicLocomotionInput,
        BasicLocomotionStateData>
    {
        static readonly ProfilerMarker KStateMachineIntegrateStatePerfMarker = new("IdleState.OnIntegrateState");

        static readonly ProfilerMarker KStateMachineResolveStateCollisionsPerfMarker =
            new("IdleState.OnResolveStateCollisions");

        internal static void OnEnterState(ref BasicLocomotionContext ctx)
        {
        }

        internal static void OnIntegrateState(ref BasicLocomotionContext ctx)
        {
            KStateMachineIntegrateStatePerfMarker.Begin();
            ref BaseLocomotionContext baseContext = ref ctx.BaseContext;

            // Manage rotation on moving platform :
            // if there is no contact with wall or ceiling, spin with the platform rotation
            // otherwise either spin, slide or roll along the wall/ceiling
            var sign = math.sign(math.dot(
                math.cross(baseContext.GravityDirection, baseContext.LocomotionContact.ValueRO.Contact.Normal),
                math.normalizesafe(baseContext.HorizontalInheritedVelocity)));

            var interpolation = ctx.IsSlopeAboveMax
                ? (sign * ctx.ProfileValue.PhysicsProfileValue.SpinOrRollAlongWall + 1) / 2
                : 0;
            quaternion rotation = math.slerp(baseContext.InheritedRotation, math.inverse(baseContext.InheritedRotation),
                interpolation);
            baseContext.CharacterPosition.ValueRW.Rotation =
                math.mul(rotation, baseContext.CharacterPosition.ValueRW.Rotation);
            KStateMachineIntegrateStatePerfMarker.End();
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
            if (IdleToGrounded(ref ctx, out newState)) return;
            if (IdleToFalling(ref ctx, out newState)) return;
            if (IdleToSliding(ref ctx, out newState)) return;
            if (IdleToJumping(ref ctx, out newState)) return;
        }

        static void OnExitState(ref BasicLocomotionContext ctx)
        {
        }

        static bool IdleToFalling(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Idle;
            if (ctx.BaseContext.IsInContact) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Falling;
            FallingState.OnEnterState(ref ctx);
            return true;
        }


        static bool IdleToGrounded(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Idle;
            if (!ctx.WantsToMove) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Moving;
            MovingState.OnEnterState(ref ctx);
            return true;
        }

        static bool IdleToSliding(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Idle;
            if (!ctx.IsSlopeAboveMax) return false;
            if (ctx.BaseContext.IsCeiling) return false;
            if (ctx.IsWall) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Sliding;
            SlidingState.OnEnterState(ref ctx);
            return true;
        }
        
        static bool IdleToJumping(ref BasicLocomotionContext ctx,
            out BasicLocomotionStates newState)
        {
            newState = BasicLocomotionStates.Idle;
            if (!ctx.BaseContext.IsInContact) return false;
            if (ctx.InputValue.JumpTrigger <= 0) return false;
            OnExitState(ref ctx);
            newState = BasicLocomotionStates.Jumping;
            JumpingState.OnEnterState(ref ctx);
            return true;
        }
    }
}