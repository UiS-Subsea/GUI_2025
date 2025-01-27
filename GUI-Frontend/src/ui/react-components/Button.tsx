import React, { useState } from 'react';

import { motion } from 'framer-motion';

interface ButtonProps {
  name: string;
  action?: () => void;
}

export const Button: React.FC<ButtonProps> = ({ name, action }) => {
  const [isClicked, setIsClicked] = useState(false);

  const handleClick = () => {
    setIsClicked(!isClicked);
  };
  return (
    <motion.button
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.9 }}
      onClick={handleClick}
      className={`dark:text-white text-black border-2 border-black font-bold py-2 px-4 rounded text-[18px] min-w-[70px] w-full transition-colors duration-300 ${
        isClicked
          ? 'bg-[#4ae4ff] hover:bg-[#4ae4ff]'
          : 'dark:bg-[#2A2A2A] dark:hover:bg-[#2A2A2A] bg-white hover:bg-white'
      }`}
    >
      {name}
    </motion.button>
  );
};
