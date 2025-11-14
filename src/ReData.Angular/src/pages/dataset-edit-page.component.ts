
import {Component, computed, effect, inject, input, model, signal, untracked} from '@angular/core';
import {DatasetsService} from '../services/datasets.service';
import {NzListModule} from 'ng-zorro-antd/list';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {
  ApiResponse,
  DataSetViewModel,
  ExprError,
  Field,
  TransformationBlock,
  TransformationData,
  UploadResponse
} from '../types';
import {catchError, finalize, of, Subject, takeUntil} from 'rxjs';
import {HttpClient, HttpEventType, HttpRequest, HttpResponse} from '@angular/common/http';
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
import {NzUploadChangeParam, NzUploadFile, NzUploadModule} from 'ng-zorro-antd/upload';
import {data} from 'autoprefixer';

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
    NzButtonModule,
    TransformationListComponent,
    NzListModule,
    NzIconModule,
    NzUploadModule,
    NzButtonModule,
    DataTableComponent,
  ],
  template: `
    <div class="w-full h-full overflow-hidden flex flex-row gap-2">
      <div class="flex flex-col w-[50%] h-full pt-2">
        <nz-form-item class="w-[60%] ">
          <nz-form-label [nzSm]="6" [nzXs]="24" nzRequired nzFor="email">Название</nz-form-label>
          <nz-form-control [nzSm]="14" [nzXs]="24">
            <input nz-input name="name" [(ngModel)]="datasetName"/>
          </nz-form-control>
        </nz-form-item>
        <nz-upload
          [nzCustomRequest]="customUpload"
          [nzFileList]="fileList"
          nzListType="text"
        >
          <button nz-button>
            <span nz-icon nzType="upload"></span>
            Click to Upload
          </button>
        </nz-upload>
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

  datasetLoaded = effect(() => {
    let dataset = this.dataset();
    if(dataset) {
      this.transformationsChanged(dataset.transformations);
      this.datasetName.set(dataset.name);
      this.datasetFile.set({ fieldList: dataset.fieldList, tableId: dataset.tableId });
      this.breadcrumbs.setLastSegment(dataset.name);
    }
  }, { allowSignalWrites: true });

  fileList: NzUploadFile[] = [];

  unsaved = signal(false);
  datasetName = model<string>('');
  datasetFile= signal<UploadResponse>({ tableId: null, fieldList: null});
  loading = signal(false);
  error = signal<{index: number, errors?: (ExprError | null)[], message?: string, query?: string } | null>(null);
  response = signal<ApiResponse>({ data: [], fields: [], query: '', total:0 });

  api = effect(() => {
    let transformations: TransformationData[] = this.transformations().filter(t => t.enabled).map(t => t.transformation);
    if(transformations === null) return;
    untracked(() => {
      this.loading.set(true);
      console.log(this.datasetFile());
      let _ = this.http.post<ApiResponse>('api/transform', { tableId: this.datasetFile().tableId, fieldList: this.datasetFile().fieldList, transformations }
      ).pipe(
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
    this.unsaved.set(true);
    this.transformations.set([...transformations]);
  }

  saveDataset() {
    let datasetName = this.datasetName();
    let datasetFile = this.datasetFile();
    let transformations = this.transformations();

    if(this.id == 'new') {
      let _ = this.http.post<{}>(`api/datasets/`, { name: datasetName, transformations, tableId: datasetFile.tableId, fieldList: datasetFile.fieldList }).subscribe(dataset => {
        this.unsaved.set(false);
      });

    } else {
      let _ = this.http.put<DataSetViewModel>(`api/datasets/${this.id}`, { id: this.id, name: datasetName, transformations, tableId: datasetFile.tableId, fieldList: datasetFile.fieldList }).subscribe(dataset => {
        this.unsaved.set(false);
      });

    }


  }

  customUpload = (item: any) => {
    const formData = new FormData();
    formData.append('file', item.file);

    const req = new HttpRequest('POST', '/api/datasets/upload', formData, {
      reportProgress: true,
    });

    return this.http.request<UploadResponse>(req).subscribe(res => {
      if(res instanceof HttpResponse) {
        const body: UploadResponse | null = res.body;
        if(body) {
          this.datasetFile.set(body);
        }
      }

    })

  }
}
