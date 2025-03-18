/* eslint-disable import/no-unresolved */
import React, { useState, useEffect } from 'react';
import { HashRouter as Router, Route, Routes } from 'react-router-dom';
import { MenuChannels } from 'src/channels/menuChannels';
import { useRendererListener } from 'src/ui/hooks';
import Menu from 'ui/components/Menu';
import Titlebar from 'ui/components/Titlebar';
import WindowControls from 'ui/components/WindowControls';
import Home from 'ui/screens/Home';
import '../ui/App.css';
import 'react-grid-layout/css/styles.css';
import 'react-resizable/css/styles.css';
import Settings from './screens/Settings';
import Data from './screens/Data';
import CameraWindow from './screens/CameraWindow';
import { WebSocketProvider } from '../../WebSocketProvider';
import SensorErrorPopup from './react-components/SensorErrorPopup';
import { WebSocketCommandProvider } from '../../WebSocketManager';

const onMenuEvent = (_: Electron.IpcRendererEvent, channel: string, ...args: any[]) => {
  electron.ipcRenderer.invoke(channel, args);
};

export default function App() {
  // Manage the dark mode state
  const [isDarkMode, setIsDarkMode] = useState<boolean>(() => {
    const savedMode = localStorage.getItem('dark-mode');
    return savedMode ? savedMode === 'true' : window.matchMedia('(prefers-color-scheme: dark)').matches;
  });

  const toggleDarkMode = () => {
    setIsDarkMode((prevMode) => {
      const newMode = !prevMode;
      localStorage.setItem('dark-mode', newMode.toString());
      return newMode;
    });
  };

  useEffect(() => {
    if (isDarkMode) {
      document.documentElement.classList.add('dark');
      document.documentElement.classList.remove('light');
    } else {
      document.documentElement.classList.add('light');
      document.documentElement.classList.remove('dark');
    }
  }, [isDarkMode]);

  useRendererListener(MenuChannels.MENU_EVENT, onMenuEvent);

  // Check if this is the camera window
  const isCameraWindow = window.location.hash.includes('/camera');

  // Render only the camera component for the camera window
  if (isCameraWindow) {
    return <CameraWindow />;
  }

  // Otherwise render the full app
  return (
    <div className='w-screen h-screen'>
      <Router>
        <WebSocketProvider>
          <WebSocketCommandProvider>
            <SensorErrorPopup />
            <Titlebar>
              {(windowState) => (
                <>
                  {__WIN32__ && (
                    <>
                      <Menu />
                      <WindowControls windowState={windowState} />
                    </>
                  )}
                </>
              )}
            </Titlebar>
            <Routes>
              <Route path='/' element={<Home isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />} />
              <Route path='/settings' element={<Settings isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />} />
              <Route path='/data' element={<Data isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />} />
            </Routes>
          </WebSocketCommandProvider>
        </WebSocketProvider>
      </Router>
    </div>
  );
}
