$ErrorActionPreference = 'Stop'

$repo = (Get-Location).Path
$manifest = Import-Csv (Join-Path $repo 'docs/flowstate/document-manifest.csv')
$docsFiles = Get-ChildItem -Recurse -File (Join-Path $repo 'docs')
$allowedStages = @('N1','N2','N3','N4','N5','N6','N7','N8','N9')
$allowedStatuses = @('active','accepted','deferred','archived','excluded')
$errors = @()

if ($manifest.Count -ne $docsFiles.Count) {
  $errors += "manifest count $($manifest.Count) != docs file count $($docsFiles.Count)"
}

$duplicatePaths = $manifest | Group-Object path | Where-Object Count -gt 1
if ($duplicatePaths) {
  $errors += 'duplicate manifest paths: ' + (($duplicatePaths | Select-Object -ExpandProperty Name) -join ', ')
}

foreach ($row in $manifest) {
  if ($row.stage -notin $allowedStages) {
    $errors += "invalid stage $($row.stage) for $($row.path)"
  }
  if ($row.status -notin $allowedStatuses) {
    $errors += "invalid status $($row.status) for $($row.path)"
  }
  $fullPath = Join-Path $repo $row.path
  if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
    $errors += "missing manifest path $($row.path)"
  }
  if (-not (Test-Path -LiteralPath (Join-Path $repo $row.canonicalArtifact) -PathType Leaf)) {
    $errors += "missing canonical artifact $($row.canonicalArtifact)"
  }
}

$buildManifest = Import-Csv (Join-Path $repo 'docs/flowstate/build-context-manifest.csv')
foreach ($row in $buildManifest) {
  if ($row.class -eq 'generated-output' -and $row.path -match '^Build/(bin|obj|generated|packages)(/|$)') {
    continue
  }
  if ($row.class -notin @('generated-output','verification-evidence','tooling','historical-or-scratch','stale-reference','planned-reference','historical-reference')) {
    $errors += "invalid Build class $($row.class) for $($row.path)"
  }
  if ($row.class -eq 'verification-evidence' -and $row.exists -ne 'True') {
    $errors += "verification evidence path is missing but not marked stale: $($row.path)"
  }
}

if ($errors.Count -gt 0) {
  $errors | ForEach-Object { Write-Error $_ }
  exit 1
}

Write-Output "PASS docs=$($docsFiles.Count) manifest=$($manifest.Count) buildReferences=$($buildManifest.Count)"
