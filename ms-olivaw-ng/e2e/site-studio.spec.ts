import { expect, test } from '@playwright/test';

test('Quero um site abre um estúdio que transforma a prévia sem enviar dados', async ({
  page,
}, testInfo) => {
  let apiRequests = 0;
  page.on('request', (request) => {
    if (request.url().startsWith('https://api.olivaw.com.br')) apiRequests++;
  });
  await page.goto('/');
  await page.getByRole('button', { name: /Quero um site/ }).click();
  await expect(page).toHaveURL(/\/estudio$/);
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Sua ideia, ganhando forma.');
  const preview = page.getByRole('region', { name: 'Prévia interativa do seu site' });
  await expect(
    preview.getByRole('heading', { name: 'Seu trabalho merece ser encontrado.' }),
  ).toBeVisible();
  await page.screenshot({ path: testInfo.outputPath('site-studio-desktop.png'), fullPage: true });
  await page.getByRole('button', { name: /Divulgar produtos/ }).click();
  await expect(preview.getByRole('heading', { name: 'Uma seleção para conhecer.' })).toBeAttached();
  await expect(preview.getByRole('heading', { name: 'Soluções com o seu jeito.' })).toHaveCount(0);
  await preview
    .getByRole('button', { name: /Ver detalhes/ })
    .first()
    .click();
  await expect(preview.getByText(/Aqui entram descrição/)).toBeVisible();
  await page.getByRole('button', { name: /Dúvidas frequentes/ }).click();
  await preview.getByText('Como funciona o primeiro contato?', { exact: true }).click();
  await expect(
    preview.getByText('Aqui você explica o atendimento do seu negócio e os canais disponíveis.'),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Ver meu ponto de partida' }).click();
  await expect(page.getByRole('heading', { name: 'Já dá para imaginar, né?' })).toBeVisible();
  await expect(page.getByRole('textbox')).toHaveCount(0);
  expect(apiRequests).toBe(0);
});

test('estúdio funciona em celular, respeita movimento reduzido e permite recomeçar', async ({
  page,
}, testInfo) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await page.goto('/estudio');
  await page.getByRole('button', { name: /Mostrar meus trabalhos/ }).click();
  await page.getByRole('button', { name: /Dúvidas frequentes/ }).click();
  await page.getByRole('button', { name: /Meus trabalhos/ }).click();
  const preview = page.getByRole('region', { name: 'Prévia interativa do seu site' });
  await expect(
    preview.getByRole('heading', { name: 'Trabalhos que contam histórias.' }),
  ).toHaveCount(0);
  await page.getByRole('button', { name: 'Celular', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Celular', exact: true })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  await page.screenshot({ path: testInfo.outputPath('site-studio-mobile.png'), fullPage: true });
  await page.getByRole('button', { name: 'Recomeçar', exact: true }).click();
  await expect(page.getByRole('button', { name: /Apresentar meu negócio/ })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await expect(page.getByRole('button', { name: /Dúvidas frequentes/ })).toHaveAttribute(
    'aria-pressed',
    'false',
  );
  await expect(preview.getByRole('heading', { name: 'Soluções com o seu jeito.' })).toBeAttached();
});
