import { expect, test } from '@playwright/test';
for (const width of [1440, 390]) {
  test(`planos públicos explicam escopo e oferta e preservam interesse ${width}`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width, height: 1000 });
    let administrativeReads = 0;
    page.on('request', (request) => {
      if (request.url().includes('api.olivaw.com.br/plans/')) administrativeReads++;
    });
    await page.goto('/planos');
    await expect(
      page.getByRole('heading', { name: 'Comece com o que resolve hoje.' }),
    ).toBeVisible();
    await expect(
      page.getByRole('region', { name: 'Planos da Olivaw' }).getByRole('article'),
    ).toHaveCount(4);
    await page.getByRole('link', { name: 'Conhecer Prata' }).click();
    await expect(page).toHaveURL(/\/planos\/prata$/);
    await expect(page).toHaveTitle('Olivaw - Prata');
    await expect(page.locator('link[rel=canonical]')).toHaveAttribute(
      'href',
      'https://olivaw.com.br/planos/prata',
    );
    await expect(page.getByLabel('Pontos de melhoria')).toContainText('sem acúmulo');
    await expect(
      page.getByText('4 pontos de melhoria por ciclo mensal', { exact: true }),
    ).toBeVisible();
    await expect(page.getByText('5 GB de armazenamento total', { exact: true })).toBeVisible();
    await expect(
      page.getByText('2 contas administrativas do seu site', { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText(
        'Criação e implantação incluídas para projetos Essenciais ou Intermediários em assinaturas com mais de 12 meses.',
        { exact: true },
      ),
    ).toBeVisible();
    await expect(
      page.getByRole('heading', { name: 'Proporcional ao escopo e ao esforço.' }),
    ).toBeVisible();
    await expect(
      page.getByRole('heading', { name: 'Uma primeira versão útil. Depois, o próximo passo.' }),
    ).toBeVisible();
    await expect(page.getByLabel('Condições da oferta')).toContainText(
      'A mensalidade continua sendo cobrada.',
    );
    expect(administrativeReads).toBe(0);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      width,
    );
    await page.screenshot({ path: testInfo.outputPath('prata-publico.png'), fullPage: true });
    const crumbs = page.getByRole('navigation', { name: 'Caminho de navegação' });
    await expect(crumbs.locator('[aria-current=page]')).toHaveText('Prata');
    await crumbs.getByRole('link', { name: 'Planos', exact: true }).click();
    await page.getByRole('link', { name: 'Conhecer Bronze' }).click();
    await expect(
      page.getByText(
        'Criação e implantação incluídas para projetos Essenciais em assinaturas com mais de 12 meses.',
        { exact: true },
      ),
    ).toBeVisible();
    await expect(
      page.getByText('1 ponto de melhoria por ciclo mensal', { exact: true }),
    ).toBeVisible();
    await page.getByRole('link', { name: 'Quero conversar sobre o Bronze' }).click();
    await expect(page).toHaveURL(/\/projeto\?plano=bronze$/);
    await page.getByLabel('Como se chama seu negócio?').fill('Empresa de teste');
    await page
      .getByLabel('O que você gostaria de fazer acontecer?')
      .fill('Apresentar minha empresa e receber contatos.');
    await page.getByRole('button', { name: 'Próximo: O que somamos' }).click();
    await page.getByRole('button', { name: 'Próximo: Primeira entrega' }).click();
    await page.getByRole('button', { name: 'Próximo: Nossa parceria' }).click();
    const bronze = page
      .getByRole('article')
      .filter({ has: page.getByRole('heading', { name: 'Bronze', exact: true }) });
    await expect(bronze.getByRole('button')).toHaveAttribute('aria-pressed', 'true');
    await expect(
      bronze.getByRole('link', { name: 'Detalhes e condições do Bronze' }),
    ).toHaveAttribute('href', '/planos/bronze');
  });
}
test('plano público desconhecido oferece volta ao catálogo', async ({ page }) => {
  await page.goto('/planos/inexistente');
  await expect(
    page.getByRole('heading', { name: 'Plano não encontrado', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Consultar os planos disponíveis' }).click();
  await expect(page).toHaveURL(/\/planos$/);
});
