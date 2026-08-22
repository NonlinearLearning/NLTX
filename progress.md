# Project Progress Summary

# 2026-08-22 Entity/event lifecycle gap matrix

Added a source-backed accounting matrix for player/NPC/projectile/item lifecycle, invasion/weather
events, static tables, random starts and client/presentation branches. Existing narrow owners remain
scoped to their accepted predicates; unresolved tables, global random ordering, NPC-driven invasion
spawns and client effects remain explicitly deferred. No production code or default table was added.
Evidence:
`docs/research/2026-08-22-entity-event-lifecycle-gap-matrix.md` and
`Build/diagnostics/main-migration/task-13-entity-event-lifecycle-gap/20260822-100000/`.

Last updated: 2026-08-22

完整历史记录已归档至
`docs/archive/progress-2026-08-22-full.md`。本文件只保留当前上下文所需的摘要，避免每次 AI
任务加载数千行历史记录。

## Current Architecture

- `Terraria.Dome.sln` 是当前构建入口，目标框架为 `net10.0`。
- `Terraria.Dome.Simulation` 承载 Arch ECS、组件、系统、命令和不可变快照。
- `Terraria.Dome.Server` 承载服务器循环、会话、权威状态和复制。
- `Terraria.Dome.Protocol.V1456` 承载 V1456 协议编解码与兼容投影。
- `Terraria.WorldFile.V319` 和 `Terraria.WorldCompatibility` 承载世界文件读取及兼容投影。

## Current Qualification

- MainBoundary、WorldObjects、Persistence、WorldRules、WorldClock、WorldImport、
  PlayerLifecycle、Combat 和 Completion 已有新鲜退出码为 0 的证据。
- PlayerAuthority 的 bootstrap 队列修复已有五次重复运行成功证据；后续变更仍需重新验证。
- 当前约 40% 的主迁移范围已完成资格审查，整体状态仍为 `incomplete`。
- TileEntity 完整行为、若干初始化责任族、完整 NPC/Projectile/Item 行为表和
  WorldGen 物理删除仍未完成或明确延期。

## Recent Boundaries

- Projectile motion and world-item pickup reject non-finite input before mutation.
- NPC target routing and target selection use fail-closed validation for encoded targets,
  identities, lifecycle state, stable ids, and positions.
- Server session outbound flushing uses the bounded writer queue without blocking the
  Simulation tick loop.

## Verification Policy

- Build and test commands run from the repository root with
  `-p:UseSharedCompilation=false`.
- Build outputs, generated files, packages, diagnostics, and other local artifacts belong
  under `Build/` and are excluded from source commits.
- New claims require fresh evidence under `Build/diagnostics/` and a source-backed boundary.

## Open Work

- Continue migration in small responsibility families with explicit source ownership.
- Re-run focused verifiers after each behavioral change and record only the current result here.
- Keep deferred capability families and physical-deletion gates separate from accepted slices.
