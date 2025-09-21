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

using Locomotion.Runtime.Authoring;
using Locomotion.Runtime.Components;
using Unity.Physics;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    public class BasicCharacterLocomotionAuthoring : BaseCharacterAuthoring<BasicLocomotionProfile>
    {
        public class BasicCharacterLocomotionAuthoringBaker : BaseCharacterBaker<BasicCharacterLocomotionAuthoring,
            BasicLocomotionStatesStateMachine, BasicLocomotionStates,
            BasicLocomotionContext, BasicLocomotionInput, BasicLocomotionStateData>
        {
            // completely optional
            protected override void ContinueBaking()
            {
                // Override the gravity component to take into consideration de the fall multipler
                SetComponent(LocomotorEntity, new LocomotionGravity
                {
                    Gravity = PhysicsStep.Default.Gravity * Profile.Falling.GravityMultiplier
                });

                // add a component to the Model entity to gather data for the Animator.
                AddComponent<AnimationBlackBoard>(ModelEntity);
            }
        }
    }
}