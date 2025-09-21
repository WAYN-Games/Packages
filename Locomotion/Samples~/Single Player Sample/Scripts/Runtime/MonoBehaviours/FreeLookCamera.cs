using UnityEngine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{

    public class FreeLookCamera : MonoBehaviour
    {
        public Transform target; // The object to orbit around
        public float distance = 5f; // Default distance from target
        public float zoomSpeed = 2f; // Scroll wheel zoom speed
        public float minDistance = 2f; // Minimum zoom distance
        public float maxDistance = 15f; // Maximum zoom distance

        public float xSpeed = 120f; // Horizontal rotation speed
        public float ySpeed = 120f; // Vertical rotation speed
        public float minY = -20f; // Minimum vertical angle
        public float maxY = 80f; // Maximum vertical angle

        float x;
        float y;

        void Start()
        {
            if (target == null)
            {
                Debug.LogError("FreeLookCamera: No target assigned.");
                enabled = false;
                return;
            }

            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;

            // Optional: Lock and hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            // Rotate with mouse
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, minY, maxY);

            // Zoom with scroll wheel
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            // Calculate rotation and position
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
            transform.position = target.position + offset;

            // Always look at target
            transform.LookAt(target);
        }
    }
}