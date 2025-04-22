import exp from 'node:constants';
import React, { createContext, useContext, useEffect, useRef, useState } from 'react';

// Create a WebSocket context to share the sensor data with components
export const WebSocketContext = createContext<any>(null);

// WebSocketProvider component: Wraps the application to provide WebSocket data
export const WebSocketProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // Ref to hold the WebSocket connection object. This will persist between renders.
  const socketRef = useRef<WebSocket | null>(null);

  // State object to store all sensor data (temperature, humidity, pressure, battery).
  // It starts with null values, which will be updated as data comes in.
  const [sensorData, setSensorData] = useState<{
    THRUSTPAADRAG: {
      HFF: Number | 0;
      HHB: Number | 0;
      HVB: Number | 0;
      HVF: Number | 0;
      VHF: Number | 0;
      VHB: Number | 0;
      VVB: Number | 0;
      VVF: Number | 0;
    };
    REGTEMP: {
      REG_temp: Number | 0;
      Motor_temp: Number | 0;
      Depth: Number | 0;
    };
    VINKLER: {
      Roll: Number | 0;
      Stamp: Number | 0;
      Gir: Number | 0;
    };
    TEMPDYBDE: {
      Depth: Number | 0;
      Water_temp: Number | 0;
      Sensor_temp: Number | 0;
    };
    SENSORERROR: {
      IMU_Errors: string | '';
      Temp_Errors: string | '';
      PRESSURE_Errors: string | '';
      Leak_Errors: string | '';
    };
    COMTEMP: {
      Com_temp: Number | 0;
    };
    DATA12VRIGHT: {
      Power: Number | 0;
      Temp: Number | 0;
      Fuse: string | '';
    };
    DATA12VLEFT: {
      Power: Number | 0;
      Temp: Number | 0;
      Fuse: string | '';
    };
    DATA5V: {
      Power_temp: Number | 0;
    };
    BATTERY: {
      batteryPercentage: Number | 0;
      voltage: Number | 0;
      current: Number | 0;
    };
  }>({
    THRUSTPAADRAG: {
      HFF: 0,
      HHB: 0,
      HVB: 0,
      HVF: 0,
      VHF: 0,
      VHB: 0,
      VVB: 0,
      VVF: 0,
    },
    REGTEMP: {
      REG_temp: 0,
      Motor_temp: 0,
      Depth: 0,
    },
    VINKLER: {
      Roll: 0,
      Stamp: 0,
      Gir: 0,
    },
    TEMPDYBDE: {
      Depth: 0,
      Water_temp: 0,
      Sensor_temp: 0,
    },
    SENSORERROR: {
      IMU_Errors: '',
      Temp_Errors: '',
      PRESSURE_Errors: '',
      Leak_Errors: '',
    },
    COMTEMP: {
      Com_temp: 0,
    },
    DATA12VRIGHT: {
      Power: 0,
      Temp: 0,
      Fuse: '',
    },
    DATA12VLEFT: {
      Power: 0,
      Temp: 0,
      Fuse: '',
    },
    DATA5V: {
      Power_temp: 0,
    },
    BATTERY: {
      batteryPercentage: 0,
      voltage: 0,
      current: 0,
    },
  });
  const [stateData, setStateData] = useState<{
    ROVConState: boolean | false;
    ManiConState: boolean | false;
    ROVState: boolean | false;
  }>({
    ROVConState: false,
    ManiConState: false,
    ROVState: false,
  });

  // Function to send a message to the backend via WebSocket
  const sendMessage = (message: object) => {
    if (socketRef.current && socketRef.current.readyState === WebSocket.OPEN) {
      socketRef.current.send(JSON.stringify(message));
      console.log('Message sent:', message);
    } else {
      console.warn('WebSocket is not connected');
    }
  };

  // Effect hook to establish the WebSocket connection when the component mounts
  useEffect(() => {
    // Function to create and connect the WebSocket.
    const connectWebSocket = () => {
      // Initialize the WebSocket connection
      socketRef.current = new WebSocket('ws://localhost:5009/ws');

      // WebSocket connection open event handler: Triggered when the connection is established
      socketRef.current.onopen = () => {
        console.log('WebSocket connected to ws://localhost:5009/ws'); // Log when the connection is established
      };

      // WebSocket message event handler: Triggered when data is received from the server
      socketRef.current.onmessage = (event) => {
        // Parse the incoming data from JSON
        const data = JSON.parse(event.data);

        setStateData((prevData) => {
          const updatedData = { ...prevData };

          for (const item of data) {
            if (!item || typeof item !== 'object' || !item.Type) {
              continue;
            }

            switch (item.Type) {
              case 'ROVConState':
                updatedData.ROVConState = item.value;
                console.log('Updated ROV controller state:', item.value);
                break;

              case 'ManiConState':
                updatedData.ManiConState = item.value;
                console.log('Updated Mani controller state:', item.value);
                break;

              case 'ROVState':
                updatedData.ROVState = item.value;
                console.log('Updated Mani controller state:', item.value);
                break;

              default:
                console.warn(`1Received unknown data type: ${item.Type}`, item);
                break;
            }
          }

          return updatedData;
        });

        // Update the state with the incoming data if the value is defined
        setSensorData((prevData) => {
          const updatedData = { ...prevData }; // Copy previous data to retain unchanged values

          console.warn(' data item:', data);

          for (const item of data) {
            if (!item || typeof item !== 'object' || !item.Type) {
              continue;
            }

            switch (item.Type) {
              case 'THRUSTPAADRAG':
                updatedData.THRUSTPAADRAG = { ...updatedData.THRUSTPAADRAG, ...item };
                break;

              case 'REGTEMP':
                updatedData.REGTEMP = { ...updatedData.REGTEMP, ...item };
                break;

              case 'VINKLER':
                updatedData.VINKLER = { ...updatedData.VINKLER, ...item };
                break;

              case 'TEMPDYBDE':
                updatedData.TEMPDYBDE = { ...updatedData.TEMPDYBDE, ...item };
                break;

              case 'SENSORERROR':
                updatedData.SENSORERROR = { ...updatedData.SENSORERROR, ...item };
                break;

              case 'COMTEMP':
                updatedData.COMTEMP = { ...updatedData.COMTEMP, ...item };
                break;

              case 'DATA12VRIGHT':
                updatedData.DATA12VRIGHT = { ...updatedData.DATA12VRIGHT, ...item };
                break;

              case 'DATA12VLEFT':
                updatedData.DATA12VLEFT = { ...updatedData.DATA12VLEFT, ...item };
                break;

              case 'DATA5V':
                updatedData.DATA5V = { ...updatedData.DATA5V, ...item };
                break;
              case 'BATTERY':
                updatedData.BATTERY = { ...updatedData.BATTERY, ...item };
                break;

              default:
                console.warn(`Received unknown data type: ${item.Type}`, item);
                break;
            }
          }

          return updatedData;
        });
      };

      // WebSocket close event handler: Called when the connection is closed
      socketRef.current.onclose = () => {
        // If the WebSocket connection closes, attempt to reconnect after 3 seconds
        setTimeout(connectWebSocket, 3000); // Auto-reconnect after 3 seconds
      };
    };

    // Call the connectWebSocket function to establish the WebSocket connection
    connectWebSocket();

    // Cleanup function when the component unmounts or the WebSocket connection is no longer needed
    return () => socketRef.current?.close(); // Close WebSocket connection when component unmounts
  }, []); // Empty dependency array means this effect runs only once (on mount)

  return (
    // Provide the sensor data to the context, so any child component can access it
    <WebSocketContext.Provider value={{ sensorData, stateData, sendMessage }}>
      {children} {/* Render child components that will have access to the context */}
    </WebSocketContext.Provider>
  );
};

// Custom hook to get the full sensor data object
export const useSensorData = () => useContext(WebSocketContext).sensorData;

export const useStateData = () => useContext(WebSocketContext).stateData;

export const batteryDataRight = () => useContext(WebSocketContext).sensorData.DATA12VRIGHT;

export const batteryDataLeft = () => useContext(WebSocketContext).sensorData.DATA12VLEFT;

export const thrustpaadrag = () => useContext(WebSocketContext).sensorData.THRUSTPAADRAG;

export const regtemp = () => useContext(WebSocketContext).sensorData.REGTEMP;

export const vinkler = () => useContext(WebSocketContext).sensorData.VINKLER;

export const tempdybde = () => useContext(WebSocketContext).sensorData.TEMPDYBDE;

export const sensorerror = () => useContext(WebSocketContext).sensorData.SENSORERROR;

export const comtemp = () => useContext(WebSocketContext).sensorData.COMTEMP;

export const data5v = () => useContext(WebSocketContext).sensorData.DATA5V;

export const battery = () => useContext(WebSocketContext).sensorData.BATTERY;
