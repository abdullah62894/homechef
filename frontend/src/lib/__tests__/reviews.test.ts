import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  createChefReview,
  deleteReview,
  getChefRatingSummary,
  listChefReviews,
  updateReview,
} from "@/lib/reviews";

const baseUrl = "http://localhost:5050";

const sampleReview = {
  id: "68379294-8149-43c2-bf77-1f4806a6b579",
  chefProfileId: "17994471-e812-4da7-ae46-441555e5f09a",
  customerUserId: "cce11a51-de1d-4cf7-b4d2-f48ea6a957f2",
  customerName: "Sara Ali",
  rating: 5,
  comment: "Incredible biryani and quick service!",
  createdAtUtc: "2026-08-16T14:10:00Z",
  updatedAtUtc: "2026-08-16T14:10:00Z",
};

const sampleSummary = {
  chefProfileId: "17994471-e812-4da7-ae46-441555e5f09a",
  averageRating: 4.8,
  totalReviews: 12,
  ratingDistribution: { 1: 0, 2: 0, 3: 1, 4: 3, 5: 8 },
};

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("reviews lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("listChefReviews fetches reviews with pagination", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, {
        data: [sampleReview],
        meta: { page: 1, pageSize: 10, total: 1, hasMore: false },
      })
    );

    const result = await listChefReviews(sampleReview.chefProfileId, 1, 10);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/chefs/${sampleReview.chefProfileId}/reviews?page=1&pageSize=10`);
    expect(result.items).toEqual([sampleReview]);
    expect(result.total).toBe(1);
  });

  it("getChefRatingSummary fetches aggregated summary", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: sampleSummary, meta: null }));

    const result = await getChefRatingSummary(sampleReview.chefProfileId);

    const calledUrl = fetchMock.mock.calls[0][0] as string;
    expect(calledUrl).toBe(`${baseUrl}/api/chefs/${sampleReview.chefProfileId}/reviews/summary`);
    expect(result.averageRating).toBe(4.8);
    expect(result.totalReviews).toBe(12);
  });

  it("createChefReview sends POST with review data", async () => {
    fetchMock.mockResolvedValue(jsonResponse(201, { data: sampleReview, meta: null }));

    const result = await createChefReview(sampleReview.chefProfileId, {
      rating: 5,
      comment: "Incredible biryani and quick service!",
    });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/${sampleReview.chefProfileId}/reviews`);
    expect(init.method).toBe("POST");
    expect(result).toEqual(sampleReview);
  });

  it("updateReview sends PUT with updated data", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: sampleReview, meta: null }));

    const result = await updateReview(sampleReview.id, {
      rating: 4,
      comment: "Updated feedback",
    });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/reviews/${sampleReview.id}`);
    expect(init.method).toBe("PUT");
    expect(result).toEqual(sampleReview);
  });

  it("deleteReview sends DELETE request", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await deleteReview(sampleReview.id);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/reviews/${sampleReview.id}`);
    expect(init.method).toBe("DELETE");
  });
});
