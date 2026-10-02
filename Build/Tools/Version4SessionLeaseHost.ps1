[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $LeaseName,
    [Parameter(Mandatory)][string] $LeaseFilePath,
    [Parameter(Mandatory)][string] $StopFilePath,
    [Parameter(Mandatory)][string] $SessionToken,
    [string] $CodexSessionId,
    [ValidateRange(2, 3600)][int] $HeartbeatSeconds = 15,
    [int] $OwnerProcessId = 0,
    [string] $OwnerProcessStartTime
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-LeaseFile {
    param([Parameter(Mandatory)][string] $Status)

    $now = [DateTime]::UtcNow
    $lease = [pscustomobject]@{
        status = $Status
        sessionToken = $SessionToken
        codexSessionId = if ([string]::IsNullOrWhiteSpace($CodexSessionId)) { $null } else { $CodexSessionId }
        leaseName = $LeaseName
        hostProcessId = $PID
        hostProcessStartTime = $script:hostProcessStartTime
        ownerProcessId = if ($OwnerProcessId -gt 0) { $OwnerProcessId } else { $null }
        ownerProcessStartTime = if ([string]::IsNullOrWhiteSpace($OwnerProcessStartTime)) { $null } else { $OwnerProcessStartTime }
        lastHeartbeatUtc = $now.ToString('o')
        leaseExpiresAtUtc = $now.AddSeconds($HeartbeatSeconds * 3).ToString('o')
        stopFilePath = $StopFilePath
    }
    $parent = [System.IO.Path]::GetDirectoryName($LeaseFilePath)
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        $null = New-Item -ItemType Directory -Path $parent -Force
    }
    $temporaryPath = "$LeaseFilePath.$([guid]::NewGuid().ToString('N')).tmp"
    try {
        $json = $lease | ConvertTo-Json -Depth 8 -Compress
        [System.IO.File]::WriteAllText($temporaryPath, $json, [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::Move($temporaryPath, $LeaseFilePath, $true)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
        }
    }
}

function Test-ProcessIdentity {
    if ($OwnerProcessId -le 0) {
        return $true
    }

    try {
        $process = Get-Process -Id $OwnerProcessId -ErrorAction Stop
        if ([string]::IsNullOrWhiteSpace($OwnerProcessStartTime)) {
            return $true
        }
        $actual = $process.StartTime.ToUniversalTime()
        $expected = [DateTime]::Parse($OwnerProcessStartTime).ToUniversalTime()
        return [Math]::Abs(($actual - $expected).TotalSeconds) -lt 2
    }
    catch {
        return $false
    }
}

$mutex = $null
$mutexHeld = $false
$script:hostProcessStartTime = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
try {
    $createdNew = $false
    $mutex = [System.Threading.Mutex]::new($false, $LeaseName, [ref]$createdNew)
    try {
        $mutexHeld = $mutex.WaitOne(0)
    }
    catch [System.Threading.AbandonedMutexException] {
        $mutexHeld = $true
    }
    if (-not $mutexHeld) {
        exit 4
    }

    Write-LeaseFile -Status 'active'
    while ($true) {
        if (Test-Path -LiteralPath $StopFilePath -PathType Leaf) {
            break
        }
        if (-not (Test-ProcessIdentity)) {
            break
        }

        Start-Sleep -Seconds $HeartbeatSeconds
        if (Test-Path -LiteralPath $StopFilePath -PathType Leaf) {
            break
        }
        if (-not (Test-ProcessIdentity)) {
            break
        }
        Write-LeaseFile -Status 'active'
    }
}
catch {
    try { Write-LeaseFile -Status 'faulted' } catch { }
    exit 6
}
finally {
    if ($mutexHeld -and $null -ne $mutex) {
        try { $mutex.ReleaseMutex() } catch { }
    }
    if ($null -ne $mutex) {
        $mutex.Dispose()
    }
    if (Test-Path -LiteralPath $LeaseFilePath -PathType Leaf) {
        try {
            $current = Get-Content -Raw -LiteralPath $LeaseFilePath | ConvertFrom-Json -DateKind String
            if ([string]$current.sessionToken -eq $SessionToken) {
                Remove-Item -LiteralPath $LeaseFilePath -Force -ErrorAction SilentlyContinue
            }
        }
        catch { }
    }
}
