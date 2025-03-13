/* eslint-disable import/no-duplicates */
import React from 'react';
import { useSensorData } from '../../../../WebSocketProvider'; // Import the custom hook to access WebSocket data
import { tempdybde } from '../../../../WebSocketProvider';

export const Sensor = () => {
  const sensorData = tempdybde(); // Get the sensor data from the context
  console.log('test', sensorData);

  // Constants
  const P0 = 101325; // Atmospheric pressure at surface in Pascals
  const g = 9.81; // Gravity
  const rho = 1000; // Default water density in kg/m³ (adjust if you have more accurate values)

  // Calculate pressure in Pascals
  const pressurePa = P0 + rho * g * sensorData.Depth;

  // Convert to bar or other units
  const pressureBar = (pressurePa / 100000).toFixed(3); // 1 bar = 100,000 Pa

  return (
    <div className='flex flex-col gap-4 justify-center items-center lg:text-[25px] p-2'>
      <p className='lg:text-[25px] text-[18px]'>Sensor</p>
      <div className=' text-[15px] w-full gap-7 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Pressure </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{pressureBar}</p>
      </div>
      <div className=' text-[15px] w-full h-full gap-8 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Depth </p>
        <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{sensorData.Depth}</p>
      </div>
      <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Sensor Temp </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{sensorData.Sensor_temp}</p>
      </div>
      <div className=' text-[15px] w-full gap-8 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Water temp </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{sensorData.Water_temp}</p>
      </div>
    </div>
  );
};
