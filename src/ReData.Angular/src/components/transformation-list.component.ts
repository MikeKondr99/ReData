import {Component, EventEmitter, Output, effect, signal, input, output} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { debounceTime, Subject } from 'rxjs';
import {CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList, moveItemInArray} from '@angular/cdk/drag-drop';
import {
  ExprError, isGroupByTransformation, isLimitTransformation,
  isOrderByTransformation, isSelectTransformation, isWhereTransformation, LimitTransformation,
  OrderByTransformation, SelectTransformation,
  Transformation,
  WhereTransformation,
} from '../types';
import {FxInputComponent} from './fx-input.component';
import {NzInputNumberModule} from 'ng-zorro-antd/input-number';
import {NzCheckboxModule} from 'ng-zorro-antd/checkbox';

@Component({
  selector: 'app-transformations-list',
  standalone: true,
  imports: [
    FormsModule,
    NzButtonModule,
    NzFormModule,
    NzCheckboxModule,
    NzInputModule,
    NzSelectModule,
    NzSwitchModule,
    NzDividerModule,
    NzIconModule,
    NzInputNumberModule,
    FxInputComponent,
    CdkDropList,
    CdkDrag,
    CdkDragHandle
  ],
  styles: `
    .cdk-drag-preview {
      opacity: 0.5;
    }

    .cdk-drag-placeholder {
      opacity: 0.3;
    }
  `,
  template: `
    <div class="relative flex min-h-screen flex-col gap-3 overflow-hidden bg-gray-50 px-5 py-6 font-sans" cdkDropList (cdkDropListDropped)="drop($event)">

      @for (item of transformations; track i; let i = $index) {
        <div class="relative w-full bg-white pb-3 pl-6 pr-3 pt-3.5 shadow-xl ring-1 ring-gray-900/5 sm:rounded-lg border-l-8 border-blue-500"
             [class.border-red-400]="hasErrors(i)"
             [class.border-gray-200]="!item.enabled"
             cdkDrag cdkDragLockAxis="y">
          <div class="flex items-start">
            <div class="flex flex-grow flex-wrap items-baseline gap-x-2 gap-y-1.5">
              @if (isWhereTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                    <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                    <span>Фильтр</span>
                  </div>
                  <app-fx-input ngDefaultControl class="min-w-72 max-w-[25rem]" [(ngModel)]="item.condition" [error]="getError(i,0)"
                                (ngModelChange)="onTransformationChange()">
                  </app-fx-input>
                </div>
              } @else if (isOrderByTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                    <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
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
                      <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'items', idx)">
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
                    <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                    <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                    <span>Преобразовать</span>
                  </div>
                  @for (selectItem of item.items; track idx; let idx = $index) {
                    <div class="flex items-center gap-2">
                      <span class="max-w-44 min-w-32">
                        <input
                          nz-input
                          [(ngModel)]="selectItem.field"
                          (ngModelChange)="onTransformationChange()"
                          placeholder="Field name"
                        />
                      </span>
                      <span>=</span>
                      <app-fx-input ngDefaultControl class="min-w-[25rem]" [(ngModel)]="selectItem.expression"
                                    (ngModelChange)="onTransformationChange()"
                        [error]="getError(i,idx)">
                      </app-fx-input>
                      <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'items', idx)">
                        <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                      </button>
                    </div>
                  }
                  <button nz-button nzType="default" nzShape="circle" (click)="addSelectItem(i)">
                    <span nz-icon nzType="plus"></span>
                  </button>
                </div>
              } @else if (isGroupByTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                    <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                    <span>Группировка</span>
                  </div>
                  Для группы
                  @for (selectItem of item.groups; track idx; let idx = $index) {
                    <div class="flex items-center gap-2">
                      <span class="max-w-44 min-w-32">
                        <input
                          nz-input
                          [(ngModel)]="selectItem.field"
                          (ngModelChange)="onTransformationChange()"
                          placeholder="Field name"
                        />
                      </span>
                      <span>=</span>
                      <app-fx-input ngDefaultControl class="min-w-[25rem]" [(ngModel)]="selectItem.expression"
                                    (ngModelChange)="onTransformationChange()"
                                    [error]="getError(i, idx + item.groups.length) ?? getError(i,idx)">
                      </app-fx-input>
                      <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'groups', idx)">
                        <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                      </button>
                    </div>
                  }
                  <button nz-button nzType="default" nzShape="circle" (click)="addGroupItem(i)">
                    <span nz-icon nzType="plus"></span>
                  </button>
                  Расчитать
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
                      <app-fx-input ngDefaultControl class="min-w-[25rem] max-w-[25rem]" [(ngModel)]="selectItem.expression"
                                    (ngModelChange)="onTransformationChange()"
                                    [error]="getError(i,item.groups.length * 2 + idx)">
                      </app-fx-input>
                      <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'items', idx)">
                        <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                      </button>
                    </div>
                  }
                  <button nz-button nzType="default" nzShape="circle" (click)="addSelectItem(i)">
                    <span nz-icon nzType="plus"></span>
                  </button>
                </div>
              } @else if(isLimitTransformation(item)) {
                <div class="flex flex-col gap-2 w-full">
                  <div class="flex items-center gap-2">
                    <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                    <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                    <span>Ограничить</span>
                  </div>
                  <div>
                    Взять только
                    <nz-input-number [(ngModel)]="item.limit" (ngModelChange)="onTransformationChange()"></nz-input-number>
                    записей
                  </div>

                  <div>
                    Со смещением в
                    <nz-input-number [(ngModel)]="item.offset" (ngModelChange)="onTransformationChange()"></nz-input-number>
                    записей

                  </div>
                </div>
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
      @if(errors()?.message) {
        <pre class="text-red-500 whitespace-pre-line relative w-full bg-white pb-3 pl-6 pr-3 pt-3.5 shadow-xl ring-1 ring-gray-900/5 sm:rounded-lg">
          {{ errors()?.message }}
        </pre>
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
        <button nz-button nzType="primary" (click)="addGroupByTransformation()">
          Сгруппировать
          <span nz-icon nzType="group" nzTheme="outline"></span>
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

  transformations: Transformation[] = JSON.parse(localStorage.getItem('transformations') ?? '[]')

  private changesSubject = new Subject<void>();

  public errors = input<{ index: number, errors?: (ExprError | null)[], message?: string } | null>(null);

  transformationsChange = output<Transformation[]>();

  hasErrors(index: number) {
    let transformations = this.transformations;

    const enabledIndices = transformations
      .map((t, i) => t.enabled ? i : -1)  // keep original index if enabled, -1 otherwise
      .filter(i => i !== -1);            // remove disabled ones
    index = enabledIndices.indexOf(index);

    let errors = this.errors();
    if(errors?.index == index) {
      if(errors?.errors) {
        return true;
      }
    }
    return false;
  }

  getError(index: number, pos: number) {
    let transformations = this.transformations;

    const enabledIndices = transformations
      .map((t, i) => t.enabled ? i : -1)  // keep original index if enabled, -1 otherwise
      .filter(i => i !== -1);            // remove disabled ones
    index = enabledIndices.indexOf(index);

    let errors = this.errors();
    if(errors?.index == index) {
      if(errors?.errors) {
        return errors?.errors[pos] ?? undefined;
      }
    }
    return undefined;
  }

  constructor() {
    // Debounce the changes
    this.changesSubject.pipe(debounceTime(500)).subscribe(() => {

      console.log(this.transformations);
      this.transformationsChange.emit([...this.transformations.filter(t => t.enabled)]);
    });
  }

  addWhereTransformation() {
    this.transformations.push({ $type: 'where', condition: '\'10\' = 10.Text()' , enabled: true });
    this.onTransformationChange();
  }

  addOrderByTransformation() {
    this.transformations.push(
      {
        $type: 'orderBy',
        items: [{ expression: 'Now()', descending: false }],
        enabled: true
      });
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

  removeTransformation(index: number) {
    this.transformations.splice(index, 1);
    this.onTransformationChange();
  }

  // Add these methods to the component class
  addSelectTransformation() {
    this.transformations.push(
      {
        $type: 'select',
        items: [{ field: 'Поле1', expression: '100' }],
        enabled: true,
      });
    this.onTransformationChange();
  }

  // Add these methods to the component class
  addGroupByTransformation() {
    this.transformations.push(
      {
        $type: 'groupBy',
        groups: [{ field: 'Группа1', expression: "[Поле1]"}],
        items: [{ field: 'Поле1', expression: 'COUNT(1)' }],
        enabled: true,
      });
    this.onTransformationChange();
  }

  addLimitTransformation() {
    this.transformations.push(
      {
        $type: 'limit',
        limit: 0,
        offset: 0,
        enabled: true,
      });
    this.onTransformationChange();
  }

  addSelectItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex];
    if(isSelectTransformation(transformation) || isGroupByTransformation(transformation)) {
      transformation.items.push({
        field: `Поле${transformation.items.length + 1}`,
        expression: '100',
      })
    }
    this.onTransformationChange();
  }

  addGroupItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex];
    if(isGroupByTransformation(transformation)) {
      transformation.groups.push({
        field: `Группа${transformation.groups.length + 1}`,
        expression: '[Поле]',
      })
    }
    this.onTransformationChange();
  }

  removeItem(transformationIndex: number, key: string, index: number)
  {
    let transformation = this.transformations[transformationIndex];
    let items = (transformation as any)[key] as never[];
    if(items.length > index) {
      items.splice(index, 1);

      if (items.length === 0) {
        this.transformations.splice(transformationIndex, 1);
      }
      this.onTransformationChange();
    }
  }


  onTransformationChange() {
    localStorage.setItem('transformations', JSON.stringify(this.transformations));
    this.changesSubject.next();
  }

  drop(event: CdkDragDrop<any[]>): void {
    if(event.previousIndex !== event.currentIndex) {
      moveItemInArray(this.transformations, event.previousIndex, event.currentIndex);
      this.onTransformationChange();
    }
  }

  toggle(index: number, value: boolean) {
    this.transformations[index].enabled = value;
    this.onTransformationChange();
  }

  protected readonly isOrderByTransformation = isOrderByTransformation;
  protected readonly isWhereTransformation = isWhereTransformation;
  protected readonly isSelectTransformation = isSelectTransformation;
  protected readonly isLimitTransformation = isLimitTransformation;
  protected readonly isGroupByTransformation = isGroupByTransformation;
}
