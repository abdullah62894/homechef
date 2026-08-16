import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
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
const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

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
        <header className="border-b">
          <div className="mx-auto flex h-14 max-w-5xl items-center px-4">
            <span className="text-lg font-semibold">HomeChef</span>
          </div>
        </header>
        <main className="flex-1">{children}</main>
        <footer className="border-t">
          <div className="mx-auto max-w-5xl px-4 py-6 text-sm text-gray-500">
            HomeChef — home-based food, discovered.
          </div>
        </footer>
      </body>
    </html>
  );
}