import tippy, { roundArrow } from "tippy.js";

export const tooltip = (node: HTMLElement, params = {}) => {
    const title = node.title;
    const label = node.getAttribute("aria-label");
    const content = label || title;

    const tip = tippy(node, { content, theme: "myst", arrow: roundArrow, ...params });

    return {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        update: (newParams: any) => tip.setProps({ content, ...newParams }),

        destroy: () => tip.destroy()
    };
};
