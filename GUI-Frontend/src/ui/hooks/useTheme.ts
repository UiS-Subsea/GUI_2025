import { useEffect } from 'react';

const matchColorSchemeMedia = window.matchMedia('(prefers-color-scheme: dark)');

const themeToggler = (matches: boolean) => {
  document.documentElement.classList.toggle('dark', matches); // Toggle 'dark' on <html>
  document.documentElement.classList.toggle('light', !matches); // Toggle 'light' on <html>
};

const mediaQueryListener = (e: MediaQueryListEvent) => {
  themeToggler(e.matches);
};

export function useThemeListener() {
  useEffect(() => {
    themeToggler(matchColorSchemeMedia.matches);

    matchColorSchemeMedia.addEventListener('change', mediaQueryListener);

    return () => {
      matchColorSchemeMedia.removeEventListener('change', mediaQueryListener);
    };
  }, []);
}
