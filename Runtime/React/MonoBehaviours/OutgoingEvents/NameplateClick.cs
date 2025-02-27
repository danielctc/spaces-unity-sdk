using UnityEngine;
using Spaces.React.Runtime;
using NinjutsuGames.FusionNetwork.Runtime;
using Fusion;

public class NameplateClick : NetworkBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"Unity: Nameplate clicked on {gameObject.name}");
        
        NetworkPlayer networkPlayer = GetComponentInParent<NetworkPlayer>();
        
        string nickname;
        string playerId;

        // Compare the InputAuthority to the local player's PlayerRef
        if (networkPlayer != null && networkPlayer.Object.InputAuthority != NetworkManager.Runner.LocalPlayer)
        {
            // This is a remote player, so use their network-synced username
            nickname = networkPlayer.Username.Value;
            playerId = networkPlayer.Object.InputAuthority.ToString();
            Debug.Log($"Unity: Remote player clicked, Username: {nickname}");
        }
        else
        {
            // This is the local player, so use the Firebase data
            var userData = UserManager.CurrentUser;
            nickname = userData?.Nickname ?? "Unknown";
            playerId = userData?.uid ?? "Unknown";
            Debug.Log($"Unity: Local player clicked, Nickname: {nickname}");
        }

        NameplateClickData nameplateData = new NameplateClickData
        {
            playerName = nickname,
            playerId = playerId,
            nickname = nickname
        };

        string jsonData = JsonUtility.ToJson(nameplateData);
        Debug.Log($"Unity: Sending nameplate click data: {jsonData}");
        
        ReactRaiseEvent.OpenNameplateModal(nameplateData);
    }
}
