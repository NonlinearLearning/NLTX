# Main ECS migration batch acceptance review

本 review 对 2026-08-23 批次的四张 M-001 family qualification cards 做独立复核。它不是
整体迁移完成声明，也不把 qualification evidence 误写成 runtime parity。

## Reviewed cards

| Card | Outcome | Runtime change |
|---|---|---|
| ArmorSetBonuses | `unknown/deferred` | none |
| ShopHelper | `unknown/deferred` | none |
| TeleportPylonsSystem | `unknown/deferred` | none |
| ContentSamples item repair | `unknown/deferred` | none |

每张卡都固定了 source path、SHA-256、line scope、缺少的 authority/persistence/replay 条件，
并记录了禁止新增的伪 registry 或 aggregate initializer。

## Gate review

- MainBoundary: `771` source files, `0` violations, exit `0`。
- Reduced Items verifier: exit `0`。它只证明现有 item/equipment 窄 authority，不证明四个
  deferred family 已完成。
- 每张卡的 scoped `git diff --check`: exit `0`。
- 没有 executable source change，因此没有扩大到 full regression、完整 client suite 或
  root Release build。

## Remaining work

M-001 仍为 planned；M-014/M-024、B-007、完整 entity/event lifecycle、NPC/projectile/item
tables、random starts 和 client/presentation branches 仍未完成。下一张 implementation card
必须先有 source-backed typed/replayable owner、可否证 RED/GREEN 和 persistence/projection
边界；否则继续保持显式 deferred。
