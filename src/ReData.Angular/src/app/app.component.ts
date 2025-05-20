import {Component, computed, effect, inject, model, signal, untracked} from '@angular/core';
import { CommonModule } from '@angular/common';
import {NzInputModule} from 'ng-zorro-antd/input';
import {FormsModule} from '@angular/forms';
import {HttpClient} from '@angular/common/http';
import {finalize, debounce, debounceTime, catchError, of} from 'rxjs';
import {NzTableModule} from 'ng-zorro-antd/table';
import {NzIconDirective, NzIconModule} from 'ng-zorro-antd/icon';
import {NzTabsModule} from 'ng-zorro-antd/tabs';
import {EditorComponent} from 'ngx-monaco-editor-v2';
import {ApiResponse, ExprError, Field, Transformation} from '../types';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {TransformationListComponent} from '../components/transformation-list.component';
import {FunctionsComponent} from '../components/functions.component';
import {InstructionComponent} from '../components/instruction.component';
import {NzToolTipModule} from 'ng-zorro-antd/tooltip';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    NzInputModule,
    NzTableModule,
    NzIconModule,
    NzTabsModule,
    NzToolTipModule,
    FormsModule,
    EditorComponent,
    NzIconModule,
    NzButtonModule,
    TransformationListComponent,
    FunctionsComponent,
    InstructionComponent,
  ],
  template: `
    <div class="w-screen h-screen overflow-hidden flex flex-row gap-4">
      <app-transformations-list [errors]="error()" class="w-[50%]"
                                (transformationsChange)="transformationsChanged($event)"></app-transformations-list>
      <div class="max-h-screen h-screen w-[50%]">
        <nz-tabset class="max-h-screen">
          <nz-tab nzTitle="Инструкция" >
              <app-instruction class="tab-hack overflow-y-scroll"></app-instruction>
          </nz-tab>
          <nz-tab nzTitle="Данные" class="max-h-screen">
            <div class="tab-hack pr-4">
              @if (response(); as apiResponse) {
                <nz-table
                  #basicTable
                  [nzBordered]="true"
                  [nzVirtualItemSize]="55"
                  [nzVirtualMaxBufferPx]="1300"
                  [nzVirtualMinBufferPx]="1300"
                  [nzData]="apiResponse.data"
                  [nzFrontPagination]="false"
                  [nzShowPagination]="false"
                  [nzScroll]="{ y: 'calc(100vh - 160px)' }" >
                  <thead>
                  <tr>
                    @for (field of apiResponse.fields; track field.alias) {
                      <th nzWidth="175px">
                        <div class="flex flex-row flex-nowrap gap-2">
                          <div class="text-blue-700">
                            @switch (field.type) {
                              @case ('DateTime') {
                                date
                              }
                              @case ('Number') {
                                num
                              }
                              @case ('Integer') {
                                int
                              }
                              @case ('Boolean') {
                                bool
                              }
                              @case ('Text') {
                                text
                              }
                              @default {
                                unk
                              }
                            }
                            @if(!field.canBeNull) {
                              !
                            }
                          </div>
                          <span class="whitespace-pre">
                            {{ displayFieldAlias(field) }}
                          </span>
                        </div>
                      </th>
                    }
                  </tr>
                  </thead>
                  <tbody>
                    <ng-template nz-virtual-scroll let-data let-index="index">
                      <tr>
                        @for (field of apiResponse.fields; track field.alias) {
                          <td class="text-ellipsis text-nowrap overflow-hidden max-h-14 min-h-14" nz-tooltip [nzTooltipTitle]="data[field.alias]" nzTooltipPlacement="bottomLeft" [style.text-align]="textAlign(field.type)">
                            @if (data[field.alias]?.type != null) {
                              <span class="text-gray-400 italic">{{ data[field.alias].type }}</span>
                            } @else if (data[field.alias] == null) {
                              <span class="text-gray-400 italic">NULL</span>
                            } @else if (data[field.alias] === '') {
                              <span class="text-gray-400 italic">Пустая строка</span>
                            } @else {
                              @if(field.type == 'DateTime') {
                                {{ date(data[field.alias]) }}
                              } @else {
                                {{ data[field.alias] }}
                              }
                            }
                          </td>
                        }
                      </tr>
                    </ng-template>
                  </tbody>
                </nz-table>
                <div class="text-sm text-gray-500">
                  Total: {{ apiResponse.total }} records
                </div>
              }
            </div>

          </nz-tab>
          <nz-tab nzTitle="Запрос">
            <ngx-monaco-editor class="tab-hack overflow-y-scroll"
                               [ngModel]="query()"
                               [options]="{
                language: 'SQL',
                readOnly: true,
                automaticLayout: true,
                minimap: { enabled: false },
              }"
            >
            </ngx-monaco-editor>

          </nz-tab>
          <nz-tab nzTitle="Функции">
            <app-functions class="tab-hack overflow-y-hidden"></app-functions>
          </nz-tab>
        </nz-tabset>
      </div>
    </div>
  `,
  styles: `
    .sql-editor {
      height: 800px;
      width: 600px;
    }

    .tab-hack  {
      display: block;
      max-height: calc(100vh - 50px) !important;
      height: calc(100vh - 50px) !important;
    }

  `
})
export class AppComponent {
  private http = inject(HttpClient);

  transformations = signal<Transformation[]>([]);
  loading = signal(false);
  error = signal<{index: number, errors?: (ExprError | null)[], message?: string, query?: string } | null>(null);
  response = signal<ApiResponse>({ data: [], fields: [], query: '', total:0 });

  query = computed(() => {
    let response = this.response();
    let error = this.error();
    console.log(response);
    console.log(error);

    let q = error?.query ?? response?.query;
    return q;
  })


  api = effect(() => {
    let transformations = this.transformations();
    untracked(() => {
      this.loading.set(true)
      let _ = this.http.post<ApiResponse>('api/transform', { transformations }
      ).pipe(
        finalize(() => {
          this.loading.set(false);
        }),
        catchError(err => {
          console.log(err.error.message);
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

  transformationsChanged(transformations: Transformation[])
  {
    console.log("transformationsChanged");
    this.transformations.set([...transformations]);
  }

  textAlign(type: string) {
    if(type != 'Text') return 'right'
    return 'left'
  }

  date(value: string) {
    return new Date(value).toLocaleString();
  }

  displayFieldAlias(field: Field) {
    const regex = /^[a-zA-Zа-яА-Я][a-zA-Zа-яА-Я0-9]*$/;
    if(regex.test(field.alias))
      return field.alias;
    return `[${field.alias}]`
  }


  protected readonly Date = Date;
}
