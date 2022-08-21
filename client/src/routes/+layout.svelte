<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import type { LayoutData } from "./$types";
    import { activePastesStores, cmdPalCommands, currentUserStore, versionStore } from "$lib/stores";
    import CommandPalette from "$lib/CommandPalette.svelte";
    import type { Command } from "$lib/command";
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";
import { apiBase } from "$lib/api/api";

    export let data: LayoutData;
    $: currentUserStore.set(data.self);
    $: versionStore.set(data.version);
    $: activePastesStores.set(data.activePastes);

    onMount(() => {
        const commands: Command[] = [
            {
                name: "view changelog",
                action: () => {
                    goto("/changelog");
                }
            }
        ];

        if (data.self) {
            commands.push({
                name: "view my profile",
                action: () => {
                    goto(`/~${data.self?.username}`);
                }
            });

            commands.push({
                name: "logout",
                action: () => {
                    window.location.href = `${apiBase}/auth/logout`;
                }
            });
        } else {
            commands.push({
                name: "login / register",
                action: () => {
                    goto("/login");
                }
            });
        }

        cmdPalCommands.set(commands);
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
