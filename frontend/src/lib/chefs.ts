import { apiFetch, type ApiEnvelope } from "./api";

export interface ChefListItem {
  id: string;
  displayName: string;
  bio: string;
  city: string;
  area: string | null;
  cuisines: string[];
  photoUrl: string | null;
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

export function listChefs(page = 1, pageSize = 20): Promise<ChefListPage> {
  return apiFetch<ChefListEnvelope>(`/api/chefs?page=${page}&pageSize=${pageSize}`).then(
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
