<script lang="ts">
    import Header from "../components/Header.svelte";
    import CommandPalette from "../components/CommandPalette.svelte";
    import { onMount } from "svelte";
    import { isCommandPaletteOpen } from "../stores";

    import "../app.scss";

    let commandPalette: CommandPalette;

    const onKeyUp = async (e: KeyboardEvent) => {
        if (e.key === "Escape") {
            isCommandPaletteOpen.update(open => !open);
        }
    };

    onMount(() => {
        document.addEventListener("keyup", onKeyUp);
    });
</script>

<div id="container">
    <Header />

    <slot />
</div>

<CommandPalette bind:this={commandPalette} />
