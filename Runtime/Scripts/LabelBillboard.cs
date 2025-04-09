using UnityEngine;

namespace Spaces.Fusion.Runtime // Added namespace
{
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            // Cache the main camera. Consider making this more robust if Camera.main might not be available/correct.
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Billboard: Main Camera not found. Make sure your camera is tagged 'MainCamera'.", this);
            }
        }

        // Using LateUpdate ensures the camera has finished its movement for the frame
        void LateUpdate()
        {
            if (mainCamera != null)
            {
                // Option 1: Make it fully face the camera (match camera's rotation)
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                 mainCamera.transform.rotation * Vector3.up);

                // Option 2: Make it only rotate around the world Y axis to face camera horizontally
                // Vector3 directionToCamera = mainCamera.transform.position - transform.position;
                // directionToCamera.y = 0; // Ignore vertical difference
                // if (directionToCamera.sqrMagnitude > 0.001f) // Avoid zero vector
                // {
                //    transform.rotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up); // Look opposite to the direction
                //}
            }
            else
            {
                // Attempt to find the camera again if it wasn't found initially
                mainCamera = Camera.main;
            }
        }
    }
}