<script lang="ts">
    import Header from "../components/Header.svelte";
    import Footer from "../components/Footer.svelte";
    import CommandPalette from "../components/CommandPalette.svelte";
    import { onMount } from "svelte";
    import { currentUserStore, isCommandPaletteOpen } from "../stores";
    import { getSelf } from "../api/auth";

    import "../app.scss";

    const onKeyUp = async (e: KeyboardEvent) => {
        if (e.key === "Escape") {
            isCommandPaletteOpen.update(open => !open);
        }
    };

    onMount(async () => {
        document.addEventListener("keyup", onKeyUp);

        $currentUserStore = await getSelf();
    });
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
