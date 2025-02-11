import React from 'react';
import Switch from 'react-switch';

export const Angles = () => {
  const data = {
    roll: 1.72,
    pitch: 2.23,
    yaw: 3.34,
  };

  const [transduserOn, setTransduserOn] = React.useState(false);
  const handleTransduser = () => setTransduserOn(!transduserOn);

  return (
    <>
      <div className=' flex flex-col gap-4 items-center lg:text-[20px] p-2 w-full h-full '>
        <p className='flex flex-row items-center lg:text-[25px] text-[18px]'>Angles</p>
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
        <p className='flex flex-row items-center lg:text-[25px] text-[18px]'>DVL</p>
        <div className='lg:flex-col flex-row'>
          <div className=' text-[15px] w-full gap-7 flex-col flex items-center justify-center lg:text-[20px]'>
            <p className='w-full lg:max-h-[70px]'>Transduser</p>
            <Switch onChange={handleTransduser} checked={transduserOn} />
          </div>
        </div>
      </div>
    </>
  );
};
