[CmdletBinding()]
param(
  [string]$SnapshotPath = 'Build/generated/version4-source-all-projects-2026-09-07.json',
  [string]$GeneratorPath = 'Build/Tools/Generate-Version4FieldPropertyDeclarations.ps1',

  [ValidateSet('all', 'field', 'property')]
  [string]$MemberKind = 'all'
)

Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$snapshotFullPath = if ([IO.Path]::IsPathRooted($SnapshotPath)) { [IO.Path]::GetFullPath($SnapshotPath) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $SnapshotPath)) }
$generatorFullPath = if ([IO.Path]::IsPathRooted($GeneratorPath)) { [IO.Path]::GetFullPath($GeneratorPath) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $GeneratorPath)) }

if (-not (Test-Path -LiteralPath $snapshotFullPath -PathType Leaf)) {
  throw "Snapshot does not exist: $snapshotFullPath"
}
if (-not (Test-Path -LiteralPath $generatorFullPath -PathType Leaf)) {
  throw "Generator does not exist: $generatorFullPath"
}

$temporaryOutput = Join-Path ([IO.Path]::GetTempPath()) "version4-field-property-declarations-$MemberKind-$([Guid]::NewGuid().ToString('N')).md"

$expectedCount = @{
  all = 7201
  field = 6414
  property = 787
}[$MemberKind]

try {
  $LASTEXITCODE = 0
  & $generatorFullPath -SnapshotPath $snapshotFullPath -OutputPath $temporaryOutput -MemberKind $MemberKind
  if ($LASTEXITCODE -ne 0) {
    throw "Generator failed with exit code $LASTEXITCODE"
  }
  if (-not (Test-Path -LiteralPath $temporaryOutput -PathType Leaf)) {
    throw "Generator did not create output: $temporaryOutput"
  }

  $lines = @(Get-Content -LiteralPath $temporaryOutput)
  $start = [Array]::IndexOf($lines, '<!-- member-declaration-start -->')
  $end = [Array]::IndexOf($lines, '<!-- member-declaration-end -->')
  if ($start -lt 0 -or $end -le $start) {
    throw 'Generated Markdown is missing declaration table markers.'
  }

  $rows = @($lines[($start + 1)..($end - 1)] | Where-Object { $_ -match '^\|\s*\d+\s*\|' })
  $fieldRows = @($rows | Where-Object { $_ -match '^\|\s*\d+\s*\|\s*field\s*\|' })
  $propertyRows = @($rows | Where-Object { $_ -match '^\|\s*\d+\s*\|\s*property\s*\|' })
  $idRows = @($rows | Where-Object {
      $parts = @($_.Split('|') | ForEach-Object { $_.Trim() })
      if ($parts.Count -lt 5) {
        return $false
      }
      $stem = [IO.Path]::GetFileNameWithoutExtension($parts[4])
      $stem -cmatch 'IDs?$'
    })

  if ($rows.Count -ne $expectedCount) { throw "Expected $expectedCount declaration rows for '$MemberKind', got $($rows.Count)." }
  if ($MemberKind -eq 'all' -and $fieldRows.Count -ne 6414) { throw "Expected 6414 field rows, got $($fieldRows.Count)." }
  if ($MemberKind -eq 'all' -and $propertyRows.Count -ne 787) { throw "Expected 787 property rows, got $($propertyRows.Count)." }
  if ($MemberKind -eq 'field' -and $fieldRows.Count -ne 6414) { throw "Expected 6414 field rows, got $($fieldRows.Count)." }
  if ($MemberKind -eq 'property' -and $propertyRows.Count -ne 787) { throw "Expected 787 property rows, got $($propertyRows.Count)." }
  if ($MemberKind -eq 'field' -and $propertyRows.Count -ne 0) { throw "Expected no property rows in field document, got $($propertyRows.Count)." }
  if ($MemberKind -eq 'property' -and $fieldRows.Count -ne 0) { throw "Expected no field rows in property document, got $($fieldRows.Count)." }
  if ($idRows.Count -ne 0) { throw "Expected no ID file rows, got $($idRows.Count)." }

  if ($MemberKind -ne 'property') {
    $mapDelayRows = @($rows | Where-Object {
        $_ -match '\|\s*field\s*\|\s*Terraria\.Main\s*\|\s*Terraria/Main\.cs\s*\|' -and
        $_ -match '\|\s*118\s*\|' -and
        $_ -match 'mapDelay' -and
        $_ -match 'public static int mapDelay = 2;'
      })
    if ($mapDelayRows.Count -ne 1) {
      throw "Expected exactly one Terraria.Main.mapDelay row, got $($mapDelayRows.Count)."
    }
  }

  Write-Output "PASS: kind=$MemberKind, $($rows.Count) declaration rows, $($fieldRows.Count) fields, $($propertyRows.Count) properties"
}
finally {
  if (Test-Path -LiteralPath $temporaryOutput) {
    Remove-Item -LiteralPath $temporaryOutput -Force
  }
}
