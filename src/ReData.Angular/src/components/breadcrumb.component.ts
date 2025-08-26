import {Component, effect, inject} from '@angular/core';
import {BreadcrumbsService} from '../services/breadcrumb.service';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzBreadCrumbModule} from 'ng-zorro-antd/breadcrumb';
import {RouterLink} from '@angular/router';
import {NzSkeletonComponent, NzSkeletonModule} from 'ng-zorro-antd/skeleton';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [
    NzIconModule,
    NzBreadCrumbModule,
    RouterLink,
    NzSkeletonModule,
  ],
  template: `
    <nz-breadcrumb>
      <nz-breadcrumb-item>
        <a routerLink="/">
          <span class="mr-1 " nz-icon nzType="home"></span>
          ReData
        </a>
      </nz-breadcrumb-item>
      @for (breadcrumb of breadcrumbs.path(); track breadcrumb; let last = $last) {
          <nz-breadcrumb-item [style.pointer-events]="last ? 'none' : ''">
            @if(isGuid(breadcrumb.label)) {
              <span class="bg-gray-200 rounded-md text-gray-200 whitespace-pre">ooooooooo</span>
            } @else {
              <a [routerLink]="breadcrumb.url">
                @if (breadcrumb.icon) {
                  <span class="mr-1" nz-icon [nzType]="breadcrumb.icon"></span>
                }
                <span>{{ t(breadcrumb.label) }}</span>
              </a>
            }
          </nz-breadcrumb-item>
        }
    </nz-breadcrumb>
  `
})
export class BreadcrumbComponent {

  public breadcrumbs = inject(BreadcrumbsService);

  isGuid(text:string) {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(text);
  }

  _ = effect(() => {
    console.log('breadcrumbs updated', this.breadcrumbs.path());
  })

  public t(segment: string) {
    const dict:Record<string,string> = {
      'datasets': 'Наборы данных',
      'new': 'Новый',
      'docs': 'Инструкция',
      'functions': 'Функции',
    }
    return dict[segment] ?? segment;
  }

}
