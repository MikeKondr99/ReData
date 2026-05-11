import Keycloak from 'keycloak-js';

type UserDebug = {
	username?: string;
	email?: string;
	firstName?: string;
	lastName?: string;
	fullName?: string;
	picture?: string;
};

type AuthState = {
	initialized: boolean;
	authenticated: boolean;
	user: UserDebug | null;
	errorMessage: string | null;
};

const authState = $state<AuthState>({
	initialized: false,
	authenticated: false,
	user: null,
	errorMessage: null
});

let keycloak: Keycloak | undefined;
let initPromise: Promise<void> | null = null;

export function getAuthState(): AuthState {
	return authState;
}

export async function initAuth(): Promise<void> {
	if (authState.initialized) {
		return;
	}

	if (initPromise) {
		return initPromise;
	}

	initPromise = (async () => {
		const keycloakUrl =
			import.meta.env.VITE_KEYCLOAK_URL ??
			(window as Window & { __KEYCLOAK_URL__?: string }).__KEYCLOAK_URL__ ??
			`${window.location.origin}/kc`;

		keycloak = new Keycloak({
			url: keycloakUrl,
			realm: 'redata',
			clientId: 'svelte-client'
		});

		try {
			await keycloak.init({
				onLoad: 'login-required',
				checkLoginIframe: false,
				pkceMethod: 'S256',
				responseMode: 'query'
			});

			cleanupOidcQueryParams();
			await refreshUser();
			authState.errorMessage = null;
		} catch {
			authState.errorMessage = 'Не удалось инициализировать вход через Keycloak.';
		}

		authState.initialized = true;
		authState.authenticated = !!keycloak?.authenticated;
	})().finally(() => {
		initPromise = null;
	});

	return initPromise;
}

export async function login(): Promise<void> {
	if (!keycloak) {
		await initAuth();
	}

	if (!keycloak) {
		return;
	}

	await keycloak.login({
		redirectUri: window.location.href
	});
}

export async function logout(): Promise<void> {
	if (!keycloak) {
		return;
	}

	await keycloak.logout({
		redirectUri: window.location.origin
	});

	authState.authenticated = false;
	authState.user = null;
}

export function openAccountManagement(): void {
	if (!keycloak || !keycloak.authenticated) {
		return;
	}

	window.location.href = keycloak.createAccountUrl({
		redirectUri: window.location.href
	});
}

export async function getToken(): Promise<string | null> {
	if (!keycloak || !keycloak.authenticated) {
		return null;
	}

	try {
		await keycloak.updateToken(30);
		return keycloak.token ?? null;
	} catch {
		return null;
	}
}

async function refreshUser(): Promise<void> {
	if (!keycloak || !keycloak.authenticated) {
		authState.user = null;
		authState.authenticated = false;
		return;
	}

	try {
		const profile = await keycloak.loadUserProfile();
		authState.user = {
			username: profile.username,
			email: profile.email,
			firstName: profile.firstName,
			lastName: profile.lastName,
			fullName: [profile.firstName, profile.lastName].filter(Boolean).join(' ') || undefined,
			picture: keycloak.tokenParsed?.['picture'] as string | undefined
		};
	} catch {
		authState.user = {
			username: keycloak.tokenParsed?.['preferred_username'] as string | undefined,
			email: keycloak.tokenParsed?.['email'] as string | undefined,
			picture: keycloak.tokenParsed?.['picture'] as string | undefined
		};
	}

	authState.authenticated = true;
}

function cleanupOidcQueryParams(): void {
	if (typeof window === 'undefined') {
		return;
	}

	const url = new URL(window.location.href);
	const oidcParams = ['state', 'session_state', 'code', 'iss', 'error', 'error_description'];
	let changed = false;

	for (const param of oidcParams) {
		if (url.searchParams.has(param)) {
			url.searchParams.delete(param);
			changed = true;
		}
	}

	if (!changed) {
		return;
	}

	const nextUrl = `${url.pathname}${url.search}${url.hash}`;
	window.history.replaceState(window.history.state, '', nextUrl);
}
