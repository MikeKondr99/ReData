<script lang="ts">
	import { onMount } from 'svelte';
	import { acePromise, type Ace } from './redata';

	type Props = {
		value: string;
		readonly: boolean;
		class: string;
	};

	let { value = $bindable(), readonly = false, class: clazz }: Props = $props();

	let host: HTMLElement;
	let editor: Ace.Editor | undefined;

	onMount(() => {
		let disposed = false;

		void (async () => {
			const ace = await acePromise;
			if (disposed) {
				return;
			}

			editor = ace.edit(host);
			editor.setOptions({
				mode: `ace/mode/relang`,
				theme: `ace/theme/eclipse`,
				fontSize: 18,
				enableMultiselect: true,
				autoScrollEditorIntoView: true,
				highlightActiveLine: false,
				maxLines: 100,
				fontFamily: 'Fira Code',
				showLineNumbers: true,
				showGutter: false,
				readOnly: readonly,
				cursorStyle: 'slim'
				// enableBasicAutocompletion: true,
				// enableLiveAutocompletion: true,
				// enableSnippets: true,
			});
			editor.setValue(value);
			editor.clearSelection();
		})();

		return () => {
			disposed = true;
			editor?.destroy();
			editor = undefined;
		};
	});
</script>

<div class={["editor-container", clazz]} bind:this={host}>
	<span class="preview-text inline  not-prose font-[Fira Code] mx-1">
		(price * quantity) &lt= 0.8
	</span>
</div>

<style>
    :global(.editor-container-readonly .ace_content) {
        background-color: oklch(0.967 0.003 264.542);
    }
    
	.editor-container {
		border: 1px solid rgb(229, 231, 235);
		border-radius: 4px;
		height: 24px;
	}
	.preview-text {
		font-family: 'Fira Code', monospace;
		font-size: 18px;
		font-weight: 300; /* у тебя Light */
		letter-spacing: 0;
		line-height: 1;
		font-kerning: none;
		/*height: 110%;*/
		font-variant-ligatures: none;
		white-space: pre;
		overflow: hidden;
		tab-size: 4;
		width: calc(100% - 4px);
		margin-top: 5px;
		display: inline-flex;
		transform: translateY(-2px);
	}
</style>
