/* eslint-disable @typescript-eslint/no-empty-interface */
/// <reference types="@sveltejs/kit" />

// See https://kit.svelte.dev/docs#typescript
// for information about these interfaces
declare namespace App {
    interface Locals {}

    interface Platform {}

    interface Session {}

    interface Stuff {}
}

declare namespace svelte.JSX {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    interface HTMLAttributes<T> {
        oncmdShowOptions?: (event: CustomEvent) => void;
        onopenCmd?: () => void;
        ontoggleCmd?: () => void;
    }
}
