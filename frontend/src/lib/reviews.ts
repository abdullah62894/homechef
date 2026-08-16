import { apiFetch, type ApiEnvelope } from "./api";

export interface Review {
  id: string;
  chefProfileId: string;
  customerUserId: string;
  customerName: string;
  rating: number;
  comment: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ChefRatingSummary {
  chefProfileId: string;
  averageRating: number;
  totalReviews: number;
  ratingDistribution: Record<number, number>;
}

export interface CreateReviewInput {
  rating: number;
  comment: string;
}

export interface UpdateReviewInput {
  rating: number;
  comment: string;
}

export interface ReviewListPage {
  items: Review[];
  page: number;
  pageSize: number;
  total: number;
  hasMore: boolean;
}

interface ReviewListEnvelope extends ApiEnvelope<Review[]> {
  meta: {
    page: number;
    pageSize: number;
    total: number;
    hasMore: boolean;
  } | null;
}

async function unwrap<T>(envelope: ApiEnvelope<T>): Promise<T> {
  return envelope.data;
}

export function listChefReviews(
  chefId: string,
  page = 1,
  pageSize = 20
): Promise<ReviewListPage> {
  const params = new URLSearchParams();
  params.set("page", page.toString());
  params.set("pageSize", pageSize.toString());

  return apiFetch<ReviewListEnvelope>(`/api/chefs/${chefId}/reviews?${params.toString()}`).then(
    (envelope) => ({
      items: envelope.data,
      page: envelope.meta?.page ?? page,
      pageSize: envelope.meta?.pageSize ?? pageSize,
      total: envelope.meta?.total ?? envelope.data.length,
      hasMore: envelope.meta?.hasMore ?? false,
    })
  );
}

export function getChefRatingSummary(chefId: string): Promise<ChefRatingSummary> {
  return apiFetch<ApiEnvelope<ChefRatingSummary>>(`/api/chefs/${chefId}/reviews/summary`).then(unwrap);
}

export function createChefReview(chefId: string, input: CreateReviewInput): Promise<Review> {
  return apiFetch<ApiEnvelope<Review>>(`/api/chefs/${chefId}/reviews`, {
    method: "POST",
    body: input,
  }).then(unwrap);
}

export function updateReview(reviewId: string, input: UpdateReviewInput): Promise<Review> {
  return apiFetch<ApiEnvelope<Review>>(`/api/reviews/${reviewId}`, {
    method: "PUT",
    body: input,
  }).then(unwrap);
}

export function deleteReview(reviewId: string): Promise<void> {
  return apiFetch<void>(`/api/reviews/${reviewId}`, {
    method: "DELETE",
  });
}
