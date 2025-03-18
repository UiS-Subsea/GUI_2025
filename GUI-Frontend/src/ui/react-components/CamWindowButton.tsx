import React from 'react';
import 'react-grid-layout/css/styles.css';
import 'react-resizable/css/styles.css';
import { motion } from 'framer-motion';
import { NavigateButton } from '../react-components/NavigateButton';
import { DarkModeToggle } from '../react-components/navbar-items/DarkModeToggle';

// Add this to let TypeScript know about the electron global
declare global {
  interface Window {
    electron: {
      openCameraWindow: () => void;
      ipcRenderer: any;
      versions: any;
    };
  }
}

export const CamWindowButton = () => {
  // Function to open camera window
  const handleOpenCameraWindow = () => {
    console.log('Opening camera window'); // Add logging to confirm function is called
    if (window.electron && window.electron.openCameraWindow) {
      window.electron.openCameraWindow();
    } else {
      console.error('Electron API not available');
    }
  };

  return (
    <>
      {/* Add the button to open camera window */}
      <motion.button
        onClick={handleOpenCameraWindow}
        whileHover={{ scale: 1.05 }}
        whileTap={{ scale: 0.9 }}
        className="flex justify-center dark:text-white text-black border-2 border-black font-bold py-2 px-4 rounded text-[18px] min-w-[70px] w-full transition-colors duration-300 text-white' dark:bg-[#2A2A2A] dark:hover:bg-[#2A2A2A] bg-white hover:bg-gray-200 font-silkscreen 
             "
      >
        Open Camera Feed
      </motion.button>
    </>
  );
};

export default CamWindowButton;
