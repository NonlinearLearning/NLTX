[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$authRunner = (Resolve-Path (Join-Path $PSScriptRoot 'Invoke-Version4AuthoritativePartitionSession.ps1')).Path
$nonAuthRunner = (Resolve-Path (Join-Path $PSScriptRoot 'Invoke-Version4NonAuthoritativePartitionSession.ps1')).Path
$authTable = Join-Path $repoRoot 'docs\system-decomposition\authoritative\2026-09-18-system-decomposition-authoritative-20-partition-tasks.md'
$nonAuthTable = Join-Path $repoRoot 'docs\system-decomposition\non-authoritative\2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md'
$authTasksRoot = Join-Path $repoRoot '.agents\skills\version4-partition-session-runner\sessions\version4-authoritative-partition-session\tasks'
$nonAuthTasksRoot = Join-Path $repoRoot '.agents\skills\version4-partition-session-runner\sessions\version4-non-authoritative-partition-session\tasks'
$originalCodexSessionId = $env:CODEX_SESSION_ID
$suffix = [guid]::NewGuid().ToString('N').Substring(0, 10)
$authTaskSet = "error-auth-$suffix"
$nonAuthTaskSet = "error-nonauth-$suffix"
$testSessionId = "error-main-$suffix"
$nonAuthSessionId = "error-nonauth-$suffix"
$createdTaskSets = @(
    [pscustomobject]@{ runner = $authRunner; name = $authTaskSet; root = $authTasksRoot },
    [pscustomobject]@{ runner = $nonAuthRunner; name = $nonAuthTaskSet; root = $nonAuthTasksRoot }
)

function Invoke-Runner {
    param(
        [Parameter(Mandatory)][string] $Runner,
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
    $psi.ArgumentList.Add($Runner)
    foreach ($argument in $Arguments) {
        $psi.ArgumentList.Add([string]$argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $psi
    $null = $process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $operation = "$Runner $($Arguments -join ' ')"
        try { $process.Kill($true) } catch { }
        $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        throw "Runner timed out after $TimeoutSeconds seconds: $operation`n$stdout`n$stderr"
    }

    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $exitCode = $process.ExitCode
    $process.Dispose()
    $text = $stdout.Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        $text = $stderr.Trim()
    }
    $json = $null
    try { $json = $text | ConvertFrom-Json -DateKind String } catch { }
    return [pscustomobject]@{ exitCode = $exitCode; text = $text; json = $json }
}

function Assert-True {
    param([Parameter(Mandatory)][bool] $Condition, [Parameter(Mandatory)][string] $Message)
    if (-not $Condition) {
        throw "ASSERT FAILED: $Message"
    }
}

function Expect-Result {
    param(
        [Parameter(Mandatory)][string] $Case,
        [Parameter(Mandatory)] $Result,
        [Parameter(Mandatory)][int] $ExitCode,
        [Parameter(Mandatory)][string] $Status,
        [AllowNull()][string] $ErrorContains
    )

    Assert-True ($Result.exitCode -eq $ExitCode) "$Case exit code ($($Result.exitCode)): $($Result.text)"
    Assert-True ($null -ne $Result.json -and [string]$Result.json.status -eq $Status) "$Case status: $($Result.text)"
    if (-not [string]::IsNullOrWhiteSpace($ErrorContains)) {
        Assert-True ([string]$Result.json.error -like "*$ErrorContains*") "$Case error text: $($Result.text)"
    }
    return $Result.json
}

function Get-TaskDirectory {
    param([Parameter(Mandatory)][string] $Root, [Parameter(Mandatory)][string] $TaskSetName)
    return Join-Path $Root $TaskSetName
}

function Get-Record {
    param(
        [Parameter(Mandatory)][string] $TaskDirectory,
        [Parameter(Mandatory)][string] $PartitionId
    )

    $state = Get-Content -Raw -LiteralPath (Join-Path $TaskDirectory 'task-state.json') | ConvertFrom-Json -DateKind String
    return @($state.partitions | Where-Object { $_.id -eq $PartitionId }) | Select-Object -First 1
}

function Get-Lease {
    param([Parameter(Mandatory)][string] $TaskDirectory, [Parameter(Mandatory)][string] $PartitionId)
    return Get-Content -Raw -LiteralPath (Join-Path $TaskDirectory "session-lease-$PartitionId.json") | ConvertFrom-Json -DateKind String
}

function Write-Lease {
    param(
        [Parameter(Mandatory)][string] $TaskDirectory,
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)] $Lease
    )

    $path = Join-Path $TaskDirectory "session-lease-$PartitionId.json"
    $json = $Lease | ConvertTo-Json -Depth 8 -Compress
    [IO.File]::WriteAllText($path, $json, [Text.UTF8Encoding]::new($false))
}

function Stop-TestHosts {
    param([Parameter(Mandatory)][string] $TaskSetName)

    $escaped = [Regex]::Escape($TaskSetName)
    $hosts = Get-CimInstance Win32_Process | Where-Object {
        $_.Name -eq 'pwsh.exe' -and
        $_.CommandLine -match 'Version4SessionLeaseHost\.ps1' -and
        $_.CommandLine -match $escaped
    }
    foreach ($hostProcess in @($hosts)) {
        Stop-Process -Id ([int]$hostProcess.ProcessId) -Force -ErrorAction SilentlyContinue
    }
}

function Reclaim-Partition {
    param(
        [Parameter(Mandatory)][string] $Runner,
        [Parameter(Mandatory)][string] $TaskSetName,
        [Parameter(Mandatory)][string] $PartitionId
    )

    $result = Invoke-Runner -Runner $Runner -Arguments @('-Action', 'Claim', '-TaskSetName', $TaskSetName, '-PartitionId', $PartitionId, '-Retry')
    return Expect-Result -Case "Retry $PartitionId" -Result $result -ExitCode 0 -Status 'claimed'
}

function Cleanup-TaskSet {
    param([Parameter(Mandatory)] $Context)

    $directory = Get-TaskDirectory -Root $Context.root -TaskSetName $Context.name
    $statePath = Join-Path $directory 'task-state.json'
    if (Test-Path -LiteralPath $statePath -PathType Leaf) {
        try {
            $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json -DateKind String
            foreach ($record in @($state.partitions | Where-Object { $_.status -eq 'running' -and $_.claimMode -eq 'manual' })) {
                try {
                    $null = Invoke-Runner -Runner $Context.runner -Arguments @('-Action', 'Cleanup', '-TaskSetName', $Context.name, '-PartitionId', [string]$record.id, '-LockWaitSeconds', '5') -TimeoutSeconds 10
                }
                catch { }
            }
        }
        catch { }
    }
    Stop-TestHosts -TaskSetName $Context.name
    $fullDirectory = [IO.Path]::GetFullPath($directory)
    $fullRoot = [IO.Path]::GetFullPath($Context.root)
    if ($fullDirectory.StartsWith($fullRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -and [IO.Directory]::Exists($fullDirectory)) {
        [IO.Directory]::Delete($fullDirectory, $true)
    }
}

try {
    Assert-True (Test-Path -LiteralPath $authTable -PathType Leaf) 'authoritative task table exists'
    Assert-True (Test-Path -LiteralPath $nonAuthTable -PathType Leaf) 'non-authoritative task table exists'

    $env:CODEX_SESSION_ID = $testSessionId
    $authDirectory = Get-TaskDirectory -Root $authTasksRoot -TaskSetName $authTaskSet
    $nonAuthDirectory = Get-TaskDirectory -Root $nonAuthTasksRoot -TaskSetName $nonAuthTaskSet

    Expect-Result -Case 'Initialize authoritative' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Initialize', '-TaskSetName', $authTaskSet, '-TaskTablePath', $authTable)) -ExitCode 0 -Status 'initialized' | Out-Null
    Expect-Result -Case 'Initialize non-authoritative' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Initialize', '-TaskSetName', $nonAuthTaskSet, '-TaskTablePath', $nonAuthTable)) -ExitCode 0 -Status 'initialized' | Out-Null

    # Input and action validation must be deterministic and must not create a lease.
    Expect-Result -Case 'Unknown task set' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', "missing-$suffix")) -ExitCode 3 -Status 'invalid' -ErrorContains 'not initialized' | Out-Null
    Expect-Result -Case 'Missing initialization table' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Initialize', '-TaskSetName', "missing-table-$suffix", '-TaskTablePath', (Join-Path $repoRoot 'missing-task-table.md'))) -ExitCode 3 -Status 'invalid' | Out-Null
    Expect-Result -Case 'Task table on List' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', $authTaskSet, '-TaskTablePath', $authTable)) -ExitCode 3 -Status 'invalid' -ErrorContains 'valid only with Initialize' | Out-Null
    Expect-Result -Case 'Unknown partition' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P999')) -ExitCode 3 -Status 'invalid' -ErrorContains 'Unknown partition' | Out-Null
    Expect-Result -Case 'Missing session id' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01')) -ExitCode 3 -Status 'invalid' -ErrorContains 'SessionId is required' | Out-Null
    Expect-Result -Case 'Missing handoff id' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Handoff', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', 'missing')) -ExitCode 3 -Status 'invalid' -ErrorContains 'HandoffId is required' | Out-Null
    Expect-Result -Case 'Missing managed command' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01')) -ExitCode 3 -Status 'invalid' -ErrorContains 'CommandPath is required' | Out-Null
    Expect-Result -Case 'Malformed argument json' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', '{')) -ExitCode 3 -Status 'invalid' -ErrorContains 'CommandArgumentListJson' | Out-Null
    Expect-Result -Case 'Conflicting argument forms' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentList', '-NoProfile', '-CommandArgumentListJson', '[]')) -ExitCode 3 -Status 'invalid' -ErrorContains 'either CommandArgumentList' | Out-Null
    Expect-Result -Case 'Invalid owner process' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-OwnerProcessId', '2147483647')) -ExitCode 3 -Status 'invalid' -ErrorContains 'Owner process is not available' | Out-Null

    # Concurrent ownership and stale-token rejection.
    $claim = Expect-Result -Case 'Claim P01' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01')) -ExitCode 0 -Status 'claimed'
    $sessionId = [string]$claim.sessionId
    Expect-Result -Case 'Duplicate partition claim' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01')) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Second claim in same Codex session' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P02')) -ExitCode 4 -Status 'conflict' -ErrorContains 'already owns partition' | Out-Null
    Expect-Result -Case 'Wrong session heartbeat' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', 'wrong')) -ExitCode 4 -Status 'conflict' -ErrorContains 'Session id does not own' | Out-Null
    $env:CODEX_SESSION_ID = 'wrong-codex-session'
    Expect-Result -Case 'Wrong Codex session heartbeat' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 4 -Status 'conflict' -ErrorContains 'Owner Codex session does not own' | Out-Null
    $env:CODEX_SESSION_ID = $testSessionId

    # Lease fault injection: token mismatch, PID start-time mismatch, malformed JSON, and expiry.
    $lease = Get-Lease -TaskDirectory $authDirectory -PartitionId 'P01'
    $hostPid = [int]$lease.hostProcessId
    $lease.sessionToken = 'tampered-token'
    Write-Lease -TaskDirectory $authDirectory -PartitionId 'P01' -Lease $lease
    Expect-Result -Case 'Tampered lease token heartbeat' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Tampered lease token reconciliation' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', $authTaskSet)) -ExitCode 0 -Status 'ok' | Out-Null
    Assert-True ((Get-Record -TaskDirectory $authDirectory -PartitionId 'P01').status -eq 'abandoned') 'tampered token becomes abandoned'
    Stop-TestHosts -TaskSetName $authTaskSet
    $claim = Reclaim-Partition -Runner $authRunner -TaskSetName $authTaskSet -PartitionId 'P01'
    $sessionId = [string]$claim.sessionId

    $lease = Get-Lease -TaskDirectory $authDirectory -PartitionId 'P01'
    $hostPid = [int]$lease.hostProcessId
    $lease.hostProcessStartTime = '2000-01-01T00:00:00.0000000Z'
    Write-Lease -TaskDirectory $authDirectory -PartitionId 'P01' -Lease $lease
    Expect-Result -Case 'PID start-time mismatch heartbeat' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'PID start-time mismatch reconciliation' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', $authTaskSet)) -ExitCode 0 -Status 'ok' | Out-Null
    Assert-True ((Get-Record -TaskDirectory $authDirectory -PartitionId 'P01').status -eq 'abandoned') 'PID mismatch becomes abandoned'
    Stop-TestHosts -TaskSetName $authTaskSet
    $claim = Reclaim-Partition -Runner $authRunner -TaskSetName $authTaskSet -PartitionId 'P01'
    $sessionId = [string]$claim.sessionId

    $lease = Get-Lease -TaskDirectory $authDirectory -PartitionId 'P01'
    $hostPid = [int]$lease.hostProcessId
    [IO.File]::WriteAllText((Join-Path $authDirectory 'session-lease-P01.json'), '{not-json', [Text.UTF8Encoding]::new($false))
    Expect-Result -Case 'Corrupt lease heartbeat' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Corrupt lease reconciliation' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', $authTaskSet)) -ExitCode 0 -Status 'ok' | Out-Null
    Assert-True ((Get-Record -TaskDirectory $authDirectory -PartitionId 'P01').status -eq 'abandoned') 'corrupt lease becomes abandoned'
    Stop-TestHosts -TaskSetName $authTaskSet
    $claim = Reclaim-Partition -Runner $authRunner -TaskSetName $authTaskSet -PartitionId 'P01'
    $sessionId = [string]$claim.sessionId

    $lease = Get-Lease -TaskDirectory $authDirectory -PartitionId 'P01'
    $lease.leaseExpiresAtUtc = '2000-01-01T00:00:00.0000000Z'
    Write-Lease -TaskDirectory $authDirectory -PartitionId 'P01' -Lease $lease
    Expect-Result -Case 'Expired lease heartbeat' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Heartbeat', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Expired lease reconciliation' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', $authTaskSet)) -ExitCode 0 -Status 'ok' | Out-Null
    Assert-True ((Get-Record -TaskDirectory $authDirectory -PartitionId 'P01').status -eq 'abandoned') 'expired lease becomes abandoned'
    Stop-TestHosts -TaskSetName $authTaskSet
    $claim = Reclaim-Partition -Runner $authRunner -TaskSetName $authTaskSet -PartitionId 'P01'
    $sessionId = [string]$claim.sessionId
    Expect-Result -Case 'Close after lease recovery' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'CloseSession', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 0 -Status 'abandoned' | Out-Null

    # Terminal-state conflicts, stale owner tokens, and explicit lock contention.
    $claim = Expect-Result -Case 'Claim P01 after recovery' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-Retry')) -ExitCode 0 -Status 'claimed'
    $newSessionId = [string]$claim.sessionId
    Expect-Result -Case 'Old token after close' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Complete', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $sessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Complete current P01' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Complete', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $newSessionId)) -ExitCode 0 -Status 'completed' | Out-Null
    Expect-Result -Case 'Complete already completed' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Complete', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $newSessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Retry completed partition' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-Retry')) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Cleanup completed partition' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Cleanup', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01')) -ExitCode 0 -Status 'cleaned' | Out-Null
    $claim = Expect-Result -Case 'Reclaim after cleanup' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01')) -ExitCode 0 -Status 'claimed'
    $cleanupSessionId = [string]$claim.sessionId
    Expect-Result -Case 'Stale token after cleanup' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Complete', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $newSessionId)) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Close reclaimed P01' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'CloseSession', '-TaskSetName', $authTaskSet, '-PartitionId', 'P01', '-SessionId', $cleanupSessionId)) -ExitCode 0 -Status 'abandoned' | Out-Null

    $lockPath = Join-Path $authDirectory 'task.lock'
    $heldLock = [IO.File]::Open($lockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try {
        Expect-Result -Case 'Checkout lock contention' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'List', '-TaskSetName', $authTaskSet, '-LockWaitSeconds', '0')) -ExitCode 2 -Status 'busy' | Out-Null
    }
    finally {
        $heldLock.Dispose()
    }

    # Managed run failures are terminal until -Retry, and a successful retry is terminal.
    $failArgs = ConvertTo-Json -InputObject ([string[]]@('-NoProfile', '-Command', 'exit 7')) -Compress
    Expect-Result -Case 'Managed worker failure' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P02', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', $failArgs)) -ExitCode 5 -Status 'failed' | Out-Null
    Expect-Result -Case 'Retry failed managed worker' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P02', '-Retry', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', '[]')) -ExitCode 0 -Status 'completed' | Out-Null
    Expect-Result -Case 'Retry completed managed worker' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P02', '-Retry', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', '[]')) -ExitCode 4 -Status 'conflict' | Out-Null
    Expect-Result -Case 'Missing managed executable' -Result (Invoke-Runner -Runner $authRunner -Arguments @('-Action', 'Run', '-TaskSetName', $authTaskSet, '-PartitionId', 'P03', '-CommandPath', (Join-Path $repoRoot 'missing-worker.exe'), '-CommandArgumentListJson', '[]')) -ExitCode 5 -Status 'failed' | Out-Null

    # Non-authoritative profile has the same lease boundary plus Prune-specific validation.
    Expect-Result -Case 'Prune without switch' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Prune', '-TaskSetName', $nonAuthTaskSet)) -ExitCode 3 -Status 'invalid' -ErrorContains 'PreserveRunning' | Out-Null
    Expect-Result -Case 'Non-authoritative conflicting argument forms' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Run', '-TaskSetName', $nonAuthTaskSet, '-PartitionId', 'P01', '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentList', '-NoProfile', '-CommandArgumentListJson', '[]')) -ExitCode 3 -Status 'invalid' -ErrorContains 'either CommandArgumentList' | Out-Null
    $env:CODEX_SESSION_ID = $nonAuthSessionId
    $nonClaim = Expect-Result -Case 'Non-authoritative claim' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $nonAuthTaskSet, '-PartitionId', 'P01')) -ExitCode 0 -Status 'claimed'
    Expect-Result -Case 'Non-authoritative same-session conflict' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Claim', '-TaskSetName', $nonAuthTaskSet, '-PartitionId', 'P02')) -ExitCode 4 -Status 'conflict' | Out-Null
    $pruned = Expect-Result -Case 'Prune preserving running' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Prune', '-TaskSetName', $nonAuthTaskSet, '-PreserveRunning')) -ExitCode 0 -Status 'pruned'
    Assert-True (@($pruned.preservedPartitions) -contains 'P01') 'Prune preserves the running lease'
    Expect-Result -Case 'Close non-authoritative claim' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'CloseSession', '-TaskSetName', $nonAuthTaskSet, '-PartitionId', 'P01', '-SessionId', [string]$nonClaim.sessionId)) -ExitCode 0 -Status 'abandoned' | Out-Null
    $emptyPrune = Expect-Result -Case 'Prune with empty running set' -Result (Invoke-Runner -Runner $nonAuthRunner -Arguments @('-Action', 'Prune', '-TaskSetName', $nonAuthTaskSet, '-PreserveRunning')) -ExitCode 0 -Status 'pruned'
    Assert-True ([int]$emptyPrune.preservedCount -eq 0) 'Empty running Prune preserves zero partitions'

    [pscustomobject]@{
        status = 'PASS'
        authoritativeTaskSet = $authTaskSet
        nonAuthoritativeTaskSet = $nonAuthTaskSet
        cases = @(
            'input-validation', 'same-session-concurrency', 'wrong-session', 'wrong-codex-session',
            'lease-token-tamper', 'lease-pid-start-time-tamper', 'lease-json-corruption',
            'lease-expiry', 'terminal-state-conflicts', 'stale-token-after-cleanup',
            'checkout-lock-contention', 'managed-worker-failure-retry', 'missing-worker',
            'nonauthoritative-prune-validation', 'nonauthoritative-prune-preserve',
            'nonauthoritative-empty-prune'
        )
    } | ConvertTo-Json -Depth 6
}
catch {
    [pscustomobject]@{
        status = 'FAIL'
        authoritativeTaskSet = $authTaskSet
        nonAuthoritativeTaskSet = $nonAuthTaskSet
        error = $_.Exception.Message
    } | ConvertTo-Json -Depth 6
    throw
}
finally {
    $env:CODEX_SESSION_ID = $originalCodexSessionId
    foreach ($context in $createdTaskSets) {
        Cleanup-TaskSet -Context $context
    }
}
