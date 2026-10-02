# P09 玩家库存、装备、Buff 与防御装载 System 执行计划

documentKind: system-execution-plan  
partitionId: P09  
taskId: AUTH-SYS-P09  
derivedFromDesign: D:\TRbackup\NLTX\docs\system-decomposition\design\2026-09-30-version4-P09-player-inventory-equipment-system-design.md  
derivedFromSystemReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P09-player-inventory-equipment.md  
sourceReportSessionId: 811f7b34369747f088123f4f0224b106  
claimInputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P09-Player-Inventory-Equipment.md  
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P09-player-inventory-equipment.md  
settledSessionId: 811f7b34369747f088123f4f0224b106  
targetSource: D:\TRbackup\Version4  
fullReferenceSource: D:\TRbackup\无任何删减通过编译  
migrationRoot: D:\TRbackup\NLTX\src\NSSLC  
executionStatus: partial-loadout-visibility-network-composition  
implementationStatus: partial-p09-core  
verificationStatus: partial-local-verifier  
sourceModified: true  
sourceChangeContext: mixed-existing-and-current-p09-staged-verifier  
existingSourceChangesObserved: true  
migrationStatus: not-claimed  
buildRun: true  
verifierRun: true  
buildRunScope: through-staged-packet-147  
verifierRunScope: through-staged-packet-147  
stagedPacket147BuildRun: true  
stagedPacket147VerifierRun: true  
testsRun: false  
runnerSettlement: completed  

## 1. 执行边界

本文是 P09 System 拆分的分阶段执行计划，并记录工作树中已经出现的局部库存 pickup composition。候选迁移路径限定在 `src/NSSLC`；后续仍需把 `PlayerBuffResourceSystem`、`PlayerTickCoordinator`、loadout 网络入口和其他 owner 接入真实入口。本次已补充 staged packet-147 的 header/mask 截断与完整包核心路径，并完成受影响项目的串行 build 与 focused verifier；不运行全量测试，也不证明行为等价或迁移成功。

输入 `outputReport` 按静态报告合同继续保持 `designStatus: proposed`、`verificationStatus: not-run`、`sourceModified: false` 和 `migrationStatus: not-claimed`。本文件记录的 `sourceModified: true` 表示工作树已有 P09 代码并在本次补充 staged packet-147 核心断言；不把局部 API、局部 build/verifier 或静态关系升级为 verified/success。真实入口、跨域 adapter、调度、网络、保存和生命周期仍按 `unknown` 或 `crossSubsystemOwner: integration-review` 处理。

P09 runner session `811f7b34369747f088123f4f0224b106` 已在 task ledger 中完成并释放 lease。本文仅以 `sourceReportSessionId` 保存来源关系，不冒称本文件由该 session claim，也不重复调用 `Complete`。若未来需要重新生成 runner `outputReport`，必须创建新的合法 claim 并使用该次 claim 返回的原始 sessionId 结算，不能复用已完成 ID。

## 2. 目标路径与来源冻结

实现授权后，代码候选范围限定于 `D:\TRbackup\NLTX\src\NSSLC` 的既有领域目录，优先复用 `Component/Player`、`Component/Items`、`Component/Combat` 和其他已有 owner；不按本计划预建空目录。每次新增或移动公开类型前，记录 source path、target path、namespace、依赖影响和回滚点。

来源优先级固定为：

1. `D:\TRbackup\Version4` 的实际源码和经查询 API 返回的关系；
2. CPG manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364` 的只读查询；
3. `D:\TRbackup\无任何删减通过编译` 的完整保存/调用补证，始终标记 partial；
4. SS14 的 System/Component/API 组织参考，不能替代 Terraria 行为 oracle。

CPG 结果必须随 source/target、selected scope、Status、Coverage、Gaps 保存；`partial`、`unknown` 或 `NoMatchingFactInScannedScope` 不能转写为“没有调用”或“没有写入”。

完整参考的 Terraria/Player.cs 与目标文件指纹不同（目标 634,359 bytes，完整参考 1,491,040 bytes）；MessageBuffer.cs 也不同。执行时必须先以目标源码和查询 API 定义行为，再把完整参考中的额外保存、重载和调用点登记为 partial 补证，不能把完整参考直接当作目标版本闭包。

## 3. 阶段门禁

| Gate | 目标 | 进入条件 | 退出条件 | 当前状态 |
| --- | --- | --- | --- | --- |
| G0 来源冻结 | 固定 Version4、CPG、完整参考和 NSSLC 快照 | 计划被批准 | 关键方法、调用点、版本差异和未知项登记完成 | partial |
| G1 Owner/整合评审 | 决定唯一 writer、ID、snapshot、事件和 phase | G0 差异清单可审查 | Items、Container、Combat、Environment、Animation、Network、Persistence 及相邻分区完成 integration review | 未完成，阻塞跨域切片 |
| G2 行为 oracle | 固定旧入口 observation vector | G0/G1 对应 owner 已明确 | 能区分 accepted/rejected、state delta、顺序、副作用和错误 | 未开始 |
| G3 API/状态合同 | 冻结 Query、Command、Projection、Adapter 和 commit result | G2 覆盖目标切片 | 新旧 API 组合可审查，无隐式 writer | 未开始 |
| G4 分片接线 | 按垂直切片接入新 owner | G3 通过且有实现授权 | 真实入口到达新 owner，旧路径不双写 | 局部库存组合已存在；真实入口未接 |
| G5 行为验收 | 运行真实迁移项目所需验证 | G4 所有依赖切片完成 | 观察向量由新 owner/API composition 通过 | 约 10% 局部 verifier 通过；真实入口与全量未运行 |
| G6 兼容移除 | 删除旧字段、重复 Component 或 facade | G5 通过且整合 owner 批准 | 网络、存档、恢复、卸载和旧入口均有替代证据 | 禁止提前执行 |

任何 gate 出现第二个 authority writer、无法解释的顺序变化、协议差异或 source/reference 冲突时，停止依赖该 gate 的切片并记录 blocking decision；不以双写掩盖差异。

## 4. 分片执行顺序

切片顺序是依赖计划，不等于运行时 scheduler 顺序；最终 phase 必须由 G1/G2 固定。

| Slice | 目标 | 前置阻塞 | 计划动作 |
| --- | --- | --- | --- |
| P09-S01 Inventory commit | `GetItem`、`FillAmmo`、`DoCoins`、trash、void-vault | Item payload、Container transfer、logger/effect retry 未闭合 | 先冻结 59 槽和 54–57 边界，再把资格 Query、一次 commit 和 effect intents 分开；失败不部分写 |
| P09-S02 Container relation | bank/bank2/bank3/bank4、`inventoryChestStack`、void capability | Chest 内容/容量/世界锚点/持久化 ID owner 未定 | 只迁移玩家关系和 capability；Container contents 继续走显式 port，不复制到 Player authority |
| P09-S03 Equipment commit | armor/dye/miscEquips/miscDyes | Items EquipmentComponent、P08/P11 effect consumer、网络/保存字段未定 | 冻结 20/10/5/5 长度和 slot relation；唯一 writer 提交后发布 effect invalidation |
| P09-S04 Buff/resource | buff pair、immunity、breath/lava | Combat status、Environment capability、Buff definition writer 未定 | 先建立 44 槽 pairing 和 reset 顺序，再接 capability snapshot；禁止把 immunity 与 cooldown 合并 |
| P09-S05 Loadout/appearance | `TrySwitchingLoadout`、MessageBuffer case 147、10-bit accessory visibility、hide/voice/frame seams | MessageBuffer authority、Animation/P11 owner、dead/local semantics需确认 | 保持 validate -> current swap -> target swap -> index assignment；再按源码读取顺序展开 visibility mask；当前组合在 rejected switch 后仍应用 mask，真实协议语义仍需接线验收 |
| P09-S06 Projection/effects | `UpdateDyes`、visible selection、colors、held/effect state | Item metadata、renderer、mount、Animation 消费 closure 未定 | 只读 committed snapshot；projection 不反写 equipment/dye/loadout |
| P09-S07 Defense interaction | shield/parry、interaction lock、hover handoff | Combat cooldown、World chest、Item logger owner 未定 | 只提交 P09 局部状态；Combat arrays和World relation通过明确 seam交接 |
| P09-S08 Network/persistence/clone | MessageBuffer、NetMessage、Save/Load、SerializedClone | 协议、schema、stream isolation、重试/恢复 unknown | 最后接 adapter；parse/validate 先于 commit，adapter 不直接写 authority |

## 4A. 工作树观察（不构成执行结果）

以下记录工作树中可见的局部边界、已实现的隔离组合和仍待验证的真实入口；局部实现与 verifier 结果不表示生产接线或完整迁移：

| 项目 | 观察结果 | 状态 |
| --- | --- | --- |
| authority 候选 | `PlayerInventorySlotsComponent` 保存 59 个主槽、marker 和 trash 引用 | `partial`；全树唯一 writer 未证明 |
| Query/Command/Port | `PlayerInventoryCommitCommand` -> `IPlayerInventoryItemQuery`/`PlayerItemSpaceQuery` -> `PlayerInventoryCommitPlan` -> `IPlayerInventoryCommitPort` | `partial`；局部 commit composition 可见，仍无真实入口接线 |
| 库存/coin merge 核心 | 空槽逆向 0–49、堆叠容量、unique、ammo 54–57、coin 50–53、VoidVault fallback、拒绝保护和重复 command | `partial-local-verifier`；真实 Item/Container/effect 仍未闭合 |
| pickup/effect composition | `PlayerInventoryPickupSystem` 组合 commit、coin merge 和 effect intents；`CanGoIntoVoidVault: false` 阻止隐式 fallback | `partial-local-verifier`；未接 Version4 world-item/GetItem 入口，effect failure 的回滚/重试为 `unknown` |
| loadout 核心 | 校验、20/10/10 快照交换、index 最后提交、重复命令和 local blocked 分支 | `partial-local-verifier`；未接真实 MessageBuffer，协议/权限/重放和完整行为仍未闭合 |
| visibility/network composition | packet/authority player index、loadout switch、16-bit mask -> 10-bit visibility、rejected switch 后 mask、重复 command、decode-only、header/mask truncated packet、完整 staged packet | `partial-local-verifier`；局部路径已通过，真实 MessageBuffer authority、响应发送和 production scheduler 仍未接入 |
| equipment commit 核心 | armor/dye/misc equipment/misc dye 的 20/10/5/5 槽位、revision、expected current item、跨槽重复装备拒绝和 effect 标记 | `proposed/unknown`；真实 Item payload、网络/保存、effect rebuild 和全树唯一 writer 未闭合 |
| equipment projection/effect 核心 | dye 顺序、robe legs 覆盖、hidden/wing 例外、shield fallback、vanity 覆盖和 effect reset/rebuild | `proposed/unknown`；ArmorID 上界、完整 `UpdateArmorSets`、Item adapter 和 renderer consumer 未闭合 |
| relation/Buff 核心 | bank 关系、VoidVault capability、非法状态、Buff immunity reset 和 lifecycle 清理 | `proposed/unknown`；bank 合法性、容器内容和 UpdateBuffs effect closure 未闭合 |
| buff resource 核心 | breath/lava rebuild、dry recovery、wet opacity、tick/lifecycle reset | `proposed/unknown`；环境碰撞、伤害、装备定义和真实 scheduler 未接入 |
| defense/tick coordinator 核心 | shield/parry、raise/release 的 `15/20` 输出、raise 时 item timer reset、3 tick tile lock，以及 reset -> projection -> buff -> effect -> defense 编排 | `proposed/unknown`；`shieldParryTimeLeft` 逐 tick 演进尚未由目标源码/查询闭合，输入资格、Combat/World handoff 和真实 scheduler 未接入 |
| resource coordinator 组合 | 显式 `PlayerBuffResourceRebuildInput`/`PlayerBuffResourceTickInput` 重载；rebuild 失败不 advance；旧 `Tick` API 保持兼容 | `proposed/unknown`；资源定义、Environment collision、Combat capability 和真实 scheduler 未接入 |
| 未覆盖决策 | 真实 `GetItem`/MessageBuffer 入口、Item/Container adapter、effect ports、网络/保存/clone、scheduler/lifecycle | `unknown`/`not-run` |

这些静态观察不能支持“P09 已实现”“行为等价”或“迁移成功”。

## 5. 每个切片的执行规则

1. 先做全树 writer/readers 搜索和 CPG 查询，再决定是否新增 System 类型；命名或文件存在不能代替 owner 证据。
2. Component 只保存持续状态，Command 表达一次性意图，Query 不得 lazy 写入，Projection 只读 authority，Adapter 承担外部副作用。
3. 先接一条真实入口到唯一 writer，再迁移 readers；legacy facade 只能受控转发或只读，不能与新 owner 并行写同一 invariant。
4. 每个 commit 返回 accepted/rejected、state delta、remaining item 或 error reason；重复、取消、超时和 effect failure 未定义前保持 unknown。
5. 保存、网络、日志、音频、PopupText、Achievements、BinaryReader/Writer、MemoryStream 和 renderer sink 不进入纯 ECS Query。
6. 每个切片完成后记录 source/target、依赖、观察向量、回滚点和未闭合边，再进入下一切片。

### 5.1 P09-S01 后续执行清单

1. 先做全树 writer/readers review，确认 `PlayerInventoryComponent`、`PlayerInventoryState`、`PlayerEquipmentComponent`、`PlayerEquipmentState` 与候选 authority 不会双写。
2. 再冻结库存/coin merge 的真实 Item payload、VoidVault transfer、失败/重试/回滚和 effect 顺序合同。
3. 再接入 loadout 的真实 MessageBuffer command、网络回写和 Item/effect invalidation。
4. 再接入 bank/VoidVault 关系、Container owner、UpdateBuffs effect rebuild、Environment/Combat capability。
5. 在接入 bank relation 前，必须从 Version4 bank 创建/打开路径确认 `IsAvailable=true` 且 relation 为空是否是合法中间态；当前局部 System 对此保持 `unknown`。
6. 只有真实入口、必要副作用和观察向量闭合后，才进入 P09 行为验收；既有 focused verifier 只覆盖局部组合，不能升级 P09 状态。

### 5.2 P09-S05 Loadout/visibility 执行清单

1. 以 `D:\TRbackup\Version4\Terraria\MessageBuffer.cs` 的入站片段和只读 CPG confirmed call-site 为入口，记录 player byte、loadout byte、`TrySwitchingLoadout` 和后续 16-bit mask 的真实读取顺序。
2. 先冻结 `PlayerLoadoutSystem.Switch` 的拒绝条件、当前/目标 `Swap` 顺序和 `CurrentLoadoutIndex` 最后提交；不得让 network Adapter 绕过该 owner。
3. 单独实现或评审 `PlayerAccessoryVisibilitySystem` 的 10-bit mask 展开，输入必须是显式 mask，输出必须是固定长度的 visibility facts。
4. 用旧入口 observation vector 区分 accepted switch、rejected switch、无效 player/loadout、重复包和重复 command；当前组合选择 rejected switch 后仍应用 visibility mask，局部 verifier 已覆盖该组合，接入真实 MessageBuffer 后仍需核对权限、重放和响应语义。
5. 在真实网络入口接线前，不得把局部 loadout verifier 或 mask 单元观察写成网络协议兼容、行为等价或迁移成功。

6. 使用 `PlayerLoadoutPacket147Adapter.Decode` 解码 player byte、loadout byte、little-endian `UInt16` mask；截断输入返回显式 decode failure，不生成半成品 request。
7. 使用 `PlayerLoadoutNetworkSystem.ProcessPacket147` 记录 staged 路径：header 截断不执行 switch；header 成功但 mask 截断时保留已返回的 loadout result，不提交 visibility，并返回 `VisibilityMaskTruncated`。
8. decode-before-commit 与目标源码的 switch-before-mask 异常时序存在差异；在 G4 真实接线前必须由协议 owner 选择 staged decoder 或批准兼容变更，不能把任一路径的局部截断结果升级为旧行为等价。

## 5A. 工作树与验证状态

工作树已经包含库存 pickup、coin merge、equipment、Buff/resource、loadout、visibility/network composition、packet-147 decode/staged process、defense 和 container relation 的局部代码。本次补充了 staged header 截断、mask 截断和完整包的核心断言；受影响项目串行 build 通过（0 warning/0 error），`PlayerItemSpaceVerification` focused verifier 输出 PASS，覆盖约 10% 核心观察向量。上述证据不等于真实 `MessageBuffer` 入口接线，也不等于 P09 全量或迁移成功；全量测试和真实网络/持久化/调度验证仍未运行。`testsRun: false` 表示没有运行全量测试套件。

## 5B. 本次局部验证证据

构建按 `BUILD-CONCURRENCY-1` 通过 `Invoke-SerialDotnet.ps1` 串行执行，命令如下：

```powershell
$sdk=(Resolve-Path 'Build/dotnet-sdk-10.0.400').Path
$env:PATH="$sdk;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  'src/NSSLC/Component/PlayerItemSpaceVerification/Terraria.PlayerItemSpaceVerification.csproj',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

结果：exit code `0`，`0` warnings，`0` errors；产物为 `Build/bin/Terraria.PlayerItemSpaceVerification/Debug/net10.0/Terraria.PlayerItemSpaceVerification.dll`。

核心 verifier 使用已生成产物运行：

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run',
  '--project',
  'src/NSSLC/Component/PlayerItemSpaceVerification/Terraria.PlayerItemSpaceVerification.csproj',
  '--no-build',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

结果：exit code `0`，输出 `PASS: player inventory, equipment, defense, loadout-switch, and container-relation core semantics`。该 verifier 只覆盖约 10% 的核心观察向量；没有运行全量测试、真实 MessageBuffer 接线、协议权限/重放/响应、保存/clone 或跨分区验收。

## 6. 未来验收矩阵

| 能力 | 必测场景 | 通过条件 |
| --- | --- | --- |
| Inventory | 59 槽、favorite/unique、stack/prefix、ammo 54–57、coin merge、void-vault、拒绝和重复 command | slot delta、remaining item、logger/audio/text/post-action 顺序与目标行为一致；无部分写 |
| Container | 四个 bank relation、容量边界、空/失效关系、disconnect/reconnect | 玩家关系不丢失，内容仍由 Container owner 管理，ID 不混用 |
| Equipment | 空槽、重复装备、20/10/5/5 长度、effect invalidation、Item relation | 一个 authority writer，提交原子，Item payload 不被复制 |
| Buff/resource | 44 槽 add/refresh/delete/compact/net apply、immunity reset、breath/lava边界 | type/time 永远成对，reset 顺序稳定，Combat/Environment 无双写 |
| Loadout | invalid/dead/local reject、current/target swap、三套 armor/dye/hide、10-bit visibility mask、重复请求 | 失败无部分交换，index 最后提交，mask 展开顺序和 rejected-switch 语义有明确 oracle，网络结果顺序明确 |
| Projection | robe/vanity/hide、shield fallback、颜色和 visible IDs、projection 前后 authority | 同输入同输出，projection 不回写 authority |
| Defense | shield raise/lower、parry window、tile lock timeout、Combat cooldown handoff | P09 只写自身状态，Combat/World owner 边界可观察 |
| Network | 正常包、坏包、重复/重放、authority、recipient、字段顺序和版本 | parse 先于 commit，坏包不部分写，输出只读 committed snapshot |
| Persistence/clone | save/reload、server-side branch、完整 P09 字段、stream isolation、失败重试、多 session | schema 和恢复行为有明确 oracle；缺字段不静默丢失 |
| Lifecycle | create/rehydrate、tick barrier、destroy/unload、multi-world、disconnect/reconnect | relation、snapshot、stream 和 effect resources 清理完整 |

本矩阵是未来全量验收设计；focused verifier 已覆盖约 10% 的局部组合，包括 staged packet-147 的正常和截断时序，其余真实入口、协议权限/重放/响应、保存/clone、scheduler、生命周期和跨分区项仍为 `not-run`。只有真实迁移项目的行为测试命中新 owner 和新组合并通过后，才可以讨论 verified 或 migration-success。

## 7. 停工、回滚与交付条件

### 停工条件

- 发现同一 invariant 存在第二个 writer；
- CPG、Version4 源码和完整参考对关键分支冲突且没有权威版本决议；
- network/persistence/lifecycle 需要的 owner 或 schema 仍为 unknown；
- 新 API 只能被 facade 调用，尚未有真实 scheduler/入口接线；
- 观察向量无法区分拒绝、部分提交、effect failure 或重试结果。

### 回滚条件

每个 slice 必须先记录 legacy reader/writer、目标 writer、依赖和删除条件。出现失败时只回退该 slice 的路由，保留已确认的只读 Query 和证据登记；不得通过删除无关文件或重置用户已有修改来“清理”。旧 facade 在 G5 前不得移除。

### 交付条件

当前交付包括本设计文档、本执行文档、`src/NSSLC` 中的 P09 staged packet-147 代码/核心断言和静态证据/缺口登记；本次没有修改原 `outputReport` 的结算字段，也没有运行全量测试。两份文档交付后记录：

- `executionStatus: partial-loadout-visibility-network-composition`；
- `implementationStatus: partial-p09-core`；
- `verificationStatus: partial-local-verifier`；
- `sourceModified: true`（既有代码加当前 P09 staged packet-147 核心断言）；
- `sourceChangeContext: mixed-existing-and-current-p09-staged-verifier`；
- `existingSourceChangesObserved: true`（工作树已有库存切片）；
- `buildRun: true`（受影响项目串行 build 成功，0 warning/0 error）；
- `verifierRun: true`（约 10% focused verifier 通过，包含 staged packet-147 三条路径）；
- `stagedPacket147BuildRun: true`；
- `stagedPacket147VerifierRun: true`；
- `testsRun: false`。

这表示交付的是 proposed 设计、执行计划和局部实现状态登记；不表示真实 API 已完整接线、行为已等价、P09 已完整迁移或 runner 会话需要重新结算。原 `outputReport` 仍为 `proposed / not-run / sourceModified:false / not-claimed`。

## 8. PUA 自检清单

- [x] 已先读 P09 报告、runner/System 契约、仓库约束和 PUA 规则。
- [x] 已使用只读 CPG Query API，并对 partial/零命中保留缺口。
- [x] 已回读 Version4 目标源码和完整参考源码的关键保存、调用和提交路径。
- [x] 已把 SS14 限定为组织/API 参考，没有冒充 Terraria 行为证据。
- [x] 已区分 `confirmed`、`partial`、`unknown`、`proposed` 和 `not-run`。
- [x] 迁移代码限定在 `src/NSSLC`；未修改输入报告、prompt、既有 outputReport 或 runner ledger。
- [x] 本次代码修改限定在 `src/NSSLC`，并同步更新两份文档；没有修改输入报告、prompt、既有 `outputReport` 或 runner 状态。
- [x] 已按串行约束重新 build 受影响项目并运行约 10% focused verifier；没有运行全量测试或全量迁移验收。
- [x] 局部实现和 verifier 结果仍标为 `partial`，未把它们写成 P09 完成、行为等价或迁移成功。
