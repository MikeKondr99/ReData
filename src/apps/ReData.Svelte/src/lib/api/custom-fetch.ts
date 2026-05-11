import { getToken, initAuth, login } from '$lib/auth/auth.svelte';

type CustomFetchOptions = RequestInit & {
	method?: string;
};

export const customFetch = async <T>(url: string, options: CustomFetchOptions = {}): Promise<T> => {
	await initAuth();

	const token = await getToken();
	const headers = new Headers(options.headers);

	if (token && !headers.has('Authorization')) {
		headers.set('Authorization', `Bearer ${token}`);
	}

	const response = await fetch(url, {
		...options,
		headers
	});

	if (response.status === 401) {
		void login();
	}

	const body = [204, 205, 304].includes(response.status) ? null : await response.text();
	const data = body ? JSON.parse(body) : {};

	return {
		data,
		status: response.status,
		headers: response.headers
	} as T;
};
