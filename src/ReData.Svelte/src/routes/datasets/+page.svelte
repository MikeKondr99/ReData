<script lang="ts">
	import { Alert, Button } from 'flowbite-svelte';
	import DatasetsTable from './datasets-table.svelte';
	import { PlusOutline } from 'flowbite-svelte-icons';

	type Props = {};

	let {}: Props = $props();
</script>

<div class="flex flex-col gap-2">
	<Alert color="red" class="font-medium">
		Мне лень делать auth поэтому прошу вести себя прилично. Не удаляйте чужие
		эксперименты. По желанию подписывайтесь под своими.
	</Alert>
	<Button class="bg-blue-500" size="xs">
		<PlusOutline class="h-6 w-6 shrink-0" /> Добавить набор
	</Button>
	<DatasetsTable class="w-full" />
</div>
