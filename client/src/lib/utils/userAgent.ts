import { browser } from "$app/environment";

export const isMacOs = () => {
    if (browser) {
        return window.navigator.userAgent.includes("Mac");
    } else {
        return false;
    }
};
