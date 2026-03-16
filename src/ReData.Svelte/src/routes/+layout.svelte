<script lang="ts">
	import '../app.css';
	import { Breadcrumb, BreadcrumbItem } from 'flowbite-svelte';
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<header class="border-b border-gray-200 bg-white">
		<div class="mx-auto w-full max-w-6xl px-6 py-4">
			<Breadcrumb>
				<BreadcrumbItem href="/">ReData</BreadcrumbItem>
				<BreadcrumbItem href="/functions">Функции</BreadcrumbItem>
			</Breadcrumb>
		</div>
	</header>

	<main class="mx-auto w-full max-w-6xl px-6 py-6">
		{@render children()}
	</main>
</div>
