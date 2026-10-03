// Per-key token bucket rate limiter, kept in memory (one client instance).

type Bucket = { tokens: number; updatedAt: number };

export class RateLimiter {
    private readonly buckets = new Map<string, Bucket>();

    constructor(
        private readonly burst: number,
        private readonly perSecond: number
    ) {
        // Drop buckets that have refilled completely, so the map doesn't grow forever.
        setInterval(() => this.cleanup(), 60_000).unref();
    }

    /** Takes a token for the key; false if there is none left. */
    tryTake(key: string, now = Date.now()): boolean {
        const bucket = this.buckets.get(key) ?? { tokens: this.burst, updatedAt: now };

        bucket.tokens = Math.min(
            this.burst,
            bucket.tokens + ((now - bucket.updatedAt) / 1000) * this.perSecond
        );
        bucket.updatedAt = now;

        const allowed = bucket.tokens >= 1;
        if (allowed) bucket.tokens -= 1;

        this.buckets.set(key, bucket);
        return allowed;
    }

    /** Seconds until the key has a token again. */
    retryAfter(key: string): number {
        const tokens = this.buckets.get(key)?.tokens ?? this.burst;
        return Math.max(1, Math.ceil((1 - tokens) / this.perSecond));
    }

    private cleanup(now = Date.now()) {
        for (const [key, bucket] of this.buckets) {
            const refilled = bucket.tokens + ((now - bucket.updatedAt) / 1000) * this.perSecond;
            if (refilled >= this.burst) this.buckets.delete(key);
        }
    }
}

export const isLoopback = (address: string) =>
    address === "::1" || address.startsWith("127.") || address.startsWith("::ffff:127.");
