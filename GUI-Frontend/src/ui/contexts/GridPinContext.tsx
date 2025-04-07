import React, { createContext, useContext, useState } from 'react';

interface GridPinContextType {
  pinnedItems: string[];
  togglePin: (itemId: string) => void;
  isPinned: (itemId: string) => boolean;
}

const GridPinContext = createContext<GridPinContextType>({
  pinnedItems: [],
  togglePin: () => {},
  isPinned: () => false,
});

export const useGridPin = () => useContext(GridPinContext);

export const GridPinProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [pinnedItems, setPinnedItems] = useState<string[]>([]);

  const togglePin = (itemId: string) => {
    setPinnedItems((prev) => {
      if (prev.includes(itemId)) {
        return prev.filter((id) => id !== itemId);
      }
      return [...prev, itemId];
    });
  };

  const isPinned = (itemId: string) => pinnedItems.includes(itemId);

  return <GridPinContext.Provider value={{ pinnedItems, togglePin, isPinned }}>{children}</GridPinContext.Provider>;
};
