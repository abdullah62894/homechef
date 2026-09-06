import type { MetadataRoute } from "next";
import { apiUrl } from "@/lib/api";

export const revalidate = 3600;

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "https://frontend-delta-liart-66.vercel.app";

interface ChefRow {
  id: string;
  displayName: string;
  updatedAtUtc?: string;
}
interface FoodRow {
  id: string;
  name: string;
  chefProfileId: string;
}
interface LocationDirectory {
  data: {
    cities: { city: string; areas: { name: string }[] }[];
  } | null;
}

async function fetchJson<T>(path: string): Promise<T | null> {
  try {
    const res = await fetch(apiUrl(path), { next: { revalidate: 3600 } });
    if (!res.ok) return null;
    return (await res.json()) as T;
  } catch {
    return null;
  }
}

/**
 * Sitemap with only meaningful pages: static routes, chef kitchens,
 * dishes and location directories (Stage 13). Detail rows are capped so a
 * large database can never produce a bloated sitemap.
 */
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticEntries: MetadataRoute.Sitemap = [
    "",
    "/chefs",
    "/food",
    "/locations",
    "/search",
    "/become-a-chef",
  ].map((path) => ({
    url: `${SITE_URL}${path}`,
    lastModified: new Date(),
    changeFrequency: path === "" ? "daily" : "weekly",
    priority: path === "" ? 1 : 0.8,
  }));

  const [chefsPage, foodsPage, locations] = await Promise.all([
    fetchJson<{ data: ChefRow[] }>("/api/chefs?page=1&pageSize=200"),
    fetchJson<{ data: FoodRow[] }>("/api/foods?page=1&pageSize=500"),
    fetchJson<LocationDirectory>("/api/locations"),
  ]);

  const slug = (s: string) =>
    s.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-+|-+$/g, "");

  const chefEntries: MetadataRoute.Sitemap = (chefsPage?.data ?? []).slice(0, 200).map((chef) => ({
    url: `${SITE_URL}/chefs/${chef.id}-${slug(chef.displayName)}`,
    changeFrequency: "weekly",
    priority: 0.7,
  }));

  const foodEntries: MetadataRoute.Sitemap = (foodsPage?.data ?? []).slice(0, 500).map((food) => ({
    url: `${SITE_URL}/food/${food.id}-${slug(food.name)}`,
    changeFrequency: "weekly",
    priority: 0.6,
  }));

  const locationEntries: MetadataRoute.Sitemap = [];
  for (const entry of locations?.data?.cities ?? []) {
    const city = entry.city;
    locationEntries.push({
      url: `${SITE_URL}/locations/${encodeURIComponent(city.toLowerCase())}`,
      changeFrequency: "weekly",
      priority: 0.6,
    });
    for (const area of entry.areas ?? []) {
      locationEntries.push({
        url: `${SITE_URL}/locations/${encodeURIComponent(city.toLowerCase())}/${encodeURIComponent(area.name.toLowerCase())}`,
        changeFrequency: "weekly",
        priority: 0.5,
      });
    }
  }

  return [...staticEntries, ...chefEntries, ...foodEntries, ...locationEntries];
}
