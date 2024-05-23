<script lang="ts">
    import { onDestroy, onMount } from "svelte";
    import tippy, { type Instance, type Props } from "tippy.js";

    let buttonNode: HTMLElement;
    let dropdownNode: HTMLElement;
    let tip: Instance<Props>;

    onMount(() => {
        tip = tippy(buttonNode, { content: dropdownNode, theme: "myst-dropdown", interactive: true, trigger: "click", arrow: false, placement: "bottom-end", allowHTML: true });
    });

    onDestroy(() => {
        if (tip) tip.destroy();
    });

    export const hide = () => {
        tip.hide();
    };
</script>

<button bind:this={buttonNode}>
    <slot name="button" />

    <div bind:this={dropdownNode}>
        <slot name="dropdown" />
    </div>
</button>
