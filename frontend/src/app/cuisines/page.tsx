"use client";

import { listActiveCuisines, type Cuisine } from "@/lib/cuisines";
import Link from "next/link";
import Image from "next/image";
import { resolveImageUrl } from "@/lib/images";
import { useState, useEffect } from "react";

export default function CuisinesPage() {
  const [cuisines, setCuisines] = useState<Cuisine[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    listActiveCuisines().then(setCuisines).finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center">
        <p className="text-gray-500">Loading cuisines...</p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900">Cuisines</h1>
      <p className="mt-2 text-gray-500">Browse dishes by your favorite cuisine</p>

      <div className="mt-8 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {cuisines.map((cuisine) => (
          <Link
            key={cuisine.id}
            href={`/cuisines/${cuisine.id}`}
            className="group relative overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm hover:shadow-md transition-shadow"
          >
            {cuisine.imageThumbnailUrl || cuisine.imageUrl ? (
              <div className="relative aspect-square">
                <Image
                  src={resolveImageUrl(cuisine.imageThumbnailUrl || cuisine.imageUrl!) ?? ""}
                  alt={cuisine.name}
                  fill
                  className="object-cover"
                />
              </div>
            ) : (
              <div className="flex aspect-square items-center justify-center bg-gradient-to-br from-orange-100 to-orange-200">
                <span className="text-4xl">🍽️</span>
              </div>
            )}
            <div className="p-3">
              <h2 className="text-sm font-semibold text-gray-900 group-hover:text-orange-500">{cuisine.name}</h2>
              {cuisine.description && (
                <p className="mt-1 text-xs text-gray-500 line-clamp-2">{cuisine.description}</p>
              )}
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
