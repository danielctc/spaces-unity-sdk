// This script has been retired and is no longer in use.
// Use the new OnFallingBelowFloor script instead for fall detection and respawn logic.

/*
using UnityEngine;

namespace Spaces.Core
{
    public class Respawn : MonoBehaviour 
    {
        public float threshold;
        public GameObject lightningEffectPrefab;  // Reference to your lightning effect prefab

        private float timeFalling = 0f;
        private bool hasRespawned = false;
        private Transform parentTransform;

        void Start()
        {
            // Find the parent transform
            parentTransform = transform.parent;

            if (parentTransform == null)
            {
                Debug.LogError("No parent found. Respawn script requires the GameObject to be a child of another GameObject.");
            }
        }

        void FixedUpdate() 
        {
            if (transform.position.y < threshold) 
            {
                if (!hasRespawned)
                {
                    timeFalling += Time.fixedDeltaTime;  // Increment the time the player has been falling

                    if (timeFalling >= 0.5f && timeFalling < 2f)  // Just before 2 seconds
                    {
                        CreateLightningEffect();
                    }

                    if (timeFalling > 2f)  // Check if falling for more than 2 seconds
                    {
                        RespawnPlayer();
                    }
                }
            } 
            else 
            {
                timeFalling = 0f;  // Reset the timer if player is above the threshold
                hasRespawned = false;  // Reset the respawn flag
            }
        }

        void CreateLightningEffect()
        {
            // Instantiate the lightning effect just below the player
            Vector3 effectPosition = transform.position + Vector3.down;
            Instantiate(lightningEffectPrefab, effectPosition, Quaternion.identity);
        }

        void RespawnPlayer() 
        {
            if (parentTransform != null)
            {
                // Reset the parent GameObject's position
                parentTransform.position = new Vector3(0, 5, 0);
                Debug.Log("Parent object respawned at (0, 5, 0)");
            }
            else
            {
                // Fallback: reset the current GameObject's position
                transform.position = new Vector3(0, 5, 0);
                Debug.Log("Current object respawned at (0, 5, 0)");
            }

            timeFalling = 0f;  // Reset the falling timer
            hasRespawned = true;  // Set the respawn flag
        }
    }
}
*/
