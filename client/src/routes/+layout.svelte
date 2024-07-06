<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import type { LayoutData } from "./$types";
    import { activePastesStores, currentUserStore, settingsStore, versionStore } from "$lib/stores";
    import CommandPalette from "$lib/CommandPalette.svelte";
    import { Close, setBaseCommands, type Command } from "$lib/command";
    import { beforeNavigate, goto } from "$app/navigation";
    import ThemeContext from "$lib/ThemeContext.svelte";
    import { env } from "$env/dynamic/public";
    import { onMount } from "svelte";
    import { getLocalSettings } from "$lib/api/settings";
    import { Toaster } from "svelte-french-toast";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";

    export let data: LayoutData;
    $: currentUserStore.set(data.self);
    $: versionStore.set(data.version);
    $: activePastesStores.set(data.activePastes);

    onMount(() => {
        settingsStore.set(getLocalSettings());
    });

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
                    name: "settings",
                    action: () => {
                        goto("/settings/profile");
                        return Close.yes;
                    }
                },
                {
                    name: "logout",
                    action: () => {
                        window.location.href = `${env.PUBLIC_API_BASE}/auth/logout`;
                        return Close.yes;
                    }
                }
            );
        } else {
            commands.push(
                {
                    name: "settings",
                    action: () => {
                        goto("/settings/behaviour");
                        return Close.yes;
                    }
                },
                {
                    name: "login / register",
                    action: () => {
                        goto("/login");
                        return Close.yes;
                    }
                }
            );
        }

        return commands;
    };

    setBaseCommands(getCommands());

    beforeNavigate(() => {
        setBaseCommands(getCommands());
    });
</script>

<ThemeContext>
    <Toaster />

    <div id="container">
        <Header />

        <main>
            <slot />
        </main>

        <Footer />
    </div>
</ThemeContext>

<CommandPalette />

<style lang="scss">
    main {
        flex: 1 0 auto;
    }
</style>
