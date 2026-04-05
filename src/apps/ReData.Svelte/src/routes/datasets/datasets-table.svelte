<script lang="ts">
	import { getAllDatasets, deleteDataset as deleteDatasetApi } from '$lib/api/generated/datasets/datasets';
	import {
		Table,
		TableBodyCell,
		TableBodyRow,
		TableHead,
		TableHeadCell
	} from 'flowbite-svelte';
	import {
		EditOutline,
		FileExportOutline,
		TrashBinOutline
	} from 'flowbite-svelte-icons';

	type Props = {};

	let {}: Props = $props();
	let datasetsPromise = $state(getAllDatasets());
	let deletingIds = $state(new Set<string>());

	function toDate(date: string) {
		const format = new Intl.DateTimeFormat('ru-RU', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
		return format.format(new Date(date));
	}

	async function deleteDataset(datasetId: string, datasetName: string): Promise<void> {
		if (deletingIds.has(datasetId)) {
			return;
		}

		const approved = window.confirm(`Удалить набор "${datasetName}"?`);
		if (!approved) {
			return;
		}

		deletingIds.add(datasetId);
		deletingIds = new Set(deletingIds);

		try {
			const response = await deleteDatasetApi(datasetId);
			if (response.status !== 200) {
				throw new Error(`Delete failed with status ${response.status}`);
			}

			datasetsPromise = getAllDatasets();
		} finally {
			deletingIds.delete(datasetId);
			deletingIds = new Set(deletingIds);
		}
	}
</script>

{#await datasetsPromise}
	Loading...
{:then response}
	{@const datasets = response.data}
	<Table>
		<TableHead>
			<TableHeadCell>Название</TableHeadCell>
			<TableHeadCell>Поля</TableHeadCell>
			<TableHeadCell>Записи</TableHeadCell>
			<TableHeadCell>Дата создания</TableHeadCell>
			<TableHeadCell>Дата редактирования</TableHeadCell>
			<TableHeadCell>Действия</TableHeadCell>
		</TableHead>
		{#each datasets as dataset}
			<TableBodyRow>
				<TableBodyCell>{dataset.name}</TableBodyCell>
				<TableBodyCell>{dataset.fieldList?.length ?? '-'}</TableBodyCell>
				<TableBodyCell>{dataset.rowsCount ?? '-'}</TableBodyCell>
				<TableBodyCell>{toDate(dataset.createdAt)}</TableBodyCell>
				<TableBodyCell>{toDate(dataset.updatedAt)}</TableBodyCell>
				<TableBodyCell>
					<div class="flex flex-row gap-2">
						<FileExportOutline class="h-6 w-6 shrink-0 text-blue-500" />
						<EditOutline class="h-6 w-6 shrink-0 text-blue-500" />
						<button
							type="button"
							class="cursor-pointer disabled:cursor-not-allowed"
							title="Удалить"
							disabled={deletingIds.has(dataset.id)}
							onclick={() => deleteDataset(dataset.id, dataset.name)}
						>
							<TrashBinOutline class="h-6 w-6 shrink-0 text-red-500" />
						</button>
					</div>
				</TableBodyCell>
			</TableBodyRow>
		{/each}
	</Table>
{:catch error}
	ERROR
	<!-- Do something with the error -->
{/await}
