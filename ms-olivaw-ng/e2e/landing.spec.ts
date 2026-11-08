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

test('descoberta oferece sugestão antes dos dados e copia sem envio à API', async ({
  page,
  context,
}, testInfo) => {
  await context.grantPermissions(['clipboard-read', 'clipboard-write']);
  let apiRequests = 0;
  page.on('request', (request) => {
    if (request.url().startsWith('https://api.olivaw.com.br')) apiRequests++;
  });
  await page.goto('/');
  await page.getByRole('button', { name: /Quero organizar minha ideia/ }).click();
  const guide = page.getByRole('region', { name: 'Descubra um caminho para seu projeto' });
  await expect(guide.getByRole('textbox')).toHaveCount(0);
  await guide.screenshot({ path: testInfo.outputPath('discovery-desktop.png') });
  await guide.getByRole('button', { name: /Quero automatizar tarefas/ }).click();
  await expect(guide.getByRole('heading', { level: 3 })).toBeFocused();
  await guide.getByRole('button', { name: /Copiar informações de um lugar para outro/ }).click();
  await guide.getByRole('button', { name: /Faço tudo manualmente/ }).click();
  await expect(guide.getByRole('heading', { level: 4 })).toContainText('Uma tarefa repetida');
  await expect(guide.getByRole('textbox')).toHaveCount(0);
  await guide.getByRole('button', { name: 'Explorar meu projeto com a Olivaw' }).click();
  await expect(page).toHaveURL(/\/projeto$/);
  await page.getByLabel('Como se chama seu negócio?').fill('Empresa de eventos');
  await page
    .getByLabel('O que você gostaria de fazer acontecer?')
    .fill('Quero destacar os eventos que já fiz.');
  await expect(
    page.getByRole('checkbox', { name: 'Copiar informações de um lugar para outro' }),
  ).toBeChecked();
  for (let index = 0; index < 4; index++)
    await page.getByRole('button', { name: /^Próximo:/ }).click();
  await page.getByRole('button', { name: 'Revisar minha pasta' }).click();
  await page.getByRole('button', { name: 'Copiar resumo do projeto' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Resumo copiado' })).toBeVisible();
  const copied = await page.evaluate(() => navigator.clipboard.readText());
  expect(copied).toContain('Empresa de eventos');
  expect(copied).toContain('Quero destacar os eventos que já fiz.');
  expect(apiRequests).toBe(0);
  await page.screenshot({
    path: testInfo.outputPath('project-folder-discovery.png'),
    fullPage: true,
  });
});

test('descoberta mobile troca de caminho sem respostas incompatíveis', async ({
  page,
}, testInfo) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/');
  await page.getByRole('button', { name: /Quero organizar minha ideia/ }).click();
  const guide = page.getByRole('region', { name: 'Descubra um caminho para seu projeto' });
  await guide.screenshot({ path: testInfo.outputPath('discovery-mobile.png') });
  await guide.getByRole('button', { name: /Quero automatizar tarefas/ }).click();
  await guide.getByRole('button', { name: /Enviar avisos e lembretes/ }).click();
  await guide.getByRole('button', { name: 'Voltar' }).click();
  await expect(guide.getByRole('button', { name: /Enviar avisos e lembretes/ })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await guide.getByRole('button', { name: 'Voltar' }).click();
  await guide.getByRole('button', { name: /Ainda não sei por onde começar/ }).click();
  await guide.getByRole('button', { name: /Só quero conhecer as possibilidades/ }).click();
  await guide.getByRole('button', { name: /Quero ajuda para decidir/ }).click();
  await expect(guide.getByRole('heading', { level: 4 })).toContainText(
    'Encontrar uma oportunidade',
  );
  await expect(guide).not.toContainText('Enviar avisos e lembretes');
  await guide.getByRole('button', { name: 'Explorar meu projeto com a Olivaw' }).click();
  await expect(
    page.getByRole('checkbox', { name: 'Só quero conhecer as possibilidades' }),
  ).toBeChecked();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
});

for (const viewport of [
  { name: 'desktop', width: 1280, height: 900 },
  { name: 'mobile', width: 390, height: 844 },
]) {
  test(`menu ${viewport.name} alcança cada seção com um único clique`, async ({ page }) => {
    await page.setViewportSize(viewport);
    await page.emulateMedia({ reducedMotion: 'reduce' });
    for (const [name, id] of [
      ['Soluções', 'solucoes'],
      ['Clientes', 'clientes'],
      ['Como funciona', 'como-funciona'],
      ['Sobre nós', 'sobre'],
      ['Contato', 'contato'],
    ]) {
      await page.goto('/');
      if (viewport.name === 'mobile') {
        await page.locator('button[aria-controls="menu-mobile"]').click();
      }
      const navigation = page.getByRole('navigation', {
        name: viewport.name === 'mobile' ? 'Navegação mobile' : 'Navegação principal',
        exact: true,
      });
      await navigation.getByRole('link', { name, exact: true }).click();
      await expect(page).toHaveURL(new RegExp(`#${id}$`));
      // Wait for the router's scheduled scroll after native anchor navigation.
      await page.evaluate(
        () =>
          new Promise<void>((resolve) =>
            requestAnimationFrame(() => requestAnimationFrame(() => resolve())),
          ),
      );
      await expect
        .poll(() =>
          page.locator(`#${id}`).evaluate((section) => {
            const top = section.getBoundingClientRect().top;
            const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
            return (
              window.scrollY > 0 &&
              (Math.abs(top) <= 40 || Math.abs(window.scrollY - maxScroll) <= 4)
            );
          }),
        )
        .toBe(true);
      if (viewport.name === 'mobile') {
        await expect(page.locator('#menu-mobile')).toHaveCount(0);
      }
    }
  });
}
