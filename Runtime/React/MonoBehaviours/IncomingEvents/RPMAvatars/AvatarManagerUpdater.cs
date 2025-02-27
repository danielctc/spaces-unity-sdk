using System;
using Fusion;
using UnityEngine;

namespace Spaces.Fusion.Runtime
{
    public class AvatarManagerUpdater : NetworkBehaviour
    {
        // Reference to the AvatarUrlUpdater to handle avatar changes
        public AvatarUrlUpdater avatarUrlUpdater;

        public override void Spawned()
        {
            // Check for state authority to manage avatar updates
            if (Object.HasStateAuthority)
            {
                Debug.Log("[AvatarManagerUpdater] Player spawned with state authority.");

                // Try to assign avatarUrlUpdater if it's not set in the Inspector
                if (avatarUrlUpdater == null)
                {
                    avatarUrlUpdater = GetComponentInChildren<AvatarUrlUpdater>();
                }

                if (avatarUrlUpdater == null)
                {
                    Debug.LogError("[AvatarManagerUpdater] AvatarUrlUpdater component is missing.");
                }
                else
                {
                    Debug.Log("[AvatarManagerUpdater] AvatarUrlUpdater component successfully assigned.");
                }
            }
        }

        // Method to update the avatar URL dynamically
        public void ChangeAvatarUrl(string newAvatarUrl)
        {
            if (avatarUrlUpdater != null)
            {
                Debug.Log($"[AvatarManagerUpdater] Changing avatar to new URL: {newAvatarUrl}");
                avatarUrlUpdater.ChangeAvatarUrl(newAvatarUrl);
            }
            else
            {
                Debug.LogError("[AvatarManagerUpdater] AvatarUrlUpdater is not assigned.");
            }
        }
    }
}
