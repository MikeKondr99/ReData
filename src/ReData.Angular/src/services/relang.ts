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
        next: "in_string"
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
        token: "keyword", // let
        regex: "\\blet\\b"
      }, {
        token: "keyword.operator.comparison", // comparison operators
        regex: "<|>|<=|>=|!=|="
      }],
      "in_string": [{
        token: "keyword", // escape sequences
        regex: "\\\\'"
      },  {
        token: "string",
        regex: "'",
        next: "start"
      }, {
        defaultToken: "string"
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
