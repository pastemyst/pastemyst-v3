<script lang="ts">
    import Header from "../components/Header.svelte";
    import Footer from "../components/Footer.svelte";
    import CommandPalette from "../components/CommandPalette.svelte";
    import { onMount } from "svelte";
    import { currentUserStore, isCommandPaletteOpen } from "../stores";
    import { getSelf } from "../api/auth";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";

    const handleKeys = async (e: KeyboardEvent) => {
        if (e.ctrlKey && e.key === "k") {
            e.preventDefault();
            isCommandPaletteOpen.update((open) => !open);
        }
    };

    onMount(async () => {
        $currentUserStore = await getSelf();
    });
</script>

<svelte:window on:keydown={handleKeys} />

<div id="container">
    <Header />

    <main>
        <slot />
    </main>

    <Footer />
</div>

<CommandPalette />

<style lang="scss">
    main {
        flex: 1 0 auto;
    }
</style>
