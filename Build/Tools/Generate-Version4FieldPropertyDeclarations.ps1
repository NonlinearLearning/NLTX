[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$SnapshotPath,

  [Parameter(Mandatory = $true)]
  [string]$OutputPath,

  [string]$RoslynDirectory = 'Build/bin/Version4MemberMigrationScanner/Debug/net10.0',

  [ValidateSet('all', 'field', 'property')]
  [string]$MemberKind = 'all'
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

function ConvertTo-OneLineDeclaration {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Text
  )

  $oneLine = $Text.Replace("`r`n", ' ').Replace("`n", ' ').Replace("`r", ' ')
  return ($oneLine -replace '\s+', ' ').Trim()
}

function Get-MemberLocation {
  param(
    [Parameter(Mandatory = $true)]
    [object]$Member
  )

  $locations = @($Member.locations)
  if ($locations.Count -ne 1) {
    throw "Expected exactly one source location for '$($Member.sourceMemberId)', got $($locations.Count)."
  }

  $location = $locations[0]
  $line = 0
  $column = 0
  if (-not [int]::TryParse([string]$location.line, [ref]$line) -or $line -lt 1) {
    throw "Invalid source line for '$($Member.sourceMemberId)': $($location.line)."
  }
  if (-not [int]::TryParse([string]$location.column, [ref]$column) -or $column -lt 1) {
    throw "Invalid source column for '$($Member.sourceMemberId)': $($location.column)."
  }

  return [pscustomobject]@{
    Line = $line
    Column = $column
    Path = Normalize-RelativePath ([string]$location.path)
  }
}

function Get-NodeKind {
  param(
    [Parameter(Mandatory = $true)]
    [object]$Node
  )

  if ($Node -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax]) {
    return 'field'
  }
  if ($Node -is [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax] -or
      $Node -is [Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax]) {
    return 'property'
  }

  throw "Unsupported declaration node type: $($Node.GetType().FullName)"
}

function Get-NodeKey {
  param(
    [Parameter(Mandatory = $true)]
    [object]$Node
  )

  $span = $Node.GetLocation().GetLineSpan().Span
  return "$(Get-NodeKind $Node)|$($span.Start.Line + 1)|$($span.Start.Character + 1)"
}

function Get-FieldMemberDeclaration {
  param(
    [Parameter(Mandatory = $true)]
    [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax]$Field,

    [Parameter(Mandatory = $true)]
    [string]$MemberName
  )

  $variable = @($Field.Declaration.Variables | Where-Object { $_.Identifier.ValueText -ceq $MemberName })
  if ($variable.Count -ne 1) {
    throw "Cannot resolve field '$MemberName' in declaration '$($Field.ToString())'."
  }

  $attributeText = (($Field.AttributeLists | ForEach-Object { $_.ToString().Trim() }) -join ' ')
  $modifierText = (($Field.Modifiers | ForEach-Object { $_.Text }) -join ' ')
  $typeText = $Field.Declaration.Type.ToString().Trim()
  $variableText = $variable[0].ToString().Trim()
  $parts = @($attributeText, $modifierText, $typeText, $variableText) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
  return ((($parts -join ' ') + ';').Trim())
}

$snapshotFullPath = Resolve-RepositoryPath $SnapshotPath
$outputFullPath = Resolve-RepositoryPath $OutputPath
$roslynDirectoryFullPath = Resolve-RepositoryPath $RoslynDirectory

if (-not (Test-Path -LiteralPath $snapshotFullPath -PathType Leaf)) {
  throw "Snapshot does not exist: $snapshotFullPath"
}
if (-not (Test-Path -LiteralPath $roslynDirectoryFullPath -PathType Container)) {
  throw "Roslyn directory does not exist: $roslynDirectoryFullPath"
}

$codeAnalysisAssembly = Join-Path $roslynDirectoryFullPath 'Microsoft.CodeAnalysis.dll'
$csharpAssembly = Join-Path $roslynDirectoryFullPath 'Microsoft.CodeAnalysis.CSharp.dll'
if (-not (Test-Path -LiteralPath $codeAnalysisAssembly -PathType Leaf) -or
    -not (Test-Path -LiteralPath $csharpAssembly -PathType Leaf)) {
  throw "Roslyn assemblies are missing under: $roslynDirectoryFullPath"
}

if ($null -eq ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
  Add-Type -Path $codeAnalysisAssembly
  Add-Type -Path $csharpAssembly
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
if ($null -eq $snapshot.root -or $null -eq $snapshot.members) {
  throw 'Snapshot must contain root and members arrays.'
}

$sourceRoot = [IO.Path]::GetFullPath([string]$snapshot.root)
if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
  throw "Snapshot source root does not exist: $sourceRoot"
}

$selectedSnapshotMembers = @($snapshot.members | Where-Object {
    $kind = [string]$_.kind
    ($MemberKind -eq 'all' -or $kind -ceq $MemberKind) -and $kind -in @('field', 'property')
  })
$members = @($selectedSnapshotMembers | Where-Object {
    -not (Test-IdFile (Normalize-RelativePath ([string]$_.path)))
  })
$memberIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$membersByPath = @{}
foreach ($member in $members) {
  $sourceMemberId = [string]$member.sourceMemberId
  if (-not $memberIds.Add($sourceMemberId)) {
    throw "Snapshot contains duplicate sourceMemberId: $sourceMemberId"
  }

  $path = Normalize-RelativePath ([string]$member.path)
  $location = Get-MemberLocation $member
  if ($location.Path -cne $path) {
    throw "Member '$sourceMemberId' location path '$($location.Path)' does not match member path '$path'."
  }
  if (-not $membersByPath.ContainsKey($path)) {
    $membersByPath[$path] = [System.Collections.Generic.List[object]]::new()
  }
  $membersByPath[$path].Add([pscustomobject]@{
      Source = $member
      Path = $path
      Location = $location
    })
}

$parseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default
$parseOptions = $parseOptions.WithLanguageVersion([Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Preview)
$parseOptions = $parseOptions.WithDocumentationMode([Microsoft.CodeAnalysis.DocumentationMode]::Parse)
$declarations = [System.Collections.Generic.List[object]]::new()

foreach ($path in @($membersByPath.Keys | Sort-Object)) {
  $fullPath = Join-Path $sourceRoot ($path.Replace('/', [IO.Path]::DirectorySeparatorChar))
  if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
    throw "Source file does not exist: $fullPath"
  }

  $bytes = [IO.File]::ReadAllBytes($fullPath)
  $encoding = [Text.UTF8Encoding]::new($false, $true)
  $preamble = $encoding.GetPreamble()
  $offset = if ($preamble.Length -gt 0 -and $bytes.AsSpan().StartsWith($preamble)) { $preamble.Length } else { 0 }
  $text = $encoding.GetString($bytes, $offset, $bytes.Length - $offset)
  $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($text, $parseOptions, $fullPath)
  $root = $tree.GetRoot()
  $nodeByKey = @{}
  foreach ($node in @($root.DescendantNodes() | Where-Object {
      $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax] -or
      $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax] -or
      $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax]
    })) {
    $key = Get-NodeKey $node
    if ($nodeByKey.ContainsKey($key)) {
      throw "Duplicate declaration location '$key' in $path"
    }
    $nodeByKey[$key] = $node
  }

  foreach ($entry in $membersByPath[$path]) {
    $member = $entry.Source
    $location = $entry.Location
    $key = "$( [string]$member.kind )|$($location.Line)|$($location.Column)".Replace(' ', '')
    if (-not $nodeByKey.ContainsKey($key)) {
      throw "Cannot find declaration node for '$($member.sourceMemberId)' at ${path}:$($location.Line):$($location.Column)."
    }

    $node = $nodeByKey[$key]
    $kind = Get-NodeKind $node
    if ($kind -ne [string]$member.kind) {
      throw "Declaration kind mismatch for '$($member.sourceMemberId)': snapshot=$($member.kind), syntax=$kind."
    }

    if ($kind -eq 'field') {
      $memberDeclaration = Get-FieldMemberDeclaration $node ([string]$member.member)
    }
    else {
      $memberDeclaration = ConvertTo-OneLineDeclaration $node.ToString()
    }

    $sourceDeclaration = ConvertTo-OneLineDeclaration $node.ToString()
    $declarations.Add([pscustomobject]@{
        SourceMemberId = [string]$member.sourceMemberId
        Path = $entry.Path
        AbsolutePath = $fullPath
        Line = $location.Line
        Column = $location.Column
        Kind = $kind
        DeclaringType = [string]$member.declaringType
        Member = [string]$member.member
        Type = [string]$member.type
        MemberDeclaration = $memberDeclaration
        SourceDeclaration = $sourceDeclaration
      })
  }
}

$sortedDeclarations = @($declarations | Sort-Object -Property Path, Line, Column, Kind, DeclaringType, Member, SourceMemberId)
$fieldCount = @($sortedDeclarations | Where-Object Kind -eq 'field').Count
$propertyCount = @($sortedDeclarations | Where-Object Kind -eq 'property').Count
$snapshotMemberCount = $selectedSnapshotMembers.Count
$excludedMemberCount = $snapshotMemberCount - $sortedDeclarations.Count
$allFiles = @($snapshot.files)
$excludedFiles = @($allFiles | Where-Object { Test-IdFile (Normalize-RelativePath ([string]$_.path)) })
$retainedFiles = $allFiles.Count - $excludedFiles.Count

$expectedCount = @{
  all = 7201
  field = 6414
  property = 787
}[$MemberKind]
if ($sortedDeclarations.Count -ne $expectedCount) {
  throw "Unexpected retained count for '$MemberKind': total=$($sortedDeclarations.Count), expected=$expectedCount."
}

$memberLabel = switch ($MemberKind) {
  'field' { '字段' }
  'property' { '属性' }
  default { '字段和属性' }
}

$outputDirectory = Split-Path -Parent $outputFullPath
if (-not (Test-Path -LiteralPath $outputDirectory -PathType Container)) {
  New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$temporaryOutput = Join-Path $outputDirectory ".$([IO.Path]::GetFileName($outputFullPath)).$([Guid]::NewGuid().ToString('N')).tmp"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add(('# Version4 ' + $memberLabel + '逐成员源码声明（去除 ID 类文件）'))
$lines.Add('')
$lines.Add('## 使用说明')
$lines.Add('')
$lines.Add(('- 本文档按当前 Version4 Roslyn 快照逐条保留' + $memberLabel + '的源码声明。'))
$lines.Add('- 每条记录包含成员类型、完整类名、相对路径、绝对路径、源码行号、列号、成员名称、C# 类型和声明文本。')
if ($MemberKind -in @('all', 'field')) {
  $lines.Add('- 字段声明按单个变量展开；例如源码中的 `public static int A, B;` 会分别记录为 `public static int A;` 和 `public static int B;`，同时保留该字段声明的原始文本。')
}
if ($MemberKind -in @('all', 'property')) {
  $lines.Add('- 属性保留其完整属性声明，包括访问器或表达式主体。')
}
$lines.Add('- 排除规则：文件名主体以大写 `ID` 或 `IDs` 结尾的代码文件。')
$lines.Add('')
$lines.Add('## 扫描汇总')
$lines.Add('')
$lines.Add(('- 源码根目录：`' + $sourceRoot + '`'))
$lines.Add("- 源代码文件总数：$($allFiles.Count)")
$lines.Add("- 排除 ID 类文件：$($excludedFiles.Count)")
$lines.Add("- 保留代码文件：$retainedFiles")
$lines.Add("- 排除 ID 类文件成员：$excludedMemberCount")
$lines.Add("- 保留字段：$fieldCount")
$lines.Add("- 保留属性：$propertyCount")
$lines.Add("- 本文档成员合计：$($sortedDeclarations.Count)")
$lines.Add('')
$lines.Add('## 逐成员源码声明')
$lines.Add('')
$lines.Add('<!-- member-declaration-start -->')
$lines.Add('| 序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |')
$lines.Add('|---:|---|---|---|---|---:|---:|---|---|---|---|')
$index = 0
foreach ($declaration in $sortedDeclarations) {
  $index++
  $lines.Add("| $index | $($declaration.Kind) | $(ConvertTo-MarkdownCell $declaration.DeclaringType) | $(ConvertTo-MarkdownCell $declaration.Path) | $(ConvertTo-MarkdownCell $declaration.AbsolutePath) | $($declaration.Line) | $($declaration.Column) | $(ConvertTo-MarkdownCell $declaration.Member) | $(ConvertTo-MarkdownCell $declaration.Type) | ``$(ConvertTo-MarkdownCell $declaration.MemberDeclaration)`` | ``$(ConvertTo-MarkdownCell $declaration.SourceDeclaration)`` |")
}
$lines.Add('<!-- member-declaration-end -->')
$lines.Add('')
$lines.Add('## 排除的 ID 类文件')
$lines.Add('')
foreach ($file in @($excludedFiles | Sort-Object { Normalize-RelativePath ([string]$_.path) })) {
  $lines.Add("- ``$(ConvertTo-MarkdownCell (Normalize-RelativePath ([string]$file.path)))``")
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
Write-Output "Retained fields: $fieldCount"
Write-Output "Retained properties: $propertyCount"
Write-Output "Retained field/property declarations: $($sortedDeclarations.Count)"
Write-Output "Excluded ID files: $($excludedFiles.Count)"
