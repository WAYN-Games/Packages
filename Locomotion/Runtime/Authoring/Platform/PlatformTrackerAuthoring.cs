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
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Mark an object as a tracked platform allowing it to move a character standing on it. 
/// </summary>
public class PlatformTrackerAuthoring : MonoBehaviour
{
    internal class PlatformTrackerAuthoringBaker : Baker<PlatformTrackerAuthoring>
    {
        public override void Bake(PlatformTrackerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TrackedMotion>(entity);
        }
    }
}