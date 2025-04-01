// SensorErrorContext.tsx
import React, { createContext, useContext, useEffect, useState } from 'react';
import { sensorerror } from '../../../WebSocketProvider';
import '../styles/SensorErrorPopup.css';

interface SensorErrorContextType {
  visibleErrors: string[];
  clearErrors: () => void;
  showPopup: () => void;
}

const SensorErrorContext = createContext<SensorErrorContextType>({
  visibleErrors: [],
  clearErrors: () => {},
  showPopup: () => {},
});

// hook to use this context outside of the component
export const useSensorError = () => useContext(SensorErrorContext);

// Component to display the error popup
const ErrorPopup: React.FC<{
  errors: string[];
  onClose: () => void;
  isVisible: boolean;
  onVisibilityChange: (visible: boolean) => void;
}> = ({ errors, onClose, isVisible, onVisibilityChange }) => {
  if (!isVisible) return null;

  return (
    <div className='sensor-error-popup-overlay'>
      <div className='sensor-error-popup font-silkscreen'>
        <h1 className='text-lg font-bold'>Sensor Errors:</h1>
        <ul>
          {errors.length > 0 ? errors.map((error, index) => <li key={index}>{error}</li>) : <li>No active alarms</li>}
        </ul>
        <button
          onClick={() => {
            onVisibilityChange(false);
            onClose();
          }}
        >
          Acknowledge
        </button>
      </div>
    </div>
  );
};

export const SensorErrorProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const sensorErrors = sensorerror();
  const [visibleErrors, setVisibleErrors] = useState<string[]>([]);
  const [isPopupVisible, setIsPopupVisible] = useState(false);

  useEffect(() => {
    const newErrors: string[] = [];
    if (sensorErrors?.IMU_Errors) newErrors.push(`IMU Error: ${sensorErrors.IMU_Errors}`);
    if (sensorErrors?.Temp_Errors) newErrors.push(`Temperature Error: ${sensorErrors.Temp_Errors}`);
    if (sensorErrors?.PRESSURE_Errors) newErrors.push(`Pressure Error: ${sensorErrors.PRESSURE_Errors}`);
    if (sensorErrors?.Leak_Errors) newErrors.push(`Leak Error: ${sensorErrors.Leak_Errors}`);
    if (newErrors.length > 0) {
      setVisibleErrors(newErrors);
    }
  }, [sensorErrors]);

  const clearErrors = () => {
    setVisibleErrors([]);
  };

  const showPopup = () => {
    setIsPopupVisible(true);
  };

  return (
    <SensorErrorContext.Provider value={{ visibleErrors, clearErrors, showPopup }}>
      {children}
      <ErrorPopup
        errors={visibleErrors}
        onClose={() => {}}
        isVisible={isPopupVisible}
        onVisibilityChange={setIsPopupVisible}
      />
    </SensorErrorContext.Provider>
  );
};
