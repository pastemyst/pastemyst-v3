import { sveltekit } from "@sveltejs/kit/vite";

/** @type {import('vite').UserConfig} */
const config = {
    plugins: [sveltekit()],
    css: {
        preprocessorOptions: {
            scss: {
                additionalData: '@use "/src/variables.scss" as *;',
                api: "modern-compiler"
            }
        }
    },
    server: {
        port: 3000,
        fs: {
            allow: ["static/scripts/"]
        }
    }
};

export default config;
