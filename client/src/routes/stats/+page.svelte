<script lang="ts">
    import { onMount } from "svelte";
    import type { PageData } from "./$types";
    import {
        Chart,
        Legend,
        LinearScale,
        LineController,
        LineElement,
        PointElement,
        TimeScale,
        Title,
        Tooltip
    } from "chart.js";
    import "chartjs-adapter-date-fns";

    interface Props {
        data: PageData;
    }

    let { data }: Props = $props();

    let canvasElement: HTMLCanvasElement;

    onMount(() => {
        Chart.register(
            LineController,
            LineElement,
            PointElement,
            LinearScale,
            TimeScale,
            Title,
            Tooltip,
            Legend
        );

        new Chart(canvasElement, {
            type: "line",
            data: {
                labels: data.weeklyPasteStats.map((s) => new Date(s.date)),
                datasets: [
                    {
                        label: "total pastes",
                        data: data.weeklyPasteStats.map((s) => s.total),
                        borderColor: "rgb(238, 114, 13)",
                        tension: 0.6,
                        cubicInterpolationMode: "monotone",
                        pointRadius: 0
                    },
                    {
                        label: "active pastes",
                        data: data.weeklyPasteStats.map((s) => s.active),
                        borderColor: "rgb(30, 174, 219)",
                        tension: 0.6,
                        cubicInterpolationMode: "monotone",
                        pointRadius: 0
                    }
                ]
            },
            options: {
                responsive: true,
                interaction: {
                    mode: "index",
                    intersect: false
                },
                scales: {
                    x: {
                        type: "time",
                        time: {
                            unit: "week"
                        },
                        title: {
                            display: true,
                            text: "date"
                        },
                        min: "2019-01-01T00:00:00Z"
                    },
                    y: {
                        title: {
                            display: true,
                            text: "pastes"
                        }
                    }
                }
            }
        });
    });
</script>

<svelte:head>
    <title>pastemyst | stats</title>
    <meta property="og:title" content="pastemyst | stats" />
    <meta property="twitter:title" content="pastemyst | stats" />
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

    <canvas bind:this={canvasElement}></canvas>
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
