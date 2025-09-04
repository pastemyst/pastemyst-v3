import js from "@eslint/js";
import ts from "typescript-eslint";
import svelte from "eslint-plugin-svelte";
import prettier from "eslint-config-prettier";
import globals from "globals";

/** @type {import('eslint').Linter.FlatConfig[]} */
export default [
    js.configs.recommended,
    ...ts.configs.recommended,
    ...svelte.configs["flat/recommended"],
    prettier,
    ...svelte.configs["flat/prettier"],
    {
        languageOptions: {
            globals: {
                ...globals.browser,
                ...globals.node
            }
        }
    },
    {
        files: ["**/*.svelte", "**/*.svelte.ts"],
        languageOptions: {
            parserOptions: {
                parser: ts.parser,
                extraFileExtensions: [".svelte", ".svelte.ts"],
                svelteFeatures: {
                    experimentalGenerics: true
                }
            }
        }
    },
    {
        ignores: ["build/", ".svelte-kit/", "dist/", "static/scripts/"]
    },
    {
        rules: {
            "svelte/no-navigation-without-resolve": "off"
        }
    }
];
