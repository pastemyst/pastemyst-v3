<script lang="ts">
    import { onMount } from "svelte";
    import { themeStore } from "./stores";
    import { themes, type Theme } from "./themes";

    let currentTheme = themes[0];

    let mounted = false;

    themeStore.subscribe((theme) => {
        currentTheme = theme;
        if (mounted) setRootColors(currentTheme);
    });

    onMount(() => {
        mounted = true;

        setRootColors(currentTheme);
    });

    const setRootColors = (theme: Theme) => {
        for (let [prop, color] of Object.entries(theme.colors)) {
            const varString = `--color-${prop}`;
            document.documentElement.style.setProperty(varString, color);
        }
    };
</script>

<slot />
