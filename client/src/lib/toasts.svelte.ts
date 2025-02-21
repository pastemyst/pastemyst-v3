import { nanoid } from "nanoid";

export type ToastType = "info" | "success" | "error";

export interface Toast {
    id: string;
    type: ToastType;
    message: string;
}

export const toasts = $state<Toast[]>([]);

export const addToast = (message: string, type: ToastType = "info") => {
    const id = nanoid();
    toasts.push({ id, message, type });

    setTimeout(() => {
        removeToast(id);
    }, 3000);
};

export const removeToast = (id: string) => {
    const index = toasts.findIndex((toast) => toast.id === id);
    if (index === -1) return;
    toasts.splice(index, 1);
};
