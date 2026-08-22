import { apiFetch, type ApiEnvelope } from "./api";
import type { Report } from "./reports";

export interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  isSuspended: boolean;
  chefProfileId: string | null;
  createdAtUtc: string;
}

export interface AdminReview {
  id: string;
  chefProfileId: string;
  chefDisplayName: string;
  customerUserId: string;
  reviewerName: string;
  rating: number;
  comment: string;
  createdAtUtc: string;
}

export interface AdminPage<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  hasMore: boolean;
}

interface MetaEnvelope<T> extends ApiEnvelope<T> {
  meta: {
    page: number;
    pageSize: number;
    total: number;
    hasMore: boolean;
  } | null;
}

function toPage<T>(envelope: MetaEnvelope<T[]>, page: number, pageSize: number): AdminPage<T> {
  return {
    items: envelope.data,
    page: envelope.meta?.page ?? page,
    pageSize: envelope.meta?.pageSize ?? pageSize,
    total: envelope.meta?.total ?? envelope.data.length,
    hasMore: envelope.meta?.hasMore ?? false,
  };
}

export function listAdminUsers(
  search?: string,
  role?: string,
  page = 1,
  pageSize = 20
): Promise<AdminPage<AdminUser>> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());
  if (search?.trim()) params.set("search", search.trim());
  if (role) params.set("role", role);

  return apiFetch<MetaEnvelope<AdminUser[]>>(`/api/admin/users?${params.toString()}`).then(
    (envelope) => toPage(envelope, page, pageSize)
  );
}

export function suspendUser(userId: string): Promise<AdminUser> {
  return apiFetch<ApiEnvelope<AdminUser>>(`/api/admin/users/${userId}/suspend`, {
    method: "POST",
  }).then((envelope) => envelope.data);
}

export function restoreUser(userId: string): Promise<AdminUser> {
  return apiFetch<ApiEnvelope<AdminUser>>(`/api/admin/users/${userId}/restore`, {
    method: "POST",
  }).then((envelope) => envelope.data);
}

export function listAdminReviews(page = 1, pageSize = 20): Promise<AdminPage<AdminReview>> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  return apiFetch<MetaEnvelope<AdminReview[]>>(`/api/admin/reviews?${params.toString()}`).then(
    (envelope) => toPage(envelope, page, pageSize)
  );
}

export function deleteAdminReview(reviewId: string): Promise<void> {
  return apiFetch<void>(`/api/admin/reviews/${reviewId}`, { method: "DELETE" });
}

export function deleteAdminFood(foodId: string): Promise<void> {
  return apiFetch<void>(`/api/admin/foods/${foodId}`, { method: "DELETE" });
}

export function deleteAdminChef(chefProfileId: string): Promise<void> {
  return apiFetch<void>(`/api/admin/chefs/${chefProfileId}`, { method: "DELETE" });
}

export function listAdminReports(
  status?: "Open" | "Resolved" | "Dismissed",
  page = 1,
  pageSize = 20
): Promise<AdminPage<Report>> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());
  if (status) params.set("status", status);

  return apiFetch<MetaEnvelope<Report[]>>(`/api/admin/reports?${params.toString()}`).then(
    (envelope) => toPage(envelope, page, pageSize)
  );
}

export function resolveReport(reportId: string): Promise<Report> {
  return apiFetch<ApiEnvelope<Report>>(`/api/admin/reports/${reportId}/resolve`, {
    method: "POST",
  }).then((envelope) => envelope.data);
}

export function dismissReport(reportId: string): Promise<Report> {
  return apiFetch<ApiEnvelope<Report>>(`/api/admin/reports/${reportId}/dismiss`, {
    method: "POST",
  }).then((envelope) => envelope.data);
}
