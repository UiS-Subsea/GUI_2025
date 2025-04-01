import React, { useEffect, useRef } from 'react';
import WebRTCStream from '../../../WebRTCStream';

const CameraWindow = () => {
  return (
    <div className='w-full h-full bg-gray-900 flex items-center justify-center'>
      <WebRTCStream />
    </div>
  );
};

export default CameraWindow;
