import { defineConfig } from 'orval';

const OPENAPI_TARGET = process.env.OPENAPI_URL ?? 'http://localhost:5223/openapi/v1.json';

export default defineConfig({
	redata: {
		input: {
			target: OPENAPI_TARGET
		},
		output: {
			target: 'src/lib/api/generated/endpoints.ts',
			schemas: 'src/lib/api/generated/model',
			client: 'fetch',
			mode: 'tags-split',
			clean: true,
			override: {
				mutator: {
					path: './src/lib/api/custom-fetch.ts',
					name: 'customFetch'
				}
			}
		}
	}
});
