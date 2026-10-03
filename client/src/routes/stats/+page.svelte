<script lang="ts">
    import { onMount } from "svelte";
    import type { PageData } from "./$types";
    import {
        BarController,
        BarElement,
        Chart,
        Filler,
        Legend,
        LinearScale,
        LineController,
        LineElement,
        PointElement,
        TimeScale,
        Tooltip,
        type ChartOptions
    } from "chart.js";
    import "chartjs-adapter-date-fns";
    import { format } from "date-fns";

    interface Props {
        data: PageData;
    }

    let { data }: Props = $props();

    const numberFormat = new Intl.NumberFormat("en-US");
    const fmt = (n: number) => numberFormat.format(n);

    // A few hand-made v2 pastes have a 1970 creation date; they count towards the totals but
    // would stretch the time axis back 50 years.
    const weeks = $derived(
        data.weeklyPasteStats.filter((w) => new Date(w.date).getUTCFullYear() >= 2000)
    );

    // v2 didn't record deletions/expirations, so active only differs from total from v3 on.
    const showActive = $derived(weeks.some((w) => w.total !== w.active));

    const years = $derived.by(() => {
        const byYear: Record<number, { year: number; created: number; total: number }> = {};
        for (const w of weeks) {
            const year = new Date(w.date).getUTCFullYear();
            byYear[year] ??= { year, created: 0, total: 0 };
            byYear[year].created += w.created;
            byYear[year].total = w.total;
        }
        return Object.values(byYear).sort((a, b) => a.year - b.year);
    });

    let weeklyCanvas: HTMLCanvasElement;
    let totalCanvas: HTMLCanvasElement;

    // Theme colours, 8% darker for the marks: the raw accents are a bit too light for chart
    // marks on dark themes (checked with a palette validator), and it helps contrast on light ones.
    const darken = (color: string, alpha = 1, amount = 0.92): string => {
        const hex = color.trim().match(/^#([0-9a-f]{6})$/i);
        if (!hex) return color.trim();
        const [r, g, b] = [0, 2, 4].map((i) =>
            Math.round(parseInt(hex[1].slice(i, i + 2), 16) * amount)
        );
        return `rgba(${r}, ${g}, ${b}, ${alpha})`;
    };

    const themeColors = (element: HTMLElement) => {
        const style = getComputedStyle(element);
        const v = (name: string) => style.getPropertyValue(name).trim();
        return {
            primary: darken(v("--color-primary")),
            primaryWash: darken(v("--color-primary"), 0.1),
            secondary: darken(v("--color-secondary")),
            fg: v("--color-fg"),
            muted: v("--color-bg3"),
            grid: v("--color-bg2"),
            surface: v("--color-bg1")
        };
    };

    // same stack as the site ($font-stack), so the canvas falls back to monospace, not serif
    const fontFamily = '"Ubuntu Mono", monospace';

    const baseOptions = (c: ReturnType<typeof themeColors>, legend: boolean): ChartOptions => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        interaction: { mode: "index", intersect: false },
        plugins: {
            legend: {
                display: legend,
                align: "start",
                labels: { color: c.fg, boxWidth: 16, boxHeight: 2, font: { family: fontFamily } }
            },
            tooltip: {
                backgroundColor: c.grid,
                titleColor: c.fg,
                bodyColor: c.fg,
                titleFont: { family: fontFamily },
                bodyFont: { family: fontFamily, weight: "bold" },
                boxWidth: 8,
                boxHeight: 2,
                callbacks: {
                    title: (items) =>
                        `week of ${format(new Date(items[0].parsed.x!), "MMM d, yyyy")}`,
                    label: (item) => ` ${fmt(item.parsed.y!)} ${item.dataset.label}`
                }
            }
        },
        scales: {
            x: {
                type: "time",
                // same range and padding on both charts, so their dates line up vertically
                min: weeks[0]?.date.toString(),
                max: weeks[weeks.length - 1]?.date.toString(),
                offset: true,
                time: { unit: "year", tooltipFormat: "MMM d, yyyy" },
                grid: { display: false },
                border: { color: c.grid },
                ticks: { color: c.muted, maxRotation: 0, font: { family: fontFamily } }
            },
            y: {
                beginAtZero: true,
                // same width on both charts, so their plot areas (and dates) line up exactly
                afterFit: (scale) => {
                    scale.width = 64;
                },
                grid: { color: c.grid, lineWidth: 1 },
                border: { display: false },
                ticks: {
                    color: c.muted,
                    font: { family: fontFamily },
                    callback: (value) => fmt(Number(value))
                }
            }
        }
    });

    const renderCharts = () => {
        const c = themeColors(weeklyCanvas);
        const dates = weeks.map((w) => new Date(w.date));

        const weekly = new Chart(weeklyCanvas, {
            type: "bar",
            data: {
                labels: dates,
                datasets: [
                    {
                        label: "new pastes",
                        data: weeks.map((w) => w.created),
                        backgroundColor: c.primary,
                        borderRadius: { topLeft: 1, topRight: 1 },
                        borderSkipped: "bottom",
                        barPercentage: 1,
                        categoryPercentage: 0.8
                    }
                ]
            },
            options: baseOptions(c, false) as ChartOptions<"bar">
        });

        const datasets = [
            {
                label: "total pastes",
                data: weeks.map((w) => w.total),
                borderColor: c.primary,
                backgroundColor: c.primaryWash,
                fill: !showActive ? "origin" : false,
                borderWidth: 2,
                pointRadius: 0,
                pointHoverRadius: 4,
                pointHoverBorderWidth: 2,
                pointHoverBorderColor: c.surface,
                pointHoverBackgroundColor: c.primary,
                tension: 0
            }
        ];

        if (showActive) {
            datasets.push({
                ...datasets[0],
                label: "active pastes",
                data: weeks.map((w) => w.active),
                borderColor: c.secondary,
                backgroundColor: c.secondary,
                pointHoverBackgroundColor: c.secondary,
                fill: false
            });
        }

        const total = new Chart(totalCanvas, {
            type: "line",
            data: { labels: dates, datasets },
            options: baseOptions(c, showActive) as ChartOptions<"line">
        });

        return () => {
            weekly.destroy();
            total.destroy();
        };
    };

    onMount(() => {
        Chart.register(
            BarController,
            BarElement,
            LineController,
            LineElement,
            PointElement,
            Filler,
            LinearScale,
            TimeScale,
            Tooltip,
            Legend
        );

        let destroy = renderCharts();
        let unmounted = false;

        // canvas text doesn't update when the web font arrives, so redraw once it has loaded
        if (document.fonts.status !== "loaded") {
            document.fonts.ready.then(() => {
                if (unmounted) return;
                destroy();
                destroy = renderCharts();
            });
        }

        // Redraw with the new colours when the theme changes
        const themeContext = document.getElementById("theme-context");
        const observer = new MutationObserver(() => {
            destroy();
            destroy = renderCharts();
        });
        if (themeContext) observer.observe(themeContext, { attributeFilter: ["data-theme"] });

        return () => {
            unmounted = true;
            observer.disconnect();
            destroy();
        };
    });
</script>

<svelte:head>
    <title>pastemyst | stats</title>
    <meta property="og:title" content="pastemyst | stats" />
    <meta property="twitter:title" content="pastemyst | stats" />
</svelte:head>

<section>
    <h1>stats</h1>

    <div class="tiles">
        <div class="tile">
            <span class="value">{fmt(data.totalPastes)}</span>
            <span class="label">total pastes</span>
        </div>
        <div class="tile">
            <span class="value">{fmt(data.activePastes)}</span>
            <span class="label">active pastes</span>
        </div>
        <div class="tile">
            <span class="value">{fmt(data.totalUsers)}</span>
            <span class="label">total users</span>
        </div>
        <div class="tile">
            <span class="value">{fmt(data.activeUsers)}</span>
            <span class="label">active users</span>
        </div>
    </div>

    <div class="chart">
        <h2>new pastes per week</h2>
        <div class="canvas-wrapper">
            <canvas bind:this={weeklyCanvas} aria-label="new pastes per week"></canvas>
        </div>
    </div>

    <div class="chart">
        <h2>{showActive ? "pastes over time" : "total pastes over time"}</h2>
        <div class="canvas-wrapper">
            <canvas bind:this={totalCanvas} aria-label="total pastes over time"></canvas>
        </div>
    </div>

    <details>
        <summary>pastes per year</summary>
        <table>
            <thead>
                <tr>
                    <th>year</th>
                    <th>new pastes</th>
                    <th>total at year end</th>
                </tr>
            </thead>
            <tbody>
                {#each years as row (row.year)}
                    <tr>
                        <td>{row.year}</td>
                        <td>{fmt(row.created)}</td>
                        <td>{fmt(row.total)}</td>
                    </tr>
                {/each}
            </tbody>
        </table>
    </details>
</section>

<style lang="scss">
    h1 {
        margin-top: 0;
    }

    h2 {
        font-size: $fs-normal;
        font-weight: normal;
        color: var(--color-fg);
        margin: 0 0 0.75rem 0;
    }

    .tiles {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 1rem;
        margin-bottom: 1.5rem;

        @media screen and (max-width: $break-med) {
            grid-template-columns: repeat(2, 1fr);
        }
    }

    .tile {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        padding: 1rem;
        background-color: var(--color-bg);
        border: 1px solid var(--color-bg2);
        border-radius: $border-radius;

        .value {
            font-size: 2rem;
            font-weight: bold;
            color: var(--color-fg);
        }

        .label {
            font-size: $fs-small;
            color: var(--color-bg3);
        }
    }

    .chart {
        padding: 1rem;
        margin-bottom: 1rem;
        background-color: var(--color-bg);
        border: 1px solid var(--color-bg2);
        border-radius: $border-radius;
    }

    // fixed height for the whole chart incl. axis labels (Chart.js draws them inside the canvas)
    .canvas-wrapper {
        position: relative;
        height: 260px;
    }

    details {
        summary {
            cursor: pointer;
            color: var(--color-bg3);
            font-size: $fs-small;
        }

        table {
            margin-top: 0.75rem;
            border-collapse: collapse;
            font-size: $fs-small;
            font-variant-numeric: tabular-nums;

            th,
            td {
                padding: 0.25rem 1.5rem 0.25rem 0;
                text-align: right;
                border-bottom: 1px solid var(--color-bg2);

                &:first-child {
                    text-align: left;
                }
            }

            th {
                font-weight: normal;
                color: var(--color-bg3);
            }
        }
    }
</style>
