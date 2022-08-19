<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import CommandPalette from "$lib/CommandPalette.svelte";
    import { onMount } from "svelte";
    import { currentUserStore } from "$lib/stores";
    import { getSelf } from "$lib/api/auth";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";

    const handleKeys = async (e: KeyboardEvent) => {
        if (e.ctrlKey && e.key === "k") {
            e.preventDefault();
            window.dispatchEvent(new CustomEvent("toggleCmd"));
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
