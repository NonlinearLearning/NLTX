[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$runner = (Resolve-Path (Join-Path $PSScriptRoot 'Invoke-Version4AuthoritativePartitionSession.ps1')).Path
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$table = Join-Path $repoRoot 'docs\system-decomposition\authoritative\2026-09-18-system-decomposition-authoritative-20-partition-tasks.md'
$tasksRoot = Join-Path $repoRoot '.agents\skills\version4-partition-session-runner\sessions\version4-authoritative-partition-session\tasks'
$taskSet = 'lease-contract-' + [guid]::NewGuid().ToString('N').Substring(0, 10)
$taskDirectory = Join-Path $tasksRoot $taskSet
$originalCodexSessionId = $env:CODEX_SESSION_ID

function Invoke-Runner {
    param(
        [Parameter(Mandatory)][string[]] $Arguments,
        [ValidateRange(1, 300)][int] $TimeoutSeconds = 30
    )

    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = (Get-Command pwsh).Source
    $psi.WorkingDirectory = $repoRoot
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.ArgumentList.Add('-NoProfile')
    $psi.ArgumentList.Add('-File')
    $psi.ArgumentList.Add($runner)
    foreach ($argument in $Arguments) { $psi.ArgumentList.Add([string]$argument) }
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $psi
    $null = $process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $operation = ($Arguments -join ' ')
        try { $process.Kill($true) } catch { }
        $process.WaitForExit()
        $timedOutOutput = $stdoutTask.GetAwaiter().GetResult()
        $timedOutError = $stderrTask.GetAwaiter().GetResult()
        throw "Runner timed out after $TimeoutSeconds seconds: $operation`n$timedOutOutput`n$timedOutError"
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $exitCode = $process.ExitCode
    $process.Dispose()
    $text = $stdout.Trim()
    if ([string]::IsNullOrWhiteSpace($text)) { $text = $stderr.Trim() }
    $json = $null
    try { $json = $text | ConvertFrom-Json -DateKind String } catch { }
    [pscustomobject]@{ exitCode = $exitCode; text = $text; json = $json }
}

function Assert-True {
    param([Parameter(Mandatory)][bool] $Condition, [Parameter(Mandatory)][string] $Message)
    if (-not $Condition) { throw "ASSERT FAILED: $Message" }
}

function Get-Record {
    param([Parameter(Mandatory)][string] $PartitionId)

    $statePath = Join-Path $taskDirectory 'task-state.json'
    $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json -DateKind String
    return @($state.partitions | Where-Object id -eq $PartitionId) | Select-Object -First 1
}

function Assert-Status {
    param(
        [Parameter(Mandatory)] $Result,
        [Parameter(Mandatory)][int] $ExitCode,
        [Parameter(Mandatory)][string] $Status,
        [Parameter(Mandatory)][string] $Message
    )

    Assert-True ($Result.exitCode -eq $ExitCode) "$Message exit code"
    Assert-True ($null -ne $Result.json -and $Result.json.status -eq $Status) "$Message status"
}

try {
    Assert-True (Test-Path -LiteralPath $table -PathType Leaf) 'authoritative task table exists'
    $result = Invoke-Runner @('-Action', 'Initialize', '-TaskSetName', $taskSet, '-TaskTablePath', $table)
    Assert-Status -Result $result -ExitCode 0 -Status 'initialized' -Message 'Initialize'

    # Normal claim, heartbeat, duplicate protection, and parallel partition claim.
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P01')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'Claim P01'
    $p01 = $result.json
    $p01Record = Get-Record -PartitionId 'P01'
    Assert-True ($p01Record.leaseState -eq 'active') 'P01 lease is active'
    Assert-True ($null -ne (Get-Process -Id ([int]$p01Record.leaseHostProcessId) -ErrorAction SilentlyContinue)) 'P01 lease host is alive'

    $result = Invoke-Runner @('-Action', 'Heartbeat', '-TaskSetName', $taskSet, '-PartitionId', 'P01', '-SessionId', $p01.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'heartbeat' -Message 'Heartbeat P01'
    $result = Invoke-Runner @('-Action', 'Heartbeat', '-TaskSetName', $taskSet, '-PartitionId', 'P01', '-SessionId', 'wrong-session')
    Assert-Status -Result $result -ExitCode 4 -Status 'conflict' -Message 'wrong session heartbeat'
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P01')
    Assert-True ($result.exitCode -eq 4) 'duplicate claim is rejected'

    # A Codex session owns at most one manual partition. Use a second session
    # identity to exercise an independent concurrent claim.
    $parallelCodexSessionId = 'parallel-' + [guid]::NewGuid().ToString('N')
    $env:CODEX_SESSION_ID = $parallelCodexSessionId
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P02')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'parallel P02 claim'
    $p02 = $result.json
    $p02CodexSessionId = $parallelCodexSessionId
    $env:CODEX_SESSION_ID = $originalCodexSessionId

    # Normal settlement paths.
    $result = Invoke-Runner @('-Action', 'Complete', '-TaskSetName', $taskSet, '-PartitionId', 'P01', '-SessionId', $p01.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'completed' -Message 'Complete P01'
    Assert-True ($null -eq (Get-Process -Id ([int]$p01Record.leaseHostProcessId) -ErrorAction SilentlyContinue)) 'Complete stops P01 host'
    $env:CODEX_SESSION_ID = $p02CodexSessionId
    $result = Invoke-Runner @('-Action', 'Fail', '-TaskSetName', $taskSet, '-PartitionId', 'P02', '-SessionId', $p02.sessionId, '-FailureMessage', 'fixture failure')
    Assert-Status -Result $result -ExitCode 5 -Status 'failed' -Message 'Fail P02'
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P02', '-Retry')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'Retry failed P02'
    $result = Invoke-Runner @('-Action', 'Abandon', '-TaskSetName', $taskSet, '-PartitionId', 'P02', '-SessionId', $result.json.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'Abandon P02'
    $env:CODEX_SESSION_ID = $originalCodexSessionId

    # Explicit normal session close, handoff, cleanup, and managed worker compatibility.
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P03')
    $p03 = $result.json
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P03', '-SessionId', $p03.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'CloseSession P03'

    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P04')
    $p04 = $result.json
    $result = Invoke-Runner @('-Action', 'Handoff', '-TaskSetName', $taskSet, '-PartitionId', 'P04', '-SessionId', $p04.sessionId, '-HandoffId', 'lease-test-handoff')
    Assert-Status -Result $result -ExitCode 0 -Status 'handed-off' -Message 'Handoff P04'
    $p04New = $result.json
    $result = Invoke-Runner @('-Action', 'Complete', '-TaskSetName', $taskSet, '-PartitionId', 'P04', '-SessionId', $p04.sessionId)
    Assert-True ($result.exitCode -eq 4) 'old handoff owner cannot settle'
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P04', '-SessionId', $p04New.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'new handoff owner closes'

    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P05')
    $result = Invoke-Runner @('-Action', 'Cleanup', '-TaskSetName', $taskSet, '-PartitionId', 'P05')
    Assert-Status -Result $result -ExitCode 0 -Status 'cleaned' -Message 'Cleanup P05'
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P05')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'reclaim cleaned P05'
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P05', '-SessionId', $result.json.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'close reclaimed P05'

    $childArgs = ConvertTo-Json -InputObject ([string[]]@('-NoProfile', '-Command', 'exit 0')) -Compress
    $result = Invoke-Runner @('-Action', 'Run', '-TaskSetName', $taskSet, '-PartitionId', 'P06', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', $childArgs)
    Assert-Status -Result $result -ExitCode 0 -Status 'completed' -Message 'managed Run P06'
    Assert-True ([string]::IsNullOrWhiteSpace([string](Get-Record -PartitionId 'P06').leaseName)) 'managed run does not use manual lease'

    # Abnormal lease host termination and TTL expiry.
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P07')
    $p07Record = Get-Record -PartitionId 'P07'
    Stop-Process -Id ([int]$p07Record.leaseHostProcessId) -Force
    Start-Sleep -Milliseconds 300
    $result = Invoke-Runner @('-Action', 'List', '-TaskSetName', $taskSet)
    Assert-True ($result.exitCode -eq 0 -and (Get-Record -PartitionId 'P07').status -eq 'abandoned') 'killed lease host is reconciled'
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P07', '-Retry')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'retry after killed host'
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P07', '-SessionId', $result.json.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'close retry P07'

    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P08')
    $leasePath = Join-Path $taskDirectory 'session-lease-P08.json'
    $lease = Get-Content -Raw -LiteralPath $leasePath | ConvertFrom-Json -DateKind String
    $lease.leaseExpiresAtUtc = '2000-01-01T00:00:00.0000000Z'
    $lease | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $leasePath -Encoding utf8
    $result = Invoke-Runner @('-Action', 'List', '-TaskSetName', $taskSet)
    Assert-True ($result.exitCode -eq 0 -and (Get-Record -PartitionId 'P08').status -eq 'abandoned') 'expired lease is reconciled'
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P08', '-Retry')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'retry expired lease'
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P08', '-SessionId', $result.json.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'close retry P08'

    # Codex session identity and Mutex collision.
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P09')
    $p09 = $result.json
    $env:CODEX_SESSION_ID = 'different-codex-session'
    $result = Invoke-Runner @('-Action', 'Heartbeat', '-TaskSetName', $taskSet, '-PartitionId', 'P09', '-SessionId', $p09.sessionId)
    Assert-True ($result.exitCode -eq 4) 'different Codex session is rejected'
    $env:CODEX_SESSION_ID = $originalCodexSessionId
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P09', '-SessionId', $p09.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'close P09'

    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P10')
    $p10Record = Get-Record -PartitionId 'P10'
    $collisionLease = Join-Path $taskDirectory 'collision-lease.json'
    $collisionStop = Join-Path $taskDirectory 'collision-stop'
    $collision = Start-Process -FilePath (Get-Command pwsh).Source -ArgumentList @('-NoProfile', '-File', (Join-Path $PSScriptRoot 'Version4SessionLeaseHost.ps1'), '-LeaseName', $p10Record.leaseName, '-LeaseFilePath', $collisionLease, '-StopFilePath', $collisionStop, '-SessionToken', 'collision', '-HeartbeatSeconds', '2') -PassThru -WindowStyle Hidden
    $collision.WaitForExit()
    Assert-True ($collision.ExitCode -eq 4) 'same Mutex collision is rejected'
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P10', '-SessionId', (Get-Record -PartitionId 'P10').sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'close P10'

    # Bound owner process exit: the lease host observes PID + start time and exits.
    $owner = Start-Process -FilePath (Get-Command pwsh).Source -ArgumentList @('-NoProfile', '-Command', 'Start-Sleep -Seconds 1') -PassThru -WindowStyle Hidden
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P11', '-OwnerProcessId', [string]$owner.Id)
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'Claim P11 with owner PID'
    $owner.WaitForExit()
    Start-Sleep -Seconds 16
    $result = Invoke-Runner @('-Action', 'List', '-TaskSetName', $taskSet)
    Assert-True ($result.exitCode -eq 0 -and (Get-Record -PartitionId 'P11').status -eq 'abandoned') 'owner process exit is reconciled'
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P11', '-Retry')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'retry owner process exit'
    $result = Invoke-Runner @('-Action', 'CloseSession', '-TaskSetName', $taskSet, '-PartitionId', 'P11', '-SessionId', $result.json.sessionId)
    Assert-Status -Result $result -ExitCode 0 -Status 'abandoned' -Message 'close P11'

    # Claim command exits normally, but the lease remains owned by the Codex parent process.
    $result = Invoke-Runner @('-Action', 'Claim', '-TaskSetName', $taskSet, '-PartitionId', 'P12')
    Assert-Status -Result $result -ExitCode 0 -Status 'claimed' -Message 'auto Codex owner claim'
    $p12Record = Get-Record -PartitionId 'P12'
    Assert-True ($p12Record.codexSessionId -eq $originalCodexSessionId) 'Codex session identity is recorded'
    Assert-True ($null -ne (Get-Process -Id ([int]$p12Record.leaseHostProcessId) -ErrorAction SilentlyContinue)) 'host survives claim command exit'
    $result = Invoke-Runner @('-Action', 'Cleanup', '-TaskSetName', $taskSet, '-PartitionId', 'P12')
    Assert-Status -Result $result -ExitCode 0 -Status 'cleaned' -Message 'cleanup P12'

    [pscustomobject]@{
        status = 'PASS'
        taskSet = $taskSet
        cases = @('claim-heartbeat', 'wrong-session', 'duplicate-claim', 'parallel-claim', 'complete', 'fail-retry', 'abandon', 'close-session', 'handoff-old-owner', 'cleanup-reclaim', 'managed-run', 'killed-host-recovery', 'expired-lease-recovery', 'codex-session-mismatch', 'mutex-collision', 'owner-process-exit', 'auto-codex-owner-survives-claim')
    } | ConvertTo-Json -Depth 5
}
catch {
    [pscustomobject]@{ status = 'FAIL'; taskSet = $taskSet; error = $_.Exception.Message } | ConvertTo-Json -Depth 5
    throw
}
finally {
    $env:CODEX_SESSION_ID = $originalCodexSessionId
    if ([IO.Directory]::Exists($taskDirectory)) {
        $statePath = Join-Path $taskDirectory 'task-state.json'
        if ([IO.File]::Exists($statePath)) {
            try {
                $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json -DateKind String
                foreach ($record in @($state.partitions | Where-Object { $_.status -eq 'running' -and $_.claimMode -eq 'manual' })) {
                    try {
                        $null = Invoke-Runner @('-Action', 'Cleanup', '-TaskSetName', $taskSet, '-PartitionId', [string]$record.id, '-LockWaitSeconds', '5') -TimeoutSeconds 10
                    }
                    catch {
                        # Cleanup is best effort here; the assertion failure is the primary result.
                    }
                }
            }
            catch {
                # The task state may be incomplete when initialization itself fails.
            }
        }
        [IO.Directory]::Delete($taskDirectory, $true)
    }
}
