"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { getFood, type FoodItem } from "@/lib/foods";
import {
  addFoodFavorite,
  getUserFavoriteIds,
  removeFoodFavorite,
} from "@/lib/favorites";
import { ApiError } from "@/lib/api";

type LoadState =
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "ready"; food: FoodItem };

export default function FoodDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [state, setState] = useState<LoadState>({ status: "loading" });
  const [isFavorited, setIsFavorited] = useState(false);
  const [togglingFav, setTogglingFav] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getFood(id)
      .then((food) => {
        if (!cancelled) setState({ status: "ready", food });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setState({
          status: "error",
          message: err instanceof ApiError ? err.message : "Unable to load dish details.",
        });
      });

    // Check initial favorite status
    getUserFavoriteIds()
      .then((ids) => {
        if (!cancelled) {
          setIsFavorited(ids.foodIds.includes(id));
        }
      })
      .catch(() => {
        // Unauthenticated or error
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  async function handleToggleFavorite() {
    setTogglingFav(true);
    try {
      if (isFavorited) {
        await removeFoodFavorite(id);
        setIsFavorited(false);
      } else {
        await addFoodFavorite(id);
        setIsFavorited(true);
      }
    } catch {
      // Ignored
    } finally {
      setTogglingFav(false);
    }
  }

  if (state.status === "loading") {
    return <section className="mx-auto max-w-4xl px-4 py-16 text-gray-600">Loading dish details…</section>;
  }

  if (state.status === "error") {
    return (
      <section className="mx-auto max-w-4xl px-4 py-16">
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {state.message}
        </div>
        <Link href="/food" className="mt-6 inline-block text-sm text-gray-600 underline">
          ← Back to all dishes
        </Link>
      </section>
    );
  }

  const food = state.food;

  return (
    <section className="mx-auto max-w-4xl px-4 py-12 sm:py-16">
      <Link href="/food" className="text-sm font-medium text-gray-500 hover:text-gray-900">
        ← Back to all dishes
      </Link>

      <div className="mt-6 grid gap-8 md:grid-cols-3">
        <div className="md:col-span-2">
          <div className="rounded-2xl border border-gray-200 bg-white p-6 sm:p-8">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div className="flex flex-wrap items-center gap-2">
                <span className="rounded-md bg-gray-100 px-2.5 py-0.5 text-xs font-semibold text-gray-700">
                  {food.categoryName ?? "General Dish"}
                </span>
                <span
                  className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                    food.isAvailable
                      ? "bg-emerald-50 text-emerald-700 border border-emerald-200"
                      : "bg-gray-100 text-gray-500"
                  }`}
                >
                  {food.isAvailable ? "Available Now" : "Currently Sold Out"}
                </span>
              </div>

              {/* Favorite Button */}
              <button
                type="button"
                onClick={handleToggleFavorite}
                disabled={togglingFav}
                className={`flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold border transition ${
                  isFavorited
                    ? "bg-red-50 text-red-600 border-red-200 hover:bg-red-100"
                    : "bg-gray-50 text-gray-700 border-gray-200 hover:bg-gray-100"
                }`}
              >
                <span>{isFavorited ? "♥" : "♡"}</span>
                <span>{isFavorited ? "Favorited" : "Favorite"}</span>
              </button>
            </div>

            <h1 className="mt-4 text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
              {food.name}
            </h1>

            <div className="mt-4 flex items-baseline gap-2">
              <span className="text-3xl font-extrabold text-gray-900">
                {food.currency} {food.price.toLocaleString()}
              </span>
            </div>

            {food.preparationTimeMinutes && (
              <div className="mt-4 flex items-center gap-1.5 text-sm text-gray-500">
                <span>⏱ Prep time: ~{food.preparationTimeMinutes} minutes</span>
              </div>
            )}

            <div className="mt-6 border-t border-gray-100 pt-6">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-gray-500">Description</h2>
              <p className="mt-2 text-base leading-relaxed text-gray-700 whitespace-pre-line">
                {food.description}
              </p>
            </div>
          </div>
        </div>

        {/* Chef Sidebar Card */}
        <div>
          <div className="rounded-2xl border border-gray-200 bg-gray-50/50 p-6">
            <h2 className="text-xs font-bold uppercase tracking-wider text-gray-500">Prepared by</h2>
            <div className="mt-3">
              <Link
                href={`/chefs/${food.chefProfileId}`}
                className="text-lg font-bold text-gray-900 hover:underline"
              >
                {food.chefDisplayName}
              </Link>
              <p className="mt-1 text-sm text-gray-600">
                {food.chefCity}
                {food.chefArea ? `, ${food.chefArea}` : ""}
              </p>
            </div>

            <div className="mt-6 pt-4 border-t border-gray-200">
              <Link
                href={`/chefs/${food.chefProfileId}`}
                className="block w-full text-center rounded-lg bg-gray-900 px-4 py-2.5 text-sm font-medium text-white hover:bg-gray-800 transition"
              >
                View Chef Profile &amp; Full Menu
              </Link>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
