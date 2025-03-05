import React from 'react';
import Plot from 'react-plotly.js';
import { Layout } from 'plotly.js';

export const Coordinates = () => {
  // Use Partial<PlotData> to prevent unnecessary required properties
  const data: Partial<Plotly.Data> = {
    x: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11], // Spread out X values
    y: [10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0], // Spread out Y values
    z: [5, 15, 10, 20, 30, 25, 35, 40, 45, 50, 60], // Spread out Z values
    mode: 'markers',
    type: 'scatter3d', // Ensures it's correctly typed
    marker: {
      size: 3,
      color: '#4bd5ff',
    },
  };

  // Layout with full width and height + margin fixes
  const layout: Partial<Layout> = {
    autosize: true,
    title: '3D Scatter Plot',
    margin: { l: 1, r: 1, t: 1, b: 1 },
    scene: { aspectmode: 'auto', camera: { eye: { x: 1.4, y: 1.4, z: 1 } } },
    paper_bgcolor: 'rgba(0,0,0,0)', // Transparent background
    plot_bgcolor: 'rgba(0,0,0,0)', // Transparent plot area
    font: { color: '#ffffff' },
  };

  return (
    <>
      <div className='w-full h-full flex flex-col justify-center items-center overflow-y-hidden'>
        <div className=' w-full min-h-[20px] text-[18px] lg:text-[25px] text-center '>Coordinates</div>
        <div className='h-[90%] mb-3 ml-6 mr-6 pb-2  bg-[#2A2A2A] dark:bg-[#2A2A2A] '>
          <Plot
            data={[data]}
            layout={layout}
            config={{ displayModeBar: false }}
            useResizeHandler={true}
            style={{ width: '100%', height: '100%' }}
          />
        </div>
      </div>
    </>
  );
};
