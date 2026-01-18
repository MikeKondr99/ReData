
import {Component, inject} from '@angular/core';
import {DatasetsService} from '../services/datasets.service';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {RouterLink} from '@angular/router';
import {HttpClient} from '@angular/common/http';
import {DataSetViewModel} from '../types';
import {NzTableModule} from 'ng-zorro-antd/table';
import {DatePipe, JsonPipe} from '@angular/common';
import {NzCollapseModule} from 'ng-zorro-antd/collapse';
import {CollapseComponent} from '../components/collapse.component';
import {NzPopoverModule} from 'ng-zorro-antd/popover';

@Component({
  selector: 'app-datasets-page',
  standalone: true,
  imports: [
    NzTableModule,
    NzIconModule,
    NzButtonModule,
    NzPopoverModule,
    RouterLink,
    CollapseComponent,
    DatePipe,
  ],
  template: `
    <div #header>
      <div class="text-red-500 text-l ml-5">
            Мне лень делать auth поэтому прошу вести себя прилично. Не удаляйте чужие эксперименты. По желанию подписывайтесь под своими.
      </div>
      <div class="my-3 mx-2">
        <button nz-button nzType="primary" nzShape="round" routerLink="new">
          <span nz-icon nzType="plus"></span>
          Добавить набор
        </button>
      </div>
    </div>
    <nz-table #basicTable [nzData]="datasets()" [nzShowPagination]="false" >
      <thead>
      <tr>
        <th>Название</th>
        <th>Поля</th>
        <th>Записи</th>
        <th>Дата создания</th>
        <th>Дата редактирования</th>
        <th>Действия</th>
      </tr>
      </thead>
      <tbody>
        @for (dataset of datasets(); track dataset.id) {
          <tr>
            <td>{{ dataset.name }}</td>
<!--            {{ (dataset.fieldList ?? '-') | json }}-->
            <td>
              @if(dataset.fieldList) {
                <app-collapse [header]="'Количество полей: ' + dataset.fieldList.length">
                  @for (field of dataset.fieldList; track field) {
                    <div>
                      {{field.alias}}:
                      <span class="text-blue-700">
                        {{field.dataType}}{{field.canBeNull ? '' : '!'}}
                      </span>
                    </div>
                  }

                </app-collapse>
              } @else {
                -
              }
            </td>
            <td>{{ dataset.rowsCount ?? '-' }}</td>
            <td>{{ dataset.createdAt | date:'dd.MM.yyyy hh:mm'}}</td>
            <td>{{ dataset.updatedAt | date:'dd.MM.yyyy hh:mm'}}</td>
            <td>
              <button nz-button
                      nz-popover
                      nzPopoverTitle="Экспорт"
                      nzPopoverTrigger="click"
                      nzPopoverPlacement="left"
                      [nzPopoverContent]="exportPopoverTemplate"
                      nzType="link"
                      nzShape="circle"
                >
                <span nz-icon nzType="export"></span>
              </button>
              <ng-template #exportPopoverTemplate>
                <button nz-button nzType="link" nzShape="circle" (click)="exportDataset(dataset.id,'csv')">
                  Csv
                </button>
                <button nz-button nzType="link" nzShape="circle" (click)="exportDataset(dataset.id,'excel')">
                  Excel
                </button>
                <button nz-button nzType="link" nzShape="circle" (click)="exportDataset(dataset.id,'json')">
                  Json
                </button>
              </ng-template>
              <button nz-button nzType="link" nzShape="circle" [routerLink]="[dataset.id]">
                <span nz-icon nzType="edit"></span>
              </button>
              <button nz-button nzType="link" nzDanger nzShape="circle" (click)="deleteDataset(dataset.id)">
                <span nz-icon nzType="delete"></span>
              </button>
            </td>
          </tr>

        }
      </tbody>
    </nz-table>
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

  exportDataset(id: string, fileType: string) {
    // window.open(`api/datasets/${id}/export?fileType=${fileType}`,'_blank');
    window.location.assign(`api/datasets/${id}/export?fileType=${fileType}`);
  }
}
