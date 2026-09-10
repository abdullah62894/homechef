"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { listChefs, type ChefListItem } from "@/lib/chefs";
import { ApiError } from "@/lib/api";
import ChefCard from "@/components/ChefCard";

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
    listChefs({}, page, PAGE_SIZE)
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
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <h1 className="text-3xl font-bold tracking-tight">Home Chefs</h1>
      <p className="mt-2 text-gray-600">Browse home cooks and food providers in your area.</p>

      {state.status === "error" && (
        <div className="mt-8 rounded-xl border border-red-200 bg-red-50 p-6 text-center">
          <div className="mx-auto mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-red-100">
            <svg className="h-5 w-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
          </div>
          <p className="font-medium text-gray-900">{state.message}</p>
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="mt-3 text-sm text-gray-600 underline hover:text-gray-900"
          >
            Try again
          </button>
        </div>
      )}

      {state.status === "loading" && (
        <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <div key={i} className="animate-pulse rounded-2xl border border-gray-200 bg-white overflow-hidden">
              <div className="h-44 bg-gray-200" />
              <div className="p-4 space-y-3">
                <div className="h-4 w-3/4 rounded bg-gray-200" />
                <div className="h-3 w-1/2 rounded bg-gray-200" />
                <div className="flex gap-2">
                  <div className="h-5 w-16 rounded-full bg-gray-200" />
                  <div className="h-5 w-20 rounded-full bg-gray-200" />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {state.status === "ready" && state.items.length === 0 && (
        <div className="mt-8 rounded-2xl border border-dashed border-gray-300 p-10 text-center">
          <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-orange-100">
            <span className="text-2xl">👩‍🍳</span>
          </div>
          <p className="font-medium text-gray-700">No chefs yet</p>
          <p className="mt-1 text-sm text-gray-500">
            Be the first — turn your home kitchen into a food business.
          </p>
          <Link
            href="/become-a-chef"
            className="mt-4 inline-block rounded-xl bg-gray-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-gray-800"
          >
            Become a Home Chef
          </Link>
        </div>
      )}

      {state.status === "ready" && state.items.length > 0 && (
        <>
          <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {state.items.map((chef) => (
              <ChefCard key={chef.id} chef={chef} />
            ))}
          </div>

          <div className="mt-10 flex items-center justify-center gap-4">
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
