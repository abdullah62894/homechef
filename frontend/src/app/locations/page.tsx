'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { getLocations, LocationDirectory } from '@/lib/search';
import { ApiError } from '@/lib/api';

export default function LocationsPage() {
  const [directory, setDirectory] = useState<LocationDirectory | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchLocations() {
      try {
        const data = await getLocations();
        setDirectory(data);
      } catch (err) {
        if (err instanceof ApiError) {
          setError(err.message);
        } else {
          setError('Failed to load locations');
        }
      } finally {
        setLoading(false);
      }
    }

    fetchLocations();
  }, []);

  if (loading) {
    return <div className="p-8 text-center text-gray-500">Loading locations...</div>;
  }

  if (error) {
    return <div className="p-8 text-center text-red-500">{error}</div>;
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <h1 className="text-3xl font-bold mb-8 text-gray-900">Locations Directory</h1>
      
      {!directory || directory.cities.length === 0 ? (
        <p className="text-gray-500">No locations available.</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {directory.cities.map((city) => (
            <div key={city.city} className="border border-gray-200 rounded-xl p-6 bg-white shadow-xs hover:border-gray-300 transition">
              <Link href={`/locations/${encodeURIComponent(city.city)}`} className="group">
                <h2 className="text-2xl font-semibold mb-2 group-hover:text-gray-700 transition-colors">
                  {city.city}
                  <span className="ml-2 text-sm font-normal text-gray-500 bg-gray-100 px-2.5 py-0.5 rounded-full">
                    {city.totalChefs} chef{city.totalChefs !== 1 ? 's' : ''}
                  </span>
                </h2>
              </Link>
              
              <div className="mt-4 space-y-2">
                <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider">Areas</h3>
                <ul className="space-y-1.5">
                  {city.areas.map((area) => (
                    <li key={area.name} className="flex justify-between items-center text-gray-700">
                      <Link 
                        href={`/locations/${encodeURIComponent(city.city)}/${encodeURIComponent(area.name)}`}
                        className="hover:text-gray-900 hover:underline text-sm font-medium"
                      >
                        {area.name}
                      </Link>
                      <span className="text-xs text-gray-400">{area.chefCount} chef{area.chefCount !== 1 ? 's' : ''}</span>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
