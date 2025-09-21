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

using System.Collections.Generic;
using Locomotion.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// The PlatformAuthoring class is responsible for configuring platform components
/// and their associated properties within the Unity Editor. It is used to initialize
/// platform behavior by baking it into ECS-compatible components, such as Platform,
/// Rotor, and Track.
/// </summary>
public class PlatformAuthoring : MonoBehaviour
{
    /// <summary>
    /// Represents the movement speed of a platform.
    /// This value is used to determine how quickly the platform moves along the <c>Track</c>.
    /// </summary>
    [SerializeField] float Speed = 1f;

    /// <summary>
    /// Represents an individual waypoint or position in a series of points that define the path along which a platform moves.
    /// </summary>
    [SerializeField] List<Transform> Track;

    /// <summary>
    /// Represents the axis of rotation for a platform rotor.
    /// This value is used to specify the rotational direction and speed of the platform in 3D space.
    /// </summary>
    [SerializeField] float3 Rotation ;



    internal class PlatformSplineAuthoringBaker : Baker<PlatformAuthoring>
    {
        public override void Bake(PlatformAuthoring authoring)
        {
            var platformEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            if (authoring.Track.Count > 0)
            {
                AddComponent(platformEntity,new Platform()
                {
                    Speed = authoring.Speed
                });
                var track = AddBuffer<Track>(platformEntity);
                foreach (var transform in authoring.Track)
                {
                    track.Add(new Track()
                    {
                        Position = transform.position
                    });
                }
            }

            if (!float3.zero.Equals(authoring.Rotation))
            {
                AddComponent(platformEntity,new Rotor()
                {
                    Axis = authoring.Rotation
                });
            }
          
        }
    }
}