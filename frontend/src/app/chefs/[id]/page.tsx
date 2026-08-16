"use client";

import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { getChef, type ChefProfile } from "@/lib/chefs";
import { listChefFoods, type FoodListItem } from "@/lib/foods";
import {
  createChefReview,
  getChefRatingSummary,
  listChefReviews,
  type ChefRatingSummary,
  type Review,
} from "@/lib/reviews";
import {
  addChefFavorite,
  getUserFavoriteIds,
  removeChefFavorite,
} from "@/lib/favorites";
import { ApiError } from "@/lib/api";

type LoadState =
  | { status: "loading" }
  | { status: "error"; message: string }
  | {
      status: "ready";
      chef: ChefProfile;
      foods: FoodListItem[];
      reviews: Review[];
      summary: ChefRatingSummary;
    };

export default function ChefDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [state, setState] = useState<LoadState>({ status: "loading" });
  const [isFavorited, setIsFavorited] = useState(false);
  const [togglingFav, setTogglingFav] = useState(false);

  // Review Form state
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [submittingReview, setSubmittingReview] = useState(false);
  const [reviewError, setReviewError] = useState<string | null>(null);
  const [reviewSuccess, setReviewSuccess] = useState<string | null>(null);

  const loadData = useCallback(() => {
    return Promise.all([
      getChef(id),
      listChefFoods(id, undefined, 1, 50),
      listChefReviews(id, 1, 50),
      getChefRatingSummary(id),
    ]);
  }, [id]);

  useEffect(() => {
    let cancelled = false;
    loadData()
      .then(([chef, foodsPage, reviewsPage, summary]) => {
        if (!cancelled) {
          setState({
            status: "ready",
            chef,
            foods: foodsPage.items,
            reviews: reviewsPage.items,
            summary,
          });
        }
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setState({
          status: "error",
          message: err instanceof ApiError ? err.message : "Unable to load this chef.",
        });
      });

    // Check initial favorite status
    getUserFavoriteIds()
      .then((ids) => {
        if (!cancelled) {
          setIsFavorited(ids.chefIds.includes(id));
        }
      })
      .catch(() => {
        // Unauthenticated or error
      });

    return () => {
      cancelled = true;
    };
  }, [id, loadData]);

  async function handleToggleFavorite() {
    setTogglingFav(true);
    try {
      if (isFavorited) {
        await removeChefFavorite(id);
        setIsFavorited(false);
      } else {
        await addChefFavorite(id);
        setIsFavorited(true);
      }
    } catch {
      // Ignored
    } finally {
      setTogglingFav(false);
    }
  }

  async function handleReviewSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setReviewError(null);
    setReviewSuccess(null);
    setSubmittingReview(true);

    try {
      await createChefReview(id, {
        rating,
        comment: comment.trim(),
      });
      setReviewSuccess("Thank you! Your review has been published.");
      setComment("");
      setRating(5);

      // Refresh reviews and summary
      const [reviewsPage, summary] = await Promise.all([
        listChefReviews(id, 1, 50),
        getChefRatingSummary(id),
      ]);

      setState((prev) => {
        if (prev.status !== "ready") return prev;
        return {
          ...prev,
          reviews: reviewsPage.items,
          summary,
        };
      });
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        if (err.status === 401) {
          setReviewError("Please sign in as a customer to leave a review.");
        } else {
          setReviewError(err.message);
        }
      } else {
        setReviewError("Failed to submit your review. Please try again.");
      }
    } finally {
      setSubmittingReview(false);
    }
  }

  if (state.status === "loading") {
    return <section className="mx-auto max-w-5xl px-4 py-16 text-gray-600">Loading chef…</section>;
  }

  if (state.status === "error") {
    return (
      <section className="mx-auto max-w-5xl px-4 py-16">
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {state.message}
        </div>
        <Link href="/chefs" className="mt-6 inline-block text-sm text-gray-600 underline">
          Back to chefs
        </Link>
      </section>
    );
  }

  const { chef, foods, reviews, summary } = state;

  return (
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <Link href="/chefs" className="text-sm text-gray-600 hover:text-gray-900 underline">
        ← Back to all chefs
      </Link>

      {/* Chef Profile Header */}
      <div className="mt-6 rounded-2xl border border-gray-200 bg-white p-6 sm:p-8">
        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">{chef.displayName}</h1>
            <p className="mt-2 text-base text-gray-600">
              📍 {chef.city}
              {chef.area ? `, ${chef.area}` : ""}
              {chef.address ? ` • ${chef.address}` : ""}
            </p>
          </div>

          {/* Actions & Rating Badge */}
          <div className="flex flex-wrap items-center gap-3 self-start">
            <button
              type="button"
              onClick={handleToggleFavorite}
              disabled={togglingFav}
              className={`flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-xs font-semibold border transition ${
                isFavorited
                  ? "bg-red-50 text-red-600 border-red-200 hover:bg-red-100"
                  : "bg-gray-50 text-gray-700 border-gray-200 hover:bg-gray-100"
              }`}
            >
              <span>{isFavorited ? "♥" : "♡"}</span>
              <span>{isFavorited ? "Favorited" : "Favorite Kitchen"}</span>
            </button>

            <div className="flex items-center gap-2 rounded-xl border border-gray-200 bg-gray-50 px-4 py-2">
              <span className="text-xl font-bold text-gray-900">
                {summary.totalReviews > 0 ? summary.averageRating.toFixed(1) : "New"}
              </span>
              <span className="text-amber-400 text-lg">★</span>
              <span className="text-xs text-gray-500 font-medium">
                ({summary.totalReviews} {summary.totalReviews === 1 ? "review" : "reviews"})
              </span>
            </div>
          </div>
        </div>

        {chef.cuisines.length > 0 && (
          <div className="mt-4 flex flex-wrap gap-1.5">
            {chef.cuisines.map((cuisine) => (
              <span
                key={cuisine}
                className="rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-700"
              >
                {cuisine}
              </span>
            ))}
          </div>
        )}

        <p className="mt-6 text-base text-gray-700 leading-relaxed whitespace-pre-line">{chef.bio}</p>
      </div>

      {/* Chef's Food / Menu Items */}
      <div className="mt-12">
        <div className="flex items-center justify-between">
          <h2 className="text-2xl font-bold tracking-tight text-gray-900">Menu &amp; Dishes</h2>
          <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
            {foods.length} {foods.length === 1 ? "Item" : "Items"}
          </span>
        </div>

        {foods.length === 0 ? (
          <div className="mt-6 rounded-xl border border-dashed border-gray-300 py-12 text-center text-sm text-gray-500">
            This chef hasn&apos;t published any dishes yet. Check back soon!
          </div>
        ) : (
          <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {foods.map((food) => (
              <div
                key={food.id}
                className="flex flex-col justify-between rounded-xl border border-gray-200 bg-white p-5 shadow-xs hover:border-gray-300 transition"
              >
                <div>
                  <div className="flex items-center justify-between gap-2">
                    <span className="rounded bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                      {food.categoryName ?? "Dish"}
                    </span>
                    <span
                      className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${
                        food.isAvailable
                          ? "bg-emerald-50 text-emerald-700 border border-emerald-200"
                          : "bg-gray-100 text-gray-500"
                      }`}
                    >
                      {food.isAvailable ? "Available" : "Sold out"}
                    </span>
                  </div>

                  <h3 className="mt-3 text-lg font-semibold text-gray-900">
                    <Link href={`/food/${food.id}`} className="hover:underline">
                      {food.name}
                    </Link>
                  </h3>

                  <p className="mt-1 text-sm text-gray-600 line-clamp-2">{food.description}</p>
                </div>

                <div className="mt-4 flex items-center justify-between border-t border-gray-100 pt-3">
                  <span className="text-base font-bold text-gray-900">
                    {food.currency} {food.price.toLocaleString()}
                  </span>
                  <Link
                    href={`/food/${food.id}`}
                    className="rounded-lg bg-gray-50 px-3 py-1 text-xs font-medium text-gray-700 hover:bg-gray-100 transition"
                  >
                    Details →
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Ratings & Reviews Section */}
      <div className="mt-16 border-t border-gray-200 pt-12">
        <h2 className="text-2xl font-bold tracking-tight text-gray-900">Customer Ratings &amp; Reviews</h2>

        <div className="mt-6 grid gap-8 lg:grid-cols-3">
          {/* Rating Summary Breakdown */}
          <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-xs">
            <div className="text-center">
              <div className="text-5xl font-extrabold text-gray-900">
                {summary.totalReviews > 0 ? summary.averageRating.toFixed(1) : "—"}
              </div>
              <div className="mt-2 flex justify-center text-amber-400 text-xl">
                {[1, 2, 3, 4, 5].map((star) => (
                  <span key={star}>
                    {summary.totalReviews > 0 && star <= Math.round(summary.averageRating) ? "★" : "☆"}
                  </span>
                ))}
              </div>
              <p className="mt-1 text-xs text-gray-500">Based on {summary.totalReviews} customer reviews</p>
            </div>

            <div className="mt-6 space-y-2 border-t border-gray-100 pt-4 text-xs">
              {[5, 4, 3, 2, 1].map((starCount) => {
                const count = summary.ratingDistribution[starCount] ?? 0;
                const percentage = summary.totalReviews > 0 ? Math.round((count / summary.totalReviews) * 100) : 0;
                return (
                  <div key={starCount} className="flex items-center gap-2">
                    <span className="w-8 font-medium text-gray-700">{starCount} ★</span>
                    <div className="h-2 flex-1 rounded-full bg-gray-100 overflow-hidden">
                      <div
                        className="h-full bg-amber-400 rounded-full transition-all duration-300"
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                    <span className="w-8 text-right text-gray-400">{count}</span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Write a Review Form */}
          <div className="lg:col-span-2 rounded-2xl border border-gray-200 bg-white p-6 shadow-xs">
            <h3 className="text-lg font-semibold text-gray-900">Leave a Review</h3>
            <p className="mt-1 text-xs text-gray-500">
              Share your experience with this home chef and help the community.
            </p>

            <form onSubmit={handleReviewSubmit} className="mt-4 space-y-4">
              {reviewError && (
                <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-xs text-red-700">
                  {reviewError}
                </div>
              )}
              {reviewSuccess && (
                <div className="rounded-lg border border-green-200 bg-green-50 p-3 text-xs text-green-700">
                  {reviewSuccess}
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-gray-700 uppercase tracking-wider">
                  Your Rating
                </label>
                <div className="mt-1.5 flex gap-1">
                  {[1, 2, 3, 4, 5].map((val) => (
                    <button
                      key={val}
                      type="button"
                      onClick={() => setRating(val)}
                      className={`text-2xl transition hover:scale-110 focus:outline-none ${
                        val <= rating ? "text-amber-400" : "text-gray-300"
                      }`}
                    >
                      ★
                    </button>
                  ))}
                  <span className="ml-2 self-center text-xs font-medium text-gray-600">
                    {rating} of 5 stars
                  </span>
                </div>
              </div>

              <div>
                <label htmlFor="comment" className="block text-xs font-semibold text-gray-700 uppercase tracking-wider">
                  Review &amp; Feedback
                </label>
                <textarea
                  id="comment"
                  required
                  minLength={3}
                  maxLength={1000}
                  rows={3}
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="How was the food, taste, packaging, and timeliness?"
                  className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-xs focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
                />
              </div>

              <button
                type="submit"
                disabled={submittingReview}
                className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white shadow-xs hover:bg-gray-800 transition disabled:opacity-50"
              >
                {submittingReview ? "Submitting…" : "Post Review"}
              </button>
            </form>
          </div>
        </div>

        {/* Customer Reviews List */}
        <div className="mt-8 space-y-4">
          <h3 className="text-lg font-semibold text-gray-900">
            Reviews ({reviews.length})
          </h3>

          {reviews.length === 0 ? (
            <div className="rounded-xl border border-dashed border-gray-200 py-8 text-center text-sm text-gray-500">
              No reviews yet. Be the first to leave feedback for this chef!
            </div>
          ) : (
            reviews.map((rev) => (
              <div key={rev.id} className="rounded-xl border border-gray-200 bg-white p-5 shadow-xs">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-amber-400 text-sm">
                      {"★".repeat(rev.rating)}
                      {"☆".repeat(5 - rev.rating)}
                    </span>
                    <span className="text-sm font-semibold text-gray-900">{rev.customerName}</span>
                  </div>
                  <span className="text-xs text-gray-400">
                    {new Date(rev.createdAtUtc).toLocaleDateString(undefined, {
                      year: "numeric",
                      month: "short",
                      day: "numeric",
                    })}
                  </span>
                </div>
                <p className="mt-2 text-sm text-gray-700 leading-relaxed">{rev.comment}</p>
              </div>
            ))
          )}
        </div>
      </div>
    </section>
  );
}
