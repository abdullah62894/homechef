"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { fetchMe, logoutUser, type UserDto } from "@/lib/auth";

/**
 * Session-aware site header: shows Sign in / Create account only when
 * logged out, and the account link (plus sign out) when authenticated.
 */
export default function SiteHeader() {
  const router = useRouter();
  const [user, setUser] = useState<UserDto | null>(null);
  const [checked, setChecked] = useState(false);

  useEffect(() => {
    let cancelled = false;
    fetchMe()
      .then((me) => {
        if (!cancelled) setUser(me);
      })
      .catch(() => {
        // Not signed in (or API unreachable) — signed-out nav.
      })
      .finally(() => {
        if (!cancelled) setChecked(true);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSignOut() {
    await logoutUser();
    setUser(null);
    router.push("/");
    router.refresh();
  }

  return (
    <header className="border-b">
      <div className="mx-auto flex h-14 max-w-5xl items-center gap-6 px-4">
        <Link
          href="/"
          className="text-lg font-semibold tracking-tight text-gray-900 hover:text-gray-700"
        >
          HomeChef
        </Link>
        <nav className="ml-auto flex items-center gap-4 overflow-x-auto text-sm [-ms-overflow-style:none] [scrollbar-width:none] sm:gap-4 [&::-webkit-scrollbar]:hidden">
          <Link href="/food" className="whitespace-nowrap text-gray-600 hover:text-gray-900 font-medium">
            Explore Food
          </Link>
          <Link href="/chefs" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
            Chefs
          </Link>
          <Link href="/locations" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
            Locations
          </Link>
          <Link href="/search" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
            Search
          </Link>
          {user ? (
            <>
              <Link href="/favorites" className="whitespace-nowrap text-gray-600 hover:text-gray-900">
                Favorites
              </Link>
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
      </div>
    </header>
  );
}
