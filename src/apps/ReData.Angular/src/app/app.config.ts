import {APP_INITIALIZER, ApplicationConfig, importProvidersFrom,} from '@angular/core';
import { registerLocaleData } from '@angular/common';
import ru from '@angular/common/locales/en';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {provideRouter} from '@angular/router';
import { routes } from './app.routes';
import {provideMarkdown} from 'ngx-markdown';
import { provideApi } from '../api';
import { provideNzI18n, ru_RU } from 'ng-zorro-antd/i18n';
import {authInterceptor} from '../services/auth.interceptor';
import {AuthService} from '../services/auth.service';

registerLocaleData(ru);

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    provideApi( {
      basePath: '',
    }),
    provideMarkdown(),
    provideNzI18n(ru_RU),
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [AuthService],
      useFactory: (auth: AuthService) => () => auth.init(),
    }
  ]
};
