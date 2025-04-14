import React, { useContext, useState } from 'react';
import { WebSocketContext } from '../../../../WebSocketProvider';

interface PIDValues {
  KI: number;
  KD: number;
  KP: number;
}

interface PIDRow {
  name: string;
  currentValues: PIDValues;
  setAutotuner: PIDValues;
  stepsize: number;
}

export const PIDGrid = () => {
  const [pidData, setPidData] = useState<PIDRow[]>([
    { name: 'Surge', currentValues: { KI: 0, KD: 0, KP: 0 }, setAutotuner: { KI: 0, KD: 0, KP: 0 }, stepsize: 0 },
    { name: 'Sway', currentValues: { KI: 0, KD: 0, KP: 0 }, setAutotuner: { KI: 0, KD: 0, KP: 0 }, stepsize: 0 },
    { name: 'Heave', currentValues: { KI: 0, KD: 0, KP: 0 }, setAutotuner: { KI: 0, KD: 0, KP: 0 }, stepsize: 0 },
    { name: 'Roll', currentValues: { KI: 0, KD: 0, KP: 0 }, setAutotuner: { KI: 0, KD: 0, KP: 0 }, stepsize: 0 },
    { name: 'Pitch', currentValues: { KI: 0, KD: 0, KP: 0 }, setAutotuner: { KI: 0, KD: 0, KP: 0 }, stepsize: 0 },
    { name: 'Yaw', currentValues: { KI: 0, KD: 0, KP: 0 }, setAutotuner: { KI: 0, KD: 0, KP: 0 }, stepsize: 0 },
  ]);

  const { sendMessage } = useContext(WebSocketContext);

  const handleValueChange = (
    rowIndex: number,
    field: 'currentValues' | 'setAutotuner',
    param: 'KI' | 'KD' | 'KP',
    value: string,
  ) => {
    // Allow empty string to be set as the value
    const newValue = value === '' ? '' : parseFloat(value) || 0;

    setPidData((prevData) => {
      const newData = [...prevData];
      // Use type assertion to handle the empty string case
      newData[rowIndex][field][param] = newValue as any;
      return newData;
    });
  };

  const handleStepsizeChange = (rowIndex: number, value: string) => {
    const newValue = parseFloat(value) || 0;
    setPidData((prevData) => {
      const newData = [...prevData];
      newData[rowIndex].stepsize = newValue;
      return newData;
    });
  };

  const handleStart = (rowIndex: number) => {
    sendMessage({ autotune: [1, 0, rowIndex, pidData[rowIndex].stepsize] });
    console.log('Starting with values for', pidData[rowIndex].name, pidData[rowIndex].setAutotuner);
  };

  const handleSend = (rowIndex: number, event?: React.MouseEvent<HTMLButtonElement>) => {
    // Prevent default form submission behavior to avoid page reload
    if (event) {
      event.preventDefault();
    }

    // Get the current values for the selected row
    const { KI, KD, KP } = pidData[rowIndex].currentValues;
    const scaledKI = KI * 100;
    const scaledKD = KD * 100;
    const scaledKP = KP * 100;

    const pidSettings = [rowIndex, scaledKP, scaledKI, scaledKD];

    console.log('Sending formatted data for', pidData[rowIndex].name, ':', pidSettings);

    sendMessage({ pid_settings: pidSettings });
  };

  const handleCancel = (rowIndex: number) => {
    sendMessage({ autotune: [0, 1, rowIndex, pidData[rowIndex].stepsize] });
    console.log('Canceling autotuner for', pidData[rowIndex].name, pidData[rowIndex].setAutotuner);
  };

  // Check if a row should have Set Autotuner content
  const shouldShowSetAutotuner = (rowName: string) => {
    return ['Surge', 'Sway', 'Heave', 'Yaw'].includes(rowName);
  };

  return (
    <div className='w-full overflow-x-auto font-silkscreen'>
      <table className='min-w-full bg-gray-900 border border-gray-700 text-white'>
        <thead>
          <tr>
            <th className=' text-[20px] py-2 px-4 border-b border-r border-gray-600' colSpan={10}>
              PID SETTINGS
            </th>
          </tr>
          <tr className='bg-gray-800'>
            <th className='py-2 px-4 border-b border-r border-gray-700'></th>
            <th className='py-2 px-4 border-b border-r border-gray-700' colSpan={4}>
              Current Values
            </th>
            <th className='py-2 px-4 border-b border-r border-gray-700' colSpan={4}>
              Set Autotuner
            </th>
            <th className='py-2 px-4 border-b border-gray-700' colSpan={2}></th>
          </tr>
          <tr className='bg-gray-800'>
            <th className='py-2 px-4 border-b border-r border-gray-700'></th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>KP</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>KI</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>KD</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>Actions</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>KP</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>KI</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>KD</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>Stepsize</th>
            <th className='py-2 px-4 border-b border-gray-700'>Actions</th>
          </tr>
        </thead>
        <tbody>
          {pidData.map((row, rowIndex) => (
            <tr key={row.name} className={rowIndex % 2 === 0 ? 'bg-gray-900' : 'bg-gray-800'}>
              <td className='py-2 px-4 border-b border-r border-gray-700 font-medium'>{row.name}</td>

              {/* Current Values */}
              <td className='py-2 px-4 border-b border-r border-gray-700'>
                <input
                  type='number'
                  value={row.currentValues.KP}
                  onChange={(e) => handleValueChange(rowIndex, 'currentValues', 'KP', e.target.value)}
                  className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                />
              </td>
              <td className='py-2 px-4 border-b border-r border-gray-700'>
                <input
                  type='number'
                  value={row.currentValues.KI}
                  onChange={(e) => handleValueChange(rowIndex, 'currentValues', 'KI', e.target.value)}
                  className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                />
              </td>
              <td className='py-2 px-4 border-b border-r border-gray-700'>
                <input
                  type='number'
                  value={row.currentValues.KD}
                  onChange={(e) => handleValueChange(rowIndex, 'currentValues', 'KD', e.target.value)}
                  className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                />
              </td>
              <td className='py-2 px-4 border-b border-r border-gray-700'>
                <button
                  onClick={(e) => handleSend(rowIndex, e)}
                  className='px-2 py-1 bg-indigo-600 hover:bg-indigo-700 rounded text-white text-sm'
                >
                  Send Values
                </button>
              </td>

              {/* Set Autotuner */}
              {shouldShowSetAutotuner(row.name) ? (
                <>
                  <td className='py-2 px-4 border-b border-r border-gray-700'>
                    <div className='text-center'>{row.setAutotuner.KP}</div>
                  </td>
                  <td className='py-2 px-4 border-b border-r border-gray-700'>
                    <div className='text-center'>{row.setAutotuner.KI}</div>
                  </td>
                  <td className='py-2 px-4 border-b border-r border-gray-700'>
                    <div className='text-center'>{row.setAutotuner.KD}</div>
                  </td>

                  {/* Stepsize */}
                  <td className='py-2 px-4 border-b border-r border-gray-700'>
                    <input
                      type='number'
                      value={row.stepsize}
                      onChange={(e) => handleStepsizeChange(rowIndex, e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  </td>

                  {/* Actions */}
                  <td className='py-2 px-4 border-b border-gray-700'>
                    <div className='flex space-x-2'>
                      <button
                        onClick={() => handleStart(rowIndex)}
                        className='px-2 py-1 bg-indigo-600 hover:bg-indigo-700 rounded text-white text-sm'
                      >
                        Start
                      </button>
                      <button
                        onClick={() => handleCancel(rowIndex)}
                        className='px-2 py-1 bg-red-600 hover:bg-red-700 rounded text-white text-sm'
                      >
                        Cancel
                      </button>
                    </div>
                  </td>
                </>
              ) : (
                <>
                  <td className='py-2 px-4 border-b border-r border-gray-700' colSpan={5}>
                    <div className='text-center text-gray-400'>Not Available</div>
                  </td>
                </>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
