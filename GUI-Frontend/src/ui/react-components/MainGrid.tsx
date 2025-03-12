import 'react-grid-layout/css/styles.css';
import 'react-resizable/css/styles.css';

import React from 'react';
import { Responsive, WidthProvider } from 'react-grid-layout';
import { CameraFunctions } from './grid-items/CameraFunctions';
import { CameraFeed } from './grid-items/CameraFeed';
import { Temperature } from './grid-items/Temperature';
import { Sensor } from './grid-items/Sensor';
import { Battery } from './grid-items/Battery';
import { DriveMode } from './grid-items/DriveMode';
import { LightSettings } from './grid-items/LightSettings';
import { Angles } from './grid-items/Angles';
import { Manipulator } from './grid-items/Manipulator';
import { Coordinates } from './grid-items/Coordinates';

const ResponsiveGridLayout = WidthProvider(Responsive);
export const DashboardGrid = () => {
  const layouts = {
    lg: [
      { i: 'cameraFunctions', x: 0, y: 0, w: 2, h: 2 },
      { i: 'lightSettings', x: 0, y: 2, w: 2, h: 2 },
      { i: 'driveMode', x: 0, y: 5, w: 1, h: 4.5 },
      { i: 'angles', x: 1, y: 5, w: 1, h: 4.5 },
      { i: 'temperature', x: 2, y: 5, w: 2, h: 3.5 },
      { i: 'coordinates', x: 6, y: 4, w: 2, h: 4.5 },
      { i: 'cameraFeed', x: 2, y: 0, w: 4, h: 5 },
      { i: 'sensors', x: 6, y: 0, w: 1, h: 4 },
      { i: 'battery', x: 7, y: 0, w: 1, h: 4 },
      { i: 'manipulatorConnection', x: 4, y: 5, w: 2, h: 3.5 },
    ],
    sm: [
      { i: 'cameraFunctions', x: 0, y: 0, w: 2, h: 3 },
      { i: 'lightSettings', x: 0, y: 2, w: 2, h: 2 },
      { i: 'driveMode', x: 0, y: 6, w: 2, h: 3 },
      { i: 'angles', x: 2, y: 8, w: 4, h: 2 },
      { i: 'temperature', x: 2, y: 5, w: 4, h: 2 },
      { i: 'cameraFeed', x: 2, y: 0, w: 4, h: 4 },
      { i: 'sensors', x: 6, y: 0, w: 2, h: 3 },
      { i: 'battery', x: 6, y: 4, w: 2, h: 2 },
      { i: 'manipulatorConnection', x: 6, y: 5, w: 2, h: 3 },
      { i: 'coordinates', x: 2, y: 9, w: 4, h: 4 },
    ],
  };

  const cols = { lg: 8, md: 8, sm: 8 };

  return (
    <div className='w-full h-full overflow-auto'>
      <style>
        {`
          .react-grid-placeholder {
            background-color: grey !important;
            border: grey !important;
            rounded-md !important;
            border-radius: 8px important;
            display:none;
          }
        `}
      </style>
      <ResponsiveGridLayout
        className='layout font-silkscreen'
        autoSize={true}
        layouts={layouts}
        cols={cols}
        breakpoint=''
        useCSSTransforms={true}
        rowHeight={100}
        isBounded={false}
        isDraggable={true}
        isResizable={true}
      >
        <div
          key='cameraFunctions'
          className=' dark:bg-[#2A2A2A] text-black dark:text-white bg-white  p-2 border-black border-2 rounded-md overflow-auto '
        >
          <CameraFunctions />
        </div>
        <div
          key='lightSettings'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <LightSettings />
        </div>
        <div
          key='driveMode'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <DriveMode />
        </div>
        <div
          key='angles'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <Angles />
        </div>
        <div
          key='temperature'
          className=' drag-Handle flex justify-center dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <Temperature />
        </div>
        <div
          key='coordinates'
          className=' drag-Handle flex justify-center dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <Coordinates />
        </div>
        <div
          key='cameraFeed'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white border-black border-2 rounded-md'
        >
          <CameraFeed />
        </div>
        <div
          key='sensors'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md overflow-auto'
        >
          <Sensor />
        </div>
        <div
          key='battery'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <Battery />
        </div>
        <div
          key='manipulatorConnection'
          className='drag-Handle  dark:bg-[#2A2A2A] bg-white text-black dark:text-white p-2 border-black border-2 rounded-md'
        >
          <Manipulator />
        </div>
      </ResponsiveGridLayout>
    </div>
  );
};
