<#
.SYNOPSIS
    Manages one isolated Version4 authoritative partition task set.

.DESCRIPTION
    Initializes a Markdown task table, lists its ledger, claims one partition for a manual
    session or managed worker, and records completion, failure, abandonment, cleanup, or handoff.
    Claim and settlement locks are short-lived; the caller performs partition work after Claim
    returns. JSON is written to stdout and the process exit code carries the operation result.

    Use version4-partition-session-runner for the shared operating contract and
    sessions/version4-authoritative-partition-session for the authoritative evidence and write
    boundary.
    This tool does not analyze a partition, implement ECS code, or prove behavior equivalence.

.PARAMETER Action
    Initialize, List, Claim, ClaimNext, Run, RunNext, Heartbeat, Complete, Fail, Abandon,
    CloseSession, Cleanup, or Handoff.

.PARAMETER TaskSetName
    Isolated task-set name. Required for Initialize and for actions using named task-set state.

.PARAMETER TaskTablePath
    Markdown task table used only by Initialize. The table is copied and hashed into the task set.

.PARAMETER PartitionId
    Partition selected by Claim, Run, Complete, Fail, Abandon, Cleanup, or Handoff.

.PARAMETER CommandPath
    Executable used by Run or RunNext for a managed worker.

.PARAMETER CommandArgumentList
    String arguments passed to the managed worker. Do not combine with CommandArgumentListJson.

.PARAMETER CommandArgumentListJson
    JSON array of string arguments for a managed worker.

.PARAMETER SessionId
    Session ID returned by Claim or ClaimNext for Complete, Fail, or Abandon.

.PARAMETER HandoffId
    Caller-provided handoff identifier required by Handoff.

.PARAMETER FailureMessage
    Failure explanation stored by Fail.

.PARAMETER ResultExitCode
    Optional worker/result exit code stored by a failure settlement.

.PARAMETER Retry
    Allows failed or abandoned records to be claimed again; completed records are not retried.

.PARAMETER LockWaitSeconds
    Maximum seconds to wait for the short ledger lock. It does not hold the lock during analysis.

.PARAMETER OwnerProcessId
    Optional stable Codex host process ID. When supplied, the lease host releases the lease when
    this process exits or its start time changes. VERSION4_CODEX_SESSION_PID is used when omitted.

.EXAMPLE
    pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
        -Action Initialize -TaskSetName authoritative-system-decomposition `
        -TaskTablePath .\docs\system-decomposition\authoritative\2026-09-18-system-decomposition-authoritative-20-partition-tasks.md

.EXAMPLE
    pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
        -Action ClaimNext -TaskSetName authoritative-system-decomposition -LockWaitSeconds 60

.OUTPUTS
    JSON status/result object written to stdout. Exit codes: 0 success, 2 lock busy, 3 invalid
    input, 4 state conflict, 5 worker/manual failure, 6 internal error.

.NOTES
    Do not edit task-state.json or delete task.lock. A completed ledger record means only that the
    task session was settled; it is not a migration or behavior-verification result.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Initialize', 'List', 'Claim', 'ClaimNext', 'Run', 'RunNext', 'Heartbeat', 'Complete', 'Fail', 'Abandon', 'CloseSession', 'Cleanup', 'Handoff')]
    [string] $Action,

    [string] $TaskSetName,

    [string] $TaskTablePath,

    [string] $PartitionId,

    [string] $CommandPath,

    [string[]] $CommandArgumentList,

    [string] $CommandArgumentListJson,

    [string] $SessionId,

    [string] $HandoffId,

    [string] $FailureMessage,

    [Nullable[int]] $ResultExitCode,

    [Nullable[int]] $OwnerProcessId,

    [switch] $Retry,

    [ValidateRange(0, 86400)]
    [int] $LockWaitSeconds = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Set-Variable -Option Constant -Name ExitSuccess -Value 0
Set-Variable -Option Constant -Name ExitBusy -Value 2
Set-Variable -Option Constant -Name ExitInvalid -Value 3
Set-Variable -Option Constant -Name ExitConflict -Value 4
Set-Variable -Option Constant -Name ExitChildFailure -Value 5
Set-Variable -Option Constant -Name ExitInternal -Value 6

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$taskRoot = Join-Path $repoRoot '.agents\skills\version4-partition-session-runner\sessions\version4-authoritative-partition-session\tasks'
$activeTaskSetContext = $null
Import-Module (Join-Path $PSScriptRoot 'Version4PartitionSessionLease.psm1') -Force

function New-PartitionDefinition {
    param(
        [Parameter(Mandatory)][string] $Id,
        [Parameter(Mandatory)][string] $FileName,
        [Parameter(Mandatory)][int] $ExpectedMemberCount,
        [AllowNull()][string] $TaskId,
        [AllowNull()][string] $ReportPath,
        [AllowNull()][string] $PromptPath,
        [AllowNull()][string] $OutputReportPath,
        [int] $Order = 0
    )

    [pscustomobject] @{
        id = $Id
        taskId = $TaskId
        order = $Order
        reportFileName = $FileName
        reportPath = $ReportPath
        promptPath = $PromptPath
        outputReportPath = $OutputReportPath
        expectedMemberCount = $ExpectedMemberCount
    }
}

$partitionManifest = @()

function Get-AbsolutePath {
    param([Parameter(Mandatory)][string] $Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
}

function Ensure-ParentDirectory {
    param([Parameter(Mandatory)][string] $Path)

    $parent = [System.IO.Path]::GetDirectoryName($Path)
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        $null = New-Item -ItemType Directory -Path $parent -Force
    }
}

function ConvertFrom-MarkdownTaskCell {
    param([AllowNull()][string] $Value)

    if ($null -eq $Value) {
        return ''
    }

    $cell = $Value.Trim().Replace('\|', '|')
    if ($cell.Length -ge 2 -and $cell.StartsWith('`') -and $cell.EndsWith('`')) {
        $cell = $cell.Substring(1, $cell.Length - 2).Trim()
    }
    return $cell
}

function Split-MarkdownTaskRow {
    param([Parameter(Mandatory)][string] $Line)

    $trimmed = $Line.Trim()
    if (-not ($trimmed.StartsWith('|') -and $trimmed.EndsWith('|'))) {
        throw 'Task table row must start and end with a pipe.'
    }

    $inner = $trimmed.Substring(1, $trimmed.Length - 2)
    return @([regex]::Split($inner, '(?<!\\)\|') | ForEach-Object { ConvertFrom-MarkdownTaskCell -Value $_ })
}

function Get-NormalizedTaskHeader {
    param([Parameter(Mandatory)][string] $Value)

    return (($Value -replace '\s', '').ToLowerInvariant())
}

function Get-TaskColumnIndex {
    param(
        [Parameter(Mandatory)][string[]] $Headers,
        [Parameter(Mandatory)][string[]] $Aliases,
        [Parameter(Mandatory)][string] $DisplayName
    )

    foreach ($alias in $Aliases) {
        $index = [Array]::IndexOf($Headers, (Get-NormalizedTaskHeader -Value $alias))
        if ($index -ge 0) {
            return $index
        }
    }
    throw "Task table is missing required column: $DisplayName"
}

function Import-PartitionTaskTable {
    param([Parameter(Mandatory)][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Task table does not exist: $Path"
    }

    $lines = @(Get-Content -LiteralPath $Path)
    $headerLine = -1
    $headers = @()
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $candidate = [string]$lines[$index]
        if (-not ($candidate.Trim().StartsWith('|') -and $candidate.Trim().EndsWith('|'))) {
            continue
        }
        $candidateHeaders = @(Split-MarkdownTaskRow -Line $candidate | ForEach-Object { Get-NormalizedTaskHeader -Value $_ })
        if ($candidateHeaders -contains 'taskid' -and $candidateHeaders -contains 'partitionid') {
            $headerLine = $index
            $headers = $candidateHeaders
            break
        }
    }
    if ($headerLine -lt 0 -or $headerLine + 1 -ge $lines.Count) {
        throw 'Task table header was not found.'
    }

    $separator = @(Split-MarkdownTaskRow -Line ([string]$lines[$headerLine + 1]))
    if ($separator.Count -ne $headers.Count -or @($separator | Where-Object { $_ -notmatch '^:?-{3,}:?$' }).Count -gt 0) {
        throw 'Task table header must be followed by a Markdown separator row.'
    }

    $taskIdIndex = Get-TaskColumnIndex -Headers $headers -Aliases @('taskId') -DisplayName 'taskId'
    $partitionIdIndex = Get-TaskColumnIndex -Headers $headers -Aliases @('partitionId') -DisplayName 'partitionId'
    $memberCountIndex = Get-TaskColumnIndex -Headers $headers -Aliases @('成员数', 'expectedMemberCount', 'memberCount') -DisplayName 'expectedMemberCount'
    $inputReportIndex = Get-TaskColumnIndex -Headers $headers -Aliases @('输入报告', 'inputReport') -DisplayName 'inputReport'
    $promptIndex = Get-TaskColumnIndex -Headers $headers -Aliases @('专属prompt', 'prompt') -DisplayName 'prompt'
    $outputReportIndex = Get-TaskColumnIndex -Headers $headers -Aliases @('outputReport', '输出报告') -DisplayName 'outputReport'

    $definitions = [System.Collections.Generic.List[object]]::new()
    $taskIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $partitionIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    for ($lineIndex = $headerLine + 2; $lineIndex -lt $lines.Count; $lineIndex++) {
        $line = [string]$lines[$lineIndex]
        if (-not ($line.Trim().StartsWith('|') -and $line.Trim().EndsWith('|'))) {
            break
        }

        $cells = @(Split-MarkdownTaskRow -Line $line)
        if ($cells.Count -ne $headers.Count) {
            throw "Task table row $($lineIndex + 1) has $($cells.Count) cells; expected $($headers.Count)."
        }

        $taskId = $cells[$taskIdIndex].Trim()
        $partitionId = $cells[$partitionIdIndex].Trim()
        $memberCountText = $cells[$memberCountIndex].Trim()
        $inputReport = $cells[$inputReportIndex].Trim()
        $prompt = $cells[$promptIndex].Trim()
        $outputReport = $cells[$outputReportIndex].Trim()
        if ([string]::IsNullOrWhiteSpace($taskId) -or [string]::IsNullOrWhiteSpace($partitionId) -or
            [string]::IsNullOrWhiteSpace($inputReport) -or [string]::IsNullOrWhiteSpace($prompt) -or
            [string]::IsNullOrWhiteSpace($outputReport)) {
            throw "Task table row $($lineIndex + 1) contains an empty required value."
        }
        $memberCount = 0
        if (-not [int]::TryParse($memberCountText, [ref]$memberCount) -or $memberCount -lt 0) {
            throw "Task table row $($lineIndex + 1) has an invalid member count: $memberCountText"
        }
        if (-not $taskIds.Add($taskId)) {
            throw "Task table contains duplicate taskId: $taskId"
        }
        if (-not $partitionIds.Add($partitionId)) {
            throw "Task table contains duplicate partitionId: $partitionId"
        }

        $absoluteReportPath = Get-AbsolutePath -Path $inputReport
        $absolutePromptPath = Get-AbsolutePath -Path $prompt
        $absoluteOutputReportPath = Get-AbsolutePath -Path $outputReport
        if (-not (Test-Path -LiteralPath $absoluteReportPath -PathType Leaf)) {
            throw "Task table input report does not exist for $partitionId`: $absoluteReportPath"
        }
        if (-not (Test-Path -LiteralPath $absolutePromptPath -PathType Leaf)) {
            throw "Task table prompt does not exist for $partitionId`: $absolutePromptPath"
        }

        $definitions.Add((New-PartitionDefinition `
                    -Id $partitionId `
                    -FileName ([System.IO.Path]::GetFileName($absoluteReportPath)) `
                    -ExpectedMemberCount $memberCount `
                    -TaskId $taskId `
                    -ReportPath $absoluteReportPath `
                    -PromptPath $absolutePromptPath `
                    -OutputReportPath $absoluteOutputReportPath `
                    -Order $definitions.Count))
    }

    if ($definitions.Count -eq 0) {
        throw 'Task table contains no task rows.'
    }
    return @($definitions)
}

function Get-TaskSetContext {
    param([Parameter(Mandatory)][string] $Name)

    if ($Name -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$') {
        throw "TaskSetName must match ^[A-Za-z0-9][A-Za-z0-9._-]{0,63}`$: $Name"
    }

    $directory = [System.IO.Path]::GetFullPath((Join-Path $taskRoot $Name))
    [pscustomobject]@{
        name = $Name
        directory = $directory
        taskTablePath = Join-Path $directory 'task-table.md'
        statePath = Join-Path $directory 'task-state.json'
        lockPath = Join-Path $directory 'task.lock'
        taskTableSourcePath = $null
        taskTableSha256 = $null
    }
}

function Get-TaskTableHash {
    param([Parameter(Mandatory)][string] $Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Copy-TaskTableAtomically {
    param(
        [Parameter(Mandatory)][string] $Source,
        [Parameter(Mandatory)][string] $Destination
    )

    Ensure-ParentDirectory -Path $Destination
    $temporaryPath = "$Destination.$([guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::Copy($Source, $temporaryPath, $true)
        [System.IO.File]::Move($temporaryPath, $Destination, $true)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
        }
    }
}

function Get-ManifestPartition {
    param([Parameter(Mandatory)][string] $Id)

    $partition = @($partitionManifest | Where-Object { $_.id -eq $Id }) | Select-Object -First 1
    if ($null -eq $partition) {
        throw "Unknown partition id: $Id"
    }

    return $partition
}

function Get-ReportInfo {
    param(
        [Parameter(Mandatory)] $Partition
    )

    $reportPath = [string]$Partition.reportPath
    if ([string]::IsNullOrWhiteSpace($reportPath)) {
        throw "Task table report path is empty for $($Partition.id)"
    }
    if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
        throw "Report does not exist: $reportPath"
    }

    $reportLines = @(Get-Content -LiteralPath $reportPath)
    $memberRows = @(
        $reportLines |
            Where-Object { $_ -match '^\|\s*\d+\s*\|\s*(field|property)\s*\|' }
    )
    $memberCount = $memberRows.Count
    $fieldCount = @($memberRows | Where-Object { $_ -match '^\|\s*\d+\s*\|\s*field\s*\|' }).Count
    $propertyCount = @($memberRows | Where-Object { $_ -match '^\|\s*\d+\s*\|\s*property\s*\|' }).Count

    if ($memberCount -ne $Partition.expectedMemberCount) {
        throw "Report member count mismatch for $($Partition.id): expected $($Partition.expectedMemberCount), actual $memberCount"
    }

    $idRows = @($reportLines | Where-Object { $_ -match '(?i)(?:^|[\\|/])\s*ID\.cs(?:\||\s|$)' })
    if ($idRows.Count -gt 0) {
        throw "Report contains ID-class source rows for $($Partition.id): $($idRows.Count)"
    }

    [pscustomobject] @{
        id = $Partition.id
        taskId = if ($null -ne $Partition.PSObject.Properties['taskId']) { $Partition.taskId } else { $null }
        order = if ($null -ne $Partition.PSObject.Properties['order']) { $Partition.order } else { 0 }
        reportFileName = $Partition.reportFileName
        reportPath = [System.IO.Path]::GetFullPath($reportPath)
        promptPath = if ($null -ne $Partition.PSObject.Properties['promptPath']) { $Partition.promptPath } else { $null }
        outputReportPath = if ($null -ne $Partition.PSObject.Properties['outputReportPath']) { $Partition.outputReportPath } else { $null }
        expectedMemberCount = $Partition.expectedMemberCount
        observedMemberCount = $memberCount
        fieldCount = $fieldCount
        propertyCount = $propertyCount
    }
}

function Get-AllReportInfo {
    param()

    $infos = [System.Collections.Generic.List[object]]::new()
    foreach ($partition in $partitionManifest) {
        $infos.Add((Get-ReportInfo -Partition $partition))
    }

    return @($infos)
}

function New-SessionState {
    param()

    $records = [System.Collections.Generic.List[object]]::new()
    foreach ($partition in $partitionManifest) {
        $records.Add([pscustomobject]@{
                id = $partition.id
                taskId = if ($null -ne $partition.PSObject.Properties['taskId']) { $partition.taskId } else { $null }
                order = if ($null -ne $partition.PSObject.Properties['order']) { $partition.order } else { 0 }
                reportFileName = $partition.reportFileName
                reportPath = [System.IO.Path]::GetFullPath([string]$partition.reportPath)
                promptPath = if ($null -ne $partition.PSObject.Properties['promptPath']) { $partition.promptPath } else { $null }
                outputReportPath = if ($null -ne $partition.PSObject.Properties['outputReportPath']) { $partition.outputReportPath } else { $null }
                expectedMemberCount = $partition.expectedMemberCount
                observedMemberCount = $null
                status = 'available'
                claimMode = $null
                sessionId = $null
                previousSessionId = $null
                handoffId = $null
                handedOffAtUtc = $null
                ownerProcessId = $null
                ownerProcessStartTime = $null
                ownerKind = $null
                leaseName = $null
                leaseFilePath = $null
                leaseStopFilePath = $null
                leaseHostProcessId = $null
                leaseHostProcessStartTime = $null
                leaseState = $null
                leaseHeartbeatSeconds = $null
                lastHeartbeatUtc = $null
                leaseExpiresAtUtc = $null
                releaseReason = $null
                codexSessionId = $null
                claimedAtUtc = $null
                startedAtUtc = $null
                completedAtUtc = $null
                abandonedAtUtc = $null
                exitCode = $null
                commandPath = $null
                commandArguments = @()
                stdout = $null
                stderr = $null
                error = $null
            })
    }

    $state = [pscustomobject]@{
        schemaVersion = 4
        checkoutRoot = $repoRoot
        updatedAtUtc = [DateTime]::UtcNow.ToString('o')
        taskSetName = $activeTaskSetContext.name
        taskTablePath = $activeTaskSetContext.taskTablePath
        taskTableSourcePath = $activeTaskSetContext.taskTableSourcePath
        taskTableSha256 = $activeTaskSetContext.taskTableSha256
        initializedAtUtc = [DateTime]::UtcNow.ToString('o')
        partitions = @($records)
    }
    return $state
}

function Load-SessionState {
    param([Parameter(Mandatory)][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Task set is not initialized: $($activeTaskSetContext.name)"
    }

    $raw = Get-Content -Raw -LiteralPath $Path
    if ([string]::IsNullOrWhiteSpace($raw)) {
        throw "Session state file is empty: $Path"
    }

    $state = $raw | ConvertFrom-Json -DateKind String
    if ($null -eq $state.partitions) {
        throw "Session state has no partitions array: $Path"
    }

    $expectedIds = @($partitionManifest | ForEach-Object { $_.id })
    $actualIds = @($state.partitions | ForEach-Object { $_.id })
    if ($actualIds.Count -ne $expectedIds.Count -or (($actualIds | Sort-Object) -join ',') -ne (($expectedIds | Sort-Object) -join ',')) {
        throw "Session state partition set does not match the active task table: $Path"
    }

    if ($null -eq $state.PSObject.Properties['schemaVersion'] -or ([int]$state.schemaVersion -ne 3 -and [int]$state.schemaVersion -ne 4)) {
        throw "Task state must use schemaVersion 3 or 4: $Path"
    }
    foreach ($record in @($state.partitions)) {
        if ($null -eq $record.PSObject.Properties['claimMode']) {
            $record | Add-Member -MemberType NoteProperty -Name claimMode -Value 'managed'
        }
        if ($null -eq $record.PSObject.Properties['previousSessionId']) {
            $record | Add-Member -MemberType NoteProperty -Name previousSessionId -Value $null
        }
        if ($null -eq $record.PSObject.Properties['handoffId']) {
            $record | Add-Member -MemberType NoteProperty -Name handoffId -Value $null
        }
        if ($null -eq $record.PSObject.Properties['handedOffAtUtc']) {
            $record | Add-Member -MemberType NoteProperty -Name handedOffAtUtc -Value $null
        }
        foreach ($property in @(
                @{ Name = 'ownerKind'; Value = $null },
                @{ Name = 'leaseName'; Value = $null },
                @{ Name = 'leaseFilePath'; Value = $null },
                @{ Name = 'leaseStopFilePath'; Value = $null },
                @{ Name = 'leaseHostProcessId'; Value = $null },
                @{ Name = 'leaseHostProcessStartTime'; Value = $null },
                @{ Name = 'leaseState'; Value = $null },
                @{ Name = 'leaseHeartbeatSeconds'; Value = $null },
                @{ Name = 'lastHeartbeatUtc'; Value = $null },
                @{ Name = 'leaseExpiresAtUtc'; Value = $null },
                @{ Name = 'releaseReason'; Value = $null }
                @{ Name = 'codexSessionId'; Value = $null }
            )) {
            if ($null -eq $record.PSObject.Properties[$property.Name]) {
                $record | Add-Member -MemberType NoteProperty -Name $property.Name -Value $property.Value
            }
        }
    }
    if ($null -eq $state.PSObject.Properties['taskSetName'] -or [string]$state.taskSetName -ne $activeTaskSetContext.name) {
        throw "Task set state name does not match directory: $Path"
    }
    if ($null -eq $state.PSObject.Properties['taskTableSha256'] -or [string]$state.taskTableSha256 -ne $activeTaskSetContext.taskTableSha256) {
        throw "Task table hash does not match task state: $Path"
    }

    return $state
}

function Save-SessionState {
    param(
        [Parameter(Mandatory)] $State,
        [Parameter(Mandatory)][string] $Path
    )

    Ensure-ParentDirectory -Path $Path
    $State.updatedAtUtc = [DateTime]::UtcNow.ToString('o')
    $State.schemaVersion = 4
    $temporaryPath = "$Path.$([guid]::NewGuid().ToString('N')).tmp"
    $json = $State | ConvertTo-Json -Depth 20
    $utf8 = [System.Text.UTF8Encoding]::new($false)
    try {
        [System.IO.File]::WriteAllText($temporaryPath, $json, $utf8)
        [System.IO.File]::Move($temporaryPath, $Path, $true)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
        }
    }
}

function Get-PartitionRecord {
    param(
        [Parameter(Mandatory)] $State,
        [Parameter(Mandatory)][string] $Id
    )

    $record = @($State.partitions | Where-Object { $_.id -eq $Id }) | Select-Object -First 1
    if ($null -eq $record) {
        throw "Session state has no partition record: $Id"
    }

    return $record
}

function Test-OwnerProcessAlive {
    param([Parameter(Mandatory)] $Record)

    if ($null -eq $Record.ownerProcessId) {
        return $false
    }

    try {
        $process = Get-Process -Id ([int]$Record.ownerProcessId) -ErrorAction Stop
        if ([string]::IsNullOrWhiteSpace([string]$Record.ownerProcessStartTime)) {
            return $true
        }

        $actualStart = $process.StartTime.ToUniversalTime()
        $recordedStart = [DateTime]::Parse([string]$Record.ownerProcessStartTime).ToUniversalTime()
        return [Math]::Abs(($actualStart - $recordedStart).TotalSeconds) -lt 2
    }
    catch {
        return $false
    }
}

function Get-CodexOwnerProcessInfo {
    param([AllowNull()][Nullable[int]] $RequestedProcessId)

    $effectiveId = $RequestedProcessId
    if ($null -eq $effectiveId -or $effectiveId -le 0) {
        $environmentId = 0
        if ([int]::TryParse([string]$env:VERSION4_CODEX_SESSION_PID, [ref]$environmentId) -and $environmentId -gt 0) {
            $effectiveId = $environmentId
        }
    }
    if (($null -eq $effectiveId -or $effectiveId -le 0) -and -not [string]::IsNullOrWhiteSpace([string]$env:CODEX_SESSION_ID)) {
        try {
            $candidateId = $PID
            for ($depth = 0; $depth -lt 16; $depth++) {
                $processInfo = Get-CimInstance Win32_Process -Filter "ProcessId=$candidateId" -ErrorAction Stop
                if ($null -eq $processInfo) { break }
                if ([string]$processInfo.Name -match '(?i)^codex(?:\.exe)?$') {
                    $effectiveId = [int]$processInfo.ProcessId
                    break
                }
                if ($null -eq $processInfo.ParentProcessId -or [int]$processInfo.ParentProcessId -le 0 -or [int]$processInfo.ParentProcessId -eq $candidateId) { break }
                $candidateId = [int]$processInfo.ParentProcessId
            }
        }
        catch {
            $effectiveId = $null
        }
    }
    if ($null -eq $effectiveId -or $effectiveId -le 0) {
        return [pscustomobject]@{ processId = $null; processStartTime = $null }
    }
    try {
        return [pscustomobject]@{
            processId = [int]$effectiveId
            processStartTime = Get-Version4ProcessStartTime -ProcessId ([int]$effectiveId)
        }
    }
    catch {
        throw "Owner process is not available: $effectiveId"
    }
}

function Get-RecordLeaseStatus {
    param([Parameter(Mandatory)] $Record)

    if ([string]::IsNullOrWhiteSpace([string]$Record.leaseName) -or
        [string]::IsNullOrWhiteSpace([string]$Record.leaseFilePath) -or
        [string]::IsNullOrWhiteSpace([string]$Record.sessionId)) {
        return [pscustomobject]@{ active = $false; reason = 'legacy-record'; lease = $null }
    }
    return Get-Version4LeaseStatus -LeaseName ([string]$Record.leaseName) -LeaseFilePath ([string]$Record.leaseFilePath) -SessionToken ([string]$Record.sessionId)
}

function Sync-RecordLease {
    param(
        [Parameter(Mandatory)] $Record,
        [Parameter(Mandatory)] $LeaseStatus
    )

    if ($null -ne $LeaseStatus.lease) {
        $Record.lastHeartbeatUtc = $LeaseStatus.lease.lastHeartbeatUtc
        $Record.leaseExpiresAtUtc = $LeaseStatus.lease.leaseExpiresAtUtc
        $Record.leaseHostProcessId = $LeaseStatus.lease.hostProcessId
        $Record.leaseHostProcessStartTime = $LeaseStatus.lease.hostProcessStartTime
        $Record.ownerProcessId = $LeaseStatus.lease.hostProcessId
        $Record.ownerProcessStartTime = $LeaseStatus.lease.hostProcessStartTime
    }
}

function Stop-RecordLease {
    param(
        [Parameter(Mandatory)] $Record,
        [Parameter(Mandatory)][string] $Reason
    )

    if (-not [string]::IsNullOrWhiteSpace([string]$Record.leaseFilePath) -and
        -not [string]::IsNullOrWhiteSpace([string]$Record.leaseStopFilePath) -and
        -not [string]::IsNullOrWhiteSpace([string]$Record.sessionId)) {
        Stop-Version4SessionLeaseHost -LeaseFilePath ([string]$Record.leaseFilePath) -StopFilePath ([string]$Record.leaseStopFilePath) -SessionToken ([string]$Record.sessionId)
    }
    $Record.leaseState = 'released'
    $Record.releaseReason = $Reason
    $Record.leaseExpiresAtUtc = [DateTime]::UtcNow.ToString('o')
}

function Start-RecordLease {
    param(
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $SessionToken
    )

    $leaseName = Get-Version4LeaseName -TaskSetName "authoritative|$($activeTaskSetContext.name)" -PartitionId $PartitionId
    $leaseFilePath = Join-Path $activeTaskSetContext.directory "session-lease-$PartitionId.json"
    $owner = Get-CodexOwnerProcessInfo -RequestedProcessId $OwnerProcessId
    $heartbeatSeconds = 15
    $codexSessionId = [string]$env:CODEX_SESSION_ID
    $started = Start-Version4SessionLeaseHost -LeaseName $leaseName -LeaseFilePath $leaseFilePath -SessionToken $SessionToken -CodexSessionId $(if ([string]::IsNullOrWhiteSpace($codexSessionId)) { $null } else { $codexSessionId }) -HeartbeatSeconds $heartbeatSeconds -OwnerProcessId $owner.processId -OwnerProcessStartTime $owner.processStartTime
    $started | Add-Member -MemberType NoteProperty -Name heartbeatSeconds -Value $heartbeatSeconds
    $started | Add-Member -MemberType NoteProperty -Name codexSessionId -Value $(if ([string]::IsNullOrWhiteSpace($codexSessionId)) { $null } else { $codexSessionId })
    return $started
}

function Assert-ManualSessionLease {
    param(
        [Parameter(Mandatory)] $Record,
        [Parameter(Mandatory)][string] $ClaimSessionId
    )

    if ($record.status -ne 'running') {
        throw "Partition is not running: $($Record.id)"
    }
    if ([string]$record.sessionId -ne $ClaimSessionId) {
        throw "Session id does not own partition: $($Record.id)"
    }
    if ($record.claimMode -ne 'manual') {
        throw "Partition was not claimed through Claim: $($Record.id)"
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$record.codexSessionId) -and
        -not [string]::IsNullOrWhiteSpace([string]$env:CODEX_SESSION_ID) -and
        [string]$record.codexSessionId -ne [string]$env:CODEX_SESSION_ID) {
        throw "Owner Codex session does not own partition: $($Record.id)"
    }
    $leaseStatus = Get-RecordLeaseStatus -Record $Record
    if ($leaseStatus.reason -eq 'legacy-record') {
        return $null
    }
    if (-not $leaseStatus.active) {
        throw "Session lease is not active for partition: $($Record.id) ($($leaseStatus.reason))"
    }
    Sync-RecordLease -Record $Record -LeaseStatus $leaseStatus
    return $leaseStatus
}

function Reconcile-AbandonedRecords {
    param([Parameter(Mandatory)] $State)

    $changed = $false
    foreach ($record in @($State.partitions)) {
        if ($record.status -ne 'running') { continue }
        if (-not [string]::IsNullOrWhiteSpace([string]$record.leaseName)) {
            $leaseStatus = Get-RecordLeaseStatus -Record $record
            if ($leaseStatus.active) {
                Sync-RecordLease -Record $record -LeaseStatus $leaseStatus
                continue
            }
            Stop-RecordLease -Record $record -Reason $leaseStatus.reason
            $record.status = 'abandoned'
            $record.abandonedAtUtc = [DateTime]::UtcNow.ToString('o')
            $record.error = "Session lease is no longer active: $($leaseStatus.reason)."
            $changed = $true
            continue
        }
        if ($record.claimMode -ne 'manual' -and -not (Test-OwnerProcessAlive -Record $record)) {
            $record.status = 'abandoned'
            $record.abandonedAtUtc = [DateTime]::UtcNow.ToString('o')
            $record.error = "Owner process $($record.ownerProcessId) is no longer alive."
            $changed = $true
        }
    }

    return $changed
}

function Acquire-ExclusiveLock {
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][int] $WaitSeconds,
        [Parameter(Mandatory)][string] $SessionId
    )

    Ensure-ParentDirectory -Path $Path
    $deadline = [DateTime]::UtcNow.AddSeconds($WaitSeconds)
    $stream = $null
    while ($true) {
        try {
            $stream = [System.IO.FileStream]::new(
                $Path,
                [System.IO.FileMode]::OpenOrCreate,
                [System.IO.FileAccess]::ReadWrite,
                [System.IO.FileShare]::None
            )
            $metadata = [pscustomobject]@{
                sessionId = $SessionId
                processId = $PID
                acquiredAtUtc = [DateTime]::UtcNow.ToString('o')
            } | ConvertTo-Json -Compress
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($metadata)
            $stream.SetLength(0)
            $stream.Position = 0
            $null = $stream.Write($bytes, 0, $bytes.Length)
            $stream.Flush($true)
            return $stream
        }
        catch [System.IO.IOException] {
            if ($null -ne $stream) {
                $stream.Dispose()
                $stream = $null
            }
            if ([DateTime]::UtcNow -ge $deadline) {
                return $null
            }
            Start-Sleep -Milliseconds 100
        }
        catch {
            if ($null -ne $stream) {
                $stream.Dispose()
            }
            throw
        }
    }
}

function Get-OutputExcerpt {
    param([AllowNull()][string] $Value)

    if ($null -eq $Value) { return $null }
    $limit = 16384
    if ($Value.Length -le $limit) { return $Value }
    return $Value.Substring(0, $limit) + "`n[output truncated]"
}

function Invoke-ExternalCommand {
    param(
        [Parameter(Mandatory)][string] $Executable,
        [AllowNull()][string[]] $Arguments,
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $ReportPath,
        [Parameter(Mandatory)][string] $SessionId
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $Executable
    $startInfo.WorkingDirectory = $repoRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    if ($null -ne $Arguments) {
        foreach ($argument in $Arguments) {
            $startInfo.ArgumentList.Add([string]$argument)
        }
    }
    $startInfo.Environment['VERSION4_PARTITION_ID'] = $PartitionId
    $startInfo.Environment['VERSION4_PARTITION_REPORT_PATH'] = $ReportPath
    $startInfo.Environment['VERSION4_PARTITION_SESSION_ID'] = $SessionId

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw "Could not start external command: $Executable"
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        return [pscustomobject]@{
            exitCode = $process.ExitCode
            stdout = Get-OutputExcerpt -Value $stdout
            stderr = Get-OutputExcerpt -Value $stderr
        }
    }
    finally {
        $process.Dispose()
    }
}

function Resolve-CommandArguments {
    param(
        [AllowNull()][string[]] $ArgumentList,
        [AllowNull()][string] $ArgumentListJson
    )

    if (-not [string]::IsNullOrWhiteSpace($ArgumentListJson)) {
        if ($null -ne $ArgumentList -and @($ArgumentList).Count -gt 0) {
            throw 'Use either CommandArgumentList or CommandArgumentListJson, not both.'
        }

        try {
            $parsed = ConvertFrom-Json -InputObject $ArgumentListJson -ErrorAction Stop
        }
        catch {
            throw "CommandArgumentListJson must be a JSON array of strings: $($_.Exception.Message)"
        }

        $jsonText = $ArgumentListJson.Trim()
        if ($null -eq $parsed) {
            if ($jsonText -eq '[]') {
                return @()
            }
            throw 'CommandArgumentListJson must be a JSON array of strings.'
        }

        if ($parsed -is [string]) {
            if (-not ($jsonText.StartsWith('[') -and $jsonText.EndsWith(']'))) {
                throw 'CommandArgumentListJson must be a JSON array of strings.'
            }
            $parsedValues = @($parsed)
        }
        elseif ($parsed -is [System.Collections.IEnumerable]) {
            $parsedValues = @($parsed)
        }
        else {
            throw 'CommandArgumentListJson must be a JSON array of strings.'
        }

        $resolved = [System.Collections.Generic.List[string]]::new()
        foreach ($value in $parsedValues) {
            if ($value -isnot [string]) {
                throw 'CommandArgumentListJson must contain only strings.'
            }
            $resolved.Add($value)
        }
        return @($resolved)
    }

    return @($ArgumentList)
}

function Set-ReportRecord {
    param(
        [Parameter(Mandatory)] $Record,
        [Parameter(Mandatory)] $ReportInfo
    )

    $Record.reportPath = $ReportInfo.reportPath
    $Record.observedMemberCount = $ReportInfo.observedMemberCount
}

function Reset-RecordToAvailable {
    param([Parameter(Mandatory)] $Record)

    $Record.status = 'available'
    $Record.claimMode = $null
    $Record.sessionId = $null
    $Record.previousSessionId = $null
    $Record.handoffId = $null
    $Record.handedOffAtUtc = $null
    $Record.ownerProcessId = $null
    $Record.ownerProcessStartTime = $null
    $Record.ownerKind = $null
    $Record.leaseName = $null
    $Record.leaseFilePath = $null
    $Record.leaseStopFilePath = $null
    $Record.leaseHostProcessId = $null
    $Record.leaseHostProcessStartTime = $null
    $Record.leaseState = $null
    $Record.leaseHeartbeatSeconds = $null
    $Record.lastHeartbeatUtc = $null
    $Record.leaseExpiresAtUtc = $null
    $Record.releaseReason = $null
    $Record.codexSessionId = $null
    $Record.claimedAtUtc = $null
    $Record.startedAtUtc = $null
    $Record.completedAtUtc = $null
    $Record.abandonedAtUtc = $null
    $Record.exitCode = $null
    $Record.commandPath = $null
    $Record.commandArguments = @()
    $Record.stdout = $null
    $Record.stderr = $null
    $Record.error = $null
}

function Set-ClaimRecord {
    param(
        [Parameter(Mandatory)] $Record,
        [Parameter(Mandatory)] $ReportInfo,
        [Parameter(Mandatory)][string] $SessionId,
        [Parameter(Mandatory)][ValidateSet('manual', 'managed')][string] $ClaimMode,
        [AllowNull()][string] $CommandPath,
        [AllowNull()][string[]] $CommandArguments,
        [AllowNull()] $LeaseInfo
    )

    $now = [DateTime]::UtcNow.ToString('o')
    $Record.status = 'running'
    $Record.claimMode = $ClaimMode
    $Record.sessionId = $SessionId
    $Record.previousSessionId = $null
    $Record.handoffId = $null
    $Record.handedOffAtUtc = $null
    $Record.reportPath = $ReportInfo.reportPath
    $Record.observedMemberCount = $ReportInfo.observedMemberCount
    $Record.claimedAtUtc = $now
    $Record.startedAtUtc = $now
    $Record.completedAtUtc = $null
    $Record.abandonedAtUtc = $null
    $Record.exitCode = $null
    $Record.commandPath = $CommandPath
    $Record.commandArguments = if ($null -eq $CommandArguments) { @() } else { @($CommandArguments) }
    $Record.stdout = $null
    $Record.stderr = $null
    $Record.error = $null

    $Record.ownerKind = if ($null -ne $LeaseInfo) { 'session-lease-host' } elseif ($ClaimMode -eq 'managed') { 'managed-worker' } else { $null }
    $Record.leaseName = if ($null -ne $LeaseInfo) { $LeaseInfo.leaseName } else { $null }
    $Record.leaseFilePath = if ($null -ne $LeaseInfo) { $LeaseInfo.leaseFilePath } else { $null }
    $Record.leaseStopFilePath = if ($null -ne $LeaseInfo) { $LeaseInfo.stopFilePath } else { $null }
    $Record.leaseHostProcessId = if ($null -ne $LeaseInfo) { $LeaseInfo.hostProcessId } else { $null }
    $Record.leaseHostProcessStartTime = if ($null -ne $LeaseInfo) { $LeaseInfo.hostProcessStartTime } else { $null }
    $Record.leaseState = if ($null -ne $LeaseInfo) { 'active' } else { $null }
    $Record.leaseHeartbeatSeconds = if ($null -ne $LeaseInfo) { $LeaseInfo.heartbeatSeconds } else { $null }
    $Record.lastHeartbeatUtc = if ($null -ne $LeaseInfo) { $LeaseInfo.lastHeartbeatUtc } else { $null }
    $Record.leaseExpiresAtUtc = if ($null -ne $LeaseInfo) { $LeaseInfo.leaseExpiresAtUtc } else { $null }
    $Record.releaseReason = $null
    $Record.codexSessionId = if ($null -ne $LeaseInfo) { $LeaseInfo.codexSessionId } else { $null }

    if ($null -ne $LeaseInfo) {
        $Record.ownerProcessId = $LeaseInfo.hostProcessId
        $Record.ownerProcessStartTime = $LeaseInfo.hostProcessStartTime
    }
    elseif ($ClaimMode -eq 'managed') {
        $ownerProcess = Get-Process -Id $PID
        $Record.ownerProcessId = $PID
        $Record.ownerProcessStartTime = $ownerProcess.StartTime.ToUniversalTime().ToString('o')
    }
    else {
        $Record.ownerProcessId = $null
        $Record.ownerProcessStartTime = $null
    }
}

function New-ClaimedResult {
    param(
        [Parameter(Mandatory)] $ReportInfo,
        [Parameter(Mandatory)][string] $SessionId
    )

    [pscustomobject]@{
        status = 'claimed'
        taskSetName = $activeTaskSetContext.name
        taskId = $ReportInfo.taskId
        partition = $ReportInfo.id
        report = $ReportInfo.reportPath
        prompt = $ReportInfo.promptPath
        outputReport = $ReportInfo.outputReportPath
        sessionId = $SessionId
        claimMode = 'manual'
        lockReleased = $true
        expectedMemberCount = $ReportInfo.expectedMemberCount
        observedMemberCount = $ReportInfo.observedMemberCount
    }
}

function Assert-CodexSessionHasNoOtherClaim {
    param(
        [Parameter(Mandatory)] $State,
        [Parameter(Mandatory)][string] $PartitionId
    )

    $codexSessionId = [string]$env:CODEX_SESSION_ID
    if ([string]::IsNullOrWhiteSpace($codexSessionId)) {
        return
    }

    foreach ($record in @($State.partitions)) {
        if ([string]$record.id -eq $PartitionId -or $record.status -ne 'running' -or
            $record.claimMode -ne 'manual') {
            continue
        }
        if ([string]$record.codexSessionId -eq $codexSessionId) {
            throw "Codex session already owns partition: $($record.id)"
        }
    }
}

function New-CleanupResult {
    param(
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $PreviousStatus
    )

    [pscustomobject]@{
        status = 'cleaned'
        partition = $PartitionId
        previousStatus = $PreviousStatus
        lockReleased = $true
    }
}

function Get-ClaimablePartitionId {
    param(
        [Parameter(Mandatory)] $State,
        [AllowNull()][string] $RequestedPartitionId,
        [Parameter(Mandatory)][switch] $SelectNext,
        [Parameter(Mandatory)][switch] $Retry
    )

    if (-not $SelectNext) {
        return $RequestedPartitionId
    }

    foreach ($manifestPartition in $partitionManifest) {
        $candidate = Get-PartitionRecord -State $State -Id $manifestPartition.id
        if ($candidate.status -eq 'available' -or ($Retry -and ($candidate.status -eq 'failed' -or $candidate.status -eq 'abandoned'))) {
            return $manifestPartition.id
        }
    }

    return $null
}

function Complete-ManualRecord {
    param(
        [Parameter(Mandatory)] $State,
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $ClaimSessionId,
        [Parameter(Mandatory)][ValidateSet('completed', 'failed', 'abandoned')][string] $FinalStatus,
        [AllowNull()][string] $FailureMessage,
        [AllowNull()][Nullable[int]] $FinalExitCode
    )

    $record = Get-PartitionRecord -State $State -Id $PartitionId
    if ($record.status -ne 'running') {
        throw "Partition is not running: $PartitionId"
    }
    if ([string]$record.sessionId -ne $ClaimSessionId) {
        throw "Session id does not own partition: $PartitionId"
    }
    if ($record.claimMode -ne 'manual') {
        throw "Partition was not claimed through Claim: $PartitionId"
    }

    $record.status = $FinalStatus
    $record.completedAtUtc = if ($FinalStatus -eq 'abandoned') { $null } else { [DateTime]::UtcNow.ToString('o') }
    $record.abandonedAtUtc = if ($FinalStatus -eq 'abandoned') { [DateTime]::UtcNow.ToString('o') } else { $null }
    $record.exitCode = $FinalExitCode
    if ($FinalStatus -eq 'failed') {
        $record.error = if ([string]::IsNullOrWhiteSpace($FailureMessage)) { 'Manual session reported failure.' } else { $FailureMessage }
    }
    elseif ($FinalStatus -eq 'abandoned') {
        $record.error = if ([string]::IsNullOrWhiteSpace($FailureMessage)) { 'Manual session abandoned the partition.' } else { $FailureMessage }
    }
    else {
        $record.error = $null
    }

    return $record
}

function Handoff-ManualRecord {
    param(
        [Parameter(Mandatory)] $State,
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $OldSessionId,
        [Parameter(Mandatory)][string] $HandoffId,
        [Parameter(Mandatory)][string] $NewSessionId,
        [AllowNull()] $LeaseInfo
    )

    $record = Get-PartitionRecord -State $State -Id $PartitionId
    if ($record.status -eq 'completed') {
        throw "Partition is already completed: $PartitionId"
    }
    if ($record.status -ne 'running' -and $record.status -ne 'failed' -and $record.status -ne 'abandoned') {
        throw "Partition status '$($record.status)' is not handoffable: $PartitionId"
    }
    if ([string]$record.sessionId -ne $OldSessionId) {
        throw "Session id does not own partition: $PartitionId"
    }
    if ($record.claimMode -ne 'manual') {
        throw "Partition was not claimed through Claim: $PartitionId"
    }

    $previousStatus = [string]$record.status
    $record.status = 'running'
    $record.claimMode = 'manual'
    $record.previousSessionId = $OldSessionId
    $record.sessionId = $NewSessionId
    $record.handoffId = $HandoffId
    $record.handedOffAtUtc = [DateTime]::UtcNow.ToString('o')
    $record.claimedAtUtc = $record.handedOffAtUtc
    $record.startedAtUtc = $record.handedOffAtUtc
    $record.completedAtUtc = $null
    $record.abandonedAtUtc = $null
    $record.exitCode = $null
    $record.ownerProcessId = $null
    $record.ownerProcessStartTime = $null
    $record.ownerKind = if ($null -ne $LeaseInfo) { 'session-lease-host' } else { $null }
    $record.leaseName = if ($null -ne $LeaseInfo) { $LeaseInfo.leaseName } else { $null }
    $record.leaseFilePath = if ($null -ne $LeaseInfo) { $LeaseInfo.leaseFilePath } else { $null }
    $record.leaseStopFilePath = if ($null -ne $LeaseInfo) { $LeaseInfo.stopFilePath } else { $null }
    $record.leaseHostProcessId = if ($null -ne $LeaseInfo) { $LeaseInfo.hostProcessId } else { $null }
    $record.leaseHostProcessStartTime = if ($null -ne $LeaseInfo) { $LeaseInfo.hostProcessStartTime } else { $null }
    $record.leaseState = if ($null -ne $LeaseInfo) { 'active' } else { $null }
    $record.leaseHeartbeatSeconds = if ($null -ne $LeaseInfo) { $LeaseInfo.heartbeatSeconds } else { $null }
    $record.lastHeartbeatUtc = if ($null -ne $LeaseInfo) { $LeaseInfo.lastHeartbeatUtc } else { $null }
    $record.leaseExpiresAtUtc = if ($null -ne $LeaseInfo) { $LeaseInfo.leaseExpiresAtUtc } else { $null }
    $record.releaseReason = $null
    $record.codexSessionId = if ($null -ne $LeaseInfo) { $LeaseInfo.codexSessionId } else { $null }
    $record.commandPath = $null
    $record.commandArguments = @()
    $record.stdout = $null
    $record.stderr = $null
    $record.error = $null
    if ($null -ne $LeaseInfo) {
        $record.ownerProcessId = $LeaseInfo.hostProcessId
        $record.ownerProcessStartTime = $LeaseInfo.hostProcessStartTime
    }

    [pscustomobject]@{
        previousStatus = $previousStatus
        oldSessionId = $OldSessionId
        newSessionId = $NewSessionId
    }
}

function New-PartitionListResult {
    param([Parameter(Mandatory)] $State)

    [pscustomobject]@{
        status = 'ok'
        taskSetName = if ($null -ne $State.PSObject.Properties['taskSetName']) { $State.taskSetName } else { $null }
        taskTablePath = if ($null -ne $State.PSObject.Properties['taskTablePath']) { $State.taskTablePath } else { $null }
        partitions = @($State.partitions | ForEach-Object {
                [pscustomobject]@{
                    id = $_.id
                    taskId = if ($null -ne $_.PSObject.Properties['taskId']) { $_.taskId } else { $null }
                    order = if ($null -ne $_.PSObject.Properties['order']) { $_.order } else { 0 }
                    reportFileName = $_.reportFileName
                    reportPath = $_.reportPath
                    promptPath = if ($null -ne $_.PSObject.Properties['promptPath']) { $_.promptPath } else { $null }
                    outputReportPath = if ($null -ne $_.PSObject.Properties['outputReportPath']) { $_.outputReportPath } else { $null }
                    expectedMemberCount = $_.expectedMemberCount
                    observedMemberCount = $_.observedMemberCount
                    status = $_.status
                     claimMode = $_.claimMode
                     sessionId = $_.sessionId
                     previousSessionId = $_.previousSessionId
                     handoffId = $_.handoffId
                     handedOffAtUtc = $_.handedOffAtUtc
                    ownerProcessId = $_.ownerProcessId
                    claimedAtUtc = $_.claimedAtUtc
                    startedAtUtc = $_.startedAtUtc
                    completedAtUtc = $_.completedAtUtc
                    abandonedAtUtc = $_.abandonedAtUtc
                    exitCode = $_.exitCode
                    error = $_.error
                }
            })
    }
}

function New-HandoffResult {
    param(
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $ReportPath,
        [Parameter(Mandatory)][string] $OldSessionId,
        [Parameter(Mandatory)][string] $NewSessionId,
        [Parameter(Mandatory)][string] $HandoffId,
        [Parameter(Mandatory)][string] $PreviousStatus
    )

    [pscustomobject]@{
        status = 'handed-off'
        partition = $PartitionId
        report = $ReportPath
        oldSessionId = $OldSessionId
        sessionId = $NewSessionId
        handoffId = $HandoffId
        previousStatus = $PreviousStatus
        claimMode = 'manual'
        lockReleased = $true
    }
}

function New-InvalidResult {
    param([Parameter(Mandatory)][string] $Message)

    [pscustomobject]@{
        status = 'invalid'
        error = $Message
    }
}

function New-BusyResult {
    param([Parameter(Mandatory)][string] $Path)

    [pscustomobject]@{
        status = 'busy'
        lockPath = $Path
        error = 'Another partition session currently owns the checkout lock.'
    }
}

function Initialize-PartitionTaskSet {
    param(
        [Parameter(Mandatory)] $Context,
        [Parameter(Mandatory)][string] $SourcePath
    )

    $sourceManifest = @(Import-PartitionTaskTable -Path $SourcePath)
    $sourceHash = Get-TaskTableHash -Path $SourcePath
    $script:partitionManifest = $sourceManifest
    $reportInfos = @(Get-AllReportInfo)
    $initializationLock = $null
    try {
        $initializationLock = Acquire-ExclusiveLock -Path $Context.lockPath -WaitSeconds $LockWaitSeconds -SessionId $invocationSessionId
        if ($null -eq $initializationLock) {
            return [pscustomobject]@{
                result = New-BusyResult -Path $Context.lockPath
                exitCode = $ExitBusy
            }
        }

        if (Test-Path -LiteralPath $Context.statePath -PathType Leaf) {
            if (-not (Test-Path -LiteralPath $Context.taskTablePath -PathType Leaf)) {
                throw "Task set state exists but its managed task table is missing: $($Context.directory)"
            }
            $managedHash = Get-TaskTableHash -Path $Context.taskTablePath
            if ($managedHash -ne $sourceHash) {
                throw "Task set already exists with a different task table: $($Context.name)"
            }

            $Context.taskTableSourcePath = $SourcePath
            $Context.taskTableSha256 = $managedHash
            $script:partitionManifest = @(Import-PartitionTaskTable -Path $Context.taskTablePath)
            $null = Load-SessionState -Path $Context.statePath
            return [pscustomobject]@{
                result = [pscustomobject]@{
                    status = 'already-initialized'
                    taskSetName = $Context.name
                    taskDirectory = $Context.directory
                    taskTablePath = $Context.taskTablePath
                    statePath = $Context.statePath
                    taskTableSha256 = $managedHash
                    taskCount = $script:partitionManifest.Count
                    totalExpectedMemberCount = ($script:partitionManifest | Measure-Object -Property expectedMemberCount -Sum).Sum
                    lockReleased = $true
                }
                exitCode = $ExitSuccess
            }
        }

        Copy-TaskTableAtomically -Source $SourcePath -Destination $Context.taskTablePath
        $managedHash = Get-TaskTableHash -Path $Context.taskTablePath
        $Context.taskTableSourcePath = $SourcePath
        $Context.taskTableSha256 = $managedHash
        $script:partitionManifest = @(Import-PartitionTaskTable -Path $Context.taskTablePath)
        $state = New-SessionState
        foreach ($info in $reportInfos) {
            $record = Get-PartitionRecord -State $state -Id $info.id
            Set-ReportRecord -Record $record -ReportInfo $info
        }
        Save-SessionState -State $state -Path $Context.statePath

        return [pscustomobject]@{
            result = [pscustomobject]@{
                status = 'initialized'
                taskSetName = $Context.name
                taskDirectory = $Context.directory
                taskTablePath = $Context.taskTablePath
                statePath = $Context.statePath
                taskTableSha256 = $managedHash
                taskCount = $script:partitionManifest.Count
                totalExpectedMemberCount = ($script:partitionManifest | Measure-Object -Property expectedMemberCount -Sum).Sum
                lockReleased = $true
            }
            exitCode = $ExitSuccess
        }
    }
    finally {
        if ($null -ne $initializationLock) {
            $initializationLock.Dispose()
        }
    }
}

function New-ConflictResult {
    param(
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $Reason
    )

    [pscustomobject]@{
        status = 'conflict'
        partition = $PartitionId
        error = $Reason
    }
}

function New-FailedResult {
    param(
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)][string] $SessionId,
        [AllowNull()][Nullable[int]] $ExitCode,
        [AllowNull()][string] $Stdout,
        [AllowNull()][string] $Stderr,
        [AllowNull()][string] $Error
    )

    [pscustomobject]@{
        status = 'failed'
        partition = $PartitionId
        sessionId = $SessionId
        exitCode = $ExitCode
        stdout = $Stdout
        stderr = $Stderr
        error = $Error
        lockReleased = $true
    }
}

function Finalize-ManagedRun {
    param(
        [Parameter(Mandatory)][string] $PartitionId,
        [Parameter(Mandatory)] $ReportInfo,
        [Parameter(Mandatory)][string] $RunSessionId,
        [AllowNull()] $ChildResult,
        [AllowNull()][string] $ChildError
    )

    $finalizationLock = $null
    $finalizationWaitSeconds = [Math]::Max($LockWaitSeconds, 30)
    try {
        $finalizationLock = Acquire-ExclusiveLock -Path $absoluteLockPath -WaitSeconds $finalizationWaitSeconds -SessionId $RunSessionId
        if ($null -eq $finalizationLock) {
            return [pscustomobject]@{
                result = [pscustomobject]@{
                    status = 'internal-error'
                    partition = $PartitionId
                    sessionId = $RunSessionId
                    error = 'Could not reacquire the checkout lock to finalize the managed run.'
                }
                exitCode = $ExitInternal
            }
        }

        $state = Load-SessionState -Path $absoluteStatePath
        $record = Get-PartitionRecord -State $state -Id $PartitionId
        if ($record.status -ne 'running' -or [string]$record.sessionId -ne $RunSessionId -or $record.claimMode -ne 'managed') {
            return [pscustomobject]@{
                result = New-ConflictResult -PartitionId $PartitionId -Reason 'Managed run no longer owns the running record.'
                exitCode = $ExitConflict
            }
        }

        if (-not [string]::IsNullOrEmpty($ChildError)) {
            $record.status = 'failed'
            $record.exitCode = $null
            $record.stdout = $null
            $record.stderr = $null
            $record.error = $ChildError
            $record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
            Save-SessionState -State $state -Path $absoluteStatePath
            return [pscustomobject]@{
                result = New-FailedResult -PartitionId $PartitionId -SessionId $RunSessionId -ExitCode $null -Stdout $null -Stderr $null -Error $ChildError
                exitCode = $ExitChildFailure
            }
        }

        $record.exitCode = $ChildResult.exitCode
        $record.stdout = $ChildResult.stdout
        $record.stderr = $ChildResult.stderr
        $record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        if ($ChildResult.exitCode -eq 0) {
            $record.status = 'completed'
            $record.error = $null
            Save-SessionState -State $state -Path $absoluteStatePath
            return [pscustomobject]@{
                result = [pscustomobject]@{
                    status = 'completed'
                    partition = $PartitionId
                    report = $ReportInfo.reportPath
                    sessionId = $RunSessionId
                    exitCode = $ChildResult.exitCode
                    stdout = $ChildResult.stdout
                    stderr = $ChildResult.stderr
                    lockReleased = $true
                }
                exitCode = $ExitSuccess
            }
        }

        $record.status = 'failed'
        $record.error = "External command exited with code $($ChildResult.exitCode)."
        Save-SessionState -State $state -Path $absoluteStatePath
        return [pscustomobject]@{
            result = New-FailedResult -PartitionId $PartitionId -SessionId $RunSessionId -ExitCode $ChildResult.exitCode -Stdout $ChildResult.stdout -Stderr $ChildResult.stderr -Error $record.error
            exitCode = $ExitChildFailure
        }
    }
    catch {
        return [pscustomobject]@{
            result = [pscustomobject]@{
                status = 'internal-error'
                partition = $PartitionId
                sessionId = $RunSessionId
                error = $_.Exception.Message
            }
            exitCode = $ExitInternal
        }
    }
    finally {
        if ($null -ne $finalizationLock) {
            $finalizationLock.Dispose()
        }
    }
}

function Finalize-ManagedRunFromState {
    param(
        [Parameter(Mandatory)] $PendingRun,
        [AllowNull()] $ChildResult,
        [AllowNull()][string] $ChildError
    )

    $finalizationLock = $null
    try {
        $finalizationLock = Acquire-ExclusiveLock -Path $absoluteLockPath -WaitSeconds 30 -SessionId $PendingRun.sessionId
        if ($null -eq $finalizationLock) {
            return [pscustomobject]@{
                result = [pscustomobject]@{
                    status = 'internal-error'
                    partition = $PendingRun.partitionId
                    sessionId = $PendingRun.sessionId
                    error = 'Could not reacquire the checkout lock to finalize the managed run.'
                }
                exitCode = $ExitInternal
            }
        }

        $state = Load-SessionState -Path $absoluteStatePath
        $record = Get-PartitionRecord -State $state -Id $PendingRun.partitionId
        if ($record.status -ne 'running' -or [string]$record.sessionId -ne $PendingRun.sessionId -or $record.claimMode -ne 'managed') {
            return [pscustomobject]@{
                result = New-ConflictResult -PartitionId $PendingRun.partitionId -Reason 'Managed run no longer owns the running record.'
                exitCode = $ExitConflict
            }
        }

        if (-not [string]::IsNullOrEmpty($ChildError)) {
            $record.status = 'failed'
            $record.exitCode = $null
            $record.stdout = $null
            $record.stderr = $null
            $record.error = $ChildError
            $record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
            Save-SessionState -State $state -Path $absoluteStatePath
            return [pscustomobject]@{
                result = New-FailedResult -PartitionId $PendingRun.partitionId -SessionId $PendingRun.sessionId -ExitCode $null -Stdout $null -Stderr $null -Error $ChildError
                exitCode = $ExitChildFailure
            }
        }

        $record.exitCode = $ChildResult.exitCode
        $record.stdout = $ChildResult.stdout
        $record.stderr = $ChildResult.stderr
        $record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        if ($ChildResult.exitCode -eq 0) {
            $record.status = 'completed'
            $record.error = $null
            Save-SessionState -State $state -Path $absoluteStatePath
            return [pscustomobject]@{
                result = [pscustomobject]@{
                    status = 'completed'
                    partition = $PendingRun.partitionId
                    report = $PendingRun.reportInfo.reportPath
                    sessionId = $PendingRun.sessionId
                    exitCode = $ChildResult.exitCode
                    stdout = $ChildResult.stdout
                    stderr = $ChildResult.stderr
                    lockReleased = $true
                }
                exitCode = $ExitSuccess
            }
        }

        $record.status = 'failed'
        $record.error = "External command exited with code $($ChildResult.exitCode)."
        Save-SessionState -State $state -Path $absoluteStatePath
        return [pscustomobject]@{
            result = New-FailedResult -PartitionId $PendingRun.partitionId -SessionId $PendingRun.sessionId -ExitCode $ChildResult.exitCode -Stdout $ChildResult.stdout -Stderr $ChildResult.stderr -Error $record.error
            exitCode = $ExitChildFailure
        }
    }
    catch {
        return [pscustomobject]@{
            result = [pscustomobject]@{
                status = 'internal-error'
                partition = $PendingRun.partitionId
                sessionId = $PendingRun.sessionId
                error = $_.Exception.Message
            }
            exitCode = $ExitInternal
        }
    }
    finally {
        if ($null -ne $finalizationLock) {
            $finalizationLock.Dispose()
        }
    }
}

$exitCode = $ExitSuccess
$result = $null
$lockStream = $null
$pendingManagedRun = $null
$invocationSessionId = [guid]::NewGuid().ToString('N')
$absoluteStatePath = $null
$absoluteLockPath = $null
$effectiveCommandArguments = @()

try {
    if ($Action -eq 'Initialize') {
        if ([string]::IsNullOrWhiteSpace($TaskSetName)) {
            throw 'TaskSetName is required for Initialize.'
        }
        if ([string]::IsNullOrWhiteSpace($TaskTablePath)) {
            throw 'TaskTablePath is required for Initialize.'
        }
        $activeTaskSetContext = Get-TaskSetContext -Name $TaskSetName
        $taskTableSourcePath = Get-AbsolutePath -Path $TaskTablePath
        $initialized = Initialize-PartitionTaskSet -Context $activeTaskSetContext -SourcePath $taskTableSourcePath
        $result = $initialized.result
        $exitCode = $initialized.exitCode
    }
    else {
        if (-not [string]::IsNullOrWhiteSpace($TaskTablePath)) {
            throw 'TaskTablePath is valid only with Initialize.'
        }
        if ([string]::IsNullOrWhiteSpace($TaskSetName)) {
            throw 'TaskSetName is required for every action except Initialize.'
        }
        $activeTaskSetContext = Get-TaskSetContext -Name $TaskSetName
        if (-not (Test-Path -LiteralPath $activeTaskSetContext.taskTablePath -PathType Leaf)) {
            throw "Task set is not initialized: $TaskSetName"
        }
        $activeTaskSetContext.taskTableSha256 = Get-TaskTableHash -Path $activeTaskSetContext.taskTablePath
        $partitionManifest = @(Import-PartitionTaskTable -Path $activeTaskSetContext.taskTablePath)
        $absoluteStatePath = $activeTaskSetContext.statePath
        $absoluteLockPath = $activeTaskSetContext.lockPath

    if (($Action -eq 'Run' -or $Action -eq 'RunNext') -and [string]::IsNullOrWhiteSpace($CommandPath)) {
        throw "CommandPath is required for $Action."
    }
    if (($Action -eq 'Run' -or $Action -eq 'Claim' -or $Action -eq 'Heartbeat' -or $Action -eq 'Complete' -or $Action -eq 'Fail' -or $Action -eq 'Abandon' -or $Action -eq 'CloseSession' -or $Action -eq 'Cleanup' -or $Action -eq 'Handoff') -and [string]::IsNullOrWhiteSpace($PartitionId)) {
        throw "PartitionId is required for $Action."
    }
    if (($Action -eq 'Run' -or $Action -eq 'Claim' -or $Action -eq 'Heartbeat' -or $Action -eq 'Complete' -or $Action -eq 'Fail' -or $Action -eq 'Abandon' -or $Action -eq 'CloseSession' -or $Action -eq 'Cleanup' -or $Action -eq 'Handoff') -and $null -eq (Get-ManifestPartition -Id $PartitionId)) {
        throw "Unknown partition id: $PartitionId"
    }
    if (($Action -eq 'Heartbeat' -or $Action -eq 'Complete' -or $Action -eq 'Fail' -or $Action -eq 'Abandon' -or $Action -eq 'CloseSession' -or $Action -eq 'Handoff') -and [string]::IsNullOrWhiteSpace($SessionId)) {
        throw "SessionId is required for $Action."
    }
    if ($Action -eq 'Handoff' -and [string]::IsNullOrWhiteSpace($HandoffId)) {
        throw 'HandoffId is required for Handoff.'
    }
    if ($Action -eq 'Fail' -and [string]::IsNullOrWhiteSpace($FailureMessage)) {
        $FailureMessage = 'Manual session reported failure.'
    }
    if ($Action -eq 'Run' -or $Action -eq 'RunNext') {
        $effectiveCommandArguments = Resolve-CommandArguments -ArgumentList $CommandArgumentList -ArgumentListJson $CommandArgumentListJson
    }

    $reportInfos = @{}
    if ($Action -eq 'List' -or $Action -eq 'RunNext' -or $Action -eq 'ClaimNext') {
        foreach ($info in @(Get-AllReportInfo)) {
            $reportInfos[$info.id] = $info
        }
    }
    else {
        $selectedManifest = Get-ManifestPartition -Id $PartitionId
        $selectedInfo = Get-ReportInfo -Partition $selectedManifest
        $reportInfos[$selectedInfo.id] = $selectedInfo
    }

    $lockStream = Acquire-ExclusiveLock -Path $absoluteLockPath -WaitSeconds $LockWaitSeconds -SessionId $invocationSessionId
    if ($null -eq $lockStream) {
        $result = New-BusyResult -Path $absoluteLockPath
        $exitCode = $ExitBusy
    }
    else {
        try {
            $state = Load-SessionState -Path $absoluteStatePath
            $reconciled = $false
            if ($Action -ne 'Cleanup') {
                $reconciled = Reconcile-AbandonedRecords -State $state
            }

            if ($Action -eq 'List') {
                foreach ($info in $reportInfos.Values) {
                    $record = Get-PartitionRecord -State $state -Id $info.id
                    Set-ReportRecord -Record $record -ReportInfo $info
                }
                if ($reconciled) {
                    Save-SessionState -State $state -Path $absoluteStatePath
                }
                $result = New-PartitionListResult -State $state
            }
            elseif ($Action -eq 'Heartbeat') {
                $record = Get-PartitionRecord -State $state -Id $PartitionId
                $leaseStatus = Assert-ManualSessionLease -Record $record -ClaimSessionId $SessionId
                if ($null -ne $leaseStatus) {
                    Save-SessionState -State $state -Path $absoluteStatePath
                }
                $result = [pscustomobject]@{
                    status = 'heartbeat'
                    partition = $PartitionId
                    sessionId = $SessionId
                    lastHeartbeatUtc = $record.lastHeartbeatUtc
                    leaseExpiresAtUtc = $record.leaseExpiresAtUtc
                    lockReleased = $true
                }
            }
            elseif ($Action -eq 'Handoff') {
                $oldRecord = Get-PartitionRecord -State $state -Id $PartitionId
                $null = Assert-ManualSessionLease -Record $oldRecord -ClaimSessionId $SessionId
                Stop-RecordLease -Record $oldRecord -Reason 'handoff'
                $newLease = Start-RecordLease -PartitionId $PartitionId -SessionToken $invocationSessionId
                $handoff = Handoff-ManualRecord -State $state -PartitionId $PartitionId -OldSessionId $SessionId -HandoffId $HandoffId -NewSessionId $invocationSessionId -LeaseInfo $newLease
                Save-SessionState -State $state -Path $absoluteStatePath
                $result = New-HandoffResult -PartitionId $PartitionId -ReportPath $reportInfos[$PartitionId].reportPath -OldSessionId $handoff.oldSessionId -NewSessionId $handoff.newSessionId -HandoffId $HandoffId -PreviousStatus $handoff.previousStatus
            }
            elseif ($Action -eq 'Complete' -or $Action -eq 'Fail' -or $Action -eq 'Abandon' -or $Action -eq 'CloseSession') {
                $record = Get-PartitionRecord -State $state -Id $PartitionId
                $null = Assert-ManualSessionLease -Record $record -ClaimSessionId $SessionId
                $finalStatus = if ($Action -eq 'Complete') { 'completed' } elseif ($Action -eq 'Fail') { 'failed' } else { 'abandoned' }
                $finalExitCode = if ($Action -eq 'Complete') { 0 } else { $ResultExitCode }
                Stop-RecordLease -Record $record -Reason $(if ($Action -eq 'CloseSession') { 'codex-session-closed' } else { $Action.ToLowerInvariant() })
                $record = Complete-ManualRecord -State $state -PartitionId $PartitionId -ClaimSessionId $SessionId -FinalStatus $finalStatus -FailureMessage $FailureMessage -FinalExitCode $finalExitCode
                Save-SessionState -State $state -Path $absoluteStatePath
                $result = [pscustomobject]@{
                    status = $record.status
                    partition = $PartitionId
                    report = $reportInfos[$PartitionId].reportPath
                    sessionId = $SessionId
                    exitCode = $record.exitCode
                    error = $record.error
                    lockReleased = $true
                }
                if ($Action -eq 'Fail') {
                    $exitCode = $ExitChildFailure
                }
            }
            elseif ($Action -eq 'Cleanup') {
                $record = Get-PartitionRecord -State $state -Id $PartitionId
                $previousStatus = [string]$record.status
                Set-ReportRecord -Record $record -ReportInfo $reportInfos[$PartitionId]
                Stop-RecordLease -Record $record -Reason 'cleanup'
                Reset-RecordToAvailable -Record $record
                Save-SessionState -State $state -Path $absoluteStatePath
                $result = New-CleanupResult -PartitionId $PartitionId -PreviousStatus $previousStatus
            }
            else {
                $selectNext = $Action -eq 'RunNext' -or $Action -eq 'ClaimNext'
                $selectedId = Get-ClaimablePartitionId -State $state -RequestedPartitionId $PartitionId -SelectNext:$selectNext -Retry:$Retry
                if ($null -eq $selectedId) {
                    $result = [pscustomobject]@{ status = 'conflict'; error = 'No claimable partition remains.' }
                    $exitCode = $ExitConflict
                }
                else {
                    $record = Get-PartitionRecord -State $state -Id $selectedId
                    $reportInfo = $reportInfos[$selectedId]
                    $canClaim = $record.status -eq 'available' -or ($Retry -and ($record.status -eq 'failed' -or $record.status -eq 'abandoned'))
                    if (-not $canClaim) {
                        $reason = if ($record.status -eq 'completed') { 'Partition is already completed.' } elseif ($record.status -eq 'running') { 'Partition is already running.' } else { "Partition status '$($record.status)' is not claimable without -Retry." }
                        $result = New-ConflictResult -PartitionId $selectedId -Reason $reason
                        $exitCode = $ExitConflict
                    }
                    else {
                        $claimMode = if ($Action -eq 'Claim' -or $Action -eq 'ClaimNext') { 'manual' } else { 'managed' }
                        if ($claimMode -eq 'manual') {
                            Assert-CodexSessionHasNoOtherClaim -State $state -PartitionId $selectedId
                        }
                        $leaseInfo = if ($claimMode -eq 'manual') { Start-RecordLease -PartitionId $selectedId -SessionToken $invocationSessionId } else { $null }
                        try {
                            Set-ClaimRecord -Record $record -ReportInfo $reportInfo -SessionId $invocationSessionId -ClaimMode $claimMode -CommandPath $(if ($claimMode -eq 'managed') { $CommandPath } else { $null }) -CommandArguments $(if ($claimMode -eq 'managed') { $effectiveCommandArguments } else { @() }) -LeaseInfo $leaseInfo
                            Save-SessionState -State $state -Path $absoluteStatePath
                        }
                        catch {
                            if ($null -ne $leaseInfo) {
                                Stop-Version4SessionLeaseHost -LeaseFilePath $leaseInfo.leaseFilePath -StopFilePath $leaseInfo.stopFilePath -SessionToken $invocationSessionId
                            }
                            throw
                        }

                        if ($claimMode -eq 'manual') {
                            $result = New-ClaimedResult -ReportInfo $reportInfo -SessionId $invocationSessionId
                            $result | Add-Member -MemberType NoteProperty -Name leaseHostProcessId -Value $leaseInfo.hostProcessId
                            $result | Add-Member -MemberType NoteProperty -Name leaseExpiresAtUtc -Value $leaseInfo.leaseExpiresAtUtc
                            $result | Add-Member -MemberType NoteProperty -Name codexSessionId -Value $leaseInfo.codexSessionId
                        }
                        else {
                            $pendingManagedRun = [pscustomobject]@{
                                partitionId = $selectedId
                                reportInfo = $reportInfo
                                sessionId = $invocationSessionId
                                commandPath = $CommandPath
                                commandArguments = @($effectiveCommandArguments)
                            }
                        }
                    }
                }
            }
        }
        finally {
            $lockStream.Dispose()
            $lockStream = $null
        }
    }

    if ($null -ne $pendingManagedRun -and $null -eq $result) {
        $child = $null
        $childError = $null
        try {
            $child = Invoke-ExternalCommand -Executable $pendingManagedRun.commandPath -Arguments $pendingManagedRun.commandArguments -PartitionId $pendingManagedRun.partitionId -ReportPath $pendingManagedRun.reportInfo.reportPath -SessionId $pendingManagedRun.sessionId
        }
        catch {
            $childError = $_.Exception.Message
        }
        $finalized = Finalize-ManagedRun -PartitionId $pendingManagedRun.partitionId -ReportInfo $pendingManagedRun.reportInfo -RunSessionId $pendingManagedRun.sessionId -ChildResult $child -ChildError $childError
        $result = $finalized.result
        $exitCode = $finalized.exitCode
    }
    }
}
catch {
    if ($_.Exception.Message -match '^Report |^Unknown partition |^CommandPath |^CommandArgumentList|^Use either CommandArgumentList|^PartitionId |^SessionId |^HandoffId |^Owner process is not available|^Report contains ID-class|^Task table |^Task set |^TaskSetName |^TaskTablePath |^Initialize ') {
        $result = New-InvalidResult -Message $_.Exception.Message
        $exitCode = $ExitInvalid
    }
    elseif ($_.Exception.Message -match '^Partition is already|^Partition is not running|^Partition was not claimed|^Session id does not own|^Owner Codex session does not own|^Codex session already owns|^Session lease is not active|^Partition status|^Session state partition set|^No claimable') {
        $result = [pscustomobject]@{ status = 'conflict'; partition = $PartitionId; error = $_.Exception.Message }
        $exitCode = $ExitConflict
    }
    else {
        $result = [pscustomobject]@{
            status = 'internal-error'
            error = $_.Exception.Message
        }
        $exitCode = $ExitInternal
    }
}

if ($null -eq $result) {
    $result = [pscustomobject]@{
        status = 'internal-error'
        error = 'The session ended without a result.'
    }
    $exitCode = $ExitInternal
}

Write-Output ($result | ConvertTo-Json -Depth 20)
exit $exitCode
