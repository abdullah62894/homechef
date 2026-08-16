export default function Home() {
  return (
    <section className="mx-auto max-w-5xl px-4 py-16 sm:py-24">
      <h1 className="max-w-3xl text-4xl font-bold tracking-tight sm:text-5xl">
        Discover home-based food chefs near you
      </h1>
      <p className="mt-4 max-w-2xl text-lg text-gray-600">
        HomeChef connects you with home cooks, bakers, and small independent
        food providers. Browse their menus, see where they are, and support
        local kitchens.
      </p>

      <div className="mt-10 grid gap-4 sm:grid-cols-3">
        {[
          {
            title: "Browse chefs",
            description: "Explore home chefs and food providers in your city and area.",
          },
          {
            title: "View menus",
            description: "See what each chef offers, with availability and details.",
          },
          {
            title: "Ratings & reviews",
            description: "Read and leave reviews so you can order with confidence.",
          },
        ].map((feature) => (
          <div
            key={feature.title}
            className="rounded-xl border border-gray-200 p-6"
          >
            <h2 className="text-lg font-semibold">{feature.title}</h2>
            <p className="mt-2 text-sm text-gray-600">{feature.description}</p>
          </div>
        ))}
      </div>

      <p className="mt-10 text-sm text-gray-400">
        Chefs and menus are coming soon.
      </p>
    </section>
  );
}