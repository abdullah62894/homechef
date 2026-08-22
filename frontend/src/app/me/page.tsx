"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { fetchMe, logoutUser, type UserDto } from "@/lib/auth";
import {
  getUnreadCount,
  listInboxMessages,
  listSentMessages,
  markMessageRead,
  type ChefMessage,
} from "@/lib/messages";
import {
  listNotifications,
  markAllNotificationsRead,
  markNotificationRead,
  getUnreadNotificationCount,
  type AppNotification,
} from "@/lib/notifications";
import { ApiError } from "@/lib/api";

export default function MePage() {
  const router = useRouter();
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [inbox, setInbox] = useState<ChefMessage[]>([]);
  const [unread, setUnread] = useState(0);
  const [sent, setSent] = useState<ChefMessage[]>([]);
  const [markingReadId, setMarkingReadId] = useState<string | null>(null);
  const [notifications, setNotifications] = useState<AppNotification[]>([]);
  const [unreadNotifications, setUnreadNotifications] = useState(0);
  const [markingAllRead, setMarkingAllRead] = useState(false);

  useEffect(() => {
    let cancelled = false;
    fetchMe()
      .then((me) => {
        if (cancelled) return;
        setUser(me);

        const isChef = me.roles.includes("Chef");
        const loaders: Promise<void>[] = [
          listSentMessages(1, 50)
            .then((page) => {
              if (!cancelled) setSent(page.items);
            })
            .catch(() => {}),
          listNotifications(1, 20)
            .then((page) => {
              if (!cancelled) setNotifications(page.items);
            })
            .catch(() => {}),
          getUnreadNotificationCount()
            .then((count) => {
              if (!cancelled) setUnreadNotifications(count);
            })
            .catch(() => {}),
        ];

        if (isChef) {
          loaders.push(
            getUnreadCount()
              .then((count) => {
                if (!cancelled) setUnread(count);
              })
              .catch(() => {}),
            listInboxMessages(1, 50)
              .then((page) => {
                if (!cancelled) setInbox(page.items);
              })
              .catch(() => {})
          );
        }

        return Promise.all(loaders).then(() => undefined);
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

  async function handleMarkRead(message: ChefMessage) {
    setMarkingReadId(message.id);
    try {
      await markMessageRead(message.id);
      setInbox((prev) =>
        prev.map((m) =>
          m.id === message.id ? { ...m, readAtUtc: new Date().toISOString() } : m
        )
      );
      setUnread((prev) => Math.max(0, prev - 1));
    } catch {
      // Ignored — the message stays unread visually.
    } finally {
      setMarkingReadId(null);
    }
  }

  async function handleLogout() {
    await logoutUser();
    router.push("/");
  }

  async function handleMarkNotificationRead(notification: AppNotification) {
    try {
      await markNotificationRead(notification.id);
      setNotifications((prev) =>
        prev.map((n) =>
          n.id === notification.id ? { ...n, readAtUtc: new Date().toISOString() } : n
        )
      );
      setUnreadNotifications((prev) => Math.max(0, prev - 1));
    } catch {
      // Ignored — stays unread visually.
    }
  }

  async function handleMarkAllNotificationsRead() {
    setMarkingAllRead(true);
    try {
      await markAllNotificationsRead();
      setNotifications((prev) =>
        prev.map((n) => (n.readAtUtc ? n : { ...n, readAtUtc: new Date().toISOString() }))
      );
      setUnreadNotifications(0);
    } catch {
      // Ignored.
    } finally {
      setMarkingAllRead(false);
    }
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

      {user.roles.includes("Admin") && (
        <div className="mt-8 rounded-xl border border-gray-900/15 bg-gray-900 p-4 flex items-center justify-between">
          <div>
            <div className="text-sm font-semibold text-white">Admin console</div>
            <div className="text-xs text-gray-300">
              Moderate accounts, reviews and dishes.
            </div>
          </div>
          <Link
            href="/admin"
            className="rounded-lg bg-white px-3 py-1.5 text-xs font-medium text-gray-900 hover:bg-gray-100 transition"
          >
            Open console →
          </Link>
        </div>
      )}

      {user.roles.includes("Chef") && (
        <div className="mt-10">
          <div className="flex items-center gap-3">
            <h2 className="text-xl font-bold tracking-tight">Inbox</h2>
            {unread > 0 && (
              <span className="rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-700">
                {unread} unread
              </span>
            )}
          </div>

          {inbox.length === 0 ? (
            <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
              No customer messages yet.
            </p>
          ) : (
            <ul className="mt-4 space-y-3">
              {inbox.map((message) => (
                <li
                  key={message.id}
                  className={`rounded-xl border p-4 ${
                    message.readAtUtc ? "border-gray-200 bg-white" : "border-gray-900/20 bg-gray-50"
                  }`}
                >
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span className="text-sm font-semibold text-gray-900">
                      {message.senderName || "Customer"}
                    </span>
                    <span className="text-xs text-gray-400">
                      {new Date(message.createdAtUtc).toLocaleString()}
                    </span>
                  </div>
                  <p className="mt-2 text-sm text-gray-700 whitespace-pre-line">{message.body}</p>
                  <div className="mt-3 flex items-center justify-between">
                    {!message.readAtUtc ? (
                      <button
                        type="button"
                        onClick={() => handleMarkRead(message)}
                        disabled={markingReadId === message.id}
                        className="rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium hover:bg-gray-100 disabled:opacity-50"
                      >
                        {markingReadId === message.id ? "Marking…" : "Mark as read"}
                      </button>
                    ) : (
                      <span className="text-xs text-gray-400">Read</span>
                    )}
                    <Link
                      href={`/chefs/${message.chefProfileId}`}
                      className="text-xs text-gray-500 underline"
                    >
                      View your kitchen
                    </Link>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="mt-10">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex items-center gap-3">
            <h2 className="text-xl font-bold tracking-tight">Notifications</h2>
            {unreadNotifications > 0 && (
              <span className="rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-700">
                {unreadNotifications} new
              </span>
            )}
          </div>
          {notifications.length > 0 && (
            <button
              type="button"
              onClick={handleMarkAllNotificationsRead}
              disabled={markingAllRead || unreadNotifications === 0}
              className="text-xs font-medium text-gray-600 underline hover:text-gray-900 disabled:opacity-50"
            >
              {markingAllRead ? "Marking…" : "Mark all as read"}
            </button>
          )}
        </div>

        {notifications.length === 0 ? (
          <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
            No notifications yet. You&apos;ll be notified here when customers contact you or leave reviews.
          </p>
        ) : (
          <ul className="mt-4 space-y-3">
            {notifications.map((notification) => (
              <li
                key={notification.id}
                className={`rounded-xl border p-4 ${
                  notification.readAtUtc
                    ? "border-gray-200 bg-white"
                    : "border-gray-900/20 bg-gray-50"
                }`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="text-sm font-semibold text-gray-900">
                    {notification.type === "NewReview" ? "⭐ " : "✉️ "}
                    {notification.title}
                  </span>
                  <span className="text-xs text-gray-400">
                    {new Date(notification.createdAtUtc).toLocaleString()}
                  </span>
                </div>
                <p className="mt-1 text-sm text-gray-700">{notification.body}</p>
                <div className="mt-2 flex justify-end">
                  {!notification.readAtUtc && (
                    <button
                      type="button"
                      onClick={() => handleMarkNotificationRead(notification)}
                      className="rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium hover:bg-gray-100"
                    >
                      Mark as read
                    </button>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-10">
        <h2 className="text-xl font-bold tracking-tight">Messages you sent</h2>
        {sent.length === 0 ? (
          <p className="mt-4 rounded-xl border border-dashed border-gray-300 p-6 text-sm text-gray-500">
            You haven&apos;t contacted any chefs yet. Open a chef&apos;s page and use the contact form.
          </p>
        ) : (
          <ul className="mt-4 space-y-3">
            {sent.map((message) => (
              <li key={message.id} className="rounded-xl border border-gray-200 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <Link
                    href={`/chefs/${message.chefProfileId}`}
                    className="text-sm font-semibold text-gray-900 underline-offset-2 hover:underline"
                  >
                    {message.chefDisplayName}
                  </Link>
                  <span className="text-xs text-gray-400">
                    {new Date(message.createdAtUtc).toLocaleString()}
                  </span>
                </div>
                <p className="mt-2 text-sm text-gray-700 whitespace-pre-line">{message.body}</p>
                <span className="mt-2 inline-block text-xs text-gray-400">
                  {message.readAtUtc ? "Read by chef" : "Delivered"}
                </span>
              </li>
            ))}
          </ul>
        )}
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