import { apiFetch, type ApiEnvelope } from "./api";

export interface Cuisine {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  imageThumbnailUrl: string | null;
  displayOrder: number;
  isActive: boolean;
  createdAtUtc: string;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function listActiveCuisines(): Promise<Cuisine[]> {
  return apiFetch<ApiEnvelope<Cuisine[]>>(`/api/cuisines`).then(unwrap);
}

export function listAllCuisines(): Promise<Cuisine[]> {
  return apiFetch<ApiEnvelope<Cuisine[]>>(`/api/cuisines/admin`).then(unwrap);
}

export function getCuisine(id: string): Promise<Cuisine> {
  return apiFetch<ApiEnvelope<Cuisine>>(`/api/cuisines/${id}`).then(unwrap);
}
