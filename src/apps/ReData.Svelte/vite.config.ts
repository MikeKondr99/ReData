import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { svelteTesting } from '@testing-library/svelte/vite';
import { defineConfig } from 'vitest/config';

export default defineConfig({
	server: {
		host: '127.0.0.1',
		allowedHosts: true,
		proxy: {
			'/api': {
				target: 'http://localhost:5223',
				changeOrigin: true,
				secure: false
			},
			'/openapi': {
				target: 'http://localhost:5223',
				changeOrigin: true,
				secure: false
			},
			'/kc': {
				target: 'http://localhost:8080',
				changeOrigin: true,
				secure: false
			}
		}
	},
	plugins: [tailwindcss(), sveltekit(), svelteTesting()],
	test: {
		environment: 'jsdom',
		setupFiles: ['./vitest.setup.ts']
	}
});
