<script lang="ts">
    import Header from "../components/Header.svelte";
    import Footer from "../components/Footer.svelte";
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

    <main>
        <slot />
    </main>

    <Footer />
</div>

<CommandPalette bind:this={commandPalette} />

<style lang="scss">
    main {
        flex: 1 0 auto;
    }
</style>
