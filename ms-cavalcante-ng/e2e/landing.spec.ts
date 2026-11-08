import { expect, test } from '@playwright/test';

test('filtra a frota e mantém o contexto no orçamento sem API', async ({ page }) => {
  const apiCalls: string[] = [];
  page.on('request', (request) => {
    if (request.url().includes('/identity/')) apiCalls.push(request.url());
  });
  await page.goto('/');
  await expect(page.locator('.machine')).toHaveCount(5);
  await page.getByRole('button', { name: 'Espaços compactos', exact: true }).click();
  await expect(page.locator('.machine')).toHaveCount(3);
  await expect(page.getByRole('button', { name: 'Espaços compactos' })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await page.getByRole('button', { name: 'Transporte', exact: true }).click();
  await expect(page.locator('.machine')).toHaveCount(1);
  const href = await page
    .getByRole('link', { name: 'Consultar Caminhão caçamba' })
    .getAttribute('href');
  expect(new URL(href!).hostname).toBe('wa.me');
  expect(new URL(href!).searchParams.get('text')).toContain('Caminhão caçamba');
  await page.getByRole('button', { name: 'Todas', exact: true }).click();
  await expect(page.locator('.machine')).toHaveCount(5);
  await page
    .locator('summary')
    .filter({ hasText: 'A locação inclui operador e transporte?' })
    .click();
  await expect(
    page.getByText('Converse com a equipe para confirmar operador', { exact: false }),
  ).toBeVisible();
  expect(apiCalls).toEqual([]);
});

test('navega em celular sem cortar conteúdo e exibe as fotos', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/');
  const menu = page.locator('.menu-toggle');
  await menu.click();
  await expect(menu).toHaveAttribute('aria-expanded', 'true');
  await page.getByRole('navigation').getByRole('link', { name: 'Máquinas', exact: true }).click();
  await expect(menu).toHaveAttribute('aria-expanded', 'false');
  await expect(page).toHaveURL(/#maquinas$/);
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(
    true,
  );
  for (const image of await page.locator('.machine-image img').all()) {
    await image.scrollIntoViewIfNeeded();
    await expect
      .poll(() =>
        image.evaluate((element: HTMLImageElement) => element.complete && element.naturalWidth > 0),
      )
      .toBe(true);
  }
});
