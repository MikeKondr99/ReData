# ReData.Svelte

SvelteKit frontend for ReData demo app.

Stack:

- SvelteKit
- TypeScript
- Tailwind CSS (v4)
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

To override URL in PowerShell:

```powershell
$env:OPENAPI_URL = 'https://example.com/openapi/v1.json'
npm run api:generate
```
