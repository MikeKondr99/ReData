import {Component, computed, inject, model} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {FunctionService} from '../services/function.service';
import {NzListModule} from 'ng-zorro-antd/list';
import {NzCardModule} from 'ng-zorro-antd/card';
import {NzInputModule} from 'ng-zorro-antd/input';
import {NzIconModule} from 'ng-zorro-antd/icon';

@Component({
  selector: 'app-functions',
  standalone: true,
  imports: [
    FormsModule,
    NzListModule,
    NzCardModule,
    NzInputModule,
    NzIconModule,
  ],
  template: `
    <div class="h-full">
      <nz-input-group [nzSuffix]="suffixIconSearch">
        <input type="text" nz-input placeholder="input search text" [(ngModel)]="search" />
      </nz-input-group>
      <ng-template #suffixIconSearch>
        <span nz-icon nzType="search" ></span>
      </ng-template>
      <nz-list>
        @for (f of data(); track f) {
          <nz-list-item>
              <nz-card-meta
                [nzTitle]="f.sign"
                [nzDescription]="f.doc"
              ></nz-card-meta>
          </nz-list-item>
        }
      </nz-list>
    </div>
  `,
  styles: ``
})
export class FunctionsComponent {


  functions = inject(FunctionService);

  search = model<string>('');

  data = computed(() => {
    let search = this.search().toLowerCase();
    return this.functions.data()
      ?.map(f => ({ sign: f.displayText, doc: f.doc ?? ''}))
      ?.filter(f =>
        f.sign.toLowerCase().includes(search)
        || f.doc.toLowerCase().includes(search)
      )
  })
}
