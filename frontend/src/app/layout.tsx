import type { Metadata } from "next";
import Link from "next/link";
import { Geist, Geist_Mono } from "next/font/google";
import SiteHeader from "@/components/SiteHeader";
import { CartProvider } from "@/components/CartProvider";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

const SITE_NAME = "HomeChef";
const rawSiteUrl = process.env.NEXT_PUBLIC_SITE_URL;
const SITE_URL = rawSiteUrl && /^https?:\/\//.test(rawSiteUrl) ? rawSiteUrl : "http://localhost:3000";

export const metadata: Metadata = {
  metadataBase: new URL(SITE_URL),
  title: {
    default: "HomeChef — Discover Home-Based Food Chefs",
    template: "%s | HomeChef",
  },
  description:
    "Discover home-based food chefs and small independent food providers, browse their menus, view locations, and leave ratings and reviews.",
  applicationName: SITE_NAME,
  keywords: ["home chefs", "home cooking", "food", "local food", "menu", "restaurant"],
  openGraph: {
    type: "website",
    siteName: SITE_NAME,
    title: "HomeChef — Discover Home-Based Food Chefs",
    description:
      "Discover home-based food chefs and small independent food providers, browse their menus, view locations, and leave ratings and reviews.",
    url: SITE_URL,
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "HomeChef — Discover Home-Based Food Chefs",
    description:
      "Discover home-based food chefs and small independent food providers, browse their menus, view locations, and leave ratings and reviews.",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col">
        <CartProvider>
          <SiteHeader />
          <main className="flex-1">{children}</main>
          <footer className="border-t bg-gray-50">
          <div className="mx-auto max-w-5xl px-4 py-10">
            <div className="grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
              <div>
                <h3 className="text-sm font-semibold text-gray-900">HomeChef</h3>
                <p className="mt-2 text-xs text-gray-500 leading-relaxed">
                  Discover homemade food from trusted home chefs in your city. Fresh, authentic, and made to order.
                </p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-900">Explore</h3>
                <ul className="mt-2 space-y-1.5">
                  <li><Link href="/cuisines" className="text-xs text-gray-500 hover:text-gray-900">Cuisines</Link></li>
                  <li><Link href="/food" className="text-xs text-gray-500 hover:text-gray-900">Browse Dishes</Link></li>
                  <li><Link href="/chefs" className="text-xs text-gray-500 hover:text-gray-900">Find Chefs</Link></li>
                  <li><Link href="/locations" className="text-xs text-gray-500 hover:text-gray-900">Locations</Link></li>
                  <li><Link href="/search" className="text-xs text-gray-500 hover:text-gray-900">Search</Link></li>
                </ul>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-900">For Chefs</h3>
                <ul className="mt-2 space-y-1.5">
                  <li><Link href="/become-a-chef" className="text-xs text-gray-500 hover:text-gray-900">Become a Chef</Link></li>
                  <li><Link href="/register?role=Chef" className="text-xs text-gray-500 hover:text-gray-900">Chef Registration</Link></li>
                </ul>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-900">Trust &amp; Safety</h3>
                <ul className="mt-2 space-y-1.5">
                  <li><Link href="/contact" className="text-xs text-gray-500 hover:text-gray-900">Contact Us</Link></li>
                  <li className="flex items-center gap-1.5 text-xs text-gray-500">
                    <svg className="h-3 w-3 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    Verified Chefs
                  </li>
                  <li className="flex items-center gap-1.5 text-xs text-gray-500">
                    <svg className="h-3 w-3 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    Customer Reviews
                  </li>
                  <li className="flex items-center gap-1.5 text-xs text-gray-500">
                    <svg className="h-3 w-3 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    Direct Messaging
                  </li>
                </ul>
              </div>
            </div>
            <div className="mt-8 border-t border-gray-200 pt-6 text-center text-xs text-gray-400">
              HomeChef — home-based food, discovered.
            </div>
          </div>
          </footer>
        </CartProvider>
      </body>
    </html>
  );
}