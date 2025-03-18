/* eslint-disable jsx-a11y/media-has-caption */
import React, { useEffect, useRef, useState } from 'react';
import Peer from 'peerjs';

const WebRTCStream = () => {
  const [peerId, setPeerId] = useState<string | null>(null); // To store the peer ID from PeerServer
  const [videoTracks, setVideoTracks] = useState<MediaStreamTrack[]>([]);

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
            maxBitrate: 5000000, // Set the max bitrate to 1 Mbps (1000000 bps)
            scaleResolutionDownBy: 2.0,
          },
        ],
      });
    }

    pc.ontrack = (event) => {
      if (event.track.kind === 'video') {
        const stream = new MediaStream();
        stream.addTrack(event.track);

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

    console.log('Ready to send post request down under...');

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
      if (trackIndex == 0) {
        Track0.current.style.display = 'none';
      } else if (trackIndex == 1) {
        Track1.current.style.display = 'none';
      } else if (trackIndex == 2) {
        Track2.current.style.display = 'none';
      } else if (trackIndex == 3) {
        Track3.current.style.display = 'none';
      }
    });
    // On a unmute event the style of the Video element is set to inline-block, making it visible.
    track.addEventListener('unmute', () => {
      console.log(`Track ${trackIndex} unmuted`);
      if (trackIndex == 0) {
        Track0.current.style.display = 'inline-block';
      } else if (trackIndex == 1) {
        Track1.current.style.display = 'inline-block';
      } else if (trackIndex == 2) {
        Track2.current.style.display = 'inline-block';
      } else if (trackIndex == 3) {
        Track3.current.style.display = 'inline-block';
      }
    });
  };

  return (
    <div
      className='grid gap-4 p-4'
      style={{ display: 'grid', gridTemplateColumns: `repeat(${Math.min(4, videoTracks.length)}, 1fr)` }}
    >
      <div>
        <video ref={Track0} autoPlay playsInline />
        <video ref={Track1} autoPlay playsInline />
        <video ref={Track2} autoPlay playsInline />
        <video ref={Track3} autoPlay playsInline />
      </div>
    </div>
  );
};

export default WebRTCStream;
