"use client";

import { listMyOrders, type Order } from "@/lib/orders";
import Link from "next/link";
import { useState, useEffect } from "react";

const STATUS_STYLES: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  WhatsAppInitiated: "bg-blue-100 text-blue-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    listMyOrders().then(setOrders).finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <p className="text-gray-500">Loading orders...</p>
      </div>
    );
  }

  if (orders.length === 0) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900">My Orders</h1>
        <p className="mt-2 text-gray-500">No orders yet.</p>
        <Link href="/food" className="mt-6 inline-block rounded-lg bg-orange-500 px-6 py-3 text-sm font-semibold text-white hover:bg-orange-600">
          Browse Dishes
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">My Orders</h1>
      <div className="mt-6 space-y-4">
        {orders.map((order) => (
          <div key={order.id} className="rounded-lg border border-gray-200 bg-white p-4">
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-500">
                {new Date(order.createdAtUtc).toLocaleDateString()}
              </span>
              <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_STYLES[order.status] ?? "bg-gray-100 text-gray-800"}`}>
                {order.status}
              </span>
            </div>
            <div className="mt-3 divide-y divide-gray-100">
              {order.items.map((item) => (
                <div key={item.id} className="flex justify-between py-1.5 first:pt-0 last:pb-0">
                  <span className="text-sm text-gray-700">{item.dishName} × {item.quantity}</span>
                  <span className="text-sm font-medium text-gray-900">{item.currency} {(item.unitPrice * item.quantity).toLocaleString()}</span>
                </div>
              ))}
            </div>
            <div className="mt-3 flex items-center justify-between border-t border-gray-100 pt-3">
              <span className="text-sm font-medium text-gray-700">Total</span>
              <span className="text-sm font-bold text-gray-900">{order.currency} {order.subtotal.toLocaleString()}</span>
            </div>
            {order.status === "Pending" && (
              <Link href={`/orders/${order.id}`} className="mt-3 block text-center text-sm text-orange-500 hover:text-orange-600">
                View Order &amp; WhatsApp
              </Link>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
