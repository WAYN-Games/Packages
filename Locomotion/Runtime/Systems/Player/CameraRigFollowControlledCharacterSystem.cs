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
using WAYNGames.Locomotion.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine.Jobs;

namespace WAYNGames.Locomotion.Runtime.Systems
{
    // We update before the PresentationGoTransformSyncSystem so its update only once per frame after character movements.
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct CameraRigFollowControlledCharacterSystem : ISystem
    {
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_Allocation =
            new("Allocation");
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_TransformAccessArray =
            new("TransformAccessArray");
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_get =
            new("Get");
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_Add =
            new("Add");
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_Switch =
            new("Switch");
        static readonly ProfilerMarker k_LocomotionCameraTargetTrackerSystem_Schedule =
            new("Schedule");

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
            k_LocomotionCameraTargetTrackerSystem_Allocation.Begin();
            // Clear last frame’s data
            if (_taa.isCreated)
                _taa.Dispose();
            _taa = new TransformAccessArray(0);
            var entities = new NativeList<Entity>(state.WorldUpdateAllocator);
            var entityCounts = new NativeList<int>(state.WorldUpdateAllocator);
            var entityStarts = new NativeList<int>(state.WorldUpdateAllocator);
            k_LocomotionCameraTargetTrackerSystem_Allocation.End();

            k_LocomotionCameraTargetTrackerSystem_TransformAccessArray.Begin();
            // Gather all (CameraTarget, Entity) pairs on the main thread
            foreach (var (controlledCharacters, playerCameraRig) in SystemAPI
                         .Query<DynamicBuffer<ControlledCharacters>, RefRO<PlayerGameObjectInstance>>())
            {
                if(controlledCharacters.Length == 0) continue;
                k_LocomotionCameraTargetTrackerSystem_get.Begin();
                var cameraRig = playerCameraRig.ValueRO.Instance.Value.GetComponent<PlayerGoAuthoring>();
                k_LocomotionCameraTargetTrackerSystem_get.End();
                if (cameraRig == null) continue;
                if (cameraRig.FollowStrategy == FollowStrategy.None) continue;
                if (cameraRig.CameraTarget == null) continue;
                if (controlledCharacters.IsEmpty) continue;
                k_LocomotionCameraTargetTrackerSystem_Add.Begin();
                _taa.Add(cameraRig.CameraTarget);
                k_LocomotionCameraTargetTrackerSystem_Add.End();


                AddControlledCharacterEntities(cameraRig.FollowStrategy, ref entityCounts, ref entityStarts,
                    ref entities,
                    in controlledCharacters);


            }

            k_LocomotionCameraTargetTrackerSystem_TransformAccessArray.End();

            // Schedule the TransformJob in parallel using AsDeferredJobArray to avoid hard sync-point
          
            ScheduleCameraTransformJob(ref state, entities, entityCounts, entityStarts);

        }

        [BurstCompile]
        void ScheduleCameraTransformJob(ref SystemState state, NativeList<Entity> entities, NativeList<int> entityCounts,
            NativeList<int> entityStarts)
        {  k_LocomotionCameraTargetTrackerSystem_Schedule.Begin();
            _transformLookup.Update(ref state);
            state.Dependency = new ApplyCameraTransformJob
            {
                Entities = entities.AsDeferredJobArray(),
                EntityCounts = entityCounts.AsDeferredJobArray(),
                EntityStarts = entityStarts.AsDeferredJobArray(),
                TransformLookup = _transformLookup
            }.Schedule(_taa, state.Dependency);
            k_LocomotionCameraTargetTrackerSystem_Schedule.End();
        }

        [BurstCompile]
        static void AddControlledCharacterEntities(FollowStrategy followStrategy, ref NativeList<int> entityCounts,
            ref NativeList<int> entityStarts, ref NativeList<Entity> entities, in DynamicBuffer<ControlledCharacters> controlledCharacters)
        {
            using (k_LocomotionCameraTargetTrackerSystem_Switch.Auto())
            {
                switch (followStrategy)
                {
                    case FollowStrategy.First:
                        entityCounts.Add(1);
                        entityStarts.Add(entities.Length);
                        entities.Add(controlledCharacters[0].Model.Entity);
                        break;
                    case FollowStrategy.Average:
                    {
                        entityCounts.Add(controlledCharacters.Length);
                        entityStarts.Add(entities.Length);
                        foreach (ControlledCharacters controlledCharacter in controlledCharacters)
                            entities.Add(controlledCharacter.Model.Entity);
                        break;
                    }
                }
            }
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