[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$runnerPath = Join-Path $PSScriptRoot 'Invoke-Version4AuthoritativePartitionSession.ps1'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$sourceDirectory = Join-Path $repoRoot 'docs\组件文档\迁移参考表\Version4权威模拟系统字段属性逐成员源码声明-20分区'

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw "ASSERT FAILED: $Message" }
}

function Assert-Equal {
    param($Expected, $Actual, [string] $Message)
    if ($Expected -ne $Actual) {
        throw "ASSERT FAILED: $Message; expected=[$Expected], actual=[$Actual]"
    }
}

function Invoke-Runner {
    param(
        [string[]] $Arguments,
        [int] $TimeoutMilliseconds = 30000
    )

    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = (Get-Command pwsh).Source
    $psi.ArgumentList.Add('-NoProfile')
    $psi.ArgumentList.Add('-File')
    $psi.ArgumentList.Add($runnerPath)
    foreach ($argument in $Arguments) { $psi.ArgumentList.Add($argument) }
    $psi.WorkingDirectory = $repoRoot
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $process = [System.Diagnostics.Process]::Start($psi)
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutMilliseconds)) {
        $process.Kill($true)
        throw "Runner timed out after $TimeoutMilliseconds ms."
    }
    [pscustomobject]@{
        ExitCode = $process.ExitCode
        Stdout = $stdoutTask.GetAwaiter().GetResult()
        Stderr = $stderrTask.GetAwaiter().GetResult()
    }
}

function Start-Runner {
    param([string[]] $Arguments)

    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = (Get-Command pwsh).Source
    $psi.ArgumentList.Add('-NoProfile')
    $psi.ArgumentList.Add('-File')
    $psi.ArgumentList.Add($runnerPath)
    foreach ($argument in $Arguments) { $psi.ArgumentList.Add($argument) }
    $psi.WorkingDirectory = $repoRoot
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $process = [System.Diagnostics.Process]::Start($psi)
    [pscustomobject]@{
        Process = $process
        StdoutTask = $process.StandardOutput.ReadToEndAsync()
        StderrTask = $process.StandardError.ReadToEndAsync()
    }
}

function Complete-Runner {
    param($Handle)

    $Handle.Process.WaitForExit()
    [pscustomobject]@{
        ExitCode = $Handle.Process.ExitCode
        Stdout = $Handle.StdoutTask.GetAwaiter().GetResult()
        Stderr = $Handle.StderrTask.GetAwaiter().GetResult()
    }
}

function Read-JsonResult {
    param($Invocation)
    if ([string]::IsNullOrWhiteSpace($Invocation.Stdout)) {
        throw "Runner returned no JSON. stderr=$($Invocation.Stderr)"
    }
    $Invocation.Stdout | ConvertFrom-Json
}

function Read-State {
    param([string] $Path)
    Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-TestPartitionRecord {
    param(
        $State,
        [string] $PartitionId
    )

    @($State.partitions | Where-Object id -eq $PartitionId) | Select-Object -First 1
}

function Wait-ForRunning {
    param([string] $Path, [string] $PartitionId)
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    while ([DateTime]::UtcNow -lt $deadline) {
        if (Test-Path -LiteralPath $Path) {
            $state = Read-State $Path
            $record = @($state.partitions | Where-Object id -eq $PartitionId) | Select-Object -First 1
            if ($null -ne $record -and $record.status -eq 'running') { return }
        }
        Start-Sleep -Milliseconds 50
    }
    throw "Timed out waiting for $PartitionId to enter running state."
}

function Get-CommonArguments {
    param([string] $StatePath, [string] $LockPath)
    @('-PartitionDirectory', $sourceDirectory, '-StatePath', $StatePath, '-LockPath', $LockPath)
}

function Get-SuccessCommandArguments {
    param([string] $PartitionId)
    $childArguments = @('-NoProfile', '-Command', 'Write-Output "$env:VERSION4_PARTITION_ID|$env:VERSION4_PARTITION_SESSION_ID"')
    @('-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$childArguments) -Compress))
}

function Assert-ClearedRecord {
    param(
        $Record,
        [string] $Message
    )

    Assert-Equal 'available' $Record.status "$Message status"
    Assert-True ($null -eq $Record.claimMode) "$Message claim mode"
    Assert-True ($null -eq $Record.sessionId) "$Message session id"
    Assert-True ($null -eq $Record.ownerProcessId) "$Message owner process id"
    Assert-True ($null -eq $Record.ownerProcessStartTime) "$Message owner process start time"
    Assert-True ($null -eq $Record.claimedAtUtc) "$Message claimed timestamp"
    Assert-True ($null -eq $Record.startedAtUtc) "$Message started timestamp"
    Assert-True ($null -eq $Record.completedAtUtc) "$Message completed timestamp"
    Assert-True ($null -eq $Record.abandonedAtUtc) "$Message abandoned timestamp"
    Assert-True ($null -eq $Record.exitCode) "$Message exit code"
    Assert-True ($null -eq $Record.commandPath) "$Message command path"
    Assert-Equal 0 @($Record.commandArguments).Count "$Message command arguments"
    Assert-True ($null -eq $Record.stdout) "$Message stdout"
    Assert-True ($null -eq $Record.stderr) "$Message stderr"
    Assert-True ($null -eq $Record.error) "$Message error"
}

function Assert-FixedRecordFields {
    param(
        $Record,
        [string] $ExpectedReportFileName,
        [int] $ExpectedMemberCount,
        [string] $Message
    )

    Assert-Equal $ExpectedReportFileName $Record.reportFileName "$Message report file name"
    Assert-Equal $ExpectedMemberCount $Record.expectedMemberCount "$Message expected member count"
    Assert-True (-not [string]::IsNullOrWhiteSpace([string]$Record.reportPath)) "$Message report path"
}

$testRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('version4-partition-session-test-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $testRoot -Force

try {
    Assert-True (Test-Path -LiteralPath $sourceDirectory) 'the authoritative partition directory must exist'

    $statePath = Join-Path $testRoot 'basic-state.json'
    $lockPath = Join-Path $testRoot 'basic-state.lock'
    $common = Get-CommonArguments $statePath $lockPath

    $list = Invoke-Runner (@('-Action', 'List') + $common)
    Assert-True ($list.ExitCode -eq 0) 'List should succeed'
    $listResult = Read-JsonResult $list
    Assert-True ($listResult.partitions.Count -eq 20) 'List should return twenty partitions'
    Assert-True (@($listResult.partitions | Where-Object id -eq 'P01').Count -eq 1) 'List should include P01'
    Assert-True (@($listResult.partitions | Where-Object id -eq 'P20').Count -eq 1) 'List should include P20'
    Assert-Equal 113 (@($listResult.partitions | Where-Object id -eq 'P01').expectedMemberCount) 'P01 expected count'

    $claimState = Join-Path $testRoot 'claim-state.json'
    $claimLock = Join-Path $testRoot 'claim-state.lock'
    $claimCommon = Get-CommonArguments $claimState $claimLock
    $claim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P01', '-LockWaitSeconds', '5') + $claimCommon)
    Assert-Equal 0 $claim.ExitCode 'Claim should exit zero'
    $claimResult = Read-JsonResult $claim
    Assert-Equal 'claimed' $claimResult.status 'Claim status'
    Assert-Equal 'P01' $claimResult.partition 'Claim partition'
    Assert-Equal $true $claimResult.lockReleased 'Claim should report released lock'
    Assert-True (-not [string]::IsNullOrWhiteSpace($claimResult.sessionId)) 'Claim should return a session id'
    $claimRecord = @((Read-State $claimState).partitions | Where-Object id -eq 'P01') | Select-Object -First 1
    Assert-Equal 'running' $claimRecord.status 'Claim should persist running status'
    Assert-Equal 'manual' $claimRecord.claimMode 'Claim should use manual claim mode'

    $parallelClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P02', '-LockWaitSeconds', '5') + $claimCommon)
    Assert-Equal 0 $parallelClaim.ExitCode 'Claim should release the lock after persisting'
    $parallelClaimResult = Read-JsonResult $parallelClaim
    Assert-Equal 'claimed' $parallelClaimResult.status 'A different partition should be claimable after Claim'
    Assert-Equal $true $parallelClaimResult.lockReleased 'Parallel Claim should report released lock'

    $duplicateClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P01') + $claimCommon)
    Assert-Equal 4 $duplicateClaim.ExitCode 'Duplicate Claim should conflict'
    Assert-Equal 'conflict' (Read-JsonResult $duplicateClaim).status 'Duplicate Claim status'

    $complete = Invoke-Runner (@('-Action', 'Complete', '-PartitionId', 'P01', '-SessionId', $claimResult.sessionId, '-LockWaitSeconds', '5') + $claimCommon)
    Assert-Equal 0 $complete.ExitCode 'Complete should exit zero'
    $completeResult = Read-JsonResult $complete
    Assert-Equal 'completed' $completeResult.status 'Complete status'
    Assert-Equal $true $completeResult.lockReleased 'Complete should report released lock'

    $handoffState = Join-Path $testRoot 'handoff-state.json'
    $handoffLock = Join-Path $testRoot 'handoff-state.lock'
    $handoffCommon = Get-CommonArguments $handoffState $handoffLock
    $handoffClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P01') + $handoffCommon)
    Assert-Equal 0 $handoffClaim.ExitCode 'Handoff setup claim should succeed'
    $handoffClaimResult = Read-JsonResult $handoffClaim
    $oldHandoffSessionId = $handoffClaimResult.sessionId
    $handoffId = 'handoff-authoritative-fixture-001'
    $handoff = Invoke-Runner (@('-Action', 'Handoff', '-PartitionId', 'P01', '-SessionId', $oldHandoffSessionId, '-HandoffId', $handoffId) + $handoffCommon)
    Assert-Equal 0 $handoff.ExitCode 'Handoff should exit zero'
    $handoffResult = Read-JsonResult $handoff
    Assert-Equal 'handed-off' $handoffResult.status 'Handoff status'
    Assert-Equal 'P01' $handoffResult.partition 'Handoff partition'
    Assert-Equal $oldHandoffSessionId $handoffResult.oldSessionId 'Handoff should report the old session id'
    Assert-Equal $handoffId $handoffResult.handoffId 'Handoff should report the handoff id'
    Assert-True ($handoffResult.sessionId -ne $oldHandoffSessionId) 'Handoff should issue a new session id'
    $handoffRecord = Get-TestPartitionRecord -State (Read-State $handoffState) -PartitionId 'P01'
    Assert-Equal 'running' $handoffRecord.status 'Handoff should preserve running status'
    Assert-Equal 'manual' $handoffRecord.claimMode 'Handoff should preserve manual claim mode'
    Assert-Equal $handoffResult.sessionId $handoffRecord.sessionId 'Handoff should persist the new session id'
    Assert-Equal $handoffId $handoffRecord.handoffId 'Handoff should persist the handoff id'
    Assert-Equal $oldHandoffSessionId $handoffRecord.previousSessionId 'Handoff should persist the old session id'
    $oldHandoffSettlement = Invoke-Runner (@('-Action', 'Complete', '-PartitionId', 'P01', '-SessionId', $oldHandoffSessionId) + $handoffCommon)
    Assert-Equal 4 $oldHandoffSettlement.ExitCode 'The old session must not settle after Handoff'
    Assert-Equal 'conflict' (Read-JsonResult $oldHandoffSettlement).status 'Old Handoff session conflict status'
    $newHandoffSettlement = Invoke-Runner (@('-Action', 'Complete', '-PartitionId', 'P01', '-SessionId', $handoffResult.sessionId) + $handoffCommon)
    Assert-Equal 0 $newHandoffSettlement.ExitCode 'The new session should settle after Handoff'
    Assert-Equal 'completed' (Read-JsonResult $newHandoffSettlement).status 'New Handoff session settlement status'

    $failedClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P03') + $claimCommon)
    Assert-Equal 0 $failedClaim.ExitCode 'second manual Claim should exit zero'
    $failedClaimResult = Read-JsonResult $failedClaim
    $fail = Invoke-Runner (@('-Action', 'Fail', '-PartitionId', 'P03', '-SessionId', $failedClaimResult.sessionId, '-FailureMessage', 'fixture failure', '-ResultExitCode', '17') + $claimCommon)
    Assert-Equal 5 $fail.ExitCode 'Fail should return child-failure exit code'
    $failResult = Read-JsonResult $fail
    Assert-Equal 'failed' $failResult.status 'Fail status'
    Assert-Equal 17 $failResult.exitCode 'Fail exit code'
    Assert-Equal $true $failResult.lockReleased 'Fail should report released lock'

    $retryClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P03', '-Retry') + $claimCommon)
    Assert-Equal 0 $retryClaim.ExitCode 'Retry should reclaim a failed manual claim'
    Assert-Equal 'claimed' (Read-JsonResult $retryClaim).status 'Retry Claim status'

    $abandonClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P06') + $claimCommon)
    Assert-Equal 0 $abandonClaim.ExitCode 'third manual Claim should exit zero'
    $abandonClaimResult = Read-JsonResult $abandonClaim
    $abandon = Invoke-Runner (@('-Action', 'Abandon', '-PartitionId', 'P06', '-SessionId', $abandonClaimResult.sessionId, '-LockWaitSeconds', '5') + $claimCommon)
    Assert-Equal 0 $abandon.ExitCode 'Abandon should exit zero'
    $abandonResult = Read-JsonResult $abandon
    Assert-Equal 'abandoned' $abandonResult.status 'Abandon status'
    Assert-Equal $true $abandonResult.lockReleased 'Abandon should report released lock'

    $cleanupState = Join-Path $testRoot 'cleanup-state.json'
    $cleanupLock = Join-Path $testRoot 'cleanup-state.lock'
    $cleanupCommon = Get-CommonArguments $cleanupState $cleanupLock

    $cleanupRunningClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P01') + $cleanupCommon)
    Assert-Equal 0 $cleanupRunningClaim.ExitCode 'cleanup running setup claim should succeed'
    $cleanupRunning = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P01', '-LockWaitSeconds', '5') + $cleanupCommon)
    Assert-Equal 0 $cleanupRunning.ExitCode 'Cleanup should clear a running record'
    $cleanupRunningResult = Read-JsonResult $cleanupRunning
    Assert-Equal 'cleaned' $cleanupRunningResult.status 'Cleanup running status'
    Assert-Equal 'running' $cleanupRunningResult.previousStatus 'Cleanup running previous status'
    Assert-Equal 'P01' $cleanupRunningResult.partition 'Cleanup running partition'
    Assert-Equal $true $cleanupRunningResult.lockReleased 'Cleanup running lock release'
    $cleanupRunningRecord = Get-TestPartitionRecord -State (Read-State $cleanupState) -PartitionId 'P01'
    Assert-ClearedRecord $cleanupRunningRecord 'Cleanup running record'
    Assert-FixedRecordFields $cleanupRunningRecord 'P01-Liquid-Wiring-Spatial-Death-Teleport.md' 113 'Cleanup running record'

    $cleanupRunningReclaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P01') + $cleanupCommon)
    Assert-Equal 0 $cleanupRunningReclaim.ExitCode 'a cleaned running record should be claimable'

    $cleanupCompleted = Invoke-Runner (@('-Action', 'Complete', '-PartitionId', 'P01', '-SessionId', (Read-JsonResult $cleanupRunningReclaim).sessionId) + $cleanupCommon)
    Assert-Equal 0 $cleanupCompleted.ExitCode 'cleanup completed setup should succeed'
    $cleanupCompletedCleanup = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P01') + $cleanupCommon)
    Assert-Equal 0 $cleanupCompletedCleanup.ExitCode 'Cleanup should clear a completed record'
    Assert-Equal 'completed' (Read-JsonResult $cleanupCompletedCleanup).previousStatus 'Cleanup completed previous status'
    $cleanupCompletedRecord = Get-TestPartitionRecord -State (Read-State $cleanupState) -PartitionId 'P01'
    Assert-ClearedRecord $cleanupCompletedRecord 'Cleanup completed record'
    Assert-FixedRecordFields $cleanupCompletedRecord 'P01-Liquid-Wiring-Spatial-Death-Teleport.md' 113 'Cleanup completed record'

    $cleanupFailureArguments = @('-NoProfile', '-Command', 'Write-Output failure-output; [Console]::Error.WriteLine("failure-error"); exit 7')
    $cleanupFailedRun = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P02') + $cleanupCommon + @('-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$cleanupFailureArguments) -Compress)))
    Assert-Equal 5 $cleanupFailedRun.ExitCode 'cleanup failed setup should record child failure'
    $cleanupFailedCleanup = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P02') + $cleanupCommon)
    Assert-Equal 0 $cleanupFailedCleanup.ExitCode 'Cleanup should clear a failed record'
    Assert-Equal 'failed' (Read-JsonResult $cleanupFailedCleanup).previousStatus 'Cleanup failed previous status'
    $cleanupFailedRecord = Get-TestPartitionRecord -State (Read-State $cleanupState) -PartitionId 'P02'
    Assert-ClearedRecord $cleanupFailedRecord 'Cleanup failed record'
    Assert-FixedRecordFields $cleanupFailedRecord 'P02-Leashed-Entity.md' 98 'Cleanup failed record'

    $cleanupAbandonClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P03') + $cleanupCommon)
    Assert-Equal 0 $cleanupAbandonClaim.ExitCode 'cleanup abandoned setup claim should succeed'
    $cleanupAbandonSession = (Read-JsonResult $cleanupAbandonClaim).sessionId
    $cleanupAbandon = Invoke-Runner (@('-Action', 'Abandon', '-PartitionId', 'P03', '-SessionId', $cleanupAbandonSession) + $cleanupCommon)
    Assert-Equal 0 $cleanupAbandon.ExitCode 'cleanup abandoned setup should succeed'
    $cleanupAbandonedCleanup = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P03') + $cleanupCommon)
    Assert-Equal 0 $cleanupAbandonedCleanup.ExitCode 'Cleanup should clear an abandoned record'
    Assert-Equal 'abandoned' (Read-JsonResult $cleanupAbandonedCleanup).previousStatus 'Cleanup abandoned previous status'
    $cleanupAbandonedRecord = Get-TestPartitionRecord -State (Read-State $cleanupState) -PartitionId 'P03'
    Assert-ClearedRecord $cleanupAbandonedRecord 'Cleanup abandoned record'
    Assert-FixedRecordFields $cleanupAbandonedRecord 'P03-Mount-Vehicle.md' 163 'Cleanup abandoned record'

    $cleanupIsolationClaim = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P04') + $cleanupCommon)
    Assert-Equal 0 $cleanupIsolationClaim.ExitCode 'cleanup isolation setup claim should succeed'
    $cleanupAvailableAgain = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P03') + $cleanupCommon)
    Assert-Equal 0 $cleanupAvailableAgain.ExitCode 'Cleanup should be idempotent for an available record'
    Assert-Equal 'available' (Read-JsonResult $cleanupAvailableAgain).previousStatus 'Cleanup available previous status'
    Assert-Equal 'running' (@((Read-State $cleanupState).partitions | Where-Object id -eq 'P04').status) 'Cleanup should not alter another partition'

    $cleanupBusyState = Join-Path $testRoot 'cleanup-busy-state.json'
    $cleanupBusyLock = Join-Path $testRoot 'cleanup-busy.lock'
    $cleanupBusyCommon = Get-CommonArguments $cleanupBusyState $cleanupBusyLock
    $cleanupBusySetup = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P05') + $cleanupBusyCommon)
    Assert-Equal 0 $cleanupBusySetup.ExitCode 'cleanup busy setup claim should succeed'
    $lockReadyPath = Join-Path $testRoot 'cleanup-busy.ready'
    $lockHolderCommand = "`$stream = [System.IO.FileStream]::new('$($cleanupBusyLock.Replace("'", "''"))', [System.IO.FileMode]::OpenOrCreate, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None); Set-Content -LiteralPath '$($lockReadyPath.Replace("'", "''"))' -Value ready; Start-Sleep -Seconds 2; `$stream.Dispose()"
    $lockHolder = Start-Process -FilePath (Get-Command pwsh).Source -ArgumentList @('-NoProfile', '-Command', $lockHolderCommand) -PassThru -WindowStyle Hidden
    $lockReadyDeadline = [DateTime]::UtcNow.AddSeconds(5)
    while (-not (Test-Path -LiteralPath $lockReadyPath) -and [DateTime]::UtcNow -lt $lockReadyDeadline) {
        Start-Sleep -Milliseconds 50
    }
    Assert-True (Test-Path -LiteralPath $lockReadyPath) 'lock holder should signal after acquiring the lock'
    $cleanupBusy = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P05') + $cleanupBusyCommon)
    Assert-Equal 2 $cleanupBusy.ExitCode 'Cleanup should report a busy lock'
    Assert-Equal 'busy' (Read-JsonResult $cleanupBusy).status 'Cleanup busy status'
    Assert-Equal 'running' (@((Read-State $cleanupBusyState).partitions | Where-Object id -eq 'P05').status) 'busy Cleanup should not mutate state'
    $lockHolder.WaitForExit()
    $cleanupAfterBusy = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P05') + $cleanupBusyCommon)
    Assert-Equal 0 $cleanupAfterBusy.ExitCode 'Cleanup should reuse the lock after contention'
    Assert-Equal 'cleaned' (Read-JsonResult $cleanupAfterBusy).status 'Cleanup after busy status'

    $managedCleanupState = Join-Path $testRoot 'managed-cleanup-state.json'
    $managedCleanupLock = Join-Path $testRoot 'managed-cleanup.lock'
    $managedCleanupCommon = Get-CommonArguments $managedCleanupState $managedCleanupLock
    $managedSleepArguments = @('-NoProfile', '-Command', 'Start-Sleep -Seconds 2')
    $managedRun = Start-Runner (@('-Action', 'Run', '-PartitionId', 'P06') + $managedCleanupCommon + @('-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$managedSleepArguments) -Compress)))
    Wait-ForRunning $managedCleanupState 'P06'
    $managedCleanup = Invoke-Runner (@('-Action', 'Cleanup', '-PartitionId', 'P06') + $managedCleanupCommon)
    Assert-Equal 0 $managedCleanup.ExitCode 'Cleanup should clear an active managed record'
    Assert-Equal 'running' (Read-JsonResult $managedCleanup).previousStatus 'active managed Cleanup previous status'
    $managedRunResult = Complete-Runner $managedRun
    Assert-Equal 4 $managedRunResult.ExitCode 'a managed run cleaned while executing should not finalize over cleanup'
    Assert-Equal 'conflict' (Read-JsonResult $managedRunResult).status 'cleaned managed run finalization status'
    Assert-Equal 'available' (@((Read-State $managedCleanupState).partitions | Where-Object id -eq 'P06').status) 'cleaned managed record should remain available'

    $invalidCleanupState = Join-Path $testRoot 'invalid-cleanup-state.json'
    $invalidCleanupLock = Join-Path $testRoot 'invalid-cleanup.lock'
    $invalidCleanup = Invoke-Runner @('-Action', 'Cleanup', '-PartitionId', 'P99', '-StatePath', $invalidCleanupState, '-LockPath', $invalidCleanupLock)
    Assert-Equal 3 $invalidCleanup.ExitCode 'unknown Cleanup partition should fail validation'
    Assert-Equal 'invalid' (Read-JsonResult $invalidCleanup).status 'unknown Cleanup partition status'
    Assert-True (-not (Test-Path -LiteralPath $invalidCleanupState)) 'invalid Cleanup must not create ledger state'

    $success = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P01') + $common + (Get-SuccessCommandArguments 'P01'))
    Assert-True ($success.ExitCode -eq 0) 'successful Run should exit zero'
    $successResult = Read-JsonResult $success
    Assert-Equal 'completed' $successResult.status 'successful Run status'
    Assert-Equal 0 $successResult.exitCode 'successful child exit code'
    Assert-Equal $true $successResult.lockReleased 'successful Run should report released lock'
    Assert-True ($successResult.stdout -match 'P01\|') 'partition/session environment should reach child'

    $completed = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P01') + $common + (Get-SuccessCommandArguments 'P01'))
    Assert-Equal 4 $completed.ExitCode 'completed partition should refuse rerun'
    Assert-Equal 'conflict' (Read-JsonResult $completed).status 'completed conflict status'

    $failureState = Join-Path $testRoot 'failure-state.json'
    $failureLock = Join-Path $testRoot 'failure-state.lock'
    $failureCommon = Get-CommonArguments $failureState $failureLock
    $failureArguments = @('-NoProfile', '-Command', 'exit 7')
    $failure = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P02') + $failureCommon + @('-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$failureArguments) -Compress)))
    Assert-Equal 5 $failure.ExitCode 'failed child should return child-failure exit code'
    $failureResult = Read-JsonResult $failure
    Assert-Equal 'failed' $failureResult.status 'failed child status'
    Assert-Equal 7 $failureResult.exitCode 'failed child exit code'
    Assert-Equal $true $failureResult.lockReleased 'failed Run should report released lock'
    $retry = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P02', '-Retry') + $failureCommon + (Get-SuccessCommandArguments 'P02'))
    Assert-Equal 0 $retry.ExitCode 'failed partition retry should succeed'
    $retryResult = Read-JsonResult $retry
    Assert-Equal 'completed' $retryResult.status 'retry status'
    Assert-Equal $true $retryResult.lockReleased 'retry Run should report released lock'

    $emptyArgumentsState = Join-Path $testRoot 'empty-arguments-state.json'
    $emptyArgumentsLock = Join-Path $testRoot 'empty-arguments-state.lock'
    $emptyArgumentsCommon = Get-CommonArguments $emptyArgumentsState $emptyArgumentsLock
    $emptyArguments = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P03') + $emptyArgumentsCommon + @('-CommandPath', (Get-Command where.exe).Source, '-CommandArgumentListJson', '[]'))
    Assert-Equal 5 $emptyArguments.ExitCode 'empty JSON argument array should launch the child'
    Assert-Equal 2 (Read-JsonResult $emptyArguments).exitCode 'empty JSON argument array child exit code'

    $singleArgumentState = Join-Path $testRoot 'single-argument-state.json'
    $singleArgumentLock = Join-Path $testRoot 'single-argument-state.lock'
    $singleArgumentCommon = Get-CommonArguments $singleArgumentState $singleArgumentLock
    $singleArgument = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P04') + $singleArgumentCommon + @('-CommandPath', (Get-Command where.exe).Source, '-CommandArgumentListJson', '["/?"]'))
    Assert-Equal 0 $singleArgument.ExitCode 'single JSON argument should launch the child'
    Assert-Equal 0 (Read-JsonResult $singleArgument).exitCode 'single JSON argument child exit code'

    $idState = Join-Path $testRoot 'id-state.json'
    $idLock = Join-Path $testRoot 'id-state.lock'
    $idDirectory = Join-Path $testRoot 'id-report'
    $null = New-Item -ItemType Directory -Path $idDirectory -Force
    $p01Source = Join-Path $sourceDirectory 'P01-Liquid-Wiring-Spatial-Death-Teleport.md'
    $p01WithId = Join-Path $idDirectory 'P01-Liquid-Wiring-Spatial-Death-Teleport.md'
    Copy-Item -LiteralPath $p01Source -Destination $p01WithId
    Add-Content -LiteralPath $p01WithId -Value '| source | ID.cs |'
    $idArguments = @('-NoProfile', '-Command', 'exit 0')
    $idReport = Invoke-Runner @('-Action', 'Run', '-PartitionId', 'P01', '-PartitionDirectory', $idDirectory, '-StatePath', $idState, '-LockPath', $idLock, '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$idArguments) -Compress))
    Assert-Equal 3 $idReport.ExitCode 'ID-class source row should fail validation'
    Assert-Equal 'invalid' (Read-JsonResult $idReport).status 'ID-class source row status'
    Assert-True (-not (Test-Path -LiteralPath $idState)) 'ID-class validation failure must not create ledger state'

    $nextState = Join-Path $testRoot 'next-state.json'
    $nextLock = Join-Path $testRoot 'next-state.lock'
    $nextCommon = Get-CommonArguments $nextState $nextLock
    $nextOne = Invoke-Runner (@('-Action', 'RunNext') + $nextCommon + (Get-SuccessCommandArguments 'P01'))
    Assert-Equal 0 $nextOne.ExitCode 'first RunNext should succeed'
    Assert-Equal 'P01' (Read-JsonResult $nextOne).partition 'first RunNext order'
    $nextTwo = Invoke-Runner (@('-Action', 'RunNext') + $nextCommon + (Get-SuccessCommandArguments 'P02'))
    Assert-Equal 0 $nextTwo.ExitCode 'second RunNext should succeed'
    Assert-Equal 'P02' (Read-JsonResult $nextTwo).partition 'second RunNext order'

    $invalidState = Join-Path $testRoot 'invalid-state.json'
    $invalidLock = Join-Path $testRoot 'invalid-state.lock'
    $invalidDirectory = Join-Path $testRoot 'missing-reports'
    $invalidArguments = @('-NoProfile', '-Command', 'exit 0')
    $invalid = Invoke-Runner @('-Action', 'Run', '-PartitionId', 'P01', '-PartitionDirectory', $invalidDirectory, '-StatePath', $invalidState, '-LockPath', $invalidLock, '-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$invalidArguments) -Compress))
    Assert-Equal 3 $invalid.ExitCode 'missing report should fail validation'
    Assert-Equal 'invalid' (Read-JsonResult $invalid).status 'invalid report status'
    Assert-True (-not (Test-Path -LiteralPath $invalidState)) 'validation failure must not create ledger state'

    $abandonedState = Join-Path $testRoot 'abandoned-state.json'
    $abandonedLock = Join-Path $testRoot 'abandoned-state.lock'
    $abandonedCommon = Get-CommonArguments $abandonedState $abandonedLock
    $null = Invoke-Runner (@('-Action', 'List') + $abandonedCommon)
    $deadProcess = Start-Process -FilePath (Get-Command pwsh).Source -ArgumentList @('-NoProfile', '-Command', 'exit 0') -PassThru -WindowStyle Hidden
    $deadProcess.WaitForExit()
    $abandonedLedger = Read-State $abandonedState
    $abandonedRecord = @($abandonedLedger.partitions | Where-Object id -eq 'P03') | Select-Object -First 1
    $abandonedRecord.status = 'running'
    $abandonedRecord.ownerProcessId = $deadProcess.Id
    $abandonedRecord.ownerProcessStartTime = [DateTime]::UtcNow.AddMinutes(-5).ToString('o')
    $abandonedRecord.sessionId = 'dead-session'
    $abandonedLedger | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $abandonedState -Encoding utf8
    $recovered = Invoke-Runner (@('-Action', 'List') + $abandonedCommon)
    Assert-Equal 0 $recovered.ExitCode 'dead owner reconciliation should succeed'
    Assert-Equal 'abandoned' (@((Read-JsonResult $recovered).partitions | Where-Object id -eq 'P03').status) 'dead owner status'
    $recoveredRetry = Invoke-Runner (@('-Action', 'Run', '-PartitionId', 'P03', '-Retry') + $abandonedCommon + (Get-SuccessCommandArguments 'P03'))
    Assert-Equal 0 $recoveredRetry.ExitCode 'abandoned partition retry should succeed'

    $concurrentState = Join-Path $testRoot 'concurrent-state.json'
    $concurrentLock = Join-Path $testRoot 'concurrent-state.lock'
    $concurrentCommon = Get-CommonArguments $concurrentState $concurrentLock
    $sleepArguments = @('-NoProfile', '-Command', 'Start-Sleep -Seconds 2')
    $sleeping = Start-Runner (@('-Action', 'Run', '-PartitionId', 'P04') + $concurrentCommon + @('-CommandPath', (Get-Command pwsh).Source, '-CommandArgumentListJson', (ConvertTo-Json -InputObject ([string[]]$sleepArguments) -Compress)))
    Wait-ForRunning $concurrentState 'P04'
    $contender = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P05') + $concurrentCommon)
    Assert-Equal 0 $contender.ExitCode 'a different partition should be claimable while P04 executes'
    Assert-Equal 'claimed' (Read-JsonResult $contender).status 'parallel partition claim status'
    $samePartition = Invoke-Runner (@('-Action', 'Claim', '-PartitionId', 'P04') + $concurrentCommon)
    Assert-Equal 4 $samePartition.ExitCode 'the executing partition should reject a duplicate claim'
    Assert-Equal 'conflict' (Read-JsonResult $samePartition).status 'duplicate executing partition status'
    $sleepingResult = Complete-Runner $sleeping
    Assert-Equal 0 $sleepingResult.ExitCode 'first concurrent session should finish'
    $afterConcurrent = Invoke-Runner (@('-Action', 'List') + $concurrentCommon)
    Assert-Equal 0 $afterConcurrent.ExitCode 'lock should release after concurrent execution'
    Assert-Equal 'running' (@((Read-JsonResult $afterConcurrent).partitions | Where-Object id -eq 'P05').status) 'parallel claim should remain running until finalized'

    Write-Output 'PASS: authoritative partition session contract tests passed.'
}
finally {
    if (Test-Path -LiteralPath $testRoot) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
