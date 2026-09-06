import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { dirname, join, resolve, sep } from 'node:path';

const documentationRoot = new URL('https://docs.tmodloader.net/docs/stable/');
const outputRoot = resolve(process.argv[2] ?? 'Build/generated/tmodloader-api-docs-stable');
const maxDocuments = 20000;
const concurrency = 16;
const maxAttempts = 4;
const pending = [new URL('index.html', documentationRoot)];
const discovered = new Set();
const downloaded = [];
const failures = [];

function normalize(candidate, baseUrl) {
  try {
    const url = new URL(candidate, baseUrl);
    if (url.origin !== documentationRoot.origin ||
        !url.pathname.startsWith(documentationRoot.pathname)) {
      return null;
    }

    url.hash = '';
    url.search = '';
    return url;
  } catch {
    return null;
  }
}

function targetFor(url) {
  const relativePath = decodeURIComponent(url.pathname.slice(documentationRoot.pathname.length));
  const safePath = relativePath === '' || relativePath.endsWith('/')
    ? `${relativePath}index.html`
    : relativePath;
  const target = resolve(outputRoot, safePath);
  if (target !== outputRoot && !target.startsWith(`${outputRoot}${sep}`)) {
    throw new Error(`Refusing to write outside output directory: ${url}`);
  }

  return target;
}

function enqueue(candidate, baseUrl) {
  const url = normalize(candidate, baseUrl);
  if (url === null || discovered.size >= maxDocuments) {
    return;
  }

  const key = url.href;
  if (!discovered.has(key)) {
    discovered.add(key);
    pending.push(url);
  }
}

function discoverLinks(text, baseUrl) {
  const attributePattern = /(?:href|src)\s*=\s*["']([^"']+)["']/gi;
  for (const match of text.matchAll(attributePattern)) {
    enqueue(match[1], baseUrl);
  }

  const cssUrlPattern = /url\(\s*["']?([^'"\s)]+)["']?\s*\)/gi;
  for (const match of text.matchAll(cssUrlPattern)) {
    enqueue(match[1], baseUrl);
  }

  // Doxygen keeps most page URLs in JavaScript arrays rather than HTML attributes.
  const embeddedResourcePattern = /["']([^"']+\.(?:html?|css|js|png|svg|gif|jpe?g|webp))["']/gi;
  for (const match of text.matchAll(embeddedResourcePattern)) {
    enqueue(match[1], baseUrl);
  }
}

async function fetchWithRetry(url) {
  let lastError;
  for (let attempt = 1; attempt <= maxAttempts; attempt += 1) {
    try {
      const response = await fetch(url, {
        headers: { 'user-agent': 'NLTX API documentation mirror' },
        signal: AbortSignal.timeout(20000),
      });
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }
      return response;
    } catch (error) {
      lastError = error;
      if (attempt < maxAttempts) {
        await new Promise(resolve => setTimeout(resolve, attempt * 1500));
      }
    }
  }
  throw lastError;
}

async function download(url) {
  const target = targetFor(url);
  try {
    const extension = target.slice(target.lastIndexOf('.')).toLowerCase();
    try {
      const existing = await readFile(target);
      if (['.html', '.htm', '.css', '.js', '.json'].includes(extension)) {
        discoverLinks(existing.toString('utf8'), url);
      }
      return;
    } catch {
      // The file is not present locally; request it below.
    }

    const response = await fetchWithRetry(url);

    const buffer = Buffer.from(await response.arrayBuffer());
    await mkdir(dirname(target), { recursive: true });
    await writeFile(target, buffer);
    downloaded.push({ url: url.href, path: target, bytes: buffer.length });

    const contentType = response.headers.get('content-type') ?? '';
    if (/^(text\/|application\/(javascript|json))/.test(contentType)) {
      discoverLinks(buffer.toString('utf8'), url);
    }
  } catch (error) {
    failures.push({ url: url.href, error: String(error) });
  }
}

await mkdir(outputRoot, { recursive: true });
discovered.add(pending[0].href);

while (pending.length > 0) {
  const batch = pending.splice(0, concurrency);
  await Promise.all(batch.map(download));
  process.stdout.write(`Downloaded ${downloaded.length}; queued ${pending.length}; failed ${failures.length}\n`);
}

const manifest = {
  documentationRoot: documentationRoot.href,
  downloadedAt: new Date().toISOString(),
  downloadedCount: downloaded.length,
  failureCount: failures.length,
  files: downloaded,
  failures,
};
await writeFile(join(outputRoot, 'mirror-manifest.json'), `${JSON.stringify(manifest, null, 2)}\n`);

if (failures.length > 0) {
  console.error(`Completed with ${failures.length} failed requests; see mirror-manifest.json.`);
  process.exitCode = 1;
}
