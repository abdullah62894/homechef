import { apiFetch, type ApiEnvelope } from "./api";

export interface ChefListItem {
  id: string;
  displayName: string;
  bio: string;
  city: string;
  area: string | null;
  address: string | null;
  latitude: number | null;
  longitude: number | null;
  distanceKm: number | null;
  cuisines: string[];
  photoUrl: string | null;
  photoThumbnailUrl: string | null;
}

export interface ChefProfile extends ChefListItem {
  userId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ChefProfileInput {
  displayName: string;
  bio: string;
  city: string;
  area?: string | null;
  address?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  cuisines?: string[];
}

export interface ChefListPage {
  items: ChefListItem[];
  page: number;
  pageSize: number;
  total: number;
  hasMore: boolean;
}

interface ChefListEnvelope extends ApiEnvelope<ChefListItem[]> {
  meta: {
    page: number;
    pageSize: number;
    total: number;
    hasMore: boolean;
  } | null;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export interface ChefFilterOptions {
  search?: string;
  city?: string;
  area?: string;
  cuisine?: string;
  lat?: number;
  lng?: number;
  radiusKm?: number;
}

export function listChefs(
  filter: ChefFilterOptions = {},
  page = 1,
  pageSize = 20
): Promise<ChefListPage> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  if (filter.search) params.set("search", filter.search);
  if (filter.city) params.set("city", filter.city);
  if (filter.area) params.set("area", filter.area);
  if (filter.cuisine) params.set("cuisine", filter.cuisine);
  if (filter.lat !== undefined) params.set("lat", filter.lat.toString());
  if (filter.lng !== undefined) params.set("lng", filter.lng.toString());
  if (filter.radiusKm !== undefined) params.set("radiusKm", filter.radiusKm.toString());

  return apiFetch<ChefListEnvelope>(`/api/chefs?${params.toString()}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function getChef(id: string): Promise<ChefProfile> {
  return apiFetch<ApiEnvelope<ChefProfile>>(`/api/chefs/${id}`).then(unwrap);
}

export function getMyChefProfile(): Promise<ChefProfile> {
  return apiFetch<ApiEnvelope<ChefProfile>>("/api/chefs/me").then(unwrap);
}

export function createChefProfile(input: ChefProfileInput): Promise<ChefProfile> {
  return apiFetch<ApiEnvelope<ChefProfile>>("/api/chefs/me", {
    method: "POST",
    body: input,
  }).then(unwrap);
}

export function updateChefProfile(input: ChefProfileInput): Promise<ChefProfile> {
  return apiFetch<ApiEnvelope<ChefProfile>>("/api/chefs/me", {
    method: "PUT",
    body: input,
  }).then(unwrap);
}
