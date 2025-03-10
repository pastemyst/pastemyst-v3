<script lang="ts">
    import { cmdPalOpen, cmdPalTitle, currentUserStore } from "./stores";
    import { tooltip } from "./tooltips";
    import { env } from "$env/dynamic/public";
    import { isMacOs } from "./utils/userAgent";
    import { API_URL } from "./api/fetch";

    const isAlphaRelease = env.PUBLIC_ALPHA_RELEASE === "true";

    const conditionalUseTooltip = isAlphaRelease ? tooltip : () => ({});

    const onMenuClick = () => {
        cmdPalTitle.set(null);
        cmdPalOpen.set(true);
    };
</script>

<header class="flex row center space-between">
    <a
        href="/"
        class:alpha={isAlphaRelease}
        class="title btn"
        aria-label={isAlphaRelease
            ? "this is an alpha release of pastemyst, use only for testing, things will break and data will get lost"
            : undefined}
        use:conditionalUseTooltip
    >
        <img src="/images/pastemyst.svg" alt="pastemyst logo" />

        <h1>pastemyst{isAlphaRelease ? "-alpha" : ""}</h1>

        {#if isAlphaRelease}
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Danger Icon</title>
                <path
                    fill="currentColor"
                    d="M6.457 1.047c.659-1.234 2.427-1.234 3.086 0l6.082 11.378A1.75 1.75 0 0 1 14.082 15H1.918a1.75 1.75 0 0 1-1.543-2.575Zm1.763.707a.25.25 0 0 0-.44 0L1.698 13.132a.25.25 0 0 0 .22.368h12.164a.25.25 0 0 0 .22-.368Zm.53 3.996v2.5a.75.75 0 0 1-1.5 0v-2.5a.75.75 0 0 1 1.5 0ZM9 11a1 1 0 1 1-2 0 1 1 0 0 1 2 0Z"
                />
            </svg>
        {/if}
    </a>

    <div class="flex row center">
        {#if $currentUserStore != null}
            <a href="/~{$currentUserStore.username}" class="nav-item user btn btn-icon">
                <img
                    src="{API_URL}/images/{$currentUserStore.avatarId}"
                    alt="{$currentUserStore.username}'s avatar"
                />
            </a>
        {:else}
            <a href="/login" class="btn btn-icon nav-item" aria-label="login/register" use:tooltip>
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                    <title>Login Icon</title>
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M2 2.75C2 1.784 2.784 1 3.75 1h2.5a.75.75 0 010 1.5h-2.5a.25.25 0 00-.25.25v10.5c0 .138.112.25.25.25h2.5a.75.75 0 010 1.5h-2.5A1.75 1.75 0 012 13.25V2.75zm6.56 4.5l1.97-1.97a.75.75 0 10-1.06-1.06L6.22 7.47a.75.75 0 000 1.06l3.25 3.25a.75.75 0 101.06-1.06L8.56 8.75h5.69a.75.75 0 000-1.5H8.56z"
                    />
                </svg>
            </a>
        {/if}

        <a
            class="btn nav-item btn-icon"
            href={$currentUserStore !== null ? "/settings/profile" : "/settings/behaviour"}
            aria-label="settings"
            use:tooltip
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M7.429 1.525a6.593 6.593 0 011.142 0c.036.003.108.036.137.146l.289 1.105c.147.56.55.967.997 1.189.174.086.341.183.501.29.417.278.97.423 1.53.27l1.102-.303c.11-.03.175.016.195.046.219.31.41.641.573.989.014.031.022.11-.059.19l-.815.806c-.411.406-.562.957-.53 1.456a4.588 4.588 0 010 .582c-.032.499.119 1.05.53 1.456l.815.806c.08.08.073.159.059.19a6.494 6.494 0 01-.573.99c-.02.029-.086.074-.195.045l-1.103-.303c-.559-.153-1.112-.008-1.529.27-.16.107-.327.204-.5.29-.449.222-.851.628-.998 1.189l-.289 1.105c-.029.11-.101.143-.137.146a6.613 6.613 0 01-1.142 0c-.036-.003-.108-.037-.137-.146l-.289-1.105c-.147-.56-.55-.967-.997-1.189a4.502 4.502 0 01-.501-.29c-.417-.278-.97-.423-1.53-.27l-1.102.303c-.11.03-.175-.016-.195-.046a6.492 6.492 0 01-.573-.989c-.014-.031-.022-.11.059-.19l.815-.806c.411-.406.562-.957.53-1.456a4.587 4.587 0 010-.582c.032-.499-.119-1.05-.53-1.456l-.815-.806c-.08-.08-.073-.159-.059-.19a6.44 6.44 0 01.573-.99c.02-.029.086-.075.195-.045l1.103.303c.559.153 1.112.008 1.529-.27.16-.107.327-.204.5-.29.449-.222.851-.628.998-1.189l.289-1.105c.029-.11.101-.143.137-.146zM8 0c-.236 0-.47.01-.701.03-.743.065-1.29.615-1.458 1.261l-.29 1.106c-.017.066-.078.158-.211.224a5.994 5.994 0 00-.668.386c-.123.082-.233.09-.3.071L3.27 2.776c-.644-.177-1.392.02-1.82.63a7.977 7.977 0 00-.704 1.217c-.315.675-.111 1.422.363 1.891l.815.806c.05.048.098.147.088.294a6.084 6.084 0 000 .772c.01.147-.038.246-.088.294l-.815.806c-.474.469-.678 1.216-.363 1.891.2.428.436.835.704 1.218.428.609 1.176.806 1.82.63l1.103-.303c.066-.019.176-.011.299.071.213.143.436.272.668.386.133.066.194.158.212.224l.289 1.106c.169.646.715 1.196 1.458 1.26a8.094 8.094 0 001.402 0c.743-.064 1.29-.614 1.458-1.26l.29-1.106c.017-.066.078-.158.211-.224a5.98 5.98 0 00.668-.386c.123-.082.233-.09.3-.071l1.102.302c.644.177 1.392-.02 1.82-.63.268-.382.505-.789.704-1.217.315-.675.111-1.422-.364-1.891l-.814-.806c-.05-.048-.098-.147-.088-.294a6.1 6.1 0 000-.772c-.01-.147.039-.246.088-.294l.814-.806c.475-.469.679-1.216.364-1.891a7.992 7.992 0 00-.704-1.218c-.428-.609-1.176-.806-1.82-.63l-1.103.303c-.066.019-.176.011-.299-.071a5.991 5.991 0 00-.668-.386c-.133-.066-.194-.158-.212-.224L10.16 1.29C9.99.645 9.444.095 8.701.031A8.094 8.094 0 008 0zm1.5 8a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0zM11 8a3 3 0 11-6 0 3 3 0 016 0z"
                />
            </svg>
        </a>

        <button
            class="nav-item btn-icon"
            onclick={onMenuClick}
            aria-label={`menu (${isMacOs() ? "cmd" : "ctrl"}+k)`}
            role="menu"
            use:tooltip
        >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                <title>Menu Icon</title>
                <path
                    fill="currentColor"
                    fill-rule="evenodd"
                    d="M1 2.75A.75.75 0 011.75 2h12.5a.75.75 0 110 1.5H1.75A.75.75 0 011 2.75zm0 5A.75.75 0 011.75 7h12.5a.75.75 0 110 1.5H1.75A.75.75 0 011 7.75zM1.75 12a.75.75 0 100 1.5h12.5a.75.75 0 100-1.5H1.75z"
                />
            </svg>
        </button>
    </div>
</header>

<style lang="scss">
    header {
        margin-top: 1rem;
        flex-shrink: 0;
    }

    .title {
        text-decoration: none;

        &.alpha {
            border-color: var(--color-danger);

            h1 {
                color: var(--color-danger);
            }

            .icon {
                margin-left: 0.5rem;
                color: var(--color-danger);
            }
        }

        h1 {
            font-size: $fs-normal;
            font-weight: normal;
            margin: 0;
            margin-top: -2px;
            color: var(--color-primary);
        }

        img {
            width: 24px;
            height: 24px;
            margin: 0;
            margin-right: 0.5rem;
        }
    }

    .nav-item {
        font-size: $fs-large;
        margin-left: 1rem;

        &.user {
            font-size: $fs-normal;
            text-decoration: none;
            word-break: break-word;
            padding: 0;

            img {
                width: 34px;
                height: 34px;
                margin: 0;
                border-radius: $border-radius;
            }
        }
    }
</style>
