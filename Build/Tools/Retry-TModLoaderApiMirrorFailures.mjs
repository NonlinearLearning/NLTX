import { readFile, writeFile } from 'node:fs/promises';
import { basename, dirname, join, resolve, sep } from 'node:path';

const root = resolve(process.argv[2] ?? 'Build/generated/tmodloader-api-docs-stable');
const manifestPath = join(root, 'mirror-manifest.json');
const manifest = JSON.parse(await readFile(manifestPath, 'utf8'));
const retryResults = [];

for (const failure of manifest.failures) {
  const url = new URL(failure.url);
  const localPath = resolve(root, decodeURIComponent(url.pathname.replace('/docs/stable/', '')));
  if (!localPath.startsWith(`${root}${sep}`)) {
    throw new Error(`Unexpected mirror target: ${url}`);
  }

  let response;
  try {
    response = await fetch(url, {
      headers: { 'user-agent': 'NLTX API documentation mirror retry' },
      signal: AbortSignal.timeout(30000),
    });
    const result = { url: url.href, status: response.status, saved: false };
    if (response.ok) {
      await writeFile(localPath, Buffer.from(await response.arrayBuffer()));
      result.saved = true;
    }
    retryResults.push(result);
  } catch (error) {
    retryResults.push({ url: url.href, status: null, saved: false, error: String(error) });
  }
}

const resultPath = join(root, 'retry-failures-result.json');
await writeFile(resultPath, `${JSON.stringify({ retriedAt: new Date().toISOString(), retryResults }, null, 2)}\n`);
for (const result of retryResults) {
  console.log(`${result.status ?? 'ERROR'} ${result.saved ? 'saved' : 'not-saved'} ${basename(new URL(result.url).pathname)}`);
}
