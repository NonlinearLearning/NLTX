[CmdletBinding()]
param(
  [Parameter(
    Mandatory = $true,
    Position = 0,
    ValueFromRemainingArguments = $true
  )]
  [string[]]$DotnetArguments
)

$checkoutRootCandidate = Join-Path -Path $PSScriptRoot -ChildPath '..\..'
$checkoutRoot = (Resolve-Path -LiteralPath $checkoutRootCandidate -ErrorAction Stop).ProviderPath
$checkoutRoot = [System.IO.Path]::GetFullPath($checkoutRoot)
$normalizedCheckoutRoot = ($checkoutRoot -replace '[\\/]+$', '').ToUpperInvariant()

$sha256 = [System.Security.Cryptography.SHA256]::Create()
try {
  $rootBytes = [System.Text.Encoding]::UTF8.GetBytes($normalizedCheckoutRoot)
  $hashBytes = $sha256.ComputeHash($rootBytes)
  $checkoutHash = [System.BitConverter]::ToString($hashBytes).Replace('-', '')
}
finally {
  $sha256.Dispose()
}

$checkoutHash = $checkoutHash.Substring(0, 16)
$mutexScope = if ($env:OS -eq 'Windows_NT') { 'Global\' } else { '' }
$mutexName = '{0}NLTX-DotnetBuild-{1}' -f $mutexScope, $checkoutHash

$mutex = $null
$ownsMutex = $false
$locationPushed = $false
$childExitCode = 1

try {
  $mutex = [System.Threading.Mutex]::new($false, $mutexName)
  $reportedWait = $false

  while (-not $ownsMutex) {
    try {
      $ownsMutex = $mutex.WaitOne(1000)
    }
    catch [System.Threading.AbandonedMutexException] {
      $ownsMutex = $true
      Write-Verbose "Acquired abandoned serialized dotnet mutex '$mutexName'."
    }

    if (-not $ownsMutex -and -not $reportedWait) {
      Write-Host "Waiting for serialized dotnet mutex '$mutexName'."
      $reportedWait = $true
    }
  }

  Push-Location -LiteralPath $checkoutRoot
  $locationPushed = $true

  $dotnetCommand = Get-Command -Name dotnet -CommandType Application -ErrorAction Stop |
    Select-Object -First 1
  & $dotnetCommand.Source @DotnetArguments
  $childExitCode = $LASTEXITCODE
}
finally {
  try {
    if ($locationPushed) {
      Pop-Location
    }
  }
  finally {
    if ($null -ne $mutex) {
      try {
        if ($ownsMutex) {
          $mutex.ReleaseMutex()
        }
      }
      finally {
        $mutex.Dispose()
      }
    }
  }
}

exit $childExitCode
