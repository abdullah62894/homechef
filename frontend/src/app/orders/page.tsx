"use client";

import { listMyOrders, listChefOrders, type Order } from "@/lib/orders";
import { fetchMe, type UserDto } from "@/lib/auth";
import { getMyChefProfile, type ChefProfile } from "@/lib/chefs";
import { ApiError } from "@/lib/api";
import Link from "next/link";
import { useState, useEffect } from "react";

const STATUS_STYLES: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  WhatsAppInitiated: "bg-blue-100 text-blue-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};

export default function OrdersPage() {
  const [user, setUser] = useState<UserDto | null>(null);
  const [chefProfile, setChefProfile] = useState<ChefProfile | null>(null);
  const [outgoing, setOutgoing] = useState<Order[]>([]);
  const [incoming, setIncoming] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<"outgoing" | "incoming">("outgoing");

  useEffect(() => {
    let cancelled = false;
    fetchMe()
      .then(async (me) => {
        if (cancelled) return;
        setUser(me);

        const isChef = me.roles.includes("Chef");
        const loaders: Promise<void>[] = [
          listMyOrders().then((items) => { if (!cancelled) setOutgoing(items); }),
        ];

        if (isChef) {
          loaders.push(
            getMyChefProfile()
              .then(async (profile) => {
                if (cancelled) return;
                setChefProfile(profile);
                const items = await listChefOrders(profile.id);
                if (!cancelled) setIncoming(items);
              })
          );
        }

        await Promise.all(loaders);
      })
      .catch((err) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 401) return;
      })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, []);

  const isChef = !!chefProfile;
  const activeOrders = tab === "outgoing" ? outgoing : incoming;

  if (loading) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <p className="text-gray-500">Loading orders...</p>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900">My Orders</h1>
        <p className="mt-2 text-gray-500">Please sign in to view your orders.</p>
        <Link href="/login?redirect=/orders" className="mt-6 inline-block rounded-lg bg-orange-500 px-6 py-3 text-sm font-semibold text-white hover:bg-orange-600">
          Sign in
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">My Orders</h1>

      {isChef && (
        <div className="mt-4 flex gap-2 border-b border-gray-200">
          <button
            onClick={() => setTab("outgoing")}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              tab === "outgoing"
                ? "border-orange-500 text-orange-600"
                : "border-transparent text-gray-500 hover:text-gray-700"
            }`}
          >
            My Orders ({outgoing.length})
          </button>
          <button
            onClick={() => setTab("incoming")}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              tab === "incoming"
                ? "border-orange-500 text-orange-600"
                : "border-transparent text-gray-500 hover:text-gray-700"
            }`}
          >
            Incoming Orders ({incoming.length})
          </button>
        </div>
      )}

      {activeOrders.length === 0 ? (
        <div className="mt-6 text-center">
          <p className="text-gray-500">
            {tab === "outgoing" ? "No orders yet." : "No incoming orders yet."}
          </p>
          {tab === "outgoing" && (
            <Link href="/food" className="mt-4 inline-block rounded-lg bg-orange-500 px-6 py-3 text-sm font-semibold text-white hover:bg-orange-600">
              Browse Dishes
            </Link>
          )}
        </div>
      ) : (
        <div className="mt-6 space-y-4">
          {activeOrders.map((order) => (
            <div key={order.id} className="rounded-lg border border-gray-200 bg-white p-4">
              <div className="flex items-center justify-between">
                <div className="text-sm">
                  {tab === "incoming" ? (
                    <>
                      <span className="font-semibold text-gray-900">{order.customerName || "Customer"}</span>
                      {order.customerPhone && <span className="text-gray-500"> · {order.customerPhone}</span>}
                    </>
                  ) : (
                    <span className="text-xs text-gray-500">
                      {new Date(order.createdAtUtc).toLocaleDateString()}
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_STYLES[order.status] ?? "bg-gray-100 text-gray-800"}`}>
                    {order.status}
                  </span>
                  {tab === "incoming" && (
                    <span className="text-xs text-gray-400">{new Date(order.createdAtUtc).toLocaleString()}</span>
                  )}
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
              <div className="mt-3 flex items-center justify-between border-t border-gray-100 pt-3">
                <span className="text-sm font-medium text-gray-700">Total</span>
                <span className="text-sm font-bold text-gray-900">{order.currency} {order.subtotal.toLocaleString()}</span>
              </div>
              {tab === "incoming" && order.deliveryAddress && (
                <p className="mt-2 text-xs text-gray-500">📍 {order.deliveryAddress}</p>
              )}
              {order.status === "Pending" && (
                <Link href={`/orders/${order.id}`} className="mt-3 block text-center text-sm text-orange-500 hover:text-orange-600">
                  {tab === "incoming" ? "View & Send WhatsApp" : "View Order & WhatsApp"}
                </Link>
              )}
              {order.status === "Completed" && tab === "outgoing" && (
                <Link href={`/orders/${order.id}`} className="mt-3 block text-center text-sm text-orange-500 hover:text-orange-600">
                  Leave a Review
                </Link>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
