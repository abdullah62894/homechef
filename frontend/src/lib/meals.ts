import { apiFetch, type ApiEnvelope } from "./api";

export interface MealCategory {
  id: string;
  name: string;
  slug: string;
  displayOrder: number;
  createdAtUtc: string;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function listMealCategories(): Promise<MealCategory[]> {
  return apiFetch<ApiEnvelope<MealCategory[]>>(`/api/meals`).then(unwrap);
}
