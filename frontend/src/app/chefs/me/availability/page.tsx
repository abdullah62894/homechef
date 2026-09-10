"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { getMyAvailability, updateMyAvailability, getDayName, type ChefAvailability, type AvailabilityWindowInput } from "@/lib/availability";
import { ApiError } from "@/lib/api";

const EMPTY_WINDOWS: AvailabilityWindowInput[] = [0, 1, 2, 3, 4, 5, 6].map((day) => ({
  dayOfWeek: day,
  openTime: "09:00",
  closeTime: "17:00",
  label: null,
  isActive: false,
}));

export default function ChefAvailabilityPage() {
  const router = useRouter();
  const [windows, setWindows] = useState<AvailabilityWindowInput[]>(EMPTY_WINDOWS);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getMyAvailability()
      .then((existing) => {
        if (cancelled || existing.length === 0) return;
        const merged = EMPTY_WINDOWS.map((def) => {
          const match = existing.find((e) => e.dayOfWeek === def.dayOfWeek);
          return match
            ? { dayOfWeek: def.dayOfWeek, openTime: match.openTime, closeTime: match.closeTime, label: match.label, isActive: match.isActive }
            : def;
        });
        setWindows(merged);
      })
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          router.replace("/login");
        }
      })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [router]);

  function updateWindow(day: number, field: keyof AvailabilityWindowInput, value: string | boolean | null) {
    setWindows((prev) => prev.map((w) => (w.dayOfWeek === day ? { ...w, [field]: value } : w)));
  }

  async function handleSave() {
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await updateMyAvailability(windows);
      setSuccess(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save availability.");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <section className="mx-auto max-w-3xl px-4 py-16 text-gray-600">Loading availability…</section>;
  }

  return (
    <section className="mx-auto max-w-3xl px-4 py-8">
      <Link href="/chefs/me" className="text-sm text-orange-500 hover:text-orange-600">&larr; Back to dashboard</Link>
      <h1 className="mt-4 text-3xl font-bold tracking-tight text-gray-900">Weekly Availability</h1>
      <p className="mt-2 text-sm text-gray-600">Set your working hours for each day of the week. Customers can only order when you are available.</p>

      {error && <div className="mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</div>}
      {success && <div className="mt-4 rounded-lg border border-green-200 bg-green-50 p-3 text-sm text-green-700">Availability saved.</div>}

      <div className="mt-6 space-y-3">
        {windows.map((w) => (
          <div key={w.dayOfWeek} className={`flex items-center gap-3 rounded-xl border p-4 transition ${w.isActive ? "border-green-200 bg-green-50/50" : "border-gray-200 bg-white"}`}>
            <label className="flex items-center gap-2 min-w-[140px]">
              <input
                type="checkbox"
                checked={w.isActive}
                onChange={(e) => updateWindow(w.dayOfWeek, "isActive", e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-green-600 focus:ring-green-500"
              />
              <span className={`text-sm font-semibold ${w.isActive ? "text-green-700" : "text-gray-500"}`}>{getDayName(w.dayOfWeek)}</span>
            </label>
            {w.isActive ? (
              <div className="flex items-center gap-2">
                <input
                  type="time"
                  value={w.openTime}
                  onChange={(e) => updateWindow(w.dayOfWeek, "openTime", e.target.value)}
                  className="rounded-lg border border-gray-300 px-2 py-1.5 text-sm"
                />
                <span className="text-sm text-gray-500">to</span>
                <input
                  type="time"
                  value={w.closeTime}
                  onChange={(e) => updateWindow(w.dayOfWeek, "closeTime", e.target.value)}
                  className="rounded-lg border border-gray-300 px-2 py-1.5 text-sm"
                />
                <input
                  type="text"
                  value={w.label ?? ""}
                  onChange={(e) => updateWindow(w.dayOfWeek, "label", e.target.value || null)}
                  placeholder="Label (optional)"
                  className="ml-2 w-32 rounded-lg border border-gray-300 px-2 py-1.5 text-sm"
                />
              </div>
            ) : (
              <span className="text-sm text-gray-400 italic">Closed</span>
            )}
          </div>
        ))}
      </div>

      <button
        onClick={handleSave}
        disabled={saving}
        className="mt-6 w-full rounded-lg bg-orange-500 px-4 py-2.5 text-sm font-semibold text-white hover:bg-orange-600 disabled:opacity-50"
      >
        {saving ? "Saving…" : "Save Availability"}
      </button>
    </section>
  );
}
