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

using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace WAYNGames.Locomotion.Runtime.Components
{

    public struct Locomotor : IComponentData
    {
        public Entity Entity;
    }
    
    public struct Model : IComponentData
    {
        public Entity Entity;
    }

    /// <summary>
    /// A list of transform teh character went through.
    /// The first element is the next target position and the second and third element are previous transforms.
    /// Time allows for interpolation between the previous position and next target position.
    /// The third element can be used to compute velocity and acceleration.
    /// </summary>
    [WriteGroup(typeof(LocalToWorld))]
    [InternalBufferCapacity(3)]
    public struct CharacterPath : IBufferElementData
    {
        public RigidTransform Transform;
        public float Time;
    }

    /// <summary>
    ///     Locomotion velocity for a character locomotor.
    /// </summary>
    public struct LocomotionVelocity : IComponentData
    {
        /// <summary>Current velocity vector of the character.</summary>
        public float3 Velocity;

        /// <summary>Velocity inherited from external forces (e.g. moving platforms).</summary>
        public float3 InheritedVelocity;
    }

    /// <summary>
    ///     Last ground or obstacle contact information.
    /// </summary>
    public struct LocomotionContact : IComponentData
    {
        public Contact Contact;
    }


    /// <summary>
    ///     Encapsulates gravity settings for the character.
    ///     This value can be updated by a system to simulate any arbitrary gravitiy direction. 
    /// </summary>
    public struct LocomotionGravity : IComponentData
    {
        /// <summary>Gravity vector applied to the character.</summary>
        public float3 Gravity;

        /// <summary>Normalized direction of gravity (unit vector).</summary>
        public float3 GravityDirection => math.normalizesafe(Gravity);

        /// <summary>Magnitude (strength) of the gravity vector.</summary>
        public float GravityStrength => math.length(Gravity);
    }

    /// <summary>
    ///     Information about a collision contact point.
    /// </summary>
    public struct Contact : IComponentData
    {
        /// <summary>Surface normal at the contact point.</summary>
        public float3 Normal;

        /// <summary>World‐space position of the contact point.</summary>
        public float3 Position;

        /// <summary>Entity that was collided with.</summary>
        public Entity Entity;
    }

    /// <summary>
    /// Represents a physics configuration profile for a character, encapsulating core properties that define
    /// collision behavior, dimensions, and interaction with physics layers.
    /// </summary>
    [Serializable]
    public struct CharacterPhysicsProfile
    {
        /// <summary>Thickness of the character’s collision shell.</summary>
        public float ShellWidth;

        /// <summary>Maximum number of times the character can bounce when resolving collisions.</summary>
        public int MaxBounce;

        /// <summary>Total height of the character's collision capsule (head to toes).</summary>
        [Tooltip("Total height of the collision capsule for the character.")]
        public float CapsuleHeight;

        /// <summary>Radius of the players' collision capsule.</summary>
        public float CapsuleRadius;


        /// <summary>Spin or roll factor when sliding along walls.</summary>
        [Range(-1, 1)]
        [Tooltip("Controls the behavior of the player when idle on a rotating platform while pushed on a wall.\n" +
                 "-1, the player keeps spinning with the platform while sliding along the wall\n" +
                 "0, the player slide along the wall without rotating\n" +
                 "1, the player rolls on the wall")]
        public float SpinOrRollAlongWall;


        /// <summary>
        ///     Collision layer the character capsule belongs to.
        /// </summary>
        public LayerMask BelongsTo;

        /// <summary>
        ///     Collision layer the character capsule can collide with.
        /// </summary>
        public LayerMask CollidesWith;


        public CapsuleGeometry CapsuleGeometry()
        {
            return new CapsuleGeometry
            {
                Vertex0 = math.up() * CapsuleRadius,
                Vertex1 = math.up() * (CapsuleHeight -
                                       CapsuleRadius),
                Radius = CapsuleRadius
            };
        }

        public CollisionFilter Filter()
        {
            var filter = CollisionFilter.Default;
            filter.BelongsTo = (uint)BelongsTo.value;
            filter.CollidesWith = (uint)CollidesWith.value;
            return filter;
        }


        public static CharacterPhysicsProfile Default => new()
        {
            MaxBounce = 6,
            ShellWidth = 0.01f,
            SpinOrRollAlongWall = 0,
            CapsuleRadius = 0.5f,
            CapsuleHeight = 2f,
            BelongsTo = 1,
            CollidesWith = ~0
        };
    }
}