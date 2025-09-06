
// editor.component.ts
import {
  Component,
  ElementRef,
  AfterViewInit,
  OnDestroy,
   effect, viewChild, input, output
} from '@angular/core';
import * as ace from 'ace-builds';
import { Ace } from 'ace-builds';
import '../services/relang';
import 'ace-builds/src-noconflict/theme-eclipse';
import 'ace-builds/src-noconflict/ext-language_tools'
import {ExprError, FunctionArgument, FunctionViewModel} from '../types';

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

  // _ = effect(() => {
  //   let funcs = this.functions.data();
  //   if(funcs) {
  //     this.editor?.setOptions({
  //       enableBasicAutocompletion: [this.getCompleter(['name', 'age', 'super field'], funcs)],
  //       enableLiveAutocompletion: [this.getCompleter(['name', 'age', 'super field'], funcs)],
  //     })
  //
  //   }
  // })

  public editorElement = viewChild<ElementRef>('editor', );

  public value = input<string>('');

  public error = input<ExprError>();

  public valueChange = output<string>();





  updateValue = effect(() => {
    let value = this.value();
    if(this.editor?.getValue() !== value) {
      this.editor?.setValue(value);
      this.editor?.clearSelection();
    }
    // this.valueChange.emit(this.value());


  });


  updateError = effect(() => {
    let error = this.error();
    if(error) {
      console.log(error);
      this.editor?.getSession().setAnnotations([{
        row: error.span.startRow - 1,
        column: error.span.startColumn - 1,
        text: error.message,
        type: "error", // Shows in gutter and hover
      }]);
      if(this.markerId) {
        this.editor?.getSession().removeMarker(this.markerId);
        this.markerId = undefined;
      }
      this.markerId =  this.editor?.getSession().addMarker(new ace.Range(error.span.startRow - 1, error.span.startColumn, error.span.endRow - 1, error.span.endColumn), "ace-warning", "text", false);
    } else {
      this.editor?.getSession().setAnnotations([]);
      if(this.markerId) {
        this.editor?.getSession().removeMarker(this.markerId);
        this.markerId = undefined;
      }
    }
    this.updateGutter();
  })

  private markerId?: number;
  private editor?: Ace.Editor;
  private functionList: FunctionViewModel[] = [];

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
      // enableBasicAutocompletion: false,
      // enableLiveAutocompletion: false,
      // enableSnippets: true,
    });
    this.editor.clearSelection();
    this.editor.session.on('change', () => {
      let newValue = this.editor?.getValue() ?? '';
      this.updateGutter();
      this.valueChange.emit(newValue);

    });


    // this.editor.getSession().setAnnotations([
    //   {
    //     row: 1,
    //     column: 5,
    //     text: "This is an error message",
    //     type: "error", // Shows in gutter and hover
    //   }
    // ])
  }

  updateGutter() {
    this.editor?.setOptions({
      showGutter: /[\r\n]/.test(this.editor?.getValue()) || !!this.markerId
    })
  }

  ngOnDestroy() {
    if (this.editor) {
      this.editor.destroy();
    }
  }

  // getCompleter(fields: string[], funcs: FunctionViewModel[]): Ace.Completer {
  //   var completions:Ace.Completion[] = [];
  //   var methodCompletions:Ace.Completion[] = [];
  //   const regex = /^[a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*$/;
  //
  //   for (let field of fields) {
  //     if(regex.test(field)) {
  //       completions.push({
  //         caption: field,
  //         value: field,
  //         meta: "variable",
  //         score: 10
  //       });
  //     }
  //     completions.push({
  //       caption: field,
  //       value: `[${field}]`,
  //       meta: "variable",
  //       score: 10
  //     });
  //   }
  //
  //   for(let func of funcs.filter(f => f.name[0].toLowerCase() != f.name[0].toUpperCase())) {
  //     completions.push({
  //       caption: `${func.name}`,
  //       snippet: `${func.name}(${func.arguments.map((a,i) => `\${${i+1}:${a.name}}`).join(', ')})`,
  //       meta: "function",
  //       score: 20
  //     });
  //     if(func.kind === 'Method' && func.arguments.length > 0) {
  //       methodCompletions.push({
  //         caption: `${func.name}`,
  //         snippet: `${func.name}(${(func.arguments.slice(1)).map((a,i) => `\${${i+1}:${a.name}}`).join(', ')})`,
  //         meta: "function",
  //         score: 20
  //       });
  //     }
  //   }
  //
  //   console.log(`creating new Completer with ${completions.length} completions`);
  //   console.log(completions);
  //
  //     return {
  //       getCompletions: function(editor: Ace.Editor, session: Ace.EditSession, pos: Ace.Position, prefix: string, callback: Ace.CompleterCallback) {
  //         const line = session.getLine(pos.row);
  //         const cursor = pos.column;
  //
  //         const isAfterDot = (line:string, cursor:number) => {
  //           // Look backwards from cursor to find if there's a dot before
  //           for (let i = cursor - 1; i >= 0; i--) {
  //             const char = line.charAt(i);
  //             if (char === '.') {
  //               return true;
  //             }
  //             if (char === ' ' || char === '\t') {
  //               continue; // skip whitespace
  //             }
  //             if (char === '\n' || char === ';' || char === '(') {
  //               break; // we hit the beginning of expression
  //             }
  //           }
  //           return false;
  //         };
  //
  //
  //         // Check if we're after a dot (for method completion)
  //         const isMethodCall = isAfterDot(line, cursor);
  //
  //         if (isMethodCall) {
  //           console.log('🔵 Method call context detected');
  //           // Return method completions with modified signatures
  //           callback(null, methodCompletions);
  //         } else {
  //           console.log('🔵 Regular context');
  //           // Return regular completions
  //           callback(null, completions);
  //         }
  //       },
  //     }
  //   }
}


