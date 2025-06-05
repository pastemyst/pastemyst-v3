<script lang="ts">
    import { updateSettings } from "$lib/api/settings";
    import type { PageData } from "./$types";
    import { cmdPalOpen, cmdPalTitle, systemThemeStore, themeStore } from "$lib/stores.svelte";
    import { Close, setTempCommands, type Command } from "$lib/command";
    import { themes } from "$lib/themes";
    import Checkbox from "$lib/Checkbox.svelte";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let settings = $state(data.settings);

    const getPasteViewCommands = (): Command[] => {
        return [
            {
                name: "tabbed",
                action: () => {
                    settings.pasteView = "tabbed";
                    updateSettings(fetch, settings);

                    return Close.yes;
                }
            },
            {
                name: "stacked",
                action: () => {
                    settings.pasteView = "stacked";
                    updateSettings(fetch, settings);

                    return Close.yes;
                }
            }
        ];
    };

    const getThemeCommands = (isDarkTheme: boolean): Command[] => {
        const commands: Command[] = [];
        for (const theme of themes) {
            commands.push({
                name: theme.name,
                action: () => {
                    if (isDarkTheme) {
                        settings.darkTheme = theme.name;
                    } else {
                        settings.theme = theme.name;
                    }

                    updateSettings(fetch, settings);

                    if (settings.followSystemTheme) {
                        const themeName =
                            $systemThemeStore === "dark" ? settings.darkTheme : settings.theme;
                        const theme = themes.find((t) => t.name === themeName);
                        themeStore.set(theme!);
                    } else if (!isDarkTheme) {
                        themeStore.set(theme);
                    }

                    return Close.yes;
                }
            });
        }

        return commands;
    };

    const onPasteViewClicked = () => {
        setTempCommands(getPasteViewCommands());
        cmdPalTitle.set("select default paste view");
        cmdPalOpen.set(true);
    };

    const onThemeClicked = () => {
        setTempCommands(getThemeCommands(false));
        cmdPalTitle.set("select the theme");
        cmdPalOpen.set(true);
    };

    const onDarkThemeClicked = () => {
        setTempCommands(getThemeCommands(true));
        cmdPalTitle.set("select the theme");
        cmdPalOpen.set(true);
    };

    const onFollowSystemThemeChanged = () => {
        updateSettings(fetch, settings);

        const themeName = settings.followSystemTheme
            ? $systemThemeStore === "dark"
                ? settings.darkTheme
                : settings.theme
            : settings.theme;

        const theme = themes.find((t) => t.name === themeName);

        if (theme) {
            themeStore.set(theme);
        }
    };
</script>

<svelte:head>
    <title>pastemyst | appearance settings</title>
    <meta property="og:title" content="pastemyst | appearance settings" />
    <meta property="twitter:title" content="pastemyst | appearance settings" />
</svelte:head>

<h3>appearance settings</h3>

<p>various settings to customize the look and feel of pastemyst.</p>

<h4>general</h4>

<div class="flex row center gap-s">
    <p>theme:</p>
    <button onclick={onThemeClicked}>{settings.theme}</button>
</div>

<span class="hint">change the theme of pastemyst</span>

<div class="flex row center gap-s">
    <Checkbox
        label="follow system theme"
        bind:checked={settings.followSystemTheme}
        onchange={onFollowSystemThemeChanged}
    />
</div>

<span class="hint">sets the theme based on your system theme</span>

{#if settings.followSystemTheme}
    <div class="flex row center gap-s">
        <p>dark theme:</p>
        <button onclick={onDarkThemeClicked}>{settings.darkTheme}</button>
    </div>

    <span class="hint">change the dark theme of pastemyst</span>
{/if}

<h4>paste</h4>

<div class="flex row center gap-s">
    <p>paste view:</p>
    <button onclick={onPasteViewClicked}>{settings.pasteView}</button>
</div>

<span class="hint">change how the files look when viewing a paste: tabbed or stacked</span>

<style lang="scss">
    p {
        margin: 0;
    }

    .hint {
        color: var(--color-bg3);
        font-size: $fs-small;
        display: block;
        margin-bottom: 1rem;
    }
</style>
