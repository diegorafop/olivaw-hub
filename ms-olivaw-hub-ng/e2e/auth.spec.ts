import { expect, test } from '@playwright/test';

test('rota pública e acesso direto protegido', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Seu negócio cresce.');
  await page.goto('/admin');
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByLabel('Login', { exact: true })).toBeVisible();
});

test('login, acesso e logout seguem o contrato da API', async ({ page }) => {
  let logout = false;
  await page.route('https://api.olivaw-hub.duckdns.org/identity/autenticacao/**', async (route) => {
    expect(new URL(route.request().url()).origin).toBe('https://api.olivaw-hub.duckdns.org');
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/login')) {
      expect(route.request().postDataJSON()).toEqual({ login: 'operador', senha: 'senha-exemplo' });
      await route.fulfill({
        json: {
          accessToken: 'token-de-teste',
          expiresAt: new Date(Date.now() + 60000).toISOString(),
        },
      });
    } else if (path.endsWith('/acesso')) {
      expect(route.request().headers()['authorization']).toBe('Bearer token-de-teste');
      await route.fulfill({
        json: { usuarioId: 'exemplo', nome: 'Operador', root: false, autorizacoes: [] },
      });
    } else if (path.endsWith('/logout')) {
      expect(route.request().headers()['authorization']).toBe('Bearer token-de-teste');
      logout = true;
      await route.fulfill({ json: {} });
    } else await route.abort();
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('senha-exemplo');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/admin$/);
  await expect(page.getByText('Olá, Operador.')).toBeVisible();
  await page.getByRole('button', { name: 'Sair', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  expect(logout).toBe(true);
});

test('erro de credenciais aparece uma única vez', async ({ page }) => {
  await page.route('https://api.olivaw-hub.duckdns.org/identity/autenticacao/login', (route) =>
    route.fulfill({ status: 401, json: { title: 'Credenciais inválidas.' } }),
  );
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('incorreta');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveCount(1);
  await expect(page.getByRole('alert')).toContainText('Credenciais inválidas');
});
