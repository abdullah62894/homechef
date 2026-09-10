"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { fetchMe, logoutUser, type UserDto } from "@/lib/auth";
import { useCart } from "@/components/CartProvider";

export default function SiteHeader() {
  const router = useRouter();
  const pathname = usePathname();
  const [user, setUser] = useState<UserDto | null>(null);
  const [checked, setChecked] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const { totalItems } = useCart();

  useEffect(() => {
    let cancelled = false;
    fetchMe()
      .then((me) => {
        if (!cancelled) setUser(me);
      })
      .catch(() => {
        if (!cancelled) setUser(null);
      })
      .finally(() => {
        if (!cancelled) setChecked(true);
      });
    return () => {
      cancelled = true;
    };
  }, [pathname]);

  async function handleSignOut() {
    await logoutUser();
    setUser(null);
    setMobileMenuOpen(false);
    router.push("/");
    router.refresh();
  }

  return (
    <header className="sticky top-0 z-50 border-b bg-white/95 backdrop-blur-sm">
      <div className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4">
        <Link
          href="/"
          className="text-lg font-bold tracking-tight text-gray-900 hover:text-orange-600 transition-colors"
        >
          HomeChef
        </Link>

        {/* Desktop nav */}
        <nav className="hidden sm:flex items-center gap-4 text-sm">
          <Link href="/cuisines" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
            Cuisines
          </Link>
          <Link href="/food" className="whitespace-nowrap text-gray-600 hover:text-gray-900 font-medium">
            Explore Food
          </Link>
          <Link href="/chefs" className="whitespace-nowrap text-gray-600 hover:text-gray-900 font-medium">
            Chefs
          </Link>
          <Link href="/locations" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
            Locations
          </Link>
          <Link href="/search" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
            Search
          </Link>
          <Link href="/cart" className="relative whitespace-nowrap text-gray-600 hover:text-gray-900">
            Cart
            {totalItems > 0 && (
              <span className="absolute -top-2 -right-3 flex h-4 w-4 items-center justify-center rounded-full bg-orange-500 text-[10px] font-bold text-white">
                {totalItems}
              </span>
            )}
          </Link>
          {user ? (
            <>
              <Link href="/favorites" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
                Favorites
              </Link>
              <Link href="/orders" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
                Orders
              </Link>
              {user.roles.includes("Chef") && (
                <Link
                  href="/chefs/me"
                  className="whitespace-nowrap text-gray-600 hover:text-gray-900"
                >
                  My kitchen
                </Link>
              )}
              <Link href="/me" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
                My account
              </Link>
              <button
                type="button"
                onClick={handleSignOut}
                className="whitespace-nowrap rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium text-gray-700 hover:bg-gray-50"
              >
                Sign out
              </button>
            </>
          ) : (
            checked && (
              <>
                <Link href="/login" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
                  Sign in
                </Link>
                <Link
                  href="/register"
                  className="whitespace-nowrap rounded-lg bg-gray-900 px-3 py-1.5 text-xs font-medium text-white hover:bg-gray-800"
                >
                  Create account
                </Link>
              </>
            )
          )}
        </nav>

        {/* Mobile hamburger */}
        <button
          type="button"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          className="sm:hidden flex items-center justify-center rounded-lg p-2 text-gray-700 hover:bg-gray-100"
          aria-label="Toggle menu"
        >
          {mobileMenuOpen ? (
            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          ) : (
            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          )}
        </button>
      </div>

      {/* Mobile menu */}
      {mobileMenuOpen && (
        <div className="sm:hidden border-t bg-white">
          <nav className="mx-auto max-w-5xl px-4 py-3 space-y-1">
            <Link href="/cuisines" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
              Cuisines
            </Link>
            <Link href="/food" className="block rounded-lg px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
              Explore Food
            </Link>
            <Link href="/chefs" className="block rounded-lg px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
              Chefs
            </Link>
            <Link href="/locations" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
              Locations
            </Link>
            <Link href="/search" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
              Search
            </Link>
            <Link href="/cart" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
              Cart {totalItems > 0 && `(${totalItems})`}
            </Link>
            {user ? (
              <>
                <Link href="/favorites" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
                  Favorites
                </Link>
                <Link href="/orders" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
                  Orders
                </Link>
                {user.roles.includes("Chef") && (
                  <Link href="/chefs/me" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
                    My kitchen
                  </Link>
                )}
                <Link href="/me" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
                  My account
                </Link>
                <button
                  type="button"
                  onClick={handleSignOut}
                  className="w-full text-left rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50"
                >
                  Sign out
                </button>
              </>
            ) : (
              checked && (
                <>
                  <Link href="/login" className="block rounded-lg px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">
                    Sign in
                  </Link>
                  <Link
                    href="/register"
                    className="block rounded-lg bg-gray-900 px-3 py-2 text-center text-sm font-medium text-white hover:bg-gray-800"
                  >
                    Create account
                  </Link>
                </>
              )
            )}
          </nav>
        </div>
      )}
    </header>
  );
}
