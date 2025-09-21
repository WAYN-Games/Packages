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

using Locomotion.Runtime.Authoring.Player;
using Locomotion.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine.Jobs;

namespace Locomotion.Systems
{
    // We update before the PresentationGoTransformSyncSystem so its update only once per frame after character movements.
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct CameraRigFollowControlledCharacterSystem : ISystem
    {
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_TransformAccessArray =
            new("TransformAccessArray");

        TransformAccessArray _taa;
        ComponentLookup<LocalToWorld> _transformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalTransform>();
            _transformLookup = state.GetComponentLookup<LocalToWorld>(true);
            _taa = new TransformAccessArray(0);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_taa.isCreated)
                _taa.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            k_LocomotionCameraTargetTrackerSystem_TransformAccessArray.Begin();
            // Clear last frame’s data
            if (_taa.isCreated)
                _taa.Dispose();
            _taa = new TransformAccessArray(0);
            var entities = new NativeList<Entity>(state.WorldUpdateAllocator);
            var entityCounts = new NativeList<int>(state.WorldUpdateAllocator);
            var entityStarts = new NativeList<int>(state.WorldUpdateAllocator);


            // Gather all (CameraTarget, Entity) pairs on the main thread
            foreach (var (controlledCharacters, playerCameraRig) in SystemAPI
                         .Query<DynamicBuffer<ControlledCharacters>, RefRO<PlayerGameObject>>())
            {
                var cameraRig = playerCameraRig.ValueRO.Instance.Value.GetComponent<PlayerGoAuthoring>();
                if (cameraRig.CameraTarget == null) continue;
                if (controlledCharacters.IsEmpty) continue;

                _taa.Add(cameraRig.CameraTarget);
                entityCounts.Add(controlledCharacters.Length);
                entityStarts.Add(entities.Length);
                foreach (ControlledCharacters controlledCharacter in controlledCharacters)
                    entities.Add(controlledCharacter.Character.Model);
            }

            k_LocomotionCameraTargetTrackerSystem_TransformAccessArray.End();

            // Schedule the TransformJob in parallel using AsDeferredJobArray to avoid hard sync-point

            _transformLookup.Update(ref state);
            state.Dependency = new ApplyCameraTransformJob
            {
                Entities = entities.AsDeferredJobArray(),
                EntityCounts = entityCounts.AsDeferredJobArray(),
                EntityStarts = entityStarts.AsDeferredJobArray(),
                TransformLookup = _transformLookup
            }.Schedule(_taa, state.Dependency);
        }


        [BurstCompile]
        struct ApplyCameraTransformJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public NativeArray<int> EntityCounts;
            [ReadOnly] public NativeArray<int> EntityStarts;
            [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookup;

            public void Execute(int index, TransformAccess ta)
            {
                float3 averagePosition = float3.zero;
                for (var i = EntityStarts[index]; i < EntityStarts[index] + EntityCounts[index]; i++)
                {
                    Entity e = Entities[i];
                    LocalToWorld lt = TransformLookup[e];
                    averagePosition += lt.Position;
                    ta.rotation = lt.Rotation;
                }

                ta.position = averagePosition / EntityCounts[index];
            }
        }
    }
}