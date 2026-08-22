import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  deleteAdminChef,
  deleteAdminFood,
  deleteAdminReview,
  listAdminReviews,
  listAdminUsers,
  restoreUser,
  suspendUser,
} from "@/lib/admin";

const baseUrl = "http://localhost:5050";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("admin lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("lists users with search and role filters", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse({
        data: [{ id: "u1", email: "a@b.c", roles: ["Chef"], isSuspended: false }],
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const page = await listAdminUsers("amna", "Chef");

    const [url] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/admin/users?page=1&pageSize=20&search=amna&role=Chef`);
    expect(page.total).toBe(1);
    expect(page.items[0].email).toBe("a@b.c");
  });

  it("skips empty search and role params", async () => {
    fetchMock.mockResolvedValue(jsonResponse({ data: [], meta: null }));

    await listAdminUsers("   ", "");

    const [url] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/admin/users?page=1&pageSize=20`);
  });

  it("lists reviews newest first", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse({
        data: [{ id: "r1", rating: 3, comment: "ok" }],
        meta: { page: 2, pageSize: 20, total: 25, hasMore: false },
      })
    );

    const page = await listAdminReviews(2);

    const [url] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/admin/reviews?page=2&pageSize=20`);
    expect(page.page).toBe(2);
    expect(page.hasMore).toBe(false);
  });

  it("suspends and restores users via POST", async () => {
    fetchMock.mockImplementation(() =>
      Promise.resolve(jsonResponse({ data: { id: "u1", isSuspended: true }, meta: null }))
    );

    await suspendUser("u1");
    expect(fetchMock.mock.calls[0][1].method).toBe("POST");
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/admin/users/u1/suspend`);

    await restoreUser("u1");
    expect(fetchMock.mock.calls[1][0]).toBe(`${baseUrl}/api/admin/users/u1/restore`);
  });

  it("deletes reviews, foods and chefs via DELETE", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 204 }));

    await deleteAdminReview("r1");
    await deleteAdminFood("f1");
    await deleteAdminChef("c1");

    const calls = fetchMock.mock.calls;
    expect(calls[0][0]).toBe(`${baseUrl}/api/admin/reviews/r1`);
    expect(calls[1][0]).toBe(`${baseUrl}/api/admin/foods/f1`);
    expect(calls[2][0]).toBe(`${baseUrl}/api/admin/chefs/c1`);
    expect(calls.every(([, init]) => init.method === "DELETE")).toBe(true);
  });
});
