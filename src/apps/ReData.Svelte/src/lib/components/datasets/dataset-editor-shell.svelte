<script lang="ts">
	import { Badge, Button, Card } from 'flowbite-svelte';
	import { ChevronLeftOutline, PlusOutline } from 'flowbite-svelte-icons';

	type EditorMode = 'create' | 'edit';

	type Props = {
		mode: EditorMode;
		datasetId?: string;
	};

	let { mode, datasetId }: Props = $props();

	const isCreateMode = $derived(mode === 'create');
	const pageTitle = $derived(isCreateMode ? 'Новый набор' : 'Редактирование набора');
	const pageDescription = $derived(
		isCreateMode
		? 'Подготовил каркас страницы создания. Следующим шагом подключу поля формы и сохранение.'
		: 'Подготовил каркас страницы редактирования. Следующим шагом подключу загрузку набора и рабочую форму.'
	);
</script>

<div class="flex flex-col gap-6" data-testid="dataset-editor-shell">
	<div class="flex flex-wrap items-start justify-between gap-4">
		<div class="space-y-3">
			<a
				href="/datasets"
				class="inline-flex items-center gap-2 text-sm font-medium text-slate-500 transition hover:text-slate-900"
			>
				<ChevronLeftOutline class="h-4 w-4" />
				К списку наборов
			</a>

			<div class="space-y-2">
				<div class="flex flex-wrap items-center gap-3">
					<h1 class="text-3xl font-semibold tracking-tight text-slate-950">{pageTitle}</h1>
					<Badge color={isCreateMode ? 'green' : 'blue'}>{isCreateMode ? 'Create' : 'Edit'}</Badge>
				</div>
				<p class="max-w-3xl text-sm leading-6 text-slate-600">{pageDescription}</p>
				{#if datasetId}
					<p class="text-xs text-slate-400">Dataset ID: {datasetId}</p>
				{/if}
			</div>
		</div>

		<div class="flex flex-wrap gap-2">
			<Button color="alternative" href="/datasets">Отмена</Button>
			<Button disabled>Сохранить</Button>
		</div>
	</div>

	<div class="grid gap-6 xl:grid-cols-[minmax(0,1.15fr)_minmax(340px,0.85fr)]">
		<div class="space-y-6">
			<Card>
				<div class="space-y-4">
					<div class="flex items-center justify-between gap-3">
						<div>
							<h2 class="text-lg font-semibold text-slate-950">Основные параметры</h2>
							<p class="text-sm text-slate-500">
								Здесь будут поля названия набора, выбранного коннектора и основные действия формы.
							</p>
						</div>
						<Button size="sm" disabled>
							<PlusOutline class="mr-2 h-4 w-4" />
							Создать коннектор
						</Button>
					</div>

					<div class="grid gap-4 md:grid-cols-2">
						<div class="space-y-2">
							<div class="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Название</div>
							<div class="rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-400">
								Поле названия подключу следующим шагом
							</div>
						</div>
						<div class="space-y-2">
							<div class="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Коннектор</div>
							<div class="rounded-xl border border-dashed border-slate-300 px-4 py-3 text-sm text-slate-400">
								Selector коннектора и модалка будут здесь
							</div>
						</div>
					</div>
				</div>
			</Card>

			<Card>
				<div class="space-y-4">
					<div>
						<h2 class="text-lg font-semibold text-slate-950">Трансформации</h2>
						<p class="text-sm text-slate-500">
							Этот блок зарезервирован под редактор шагов обработки данных и ошибки валидации.
						</p>
					</div>

					<div class="space-y-3">
						<div class="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-5 text-sm text-slate-400">
							Секция списка трансформаций
						</div>
						<div class="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-5 text-sm text-slate-400">
							Секция ошибок и негативных сценариев
						</div>
					</div>
				</div>
			</Card>
		</div>

		<Card class="h-fit">
			<div class="space-y-4">
				<div>
					<h2 class="text-lg font-semibold text-slate-950">Предпросмотр результата</h2>
					<p class="text-sm text-slate-500">
						Здесь будет таблица данных, состояния загрузки и server-side ошибки transform API.
					</p>
				</div>

				<div class="space-y-3">
					{#each Array.from({ length: 6 }) as _, index}
						<div class={`h-12 rounded-xl ${index === 0 ? 'bg-slate-200' : 'bg-slate-100'}`}></div>
					{/each}
				</div>
			</div>
		</Card>
	</div>
</div>
