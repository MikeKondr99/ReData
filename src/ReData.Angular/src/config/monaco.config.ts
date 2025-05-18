import {NgxMonacoEditorConfig} from 'ngx-monaco-editor-v2';
import * as MonacoModule from 'monaco-editor';

type Monaco = typeof MonacoModule;

export const monacoConfig: NgxMonacoEditorConfig = {
  baseUrl: 'assets',
  onMonacoLoad: () => {

    let monaco = <Monaco>((<any>window).monaco);


    monaco.languages.register({ id: "lang" });

    monaco.languages.setMonarchTokensProvider("lang", {
      keywords: ["true", "false", "null", "and", "or"],
      typeKeywords: ["Text", "Num", "Int", "Bool", "Date"],
      operators: ["=", ">", "<", "<=", ">=", "!=", "+", "-", "*", "/", "^"],
      symbols: /[=><!~?:&|+\-*\/\^%]+/,
      escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

      tokenizer: {
        root: [
          // Identifiers and keywords
          [/[a-zA-Zа-яА-Я_$][\w$]*/, {
            cases: {
              "@typeKeywords": "keyword",
              "@keywords": "keyword",
              "@default": "identifier",
            },
          }],
          // Whitespace
          { include: "@whitespace" },
          // Delimiters and operators
          [/[{}()\[\]]/, "@brackets"], // <-- Now {} are treated as brackets (default color)
          [/[<>](?!@symbols)/, "@brackets"],
          [/@symbols/, {
            cases: {
              "@operators": "operator",
              "@default": "",
            },
          }],
          // Numbers
          [/\d*\.\d+([eE][\-+]?\d+)?/, "number.float"],
          [/\d+/, "number"],
          // Delimiters
          [/[,.]/, "delimiter"],
          // Strings
          [/'([^'\\]|\\.)*$/, "string.invalid"], // Unclosed string
          [/'/, { token: "string.quote", bracket: "@open", next: "@string" }],
        ],

        string: [
          // Regular string content (not `{`, `}`, or `\`)
          [/[^\\'{]+/, "string"],
          // Escapes
          [/@escapes/, "string.escape"],
          [/\\./, "string.escape.invalid"],
          // Start interpolation (`{` is a bracket, not a string)
          [/\{/, { token: "@brackets", bracket: "@open", next: "@interpolate" }],
          // Close string
          [/'/, { token: "string.quote", bracket: "@close", next: "@pop" }],
        ],

        // Inside `{expression}`
        interpolate: [
          // End interpolation (`}` is a bracket, not a string)
          [/\}/, { token: "@brackets", bracket: "@close", next: "@pop" }],
          // Tokenize the expression inside `{}` like normal code
          { include: "root" },
        ],

        comment: [
          [/[^\/*]+/, "comment"],
          [/\/\*/, "comment", "@push"], // Nested comment
          ["\\*/", "comment", "@pop"],
          [/[\/*]/, "comment"],
        ],

        whitespace: [
          [/[ \t\r\n]+/, "white"],
          [/\/\*/, "comment", "@comment"],
          [/\/\/.*$/, "comment"],
        ],
      },
    });

  }
}


