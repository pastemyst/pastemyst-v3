<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import type { LayoutData } from "./$types";
    import { activePastesStores, currentUserStore, versionStore } from "$lib/stores";
    import CommandPalette from "$lib/CommandPalette.svelte";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";

    export let data: LayoutData;
    $: currentUserStore.set(data.self);
    $: versionStore.set(data.version);
    $: activePastesStores.set(data.activePastes);
</script>

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
