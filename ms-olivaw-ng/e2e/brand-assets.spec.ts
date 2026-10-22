import { expect, test } from '@playwright/test';
test('identidade usa órbita fina no site, favicon e compartilhamento', async ({
  page,
  request,
}) => {
  await page.goto('/');
  await expect(page.locator('link[rel="icon"]')).toHaveAttribute(
    'href',
    '/assets/brand/olivaw-favicon-v4.png',
  );
  await expect(page.locator('link[rel="apple-touch-icon"]')).toHaveAttribute(
    'href',
    '/assets/brand/olivaw-apple-touch-v4.png',
  );
  await expect(page.locator('meta[property="og:image"]')).toHaveAttribute(
    'content',
    'https://olivaw.com.br/assets/brand/olivaw-share-v3.jpg',
  );
  await expect(page.locator('img[src*="olivaw-avatar-web-v4.png"]').first()).toBeVisible();
  for (const [path, type] of [
    ['/assets/brand/olivaw-avatar-web-v4.png', 'image/png'],
    ['/assets/brand/olivaw-favicon-v4.png', 'image/png'],
    ['/assets/brand/olivaw-apple-touch-v4.png', 'image/png'],
    ['/assets/brand/olivaw-share-v3.jpg', 'image/jpeg'],
    ['/assets/brand/reactions/olivaw-message-welcome-v3.webp', 'image/webp'],
    ['/assets/brand/reactions/olivaw-operations-coordinator-v3.webp', 'image/webp'],
    ['/favicon.ico', 'image/'],
  ]) {
    const response = await request.get(path);
    expect(response.ok()).toBe(true);
    expect(response.headers()['content-type']).toContain(type);
  }
});
