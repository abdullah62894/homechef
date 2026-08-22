import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  clearChefPhoto,
  clearFoodImage,
  resolveImageUrl,
  uploadChefPhoto,
  uploadFoodImage,
  validateImageFile,
} from "@/lib/images";

const baseUrl = "http://localhost:5050";

describe("validateImageFile", () => {
  it("accepts JPEG, PNG and WebP under the size limit", () => {
    const file = new File(["x"], "photo.jpg", { type: "image/jpeg" });
    expect(validateImageFile(file)).toBeNull();
  });

  it("rejects unsupported content types", () => {
    const file = new File(["x"], "notes.txt", { type: "text/plain" });
    expect(validateImageFile(file)).toContain("JPEG, PNG or WebP");
  });

  it("rejects files above 5 MB", () => {
    const file = new File(["x"], "big.png", { type: "image/png" });
    Object.defineProperty(file, "size", { value: 6 * 1024 * 1024 });
    expect(validateImageFile(file)).toContain("5 MB");
  });
});

describe("resolveImageUrl", () => {
  it("prefixes relative upload paths with the API base URL", () => {
    expect(resolveImageUrl("/uploads/2026/08/abc.webp")).toBe(
      `${baseUrl}/uploads/2026/08/abc.webp`
    );
  });

  it("returns absolute URLs unchanged", () => {
    expect(resolveImageUrl("https://cdn.example.com/x.webp")).toBe("https://cdn.example.com/x.webp");
  });

  it("returns null for missing paths", () => {
    expect(resolveImageUrl(null)).toBeNull();
    expect(resolveImageUrl(undefined)).toBeNull();
  });
});

describe("image uploads", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("uploads the chef photo as multipart with the 'file' field", async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify({ data: { photoUrl: "/uploads/x.webp" }, meta: null }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      })
    );

    await uploadChefPhoto(new File(["x"], "p.png", { type: "image/png" }));

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/photo`);
    expect(init.method).toBe("POST");
    expect(init.credentials).toBe("include");
    expect(init.body).toBeInstanceOf(FormData);
    expect((init.body as FormData).get("file")).toBeInstanceOf(File);
    // The multipart boundary is set by the browser — no manual Content-Type.
    expect(init.headers).not.toHaveProperty("Content-Type");
  });

  it("uploads a food image against the food id", async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify({ data: { imageUrl: "/uploads/y.webp" }, meta: null }), {
        status: 200,
      })
    );

    const foodId = "44444444-4444-4444-4444-444444444444";
    await uploadFoodImage(foodId, new File(["x"], "f.jpg", { type: "image/jpeg" }));

    const [url] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/foods/${foodId}/image`);
  });

  it("clears the chef photo with DELETE", async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify({ data: { photoUrl: null }, meta: null }), { status: 200 })
    );

    await clearChefPhoto();

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/photo`);
    expect(init.method).toBe("DELETE");
  });

  it("clears a food image with DELETE", async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify({ data: { imageUrl: null }, meta: null }), { status: 200 })
    );

    const foodId = "44444444-4444-4444-4444-444444444444";
    await clearFoodImage(foodId);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/chefs/me/foods/${foodId}/image`);
    expect(init.method).toBe("DELETE");
  });

  it("surfaces API errors from failed uploads", async () => {
    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({
          error: { code: "IMAGE_TOO_LARGE", message: "The uploaded image exceeds the maximum size of 5 MB." },
        }),
        { status: 400 }
      )
    );

    await expect(
      uploadChefPhoto(new File(["x"], "big.png", { type: "image/png" }))
    ).rejects.toThrow("exceeds the maximum size");
  });
});
