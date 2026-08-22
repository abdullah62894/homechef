"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  listAdminUsers,
  listAdminReviews,
  suspendUser,
  restoreUser,
  deleteAdminReview,
  deleteAdminFood,
  deleteAdminChef,
  type AdminUser,
  type AdminReview,
} from "@/lib/admin";
import { listFoods } from "@/lib/foods";
import { ApiError } from "@/lib/api";

type Access =
  | { status: "checking" }
  | { status: "denied" }
  | { status: "allowed" };

export default function AdminConsolePage() {
  const [access, setAccess] = useState<Access>({ status: "checking" });

  const [users, setUsers] = useState<AdminUser[]>([]);
  const [userSearch, setUserSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState("");
  const [loadingUsers, setLoadingUsers] = useState(true);

  const [reviews, setReviews] = useState<AdminReview[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(true);

  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadUsers = useCallback(async () => {
    const page = await listAdminUsers(userSearch, roleFilter || undefined, 1, 50);
    return page.items;
  }, [userSearch, roleFilter]);

  const loadReviews = useCallback(async () => {
    const page = await listAdminReviews(1, 20);
    return page.items;
  }, []);

  useEffect(() => {
    let cancelled = false;

    listAdminUsers("", undefined, 1, 1)
      .then(() => {
        if (!cancelled) setAccess({ status: "allowed" });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
          setAccess({ status: "denied" });
        } else {
          setAccess({ status: "denied" });
          setError(err instanceof ApiError ? err.message : "Unable to open the admin console.");
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    if (access.status !== "allowed") return;
    let cancelled = false;

    setLoadingUsers(true);
    loadUsers()
      .then((items) => {
        if (!cancelled) setUsers(items);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : "Unable to load users.");
      })
      .finally(() => {
        if (!cancelled) setLoadingUsers(false);
      });

    return () => {
      cancelled = true;
    };
  }, [access.status, loadUsers]);

  useEffect(() => {
    if (access.status !== "allowed") return;
    let cancelled = false;

    setLoadingReviews(true);
    loadReviews()
      .then((items) => {
        if (!cancelled) setReviews(items);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : "Unable to load reviews.");
      })
      .finally(() => {
        if (!cancelled) setLoadingReviews(false);
      });

    return () => {
      cancelled = true;
    };
  }, [access.status, loadReviews]);

  async function runAction(id: string, action: () => Promise<void>, doneMessage: string) {
    setBusyId(id);
    setError(null);
    setSuccess(null);
    try {
      await action();
      setSuccess(doneMessage);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Action failed.");
    } finally {
      setBusyId(null);
    }
  }

  async function handleSuspend(user: AdminUser) {
    await runAction(user.id, async () => {
      await suspendUser(user.id);
      setUsers((prev) => prev.map((u) => (u.id === user.id ? { ...u, isSuspended: true } : u)));
    }, `${user.email} was suspended.`);
  }

  async function handleRestore(user: AdminUser) {
    await runAction(user.id, async () => {
      await restoreUser(user.id);
      setUsers((prev) => prev.map((u) => (u.id === user.id ? { ...u, isSuspended: false } : u)));
    }, `${user.email} was restored.`);
  }

  async function handleDeleteReview(review: AdminReview) {
    if (!window.confirm(`Delete this ${review.rating}★ review by ${review.reviewerName}?`)) return;
    await runAction(review.id, async () => {
      await deleteAdminReview(review.id);
      setReviews((prev) => prev.filter((r) => r.id !== review.id));
    }, "Review was removed.");
  }

  async function handleRemoveKitchen(user: AdminUser) {
    if (!user.chefProfileId) return;
    if (
      !window.confirm(
        `Permanently remove ${user.email}'s kitchen? All of its dishes, reviews, messages and favorites are deleted.`
      )
    ) {
      return;
    }
    await runAction(user.id, async () => {
      await deleteAdminChef(user.chefProfileId!);
      setUsers((prev) =>
        prev.map((u) => (u.id === user.id ? { ...u, chefProfileId: null } : u))
      );
    }, "Kitchen was removed.");
  }

  async function handleDeleteFood(foodId: string, name: string) {
    if (!window.confirm(`Delete dish "${name}"?`)) return;
    await runAction(foodId, async () => {
      await deleteAdminFood(foodId);
    }, `"${name}" was deleted.`);
  }

  if (access.status === "checking") {
    return <section className="mx-auto max-w-5xl px-4 py-16 text-gray-600">Checking access…</section>;
  }

  if (access.status === "denied") {
    return (
      <section className="mx-auto max-w-5xl px-4 py-16">
        <h1 className="text-3xl font-bold tracking-tight">Admin console</h1>
        <div className="mt-6 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error ?? "You need an administrator account to open this page."}
        </div>
        <Link href="/" className="mt-6 inline-block text-sm text-gray-600 underline">
          ← Back to home
        </Link>
      </section>
    );
  }

  return (
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <h1 className="text-3xl font-bold tracking-tight">Admin console</h1>
      <p className="mt-1 text-sm text-gray-600">
        Moderate accounts, reviews and dishes. Actions are immediate.
      </p>

      {error && (
        <div className="mt-6 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">{error}</div>
      )}
      {success && (
        <div className="mt-6 rounded-lg border border-green-200 bg-green-50 p-4 text-sm text-green-700">
          {success}
        </div>
      )}

      {/* Users */}
      <div className="mt-10">
        <h2 className="text-xl font-bold tracking-tight">Users</h2>
        <div className="mt-4 flex flex-wrap gap-2">
          <input
            type="search"
            placeholder="Search email or name…"
            value={userSearch}
            onChange={(e) => setUserSearch(e.target.value)}
            className="w-full max-w-xs rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
          />
          <select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
          >
            <option value="">All roles</option>
            <option value="Customer">Customer</option>
            <option value="Chef">Chef</option>
            <option value="Admin">Admin</option>
          </select>
        </div>

        {loadingUsers ? (
          <p className="mt-4 text-sm text-gray-500">Loading users…</p>
        ) : users.length === 0 ? (
          <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
            No users match this filter.
          </p>
        ) : (
          <div className="mt-4 overflow-x-auto rounded-xl border border-gray-200 bg-white">
            <table className="min-w-full divide-y divide-gray-200 text-left text-sm">
              <thead className="bg-gray-50 text-xs font-semibold uppercase tracking-wider text-gray-500">
                <tr>
                  <th className="px-4 py-3">User</th>
                  <th className="px-4 py-3">Roles</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {users.map((user) => (
                  <tr key={user.id} className="hover:bg-gray-50/70">
                    <td className="px-4 py-3">
                      <div className="font-semibold text-gray-900">{user.email}</div>
                      <div className="text-xs text-gray-500">
                        {user.firstName} {user.lastName} · joined{" "}
                        {new Date(user.createdAtUtc).toLocaleDateString()}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-gray-600">{user.roles.join(", ") || "—"}</td>
                    <td className="px-4 py-3">
                      {user.isSuspended ? (
                        <span className="rounded-full border border-red-200 bg-red-50 px-2.5 py-0.5 text-xs font-semibold text-red-700">
                          Suspended
                        </span>
                      ) : (
                        <span className="rounded-full border border-emerald-200 bg-emerald-50 px-2.5 py-0.5 text-xs font-semibold text-emerald-700">
                          Active
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right space-x-2 whitespace-nowrap">
                      {user.isSuspended ? (
                        <button
                          type="button"
                          onClick={() => handleRestore(user)}
                          disabled={busyId === user.id}
                          className="font-medium text-emerald-700 hover:text-emerald-900 underline text-xs disabled:opacity-50"
                        >
                          Restore
                        </button>
                      ) : (
                        !user.roles.includes("Admin") && (
                          <button
                            type="button"
                            onClick={() => handleSuspend(user)}
                            disabled={busyId === user.id}
                            className="font-medium text-amber-700 hover:text-amber-900 underline text-xs disabled:opacity-50"
                          >
                            Suspend
                          </button>
                        )
                      )}
                      {user.chefProfileId && (
                        <>
                          <Link
                            href={`/chefs/${user.chefProfileId}`}
                            className="font-medium text-gray-700 hover:text-gray-900 underline text-xs"
                          >
                            View kitchen
                          </Link>
                          <button
                            type="button"
                            onClick={() => handleRemoveKitchen(user)}
                            disabled={busyId === user.id}
                            className="font-medium text-red-600 hover:text-red-800 underline text-xs disabled:opacity-50"
                          >
                            Remove kitchen
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Reviews */}
      <div className="mt-12">
        <h2 className="text-xl font-bold tracking-tight">Recent reviews</h2>
        {loadingReviews ? (
          <p className="mt-4 text-sm text-gray-500">Loading reviews…</p>
        ) : reviews.length === 0 ? (
          <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
            No reviews to moderate.
          </p>
        ) : (
          <ul className="mt-4 space-y-3">
            {reviews.map((review) => (
              <li key={review.id} className="rounded-xl border border-gray-200 bg-white p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div className="text-sm">
                    <span className="font-semibold text-gray-900">{review.chefDisplayName}</span>
                    <span className="text-gray-500"> · reviewed by {review.reviewerName}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-amber-400 text-sm">{"★".repeat(review.rating)}</span>
                    <span className="text-xs text-gray-400">
                      {new Date(review.createdAtUtc).toLocaleString()}
                    </span>
                  </div>
                </div>
                <p className="mt-2 text-sm text-gray-700">{review.comment}</p>
                <div className="mt-3 flex justify-end">
                  <button
                    type="button"
                    onClick={() => handleDeleteReview(review)}
                    disabled={busyId === review.id}
                    className="font-medium text-red-600 hover:text-red-800 underline text-xs disabled:opacity-50"
                  >
                    Delete review
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <p className="mt-10 text-xs text-gray-500">
        To moderate a specific dish, open it from the{" "}
        <Link href="/food" className="underline">
          dish directory
        </Link>{" "}
        and use the delete control shown to admins there.
      </p>
    </section>
  );
}
