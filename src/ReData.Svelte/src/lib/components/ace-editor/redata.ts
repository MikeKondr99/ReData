console.info('[AceLoader] Module imported: start dynamic preload');

export type { Ace } from 'ace-builds';

export type AceModule = typeof import('ace-builds');

export const acePromise: Promise<AceModule> = (async () => {
	console.info('[AceLoader] Loading ace core...');
	const aceModule = await import('ace-builds/src-noconflict/ace');
	const ace = ((aceModule as unknown as { default?: AceModule }).default ??
		(aceModule as unknown as AceModule)) as AceModule;

	console.info('[AceLoader] Loading theme/ext...');
	await Promise.all([
		import('ace-builds/src-noconflict/ext-language_tools'),
		import('ace-builds/src-noconflict/theme-eclipse')
	]);
	console.info('[AceLoader] theme/ext loaded');

	console.info('[AceLoader] Registering relang mode...');
	registerRelang(ace);
	console.info('[AceLoader] relang mode registered');

	console.info('[AceLoader] ace export is ready');
	return ace;
})().catch((error) => {
	console.error('[AceLoader] Failed to load Ace', error);
	throw error;
});

console.info('[AceLoader] Dynamic preload started');

function registerRelang(ace: any): void {
	ace.define(
		'ace/mode/relang_highlight_rules',
		['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text_highlight_rules'],
		function (require: any, exports: any) {
			'use strict';
			const oop = require('../lib/oop');
			const TextHighlightRules = require('./text_highlight_rules').TextHighlightRules;
			const RelangHighlightRules = function (this: any) {
				this.$rules = {
					start: [
						{
							token: 'comment.line',
							regex: '\\/\\/.*$'
						},
						{
							token: 'comment.block',
							regex: '\\/\\*',
							next: 'block_comment'
						},
						{
							token: 'string',
							regex: "'",
							next: 'string_0'
						},
						{
							token: 'tag',
							regex: '\\[',
							next: 'blocked_name'
						},
						{
							token: 'constant.numeric',
							regex: '\\b\\d+\\.\\d+\\b'
						},
						{
							token: 'constant.numeric',
							regex: '\\b\\d+\\b'
						},
						{
							token: 'constant.numeric',
							regex: '\\b(true|false|null)\\b'
						},
						{
							token: 'keyword.operator',
							regex: '\\b(and|or)\\b'
						},
						{
							token: 'keyword.operator',
							regex: '[+\\-*/^]'
						},
						{
							token: 'keyword',
							regex: '\\b(let|const)\\b'
						},
						{
							token: 'keyword.operator.comparison',
							regex: '<|>|<=|>=|!=|='
						}
					],
					blocked_name: [
						{
							token: 'keyword',
							regex: '\\\\\\]'
						},
						{
							token: 'tag',
							regex: '\\]',
							next: 'start'
						},
						{
							defaultToken: 'tag'
						}
					],
					block_comment: [
						{
							token: 'comment.block',
							regex: '\\*\\/',
							next: 'start'
						},
						{
							defaultToken: 'comment.block'
						}
					]
				};

				const maxInterpolationDepth = 3;
				for (let level = 0; level <= maxInterpolationDepth; level += 1) {
					const isLastLevel = level === maxInterpolationDepth;

					this.$rules[`string_${level}`] = [
						{
							token: 'keyword',
							regex: "\\\\'"
						},
						{
							token: 'keyword',
							regex: '\\\\\\$'
						}
					];

					if (!isLastLevel) {
						this.$rules[`string_${level}`].push({
							token: 'keyword',
							regex: '\\$\\{',
							next: `interpolation_${level}`
						});
					}

					this.$rules[`string_${level}`].push({
						token: 'string',
						regex: "'",
						next: level === 0 ? 'start' : `interpolation_${level - 1}`
					});

					this.$rules[`string_${level}`].push({
						defaultToken: 'string'
					});

					if (!isLastLevel) {
						this.$rules[`interpolation_${level}`] = [
							{
								token: 'keyword',
								regex: '\\}',
								next: `string_${level}`
							},
							{
								token: 'string',
								regex: "'",
								next: `string_${level + 1}`
							},
							{
								include: 'start'
							}
						];
					}
				}
				// Очень важно для динамических рулов
				this.normalizeRules();
			};

			oop.inherits(RelangHighlightRules, TextHighlightRules);
			exports.RelangHighlightRules = RelangHighlightRules;
		}
	);

	ace.define(
		'ace/mode/relang',
		['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text', 'ace/mode/relang_highlight_rules'],
		function (require: any, exports: any) {
			'use strict';
			const oop = require('../lib/oop');
			const TextMode = require('./text').Mode;
			const RelangHighlightRules = require('./relang_highlight_rules').RelangHighlightRules;
			const Mode = function (this: any) {
				this.HighlightRules = RelangHighlightRules;
				this.$behaviour = this.$defaultBehaviour;
			};
			oop.inherits(Mode, TextMode);
			(function (this: any) {
				this.$id = 'ace/mode/relang';
			}.call(Mode.prototype));
			exports.Mode = Mode;
		}
	);
}
