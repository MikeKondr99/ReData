import {Component, effect, input, output, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {NzFormModule} from 'ng-zorro-antd/form';
import {NzInputModule} from 'ng-zorro-antd/input';
import {NzSelectModule} from 'ng-zorro-antd/select';
import {NzSwitchModule} from 'ng-zorro-antd/switch';
import {NzDividerModule} from 'ng-zorro-antd/divider';
import {NzIconModule} from 'ng-zorro-antd/icon';
import { NzTypographyModule } from 'ng-zorro-antd/typography';
import {debounceTime, Subject} from 'rxjs';
import {CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList, moveItemInArray} from '@angular/cdk/drag-drop';
import {
  ExprError, isGroupByTransformation, isLimitTransformation,
  isOrderByTransformation, isSelectTransformation, isWhereTransformation,
  TransformationBlock
} from '../types';
import {NzInputNumberModule} from 'ng-zorro-antd/input-number';
import {NzCheckboxModule} from 'ng-zorro-antd/checkbox';
import {AceEditorComponent} from './ace-editor.component';
import {NzSpaceModule} from 'ng-zorro-antd/space';

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
    NzTypographyModule,
    CdkDropList,
    CdkDrag,
    CdkDragHandle,
    AceEditorComponent,
    NzSpaceModule
  ],
  styles: `
    .cdk-drag-preview {
      opacity: 0.5;
    }

    .cdk-drag-placeholder {
      opacity: 0.3;
    }

    :host {
      height: 100%;
    }
    .mini-select {
      width: fit-content;
    }
    .mini-select.ant-select-focused {
      width: 150px;
    }
  `,
  template: `
    <div class="h-full">
      <nz-space-compact class="mb-3 ml-3">
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
          <span nz-icon nzType="pic-center" nzTheme="outline"></span>
        </button>
        <button nz-button nzType="primary" (click)="addLimitTransformation()">
          Ограничить
          <span nz-icon nzType="vertical-align-bottom" nzTheme="outline"></span>
        </button>
      </nz-space-compact>
      <div cdkDropList (cdkDropListDropped)="drop($event)"
           class="flex h-full overflow-y-auto flex-col gap-3 max-h-[79vh] bg-gray-50 px-5 py-6 font-sans">
        @for (item of transformations; track i; let i = $index) {
          <div
            class="relative w-full bg-white pb-3 pl-6 pr-3 pt-3.5 shadow-xl ring-1 ring-gray-900/5 sm:rounded-lg border-l-8 border-blue-500"
            [class.border-red-400]="hasErrors(i)"
            [class.border-gray-200]="!item.enabled"
            cdkDrag cdkDragLockAxis="y">
            <div class="flex items-start">
              <div class="flex flex-grow flex-wrap items-baseline gap-x-2 gap-y-1.5">
                @if (isWhereTransformation(item.transformation)) {
                  <div class="flex flex-col gap-2 w-full">
                    <div class="flex items-center gap-2">
                      <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                      <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                      <span>Фильтр</span>
                    </div>
                    <app-ace-editor [(value)]="item.transformation.condition"
                                    (valueChange)="onTransformationChange()"
                                    [fields]="fields()[i]"
                                    [error]="getError(i,0)"></app-ace-editor>
                  </div>
                } @else if (isOrderByTransformation(item.transformation)) {
                  <div class="flex flex-col gap-2 w-full">
                    <div class="flex items-center gap-2">
                      <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                      <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                      <span>Сортировка</span>
                    </div>

                    @for (orderItem of item.transformation.items; track orderItem; let idx = $index) {
                      <div class="flex items-center gap-2">
                        <app-ace-editor class="min-w-[250px] max-w-[250px]"
                                        [(value)]="orderItem.expression"
                                        [error]="getError(i,idx)"
                                        (valueChange)="onTransformationChange()"
                                        [fields]="fields()[i]"
                        ></app-ace-editor>
                        <nz-switch [(ngModel)]="orderItem.descending"
                                   (ngModelChange)="onTransformationChange()"></nz-switch>
                        <span>{{ orderItem.descending ? 'DESC' : 'ASC' }}</span>
                        <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'items', idx)">
                          <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                        </button>
                      </div>
                    }
                    <button nz-button nzType="default" nzShape="circle"><span nz-icon nzType="plus"
                                                                              (click)="addOrderByItem(i)"></span>
                    </button>
                  </div>
                } @else if (isSelectTransformation(item.transformation)) {
                  <div class="flex flex-col gap-2 w-full">
                    <div class="flex items-center gap-2">
                      <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                      <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                      <span>Преобразовать</span>
                    </div>
                    @for (selectItem of item.transformation.items; track idx; let idx = $index) {
                      <div class="flex items-start gap-1">
                        <input
                          nz-input
                          class="max-w-44 min-w-44"
                          [nzSize]="'small'"
                          [(ngModel)]="selectItem.field"
                          (ngModelChange)="onTransformationChange()"
                          placeholder="Field name"
                        />
                        <nz-select class="mini-select" nzSize="small" nzSuffixIcon="1">
                          <nz-option nzValue="Equal" nzLabel="=" ></nz-option>
                          <nz-option nzValue="Rename" nzLabel="Заменить"></nz-option>
                          <nz-option nzValue="Rename" nzLabel="Удалить"></nz-option>
                        </nz-select>

                        <app-ace-editor class="w-[550px]"
                                        [(value)]="selectItem.expression"
                                        (valueChange)="onTransformationChange()"
                                        [error]="getError(i,idx)"
                                        [fields]="fields()[i]"
                        >
                        </app-ace-editor>
                        <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'items', idx)">
                          <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                        </button>
                      </div>
                    }
                    <button nz-button nzType="default" nzShape="circle" nzSize="small" (click)="addSelectItem(i)">
                      <span nz-icon nzType="plus"></span>
                    </button>
                    <span class="flex gap-2 items-center">
                      <span>Остальное</span>
                      <nz-select class="w-36" [(ngModel)]="item.transformation.restOptions"
                                 (ngModelChange)="onTransformationChange()">
                        <nz-option nzValue="NoAction" nzLabel="Оставить"></nz-option>
                        <nz-option nzValue="Delete" nzLabel="Удалить"></nz-option>
                      </nz-select>
                    </span>
                  </div>
                } @else if (isGroupByTransformation(item.transformation)) {
                  <div class="flex flex-col gap-2 w-full">
                    <div class="flex items-center gap-2">
                      <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                      <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                      <span>Преобразовать</span>
                      <!--                      <p nz-typography nzEditable [(nzContent)]="item.description" [nzEditTooltip]="null" class="mb-0 -ml-1 text-gray-200" [nzExpandable]="false" ></p>-->
                    </div>
                    Группы
                    @for (selectItem of item.transformation.groups; track idx; let idx = $index) {
                      <div class="flex gap-1 items-start">
                        <input
                          nz-input
                          class="max-w-44"
                          [nzSize]="'small'"
                          [(ngModel)]="selectItem.field"
                          (ngModelChange)="onTransformationChange()"
                          placeholder="Field name"
                        />
                        <span>=</span>
                        <app-ace-editor class="w-[550px]"
                                        [(value)]="selectItem.expression"
                                        (valueChange)="onTransformationChange()"
                                        [error]="getError(i,idx + item.transformation.groups.length) ?? getError(i,idx)"
                                        [fields]="fields()[i]"
                        >
                        </app-ace-editor>
                        <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'groups', idx)">
                          <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                        </button>
                      </div>
                    }
                    <button nz-button nzType="default" nzShape="circle" nzSize="small" (click)="addGroupItem(i)">
                      <span nz-icon nzType="plus"></span>
                    </button>
                    Агрегации
                    @for (selectItem of item.transformation.items; track idx; let idx = $index) {
                      <div class="flex items-start gap-1">
                        <input
                          nz-input
                          class="max-w-44"
                          nzSize="small"
                          [(ngModel)]="selectItem.field"
                          (ngModelChange)="onTransformationChange()"
                          placeholder="Field name"
                        />
                        <span>=</span>
                        <app-ace-editor class="w-[550px]"
                                        [(value)]="selectItem.expression"
                                        (valueChange)="onTransformationChange()"
                                        [error]="getError(i,item.transformation.items.length + idx)"
                                        [fields]="fields()[i]"
                        >
                        </app-ace-editor>
                        <button nz-button nzType="text" nzDanger nzSize="small" (click)="removeItem(i,'items', idx)">
                          <span nz-icon nzType="close-circle" nzTheme="outline"></span>
                        </button>
                      </div>
                    }
                    <button nz-button nzType="default" nzShape="circle" nzSize="small" (click)="addSelectItem(i)">
                      <span nz-icon nzType="plus"></span>
                    </button>
                  </div>
                } @else if (isLimitTransformation(item.transformation)) {
                  <div class="flex flex-col gap-2 w-full">
                    <div class="flex items-center gap-2">
                      <span nz-icon nzType="drag" nzTheme="outline" cdkDragHandle class="cursor-grab"></span>
                      <label nz-checkbox (nzCheckedChange)="toggle(i, $event)" [ngModel]="item.enabled"></label>
                      <span>Ограничить</span>
                    </div>
                    <div>
                      Взять только
                      <nz-input-number [(ngModel)]="item.transformation.limit"
                                       (ngModelChange)="onTransformationChange()"></nz-input-number>
                      записей
                    </div>

                    <div>
                      Со смещением в
                      <nz-input-number [(ngModel)]="item.transformation.offset"
                                       (ngModelChange)="onTransformationChange()"></nz-input-number>
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
        @if (errors()?.message) {
          <pre
            class="text-red-500 whitespace-pre-line relative w-full bg-white pb-3 pl-6 pr-3 pt-3.5 shadow-xl ring-1 ring-gray-900/5 sm:rounded-lg">
          {{ errors()?.message }}
        </pre>
        }
      </div>
    </div>
  `,
})
export class TransformationListComponent {


  transformations: TransformationBlock[] = <any>null;

  private changesSubject = new Subject<void>();
  transformationsChange = output<TransformationBlock[]>();

  public errors = input<{ index: number, errors?: (ExprError | null)[], message?: string } | null>(null);

  public fields = signal<string[][]>([])

  initialTransformations = input.required<TransformationBlock[]>()

  applyInitialTransformations = effect(() => {
    if (this.transformations === null) {
      console.log('initial transformations loaded',this.initialTransformations());
      this.transformations = this.initialTransformations();
      this.onTransformationChange();
    }
  }, {allowSignalWrites: true})

  hasErrors(index: number) {
    let transformations = this.transformations;

    const enabledIndices = transformations
      .map((t, i) => t.enabled ? i : -1)  // keep original index if enabled, -1 otherwise
      .filter(i => i !== -1);            // remove disabled ones
    index = enabledIndices.indexOf(index);

    let errors = this.errors();
    if (errors?.index == index) {
      if (errors?.errors) {
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
    if (errors?.index == index) {
      if (errors?.errors) {
        return errors?.errors[pos] ?? undefined;
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
    this.transformations ??= [];
    this.transformations.push({
      enabled: true,
      transformation: {
        $type: 'where',
        condition: '\'10\' = 10.Text()',
      },
    });
    this.onTransformationChange();
  }

  addOrderByTransformation() {
    this.transformations ??= [];
    this.transformations.push(
      {
        enabled: true,
        transformation: {
          $type: 'orderBy',
          items: [{expression: '10', descending: false}],
        },
      });
    this.onTransformationChange();
  }

  // Add these methods to the TransformationListComponent class
  addOrderByItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex].transformation;
    if (isOrderByTransformation(transformation)) {
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
    this.transformations ??= [];
    this.transformations.push(
      {
        enabled: true,
        transformation: {
          $type: 'select',
          items: [{field: 'Поле1', expression: '100'}],
          restOptions: "NoAction"
        },
      });
    this.onTransformationChange();
  }

  // Add these methods to the component class
  addGroupByTransformation() {
    this.transformations ??= [];
    this.transformations.push(
      {
        enabled: true,
        transformation: {
          $type: 'groupBy',
          groups: [{field: 'Группа1', expression: "[Поле1]"}],
          items: [{field: 'Поле1', expression: 'SUM(1)'}],
        }
      });
    this.onTransformationChange();
  }

  addLimitTransformation() {
    this.transformations ??= [];
    this.transformations.push(
      {
        enabled: true,
        transformation: {
          $type: 'limit',
          limit: 0,
          offset: 0,
        },
      });
    this.onTransformationChange();
  }

  addSelectItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex].transformation;
    if (isSelectTransformation(transformation) || isGroupByTransformation(transformation)) {
      transformation.items.push({
        field: `Поле${transformation.items.length + 1}`,
        expression: '100',
      })
    }
    this.onTransformationChange();
  }

  addGroupItem(transformationIndex: number) {
    let transformation = this.transformations[transformationIndex].transformation;
    if (isGroupByTransformation(transformation)) {
      transformation.groups.push({
        field: `Группа${transformation.groups.length + 1}`,
        expression: '[Поле]',
      })
    }
    this.onTransformationChange();
  }

  removeItem(transformationIndex: number, key: string, index: number) {
    let transformation = this.transformations[transformationIndex].transformation;
    let items = (transformation as any)[key] as never[];
    if (items.length > index) {
      items.splice(index, 1);

      if (items.length === 0) {
        this.transformations.splice(transformationIndex, 1);
      }
      this.onTransformationChange();
    }
  }


  onTransformationChange() {
    this.changesSubject.next();
    this.updateFields();
  }

  drop(event: CdkDragDrop<any[]>): void {
    if (event.previousIndex !== event.currentIndex) {
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

  updateFields() {
    const fields = [["id", "customer_name", "email", "age", "account_balance", "is_active", "signup_date", "last_login", "customer_category", "random_number", "notes", "purchase_count"]];

    for (const tr of this.transformations) {

      if(isSelectTransformation(tr.transformation) && tr.enabled) {
        fields.push(tr.transformation.items.map(i => i.field));
      } else if(isGroupByTransformation(tr.transformation) && tr.enabled) {
        fields.push([...tr.transformation.groups.map(i => i.field), ...tr.transformation.items.map(i => i.field)]);
      } else {
        fields.push(fields[fields.length-1]);
      }
    }
    this.fields.set(fields)
    console.log(fields);
  }
}
