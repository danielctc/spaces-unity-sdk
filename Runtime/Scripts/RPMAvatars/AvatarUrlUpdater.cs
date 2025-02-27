using System.Collections;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Characters;
using ReadyPlayerMe.Core;
using UnityEngine;
using NinjutsuGames.FusionNetwork.Runtime;
using Spaces.React.Runtime;

namespace Spaces.Fusion.Runtime
{
    public class AvatarUrlUpdater : NetworkBehaviour
    {
        [Networked] public NetworkString<_128> NetworkedAvatarUrl { get; private set; }

        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private Character character;

        private bool isAvatarLoaded = false;
        private bool rpcPending = false; // Flag to track pending RPCs
        private const int MaxRetries = 15;
        private int retryCount = 0;

        private void Start()
        {
            ReactIncomingEvent.OnReactAvatarUrlFromReact += HandleAvatarUrlFromReact;

            if (character == null)
            {
                character = GetComponent<Character>();
            }

            if (character == null)
            {
                Debug.LogError("[AvatarUrlUpdater] Character component is missing.");
                return;
            }

            character.EventAfterChangeModel += OnCharacterModelChanged;

            StartCoroutine(WaitForAvatarUrlAndLoad());
        }

        private void OnDestroy()
        {
            ReactIncomingEvent.OnReactAvatarUrlFromReact -= HandleAvatarUrlFromReact;

            if (character != null)
            {
                character.EventAfterChangeModel -= OnCharacterModelChanged;
            }
        }

        public override void Spawned()
        {
            Debug.Log("[AvatarUrlUpdater] Network object spawned");
            StartCoroutine(WaitForAvatarUrlAndLoad());
        }

        private void HandleAvatarUrlFromReact(AvatarUrlData data)
        {
            Debug.Log($"[AvatarUrlUpdater] Received new avatar URL from React: {data.url}");
            SetPlayerAvatarUrl(data.url);
        }

        public void SetPlayerAvatarUrl(string avatarUrl)
        {
            if (Object.HasStateAuthority)
            {
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    Debug.Log($"[AvatarUrlUpdater] Setting avatar URL: {avatarUrl}");
                    NetworkedAvatarUrl = avatarUrl;
                }

                StartCoroutine(LoadAvatarModelRepeatedly());
            }
        }

        public void ChangeAvatarUrl(string newAvatarUrl)
        {
            Debug.Log($"[AvatarUrlUpdater] Changing avatar URL to: {newAvatarUrl}");
            SetPlayerAvatarUrl(newAvatarUrl);
        }

        private IEnumerator WaitForAvatarUrlAndLoad()
        {
            Debug.Log("[AvatarUrlUpdater] Waiting for avatar URL...");

            float waitTime = 2.0f;
            while (string.IsNullOrEmpty(NetworkedAvatarUrl.ToString()) && waitTime > 0)
            {
                waitTime -= 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            if (!string.IsNullOrEmpty(NetworkedAvatarUrl.ToString()))
            {
                StartCoroutine(LoadAvatarModelRepeatedly());
            }
            else
            {
                Debug.LogError("[AvatarUrlUpdater] No valid avatar URL found after waiting.");
            }
        }

        private IEnumerator LoadAvatarModelRepeatedly()
        {
            retryCount = 0;
            isAvatarLoaded = false;

            while (!isAvatarLoaded && retryCount < MaxRetries)
            {
                yield return LoadAvatarModel();
                if (!isAvatarLoaded)
                {
                    yield return new WaitForSeconds(2);
                    retryCount++;
                }
            }

            if (!isAvatarLoaded)
            {
                Debug.LogError("[AvatarUrlUpdater] Failed to load avatar after multiple attempts.");
            }
        }

        private async Task LoadAvatarModel()
        {
            Debug.Log("[AvatarUrlUpdater] Starting avatar load process");

            string avatarUrl = NetworkedAvatarUrl.ToString();

            if (string.IsNullOrEmpty(avatarUrl))
            {
                Debug.LogWarning("[AvatarUrlUpdater] Networked Avatar URL is empty or null.");
                return;
            }

            var avatarLoader = new AvatarObjectLoader();
            avatarLoader.OnCompleted += (sender, args) =>
            {
                Debug.Log("[AvatarUrlUpdater] Avatar loading completed");

                if (character == null)
                {
                    Debug.LogError("[AvatarUrlUpdater] Character component is null at OnCompleted.");
                    return;
                }

                character.ChangeModel(args.Avatar, new Character.ChangeOptions
                {
                    offset = offset
                });

                Debug.Log("[AvatarUrlUpdater] Avatar model changed successfully");
                Destroy(args.Avatar); // Clean up the loaded avatar

                isAvatarLoaded = true;

                if (rpcPending) // Check if any RPCs were queued
                {
                    Debug.Log("[AvatarUrlUpdater] Executing queued RPC actions...");
                    ExecutePendingRpcActions();
                }
            };
            avatarLoader.OnFailed += (sender, error) =>
            {
                Debug.LogError($"[AvatarUrlUpdater] Avatar loading failed: {error.Message}. Retrying... ({retryCount}/{MaxRetries})");
                retryCount++;
            };

            Debug.Log("[AvatarUrlUpdater] Loading avatar asynchronously");

            try
            {
                await avatarLoader.LoadAvatarAsync(avatarUrl);
                Debug.Log("[AvatarUrlUpdater] Avatar loaded asynchronously");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AvatarUrlUpdater] Exception during avatar loading: {ex}");
            }
        }

        private void OnCharacterModelChanged()
        {
            Debug.Log("[AvatarUrlUpdater] Model changed event triggered.");
        }

        public void QueueRpcAction()
        {
            if (!isAvatarLoaded)
            {
                Debug.Log("[AvatarUrlUpdater] Avatar not loaded yet. Queuing RPC action...");
                rpcPending = true;
                return;
            }

            Debug.Log("[AvatarUrlUpdater] Avatar is loaded. Executing RPC action immediately.");
            ExecutePendingRpcActions();
        }

        private void ExecutePendingRpcActions()
        {
            // Execute or reapply any pending RPC actions
            if (NetworkManager.Runner != null && NetworkManager.Runner.IsRunning)
            {
                Debug.Log("[AvatarUrlUpdater] Reapplying cached RPC actions...");

                NetworkCharacter.LocalPlayer.RPC(
                    RpcTargets.All,
                    Object.Id,
                    NetworkCharacter.RpcType.Actions,
                    true // Placeholder: Reapply actions
                );

                rpcPending = false; // Clear pending flag after execution
            }
        }
    }
}
