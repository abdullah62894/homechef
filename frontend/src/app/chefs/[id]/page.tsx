"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { getChef, type ChefProfile } from "@/lib/chefs";
import { ApiError } from "@/lib/api";

type LoadState =
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "ready"; chef: ChefProfile };

export default function ChefDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [state, setState] = useState<LoadState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;
    getChef(id)
      .then((chef) => {
        if (!cancelled) setState({ status: "ready", chef });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setState({
          status: "error",
          message: err instanceof ApiError ? err.message : "Unable to load this chef.",
        });
      });
    return () => {
      cancelled = true;
    };
  }, [id]);

  if (state.status === "loading") {
    return <section className="mx-auto max-w-5xl px-4 py-16 text-gray-600">Loading chef…</section>;
  }

  if (state.status === "error") {
    return (
      <section className="mx-auto max-w-5xl px-4 py-16">
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {state.message}
        </div>
        <Link href="/chefs" className="mt-6 inline-block text-sm text-gray-600 underline">
          Back to chefs
        </Link>
      </section>
    );
  }

  const chef = state.chef;

  return (
    <section className="mx-auto max-w-5xl px-4 py-16">
      <Link href="/chefs" className="text-sm text-gray-600 underline">
        Back to chefs
      </Link>

      <div className="mt-4 rounded-xl border border-gray-200 p-8">
        <h1 className="text-3xl font-bold tracking-tight">{chef.displayName}</h1>
        <p className="mt-2 text-gray-600">
          {chef.city}
          {chef.area ? `, ${chef.area}` : ""}
        </p>

        {chef.cuisines.length > 0 && (
          <div className="mt-4 flex flex-wrap gap-1.5">
            {chef.cuisines.map((cuisine) => (
              <span
                key={cuisine}
                className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs text-gray-700"
              >
                {cuisine}
              </span>
            ))}
          </div>
        )}

        <p className="mt-6 text-gray-700">{chef.bio}</p>
      </div>

      <p className="mt-8 text-sm text-gray-400">Menus and ordering are coming soon.</p>
    </section>
  );
}
