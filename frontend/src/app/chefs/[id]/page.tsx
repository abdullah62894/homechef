import type { Metadata } from "next";
import { notFound } from "next/navigation";
import ChefDetailView from "./ChefDetailView";
import { apiUrl } from "@/lib/api";
import { parseIdSlug, chefHref } from "@/lib/slugs";

interface ChefApiRow {
  data?: {
    id: string;
    displayName: string;
    bio: string;
    city: string;
    area: string | null;
    cuisines: string[];
    photoUrl: string | null;
  };
}

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "https://frontend-delta-liart-66.vercel.app";

async function fetchChef(id: string): Promise<ChefApiRow["data"] | null> {
  try {
    const res = await fetch(apiUrl(`/api/chefs/${id}`), { next: { revalidate: 120 } });
    if (!res.ok) return null;
    return ((await res.json()) as ChefApiRow).data ?? null;
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
  const chef = await fetchChef(id);
  if (!chef) return { title: "Chef not found" };

  const path = chefHref(chef.id, chef.displayName);
  const title = `${chef.displayName} — home food in ${chef.city}`;
  const description =
    `Order homemade food from ${chef.displayName}${chef.area ? ` in ${chef.area}, ${chef.city}` : ` in ${chef.city}`}. ` +
    (chef.cuisines.length > 0 ? `${chef.cuisines.join(", ")} — ` : "") +
    "menu, reviews and direct contact on HomeChef.";
  const images = chef.photoUrl ? [{ url: `${apiUrl(chef.photoUrl)}` }] : undefined;

  return {
    title,
    description,
    alternates: { canonical: path },
    openGraph: {
      title,
      description,
      url: `${SITE_URL}${path}`,
      siteName: "HomeChef",
      type: "profile",
      images,
    },
    twitter: {
      card: chef.photoUrl ? "summary_large_image" : "summary",
      title,
      description,
    },
  };
}

export default async function ChefDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id: raw } = await params;
  const id = parseIdSlug(raw);
  if (!id) notFound();

  const chef = await fetchChef(id);

  const jsonLd = chef
    ? {
        "@context": "https://schema.org",
        "@type": "Restaurant",
        name: chef.displayName,
        description: chef.bio,
        servesCuisine: chef.cuisines,
        address: {
          "@type": "PostalAddress",
          addressLocality: chef.city,
          addressRegion: chef.area ?? undefined,
        },
        url: `${SITE_URL}${chefHref(chef.id, chef.displayName)}`,
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
      <ChefDetailView id={id} />
    </>
  );
}
