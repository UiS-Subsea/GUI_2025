import React from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';

interface NavigateButtonProps {
  text: string;
  route: string;
  image?: React.ReactNode;
}

export const NavigateButton = ({ text, route, image }: NavigateButtonProps) => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate(route);
  };

  return (
    <motion.button
      whileTap={{ scale: 0.9 }}
      onClick={handleClick}
      className='rounded-md border-2 border-black dark:border-white text-lightText dark:text-darkText w-full h-full text-[20px] flex items-center justify-center gap-2 p-2'
    >
      {image && <span className='flex items-center'>{image}</span>} {/* Render image if provided */}
      {text && <span>{text}</span>} {/* Render text if provided */}
    </motion.button>
  );
};
