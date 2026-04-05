import * as ace from 'ace-builds/src-noconflict/ace';

ace.define("ace/mode/relang_highlight_rules", ["require", "exports", "module", "ace/lib/oop", "ace/mode/text_highlight_rules"], function (require: any, exports: any, module: any) {
  "use strict";
  var oop = require("../lib/oop");
  var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;
  var RelangHighlightRules = function (this: any) {

    this.$rules = {
      "start": [{
        token: "comment.line", // line comments
        regex: "\\/\\/.*$"
      }, {
        token: "comment.block", // block comments start
        regex: "\\/\\*",
        next: "block_comment"
      }, {
        token: "string", // string start quote
        regex: "'",
        next: "string_0"
      }, {
        token: "tag", // blocked name start bracket
        regex: "\\[",
        next: "blocked_name"
      }, {
        token: "constant.numeric", // decimals with digits before and after decimal
        regex: "\\b\\d+\\.\\d+\\b"
      }, {
        token: "constant.numeric", // integers
        regex: "\\b\\d+\\b"
      }, {
        token: "constant.numeric", // true, false, null
        regex: "\\b(true|false|null)\\b"
      }, {
        token: "keyword.operator", // and, or
        regex: "\\b(and|or)\\b"
      }, {
        token: "keyword.operator", // mathematical operators
        regex: "[+\\-*/^]"
      }, {
        token: "keyword", // let, const
        regex: "\\b(let|const)\\b"
      }, {
        token: "keyword.operator.comparison", // comparison operators
        regex: "<|>|<=|>=|!=|="
      }],
      "blocked_name": [{
        token: "keyword", // escape sequences
        regex: "\\\\\\]"
      }, {
        token: "tag",
        regex: "\\]",
        next: "start"
      }, {
        defaultToken: "tag"
      }],
      "block_comment": [{
        token: "comment.block", // block comments end
        regex: "\\*\\/",
        next: "start"
      }, {
        defaultToken: "comment.block" // block comment content
      }]
    };

    const MAX_INTERPOLATION_DEPTH = 3;
    // Generate string and interpolation rules for each level
    for (let level = 0; level <= MAX_INTERPOLATION_DEPTH; level++) {
      const isLastLevel = level === MAX_INTERPOLATION_DEPTH;

      // String rule for this level
      this.$rules[`string_${level}`] = [{
        token: "keyword", // escape sequences for quote
        regex: "\\\\'"
      }, {
        token: "keyword", // escape sequences for $
        regex: "\\\\\\$"
      }];

      // Only add interpolation if not the last level
      if (!isLastLevel) {
        this.$rules[`string_${level}`].push({
          token: "keyword",
          regex: "\\$\\{",
          next: `interpolation_${level}`
        });
      }

    // String end - go back to appropriate state
    this.$rules[`string_${level}`].push({
      token: "string",
      regex: "'",
      next: level === 0 ? "start" : `interpolation_${level - 1}`
    });

    this.$rules[`string_${level}`].push({
      defaultToken: "string"
    });

    // Interpolation rule for this level (only create if not the last level)
    if (!isLastLevel) {
      this.$rules[`interpolation_${level}`] = [{
        token: "keyword",
        regex: "\\}",
        next: `string_${level}`
      }, {
        token: "string",
        regex: "'",
        next: `string_${level + 1}`
      }, {
        include: "start"
      }];
    }
  }

  };
  oop.inherits(RelangHighlightRules, TextHighlightRules);
  exports.RelangHighlightRules = RelangHighlightRules;

});

ace.define("ace/mode/relang", ["require", "exports", "module", "ace/lib/oop", "ace/mode/text", "ace/mode/relang_highlight_rules"], function (require: any, exports: any, module: any) {
  "use strict";
  var oop = require("../lib/oop");
  var TextMode = require("./text").Mode;
  var RelangHighlightRules = require("./relang_highlight_rules").RelangHighlightRules;
  var Mode = function (this: any) {
    this.HighlightRules = RelangHighlightRules;
    this.$behaviour = this.$defaultBehaviour;
  };
  oop.inherits(Mode, TextMode);
  (function (this: any) {
    this.$id = "ace/mode/relang";
  }).call(Mode.prototype);
  exports.Mode = Mode;

});
(function () {
  ace.require(["ace/mode/relang"], function (m: any) {
    if (typeof module == "object" && typeof exports == "object" && module) {
      module.exports = m;
    }
  });
})();
