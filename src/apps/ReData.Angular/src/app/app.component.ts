import {Component, inject} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {BreadcrumbComponent} from '../components/breadcrumb.component';
import {VisitorIdService} from '../services/visitor-id.service';


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

  private http = inject(VisitorIdService);

  // transformations = signal<Transformation[]>([]);
  // loading = signal(false);
  // error = signal<{ index: number, errors?: (ExprError | null)[], message?: string, query?: string } | null>(null);
  // response = signal<ApiResponse>({data: [], fields: [], query: '', total: 0});
  //
  // query = computed(() => {
  //   let response = this.response();
  //   let error = this.error();
  //   console.log(response);
  //   console.log(error);
  //
  //   let q = error?.query ?? response?.query;
  //   return q;
  // })
  //
  //
  // api = effect(() => {
  //   let transformations = this.transformations();
  //   untracked(() => {
  //     this.loading.set(true)
  //     let _ = this.http.post<ApiResponse>('api/transform', {transformations}
  //     ).pipe(
  //       finalize(() => {
  //         this.loading.set(false);
  //       }),
  //       catchError(err => {
  //         console.log(err.error.message);
  //         err = err.error;
  //         this.error.set(err);
  //         return of(null);
  //       })
  //     ).subscribe(res => {
  //       if (res != null) {
  //         this.response.set(res);
  //         this.error.set(null);
  //       }
  //     })
  //   })
  // })
  //
  // transformationsChanged(transformations: Transformation[]) {
  //   console.log("transformationsChanged");
  //   this.transformations.set([...transformations]);
  // }
  //
  // textAlign(type: string) {
  //   if (type != 'Text') return 'right'
  //   return 'left'
  // }
  //
  // date(value: string) {
  //   return new Date(value).toLocaleString();
  // }
  //
  // displayFieldAlias(field: Field) {
  //   const regex = /^[a-zA-Zа-яА-Я][a-zA-Zа-яА-Я0-9]*$/;
  //   if (regex.test(field.alias))
  //     return field.alias;
  //   return `[${field.alias}]`
  // }
  //
  //
  // protected readonly Date = Date;
}
