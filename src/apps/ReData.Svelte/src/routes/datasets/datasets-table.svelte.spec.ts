import { fireEvent, render, screen } from '@testing-library/svelte';
import { describe, expect, it } from 'vitest';
import type { getAllDatasetsResponse } from '$lib/api/generated/datasets/datasets';
import { DataType } from '$lib/api/generated/model';
import DatasetsTable from './datasets-table.svelte';

describe('DatasetsTable', () => {
	it('renders loading state before data is resolved', () => {
		let resolvePromise: ((value: getAllDatasetsResponse) => void) | undefined;
		const loadDatasets = () =>
			new Promise<getAllDatasetsResponse>((resolve) => {
				resolvePromise = resolve;
			});

		render(DatasetsTable, { loadDatasets });

		expect(screen.getByText('Загружаю наборы...')).toBeTruthy();

		resolvePromise?.({
			status: 200,
			headers: new Headers(),
			data: []
		});
	});

	it('renders empty state', async () => {
		const loadDatasets = async (): Promise<getAllDatasetsResponse> => ({
			status: 200,
			headers: new Headers(),
			data: []
		});

		render(DatasetsTable, { loadDatasets });

		expect(await screen.findByText('Наборов пока нет. Можно начать с создания нового набора.')).toBeTruthy();
	});

	it('renders edit links for loaded datasets', async () => {
		const loadDatasets = async (): Promise<getAllDatasetsResponse> => ({
			status: 200,
			headers: new Headers(),
			data: [
				{
					id: 'dataset-1',
					name: 'Продажи',
					createdAt: '2026-05-09T10:00:00Z',
					updatedAt: '2026-05-09T11:00:00Z',
					rowsCount: 12,
					fieldList: [
						{
							alias: 'amount',
							dataType: DataType.Integer,
							canBeNull: false
						}
					]
				}
			]
		});

		render(DatasetsTable, { loadDatasets });

		expect(await screen.findByText('Продажи')).toBeTruthy();
		expect(screen.getByTitle('Редактировать Продажи').getAttribute('href')).toBe('/datasets/dataset-1');
		expect(screen.getByText('Количество полей: 1')).toBeTruthy();
	});

	it('reveals field list when fields details is expanded', async () => {
		const loadDatasets = async (): Promise<getAllDatasetsResponse> => ({
			status: 200,
			headers: new Headers(),
			data: [
				{
					id: 'dataset-1',
					name: 'Продажи',
					createdAt: '2026-05-09T10:00:00Z',
					updatedAt: '2026-05-09T11:00:00Z',
					rowsCount: 12,
					fieldList: [
						{
							alias: 'amount',
							dataType: DataType.Integer,
							canBeNull: false
						}
					]
				}
			]
		});

		render(DatasetsTable, { loadDatasets });

		const summary = await screen.findByText('Количество полей: 1');
		await fireEvent.click(summary);

		expect(screen.getByText('amount: Integer!')).toBeTruthy();
	});

	it('renders access denied message for unauthorized response', async () => {
		const loadDatasets = async () =>
			({
				status: 401,
				headers: new Headers(),
				data: null
			}) as unknown as getAllDatasetsResponse;

		render(DatasetsTable, { loadDatasets });

		expect(await screen.findByText('Нет доступа к наборам данных. Выполните вход через Keycloak.')).toBeTruthy();
	});

	it('renders generic error when loader rejects', async () => {
		const loadDatasets = async () => {
			throw new Error('network down');
		};

		render(DatasetsTable, { loadDatasets });

		expect(await screen.findByText('Не удалось загрузить наборы данных: network down.')).toBeTruthy();
	});
});
