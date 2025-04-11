[System.Serializable]
public class KickPlayerData
{
    public string uid;        // The Firebase UID of the player to kick
    public string requestedBy; // UID of the user requesting the kick (for validation)
} 