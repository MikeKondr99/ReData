import {Component, computed, effect, input, viewChild} from '@angular/core';
import {EditorComponent} from 'ngx-monaco-editor-v2';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-fx-editor',
  standalone: true,
  imports: [
    EditorComponent,
    FormsModule
  ],
  template: `
    <div class="h-[300px]">
      <ngx-monaco-editor #monacoEditor
        [options]="editorOptions"


        [(ngModel)]="code">
      </ngx-monaco-editor>
    </div>
  `,
  styles: ``
})
export class FxEditorComponent {

  monacoEditor = viewChild.required<EditorComponent>('monacoEditor');

  fields = input<string[]>();

  model = computed(() => this.monacoEditor().model);

  editorOptions = {
    language: 'lang',
    fields: this.fields(),
  };
  code = `
  A
    `;

}
