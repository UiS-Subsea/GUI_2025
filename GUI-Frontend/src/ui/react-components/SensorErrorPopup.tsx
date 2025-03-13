import React, { useEffect, useState } from 'react';
import { sensorerror } from '../../../WebSocketProvider';
import '../styles/SensorErrorPopup.css';

const SensorErrorPopup: React.FC = () => {
  const sensorErrors = sensorerror();
  const [visibleErrors, setVisibleErrors] = useState<string[]>([]);

  useEffect(() => {
    const newErrors: string[] = [];
    console.log('SensorErrorPopup mounted');
    console.log('Current sensorErrors:', sensorErrors);

    if (sensorErrors?.IMU_Errors) newErrors.push(`IMU Error: ${sensorErrors.IMU_Errors}`);
    if (sensorErrors?.Temp_Errors) newErrors.push(`Temperature Error: ${sensorErrors.Temp_Errors}`);
    if (sensorErrors?.PRESSURE_Errors) newErrors.push(`Pressure Error: ${sensorErrors.PRESSURE_Errors}`);
    if (sensorErrors?.Leak_Errors) newErrors.push(`Leak Error: ${sensorErrors.Leak_Errors}`);

    if (newErrors.length > 0) {
      setVisibleErrors(newErrors);
    }
  }, [sensorErrors]);

  if (visibleErrors.length === 0) return null;

  return (
    <div className='sensor-error-popup-overlay '>
      <div className='sensor-error-popup font-silkscreen border-4 border-red-600 justify-center flex flex-col items-center'>
        <h2 className='text-[18px] text-red-600 '>WARNING! Errors on ROV!</h2>
        <h2 className='text-[15px] text-red-600 underline'>Please take actions immediately!</h2>
        <ul>
          {visibleErrors.map((err, index) => (
            <li key={index}>{err}</li>
          ))}
        </ul>
        <button onClick={() => setVisibleErrors([])}>Close</button>
      </div>
    </div>
  );
};

export default SensorErrorPopup;
