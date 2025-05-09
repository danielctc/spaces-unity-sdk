using UnityEngine;

namespace Spaces.Fusion.Runtime 
{
    public class HoverEffect : MonoBehaviour
    {
        [Tooltip("The speed at which the object hovers up and down.")]
        public float hoverSpeed = 1f;

        [Tooltip("The maximum height the object moves up and down from its current position.")]
        public float hoverHeight = 0.5f;

        private Vector3 basePosition;

        void Start()
        {
            // Initialize base position
            basePosition = transform.position;
        }

        void Update()
        {
            // Store the current position as the new base position
            basePosition = new Vector3(transform.position.x, basePosition.y, transform.position.z);
            
            // Calculate the new vertical position using a sine wave
            float newY = basePosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

            // Update the object's position, preserving X and Z from the current position
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
} 