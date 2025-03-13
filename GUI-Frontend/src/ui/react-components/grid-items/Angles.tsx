import React from 'react';
import Switch from 'react-switch';
// eslint-disable-next-line import/no-unresolved
import { vinkler } from 'WebSocketProvider';

export const Angles = () => {
  const vinklerData = vinkler();

  const [transduserOn, setTransduserOn] = React.useState(false);
  const handleTransduser = () => setTransduserOn(!transduserOn);

  return (
    <>
      <div className=' flex flex-row gap-4 items-center justify-center lg:text-[20px] p-2 w-full h-full lg:flex-col '>
        <div className='gap-4 flex flex-col justify-center items-center lg:pb-8 pr-10 lg:pr-0'>
          <p className='flex flex-row items-center lg:text-[25px] text-[18px]'>DVL</p>
          <div className='lg:flex-col flex-row'>
            <div className=' text-[15px] w-full gap-7 flex-col flex items-center justify-center lg:text-[20px]'>
              <p className='w-full lg:max-h-[70px]'>Transduser</p>
              <Switch onChange={handleTransduser} checked={transduserOn} />
            </div>
          </div>
        </div>
        <div className='gap-4 flex flex-col justify-center items-center'>
          <p className='flex flex-row items-center lg:text-[25px] text-[18px]'>Angles</p>
          <div className='lg:flex-col flex-row gap-4'>
            <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[18px]'>
              <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Roll </p>
              <p className='dark:text-[#4bd5ff] text-whites'>{vinklerData.Roll}</p>
            </div>
            <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[18px]'>
              <p className='  max-w-[120px] h-full w-full lg:max-h-[70px]'>Pitch </p>
              <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{vinklerData.Stamp}</p>
            </div>
            <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[18px]'>
              <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Yaw </p>
              <p className='dark:text-[#4bd5ff] text-whites'>{vinklerData.Gir}</p>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
