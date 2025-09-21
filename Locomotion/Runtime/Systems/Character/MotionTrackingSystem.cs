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

namespace Locomotion.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct MotionTrackingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (trackedMotion, transform) in
                     SystemAPI.Query<RefRW<TrackedMotion>, RefRO<LocalTransform>>())
            {
                trackedMotion.ValueRW.PreviousTransform = trackedMotion.ValueRO.CurrentTransform;
                trackedMotion.ValueRW.CurrentTransform =
                    new RigidTransform(transform.ValueRO.Rotation, transform.ValueRO.Position);
                trackedMotion.ValueRW.DeltaTime = SystemAPI.Time.DeltaTime;
            }
        }
    }
}