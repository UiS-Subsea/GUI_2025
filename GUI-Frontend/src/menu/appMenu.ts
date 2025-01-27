import { MenuChannels } from 'src/channels/menuChannels';
import { emitEvent } from 'src/webContents';

const MenuItems: Electron.MenuItemConstructorOptions[] = [
  {
    label: 'SUBSEA GUI',
    submenu: [
      {
        label: 'Desktop',
      },
      {
        type: 'separator',
      },
      {
        id: MenuChannels.WINDOW_CLOSE,
        label: 'Exit',
        role: 'quit',
        accelerator: 'CmdOrCtrl+Q',
      },
    ],
  },
  {
    label: 'View',
    submenu: [
      {
        id: MenuChannels.WEB_ACTUAL_SIZE,
        label: 'Reset Zoom',
        role: 'resetZoom',
        accelerator: 'CmdOrCtrl+0',
      },
      {
        id: MenuChannels.WEB_ZOOM_IN,
        label: 'Zoom In',
        role: 'zoomIn',
      },
      {
        id: MenuChannels.WEB_ZOOM_OUT,
        label: 'Zoom Out',
        role: 'zoomOut',
        accelerator: 'CmdOrCtrl+-',
      },
      {
        type: 'separator',
      },
      {
        id: MenuChannels.WEB_TOGGLE_FULLSCREEN,
        label: 'Toggle Full Screen',
        role: 'togglefullscreen',
      },
      {
        type: 'separator',
      },
      {
        id: MenuChannels.WEB_TOGGLE_DEVTOOLS,
        label: 'Toogle Developer Tools',
        role: 'toggleDevTools',
        accelerator: 'CmdOrCtrl+Shift+I',
      },
    ],
  },
  {
    label: 'UIS SUBSEA',
    submenu: [
      {
        id: MenuChannels.OPEN_GITHUB_PROFILE,
        label: 'About UiS Subsea',
        click: emitEvent(MenuChannels.OPEN_GITHUB_PROFILE),
      },
    ],
  },
];

export default MenuItems;
