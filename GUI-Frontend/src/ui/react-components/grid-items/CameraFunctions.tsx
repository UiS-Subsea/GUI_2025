import React from 'react';
import { Button } from '../Button';
import CamWindowButton from '../CamWindowButton';

export const CameraFunctions = () => {
  return (
    <div className='h-full w-full flex flex-col lg:text-[25px] text-[17px] justify-center items-center'>
      <div className='w-full pt-2 justify-center flex '>Camera Functions</div>
      <div className='w-full h-full flex flex-col gap-4 justify-center items-center'>
        <div className='gap-4 flex flex-col lg:flex-row min-w-[70px] w-full'>
          <CamWindowButton />
        </div>
        <div className='gap-4 flex flex-col lg:flex-row min-w-[70px] w-full'>
          <Button name='Screenshot' />
          <Button name='Record' />
        </div>
      </div>
    </div>
  );
};
