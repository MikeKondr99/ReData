import {Component, inject} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {BreadcrumbComponent} from '../components/breadcrumb.component';
import {VisitorIdService} from '../services/visitor-id.service';
import {AuthService} from '../services/auth.service';
import {NzAvatarModule} from 'ng-zorro-antd/avatar';
import {NzButtonModule} from 'ng-zorro-antd/button';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    BreadcrumbComponent,
    NzAvatarModule,
    NzButtonModule,
  ],
  template: `
    <div class="flex flex-col h-full">
      <div class="px-5 py-3 flex items-center justify-between gap-4">
        <app-breadcrumb/>
        <div class="text-sm flex items-center gap-3">
          @if (auth.user(); as user) {
            <button
              type="button"
              class="flex items-center gap-3 hover:opacity-85 transition-opacity"
              (click)="openAccountManagement()">
              <nz-avatar
                [nzSrc]="avatarUrl(user)"
                [nzText]="avatarText(user)"
                nzSize="default"
                class="bg-blue-600 text-white">
              </nz-avatar>
              <div class="leading-tight text-right">
                <div class="font-medium text-gray-900">
                  {{ user.fullName ?? user.username ?? user.email ?? 'User' }}
                </div>
                <div class="text-xs text-gray-500">
                  {{ user.email ?? user.username ?? '' }}
                </div>
              </div>
            </button>
            <button nz-button nzType="default" type="button" (click)="logout()">Logout</button>
          } @else {
            <nz-avatar nzIcon="user" nzSize="default"></nz-avatar>
            <span class="text-gray-600">Anonymous</span>
            <button nz-button nzType="default" type="button" (click)="login()">Login</button>
          }
        </div>
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
  protected auth = inject(AuthService);

  protected async login(): Promise<void> {
    await this.auth.login();
  }

  protected async logout(): Promise<void> {
    await this.auth.logout();
  }

  protected openAccountManagement(): void {
    this.auth.openAccountManagement();
  }

  protected avatarText(user: {
    firstName?: string;
    lastName?: string;
    username?: string;
    email?: string;
    picture?: string;
  }): string {
    const first = user.firstName?.[0] ?? '';
    const last = user.lastName?.[0] ?? '';
    const initials = `${first}${last}`.trim().toUpperCase();
    if (initials.length > 0) {
      return initials;
    }

    return (user.username?.[0] ?? user.email?.[0] ?? 'U').toUpperCase();
  }

  protected avatarUrl(user: {
    username?: string;
    email?: string;
    picture?: string;
  }): string | undefined {
    if (user.picture && user.picture.trim().length > 0) {
      return user.picture;
    }

    return undefined;
  }

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
