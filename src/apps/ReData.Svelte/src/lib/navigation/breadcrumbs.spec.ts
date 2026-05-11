import { describe, expect, it } from 'vitest';
import { buildBreadcrumbs } from './breadcrumbs';

describe('buildBreadcrumbs', () => {
	it('maps datasets create route to readable labels', () => {
		expect(buildBreadcrumbs('/datasets/new')).toEqual([
			{ label: 'Наборы данных', href: '/datasets' },
			{ label: 'Новый набор', href: undefined }
		]);
	});

	it('maps dataset edit route to readable labels', () => {
		expect(buildBreadcrumbs('/datasets/123')).toEqual([
			{ label: 'Наборы данных', href: '/datasets' },
			{ label: 'Редактирование', href: undefined }
		]);
	});

	it('maps known top-level routes', () => {
		expect(buildBreadcrumbs('/functions')).toEqual([{ label: 'Функции', href: undefined }]);
	});
});
