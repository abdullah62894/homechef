import { apiFetch, type ApiEnvelope } from "./api";

/** Authenticated user returned by the HomeChef API. */
export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  createdAtUtc: string;
}

export type SelfServiceRole = "Customer" | "Chef";

export interface RegisterInput {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role?: SelfServiceRole;
}

export interface LoginInput {
  email: string;
  password: string;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function registerUser(input: RegisterInput): Promise<UserDto> {
  return apiFetch<ApiEnvelope<UserDto>>("/api/auth/register", {
    method: "POST",
    body: input,
  }).then(unwrap);
}

export function loginUser(input: LoginInput): Promise<UserDto> {
  return apiFetch<ApiEnvelope<UserDto>>("/api/auth/login", {
    method: "POST",
    body: input,
  }).then(unwrap);
}

/** Logs the current session out. Never rejects on network errors. */
export async function logoutUser(): Promise<void> {
  try {
    await apiFetch<void>("/api/auth/logout", { method: "POST" });
  } catch {
    // Best effort: the server clears the cookie either way.
  }
}

export function fetchMe(): Promise<UserDto> {
  return apiFetch<ApiEnvelope<UserDto>>("/api/users/me").then(unwrap);
}