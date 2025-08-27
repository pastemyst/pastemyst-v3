import type { LayoutLoad } from "./$types";

export const load: LayoutLoad = async ({ parent, url }) => {
    const { self } = await parent();

    const category = url.pathname.substring(url.pathname.lastIndexOf("/") + 1);

    return {
        self: self,
        category: category
    };
};
