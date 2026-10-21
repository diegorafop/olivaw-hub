import { expect, test } from '@playwright/test';

test('três intenções começam simples e oferecem quatro possibilidades independentes', async ({
  page,
}, testInfo) => {
  let requests = 0;
  page.on('request', (request) => {
    if (request.url().startsWith('https://api.olivaw.com.br')) requests++;
  });
  await page.goto('/estudio');
  const controls = page.getByRole('complementary');
  const preview = page.getByRole('region', { name: 'Prévia interativa do seu site' });
  for (const name of [
    'Apresentar minha empresa',
    'Vender meus produtos',
    'Organizar meu negócio',
  ]) {
    await controls.getByRole('button', { name: new RegExp(name) }).click();
    await expect(controls.locator('.block-option')).toHaveCount(4);
    await expect(controls.locator('.block-option[aria-pressed="true"]')).toHaveCount(0);
  }
  await controls.getByRole('button', { name: /Um dashboard/ }).click();
  await expect(preview.getByRole('heading', { name: 'Seu negócio em um olhar.' })).toBeVisible();
  await controls.getByRole('button', { name: /Contas a pagar/ }).click();
  await preview.getByRole('button', { name: 'Registrar pagamento' }).click();
  await expect(preview.getByRole('button', { name: 'Pagamento registrado ✓' })).toBeDisabled();
  await page.screenshot({ path: testInfo.outputPath('studio-management.png'), fullPage: true });
  await controls.getByRole('button', { name: /Vender meus produtos/ }).click();
  await expect(preview.getByRole('heading', { name: 'Contas a pagar' })).toHaveCount(0);
  await controls.getByRole('button', { name: /Carrinho/ }).click();
  await preview.getByRole('button', { name: 'Adicionar ao carrinho +' }).first().click();
  await expect(preview.getByText('1 produto escolhido.')).toBeAttached();
  await controls.getByRole('button', { name: 'Quero criar do meu jeito' }).click();
  await expect(page).toHaveURL(/\/projeto$/);
  await page
    .getByLabel('O que você gostaria de fazer acontecer?')
    .fill('Quero controlar minhas entregas');
  await page.getByRole('link', { name: 'Voltar às ideias' }).click();
  await expect(controls.getByRole('button', { name: /Carrinho/ })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await controls.getByRole('button', { name: 'Quero criar do meu jeito' }).click();
  await expect(page.getByLabel('O que você gostaria de fazer acontecer?')).toHaveValue(
    'Quero controlar minhas entregas',
  );
  expect(requests).toBe(0);
});

test('celular e movimento reduzido preservam seleção, remoção e recomeço', async ({
  page,
}, testInfo) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await page.goto('/estudio');
  const controls = page.getByRole('complementary');
  const preview = page.getByRole('region', { name: 'Prévia interativa do seu site' });
  await controls.getByRole('button', { name: /Meus serviços/ }).click();
  await expect(preview.getByRole('heading', { name: 'Soluções com o seu jeito.' })).toBeVisible();
  await controls.getByRole('button', { name: /Meus serviços/ }).click();
  await expect(preview.getByRole('heading', { name: 'Soluções com o seu jeito.' })).toHaveCount(0);
  await controls.getByRole('button', { name: /Organizar meu negócio/ }).click();
  await controls.getByRole('button', { name: /Uma planilha/ }).click();
  await preview.getByRole('searchbox').fill('Luminária');
  await expect(preview.getByRole('cell', { name: 'Luminária Aurora' })).toBeVisible();
  await expect(preview.getByRole('cell', { name: 'Mesa Essencial' })).toHaveCount(0);
  await page.getByRole('button', { name: 'Celular', exact: true }).click();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  await page.screenshot({ path: testInfo.outputPath('studio-mobile.png'), fullPage: true });
  await controls.getByRole('button', { name: 'Recomeçar' }).click();
  await expect(controls.getByRole('button', { name: /Apresentar minha empresa/ })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await expect(controls.locator('.block-option[aria-pressed="true"]')).toHaveCount(0);
  await expect(
    preview.getByRole('heading', { name: 'Seu trabalho merece ser encontrado.' }),
  ).toBeVisible();
});
