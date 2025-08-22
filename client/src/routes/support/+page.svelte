<svelte:head>
    <title>pastemyst | support</title>
    <meta property="og:title" content="pastemyst | support" />
    <meta property="twitter:title" content="pastemyst | support" />
</svelte:head>

<section>
    <h1>support pastemyst</h1>

    <p>
        pastemyst is completely free and open source, meaning that this project doesn't generate
        much money.
    </p>

    <p>
        if you like pastemyst (and my <a href="https://github.com/codemyst">other projects</a>) and
        want to support my work, you can either donate some money to help cover the running costs of
        the services, or contribute directly by creating an
        <a href="https://github.com/pastemyst/pastemyst-v3/issues">issue</a>
        or <a href="https://github.com/pastemyst/pastemyst-v3/pulls">pull request</a>.
    </p>

    <p>
        you can donate on my <a href="https://github.com/sponsors/CodeMyst">github sponsors page</a>
        or on my <a href="https://ko-fi.com/codemyst">ko-fi page</a>. i can also receive crypto, but
        you will need to send me an email first (<a href="mailto:code@myst.rs">code@myst.rs</a>).
    </p>

    <p>
        if you donate to the project or contribute by getting a PR merged, you will get a little
        badge on your profile.
    </p>

    <div class="badge-images flex row gap-m">
        <img src="/images/contributor-example.png" alt="contributor badge example" />

        <img src="/images/supporter-example.png" alt="supporter badge example" />
    </div>
</section>

<style lang="scss">
    .badge-images {
        max-width: 60%;
        margin: 0 auto;

        img {
            width: 100%;
            border: 1px solid var(--color-bg2);
            border-radius: $border-radius;
        }
    }

    @media screen and (max-width: $break-med) {
        .badge-images {
            max-width: 100%;
        }
    }
</style>
