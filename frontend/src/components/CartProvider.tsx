"use client";

import { createContext, useContext, useState, useCallback, type ReactNode } from "react";

export interface CartItem {
  foodItemId: string;
  chefProfileId: string;
  chefName: string;
  dishName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
  imageUrl?: string | null;
}

interface CartContextValue {
  items: CartItem[];
  addItem: (item: Omit<CartItem, "quantity">, quantity?: number) => void;
  removeItem: (foodItemId: string) => void;
  updateQuantity: (foodItemId: string, quantity: number) => void;
  clearCart: () => void;
  totalItems: number;
  totalPrice: number;
  itemsByChef: Record<string, CartItem[]>;
}

const CartContext = createContext<CartContextValue | null>(null);

const CART_KEY = "homechef_cart";

function loadCart(): CartItem[] {
  if (typeof window === "undefined") return [];
  try {
    const raw = localStorage.getItem(CART_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveCart(items: CartItem[]) {
  if (typeof window === "undefined") return;
  localStorage.setItem(CART_KEY, JSON.stringify(items));
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartItem[]>(() => loadCart());

  const persist = useCallback((next: CartItem[]) => {
    setItems(next);
    saveCart(next);
  }, []);

  const addItem = useCallback(
    (item: Omit<CartItem, "quantity">, quantity = 1) => {
      const existingIndex = items.findIndex((i) => i.foodItemId === item.foodItemId);
      if (existingIndex >= 0) {
        persist(items.map((i, idx) => idx === existingIndex ? { ...i, quantity: i.quantity + quantity } : i));
      } else {
        persist([...items, { ...item, quantity }]);
      }
    },
    [items, persist]
  );

  const removeItem = useCallback(
    (foodItemId: string) => persist(items.filter((i) => i.foodItemId !== foodItemId)),
    [items, persist]
  );

  const updateQuantity = useCallback(
    (foodItemId: string, quantity: number) => {
      if (quantity <= 0) return removeItem(foodItemId);
      persist(items.map((i) => (i.foodItemId === foodItemId ? { ...i, quantity } : i)));
    },
    [items, persist, removeItem]
  );

  const clearCart = useCallback(() => persist([]), [persist]);

  const totalItems = items.reduce((sum, i) => sum + i.quantity, 0);
  const totalPrice = items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);

  const itemsByChef = items.reduce<Record<string, CartItem[]>>((acc, item) => {
    (acc[item.chefProfileId] ??= []).push(item);
    return acc;
  }, {});

  return (
    <CartContext.Provider
      value={{ items, addItem, removeItem, updateQuantity, clearCart, totalItems, totalPrice, itemsByChef }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error("useCart must be used within CartProvider");
  return ctx;
}
