import {ApplicationConfig, importProvidersFrom,} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { monacoConfig } from '../config/monaco.config';
import {provideMonacoEditor} from 'ngx-monaco-editor-v2';
import {provideHttpClient} from '@angular/common/http';
import {provideRouter} from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(),
    provideMonacoEditor(monacoConfig),
  ]
};
