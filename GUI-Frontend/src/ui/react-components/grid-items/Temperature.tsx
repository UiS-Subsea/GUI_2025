import React from 'react';
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
    { id: temperatureDataBackend.COMTEMP.Com_temp, x: '50%', y: '15%' }, // Com temp
    { id: temperatureDataBackend.REGTEMP.REG_temp, x: '50%', y: '27%' }, // Regulator temp
    { id: temperatureDataBackend.REGTEMP.Motor_temp, x: '50%', y: '39%' }, // Motor temp
    { id: temperatureDataBackend.TEMPDYBDE.Sensor_temp, x: '50%', y: '51%' }, // sensor temp
    { id: temperatureDataBackend.DATA12VRIGHT.Temp, x: '50%', y: '63%' }, // 12V right kort temp
    { id: temperatureDataBackend.DATA12VLEFT.Temp, x: '50%', y: '75%' }, // 12V left kort temp
    { id: temperatureDataBackend.DATA5V.Power_temp, x: '50%', y: '87%' }, // 5V power temp
  ];

  return (
    <>
      <div className='  h-full flex flex-col lg:w-full w-[200px] text-[18px] max-w-[320px] justify-center items-center '>
        <div className='absolute top-2 left-4 lg:text-[25px] text-[18px]'>Temperature</div>
        <div style={{ position: 'relative', width: '100%', textAlign: 'center' }}>
          <img src='./assets/images/rov.png' alt='ROV' style={{ width: 350, height: 'auto', display: 'block' }} />
          {temperatureData.map((item) => (
            <div
              key={item.id}
              style={{
                position: 'absolute',
                top: item.y,
                left: item.x,
                transform: 'translate(-50%, -50%)',
                background: 'rgba(0, 0, 0, 0.5)',
                color: 'white',
                padding: '5px 10px',
                borderRadius: '10px',
                border: '2px solid #4bd5ff',
                fontSize: '14px',
                fontWeight: 'bold',
                textAlign: 'center',
                minWidth: '30px',
                textDecorationColor: 'white',
                WebkitTextDecorationColor: 'black',
                textShadow: '2px 2px 2px black',
              }}
            >
              {item.id}
            </div>
          ))}
        </div>
      </div>
    </>
  );
};
