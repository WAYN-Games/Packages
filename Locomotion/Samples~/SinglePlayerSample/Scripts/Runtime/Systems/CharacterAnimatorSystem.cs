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
using UnityEngine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// CharacterAnimatorSystem is responsible for updating the parameters of Unity's Animator component
    /// based on data provided by the AnimationBlackBoard. This system operates within the
    /// PresentationSystemGroup, ensuring that animation updates are processed during the presentation phase
    /// of the Entity Component System (ECS) pipeline.
    /// </summary>
    /// <remarks>
    /// This system queries pairs of Animator components and corresponding AnimationBlackBoard data components.
    /// It utilizes values stored in the AnimationBlackBoard to adjust Animator parameters, enabling the animation
    /// system to reflect state changes such as velocity and jumping.
    /// </remarks>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct CharacterAnimatorSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            foreach (var (animator, blackBoard) in SystemAPI
                         .Query<SystemAPI.ManagedAPI.UnityEngineComponent<Animator>, RefRO<AnimationBlackBoard>>())
                blackBoard.ValueRO.Set(animator.Value);
        }
    }


    public struct AnimationBlackBoard : IComponentData
    {
        public float XVel;
        public float YVel;
        public float ZVel;
        public bool Jumping;

        static readonly int ZVelParam = Animator.StringToHash("velZ");
        static readonly int YVelParam = Animator.StringToHash("velY");
        static readonly int XVelParam = Animator.StringToHash("velX");
        static readonly int JumpingParam = Animator.StringToHash("jumping");

        public void Set(Animator animator)
        {
            animator.SetBool(JumpingParam, Jumping);
            animator.SetFloat(ZVelParam, ZVel);
            animator.SetFloat(YVelParam, YVel);
            animator.SetFloat(XVelParam, XVel);
        }
    }
}