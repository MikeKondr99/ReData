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
import {ApiResponse, ExprError, Transformation} from '../types';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {TransformationListComponent} from '../components/transformation-list.component';
import {FunctionsComponent} from '../components/functions.component';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    NzInputModule,
    NzTableModule,
    NzIconModule,
    NzTabsModule,
    FormsModule,
    EditorComponent,
    NzIconModule,
    NzButtonModule,
    TransformationListComponent,
    FunctionsComponent,
  ],
  template: `
    <div class="w-screen h-screen flex flex-row gap-4 overflow-hidden">
      <app-transformations-list [errors]="error()"  class="basis-1/2"
                                (transformationsChange)="transformationsChanged($event)"></app-transformations-list>
    <div class="max-w-screen-xl max-h-screen">
      <nz-tabset class="max-h-screen">
        <nz-tab nzTitle="Данные" class="max-h-screen">
          <div class="overflow-s">
            @if (response(); as apiResponse) {
              <nz-table #basicTable [nzData]="apiResponse.data" [nzLoading]="loading()" [nzScroll]="{ y: ' 1000px' }" [nzFrontPagination]="false">
                <thead>
                <tr>
                  @for (field of apiResponse.fields; track field.alias) {
                    <th nzWidth="50px">
                      <div class="flex flex-row flex-nowrap gap-2">
                        <div class="text-blue-700">
                          @switch (field.type) {
                            @case ('DateTime') {
                              dat
                            }
                            @case ('Number') {
                              num
                            }
                            @case ('Boolean') {
                              bool
                            }
                            @case ('Integer') {
                              int
                            }
                            @case ('Boolean') {
                              bl
                            }
                            @case ('Text') {
                              txt
                            }
                            @default {
                              ?
                            }
                          }
                        </div>
                        {{ field.alias }}
                      </div>
                    </th>
                  }
                </tr>
                </thead>
                <tbody >
                <tr *ngFor="let data of basicTable.data">
                  @for (field of apiResponse.fields; track field.alias) {
                    <td>
                      @if (data[field.alias]?.type != null) {
                        <span class="text-gray-400 italic">{{ data[field.alias].type }}</span>
                      } @else if (data[field.alias] == null) {
                        <span class="text-gray-400 italic">NULL</span>
                      } @else if (data[field.alias] === '') {
                        <span class="text-gray-400 italic">Пустая строка</span>
                      } @else {
                        {{ data[field.alias] }}
                      }
                    </td>
                  }
                </tr>
                </tbody>
              </nz-table>
              <div class="text-sm text-gray-500">
                Total: {{ apiResponse.total }} records
              </div>
            }
          </div>

        </nz-tab>
        <nz-tab nzTitle="Запрос">
          <ngx-monaco-editor class="sql-editor"
                             [ngModel]="response().query.join('\n')"
                             [options]="{
                language: 'SQL',
                readonly: true,
                automaticLayout: true,
                height: '800px',
                width: '800px'

              }"
          >
          </ngx-monaco-editor>

        </nz-tab>
        <nz-tab nzTitle="Функции">
          <app-functions class="max-h-screen"></app-functions>
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
  `
})
export class AppComponent {
  private http = inject(HttpClient);

  transformations = signal<Transformation[]>([]);
  loading = signal(false);
  error = signal<{index: number, errors: (ExprError | null)[] } | null>(null);
  response = signal<ApiResponse>({ data: [], fields: [], query: [], total:0 });

  errorEf = effect(() => {
    console.log(this.error());
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
          this.error.set({ index: err.error.index, errors: err.error.errors });
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


}
