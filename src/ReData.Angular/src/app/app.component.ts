import {Component, computed, effect, inject, model, signal, untracked} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {finalize, catchError, of} from 'rxjs';
import {ApiResponse, ExprError, Field, TransformationData} from '../types';
import {ActivatedRoute, RouterLink, RouterOutlet} from '@angular/router';
import {NzBreadCrumbModule} from 'ng-zorro-antd/breadcrumb';
import {BreadcrumbComponent} from '../components/breadcrumb.component';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    BreadcrumbComponent,
  ],
  template: `
    <div class="flex flex-col h-full">
      <div class="px-5 py-3">
        <app-breadcrumb></app-breadcrumb>
      </div>
      <div class="flex-1">
        <router-outlet/>
      </div>
    </div>
  `,

  styles: `
    //.sql-editor {
    //  height: 800px;
    //  width: 600px;
    //}
    //
    //.tab-hack {
    //  display: block;
    //  max-height: calc(100vh - 50px) !important;
    //  height: calc(100vh - 50px) !important;
    //}

  `
})
export class AppComponent {

}
