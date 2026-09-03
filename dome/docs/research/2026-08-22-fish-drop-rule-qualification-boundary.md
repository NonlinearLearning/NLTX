# FishDropRuleList Qualification Boundary

Source oracles:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria.GameContent.FishDropRules\FishDropRuleList.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria.GameContent.FishDropRules\GameContentFishDropPopulator.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3769-3771`

`Main.Initialize_AlmostEverything` constructs `FishDropRuleList`, runs
`GameContentFishDropPopulator.Populate`, and publishes it as `FishDropsDB`. The populator contains
multiple region/height/biome/lava/honey/crate/junk/quest and progression conditions, plus stopper
ordering and weighted rule evaluation. It is not a fixed identity table.

The current Simulation has item-drop and NPC loot owners, but no fishing context, fishing command,
angler quest authority, fish-rule evaluator, or persistence/replay contract. Copying the source
rule list into either item drops or NPC loot would conflate domains and lose condition semantics.

## Decision

Keep the fish-drop initializer family `unknown/deferred`. A future card must first define a named
fishing authority and typed context (water/biome/height/lava/honey/crate/progression/quest), then
port rules as deterministic data with explicit ordering and replay state. No aggregate initializer
or generic fish-drop catalog is added.
