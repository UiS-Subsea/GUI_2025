import React from 'react';

export const Battery = () => {
  const batteryData = {
    batteryPercentage: 56,
    voltage: 12.5,
    current: 2.5,
  };
  const watt = batteryData.voltage * batteryData.current;

  return (
    <div className='flex justify-center items-center flex-col lg:text-[25px]  text-[18px] gap-4 '>
      <p className='lg:text-[25px] text-[18px]'>Battery</p>
      <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[20px]'>
        <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Percentage </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{batteryData.batteryPercentage}</p>
      </div>
      <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[20px]'>
        <p className='  max-w-[120px] h-full w-full lg:max-h-[70px]'>Current </p>
        <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{batteryData.current}</p>
      </div>
      <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[20px]'>
        <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Voltage </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{batteryData.voltage}</p>
      </div>
      <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[20px]'>
        <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Watt </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{watt}</p>
      </div>
      <div className='lg:block hidden'>
        <div className='w-full h-full justify-center items-center flex p-2 overflow-y-hidden'>
          {batteryData.batteryPercentage < 20 ? (
            <img className='max-w-[120px] w-full' src='./assets/images/BatteryLOW.svg' alt='Battery low' />
          ) : batteryData.batteryPercentage < 61 ? (
            <img className='max-w-[120px] w-full' src='./assets/images/BatteryMiddle.svg' alt='Battery medium' />
          ) : (
            <img className='max-w-[120px] w-full' src='./assets/images/BatteryFull.svg' alt='Battery full' />
          )}
        </div>
      </div>
    </div>
  );
};
