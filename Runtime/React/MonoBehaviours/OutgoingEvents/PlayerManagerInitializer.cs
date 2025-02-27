using UnityEngine;

public class PlayerManagerInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Check if PlayerManager already exists using the new API
        if (FindAnyObjectByType<PlayerManager>() == null)
        {
            // Create a new GameObject with PlayerManager if it doesn't exist
            GameObject playerManagerObject = new GameObject("PlayerManager");
            playerManagerObject.AddComponent<PlayerManager>();
            Debug.Log("PlayerManager: Created new PlayerManager instance");
        }
    }
} 