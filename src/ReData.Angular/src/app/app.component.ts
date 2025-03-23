import { Component } from '@angular/core';
import {RouterModule} from '@angular/router';
import {NzMenuModule} from 'ng-zorro-antd/menu';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzTabsModule} from 'ng-zorro-antd/tabs';
import {NzHeaderComponent, NzLayoutModule} from 'ng-zorro-antd/layout';
import {NzPageHeaderModule} from 'ng-zorro-antd/page-header';

@Component({
  standalone: true,
  imports: [RouterModule, NzTabsModule, NzPageHeaderModule, NzLayoutModule],
  selector: 'app-root',
  template: `
    <div class="w-[100vw] flex flex-row items-end bg-gray-100 shadow-lg" >
      <div class="text-lg font-bold bg-blue-600 text-white px-4 place-self-stretch">
        <div class="flex items-center justify-center bg-blue-600">
          <span class="text-3xl font-black p-1 text-white font-sans">ReData</span>
        </div>
      </div>
      <span class="mx-10 grow">
        <nz-tabset nzLinkRouter [nzTabPosition]="'top'">
          <nz-tab>
            <a *nzTabLink nz-tab-link [routerLink]="['/']">
              Home
            </a>
          </nz-tab>
          <nz-tab>
            <a *nzTabLink nz-tab-link [routerLink]="['/datasources']">
              Data Sources
            </a>
          </nz-tab>
        </nz-tabset>
      </span>
    </div>
    <div class="p-3">
      <router-outlet/>
    </div>
  `,
  styles: [`
    :host ::ng-deep .ant-tabs-nav {
      margin: 0 !important; /* Use !important to enforce the rule */
    }
  `],
})
export class AppComponent {
  title = 'ReData.Angular';
}
