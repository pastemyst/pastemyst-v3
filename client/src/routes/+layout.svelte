<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import CommandPalette from "$lib/CommandPalette.svelte";
    import type { LayoutData } from "./$types";
    import { activePastesStores, currentUserStore, versionStore } from "$lib/stores";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";

    export let data: LayoutData;
    $: currentUserStore.set(data.self);
    $: versionStore.set(data.version);
    $: activePastesStores.set(data.activePastes);

    const handleKeys = async (e: KeyboardEvent) => {
        if (e.ctrlKey && e.key === "k") {
            e.preventDefault();
            window.dispatchEvent(new CustomEvent("toggleCmd"));
        }
    };
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
