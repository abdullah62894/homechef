import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { fetchMe, loginUser, logoutUser, registerUser } from "@/lib/auth";

const baseUrl = "http://localhost:5050";
const user = {
  id: "00000000-0000-0000-0000-000000000001",
  email: "test@test.com",
  firstName: "Test",
  lastName: "User",
  roles: ["Customer"],
  createdAtUtc: "2026-08-16T00:00:00Z",
};

function jsonResponse(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("auth lib", () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("registerUser posts to /api/auth/register and unwraps the envelope", async () => {
    fetchMock.mockResolvedValue(jsonResponse(201, { data: user, meta: null }));

    const result = await registerUser({
      firstName: "Test",
      lastName: "User",
      email: user.email,
      password: "Password123",
      role: "Customer",
    });

    expect(result).toEqual(user);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/auth/register`);
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body)).toMatchObject({
      email: user.email,
      role: "Customer",
    });
    expect(init.credentials).toBe("include");
  });

  it("loginUser posts to /api/auth/login", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: user, meta: null }));

    const result = await loginUser({ email: user.email, password: "Password123" });

    expect(result).toEqual(user);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/auth/login`);
  });

  it("fetchMe returns the current user", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { data: user, meta: null }));

    const result = await fetchMe();

    expect(result).toEqual(user);
    expect(fetchMock.mock.calls[0][0]).toBe(`${baseUrl}/api/users/me`);
  });

  it("logoutUser posts to /api/auth/logout", async () => {
    fetchMock.mockResolvedValue(jsonResponse(204));

    await logoutUser();

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(`${baseUrl}/api/auth/logout`);
    expect(init.method).toBe("POST");
  });

  it("logoutUser swallows network errors", async () => {
    fetchMock.mockRejectedValue(new Error("network down"));

    await expect(logoutUser()).resolves.toBeUndefined();
  });

  it("propagates ApiError with code and message on error responses", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(409, { error: { code: "EMAIL_TAKEN", message: "An account with this email already exists." } }),
    );

    const error = await registerUser({
      firstName: "Test",
      lastName: "User",
      email: user.email,
      password: "Password123",
    }).catch((err: unknown) => err);

    expect(error).toMatchObject({
      status: 409,
      code: "EMAIL_TAKEN",
      message: "An account with this email already exists.",
    });
  });
});