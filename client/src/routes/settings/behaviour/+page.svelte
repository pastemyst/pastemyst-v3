<script lang="ts">
    import Checkbox from "$lib/Checkbox.svelte";
    import { getLangs, getPopularLangNames } from "$lib/api/lang";
    import { updateSettings } from "$lib/api/settings";
    import { Close, setTempCommands, type Command } from "$lib/command";
    import { cmdPalOpen, cmdPalTitle } from "$lib/stores";
    import type { PageData } from "./$types";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let settings = $state(data.settings);

    const getIndentUnitCommands = (): Command[] => {
        return [
            {
                name: "spaces",
                action: () => {
                    settings.defaultIndentationUnit = "spaces";
                    cmdPalTitle.set("select indentation width (spaces)");

                    setTempCommands(getIndentWidthCommands());

                    updateSettings(fetch, settings);

                    return Close.no;
                }
            },
            {
                name: "tabs",
                action: () => {
                    settings.defaultIndentationUnit = "tabs";
                    cmdPalTitle.set("select indentation width (tabs)");
                    setTempCommands(getIndentWidthCommands());

                    updateSettings(fetch, settings);

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

                    updateSettings(fetch, settings);

                    return Close.yes;
                }
            });
        }

        return commands;
    };

    const getLanguageCommands = async (): Promise<Command[]> => {
        const commands: Command[] = [];

        const langs = await getLangs(fetch);

        const popularLangs = await getPopularLangNames(fetch);

        // make sure popular languages are at the top
        langs.sort((a, b) => {
            const aPopular = popularLangs.includes(a.name) ? 1 : 0;
            const bPopular = popularLangs.includes(b.name) ? 1 : 0;

            return bPopular - aPopular;
        });

        const textLangIndex = langs.findIndex((l) => l.name === "Text");
        const textLang = langs[textLangIndex];

        // place text on the top of the lang list below autodetect
        langs.splice(textLangIndex, 1);
        langs.unshift(textLang);

        const autodetectLangIndex = langs.findIndex((l) => l.name === "Autodetect");
        const autodetectLang = langs[autodetectLangIndex];

        // place autodetect on the top of the lang list
        langs.splice(autodetectLangIndex, 1);
        langs.unshift(autodetectLang);

        for (const lang of langs) {
            commands.push({
                name: lang.name,
                description: lang.aliases?.join(", "),
                action: () => {
                    settings.defaultLanguage = lang.name;

                    updateSettings(fetch, settings);

                    return Close.yes;
                }
            });
        }

        return commands;
    };

    const onDefaultLanguageClicked = async () => {
        setTempCommands(await getLanguageCommands());

        cmdPalTitle.set("select language");
        cmdPalOpen.set(true);
    };

    const onDefaultIndentationClicked = () => {
        setTempCommands(getIndentUnitCommands());

        cmdPalTitle.set("select indentation unit");
        cmdPalOpen.set(true);
    };

    const onTextWrapClicked = async () => {
        await updateSettings(fetch, settings);
    };

    const onCopyLinkOnCreateClicked = async () => {
        await updateSettings(fetch, settings);
    };
</script>

<svelte:head>
    <title>pastemyst | behaviour settings</title>
</svelte:head>

<h3>behaviour settings</h3>

<p>various settings to customize the behaviour of pastemyst.</p>

<h4>editor</h4>

<div class="flex row center gap-s">
    <p>default language:</p>
    <button onclick={onDefaultLanguageClicked}>{settings.defaultLanguage}</button>
</div>

<span class="hint">set the default language for the text editor</span>

<div class="flex row center gap-s">
    <p>default indentation:</p>
    <button onclick={onDefaultIndentationClicked}
        >{settings.defaultIndentationUnit}: {settings.defaultIndentationWidth}</button
    >
</div>

<span class="hint">set the default indentation unit and width for the text editor</span>

<div class="flex row center gap-s">
    <Checkbox
        label="text wrap"
        bind:checked={settings.textWrap}
        onchange={onTextWrapClicked}
    />
</div>

<span class="hint">text wrapping in the editor</span>

<div class="flex row center gap-s">
    <Checkbox
        label="copy link on create"
        bind:checked={settings.copyLinkOnCreate}
        onchange={onCopyLinkOnCreateClicked}
    />
</div>

<span class="hint">copies the link to the paste when you create a paste</span>

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
