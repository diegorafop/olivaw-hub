import { expect, test } from '@playwright/test';
test('identidade usa órbita fina no site, favicon e compartilhamento', async ({
  page,
  request,
}) => {
  await page.goto('/');
  await expect(page.locator('link[rel="icon"]')).toHaveAttribute(
    'href',
    '/assets/brand/olivaw-favicon-v3.png',
  );
  await expect(page.locator('link[rel="apple-touch-icon"]')).toHaveAttribute(
    'href',
    '/assets/brand/olivaw-apple-touch-v3.png',
  );
  await expect(page.locator('meta[property="og:image"]')).toHaveAttribute(
    'content',
    'https://olivaw.com.br/assets/brand/olivaw-share-v2.jpg',
  );
  await expect(page.locator('img[src*="olivaw-avatar-web-v3.png"]').first()).toBeVisible();
  for (const [path, type] of [
    ['/assets/brand/olivaw-avatar-web-v3.png', 'image/png'],
    ['/assets/brand/olivaw-favicon-v3.png', 'image/png'],
    ['/assets/brand/olivaw-apple-touch-v3.png', 'image/png'],
    ['/assets/brand/olivaw-share-v2.jpg', 'image/jpeg'],
    ['/assets/brand/reactions/olivaw-message-welcome-v2.webp', 'image/webp'],
    ['/assets/brand/reactions/olivaw-operations-coordinator-v2.webp', 'image/webp'],
    ['/favicon.ico', 'image/'],
  ]) {
    const response = await request.get(path);
    expect(response.ok()).toBe(true);
    expect(response.headers()['content-type']).toContain(type);
  }
});
