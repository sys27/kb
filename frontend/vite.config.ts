import { reactRouter } from "@react-router/dev/vite";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";

export default defineConfig({
    server: {
        proxy: {
            '/api': {
                target: 'http://localhost:5164',
                changeOrigin: true,
                secure: false,
            }
        }
    },
    plugins: [tailwindcss(), reactRouter()],
    resolve: {
        tsconfigPaths: true,
    },
});
