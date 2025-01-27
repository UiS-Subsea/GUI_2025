module.exports = {
  darkMode: 'class',
  content: ['./src/**/*.{tsx, ts, js, jsx, html}'],
    theme: {
      
        screens: {
            sm: '400px',
            md: '768px',
            lg: '1024px',
            xl: '1280px',
            '2xl': '1536px',
          },
      extend: {
        colors: {
          lightBg: '#ffffff',
          darkBg: '#232323',
          lightText: '#000000',
          darkText: '#ffffff',
        },
        fontFamily: {
            'silkscreen': ['Silkscreen', 'cursive'], 
          }
      },
    },
    plugins: [],
  }
  