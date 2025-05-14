using UnityEngine;

namespace Spaces.Fusion.Runtime
{
    public class ObjectFaceCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("The camera to face. If not assigned, will use Camera.main")]
        [SerializeField] private Camera targetCamera;

        [Header("Rotation Settings")]
        [Tooltip("If true, object will only rotate around Y axis (stay upright)")]
        [SerializeField] private bool lockYAxis = true;
        
        [Tooltip("Speed at which the object rotates to face camera. Higher values = faster rotation. 0 = instant rotation")]
        [SerializeField] private float rotationSpeed = 5f;

        private void Start()
        {
            // If no camera is assigned, try to get the main camera
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogWarning("ObjectFaceCamera: No camera assigned and Camera.main not found. Make sure your camera is tagged 'MainCamera'.", this);
                }
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            if (lockYAxis)
            {
                // Get direction to camera, but ignore vertical difference
                Vector3 directionToCamera = targetCamera.transform.position - transform.position;
                directionToCamera.y = 0; // This keeps the object upright
                
                if (directionToCamera.sqrMagnitude > 0.001f) // Avoid zero vector
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
                    
                    if (rotationSpeed > 0)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                    else
                    {
                        transform.rotation = targetRotation;
                    }
                }
            }
            else
            {
                // Make the object face the camera exactly
                Quaternion targetRotation = Quaternion.LookRotation(
                    transform.position + targetCamera.transform.rotation * Vector3.forward,
                    targetCamera.transform.rotation * Vector3.up
                );

                if (rotationSpeed > 0)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = targetRotation;
                }
            }
        }
    }
} 