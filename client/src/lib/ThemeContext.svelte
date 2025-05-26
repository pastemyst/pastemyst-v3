<script lang="ts">
    import { onMount, type Snippet } from "svelte";
    import { themeStore } from "./stores";
    import { themes } from "./themes";

    interface Props {
        currentTheme: string;
        children: Snippet;
    }

    let { currentTheme = $bindable(), children }: Props = $props();

    themeStore.subscribe((theme) => {
        if (!theme) return;

        currentTheme = theme.name;
    });

    onMount(() => {
        const themeObj = themes.find((t) => t.name === currentTheme) || themes[0];
        themeStore.set(themeObj);
    });
</script>

<div id="theme-context" data-theme={currentTheme}>
    {@render children()}
</div>
