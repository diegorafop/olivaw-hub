import { expect, test } from '@playwright/test';

test('painel Trevize mostra métricas do CRM sem iniciar comunicação', async ({ page }) => {
  const writes: string[] = [];
  await page.route('https://api.olivaw.com.br/**', async (route) => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 120000).toISOString() },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: {
          name: 'Operador',
          isRoot: true,
          permissions: ['leads.manage'],
          userProfileId: 'test',
        },
      });
    if (request.method() !== 'GET') writes.push(path);
    if (path.endsWith('/communication-summary'))
      return route.fulfill({
        json: {
          inspected: 40,
          truncated: false,
          asOf: '2026-11-07T12:00:00Z',
          counts: { collected: 40, enriched: 12, noEmail: 8, pending: 2, sent: 0, suppressed: 1 },
        },
      });
    if (path === '/leads')
      return route.fulfill({
        json: { items: [], total: 0, page: 1, pageSize: 20, asOf: new Date().toISOString() },
      });
    return route.fulfill({ json: {} });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).not.toHaveURL(/\/login$/);
  await page.goto('/admin/agents');
  await page.getByRole('link', { name: /Trevize/ }).click();
  await expect(page.getByRole('heading', { name: 'Trevize', exact: true })).toBeVisible();
  await expect(
    page.getByRole('navigation', { name: 'Caminho de navegação' }).locator('[aria-current=page]'),
  ).toHaveText('Trevize');
  const metrics = page.getByRole('region', { name: 'Indicadores do Trevize' });
  await expect(
    metrics
      .locator('article')
      .filter({ has: page.getByRole('heading', { name: 'Leads de prospecção', exact: true }) }),
  ).toContainText('40');
  await expect(
    metrics
      .locator('article')
      .filter({ has: page.getByRole('heading', { name: 'Envios confirmados', exact: true }) }),
  ).toContainText('0');
  await page.getByRole('button', { name: 'Atualizar indicadores' }).click();
  expect(writes).toEqual([]);
});
