import React from 'react';
import { motion } from 'framer-motion';
import { useGridPin } from '../../contexts/GridPinContext';

interface PinButtonProps {
  itemId: string;
}

export const PinButton: React.FC<PinButtonProps> = ({ itemId }) => {
  const { togglePin, isPinned } = useGridPin();
  const pinned = isPinned(itemId);

  return (
    <motion.button
      whileHover={{ scale: 1.1 }}
      whileTap={{ scale: 0.9 }}
      onClick={() => togglePin(itemId)}
      className={`pin-button absolute top-2 right-2 z-10 p-1 rounded-full ${
        pinned ? 'bg-black text-white ' : 'bg-gray-200 dark:bg-[#2A2A2A] text-gray-600 dark:text-gray-300'
      }`}
      title={pinned ? 'Unlock' : 'Lock'}
    >
      <svg xmlns='http://www.w3.org/2000/svg' className='h-4 w-4' fill='none' viewBox='0 0 24 24' stroke='currentColor'>
        {pinned ? (
          // Lock icon
          <path
            strokeLinecap='round'
            strokeLinejoin='round'
            strokeWidth={2}
            d='M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z'
          />
        ) : (
          // Unlock icon
          <path
            strokeLinecap='round'
            strokeLinejoin='round'
            strokeWidth={2}
            d='M8 11V7a4 4 0 118 0m-4 8v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2z'
          />
        )}
      </svg>
    </motion.button>
  );
};
