"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import {
  listFoods,
  listFoodCategories,
  type FoodListItem,
  type FoodCategory,
} from "@/lib/foods";

export default function FoodDiscoveryPage() {
  const [foods, setFoods] = useState<FoodListItem[]>([]);
  const [categories, setCategories] = useState<FoodCategory[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    listFoodCategories()
      .then((cats) => setCategories(cats))
      .catch(() => {
        // Fallback silently if categories fail to load
      });
  }, []);

  useEffect(() => {
    let cancelled = false;

    listFoods({
      categoryId: selectedCategory ?? undefined,
      search: searchQuery.trim() || undefined,
    })
      .then((page) => {
        if (!cancelled) setFoods(page.items);
      })
      .catch(() => {
        if (!cancelled) setError("Unable to load food items right now.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [selectedCategory, searchQuery]);

  return (
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">Explore Food &amp; Menus</h1>
          <p className="mt-2 text-base text-gray-600">
            Fresh homemade meals, bakery delights, and traditional dishes crafted by local home chefs.
          </p>
        </div>
      </div>

      {/* Search & Category Filters */}
      <div className="mt-8 space-y-4">
        <div>
          <input
            type="search"
            placeholder="Search dishes (e.g. Biryani, Karahi, Chocolate Cake)..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full max-w-md rounded-lg border border-gray-300 px-4 py-2.5 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>

        {categories.length > 0 && (
          <div className="flex flex-wrap items-center gap-2 pt-2">
            <button
              type="button"
              onClick={() => setSelectedCategory(null)}
              className={`rounded-full px-3 py-1 text-xs font-medium transition ${
                selectedCategory === null
                  ? "bg-gray-900 text-white"
                  : "bg-gray-100 text-gray-700 hover:bg-gray-200"
              }`}
            >
              All Dishes
            </button>
            {categories.map((cat) => (
              <button
                key={cat.id}
                type="button"
                onClick={() => setSelectedCategory(cat.id === selectedCategory ? null : cat.id)}
                className={`rounded-full px-3 py-1 text-xs font-medium transition ${
                  selectedCategory === cat.id
                    ? "bg-gray-900 text-white"
                    : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                }`}
              >
                {cat.name}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Content Feed */}
      {loading ? (
        <div className="py-20 text-center text-sm text-gray-500">Loading dishes…</div>
      ) : error ? (
        <div className="mt-8 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          {error}
        </div>
      ) : foods.length === 0 ? (
        <div className="mt-12 rounded-xl border border-dashed border-gray-300 py-16 text-center">
          <p className="text-gray-600 font-medium">No dishes found</p>
          <p className="mt-1 text-sm text-gray-400">
            {searchQuery || selectedCategory
              ? "Try adjusting your search query or category filter."
              : "Chefs haven't listed dishes yet. Check back soon!"}
          </p>
        </div>
      ) : (
        <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {foods.map((food) => (
            <article
              key={food.id}
              className="flex flex-col justify-between rounded-xl border border-gray-200 bg-white p-6 shadow-xs hover:border-gray-300 transition"
            >
              <div>
                <div className="flex items-start justify-between gap-2">
                  <span className="inline-block rounded-md bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                    {food.categoryName ?? "Dish"}
                  </span>
                  <span
                    className={`inline-block rounded-full px-2 py-0.5 text-[11px] font-semibold ${
                      food.isAvailable
                        ? "bg-emerald-50 text-emerald-700 border border-emerald-200"
                        : "bg-gray-100 text-gray-500"
                    }`}
                  >
                    {food.isAvailable ? "Available" : "Sold out"}
                  </span>
                </div>

                <h2 className="mt-3 text-lg font-semibold text-gray-900 leading-snug">
                  <Link href={`/food/${food.id}`} className="hover:underline">
                    {food.name}
                  </Link>
                </h2>

                <p className="mt-2 text-sm text-gray-600 line-clamp-2">{food.description}</p>
              </div>

              <div className="mt-6 border-t border-gray-100 pt-4 flex items-center justify-between">
                <div>
                  <div className="text-base font-bold text-gray-900">
                    {food.currency} {food.price.toLocaleString()}
                  </div>
                  <Link
                    href={`/chefs/${food.chefProfileId}`}
                    className="text-xs text-gray-500 hover:text-gray-800 hover:underline line-clamp-1"
                  >
                    by {food.chefDisplayName} ({food.chefCity})
                  </Link>
                </div>

                <Link
                  href={`/food/${food.id}`}
                  className="rounded-lg bg-gray-50 px-3 py-1.5 text-xs font-medium text-gray-700 hover:bg-gray-100 transition"
                >
                  View Details →
                </Link>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
