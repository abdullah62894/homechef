import type { Metadata } from "next";
import { notFound } from "next/navigation";
import FoodDetailView from "./FoodDetailView";
import { apiUrl } from "@/lib/api";
import { parseIdSlug, foodHref } from "@/lib/slugs";

interface FoodApiRow {
  data?: {
    id: string;
    name: string;
    description: string;
    price: number;
    currency: string;
    chefProfileId: string;
    chefDisplayName: string;
    chefCity: string;
    imageUrl: string | null;
  };
}

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "https://frontend-delta-liart-66.vercel.app";

async function fetchFood(id: string): Promise<FoodApiRow["data"] | null> {
  try {
    const res = await fetch(apiUrl(`/api/foods/${id}`), { next: { revalidate: 120 } });
    if (!res.ok) return null;
    return ((await res.json()) as FoodApiRow).data ?? null;
  } catch {
    return null;
  }
}

export async function generateMetadata({
  params,
}: {
  params: Promise<{ id: string }>;
}): Promise<Metadata> {
  const { id: raw } = await params;
  const id = parseIdSlug(raw);
  if (!id) return {};
  const food = await fetchFood(id);
  if (!food) return { title: "Dish not found" };

  const path = foodHref(food.id, food.name);
  const title = `${food.name} by ${food.chefDisplayName} — ${food.currency} ${food.price.toLocaleString()}`;
  const description =
    `${food.description.slice(0, 140)} — homemade ${food.name} by ${food.chefDisplayName} in ${food.chefCity}. Order directly on HomeChef.`;
  const images = food.imageUrl ? [{ url: apiUrl(food.imageUrl) }] : undefined;

  return {
    title,
    description,
    alternates: { canonical: path },
    openGraph: {
      title,
      description,
      url: `${SITE_URL}${path}`,
      siteName: "HomeChef",
      type: "article",
      images,
    },
    twitter: {
      card: food.imageUrl ? "summary_large_image" : "summary",
      title,
      description,
    },
  };
}

export default async function FoodDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id: raw } = await params;
  const id = parseIdSlug(raw);
  if (!id) notFound();

  const food = await fetchFood(id);

  const jsonLd = food
    ? {
        "@context": "https://schema.org",
        "@type": "Product",
        name: food.name,
        description: food.description,
        image: food.imageUrl ? apiUrl(food.imageUrl) : undefined,
        offers: {
          "@type": "Offer",
          price: food.price,
          priceCurrency: food.currency,
          availability: "https://schema.org/InStock",
        },
      }
    : null;

  return (
    <>
      {jsonLd && (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
        />
      )}
      <FoodDetailView id={id} />
    </>
  );
}
