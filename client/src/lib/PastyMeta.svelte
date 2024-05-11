<script lang="ts">
    import type { LangStat, Language } from "./api/lang";
    import type { Paste, Pasty, Stats } from "./api/paste";
    import { humanFileSize } from "./strings";
    import { tooltip } from "$lib/tooltips";
    import { isLanguageMarkdown } from "./utils/markdown";

    export let paste: Paste;
    export let pasty: Pasty;
    export let langStats: LangStat[];
    export let stats: Stats;
    export let previewMarkdown = true;

    let copied = false;

    // returns the full lang from the name
    // uses the already fetched langStats which already hold the full lang
    const getLanguage = (name: string): Language | undefined => {
        return langStats.find((s) => s.language.name === name)?.language;
    };

    const onCopyClick = async () => {
        navigator.clipboard.writeText(pasty.content);

        copied = true;

        setTimeout(() => {
            copied = false;
        }, 2000);
    };

    const onMarkdownPreviewClick = () => {
        previewMarkdown = !previewMarkdown;
    };
</script>

<div class="stats flex row center">
    <div class="meta flex row center">
        <span>
            {getLanguage(pasty.language)?.name}
        </span>

        <span>
            {stats.lines} lines
        </span>

        <span>
            {stats.words} words
        </span>

        <span>
            {humanFileSize(stats.bytes).toLowerCase()}
        </span>
    </div>

    <div class="buttons flex row center">
        {#if isLanguageMarkdown(pasty.language)}
            <button
                aria-label="view markdown code"
                use:tooltip
                class:enabled={previewMarkdown}
                on:click={onMarkdownPreviewClick}
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Markdown Icon</title>
                    <path
                        fill="currentColor"
                        d="M14.85 3c.63 0 1.15.52 1.14 1.15v7.7c0 .63-.51 1.15-1.15 1.15H1.15C.52 13 0 12.48 0 11.84V4.15C0 3.52.52 3 1.15 3ZM9 11V5H7L5.5 7 4 5H2v6h2V8l1.5 1.92L7 8v3Zm2.99.5L14.5 8H13V5h-2v3H9.5Z"
                    />
                </svg>
            </button>
        {/if}

        <button
            aria-label="copy content"
            on:click={onCopyClick}
            use:tooltip={{ content: copied ? "copied" : "copy content", hideOnClick: false }}
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Copy Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 010 1.5h-1.5a.25.25 0 00-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 00.25-.25v-1.5a.75.75 0 011.5 0v1.5A1.75 1.75 0 019.25 16h-7.5A1.75 1.75 0 010 14.25v-7.5z"
                />
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0114.25 11h-7.5A1.75 1.75 0 015 9.25v-7.5zm1.75-.25a.25.25 0 00-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 00.25-.25v-7.5a.25.25 0 00-.25-.25h-7.5z"
                />
            </svg>
        </button>

        {#if !paste.private}
            <a
                class="btn"
                href="/raw/{paste.id}/{pasty.id}"
                aria-label="view raw content"
                use:tooltip
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>File Code Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M4 1.75C4 .784 4.784 0 5.75 0h5.586c.464 0 .909.184 1.237.513l2.914 2.914c.329.328.513.773.513 1.237v8.586A1.75 1.75 0 0114.25 15h-9a.75.75 0 010-1.5h9a.25.25 0 00.25-.25V6h-2.75A1.75 1.75 0 0110 4.25V1.5H5.75a.25.25 0 00-.25.25v2.5a.75.75 0 01-1.5 0v-2.5zm7.5-.188V4.25c0 .138.112.25.25.25h2.688a.252.252 0 00-.011-.013l-2.914-2.914a.272.272 0 00-.013-.011zM5.72 6.72a.75.75 0 000 1.06l1.47 1.47-1.47 1.47a.75.75 0 101.06 1.06l2-2a.75.75 0 000-1.06l-2-2a.75.75 0 00-1.06 0zM3.28 7.78a.75.75 0 00-1.06-1.06l-2 2a.75.75 0 000 1.06l2 2a.75.75 0 001.06-1.06L1.81 9.25l1.47-1.47z"
                    />
                </svg>
            </a>
        {/if}
    </div>
</div>

<style lang="scss">
    .stats {
        font-size: $fs-normal;
        color: var(--color-bg3);
        padding-left: 0.5rem;
    }

    .meta {
        span::after {
            content: "|";
            opacity: 0.3;
            font-size: $fs-medium;
            margin: 0 0.25rem;
        }

        span:last-child::after {
            content: "";
        }
    }

    .buttons {
        margin-left: auto;

        button,
        .btn {
            margin-left: 0.5rem;

            &.enabled {
                color: var(--color-secondary);
                border-color: var(--color-secondary);

                .icon {
                    color: var(--color-secondary);
                }
            }
        }
    }

    button,
    .btn {
        background-color: var(--color-bg);

        &:hover {
            background-color: var(--color-bg2);
        }
    }
</style>
