<script lang="ts">
    import { onDestroy, onMount, type Snippet } from "svelte";
    import tippy, { type Instance, type Props as TippyProps } from "tippy.js";

    interface Props {
        buttonSlot: Snippet;
        dropdownSlot: Snippet;
    }

    let { buttonSlot, dropdownSlot }: Props = $props();

    let buttonNode: HTMLElement;
    let dropdownNode: HTMLElement;
    let tip: Instance<TippyProps>;

    onMount(() => {
        tip = tippy(buttonNode, {
            content: dropdownNode,
            theme: "myst-dropdown",
            interactive: true,
            trigger: "click",
            arrow: false,
            placement: "bottom-end",
            allowHTML: true
        });
    });

    onDestroy(() => {
        if (tip) tip.destroy();
    });

    export const hide = () => {
        tip.hide();
    };
</script>

<button bind:this={buttonNode}>
    {@render buttonSlot()}

    <div bind:this={dropdownNode}>
        {@render dropdownSlot()}
    </div>
</button>
