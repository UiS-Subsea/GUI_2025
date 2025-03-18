import React, { useState } from 'react';
import { Button } from '../Button';
import { useWebSocketCommand } from '../../../../WebSocketManager'; // new line

export const DriveMode = () => {
  const ws = useWebSocketCommand(); // Get WebSocketManager instance

  const sendManualCommand = () => {
    if (ws) {
      ws.sendCommand('START_MANUAL'); // Send "manual" command
    } else {
      console.log('WebSocket is not connected.');
    }
  };

  const sendDockingCommand = () => {
    if (ws) {
      ws.sendCommand('START_DOCKING'); // Send "docking" command
    } else {
      console.log('WebSocket is not connected.');
    }
  };

  const sendTransectCommand = () => {
    if (ws) {
      ws.sendCommand('START_TRANSECT'); // Send "docking" command
    } else {
      console.log('WebSocket is not connected.');
    }
  };

  return (
    <>
      <p className='w-full text-center text-[18px] lg:text-[25px] p-2'>Drive Mode</p>
      <div className='w-full flex flex-col gap-4 justify-center items-center p-4 text-[18px]'>
        <div className='gap-4 flex flex-col min-w-[70px] w-full'>
          <Button name='Manual' action={sendManualCommand} />
          <Button name='Cable' action={sendTransectCommand} />
          <Button name='Structure' />
          <Button name='Docking' action={sendDockingCommand} />
        </div>
      </div>
    </>
  );
};
