import { apiFetch, type ApiEnvelope } from "./api";
import type { ChefListItem, ChefListPage } from "./chefs";
import type { FoodListItem } from "./foods";

/* ── Search ─────────────────────────────────────────────── */

export interface SearchFilter {
  q?: string;
  city?: string;
  area?: string;
  cuisine?: string;
  categoryId?: string;
  lat?: number;
  lng?: number;
  radiusKm?: number;
  type?: "all" | "chefs" | "foods";
}

export interface SearchResult {
  chefs: ChefListItem[];
  foods: FoodListItem[];
  totalChefs: number;
  totalFoods: number;
  page: number;
  pageSize: number;
}

export function search(
  filter: SearchFilter = {},
  page = 1,
  pageSize = 20
): Promise<SearchResult> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  if (filter.q) params.set("q", filter.q);
  if (filter.city) params.set("city", filter.city);
  if (filter.area) params.set("area", filter.area);
  if (filter.cuisine) params.set("cuisine", filter.cuisine);
  if (filter.categoryId) params.set("categoryId", filter.categoryId);
  if (filter.lat !== undefined) params.set("lat", filter.lat.toString());
  if (filter.lng !== undefined) params.set("lng", filter.lng.toString());
  if (filter.radiusKm !== undefined) params.set("radiusKm", filter.radiusKm.toString());
  if (filter.type) params.set("type", filter.type);

  return apiFetch<ApiEnvelope<SearchResult>>(`/api/search?${params.toString()}`).then(
    (envelope) => envelope.data
  );
}

/* ── Locations ──────────────────────────────────────────── */

export interface AreaSummary {
  name: string;
  chefCount: number;
}

export interface CitySummary {
  city: string;
  totalChefs: number;
  areas: AreaSummary[];
}

export interface LocationDirectory {
  cities: CitySummary[];
}

export function getLocations(): Promise<LocationDirectory> {
  return apiFetch<ApiEnvelope<LocationDirectory>>("/api/locations").then(
    (envelope) => envelope.data
  );
}

export function getCityLocation(city: string): Promise<CitySummary> {
  return apiFetch<ApiEnvelope<CitySummary>>(`/api/locations/${encodeURIComponent(city)}`).then(
    (envelope) => envelope.data
  );
}

interface ChefListEnvelope extends ApiEnvelope<ChefListItem[]> {
  meta: {
    page: number;
    pageSize: number;
    total: number;
    hasMore: boolean;
  } | null;
}

export function listChefsInArea(
  city: string,
  area: string,
  page = 1,
  pageSize = 20
): Promise<ChefListPage> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  return apiFetch<ChefListEnvelope>(
    `/api/locations/${encodeURIComponent(city)}/${encodeURIComponent(area)}?${params.toString()}`
  ).then((envelope) => ({
    items: envelope.data,
    page: envelope.meta?.page ?? page,
    pageSize: envelope.meta?.pageSize ?? pageSize,
    total: envelope.meta?.total ?? envelope.data.length,
    hasMore: envelope.meta?.hasMore ?? false,
  }));
}
