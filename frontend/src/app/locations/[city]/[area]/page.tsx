'use client';

import { use, useEffect, useState } from 'react';
import Link from 'next/link';
import { listChefsInArea } from '@/lib/search';
import type { ChefListPage } from '@/lib/chefs';
import { ApiError } from '@/lib/api';

const PAGE_SIZE = 10;

export default function AreaPage({ params }: { params: Promise<{ city: string; area: string }> }) {
  const { city: encodedCity, area: encodedArea } = use(params);
  const city = decodeURIComponent(encodedCity);
  const area = decodeURIComponent(encodedArea);
  
  const [data, setData] = useState<ChefListPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  useEffect(() => {
    async function fetchChefs() {
      setLoading(true);
      try {
        const result = await listChefsInArea(city, area, page, PAGE_SIZE);
        setData(result);
        window.scrollTo(0, 0);
      } catch (err) {
        if (err instanceof ApiError) {
          setError(err.message);
        } else {
          setError(`Failed to load chefs in ${area}`);
        }
      } finally {
        setLoading(false);
      }
    }

    fetchChefs();
  }, [city, area, page]);

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <div className="mb-6 text-sm text-gray-500">
        <Link href="/locations" className="hover:text-gray-900 underline">Locations</Link> &gt;{' '}
        <Link href={`/locations/${encodeURIComponent(city)}`} className="hover:text-gray-900 underline">{city}</Link> &gt;{' '}
        <span className="text-gray-800 font-medium">{area}</span>
      </div>

      <div className="mb-8 pb-4 border-b">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Chefs in {area}, {city}</h1>
        {data && (
          <p className="text-gray-600 text-sm">
            Showing {data.items.length} of {data.total} chef{data.total !== 1 ? 's' : ''}
          </p>
        )}
      </div>

      {loading && !data ? (
        <div className="p-8 text-center text-gray-500">Loading chefs...</div>
      ) : error ? (
        <div className="p-8 text-center text-red-500">{error}</div>
      ) : data?.items.length === 0 ? (
        <div className="p-8 text-center text-gray-500">No chefs found in this area.</div>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-6 mb-8">
            {data?.items.map((chef) => (
              <div key={chef.id} className="border border-gray-200 rounded-xl p-6 bg-white shadow-xs hover:border-gray-300 transition flex flex-col sm:flex-row gap-6">
                <div className="flex-grow">
                  <div className="flex justify-between items-start mb-2">
                    <h2 className="text-xl font-bold text-gray-900">
                      <Link href={`/chefs/${chef.id}`} className="hover:underline">
                        {chef.displayName}
                      </Link>
                    </h2>
                  </div>
                  
                  {chef.cuisines && chef.cuisines.length > 0 && (
                    <div className="flex flex-wrap gap-1.5 mb-3">
                      {chef.cuisines.map((c) => (
                        <span key={c} className="px-2 py-0.5 bg-gray-100 text-gray-700 text-xs rounded-full">
                          {c}
                        </span>
                      ))}
                    </div>
                  )}
                  
                  <p className="text-gray-600 text-sm mb-4 line-clamp-2">
                    {chef.bio || 'Home-based chef offering authentic homemade meals.'}
                  </p>
                  
                  <div className="mt-auto pt-4 border-t border-gray-100 flex items-center justify-between">
                    <span className="text-xs text-gray-500">{chef.city}{chef.area ? `, ${chef.area}` : ''}</span>
                    <Link 
                      href={`/chefs/${chef.id}`}
                      className="text-sm font-medium text-gray-900 hover:underline"
                    >
                      View profile &rarr;
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
          
          {/* Pagination */}
          {data && (data.hasMore || page > 1) && (
            <div className="flex justify-center items-center space-x-4 mt-8">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1 || loading}
                className="px-4 py-2 text-sm border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Previous
              </button>
              <span className="text-sm text-gray-600">
                Page {page}
              </span>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={!data.hasMore || loading}
                className="px-4 py-2 text-sm border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
