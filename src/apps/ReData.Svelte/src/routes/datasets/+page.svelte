<script lang="ts">
	import { Button } from 'flowbite-svelte';
	import { PlusOutline } from 'flowbite-svelte-icons';
	import DatasetsTable from './datasets-table.svelte';
</script>

<div class="flex flex-col gap-4">
	<div class="flex items-center justify-between gap-3">
		<div>
			<h1 class="text-2xl font-semibold tracking-tight text-slate-950">Наборы данных</h1>
			<p class="text-sm text-slate-500">Список наборов с поиском нужного набора и экспортом в файлы.</p>
		</div>

		<Button class="bg-blue-500" size="sm" href="/datasets/new">
			<PlusOutline class="mr-2 h-5 w-5 shrink-0" />
			Добавить набор
		</Button>
	</div>

	<DatasetsTable class="w-full" />
</div>
