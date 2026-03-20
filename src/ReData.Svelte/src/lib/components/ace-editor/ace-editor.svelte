<script lang="ts">
	import { onMount } from 'svelte';
	import type { Ace } from 'ace-builds';
	import * as ace from 'ace-builds/src-noconflict/ace';
	import 'ace-builds/css/ace.css';
	import 'ace-builds/src-noconflict/theme-eclipse';
	import 'ace-builds/src-noconflict/ext-language_tools';
	import './relang';
	import type { ExprError, FunctionArgument, FunctionResponse } from '$lib/api/generated/model';
	import { DataType, FunctionKind } from '$lib/api/generated/model';

	interface Props {
		value?: string;
		errors?: ExprError[] | null;
		fields?: string[];
		functions?: FunctionResponse[];
		class?: string;
	}

	const keywords = ['const', 'null', 'true', 'false', 'and', 'or'] as const;

	let {
		value = $bindable(''),
		errors = undefined,
		fields = [],
		functions = [],
		class: className = ''
	}: Props = $props();

	let editorElement: HTMLDivElement | undefined;
	let editor: Ace.Editor | undefined;
	let markers: number[] = [];
	let syncingFromEditor = false;

	function updateGutter() {
		if (editor === undefined) {
			return;
		}

		editor.setOptions({
			showGutter: /[\r\n]/.test(editor.getValue()) || markers.length > 0
		});
	}

	function clearMarkersAndAnnotations() {
		if (editor === undefined) {
			return;
		}

		editor.getSession().setAnnotations([]);
		for (const marker of markers) {
			editor.getSession().removeMarker(marker);
		}
		markers = [];
	}

	function applyErrors() {
		if (editor === undefined) {
			return;
		}

		if (errors === undefined || errors === null || errors.length === 0) {
			clearMarkersAndAnnotations();
			updateGutter();
			return;
		}

		const annotations: Ace.Annotation[] = [];
		for (const marker of markers) {
			editor.getSession().removeMarker(marker);
		}
		markers = [];

		for (const error of errors) {
			const startRow = (error.span.startRow ?? 1) - 1;
			const startColumn = Math.max((error.span.startColumn ?? 1) - 1, 0);
			const endRow = (error.span.endRow ?? error.span.startRow ?? 1) - 1;
			const endColumn = Math.max(error.span.endColumn ?? error.span.startColumn ?? 1, 1);

			annotations.push({
				row: startRow,
				column: startColumn,
				text: error.message,
				type: 'error'
			});

			const marker = editor.getSession().addMarker(
				new ace.Range(
					startRow,
					startColumn,
					Math.max(endRow, startRow),
					Math.max(endColumn, startColumn + 1)
				),
				'ace-error',
				'text',
				false
			);

			markers.push(marker);
		}

		editor.getSession().setAnnotations(annotations);
		updateGutter();
	}

	function getKeywordCompletions(): Ace.Completion[] {
		return keywords.map((keyword) => ({
			caption: keyword,
			value: keyword,
			meta: 'keyword',
			score: 40
		}));
	}

	function getFunctionsCompletions(items: FunctionResponse[]): Ace.Completion[] {
		const grouped = new Map<string, FunctionResponse[]>();
		for (const item of items) {
			const existing = grouped.get(item.name);
			if (existing === undefined) {
				grouped.set(item.name, [item]);
				continue;
			}
			existing.push(item);
		}

		const result: Ace.Completion[] = [];
		for (const [name, groupedItems] of grouped.entries()) {
			const defaultOrMethod = groupedItems.filter(
				(item) => item.kind === FunctionKind.Default || item.kind === FunctionKind.Method
			);
			if (defaultOrMethod.length > 0) {
				result.push({
					caption: name,
					snippet: `${name}(\${1:})`,
					meta: 'function',
					score: 20,
					docHTML: createFunctionHtml(defaultOrMethod, false)
				});
			}

			const methods = groupedItems.filter((item) => item.kind === FunctionKind.Method);
			if (methods.length > 0) {
				result.push({
					caption: name,
					snippet: `${name}(\${1:})`,
					meta: 'method',
					score: 100,
					docHTML: createFunctionHtml(methods, true)
				});
			}
		}
		return result;
	}

	function getFieldCompletions(items: string[]): Ace.Completion[] {
		const simpleField = /^[a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*$/;
		const result: Ace.Completion[] = [];

		for (const field of items) {
			if (simpleField.test(field)) {
				result.push({
					caption: field,
					value: field,
					meta: 'field',
					score: 30
				});
				continue;
			}

			result.push({
				caption: field,
				value: `[${field}]`,
				meta: 'field',
				score: 29
			});
		}
		return result;
	}

	function getCompleter(fieldItems: string[], functionItems: FunctionResponse[]): Ace.Completer {
		const completions: Ace.Completion[] = [
			...getFieldCompletions(fieldItems),
			...getKeywordCompletions(),
			...getFunctionsCompletions(functionItems)
		];

		return {
			getCompletions: (
				_aceEditor: Ace.Editor,
				session: Ace.EditSession,
				pos: Ace.Position,
				_prefix: string,
				callback: Ace.CompleterCallback
			) => {
				const line = session.getLine(pos.row);
				const cursor = pos.column;
				const isMethodCall = isAfterDot(line, cursor);
				const resultCompletions = isMethodCall
					? completions.filter((item) => item.meta === 'method')
					: completions.filter((item) => item.meta !== 'method');

				callback(null, resultCompletions);
			}
		};
	}

	function applyCompleter() {
		if (editor === undefined) {
			return;
		}

		editor.setOptions({
			enableBasicAutocompletion: [getCompleter(fields, functions)]
		});
	}

	function isAfterDot(line: string, cursor: number): boolean {
		for (let index = cursor - 1; index >= 0; index -= 1) {
			const char = line.charAt(index);
			if (char === '.') {
				return true;
			}
			if (char === ' ' || char === '\t') {
				continue;
			}
			if (char === '\n' || char === ';' || char === '(') {
				break;
			}
		}
		return false;
	}

	onMount(() => {
		if (editorElement === undefined) {
			return;
		}

		const mountedEditor = ace.edit(editorElement);
		editor = mountedEditor;
		mountedEditor.setOptions({
			mode: 'ace/mode/relang',
			theme: 'ace/theme/eclipse',
			fontSize: 18,
			enableMultiselect: true,
			autoScrollEditorIntoView: true,
			highlightActiveLine: false,
			maxLines: 100,
			fontFamily: 'monospace',
			showLineNumbers: true,
			showGutter: true,
			readOnly: false,
			cursorStyle: 'slim'
		});

		mountedEditor.setValue(value);
		mountedEditor.clearSelection();
		applyCompleter();
		applyErrors();
		updateGutter();

		mountedEditor.session.on('change', () => {
			if (editor === undefined) {
				return;
			}

			syncingFromEditor = true;
			value = editor.getValue();
			updateGutter();
			syncingFromEditor = false;
		});

		return () => {
			if (editor !== undefined) {
				editor.destroy();
				editor = undefined;
			}
			markers = [];
		};
	});

	$effect(() => {
		if (editor === undefined) {
			return;
		}
		if (syncingFromEditor) {
			return;
		}
		if (editor.getValue() === value) {
			return;
		}

		editor.setValue(value);
		editor.clearSelection();
		updateGutter();
	});

	$effect(() => {
		if (editor === undefined) {
			return;
		}
		applyErrors();
	});

	$effect(() => {
		if (editor === undefined) {
			return;
		}
		applyCompleter();
	});

	function createFunctionHtml(funcs: FunctionResponse[], methods: boolean): string {
		let result = '';
		const skip = methods ? 1 : 0;

		for (const func of funcs) {
			const argsHtml = func.arguments
				.slice(skip)
				.map((arg, index) => createFunctionArgHtml(arg, index === func.arguments.length - 1 - skip))
				.join('');
			result += `<div class="function-item"><div class="function-signature">(${argsHtml}) → <span class="return-type">${createReturnType(func)}</span></div><div class="function-doc">${func.doc ?? ''}</div></div>`;
		}
		return `<div class="function-hint">${result}</div>`;
	}

	function createReturnType(func: FunctionResponse): string {
		const aggregatedPrefix = func.returnType.aggregated === true ? 'agg<' : '';
		const aggregatedSuffix = func.returnType.aggregated === true ? '>' : '';
		const notNullSuffix = func.returnType.canBeNull ? '' : '!';
		return `${aggregatedPrefix}${toDataTypeLabel(func.returnType.dataType)}${notNullSuffix}${aggregatedSuffix}`;
	}

	function createFunctionArgHtml(arg: FunctionArgument, last: boolean): string {
		const typeHtml = createTypeHtml(arg.type.dataType, arg.type.canBeNull);
		return `<span class="argument">${arg.name}: ${typeHtml}${last ? '' : ', '}</span>`;
	}

	function createTypeHtml(type: DataType, canBeNull: boolean): string {
		const notNullSuffix = canBeNull ? '' : '!';
		return `<span class="type">${toDataTypeLabel(type)}${notNullSuffix}</span>`;
	}

	function toDataTypeLabel(type: DataType): string {
		switch (type) {
			case DataType.Unknown:
				return 'Unknown';
			case DataType.Null:
				return 'Null';
			case DataType.Number:
				return 'Number';
			case DataType.Integer:
				return 'Integer';
			case DataType.Text:
				return 'Text';
			case DataType.Bool:
				return 'Bool';
			case DataType.DateTime:
				return 'DateTime';
			default:
				return String(type);
		}
	}
</script>

<div class={`ace-editor-container ${className}`.trim()}>
	<div bind:this={editorElement} class="editor-surface"></div>
</div>

<style>
	.ace-editor-container {
		width: 100%;
		border: 1px solid rgb(229, 231, 235);
		border-radius: 4px;
	}

	.editor-surface {
		width: 100%;
	}

	:global(.ace-warning) {
		border-radius: 0 !important;
		height: 21px !important;
		border-style: solid;
		border-color: coral;
		border-bottom-width: 2px;
		position: absolute;
	}

	:global(.ace-error) {
		border-radius: 0 !important;
		height: 21px !important;
		border-style: solid;
		border-color: red;
		border-bottom-width: 2px;
		position: absolute;
	}

	:global(.ace_doc-tooltip) {
		max-width: 400px !important;
	}

	:global(.function-item) {
		margin-bottom: 0;
	}

	:global(.argument) {
		color: #333;
	}

	:global(.type) {
		display: inline-block;
		color: #0066cc;
		font-weight: bold;
	}

	:global(.return-type) {
		display: inline-block;
		color: #0066cc;
		font-weight: bold;
	}

	:global(.function-doc) {
		color: #666;
		font-size: 15px;
	}
</style>
