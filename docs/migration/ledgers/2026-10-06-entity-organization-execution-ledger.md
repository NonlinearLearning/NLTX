# Entity 组织迁移执行 ledger

日期：2026-10-07。关联：[代码设计](../../architecture/2026-10-06-entity-organization-design.md)、
[执行方案](../../architecture/execution/2026-10-06-entity-organization-execution.md)。

本 ledger 记录已执行的代码批次、单写边界和可复核证据。`进行中` 仅表示已有代码进入目标
路径，不代表该批满足全部验收条件。

本轮文档审计仅读取源文件、调用关系和既有 evidence JSON；没有构建、restore、编译型运行、
提交或修改生产源码。证据分为静态 source/caller、domain fixture、Simulation fixture host 与
真实 NetworkServer host；unknown、并行 blocker、旧 DLL 和无成功 fresh build 的结果独立标识。

最终正式文档单写职责：Audit/documentation 负责 design、ledger 和统一身份 PRD；Spatial 负责 execution、docs/README.md 与 docs/document-manifest.tsv；主验收会话负责记录最终 checker 与验收 receipt。职责映射只界定本轮文档维护边界，不用于推断历史作者归属。

## 批次状态

| 批次 | 状态 | 已交付 | 未完成门禁 |
| --- | --- | --- | --- |
| B0 基线和冲突同步 | 执行门禁通过；主验收 receipt 状态为 `ACCEPTED_WITH_NAMED_LIMITS`。owner-attribution artifact 中早先的 `PENDING_REHASH_AFTER_ARTIFACT_AND_PROGRESS_WRITE` 已由 receipt 的 `FINAL_RECHECK_STABLE; PROVIDER_DRIFT_RECORDED` 后续复核结果更新 | 原始 523 项 input-copy/hash 保留；9 个 Version4 指纹匹配；fresh Simulation DLL 621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE 按 exact B0 arguments 完成 3600 tick run | exact 237 项中 11 项 Mother Slime / NetId 16 差异保留为具名行为限制；不宣称逐 tick 完整因果或旧算法等价。0B052D…981E56 的 PDB/source map 仅证明 artifact-time 对应；receipt 记录 9 项独立 NPC AI 后续 source drift 超出本迁移运行验收，不表示当前源码等于编译输入，也不使该 DLL 的运行证据失效 |
| B1 根身份和作用域 | 已通过（当前受支持路径） | EntityUuid/runtime/handle、identity registry、NPC/Player scoped references；跨 runtime、generation exhaustion、槽复用、Reset、正常复活/重建和 world switch 的旧引用规则有当前证据 | 分类 slot、NpcInstanceId、Item RuntimeEntityId、TileEntity ID/anchor 与 protocol identity 保持边界 projection，不成为新根 |
| B2 组件关联与访问协议 | 已通过（实现、版本化访问、受控借用与测量范围） | 精确类型组件表、稳定 cell、反向成员、struct Edit/Replace、版本化快照提交、组合查询/复用缓冲区、owner-thread/阶段/借用冲突检查；class projection 引用拒绝、reentry/exception、Dispose preflight、过期 revision 与查询候选失效均已验证。1000 Projectile / 200 NPC / 255 Player 容量 microbenchmark 已记录 | microbenchmark 显示组件表读写/扫描慢于直接数组；C# 不能阻断所有 class alias 或不安全 ref 逃逸，callback 依赖受信任 owner 约定与审查，不宣称安全隔离 |
| B3 NPC 纵向切片 | 已通过（当前受支持空间/cleanup 范围） | 38 场景 matrix 的 0/1/2 Player spatial probe 覆盖 NetId 1,2,3,4,16,22,37,488；NPC Reset borrow-preflight、旧引用失效与两次正常 world switch 有证据 | 0B052D…981E56 PDB/CodeView map 只确认 artifact-time DLL/PDB 与 17 项 source Documents 对应；主验收 receipt 记录当前树有 9 项独立 NPC AI 后续 source hash drift，超出本迁移运行验收范围，不表示当前源码等于编译输入，也不使该 DLL 的运行证据失效。Eye profile 只覆盖开场计数、失去目标退出和首次 dash，不宣称完整 AI parity；aiStyle 28 不支持；11/237 Mother Slime 是 B0 行为限制 |
| B4 Player | 已通过（当前矩阵内生命周期与 switch 范围） | 0/1/2 Player spatial/cleanup、普通 respawn 保根、destroy/recreate 换根、两次 WorldFile switch 中旧引用/owner 拒绝与新引用解析，DLL 621C3573…E2FDE | 普通 respawn 不销毁实例；失败 candidate rollback 由 B8 的真实 WorldFile 与 Simulation actual-owner 证据覆盖 |
| B5 Projectile | 已通过（已声明 type 1 ordinary-arrow 与七个 domain mode 范围） | ProjectileEntityState 已删除；删除旧 identity 后 fresh DLL 5C631F60797CD86E7B23A88CD8B04CF706B3DA2D993EDB75C2ADA65D7CD487B3 的 default、identity-index、lifecycle、hydration、network、tick-coordinator、damage-candidates 七 mode 全通过；B8 38 场景 matrix 与 621C…E2FDE exact run 覆盖真实 ordinary-arrow host path | Simulation 仅支持 Projectile type 1；NetworkServer packet-27/29 disabled。domain modes 不实例化 private Simulation adapter，不外推到其他 Projectile 类型或权威网络 gameplay |
| B6 Item/库存/掉落 | 已通过（17 场景 owner matrix 与当前 Simulation host 子范围） | slot 49 corrected owner fixture 的 17 场景矩阵、spawn cleanup、containment/scope、revision、effect rollback/retry、expiry、partial/full pickup；38 场景 host 含 expiry/physics/inventory/combat drop/pickup | Simulation catalog 无 coin definitions；Version4 GetItem_Fill* 为 stub，不宣称全量 coin/pickup parity |
| B7 TileEntity/Leashed | 已通过（TileEntity 支持场景；Leashed 按未接线边界关闭） | disposed-store/reuse 17 类访问拒绝、captured SessionDisposal、真实 2-root TileEntity save/reload/switch 与 8 项 reload assertions；38 场景 matrix 包含 TileEntity remove 与 world switch | 当前没有 Leashed production registration/remove/section caller；Version4 TileEntity Place/NetPlaceEntityAttempt 为 stub，TileEntity.Remove 有索引与 update-list 清理 |
| B8 两个宿主和边界收口 | 已通过（38 场景矩阵、真实 WorldFile rollback、Simulation actual-owner rollback/retry） | Simulation matrix DLL 621C…E2FDE；真实 WorldFile rollback DLL 5BA221…E15B4；Simulation rollback、zero-tick、two-switch narrow regressions DLL 0B052D…981E56，0 warnings/errors、runs exit 0；PDB map 只绑定该 artifact-time DLL/PDB 与 17 项 source Documents | 无 B8 runtime gate 待验。主验收 receipt 记录 9 项独立 NPC AI 后续 source drift 在运行验收范围外；不使 0B052D… 运行证据失效，也不证明当前源码等于编译输入。NetworkServer packet-27/29、非权威 Simulation 与未列内容仍为支持限制 |
| B9 兼容退出及最终审计 | 三项生产声明删除、生产范围 source audit 与对应 build/mode 门禁已通过；主验收 receipt 记录 `ACCEPTED_WITH_NAMED_LIMITS` | ProjectileEntityState.cs、EntityIdentityState.cs、EntityId.cs 均删除；literal/config/alias audit 范围为生产源码与 `Test`，排除只读 `src/NSSLC.Infrastructure/分类参考/`；旧 identity 删除后的七 mode DLL 5C631F…487B3 全通过，Simulation DLL 0B052D…981E56 为 0/0 | receipt 在 15:37 记录过 checker PASS（59 links / 16 line links / 22 anchors / 15 fingerprints / 393 manifest paths）；本轮新增 B9 范围说明后当前文档待主验收重跑。主 receipt 所引 final summary 未序列化参考目录排除项；同名 `EntityIdentityState` 仅存在于被排除的只读参考目录。合法 WorldSession.Calendar.EntityId 是不同领域类型，不属于旧声明 |

### B5 Projectile 根生命周期切片（前序阶段记录；当前状态见 2026-10-07 source audit 增量）

- `WorldStorageRoot` 为 Projectile 提供共享 `EntityRuntime`；slot 中保留的
  `ProjectileEntityState` 只持有 `EntityRuntime` 和 `RuntimeEntityHandle`，不再保留组件值；
  运行态组件保存在精确类型组件表。
  hydration 现在产出临时 `ProjectileInitialComponents`，生命周期 owner 创建/发布根后再协调
  slot 和独立的 owner-scoped protocol index。EntityRuntime 自动登记的 `EntityIdentityComponent`
  是实例身份；`ProjectileIdentityComponent.Identity/ProjectileUuid` 仍是协议投影，不改 wire format。
- 终止、身份登记冲突和容量拒绝会移除候选根；slot 释放后根才完成移除。packet-27 同类型
  Active 更新在原根内更新允许字段；类型替换发新 EntityUuid 和 slot generation。Minion、Sentry、
  Bobber、Counterweight 能力按 hydrated 值决定是否附着，查询可以区分 absent 和 attached 状态。
- Network、HitImmunity、Trail facade getter 在同步 inspect 回调内复制数组/list，不泄漏组件表引用；
  新增 `EntityRuntime.TryInspect` 不推进 data revision。数组写入通过 Projectile system 编辑本地副本后
  提交。tick-coordinator 的免疫次数/额外子步/寿命终止场景仍通过。
- 专项构建：
  `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --nologo -v:minimal`
  退出码 0，0 warning / 0 error；输出位于
  `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`。以下均以
  `--no-build --no-restore` 执行、退出码 0：默认（Projectile disposition 等字段验证）、
  `--identity-index`、`--lifecycle`、`--hydration`、`--network`、`--tick-coordinator`。
  新断言覆盖 root 注册/终止/拒绝清理、slot 复用换根、packet-27 保根/换根、mutable copy 隔离和
  optional capability attachment。`--network` 还覆盖 packet-29 wrong-owner 拒绝、匹配 owner 终止和
  终止后的重复请求 no-op；缺失 identity 对应的索引 no-op 已由 lifecycle 网络终止入口保持并在同组检查。
- Packet gateway 项目验证：
  `dotnet build Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --nologo -v:minimal`
  退出码 0，0 warning / 0 error，输出位于 `Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/`；
  `dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore`
  退出码 0，60 groups passed。`projectile codecs and authenticated gateway owners` 验证现有 wire DTO、sender
  slot 绑定和 gateway handler；owner 为验证 double，不能作为 Simulation 的真实网络 Projectile owner 已接线证据。
- EntityRuntime 只读访问新增 `TryInspect`，不改变 revision。Simulation 受影响项目构建：
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`
  退出码 0，23 warning / 0 error；warning 来自既有 `WorldSectionState` 和 `WorldGeneration` 源码，产物位于
  `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。
- `Test/Terraria.EntityOrganization.Verification` 因 EntityRuntime 访问协议新增的检查通过：
  `dotnet build Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --nologo -v:minimal`
  退出码 0，0 warning / 0 error；`dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore`
  输出 `Entity organization verification passed.`。场景确认 inspect 不推进 revision，且 callback 借用期间结构性终止被拒绝。
- 固定输入战斗场景使用 WorldFile SHA-256
  `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`、seed `1313625176`、
  显式 NPC `--spawn-npc 3` 与 `combat-drop-pickup-input.json`，完成 3600 tick：99 shots、45 accepted
  NPC hits、54 tile collisions、1 death drop、1 full pickup、0 partial pickup；保存提交成功且来源文件未变。
  报告 `Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600.json`，SHA-256
  `CF9A1905B644C52C10A175FC2D99ECCE308460423E3B1D6504C01D22B4BBFCF3`；保存文件 SHA-256
  `F3BBBBB8A4030F8AD3B0D18B507C0AD3213A763602D2B258EA95CF3FBE193BE3`。新进程加载保存文件并推进 1 tick
  成功，2 个存档 NPC 恢复；报告 `projectile-runtime-combat-3600-reload.json`，SHA-256
  `4DC514FC2C3D612D59C977DC1870178C3C5FC3266675069B4107B937C53C93E3`。
  命令：

  ```powershell
  dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 3600 --players 1 --seed 1313625176 --input-script Build/diagnostics/NonCommunicationSimulationAudit/verification-run/combat-drop-pickup-input.json --spawn-npc 3 --save Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600.wld --report Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600.json
  dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600.wld 1 --players 0 --seed 1313625176 --report Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600-reload.json
  ```
- 最新 tick/lifecycle 边界补充：Projectile verifier 使用
  `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-restore --nologo -v:minimal`
  构建成功，0 warning / 0 error，输出位于
  `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`。随后默认、`--identity-index`、
  `--lifecycle`、`--hydration`、`--network`、`--tick-coordinator` 六种模式均以
  `--no-build --no-restore` 运行成功。`--lifecycle` 现以 1000 个可替换且同寿命的 Projectile
  验证满池 replacement 选择 slot 0，generation、protocol index 和 entity-root 数量均正确。
  `--tick-coordinator` 覆盖 callback 替换当前实例后停止旧子步、替换未访问 slot 后本轮可见、
  替换已访问 slot 后不重扫，以及 preparation/update `continue` 和 `return` 跳过寿命尾部；
  越界路径验证跳过 Projectile callback、释放 slot/root，并由 lifetime primitive 保留 TimeLeft。
- Trap optional attachment 调整后的 Simulation 宿主复核：
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`
  成功，0 warning / 0 error；输出位于
  `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。使用相同 fixture、seed 和输入脚本
  完成 3600 tick：99 shots、45 NPC hits、54 tile collisions、1 death drop、1 full pickup、0 partial
  pickup，save committed，来源 WorldFile 未变。新报告
  `Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600-followup.json` SHA-256
  `67B7FEE4CB4EDC30B3C454AEF1350B58421CB091004421BDAC302F46ADBAD69F`；保存文件 SHA-256
  `F3BBBBB8A4030F8AD3B0D18B507C0AD3213A763602D2B258EA95CF3FBE193BE3`。新进程从该保存推进 1 tick，
  成功恢复并更新 2 个存档 NPC；重载报告
  `projectile-runtime-combat-3600-followup-reload.json` SHA-256
  `416BB60D10C4F3F18F01BA92526E524441B850445213A1023C22F2836CB75991`，报告来源 SHA 与保存文件一致。
- 本轮 damage-candidate 与 tick adapter 收口：
  `ProjectileDamageCandidateInput` 为 NPC/PVP admission 捕获查询实际使用的组件值，不再复制
  `HitImmunity` 中的两组免疫数组。Typed NPC/PVP 查询从 gate context 读取单目标免疫快照；保留的
  `ProjectileEntityState` facade overload 只通过同步 inspect 读取指定 NPC/Player 槽位，并委托 typed 路径。
  registry-backed NPC overload 仍由 world registry 查询 static immunity。`IProjectileTickAdapter` 的
  owner movement 输入改收 owner `EntityReference`，trail dust 效果只收 `ProjectileTrailDustRequest`，
  不再收完整 Projectile facade；preparation/update adapter 暂时仍接收 facade。
  `--damage-candidates` 覆盖 NPC/PVP 命中资格、单 NPC/Player 免疫槽、facade/typed 结果一致、static NPC
  expiry 前拒绝及 expiry 相等时放行；`--tick-coordinator` 新增 mode-4 owner reference 专项断言。
- 本轮专项构建和验证：
  `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-restore --nologo -v:minimal`
  退出码 0，0 warning / 0 error，输出在 `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`；
  默认、`--identity-index`、`--lifecycle`、`--hydration`、`--network`、`--tick-coordinator`、
  `--damage-candidates` 七种模式均以 `--no-build --no-restore` 运行，退出码 0。
  Packet gateway verifier 以 `dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore`
  运行，退出码 0，60 groups passed。
- 新 Projectile API 下 Simulation 构建：
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`
  退出码 0，0 warning / 0 error，产物位于 `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。
  固定输入再次完成 3600 tick：99 shots、45 NPC hits、54 tile collisions、1 death drop、1 full pickup、
  0 partial pickup；save committed，来源 WorldFile 未变。报告
  `Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600-candidate-input-v3.json` SHA-256
  `20EA8712F4B2678D256E9E1D4C505678A66FEDBB4B4B21C76A604C1056694A2F`；保存文件 SHA-256
  `F3BBBBB8A4030F8AD3B0D18B507C0AD3213A763602D2B258EA95CF3FBE193BE3`。命令输入仍是 source WorldFile
  SHA-256 `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`、seed `1313625176`、
  `combat-drop-pickup-input.json` 和 `--spawn-npc 3`。新进程加载保存文件并推进 1 tick，成功发布、
  恢复且更新 2 个存档 NPC；source SHA 与刚写出的保存文件一致。重载报告
  `projectile-runtime-combat-3600-candidate-input-v3-reload.json` SHA-256
  `355785DEEE8C15C3C4786D9E30330B72FD7B778B596036944B42A57E38DC3D6F`。
- 随后修正 facade 的免疫读取顺序，确保非本地 NPC 以及无效 Player 槽先返回资格拒绝，不提前索引免疫数组。
  `--damage-candidates` 新增 `NonLocalOwner`/`InvalidTargetSlot` 顺序断言；Projectile verifier 重建为
  0 warning / 0 error，七种模式再次全部以 `--no-build --no-restore` 运行通过。修正后的 Simulation 项目
  重新构建退出码 0，0 warning / 0 error，仍输出在 `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。
  固定输入 3600 tick 结果仍为 99 shots、45 NPC hits、54 tile collisions、1 death drop、1 full pickup、
  0 partial pickup；save committed、来源未变。最终报告
  `Build/diagnostics/EntityOrganization/B5/projectile-runtime-combat-3600-candidate-input-v4.json` SHA-256
  `CC737F42111C8DDCB4EF73A031417D6E1095223C2974B80C1A64FF5E78F667AD`；saved WorldFile SHA-256
  `F3BBBBB8A4030F8AD3B0D18B507C0AD3213A763602D2B258EA95CF3FBE193BE3`。新进程重载并推进 1 tick，成功
  发布及更新 2 个 NPC，报告 source SHA 与保存文件一致；重载报告
  `projectile-runtime-combat-3600-candidate-input-v4-reload.json` SHA-256
  `C9A6FFE595B0A032F39D19328D7C5CFBCC806C54B24A192A8F2B560B1D06703D`。
- 普通箭矢 AI、移动提交和 Magic Quiver cadence 已提供组件/ref API；Simulation adapter 对 behavior、
  kinematics、trajectory、definition、direction 和 cadence 分别通过 `EntityRuntime.TryEditComponents`
  或 `TryEdit` 提交。嵌套 adapter 现在显式接收当前 `EntityRuntime`，不再隐式访问外层 store 字段。
  Projectile lifecycle verifier 新增最终代际边界：满池 replacement 在 `uint.MaxValue` 拒绝且清理候选根，
  原 slot/index/root 保持一致；终止后该 slot 永久耗尽，后续创建同样拒绝并清理候选根。
  Magic Quiver typed cadence 验证覆盖 owner 无装备、NPC 弹体、非友方、非箭矢及已达上限的拒绝且不改 cadence；
  `--tick-coordinator` 覆盖 mode-1 trail dust 的 typed request、最旧缓存位置和 update→effect→post 顺序，
  与现有 mode-4 owner reference 场景一同通过。
- 本轮构建/验证：Simulation 项目命令
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`
  退出码 0，0 warning / 0 error，DLL 位于 `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。
  Projectile verifier 构建退出码 0，0 warning / 0 error，产物位于
  `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`；默认、`--identity-index`、`--lifecycle`、
  `--hydration`、`--network`、`--tick-coordinator`、`--damage-candidates` 七种模式均通过。
  Entity Organization verifier 构建退出码 0，0 warning / 0 error，运行输出
  `Entity organization verification passed.`，DLL 位于
  `Build/bin/Terraria.EntityOrganization.Verification/Debug/net10.0/`。
- 最新 typed motion API 的宿主固定输入证据：source WorldFile SHA-256
  `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`，输入脚本 SHA-256
  `D5B3FD9F9E579C296CC6EF821312CF60F8D3E1038129670376AFE47AA9B42CC4`，seed `1313625176`、
  显式 `--spawn-npc 3`，3600 ticks：99 shots、45 NPC hits、54 tile collisions、1 death drop、
  1 full pickup、0 partial pickup；save committed，source 未变。主报告
  `projectile-runtime-combat-3600-candidate-input-v5.json` SHA-256
  `D31A2AD6D82D01F5D6B37D9D4C8CFA66DE42A0D111B4BE23B3CC571C83A98718`；保存文件
  `projectile-runtime-combat-3600-candidate-input-v5.wld` SHA-256
  `F3BBBBB8A4030F8AD3B0D18B507C0AD3213A763602D2B258EA95CF3FBE193BE3`。新进程重载该文件并推进
  1 tick，成功恢复/更新 2 个 NPC，source SHA 与保存文件一致；重载报告
  `projectile-runtime-combat-3600-candidate-input-v5-reload.json` SHA-256
  `6022D5D580A04108512A3A3030B8291494EEF0208161CD994A3E5B24E7559321`。
- Magic Quiver cadence 的宿主对照使用相同 WorldFile/seed、2 ticks 和一发箭；两个报告都成功、source 未变、
  且末 tick 保留同一个 Projectile。无 Magic Quiver 时 `LastTickProjectileUpdateCount=1`，输入脚本
  `magic-quiver-cadence-off-2ticks-input.json` SHA-256 `FF7777235FC76C7E946B1A76C5D5AB8C36273381D8F83880F41050B31C92D5EC`，
  报告 SHA-256 `483D05F0347E7F9F84617312E007075C4F64A1A269EE2EC33301BE1BD14F0FEE`；装备时子步数为 2，
  输入脚本 `magic-quiver-cadence-on-2ticks-input.json` SHA-256
  `3D25ED38A05E235E75DAF69CDDC0BF6ED711621E9FBD977790F837FC3BE14EE4`，报告 SHA-256
  `42CE2F001D067FB494CF4CBC054F904BAA6E0321BEB7F5F8793A978EAC589021`。两个报告的 source SHA 均为
  `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`。
- 命中和碰撞入口继续从 facade 移向显式值输入：`RuntimeProjectileStore.TryHitNpc` 捕获一次
  `ProjectileDamageCandidateInput` 与 `ProjectileCollisionGeometryInput`，damage gate、NPC candidate、
  damage hitbox、collision environment、AI 137/19 辅助 query 与 selected collision geometry 都按值计算。
  Trail 数组/whip points 仅对实际读取 trail 的类型复制。NPC owner 通过一次 `TryInspect` 获取
  local-immunity 组件，再按目标 slot 显式生成 gate 值（626–628 仍使用调用方选择值）；hitbox 的
  `LocalAi0` 变更和 accepted hit 的 local immunity/penetration 仍分别由 `EntityRuntime.TryEdit` 提交；
  static-immunity System 接收 policy、
  penetration 和 projectile type 值，不再读取 `ProjectileEntityState`。回调、local/static cooldown、
  death drop 与 penetration 的现有顺序保持不变。`--damage-candidates` 覆盖 typed candidate + geometry
  命中分支、disabled static policy、单穿透默认跳过、显式 `AppliesOnSingleHit` 写入及 expiry 边界。
- 本轮 Simulation 构建命令
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`
  退出码 0、0 warning / 0 error，host DLL 位于
  `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。Projectile verifier 构建命令
  `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-restore --nologo -v:minimal`
  退出码 0、0 warning / 0 error，输出在 `Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/`；
  默认、`--identity-index`、`--lifecycle`、`--hydration`、`--network`、`--tick-coordinator`、
  `--damage-candidates` 七种模式均退出码 0。
- typed collision input 和 explicit local-immunity gate 接入后，host 固定输入重新完成 3600 ticks：99 shots、45 NPC hits、54 tile collisions、1 death drop、
  1 full pickup、0 partial pickup；保存已提交且来源未变化。主报告
  `projectile-runtime-combat-3600-candidate-input-v8.json` SHA-256
  `CBDAC30D5E3E466E3A5F9812F0F7B0926E2E31A76091AF364070CA339BAD25C4`，保存文件 SHA-256
  `F3BBBBB8A4030F8AD3B0D18B507C0AD3213A763602D2B258EA95CF3FBE193BE3`，来源 WorldFile SHA-256
  `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`。新进程重载保存文件并推进
  1 tick，恢复并更新 2 个 NPC；重载报告 SHA-256
  `8AA7D8AC77B455D31671180065081638AADB24EE5D5EEA15E247B3DCF01B9BD1`，其来源 SHA 与保存文件一致。
- 本轮源码 SHA-256：`RuntimeProjectileStore.cs`
  `64B2261570D0A6D26E8A631CD7A68DEBD70FE82BAFD32B0655D8D8B9DDFCE6EF`；
  `ProjectileCollisionGeometryInput.cs`
  `A10E53064D0DF92F5A8761172C1BC653D794642B9C66C6B1897C8C11DA2A3467`；
  `ProjectileCollidingGeometryQuery.cs`
  `18F0295CAFFAD20711272D8946580E0DEC2ABC01116058E0612F10D7993AE002`；
  `ProjectileCollisionEnvironmentQuery.cs`
  `3861AE25E43A4A3408D75120F08DA959A4AB1682E48A0900CF2E5E0A351AFDEF`；
  `ProjectileAi137VisibilityQuery.cs`
  `510E169A57C227C7BA0B367A9597F0303319CD677DDE7472D30C9070B9DBA501`；
  `ProjectileAi19ExtensionQuery.cs`
  `C21395AE2BBEBFE936D6DAFB5DD1C33654F8D21E021A7048D5D925D3467E3B09`；
  `ProjectileDamageCollisionQuery.cs`
  `A34798D8E074973F6355B25A4ACC225A77F6276CE60B6B2D2BA11DFD2DB57448`；
  `ProjectileDamageHitboxSystem.cs`
  `12C73144B325AC6CC4DF8B09E600E227E568D3BE04DFEFA5B68EE8E0D474C406`；
  `ProjectileStaticNpcImmunitySystem.cs`
  `6C3612A3267CEDB01B965AF598891623A4973FC43011449209EF10C2F7EE05E9`；
  `ProjectileDamageCandidateVerification.cs`
  `30DA72CC3AA4A98C7EE94EEE87F5E3AC6894A9DC89C03722C979129D90B3F8B1`。
- 该前序检查点的 B5 尚未通过。`ProjectileEntityState` 仍是 preparation/update、damage admission 与碰撞几何的
  兼容输入视图；这些入口尚未全部改为最小能力输入或短期 component access。
  其余受支持 adapter/cadence/callback 组合仍待补证。Application authenticated gateway 到生产
  Projectile runtime owner 的窄 fixture 已通过（包含 queued-world/epoch invalidation），但 Terraria319
  NetworkServer 仍禁用 Projectile packet，不能声称 packet-27/29 进入服务器 EntityRuntime 生命周期。
  当前实现的回退边界是停止会话并从 hydration/WorldStorage 数据重建；不可将运行组件倒灌到另一个长期聚合副本。

## 单写切片和回退边界

- NPC 实例根由 `EntityRuntime.CreateEntity` 注册；`NpcInstanceId` 是兼容查找投影，slot 与
  slot generation 是容量/瞬时句柄投影。它们都不生成或替代 UUID 根。
- NPC 的 `MovementStateComponent`、NaturalDespawn、Health、Lifecycle、Direction、Target、Behavior、
  LocalBehavior、MovementTick、Spawn/Critter、Presentation、Housing、GivenName、Task、ImmediateEffect、
  Identity、LegacySlot 与 DefinitionReference 均挂在 `EntityRuntime` 确切类型表。`RuntimeNpcEntity` 只作
  typed slot/root facade，Definition 是共享只读定义，SavedState 是持久化投影基底；不长期保留组件对象。
  Movement getter 捕获组件值，setter 经 `TryReplace` 提交；自然消失与 housing 等由能力快照读取，
  持续计时/AI 通过短期 `TryEdit` 更新。
- NPC `NpcHealthComponent`、`NpcLifecycleComponent`、`NpcBehaviorStateComponent`、
  `NpcLocalBehaviorStateComponent` 也由同一根身份关联。facade 只返回生命/AI 标量或 AI 值快照，
  不再保存这些组件对象。AI 规则先计算 `NpcAiDecision`，随后分别用短期 `TryEdit` 提交两个 AI
  组件；Projectile 命中解析根、handle、slot generation 后，在 `TryEditPair<Health, Lifecycle>`
  借用范围内调用现有 combat/death owner，借用结束后按原顺序提交 Movement/knockback 并释放死亡 NPC。
  paired edit 不是跨多个领域效果的事务；它只限定两个组件的同步访问与结构变更窗口。
- Projectile 碰撞使用 `RuntimeNpcProjectileTargetSnapshot`，捕获 NPC 根引用、runtime handle、slot
  generation 与当前碰撞所需值。`TryApplyProjectileHit` 在写入前重新解析根并核对 handle、slot 和
  generation；碰撞遍历不再把 `RuntimeNpcEntity` façade 交给 hit owner。该快照限于一次碰撞访问，
  不是跨帧引用，也不承载组件写入权。
- 父生命关系仅在关系 owner 明确绑定时附加；child 的 ParentRelation 值快照同时带父 EntityReference、
  `NpcInstanceId`、旧 slot 和 attached tick。Projectile 命中时重新解析引用、handle、slot 与 generation，
  再嵌套借用父/子 Health、Lifecycle 调用 `NpcCombatSystem`；陈旧或错配关系传入 combat 后会拒绝且不写
  生命/追踪。父死亡时 owner 释放关联子组，并将父 NetId/位置作为 death-drop snapshot 交给 Projectile
  adapter。释放父实例会解绑现存子关系；新父复用旧 slot 不会继承旧关系。
- NPC 创建失败、候选加载失败、释放、Reset 和 Dispose 都移除或释放同一 runtime 的根与组件。
  当前切片回退时应停止该 Simulation runtime，并从 `WorldNpcState`/加载 DTO 重建；运行中不将
  表中 Movement 反向复制到第二个长期字段，也不让两处同时写入。

## 构建与验证证据

### 构建

| 命令/项目 | 退出码 | Warning / error | 输出 |
| --- | ---: | ---: | --- |
| `dotnet build src/NSSLC/Component/Share/Entity/Terraria.EntityEcs.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（加入 `TryEditPair` 后） | 0 | 0 / 0 | `Build/bin/FixtureHost/Terraria.EntityEcs/Debug/net10.0/Terraria.EntityEcs.dll` |
| `dotnet build Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（加入 paired-edit 场景后） | 0 | 0 / 0 | `Build/bin/FixtureHost/Terraria.EntityOrganization.Verification/Debug/net10.0/` |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（Health/Lifecycle/Behavior 切片后） | 0 | 17 / 0 | `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`；17 条 warning 来自既有 WorldGeneration 源码 |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（扩展释放后陈旧 component probe 后） | 0 | 0 / 0 | 同上；Simulation 最新源码已编译 |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（父生命关系接入与 probe 完成后） | 0 | 0 / 0 | `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --nologo -v:minimal -p:FixtureHostBuild=true` | 0 | 23 / 0 | `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（Movement 切片后增量构建） | 0 | 0 / 0 | 同上；依赖 DLL 均位于 `Build/bin/FixtureHost/` |
| `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（加入 NPC 自然消失组件后） | 0 | 17 / 0 | 同上；警告来自 WorldGeneration 遗留代码 |
| `dotnet build Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --nologo -v:minimal -p:FixtureHostBuild=true` | 0 | 0 / 0 | `Build/bin/FixtureHost/Terraria.EntityOrganization.Verification/Debug/net10.0/` |
| `dotnet build Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（增加 class component 编辑场景后） | 0 | 0 / 0 | 同上 |
| `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --nologo -v:minimal -p:FixtureHostBuild=true` | 0 | 0 / 0 | `Build/bin/FixtureHost/Terraria.NpcAi.Verification/Debug/net10.0/` |
| `dotnet build Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj --nologo -v:minimal -p:FixtureHostBuild=true` | 0 | 0 / 0 | `Build/bin/FixtureHost/Terraria.NpcDamageCombatVerification/Debug/net10.0/` |

第一次标准输出构建因 Simulation DLL 被运行中的 `.NET Host` 锁定而以 MSB3021/MSB3027 退出；未结束未知进程，改用仓库定义的 `FixtureHostBuild=true` 隔离输出后构建通过。首次隔离完整构建的 23 条警告来自 WorldStorage/WorldGeneration 遗留字段和源码，不在本批源码中。

### 验证器

| 命令 | 结果 |
| --- | --- |
| `dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | `Entity organization verification passed.` |
| `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | `PASS: NPC AI center, ties, target gates, repeatability, phase boundary, home, coverage.` |
| `dotnet run --project Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | `PASS: P12 combat, life, tracker and death core smoke` |

EntityRuntime verifier 的 paired-edit 场景检查了两个组件同时提交、借用期间禁止 detach/termination，以及第二组件缺失时拒绝且不调用 editor、不改变第一组件；最终 `dotnet run` 退出 0。

### B2 组件表、版本化访问与性能证据

- `ComponentStore<T>` 按 runtime local index 使用稠密 cell 数组，cell 核对 generation；实体表仍
  校验 RuntimeId。Type 反向成员供 Match 使用，同一组件 cell 由定向访问和查询解析到。
- `TryCapture` 递归检查返回投影的字段形状，只接受值类型和不可变字符串，拒绝会带出内部实例的
  class/array/collection 引用。class 组件 capture 委托仍是受信任 owner 代码：C# 无法阻止委托在
  执行时修改或保存传入对象；对当前调用点已审查为仅投影值字段/不可变字符串、不存留引用。该项
  是受控 API 限制，不是恶意调用者安全边界。
- 新增 `EntityComponentSnapshot<T>`，捕获时绑定 handle、精确 component type、attachment revision、
  data revision 和值。版本化 `TryReplace` 在任一身份/版本变化后拒绝提交；Replace 推进两个 revision，
  每次 `TryEdit`（含抛异常回调）推进 data revision。编辑异常透传，可能已有局部写入，不提供事务回滚。
  `Dispose` 在任何实体仍有借用时拒绝清理。`Match(List<RuntimeEntityHandle>)` 清空并复用调用方缓冲区，
  每个候选仍须重新解析。
- EntityOrganization verifier 增加：mutable-reference snapshot rejection、class 状态实例隔离、struct 编辑
  经新 Match 可见、edit/pair-edit/replace/detach-readd/异常后的 revision 冲突、借用内拒绝 Dispose 与
  重入写、删除后拒绝陈旧查询候选、缓冲区重用清空。以下命令 exit 0：
  `dotnet build Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`（0 warnings / 0 errors，输出在 `Build/bin/FixtureHost/Terraria.EntityOrganization.Verification/Debug/net10.0/`）；
  `dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true`（`Entity organization verification passed.`）。
- 容量基准报告：`Build/diagnostics/EntityOrganization/B2/component-store-benchmark.json`，SHA-256
  `DD3A9A9FF2C7442495B3970C0BB614D74067E01DCC78BE41D7C29C638F57F33D`。命令：
  `dotnet Build/bin/FixtureHost/Terraria.EntityOrganization.Verification/Debug/net10.0/Terraria.EntityOrganization.Verification.dll --benchmark Build/diagnostics/EntityOrganization/B2/component-store-benchmark.json`；退出码 0。每个场景 5 次 × 100 次完整遍历，基线为等容量的直接数组值组件访问；计时包含访问，不含创建。基线不含 runtime 身份、线程、borrow 或 revision 检查，因此只用于显示组件表访问成本，不是完整帧或语义等价结论。

  | 场景 | 容量 | Read ns/实体 基线/runtime | Write ns/实体 基线/runtime | Scan ns/实体 直接数组/复用 Match/数组 Match | Match 分配 复用/数组 |
  | --- | ---: | ---: | ---: | ---: | ---: |
  | Projectile | 1000 | 5.3 / 249.7 | 8.8 / 342.2 | 5.2 / 394.6 / 304.9 | 0 / 7,332,800 B |
  | NPC | 200 | 5.9 / 226.5 | 8.8 / 256.9 | 5.4 / 278.9 / 303.8 | 0 / 1,721,600 B |
  | Player | 255 | 4.8 / 219.0 | 8.7 / 242.1 | 5.1 / 265.4 / 299.3 | 0 / 1,853,600 B |

  复用 List 消除了查询候选分配，运行时访问仍明显慢于直接数组；保留测量，不声称性能提升。已将完整
  handle 字典改为 local-index cell 数组，并增加可复用 Match 入口作为针对该成本的存储优化。基准是
  harness microbenchmark，真实领域 tick 的批量访问仍由 B4/B5/B8 性能回归决定是否引入批量访问 API。
- 以最新 EntityRuntime 在 Simulation 的 NPC 运行态上做一次宿主投影 smoke：
  `dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 1 --players 1 --seed 1313625176 --spawn-npc 3 --report Build/diagnostics/EntityOrganization/B2/simulation-snapshot-revalidation-1.json`
  exit 0；加载发布完成，1 tick/3 NPC 更新，NPC 行为与 Direction snapshot 捕获通过，来源 WorldFile 未变。
  报告 SHA-256 `2A78C13964EC00ABCA36F5BD6DB7E7D23DC4EB3D0394AD3BDAE84E2F0A761B7D`。

### 宿主场景

Simulation 运行程序集：`Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。

输入 WorldFile fixture：

| 文件 | SHA-256 |
| --- | --- |
| `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/gameplay-3600.wld` | `417FC468105F0F7D55C7975B107AB0668A735E56D728E8FE3BC7A14058A619F6` |
| `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/pressure-plate-probe.wld` | `999E73436C98C6CE6FE9C55B47BB59892CC15D147B8E645D4999AB7CCBE6695A` |

| 场景 | 命令/报告 | 结果 |
| --- | --- | --- |
| NPC 槽位、Reset、两次连续世界切换（在 Movement 切片前的身份基线） | `Build/diagnostics/EntityOrganization/B3/npc-root-world-switch.json` 及 `.switch.json` | 初始 3 NPC；满 200 槽拒绝第 201 个；释放后同槽 generation 递增、根变化、旧 slot handle/root 失败；Reset 发新根；两次切换各 3 NPC，旧引用均被新 runtime 拒绝 |
| 同样场景加 MovementStateComponent 表访问 | `Build/diagnostics/EntityOrganization/B3/npc-root-world-switch-component.json` 及 `.switch.json` | 退出码 0；1 个初始 tick、2 次世界切换成功；slot reuse/reset 根变化；两次切换均 `NpcRootsDidNotOverlap=true`、`PreviousNpcReferencesRejected=true`、`PreviousNpcHandleRejected=true` |
| NPC 槽位、Reset、两次连续世界切换（Movement 与自然消失状态均在表中） | `Build/diagnostics/EntityOrganization/B3/npc-root-world-switch-natural-state.json` 及 `.switch.json` | 退出码 0；slot reuse、Reset 根变化通过；两次切换均根不重叠并拒绝旧引用。主报告 SHA-256 `7A2A62F2BE60DEA2627C07BB489425A8BD3AE2C7A57CA4618D9D27149401AA1F`，切换报告 `18A6FE7579311111319113E2EDD9826AA84E4A4A36C88D277C1C3A234D1E2715` |
| 连续 NPC 更新 | `Build/diagnostics/EntityOrganization/B3/npc-movement-natural-state-600.json`（SHA-256 `8BB72647649CE29F14D998263571D2719865D24526C977D94F83ED8255703C77`） | 退出码 0；600 ticks；3 个 NPC；`RuntimeNpcsUpdatedAtFinalTick=true`；NPC AI/重力更新后的坐标从 runtime 组件表读取 |
| NPC 自然消失 grace period | `Build/diagnostics/EntityOrganization/B3/npc-natural-despawn-1-300.json`（SHA-256 `EC764D2BB225253BC62397B8C0B4467F4E91C32C6F2760376C9060E1815B73EC`） | 退出码 0；NetId 1 在第 300 tick 按 `OutsideLivingPlayerRangeAfterGracePeriod` 策略退出，runtime active 数由 3 降至 2 |
| Blue Slime 当前 AI 600 tick | `Build/diagnostics/EntityOrganization/B3/npc-behavior-600-parent-relation-verified.json`（SHA-256 `2FA2826AC41727C57CF69BC783D2581F0373053296D44D22C2C60D4FCB9070A7`） | 退出码 0；world SHA-256 `417FC468105F0F7D55C7975B107AB0668A735E56D728E8FE3BC7A14058A619F6`、seed `1313625176`、600 tick；NetId 1 从 X=33520 按当前 `NpcBlueSlimeProfile` 推进至 X=34032，3 NPC 均更新到最终 tick |
| 父关系伤害、解绑、父 slot 复用与死亡掉落投影 | `Build/diagnostics/EntityOrganization/B3/npc-parent-relation-combat-reuse-verified.json`（SHA-256 `BEEA7B3C03DBE0878EA5232235D3F119D2975C3E8F3F00405F6DE8074C7E0B55`） | 退出码 0；父/子命中与生命镜像通过；释放后关系解绑；Zombie 父 slot 复用为新 instance，子命中不影响替代父；父致死命中释放父子组并投影 Zombie 掉落类型/父位置；active count 恢复 |

Movement 切片前后对照命令读取 `npc-root-world-switch.json` 与
`npc-root-world-switch-component.json` 两份报告；1 tick、3 个 NPC 及 slot、NetId、Action、生命和
位置快照相同。UUID 是每次运行新发放的随机根，因此只比较其不重叠和陈旧拒绝断言，不要求字面相同。

这些场景只证明 B3 当前明确支持集及其合成父关系 owner 路径，不代表其他批次验收；Wall of Flesh
`aiStyle 28` 分段来源仍由 Simulation exact manifest 明确拒绝。

### NPC Projectile 命中、死亡掉落与保存重载复核

- 扩展 `RuntimeNpcEntity.Movement` 缺失错误，报告 NPC slot/generation、runtime handle、runtime ID、
  当前 entity 状态和 Movement 组件是否仍登记，便于区分 stale facade、终止中实例与组件关联缺失。
- 受影响宿主及依赖图构建：
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`；
  exit 0，17 warnings / 0 errors，输出位于
  `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。警告来自既有
  WorldGeneration 源码。
- 按固定来源世界、seed、script 和实际显式 NPC 输入运行 3600 ticks：
  `dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 3600 --players 1 --seed 1313625176 --input-script Build/diagnostics/NonCommunicationSimulationAudit/verification-run/combat-drop-pickup-input.json --spawn-npc 3 --save Build/diagnostics/EntityOrganization/B3/combat-drop-pickup-3600-spawned-zombie.wld --report Build/diagnostics/EntityOrganization/B3/combat-drop-pickup-3600-spawned-zombie.json`
  exit 0；99 shots、45 accepted NPC hits、1 Zombie death drop、1 full pickup、0 partial pickup；
  保存已提交，来源世界未变。输入世界 SHA-256 为
  `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`，保存文件 SHA-256 为
  `C5D617A39E53E1954BA6ABA9292CF4BC28DBC642323AF4465287D19FE477F7B7`。报告 SHA-256 为
  `811CA34C243EC7F979F292096C82C0A8D6FCD7AE23A7BCB8CBE959E6C4EE45A5`。
- 在新进程以保存文件加载并提交 1 tick：
  `dotnet Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/EntityOrganization/B3/combat-drop-pickup-3600-spawned-zombie.wld 1 --players 0 --seed 1313625176 --report Build/diagnostics/EntityOrganization/B3/combat-drop-pickup-3600-spawned-zombie-reload.json`
  exit 0；来源 SHA-256 与保存文件相同；已死亡 Zombie 未重新加载，Guide / Old Man 两个 NPC 的
  存档位置恢复并推进。重载报告 SHA-256 为
  `25C4B8B8CCD325C6B73471E42085A8D436B78EA3370266E3D12D44E0826D5572`。
- 在此前 760/900 tick 的探测运行中观察到一次 Movement 缺失异常；完成上述受影响依赖图构建后，
  760 tick 死亡/释放场景和规定的 3600 tick Zombie 场景均通过。当前证据不能确定前次异常根因，
  因此不把它记为已修复；该历史轮次记下当时 B3 当前明确支持集的必需场景已验收。2026-10-07
  源审计曾重开共享空间宿主门禁；该检查点的 B3 状态是历史记录。随后 38 场景 matrix 覆盖当前支持集的 NetId 4 spatial/cleanup；0B052D…981E56 PDB/CodeView map 只核实 artifact-time source 与 DLL/PDB 对应。主验收 receipt 记录 9 项独立 NPC AI 后续 current-tree source hash drift，超出本次运行验收范围；该 map 不声明当前源码与编译输入相同。若未来复现，仍应通过 handle/runtime 诊断定位实际释放路径。

### Projectile target 根快照切换与陈旧引用探针

- 新增 `RuntimeNpcProjectileTargetSnapshot`。`RuntimeNpcStore.CreateProjectileTargetSnapshot` 只投影
  Projectile 命中判断所需的 NPC 值及根引用；`TryApplyProjectileHit` 重新解析根并核对当前
  runtime handle、slot 与 generation 后再调用 combat owner。
- `RuntimeNpcStore` 槽位容量 probe 延伸检查 Projectile target：200 槽填满，释放 slot 2 generation 1，
  同槽复用为 generation 2；旧 EntityReference、旧 `RuntimeNpcEntity` facade 与旧 projectile target
  均被拒绝，替换实例生命值未改变，probe 后 active count 回到 2。报告：
  `Build/diagnostics/EntityOrganization/B3/npc-stale-projectile-target-slot-probe.json`，SHA-256
  `CE191B62A4AD866F1C423F0FC0763120025BAE8467153C58DECC3B55CB430A1F`。
- 完整 combat fixture 在快照切换后通过 3600 ticks：99 shots、45 accepted hits、1 NPC death drop、
  1 full pickup、0 partial pickup；保存已提交。报告：
  `Build/diagnostics/EntityOrganization/B3/combat-drop-pickup-3600-runtime-npc-snapshot-final.json`，
  SHA-256 `0E4359B15C8F649BA7DF9696D4691A1992DBA92F649495485EBCFB55627C7AA5`；保存文件仍为 SHA-256
  `C5D617A39E53E1954BA6ABA9292CF4BC28DBC642323AF4465287D19FE477F7B7`。
- 新进程加载该保存文件并提交 1 tick 后，死亡 Zombie 未恢复，Guide / Old Man 两 NPC 的存档位置恢复；
  `SourceSha256` 与保存文件一致。重载报告：
  `Build/diagnostics/EntityOrganization/B3/combat-drop-pickup-3600-runtime-npc-snapshot-reload.json`，
  SHA-256 `3DB1941B5FC9B5F6074C5139D75C748FBC558C16CC1EA0BB8BE0875014E8B4D2`。
- 快照切换后增量构建命令
  `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`
  exit 0，0 warnings / 0 errors；产物位于 `Build/bin/FixtureHost/NSSLC.Tools.Simulation/Debug/net10.0/`。
- Projectile target 快照切片验收时的 Simulation 源文件指纹（后续 Health/Lifecycle/Behavior
  切片的最终源码指纹见下一节）：

  | 文件 | SHA-256 |
  | --- | --- |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs` | `7F331A81B511A884D8DD92EA434548BC6C258E0F85A118B906D470BC39360DBF` |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs` | `5574BF6FAD8E42A74016921FA1436A4C0EAD793228D514F4192520A147A042BD` |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcProjectileTargetSnapshot.cs` | `B86C180997F9E5E5045E3E7377208D08B8D95A21B468D22F65789B7C36E7133D` |
  | `src/NSSLC.Tools.Simulation/RuntimeProjectileStore.cs` | `18FEA3D888DA30A2F16F0162B0D33855BB648C5EF51F91FE016AAD443E7FAC99` |
  | `src/NSSLC.Tools.Simulation/Program.cs` | `48B3F3A1EB4F89E299BBF4731ABCC74AF4F6710EDC1F3D30FF19A1EFECC0331C` |

### NPC Health/Lifecycle/Behavior 组件表切换

- `RuntimeNpcEntity.Hydrate` 在实体发布前附着 Health、Lifecycle、Behavior 和 LocalBehavior；
  facade 不持有组件对象。行为读取复制为 `NpcAiStateComponent`，NPC AI owner 计算后按组件 cell
  编辑并提交 Action/AI 槽位；每 tick 更新标记也写回 Behavior cell。
- `RuntimeNpcStore.ApplyProjectileHit` 保持旧入口的 NPC 根/handle/slot generation 校验。combat owner
  在 `TryEditPair<NpcHealthComponent, NpcLifecycleComponent>` 范围内接收原组件实例，保留当前伤害
  追踪、生命扣减、死亡转换和结果；paired borrow 返回后仍按原顺序提交 movement/knockback，随后
  死亡时释放根、组件和 slot。该阶段的 Player contact 只读取 `IsActive` 标量；完整根/值快照
  协议在下方后续复核中完成。
- 固定 600 tick 输入对照 `npc-movement-natural-state-600.json`，按旧报告实际记录的显式
  `--spawn-npc 1` 重跑；Slot/NetId/Action/Life/Position/NaturalSpawned 状态投影相同，3 个 NPC
  均更新至最终 tick。输入 WorldFile SHA-256 `417FC468105F0F7D55C7975B107AB0668A735E56D728E8FE3BC7A14058A619F6`。
  新报告 `npc-behavior-components-gameplay-600-spawned-1.json` SHA-256
  `7911FAAF324F597A738B046826F67CA38A42F0AD6040C8EF7F0F958B71E17815`。
- 同槽复用 probe 报告 `npc-behavior-components-slot-probe.json`，SHA-256
  `4ABB699E856F6327E6E9363A180C4F0447E37F210FE5E31B9367F5CAF15572CC`：容量 200，slot 2
  generation 1 复用为 2，旧根/handle/Projectile target 拒绝，替换 NPC 生命不变；释放后的旧 facade
  无法读取 Health/Behavior cell，新实例 AI/LocalAI 均恢复默认值，probe 后 active 数回到 2。
- 使用固定来源世界、seed、输入脚本和显式 `--spawn-npc 3` 的 3600 tick 主线：99 shots、45
  accepted hits、1 NPC death drop、1 full pickup、0 partial pickup，save committed，final NPC
  tick 全覆盖。报告 `combat-drop-pickup-3600-behavior-components.json` SHA-256
  `E23B0B5A575B11F5E2B8F74B54B205DC4039089E8589189BC24CC0ED22413555`；saved WorldFile SHA-256
  `C5D617A39E53E1954BA6ABA9292CF4BC28DBC642323AF4465287D19FE477F7B7`，与前切片同输入的存档一致。
- 新进程读取本次保存并运行 1 tick：死亡 Zombie 未恢复，Guide/Old Man 两个存档 NPC 恢复并推进；
  报告 `combat-drop-pickup-3600-behavior-components-reload.json` SHA-256
  `CD3076F07381B8A722DDF0FB24EA4A01F553D3AED6C7AF6E519108A70B6CAB1E`。
- 该阶段实际运行了 EntityOrganization、NpcAi、NpcDamageCombat 三个 verifier，分别通过；
  受影响程序集都输出到 `Build/bin/FixtureHost/`。当时 B3 仍进行中；Player contact 和
  TrainingDummy 的后续切片见下方，压力板当前入口范围也已复核。
- 本轮最终源码指纹：

  | 文件 | SHA-256 |
  | --- | --- |
  | `src/NSSLC/Component/Share/Entity/ComponentAccess.cs` | `897637F1E5614756825C0949C0CC419DB5BE07AB0842E56EBE394D2679B2D430` |
  | `src/NSSLC/Component/Share/Entity/EntityRuntime.cs` | `0BD94223765310B33E2CB582F2816A1B6B5CD757C7AFA17430F79C41FBA111EA` |
  | `Test/Terraria.EntityOrganization.Verification/Program.cs` | `CE5CC6C1BDBEA66A5CA6BA6D05834286518C504924A8ACE007767D145D4CB1D4` |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs` | `FA38F35BAFDF4029C7DD1757A12929969104F97B8941B3BCE900EA8378DCC32B` |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs` | `7DCC6F27A94E9F9D2D5C0F585A61BF4C67FD4E6BE893115FB9D1A356AD41DC96` |
  | `src/NSSLC.Tools.Simulation/RuntimePlayerStore.cs` | `B566A9C865111026E3A34D6080DC4BD8B4FBB543882A1BC770A17F9FE4C0D196` |
  | `src/NSSLC.Tools.Simulation/Program.cs` | `700FC9D65F0E86FDECBED8B0A65450F1B0F3BE0C5308C7982C8054F4AB8075B8` |

### NPC 跨系统读取、TrainingDummy 生命周期与保存兼容性复核

- Player/NPC contact 改为逐 tick `RuntimeNpcContactSnapshot`，包含 NPC 根引用和伤害/位置/碰撞值；
  Player owner 不再持有 `RuntimeNpcEntity`。同一固定 world/seed 的 600 tick 场景记录 6 次 NPC
  命中、玩家生命 79；报告 `npc-player-contact-600-final.json`，SHA-256
  `C3528B9DD1E2B368BEC43AEA69BD88FF8DAD86C58BF79AA7DC4843243C5A0CFB`。
- `ActiveTileEntityTickPhase` 对 TrainingDummy 使用 `RuntimeNpcTrainingDummyBinding`，绑定包含根、
  runtime handle、slot generation 与 anchor；owner 在移除前重新解析。扩展 `--npc-slot-probe`
  覆盖释放/复用后的旧绑定拒绝和替代 NPC 不受影响。与实际 TileEntity 更新结合的 3 tick 探针
  通过：绑定、tile 移除、NPC 释放和 active count 恢复均为 true。合并探针报告
  `npc-slot-training-dummy-lifecycle-final.json`，SHA-256
  `EBA3CC9ABB0509C3D273186A093EDCFF86781BA5D65C82A012E80D9D0DC2450C`。
- NPC 自然生成 adapter 的城镇 slot、中心点和附近 hostile/town 查询改为读取 owner 捕获的
  `SpawnSnapshot`；adapter 不再遍历 `RuntimeNpcEntity`。压力板源码核对结果：
  `ActivePressurePlateTickPhase` 只遍历玩家位置，不存在 NPC 引用入口；执行文档已按当前宿主行为
  修正该范围。
- natural-despawn probe 不再跨 300 ticks 持有 `RuntimeNpcEntity`；初始化捕获根引用、slot 和 NetId，
  报告时重新解析根。固定 300 tick Blue Slime 场景按 grace-period 策略释放 slot，
  `NpcDespawnProbeRemoved=true`。报告 `npc-despawn-root-reference-300.json`，SHA-256
  `1F47ED434AB50FCA9CC474ACEF373B708F460E0924B925B7F012CC7AA9A6222C`。
- 保存投影读取 GivenName、Housing、Presentation 和 Movement 组件；Housing 无当前 HomeTile 时沿用
  `SavedState.Home`。修复后的 3600 tick 存档 SHA-256 恢复为 `C5D617A39E53E1954BA6ABA9292CF4BC28DBC642323AF4465287D19FE477F7B7`。
- 固定战斗输入重新通过 3600 ticks：99 shots、45 accepted hits、1 death drop、1 full pickup、0
  partial pickup，save committed，final NPC tick 覆盖。主报告 `combat-drop-pickup-3600-entity-organization-final.json`
  SHA-256 `AF826DA331EF7EC26FA082EFACEC0A3790E99F1EB2AD1341DFE4E701AF802CCE`；新进程重载 1 tick，
  仅恢复两名存档城镇 NPC，来源 SHA 与保存文件一致。重载报告 SHA-256
  `13111D85758B5295F4BD5EA1239EA1B8D3B129309A6E5195D4E1ECE2AB7A9424`。
- 父关系接入后的固定战斗主线再次通过 3600 ticks：99 shots、45 accepted hits、1 death drop、1 full
  pickup、0 partial pickup；保存已提交，保存 SHA-256 仍为 `C5D617A39E53E1954BA6ABA9292CF4BC28DBC642323AF4465287D19FE477F7B7`。
  主报告 `combat-drop-pickup-3600-parent-relation-verified.json` SHA-256
  `340511933249C03185C16EEF85DE890453DB486C7EF229956E2A280B04EF5A89`；新进程重载 1 tick、2 NPC 更新，
  来源 SHA 与保存文件一致，报告 `combat-drop-pickup-3600-parent-relation-verified-reload.json` SHA-256
  `14B89532AEC76CF6423DEF4262609219C9CCA89F315B0822778064B44D0B9322`。
- 受影响 Simulation 构建通过：`dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -p:FixtureHostBuild=true`，0 warnings / 0 errors。EntityOrganization、NpcAi、NpcDamageCombat verifier 均通过。
- 600 tick Blue Slime 报告差异现按阶段基线判读：早期 Behavior 组件切片报告
  `npc-behavior-components-gameplay-600-spawned-1.json` 保持 X=33520；当前 source-profile AI 路径报告
  `npc-behavior-600-parent-relation-verified.json` 为 X=34032。Version4 `NPC.cs` 的 aiStyle 1 dispatch
  进入 `AI_001_Slimes` grounded counter/jump 分支，当前 B3 明确要求接入该支持 AI，所以旧静止投影不再是
  当前实现的等价期望值。Movement 切片前后 1 tick 核对仍相同；`NpcAi.Verification` 与当前固定 600 tick
  报告通过。此判定不声称对完整 Version4 逐 tick 等价。
- 当前源码指纹：

  | 文件 | SHA-256 |
  | --- | --- |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs` | `B9D18A4ED134D5097E2184CB0C5887FC02E47C63F16B37D657F978C59831EB84` |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs` | `CCDA2A3787E1E9A9116996EED8266C64AE8B89EE990D403BA409C27EB8FCC633` |
  | `src/NSSLC.Tools.Simulation/RuntimeProjectileStore.cs` | `8842CC07411AC42F8425A6D59C94E17E8E2CFC716327594F50C1AF1153D48AF3` |
  | `src/NSSLC.Tools.Simulation/RuntimeNpcNaturalSpawnPass.cs` | `D1D4B4DBB4D635617AFBD30F51896B39F75FF9F9542D6A1EDF9E67F1C20EF21A` |
  | `src/NSSLC.Tools.Simulation/RuntimePlayerStore.cs` | `68583E96B2248FB22F7E9FC16212CAD2539E62A822CFDF6206091CAEFAFEE01C` |
  | `src/NSSLC.Tools.Simulation/ActiveTileEntityTickPhase.cs` | `756C2897A6585EFA7E83698C7CAAD5BF87A863C1B3CE4A1DF68D708127CA2467` |
  | `src/NSSLC.Tools.Simulation/TileEntityRemovalProbeInstaller.cs` | `C00385F6D8B0255F2C11FABE9C2E064AEE282ADB40DC7105C7F5ACA62329660D` |
  | `src/NSSLC.Tools.Simulation/Program.cs` | `A950F0100B1D80886F8504A168B8C99854FA1B8E64A2C1DB4D295CD79CAF2D35` |
  | `Test/NSSLC.Tools.Simulation.Verification/verify.ps1` | `E80086E2CF5F66005C2E1569B1868C0BE349814250021D4A5FD67CBC415D236D` |

### B3 住房重新验证探针回归

- 新增 `RevalidateHousing` 后，Guide 夜间探针继续使用 world spawn tile 作为 Home；该坐标不是有效
  Housing room，首 tick 被正确清除，导致旧断言未进入 `GuideReturnHome`。探针现使用已安装并通过
  `WorldGen.IsHousingRoomValidAt` 校验的房间；home-return `success` 场景也使用相同的有效房间来源。
- 红—绿定向命令使用 fixture `12345-small-final/generated.wld`：夜间场景 1 tick、time rate 50000，
  `GuideReturnHome/Running/cursor 1`；成功回家场景 60 tick，`GuideReturnHome/Completed/cursor 60`，
  `HomeTeleportSucceeded=true` 且最终位置改变。报告分别为
  `Build/diagnostics/EntityOrganization/B4/guide-night-return-home-fixed.json` 和
  `Build/diagnostics/EntityOrganization/B4/npc-home-return-fixed.json`。

### B4 Player 根与能力切片

- `RuntimePlayerEntity` 现在是 runtime handle facade。Lifecycle、Vitals、Movement/Physics、Contact
  Immunity、Spawn Point、ItemUse、Rest、DeathRecord、Ranged Accessories 与 Inventory Slots 均附着在
  Player 根；生命周期和伤害变化通过组件编辑协议提交。普通复活更新原根组件，根身份不变。
- `RuntimePlayerStore` 为 Player 根注册 `EntityReferenceScope.Player`，并使用 NPC store 共用的身份
  登记表。销毁后旧引用失效；同 slot 重建发新根。玩家库存 owner 借用根上的 SlotsComponent，清理时
  释放槽中 Item 引用并转为失效状态；已释放 owner 的查询/编辑入口不会继续访问已 Dispose 的 runtime。
- Projectile owner 保存 Player `EntityReference`，在读取 owner 能力或移动值时按根解析；slot 仅作
  当前容量/连接投影，不能命中新会话玩家。世界切换前清理玩家 runtime，切换后重新 hydrate，新会话
  不接受旧 Player 引用或库存 owner。
- 根销毁/重建报告：`Build/diagnostics/EntityOrganization/B4/player-root-probe-final.json`；覆盖旧根、
  旧库存 owner 拒绝、新引用可解析、handle/reference 改变及新库存 99 箭。
- Simulation 最新 build 退出码 0、0 warning / 0 error；World Clock build/run、
  `Player.LifecycleInteraction.Verification` 与 `SpatialSimulation.Verification` 也通过。当前宿主脚本已
  通过 0/1/2 Player、600/3600 tick、独立输入、jump/landing、NPC、接触伤害和保存等场景，后续在
  `world-item-full-inventory-probe` 启动的 dotnet 子进程返回 `-1` 时停止，`.run.log` 为空；相同 CLI
  直接运行退出码 0，断言 0 pickup、0 partial pickup、保留 stack 5 和 59 个占用槽。该聚合运行未产出
  新 summary，不能以旧 `summary.json` 作为本轮完整通过证据。
- 最新接触伤害报告 `Build/diagnostics/EntityOrganization/B4/simulation/npc-contact-deaths-3600.json`
  记录 3600 tick、3 次 PvE death；初始/最终 EntityReference 相同，最终位置与出生点相同。
- 两次世界切换报告：`Build/diagnostics/EntityOrganization/B4/simulation/repeated-switch-initial.json.switch.json`。
  两项 `PlayerRootLifecycles` 均确认旧根引用/库存 owner 被拒绝，替代根引用改变并能解析；NPC/session
  隔离与新 world tick 也通过。独立 `Player.LifecycleInteraction.Verification` 和
  `SpatialSimulation.Verification` 均退出码 0。
- Player 根探针、死亡/复活身份与位置断言已加入 Simulation 报告/`verify.ps1`。旧库存 owner 首次复核
  曾因 Dispose runtime 查询抛 `ObjectDisposedException` 而失败；owner release 状态修复后定向切换、
  销毁/重建、普通复活与位置断言均通过。最新受影响项目构建通过。

## 2026-10-07 当前 source audit 与正式状态增量

本节覆盖当前只读检查结果和主验收卡最近追加的 evidence。原先 B3/B4 的 NPC/Player 纵向行为与根身份
场景仍作为历史窄证据保留；当时重开的共享空间 host gate 后由本节后续记录的 38 场景矩阵和 source map
按当前支持范围关闭。构建失败日志同样保留，
但失败 artifact 若来自更早的成功构建，不得拿来做新源码 run。

### B0 输入来源和归因边界

| 检查 | 结果 | 使用限制 |
| --- | --- | --- |
| `.agent-workplace/entity-organization-2026-10-07/source-input-sha256.json` 与 `input-copy/` | 523 项全部存在，0 SHA-256 mismatch | 固定的是内容快照，不代表由本迁移改动 |
| 当前 workspace 与 source snapshot | 2026-10-07 10:37:52.7216357+08:00 的只读 checkpoint 有 73 changed / 3 missing；checkpoint 只记录路径与 before/current hash | 原始 523 项 input-copy 与 source-input-sha256.json 历史 hash 保持不变；Git dirty 数量不用于 owner attribution。该 checkpoint 当时记录 owner-attribution artifact 有 8 个后续哈希变化且尚无 post-write rehash；对 73 个 changed 路径的 64/9 重算也是该时点结果。之后主验收 receipt 记录 final recheck stable、provider drift 已记录，并接受本迁移范围；PDB map 的 9 项 current-tree follow-up drift 来自独立 NPC AI 后续写入，超出本迁移运行验收，不证明当前源码等于编译输入 |
| git-status-before.txt | 初始捕获 381 条历史工作树状态 | 仅是背景快照；之后状态含 NPC AI、Social/network 等并行工作。Git dirty 数量不用于本主题当前路径计数、owner attribution 或作者归因 |
| 设计列出的 Version4 源指纹 | 9 个指纹逐个与当前只读文件核对，全部匹配；`Terraria/Entity.cs` SHA-256 为 `EDA89950B21692E6859F993D405AF53BBEC675A55E2FE4E64F748A9CE24ECEC6` | Version4 仍只读；原 hash 表保留为输入证据，不替代每个方法的当前正文审核 |

### 当前支持、single-writer 与真实 caller/兼容投影清单

| 领域 | 当前权威/实际入口事实 | 保留的兼容投影 | 未闭合出口或 unknown |
| --- | --- | --- | --- |
| NPC | RuntimeNpcStore 通过 runtime 根管理当前准入 NPC；Health/Lifecycle/Behavior、自然消失及共享空间状态位于根组件表。Projectile target、Player contact、TrainingDummy、AI/housing/save 等消费者在 38 场景 current host matrix 中有证据 | numeric slot、generation、NpcInstanceId 是分类/旧调用 projection，不是根 | 当前支持集 NetId 1,2,3,4,16,22,37,488 的 spatial/cleanup host matrix 已通过。0B052D…981E56 的 PDB/CodeView source map 只对应 artifact-time DLL/PDB 与 17 项 source Documents；receipt 记录 9 项独立 NPC AI 后续 source hash drift，超出本次运行验收且不使运行证据失效，不表示当前源码等于编译输入。Eye profile 只覆盖开场计数、失去目标退出和首次 dash，不宣称完整 AI parity；aiStyle 28 不支持 |
| Player | `RuntimePlayerStore` 创建根；生命周期、combat、inventory owner 与 Projectile owner 按根解析。普通死亡/复活保根，销毁重建换根；38 场景 host matrix 覆盖 0/1/2 Player、NPC Reset、Player cleanup、small→medium→small switch 与引用/owner 生命周期 | Player slot/session/account 作为边界 projection | 当前矩阵覆盖场景通过；不由此替代 B8 真实 WorldFile late-failure rollback |
| Projectile | B5 typed source 已交付，`ProjectileEntityState` 已删除，`src/`、`Test/` C# 与 csproj 静态搜索无引用；删除旧 identity 后 fresh DLL 5C631F60797CD86E7B23A88CD8B04CF706B3DA2D993EDB75C2ADA65D7CD487B3 上 default、identity-index、lifecycle、hydration、network、tick-coordinator、damage-candidates 七 mode 全通过；另有独立 Application owner gateway fixture | slot/owner/protocol identity 与 wire DTO；NetworkServer packet 27/29 仍 disabled | Simulation Manifest 与 private `RuntimeProjectileTickAdapter` 只支持 type `1` ordinary arrow；七个 domain mode 不实例化该 adapter。38-scene Simulation host 与 same-hash exact 3600 run 另证 ordinary-arrow path；NetId 16 仍有 11/237 behavior differences |
| Item | 17 场景 `RuntimeItemOwnershipVerification.Run()` owner matrix 与 38 场景 Simulation Item 子场景通过，含 spawn cleanup、containment/runtime scope、revision、effect rollback/retry、expiry、partial/full pickup 与 world-presence detach | `ItemEntityRef`、槽位和世界掉落槽保留作投影 | Simulation catalog 无 coin definitions；Version4 `GetItem_FillIntoOccupiedSlot` / `GetItem_FillEmptyInventorySlot` 为 stub，不宣称全量 coin/pickup parity |
| TileEntity | domain root/capture/index/schedule；disposed-store/reuse 17 类访问拒绝；38 场景 host matrix 内 2-root TileEntity save/reload/switch 通过，8 项 reload assertion 均通过；Simulation 当前只处理 type 0/2 | TileEntity numeric ID、anchor、schedule | Version4 Place/NetPlace 是存根；`TileEntity.Remove` 有 index/update-list 清理实现。Leashed 无生产 caller，作为独立限制 |
| Leashed | corrected domain verifier 证明 root、generation、section lifecycle、双向 anchor 与网络校验核心 | legacy slot/generation/section | 当前源码搜索未发现生产 register/remove/section caller；domain fixture 不能补出缺失的宿主接线 |
| LoadedWorldSession / Application | partial-candidate-borrow controlled fixture、38 场景 host matrix、authenticated Application gateway、queued invalidation、真实 WorldFile late-finalize rollback 与 Simulation actual-owner/static-immunity rollback/retry 均通过；zero-tick/normal-switch narrow regression 也绑定当前 DLL `0B052D…981E56` 通过 | session/connection epoch 和 protocol packet 不变 | 历史 failure candidate 过早 Dispose 后 terminal project 读取 `candidate.Lifecycle` 抛错、旧 restore callback 0 次；修复后真实 mode 的 `CleanupFailure Kind=0`、`Gate=false`、`previousReprojectCount=1`，tiles/snapshot 与旧 roots/references/handles 保持、candidate registry 清除、static cooldown 保留，retry 使用第三独立 registry 且拒绝旧引用。helper/fixture frozen hashes `8BE9ED…` / `9EF384…` |
| NetworkServer | 实际进程启动固定 WorldFile，Terraria319 TCP Hello packet 3 admission 分配 slot 0，disconnect 后 clean exit；默认网络 regression 70 groups 通过 | 协议 packet id、Player slot 与 sender scope | 仅有限 host/network boundary；Projectile ingress 未启用，不是权威 gameplay 验收 |

### 实际 caller、write-owner 与退出/清理点

下表记录当前生产 owner 的状态写入口与清理入口。类型表的访问 API 不替代领域 owner；同一组件上
多处入口必须通过所列 owner 协议提交。历史 B0 文件归属另由 Integration 的只读 path/hunk artifact
说明，不从 Git status 或当前表反推作者。

| 状态/投影 | 生产 caller 与写 owner | 退出、清理或失效点 |
| --- | --- | --- |
| EntityUuid 根身份与 identity registry | EntityRuntime.CreateEntity 分配 UUID、登记 EntityIdentityComponent；RuntimeNpcStore、RuntimePlayerStore、ProjectileLifecycleSystem、RuntimeItemRegistry 等领域创建入口向 runtime 请求根 | EntityRuntime.TryRemoveEntity / Dispose 注销 registry 并移除该 handle 的组件；领域 store 在 Reset/Destroy/Dispose 或 candidate 失败路径清理对应根。世界恢复创建新根，旧 runtime reference 不复用 |
| runtime slot 与 generation | EntitySlotStore.TryAllocate/TryAllocateAt/TryReplace/TryRelease 管理可复用 slot generation；RuntimeNpcStore 另管理 NPC instance/slot generation；PlayerSlot 是固定边界 slot | release/replace 校验 expected generation；max generation 退休。玩家 root 销毁走 RuntimePlayerStore.TryDestroyPlayer / store/session Dispose，重建分配新 RuntimeEntityHandle |
| NPC spatial state | RuntimeNpcStore.Hydrate/TrySpawn 构造并挂载 Location/Velocity/Collider/grounded 组件；NPC tick 与 movement/gravity owner 在 runtime component API 提交位置和速度 | NPC 实例由 RuntimeNpcStore.TryRelease、Reset、Dispose 清根并释放槽；玩法死亡按 Npc death owner 规则处理，不默认等于销毁 |
| Player spatial state | RuntimePlayerStore hydration/creation 初始化 Location/Collider；Player physics/tick owner 经 RuntimePlayerEntity 的短期 edit 和 traversal physics commit 更新状态 | 普通 respawn 保持根和组件；RuntimePlayerStore.TryDestroyPlayer 先释放库存关系再移除根，Reset/Dispose 清理整个 store/session |
| NPC life/health | RuntimeNpcStore 的 combat/death owner 与 NpcHealthSystem、NpcDeathLifecycleSystem 提交 Health/Lifecycle/Behavior；Projectile 命中先按根和 generation 重解析 | death owner 按内容决定留根或释放关联组；实例退出由 RuntimeNpcStore.TryRelease/Reset/Dispose 注销引用、清理 slot 与相关 static immunity |
| Player life/respawn | RuntimePlayerEntity.AdvanceLifecycle 调用 PlayerLifecycleSystem；RuntimePlayerStore 的生命周期 tick 在同一 handle 上提交 dead/respawn 状态与 spawn position | 普通 respawn 不换根；只有 RuntimePlayerStore.TryDestroyPlayer 后的 TryCreatePlayerAtSlot 才取得新根；失败创建移除 candidate handle |
| Item stack 与 world presence | RuntimeItemRegistry 创建 ItemInstance/ItemStack，stack 数量与 split/merge/consume 经 registry paired edit 提交；RuntimePlayerInventoryOwner 协调 transfer/pickup，RuntimeWorldItemStore 管世界掉落槽与运动 | split 失败回滚 candidate；归零/consume 或释放关系由 item registry/owner 提交；world item despawn/expiry 由 RuntimeWorldItemStore release 槽位并清根/投影 |
| Player inventory slots | PlayerInventoryCommitSystem 形成提交计划；RuntimePlayerInventoryOwner 通过 PlayerInventorySlotsComponent 编辑槽位并与 ItemInstance relation 同步 | pickup/transfer 失败回滚双方关系；RuntimePlayerStore.TryDestroyPlayer / reset/dispose 先 ReleaseAllItems，再移除 Player root。MainInventorySlots 是公开可变数组，单写边界靠源码约定与 review，不由类型系统保证 |
| Projectile static NPC immunity | RuntimeProjectileStore 在 NPC hit 接受点调用 ProjectileStaticNpcImmunitySystem.RecordAcceptedNpcHit 更新 registry/cooldown | RuntimeNpcStore.TryRelease 清空该 NPC slot 的 static 与 local immunity；WorldStorageRoot.Dispose 清理 registry。B8 rollback assertions 另验证旧 cooldown 保留、失败 candidate registry 清除 |
| 分类与领域 ID projection | RuntimeNpcStore 分配 NpcInstanceId/slot；RuntimePlayerStore 保留 PlayerSlot；RuntimeItemRegistry 生成 RuntimeEntityId/ItemEntityRef；它们各由相应 domain owner 写入，不覆盖 EntityUuid | NPC release/reset、Player destroy/recreate、Item consume/despawn 由各 owner 失效 projection；session/store disposal 退休作用域。TileEntity numeric ID 不在此表当作 EntityUuid |
| TileEntity ID 与 anchor | TileEntityStore 负责 _byId / _byAnchor、TileEntityBindingComponent、ID 分配及 restore/replace；WorldFile codec 只解码/编码持久化投影，anchor validator 校验 tile 位置 | TileEntityStore.Remove 同步删 ID/anchor 索引、runtime entity 与 update schedule；replace/failure candidate 按事务清理；WorldStorageRoot.Dispose 清 store/schedule。TileEntity numeric ID/anchor 是存储域标识 |
| Projectile protocol identity | ProjectileLifecycleSystem 从 spawn owner/identity 输入写 ProjectileIdentityComponent 并在 ProjectileIdentityIndex 登记 OwnerSlot + identity；packet codec 按 NeedsUUID 投影 wire identity | TryTerminate/Replace/RemoveRuntimeEntity 从双向 identity index 注销并移除 runtime root；WorldStorageRoot.Dispose 清索引。packet-27/29 当前仍未在 NetworkServer 注册 |

`PlayerInventorySlotsComponent.MainInventorySlots` 是公开可变数组，单写边界靠源码约定与 review，而非类型系统保证。
`WorldSession.Calendar.EntityId` 是独立日历领域类型，不属于共享旧 EntityId 声明删除范围。
### B5 typed source、mode 与 adapter 映射（当前状态）

B5 已完成 Projectile typed source 交付；`ProjectileEntityState` 文件删除，`src/`、`Test/` C# 与 csproj
静态搜索无引用。旧 facade 退出属于源码 caller/signature 审计，API type-safety build 是补充证据，不由单个
mode 证明。7 个 mode 的精确方法/场景及范围按[执行文档 B5 mapping](../../architecture/execution/2026-10-06-entity-organization-execution.md#b5-verifier-mode-与证据边界)
登记；其中 `--network` 是 domain lifecycle/network apply，Application authenticated owner + Gateway 是独立
fixture。Simulation Manifest 和 `RuntimeProjectileTickAdapter` 当前仅支持 Projectile type 1 ordinary arrow，
其他类型组合明确拒绝；七个 component FixtureHost mode 没有直接实例化该私有 adapter。NetworkServer
packet-27/29 仍禁用。

生产 `ProjectileTickCoordinator` 的 lifetime tail 后已有 `RemainingHits == 0 → TryTerminate(HitLimitReached)`
分支。B5 新增 `VerifyHitLimitTailCleansEntityRoot`，由 typed test adapter 提交末次 accepted hit，并检查旧 handle、
slot、identity index 与 EntityRuntime root 清理，同时排除 lifetime expiry；旧七 mode evidence 不含该断言。
Coordinator 随后以独立 FixtureHost build/run 通过该 `--tick-coordinator` 场景（DLL SHA
`6554391BED980EAD773720A903FCF29F4C8FC6A21139976C4F48DA3C4AB71F01`）。这只关闭新增 assertion 的窄验证缺口，
不表示生产清理逻辑此前缺失，也不升级 B5 整批或 Simulation host gameplay。

独立 B8 3600 tick Simulation run 确实走过普通箭 AI、NPC hit 与 tile collision 路径，详情列于上方 evidence 表；
该 run 不属于七个 component mode，且 artifact SHA `030965…E060` 无匹配 build evidence，与空间矩阵的
`5BDEDF…014D` 不同。237 项比较有 11 项都在 `RuntimeNpcStates[2]`（NetId 16）；故它只能作为成功 run 观测，
行为等价 gate 保持未通过。

### Version4 存根和空方法逐项范围

| Version4 方法 | 当前源审计结果 | 迁移声明限制 |
| --- | --- | --- |
| `Player.GetItem_FillIntoOccupiedSlot` / `FillEmptyInventorySlot` | 方法体是存根 | 当前库存支持不能用来声称 Version4 全量拾取等价 |
| `TileEntity.Place` / `NetPlaceEntityAttempt` | 方法体是存根 | Placement/network side effect 为 unknown |
| `TileEntity.Remove` | 方法体执行索引与 update-list 清理 | 不可将整个方法记为空；需分离已读清理与未读/缺失的外部效果 |
| `LeashedEntity.Remove` / `StreamNetUpdates` / 基类 `NetSend` / `NetReceive` | 空方法体 | 外部/继承/宿主效果为 unknown；生产 caller 亦未找到 |

### 本轮可复核的 JSON acceptance evidence

以下均为已有构建/运行记录；本次文档审计没有重新执行它们。`ArtifactSha256` 指 evidence JSON 中产物
SHA。证据文件包含完整绝对命令、Project、日志路径、时间和计数。

| Evidence JSON（相对 `Build/diagnostics/EntityOrganization/`） | 命令/项目 | Exit；warning/error | 实际输出/范围 | Artifact SHA-256 |
| --- | --- | --- | --- | --- |
| `B5/parallel-20261007/projectile-full-typed-build.evidence.json` | `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true` | 0；0/0 | 编译成功；default、identity-index、lifecycle、hydration、network、tick-coordinator 六模式均以同一 DLL 后续运行通过 | `E613A23F2449E5EF504433E1D5742BFEE8CAC599444E3DC3032A244219F7BA1E` |
| `B5/parallel-20261007/projectile-<mode>-typed-run.evidence.json`（6 files） | `dotnet run --project Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true`；模式：无参数、`--identity-index`、`--lifecycle`、`--hydration`、`--network`、`--tick-coordinator` | 每项 0；0/0 | 记录六个既有 mode 各自 PASS；旧 `ProjectileEntityState` facade 已由后续 typed source 删除，静态 caller/signature 审计与 mode 证据分开 | 同上 |
| `B5/parallel-20261007/projectile-query-immunity-fixture-repair-build.evidence.json` | `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true` | 0；6/0 | 6 条既有 `WorldSectionState` warning；DLL build 成功 | `BB05EB295ED20CC12CD5917117FA6A644D75A57182A1A30966472BA74A1A2813` |
| `B5/parallel-20261007/projectile-damage-candidates-immunity-fixture-repair-run.evidence.json` | `dotnet run --project Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true -- --damage-candidates` | 0；0/0 | `PASS: projectile NPC and PVP capability candidate inputs` | 同上 |
| `B5/parallel-20261007/projectile-hit-limit-tail-current-build.evidence.json` | `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true` | 0；0/0 | Fresh FixtureHost build for the added coordinator assertion; paired source-stability check covered 124 Projectile/verifier files and pre/post set SHA-256 was identical: `B256E37E42F5AF6389437657781B22CA44E567E06F56D7D91689C7939BA3ECDB` | `6554391BED980EAD773720A903FCF29F4C8FC6A21139976C4F48DA3C4AB71F01` |
| `B5/parallel-20261007/projectile-hit-limit-tail-current-run.evidence.json` | `dotnet run --project Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true -- --tick-coordinator` | 0；0/0 | `PASS: projectile ordered tick coordinator and extra-update invariants`; fresh run covers `VerifyHitLimitTailCleansEntityRoot` only as added to the coordinator mode, including handle/slot/identity-index/EntityRuntime cleanup distinct from lifetime expiry; this does not refresh the other seven modes | 同上 |
| B6/parallel-20261007/player-item-owner-slot49-fixture-current-build.evidence.json / -run.evidence.json | dotnet build / dotnet run --project Test/Terraria.PlayerItemSpaceVerification/Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true | 两项 0；0/0 | 该历史单项修正 Gel 23 reverse-scan 实际命中 slot 49 的 owner fixture；17 场景 B6 owner matrix 与当前 38 场景 host Item 子范围后来均通过，见本 ledger 当前状态及 closure 表 | 3E6797A2FC2943E7BDE9D6E688944357812878B474CBD7F75230D7105AA9B761
| B7/parallel-20261007/tile-domain-disposed-store-reuse-current-build.evidence.json / -run.evidence.json | dotnet build / dotnet run --project Test/Terraria.TileEntityVerification/Terraria.TileEntityVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true | 两项 0；0/0 | 该历史单项覆盖 dispose 后 17 类旧读写访问均以 ObjectDisposedException 拒绝且不分配 EntityRuntime root；captured SessionDisposal 与真实 TileEntity save/reload/switch 后续通过，见当前 B7 状态 | B6A61DAF7585724567B1937DF6D12851CB7D02501E5AC51C10ACAD1C479C0012
| B8/parallel-20261007/world-session-generated-failure-import-current-build.evidence.json / world-session-generated-failure-current-run.evidence.json | dotnet build / dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true | build 0；0/0；run -532462766 | 历史失败尝试：run 在 Candidate cleanup must dispose the TileEntity projection. 断言失败。后续 controlled fixture、真实 WorldFile rollback 与 Simulation actual-owner rollback/retry 均通过；此行只保留失败诊断 | 3A5EBEC3B06EC4C2B380541A71412A6A26ED148C17BC766CD47B06E18EA9C7B1
| B8/parallel-20261007/world-session-generated-failure-import-current-build.evidence.json / world-session-generated-failure-current-run.evidence.json | dotnet build / dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true | build 0；0/0；run -532462766 | 历史失败尝试：run 在 Candidate cleanup must dispose the TileEntity projection. 断言失败。后续 controlled fixture、真实 WorldFile rollback 与 Simulation actual-owner rollback/retry 均通过；此行只保留失败诊断 | 3A5EBEC3B06EC4C2B380541A71412A6A26ED148C17BC766CD47B06E18EA9C7B1
| `B8/parallel-20261007/world-session-partial-candidate-borrow-current-build.evidence.json` / `-run.evidence.json` | `dotnet build` / `dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | 两项 0；0/0 | `Entity organization verification passed.`；generation exhaustion、disposed captured store throws、generated handler partial prepare/commit/finalize/publication/cancel cleanup、borrowed old session blocks production commit retirement and restores old state、zero roots、fresh dirty factory、issued-root history 与 final successful switch 均在该 narrow verifier run 覆盖；不覆盖 real WorldFile all-legacy-projection/TreeTops rollback | `3CC305B5E349F382A05C001D786FB72EFA3D02D2FA25CE28CF283D30974FE1E3` |
| `B7/parallel-20261007/tile-host-helper-current-pair-caller-build.evidence.json` / `tile-host-helper-prepare-current-run.evidence.json` | `dotnet build` / `dotnet run --project Test/Terraria.TileEntityVerification/Terraria.TileEntityVerification.csproj --no-build --no-restore -p:FixtureHostBuild=true -- prepare ...` | 两项 0；0/0 | Host fixture preparation only; generated `tile-host-fixture-current.wld` has 2 TileEntities, SHA-256 `DB9E14472323CCB1965B6E3EAEC4A12FD49DA4A95BDFA6504F245A524BF0F417`; not save/reload/world-switch host acceptance | build DLL `831FCEDE4E8FD75A9F160687A5CF2BB0DDE81C9ADFBB5EAD4E8958D5E8DFF7C7` |
| `B8/parallel-20261007/projectile-real-network-owner-queued-player-seam-current-build.evidence.json` | `dotnet build Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true` | 0；0/0 | 成功 build；实际 Application authenticated gateway/Projectile runtime owner 新鲜 fixture | `5C711CCA5032F5840BF13A908D0BD94F1997B07C6500F1ABCFB7DB565344E95B` |
| `B8/parallel-20261007/projectile-real-network-owner-queued-invalidations-run.evidence.json` | `dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true -- --projectile-runtime-owner` | 0；0/0 | `PASS authenticated gateway commands through the production Projectile runtime owner`；覆盖 queued-world/epoch invalidation | 同上 |
| `B8/parallel-20261007/network-default-regression-current-runtime-run.evidence.json` | `dotnet run --project Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | 0；0/0 | 同 fresh DLL；70 groups passed、0 failed | 同上 |
| `B8/parallel-20261007/network-server-runtime-boundary-restored-build.evidence.json` | `dotnet build src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj --nologo -v:minimal -p:FixtureHostBuild=true` | 0；0/0 | NetworkServer build 成功 | `8A3DA4299A13DC95B93604C9E77FAC1633BBFE656F2D7CCF07739C7E6CB48CB0` |
| `B8/parallel-20261007/network-host-smoke/host-smoke.evidence.json` | `dotnet Build/bin/FixtureHost/NSSLC.Tools.NetworkServer/Debug/net10.0/NSSLC.Tools.NetworkServer.dll --world Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld --listen 127.0.0.1 --port 0 --exit-after-first-client --report Build/diagnostics/EntityOrganization/B8/parallel-20261007/network-host-smoke/host-report.json` | 0；0/0 | 固定 world SHA `022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`；实际端点 `127.0.0.1:53278`；Terraria319 Hello Packet 3 admission、slot 0、disconnect clean exit；Projectile disabled，无 packet-27/29 ingress；不是权威 gameplay | 同上 |
| `B8/parallel-20261007/session-borrow-fixture-build.evidence.json` / `session-borrow-fixture-run.evidence.json` | EntityOrganization verifier build；`dotnet run --project Test/Terraria.EntityOrganization.Verification/Terraria.EntityOrganization.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | 两项 0；0/0 | `Entity organization verification passed.`；仅证明 borrowed dispose refusal 保持状态及释放后幂等清理 | `4F5E051FF1B4C7C8BDEE118BAEED21B09C6D44B4B775D21A3016300123347D74` |
| `core/parallel-20261007/spatial-core-lookup-interface-current-build.evidence.json` / `-run.evidence.json` | `dotnet build` / `dotnet run --project Test/Terraria.SpatialSimulation.Verification/Terraria.SpatialSimulation.Verification.csproj --no-build --no-restore -p:FixtureHostBuild=true` | 两项 0；0/0 | `PASS: SpatialSimulation core query cases`；只证明 domain query seam | `356B0C219703DC163BDC7A7DFCCB6278ECD09CCC25B0274128B80A02F177178F` |
| `B8/parallel-20261007/simulation-spatial-and-cleanup-probes-current-build.evidence.json` | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true` | 0；0/0 | 成功 fresh build；产物用于下列 0/1/2 Player 600 tick live host runs | `5BDEDFACD8B1E50898D15343C9A503912F5B1AB48F911D291197356D18E8014D` |
| `B8/parallel-20261007/simulation-live-spatial-600-players-0-run.evidence.json` | `dotnet run --project src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-build --no-restore -p:FixtureHostBuild=true -- <small-world.wld> 600 --players 0 --seed 1313625176 --spatial-entity-probe true --spawn-npc 1 --spawn-npc 1 --spawn-npc 2 --spawn-npc 3 --spawn-npc 16 --spawn-npc 22 --spawn-npc 37 --spawn-npc 488 --report <spatial-600-players-0.json>` | 0；0/0 | 600 ticks；spatial probe enabled；artifact-time unique NPC NetIds `1,2,3,16,22,37,488`，含第二 Blue Slime；当前新增 NetId 4 未生成；source WorldFile unchanged。报告 SHA-256 `13A348D4A47D13D75614DD55BEB85C01DFFF94151CEB82D275F3147E67C56C88` | `5BDEDFACD8B1E50898D15343C9A503912F5B1AB48F911D291197356D18E8014D` |
| `B8/parallel-20261007/simulation-live-spatial-and-npc-cleanup-600-players-1-run.evidence.json` | `dotnet run --project src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-build --no-restore -p:FixtureHostBuild=true -- <small-world.wld> 600 --players 1 --seed 1313625176 --input-script <spatial-probe-input-1.json> --spatial-entity-probe true --npc-reset-preflight-probe true --spawn-npc 1 --spawn-npc 1 --spawn-npc 2 --spawn-npc 3 --spawn-npc 16 --spawn-npc 22 --spawn-npc 37 --spawn-npc 488 --report <spatial-600-players-1.json>` | 0；0/0 | 600 ticks；artifact-time unique NPC NetIds `1,2,3,16,22,37,488`，不含 current NetId 4；borrowed NPC Reset rejected without writes; release then retry; Player/Item/Projectile references and RuntimeId preserved; same local index reused at new generation; stale NPC reference rejected. Report SHA-256 `30D20EFBD722988419E103A8FD2E1AC9381700AF06C4600A9E158159E366CA8A` | 同上 |
| `B8/parallel-20261007/simulation-live-spatial-and-player-cleanup-600-players-2-run.evidence.json` | `dotnet run --project src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-build --no-restore -p:FixtureHostBuild=true -- <small-world.wld> 600 --players 2 --seed 1313625176 --input-script <spatial-probe-input-2.json> --spatial-entity-probe true --player-cleanup-preflight-probe true --spawn-npc 1 --spawn-npc 1 --spawn-npc 2 --spawn-npc 3 --spawn-npc 16 --spawn-npc 22 --spawn-npc 37 --spawn-npc 488 --report <spatial-600-players-2.json>` | 0；0/0 | 600 ticks；artifact-time unique NPC NetIds `1,2,3,16,22,37,488`，不含 current NetId 4；Initialize rejects borrowed Item without writes; Clear/Dispose reject borrowed Player; destroy rejects borrowed Player/Item; release allows retry; Player root rebuilt with generation 2 while NPC root and RuntimeId remain. Report SHA-256 `70E9B055CECCEF823D804F5EF279E8C83CEE8D902722D8EA1C0C714604B15FEB` | 同上 |
| `B8/parallel-20261007/simulation-baseline-comparison-combat-3600-current-run.evidence.json` | `dotnet run --project src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-build --no-restore -p:FixtureHostBuild=true -- Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 3600 --players 1 --seed 1313625176 --input-script Build/diagnostics/NonCommunicationSimulationAudit/verification-run/combat-drop-pickup-input.json --spawn-npc 3 --save Build/diagnostics/EntityOrganization/B8/parallel-20261007/combat-drop-pickup-3600-current.wld --report Build/diagnostics/EntityOrganization/B8/parallel-20261007/combat-drop-pickup-3600-current.json` | 0；0/0 | `Succeeded=true`; 99 shots, 45 accepted NPC hits, 54 tile collisions, 1 death drop, 1 full pickup, 0 partial pickup, final `RuntimeProjectileCount=0`; actual Simulation `RuntimeProjectileStore` ordinary-arrow AI/NPC-hit/tile path ran. This is run observation only: diagnostics has no matching build evidence for this SHA | `03096536CC7FCB1DA1FD07B47926342F10AFDB20F1872A2B39583649ACBDE060` |
| `B8/parallel-20261007/combat-3600-behavior-comparison.evidence.json` | exact finite-host comparison of B0 baseline vs. current 3600 report | comparison: 237 values; 11 different | All differences are `RuntimeNpcStates[2]` (NetId 16): Action, Ai0/Ai1/Ai2, X/Y/OldX/OldY/OldVelocityY, CollideY, DirectionY. Behavior gate remains open; run DLL SHA above differs from spatial matrix DLL `5BDEDFACD8B1E50898D15343C9A503912F5B1AB48F911D291197356D18E8014D` | comparison artifact; not a build |
| `B8/parallel-20261007/simulation-spatial-live-probe-build.evidence.json` | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --nologo -v:minimal --no-restore -p:FixtureHostBuild=true` | 1；17/1 | 历史首次失败：`PlayerNetworkStateSystem.cs:85` 缺 `PlayerZoneAndEnvironmentStateComponent`；无 run，artifact 字段指向旧 DLL | `F5B932A5860B7474A846ABAF588E6A968E1468B51AE097669DC9F0B687F1D55F`（旧产物，不可运行作本轮证据） |
| `B8/parallel-20261007/simulation-spatial-live-probe-import-current-build.evidence.json` | 同 Simulation project fresh build | 1；1/1 | 历史重试失败：并行 `SocialNpcEffectOwner.cs:444` 缺 `PersistedNpcSlot`；无 run，artifact 字段仍是旧 DLL | 同上（旧产物，不可运行作本轮证据） |
| `B8/full-host-current-20261007-r1/full-host-matrix.evidence.json` | `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -FixtureHostBuild -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld -OutputDirectory Build/diagnostics/EntityOrganization/B8/full-host-current-20261007-r1` | wrapper 0；Simulation、clock、TileEntity helper 三个 build 均 0 warnings/0 errors；38 个具名 scenario `summary.Succeeded=true`；script SHA before/after 均 `49FBA5D17D8101CD0E629D735DBB7D235E72C4391E4C9E087C81A3FBD2B59118` | 当前 NPC 集合 `1,2,3,4,16,22,37,488` 的 0/1/2 Player spatial + cleanup probes；Item expiry/physics/partial/full-inventory；TileEntity removal、2-root save/reload/switch、NPC relations/despawn/slot reuse、combat/drop/pickup/respawn、save/reload/cancel/failure 与两次 world switch。WorldFile late-failure legacy rollback 不在通过声明内 | Simulation DLL `621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE` |
| `B8/full-host-current-20261007-r1/tile-entity-host-roundtrip-host.json` + `.switch.json` | 上述 FixtureHost matrix 中的 TileEntity roundtrip/switch 子场景 | Host/save/reload/switch 均 `Succeeded=true` | `RuntimeReferencesChanged`、`PreviousReferencesRejected`、`ReloadedReferencesResolve`、`ScheduleMatchesRecords`、`TrainingDummyBindingValid` 为 true；LogicSensorCheck=3、LogicSensorOn=false、UpdatePassCount=2；old/new NPC roots 与引用隔离 | host report `C3F1D6987302F9E43BB7F64D8B4FCC7C1BC634DAAE633E849A78D7A209467543`; switch report `A0F225985C7CCD35341C258F587EEC4836712E8E03A229CB558BDC972A1C7DF4`; DLL `621C3573…E2FDE` |
| `B8/full-host-current-20261007-r1/combat-3600-exact-baseline-arguments-run.evidence.json` | B0 exact 3600 arguments：small WorldFile、seed `1313625176`、`combat-drop-pickup-input.json`、`--spawn-npc 3` | 0；0/0 | 99 shots、45 accepted hits、54 tile collisions、1 death drop、1 full pickup、0 partial pickup、final Projectile count 0、save committed；input script SHA `D5B3FD…B42CC4` | Simulation DLL `621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE` |
| `B8/full-host-current-20261007-r1/combat-3600-exact-input-comparison-run.evidence.json` + `combat-3600-exact-input-behavior-comparison.evidence.json` | B0 baseline report vs. above same-DLL exact-input report；no additional field filtering | comparator exit 1；237 values / 11 different | Same 11 Mother Slime (`RuntimeNpcStates[2]`, NetId 16) fields/values as the earlier comparison; fresh build/run SHA gap is closed, behavior parity is not | Simulation DLL `621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE` |
| `B8/full-host-current-20261007-r1/combat-3600-comparison-run.evidence.json` + `combat-3600-behavior-comparison.evidence.json` | Full-host gameplay report (liquid diagnostic probes enabled) vs. B0 | comparator exit 1；237 values / 15 different | Previous 11 Mother Slime differences plus 4 liquid diagnostic fields enabled only in this script input; keep this as an input-scoped raw comparison, not a failure of the 38-scenario matrix | Simulation DLL `621C35731250C8790A12CD55B312F5B74145B9FF1444A55A31C6F918BB8E2FDE` |

主验收 current FixtureHost matrix 的 38 个 named scenario 均已通过；历史两次编译错误只保留为诊断，
不代表当前 blocker。当前 0/1/2 Player spatial probe 已覆盖 NetId 4；TileEntity real save/reload/switch 与
Player/NPC old/new reference isolation 也有具名通过报告。当前 38 场景 matrix、正常生命周期和 B8 两类真实 late-failure rollback 均按下表关闭；11/237 行为差异与有限支持范围仍作具名限制。

### 当前 batch gate 与下一项兼容退出

- B0：523 项原始 input-copy/hash 保留；exact 3600 tick run 已按同一 Simulation DLL 通过核心场景。237 项中 11 项 Mother Slime / NetId 16 差异是具名行为限制，不是 parity pass。早先独立重算 73 个 changed 路径有 9 个与 task-capture hash 不符；主验收 receipt 后续记录 source recheck stable、provider drift 已记录，并以 `ACCEPTED_WITH_NAMED_LIMITS` 接受本迁移范围。0B052D…981E56 PDB/source map 的 9 项 current-tree follow-up drift 来自独立 NPC AI 后续写入，超出本迁移运行验收，不使该 DLL 运行证据失效，也不证明当前源码等于编译输入。
- B1/B2：根身份、scope、generation/旧引用拒绝及共享组件/版本化借用协议已在记录范围通过。1000 Projectile、200 NPC、255 Player microbenchmark 记录访问成本；class alias 与不安全 ref 逃逸不受 C# 类型系统完全阻断。
- B3/B4：38 场景 fresh host matrix 覆盖当前支持 NPC NetId 1,2,3,4,16,22,37,488，包含 0/1/2 Player spatial/cleanup、普通 respawn 保根、destroy/recreate 换根与两次正常 switch。0B052D…981E56 source map 只核实 artifact-time DLL/PDB 与 17 项 Documents；9 项独立 NPC AI 后续 source hash drift 超出本次运行验收，不表示当前源码等于编译输入，也不使该 DLL 的空间运行证据失效。Eye profile 仅开场计数/失去目标退出/首次 dash，不宣称完整 AI parity；aiStyle 28 拒绝仍是具名范围限制。
- B5：旧 identity 删除后的 fresh DLL 5C631F…487B3 上七个 mode 全通过；ProjectileEntityState 源码文件及静态引用已退出。真实 Simulation ordinary-arrow path 单独受限于 type 1；NetworkServer packet-27/29 disabled。
- B6：17 场景 owner matrix 与 current Simulation Item 子场景通过。coin definitions 缺失及 Version4 GetItem_Fill* 存根保留为范围限制。
- B7：TileEntity disposed-store/session/save-reload/switch 场景通过。Leashed domain 证据不替代 production register/remove/section caller；当前没有此类生产接线。
- B8：38 场景 host matrix、真实 WorldFile late-finalize rollback、Simulation actual-owner/static-immunity late-failure→rollback→retry、zero-tick 与两次 switch narrow regression 均通过；没有 B8 runtime gate 待验。有限 NetworkServer smoke 不证明 Projectile authoritative gameplay。
- B9：三个生产源码旧声明已删除；literal/config/alias audit 的有效范围为生产源码与 `Test`，排除只读 `src/NSSLC.Infrastructure/分类参考/`。该排除由独立 source-audit evidence 的 `ExcludedReadOnlyReferenceDirectory` 字段明确记录；主 receipt 引用的 final summary 只写 `src`/`Test` 和 0 matches，未重复记录排除字段。被排除目录中的 `EntityIdentityState` 是分类参考类型，不是生产声明。旧 identity 后七 mode 与 current Simulation build 均通过。receipt 15:37 checker PASS（59 links / 16 line links / 22 anchors / 15 fingerprints / 393 manifest）对应本轮范围澄清前的文档快照；当前文档待主验收重跑。

### B9 三项删除退出清单

| 删除声明 | 所属批次与当前状态 | 删除前/后出口审计 | 验收证据与范围 |
| --- | --- | --- | --- |
| src/NSSLC/Component/Projectile/ProjectileEntityState.cs | B5 typed Projectile source 交付后删除；不记为 B9 的身份根类型 | 源码与 verifier C# / csproj 无类型引用；旧 facade/caller 已退出 typed API；Projectile identity index、slot 与 runtime root 仍由对应 owner 管理 | B9/final-current-20261007/old-identity-literal-exit-independent-source-audit.evidence.json 的 production-source/Test 范围 0 hits（排除只读分类参考目录）；删除旧 identity 后七 mode DLL 5C631F…487B3 全通过 |
| src/NSSLC/Component/Share/Entity/Components/EntityIdentityState.cs | B9 共享旧 identity state 声明删除 | 生产范围 literal/config/alias audit 无残留引用；`src/NSSLC.Infrastructure/分类参考/` 中的同名类型按只读参考目录规则排除；新身份根为 EntityIdentityComponent.EntityUuid | independent source-audit evidence 明确排除只读参考目录；主 receipt 所引 final summary 没有序列化排除字段。当前 Simulation rollback/owner build DLL 0B052D…981E56 为 0 warnings/errors |
| src/NSSLC/Component/Share/Entity/Components/EntityId.cs | B9 共享旧 EntityId 声明删除 | 编译/注册、生产范围 literal/config/alias scoped audit 无旧共享声明入口；typed projections 按各 domain owner 持有 | 同上 production-scope audit；当前 Simulation DLL 0B052D…981E56 与旧 identity 后七 mode evidence。WorldSession.Calendar.EntityId 是独立领域类型，合法保留 |

### B0–B9 最小 closure 表

“fixture/method/evidence hash”列绑定到具体已执行 artifact；窄通过不扩大相邻场景。主验收 receipt 已接受 B0–B9 声明范围，并记录 source recheck stable 与 provider drift。0B052D…981E56 的 PDB/source map 后续检查发现 9 项 current-tree source hash drift，来自独立 NPC AI 后续写入，超出本迁移运行验收范围；此 map 只证明 artifact-time DLL/PDB/source 对应。

| 计划场景 | 生产 caller / write owner | 已有 fixture / method / evidence | 唯一待验或具名限制 |
| --- | --- | --- | --- |
| B0：523 项输入与 3600 tick baseline | NSSLC.Tools.Simulation.Program → ActiveNpcTickPhase / RuntimeNpcStore；world item/projectile phases | source-input-sha256.json 保留 523 条；B0 exact arguments run 成功，同 DLL 621C…E2FDE、input SHA D5B3FD…B42CC4；exact comparison 237/11 | 主验收 receipt 已记录 `ACCEPTED_WITH_NAMED_LIMITS` 与 final source recheck stable/provider drift recorded；0B052D…981E56 PDB/source map 另记录 9 项独立 NPC AI 后续 current-tree source hash drift，超出本迁移运行验收范围。11 项 Mother Slime / NetId 16 差异保留为限制，不写 parity |
| B1：根身份、runtime scope、generation exhaustion、旧引用拒绝 | EntityRuntime.CreateEntity / TryResolve / TryRemoveEntity；各 domain store 是创建/销毁 caller | scoped-reference build、generation exhaustion/issued-root run SHA 3CC305B5…74FE1E3；full host slot reuse / Reset / Player cleanup DLL 621C…E2FDE | 无运行门禁待验；分类 slot/ID/protocol projection 继续由领域 owner 持有 |
| B2：组件关联、版本化提交、组合查询、受控借用 | EntityRuntime + typed domain owners | shared-component contract verifier；component-store-benchmark.json SHA DD3A9A…7F33D，覆盖 1000/200/255 capacities | 直接数组更快；C# 对恶意 class alias / 不安全 ref 逃逸没有完全隔离保证 |
| B3：NPC 当前支持集空间、几何、reset/cleanup | SimulationContentSupportManifest → RuntimeNpcStore.Hydrate/TrySpawn → ActiveNpcTickPhase / movement owners | 38 场景 matrix；0/1/2 Player spatial 与 cleanup probes，NetId 1,2,3,4,16,22,37,488；B9 `simulation-final-spatial-required-input-two-switches-current-run.evidence.json` 的 600 tick/2 Player/NetId 4 双切换运行及独立 assertions；DLL 621C…E2FDE 与 0B052D…981E56 | 一次更窄的 NetId 4 命令因漏传必需 `--spawn-npc 1` 退出 1；随后 required-input run 在同一 0B052D…981E56 DLL 上通过，故前者保留为输入配置诊断。PDB source map 仅为 artifact-time 对应；receipt 记录 9 项独立 NPC AI 后续 current-tree source hash drift，超出本迁移运行验收且不使该 DLL 的空间运行证据失效，不声称当前源码等于编译输入。Eye profile 仅开场计数/失去目标退出/首次 dash、不代表完整 AI parity；aiStyle 28 不支持；11/237 行为差异归 B0 |
| B4：Player root/life、库存 owner、引用在正常 world switch 中的保留/失效 | RuntimePlayerStore / RuntimePlayerEntity.AdvanceLifecycle / RuntimePlayerInventoryOwner | 38 场景 host；普通 respawn 保根、destroy/recreate 换根、两次 switch 中旧引用拒绝，DLL 621C…E2FDE | 当前声明限定该矩阵支持范围；failure rollback 由 B8 证据覆盖 |
| B5：typed Projectile modes 与真实 ordinary-arrow host path | RuntimeProjectileStore → ProjectileTickCoordinator / ProjectileLifecycleSystem；hit 交给 NPC owner | 删除旧 identity 后 fresh DLL 5C631F…487B3 七 mode 全过；hit-limit tail SHA 6554391B…71F01；B8 38 场景 / exact 3600 DLL 621C…E2FDE | Simulation 仅 Projectile type 1；packet-27/29 disabled；domain modes 不实例化 private Simulation adapter |
| B6：Item stack/slot transfer/pickup/rollback | RuntimeItemRegistry → RuntimePlayerInventoryOwner / PlayerInventoryCommitSystem；RuntimeWorldItemStore 处理 world presence | 17 场景 owner matrix DLL 3E6797A2…9B761；38 场景 host Item 子范围 DLL 621C…E2FDE | Simulation 无 coin definitions；Version4 GetItem_Fill* 为存根；MainInventorySlots 是公开可变数组、边界靠约定 |
| B7：TileEntity store/index/anchor/schedule 与 Leashed 边界 | TileEntityStore / TileEntityUpdateSchedule / ActiveTileEntityTickPhase；Leashed registration owner 未接线 | disposed-store/reuse 17 access SHA B6A61DAF…C0012；真实 2-root save/reload/switch reports C3F1D6…7543 / A0F225…7DF4；DLL 621C…E2FDE | Leashed 当前无 production register/remove/section caller；Version4 Place/NetPlaceEntityAttempt 是存根 |
| B8a：38 场景 Simulation FixtureHost matrix | Test/NSSLC.Tools.Simulation.Verification/verify.ps1 → Simulation.Program → world/session/domain owners | full-host-matrix 38 scenarios、三 builds 0 warnings/errors，DLL 621C…E2FDE | 矩阵已通过；非权威 Simulation，不能外推 NetworkServer 全玩法 |
| B8b：真实 WorldFile candidate late-failure rollback | WorldFile → WorldLoadCoordinator / RuntimeWorldLoadLifecycleProjection → restore callback | real-world-late-finalize rollback evidence，DLL 5BA221…E15B4；失败 candidate 清理、旧 roots/references/handles、tile bytes/snapshot 保持、CleanupFailure Kind=0、Gate=false、previousReprojectCount=1、successful retry | 已通过；保留旧故障为历史记录，不作为当前状态 |
| B8c：Simulation actual-owner/static-immunity late-failure rollback 与 retry | actual Simulation owner / static-immunity registry / world-load recovery path | current run + independent assertions、zero-tick、two-switch runs，DLL 0B052D…981E56，0 warnings/errors、exit 0；PDB map artifact-time 绑定该 DLL/PDB 与 17 项 source Documents | 已通过；主验收 receipt 记录 9 项独立 NPC AI 后续 source drift 在本次运行验收范围外；运行结果仍绑定 0B052D…，不声称当前源码等于编译输入，也不外推到 621C 或其他产物 |
| B9：三项旧声明删除与兼容出口 | ProjectileIdentityIndex / EntityRuntime / domain typed projection owners；旧声明 registration/config/literal surfaces | `old-identity-literal-exit-independent-source-audit.evidence.json` 明确排除只读 `src/NSSLC.Infrastructure/分类参考/`；旧 identity 后七 mode DLL 5C631F…487B3；current Simulation DLL 0B052D…981E56 | 三项生产声明删除门禁通过；主 receipt 所引 final source-exit summary 未记录该排除字段，故 0-match 结论不得扩大到整棵 `src`。Calendar.EntityId 等不同领域投影合法保留；当前文档 checker 待主验收重跑 |
