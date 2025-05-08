import {Component, EventEmitter, Output, effect, signal, input} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { debounceTime, Subject } from 'rxjs';
import {
  ExprError, isLimitTransformation,
  isOrderByTransformation, isSelectTransformation, isWhereTransformation, LimitTransformation,
  OrderByTransformation, SelectTransformation,
  Transformation,
  WhereTransformation,
} from '../types';
import {FxInputComponent} from './fx-input.component';
import {JsonPipe} from '@angular/common';
import {NzInputNumberModule} from 'ng-zorro-antd/input-number';

@Component({
  selector: 'app-transformations-list',
  standalone: true,
  imports: [
    FormsModule,
    NzButtonModule,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzSwitchModule,
    NzDividerModule,
    NzIconModule,
    NzInputNumberModule,
    FxInputComponent,
    JsonPipe
  ],
  template: `
    <div class="relative flex min-h-screen flex-col gap-3 overflow-hidden bg-gray-50 px-5 py-6 font-sans">

      @for (item of transformations; track i; let i = $index) {
        <div class="relative w-full bg-white pb-3 pl-6 pr-3 pt-3.5 shadow-xl ring-1 ring-gray-900/5 sm:rounded-lg">
          <div class="flex items-start">
            <div class="flex flex-grow flex-wrap items-baseline gap-x-2 gap-y-1.5">
              @if (isWhereTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span>Фильтр</span>
                  </div>
                  <app-fx-input ngDefaultControl class="min-w-72" [(ngModel)]="item.condition" [error]="getError(i,0)"
                                (ngModelChange)="onTransformationChange()">
                  </app-fx-input>
                </div>
              } @else if (isOrderByTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span>Сортировка</span>
                  </div>

                  @for (orderItem of item.items; track orderItem; let idx = $index) {
                    <div class="flex items-center gap-2">
                      <app-fx-input class="min-w-72" ngDefaultControl [(ngModel)]="orderItem.expression"
                                    [error]="getError(i,idx)"
                                    (ngModelChange)="onTransformationChange()">
                      </app-fx-input>
                      <nz-switch [(ngModel)]="orderItem.descending" (ngModelChange)="onTransformationChange()" ></nz-switch>
                      <span>{{ orderItem.descending ? 'DESC' : 'ASC' }}</span>
                      <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeOrderByItem(i, idx)">
                        <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                      </button>
                    </div>
                  }
                  <button nz-button nzType="default" nzShape="circle"><span nz-icon nzType="plus"
                                                                            (click)="addOrderByItem(i)"></span></button>
                </div>
              } @else if (isSelectTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span>Преобразовать</span>
                  </div>
                  @for (selectItem of item.items; track idx; let idx = $index) {
                    <div class="flex items-center gap-2">
                      <input
                        nz-input
                        class="max-w-44"
                        [(ngModel)]="selectItem.field"
                        (ngModelChange)="onTransformationChange()"
                        placeholder="Field name"
                      />
                      <span>=</span>
                      <app-fx-input ngDefaultControl class="flex-grow" [(ngModel)]="selectItem.expression"
                                    (ngModelChange)="onTransformationChange()"
                        [error]="getError(i,idx)">
                      </app-fx-input>
                      <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeSelectItem(i, idx)">
                        <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                      </button>
                    </div>
                  }
                  <button nz-button nzType="default" nzShape="circle" (click)="addSelectItem(i)">
                    <span nz-icon nzType="plus"></span>
                  </button>
                </div>
              } @else if(isLimitTransformation(item)) {
                Лимит
                <nz-input-number [(ngModel)]="item.limit" (ngModelChange)="onTransformationChange()"></nz-input-number>
                Смещение
                <nz-input-number [(ngModel)]="item.offset" (ngModelChange)="onTransformationChange()"></nz-input-number>
              }
            </div>
            <div class="ml-5">
              <button nz-button nzType="default" nzDanger nzShape="circle" (click)="removeTransformation(i)">
                <span nz-icon nzType="delete"></span>
              </button>
            </div>
          </div>
        </div>
      }
      <nz-button-group>
        <button nz-button nzType="primary" (click)="addWhereTransformation()">
          Фильтровать
          <span nz-icon nzType="filter"></span>
        </button>
        <button nz-button nzType="primary" (click)="addOrderByTransformation()">
          Сортировать
          <span class="rotate-90" nz-icon nzType="swap"></span>
        </button>
        <button nz-button nzType="primary" (click)="addSelectTransformation()">
          Преобразовать
          <span nz-icon nzType="pic-center" nzTheme="outline"></span>
        </button>
        <button nz-button nzType="primary" (click)="addLimitTransformation()">
          Ограничить
          <span nz-icon nzType="vertical-align-bottom" nzTheme="outline"></span>
        </button>
      </nz-button-group>
    </div>
  `,
})
export class TransformationListComponent {
  transformations: Transformation[] = [];

  private changesSubject = new Subject<void>();

  public errors = input<{ index: number, errors: (ExprError | null)[] } | null>(null);

  @Output() transformationsChange = new EventEmitter<Transformation[]>();

  getError(index: number, pos: number) {
    if(this.errors()?.index == index) {
      if(this.errors()?.errors) {
        return this.errors()?.errors[pos] ?? undefined;
      }
    }
    return undefined;
  }

  constructor() {
    // Debounce the changes
    this.changesSubject.pipe(debounceTime(500)).subscribe(() => {
      this.transformationsChange.emit([...this.transformations]);
    });
  }

  addWhereTransformation() {
    this.transformations.push({ $type: 'where', condition: '' } as WhereTransformation);
    this.onTransformationChange();
  }

  addOrderByTransformation() {
    this.transformations.push(
      {
        $type: 'orderBy',
          items: [{ expression: '', descending: false }]
      } as OrderByTransformation);
    this.onTransformationChange();
  }

  // Add these methods to the TransformationListComponent class
  addOrderByItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex];
    if(isOrderByTransformation(transformation)) {
      transformation.items.push({
        expression: '',
        descending: false
      })
      this.onTransformationChange();
    }
  }

  removeOrderByItem(transformationIndex: number, itemIndex: number) {
    let transformation = this.transformations[transformationIndex];
    if(isOrderByTransformation(transformation)) {
      if(transformation.items.length > itemIndex) {
        transformation.items.splice(itemIndex, 1);
        if (transformation.items.length === 0) {
          this.transformations.splice(transformationIndex, 1);
        }
        this.onTransformationChange();
      }
    }
  }

  removeTransformation(index: number) {
    this.transformations.splice(index, 1);
    this.onTransformationChange();
  }

  // Add these methods to the component class
  addSelectTransformation() {
    this.transformations.push(
      {
        $type: 'select',
        items: [{ field: '', expression: '' }]
      } as SelectTransformation);
    this.onTransformationChange();
  }

  addLimitTransformation() {
    this.transformations.push(
      {
        $type: 'limit',
        limit: 0,
        offset: 0,
      } as LimitTransformation);
    this.onTransformationChange();
  }

  addSelectItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex];
    if(isSelectTransformation(transformation)) {
      transformation.items.push({
        field: `field${transformation.items.length + 1}`,
        expression: '100',
      })
    }
    this.onTransformationChange();
  }

  removeSelectItem(transformationIndex: number, itemIndex: number) {
    let transformation = this.transformations[transformationIndex];
    if(isSelectTransformation(transformation)) {
      if(transformation.items.length > itemIndex) {
        console.log(transformation.items);
        transformation.items.splice(itemIndex, 1);

        if (transformation.items.length === 0) {
          this.transformations.splice(transformationIndex, 1);
        }
        this.onTransformationChange();
      }
    }
  }

  onTransformationChange() {
    this.changesSubject.next();
  }

  public displayErrors() {
    let result = "";
    for (const [i, e] of (this.errors()?.errors ?? []).entries()) {
      if(e !== null) {
        result += `[${i}] ${e.span.column}:${e.span.column + e.span.length} | ${e?.message}\n`;
      }
    }
    return result;
  }

  protected readonly isOrderByTransformation = isOrderByTransformation;
  protected readonly isWhereTransformation = isWhereTransformation;
  protected readonly isSelectTransformation = isSelectTransformation;
  protected readonly isLimitTransformation = isLimitTransformation;
}
