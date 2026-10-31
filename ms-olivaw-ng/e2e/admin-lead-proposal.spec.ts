import { expect, test, type Page } from '@playwright/test';
import { readFileSync } from 'node:fs';
import {
  emptyProposal,
  type LeadProposal,
} from '../src/app/features/admin/leads/lead-proposal-model';
const casePath = '../docs/99freelas/795293/';
const lead = JSON.parse(readFileSync(casePath + 'cadastro-lead.json', 'utf8'));
const prepared = JSON.parse(
  readFileSync(casePath + 'orcamento-proposta.json', 'utf8'),
) as LeadProposal;
async function setup(page: Page, conflict = false) {
  let proposal: LeadProposal | undefined;
  let revision = 1;
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
          permissions: ['leads.manage'],
          userProfileId: 'test',
        },
      });
    expect(route.request().headers()['authorization']).toBe('Bearer test');
    if (url.pathname === '/leads/summary')
      return route.fulfill({ json: { total: 0, recent: 0, byStatus: {} } });
    if (url.pathname === '/leads' && route.request().method() === 'POST') {
      writes.push(route.request().postDataJSON());
      return route.fulfill({ status: 201, json: { externalId: lead.externalId } });
    }
    if (url.pathname === '/leads') return route.fulfill({ json: { page: 1, total: 0, items: [] } });
    if (url.pathname.endsWith('/proposal')) {
      writes.push(route.request().postDataJSON());
      if (conflict) return route.fulfill({ status: 409, json: {} });
      proposal = route.request().postDataJSON().proposal;
      return route.fulfill({ json: { revision: ++revision } });
    }
    if (url.pathname === `/leads/${lead.externalId}`)
      return route.fulfill({
        json: {
          externalId: lead.externalId,
          createdAt: '2026-10-31T12:00:00Z',
          followUpStatus: 'new',
          followUpRevision: 0,
          contentUnavailable: false,
          snapshot: {
            schemaVersion: 1,
            revision,
            entry: 'prospecting',
            stage: 'submitted',
            startedAt: '2026-10-31T12:00:00Z',
            business: '',
            email: '',
            requirements: lead.requirements,
            excluded: [],
            events: [],
            plan: null,
            acquisition: {
              initiative: 'outbound',
              origin: '99Freelas',
              externalReference: '795293',
              sourceUrl: '',
            },
            project: { source: 'discovery', title: lead.title, choices: [], context: '' },
            proposal,
          },
        },
      });
    return route.fulfill({ json: { items: [], nextBeforeRevision: null } });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/admin\/leads$/);
  return writes;
}
for (const mobile of [false, true]) {
  test(`importa lead e orçamento, calcula, persiste e exporta HTML ${mobile ? 'mobile' : 'desktop'}`, async ({
    page,
  }) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 1000 });
    const writes = await setup(page);
    await page.getByRole('link', { name: 'Cadastrar lead', exact: true }).click();
    await page
      .getByLabel('Carregar análise preparada (JSON)')
      .setInputFiles(casePath + 'cadastro-lead.json');
    await expect(page.getByLabel('Origem *', { exact: true })).toHaveValue('99Freelas');
    expect(writes).toHaveLength(0);
    await page.getByRole('button', { name: 'Cadastrar e preparar orçamento' }).click();
    await expect(page).toHaveURL(new RegExp(`/leads/${lead.externalId}/proposal$`));
    expect(writes[0]).toMatchObject({
      email: '',
      whatsapp: '',
      initiative: 'outbound',
      externalReference: '795293',
    });
    await page
      .getByLabel('Carregar orçamento preparado (JSON)')
      .setInputFiles(casePath + 'orcamento-proposta.json');
    await expect(page.getByLabel('Título *', { exact: true })).toHaveValue(lead.title);
    expect(writes).toHaveLength(1);
    await page.getByRole('button', { name: 'Salvar orçamento', exact: true }).click();
    await expect(page.getByRole('status')).toHaveText('Orçamento salvo.');
    expect(writes[1]).toMatchObject({
      revision: 1,
      proposal: { rows: prepared.rows, discountPercent: 0 },
    });
    await page.getByRole('button', { name: 'Ver apresentação' }).click();
    await expect(page.getByLabel('Comparação comercial')).toContainText('3.240,00');
    await expect(page.getByRole('table')).toContainText('Opcional');
    const downloadPromise = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Baixar proposta HTML' }).click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toContain('olivaw-proposta-');
    const file = await download.path();
    const html = readFileSync(file!, 'utf8');
    expect(html).toContain('Implementação avulsa');
    expect(html).toContain('3.240,00');
    expect(html).not.toContain('accessToken');
    expect(
      await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth),
    ).toBe(true);
    await page.screenshot({
      path: `/tmp/olivaw-proposal-${mobile ? 'mobile' : 'desktop'}.png`,
      fullPage: true,
    });
    await page.getByRole('button', { name: 'Recarregar versão salva' }).click();
    await page.getByRole('button', { name: 'Editar orçamento' }).click();
    await expect(page.getByLabel('Título *', { exact: true })).toHaveValue(lead.title);
  });
}
test('conflito preserva a edição e permite exportar antes de recarregar', async ({ page }) => {
  await setup(page, true);
  await page.goto(`/admin/leads/${lead.externalId}/proposal`);
  await page.getByLabel('Título *', { exact: true }).fill('Nova proposta');
  await page.getByRole('button', { name: 'Salvar orçamento', exact: true }).click();
  await expect(page.getByRole('status').filter({ hasText: 'A pasta mudou' })).toBeVisible();
  await expect(page.getByLabel('Título *', { exact: true })).toHaveValue('Nova proposta');
  page.once('dialog', (dialog) => dialog.dismiss());
  await page.getByRole('link', { name: '← Ficha do lead' }).click();
  await expect(page).toHaveURL(/proposal$/);
});
test('HTML importado é apresentado como texto e não executa scripts', async ({ page }) => {
  await setup(page);
  await page.goto(`/admin/leads/${lead.externalId}/proposal`);
  const proposal = emptyProposal('<script>window.hacked=true</script>');
  await page
    .getByLabel('Carregar orçamento preparado (JSON)')
    .setInputFiles({
      name: 'proposal.json',
      mimeType: 'application/json',
      buffer: Buffer.from(JSON.stringify(proposal)),
    });
  await page.getByRole('button', { name: 'Ver apresentação' }).click();
  await expect(page.getByRole('heading', { name: proposal.title, exact: true })).toBeVisible();
  expect(await page.evaluate(() => 'hacked' in window)).toBe(false);
});
