import React, { useEffect, useState, useMemo } from 'react';
import Switch from 'react-switch';
// eslint-disable-next-line import/no-unresolved
import { vinkler } from 'WebSocketProvider';

export const Angles = () => {
  const vinklerData = vinkler();
  const [transducerOn, setTransducerOn] = useState(false);

  // Memoize angles data - only update when values change
  const anglesDisplay = useMemo(
    () => ({
      roll: vinklerData.Roll,
      pitch: vinklerData.Stamp,
      yaw: vinklerData.Gir,
    }),
    [vinklerData.Roll, vinklerData.Stamp, vinklerData.Gir],
  );

  //Fetch Transducer status from backend when the component loads
  useEffect(() => {
    const fetchTransducerStatus = async () => {
      try {
        const response = await fetch('http://localhost:5017/api/rov/TransducerStatus');
        const data = await response.json();
        setTransducerOn(data.isOn);
      } catch (error) {
        console.error('Error fetching transducer status:', error);
      }
    };

    fetchTransducerStatus();
  }, []);

  //Handle Transducer switch toggle
  const handleTransducerToggle = async () => {
    const newStatus = !transducerOn;
    setTransducerOn(newStatus);

    try {
      await fetch('http://localhost:5017/api/rov/Transducer_Toggle', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isOn: newStatus }),
      });
    } catch (error) {
      console.error('Error updating transducer status:', error);
    }
  };

  return (
    <div className='flex flex-row gap-4 items-center justify-center lg:text-[20px] p-2 w-full h-full lg:flex-col'>
      <div className='gap-4 flex flex-col justify-center items-center lg:pb-8 pr-10 lg:pr-0'>
        <p className='flex flex-row items-center lg:text-[25px] text-[18px]'>DVL</p>
        <div className='lg:flex-col flex-row'>
          <div className='text-[15px] w-full gap-7 flex-col flex items-center justify-center lg:text-[20px]'>
            <p className='w-full lg:max-h-[70px]'>Transducer</p>
            <Switch onChange={handleTransducerToggle} checked={transducerOn} />
          </div>
        </div>
      </div>

      <div className='gap-4 flex flex-col justify-center items-center'>
        <p className='flex flex-row items-center lg:text-[25px] text-[18px]'>Angles</p>
        <div className='lg:flex-col flex-row gap-4'>
          <div className='text-[15px] w-full gap-7 flex-row flex lg:text-[18px]'>
            <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Roll</p>
            <p className='dark:text-[#4bd5ff] text-whites'>{anglesDisplay.roll}</p>
          </div>
          <div className='text-[15px] w-full gap-7 flex-row flex lg:text-[18px]'>
            <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Pitch</p>
            <p className='dark:text-[#4bd5ff] text-whites lg:text-[20px]'>{anglesDisplay.pitch}</p>
          </div>
          <div className='text-[15px] w-full gap-8 flex-row flex lg:text-[18px]'>
            <p className='max-w-[120px] h-full w-full lg:max-h-[70px]'>Yaw</p>
            <p className='dark:text-[#4bd5ff] text-whites'>{anglesDisplay.yaw}</p>
          </div>
        </div>
      </div>
    </div>
  );
};
