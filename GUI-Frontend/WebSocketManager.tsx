import React, { createContext, useContext, useEffect, useState, useRef } from 'react';

// Define the WebSocketManager class for managing the WebSocket connection
class WebSocketManager {
  private ws: WebSocket | null = null;
  private connected = false;

  constructor() {
    const url = 'ws://localhost:8765'; // Hardcoded WebSocket URL
    this.ws = new WebSocket(url);

    this.ws.onopen = () => {
      this.connected = true;
      console.log('Connected to WebSocket server');
    };

    this.ws.onclose = () => {
      this.connected = false;
      console.log('Disconnected from WebSocket server');
      // Attempt to reconnect after 3 seconds if connection is lost
      setTimeout(() => this.connect(), 3000);
    };

    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }

  // Reconnect to WebSocket server
  private connect() {
    const url = 'ws://localhost:8765'; // Ensure URL is the same as in constructor
    this.ws = new WebSocket(url);
    this.ws.onopen = () => {
      this.connected = true;
      console.log('Reconnected to WebSocket server');
    };

    this.ws.onclose = () => {
      this.connected = false;
      console.log('Disconnected from WebSocket server');
      setTimeout(() => this.connect(), 3000); // Reconnect after 3 seconds
    };

    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }

  sendCommand(command: string) {
    if (this.ws && this.connected) {
      const data = { type: 'command', command: command };
      this.ws.send(JSON.stringify(data));
      console.log(`Sent command: ${command}`);
    } else {
      console.log('WebSocket is not connected.');
    }
  }

  close() {
    if (this.ws) {
      this.ws.close();
    }
  }
}

// Create the WebSocket context to provide WebSocketManager to components
const WebSocketCommandContext = createContext<WebSocketManager | null>(null);

// WebSocketProvider component to wrap the app and provide WebSocketManager to components
export const WebSocketCommandProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [webSocketManager, setWebSocketManager] = useState<WebSocketManager | null>(null);

  useEffect(() => {
    const manager = new WebSocketManager();
    setWebSocketManager(manager);

    return () => {
      manager.close();
    };
  }, []);

  return <WebSocketCommandContext.Provider value={webSocketManager}>{children}</WebSocketCommandContext.Provider>;
};

// Custom hook to use the WebSocketManager in any component
export const useWebSocketCommand = (): WebSocketManager | null => {
  return useContext(WebSocketCommandContext);
};
