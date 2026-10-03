import { json, type Handle, type HandleFetch } from "@sveltejs/kit";
import { env } from "$env/dynamic/public";
import { env as privateEnv } from "$env/dynamic/private";
import { RateLimiter, isLoopback } from "$lib/server/rateLimit";

const getClientAddress = (event: Parameters<Handle>[0]["event"]): string | undefined => {
    try {
        return event.getClientAddress();
    } catch {
        // No client address (e.g. ADDRESS_HEADER is set but the header is missing in dev)
        return undefined;
    }
};

// Highlighting runs Shiki synchronously on whatever text is posted, so it's limited per visitor.
// Everything else that does real work goes through the API, which has its own rate limiting.
const highlightLimiter = new RateLimiter(
    Number(privateEnv.HIGHLIGHT_RATE_LIMIT_BURST ?? 20),
    Number(privateEnv.HIGHLIGHT_RATE_LIMIT_PER_SECOND ?? 2)
);

export const handle: Handle = async ({ event, resolve }) => {
    // Sub-requests come from this server's own page loads, which the API already limits.
    if (event.url.pathname === "/internal/highlight" && !event.isSubRequest) {
        const clientAddress = getClientAddress(event);

        if (
            clientAddress &&
            !isLoopback(clientAddress) &&
            !highlightLimiter.tryTake(clientAddress)
        ) {
            return json(
                { statusCode: 429, message: "Too many requests, please slow down." },
                {
                    status: 429,
                    headers: {
                        "Retry-After": highlightLimiter.retryAfter(clientAddress).toString()
                    }
                }
            );
        }
    }

    return resolve(event);
};

// Server-side calls to the API come from this container's IP. Pass the visitor's IP on, so the
// API's per-IP rate limiting applies to the visitor (the API trusts X-Forwarded-For from here).
export const handleFetch: HandleFetch = async ({ event, request, fetch }) => {
    if (env.PUBLIC_API_SERVER_BASE && request.url.startsWith(env.PUBLIC_API_SERVER_BASE)) {
        const clientAddress = getClientAddress(event);

        if (clientAddress) {
            const headers = new Headers(request.headers);
            headers.set("X-Forwarded-For", clientAddress);
            request = new Request(request, { headers });
        }
    }

    return fetch(request);
};
