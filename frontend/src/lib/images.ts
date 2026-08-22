import { apiFetch, apiUpload, API_BASE_URL, type ApiEnvelope } from "./api";

/** Content types the backend accepts for image uploads. */
export const ACCEPTED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];

/** Client-side mirror of the backend 5 MB limit. */
export const MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024;

/**
 * Client-side pre-validation so users get immediate feedback before the
 * round-trip. The backend re-validates everything.
 */
export function validateImageFile(file: File): string | null {
  if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
    return "Please choose a JPEG, PNG or WebP image.";
  }
  if (file.size > MAX_IMAGE_SIZE_BYTES) {
    return "Image must be 5 MB or smaller.";
  }
  return null;
}

/** Turns a relative API image path (e.g. /uploads/...) into a full URL. */
export function resolveImageUrl(path: string | null | undefined): string | null {
  if (!path) return null;
  if (/^https?:\/\//i.test(path)) return path;
  return `${API_BASE_URL}${path.startsWith("/") ? path : `/${path}`}`;
}

/** Uploads the calling chef's profile photo (optimized server-side to WebP). */
export function uploadChefPhoto(file: File): Promise<void> {
  return apiUpload<ApiEnvelope<unknown>>("/api/chefs/me/photo", file).then(() => undefined);
}

/** Removes the calling chef's profile photo. */
export function clearChefPhoto(): Promise<void> {
  return apiFetch<void>("/api/chefs/me/photo", { method: "DELETE" });
}

/** Uploads the image for one of the calling chef's food items. */
export function uploadFoodImage(foodId: string, file: File): Promise<void> {
  return apiUpload<ApiEnvelope<unknown>>(`/api/chefs/me/foods/${foodId}/image`, file).then(
    () => undefined
  );
}

/** Removes the image of one of the calling chef's food items. */
export function clearFoodImage(foodId: string): Promise<void> {
  return apiFetch<void>(`/api/chefs/me/foods/${foodId}/image`, { method: "DELETE" });
}
