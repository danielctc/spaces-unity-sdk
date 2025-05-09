# React to Unity: Setting Portal Images

To update a portal's image in Unity, your React application needs to dispatch a `"SetPortalImage"` event with the portal's data.

## Event Details

- **Event Name**: `"SetPortalImage"`
- **Payload Structure**: The `data` field should be a JSON string with the following structure:

```json
{
  "portalId": "unique_portal_identifier_string",
  "imageUrl": "https://example.com/path/to/image.jpg"
}
```

**Field Descriptions:**
* `portalId` (string, **required**): The unique identifier of the portal to update. This must match the `portalId` assigned to the portal in Unity.
* `imageUrl` (string, **required**): The URL of the image to display on the portal.

## Example: Dispatching the Event from JavaScript

```javascript
function updatePortalImage(portalId, imageUrl) {
  if (window.dispatchReactUnityEvent) {
    const portalData = {
      portalId: portalId,
      imageUrl: imageUrl
    };
    
    const jsonDataString = JSON.stringify(portalData);
    window.dispatchReactUnityEvent("SetPortalImage", jsonDataString);
    console.log("React: Sent Portal image update:", portalData);
  } else {
    console.error("React: Unity event dispatcher (dispatchReactUnityEvent) not found.");
  }
}

// Example usage:
updatePortalImage("myOfficePortal_001", "https://example.com/office-view.jpg");
```

## How Unity Handles This

1. The `JsSetPortalImage` function in `React.jslib` receives the event and data.
2. It forwards the event to the `HandleEvent` method in `ReactIncomingEvent.cs`.
3. `ReactIncomingEvent.cs` parses the JSON into a `PortalImageData` object and invokes the `OnSetPortalImage` event.
4. The `PortalManager` component on the target portal receives the event and:
   - Verifies the `portalId` matches
   - Loads the image from the provided URL
   - Applies the image to the portal's material

## Best Practices

1. **Image URLs**:
   - Use HTTPS URLs for security
   - Ensure images are accessible (CORS-enabled if needed)
   - Consider image size and format (JPEG/PNG recommended)
   - Use CDN URLs for better performance

2. **Portal IDs**:
   - Always use the exact `portalId` assigned in Unity
   - Keep track of portal IDs in your React state
   - Validate portal IDs before sending updates

3. **Error Handling**:
   - Check if the image URL is valid before sending
   - Handle cases where the portal might not exist
   - Implement retry logic for failed image loads

## Example React Component

```jsx
import { useState } from 'react';

function PortalImageUpdater({ portalId, imageUrl }) {
  const [isUpdating, setIsUpdating] = useState(false);

  const updatePortalImage = () => {
    if (!portalId || !imageUrl) {
      console.error('Portal ID and image URL are required');
      return;
    }

    setIsUpdating(true);
    
    try {
      const portalData = {
        portalId: portalId,
        imageUrl: imageUrl
      };
      
      const jsonDataString = JSON.stringify(portalData);
      window.dispatchReactUnityEvent("SetPortalImage", jsonDataString);
      console.log('Portal image update sent:', portalData);
    } catch (error) {
      console.error('Failed to update portal image:', error);
    } finally {
      setIsUpdating(false);
    }
  };

  return (
    <div>
      <button 
        onClick={updatePortalImage}
        disabled={isUpdating || !portalId || !imageUrl}
      >
        {isUpdating ? 'Updating...' : 'Update Portal Image'}
      </button>
    </div>
  );
}

// Usage:
<PortalImageUpdater 
  portalId="myOfficePortal_001"
  imageUrl="https://example.com/office-view.jpg"
/>
```

## Testing

To test the portal image updates:
1. Ensure the portal exists in Unity with the correct `portalId`
2. Send a test image URL from React
3. Verify the image loads and displays correctly on the portal
4. Check the Unity console for any error messages
5. Verify the image updates when changing the URL

Remember that the portal must be properly initialized in Unity with a `PortalManager` component and a valid `portalId` before it can receive image updates. 