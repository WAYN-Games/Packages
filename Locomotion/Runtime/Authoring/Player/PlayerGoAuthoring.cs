using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Locomotion.Runtime.Authoring.Player
{
    /// <summary>
    /// Most of the player components (camera, input, ...) still required MonoBehaviour.
    /// This authoring component has to be set on a game object prefab that will represent the game object side of the player and handle all MonoBehaviour interactions. 
    /// </summary>
    public class PlayerGoAuthoring : MonoBehaviour
    {
        /// <summary>
        /// Camera utilized for rendering the player's view.
        /// If no specific camera is set, the main camera will be used by default.
        /// </summary>
        [Tooltip("Camera to use for rendering the player's view. If not set, the main camera will be used." )]
        public Camera Camera;

        /// <summary>
        /// Transform to serve as the target position for the player camera.
        /// This is updated by the character's controller to ensure the player's camera
        /// follows the character's movements.
        /// If not assigned, a new Transform will be created as a child of the player game object.
        /// </summary>
        [Tooltip(
            "Transform to use as a camera target. This transform is updated by the character's controller. If not set, a new game object will be created as a child of the player game object." )]
        public Transform CameraTarget;


        /// <summary>
        /// Strategy used to determine how the player's camera follows the controlled character(s).
        /// - None : Disable the following of the camera
        /// - First : follow the first character in the controlled character buffer
        /// - Average : averages the position of all characters controlled by the player
        /// </summary>
        [Tooltip(
            "Select a follow strategy for the camera target." )]
        public FollowStrategy FollowStrategy;

        void Awake()
        {
            if (!Camera) Camera = Camera.main;
            if (CameraTarget) return;
            CameraTarget = new GameObject("CameraTarget").transform;
            CameraTarget.parent = transform;
        }
    }

    public enum FollowStrategy
    {
        None,
        First,
        Average
    }
}