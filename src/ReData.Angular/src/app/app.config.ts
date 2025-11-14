import {ApplicationConfig, importProvidersFrom,} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {provideHttpClient} from '@angular/common/http';
import {provideRouter} from '@angular/router';
import { routes } from './app.routes';
import { provideNzI18n, en_US, ru_RU} from 'ng-zorro-antd/i18n';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(),
    provideNzI18n(ru_RU)
  ]
};
