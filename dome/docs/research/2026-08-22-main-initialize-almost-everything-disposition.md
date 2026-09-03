# Main Initialize_AlmostEverything disposition

Reference: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`.

`Initialize_AlmostEverything` is an orchestration entry point, not a single server behavior. Its
calls must be accounted for by their domain owner before the entry point can be removed or replaced.

The complete source-order accounting is recorded in
`docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`. It covers source lines
3732-3859 and keeps child-card coverage distinct from aggregate M-001 acceptance.

| Source call/family | Current disposition | Owner or evidence |
|---|---|---|
| `Initialize_Entities` | accepted narrowly | M-002 entity ownership boundary; client arrays remain deferred |
| `Initialize_Items` | accepted narrowly | M-003 item definition registration boundary; complete SetDefaults table deferred |
| `Initialize_TileAndNPCData1/2` | partial/deferred | Tile/NPC definition cards cover selected rules, not the full static tables |
| `Projectile.InitializeStaticThings` and projectile defaults | partial/deferred | Projectile definition/ownership cards cover selected contracts, not all type defaults |
| `Chest.Initialize`, `Wiring.Initialize`, `Framing.Initialize`, `TileEntity.InitializeAll` | separate domain cards | Existing chest/wiring/tile framing boundaries; complete static initialization remains open |
| `ItemDropDatabase`, `FishDropsDB`, `ItemDropSolver`, shops and recipes | partial/deferred | Item drop/extractinator slices only; full content databases are not Simulation state |
| `WorldGen.RandomizeBackgrounds`, cave backgrounds and hooks | deferred/client | Presentation and legacy global hook behavior have no server owner in this plan |
| `Liquid` and `LiquidBuffer` array allocation | separate domain | Liquid ECS state and bounded propagation systems own supported liquid behavior |
| `Netplay.Initialize`, `NetworkInitializer`, `ChatInitializer` | Server/Protocol | These are adapter startup concerns, not Simulation initialization |
| Bestiary, Creative, Mount, Minecart, dye/content sorting and UI catalogs | excluded/deferred | Client/content presentation or unsupported data domains |

## Decision

M-001 remains `planned`: no generic `Simulation.InitializeAlmostEverything` method is introduced.
The focused MainBoundary accounting gate passed with exit code 0, but no unresolved family had a
unique source-backed server owner and falsifiable predicate in this card. The matrix is an
accounting artifact and does not claim complete Main startup parity.
