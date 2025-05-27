using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Spaces.Fusion.Runtime;

namespace Spaces.React.Runtime
{
    [Title("Set Ready Player Me Avatar URL from React")]
    [Description("Sets the RPM avatar URL for the player based on a URL received from React.")]

    [Category("Spaces/Player/Set RPM React Avatar URL")]
    public class InstructionSetReactAvatarUrl : Instruction
    {
        [SerializeField] private InputField inputField; // Input field to be set in the Unity Inspector

        public override string Title => "Set Player Avatar URL from React";

        protected override async Task Run(Args args)
        {
            // Step 1: Wait for the player to spawn before doing anything
            AvatarManagerUpdater avatarManagerUpdater = await WaitForPlayerSpawnAndManagerUpdater();
            if (avatarManagerUpdater == null)
            {
                Debug.LogError("[Game Creator] AvatarManagerUpdater component not found.");
                return;
            }

            // Step 2: Retrieve the URL from the InputField (representing React input)
            string avatarUrl = inputField != null ? inputField.text : null;

            // Step 3: Check if the input URL is valid
            if (string.IsNullOrEmpty(avatarUrl))
            {
                Debug.LogError("[Game Creator] No valid avatar URL available.");
                return;
            }

            // Step 4: Set the avatar URL in the manager
            avatarManagerUpdater.ChangeAvatarUrl(avatarUrl);
            Debug.Log($"[Game Creator] Avatar URL set to: {avatarUrl}");
        }

        // Helper method to wait for the player object to spawn and find the new AvatarManagerUpdater
        private async Task<AvatarManagerUpdater> WaitForPlayerSpawnAndManagerUpdater()
        {
            AvatarManagerUpdater avatarManagerUpdater = null;
            GameObject playerObj = null;

            while (playerObj == null)
            {
                playerObj = GetLocalPlayerObject();
                if (playerObj == null)
                {
                    Debug.Log("[Game Creator] Waiting for player object to spawn...");
                    await Task.Delay(100);
                }
            }

            // Find AvatarManagerUpdater on the player object
            avatarManagerUpdater = playerObj.GetComponent<AvatarManagerUpdater>();
            return avatarManagerUpdater;
        }

        // Helper method to find the local player object
        private GameObject GetLocalPlayerObject()
        {
            foreach (var player in NetworkRunner.Instances[0].ActivePlayers)
            {
                if (player == NetworkRunner.Instances[0].LocalPlayer)
                {
                    NetworkObject playerObj = NetworkRunner.Instances[0].GetPlayerObject(player);
                    if (playerObj != null)
                    {
                        return playerObj.gameObject;
                    }
                }
            }
            return null;
        }
    }
}
