import type { Metadata } from "next";
import type { ReactNode } from "react";

export const metadata: Metadata = {
  title: "Browse home chefs by city and area",
  description:
    "Discover home-based food chefs across Pakistan — explore kitchens by city and neighborhood, from Islamabad and Lahore to Karachi.",
  alternates: { canonical: "/locations" },
};

export default function LocationsLayout({ children }: { children: ReactNode }) {
  return children;
}
