import { apiFetch, type ApiEnvelope } from "./api";

export type NotificationType = "NewMessage" | "NewReview";

export interface AppNotification {
  id: string;
  type: NotificationType;
  title: string;
  body: string;
  readAtUtc: string | null;
  createdAtUtc: string;
}

export interface NotificationPage {
  items: AppNotification[];
  page: number;
  pageSize: number;
  total: number;
  hasMore: boolean;
}

interface NotificationEnvelope extends ApiEnvelope<AppNotification[]> {
  meta: { page: number; pageSize: number; total: number; hasMore: boolean } | null;
}

export function listNotifications(page = 1, pageSize = 20): Promise<NotificationPage> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  return apiFetch<NotificationEnvelope>(`/api/notifications?${params.toString()}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function getUnreadNotificationCount(): Promise<number> {
  return apiFetch<ApiEnvelope<number>>("/api/notifications/unread-count").then(
    (envelope) => envelope.data
  );
}

export function markNotificationRead(id: string): Promise<void> {
  return apiFetch<void>(`/api/notifications/${id}/read`, { method: "POST" });
}

export function markAllNotificationsRead(): Promise<void> {
  return apiFetch<void>("/api/notifications/read-all", { method: "POST" });
}
