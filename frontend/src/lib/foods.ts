import { apiFetch, type ApiEnvelope } from "./api";

export interface FoodCategory {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  displayOrder: number;
}

export interface FoodListItem {
  id: string;
  chefProfileId: string;
  chefDisplayName: string;
  chefCity: string;
  chefArea: string | null;
  chefAddress: string | null;
  distanceKm: number | null;
  categoryId: string | null;
  categoryName: string | null;
  name: string;
  description: string;
  price: number;
  currency: string;
  isAvailable: boolean;
  imageUrl: string | null;
  imageThumbnailUrl: string | null;
  preparationTimeMinutes: number | null;
}

export interface FoodItem extends FoodListItem {
  chefLatitude: number | null;
  chefLongitude: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface FoodItemInput {
  name: string;
  description: string;
  price: number;
  currency?: string;
  categoryId?: string | null;
  isAvailable?: boolean;
  imageUrl?: string | null;
  preparationTimeMinutes?: number | null;
}

export interface FoodListPage {
  items: FoodListItem[];
  page: number;
  pageSize: number;
  total: number;
  hasMore: boolean;
}

export interface FoodFilterOptions {
  categoryId?: string;
  chefId?: string;
  search?: string;
  city?: string;
  area?: string;
  cuisine?: string;
  lat?: number;
  lng?: number;
  radiusKm?: number;
  isAvailable?: boolean;
}

interface FoodListEnvelope extends ApiEnvelope<FoodListItem[]> {
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

export function listFoods(
  filter: FoodFilterOptions = {},
  page = 1,
  pageSize = 20
): Promise<FoodListPage> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  if (filter.categoryId) params.set("categoryId", filter.categoryId);
  if (filter.chefId) params.set("chefId", filter.chefId);
  if (filter.search) params.set("search", filter.search);
  if (filter.city) params.set("city", filter.city);
  if (filter.area) params.set("area", filter.area);
  if (filter.cuisine) params.set("cuisine", filter.cuisine);
  if (filter.lat !== undefined) params.set("lat", filter.lat.toString());
  if (filter.lng !== undefined) params.set("lng", filter.lng.toString());
  if (filter.radiusKm !== undefined) params.set("radiusKm", filter.radiusKm.toString());
  if (filter.isAvailable !== undefined) params.set("isAvailable", String(filter.isAvailable));

  return apiFetch<FoodListEnvelope>(`/api/foods?${params.toString()}`).then((envelope) => ({
    items: envelope.data,
    page: envelope.meta?.page ?? page,
    pageSize: envelope.meta?.pageSize ?? pageSize,
    total: envelope.meta?.total ?? envelope.data.length,
    hasMore: envelope.meta?.hasMore ?? false,
  }));
}

export function getFood(id: string): Promise<FoodItem> {
  return apiFetch<ApiEnvelope<FoodItem>>(`/api/foods/${id}`).then(unwrap);
}

export function listFoodCategories(): Promise<FoodCategory[]> {
  return apiFetch<ApiEnvelope<FoodCategory[]>>("/api/foods/categories").then(unwrap);
}

export function listChefFoods(
  chefId: string,
  isAvailable?: boolean,
  page = 1,
  pageSize = 50
): Promise<FoodListPage> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());
  if (isAvailable !== undefined) params.set("isAvailable", String(isAvailable));

  return apiFetch<FoodListEnvelope>(`/api/chefs/${chefId}/foods?${params.toString()}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function listMyFoods(page = 1, pageSize = 50): Promise<FoodListPage> {
  return apiFetch<FoodListEnvelope>(`/api/chefs/me/foods?page=${page}&pageSize=${pageSize}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function createFoodItem(input: FoodItemInput): Promise<FoodItem> {
  return apiFetch<ApiEnvelope<FoodItem>>("/api/chefs/me/foods", {
    method: "POST",
    body: input,
  }).then(unwrap);
}

export function updateFoodItem(id: string, input: FoodItemInput): Promise<FoodItem> {
  return apiFetch<ApiEnvelope<FoodItem>>(`/api/chefs/me/foods/${id}`, {
    method: "PUT",
    body: input,
  }).then(unwrap);
}

export function deleteFoodItem(id: string): Promise<void> {
  return apiFetch<void>(`/api/chefs/me/foods/${id}`, {
    method: "DELETE",
  });
}

export function toggleFoodAvailability(id: string, isAvailable: boolean): Promise<FoodItem> {
  return apiFetch<ApiEnvelope<FoodItem>>(`/api/chefs/me/foods/${id}/availability`, {
    method: "PATCH",
    body: { isAvailable },
  }).then(unwrap);
}
