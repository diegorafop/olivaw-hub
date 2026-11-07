import { expect, test, type Page } from '@playwright/test';
const id = '00000000-0000-0000-0000-000000000027';
async function setup(
  page: Page,
  conflict = false,
  forbidden = false,
  initialQualification?: Record<string, unknown>,
) {
  let qualification: Record<string, unknown> | undefined = initialQualification;
  let revision = 1;
  let followUpRevision = 0;
  const writes: unknown[] = [];
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 120000).toISOString() },
      });
    if (url.pathname.endsWith('/access'))
      return route.fulfill({
        json: {
          name: 'Comercial',
          isRoot: false,
          permissions: forbidden ? ['portal.read'] : ['leads.manage'],
          userProfileId: 'test',
        },
      });
    expect(route.request().headers()['authorization']).toBe('Bearer test');
    if (url.pathname.endsWith('/qualification')) {
      writes.push(route.request().postDataJSON());
      if (conflict) return route.fulfill({ status: 409, json: {} });
      qualification = route.request().postDataJSON().qualification;
      return route.fulfill({ json: { revision: ++revision, updatedAt: '2026-11-01T12:00:00Z' } });
    }
    if (url.pathname.endsWith('/follow-up'))
      return route.fulfill({ json: { revision: ++followUpRevision } });
    if (url.pathname === `/leads/${id}`)
      return route.fulfill({
        json: {
          externalId: id,
          createdAt: '2026-11-01T10:00:00Z',
          updatedAt: null,
          followUpStatus: 'new',
          followUpRevision,
          contentUnavailable: false,
          snapshot: {
            schemaVersion: 1,
            revision,
            startedAt: '2026-11-01T10:00:00Z',
            entry: 'prospecting',
            stage: 'submitted',
            business: 'Eventos Aurora',
            requirements: 'Organização de eventos corporativos.',
            email: '',
            whatsapp: '',
            excluded: [],
            events: [],
            project: { source: 'discovery', title: 'Presença digital', choices: [], context: '' },
            acquisition: {
              initiative: 'outbound',
              origin: 'Pesquisa web',
              externalReference: '',
              sourceUrl: '',
            },
            qualification,
          },
        },
      });
    if (url.pathname.endsWith('/history'))
      return route.fulfill({ json: { items: [], nextBeforeRevision: null } });
    if (url.pathname.endsWith('/summary'))
      return route.fulfill({ json: { total: 1, recent: 1, byStatus: {} } });
    if (url.pathname === '/leads')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: 1,
          asOf: '2026-11-01T10:00:00Z',
          items: [
            {
              externalId: id,
              business: 'Eventos Aurora',
              title: 'Presença digital',
              createdAt: '2026-11-01T10:00:00Z',
              entry: 'prospecting',
              followUpStatus: 'new',
              email: '',
              whatsapp: '',
              phone: '(41) 99897-0017',
              segment: 'Eventos',
            },
          ],
        },
      });
    return route.fulfill({ json: {} });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).not.toHaveURL(/\/login$/);
  return writes;
}
test('preserva emails e evidências de formulário ao editar a ficha', async ({ page }) => {
  const evidence = {
    publicEmails: [{ email: 'contato@example.test', sourceUrl: 'https://example.test/contato' }],
    contactPageUrl: 'https://example.test/contato',
    contactFormUrl: 'https://example.test/contato',
    contactFormStatus: 'found',
    contactCheckedAt: '2026-11-07T12:00:00Z',
  };
  const writes = await setup(page, false, false, evidence);
  await page.goto(`/admin/leads/${id}/details`);
  const channels = page.getByRole('region', { name: 'Canais da oportunidade' });
  await expect(channels).toContainText('contato@example.test');
  await expect(channels).toContainText('Formulário de contato encontrado.');
  await expect(channels.getByRole('link', { name: 'Página com formulário' })).toHaveAttribute(
    'href',
    evidence.contactFormUrl,
  );
  await page.getByLabel('Segmento', { exact: true }).fill('Eventos');
  await page.getByRole('button', { name: 'Salvar qualificação e abordagem' }).click();
  await expect(
    page.getByText('Qualificação e rascunho salvos. Nenhuma mensagem foi enviada.', {
      exact: true,
    }),
  ).toBeVisible();
  expect(writes).toHaveLength(1);
  expect((writes[0] as { qualification: unknown }).qualification).toMatchObject(evidence);
});
for (const width of [1440, 390]) {
  test(`ficha qualifica e prepara abordagem sem envio ${width}`, async ({ page }, testInfo) => {
    await page.setViewportSize({ width, height: 1000 });
    const writes = await setup(page);
    await page.goto(`/admin/leads/${id}?search=eventos`);
    const preview = page.getByRole('complementary', { name: 'Detalhes do lead' });
    await expect(preview.getByLabel('Nota interna')).toHaveCount(0);
    await expect(preview.getByRole('combobox')).toHaveCount(0);
    await preview.getByRole('link', { name: 'Abrir ficha completa' }).click();
    await expect(page).toHaveURL(new RegExp(`/admin/leads/${id}/details\\?search=eventos$`));
    await expect(
      page.getByRole('navigation', { name: 'Caminho de navegação' }).locator('[aria-current=page]'),
    ).toHaveText('Ficha do lead');
    await page.getByLabel('Segmento', { exact: true }).fill('eventos');
    await page.getByLabel('Região', { exact: true }).fill('Curitiba');
    await page.getByLabel('Presença digital').selectOption('website');
    await page.getByLabel('Pontos positivos').fill('Apresentação clara dos serviços.');
    await page.getByLabel('Telefone comercial').fill('(41) 99897-0017');
    await page
      .getByRole('navigation', { name: 'Seções da ficha' })
      .getByRole('link', { name: 'Abordagem', exact: true })
      .click();
    await expect(page).toHaveURL(/details\?search=eventos#approach$/);
    await expect(page.getByLabel('Segmento', { exact: true })).toHaveValue('eventos');
    await page.getByRole('button', { name: 'Preparar sugestão' }).click();
    const approach = page.getByLabel('Rascunho de abordagem');
    await expect(approach).toHaveValue(/pessoal da Eventos Aurora/);
    await expect(approach).toHaveValue(/Apresentação clara dos serviços/);
    expect(writes).toHaveLength(0);
    await page.getByLabel('Nota interna').fill('Perguntar sobre a operação.');
    await page.getByRole('button', { name: 'Salvar qualificação e abordagem' }).click();
    await expect(
      page.getByText('Qualificação e rascunho salvos. Nenhuma mensagem foi enviada.', {
        exact: true,
      }),
    ).toBeVisible();
    await expect(page.getByLabel('Nota interna')).toHaveValue('Perguntar sobre a operação.');
    await page.getByRole('button', { name: 'Salvar acompanhamento' }).click();
    await expect(page.getByLabel('Nota interna')).toHaveValue('');
    await page.reload();
    await expect(page.getByLabel('Segmento', { exact: true })).toHaveValue('eventos');
    await expect(approach).toHaveValue(/construir parcerias/);
    const channels = page.getByRole('region', { name: 'Canais da oportunidade' });
    await expect(channels).toContainText('(41) 99897-0017');
    await expect(channels.getByRole('link', { name: /WhatsApp/ })).toHaveCount(0);
    expect(writes).toHaveLength(1);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      width,
    );
    await page.screenshot({ path: testInfo.outputPath('lead-record.png'), fullPage: true });
    await page.getByRole('link', { name: 'Voltar ao resumo' }).click();
    await expect(page).toHaveURL(new RegExp(`/admin/leads/${id}\\?search=eventos$`));
  });
}
test('concorrência mantém texto e impede sobrescrever', async ({ page }) => {
  const writes = await setup(page, true);
  await page.goto(`/admin/leads/${id}/details`);
  await page.getByLabel('Rascunho de abordagem').fill('Meu texto revisável');
  await page.getByRole('button', { name: 'Salvar qualificação e abordagem' }).click();
  await expect(page.getByRole('alert')).toContainText('A ficha mudou');
  await expect(page.getByLabel('Rascunho de abordagem')).toHaveValue('Meu texto revisável');
  expect(writes).toHaveLength(1);
  await expect(
    page.getByRole('button', { name: 'Salvar qualificação e abordagem' }),
  ).toBeDisabled();
  page.on('dialog', (dialog) => dialog.dismiss());
  await page.getByRole('link', { name: 'Voltar ao resumo' }).click();
  await expect(page).toHaveURL(new RegExp(`/admin/leads/${id}/details$`));
});
test('ficha exige permissão de leads', async ({ page }) => {
  await setup(page, false, true);
  await page.goto(`/admin/leads/${id}/details`);
  await expect(page).toHaveURL(/\/acesso-negado$/);
});
