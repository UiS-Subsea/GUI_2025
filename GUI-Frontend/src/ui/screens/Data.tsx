import React, { useState, useEffect } from 'react';
import 'react-grid-layout/css/styles.css';
import 'react-resizable/css/styles.css';
import { NavigateButton } from '../react-components/NavigateButton';
import { DarkModeToggle } from '../react-components/navbar-items/DarkModeToggle';

export const Data: React.FC<{ isDarkMode: boolean; toggleDarkMode: () => void }> = ({ isDarkMode, toggleDarkMode }) => {
  const [logs, setLogs] = useState<string>('');
  const [selectedFilter, setSelectedFilter] = useState('today');

  const fetchLogs = async (filter: string) => {
    try {
      const response = await fetch(`http://localhost:5017/api/logs?filter=${filter}`);

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const data = await response.json();

      setLogs(data.logs || 'No logs available.');
    } catch (error) {
      console.error('Error fetching logs:', error);
      setLogs('Failed to fetch logs.');
    }
  };

  useEffect(() => {
    fetchLogs(selectedFilter);
    const interval = setInterval(() => fetchLogs(selectedFilter), 2000); // Update every 2s
    return () => clearInterval(interval);
  }, [selectedFilter]);

  const downloadLogs = () => {
    const element = document.createElement('a');
    const file = new Blob([logs], { type: 'text/plain' });
    element.href = URL.createObjectURL(file);
    element.download = `logs_${selectedFilter}.txt`;
    document.body.appendChild(element);
    element.click();
  };

  const printLogs = () => {
    const printWindow = window.open('', '', 'width=900,height=600');
    if (printWindow) {
      printWindow.document.write(`<pre>${logs}</pre>`);
      printWindow.document.close();
      printWindow.print();
    }
  };

  const getFilterLabel = (filter: string) => {
    switch (filter) {
      case 'today':
        return ' Today';
      case 'last7days':
        return ' Last 7 Days ';
      case 'last30days':
        return ' Last 30 Days ';
      default:
        return filter;
    }
  };

  return (
    <>
      <div className='w-screen h-screen bg-lightBg font-silkscreen dark:bg-darkBg '>
        <div className='w-screen h-[70px] pt-2 flex flex-row gap-2  bg-lightBg dark:bg-darkBg justify-center items-center transition-colors duration-300'>
          <div className='rounded-md sm:w-[30px] max-w-[80px] md:w-full h-full items-center justify-center flex'>
            {isDarkMode ? (
              <img src='./assets/images/logo.png' width={60} alt='dark logo' />
            ) : (
              <img src='/assets/images/logoDark.png' width={60} alt='light logo' />
            )}
          </div>

          <div className='h-full border-2 min-w-[300px] flex justify-center items-center rounded-md border-black dark:border-white text-lightText dark:text-darkText w-full text-[30px]'>
            ROV LOGGER
          </div>

          <div className='w-[110px] h-full'>
            <NavigateButton
              text=''
              route='/'
              image={
                isDarkMode ? (
                  <img className='w-9 h-9' src='/assets/images/BackDark.svg' alt='settings' width={40} />
                ) : (
                  <img className='w-9 h-9' src='/assets/images/BackLight.svg' alt='settings' width={40} />
                )
              }
            />
          </div>

          <div className='flex justify-center items-center'>
            <DarkModeToggle isDarkMode={isDarkMode} toggleDarkMode={toggleDarkMode} />
          </div>
        </div>

        <div className='flex justify-center mt-4 gap-4 text-[15px] lg:text-[18px]'>
          <button
            className={`px-4 py-2 border-2 p-4 min-w-[120px] flex justify-center items-center rounded-md transition-colors duration-300
          ${
            selectedFilter === 'today'
              ? 'bg-black text-white border-black dark:bg-white dark:text-black dark:border-white'
              : 'bg-lightBg text-lightText border-black dark:bg-darkBg dark:text-darkText dark:border-white'
          }`}
            onClick={() => setSelectedFilter('today')}
          >
            Today
          </button>
          <button
            className={`px-4 py-2 border-2 p-4 min-w-[120px] flex justify-center items-center rounded-md transition-colors duration-300
          ${
            selectedFilter === 'last7days'
              ? 'bg-black text-white border-black dark:bg-white dark:text-black dark:border-white'
              : 'bg-lightBg text-lightText border-black dark:bg-darkBg dark:text-darkText dark:border-white'
          }`}
            onClick={() => setSelectedFilter('last7days')}
          >
            Last 7 Days
          </button>
          <button
            className={`px-4 py-2 border-2 p-4 min-w-[120px] flex justify-center items-center rounded-md transition-colors duration-300
          ${
            selectedFilter === 'last30days'
              ? 'bg-black text-white border-black dark:bg-white dark:text-black dark:border-white'
              : 'bg-lightBg text-lightText border-black dark:bg-darkBg dark:text-darkText dark:border-white'
          }`}
            onClick={() => setSelectedFilter('last30days')}
          >
            Last 30 Days
          </button>

          <div className='mx-8'></div>

          <button
            className='border-2 p-4 min-w-[120px]  flex justify-center items-center rounded-md border-black dark:border-white text-lightText dark:text-darkText'
            onClick={printLogs}
          >
            Print Logs
          </button>
          <button
            className='border-2 p-4 min-w-[120px]  flex justify-center items-center rounded-md border-black dark:border-white text-lightText dark:text-darkText'
            onClick={downloadLogs}
          >
            Download Logs
          </button>
        </div>

        <div className='mt-6 p-4 max-w-4xl mx-auto'>
          <h2
            className={`text-lg font-bold mb-2 text-[15px] lg:text-[18px] transition-all duration-300 ${isDarkMode ? 'text-white' : 'text-black'}`}
          >
            Logs ({getFilterLabel(selectedFilter)})
          </h2>

          <pre
            className={`p-3 text-[15px] font-silkscreen lg:text-[18px] rounded overflow-auto max-h-96 whitespace-pre-wrap transition-all duration-300 flex justify-start items-start ${
              isDarkMode ? 'bg-[#232323] text-white' : 'bg-gray-100 text-black'
            }`}
            style={{ whiteSpace: 'pre-wrap', wordWrap: 'break-word', maxHeight: '500px', overflowY: 'auto' }}
          >
            {logs}
          </pre>
        </div>
      </div>
    </>
  );
};

export default Data;
