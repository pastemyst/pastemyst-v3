<script lang="ts">
    import { fade } from "svelte/transition";
    import { removeToast, toasts } from "./toasts.svelte";
</script>

<div class="toaster flex col gap-m">
    {#each toasts as { id, message, type }}
        <button
            class="toast flex row center gap-m {type}"
            transition:fade
            onclick={() => removeToast(id)}
        >
            <div class="icon">
                {#if type === "error"}
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <path
                            fill="currentColor"
                            d="M2.344 2.343h-.001a8 8 0 0 1 11.314 11.314A8.002 8.002 0 0 1 .234 10.089a8 8 0 0 1 2.11-7.746Zm1.06 10.253a6.5 6.5 0 1 0 9.108-9.275 6.5 6.5 0 0 0-9.108 9.275ZM6.03 4.97 8 6.94l1.97-1.97a.749.749 0 0 1 1.275.326.749.749 0 0 1-.215.734L9.06 8l1.97 1.97a.749.749 0 0 1-.326 1.275.749.749 0 0 1-.734-.215L8 9.06l-1.97 1.97a.749.749 0 0 1-1.275-.326.749.749 0 0 1 .215-.734L6.94 8 4.97 6.03a.751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018Z"
                        /></svg
                    >
                {:else if type === "info"}
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <path
                            fill="currentColor"
                            d="M0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8Zm8-6.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13ZM6.5 7.75A.75.75 0 0 1 7.25 7h1a.75.75 0 0 1 .75.75v2.75h.25a.75.75 0 0 1 0 1.5h-2a.75.75 0 0 1 0-1.5h.25v-2h-.25a.75.75 0 0 1-.75-.75ZM8 6a1 1 0 1 1 0-2 1 1 0 0 1 0 2Z"
                        />
                    </svg>
                {:else if type === "success"}
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                        <path
                            fill="currentColor"
                            d="M0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8Zm1.5 0a6.5 6.5 0 1 0 13 0 6.5 6.5 0 0 0-13 0Zm10.28-1.72-4.5 4.5a.75.75 0 0 1-1.06 0l-2-2a.751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018l1.47 1.47 3.97-3.97a.751.751 0 0 1 1.042.018.751.751 0 0 1 .018 1.042Z"
                        />
                    </svg>
                {/if}
            </div>
            <div class="message">
                {message}
            </div>
        </button>
    {/each}
</div>

<style lang="scss">
    .toaster {
        position: fixed;
        top: 20px;
        left: 50%;
        z-index: 100;
        transform: translateX(-50%);

        .toast {
            margin: 0 auto;
            background-color: var(--color-bg2);
            font-size: $fs-medium;
            padding: 0.5rem 1rem;
            border-radius: $border-radius;
            border: 1px solid var(--color-bg3);
            @include shadow-big();

            &.info {
                border-color: var(--color-secondary);

                .icon {
                    color: var(--color-secondary);
                }
            }

            &.error {
                border-color: var(--color-danger);

                .icon {
                    color: var(--color-danger);
                }
            }

            &.success {
                border-color: var(--color-success);

                .icon {
                    color: var(--color-success);
                }
            }
        }
    }
</style>
