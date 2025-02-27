using System;
using GameCreator.Runtime.Common;
using UnityEngine;
using Spaces.Fusion.Runtime; // Namespace for AvatarUrlUpdater

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player React Avatar URL")]
    [Category("Fusion/Player React Avatar URL")]
    [Image(typeof(IconString), ColorTheme.Type.Green)]
    [Description("Set player Ready Player Me avatar URL directly from React.")]

    [Serializable]
    public class SetStringReactAvatarUrl : PropertyTypeSetString
    {
        private AvatarUrlUpdater avatarUrlUpdater;

        // Called manually by the button press, no automatic setting anymore
        public void ApplyAvatarUrl(string value, MonoBehaviour context)
        {
            Debug.Log("[Game Creator] Applying Avatar URL.");

            // Assign avatarUrlUpdater if not already assigned
            if (avatarUrlUpdater == null)
            {
                avatarUrlUpdater = FindAvatarUpdater();
                if (avatarUrlUpdater == null)
                {
                    Debug.LogError("AvatarUrlUpdater component not found on the player prefab.");
                    return;
                }
            }

            // Check if the URL is valid before setting it
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarning("[Game Creator] No valid avatar URL provided.");
                return;
            }

            // Set the Ready Player Me URL once the player is available
            Debug.Log($"[Game Creator] Setting avatar URL to: {value}");
            avatarUrlUpdater.SetPlayerAvatarUrl(value);
        }

        // Find the AvatarUrlUpdater component on the player object
        private AvatarUrlUpdater FindAvatarUpdater()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("[Game Creator] Player object not found in the scene.");
                return null;
            }

            return playerObj.GetComponentInChildren<AvatarUrlUpdater>();
        }

        public override string String => $"Player React Avatar URL";
    }
}
