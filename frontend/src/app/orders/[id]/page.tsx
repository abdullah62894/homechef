"use client";

import { getOrder, getWhatsAppUrl, markWhatsAppInitiated, type Order } from "@/lib/orders";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useEffect, use } from "react";

export default function OrderDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const [order, setOrder] = useState<Order | null>(null);
  const [whatsappUrl, setWhatsappUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);

  useEffect(() => {
    getOrder(id)
      .then(setOrder)
      .finally(() => setLoading(false));
  }, [id]);

  async function handleWhatsApp() {
    if (!order) return;
    setSending(true);
    try {
      const result = await getWhatsAppUrl(order.id);
      setWhatsappUrl(result.url);
      await markWhatsAppInitiated(order.id);
      setOrder({ ...order, status: "WhatsAppInitiated" });
    } catch (err: any) {
      alert(err.message || "Failed to generate WhatsApp link.");
    } finally {
      setSending(false);
    }
  }

  if (loading) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <p className="text-gray-500">Loading order...</p>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900">Order not found</h1>
        <Link href="/orders" className="mt-4 inline-block text-orange-500 hover:text-orange-600">
          Back to orders
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <Link href="/orders" className="text-sm text-orange-500 hover:text-orange-600">&larr; Back to orders</Link>

      <h1 className="mt-4 text-2xl font-bold text-gray-900">Order Details</h1>
      <p className="text-xs text-gray-500">{new Date(order.createdAtUtc).toLocaleString()}</p>

      <div className="mt-6 rounded-lg border border-gray-200 bg-white p-4">
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold text-gray-700">Status</span>
          <span className="text-sm text-gray-900">{order.status}</span>
        </div>

        <div className="mt-4 divide-y divide-gray-100">
          {order.items.map((item) => (
            <div key={item.id} className="flex justify-between py-2 first:pt-0 last:pb-0">
              <span className="text-sm text-gray-700">{item.dishName} × {item.quantity}</span>
              <span className="text-sm font-medium text-gray-900">{item.currency} {(item.unitPrice * item.quantity).toLocaleString()}</span>
            </div>
          ))}
        </div>

        <div className="mt-4 border-t border-gray-200 pt-3">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-700">Total</span>
            <span className="text-lg font-bold text-gray-900">{order.currency} {order.subtotal.toLocaleString()}</span>
          </div>
        </div>
      </div>

      {order.status === "Pending" && (
        <div className="mt-6">
          {whatsappUrl ? (
            <a
              href={whatsappUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="block w-full rounded-lg bg-green-500 py-3 text-center text-sm font-semibold text-white hover:bg-green-600"
            >
              Open WhatsApp
            </a>
          ) : (
            <button
              onClick={handleWhatsApp}
              disabled={sending}
              className="block w-full rounded-lg bg-green-500 py-3 text-center text-sm font-semibold text-white hover:bg-green-600 disabled:opacity-50"
            >
              {sending ? "Generating..." : "Send via WhatsApp"}
            </button>
          )}
        </div>
      )}
    </div>
  );
}
