import React, { useState } from 'react';
import { Button } from '../Button';

export const DriveMode = () => {
  const [selectedMode, setSelectedMode] = useState('Manual');

  const sendDriveModeCommand = async (mode: string) => {
    try {
      const response = await fetch('http://localhost:5017/api/rov/DriveMode', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mode }),
      });
      const result = await response.text();
      console.log(`Drive mode changed to ${mode}:`, result);
    } catch (error) {
      console.error('Error sending drive mode command:', error);
    }
  };

  const handleModeChange = (mode: string) => {
    if (mode !== selectedMode) {
      setSelectedMode(mode);
      sendDriveModeCommand(mode);
    }
  };

  return (
    <>
      <p className='w-full text-center text-[18px] lg:text-[25px] p-2'>Drive Mode</p>
      <div className='w-full flex flex-col gap-4 justify-center items-center p-4 text-[18px]'>
        <div className='gap-4 flex flex-col min-w-[70px] w-full'>
          <Button name='Manual' action={() => handleModeChange('Manual')} selected={selectedMode === 'Manual'} />
          <Button name='Cable' action={() => handleModeChange('Cable')} selected={selectedMode === 'Cable'} />
          <Button
            name='Structure'
            action={() => handleModeChange('Structure')}
            selected={selectedMode === 'Structure'}
          />
          <Button name='Docking' action={() => handleModeChange('Docking')} selected={selectedMode === 'Docking'} />
        </div>
      </div>
    </>
  );
};
