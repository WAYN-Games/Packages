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

using WAYNGames.Locomotion.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace WAYNGames.Locomotion.Runtime.StateMachine
{
    /// <summary>
    /// Base context available to all locomotion state machine.
    /// Contains the minimal core state data (Velocity, Position, ...) and information (Entity, Collider, PhysicsWorld,...) required for locomotion state logic and platform handling logic. 
    /// </summary>
    public struct BaseLocomotionContext
    {
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<TrackedMotion> PlatformMotionFrameLookup;
        [ReadOnly] public BlobAssetReference<Collider> Collider;

        public RefRW<LocomotionContact> LocomotionContact;
        public RefRW<LocalTransform> CharacterPosition;
        public float3 InitialPosition;

        public float GravityStrength;
        public float3 GravityDirection;

        public Entity CharacterEntity;
        public float TimeStep;

        public float3 HorizontalDisplacement;
        public float3 VerticalDisplacement;
        public float3 HorizontalVelocity;
        public float3 VerticalVelocity;
        public float3 HorizontalInheritedVelocity;
        public float3 HorizontalInheritedDisplacement;
        public float3 VerticalInheritedVelocity;
        public float3 VerticalInheritedDisplacement;
        public quaternion InheritedRotation;


        public void UpdateVelocitiesAndComputeDisplacements(float3 newHorizontalAcceleration = default,
            float3 newVerticalAcceleration = default)
        {
            HorizontalDisplacement += HorizontalVelocity * TimeStep +
                                      0.5f * newHorizontalAcceleration * TimeStep * TimeStep;
            HorizontalVelocity += newHorizontalAcceleration * TimeStep;
            VerticalDisplacement += VerticalVelocity * TimeStep +
                                    0.5f * newVerticalAcceleration * TimeStep * TimeStep;
            VerticalVelocity += newVerticalAcceleration * TimeStep;
        }

        public bool HasRemainingDisplacement => math.lengthsq(HorizontalDisplacement + VerticalDisplacement) >=
                                                math.EPSILON;

        public bool IsInContact => !LocomotionContact.ValueRO.Contact.Normal.Equals(float3.zero);

        public bool IsCeiling => math.dot(LocomotionContact.ValueRO.Contact.Normal, -GravityDirection) < -math.EPSILON;


        public bool IsOnMovingPlatform =>
            math.lengthsq(HorizontalInheritedVelocity + VerticalInheritedVelocity) > math.EPSILON;

        public bool IsMoving => math.lengthsq(HorizontalVelocity + VerticalVelocity) > math.EPSILON;

        public bool IsMovingAgainstSlope =>
            math.dot(LocomotionContact.ValueRO.Contact.Normal, HorizontalDisplacement) <= math.EPSILON;


        public float3 StepHeight(float shellWidth)
        {
            return math.project(LocomotionContact.ValueRO.Contact.Position - CharacterPosition.ValueRO.Position,
                -GravityDirection) + shellWidth * -GravityDirection;
        }
    }
}