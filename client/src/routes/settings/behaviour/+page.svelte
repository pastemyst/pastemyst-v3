<script lang="ts">
    import { getLocalSettings, updateSettings } from "$lib/api/settings";
    import { Close, setTempCommands, type Command } from "$lib/command";
    import { cmdPalOpen, cmdPalTitle, settingsContextStore } from "$lib/stores";
    import type { PageData } from "./$types";

    export let data: PageData;

    $: settings =
        $settingsContextStore === "profile" && data.settings ? data.settings : getLocalSettings();

    const getIndentUnitCommands = (): Command[] => {
        return [
            {
                name: "spaces",
                action: () => {
                    settings.defaultIndentationUnit = "spaces";
                    cmdPalTitle.set("select indentation width (spaces)");
                    setTempCommands(getIndentWidthCommands());

                    updateSettings(fetch, $settingsContextStore, settings);

                    return Close.no;
                }
            },
            {
                name: "tabs",
                action: () => {
                    settings.defaultIndentationUnit = "tabs";
                    cmdPalTitle.set("select indentation width (tabs)");
                    setTempCommands(getIndentWidthCommands());

                    updateSettings(fetch, $settingsContextStore, settings);

                    return Close.no;
                }
            }
        ];
    };

    const getIndentWidthCommands = (): Command[] => {
        const commands: Command[] = [];

        for (let i = 1; i <= 8; i++) {
            commands.push({
                name: i.toString(),
                action: () => {
                    settings.defaultIndentationWidth = i;

                    updateSettings(fetch, $settingsContextStore, settings);

                    return Close.yes;
                }
            });
        }

        return commands;
    };

    const onDefaultIndentationClicked = () => {
        setTempCommands(getIndentUnitCommands());

        cmdPalTitle.set("select indentation unit");
        cmdPalOpen.set(true);
    };
</script>

<svelte:head>
    <title>pastemyst | behaviour settings</title>
</svelte:head>

<h3>behaviour settings</h3>

<p>various settings to customize the behaviour of pastemyst.</p>

<h4>editor</h4>

<div class="flex row center gap-s">
    <p>default indentation:</p>
    <button on:click={onDefaultIndentationClicked}
        >{settings.defaultIndentationUnit}: {settings.defaultIndentationWidth}</button
    >
</div>

<span class="hint"> set the default indentation unit and width for the text editor </span>

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
