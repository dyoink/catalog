/** @type {import('tailwindcss').Config} */
module.exports = {
  // Bật dark mode dựa vào class .dark trên <html> — toggle trong admin
  darkMode: 'class',
  content: [
    "./Views/**/*.cshtml",
    "./Areas/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        primary: '#55B3D9',
      }
    }
  },
  plugins: [
    require('@tailwindcss/typography'),
    require('@tailwindcss/forms'),
  ],
}
