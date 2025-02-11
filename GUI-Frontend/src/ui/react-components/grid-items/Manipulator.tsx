import React, { useEffect, useState } from 'react';

export const Manipulator = () => {
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
    <>
      <div className=' h-full w-full overflow-auto flex flex-col'>
        <div className='h-full manipulatorConnection flex flex-row justify-between p-2 pb-4'>
          <div className=' max-w-[200px] w-full flex flex-row items-center lg:text-[25px] text-[18px]'>
            Manipulator connection
          </div>
          <img className='' src={imagePath} width={40} alt='Connection status' />
        </div>
        <div className='manipulatorImage h-full w-full pt-7 pb-7 flex justify-center items-center flex-col'>
          <img src='./assets/images/rov_from_side_arms.png' alt='Manipulator' width={400} />
        </div>
        <div className='p-2 connectionText h-full flex flex-row pt-4'>
          <p className=' text-[18px]'>Connected to: </p>
          <p className=' pl-4 text-[18px] dark:text-[#4bd5ff] text-whites '>XBOX CONTROLLER 1</p>
        </div>
      </div>
    </>
  );
};
