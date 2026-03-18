<script lang="ts">
	import '../app.css';
	import { Breadcrumb, BreadcrumbItem } from 'flowbite-svelte';
	import favicon from '$lib/assets/favicon.svg';
	import { HomeSolid } from 'flowbite-svelte-icons';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<header class="border-b border-gray-200 bg-white">
		<div class="w-full max-w-6xl px-6 py-4">
			<Breadcrumb>
				<BreadcrumbItem href="/">
				    {#snippet icon()} 
					<HomeSolid class="shrink-0 h-6 w-6" />
					{/snippet}
    				ReData
				</BreadcrumbItem>
				<BreadcrumbItem href="/functions">TODO</BreadcrumbItem>
			</Breadcrumb>
		</div>
	</header>

	<main class="w-full px-6 py-6">
		{@render children()}
	</main>
</div>
