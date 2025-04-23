/* eslint-disable import/no-duplicates */
import React, { useMemo } from 'react';
import { useSensorData } from '../../../../WebSocketProvider'; // Import the custom hook to access WebSocket data
import { tempdybde } from '../../../../WebSocketProvider';

export const Sensor = () => {
  const sensorData = tempdybde(); // Get the sensor data from the context
  console.log('test', sensorData);

  // Constants that don't change
  const P0 = 101325; // Atmospheric pressure at surface in Pascals
  const g = 9.81; // Gravity
  const rho = 1000; // Default water density in kg/m³

  // Memoize pressure calculations - only recalculate when depth changes
  const { pressurePa, pressureBar } = useMemo(() => {
    const pa = P0 + rho * g * sensorData.Depth;
    const bar = (pa / 100000).toFixed(3);
    return { pressurePa: pa, pressureBar: bar };
  }, [sensorData.Depth]);

  // Memoize sensor data display - only update when values change
  const sensorDisplay = useMemo(
    () => ({
      depth: sensorData.Depth,
      sensorTemp: sensorData.Sensor_temp,
      waterTemp: sensorData.Water_temp,
    }),
    [sensorData.Depth, sensorData.Sensor_temp, sensorData.Water_temp],
  );

  return (
    <div className='flex flex-col gap-4 justify-center items-center lg:text-[25px] p-2'>
      <p className='lg:text-[25px] text-[18px]'>Sensor</p>
      <div className='text-[15px] w-full gap-7 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Pressure </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{pressureBar}</p>
      </div>
      <div className='text-[15px] w-full h-full gap-8 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Depth </p>
        <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{sensorDisplay.depth}</p>
      </div>
      <div className='text-[15px] w-full gap-8 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Sensor Temp </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{sensorDisplay.sensorTemp}</p>
      </div>
      <div className='text-[15px] w-full gap-8 flex-row flex lg:text-[18px] items-center justify-center'>
        <p className='max-w-[140px] h-full w-full lg:max-h-[70px]'>Water temp </p>
        <p className='dark:text-[#4bd5ff] text-whites'>{sensorDisplay.waterTemp}</p>
      </div>
    </div>
  );
};
