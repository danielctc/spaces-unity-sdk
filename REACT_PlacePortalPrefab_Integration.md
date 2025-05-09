# React to Unity: Placing Portal Prefabs

To instruct Unity to place a portal prefab, your React application needs to dispatch an event named `"PlacePortalPrefab"` to the Unity instance. This event should carry a JSON payload containing the necessary data for the portal's instantiation and configuration.

## Event Details

-   **Event Name**: `"PlacePortalPrefab"`
-   **Payload Structure**: The `data` field of the dispatched event should be a JSON string representing an object with the following properties:

    ```json
    {
      "portalId": "unique_portal_identifier_string",
      "prefabName": "Optional_Portal_Prefab_Name_String",
      "position": {
        "x": 0.0,
        "y": 1.5,
        "z": 5.0
      },
      "rotation": { // Euler angles
        "x": 0.0,
        "y": 90.0,
        "z": 0.0
      },
      "scale": {
        "x": 1.0,
        "y": 1.0,
        "z": 1.0
      }
    }
    ```

    **Field Descriptions:**

    *   `portalId` (string, **required**): A unique identifier for this specific portal instance. This ID will be assigned to the `PortalManager` component on the instantiated portal prefab.
    *   `prefabName` (string, *optional*): An optional name or identifier for the type of portal prefab you wish to place. This can be used in the `SimplePortalPlacer` script if you extend it to handle multiple portal prefab types.
    *   `position` (object, **required**): An object with `x`, `y`, and `z` float properties representing the world-space coordinates where the portal prefab should be placed.
    *   `rotation` (object, **required**): An object with `x`, `y`, and `z` float properties representing the Euler angles (in degrees) for the portal prefab's orientation.
    *   `scale` (object, **required**): An object with `x`, `y`, and `z` float properties representing the local scale of the portal prefab.

## Example: Dispatching the Event from JavaScript

Assuming you have a way to send events to your Unity WebGL build (typically via a function exposed on the `window` object by Unity, which in your `React.jslib` is `window.dispatchReactUnityEvent`), the JavaScript code would look something like this:

```javascript
function placePortalInUnity(portalData) {
  if (window.dispatchReactUnityEvent) {
    // The second argument (portalData) will be automatically stringified 
    // by the `dispatchReactUnityEvent` function if it's an object,
    // or you can stringify it yourself.
    // The `JsPlacePortalPrefab` in React.jslib expects a string.
    const jsonDataString = JSON.stringify(portalData);
    window.dispatchReactUnityEvent("PlacePortalPrefab", jsonDataString);
    console.log("React: Sent PlacePortalPrefab event to Unity with data:", portalData);
  } else {
    console.error("React: Unity event dispatcher (dispatchReactUnityEvent) not found.");
  }
}

// Example usage:
const myPortalData = {
  portalId: "myOfficePortal_001",
  prefabName: "StandardBluePortal", // Optional
  position: { x: 10.2, y: 1.0, z: -3.5 },
  rotation: { x: 0, y: 45, z: 0 },
  scale: { x: 2, y: 2, z: 0.1 } // A thin portal
};

placePortalInUnity(myPortalData);
```

## How Unity Handles This

1.  The `JsPlacePortalPrefab` function in `Extensions/WebGL/React.jslib` receives this event and data.
2.  It forwards the event name (`"PlacePortalPrefab"`) and the JSON data string to the `HandleEvent` method in `Runtime/React/EventsManagement/ReactIncomingEvent.cs` in your Unity project.
3.  `ReactIncomingEvent.cs` parses the JSON string into a `PortalPrefabPlacementData` object.
4.  It then invokes the `OnPlacePortalPrefab` C# event.
5.  The `SimplePortalPlacer.cs` script, subscribed to `OnPlacePortalPrefab`, receives this data.
6.  `SimplePortalPlacer.cs` instantiates the specified portal prefab at the given position, rotation, and scale, and assigns the `portalId` to the `PortalManager` component on the new instance.
7.  The `PortalManager` on the newly instantiated portal will then handle its own registration with React (e.g., in its `OnEnable` or `Awake` method) using the provided `portalId`.

Ensure the `portalId` is unique for each portal you intend to place, as this is crucial for the `PortalManager` to correctly identify and manage its state with React. 