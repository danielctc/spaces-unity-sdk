import React, { useState, useEffect } from 'react';

/**
 * MediaScreenManager Component
 * 
 * This component demonstrates how to:
 * 1. Listen for MediaScreen registrations from Unity
 * 2. Send images to MediaScreens
 * 3. Send videos with thumbnails to MediaScreens
 * 4. Handle video playback when a user clicks on a video thumbnail
 */
const MediaScreenManager = () => {
  // State to track registered media screens
  const [mediaScreens, setMediaScreens] = useState([]);
  
  // State for form inputs
  const [selectedScreenId, setSelectedScreenId] = useState('');
  const [mediaType, setMediaType] = useState('image');
  const [imageUrl, setImageUrl] = useState('');
  const [videoUrl, setVideoUrl] = useState('');
  const [thumbnailUrl, setThumbnailUrl] = useState('');
  
  // State for video player
  const [videoPlayerVisible, setVideoPlayerVisible] = useState(false);
  const [currentVideoUrl, setCurrentVideoUrl] = useState('');

  // Example media for testing
  const exampleImages = [
    'https://picsum.photos/800/600',
    'https://picsum.photos/800/600?random=1',
    'https://picsum.photos/800/600?random=2',
  ];
  
  const exampleVideos = [
    {
      videoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4',
      thumbnailUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/BigBuckBunny.jpg'
    },
    {
      videoUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4',
      thumbnailUrl: 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/ElephantsDream.jpg'
    }
  ];

  useEffect(() => {
    // Set up event listeners when component mounts
    window.addEventListener('RegisterMediaScreen', handleMediaScreenRegistration);
    window.addEventListener('MediaScreenClick', handleMediaScreenClick);
    window.addEventListener('PlayMediaScreenVideo', handlePlayMediaScreenVideo);
    
    // Clean up event listeners when component unmounts
    return () => {
      window.removeEventListener('RegisterMediaScreen', handleMediaScreenRegistration);
      window.removeEventListener('MediaScreenClick', handleMediaScreenClick);
      window.removeEventListener('PlayMediaScreenVideo', handlePlayMediaScreenVideo);
    };
  }, []);

  // Handle media screen registration events from Unity
  const handleMediaScreenRegistration = (event) => {
    const data = JSON.parse(event.detail);
    console.log('Media screen registered:', data);
    
    // Add to our list of media screens if it's not already there
    setMediaScreens(prevScreens => {
      const exists = prevScreens.some(screen => screen.mediaScreenId === data.mediaScreenId);
      if (!exists) {
        return [...prevScreens, data];
      }
      return prevScreens;
    });
    
    // If this is our first screen, select it automatically
    if (mediaScreens.length === 0) {
      setSelectedScreenId(data.mediaScreenId);
    }
  };

  // Handle click events from media screens
  const handleMediaScreenClick = (event) => {
    const data = JSON.parse(event.detail);
    console.log('Media screen clicked:', data);
    // You can implement custom behavior when a media screen is clicked
  };

  // Handle video playback requests from Unity
  const handlePlayMediaScreenVideo = (event) => {
    const data = JSON.parse(event.detail);
    console.log('Play video requested:', data);
    
    // Open the video player with the requested video
    if (data.videoUrl) {
      openVideoPlayer(data.videoUrl);
    } else {
      // If no videoUrl was provided, try to find it in our state
      const screen = mediaScreens.find(s => s.mediaScreenId === data.mediaScreenId);
      if (screen && screen.videoUrl) {
        openVideoPlayer(screen.videoUrl);
      }
    }
  };

  // Open the video player with the specified URL
  const openVideoPlayer = (url) => {
    setCurrentVideoUrl(url);
    setVideoPlayerVisible(true);
  };

  // Close the video player
  const closeVideoPlayer = () => {
    setVideoPlayerVisible(false);
    setCurrentVideoUrl('');
  };

  // Send an image to a media screen
  const sendImageToMediaScreen = () => {
    if (!selectedScreenId || !imageUrl) return;
    
    const data = {
      mediaScreenId: selectedScreenId,
      imageUrl: imageUrl,
      mediaType: 'image',
      displayAsVideo: false
    };
    
    console.log('Sending image to media screen:', data);
    window.dispatchReactUnityEvent('SetMediaScreenImage', JSON.stringify(data));
    
    // Update our local state
    setMediaScreens(prevScreens => 
      prevScreens.map(screen => 
        screen.mediaScreenId === selectedScreenId 
          ? { ...screen, currentImageUrl: imageUrl, mediaType: 'image' } 
          : screen
      )
    );
  };

  // Send a video to a media screen
  const sendVideoToMediaScreen = () => {
    if (!selectedScreenId || !videoUrl || !thumbnailUrl) return;
    
    // First, send the video metadata
    const videoData = {
      mediaScreenId: selectedScreenId,
      imageUrl: thumbnailUrl, // We'll use the thumbnail URL as the image URL for backward compatibility
      videoUrl: videoUrl,     // This is the actual video URL that will be used for playback
      mediaType: 'video',
      displayAsVideo: true
    };
    
    console.log('Sending video metadata to media screen:', videoData);
    window.dispatchReactUnityEvent('SetMediaScreenImage', JSON.stringify(videoData));
    
    // Then, send the thumbnail
    const thumbnailData = {
      mediaScreenId: selectedScreenId,
      thumbnailUrl: thumbnailUrl,
      displayAsVideo: true
    };
    
    console.log('Sending thumbnail to media screen:', thumbnailData);
    window.dispatchReactUnityEvent('SetMediaScreenThumbnail', JSON.stringify(thumbnailData));
    
    // Update our local state
    setMediaScreens(prevScreens => 
      prevScreens.map(screen => 
        screen.mediaScreenId === selectedScreenId 
          ? { 
              ...screen, 
              currentImageUrl: thumbnailUrl, 
              videoUrl: videoUrl,
              mediaType: 'video' 
            } 
          : screen
      )
    );
  };

  // Use an example image
  const useExampleImage = (index) => {
    setImageUrl(exampleImages[index]);
    setMediaType('image');
  };

  // Use an example video
  const useExampleVideo = (index) => {
    const example = exampleVideos[index];
    setVideoUrl(example.videoUrl);
    setThumbnailUrl(example.thumbnailUrl);
    setMediaType('video');
  };

  return (
    <div className="media-screen-manager">
      <h2>Media Screen Manager</h2>
      
      {/* Media Screen Selection */}
      <div className="section">
        <h3>Select Media Screen</h3>
        <select 
          value={selectedScreenId} 
          onChange={(e) => setSelectedScreenId(e.target.value)}
        >
          <option value="">Select a screen</option>
          {mediaScreens.map(screen => (
            <option key={screen.mediaScreenId} value={screen.mediaScreenId}>
              {screen.mediaScreenId}
            </option>
          ))}
        </select>
        <p>Registered screens: {mediaScreens.length}</p>
      </div>
      
      {/* Media Type Selection */}
      <div className="section">
        <h3>Select Media Type</h3>
        <div>
          <label>
            <input 
              type="radio" 
              value="image" 
              checked={mediaType === 'image'} 
              onChange={() => setMediaType('image')} 
            />
            Image
          </label>
          <label>
            <input 
              type="radio" 
              value="video" 
              checked={mediaType === 'video'} 
              onChange={() => setMediaType('video')} 
            />
            Video
          </label>
        </div>
      </div>
      
      {/* Media URL Inputs */}
      {mediaType === 'image' ? (
        <div className="section">
          <h3>Image Settings</h3>
          <div>
            <label>Image URL:</label>
            <input 
              type="text" 
              value={imageUrl} 
              onChange={(e) => setImageUrl(e.target.value)} 
              placeholder="Enter image URL" 
            />
          </div>
          <button onClick={sendImageToMediaScreen}>Send Image</button>
          
          <div className="example-section">
            <h4>Example Images</h4>
            <div className="example-buttons">
              {exampleImages.map((url, index) => (
                <button key={index} onClick={() => useExampleImage(index)}>
                  Example {index + 1}
                </button>
              ))}
            </div>
          </div>
        </div>
      ) : (
        <div className="section">
          <h3>Video Settings</h3>
          <div>
            <label>Video URL:</label>
            <input 
              type="text" 
              value={videoUrl} 
              onChange={(e) => setVideoUrl(e.target.value)} 
              placeholder="Enter video URL" 
            />
          </div>
          <div>
            <label>Thumbnail URL:</label>
            <input 
              type="text" 
              value={thumbnailUrl} 
              onChange={(e) => setThumbnailUrl(e.target.value)} 
              placeholder="Enter thumbnail URL" 
            />
          </div>
          <button onClick={sendVideoToMediaScreen}>Send Video</button>
          
          <div className="example-section">
            <h4>Example Videos</h4>
            <div className="example-buttons">
              {exampleVideos.map((video, index) => (
                <button key={index} onClick={() => useExampleVideo(index)}>
                  Example {index + 1}
                </button>
              ))}
            </div>
          </div>
        </div>
      )}
      
      {/* Video Player */}
      {videoPlayerVisible && (
        <div className="video-player-overlay">
          <div className="video-player-container">
            <button className="close-button" onClick={closeVideoPlayer}>×</button>
            <video 
              src={currentVideoUrl} 
              controls 
              autoPlay 
              className="video-player"
            />
          </div>
        </div>
      )}
      
      {/* Media Screen List */}
      <div className="section">
        <h3>Registered Media Screens</h3>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Type</th>
              <th>Current Media</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {mediaScreens.map(screen => (
              <tr key={screen.mediaScreenId}>
                <td>{screen.mediaScreenId}</td>
                <td>{screen.mediaType || 'None'}</td>
                <td>
                  {screen.currentImageUrl ? (
                    <img 
                      src={screen.currentImageUrl} 
                      alt="Thumbnail" 
                      style={{ width: '50px', height: '30px' }} 
                    />
                  ) : 'No media'}
                </td>
                <td>
                  <button onClick={() => setSelectedScreenId(screen.mediaScreenId)}>
                    Select
                  </button>
                  {screen.mediaType === 'video' && screen.videoUrl && (
                    <button onClick={() => openVideoPlayer(screen.videoUrl)}>
                      Play
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      
      {/* Testing Functions */}
      <div className="section">
        <h3>Testing Functions</h3>
        <p>You can use these functions in the browser console:</p>
        <pre>
          {`
// Check the current settings for a media screen
window.checkMediaScreenSettings = (screenId) => {
  const screen = ${JSON.stringify(mediaScreens)}.find(s => s.mediaScreenId === screenId);
  console.log('Media Screen Settings:', screen);
  return screen;
};

// Test video playback for a media screen
window.testPlayMediaScreenVideo = (screenId) => {
  const screen = ${JSON.stringify(mediaScreens)}.find(s => s.mediaScreenId === screenId);
  if (screen && screen.videoUrl) {
    console.log('Playing video:', screen.videoUrl);
    window.dispatchEvent(new CustomEvent('PlayMediaScreenVideo', {
      detail: JSON.stringify({
        mediaScreenId: screenId,
        videoUrl: screen.videoUrl
      })
    }));
  } else {
    console.error('No video URL found for screen:', screenId);
  }
};
          `}
        </pre>
      </div>
      
      <style jsx>{`
        .media-screen-manager {
          font-family: Arial, sans-serif;
          max-width: 800px;
          margin: 0 auto;
          padding: 20px;
        }
        
        .section {
          margin-bottom: 20px;
          padding: 15px;
          border: 1px solid #ddd;
          border-radius: 5px;
        }
        
        input[type="text"] {
          width: 100%;
          padding: 8px;
          margin: 5px 0;
        }
        
        button {
          padding: 8px 12px;
          margin: 5px 5px 5px 0;
          background-color: #4CAF50;
          color: white;
          border: none;
          border-radius: 4px;
          cursor: pointer;
        }
        
        button:hover {
          background-color: #45a049;
        }
        
        table {
          width: 100%;
          border-collapse: collapse;
        }
        
        th, td {
          border: 1px solid #ddd;
          padding: 8px;
          text-align: left;
        }
        
        th {
          background-color: #f2f2f2;
        }
        
        .video-player-overlay {
          position: fixed;
          top: 0;
          left: 0;
          width: 100%;
          height: 100%;
          background-color: rgba(0, 0, 0, 0.8);
          display: flex;
          justify-content: center;
          align-items: center;
          z-index: 1000;
        }
        
        .video-player-container {
          position: relative;
          width: 80%;
          max-width: 800px;
        }
        
        .video-player {
          width: 100%;
        }
        
        .close-button {
          position: absolute;
          top: -40px;
          right: 0;
          background-color: transparent;
          color: white;
          font-size: 24px;
          border: none;
          cursor: pointer;
        }
        
        pre {
          background-color: #f5f5f5;
          padding: 10px;
          border-radius: 5px;
          overflow-x: auto;
        }
      `}</style>
    </div>
  );
};

export default MediaScreenManager; 