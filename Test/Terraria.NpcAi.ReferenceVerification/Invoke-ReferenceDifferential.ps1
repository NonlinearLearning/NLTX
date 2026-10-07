param(
  [string]$ReferenceRoot,
  [switch]$CompareOnly,
  [string]$GateCasesPath,
  [string]$GateOriginalCapturePath,
  [string]$GateSourceCapturePath,
  [string]$GateProvenancePath,
  [string]$GateReportPath
)

$ErrorActionPreference = 'Stop'
$referenceTreeName = -join @(
  [char]0x65E0, [char]0x4EFB, [char]0x4F55, [char]0x5220, [char]0x51CF,
  [char]0x901A, [char]0x8FC7, [char]0x7F16, [char]0x8BD1
)
$authorizedReferenceRoot = Join-Path 'D:\TRbackup' $referenceTreeName
if ([string]::IsNullOrWhiteSpace($ReferenceRoot)) {
  $ReferenceRoot = $authorizedReferenceRoot
}
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$referenceRoot = (Resolve-Path -LiteralPath $ReferenceRoot).Path
$testRoot = $PSScriptRoot
$buildRoot = Join-Path $repoRoot 'Build\NpcAiReferenceVerification'
$runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
$diagnosticsRoot = Join-Path $repoRoot "Build\diagnostics\NpcAiReferenceVerification\$runId"
$referenceSourceRoot = Join-Path $buildRoot "ReferenceSource-$runId"
$sourceServerOutput = Join-Path $repoRoot 'Build\bin\NpcAiReferenceVerification\SourceServer\Debug\net40'
$referenceHarnessOutput = Join-Path $repoRoot 'Build\bin\NpcAiReferenceVerification\ReferenceHarness'
$verifierProject = Join-Path $testRoot 'Terraria.NpcAi.ReferenceVerification.csproj'
$verifierProjectName = [IO.Path]::GetFileNameWithoutExtension($verifierProject)
$verifierAssemblyPath = Join-Path $repoRoot "Build\bin\$verifierProjectName\Debug\net10.0\$verifierProjectName.dll"
$referenceProject = Join-Path $referenceSourceRoot 'TerrariaServer.csproj'
$referenceOriginalExe = Join-Path $referenceRoot 'bin\Debug\net40\TerrariaServer.exe'
$referenceBuiltExe = Join-Path $sourceServerOutput 'TerrariaServer.exe'
$referenceBuiltPdb = Join-Path $sourceServerOutput 'TerrariaServer.pdb'
$harnessSource = Join-Path $testRoot 'ReferenceHarness.cs'
$harnessExe = Join-Path $referenceHarnessOutput 'ReferenceHarness.exe'
$frameworkCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'

$expectedPinnedHashes = [ordered]@{
  'Terraria/NPC.cs' = 'ed8aa2302730a9e046ef310e543204fba391bd35b1c9c0b213c8065dfbbe39f0'
  'Terraria/Main.cs' = 'e24e61c9903bb7995f47e36c51edbe43481b643226861b717020877e7e22b63f'
  'Terraria.ID/NPCID.cs' = 'e040b9cfffd57842c0099f11322ee5ec1dddd605743a25bae5415db31b0c025d'
}

function Get-Sha256([string]$Path) {
  return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-SourceManifest([string]$Root) {
  $manifest = [ordered]@{}
  $normalizedRoot = $Root.TrimEnd('\')
  $files = Get-ChildItem -LiteralPath $Root -Recurse -File | Where-Object {
    $relative = $_.FullName.Substring($normalizedRoot.Length).TrimStart('\')
    $relative -notmatch '^(bin|obj|\.git|\.vs|\.idea|packages|TestResults)(\\|$)'
  } | Sort-Object { $_.FullName.Substring($normalizedRoot.Length).TrimStart('\').Replace('\', '/') }
  foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($normalizedRoot.Length).TrimStart('\').Replace('\', '/')
    $manifest[$relativePath] = Get-Sha256 $file.FullName
  }
  $canonical = ($manifest.GetEnumerator() | ForEach-Object { "$($_.Key):$($_.Value)" }) -join "`n"
  $algorithm = [Security.Cryptography.SHA256]::Create()
  try {
    $hashBytes = $algorithm.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonical))
  }
  finally {
    $algorithm.Dispose()
  }
  $manifestHash = ([BitConverter]::ToString($hashBytes)).Replace('-', '').ToLowerInvariant()
  return [pscustomobject]@{
    Hash = $manifestHash
    Files = $manifest
  }
}

function Get-VerifierBuildManifest([string]$RepositoryRoot, [string]$ProjectPath) {
  $projectDirectory = Split-Path -Parent $ProjectPath
  $repositoryRoot = $RepositoryRoot.TrimEnd('\')
  $projectXml = [xml](Get-Content -Raw -LiteralPath $ProjectPath)
  $removedSources = @($projectXml.SelectNodes('//Compile[@Remove]') | ForEach-Object {
    $_.GetAttribute('Remove')
  })
  $sourcePaths = [System.Collections.Generic.List[string]]::new()
  $sourcePaths.Add((Resolve-Path -LiteralPath $ProjectPath).Path)

  Get-ChildItem -LiteralPath $projectDirectory -Recurse -File -Filter '*.cs' | Where-Object {
    $_.FullName -notmatch '\\(bin|obj)\\'
  } | ForEach-Object {
    $relative = $_.FullName.Substring($projectDirectory.Length).TrimStart('\').Replace('\', '/')
    $isRemoved = $false
    foreach ($pattern in $removedSources) {
      if ($relative -like $pattern.Replace('\', '/')) {
        $isRemoved = $true
        break
      }
    }
    if (-not $isRemoved) {
      $sourcePaths.Add($_.FullName)
    }
  }

  foreach ($compileNode in $projectXml.SelectNodes('//Compile[@Include]')) {
    $include = $compileNode.GetAttribute('Include')
    if ($include -match '[*?]') {
      throw "Verifier source manifest does not accept wildcard Compile Include '$include'."
    }
    $includedPath = [IO.Path]::GetFullPath((Join-Path $projectDirectory $include))
    if (-not (Test-Path -LiteralPath $includedPath -PathType Leaf)) {
      throw "Verifier compile input does not exist: $includedPath"
    }
    $sourcePaths.Add($includedPath)
  }

  $manifest = [System.Collections.Generic.SortedDictionary[string, string]]::new([StringComparer]::Ordinal)
  foreach ($sourcePath in $sourcePaths | Select-Object -Unique) {
    $relativePath = $sourcePath.Substring($repositoryRoot.Length).TrimStart('\').Replace('\', '/')
    if (-not $relativePath.StartsWith('Test/Terraria.NpcAi.ReferenceVerification/', [StringComparison]::Ordinal) -and
        -not $relativePath.StartsWith('src/NSSLC/Component/Npc/', [StringComparison]::Ordinal)) {
      throw "Verifier compile input is outside the supported project/profile roots: $relativePath"
    }
    $manifest.Add($relativePath, (Get-Sha256 $sourcePath))
  }

  $canonical = ($manifest.GetEnumerator() | ForEach-Object { "$($_.Key):$($_.Value)" }) -join "`n"
  $algorithm = [Security.Cryptography.SHA256]::Create()
  try {
    $hashBytes = $algorithm.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonical))
  }
  finally {
    $algorithm.Dispose()
  }
  $manifestHash = ([BitConverter]::ToString($hashBytes)).Replace('-', '').ToLowerInvariant()
  return [pscustomobject]@{
    Hash = $manifestHash
    Files = $manifest
  }
}

function Invoke-LoggedCommand([string]$FilePath, [string[]]$Arguments, [string]$LogPath) {
  $argumentLine = ($Arguments | ForEach-Object { '"' + $_.Replace('"', '\"') + '"' }) -join ' '
  "COMMAND: $FilePath $argumentLine" | Set-Content -LiteralPath $LogPath -Encoding utf8
  $commandOutput = & $FilePath @Arguments 2>&1
  $exitCode = $LASTEXITCODE
  $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
  foreach ($line in $commandOutput) {
    $lineText = [string]$line
    [Console]::Out.WriteLine($lineText)
    [System.IO.File]::AppendAllText($LogPath, $lineText + [Environment]::NewLine, $utf8WithoutBom)
  }
  "EXIT_CODE: $exitCode" | Add-Content -LiteralPath $LogPath -Encoding utf8
  if ($exitCode -ne 0) {
    throw "Command failed with exit code $exitCode. Log: $LogPath"
  }
  return $exitCode
}

function Invoke-CompareGate(
  [string]$CasesPath,
  [string]$OriginalCapturePath,
  [string]$SourceCapturePath,
  [string]$ProvenancePath,
  [string]$ReportPath,
  [string]$LogPath) {
  $arguments = @(
    'run', '--project', $verifierProject, '--no-build', '--no-restore', '--',
    '--compare', '--cases', $CasesPath,
    '--original', $OriginalCapturePath,
    '--source-build', $SourceCapturePath,
    '--provenance', $ProvenancePath,
    '--report', $ReportPath
  )
  "COMMAND: dotnet $($arguments -join ' ')" | Set-Content -LiteralPath $LogPath -Encoding utf8
  $previousErrorActionPreference = $ErrorActionPreference
  try {
    $ErrorActionPreference = 'Continue'
    $commandOutput = & dotnet @arguments 2>&1
    $exitCode = $LASTEXITCODE
  }
  finally {
    $ErrorActionPreference = $previousErrorActionPreference
  }
  $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
  foreach ($line in $commandOutput) {
    $lineText = [string]$line
    [Console]::Out.WriteLine($lineText)
    [System.IO.File]::AppendAllText($LogPath, $lineText + [Environment]::NewLine, $utf8WithoutBom)
  }
  "EXIT_CODE: $exitCode" | Add-Content -LiteralPath $LogPath -Encoding utf8
  return $exitCode
}

function Invoke-Robocopy([string]$Source, [string]$Destination, [string]$LogPath) {
  $arguments = @(
    $Source,
    $Destination,
    '/E',
    '/COPY:DAT',
    '/R:1',
    '/W:1',
    '/XJ',
    '/XD', 'bin', 'obj', '.git', '.vs', '.idea', 'packages', 'TestResults',
    '/NFL', '/NDL', '/NJH', '/NJS', '/NP', '/NC', '/NS',
    "/LOG:$LogPath"
  )
  & robocopy @arguments
  $exitCode = $LASTEXITCODE
  "robocopy exit code: $exitCode" | Add-Content -LiteralPath $LogPath -Encoding utf8
  if ($exitCode -ge 8) {
    throw "Reference source copy failed with robocopy exit code $exitCode. Log: $LogPath"
  }
}

function Get-BuildCount([string]$LogPath, [string]$Kind) {
  $text = Get-Content -Raw -LiteralPath $LogPath
  $localizedWord = if ($Kind -eq 'Warning') {
    -join @([char]0x8B66, [char]0x544A)
  }
  else {
    -join @([char]0x9519, [char]0x8BEF)
  }
  $localizedLabel = [string][char]0x4E2A + $localizedWord
  $pattern = "(?im)^\s*(\d+)\s+$Kind(?:\(s\))?\s*$|^\s*(\d+)\s+$([regex]::Escape($localizedLabel))\s*$"
  $matches = [regex]::Matches($text, $pattern)
  if ($matches.Count -eq 0) {
    return -1
  }
  $match = $matches[$matches.Count - 1]
  if ($match.Groups[1].Success) {
    return [int]$match.Groups[1].Value
  }
  return [int]$match.Groups[2].Value
}

if ($CompareOnly) {
  $gateInputs = @($GateCasesPath, $GateOriginalCapturePath, $GateSourceCapturePath, $GateProvenancePath)
  if ($gateInputs | Where-Object { [string]::IsNullOrWhiteSpace($_) -or -not (Test-Path -LiteralPath $_ -PathType Leaf) }) {
    throw 'CompareOnly requires existing case, original capture, source capture, and provenance files.'
  }
  if ([string]::IsNullOrWhiteSpace($GateReportPath)) {
    throw 'CompareOnly requires GateReportPath so the diagnostic comparison report has an explicit output.'
  }
  New-Item -ItemType Directory -Path $diagnosticsRoot -Force | Out-Null
  New-Item -ItemType Directory -Path (Split-Path -Parent $GateReportPath) -Force | Out-Null
  $compareLog = Join-Path $diagnosticsRoot 'differential-compare-only.log'
  $compareExitCode = Invoke-CompareGate $GateCasesPath $GateOriginalCapturePath $GateSourceCapturePath $GateProvenancePath $GateReportPath $compareLog
  Write-Host "Differential report: $GateReportPath"
  Write-Host "Diagnostics: $diagnosticsRoot"
  exit $compareExitCode
}

if (-not $referenceRoot.StartsWith($authorizedReferenceRoot, [StringComparison]::OrdinalIgnoreCase)) {
  throw "ReferenceRoot is outside the authorized read-only tree: $referenceRoot"
}
if ($referenceRoot.StartsWith($repoRoot, [StringComparison]::OrdinalIgnoreCase)) {
  throw 'The read-only reference tree must remain outside this writable repository.'
}
if (-not (Test-Path -LiteralPath $referenceOriginalExe -PathType Leaf)) {
  throw "The read-only reference executable does not exist: $referenceOriginalExe"
}
if (-not (Test-Path -LiteralPath $frameworkCompiler -PathType Leaf)) {
  throw "The .NET Framework 4 compiler was not found: $frameworkCompiler"
}

New-Item -ItemType Directory -Path $diagnosticsRoot -Force | Out-Null
New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null
New-Item -ItemType Directory -Path $sourceServerOutput -Force | Out-Null
New-Item -ItemType Directory -Path $referenceHarnessOutput -Force | Out-Null

$verifierRestoreLog = Join-Path $diagnosticsRoot 'verifier-restore.log'
Invoke-LoggedCommand 'dotnet' @('restore', $verifierProject, '--verbosity', 'minimal') $verifierRestoreLog | Out-Null
$verifierSourceManifestBeforeBuild = Get-VerifierBuildManifest $repoRoot $verifierProject
$verifierBuildLog = Join-Path $diagnosticsRoot 'verifier-build.log'
Invoke-LoggedCommand 'dotnet' @('build', $verifierProject, '--no-restore', '--configuration', 'Debug', '--nologo', '-v:minimal') $verifierBuildLog | Out-Null
$verifierBuildWarnings = Get-BuildCount $verifierBuildLog 'Warning'
$verifierBuildErrors = Get-BuildCount $verifierBuildLog 'Error'
if ($verifierBuildWarnings -ne 0 -or $verifierBuildErrors -ne 0) {
  throw "Differential verifier build did not complete cleanly ($verifierBuildWarnings warnings, $verifierBuildErrors errors)."
}
if (-not (Test-Path -LiteralPath $verifierAssemblyPath -PathType Leaf)) {
  throw "Verifier assembly was not found under the configured Build/bin output: $verifierAssemblyPath"
}
$verifierAssemblyHash = Get-Sha256 $verifierAssemblyPath
$verifierSourceManifest = Get-VerifierBuildManifest $repoRoot $verifierProject
if ($verifierSourceManifest.Hash -ne $verifierSourceManifestBeforeBuild.Hash) {
  throw 'Verifier source inputs changed during the verifier build; no build provenance was recorded.'
}

$pinnedSourceHashesBefore = [ordered]@{}
foreach ($entry in $expectedPinnedHashes.GetEnumerator()) {
  $sourcePath = Join-Path $referenceRoot $entry.Key.Replace('/', '\')
  $actualHash = Get-Sha256 $sourcePath
  if ($actualHash -ne $entry.Value) {
    throw "Pinned source identity mismatch for $($entry.Key): expected $($entry.Value), got $actualHash"
  }
  $pinnedSourceHashesBefore[$entry.Key] = $actualHash
}
$originalExeHashBefore = Get-Sha256 $referenceOriginalExe
$originalProjectPath = Join-Path $referenceRoot 'TerrariaServer.csproj'
$originalProjectHash = Get-Sha256 $originalProjectPath

Invoke-Robocopy $referenceRoot $referenceSourceRoot (Join-Path $diagnosticsRoot 'reference-copy.log')
$sourceManifest = Get-SourceManifest $referenceRoot
$copiedManifest = Get-SourceManifest $referenceSourceRoot
if ($sourceManifest.Hash -ne $copiedManifest.Hash) {
  throw "Isolated source copy manifest mismatch: source $($sourceManifest.Hash), copy $($copiedManifest.Hash)"
}

$copiedProjectOriginalHash = Get-Sha256 $referenceProject
$copiedProjectText = Get-Content -Raw -LiteralPath $referenceProject
$hintPathPattern = '<HintPath>[^<]*\\lodes\\TR\\Backup\\New9\.27\\LIBS\\([^<]+)</HintPath>'
$hintMatches = [regex]::Matches($copiedProjectText, $hintPathPattern)
if ($hintMatches.Count -ne 6) {
  throw "Expected to redirect six stale dependency hints in the copied project; found $($hintMatches.Count)."
}
$copiedProjectText = [regex]::Replace($copiedProjectText, $hintPathPattern, '<HintPath>.\$1</HintPath>')
if ($copiedProjectText -notmatch '<HintPath>\.\\ReLogic\.dll</HintPath>' -or
    $copiedProjectText -notmatch '<HintPath>\.\\Newtonsoft\.Json\.dll</HintPath>') {
  throw 'The copied project dependency hints were not redirected to DLLs inside the isolated source copy.'
}
Set-Content -LiteralPath $referenceProject -Value $copiedProjectText -Encoding utf8
$copiedProjectPatchedHash = Get-Sha256 $referenceProject

$repoBuildRootXml = (Join-Path $repoRoot 'Build').Replace('&', '&amp;')
$localProps = @"
<Project>
  <PropertyGroup>
    <RepositoryBuildRoot>$repoBuildRootXml\</RepositoryBuildRoot>
    <BaseOutputPath>`$(RepositoryBuildRoot)bin\NpcAiReferenceVerification\SourceServer\</BaseOutputPath>
    <BaseIntermediateOutputPath>`$(RepositoryBuildRoot)obj\NpcAiReferenceVerification\SourceServer\</BaseIntermediateOutputPath>
    <RestorePackagesPath>`$(RepositoryBuildRoot)packages\</RestorePackagesPath>
  </PropertyGroup>
</Project>
"@
Set-Content -LiteralPath (Join-Path $referenceSourceRoot 'Directory.Build.props') -Value $localProps -Encoding utf8

$sdkVersionLog = Join-Path $diagnosticsRoot 'dotnet-version.log'
Invoke-LoggedCommand 'dotnet' @('--version') $sdkVersionLog | Out-Null
$resolvedPropertiesLog = Join-Path $diagnosticsRoot 'resolved-msbuild-properties.json'
Invoke-LoggedCommand 'dotnet' @('msbuild', $referenceProject, '-nologo', '-getProperty:BaseOutputPath,BaseIntermediateOutputPath,TargetPath,TargetFramework,PlatformTarget') $resolvedPropertiesLog | Out-Null

$restoreLog = Join-Path $diagnosticsRoot 'reference-source-restore.log'
Invoke-LoggedCommand 'dotnet' @('restore', $referenceProject, '--verbosity', 'minimal') $restoreLog | Out-Null
$buildLog = Join-Path $diagnosticsRoot 'reference-source-build.log'
$buildBinaryLog = Join-Path $diagnosticsRoot 'reference-source-build.binlog'
$buildArguments = @('build', $referenceProject, '--no-restore', '--configuration', 'Debug', '--nologo', '-v:minimal', "-bl:$buildBinaryLog")
$buildCommand = "dotnet " + ($buildArguments -join ' ')
Invoke-LoggedCommand 'dotnet' $buildArguments $buildLog | Out-Null
$buildWarnings = Get-BuildCount $buildLog 'Warning'
$buildErrors = Get-BuildCount $buildLog 'Error'
if ($buildWarnings -lt 0 -or $buildErrors -lt 0) {
  throw "Could not derive warning/error counts from the source build log: $buildLog"
}
if ($buildErrors -ne 0) {
  throw "Isolated reference source build reported $buildErrors errors (and $buildWarnings warnings)."
}
if (-not (Test-Path -LiteralPath $referenceBuiltExe -PathType Leaf)) {
  throw "Isolated source build executable was not found at the expected Build/bin path: $referenceBuiltExe"
}
if (-not (Test-Path -LiteralPath $referenceBuiltPdb -PathType Leaf)) {
  throw "Isolated source build PDB was not found at the expected Build/bin path: $referenceBuiltPdb"
}

$compilerLog = Join-Path $diagnosticsRoot 'reference-harness-compile.log'
Invoke-LoggedCommand $frameworkCompiler @(
  '/nologo',
  '/target:exe',
  '/platform:x86',
  '/r:System.Web.Extensions.dll',
  "/out:$harnessExe",
  $harnessSource
) $compilerLog | Out-Null

$casesPath = Join-Path $diagnosticsRoot 'differential-cases.json'
$prepareLog = Join-Path $diagnosticsRoot 'prepare-cases.log'
Invoke-LoggedCommand 'dotnet' @(
  'run', '--project', $verifierProject, '--no-build', '--no-restore', '--',
  '--prepare-cases', '--output', $casesPath
) $prepareLog | Out-Null

$originalCapturePath = Join-Path $diagnosticsRoot 'original-executable-capture.json'
$sourceBuildCapturePath = Join-Path $diagnosticsRoot 'isolated-source-build-capture.json'
$originalCaptureLog = Join-Path $diagnosticsRoot 'original-executable-capture.log'
$sourceBuildCaptureLog = Join-Path $diagnosticsRoot 'isolated-source-build-capture.log'
$originalSandbox = Join-Path $diagnosticsRoot 'original-runtime-sandbox'
$sourceBuildSandbox = Join-Path $diagnosticsRoot 'source-build-runtime-sandbox'
Invoke-LoggedCommand $harnessExe @('--assembly', $referenceOriginalExe, '--cases', $casesPath, '--output', $originalCapturePath, '--sandbox', $originalSandbox) $originalCaptureLog | Out-Null
Invoke-LoggedCommand $harnessExe @('--assembly', $referenceBuiltExe, '--cases', $casesPath, '--output', $sourceBuildCapturePath, '--sandbox', $sourceBuildSandbox) $sourceBuildCaptureLog | Out-Null

$pinnedSourceHashesAfter = [ordered]@{}
foreach ($entry in $expectedPinnedHashes.GetEnumerator()) {
  $sourcePath = Join-Path $referenceRoot $entry.Key.Replace('/', '\')
  $actualHash = Get-Sha256 $sourcePath
  if ($actualHash -ne $pinnedSourceHashesBefore[$entry.Key]) {
    throw "Read-only reference source changed during verification: $($entry.Key)"
  }
  $pinnedSourceHashesAfter[$entry.Key] = $actualHash
}
$originalExeHashAfter = Get-Sha256 $referenceOriginalExe
if ($originalExeHashAfter -ne $originalExeHashBefore) {
  throw 'The read-only reference executable hash changed during verification.'
}
$sourceManifestAfter = Get-SourceManifest $referenceRoot
if ($sourceManifestAfter.Hash -ne $sourceManifest.Hash) {
  throw 'The read-only reference source manifest changed during verification.'
}

$provenancePath = Join-Path $diagnosticsRoot 'source-build-provenance.json'
$reportPath = Join-Path $diagnosticsRoot 'differential-report.json'
$sdkVersionOutput = Get-Content -Raw -LiteralPath $sdkVersionLog
$sdkVersionMatch = [regex]::Match($sdkVersionOutput, '(?m)^([0-9]+\.[0-9]+\.[0-9]+)\r?$')
if (-not $sdkVersionMatch.Success) {
  throw "Could not parse the selected .NET SDK version from $sdkVersionLog"
}
$provenance = [ordered]@{
  referenceRoot = $referenceRoot
  referenceTreeWasReadOnly = $true
  pinnedSourceHashes = $pinnedSourceHashesAfter
  pinnedSourceManifestSha256 = $sourceManifest.Hash
  pinnedSourceFileCount = $sourceManifest.Files.Count
  originalServerExeSha256 = $originalExeHashAfter
  originalServerExeSha256Before = $originalExeHashBefore
  originalServerExePath = $referenceOriginalExe
  originalServerExeUnchanged = ($originalExeHashAfter -eq $originalExeHashBefore)
  originalProjectSha256 = $originalProjectHash
  copiedProjectBeforePatchSha256 = $copiedProjectOriginalHash
  copiedProjectAfterPatchSha256 = $copiedProjectPatchedHash
  copiedProjectPath = $referenceProject
  builtServerExeSha256 = Get-Sha256 $referenceBuiltExe
  builtServerPdbSha256 = Get-Sha256 $referenceBuiltPdb
  builtExecutablePath = $referenceBuiltExe
  buildCommand = $buildCommand
  buildExitCode = 0
  buildWarningCount = $buildWarnings
  buildErrorCount = $buildErrors
  buildLogPath = $buildLog
  verifierBuildLogPath = $verifierBuildLog
  verifierBuildWarningCount = $verifierBuildWarnings
  verifierBuildErrorCount = $verifierBuildErrors
  verifierAssemblyPath = $verifierAssemblyPath
  verifierAssemblySha256 = $verifierAssemblyHash
  verifierSourceManifestSha256 = $verifierSourceManifest.Hash
  verifierSourceFiles = $verifierSourceManifest.Files
  binaryLogPath = $buildBinaryLog
  resolvedPropertiesLogPath = $resolvedPropertiesLog
  dotnetSdkVersion = $sdkVersionMatch.Groups[1].Value
  patchedProjectExplanation = 'Only the six stale HintPath values were redirected to the matching DLLs copied beside the project; repository Build outputs were selected by a local Directory.Build.props in the copied tree. The pinned reference tree was not edited.'
  diagnosticsRoot = $diagnosticsRoot
  referenceCopyLogPath = Join-Path $diagnosticsRoot 'reference-copy.log'
  referenceHarnessPath = $harnessExe
  referenceHarnessCompileLogPath = $compilerLog
  caseFilePath = $casesPath
  originalCapturePath = $originalCapturePath
  originalCaptureLogPath = $originalCaptureLog
  originalRuntimeSandbox = $originalSandbox
  sourceBuildCapturePath = $sourceBuildCapturePath
  sourceBuildCaptureLogPath = $sourceBuildCaptureLog
  sourceBuildRuntimeSandbox = $sourceBuildSandbox
  differentialReportPath = $reportPath
}
Set-Content -LiteralPath $provenancePath -Value ($provenance | ConvertTo-Json -Depth 8) -Encoding utf8

$compareLog = Join-Path $diagnosticsRoot 'differential-compare.log'
$compareExitCode = Invoke-CompareGate $casesPath $originalCapturePath $sourceBuildCapturePath $provenancePath $reportPath $compareLog

if ($compareExitCode -eq 0) {
  Write-Host "NPC AI reference differential gate passed for the declared sample scope."
}
else {
  Write-Host "NPC AI reference differential gate failed with exit code $compareExitCode; the JSON report and captures are preserved." -ForegroundColor Yellow
}
Write-Host "Pinned source: $referenceRoot"
Write-Host "Source build: $referenceBuiltExe"
Write-Host "Report: $reportPath"
Write-Host "Diagnostics: $diagnosticsRoot"
exit $compareExitCode
