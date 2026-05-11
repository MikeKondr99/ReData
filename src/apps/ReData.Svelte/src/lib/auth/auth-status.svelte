<script lang="ts">
	import { Button, Dropdown, DropdownItem } from 'flowbite-svelte';
	import { getAuthState, login, logout } from './auth.svelte';

	const auth = getAuthState();

	function initials(name?: string, email?: string): string {
		const source = name?.trim() || email?.trim() || 'U';
		const parts = source.split(/\s+/).filter(Boolean);
		if (parts.length >= 2) {
			return `${parts[0][0] ?? ''}${parts[1][0] ?? ''}`.toUpperCase();
		}

		return source.slice(0, 2).toUpperCase();
	}
</script>

<div class="flex items-center gap-2">
	{#if auth.authenticated}
		<div class="text-right leading-tight">
			<div class="text-sm font-medium text-slate-900">{auth.user?.fullName ?? auth.user?.username ?? 'Выполнен вход'}</div>
			<div class="text-xs text-slate-500">{auth.user?.email ?? 'Авторизованный пользователь'}</div>
		</div>
		<button
			id="auth-user-menu"
			type="button"
			title="Меню профиля"
			class="flex h-9 w-9 cursor-pointer items-center justify-center overflow-hidden rounded-full border border-slate-200 bg-slate-100 transition hover:border-slate-300 hover:bg-slate-200"
		>
			{#if auth.user?.picture}
				<img src={auth.user.picture} alt="User avatar" class="pointer-events-none h-full w-full object-cover" />
			{:else}
				<div class="pointer-events-none flex h-full w-full items-center justify-center text-[10px] font-semibold text-slate-700">
					{initials(auth.user?.fullName ?? auth.user?.username, auth.user?.email)}
				</div>
			{/if}
		</button>
		<Dropdown placement="bottom-end" triggeredBy="#auth-user-menu" class="list-none">
			<DropdownItem class="list-none text-slate-400" onclick={(event) => event.preventDefault()}>
				Профиль (WIP)
			</DropdownItem>
			<DropdownItem class="list-none" onclick={() => logout()}>Выйти</DropdownItem>
		</Dropdown>
	{:else}
		<div class="text-right leading-tight text-slate-500">
			<div class="text-sm font-medium text-slate-900">Гость</div>
			<div class="text-xs">Вход через Keycloak</div>
		</div>
		<Button size="sm" onclick={() => login()}>Войти</Button>
	{/if}
</div>
