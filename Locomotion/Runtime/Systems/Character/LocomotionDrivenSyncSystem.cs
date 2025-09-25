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
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace WAYNGames.Locomotion.Runtime.Systems
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    [BurstCompile]
    public partial struct LocomotionDrivenSyncSystem : ISystem
    {
        ComponentLookup<LocalTransform> m_LocalTransformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_LocalTransformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_LocalTransformLookup.Update(ref state);
            var job = new SyncDrivingTransformsJob
            {
                TransformLookup = m_LocalTransformLookup,
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = job.ScheduleParallelByRef(state.Dependency);
        }


        [BurstCompile]
        internal partial struct SyncDrivingTransformsJob : IJobEntity
        {
            // The driving and controlled entity can't be the same so it is safe to write in parallel.
            [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
            [ReadOnly] public float DeltaTime;

            void Execute(DynamicBuffer<CharacterPath> path, in Locomotor character)
            {
                path.Insert(0, new CharacterPath
                {
                    Transform = new RigidTransform(TransformLookup[character.Entity].ToMatrix()),
                    Time = DeltaTime
                });
                path.ResizeUninitialized(3);
            }
        }
    }
}