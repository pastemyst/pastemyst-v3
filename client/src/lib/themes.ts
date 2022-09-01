export interface Theme {
    name: string;
    colors: ThemeColors;
}

export interface ThemeColors {
    bg: string;
    bg1: string;
    bg2: string;
    bg3: string;

    fg: string;

    primary: string;
    secondary: string;

    danger: string;
    success: string;
    pink: string;
}

export const themes: Theme[] = [
    {
        name: "myst",
        colors: {
            bg: "#141414",
            bg1: "#1c1c1c",
            bg2: "#222222",
            bg3: "#777777",
            fg: "#cccccc",
            primary: "#ee720d",
            secondary: "#1eaedb",
            danger: "#ff4c4c",
            success: "#2ec933",
            pink: "#cb6dce"
        }
    },
    {
        name: "test",
        colors: {
            bg: "#000000",
            bg1: "#111111",
            bg2: "#222222",
            bg3: "#333333",
            fg: "#ffffff",
            primary: "blue",
            secondary: "red",
            danger: "red",
            success: "green",
            pink: "pink"
        }
    }
];
