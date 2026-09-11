import { apiFetch, type ApiEnvelope } from "./api";

export interface OrderItem {
  id: string;
  foodItemId: string;
  dishName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

export interface Order {
  id: string;
  customerUserId: string;
  chefProfileId: string;
  subtotal: number;
  currency: string;
  status: "Pending" | "WhatsAppInitiated" | "Completed" | "Cancelled";
  deliveryAddress: string | null;
  customerPhone: string | null;
  customerName: string | null;
  deliveryMethod: string | null;
  whatsappInitiatedAtUtc: string | null;
  items: OrderItem[];
  createdAtUtc: string;
}

export interface OrderItemInput {
  foodItemId: string;
  dishName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

export interface CartItem {
  foodItemId: string;
  chefProfileId: string;
  dishName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

export interface CreateOrderRequest {
  chefProfileId: string;
  items: OrderItemInput[];
  deliveryAddress?: string;
  customerPhone?: string;
  customerName?: string;
  deliveryMethod?: string;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function createOrder(request: CreateOrderRequest): Promise<Order> {
  return apiFetch<ApiEnvelope<Order>>("/api/orders", {
    method: "POST",
    body: request,
  }).then(unwrap);
}

export function getOrder(id: string): Promise<Order> {
  return apiFetch<ApiEnvelope<Order>>(`/api/orders/${id}`).then(unwrap);
}

export function listMyOrders(): Promise<Order[]> {
  return apiFetch<ApiEnvelope<Order[]>>(`/api/orders/my`).then(unwrap);
}

export function listChefOrders(chefId: string): Promise<Order[]> {
  return apiFetch<ApiEnvelope<Order[]>>(`/api/orders/chef/${chefId}`).then(unwrap);
}

export function markWhatsAppInitiated(id: string): Promise<void> {
  return apiFetch<void>(`/api/orders/${id}/whatsapp-initiated`, { method: "POST" });
}

export function markOrderCompleted(id: string): Promise<void> {
  return apiFetch<void>(`/api/orders/${id}/complete`, { method: "POST" });
}

export function getWhatsAppUrl(id: string): Promise<{ url: string; message: string }> {
  return apiFetch<ApiEnvelope<{ url: string; message: string }>>(`/api/orders/${id}/whatsapp`).then(unwrap);
}
