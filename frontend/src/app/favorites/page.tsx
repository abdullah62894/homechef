"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  listFavoriteChefs,
  listFavoriteFoods,
  removeChefFavorite,
  removeFoodFavorite,
} from "@/lib/favorites";
import type { ChefListItem } from "@/lib/chefs";
import type { FoodListItem } from "@/lib/foods";
import { ApiError } from "@/lib/api";

type Tab = "chefs" | "foods";

export default function FavoritesPage() {
  const router = useRouter();
  const [tab, setTab] = useState<Tab>("chefs");
  const [chefs, setChefs] = useState<ChefListItem[]>([]);
  const [foods, setFoods] = useState<FoodListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    Promise.all([listFavoriteChefs(1, 50), listFavoriteFoods(1, 50)])
      .then(([chefsData, foodsData]) => {
        if (cancelled) return;
        setChefs(chefsData.items);
        setFoods(foodsData.items);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 401) {
          router.replace("/login");
          return;
        }
        setError(err instanceof ApiError ? err.message : "Unable to load your saved favorites.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [router]);

  async function handleRemoveChef(chefId: string) {
    try {
      await removeChefFavorite(chefId);
      setChefs((prev) => prev.filter((c) => c.id !== chefId));
    } catch {
      // Ignored
    }
  }

  async function handleRemoveFood(foodId: string) {
    try {
      await removeFoodFavorite(foodId);
      setFoods((prev) => prev.filter((f) => f.id !== foodId));
    } catch {
      // Ignored
    }
  }

  return (
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-gray-200 pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">Your Favorites</h1>
          <p className="mt-1 text-sm text-gray-600">Quickly find your saved home chefs and favorite dishes.</p>
        </div>

        {/* Tab switcher */}
        <div className="flex rounded-xl bg-gray-100 p-1">
          <button
            type="button"
            onClick={() => setTab("chefs")}
            className={`rounded-lg px-4 py-2 text-xs font-semibold transition ${
              tab === "chefs"
                ? "bg-white text-gray-900 shadow-xs"
                : "text-gray-600 hover:text-gray-900"
            }`}
          >
            Chefs ({chefs.length})
          </button>
          <button
            type="button"
            onClick={() => setTab("foods")}
            className={`rounded-lg px-4 py-2 text-xs font-semibold transition ${
              tab === "foods"
                ? "bg-white text-gray-900 shadow-xs"
                : "text-gray-600 hover:text-gray-900"
            }`}
          >
            Dishes ({foods.length})
          </button>
        </div>
      </div>

      {loading ? (
        <div className="py-16 text-center text-sm text-gray-500">Loading your favorites…</div>
      ) : error ? (
        <div className="mt-8 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          {error}
        </div>
      ) : tab === "chefs" ? (
        chefs.length === 0 ? (
          <div className="mt-12 rounded-2xl border border-dashed border-gray-300 py-16 text-center">
            <h3 className="text-base font-semibold text-gray-900">No favorite chefs saved yet</h3>
            <p className="mt-1 text-sm text-gray-500">Explore talented local home cooks and bookmark your favorites.</p>
            <Link
              href="/chefs"
              className="mt-6 inline-block rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 transition"
            >
              Browse Chefs →
            </Link>
          </div>
        ) : (
          <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {chefs.map((chef) => (
              <div
                key={chef.id}
                className="flex flex-col justify-between rounded-xl border border-gray-200 bg-white p-5 shadow-xs hover:border-gray-300 transition"
              >
                <div>
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="text-lg font-bold text-gray-900">
                      <Link href={`/chefs/${chef.id}`} className="hover:underline">
                        {chef.displayName}
                      </Link>
                    </h3>
                    <button
                      type="button"
                      onClick={() => handleRemoveChef(chef.id)}
                      title="Remove from favorites"
                      className="text-red-500 hover:text-red-700 text-lg transition p-1"
                    >
                      ♥
                    </button>
                  </div>

                  <p className="mt-1 text-xs text-gray-500">
                    📍 {chef.city}
                    {chef.area ? `, ${chef.area}` : ""}
                  </p>

                  <p className="mt-3 text-sm text-gray-600 line-clamp-2">{chef.bio}</p>

                  {chef.cuisines.length > 0 && (
                    <div className="mt-3 flex flex-wrap gap-1">
                      {chef.cuisines.map((c) => (
                        <span key={c} className="rounded bg-gray-100 px-2 py-0.5 text-[11px] text-gray-600">
                          {c}
                        </span>
                      ))}
                    </div>
                  )}
                </div>

                <div className="mt-4 border-t border-gray-100 pt-3">
                  <Link
                    href={`/chefs/${chef.id}`}
                    className="block text-center text-xs font-semibold text-gray-900 hover:underline"
                  >
                    View Kitchen &amp; Menu →
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )
      ) : foods.length === 0 ? (
        <div className="mt-12 rounded-2xl border border-dashed border-gray-300 py-16 text-center">
          <h3 className="text-base font-semibold text-gray-900">No favorite dishes saved yet</h3>
          <p className="mt-1 text-sm text-gray-500">Discover mouthwatering homemade food and save dishes to order again.</p>
          <Link
            href="/food"
            className="mt-6 inline-block rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 transition"
          >
            Explore Food →
          </Link>
        </div>
      ) : (
        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {foods.map((food) => (
            <div
              key={food.id}
              className="flex flex-col justify-between rounded-xl border border-gray-200 bg-white p-5 shadow-xs hover:border-gray-300 transition"
            >
              <div>
                <div className="flex items-center justify-between gap-2">
                  <span className="rounded bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                    {food.categoryName ?? "Dish"}
                  </span>
                  <button
                    type="button"
                    onClick={() => handleRemoveFood(food.id)}
                    title="Remove from favorites"
                    className="text-red-500 hover:text-red-700 text-lg transition p-1"
                  >
                    ♥
                  </button>
                </div>

                <h3 className="mt-3 text-lg font-bold text-gray-900">
                  <Link href={`/food/${food.id}`} className="hover:underline">
                    {food.name}
                  </Link>
                </h3>

                <p className="mt-1 text-xs text-gray-500">
                  by{" "}
                  <Link href={`/chefs/${food.chefProfileId}`} className="font-medium text-gray-700 underline">
                    {food.chefDisplayName}
                  </Link>
                </p>

                <p className="mt-2 text-sm text-gray-600 line-clamp-2">{food.description}</p>
              </div>

              <div className="mt-4 flex items-center justify-between border-t border-gray-100 pt-3">
                <span className="text-base font-bold text-gray-900">
                  {food.currency} {food.price.toLocaleString()}
                </span>
                <Link
                  href={`/food/${food.id}`}
                  className="rounded-lg bg-gray-50 px-3 py-1 text-xs font-medium text-gray-700 hover:bg-gray-100 transition"
                >
                  Details →
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
