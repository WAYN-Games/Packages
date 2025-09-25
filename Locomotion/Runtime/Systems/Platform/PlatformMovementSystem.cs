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
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WAYNGames.Locomotion.Runtime.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial struct PlatformMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            state.Dependency = new PlatformMovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
            state.Dependency = new PlatformRotateJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);

        }
    }
    [BurstCompile]
    public partial struct PlatformRotateJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(ref Rotor rotor, ref LocalTransform localTransform)
        {
            var rotationAngle = rotor.Axis * DeltaTime;
            localTransform = localTransform.RotateX(rotationAngle.x);
            localTransform = localTransform.RotateY(rotationAngle.y);
            localTransform = localTransform.RotateZ(rotationAngle.z);
        }
    }
    
    
    [BurstCompile]
    public partial struct PlatformMovementJob : IJobEntity
    {
        public float DeltaTime;

        const float PositionTolerance = 0.01f;

        void Execute(ref Platform platform, ref LocalTransform localTransform, in DynamicBuffer<Track> track)
        {
            int currentIndex = platform.Index;
            int nextIndex = (currentIndex + 1) % track.Length;

            float3 targetPosition = track[nextIndex].Position;
            float3 currentPosition = localTransform.Position;

            float3 direction = math.normalize(targetPosition - currentPosition);
            float distanceToMove = platform.Speed * DeltaTime;
            float3 newPosition = currentPosition + direction * distanceToMove;

            if (math.distancesq(newPosition, targetPosition) < PositionTolerance)
            {
                newPosition = targetPosition;
                platform.Index = nextIndex;
            }

            localTransform.Position = newPosition;
        }
    }

}