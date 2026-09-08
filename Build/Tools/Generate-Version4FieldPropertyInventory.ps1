[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$SnapshotPath,

  [Parameter(Mandatory = $true)]
  [string]$OutputPath
)

Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

function Resolve-RepositoryPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path
  )

  if ([IO.Path]::IsPathRooted($Path)) {
    return [IO.Path]::GetFullPath($Path)
  }

  return [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
}

function Normalize-RelativePath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path
  )

  return $Path.Replace('\', '/')
}

function Test-IdFile {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path
  )

  $stem = [IO.Path]::GetFileNameWithoutExtension($Path)
  return $stem -cmatch 'IDs?$'
}

function ConvertTo-MarkdownCell {
  param(
    [AllowNull()]
    [object]$Value
  )

  if ($null -eq $Value) {
    return '-'
  }

  $text = ([string]$Value).Replace("`r", ' ').Replace("`n", ' ')
  return $text.Replace('|', '\|')
}

function Get-MemberLocation {
  param(
    [Parameter(Mandatory = $true)]
    [object]$Member,

    [Parameter(Mandatory = $true)]
    [string]$MemberPath
  )

  $locations = @($Member.locations)
  if ($locations.Count -ne 1) {
    throw "Expected exactly one source location for '$($Member.sourceMemberId)', got $($locations.Count)."
  }

  $location = $locations[0]
  $line = 0
  if (-not [int]::TryParse([string]$location.line, [ref]$line) -or $line -lt 1) {
    throw "Invalid source line for '$($Member.sourceMemberId)': $($location.line)."
  }

  $locationPath = Normalize-RelativePath ([string]$location.path)
  if ($locationPath -cne $MemberPath) {
    throw "Member '$($Member.sourceMemberId)' location path '$locationPath' does not match member path '$MemberPath'."
  }

  return [pscustomobject]@{
    Line = $line
    Column = [int]$location.column
  }
}

$snapshotFullPath = Resolve-RepositoryPath $SnapshotPath
$outputFullPath = Resolve-RepositoryPath $OutputPath

if (-not (Test-Path -LiteralPath $snapshotFullPath -PathType Leaf)) {
  throw "Snapshot does not exist: $snapshotFullPath"
}

$snapshot = Get-Content -Raw -LiteralPath $snapshotFullPath | ConvertFrom-Json
if ([string]$snapshot.snapshotKind -cne 'source') {
  throw "Snapshot must have snapshotKind 'source', got '$($snapshot.snapshotKind)'."
}
if ([string]$snapshot.completeness -cne 'complete') {
  throw "Snapshot is not complete: $($snapshot.completeness)"
}
if (@($snapshot.diagnostics).Count -ne 0) {
  throw "Snapshot contains $(@($snapshot.diagnostics).Count) diagnostics."
}
if ($null -eq $snapshot.files -or $null -eq $snapshot.members) {
  throw 'Snapshot must contain files and members arrays.'
}

$allFiles = @($snapshot.files)
$allMembers = @($snapshot.members)
$filePaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$retainedFiles = [System.Collections.Generic.List[object]]::new()
$excludedFiles = [System.Collections.Generic.List[object]]::new()

foreach ($file in $allFiles) {
  $path = Normalize-RelativePath ([string]$file.path)
  if ([string]::IsNullOrWhiteSpace($path)) {
    throw 'Snapshot contains a file with an empty path.'
  }
  if (-not $filePaths.Add($path)) {
    throw "Snapshot contains duplicate file path: $path"
  }

  $record = [pscustomobject]@{
    Path = $path
    Count = 0
    Fields = 0
    Properties = 0
  }
  if (Test-IdFile $path) {
    $excludedFiles.Add($record)
  }
  else {
    $retainedFiles.Add($record)
  }
}

$membersByPath = @{}
$memberIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$excludedMemberCount = 0
$retainedMembers = [System.Collections.Generic.List[object]]::new()

foreach ($member in $allMembers) {
  $kind = [string]$member.kind
  if ($kind -notin @('field', 'property')) {
    continue
  }

  $path = Normalize-RelativePath ([string]$member.path)
  if (-not $filePaths.Contains($path)) {
    throw "Member '$($member.sourceMemberId)' references a path absent from the files array: $path"
  }
  if (-not $memberIds.Add([string]$member.sourceMemberId)) {
    throw "Snapshot contains duplicate sourceMemberId: $($member.sourceMemberId)"
  }

  $location = Get-MemberLocation $member $path
  if (Test-IdFile $path) {
    $excludedMemberCount++
    continue
  }

  $record = [pscustomobject]@{
    Path = $path
    Line = $location.Line
    Column = $location.Column
    Kind = $kind
    DeclaringType = [string]$member.declaringType
    Member = [string]$member.member
    Signature = [string]$member.signature
    Type = [string]$member.type
    Accessibility = [string]$member.accessibility
    Modifiers = (@($member.modifiers) -join ' ')
    SourceMemberId = [string]$member.sourceMemberId
  }
  $retainedMembers.Add($record)

  if (-not $membersByPath.ContainsKey($path)) {
    $membersByPath[$path] = [System.Collections.Generic.List[object]]::new()
  }
  $membersByPath[$path].Add($record)
}

foreach ($file in $retainedFiles) {
  if ($membersByPath.ContainsKey($file.Path)) {
    $file.Count = $membersByPath[$file.Path].Count
    $file.Fields = @($membersByPath[$file.Path] | Where-Object Kind -eq 'field').Count
    $file.Properties = @($membersByPath[$file.Path] | Where-Object Kind -eq 'property').Count
  }
}

$sortedFiles = @($retainedFiles | Sort-Object -Property @{Expression = 'Count'; Descending = $true}, @{Expression = 'Path'; Descending = $false})
$sortedMembers = @($retainedMembers | Sort-Object -Property Path, Line, Column, Kind, DeclaringType, Member, Signature)
$fieldCount = @($sortedMembers | Where-Object Kind -eq 'field').Count
$propertyCount = @($sortedMembers | Where-Object Kind -eq 'property').Count

$outputDirectory = Split-Path -Parent $outputFullPath
if (-not (Test-Path -LiteralPath $outputDirectory -PathType Container)) {
  New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$temporaryOutput = Join-Path $outputDirectory ".$([IO.Path]::GetFileName($outputFullPath)).$([Guid]::NewGuid().ToString('N')).tmp"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('# Version4 全部字段和属性明细（去除 ID 类文件）')
$lines.Add('')
$lines.Add('## 统计口径')
$lines.Add('')
$lines.Add(('- 源码根目录：`' + [string]$snapshot.root + '`。'))
$lines.Add('- 扫描方式：仓库内 `Version4MemberMigrationScanner` 使用 Roslyn 语法树和语义模型生成快照；本工具负责校验快照并生成 Markdown。')
$lines.Add('- 排除规则：文件名主体以大写 `ID` 或 `IDs` 结尾的代码文件，例如 `ItemID.cs`、`ArmorIDs.cs`。')
$lines.Add('- 成员范围：C# `field` 和 `property`；不包含事件、方法、局部变量、参数或嵌套类型以外的非字段/属性成员。')
$lines.Add('- 每条明细均保留相对源码文件路径、源码行号、列号、声明类型、成员名称、C# 类型、访问级别和修饰符。')
$lines.Add('- 文件索引包含所有保留源文件，包括字段/属性数量为 `0` 的文件。')
$lines.Add('')
$lines.Add('## 扫描汇总')
$lines.Add('')
$lines.Add("- 快照文件总数：$($allFiles.Count)")
$lines.Add("- 排除 ID 类文件：$($excludedFiles.Count)")
$lines.Add("- 保留文件：$($sortedFiles.Count)")
$lines.Add("- 快照字段/属性成员：$(@($allMembers | Where-Object { [string]$_.kind -in @('field', 'property') }).Count)")
$lines.Add("- 排除 ID 类文件成员：$excludedMemberCount")
$lines.Add("- 保留字段：$fieldCount")
$lines.Add("- 保留属性：$propertyCount")
$lines.Add("- 保留字段/属性合计：$($sortedMembers.Count)")
$lines.Add("- 保留文件中成员数大于 0：$(@($sortedFiles | Where-Object Count -gt 0).Count)")
$lines.Add("- 保留文件中成员数为 0：$(@($sortedFiles | Where-Object Count -eq 0).Count)")
$lines.Add(('- 快照完整性：`' + [string]$snapshot.completeness + '`；诊断数：' + [string]@($snapshot.diagnostics).Count))
$lines.Add('')
$lines.Add('## 文件索引（全部保留文件）')
$lines.Add('')
$lines.Add('<!-- file-index-start -->')
$lines.Add('| 序号 | 文件路径（相对 Version4） | 字段/属性合计 | 字段 | 属性 |')
$lines.Add('|---:|---|---:|---:|---:|')
$fileIndex = 0
foreach ($file in $sortedFiles) {
  $fileIndex++
  $lines.Add("| $fileIndex | $(ConvertTo-MarkdownCell $file.Path) | $($file.Count) | $($file.Fields) | $($file.Properties) |")
}
$lines.Add('<!-- file-index-end -->')
$lines.Add('')
$lines.Add('## 字段和属性明细（全部保留成员）')
$lines.Add('')
$lines.Add('<!-- member-detail-start -->')
$lines.Add('| 序号 | 文件路径（相对 Version4） | 行号 | 列号 | 成员类型 | 声明类型 | 成员 | C# 类型 | 访问级别 | 修饰符 |')
$lines.Add('|---:|---|---:|---:|---|---|---|---|---|---|')
$memberIndex = 0
foreach ($member in $sortedMembers) {
  $memberIndex++
  $lines.Add("| $memberIndex | $(ConvertTo-MarkdownCell $member.Path) | $($member.Line) | $($member.Column) | $($member.Kind) | $(ConvertTo-MarkdownCell $member.DeclaringType) | $(ConvertTo-MarkdownCell $member.Member) | $(ConvertTo-MarkdownCell $member.Type) | $(ConvertTo-MarkdownCell $member.Accessibility) | $(ConvertTo-MarkdownCell $member.Modifiers) |")
}
$lines.Add('<!-- member-detail-end -->')
$lines.Add('')
$lines.Add('## 排除的 ID 类文件')
$lines.Add('')
$lines.Add("共 $($excludedFiles.Count) 个：")
$lines.Add('')
foreach ($file in @($excludedFiles | Sort-Object Path)) {
  $lines.Add("- ``$(ConvertTo-MarkdownCell $file.Path)``")
}

try {
  Set-Content -LiteralPath $temporaryOutput -Value $lines -Encoding utf8NoBOM
  Move-Item -LiteralPath $temporaryOutput -Destination $outputFullPath -Force
}
finally {
  if (Test-Path -LiteralPath $temporaryOutput) {
    Remove-Item -LiteralPath $temporaryOutput -Force
  }
}

Write-Output "Generated Markdown: $outputFullPath"
Write-Output "Retained files: $($sortedFiles.Count)"
Write-Output "Retained fields: $fieldCount"
Write-Output "Retained properties: $propertyCount"
Write-Output "Retained field/property members: $($sortedMembers.Count)"
Write-Output "Excluded ID files: $($excludedFiles.Count)"
