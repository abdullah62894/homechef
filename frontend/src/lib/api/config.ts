export const API_BASE_URL: string =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5050";

/** Server-side only base URL. Overridable for server-rendered fetches. */
export function apiUrl(path: string): string {
  return `${API_BASE_URL}${path}`;
}