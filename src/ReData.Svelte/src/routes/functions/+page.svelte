<script lang="ts">
	import Alert from 'flowbite-svelte/Alert.svelte';
	import ListPlaceholder from 'flowbite-svelte/ListPlaceholder.svelte';
	import Listgroup from 'flowbite-svelte/Listgroup.svelte';
	import ListgroupItem from 'flowbite-svelte/ListgroupItem.svelte';
	import Search from 'flowbite-svelte/Search.svelte';
	import { FileSearchOutline } from 'flowbite-svelte-icons';
	import {
		getFunctions,
		type FunctionResponse
	} from '$lib/stores/functions-store.svelte';
	import { useDebounce } from '$lib/utils.svelte';

	let search = $state('');
	const debouncedSearch = useDebounce(() => search, 300);

	const functionsPromise = getFunctions().then((response) => {
		if (response.status !== 200) {
			throw response;
		}
		functions = response.data;
		return response.data;
	});

	let functions = $state<FunctionResponse[]>([]);

	const filteredFunctions = $derived.by(() => {
		let query = debouncedSearch().trim().toLowerCase();
		return functions.filter((item) => {
			let doc = item.doc?.toLowerCase();
			let sign = item.displayText.toLowerCase();
			return (sign + doc).includes(query);
		});
	});
</script>

<Search bind:value={search} placeholder="Поиск функций" class="mb-2" />

{#await functionsPromise}
	<ListPlaceholder itemNumber={20} class="max-w-full" />
{:then}
	<Listgroup>
		{#each filteredFunctions as item (item.displayText)}
			<ListgroupItem class="flex flex-col items-start">
				<div class="text-base text-black">{item.displayText}</div>
				<div class="text-sm">
					{item.doc ?? 'Документация отсутствует.'}
				</div>
			</ListgroupItem>
		{:else}
			<div class="flex flex-col items-center py-5">
				<FileSearchOutline class="h-10 w-10" />
				<p>Ничего не найдено.</p>
			</div>
		{/each}
	</Listgroup>
{:catch error}
	<Alert
		>Не удалось загрузить функции: {error instanceof Error
			? error.message
			: 'unknown error'}</Alert
	>
{/await}
