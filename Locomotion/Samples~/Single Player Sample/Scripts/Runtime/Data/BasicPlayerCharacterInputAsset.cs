using System;
using Locomotion.Runtime.Authoring.Player;
using Locomotion.Runtime.Components;
using Unity.Mathematics;
using UnityEngine;
using WAYN.Locomotion.Demo.BasicLocomotion;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// A specialized ScriptableObject representing a player's input asset for basic locomotion.
    /// This class provides the mechanisms to map inputs from a player (such as key presses or
    /// joystick motions) into meaningful locomotion values for gameplay behavior. It uses the
    /// camera's perspective to determine movement vector directions and handles actions like jumping.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "BasicCharacterProfileAsset",
        menuName = "ScriptableObjects/WAYN Games/Basic Player Character Input Asset", order = 1)]
    public class BasicPlayerCharacterInputAsset : PlayerInputAsset<BasicLocomotionInput>
    {
        /// <summary>
        /// Reference to the player's camera used for determining movement directions
        /// and input in relation to the camera's perspective in the game world.
        /// </summary>
        private Camera m_PlayerCamera;

        public override void ReadInputsFromAsset(out BasicLocomotionInput inputs)
        {
            Transform cameraTransform = m_PlayerCamera.transform;
            float3 cameraSpaceMovement = cameraTransform.forward * Input.GetAxis("Vertical") +
                                         cameraTransform.right * Input.GetAxis("Horizontal");
            float3 groundSpaceMovement = cameraSpaceMovement - math.projectsafe(cameraSpaceMovement, math.up());
            inputs.Movement = math.normalizesafe(groundSpaceMovement);
            inputs.JumpTrigger = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;
        }


        public override void InitInputAsset(PlayerGameObject playerGameObject)
        {
            // Get the camera from the player game object and cache it for use when reading inputs.
            m_PlayerCamera = playerGameObject.Instance.Value.GetComponent<PlayerGoAuthoring>().Camera;
        }
    }
}