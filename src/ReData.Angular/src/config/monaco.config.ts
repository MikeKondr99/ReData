import {NgxMonacoEditorConfig} from 'ngx-monaco-editor-v2';
import * as MonacoModule from 'monaco-editor';

type Monaco = typeof MonacoModule;

export const monacoConfig: NgxMonacoEditorConfig = {
  baseUrl: 'assets',
  onMonacoLoad: () => {

    let monaco = <Monaco>((<any>window).monaco);


    monaco.languages.register({id: "lang"})

    monaco.languages.setMonarchTokensProvider('lang', {

      keywords: ['true', 'false', 'null', 'and', 'or'],

      typeKeywords: [
        'Text', 'Num', 'Int', 'Bool', 'Date',
      ],

      operators: ['=', '>', '<', '<=', '>=', '!=', '+', '-', '*', '/', '^'],

      // we include these common regular expressions
      symbols: /[=><!~?:&|+\-*\/\^%]+/,

      // C# style strings
      escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

      // The main tokenizer for our languages
      tokenizer: {
        root: [
          // identifiers and keywords
          [/[a-zA-Zа-яА-Я_$][\w$]*/, {
            cases: {
              '@typeKeywords': 'keyword',
              '@keywords': 'keyword',
              '@default': 'identifier'
            }
          }],
          // whitespace
          {include: '@whitespace'},

          // delimiters and operators
          [/[{}()\[\]]/, '@brackets'],
          [/[<>](?!@symbols)/, '@brackets'],
          [/@symbols/, {
            cases: {
              '@operators': 'operator',
              '@default': ''
            }
          }],

          // numbers
          [/\d*\.\d+([eE][\-+]?\d+)?/, 'number.float'],
          // [/0[xX][0-9a-fA-F]+/, 'number.hex'],
          [/\d+/, 'number'],

          // delimiter: after number because of .\d floats
          [/[,.]/, 'delimiter'],

          // strings
          [/'([^'\\]|\\.)*$/, 'string.invalid'],  // non-teminated string
          [/'/, {token: 'string.quote', bracket: '@open', next: '@string'}],

        ],

        comment: [
          [/[^\/*]+/, 'comment'],
          [/\/\*/, 'comment', '@push'],    // nested comment
          ["\\*/", 'comment', '@pop'],
          [/[\/*]/, 'comment']
        ],

        string: [
          [/[^\\']+/, 'string'],
          [/@escapes/, 'string.escape'],
          [/\\./, 'string.escape.invalid'],
          [/'/, {token: 'string.quote', bracket: '@close', next: '@pop'}]
        ],

        whitespace: [
          [/[ \t\r\n]+/, 'white'],
          [/\/\*/,       'comment', '@comment' ],
          [/\/\/.*$/,    'comment'],
        ],
      }
    });

  }
}


