import {ApplicationConfig, importProvidersFrom,} from '@angular/core';
import { registerLocaleData } from '@angular/common';
import ru from '@angular/common/locales/en';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient} from '@angular/common/http';
import {provideRouter} from '@angular/router';
import { routes } from './app.routes';
import {provideMarkdown} from 'ngx-markdown';
import { provideApi } from '../api';
import { provideNzI18n, ru_RU } from 'ng-zorro-antd/i18n';

registerLocaleData(ru);

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideAnimationsAsync(),
    provideApi( {
      basePath: '',
    }),
    provideMarkdown(),
    provideNzI18n(ru_RU)
  ]
};
