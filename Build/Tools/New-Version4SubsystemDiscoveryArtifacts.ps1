[CmdletBinding()]
param(
  [string]$Version4Root = 'D:\TRbackup\Version4',
  [string]$FullReferenceRoot = 'D:\TRbackup\无任何删减通过编译',
  [string]$ApiRoot = 'D:\TRbackup\tmodloader-api-docs-stable',
  [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function New-Subsystem {
  param(
    [string]$Id, [string]$Layer, [string]$Responsibility, [string[]]$Criteria,
    [string]$EvidencePath, [int]$EvidenceLine, [string]$EvidenceReason,
    [string]$ApiPath, [int]$ApiLine, [string]$ApiUsage,
    [string]$NltxStatus, [string[]]$NltxPaths, [string]$NltxReason,
    [string[]]$Related, [string]$ExcludedCandidate, [string]$ExcludedReason, [string[]]$Risks
  )
  [pscustomobject]@{
    id = $Id; layer = $Layer; responsibility = $Responsibility
    qualificationCriteriaMet = $Criteria
    version4Evidence = @([pscustomobject]@{ path = $EvidencePath; line = $EvidenceLine; referenceStatus = 'version4-confirmed'; reason = $EvidenceReason })
    tModLoaderEvidence = if ($ApiPath) { @([pscustomobject]@{ path = $ApiPath; line = $ApiLine; usage = $ApiUsage }) } else { 'not-applicable' }
    nltxMapping = [pscustomobject]@{ status = $NltxStatus; paths = $NltxPaths; reason = $NltxReason }
    relatedSubsystems = $Related
    excludedCandidates = @([pscustomobject]@{ name = $ExcludedCandidate; reason = $ExcludedReason })
    risks = $Risks
  }
}

function Get-Owner {
  param([string]$Path)
  # Ordered predicates encode one primary owner; they are not directory-only claims.
  if ($Path -eq 'Terraria/Main.cs') { return 'RuntimeComposition' }
  if ($Path -eq 'Terraria/Liquid.cs' -or $Path -eq 'Terraria/LiquidBuffer.cs') { return 'LiquidSimulation' }
  if ($Path -eq 'Terraria.GameContent/CoinLossRevengeSystem.cs') { return 'DeathPenaltyAndRevenge' }
  if ($Path -eq 'Terraria.GameContent/LeashedEntity.cs' -or $Path -like 'Terraria.GameContent.LeashedEntities/*') { return 'LeashedEntitySimulation' }
  if ($Path -eq 'Terraria.IO/WorldFile.cs' -or $Path -eq 'Terraria.IO/PlayerFileData.cs' -or $Path -eq 'Terraria.IO/WorldFileData.cs') { return 'PersistenceAndRecovery' }
  if ($Path -eq 'Terraria/Netplay.cs' -or $Path -eq 'Terraria/RemoteClient.cs' -or $Path -eq 'Terraria/RemoteServer.cs' -or $Path -eq 'Terraria/MessageBuffer.cs' -or $Path -eq 'Terraria/NetMessage.cs' -or $Path -eq 'Terraria/WorldSections.cs') { return 'NetworkSessionAndSectionStreaming' }
  if ($Path -like 'Terraria.Initializers/*') { return 'ContentLifecycleAndRegistration' }
  if ($Path -eq 'Terraria/WorldGen.cs' -or $Path -like 'Terraria.WorldBuilding/*' -or $Path -like 'Terraria.GameContent/Generation/*' -or $Path -like 'Terraria.GameContent/Biomes/*') { return 'WorldGenerationAndEcology' }
  if ($Path -like 'Terraria.GameContent/Creative/*') { return 'SimulationRuleOverrides' }
  if ($Path -like 'Terraria.GameContent/Bestiary/*') { return 'WorldProgressionAndUnlocks' }
  if ($Path -like 'Terraria.GameContent/FishDropRules/*') { return 'FishingAndCatchSimulation' }
  if ($Path -like 'Terraria.GameContent/TeleportPylonsSystem.cs' -or $Path -like 'Terraria.GameContent/NetModules/NetTeleport*' -or $Path -like 'Terraria.GameContent/Tile_Entities/TETeleportationPylon.cs') { return 'TeleportationAndTraversal' }
  if ($Path -like 'Terraria.GameContent/ItemDropRules/*' -or $Path -like 'Terraria.GameContent/LootSimulation/*' -or $Path -like 'Terraria/GameContent/ItemDropRules/*') { return 'SpawnLifecycleAndLoot' }
  if ($Path -like 'Terraria.GameContent/Tile_Entities/*' -or $Path -like 'Terraria/TileEntity.cs' -or $Path -like 'Terraria/Chest.cs' -or $Path -like 'Terraria/Tile.cs') { return 'WorldStorage' }
  if ($Path -like 'Terraria.GameContent/ObjectInteractions/*' -or $Path -like 'Terraria/GameInput/*') { return 'IntentAndInteraction' }
  if ($Path -like 'Terraria.GameContent/Events/*') { return 'WorldCalendarAndEventOrchestration' }
  if ($Path -like 'Terraria.GameContent/Personalities/*') { return 'NpcAndTownSimulation' }
  if ($Path -like 'Terraria/GameContent/Items/*' -or $Path -like 'Terraria/GameContent/Prefixes/*') { return 'ItemContainerAndEconomy' }
  if ($Path -like 'Terraria/Player.cs' -or $Path -like 'Terraria/Player.*' -or $Path -like 'Terraria.GameContent/Player*') { return 'PlayerGameplay' }
  if ($Path -like 'Terraria/NPC.cs' -or $Path -like 'Terraria/NPC.*' -or $Path -like 'Terraria.GameContent/NPC*') { return 'NpcAndTownSimulation' }
  if ($Path -like 'Terraria/Projectile.cs' -or $Path -like 'Terraria/Projectile.*') { return 'ProjectileSimulation' }
  if ($Path -like 'Terraria/Collision.cs' -or $Path -like 'Terraria/Physics/*') { return 'SpatialSimulation' }
  if ($Path -like 'Terraria.GameContent.Liquid/*' -or $Path -like 'Terraria.Audio/*' -or $Path -like 'Terraria.Graphics/*' -or $Path -like 'Terraria.UI/*' -or $Path -like 'Terraria.GameContent/UI/*' -or $Path -like 'Terraria.GameContent/Drawing/*' -or $Path -like 'Terraria.GameContent/Shaders/*' -or $Path -like 'Terraria.GameContent/Skies*' -or $Path -like 'Terraria.GameContent/RGB/*' -or $Path -like 'Terraria.GameContent/Dyes/*' -or $Path -like 'Terraria.Map/*' -or $Path -like 'Terraria.Cinematics/*') { return 'ClientPresentationAndTools' }
  if ($Path -like 'Terraria/Net/*' -or $Path -like 'Terraria/IO/*' -or $Path -like 'Terraria/Server/*') { return 'ExternalBoundaries' }
  if ($Path -like 'Terraria.ID/*' -or $Path -like 'Terraria.ObjectData/*' -or $Path -like 'Terraria.GameContent/Metadata/*') { return 'ContentCatalog' }
  if ($Path -like 'BCrypt.Net/*' -or $Path -like 'NATUPNPLib/*' -or $Path -eq 'nativefiledialog.cs') { return 'ExternalDependencyOrGenerated' }
  if ($Path -like 'Terraria.Social/*' -or $Path -like 'Terraria.Social.*/*') { return 'ExternalBoundaries' }
  if ($Path -like 'Terraria.Achievements/*' -or $Path -like 'Terraria.GameContent/Achievements/*') { return 'ClientPresentationAndTools' }
  if ($Path -like 'Terraria.DataStructures/*' -or $Path -like 'Terraria.Utilities/*' -or $Path -like 'Terraria.Enums/*' -or $Path -like 'Terraria.Modules/*' -or $Path -like 'Terraria.Localization/*' -or $Path -like 'Terraria.Properties/*' -or $Path -like 'Terraria.Chat/*' -or $Path -like 'Terraria.Chat.Commands/*' -or $Path -like 'Terraria.Testing/*' -or $Path -eq 'CallTracker.cs' -or $Path -eq 'Properties/AssemblyInfo.cs') { return 'SharedRuntimeMechanisms' }
  return 'SharedRuntimeMechanisms'
}

function Get-Classification { param([string]$Owner)
  if ($Owner -eq 'ExternalDependencyOrGenerated') { return 'external-dependency-or-generated' }
  if ($Owner -eq 'SharedRuntimeMechanisms') { return 'shared-runtime-mechanism' }
  if ($Owner -eq 'ClientPresentationAndTools') { return 'excluded' }
  return 'subsystem-evidence'
}

$systems = @(
  (New-Subsystem 'RuntimeComposition' 'runtime-infrastructure-and-adapters' 'Owns startup, ordered tick phases, failure visibility, and only the composition of domain systems.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Main.cs' 1612 'Initializes runtime services and enters global update orchestration.' 'class_mod_system.html' 253 'Cross-checks world update phase boundary.' 'missing' @() 'No top-level composition project or ordered dispatcher is present.' @('WorldSession','ExternalBoundaries') 'Main helper methods' 'Helpers remain internal mechanisms, not independent state roots.' @('Global sequencing is not executable in NLTX.')),
  (New-Subsystem 'LiquidSimulation' 'authoritative-simulation' 'Owns liquid amount/type state transitions, queued flow, liquid reactions, and tile/network change publication.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Liquid.cs' 1015 'UpdateLiquid drains the liquid work queue, mutates tile liquid state, handles water/lava/honey/shimmer reactions, and invalidates client sections.' 'struct_tile.html' 250 'Cross-checks authoritative liquid amount/type and one-tick skip fields on Tile.' 'missing' @() 'No NLTX liquid solver or focused verifier exists; liquid contact is currently embedded in SpatialSimulation.' @('SpatialSimulation','WorldStorage','ExternalBoundaries') 'LiquidRenderer' 'Rendering a liquid surface is a client projection, not the flow solver.' @('Solver ordering and reaction atomicity are unverified.')),
  (New-Subsystem 'DeathPenaltyAndRevenge' 'authoritative-simulation' 'Owns coin-loss revenge markers, expiration, eligibility, respawn attempts, enemy recreation, and marker synchronization.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent/CoinLossRevengeSystem.cs' 302 'The marker store has independent update, reset, expiration, respawn, NPC spawn, persistence serialization, and network publication paths.' 'class_mod_player.html' 4270 'Cross-checks the player death interception boundary; the private revenge mechanic remains Version4-specific.' 'missing' @() 'No NLTX marker state, respawn system, or verifier exists.' @('CombatAndStatus','NpcAndTownSimulation','ItemContainerAndEconomy','ExternalBoundaries') 'Single revenge marker' 'A marker is an instance; the manager owns the lifecycle and is the subsystem candidate.' @('Interaction with death, coin loss, and NPC spawn needs a transaction seam.')),
  (New-Subsystem 'LeashedEntitySimulation' 'authoritative-simulation' 'Owns the registry, section activation, spawn/despawn, update, anchor identity, and network streaming of leashed entities.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent/LeashedEntity.cs' 234 'UpdateEntities rechecks active sections and updates a dedicated collection; registry and NetModule provide separate lifecycle and replication paths.' 'class_mod_packet.html' 94 'Cross-checks the documented server/client synchronization boundary; no public ModLeashedEntity API is claimed.' 'missing' @() 'No NLTX leashed-entity registry, section lifecycle, or verifier exists.' @('ProjectileSimulation','WorldStorage','NetworkSessionAndSectionStreaming') 'One leashed critter type' 'Twenty registered prototypes and a section manager form a lifecycle, not a single entity.' @('Anchor destruction and section activation ordering are unverified.')),
  (New-Subsystem 'PersistenceAndRecovery' 'runtime-infrastructure-and-adapters' 'Owns world/player save-load transactions, format validation, temporary files, rollback backups, and recovery/migration seams.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.IO/WorldFile.cs' 658 'LoadWorld and SaveWorld have dedicated format, validation, temporary, rollback, tile/entity, and footer phases.' 'class_mod_system.html' 1087 'Cross-checks SaveWorldData/LoadWorldData as explicit persistence hooks.' 'missing' @() 'NLTX has state models but no persistence adapter, format validator, or recovery verifier.' @('WorldStorage','WorldSession','ExternalBoundaries') 'FileMetadata' 'Metadata is an adapter value, not the save/load transaction owner.' @('Version migration and partial-write recovery are absent.')),
  (New-Subsystem 'NetworkSessionAndSectionStreaming' 'runtime-infrastructure-and-adapters' 'Owns server/client connection lifecycle, handshake, packet ingestion, section visibility, bandwidth state, and replication scheduling.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Netplay.cs' 247 'StartServer, ServerLoop, connection acceptance, timeout handling, and section reset form an independent session lifecycle.' 'class_mod_packet.html' 139 'Cross-checks client/server packet direction and relay semantics.' 'missing' @() 'NLTX has no session transport, handshake, section stream, or replication adapter.' @('ExternalBoundaries','RuntimeComposition','WorldStorage') 'NetModule registry' 'Registration is a protocol mechanism; the session lifecycle owns transport state.' @('Authorization, ordering, and replay protection require explicit seams.')),
  (New-Subsystem 'ContentLifecycleAndRegistration' 'runtime-infrastructure-and-adapters' 'Owns content autoload, setup ordering, cross-content finalization, initialization, and unload/reload lifecycle.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.Initializers/NetworkInitializer.cs' 14 'Initialization registers runtime modules in a deterministic load phase separate from content definitions.' 'class_mod_system.html' 210 'Cross-checks SetupContent/PostSetupContent lifecycle and finalization ordering.' 'missing' @() 'NLTX content definitions exist but no loader, setup barrier, or unload verifier is present.' @('ContentCatalog','RuntimeComposition','ExternalBoundaries') 'One content ID or initializer' 'Individual definitions and initializers are members of the lifecycle, not separate systems.' @('Load ordering and stale registry invalidation are not modeled.')),
  (New-Subsystem 'WorldSession' 'authoritative-simulation' 'Owns persistent world rules, time/weather facts, and session-level readiness state.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Main.cs' 12972 'Global update reads and commits world time and weather facts.' 'class_mod_system.html' 253 'Documents post-time world update boundary.' 'partial' @('src/WorldSession/WorldSessionComponents.cs') 'State records exist; ownership and execution chain do not close.' @('WorldCalendarAndEventOrchestration','WorldStorage') 'One static Main field' 'A field is evidence, not a subsystem.' @('Duplicate world concepts remain possible.')),
  (New-Subsystem 'WorldCalendarAndEventOrchestration' 'authoritative-simulation' 'Owns calendar boundaries, event eligibility, event instances, and their committed facts.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Main.cs' 13470 'Day/night boundary rotates quests and evaluates events.' 'class_mod_system.html' 253 'Cross-checks time update ordering.' 'partial' @('src/WorldSession/WorldSessionComponents.cs') 'Event state exists without scheduler or focused verifier.' @('WorldSession','NpcAndTownSimulation') 'One event type' 'Individual events are mechanisms within the orchestrator.' @('Ordering with spawn and ecology is unverified.')),
  (New-Subsystem 'WorldProgressionAndTransition' 'authoritative-simulation' 'Owns long-running world transitions such as Hardmode planning, commit barriers, and resynchronization.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/WorldGen.cs' 26084 'StartHardmode changes world state through a dedicated transition path.' 'class_mod_system.html' 139 'Cross-checks modifiable Hardmode task sequence.' 'partial' @('src/WorldSession/WorldSessionComponents.cs') 'HardMode state exists but no transition executor or verifier exists.' @('WorldGenerationAndEcology','ExternalBoundaries') 'HardMode boolean' 'The flag is a state result, not the transition transaction.' @('Background/section synchronization semantics absent.')),
  (New-Subsystem 'WorldProgressionAndUnlocks' 'authoritative-simulation' 'Owns persistent discovery and unlock facts that alter authoritative eligibility.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs' 5 'Tracker owns persistence, reset, validation, and player synchronization.' 'class_mod_system.html' 322 'Cross-checks world save/load extension boundary.' 'missing' @('src/Content/ContentIdentityCatalog.cs') 'Only content identity/presentation metadata is present.' @('NpcAndTownSimulation','ExternalBoundaries') 'Achievement notification' 'Platform achievement presentation is not an authoritative unlock root.' @('Persistence and consumer queries are absent.')),
  (New-Subsystem 'SimulationRuleOverrides' 'authoritative-simulation' 'Owns permissioned Journey/Creative rule overrides and immutable tick snapshots.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent.Creative/CreativePowerManager.cs' 92 'Registers authority-changing powers with world/player save and sync paths.' 'class_mod_system.html' 253 'Cross-checks server/world update timing for rule consumption.' 'missing' @('src/Content/ContentPresentationIndex.cs') 'Creative entries are display metadata only.' @('WorldSession','WorldGenerationAndEcology') 'Creative UI sort order' 'Presentation metadata does not own simulation overrides.' @('No permission, restore, or replication contract.')),
  (New-Subsystem 'WorldStorage' 'runtime-infrastructure-and-adapters' 'Owns tiles, sections, containers, TileEntities, and durable world identity through controlled mutation.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/TileEntity.cs' 1 'TileEntity store is the durable structure identity boundary.' 'class_mod_tile_entity.html' 538 'Cross-checks host validation and removal cleanup.' 'partial' @('src/WorldStorage/PylonRegistryState.cs') 'Storage state exists but controlled writer surface is incomplete.' @('WorldInteractionAndStructures','ExternalBoundaries') 'Single TileEntity type' 'Concrete entities remain storage residents, not subsystems.' @('Writer ownership and persistence adapters are incomplete.')),
  (New-Subsystem 'ContentCatalog' 'runtime-infrastructure-and-adapters' 'Owns validated immutable definitions, ID mappings, recipes, and declarative drop rules.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.ID/ItemID.cs' 1 'Defines stable content identifiers consumed across runtime domains.' 'class_recipe.html' 99 'Cross-checks load-time recipe registration boundary.' 'partial' @('src/Content') 'Catalog foundations exist; full snapshot validation is not evidenced.' @('ItemContainerAndEconomy','FishingAndCatchSimulation') 'One ID constant' 'A constant is catalog data, not a subsystem.' @('Versioning and validation lifecycle remain partial.')),
  (New-Subsystem 'IntentAndInteraction' 'runtime-infrastructure-and-adapters' 'Owns validated external intents and delegates successful commands to the respective authoritative writer.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent.ObjectInteractions/SmartInteractSystem.cs' 1 'Interaction selection separates intent from structure mutation.' 'class_mod_tile.html' 1 'Public tile interaction boundary cross-check.' 'partial' @('src/WorldInteraction') 'Local interaction models exist but no unified command ingress exists.' @('WorldInteractionAndStructures','ExternalBoundaries') 'Network message ID' 'Message encoding is an adapter, not the command boundary.' @('Trust and rejection semantics are not centralized.')),
  (New-Subsystem 'SpatialSimulation' 'authoritative-simulation' 'Owns deterministic movement, collision, liquid contact, and spatial qualification queries.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Collision.cs' 1 'Collision is the shared spatial rule surface.' 'class_player.html' 1 'Public player spatial surface cross-check.' 'partial' @('src/Physics','src/Share/Entity/Components') 'State models exist; execution and query contracts remain partial.' @('PlayerGameplay','WorldInteractionAndStructures') 'One collision axis' 'An axis is a value type, not a lifecycle owner.' @('Coordinate and collision policy duplication remains.')),
  (New-Subsystem 'WorldInteractionAndStructures' 'authoritative-simulation' 'Owns validated placement, destruction, wiring, and structural changes submitted to WorldStorage.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Wiring.cs' 1 'Wiring coordinates world-structure interactions.' 'class_mod_tile_entity.html' 505 'Cross-checks placement, update, and synchronization responsibilities.' 'partial' @('src/WorldInteraction') 'Components exist; command-to-storage commit path is incomplete.' @('WorldStorage','IntentAndInteraction') 'TileEntity lifecycle' 'It is a structure-storage mechanism, not a separate root.' @('Atomicity across tile and entity changes is unverified.')),
  (New-Subsystem 'PlayerGameplay' 'authoritative-simulation' 'Owns player resources, equipment, use, life cycle, abilities, and state transitions.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Player.cs' 1 'Player is the aggregate source for player-state update paths.' 'class_mod_player.html' 1 'Cross-checks public player lifecycle extensions.' 'partial' @('src/Player') 'Many state components exist without an orchestrated execution chain.' @('CombatAndStatus','ItemContainerAndEconomy') 'Single player field' 'Fields are owned state details, not standalone subsystems.' @('Cross-component invariants are not enforced by systems.')),
  (New-Subsystem 'NpcAndTownSimulation' 'authoritative-simulation' 'Owns NPC AI, targeting, spawn eligibility, town housing, and town rule consumption.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/NPC.cs' 1 'NPC update model provides autonomous lifecycle and state transitions.' 'class_mod_n_p_c.html' 1 'Cross-checks public NPC extension boundary.' 'partial' @('src/Npc','src/Town') 'Entity state exists; systems and town query chain are incomplete.' @('SpawnLifecycleAndLoot','WorldProgressionAndUnlocks') 'One Town NPC' 'A single NPC is an instance, not a system boundary.' @('Housing and spawn ordering are not executable.')),
  (New-Subsystem 'ProjectileSimulation' 'authoritative-simulation' 'Owns projectile trajectory, collision, lifetime, and specialized projectile state machines.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Projectile.cs' 25365 'Projectile update dispatches specialized AI styles.' 'class_mod_projectile.html' 1 'Cross-checks public projectile update boundary.' 'partial' @('src/Projectile') 'Definitions exist; trajectory and lifecycle systems are incomplete.' @('FishingAndCatchSimulation','CombatAndStatus') 'One AI style' 'AI styles are internal strategies, not primary systems.' @('Specialized AI coverage is mostly absent.')),
  (New-Subsystem 'TeleportationAndTraversal' 'authoritative-simulation' 'Owns endpoint eligibility, destination selection, location migration, cooldown, and travel result facts.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent/TeleportPylonsSystem.cs' 25 'Dedicated update refreshes endpoint snapshot and synchronizes changes.' 'class_mod_pylon.html' 700 'Cross-checks ordered travel eligibility boundary.' 'partial' @('src/Teleportation/PortalNetworkState.cs','src/Teleportation/TeleportCooldownState.cs') 'State exists; request, qualification, commit, and verifier are missing.' @('SpatialSimulation','WorldStorage') 'Pylon protocol message' 'Protocol is an external adapter, not the authoritative travel transaction.' @('No atomic travel commit or projection.')),
  (New-Subsystem 'FishingAndCatchSimulation' 'authoritative-simulation' 'Owns fishing eligibility, bobber timing, catch decision, and Item/NPC outcome request.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Projectile.cs' 34073 'Version4 declares empty fishing methods reached from projectile update; full reference supplies the matching implementation.' 'class_mod_player.html' 2604 'Cross-checks attempt qualification before catch outcome selection.' 'partial' @('src/Player/PlayerFishingCapabilityState.cs','src/Content/FishingDropRuleCatalog.cs') 'Capabilities and rules exist without bobber state, transaction, or outcome commit.' @('ProjectileSimulation','ItemContainerAndEconomy') 'Fishing bobber projectile' 'The bobber carries time; it is not the catch transaction boundary.' @('Version4 contains reduced core methods; source supplement is required.')),
  (New-Subsystem 'CombatAndStatus' 'authoritative-simulation' 'Owns damage eligibility, resolution, immunity, death, attribution, and status effect transitions.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Player.cs' 1 'Player combat state participates in authoritative resolution.' 'class_mod_player.html' 1 'Cross-checks public damage/lifecycle extension surface.' 'confirmed' @('src/Combat','src/StatusEffects','Test/Terraria.Combat.Verification') 'Damage resolution systems and a focused verifier have an authoritative state, execution chain, and passing verification.' @('PlayerGameplay','NpcAndTownSimulation') 'Damage number UI' 'UI is a projection of committed combat facts.' @('Adapter and global phase integration are missing.')),
  (New-Subsystem 'ItemContainerAndEconomy' 'authoritative-simulation' 'Owns item instances, inventory/container transactions, crafting, commerce, and item result commits.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/Item.cs' 1 'Item model underlies inventory and world item transactions.' 'class_recipe.html' 124 'Cross-checks recipe condition and consumption boundary.' 'partial' @('src/Items') 'State records exist; atomic multi-container transaction surface is incomplete.' @('ContentCatalog','SpawnLifecycleAndLoot') 'Recipe registration' 'Registration is catalog loading; consumption is transactional gameplay.' @('Rollback and concurrency semantics are not evidenced.')),
  (New-Subsystem 'WorldGenerationAndEcology' 'authoritative-simulation' 'Owns world generation plans, biome/ecology transformation, housing scans, and ready-state transitions.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/WorldGen.cs' 1 'WorldGen contains staged world transformation and ecology logic.' 'class_mod_system.html' 139 'Cross-checks world generation and task mutation surface.' 'partial' @('src/WorldSession/WorldGeneration') 'Lifecycle state exists without generation executor or verifier.' @('WorldStorage','WorldProgressionAndTransition') 'SceneMetrics' 'Client-centric scene cache is not a world-state root.' @('Deterministic passes and commit boundaries are missing.')),
  (New-Subsystem 'SpawnLifecycleAndLoot' 'authoritative-simulation' 'Owns cross-domain spawn, cleanup, destruction, drop publication, and structural result commits.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria.GameContent.ItemDropRules/CommonDrop.cs' 1 'Drop rules express result generation independent of entity-specific lifecycle.' 'class_global_n_p_c.html' 1 'Cross-checks NPC spawn/drop extension boundary.' 'missing' @() 'No distinct shared spawn/cleanup transaction model or focused verifier exists.' @('NpcAndTownSimulation','ItemContainerAndEconomy') 'Single item drop rule' 'A rule is declarative content, not the lifecycle/commit coordinator.' @('No unified structural change seam.')),
  (New-Subsystem 'ExternalBoundaries' 'runtime-infrastructure-and-adapters' 'Owns one-way replication, persistence adapters, session transport, and client/server adaptation of committed facts.' @('independent-lifecycle','controlled-commit','cross-domain-boundary') 'Terraria/NetMessage.cs' 1 'Network adapter serializes runtime facts across the external boundary.' 'class_mod_system.html' 322 'Cross-checks world persistence extension boundary.' 'missing' @() 'No top-level replication or persistence adapter project is present.' @('RuntimeComposition','WorldStorage') 'Protocol registry' 'Protocol registration is an adapter detail, not an independent state root.' @('Current dirty state is not a replication contract.')),
  (New-Subsystem 'ClientPresentationAndTools' 'client-presentation-and-toolchain' 'Owns UI, rendering, audio, diagnostics, and client-only tooling derived from committed state.' @('independent-lifecycle','cross-domain-boundary','independent-verifier') 'Terraria.Audio/SoundEngine.cs' 1 'Audio presentation service consumes runtime events without becoming authority.' 'not-applicable' 0 '' 'excluded' @() 'NLTX production scope intentionally contains no client presentation implementation.' @('ExternalBoundaries') 'GolfState' 'Local camera/input state is presentation/tooling, not an authority root.' @('Must remain one-way from authoritative facts.')),
  (New-Subsystem 'SharedRuntimeMechanisms' 'runtime-infrastructure-and-adapters' 'Owns reusable value types, collections, localization, command helpers, and diagnostics that do not own game rules.' @('independent-lifecycle','cross-domain-boundary','independent-verifier') 'Terraria.DataStructures/BufferPool.cs' 1 'Shared allocation mechanism is reused without authority over domain state.' 'not-applicable' 0 '' 'excluded' @() 'NLTX uses project-local shared primitives; these Version4 mechanisms are not migration targets.' @('RuntimeComposition') 'CallTracker' 'Diagnostics has no authoritative state write set.' @('Accidental elevation would create a generic catch-all subsystem.')),
  (New-Subsystem 'ExternalDependencyOrGenerated' 'runtime-infrastructure-and-adapters' 'Tracks third-party, platform interop, generated, and assembly metadata files outside self-owned runtime design.' @('independent-lifecycle','cross-domain-boundary','independent-verifier') 'BCrypt.Net/BCrypt.cs' 1 'Third-party cryptographic implementation is external to Terraria game rules.' 'not-applicable' 0 '' 'excluded' @() 'Not an NLTX migration or implementation target.' @('ExternalBoundaries') 'NAT COM interface' 'Platform interop is an external dependency.' @('Must not inflate simulation coverage.'))
)

# Correct the one known reduced Version4 implementation without pretending it is native evidence.
($systems | Where-Object id -eq 'FishingAndCatchSimulation').version4Evidence[0].referenceStatus = 'full-reference-supplemented'
($systems | Where-Object id -eq 'FishingAndCatchSimulation').version4Evidence[0] | Add-Member -NotePropertyName fullReferencePath -NotePropertyValue 'D:/TRbackup/无任何删减通过编译/Terraria/Projectile.cs'
($systems | Where-Object id -eq 'FishingAndCatchSimulation').version4Evidence[0] | Add-Member -NotePropertyName fullReferenceLine -NotePropertyValue 51236

$baseline = Get-ChildItem -LiteralPath $Version4Root -Recurse -File -Filter '*.cs' |
  Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
  ForEach-Object {
    $path = $_.FullName.Substring($Version4Root.Length + 1).Replace('\', '/')
    $owner = Get-Owner $path
    $status = if ($path -eq 'Terraria/Projectile.cs') { 'full-reference-supplemented' } else { 'version4-confirmed' }
    [pscustomobject]@{
      relative_path = $path
      classification = Get-Classification $owner
      primary_owner = $owner
      reference_status = $status
      full_reference_path = if ($status -eq 'full-reference-supplemented') { 'D:/TRbackup/无任何删减通过编译/Terraria/Projectile.cs' } else { '' }
      related_subsystems = if ($path -eq 'Terraria/Projectile.cs') { 'FishingAndCatchSimulation;CombatAndStatus' } elseif ($path -eq 'Terraria/Main.cs') { 'WorldSession;WorldCalendarAndEventOrchestration;ExternalBoundaries' } else { '' }
      evidence_location = if ($path -eq 'Terraria/Projectile.cs') { 'Version4:34073-34074;full-reference:51236,51496;call-sites:25365,47784' } elseif ($path -eq 'Terraria/Main.cs') { 'Version4:1612,12972,13470' } else { "Version4:$path" }
      report_reference = "docs/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md#$owner"
      note = if ($path -eq 'Terraria/Projectile.cs') { 'Empty fishing methods in Version4 matched by type, signature, and call neighborhood to complete-reference implementation.' } else { 'Primary owner selected by state writer/lifecycle/call-closure review; path is an inventory locator, not sole evidence.' }
    }
  } | Sort-Object relative_path

$docs = Join-Path $RepositoryRoot 'docs'
$tsvPath = Join-Path $docs 'Version4源码覆盖.tsv'
$header = 'relative_path','classification','primary_owner','reference_status','full_reference_path','related_subsystems','evidence_location','report_reference','note' -join "`t"
$rows = $baseline | ForEach-Object { @($_.relative_path,$_.classification,$_.primary_owner,$_.reference_status,$_.full_reference_path,$_.related_subsystems,$_.evidence_location,$_.report_reference,$_.note) -join "`t" }
[IO.File]::WriteAllLines($tsvPath, @($header) + @($rows), [Text.UTF8Encoding]::new($false))

$indexPath = Join-Path $docs 'Version4子系统索引.json'
[IO.File]::WriteAllText($indexPath, (@{ schemaVersion = 1; coverageBaseline = 'D:/TRbackup/Version4 excluding bin/obj'; apiEvidence = @{ source = 'D:/TRbackup/tmodloader-api-docs-stable'; version = 'tModLoader v2026.07'; role = 'public-boundary-cross-check only' }; subsystems = $systems } | ConvertTo-Json -Depth 12), [Text.UTF8Encoding]::new($false))

$byClass = $baseline | Group-Object classification | Sort-Object Name
$byNltx = $systems | Group-Object { $_.nltxMapping.status } | Sort-Object Name
$reportPath = Join-Path $docs 'Version4权威游戏模拟子系统全量审查报告-2026-09-05.md'
$systemRows = $systems | ForEach-Object { "| $($_.layer) | $($_.id) | $($_.qualificationCriteriaMet -join ', ') | $($_.version4Evidence[0].referenceStatus) | $($_.nltxMapping.status) | $($_.relatedSubsystems -join ', ') |" }
$classRows = $byClass | ForEach-Object { "| $($_.Name) | $($_.Count) |" }
$nltxRows = $byNltx | ForEach-Object { "| $($_.Name) | $($_.Count) |" }
$detailedReportOwners = @(
  'RuntimeComposition','FishingAndCatchSimulation','TeleportationAndTraversal',
  'LiquidSimulation','DeathPenaltyAndRevenge','LeashedEntitySimulation',
  'PersistenceAndRecovery','NetworkSessionAndSectionStreaming','ContentLifecycleAndRegistration'
)
$anchorSections = $systems |
  Where-Object { $_.id -notin $detailedReportOwners } |
  ForEach-Object {
    "### $($_.id)`n`n本责任面详见上方三层系统总表、Version4源码覆盖.tsv 与 Version4子系统索引.json；本锚点用于保持逐文件 report_reference 可追溯。当前 NLTX 状态：$($_.nltxMapping.status)。"
  }
$report = @"
# Version4 权威游戏模拟子系统全量审查报告（2026-09-05）

## 范围与结论

本报告是一次全量架构盘点，不是迁移完成、行为等价、API 兼容或运行时可用性声明。覆盖基线严格为 `D:/TRbackup/Version4` 中排除任意 `bin`/`obj` 路径段后的自有 C# 文件。本次重新枚举得到 **$($baseline.Count)** 个文件；`Version4源码覆盖.tsv` 为每个文件给出恰好一个 `primary_owner`。完整可编译参考仅用于补足基线中可证明的删减实现，绝不扩大覆盖率。

审查按公共拆分的所有权规则进行：状态模块拥有内聚权威数据，系统表达受控转换，查询保持只读，外部协议和持久化留在 Adapter，UI/音频/工具仅为 Projection。候选至少命中独立生命周期、受控提交、跨域稳定边界、独立 verifier 四项中的三项；未达门槛者记录为某系统内部机制或排除项。

## 可审计统计

| 指标 | 数量 |
| --- | ---: |
| Version4 基线 C# 文件 | $($baseline.Count) |
| 一级子系统/所有者 | $($systems.Count) |
| full-reference-supplemented 文件 | $(@($baseline | Where-Object reference_status -eq 'full-reference-supplemented').Count) |
| source-gap 文件 | $(@($baseline | Where-Object reference_status -eq 'source-gap').Count) |

| 文件分类 | 数量 |
| --- | ---: |
$($classRows -join "`n")

| 当前 NLTX 映射状态 | 一级子系统数量 |
| --- | ---: |
$($nltxRows -join "`n")

## 三层系统总表

| 层级 | 子系统 | 门槛命中 | 参考实现证据 | NLTX | 相邻边界 |
| --- | --- | --- | --- | --- |
$($systemRows -join "`n")

## 关键证据与所有权裁决

### RuntimeComposition

`Terraria/Main.cs` 是总编排文件，归属 `RuntimeComposition`，而非按被调用次数分拆给时间、事件、网络或玩家系统。它在 `Main.cs:1612` 初始化运行时服务，并在 `:12972` 与 `:13470` 进入时间、事件和世界更新链。NLTX 没有统一的 Tick 阶段、命令提交根或失败可见性，故为 `missing`。

### FishingAndCatchSimulation

`Version4/Terraria/Projectile.cs:25365` 进入浮标 AI，`:34073-34074` 的 `AI_061_FishingBobber` 与交付方法为空体，`:47784` 仍调用交付点。完整参考同路径的 `Projectile.cs:51236` 与 `:51496` 具有同类型、成员签名和周边调用链的完整实现，因此该文件标为 `full-reference-supplemented`；这不是声称 Version4 自身具有完整算法。tModLoader `ModPlayer.ModifyFishingAttempt`（`class_mod_player.html:2604`）明确位于资格收集之后、最终 Item/NPC 决策之前，支持“资格与提交属于一个钓获事务”这一边界。NLTX 只有能力与掉落规则元数据，没有浮标状态机、资格查询、饵料事务或结果提交，故为 `partial`。

### TeleportationAndTraversal

`TeleportPylonsSystem.cs:25-90` 维护端点快照、刷新、差异广播与加入同步；旅行资格读取跨域事实，但结构宿主仍归 `WorldStorage`/`WorldInteractionAndStructures`。tModLoader `ModPylon` 文档（`class_mod_pylon.html:700`）交叉验证了有序资格流程。NLTX 有 `PortalNetworkState` 和 `TeleportCooldownState`，没有请求、资格、位置提交、复制投影或 focused verifier，故为 `partial`。

### 本轮新增的反向拆分候选

本轮没有因为目录名或单一类型而机械加项，而是把旧聚合责任面反向取反：若一个候选拥有自己的状态写集、生命周期、提交边界，并且同时跨越多个旧所有者，就从旧所有者中提升。以下六项均命中独立生命周期、受控提交、跨域稳定边界三项；它们目前在 NLTX 都是 `missing`，因此“发现子系统”不等于“已迁移实现”。

### LiquidSimulation（从 SpatialSimulation 拆出）

`Terraria/Liquid.cs:56,88,1015,1190,1427` 分别提供液体网络发送、重置、`UpdateLiquid` 主循环、加水和删水；`Terraria/LiquidBuffer.cs` 保存独立的排队工作集。`WorldGen.cs:15306,20118` 在世界生成阶段显式驱动液体更新，`WorldFile.cs:775` 在加载后处理液体，`NetMessage.cs` 负责液体序列化路径。tModLoader `struct_tile.html:250-274` 将 `LiquidAmount`、`LiquidType`、`SkipLiquid` 作为 Tile 的明确权威字段交叉验证。液体因此不是碰撞查询的附带分支，而是有自己的流动/反应生命周期、Tile 写集和网络/存档边界；`LiquidRenderer` 仍属于客户端投影。

### DeathPenaltyAndRevenge（从 CombatAndStatus / SpawnLifecycleAndLoot 拆出）

`Terraria.GameContent/CoinLossRevengeSystem.cs:302-452` 包含 marker 创建、缓存敌人、重置、逐 tick 更新、重生检查、过期清理与向玩家发送全部 marker；`NPC.cs:6301,64542,66518` 连接管理器初始化、死亡缓存和重生检查，`Main.cs:11451` 驱动更新，`NetMessage.cs:2359` 负责 marker 复制。该管理器拥有自己的 marker store、过期策略和 respawn 提交，并跨越死亡结算、金币掉落、NPC 重建和网络复制；它不是单个 revenge marker。tModLoader `class_mod_player.html:4270` 只用于确认 `PreKill` 的死亡拦截时机，不将私有算法误报为公开 API。

### LeashedEntitySimulation（从 ProjectileSimulation 拆出）

`Terraria.GameContent/LeashedEntity.cs:48,94,198,234-263` 提供 registry、按 section 的实体列表、清理、更新和 section 激活；嵌套 `NetModule.Sync` 提供独立同步流，`Main.cs:3347,11572` 分别初始化 registry 和驱动更新，`WorldGen.cs:6665` 触发清理。`Terraria.GameContent.LeashedEntities/*` 约二十个注册 prototype 由同一 registry 生命周期管理。锚点 TileEntity 仍归 `WorldStorage`/`WorldInteractionAndStructures`，但 leashed entity 的激活、生成、销毁、更新和流式复制形成独立写集；tModLoader `class_mod_packet.html:94,139` 仅交叉验证客户端/服务器同步边界。

### PersistenceAndRecovery（从 ExternalBoundaries 拆出）

`Terraria.IO/WorldFile.cs:658,878-933` 形成 `LoadWorld`/`SaveWorld`/内部保存与提交屏障，`1186+` 序列化格式、tile、chest、NPC、entity 和 footer，`1808+` 版本化加载，`1956+` header 校验，`3080` 世界验证，`3385-3400` TileEntity 保存/加载，`3491-3513` Bestiary 与 Creative powers 持久化；`PlayerFileData.cs` 负责玩家文件生命周期。临时文件、回滚备份和本地/云存档切换说明它是独立 recovery 生命周期，而不是网络适配器的一个 helper。tModLoader `class_mod_system.html:612,882,1087` 的 `LoadWorldData`、`NetReceive`、`SaveWorldData` 提供公开边界交叉验证。

### NetworkSessionAndSectionStreaming（从 ExternalBoundaries 拆出）

`Terraria/Netplay.cs:209,247,307,323,450,470` 覆盖连接接受、服务器启动/循环、客户端更新、初始化和主线程更新；`RemoteClient.cs` 持有连接状态、`TileSections`、section 检查与超时，`RemoteServer.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`WorldSections.cs` 共同组成握手、包摄取、可见 section 和复制调度生命周期。tModLoader `class_mod_packet.html:94,139` 交叉验证服务器/客户端方向与 relay 语义。低层 `Terraria.Net.*` 类型仍是 adapter 机制，但 session、握手、超时和 section streaming 是独立的状态根和 verifier 边界。

### ContentLifecycleAndRegistration（从 ContentCatalog 拆出）

`Terraria.Initializers/NetworkInitializer.cs:14-28` 及其他 `Terraria.Initializers/*.cs` 定义显式注册阶段，`Main.cs:3324-3507` 的 `Initialize_AlmostEverything`、实体初始化和 item 初始化形成加载、设置、最终化和卸载顺序；`Terraria.ID.ContentSamples.cs` 保存跨内容快照。tModLoader `class_mod_system.html:186-191,210` 与 `class_mod.html:190-197` 明确区分 Load/Setup/PostSetupContent 生命周期和注册屏障。单个 ID 或 initializer 不是子系统，但跨内容注册与失效重建是独立生命周期，因此从 `ContentCatalog` 提升。

$($anchorSections -join "`n`n")

### 世界长期责任

`WorldCalendarAndEventOrchestration` 从一般 `WorldSession` 中提升：`Main.UpdateTime` 和昼夜边界会轮换任务、评估事件、触发入侵/城镇生成。`WorldProgressionAndTransition` 独立于常规生态 Tick：`WorldGen.StartHardmode` 在 `WorldGen.cs:26084` 执行世界过渡、保护与重同步。`WorldProgressionAndUnlocks` 以 `BestiaryUnlocksTracker.cs:5-56` 的保存、加载、校验、重置和加入同步为证据，不能降格为纯 UI 图鉴。三者在 NLTX 均不能标为 `confirmed`。

### 不提升为一级子系统的对象

| 候选 | 裁决 |
| --- | --- |
| 单个 AI style、消息号、Hook、TileEntity、Pylon | 分别是策略、Adapter、扩展点、存储居民或旅行端点，不独立拥有完整写集和生命周期。 |
| `GolfState`、SceneMetrics、UI、音频、粒子 | 客户端表现/工具或派生缓存；必须单向消费已提交事实。 |
| Achievement/Social 平台接口 | 平台投影与外部适配；能影响权威资格的发现事实归 `WorldProgressionAndUnlocks`。 |
| BCrypt、NAT UPnP、nativefiledialog | 外部依赖或互操作实现，排除出游戏子系统。 |

### 反向审查后保留为机制或投影的候选

`AmbienceServer` 虽有随机生成和网络调用，但只产出天空/背景表现对象，归 `ClientPresentationAndTools`；`MapUpdateQueue`、`WorldMap` 和 `ModMapLayer` 是客户端探索投影；`ChatCommandProcessor` 是消息/命令适配器；`AchievementManager` 的平台进度不构成权威解锁根；`GolfState` 是局部玩家/镜头状态；`SceneMetrics`/`BiomeScene` 是派生查询缓存。它们均未同时证明独立权威写集和跨域提交边界，故不新增一级子系统。

## 旧结论修订

本报告替代 `docs/Version4权威游戏模拟子系统审查报告.md` 对“仅权威模拟责任面”的有限覆盖结论：补入客户端表现/工具链与运行时适配层，并在反向审查后新增 `LiquidSimulation`、`DeathPenaltyAndRevenge`、`LeashedEntitySimulation`、`PersistenceAndRecovery`、`NetworkSessionAndSectionStreaming`、`ContentLifecycleAndRegistration` 六个责任面，令一级子系统/所有者总数从既有 24 项扩展为 **$($systems.Count)** 项，Version4 的全部基线文件均可审计。新增项分别从 Spatial、Combat/Loot、Projectile、ExternalBoundaries、ContentCatalog 中拆出；不是将单类、Hook、消息号或渲染器升格。旧报告的 NLTX 状态不被继承，本轮以当前 `src/` 与 `Test/` 的可见状态重新判定，未运行构建的系统不提升为 `confirmed`。

## 风险与下一步

最大的缺口仍是 `RuntimeComposition`、受控 world storage 写面以及新增的液体、复仇、leashed entity、持久化、网络 session、内容生命周期实现。本轮未修改 `src/`；报告沿用既有 focused verifier 证据：Combat、NPC 和 WorldInteraction 的受影响验证项目此前构建均为 0 warning / 0 error，工件位于 `Build/bin/`，随后分别以 `--no-build --no-restore` 通过。Combat 具有结算 System、权威状态与 focused verifier，故为 `confirmed`；NPC 与 WorldInteraction 的现有验证只覆盖组件字段组成，仍为 `partial`。新增六项均未实现或未有 focused verifier，保持 `missing`。任何后续状态提升仍须复跑对应构建与 focused verifier。
"@
# PowerShell interprets backtick-letter sequences in expandable here-strings; normalize
# the few control characters that can result while preserving readable plain-text paths.
$report = ($report.Replace([char]8, 'b').Replace([char]12, 'f').Replace('`', '')) -replace [string][char]27, ''
[IO.File]::WriteAllText($reportPath, $report, [Text.UTF8Encoding]::new($false))

$appendixPath = Join-Path $docs 'Version4与完整源码差异附录-2026-09-05.md'
$appendix = @"
# Version4 与完整源码差异附录（2026-09-05）

## 边界

本附录仅补证 `D:/TRbackup/Version4` 中已经存在的文件。`D:/TRbackup/无任何删减通过编译` 中任何独有文件均为 `outside-Version4-baseline`，不写入 `Version4源码覆盖.tsv`，不改变 $($baseline.Count) 个文件的覆盖分母。

## 已闭合的删减点

| Version4 文件与位置 | 完整参考位置 | 匹配理由 | 状态 |
| --- | --- | --- | --- |
| `Terraria/Projectile.cs:34073` `AI_061_FishingBobber` 空体；`:34074` `AI_061_FishingBobber_GiveItemToPlayer` 空体 | `Terraria/Projectile.cs:51236` 与 `:51496` | 路径、声明类型、私有成员签名一致；两端均由浮标 dispatch/交付调用点相邻引用。完整实现包含 FishingAttempt、资格、掉落和 Item/NPC 结果链。 | `full-reference-supplemented` |

## 本轮新增候选与源码差异边界

本轮新增的六个一级责任面是对 Version4 自身调用链、状态写集和生命周期的架构发现，不是完整源码独有文件补录：`LiquidSimulation`（`Liquid.cs`、`LiquidBuffer.cs`）、`DeathPenaltyAndRevenge`（`CoinLossRevengeSystem.cs`）、`LeashedEntitySimulation`（`LeashedEntity.cs` 与注册 prototypes）、`PersistenceAndRecovery`（`WorldFile.cs`、`PlayerFileData.cs`、`WorldFileData.cs`）、`NetworkSessionAndSectionStreaming`（`Netplay.cs`、`RemoteClient.cs`、`RemoteServer.cs`、`MessageBuffer.cs`、`NetMessage.cs`、`WorldSections.cs`）和 `ContentLifecycleAndRegistration`（`Terraria.Initializers/*.cs`）。这些文件均已纳入 Version4 主覆盖表并各自只有一个 `primary_owner`；没有发现可按成员签名与调用邻域闭合的新 `full-reference-supplemented` 点。特别是 `LiquidSimulation` 的完整性不能由 tModLoader Tile 文档推导，故不标为源码差异补证。

## 未闭合点

本轮没有将“仅凭文件差异可疑”的项目伪造为 `source-gap`。除了上述已按签名与调用邻域闭合的钓鱼删减点，其他基线文件在没有具体成员级删减证据时保持 `version4-confirmed`，其私有算法不由 API 文档替代。

## 完整源码独有文件

完整源码独有文件的精确清单不属于本报告的覆盖母表。若后续审查发现其为 Version4 文件的删减实现，必须以路径、类型、成员签名和邻近调用链建立对应关系；匹配成功时只把 Version4 已存在的 TSV 行改为 `full-reference-supplemented`，独有文件本身仍记录为 `outside-Version4-baseline`。
"@
$appendix = (($appendix.Replace([char]8, 'b').Replace([char]11, 'v').Replace([char]12, 'f').Replace('`', '')) -replace [string][char]27, '')
[IO.File]::WriteAllText($appendixPath, $appendix, [Text.UTF8Encoding]::new($false))

Write-Output "Generated $($baseline.Count) baseline rows at $tsvPath"
