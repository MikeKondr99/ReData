import {Component, ElementRef, input, Renderer2, TemplateRef} from '@angular/core';
import {CommonModule} from '@angular/common';
import {DataType, Field} from '../types';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {NzModalService} from 'ng-zorro-antd/modal';


@Component({
  selector: 'td [appTableCell]',
  standalone: true,
  imports: [CommonModule, NzIconModule, NzButtonModule],
  styles: `
    :host {
      text-overflow: ellipsis;
      white-space: nowrap;
      overflow: hidden;
      height: 100%;
      width: 100%;
      position: relative;
    }

    .text-content {
      display: block;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      width: 100%;
    }

    :host(:hover) .text-content.text {
      padding-right: 16px; /* Space for icon when visible */
    }

    .icon-container {
      position: absolute;
      right: 0.5rem;
      top: 50%;
      transform: translateY(-50%);
      opacity: 0;
      padding-left: 4px; /* Small gap from text */
      transition: opacity 0.2s ease;
    }

    :host(:hover) .icon-container {
      opacity: 1;
    }
  `,
  template: `
    @let value = this.value();
    @let type = field().type;

    <span class="cell-content flex" [style.text-align]="textAlign(type)">
      <span class="text-content" [class.text]="type === 'text'">
        @if (value?.type != null) {
          <span class="text-red-800 italic">{{ value.type }}</span>
        } @else if (value === null) {
          <span class="text-red-800 italic">NULL</span>
        } @else if (value === '') {
          <span class="text-gray-500 italic">Пустая строка</span>
        } @else if (type == 'date') {
          {{ date(value) }}
        } @else {
          {{ value }}
        }
      </span>
      @if (type === 'text' && value) {
        @defer (when hoverLoad) {
          <button class="icon-container" nz-button nzType="link" nzShape="circle" nzSize="small" (click)="showModal(modal, title, footer)">
            <span nz-icon nzType="file-search"></span>
          </button>
        }
      }
    </span>


    <ng-template #title>
      <span nz-icon nzType="file-search"></span>
      {{ field().alias }}
    </ng-template>
    <ng-template #modal >
      <p class="whitespace-pre overflow-auto">{{ value }}</p>
    </ng-template>
    <ng-template #footer>
    </ng-template>
  `
})
export class TableCellComponent {
  public hoverLoad = false;

  constructor(private elementRef: ElementRef, private renderer: Renderer2, private modalService: NzModalService) {
    this.renderer.listen(this.elementRef.nativeElement, 'mouseenter', () => {
      this.hoverLoad = true;
    });
  }


  public field = input.required<Field>();

  public value = input.required<any>();

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
    if (type != 'text') return 'right'
    return 'left'
  }

  showModal(template: TemplateRef<{}>, title: TemplateRef<{}>, footer: TemplateRef<{}>): void {
    this.modalService.create({
      nzTitle: title,
      nzContent: template,
      nzFooter: footer,
      nzIconType: 'file-search',
    });
  }
}
