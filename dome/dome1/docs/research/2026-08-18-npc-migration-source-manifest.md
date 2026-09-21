# NPC Migration Source Manifest

This is read-only evidence about the historical source. The deleted Version4 file is not copied
into this repository and is not a runtime dependency.

| Field | Value |
| --- | --- |
| Source path | `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs` |
| SHA-256 | `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17` |
| Line count | `79,688` |
| Method declaration count | `354` (access-modifier declaration-line count) |
| `AI_` marker count | `158` |

## Anchors

| Behavior | Line | Evidence |
| --- | ---: | --- |
| `Spawner` type/constructor | 39 / 159 | `public class Spawner` and `public Spawner()` |
| `SetDefaults` | 8,095 / 8,133 | `SetDefaults_ForNetId` and `SetDefaults` |
| Main AI entry | 18,642 | `public void AI()` |
| Death check | 64,571 | `public void checkDead()` |
| Loot | 65,312 | `public void NPCLoot()` |
| Spawn entry | 67,051 | `public static int NewNPC(...)` |

The counts and anchors were measured from the source path above on 2026-08-18. They are evidence
for migration coverage only; no source member is treated as an inheritance or implementation
contract for `Terraria.Dome.Simulation`.
