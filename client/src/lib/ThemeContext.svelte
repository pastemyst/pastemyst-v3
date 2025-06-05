<script lang="ts">
    import { onMount, type Snippet } from "svelte";
    import { settingsStore, systemThemeStore, themeStore } from "./stores.svelte";
    import { themes, type Theme } from "./themes";

    interface Props {
        theme: string;
        darkTheme: string;
        followSystemTheme: boolean;
        children: Snippet;
    }

    let { theme, darkTheme, followSystemTheme, children }: Props = $props();

    let mounted = false;

    themeStore.subscribe((theme) => {
        if (!theme) return;

        if (mounted) setRootColors(theme);
    });

    systemThemeStore.subscribe((systemTheme) => {
        if (!systemTheme) return;

        if (!settingsStore.settings?.followSystemTheme) return;

        const themeName =
            systemTheme === "dark"
                ? settingsStore.settings.darkTheme
                : settingsStore.settings.theme;
        themeStore.set(themes.find((t) => t.name === themeName)!);
    });

    onMount(() => {
        mounted = true;

        const themeName = followSystemTheme
            ? $systemThemeStore === "dark"
                ? darkTheme
                : theme
            : theme;

        const themeObj = themes.find((t) => t.name === themeName) || themes[0];
        themeStore.set(themeObj);
    });

    const setRootColors = (theme: Theme) => {
        for (let [prop, color] of Object.entries(theme.colors)) {
            const varString = `--color-${prop}`;
            document.documentElement.style.setProperty(varString, color);
        }
    };
</script>

<div id="theme-context" data-theme={$themeStore?.name}>
    {@render children()}
</div>
