"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { getFood, type FoodItem } from "@/lib/foods";
import { resolveImageUrl } from "@/lib/images";
import ReportButton from "@/components/ReportButton";
import {
  addFoodFavorite,
  getUserFavoriteIds,
  removeFoodFavorite,
} from "@/lib/favorites";
import { ApiError } from "@/lib/api";
import { chefHref } from "@/lib/slugs";
import { useCart } from "@/components/CartProvider";

type LoadState =
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "ready"; food: FoodItem };

export default function FoodDetailView({ id }: { id: string }) {
  const [state, setState] = useState<LoadState>({ status: "loading" });
  const [isFavorited, setIsFavorited] = useState(false);
  const [togglingFav, setTogglingFav] = useState(false);
  const [quantity, setQuantity] = useState(1);
  const { addItem } = useCart();

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

  function handleAddToCart() {
    if (state.status !== "ready") return;
    const food = state.food;
    addItem({
      foodItemId: food.id,
      chefProfileId: food.chefProfileId,
      chefName: food.chefDisplayName,
      dishName: food.name,
      unitPrice: food.price,
      currency: food.currency,
      imageUrl: food.imageUrl,
    }, quantity);
  }

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
      <nav aria-label="Breadcrumb" className="text-sm text-gray-500">
        <Link href="/" className="hover:underline">Home</Link>
        <span className="mx-1.5">/</span>
        <Link href="/food" className="hover:underline">Dishes</Link>
        <span className="mx-1.5">/</span>
        <span className="text-gray-900">{food.name}</span>
      </nav>

      <div className="mt-6 grid gap-8 md:grid-cols-3">
        <div className="md:col-span-2">
          <div className="rounded-2xl border border-gray-200 bg-white p-6 sm:p-8">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div className="flex flex-wrap items-center gap-2">
                {food.cuisineNames?.length > 0 ? (
                  food.cuisineNames.map((name) => (
                    <span key={name} className="rounded-md bg-orange-100 px-2.5 py-0.5 text-xs font-semibold text-orange-700">
                      {name}
                    </span>
                  ))
                ) : (
                  <span className="rounded-md bg-gray-100 px-2.5 py-0.5 text-xs font-semibold text-gray-700">
                    {food.categoryName ?? "General Dish"}
                  </span>
                )}
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

            {resolveImageUrl(food.imageUrl) && (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={resolveImageUrl(food.imageUrl) ?? ""}
                alt={food.name}
                className="mt-4 aspect-video w-full rounded-xl border border-gray-100 object-cover"
              />
            )}

            <div className="mt-4 flex items-baseline gap-2">
              <span className="text-3xl font-extrabold text-gray-900">
                {food.currency} {food.price.toLocaleString()}
              </span>
            </div>

            {food.isAvailable && (
              <div className="mt-4 flex items-center gap-3">
                <div className="flex items-center rounded-lg border border-gray-300">
                  <button onClick={() => setQuantity(Math.max(1, quantity - 1))} className="px-3 py-2 text-gray-600 hover:bg-gray-50">-</button>
                  <span className="w-10 text-center text-sm font-medium">{quantity}</span>
                  <button onClick={() => setQuantity(quantity + 1)} className="px-3 py-2 text-gray-600 hover:bg-gray-50">+</button>
                </div>
                <button
                  onClick={handleAddToCart}
                  className="flex-1 rounded-lg bg-orange-500 px-4 py-2.5 text-sm font-semibold text-white hover:bg-orange-600 transition"
                >
                  Add to Cart
                </button>
              </div>
            )}

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
              <div className="mt-4 flex justify-end">
                <ReportButton targetType="FoodItem" targetId={food.id} targetName="dish" />
              </div>
            </div>
          </div>
        </div>

        {/* Chef Sidebar Card */}
        <div>
          <div className="rounded-2xl border border-gray-200 bg-gray-50/50 p-6">
            <h2 className="text-xs font-bold uppercase tracking-wider text-gray-500">Prepared by</h2>
            <div className="mt-3">
              <Link
                href={chefHref(food.chefProfileId, food.chefDisplayName)}
                className="text-lg font-bold text-gray-900 hover:underline"
              >
                {food.chefDisplayName}
              </Link>
              <p className="mt-1 flex items-center gap-1 text-sm text-gray-600">
                <svg className="h-3.5 w-3.5 shrink-0 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                {food.chefCity}
                {food.chefArea ? `, ${food.chefArea}` : ""}
              </p>
            </div>

            <div className="mt-6 space-y-2 pt-4 border-t border-gray-200">
              <Link
                href={chefHref(food.chefProfileId, food.chefDisplayName)}
                className="block w-full text-center rounded-lg bg-orange-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-orange-700 transition"
              >
                Contact Chef
              </Link>
              <Link
                href={chefHref(food.chefProfileId, food.chefDisplayName)}
                className="block w-full text-center rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition"
              >
                View Full Menu
              </Link>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
