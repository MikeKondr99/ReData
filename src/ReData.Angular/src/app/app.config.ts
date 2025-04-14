import {
  APP_INITIALIZER,
  ApplicationConfig,
  Provider,
  provideZoneChangeDetection
} from '@angular/core';
import {provideRouter, withComponentInputBinding, withViewTransitions} from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient, withInterceptors,
  withInterceptorsFromDi
} from '@angular/common/http';
import {KeycloakBearerInterceptor, KeycloakService} from 'keycloak-angular';

function initializeKeycloak(keycloak: KeycloakService) {
  return () =>
    keycloak.init({
      // Configuration details for Keycloak
      config: {
        url: 'http://localhost:8080', // URL of the Keycloak server
        realm: 'Test', // Realm to be used in Keycloak
        clientId: 'workspaces-client', // Client ID for the application in Keycloak
      },
      // Options for Keycloak initialization
      initOptions: {
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri:
          window.location.origin + '/assets/silent-check-sso.html'
      },
      enableBearerInterceptor: true,
      bearerPrefix: 'Bearer',
    });
}

const KeycloakBearerInterceptorProvider: Provider = {
  provide: HTTP_INTERCEPTORS,
  useClass: KeycloakBearerInterceptor,
  multi: true
};

const KeycloakInitializerProvider: Provider = {
  provide: APP_INITIALIZER,
  useFactory: initializeKeycloak,
  multi: true,
  deps: [KeycloakService]
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimationsAsync(),
    provideHttpClient(),
    // provideZoneChangeDetection({ eventCoalescing: true }),
    // provideRouter(routes),
    // KeycloakService, // Service for Keycloak
    // KeycloakBearerInterceptorProvider, // Provides Keycloak Bearer Interceptor
    // KeycloakInitializerProvider, // Initializes Keycloak
    // provideHttpClient(withInterceptorsFromDi()), // Provides HttpClient with interceptors
    // provideRouter(routes,withViewTransitions(),withComponentInputBinding()),
    // provideAnimationsAsync(),
    // provideClientHydration(withEventReplay()), provideAnimationsAsync(),
  ]
};
