import {Component, computed, effect, input, model, viewChild} from '@angular/core';
import {EditorComponent} from 'ngx-monaco-editor-v2';
import {FormsModule} from '@angular/forms';
import {NzButtonComponent, NzButtonModule} from 'ng-zorro-antd/button';
import {NzIconDirective, NzIconModule} from 'ng-zorro-antd/icon';
import {editor} from 'monaco-editor';
import IEditor = editor.IEditor;
import EditorOption = editor.EditorOption;
import IEditorOptions = editor.IEditorOptions;

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
    renderLineHighlight: 'none', // Disable line highlighting
    autoClosingBrackets: 'never', // Disable auto-closing brackets
    autoClosingQuotes: 'never',   // Disable auto-closing quotes
    fontSize: 14,                // Adjust font size as needed
    automaticLayout: true        // Important for proper resizing
  };


  onInputChange(newValue: string) {
    const sanitizedValue = newValue.replace(/[\r\n]+/g, '');
    this.ngModel.set(sanitizedValue);
  }

  editorInit($event: any) {
    // Handle Enter key
    $event.addCommand(3, () => {
      return null
    }, '');
    // Ignore F1
    $event.addCommand(59, () => {
      return null;
    });
  }
}
