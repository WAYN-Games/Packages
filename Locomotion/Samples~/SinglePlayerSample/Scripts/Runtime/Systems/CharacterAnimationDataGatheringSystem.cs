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
using WAYNGames.Locomotion.Runtime.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// CharacterAnimationDataGatheringSystem is responsible for gathering animation data
    /// based on character locomotion states and updating the animation blackboard.
    /// This system processes the required entities and updates their animation-related
    /// components to align with their movement and state transitions.
    /// </summary>
    /// <remarks>
    /// This system operates within the TransformSystemGroup and runs before
    /// LocalToWorldSystem and CameraRigFollowControlledCharacterSystem, ensuring
    /// proper order of transformations and animations. It schedules the GatherAnimationDataJob
    /// to handle the computations for animation data updates.
    /// </remarks>
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(LocalToWorldSystem))]
    [UpdateBefore(typeof(CameraRigFollowControlledCharacterSystem))]
    [UpdateAfter(typeof(LocomotionInterpolationSystem))]
    [BurstCompile]
    public partial struct CharacterAnimationDataGatheringSystem : ISystem
    {
        ComponentLookup<BasicLocomotionStatesStateMachine> _characterLocomotionStateLookup;
        ComponentLookup<AnimationBlackBoard> _animationBlackBoardLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _characterLocomotionStateLookup = SystemAPI.GetComponentLookup<BasicLocomotionStatesStateMachine>(true);
            _animationBlackBoardLookup = SystemAPI.GetComponentLookup<AnimationBlackBoard>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _characterLocomotionStateLookup.Update(ref state);
            _animationBlackBoardLookup.Update(ref state);

            state.Dependency = new GatherAnimationDataJob
            {
                AnimationBlackBoardLookup = _animationBlackBoardLookup,
                CharacterLocomotionStateLookup = _characterLocomotionStateLookup
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct GatherAnimationDataJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<BasicLocomotionStatesStateMachine> CharacterLocomotionStateLookup;
            [NativeDisableParallelForRestriction] public ComponentLookup<AnimationBlackBoard> AnimationBlackBoardLookup;

            void Execute(DynamicBuffer<CharacterPath> path, in Locomotor locomotor, in Model model)
            {
                if (path.Length < 3) return;
                if (!AnimationBlackBoardLookup.HasComponent(model.Entity)) return;
                if (!CharacterLocomotionStateLookup.HasComponent(locomotor.Entity)) return;
                var bb = new AnimationBlackBoard();

                GetForwardAndRightSpeed(path[1].Transform, path[0].Transform, path[0].Time, out var currentForwardSpeed,
                    out var currentUpwardSpeed, out var currentRightSpeed);

                BasicLocomotionStates locomotionState =
                    CharacterLocomotionStateLookup[locomotor.Entity].CurrentState;

                bb.ZVel = BasicLocomotionStates.Idle == locomotionState ? 0 : currentForwardSpeed;
                bb.YVel = BasicLocomotionStates.Idle == locomotionState ? 0 : currentUpwardSpeed;
                bb.XVel = BasicLocomotionStates.Idle == locomotionState ? 0 : currentRightSpeed;
                bb.Jumping = BasicLocomotionStates.Jumping == locomotionState;

                AnimationBlackBoardLookup[model.Entity] = bb;
            }
        }

        /// <summary>
        ///     Computes forward and right speeds (in world units per second)
        ///     between two RigidTransforms.
        /// </summary>
        /// <param name="prev">The previous transform (position + rotation).</param>
        /// <param name="curr">The current transform.</param>
        /// <param name="deltaTime">Time elapsed from prev → curr.</param>
        /// <param name="forwardSpeed">Output: speed along the forward (Z) axis.</param>
        /// <param name="upwardSpeed">Output: speed along the forward (Y) axis.</param>
        /// <param name="rightSpeed">Output: speed along the right (X) axis.</param>
        public static void GetForwardAndRightSpeed(
            RigidTransform prev,
            RigidTransform curr,
            float deltaTime,
            out float forwardSpeed,
            out float upwardSpeed,
            out float rightSpeed)
        {
            // 1) Compute world-space velocity vector
            float3 deltaPos = curr.pos - prev.pos;
            float3 velocity = deltaPos / deltaTime;

            // 2) Extract the world-space forward/right axes from the current rotation
            //    In Unity.Mathematics, forward is +Z, right is +X
            float3 forward = math.mul(curr.rot, new float3(0, 0, 1));
            float3 right = math.mul(curr.rot, new float3(1, 0, 0));
            float3 up = math.mul(curr.rot, new float3(0, 1, 0));

            // 3) Project velocity onto those axes
            forwardSpeed = math.dot(velocity, forward);
            rightSpeed = math.dot(velocity, right);
            upwardSpeed = math.dot(velocity, up);
        }
    }
}