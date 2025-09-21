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
using UnityEngine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// A scriptable object to define the configuration for a basic character profile.
    /// </summary>
    /// <remarks>
    /// This class extends `LocomotionProfileAsset` with a specific implementation
    /// of the `BasicLocomotionProfile`. It provides profiles for various locomotion
    /// states such as moving, falling, and jumping.
    /// </remarks>
    [Serializable]
    [CreateAssetMenu(fileName = "BasicCharacterProfileAsset",
        menuName = "ScriptableObjects/WAYN Games/Basic Character Profile Asset", order = 1)]
    public class BasicCharacterProfileAsset : LocomotionProfileAsset<BasicLocomotionProfile>
    {
        public MovingProfile Moving;
        public FallingProfile Falling;
        public JumpingProfile Jumping;

        /// <summary>
        /// Constructs and initializes a <see cref="BasicLocomotionProfile"/> instance using the provided <see cref="BlobBuilder"/>.
        /// </summary>
        /// <param name="blobBuilder">
        /// A reference to a <see cref="BlobBuilder"/> object, which is used to construct the blob asset data.
        /// </param>
        /// <returns>
        /// A reference to the constructed <see cref="BasicLocomotionProfile"/> instance populated with physics,
        /// moving, falling, and jumping profiles.
        /// </returns>
        public override ref BasicLocomotionProfile BuildBlobAsset(ref BlobBuilder blobBuilder)
        {
            ref BasicLocomotionProfile locomotionProfile = ref blobBuilder.ConstructRoot<BasicLocomotionProfile>();
            locomotionProfile.PhysicsProfileValue = PhysicsProfile;
            locomotionProfile.Moving = Moving;
            locomotionProfile.Falling = Falling;
            locomotionProfile.Jumping = Jumping;
            return ref locomotionProfile;
        }
    }
}