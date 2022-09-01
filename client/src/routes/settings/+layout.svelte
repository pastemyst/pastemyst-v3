<script lang="ts">
    import type { SettingsContext } from "$lib/api/settings";
    import type { LayoutData } from "./$types";

    export let data: LayoutData;

    let settingsContext: SettingsContext = data.self ? "profile" : "browser";
</script>

<section class="settings-header flex sm-row space-between center">
    <h2>settings</h2>

    <div class="settings-context flex col">
        <div class="flex row center">
            <button
                class:active={settingsContext === "browser" && data.category !== "profile"}
                class="browser"
                disabled={data.category === "profile"}
                on:click={() => (settingsContext = "browser")}
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="icon">
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M0 3.75C0 2.784.784 2 1.75 2h20.5c.966 0 1.75.784 1.75 1.75v16.5A1.75 1.75 0 0122.25 22H1.75A1.75 1.75 0 010 20.25V3.75zm1.75-.25a.25.25 0 00-.25.25V5.5h4v-2H1.75zM7 3.5v2h4v-2H7zm5.5 0v2h10V3.75a.25.25 0 00-.25-.25H12.5zm10 3.5h-21v13.25c0 .138.112.25.25.25h20.5a.25.25 0 00.25-.25V7z"
                    />
                </svg>
                browser
            </button>

            <button
                class:active={settingsContext === "profile" || data.category === "profile"}
                class="profile"
                disabled={data.category === "profile"}
                on:click={() => (settingsContext = "profile")}
            >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="icon">
                    <path
                        fill="currentColor"
                        fill-rule="evenodd"
                        d="M12 2.5a5.5 5.5 0 00-3.096 10.047 9.005 9.005 0 00-5.9 8.18.75.75 0 001.5.045 7.5 7.5 0 0114.993 0 .75.75 0 101.499-.044 9.005 9.005 0 00-5.9-8.181A5.5 5.5 0 0012 2.5zM8 8a4 4 0 118 0 4 4 0 01-8 0z"
                    />
                </svg>
                profile
            </button>
        </div>

        {#if settingsContext === "profile" || data.category === "profile"}
            <p>settings will be saved on your profile</p>
        {:else if settingsContext === "browser"}
            <p>settings will be saved in your browser</p>
        {/if}
    </div>
</section>

<div class="flex sm-row">
    <section class="categories flex sm-col">
        <nav>
            {#if data.self}
                <ul>
                    <li>
                        <a
                            href="/settings/profile"
                            class="btn"
                            class:active={data.category === "profile"}
                        >
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                viewBox="0 0 24 24"
                                class="icon"
                            >
                                <path
                                    fill="currentColor"
                                    fill-rule="evenodd"
                                    d="M12 2.5a5.5 5.5 0 00-3.096 10.047 9.005 9.005 0 00-5.9 8.18.75.75 0 001.5.045 7.5 7.5 0 0114.993 0 .75.75 0 101.499-.044 9.005 9.005 0 00-5.9-8.181A5.5 5.5 0 0012 2.5zM8 8a4 4 0 118 0 4 4 0 01-8 0z"
                                />
                            </svg>
                            profile
                        </a>
                    </li>
                </ul>
            {/if}

            <ul>
                <li>
                    <a
                        href="/settings/behaviour"
                        class="btn"
                        class:active={data.category === "behaviour"}
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                            <path
                                fill="currentColor"
                                d="M15 2.75a.75.75 0 01-.75.75h-4a.75.75 0 010-1.5h4a.75.75 0 01.75.75zm-8.5.75v1.25a.75.75 0 001.5 0v-4a.75.75 0 00-1.5 0V2H1.75a.75.75 0 000 1.5H6.5zm1.25 5.25a.75.75 0 000-1.5h-6a.75.75 0 000 1.5h6zM15 8a.75.75 0 01-.75.75H11.5V10a.75.75 0 11-1.5 0V6a.75.75 0 011.5 0v1.25h2.75A.75.75 0 0115 8zm-9 5.25v-2a.75.75 0 00-1.5 0v1.25H1.75a.75.75 0 000 1.5H4.5v1.25a.75.75 0 001.5 0v-2zm9 0a.75.75 0 01-.75.75h-6a.75.75 0 010-1.5h6a.75.75 0 01.75.75z"
                            />
                        </svg>
                        behaviour
                    </a>
                </li>
            </ul>

            <ul>
                <li>
                    <a
                        href="/settings/appearance"
                        class="btn"
                        class:active={data.category === "appearance"}
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" class="icon">
                            <path
                                fill="currentColor"
                                fill-rule="evenodd"
                                d="M11.134 1.535C9.722 2.562 8.16 4.057 6.889 5.312 5.8 6.387 5.041 7.401 4.575 8.294a3.745 3.745 0 00-3.227 1.054c-.43.431-.69 1.066-.86 1.657a11.982 11.982 0 00-.358 1.914A21.263 21.263 0 000 15.203v.054l.75-.007-.007.75h.054a14.404 14.404 0 00.654-.012 21.243 21.243 0 001.63-.118c.62-.07 1.3-.18 1.914-.357.592-.17 1.226-.43 1.657-.861a3.745 3.745 0 001.055-3.217c.908-.461 1.942-1.216 3.04-2.3 1.279-1.262 2.764-2.825 3.775-4.249.501-.706.923-1.428 1.125-2.096.2-.659.235-1.469-.368-2.07-.606-.607-1.42-.55-2.069-.34-.66.213-1.376.646-2.076 1.155zm-3.95 8.48a3.76 3.76 0 00-1.19-1.192 9.758 9.758 0 011.161-1.607l1.658 1.658a9.853 9.853 0 01-1.63 1.142zM.742 16l.007-.75-.75.008A.75.75 0 00.743 16zM12.016 2.749c-1.224.89-2.605 2.189-3.822 3.384l1.718 1.718c1.21-1.205 2.51-2.597 3.387-3.833.47-.662.78-1.227.912-1.662.134-.444.032-.551.009-.575h-.001V1.78c-.014-.014-.112-.113-.548.027-.432.14-.995.462-1.655.942zM1.62 13.089a19.56 19.56 0 00-.104 1.395 19.55 19.55 0 001.396-.104 10.528 10.528 0 001.668-.309c.526-.151.856-.325 1.011-.48a2.25 2.25 0 00-3.182-3.182c-.155.155-.329.485-.48 1.01a10.515 10.515 0 00-.309 1.67z"
                            />
                        </svg>
                        appearance
                    </a>
                </li>
            </ul>
        </nav>
    </section>

    <section class="settings">
        <slot />
    </section>
</div>

<style lang="scss">
    .settings-header {
        margin-bottom: 0;

        h2 {
            margin: 0;
        }

        .settings-context {
            align-items: flex-start;
            margin-top: 0.5rem;

            p {
                margin: 0;
                margin-right: 0.5rem;
                margin-top: 0.25rem;
                font-size: $fs-small;
                color: var(--color-bg3);
            }

            button {
                &.browser {
                    border-top-right-radius: 0;
                    border-bottom-right-radius: 0;
                }

                &.profile {
                    border-top-left-radius: 0;
                    border-bottom-left-radius: 0;
                }

                &.active {
                    border-color: var(--color-primary);
                }

                .icon {
                    margin-right: 0.5rem;
                }
            }
        }
    }

    .categories {
        margin-bottom: 0;
        height: 100%;

        nav {
            width: 100%;

            ul {
                margin: 0;
                margin-bottom: 1rem;
                padding: 0;
                list-style: none;

                &:last-child {
                    margin: 0;
                }

                .btn {
                    .icon {
                        margin-right: 0.5rem;
                    }

                    &.active {
                        border-color: var(--color-primary);
                    }
                }
            }
        }
    }

    .settings {
        flex-grow: 1;
        height: 100%;
        padding-top: 0;
        padding-bottom: 0;
    }

    @media screen and (min-width: $break-med) {
        .settings-header {
            .settings-context {
                align-items: flex-end;
                margin-top: 0;
            }
        }

        .categories {
            min-width: 256px;
            max-width: 33.33%;
            margin-right: 2rem;
        }
    }
</style>
