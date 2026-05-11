<script lang="ts">
	import { DataType, type DataSetField } from '$lib/api/generated/model';
	import { ExportFileType } from '$lib/api/generated/model';
	import { getToken, initAuth, login } from '$lib/auth/auth.svelte';
	import {
		deleteDataset as deleteDatasetApi,
		getAllDatasets,
		getExportDatasetUrl
	} from '$lib/api/generated/datasets/datasets';
	import {
		Dropdown,
		DropdownItem,
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

	type Props = {
		class?: string;
		loadDatasets?: typeof getAllDatasets;
	};

	type DatasetListResponse = Awaited<ReturnType<typeof getAllDatasets>>;
	type DatasetsTableError = {
		status?: number;
		message: string;
	};

	const exportFormats = [
		{ label: 'CSV', value: ExportFileType.Csv },
		{ label: 'Excel', value: ExportFileType.Excel },
		{ label: 'JSON', value: ExportFileType.Json }
	] as const;

	let { class: className = '', loadDatasets = getAllDatasets }: Props = $props();
	let datasetsPromise = $derived.by(() => resolveDatasets(loadDatasets));
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

	function formatField(field: DataSetField): string {
		const dataTypeName = Object.entries(DataType).find(([, value]) => value === field.dataType)?.[0] ?? String(field.dataType);
		return `${field.alias}: ${dataTypeName}${field.canBeNull ? '' : '!'}`;
	}

	async function resolveDatasets(loader: typeof getAllDatasets): Promise<DatasetListResponse> {
		const response = (await loader()) as DatasetListResponse | { status: number; data?: unknown };
		if (response.status === 200) {
			return response as DatasetListResponse;
		}

		throw createLoadError(response.status);
	}

	function createLoadError(status?: number): DatasetsTableError {
		if (status === 401 || status === 403) {
			return {
				status,
				message: 'Нет доступа к наборам данных. Выполните вход через Keycloak.'
			};
		}

		return {
			status,
			message: 'Не удалось загрузить наборы данных.'
		};
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

			datasetsPromise = resolveDatasets(loadDatasets);
		} finally {
			deletingIds.delete(datasetId);
			deletingIds = new Set(deletingIds);
		}
	}

	async function exportDataset(datasetId: string, fileType: (typeof exportFormats)[number]['value']): Promise<void> {
		const url = getExportDatasetUrl(datasetId, { fileType });
		await initAuth();

		const token = await getToken();
		const headers = new Headers();
		if (token) {
			headers.set('Authorization', `Bearer ${token}`);
		}

		const response = await fetch(url, {
			method: 'GET',
			headers
		});

		if (response.status === 401) {
			void login();
			return;
		}

		if (!response.ok) {
			throw new Error(`Export failed with status ${response.status}`);
		}

		const blob = await response.blob();
		const downloadUrl = URL.createObjectURL(blob);
		const anchor = document.createElement('a');
		const contentDisposition = response.headers.get('Content-Disposition');
		const fileName = getFileName(contentDisposition, datasetId, fileType);

		anchor.href = downloadUrl;
		anchor.download = fileName;
		document.body.appendChild(anchor);
		anchor.click();
		anchor.remove();
		URL.revokeObjectURL(downloadUrl);
	}

	function getFileName(contentDisposition: string | null, datasetId: string, fileType: (typeof exportFormats)[number]['value']): string {
		const match = contentDisposition?.match(/filename\*?=(?:UTF-8''|")?([^";]+)/i);
		if (match?.[1]) {
			return decodeURIComponent(match[1].replace(/"/g, ''));
		}

		const extension = fileType === ExportFileType.Csv ? 'csv' : fileType === ExportFileType.Excel ? 'xlsx' : 'json';
		return `dataset-${datasetId}.${extension}`;
	}
</script>

{#await datasetsPromise}
	<div class="rounded-2xl border border-slate-200 bg-white px-6 py-10 text-sm text-slate-500">Загружаю наборы...</div>
{:then response}
	{@const datasets = response.data}
	{#if datasets.length === 0}
		<div class="rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-10 text-sm text-slate-500">
			Наборов пока нет. Можно начать с создания нового набора.
		</div>
	{:else}
		<Table class={className}>
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
					<TableBodyCell>
						<a href={`/datasets/${dataset.id}`} class="font-medium text-slate-900 hover:text-blue-600">
							{dataset.name}
						</a>
					</TableBodyCell>
					<TableBodyCell>
						{#if dataset.fieldList}
							<details class="group text-sm">
								<summary class="cursor-pointer list-none text-slate-700 marker:hidden">
									<span class="font-medium text-slate-900">Количество полей: {dataset.fieldList.length}</span>
									<span class="ml-2 text-xs text-slate-400 group-open:hidden">Показать</span>
									<span class="ml-2 hidden text-xs text-slate-400 group-open:inline">Скрыть</span>
								</summary>
								<div class="mt-2 space-y-1 rounded-xl border border-slate-200 bg-slate-50 p-3">
									{#if dataset.fieldList.length === 0}
										<div class="text-xs text-slate-500">Полей пока нет</div>
									{:else}
										{#each dataset.fieldList as field}
											<div class="text-xs text-slate-700">{formatField(field)}</div>
										{/each}
									{/if}
								</div>
							</details>
						{:else}
							-
						{/if}
					</TableBodyCell>
					<TableBodyCell>{dataset.rowsCount ?? '-'}</TableBodyCell>
					<TableBodyCell>{toDate(dataset.createdAt)}</TableBodyCell>
					<TableBodyCell>{toDate(dataset.updatedAt)}</TableBodyCell>
					<TableBodyCell>
						<div class="flex flex-row gap-2">
							<button
								id={`dataset-export-${dataset.id}`}
								type="button"
								title="Экспортировать"
								class="inline-flex cursor-pointer"
							>
								<FileExportOutline class="h-6 w-6 shrink-0 text-blue-500" />
							</button>
							<Dropdown placement="bottom-end" triggeredBy={`#dataset-export-${dataset.id}`} class="list-none">
								{#each exportFormats as format}
									<DropdownItem class="list-none" onclick={() => exportDataset(dataset.id, format.value)}>
										Экспорт в {format.label}
									</DropdownItem>
								{/each}
							</Dropdown>
							<a
								href={`/datasets/${dataset.id}`}
								class="inline-flex"
								title={`Редактировать ${dataset.name}`}
							>
								<EditOutline class="h-6 w-6 shrink-0 text-blue-500" />
							</a>
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
	{/if}
{:catch error}
	<div class="rounded-2xl border border-red-200 bg-red-50 px-6 py-10 text-sm text-red-700">
		{#if error instanceof Error}
			Не удалось загрузить наборы данных: {error.message}.
		{:else if typeof error === 'object' && error !== null && 'message' in error}
			{error.message}
		{:else}
			Не удалось загрузить наборы данных.
		{/if}
	</div>
{/await}
