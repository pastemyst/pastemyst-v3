<script lang="ts">
    export let tags: string[];
    export let ownerUsername: string;
</script>

<div class="tags flex row center">
    {#each tags as tag}
        <a href="/~{ownerUsername}?tag={tag}" class="btn">{tag}</a>
    {/each}
</div>

<style lang="scss">
    .tags {
        font-size: $fs-small;
        gap: 0.5rem;
        flex-wrap: wrap;
        margin-top: 1rem;
    }
</style>
