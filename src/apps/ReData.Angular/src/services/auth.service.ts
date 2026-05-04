import {Injectable, signal} from '@angular/core';
import Keycloak from 'keycloak-js';

type UserDebug = {
  username?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  picture?: string;
};

@Injectable({providedIn: 'root'})
export class AuthService {
  private keycloak?: Keycloak;
  private initialized = false;
  private userSource = signal<UserDebug | null>(null);

  public user = this.userSource.asReadonly();

  async init(): Promise<void> {
    if (this.initialized) {
      return;
    }

    const keycloakUrl = (window as Window & { __KEYCLOAK_URL__?: string }).__KEYCLOAK_URL__ ?? 'http://localhost:8080';

    this.keycloak = new Keycloak({
      url: keycloakUrl,
      realm: 'redata',
      clientId: 'angular-client',
    });

    try {
      await this.keycloak.init({
        onLoad: 'check-sso',
        checkLoginIframe: false,
        pkceMethod: 'S256',
        responseMode: 'query',
      });
    } catch {
      // keep app usable in dev even if keycloak is not configured yet
      this.initialized = true;
      return;
    }

    this.initialized = true;
    await this.refreshUser();
  }

  async login(): Promise<void> {
    if (!this.keycloak) {
      return;
    }

    const redirectUri = window.location.origin;

    await this.keycloak.login({
      redirectUri,
    });
  }

  async logout(): Promise<void> {
    if (!this.keycloak) {
      return;
    }

    await this.keycloak.logout({
      redirectUri: window.location.origin,
    });
    this.userSource.set(null);
  }

  openAccountManagement(): void {
    if (!this.keycloak || !this.keycloak.authenticated) {
      return;
    }

    window.location.href = this.keycloak.createAccountUrl({
      redirectUri: window.location.origin,
    });
  }

  async getToken(): Promise<string | null> {
    if (!this.keycloak || !this.keycloak.authenticated) {
      return null;
    }

    try {
      await this.keycloak.updateToken(30);
      return this.keycloak.token ?? null;
    } catch {
      return null;
    }
  }

  isAuthenticated(): boolean {
    return !!this.keycloak?.authenticated;
  }

  private async refreshUser(): Promise<void> {
    if (!this.keycloak || !this.keycloak.authenticated) {
      this.userSource.set(null);
      return;
    }

    try {
      const profile = await this.keycloak.loadUserProfile();
      this.userSource.set({
        username: profile.username,
        email: profile.email,
        firstName: profile.firstName,
        lastName: profile.lastName,
        fullName: [profile.firstName, profile.lastName].filter(Boolean).join(' ') || undefined,
        picture: this.keycloak.tokenParsed?.['picture'] as string | undefined,
      });
    } catch {
      this.userSource.set({
        username: this.keycloak.tokenParsed?.['preferred_username'] as string | undefined,
        email: this.keycloak.tokenParsed?.['email'] as string | undefined,
        picture: this.keycloak.tokenParsed?.['picture'] as string | undefined,
      });
    }
  }
}
