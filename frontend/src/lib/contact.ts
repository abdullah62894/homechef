import { apiFetch, type ApiEnvelope } from "./api";

export interface ContactMessage {
  id: string;
  name: string;
  phone: string;
  email: string;
  message: string;
  isRead: boolean;
  status: string;
  createdAtUtc: string;
  readAtUtc: string | null;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function submitContact(name: string, phone: string, email: string, message: string): Promise<ContactMessage> {
  return apiFetch<ApiEnvelope<ContactMessage>>("/api/contact", {
    method: "POST",
    body: { name, phone, email, message },
  }).then(unwrap);
}

export function listContactMessages(page = 1, pageSize = 20): Promise<ContactMessage[]> {
  return apiFetch<ApiEnvelope<ContactMessage[]>>(`/api/contact/admin?page=${page}&pageSize=${pageSize}`).then(unwrap);
}

export function markContactAsRead(id: string): Promise<void> {
  return apiFetch<void>(`/api/contact/admin/${id}/read`, { method: "PUT" });
}

export function updateContactStatus(id: string, status: string): Promise<void> {
  return apiFetch<void>(`/api/contact/admin/${id}/status`, {
    method: "PUT",
    body: { status },
  });
}

export function getUnreadContactCount(): Promise<number> {
  return apiFetch<ApiEnvelope<number>>(`/api/contact/admin/unread-count`).then(unwrap);
}
