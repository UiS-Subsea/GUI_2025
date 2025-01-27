import React from 'react';

export const Angles = () => {
  const data = {
    roll: 1.72,
    pitch: 2.23,
    yaw: 3.34,
  };
  return (
    <>
      <p className='lg:text-[25px] text-[18px]'>Angles</p>
      <div className=' flex flex-col gap-4 items-center lg:text-[20px] p-2 w-full h-full '>
        <div className='lg:flex-col flex-row'>
          <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[20px]'>
            <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Roll </p>
            <p className='dark:text-[#4bd5ff] text-whites'>{data.pitch}</p>
          </div>
          <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[20px]'>
            <p className='  max-w-[120px] h-full w-full lg:max-h-[70px]'>Pitch </p>
            <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{data.roll}</p>
          </div>
          <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[20px]'>
            <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Yaw </p>
            <p className='dark:text-[#4bd5ff] text-whites'>{data.yaw}</p>
          </div>
        </div>
        <div className='border'>Bilde</div>
      </div>
    </>
  );
};
