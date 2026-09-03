# ShopHelper 资格边界审计

这是 M-001 `Initialize_AlmostEverything` 中 `ShopHelper` family 的资格审计。它不实现
商店价格或 NPC 个性，也不新增聚合 initializer。

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria.GameContent\ShopHelper.cs`
- SHA-256: `CB73A0D1A9F7921EA5EDCB4EACBD2E338F77A5F1261A2365C1D8AE2FB8FAB2DC`
- Constructor and personality population: lines 43-47
- `GetShoppingSettings`: lines 49-65
- mutable query context: lines 18-26, 58-62
- biome dependencies: lines 28-33
- source construction call: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3774`
- player query call: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs:3393`

## Qualification result

| Predicate | Result | Evidence |
|---|---|---|
| bounded constructor | yes | one `new ShopHelper()` call in the initializer |
| fixed definition table | no | constructor populates a personality database and dangerous biome objects |
| unique server owner | no | query consumes Player, NPC, biome and personality state |
| typed transaction/session contract | no | mutable current player/NPC fields are held on the helper instance |
| deterministic replay contract | no | source does not expose input snapshot, ordering or revision identity |
| persistence contract | no | shop stock, happiness and price calculation state have no snapshot owner |
| protocol/presentation boundary | no | `ShoppingSettings.HappinessReport` is localized presentation output and travel-shop packets are separate concerns |

## Decision

`ShopHelper` remains `unknown/deferred`. Existing item and equipment authorities must not be
used as a substitute for shop/economy ownership, and no generic `ShopHelper` replacement or
`NpcInteractionRegistry` is added.

## Prerequisites

1. A server-owned shop inventory/session model with stable NPC identity and revision.
2. A typed happiness/price query context containing all biome, housing, progression and player
   inputs, with explicit locale/presentation projection.
3. Persistence and replay rules for stock refresh and travel-shop transitions.
4. Forged, stale, duplicate and cross-player shop command rejection plus loopback packet evidence.
