import type { Metadata } from "next";
import Link from "next/link";
import BecomeChefCTA from "@/components/BecomeChefCTA";

export const metadata: Metadata = {
  title: "Become a Home Chef — sell your homemade food",
  description:
    "Turn your home kitchen into a food business on HomeChef. Create your profile, list your menu, set your location and get discovered by hungry customers nearby.",
  alternates: { canonical: "/become-a-chef" },
};

const STEPS = [
  {
    title: "Create your chef profile",
    body: "Tell customers who you are: your name, your city and neighborhood, and the cuisines you love to cook.",
  },
  {
    title: "Add your menu & prices",
    body: "List your dishes with photos, descriptions and prices. Toggle availability anytime — sold out biryani at 9pm? One tap.",
  },
  {
    title: "Add your location",
    body: "Set your area or exact coordinates so nearby customers find you first when they search 'food near me'.",
  },
  {
    title: "Get discovered & contacted",
    body: "Customers browse your kitchen, read reviews, favorite their finds and message you directly to order.",
  },
];

export default function BecomeAChefPage() {
  return (
    <section className="mx-auto max-w-3xl px-4 py-12 sm:py-16">
      <nav aria-label="Breadcrumb" className="text-sm text-gray-500">
        <Link href="/" className="hover:underline">
          Home
        </Link>
        <span className="mx-1.5">/</span>
        <span className="text-gray-900">Become a Home Chef</span>
      </nav>

      <h1 className="mt-4 text-4xl font-bold tracking-tight text-gray-900">
        Turn your kitchen into a food business
      </h1>
      <p className="mt-3 max-w-xl text-lg text-gray-600">
        HomeChef gives home cooks, bakers and small food makers a storefront —
        no restaurant, no commission, no setup fee while we grow.
      </p>

      <BecomeChefCTA />

      <ol className="mt-12 space-y-5">
        {STEPS.map((step, i) => (
          <li key={step.title} className="flex gap-4 rounded-2xl border border-gray-200 bg-white p-5 shadow-xs">
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gray-900 text-sm font-bold text-white">
              {i + 1}
            </span>
            <div>
              <h2 className="font-semibold text-gray-900">{step.title}</h2>
              <p className="mt-1 text-sm leading-relaxed text-gray-600">{step.body}</p>
            </div>
          </li>
        ))}
      </ol>

      <div className="mt-10 rounded-2xl bg-gray-900 p-6 text-center">
        <p className="text-lg font-semibold text-white">Ready to cook for your neighborhood?</p>
        <BecomeChefCTA variant="light" />
      </div>
    </section>
  );
}
