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
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace WAYNGames.Locomotion.Runtime.StateMachine
{
    [BurstCompile]
    public struct LocomotionJob<TStateMachine, TStates, TContext, TProfile, TInput, TStateData> : IJobChunk
        where TContext : unmanaged, ILocomotionContext<TProfile, TInput, TStateData>
        where TProfile : unmanaged, ILocomotionProfile
        where TStateMachine : unmanaged,
        ILocomotionStateMachine<TStateMachine, TStates, TContext, TProfile, TInput, TStateData>, IComponentData
        where TInput : unmanaged, IComponentData, ICharacterLocomotionInput<TInput>
        where TStateData : unmanaged, IComponentData
        where TStates : unmanaged
    {
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<TrackedMotion> PlatformMotionFrameLookup;
        public float DeltaTime;

        public EntityTypeHandle Entity;

        [ReadOnly] public ComponentTypeHandle<PhysicsCollider> PhysicsCollider;
        [ReadOnly] public ComponentTypeHandle<LocomotionGravity> LocomotionGravity;
        [ReadOnly] public ComponentTypeHandle<LocomotionProfileBlob<TProfile>> LocomotionProfile;

        public ComponentTypeHandle<TStateMachine> LocomotionStateMachine;
        public ComponentTypeHandle<LocalTransform> Transform;
        public ComponentTypeHandle<LocomotionVelocity> LocomotionVelocity;
        public ComponentTypeHandle<LocomotionContact> LocomotionContact;
        public ComponentTypeHandle<TInput> LocomotionInput;
        public ComponentTypeHandle<TStateData> LocomotionStateData;


        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            TContext context = default;

            var entities = chunk.GetNativeArray(Entity);
            var physicsColliders = chunk.GetNativeArray(ref PhysicsCollider);
            var locomotionGravities = chunk.GetNativeArray(ref LocomotionGravity);
            var transforms = chunk.GetNativeArray(ref Transform);
            var locomotionProfiles = chunk.GetNativeArray(ref LocomotionProfile);
            var locomotionVelocities = chunk.GetNativeArray(ref LocomotionVelocity);
            var locomotionContacts = chunk.GetNativeArray(ref LocomotionContact);
            var locomotionStateMachines = chunk.GetNativeArray(ref LocomotionStateMachine);
            var locomotionInputs = chunk.GetNativeArray(ref LocomotionInput);
            var locomotionStateDatas = chunk.GetNativeArray(ref LocomotionStateData);

            for (var index = 0; index < chunk.Count; index++)
            {
                TStateMachine stateMachine = locomotionStateMachines[index];
                BaseLocomotionContext locomotionContext = default;
                locomotionContext.TimeStep = DeltaTime;
                locomotionContext.PhysicsWorld = PhysicsWorld;
                locomotionContext.PlatformMotionFrameLookup = PlatformMotionFrameLookup;
                locomotionContext.CharacterEntity = entities[index];
                locomotionContext.LocomotionContact = new RefRW<LocomotionContact>(locomotionContacts, index);
                locomotionContext.CharacterPosition = new RefRW<LocalTransform>(transforms, index);
                locomotionContext.GravityStrength = math.length(locomotionGravities[index].GravityStrength);
                locomotionContext.GravityDirection = locomotionGravities[index].GravityDirection;
                locomotionContext.Collider = physicsColliders[index].Value;
                locomotionContext.InheritedRotation = quaternion.identity;

                locomotionContext.VerticalVelocity = math.project(locomotionVelocities[index].Velocity,
                    -locomotionGravities[index].GravityDirection);
                locomotionContext.HorizontalVelocity =
                    locomotionVelocities[index].Velocity - locomotionContext.VerticalVelocity;


                context.BaseContextValue = locomotionContext;

                context.InputValue = locomotionInputs[index];
                context.StateDataValue = locomotionStateDatas[index];
                context.ProfileValue = locomotionProfiles[index].Profile.Value;


                ILocomotionStateMachine<TStateMachine, TStates, TContext, TProfile, TInput, TStateData>.Execute(
                    ref stateMachine,
                    ref context);


                locomotionInputs[index] = context.InputValue;
                locomotionStateDatas[index] = context.StateDataValue;
                locomotionVelocities[index] = new LocomotionVelocity
                {
                    Velocity = context.BaseContextValue.HorizontalVelocity + context.BaseContextValue.VerticalVelocity
                };
                locomotionStateMachines[index] = stateMachine;
            }
        }
    }
}