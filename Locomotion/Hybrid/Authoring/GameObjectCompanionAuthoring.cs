using Unity.Entities;
using UnityEngine;

/// <summary>
///     Entities using this component will have a GameObject counter part that can be interacted with from the
///     monobehaviour or from a system.
/// </summary>
public class GameObjectCompanionAuthoring : MonoBehaviour
{
    [Tooltip("GameObject prefab that will be used for presentation")]
    public GameObject Prefab;

    internal class GameObjectCompanionBaker : Baker<GameObjectCompanionAuthoring>
    {
        public override void Bake(GameObjectCompanionAuthoring authoring)
        {
            Entity bakingEntity = GetEntity(TransformUsageFlags.WorldSpace);

            if (authoring.Prefab == null) return;
            // We store the game object prefab into a managed component
            var pgo = new GameObjectEntity
            {
                Prefab = authoring.Prefab
            };
            AddComponentObject(bakingEntity, pgo);
        }
    }
}