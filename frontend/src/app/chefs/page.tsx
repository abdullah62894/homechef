"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { listChefs, type ChefListItem } from "@/lib/chefs";
import { ApiError } from "@/lib/api";

const PAGE_SIZE = 12;

type LoadState =
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "ready"; items: ChefListItem[]; hasMore: boolean };

export default function ChefsPage() {
  const [page, setPage] = useState(1);
  const [state, setState] = useState<LoadState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;
    listChefs(page, PAGE_SIZE)
      .then((result) => {
        if (!cancelled) setState({ status: "ready", items: result.items, hasMore: result.hasMore });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setState({
          status: "error",
          message: err instanceof ApiError ? err.message : "Unable to load chefs.",
        });
      });
    return () => {
      cancelled = true;
    };
  }, [page]);

  return (
    <section className="mx-auto max-w-5xl px-4 py-16">
      <h1 className="text-3xl font-bold tracking-tight">Home chefs</h1>
      <p className="mt-2 text-gray-600">Browse home cooks and food providers in your area.</p>

      {state.status === "error" && (
        <div className="mt-8 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {state.message}
        </div>
      )}

      {state.status === "loading" && <p className="mt-10 text-gray-600">Loading chefs…</p>}

      {state.status === "ready" && state.items.length === 0 && (
        <p className="mt-10 text-gray-600">
          No chefs have published a profile yet. Check back soon.
        </p>
      )}

      {state.status === "ready" && state.items.length > 0 && (
        <>
          <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {state.items.map((chef) => (
              <Link
                key={chef.id}
                href={`/chefs/${chef.id}`}
                className="rounded-xl border border-gray-200 p-6 transition hover:border-gray-300 hover:shadow-sm"
              >
                <h2 className="text-lg font-semibold">{chef.displayName}</h2>
                <p className="mt-1 text-sm text-gray-500">
                  {chef.city}
                  {chef.area ? `, ${chef.area}` : ""}
                </p>
                {chef.cuisines.length > 0 && (
                  <div className="mt-3 flex flex-wrap gap-1.5">
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
                <p className="mt-3 line-clamp-3 text-sm text-gray-600">{chef.bio}</p>
              </Link>
            ))}
          </div>

          <div className="mt-10 flex items-center gap-4">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              className="rounded-lg border border-gray-300 px-4 py-2 font-medium hover:bg-gray-50 disabled:opacity-40"
            >
              Previous
            </button>
            <span className="text-sm text-gray-500">Page {page}</span>
            <button
              type="button"
              disabled={!state.hasMore}
              onClick={() => setPage((p) => p + 1)}
              className="rounded-lg border border-gray-300 px-4 py-2 font-medium hover:bg-gray-50 disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </>
      )}
    </section>
  );
}
