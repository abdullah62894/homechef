"use client";

import { useState } from "react";
import { createReport, type ReportReason, type ReportTargetType } from "@/lib/reports";
import { ApiError } from "@/lib/api";

const REASONS: { value: ReportReason; label: string }[] = [
  { value: "Spam", label: "Spam" },
  { value: "AbusiveContent", label: "Abusive content" },
  { value: "InappropriateImage", label: "Inappropriate image" },
  { value: "Misleading", label: "Misleading" },
  { value: "Other", label: "Other" },
];

interface ReportButtonProps {
  targetType: ReportTargetType;
  targetId: string;
  /** e.g. "kitchen", "dish" — used in confirmation copy. */
  targetName: string;
  className?: string;
}

/**
 * Inline "report content" control (Stage 10). Expands to a small form with a
 * reason and optional details, then submits to /api/reports.
 */
export default function ReportButton({ targetType, targetId, targetName, className }: ReportButtonProps) {
  const [open, setOpen] = useState(false);
  const [reason, setReason] = useState<ReportReason>("Spam");
  const [details, setDetails] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);

  async function handleSubmit() {
    setSubmitting(true);
    setError(null);
    try {
      await createReport({
        targetType,
        targetId,
        reason,
        details: details.trim() || undefined,
      });
      setDone(true);
    } catch (err) {
      if (err instanceof ApiError && err.code === "REPORT_DUPLICATE") {
        setDone(true);
        return;
      }
      setError(err instanceof ApiError ? err.message : "Unable to submit the report.");
    } finally {
      setSubmitting(false);
    }
  }

  if (done) {
    return <span className="text-xs text-gray-400">Reported — our moderators will take a look.</span>;
  }

  return (
    <div className={className}>
      {open ? (
        <div className="rounded-lg border border-gray-200 bg-gray-50 p-3 text-left">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-gray-700">
              Report this {targetName}
            </span>
            <button
              type="button"
              onClick={() => setOpen(false)}
              className="text-xs text-gray-400 hover:text-gray-600"
            >
              ✕
            </button>
          </div>
          <select
            value={reason}
            onChange={(e) => setReason(e.target.value as ReportReason)}
            className="mt-2 w-full rounded-lg border border-gray-300 px-2 py-1.5 text-xs focus:border-gray-900 focus:outline-none"
          >
            {REASONS.map((r) => (
              <option key={r.value} value={r.value}>
                {r.label}
              </option>
            ))}
          </select>
          <textarea
            value={details}
            onChange={(e) => setDetails(e.target.value)}
            rows={2}
            maxLength={1000}
            placeholder="Anything else the moderators should know? (optional)"
            className="mt-2 w-full rounded-lg border border-gray-300 px-2 py-1.5 text-xs focus:border-gray-900 focus:outline-none"
          />
          {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
          <div className="mt-2 flex justify-end gap-2">
            <button
              type="button"
              onClick={() => setOpen(false)}
              className="rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium text-gray-600 hover:bg-gray-100"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSubmit}
              disabled={submitting}
              className="rounded-lg bg-gray-900 px-3 py-1 text-xs font-medium text-white hover:bg-gray-800 disabled:opacity-50"
            >
              {submitting ? "Sending…" : "Submit report"}
            </button>
          </div>
        </div>
      ) : (
        <button
          type="button"
          onClick={() => setOpen(true)}
          className="text-xs font-medium text-gray-400 hover:text-red-600 underline"
        >
          ⚑ Report
        </button>
      )}
    </div>
  );
}
