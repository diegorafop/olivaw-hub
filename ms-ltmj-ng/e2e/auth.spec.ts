import { expect, test } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  await page.route('**/portfolio/events?*', (route) => route.fulfill({ json: { items: [] } }));
});

test('rota pública e acesso direto protegido', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Você vive');
  await page.goto('/gestao');
  await expect(page).toHaveURL(/\/gestao\/entrar$/);
  await expect(page.getByLabel('Login', { exact: true })).toBeVisible();
});

test('login, acesso e logout seguem o contrato da API', async ({ page }) => {
  let logout = false;
  const authenticationCalls: string[] = [];
  await page.route('https://api.ltmj.olivaw.com.br/identity/authentication/**', async (route) => {
    expect(new URL(route.request().url()).origin).toBe('https://api.ltmj.olivaw.com.br');
    const path = new URL(route.request().url()).pathname;
    authenticationCalls.push(path);
    if (path.endsWith('/login')) {
      expect(route.request().postDataJSON()).toEqual({
        login: 'operador',
        password: 'password-exemplo',
      });
      await route.fulfill({
        json: {
          accessToken: 'token-de-teste',
          expiresAt: new Date(Date.now() + 60000).toISOString(),
        },
      });
    } else if (path.endsWith('/access')) {
      expect(route.request().headers()['authorization']).toBe('Bearer token-de-teste');
      await route.fulfill({
        json: {
          userProfileId: 'exemplo',
          name: 'Operador',
          isRoot: false,
          permissions: ['portfolio.manage'],
        },
      });
    } else if (path.endsWith('/logout')) {
      expect(route.request().headers()['authorization']).toBe('Bearer token-de-teste');
      logout = true;
      await route.fulfill({ json: {} });
    } else await route.abort();
  });
  await page.route('**/portfolio/events/management?*', (route) =>
    route.fulfill({ json: { items: [] } }),
  );
  await page.goto('/gestao/entrar');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('password-exemplo');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/gestao$/);
  await expect(page.getByText('Olá, Operador.')).toBeVisible();
  await page.getByRole('button', { name: 'Sair', exact: true }).click();
  await expect(page).toHaveURL(/\/gestao\/entrar$/);
  expect(logout).toBe(true);
  expect(authenticationCalls).toEqual([
    '/identity/authentication/login',
    '/identity/authentication/access',
    '/identity/authentication/logout',
  ]);
});

test('erro de credenciais aparece uma única vez', async ({ page }) => {
  await page.route('https://api.ltmj.olivaw.com.br/identity/authentication/login', (route) =>
    route.fulfill({ status: 401, json: { title: 'Credenciais inválidas.' } }),
  );
  await page.route('**/portfolio/events/management?*', (route) =>
    route.fulfill({ json: { items: [] } }),
  );
  await page.goto('/gestao/entrar');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('incorreta');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveCount(1);
  await expect(page.getByRole('alert')).toContainText('Credenciais inválidas');
});
