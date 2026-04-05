<script lang="ts">
	let { children } = $props();
</script>

<section class="prose prose-pre:bg-gray-50 prose-pre:text-black max-w-none">
	{@render children()}
</section>
