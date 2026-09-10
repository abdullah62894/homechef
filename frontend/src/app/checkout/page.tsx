"use client";

import { useCart } from "@/components/CartProvider";
import { createOrder, type CreateOrderRequest, type Order } from "@/lib/orders";
import { fetchMe, type UserDto } from "@/lib/auth";
import { useRouter } from "next/navigation";
import { useState, useEffect } from "react";
import Link from "next/link";

export default function CheckoutPage() {
  const { items, itemsByChef, totalPrice, clearCart } = useCart();
  const router = useRouter();
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deliveryAddress, setDeliveryAddress] = useState("");
  const [customerPhone, setCustomerPhone] = useState("");
  const [customerName, setCustomerName] = useState("");

  useEffect(() => {
    fetchMe().then(setUser).catch(() => {});
  }, []);

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900">Checkout</h1>
        <p className="mt-2 text-gray-500">Your cart is empty.</p>
        <Link href="/food" className="mt-6 inline-block rounded-lg bg-orange-500 px-6 py-3 text-sm font-semibold text-white hover:bg-orange-600">
          Browse Dishes
        </Link>
      </div>
    );
  }

  async function handleOrder() {
    if (user) {
      setCustomerName(`${user.firstName} ${user.lastName}`);
    }

    setLoading(true);
    setError(null);

    try {
      const chefIds = Object.keys(itemsByChef);

      for (const chefId of chefIds) {
        const chefItems = itemsByChef[chefId];
        const request: CreateOrderRequest = {
          chefProfileId: chefId,
          items: chefItems.map((i) => ({
            foodItemId: i.foodItemId,
            dishName: i.dishName,
            quantity: i.quantity,
            unitPrice: i.unitPrice,
            currency: i.currency,
          })),
          deliveryAddress: deliveryAddress || undefined,
          customerPhone: customerPhone || undefined,
          customerName: customerName || undefined,
          deliveryMethod: "WhatsApp",
        };

        const order = await createOrder(request);
      }

      clearCart();
      router.push("/orders");
    } catch (err: any) {
      setError(err.message || "Failed to place order. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Checkout</h1>

      <div className="mt-6 space-y-4 rounded-lg border border-gray-200 bg-white p-4">
        <h2 className="text-sm font-semibold text-gray-700">Delivery Details</h2>
        <div>
          <label className="block text-xs font-medium text-gray-700">Full Name</label>
          <input
            type="text"
            value={customerName}
            onChange={(e) => setCustomerName(e.target.value)}
            placeholder={user ? `${user.firstName} ${user.lastName}` : "Your name"}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-700">Phone Number</label>
          <input
            type="tel"
            value={customerPhone}
            onChange={(e) => setCustomerPhone(e.target.value)}
            placeholder="e.g. 03001234567"
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-700">Delivery Address</label>
          <textarea
            value={deliveryAddress}
            onChange={(e) => setDeliveryAddress(e.target.value)}
            placeholder="Street address, area, city"
            rows={3}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
          />
        </div>
      </div>

      <div className="mt-6 rounded-lg border border-gray-200 bg-white p-4">
        <h2 className="text-sm font-semibold text-gray-700">Order Summary</h2>
        <div className="mt-3 divide-y divide-gray-100">
          {Object.entries(itemsByChef).map(([chefId, chefItems]) => (
            <div key={chefId} className="py-3 first:pt-0 last:pb-0">
              <p className="text-xs font-medium text-gray-500">{chefItems[0].chefName}</p>
              {chefItems.map((item) => (
                <div key={item.foodItemId} className="mt-1 flex justify-between text-sm">
                  <span className="text-gray-700">{item.dishName} × {item.quantity}</span>
                  <span className="font-medium text-gray-900">PKR {(item.unitPrice * item.quantity).toLocaleString()}</span>
                </div>
              ))}
            </div>
          ))}
        </div>
        <div className="mt-4 border-t border-gray-200 pt-3">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-700">Total</span>
            <span className="text-lg font-bold text-gray-900">PKR {totalPrice.toLocaleString()}</span>
          </div>
        </div>
      </div>

      {error && <p className="mt-4 text-sm text-red-600">{error}</p>}

      <button
        onClick={handleOrder}
        disabled={loading}
        className="mt-6 block w-full rounded-lg bg-orange-500 py-3 text-center text-sm font-semibold text-white hover:bg-orange-600 disabled:opacity-50"
      >
        {loading ? "Placing Order..." : "Place Order via WhatsApp"}
      </button>

      <Link href="/cart" className="mt-3 block text-center text-sm text-orange-500 hover:text-orange-600">
        Back to Cart
      </Link>
    </div>
  );
}
