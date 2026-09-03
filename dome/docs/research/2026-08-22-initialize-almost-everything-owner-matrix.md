# Initialize_AlmostEverything Owner Matrix

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3732-3859`.
Source SHA-256: `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`.

This is a source-order accounting matrix for the Version4 initializer. It is not an
implementation plan for a replacement aggregate initializer. `covered` means that a
narrow child card already owns a proven slice; it does not promote M-001 to accepted.

| # | Line | Source call/family | Observable side effect / state | Candidate owner | Status | Existing coverage or blocker |
|---:|---:|---|---|---|---|---|
| 1 | 3739 | `CreativePowerManager.Initialize` | Registers creative-power content and client behavior | Client/content | excluded | No server ECS state proven |
| 2 | 3740 | `LocalFavoriteData.Load` | Reads local user favorites | Client | excluded | Local profile I/O |
| 3 | 3741 | `CloudFavoritesData.Load` | Reads cloud user favorites | Client | excluded | Cloud/profile I/O |
| 4 | 3742 | `Initialize_Entities` | Allocates legacy entity arrays/identity containers | Simulation | accepted | M-002 narrow ownership boundary; legacy arrays deferred |
| 5 | 3743 | `FindAnnouncementBoxStatus` | Reads announcement/UI state | Client | excluded | Presentation state |
| 6 | 3744 | `CustomCurrencyManager.Initialize` | Registers currency definitions | Client/content | excluded | No server transaction contract |
| 7 | 3745 | `WingStatsInitializer.Load` | Loads wing movement/content metadata | Client/content | excluded | No server movement owner proven |
| 8 | 3746 | `TileObjectData.Initialize` | Registers tile-object footprints | Simulation/WorldObjects | accepted | Tile/object child cards; full table deferred |
| 9 | 3747 | `Animation.Initialize` | Initializes animation/content tables | Client/content | excluded | Client animation |
| 10 | 3748 | `Chest.Initialize` | Initializes chest/object static state | Simulation/WorldObjects | accepted | Chest authority child cards; full static table deferred |
| 11 | 3749 | `Wiring.Initialize` | Initializes wiring static state | Simulation/WorldObjects | accepted | Wiring child cards; complete legacy wiring deferred |
| 12 | 3750 | `Framing.Initialize` | Initializes tile framing tables | Simulation/WorldObjects | accepted | Tile framing child cards; complete table deferred |
| 13 | 3751 | `ItemRarity.Initialize` | Registers item rarity/content metadata | Client/content | excluded | No server behavior contract |
| 14 | 3752 | `TileEntity.InitializeAll` | Registers tile-entity types | Simulation/WorldObjects | accepted | `TileEntityDefinitionRegistry` preserves source IDs `0..10`; unknown type `11` rejected; payload/update/persistence remain deferred |
| 15 | 3753 | `Projectile.InitializeStaticThings` | Registers projectile static tables | Simulation | accepted | M-001 projectile-definition child; full defaults deferred |
| 16 | 3754 | `TorchID.Initialize` | Registers torch IDs/content | Simulation/WorldObjects | accepted | `TorchDefinitionRegistry` preserves 24 IDs, Dust mapping and biome flags; color/light providers remain deferred |
| 17 | 3755 | `LeashedEntity.Registry.RegisterAll` | Registers leash entity handlers | Simulation/entities | deferred | Ownership and lifecycle contract unavailable |
| 18 | 3756 | `NPCInteractions.Initialize` | Registers NPC interaction rules | Server/Simulation | deferred | Supported interaction family not isolated |
| 19 | 3757 | `InitializeItemAnimations` | Initializes item animation state | Client/content | excluded | Client presentation |
| 20 | 3758 | `new BestiaryDatabase` | Allocates bestiary database | Client/content | excluded | Client/content catalog |
| 21 | 3761 | `BestiaryDB = bestiaryDatabase` | Publishes bestiary database global | Client/content | excluded | Client/content catalog |
| 22 | 3762 | `ContentSamples.RebuildBestiarySortingIDs...` | Rebuilds content sorting IDs | Client/content | excluded | Client/content catalog |
| 23 | 3763 | `BestiaryTracker = new ...` | Allocates client unlock tracker | Client/content | excluded | Client progression UI |
| 24 | 3764-3766 | `ItemDropDatabase.Populate` / `ItemDropsDB = ...` | Builds item-drop content database | Simulation/content | accepted | Item-drop child cards; full content database deferred |
| 25 | 3767 | `bestiaryDatabase.Merge(ItemDropsDB)` | Merges content databases | Client/content | excluded | Presentation/content aggregation |
| 26 | 3769-3771 | `FishDropRuleList` population | Builds fish-drop content rules | Simulation/content | deferred | No complete source-backed server owner |
| 27 | 3772 | `PylonSystem = new ...` | Allocates teleport-pylon system | Simulation/WorldObjects | deferred | Teleport authority not modeled |
| 28 | 3773 | `ItemDropSolver = new ...` | Allocates drop resolver | Simulation/content | accepted | Item-drop child cards; complete resolver deferred |
| 29 | 3774 | `ShopHelper = new ...` | Allocates shop helper | Server/Simulation | deferred | Shop inventory/session semantics unresolved |
| 30 | 3775 | `CreativeItemSacrificesCatalog.Instance.Initialize` | Loads creative sacrifice catalog | Client/content | excluded | Client/content catalog |
| 31 | 3776 | `Mount.Initialize` | Registers mount content/state | Client/content | excluded | Client movement/content |
| 32 | 3777 | `Minecart.Initialize` | Registers minecart content/state | Client/content | excluded | Client movement/content |
| 33 | 3778,3781,3784 | `WorldGen.RandomizeBackgrounds(rand)` | Mutates background selection using global random | Client | excluded | Presentation randomness; no server owner |
| 34 | 3787 | `WorldGen.RandomizeCaveBackgrounds` | Mutates cave background selection | Client | excluded | Presentation state |
| 35 | 3788-3789 | `WorldGen.Hooks.Initialize` / `OnWorldLoad += ...` | Installs global world-generation hook | WorldGen/Server | deferred | Hook ordering and lifecycle not modeled |
| 36 | 3790-3791 | `bgAlphaFrontLayer/FarBackLayer` writes | Initializes render alpha arrays | Client | excluded | Rendering state |
| 37 | 3792 | `invBottom = 258` | Initializes inventory UI coordinate | Client | excluded | UI layout |
| 38 | 3793 | `Initialize_TileAndNPCData1` | Loads tile/NPC static definitions | Simulation | accepted | M-001 tile/NPC definition children; full tables deferred |
| 39 | 3794 | `Initialize_TileAndNPCData2` | Loads tile/NPC static definitions | Simulation | accepted | M-001 tile/NPC definition children; full tables deferred |
| 40 | 3795 | `Initialize_Items` | Registers item definitions | Simulation | accepted | M-003 narrow registration boundary; full SetDefaults deferred |
| 41 | 3796-3808 | Projectile default loop | Allocates temporary defaults and writes hostile/hook lookup arrays | Simulation | accepted | Supported projectile definition child; complete type table deferred |
| 42 | 3810 | `ConditionalDialogue.Init` | Registers dialogue/content rules | Client/content | excluded | Client/content catalog |
| 43 | 3811-3812 | `ArmorSetBonuses.Initialize/BuildLookup` | Builds armor set lookup | Simulation/content | deferred | Complete set semantics not source-backed |
| 44 | 3813 | `ItemID.Sets.PostSetupContent` | Finalizes item content sets | Simulation/content | accepted | M-003 child registration; full set table deferred |
| 45 | 3814 | `TileID.Sets.PostSetupContent` | Finalizes tile content sets | Simulation/WorldObjects | accepted | Static tile definition child; full set table deferred |
| 46 | 3815 | `ConditionalDialogue.ItemGroups.PostSetupContent` | Finalizes dialogue item groups | Client/content | excluded | Client/content catalog |
| 47 | 3817 | `ContentSamples.DyeShaderIDs.Initialize` | Registers dye shader IDs | Client/content | excluded | Rendering/content state |
| 48 | 3823 | `ContentSamples.FixItemsAfterRecipesAreAdded` | Repairs/finalizes item content | Simulation/content | deferred | Repair predicates and supported set unresolved |
| 49 | 3824 | `ItemSorting.SetupWhiteLists` | Builds item UI sorting lists | Client/content | excluded | UI sorting |
| 50 | 3825 | `ContentSamples.RebuildItemCreativeSortingIDsAfterRecipesAreSetUp` | Rebuilds UI/content IDs | Client/content | excluded | Client/content catalog |
| 51 | 3826-3829 | `liquid` allocation loop | Allocates legacy liquid array entries | Simulation/World | accepted | Liquid state/propagation child; legacy array parity deferred |
| 52 | 3830-3833 | `liquidBuffer` allocation loop | Allocates legacy liquid buffer entries | Simulation/World | accepted | Liquid state/propagation child; legacy buffer parity deferred |
| 53 | 3834 | `shop[0] = Chest.CreateShop()` | Creates shop inventory | Server/Simulation | deferred | Shop ownership and persistence unresolved |
| 54 | 3835 | `Chest.SetupTravelShop` | Mutates travel-shop state | Server/Simulation | deferred | Travel-shop scheduling/source contract unresolved |
| 55 | 3836-3840 | shop creation/setup loop | Creates and populates shop inventories | Server/Simulation | deferred | Complete item table and session projection unresolved |
| 56 | 3841-3846 | `teamColor` writes | Initializes team UI colors | Client/protocol | excluded | Presentation; protocol projection not proven |
| 57 | 3847 | `Netplay.Initialize` | Initializes legacy network host state | Server | accepted | Server bootstrap/network boundary; cadence deferred |
| 58 | 3848 | `NetworkInitializer.Load` | Loads network adapters | Server/Protocol | accepted | Server bootstrap/protocol boundary |
| 59 | 3849 | `ChatInitializer.Load` | Loads chat/network content | Server/Protocol | deferred | Chat message ownership/projection unresolved |

## Decision

No unresolved row in this source slice currently has both a unique owner and a falsifiable,
source-backed server predicate that is not already covered by a child card. M-001 therefore
remains `planned`; no aggregate `Simulation.InitializeAlmostEverything` is introduced and the
coverage matrix is intentionally unchanged. The next candidate cards are the individually scoped
M-007, M-008, M-009, M-014 and M-024 boundaries.
