[CmdletBinding()]
param(
  [string]$SnapshotPath = 'Build/generated/version4-source-all-projects-2026-09-07.json',
  [string]$GeneratorPath = 'Build/Tools/Generate-Version4FieldPropertyInventory.ps1'
)

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$snapshotFullPath = if ([IO.Path]::IsPathRooted($SnapshotPath)) { [IO.Path]::GetFullPath($SnapshotPath) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $SnapshotPath)) }
$generatorFullPath = if ([IO.Path]::IsPathRooted($GeneratorPath)) { [IO.Path]::GetFullPath($GeneratorPath) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $GeneratorPath)) }

if (-not (Test-Path -LiteralPath $snapshotFullPath)) {
  throw "Snapshot does not exist: $snapshotFullPath"
}
if (-not (Test-Path -LiteralPath $generatorFullPath)) {
  throw "Generator does not exist: $generatorFullPath"
}

$snapshot = Get-Content -Raw -LiteralPath $snapshotFullPath | ConvertFrom-Json
$temporaryOutput = Join-Path ([IO.Path]::GetTempPath()) "version4-field-property-inventory-$([Guid]::NewGuid().ToString('N')).md"

try {
  $LASTEXITCODE = 0
  & $generatorFullPath -SnapshotPath $snapshotFullPath -OutputPath $temporaryOutput
  if ($LASTEXITCODE -ne 0) {
    throw "Generator failed with exit code $LASTEXITCODE"
  }

  if (-not (Test-Path -LiteralPath $temporaryOutput)) {
    throw "Generator did not create output: $temporaryOutput"
  }

  $lines = @(Get-Content -LiteralPath $temporaryOutput)
  $fileStart = [Array]::IndexOf($lines, '<!-- file-index-start -->')
  $fileEnd = [Array]::IndexOf($lines, '<!-- file-index-end -->')
  $memberStart = [Array]::IndexOf($lines, '<!-- member-detail-start -->')
  $memberEnd = [Array]::IndexOf($lines, '<!-- member-detail-end -->')
  if ($fileStart -lt 0 -or $fileEnd -le $fileStart -or $memberStart -lt 0 -or $memberEnd -le $memberStart) {
    throw 'Generated Markdown is missing required table markers.'
  }

  $fileRows = @($lines[($fileStart + 1)..($fileEnd - 1)] | Where-Object { $_ -match '^\|\s*\d+\s*\|' })
  $memberRows = @($lines[($memberStart + 1)..($memberEnd - 1)] | Where-Object { $_ -match '^\|\s*\d+\s*\|' })
  $idRows = @($fileRows + $memberRows | Where-Object {
      $parts = @($_.Split('|') | ForEach-Object { $_.Trim() })
      $parts.Count -gt 2 -and $parts[2] -cmatch 'IDs?\.cs$'
    })
  $invalidMemberRows = @($memberRows | Where-Object {
      $parts = @($_.Split('|') | ForEach-Object { $_.Trim() })
      $parts.Count -lt 10 -or $parts[3] -notmatch '^\d+$' -or [int]$parts[3] -lt 1
    })

  if ($fileRows.Count -ne 915) { throw "Expected 915 file rows, got $($fileRows.Count)." }
  if ($memberRows.Count -ne 7201) { throw "Expected 7201 member rows, got $($memberRows.Count)." }
  if ($idRows.Count -ne 0) { throw "Expected no ID file rows, got $($idRows.Count)." }
  if ($invalidMemberRows.Count -ne 0) { throw "Found $($invalidMemberRows.Count) invalid member rows." }
  if ($snapshot.completeness -ne 'complete') { throw "Snapshot is not complete: $($snapshot.completeness)" }
  if (@($snapshot.diagnostics).Count -ne 0) { throw "Snapshot contains $(@($snapshot.diagnostics).Count) diagnostics." }

  $fieldRows = @($memberRows | Where-Object {
      $parts = @($_.Split('|') | ForEach-Object { $_.Trim() })
      $parts.Count -gt 5 -and $parts[5] -eq 'field'
    })
  $propertyRows = @($memberRows | Where-Object {
      $parts = @($_.Split('|') | ForEach-Object { $_.Trim() })
      $parts.Count -gt 5 -and $parts[5] -eq 'property'
    })
  if ($fieldRows.Count -ne 6414) { throw "Expected 6414 field rows, got $($fieldRows.Count)." }
  if ($propertyRows.Count -ne 787) { throw "Expected 787 property rows, got $($propertyRows.Count)." }

  Write-Output "PASS: $($fileRows.Count) files, $($memberRows.Count) field/property rows, $($fieldRows.Count) fields, $($propertyRows.Count) properties"
}
finally {
  if (Test-Path -LiteralPath $temporaryOutput) {
    Remove-Item -LiteralPath $temporaryOutput -Force
  }
}
