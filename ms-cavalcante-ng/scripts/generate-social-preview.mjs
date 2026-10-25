import { readFile, writeFile } from 'node:fs/promises';
import { chromium } from '@playwright/test';

const source = new URL('../public/tractor.svg', import.meta.url);
const destination = new URL('../public/images/tractor-share-v1.png', import.meta.url);
const svg = await readFile(source, 'utf8');
const browser = await chromium.launch({ headless: true });
try {
  const page = await browser.newPage();
  const encoded = await page.evaluate(async (svgData) => {
    const image = new Image();
    image.src = `data:image/svg+xml;base64,${btoa(svgData)}`;
    await image.decode();
    const canvas = document.createElement('canvas');
    canvas.width = 1200;
    canvas.height = 630;
    const context = canvas.getContext('2d');
    if (!context) throw new Error('Canvas indisponível.');
    context.fillStyle = '#f7f7f2';
    context.fillRect(0, 0, 1200, 630);
    context.drawImage(image, 345, 60, 510, 510);
    return canvas.toDataURL('image/png').split(',')[1];
  }, svg);
  const output = Buffer.from(encoded, 'base64');
  if (output.length > 150_000) throw new Error('A prévia excedeu o limite de 150 KB.');
  await writeFile(destination, output);
  process.stdout.write(`Prévia social: 1200 × 630, ${Math.round(output.length / 1024)} KiB.\n`);
} finally {
  await browser.close();
}
