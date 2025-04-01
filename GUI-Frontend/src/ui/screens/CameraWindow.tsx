import React, { useEffect, useRef } from 'react';
import WebRTCStream from '../../../WebRTCStream';

const CameraWindow = () => {
  /*
  const videoRef = useRef<HTMLVideoElement>(null);
  
  useEffect(() => {
    const startCamera = async () => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
        }
      } catch (error) {
        console.error('Error accessing camera:', error);
      }
    };

    startCamera();

    // Cleanup function to stop the camera when component unmounts
    return () => {
      if (videoRef.current?.srcObject) {
        const stream = videoRef.current.srcObject as MediaStream;
        stream.getTracks().forEach((track) => track.stop());
      }
    };
  }, []);

  return (
    <div className='w-full h-full bg-gray-900 flex items-center justify-center'>
      <video
        ref={videoRef}
        autoPlay
        playsInline
        className='max-w-full max-h-full'
        style={{ width: '100%', height: '100%', objectFit: 'contain' }}
      >
        <track kind='captions' />
      </video>
      <div className='w-full h-96 bg-gray-900 flex items-center justify-center'>
        <WebRTCStream />
      </div>
    </div>
  );*/
  return (
    <div className='w-full h-full bg-gray-900 flex items-center justify-center'>
      <WebRTCStream />
    </div>
  );
};

export default CameraWindow;
