<script lang="ts">
    import Header from "$lib/Header.svelte";
    import Footer from "$lib/Footer.svelte";
    import type { LayoutServerData } from "./$types";
    import CommandPalette from "$lib/CommandPalette.svelte";
    import {
        Close,
        setBaseCommands,
        setPreselectedCommand,
        setTempCommands,
        type Command
    } from "$lib/command";
    import { beforeNavigate, goto } from "$app/navigation";
    import ThemeContext from "$lib/ThemeContext.svelte";
    import { onMount, type Snippet } from "svelte";
    import {
        activePastesStores,
        cmdPalTitle,
        currentUserStore,
        settingsStore,
        systemThemeStore,
        themeStore,
        versionStore
    } from "$lib/stores.svelte";

    import "tippy.js/dist/tippy.css";
    import "tippy.js/dist/svg-arrow.css";
    import "../app.scss";
    import Toaster from "$lib/Toaster.svelte";
    import { tooltip } from "$lib/tooltips";
    import { formatDistanceToNow } from "date-fns";
    import { page } from "$app/state";
    import { themes } from "$lib/themes";
    import { updateSettings } from "$lib/api/settings";
    import { getApiUrl } from "$lib/api/fetch";

    interface Props {
        data: LayoutServerData;
        children: Snippet;
    }

    let { data, children }: Props = $props();

    let latestAnnouncement = $state(data.latestAnnouncement);
    let hiddenAnnouncement = $state(true);

    versionStore.set(data.version);
    activePastesStores.set(data.activePastes);
    currentUserStore.set(data.self);

    onMount(() => {
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.has("login_redirect")) {
            window.location.href = "/";
        }

        if (latestAnnouncement) {
            hiddenAnnouncement =
                localStorage.getItem(`dismissedAnnouncement-${latestAnnouncement.id}`) === "true";
        }

        settingsStore.settings = data.settings;
    });

    const themeCommands = (): Command[] => {
        return themes.map(
            (theme) =>
                ({
                    name: theme.name,
                    action: () => {
                        if (data.settings.followSystemTheme && $systemThemeStore === "dark") {
                            data.settings.darkTheme = theme.name;
                        } else {
                            data.settings.theme = theme.name;
                        }

                        updateSettings(fetch, data.settings);

                        themeStore.set(theme);

                        return Close.yes;
                    },
                    onHover: () => {
                        themeStore.set(theme);
                    },
                    onClose: () => {
                        const themeName = data.settings.followSystemTheme
                            ? $systemThemeStore === "dark"
                                ? data.settings.darkTheme
                                : data.settings.theme
                            : data.settings.theme;
                        const theme = themes.find((t) => t.name === themeName)!;
                        themeStore.set(theme);
                    }
                }) satisfies Command
        );
    };

    const baseCommands = (): Command[] => {
        const commands: Command[] = [
            {
                name: "change theme",
                action: () => {
                    cmdPalTitle.set("select a theme");
                    setTempCommands(themeCommands());

                    if (data.settings.followSystemTheme && $systemThemeStore === "dark") {
                        setPreselectedCommand(data.settings.darkTheme);
                    } else {
                        setPreselectedCommand(data.settings.theme);
                    }

                    return Close.no;
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
                    name: "settings page",
                    action: () => {
                        goto("/settings/profile");
                        return Close.yes;
                    }
                },
                {
                    name: "logout",
                    action: () => {
                        window.location.href = `${getApiUrl()}/auth/logout`;
                        return Close.yes;
                    }
                }
            );
        } else {
            commands.push(
                {
                    name: "settings page",
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

        commands.push({
            name: "view changelog",
            action: () => {
                goto("/changelog");
                return Close.yes;
            }
        });

        return commands;
    };

    setBaseCommands(baseCommands());

    beforeNavigate(() => {
        setBaseCommands(baseCommands());
    });

    const dismissAnnouncement = () => {
        if (!latestAnnouncement) return;

        localStorage.setItem(`dismissedAnnouncement-${latestAnnouncement.id}`, "true");

        latestAnnouncement = undefined;
    };
</script>

{#if page.route.id?.includes("embed")}
    {@render children()}
{:else}
    <ThemeContext
        theme={data.settings.theme}
        darkTheme={data.settings.darkTheme}
        followSystemTheme={data.settings.followSystemTheme}
    >
        <Toaster />

        <div id="container">
            <Header />

            {#if latestAnnouncement && !hiddenAnnouncement}
                <div class="announcement">
                    <div class="flex row space-between">
                        <p class="title">
                            {latestAnnouncement.title}
                            <span
                                class="date"
                                use:tooltip
                                aria-label={new Date(latestAnnouncement.createdAt).toString()}
                                >{formatDistanceToNow(new Date(latestAnnouncement.createdAt), {
                                    addSuffix: true
                                })}</span
                            >
                        </p>
                        <button
                            class="close-icon flex center"
                            onclick={dismissAnnouncement}
                            use:tooltip
                            aria-label="dismiss"
                        >
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                viewBox="0 0 16 16"
                                class="icon"
                            >
                                <title>Close Icon</title>
                                <path
                                    fill="currentColor"
                                    fill-rule="evenodd"
                                    d="M3.72 3.72a.75.75 0 011.06 0L8 6.94l3.22-3.22a.75.75 0 111.06 1.06L9.06 8l3.22 3.22a.75.75 0 11-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 01-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 010-1.06z"
                                />
                            </svg>
                        </button>
                    </div>
                    <!-- eslint-disable-next-line svelte/no-at-html-tags -->
                    <p class="content markdown">{@html data.latestAnnouncementRendered}</p>
                </div>
            {/if}

            <main>
                {@render children()}
            </main>

            <Footer />
        </div>

        <CommandPalette />
    </ThemeContext>
{/if}

<style lang="scss">
    main {
        flex: 1 0 auto;
    }

    .announcement {
        background-color: var(--color-bg1);
        padding: 0.5rem 1rem;
        padding-bottom: 0;
        margin-top: 2rem;
        border-radius: $border-radius;
        border: 1px solid var(--color-primary);
        font-size: $fs-normal;

        .title {
            margin: 0;
            font-weight: bold;
            font-size: $fs-medium;
        }

        .date {
            font-size: $fs-small;
            color: var(--color-bg3);
            font-weight: normal;
        }

        .close-icon {
            background-color: var(--color-bg);
        }

        .content {
            margin: 0;

            :global(p) {
                margin: 0.5rem 0;
            }
        }
    }
</style>
