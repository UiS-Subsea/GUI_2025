import React, { createContext, useContext, useState } from 'react';

interface DriveModeContextType {
  isTrackingMode: boolean;
  toggleTrackingMode: () => void;
}

const DriveModeContext = createContext<DriveModeContextType>({
  isTrackingMode: false,
  toggleTrackingMode: () => {},
});

export const useDriveMode = () => useContext(DriveModeContext);

export const DriveModeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isTrackingMode, setIsTrackingMode] = useState(false);

  const toggleTrackingMode = () => {
    setIsTrackingMode((prev) => !prev);
  };

  return (
    <DriveModeContext.Provider value={{ isTrackingMode, toggleTrackingMode }}>{children}</DriveModeContext.Provider>
  );
};
