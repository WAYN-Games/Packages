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

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Wayn.Locomotion.StateMachine
{
    /// <summary>
    ///     Struct for collecting closest raycast hits while ignoring a specified entity. Implements the ICollector interface
    ///     to manage raycast hit results and avoids processing hits that do not meet the criteria, such as hits associated
    ///     with a self entity or backface collisions. Useful in scenarios where certain entities need to be ignored during
    ///     raycasting computations, such as character locomotion systems in physics-based simulations.
    /// </summary>
    public struct ClosestRaycastHitCollectorIgnoreSelf : ICollector<RaycastHit>
    {
        public bool EarlyOutOnFirstHit => false; // Continue collecting to find the closest hit
        public float MaxDistance { get; private set; } // Maximum allowed hit fraction

        public float MaxFraction
        {
            get => MaxDistance;
            private set => MaxDistance = value;
        }

        public int NumHits { get; private set; } // Number of valid hits collected (0 or 1 in this case)

        readonly Entity _ignoredEntity;
        NativeSlice<RigidBody> _rigidBodies;

        public RaycastHit ClosestHit { get; private set; }

        public ClosestRaycastHitCollectorIgnoreSelf(Entity ignoredEntity, NativeSlice<RigidBody> rigidBodies,
            float maxDistance)
        {
            _rigidBodies = rigidBodies;
            _ignoredEntity = ignoredEntity;
            ClosestHit = default;
            MaxDistance = maxDistance; // Start with full fraction
            NumHits = 0;
        }

        public bool AddHit(RaycastHit hit)
        {
            // Ignore the specified entity
            if (hit.Entity == _ignoredEntity) return false;
            if (hit.Fraction >= MaxDistance) return false;
            if (hit.Fraction == 0) return false;
            if (IsBackFaceCollision(hit)) return false;

            NumHits++;
            ClosestHit = hit;
            MaxDistance = hit.Fraction;


            return true;
        }

        bool IsBackFaceCollision(RaycastHit hit)
        {
            RigidBody body = _rigidBodies[hit.RigidBodyIndex];
            float3 surfaceNormal = hit.SurfaceNormal;
            return body.Collider.Value.GetLeaf(hit.ColliderKey, out ChildCollider childCollider) &&
                   PhysicsOperations.CheckForBackFaceCollision(ref body, ref childCollider, ref surfaceNormal);
        }
    }

    /// <summary>
    ///     Struct for collecting the closest distance-based hits while ignoring a specified entity. Implements the ICollector
    ///     interface
    ///     to manage distance hit results and filters hits based on custom criteria such as excluding hits involving a
    ///     specified
    ///     entity or backface collisions. Useful in scenarios where distance queries need to selectively exclude certain
    ///     entities
    ///     or object surfaces, e.g., during character or object navigation systems in physics simulations.
    /// </summary>
    public struct ClosestDistanceHitCollectorIgnoreSelf : ICollector<DistanceHit>
    {
        public bool EarlyOutOnFirstHit => false; // Continue collecting to find the closest hit
        public float MaxDistance { get; private set; } // Maximum allowed hit fraction

        public float MaxFraction
        {
            get => MaxDistance;
            private set => MaxDistance = value;
        }

        public int NumHits { get; private set; } // Number of valid hits collected (0 or 1 in this case)

        readonly Entity _ignoredEntity;
        NativeSlice<RigidBody> _rigidBodies;

        public DistanceHit ClosestHit { get; private set; }

        public ClosestDistanceHitCollectorIgnoreSelf(Entity ignoredEntity, NativeSlice<RigidBody> rigidBodies,
            float maxDistance)
        {
            _rigidBodies = rigidBodies;
            _ignoredEntity = ignoredEntity;
            ClosestHit = default;
            MaxDistance = maxDistance; // Start with full fraction
            NumHits = 0;
        }

        public bool AddHit(DistanceHit hit)
        {
            // Ignore the specified entity
            if (hit.Entity == _ignoredEntity) return false;
            if (hit.Fraction >= MaxDistance) return false;
            if (IsBackFaceCollision(hit)) return false;

            NumHits++;
            ClosestHit = hit;
            MaxDistance = hit.Fraction;


            return true;
        }

        bool IsBackFaceCollision(DistanceHit hit)
        {
            RigidBody body = _rigidBodies[hit.RigidBodyIndex];
            float3 surfaceNormal = hit.SurfaceNormal;
            return body.Collider.Value.GetLeaf(hit.ColliderKey, out ChildCollider childCollider) &&
                   PhysicsOperations.CheckForBackFaceCollision(ref body, ref childCollider, ref surfaceNormal);
        }
    }

    /// <summary>
    ///     Struct for collecting the closest collider cast hits while ignoring a specific entity.
    ///     Implements the ICollector interface to gather and manage collider cast hit results.
    ///     Filters out results that do not meet defined criteria such as hits associated with
    ///     an ignored entity or backface collisions. This is particularly useful in physics-related
    ///     computations where certain entities need to be excluded, such as in character or object
    ///     traversal and detection systems.
    /// </summary>
    public struct ClosestColliderCastHitCollectorIgnoreSelf : ICollector<ColliderCastHit>
    {
        public bool EarlyOutOnFirstHit => false; // Continue collecting to find the closest hit
        public float MaxDistance { get; private set; } // Maximum allowed hit fraction

        public float MaxFraction
        {
            get => MaxDistance;
            private set => MaxDistance = value;
        }

        public int NumHits { get; private set; } // Number of valid hits collected (0 or 1 in this case)

        readonly Entity _ignoredEntity;
        NativeSlice<RigidBody> _rigidBodies;

        public ColliderCastHit ClosestHit { get; private set; }

        public ClosestColliderCastHitCollectorIgnoreSelf(Entity ignoredEntity, NativeSlice<RigidBody> rigidBodies,
            float maxDistance)
        {
            _rigidBodies = rigidBodies;
            _ignoredEntity = ignoredEntity;
            ClosestHit = default;
            MaxDistance = maxDistance; // Start with full fraction
            NumHits = 0;
        }

        public bool AddHit(ColliderCastHit hit)
        {
            // Ignore the specified entity
            if (hit.Entity == _ignoredEntity) return false;
            if (hit.Fraction >= MaxDistance) return false;
            if (hit.Fraction == 0) return false;
            if (IsBackFaceCollision(hit)) return false;

            NumHits++;
            ClosestHit = hit;
            MaxDistance = hit.Fraction;


            return true;
        }

        bool IsBackFaceCollision(ColliderCastHit hit)
        {
            RigidBody body = _rigidBodies[hit.RigidBodyIndex];
            float3 surfaceNormal = hit.SurfaceNormal;
            return body.Collider.Value.GetLeaf(hit.ColliderKey, out ChildCollider childCollider) &&
                   PhysicsOperations.CheckForBackFaceCollision(ref body, ref childCollider, ref surfaceNormal);
        }
    }
}