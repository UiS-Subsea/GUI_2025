import React from 'react';
import { useSensorError } from './SensorErrorPopup';
import { motion } from 'framer-motion';

import { Connection } from './navbar-items/Connection';
import { DarkModeToggle } from './navbar-items/DarkModeToggle';
import { NavigateButton } from './NavigateButton';

export const NavBar: React.FC<{ isDarkMode: boolean; toggleDarkMode: () => void }> = ({
  isDarkMode,
  toggleDarkMode,
}) => {
  const { visibleErrors, showPopup } = useSensorError();

  const handleAlarmClick = () => {
    showPopup();
    if (visibleErrors.length < 0) {
      showPopup();
    }
  };

  return (
    <div className='w-screen h-[70px] pt-2 flex flex-row gap-2 font-silkscreen bg-lightBg dark:bg-darkBg  justify-center items-center transition-colors duration-300'>
      <div className='rounded-md sm:w-[30px] max-w-[80px] md:w-full h-full items-center justify-center flex'>
        {isDarkMode ? (
          <img src='./assets/images/logo.png' width={60} alt='dark logo' />
        ) : (
          <img src='/assets/images/logoDark.png' width={60} alt='light logo' />
        )}
      </div>
      <div className='rounded-md  border-2 border-black dark:border-white text-lightText dark:text-darkText max-w-[300px] min-w-[250px] w-full h-full font-silkscreen sm:text-[10px] md:text-[20px] px-1'>
        <Connection />
      </div>
      <div className='h-full border-2 min-w-[300px] flex justify-center items-center rounded-md  border-black dark:border-white text-lightText dark:text-darkText w-full text-[30px]'>
        ROV DASHBOARD
      </div>
      <div className='max-w-[300px] w-[110PX] h-full'>
        <NavigateButton text='LOGS' route='/data' />
      </div>
      <button
        className='text-[20px] bg-lightBg flex justify-center items-center h-full w-[110px] dark:bg-darkBg text-lightText dark:text-darkText font-silkscreen border-2 border-black dark:border-white rounded-md px-4 py-2 cursor-pointer hover:opacity-80 transition-opacity'
        onClick={handleAlarmClick}
        onKeyDown={(e) => e.key === 'Enter' && handleAlarmClick()}
      >
        {visibleErrors.length > 0 ? (
          <motion.img
            src='/assets/images/warningRed.svg'
            alt='alarm'
            width={40}
            animate={{
              scale: [1, 1.2, 1],
              opacity: [1, 0.8, 1],
            }}
            transition={{
              duration: 1.5,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        ) : (
          <img src='/assets/images/warning.svg' alt='alarm' width={40} />
        )}
      </button>
      <div className=' min-w-[90px] w-[110px] h-full'>
        <NavigateButton
          text=''
          route='/settings'
          image={
            isDarkMode ? (
              <img className='w-9 h-9' src='/assets/images/settings.svg' alt='settings' width={40} />
            ) : (
              <img className='w-9 h-9' src='/assets/images/settingsBlack.svg' alt='settings' width={40} />
            )
          }
        />
      </div>

      <div className='flex justify-center items-center'>
        <DarkModeToggle isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />
      </div>
    </div>
  );
};
