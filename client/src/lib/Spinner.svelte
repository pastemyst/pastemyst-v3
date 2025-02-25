<span></span>

<style lang="scss">
    span {
        width: 20px;
        height: 20px;
        border: 2px solid var(--color-bg2);
        border-bottom-color: transparent;
        border-radius: 50%;
        display: inline-block;
        box-sizing: border-box;
        animation: rotation 0.5s linear infinite;
        display: flex;
        margin: 0 auto;
    }

    @keyframes rotation {
        0% {
            transform: rotate(0deg);
        }

        100% {
            transform: rotate(360deg);
        }
    }
</style>
