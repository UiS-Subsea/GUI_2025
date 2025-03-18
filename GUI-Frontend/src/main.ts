import { app, BrowserWindow } from 'electron';

import installExtension, { REACT_DEVELOPER_TOOLS } from 'electron-devtools-installer';

import { createAppWindow } from './appWindow';

// Import PeerJS to run the signaling server
import { PeerServer } from 'peer'; // Import the PeerServer module

process.env.ELECTRON_DISABLE_SECURITY_WARNINGS = 'true';

/** Handle creating/removing shortcuts on Windows when installing/uninstalling. */
if (require('electron-squirrel-startup')) {
  app.quit();
}

// Declare peerServer globally
let peerServer: any; // Declare the PeerJS server variable

app.whenReady().then(() => {
  installExtension(REACT_DEVELOPER_TOOLS)
    .then((name) => console.info(`Added Extension:  ${name}`))
    .catch((err) => console.info('An error occurred: ', err));

  // Start the PeerJS signaling server
  startPeerServer();
});

// Function to start the PeerJS server
function startPeerServer() {
  peerServer = PeerServer({
    port: 9000, // You can choose any available port
    path: '/peerjs', // The path for the PeerJS server (same path you'll use in your frontend)
  });

  console.log('[PeerJS] Signaling server running on port 9000');
}

/**
 * This method will be called when Electron has finished
 * initialization and is ready to create browser windows.
 * Some APIs can only be used after this event occurs.
 */
app.on('ready', createAppWindow);

/**
 * Emitted when the application is activated. Various actions can
 * trigger this event, such as launching the application for the first time,
 * attempting to re-launch the application when it's already running,
 * or clicking on the application's dock or taskbar icon.
 */
app.on('activate', () => {
  /**
   * On OS X it's common to re-create a window in the app when the
   * dock icon is clicked and there are no other windows open.
   */
  if (BrowserWindow.getAllWindows().length === 0) {
    createAppWindow();
  }
});

/**
 * Emitted when all windows have been closed.
 */
app.on('window-all-closed', () => {
  /**
   * On OS X it is common for applications and their menu bar
   * to stay active until the user quits explicitly with Cmd + Q
   */
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

/**
 * In this file you can include the rest of your app's specific main process code.
 * You can also put them in separate files and import them here.
 */
