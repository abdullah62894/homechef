import { apiFetch, type ApiEnvelope } from "./api";

export interface ChefAvailability {
  id: string;
  chefProfileId: string;
  dayOfWeek: number;
  openTime: string;
  closeTime: string;
  label: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface AvailabilityWindowInput {
  dayOfWeek: number;
  openTime: string;
  closeTime: string;
  label?: string | null;
  isActive: boolean;
}

const DAY_NAMES = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

export function getDayName(day: number): string {
  return DAY_NAMES[day] ?? `Day ${day}`;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function getMyAvailability(): Promise<ChefAvailability[]> {
  return apiFetch<ApiEnvelope<ChefAvailability[]>>("/api/chefs/availability/me").then(unwrap);
}

export function getChefAvailability(chefId: string): Promise<ChefAvailability[]> {
  return apiFetch<ApiEnvelope<ChefAvailability[]>>(`/api/chefs/availability/${chefId}`).then(unwrap);
}

export function updateMyAvailability(windows: AvailabilityWindowInput[]): Promise<void> {
  return apiFetch<void>("/api/chefs/availability/me", {
    method: "PUT",
    body: { windows },
  });
}
