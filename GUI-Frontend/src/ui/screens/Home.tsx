import React from 'react';

import { NavBar } from '../react-components/NavBar';
import { DashboardGrid } from '../react-components/MainGrid';
import 'react-grid-layout/css/styles.css';
import 'react-resizable/css/styles.css';

export const Home: React.FC<{ isDarkMode: boolean; toggleDarkMode: () => void }> = ({ isDarkMode, toggleDarkMode }) => {
  return (
    <div className='home w-full h-full bg-lightBg dark:bg-darkBg transition-colors duration-300 flex flex-col '>
      <div className=' flex w-full top-1 pt-2 px-2 '>
        <NavBar isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />
      </div>
      <div className=' flex w-full h-full overflow-auto pb-6'>
        <DashboardGrid />
      </div>
    </div>
  );
};

export default Home;
