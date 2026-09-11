"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { getCuisine, type Cuisine } from "@/lib/cuisines";
import { listFoods, type FoodListItem } from "@/lib/foods";
import { resolveImageUrl } from "@/lib/images";
import { useCart } from "@/components/CartProvider";
import { ApiError } from "@/lib/api";

export default function CuisineDetailPage() {
  const params = useParams();
  const id = params.id as string;
  const { addItem } = useCart();
  const [cuisine, setCuisine] = useState<Cuisine | null>(null);
  const [foods, setFoods] = useState<FoodListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    getCuisine(id)
      .then(async (cuisineResult) => {
        if (cancelled) return;
        setCuisine(cuisineResult);

        const foodResult = await listFoods({ cuisine: cuisineResult.name }, 1, 50);
        if (!cancelled) {
          setFoods(foodResult.items);
        }
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 404) {
          setError("Cuisine not found.");
        } else {
          setError(err instanceof ApiError ? err.message : "Failed to load cuisine.");
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, [id]);

  if (loading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center text-gray-500">
        Loading...
      </div>
    );
  }

  if (error || !cuisine) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900">Cuisine not found</h1>
        <p className="mt-2 text-gray-500">{error || "This cuisine doesn't exist."}</p>
        <Link href="/cuisines" className="mt-6 inline-block text-sm font-medium text-orange-500 hover:text-orange-600 underline">
          Browse all cuisines
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <div className="flex items-center gap-2 text-sm text-gray-500 mb-4">
        <Link href="/cuisines" className="hover:underline">Cuisines</Link>
        <span>/</span>
        <span className="text-gray-900 font-medium">{cuisine.name}</span>
      </div>

      {resolveImageUrl(cuisine.imageThumbnailUrl ?? cuisine.imageUrl) ? (
        <div className="relative h-48 w-full overflow-hidden rounded-xl sm:h-64">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={resolveImageUrl(cuisine.imageThumbnailUrl ?? cuisine.imageUrl)!}
            alt={cuisine.name}
            className="h-full w-full object-cover"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
          <div className="absolute bottom-4 left-4">
            <h1 className="text-2xl font-bold text-white sm:text-3xl">{cuisine.name}</h1>
            {cuisine.description && (
              <p className="mt-1 text-sm text-white/80 max-w-lg">{cuisine.description}</p>
            )}
          </div>
        </div>
      ) : (
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{cuisine.name}</h1>
          {cuisine.description && (
            <p className="mt-2 text-gray-600">{cuisine.description}</p>
          )}
        </div>
      )}

      <div className="mt-8">
        <h2 className="text-lg font-bold text-gray-900">
          Dishes in {cuisine.name}
          <span className="ml-2 text-sm font-normal text-gray-500">({foods.length})</span>
        </h2>

        {foods.length === 0 ? (
          <div className="mt-6 rounded-xl border border-dashed border-gray-300 py-12 text-center">
            <p className="text-gray-500">No dishes found for this cuisine yet.</p>
            <Link href="/food" className="mt-3 inline-block text-sm font-medium text-orange-500 hover:text-orange-600 underline">
              Browse all dishes
            </Link>
          </div>
        ) : (
          <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {foods.map((food) => (
              <Link
                key={food.id}
                href={`/food/${food.id}`}
                className="group rounded-xl border border-gray-200 bg-white overflow-hidden shadow-sm hover:shadow-md transition-shadow"
              >
                {resolveImageUrl(food.imageThumbnailUrl ?? food.imageUrl) ? (
                  <div className="relative aspect-video">
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    <img
                      src={resolveImageUrl(food.imageThumbnailUrl ?? food.imageUrl)!}
                      alt={food.name}
                      className="h-full w-full object-cover group-hover:scale-105 transition-transform"
                    />
                  </div>
                ) : (
                  <div className="flex aspect-video items-center justify-center bg-orange-50">
                    <span className="text-3xl">🍽️</span>
                  </div>
                )}
                <div className="p-3">
                  <div className="flex items-start justify-between">
                    <div className="min-w-0">
                      <h3 className="text-sm font-semibold text-gray-900 group-hover:text-orange-500">{food.name}</h3>
                      <p className="mt-0.5 text-xs text-gray-500 line-clamp-1">{food.description}</p>
                    </div>
                    <span className="text-sm font-bold text-gray-900 whitespace-nowrap ml-2">
                      {food.currency} {food.price.toLocaleString()}
                    </span>
                  </div>
                  <div className="mt-2 flex items-center justify-between">
                    <span className="text-xs text-gray-400">{food.chefDisplayName}</span>
                    <button
                      type="button"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        addItem({
                          foodItemId: food.id,
                          chefProfileId: food.chefProfileId,
                          chefName: food.chefDisplayName,
                          dishName: food.name,
                          unitPrice: food.price,
                          currency: food.currency,
                          imageUrl: food.imageThumbnailUrl ?? food.imageUrl,
                        });
                      }}
                      className="rounded-lg bg-orange-500 px-3 py-1 text-xs font-semibold text-white hover:bg-orange-600 transition"
                    >
                      Add to cart
                    </button>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
