<script lang="ts">
    import type { ActionData } from "./$types";

    let { form }: { form: ActionData } = $props();
</script>

<svelte:head>
    <title>pastemyst | encrypted paste</title>
    <meta property="og:title" content="pastemyst | encrypted paste" />
    <meta property="twitter:title" content="pastemyst | encrypted paste" />
</svelte:head>

<section class="block">
    <h2>encrypted paste</h2>
    <p>this paste is encrypted, please enter the encryption password:</p>
    {#if form?.success === false}
        <p class="error-message">wrong encryption password</p>
    {/if}
    <form method="POST" class="flex col gap-m">
        <input
            type="password"
            name="encryptionKey"
            id="encryptionKey"
            required
            placeholder="encryption password"
        />
        <input class="btn btn-main" type="submit" value="decrypt" />
    </form>
</section>
