import React, { useEffect, useState } from 'react';

export const Connection = () => {
  const [connection, setConnection] = useState<boolean>(true);
  const [imagePath, setImagePath] = useState('../assets/images/red.svg');

  const [lastLoggedStatus, setLastLoggedStatus] = useState<boolean | null>(null);

  const sendLogToBackend = async (isConnected: boolean) => {
    if (lastLoggedStatus === isConnected) {
      return;
    }

    console.log(`Sending log to backend: ${isConnected ? 'ROV is connected' : 'ROV is disconnected'}`);

    try {
      const response = await fetch('http://localhost:5017/api/rov/RovConnection', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isConnected }),
      });

      const result = await response.text();
      console.log(`Backend response: ${result}`);

      setLastLoggedStatus(isConnected);
    } catch (error) {
      console.error('Error sending log:', error);
    }
  };

  //   useEffect(() => {
  //     //lage kode som sjekker om det er en connection for så å bruke setConnection

  //     if (connection === true) {
  //       console.log('connected');
  //       setImagePath('../assets/images/green.svg');
  //     } else {
  //       console.log('disconnected');
  //       setImagePath('../assets/images/red.svg');
  //     }
  //   }, [connection]); // Trigger effect when `connection` changes

  useEffect(() => {
    if (lastLoggedStatus === null || lastLoggedStatus !== connection) {
      sendLogToBackend(connection);
      setLastLoggedStatus(connection);
    }

    setImagePath(connection ? '../assets/images/green.svg' : '../assets/images/red.svg');
  }, [connection]);

  return (
    <div className='w-full h-full flex flex-row justify-center items-center gap-1 '>
      ROV Connection: <img src={imagePath} width={40} alt='Connection status' />
    </div>
  );
};
