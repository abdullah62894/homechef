import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  createFoodItem,
  deleteFoodItem,
  getFood,
  listChefFoods,
  listFoodCategories,
  listFoods,
  listMyFoods,
  toggleFoodAvailability,
  updateFoodItem,
} from "@/lib/foods";

const baseUrl = "http://localhost:5050";

const mockFood = {
  id: "44444444-4444-4444-4444-444444444444",
  chefProfileId: "17994471-e812-4da7-ae46-441555e5f09a",
  chefDisplayName: "Amna's Kitchen",
  chefCity: "Karachi",
  chefArea: "Clifton",
  categoryId: "11111111-1111-1111-1111-111111111102",
  categoryName: "Rice & Biryani",
  name: "Special Chicken Biryani",
  description: "Traditional Sindhi biryani made with fragrant basmati rice.",
  price: 650,
  currency: "PKR",
  isAvailable: true,
  imageUrl: null,
  preparationTimeMinutes: 45,
  createdAtUtc: "2026-08-16T14:00:00Z",
  updatedAtUtc: "2026-08-16T14:00:00Z",
};

const mockCategory = {
  id: "11111111-1111-1111-1111-111111111102",
  name: "Rice & Biryani",
  slug: "rice-biryani",
  description: "Spiced rice dishes",
  displayOrder: 2,
};

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("foods lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("listFoods fetches paginated foods with filters", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [mockFood],
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const result = await listFoods({ search: "Biryani", categoryId: mockCategory.id }, 1, 20);

    expect(fetchMock.mock.calls[0][0]).toContain("/api/foods?");
    expect(fetchMock.mock.calls[0][0]).toContain("search=Biryani");
    expect(fetchMock.mock.calls[0][0]).toContain(`categoryId=${mockCategory.id}`);
    expect(result.items).toEqual([mockFood]);
  });

  it("getFood fetches a single food item", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: mockFood, meta: null }));

    const result = await getFood(mockFood.id);

    expect(result).toEqual(mockFood);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/foods/${mockFood.id}`);
  });

  it("listFoodCategories fetches all categories", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: [mockCategory], meta: null }));

    const result = await listFoodCategories();

    expect(result).toEqual([mockCategory]);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/foods/categories`);
  });

  it("listChefFoods fetches menu items for a specific chef", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [mockFood],
        meta: { page: 1, pageSize: 50, total: 1, hasMore: false },
      })
    );

    const result = await listChefFoods(mockFood.chefProfileId, true);

    expect(fetchMock.mock.calls[0][0]).toBe(
      `${baseUrl}/api/chefs/${mockFood.chefProfileId}/foods?page=1&pageSize=50&isAvailable=true`
    );
    expect(result.items).toEqual([mockFood]);
  });

  it("listMyFoods fetches current chef foods", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [mockFood],
        meta: { page: 1, pageSize: 50, total: 1, hasMore: false },
      })
    );

    const result = await listMyFoods();

    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/chefs/me/foods?page=1&pageSize=50`);
    expect(result.items).toEqual([mockFood]);
  });

  it("createFoodItem POSTs to /api/chefs/me/foods", async () => {
    fetchMock.mockResolvedValue(jsonResponse(201, { data: mockFood, meta: null }));

    const result = await createFoodItem({
      name: mockFood.name,
      description: mockFood.description,
      price: mockFood.price,
      categoryId: mockFood.categoryId,
    });

    expect(result).toEqual(mockFood);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/foods`);
    expect(init.method).toBe("POST");
  });

  it("updateFoodItem PUTs to /api/chefs/me/foods/:id", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: mockFood, meta: null }));

    const result = await updateFoodItem(mockFood.id, {
      name: mockFood.name,
      description: mockFood.description,
      price: 700,
    });

    expect(result).toEqual(mockFood);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/foods/${mockFood.id}`);
    expect(init.method).toBe("PUT");
  });

  it("deleteFoodItem DELETEs /api/chefs/me/foods/:id", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await deleteFoodItem(mockFood.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/foods/${mockFood.id}`);
    expect(init.method).toBe("DELETE");
  });

  it("toggleFoodAvailability PATCHes /api/chefs/me/foods/:id/availability", async () => {
    const updated = { ...mockFood, isAvailable: false };
    fetchMock.mockResolvedValue(jsonResponse(200, { data: updated, meta: null }));

    const result = await toggleFoodAvailability(mockFood.id, false);

    expect(result.isAvailable).toBe(false);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/foods/${mockFood.id}/availability`);
    expect(init.method).toBe("PATCH");
  });
});
