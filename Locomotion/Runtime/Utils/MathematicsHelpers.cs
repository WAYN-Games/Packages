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

using Unity.Burst;
using Unity.Mathematics;
using Wayn.Locomotion.StateMachine;

[BurstCompile]
public static class MathematicsHelpers
{
    /// <summary>
    /// Projects the horizontal and vertical displacements of a locomotion context
    /// onto a plane defined by the locomotion contact's normal vector, preserving the magnitude of the displacement.
    /// </summary>
    /// <param name="ctx">
    /// The locomotion context containing the horizontal and vertical displacements to be projected.
    /// </param>
    [BurstCompile]
    public static void SimpleSlideProjection(ref BaseLocomotionContext ctx)
    {
        var magnitude = math.length(ctx.HorizontalDisplacement);
        ProjectOnPlane(ctx.HorizontalDisplacement, ctx.LocomotionContact.ValueRO.Contact.Normal,
            out float3 projectedHorizontalLeftoverDistance);
        ctx.HorizontalDisplacement = math.normalizesafe(projectedHorizontalLeftoverDistance) * magnitude;

        var magnitudeV = math.length(ctx.VerticalDisplacement);
        ProjectOnPlane(ctx.VerticalDisplacement, ctx.LocomotionContact.ValueRO.Contact.Normal,
            out float3 projectedVerticalLeftoverDistance);
        ctx.VerticalDisplacement = math.normalizesafe(projectedVerticalLeftoverDistance) * magnitudeV;
    }


    /// <summary>
    /// Projects the horizontal and vertical displacements of a locomotion context
    /// onto a plane defined by the locomotion contact's normal vector, preserving the magnitude of the displacement.
    /// The horizontal projection is constrained to the movement plane to avoid deviation in the trajectory while moving on a slope.
    /// </summary>
    /// <param name="ctx">
    /// The locomotion context containing the displacements and contact information that will be modified.
    /// </param>
    [BurstCompile]
    public static void SimpleGroundProjection(ref BaseLocomotionContext ctx)
    {
        var magnitude = math.length(ctx.HorizontalDisplacement);

        float3 movementPlaneNormal = math.cross(-ctx.GravityDirection, ctx.HorizontalDisplacement);
        ProjectOnPlane(ctx.LocomotionContact.ValueRO.Contact.Normal, movementPlaneNormal,
            out float3 correctedNormal);
        ProjectOnPlane(ctx.HorizontalDisplacement, correctedNormal, out float3 projectedHorizontalLeftoverDistance);
        ctx.HorizontalDisplacement = math.normalizesafe(projectedHorizontalLeftoverDistance) * magnitude;


        var magnitudeV = math.length(ctx.VerticalDisplacement);
        ProjectOnPlane(ctx.VerticalDisplacement, correctedNormal, out float3 projectedVerticalLeftoverDistance);
        ctx.VerticalDisplacement = math.normalizesafe(projectedVerticalLeftoverDistance) * magnitudeV;
    }

    /// <summary>
    /// Projects the horizontal and vertical displacements of a locomotion context
    /// onto a plane defined by the locomotion contact's normal vector, preserving the magnitude of the displacement.
    /// The horizontal projection is constrained to the locomotion contact's normal vector projected to the gravity plane. This is used to simulate sliding along a wall. Prevents displacement adjustments going against the current velocity.
    /// </summary>
    /// <param name="ctx">
    /// The locomotion context containing the displacements and contact information that will be modified.
    /// </param>
    [BurstCompile]
    public static void WallProjection(ref BaseLocomotionContext ctx)
    {
        ProjectOnPlane(ctx.LocomotionContact.ValueRO.Contact.Normal, ctx.GravityDirection,
            out float3 wallNormal);
        ProjectOnPlane(ctx.HorizontalDisplacement, wallNormal,
            out float3 projectedHorizontalDisplacement);
        // prevent movement against desired direction
        if (math.dot(ctx.HorizontalVelocity, projectedHorizontalDisplacement) < math.EPSILON)
        {
            projectedHorizontalDisplacement = float3.zero;
            ctx.HorizontalDisplacement = float3.zero;
        }

        ctx.HorizontalDisplacement = projectedHorizontalDisplacement;

        ProjectOnPlane(ctx.VerticalDisplacement, ctx.LocomotionContact.ValueRO.Contact.Normal,
            out float3 projectedVerticalDisplacement);
        ctx.VerticalDisplacement = projectedVerticalDisplacement;
    }

    /// <summary>
    /// Calculates the angle in radians of the slope relative to a given vertical axis direction using the slope normal vector.
    /// </summary>
    /// <param name="slopeAngleInRad">
    /// Output parameter that will hold the calculated slope angle in radians.
    /// </param>
    /// <param name="slopeNormal">
    /// The normal vector of the slope surface to calculate the angle for.
    /// </param>
    /// <param name="verticalAxis">
    /// The vector representing the vertical axis against which the slope angle is calculated.
    /// </param>
    [BurstCompile]
    public static void CalculateSlopeAngle(out float slopeAngleInRad,
        in float3 slopeNormal, in float3 verticalAxis)
    {
        var dotProduct = math.dot(slopeNormal, verticalAxis);
        // We need to clamp the dot product to the valid range of acos to handle numerical errors
        dotProduct = math.clamp(dotProduct, -1f, 1f);
        slopeAngleInRad = math.acos(dotProduct);
    }


    /// <summary>
    /// Computes the required acceleration to transition from the current velocity to a target velocity
    /// within a given time step, and clamps the acceleration to a specified maximum magnitude.
    /// </summary>
    /// <param name="accelerationToTargetVel">
    /// The computed acceleration vector needed to reach the target velocity from the current velocity.
    /// The result is clamped to the maximum acceleration if necessary.
    /// </param>
    /// <param name="currentVelocity">
    /// The current velocity of the moving object.
    /// </param>
    /// <param name="targetVelocity">
    /// The desired target velocity to achieve.
    /// </param>
    /// <param name="timeStep">
    /// The time interval over which the acceleration is applied.
    /// </param>
    /// <param name="maxAcceleration">
    /// The maximum permissible magnitude of the computed acceleration. Defaults to an unlimited value if not specified.
    /// </param>
    [BurstCompile]
    public static void ComputeAccelerationToTargetVelocity(out float3 accelerationToTargetVel,
        in float3 currentVelocity, in float3 targetVelocity,
        in float timeStep, in float maxAcceleration = float.MaxValue)
    {
        float3 deltaV = targetVelocity - currentVelocity;
        float3 accelerationToTarget = deltaV / timeStep;
        ClampMagnitude(out accelerationToTargetVel, accelerationToTarget, maxAcceleration);
    }

    /// <summary>
    /// Clamps the magnitude of a vector to a specified maximum length.
    /// </summary>
    /// <param name="clampedMagnitude">
    /// The resulting vector after clamping its magnitude.
    /// </param>
    /// <param name="v">
    /// The input vector whose magnitude will be clamped.
    /// </param>
    /// <param name="maxLength">
    /// The maximum permissible length for the vector.
    /// </param>
    [BurstCompile]
    public static void ClampMagnitude(out float3 clampedMagnitude, in float3 v, in float maxLength)
    {
        var lengthSq = math.lengthsq(v);
        var maxLengthSq = maxLength * maxLength;
        var scale = math.select(1f, maxLength / (math.sqrt(lengthSq) + 1e-6f), lengthSq > maxLengthSq);
        clampedMagnitude = v * scale;
    }

    /// <summary>
    ///     Projects a vector onto a plane defined by a normal vector.
    /// </summary>
    /// <param name="value">The vector to project onto the plane.</param>
    /// <param name="planeNormal">
    ///     The normal vector defining the plane. Does not need to be normalized.
    /// </param>
    /// <param name="projectedOnPlane">
    ///     The resulting vector after projection onto the plane.
    /// </param>
    public static void ProjectOnPlane(
        in float3 value, in float3 planeNormal,
        out float3 projectedOnPlane)
    {
        float3 projectionOnNormal = math.projectsafe(value, planeNormal);
        projectedOnPlane = value - projectionOnNormal;
    }
}