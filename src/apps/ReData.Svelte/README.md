# ReData.Svelte

SvelteKit frontend for ReData demo app.

Stack:

- SvelteKit
- TypeScript
- Tailwind CSS (v4)
- Flowbite + Flowbite Svelte
- Prettier

## Recreate

If you need to recreate the base template:

```sh
npx sv@0.12.5 create --template minimal --types ts --install npm src/ReData.Svelte
```

## Developing

```sh
cd src/ReData.Svelte
npm install
npm run dev
```

`vite.config.ts` already proxies `/api` to `http://localhost:5223` for local DemoApp backend.
Flowbite is configured in `src/app.css`.

Current routes:

- `/` - home page
- `/functions` - functions page

## Checks and build

```sh
cd src/ReData.Svelte
npm run check
npm run build
```

## Formatting

```sh
cd src/ReData.Svelte
npm run format
npm run format:check
```

## Generate API client (Orval)

```sh
cd src/ReData.Svelte
npm run api:generate
```

By default Orval reads OpenAPI from `http://localhost:5223/openapi/v1.json`.

Before generation, start the backend locally if you use the default URL.

To override URL in PowerShell:

```powershell
$env:OPENAPI_URL = 'https://example.com/openapi/v1.json'
npm run api:generate
```

Notes:

- `orval` is pinned to `7.13.2` on purpose. Newer `7.14+` and `8.x` releases require
  Node `>= 22.18.0`.
- In the current environment (`Node v22.15.0`) `npm install orval@8.9.1` fails with
  `EBADENGINE`, so upgrade Node first if you want to move past `7.13.2`.
