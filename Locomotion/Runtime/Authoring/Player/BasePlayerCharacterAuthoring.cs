using System.Collections.Generic;
using Locomotion.Runtime.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Wayn.Locomotion.StateMachine;

namespace Locomotion.Runtime.Authoring.Player
{
    /// <summary>
    /// Abstract base class for authoring player character components.
    /// Provides support for player input assets and control over multiple characters.
    /// </summary>
    /// <typeparam name="TInput">
    /// Represents the input data type set by the player to control characters.
    /// </typeparam>
    public abstract class BasePlayerCharacterAuthoring<TInput> : MonoBehaviour
        where TInput : unmanaged, IComponentData, ICharacterLocomotionInput<TInput>
    {
        [Tooltip("The scriptable InputAsset that defines how the player can move the characters in the world.")]
        [SerializeField]
        protected PlayerInputAsset<TInput> InputAsset;


        [Tooltip("The the player game object prefab containing camera, input system, ...")] [SerializeField]
        protected PlayerGoAuthoring PlayerGoPrefab;

        [Tooltip("The list of characters controlled by this player.")] [SerializeField]
        protected List<GameObject> ControlledCharacters = new();



        /// <summary>
        ///     Baker class to convert PlayerAuthoring into ECS entity.
        /// </summary>
        public abstract class BasePlayerCharacterBaker<TAuthoring> : Baker<TAuthoring>
            where TAuthoring : BasePlayerCharacterAuthoring<TInput>
        {
            protected TAuthoring Authoring;
            protected Entity PlayerEntity;

            /// <summary>
            /// Allows for additional logic or processing during the baking process of a player.
            /// Derived classes can override this method to implement custom behavior for extending the baking process.
            /// This method is invoked after the <c>BasePlayerCharacterAuthoring.Bake(TAuthoring authoring)</c> method.
            /// The authoring component and baking entity can be accessed through the protected fields of the baker.
            /// </summary>
            public virtual void ContinueBaking()
            {
            }

            public sealed override void Bake(TAuthoring authoring)
            {
                Authoring = authoring;
                Entity playerEntity = GetEntity(TransformUsageFlags.ManualOverride);
                PlayerEntity = playerEntity;
                AddBuffer<ControlledCharacters>(playerEntity);
                var buffer = AddBuffer<ControlledCharactersBakingList>(playerEntity);
                foreach (GameObject character in authoring.ControlledCharacters)
                    buffer.Add(new ControlledCharactersBakingList
                    {
                        Character = GetEntity(character, TransformUsageFlags.None)
                    });

                AddComponent(playerEntity, new LocomotionInput<TInput>
                {
                    Asset = authoring.InputAsset
                });


                AddComponent(playerEntity, new PlayerGameObjectPrefab()
                {
                    Prefab = authoring.PlayerGoPrefab.gameObject
                });
            }
        }
    }

    /// <summary>
    /// Temporary buffer element used during the baking process to store a list of entities representing controlled characters.
    /// This structure is utilized to transfer character entity references during authoring.
    /// </summary>
    /// <remarks>
    /// <c>ControlledCharactersBakingList</c> references the root character entity while the <c>ControlledCharacters</c> used at runtime requires the <c>Locomotion</c>  and <c>Model</c> which aren't available from the Baker. Therefore, we need to rely on a temporary baking component to reference the root character entity and retrieve its sub entities in the <see cref="BasePlayerCharacterBackingSystem"/>.
    /// </remarks>
    [TemporaryBakingType]
    public struct ControlledCharactersBakingList : IBufferElementData
    {
        public Entity Character;
    }
}