import React, { useEffect, useState } from 'react';
// eslint-disable-next-line import/no-unresolved
import { useSensorData } from 'WebSocketProvider';

export const Temperature = () => {
  //fetch data from websocket
  const temperatureDataBackend = useSensorData();
  console.log('temp', temperatureDataBackend);

  const temperatureData = [
    { id: temperatureDataBackend.THRUSTPAADRAG.VVF, x: '10%', y: '20%' }, // vertikal venstre fram
    { id: temperatureDataBackend.THRUSTPAADRAG.VVB, x: '10%', y: '60%' }, // vertifal venstre bak
    { id: temperatureDataBackend.THRUSTPAADRAG.VHF, x: '90%', y: '20%' }, // vertikal høyre fram
    { id: temperatureDataBackend.THRUSTPAADRAG.VHB, x: '90%', y: '60%' }, // vertikal høyre bak
    { id: temperatureDataBackend.THRUSTPAADRAG.HVF, x: '30%', y: '25%' }, // horisontal venstre fram
    { id: temperatureDataBackend.THRUSTPAADRAG.HFF, x: '72%', y: '25%' }, // horisontal høyre fram
    { id: temperatureDataBackend.THRUSTPAADRAG.HHB, x: '72%', y: '59%' }, // horisontal høyre bak
    { id: temperatureDataBackend.THRUSTPAADRAG.HVB, x: '30%', y: '59%' }, // horisontal venstre bak
    { name: 'Com Temperature', id: temperatureDataBackend.COMTEMP.Com_temp, x: '50%', y: '15%' }, // Com temp
    { name: 'Regulator Temperature', id: temperatureDataBackend.REGTEMP.REG_temp, x: '50%', y: '27%' }, // Regulator temp
    { name: 'Motor Temperature', id: temperatureDataBackend.REGTEMP.Motor_temp, x: '50%', y: '39%' }, // Motor temp
    { name: 'Sensor Temperature', id: temperatureDataBackend.TEMPDYBDE.Sensor_temp, x: '50%', y: '51%' }, // sensor temp
    { name: '12V Right Card Temperature', id: temperatureDataBackend.DATA12VRIGHT.Temp, x: '50%', y: '63%' }, // 12V right kort temp
    { name: '12V Left Temperature', id: temperatureDataBackend.DATA12VLEFT.Temp, x: '50%', y: '75%' }, // 12V left kort temp
    { name: '5V Power Temperature', id: temperatureDataBackend.DATA5V.Power_temp, x: '50%', y: '87%' }, // 5V power temp
  ];

  const [lastLoggedStatus, setLastLoggedStatus] = useState<Record<string, string>>({});

  const getTemperatureStatus = (temp: number) => {
    if (temp >= 30) return 'WARNING: Too High';
    if (temp >= 20) return 'WARNING: High';
    return 'INFO: Normal';
  };

  const sendTemperatureLog = async (sensorName: string, tempValue: number) => {
    try {
      await fetch('http://localhost:5017/api/rov/TemperatureLog', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sensorName, value: tempValue }),
      });
    } catch (error) {
      console.error('Error sending temperature log:', error);
    }
  };

  useEffect(() => {
    temperatureData.forEach((item) => {
      const tempValue = Number(item.id);
      if (!isNaN(tempValue)) {
        const newStatus = getTemperatureStatus(tempValue);
        const lastStatus = lastLoggedStatus[item.name];

        if (newStatus !== lastStatus) {
          sendTemperatureLog(item.name, tempValue);
          setLastLoggedStatus((prev) => ({
            ...prev,
            [item.name]: newStatus,
          }));
        }
      }
    });
  }, [temperatureData]);

  return (
    <>
      <div className='  h-full flex flex-col lg:w-full w-[200px] text-[18px] max-w-[320px] justify-center items-center '>
        <div className='absolute top-2 left-4 lg:text-[25px] text-[18px]'>Temperature</div>
        <div style={{ position: 'relative', width: '100%', textAlign: 'center' }}>
          <img src='./assets/images/rov.png' alt='ROV' style={{ width: 350, height: 'auto', display: 'block' }} />
          {temperatureData.map((item, index) => {
            // Convert to number (handle null/undefined just in case)
            const tempValue = Number(item.id);
            let borderColor = '#4bd5ff'; // default (blue)

            if (!isNaN(tempValue)) {
              if (tempValue >= 60) {
                borderColor = 'red';
              } else if (tempValue >= 50) {
                borderColor = 'orange';
              } else if (tempValue >= 40) {
                borderColor = 'yellow';
              }
            }

            return (
              <div
                key={index}
                style={{
                  position: 'absolute',
                  top: item.y,
                  left: item.x,
                  transform: 'translate(-50%, -50%)',
                  background: 'rgba(0, 0, 0, 0.5)',
                  color: 'white',
                  padding: '5px 10px',
                  borderRadius: '10px',
                  border: `2px solid ${borderColor}`,
                  fontSize: '14px',
                  fontWeight: 'bold',
                  textAlign: 'center',
                  minWidth: '30px',
                  textDecorationColor: 'white',
                  WebkitTextDecorationColor: 'black',
                  textShadow: '2px 2px 2px black',
                }}
              >
                {tempValue}
              </div>
            );
          })}
        </div>
      </div>
    </>
  );
};
