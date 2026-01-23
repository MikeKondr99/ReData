import {Component, computed, inject, input, output, SecurityContext} from '@angular/core';
import {NzTableModule, NzTableQueryParams} from 'ng-zorro-antd/table';
import {ApiResponse, DataType, Field} from '../types';
import {TableCellComponent} from './table-cell.component';
import {NzDropDownModule} from 'ng-zorro-antd/dropdown';
import {NzInputModule} from 'ng-zorro-antd/input';
import {FormsModule} from '@angular/forms';
import {DomSanitizer} from '@angular/platform-browser';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    NzTableModule,
    TableCellComponent,
  ],
  template: `
    @let data = dataResponse().data;
    @let total = dataResponse().total;

    <ng-template #rangeTemplate let-range="range" let-total>
      {{ range[0] }}-{{ range[1] }} of {{ total }} items
    </ng-template>

    <nz-table
      #basicTable
      nzShowSizeChanger
      [nzBordered]="true"
      [nzData]="data"
      [nzFrontPagination]="false"
      [nzShowPagination]="true"
      (nzQueryParams)="tableQueryParamsChange.emit($event)"
      [nzPageSize]="50"
      [nzTotal]="total"
      nzSize = 'small'
      [nzShowTotal]="rangeTemplate"
      [nzScroll]="{ y: height() }">
      <thead>
      <tr>
        @for (field of fields(); track field.alias) {
          <th [nzWidth]="'174px'" [nzSortFn]="true" [nzShowSort]="field.type !== 'bool'" [nzFilterFn]="true" >
            <div class="flex flex-row flex-nowrap">
              <span class="text-blue-700">
                {{ fieldType(field) }}
              </span>
              <span class="whitespace-pre">
                {{ fieldAlias(field) }}
              </span>
            </div>
          </th>
        }
      </tr>
      </thead>
      <tbody>
      <ng-template nz-virtual-scroll let-data let-index="index">
        <tr>
          @for (field of fields(); track field) {
            <td class="" appTableCell [value]="data[field.alias]" [field]="field" [style.background]="domSanitizer.sanitize(SecurityContext.STYLE, data[field.alias + '!bg'])">
            </td>
          }
        </tr>
      </ng-template>
      </tbody>
    </nz-table>
  `
})
export class DataTableComponent {


  dataResponse = input.required<ApiResponse>();
  height = input.required<string>();
  domSanitizer = inject(DomSanitizer);


  public fields = computed(() => {
    return this.dataResponse().fields.filter(f => !f.alias.includes('!bg'));
  })

  tableQueryParamsChange = output<NzTableQueryParams>();

  date(value: string) {
    const date = new Date(value);
    const isMidnight = date.getHours() === 0 && date.getMinutes() === 0 && date.getSeconds() === 0;

    if (isMidnight) {
      return date.toLocaleDateString();
    } else {
      return date.toLocaleString();
    }
  }

  textAlign(type: DataType) {
    if(type != 'text') return 'right'
    return 'left'
  }

  fieldType(field: Field) {
    return `${field.type}${field.canBeNull ? '' : '!'}`;
  }

  fieldAlias(field: Field) {
    const regex = /^[a-zA-Zа-яА-Я_][a-zA-Zа-яА-Я0-9_]*$/;
    if(regex.test(field.alias))
      return field.alias;
    return `[${field.alias}]`
  }

  protected readonly SecurityContext = SecurityContext;
}

