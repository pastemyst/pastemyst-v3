<script lang="ts">
    import { page } from "$app/stores";
</script>

<section class="markdown">
    <h1>error {$page.status}</h1>

    {#if $page.status === 404}
        <p>not found</p>
    {:else}
        <p>{$page.error?.message}</p>
    {/if}

    <p>you can go back to the <a href="/">home page</a></p>
</section>

