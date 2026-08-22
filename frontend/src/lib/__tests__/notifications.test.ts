import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  getUnreadNotificationCount,
  listNotifications,
  markAllNotificationsRead,
  markNotificationRead,
} from "@/lib/notifications";

const baseUrl = "http://localhost:5050";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("notifications lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("lists notifications with unread state", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse({
        data: [
          { id: "n1", type: "NewMessage", title: "New message", readAtUtc: null },
        ],
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const page = await listNotifications();

    const [url] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/notifications?page=1&pageSize=20`);
    expect(page.items[0].type).toBe("NewMessage");
    expect(page.total).toBe(1);
  });

  it("fetches the unread count", async () => {
    fetchMock.mockResolvedValue(jsonResponse({ data: 3, meta: null }));

    await expect(getUnreadNotificationCount()).resolves.toBe(3);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/notifications/unread-count`);
  });

  it("marks one and then all notifications read", async () => {
    fetchMock
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(new Response(null, { status: 204 }));

    await markNotificationRead("n1");
    await markAllNotificationsRead();

    const calls = fetchMock.mock.calls;
    expect(calls[0][0]).toBe(`${baseUrl}/api/notifications/n1/read`);
    expect(calls[0][1].method).toBe("POST");
    expect(calls[1][0]).toBe(`${baseUrl}/api/notifications/read-all`);
    expect(calls[1][1].method).toBe("POST");
  });
});
