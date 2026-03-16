export function useDebounce<T>(getter: () => T, delay: number = 300): () => T {
	let debounced = $state<T>(getter());
	let timer: ReturnType<typeof setTimeout> | undefined;

	$effect(() => {
		console.log('effect');
		const next = getter();

		if (timer) clearTimeout(timer);

		timer = setTimeout(() => {
			debounced = next;
		}, delay);

		return () => {
			if (timer) clearTimeout(timer);
		};
	});

	return () => debounced;
}
