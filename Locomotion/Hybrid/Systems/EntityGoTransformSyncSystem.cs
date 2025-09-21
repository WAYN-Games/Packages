using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

/// <summary>
///     This system keep the game object position in sync with its entity
///     We can't use burst in this system because we are iterating over managed components
/// </summary>
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct EntityGoTransformSyncSystem : ISystem
{
    static TransformAccessArray _transforms;
    static NativeList<Entity> _entities;
    ComponentLookup<LocalToWorld> _localToWorldsLookup;

    public static EntityGoTransformSyncSystem Instance;

    public void RegisterTransform(Transform transform, Entity entity)
    {
        EnsureTransformCollections();
        _transforms.Add(transform);
        _entities.Add(entity);
    }

    void EnsureTransformCollections()
    {
        if (!_transforms.isCreated)
            _transforms = new TransformAccessArray(1, JobsUtility.JobWorkerCount);
        if (!_entities.IsCreated)
            _entities = new NativeList<Entity>(1, Allocator.Persistent);
    }

    public void UnRegisterTransform(Entity entity)
    {
        var index = _entities.IndexOf(entity);
        if (index == -1) return;
        _transforms.RemoveAtSwapBack(index);
        _entities.RemoveAtSwapBack(index);
    }

    public void OnCreate(ref SystemState state)
    {
        Instance = this;
        EnsureTransformCollections();
        _localToWorldsLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
        _transforms.Dispose();
        _entities.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        _localToWorldsLookup.Update(ref state);


        state.Dependency = new GoTransformSyncJob
        {
            LocalToWorlds = _localToWorldsLookup,
            Entities = _entities.AsDeferredJobArray()
        }.Schedule(_transforms, state.Dependency);
    }

    [BurstCompile]
    struct GoTransformSyncJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<Entity> Entities;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorlds;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = LocalToWorlds[Entities[index]].Position;
            transform.rotation = LocalToWorlds[Entities[index]].Rotation;
        }
    }
}