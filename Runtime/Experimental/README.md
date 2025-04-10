# Experimental Fusion Players Display

This folder contains experimental components for testing and debugging Fusion Shared Mode multiplayer functionality.

## FusionPlayersDisplay

A test component that displays all players currently in a Fusion Shared Mode room. This is useful for debugging multiplayer sessions and visualizing connected players.

### Features

- Shows all connected players in a Fusion room
- Highlights the local player
- Displays player IDs and optional UIDs
- Auto-refreshes to keep the player list up to date
- Works with both lobby and direct connection scenarios
- **Player Kick Functionality**: Ability to kick players from the room

### Usage Instructions

#### 1. Add the Component to a Scene

- Create a new GameObject in your scene
- Add a Canvas component to it (UI → Canvas)
- Add the `FusionPlayersDisplay` component

#### 2. Configure the Display (Optional)

The component will work with default settings, but you can customize:

- **Refresh Interval**: How often the player list updates
- **Show Detailed Player Info**: Whether to display additional player information like UIDs
- **Log Players On Refresh**: Enable for additional console logging
- **Result Display Time**: How long the kick notification displays

#### 3. Custom Player Entry Prefab (Optional)

For advanced customization:
- Create a prefab with a `PlayerEntryUI` component
- Assign it to the `Player Entry Prefab` field in the inspector

### Player Kick Functionality

Each player entry includes a "Kick" button that allows you to remove remote players from the room:

1. Click the "Kick" button next to a player's name
2. Confirm the action in the dialog
3. The player will be disconnected from the session

Notes on the kick system:
- Only works if you have StateAuthority (host/server)
- If you don't have authority, an RPC request is sent to the host
- Kicked players are returned to the main scene (scene index 0)
- Kick confirmations are shown to all players in the room

### Unity Setup Requirements

To use this component in your project:

1. Ensure you have Fusion 2 installed
2. Make sure TextMeshPro is in your project
3. The Fusion NetworkRunner must be initialized in Shared mode

### Example Scene Setup

```csharp
// Sample code to initialize Fusion in Shared mode
public class FusionSetup : MonoBehaviour
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private GameObject fusionPlayerDisplayPrefab;

    private NetworkRunner _runner;

    private async void Start()
    {
        _runner = Instantiate(networkRunnerPrefab);
        
        // Configure the connection
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared, // Important: Must be in Shared mode
            SessionName = "TestRoom",
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        };
        
        // Start the session
        await _runner.StartGame(startGameArgs);
        
        // Add the player display UI after connection
        var display = Instantiate(fusionPlayerDisplayPrefab);
    }
}
```

## Troubleshooting

- If players aren't showing up, ensure your NetworkRunner is in Shared mode and player objects are being spawned properly
- Check console logs for any errors related to player spawning
- Verify that each player has a NetworkPlayer component with Username properly set 