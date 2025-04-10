# Experimental

This folder contains experimental scripts and utilities that are not part of the main SpacesSDK feature set but can be useful for testing and debugging.

## Contents

- **FusionPlayersDisplay**: A test component for visualizing all players in a Fusion Shared Mode room
- **PlayerEntryUI**: UI component for displaying individual player entries in the player list
- **FusionPlayersDisplayPrefab**: Helper component for creating and setting up the FusionPlayersDisplay at runtime

## Usage

These components are primarily intended for development and debugging purposes. They demonstrate how to access and display information about connected players in a Fusion Shared Mode session.

To use in your Unity project:
1. Create a Canvas GameObject in your scene
2. Add the FusionPlayersDisplay component to it
3. Start a Fusion Shared Mode session to see connected players

For runtime creation, you can use:
```csharp
GameObject display = FusionPlayersDisplayPrefab.CreatePlayersDisplayInstance();
```

## Unity Requirements

- TextMeshPro package
- Fusion 2 networking 