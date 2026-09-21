[CmdletBinding()]
param(
    [string]$SourceReportPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md',
    [string]$PartitionDirectory = 'docs/migration/ledgers/non-authoritative-component-partitions'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-FullPath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Get-MemberRows {
    param([string[]]$Lines)

    $rows = [System.Collections.Generic.Dictionary[int, string]]::new()
    foreach ($line in $Lines) {
        if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
            $sequence = [int]$Matches[1]
            if ($rows.ContainsKey($sequence)) {
                throw "Duplicate member sequence $sequence."
            }
            $rows[$sequence] = $line
        }
    }
    return $rows
}

function Get-FineNames {
    param([string[]]$Lines)

    return @($Lines | ForEach-Object {
        if ($_ -match '^#{3,4} 4(?:\.\d+){1,2} 细分子系统：`([^`]+)`') {
            $Matches[1]
        }
    })
}

$sourcePath = Get-FullPath -Path $SourceReportPath
$partitionPath = Get-FullPath -Path $PartitionDirectory
if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Source report does not exist: $sourcePath"
}
if (-not (Test-Path -LiteralPath $partitionPath -PathType Container)) {
    throw "Partition directory does not exist: $partitionPath"
}

$sourceLines = [System.IO.File]::ReadAllLines($sourcePath)
$sourceRows = Get-MemberRows -Lines $sourceLines
$sourceFineNames = Get-FineNames -Lines $sourceLines
if ($sourceRows.Count -ne 4542) {
    throw "Source report has $($sourceRows.Count) member rows instead of 4,542."
}
if ($sourceFineNames.Count -ne 349 -or @($sourceFineNames | Sort-Object -Unique).Count -ne 349) {
    throw 'Source report does not contain exactly 349 unique fine subsystem headings.'
}

$expectedHash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
$partitionFiles = @(Get-ChildItem -LiteralPath $partitionPath -File | Sort-Object Name)
if ($partitionFiles.Count -ne 20) {
    throw "Expected exactly 20 partition Markdown files, found $($partitionFiles.Count)."
}
if (@($partitionFiles | Where-Object Extension -ne '.md').Count -ne 0) {
    throw 'Partition directory contains a non-Markdown file.'
}

$outputRows = [System.Collections.Generic.Dictionary[int, string]]::new()
$outputFineNames = [System.Collections.Generic.List[string]]::new()
$actualFields = 0
$actualProperties = 0

foreach ($file in $partitionFiles) {
    $lines = [System.IO.File]::ReadAllLines($file.FullName)
    if (-not ($lines | Where-Object { $_ -eq '## 5. 追溯与验收' })) {
        throw "Partition file has no traceability section: $($file.Name)"
    }

    $hashLine = $lines | Where-Object { $_ -match '^> 来源报告 SHA-256：`([0-9a-f]{64})`$' } | Select-Object -First 1
    if ($null -eq $hashLine) {
        throw "Partition file has no source hash: $($file.Name)"
    }
    if ($hashLine -notmatch '^> 来源报告 SHA-256：`([0-9a-f]{64})`$' -or $Matches[1] -ne $expectedHash) {
        throw "Partition file source hash mismatch: $($file.Name)"
    }

    foreach ($fine in Get-FineNames -Lines $lines) {
        if ($outputFineNames.Contains($fine)) {
            throw "Fine subsystem appears more than once: $fine"
        }
        $outputFineNames.Add($fine)
    }

    foreach ($line in $lines) {
        if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
            $sequence = [int]$Matches[1]
            if ($outputRows.ContainsKey($sequence)) {
                throw "Member sequence $sequence appears in more than one partition or more than once in a partition."
            }
            if (-not $sourceRows.ContainsKey($sequence)) {
                throw "Partition contains unknown member sequence $sequence."
            }
            if ($line -cne $sourceRows[$sequence]) {
                throw "Member row $sequence differs from the source report in $($file.Name)."
            }
            $outputRows[$sequence] = $line
            if ($Matches[2] -eq 'field') {
                $actualFields++
            } else {
                $actualProperties++
            }
        }
    }
}

if ($outputRows.Count -ne $sourceRows.Count) {
    throw "Partition member row count is $($outputRows.Count), source count is $($sourceRows.Count)."
}
for ($sequence = 1; $sequence -le 4542; $sequence++) {
    if (-not $outputRows.ContainsKey($sequence)) {
        throw "Partition member sequence is missing: $sequence"
    }
}

$sourceFineSet = @($sourceFineNames | Sort-Object)
$outputFineSet = @($outputFineNames | Sort-Object)
if ($sourceFineSet.Count -ne $outputFineSet.Count) {
    throw "Fine subsystem heading count mismatch: source=$($sourceFineSet.Count), output=$($outputFineSet.Count)."
}
for ($index = 0; $index -lt $sourceFineSet.Count; $index++) {
    if ($sourceFineSet[$index] -cne $outputFineSet[$index]) {
        throw "Fine subsystem heading mismatch at sorted position $($index + 1): source='$($sourceFineSet[$index])', output='$($outputFineSet[$index])'."
    }
}

$expectedFields = @($sourceRows.Values | Where-Object { $_ -match '^\|\s*\d+\s*\|\s*field\s*\|' }).Count
$expectedProperties = @($sourceRows.Values | Where-Object { $_ -match '^\|\s*\d+\s*\|\s*property\s*\|' }).Count
if ($actualFields -ne $expectedFields -or $actualProperties -ne $expectedProperties) {
    throw "Partition field/property totals differ: expected=$expectedFields/$expectedProperties, actual=$actualFields/$actualProperties."
}

$idRows = @($outputRows.Values | Where-Object { $_ -cmatch '(^|/)[^/]*IDs?\.cs\|' })
if ($idRows.Count -ne 0) {
    throw "Partition output contains $($idRows.Count) ID-class file rows."
}

Write-Output 'PASS: exactly 20 Markdown partition files exist.'
Write-Output 'PASS: 349 fine subsystem headings are represented exactly once.'
Write-Output 'PASS: every source member row is reproduced byte-for-byte exactly once.'
Write-Output "PASS: fields=$actualFields, properties=$actualProperties, total=$($outputRows.Count); ID-class file rows=0."
Write-Output "PASS: all partition files reference source SHA-256 $expectedHash."
