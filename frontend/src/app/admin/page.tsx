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
  type AdminUser,
  type AdminReview,
} from "@/lib/admin";
import type { Report } from "@/lib/reports";
import { ApiError } from "@/lib/api";

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

  const [userModal, setUserModal] = useState<UserModalMode>({ mode: "closed" });
  const [createForm, setCreateForm] = useState(emptyCreateForm);
  const [editForm, setEditForm] = useState(emptyEditForm);
  const [passwordForm, setPasswordForm] = useState("");
  const [modalSubmitting, setModalSubmitting] = useState(false);

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

      {/* Users */}
      <div className="mt-10">
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

      {/* Reports */}
      <div className="mt-12">
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
                  <button
                    type="button"
                    onClick={() => handleDismissReport(report)}
                    disabled={busyId === report.id}
                    className="font-medium text-gray-600 hover:text-gray-900 underline text-xs disabled:opacity-50"
                  >
                    Dismiss
                  </button>
                  <button
                    type="button"
                    onClick={() => handleResolveReport(report)}
                    disabled={busyId === report.id}
                    className="font-medium text-emerald-700 hover:text-emerald-900 underline text-xs disabled:opacity-50"
                  >
                    Resolve
                  </button>
                </div>
              </li>
            ))}
          </ul>
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
    </section>
  );
}
