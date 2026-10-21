import { expect, test } from '@playwright/test';

for (const mobile of [false, true]) {
  test(`pasta ${mobile ? 'mobile' : 'desktop'} preserva ideias e explica parceria sem contratar`, async ({
    page,
  }, testInfo) => {
    if (mobile) await page.setViewportSize({ width: 390, height: 844 });
    await page.emulateMedia({ reducedMotion: 'reduce' });
    const requests: string[] = [];
    page.on('request', (request) => {
      if (request.method() !== 'GET') requests.push(request.url());
    });
    await page.goto('/estudio');
    await page.getByRole('button', { name: /Vender meus produtos/ }).click();
    await page.getByRole('button', { name: /^Categorias/ }).click();
    await page.getByRole('button', { name: /^Detalhes do produto/ }).click();
    await page.getByRole('button', { name: 'Quero criar do meu jeito' }).click();
    await expect(page.getByRole('heading', { name: 'Vender meus produtos' })).toBeVisible();
    await expect(page.getByRole('checkbox', { name: 'Categorias' })).toBeChecked();
    await page.getByRole('checkbox', { name: 'Detalhes do produto' }).uncheck();
    await page
      .getByLabel('O que você gostaria de fazer acontecer?')
      .fill('Vender minha coleção e acompanhar entregas.');
    await page.getByLabel('Como se chama seu negócio?').fill('Minha coleção');
    await page.getByRole('button', { name: 'Próximo: O que somamos' }).click();
    await page.locator('summary').first().click();
    await expect(page.getByText(/Mapeamos processos/)).toBeVisible();
    await page.getByRole('button', { name: 'Próximo: Primeira entrega' }).click();
    await page.getByRole('button', { name: 'Próximo: Nossa parceria' }).click();
    await page
      .getByRole('article')
      .filter({ has: page.getByRole('heading', { name: 'Ouro', exact: true }) })
      .getByRole('button')
      .click();
    await page.getByRole('button', { name: 'Próximo: Sua pasta' }).click();
    await page.getByRole('button', { name: 'Revisar minha pasta' }).click();
    const summary = page.getByLabel('Resumo do projeto', { exact: true });
    await expect(summary).toHaveValue(/Possibilidades: Categorias/);
    await expect(summary).not.toHaveValue(/Detalhes do produto/);
    await expect(summary).toHaveValue(/Ouro \(a avaliar\)/);
    await expect(summary).toHaveValue(/MVP/);
    await expect(page.locator('input[type="file"]')).toHaveCount(0);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1280,
    );
    await page.screenshot({
      path: testInfo.outputPath(`folder-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    expect(requests).toEqual([]);
    await page.reload();
    await page
      .getByRole('navigation', { name: 'Etapas do seu projeto' })
      .getByRole('button', { name: /Sua ideia/ })
      .click();
    await expect(page.getByRole('heading', { name: 'Vender meus produtos' })).toBeVisible();
    await expect(page.getByLabel('O que você gostaria de fazer acontecer?')).toHaveValue(
      'Vender minha coleção e acompanhar entregas.',
    );
  });
}

test('envia o lead apenas na confirmação e permite repetir falha sem duplicar a jornada', async ({
  page,
}) => {
  const payloads: { url: string; revision: number; email: string }[] = [];
  await page.route('**/leads/*', (route) => {
    const body = route.request().postDataJSON();
    payloads.push({ url: route.request().url(), revision: body.revision, email: body.email });
    return route.fulfill({
      status: payloads.length === 1 ? 503 : 200,
      contentType: 'application/json',
      body: JSON.stringify({ revision: body.revision }),
    });
  });
  await page.goto('/estudio');
  await expect(page).toHaveTitle('Olivaw - Estúdio');
  await page.getByRole('button', { name: /Vender meus produtos/ }).click();
  await page.getByRole('button', { name: /^Categorias/ }).click();
  await page.getByRole('button', { name: 'Quero criar do meu jeito' }).click();
  await expect(page).toHaveTitle('Olivaw - Projeto');
  await page.getByLabel('O que você gostaria de fazer acontecer?').fill('Minha primeira loja');
  await page.getByLabel('Como se chama seu negócio?').fill('Minha loja');
  for (let index = 0; index < 4; index++)
    await page.getByRole('button', { name: /^Próximo:/ }).click();
  await page.getByLabel('Seu e-mail para contato').fill('lead@example.test');
  expect(payloads).toHaveLength(0);
  await page.getByRole('button', { name: 'Enviar para análise da Olivaw' }).click();
  await expect(page.getByText(/Não foi possível enviar agora/)).toBeVisible();
  await page.getByRole('button', { name: 'Enviar para análise da Olivaw' }).click();
  await expect(page.getByRole('button', { name: 'Ideia enviada ✓' })).toBeDisabled();
  expect(payloads).toHaveLength(2);
  expect(payloads[1]).toEqual(payloads[0]);
  await page
    .getByRole('navigation', { name: 'Etapas do seu projeto' })
    .getByRole('button', { name: /Sua ideia/ })
    .click();
  await expect(page.getByLabel('O que você gostaria de fazer acontecer?')).toHaveValue(
    'Minha primeira loja',
  );
});
