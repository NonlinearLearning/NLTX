Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-Version4LeaseName {
    param(
        [Parameter(Mandatory)][string] $TaskSetName,
        [Parameter(Mandatory)][string] $PartitionId
    )

    $text = "Version4PartitionSession|$TaskSetName|$PartitionId"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
    $hex = ([System.BitConverter]::ToString($hash)).Replace('-', '').ToLowerInvariant()
    return "Local\Version4PartitionSession_$hex"
}

function Get-Version4PowerShellPath {
    $command = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        $command = Get-Command powershell -ErrorAction Stop
    }
    return $command.Source
}

function Get-Version4ProcessStartTime {
    param([Parameter(Mandatory)][int] $ProcessId)

    $process = Get-Process -Id $ProcessId -ErrorAction Stop
    return $process.StartTime.ToUniversalTime().ToString('o')
}

function Test-Version4ProcessIdentity {
    param(
        [AllowNull()][object] $ProcessId,
        [AllowNull()][string] $ProcessStartTime
    )

    if ($null -eq $ProcessId -or [int]$ProcessId -le 0) {
        return $false
    }
    try {
        $process = Get-Process -Id ([int]$ProcessId) -ErrorAction Stop
        if ([string]::IsNullOrWhiteSpace($ProcessStartTime)) {
            return $true
        }
        $actual = $process.StartTime.ToUniversalTime()
        $expected = [DateTime]::Parse($ProcessStartTime).ToUniversalTime()
        return [Math]::Abs(($actual - $expected).TotalSeconds) -lt 2
    }
    catch {
        return $false
    }
}

function Read-Version4LeaseFile {
    param([Parameter(Mandatory)][string] $LeaseFilePath)

    if (-not (Test-Path -LiteralPath $LeaseFilePath -PathType Leaf)) {
        return $null
    }
    try {
        return (Get-Content -Raw -LiteralPath $LeaseFilePath | ConvertFrom-Json -DateKind String)
    }
    catch {
        return $null
    }
}

function Test-Version4LeaseMutexHeld {
    param([Parameter(Mandatory)][string] $LeaseName)

    $mutex = $null
    try {
        $mutex = [System.Threading.Mutex]::OpenExisting($LeaseName)
        try {
            $acquired = $mutex.WaitOne(0)
            if ($acquired) {
                $mutex.ReleaseMutex()
                return $false
            }
            return $true
        }
        catch [System.Threading.AbandonedMutexException] {
            try { $mutex.ReleaseMutex() } catch { }
            return $false
        }
    }
    catch [System.Threading.WaitHandleCannotBeOpenedException] {
        return $false
    }
    finally {
        if ($null -ne $mutex) { $mutex.Dispose() }
    }
}

function Get-Version4LeaseStatus {
    param(
        [Parameter(Mandatory)][string] $LeaseName,
        [Parameter(Mandatory)][string] $LeaseFilePath,
        [Parameter(Mandatory)][string] $SessionToken
    )

    $lease = Read-Version4LeaseFile -LeaseFilePath $LeaseFilePath
    if ($null -eq $lease -or [string]$lease.sessionToken -ne $SessionToken) {
        return [pscustomobject]@{ active = $false; reason = 'lease-file-missing-or-token-mismatch'; lease = $lease }
    }
    $expires = $null
    try { $expires = [DateTime]::Parse([string]$lease.leaseExpiresAtUtc).ToUniversalTime() } catch { }
    if ($null -eq $expires -or $expires -le [DateTime]::UtcNow) {
        return [pscustomobject]@{ active = $false; reason = 'lease-expired'; lease = $lease }
    }
    if (-not (Test-Version4ProcessIdentity -ProcessId $lease.hostProcessId -ProcessStartTime ([string]$lease.hostProcessStartTime))) {
        return [pscustomobject]@{ active = $false; reason = 'lease-host-unavailable'; lease = $lease }
    }
    if (-not (Test-Version4LeaseMutexHeld -LeaseName $LeaseName)) {
        return [pscustomobject]@{ active = $false; reason = 'lease-mutex-not-held'; lease = $lease }
    }
    return [pscustomobject]@{ active = $true; reason = $null; lease = $lease }
}

function Start-Version4SessionLeaseHost {
    param(
        [Parameter(Mandatory)][string] $LeaseName,
        [Parameter(Mandatory)][string] $LeaseFilePath,
        [Parameter(Mandatory)][string] $SessionToken,
        [AllowNull()][string] $CodexSessionId,
        [ValidateRange(2, 3600)][int] $HeartbeatSeconds = 15,
        [AllowNull()][int] $OwnerProcessId,
        [AllowNull()][string] $OwnerProcessStartTime,
        [ValidateRange(1, 30)][int] $ReadyTimeoutSeconds = 5
    )

    $parent = [System.IO.Path]::GetDirectoryName($LeaseFilePath)
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        $null = New-Item -ItemType Directory -Path $parent -Force
    }
    $stopFilePath = "$LeaseFilePath.$SessionToken.stop"
    Remove-Item -LiteralPath $stopFilePath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $LeaseFilePath -Force -ErrorAction SilentlyContinue

    $hostScript = Join-Path $PSScriptRoot 'Version4SessionLeaseHost.ps1'
    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = Get-Version4PowerShellPath
    # The host outlives this runner invocation. Shell execution with a hidden window
    # prevents the runner's stdout/stderr pipes from being inherited by the host.
    $psi.UseShellExecute = $true
    $psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $psi.WorkingDirectory = $PSScriptRoot
    foreach ($argument in @('-NoProfile', '-File', $hostScript, '-LeaseName', $LeaseName, '-LeaseFilePath', $LeaseFilePath, '-StopFilePath', $stopFilePath, '-SessionToken', $SessionToken, '-HeartbeatSeconds', [string]$HeartbeatSeconds)) {
        $psi.ArgumentList.Add($argument)
    }
    if (-not [string]::IsNullOrWhiteSpace($CodexSessionId)) {
        $psi.ArgumentList.Add('-CodexSessionId')
        $psi.ArgumentList.Add($CodexSessionId)
    }
    if ($null -ne $OwnerProcessId -and $OwnerProcessId -gt 0) {
        $psi.ArgumentList.Add('-OwnerProcessId')
        $psi.ArgumentList.Add([string]$OwnerProcessId)
    }
    if (-not [string]::IsNullOrWhiteSpace($OwnerProcessStartTime)) {
        $psi.ArgumentList.Add('-OwnerProcessStartTime')
        $psi.ArgumentList.Add($OwnerProcessStartTime)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $psi
    if (-not $process.Start()) {
        throw 'Could not start Version4 session lease host.'
    }
    $deadline = [DateTime]::UtcNow.AddSeconds($ReadyTimeoutSeconds)
    try {
        while ([DateTime]::UtcNow -lt $deadline) {
            $lease = Read-Version4LeaseFile -LeaseFilePath $LeaseFilePath
            if ($null -ne $lease -and [string]$lease.sessionToken -eq $SessionToken -and [string]$lease.status -eq 'active') {
                return [pscustomobject]@{
                    leaseName = $LeaseName
                    leaseFilePath = $LeaseFilePath
                    stopFilePath = $stopFilePath
                    hostProcessId = $process.Id
                    hostProcessStartTime = Get-Version4ProcessStartTime -ProcessId $process.Id
                    lastHeartbeatUtc = [string]$lease.lastHeartbeatUtc
                    leaseExpiresAtUtc = [string]$lease.leaseExpiresAtUtc
                }
            }
            if ($process.HasExited) {
                throw "Version4 session lease host exited with code $($process.ExitCode)."
            }
            Start-Sleep -Milliseconds 50
        }
        throw 'Timed out waiting for Version4 session lease host readiness.'
    }
    catch {
        if (-not $process.HasExited) { $process.Kill($true) }
        throw
    }
    finally {
        $process.Dispose()
    }
}

function Stop-Version4SessionLeaseHost {
    param(
        [Parameter(Mandatory)][string] $LeaseFilePath,
        [Parameter(Mandatory)][string] $StopFilePath,
        [Parameter(Mandatory)][string] $SessionToken,
        [ValidateRange(1, 30)][int] $WaitSeconds = 5
    )

    $parent = [System.IO.Path]::GetDirectoryName($StopFilePath)
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        $null = New-Item -ItemType Directory -Path $parent -Force
    }
    [System.IO.File]::WriteAllText($StopFilePath, $SessionToken, [System.Text.UTF8Encoding]::new($false))
    $lease = Read-Version4LeaseFile -LeaseFilePath $LeaseFilePath
    if ($null -ne $lease -and [string]$lease.sessionToken -eq $SessionToken -and (Test-Version4ProcessIdentity -ProcessId $lease.hostProcessId -ProcessStartTime ([string]$lease.hostProcessStartTime))) {
        $process = Get-Process -Id ([int]$lease.hostProcessId) -ErrorAction SilentlyContinue
        if ($null -ne $process) {
            $deadline = [DateTime]::UtcNow.AddSeconds($WaitSeconds)
            while (-not $process.HasExited -and [DateTime]::UtcNow -lt $deadline) {
                Start-Sleep -Milliseconds 50
                $process.Refresh()
            }
            if (-not $process.HasExited) { $process.Kill($true) }
            $process.Dispose()
        }
    }
    Remove-Item -LiteralPath $LeaseFilePath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $StopFilePath -Force -ErrorAction SilentlyContinue
}

Export-ModuleMember -Function Get-Version4LeaseName, Get-Version4ProcessStartTime, Get-Version4LeaseStatus, Start-Version4SessionLeaseHost, Stop-Version4SessionLeaseHost, Test-Version4ProcessIdentity
