import React from 'react';

export const Temperature = () => {
  const temperatureData = [
    { id: 12, x: '10%', y: '20%' }, // vifte venstre oppe
    { id: 12, x: '10%', y: '60%' }, // vifte venstre nede
    { id: 16, x: '90%', y: '20%' }, // høyre vifte oppe
    { id: 19, x: '90%', y: '60%' }, // høyre vifte nede
    { id: 10, x: '30%', y: '25%' }, // venstre midt oppe
    { id: 10, x: '72%', y: '25%' }, // høyre midt oppe
    { id: 10, x: '72%', y: '59%' }, // høyre midt nede
    { id: 11, x: '30%', y: '59%' }, // venstre midt nede
    { id: 13, x: '50%', y: '20%' }, // midt øverst
    { id: 10, x: '50%', y: '32%' }, // midt oppe
    { id: 11, x: '50%', y: '44%' }, // midt nede
    { id: 12, x: '50%', y: '56%' }, // midt nederst
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
