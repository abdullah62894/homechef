import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  addChefFavorite,
  addFoodFavorite,
  getUserFavoriteIds,
  listFavoriteChefs,
  listFavoriteFoods,
  removeChefFavorite,
  removeFoodFavorite,
} from "@/lib/favorites";

const baseUrl = "http://localhost:5050";

const sampleChef = {
  id: "17994471-e812-4da7-ae46-441555e5f09a",
  displayName: "Amna's Kitchen",
  bio: "Home-cooked dishes.",
  city: "Karachi",
  area: "Clifton",
  address: null,
  latitude: null,
  longitude: null,
  distanceKm: null,
  cuisines: ["Pakistani"],
  photoUrl: null,
};

const sampleFood = {
  id: "44444444-4444-4444-4444-444444444444",
  chefProfileId: "17994471-e812-4da7-ae46-441555e5f09a",
  chefDisplayName: "Amna's Kitchen",
  chefCity: "Karachi",
  chefArea: "Clifton",
  chefAddress: null,
  distanceKm: null,
  categoryId: null,
  categoryName: null,
  name: "Chicken Biryani",
  description: "Special biryani",
  price: 650,
  currency: "PKR",
  isAvailable: true,
  imageUrl: null,
  preparationTimeMinutes: 45,
};

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("favorites lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("addChefFavorite sends POST request", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await addChefFavorite(sampleChef.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/favorites/chefs/${sampleChef.id}`);
    expect(init.method).toBe("POST");
  });

  it("removeChefFavorite sends DELETE request", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await removeChefFavorite(sampleChef.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/favorites/chefs/${sampleChef.id}`);
    expect(init.method).toBe("DELETE");
  });

  it("listFavoriteChefs fetches favorite chefs with pagination", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [sampleChef],
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const result = await listFavoriteChefs(1, 20);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/favorites/chefs?page=1&pageSize=20`);
    expect(result.items).toEqual([sampleChef]);
    expect(result.total).toBe(1);
  });

  it("addFoodFavorite sends POST request", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await addFoodFavorite(sampleFood.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/favorites/foods/${sampleFood.id}`);
    expect(init.method).toBe("POST");
  });

  it("removeFoodFavorite sends DELETE request", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await removeFoodFavorite(sampleFood.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/favorites/foods/${sampleFood.id}`);
    expect(init.method).toBe("DELETE");
  });

  it("listFavoriteFoods fetches favorite foods with pagination", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [sampleFood],
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const result = await listFavoriteFoods(1, 20);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/favorites/foods?page=1&pageSize=20`);
    expect(result.items).toEqual([sampleFood]);
    expect(result.total).toBe(1);
  });

  it("getUserFavoriteIds fetches user favorite IDs", async () => {
    const ids = {
      chefIds: [sampleChef.id],
      foodIds: [sampleFood.id],
    };
    fetchMock.mockResolvedValue(jsonResponse(200, { data: ids, meta: null }));

    const result = await getUserFavoriteIds();

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/favorites/ids`);
    expect(result).toEqual(ids);
  });
});
