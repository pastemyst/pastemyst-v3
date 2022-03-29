<section class="centered">
    <h2>login / register</h2>

    <p>
        here you can create a pastemyst account, or login to an existing one. accounts provide a lot
        of functionality, biggest ones are: viewing all your pastes (including tagging), private
        pastes, editing pastes and much more.
    </p>

    <ul>
        <li>
            <a href="/api/v3/auth-web/login/github" rel="external">
                <ion-icon name="logo-github" />
                <span>github</span>
            </a>
        </li>

        <li>
            <a href="/api/v3/auth-web/login/gitlab" rel="external">
                <ion-icon name="logo-gitlab" />
                <span>gitlab</span>
            </a>
        </li>

        <li>
            <a href="/api/v3/auth-web/login/google" rel="external">
                <ion-icon name="logo-google" />
                <span>google</span>
            </a>
        </li>
    </ul>
</section>

<style lang="scss">
    ul {
        list-style: none;
        padding-left: 10%;
        padding-right: 10%;

        li {
            margin-top: 2rem;

            a {
                background-color: $color-bg;
                padding: 1rem;
                text-decoration: none;
                display: flex;
                flex-direction: row;
                align-items: center;
                border-radius: $border-radius;
                margin: 0 auto;
                justify-content: center;
                gap: 1rem;
            }
        }
    }
</style>
