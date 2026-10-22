import { readFile, writeFile } from 'node:fs/promises';
import { chromium } from '@playwright/test';

// Export a dedicated social JPEG; never overwrite the original brand assets.
const source = new URL('../public/assets/brand/olivaw-avatar-web-v4.png', import.meta.url);
const destination = new URL('../public/assets/brand/olivaw-share-v3.jpg', import.meta.url);
const sourceData = await readFile(source);
const browser = await chromium.launch({ headless: true });
try {
  const page = await browser.newPage();
  const encoded = await page.evaluate(async (data) => {
    const image = new Image();
    image.src = `data:image/png;base64,${data}`;
    await image.decode();
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 720;
    const context = canvas.getContext('2d');
    if (!context) throw new Error('Canvas indisponível.');
    context.fillStyle = '#fafbf8';
    context.fillRect(0, 0, 720, 720);
    context.imageSmoothingQuality = 'high';
    context.drawImage(image, 0, 0, 720, 720);
    return canvas.toDataURL('image/jpeg', 0.82).split(',')[1];
  }, sourceData.toString('base64'));
  const output = Buffer.from(encoded, 'base64');
  if (output.length > 150_000) throw new Error('A prévia excedeu o limite de 150 KB.');
  await writeFile(destination, output);
  process.stdout.write(`Prévia social: 720 × 720, ${Math.round(output.length / 1024)} KiB.\n`);
} finally {
  await browser.close();
}
