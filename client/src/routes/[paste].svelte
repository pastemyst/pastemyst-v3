<script lang="ts" context="module">
    import { apiBase } from "$lib/api/api";
    import { fetcherGet } from "$lib/api/fetcher";
    import type { Paste } from "$lib/api/paste";
    import { tooltip } from "$lib/tooltips";

    export const router = false;

    export const load = async ({ params }) => {
        const res = await fetcherGet<Paste>(`${apiBase}/paste/${params.paste}`);

        return {
            status: res.ok,
            props: {
                paste: res.ok ? res.data : null
            }
        };
    };
</script>

<script lang="ts">
    export let paste: Paste;
</script>

<section class="paste-header flex column center space-between">
    <h2>{paste.title}</h2>

    <div class="options flex row center">
        <div class="btn stars" aria-label="stars" use:tooltip>
            <svg class="icon" xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><title>ionicons-v5-e</title><path fill="currentColor" d="M394,480a16,16,0,0,1-9.39-3L256,383.76,127.39,477a16,16,0,0,1-24.55-18.08L153,310.35,23,221.2A16,16,0,0,1,32,192H192.38l48.4-148.95a16,16,0,0,1,30.44,0l48.4,149H480a16,16,0,0,1,9.05,29.2L359,310.35l50.13,148.53A16,16,0,0,1,394,480Z"/></svg>
            <p>54</p>
        </div>

        <div class="btn" aria-label="edit" use:tooltip>
            <svg class="icon" xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><title>ionicons-v5-n</title><path fill="currentColor" d="M459.94,53.25a16.06,16.06,0,0,0-23.22-.56L424.35,65a8,8,0,0,0,0,11.31l11.34,11.32a8,8,0,0,0,11.34,0l12.06-12C465.19,69.54,465.76,59.62,459.94,53.25Z"/><path fill="currentColor" d="M399.34,90,218.82,270.2a9,9,0,0,0-2.31,3.93L208.16,299a3.91,3.91,0,0,0,4.86,4.86l24.85-8.35a9,9,0,0,0,3.93-2.31L422,112.66A9,9,0,0,0,422,100L412.05,90A9,9,0,0,0,399.34,90Z"/><path fill="currentColor" d="M386.34,193.66,264.45,315.79A41.08,41.08,0,0,1,247.58,326l-25.9,8.67a35.92,35.92,0,0,1-44.33-44.33l8.67-25.9a41.08,41.08,0,0,1,10.19-16.87L318.34,125.66A8,8,0,0,0,312.69,112H104a56,56,0,0,0-56,56V408a56,56,0,0,0,56,56H344a56,56,0,0,0,56-56V199.31A8,8,0,0,0,386.34,193.66Z"/></svg>
        </div>

        <div class="btn" aria-label="copy link" use:tooltip>
            <svg class="icon" xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><title>ionicons-v5-o</title><path stroke="currentColor" d="M200.66,352H144a96,96,0,0,1,0-192h55.41" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:48px"/><path stroke="currentColor" d="M312.59,160H368a96,96,0,0,1,0,192H311.34" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:48px"/><line stroke="currentColor" x1="169.07" y1="256" x2="344.93" y2="256" style="fill:none;stroke-linecap:round;stroke-linejoin:round;stroke-width:48px"/></svg>
        </div>

        <div class="btn" aria-label="clone paste" use:tooltip>
            <svg class="icon" xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><title>ionicons-v5-j</title><path fill="currentColor" d="M408,112H184a72,72,0,0,0-72,72V408a72,72,0,0,0,72,72H408a72,72,0,0,0,72-72V184A72,72,0,0,0,408,112ZM375.55,312H312v63.55c0,8.61-6.62,16-15.23,16.43A16,16,0,0,1,280,376V312H216.45c-8.61,0-16-6.62-16.43-15.23A16,16,0,0,1,216,280h64V216.45c0-8.61,6.62-16,15.23-16.43A16,16,0,0,1,312,216v64h64a16,16,0,0,1,16,16.77C391.58,305.38,384.16,312,375.55,312Z"/><path fill="currentColor" d="M395.88,80A72.12,72.12,0,0,0,328,32H104a72,72,0,0,0-72,72V328a72.12,72.12,0,0,0,48,67.88V160a80,80,0,0,1,80-80Z"/></svg>
        </div>

        <div class="btn" aria-label="more options" use:tooltip>
            <svg class="icon" xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><title>ionicons-v5-f</title><circle fill="currentColor" cx="256" cy="256" r="48"/><circle fill="currentColor" cx="416" cy="256" r="48"/><circle fill="currentColor" cx="96" cy="256" r="48"/></svg>
        </div>
    </div>
</section>

<div class="lang-stats flex">
    <div class="lang d" aria-label="D 50%" use:tooltip></div>
    <div class="lang java" aria-label="Java 50%" use:tooltip></div>
</div>

<style lang="scss">
    .paste-header {
        padding: 0.75rem 1rem;
        margin-bottom: 0;
        border-bottom-left-radius: 0;
        border-bottom-right-radius: 0;
        border-bottom: none;

        h2 {
            margin: 0;
            font-weight: normal;
            font-size: $fs-medium;
        }

        .options {
            .btn {
                background-color: $color-bg;
                margin-left: 0.5rem;

                svg {
                    max-width: 20px;
                    max-height: 20px;
                }
            }

            .stars {
                p {
                    margin: 0;
                    margin-left: 0.5rem;
                }
            }
        }
    }

    .lang-stats {
        height: 5px;

        .lang {
            border-right: 2px solid $color-bg;

            &:first-child {
                border-bottom-left-radius: $border-radius;
            }

            &:last-child {
                border-bottom-right-radius: $border-radius;
                border-right: none;
            }
        }

        .d {
            width: 50%;
            background-color: #ba595e;
        }

        .java {
            width: 50%;
            background-color: #b07219;
        }
    }

    @media screen and (max-width: $break-med) {
        .paste-header {
            flex-direction: column;
            align-items: baseline;

            h2 {
                margin-bottom: 1rem;
            }

            .options {
                .btn:first-child {
                    margin-left: 0;
                }
            }
        }
    }
</style>
