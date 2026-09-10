"use client";

import { useCart } from "@/components/CartProvider";
import Link from "next/link";
import Image from "next/image";
import { resolveImageUrl } from "@/lib/images";
import { useState } from "react";

export default function CartPage() {
  const { items, removeItem, updateQuantity, clearCart, totalItems, totalPrice, itemsByChef } = useCart();
  const [ordering, setOrdering] = useState(false);

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900">Your cart is empty</h1>
        <p className="mt-2 text-gray-500">Browse our chefs and add some delicious dishes.</p>
        <Link href="/food" className="mt-6 inline-block rounded-lg bg-orange-500 px-6 py-3 text-sm font-semibold text-white hover:bg-orange-600">
          Browse Dishes
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Your Cart ({totalItems} items)</h1>
        <button onClick={clearCart} className="text-sm text-red-500 hover:text-red-600">Clear cart</button>
      </div>

      <div className="mt-6 space-y-6">
        {Object.entries(itemsByChef).map(([chefId, chefItems]) => (
          <div key={chefId} className="rounded-lg border border-gray-200 bg-white p-4">
            <h2 className="text-sm font-semibold text-gray-700">
              Chef: {chefItems[0].chefName}
            </h2>
            <div className="mt-3 divide-y divide-gray-100">
              {chefItems.map((item) => (
                <div key={item.foodItemId} className="flex items-center gap-3 py-3 first:pt-0 last:pb-0">
                  {item.imageUrl && (
                    <Image src={resolveImageUrl(item.imageUrl) ?? ""} alt={item.dishName} width={48} height={48} className="h-12 w-12 rounded-lg object-cover" />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">{item.dishName}</p>
                    <p className="text-xs text-gray-500">{item.currency} {item.unitPrice.toLocaleString()} each</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <button onClick={() => updateQuantity(item.foodItemId, item.quantity - 1)} className="h-6 w-6 rounded-full border text-gray-600 hover:bg-gray-100">-</button>
                    <span className="w-6 text-center text-sm">{item.quantity}</span>
                    <button onClick={() => updateQuantity(item.foodItemId, item.quantity + 1)} className="h-6 w-6 rounded-full border text-gray-600 hover:bg-gray-100">+</button>
                  </div>
                  <p className="w-20 text-right text-sm font-medium text-gray-900">
                    {item.currency} {(item.unitPrice * item.quantity).toLocaleString()}
                  </p>
                  <button onClick={() => removeItem(item.foodItemId)} className="text-xs text-red-500 hover:text-red-600">Remove</button>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>

      <div className="mt-6 rounded-lg border border-gray-200 bg-gray-50 p-4">
        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-gray-700">Total</span>
          <span className="text-lg font-bold text-gray-900">PKR {totalPrice.toLocaleString()}</span>
        </div>
      </div>

      <Link href="/checkout" className="mt-4 block w-full rounded-lg bg-orange-500 py-3 text-center text-sm font-semibold text-white hover:bg-orange-600">
        Proceed to Checkout
      </Link>
    </div>
  );
}
