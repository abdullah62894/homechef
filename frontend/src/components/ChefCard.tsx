import Link from "next/link";
import type { ChefListItem } from "@/lib/chefs";
import { chefHref } from "@/lib/slugs";
import { resolveImageUrl } from "@/lib/images";

export default function ChefCard({ chef }: { chef: ChefListItem }) {
  const photo = resolveImageUrl(chef.photoThumbnailUrl ?? chef.photoUrl);
  const hasRating = chef.ratingCount > 0 && chef.ratingAverage != null;
  const hasPrice = chef.startingPrice != null;

  return (
    <Link
      href={chefHref(chef.id, chef.displayName)}
      className="group flex flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-xs transition-all duration-200 hover:border-gray-300 hover:shadow-md"
    >
      {/* Cover image area */}
      <div className="relative h-44 w-full bg-gradient-to-br from-orange-100 to-orange-50">
        {photo ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={photo}
            alt={chef.displayName}
            loading="lazy"
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-orange-100 to-orange-50">
            <span className="text-5xl">👩‍🍳</span>
          </div>
        )}

        {/* Overlay badges */}
        <div className="absolute left-2.5 top-2.5 flex gap-1.5">
          {hasRating && (
            <span className="flex items-center gap-1 rounded-full bg-white/95 px-2.5 py-1 text-xs font-semibold shadow-sm backdrop-blur-sm">
              <span className="text-amber-500">★</span>
              <span className="text-gray-900">{chef.ratingAverage!.toFixed(1)}</span>
            </span>
          )}
          {chef.ratingCount > 0 && (
            <span className="flex items-center rounded-full bg-white/95 px-2.5 py-1 text-xs font-medium text-gray-600 shadow-sm backdrop-blur-sm">
              {chef.ratingCount} {chef.ratingCount === 1 ? "review" : "reviews"}
            </span>
          )}
        </div>

        {chef.distanceKm != null && (
          <span className="absolute right-2.5 top-2.5 rounded-full bg-white/95 px-2.5 py-1 text-xs font-semibold text-gray-700 shadow-sm backdrop-blur-sm">
            {chef.distanceKm} km away
          </span>
        )}
      </div>

      {/* Content */}
      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-start justify-between gap-2">
          <h3 className="font-semibold leading-snug text-gray-900 group-hover:text-orange-600 transition-colors">
            {chef.displayName}
          </h3>
          <span className="flex shrink-0 items-center gap-1 rounded-full bg-orange-50 px-2 py-0.5 text-[10px] font-semibold text-orange-700 ring-1 ring-orange-200">
            <svg className="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M16.403 12.652a3 3 0 000-5.304 3 3 0 00-3.75-3.751 3 3 0 00-5.305 0 3 3 0 00-3.751 3.75 3 3 0 000 5.305 3 3 0 003.75 3.751 3 3 0 005.305 0 3 3 0 003.751-3.75zm-2.546-4.46a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z" clipRule="evenodd" />
            </svg>
            Verified
          </span>
        </div>

        <p className="mt-1.5 flex items-center gap-1 text-sm text-gray-500">
          <svg className="h-3.5 w-3.5 shrink-0 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          {chef.city}
          {chef.area ? `, ${chef.area}` : ""}
        </p>

        {chef.cuisines.length > 0 && (
          <div className="mt-2.5 flex flex-wrap gap-1.5">
            {chef.cuisines.slice(0, 3).map((cuisine) => (
              <span
                key={cuisine}
                className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600"
              >
                {cuisine}
              </span>
            ))}
            {chef.cuisines.length > 3 && (
              <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-500">
                +{chef.cuisines.length - 3}
              </span>
            )}
          </div>
        )}

        <div className="mt-auto pt-3 border-t border-gray-100">
          {hasPrice ? (
            <div className="flex items-center justify-between">
              <p className="text-sm text-gray-500">
                Starting from{" "}
                <span className="font-bold text-gray-900">
                  {chef.startingPrice!.toLocaleString()}
                </span>
              </p>
              <span className="text-xs font-medium text-orange-600 group-hover:underline">
                View menu →
              </span>
            </div>
          ) : (
            <p className="text-xs text-gray-400 italic">Chef is setting up their menu</p>
          )}
        </div>
      </div>
    </Link>
  );
}
