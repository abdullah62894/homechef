import { apiFetch, type ApiEnvelope } from "./api";

export type ReportTargetType = "ChefProfile" | "FoodItem" | "Review";

export type ReportReason =
  | "Spam"
  | "AbusiveContent"
  | "InappropriateImage"
  | "Misleading"
  | "Other";

export interface Report {
  id: string;
  reporterUserId: string;
  reporterName: string;
  targetType: ReportTargetType;
  targetId: string;
  targetLabel: string;
  reason: ReportReason;
  details: string;
  status: "Open" | "Resolved" | "Dismissed";
  createdAtUtc: string;
  resolvedAtUtc: string | null;
}

export interface ReportInput {
  targetType: ReportTargetType;
  targetId: string;
  reason: ReportReason;
  details?: string;
}

export function createReport(input: ReportInput): Promise<Report> {
  return apiFetch<ApiEnvelope<Report>>("/api/reports", {
    method: "POST",
    body: input,
  }).then((envelope) => envelope.data);
}
