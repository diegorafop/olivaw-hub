import { expect, test } from '@playwright/test';

test('menu mobile navega sem transbordamento horizontal', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/');
  const menu = page.locator('button[aria-controls="menu-mobile"]');
  await menu.click();
  await expect(menu).toHaveAttribute('aria-expanded', 'true');
  await page
    .getByRole('navigation', { name: 'Navegação mobile' })
    .getByRole('link', { name: 'Soluções', exact: true })
    .click();
  await expect(menu).toHaveAttribute('aria-expanded', 'false');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  await page.screenshot({ path: testInfo.outputPath('landing-mobile.png'), fullPage: true });
});

test('resumo é copiado localmente sem envio à API', async ({ page, context }, testInfo) => {
  await context.grantPermissions(['clipboard-read', 'clipboard-write']);
  let apiRequests = 0;
  page.on('request', (request) => {
    if (request.url().startsWith('https://api.olivaw.com.br')) apiRequests++;
  });
  await page.goto('/');
  await page.getByLabel('Seu negócio', { exact: true }).fill('Empresa de eventos');
  await page
    .getByLabel('O que você gostaria de simplificar?', { exact: true })
    .fill('Divulgar meus serviços e minha agenda.');
  await page.getByRole('button', { name: 'Copiar resumo do projeto' }).click();
  await expect(page.getByRole('status')).toContainText('Resumo copiado');
  expect(await page.evaluate(() => navigator.clipboard.readText())).toContain('Empresa de eventos');
  expect(apiRequests).toBe(0);
  await page.goto('/');
  await page.screenshot({ path: testInfo.outputPath('landing-desktop.png'), fullPage: true });
});
