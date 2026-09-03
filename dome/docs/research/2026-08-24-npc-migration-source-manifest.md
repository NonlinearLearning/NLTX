# NPC migration source manifest

Read-only source evidence for the 2026-08-24 NPC execution plan. The historical
file is not copied into this repository and is not a runtime dependency.

| Field | Value |
| --- | --- |
| Source path | `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs` |
| SHA-256 | `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17` |
| Line count | `79,688` |
| Method declaration count | `354` |
| `AI_` marker count | `158` |

| Anchor | Line | Current owner/evidence |
| --- | ---: | --- |
| `Spawner` | 39 / 159 | `NpcSpawnEligibilitySystem` and `NpcSpawnCommitSystem` |
| `SetDefaults` | 8,095 / 8,133 | `NpcDefinitionRegistry` |
| `AI()` | 18,642 | typed behavior registry and family systems |
| `checkDead()` | 64,571 | lifecycle/death/loot command chain |
| `NPCLoot()` | 65,312 | deterministic `NpcLootSystem` |
| `NewNPC(...)` | 67,051 | bounded spawn command and commit |

The source counts and anchors are inventory evidence, not a claim of complete
legacy parity. Unsupported families remain explicitly partial, deferred, or
excluded in the behavior matrix.
