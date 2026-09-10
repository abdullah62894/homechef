"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { listChefOrders, getWhatsAppUrl, type Order } from "@/lib/orders";
import { getMyChefProfile, type ChefProfile } from "@/lib/chefs";
import { ApiError } from "@/lib/api";

const STATUS_STYLES: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  WhatsAppInitiated: "bg-blue-100 text-blue-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};

export default function ChefOrdersPage() {
  const router = useRouter();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getMyChefProfile()
      .then((profile) => listChefOrders(profile.id))
      .then((items) => { if (!cancelled) setOrders(items); })
      .catch((err) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 401) {
          router.replace("/login");
          return;
        }
        setError(err instanceof ApiError ? err.message : "Unable to load orders.");
      })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [router]);

  async function handleWhatsApp(order: Order) {
    try {
      const result = await getWhatsAppUrl(order.id);
      window.open(result.url, "_blank");
    } catch (err: any) {
      alert(err.message || "Failed to generate WhatsApp link.");
    }
  }

  if (loading) {
    return <section className="mx-auto max-w-3xl px-4 py-16 text-gray-600">Loading orders…</section>;
  }

  return (
    <section className="mx-auto max-w-3xl px-4 py-8">
      <Link href="/chefs/me" className="text-sm text-orange-500 hover:text-orange-600">&larr; Back to dashboard</Link>
      <h1 className="mt-4 text-3xl font-bold tracking-tight text-gray-900">Incoming Orders</h1>

      {error && <div className="mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</div>}

      {orders.length === 0 ? (
        <p className="mt-6 text-gray-500">No orders yet. When customers place orders, they will appear here.</p>
      ) : (
        <div className="mt-6 space-y-4">
          {orders.map((order) => (
            <div key={order.id} className="rounded-xl border border-gray-200 bg-white p-4">
              <div className="flex items-center justify-between">
                <div className="text-sm">
                  <span className="font-semibold text-gray-900">{order.customerName || "Customer"}</span>
                  {order.customerPhone && <span className="text-gray-500"> · {order.customerPhone}</span>}
                </div>
                <div className="flex items-center gap-2">
                  <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${STATUS_STYLES[order.status] ?? "bg-gray-100 text-gray-800"}`}>
                    {order.status}
                  </span>
                  <span className="text-xs text-gray-400">{new Date(order.createdAtUtc).toLocaleString()}</span>
                </div>
              </div>

              <div className="mt-3 divide-y divide-gray-100">
                {order.items.map((item) => (
                  <div key={item.id} className="flex justify-between py-1.5 first:pt-0 last:pb-0">
                    <span className="text-sm text-gray-700">{item.dishName} × {item.quantity}</span>
                    <span className="text-sm font-medium text-gray-900">{item.currency} {(item.unitPrice * item.quantity).toLocaleString()}</span>
                  </div>
                ))}
              </div>

              <div className="mt-3 border-t border-gray-100 pt-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Total: {order.currency} {order.subtotal.toLocaleString()}</span>
                  {order.deliveryAddress && (
                    <span className="text-xs text-gray-500">📍 {order.deliveryAddress}</span>
                  )}
                </div>
              </div>

              {order.status === "Pending" && (
                <div className="mt-3 flex gap-2">
                  <button
                    onClick={() => handleWhatsApp(order)}
                    className="flex-1 rounded-lg bg-green-500 px-4 py-2 text-sm font-semibold text-white hover:bg-green-600"
                  >
                    Send via WhatsApp
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
