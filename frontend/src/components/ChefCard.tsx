import Link from "next/link";
import type { ChefListItem } from "@/lib/chefs";
import { chefHref } from "@/lib/slugs";
import { resolveImageUrl } from "@/lib/images";

/**
 * Marketplace-style chef card (Stage 13): photo, rating, location,
 * cuisines and starting price.
 */
export default function ChefCard({ chef }: { chef: ChefListItem }) {
  const photo = resolveImageUrl(chef.photoThumbnailUrl ?? chef.photoUrl);

  return (
    <Link
      href={chefHref(chef.id, chef.displayName)}
      className="group flex flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-xs transition hover:border-gray-300 hover:shadow-md"
    >
      <div className="relative h-40 w-full bg-gray-100">
        {photo ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={photo}
            alt={chef.displayName}
            loading="lazy"
            className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-4xl">👩‍🍳</div>
        )}
        {chef.distanceKm != null && (
          <span className="absolute right-2 top-2 rounded-full bg-white/90 px-2 py-0.5 text-xs font-semibold text-gray-700 shadow-xs">
            {chef.distanceKm} km away
          </span>
        )}
      </div>

      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-start justify-between gap-2">
          <h3 className="font-semibold leading-snug text-gray-900 group-hover:underline">
            {chef.displayName}
          </h3>
          {chef.ratingCount > 0 && chef.ratingAverage != null && (
            <span className="flex shrink-0 items-center gap-1 text-sm">
              <span className="text-amber-400">★</span>
              <span className="font-semibold text-gray-900">{chef.ratingAverage.toFixed(1)}</span>
              <span className="text-xs text-gray-500">({chef.ratingCount})</span>
            </span>
          )}
        </div>

        <p className="mt-1 text-sm text-gray-500">
          📍 {chef.city}
          {chef.area ? `, ${chef.area}` : ""}
        </p>

        {chef.cuisines.length > 0 && (
          <div className="mt-2 flex flex-wrap gap-1.5">
            {chef.cuisines.slice(0, 3).map((cuisine) => (
              <span
                key={cuisine}
                className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-700"
              >
                {cuisine}
              </span>
            ))}
          </div>
        )}

        <div className="mt-auto pt-3">
          {chef.startingPrice != null ? (
            <p className="text-sm text-gray-700">
              Dishes from{" "}
              <span className="font-bold text-gray-900">
                {chef.startingPrice.toLocaleString()}
              </span>
            </p>
          ) : (
            <p className="text-xs text-gray-400">Menu coming soon</p>
          )}
        </div>
      </div>
    </Link>
  );
}
