"use client";

import { useCallback, useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import {
  listAdminUsers,
  listAdminReviews,
  listAdminReports,
  suspendUser,
  restoreUser,
  deleteAdminReview,
  deleteAdminChef,
  resolveReport,
  dismissReport,
  createAdminUser,
  updateAdminUser,
  setAdminUserPassword,
  deleteAdminUser,
  approveChef,
  rejectChef,
  uploadCuisineImage,
  createCuisine,
  updateCuisine,
  deleteCuisine,
  type AdminUser,
  type AdminReview,
} from "@/lib/admin";
import type { Report } from "@/lib/reports";
import { ApiError } from "@/lib/api";
import { chefHref } from "@/lib/slugs";
import { listAllCuisines, type Cuisine } from "@/lib/cuisines";
import { listContactMessages, type ContactMessage } from "@/lib/contact";
import { resolveImageUrl } from "@/lib/images";

type AdminTab = "users" | "reports" | "reviews" | "cuisines" | "messages";

type Access =
  | { status: "checking" }
  | { status: "denied" }
  | { status: "allowed" };

type UserModalMode =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; user: AdminUser }
  | { mode: "password"; user: AdminUser };

const emptyCreateForm = { email: "", password: "", firstName: "", lastName: "", role: "Customer" as const };
type EditRole = "" | "Customer" | "Chef" | "Admin";
const emptyEditForm: { firstName: string; lastName: string; role: EditRole } = { firstName: "", lastName: "", role: "" };

export default function AdminConsolePage() {
  const [access, setAccess] = useState<Access>({ status: "checking" });

  const [users, setUsers] = useState<AdminUser[]>([]);
  const [userSearch, setUserSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState("");
  const [loadingUsers, setLoadingUsers] = useState(true);

  const [reviews, setReviews] = useState<AdminReview[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(true);

  const [reports, setReports] = useState<Report[]>([]);
  const [loadingReports, setLoadingReports] = useState(true);

  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [activeTab, setActiveTab] = useState<AdminTab>("users");
  const [cuisines, setCuisines] = useState<Cuisine[]>([]);
  const [loadingCuisines, setLoadingCuisines] = useState(true);
  const [messages, setMessages] = useState<ContactMessage[]>([]);
  const [loadingMessages, setLoadingMessages] = useState(true);

  const [userModal, setUserModal] = useState<UserModalMode>({ mode: "closed" });
  const [createForm, setCreateForm] = useState(emptyCreateForm);
  const [editForm, setEditForm] = useState(emptyEditForm);
  const [passwordForm, setPasswordForm] = useState("");
  const [modalSubmitting, setModalSubmitting] = useState(false);

  const [cuisineModal, setCuisineModal] = useState<{ mode: "closed" } | { mode: "create" } | { mode: "edit"; cuisine: Cuisine }>({ mode: "closed" });
  const [cuisineForm, setCuisineForm] = useState({ name: "", slug: "", description: "", displayOrder: 0, isActive: true });
  const [cuisineSubmitting, setCuisineSubmitting] = useState(false);
  const [cuisineImageBusy, setCuisineImageBusy] = useState<string | null>(null);

  const loadUsers = useCallback(async () => {
    const page = await listAdminUsers(userSearch, roleFilter || undefined, 1, 50);
    return page.items;
  }, [userSearch, roleFilter]);

  const loadReviews = useCallback(async () => {
    const page = await listAdminReviews(1, 20);
    return page.items;
  }, []);

  const loadReports = useCallback(async () => {
    const page = await listAdminReports("Open", 1, 20);
    return page.items;
  }, []);

  useEffect(() => {
    let cancelled = false;

    listAdminUsers("", undefined, 1, 1)
      .then(() => {
        if (!cancelled) setAccess({ status: "allowed" });
      })
      .catch(() => {
        if (!cancelled) setAccess({ status: "denied" });
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

  useEffect(() => {
    if (access.status !== "allowed") return;
    let cancelled = false;

    setLoadingReports(true);
    loadReports()
      .then((items) => {
        if (!cancelled) setReports(items);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : "Unable to load reports.");
      })
      .finally(() => {
        if (!cancelled) setLoadingReports(false);
      });

    return () => {
      cancelled = true;
    };
  }, [access.status, loadReports]);

  useEffect(() => {
    if (access.status !== "allowed") return;
    let cancelled = false;
    setLoadingCuisines(true);
    listAllCuisines()
      .then((items) => { if (!cancelled) setCuisines(items); })
      .catch(() => {})
      .finally(() => { if (!cancelled) setLoadingCuisines(false); });
    return () => { cancelled = true; };
  }, [access.status]);

  useEffect(() => {
    if (access.status !== "allowed") return;
    let cancelled = false;
    setLoadingMessages(true);
    listContactMessages(1, 50)
      .then((items) => { if (!cancelled) setMessages(items); })
      .catch(() => {})
      .finally(() => { if (!cancelled) setLoadingMessages(false); });
    return () => { cancelled = true; };
  }, [access.status]);

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

  function openCreateUser() {
    setCreateForm(emptyCreateForm);
    setError(null);
    setSuccess(null);
    setUserModal({ mode: "create" });
  }

  function openEditUser(user: AdminUser) {
    setEditForm({
      firstName: user.firstName,
      lastName: user.lastName,
      role: user.roles.includes("Admin") ? "Admin" : user.roles.includes("Chef") ? "Chef" : "Customer",
    });
    setError(null);
    setSuccess(null);
    setUserModal({ mode: "edit", user });
  }

  function openSetPassword(user: AdminUser) {
    setPasswordForm("");
    setError(null);
    setSuccess(null);
    setUserModal({ mode: "password", user });
  }

  function closeUserModal() {
    setUserModal({ mode: "closed" });
    setModalSubmitting(false);
  }

  async function refreshUsers() {
    const items = await loadUsers();
    setUsers(items);
  }

  async function handleCreateUser(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setModalSubmitting(true);
    setError(null);
    try {
      await createAdminUser(createForm);
      setSuccess(`${createForm.email} was created.`);
      closeUserModal();
      await refreshUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to create the user.");
    } finally {
      setModalSubmitting(false);
    }
  }

  async function handleEditUser(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (userModal.mode !== "edit") return;
    setModalSubmitting(true);
    setError(null);
    try {
      await updateAdminUser(userModal.user.id, {
        firstName: editForm.firstName,
        lastName: editForm.lastName,
        role: editForm.role || undefined,
      });
      setSuccess(`${userModal.user.email} was updated.`);
      closeUserModal();
      await refreshUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to update the user.");
    } finally {
      setModalSubmitting(false);
    }
  }

  async function handleSetPassword(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (userModal.mode !== "password") return;
    setModalSubmitting(true);
    setError(null);
    try {
      await setAdminUserPassword(userModal.user.id, passwordForm);
      setSuccess(`Password was reset for ${userModal.user.email}.`);
      closeUserModal();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to set the password.");
    } finally {
      setModalSubmitting(false);
    }
  }

  async function handleDeleteUser(user: AdminUser) {
    if (
      !window.confirm(
        `Permanently delete ${user.email}? Their kitchen and all its content will be removed too.`
      )
    ) {
      return;
    }
    await runAction(user.id, async () => {
      await deleteAdminUser(user.id);
      setUsers((prev) => prev.filter((u) => u.id !== user.id));
    }, `${user.email} was deleted.`);
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
      await refreshUsers();
    }, "Kitchen was removed.");
  }

  async function handleApproveChef(user: AdminUser) {
    if (!user.chefProfileId) return;
    await runAction(user.id, async () => {
      await approveChef(user.chefProfileId!);
      await refreshUsers();
    }, `${user.email}'s kitchen was approved.`);
  }

  async function handleRejectChef(user: AdminUser) {
    if (!user.chefProfileId) return;
    const reason = window.prompt("Rejection reason (optional):");
    await runAction(user.id, async () => {
      await rejectChef(user.chefProfileId!, reason || undefined);
      await refreshUsers();
    }, `${user.email}'s kitchen was rejected.`);
  }

  async function handleResolveReport(report: Report) {
    await runAction(report.id, async () => {
      await resolveReport(report.id);
      setReports((prev) => prev.filter((r) => r.id !== report.id));
    }, "Report was resolved.");
  }

  async function handleDismissReport(report: Report) {
    await runAction(report.id, async () => {
      await dismissReport(report.id);
      setReports((prev) => prev.filter((r) => r.id !== report.id));
    }, "Report was dismissed.");
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
        Manage accounts and moderate content. Actions are immediate.
      </p>

      {error && (
        <div className="mt-6 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">{error}</div>
      )}
      {success && (
        <div className="mt-6 rounded-lg border border-green-200 bg-green-50 p-4 text-sm text-green-700">
          {success}
        </div>
      )}

      {/* Tab Navigation */}
      <div className="mt-6 flex flex-wrap gap-1 border-b border-gray-200">
        {(["users", "reports", "reviews", "cuisines", "messages"] as AdminTab[]).map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2.5 text-sm font-medium border-b-2 transition ${
              activeTab === tab
                ? "border-gray-900 text-gray-900"
                : "border-transparent text-gray-500 hover:text-gray-700"
            }`}
          >
            {tab === "users" && "Users"}
            {tab === "reports" && "Reports"}
            {tab === "reviews" && "Reviews"}
            {tab === "cuisines" && "Cuisines"}
            {tab === "messages" && `Messages ${messages.filter(m => !m.isRead).length > 0 ? `(${messages.filter(m => !m.isRead).length})` : ""}`}
          </button>
        ))}
      </div>

      {/* Users Tab */}
      {activeTab === "users" && (
        <div className="mt-6">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-xl font-bold tracking-tight">Users</h2>
            <button
              type="button"
              onClick={openCreateUser}
              className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800"
            >
              + Add user
            </button>
          </div>
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
                      <button
                        type="button"
                        onClick={() => openEditUser(user)}
                        disabled={busyId === user.id}
                        className="font-medium text-gray-700 hover:text-gray-900 underline text-xs disabled:opacity-50"
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        onClick={() => openSetPassword(user)}
                        disabled={busyId === user.id}
                        className="font-medium text-gray-700 hover:text-gray-900 underline text-xs disabled:opacity-50"
                      >
                        Set password
                      </button>
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
                      {/* Kitchen actions exist only for chefs with a kitchen. */}
                      {user.roles.includes("Chef") && user.chefProfileId && (
                        <>
                          <Link
                            href={chefHref(user.chefProfileId)}
                            className="font-medium text-gray-700 hover:text-gray-900 underline text-xs"
                          >
                            View kitchen
                          </Link>
                          <button
                            type="button"
                            onClick={() => handleApproveChef(user)}
                            disabled={busyId === user.id}
                            className="font-medium text-emerald-700 hover:text-emerald-900 underline text-xs disabled:opacity-50"
                          >
                            Approve
                          </button>
                          <button
                            type="button"
                            onClick={() => handleRejectChef(user)}
                            disabled={busyId === user.id}
                            className="font-medium text-amber-700 hover:text-amber-900 underline text-xs disabled:opacity-50"
                          >
                            Reject
                          </button>
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
                      <button
                        type="button"
                        onClick={() => handleDeleteUser(user)}
                        disabled={busyId === user.id}
                        className="font-medium text-red-600 hover:text-red-800 underline text-xs disabled:opacity-50"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        </div>
      )}

      {/* Reports Tab */}
      {activeTab === "reports" && (
        <div className="mt-6">
          <h2 className="text-xl font-bold tracking-tight">Open reports</h2>
          {loadingReports ? (
            <p className="mt-4 text-sm text-gray-500">Loading reports…</p>
          ) : reports.length === 0 ? (
            <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
              No open reports — the community is behaving.
            </p>
          ) : (
            <ul className="mt-4 space-y-3">
              {reports.map((report) => (
                <li key={report.id} className="rounded-xl border border-amber-200 bg-amber-50/50 p-4">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div className="text-sm">
                      <span className="font-semibold text-gray-900">
                        {report.targetType === "ChefProfile"
                          ? "Kitchen"
                          : report.targetType === "FoodItem"
                            ? "Dish"
                            : "Review"}
                      </span>
                      <span className="text-gray-700"> · {report.targetLabel || report.targetId}</span>
                    </div>
                    <div className="flex items-center gap-2 text-xs text-gray-500">
                      <span className="rounded bg-gray-100 px-2 py-0.5 font-medium">{report.reason}</span>
                      <span>
                        {new Date(report.createdAtUtc).toLocaleString()} · by {report.reporterName}
                      </span>
                    </div>
                  </div>
                  {report.details && (
                    <p className="mt-2 text-sm text-gray-700">{report.details}</p>
                  )}
                  <div className="mt-3 flex justify-end gap-3">
                    <button type="button" onClick={() => handleDismissReport(report)} disabled={busyId === report.id}
                      className="font-medium text-gray-600 hover:text-gray-900 underline text-xs disabled:opacity-50">Dismiss</button>
                    <button type="button" onClick={() => handleResolveReport(report)} disabled={busyId === report.id}
                      className="font-medium text-emerald-700 hover:text-emerald-900 underline text-xs disabled:opacity-50">Resolve</button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {/* Reviews Tab */}
      {activeTab === "reviews" && (
        <div className="mt-6">
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
                      <span className="text-xs text-gray-400">{new Date(review.createdAtUtc).toLocaleString()}</span>
                    </div>
                  </div>
                  <p className="mt-2 text-sm text-gray-700">{review.comment}</p>
                  <div className="mt-3 flex justify-end">
                    <button type="button" onClick={() => handleDeleteReview(review)} disabled={busyId === review.id}
                      className="font-medium text-red-600 hover:text-red-800 underline text-xs disabled:opacity-50">Delete review</button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {/* Cuisines Tab */}
      {activeTab === "cuisines" && (
        <div className="mt-6">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold tracking-tight">Cuisines</h2>
            <button
              type="button"
              onClick={() => { setCuisineForm({ name: "", slug: "", description: "", displayOrder: cuisines.length, isActive: true }); setCuisineModal({ mode: "create" }); }}
              className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800"
            >
              + Add cuisine
            </button>
          </div>
          {loadingCuisines ? (
            <p className="mt-4 text-sm text-gray-500">Loading cuisines…</p>
          ) : (
            <div className="mt-4 space-y-3">
              {cuisines.map((cuisine) => (
                <div key={cuisine.id} className="flex items-center gap-4 rounded-xl border border-gray-200 bg-white p-4">
                  {resolveImageUrl(cuisine.imageThumbnailUrl ?? cuisine.imageUrl) ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={resolveImageUrl(cuisine.imageThumbnailUrl ?? cuisine.imageUrl)!} alt={cuisine.name} className="h-14 w-14 rounded-lg object-cover" />
                  ) : (
                    <div className="flex h-14 w-14 items-center justify-center rounded-lg border border-dashed border-gray-300 bg-gray-50 text-lg">🍽️</div>
                  )}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <h3 className="text-sm font-semibold text-gray-900">{cuisine.name}</h3>
                      <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${cuisine.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                        {cuisine.isActive ? "Active" : "Inactive"}
                      </span>
                    </div>
                    <p className="text-xs text-gray-500">#{cuisine.slug} · Order: {cuisine.displayOrder}</p>
                    {cuisine.description && <p className="text-xs text-gray-400 mt-0.5 truncate">{cuisine.description}</p>}
                  </div>
                  <div className="flex items-center gap-2">
                    <label className={`cursor-pointer rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 hover:bg-gray-50 ${cuisineImageBusy === cuisine.id ? "pointer-events-none opacity-50" : ""}`}>
                      {cuisineImageBusy === cuisine.id ? "Uploading…" : "Upload image"}
                      <input type="file" accept="image/jpeg,image/png,image/webp" className="hidden" onChange={async (e) => {
                        const file = e.target.files?.[0];
                        e.target.value = "";
                        if (!file) return;
                        setCuisineImageBusy(cuisine.id);
                        try {
                          await uploadCuisineImage(cuisine.id, file);
                          const updated = await listAllCuisines();
                          setCuisines(updated);
                        } catch (err) {
                          setError(err instanceof Error ? err.message : "Image upload failed.");
                        } finally {
                          setCuisineImageBusy(null);
                        }
                      }} />
                    </label>
                    <button type="button" onClick={() => {
                      setCuisineForm({ name: cuisine.name, slug: cuisine.slug, description: cuisine.description ?? "", displayOrder: cuisine.displayOrder, isActive: cuisine.isActive });
                      setCuisineModal({ mode: "edit", cuisine });
                    }} className="rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 hover:bg-gray-50">Edit</button>
                    <button type="button" onClick={async () => {
                      if (!window.confirm(`Delete cuisine "${cuisine.name}"?`)) return;
                      await runAction(cuisine.id, async () => { await deleteCuisine(cuisine.id); setCuisines(prev => prev.filter(c => c.id !== cuisine.id)); }, `"${cuisine.name}" was deleted.`);
                    }} disabled={busyId === cuisine.id} className="rounded-lg border border-red-200 px-3 py-1.5 text-xs font-medium text-red-600 hover:bg-red-50 disabled:opacity-50">Delete</button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Messages Tab */}
      {activeTab === "messages" && (
        <div className="mt-6">
          <h2 className="text-xl font-bold tracking-tight">Contact Messages</h2>
          {loadingMessages ? (
            <p className="mt-4 text-sm text-gray-500">Loading messages…</p>
          ) : messages.length === 0 ? (
            <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
              No contact messages yet.
            </p>
          ) : (
            <ul className="mt-4 space-y-3">
              {messages.map((msg) => (
                <li key={msg.id} className={`rounded-xl border p-4 ${msg.isRead ? "border-gray-200 bg-white" : "border-blue-200 bg-blue-50/50"}`}>
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div className="text-sm">
                      <span className="font-semibold text-gray-900">{msg.name}</span>
                      <span className="text-gray-500"> · {msg.email} · {msg.phone}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${msg.isRead ? "bg-gray-100 text-gray-500" : "bg-blue-100 text-blue-700"}`}>
                        {msg.status}
                      </span>
                      <span className="text-xs text-gray-400">{new Date(msg.createdAtUtc).toLocaleString()}</span>
                    </div>
                  </div>
                  <p className="mt-2 text-sm text-gray-700">{msg.message}</p>
                  {!msg.isRead && (
                    <div className="mt-3 flex justify-end">
                      <button type="button" onClick={async () => {
                        const { markContactAsRead } = await import("@/lib/contact");
                        await markContactAsRead(msg.id);
                        setMessages(prev => prev.map(m => m.id === msg.id ? { ...m, isRead: true, status: "Read" } : m));
                      }} className="font-medium text-blue-700 hover:text-blue-900 underline text-xs">Mark as read</button>
                    </div>
                  )}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <p className="mt-10 text-xs text-gray-500">
        To moderate a specific dish, open it from the{" "}
        <Link href="/food" className="underline">
          dish directory
        </Link>{" "}
        and use the delete control shown to admins there.
      </p>

      {/* User modal */}
      {userModal.mode !== "closed" && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl max-h-[90vh] overflow-y-auto">
            {userModal.mode === "create" && (
              <>
                <h2 className="text-lg font-bold text-gray-900">Add user</h2>
                <form onSubmit={handleCreateUser} className="mt-4 space-y-3">
                  <div>
                    <label className="block text-xs font-semibold uppercase text-gray-700">Email *</label>
                    <input
                      type="email"
                      required
                      value={createForm.email}
                      onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                      className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold uppercase text-gray-700">Password *</label>
                    <input
                      type="text"
                      required
                      minLength={8}
                      value={createForm.password}
                      onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                      placeholder="Temporary password (8+ chars, upper/lower/digit)"
                      className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-semibold uppercase text-gray-700">First name *</label>
                      <input
                        type="text"
                        required
                        value={createForm.firstName}
                        onChange={(e) => setCreateForm({ ...createForm, firstName: e.target.value })}
                        className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold uppercase text-gray-700">Last name *</label>
                      <input
                        type="text"
                        required
                        value={createForm.lastName}
                        onChange={(e) => setCreateForm({ ...createForm, lastName: e.target.value })}
                        className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold uppercase text-gray-700">Role *</label>
                    <select
                      value={createForm.role}
                      onChange={(e) => setCreateForm({ ...createForm, role: e.target.value as typeof createForm.role })}
                      className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                    >
                      <option value="Customer">Customer</option>
                      <option value="Chef">Chef</option>
                      <option value="Admin">Admin</option>
                    </select>
                  </div>
                  <div className="mt-4 flex justify-end gap-3 border-t pt-4">
                    <button type="button" onClick={closeUserModal} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
                      Cancel
                    </button>
                    <button type="submit" disabled={modalSubmitting} className="rounded-lg bg-gray-900 px-5 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50">
                      {modalSubmitting ? "Creating…" : "Create user"}
                    </button>
                  </div>
                </form>
              </>
            )}

            {userModal.mode === "edit" && (
              <>
                <h2 className="text-lg font-bold text-gray-900">Edit {userModal.user.email}</h2>
                <form onSubmit={handleEditUser} className="mt-4 space-y-3">
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-semibold uppercase text-gray-700">First name *</label>
                      <input
                        type="text"
                        required
                        value={editForm.firstName}
                        onChange={(e) => setEditForm({ ...editForm, firstName: e.target.value })}
                        className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold uppercase text-gray-700">Last name *</label>
                      <input
                        type="text"
                        required
                        value={editForm.lastName}
                        onChange={(e) => setEditForm({ ...editForm, lastName: e.target.value })}
                        className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold uppercase text-gray-700">Role</label>
                    <select
                      value={editForm.role}
                      onChange={(e) => setEditForm({ ...editForm, role: e.target.value as EditRole })}
                      className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                    >
                      <option value="">Keep current</option>
                      <option value="Customer">Customer</option>
                      <option value="Chef">Chef</option>
                      <option value="Admin">Admin</option>
                    </select>
                    {userModal.user.roles.includes("Chef") && editForm.role && editForm.role !== "Chef" && (
                      <p className="mt-1 text-xs text-amber-700">
                        Demoting this chef also removes their kitchen and its content.
                      </p>
                    )}
                    {userModal.user.roles.includes("Chef") && !editForm.role && (
                      <p className="mt-1 text-xs text-gray-500">Leave unchanged to keep their role and kitchen.</p>
                    )}
                  </div>
                  <div className="mt-4 flex justify-end gap-3 border-t pt-4">
                    <button type="button" onClick={closeUserModal} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
                      Cancel
                    </button>
                    <button type="submit" disabled={modalSubmitting} className="rounded-lg bg-gray-900 px-5 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50">
                      {modalSubmitting ? "Saving…" : "Save changes"}
                    </button>
                  </div>
                </form>
              </>
            )}

            {userModal.mode === "password" && (
              <>
                <h2 className="text-lg font-bold text-gray-900">Set password</h2>
                <p className="mt-1 text-sm text-gray-600">
                  New password for <span className="font-medium">{userModal.user.email}</span>.
                </p>
                <form onSubmit={handleSetPassword} className="mt-4 space-y-3">
                  <div>
                    <label className="block text-xs font-semibold uppercase text-gray-700">New password *</label>
                    <input
                      type="text"
                      required
                      minLength={8}
                      value={passwordForm}
                      onChange={(e) => setPasswordForm(e.target.value)}
                      placeholder="8+ chars, upper/lower/digit"
                      className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none"
                    />
                  </div>
                  <div className="mt-4 flex justify-end gap-3 border-t pt-4">
                    <button type="button" onClick={closeUserModal} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
                      Cancel
                    </button>
                    <button type="submit" disabled={modalSubmitting} className="rounded-lg bg-gray-900 px-5 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50">
                      {modalSubmitting ? "Setting…" : "Set password"}
                    </button>
                  </div>
                </form>
              </>
            )}
          </div>
        </div>
      )}

      {/* Cuisine modal */}
      {cuisineModal.mode !== "closed" && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
            <h2 className="text-lg font-bold text-gray-900">
              {cuisineModal.mode === "create" ? "Add cuisine" : "Edit cuisine"}
            </h2>
            <form onSubmit={async (e) => {
              e.preventDefault();
              setCuisineSubmitting(true);
              setError(null);
              try {
                if (cuisineModal.mode === "create") {
                  const created = await createCuisine(cuisineForm);
                  setCuisines(prev => [...prev, created].sort((a, b) => a.displayOrder - b.displayOrder));
                  setSuccess(`"${cuisineForm.name}" was created.`);
                } else {
                  const updated = await updateCuisine(cuisineModal.cuisine.id, cuisineForm);
                  setCuisines(prev => prev.map(c => c.id === updated.id ? updated : c));
                  setSuccess(`"${cuisineForm.name}" was updated.`);
                }
                setCuisineModal({ mode: "closed" });
              } catch (err) {
                setError(err instanceof ApiError ? err.message : "Failed to save cuisine.");
              } finally {
                setCuisineSubmitting(false);
              }
            }} className="mt-4 space-y-3">
              <div>
                <label className="block text-xs font-semibold uppercase text-gray-700">Name *</label>
                <input type="text" required value={cuisineForm.name} onChange={(e) => setCuisineForm({ ...cuisineForm, name: e.target.value })}
                  className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none" />
              </div>
              <div>
                <label className="block text-xs font-semibold uppercase text-gray-700">Slug *</label>
                <input type="text" required value={cuisineForm.slug} onChange={(e) => setCuisineForm({ ...cuisineForm, slug: e.target.value })}
                  placeholder="e.g. pakistani"
                  className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none" />
              </div>
              <div>
                <label className="block text-xs font-semibold uppercase text-gray-700">Description</label>
                <textarea rows={2} value={cuisineForm.description} onChange={(e) => setCuisineForm({ ...cuisineForm, description: e.target.value })}
                  className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold uppercase text-gray-700">Display order</label>
                  <input type="number" value={cuisineForm.displayOrder} onChange={(e) => setCuisineForm({ ...cuisineForm, displayOrder: parseInt(e.target.value) || 0 })}
                    className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none" />
                </div>
                <div className="flex items-end">
                  <label className="flex items-center gap-2 text-sm text-gray-700">
                    <input type="checkbox" checked={cuisineForm.isActive} onChange={(e) => setCuisineForm({ ...cuisineForm, isActive: e.target.checked })}
                      className="rounded border-gray-300" />
                    Active
                  </label>
                </div>
              </div>
              <div className="mt-4 flex justify-end gap-3 border-t pt-4">
                <button type="button" onClick={() => setCuisineModal({ mode: "closed" })}
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
                <button type="submit" disabled={cuisineSubmitting}
                  className="rounded-lg bg-gray-900 px-5 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50">
                  {cuisineSubmitting ? "Saving…" : "Save"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
}
