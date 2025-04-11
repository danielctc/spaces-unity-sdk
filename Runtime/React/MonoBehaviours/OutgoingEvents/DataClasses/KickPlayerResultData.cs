[System.Serializable]
public class KickPlayerResultData
{
    public bool success;             // Whether the kick was successful
    public string playerName;        // Name of the player who was kicked
    public string playerUid;         // UID of the player who was kicked
    public string errorMessage;      // Error message if the kick failed
} 