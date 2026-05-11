<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/state';
	import '../app.css';
	import { Breadcrumb, BreadcrumbItem } from 'flowbite-svelte';
	import favicon from '$lib/assets/favicon.svg';
	import { HomeSolid } from 'flowbite-svelte-icons';
	import AuthStatus from '$lib/auth/auth-status.svelte';
	import { initAuth } from '$lib/auth/auth.svelte';
	import { buildBreadcrumbs } from '$lib/navigation/breadcrumbs';

	let { children } = $props();
	const breadcrumbs = $derived(buildBreadcrumbs(page.url.pathname));

	onMount(() => {
		void initAuth();
	});
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
	<title>ReData</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<header class="border-b border-gray-200 bg-white">
		<div class="flex w-full items-center justify-between gap-4 px-6 py-4">
			<Breadcrumb>
				<BreadcrumbItem href="/">
					{#snippet icon()}
						<HomeSolid class="h-6 w-6 shrink-0" />
					{/snippet}
					ReData
				</BreadcrumbItem>
				{#each breadcrumbs as breadcrumb}
					<BreadcrumbItem href={breadcrumb.href}>{breadcrumb.label}</BreadcrumbItem>
				{/each}
			</Breadcrumb>

			<AuthStatus />
		</div>
	</header>

	<main class="w-full px-6 py-6">
		{@render children()}
	</main>
</div>
