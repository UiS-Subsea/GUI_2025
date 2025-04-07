import { app, BrowserWindow, ipcMain } from 'electron';
import path from 'path';
import installExtension, { REACT_DEVELOPER_TOOLS } from 'electron-devtools-installer';
import { createAppWindow } from './appWindow';
import fs from 'fs';
import { PeerServer } from 'peer';

process.env.ELECTRON_DISABLE_SECURITY_WARNINGS = 'true';

let cameraWindow: BrowserWindow | null = null;

/**
 * Create a secondary Camera Window
 */
function createCameraWindow() {
  cameraWindow = new BrowserWindow({
    width: 800,
    height: 600,
    title: 'Camera Window',
    webPreferences: {
      contextIsolation: true,
      nodeIntegration: false,
      preload: path.join(__dirname, 'preload.js'),
      webSecurity: true,
    },
  });

  const devServerURL = process.env.VITE_DEV_SERVER_URL;

  if (devServerURL) {
    // Development mode
    console.log('✅ Dev mode: Loading CameraWindow from Vite dev server:', devServerURL);
    cameraWindow.loadURL(`${devServerURL}#/camera`);
  } else {
    // Production mode
    // Look for index.html in the dist directory instead of .vite/build
    const indexHtmlPath = path.join(__dirname, '..', 'dist', 'index.html');
    console.log('✅ Production mode: Loading CameraWindow from file:', indexHtmlPath);

    // Optional: Check if file exists to avoid silent white screen
    if (!fs.existsSync(indexHtmlPath)) {
      console.error('❌ index.html not found at:', indexHtmlPath);
      // Try alternative path
      const altPath = path.join(process.cwd(), 'dist', 'index.html');
      if (fs.existsSync(altPath)) {
        cameraWindow.loadFile(altPath, { hash: '/camera' });
        return;
      }
      console.error('❌ index.html not found at alternative path:', altPath);
      return;
    }

    cameraWindow.loadFile(indexHtmlPath, { hash: '/camera' });
  }

  // Set permissions for camera access
  cameraWindow.webContents.session.setPermissionRequestHandler((webContents, permission, callback) => {
    const allowedPermissions = ['media'];
    if (allowedPermissions.includes(permission)) {
      callback(true);
    } else {
      callback(false);
    }
  });

  // Enable camera access
  cameraWindow.webContents.session.setPermissionCheckHandler((webContents, permission) => {
    return permission === 'media';
  });

  cameraWindow.webContents.openDevTools(); // Remove or comment out in production

  cameraWindow.on('closed', () => {
    cameraWindow = null;
  });
}

// Listen for the open-camera-window event from renderer
ipcMain.on('open-camera-window', () => {
  createCameraWindow();
});

// Squirrel installer handler (Windows)
if (require('electron-squirrel-startup')) {
  app.quit();
}

// Declare peerServer globally
let peerServer: any; // Declare the PeerJS server variable

// Install React Developer Tools on dev startup
app.whenReady().then(() => {
  installExtension(REACT_DEVELOPER_TOOLS)
    .then((name) => console.info(`✅ DevTools Extension Loaded: ${name}`))
    .catch((err) => console.warn('❌ DevTools install error:', err));

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

// Create main window on app ready
app.on('ready', createAppWindow);

// macOS: Re-create window when dock icon is clicked and no windows are open
app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createAppWindow();
  }
});

// Quit app when all windows are closed (except on macOS)
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});
