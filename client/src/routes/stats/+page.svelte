<script lang="ts">
    import { onMount } from "svelte";
    import type { PageData } from "./$types";
    import * as frappe from "frappe-charts";

    export let data: PageData;

    onMount(() => {
        const labels = Object.keys(data.activePastesOverTime).map((d) =>
            new Date(d).toDateString()
        );

        const chartData = {
            labels: labels,
            datasets: [
                {
                    name: "active pastes",
                    chartType: "line",
                    values: Object.values(data.activePastesOverTime)
                },
                {
                    name: "total pastes",
                    chartType: "line",
                    values: Object.values(data.totalPastesOverTime)
                }
            ]
        };

        new frappe.Chart("#chart", {
            data: chartData,
            type: "line",
            colors: ["#fff"],
            lineOptions: {
                hideDots: 1,
                spline: 1
            }
        });
    });
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

    <div id="chart" />
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

    :global(#chart) {
        background-color: var(--color-bg);
        border-radius: $border-radius;
        border: 1px solid var(--color-bg2);

        :global(.dataset-1 .line-graph-path) {
            stroke: var(--color-primary) !important;
        }

        :global(.dataset-0 .line-graph-path) {
            stroke: var(--color-secondary) !important;
        }

        :global(text),
        :global(span) {
            font-size: $fs-small;
            font-family: $font-stack;
            font-weight: normal;
            text-transform: lowercase;
        }

        :global(.chart-legend) {
            :global(g:first-child .legend-bar) {
                fill: var(--color-secondary);
            }

            :global(g:last-child .legend-bar) {
                fill: var(--color-primary);
            }
        }

        :global(.graph-svg-tip .data-point-list) {
            :global(li:first-child) {
                border-top: 3px solid var(--color-secondary) !important;
            }

            :global(li:last-child) {
                border-top: 3px solid var(--color-primary) !important;
            }
        }

        :global(.y.axis) {
            stroke: var(--color-bg3);
            font-family: $font-stack;

            :global(line) {
                opacity: 0;
            }
        }

        :global(.x.axis) {
            :global(line) {
                opacity: 0;
            }

            :global(text) {
                fill: var(--color-bg3);
            }
        }
    }
</style>
