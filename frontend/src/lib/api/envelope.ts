/** Backend response envelope for successful responses. */
export interface ApiEnvelope<T> {
  data: T;
  meta?: Record<string, unknown> | null;
}