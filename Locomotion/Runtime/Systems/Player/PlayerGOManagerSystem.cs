using WAYNGames.Locomotion.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace WAYNGames.Locomotion.Runtime.Systems
{
    /// <summary>
    /// Manages the lifecycle of the camera rig game object.
    /// It ensures it's instantiation from the prefab for every player.
    /// When a player entity gets destroyed, the cleanup component ensures the destruction of the camera rig game object.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlayerGoManagerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (Entity entity in SystemAPI.QueryBuilder()
                         .WithAll<PlayerGameObjectPrefab>()
                         .WithNone<PlayerGameObjectInstance>()
                         .Build().ToEntityArray(Allocator.Temp))
            {
                var prefab = SystemAPI.GetComponent<PlayerGameObjectPrefab>(entity).Prefab;
                state.EntityManager.AddComponentData(entity, new PlayerGameObjectInstance
                {
                    Instance = Object.Instantiate(prefab.Value)
                });
            }

            foreach (Entity entity in SystemAPI.QueryBuilder()
                         .WithAll<PlayerGameObjectInstance>()
                         .WithNone<PlayerGameObjectPrefab>()
                         .Build().ToEntityArray(Allocator.Temp))
            {
                var instance = SystemAPI.GetComponent<PlayerGameObjectInstance>(entity).Instance;
                Object.Destroy(instance);
                state.EntityManager.RemoveComponent<PlayerGameObjectInstance>(entity);
            }
        }
    }
}