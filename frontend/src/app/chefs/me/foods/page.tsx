"use client";

import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  listMyFoods,
  listFoodCategories,
  createFoodItem,
  updateFoodItem,
  deleteFoodItem,
  toggleFoodAvailability,
  type FoodListItem,
  type FoodCategory,
  type FoodItemInput,
} from "@/lib/foods";
import {
  uploadFoodImage,
  clearFoodImage,
  resolveImageUrl,
  validateImageFile,
} from "@/lib/images";
import { ApiError } from "@/lib/api";

const initialForm: {
  name: string;
  description: string;
  price: string;
  categoryId: string;
  preparationTimeMinutes: string;
  isAvailable: boolean;
} = {
  name: "",
  description: "",
  price: "",
  categoryId: "",
  preparationTimeMinutes: "",
  isAvailable: true,
};

export default function ChefManageFoodsPage() {
  const router = useRouter();
  const [foods, setFoods] = useState<FoodListItem[]>([]);
  const [categories, setCategories] = useState<FoodCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Modal / Form state
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingFoodId, setEditingFoodId] = useState<string | null>(null);
  const [formData, setFormData] = useState(initialForm);
  const [submitting, setSubmitting] = useState(false);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [removeImage, setRemoveImage] = useState(false);

  const loadData = useCallback(async () => {
    const [foodsPage, cats] = await Promise.all([listMyFoods(1, 100), listFoodCategories()]);
    return { foods: foodsPage.items, categories: cats };
  }, []);

  useEffect(() => {
    let cancelled = false;

    loadData()
      .then((result) => {
        if (cancelled) return;
        setFoods(result.foods);
        setCategories(result.categories);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 401) {
          router.replace("/login");
          return;
        }
        if (err instanceof ApiError && err.status === 404) {
          router.replace("/chefs/me");
          return;
        }
        setError(err instanceof ApiError ? err.message : "Unable to load your dishes.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [router, loadData]);

  const handleOpenAdd = () => {
    setEditingFoodId(null);
    setFormData(initialForm);
    setImageFile(null);
    setRemoveImage(false);
    setError(null);
    setSuccess(null);
    setIsFormOpen(true);
  };

  const handleOpenEdit = (food: FoodListItem) => {
    setEditingFoodId(food.id);
    setFormData({
      name: food.name,
      description: food.description,
      price: food.price.toString(),
      categoryId: food.categoryId ?? "",
      preparationTimeMinutes: food.preparationTimeMinutes?.toString() ?? "",
      isAvailable: food.isAvailable,
    });
    setImageFile(null);
    setRemoveImage(false);
    setError(null);
    setSuccess(null);
    setIsFormOpen(true);
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setEditingFoodId(null);
    setFormData(initialForm);
    setImageFile(null);
    setRemoveImage(false);
  };

  function handleImageChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;

    const validationError = validateImageFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setImageFile(file);
    setRemoveImage(false);
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    setSubmitting(true);

    const priceNum = parseFloat(formData.price);
    if (isNaN(priceNum) || priceNum <= 0) {
      setError("Please enter a valid price greater than 0.");
      setSubmitting(false);
      return;
    }

    const prepTimeNum = formData.preparationTimeMinutes.trim()
      ? parseInt(formData.preparationTimeMinutes.trim(), 10)
      : null;

    const input: FoodItemInput = {
      name: formData.name.trim(),
      description: formData.description.trim(),
      price: priceNum,
      categoryId: formData.categoryId ? formData.categoryId : null,
      preparationTimeMinutes: prepTimeNum,
      isAvailable: formData.isAvailable,
    };

    try {
      let foodId = editingFoodId;
      if (editingFoodId) {
        await updateFoodItem(editingFoodId, input);
        setSuccess("Dish updated successfully!");
      } else {
        const created = await createFoodItem(input);
        foodId = created.id;
        setSuccess("Dish added successfully to your menu!");
      }

      if (foodId && imageFile) {
        await uploadFoodImage(foodId, imageFile);
      } else if (foodId && removeImage) {
        await clearFoodImage(foodId);
      }

      handleCloseForm();
      const result = await loadData();
      setFoods(result.foods);
      setCategories(result.categories);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save dish. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (foodId: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete "${name}"?`)) {
      return;
    }

    setError(null);
    setSuccess(null);

    try {
      await deleteFoodItem(foodId);
      setFoods((prev) => prev.filter((f) => f.id !== foodId));
      setSuccess(`"${name}" was deleted.`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete dish.");
    }
  };

  const handleToggleAvailability = async (food: FoodListItem) => {
    const nextState = !food.isAvailable;
    try {
      await toggleFoodAvailability(food.id, nextState);
      setFoods((prev) =>
        prev.map((f) => (f.id === food.id ? { ...f, isAvailable: nextState } : f))
      );
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update availability.");
    }
  };

  if (loading) {
    return (
      <section className="mx-auto max-w-5xl px-4 py-16 text-gray-600">
        Loading your menu…
      </section>
    );
  }

  return (
    <section className="mx-auto max-w-5xl px-4 py-12 sm:py-16">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
            <Link href="/chefs/me" className="hover:underline">
              Chef Profile
            </Link>
            <span>/</span>
            <span className="text-gray-900 font-medium">Menu Management</span>
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">Manage Your Menu</h1>
          <p className="mt-1 text-sm text-gray-600">
            Add, update, or toggle the availability of your delicious dishes.
          </p>
        </div>

        <button
          type="button"
          onClick={handleOpenAdd}
          className="inline-flex items-center justify-center rounded-lg bg-gray-900 px-4 py-2.5 text-sm font-medium text-white hover:bg-gray-800 shadow-xs transition"
        >
          + Add New Dish
        </button>
      </div>

      {error && (
        <div className="mt-6 rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          {error}
        </div>
      )}

      {success && (
        <div className="mt-6 rounded-lg border border-green-200 bg-green-50 p-4 text-sm text-green-700">
          {success}
        </div>
      )}

      {/* Food items list */}
      <div className="mt-8">
        {foods.length === 0 ? (
          <div className="rounded-xl border border-dashed border-gray-300 py-16 text-center">
            <h3 className="text-lg font-medium text-gray-900">No dishes on your menu yet</h3>
            <p className="mt-1 text-sm text-gray-500 max-w-md mx-auto">
              Start building your menu so hungry customers can discover your specialties and order.
            </p>
            <button
              type="button"
              onClick={handleOpenAdd}
              className="mt-6 inline-flex items-center rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800"
            >
              Add Your First Dish
            </button>
          </div>
        ) : (
          <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-xs">
            <table className="min-w-full divide-y divide-gray-200 text-left text-sm">
              <thead className="bg-gray-50 text-xs font-semibold uppercase tracking-wider text-gray-500">
                <tr>
                  <th scope="col" className="px-6 py-3.5">
                    Dish
                  </th>
                  <th scope="col" className="px-6 py-3.5">
                    Category
                  </th>
                  <th scope="col" className="px-6 py-3.5">
                    Price
                  </th>
                  <th scope="col" className="px-6 py-3.5 text-center">
                    Availability
                  </th>
                  <th scope="col" className="px-6 py-3.5 text-right">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {foods.map((food) => (
                  <tr key={food.id} className="hover:bg-gray-50/70 transition">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        {resolveImageUrl(food.imageThumbnailUrl ?? food.imageUrl) && (
                          // eslint-disable-next-line @next/next/no-img-element
                          <img
                            src={resolveImageUrl(food.imageThumbnailUrl ?? food.imageUrl) ?? ""}
                            alt={food.name}
                            className="h-12 w-12 rounded-lg border border-gray-200 object-cover"
                          />
                        )}
                        <div>
                          <div className="font-semibold text-gray-900">{food.name}</div>
                          <div className="text-xs text-gray-500 line-clamp-1 mt-0.5">
                            {food.description}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-gray-600">
                      <span className="rounded bg-gray-100 px-2 py-0.5 text-xs font-medium">
                        {food.categoryName ?? "General"}
                      </span>
                    </td>
                    <td className="px-6 py-4 font-semibold text-gray-900 whitespace-nowrap">
                      {food.currency} {food.price.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 text-center whitespace-nowrap">
                      <button
                        type="button"
                        onClick={() => handleToggleAvailability(food)}
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold transition cursor-pointer ${
                          food.isAvailable
                            ? "bg-emerald-50 text-emerald-700 border border-emerald-200 hover:bg-emerald-100"
                            : "bg-gray-100 text-gray-500 border border-gray-200 hover:bg-gray-200"
                        }`}
                      >
                        {food.isAvailable ? "● Available" : "○ Sold out"}
                      </button>
                    </td>
                    <td className="px-6 py-4 text-right space-x-2 whitespace-nowrap">
                      <button
                        type="button"
                        onClick={() => handleOpenEdit(food)}
                        className="font-medium text-gray-700 hover:text-gray-900 underline text-xs"
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        onClick={() => handleDelete(food.id, food.name)}
                        className="font-medium text-red-600 hover:text-red-800 underline text-xs"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal / Overlay for Add / Edit */}
      {isFormOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl sm:p-8 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between border-b pb-3">
              <h2 className="text-xl font-bold text-gray-900">
                {editingFoodId ? "Edit Dish" : "Add New Dish"}
              </h2>
              <button
                type="button"
                onClick={handleCloseForm}
                className="text-gray-400 hover:text-gray-600 font-bold"
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSubmit} className="mt-6 space-y-4">
              <div>
                <label htmlFor="foodName" className="block text-xs font-semibold text-gray-700 uppercase">
                  Dish Name *
                </label>
                <input
                  id="foodName"
                  type="text"
                  required
                  minLength={2}
                  maxLength={100}
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g. Special Chicken Biryani"
                  className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
                />
              </div>

              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <label htmlFor="foodCategory" className="block text-xs font-semibold text-gray-700 uppercase">
                    Category
                  </label>
                  <select
                    id="foodCategory"
                    value={formData.categoryId}
                    onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                    className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
                  >
                    <option value="">-- Select Category --</option>
                    {categories.map((cat) => (
                      <option key={cat.id} value={cat.id}>
                        {cat.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label htmlFor="foodPrice" className="block text-xs font-semibold text-gray-700 uppercase">
                    Price (PKR) *
                  </label>
                  <input
                    id="foodPrice"
                    type="number"
                    step="0.01"
                    min="0.01"
                    required
                    value={formData.price}
                    onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                    placeholder="650"
                    className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="foodPrepTime" className="block text-xs font-semibold text-gray-700 uppercase">
                  Prep Time (minutes, optional)
                </label>
                <input
                  id="foodPrepTime"
                  type="number"
                  min="1"
                  max="1440"
                  value={formData.preparationTimeMinutes}
                  onChange={(e) => setFormData({ ...formData, preparationTimeMinutes: e.target.value })}
                  placeholder="e.g. 45"
                  className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
                />
              </div>

              <div>
                <label htmlFor="foodDesc" className="block text-xs font-semibold text-gray-700 uppercase">
                  Description *
                </label>
                <textarea
                  id="foodDesc"
                  required
                  minLength={5}
                  maxLength={2000}
                  rows={3}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Describe your dish, key ingredients, spice levels, serving size..."
                  className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
                />
              </div>

              <div>
                <label htmlFor="foodImage" className="block text-xs font-semibold text-gray-700 uppercase">
                  Dish Photo (optional)
                </label>
                <div className="mt-1 flex items-center gap-3">
                  {imageFile ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img
                      src={URL.createObjectURL(imageFile)}
                      alt="Selected dish preview"
                      className="h-16 w-16 rounded-lg border border-gray-200 object-cover"
                    />
                  ) : editingFoodId &&
                    foods.find((f) => f.id === editingFoodId)?.imageThumbnailUrl ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img
                      src={
                        resolveImageUrl(
                          foods.find((f) => f.id === editingFoodId)?.imageThumbnailUrl
                        ) ?? ""
                      }
                      alt="Current dish photo"
                      className="h-16 w-16 rounded-lg border border-gray-200 object-cover"
                    />
                  ) : (
                    <div className="flex h-16 w-16 items-center justify-center rounded-lg border border-dashed border-gray-300 bg-gray-50 text-xl">
                      🍽️
                    </div>
                  )}
                  <div className="flex flex-col gap-1">
                    <label className="inline-flex w-fit cursor-pointer rounded-lg bg-gray-900 px-3 py-1.5 text-xs font-medium text-white hover:bg-gray-800">
                      {imageFile ? "Change photo" : "Upload photo"}
                      <input
                        type="file"
                        accept="image/jpeg,image/png,image/webp"
                        className="hidden"
                        onChange={handleImageChange}
                      />
                    </label>
                    {(imageFile || removeImage) && (
                      <button
                        type="button"
                        onClick={() => {
                          setImageFile(null);
                          setRemoveImage(true);
                        }}
                        className="w-fit text-xs font-medium text-red-600 hover:text-red-800 underline"
                      >
                        Remove photo
                      </button>
                    )}
                  </div>
                </div>
                <p className="mt-1 text-xs text-gray-500">
                  JPEG, PNG or WebP up to 5 MB. Resized and converted to WebP automatically.
                </p>
              </div>

              <div className="flex items-center gap-2 pt-2">
                <input
                  id="isAvailable"
                  type="checkbox"
                  checked={formData.isAvailable}
                  onChange={(e) => setFormData({ ...formData, isAvailable: e.target.checked })}
                  className="h-4 w-4 rounded border-gray-300 text-gray-900 focus:ring-gray-900"
                />
                <label htmlFor="isAvailable" className="text-sm font-medium text-gray-700">
                  Available for order immediately
                </label>
              </div>

              <div className="mt-6 flex justify-end gap-3 border-t pt-4">
                <button
                  type="button"
                  onClick={handleCloseForm}
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={submitting}
                  className="rounded-lg bg-gray-900 px-5 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                >
                  {submitting ? "Saving…" : editingFoodId ? "Save Changes" : "Create Dish"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
}
