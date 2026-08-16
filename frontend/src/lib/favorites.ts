import { apiFetch, type ApiEnvelope } from "./api";
import type { ChefListItem, ChefListPage } from "./chefs";
import type { FoodListItem, FoodListPage } from "./foods";

export interface UserFavoriteIds {
  chefIds: string[];
  foodIds: string[];
}

interface ChefListEnvelope extends ApiEnvelope<ChefListItem[]> {
  meta: {
    page: number;
    pageSize: number;
    total: number;
    hasMore: boolean;
  } | null;
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

export function addChefFavorite(chefId: string): Promise<void> {
  return apiFetch<void>(`/api/favorites/chefs/${chefId}`, {
    method: "POST",
  });
}

export function removeChefFavorite(chefId: string): Promise<void> {
  return apiFetch<void>(`/api/favorites/chefs/${chefId}`, {
    method: "DELETE",
  });
}

export function listFavoriteChefs(page = 1, pageSize = 20): Promise<ChefListPage> {
  return apiFetch<ChefListEnvelope>(`/api/favorites/chefs?page=${page}&pageSize=${pageSize}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function addFoodFavorite(foodId: string): Promise<void> {
  return apiFetch<void>(`/api/favorites/foods/${foodId}`, {
    method: "POST",
  });
}

export function removeFoodFavorite(foodId: string): Promise<void> {
  return apiFetch<void>(`/api/favorites/foods/${foodId}`, {
    method: "DELETE",
  });
}

export function listFavoriteFoods(page = 1, pageSize = 20): Promise<FoodListPage> {
  return apiFetch<FoodListEnvelope>(`/api/favorites/foods?page=${page}&pageSize=${pageSize}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function getUserFavoriteIds(): Promise<UserFavoriteIds> {
  return apiFetch<ApiEnvelope<UserFavoriteIds>>("/api/favorites/ids").then(unwrap);
}
