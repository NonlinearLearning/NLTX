param(
  [string] $ClientRoot = 'D:\TRbackup\客户端',
  [string] $OutputDirectory = 'Build\diagnostics\RealClientNetwork\MinimalImplementation'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ClientRoot = [IO.Path]::GetFullPath($ClientRoot)
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$ClientExecutable = Join-Path $ClientRoot 'Build\bin\Terraria\Debug\net40\Terraria.NetworkTests.exe'
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null

if (-not (Test-Path -LiteralPath $ClientExecutable -PathType Leaf)) {
  throw "Real client executable was not found: $ClientExecutable"
}

function New-Frame {
  param([byte] $MessageId, [byte[]] $Payload = @())

  $length = 3 + $Payload.Length
  if ($length -gt [UInt16]::MaxValue) {
    throw "Frame is too large: $length bytes."
  }

  $frame = [byte[]]::new($length)
  [BitConverter]::GetBytes([UInt16]$length).CopyTo($frame, 0)
  $frame[2] = $MessageId
  if ($Payload.Length -gt 0) {
    $Payload.CopyTo($frame, 3)
  }
  return $frame
}

function New-Bytes {
  param([object[]] $Parts)

  $bytes = [Collections.Generic.List[byte]]::new()
  foreach ($part in $Parts) {
    if ($part -is [byte[]]) {
      $bytes.AddRange($part)
    } elseif ($part -is [System.Array]) {
      foreach ($value in $part) { $bytes.Add([byte]$value) }
    } else {
      $bytes.Add([byte]$part)
    }
  }
  return $bytes.ToArray()
}

function New-HealthPayload {
  param([int] $Life)

  return New-Bytes @(
    [byte]1,
    [BitConverter]::GetBytes([Int16]$Life),
    [BitConverter]::GetBytes([Int16]100))
}

function New-NpcNamePayload {
  $stream = [IO.MemoryStream]::new()
  $writer = [IO.BinaryWriter]::new($stream)
  $writer.Write([Int16]0)
  $writer.Write('NetworkGuide')
  $writer.Write([Int32]4)
  $writer.Flush()
  return $stream.ToArray()
}

function New-NetworkTextLiteralEmptyPayload {
  # NetworkText literal mode followed by BinaryWriter's empty UTF-8 string.
  return New-Bytes @([byte]0, [byte]0)
}

function New-WorldDataPayload {
  $stream = [IO.MemoryStream]::new()
  $writer = [IO.BinaryWriter]::new($stream)
  $writer.Write([Int32]0)
  $writer.Write([byte]0)
  $writer.Write([byte]0)
  $writer.Write([Int16]4200)
  $writer.Write([Int16]1200)
  $writer.Write([Int16]2100)
  $writer.Write([Int16]300)
  $writer.Write([Int16]250)
  $writer.Write([Int16]400)
  $writer.Write([Int32]1)
  $writer.Write('MinimalImplementation raw gateway')
  $writer.Write([byte]0)
  $writer.Write([byte[]]::new(16))
  $writer.Write([UInt64]0)
  $writer.Write([byte]0)
  $writer.Write([byte[]]::new(13))
  $writer.Write([Single]0)
  $writer.Write([byte]0)
  $writer.Write([Int32]0)
  $writer.Write([Int32]2100)
  $writer.Write([Int32]4199)
  $writer.Write([byte[]]::new(4))
  $writer.Write([Int32]0)
  $writer.Write([Int32]400)
  $writer.Write([Int32]800)
  $writer.Write([byte[]]::new(4))
  $writer.Write([byte[]]::new(13))
  $writer.Write([Single]0)
  $writer.Write([byte[]]::new(11))
  $writer.Write([byte]0)
  $writer.Write([byte]0)
  for ($index = 0; $index -lt 7; $index++) {
    $writer.Write([Int16]0)
  }
  $writer.Write([sbyte]0)
  $writer.Write([UInt64]0)
  $writer.Write([Single]0)
  $writer.Write([byte]0)
  $writer.Write([byte[]]::new(3))
  $writer.Flush()
  return $stream.ToArray()
}

function Read-Exactly {
  param([Net.Sockets.NetworkStream] $Stream, [byte[]] $Buffer, [int] $Offset, [int] $Count)

  while ($Count -gt 0) {
    $read = $Stream.Read($Buffer, $Offset, $Count)
    if ($read -le 0) { throw 'The client closed before a complete frame was received.' }
    $Offset += $read
    $Count -= $read
  }
}

function Read-Frame {
  param([Net.Sockets.NetworkStream] $Stream)

  $header = [byte[]]::new(2)
  Read-Exactly $Stream $header 0 2
  $length = [BitConverter]::ToUInt16($header, 0)
  if ($length -lt 3) { throw "Invalid client frame length: $length" }
  $frame = [byte[]]::new($length)
  $header.CopyTo($frame, 0)
  Read-Exactly $Stream $frame 2 ($length - 2)
  return $frame
}

function Write-Frame {
  param([Net.Sockets.NetworkStream] $Stream, [byte[]] $Frame)
  $Stream.Write($Frame, 0, $Frame.Length)
  $Stream.Flush()
}

function Start-Client {
  param([string] $Scenario, [string] $ResultPath, [int] $Port)

  $info = [Diagnostics.ProcessStartInfo]::new($ClientExecutable)
  $info.UseShellExecute = $false
  $info.CreateNoWindow = $true
  $info.RedirectStandardOutput = $true
  $info.RedirectStandardError = $true
  $info.WorkingDirectory = $ClientRoot
  foreach ($argument in @(
      '-networktest', $Scenario,
      '-testresult', $ResultPath,
      '-testtimeoutms', '10000',
      '-join', '127.0.0.1',
      '-port', [string]$Port)) {
    [void]$info.ArgumentList.Add($argument)
  }

  $process = [Diagnostics.Process]::new()
  $process.StartInfo = $info
  [void]$process.Start()
  return $process
}

function Invoke-Scenario {
  param([string] $Scenario)

  $resultPath = Join-Path $OutputDirectory ($Scenario + '.json')
  $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
  $listener.Start()
  $port = ([Net.IPEndPoint]$listener.LocalEndpoint).Port
  $client = $null
  $connection = $null
  $received = [Collections.Generic.List[int]]::new()
  $sent = [Collections.Generic.List[int]]::new()
  try {
    $client = Start-Client $Scenario $resultPath $port
    $connection = $listener.AcceptTcpClient()
    $stream = $connection.GetStream()
    $stream.ReadTimeout = 10000
    $stream.WriteTimeout = 10000

    $read = {
      $frame = Read-Frame $stream
      $received.Add([int]$frame[2])
      return $frame
    }
    $write = {
      param([byte[]] $frame)
      $sent.Add([int]$frame[2])
      Write-Frame $stream $frame
    }

    $hello = & $read
    if ($hello[2] -ne 1) { throw "Expected client Hello packet 1, got $($hello[2])." }

    if ($Scenario -eq 'password-rejected') {
      & $write (New-Frame 37)
      $password = & $read
      if ($password[2] -ne 38) { throw "Expected password response packet 38, got $($password[2])." }
      & $write (New-Frame 2 (New-NetworkTextLiteralEmptyPayload))
    } else {
      & $write (New-Frame 3 (New-Bytes @([byte]0, [byte]0)))

      if ($Scenario -eq 'hello') {
        # No post-handshake traffic is expected in this scenario.
      } elseif ($Scenario -eq 'world-request') {
        $request = & $read
        if ($request[2] -ne 6) { throw "Expected world request packet 6, got $($request[2])." }
        & $write (New-Frame 7 (New-WorldDataPayload))
      } elseif ($Scenario -eq 'admission') {
        $admissionFrame = & $read
        if ($admissionFrame[2] -notin @(4, 5, 16, 42, 50, 68, 147)) {
          throw "Unexpected admission packet $($admissionFrame[2])."
        }
        & $write (New-Frame 16 (New-HealthPayload 100))
        # The admission scenario sends the remaining inventory/loadout frames after the
        # first packet. Drain the socket so the report records every message id emitted by
        # the real client, even though this fixture only needs one acknowledgement.
        $stream.ReadTimeout = 1000
        while ($true) {
          try {
            $ignored = & $read
          } catch {
            break
          }
        }
      } else {
        $first = & $read
        if ($Scenario -eq 'default-deny') {
          if ($first[2] -ne 30) { throw "Expected hostile toggle packet 30, got $($first[2])." }
          & $write (New-Frame 2 (New-NetworkTextLiteralEmptyPayload))
        } else {
          if ($first[2] -ne 6) { throw "Expected world request packet 6, got $($first[2])." }
          & $write (New-Frame 16 (New-HealthPayload 10))
          $second = & $read
          if ($second[2] -ne 8) { throw "Expected section request packet 8, got $($second[2])." }
          & $write (New-Frame 16 (New-HealthPayload 11))
          $third = & $read
          if ($third[2] -ne 12) { throw "Expected spawn packet 12, got $($third[2])." }
          & $write (New-Frame 16 (New-HealthPayload 12))

          if ($Scenario -eq 'ping') {
            $ping = & $read
            if ($ping[2] -ne 82) { throw "Expected Ping module packet 82, got $($ping[2])." }
            & $write (New-Frame 82 $ping[3..($ping.Length - 1)])
          } else {
            $controls = & $read
            if ($controls[2] -ne 13) { throw "Expected controls packet 13, got $($controls[2])." }
            $controls[3] = [byte]1
            & $write (New-Frame 13 $controls[3..($controls.Length - 1)])
            $life = & $read
            if ($life[2] -ne 16) { throw "Expected life packet 16, got $($life[2])." }
            & $write (New-Frame 16 (New-HealthPayload 75))
            $npc = & $read
            if ($npc[2] -ne 56) { throw "Expected NPC name packet 56, got $($npc[2])." }
            & $write (New-Frame 56 (New-NpcNamePayload))
            $projectile = & $read
            if ($projectile[2] -ne 27) { throw "Expected projectile packet 27, got $($projectile[2])." }
            & $write (New-Frame 16 (New-HealthPayload 40))
            $kill = & $read
            if ($kill[2] -ne 29) { throw "Expected projectile kill packet 29, got $($kill[2])." }
            & $write (New-Frame 16 (New-HealthPayload 41))
          }
        }
      }
    }

    if (-not $client.WaitForExit(15000)) {
      $client.Kill($true)
      throw "Client scenario '$Scenario' exceeded its 15 second process deadline."
    }
    $stdout = $client.StandardOutput.ReadToEnd()
    $stderr = $client.StandardError.ReadToEnd()
    $clientResult = $null
    if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
      $clientResult = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    }

    return [ordered]@{
      scenario = $Scenario
      clientExitCode = $client.ExitCode
      clientSuccess = if ($null -ne $clientResult) { [bool]$clientResult.success } else { $false }
      clientResult = $resultPath
      observedClientMessageIds = @($received)
      observedServerMessageIds = @($sent)
      stdout = $stdout.Trim()
      stderr = $stderr.Trim()
    }
  } finally {
    if ($null -ne $connection) { $connection.Dispose() }
    $listener.Stop()
    if ($null -ne $client) {
      $client.Refresh()
      if (-not $client.HasExited) { $client.Kill($true); [void]$client.WaitForExit() }
      $client.Dispose()
    }
  }
}

$results = [Collections.Generic.List[object]]::new()
$scenarios = @('hello', 'gateway', 'ping', 'default-deny', 'password-rejected', 'admission', 'world-request')
foreach ($scenario in $scenarios) {
  try {
    $results.Add((Invoke-Scenario $scenario))
  } catch {
    $results.Add([ordered]@{
      scenario = $scenario
      clientExitCode = $null
      clientSuccess = $false
      observedClientMessageIds = @()
      observedServerMessageIds = @()
      error = $_.Exception.ToString()
    })
  }
}

$coveredIds = [Collections.Generic.HashSet[int]]::new()
foreach ($result in $results) {
  foreach ($messageId in @($result.observedClientMessageIds)) {
    [void]$coveredIds.Add([int]$messageId)
  }
  foreach ($messageId in @($result.observedServerMessageIds)) {
    [void]$coveredIds.Add([int]$messageId)
  }
}

$summary = [ordered]@{
  clientExecutable = $ClientExecutable
  scenarios = $results
  coveredMessageIds = @($coveredIds | Sort-Object)
  limitation = 'This probe covers every scenario implemented by the retained headless client except world-player; it is not a claim that Terraria message IDs 0-161 all executed.'
}
$summaryPath = Join-Path $OutputDirectory 'real-client-packet-probe.json'
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
$summary | ConvertTo-Json -Depth 8
