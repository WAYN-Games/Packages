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

using Unity.Entities;
using Unity.Mathematics;

namespace Locomotion.Runtime.Components
{
    public struct Rotor : IComponentData
    {
        public float3 Axis;
    }
    public struct Platform : IComponentData
    {
        public float Speed;
        public int Index;
    }
    public struct Track : IBufferElementData
    {
        public float3 Position;
    }

    /// <summary>
    ///     Tracks a transform over time to compute inherited movement and velocity.
    /// </summary>
    public struct TrackedMotion : IComponentData
    {
        /// <summary>Transform at the end of the previous frame.</summary>
        public RigidTransform PreviousTransform;

        /// <summary>Transform at the end of the current frame.</summary>
        public RigidTransform CurrentTransform;

        /// <summary>Time elapsed between the two transforms.</summary>
        public float DeltaTime;
    }

}