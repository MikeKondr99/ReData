export type BreadcrumbItem = {
	label: string;
	href?: string;
};

const labelMap: Record<string, string> = {
	functions: 'Функции',
	datasets: 'Наборы данных',
	new: 'Новый набор',
	docs: 'Документация'
};

export function buildBreadcrumbs(pathname: string): BreadcrumbItem[] {
	const normalizedPath = pathname.split('?')[0] ?? '/';
	const segments = normalizedPath.split('/').filter(Boolean);

	if (segments.length === 0) {
		return [];
	}

	let currentHref = '';

	return segments.map((segment, index) => {
		currentHref += `/${segment}`;
		const isLast = index === segments.length - 1;

		return {
			label: translateSegment(segment, index, segments),
			href: isLast ? undefined : currentHref
		};
	});
}

function translateSegment(segment: string, index: number, segments: string[]): string {
	if (segment in labelMap) {
		return labelMap[segment];
	}

	const previous = segments[index - 1];
	if (previous === 'datasets') {
		return 'Редактирование';
	}

	if (isGuidLike(segment)) {
		return 'Элемент';
	}

	return decodeURIComponent(segment);
}

function isGuidLike(value: string): boolean {
	return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
}
