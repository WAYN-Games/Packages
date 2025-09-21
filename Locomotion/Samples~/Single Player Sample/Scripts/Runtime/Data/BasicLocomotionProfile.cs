using System;
using Locomotion.Runtime.Components;
using UnityEngine.Serialization;
using Wayn.Locomotion.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// Locomotion profiles used to define movement behavior for a character for various states of locomotion, such as
    /// moving, falling, and jumping, as well as the overall physics profile of the character.
    /// It implements the ILocomotionProfile interface to enforce the minimal configuration required for the state machine to work.
    /// </summary>
    [Serializable]
    public struct BasicLocomotionProfile : ILocomotionProfile
    {

        public MovingProfile Moving;
        public FallingProfile Falling;
        public JumpingProfile Jumping;

        public CharacterPhysicsProfile PhysicsProfileValue { get; set; }
    }
}