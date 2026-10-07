# 162 个网络消息分类注册：主会话验收记录

日期：2026-10-07（Asia/Shanghai）  
主会话：`01a115be-ecff-7bc1-8430-4d43f2c89e41`

## 验收结论

本轮已按 10 个大类并行创建 10 个独立会话和 10 个统一前缀分支；每个子代理都收到对应的详细执行文档，并被要求使用 `/goal` 与 PUA skill 自监督。主会话独立复核了 focused smoke、构建结果、owner/方向矩阵和缺口。

当前达到的是分类登记、唯一 owner 盘点和有条件的安全注册，不是“162 个消息全部已实现并启用”：

- `MessageID.Count = 162`，有效消息编号为 `1..161`。
- 有可信 owner、提交路径和投影证据的消息才启用；缺少可信 owner 的方向保持 disabled/default-deny 或 opaque no-effect。
- 10 个类别全部为 `accepted-partial`，没有任何类别被主会话验收为 `complete`。
- 所有类别都遵守“不扩大测试或构建范围”：不构建 solution，不运行完整 Network Verification Program，不运行 real-client、压力/吞吐或长时测试，不运行其他类别 verifier。
- 每类只执行执行文档规定的约 10% 核心 focused smoke；必要 restore 仅针对实际受影响项目。

## 并行会话与分支

统一分支前缀：`codex/network-message-`

| 类别 | 会话 | 分支 | 主会话结果 |
|---:|---|---|---|
| 01 会话与连接生命周期 | `01a115cc-98bf-7d13-8cf5-6d05d52b7410` | `codex/network-message-01-session` | accepted-partial |
| 02 世界流送 | `01a115cc-9957-7063-9c5c-65dee282fac1` | `codex/network-message-02-world-stream` | accepted-partial |
| 03 玩家状态 | `01a115cc-9826-77f3-99e0-30a2ae87105a` | `codex/network-message-03-player-state` | accepted-partial |
| 04 实体生命周期 | `01a115cc-9a6c-7d21-8cd6-48885907b74e` | `codex/network-message-04-entity-lifecycle` | accepted-partial |
| 05 世界修改 | `01a115cc-99be-7681-a578-fc3f56cbf39a` | `codex/network-message-05-world-modification` | accepted-partial |
| 06 库存与商业 | `01a115cc-9a37-72a0-8742-94c5ec85eab4` | `codex/network-message-06-inventory-commerce` | accepted-partial |
| 07 战斗结果 | `01a115cc-9ab0-73e3-9377-a74e2321b8a1` | `codex/network-message-07-combat-results` | accepted-partial |
| 08 NPC 与世界事件 | `01a115cc-9bed-7a80-b404-ce7f2ec1a8c1` | `codex/network-message-08-npc-world-events` | accepted-partial |
| 09 UI 与社交 | `01a115cc-a341-71d3-8c81-26d25021fcd1` | `codex/network-message-09-ui-social` | accepted-partial |
| 10 NetModules/legacy | `01a115cd-0052-78d2-9755-34c503c88479` | `codex/network-message-10-netmodules-legacy` | accepted-partial |

没有合并分支、没有提交主分支、没有清理或 reset 共享工作树。

## 独立验收摘要

| 类别 | 主会话复核结果 | 未达到 complete 的原因 |
|---:|---|---|
| 01 | 会话生命周期、方向门、secret bounds、stale host authorization smoke 通过；verifier 最终 0 warning / 0 error。 | ID 3/129 的计划方向与实际生成 wire profile 有冲突；ID 93 仍 disabled；共享 `PacketGateway` seam 需要最终组合核对。 |
| 02 | movement/world behavior/frame-repair TCP 三个计划内 smoke 通过；verifier 0/0，NetworkServer 0/23。 | caller 尚未提供真实 world-generation provider；158 缺独立 end-to-end client fixture。 |
| 03 | player lifecycle、players extended、result directions 三项 smoke 通过；最终 0/0。 | 生命、Buff、Hurt/Death 等 producer/owner 不完整；ID 5、125、147 仍有可信来源或完整前置校验缺口；ID 36 正确保持 fail-closed。 |
| 04 | 物品 owner verifier、`--projectile-gateway`、`--projectile-runtime-owner` 均 exit 0；物品、投射物构建 0/0，NetworkServer 0/0。 | 17 项中仅部分 Item/Projectile owner 已接通；多项仍 codec-only 或无可信 lifecycle owner；27/29 生产入口保持禁用。 |
| 05 | `--tile-placement` exit 0，覆盖 tile placement、TileEntity default-deny、Ping 目标范围；verifier 0/0，NetworkServer 0/17。 | 17 仅有 fixture committer、没有生产 Tile committer；TileEntity 合法交互链仍未实现；其余世界修改消息保持 default-deny。 |
| 06 | 主会话独立复跑库存商业 verifier exit 0；输出为 `PASS inventory-commerce catalog, chest owner, and NPC/quest/shop isolation`；verifier 0/0，NetworkServer 0/17。 | 胸箱仅有粗粒度 5×3 基线；没有原子库存转移、Buff/任务/商店 owner；第二组 NPC/Buff/quest/shop 用例因无生产 owner 未运行。 |
| 07 | combat direction quarantine 和 unique owner matrix smoke 通过；verifier 最终 0/0。 | 没有 authoritative combat tick、DamageNPC owner 或完整结果 producer；多项结果仅有 S2C binding。 |
| 08 | `--npc-world-events` 与 `--social-npc-effects` 均 exit 0；最终 verifier 0/0。 | 40/141/144 等请求校验仍有边界缺口；61/111/113 无可信提交 owner；多项仅有 codec/catalog 或无 producer。 |
| 09 | social behavior smoke 通过；Network/verifier 构建 0/0。 | 65/95/96 依赖其他类别的 Player/NPC/Portal owner；82/109/154 需要最终共享组合确认。 |
| 10 | 主会话独立复跑 `--netmodules-legacy` exit 0；Packet 82 central registry、duplicate-owner guard、Steam mapping、opaque bounds 和 disabled catalog 均通过；verifier 0/0，NetworkServer 0/17。 | 42/66/68 等仍有跨类别 registration 冲突；旧 fixture 仍存在 direct Packet 82 registration。 |

## 主会话复跑的命令边界

以下是主会话实际独立复跑的计划内 focused smoke，全部 exit `0`：

```text
CAT-04:
dotnet Build/bin/Terraria.Items.NetworkOwner.Verification/Debug/net10.0/Terraria.Items.NetworkOwner.Verification.dll
dotnet Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll --projectile-gateway
dotnet Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll --projectile-runtime-owner

CAT-05:
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --tile-placement

CAT-06:
dotnet run --project Test/NSSLC.Infrastructure.Network.InventoryCommerce.Verification/NSSLC.Infrastructure.Network.InventoryCommerce.Verification.csproj --no-build --no-restore --verbosity minimal

CAT-08:
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --npc-world-events
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --social-npc-effects

CAT-10:
dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -- --netmodules-legacy
```

CAT-01/02/03/07/09 的 focused 结果由对应类别证据记录提供；主会话没有为了复核而新增测试矩阵。上述命令不是全量回归，也不替代各类别未覆盖路径。

## 证据位置

本轮证据仍位于各独立 worktree；由于用户尚未授权合并，主会话不复制或改写其分支源码：

- CAT-01：`C:\\Users\\shan\\.codex\\worktrees\\85f8\\NLTX\\docs\\plans\\network-message-registration\\evidence\\01-session-lifecycle.md`
- CAT-02：`C:\\Users\\shan\\.codex\\worktrees\\623c\\NLTX\\Build\\diagnostics\\network-message-02-world-streaming\\evidence.md`
- CAT-03：`C:\\Users\\shan\\.codex\\worktrees\\5602\\NLTX\\Build\\diagnostics\\network-message-registration\\03-player-state.md`
- CAT-04：`C:\\Users\\shan\\.codex\\worktrees\\7cb1\\NLTX\\Build\\diagnostics\\NetworkMessageRegistration\\CAT04\\evidence.md`
- CAT-05：`C:\\Users\\shan\\.codex\\worktrees\\7ff0\\NLTX\\docs\\system-decomposition\\reports\\2026-10-07-network-message-cat-05.md`
- CAT-06：`C:\\Users\\shan\\.codex\\worktrees\\0f35\\NLTX\\Build\\diagnostics\\network-message-06-inventory-commerce.md`
- CAT-07：`C:\\Users\\shan\\.codex\\worktrees\\9e7d\\NLTX\\docs\\plans\\network-message-registration\\07-combat-results-evidence.md`
- CAT-08：`C:\\Users\\shan\\.codex\\worktrees\\da9a\\NLTX\\Build\\diagnostics\\network-message-registration\\08-npc-world-events.md`
- CAT-09：`C:\\Users\\shan\\.codex\\worktrees\\e554\\NLTX\\docs\\plans\\network-message-registration\\09-ui-social-evidence.md`
- CAT-10：`C:\\Users\\shan\\.codex\\worktrees\\1e78\\NLTX\\Build\\diagnostics\\NetworkMessage10NetModulesLegacy-20261007.md`

## 后续合并门槛

在用户明确要求合并前，不合并这些分支。未来若进入合并阶段，必须先逐个处理：

1. `PacketGateway.Registrations` 的全局唯一 message/owner key，尤其是 42、66、67、68、82、109、154 等共享或重叠项。
2. `NetworkServer/Program.cs`、`ServerProtocolProfile.cs`、验证 `Program.cs` 等共享组合入口的窄 hunk 合并。
3. 所有“格式存在但无可信 owner”的方向继续保持 disabled；不能因为 codec/catalog 完整就开启 ingress。
4. 合并后的验证仍只能使用各类别执行文档规定的 focused smoke；本记录的 10% 上限不会因为合并而放宽。

