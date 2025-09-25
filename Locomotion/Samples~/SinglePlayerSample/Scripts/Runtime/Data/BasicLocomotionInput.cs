using Unity.Entities;
using Unity.Mathematics;
using WAYN.Locomotion.Demo.BasicLocomotion;
using WAYNGames.Locomotion.Runtime.StateMachine;

[assembly: RegisterGenericComponentType(typeof(LocomotionInputAsset<BasicLocomotionInput>))]

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    ///     Input commands for the characters' locomotion.
    /// </summary>
    public struct BasicLocomotionInput : ICharacterLocomotionInput<BasicLocomotionInput>
    {
        /// <summary>
        /// Represents the directional movement input vector for the locomotion system.
        /// This vector is used to define the desired movement direction and magnitude
        /// for a character, typically based on player input, such as keyboard or joystick controls.
        /// </summary>
        public float3 Movement;

        /// <summary>
        /// Represents the input trigger value for initiating a jump action in the locomotion system.
        /// This value is accumulated over time to account for frame rate differences and indicates
        /// whether a jump has been requested by the player.
        /// </summary>
        public float JumpTrigger;

        /// <summary>
        /// Represents the unit vector derived from the movement input, indicating the direction of movement.
        /// This property normalizes the input movement vector to ensure it solely conveys direction,
        /// regardless of the vector's magnitude.
        /// </summary>
        public float3 MovementDirection => math.normalizesafe(Movement);

        /// <summary>
        /// Represents the magnitude of the movement input vector, clamped between 0 and 1.
        /// This value quantifies the intensity of the movement input, typically derived
        /// from the player's control inputs, such as joystick or keyboard, and is used
        /// for scaling locomotion behaviors accordingly.
        /// </summary>
        public float MovementMagnitude => math.clamp(math.length(Movement), 0f, 1f);
        
        
        public void ApplyInputsToCharacter(in BasicLocomotionInput playerInputs)
        {
            Movement = playerInputs.Movement;
            // Trigger inputs need to be accumulated to account for frame rate difference between input capture (update) and consumption (fixed updated)
             JumpTrigger += playerInputs.JumpTrigger;
        }
    }
}