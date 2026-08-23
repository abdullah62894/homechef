/**
 * SEO-friendly URL helpers (Stage 13). Detail routes accept
 * `/{guid}-{slug}` — the page parses the leading guid — so old
 * `/{guid}` links keep working while new links carry readable slugs.
 */
export function slugify(text: string): string {
  return text
    .toLowerCase()
    .normalize("NFKD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 60)
    .replace(/-+$/g, "");
}

export function chefHref(id: string, displayName?: string | null): string {
  const slug = displayName ? slugify(displayName) : "";
  return `/chefs/${slug ? `${id}-${slug}` : id}`;
}

export function foodHref(id: string, name?: string | null): string {
  const slug = name ? slugify(name) : "";
  return `/food/${slug ? `${id}-${slug}` : id}`;
}

/** Extracts the leading guid from a `/{guid}` or `/{guid}-{slug}` param. */
export function parseIdSlug(param: string): string | null {
  const match = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i.exec(param);
  return match ? match[0] : null;
}
