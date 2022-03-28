export const getCookie = (name: string): string => {
    return document.cookie
        .split("; ")
        .find(r => r.startsWith(`${name}=`))
        .split("=")[1];
};

export const setCookie = (name: string, value: string, days: number) => {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));

    document.cookie = `${name}=${value}; expires=${date.toUTCString()}; path=/; samesite=strict; secure;`;
};

export const deleteCookie = (name: string) => {
    document.cookie = `${name}=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT;`;
};
