import {Component, effect, input, model} from '@angular/core';
import {EditorComponent} from 'ngx-monaco-editor-v2';
import {FormsModule} from '@angular/forms';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {editor, MarkerSeverity} from 'monaco-editor';
import {ExprError, IEditor, Monaco} from '../types';
import IStandaloneCodeEditor = editor.IStandaloneCodeEditor;
import ITextModel = editor.ITextModel;

@Component({
  selector: 'app-fx-input',
  standalone: true,
  imports: [
    EditorComponent,
    FormsModule,
    NzButtonModule,
    NzIconModule
  ],
  template: `
    <div class="border border-solid rounded-sm h-[32px] px-0.5 pt-[7px] pb-1  editor-container hover:border-blue-500 bg-white">
      <ngx-monaco-editor #monacoEditor class="max-h-[20px]"
        [options]="editorOptions"
        [(ngModel)]="ngModel"
        (ngModelChange)="onInputChange($event)"
        (onInit)="editorInit($event)"
      ></ngx-monaco-editor>
    </div>
  `,
  styles: `
  `
})
export class FxInputComponent {

  error = input<ExprError>();

  ngModel = model<string | undefined>('');

  editorOptions = {
    language: 'lang',
    theme: 'vs',
    // lineNumbers: (lineNumber: number) => lineNumber == 1 ? '𝑓𝑥' : lineNumber,
    lineNumbers: '',
    lineNumbersMinChars: 0,
    fontFamily: 'Fira Code',
    fontLigatures: true,
    minimap: { enabled: false }, // Disable minimap
    overviewRulerLanes: 0,
    scrollBeyondLastLine: false, // Disable extra scrolling space
    folding: false,              // Disable code folding
    overviewRulerBorder: false,  // Hide overview ruler
    scrollbar: {
      vertical: 'hidden',        // Hide vertical scrollbar
      horizontal: 'hidden',     // Hide horizontal scrollbar
      useShadows: false
    },
    unicodeHighlight: {
      ambiguousCharacters: false,
    },
    renderLineHighlight: 'none', // Disable line highlighting
    autoClosingBrackets: 'never', // Disable auto-closing brackets
    autoClosingQuotes: 'never',   // Disable auto-closing quotes
    fontSize: 14,                // Adjust font size as needed
    automaticLayout: true        // Important for proper resizing
  };

  diag = effect(() => {
    let error = this.error();


    let monaco = (<Monaco>(<any>window).monaco);
    if(this.model) {
      if(error) {
        monaco.editor.setModelMarkers(this.model, 'owner', [
          {
            message: error.message,
            startLineNumber: 1,
            endLineNumber: 1,

            startColumn: error.span.column + 1,
            endColumn: error.span.column + error.span.length + 2,
            severity: MarkerSeverity.Error,
          }]);
      } else {
        monaco.editor.setModelMarkers(this.model, 'owner', []);
      }
    }
  })

  onInputChange(newValue: string) {
    const sanitizedValue = newValue.replace(/[\r\n]+/g, '');
    this.ngModel.set(sanitizedValue);
  }

  model: ITextModel | null = null;

  editorInit(editor: IStandaloneCodeEditor) {
    // Handle Enter key
    editor.addCommand(3, () => {
      return null
    }, '');
    // Ignore F1
    editor.addCommand(59, () => {
      return null;
    });

    this.model = editor.getModel();
  }
}
