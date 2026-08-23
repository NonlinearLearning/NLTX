$ErrorActionPreference = 'Stop'

$repo = (Get-Location).Path
$docsRoot = Join-Path $repo 'docs'
$rows = @()

Get-ChildItem -Recurse -File $docsRoot | ForEach-Object {
  $relative = $_.FullName.Substring($repo.Length + 1).Replace('\', '/')
  $parts = $relative.Split('/')
  $directory = $parts[1]
  $name = $_.Name.ToLowerInvariant()
  $stage = 'N4'
  $status = 'active'
  $evidenceClass = 'decision'
  $canonicalArtifact = 'docs/flowstate/README.md'

  switch ($directory) {
    'archive' {
      $stage = 'N8'
      $status = 'archived'
      $evidenceClass = 'historical'
      $canonicalArtifact = 'docs/flowstate/retrospective.md'
    }
    'migrations' {
      $stage = 'N3'
      $evidenceClass = 'migration-evidence'
      $canonicalArtifact = 'docs/flowstate/scope.md'
    }
    'plans' {
      $stage = if ($name -match 'design') { 'N3' } else { 'N4' }
      $evidenceClass = 'plan'
      $canonicalArtifact = if ($name -eq '2026-08-22-server-ecs-convergence-remaining-work.md') {
        'docs/flowstate/plan/2026-08-22-server-ecs-convergence.md'
      } else {
        'docs/flowstate/plan/2026-08-22-docs-normalization.md'
      }
    }
    'protocol' {
      $stage = 'N6'
      $evidenceClass = 'acceptance-evidence'
      $canonicalArtifact = 'docs/flowstate/dod-checklist.md'
    }
    'research' {
      $stage = 'N3'
      $canonicalArtifact = 'docs/flowstate/requirements.md'
    }
    'server-completion' {
      $stage = 'N6'
      $evidenceClass = 'acceptance-evidence'
      $canonicalArtifact = 'docs/flowstate/dod-checklist.md'
    }
    'worldgen' {
      $stage = 'N6'
      $evidenceClass = 'acceptance-evidence'
      $canonicalArtifact = 'docs/flowstate/dod-checklist.md'
    }
    'flowstate' {
      $stage = 'N1'
      $evidenceClass = 'canonical-process'
    }
  }

  if ($name -match 'blocked|remaining|deferred|open|in-progress|in_progress') {
    $status = 'deferred'
  }
  if ($name -match 'gate|completion|manifest|matrix|coverage|status|parity|verification') {
    $evidenceClass = 'acceptance-evidence'
  }

  $rows += [pscustomobject]@{
    path = $relative
    stage = $stage
    status = $status
    evidenceClass = $evidenceClass
    ownerArea = $directory
    canonicalArtifact = $canonicalArtifact
    lastModified = $_.LastWriteTime.ToString('yyyy-MM-ddTHH:mm:ss')
  }
}

$manifestPath = Join-Path $repo 'docs/flowstate/document-manifest.csv'
$rows | Sort-Object path | Export-Csv -NoTypeInformation -Encoding UTF8 $manifestPath

$references = [ordered]@{}
Get-ChildItem -Recurse -File $docsRoot -Include *.md,*.json,*.csv | ForEach-Object {
  $sourcePath = $_.FullName
  if ($sourcePath -match '[\\/]docs[\\/]flowstate[\\/](document-manifest|build-context-manifest|manifest-summary)') {
    return
  }
  $text = Get-Content -Raw $sourcePath
  if ($null -eq $text) {
    $text = ''
  }
  [regex]::Matches($text, 'Build/[^\s`\)\]\},;]+') | ForEach-Object {
    $value = $_.Value.TrimEnd('.', ',', ':', ';', '`', ']', '}').Replace('\', '/')
    if ($value -match '^Build/(bin|obj|generated|packages)(/|$)') {
      $class = 'generated-output'
    } elseif ($value -match '^Build/(diagnostics|evidence)(/|$)') {
      $class = 'verification-evidence'
    } elseif ($value -match '^Build/Tools(/|$)') {
      $class = 'tooling'
    } else {
      $class = 'historical-or-scratch'
    }
    $isPlanned = $value -match '<[^>]+>|\.\.\.|[()|*]'
    if ($sourcePath -match '[\\/]docs[\\/]cr[\\/]') {
      $class = 'historical-reference'
    } elseif ($isPlanned) {
      $exists = $false
    } else {
      try {
        $exists = Test-Path -LiteralPath (Join-Path $repo $value)
      } catch {
        $exists = $false
        $isPlanned = $true
      }
    }
    if ($isPlanned) {
      $class = 'planned-reference'
    } elseif (-not $exists -and $class -eq 'verification-evidence') {
      $class = 'stale-reference'
    }
    if (-not $references.Contains($value)) {
      $references[$value] = [pscustomobject]@{
        path = $value
        class = $class
        exists = $exists
        referencedBy = $sourcePath.Substring($repo.Length + 1).Replace('\', '/')
      }
    }
  }
}

$buildManifestPath = Join-Path $repo 'docs/flowstate/build-context-manifest.csv'
$references.Values | Sort-Object path | Export-Csv -NoTypeInformation -Encoding UTF8 $buildManifestPath

@(
  '# Manifest Summary'
  ''
  "Generated: $(Get-Date -Format s)"
  ''
  "- docs files indexed: $($rows.Count)"
  "- Build paths referenced by docs: $($references.Count)"
  '- canonical process root: `docs/flowstate/`'
  '- private process root: `.agent-workplace/`'
  ''
  'The CSV files are generated inventory artifacts. They do not replace the referenced evidence.'
) | Set-Content -Encoding UTF8 (Join-Path $repo 'docs/flowstate/manifest-summary.md')

Write-Output "DOCS=$($rows.Count) BUILD_REFS=$($references.Count)"
