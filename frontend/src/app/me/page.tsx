"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { fetchMe, logoutUser, type UserDto } from "@/lib/auth";
import { ApiError } from "@/lib/api";

export default function MePage() {
  const router = useRouter();
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    fetchMe()
      .then((me) => {
        if (!cancelled) setUser(me);
      })
      .catch((err) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 401) {
          router.replace("/login");
          return;
        }
        setError(err instanceof ApiError ? err.message : "Unable to load your account.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [router]);

  async function handleLogout() {
    await logoutUser();
    router.push("/");
  }

  if (loading) {
    return (
      <section className="mx-auto max-w-5xl px-4 py-16 text-gray-600">
        Loading your account…
      </section>
    );
  }

  if (error) {
    return (
      <section className="mx-auto max-w-5xl px-4 py-16">
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      </section>
    );
  }

  if (!user) return null;

  return (
    <section className="mx-auto max-w-5xl px-4 py-16">
      <h1 className="text-3xl font-bold tracking-tight">My account</h1>
      <div className="mt-8 rounded-xl border border-gray-200 p-6">
        <dl className="grid gap-4 sm:grid-cols-2">
          <div>
            <dt className="text-sm font-medium text-gray-500">Name</dt>
            <dd className="mt-1">
              {user.firstName} {user.lastName}
            </dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-gray-500">Email</dt>
            <dd className="mt-1">{user.email}</dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-gray-500">Role</dt>
            <dd className="mt-1">{user.roles.join(", ")}</dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-gray-500">Member since</dt>
            <dd className="mt-1">
              {new Date(user.createdAtUtc).toLocaleDateString()}
            </dd>
          </div>
        </dl>
      </div>

      <div className="mt-8 flex items-center gap-4">
        <button
          type="button"
          onClick={handleLogout}
          className="rounded-lg border border-gray-300 px-4 py-2 font-medium hover:bg-gray-50"
        >
          Sign out
        </button>
        <Link href="/" className="text-sm text-gray-600 underline">
          Back to home
        </Link>
      </div>
    </section>
  );
}