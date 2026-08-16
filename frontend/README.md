# HomeChef — Frontend

Next.js (App Router) + TypeScript + Tailwind CSS frontend for the HomeChef
platform. Server Components by default; client components only where
interactivity requires them.

## Getting started

```bash
npm install
cp .env.example .env   # set NEXT_PUBLIC_API_URL if needed
npm run dev            # http://localhost:3000
```

The API must be running (see the root README).

## Scripts

| Script | Purpose |
| ------ | ------- |
| `npm run dev` | Start the dev server |
| `npm run build` | Production build |
| `npm run start` | Serve the production build |
| `npm run lint` | ESLint |
| `npm run typecheck` | `tsc --noEmit` |
| `npm run format` | Prettier write |
| `npm run format:check` | Prettier check |

## Structure

```
src/
  app/            Routes (App Router)
  lib/api/        Single API fetch wrapper + consistent error handling
```

All backend access goes through `src/lib/api/` so API logic lives in one place.
The browser-facing base URL comes from `NEXT_PUBLIC_API_URL`; no secrets are
bundled into the client.
