# Physical deletion enum qualification

The following six removed declarations were reviewed against the complete source
oracle and reclassified as `SharedDefinition` in the physical-deletion ledger:

| Legacy file | Oracle declaration | Evidence |
|---|---|---|
| `Terraria.GameContent.Biomes.CaveHouse/HouseType.cs` | `enum HouseType` with seven named values | Closed enum; no fields, methods, state mutation, persistence, or protocol behavior |
| `Terraria.GameContent.Generation.Dungeon.Features/PillarType.cs` | `enum PillarType` with six named values | Closed enum; consumed as generation input only |
| `Terraria.GameContent.Generation.Dungeon/WindowType.cs` | `enum WindowType` with three named values | Closed enum; consumed as generation input only |
| `Terraria.GameContent.Generation.Dungeon.Features/DungeonDropTrapType.cs` | `enum DungeonDropTrapType` with four named values | Closed enum; consumed as generation input only |
| `Terraria.GameContent.Generation.Dungeon/ProgressionStageCheck.cs` | `enum ProgressionStageCheck` with three named values | Closed enum; comparison policy value only |
| `Terraria.Physics/BallState.cs` | `enum BallState` with three named values | Closed enum; result-state value only |

The complete oracle files contain only enum declarations. None defines an
authoritative component, snapshot, command, commit, persistence field, or
protocol frame. This is a classification correction, not evidence that the
corresponding dungeon/biome generation behavior has been migrated. The
generation owners and their behavior remain deferred elsewhere in the ledger.

`Terraria.GameContent/PotionOfReturnGateHelper.cs` was separately qualified as
`ClientOnly`. Its complete oracle implementation only computes visual gate
frames and emits `Lighting`, `Dust`, and `DrawData` effects. It does not mutate
tiles, NPCs, players, world progression, persistence, or network state.

`Terraria.GameContent/TreePaintingSettings.cs` is also `ClientOnly`: its
complete oracle stores shader tuning values and exposes only `ApplyShader`,
which calls the client `Effect` API and paint-id shader lookup. It has no tile,
world, player, persistence, or network mutation path.

`Terraria.GameContent.Generation.Dungeon/DungeonRoomSearchSettings.cs` is a
`SharedDefinition` input DTO: the complete oracle contains only five search
configuration fields and no executable member. The room-search algorithm,
tile scanning, and dungeon mutation remain `ServerRelevant` and deferred.

Two additional value-only generation/physics records are `SharedDefinition`:
`Terraria.WorldBuilding/GenShapeActionPair.cs` stores a shape/action pair, and
`Terraria.Physics/BallStepResult.cs` exposes pure factories for a `BallState`
result. Neither owns world state, mutation, persistence, or protocol behavior;
the consuming generation/physics algorithms remain separate responsibilities.

`Terraria.GameContent/CraftingEffects.cs` is also `ClientOnly`: the complete
oracle owns local craft glow state, sound, popup text, particles, and rarity
visual styling. It does not grant items or mutate authoritative inventory; the
craft result itself belongs to the separate item-use/crafting authority path.

`Terraria.GameContent/WellFedHelper.cs` remains `ServerRelevant/deferred`, not a
shared definition. The complete oracle owns three rank-specific counters,
rank-prioritized `Eat` accumulation, one-counter-per-update decay, `Rank`, and
`Clear`. The current ECS `BuffEntry` model has no equivalent three-counter state,
food-rank accumulation contract, persistence field, or protocol projection, so
the helper cannot be claimed as replaced.
