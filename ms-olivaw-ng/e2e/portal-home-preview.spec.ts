import { expect, test, type Page } from '@playwright/test';
async function login(page: Page, root = true) {
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/login'))
      return route.fulfill({
        json: {
          accessToken: 'preview-test',
          expiresAt: new Date(Date.now() + 300000).toISOString(),
        },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: {
          name: 'Operador',
          isRoot: root,
          permissions: ['leads.manage'],
          userProfileId: 'test',
        },
      });
    return route.fulfill({ json: { total: 0, recent: 0, active: 0, byStatus: {}, items: [] } });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operator');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/admin/);
  await page.goto('/portal');
}
test('prévia permite explorar períodos, limites, ausência de dados e solicitação local', async ({
  page,
}) => {
  await page.setViewportSize({ width: 1440, height: 1100 });
  await login(page);
  await expect(page.getByRole('heading', { name: 'Bom ver você por aqui.' })).toBeVisible();
  let apiCalls = 0;
  page.on('request', (request) => {
    if (request.url().startsWith('https://api.olivaw.com.br/')) apiCalls++;
  });
  const visits = page.locator('.chart-card').nth(1).locator('.chart-metric');
  const monthly = await visits.innerText();
  await page.getByRole('button', { name: 'Dia', exact: true }).click();
  await expect(visits).toContainText('328');
  await page.getByRole('button', { name: 'Semana', exact: true }).click();
  await expect(visits).not.toHaveText(monthly);
  await page.getByRole('button', { name: 'Mês', exact: true }).click();
  await expect(visits).toHaveText(monthly);
  await page.screenshot({ path: 'test-results/portal-home-desktop.png', fullPage: true });
  await page.getByLabel('Cenário').selectOption('attention');
  await expect(page.locator('.usage-note')).toContainText('92%');
  await page.getByLabel('Cenário').selectOption('missing');
  await expect(page.locator('.empty-chart')).toHaveCount(2);
  await expect(page.locator('.chart-card').first().locator('.chart-metric')).toContainText('—');
  await page.getByRole('button', { name: 'Nova solicitação', exact: true }).click();
  await page.getByLabel('Como podemos ajudar?').fill('Atualizar horário');
  await page.getByRole('button', { name: 'Simular solicitação' }).click();
  await expect(page.getByRole('status')).toContainText('Nenhum chamado real foi aberto');
  await page.getByRole('button', { name: 'Fechar', exact: true }).click();
  await expect(page.locator('.activity-grid')).toContainText('Atualizar horário');
  expect(apiCalls).toBe(0);
  await page.getByLabel('Cenário').selectOption('normal');
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(
    true,
  );
  await page.screenshot({ path: 'test-results/portal-home-mobile.png', fullPage: true });
});
test('prévia interna exige autenticação e root', async ({ page }) => {
  await page.goto('/portal');
  await expect(page).toHaveURL(/login/);
  await login(page, false);
  await expect(page).toHaveURL(/acesso-negado/);
});
