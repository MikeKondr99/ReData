import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	server: {
		host: '127.0.0.1',
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
			}
		}
	},
	plugins: [tailwindcss(), sveltekit()]
});
