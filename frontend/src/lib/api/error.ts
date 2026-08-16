export interface ApiErrorBody {
  error?: {
    code?: string;
    message?: string;
  };
}

/** Thrown for any non-2xx API response. */
export class ApiError extends Error {
  readonly status: number;
  readonly code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
  }
}

/**
 * Wraps the backend's consistent error envelope
 * { "error": { "code": "...", "message": "..." } }.
 */
export async function toApiError(response: Response): Promise<ApiError> {
  let code = "REQUEST_FAILED";
  let message = `Request failed with status ${response.status}.`;

  try {
    const body = (await response.json()) as ApiErrorBody;
    if (body.error?.message) message = body.error.message;
    if (body.error?.code) code = body.error.code;
  } catch {
    // Non-JSON error body; fall back to defaults.
  }

  return new ApiError(response.status, code, message);
}