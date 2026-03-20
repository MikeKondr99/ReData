import * as ace from 'ace-builds/src-noconflict/ace';

ace.define(
	'ace/mode/relang_highlight_rules',
	['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text_highlight_rules'],
	(require: any, exports: any) => {
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
		};

		oop.inherits(RelangHighlightRules, TextHighlightRules);
		exports.RelangHighlightRules = RelangHighlightRules;
	}
);

ace.define(
	'ace/mode/relang',
	['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text', 'ace/mode/relang_highlight_rules'],
	(require: any, exports: any) => {
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

(function () {
	ace.require(['ace/mode/relang'], (m: unknown) => {
		if (typeof module === 'object' && typeof exports === 'object' && module) {
			module.exports = m;
		}
	});
})();
