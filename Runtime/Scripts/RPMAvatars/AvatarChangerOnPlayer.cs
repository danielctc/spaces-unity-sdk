using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Fusion.Runtime
{
    public class AvatarChangerOnPlayer : MonoBehaviour
    {
        [SerializeField] private AvatarUrlUpdater avatarUrlUpdater;

        private void Start()
        {
            if (avatarUrlUpdater == null)
            {
                avatarUrlUpdater = GetComponentInChildren<AvatarUrlUpdater>();
                if (avatarUrlUpdater == null)
                {
                    Debug.LogError("[AvatarChangerOnPlayer] AvatarUrlUpdater component not found.");
                }
                else
                {
                    Debug.Log("[AvatarChangerOnPlayer] AvatarUrlUpdater component found.");
                }
            }
        }

        public void ChangeAvatarUrl(string newAvatarUrl)
        {
            if (avatarUrlUpdater == null)
            {
                Debug.LogError("[AvatarChangerOnPlayer] AvatarUrlUpdater is not assigned.");
                return;
            }

            if (string.IsNullOrEmpty(newAvatarUrl))
            {
                Debug.LogError("[AvatarChangerOnPlayer] Provided avatar URL is empty.");
                return;
            }

            Debug.Log($"[AvatarChangerOnPlayer] Changing avatar to new URL: {newAvatarUrl}");
            avatarUrlUpdater.ChangeAvatarUrl(newAvatarUrl);
        }
    }
}
