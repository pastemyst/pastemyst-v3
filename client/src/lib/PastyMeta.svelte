<script lang="ts">
    import type { LangStat, Language } from "./api/lang";
    import type { Pasty } from "./api/paste";

    export let pasty: Pasty;
    export let langStats: LangStat[];

    // returns the full lang from the name
    // uses the already fetched langStats which already hold the full lang
    const getLanguage = (name: string): Language | undefined => {
        return langStats.find((s) => s.language.name === name)?.language;
    };
</script>

<span class="lang">
    {getLanguage(pasty.language)?.name}
</span>

<style lang="scss">
    .lang {
        border-radius: $border-radius;
        font-size: $fs-normal;
        padding: 0 0.5rem;
        color: $color-bg-3;
    }
</style>
