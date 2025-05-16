# spaces-unity-sdk

## Prerequisites

This SDK requires the following dependencies:
- Photon Fusion (com.photon.fusion)

The dependencies will be automatically installed when you import the package. If you encounter any issues with the automatic installation, you can manually install the dependencies through the Unity Package Manager.

## HLS Streaming Support

The Spaces SDK includes robust support for HLS streaming from React to Unity using the HISPlayer video player. The implementation includes multiple fallback mechanisms to ensure compatibility with different versions of the HISPlayer SDK.

### Key Components

1. **HLSStreamReceiver**: Handles incoming HLS stream URLs from React and updates the video player
2. **VideoSamplesUpdater**: Directly updates the MultiStreamProperties collection in HISPlayerController
3. **HISPlayerAdapter**: Implements the IVideoStreamController interface for HISPlayerController

### Integration

To integrate HLS streaming in your project:

1. Attach the `HLSStreamReceiver` component to the GameObject that has your HISPlayerController
2. Assign the HISPlayerController to the `playerControllerComponent` field
3. From React, send HLS stream URLs using the JSON format described in the integration guide

See `HLS_Streaming_Integration_Guide.txt` for detailed integration instructions.

### Features

- Fixed identifier "LiveProjector" for easy targeting from React
- Direct targeting of Element 0 in MultiStreamProperties to update stream URLs
- Multiple fallback mechanisms for different HISPlayer SDK versions
- Regular status updates sent back to React
- Automatic component discovery and setup
