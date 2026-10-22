import { expect, test, type Page } from '@playwright/test';

async function signIn(
  page: Page,
  options: { root?: boolean; expiresIn?: number; logoutFails?: boolean } = {},
) {
  const calls: string[] = [];
  await page.route('https://api.olivaw.com.br/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    calls.push(path);
    if (path === '/identity/authentication/login') {
      return route.fulfill({
        json: {
          accessToken: 'local-test-token',
          expiresAt: new Date(Date.now() + (options.expiresIn ?? 60000)).toISOString(),
        },
      });
    }
    if (path === '/identity/authentication/access') {
      return route.fulfill({
        json: {
          userProfileId: 'test-operator',
          name: 'Operador Olivaw',
          isRoot: options.root ?? true,
          permissions: [],
        },
      });
    }
    if (path === '/identity/authentication/logout') {
      if (options.logoutFails)
        return route.fulfill({ status: 503, json: { title: 'Serviço indisponível.' } });
      return route.fulfill({ json: {} });
    }
    if (path === '/leads/summary')
      return route.fulfill({
        json: {
          total: 0,
          recent: 0,
          since: new Date(Date.now() - 7 * 86400000).toISOString(),
          asOf: new Date().toISOString(),
        },
      });
    if (path === '/customers/summary')
      return route.fulfill({
        json: {
          total: 0,
          onboarding: 0,
          active: 0,
          paused: 0,
          closed: 0,
          asOf: new Date().toISOString(),
        },
      });
    await route.abort();
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('local-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  return calls;
}

for (const mobile of [false, true]) {
  test(`dashboard ${mobile ? 'mobile' : 'desktop'} tem contexto e estados vazios sem inventar medições`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 1000 });
    await page.emulateMedia({ reducedMotion: 'reduce' });
    const calls = await signIn(page);
    await expect(page).toHaveURL(/\/admin$/);
    await expect(page).toHaveTitle('Olivaw - Operação');
    await expect(page.getByRole('heading', { level: 1, name: 'Visão geral' })).toBeVisible();
    await expect(page.getByText('Olá, Operador Olivaw.')).toBeVisible();
    const nav = page.getByRole('navigation', { name: 'Navegação da operação' });
    await expect(nav.getByRole('link')).toHaveCount(3);
    await expect(nav.getByRole('link', { name: 'Visão geral' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    for (const label of [
      'Ambiente não informado',
      'Monitoramento não conectado',
      'Solicitações ainda não habilitadas',
      'Sem fontes conectadas',
      'Histórico ainda não conectado',
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }
    const avatar = page.locator('.avatar-scene img');
    await expect
      .poll(() => avatar.evaluate((img: HTMLImageElement) => img.complete && img.naturalWidth > 0))
      .toBe(true);
    await expect(page.locator('.welcome')).toHaveCSS('animation-name', 'none');
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1440,
    );
    expect(calls).toEqual([
      '/identity/authentication/login',
      '/identity/authentication/access',
      '/leads/summary',
      '/customers/summary',
    ]);
    const skip = page.getByRole('link', { name: 'Ir para o conteúdo', exact: true });
    await skip.focus();
    await page.keyboard.press('Enter');
    await expect(page.locator('#operation-content')).toBeFocused();
    await page.screenshot({
      path: testInfo.outputPath(`admin-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    if (!mobile) {
      const toggle = page.getByRole('button', { name: 'Recolher menu', exact: true });
      await toggle.click();
      const expand = page.getByRole('button', { name: 'Expandir menu', exact: true });
      await expect(expand).toHaveAttribute('aria-expanded', 'false');
      await expect
        .poll(() =>
          page
            .locator('#operation-sidebar')
            .evaluate((sidebar) => sidebar.getBoundingClientRect().width),
        )
        .toBe(76);
      await expect(nav.getByRole('link', { name: 'Visão geral', exact: true })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Sair', exact: true })).toBeVisible();
      await page.screenshot({ path: testInfo.outputPath('admin-collapsed.png'), fullPage: true });
      await expand.focus();
      await page.keyboard.press('Enter');
      await expect(
        page.getByRole('button', { name: 'Recolher menu', exact: true }),
      ).toHaveAttribute('aria-expanded', 'true');
    }
    await page.getByRole('button', { name: 'Sair', exact: true }).click();
    await expect(page).toHaveURL(/\/login$/);
    expect(calls).toContain('/identity/authentication/logout');
  });
}

test('conta interna sem root não entra na operação', async ({ page }) => {
  await signIn(page, { root: false });
  await expect(page).toHaveURL(/\/acesso-negado$/);
  await expect(page.getByRole('heading', { name: 'Acesso negado' })).toBeVisible();
  await page.goto('/admin');
  await expect(page).toHaveURL(/\/login$/);
});

test('sessão expirada retorna ao login sem persistir token', async ({ page }) => {
  await signIn(page, { expiresIn: 3000 });
  await expect(page).toHaveURL(/\/admin$/);
  await expect(page).toHaveURL(/\/login$/, { timeout: 10000 });
  expect(
    await page.evaluate(() => [
      localStorage.getItem('accessToken'),
      sessionStorage.getItem('accessToken'),
    ]),
  ).toEqual([null, null]);
});

test('falha na revogação mantém saída local e explica a limitação', async ({ page }) => {
  await signIn(page, { logoutFails: true });
  await expect(page).toHaveURL(/\/admin$/);
  await page.getByRole('button', { name: 'Sair', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('alert')).toContainText('revogação no servidor não foi confirmada');
});
