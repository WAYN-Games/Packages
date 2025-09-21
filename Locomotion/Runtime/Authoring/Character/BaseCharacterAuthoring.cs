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
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using Wayn.Locomotion.StateMachine;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using Hash128 = UnityEngine.Hash128;


namespace Locomotion.Runtime.Authoring
{
    /// <summary>
    /// Serves as a base class for authoring character entities with locomotion capabilities. Designed to work with a
    /// profile of type <typeparamref name="TProfile" />, enabling customization of character movement and behavior
    /// through specific profile definitions.
    /// Contains shared properties and functionality for setting up entities related to character locomotion, including
    /// rendering models and defining state machines through further specialization.
    /// The class includes a nested abstract baker class used for converting authoring data into ECS entities with the
    /// following configurations:
    /// 1. Associates the authored character with a profile asset (defining locomotion behavior and rules).
    /// 2. Supports optional assignment of a prefab model entity for visual representation.
    /// 3. Facilitates the integration of locomotion state machines, states, and custom contexts specific to the
    /// movement profile.
    /// </summary>
    /// <typeparam name="TProfile">
    /// The locomotion profile type derived from <see cref="ILocomotionProfile" />. This profile encapsulates
    /// movement rules and parameters for the character.
    /// </typeparam>
    [DisallowMultipleComponent]
    public abstract class BaseCharacterAuthoring<TProfile> : MonoBehaviour
        where TProfile : unmanaged, ILocomotionProfile
    {
        /// <summary>
        /// The scriptable object that defines the locomotion profile for the character, specifying
        /// movement capabilities and behaviors in the game world.
        /// </summary>
        [Tooltip("The scriptable ProfileAsset that defines how the character can move in the world.")]
        [field: SerializeField]
        protected LocomotionProfileAsset<TProfile> ProfileAsset;


        /// <summary>
        /// The prefab GameObject associated with the character entity, used for rendering and animation.
        /// </summary>
        [Tooltip("The prefab Game object that will be spawned with the character entity for rendering and animation.")]
        [field: SerializeField]
        internal GameObject ModelPrefab { get; private set; }



        /// <summary>
        ///     Baker for the CharacterAuthoring. Converts the authoring data into one or more ECS entities with the following
        ///     setup:
        ///     1. Creates the primary character entity (manual‐override transform) and, if Playable is checked, adds a
        ///     PlayableCharacter tag.
        ///     2. If a ModelPrefab is assigned, spawns a separate “model” entity with:
        ///     – PresentationGo (holds the prefab for rendering)
        ///     – ControllingEntity (points back to the character entity)
        ///     3. Applies the authoring transform to the character via LocalTransform.
        ///     4. If a CharacterProfile is provided:
        ///     – Generates or reuses a BlobAssetReference<CharacterProfile /> from the profile asset (and calls DependsOn so
        ///     changes re-bake automatically)
        ///     – Adds LocomotionInput to drive character movement from player input or AI Agent
        ///     – Builds a CapsuleCollider from profile settings (CapsuleGeometry + CollisionFilter) and adds it as a
        ///     PhysicsCollider blob asset
        ///     – Registers the character with the physics world (PhysicsWorldIndex)
        ///     – Adds LocomotionProfileReference (points to the blob asset), LocomotionData (initial velocity, jump count, etc.),
        ///     LocomotionGravity (gravity multiplier)
        ///     – Initializes the CharacterLocomotionStateMachine in the Falling state
        /// </summary>
        public abstract class
            BaseCharacterBaker<TAuthoring, TStateMachine, TStates, TContext, TInput, TStateData> : Baker<TAuthoring>
            where TStateMachine : unmanaged,
            ILocomotionStateMachine<TStateMachine, TStates, TContext, TProfile, TInput, TStateData>, IComponentData
            where TStates : unmanaged
            where TContext : unmanaged, ILocomotionContext<TProfile, TInput, TStateData>
            where TAuthoring : BaseCharacterAuthoring<TProfile>
            where TInput : unmanaged, IComponentData
            where TStateData : unmanaged, IComponentData
        {
            protected TAuthoring Authoring;
            protected Entity CharacterEntity;
            protected Entity LocomotorEntity;
            protected Entity ModelEntity;
            protected TProfile Profile;

            /// <summary>
            /// Allows for additional logic or processing during the baking process of a character.
            /// Derived classes can override this method to implement custom behavior for extending the baking process.
            /// This method is invoked after the <c>BaseCharacterBaker.Bake(TAuthoring authoring)</c> method.
            /// The authoring component and different baking entities can be accessed through the protected fields of the baker.
            /// </summary>
            protected virtual void ContinueBaking()
            {
            }

            public sealed override void Bake(TAuthoring authoring)
            {
                Authoring = authoring;
                CharacterEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);


                var characterComponent = new Character();

                if (authoring.ModelPrefab != null)
                {
                    ModelEntity = CreateAdditionalEntity(TransformUsageFlags.WorldSpace,
                        entityName: $"{authoring.ModelPrefab.name} (Model)");
                    characterComponent.Model = ModelEntity;
                    AddComponentObject(ModelEntity, new GameObjectEntity { Prefab = authoring.ModelPrefab });
                }

                LocalTransform locLocalTransform =
                    LocalTransform.FromPositionRotationScale(authoring.transform.position, authoring.transform.rotation,
                        1);
                var characterPath = AddBuffer<CharacterPath>(CharacterEntity);
                characterPath.Add(new CharacterPath
                {
                    Transform = new RigidTransform(locLocalTransform.ToMatrix())
                });

                var profileAsset = authoring.ProfileAsset;
                if (profileAsset == null) return;
                DependsOn(profileAsset);

                var bb = new BlobBuilder(Allocator.Temp);
                ref TProfile profile = ref profileAsset.BuildBlobAsset(ref bb);
                Profile = profile;

                var blobReference = bb.CreateBlobAssetReference<TProfile>(Allocator.Persistent);
                AddBlobAsset(ref blobReference, out _);

                LocomotorEntity = CreateAdditionalEntity(TransformUsageFlags.ManualOverride,
                    entityName: $"{authoring.ModelPrefab.name} (Locomotion)");
                characterComponent.Locomotion = LocomotorEntity;
                AddComponent(LocomotorEntity, locLocalTransform);
                ;
                AddComponent<TStateMachine>(LocomotorEntity);
                AddComponent<TInput>(LocomotorEntity);
                AddComponent<TStateData>(LocomotorEntity);

                AddComponent(LocomotorEntity, new LocomotionProfileBlob<TProfile>
                {
                    Profile = blobReference
                });


                CapsuleGeometry playerCapsule = profile.PhysicsProfileValue.CapsuleGeometry();
                CollisionFilter filter = profile.PhysicsProfileValue.Filter();

                var shouldBeUniqueCollider = authoring.TryGetComponent<ForceUniqueColliderAuthoring>(out _);

                Hash128 geometryHash = Hash128.Compute(ref playerCapsule);
                Hash128 filterHash = Hash128.Compute(ref filter);
                var combinedHash = new Hash128();
                HashUtilities.AppendHash(ref geometryHash, ref combinedHash);
                HashUtilities.AppendHash(ref filterHash, ref combinedHash);

                if (!TryGetBlobAssetReference(combinedHash,
                        out BlobAssetReference<Collider> blobColliderReference) || shouldBeUniqueCollider)
                {
                    blobColliderReference = CapsuleCollider.Create(playerCapsule, filter);
                    // By default, collider created from script are unique
                    // This behavior is not the one we want by default
                    // So unless specifically requested otherwise we force it back to be shared
                    if (!shouldBeUniqueCollider)
                        UnityPhysicsInternals.ForceNonUniqueCollider(ref blobColliderReference.Value);
                    AddBlobAsset(ref blobColliderReference, out _);
                }

                AddComponent(LocomotorEntity, new PhysicsCollider
                {
                    Value = blobColliderReference
                });

                AddSharedComponent(LocomotorEntity, new PhysicsWorldIndex());

                AddComponent(LocomotorEntity, new LocomotionVelocity
                {
                    Velocity = float3.zero
                });

                AddComponent<LocomotionContact>(LocomotorEntity);

                AddComponent(LocomotorEntity, new LocomotionGravity
                {
                    Gravity = PhysicsStep.Default.Gravity
                });

                AddComponent(CharacterEntity, characterComponent);
                ContinueBaking();
            }
        }
    }
}