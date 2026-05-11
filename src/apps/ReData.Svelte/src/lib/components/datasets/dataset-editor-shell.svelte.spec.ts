import { render, screen } from '@testing-library/svelte';
import { describe, expect, it } from 'vitest';
import DatasetEditorShell from './dataset-editor-shell.svelte';

describe('DatasetEditorShell', () => {
	it('renders create mode shell copy', () => {
		render(DatasetEditorShell, { mode: 'create' });

		expect(screen.getByTestId('dataset-editor-shell')).toBeTruthy();
		expect(screen.getByRole('heading', { name: 'Новый набор' })).toBeTruthy();
		expect(screen.getByText('Подготовил каркас страницы создания. Следующим шагом подключу поля формы и сохранение.')).toBeTruthy();
	});

	it('renders edit mode shell and dataset id', () => {
		render(DatasetEditorShell, { mode: 'edit', datasetId: 'dataset-42' });

		expect(screen.getByRole('heading', { name: 'Редактирование набора' })).toBeTruthy();
		expect(screen.getByText('Dataset ID: dataset-42')).toBeTruthy();
	});
});
