<script lang="ts">
    import { formatDistanceToNow } from "date-fns";
    import type { PageData } from "./$types";
    import { tooltip } from "$lib/tooltips";
    import {
        deleteAccessToken,
        generateAccessToken,
        getAccessTokens,
        type GenerateAccessTokenResponse
    } from "$lib/api/auth";
    import { Close, getConfirmActionCommands, setTempCommands, type Command } from "$lib/command";
    import { cmdPalOpen, cmdPalTitle } from "$lib/stores";
    import { ExpiresIn, expiresInToLongString } from "$lib/api/paste";
    import Checkbox from "$lib/Checkbox.svelte";

    interface Props {
        data: PageData;
    }

    let { data = $bindable() }: Props = $props();

    let accessTokens = $state(data.accessTokens);

    let creatingAccessToken = $state(false);
    let description: string | undefined = $state(undefined);
    let expiresIn: ExpiresIn = $state(ExpiresIn.oneMonth);
    let scopePaste: boolean = $state(false);
    let scopePasteRead: boolean = $state(false);
    let scopeUser: boolean = $state(false);
    let scopeUserRead: boolean = $state(false);
    let scopeUserAccessTokens: boolean = $state(false);

    let newAccessTokenResponse: GenerateAccessTokenResponse | undefined = $state(undefined);

    const onCreateAccessTokenStart = () => {
        creatingAccessToken = true;
    };

    const onCreateAccessTokenCancel = () => {
        creatingAccessToken = false;
    };

    const onCreateAccessTokenExpiresInClick = () => {
        const commands: Command[] = [];

        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        for (const [_, exp] of Object.entries(ExpiresIn)) {
            commands.push({
                name: expiresInToLongString(exp),
                description: exp.toString(),
                action: () => {
                    expiresIn = exp;
                    return Close.yes;
                }
            });
        }

        setTempCommands(commands);
        cmdPalTitle.set("select when the access token will expire");
        cmdPalOpen.set(true);
    };

    const onCreateAccessToken = async () => {
        const scopes = [];
        if (scopePaste) {
            scopes.push("paste");
        }
        if (scopePasteRead) {
            scopes.push("paste:read");
        }
        if (scopeUser) {
            scopes.push("user");
        }
        if (scopeUserRead) {
            scopes.push("user:read");
        }
        if (scopeUserAccessTokens) {
            scopes.push("user:access_tokens");
        }

        newAccessTokenResponse = await generateAccessToken(fetch, {
            scopes,
            description,
            expiresIn
        });

        creatingAccessToken = false;
        description = "";
        expiresIn = ExpiresIn.oneMonth;
        scopePaste = false;
        scopePasteRead = false;
        scopeUser = false;
        scopeUserRead = false;
        scopeUserAccessTokens = false;

        accessTokens = await getAccessTokens(fetch);
    };

    const onDelete = async (id: string) => {
        setTempCommands(
            getConfirmActionCommands(() => {
                (async () => {
                    await deleteAccessToken(fetch, id);

                    accessTokens = await getAccessTokens(fetch);
                })();

                return Close.yes;
            })
        );

        cmdPalTitle.set(
            "are you sure you want to delete this access token? all future api requests using this access token will fail. this action can't be undone."
        );
        cmdPalOpen.set(true);
    };
</script>

<svelte:head>
    <title>pastemyst | access tokens</title>
    <meta property="og:title" content="pastemyst | access tokens" />
    <meta property="twitter:title" content="pastemyst | access tokens" />
</svelte:head>

<h3>access tokens</h3>

<p>here you can list and create your own access token to access the pastemyst api.</p>

<button class="new-access-token-btn" onclick={onCreateAccessTokenStart}
    >create new access token</button
>

{#if creatingAccessToken}
    <div class="new-access-token block flex col">
        <label for="description">description:</label>
        <input type="text" name="description" id="description" bind:value={description} />

        <p>expires in:</p>
        <button onclick={onCreateAccessTokenExpiresInClick}
            >{expiresInToLongString(expiresIn)}</button
        >

        <p>scopes:</p>
        <div class="scopes flex col">
            <div class="scope">
                <Checkbox label="paste" bind:checked={scopePaste} />
            </div>
            <div class="scope">
                <Checkbox label="paste:read" bind:checked={scopePasteRead} />
            </div>
            <div class="scope">
                <Checkbox label="user" bind:checked={scopeUser} />
            </div>
            <div class="scope">
                <Checkbox label="user:read" bind:checked={scopeUserRead} />
            </div>
            <div class="scope">
                <Checkbox label="user:access_tokens" bind:checked={scopeUserAccessTokens} />
            </div>
        </div>

        <div class="buttons flex row gap-m">
            <button onclick={onCreateAccessToken}>generate</button>
            <button onclick={onCreateAccessTokenCancel}>cancel</button>
        </div>
    </div>
{/if}

{#if newAccessTokenResponse}
    <div class="new-access-token-success block">
        <p>successfully created a new access token</p>
        <p><code>{newAccessTokenResponse.accessToken}</code></p>
        <p>copy it now since you won't be able to view the access token later!</p>
    </div>
{/if}

{#each accessTokens as accessToken}
    <div class="access-token block">
        <p>description: {accessToken.description || "<no description>"}</p>
        <p use:tooltip aria-label={new Date(accessToken.createdAt).toString()}>
            created {formatDistanceToNow(new Date(accessToken.createdAt), { addSuffix: true })}
        </p>
        {#if accessToken.expiresAt}
            <p use:tooltip aria-label={new Date(accessToken.expiresAt).toString()}>
                expires {formatDistanceToNow(new Date(accessToken.expiresAt), { addSuffix: true })}
            </p>
        {:else}
            <p>doesn't expire</p>
        {/if}
        <p>
            scopes:
            {#each accessToken.scopes as scope, index}
                <code>{scope}</code>{index < accessToken.scopes.length - 1 ? ", " : ""}
            {/each}
        </p>
        <button class="btn-danger" onclick={() => onDelete(accessToken.id)}>delete</button>
    </div>
{/each}

<style lang="scss">
    .new-access-token-success {
        background-color: var(--color-bg);
        border-color: var(--color-success);
        padding: 1rem;
        font-size: $fs-normal;
        margin-bottom: 1rem;

        p {
            margin: 0.5rem 0;
        }

        code {
            background-color: var(--color-bg1);
        }
    }

    .new-access-token {
        background-color: var(--color-bg);
        padding: 1rem;
        font-size: $fs-normal;
        line-height: 1.5;
        margin-bottom: 1rem;

        p {
            margin: 0;
            margin-top: 1rem;
        }

        input,
        button {
            padding: 0.25rem 0.5rem;
            background-color: var(--color-bg1);
        }

        .buttons {
            margin-top: 1rem;
        }
    }

    .new-access-token-btn {
        margin-bottom: 1rem;
    }

    .access-token {
        background-color: var(--color-bg);
        padding: 1rem;
        margin-bottom: 1rem;
        line-height: 1.5;
        font-size: $fs-normal;

        p {
            margin: 0;
        }

        button {
            margin-top: 1rem;
        }

        code {
            background-color: var(--color-bg1);
        }
    }
</style>
