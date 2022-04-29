<script lang="ts">
    import { tooltip } from "$lib/tooltips";
</script>

<div class="paste-options block flex sm-row center space-between">
    <div class="options flex row center">
        <input type="checkbox" id="encrypt" />
        <label
            for="encrypt"
            aria-label="encrypt the paste with a password"
            class="btn btn-square btn-plain"
            use:tooltip
        >
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <path
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    d="M6 8C4.34315 8 3 9.34315 3 11V13C3 14.6569 4.34315 16 6 16C7.65685 16 9 14.6569 9 13H15V15H17V13H18V15H20V11H9C9 9.34315 7.65685 8 6 8ZM7 13V11C7 10.4477 6.55228 10 6 10C5.44772 10 5 10.4477 5 11V13C5 13.5523 5.44772 14 6 14C6.55228 14 7 13.5523 7 13Z"
                    fill="currentColor"
                />
            </svg>
        </label>

        <input type="checkbox" id="private" />
        <label
            for="private"
            aria-label="private paste, only visible by you"
            class="btn btn-square btn-plain"
            use:tooltip
        >
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <path
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    d="M18 10.5C19.6569 10.5 21 11.8431 21 13.5V19.5C21 21.1569 19.6569 22.5 18 22.5H6C4.34315 22.5 3 21.1569 3 19.5V13.5C3 11.8431 4.34315 10.5 6 10.5V7.5C6 4.18629 8.68629 1.5 12 1.5C15.3137 1.5 18 4.18629 18 7.5V10.5ZM12 3.5C14.2091 3.5 16 5.29086 16 7.5V10.5H8V7.5C8 5.29086 9.79086 3.5 12 3.5ZM18 12.5H6C5.44772 12.5 5 12.9477 5 13.5V19.5C5 20.0523 5.44772 20.5 6 20.5H18C18.5523 20.5 19 20.0523 19 19.5V13.5C19 12.9477 18.5523 12.5 18 12.5Z"
                    fill="currentColor"
                />
            </svg>
        </label>

        <input type="checkbox" id="show-on-profile" />
        <label
            for="show-on-profile"
            aria-label="show on your public profile"
            class="btn btn-square btn-plain"
            use:tooltip
        >
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <path
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    d="M16 12C16 14.2091 14.2091 16 12 16C9.79086 16 8 14.2091 8 12C8 9.79086 9.79086 8 12 8C14.2091 8 16 9.79086 16 12ZM14 12C14 13.1046 13.1046 14 12 14C10.8954 14 10 13.1046 10 12C10 10.8954 10.8954 10 12 10C13.1046 10 14 10.8954 14 12Z"
                    fill="currentColor"
                />
                <path
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    d="M12 3C17.5915 3 22.2898 6.82432 23.6219 12C22.2898 17.1757 17.5915 21 12 21C6.40848 21 1.71018 17.1757 0.378052 12C1.71018 6.82432 6.40848 3 12 3ZM12 19C7.52443 19 3.73132 16.0581 2.45723 12C3.73132 7.94186 7.52443 5 12 5C16.4756 5 20.2687 7.94186 21.5428 12C20.2687 16.0581 16.4756 19 12 19Z"
                    fill="currentColor"
                />
            </svg>
        </label>

        <input type="checkbox" id="detach" />
        <label
            for="detach"
            aria-label="detach paste, won't be associated with your account"
            class="btn btn-square btn-plain"
            use:tooltip
        >
            <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                class="icon"
            >
                <path
                    d="M10.9759 9C10.9759 9.55228 10.5282 10 9.97589 10C9.42361 10 8.97589 9.55228 8.97589 9C8.97589 8.44771 9.42361 8 9.97589 8C10.5282 8 10.9759 8.44771 10.9759 9Z"
                    fill="currentColor"
                />
                <path
                    d="M13.9712 10C14.5235 10 14.9712 9.55228 14.9712 9C14.9712 8.44771 14.5235 8 13.9712 8C13.4189 8 12.9712 8.44771 12.9712 9C12.9712 9.55228 13.4189 10 13.9712 10Z"
                    fill="currentColor"
                />
                <path
                    fill-rule="evenodd"
                    clip-rule="evenodd"
                    d="M19 20.9999V10C19 6.13401 15.866 3 12 3C8.13401 3 5 6.13401 5 10V21L7.82846 21L9.24264 19.5858L10.6569 21L13.3433 21L14.7574 19.5858L16.1717 21L19 20.9999ZM17 10C17 7.23858 14.7614 5 12 5C9.23858 5 7 7.23858 7 10V19L9.24264 16.7573L12 19.5147L14.7574 16.7573L17 18.9999V10Z"
                    fill="currentColor"
                />
            </svg>
        </label>
    </div>

    <button class="btn-main">create paste</button>
</div>

<style lang="scss">
    .paste-options {
        padding: 0.5rem 1rem;
    }

    @media screen and (max-width: $break-med) {
        .options {
            margin-bottom: 1rem;
        }
    }

    input[type="checkbox"] {
        width: 0;
        height: 0;
        opacity: 0;

        &:checked + label {
            color: $color-prim;
        }
    }

    label {
        margin-right: 1rem;
        font-size: $fs-large;

        &:hover {
            background-color: $color-bg;
        }
    }

    button {
        padding: 0.5rem 1rem;
    }
</style>
