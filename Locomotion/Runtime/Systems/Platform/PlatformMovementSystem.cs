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
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine.Splines;

namespace Locomotion.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    public partial struct PlatformMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (platform, track, localTransform) in
                     SystemAPI.Query<RefRW<Platform>,
                         DynamicBuffer<Track>,
                         RefRW<LocalTransform>>())
            {
                var currentIndex = platform.ValueRO.Index;
                var targetIndex = (currentIndex + 1) % track.Length;
                var targetPosition = track[targetIndex].Position;
                var currentPosition = localTransform.ValueRO.Position;

                var direction = math.normalize(targetPosition - currentPosition);
                var distanceToMove = platform.ValueRO.Speed * dt;
                var newPosition = currentPosition + direction * distanceToMove;

                if (math.distancesq(newPosition, targetPosition) < 0.01f)
                {
                    newPosition = targetPosition;
                    platform.ValueRW.Index = targetIndex;
                }
                localTransform.ValueRW.Position = newPosition;
            }
            
            
            foreach (var (rotor, localTransform) in
                     SystemAPI.Query<RefRW<Rotor>,
                         RefRW<LocalTransform>>())
            {
                var rotationAngle = rotor.ValueRO.Axis * dt;
                localTransform.ValueRW = localTransform.ValueRW.RotateX(rotationAngle.x);
                localTransform.ValueRW = localTransform.ValueRW.RotateY(rotationAngle.y);
                localTransform.ValueRW = localTransform.ValueRW.RotateZ(rotationAngle.z);
            }
        }
    }
}