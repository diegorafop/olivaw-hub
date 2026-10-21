import { expect, test } from '@playwright/test';

for (const mobile of [false, true]) {
  test(`jornada ${mobile ? 'mobile' : 'desktop'} guia o avanço e respeita o movimento reduzido`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 900 });
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.goto('/projeto');
    const next = page.getByRole('button', { name: 'Próximo: O que somamos' });
    await expect(next).toBeDisabled();
    await page.getByLabel('Como se chama seu negócio?').fill('   ');
    await page
      .getByLabel('O que você gostaria de fazer acontecer?')
      .fill('Controlar pedidos e entregas');
    await expect(next).toBeDisabled();
    await page.getByLabel('Como se chama seu negócio?').fill('Minha empresa');
    await expect(next).toBeEnabled();
    await expect(page.getByRole('heading', { name: 'Muito além das telas.' })).toBeHidden();
    const nav = page.getByRole('navigation', { name: 'Etapas do seu projeto' });
    await expect(nav.getByRole('button', { name: /Nossa parceria/ })).toBeDisabled();
    await next.click();
    await expect(page.getByRole('heading', { name: 'Muito além das telas.' })).toBeFocused();
    await expect(nav.getByRole('button', { name: /O que somamos/ })).toHaveAttribute(
      'aria-current',
      'step',
    );
    await expect(page.locator('.connection-check').first()).toHaveCSS('animation-name', 'none');
    await page.getByRole('button', { name: 'Próximo: Primeira entrega' }).click();
    await page.getByRole('button', { name: 'Próximo: Nossa parceria' }).click();
    const line = page.locator('app-project-plans .energy-line');
    const rect = await line.boundingBox();
    expect(mobile ? rect!.height > rect!.width : rect!.width > rect!.height).toBe(true);
    await page.screenshot({
      path: testInfo.outputPath(`partnership-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    await page.getByRole('button', { name: 'Próximo: Sua pasta' }).click();
    await expect(page.getByLabel('Seu e-mail para contato')).toBeVisible();
    await page.reload();
    await expect(page.getByLabel('Seu e-mail para contato')).toBeVisible();
    await nav.getByRole('button', { name: /Sua ideia/ }).click();
    await expect(page.getByLabel('Como se chama seu negócio?')).toHaveValue('Minha empresa');
    await page.getByLabel('O que você gostaria de fazer acontecer?').fill('');
    await expect(next).toBeDisabled();
    await expect(nav.getByRole('button', { name: /Sua pasta/ })).toBeDisabled();
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1440,
    );
  });
}

test('efeitos são graduais e não atrasam o próximo passo', async ({ page }, testInfo) => {
  await page.goto('/projeto');
  await page.getByLabel('Como se chama seu negócio?').fill('Ateliê Aurora');
  await page.getByLabel('O que você gostaria de fazer acontecer?').fill('Organizar minha loja');
  await page.getByRole('button', { name: 'Próximo: O que somamos' }).click();
  const cards = page.locator('.service-cards article');
  await expect(cards).toHaveCount(4);
  const delays = await cards.evaluateAll((items) =>
    items.map((item) => parseFloat(getComputedStyle(item).animationDelay)),
  );
  expect(delays).toEqual([...delays].sort((a, b) => a - b));
  expect(delays[3]).toBeGreaterThan(delays[0]);
  await expect(page.getByRole('button', { name: 'Próximo: Primeira entrega' })).toBeEnabled();
  await expect
    .poll(async () =>
      Number(
        await page
          .locator('.connection-check')
          .first()
          .evaluate((item) => getComputedStyle(item).opacity),
      ),
    )
    .toBe(1);
  await page.screenshot({ path: testInfo.outputPath('services-in-motion.png'), fullPage: true });
  await page.getByRole('button', { name: 'Próximo: Primeira entrega' }).click();
  await page.getByRole('button', { name: 'Próximo: Nossa parceria' }).click();
  await expect(page.locator('app-project-plans .energy-line i')).toHaveCSS(
    'animation-duration',
    '4.8s',
  );
  await expect(page.getByRole('button', { name: 'Próximo: Sua pasta' })).toBeEnabled();
});
