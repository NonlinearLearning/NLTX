[CmdletBinding()]
param(
    [string]$InputPath = 'docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md',
    [string]$ReportPath = 'docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-RepositoryPath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Split-MarkdownRow {
    param([Parameter(Mandatory)][string]$Line)

    $content = $Line.Trim()
    if (-not ($content.StartsWith('|') -and $content.EndsWith('|'))) {
        return @()
    }

    $content = $content.Substring(1, $content.Length - 2)
    return @($content -split '(?<!\\)\|' | ForEach-Object { $_.Trim() })
}

function Read-MemberRows {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][bool]$RequireFineSubsystem
    )

    $rows = [System.Collections.Generic.List[object]]::new()
    $parent = $null
    $fine = $null
    $inMemberSection = $false

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -eq '## 4. 逐成员源码声明') {
            $inMemberSection = $true
            continue
        }

        if ($line -match '^### \d+\.\d+ (?:父级子系统|子系统)：\x60([^\x60]+)\x60') {
            $parent = $Matches[1]
            $fine = $null
            continue
        }

        if ($line -match '^#### \d+\.\d+\.\d+ 细分子系统：\x60([^\x60]+)\x60') {
            $fine = $Matches[1]
            continue
        }

        if ($inMemberSection -and $line -match '^\|\s*(\d+)\s*\|') {
            $cells = @(Split-MarkdownRow $line)
            if ($cells.Count -ne 11) {
                throw "Malformed member row in $Path. Expected 11 cells, found $($cells.Count)."
            }

            if ([string]::IsNullOrWhiteSpace($parent)) {
                throw "Member row in $Path has no parent subsystem."
            }

            if ($RequireFineSubsystem -and [string]::IsNullOrWhiteSpace($fine)) {
                throw "Member row in $Path has no fine subsystem."
            }

            $rows.Add([pscustomobject]@{
                    Seq       = [int]$cells[0]
                    Kind      = $cells[1]
                    Type      = $cells[2]
                    Member    = $cells[7]
                    Parent    = $parent
                    Fine      = $fine
                    Cells     = [string[]]$cells
                })
        }
    }

    return @($rows)
}

function Assert-Equal {
    param(
        [Parameter(Mandatory)][object]$Actual,
        [Parameter(Mandatory)][object]$Expected,
        [Parameter(Mandatory)][string]$Message
    )

    if ($Actual -ne $Expected) {
        throw "$Message. Actual: $Actual; expected: $Expected."
    }
}

$inputFullPath = Resolve-RepositoryPath $InputPath
$reportFullPath = Resolve-RepositoryPath $ReportPath
foreach ($path in @($inputFullPath, $reportFullPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required input does not exist: $path"
    }
}

$inputRows = @(Read-MemberRows -Path $inputFullPath -RequireFineSubsystem:$false)
$reportRows = @(Read-MemberRows -Path $reportFullPath -RequireFineSubsystem:$true)
Assert-Equal $inputRows.Count 2659 'Authoritative input row count mismatch'
Assert-Equal $reportRows.Count 2659 'Fine report row count mismatch'

$inputBySeq = @{}
foreach ($row in $inputRows) {
    $inputBySeq[$row.Seq] = $row
}

$reportBySeq = @{}
foreach ($row in $reportRows) {
    if ($reportBySeq.ContainsKey($row.Seq)) {
        throw "Fine report has duplicate sequence $($row.Seq)."
    }

    $reportBySeq[$row.Seq] = $row
}

foreach ($seq in 1..2659) {
    if (-not $reportBySeq.ContainsKey($seq)) {
        throw "Fine report is missing source sequence $seq."
    }

    if ($inputBySeq[$seq].Parent -ne $reportBySeq[$seq].Parent -or
        ([string]::Join([char]0x1f, $inputBySeq[$seq].Cells) -cne
        [string]::Join([char]0x1f, $reportBySeq[$seq].Cells))) {
        throw "Fine report changed the authoritative row at sequence $seq."
    }
}

$baselineTopTwenty = @(
    [pscustomobject]@{ Rank = 1; Name = 'PlayerNamedPetFlagState'; Fields = 55; Properties = 0; Total = 55; Split = $false }
    [pscustomobject]@{ Rank = 2; Name = 'WorldSecretSeedDefinitions'; Fields = 41; Properties = 0; Total = 41; Split = $false }
    [pscustomobject]@{ Rank = 3; Name = 'MountGeometryAndFrameCatalog'; Fields = 32; Properties = 0; Total = 32; Split = $true }
    [pscustomobject]@{ Rank = 4; Name = 'NpcBossDefeatProgressionState'; Fields = 31; Properties = 0; Total = 31; Split = $true }
    [pscustomobject]@{ Rank = 5; Name = 'MountStaticAndDrillConstants'; Fields = 30; Properties = 0; Total = 30; Split = $true }
    [pscustomobject]@{ Rank = 6; Name = 'NpcStatusEffectFlags'; Fields = 30; Properties = 0; Total = 30; Split = $true }
    [pscustomobject]@{ Rank = 7; Name = 'PlayerCompanionAndRestState'; Fields = 28; Properties = 2; Total = 30; Split = $true }
    [pscustomobject]@{ Rank = 8; Name = 'PlayerMinionCapacityAndSummonState'; Fields = 30; Properties = 0; Total = 30; Split = $true }
    [pscustomobject]@{ Rank = 9; Name = 'MountRuntimeProjectionProperties'; Fields = 2; Properties = 27; Total = 29; Split = $false }
    [pscustomobject]@{ Rank = 10; Name = 'PlayerIdentityAndLifecycleState'; Fields = 29; Properties = 0; Total = 29; Split = $true }
    [pscustomobject]@{ Rank = 11; Name = 'GenVarsWorldLayerAndSurfaceState'; Fields = 28; Properties = 0; Total = 28; Split = $true }
    [pscustomobject]@{ Rank = 12; Name = 'WorldGenerationDimensionsAndExecution'; Fields = 28; Properties = 0; Total = 28; Split = $true }
    [pscustomobject]@{ Rank = 13; Name = 'NpcSpawnEnvironmentEligibilityInputs'; Fields = 27; Properties = 0; Total = 27; Split = $false }
    [pscustomobject]@{ Rank = 14; Name = 'NpcTownRescueAndSpawnUnlocks'; Fields = 27; Properties = 0; Total = 27; Split = $true }
    [pscustomobject]@{ Rank = 15; Name = 'PlayerAppearanceEquipmentSelection'; Fields = 27; Properties = 0; Total = 27; Split = $true }
    [pscustomobject]@{ Rank = 16; Name = 'WorldGenerationTileMutationActions'; Fields = 27; Properties = 0; Total = 27; Split = $false }
    [pscustomobject]@{ Rank = 17; Name = 'PlayerAppearanceEquipmentProjection'; Fields = 26; Properties = 0; Total = 26; Split = $true }
    [pscustomobject]@{ Rank = 18; Name = 'NpcNetworkAndSpawnState'; Fields = 25; Properties = 0; Total = 25; Split = $true }
    [pscustomobject]@{ Rank = 19; Name = 'PlayerFrameImmunityAndInteractionState'; Fields = 25; Properties = 0; Total = 25; Split = $true }
    [pscustomobject]@{ Rank = 20; Name = 'PlayerSurvivalAndControlStatus'; Fields = 25; Properties = 0; Total = 25; Split = $true }
)

$expectedChildren = [ordered]@{
    MountGeometryAndFrameCatalog = @(
        [pscustomobject]@{ Name = 'MountGeometryAndOffsetCatalog'; Parent = 'MountAndVehicleSimulation'; Total = 9 }
        [pscustomobject]@{ Name = 'MountAnimationFrameCatalog'; Parent = 'MountAndVehicleSimulation'; Total = 23 }
    )
    NpcBossDefeatProgressionState = @(
        [pscustomobject]@{ Name = 'NpcBossDefeatFlags'; Parent = 'NpcAndTownSimulation'; Total = 17 }
        [pscustomobject]@{ Name = 'NpcEventDefeatFlags'; Parent = 'NpcAndTownSimulation'; Total = 14 }
    )
    MountStaticAndDrillConstants = @(
        [pscustomobject]@{ Name = 'MountFrameAndDrawCatalog'; Parent = 'MountAndVehicleSimulation'; Total = 11 }
        [pscustomobject]@{ Name = 'MountSpecialVehicleCatalog'; Parent = 'MountAndVehicleSimulation'; Total = 5 }
        [pscustomobject]@{ Name = 'MountDrillConstants'; Parent = 'MountAndVehicleSimulation'; Total = 9 }
        [pscustomobject]@{ Name = 'MountSuperCartConstants'; Parent = 'MountAndVehicleSimulation'; Total = 5 }
    )
    NpcStatusEffectFlags = @(
        [pscustomobject]@{ Name = 'NpcElementalDebuffState'; Parent = 'NpcAndTownSimulation'; Total = 17 }
        [pscustomobject]@{ Name = 'NpcControlAndSocialEffectState'; Parent = 'NpcAndTownSimulation'; Total = 4 }
        [pscustomobject]@{ Name = 'NpcWhipAndSpecialEffectState'; Parent = 'NpcAndTownSimulation'; Total = 9 }
    )
    PlayerCompanionAndRestState = @(
        [pscustomobject]@{ Name = 'PlayerEyeAnimationState'; Parent = 'PlayerGameplay'; Total = 4 }
        [pscustomobject]@{ Name = 'PlayerPettingState'; Parent = 'PlayerGameplay'; Total = 7 }
        [pscustomobject]@{ Name = 'PlayerSittingState'; Parent = 'PlayerGameplay'; Total = 5 }
        [pscustomobject]@{ Name = 'PlayerSleepingState'; Parent = 'PlayerGameplay'; Total = 7 }
        [pscustomobject]@{ Name = 'PlayerRabbitOrderFrameState'; Parent = 'PlayerGameplay'; Total = 7 }
    )
    PlayerMinionCapacityAndSummonState = @(
        [pscustomobject]@{ Name = 'PlayerMinionCapacityState'; Parent = 'PlayerGameplay'; Total = 3 }
        [pscustomobject]@{ Name = 'PlayerMinionSummonFlags'; Parent = 'PlayerGameplay'; Total = 25 }
        [pscustomobject]@{ Name = 'PlayerMinionDamageTrackingState'; Parent = 'PlayerGameplay'; Total = 2 }
    )
    PlayerIdentityAndLifecycleState = @(
        [pscustomobject]@{ Name = 'PlayerIdentityAndDeathRecordState'; Parent = 'PlayerGameplay'; Total = 10 }
        [pscustomobject]@{ Name = 'PlayerRuntimeInteractionAndEffectState'; Parent = 'PlayerGameplay'; Total = 13 }
        [pscustomobject]@{ Name = 'PlayerConsumedProgressionFlags'; Parent = 'PlayerGameplay'; Total = 6 }
    )
    GenVarsWorldLayerAndSurfaceState = @(
        [pscustomobject]@{ Name = 'GenVarsWorldLayerMetrics'; Parent = 'WorldGenerationAndEcology'; Total = 13 }
        [pscustomobject]@{ Name = 'GenVarsSurfaceAndBiomeState'; Parent = 'WorldGenerationAndEcology'; Total = 15 }
    )
    WorldGenerationDimensionsAndExecution = @(
        [pscustomobject]@{ Name = 'WorldGenerationDimensionsState'; Parent = 'WorldGenerationAndEcology'; Total = 8 }
        [pscustomobject]@{ Name = 'WorldGenerationExecutionState'; Parent = 'WorldGenerationAndEcology'; Total = 6 }
        [pscustomobject]@{ Name = 'WorldGenerationSecretSeedFlags'; Parent = 'WorldGenerationAndEcology'; Total = 10 }
        [pscustomobject]@{ Name = 'WorldGenerationScratchState'; Parent = 'WorldGenerationAndEcology'; Total = 4 }
    )
    NpcTownRescueAndSpawnUnlocks = @(
        [pscustomobject]@{ Name = 'NpcTownRescueState'; Parent = 'NpcAndTownSimulation'; Total = 8 }
        [pscustomobject]@{ Name = 'NpcTownPetAdoptionState'; Parent = 'NpcAndTownSimulation'; Total = 3 }
        [pscustomobject]@{ Name = 'NpcTownSpawnUnlockState'; Parent = 'NpcAndTownSimulation'; Total = 16 }
    )
    PlayerAppearanceEquipmentSelection = @(
        [pscustomobject]@{ Name = 'PlayerEquipmentSelectionSlots'; Parent = 'PlayerGameplay'; Total = 21 }
        [pscustomobject]@{ Name = 'PlayerAppearanceSelectionState'; Parent = 'PlayerGameplay'; Total = 6 }
    )
    PlayerAppearanceEquipmentProjection = @(
        [pscustomobject]@{ Name = 'PlayerEquipmentColorProjection'; Parent = 'PlayerGameplay'; Total = 20 }
        [pscustomobject]@{ Name = 'PlayerTraversalColorProjection'; Parent = 'PlayerGameplay'; Total = 6 }
    )
    NpcNetworkAndSpawnState = @(
        [pscustomobject]@{ Name = 'NpcNetworkReplicationState'; Parent = 'NpcAndTownSimulation'; Total = 11 }
        [pscustomobject]@{ Name = 'NpcSpawnBudgetAndActivityState'; Parent = 'NpcAndTownSimulation'; Total = 10 }
        [pscustomobject]@{ Name = 'NpcIdentityAndStatusState'; Parent = 'NpcAndTownSimulation'; Total = 4 }
    )
    PlayerFrameImmunityAndInteractionState = @(
        [pscustomobject]@{ Name = 'PlayerFrameAndImmunityState'; Parent = 'PlayerGameplay'; Total = 11 }
        [pscustomobject]@{ Name = 'PlayerInteractionInputState'; Parent = 'PlayerGameplay'; Total = 14 }
    )
    PlayerSurvivalAndControlStatus = @(
        [pscustomobject]@{ Name = 'PlayerSurvivalAndTransformationState'; Parent = 'PlayerGameplay'; Total = 15 }
        [pscustomobject]@{ Name = 'PlayerDebuffStatusState'; Parent = 'PlayerGameplay'; Total = 8 }
        [pscustomobject]@{ Name = 'PlayerBuilderOverlayState'; Parent = 'PlayerGameplay'; Total = 2 }
    )
}

$reportFineTotals = @{}
foreach ($row in $reportRows) {
    if (-not $reportFineTotals.ContainsKey($row.Fine)) {
        $reportFineTotals[$row.Fine] = [ordered]@{ Parent = $row.Parent; field = 0; property = 0; Total = 0 }
    }

    if ($reportFineTotals[$row.Fine].Parent -ne $row.Parent) {
        throw "Fine subsystem $($row.Fine) crosses formal parents."
    }

    $reportFineTotals[$row.Fine][$row.Kind]++
    $reportFineTotals[$row.Fine].Total++
}

Assert-Equal $reportFineTotals.Count 221 'Active fine subsystem count mismatch'

foreach ($entry in $baselineTopTwenty) {
    $sourceRows = @($reportRows | Where-Object Fine -eq $entry.Name)
    if ($entry.Split) {
        Assert-Equal $sourceRows.Count 0 "Retired current top-20 source still owns members: $($entry.Name)"
        $children = @($expectedChildren[$entry.Name])
        Assert-Equal $children.Count ($expectedChildren[$entry.Name]).Count "Child definition count mismatch for $($entry.Name)"
        $childTotal = 0
        foreach ($child in $children) {
            if (-not $reportFineTotals.ContainsKey($child.Name)) {
                throw "Missing current top-20 child subsystem $($child.Name)."
            }

            $stats = $reportFineTotals[$child.Name]
            Assert-Equal $stats.Parent $child.Parent "Wrong parent for current top-20 child $($child.Name)"
            Assert-Equal $stats.Total $child.Total "Wrong member count for current top-20 child $($child.Name)"
            $childTotal += $stats.Total
        }

        Assert-Equal $childTotal $entry.Total "Current top-20 child total mismatch for $($entry.Name)"
    } else {
        if (-not $reportFineTotals.ContainsKey($entry.Name)) {
            throw "Missing retained current top-20 subsystem $($entry.Name)."
        }

        Assert-Equal $reportFineTotals[$entry.Name].Total $entry.Total "Retained current top-20 total mismatch for $($entry.Name)"
    }
}

$reportLines = @(Get-Content -LiteralPath $reportFullPath)
if ($reportLines -notcontains '### 3.5 当前排行榜前 20 的再次细分审查') {
    throw 'Fine report is missing the current top-20 audit section.'
}

$auditStart = [array]::IndexOf($reportLines, '### 3.5 当前排行榜前 20 的再次细分审查')
$auditEnd = [array]::IndexOf($reportLines, '### 3.6 二次细分前基线来源组排行榜（前 20，仅追溯）')
if ($auditStart -lt 0 -or $auditEnd -le $auditStart) {
    throw 'Fine report has invalid current top-20 audit boundaries.'
}

$auditLines = @($reportLines[($auditStart + 1)..($auditEnd - 1)])
foreach ($entry in $baselineTopTwenty) {
    $sourceToken = "$([char]0x60)$($entry.Name)$([char]0x60)"
    if (@($auditLines | Where-Object { $_.Contains("| $($entry.Rank) | $sourceToken |") }).Count -eq 0) {
        throw "Current top-20 audit is missing source $($entry.Name)."
    }

    $decision = if ($entry.Split) { '替换' } else { '保留' }
    if (@($auditLines | Where-Object { $_.Contains("| $($entry.Rank) | $sourceToken | $decision |") }).Count -eq 0) {
        throw "Current top-20 audit has the wrong decision for $($entry.Name)."
    }
}

$rankingStart = [array]::IndexOf($reportLines, '### 3.3 当前活动细分子系统字段属性数量排行榜（全部 221）')
$rankingEnd = [array]::IndexOf($reportLines, '### 3.4 二次细分目标与新子系统')
if ($rankingStart -lt 0 -or $rankingEnd -le $rankingStart) {
    throw 'Fine report has invalid active leaderboard boundaries.'
}

$rankingRows = @($reportLines[($rankingStart + 1)..($rankingEnd - 1)] |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object {
        $cells = @(Split-MarkdownRow $_)
        if ($cells.Count -eq 5 -and $cells[0] -match '^\d+$' -and $cells[1] -match '^\x60[^\x60]+\x60$') {
            [pscustomobject]@{
                Rank       = [int]$cells[0]
                Name       = $cells[1].Trim([char]0x60)
                Fields     = [int]$cells[2]
                Properties = [int]$cells[3]
                Total      = [int]$cells[4]
            }
        }
    })
Assert-Equal $rankingRows.Count 221 'Active leaderboard row count mismatch'

$expectedRanking = @($reportFineTotals.Keys | ForEach-Object {
        [pscustomobject]@{
            Name       = $_
            Fields     = $reportFineTotals[$_].field
            Properties = $reportFineTotals[$_].property
            Total      = $reportFineTotals[$_].Total
        }
    } | Sort-Object -Property @{Expression = 'Total'; Descending = $true}, @{Expression = 'Name'; Descending = $false })

for ($index = 0; $index -lt $expectedRanking.Count; $index++) {
    $expected = $expectedRanking[$index]
    $actual = $rankingRows[$index]
    Assert-Equal $actual.Rank ($index + 1) "Active leaderboard rank mismatch at $($index + 1)"
    Assert-Equal $actual.Name $expected.Name "Active leaderboard name mismatch at $($index + 1)"
    Assert-Equal $actual.Fields $expected.Fields "Active leaderboard field count mismatch at $($index + 1)"
    Assert-Equal $actual.Properties $expected.Properties "Active leaderboard property count mismatch at $($index + 1)"
    Assert-Equal $actual.Total $expected.Total "Active leaderboard total mismatch at $($index + 1)"
}

Write-Output 'PASS: current leaderboard top 20 was independently audited.'
Write-Output 'PASS: 15 current top-20 source groups are retired into 43 non-empty children; 5 homogeneous groups are retained.'
Write-Output 'PASS: all child parents, member totals, audit decisions, and the complete 221-row leaderboard reconcile.'
