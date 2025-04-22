import React, { useContext, useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useDriveMode } from '../../contexts/DriveModeContext';
import { WebSocketContext } from '../../../../WebSocketProvider';

export const Grid1 = () => {
  // retrieve value from websocket when this is implemented
  const valueX = 0;
  const valueY = 2;
  const valueZ = 4;
  const valuePSI = 6;

  //steps value for the trajectory tracking
  const [stepValue, setStepValue] = useState<number>(0);
  const [speedValue, setSpeedValue] = useState<number>(0.1);

  // Waypoint tracking values
  const [waypointValues, setWaypointValues] = useState({
    x: 0,
    y: 0,
    z: 0,
    psi: 0,
  });

  const [activeMode, setActiveMode] = useState<'trajectory' | 'waypoint'>('trajectory');
  const { isTrackingMode } = useDriveMode();
  const { sendMessage } = useContext(WebSocketContext);

  useEffect(() => {
    if (!isTrackingMode) return;

    if (activeMode === 'trajectory') {
      sendMessage({ Mode: 'AUTO' });
      sendMessage({ reg_mode: [2] });
    } else if (activeMode === 'waypoint') {
      sendMessage({ Mode: 'AUTO' });
      sendMessage({ reg_mode: [1] });
    }
  }, [activeMode, isTrackingMode]);

  const handleStepSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!isTrackingMode) return;

    console.log('Step value submitted:', stepValue);
    console.log('Speed value submitted:', speedValue);
    sendMessage({ reg_mode_setting: [0, 0, 0, 0, stepValue, speedValue] });
  };

  const handleWaypointSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!isTrackingMode) return;

    console.log('Waypoint values submitted:', waypointValues);
    sendMessage({
      reg_mode_setting: [waypointValues['x'], waypointValues['y'], waypointValues['z'], waypointValues['psi'], 0, 0],
    });
  };

  const handleWaypointChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!isTrackingMode) return;

    const { name, value } = e.target;
    setWaypointValues((prev) => ({
      ...prev,
      [name]: parseFloat(value),
    }));
  };

  const handleStepChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    if (!isTrackingMode) return;
    setStepValue(Number(e.target.value));
  };

  const handleSpeedChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    if (!isTrackingMode) return;
    setSpeedValue(Number(e.target.value));
  };

  return (
    <>
      <div className={`h-full w-full flex flex-col ${!isTrackingMode ? 'opacity-50 pointer-events-none' : ''}`}>
        <div className='flex flex-col h-full w-full items-center justify-center text-[20px]'>
          <div className='w-full flex justify-center gap-4 mb-4'>
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={() => setActiveMode('trajectory')}
              className={`px-4 py-2 rounded-md border-2 ${
                activeMode === 'trajectory'
                  ? 'bg-[#4bd5ff] text-white border-white'
                  : 'bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300 border-gray-300 dark:border-gray-600'
              }`}
            >
              Trajectory Tracking
            </motion.button>
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={() => setActiveMode('waypoint')}
              className={`px-4 py-2 rounded-md border-2 ${
                activeMode === 'waypoint'
                  ? 'bg-[#4bd5ff] text-white border-white'
                  : 'bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300 border-gray-300 dark:border-gray-600'
              }`}
            >
              Waypoint Tracking
            </motion.button>
          </div>

          {activeMode === 'trajectory' ? (
            <div className='w-full h-[70%] flex flex-col border-t'>
              <h3>Trajectory Tracking</h3>
              <form className='flex flex-col h-full justify-center items-center' onSubmit={handleStepSubmit}>
                <div className='flex flex-row w-full justify-center items-center space-x-5 mb-4'>
                  <label htmlFor='stepSelect' className='text-sm font-medium'>
                    Trajectory:
                  </label>
                  <select
                    id='stepSelect'
                    value={stepValue}
                    onChange={handleStepChange}
                    className='border px-2 py-1 rounded'
                    disabled={!isTrackingMode}
                  >
                    <option value={0}>0</option>
                    <option value={1}>1</option>
                    <option value={2}>2</option>
                  </select>

                  <label htmlFor='speedSelect' className='text-sm font-medium'>
                    Speed:
                  </label>
                  <select
                    id='speedSelect'
                    value={speedValue}
                    onChange={handleSpeedChange}
                    className='border px-2 py-1 rounded'
                    disabled={!isTrackingMode}
                  >
                    {[...Array(10)].map((_, i) => {
                      const val = (i + 1) / 10;
                      return (
                        <option key={val} value={val}>
                          {val}
                        </option>
                      );
                    })}
                  </select>
                </div>
                <motion.button
                  whileHover={{ scale: isTrackingMode ? 1.1 : 1, transition: { duration: 0.2 } }}
                  whileTap={{ scale: isTrackingMode ? 0.9 : 1 }}
                  type='submit'
                  className={`underline text-[16px] ${isTrackingMode ? 'text-[#4bd5ff]' : 'text-gray-400'}`}
                  disabled={!isTrackingMode}
                >
                  Send values
                </motion.button>
              </form>
            </div>
          ) : (
            <div className='w-full h-[70%] flex flex-col border-t'>
              <h3>Waypoint Tracking</h3>
              <form className='flex flex-col h-full justify-center items-center' onSubmit={handleWaypointSubmit}>
                <div className='flex flex-row w-full justify-center items-center'>
                  <h3 className='text-[18px]'>input:</h3>
                  <input
                    type='number'
                    name='x'
                    value={waypointValues.x}
                    onChange={handleWaypointChange}
                    className='w-[70px] m-2 p-2 text-center rounded-lg text-black text-[15px]'
                    placeholder='X'
                    disabled={!isTrackingMode}
                  />
                  <input
                    type='number'
                    name='y'
                    value={waypointValues.y}
                    onChange={handleWaypointChange}
                    className='w-[70px] text-[15px] m-2 p-2 text-center rounded-lg text-black'
                    placeholder='Y'
                    disabled={!isTrackingMode}
                  />
                  <input
                    type='number'
                    name='z'
                    value={waypointValues.z}
                    onChange={handleWaypointChange}
                    className='w-[70px] m-2 p-2 text-center text-[15px] rounded-lg text-black'
                    placeholder='Z'
                    disabled={!isTrackingMode}
                  />
                  <input
                    type='number'
                    name='psi'
                    value={waypointValues.psi}
                    onChange={handleWaypointChange}
                    className='w-[70px] text-[15px] m-2 p-2 text-center rounded-lg text-black'
                    placeholder='PSI'
                    disabled={!isTrackingMode}
                  />
                </div>
                <motion.button
                  whileHover={{
                    scale: isTrackingMode ? 1.1 : 1,
                    transition: { duration: 0.2 },
                  }}
                  whileTap={{ scale: isTrackingMode ? 0.9 : 1 }}
                  type='submit'
                  className={`underline text-[16px] ${isTrackingMode ? 'text-[#4bd5ff]' : 'text-gray-400'}`}
                  disabled={!isTrackingMode}
                >
                  Send values
                </motion.button>
              </form>
              <div className='text-[18px] flex flex-col h-full w-fullp-2'>
                <h3>Current waypoint values :</h3>
                <div className='flex h-full flex-row w-full justify-center items-center space-x-5 '>
                  <p className='border-white border-2 rounded-lg p-2 flex text-[#4bd5ff]'>X : {valueX}</p>
                  <p className='border-white border-2 rounded-lg p-2 flex text-[#4bd5ff]'>Y : {valueY}</p>
                  <p className='border-white border-2 rounded-lg p-2 flex text-[#4bd5ff]'>Z : {valueZ}</p>
                  <p className='border-white border-2 rounded-lg p-2 flex text-[#4bd5ff]'>PSI : {valuePSI}</p>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
};
