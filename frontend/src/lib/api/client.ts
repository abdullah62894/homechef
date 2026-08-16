import { toApiError } from "./error";

export interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  headers?: Record<string, string>;
  /** Send browser credentials (cookies) with the request. */
  credentials?: RequestCredentials;
  /** Abort signal for cancellation. */
  signal?: AbortSignal;
}

/**
 * Single fetch wrapper used by every API call in the app. Keeps API logic in
 * one place, attaches the consistent envelope handling, and never leaks API
 * secrets (base URL comes from NEXT_PUBLIC_* only).
 */
export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, headers, credentials = "include", signal } = options;

  const requestHeaders: Record<string, string> = {
    Accept: "application/json",
    ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
    ...headers,
  };

  const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5050";

  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers: requestHeaders,
    credentials,
    signal,
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  });

  if (!response.ok) {
    throw await toApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}