param(
  [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
  [string]$Version4Root = 'D:\TRbackup\Version4物理删除了某些文件',
  [string]$Version3Root = 'D:\TRbackup\Version3删除多余同时人工审查代码'
)

$ErrorActionPreference = 'Stop'
$evidenceRoot = Join-Path $RepositoryRoot 'Build\evidence\item-ecs'
$migrationRoot = Join-Path $RepositoryRoot 'docs\组件文档\migrations'
New-Item -ItemType Directory -Force -Path $evidenceRoot, $migrationRoot | Out-Null

function Get-RelativeFiles([string]$root) {
  Get-ChildItem -LiteralPath $root -Recurse -File |
    ForEach-Object { $_.FullName.Substring($root.Length).TrimStart('\') } |
    Sort-Object
}

$version4Files = Get-RelativeFiles $Version4Root
$version3Files = Get-RelativeFiles $Version3Root
$version4Set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$version3Set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$version4Files | ForEach-Object { [void]$version4Set.Add($_) }
$version3Files | ForEach-Object { [void]$version3Set.Add($_) }
$deletedInVersion4 = $version3Files | Where-Object { -not $version4Set.Contains($_) }
$addedInVersion4 = $version4Files | Where-Object { -not $version3Set.Contains($_) }

$diff = [System.Text.StringBuilder]::new()
[void]$diff.AppendLine("Version3Root=$Version3Root")
[void]$diff.AppendLine("Version4Root=$Version4Root")
[void]$diff.AppendLine("GeneratedUtc=$([DateTime]::UtcNow.ToString('O'))")
[void]$diff.AppendLine("Version3FileCount=$($version3Files.Count)")
[void]$diff.AppendLine("Version4FileCount=$($version4Files.Count)")
[void]$diff.AppendLine()
[void]$diff.AppendLine('[DeletedInVersion4]')
$deletedInVersion4 | ForEach-Object { [void]$diff.AppendLine($_) }
[void]$diff.AppendLine()
[void]$diff.AppendLine('[AddedInVersion4]')
$addedInVersion4 | ForEach-Object { [void]$diff.AppendLine($_) }
[void]$diff.AppendLine()
[void]$diff.AppendLine('[ItemDomainCandidates]')
($version3Files + $version4Files | Sort-Object -Unique |
  Where-Object { $_ -match '(?i)(^|[\\/])(Item|WorldItem|ItemID|ItemDrop|ItemVariant)' }) |
  ForEach-Object { [void]$diff.AppendLine($_) }
Set-Content -LiteralPath (Join-Path $evidenceRoot 'version3-version4-file-diff.txt') -Value $diff.ToString() -Encoding utf8

function Get-MemberCategory([string]$name, [string]$declaration) {
  $uiNames = 'ToolTip|Bestiary|Draw|Texture|Shader|Sound|Audio|Lang|Creative|ItemSlot|Sorting|Hitbox|PhaseColor|Color|Tooltip|RebuildTooltip|SetNameOverride'
  if ($name -match $uiNames -or $declaration -match 'Microsoft\.Xna|Terraria\.UI|Terraria\.Graphics|Terraria\.Audio|GameContent\.UI') {
    return 'Deferred'
  }
  if ($name -match '^(SetDefaults|DefaultTo|Clone|Copy|Net|Save|Load|Serialize|Deserialize|Write|Read|From|To|Get|Is|Can|Check|Use|Consume|Shoot|Place|Pick|Axe|Hammer|Create|Drop|Prefix|Variant|Update|Tick|Find|Set|Reset|TurnTo|Change|Apply|On|Spawn|Pickup|Owner|Grab|FindOwner)') {
    return 'System'
  }
  if ($name -match '(?i)(type|stack|maxStack|use|damage|knock|heal|mana|ammo|shoot|place|tile|wall|slot|rare|value|material|consumable|accessory|vanity|social|expert|quest|buy|potion|channel|reuse|defense|width|height|scale|alpha|paint|dye|prefix|variant|name|flame|mech|fishing|bait|makeNpc)') {
    return 'Definition'
  }
  if ($declaration -match '(?i)static') {
    return 'Compatibility'
  }
  return 'Deferred'
}

$itemPath = Join-Path $Version4Root 'Terraria\Item.cs'
$lines = Get-Content -LiteralPath $itemPath
$members = [System.Collections.Generic.List[object]]::new()
$braceDepth = 0
$insideItem = $false
$pending = $null
for ($index = 0; $index -lt $lines.Count; $index++) {
  $line = $lines[$index]
  $trimmed = $line.Trim()
  $code = ($trimmed -replace '//.*$', '').Trim()
  if ($trimmed -match '^public\s+class\s+Item\b') {
    $insideItem = $true
  }
  if ($insideItem -and $braceDepth -eq 1 -and $null -eq $pending -and
      $code -match '^(?:(?:public|private|protected|internal|static|readonly|const|virtual|override|sealed|unsafe|async|extern|new|partial)\s+)+') {
    $pending = [System.Collections.Generic.List[string]]::new()
    [void]$pending.Add($code)
  } elseif ($null -ne $pending) {
    if ($code.Length -gt 0) { [void]$pending.Add($code) }
  }
  if ($null -ne $pending -and ($code -match '[;{}]$' -or $code -match '\)\s*(?:=>|where|\{)')) {
    $declaration = ($pending -join ' ') -replace '\s+', ' '
    $name = ''
    if ($declaration -match '(?<![\w])([A-Za-z_][A-Za-z0-9_]*)\s*(?:\(|\{|;|=)') {
      $name = $Matches[1]
    }
    if ($name -and $name -notmatch '^(if|for|foreach|while|switch|catch|using|lock)$') {
      $hash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData(
        [System.Text.Encoding]::UTF8.GetBytes($declaration))).Substring(0, 16)
      $kind = if ($declaration -match '\)\s*(?:=>|where|\{|;)') { 'Method' } elseif ($declaration -match '=>|\{') { 'Property' } else { 'Field' }
      $members.Add([pscustomobject]@{
          Id = "Item:${kind}:$($index + 1):${name}:${hash}"
          Name = $name
          Kind = $kind
          SourceFile = 'Terraria/Item.cs'
          SourceLine = $index + 1
          Declaration = $declaration
          TargetCategory = Get-MemberCategory $name $declaration
        })
    }
    $pending = $null
  }
  $opens = ([regex]::Matches($line, '\{')).Count
  $closes = ([regex]::Matches($line, '\}')).Count
  $braceDepth += $opens - $closes
}

$members = @($members | Sort-Object SourceLine, Name, Id)
$members | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'item-members.json') -Encoding utf8

$referenceLines = [System.Collections.Generic.List[string]]::new()
[void]$referenceLines.Add("GeneratedUtc=$([DateTime]::UtcNow.ToString('O'))")
[void]$referenceLines.Add('Production and verification references matching Item domain terms:')
Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'src'), (Join-Path $RepositoryRoot 'Test') -Recurse -File -Filter '*.cs' |
  Select-String -Pattern 'ItemDefinition|InventoryComponent|WorldItemComponent|UseItemCommand|PickupWorldItemCommand|ItemReplication|ItemPersistence|SyncItem|Terraria\.Item|Main\.item|Player\.inventory|ItemID\.Sets' |
  ForEach-Object { [void]$referenceLines.Add("$($_.Path.Substring($RepositoryRoot.Length + 1)):$($_.LineNumber):$($_.Line.Trim())") }
$referenceLines | Set-Content -LiteralPath (Join-Path $evidenceRoot 'item-reference-index.txt') -Encoding utf8

$mapping = [System.Text.StringBuilder]::new()
[void]$mapping.AppendLine('# Item ECS 成员归属表')
[void]$mapping.AppendLine()
[void]$mapping.AppendLine('本表由 `Build/Tools/GenerateItemEcsBaseline.ps1` 从 Version4 `Terraria/Item.cs` 生成。分类是迁移边界初始归属，必须在后续批次中以实现或延期证据收敛。')
[void]$mapping.AppendLine()
[void]$mapping.AppendLine('| ID | 成员 | 类型 | 来源行 | 初始归属 | 目标/原因 |')
[void]$mapping.AppendLine('| --- | --- | --- | ---: | --- | --- |')
foreach ($member in $members) {
  $target = switch ($member.TargetCategory) {
    'Definition' { '`Items/Definitions` or immutable type metadata' }
    'Component' { '`Items/Components` runtime state' }
    'System' { '`Items/Systems` or deterministic command processing' }
    'Compatibility' { '`Items/Compatibility` boundary adapter' }
    default { 'UI/rendering/legacy host concern; deferred until owner is assigned' }
  }
  $safeDeclaration = $member.Declaration.Replace('|', '\|')
  [void]$mapping.AppendLine("| ``$($member.Id)`` | ``$($member.Name)`` | $($member.Kind) | $($member.SourceLine) | $($member.TargetCategory) | $target; ``$safeDeclaration`` |")
}
[void]$mapping.AppendLine()
[void]$mapping.AppendLine('## 排除与延期规则')
[void]$mapping.AppendLine()
[void]$mapping.AppendLine('- UI、纹理、Shader、音效、Tooltip、Bestiary 和依赖 `Main`/客户端全局状态的成员只能进入 `Deferred` 或 `Compatibility`。')
[void]$mapping.AppendLine('- 任何尚未有实现或验证证据的成员不得标记为 `migrated`; 本表的初始归属不是完成声明。')
[void]$mapping.AppendLine('- 成员索引和版本差异的重放输入分别为 `item-members.json` 和 `version3-version4-file-diff.txt`。')
Set-Content -LiteralPath (Join-Path $migrationRoot 'item-ecs-member-mapping.md') -Value $mapping.ToString() -Encoding utf8

Write-Output "Generated $($members.Count) Item members and $($deletedInVersion4.Count) Version4 deletions."
