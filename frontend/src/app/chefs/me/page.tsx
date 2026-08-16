"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  createChefProfile,
  getMyChefProfile,
  updateChefProfile,
  type ChefProfile,
} from "@/lib/chefs";
import { ApiError } from "@/lib/api";

const emptyForm = { displayName: "", bio: "", city: "", area: "", cuisines: "" };

export default function ChefProfileMePage() {
  const router = useRouter();
  const [existing, setExisting] = useState<ChefProfile | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getMyChefProfile()
      .then((profile) => {
        if (cancelled) return;
        setExisting(profile);
        setForm({
          displayName: profile.displayName,
          bio: profile.bio,
          city: profile.city,
          area: profile.area ?? "",
          cuisines: profile.cuisines.join(", "),
        });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 401) {
          router.replace("/login");
          return;
        }
        if (err instanceof ApiError && err.status === 404) {
          return;
        }
        setError(err instanceof ApiError ? err.message : "Unable to load your profile.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [router]);

  function update<K extends keyof typeof form>(key: K, value: string) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(false);
    setSubmitting(true);

    const input = {
      displayName: form.displayName.trim(),
      bio: form.bio.trim(),
      city: form.city.trim(),
      area: form.area.trim() || null,
      cuisines: form.cuisines
        .split(",")
        .map((c) => c.trim())
        .filter(Boolean),
    };

    try {
      const saved = existing ? await updateChefProfile(input) : await createChefProfile(input);
      setExisting(saved);
      setSuccess(true);
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Unable to save your profile. Please try again."
      );
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return (
      <section className="mx-auto max-w-md px-4 py-16 text-gray-600">Loading your profile…</section>
    );
  }

  return (
    <section className="mx-auto max-w-md px-4 py-16">
      <h1 className="text-3xl font-bold tracking-tight">
        {existing ? "Edit your chef profile" : "Create your chef profile"}
      </h1>
      <p className="mt-2 text-gray-600">Tell hungry customers who you are and what you cook.</p>

      <form onSubmit={handleSubmit} className="mt-8 space-y-4">
        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}
        {success && (
          <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            Your profile was saved.
          </div>
        )}

        <div>
          <label htmlFor="displayName" className="block text-sm font-medium text-gray-700">
            Display name
          </label>
          <input
            id="displayName"
            type="text"
            required
            minLength={2}
            value={form.displayName}
            onChange={(event) => update("displayName", event.target.value)}
            className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        <div>
          <label htmlFor="bio" className="block text-sm font-medium text-gray-700">
            Bio
          </label>
          <textarea
            id="bio"
            required
            minLength={10}
            rows={4}
            value={form.bio}
            onChange={(event) => update("bio", event.target.value)}
            className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="city" className="block text-sm font-medium text-gray-700">
              City
            </label>
            <input
              id="city"
              type="text"
              required
              minLength={2}
              value={form.city}
              onChange={(event) => update("city", event.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
            />
          </div>
          <div>
            <label htmlFor="area" className="block text-sm font-medium text-gray-700">
              Area (optional)
            </label>
            <input
              id="area"
              type="text"
              value={form.area}
              onChange={(event) => update("area", event.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
            />
          </div>
        </div>

        <div>
          <label htmlFor="cuisines" className="block text-sm font-medium text-gray-700">
            Cuisines
          </label>
          <input
            id="cuisines"
            type="text"
            placeholder="Pakistani, Bakery, Desserts"
            value={form.cuisines}
            onChange={(event) => update("cuisines", event.target.value)}
            className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
          <p className="mt-1 text-xs text-gray-500">Comma-separated tags, up to 10.</p>
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="w-full rounded-lg bg-gray-900 px-4 py-2 font-medium text-white hover:bg-gray-800 disabled:opacity-50"
        >
          {submitting ? "Saving…" : existing ? "Save changes" : "Create profile"}
        </button>
      </form>

      <p className="mt-6 text-sm text-gray-600">
        <Link href="/chefs" className="font-medium text-gray-900 underline">
          View all chefs
        </Link>
      </p>
    </section>
  );
}
