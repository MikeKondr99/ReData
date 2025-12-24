
import {Component, effect, inject, model, signal, untracked} from '@angular/core';
import {DatasetsService} from '../services/datasets.service';
import {NzListModule} from 'ng-zorro-antd/list';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {
  ApiResponse,
  DataConnectorListItem,
  DataSetViewModel,
  ExprErrors,
  TransformationBlock,
  TransformationData
} from '../types';
import {catchError, finalize, of, Subject, takeUntil} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {TransformationListComponent} from '../components/transformation-list.component';
import {FormsModule} from '@angular/forms';
import {NzToolTipModule} from 'ng-zorro-antd/tooltip';
import {NzTabsModule} from 'ng-zorro-antd/tabs';
import {NzTableModule} from 'ng-zorro-antd/table';
import {NzInputModule} from 'ng-zorro-antd/input';
import {CommonModule} from '@angular/common';
import {NzFormModule} from 'ng-zorro-antd/form';
import {ActivatedRoute} from '@angular/router';
import {DataTableComponent} from '../components/data-table.component';
import {BreadcrumbsService} from '../services/breadcrumb.service';
import {DataConnectorSelectorComponent} from '../components/data-connector-selector.component';
import {NzModalModule} from 'ng-zorro-antd/modal';
import {CreateDataConnectorModalComponent} from '../components/create-data-connector-modal.component';
import bootstrap from '../main.server';

@Component({
  selector: 'dataset-edit-page',
  standalone: true,
  imports: [
    CommonModule,
    NzInputModule,
    NzFormModule,
    FormsModule,
    NzTableModule,
    NzIconModule,
    NzTabsModule,
    NzToolTipModule,
    NzIconModule,
    NzModalModule,
    NzButtonModule,
    TransformationListComponent,
    NzListModule,
    NzIconModule,
    NzButtonModule,
    DataTableComponent,
    DataConnectorSelectorComponent,
    CreateDataConnectorModalComponent,
  ],
  template: `
    <app-create-data-connector-modal [(isVisible)]="createModelOpened"></app-create-data-connector-modal>
    <div class="w-full h-full overflow-hidden flex flex-row gap-2">
      <div class="flex flex-col w-[50%] h-full pt-2">
        <nz-form-item class="w-[60%] gap-2">
          <nz-form-label [nzSm]="6" [nzXs]="24" nzRequired>Название</nz-form-label>
          <nz-form-control [nzSm]="14" [nzXs]="24">
            <input nz-input name="name" [(ngModel)]="datasetName"/>
          </nz-form-control>
          <nz-form-label [nzSm]="6" [nzXs]="24" nzRequired>Файл</nz-form-label>
          <nz-form-control>
            <div class="flex gap-2 items-center">
              <app-data-connector-selector class="flex-grow" (modelChange)="connector.set($event)" [id]="connector()?.id"></app-data-connector-selector>
              <button nz-button nzType="primary" nzShape="circle" nzSize="small" (click)="createModelOpened.set(true)">
                <nz-icon nzType="plus" />
              </button>
            </div>
          </nz-form-control>
        </nz-form-item>
        <app-transformations-list  [initialTransformations]="transformations()" [errors]="error()" class="w-full flex-1" (transformationsChange)="transformationsChanged($event)"></app-transformations-list>
        <button nz-button nzType="primary" [disabled]="!unsaved()" (click)="saveDataset()">Сохранить</button>
      </div>
      <div class=" w-[50%]">
        <div class="tab-hack pr-5">
          @if (response(); as apiResponse) {
            <app-data-table [data]="response().data" [fields]="response().fields" height="85vh"></app-data-table>
            <div class="text-sm text-gray-500 mt-2">
              Total: {{ apiResponse.total }} records
            </div>
          }
        </div>
      </div>
    </div>
  `,
  styles: `
    :host {
      height: 100%;
    }
  `
})
export class DatasetEditPage{
  private http = inject(HttpClient);
  private datasetsService = inject(DatasetsService);
  private route = inject(ActivatedRoute);
  private breadcrumbs = inject(BreadcrumbsService);


  id = this.route.snapshot.paramMap.get('id');

  dataset = this.datasetsService.getById(this.id ?? "");
  transformations = signal<TransformationBlock[]>(<any>null);
  connector = signal<DataConnectorListItem | undefined>(undefined);

  createModelOpened = model<boolean>(false);

  datasetLoaded = effect(() => {
    let dataset = this.dataset();
    if(dataset) {
      console.log('dataset loaded', dataset);
      this.transformationsChanged(dataset.transformations);
      this.datasetName.set(dataset.name);
      this.connector.set({ id: dataset.dataConnectorId, name: '' })
      this.breadcrumbs.setLastSegment(dataset.name);
    }
  }, { allowSignalWrites: true });

  unsaved = signal(false);
  datasetName = model<string>('');
  loading = signal(false);
  error = signal<{index: number, errors?: (ExprErrors | null)[], message?: string, query?: string } | null>(null);
  response = signal<ApiResponse>({ data: [], fields: [], query: '', total:0 });

  private cancelQuery = new Subject<void>();

  api = effect(() => {
    let transformations: TransformationData[] = this.transformations().filter(t => t.enabled).map(t => t.transformation);
    let connector = this.connector();

    if(transformations === null) return;

    this.cancelQuery?.next();
    this.cancelQuery = new Subject<void>();

    untracked(() => {
      this.loading.set(true);
      let _ = this.http.post<ApiResponse>('api/transform', { transformations, dataConnectorId: connector?.id }
      ).pipe(
        takeUntil(this.cancelQuery),
        finalize(() => {
          this.loading.set(false);
        }),
        catchError(err => {
          err = err.error;
          this.error.set(err);
          return of(null);
        })
      ).subscribe(res => {
        if(res != null) {
          this.response.set(res);
          this.error.set(null);
        }
      })
    })
  })

  transformationsChanged(transformations: TransformationBlock[])
  {
    console.log('transformationsChanged', transformations);
    this.unsaved.set(true);
    this.transformations.set([...transformations]);
  }

  saveDataset() {
    let datasetName = this.datasetName();
    let transformations = this.transformations();

    if(this.id == 'new') {
      let _ = this.http.post<{}>(`api/datasets/`, { name: datasetName, transformations, connectorId: this.connector()?.id ?? '00000000-0000-0000-0000-000000000000' }).subscribe(dataset => {
        this.unsaved.set(false);
      });

    } else {
      let _ = this.http.put<DataSetViewModel>(`api/datasets/${this.id}`, { id: this.id, name: datasetName, connectorId: this.connector()?.id ?? '00000000-0000-0000-0000-000000000000',  transformations }).subscribe(dataset => {
        this.unsaved.set(false);
      });

    }
  }


}
