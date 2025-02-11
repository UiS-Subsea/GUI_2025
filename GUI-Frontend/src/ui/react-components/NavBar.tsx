import React from 'react';

import { Connection } from './navbar-items/Connection';
import { DarkModeToggle } from './navbar-items/DarkModeToggle';
import { NavigateButton } from './NavigateButton';

export const NavBar: React.FC<{ isDarkMode: boolean; toggleDarkMode: () => void }> = ({
  isDarkMode,
  toggleDarkMode,
}) => {
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
      <div className='max-w-[300px] w-full h-full'>
        <NavigateButton text='ROV DATA' route='/data' />
      </div>
      <div className='w-[110px] h-full'>
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
