<script lang="ts">
	import { page } from '$app/state';
	import DatasetEditorShell from '$lib/components/datasets/dataset-editor-shell.svelte';
</script>

<DatasetEditorShell mode="edit" datasetId={page.params.id} />
