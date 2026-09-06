import fs from 'node:fs';
import path from 'node:path';

const root = path.resolve(process.argv[2] ?? 'Build/generated/tmodloader-api-docs-stable');
const missing = new Set();
let html = 0;
for (const relative of fs.readdirSync(root, { recursive: true })) {
  if (!relative.endsWith('.html')) continue;
  html += 1;
  const file = path.join(root, relative);
  const text = fs.readFileSync(file, 'utf8');
  for (const match of text.matchAll(/(?:href|src)=["']([^"']+)["']/gi)) {
    const target = match[1].split('#')[0].split('?')[0];
    if (!target || target.startsWith('#') || target.startsWith('http') || target.startsWith('mailto:') || target.startsWith('javascript:')) continue;
    const resolved = path.resolve(path.dirname(file), target);
    if (!fs.existsSync(resolved)) missing.add(path.relative(root, resolved));
  }
}
console.log(JSON.stringify({ html, missingCount: missing.size, missing: [...missing].slice(0, 100) }, null, 2));
