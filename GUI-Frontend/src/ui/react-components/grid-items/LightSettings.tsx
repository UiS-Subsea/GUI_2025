import React from 'react';

import Switch from 'react-switch';

export const LightSettings = () => {
  const [frontOn, setFrontOn] = React.useState(false);
  const handleFront = () => setFrontOn(!frontOn);
  const [backOn, setBackOn] = React.useState(false);
  const handleBack = () => setBackOn(!backOn);

  return (
    <>
      <p className='w-full text-center text-[18px] lg:text-[25px] p-2'>LightSettings</p>
      <div className='w-full flex flex-col  gap-4 justify-center items-center p-4 text-[18px]'>
        <div className=' gap-4 flex flex-row justify-center items-center min-w-[70px] w-full'>
          <p className='text-left lg:w-[180px]'>Front light </p>
          <Switch onChange={handleFront} checked={frontOn} className='text-right' />
        </div>
        <div className=' gap-4 flex flex-row justify-center items-center min-w-[70px] w-full'>
          <p className='lg:w-[180px]'>Back light </p>
          <Switch onChange={handleBack} checked={backOn} />
        </div>
      </div>
    </>
  );
};
