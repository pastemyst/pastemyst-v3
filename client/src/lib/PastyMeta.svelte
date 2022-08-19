<script lang="ts">
    import type { LangStat, Language } from "./api/lang";
    import type { Paste, Pasty, Stats } from "./api/paste";
    import { humanFileSize } from "./strings";
    import { tooltip } from "$lib/tooltips";

    export let paste: Paste;
    export let pasty: Pasty;
    export let langStats: LangStat[];
    export let stats: Stats;

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
            {humanFileSize(stats.size).toLowerCase()}
        </span>
    </div>

    <div class="buttons flex row center">
        <button aria-label="copy content" use:tooltip on:click={onCopyClick}>
            {copied ? "copied" : "copy"}
        </button>

        {#if !paste.private}
            <a
                class="btn"
                href="/raw/{paste.id}/{pasty.id}"
                aria-label="view raw content"
                use:tooltip>raw</a
            >
        {/if}
    </div>
</div>

<style lang="scss">
    .stats {
        font-size: $fs-normal;
        color: $color-bg-3;
        padding-left: 0.5rem;
    }

    .meta {
        margin-right: 1rem;

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
        }
    }

    button,
    .btn {
        background-color: $color-bg;
    }
</style>
