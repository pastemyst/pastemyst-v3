<script lang="ts">
    import type { PageData } from "./$types";

    export let data: PageData;
</script>

<svelte:head>
    <title>pastemyst | stats</title>
</svelte:head>

<section>
    <h1>stats</h1>

    <div class="stat-wrapper">
        <ul>
            <li><span>total pastes</span><span>{data.totalPastes}</span></li>
            <li><span>active pastes</span><span>{data.activePastes}</span></li>
        </ul>

        <ul>
            <li><span>total users</span><span>{data.totalUsers}</span></li>
            <li><span>active users</span><span>{data.activeUsers}</span></li>
        </ul>
    </div>
</section>

<style lang="scss">
    h1 {
        margin-top: 0;
    }

    .stat-wrapper {
        width: fit-content;
    }

    ul {
        list-style: none;
        padding: 0;
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1rem;

        li {
            background-color: var(--color-bg);
            margin-right: 1rem;
            padding: 0.25rem 0.5rem;
            border: 1px solid var(--color-bg2);
            border-radius: $border-radius;
            display: flex;
            flex-direction: row;

            span {
                &:first-child {
                    margin-right: 1.5rem;
                }

                &:last-child {
                    margin-left: auto;
                }
            }
        }
    }
</style>
