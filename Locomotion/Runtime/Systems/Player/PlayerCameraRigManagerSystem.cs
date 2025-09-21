using Locomotion.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Locomotion.Systems
{
    /// <summary>
    /// Manages the lifecycle of the camera rig game object.
    /// It ensures it's instantiation from the prefab for every player.
    /// When a player entity gets destroyed, the cleanup component ensures the destruction of the camera rig game object.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlayerCameraRigManagerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (Entity entity in SystemAPI.QueryBuilder()
                         .WithAll<PlayerGameObjectPrefab>()
                         .WithNone<PlayerGameObject>()
                         .Build().ToEntityArray(Allocator.Temp))
            {
                var prefab = SystemAPI.GetComponent<PlayerGameObjectPrefab>(entity).Prefab;
                state.EntityManager.AddComponentData(entity, new PlayerGameObject
                {
                    Instance = Object.Instantiate(prefab.Value)
                });
            }

            foreach (Entity entity in SystemAPI.QueryBuilder()
                         .WithAll<PlayerGameObject>()
                         .WithNone<PlayerGameObjectPrefab>()
                         .Build().ToEntityArray(Allocator.Temp))
            {
                var instance = SystemAPI.GetComponent<PlayerGameObject>(entity).Instance;
                Object.Destroy(instance);
                state.EntityManager.RemoveComponent<PlayerGameObject>(entity);
            }
        }
    }
}