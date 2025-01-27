import React, { useEffect, useState } from 'react';

export const Connection = () => {
  const [connection, setConnection] = useState(true); // State for connection status
  const [imagePath, setImagePath] = useState('../assets/images/red.svg'); // State for image path

  useEffect(() => {
    //lage kode som sjekker om det er en connection for så å bruke setConnection

    if (connection === true) {
      console.log('connected');
      setImagePath('../assets/images/green.svg');
    } else {
      console.log('disconnected');
      setImagePath('../assets/images/red.svg');
    }
  }, [connection]); // Trigger effect when `connection` changes

  return (
    <div className='w-full h-full flex flex-row justify-center items-center gap-1 '>
      Connection: <img src={imagePath} width={40} alt='Connection status' />
    </div>
  );
};
