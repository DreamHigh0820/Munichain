module.exports = {
    content: [
        './**/*.html',
        './**/*.razor',
        './**/*.cshtml',
    ],
    variants: {
        extend: {},
    },
    plugins: [
      require('@tailwindcss/forms'),
      require('@tailwindcss/line-clamp'),
    ],
}