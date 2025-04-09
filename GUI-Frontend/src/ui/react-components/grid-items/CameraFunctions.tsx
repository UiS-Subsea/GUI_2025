import React, { useState, useRef, useEffect } from 'react';
import { Button } from '../Button';
import CamWindowButton from '../CamWindowButton';

export const CameraFunctions = () => {
  const [tiltValue, setTiltValue] = useState(0);
  const sliderRef = useRef<HTMLDivElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const handleMouseDown = () => {
    setIsDragging(false);
  };

  const handleMouseUp = () => {
    setIsDragging(true);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging || !sliderRef.current) return;

    const sliderRect = sliderRef.current.getBoundingClientRect();
    const sliderWidth = sliderRect.width;
    const sliderLeft = sliderRect.left;

    // Calculate position relative to slider (0 at left, 1 at right)
    let position = (e.clientX - sliderLeft) / sliderWidth;

    // Clamp position between 0 and 1
    position = Math.max(0, Math.min(1, position));

    // Convert to tilt angle (0 to 180 degrees)
    const tiltAngle = position * 180;

    setTiltValue(Math.round(tiltAngle));
    //  TODO: add the logic to control the camera tilt
    console.log('Camera tilt value:', Math.round(tiltAngle));
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
      e.preventDefault();
      const change = e.key === 'ArrowRight' ? 5 : -5;
      const newValue = Math.max(0, Math.min(180, tiltValue + change));
      setTiltValue(newValue);
      console.log('Camera tilt value:', newValue);
    }
  };

  useEffect(() => {
    const handleGlobalMouseUp = () => {
      setIsDragging(false);
    };

    window.addEventListener('mouseup', handleGlobalMouseUp);
    return () => {
      window.removeEventListener('mouseup', handleGlobalMouseUp);
    };
  }, []);

  return (
    <div className='h-full w-full flex flex-col lg:text-[25px] text-[17px] justify-center items-center'>
      <h1>Camera Functions</h1>
      <div className='w-full justify-center flex flex-col '>
        <div className='w-full h-full flex flex-col gap-4 justify-center items-center'>
          <div className='gap-4 flex flex-col lg:flex-row min-w-[70px] w-full'>
            <CamWindowButton />
          </div>

          <div className='w-full flex justify-center'>
            <div className='flex flex-col justify-center items-center'>
              <span className=' items-center justify-center flex text-[18px] h-full'>Camera Tilt: {tiltValue}°</span>
              <div
                ref={sliderRef}
                className='relative h-8 w-40 bg-gray-200 rounded-full cursor-pointer'
                onMouseMove={handleMouseMove}
                role='slider'
                aria-label='Camera tilt control'
                aria-valuemin={0}
                aria-valuemax={180}
                aria-valuenow={tiltValue}
                tabIndex={0}
                onKeyDown={handleKeyDown}
              >
                <div
                  className='absolute h-8 w-8 bg-blue-500 rounded-full cursor-grab active:cursor-grabbing'
                  style={{
                    left: `${(tiltValue / 180) * 100}%`,
                    transform: 'translate(-50%, -50%)',
                    top: '50%',
                  }}
                  onMouseDown={handleMouseUp}
                  role='button'
                  aria-label='Tilt control handle'
                  tabIndex={0}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
