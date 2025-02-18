import React from 'react';
import { useSensorData } from '../../../../WebSocketProvider'; // Import the custom hook to access WebSocket data

export const Sensor = () => {
  const data = {
    pressure: 0,
    Depth: 0,
    FloorDistance: 0,
    Watertemperature: 0,
  };
  const sensorData = useSensorData(); // Get the sensor data from the context

  // Destructure the sensor data for easier access
  const { temperature, humidity, pressure } = sensorData;

  return (
    <div className='flex flex-col gap-4 justify-center items-center lg:text-[20px] p-2'>
      <p className='lg:text-[20px] text-[18px]'>Sensor</p>
      <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[20px]'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Pressure </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{data.pressure}</p>
      </div>
      <div className=' text-[15px] w-full h-full gap-8 flex-row flex lg:text-[20px]'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Depth </p>
        <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{sensorData.TEMPDYBDE.Depth}</p>
      </div>
      <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[20px]'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Floor distance </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{data.FloorDistance}</p>
      </div>
      <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[20px]'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Water temp </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{data.Watertemperature}</p>
      </div>
    </div>
  );
};
