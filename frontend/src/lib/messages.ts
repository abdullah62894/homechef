import { apiFetch, type ApiEnvelope } from "./api";

export interface ChefMessage {
  id: string;
  chefProfileId: string;
  chefDisplayName: string;
  senderUserId: string;
  senderName: string;
  body: string;
  readAtUtc: string | null;
  createdAtUtc: string;
}

export interface ChefMessagePage {
  items: ChefMessage[];
  page: number;
  pageSize: number;
  total: number;
  hasMore: boolean;
}

interface MessageListEnvelope extends ApiEnvelope<ChefMessage[]> {
  meta: {
    page: number;
    pageSize: number;
    total: number;
    hasMore: boolean;
  } | null;
}

export interface SendChefMessageInput {
  chefProfileId: string;
  body: string;
}

export function sendChefMessage(input: SendChefMessageInput): Promise<ChefMessage> {
  return apiFetch<ApiEnvelope<ChefMessage>>("/api/messages", {
    method: "POST",
    body: input,
  }).then((envelope) => envelope.data);
}

export async function listInboxMessages(page = 1, pageSize = 20): Promise<ChefMessagePage> {
  const envelope = await apiFetch<MessageListEnvelope>(
    `/api/messages/inbox?page=${page}&pageSize=${pageSize}`
  );

  return {
    items: envelope.data,
    page: envelope.meta?.page ?? page,
    pageSize: envelope.meta?.pageSize ?? pageSize,
    total: envelope.meta?.total ?? envelope.data.length,
    hasMore: envelope.meta?.hasMore ?? false,
  };
}

export async function listSentMessages(page = 1, pageSize = 20): Promise<ChefMessagePage> {
  const envelope = await apiFetch<MessageListEnvelope>(
    `/api/messages/sent?page=${page}&pageSize=${pageSize}`
  );

  return {
    items: envelope.data,
    page: envelope.meta?.page ?? page,
    pageSize: envelope.meta?.pageSize ?? pageSize,
    total: envelope.meta?.total ?? envelope.data.length,
    hasMore: envelope.meta?.hasMore ?? false,
  };
}

export function markMessageRead(messageId: string): Promise<void> {
  return apiFetch<void>(`/api/messages/${messageId}/read`, { method: "POST" });
}

export function getUnreadCount(): Promise<number> {
  return apiFetch<ApiEnvelope<number>>("/api/messages/unread-count").then(
    (envelope) => envelope.data
  );
}
