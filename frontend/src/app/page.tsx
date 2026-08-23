import Link from "next/link";
import ChefCard from "@/components/ChefCard";
import type { ChefListItem } from "@/lib/chefs";
import type { FoodCategory } from "@/lib/foods";
import { apiUrl } from "@/lib/api";

export const revalidate = 120;

const FALLBACK_CATEGORIES = [
  "Rice & Biryani",
  "BBQ & Grills",
  "Cakes & Bakery",
  "Desserts",
  "Fast Food",
  "Vegetarian",
];

async function fetchJson<T>(path: string, fallback: T): Promise<T> {
  try {
    const res = await fetch(apiUrl(path), { next: { revalidate: 120 } });
    if (!res.ok) return fallback;
    return (await res.json()) as T;
  } catch {
    return fallback;
  }
}

export default async function Home() {
  const [chefsPage, categoriesEnvelope] = await Promise.all([
    fetchJson<{ data: ChefListItem[] } | null>("/api/chefs?page=1&pageSize=8", null),
    fetchJson<{ data: FoodCategory[] } | null>("/api/foods/categories", null),
  ]);

  const chefs = chefsPage?.data ?? [];
  const categories = (categoriesEnvelope?.data ?? []).slice(0, 8);
  const categoryNames = categories.length > 0 ? categories.map((c) => c.name) : FALLBACK_CATEGORIES;

  const websiteJsonLd = {
    "@context": "https://schema.org",
    "@type": "WebSite",
    name: "HomeChef",
    description:
      "Discover home-based food chefs near you — browse homemade menus, reviews and order from local home kitchens.",
    potentialAction: {
      "@type": "SearchAction",
      target: "/search?q={search_term_string}",
      "query-input": "required name=search_term_string",
    },
  };

  return (
    <>
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(websiteJsonLd) }}
      />

      {/* Hero */}
      <section className="border-b bg-gradient-to-b from-orange-50 via-white to-white">
        <div className="mx-auto max-w-5xl px-4 py-14 sm:py-20">
          <h1 className="max-w-2xl text-4xl font-bold tracking-tight text-gray-900 sm:text-5xl">
            Find homemade food near you
          </h1>
          <p className="mt-4 max-w-xl text-lg text-gray-600">
            Fresh biryani, BBQ, cakes and daily specials from trusted home
            chefs in your city — cooked at home, made to order.
          </p>

          {/* Primary search: food + city */}
          <form action="/search" method="get" className="mt-8 flex max-w-xl flex-col gap-2 sm:flex-row">
            <input
              type="search"
              name="q"
              placeholder="Search dishes — biryani, karahi, cake…"
              aria-label="Search dishes"
              className="w-full rounded-xl border border-gray-300 bg-white px-4 py-3 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
            />
            <input
              type="text"
              name="city"
              placeholder="City (e.g. Islamabad)"
              aria-label="City"
              className="w-full rounded-xl border border-gray-300 bg-white px-4 py-3 text-sm shadow-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900 sm:w-44"
            />
            <button
              type="submit"
              className="shrink-0 rounded-xl bg-gray-900 px-6 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-gray-800"
            >
              Search
            </button>
          </form>

          <div className="mt-6 flex flex-wrap items-center gap-3">
            <Link
              href="/chefs"
              className="rounded-xl bg-orange-600 px-5 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-orange-700"
            >
              Browse Home Chefs →
            </Link>
            <Link
              href="/become-a-chef"
              className="rounded-xl border border-gray-300 bg-white px-5 py-3 text-sm font-semibold text-gray-800 transition hover:bg-gray-50"
            >
              Become a Home Chef
            </Link>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="mx-auto max-w-5xl px-4 py-10">
        <div className="flex items-baseline justify-between">
          <h2 className="text-xl font-bold tracking-tight">Popular categories</h2>
          <Link href="/food" className="text-sm text-gray-600 underline hover:text-gray-900">
            All dishes
          </Link>
        </div>
        <div className="mt-4 flex flex-wrap gap-2">
          {categoryNames.map((name) => (
            <Link
              key={name}
              href={`/food?category=${encodeURIComponent(name)}`}
              className="rounded-full border border-gray-200 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-xs transition hover:border-gray-900 hover:text-gray-900"
            >
              {name}
            </Link>
          ))}
        </div>
      </section>

      {/* Trending chefs */}
      <section className="mx-auto max-w-5xl px-4 pb-12">
        <div className="flex items-baseline justify-between">
          <h2 className="text-xl font-bold tracking-tight">Home chefs to try</h2>
          <Link href="/chefs" className="text-sm text-gray-600 underline hover:text-gray-900">
            View all
          </Link>
        </div>

        {chefs.length === 0 ? (
          <div className="mt-6 rounded-2xl border border-dashed border-gray-300 p-10 text-center">
            <p className="font-medium text-gray-700">Chefs are joining soon</p>
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
        ) : (
          <div className="mt-5 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
            {chefs.map((chef) => (
              <ChefCard key={chef.id} chef={chef} />
            ))}
          </div>
        )}
      </section>

      {/* Become a chef band */}
      <section className="border-t bg-gray-900">
        <div className="mx-auto max-w-5xl px-4 py-14">
          <div className="flex flex-col items-start justify-between gap-6 sm:flex-row sm:items-center">
            <div>
              <h2 className="text-2xl font-bold tracking-tight text-white sm:text-3xl">
                Turn your kitchen into a food business
              </h2>
              <p className="mt-2 max-w-lg text-sm text-gray-300">
                Create your profile, list your menu, add your location and let
                customers discover you — free while we grow.
              </p>
            </div>
            <Link
              href="/become-a-chef"
              className="shrink-0 rounded-xl bg-white px-6 py-3 text-sm font-semibold text-gray-900 transition hover:bg-orange-50"
            >
              Start your kitchen →
            </Link>
          </div>

          <ol className="mt-10 grid gap-4 sm:grid-cols-4">
            {[
              ["1", "Create your chef profile"],
              ["2", "Add your menu & prices"],
              ["3", "Set your location"],
              ["4", "Get discovered & ordered"],
            ].map(([step, label]) => (
              <li key={step} className="rounded-xl border border-gray-700 bg-gray-800/60 p-4">
                <span className="flex h-7 w-7 items-center justify-center rounded-full bg-orange-600 text-sm font-bold text-white">
                  {step}
                </span>
                <p className="mt-3 text-sm font-medium text-gray-100">{label}</p>
              </li>
            ))}
          </ol>
        </div>
      </section>
    </>
  );
}
