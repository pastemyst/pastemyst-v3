<script lang="ts">
    import { updateSettings } from "$lib/api/settings";
    import type { PageData } from "./$types";
    import { cmdPalOpen, cmdPalTitle, themeStore } from "$lib/stores";
    import { Close, setTempCommands, type Command } from "$lib/command";
    import { themes } from "$lib/themes";

    export let data: PageData;

    const getPasteViewCommands = (): Command[] => {
        return [
            {
                name: "tabbed",
                action: () => {
                    data.settings.pasteView = "tabbed";
                    updateSettings(fetch, data.settings);

                    return Close.yes;
                }
            },
            {
                name: "stacked",
                action: () => {
                    data.settings.pasteView = "stacked";
                    updateSettings(fetch, data.settings);

                    return Close.yes;
                }
            }
        ];
    };

    const getThemeCommands = (): Command[] => {
        const commands: Command[] = [];
        for (const theme of themes) {
            commands.push({
                name: theme.name,
                action: () => {
                    data.settings.theme = theme.name;
                    updateSettings(fetch, data.settings);

                    themeStore.set(theme);

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
        setTempCommands(getThemeCommands());
        cmdPalTitle.set("select the theme");
        cmdPalOpen.set(true);
    };
</script>

<svelte:head>
    <title>pastemyst | appearance settings</title>
</svelte:head>

<h3>appearance settings</h3>

<p>various settings to customize the look and feel of pastemyst.</p>

<h4>general</h4>

<div class="flex row center gap-s">
    <p>theme:</p>
    <button on:click={onThemeClicked}>{data.settings.theme}</button>
</div>

<span class="hint">change the theme of pastemyst</span>

<h4>paste</h4>

<div class="flex row center gap-s">
    <p>paste view:</p>
    <button on:click={onPasteViewClicked}>{data.settings.pasteView}</button>
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
