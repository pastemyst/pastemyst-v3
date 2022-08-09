<script lang="ts">
    import type { LangStat, Language } from "./api/lang";
    import type { Pasty, Stats } from "./api/paste";
import { humanFileSize } from "./strings";

    export let pasty: Pasty;
    export let langStats: LangStat[];
    export let stats: Stats;

    // returns the full lang from the name
    // uses the already fetched langStats which already hold the full lang
    const getLanguage = (name: string): Language | undefined => {
        return langStats.find((s) => s.language.name === name)?.language;
    };
</script>

<div class="stats">
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

<style lang="scss">
    .stats {
        font-size: $fs-normal;
        color: $color-bg-3;
        padding-left: 0.5rem;
    }

    span::after {
        content: ' |';
        opacity: 0.3;
        font-size: $fs-medium;
    }

    span:last-child::after {
        content: '';
    }
</style>
