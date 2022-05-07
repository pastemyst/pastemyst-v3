<footer class="flex sm-row space-between center">
    <span class="flex row center copy-wrap">
        <span class="copy">©</span>
        <span><a href="https://myst.rs/" rel="external" class="no-dec">codemyst</a></span>
        <span>{new Date().getFullYear()}</span>
    </span>

    <span>v3.0.0-alpha.1</span>

    <span><a href="/" class="no-dec">500 total pastes</a></span>
</footer>

<style lang="scss">
    footer {
        flex-shrink: 0;
        padding: 1rem 0rem;
        margin-top: 2rem;
        font-size: $fs-small;
        border-top: 2px solid $color-bg-1;

        .copy-wrap span {
            margin-right: 0.35rem;
        }

        .copy {
            font-size: $fs-large;
            margin-top: -3px;
        }
    }
</style>
