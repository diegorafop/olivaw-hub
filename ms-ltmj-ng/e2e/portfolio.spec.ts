import { expect, test } from '@playwright/test';
import { resolve } from 'node:path';

test('equipe cadastra fotos, edita, publica e exclui pelo celular', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 390, height: 844 });
  let stored = {
    id: 'evento-teste',
    title: '',
    description: '',
    category: 'Confraternizações',
    isVisible: false,
    photos: [] as { id: string; url: string }[],
  };
  let exists = false;
  let uploaded = false;
  await page.route('**/identity/autenticacao/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'teste', expiresAt: new Date(Date.now() + 60000).toISOString() },
      });
    return route.fulfill({
      json: {
        usuarioId: 'equipe',
        nome: 'Equipe LTMJ',
        root: false,
        autorizacoes: ['portfolio.manage'],
      },
    });
  });
  await page.route('**/portfolio/events**', async (route) => {
    const request = route.request();
    if (request.method() === 'GET')
      return route.fulfill({ json: { items: exists ? [stored] : [] } });
    if (request.method() === 'POST' && request.url().endsWith('/photos')) {
      expect(request.headers()['content-type']).toContain('multipart/form-data; boundary=');
      const payload = request.postDataBuffer()!.toString('latin1');
      expect(payload).toContain('churrasco01.jpeg');
      expect(payload).toContain('churrasco02.jpeg');
      uploaded = true;
      stored.photos = [
        { id: 'foto-1', url: 'foto-1' },
        { id: 'foto-2', url: 'foto-2' },
      ];
      return route.fulfill({ json: { count: 2 } });
    }
    if (request.method() === 'POST') {
      stored = { ...stored, ...request.postDataJSON() };
      exists = true;
      return route.fulfill({ json: { id: stored.id } });
    }
    if (request.method() === 'PUT' || request.method() === 'PATCH')
      stored = { ...stored, ...request.postDataJSON() };
    if (request.method() === 'DELETE') exists = false;
    return route.fulfill({ json: {} });
  });
  await page.goto('/gestao/entrar');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('teste-local');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Meu portfólio' })).toBeVisible();
  await page.getByLabel('Título', { exact: true }).fill('Churrasco de teste');
  await page.getByLabel('Descrição', { exact: true }).fill('Evento cadastrado pelo celular.');
  await page.getByRole('combobox', { name: 'Categoria', exact: true }).click();
  await page.getByRole('option', { name: 'Confraternizações', exact: true }).click();
  await page
    .getByLabel('Adicionar fotos', { exact: true })
    .setInputFiles([
      resolve('public/assets/events/churrasco-confraternizacao/churrasco01.jpeg'),
      resolve('public/assets/events/churrasco-confraternizacao/churrasco02.jpeg'),
    ]);
  await expect(page.getByText('2 novas fotos selecionadas')).toBeVisible();
  await page.getByRole('button', { name: 'Salvar evento', exact: true }).click();
  const card = page.locator('article');
  await expect(card).toContainText('2 fotos · Oculto');
  await expect(page.locator('input[formcontrolname=title]')).not.toHaveClass(/ng-touched/);
  expect(uploaded).toBe(true);
  await card.getByRole('switch').click();
  await expect(card).toContainText('Publicado');
  await card.getByRole('button', { name: 'Editar', exact: true }).click();
  await page.getByLabel('Título', { exact: true }).fill('Churrasco atualizado');
  await page.getByRole('button', { name: 'Salvar evento', exact: true }).click();
  await expect(card).toContainText('Churrasco atualizado');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  await page.screenshot({ path: testInfo.outputPath('gestao-mobile.png'), fullPage: true });
  await card.getByRole('button', { name: 'Excluir', exact: true }).click();
  await page.getByRole('dialog').getByRole('button', { name: 'Cancelar', exact: true }).click();
  await expect(card).toHaveCount(1);
  await card.getByRole('button', { name: 'Excluir', exact: true }).click();
  await page
    .getByRole('dialog')
    .getByRole('button', { name: 'Excluir definitivamente', exact: true })
    .click();
  await expect(card).toHaveCount(0);
});

test('prévia mostra quatro novas seleções sem depender da API', async ({ page }) => {
  let apiCalls = 0;
  page.on('request', (request) => {
    if (request.url().includes('/portfolio/events')) apiCalls++;
  });
  await page.goto('/previa-portfolio#eventos');
  const mural = page.getByRole('region', { name: /Histórias servidas/ });
  await expect(mural.locator('article')).toHaveCount(5);
  await expect(mural.getByText('Prévia do mural de eventos.', { exact: true })).toBeVisible();
  for (const title of [
    'Buffet e encontros ao ar livre',
    'Churrasco na brasa',
    'Bar e drinks para celebrar',
    'Equipe em ação',
  ]) {
    await expect(mural.getByRole('heading', { name: title, exact: true })).toBeVisible();
  }
  expect(apiCalls).toBe(0);
  await expect(page.locator('meta[name="robots"]')).toHaveAttribute('content', 'noindex, nofollow');
});
