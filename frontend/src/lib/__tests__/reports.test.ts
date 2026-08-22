import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { createReport } from "@/lib/reports";

const baseUrl = "http://localhost:5050";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("reports lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("creates a report with typed target and reason", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse({
        data: { id: "r1", targetType: "FoodItem", status: "Open" },
        meta: null,
      })
    );

    const report = await createReport({
      targetType: "FoodItem",
      targetId: "44444444-4444-4444-4444-444444444444",
      reason: "Spam",
      details: "Posted repeatedly.",
    });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/reports`);
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body)).toEqual({
      targetType: "FoodItem",
      targetId: "44444444-4444-4444-4444-444444444444",
      reason: "Spam",
      details: "Posted repeatedly.",
    });
    expect(report.status).toBe("Open");
  });

  it("omits empty details from the payload", async () => {
    fetchMock.mockResolvedValue(jsonResponse({ data: { id: "r2" }, meta: null }));

    await createReport({
      targetType: "Review",
      targetId: "55555555-5555-5555-5555-555555555555",
      reason: "Other",
    });

    const body = JSON.parse(fetchMock.mock.calls[0][1].body);
    expect(body.details).toBeUndefined();
  });
});
