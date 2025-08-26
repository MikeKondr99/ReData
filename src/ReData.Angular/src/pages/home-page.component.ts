import {Component} from '@angular/core';
import {RouterLink} from '@angular/router';
import {NzMenuModule} from 'ng-zorro-antd/menu';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    RouterLink,
    NzMenuModule,
  ],
  template: `
    <ul nz-menu [nzMode]="'vertical'">
      <li nz-menu-item>
        <a routerLink="datasets">Наборы данных</a>
      </li>
      <li nz-menu-item>
        <a routerLink="functions">Функции</a>
      </li>
      <li nz-menu-item>
        <a routerLink="docs">Инструкция</a>
      </li>
    </ul>
  `,
  styles: ``
})
export class HomePageComponent {
}
