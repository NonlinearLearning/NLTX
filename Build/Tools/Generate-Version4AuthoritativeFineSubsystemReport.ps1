[CmdletBinding()]
param(
    [string]$InputPath = 'docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md',
    [string]$SeedReportPath = 'docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md',
    [string]$OutputPath = 'docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md'
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
    $lineNumber = 0
    $inMemberSection = $false

    foreach ($line in Get-Content -LiteralPath $Path) {
        $lineNumber++

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
                throw "Malformed member row at $Path`:$lineNumber. Expected 11 cells, found $($cells.Count)."
            }

            if ([string]::IsNullOrWhiteSpace($parent)) {
                throw "Member row at $Path`:$lineNumber has no parent subsystem."
            }

            if ($RequireFineSubsystem -and [string]::IsNullOrWhiteSpace($fine)) {
                throw "Member row at $Path`:$lineNumber has no fine subsystem."
            }

            $rows.Add([pscustomobject]@{
                    Seq                  = [int]$cells[0]
                    Kind                 = $cells[1]
                    Type                 = $cells[2]
                    RelativePath         = $cells[3]
                    AbsolutePath         = $cells[4]
                    SourceLine           = [int]$cells[5]
                    SourceColumn         = [int]$cells[6]
                    Member               = $cells[7]
                    CSharpType           = $cells[8]
                    Declaration          = $cells[9]
                    OriginalDeclaration  = $cells[10]
                    Cells                = [string[]]$cells
                    Parent               = $parent
                    Fine                 = $fine
                })
        }
    }

    return @($rows)
}

function Get-CellSignature {
    param([Parameter(Mandatory)][pscustomobject]$Row)

    return [string]::Join([char]0x1f, $Row.Cells)
}

function Test-MemberName {
    param(
        [Parameter(Mandatory)][pscustomobject]$Row,
        [Parameter(Mandatory)][string[]]$Members
    )

    return $Members -contains $Row.Member
}

function New-RefinementDefinition {
    param(
        [Parameter(Mandatory)][string]$Parent,
        [Parameter(Mandatory)][string]$SourceFine,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Role,
        [Parameter(Mandatory)][string]$Responsibility,
        [Parameter(Mandatory)][string]$Seam,
        [Parameter(Mandatory)][scriptblock]$Predicate
    )

    return [pscustomobject]@{
        Parent         = $Parent
        SourceFine     = $SourceFine
        Name           = $Name
        Role           = $Role
        Responsibility = $Responsibility
        Seam           = $Seam
        Predicate      = $Predicate
    }
}

$playerVisualMembers = @(
    'dontStarveShader', 'noirShader', 'eyebrellaCloud', 'yoraiz0rEye', 'yoraiz0rDarkness',
    'hasUnicornHorn', 'hasAngelHalo', 'hasRainbowCursor', 'leinforsHair', 'musicBoxSilence',
    'stardustMonolithShader', 'nebulaMonolithShader', 'vortexMonolithShader',
    'solarMonolithShader', 'moonLordMonolithShader', 'bloodMoonMonolithShader',
    'shimmerMonolithShader', 'CRTMonolithShader', 'retroMonolithShader', 'musicBox',
    'overrideFishingBobber'
)

$playerProgressionMembers = @('unlockedBiomeTorches', 'ateArtisanBread', 'unlockedSuperCart', 'enabledSuperCart')

$playerPetMembers = @(
    'suspiciouslookingTentacle', 'crimsonHeart', 'lightOrb', 'blueFairy', 'redFairy', 'greenFairy',
    'bunny', 'turtle', 'eater', 'penguin', 'HasGardenGnomeNearby', 'magicLantern', 'rabid',
    'sunflower', 'wellFed', 'puppy', 'grinch', 'miniMinotaur', 'blackCat', 'spider', 'squashling',
    'companionCube', 'babyFaceMonster', 'dino', 'skeletron', 'hornet', 'zephyrfish', 'snowman',
    'tiki', 'parrot', 'truffle', 'sapling', 'cSapling', 'wisp', 'lizard'
)

$playerMountMembers = @('onWrongGround', 'onTrack', 'cartRampTime', 'cartFlip', 'trackBoost', 'lastBoost', 'mount')

$playerJumpMembers = @('isPerformingPogostickTricks', 'autoJump', 'justJumped', 'jumpSpeedBoost', 'extraFall', 'downDashTime')

$playerEnvironmentMembers = @(
    'canFloatInWater', 'hasFloatingTube', 'frogLegJumpBoost', 'skyStoneEffects', 'spawnMax', 'blockRange',
    'jumpBoost', 'noFallDmg', 'swimTime', 'killGuide', 'killClothier', 'equipmentBasedLuckBonus',
    'lastEquipmentBasedLuckBonus', 'hasCreditsSceneMusicBox', 'lavaImmune', 'gills', 'slowFall',
    'findTreasure', 'biomeSight', 'invis', 'detectCreature', 'nightVision', 'enemySpawns', 'thorns',
    'turtleArmor', 'turtleThorns', 'cactusThorns', 'spiderArmor', 'anglerSetSpawnReduction',
    'vampireBurningInSunlight', 'insideUnbreakableWalls', 'CanSeeInvisibleBlocks', 'honeyCombItem'
)

$playerArmorMembers = @(
    'setSolar', 'setVortex', 'setNebula', 'nebulaCD', 'setStardust', 'setForbidden',
    'setForbiddenCooldownLocked', 'setChlorophyte', 'setSquireT3', 'setHuntressT3',
    'setApprenticeT3', 'setMonkT3', 'setSquireT2', 'setHuntressT2', 'setApprenticeT2',
    'setMonkT2', 'maxTurrets', 'maxTurretsOld', 'vortexStealthActive'
)

$playerGravityMembers = @('waterWalk', 'waterWalk2', 'forcedGravity', 'gravControl', 'gravControl2')

$playerTraversalMembers = @(
    'stairFall', 'outOfRange', 'teleporting', 'teleportTime', 'teleportStyle',
    'unacknowledgedTeleports', 'sloping', 'ropeCount', 'dashType', 'dash', 'dashTime',
    'timeSinceLastDashStarted', 'dashDelay', 'accRunSpeed', 'cordage', 'gem', 'gemCount',
    'ownedLargeGems', 'meleeEnchant', 'pulleyDir', 'pulley', 'pulleyFrame', 'pulleyFrameCounter',
    'sliding', 'slideDir', 'snowBallLauncherInteractionCooldown', 'iceSkate', 'carpet', 'spikedBoots',
    'carpetFrame', 'carpetFrameCounter', 'canCarpet', 'carpetTime', 'powerrun', 'runningOnSand', 'flapSound'
)

$playerCombatMembers = @(
    'lifeSteal', 'ghostDmg', 'eocDash', 'eocHit', 'blackBelt', 'brainOfConfusionItem',
    'brainOfConfusionDodgeAnimationCounter', 'infernoCounter', 'starCloakCooldown', 'iceBarrier',
    'iceBarrierFrame', 'iceBarrierFrameCounter', 'shadowDodge', 'palladiumRegen', 'onHitDodge',
    'onHitRegen', 'onHitPetal', 'onHitTitaniumStorm', 'titaniumStormCooldown', 'hasTitaniumStormBuff',
    'petalTimer', 'shadowDodgeTimer', 'boneGloveTimer', 'phantomPhoneixCounter'
)

$playerAppearanceSlotPattern = '^c[A-Z]'
$playerPortalTargetMembers = @(
    'ownedProjectileCounts', 'npcTypeNoAggro', 'lastPortalColorIndex', '_portalPhysicsTime',
    'portalPhysicsFlag', 'lastTeleportPylonStyleUsed', 'MountFishronSpecialCounter',
    'MinionRestTargetPoint', 'MinionAttackTargetNPC', '_blackListedTileCoordsForGrappling', 'makeStrongBee'
)

$playerItemTimingMembers = @(
    'fallStart', 'fallStart2', 'potionDelayTime', 'restorationDelayTime', 'mushroomDelayTime',
    'itemAnimation', 'itemAnimationMax', 'itemTime', 'itemTimeMax', 'toolTime',
    'BlockInteractionWithProjectiles', 'wireOperationsCooldown'
)

$playerInventoryMembers = @('inventory', 'inventoryChestStack', 'trashItem', 'bank', 'bank2', 'bank3', 'bank4', 'voidVaultInfo')
$playerEquipmentMembers = @('armor', 'dye', 'miscEquips', 'miscDyes')
$playerBuffResourceMembers = @(
    'maxBuffs', 'buffType', 'buffTime', 'buffImmune', 'breathMax', 'breath', 'lavaMax', 'lavaTime',
    'ignoreWater', 'lavaVision', 'lavaOpacity'
)

$npcAiMembers = @('immune', 'directionY', 'type', 'ai', 'localAI', 'aiAction', 'aiStyle', 'justHit', 'timeLeft', 'target', 'oldDirectionY', 'oldTarget', 'netID')
$npcCombatMembers = @(
    'damage', 'defense', 'defDamage', 'defDefense', 'defLifeMax', 'coldDamage', 'boss', 'lavaImmune',
    'value', 'extraValue', 'dontTakeDamage', 'catchableNPCTempImmunityCounter',
    'statsAreScaledForThisManyPlayers', 'difficulty', 'friendly', 'friendlyRegen', 'trapImmune',
    'reflectsProjectiles', 'CommonMasterBossLifeReduction'
)
$npcCollisionMembers = @(
    'targetRect', 'frameCounter', 'frame', 'color', 'alpha', 'hide', 'scale', 'knockBackResist',
    'rotation', 'noGravity', 'noTileCollide', 'collideX', 'collideY', 'spriteDirection', 'behindTiles', 'life', 'lifeMax'
)
$npcTownMembers = @(
    'townNPC', 'nextDialogue', 'travelNPC', 'homeless', 'homelessDespawn', 'lookForHomeTimeout',
    'KickOutLookForHomeTimeout', 'homeTileX', 'homeTileY', 'housingCategory', 'oldHomeless',
    'oldHomeTileX', 'oldHomeTileY', 'closeDoor', 'doorX', 'doorY', 'breath', 'breathMax', 'breathCounter'
)

$npcTownUnlockPattern = '^(saved|bought|unlocked).*$'
$npcTowerMembers = @(
    'ShieldStrengthTowerSolar', 'ShieldStrengthTowerVortex', 'ShieldStrengthTowerNebula',
    'ShieldStrengthTowerStardust', 'LunarShieldPowerNormal', 'TowerActiveSolar', 'TowerActiveVortex',
    'TowerActiveNebula', 'TowerActiveStardust', 'LunarApocalypseIsUp'
)

$worldSecretSeedTypes = @('Terraria.WorldGen.SecretSeed')
$worldVariationTypes = @('Terraria.WorldGen.SecretSeed.Variations')
$worldSkyblockTypes = @('Terraria.WorldGen.Skyblock')

$playerControlAndReleaseMembers = @(
    'controlLeft', 'controlRight', 'controlUp', 'controlDown', 'controlJump', 'controlTorch', 'controlDash',
    'releaseJump', 'releaseUp', 'releaseUseItem', 'releaseUseTile', 'releaseLeft', 'releaseRight',
    'releaseDown', 'releaseDash', 'controlDownHold', 'tryKeepingHoveringDown', 'tryKeepingHoveringUp',
    'leftTimer', 'rightTimer'
)

$playerItemUseIntentMembers = @(
    'controlUseItem', 'controlUseTile', 'tileInteractAttempted', 'isOperatingAnotherEntity',
    'lastItemUseAttemptSuccess', 'autoReuseAllWeapons', 'altFunctionUse', 'delayUseItem', 'manaCost',
    'fireWalk', 'channel', 'TagEffectState', 'IntentionGuesser', '_channelShotCache', 'rabbitOrderFrame',
    'creativeGodMode'
)

$playerShadowAndArmMembers = @(
    'cursorItemIconReversed', 'runSoundDelay', 'shadowPos', 'shadowRotation', 'shadowOrigin',
    'shadowDirection', 'shadowCount', 'skipAnimatingValuesInPlayerFrame', 'availableAdvancedShadowsCount',
    '_advancedShadows', '_lastAddedAvancedShadow', 'compositeFrontArm', 'compositeBackArm'
)

$playerQuestAndEventMembers = @('anglerQuestsFinished', 'golferScoreAccumulated', 'downedDD2EventAnyDifficulty')

$playerAppearanceCustomizationMembers = @(
    'hairDye', 'skinDyePacked', 'hairColor', 'skinColor', 'eyeColor', 'shirtColor', 'underShirtColor',
    'pantsColor', 'shoeColor', 'hair'
)

$playerInformationAccessoryMembers = @(
    'hostile', 'hermesStepSound', 'instantMovementAccumulatedThisFrame', 'accCompass', 'accWatch',
    'accWatchTime', 'accDepthMeter', 'accFishFinder', 'accWeatherRadio', 'accJarOfSouls', 'accCalendar',
    'lastCreatureHit', 'accThirdEye', 'accThirdEyeCounter', 'accStopwatch', 'accOreFinder', 'accCritterGuide',
    'accDreamCatcher', 'hasFootball', 'drawingFootball', 'ActuationRodLock', 'InfoAccMechShowWires'
)

$playerDpsTelemetryMembers = @('dpsStart', 'dpsEnd', 'dpsLastHit', 'dpsDamage', 'dpsStarted')

$playerLuckAndCommerceMembers = @(
    'discountEquipped', 'discountAvailable', 'hasLuckyCoin', 'boneGloveItem', 'goldRing', 'accDivingHelm',
    'accFlipper', 'deadCellsPotionStation', 'hasLuck_LuckyCoin', 'hasLuck_LuckyHorseshoe',
    'hasLuck_LuckyClover', 'hasLuck_WiltedClover', 'hasLuck_RavenFeather'
)

$playerFishingCapabilityMembers = @(
    'fishingSkill', 'cratePotion', 'sonarPotion', 'accFishingLine', 'accFishingBobber', 'accTackleBox',
    'accLavaFishing'
)

$playerMinionCapacityAndSummonMembers = @(
    'maxMinions', 'numMinions', 'slotsMinions', 'pygmy', 'raven', 'slime', 'hornetMinion', 'impMinion',
    'twinsMinion', 'spiderMinion', 'pirateMinion', 'sharknadoMinion', 'UFOMinion', 'DeadlySphereMinion',
    'stardustMinion', 'stardustGuardian', 'stardustDragon', 'batsOfLight', 'babyBird', 'vampireFrog',
    'stormTiger', 'highestStormTigerGemOriginalDamage', 'smolstar', 'empressBlade', 'flinxMinion',
    'abigailMinion', 'highestAbigailCounterOriginalDamage', 'deadCellsMushroomBoiMinion', 'palworldCattivaMinion',
    'palworldFoxsparksMinion'
)

$playerTeleportTransitionMembers = @('teleporting', 'teleportTime', 'teleportStyle', 'unacknowledgedTeleports')

$playerDashAndGroundTraversalMembers = @(
    'stairFall', 'outOfRange', 'sloping', 'dashType', 'dash', 'dashTime', 'timeSinceLastDashStarted',
    'dashDelay', 'accRunSpeed', 'powerrun', 'runningOnSand', 'flapSound'
)

$playerRopeAndPulleyMembers = @(
    'ropeCount', 'cordage', 'gem', 'gemCount', 'ownedLargeGems', 'meleeEnchant', 'pulleyDir', 'pulley',
    'pulleyFrame', 'pulleyFrameCounter'
)

$playerSlideAndCarpetMembers = @(
    'sliding', 'slideDir', 'snowBallLauncherInteractionCooldown', 'iceSkate', 'carpet', 'spikedBoots',
    'carpetFrame', 'carpetFrameCounter', 'canCarpet', 'carpetTime'
)

$playerEnvironmentMobilityMembers = @(
    'canFloatInWater', 'hasFloatingTube', 'frogLegJumpBoost', 'skyStoneEffects', 'spawnMax', 'blockRange',
    'jumpBoost', 'noFallDmg', 'swimTime', 'lavaImmune', 'gills', 'slowFall'
)

$playerEnvironmentDetectionMembers = @(
    'killGuide', 'killClothier', 'equipmentBasedLuckBonus', 'lastEquipmentBasedLuckBonus',
    'hasCreditsSceneMusicBox', 'findTreasure', 'biomeSight', 'invis', 'detectCreature', 'nightVision',
    'enemySpawns', 'insideUnbreakableWalls', 'CanSeeInvisibleBlocks'
)

$playerArmorAndCombatEffectMembers = @(
    'thorns', 'turtleArmor', 'turtleThorns', 'cactusThorns', 'spiderArmor', 'anglerSetSpawnReduction',
    'vampireBurningInSunlight', 'honeyCombItem'
)

$playerWingsAndFlightMembers = @('wingTime', 'wings', 'wingsLogic', 'wingTimeMax', 'wingFrame', 'wingFrameCounter')

$playerZoneAndEnvironmentMembers = @(
    'environmentBuffImmunityTimer', 'zone1', 'zone2', 'zone3', 'zone4', 'zone5', '_wasInShimmerZone'
)

$playerSocialAndDefenseMembers = @(
    'skinVariant', 'voiceVariant', 'voicePitchOffset', 'ghost', 'ghostFrame', 'ghostFrameCounter',
    '_framesLeftEligibleForDeadmansChestDeathAchievement', 'pvpDeath', 'boneArmor', 'frostArmor', 'honey',
    'crystalLeaf', 'crystalLeafCooldown', 'portableStoolInfo', 'preventAllItemPickups', 'dontHurtCritters',
    'hasLucyTheAxe', 'dontHurtNature', 'defendedByPaladin', 'hasPaladinShield'
)

$playerPoseAndAnimationMembers = @(
    'headRotation', 'bodyRotation', 'legRotation', 'headPosition', 'bodyPosition', 'legPosition',
    'headVelocity', 'bodyVelocity', 'legVelocity', 'fullRotation', 'fullRotationOrigin', 'fartKartCloudDelay',
    'gfxOffY', 'stepSpeed'
)

$playerNetworkCameraMembers = @('netOffset', 'netCameraTarget', 'lastSyncedNetCameraTarget')

$playerDeathRespawnAndSaveMembers = @(
    'dead', 'deadTime', 'spectating', 'respawnTimer', 'respawnTimerMax', 'DeadSpectatingLockoutTime',
    'SpectatingLingerAfterDeath', 'lastTimePlayerWasSaved', 'attackCD', 'potionDelay', 'difficulty', 'wetSlime',
    'hitTile', 'hitReplace'
)

$playerVitalAndRegenMembers = @(
    'statLifeMax', 'statLifeMax2', 'statLife', 'statMana', 'statManaMax', 'statManaMax2', 'lifeRegen',
    'lifeRegenCount', 'lifeRegenTime', 'manaRegen', 'manaRegenCount', 'manaRegenDelay', 'manaRegenBuff'
)

$playerCombatModifierAndImmunityMembers = @(
    'armorPenetration', 'meleeArmorPenetration', 'statDefense', 'noKnockback', 'shimmerImmune', 'spaceGun',
    'gravDir', 'chaosState', 'strongBees', 'sporeSac', 'shinyStone', 'empressBrooch', 'volatileGelatin',
    'volatileGelatinCounter', 'hasMagiluminescence', 'shadowArmor'
)

$playerAmmoAndAccessoryEffectMembers = @(
    'chloroAmmoCost80', 'huntressAmmoCost90', 'ammoCost80', 'ammoCost75', 'stickyBreak', 'magicQuiver',
    'magmaStone', 'lavaRose', 'hasMoltenQuiver', 'phantasmTime', 'ammoBox', 'ammoPotion'
)

$npcIdentityInteractionMembers = @(
    'active', 'NPC_TARGETS_START', 'IsABestiaryIconDummy', 'IsAPortraitDummy', 'ForcePartyHatOn',
    'nameOverIncrement', 'nameOverDistance', 'nameOver', 'altTexture', 'townNpcVariationIndex', 'catchItem',
    'releaseOwner', 'rarity', 'taxCollector', 'playerInteraction', 'lastInteraction', 'takenDamageMultiplier', 'freeCake'
)

$npcTargetAndMovementMembers = @(
    'waterMovementSpeed', 'lavaMovementSpeed', 'honeyMovementSpeed', 'shimmerMovementSpeed', 'teleportStyle',
    'teleportTime', 'gfxOffY', 'stepSpeed', 'gravity', 'teleporting', 'stairFall', 'oldPos', 'oldRot', 'setFrameSize'
)

$npcBossAndInvasionMembers = @(
    'MoonLordAttacksArray', 'MoonLordAttacksArray2', 'MoonLordFightingDistance', 'MoonLordCountdown',
    'MaxMoonLordCountdown', 'NaturalMoonlordCountdownTime', 'ItemMoonlordCountdownTime', 'totalInvasionPoints',
    'waveKills', 'waveNumber'
)

$npcSpawnAndCritterMembers = @(
    'maxAI', 'goldCritterChance', 'SpawnedFromStatue', 'CanBeReplacedByOtherNPCs', 'dripping', 'drippingSlime',
    'drippingSparkleSlime', 'ShimmeredTownNPCs', 'fireFlyFriendly', 'fireFlyChance', 'fireFlyMultiple',
    'butterflyChance', 'stinkBugChance'
)

$npcBuffSlotAndImmunityMembers = @('buffType', 'buffTime', 'buffImmune', 'canDisplayBuffs')

$npcStatusEffectAndRegenMembers = @(
    'midas', 'ichor', 'brokenArmor', 'onFire', 'onFire2', 'onFire3', 'onFrostBurn', 'onFrostBurn2', 'poisoned',
    'venom', 'tipsy', 'bleeding', 'hemorrhage', 'markedByScytheWhip', 'markedByEelWhip', 'shadowFlame',
    'soulDrain', 'shimmering', 'lifeRegen', 'lifeRegenCount', 'lifeRegenExpectedLossPerSecond', 'confused',
    'loveStruck', 'stinky', 'dryadWard', 'immortal', 'chaseable', 'canGhostHeal', 'javelined', 'tentacleSpiked',
    'bloodButchered', 'celled', 'dryadBane', 'daybreak', 'dontTakeDamageFromHostiles', 'betsysCurse', 'oiled',
    'electricEelCounter'
)

$projectileIdentityAndClassificationMembers = @(
    'active', 'perIDStaticNPCImmunity', 'ownerHitCheckDistance', 'arrow', 'numHits', 'bobber', 'netImportant',
    'noDropItem', 'counterweight', 'scale', 'rotation', 'type', 'alpha', 'sentry', 'glowMask', 'owner'
)

$projectileAiMembers = @('maxAI', 'ai', 'localAI', 'aiStyle')
$projectileLifetimeAndRuntimeMembers = @('SentryLifeTime', 'ArrowLifeTime', 'gfxOffY', 'stepSpeed', 'timeLeft', 'soundDelay')

$projectileMovementAndCollisionMembers = @(
    'oldPos', 'oldRot', 'oldSpriteDirection', 'restrikeDelay', 'tileCollide', 'extraUpdates',
    'stopsDealingDamageAfterPenetrateHits', 'numUpdates', 'ignoreWater'
)

$projectileNetworkReplicationMembers = @('netUpdate', 'netUpdate2', 'netSpam', 'netSyncSkippedForPlayer')

$projectileMinionAndPresentationMembers = @(
    'light', 'minion', 'minionSlots', 'minionPos', 'isAPreviewDummy', 'isAPreviewDisplayDoll', 'MinionSpawnInfo',
    'drawLayer', 'usesOwnerLight', 'hide', 'ownerHitCheck', 'usesOwnerMeleeHitCD', 'playerImmune'
)

$projectileDamageAndElementMembers = @(
    'miscText', 'melee', 'ranged', 'magic', 'coldDamage', 'noEnchantments', 'noEnchantmentVisuals', 'trap',
    'npcProj', 'originatedFromActivableTile', 'tagEffectType', 'bonusTagDamage', 'armorPenetration'
)

$projectileAnimationAndDirectionMembers = @('frameCounter', 'frame', 'manualDirectionChange')
$projectileCollisionAndTargetingMembers = @(
    'projUUID', 'correctSlopeCollision', 'decidesManualFallThrough', 'shouldFallThrough', 'localNPCHitCooldown',
    'idStaticNPCHitCooldown', 'bannerIdToRespondTo'
)

$playerLegacyPetMembers = @(
    'suspiciouslookingTentacle', 'crimsonHeart', 'lightOrb', 'blueFairy', 'redFairy', 'greenFairy',
    'bunny', 'turtle', 'eater', 'penguin', 'HasGardenGnomeNearby', 'magicLantern', 'rabid', 'sunflower',
    'wellFed', 'puppy', 'grinch', 'miniMinotaur', 'blackCat', 'spider', 'squashling'
)
$playerCompanionMembers = @(
    'companionCube', 'babyFaceMonster', 'snowman', 'dino', 'skeletron', 'hornet', 'zephyrfish',
    'tiki', 'parrot', 'truffle', 'sapling', 'cSapling', 'wisp', 'lizard'
)

$playerElementalAndShimmerStatusMembers = @(
    'archery', 'poisoned', 'venom', 'blind', 'blackout', 'headcovered', 'frostBurn', 'onFrostBurn',
    'onFrostBurn2', 'burned', 'shimmering', 'timeShimmering', 'shimmerTransparency', 'shimmerUnstuckHelper',
    'suffocating', 'dripping', 'drippingSlime', 'drippingSparkleSlime', 'onFire', 'onFire2', 'onFire3'
)
$playerSurvivalAndControlStatusMembers = @(
    'noItems', 'cursed', 'hungry', 'starving', 'heartyMeal', 'windPushed', 'wereWolf', 'wolfAcc', 'hideMerman',
    'hideWolf', 'forceMerman', 'forceWerewolf', 'sunScorchCounter', 'rulerGrid', 'rulerLine', 'bleed', 'confused',
    'accMerman', 'merman', 'trident', 'brokenArmor', 'silence', 'slow', 'gross', 'tongued'
)
$playerAccessoryAndCombatStatusMembers = @(
    'kbGlove', 'autoReuseGlove', 'meleeScaleGlove', 'kbBuff', 'remoteVisionForDrone', 'starCloakItem',
    'starCloakItem_manaCloakOverrideItem', 'starCloakItem_starVeilOverrideItem', 'starCloakItem_beeCloakOverrideItem',
    'longInvince', 'pStone', 'PhilosopherStoneDurationMultiplier', 'manaFlower', 'moonLeech', 'vortexDebuff',
    'trapDebuffSource', 'witheredArmor', 'witheredWeapon', 'slowOgreSpit', 'parryDamageBuff', 'ballistaPanic',
    'JustDroppedAnItem'
)

$mountGeometryAndFrameMembers = @(
    'textureWidth', 'textureHeight', 'xOffset', 'yOffset', 'playerYOffsets', 'bodyFrame', 'playerHeadOffset',
    'heightBoost', 'totalFrames', 'standingFrameStart', 'standingFrameCount', 'standingFrameDelay',
    'runningFrameStart', 'runningFrameCount', 'runningFrameDelay', 'flyingFrameStart', 'flyingFrameCount',
    'flyingFrameDelay', 'inAirFrameStart', 'inAirFrameCount', 'inAirFrameDelay', 'idleFrameStart',
    'idleFrameCount', 'idleFrameDelay', 'idleFrameLoop', 'swimFrameStart', 'swimFrameCount', 'swimFrameDelay',
    'dashingFrameStart', 'dashingFrameCount', 'dashingFrameDelay', 'playerXOffset'
)
$mountMovementAndAbilityMembers = @(
    'flightTimeMax', 'usesHover', 'runSpeed', 'dashSpeed', 'swimSpeed', 'acceleration', 'jumpSpeed', 'jumpHeight',
    'fallDamage', 'extraFall', 'fatigueMax', 'constantJump', 'blockExtraJumps', 'abilityChargeMax', 'abilityDuration',
    'abilityCooldown', 'walkingGraceTimeMax', 'dismountsOnItemUse'
)
$mountVehicleAndPresentationMembers = @(
    'buff', 'spawnDust', 'spawnDustNoGravity', 'Minecart', 'CanRideMinecartTracks', 'CanUseWings', 'lightColor',
    'emitsLight', 'delegations'
)

$npcSpawnContextAndCapacityMembers = @(
    'spawnSpaceX', 'spawnSpaceY', 'fairyLog', 'numberOfActivePlayers', 'reachedInvasionBossCap', 'pX', 'pY',
    'luck', 'dayTime', 'raining'
)
$npcSpawnEnvironmentEligibilityMembers = @(
    'townNPCs', 'skyMob', 'noWorms', 'noGroundWorms', 'invaders', 'spawnFriendly', 'ignoreSafeWalls', 'waterTile',
    'nearGranite', 'nearMarble', 'spawnSpider', 'surfaceSpawn', 'spawnUndergroundDesert', 'hardDungeon',
    'deeperThanRockLayer', 'underGround', 'isOcean', 'isBeach', 'isSpawningInWindDirection', 'skyBehindPlayer',
    'livingTree', 'dualDungeonsSpawnRules', 'inDualDungeon', 'tresspassingDualDungeon', 'inRemixStartingArea',
    'offensiveToTim', 'playerHasStartingHealth'
)
$npcSpawnZoneAndEventMembers = @(
    'ZoneCorrupt', 'ZoneCrimson', 'ZoneHallow', 'ZoneJungle', 'ZoneSnow', 'ZoneGlowshroom', 'ZoneMeteor',
    'ZoneGraveyard', 'ZoneDungeon', 'ZoneLihzhardTemple', 'ZoneGranite', 'ZoneMarble', 'ZoneSandstorm',
    'ZoneTowerSolar', 'ZoneTowerVortex', 'ZoneTowerNebula', 'ZoneTowerStardust', 'ZoneOldOneArmy',
    'ZoneWaterCandle', 'ZonePeaceCandle', 'ZoneShadowCandle', 'defaultTarget'
)

$genVarsConfigurationAndOreMembers = @(
    'configuration', 'structures', 'copper', 'iron', 'silver', 'gold', 'copperBar', 'ironBar', 'silverBar', 'goldBar'
)
$genVarsWorldLayerAndSurfaceMembers = @(
    'worldSpawnHasBeenRandomized', 'landmassData', 'remixSurfaceLayerLow', 'remixSurfaceLayerHigh',
    'remixMushroomLayerLow', 'remixMushroomLayerHigh', 'lowestCloud', 'boulderPetsPlaced', 'crimStoneWall',
    'crimStone', 'ebonStoneWall', 'ebonStone', 'mossTile', 'mossWall', 'lavaLine', 'waterLine', 'worldSurfaceLow',
    'worldSurface', 'worldSurfaceHigh', 'rockLayerLow', 'rockLayer', 'rockLayerHigh', 'snowTop', 'snowBottom',
    'snowOriginLeft', 'snowOriginRight', 'snowMinX', 'snowMaxX'
)
$genVarsBeachAndOceanBoundaryMembers = @(
    'leftBeachEnd', 'rightBeachStart', 'beachBordersWidth', 'beachSandRandomCenter', 'beachSandRandomWidthRange',
    'beachSandDungeonExtraWidth', 'beachSandJungleExtraWidth', 'shellStartXLeft', 'shellStartYLeft',
    'shellStartXRight', 'shellStartYRight', 'oceanWaterStartRandomMin'
)

$worldSecretSeedDefinitionMembers = @(
    'AllSecretSeeds', 'paintEverythingGray', 'paintEverythingNegative', 'coatEverythingEcho',
    'coatEverythingIlluminant', 'noSurface', 'extraLivingTrees', 'extraFloatingIslands', 'errorWorld',
    'graveyardBloodmoonStart', 'surfaceIsInSpace', 'rainsForAYear', 'biggerAbandonedHouses', 'randomSpawn',
    'addTeleporters', 'startInHardmode', 'noInfection', 'hallowOnTheSurface', 'worldIsInfected',
    'surfaceIsMushrooms', 'surfaceIsDesert', 'pooEverywhere', 'noSpiderCaves', 'actuallyNoTraps', 'rainbowStuff',
    'digExtraHoles', 'roundLandmasses', 'extraLiquid', 'portalGunInChests', 'worldIsFrozen', 'halloweenGen',
    'endlessHalloween', 'endlessChristmas', 'vampirism', 'teamBasedSpawns', 'dualDungeons', 'Localization',
    '_code', '_sound', '_plaintext', 'TextThatWasUsedToUnlock'
)
$worldSecretSeedRuntimeMembers = @('activeSecretSeedCount', '_enabled', 'Enabled')
$worldSecretSeedDerivedOptionMembers = @('GenerateBiggerAbandonedHouses', 'GenerateRainbowGlowsticks')

$playerStringAndAccessoryEffectMembers = @(
    'extraAccessorySlots', 'extraAccessory', 'tankPet', 'tankPetReset', 'stringColor', 'counterWeight',
    'vanityCounterWeight', 'magicString', 'yoyoString', 'yoyoGlove', 'rapidAttackBonus', 'stressBall',
    'stressBallPrevious', 'staffOfRegrowthBonus'
)
$playerBeetleArmorMembers = @(
    'beetleOrbs', 'beetleCounter', 'beetleCountdown', 'beetleDefense', 'beetleOffense', 'beetleBuff',
    'beetlePos', 'beetleVel', 'beetleFrame', 'beetleFrameCounter'
)
$playerSolarAndNebulaArmorMembers = @(
    'solarShields', 'solarCounter', 'solarShieldPos', 'solarShieldVel', 'solarDashing', 'solarDashConsumedFlare',
    'nebulaLevelLife', 'nebulaLevelMana', 'nebulaManaCounter', 'nebulaLevelDamage'
)
$playerMagnetAndUtilityAccessoryMembers = @(
    'manaMagnet', 'lifeMagnet', 'treasureMagnet', 'chiselSpeed', 'lifeForce', 'hasDeadCellsDownDash', 'calmed',
    'inferno'
)

$worldGenerationShapeModifierTypes = @(
    'Terraria.WorldBuilding.Modifiers.Blotches', 'Terraria.WorldBuilding.Modifiers.Checkerboard',
    'Terraria.WorldBuilding.Modifiers.Dither', 'Terraria.WorldBuilding.Modifiers.Expand',
    'Terraria.WorldBuilding.Modifiers.Flip', 'Terraria.WorldBuilding.Modifiers.InShape',
    'Terraria.WorldBuilding.Modifiers.NotInShape', 'Terraria.WorldBuilding.Modifiers.Offset',
    'Terraria.WorldBuilding.Modifiers.RadialDither', 'Terraria.WorldBuilding.Modifiers.RectangleMask',
    'Terraria.WorldBuilding.Modifiers.ShapeScale'
)
$worldGenerationTileWallConditionTypes = @(
    'Terraria.WorldBuilding.Modifiers.Conditions', 'Terraria.WorldBuilding.Modifiers.HasLiquid',
    'Terraria.WorldBuilding.Modifiers.IsAboveHeight', 'Terraria.WorldBuilding.Modifiers.IsBelowHeight',
    'Terraria.WorldBuilding.Modifiers.IsTouching', 'Terraria.WorldBuilding.Modifiers.IsTouchingAir',
    'Terraria.WorldBuilding.Modifiers.NoLiquid', 'Terraria.WorldBuilding.Modifiers.NotTouching',
    'Terraria.WorldBuilding.Modifiers.OnlyTiles', 'Terraria.WorldBuilding.Modifiers.OnlyWalls',
    'Terraria.WorldBuilding.Modifiers.SkipTiles', 'Terraria.WorldBuilding.Modifiers.SkipWalls'
)

$worldHousingCountersAndScoringMembers = @(
    'prioritizedTownNPCType', 'numTileCount', 'maxTileCount', 'maxWallOut2', 'CountedTiles', 'lavaCount',
    'iceCount', 'sandCount', 'rockCount', 'shroomCount', 'maxRoomTiles', 'maxRoomSize', 'roomTiles',
    'numRoomTiles', 'hiScore'
)
$worldHousingRoomSearchMembers = @(
    'roomX1', 'roomX2', 'roomY1', 'roomY2', 'canSpawn', 'houseTile', 'bestX', 'bestY', 'roomTorch', 'roomDoor',
    'roomChair', 'roomTable', 'roomHasStinkbug', 'roomHasEchoStinkbug', 'LastFoundHouse',
    'currentlyTryingToUseAlternateHousingSpot', 'sharedRoomX', '_roomCheckStack', 'roomCheckFailureReason'
)
$worldHousingRuleAndDiagnosticMembers = @(
    'WorldGenParam_Evil', 'cactusWaterWidth', 'cactusWaterHeight', 'cactusWaterLimit', 'mysticLogsEvent'
)

$npcStatusEffectFlagMembers = @(
    'midas', 'ichor', 'brokenArmor', 'onFire', 'onFire2', 'onFire3', 'onFrostBurn', 'onFrostBurn2', 'poisoned',
    'venom', 'tipsy', 'bleeding', 'hemorrhage', 'markedByScytheWhip', 'markedByEelWhip', 'shadowFlame', 'soulDrain',
    'shimmering', 'confused', 'loveStruck', 'stinky', 'dryadWard', 'javelined', 'tentacleSpiked', 'bloodButchered',
    'celled', 'dryadBane', 'daybreak', 'betsysCurse', 'oiled'
)
$npcRegenerationAndProtectionMembers = @(
    'lifeRegen', 'lifeRegenCount', 'lifeRegenExpectedLossPerSecond', 'immortal', 'chaseable', 'canGhostHeal',
    'dontTakeDamageFromHostiles', 'electricEelCounter'
)

$playerAppearanceEquipmentProjectionMembers = @(
    'cHead', 'cBody', 'cLegs', 'cHandOn', 'cHandOff', 'cBack', 'cFront', 'cShoe', 'cWaist', 'cShield', 'cNeck',
    'cFace', 'cFaceHead', 'cFaceFlower', 'cFaceMask', 'cBalloon', 'cBalloonFront', 'cWings', 'cCarpet',
    'cFloatingTube', 'cBackpack', 'cTail', 'cShieldFallback', 'cGrapple', 'cMount', 'cMinecart'
)
$playerAppearanceCompanionAndEffectProjectionMembers = @(
    'cPet', 'cLight', 'cYorai', 'cPortableStool', 'cUnicornHorn', 'cAngelHalo', 'cBeard', 'cMinion',
    'cLeinShampoo', 'cFlameWaker', 'cCoat'
)

$playerManaAndAfkStatusMembers = @(
    'manaSickTime', 'manaSickLessDmg', 'manaSickReduction', 'manaSick', 'afkCounter',
    'AFKTimeNeededForNoWormSpawns', 'AFKTimeNeededForNoLuckyStars', 'afkCounterForKiting', 'manaRegenBonus',
    'manaRegenDelayBonus'
)
$playerDebuffAndRecoveryStatusMembers = @(
    'chilled', 'dazed', 'frozen', 'stoned', 'ichor', 'webbed', 'tipsy', 'noBuilding', 'miscCounter', 'sandStorm',
    'crimsonRegen', 'ghostHeal', 'ghostHurt', 'sticky', 'slippy', 'slippy2'
)
$playerDetectionAndCombatStatusMembers = @(
    'dangerSense', 'luckPotion', 'endurance', 'whipRangeMultiplier', 'whipUseTimeMultiplier', 'loveStruck',
    'stinky', 'resistCold', 'electrified', 'dryadWard', 'panic'
)

$worldGenerationTileMutationTypes = @(
    'Terraria.WorldBuilding.Actions.ClearTile', 'Terraria.WorldBuilding.Actions.ClearWall',
    'Terraria.WorldBuilding.Actions.HalfBlock', 'Terraria.WorldBuilding.Actions.PlaceTile',
    'Terraria.WorldBuilding.Actions.PlaceWall', 'Terraria.WorldBuilding.Actions.SetHalfTile',
    'Terraria.WorldBuilding.Actions.SetLiquid', 'Terraria.WorldBuilding.Actions.SetSlope',
    'Terraria.WorldBuilding.Actions.SetTile', 'Terraria.WorldBuilding.Actions.SetTileAndWallPaint',
    'Terraria.WorldBuilding.Actions.SetTileKeepWall', 'Terraria.WorldBuilding.Actions.SetTilePaint',
    'Terraria.WorldBuilding.Actions.SetWall', 'Terraria.WorldBuilding.Actions.SetWallPaint',
    'Terraria.WorldBuilding.Actions.Smooth', 'Terraria.WorldBuilding.Actions.SwapSolidTile'
)
$worldGenerationTileScanAndControlTypes = @(
    'Terraria.WorldBuilding.Actions.ContinueWrapper', 'Terraria.WorldBuilding.Actions.Count',
    'Terraria.WorldBuilding.Actions.Custom', 'Terraria.WorldBuilding.Actions.Scanner',
    'Terraria.WorldBuilding.Actions.TileScanner', 'Terraria.WorldBuilding.Actions.UpdateBounds'
)
$worldGenerationTileFramingAndDebugTypes = @(
    'Terraria.WorldBuilding.Actions.DebugDraw', 'Terraria.WorldBuilding.Actions.SetFrames'
)

$worldGenBeachAndOceanBiomeMembers = @(
    'oceanWaterStartRandomMax', 'oceanWaterForcedJungleLength', 'evilBiomeBeachAvoidance',
    'evilBiomeAvoidanceMidFixer', 'lakesBeachAvoidance', 'smallHolesBeachAvoidance', 'surfaceCavesBeachAvoidance',
    'surfaceCavesBeachAvoidance2', 'maxOceanCaveTreasure', 'numOceanCaveTreasure', 'oceanCaveTreasure',
    'skipDesertTileCheck'
)
$worldGenUndergroundDesertStructureMembers = @(
    'UndergroundDesertLocation', 'UndergroundDesertHiveLocation', 'desertHiveHigh', 'desertHiveLow',
    'desertHiveLeft', 'desertHiveRight', 'numLarva', 'larvaY', 'larvaX'
)
$worldGenJungleStructureMembers = @(
    'numPyr', 'PyrX', 'PyrY', 'extraBastStatueCount', 'extraBastStatueCountMax', 'jungleOriginX', 'jungleMinX',
    'jungleMaxX', 'jungleHut', 'mudWall', 'JungleItemCount', 'gennedLivingMahoganyWands', 'JChestX', 'JChestY',
    'numJChests'
)

$npcProgressionBookAndActiveRegistryMembers = @(
    'combatBookWasUsed', 'combatBookVolumeTwoWasUsed', 'peddlersSatchelWasUsed', 'npcsFoundForCheckActive'
)
$npcBossDefeatProgressionMembers = @(
    'downedBoss1', 'downedBoss2', 'downedBoss3', 'downedQueenBee', 'downedSlimeKing', 'downedGoblins',
    'downedFrost', 'downedPirates', 'downedClown', 'downedPlantBoss', 'downedGolemBoss', 'downedMartians',
    'downedFishron', 'downedHalloweenTree', 'downedHalloweenKing', 'downedChristmasIceQueen', 'downedChristmasTree',
    'downedChristmasSantank', 'downedAncientCultist', 'downedMoonlord', 'downedTowerSolar', 'downedTowerVortex',
    'downedTowerNebula', 'downedTowerStardust', 'downedEmpressOfLight', 'downedQueenSlime', 'downedDeerclops',
    'downedMechBossAny', 'downedMechBoss1', 'downedMechBoss2', 'downedMechBoss3'
)
$worldGenBiomeBackgroundAndDistanceMembers = @(
    'TownManager', 'Manifest', 'tileReframeCount', 'treeBG1', 'treeBG2', 'treeBG3', 'treeBG4', 'corruptBG',
    'jungleBG', 'snowBG', 'hallowBG', 'crimsonBG', 'desertBG', 'oceanBG', 'mushroomBG', 'underworldBG',
    'oceanDistance', 'beachDistance', 'shimmerSafetyDistance', 'crimson', 'generatingRandomEvil'
)
$worldGenTileCountMetricMembers = @(
    'tileCounts', 'totalEvil', 'totalBlood', 'totalGood', 'totalSolid', 'totalEvil2', 'totalBlood2', 'totalGood2',
    'totalSolid2', 'tEvil', 'tBlood', 'tGood', 'totalX', 'totalD'
)

$playerIdentityAndDerivedPropertyMembers = @('miscCounterNormalized', 'Male')
$playerBiomeZonePropertyMembers = @(
    'ZoneDungeon', 'ZoneCorrupt', 'ZoneHallow', 'ZoneMeteor', 'ZoneJungle', 'ZoneSnow', 'ZoneCrimson',
    'ZoneWaterCandle', 'ZonePeaceCandle', 'ZoneTowerSolar', 'ZoneTowerVortex', 'ZoneTowerNebula',
    'ZoneTowerStardust', 'ZoneDesert', 'ZoneGlowshroom', 'ZoneUndergroundDesert'
)
$playerVerticalAndWeatherZonePropertyMembers = @(
    'ZoneSkyHeight', 'ZoneOverworldHeight', 'ZoneUnderworldHeight', 'ZoneBeach', 'ZoneRain', 'ZoneSandstorm'
)
$playerEventAndShoppingZonePropertyMembers = @(
    'ZoneOldOneArmy', 'ZoneLihzhardTemple', 'ZoneGraveyard', 'ZoneShadowCandle', 'ZoneShimmer',
    'ShoppingZone_AnyBiome', 'ShoppingZone_BelowSurface'
)

$secondRefinementLeaderboard = @(
    [pscustomobject]@{ Rank = 1; Name = 'PlayerPetAndCompanionState'; Fields = 90; Properties = 0; Total = 90 }
    [pscustomobject]@{ Rank = 2; Name = 'PlayerBuffAndStatusEffects'; Fields = 68; Properties = 0; Total = 68 }
    [pscustomobject]@{ Rank = 3; Name = 'MountDefinitionCatalog'; Fields = 59; Properties = 0; Total = 59 }
    [pscustomobject]@{ Rank = 4; Name = 'NpcSpawnEligibilityInputs'; Fields = 59; Properties = 0; Total = 59 }
    [pscustomobject]@{ Rank = 5; Name = 'GenVarsConfigurationAndTerrainLayers'; Fields = 50; Properties = 0; Total = 50 }
    [pscustomobject]@{ Rank = 6; Name = 'WorldSecretSeedRegistryState'; Fields = 43; Properties = 3; Total = 46 }
    [pscustomobject]@{ Rank = 7; Name = 'PlayerEquipmentAndAccessoryEffects'; Fields = 42; Properties = 0; Total = 42 }
    [pscustomobject]@{ Rank = 8; Name = 'WorldGenerationModifiersAndActions'; Fields = 42; Properties = 0; Total = 42 }
    [pscustomobject]@{ Rank = 9; Name = 'WorldHousingAndSpawnRules'; Fields = 39; Properties = 0; Total = 39 }
    [pscustomobject]@{ Rank = 10; Name = 'NpcStatusEffectAndRegenState'; Fields = 38; Properties = 0; Total = 38 }
    [pscustomobject]@{ Rank = 11; Name = 'PlayerAppearanceProjectionSlots'; Fields = 37; Properties = 0; Total = 37 }
    [pscustomobject]@{ Rank = 12; Name = 'PlayerStatusAndDebuffState'; Fields = 37; Properties = 0; Total = 37 }
    [pscustomobject]@{ Rank = 13; Name = 'WorldGenerationConfigurationAndOptions'; Fields = 7; Properties = 30; Total = 37 }
    [pscustomobject]@{ Rank = 14; Name = 'WorldGenerationTileActions'; Fields = 37; Properties = 0; Total = 37 }
    [pscustomobject]@{ Rank = 15; Name = 'WorldTerrainProfilesAndOreTiers'; Fields = 36; Properties = 1; Total = 37 }
    [pscustomobject]@{ Rank = 16; Name = 'GenVarsBiomeStructures'; Fields = 36; Properties = 0; Total = 36 }
    [pscustomobject]@{ Rank = 17; Name = 'NpcBossAndWorldProgressionFlags'; Fields = 35; Properties = 0; Total = 35 }
    [pscustomobject]@{ Rank = 18; Name = 'WorldGenBiomeMetricsAndCounts'; Fields = 35; Properties = 0; Total = 35 }
    [pscustomobject]@{ Rank = 19; Name = 'PlayerJumpVariantState'; Fields = 34; Properties = 0; Total = 34 }
    [pscustomobject]@{ Rank = 20; Name = 'PlayerBiomeAndZoneProperties'; Fields = 0; Properties = 31; Total = 31 }
)

$currentTopTwentyLeaderboard = @(
    [pscustomobject]@{ Rank = 1; Name = 'PlayerNamedPetFlagState'; Fields = 55; Properties = 0; Total = 55 }
    [pscustomobject]@{ Rank = 2; Name = 'WorldSecretSeedDefinitions'; Fields = 41; Properties = 0; Total = 41 }
    [pscustomobject]@{ Rank = 3; Name = 'MountGeometryAndFrameCatalog'; Fields = 32; Properties = 0; Total = 32 }
    [pscustomobject]@{ Rank = 4; Name = 'NpcBossDefeatProgressionState'; Fields = 31; Properties = 0; Total = 31 }
    [pscustomobject]@{ Rank = 5; Name = 'MountStaticAndDrillConstants'; Fields = 30; Properties = 0; Total = 30 }
    [pscustomobject]@{ Rank = 6; Name = 'NpcStatusEffectFlags'; Fields = 30; Properties = 0; Total = 30 }
    [pscustomobject]@{ Rank = 7; Name = 'PlayerCompanionAndRestState'; Fields = 28; Properties = 2; Total = 30 }
    [pscustomobject]@{ Rank = 8; Name = 'PlayerMinionCapacityAndSummonState'; Fields = 30; Properties = 0; Total = 30 }
    [pscustomobject]@{ Rank = 9; Name = 'MountRuntimeProjectionProperties'; Fields = 2; Properties = 27; Total = 29 }
    [pscustomobject]@{ Rank = 10; Name = 'PlayerIdentityAndLifecycleState'; Fields = 29; Properties = 0; Total = 29 }
    [pscustomobject]@{ Rank = 11; Name = 'GenVarsWorldLayerAndSurfaceState'; Fields = 28; Properties = 0; Total = 28 }
    [pscustomobject]@{ Rank = 12; Name = 'WorldGenerationDimensionsAndExecution'; Fields = 28; Properties = 0; Total = 28 }
    [pscustomobject]@{ Rank = 13; Name = 'NpcSpawnEnvironmentEligibilityInputs'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 14; Name = 'NpcTownRescueAndSpawnUnlocks'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 15; Name = 'PlayerAppearanceEquipmentSelection'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 16; Name = 'WorldGenerationTileMutationActions'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 17; Name = 'PlayerAppearanceEquipmentProjection'; Fields = 26; Properties = 0; Total = 26 }
    [pscustomobject]@{ Rank = 18; Name = 'NpcNetworkAndSpawnState'; Fields = 25; Properties = 0; Total = 25 }
    [pscustomobject]@{ Rank = 19; Name = 'PlayerFrameImmunityAndInteractionState'; Fields = 25; Properties = 0; Total = 25 }
    [pscustomobject]@{ Rank = 20; Name = 'PlayerSurvivalAndControlStatus'; Fields = 25; Properties = 0; Total = 25 }
)

$fourthTopTwentyLeaderboard = @(
    [pscustomobject]@{ Rank = 1; Name = 'PlayerNamedPetFlagState'; Fields = 55; Properties = 0; Total = 55 }
    [pscustomobject]@{ Rank = 2; Name = 'WorldSecretSeedDefinitions'; Fields = 41; Properties = 0; Total = 41 }
    [pscustomobject]@{ Rank = 3; Name = 'MountRuntimeProjectionProperties'; Fields = 2; Properties = 27; Total = 29 }
    [pscustomobject]@{ Rank = 4; Name = 'NpcSpawnEnvironmentEligibilityInputs'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 5; Name = 'WorldGenerationTileMutationActions'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 6; Name = 'PlayerMinionSummonFlags'; Fields = 25; Properties = 0; Total = 25 }
    [pscustomobject]@{ Rank = 7; Name = 'WorldGenerationControllerState'; Fields = 15; Properties = 10; Total = 25 }
    [pscustomobject]@{ Rank = 8; Name = 'PlayerCombatProcAndImmunityState'; Fields = 24; Properties = 0; Total = 24 }
    [pscustomobject]@{ Rank = 9; Name = 'PlayerSpawnMovementAndTileTargeting'; Fields = 24; Properties = 0; Total = 24 }
    [pscustomobject]@{ Rank = 10; Name = 'RevengeMarkerState'; Fields = 22; Properties = 2; Total = 24 }
    [pscustomobject]@{ Rank = 11; Name = 'WorldLifecycleAndTransformState'; Fields = 24; Properties = 0; Total = 24 }
    [pscustomobject]@{ Rank = 12; Name = 'MountAnimationFrameCatalog'; Fields = 23; Properties = 0; Total = 23 }
    [pscustomobject]@{ Rank = 13; Name = 'NpcDamageAttributionAndCredits'; Fields = 14; Properties = 9; Total = 23 }
    [pscustomobject]@{ Rank = 14; Name = 'PlayerCombatModifiersAndRanges'; Fields = 23; Properties = 0; Total = 23 }
    [pscustomobject]@{ Rank = 15; Name = 'GenVarsCavesOresAndBiomes'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 16; Name = 'LiquidFlowAndBufferState'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 17; Name = 'NpcSpawnZoneAndEventEligibilityInputs'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 18; Name = 'PlayerAccessoryAndCombatStatus'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 19; Name = 'PlayerInformationAccessoryState'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 20; Name = 'ProjectileSpecializedQueriesAndCaches'; Fields = 22; Properties = 0; Total = 22 }
)

$currentTopTwentyNoSplitReasons = [ordered]@{
    PlayerNamedPetFlagState = '全部成员是 Terraria.Player 的同一宠物旗标族；共同由宠物 Buff 生命周期重置和更新，未发现独立读写者边界。'
    WorldSecretSeedDefinitions = '全部成员属于 Terraria.WorldGen.SecretSeed 定义记录；注册表、选项标志和解锁元数据共享定义生命周期，拆分会制造伪独立状态。'
    MountRuntimeProjectionProperties = '字段和属性都是 Terraria.Mount 同一运行时状态的派生访问器；属性共同读取 _data、_active 和能力计时，拆分会重复投影状态。'
    NpcSpawnEnvironmentEligibilityInputs = '全部成员属于 Terraria.NPC.Spawner 的一次生成资格输入快照；同一生成评估拥有并消费这些条件，未形成稳定生命周期边界。'
    WorldGenerationTileMutationActions = '成员已经按 Terraria.WorldBuilding.Actions 的独立 action 类型声明；再按字段拆分会把单字段命令重新聚合为无语义碎片。'
}

$fourthPlayerBossPetFlagMembers = @(
    'petFlagKingSlimePet', 'petFlagEyeOfCthulhuPet', 'petFlagEaterOfWorldsPet', 'petFlagBrainOfCthulhuPet',
    'petFlagSkeletronPet', 'petFlagQueenBeePet', 'petFlagDestroyerPet', 'petFlagTwinsPet',
    'petFlagSkeletronPrimePet', 'petFlagPlanteraPet', 'petFlagGolemPet', 'petFlagDukeFishronPet',
    'petFlagLunaticCultistPet', 'petFlagMoonLordPet', 'petFlagFairyQueenPet', 'petFlagQueenSlimePet'
)
$fourthPlayerSeasonalAndEventPetFlagMembers = @(
    'petFlagDD2Gato', 'petFlagDD2Ghost', 'petFlagDD2Dragon', 'petFlagPumpkingPet', 'petFlagEverscreamPet',
    'petFlagIceQueenPet', 'petFlagMartianPet', 'petFlagDD2OgrePet', 'petFlagDD2BetsyPet'
)
$fourthPlayerStandardNamedPetFlagMembers = @(
    'petFlagUpbeatStar', 'petFlagSugarGlider', 'petFlagBabyShark', 'petFlagLilHarpy', 'petFlagFennecFox',
    'petFlagGlitteryButterfly', 'petFlagBabyImp', 'petFlagBabyRedPanda', 'petFlagPlantero',
    'petFlagDynamiteKitten', 'petFlagBabyWerewolf', 'petFlagShadowMimic', 'petFlagVoltBunny'
)
$fourthPlayerCrossoverPetFlagMembers = @(
    'petFlagBerniePet', 'petFlagGlommerPet', 'petFlagDeerclopsPet', 'petFlagPigPet', 'petFlagChesterPet',
    'petFlagJunimoPet', 'petFlagBlueChickenPet', 'petFlagSpiffo', 'petFlagCaveling',
    'petFlagDeadCellsSwarmBiter', 'petFlagPufferfish', 'petFlagChillet', 'petFlagChilletIgnis'
)
$fourthPlayerWorldObjectPetFlagMembers = @(
    'petFlagDirtiestBlock', 'petFlagBoulderPet', 'petFlagRainbowBoulderPet', 'petFlagAxeFairyPet'
)

$fourthWorldSecretSeedRegistryMembers = @(
    'AllSecretSeeds', 'Localization', '_code', '_sound', '_plaintext', 'TextThatWasUsedToUnlock'
)
$fourthWorldSecretSeedVisualAndSurfaceMembers = @(
    'paintEverythingGray', 'paintEverythingNegative', 'coatEverythingEcho', 'coatEverythingIlluminant',
    'noSurface', 'surfaceIsInSpace', 'rainsForAYear', 'rainbowStuff', 'worldIsFrozen'
)
$fourthWorldSecretSeedTerrainAndStructureMembers = @(
    'extraLivingTrees', 'extraFloatingIslands', 'biggerAbandonedHouses', 'addTeleporters', 'noSpiderCaves',
    'actuallyNoTraps', 'digExtraHoles', 'roundLandmasses', 'extraLiquid', 'portalGunInChests', 'dualDungeons'
)
$fourthWorldSecretSeedProgressionAndInfectionMembers = @(
    'errorWorld', 'graveyardBloodmoonStart', 'randomSpawn', 'startInHardmode', 'noInfection',
    'hallowOnTheSurface', 'worldIsInfected', 'surfaceIsMushrooms', 'surfaceIsDesert', 'pooEverywhere',
    'vampirism', 'teamBasedSpawns'
)
$fourthWorldSecretSeedSeasonalMembers = @('halloweenGen', 'endlessHalloween', 'endlessChristmas')

$fourthMountRuntimeIdentityAndFrameMembers = @(
    '_debugDraw', '_defaultDelegatesData', 'Active', 'Type', 'Frame', 'FlyTime', 'BodyFrame',
    'RunningGraceTime', 'PlayerXOFfset', 'PlayerOffset', 'PlayerOffsetHitbox', 'PlayerHeadOffset', 'HeightBoost'
)
$fourthMountRuntimeMobilityAndAbilityMembers = @(
    'RunSpeed', 'DashSpeed', 'Acceleration', 'AutoJump', 'BlockExtraJumps', 'IsConsideredASlimeMount',
    'Cart', 'CanGrindRails', 'AnyTrackRider', 'CanUseWings', 'Delegations', 'AbilityCharging',
    'AbilityActive', 'AbilityCharge', 'AllowDirectionChange', 'DismountOnItemUse'
)

$fourthNpcSpawnSpatialEligibilityMembers = @(
    'surfaceSpawn', 'spawnUndergroundDesert', 'hardDungeon', 'deeperThanRockLayer', 'underGround',
    'isOcean', 'isBeach', 'skyBehindPlayer', 'livingTree', 'inRemixStartingArea'
)
$fourthNpcSpawnBiomeAndDungeonEligibilityMembers = @(
    'waterTile', 'nearGranite', 'nearMarble', 'dualDungeonsSpawnRules', 'inDualDungeon', 'tresspassingDualDungeon'
)
$fourthNpcSpawnPolicyAndEventEligibilityMembers = @(
    'townNPCs', 'skyMob', 'noWorms', 'noGroundWorms', 'invaders', 'spawnFriendly', 'ignoreSafeWalls',
    'spawnSpider', 'isSpawningInWindDirection', 'offensiveToTim', 'playerHasStartingHealth'
)

$fourthWorldGenerationTileSetActionTypes = @(
    'Terraria.WorldBuilding.Actions.ClearTile', 'Terraria.WorldBuilding.Actions.HalfBlock',
    'Terraria.WorldBuilding.Actions.SetTile', 'Terraria.WorldBuilding.Actions.SetTileKeepWall',
    'Terraria.WorldBuilding.Actions.SetSlope', 'Terraria.WorldBuilding.Actions.SetHalfTile',
    'Terraria.WorldBuilding.Actions.SwapSolidTile'
)
$fourthWorldGenerationWallMutationActionTypes = @(
    'Terraria.WorldBuilding.Actions.ClearWall', 'Terraria.WorldBuilding.Actions.SetWall',
    'Terraria.WorldBuilding.Actions.PlaceWall'
)
$fourthWorldGenerationTilePlacementAndPaintActionTypes = @(
    'Terraria.WorldBuilding.Actions.SetTilePaint', 'Terraria.WorldBuilding.Actions.SetWallPaint',
    'Terraria.WorldBuilding.Actions.SetTileAndWallPaint', 'Terraria.WorldBuilding.Actions.PlaceTile'
)
$fourthWorldGenerationLiquidAndNeighborActionTypes = @(
    'Terraria.WorldBuilding.Actions.SetLiquid', 'Terraria.WorldBuilding.Actions.Smooth'
)

$fourthPlayerCoreMinionSummonMembers = @(
    'pygmy', 'raven', 'slime', 'hornetMinion', 'impMinion', 'twinsMinion', 'spiderMinion', 'pirateMinion',
    'sharknadoMinion', 'UFOMinion', 'DeadlySphereMinion', 'stardustMinion', 'stardustGuardian',
    'stardustDragon', 'batsOfLight', 'babyBird', 'vampireFrog', 'stormTiger', 'smolstar', 'empressBlade',
    'flinxMinion', 'abigailMinion'
)
$fourthPlayerCrossoverMinionSummonMembers = @('deadCellsMushroomBoiMinion', 'palworldCattivaMinion', 'palworldFoxsparksMinion')

$fourthWorldGenerationControllerPassMembers = @('_previousManifest', '_snapshots', 'OnPassesLoaded', '_generator', 'Passes', 'CurrentPass', 'LastCompletedPass')
$fourthWorldGenerationControllerPauseAndHashMembers = @(
    '_paused', 'PauseAfterPass', 'PauseOnHashMismatch', 'PausedDueToHashMismatch', 'SnapshotFrequency', 'Paused', 'QueuedAbort'
)
$fourthWorldGenerationGeneratorExecutionMembers = @(
    '_passes', '_seed', '_configuration', '_progress', '_controller', '_controlLock', '_currentPass',
    'CurrentGenerationProgress', 'CurrentController', '_hashTime', 'PassResults'
)

$fourthPlayerCombatDamageProcMembers = @(
    'lifeSteal', 'ghostDmg', 'eocDash', 'eocHit', 'infernoCounter', 'starCloakCooldown', 'onHitDodge',
    'onHitRegen', 'onHitPetal', 'onHitTitaniumStorm', 'titaniumStormCooldown', 'hasTitaniumStormBuff',
    'petalTimer', 'boneGloveTimer', 'phantomPhoneixCounter'
)
$fourthPlayerCombatDodgeAndImmunityMembers = @(
    'blackBelt', 'brainOfConfusionItem', 'brainOfConfusionDodgeAnimationCounter', 'shadowDodge', 'shadowDodgeTimer'
)
$fourthPlayerCombatBarrierAndRegenMembers = @('iceBarrier', 'iceBarrierFrame', 'iceBarrierFrameCounter', 'palladiumRegen')

$fourthPlayerSpawnAndReturnMembers = @('SpawnX', 'SpawnY', 'PotionOfReturnOriginalUsePosition', 'PotionOfReturnHomePosition')
$fourthPlayerTileTargetingAndRangeMembers = @(
    'DefaultTileRangeX', 'DefaultTileRangeY', 'tileRangeX', 'tileRangeY', 'lastTileRangeX', 'lastTileRangeY',
    'tileTargetX', 'tileTargetY', 'adjTile', 'defaultItemGrabRange', 'itemGrabSpeed', 'itemGrabSpeedMax'
)
$fourthPlayerMovementPhysicsMembers = @(
    'defaultGravity', 'jumpHeight', 'jumpSpeed', 'gravity', 'maxFallSpeed', 'maxRunSpeed', 'runAcceleration', 'runSlowdown'
)

$fourthRevengeMarkerExpirationAndIdentityMembers = @(
    '_uniqueIDCounter', '_expirationCompCopper', '_expirationCompSilver', '_expirationCompGold', '_expirationCompPlat',
    'ONE_MINUTE', '_expirationTime', '_uniqueID', 'UniqueID'
)
$fourthRevengeMarkerEnemyContextMembers = @(
    'ENEMY_BOX_WIDTH', 'ENEMY_BOX_HEIGHT', 'EnemyBoxSize', '_location', '_hitbox', '_npcNetID', '_npcHPPercent',
    '_npcTypeAgainstDiscouragement', '_npcAIStyleAgainstDiscouragement', '_spawnedFromStatue'
)
$fourthRevengeMarkerValueAndRespawnMembers = @('_baseValue', '_coinsValue', '_forceExpire', '_attemptedRespawn', 'RespawnAttemptLocked')

$fourthWorldLifecycleLoadAndTransformMembers = @(
    '_transformingWorld', 'isGeneratingOrLoadingWorld', 'loadFailed', 'worldCleared', 'worldBackup', 'lastMaxTilesX', 'lastMaxTilesY'
)
$fourthWorldLifecycleProgressionAndEventMembers = @('spawnEye', 'spawnHardBoss', 'shadowOrbSmashed', 'shadowOrbCount', 'altarCount', 'spawnMeteor')
$fourthWorldLifecycleHousingAndSpawnPacingMembers = @(
    'builtHouseWithNoFurniture', 'builtHouseWithNoLight', 'stopDrops', 'AllowedToSpreadInfections', 'destroyObject', 'npcSpawnDelay', 'npcSpawnPeriod'
)
$fourthWorldLifecycleTileMergeMembers = @('mergeUp', 'mergeDown', 'mergeLeft', 'mergeRight')

$fourthMountGroundAnimationFrameMembers = @(
    'totalFrames', 'standingFrameStart', 'standingFrameCount', 'standingFrameDelay', 'runningFrameStart', 'runningFrameCount',
    'runningFrameDelay', 'idleFrameStart', 'idleFrameCount', 'idleFrameDelay', 'idleFrameLoop'
)
$fourthMountAerialAndWaterAnimationFrameMembers = @(
    'flyingFrameStart', 'flyingFrameCount', 'flyingFrameDelay', 'inAirFrameStart', 'inAirFrameCount', 'inAirFrameDelay',
    'swimFrameStart', 'swimFrameCount', 'swimFrameDelay'
)
$fourthMountDashAnimationFrameMembers = @('dashingFrameStart', 'dashingFrameCount', 'dashingFrameDelay')

$fourthNpcDamageDefinitionMembers = @('NPCTypes', 'Name', 'CustomBossDefinitions', 'BossTypeForMob')
$fourthNpcDamageRuntimeMembers = @(
    '_activeTrackers', '_recentFinishedTrackers', 'MAX_RECENT_TRACKERS', 'EXTRA_RECENT_TRACKER_EXPIRY_TIME', '_list',
    '_worldCredit', '_lastAttacker', '_ticks', '_lastHitTime', 'IsEmpty', 'Duration', 'TimeSinceLastHit'
)
$fourthNpcDamageCreditProjectionMembers = @('PlayerName', 'Damage', 'Name', 'Name', 'Name', 'Name', 'KillTimeMessage')

$fourthPlayerCombatDamageAndCritModifierMembers = @(
    'meleeCrit', 'magicCrit', 'rangedCrit', 'meleeDamage', 'magicDamage', 'rangedDamage', 'rangedMultDamage',
    'arrowDamageAdditiveStack', 'arrowDamage', 'bulletDamage', 'rocketDamage', 'minionDamage', 'minionKB', 'revolverCritChanceBonus'
)
$fourthPlayerCombatSpeedRangeAndPermissionMembers = @(
    'IsAllowedToHoldItems', 'meleeSpeed', 'summonerWeaponSpeedBonus', 'moveSpeed', 'pickSpeed', 'wallSpeed', 'tileSpeed', 'autoPaint', 'autoActuator'
)

$fourthGenVarsCaveTunnelAndOrePatchMembers = @(
    'numMCaves', 'mCaveX', 'mCaveY', 'maxTunnels', 'numTunnels', 'tunnelX', 'maxOrePatch', 'numOrePatch', 'orePatchX'
)
$fourthGenVarsMushroomBiomeAndLogMembers = @('maxMushroomBiomes', 'numMushroomBiomes', 'mushroomBiomesPosition', 'logX', 'logY')
$fourthGenVarsLakeAndOasisMembers = @('maxLakes', 'numLakes', 'LakeX', 'maxOasis', 'numOasis', 'oasisPosition', 'oasisWidth', 'oasisHeight')

$fourthLiquidFlowBudgetAndPanicMembers = @(
    'maxLiquidBuffer', 'maxLiquid', 'skipCount', 'stuckCount', 'stuckAmount', 'cycles', 'curMaxLiquid', 'numLiquid',
    'stuck', 'quickFall', 'quickSettle', 'wetCounter', 'panicCounter', 'panicMode', 'panicY'
)
$fourthLiquidCellWorkItemMembers = @('x', 'y', 'kill', 'delay')
$fourthLiquidBufferQueueMembers = @('numLiquidBuffer', 'x', 'y')

$fourthNpcSpawnBiomeZoneMembers = @(
    'ZoneCorrupt', 'ZoneCrimson', 'ZoneHallow', 'ZoneJungle', 'ZoneSnow', 'ZoneGlowshroom', 'ZoneMeteor',
    'ZoneGraveyard', 'ZoneDungeon', 'ZoneLihzhardTemple', 'ZoneGranite', 'ZoneMarble', 'ZoneSandstorm'
)
$fourthNpcSpawnEventAndTowerMembers = @(
    'ZoneTowerSolar', 'ZoneTowerVortex', 'ZoneTowerNebula', 'ZoneTowerStardust', 'ZoneOldOneArmy',
    'ZoneWaterCandle', 'ZonePeaceCandle', 'ZoneShadowCandle'
)
$fourthNpcSpawnTargetSelectionMembers = @('defaultTarget')

$fourthPlayerAccessoryCombatModifierMembers = @(
    'kbGlove', 'autoReuseGlove', 'meleeScaleGlove', 'kbBuff', 'remoteVisionForDrone', 'starCloakItem',
    'starCloakItem_manaCloakOverrideItem', 'starCloakItem_starVeilOverrideItem', 'starCloakItem_beeCloakOverrideItem'
)
$fourthPlayerAccessoryResourceAndInvulnerabilityMembers = @(
    'longInvince', 'pStone', 'PhilosopherStoneDurationMultiplier', 'manaFlower', 'moonLeech'
)
$fourthPlayerAccessoryDebuffAndDropMembers = @(
    'vortexDebuff', 'trapDebuffSource', 'witheredArmor', 'witheredWeapon', 'slowOgreSpit', 'parryDamageBuff',
    'ballistaPanic', 'JustDroppedAnItem'
)

$fourthPlayerInformationWorldAndMovementMembers = @('hostile', 'hermesStepSound', 'instantMovementAccumulatedThisFrame', 'lastCreatureHit', 'ActuationRodLock')
$fourthPlayerInformationNavigationAndTimeMembers = @(
    'accCompass', 'accWatch', 'accWatchTime', 'accDepthMeter', 'accWeatherRadio', 'accCalendar', 'accStopwatch'
)
$fourthPlayerInformationDetectionAndWiringMembers = @(
    'accFishFinder', 'accJarOfSouls', 'accThirdEye', 'accThirdEyeCounter', 'accOreFinder', 'accCritterGuide',
    'accDreamCatcher', 'InfoAccMechShowWires'
)
$fourthPlayerFootballPresentationMembers = @('hasFootball', 'drawingFootball')

$fourthProjectileCombatScalingMembers = @('bonusCritChance', 'hostileDamageScaling')
$fourthProjectileCollisionGeometryMembers = @(
    '_cachedConditions_solid', '_cachedConditions_notNull', '_javelinsMax6', '_javelinsMax8', '_javelinsMax10',
    'WhipPointsForCollision', '_lanceHitboxBounds', '_lightningCollisionBounds'
)
$fourthProjectileTargetSelectionMembers = @(
    '_rainbowBoulderTargetsAny', '_rainbowBoulderTargetsFar', '_medusaHeadTargetList', '_medusaTargetComparer',
    '_ai164_blacklistedTargets', '_ai158_blacklistedTargets', '_ai156_blacklistedTargets'
)
$fourthProjectileFishingAndMiningMembers = @('_availableFishTypesToShow', '_context', '_miningHelperPointsToSkip')
$fourthProjectileKiteAndLightningMembers = @('StormLightningLiquidDamageRadius', 'MinimumWindStrengthToFlyKite')

$mountGeometryMembers = @(
    'textureWidth', 'textureHeight', 'xOffset', 'yOffset', 'playerYOffsets',
    'bodyFrame', 'playerHeadOffset', 'heightBoost', 'playerXOffset'
)
$mountAnimationFrameMembers = @(
    'totalFrames', 'standingFrameStart', 'standingFrameCount', 'standingFrameDelay',
    'runningFrameStart', 'runningFrameCount', 'runningFrameDelay',
    'flyingFrameStart', 'flyingFrameCount', 'flyingFrameDelay',
    'inAirFrameStart', 'inAirFrameCount', 'inAirFrameDelay',
    'idleFrameStart', 'idleFrameCount', 'idleFrameDelay', 'idleFrameLoop',
    'swimFrameStart', 'swimFrameCount', 'swimFrameDelay',
    'dashingFrameStart', 'dashingFrameCount', 'dashingFrameDelay'
)
$npcBossDefeatMembers = @(
    'downedBoss1', 'downedBoss2', 'downedBoss3', 'downedQueenBee', 'downedSlimeKing',
    'downedPlantBoss', 'downedGolemBoss', 'downedFishron', 'downedAncientCultist',
    'downedMoonlord', 'downedEmpressOfLight', 'downedQueenSlime', 'downedDeerclops',
    'downedMechBossAny', 'downedMechBoss1', 'downedMechBoss2', 'downedMechBoss3'
)
$npcEventDefeatMembers = @(
    'downedGoblins', 'downedFrost', 'downedPirates', 'downedClown', 'downedMartians',
    'downedHalloweenTree', 'downedHalloweenKing', 'downedChristmasIceQueen',
    'downedChristmasTree', 'downedChristmasSantank', 'downedTowerSolar',
    'downedTowerVortex', 'downedTowerNebula', 'downedTowerStardust'
)
$mountFrameAndDrawMembers = @(
    'FrameStanding', 'FrameRunning', 'FrameInAir', 'FrameFlying', 'FrameSwimming',
    'FrameDashing', 'DrawBack', 'DrawBackExtra', 'DrawFront', 'DrawFrontExtra',
    'idleFrames_Rat'
)
$mountSpecialVehicleMembers = @(
    'mounts', 'scutlixEyePositions', 'scutlixTextureSize', 'scutlixBaseDamage',
    'santankTextureSize'
)
$mountDrillMembers = @(
    'drillDiodePoint1', 'drillDiodePoint2', 'drillTextureSize', 'drillTextureWidth',
    'drillRotationChange', 'drillPickPower', 'drillPickTime', 'amountOfBeamsAtOnce',
    'maxDrillLength'
)
$mountSuperCartMembers = @(
    'SuperCartRunSpeed', 'SuperCartDashSpeed', 'SuperCartAcceleration',
    'SuperCartJumpHeight', 'SuperCartJumpSpeed'
)
$npcElementalDebuffMembers = @(
    'midas', 'ichor', 'brokenArmor', 'onFire', 'onFire2', 'onFire3',
    'onFrostBurn', 'onFrostBurn2', 'poisoned', 'venom', 'tipsy', 'bleeding',
    'hemorrhage', 'shadowFlame', 'soulDrain', 'shimmering', 'oiled'
)
$npcControlAndSocialMembers = @('confused', 'loveStruck', 'stinky', 'dryadWard')
$npcWhipAndSpecialMembers = @(
    'markedByScytheWhip', 'markedByEelWhip', 'javelined', 'tentacleSpiked',
    'bloodButchered', 'celled', 'dryadBane', 'daybreak', 'betsysCurse'
)
$playerMinionCapacityMembers = @('maxMinions', 'numMinions', 'slotsMinions')
$playerMinionDamageMembers = @('highestStormTigerGemOriginalDamage', 'highestAbigailCounterOriginalDamage')
$playerIdentityAndDeathMembers = @(
    'active', 'host', 'lostCoins', 'lostCoinString', 'name', 'numberOfDeathsPVE',
    'numberOfDeathsPVP', 'lastDeathPostion', 'lastDeathTime', 'showLastDeath'
)
$playerRuntimeInteractionAndEffectMembers = @(
    'MinecartSettings', 'emoteTime', 'creativeTracker', 'chatOverhead',
    'GoingDownWithGrapple', 'spelunkerTimer', 'builderAccStatus', 'soulDrain',
    'dd2Accessory', 'crystalLeafDamage', 'crystalLeafKB', 'basiliskCharge',
    'PaladinsShieldRange'
)
$playerConsumedProgressionMembers = @(
    'usedAegisCrystal', 'usedAegisFruit', 'usedArcaneCrystal', 'usedGalaxyPearl',
    'usedGummyWorm', 'usedAmbrosia'
)
$genVarsWorldLayerMembers = @(
    'lowestCloud', 'worldSurfaceLow', 'worldSurface', 'worldSurfaceHigh',
    'rockLayerLow', 'rockLayer', 'rockLayerHigh', 'snowTop', 'snowBottom',
    'snowOriginLeft', 'snowOriginRight', 'snowMinX', 'snowMaxX'
)
$genVarsSurfaceAndBiomeMembers = @(
    'worldSpawnHasBeenRandomized', 'landmassData', 'remixSurfaceLayerLow',
    'remixSurfaceLayerHigh', 'remixMushroomLayerLow', 'remixMushroomLayerHigh',
    'boulderPetsPlaced', 'crimStoneWall', 'crimStone', 'ebonStoneWall', 'ebonStone',
    'mossTile', 'mossWall', 'lavaLine', 'waterLine'
)
$worldGenerationDimensionsMembers = @(
    'meteorShowerCount', 'WorldSizeSmallX', 'WorldSizeSmallY', 'WorldSizeMediumX',
    'WorldSizeMediumY', 'WorldSizeLargeX', 'WorldSizeLargeY',
    'InfectionAndGrassSpreadOuterWorldBuffer'
)
$worldGenerationExecutionMembers = @(
    'generatingWorld', 'generatingWorldOnThisThread', '_generator',
    'SmallConsecutivesFound', 'SmallConsecutivesEliminated', 'placingTraps'
)
$worldGenerationSecretSeedFlagMembers = @(
    'remixWorldGen', 'everythingWorldGen', 'noTrapsWorldGen', 'drunkWorldGen',
    'getGoodWorldGen', 'tenthAnniversaryWorldGen', 'dontStarveWorldGen',
    'notTheBees', 'skyblockWorldGen', 'drunkWorldGenText'
)
$worldGenerationScratchMembers = @('trapDiag', 'gem', 'mossType', 'neonMossType')
$npcTownRescueMembers = @(
    'savedTaxCollector', 'savedGoblin', 'savedWizard', 'savedMech', 'savedAngler',
    'savedStylist', 'savedBartender', 'savedGolfer'
)
$npcTownPetAdoptionMembers = @('boughtCat', 'boughtDog', 'boughtBunny')
$npcTownSpawnUnlockMembers = @(
    'unlockedSlimeBlueSpawn', 'unlockedSlimeGreenSpawn', 'unlockedSlimeOldSpawn',
    'unlockedSlimePurpleSpawn', 'unlockedSlimeRainbowSpawn', 'unlockedSlimeRedSpawn',
    'unlockedSlimeYellowSpawn', 'unlockedSlimeCopperSpawn', 'unlockedMerchantSpawn',
    'unlockedDemolitionistSpawn', 'unlockedPartyGirlSpawn', 'unlockedDyeTraderSpawn',
    'unlockedTruffleSpawn', 'unlockedArmsDealerSpawn', 'unlockedNurseSpawn',
    'unlockedPrincessSpawn'
)
$playerEquipmentSelectionMembers = @(
    'head', 'body', 'legs', 'coat', 'handon', 'handoff', 'back', 'front', 'shoe',
    'waist', 'shield', 'neck', 'face', 'balloon', 'backpack', 'tail', 'faceHead',
    'faceFlower', 'faceMask', 'balloonFront', 'beard'
)
$playerAppearanceSelectionStateMembers = @(
    'jump', 'voiceOverride', 'hideVisibleAccessory', 'hideMisc', 'bodyFrame', 'legFrame'
)
$playerEquipmentColorMembers = @(
    'cHead', 'cBody', 'cLegs', 'cHandOn', 'cHandOff', 'cBack', 'cFront', 'cShoe',
    'cWaist', 'cShield', 'cNeck', 'cFace', 'cFaceHead', 'cFaceFlower', 'cFaceMask',
    'cBalloon', 'cBalloonFront', 'cBackpack', 'cTail', 'cShieldFallback'
)
$playerTraversalColorMembers = @('cWings', 'cCarpet', 'cFloatingTube', 'cGrapple', 'cMount', 'cMinecart')
$npcNetworkMembers = @(
    'netUpdatePendingSpamCooldown', 'netUpdatePendingFullSpamCooldown', 'netSpamPacketLimit',
    'netSpamTicksPerPacket', 'netSpamTicksPerPacketForBosses', 'netSpam', 'netAlways',
    'spawnNeedsSyncing', 'netStream', 'playerNetSyncState', 'netOffset'
)
$npcSpawnBudgetMembers = @(
    'safeRangeX', 'safeRangeY', 'activeRangeX', 'activeRangeY', 'npcSlots',
    'noSpawnCycle', 'activeTime', 'defaultSpawnRate', 'defaultMaxSpawns', 'dontCountMe'
)
$npcIdentityAndStatusMembers = @('realLife', '_givenName', 'shimmerTransparency', 'maxBuffs')
$playerFrameAndImmunityMembers = @(
    'townNPCs', 'bodyFrameCounter', 'legFrameCounter', 'immune', 'immuneNoBlink',
    'immuneTime', 'immuneAlphaDirection', 'immuneAlpha', '_timeSinceLastImmuneGet',
    '_immuneStrikes', 'maxRegenDelay'
)
$playerInteractionInputMembers = @(
    'team', 'nameLen', 'sign', 'reuseDelay', 'aggro', 'nearbyActiveNPCs',
    'creativeInterface', 'mouseInterface', 'lastMouseInterface', 'noThrow',
    'changeItem', 'pendingItemReuse', 'selectedItemState', 'selectedKite'
)
$playerSurvivalAndTransformationMembers = @(
    'noItems', 'hungry', 'starving', 'heartyMeal', 'windPushed', 'wereWolf',
    'wolfAcc', 'hideMerman', 'hideWolf', 'forceMerman', 'forceWerewolf',
    'sunScorchCounter', 'accMerman', 'merman', 'trident'
)
$playerDebuffStatusMembers = @('cursed', 'bleed', 'confused', 'brokenArmor', 'silence', 'slow', 'gross', 'tongued')
$playerBuilderOverlayMembers = @('rulerGrid', 'rulerLine')

$thirdRefinementDefinitions = [ordered]@{}
$thirdRefinementDefinitions['MountGeometryAndFrameCatalog'] = @(
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountGeometryAndFrameCatalog' 'MountGeometryAndOffsetCatalog' 'definition/query' '坐骑纹理尺寸、玩家偏移、碰撞高度和身体定位定义。' '只读 Definition/Catalog view；移动和绘制系统按几何快照消费。' { param($row) Test-MemberName $row $mountGeometryMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountGeometryAndFrameCatalog' 'MountAnimationFrameCatalog' 'definition/query' '坐骑站立、奔跑、飞行、游泳、闲置和冲刺帧序列定义。' '只读 Definition/Catalog view；帧系统按状态读取，不回写定义。' { param($row) Test-MemberName $row $mountAnimationFrameMembers })
)
$thirdRefinementDefinitions['NpcBossDefeatProgressionState'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcBossDefeatProgressionState' 'NpcBossDefeatFlags' 'authoritative state/behavior' '主要 Boss、机械 Boss 和事件后 Boss 的击败进度标志。' 'Owner System/CommitPort；Boss 结算事件是唯一进度写入方向。' { param($row) Test-MemberName $row $npcBossDefeatMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcBossDefeatProgressionState' 'NpcEventDefeatFlags' 'authoritative state/behavior' '入侵、节日和天界塔事件击败进度标志。' 'Owner System/CommitPort；事件结算与世界事件查询单向交接。' { param($row) Test-MemberName $row $npcEventDefeatMembers })
)
$thirdRefinementDefinitions['MountStaticAndDrillConstants'] = @(
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountStaticAndDrillConstants' 'MountFrameAndDrawCatalog' 'definition/query' '坐骑通用帧状态、绘制层级和特殊老鼠帧序列常量。' '只读 Definition/Catalog view；表现系统按帧定义消费。' { param($row) Test-MemberName $row $mountFrameAndDrawMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountStaticAndDrillConstants' 'MountSpecialVehicleCatalog' 'definition/query' '坐骑注册表、Scutlix 和 Santank 的专用车辆/战斗定义。' '只读 Definition/Catalog view；车辆适配器单向读取。' { param($row) Test-MemberName $row $mountSpecialVehicleMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountStaticAndDrillConstants' 'MountDrillConstants' 'definition/query' '钻头二极管、钻取长度、功率、时间和光束数量定义。' '只读 Definition/Catalog view；钻头行为系统按配置读取。' { param($row) Test-MemberName $row $mountDrillMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountStaticAndDrillConstants' 'MountSuperCartConstants' 'definition/query' '超级矿车速度、加速度和跳跃能力常量。' '只读 Definition/Catalog view；矿车移动系统单向读取。' { param($row) Test-MemberName $row $mountSuperCartMembers })
)
$thirdRefinementDefinitions['NpcStatusEffectFlags'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcStatusEffectFlags' 'NpcElementalDebuffState' 'authoritative state/behavior' '元素、伤害、火焰、毒性和微光效果标志。' 'Owner System/CommitPort；状态 Tick 和命中事件通过显式提交更新。' { param($row) Test-MemberName $row $npcElementalDebuffMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcStatusEffectFlags' 'NpcControlAndSocialEffectState' 'authoritative state/behavior' '混乱、魅惑、气味和 Dryad Ward 控制/社交效果标志。' 'Owner System/CommitPort；控制效果事件单向写入。' { param($row) Test-MemberName $row $npcControlAndSocialMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcStatusEffectFlags' 'NpcWhipAndSpecialEffectState' 'authoritative state/behavior' '鞭类标记、特殊武器标记和 Betsy/Daybreak 效果标志。' 'Owner System/CommitPort；特殊命中事件单向更新。' { param($row) Test-MemberName $row $npcWhipAndSpecialMembers })
)
$thirdRefinementDefinitions['PlayerCompanionAndRestState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCompanionAndRestState' 'PlayerEyeAnimationState' 'registry/projection' '眼睛状态和受伤/中毒/睡眠驱动的眼部动画投影。' 'Registry/Projection seam；表现状态只读取玩家事实。' { param($row) $row.Type -eq 'Terraria.GameContent.PlayerEyeHelper' }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCompanionAndRestState' 'PlayerPettingState' 'authoritative state/behavior' '玩家抚摸 NPC、投射物或坐骑目标的交互状态。' 'Owner System/CommitPort；交互命令维护目标引用和生命周期。' { param($row) $row.Type -eq 'Terraria.GameContent.PlayerPettingInfo' }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCompanionAndRestState' 'PlayerSittingState' 'authoritative state/behavior' '玩家椅子坐姿、座位偏移和堆叠索引状态。' 'Owner System/CommitPort；座椅交互事件单向提交。' { param($row) $row.Type -eq 'Terraria.GameContent.PlayerSittingHelper' }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCompanionAndRestState' 'PlayerSleepingState' 'authoritative state/behavior' '玩家睡眠、入睡计时和床面投影状态。' 'Owner System/CommitPort；睡眠生命周期事件集中写入。' { param($row) $row.Type -eq 'Terraria.GameContent.PlayerSleepingHelper' }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCompanionAndRestState' 'PlayerRabbitOrderFrameState' 'registry/projection' '兔子指令帧状态机及其表现帧计数。' 'Registry/Projection seam；帧状态由表现更新消费。' { param($row) $row.Type -eq 'Terraria.Player.RabbitOrderFrameHelper' })
)
$thirdRefinementDefinitions['PlayerMinionCapacityAndSummonState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerMinionCapacityAndSummonState' 'PlayerMinionCapacityState' 'authoritative state/behavior' '玩家召唤栏位上限、当前召唤数和分数槽容量。' 'Owner System/CommitPort；召唤容量由装备/效果命令更新。' { param($row) Test-MemberName $row $playerMinionCapacityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerMinionCapacityAndSummonState' 'PlayerMinionSummonFlags' 'authoritative state/behavior' '各类召唤物当前存在与召唤资格标志。' 'Owner System/CommitPort；召唤生命周期事件维护旗标。' { param($row) ($row.Member -notin $playerMinionCapacityMembers -and $row.Member -notin $playerMinionDamageMembers) }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerMinionCapacityAndSummonState' 'PlayerMinionDamageTrackingState' 'authoritative state/behavior' 'Storm Tiger 和 Abigail 召唤物的原始伤害追踪值。' 'Owner System/CommitPort；召唤命中事件更新追踪值。' { param($row) Test-MemberName $row $playerMinionDamageMembers })
)
$thirdRefinementDefinitions['PlayerIdentityAndLifecycleState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerIdentityAndLifecycleState' 'PlayerIdentityAndDeathRecordState' 'authoritative state/behavior' '玩家活动、主机身份、名称、死亡次数和死亡记录。' 'Owner System/CommitPort；玩家生命周期和死亡结算集中提交。' { param($row) Test-MemberName $row $playerIdentityAndDeathMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerIdentityAndLifecycleState' 'PlayerRuntimeInteractionAndEffectState' 'authoritative state/behavior' '矿车、表情、建造器、抓钩、探测器和运行时效果状态。' 'Owner System/CommitPort；交互/效果系统通过显式命令更新。' { param($row) Test-MemberName $row $playerRuntimeInteractionAndEffectMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerIdentityAndLifecycleState' 'PlayerConsumedProgressionFlags' 'authoritative state/behavior' '一次性世界物品和进度消耗旗标。' 'Owner System/CommitPort；进度命令是唯一写入方向。' { param($row) Test-MemberName $row $playerConsumedProgressionMembers })
)
$thirdRefinementDefinitions['GenVarsWorldLayerAndSurfaceState'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsWorldLayerAndSurfaceState' 'GenVarsWorldLayerMetrics' 'authoritative state/behavior' '云层、世界表面、岩层和积雪边界测量值。' 'WorldGen System/CommitPort；地层 pass 计算后提交。' { param($row) Test-MemberName $row $genVarsWorldLayerMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsWorldLayerAndSurfaceState' 'GenVarsSurfaceAndBiomeState' 'authoritative state/behavior' '出生点、地貌、感染、苔藓和液体线等表面/生态状态。' 'WorldGen System/CommitPort；表面与生态 pass 按阶段写入。' { param($row) Test-MemberName $row $genVarsSurfaceAndBiomeMembers })
)
$thirdRefinementDefinitions['WorldGenerationDimensionsAndExecution'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationDimensionsAndExecution' 'WorldGenerationDimensionsState' 'authoritative state/behavior' '世界尺寸、扩散边界和流星生成计数配置。' 'WorldGen System/CommitPort；世界配置阶段集中提交。' { param($row) Test-MemberName $row $worldGenerationDimensionsMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationDimensionsAndExecution' 'WorldGenerationExecutionState' 'authoritative state/behavior' '生成线程、生成器实例、连续地形统计和陷阱放置阶段状态。' 'WorldGen System/CommitPort；生成调度阶段唯一写入。' { param($row) Test-MemberName $row $worldGenerationExecutionMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationDimensionsAndExecution' 'WorldGenerationSecretSeedFlags' 'authoritative state/behavior' '特殊世界种子和生成模式启用旗标。' 'Owner System/CommitPort；世界规则命令提交模式状态。' { param($row) Test-MemberName $row $worldGenerationSecretSeedFlagMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationDimensionsAndExecution' 'WorldGenerationScratchState' 'authoritative state/behavior' '陷阱、宝石和苔藓生成过程的临时工作数组与类型缓存。' 'Owner System/CommitPort；生成 pass 内部拥有并清理临时状态。' { param($row) Test-MemberName $row $worldGenerationScratchMembers })
)
$thirdRefinementDefinitions['NpcTownRescueAndSpawnUnlocks'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcTownRescueAndSpawnUnlocks' 'NpcTownRescueState' 'authoritative state/behavior' '已救援城镇 NPC 的持久进度旗标。' 'Owner System/CommitPort；救援事件集中更新。' { param($row) Test-MemberName $row $npcTownRescueMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcTownRescueAndSpawnUnlocks' 'NpcTownPetAdoptionState' 'authoritative state/behavior' '城镇宠物购买和领养解锁状态。' 'Owner System/CommitPort；购买事件单向提交。' { param($row) Test-MemberName $row $npcTownPetAdoptionMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcTownRescueAndSpawnUnlocks' 'NpcTownSpawnUnlockState' 'authoritative state/behavior' '城镇 NPC 与特殊史莱姆生成解锁状态。' 'Owner System/CommitPort；生成资格 Query 只读消费解锁事实。' { param($row) Test-MemberName $row $npcTownSpawnUnlockMembers })
)
$thirdRefinementDefinitions['PlayerAppearanceEquipmentSelection'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceEquipmentSelection' 'PlayerEquipmentSelectionSlots' 'authoritative state/behavior' '头身手部、饰品、背部和面部装备选择槽。' 'Owner System/CommitPort；装备选择命令集中写入。' { param($row) Test-MemberName $row $playerEquipmentSelectionMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceEquipmentSelection' 'PlayerAppearanceSelectionState' 'registry/projection' '跳跃帧、语音覆盖、隐藏配饰和身体动画帧选择状态。' 'Registry/Projection seam；表现投影只读取选择状态。' { param($row) Test-MemberName $row $playerAppearanceSelectionStateMembers })
)
$thirdRefinementDefinitions['PlayerAppearanceEquipmentProjection'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceEquipmentProjection' 'PlayerEquipmentColorProjection' 'registry/projection' '头身手部、饰品和面部装备的颜色投影槽。' 'Registry/Projection seam；颜色快照不反向修改装备状态。' { param($row) Test-MemberName $row $playerEquipmentColorMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceEquipmentProjection' 'PlayerTraversalColorProjection' 'registry/projection' '翅膀、飞毯、浮筒、抓钩、坐骑和矿车颜色投影槽。' 'Registry/Projection seam；移动表现只读消费颜色快照。' { param($row) Test-MemberName $row $playerTraversalColorMembers })
)
$thirdRefinementDefinitions['NpcNetworkAndSpawnState'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcNetworkAndSpawnState' 'NpcNetworkReplicationState' 'registry/projection' 'NPC 网络更新节流、同步流和玩家同步状态。' 'Network Projection seam；网络投影只读取 NPC 权威状态。' { param($row) Test-MemberName $row $npcNetworkMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcNetworkAndSpawnState' 'NpcSpawnBudgetAndActivityState' 'authoritative state/behavior' 'NPC 活跃范围、生成频率、生成容量和计数预算状态。' 'Owner System/CommitPort；生成调度阶段集中写入。' { param($row) Test-MemberName $row $npcSpawnBudgetMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcNetworkAndSpawnState' 'NpcIdentityAndStatusState' 'authoritative state/behavior' 'NPC 关联实体、名称、微光透明度和 Buff 容量状态。' 'Owner System/CommitPort；实体生命周期系统维护身份和容量。' { param($row) Test-MemberName $row $npcIdentityAndStatusMembers })
)
$thirdRefinementDefinitions['PlayerFrameImmunityAndInteractionState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerFrameImmunityAndInteractionState' 'PlayerFrameAndImmunityState' 'authoritative state/behavior' '玩家身体帧、无敌计时、闪烁和免疫打击状态。' 'Owner System/CommitPort；受伤状态转换集中写入。' { param($row) Test-MemberName $row $playerFrameAndImmunityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerFrameImmunityAndInteractionState' 'PlayerInteractionInputState' 'authoritative state/behavior' '队伍、交互界面、物品复用和选中目标输入状态。' 'Owner System/CommitPort；输入/交互命令单向更新。' { param($row) Test-MemberName $row $playerInteractionInputMembers })
)
$thirdRefinementDefinitions['PlayerSurvivalAndControlStatus'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerSurvivalAndControlStatus' 'PlayerSurvivalAndTransformationState' 'authoritative state/behavior' '生存饥饿、风推、变身、鱼人和三叉戟控制状态。' 'Owner System/CommitPort；生存/变身效果事件集中提交。' { param($row) Test-MemberName $row $playerSurvivalAndTransformationMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerSurvivalAndControlStatus' 'PlayerDebuffStatusState' 'authoritative state/behavior' '诅咒、流血、混乱、破甲、沉默、迟缓和舌头状态。' 'Owner System/CommitPort；减益 Tick 和命中事件单向更新。' { param($row) Test-MemberName $row $playerDebuffStatusMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerSurvivalAndControlStatus' 'PlayerBuilderOverlayState' 'registry/projection' '标尺网格和标尺线的建造者界面投影状态。' 'Registry/Projection seam；建造界面只读消费。' { param($row) Test-MemberName $row $playerBuilderOverlayMembers })
)

$refinementDefinitions = [ordered]@{}
$refinementDefinitions['PlayerInputAndActionIntent'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInputAndActionIntent' 'PlayerControlAndReleaseInput' 'authoritative state/behavior' '方向、跳跃、释放、悬停和连续输入窗口。' 'Owner System/CommitPort；输入帧通过显式命令交给行为系统。' { param($row) Test-MemberName $row $playerControlAndReleaseMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInputAndActionIntent' 'PlayerItemUseAndChannelIntent' 'authoritative state/behavior' '物品使用、交互、频道、法力消耗和行动意图状态。' 'Owner System/CommitPort；物品动作意图单向进入使用系统。' { param($row) Test-MemberName $row $playerItemUseIntentMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInputAndActionIntent' 'PlayerShadowAndArmPresentation' 'registry/projection' '玩家残影、手臂合成和动画表现缓存。' 'Registry/Projection seam；表现缓存只读取输入快照，不拥有玩法状态。' { param($row) Test-MemberName $row $playerShadowAndArmMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInputAndActionIntent' 'PlayerQuestAndEventCounters' 'authoritative state/behavior' '钓鱼任务、建筑者积分和事件进度计数。' 'Owner System/CommitPort；任务和事件完成通过显式事件提交。' { param($row) Test-MemberName $row $playerQuestAndEventMembers })
)

$refinementDefinitions['PlayerAppearanceAndInformationAccessories'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceAndInformationAccessories' 'PlayerAppearanceCustomizationState' 'authoritative state/behavior' '发型、染色和角色颜色定制状态。' 'Owner System/CommitPort；外观变更通过玩家配置命令提交。' { param($row) Test-MemberName $row $playerAppearanceCustomizationMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceAndInformationAccessories' 'PlayerInformationAccessoryState' 'authoritative state/behavior' '信息配饰、交互辅助、PVP 与相关表现状态。' 'Owner System/CommitPort；配饰效果按能力阶段提交。' { param($row) Test-MemberName $row $playerInformationAccessoryMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceAndInformationAccessories' 'PlayerDpsTelemetryState' 'authoritative state/behavior' '伤害统计窗口、最近命中和 DPS 累积状态。' 'Owner System/CommitPort；战斗事件驱动统计窗口更新。' { param($row) Test-MemberName $row $playerDpsTelemetryMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceAndInformationAccessories' 'PlayerLuckAndCommerceEffects' 'authoritative state/behavior' '幸运、折扣、金钱配饰和相关效果状态。' 'Owner System/CommitPort；经济和幸运效果由显式效果提交。' { param($row) Test-MemberName $row $playerLuckAndCommerceMembers })
)

$refinementDefinitions['PlayerFishingAndMinionCapacity'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerFishingAndMinionCapacity' 'PlayerFishingCapabilityState' 'authoritative state/behavior' '钓鱼能力、鱼饵辅助和特殊钓鱼配饰状态。' 'Owner System/CommitPort；钓鱼尝试读取能力快照并提交结果。' { param($row) Test-MemberName $row $playerFishingCapabilityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerFishingAndMinionCapacity' 'PlayerMinionCapacityAndSummonState' 'authoritative state/behavior' '召唤容量、召唤槽位和各类召唤物标志。' 'Owner System/CommitPort；召唤物生成通过显式命令交接。' { param($row) Test-MemberName $row $playerMinionCapacityAndSummonMembers })
)

$refinementDefinitions['PlayerDashRopeAndCarpetTraversal'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerDashRopeAndCarpetTraversal' 'PlayerTeleportTransitionState' 'authoritative state/behavior' '玩家传送过渡样式、计时和未确认传送状态。' 'Owner System/CommitPort；传送事务按阶段提交并等待确认。' { param($row) Test-MemberName $row $playerTeleportTransitionMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerDashRopeAndCarpetTraversal' 'PlayerDashAndGroundTraversalState' 'authoritative state/behavior' '冲刺、落阶、斜坡和地表加速状态。' 'Owner System/CommitPort；移动转换通过显式阶段更新。' { param($row) Test-MemberName $row $playerDashAndGroundTraversalMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerDashRopeAndCarpetTraversal' 'PlayerRopeAndPulleyState' 'authoritative state/behavior' '绳索、宝石钩、滑轮和绳索附加效果状态。' 'Owner System/CommitPort；抓取和滑轮输入通过移动命令交接。' { param($row) Test-MemberName $row $playerRopeAndPulleyMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerDashRopeAndCarpetTraversal' 'PlayerSlideAndCarpetTraversalState' 'authoritative state/behavior' '滑行、冰鞋、飞毯和地表滑移状态。' 'Owner System/CommitPort；滑移状态由移动阶段统一提交。' { param($row) Test-MemberName $row $playerSlideAndCarpetMembers })
)

$refinementDefinitions['PlayerEnvironmentAndMobilityEffects'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEnvironmentAndMobilityEffects' 'PlayerEnvironmentMobilityState' 'authoritative state/behavior' '水体、跳跃、移动能力和环境移动约束。' 'Owner System/CommitPort；环境查询作为只读输入，能力状态集中提交。' { param($row) Test-MemberName $row $playerEnvironmentMobilityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEnvironmentAndMobilityEffects' 'PlayerEnvironmentDetectionAndSpawnState' 'authoritative state/behavior' '环境感知、生成规则和环境交互效果状态。' 'Owner System/CommitPort；环境扫描结果通过显式快照输入。' { param($row) Test-MemberName $row $playerEnvironmentDetectionMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEnvironmentAndMobilityEffects' 'PlayerArmorAndCombatEffects' 'authoritative state/behavior' '护甲反伤、日照、荆棘和战斗效果状态。' 'Owner System/CommitPort；受击与装备事件单向驱动效果更新。' { param($row) Test-MemberName $row $playerArmorAndCombatEffectMembers })
)

$refinementDefinitions['PlayerWingsZonesAndSocialState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerWingsZonesAndSocialState' 'PlayerWingsAndFlightState' 'authoritative state/behavior' '翅膀飞行时间、飞行帧和飞行能力状态。' 'Owner System/CommitPort；飞行阶段集中提交翅膀运行状态。' { param($row) Test-MemberName $row $playerWingsAndFlightMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerWingsZonesAndSocialState' 'PlayerZoneAndEnvironmentState' 'authoritative state/behavior' '区域位标、微光区域和环境免疫计时状态。' 'Owner System/CommitPort；区域扫描通过只读查询驱动状态提交。' { param($row) Test-MemberName $row $playerZoneAndEnvironmentMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerWingsZonesAndSocialState' 'PlayerSocialAndDefenseState' 'authoritative state/behavior' '社交表现、PVP、护甲防御和环境保护状态。' 'Owner System/CommitPort；社交和防御事件分阶段提交。' { param($row) Test-MemberName $row $playerSocialAndDefenseMembers })
)

$refinementDefinitions['PlayerPoseNetworkAndRespawnState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerPoseNetworkAndRespawnState' 'PlayerPoseAndAnimationState' 'authoritative state/behavior' '身体姿态、位置速度和动画偏移状态。' 'Owner System/CommitPort；姿态更新由帧行为系统集中提交。' { param($row) Test-MemberName $row $playerPoseAndAnimationMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerPoseNetworkAndRespawnState' 'PlayerNetworkCameraState' 'registry/projection' '网络偏移、网络摄像机目标和同步摄像机缓存。' 'Registry/Projection seam；网络投影只消费姿态快照。' { param($row) Test-MemberName $row $playerNetworkCameraMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerPoseNetworkAndRespawnState' 'PlayerDeathRespawnAndSaveState' 'authoritative state/behavior' '死亡、观战、复活计时、保存时间和受击辅助状态。' 'Owner System/CommitPort；死亡与复活事务通过显式生命周期命令提交。' { param($row) Test-MemberName $row $playerDeathRespawnAndSaveMembers })
)

$refinementDefinitions['PlayerVitalAndCombatStats'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerVitalAndCombatStats' 'PlayerVitalAndRegenState' 'authoritative state/behavior' '生命、魔力和生命/魔力回复资源状态。' 'Owner System/CommitPort；资源 Tick 是本组唯一权威写入路径。' { param($row) Test-MemberName $row $playerVitalAndRegenMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerVitalAndCombatStats' 'PlayerCombatModifierAndImmunityState' 'authoritative state/behavior' '护甲穿透、防御、免疫和战斗修正状态。' 'Owner System/CommitPort；战斗事件单向更新修正状态。' { param($row) Test-MemberName $row $playerCombatModifierAndImmunityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerVitalAndCombatStats' 'PlayerAmmoAndAccessoryEffects' 'authoritative state/behavior' '弹药消耗、箭袋、药剂和武器配饰效果状态。' 'Owner System/CommitPort；装备效果通过显式能力提交。' { param($row) Test-MemberName $row $playerAmmoAndAccessoryEffectMembers })
)

$refinementDefinitions['NpcIdentityTargetAndMovementState'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcIdentityTargetAndMovementState' 'NpcIdentityInteractionAndPresentationState' 'authoritative state/behavior' 'NPC 身份、交互、名字表现和玩家交互历史状态。' 'Owner System/CommitPort；交互命令只写入 NPC 身份与交互边界。' { param($row) Test-MemberName $row $npcIdentityInteractionMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcIdentityTargetAndMovementState' 'NpcTargetAndMovementHistoryState' 'authoritative state/behavior' 'NPC 目标移动参数、传送、重力和移动历史缓存。' 'Owner System/CommitPort；移动系统通过显式阶段提交历史状态。' { param($row) Test-MemberName $row $npcTargetAndMovementMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcIdentityTargetAndMovementState' 'NpcBossAndInvasionGlobalState' 'authoritative state/behavior' 'Boss 战斗距离、倒计时和入侵波次全局状态。' 'Owner System/CommitPort；Boss/入侵阶段通过世界事件提交。' { param($row) Test-MemberName $row $npcBossAndInvasionMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcIdentityTargetAndMovementState' 'NpcSpawnAndCritterState' 'authoritative state/behavior' '生成来源、替换资格、昆虫概率和城镇微光变体状态。' 'Owner System/CommitPort；生成资格由生成系统统一提交。' { param($row) Test-MemberName $row $npcSpawnAndCritterMembers })
)

$refinementDefinitions['NpcBuffAndStatusState'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcBuffAndStatusState' 'NpcBuffSlotAndImmunityState' 'authoritative state/behavior' 'NPC Buff 槽、Buff 时间、免疫和 Buff 展示开关。' 'Owner System/CommitPort；Buff 施加与清理集中于本组。' { param($row) Test-MemberName $row $npcBuffSlotAndImmunityMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcBuffAndStatusState' 'NpcStatusEffectAndRegenState' 'authoritative state/behavior' 'NPC 状态效果、减益、生命回复和受伤免疫效果。' 'Owner System/CommitPort；状态效果由 Tick/事件显式更新。' { param($row) Test-MemberName $row $npcStatusEffectAndRegenMembers })
)

$refinementDefinitions['ProjectileIdentityAiAndLifetime'] = @(
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileIdentityAiAndLifetime' 'ProjectileIdentityAndClassificationState' 'authoritative state/behavior' '投射物激活、所有者、类型、分类和静态身份相关状态。' 'Owner System/CommitPort；生成命令建立并提交身份状态。' { param($row) Test-MemberName $row $projectileIdentityAndClassificationMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileIdentityAiAndLifetime' 'ProjectileAiState' 'authoritative state/behavior' '投射物 AI 数组、局部 AI 和 AI 风格状态。' 'Owner System/CommitPort；AI 系统是本组唯一行为写入者。' { param($row) Test-MemberName $row $projectileAiMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileIdentityAiAndLifetime' 'ProjectileLifetimeAndRuntimeState' 'authoritative state/behavior' '投射物生命周期计时、运行步进、偏移和声音延迟状态。' 'Owner System/CommitPort；生命周期系统按 Tick 提交运行状态。' { param($row) Test-MemberName $row $projectileLifetimeAndRuntimeMembers })
)

$refinementDefinitions['ProjectileMovementAndReplication'] = @(
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileMovementAndReplication' 'ProjectileMovementAndCollisionState' 'authoritative state/behavior' '投射物运动历史、碰撞、更新步数和水体交互状态。' 'Owner System/CommitPort；运动系统集中提交碰撞与历史状态。' { param($row) Test-MemberName $row $projectileMovementAndCollisionMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileMovementAndReplication' 'ProjectileNetworkReplicationState' 'registry/projection' '投射物网络更新、网络节流和按玩家同步跳过状态。' 'Registry/Projection seam；复制层只读取投射物权威快照。' { param($row) Test-MemberName $row $projectileNetworkReplicationMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileMovementAndReplication' 'ProjectileMinionAndPresentationState' 'authoritative state/behavior' '召唤物槽位、预览实体、绘制层和玩家免疫缓存。' 'Owner System/CommitPort；召唤物与表现适配器通过显式快照交接。' { param($row) Test-MemberName $row $projectileMinionAndPresentationMembers })
)

$refinementDefinitions['ProjectileBehaviorAndTargeting'] = @(
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileBehaviorAndTargeting' 'ProjectileDamageAndElementState' 'authoritative state/behavior' '伤害类型、附魔限制、陷阱来源和 Tag 效果状态。' 'Owner System/CommitPort；战斗事件集中提交伤害标签与元素状态。' { param($row) Test-MemberName $row $projectileDamageAndElementMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileBehaviorAndTargeting' 'ProjectileAnimationAndDirectionState' 'authoritative state/behavior' '投射物帧计数、帧索引和手动方向切换状态。' 'Owner System/CommitPort；表现行为消费并提交帧状态。' { param($row) Test-MemberName $row $projectileAnimationAndDirectionMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileBehaviorAndTargeting' 'ProjectileCollisionAndTargetingState' 'authoritative state/behavior' '斜坡碰撞、穿透方向、目标命中冷却和 Banner/UUID 目标状态。' 'Owner System/CommitPort；目标选择与碰撞命令显式提交。' { param($row) Test-MemberName $row $projectileCollisionAndTargetingMembers })
)

$refinementDefinitions['PlayerProgressionAndPetEffects'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerProgressionAndPetEffects' 'PlayerVisualAndShaderEffects' 'registry/projection' '玩家着色器、光环、光标、音乐盒和钓鱼钩表现标志。' 'Registry/Projection seam；表现输出单向读取，不反写玩家权威状态。' { param($row) Test-MemberName $row $playerVisualMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerProgressionAndPetEffects' 'PlayerUnlockProgressionState' 'authoritative state/behavior' '玩家世界解锁、配方进度和超级矿车启用状态。' 'Owner System/CommitPort；解锁命令只写入本组。' { param($row) Test-MemberName $row $playerProgressionMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerProgressionAndPetEffects' 'PlayerPetAndCompanionState' 'authoritative state/behavior' '玩家宠物、同伴和宠物旗标状态。' 'Owner System/CommitPort；宠物选择通过显式事件或命令交接。' { param($row) ($row.Member -like 'petFlag*') -or (Test-MemberName $row $playerPetMembers) }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerProgressionAndPetEffects' 'PlayerMountAndMinecartEffects' 'authoritative state/behavior' '玩家坐骑、轨道和矿车运行效果标志。' 'Owner System/CommitPort；坐骑运行输入从 Mount 查询读取。' { param($row) Test-MemberName $row $playerMountMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerProgressionAndPetEffects' 'PlayerAccessoryProgressionEffects' 'authoritative state/behavior' '玩家配饰、套装前置和进度效果标志。' 'Owner System/CommitPort；效果计算与进度状态分离。' {
            param($row)
            $isPet = ($row.Member -like 'petFlag*') -or (Test-MemberName $row $playerPetMembers)
            return (-not (Test-MemberName $row $playerVisualMembers)) -and (-not (Test-MemberName $row $playerProgressionMembers)) -and (-not $isPet) -and (-not (Test-MemberName $row $playerMountMembers))
        })
)

$refinementDefinitions['PlayerJumpGrappleAndMobility'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpGrappleAndMobility' 'PlayerJumpVariantState' 'authoritative state/behavior' '玩家跳跃变体、跳跃窗口和跳跃执行状态。' 'Owner System/CommitPort；跳跃资格 Query 只读消费本组状态。' { param($row) ($row.Member -match '^(hasJumpOption_|canJumpAgain_|isPerformingJump_)') -or (Test-MemberName $row $playerJumpMembers) }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpGrappleAndMobility' 'PlayerGrappleAndRocketState' 'authoritative state/behavior' '抓钩索引、火箭靴计时和火箭释放状态。' 'Owner System/CommitPort；装备输入通过显式命令进入。' { param($row) ($row.Member -match '^(grappling|grapCount|rocket)') -or $row.Member -in @('rocketBoots', 'vanityRocketBoots', 'canRocket') }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpGrappleAndMobility' 'PlayerEnvironmentAndMobilityEffects' 'authoritative state/behavior' '水体、环境感知、装备机动和重力免疫相关状态。' 'Owner System/CommitPort；环境 Query 只读输入，效果写入集中管理。' { param($row) Test-MemberName $row $playerEnvironmentMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpGrappleAndMobility' 'PlayerArmorSetAndTurretState' 'authoritative state/behavior' '套装效果、炮塔容量和 Vortex 隐身状态。' 'Owner System/CommitPort；套装效果按能力阶段提交。' { param($row) Test-MemberName $row $playerArmorMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpGrappleAndMobility' 'PlayerGravityAndWaterTraversalState' 'authoritative state/behavior' '水面行走、重力方向和重力控制状态。' 'Owner System/CommitPort；移动系统通过 Query 读取，不跨组隐式写入。' { param($row) Test-MemberName $row $playerGravityMembers })
)

$refinementDefinitions['PlayerStatusMovementAndTraversalState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerStatusMovementAndTraversalState' 'PlayerDashRopeAndCarpetTraversal' 'authoritative state/behavior' '冲刺、传送过渡、绳索、滑轮、滑毯和地表移动状态。' 'Owner System/CommitPort；移动转换按显式阶段提交。' { param($row) Test-MemberName $row $playerTraversalMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerStatusMovementAndTraversalState' 'PlayerCombatProcAndImmunityState' 'authoritative state/behavior' '伤害触发、闪避、护盾、反击和战斗免疫状态。' 'Owner System/CommitPort；战斗事件单向驱动本组状态。' { param($row) Test-MemberName $row $playerCombatMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerStatusMovementAndTraversalState' 'PlayerStatusAndDebuffState' 'authoritative state/behavior' '玩家异常状态、减益、恢复和持续效果标志。' 'Owner System/CommitPort；状态效果通过显式事件和计时器更新。' {
            param($row)
            return (-not (Test-MemberName $row $playerTraversalMembers)) -and (-not (Test-MemberName $row $playerCombatMembers))
        })
)

$refinementDefinitions['PlayerContainerWorldInteraction'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerContainerWorldInteraction' 'PlayerAppearanceProjectionSlots' 'registry/projection' '玩家外观装备、身体部位和表现投影槽索引。' 'Registry/Projection seam；只输出表现快照，不拥有玩法状态。' { param($row) $row.Member -match $playerAppearanceSlotPattern }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerContainerWorldInteraction' 'PlayerPortalAndTargetingState' 'authoritative state/behavior' '传送门物理、传送塔样式、召唤物目标和跨实体目标索引。' 'Owner System/CommitPort；目标选择通过 Query/Command 交接。' { param($row) Test-MemberName $row $playerPortalTargetMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerContainerWorldInteraction' 'PlayerItemActionTimingState' 'authoritative state/behavior' '掉落、药水、工具、物品动作和机关交互计时。' 'Owner System/CommitPort；时间推进由显式 Tick 输入驱动。' { param($row) Test-MemberName $row $playerItemTimingMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerContainerWorldInteraction' 'PlayerContainerAndWorldAnchorState' 'authoritative state/behavior' '箱体、TileEntity、住房交互、坐卧辅助和世界锚点状态。' 'Owner System/CommitPort；容器和世界交互通过命令提交。' {
            param($row)
            return (-not ($row.Member -match $playerAppearanceSlotPattern)) -and (-not (Test-MemberName $row $playerPortalTargetMembers)) -and (-not (Test-MemberName $row $playerItemTimingMembers))
        })
)

$refinementDefinitions['PlayerInventoryEquipmentAndBuffSlots'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInventoryEquipmentAndBuffSlots' 'PlayerInventoryAndContainerSlots' 'authoritative state/behavior' '玩家背包、银行、垃圾栏和虚空储存槽。' 'Owner System/CommitPort；容器写入集中在物品命令边界。' { param($row) Test-MemberName $row $playerInventoryMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInventoryEquipmentAndBuffSlots' 'PlayerEquipmentAndDyeSlots' 'authoritative state/behavior' '装备、染料和杂项装备槽。' 'Owner System/CommitPort；装备变更通过显式装备命令提交。' { param($row) Test-MemberName $row $playerEquipmentMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInventoryEquipmentAndBuffSlots' 'PlayerBuffAndResourceSlots' 'authoritative state/behavior' 'Buff 槽、呼吸、岩浆和水体资源槽。' 'Owner System/CommitPort；资源计时由状态系统推进。' { param($row) Test-MemberName $row $playerBuffResourceMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInventoryEquipmentAndBuffSlots' 'PlayerEquipmentPresentationState' 'registry/projection' '装备效果表现、潜行和展示实体投影状态。' 'Registry/Projection seam；表现读取不能反向修改装备权威状态。' {
            param($row)
            return (-not (Test-MemberName $row $playerInventoryMembers)) -and (-not (Test-MemberName $row $playerEquipmentMembers)) -and (-not (Test-MemberName $row $playerBuffResourceMembers))
        })
)

$refinementDefinitions['NpcCombatAndBehaviorState'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcCombatAndBehaviorState' 'NpcAiTargetAndIdentityState' 'authoritative state/behavior' 'NPC 类型、网络身份、AI、目标和生命期行为状态。' 'Owner System/CommitPort；AI 只通过查询读取战斗和世界输入。' { param($row) Test-MemberName $row $npcAiMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcCombatAndBehaviorState' 'NpcCombatAndLifeState' 'authoritative state/behavior' 'NPC 伤害、防御、生命、友敌、抗性和受击状态。' 'Owner System/CommitPort；战斗提交集中于本组。' { param($row) Test-MemberName $row $npcCombatMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcCombatAndBehaviorState' 'NpcCollisionAndPresentationState' 'authoritative state/behavior' 'NPC 碰撞、帧、朝向、缩放和显示状态。' 'Owner System/CommitPort；碰撞和表现读取共享只读快照。' { param($row) Test-MemberName $row $npcCollisionMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcCombatAndBehaviorState' 'NpcTownHousingAndBreathState' 'authoritative state/behavior' 'NPC 城镇住房、门交互和呼吸状态。' 'Owner System/CommitPort；城镇和环境生命周期单向更新。' { param($row) Test-MemberName $row $npcTownMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcCombatAndBehaviorState' 'NpcPortalAndSpecialBehaviorState' 'authoritative state/behavior' 'NPC 传送、事件特化、洞穴类型和 Boss 特殊行为状态。' 'Owner System/CommitPort；特殊行为由事件命令驱动。' {
            param($row)
            return (-not (Test-MemberName $row $npcAiMembers)) -and (-not (Test-MemberName $row $npcCombatMembers)) -and (-not (Test-MemberName $row $npcCollisionMembers)) -and (-not (Test-MemberName $row $npcTownMembers))
        })
)

$refinementDefinitions['NpcTownAndWorldProgression'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcTownAndWorldProgression' 'NpcTownRescueAndSpawnUnlocks' 'authoritative state/behavior' '城镇 NPC 救援、购买和自然生成解锁标志。' 'Owner System/CommitPort；救援和解锁事件集中提交。' { param($row) $row.Member -match $npcTownUnlockPattern }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcTownAndWorldProgression' 'NpcTowerAndEventShieldState' 'authoritative state/behavior' '天界塔活动、护盾和事件阶段状态。' 'Owner System/CommitPort；事件阶段由世界进度命令提交。' { param($row) Test-MemberName $row $npcTowerMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcTownAndWorldProgression' 'NpcBossAndWorldProgressionFlags' 'authoritative state/behavior' 'Boss 击败、战斗手册和全局 NPC 世界进度标志。' 'Owner System/CommitPort；世界进度只通过显式事件变更。' {
            param($row)
            return (-not ($row.Member -match $npcTownUnlockPattern)) -and (-not (Test-MemberName $row $npcTowerMembers))
        })
)

$refinementDefinitions['MountRuntimeAndAbilityState'] = @(
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountRuntimeAndAbilityState' 'MountStaticAndDrillConstants' 'definition/query' '坐骑帧常量、矿车常量、钻头参数和静态坐骑索引。' '只读 Definition/Catalog view；运行时系统消费，不能反向写入配置。' { param($row) $row.Kind -eq 'field' -and $row.Declaration -match '\b(?:const|static)\b' }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountRuntimeAndAbilityState' 'MountRuntimeFrameAndFlightState' 'authoritative state/behavior' '坐骑运行时类型、帧、飞行、闲置和激活状态。' 'Owner System/CommitPort；移动阶段集中写入运行时状态。' { param($row) $row.Kind -eq 'field' -and $row.Member -in @('_data', '_type', '_flipDraw', '_frame', '_frameCounter', '_frameExtra', '_frameExtraCounter', '_frameState', '_flyTime', '_idleTime', '_idleTimeNext', '_shouldSuperCart', '_walkingGraceTimeLeft', '_mountSpecificData', '_active') }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountRuntimeAndAbilityState' 'MountFatigueAndAbilityState' 'authoritative state/behavior' '坐骑疲劳、蓄力、冷却、持续时间和瞄准状态。' 'Owner System/CommitPort；能力转换按显式输入和冷却阶段更新。' { param($row) $row.Kind -eq 'field' -and $row.Member -in @('_fatigue', '_fatigueMax', '_abilityCharging', '_abilityCharge', '_abilityCooldown', '_abilityDuration', '_abilityActive', '_aiming') }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountRuntimeAndAbilityState' 'MountRuntimeProjectionProperties' 'derived/query' '坐骑运行时偏移、速度、轨道、翅膀和能力只读属性。' '只读快照/纯资格 Query；不得通过属性写回坐骑运行时状态。' {
            param($row)
            return ($row.Kind -eq 'property') -or (($row.Kind -eq 'field') -and $row.Member -in @('_debugDraw', '_defaultDelegatesData'))
        })
)

$refinementDefinitions['WorldRuleSeedAndSkyblockVariants'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldRuleSeedAndSkyblockVariants' 'WorldSecretSeedRegistryState' 'authoritative state/behavior' 'SecretSeed 注册、启用、解锁文本和秘密种子规则状态。' 'Owner System/CommitPort；种子注册状态是本组唯一权威写入。' { param($row) $row.Type -in $worldSecretSeedTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldRuleSeedAndSkyblockVariants' 'WorldSecretSeedDerivedVariations' 'derived/query' '由秘密种子组合派生的变体和规则资格属性。' '只读快照/纯资格 Query；不直接修改 SecretSeed 注册状态。' { param($row) $row.Type -in $worldVariationTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldRuleSeedAndSkyblockVariants' 'WorldSkyblockGenerationRules' 'definition/query' 'Skyblock 世界生成限制、Tile/Wall 规则和生成拒绝属性。' '只读 Definition/Rule view；生成 System 通过 Query 消费。' { param($row) $row.Type -in $worldSkyblockTypes })
)

$refinementDefinitions['WorldGenerationExecutionAndSnapshots'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationExecutionAndSnapshots' 'WorldGenerationProgressAndPassState' 'authoritative state/behavior' '生成进度、Pass 定义和当前 Pass 权重状态。' 'Owner System/CommitPort；生成阶段按显式 Pass 调度更新。' { param($row) $row.Type -in @('Terraria.WorldBuilding.GenerationProgress', 'Terraria.WorldBuilding.GenPass') }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationExecutionAndSnapshots' 'WorldGenerationControllerState' 'authoritative state/behavior' 'WorldGenerator 执行器、控制器、暂停和当前生成状态。' 'Owner System/CommitPort；控制命令负责暂停、恢复和中止。' { param($row) $row.Type -in @('Terraria.WorldBuilding.WorldGenerator', 'Terraria.WorldBuilding.WorldGenerator.Controller') }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationExecutionAndSnapshots' 'WorldGenerationSnapshotState' 'registry/projection' 'WorldGenSnapshot 数据、序列化配置和恢复索引。' 'Snapshot/Projection seam；快照只输出可恢复视图，不成为实时权威状态。' { param($row) $row.Type -like 'Terraria.WorldBuilding.WorldGenSnapshot*' }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationExecutionAndSnapshots' 'WorldGenerationManifestAndPassResults' 'registry/projection' '生成 Manifest、Pass 结果、哈希和耗时结果。' 'Manifest/Projection seam；结果单向发布到持久化或诊断边界。' { param($row) $row.Type -in @('Terraria.WorldBuilding.GenPassResult', 'Terraria.WorldBuilding.WorldManifest') })
)

$refinementDefinitions['WorldGenerationShapesQueriesAndModifiers'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationShapesQueriesAndModifiers' 'WorldGenerationConditionsAndSearches' 'derived/query' '世界生成条件、搜索方向和搜索约束。' '只读快照/纯资格 Query；条件和搜索不持有跨阶段可变状态。' { param($row) ($row.Type -like 'Terraria.WorldBuilding.Conditions.*') -or ($row.Type -eq 'Terraria.WorldBuilding.GenSearch') -or ($row.Type -like 'Terraria.WorldBuilding.Searches.*') }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationShapesQueriesAndModifiers' 'WorldGenerationShapeData' 'definition/query' '生成形状、ShapeData 和形状轮廓输入。' '只读 Definition/Shape view；形状查询通过值对象交接。' { param($row) ($row.Type -in @('Terraria.WorldBuilding.GenBase', 'Terraria.WorldBuilding.GenModShape', 'Terraria.WorldBuilding.GenShape', 'Terraria.WorldBuilding.ShapeData')) -or ($row.Type -like 'Terraria.WorldBuilding.ModShapes.*') }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationShapesQueriesAndModifiers' 'WorldGenerationModifiersAndActions' 'definition/query' '生成 Modifier、Tile/Wall 条件和几何动作参数。' '只读 Definition/Action view；Action 执行由生成 System 控制。' {
            param($row)
            $isConditionOrSearch = ($row.Type -like 'Terraria.WorldBuilding.Conditions.*') -or ($row.Type -eq 'Terraria.WorldBuilding.GenSearch') -or ($row.Type -like 'Terraria.WorldBuilding.Searches.*')
            $isShape = ($row.Type -in @('Terraria.WorldBuilding.GenBase', 'Terraria.WorldBuilding.GenModShape', 'Terraria.WorldBuilding.GenShape', 'Terraria.WorldBuilding.ShapeData')) -or ($row.Type -like 'Terraria.WorldBuilding.ModShapes.*')
            return (-not $isConditionOrSearch) -and (-not $isShape)
    })
)

$refinementDefinitions['PlayerPetAndCompanionState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerPetAndCompanionState' 'PlayerLegacyPetState' 'authoritative state/behavior' '传统宠物、宠物增益和早期同伴状态。' 'Owner System/CommitPort；宠物选择和清理通过显式生命周期命令交接。' { param($row) Test-MemberName $row $playerLegacyPetMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerPetAndCompanionState' 'PlayerNamedPetFlagState' 'authoritative state/behavior' '按宠物种类登记的宠物选择旗标。' 'Owner System/CommitPort；旗标更新集中于宠物选择系统。' { param($row) $row.Member -like 'petFlag*' }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerPetAndCompanionState' 'PlayerCompanionState' 'authoritative state/behavior' '同伴、坐骑宠物和特殊陪伴实体状态。' 'Owner System/CommitPort；同伴生成通过显式实体命令提交。' { param($row) Test-MemberName $row $playerCompanionMembers })
)

$refinementDefinitions['PlayerBuffAndStatusEffects'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBuffAndStatusEffects' 'PlayerElementalAndShimmerStatus' 'authoritative state/behavior' '元素、微光和环境持续状态。' 'Owner System/CommitPort；状态 Tick 负责唯一写入并维护失效时间。' { param($row) Test-MemberName $row $playerElementalAndShimmerStatusMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBuffAndStatusEffects' 'PlayerSurvivalAndControlStatus' 'authoritative state/behavior' '生存资源、控制减益、变身和交互限制状态。' 'Owner System/CommitPort；效果事件显式提交状态转换。' { param($row) Test-MemberName $row $playerSurvivalAndControlStatusMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBuffAndStatusEffects' 'PlayerAccessoryAndCombatStatus' 'authoritative state/behavior' '配饰触发、战斗免疫、法力和特殊战斗效果状态。' 'Owner System/CommitPort；战斗与装备事件单向更新本组。' { param($row) Test-MemberName $row $playerAccessoryAndCombatStatusMembers })
)

$refinementDefinitions['MountDefinitionCatalog'] = @(
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountDefinitionCatalog' 'MountGeometryAndFrameCatalog' 'definition/query' '坐骑几何偏移、玩家偏移和动画帧定义。' '只读 Definition/Catalog view；运行时移动系统按值读取。' { param($row) Test-MemberName $row $mountGeometryAndFrameMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountDefinitionCatalog' 'MountMovementAndAbilityCatalog' 'definition/query' '坐骑速度、跳跃、飞行、疲劳和能力参数。' '只读 Definition/Catalog view；能力系统不得反向修改定义。' { param($row) Test-MemberName $row $mountMovementAndAbilityMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountDefinitionCatalog' 'MountVehicleAndPresentationCatalog' 'definition/query' '矿车轨道、坐骑增益、光照和生成表现定义。' '只读 Definition/Catalog view；表现和车辆适配器单向消费。' { param($row) Test-MemberName $row $mountVehicleAndPresentationMembers })
)

$refinementDefinitions['NpcSpawnEligibilityInputs'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnEligibilityInputs' 'NpcSpawnContextAndCapacityInputs' 'authoritative state/behavior' '生成位置、时间、天气、玩家数量和入侵容量输入。' 'Spawn System/Query seam；输入快照只读，资格结果通过命令提交。' { param($row) Test-MemberName $row $npcSpawnContextAndCapacityMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnEligibilityInputs' 'NpcSpawnEnvironmentEligibilityInputs' 'derived/query' '地形、环境、地下层和特殊区域的生成资格条件。' '纯资格 Query；不直接写入 NPC 或世界状态。' { param($row) Test-MemberName $row $npcSpawnEnvironmentEligibilityMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnEligibilityInputs' 'NpcSpawnZoneAndEventEligibilityInputs' 'derived/query' 'Biome、事件区域、蜡烛和目标选择资格条件。' '纯资格 Query；区域投影只作为显式输入。' { param($row) Test-MemberName $row $npcSpawnZoneAndEventMembers })
)

$refinementDefinitions['GenVarsConfigurationAndTerrainLayers'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsConfigurationAndTerrainLayers' 'GenVarsConfigurationAndOreState' 'authoritative state/behavior' '生成配置、结构注册和矿石层级状态。' 'WorldGen System/CommitPort；生成阶段按 pass 顺序写入。' { param($row) Test-MemberName $row $genVarsConfigurationAndOreMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsConfigurationAndTerrainLayers' 'GenVarsWorldLayerAndSurfaceState' 'authoritative state/behavior' '世界表面、岩层、积雪、液体线和地貌边界状态。' 'WorldGen System/CommitPort；层级计算结果通过阶段提交。' { param($row) Test-MemberName $row $genVarsWorldLayerAndSurfaceMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsConfigurationAndTerrainLayers' 'GenVarsBeachAndOceanBoundaryState' 'authoritative state/behavior' '海滩、贝壳起点和海洋边界随机参数。' 'WorldGen System/CommitPort；海岸线 pass 是唯一写入者。' { param($row) Test-MemberName $row $genVarsBeachAndOceanBoundaryMembers })
)

$refinementDefinitions['WorldSecretSeedRegistryState'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedRegistryState' 'WorldSecretSeedDefinitions' 'definition/query' '秘密种子注册表、种子定义和解锁元数据。' '只读 Definition/Registry view；注册完成后由生成查询消费。' { param($row) Test-MemberName $row $worldSecretSeedDefinitionMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedRegistryState' 'WorldSecretSeedRuntimeRegistry' 'authoritative state/behavior' '秘密种子启用计数和运行时启用状态。' 'Owner System/CommitPort；启用状态由种子注册系统集中维护。' { param($row) Test-MemberName $row $worldSecretSeedRuntimeMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedRegistryState' 'WorldSecretSeedDerivedOptions' 'derived/query' '由秘密种子与世界规则派生的生成选项属性。' '纯资格 Query；派生属性不得写回注册状态。' { param($row) Test-MemberName $row $worldSecretSeedDerivedOptionMembers })
)

$refinementDefinitions['PlayerEquipmentAndAccessoryEffects'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEquipmentAndAccessoryEffects' 'PlayerStringAndAccessoryEffectState' 'authoritative state/behavior' '绳索、悠悠球、额外配饰和通用配饰效果状态。' 'Owner System/CommitPort；装备效果通过能力提交。' { param($row) Test-MemberName $row $playerStringAndAccessoryEffectMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEquipmentAndAccessoryEffects' 'PlayerBeetleArmorState' 'authoritative state/behavior' '甲虫套装球体、计数、攻防和动画状态。' 'Owner System/CommitPort；甲虫套装系统集中写入。' { param($row) Test-MemberName $row $playerBeetleArmorMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEquipmentAndAccessoryEffects' 'PlayerSolarAndNebulaArmorState' 'authoritative state/behavior' '日耀护盾和星云资源层级状态。' 'Owner System/CommitPort；套装效果按战斗事件提交。' { param($row) Test-MemberName $row $playerSolarAndNebulaArmorMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerEquipmentAndAccessoryEffects' 'PlayerMagnetAndUtilityAccessoryState' 'authoritative state/behavior' '磁力、生命力、工具速度和特殊配饰效果状态。' 'Owner System/CommitPort；通用配饰效果集中提交。' { param($row) Test-MemberName $row $playerMagnetAndUtilityAccessoryMembers })
)

$refinementDefinitions['WorldGenerationModifiersAndActions'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationModifiersAndActions' 'WorldGenerationShapeModifierState' 'definition/query' '几何形状、膨胀、翻转、抖动和形状遮罩 Modifier 参数。' '只读 Definition/Shape view；执行由 WorldGen System 控制。' { param($row) $row.Type -in $worldGenerationShapeModifierTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationModifiersAndActions' 'WorldGenerationTileWallConditionState' 'derived/query' 'Tile、Wall、液体、高度和接触条件 Modifier 参数。' '纯资格 Query；条件不持有跨阶段可变状态。' { param($row) $row.Type -in $worldGenerationTileWallConditionTypes })
)

$refinementDefinitions['WorldHousingAndSpawnRules'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldHousingAndSpawnRules' 'WorldHousingCountersAndScoringState' 'authoritative state/behavior' '住房扫描计数、容量阈值和房间评分状态。' 'Housing System/CommitPort；扫描阶段集中更新计数和评分。' { param($row) Test-MemberName $row $worldHousingCountersAndScoringMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldHousingAndSpawnRules' 'WorldHousingRoomSearchState' 'authoritative state/behavior' '房间坐标、门桌椅、候选点和搜索失败状态。' 'Housing Query/System seam；搜索过程通过显式快照和结果提交。' { param($row) Test-MemberName $row $worldHousingRoomSearchMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldHousingAndSpawnRules' 'WorldHousingRuleAndDiagnosticState' 'definition/query' '世界邪恶规则、仙人掌水体约束和诊断事件边界。' 'Definition/Diagnostics seam；日志事件不反向驱动住房权威状态。' { param($row) Test-MemberName $row $worldHousingRuleAndDiagnosticMembers })
)

$refinementDefinitions['NpcStatusEffectAndRegenState'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcStatusEffectAndRegenState' 'NpcStatusEffectFlags' 'authoritative state/behavior' 'NPC 元素、减益、标记和状态效果旗标。' 'Owner System/CommitPort；状态 Tick/事件集中更新旗标。' { param($row) Test-MemberName $row $npcStatusEffectFlagMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcStatusEffectAndRegenState' 'NpcRegenerationAndProtectionState' 'authoritative state/behavior' '生命回复、不可受伤、可追击和特殊保护状态。' 'Owner System/CommitPort；回复和保护规则显式排序。' { param($row) Test-MemberName $row $npcRegenerationAndProtectionMembers })
)

$refinementDefinitions['PlayerAppearanceProjectionSlots'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceProjectionSlots' 'PlayerAppearanceEquipmentProjection' 'registry/projection' '身体、装备、翅膀、坐骑和矿车的外观投影槽。' 'Projection seam；只读消费装备快照，不反向拥有装备状态。' { param($row) Test-MemberName $row $playerAppearanceEquipmentProjectionMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAppearanceProjectionSlots' 'PlayerAppearanceCompanionAndEffectProjection' 'registry/projection' '宠物、光源、配饰特效和特殊外观投影槽。' 'Projection seam；表现输出单向生成。' { param($row) Test-MemberName $row $playerAppearanceCompanionAndEffectProjectionMembers })
)

$refinementDefinitions['PlayerStatusAndDebuffState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerStatusAndDebuffState' 'PlayerManaAndAfkStatus' 'authoritative state/behavior' '法力疾病、法力回复和 AFK 计时状态。' 'Owner System/CommitPort；资源 Tick 负责唯一写入。' { param($row) Test-MemberName $row $playerManaAndAfkStatusMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerStatusAndDebuffState' 'PlayerDebuffAndRecoveryStatus' 'authoritative state/behavior' '冻结、减益、恢复、幽灵和移动阻滞状态。' 'Owner System/CommitPort；状态事件和计时器显式更新。' { param($row) Test-MemberName $row $playerDebuffAndRecoveryStatusMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerStatusAndDebuffState' 'PlayerDetectionAndCombatStatus' 'authoritative state/behavior' '危险感知、幸运、韧性、鞭子修正和战斗状态。' 'Owner System/CommitPort；战斗效果通过显式事件交接。' { param($row) Test-MemberName $row $playerDetectionAndCombatStatusMembers })
)

$refinementDefinitions['WorldGenerationConfigurationAndOptions'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationConfigurationAndOptions' 'WorldGenerationOptionBaseState' 'definition/query' '世界生成选项基类、配置根和启用、名称、描述、展示定义。' '只读 Definition/Catalog view；选项系统负责生命周期。' { param($row) ($row.Type -eq 'Terraria.WorldBuilding.AWorldGenerationOption') -or ($row.Type -eq 'Terraria.WorldBuilding.WorldGenConfiguration') }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationConfigurationAndOptions' 'WorldGenerationOptionRegistry' 'registry/projection' '世界生成选项列表和选项注册表投影。' 'Registry/Projection seam；注册表只提供只读枚举。' { param($row) $row.Type -eq 'Terraria.WorldBuilding.WorldGenerationOptions' }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationConfigurationAndOptions' 'WorldSeedOptionCatalog' 'definition/query' '各类世界种子选项及其依赖定义。' '只读 Definition/Catalog view；生成阶段通过资格 Query 消费。' { param($row) $row.Type -like 'Terraria.WorldBuilding.WorldSeedOption_*' })
)

$refinementDefinitions['WorldGenerationTileActions'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileActions' 'WorldGenerationTileMutationActions' 'authoritative state/behavior' 'Tile、Wall、液体和形状变更动作参数。' 'TileChangeCommand/CommitPort；变更按统一序列提交。' { param($row) $row.Type -in $worldGenerationTileMutationTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileActions' 'WorldGenerationTileScanAndControlActions' 'derived/query' '扫描、计数、自定义动作和执行边界控制状态。' '纯查询或显式执行命令 seam；不直接持有世界权威状态。' { param($row) $row.Type -in $worldGenerationTileScanAndControlTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileActions' 'WorldGenerationTileFramingAndDebugActions' 'registry/projection' 'Tile framing、调试绘制和表现辅助参数。' 'Projection/diagnostics seam；调试输出不反向修改生成结果。' { param($row) $row.Type -in $worldGenerationTileFramingAndDebugTypes })
)

$refinementDefinitions['WorldTerrainProfilesAndOreTiers'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldTerrainProfilesAndOreTiers' 'WorldLandmassAndTreeProfiles' 'definition/query' '地貌形状、树木生长配置和树木 profile 定义。' '只读 Definition/Profile view；生成 pass 通过值读取。' { param($row) $row.Type -in @('Terraria.WorldBuilding.LandmassData', 'Terraria.WorldGen.GrowTreeSettings', 'Terraria.WorldGen.GrowTreeSettings.Profiles') }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldTerrainProfilesAndOreTiers' 'WorldSavedOreTierState' 'authoritative state/behavior' '存档矿石层级和矿石替换状态。' 'Owner System/CommitPort；矿石层级在生成/加载边界集中写入。' { param($row) $row.Type -eq 'Terraria.WorldGen.SavedOreTiers' }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldTerrainProfilesAndOreTiers' 'WorldTileMergeCullState' 'derived/query' 'Tile 合并剔除方向和边界缓存。' '纯查询/缓存 seam；失效条件由 framing 系统显式管理。' { param($row) $row.Type -eq 'Terraria.WorldGen.TileMergeCullCache' })
)

$refinementDefinitions['GenVarsBiomeStructures'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsBiomeStructures' 'WorldGenBeachAndOceanBiomeState' 'authoritative state/behavior' '海滩、海洋洞穴和沿岸生物群系生成状态。' 'WorldGen System/CommitPort；海岸 pass 集中写入。' { param($row) Test-MemberName $row $worldGenBeachAndOceanBiomeMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsBiomeStructures' 'WorldGenUndergroundDesertStructureState' 'authoritative state/behavior' '地下沙漠、蜂巢和幼虫结构生成状态。' 'WorldGen System/CommitPort；地下沙漠 pass 负责唯一写入。' { param($row) Test-MemberName $row $worldGenUndergroundDesertStructureMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsBiomeStructures' 'WorldGenJungleStructureState' 'authoritative state/behavior' '丛林神庙、生命红木和丛林宝箱结构状态。' 'WorldGen System/CommitPort；丛林结构 pass 负责唯一写入。' { param($row) Test-MemberName $row $worldGenJungleStructureMembers })
)

$refinementDefinitions['NpcBossAndWorldProgressionFlags'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcBossAndWorldProgressionFlags' 'NpcProgressionBookAndActiveRegistryState' 'authoritative state/behavior' '战斗手册、商贩背包和活动检查登记状态。' 'WorldEvent/Registry seam；进度事件单向提交。' { param($row) Test-MemberName $row $npcProgressionBookAndActiveRegistryMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcBossAndWorldProgressionFlags' 'NpcBossDefeatProgressionState' 'authoritative state/behavior' 'Boss、入侵和事件击败进度旗标。' 'WorldProgression System/CommitPort；击败事件是唯一写入路径。' { param($row) Test-MemberName $row $npcBossDefeatProgressionMembers })
)

$refinementDefinitions['WorldGenBiomeMetricsAndCounts'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenBiomeMetricsAndCounts' 'WorldGenBiomeBackgroundAndDistanceMetrics' 'derived/query' 'Biome 背景、距离、安全边界和生成随机性指标。' '只读快照/纯 Query；指标不反向成为生成权威状态。' { param($row) Test-MemberName $row $worldGenBiomeBackgroundAndDistanceMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenBiomeMetricsAndCounts' 'WorldGenTileCountMetrics' 'derived/query' 'Tile、邪恶、血腥、善良和固体数量统计指标。' '只读快照/纯 Query；统计窗口由生成阶段显式刷新。' { param($row) Test-MemberName $row $worldGenTileCountMetricMembers })
)

$refinementDefinitions['PlayerJumpVariantState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpVariantState' 'PlayerJumpAvailabilityState' 'authoritative state/behavior' '各类额外跳跃的资格和可再次跳跃窗口。' 'Jump System/CommitPort；资格 Query 只读消费。' { param($row) ($row.Member -match '^(hasJumpOption_|canJumpAgain_)') }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpVariantState' 'PlayerJumpExecutionState' 'authoritative state/behavior' '额外跳跃、下冲和弹簧跳跃的执行状态。' 'Jump System/CommitPort；跳跃阶段按显式顺序提交。' { param($row) ($row.Member -match '^isPerformingJump_') -or $row.Member -eq 'isPerformingPogostickTricks' }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerJumpVariantState' 'PlayerJumpMobilityModifiers' 'authoritative state/behavior' '自动跳跃、最近跳跃、速度加成、额外下落和下冲计时。' 'Movement System/CommitPort；移动阶段集中写入修正。' { param($row) $row.Member -in @('downDashTime', 'autoJump', 'justJumped', 'jumpSpeedBoost', 'extraFall') })
)

$refinementDefinitions['PlayerBiomeAndZoneProperties'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBiomeAndZoneProperties' 'PlayerIdentityAndDerivedProperties' 'derived/query' '性别投影和计数归一化派生属性。' '纯派生 Query；属性不得形成第二份权威状态。' { param($row) Test-MemberName $row $playerIdentityAndDerivedPropertyMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBiomeAndZoneProperties' 'PlayerBiomeZoneProperties' 'derived/query' '地牢、邪恶、神圣、丛林、雪地和地下沙漠区域属性。' '纯资格 Query；从区域快照读取并返回只读结果。' { param($row) Test-MemberName $row $playerBiomeZonePropertyMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBiomeAndZoneProperties' 'PlayerVerticalAndWeatherZoneProperties' 'derived/query' '高度、海滩、降雨和沙尘暴区域属性。' '纯资格 Query；不直接修改世界或玩家状态。' { param($row) Test-MemberName $row $playerVerticalAndWeatherZonePropertyMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerBiomeAndZoneProperties' 'PlayerEventAndShoppingZoneProperties' 'derived/query' '事件区域、微光区域和商店区域派生属性。' '纯资格 Query；经济系统只消费结果。' { param($row) Test-MemberName $row $playerEventAndShoppingZonePropertyMembers })
)

foreach ($entry in $thirdRefinementDefinitions.GetEnumerator()) {
    if ($refinementDefinitions.Contains($entry.Key)) {
        throw "Duplicate refinement source group $($entry.Key)."
    }

    $refinementDefinitions[$entry.Key] = $entry.Value
}

$fourthRefinementDefinitions = [ordered]@{}
$fourthRefinementDefinitions['PlayerNamedPetFlagState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerNamedPetFlagState' 'PlayerBossPetFlags' 'authoritative state/behavior' 'Boss 宠物旗标及其宠物生成状态。' 'Pet System/CommitPort；Boss 宠物 Buff 生命周期集中写入。' { param($row) Test-MemberName $row $fourthPlayerBossPetFlagMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerNamedPetFlagState' 'PlayerSeasonalAndEventPetFlags' 'authoritative state/behavior' '季节事件、Old One Army 和事件宠物旗标。' 'Event Pet System/CommitPort；事件 Buff 生命周期集中写入。' { param($row) Test-MemberName $row $fourthPlayerSeasonalAndEventPetFlagMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerNamedPetFlagState' 'PlayerStandardNamedPetFlags' 'authoritative state/behavior' '常规命名宠物和常规召唤物宠物旗标。' 'Pet System/CommitPort；常规宠物效果按 Buff 事件提交。' { param($row) Test-MemberName $row $fourthPlayerStandardNamedPetFlagMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerNamedPetFlagState' 'PlayerCrossoverPetFlags' 'authoritative state/behavior' '联动内容和跨游戏宠物旗标。' 'Crossover Pet System/CommitPort；联动内容状态单向提交。' { param($row) Test-MemberName $row $fourthPlayerCrossoverPetFlagMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerNamedPetFlagState' 'PlayerWorldObjectPetFlags' 'authoritative state/behavior' '方块、巨石和特殊世界物件宠物旗标。' 'World Object Pet System/CommitPort；世界物件效果集中写入。' { param($row) Test-MemberName $row $fourthPlayerWorldObjectPetFlagMembers })
)

$fourthRefinementDefinitions['WorldSecretSeedDefinitions'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedDefinitions' 'WorldSecretSeedRegistryDefinitions' 'registry/projection' '秘密种子注册集合、文本元数据和解锁输入。' 'SecretSeed Registry/Definition view；注册表只读枚举定义。' { param($row) Test-MemberName $row $fourthWorldSecretSeedRegistryMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedDefinitions' 'WorldSecretSeedVisualAndSurfaceRules' 'definition/query' '涂色、表面、空间、降雨和冻结世界规则。' 'WorldGen Definition/Query；规则输入不持有运行时启用状态。' { param($row) Test-MemberName $row $fourthWorldSecretSeedVisualAndSurfaceMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedDefinitions' 'WorldSecretSeedTerrainAndStructureRules' 'definition/query' '地形、洞穴、结构、液体和传送器世界规则。' 'WorldGen Definition/Query；生成 pass 只读消费规则。' { param($row) Test-MemberName $row $fourthWorldSecretSeedTerrainAndStructureMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedDefinitions' 'WorldSecretSeedProgressionAndInfectionRules' 'definition/query' '进度、感染、出生点、难度和队伍生成规则。' 'WorldGen Definition/Query；进度规则不反向拥有世界结果。' { param($row) Test-MemberName $row $fourthWorldSecretSeedProgressionAndInfectionMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldSecretSeedDefinitions' 'WorldSecretSeedSeasonalRules' 'definition/query' '万圣节和圣诞节季节生成规则。' 'WorldGen Definition/Query；季节规则按生成阶段读取。' { param($row) Test-MemberName $row $fourthWorldSecretSeedSeasonalMembers })
)

$fourthRefinementDefinitions['MountRuntimeProjectionProperties'] = @(
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountRuntimeProjectionProperties' 'MountRuntimeIdentityAndFrameProjection' 'derived/query' '坐骑活动、类型、帧、玩家偏移和几何投影。' 'Mount Projection/Query；只读消费 Mount 运行时快照。' { param($row) Test-MemberName $row $fourthMountRuntimeIdentityAndFrameMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountRuntimeProjectionProperties' 'MountRuntimeMobilityAndAbilityProjection' 'derived/query' '坐骑移动、轨道、翅膀和能力计时投影。' 'Mount Ability Projection/Query；不复制 Mount 权威状态。' { param($row) Test-MemberName $row $fourthMountRuntimeMobilityAndAbilityMembers })
)

$fourthRefinementDefinitions['NpcSpawnEnvironmentEligibilityInputs'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnEnvironmentEligibilityInputs' 'NpcSpawnSpatialEligibilityInputs' 'derived/query' '地表、深度、海滩、天空和树木空间资格输入。' 'Spawn Eligibility Query；从位置快照纯计算资格。' { param($row) Test-MemberName $row $fourthNpcSpawnSpatialEligibilityMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnEnvironmentEligibilityInputs' 'NpcSpawnBiomeAndDungeonEligibilityInputs' 'derived/query' '水体、特殊生物群系和双地牢资格输入。' 'Spawn Eligibility Query；区域和地牢条件只读合并。' { param($row) Test-MemberName $row $fourthNpcSpawnBiomeAndDungeonEligibilityMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnEnvironmentEligibilityInputs' 'NpcSpawnPolicyAndEventEligibilityInputs' 'derived/query' '城镇、入侵、蠕虫、墙体和特殊事件政策输入。' 'Spawn Policy Query；政策条件不直接写入生成结果。' { param($row) Test-MemberName $row $fourthNpcSpawnPolicyAndEventEligibilityMembers })
)

$fourthRefinementDefinitions['WorldGenerationTileMutationActions'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileMutationActions' 'WorldGenerationTileSetActions' 'authoritative state/behavior' 'Tile 设置、清除、形状和固体替换动作参数。' 'Tile Change Command/CommitPort；Tile 变更统一排序提交。' { param($row) $row.Type -in $fourthWorldGenerationTileSetActionTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileMutationActions' 'WorldGenerationWallMutationActions' 'authoritative state/behavior' 'Wall 清除、设置和放置动作参数。' 'Wall Change Command/CommitPort；Wall 变更显式提交。' { param($row) $row.Type -in $fourthWorldGenerationWallMutationActionTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileMutationActions' 'WorldGenerationTilePlacementAndPaintActions' 'authoritative state/behavior' 'Tile 放置与 Tile/Wall 涂料动作参数。' 'Tile Presentation Command/CommitPort；绘制与放置保持可追踪顺序。' { param($row) $row.Type -in $fourthWorldGenerationTilePlacementAndPaintActionTypes }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationTileMutationActions' 'WorldGenerationLiquidAndNeighborActions' 'authoritative state/behavior' '液体设置和邻接平滑动作参数。' 'Liquid/Framing Command/CommitPort；邻接处理顺序显式维护。' { param($row) $row.Type -in $fourthWorldGenerationLiquidAndNeighborActionTypes })
)

$fourthRefinementDefinitions['PlayerMinionSummonFlags'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerMinionSummonFlags' 'PlayerCoreMinionSummonFlags' 'authoritative state/behavior' '核心 Terraria 召唤物和召唤物旗标。' 'Minion System/CommitPort；召唤物 Buff 事件集中写入。' { param($row) Test-MemberName $row $fourthPlayerCoreMinionSummonMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerMinionSummonFlags' 'PlayerCrossoverMinionSummonFlags' 'authoritative state/behavior' '联动召唤物旗标。' 'Crossover Minion System/CommitPort；联动召唤物状态单向提交。' { param($row) Test-MemberName $row $fourthPlayerCrossoverMinionSummonMembers })
)

$fourthRefinementDefinitions['WorldGenerationControllerState'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationControllerState' 'WorldGenerationControllerPassState' 'authoritative state/behavior' '生成 pass、当前 pass 和已完成 pass 的控制投影。' 'WorldGen Controller/PassPort；pass 生命周期集中维护。' { param($row) Test-MemberName $row $fourthWorldGenerationControllerPassMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationControllerState' 'WorldGenerationControllerPauseAndHashState' 'authoritative state/behavior' '暂停、哈希不一致和中止控制状态。' 'WorldGen Controller/ControlPort；控制命令通过锁定边界提交。' { param($row) Test-MemberName $row $fourthWorldGenerationControllerPauseAndHashMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldGenerationControllerState' 'WorldGenerationGeneratorExecutionState' 'authoritative state/behavior' '生成器配置、进度、锁、种子和结果执行状态。' 'WorldGen Execution System/CommitPort；执行状态按生成阶段更新。' { param($row) Test-MemberName $row $fourthWorldGenerationGeneratorExecutionMembers })
)

$fourthRefinementDefinitions['PlayerCombatProcAndImmunityState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCombatProcAndImmunityState' 'PlayerCombatDamageProcState' 'authoritative state/behavior' '生命窃取、命中触发和伤害反应计时状态。' 'Combat Proc System/CommitPort；攻击事件是唯一写入边界。' { param($row) Test-MemberName $row $fourthPlayerCombatDamageProcMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCombatProcAndImmunityState' 'PlayerCombatDodgeAndImmunityState' 'authoritative state/behavior' '黑带、混乱之脑和闪避免疫状态。' 'Combat Defense System/CommitPort；闪避事件显式更新。' { param($row) Test-MemberName $row $fourthPlayerCombatDodgeAndImmunityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCombatProcAndImmunityState' 'PlayerCombatBarrierAndRegenState' 'authoritative state/behavior' '冰障、冰障帧和钯金回复状态。' 'Combat Barrier System/CommitPort；护盾与回复按攻击结果提交。' { param($row) Test-MemberName $row $fourthPlayerCombatBarrierAndRegenMembers })
)

$fourthRefinementDefinitions['PlayerSpawnMovementAndTileTargeting'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerSpawnMovementAndTileTargeting' 'PlayerSpawnAndReturnState' 'authoritative state/behavior' '出生点和回城药水原始位置状态。' 'Spawn/Return System/CommitPort；出生与回城事件集中写入。' { param($row) Test-MemberName $row $fourthPlayerSpawnAndReturnMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerSpawnMovementAndTileTargeting' 'PlayerTileTargetingAndRangeState' 'authoritative state/behavior' 'Tile 交互范围、目标坐标、邻接标记和物品吸取范围。' 'Tile Interaction System/CommitPort；交互阶段显式提交范围。' { param($row) Test-MemberName $row $fourthPlayerTileTargetingAndRangeMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerSpawnMovementAndTileTargeting' 'PlayerMovementPhysicsState' 'authoritative state/behavior' '重力、跳跃、下落和奔跑物理参数。' 'Movement System/CommitPort；移动 tick 集中维护物理参数。' { param($row) Test-MemberName $row $fourthPlayerMovementPhysicsMembers })
)

$fourthRefinementDefinitions['RevengeMarkerState'] = @(
    (New-RefinementDefinition 'DeathPenaltyAndRevenge' 'RevengeMarkerState' 'RevengeMarkerExpirationAndIdentityState' 'authoritative state/behavior' '复仇标记的过期配置、唯一标识和标识投影。' 'Revenge Marker Lifecycle/CommitPort；过期与身份由标记生命周期拥有。' { param($row) Test-MemberName $row $fourthRevengeMarkerExpirationAndIdentityMembers }),
    (New-RefinementDefinition 'DeathPenaltyAndRevenge' 'RevengeMarkerState' 'RevengeMarkerEnemyContextState' 'authoritative state/behavior' '敌人位置、碰撞框、生命比例和敌人类型上下文。' 'Revenge Marker Context/Query；敌人上下文只读提供给复生资格。' { param($row) Test-MemberName $row $fourthRevengeMarkerEnemyContextMembers }),
    (New-RefinementDefinition 'DeathPenaltyAndRevenge' 'RevengeMarkerState' 'RevengeMarkerValueAndRespawnState' 'authoritative state/behavior' '金币价值和过期/复生尝试控制状态。' 'Revenge Respawn System/CommitPort；复生尝试与价值结算显式交接。' { param($row) Test-MemberName $row $fourthRevengeMarkerValueAndRespawnMembers })
)

$fourthRefinementDefinitions['WorldLifecycleAndTransformState'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldLifecycleAndTransformState' 'WorldLifecycleLoadAndTransformState' 'authoritative state/behavior' '世界加载、变换、失败和备份生命周期状态。' 'World Lifecycle System/CommitPort；加载和变换阶段集中提交。' { param($row) Test-MemberName $row $fourthWorldLifecycleLoadAndTransformMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldLifecycleAndTransformState' 'WorldLifecycleProgressionAndEventState' 'authoritative state/behavior' 'Boss、祭坛、暗影球和陨石等世界进度事件状态。' 'World Progression System/CommitPort；进度事件单向写入。' { param($row) Test-MemberName $row $fourthWorldLifecycleProgressionAndEventMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldLifecycleAndTransformState' 'WorldLifecycleHousingAndSpawnPacingState' 'authoritative state/behavior' '住房诊断、掉落许可、感染传播和 NPC 生成节奏状态。' 'World Rules System/CommitPort；世界规则按生命周期阶段更新。' { param($row) Test-MemberName $row $fourthWorldLifecycleHousingAndSpawnPacingMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'WorldLifecycleAndTransformState' 'WorldLifecycleTileMergeState' 'authoritative state/behavior' 'Tile 合并方向和合并过程状态。' 'Tile Merge System/CommitPort；合并方向由 Tile 系统集中维护。' { param($row) Test-MemberName $row $fourthWorldLifecycleTileMergeMembers })
)

$fourthRefinementDefinitions['MountAnimationFrameCatalog'] = @(
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountAnimationFrameCatalog' 'MountGroundAnimationFrames' 'definition/query' '站立、奔跑和地面空闲动画帧目录。' 'Mount Animation Query；地面帧目录只读消费。' { param($row) Test-MemberName $row $fourthMountGroundAnimationFrameMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountAnimationFrameCatalog' 'MountAerialAndWaterAnimationFrames' 'definition/query' '飞行、空中和游泳动画帧目录。' 'Mount Animation Query；空中/水中帧目录只读消费。' { param($row) Test-MemberName $row $fourthMountAerialAndWaterAnimationFrameMembers }),
    (New-RefinementDefinition 'MountAndVehicleSimulation' 'MountAnimationFrameCatalog' 'MountDashAnimationFrames' 'definition/query' '冲刺动画帧目录。' 'Mount Dash Animation Query；冲刺帧按移动状态读取。' { param($row) Test-MemberName $row $fourthMountDashAnimationFrameMembers })
)

$fourthRefinementDefinitions['NpcDamageAttributionAndCredits'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcDamageAttributionAndCredits' 'NpcDamageDefinitionRegistry' 'registry/projection' 'Boss 类型和复合 NPC 定义注册表。' 'Damage Definition Registry；注册结果只读提供给追踪系统。' { param($row) ($row.Type -eq 'Terraria.GameContent.NPCDamageTracker.CustomDefinition' -and $row.Member -in @('NPCTypes', 'Name')) -or ($row.Type -eq 'Terraria.GameContent.NPCDamageTracker' -and $row.Member -in @('CustomBossDefinitions', 'BossTypeForMob')) }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcDamageAttributionAndCredits' 'NpcDamageRuntimeTracking' 'authoritative state/behavior' '活动/已完成追踪器、攻击者和命中时间运行时状态。' 'Damage Tracker System/CommitPort；命中事件顺序化更新追踪状态。' { param($row) $row.Type -eq 'Terraria.GameContent.NPCDamageTracker' -and $row.Member -in @('_activeTrackers', '_recentFinishedTrackers', 'MAX_RECENT_TRACKERS', 'EXTRA_RECENT_TRACKER_EXPIRY_TIME', '_list', '_worldCredit', '_lastAttacker', '_ticks', '_lastHitTime', 'IsEmpty', 'Duration', 'TimeSinceLastHit') }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcDamageAttributionAndCredits' 'NpcDamageCreditProjection' 'derived/query' '玩家、世界和击杀时间的伤害 credit 投影。' 'Credit Projection/Query；投影只读消费追踪快照。' { param($row) ($row.Type -in @('Terraria.GameContent.NPCDamageTracker.CreditEntry', 'Terraria.GameContent.NPCDamageTracker.PlayerCreditEntry', 'Terraria.GameContent.NPCDamageTracker.WorldCreditEntry') -and $row.Member -in @('Damage', 'Name', 'PlayerName')) -or ($row.Type -eq 'Terraria.GameContent.NPCDamageTracker' -and $row.Member -in @('Name', 'KillTimeMessage')) })
)

$fourthRefinementDefinitions['PlayerCombatModifiersAndRanges'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCombatModifiersAndRanges' 'PlayerCombatDamageAndCritModifiers' 'authoritative state/behavior' '武器类别伤害、暴击和召唤物击退修正。' 'Combat Modifier System/CommitPort；装备计算完成后集中提交。' { param($row) Test-MemberName $row $fourthPlayerCombatDamageAndCritModifierMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerCombatModifiersAndRanges' 'PlayerCombatSpeedRangeAndPermissionState' 'authoritative state/behavior' '攻击/移动/挖掘速度、建造自动化和持物许可修正。' 'Combat/Interaction Modifier System/CommitPort；速度和范围修正显式合并。' { param($row) Test-MemberName $row $fourthPlayerCombatSpeedRangeAndPermissionMembers })
)

$fourthRefinementDefinitions['GenVarsCavesOresAndBiomes'] = @(
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsCavesOresAndBiomes' 'GenVarsCaveTunnelAndOrePatchState' 'authoritative state/behavior' '微型洞穴、隧道和矿脉 patch 生成计数与坐标。' 'WorldGen Cave/Ore System/CommitPort；生成 pass 集中更新。' { param($row) Test-MemberName $row $fourthGenVarsCaveTunnelAndOrePatchMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsCavesOresAndBiomes' 'GenVarsMushroomBiomeAndLogState' 'authoritative state/behavior' '蘑菇生物群系和树木日志生成状态。' 'WorldGen Biome System/CommitPort；蘑菇/树木 pass 集中更新。' { param($row) Test-MemberName $row $fourthGenVarsMushroomBiomeAndLogMembers }),
    (New-RefinementDefinition 'WorldGenerationAndEcology' 'GenVarsCavesOresAndBiomes' 'GenVarsLakeAndOasisState' 'authoritative state/behavior' '湖泊和绿洲生成计数、坐标及尺寸。' 'WorldGen Water/Biome System/CommitPort；水体 pass 集中更新。' { param($row) Test-MemberName $row $fourthGenVarsLakeAndOasisMembers })
)

$fourthRefinementDefinitions['LiquidFlowAndBufferState'] = @(
    (New-RefinementDefinition 'LiquidSimulation' 'LiquidFlowAndBufferState' 'LiquidFlowBudgetAndPanicState' 'authoritative state/behavior' '液体预算、循环、停滞和 panic 流程状态。' 'Liquid Flow System/CommitPort；流动 tick 集中维护预算和 panic。' { param($row) $row.Type -eq 'Terraria.Liquid' -and $row.Member -in $fourthLiquidFlowBudgetAndPanicMembers }),
    (New-RefinementDefinition 'LiquidSimulation' 'LiquidFlowAndBufferState' 'LiquidCellWorkItemState' 'authoritative state/behavior' '单个液体工作项的坐标、清除和延迟状态。' 'Liquid Work Queue/Command；工作项通过显式队列消费。' { param($row) $row.Type -eq 'Terraria.Liquid' -and $row.Member -in $fourthLiquidCellWorkItemMembers }),
    (New-RefinementDefinition 'LiquidSimulation' 'LiquidFlowAndBufferState' 'LiquidBufferQueueState' 'authoritative state/behavior' '液体缓冲队列的计数和坐标状态。' 'Liquid Buffer Queue/CommitPort；缓冲结构变化集中提交。' { param($row) $row.Type -eq 'Terraria.LiquidBuffer' -and $row.Member -in $fourthLiquidBufferQueueMembers })
)

$fourthRefinementDefinitions['NpcSpawnZoneAndEventEligibilityInputs'] = @(
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnZoneAndEventEligibilityInputs' 'NpcSpawnBiomeZoneInputs' 'derived/query' '生物群系、地形和天气区域生成资格输入。' 'Spawn Zone Query；区域快照纯计算资格。' { param($row) Test-MemberName $row $fourthNpcSpawnBiomeZoneMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnZoneAndEventEligibilityInputs' 'NpcSpawnEventAndTowerInputs' 'derived/query' '塔、旧日军、蜡烛和事件区域生成资格输入。' 'Spawn Event Query；事件区域只读提供资格。' { param($row) Test-MemberName $row $fourthNpcSpawnEventAndTowerMembers }),
    (New-RefinementDefinition 'NpcAndTownSimulation' 'NpcSpawnZoneAndEventEligibilityInputs' 'NpcSpawnTargetSelectionState' 'derived/query' '生成目标 NPC 选择状态。' 'Spawn Target Query；目标选择不修改生成上下文。' { param($row) Test-MemberName $row $fourthNpcSpawnTargetSelectionMembers })
)

$fourthRefinementDefinitions['PlayerAccessoryAndCombatStatus'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAccessoryAndCombatStatus' 'PlayerAccessoryCombatModifierState' 'authoritative state/behavior' '手套、星云/星璇披风和无人机视野等战斗配饰修正。' 'Accessory Combat System/CommitPort；装备效果集中提交。' { param($row) Test-MemberName $row $fourthPlayerAccessoryCombatModifierMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAccessoryAndCombatStatus' 'PlayerAccessoryResourceAndInvulnerabilityState' 'authoritative state/behavior' '长时间无敌、哲学之石和魔力花等资源/免疫效果。' 'Accessory Resource System/CommitPort；资源效果按装备快照更新。' { param($row) Test-MemberName $row $fourthPlayerAccessoryResourceAndInvulnerabilityMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerAccessoryAndCombatStatus' 'PlayerAccessoryDebuffAndDropState' 'authoritative state/behavior' '减益来源、枯萎、招架和掉落事件状态。' 'Accessory Status System/CommitPort；战斗状态与掉落事件显式交接。' { param($row) Test-MemberName $row $fourthPlayerAccessoryDebuffAndDropMembers })
)

$fourthRefinementDefinitions['PlayerInformationAccessoryState'] = @(
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInformationAccessoryState' 'PlayerInformationWorldAndMovementState' 'authoritative state/behavior' '敌对标志、移动声效、帧内位移和生物命中信息。' 'Player Information System/CommitPort；帧内交互状态集中更新。' { param($row) Test-MemberName $row $fourthPlayerInformationWorldAndMovementMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInformationAccessoryState' 'PlayerInformationNavigationAndTimeState' 'authoritative state/behavior' '指南针、手表、深度计、天气和计时信息。' 'Information Accessory System/CommitPort；信息配饰按观察周期更新。' { param($row) Test-MemberName $row $fourthPlayerInformationNavigationAndTimeMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInformationAccessoryState' 'PlayerInformationDetectionAndWiringState' 'authoritative state/behavior' '探测、鱼类、第三只眼、矿石、图鉴和机械线路信息。' 'Information Detection System/CommitPort；探测结果按扫描事件更新。' { param($row) Test-MemberName $row $fourthPlayerInformationDetectionAndWiringMembers }),
    (New-RefinementDefinition 'PlayerGameplay' 'PlayerInformationAccessoryState' 'PlayerFootballPresentationState' 'presentation state' '足球配饰持有和绘制表现状态。' 'Player Presentation Projection；表现状态不反向拥有配饰装备。' { param($row) Test-MemberName $row $fourthPlayerFootballPresentationMembers })
)

$fourthRefinementDefinitions['ProjectileSpecializedQueriesAndCaches'] = @(
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileSpecializedQueriesAndCaches' 'ProjectileCombatScalingState' 'authoritative state/behavior' '投射物暴击和敌对伤害缩放修正。' 'Projectile Combat System/CommitPort；难度和战斗事件集中提交。' { param($row) Test-MemberName $row $fourthProjectileCombatScalingMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileSpecializedQueriesAndCaches' 'ProjectileCollisionGeometryCache' 'derived/query' '投射物碰撞条件、长矛/鞭子/闪电几何缓存。' 'Projectile Collision Query/Cache；缓存失效由碰撞系统管理。' { param($row) Test-MemberName $row $fourthProjectileCollisionGeometryMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileSpecializedQueriesAndCaches' 'ProjectileTargetSelectionCache' 'derived/query' '彩虹巨石、Medusa 和 AI 黑名单目标缓存。' 'Projectile Target Query/Cache；目标缓存只读服务选择查询。' { param($row) Test-MemberName $row $fourthProjectileTargetSelectionMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileSpecializedQueriesAndCaches' 'ProjectileFishingAndMiningQueryState' 'derived/query' '钓鱼上下文、鱼类展示和采矿跳过点查询缓存。' 'Projectile Tool Query/Cache；工具查询结果按调用周期失效。' { param($row) Test-MemberName $row $fourthProjectileFishingAndMiningMembers }),
    (New-RefinementDefinition 'ProjectileSimulation' 'ProjectileSpecializedQueriesAndCaches' 'ProjectileKiteAndLightningRules' 'definition/query' '风筝飞行阈值和闪电液体伤害半径规则。' 'Projectile Specialized Definition/Query；规则只读提供给专用行为。' { param($row) Test-MemberName $row $fourthProjectileKiteAndLightningMembers })
)

foreach ($entry in $fourthRefinementDefinitions.GetEnumerator()) {
    if ($refinementDefinitions.Contains($entry.Key)) {
        throw "Duplicate refinement source group $($entry.Key)."
    }

    $refinementDefinitions[$entry.Key] = $entry.Value
}

$crossSeedAssignments = @(
    [pscustomobject]@{
        Parent     = 'PlayerGameplay'
        SourceFine = 'PlayerAppearanceProjectionSlots'
        Member     = 'chest'
        TargetFine = 'PlayerContainerAndWorldAnchorState'
        Reason     = 'container state belongs with world/container anchors'
    },
    [pscustomobject]@{
        Parent     = 'PlayerGameplay'
        SourceFine = 'PlayerAppearanceProjectionSlots'
        Member     = 'currentShoppingSettings'
        TargetFine = 'PlayerContainerAndWorldAnchorState'
        Reason     = 'shopping session state belongs with world/container anchors'
    },
    [pscustomobject]@{
        Parent     = 'NpcAndTownSimulation'
        SourceFine = 'NpcBuffAndStatusState'
        Member     = 'savedTaxCollector'
        TargetFine = 'NpcTownRescueState'
        Reason     = 'town rescue unlock belongs with town spawn unlocks'
    }
)

$inputFullPath = Resolve-RepositoryPath $InputPath
$seedFullPath = Resolve-RepositoryPath $SeedReportPath
$outputFullPath = Resolve-RepositoryPath $OutputPath

foreach ($path in @($inputFullPath, $seedFullPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required input does not exist: $path"
    }
}

$inputRows = @(Read-MemberRows -Path $inputFullPath -RequireFineSubsystem:$false)
$seedRows = @(Read-MemberRows -Path $seedFullPath -RequireFineSubsystem:$true)

if ($inputRows.Count -ne 2659) {
    throw "Authoritative input row count is $($inputRows.Count), expected 2659."
}

if (@($inputRows | Where-Object Kind -eq 'field').Count -ne 2397 -or @($inputRows | Where-Object Kind -eq 'property').Count -ne 262) {
    throw 'Authoritative input field/property counts do not equal 2397/262.'
}

$inputSeqs = @($inputRows | ForEach-Object Seq)
if (($inputSeqs | Sort-Object -Unique).Count -ne 2659 -or (($inputSeqs | Measure-Object -Minimum).Minimum -ne 1) -or (($inputSeqs | Measure-Object -Maximum).Maximum -ne 2659)) {
    throw 'Authoritative input sequence IDs are not a unique 1..2659 closure.'
}

$inputBySeq = @{}
foreach ($row in $inputRows) {
    if ($inputBySeq.ContainsKey($row.Seq)) {
        throw "Duplicate authoritative input sequence $($row.Seq)."
    }

    $inputBySeq[$row.Seq] = $row
}

$seedBySeq = @{}
foreach ($row in $seedRows) {
    if ($seedBySeq.ContainsKey($row.Seq)) {
        throw "Duplicate seed report sequence $($row.Seq)."
    }

    $seedBySeq[$row.Seq] = $row
}

if ($seedBySeq.Count -ne $inputBySeq.Count -or (@($seedBySeq.Keys | Sort-Object) -join ',') -ne (@($inputBySeq.Keys | Sort-Object) -join ',')) {
    throw 'Seed report sequence closure does not match the authoritative input.'
}

foreach ($seq in $inputBySeq.Keys) {
    $inputRow = $inputBySeq[$seq]
    $seedRow = $seedBySeq[$seq]
    if ($inputRow.Parent -ne $seedRow.Parent -or (Get-CellSignature $inputRow) -cne (Get-CellSignature $seedRow)) {
        throw "Seed report changes authoritative declaration cells or parent at sequence $seq."
    }
}

$seedLines = @(Get-Content -LiteralPath $seedFullPath)
$parentOrder = [System.Collections.Generic.List[string]]::new()
$parentMetadata = [ordered]@{}
$fineOrder = [System.Collections.Generic.List[string]]::new()
$fineMetadata = [ordered]@{}
$parentSummaryNotes = @{}
$inParentSummary = $false
$memberSectionIndex = -1
$parentStatsIndex = -1
$flowIndex = -1
$generatedSummaryIndex = -1

for ($index = 0; $index -lt $seedLines.Count; $index++) {
    $line = $seedLines[$index]
    if ($line -eq '## 4. 逐成员源码声明') { $memberSectionIndex = $index }
    if ($line -eq '### 3.1 父级系统统计') { $parentStatsIndex = $index; $inParentSummary = $true }
    if ($line -eq '### 3.2 调用方向和边界图（文字契约）') { $flowIndex = $index; $inParentSummary = $false }
    if ($line -match '^### 3\.3 当前活动细分子系统字段属性数量排行榜（(?:前 20|全部 \d+)）$' -and $generatedSummaryIndex -lt 0) { $generatedSummaryIndex = $index }

    if ($line -match '^### \d+\.\d+ 父级子系统：\x60([^\x60]+)\x60') {
        $parentName = $Matches[1]
        if (-not $parentMetadata.Contains($parentName)) {
            $parentOrder.Add($parentName)
            $parentMetadata[$parentName] = [ordered]@{ Name = $parentName; Duty = '' }
        }
    } elseif ($line -match '^- 父级职责：(.+)$' -and $parentOrder.Count -gt 0) {
        $parentMetadata[$parentOrder[$parentOrder.Count - 1]].Duty = $Matches[1]
    }

    if ($line -match '^#### \d+\.\d+\.\d+ 细分子系统：\x60([^\x60]+)\x60') {
        $fineName = $Matches[1]
        $fineOrder.Add($fineName)
        $fineMetadata[$fineName] = [ordered]@{
            Name           = $fineName
            Parent         = $null
            Role           = ''
            Responsibility = ''
            Seam           = ''
        }

        if ($line -match '^#### \d+\.\d+\.\d+ 细分子系统：') {
            $fineMetadata[$fineName].Parent = $parentOrder[$parentOrder.Count - 1]
        }
    } elseif ($line -match '^- 细分职责：(.+)$' -and $fineOrder.Count -gt 0) {
        $fineMetadata[$fineOrder[$fineOrder.Count - 1]].Responsibility = $Matches[1]
    } elseif ($line -match '^- 边界角色：\x60([^\x60]+)\x60；最小 seam：(.*)$' -and $fineOrder.Count -gt 0) {
        $fineMetadata[$fineOrder[$fineOrder.Count - 1]].Role = $Matches[1]
        $fineMetadata[$fineOrder[$fineOrder.Count - 1]].Seam = $Matches[2]
    }

    if ($inParentSummary -and $line -match '^\|') {
        $cells = @(Split-MarkdownRow $line)
        if ($cells.Count -eq 6 -and $cells[0] -match '^\x60([^\x60]+)\x60$') {
            $parentSummaryNotes[$Matches[1]] = $cells[5]
        }
    }
}

if ($memberSectionIndex -lt 0 -or $parentStatsIndex -lt 0 -or $flowIndex -lt 0) {
    throw 'Seed report is missing the expected summary sections.'
}

$leaderboardNames = @($secondRefinementLeaderboard | ForEach-Object Name)
$presentLeaderboardSeeds = @($leaderboardNames | Where-Object { $fineOrder -contains $_ })
if ($presentLeaderboardSeeds.Count -ne 0 -and $presentLeaderboardSeeds.Count -ne $leaderboardNames.Count) {
    throw 'Seed report contains only part of the frozen top-20 second-refinement leaderboard.'
}

if ($presentLeaderboardSeeds.Count -eq $leaderboardNames.Count) {
    foreach ($entry in $secondRefinementLeaderboard) {
        $seedRowsForEntry = @($seedRows | Where-Object Fine -eq $entry.Name)
        $seedFields = @($seedRowsForEntry | Where-Object Kind -eq 'field').Count
        $seedProperties = @($seedRowsForEntry | Where-Object Kind -eq 'property').Count
        if ($seedFields -ne $entry.Fields -or $seedProperties -ne $entry.Properties -or $seedRowsForEntry.Count -ne $entry.Total) {
            throw "Frozen leaderboard entry $($entry.Name) does not match the seed report."
        }
    }
} else {
    foreach ($entry in $secondRefinementLeaderboard) {
        foreach ($definition in $refinementDefinitions[$entry.Name]) {
            if ($fineOrder -notcontains $definition.Name) {
                if ($thirdRefinementDefinitions.Contains($definition.Name)) {
                    foreach ($grandchild in $thirdRefinementDefinitions[$definition.Name]) {
                        if ($fineOrder -notcontains $grandchild.Name) {
                            if ($fourthRefinementDefinitions.Contains($grandchild.Name)) {
                                foreach ($greatGrandchild in $fourthRefinementDefinitions[$grandchild.Name]) {
                                    if ($fineOrder -notcontains $greatGrandchild.Name) {
                                        throw "Seed report has no source group $($entry.Name) and no retained descendant $($greatGrandchild.Name)."
                                    }
                                }
                            } else {
                                throw "Seed report has no source group $($entry.Name) and no retained descendant $($grandchild.Name)."
                            }
                        }
                    }
                } elseif ($fourthRefinementDefinitions.Contains($definition.Name)) {
                    foreach ($grandchild in $fourthRefinementDefinitions[$definition.Name]) {
                        if ($fineOrder -notcontains $grandchild.Name) {
                            throw "Seed report has no source group $($entry.Name) and no retained descendant $($grandchild.Name)."
                        }
                    }
                } else {
                    throw "Seed report has no source group $($entry.Name) and no retained second-refinement sibling $($definition.Name)."
                }
            }
        }
    }
}

$currentTopTwentyNames = @($currentTopTwentyLeaderboard | ForEach-Object Name)
$presentCurrentTopTwenty = @($currentTopTwentyNames | Where-Object { $fineOrder -contains $_ })
if ($presentCurrentTopTwenty.Count -eq $currentTopTwentyNames.Count) {
    foreach ($entry in $currentTopTwentyLeaderboard) {
        $seedRowsForEntry = @($seedRows | Where-Object Fine -eq $entry.Name)
        $seedFields = @($seedRowsForEntry | Where-Object Kind -eq 'field').Count
        $seedProperties = @($seedRowsForEntry | Where-Object Kind -eq 'property').Count
        if ($seedFields -ne $entry.Fields -or $seedProperties -ne $entry.Properties -or $seedRowsForEntry.Count -ne $entry.Total) {
            throw "Current top-20 leaderboard entry $($entry.Name) does not match the seed report."
        }
    }
} else {
    foreach ($entry in $currentTopTwentyLeaderboard) {
        if ($thirdRefinementDefinitions.Contains($entry.Name)) {
            if ($fineOrder -contains $entry.Name) {
                throw "Seed report contains an unrefined current top-20 source group $($entry.Name)."
            }

            foreach ($definition in $thirdRefinementDefinitions[$entry.Name]) {
                if ($fineOrder -notcontains $definition.Name) {
                    if ($fourthRefinementDefinitions.Contains($definition.Name)) {
                        foreach ($grandchild in $fourthRefinementDefinitions[$definition.Name]) {
                            if ($fineOrder -notcontains $grandchild.Name) {
                                throw "Seed report has no current top-20 source group $($entry.Name) or retained descendant $($grandchild.Name)."
                            }
                        }
                    } else {
                        throw "Seed report has no current top-20 source group $($entry.Name) or retained sibling $($definition.Name)."
                    }
                }
            }
        } elseif ($fourthRefinementDefinitions.Contains($entry.Name)) {
            if ($fineOrder -contains $entry.Name) {
                continue
            }

            foreach ($definition in $fourthRefinementDefinitions[$entry.Name]) {
                if ($fineOrder -notcontains $definition.Name) {
                    throw "Seed report has no current top-20 source group $($entry.Name) or fourth-round sibling $($definition.Name)."
                }
            }
        } elseif ($fineOrder -notcontains $entry.Name) {
            throw "Seed report has no retained current top-20 group $($entry.Name)."
        }
    }
}

$fourthTopTwentyNames = @($fourthTopTwentyLeaderboard | ForEach-Object Name)
$presentFourthTopTwenty = @($fourthTopTwentyNames | Where-Object { $fineOrder -contains $_ })
if ($presentFourthTopTwenty.Count -eq $fourthTopTwentyNames.Count) {
    foreach ($entry in $fourthTopTwentyLeaderboard) {
        $seedRowsForEntry = @($seedRows | Where-Object Fine -eq $entry.Name)
        $seedFields = @($seedRowsForEntry | Where-Object Kind -eq 'field').Count
        $seedProperties = @($seedRowsForEntry | Where-Object Kind -eq 'property').Count
        if ($seedFields -ne $entry.Fields -or $seedProperties -ne $entry.Properties -or $seedRowsForEntry.Count -ne $entry.Total) {
            throw "Fourth-round top-20 leaderboard entry $($entry.Name) does not match the seed report."
        }
    }
} else {
    foreach ($entry in $fourthTopTwentyLeaderboard) {
        if (-not $fourthRefinementDefinitions.Contains($entry.Name)) {
            throw "Fourth-round top-20 source group $($entry.Name) has no refinement definition."
        }

        if ($fineOrder -contains $entry.Name) {
            throw "Seed report contains an unrefined fourth-round top-20 source group $($entry.Name)."
        }

        foreach ($definition in $fourthRefinementDefinitions[$entry.Name]) {
            if ($fineOrder -notcontains $definition.Name) {
                throw "Seed report has no fourth-round top-20 source group $($entry.Name) or sibling $($definition.Name)."
            }
        }
    }
}

$fineDefinitionList = [System.Collections.Generic.List[object]]::new()
foreach ($seedFine in $fineOrder) {
    if ($refinementDefinitions.Contains($seedFine)) {
        foreach ($definition in $refinementDefinitions[$seedFine]) {
            $fineDefinitionList.Add($definition)
        }
    } else {
        $fineDefinitionList.Add([pscustomobject]@{
                Parent         = $fineMetadata[$seedFine].Parent
                SourceFine     = $seedFine
                Name           = $seedFine
                Role           = $fineMetadata[$seedFine].Role
                Responsibility = $fineMetadata[$seedFine].Responsibility
                Seam           = $fineMetadata[$seedFine].Seam
                Predicate      = $null
            })
    }
}

$fineNames = @($fineDefinitionList | ForEach-Object Name)
if (($fineNames | Sort-Object -Unique).Count -ne $fineNames.Count) {
    throw 'Fine subsystem names are not unique after refinement.'
}

if ($fineDefinitionList.Count -ne 267) {
    throw "Expected 267 active fine subsystem definitions after current top-20 refinement; found $($fineDefinitionList.Count)."
}

$crossAssignmentByKey = @{}
foreach ($assignment in $crossSeedAssignments) {
    if ($fineNames -notcontains $assignment.TargetFine) {
        throw "Cross-seed assignment target $($assignment.TargetFine) is not defined in the report."
    }

    $targetDefinition = @($fineDefinitionList | Where-Object Name -eq $assignment.TargetFine)
    if ($targetDefinition.Count -ne 1 -or $targetDefinition[0].Parent -ne $assignment.Parent) {
        throw "Cross-seed assignment target $($assignment.TargetFine) has an unexpected parent."
    }

    $assignmentKey = "$($assignment.Parent)|$($assignment.SourceFine)|$($assignment.Member)"
    if ($crossAssignmentByKey.ContainsKey($assignmentKey)) {
        throw "Duplicate cross-seed assignment key $assignmentKey."
    }

    $crossAssignmentByKey[$assignmentKey] = $assignment
}

$assignedRows = [System.Collections.Generic.List[object]]::new()
$assignmentBySeq = @{}
foreach ($inputRow in $inputRows) {
    $seedFine = $seedBySeq[$inputRow.Seq].Fine
    $assignmentKey = "$($inputRow.Parent)|$seedFine|$($inputRow.Member)"
    if ($crossAssignmentByKey.ContainsKey($assignmentKey)) {
        $crossAssignment = $crossAssignmentByKey[$assignmentKey]
        $fineName = $crossAssignment.TargetFine
        $ruleName = "cross-seed:$($crossAssignment.Reason)"
    } elseif ($refinementDefinitions.Contains($seedFine)) {
        $definitions = @($refinementDefinitions[$seedFine])
        $matches = [System.Collections.Generic.List[object]]::new()
        foreach ($definition in $definitions) {
            if ([bool](& $definition.Predicate $inputRow)) {
                $matches.Add($definition)
            }
        }

        if ($matches.Count -ne 1) {
            throw "Refinement for $seedFine matched $($matches.Count) definitions at sequence $($inputRow.Seq), member $($inputRow.Member)."
        }

        $definition = $matches[0]
        if ($definition.Parent -ne $inputRow.Parent) {
            throw "Refinement $($definition.Name) assigns sequence $($inputRow.Seq) to the wrong parent."
        }

        $fineName = $definition.Name
        $ruleName = "$seedFine->$fineName"
    } else {
        $fineName = $seedFine
        $ruleName = 'seed-preserved'
    }

    if ($assignmentBySeq.ContainsKey($inputRow.Seq)) {
        throw "Sequence $($inputRow.Seq) received more than one fine subsystem assignment."
    }

    $assignmentBySeq[$inputRow.Seq] = $fineName
    $assignedRows.Add([pscustomobject]@{ Row = $inputRow; Fine = $fineName; Rule = $ruleName })
}

$assignedFineNames = @($assignedRows | ForEach-Object Fine | Sort-Object -Unique)
foreach ($definition in $fineDefinitionList) {
    if ($assignedFineNames -notcontains $definition.Name) {
        throw "Fine subsystem $($definition.Name) has no assigned members."
    }
}

foreach ($definition in $fineDefinitionList) {
    $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name)
    if (@($definitionRows | Where-Object { $_.Row.Parent -ne $definition.Parent }).Count -gt 0) {
        throw "Fine subsystem $($definition.Name) contains a row from another parent."
    }
}

foreach ($assignment in $crossSeedAssignments) {
    $crossRows = @($assignedRows | Where-Object {
            $_.Row.Parent -eq $assignment.Parent -and
            $_.Row.Member -eq $assignment.Member
        })
    if ($crossRows.Count -ne 1 -or $crossRows[0].Fine -ne $assignment.TargetFine) {
        throw "Cross-seed assignment for $($assignment.Member) did not resolve to $($assignment.TargetFine)."
    }
}

$inputHash = (Get-FileHash -LiteralPath $inputFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
$seedHash = (Get-FileHash -LiteralPath $seedFullPath -Algorithm SHA256).Hash.ToLowerInvariant()

$activeFineStats = [System.Collections.Generic.List[object]]::new()
foreach ($definition in $fineDefinitionList) {
    $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name | ForEach-Object Row)
    $activeFineStats.Add([pscustomobject]@{
            Name       = $definition.Name
            Fields     = @($definitionRows | Where-Object Kind -eq 'field').Count
            Properties = @($definitionRows | Where-Object Kind -eq 'property').Count
            Total      = $definitionRows.Count
        })
}

$outputLines = [System.Collections.Generic.List[string]]::new()
$prefixLines = @($seedLines[0..($parentStatsIndex - 1)])
foreach ($line in $prefixLines) {
    if ($line -match '^\| 本文细分子系统 \| \d+ \|$') {
        $outputLines.Add("| 本文细分子系统 | $($fineDefinitionList.Count) |")
    } elseif ($line -match '\d+ 个细分子系统的合计必须再次等于该总数。') {
        $outputLines.Add(($line -replace '\d+ 个细分子系统的合计必须再次等于该总数。', "$($fineDefinitionList.Count) 个细分子系统的合计必须再次等于该总数。"))
    } else {
        $outputLines.Add($line)
    }
}

$outputLines.Add('### 3.1 父级系统统计')
$outputLines.Add('')
$outputLines.Add('| 父级子系统 | 细分数 | 字段 | 属性 | 合计 | 说明 |')
$outputLines.Add('|---|---:|---:|---:|---:|---|')

foreach ($parent in $parentOrder) {
    $parentRows = @($inputRows | Where-Object Parent -eq $parent)
    $parentFineCount = @($fineDefinitionList | Where-Object Parent -eq $parent).Count
    $fieldCount = @($parentRows | Where-Object Kind -eq 'field').Count
    $propertyCount = @($parentRows | Where-Object Kind -eq 'property').Count
    $note = if ($parentSummaryNotes.ContainsKey($parent)) { $parentSummaryNotes[$parent] } else { $parentMetadata[$parent].Duty }
    $outputLines.Add("| ``$parent`` | $parentFineCount | $fieldCount | $propertyCount | $($parentRows.Count) | $note |")
}

$outputLines.Add('')
$flowEndIndex = if ($generatedSummaryIndex -ge 0) { $generatedSummaryIndex } else { $memberSectionIndex }
foreach ($index in $flowIndex..($flowEndIndex - 1)) {
    $outputLines.Add($seedLines[$index])
}

if ($outputLines.Count -eq 0 -or $outputLines[$outputLines.Count - 1] -ne '') {
    $outputLines.Add('')
}
$outputLines.Add("### 3.3 当前活动细分子系统字段属性数量排行榜（全部 $($activeFineStats.Count)）")
$outputLines.Add('')
$outputLines.Add("以下排行榜列出全部 $($activeFineStats.Count) 个当前活动细分子系统，计数口径为字段 + 属性；按合计数量降序、同数按子系统名称升序稳定排序。已退休来源组不计入本表。")
$outputLines.Add('')
$outputLines.Add('| 排名 | 活动细分子系统 | 字段 | 属性 | 合计 |')
$outputLines.Add('|---:|---|---:|---:|---:|')
$activeRank = 0
foreach ($stat in @($activeFineStats | Sort-Object -Property @{Expression = 'Total'; Descending = $true}, @{Expression = 'Name'; Descending = $false})) {
    $activeRank++
    $quotedName = "$([char]0x60)$($stat.Name)$([char]0x60)"
    $outputLines.Add("| $activeRank | $quotedName | $($stat.Fields) | $($stat.Properties) | $($stat.Total) |")
}

$outputLines.Add('')
$outputLines.Add('### 3.4 二次细分目标与新子系统')
$outputLines.Add('')
$outputLines.Add('每个排行榜来源组被按声明类型、成员语义、生命周期或访问边界替换为以下同级子系统；这些边界是源码库存导航契约，不代表运行时 ECS 实现已经存在。')
$outputLines.Add('')
$outputLines.Add('| 来源细分子系统 | 新子系统 | 边界角色 | 字段 | 属性 | 合计 |')
$outputLines.Add('|---|---|---|---:|---:|---:|')
foreach ($entry in $secondRefinementLeaderboard) {
    foreach ($definition in $refinementDefinitions[$entry.Name]) {
        $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name | ForEach-Object Row)
        $definitionFieldCount = @($definitionRows | Where-Object Kind -eq 'field').Count
        $definitionPropertyCount = @($definitionRows | Where-Object Kind -eq 'property').Count
        $sourceName = "$([char]0x60)$($entry.Name)$([char]0x60)"
        $fineName = "$([char]0x60)$($definition.Name)$([char]0x60)"
        $outputLines.Add("| $sourceName | $fineName | $($definition.Role) | $definitionFieldCount | $definitionPropertyCount | $($definitionRows.Count) |")
    }
}

$outputLines.Add('')
$outputLines.Add('### 3.5 当前排行榜前 20 的再次细分审查')
$outputLines.Add('')
$outputLines.Add('本表针对当前活动排行榜前 20 逐组复核职责边界；本轮 20 个来源组均已按类型族、生命周期或访问边界替换为至少两个同级子系统。')
$outputLines.Add('')
$outputLines.Add('| 排名 | 当前来源细分子系统 | 处理 | 子系统/结论 | 边界角色 | 字段 | 属性 | 合计 | 证据或不拆分理由 |')
$outputLines.Add('|---:|---|---|---|---|---:|---:|---:|---|')
foreach ($entry in $fourthTopTwentyLeaderboard) {
    $sourceName = "$([char]0x60)$($entry.Name)$([char]0x60)"
    if ($fourthRefinementDefinitions.Contains($entry.Name)) {
        foreach ($definition in $fourthRefinementDefinitions[$entry.Name]) {
            $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name | ForEach-Object Row)
            $definitionFieldCount = @($definitionRows | Where-Object Kind -eq 'field').Count
            $definitionPropertyCount = @($definitionRows | Where-Object Kind -eq 'property').Count
            $fineName = "$([char]0x60)$($definition.Name)$([char]0x60)"
            $evidence = '声明类型、成员语义或生命周期已形成独立边界。'
            $outputLines.Add("| $($entry.Rank) | $sourceName | 替换 | $fineName | $($definition.Role) | $definitionFieldCount | $definitionPropertyCount | $($definitionRows.Count) | $evidence |")
        }
    } else {
        throw "Current top-20 source group $($entry.Name) has no fourth-round refinement definition."
    }
}

$outputLines.Add('')
$outputLines.Add('### 3.6 二次细分前基线来源组排行榜（前 20，仅追溯）')
$outputLines.Add('')
$outputLines.Add('本表记录二次细分前的冻结来源组，仅用于追溯成员如何被重新分配；这些来源组不属于当前活动细分子系统。')
$outputLines.Add('')
$outputLines.Add('| 排名 | 已退休来源细分子系统 | 字段 | 属性 | 合计 |')
$outputLines.Add('|---:|---|---:|---:|---:|')
foreach ($entry in $secondRefinementLeaderboard) {
    $quotedName = "$([char]0x60)$($entry.Name)$([char]0x60)"
    $outputLines.Add("| $($entry.Rank) | $quotedName | $($entry.Fields) | $($entry.Properties) | $($entry.Total) |")
}

$outputLines.Add('')
$outputLines.Add('### 3.7 上一轮 193 组前 20 审查（历史追溯）')
$outputLines.Add('')
$outputLines.Add('本表保留 193 组报告生成时的前 20 审查结果，用于追溯第三轮细分决策；它不是本轮活动排行榜。')
$outputLines.Add('')
$outputLines.Add('| 排名 | 上一轮来源细分子系统 | 处理 | 历史子系统/结论 | 边界角色 | 原始合计 | 历史说明 |')
$outputLines.Add('|---:|---|---|---|---|---:|---|')
foreach ($entry in $currentTopTwentyLeaderboard) {
    $sourceName = "$([char]0x60)$($entry.Name)$([char]0x60)"
    if ($thirdRefinementDefinitions.Contains($entry.Name)) {
        foreach ($definition in $thirdRefinementDefinitions[$entry.Name]) {
            $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name | ForEach-Object Row)
            $fineName = "$([char]0x60)$($definition.Name)$([char]0x60)"
            $outputLines.Add("| $($entry.Rank) | $sourceName | 替换 | $fineName | $($definition.Role) | $($definitionRows.Count) | 第三轮源码成员边界仍保留为历史映射。 |")
        }
    } else {
        $reason = $currentTopTwentyNoSplitReasons[$entry.Name]
        $outputLines.Add("| $($entry.Rank) | $sourceName | 历史保留 | $sourceName | - | $($entry.Total) | $reason |")
    }
}

$outputLines.Add('## 4. 逐成员源码声明')
$outputLines.Add('')
$outputLines.Add('以下按正式父级子系统展开，再按细分子系统展开。细分子系统下的字段/属性表直接复用输入表记录；“来源序号”是输入库存中的稳定序号，保证重新分组不会丢失或复制成员。')

$parentNumber = 0
foreach ($parent in $parentOrder) {
    $parentNumber++
    $parentRows = @($inputRows | Where-Object Parent -eq $parent)
    $parentDefinitions = @($fineDefinitionList | Where-Object Parent -eq $parent)
    $fieldCount = @($parentRows | Where-Object Kind -eq 'field').Count
    $propertyCount = @($parentRows | Where-Object Kind -eq 'property').Count

    $outputLines.Add('')
    $outputLines.Add("### 4.$parentNumber 父级子系统：``$parent``")
    $outputLines.Add('')
    $outputLines.Add("- 父级职责：$($parentMetadata[$parent].Duty)")
    $outputLines.Add("- 父级统计：字段 $fieldCount；属性 $propertyCount；合计 $($parentRows.Count)；细分数 $($parentDefinitions.Count)。")

    if ($parentDefinitions.Count -gt 0) {
        $outputLines.Add('')
        $outputLines.Add('| 细分子系统 | 边界角色 | 字段 | 属性 | 合计 |')
        $outputLines.Add('|---|---|---:|---:|---:|')
        foreach ($definition in $parentDefinitions) {
            $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name | ForEach-Object Row)
            $definitionFieldCount = @($definitionRows | Where-Object Kind -eq 'field').Count
            $definitionPropertyCount = @($definitionRows | Where-Object Kind -eq 'property').Count
            $outputLines.Add("| ``$($definition.Name)`` | $($definition.Role) | $definitionFieldCount | $definitionPropertyCount | $($definitionRows.Count) |")
        }
    }

    $fineNumber = 0
    foreach ($definition in $parentDefinitions) {
        $fineNumber++
        $definitionRows = @($assignedRows | Where-Object Fine -eq $definition.Name | ForEach-Object Row | Sort-Object Seq)
        $definitionFieldCount = @($definitionRows | Where-Object Kind -eq 'field').Count
        $definitionPropertyCount = @($definitionRows | Where-Object Kind -eq 'property').Count
        $fileCount = @($definitionRows | ForEach-Object RelativePath | Sort-Object -Unique).Count
        $typeCount = @($definitionRows | ForEach-Object Type | Sort-Object -Unique).Count

        $outputLines.Add('')
        $outputLines.Add("#### 4.$parentNumber.$fineNumber 细分子系统：``$($definition.Name)``")
        $outputLines.Add('')
        $outputLines.Add("- 细分职责：$($definition.Responsibility)")
        $outputLines.Add("- 边界角色：``$($definition.Role)``；最小 seam：$($definition.Seam)")
        $outputLines.Add("- 成员文件数：$fileCount；声明类型数：$typeCount；字段：$definitionFieldCount；属性：$definitionPropertyCount；合计：$($definitionRows.Count)。")
        $outputLines.Add('- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。')

        foreach ($kind in @('field', 'property')) {
            $kindRows = @($definitionRows | Where-Object Kind -eq $kind)
            $kindLabel = if ($kind -eq 'field') { '字段' } else { '属性' }
            $outputLines.Add('')
            $outputLines.Add("##### $kindLabel（$($kindRows.Count)）")
            $outputLines.Add('')
            if ($kindRows.Count -eq 0) {
                $outputLines.Add('无该类型成员记录。')
                continue
            }

            $outputLines.Add('| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |')
            $outputLines.Add('|---:|---|---|---|---|---:|---:|---|---|---|---|')
            foreach ($row in $kindRows) {
                $outputLines.Add("| $($row.Cells -join ' | ') |")
            }
        }
    }
}

$tracePairs = [System.Collections.Generic.List[object]]::new()
if ($memberSectionIndex -ge 0) {
    $traceStart = -1
    for ($index = $memberSectionIndex; $index -lt $seedLines.Count; $index++) {
        if ($seedLines[$index] -eq '## 5. 追溯哈希') { $traceStart = $index; break }
    }

    if ($traceStart -ge 0) {
        for ($index = $traceStart; $index -lt $seedLines.Count; $index++) {
            if ($seedLines[$index] -match '^\| (.+) \| ([0-9a-fA-F]{64}) \|$') {
                if ($Matches[1] -notin @('输入', '权威成员库存输入', '细分种子报表输入')) {
                    $tracePairs.Add([pscustomobject]@{ Label = $Matches[1]; Hash = $Matches[2].ToLowerInvariant() })
                }
            }
        }
    }
}

$outputLines.Add('')
$outputLines.Add('## 5. 追溯哈希')
$outputLines.Add('')
$outputLines.Add('生成时绑定以下输入文件的 SHA-256；任一输入发生变化，都应重新生成并重新验收本文档。')
$outputLines.Add('')
$outputLines.Add('| 输入 | SHA-256 |')
$outputLines.Add('|---|---|')
$outputLines.Add("| 权威成员库存输入 | $inputHash |")
if (-not [System.String]::Equals($seedFullPath, $outputFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    $outputLines.Add("| 细分种子报表输入 | $seedHash |")
}
foreach ($pair in $tracePairs) {
    $outputLines.Add("| $($pair.Label) | $($pair.Hash) |")
}

$outputText = [string]::Join([Environment]::NewLine, $outputLines) + [Environment]::NewLine
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($outputFullPath, $outputText, $utf8NoBom)

Write-Output "Generated $outputFullPath"
Write-Output "Rows: $($assignedRows.Count); fields: $(@($assignedRows | Where-Object { $_.Row.Kind -eq 'field' }).Count); properties: $(@($assignedRows | Where-Object { $_.Row.Kind -eq 'property' }).Count)"
Write-Output "Fine subsystems: $($fineDefinitionList.Count)"
foreach ($definition in $fineDefinitionList | Where-Object { $_.SourceFine -in $refinementDefinitions.Keys }) {
    $count = @($assignedRows | Where-Object Fine -eq $definition.Name).Count
    Write-Output ("Split {0} -> {1}: {2}" -f $definition.SourceFine, $definition.Name, $count)
}
foreach ($assignment in $crossSeedAssignments) {
    $sourceRows = @($inputRows | Where-Object {
            $_.Parent -eq $assignment.Parent -and
            $_.Member -eq $assignment.Member -and
            $seedBySeq[$_.Seq].Fine -eq $assignment.SourceFine
        })
    if ($sourceRows.Count -gt 0) {
        Write-Output ("Cross-seed {0} -> {1}: {2}" -f $assignment.SourceFine, $assignment.TargetFine, $assignment.Member)
    }
}
Write-Output "Input SHA-256: $inputHash"
