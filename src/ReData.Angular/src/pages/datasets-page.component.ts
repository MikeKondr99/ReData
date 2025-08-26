
import {Component, computed, effect, inject} from '@angular/core';
import {DatasetsService} from '../services/datasets.service';
import {toSignal} from '@angular/core/rxjs-interop';
import {NzListModule} from 'ng-zorro-antd/list';
import {NzIconDirective, NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {RouterLink} from '@angular/router';
import {HttpClient} from '@angular/common/http';
import {DataSetViewModel} from '../types';

@Component({
  selector: 'app-datasets-page',
  standalone: true,
  imports: [
    NzListModule,
    NzIconModule,
    NzButtonModule,
    RouterLink,
  ],
  template: `
        <ng-template #header>
          <span class="text-xl mr-5"> Наборы данных</span>
          <span class="text-red-500 text-l" >
            Мне лень делать auth поэтому прошу вести себя прилично. Не удаляйте чужие эксперименты. По желанию подписывайтесь под своими.
          </span>
          <div class="mt-3">
            <button nz-button nzType="primary" nzShape="round" routerLink="new">
              <span nz-icon nzType="plus"></span>
              Добавить
            </button>
          </div>
        </ng-template>
        <nz-list nzBordered class="h-full" [nzHeader]="header">
          @for (dataset of datasets(); track dataset.id) {
            <nz-list-item>
                {{ dataset.name }}
                <ul nz-list-item-actions>
                  <nz-list-item-action>
                    <button nz-button nzType="default" nzShape="circle" [routerLink]="[dataset.id]">
                      <span nz-icon nzType="edit"></span>
                    </button>
                  </nz-list-item-action>
                  <nz-list-item-action>
                  <button nz-button nzType="default" nzDanger nzShape="circle" (click)="deleteDataset(dataset.id)">
                    <span nz-icon nzType="delete"></span>
                  </button>
                  </nz-list-item-action>
                </ul>
            </nz-list-item>

          }
        </nz-list>
  `,
  styles: ``
})
export class DatasetsPage {

  datasetsService = inject(DatasetsService);
  http = inject(HttpClient);

  constructor() {
    this.datasetsService.refresh();
  }

  datasets = this.datasetsService.datasets;

  deleteDataset(id: string) {
    let _ = this.http.delete<DataSetViewModel>(`api/datasets/${id}`).subscribe(dataset => {
      this.datasetsService.refresh();
    });
  }

  protected readonly JSON = JSON;
}
