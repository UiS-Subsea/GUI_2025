import React, { useState } from 'react';
import { motion } from 'framer-motion';

interface ButtonProps {
  name: string;
  action?: () => void;
  selected?: boolean;
}

export const Button: React.FC<ButtonProps> = ({ name, action, selected }) => {
  const [isClicked, setIsClicked] = useState(false);

  const handleClick = () => {
    setIsClicked(!isClicked);
    if (action) action(); //i have changed this.
  };

  return (
    <motion.button
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.9 }}
      onClick={action}
      className={`dark:text-white text-black border-2 border-black font-bold py-2 px-4 rounded text-[18px] min-w-[70px] w-full transition-colors duration-300 ${
        selected ? 'bg-blue-500 text-white' : 'dark:bg-[#2A2A2A] dark:hover:bg-[#2A2A2A] bg-white hover:bg-gray-200'
      }`}
    >
      {name}
    </motion.button>
  );
};
