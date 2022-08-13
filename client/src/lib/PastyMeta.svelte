<script lang="ts">
    import type { LangStat, Language } from "./api/lang";
    import type { Pasty, Stats } from "./api/paste";
    import { humanFileSize } from "./strings";
    import { tooltip } from "$lib/tooltips";

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
        }, 1000);
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
        <button
            aria-label="copy content"
            use:tooltip={{ content: copied ? "copied" : "copy content", hideOnClick: false }}
            on:click={onCopyClick}
        >
            <svg xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 512 512">
                <title>Clipboard</title>
                <path
                    d="M336 64h32a48 48 0 0148 48v320a48 48 0 01-48 48H144a48 48 0 01-48-48V112a48 48 0 0148-48h32"
                    fill="none"
                    stroke="currentColor"
                    stroke-linejoin="round"
                    stroke-width="32"
                />
                <rect
                    x="176"
                    y="32"
                    width="160"
                    height="64"
                    rx="26.13"
                    ry="26.13"
                    fill="none"
                    stroke="currentColor"
                    stroke-linejoin="round"
                    stroke-width="32"
                />
            </svg>
        </button>
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
    }

    button {
        background-color: $color-bg;

        .icon {
            width: 20px;
            height: 20px;
        }
    }
</style>
