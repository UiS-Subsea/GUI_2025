import React from 'react';
import { Button } from '../Button';

export const DriveMode = () => {
  return (
    <div>
      DriveMode
      <div className='w-full h-full flex flex-col gap-4 justify-center items-center py-4'>
        <div className='gap-4 flex flex-col min-w-[70px] w-full'>
          <Button name='Manual' />
          <Button name='Cable' />
          <Button name='Structure' />
          <Button name='Docking' />
        </div>
      </div>
    </div>
  );
};
