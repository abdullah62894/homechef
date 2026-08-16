"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { search, type SearchResult } from "@/lib/search";
import { ApiError } from "@/lib/api";

const PAGE_SIZE = 12;

type LoadState =
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "ready"; result: SearchResult };

export default function SearchPage() {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");
  
  const [city, setCity] = useState("");
  const [area, setArea] = useState("");
  const [cuisine, setCuisine] = useState("");
  const [type, setType] = useState<'all' | 'chefs' | 'foods'>("all");
  
  const [page, setPage] = useState(1);
  const [state, setState] = useState<LoadState>({ status: "loading" });

  useEffect(() => {
    document.title = "Search | HomeChef";
  }, []);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedQuery(query);
      setPage(1); // Reset page on new search
    }, 300);
    return () => clearTimeout(handler);
  }, [query]);

  useEffect(() => {
    let cancelled = false;

    search(
      {
        q: debouncedQuery || undefined,
        city: city || undefined,
        area: area || undefined,
        cuisine: cuisine || undefined,
        type: type,
      },
      page,
      PAGE_SIZE
    )
      .then((result) => {
        if (!cancelled) {
          setState({ status: "ready", result });
        }
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setState({
          status: "error",
          message: err instanceof ApiError ? err.message : "An error occurred while searching.",
        });
      });

    return () => {
      cancelled = true;
    };
  }, [debouncedQuery, city, area, cuisine, type, page]);

  return (
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <div>
        <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">Search</h1>
        <p className="mt-2 text-base text-gray-600">
          Find your favorite local chefs and homemade foods.
        </p>
      </div>

      <div className="mt-8 space-y-4 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div>
          <input
            type="search"
            placeholder="Search by name, dish, or keyword..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <input
            type="text"
            placeholder="City"
            value={city}
            onChange={(e) => {
              setCity(e.target.value);
              setPage(1);
            }}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
          <input
            type="text"
            placeholder="Area"
            value={area}
            onChange={(e) => {
              setArea(e.target.value);
              setPage(1);
            }}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
          <input
            type="text"
            placeholder="Cuisine"
            value={cuisine}
            onChange={(e) => {
              setCuisine(e.target.value);
              setPage(1);
            }}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
          <select
            value={type}
            onChange={(e) => {
              setType(e.target.value as 'all' | 'chefs' | 'foods');
              setPage(1);
            }}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          >
            <option value="all">All Results</option>
            <option value="chefs">Chefs Only</option>
            <option value="foods">Foods Only</option>
          </select>
        </div>
      </div>

      {state.status === "error" && (
        <div className="mt-8 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {state.message}
        </div>
      )}

      {state.status === "loading" && (
        <div className="py-20 text-center text-sm text-gray-500">Searching...</div>
      )}

      {state.status === "ready" && (
        <div className="mt-8 space-y-12">
          {state.result.chefs.length === 0 && state.result.foods.length === 0 ? (
            <div className="rounded-xl border border-dashed border-gray-300 py-16 text-center">
              <p className="font-medium text-gray-600">No results found</p>
              <p className="mt-1 text-sm text-gray-400">
                Try adjusting your search query or filters.
              </p>
            </div>
          ) : (
            <>
              {state.result.chefs.length > 0 && (
                <section>
                  <h2 className="text-2xl font-bold tracking-tight text-gray-900">
                    Chefs ({state.result.totalChefs})
                  </h2>
                  <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {state.result.chefs.map((chef) => (
                      <Link
                        key={chef.id}
                        href={`/chefs/${chef.id}`}
                        className="rounded-xl border border-gray-200 p-6 transition hover:border-gray-300 hover:shadow-sm"
                      >
                        <h3 className="text-lg font-semibold">{chef.displayName}</h3>
                        <p className="mt-1 text-sm text-gray-500">
                          {chef.city}
                          {chef.area ? `, ${chef.area}` : ""}
                        </p>
                        {chef.cuisines && chef.cuisines.length > 0 && (
                          <div className="mt-3 flex flex-wrap gap-1.5">
                            {chef.cuisines.map((c) => (
                              <span
                                key={c}
                                className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs text-gray-700"
                              >
                                {c}
                              </span>
                            ))}
                          </div>
                        )}
                        <p className="mt-3 line-clamp-3 text-sm text-gray-600">{chef.bio}</p>
                      </Link>
                    ))}
                  </div>
                </section>
              )}

              {state.result.foods.length > 0 && (
                <section>
                  <h2 className="text-2xl font-bold tracking-tight text-gray-900">
                    Foods ({state.result.totalFoods})
                  </h2>
                  <div className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {state.result.foods.map((food) => (
                      <article
                        key={food.id}
                        className="flex flex-col justify-between rounded-xl border border-gray-200 bg-white p-6 shadow-xs transition hover:border-gray-300"
                      >
                        <div>
                          <div className="flex items-start justify-between gap-2">
                            <span className="inline-block rounded-md bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                              {food.categoryName ?? "Dish"}
                            </span>
                            <span
                              className={`inline-block rounded-full px-2 py-0.5 text-[11px] font-semibold ${
                                food.isAvailable
                                  ? "border border-emerald-200 bg-emerald-50 text-emerald-700"
                                  : "bg-gray-100 text-gray-500"
                              }`}
                            >
                              {food.isAvailable ? "Available" : "Sold out"}
                            </span>
                          </div>

                          <h3 className="mt-3 text-lg font-semibold leading-snug text-gray-900">
                            <Link href={`/food/${food.id}`} className="hover:underline">
                              {food.name}
                            </Link>
                          </h3>

                          <p className="mt-2 line-clamp-2 text-sm text-gray-600">
                            {food.description}
                          </p>
                        </div>

                        <div className="mt-6 flex items-center justify-between border-t border-gray-100 pt-4">
                          <div>
                            <div className="text-base font-bold text-gray-900">
                              {food.currency} {food.price.toLocaleString()}
                            </div>
                            <Link
                              href={`/chefs/${food.chefProfileId}`}
                              className="line-clamp-1 text-xs text-gray-500 hover:text-gray-800 hover:underline"
                            >
                              by {food.chefDisplayName} ({food.chefCity})
                            </Link>
                          </div>

                          <Link
                            href={`/food/${food.id}`}
                            className="rounded-lg bg-gray-50 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-100"
                          >
                            View Details →
                          </Link>
                        </div>
                      </article>
                    ))}
                  </div>
                </section>
              )}

              <div className="mt-10 flex items-center gap-4">
                <button
                  type="button"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  className="rounded-lg border border-gray-300 px-4 py-2 font-medium transition hover:bg-gray-50 disabled:opacity-40"
                >
                  Previous
                </button>
                <span className="text-sm text-gray-500">Page {page}</span>
                <button
                  type="button"
                  disabled={
                    state.result.page * state.result.pageSize >= Math.max(
                      type === "foods" ? 0 : state.result.totalChefs,
                      type === "chefs" ? 0 : state.result.totalFoods
                    )
                  }
                  onClick={() => setPage((p) => p + 1)}
                  className="rounded-lg border border-gray-300 px-4 py-2 font-medium transition hover:bg-gray-50 disabled:opacity-40"
                >
                  Next
                </button>
              </div>
            </>
          )}
        </div>
      )}
    </section>
  );
}
