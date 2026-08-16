import Link from "next/link";

export default function Home() {
  return (
    <section className="mx-auto max-w-5xl px-4 py-16 sm:py-24">
      <h1 className="max-w-3xl text-4xl font-bold tracking-tight sm:text-5xl text-gray-900">
        Discover home-based food chefs near you
      </h1>
      <p className="mt-4 max-w-2xl text-lg text-gray-600">
        HomeChef connects you with home cooks, bakers, and small independent
        food providers. Browse fresh homemade menus, see where they are, and support
        local home kitchens.
      </p>

      <div className="mt-8 flex flex-wrap items-center gap-4">
        <Link
          href="/food"
          className="rounded-lg bg-gray-900 px-5 py-3 text-sm font-medium text-white shadow-xs hover:bg-gray-800 transition"
        >
          Explore Food &amp; Menus →
        </Link>
        <Link
          href="/chefs"
          className="rounded-lg border border-gray-300 bg-white px-5 py-3 text-sm font-medium text-gray-700 hover:bg-gray-50 transition"
        >
          Browse Home Chefs
        </Link>
        <Link
          href="/search"
          className="rounded-lg border border-gray-300 bg-white px-5 py-3 text-sm font-medium text-gray-700 hover:bg-gray-50 transition"
        >
          Search Nearby
        </Link>
      </div>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {[
          {
            title: "Browse Home Chefs",
            href: "/chefs",
            description: "Explore talented home cooks and independent food providers in your city and neighborhood.",
          },
          {
            title: "Explore Dishes & Menus",
            href: "/food",
            description: "Discover fresh biryanis, gravies, customized bakery cakes, desserts, and specialties.",
          },
          {
            title: "Search & Discover",
            href: "/search",
            description: "Search for chefs and food by name, city, cuisine, or find nearby home kitchens.",
          },
          {
            title: "Browse Locations",
            href: "/locations",
            description: "Explore home chefs by city and neighborhood. Find verified kitchens near you.",
          },
        ].map((feature) => (
          <Link
            key={feature.title}
            href={feature.href}
            className="group block rounded-2xl border border-gray-200 bg-white p-6 shadow-xs hover:border-gray-300 hover:shadow-sm transition"
          >
            <h2 className="text-lg font-semibold text-gray-900 group-hover:underline">{feature.title}</h2>
            <p className="mt-2 text-sm text-gray-600">{feature.description}</p>
          </Link>
        ))}
      </div>
    </section>
  );
}