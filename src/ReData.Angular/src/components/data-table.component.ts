import {Component, inject, input} from '@angular/core';
import {BreadcrumbsService} from '../services/breadcrumb.service';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzBreadCrumbModule} from 'ng-zorro-antd/breadcrumb';
import {RouterLink} from '@angular/router';
import {
  NzTableCellDirective,
  NzTableComponent,
  NzTableVirtualScrollDirective,
  NzTbodyComponent, NzTheadComponent, NzThMeasureDirective, NzTrDirective
} from 'ng-zorro-antd/table';
import {NzTooltipDirective} from 'ng-zorro-antd/tooltip';
import { Field } from '../types';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    NzTableCellDirective,
    NzTableComponent,
    NzTableVirtualScrollDirective,
    NzTbodyComponent,
    NzThMeasureDirective,
    NzTheadComponent,
    NzTooltipDirective,
    NzTrDirective
  ],
  template: `
    <nz-table
      #basicTable
      [nzBordered]="true"
      [nzVirtualItemSize]="54"
      [nzVirtualMaxBufferPx]="1299"
      [nzVirtualMinBufferPx]="1299"
      [nzData]="data()"
      [nzFrontPagination]="false"
      [nzShowPagination]="false"
      [nzScroll]="{ y: height() }">
      <thead>
      <tr>
        @for (field of fields(); track field.alias) {
          <th nzWidth="174px">
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
          @for (field of fields(); track field.alias) {
            <td class="text-ellipsis text-nowrap overflow-hidden max-h-15 min-h-14" nz-tooltip
                [nzTooltipTitle]="data[field.alias]" nzTooltipPlacement="bottomLeft"
                [style.text-align]="textAlign(field.type)">
              @if (data[field.alias]?.type != null) {
                <span class="text-red-800 italic">{{ data[field.alias].type }}</span>
              } @else if (data[field.alias] == null) {
                <span class="text-red-800 italic">NULL</span>
              } @else if (data[field.alias] === '') {
                <span class="text-gray-500 italic">Пустая строка</span>
              } @else {
                @if (field.type == 'DateTime') {
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
  `
})
export class DataTableComponent {

  data = input.required<any[]>();
  fields = input.required<Field[]>();
  height = input.required<string>();

  date(value: string) {
    return new Date(value).toLocaleString();
  }

  textAlign(type: string) {
    if(type != 'Text') return 'right'
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
}
