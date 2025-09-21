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
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Wayn.Locomotion.StateMachine
{
    [BurstCompile]
    public static class PhysicsOperations
    {
        /// <summary>
        /// Attempts to perform a step movement for a character based on the provided context, physics profile, and parameters.
        /// </summary>
        /// <param name="ctx">The locomotion context containing character state and relevant environmental data.</param>
        /// <param name="profile">The physics profile containing parameters defining the character's physical properties and constraints.</param>
        /// <param name="stepOffset">The offset vector representing the direction and magnitude of the step.</param>
        /// <param name="maxStepHeight">The maximum height the character is allowed to step up.</param>
        /// <returns>Returns a boolean indicating whether the step movement was successfully performed.</returns>
        [BurstCompile]
        public static bool TryStep(ref BaseLocomotionContext ctx, in CharacterPhysicsProfile profile,
            in float3 stepOffset, in float maxStepHeight)
        {
            ref LocalTransform transformRW = ref ctx.CharacterPosition.ValueRW;
            float3 gravityDirection = ctx.GravityDirection;
            float3 displacement = ctx.HorizontalDisplacement + ctx.VerticalDisplacement;

            var collector =
                new ClosestColliderCastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);
            // Compute the position of the step nose
            // Create a capsule at the position of the player
            // The capsule is placed at the height of the maximum step for the player
            // The capsule is shrunk down based on the size of the potential step
            // to prevent excessive collision with a ceiling
            var magnitude = math.length(displacement);
            float3 castDirection = math.normalizesafe(displacement);
            MathematicsHelpers.ProjectOnPlane(-ctx.LocomotionContact.ValueRO.Contact.Normal, -gravityDirection,
                out float3 stepIncidenceVector);
            var castDistanceMultiplier = 1 / math.dot(stepIncidenceVector, castDirection);
            castDistanceMultiplier = math.select(1, castDistanceMultiplier, math.isfinite(castDistanceMultiplier));

            var castDistance = math.min(float.MaxValue * 0.5f,
                magnitude + profile.CapsuleRadius * castDistanceMultiplier);

            var capsuleGeometry = new CapsuleGeometry
            {
                Vertex0 = ctx.LocomotionContact.ValueRO.Contact.Position + -gravityDirection *
                    (stepOffset + profile.CapsuleRadius + profile.ShellWidth),
                Vertex1 = ctx.LocomotionContact.ValueRO.Contact.Position + -gravityDirection * (profile.CapsuleHeight -
                    profile.CapsuleRadius +
                    maxStepHeight -
                    stepOffset +
                    2 * profile.ShellWidth),
                Radius = profile.CapsuleRadius
            };

            float3 stepCastDistance = castDirection * castDistance;
            // We need to use a custom cast to be able to use a shrunk down version of the player capsule
            var collided = ctx.PhysicsWorld.CapsuleCastCustom(capsuleGeometry.Vertex0, capsuleGeometry.Vertex1,
                capsuleGeometry.Radius,
                castDirection, castDistance, ref collector,
                ctx.Collider.Value.GetCollisionFilter());
            if (collided)
            {
                // we can update the position of the entity up to the collision point (minus the safety margin)
                float3 stepTravelDistance = stepCastDistance * collector.ClosestHit.Fraction;

                // Only move if there is sufficient space to stand on the step
                if (math.lengthsq(stepTravelDistance) > math.square(profile.CapsuleRadius))
                {
                    // Adjust fraction to the length of the displacement
                    var fractionOfDistance =
                        math.min(
                            collector.ClosestHit.Fraction * castDistance /
                            (profile.ShellWidth + castDistance), 1f);
                    float3 travelledDistance = fractionOfDistance * displacement -
                                               castDirection * profile.ShellWidth;

                    // prevent backward movement
                    if (math.dot(travelledDistance, castDirection) < 0) return false;

                    // move up to a safe distance from the collision point 
                    transformRW.Position += stepOffset;
                    transformRW.Position += travelledDistance;
                    ctx.HorizontalDisplacement *= 1 - fractionOfDistance;
                    ctx.VerticalDisplacement *= 1 - fractionOfDistance;


                    ctx.LocomotionContact.ValueRW.Contact.Position = collector.ClosestHit.Position;
                    ctx.LocomotionContact.ValueRW.Contact.Normal = collector.ClosestHit.SurfaceNormal;
                    ctx.LocomotionContact.ValueRW.Contact.Entity = collector.ClosestHit.Entity;


                    return true;
                }

                return false;
            }

            transformRW.Position += stepOffset;
            transformRW.Position += displacement;
            ctx.HorizontalDisplacement = float3.zero;
            ctx.VerticalDisplacement = float3.zero;
            return true;
        }


        /// <summary>
        /// Resolves character overlapping with other geometry by repeatedly checking overlap and moving the character based on penetration distance. This operation is performed until the character is no longer overlapping with other geometry or until CharacterPhysicsProfile.MaxBounce has been reached.
        /// </summary>
        /// <param name="context">The locomotion context containing character state and environmental information.</param>
        /// <param name="profile">The physics profile defining the character's physical constraints and properties.</param>
        [BurstCompile]
        public static void SolveOverlaps(ref BaseLocomotionContext context, in CharacterPhysicsProfile profile)
        {
            var maxIteration = profile.MaxBounce;
            bool hasOverlap;
            do
            {
                maxIteration--;

                var distInput = new ColliderDistanceInput(context.Collider, 0f,
                    new RigidTransform(context.CharacterPosition.ValueRW.ToMatrix()),
                    1 + profile.ShellWidth / profile.CapsuleRadius * 0.25f);
                // Scaling by 0.25 allows stacked capsule to not detect each other and de collide, thus allowing player stacking.

                var closestHitCollector = new ClosestDistanceHitCollectorIgnoreSelf(context.CharacterEntity,
                    context.PhysicsWorld.Bodies, distInput.MaxDistance);


                hasOverlap = context.PhysicsWorld.CalculateDistance(distInput, ref closestHitCollector);
                if (!hasOverlap) continue;

                DistanceHit hit = closestHitCollector.ClosestHit;
                float3 deCollisionVector = hit.SurfaceNormal * math.abs(hit.Distance);
                float3 planeNormal = context.LocomotionContact.ValueRO.Contact.Normal;
                if (context.LocomotionContact.ValueRO.Contact.Entity == hit.Entity)
                    planeNormal = context.GravityDirection;

                float3 projectedDeCollision =
                    deCollisionVector - math.projectsafe(deCollisionVector, planeNormal, planeNormal);


                context.CharacterPosition.ValueRW.Position += projectedDeCollision;
            } while (hasOverlap && maxIteration > 0);
        }

        /// <summary>
        /// Computes the displacement and velocity inherited by a character due to the motion of a platform it is in contact with.
        /// </summary>
        /// <param name="context">The locomotion context containing the character's state, platform motion data, and contact information.</param>
        [BurstCompile]
        public static void ComputeInheritedDisplacement(ref BaseLocomotionContext context)
        {
            if (!context.PlatformMotionFrameLookup.HasComponent(context.LocomotionContact.ValueRO.Contact.Entity))
                return;

            TrackedMotion trackedMotion =
                context.PlatformMotionFrameLookup[context.LocomotionContact.ValueRO.Contact.Entity];

            // Compute character's position relative to platform in previous frame
            float3 localOffset =
                math.transform(math.inverse(trackedMotion.PreviousTransform), context.InitialPosition);

            // Compute character position after platform move 
            float3 newCharacterPosition = math.transform(trackedMotion.CurrentTransform, localOffset);


            // Apply rotation delta to character orientation
            quaternion deltaRotation = math.mul(trackedMotion.CurrentTransform.rot,
                math.inverse(trackedMotion.PreviousTransform.rot));
            context.InheritedRotation = deltaRotation;

            // Apply displacement
            float3 totalDisplacement = newCharacterPosition - context.InitialPosition;
            float3 vertical = math.projectsafe(totalDisplacement, context.GravityDirection);
            float3 horizontal = totalDisplacement - vertical;


            context.VerticalInheritedDisplacement = vertical;
            context.HorizontalInheritedDisplacement = horizontal;
            context.VerticalInheritedVelocity = vertical / trackedMotion.DeltaTime;
            context.HorizontalInheritedVelocity = horizontal / trackedMotion.DeltaTime;
        }


        /// <summary>
        /// Checks for a downward step within the specified distance and updates the locomotion context with contact information if a step is detected. This avoids sliding deviation when taking a step down.
        /// </summary>
        /// <param name="ctx">The locomotion context containing character state and relevant environmental data.</param>
        /// <param name="distanceToCheck">The maximum distance to check for a step down from the character's current position.</param>
        [BurstCompile]
        public static void CheckForStepDown(ref BaseLocomotionContext ctx, float distanceToCheck)
        {
            ref LocalTransform transformRW = ref ctx.CharacterPosition.ValueRW;
            /*
            var distanceToCheck = ctx.Profile.Value.Movement.MaxStepHeight +
                                  ctx.Profile.Value.PhysicsProfile.ShellWidth;
                                  */
            var rci = new RaycastInput
            {
                Start = transformRW.Position + ctx.HorizontalDisplacement,
                End = transformRW.Position + ctx.HorizontalDisplacement + ctx.GravityDirection * distanceToCheck,
                Filter = ctx.Collider.Value.GetCollisionFilter()
            };
            var collectorForGroundRayHit =
                new ClosestRaycastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);

            var hit = ctx.PhysicsWorld.CastRay(rci,
                ref collectorForGroundRayHit);
            if (!hit) return;

            ctx.LocomotionContact.ValueRW.Contact.Position =
                collectorForGroundRayHit.ClosestHit.Position;
            ctx.LocomotionContact.ValueRW.Contact.Normal =
                collectorForGroundRayHit.ClosestHit.SurfaceNormal;
            ctx.LocomotionContact.ValueRW.Contact.Entity =
                collectorForGroundRayHit.ClosestHit.Entity;
        }

        /// <summary>
        /// Adjusts the character's position to align with the ground surface by performing raycast and collider cast checks.
        /// Prevents upward snapping to avoid overlaps with overhead obstacles.
        /// </summary>
        /// <param name="ctx">The locomotion context containing information about the character state, physical world, and environment interactions.</param>
        /// <param name="profile">The character's physics profile containing parameters such as shell width and constraints.</param>
        /// <param name="distanceToCheck">The maximum distance to check for ground alignment below the character.</param>
        [BurstCompile]
        public static void SnapToGround(ref BaseLocomotionContext ctx, in CharacterPhysicsProfile profile,
            float distanceToCheck)
        {
            ref LocalTransform transformRW = ref ctx.CharacterPosition.ValueRW;

            var rci = new RaycastInput
            {
                Start = transformRW.Position,
                End = transformRW.Position + ctx.GravityDirection * distanceToCheck,
                Filter = ctx.Collider.Value.GetCollisionFilter()
            };


            var collectorForGroundRayHit =
                new ClosestRaycastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);

            ctx.LocomotionContact.ValueRW.Contact.Position = float3.zero;
            ctx.LocomotionContact.ValueRW.Contact.Normal = float3.zero;
            ctx.LocomotionContact.ValueRW.Contact.Entity = Entity.Null;

            var hit = ctx.PhysicsWorld.CastRay(rci,
                ref collectorForGroundRayHit);
            if (!hit) return;


            var groundSnapColliderCastInput = new ColliderCastInput(
                ctx.Collider,
                transformRW.Position,
                transformRW.Position + ctx.GravityDirection * distanceToCheck,
                quaternion.identity
            );

            var collectorForGroundHit =
                new ClosestColliderCastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);

            hit = ctx.PhysicsWorld.CastCollider(groundSnapColliderCastInput,
                ref collectorForGroundHit);


            if (!hit) return;

            float3 groundHoveringDistance = ctx.GravityDirection *
                                            profile.ShellWidth;
            float3 traveledDistance =
                ctx.GravityDirection * distanceToCheck *
                collectorForGroundHit.ClosestHit.Fraction -
                groundHoveringDistance;

            ctx.LocomotionContact.ValueRW.Contact.Position = collectorForGroundHit.ClosestHit.Position;
            ctx.LocomotionContact.ValueRW.Contact.Normal = collectorForGroundHit.ClosestHit.SurfaceNormal;
            ctx.LocomotionContact.ValueRW.Contact.Entity = collectorForGroundHit.ClosestHit.Entity;

            // prevent snapping up as it could lead to ceiling overlap 
            if (math.dot(traveledDistance, ctx.GravityDirection) > 0) transformRW.Position += traveledDistance;
        }


        [BurstCompile]
        public static void MoveUpToNextObstacleHorizontally(ref BaseLocomotionContext ctx,
            in CharacterPhysicsProfile profile)
        {
            var collector =
                new ClosestColliderCastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);
            float3 displacement = ctx.HorizontalDisplacement;

            var magnitude = math.length(displacement);
            if (magnitude == 0) return;
            float3 castDirection = displacement / magnitude;
            var safetyDistance = profile.ShellWidth;


            var colliderCastInput = new ColliderCastInput(
                ctx.Collider,
                ctx.CharacterPosition.ValueRW.Position,
                ctx.CharacterPosition.ValueRW.Position + displacement + castDirection * safetyDistance,
                quaternion.identity
            );

            // Move the full distance if there is no hit
            if (!ctx.PhysicsWorld.CastCollider(colliderCastInput, ref collector))
            {
                ctx.CharacterPosition.ValueRW.Position += displacement;
                ctx.HorizontalDisplacement = float3.zero;
                return;
            }

            ColliderCastHit collectorClosestHit = collector.ClosestHit;
            ctx.LocomotionContact.ValueRW.Contact.Normal = collectorClosestHit.SurfaceNormal;
            ctx.LocomotionContact.ValueRW.Contact.Position = collectorClosestHit.Position;
            ctx.LocomotionContact.ValueRW.Contact.Entity = collectorClosestHit.Entity;

            // Adjust fraction to the length of the displacement
            float3 travelledDistance = collectorClosestHit.Fraction * (displacement + castDirection * safetyDistance) -
                                       castDirection * safetyDistance;

            // prevent backward movement
            if (math.dot(travelledDistance, castDirection) < 0) return;


            // move up to a safe distance from the collision point 
            ctx.CharacterPosition.ValueRW.Position += travelledDistance;
            ctx.HorizontalDisplacement *= 1 - collectorClosestHit.Fraction;
        }

        [BurstCompile]
        public static void MoveUpToNextObstacleVertically(ref BaseLocomotionContext ctx,
            in CharacterPhysicsProfile profile)
        {
            var collector =
                new ClosestColliderCastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);
            float3 displacement = ctx.VerticalDisplacement;

            var magnitude = math.length(displacement);
            if (magnitude == 0) return;
            float3 castDirection = displacement / magnitude;
            var safetyDistance = profile.ShellWidth;


            var colliderCastInput = new ColliderCastInput(
                ctx.Collider,
                ctx.CharacterPosition.ValueRW.Position,
                ctx.CharacterPosition.ValueRW.Position + displacement + castDirection * safetyDistance,
                quaternion.identity
            );

            // Move the full distance if there is no hit
            if (!ctx.PhysicsWorld.CastCollider(colliderCastInput, ref collector))
            {
                ctx.CharacterPosition.ValueRW.Position += displacement;
                ctx.VerticalDisplacement = float3.zero;
                return;
            }


            ColliderCastHit collectorClosestHit = collector.ClosestHit;
            ctx.LocomotionContact.ValueRW.Contact.Normal = collectorClosestHit.SurfaceNormal;
            ctx.LocomotionContact.ValueRW.Contact.Position = collectorClosestHit.Position;
            ctx.LocomotionContact.ValueRW.Contact.Entity = collectorClosestHit.Entity;

            // Adjust fraction to the length of the displacement
            float3 travelledDistance = collectorClosestHit.Fraction * (displacement + castDirection * safetyDistance) -
                                       castDirection * safetyDistance;

            // prevent backward movement
            if (math.dot(travelledDistance, castDirection) < 0) return;


            // move up to a safe distance from the collision point 
            ctx.CharacterPosition.ValueRW.Position += travelledDistance;
            ctx.VerticalDisplacement *= 1 - collectorClosestHit.Fraction;
        }

        [BurstCompile]
        public static bool CheckForBackFaceCollision(ref RigidBody body, ref ChildCollider childCollider,
            ref float3 surfaceNormal)
        {
            unsafe
            {
                ColliderType childColliderType = childCollider.Collider->Type;
                var childColliderIsPolygon = childColliderType == ColliderType.Triangle ||
                                             childColliderType == ColliderType.Quad;


                if (!childColliderIsPolygon) return false;

                var collider = (PolygonCollider*)childCollider.Collider;
                float3 frontFaceNormal = collider->Planes[0].Normal;
                float3 backFaceNormal = math.rotate(body.WorldFromBody, collider->Planes[1].Normal);

                return math.dot(surfaceNormal, backFaceNormal) >= 0;
            }
        }

        [BurstCompile]
        public static void MoveUpToNextObstacle(ref BaseLocomotionContext ctx, in CharacterPhysicsProfile profile)
        {
            var collector =
                new ClosestColliderCastHitCollectorIgnoreSelf(ctx.CharacterEntity, ctx.PhysicsWorld.Bodies, 1f);
            float3 displacement = ctx.HorizontalDisplacement + ctx.VerticalDisplacement +
                                  ctx.HorizontalInheritedDisplacement + ctx.VerticalInheritedDisplacement;

            var magnitude = math.length(displacement);
            if (magnitude == 0) return;
            float3 castDirection = displacement / magnitude;
            var safetyDistance = profile.ShellWidth;


            var colliderCastInput = new ColliderCastInput(
                ctx.Collider,
                ctx.CharacterPosition.ValueRW.Position,
                ctx.CharacterPosition.ValueRW.Position + displacement + castDirection * safetyDistance,
                quaternion.identity
            );

            var collided = ctx.PhysicsWorld.CastCollider(colliderCastInput, ref collector);

            ColliderCastHit collectorClosestHit = collector.ClosestHit;
            // Move the full distance if there is no hit
            if (!collided)
            {
                ctx.CharacterPosition.ValueRW.Position += displacement;
                ctx.HorizontalDisplacement = float3.zero;
                ctx.VerticalDisplacement = float3.zero;
                return;
            }


            ctx.LocomotionContact.ValueRW.Contact.Normal = collectorClosestHit.SurfaceNormal;
            ctx.LocomotionContact.ValueRW.Contact.Position = collectorClosestHit.Position;
            ctx.LocomotionContact.ValueRW.Contact.Entity = collectorClosestHit.Entity;

            // Adjust fraction to the length of the displacement
            float3 travelledDistance = collectorClosestHit.Fraction * (displacement + castDirection * safetyDistance) -
                                       castDirection * safetyDistance;

            // prevent backward movement
            if (math.dot(travelledDistance, castDirection) < 0) return;

            // move up to a safe distance from the collision point 
            ctx.CharacterPosition.ValueRW.Position += travelledDistance;
            ctx.HorizontalDisplacement *= 1 - collectorClosestHit.Fraction;
            ctx.VerticalDisplacement *= 1 - collectorClosestHit.Fraction;
            ctx.HorizontalInheritedDisplacement *= 1 - collectorClosestHit.Fraction;
            ctx.VerticalInheritedDisplacement *= 1 - collectorClosestHit.Fraction;
        }
    }
}