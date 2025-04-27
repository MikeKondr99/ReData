
import {Component, computed, effect, inject, input, model, viewChild} from '@angular/core';
import {EditorComponent} from 'ngx-monaco-editor-v2';
import {FormsModule} from '@angular/forms';
import {FunctionService} from '../services/function.service';
import {NzListModule} from 'ng-zorro-antd/list';
import {NzCardModule} from 'ng-zorro-antd/card';
import {DataType, FunctionArgument, FunctionArgumentType, FunctionViewModel} from '../types';
import {Data} from '@angular/router';
import {NzInputModule} from 'ng-zorro-antd/input';
import {NzIconModule} from 'ng-zorro-antd/icon';

@Component({
  selector: 'app-functions',
  standalone: true,
  imports: [
    EditorComponent,
    FormsModule,
    NzListModule,
    NzCardModule,
    NzInputModule,
    NzIconModule,
  ],
  template: `
    <nz-input-group [nzSuffix]="suffixIconSearch">
      <input type="text" nz-input placeholder="input search text" [(ngModel)]="search" />
    </nz-input-group>
    <ng-template #suffixIconSearch>
      <span nz-icon nzType="search" ></span>
    </ng-template>
    <nz-list class="overflow-y-scroll max-h-screen">
      @for (f of data(); track f) {
        <nz-list-item>
          <nz-card [nzTitle]="displayFunc(f)" >{{ f.doc }}</nz-card>
        </nz-list-item>
      }
    </nz-list>
  `,
  styles: ``
})
export class FunctionsComponent {

  functions = inject(FunctionService);

  search = model<string>('');

  data = computed(() => {
    return this.functions.data()
      ?.filter(f => f.name.includes(this.search()))

  })

  displayFunc(func: FunctionViewModel): string {
    if(func.kind == 'Binary') {
      return `(${this.displayArgType(func.arguments[0].type)} ${func.name} ${this.displayArgType(func.arguments[1].type)}) -> ${this.displayArgType(func.returnType)}`;
    } else if(func.kind == 'Unary') {
      return `(${func.name} ${this.displayArgType(func.arguments[0].type)}) -> ${this.displayArgType(func.returnType)}`;
    } else {
      return `${func.name}(${func.arguments.map(a => this.displayArg(a)).join(', ')}) -> ${this.displayArgType(func.returnType)}`;
    }
  }

  displayArg(arg: FunctionArgument): string {
   return `${arg.name}:${this.displayArgType(arg.type)}`;
  }

  displayArgType(argType: { dataType: DataType, canBeNull: boolean }): string {
    return `${this.displayDataType(argType.dataType)}${argType.canBeNull ? '' : '!'}`;
  }

  displayDataType(type: DataType) {
    let map: Record<DataType, string> = {
      'Bool': 'bool',
      'Integer': 'int',
      'Null': 'null',
      'DateTime': 'date',
      'Number': 'num',
      'Text': 'text',
      'Unknown': 'unknown'
    }
    return map[type];
  }

}
