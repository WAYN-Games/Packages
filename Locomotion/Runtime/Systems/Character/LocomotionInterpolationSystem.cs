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
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Locomotion.Systems
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(LocalToWorldSystem))]
    [UpdateBefore(typeof(CameraRigFollowControlledCharacterSystem))]
    [BurstCompile]
    public partial struct LocomotionInterpolationSystem : ISystem
    {
        ComponentLookup<LocalToWorld> m_LocalToWorldLookup;

        public void OnCreate(ref SystemState state)
        {
            m_LocalToWorldLookup = state.GetComponentLookup<LocalToWorld>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_LocalToWorldLookup.Update(ref state);
            var job = new InterpolationJob
            {
                ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
                LocalToWorldLookup = m_LocalToWorldLookup
            };
            state.Dependency = job.ScheduleParallelByRef(state.Dependency);
        }


        [BurstCompile]
        public partial struct InterpolationJob : IJobEntity
        {
            [ReadOnly] public float ElapsedTime;
            [NativeDisableParallelForRestriction] public ComponentLookup<LocalToWorld> LocalToWorldLookup;

            void Execute(Entity self, in Character character, in DynamicBuffer<CharacterPath> path,
                ref LocalTransform localTransform)
            {
                if (path.Length < 2) return;
                RigidTransform current = path[0].Transform;
                RigidTransform previous = path[1].Transform;

                var interpolationFactor = math.saturate(math.frac(ElapsedTime / path[0].Time));
                // Interpolate position, rotation, scale
                float3 pos = math.lerp(previous.pos, current.pos, interpolationFactor);
                quaternion rot = math.slerp(previous.rot, current.rot, interpolationFactor);

                localTransform = LocalTransform.FromPositionRotation(pos, rot);
                LocalToWorld worldTransform = LocalToWorldLookup[character.Model];
                worldTransform.Value = localTransform.ToMatrix();
                LocalToWorldLookup[self] = worldTransform;
                LocalToWorldLookup[character.Model] = worldTransform;
            }
        }
    }
}