"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { fetchMe, type UserDto } from "@/lib/auth";

/**
 * Session-aware CTA for the "Become a Home Chef" journey: chefs go
 * straight to their kitchen dashboard instead of the register page.
 */
export default function BecomeChefCTA({ variant = "primary" }: { variant?: "primary" | "light" }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [checked, setChecked] = useState(false);

  useEffect(() => {
    let cancelled = false;
    fetchMe()
      .then((me) => {
        if (!cancelled) setUser(me);
      })
      .catch(() => {})
      .finally(() => {
        if (!cancelled) setChecked(true);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const primaryClass =
    variant === "light"
      ? "mt-4 inline-block rounded-xl bg-white px-6 py-3 text-sm font-semibold text-gray-900 transition hover:bg-orange-50"
      : "mt-8 inline-block rounded-xl bg-orange-600 px-6 py-3.5 text-sm font-semibold text-white shadow-sm transition hover:bg-orange-700";

  if (!checked) {
    return null;
  }

  if (user?.roles.includes("Chef")) {
    return (
      <Link href="/chefs/me" className={primaryClass}>
        Open your kitchen dashboard →
      </Link>
    );
  }

  return (
    <div>
      <Link href="/register?role=Chef" className={primaryClass}>
        Create your free chef account →
      </Link>
      {user && (
        <p className="mt-3 text-xs text-gray-400">
          You&apos;re signed in as a customer. Chef kitchens use a separate chef
          account — sign out first, then register as a chef.
        </p>
      )}
    </div>
  );
}
