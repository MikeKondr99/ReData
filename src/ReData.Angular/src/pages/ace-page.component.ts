
import {Component} from '@angular/core';
import {NzMenuModule} from 'ng-zorro-antd/menu';
import {CdkDrag} from '@angular/cdk/drag-drop';
import {ReactiveFormsModule} from '@angular/forms';
import {AceEditorComponent} from '../components/ace-editor.component';
import {NzInputDirective, NzInputModule} from 'ng-zorro-antd/input';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    NzMenuModule,
    NzInputModule,
    AceEditorComponent,
    ReactiveFormsModule,
    CdkDrag,
    NzInputDirective,
  ],
  template: `
    <div class="relative w-full bg-white pb-3 pl-6 pr-3 pt-3.5 shadow-xl ring-1 ring-gray-900/5 sm:rounded-lg border-l-8 border-blue-500 m-4 w-[500px]"
         cdkDrag cdkDragLockAxis="y">
      <app-ace-editor></app-ace-editor>
      <input
        nz-input
        class="max-w-44"
        placeholder="Field name"
      />
    </div>
  `,
  styles: ``
})
export class AcePageComponent {
}
