import { sveltekit } from "@sveltejs/kit/vite";
import path from "node:path";

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
            allow: ["static/scripts/", "static/themes/"]
        }
    },
    resolve: {
        alias: { "tm-grammars": path.resolve("node_modules/tm-grammars") }
    }
};

export default config;
