import {ApplicationConfig, importProvidersFrom,} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { monacoConfig } from '../config/monaco.config';
import {provideMonacoEditor} from 'ngx-monaco-editor-v2';
import {provideHttpClient} from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimationsAsync(),
    provideHttpClient(),
    provideMonacoEditor(monacoConfig),
  ]
};
