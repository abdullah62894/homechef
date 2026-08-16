import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { search, getLocations, getCityLocation, listChefsInArea } from "@/lib/search";

const baseUrl = "http://localhost:5050";

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("search lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("search sends query params correctly", async () => {
    const searchResult = {
      chefs: [],
      foods: [],
      totalChefs: 0,
      totalFoods: 0,
      page: 1,
      pageSize: 20,
    };
    fetchMock.mockResolvedValue(jsonResponse(200, { data: searchResult, meta: null }));

    const result = await search({ q: "biryani", city: "Karachi" });

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toContain("/api/search?");
    expect(calledUrl).toContain("q=biryani");
    expect(calledUrl).toContain("city=Karachi");
    expect(result).toEqual(searchResult);
  });

  it("search defaults to page 1 and pageSize 20", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, { data: { chefs: [], foods: [], totalChefs: 0, totalFoods: 0, page: 1, pageSize: 20 }, meta: null })
    );

    await search();

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toContain("page=1");
    expect(calledUrl).toContain("pageSize=20");
  });

  it("search passes proximity parameters", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, { data: { chefs: [], foods: [], totalChefs: 0, totalFoods: 0, page: 1, pageSize: 10 }, meta: null })
    );

    await search({ lat: 24.86, lng: 67.01, radiusKm: 5 }, 1, 10);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toContain("lat=24.86");
    expect(calledUrl).toContain("lng=67.01");
    expect(calledUrl).toContain("radiusKm=5");
  });

  it("getLocations fetches location directory", async () => {
    const directory = {
      cities: [
        { city: "Karachi", totalChefs: 5, areas: [{ name: "Clifton", chefCount: 3 }] },
      ],
    };
    fetchMock.mockResolvedValue(jsonResponse(200, { data: directory, meta: null }));

    const result = await getLocations();

    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/locations`);
    expect(result.cities).toHaveLength(1);
    expect(result.cities[0].city).toBe("Karachi");
  });

  it("getCityLocation fetches city summary", async () => {
    const citySummary = {
      city: "Lahore",
      totalChefs: 2,
      areas: [{ name: "Gulberg", chefCount: 2 }],
    };
    fetchMock.mockResolvedValue(jsonResponse(200, { data: citySummary, meta: null }));

    const result = await getCityLocation("Lahore");

    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/locations/Lahore`);
    expect(result.city).toBe("Lahore");
  });

  it("listChefsInArea fetches chefs in a city/area", async () => {
    const chefs = [
      {
        id: "abc",
        displayName: "Chef A",
        bio: "Bio",
        city: "Karachi",
        area: "Clifton",
        address: null,
        latitude: null,
        longitude: null,
        distanceKm: null,
        cuisines: ["Pakistani"],
        photoUrl: null,
      },
    ];
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: chefs,
        meta: { page: 1, pageSize: 20, total: 1, hasMore: false },
      })
    );

    const result = await listChefsInArea("Karachi", "Clifton");

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toContain("/api/locations/Karachi/Clifton");
    expect(result.items).toEqual(chefs);
    expect(result.total).toBe(1);
  });
});
