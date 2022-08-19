<script lang="ts">
    import { apiBase } from "$lib/api/api";
</script>

<section class="centered">
    <h2>login / register</h2>

    <p>
        here you can create a pastemyst account, or login to an existing one. accounts provide a lot
        of functionality, biggest ones are: viewing all your pastes (including tagging), private
        pastes, editing pastes and much more.
    </p>

    <ul>
        <li>
            <a href="{apiBase}/login/github" rel="external" class="btn">
                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="64"
                    height="64"
                    shape-rendering="geometricPrecision"
                    text-rendering="geometricPrecision"
                    image-rendering="optimizeQuality"
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    viewBox="0 0 640 640"
                    class="icon"
                >
                    <path
                        fill="currentColor"
                        d="M319.988 7.973C143.293 7.973 0 151.242 0 327.96c0 141.392 91.678 261.298 218.826 303.63 16.004 2.964 21.886-6.957 21.886-15.414 0-7.63-.319-32.835-.449-59.552-89.032 19.359-107.8-37.772-107.8-37.772-14.552-36.993-35.529-46.831-35.529-46.831-29.032-19.879 2.209-19.442 2.209-19.442 32.126 2.245 49.04 32.954 49.04 32.954 28.56 48.922 74.883 34.76 93.131 26.598 2.882-20.681 11.15-34.807 20.315-42.803-71.08-8.067-145.797-35.516-145.797-158.14 0-34.926 12.52-63.485 32.965-85.88-3.33-8.078-14.291-40.606 3.083-84.674 0 0 26.87-8.61 88.029 32.8 25.512-7.075 52.878-10.642 80.056-10.76 27.2.118 54.614 3.673 80.162 10.76 61.076-41.386 87.922-32.8 87.922-32.8 17.398 44.08 6.485 76.631 3.154 84.675 20.516 22.394 32.93 50.953 32.93 85.879 0 122.907-74.883 149.93-146.117 157.856 11.481 9.921 21.733 29.398 21.733 59.233 0 42.792-.366 77.28-.366 87.804 0 8.516 5.764 18.473 21.992 15.354 127.076-42.354 218.637-162.274 218.637-303.582 0-176.695-143.269-319.988-320-319.988l-.023.107z"
                    />
                </svg>
                <span>github</span>
            </a>
        </li>

        <li>
            <a href="{apiBase}/login/gitlab" rel="external" class="btn">
                <svg
                    width="32px"
                    height="32px"
                    viewBox="0 0 32 32"
                    xmlns="http://www.w3.org/2000/svg"
                    class="icon"
                >
                    <path
                        fill="currentColor"
                        d="M 8.382813 1.972656 L 4.078125 13.453125 L 3.835938 14.105469 L 1.796875 19.542969 L 16 29.875 L 30.203125 19.542969 L 28.164063 14.105469 L 23.613281 1.972656 L 19.882813 13.453125 L 12.117188 13.453125 Z M 8.25 8.027344 L 10.015625 13.453125 L 6.214844 13.453125 Z M 23.75 8.027344 L 25.785156 13.453125 L 21.984375 13.453125 Z M 5.464844 15.453125 L 10.664063 15.453125 L 14.09375 26.015625 L 4.203125 18.820313 Z M 12.765625 15.453125 L 19.234375 15.453125 L 16 25.402344 Z M 21.335938 15.453125 L 26.53125 15.453125 L 27.796875 18.820313 L 17.902344 26.015625 Z"
                    />
                </svg>
                <span>gitlab</span>
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
                flex-direction: row;
                margin: 0 auto;
                justify-content: center;
                gap: 1rem;
            }
        }
    }
</style>
