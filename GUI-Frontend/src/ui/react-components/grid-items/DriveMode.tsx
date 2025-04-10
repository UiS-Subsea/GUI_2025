import React, { useContext, useState } from 'react';
import { Button } from '../../components/Button';
import { useDriveMode } from '../../contexts/DriveModeContext';
import { useWebSocketCommand } from '../../../../WebSocketManager';
import { WebSocketContext } from '../../../../WebSocketProvider';

export const DriveMode = () => {
  const [selectedMode, setSelectedMode] = useState('');
  const { isTrackingMode, toggleTrackingMode } = useDriveMode();
  const ws = useWebSocketCommand(); // Get WebSocketManager instance
  const { sendMessage } = useContext(WebSocketContext);

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

  const sendManualCommand = () => {
    if (ws) {
      ws.sendCommand('START_MANUAL'); // Send "manual" command
      sendMessage({ Mode: 'MANUAL' }); // Send mode message to .NET backend
    } else {
      console.log('WebSocket is not connected.');
    }
  };
  const sendDockingCommand = () => {
    if (ws) {
      ws.sendCommand('START_DOCKING');
      sendMessage({ Mode: 'AUTO' });
    } else {
      console.log('WebSocket is not connected.');
    }
  };
  const sendTransectCommand = () => {
    if (ws) {
      ws.sendCommand('START_TRANSECT');
      sendMessage({ Mode: 'AUTO' });
    } else {
      console.log('WebSocket is not connected.');
    }
  };

  const handleModeChange = (mode: string) => {
    if (mode !== selectedMode) {
      setSelectedMode(mode);
      sendDriveModeCommand(mode);

      // If switching to Manual mode, enable manual controls
      if (mode === 'Automatic' && !isTrackingMode) {
        toggleTrackingMode();
      }
      // If switching away from Manual mode, disable manual controls
      else if (mode !== 'Automatic' && isTrackingMode) {
        toggleTrackingMode();
      }
    }
  };

  return (
    <>
      <p className='w-full text-center text-[18px] lg:text-[25px] p-2'>Drive Mode</p>
      <div className='w-full flex flex-col gap-4 justify-center items-center p-4 text-[18px]'>
        <div className='gap-4 flex flex-col min-w-[70px] w-full'>
          <Button
            name='Manual'
            action={() => {
              handleModeChange('Manual');
              sendManualCommand();
            }}
            selected={selectedMode === 'Manual'}
          />
          <Button
            name={isTrackingMode ? 'Automatic (Active' : 'Automatic'}
            action={() => handleModeChange('Automatic')}
            selected={selectedMode === 'Automatic'}
          />
          <Button
            name='Transect'
            action={() => {
              handleModeChange('Transect');
              sendTransectCommand();
            }}
            selected={selectedMode === 'Transect'}
          />
          <Button
            name='Docking'
            action={() => {
              handleModeChange('Docking');
              sendDockingCommand();
            }}
            selected={selectedMode === 'Docking'}
          />
          <Button
            name='Autotuning'
            action={() => handleModeChange('Autotuning')}
            selected={selectedMode === 'Autotuning'}
          />
        </div>
      </div>
    </>
  );
};
