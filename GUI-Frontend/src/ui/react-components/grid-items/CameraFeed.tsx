import React from 'react';
import { motion } from 'framer-motion';

export const CameraFeed = () => {
  const [isLive, setIsLive] = React.useState(true);

  return (
    <div className='w-full h-full'>
      <div className='absolute top-0 right-3'>
        {isLive && <img className=' w-[80px]' src='./assets/images/live.svg' alt='Camera feed' />}
      </div>
      <motion.img className='w-full h-full object-center p-0 m-0' src='./assets/images/rovview.png' alt='Camera feed' />
    </div>
  );
};
