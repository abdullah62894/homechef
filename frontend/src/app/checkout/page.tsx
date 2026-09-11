"use client";

import { useCart } from "@/components/CartProvider";
import { getChef, type ChefProfile } from "@/lib/chefs";
import { fetchMe, type UserDto } from "@/lib/auth";
import { useState, useEffect } from "react";
import Link from "next/link";

function buildWhatsAppMessage(
  chefName: string,
  items: { dishName: string; quantity: number; unitPrice: number; currency: string }[],
  total: number,
  customerName: string,
  customerPhone: string,
  deliveryAddress: string,
): string {
  const lines: string[] = [];
  lines.push(`Hello Chef ${chefName},`);
  lines.push("");
  lines.push("I would like to place an order:");
  lines.push("");
  lines.push(`Chef: ${chefName}`);
  lines.push("");
  lines.push("Items:");

  for (const item of items) {
    lines.push(`• ${item.dishName} × ${item.quantity} — ${item.currency} ${(item.unitPrice * item.quantity).toLocaleString()}`);
  }

  lines.push("");
  lines.push(`Total: PKR ${total.toLocaleString()}`);
  lines.push("");

  if (customerName || customerPhone) {
    lines.push("Customer:");
    if (customerName) lines.push(`Name: ${customerName}`);
    if (customerPhone) lines.push(`Phone: ${customerPhone}`);
    lines.push("");
  }

  if (deliveryAddress) {
    lines.push("Delivery:");
    lines.push(deliveryAddress);
    lines.push("");
  }

  lines.push("Please confirm availability and delivery details.");
  return lines.join("\n");
}

function buildWhatsAppUrl(phone: string, message: string): string {
  const cleaned = phone.replace(/[^0-9+]/g, "");
  const encoded = encodeURIComponent(message);
  return `https://wa.me/${cleaned}?text=${encoded}`;
}

export default function CheckoutPage() {
  const { items, itemsByChef, totalPrice, clearCart } = useCart();
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deliveryAddress, setDeliveryAddress] = useState("");
  const [customerPhone, setCustomerPhone] = useState("");
  const [customerName, setCustomerName] = useState("");
  const [chefProfiles, setChefProfiles] = useState<Record<string, ChefProfile>>({});
  const [whatsappLinks, setWhatsappLinks] = useState<{ chefName: string; url: string }[] | null>(null);

  useEffect(() => {
    fetchMe().then(setUser).catch(() => {});
  }, []);

  useEffect(() => {
    const chefIds = Object.keys(itemsByChef);
    if (chefIds.length === 0) return;

    Promise.all(
      chefIds.map((id) => getChef(id).then((p) => [id, p] as const).catch(() => null))
    ).then((results) => {
      const map: Record<string, ChefProfile> = {};
      for (const r of results) {
        if (r) map[r[0]] = r[1];
      }
      setChefProfiles(map);
    });
  }, [itemsByChef]);

  if (items.length === 0 && !whatsappLinks) {
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

  function handleOrder() {
    setError(null);

    const name = customerName.trim() || (user ? `${user.firstName} ${user.lastName}` : "");
    const phone = customerPhone.trim();
    const address = deliveryAddress.trim();

    if (!name) {
      setError("Please enter your name.");
      return;
    }

    const chefIds = Object.keys(itemsByChef);
    const links: { chefName: string; url: string }[] = [];

    for (const chefId of chefIds) {
      const chefItems = itemsByChef[chefId];
      const chefProfile = chefProfiles[chefId];
      const chefName = chefItems[0].chefName;
      const chefTotal = chefItems.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);

      const phoneNum = chefProfile?.whatsAppNumber || chefProfile?.phoneNumber;
      if (!phoneNum) {
        setError(`${chefName} has not set up a phone number for WhatsApp orders yet.`);
        return;
      }

      const message = buildWhatsAppMessage(chefName, chefItems, chefTotal, name, phone, address);
      const url = buildWhatsAppUrl(phoneNum, message);
      links.push({ chefName, url });
    }

    setWhatsappLinks(links);
  }

  if (whatsappLinks) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-green-100 text-3xl">
          ✅
        </div>
        <h1 className="text-2xl font-bold text-gray-900">Order Ready!</h1>
        <p className="mt-2 text-gray-600">Click below to open WhatsApp and send your order to the chef.</p>

        <div className="mt-8 space-y-4">
          {whatsappLinks.map((link) => (
            <a
              key={link.chefName}
              href={link.url}
              target="_blank"
              rel="noopener noreferrer"
              onClick={() => clearCart()}
              className="block w-full rounded-xl bg-green-500 px-6 py-4 text-base font-semibold text-white shadow hover:bg-green-600 transition"
            >
              Order from {link.chefName} via WhatsApp
            </a>
          ))}
        </div>

        <Link href="/orders" className="mt-8 inline-block text-sm text-gray-600 underline hover:text-gray-900">
          View my orders
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Checkout</h1>

      <div className="mt-6 space-y-4 rounded-lg border border-gray-200 bg-white p-4">
        <h2 className="text-sm font-semibold text-gray-700">Your Details</h2>
        <div>
          <label className="block text-xs font-medium text-gray-700">Full Name *</label>
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

      {error && <p className="mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <button
        onClick={handleOrder}
        disabled={loading}
        className="mt-6 block w-full rounded-lg bg-green-500 py-3 text-center text-sm font-semibold text-white hover:bg-green-600 disabled:opacity-50"
      >
        {loading ? "Preparing..." : "Order via WhatsApp"}
      </button>

      <Link href="/cart" className="mt-3 block text-center text-sm text-orange-500 hover:text-orange-600">
        Back to Cart
      </Link>
    </div>
  );
}
