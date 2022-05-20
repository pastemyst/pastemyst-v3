/**
 * Small wrapper functions around fetch.
 */

export interface FetcherResponse<T> {
    status: number;
    ok: boolean;
    data: T | null;
}

export interface FetcherRequest {
    body?: BodyInit;
    bearer?: string;
}

export const fetcherGet = async <T> (url: string, req: FetcherRequest = {}): Promise<FetcherResponse<T>> => {
    const res = await fetcher(url, "get", req);

    return {
        status: res.status,
        ok: res.ok,
        data: res.ok ? await res.json() : null
    };
};

export const fetcherPost = async <T> (url: string, req: FetcherRequest = {}): Promise<FetcherResponse<T>> => {
    const res = await fetcher(url, "post", req);

    return {
        status: res.status,
        ok: res.ok,
        data: res.ok ? await res.json() : null
    };
};

export const fetcher = async (url: string, method = "get", req: FetcherRequest): Promise<Response> => {
    return await fetch(url, {
        method: method,
        headers: {
            "Content-Type": "application/json",
            "Authorization": req.bearer ? `Bearer ${req.bearer}` : null
        },
        body: req.body
    });
};
