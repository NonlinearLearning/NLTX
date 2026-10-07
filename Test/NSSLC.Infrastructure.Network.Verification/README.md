# Network verification

Independent verification of `NSSLC.Infrastructure.Network`, using a console verifier
consistent with the repository's existing verification projects.

## 行为分类与验收边界

测试先按玩家行为及其可观察结果分类，再映射到报文、处理器和验证程序。
报文编号与测试程序名称用于定位实现，不决定一个游戏行为是否完整。
现有验证组的通过数量不能作为完整玩法的通过数量。

以下分类记录当前 world-data host 的覆盖范围；不代表仓库其他领域项目的能力清单。
“部分覆盖”表示已有子行为断言；“未覆盖”表示当前没有对应的完整行为验收。
未实现的预期结果仍属于验收条件，不能因为宿主是 headless 而从游戏行为中删去。

| 行为类别 | 玩家可观察的完整结果 | 当前验证入口与覆盖事实 | 当前缺口 |
| --- | --- | --- | --- |
| 连接与会话 | 连接、身份绑定、入场、断开及重连按约定推进 | Gateway、PlayerSlot、Reconnect、Steam 专项覆盖协议与会话契约 | 这些结果不证明挖掘、放置或射击功能完整 |
| 移动与世界可见性 | 移动到新区块后收到地形，已加载区块不重复发送 | WorldMovementSectionVerification、ProbeMovementSections 覆盖区段传输及去重 | 权威移动、碰撞和其他世界实体的可见性未覆盖 |
| 挖掘与地形变化 | 部分击打保留方块，成功破坏更新地形，并触发应有掉落 | MiningPacketVerification、LiveMiningVerification 覆盖击打标志、地块变化、重复请求、区段缓存和连接 | 掉落计算、世界物品创建与物品同步未覆盖；完整挖掘未通过验收 |
| 世界物品与拾取 | 掉落物类型、数量和位置正确；可被拾取，拾取后从世界移除 | 当前宿主验证未覆盖 | 物品生命周期、所有权、拾取条件、重复拾取及并发争抢未覆盖 |
| 背包与资源转移 | 拾取增加正确堆叠，放置和使用消耗正确资源，容量限制得到遵守 | PlayerAdmissionPacketVerification 覆盖上传格式和数值边界 | 权威背包状态、堆叠、容量、溢出及资源转移未覆盖 |
| 放置与资源消耗 | 合法放置产生正确地块并扣除物品，失败放置不扣物品 | TilePlacementPacketVerification、LiveTilePlacementVerification 覆盖普通单格 overlay、纠正和区段回读 | 物品消耗、支撑规则及完整家具行为未覆盖；完整放置未通过验收 |
| 物品使用与战斗 | 使用武器产生射弹，消耗对应资源；碰撞、伤害和结束结果正确 | SteamProjectileVerification、射击 TCP 探针覆盖格式、绑定、报文突发和连接 | 宿主射弹创建、模拟、伤害、弹药和法力消耗未覆盖；完整射击未通过验收 |
| 多人一致性 | 其他玩家看见相同地形和物品结果，共享资源只被发放一次 | RoutingVerification 覆盖传输目标、代次和发送隔离 | 游戏行为的跨客户端一致性和共享资源竞争未覆盖 |
| 生命周期与持久化 | 按行为约定保存、重载和重连后，世界与玩家状态保持正确 | 当前宿主不写输入世界文件；重连验证仅覆盖连接生命周期 | 已修改地形、掉落物和背包的保存恢复未覆盖 |

## 每个行为用例必须描述的内容

1. 行为类别、执行者和前置状态，例如玩家背包数量、地块类型和工具条件。
2. 玩家触发的动作及其完成条件，例如“成功挖碎”，而不仅是收到 packet 17。
3. 所有应有状态变化、产生的资源和受影响对象，例如地块、世界物品和背包。
4. 对执行者及其他相关客户端可观察的结果，并按行为约定检查后续生命周期。
5. 拒绝、未完成、重复、并发和中途断开时的结果，包括不能发生的副作用。

完整挖掘至少拆成“部分击打”“成功破坏与掉落”“掉落同步”“拾取与背包转移”
这些子行为，并组合验证一次完整操作。成功破坏后没有生成应有掉落物时，
完整挖掘用例必须失败，即使 packet 20、区段回读和 Ping 都已通过。

每项结果分别报告“协议契约”“已有行为的部分覆盖”“完整游戏行为验收”。
只验证其中一段时，结果名称和报告应注明该子行为，不能把未覆盖的行为算作通过。

## 网络契约验证

The implementation is verified against the network design's observable contracts:

- Framing: lengths 3/65535, every split boundary, multiple frames in one callback,
  invalid length, body truncation and trailing data, clean EOF and partial EOF.
- Ownership: callback input may be reused immediately; decoded values retain their data.
- Sending: serialized concurrent writes, partial and synchronous sent callbacks,
  accepted versus locally sent, failed submission, disconnect outcome certainty,
  cancellation before and after submission, item/byte capacity and timeout cleanup.
- Admission: phase/direction allowlists, opaque and module rejection before decoding,
  authoritative sender binding, application-confirmed transitions and old epoch events.
- Routing: current target epochs, sender inclusion/exclusion, selected targets,
  section subscriptions and failure isolation for a slow recipient.
- Cache: input/output ownership, profile/world/revision keys, TTL, bounded eviction,
  explicit invalidation and rejection of private or sensitive entries.
- Reconnect: finite attempts, jitter caps, total deadline, terminal failures,
  lifecycle cancellation, fresh epochs and no packet replay.
- Integration: generated packet bodies use one frame header, correct directional
  codecs, a real NetCoreServer loopback connection and controlled server shutdown.

Build only this project from the repository root, then run with `--no-build --no-restore`.
All build outputs inherit `Directory.Build.props` and belong under `Build/bin/`.
Command results and limitations are recorded in the implementation verification report.

The headless real client can also be run against the repository's world-data host with
`Invoke-NetworkServerRealClientProbe.ps1`. The world-player case reads an actual decoded world,
sends 207 player-control frames over a 600-tile horizontal route, requests the three crossed
sections, breaks a nearby ordinary tile with packet 17, and verifies the packet 20 update through
the original client parser. It requests that section again to prove the cached section also shows
the updated tile. The host reports its in-memory tile change separately from the client
acknowledgement. No UI or graphics device is created, and the input `.wld` file is not modified.

This remains a network and protocol probe: it does not run authoritative player physics, collision,
tile drops, protection checks, world persistence, NPC AI, or projectile AI. The host enables packet
17 only for this constrained test behavior; other gameplay packets remain denied.
