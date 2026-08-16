import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  createChefProfile,
  getChef,
  getMyChefProfile,
  listChefs,
  updateChefProfile,
} from "@/lib/chefs";

const baseUrl = "http://localhost:5050";

const profile = {
  id: "17994471-e812-4da7-ae46-441555e5f09a",
  userId: "cce11a51-de1d-4cf7-b4d2-f48ea6a957f2",
  displayName: "Amna's Kitchen",
  bio: "Home-cooked Pakistani and continental dishes.",
  city: "Karachi",
  area: "Clifton",
  cuisines: ["Bakery", "Pakistani"],
  photoUrl: null,
  createdAtUtc: "2026-08-16T13:03:25Z",
  updatedAtUtc: "2026-08-16T13:03:25Z",
};

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("chefs lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("listChefs fetches the paginated list and normalizes meta", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [profile],
        meta: { page: 2, pageSize: 12, total: 13, hasMore: true },
      })
    );

    const result = await listChefs({}, 2, 12);

    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/chefs?page=2&pageSize=12`);
    expect(result.items).toEqual([profile]);
    expect(result).toMatchObject({ page: 2, pageSize: 12, total: 13, hasMore: true });
  });

  it("listChefs falls back gracefully when meta is null", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: [profile], meta: null }));

    const result = await listChefs({}, 1, 20);

    expect(result).toMatchObject({ page: 1, pageSize: 20, total: 1, hasMore: false });
  });

  it("getChef fetches a single profile", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: profile, meta: null }));

    const result = await getChef(profile.id);

    expect(result).toEqual(profile);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/chefs/${profile.id}`);
  });

  it("getMyChefProfile fetches /api/chefs/me", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: profile, meta: null }));

    const result = await getMyChefProfile();

    expect(result).toEqual(profile);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/chefs/me`);
  });

  it("createChefProfile POSTs to /api/chefs/me", async () => {
    fetchMock.mockResolvedValue(jsonResponse(201, { data: profile, meta: null }));

    const result = await createChefProfile({
      displayName: profile.displayName,
      bio: profile.bio,
      city: profile.city,
      cuisines: profile.cuisines,
    });

    expect(result).toEqual(profile);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me`);
    expect(init.method).toBe("POST");
    expect(init.credentials).toBe("include");
  });

  it("updateChefProfile PUTs to /api/chefs/me", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: profile, meta: null }));

    const result = await updateChefProfile({
      displayName: profile.displayName,
      bio: profile.bio,
      city: profile.city,
    });

    expect(result).toEqual(profile);
    expect(fetchMock.mock.calls[0][1].method).toBe("PUT");
  });
});
