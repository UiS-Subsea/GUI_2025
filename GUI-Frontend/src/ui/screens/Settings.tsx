import React from 'react';

import 'react-grid-layout/css/styles.css';
import 'react-resizable/css/styles.css';
import { NavigateButton } from '../react-components/NavigateButton';
import { DarkModeToggle } from '../react-components/navbar-items/DarkModeToggle';

export const Settings: React.FC<{ isDarkMode: boolean; toggleDarkMode: () => void }> = ({
  isDarkMode,
  toggleDarkMode,
}) => {
  return (
    <>
      <div className='w-screen h-screen bg-lightBg dark:bg-darkBg'>
        <div className='w-screen h-[70px] pt-2 flex flex-row gap-2 font-silkscreen bg-lightBg dark:bg-darkBg  justify-center items-center transition-colors duration-300'>
          <div className='rounded-md sm:w-[30px] max-w-[80px] md:w-full h-full items-center justify-center flex'>
            {isDarkMode ? (
              <img src='./assets/images/logo.png' width={60} alt='dark logo' />
            ) : (
              <img src='/assets/images/logoDark.png' width={60} alt='light logo' />
            )}
          </div>

          <div className='h-full border-2 min-w-[300px] flex justify-center items-center rounded-md  border-black dark:border-white text-lightText dark:text-darkText w-full text-[30px]'>
            Settings
          </div>

          <div className='w-[110px] h-full'>
            <NavigateButton
              text=''
              route='/'
              image={
                isDarkMode ? (
                  <img className='w-9 h-9' src='/assets/images/BackDark.svg' alt='settings' width={40} />
                ) : (
                  <img className='w-9 h-9' src='/assets/images/BackLight.svg' alt='settings' width={40} />
                )
              }
            />
          </div>

          <div className='flex justify-center items-center'>
            <DarkModeToggle isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />
          </div>
        </div>
        <div className='bg-[]'></div>
      </div>
    </>
  );
};

export default Settings;
