/* eslint-disable jsx-a11y/media-has-caption */
import React, { useEffect, useRef, useState } from 'react';
import Peer from 'peerjs';

const WebRTCStream = () => {
  const [peerId, setPeerId] = useState<string | null>(null); // To store the peer ID from PeerServer
  const [unmutedTracksCount, setUnmutedTracksCount] = useState(0); // Count of unmuted tracks
  const [isConnected, setIsConnected] = useState(false); // Track WebRTC connection state

  const Track0 = useRef<HTMLVideoElement | null>(null);
  const Track1 = useRef<HTMLVideoElement | null>(null);
  const Track2 = useRef<HTMLVideoElement | null>(null);
  const Track3 = useRef<HTMLVideoElement | null>(null);

  useEffect(() => {
    console.log('WebRTC Component Mounted');

    // Connect to the PeerJS server
    const peer = new Peer(undefined, {
      host: 'localhost',
      port: 9000,
      path: '/peerjs',
    });

    // Peer connection established to PeerJS server
    peer.on('open', (id) => {
      console.log(`My Peer ID is: ${id}`);
      setPeerId(id); // Store the peer ID from PeerServer
      createOfferAndSendToBackend(id); // Automatically initiate connection to backend
    });

    return () => {
      peer.destroy(); // Clean up the PeerJS connection
      console.log('[PeerJS] Peer connection closed');
    };
  }, []);

  // Function to create and send an offer to the backend (port 9001)
  const createOfferAndSendToBackend = async (peerId: string) => {
    // Create a connection to the backend and exchange SDP offer/answer
    const pc = new RTCPeerConnection();

    for (let i = 0; i < 4; i++) {
      pc.addTransceiver('video', {
        direction: 'recvonly',
        sendEncodings: [
          {
            maxBitrate: 5000000, // Set the max bitrate to 5 Mbps (5000000 bps)
            scaleResolutionDownBy: 2.0,
          },
        ],
      });
    }

    pc.ontrack = (event) => {
      if (event.track.kind === 'video') {
        const stream = new MediaStream();
        stream.addTrack(event.track);
        console.log('Track received:', event.track.kind, event.track.muted);

        // Assign first track to localVideoRef, second to remoteVideoRef
        if (!Track0.current.srcObject) {
          Track0.current.srcObject = stream;

          addMuteUnmuteEventListener(event.track, 0); // Add mute/unmute listener for Track
        } else if (!Track1.current.srcObject) {
          Track1.current.srcObject = stream;
          addMuteUnmuteEventListener(event.track, 1);
        } else if (!Track2.current.srcObject) {
          Track2.current.srcObject = stream;
          addMuteUnmuteEventListener(event.track, 2);
        } else if (!Track3.current.srcObject) {
          Track3.current.srcObject = stream;
          addMuteUnmuteEventListener(event.track, 3);
        }
      }
    };

    // Create an offer for WebRTC connection
    // Sent to the backend to establish the UDP connection.
    const offer = await pc.createOffer();

    const sdp = offer.sdp;

    // Check if H.264 codec is available
    if (sdp.indexOf('H264') === -1) {
      console.error('H.264 codec is not supported by this browser.');
      return;
    }

    // Replace VP8 (or other codecs) with H.264 in the SDP
    const modifiedSdp = sdp.replace(/m=video.*?(a=rtpmap:\d+ VP8)/g, (match, p1) => {
      return match.replace(p1, 'a=rtpmap:100 H264/90000'); // Force H.264 (payload type 100)
    });

    // Set the modified SDP offer
    offer.sdp = modifiedSdp;

    await pc.setLocalDescription(offer);

    try {
      const response = await fetch('http://localhost:9001/connect', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          sdp: pc.localDescription.sdp,
          type: pc.localDescription.type,
        }),
      });

      if (response.ok) {
        const answer = await response.json();
        await pc.setRemoteDescription(new RTCSessionDescription(answer));

        setIsConnected(true); // Set connected state
        console.log('WebRTC connection established!');
      } else {
        console.error('Failed to establish connection with backend!');
      }
    } catch (error) {
      console.error('Error during WebRTC offer/answer exchange:', error);
    }
  };

  // Function to add mute/unmute event listeners
  // On a mute event the style of the Video element is set to none, making it not visible.
  const addMuteUnmuteEventListener = (track: MediaStreamTrack, trackIndex: number) => {
    track.addEventListener('mute', () => {
      console.log(`Track ${trackIndex} muted`);
      setUnmutedTracksCount((prevCount) => Math.max(prevCount - 1, 0)); // Decrease unmuted count
      if (trackIndex == 0) Track0.current.style.display = 'none';
      else if (trackIndex == 1) Track1.current.style.display = 'none';
      else if (trackIndex == 2) Track2.current.style.display = 'none';
      else if (trackIndex == 3) Track3.current.style.display = 'none';
      console.log('Track received(muted):', track.kind, track.muted);
    });
    // On a unmute event the style of the Video element is set to inline-block, making it visible.
    track.addEventListener('unmute', () => {
      console.log(`Track ${trackIndex} unmuted`);
      setUnmutedTracksCount((prevCount) => Math.min(prevCount + 1, 4)); // Increase unmuted count
      if (trackIndex == 0) Track0.current.style.display = 'inline-block';
      else if (trackIndex == 1) Track1.current.style.display = 'inline-block';
      else if (trackIndex == 2) Track2.current.style.display = 'inline-block';
      else if (trackIndex == 3) Track3.current.style.display = 'inline-block';
      console.log('Track received (unmuted):', track.kind, track.muted);
    });
  };

  // Function to determine the grid style based on the number of unmuted tracks
  const getGridStyle = () => {
    if (unmutedTracksCount === 1) {
      return {
        gridTemplateColumns: '1fr',
        height: '100vh',
        width: '100vw',
      }; // 1 video
    }
    if (unmutedTracksCount === 2) {
      return {
        gridTemplateColumns: '1fr 1fr',
        height: '100vh',
        width: '100vw',
      }; // 2 videos side by side
    }
    if (unmutedTracksCount === 3) {
      return {
        gridTemplateColumns: '1fr 1fr',
        gridTemplateRows: '1fr 1fr',
        height: '75vh',
        width: '100vw',
      }; // 3 videos: 2 on top, 1 on bottom
    }
    if (unmutedTracksCount === 4) {
      return {
        gridTemplateColumns: '1fr 1fr',
        gridTemplateRows: '1fr 1fr',
        height: '50vh',
        width: '100vw',
      }; // 4 videos: 2x2 grid
    }
    return {};
  };

  const renderMessage = () => {
    return (
      <>
        {/* Inline style for the message container */}
        {(!isConnected || unmutedTracksCount === 0) && (
          <div
            style={{
              backgroundColor: 'white',
              padding: '20px',
              fontSize: '24px', // Larger text
              textAlign: 'center',
              borderRadius: '15px', // Rounded corners
              position: 'fixed',
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              boxShadow: '0 4px 8px rgba(0, 0, 0, 0.2)',
              zIndex: 1000, // Ensure the message is above other content
            }}
          >
            {isConnected ? 'Waiting for stream' : 'Waiting for WebRTC connection'} {/* Dots with animation */}
            <span
              className='dots'
              style={{
                display: 'inline-block',
                position: 'relative',
              }}
            ></span>
          </div>
        )}

        {/* Add keyframes for animation */}
        <style>
          {`
            @keyframes dot-animation {
              0% { content: '.'; }
              33% { content: '..'; }
              66% { content: '...'; }
              100% { content: '.'; }
            }
  
            .dots::after {
              content: '.';
              animation: dot-animation 1.5s steps(1, end) infinite;
            }
          `}
        </style>
      </>
    );
  };

  return (
    <div className='p-4'>
      {renderMessage()}
      <div
        className='grid gap-4 p-4'
        style={{
          display: 'grid',
          ...getGridStyle(),
        }}
      >
        <video ref={Track0} autoPlay playsInline style={{ objectFit: 'cover' }} />
        <video ref={Track1} autoPlay playsInline style={{ objectFit: 'cover' }} />
        <video ref={Track2} autoPlay playsInline style={{ objectFit: 'cover' }} />
        <video ref={Track3} autoPlay playsInline style={{ objectFit: 'cover' }} />
      </div>
    </div>
  );
};

export default WebRTCStream;
