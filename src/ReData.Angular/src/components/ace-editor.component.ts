// editor.component.ts
import {
  Component,
  ElementRef,
  AfterViewInit,
  OnDestroy,
  effect, viewChild, input, output, inject, untracked
} from '@angular/core';
import * as ace from 'ace-builds';
import type {Ace} from 'ace-builds';
import '../services/relang';
import 'ace-builds/src-noconflict/theme-eclipse';
import 'ace-builds/src-noconflict/ext-language_tools'
import {DataType, ExprErrors, FunctionArgument, FunctionViewModel} from '../types';
import {FunctionService} from '../services/function.service';
import {groupBy} from '../helpers';

@Component({
  standalone: true,
  selector: 'app-ace-editor',
  template: `
    <div #editor class="editor-container" [style.width]="'100%'"></div>
  `,
  styles: [`
    .editor-container {
      border: 1px solid rgb(229, 231, 235);
      border-radius: 4px;
    }
  `]
})
export class AceEditorComponent implements AfterViewInit, OnDestroy {


  public editorElement = viewChild<ElementRef>('editor',);

  public value = input<string>('');

  public errors = input<ExprErrors>();

  public fields = input.required<string[]>();

  public valueChange = output<string>();

  private functions = inject(FunctionService);


  _ = effect(() => {
    let funcs = this.functions.data();
    // let fields = this.fields();
    if (funcs && this.editor) {

      this.editor?.setOptions({
        enableBasicAutocompletion: [this.getCompleter([], funcs)],
      })

    }
  })


  updateValue = effect(() => {
    let value = this.value();
    if (this.editor?.getValue() !== value) {
      this.editor?.setValue(value);
      this.editor?.clearSelection();
    }
    // this.valueChange.emit(this.value());
  });


  updateError = effect(() => {
    let errors = this.errors();

    if(errors && errors?.length > 0) {
      const annotations: Ace.Annotation[] = [];
      for (const marker of this.markers) {
        this.editor?.getSession().removeMarker(marker);
      }
      this.markers = [];
      for (const error of errors) {
        annotations.push({
          row: error.span.startRow - 1,
          column: error.span.startColumn - 1,
          text: error.message,
          type: "error", // Shows in gutter and hover
        })
        const newMarker = this.editor?.getSession().addMarker(new ace.Range(error.span.startRow - 1, error.span.startColumn, error.span.endRow - 1, error.span.endColumn), "ace-error", "text", false);
        if(newMarker) {
          this.markers.push(newMarker);
        }
      }
      this.editor?.getSession().setAnnotations(annotations);
    } else {
      // Стираем аннотации и маркеры
      this.editor?.getSession().setAnnotations([]);
      for (const marker of this.markers) {
        this.editor?.getSession().removeMarker(marker);
      }
      this.markers = [];
    }


    this.updateGutter();
  })

  private markers: number[] = [];
  private editor?: Ace.Editor;

  ngAfterViewInit() {
    this.initializeEditor();
  }

  private initializeEditor() {
    // Initialize the editor
    this.editor = ace.edit(this.editorElement()!.nativeElement);

    this.editor.setOptions({
      mode: `ace/mode/relang`,
      theme: `ace/theme/eclipse`,
      fontSize: 18,
      enableMultiselect: true,
      autoScrollEditorIntoView: true,
      highlightActiveLine: false,
      maxLines: 100,
      fontFamily: 'Fira Code',
      showLineNumbers: true,
      showGutter: true,
      readOnly: false,
      cursorStyle: 'slim',
      // enableBasicAutocompletion: true,
      // enableLiveAutocompletion: true,
      // enableSnippets: true,
    });
    this.editor.setValue(this.value())
    this.updateGutter();
    this.editor.clearSelection();
    let funcs = untracked(this.functions.data);
    if (funcs) {
      this.editor?.setOptions({
        enableBasicAutocompletion: [this.getCompleter([], funcs)],
      })
    }
    this.editor.session.on('change', () => {
      let newValue = this.editor?.getValue() ?? '';
      this.updateGutter();
      this.valueChange.emit(newValue);
    });
  }

  updateGutter() {
    this.editor?.setOptions({
      showGutter: /[\r\n]/.test(this.editor?.getValue()) || !!this.markers
    })
  }

  ngOnDestroy() {
    if (this.editor) {
      this.editor.destroy();
    }
  }

  private static getFieldCompletions(fields: string[]): Ace.Completion[] {
    const simpleField = /^[a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*$/;
    const result: Ace.Completion[] = [];

    for (let field of fields) {
      if (simpleField.test(field)) {
        result.push({
          caption: field,
          value: field,
          meta: "field",
          score: 30
        });
      } else {
        result.push({
          caption: field,
          value: `[${field}]`,
          meta: "field",
          score: 29
        });

      }
    }
    return result;
  }

  private static getFunctionsCompletions(functions: FunctionViewModel[]): Ace.Completion[] {
    const result: Ace.Completion[] = [];

    // Функции
    let funcGroups = groupBy(functions.filter(f => f.kind === 'Default' || f.kind === 'Method'), (f) => f.name);
    for (let funcName in funcGroups) {
      result.push({
        caption: `${funcName}`,
        snippet: `${funcName}(\${1:})`,
        meta: "function",
        score: 20,
        docHTML: createFunctionHtml(funcGroups[funcName], false),
      });
    }

    // Методы
    funcGroups = groupBy(functions.filter(f => f.kind === 'Method'), (f) => f.name);
    for (let funcName in funcGroups) {
      result.push({
        caption: `${funcName}`,
        snippet: `${funcName}(\${1:})`,
        meta: "method",
        score: 100,
        docHTML: createFunctionHtml(funcGroups[funcName], true),
      });
    }
    return result;

  }


  getCompleter(fields: string[], functions: FunctionViewModel[]): Ace.Completer {
    let completions: Ace.Completion[] = [];

    // completions = completions.concat(AceEditorComponent.getFieldCompletions(fields));
    completions = completions.concat(AceEditorComponent.getFunctionsCompletions(functions))

    return {
      getCompletions: function (editor: Ace.Editor, session: Ace.EditSession, pos: Ace.Position, prefix: string, callback: Ace.CompleterCallback) {
        const line = session.getLine(pos.row);
        const cursor = pos.column;

        const isAfterDot = (line: string, cursor: number) => {
          // Look backwards from cursor to find if there's a dot before
          for (let i = cursor - 1; i >= 0; i--) {
            const char = line.charAt(i);
            if (char === '.') {
              return true;
            }
            if (char === ' ' || char === '\t') {
              continue; // skip whitespace
            }
            if (char === '\n' || char === ';' || char === '(') {
              break; // we hit the beginning of expression
            }
          }
          return false;
        };


        // Check if we're after a dot (for method completion)
        const isMethodCall = isAfterDot(line, cursor);

        let resultCompletions: Ace.Completion[] = [];
        if (isMethodCall) {
          // Return method completions with modified signatures
          resultCompletions = completions.filter(c => c.meta == 'method')
        } else {
          // Return regular completions
          resultCompletions = completions.filter(c => c.meta != 'method')
        }
        console.log('resultCompletions', resultCompletions);
        callback(null, resultCompletions);
      },
    }
  }

}


function createFunctionHtml(funcs: FunctionViewModel[], methods: boolean): string {
  let result = '';
  let skip = methods ? 1 : 0;

  for (let func of funcs) {
    result += `<div class="function-item"><div class="function-signature">(${func.arguments.slice(skip).map((a, i) => createFunctionArgHtml(a, i == func.arguments.length - 1 - skip)).join('')}) → <span class="return-type">${(func.returnType.aggregated ? 'agg<' : '')}${func.returnType.dataType}${(func.returnType.canBeNull ? '' : '!')}${(func.returnType.aggregated ? '>' : '')}</span></div><div class="function-doc">${func.doc}</div></div>`;
  }
  return `<div class="function-hint">${result}</div>`
}

function createFunctionArgHtml(arg: FunctionArgument, last: boolean) {
  return `<span class="argument">${arg.name}: ${createTypeHtml(arg.type.dataType, arg.type.canBeNull)}${last ? '' : ', '}</span>`;
}

function createTypeHtml(type: DataType, canBeNull: boolean) {
  return `<span class="type">${type}${(canBeNull ? '' : '!')}</span>`;
}


