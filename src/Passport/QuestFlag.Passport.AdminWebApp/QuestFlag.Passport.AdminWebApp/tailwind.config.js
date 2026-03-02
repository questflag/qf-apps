/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./**/*.{razor,html,cshtml}",
        "../QuestFlag.Passport.AdminWebApp.Client/**/*.{razor,html,cshtml}"
    ],
    theme: {
        extend: {
            colors: {
                border: "hsl(var(--border))",
                background: "hsl(var(--background))",
                foreground: "hsl(var(--foreground))",
            }
        },
    },
    plugins: [],
}
