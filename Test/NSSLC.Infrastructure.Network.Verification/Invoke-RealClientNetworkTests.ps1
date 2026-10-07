param(
  [string] $ClientRoot = 'D:\TRbackup\客户端',
  [string] $OutputDirectory,
  [string] $WorldFile,
  [switch] $ProbeCurrentServer
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$ClientRoot = [IO.Path]::GetFullPath($ClientRoot)
if (-not $OutputDirectory) {
  $OutputDirectory = if ($ProbeCurrentServer) {
    Join-Path $repositoryRoot 'Build/diagnostics/CurrentServerProbe/ProductionHost'
  } else {
    Join-Path $repositoryRoot 'Build/diagnostics/RealClientNetwork'
  }
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
if ($ProbeCurrentServer) {
  $probeScript = Join-Path $PSScriptRoot 'Invoke-NetworkServerRealClientProbe.ps1'
  $probeParameters = @{ ClientRoot = $ClientRoot; OutputDirectory = $OutputDirectory }
  if ($WorldFile) {
    $probeParameters.WorldFile = $WorldFile
  }
  & $probeScript @probeParameters
  return
}
$clientProject = Join-Path $ClientRoot 'Terraria.csproj'
$gatewayProject = Join-Path $PSScriptRoot 'NSSLC.Infrastructure.Network.Verification.csproj'
$clientExecutable = Join-Path $ClientRoot 'Build/bin/Terraria/Debug/net40/Terraria.NetworkTests.exe'
$records = [Collections.Generic.List[object]]::new()

function Invoke-RecordedDotnet {
  param([string] $Name, [string[]] $CommandArguments, [string] $Project, [string] $Artifact)
  $log = Join-Path $OutputDirectory ($Name + '.log')
  $lines = & dotnet @CommandArguments 2>&1
  $code = $LASTEXITCODE
  $lines | Set-Content -LiteralPath $log -Encoding utf8
  $warningCount = $null
  $errorCount = $null
  foreach ($line in $lines) {
    if ($line -match '^\s*(\d+)\s+(?:Warning\(s\)|个警告)') {
      $warningCount = [int] $Matches[1]
    }
    if ($line -match '^\s*(\d+)\s+(?:Error\(s\)|个错误)') {
      $errorCount = [int] $Matches[1]
    }
  }
  $records.Add([ordered] @{
    name = $Name
    executable = 'dotnet'
    arguments = $CommandArguments
    project = $Project
    exitCode = $code
    warningCount = $warningCount
    errorCount = $errorCount
    artifact = $Artifact
    log = $log
  })
  $records | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (
    Join-Path $OutputDirectory 'verification-commands.json') -Encoding utf8
  Write-Output "$Name exit=$code log=$log"
  if ($code -ne 0) {
    Get-Content -LiteralPath $log -Tail 30 | Write-Output
    throw "$Name failed with exit code $code."
  }
}

Push-Location $repositoryRoot
try {
  Invoke-RecordedDotnet 'client-properties' @(
    'msbuild', $clientProject, '-p:NetworkTestBuild=true',
    '-getProperty:TargetFramework,OutputType,AssemblyName,DefineConstants,BaseOutputPath,BaseIntermediateOutputPath,OutputPath,IntermediateOutputPath,TargetPath'
  ) $clientProject $clientExecutable
  $clientProperties = Get-Content -LiteralPath (
    Join-Path $OutputDirectory 'client-properties.log') -Raw | ConvertFrom-Json
  if ($clientProperties.Properties.TargetPath -ne $clientExecutable) {
    throw 'The real client output path differs from its documented Build/bin location.'
  }
  $clientBuild = @('build', $clientProject, '-p:NetworkTestBuild=true', '--nologo', '-v:minimal')
  if (Test-Path -LiteralPath (Join-Path $ClientRoot 'Build/obj/Terraria/project.assets.json')) {
    $clientBuild += '--no-restore'
  }
  Invoke-RecordedDotnet 'client-build' $clientBuild $clientProject $clientExecutable
  if (-not (Test-Path -LiteralPath $clientExecutable)) {
    throw 'The real client build did not produce its expected executable.'
  }
  $gatewayArtifact = Join-Path $repositoryRoot (
    'Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll')
  Invoke-RecordedDotnet 'gateway-properties' @(
    'msbuild', $gatewayProject,
    '-getProperty:TargetFramework,BaseOutputPath,BaseIntermediateOutputPath,OutputPath,IntermediateOutputPath,TargetPath'
  ) $gatewayProject $gatewayArtifact
  $gatewayProperties = Get-Content -LiteralPath (
    Join-Path $OutputDirectory 'gateway-properties.log') -Raw | ConvertFrom-Json
  if ($gatewayProperties.Properties.TargetPath -ne $gatewayArtifact) {
    throw 'The verifier output path differs from its documented Build/bin location.'
  }
  $gatewayBuild = @('build', $gatewayProject, '--nologo', '-v:minimal')
  if (Test-Path -LiteralPath (Join-Path $repositoryRoot (
      'Build/obj/NSSLC.Infrastructure.Network.Verification/project.assets.json'))) {
    $gatewayBuild += '--no-restore'
  }
  Invoke-RecordedDotnet 'gateway-build' $gatewayBuild $gatewayProject $gatewayArtifact
  $run = @('run', '--project', $gatewayProject, '--no-build', '--no-restore')
  Invoke-RecordedDotnet 'real-client' ($run + @(
    '--', '--real-client', $clientExecutable, (Join-Path $OutputDirectory 'Results')
  )) $gatewayProject (Join-Path $OutputDirectory 'Results')
  Invoke-RecordedDotnet 'gateway-regression' $run $gatewayProject $gatewayArtifact
  Invoke-RecordedDotnet 'pocket-wire' ($run + @('--', '--pocket-wire')) $gatewayProject $gatewayArtifact
  Get-Content -LiteralPath (Join-Path $OutputDirectory 'real-client.log') | Write-Output
  Get-Content -LiteralPath (Join-Path $OutputDirectory 'gateway-regression.log') -Tail 1 | Write-Output
  Get-Content -LiteralPath (Join-Path $OutputDirectory 'pocket-wire.log') -Tail 1 | Write-Output
} finally {
  Pop-Location
}
