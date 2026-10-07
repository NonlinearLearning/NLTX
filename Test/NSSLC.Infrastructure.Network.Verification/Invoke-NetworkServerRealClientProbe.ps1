param(
  [string] $ClientRoot = 'D:\TRbackup\客户端',
  [string] $WorldFile,
  [string] $OutputDirectory,
  [int] $Port = 7777
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$ClientRoot = [IO.Path]::GetFullPath($ClientRoot)
if (-not $WorldFile) {
  $WorldFile = Join-Path $repositoryRoot (
    'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld')
}
$WorldFile = [IO.Path]::GetFullPath($WorldFile)
if (-not $OutputDirectory) {
  $OutputDirectory = Join-Path $repositoryRoot 'Build/diagnostics/CurrentServerProbe/ProductionHost'
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
$resultsDirectory = Join-Path $OutputDirectory 'Results'
[IO.Directory]::CreateDirectory($resultsDirectory) | Out-Null

$clientProject = Join-Path $ClientRoot 'Terraria.csproj'
$clientExecutable = Join-Path $ClientRoot 'Build/bin/Terraria/Debug/net40/Terraria.NetworkTests.exe'
$serverProject = Join-Path $repositoryRoot 'src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj'
$serverAssembly = Join-Path $repositoryRoot (
  'Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/NSSLC.Tools.NetworkServer.dll')
$records = [Collections.Generic.List[object]]::new()

function Invoke-RecordedDotnet {
  param([string] $Name, [string[]] $Arguments, [string] $Project, [string] $Artifact)
  $log = Join-Path $OutputDirectory ($Name + '.log')
  $lines = & dotnet @Arguments 2>&1
  $code = $LASTEXITCODE
  $lines | Set-Content -LiteralPath $log -Encoding utf8
  $warningCount = 0
  $errorCount = 0
  foreach ($line in $lines) {
    if ($line -match '^\s*(\d+)\s+(?:Warning\(s\)|个警告)') {
      $warningCount = [int]$Matches[1]
    }
    if ($line -match '^\s*(\d+)\s+(?:Error\(s\)|个错误)') {
      $errorCount = [int]$Matches[1]
    }
  }
  $records.Add([ordered]@{
    name = $Name
    executable = 'dotnet'
    arguments = $Arguments
    project = $Project
    exitCode = $code
    warningCount = $warningCount
    errorCount = $errorCount
    artifact = $Artifact
    log = $log
  })
  Write-Output "$Name exit=$code warnings=$warningCount errors=$errorCount"
  if ($code -ne 0) {
    Get-Content -LiteralPath $log -Tail 30 | Write-Output
    throw "$Name failed with exit code $code."
  }
}

function New-HiddenProcess {
  param([string] $Executable, [string[]] $Arguments, [string] $WorkingDirectory)
  $info = [Diagnostics.ProcessStartInfo]::new($Executable)
  $info.UseShellExecute = $false
  $info.CreateNoWindow = $true
  $info.RedirectStandardOutput = $true
  $info.RedirectStandardError = $true
  $info.WorkingDirectory = $WorkingDirectory
  foreach ($argument in $Arguments) {
    [void]$info.ArgumentList.Add($argument)
  }
  $process = [Diagnostics.Process]::new()
  $process.StartInfo = $info
  [void]$process.Start()
  return $process
}

function Stop-IfRunning {
  param([Diagnostics.Process] $Process)
  if ($null -ne $Process) {
    $Process.Refresh()
    if (-not $Process.HasExited) {
      $Process.Kill($true)
      [void]$Process.WaitForExit()
    }
  }
}

function Invoke-ClientScenario {
  param([string] $Scenario)
  $server = $null
  $client = $null
  $name = $Scenario
  $resultPath = Join-Path $resultsDirectory ($name + '.json')
  $factsPath = Join-Path $OutputDirectory ($name + '.test-facts.json')
  $serverReportPath = Join-Path $OutputDirectory ($name + '.server-report.json')
  try {
    if (@(Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue).Count -gt 0) {
      throw "Port $Port already has a listener."
    }
    $server = New-HiddenProcess 'dotnet' @(
      $serverAssembly, '--world', $WorldFile, '--listen', '127.0.0.1', '--port', [string]$Port,
      '--exit-after-first-client', '--test-facts', $factsPath, '--report', $serverReportPath) $repositoryRoot
    $serverOutputTask = $server.StandardOutput.ReadToEndAsync()
    $serverErrorTask = $server.StandardError.ReadToEndAsync()
    $ready = $false
    for ($attempt = 0; $attempt -lt 400; $attempt++) {
      $server.Refresh()
      if ($server.HasExited) { break }
      $listeners = @(Get-NetTCPConnection -State Listen -LocalPort $Port `
        -ErrorAction SilentlyContinue | Where-Object { $_.OwningProcess -eq $server.Id })
      if ($listeners.Count -gt 0) {
        $ready = $true
        break
      }
      Start-Sleep -Milliseconds 50
    }
    if (-not $ready) {
      $serverOutput = $serverOutputTask.GetAwaiter().GetResult()
      $serverError = $serverErrorTask.GetAwaiter().GetResult()
      [IO.File]::WriteAllText((Join-Path $OutputDirectory ($name + '.server.stdout.log')),
        $serverOutput)
      [IO.File]::WriteAllText((Join-Path $OutputDirectory ($name + '.server.stderr.log')),
        $serverError)
      throw "The NLTX network host did not listen. $serverError $serverOutput"
    }

    $clientArguments = @(
      '-networktest', $Scenario, '-testresult', $resultPath, '-testtimeoutms', '30000',
      '-join', '127.0.0.1', '-port', [string]$Port)
    if ($Scenario -eq 'world-player') {
      $clientArguments += @('-tile-frame-important', $factsPath)
    }
    $client = New-HiddenProcess $clientExecutable $clientArguments $ClientRoot
    $clientOutputTask = $client.StandardOutput.ReadToEndAsync()
    $clientErrorTask = $client.StandardError.ReadToEndAsync()
    $windowSeen = $false
    $clientDeadline = [DateTime]::UtcNow.AddSeconds(45)
    while (-not $client.HasExited -and [DateTime]::UtcNow -lt $clientDeadline) {
      $client.Refresh()
      $windowSeen = $windowSeen -or ($client.MainWindowHandle -ne [IntPtr]::Zero)
      Start-Sleep -Milliseconds 25
    }
    if (-not $client.HasExited) {
      Stop-IfRunning $client
    }
    $serverDeadline = [DateTime]::UtcNow.AddSeconds(8)
    while (-not $server.HasExited -and [DateTime]::UtcNow -lt $serverDeadline) {
      Start-Sleep -Milliseconds 50
      $server.Refresh()
    }
    if (-not $server.HasExited) {
      Stop-IfRunning $server
    }

    $clientStdout = $clientOutputTask.GetAwaiter().GetResult()
    $clientStderr = $clientErrorTask.GetAwaiter().GetResult()
    $serverStdout = $serverOutputTask.GetAwaiter().GetResult()
    $serverStderr = $serverErrorTask.GetAwaiter().GetResult()
    $worldLine = [regex]::Match($serverStdout,
      '(?m)^World: (.+) \((\d+)x(\d+)\), id=(-?\d+), packet 6 enabled;')
    if (-not $worldLine.Success) {
      throw 'The server did not report the loaded world metadata.'
    }
    $serverOutputWorldName = $worldLine.Groups[1].Value
    $serverOutputWorldWidth = [int]$worldLine.Groups[2].Value
    $serverOutputWorldHeight = [int]$worldLine.Groups[3].Value
    $serverOutputWorldId = [int]$worldLine.Groups[4].Value
    $clientStdoutPath = Join-Path $OutputDirectory ($name + '.client.stdout.log')
    $clientStderrPath = Join-Path $OutputDirectory ($name + '.client.stderr.log')
    $serverStdoutPath = Join-Path $OutputDirectory ($name + '.server.stdout.log')
    $serverStderrPath = Join-Path $OutputDirectory ($name + '.server.stderr.log')
    [IO.File]::WriteAllText($clientStdoutPath, $clientStdout)
    [IO.File]::WriteAllText($clientStderrPath, $clientStderr)
    [IO.File]::WriteAllText($serverStdoutPath, $serverStdout)
    [IO.File]::WriteAllText($serverStderrPath, $serverStderr)
    $clientResult = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    $serverReport = $null
    if (Test-Path -LiteralPath $serverReportPath -PathType Leaf) {
      $serverReport = Get-Content -LiteralPath $serverReportPath -Raw | ConvertFrom-Json
    }
    $rejectedMessageId = 0
    if ($clientResult.PSObject.Properties.Name -contains 'rejectedMessageId') {
      $rejectedMessageId = $clientResult.rejectedMessageId
    }
    $worldSectionsReceived = 0
    $controlFramesSent = 0
    $movementDistancePixels = 0
    $movementSectionTransitions = 0
    $requestedSectionsByClient = 0
    $tileBreakRequestSent = $false
    $tileUpdateReceived = $false
    $tileSectionRefreshReceived = $false
    $destructionTargetX = 0
    $destructionTargetY = 0
    $destructionTargetType = 0
    $sawRightControl = $false
    $sawJumpControl = $false
    if ($clientResult.PSObject.Properties.Name -contains 'worldSectionsReceived') {
      $worldSectionsReceived = $clientResult.worldSectionsReceived
    }
    if ($clientResult.PSObject.Properties.Name -contains 'controlFramesSent') {
      $controlFramesSent = $clientResult.controlFramesSent
      $sawRightControl = $clientResult.sawRightControl
      $sawJumpControl = $clientResult.sawJumpControl
    }
    if ($clientResult.PSObject.Properties.Name -contains 'movementDistancePixels') {
      $movementDistancePixels = $clientResult.movementDistancePixels
      $movementSectionTransitions = $clientResult.movementSectionTransitions
      $requestedSectionsByClient = $clientResult.requestedSections
      $tileBreakRequestSent = $clientResult.tileBreakRequestSent
      $tileUpdateReceived = $clientResult.tileUpdateReceived
      $tileSectionRefreshReceived = $clientResult.tileSectionRefreshReceived
      $destructionTargetX = $clientResult.destructionTargetX
      $destructionTargetY = $clientResult.destructionTargetY
      $destructionTargetType = $clientResult.destructionTargetType
    }
    $caseResult = [ordered]@{
      scenario = $Scenario
      endpoint = "127.0.0.1:$Port"
      clientExitCode = $client.ExitCode
      serverExitCode = $server.ExitCode
      clientSuccess = $clientResult.success
      playerSlot = $clientResult.playerSlot
      worldName = $clientResult.worldName
      worldId = $clientResult.worldId
      worldWidth = $clientResult.worldWidth
      worldHeight = $clientResult.worldHeight
      serverWorldName = $serverOutputWorldName
      serverWorldId = $serverOutputWorldId
      serverWorldWidth = $serverOutputWorldWidth
      serverWorldHeight = $serverOutputWorldHeight
      rejectedMessageId = $rejectedMessageId
      worldSectionsReceived = $worldSectionsReceived
      controlFramesSent = $controlFramesSent
      movementDistancePixels = $movementDistancePixels
      movementSectionTransitions = $movementSectionTransitions
      requestedSectionsByClient = $requestedSectionsByClient
      tileBreakRequestSent = $tileBreakRequestSent
      tileUpdateReceived = $tileUpdateReceived
      tileSectionRefreshReceived = $tileSectionRefreshReceived
      destructionTargetX = $destructionTargetX
      destructionTargetY = $destructionTargetY
      destructionTargetType = $destructionTargetType
      sawRightControl = $sawRightControl
      sawJumpControl = $sawJumpControl
      serverSectionCount = if ($null -ne $serverReport) { @($serverReport.sections).Count } else { 0 }
      serverPlayerControls = if ($null -ne $serverReport) { $serverReport.playerControls } else { @() }
      serverReport = $serverReportPath
      createNoWindow = $client.StartInfo.CreateNoWindow
      windowSeen = $windowSeen
      clientResult = $resultPath
      serverStdout = $serverStdoutPath
      serverStderr = $serverStderrPath
    }
    $caseResult | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (
      Join-Path $OutputDirectory ($name + '.process.json')) -Encoding utf8

    if ($client.ExitCode -ne 0 -or $server.ExitCode -ne 0 -or -not $clientResult.success -or
        $windowSeen -or $clientResult.worldName -ne $serverOutputWorldName -or
        $clientResult.worldId -ne $serverOutputWorldId -or
        $clientResult.worldWidth -ne $serverOutputWorldWidth -or
        $clientResult.worldHeight -ne $serverOutputWorldHeight) {
      throw "$Scenario did not complete a clean packet-7 exchange."
    }
    if ($Scenario -eq 'world-player') {
      if (-not (Test-Path -LiteralPath $serverReportPath -PathType Leaf)) {
        throw 'The production host did not write its world-player verification report.'
      }
      if ($clientResult.worldSectionsReceived -lt 18 -or
          $clientResult.controlFramesSent -lt 200 -or
          -not $clientResult.sawRightControl -or -not $clientResult.sawJumpControl) {
        throw 'The client did not parse route sections and send long-distance movement controls.'
      }
      $playerControl = @($serverReport.playerControls | Where-Object { $_.playerSlot -eq 0 })
      $spawn = @($serverReport.playerSpawns | Where-Object { $_.playerSlot -eq 0 })
      $requestedSections = @($serverReport.sections | Where-Object { $_.reason -eq 'request' })
      $tileBreaks = @($serverReport.tileBreaks | Where-Object { $_.playerSlot -eq 0 })
      $samples = if ($playerControl.Count -eq 1) { @($playerControl[0].samples) } else { @() }
      $uniqueRequestedSections = @($requestedSections |
        ForEach-Object { "$($_.sectionX),$($_.sectionY)" } | Select-Object -Unique)
      if ($playerControl.Count -ne 1 -or $playerControl[0].packetCount -lt 200 -or
          -not $playerControl[0].sawRight -or -not $playerControl[0].sawJump -or
          $playerControl[0].traveledDistancePixels -lt 9600 -or
          [Math]::Abs($playerControl[0].lastPosition.x - $playerControl[0].firstPosition.x) -lt 9600 -or
          $samples.Count -ne 128 -or
          (($samples[-1].controlFlags -band 0x18) -ne 0) -or
          $null -ne $samples[-1].velocity -or
          $spawn.Count -ne 1 -or $requestedSections.Count -lt 4 -or
          $uniqueRequestedSections.Count -lt 3 -or
          $movementDistancePixels -ne 9600 -or
          $movementSectionTransitions -lt 3 -or
          $requestedSectionsByClient -ne $movementSectionTransitions -or
          -not $clientResult.tileBreakRequestSent -or
          -not $clientResult.tileUpdateReceived -or
          -not $clientResult.tileSectionRefreshReceived -or
          $tileBreaks.Count -ne 1 -or
          $tileBreaks[0].x -ne $clientResult.destructionTargetX -or
          $tileBreaks[0].y -ne $clientResult.destructionTargetY -or
          $tileBreaks[0].originalType -ne $clientResult.destructionTargetType -or
          $tileBreaks[0].activeAfter) {
        throw 'The host did not confirm a 600-tile route, three section requests, and a synchronized tile break.'
      }
    }
    $caseResult
  } finally {
    Stop-IfRunning $client
    Stop-IfRunning $server
  }
}

if (-not (Test-Path -LiteralPath $WorldFile -PathType Leaf)) {
  throw "World file not found: $WorldFile"
}
Push-Location $repositoryRoot
try {
  Invoke-RecordedDotnet 'client-properties' @(
    'msbuild', $clientProject, '-p:NetworkTestBuild=true',
    '-getProperty:TargetFramework,OutputType,AssemblyName,BaseOutputPath,BaseIntermediateOutputPath,OutputPath,IntermediateOutputPath,TargetPath'
  ) $clientProject $clientExecutable
  $clientProperties = Get-Content -LiteralPath (
    Join-Path $OutputDirectory 'client-properties.log') -Raw | ConvertFrom-Json
  if ($clientProperties.Properties.TargetPath -ne $clientExecutable) {
    throw 'The real client output differs from its documented Build/bin path.'
  }
  $clientBuild = @('build', $clientProject, '-p:NetworkTestBuild=true', '--nologo', '-v:minimal')
  if (Test-Path -LiteralPath (Join-Path $ClientRoot 'Build/obj/Terraria/project.assets.json')) {
    $clientBuild += '--no-restore'
  }
  Invoke-RecordedDotnet 'client-build' $clientBuild $clientProject $clientExecutable

  $serverBuild = @('build', $serverProject, '--nologo', '-v:minimal')
  if (Test-Path -LiteralPath (Join-Path $repositoryRoot (
      'Build/obj/NSSLC.Tools.NetworkServer/project.assets.json'))) {
    $serverBuild += '--no-restore'
  }
  Invoke-RecordedDotnet 'server-build' $serverBuild $serverProject $serverAssembly

  $propertyLog = & dotnet msbuild $serverProject `
    '-getProperty:TargetFramework,BaseOutputPath,BaseIntermediateOutputPath,OutputPath,IntermediateOutputPath,TargetPath' 2>&1
  $propertyCode = $LASTEXITCODE
  $propertyPath = Join-Path $OutputDirectory 'server-properties.log'
  $propertyLog | Set-Content -LiteralPath $propertyPath -Encoding utf8
  if ($propertyCode -ne 0) { throw 'Could not resolve the server output properties.' }
  $properties = Get-Content -LiteralPath $propertyPath -Raw | ConvertFrom-Json
  if ($properties.Properties.TargetPath -ne $serverAssembly) {
    throw 'The network server output differs from its documented Build/bin path.'
  }
  $records.Add([ordered]@{
    name = 'server-properties'
    executable = 'dotnet'
    arguments = @('msbuild', $serverProject,
      '-getProperty:TargetFramework,BaseOutputPath,BaseIntermediateOutputPath,OutputPath,IntermediateOutputPath,TargetPath')
    project = $serverProject
    exitCode = $propertyCode
    warningCount = 0
    errorCount = 0
    artifact = $serverAssembly
    log = $propertyPath
  })

  $results = @(
    Invoke-ClientScenario 'world-request'
    Invoke-ClientScenario 'world-player'
  )
  $results | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (
    Join-Path $OutputDirectory 'production-server-report.json') -Encoding utf8
  $records | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (
    Join-Path $OutputDirectory 'verification-commands.json') -Encoding utf8
  $results | ConvertTo-Json -Depth 6
} finally {
  Pop-Location
}
