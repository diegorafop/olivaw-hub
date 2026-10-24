import { expect, test } from '@playwright/test';

for (const mobile of [false, true]) {
  test(`clientes e contato ${mobile ? 'mobile' : 'desktop'} ficam acessíveis sem percorrer a jornada`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1280, height: 900 });
    await page.goto('/');
    if (mobile) await page.getByRole('button', { name: 'Menu', exact: true }).click();
    const nav = page.getByRole('navigation', {
      name: mobile ? 'Navegação mobile' : 'Navegação principal',
    });
    await nav.getByRole('link', { name: 'Clientes', exact: true }).click();
    const clients = page.locator('#clientes');
    await expect(clients.getByRole('heading', { name: 'LTMJ Eventos' })).toBeVisible();
    await expect(clients.getByRole('link', { name: /Visitar o site/ })).toHaveAttribute(
      'href',
      'https://ltmj.olivaw.com.br/',
    );
    await expect(clients.getByRole('link', { name: /Visitar o site/ })).toHaveAttribute(
      'target',
      '_blank',
    );
    await clients.screenshot({
      path: testInfo.outputPath(`clients-${mobile ? 'mobile' : 'desktop'}.png`),
    });
    if (mobile) await page.getByRole('button', { name: 'Menu', exact: true }).click();
    await nav.getByRole('link', { name: 'Contato', exact: true }).click();
    const form = page.getByRole('form', { name: 'Contato direto com a Olivaw' });
    await expect(form).toBeVisible();
    await form.getByRole('button', { name: 'Enviar mensagem' }).click();
    await expect(
      form.getByText('Preencha seu e-mail ou WhatsApp para podermos responder.'),
    ).toBeVisible();
    await form.getByLabel('Seu e-mail').fill('lead@example.test');
    await form.getByLabel('Sua mensagem').fill('Queria tirar uma dúvida sobre hospedagem.');
    await expect(form.getByRole('button', { name: 'Enviar mensagem' })).toBeEnabled();
    await page.getByRole('button', { name: /Quero organizar minha ideia/ }).click();
    await expect(page.getByRole('button', { name: /Quero um site/ })).toBeVisible();
    await page.getByRole('button', { name: /Quero conversar com a Olivaw/ }).click();
    await expect(form.getByLabel('Sua mensagem')).toHaveValue(
      'Queria tirar uma dúvida sobre hospedagem.',
    );
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1280,
    );
    await page
      .locator('#contato')
      .screenshot({ path: testInfo.outputPath(`contact-${mobile ? 'mobile' : 'desktop'}.png`) });
  });
}

test('contato envia só na confirmação, preserva mensagem na falha e repete sem duplicar', async ({
  page,
}) => {
  const payloads: { url: string; body: unknown }[] = [];
  await page.route('**/leads/*', (route) => {
    payloads.push({ url: route.request().url(), body: route.request().postDataJSON() });
    return route.fulfill({
      status: payloads.length === 1 ? 503 : 200,
      contentType: 'application/json',
      body: '{}',
    });
  });
  await page.goto('/#contato');
  const form = page.getByRole('form', { name: 'Contato direto com a Olivaw' });
  await form.getByLabel('Seu e-mail').fill('lead@example.test');
  await form.getByLabel('Sua mensagem').fill('Preciso conversar sobre uma integração.');
  expect(payloads).toHaveLength(0);
  await form.getByRole('button', { name: 'Enviar mensagem' }).click();
  await expect(form.getByRole('status')).toContainText('Não foi possível enviar');
  await expect(form.getByLabel('Sua mensagem')).toHaveValue(
    'Preciso conversar sobre uma integração.',
  );
  await form.getByRole('button', { name: 'Enviar mensagem' }).click();
  await expect(form.getByRole('status')).toContainText('Mensagem recebida');
  await expect(form.getByRole('button', { name: 'Mensagem enviada ✓' })).toBeDisabled();
  expect(payloads).toHaveLength(2);
  expect(payloads[1]).toEqual(payloads[0]);
});

test('WhatsApp com máscara envia sem e-mail e explica campos inválidos', async ({ page }) => {
  const payloads: { email: string; whatsapp: string }[] = [];
  await page.route('**/leads/*', (route) => {
    payloads.push(route.request().postDataJSON());
    return route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });
  });
  await page.goto('/#contato');
  const form = page.getByRole('form', { name: 'Contato direto com a Olivaw' });
  await form.getByLabel('Seu e-mail', { exact: true }).fill('pastelaria');
  await form.getByLabel('Sua mensagem').fill('Quero apresentar meu negócio.');
  await expect(form.getByLabel('Seu e-mail', { exact: true })).toHaveAttribute(
    'aria-invalid',
    'true',
  );
  await expect(form.getByText('Digite um e-mail válido', { exact: false })).toBeVisible();
  await form.getByLabel('Seu e-mail', { exact: true }).fill('');
  const phone = form.getByLabel('Seu WhatsApp com DDD');
  await phone.pressSequentially('11987654321');
  await expect(phone).toHaveValue('(11) 98765-4321');
  await form.getByRole('button', { name: 'Enviar mensagem' }).click();
  await expect(form.getByRole('status')).toContainText('Mensagem recebida');
  expect(payloads).toHaveLength(1);
  expect(payloads[0].email).toBe('');
  expect(payloads[0].whatsapp).toBe('11987654321');
});
