'use client';

import { use, useEffect, useState } from 'react';
import Link from 'next/link';
import { getCityLocation, CitySummary } from '@/lib/search';
import { ApiError } from '@/lib/api';

export default function CityPage({ params }: { params: Promise<{ city: string }> }) {
  const { city: encodedCity } = use(params);
  const city = decodeURIComponent(encodedCity);
  
  const [cityData, setCityData] = useState<CitySummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchCity() {
      try {
        const data = await getCityLocation(city);
        setCityData(data);
      } catch (err) {
        if (err instanceof ApiError) {
          setError(err.message);
        } else {
          setError(`Failed to load data for ${city}`);
        }
      } finally {
        setLoading(false);
      }
    }

    fetchCity();
  }, [city]);

  if (loading) {
    return <div className="p-8 text-center text-gray-500">Loading city...</div>;
  }

  if (error) {
    return <div className="p-8 text-center text-red-500">{error}</div>;
  }

  if (!cityData) {
    return <div className="p-8 text-center text-gray-500">City not found</div>;
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <div className="mb-6 text-sm text-gray-500">
        <Link href="/locations" className="hover:text-gray-900 underline">Locations</Link> &gt; {cityData.city}
      </div>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-end gap-4 mb-8 border-b pb-6">
        <div>
          <h1 className="text-4xl font-bold text-gray-900">{cityData.city}</h1>
          <p className="mt-2 text-lg text-gray-600">
            {cityData.totalChefs} chef{cityData.totalChefs !== 1 ? 's' : ''} available
          </p>
        </div>
        
        <Link 
          href={`/search?city=${encodeURIComponent(cityData.city)}`}
          className="rounded-lg bg-gray-900 px-5 py-2.5 text-sm font-medium text-white hover:bg-gray-800 transition"
        >
          Search all {cityData.city} chefs →
        </Link>
      </div>

      <h2 className="text-xl font-semibold mb-6 text-gray-800">Areas &amp; Neighborhoods</h2>
      
      {cityData.areas.length === 0 ? (
        <p className="text-gray-500">No areas available in {cityData.city}.</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {cityData.areas.map((area) => (
            <Link 
              key={area.name} 
              href={`/locations/${encodeURIComponent(cityData.city)}/${encodeURIComponent(area.name)}`}
              className="border border-gray-200 rounded-xl p-5 bg-white hover:border-gray-300 hover:shadow-xs transition group"
            >
              <div className="flex justify-between items-center">
                <h3 className="text-base font-semibold text-gray-900 group-hover:underline">{area.name}</h3>
                <span className="bg-gray-100 text-gray-600 px-2.5 py-0.5 rounded-full text-xs font-medium">
                  {area.chefCount} chef{area.chefCount !== 1 ? 's' : ''}
                </span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
