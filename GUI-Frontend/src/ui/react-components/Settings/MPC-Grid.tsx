import React, { useState } from 'react';

interface MPCRow {
  name: string;
  QX?: number;
  QY?: number;
  QZ?: number;
  QPSI?: number;
  RU?: number;
  RV?: number;
  RW?: number;
  RR?: number;
  Dt?: number;
  N?: number;
  VU?: number;
  VV?: number;
  VW?: number;
  VR?: number;
  AU?: number;
  AV?: number;
  AW?: number;
  AR?: number;
}

export const MPCGrid = () => {
  const [MPCData, setMPCData] = useState<MPCRow[]>([
    { name: 'Deviance Position Weight Q', QX: 0, QY: 0, QZ: 0, QPSI: 0 },
    { name: 'Actuation weight R', RU: 0, RV: 0, RW: 0, RR: 0 },
    { name: 'Step Horizon S', Dt: 0 },
    { name: 'Prediction Horizon', N: 0 },
    { name: 'Maximum velocity', VU: 0, VV: 0, VW: 0, VR: 0 },
    { name: 'Maximum acceleration', AU: 0, AV: 0, AW: 0, AR: 0 },
  ]);

  const handleValueChange = (rowIndex: number, param: string, value: string) => {
    const newValue = parseFloat(value) || 0;
    setMPCData((prevData) => {
      const newData = [...prevData];
      newData[rowIndex] = {
        ...newData[rowIndex],
        [param]: newValue,
      };
      return newData;
    });
  };

  const handleSendAll = () => {
    // Send all values together, even if some fields are not filled
    console.log('Sending all MPC values:', MPCData);
    // TODO: Implement the logic to send all values to the backend
  };

  return (
    <div className='w-full overflow-x-auto font-silkscreen'>
      <table className='min-w-full bg-gray-900 border border-gray-700 text-white'>
        <thead>
          <tr>
            <th className=' text-[20px] py-2 px-4 border-b border-r border-gray-700' colSpan={10}>
              MPC SETTINGS
            </th>
          </tr>
          <tr className='bg-gray-800'>
            <th className='py-2 px-4 border-b border-r border-gray-700'>Parameter</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>X</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>Y</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>Z</th>
            <th className='py-2 px-4 border-b border-r border-gray-700'>PSI</th>
            <th className='py-2 px-4 border-b border-gray-700'>Actions</th>
          </tr>
        </thead>
        <tbody>
          {MPCData.map((row, rowIndex) => {
            // Determine which parameters to display based on the row name
            const isDeviancePosition = row.name === 'Deviance Position Weight Q';
            const isActuationWeight = row.name === 'Actuation weight R';
            const isStepHorizon = row.name === 'Step Horizon S';
            const isPredictionHorizon = row.name === 'Prediction Horizon';
            const isMaxVelocity = row.name === 'Maximum velocity';
            const isMaxAcceleration = row.name === 'Maximum acceleration';

            return (
              <tr key={row.name} className={rowIndex % 2 === 0 ? 'bg-gray-900' : 'bg-gray-800'}>
                <td className='py-2 px-4 border-b border-r border-gray-700 font-medium'>{row.name}</td>

                {/* X parameter */}
                <td className='py-2 px-4 border-b border-r border-gray-700'>
                  {isDeviancePosition && (
                    <input
                      type='number'
                      value={row.QX}
                      onChange={(e) => handleValueChange(rowIndex, 'QX', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isActuationWeight && (
                    <input
                      type='number'
                      value={row.RU}
                      onChange={(e) => handleValueChange(rowIndex, 'RU', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isStepHorizon && (
                    <input
                      type='number'
                      value={row.Dt}
                      onChange={(e) => handleValueChange(rowIndex, 'Dt', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isPredictionHorizon && (
                    <input
                      type='number'
                      value={row.N}
                      onChange={(e) => handleValueChange(rowIndex, 'N', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isMaxVelocity && (
                    <input
                      type='number'
                      value={row.VU}
                      onChange={(e) => handleValueChange(rowIndex, 'VU', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isMaxAcceleration && (
                    <input
                      type='number'
                      value={row.AU}
                      onChange={(e) => handleValueChange(rowIndex, 'AU', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                </td>

                {/* Y parameter */}
                <td className='py-2 px-4 border-b border-r border-gray-700'>
                  {isDeviancePosition && (
                    <input
                      type='number'
                      value={row.QY}
                      onChange={(e) => handleValueChange(rowIndex, 'QY', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isActuationWeight && (
                    <input
                      type='number'
                      value={row.RV}
                      onChange={(e) => handleValueChange(rowIndex, 'RV', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isStepHorizon && <div className='text-center text-gray-400'>-</div>}
                  {isPredictionHorizon && <div className='text-center text-gray-400'>-</div>}
                  {isMaxVelocity && (
                    <input
                      type='number'
                      value={row.VV}
                      onChange={(e) => handleValueChange(rowIndex, 'VV', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isMaxAcceleration && (
                    <input
                      type='number'
                      value={row.AV}
                      onChange={(e) => handleValueChange(rowIndex, 'AV', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                </td>

                {/* Z parameter */}
                <td className='py-2 px-4 border-b border-r border-gray-700'>
                  {isDeviancePosition && (
                    <input
                      type='number'
                      value={row.QZ}
                      onChange={(e) => handleValueChange(rowIndex, 'QZ', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isActuationWeight && (
                    <input
                      type='number'
                      value={row.RW}
                      onChange={(e) => handleValueChange(rowIndex, 'RW', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isStepHorizon && <div className='text-center text-gray-400'>-</div>}
                  {isPredictionHorizon && <div className='text-center text-gray-400'>-</div>}
                  {isMaxVelocity && (
                    <input
                      type='number'
                      value={row.VW}
                      onChange={(e) => handleValueChange(rowIndex, 'VW', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isMaxAcceleration && (
                    <input
                      type='number'
                      value={row.AW}
                      onChange={(e) => handleValueChange(rowIndex, 'AW', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                </td>

                {/* PSI parameter */}
                <td className='py-2 px-4 border-b border-r border-gray-700'>
                  {isDeviancePosition && (
                    <input
                      type='number'
                      value={row.QPSI}
                      onChange={(e) => handleValueChange(rowIndex, 'QPSI', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isActuationWeight && (
                    <input
                      type='number'
                      value={row.RR}
                      onChange={(e) => handleValueChange(rowIndex, 'RR', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isStepHorizon && <div className='text-center text-gray-400'>-</div>}
                  {isPredictionHorizon && <div className='text-center text-gray-400'>-</div>}
                  {isMaxVelocity && (
                    <input
                      type='number'
                      value={row.VR}
                      onChange={(e) => handleValueChange(rowIndex, 'VR', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                  {isMaxAcceleration && (
                    <input
                      type='number'
                      value={row.AR}
                      onChange={(e) => handleValueChange(rowIndex, 'AR', e.target.value)}
                      className='w-full p-1 border rounded bg-gray-800 text-white border-gray-600'
                    />
                  )}
                </td>

                {/* Send All button - only show in the first row */}
                {rowIndex === 0 && (
                  <td className='py-2 px-4 border-b border-gray-700' rowSpan={MPCData.length}>
                    <button
                      onClick={handleSendAll}
                      className='px-4 py-2 bg-indigo-600 hover:bg-indigo-700 rounded text-white font-medium  flex justify-center items-center'
                    >
                      Send All Values
                    </button>
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
