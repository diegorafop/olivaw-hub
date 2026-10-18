import { expect, test } from '@playwright/test';

test('menu mobile e marca oficial sem transbordamento', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.route('**/portfolio/events?*', (route) =>
    route.fulfill({
      json: {
        items: [
          {
            id: 'churrasco',
            title: 'Churrasco de confraternização',
            description: 'Churrasco da LTMJ',
            category: 'Confraternizações',
            isVisible: true,
            photos: Array.from({ length: 12 }, (_, index) => ({
              id: String(index),
              url: `assets/events/churrasco-confraternizacao/churrasco${String(index + 1).padStart(2, '0')}.jpeg`,
            })),
          },
        ],
      },
    }),
  );
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Você vive');
  await expect(page.getByRole('img', { name: /Logotipo oficial LTMJ/ })).toBeVisible();
  const menu = page.locator('button[aria-controls="menu-mobile"]');
  await menu.click();
  await expect(menu).toHaveAttribute('aria-expanded', 'true');
  await page
    .getByRole('navigation', { name: 'Navegação mobile' })
    .getByRole('link', { name: 'Solicitar orçamento' })
    .click();
  await expect(menu).toHaveAttribute('aria-expanded', 'false');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  await page.screenshot({ path: testInfo.outputPath('ltmj-mobile.png'), fullPage: true });
});

test('prepara orçamento revisável sem enviar dados à API', async ({ page, context }, testInfo) => {
  await context.grantPermissions(['clipboard-read', 'clipboard-write']);
  let apiRequests = 0;
  page.on('request', (request) => {
    if (request.url().startsWith('https://api.ltmj.') && request.method() !== 'GET') apiRequests++;
  });
  await page.route('**/portfolio/events?*', (route) =>
    route.fulfill({
      json: {
        items: [
          {
            id: 'churrasco',
            title: 'Churrasco de confraternização',
            description: 'Churrasco da LTMJ',
            category: 'Confraternizações',
            isVisible: true,
            photos: Array.from({ length: 12 }, (_, index) => ({
              id: String(index),
              url: `assets/events/churrasco-confraternizacao/churrasco${String(index + 1).padStart(2, '0')}.jpeg`,
            })),
          },
        ],
      },
    }),
  );
  await page.goto('/');
  await page.getByRole('combobox', { name: 'Qual é a ocasião?' }).click();
  await page.getByRole('option', { name: 'Casamento', exact: true }).click();
  await page.getByRole('textbox', { name: 'Cidade do evento' }).fill('Curitiba');
  await page.getByRole('spinbutton', { name: 'Convidados, aproximadamente' }).fill('80');
  await page.getByRole('button', { name: 'Bar e drinks', exact: true }).click();
  await page.getByRole('button', { name: 'Preparar meu pedido' }).click();
  const review = page.getByRole('region', { name: 'Revisão do pedido' });
  await expect(review).toContainText('Casamento');
  await expect(review).toContainText('80');
  await expect(review).toContainText('Bar e drinks');
  await page.getByRole('button', { name: 'Copiar resumo' }).click();
  await expect(page.getByRole('status')).toContainText('Resumo copiado');
  expect(await page.evaluate(() => navigator.clipboard.readText())).toContain('Olá, LTMJ!');
  expect(apiRequests).toBe(0);
  await expect(page.locator('a[href^="https://wa.me/"]')).toHaveCount(0);
  await page.getByRole('textbox', { name: 'Cidade do evento' }).fill('Pinhais');
  await expect(review).toHaveCount(0);
  await page.route('**/portfolio/events?*', (route) =>
    route.fulfill({
      json: {
        items: [
          {
            id: 'churrasco',
            title: 'Churrasco de confraternização',
            description: 'Churrasco da LTMJ',
            category: 'Confraternizações',
            isVisible: true,
            photos: Array.from({ length: 12 }, (_, index) => ({
              id: String(index),
              url: `assets/events/churrasco-confraternizacao/churrasco${String(index + 1).padStart(2, '0')}.jpeg`,
            })),
          },
        ],
      },
    }),
  );
  await page.goto('/');
  await page.screenshot({ path: testInfo.outputPath('ltmj-desktop.png'), fullPage: true });
});

test('mural filtra ocasiões e apresenta o churrasco com fotos', async ({ page }) => {
  await page.route('**/portfolio/events?*', (route) =>
    route.fulfill({
      json: {
        items: [
          {
            id: 'churrasco',
            title: 'Churrasco de confraternização',
            description: 'Churrasco da LTMJ',
            category: 'Confraternizações',
            isVisible: true,
            photos: Array.from({ length: 12 }, (_, index) => ({
              id: String(index),
              url: `assets/events/churrasco-confraternizacao/churrasco${String(index + 1).padStart(2, '0')}.jpeg`,
            })),
          },
        ],
      },
    }),
  );
  await page.goto('/');
  const mural = page.getByRole('region', { name: /Histórias servidas/ });
  await expect(mural.locator('article')).toHaveCount(1);
  await mural.getByRole('button', { name: 'Aniversários', exact: true }).click();
  await expect(mural.locator('article')).toHaveCount(0);
  await mural.getByRole('button', { name: 'Confraternizações', exact: true }).click();
  await expect(mural.locator('article')).toHaveCount(1);
  const explore = mural.getByRole('button', { name: 'Ver fotos e detalhes' });
  await explore.click();
  const dialog = page.getByRole('dialog', { name: 'Churrasco de confraternização' });
  await expect(dialog).toBeVisible();
  await expect(dialog.getByRole('status')).toHaveText('1 de 12');
  await expect(dialog.getByRole('img')).toHaveAttribute('src', /churrasco01\.jpeg$/);
  await dialog.getByRole('button', { name: 'Próxima foto' }).click();
  await expect(dialog.getByRole('status')).toHaveText('2 de 12');
  await page.keyboard.press('Escape');
  await expect(dialog).toHaveCount(0);
  await expect(explore).toBeFocused();
});
