<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import type { LayoutData } from "./$types";
    import { activePastesStores, currentUserStore, versionStore } from "$lib/stores";
    import CommandPalette from "$lib/CommandPalette.svelte";
    import { Close, setBaseCommands, type Command } from "$lib/command";
    import { beforeNavigate, goto } from "$app/navigation";
    import { apiBase } from "$lib/api/api";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";

    export let data: LayoutData;
    $: currentUserStore.set(data.self);
    $: versionStore.set(data.version);
    $: activePastesStores.set(data.activePastes);

    const getCommands = (): Command[] => {
        const commands: Command[] = [
            {
                name: "view changelog",
                action: () => {
                    goto("/changelog");
                    return Close.yes;
                }
            }
        ];

        if (data.self) {
            commands.push(
                {
                    name: "view my profile",
                    action: () => {
                        goto(`/~${data.self?.username}`);
                        return Close.yes;
                    }
                },
                {
                    name: "logout",
                    action: () => {
                        window.location.href = `${apiBase}/auth/logout`;
                        return Close.yes;
                    }
                }
            );
        } else {
            commands.push({
                name: "login / register",
                action: () => {
                    goto("/login");
                    return Close.yes;
                }
            });
        }

        return commands;
    };

    setBaseCommands(getCommands());

    beforeNavigate(() => {
        setBaseCommands(getCommands());
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
