<script lang="ts">
    import { getLocalSettings, updateSettings, type Settings } from "$lib/api/settings";
    import type { PageData } from "./$types";
    import { cmdPalOpen, cmdPalTitle, settingsContextStore } from "$lib/stores";
    import { Close, setTempCommands, type Command } from "$lib/command";

    export let data: PageData;

    let settings: Settings;

    settingsContextStore.subscribe((context) => {
        settings = context === "profile" && data.settings ? data.settings : getLocalSettings();
    });

    const getPasteViewCommands = (): Command[] => {
        return [
            {
                name: "tabbed",
                action: () => {
                    settings.pasteView = "tabbed";
                    updateSettings(fetch, $settingsContextStore, settings);

                    return Close.yes;
                }
            },
            {
                name: "stacked",
                action: () => {
                    settings.pasteView = "stacked";
                    updateSettings(fetch, $settingsContextStore, settings);

                    return Close.yes;
                }
            }
        ];
    };

    const onPasteViewClicked = () => {
        setTempCommands(getPasteViewCommands());
        cmdPalTitle.set("select default paste view");
        cmdPalOpen.set(true);
    };
</script>

<svelte:head>
    <title>pastemyst | appearance settings</title>
</svelte:head>

<h3>appearance settings</h3>

<p>various settings to customize the look and feel of pastemyst.</p>

<h4>paste</h4>

<div class="flex row center gap-s">
    <p>paste view:</p>
    <button on:click={onPasteViewClicked}>{settings.pasteView}</button>
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
