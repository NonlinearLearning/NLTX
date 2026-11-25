# PlayerCombatResultSystem 是否成立：职责与共享系统重叠判定

```text
文档 ID：DOC-2026-11-24-player-combat-result-system-boundary
逻辑域：reviews
产物类型：audit
状态：concluded（静态检视 + 运行时探针 7/7 PASS；真实服务器整包运行未做）
范围：src/NSSLC/Player/PlayerCombatResultSystem.cs、src/NSSLC/Share/Combat/*
      （CombatDamageRequestSystem、CombatResultDrain、CombatDamageResultQueueComponent）、
      src/NSSLC/Npc/NpcCombatResultSystem.cs、src/NSSLC/Projectile/ProjectileDamageResultSystem.cs、
      src/NSSLC.Server/Registration/{CombatSystemRegistration.cs,PlayerEntityDefinition.cs,
      EcsCoordinationRules.rules}、Build/PlayerCombatResultProbe/
证据入口：见 §2 逐条行号、§3 探针输出、§9 运行记录
canonical 路径：docs/reviews/audits/2026-11-24-player-combat-result-system-boundary.md
```

## 0. 结论

**不合理，且与共享系统在"结果队列生命周期"这一职责上正面重叠。建议删除（选项 A），
不建议保留现状。** 判定分三句：

1. **它是一个空系统**：查询回调只有一条语句，把排空得到的列表整体丢弃；回调参数只有
   `ArchEntity`，没有 `ref` 组件，因此**本系统对任何实体状态都没有写入**。它唯一的实际效果
   是**缩短共享结果队列的存活列表**。
2. **这个唯一效果已经被共享侧覆盖**：`CombatDamageRequestSystem` 每 tick 开始时调用
   `BeginResultTick`，跨 tick 残留一律清空。删除本系统不会让任何结果跨 tick 堆积（§3 已实测）。
3. **它挡在唯一可能的真实消费者前面**：注册顺序把它放在 `PlayerDeathRespawnSystem` 之前，
   而后者声明依赖它（`after`），但运行时既不读队列也不读结果（§3、§4.3）。也就是说，
   真正需要"玩家这次被打成什么样"的那一刻，信息已经被它删掉了。

它唯一能讲得通的角色是"玩家侧结果消费者的占位"。但这与本域自己写下的规则冲突：

> 当前没有为了接口完整性创建空的 `PlayerMovementIntentSystem`、`PlayerCombatSystem` 或
> `PlayerInteractionSystem`。
> —— `src/NSSLC/Player/README.md:35-37`

`PlayerCombatResultSystem` 正是"为了接口完整性创建的空系统"，只是它没有被叫作
`PlayerCombatSystem`。

## 1. 判定口径

"是否合理"按三条同时成立才算：**有本域独有的规则、有本域独有的状态写入、维护一条别人不维护的
不变量**。任何一条都不由"注册了、能跑、不报错"推出。

"是否和共享系统职责重叠"按一件事判断：**两个系统是否在同一个共享可变状态上竞争同一个后置条件**。
后置条件相同即重叠，与它们是否都在 Commit 阶段、是否都叫 Result 无关。

本次不引入新术语，也不改代码：交付为判定与可执行的落地清单。

## 2. 静态事实

| 事实 | 位置 | 说明 |
| --- | --- | --- |
| 查询键是死亡状态组件 | `PlayerCombatResultSystem.cs:17-18` | `WithAll<PlayerDeathStateComponent>()`，不是玩家身份/能力组件 |
| 循环体只有一次丢弃 | `PlayerCombatResultSystem.cs:35` | `_ = CombatResultDrain.Drain(...)` |
| 回调没有 `ref` 参数 | `PlayerCombatResultSystem.cs:33` | `(ArchEntity entity) =>`，无组件写入 |
| 排空语义已下沉共享侧 | `CombatResultDrain.cs:23-31` | 句柄解析 + 一次性取走；本系统只是调用方 |
| 共享侧每 tick 清空结果 | `CombatDamageRequestSystem.cs:30` | `resultQueue.BeginResultTick(context.Tick)` |
| 清空实现 | `CombatDamageResultQueueComponent.cs:31-45` | tick 变化即 `_results.Clear()` |
| 注册无 reason、无依赖边 | `CombatSystemRegistration.cs:113-116` | 与相邻 NPC/投射物结果系统不同，它连"为什么排在这里"都没写 |
| 生死系统声明依赖它 | `CombatSystemRegistration.cs:117-122` | `after: [CombatPlayerResult]`，理由是"observe the player combat result" |
| 生死系统实际不读结果 | `PlayerDeathRespawnSystem.cs:38-41` | 只判 `life.IsAlive`，随后 `death.MarkDead(tick)` |
| 死亡原因通道为空 | `PlayerDeathStateComponent.cs:12,14` | `CauseId` 与 `MarkDead(tick, causeId)` 全仓无调用方传值 |
| 协作规则不覆盖该键 | `EcsCoordinationRules.rules` | 全文无 `combat.player.result`；phase/reason/dependency 三条规则都不约束它 |
| 生产代码不读队列长度 | 全仓 grep `resultQueue.Count` | 零命中；`Count` 只有探针在用 |
| 代码引用只有一个 | 全仓 grep `PlayerCombatResultSystem` | 仅 `CombatSystemRegistration.cs:116`；另有一处注释见下一条 |
| 假不变量已写进注释 | `PlayerEntityDefinition.cs:46-48` | 注释声称不修本系统会让结果"越堆越多"，与 §3 实测相反 |

作为对照，同构的 NPC 消费者做的是真判断：`NpcCombatResultSystem.cs:36-48` 在
`Result.Applied` 时改写 `NpcBehaviorStateComponent` 的决策相位。三处消费者里只有玩家侧没有任何决策。

## 3. 运行时证据

探针 `Build/PlayerCombatResultProbe/`（`PlayerCombatResultProbe.csproj` + `Program.cs`）
使用真实 `Terraria.Components` 类型，7 条断言全部 PASS：

| 检查 | 观测 | 含义 |
| --- | --- | --- |
| `shared-producer-clears-stale-results` | `before=3 after=0` | 共享生产者单独运行即可清掉上一 tick 残留 |
| `no-consumer-no-growth` | `peak=5 final=0 ticks=100` | **完全不放玩家消费者**，100 tick 内队列不增长 |
| `player-consumer-only-dequeues` | `queueBefore=4 queueAfter=0 life=100/100 dead=False` | 唯一效果是出队；生命与死亡状态不变 |
| `match-set-is-death-state-component` | `nonPlayerKept=2`，伪造者被清空 | 匹配集合来自死亡状态组件，不来自玩家身份 |
| `death-respawn-does-not-read-results` | `before=2 after=2` | 生死系统对结果队列零读写，注册理由不成立 |
| `drained-result-cannot-set-death-cause` | `isDead=True causeId=0 cause=PlayerAttack` | 结果在本 tick 可达，但被删除，死亡原因只能为空 |
| `queue-count-not-observed` | `count=0` | 队列长度没有生产读者，清空与否不可观察 |

`no-consumer-no-growth` 是本次判定的关键一条：它把"删掉会不会泄漏"从推理变成了观测。
配合 §2 的"代码引用只有一个"，删掉它的可观察差异为零。

## 4. 与共享系统的职责重叠（逐项）

### 4.1 与 `CombatDamageRequestSystem`（共享生产者）：重叠，且是本系统唯一的实际作用

后置条件完全相同——"上一 tick 的结果不留在队列里"。共享侧在 `CombatDamageRequestSystem.cs:30`
每个 tick 无条件执行，不依赖任何玩家实体是否存在、是否挂载死亡状态组件、是否被注册。
本系统是这条后置条件的**冗余第二实现**。区别只在于：共享侧按 tick 生效（最迟下一 tick），
本系统按实体生效（同 tick 内）。

### 4.2 与 `CombatResultDrain`（共享排空规则）：语义已全部下沉，本系统只剩转发

2026-11-02 审计 17.2 节把"先解析句柄、再取走该身份全部结果、解析不到返回空"抽到了
`Share/Combat/System/CombatResultDrain.cs`。抽取之后，玩家侧剩下的三条语句没有一条是本域规则：
取队列、遍历实体、调用共享函数并丢弃返回值。**共享侧拥有规则，玩家侧拥有一次调用。**

### 4.3 与 `PlayerDeathRespawnSystem`（同域）：同一"战斗后果"槽位的双通道，且依赖边是假的

玩家"被打之后要发生什么"目前由两套通道表达：

- **在用的一套**：`DamageCommitSystem` 写 `LifeComponent` → `PlayerDeathRespawnSystem` 读
  `life.IsAlive` 判定死亡。权威、单一、无中间载体。
- **空转的一套**：Combat 结果队列 → `PlayerCombatResultSystem` → 丢弃。

注册上还有一条 `CombatPlayerDeathRespawn after CombatPlayerResult`，理由写的是
"Death and respawn observe the player combat result for this tick."。运行时反证：给队列放入
2 条玩家结果后单独跑生死系统，队列仍是 2 条（§3 第 5 条）。**这条边不是数据依赖，是叙述。**

后果不只是"多一条边"：它把本系统固定在前、生死系统固定在后。将来若真要让死亡带上
`CauseId`，必须反过来把生死系统排到排空之前——也就是说，为了这个占位系统，真实需求一出现
就要改注册顺序。

### 4.4 与 `NpcCombatResultSystem`、`ProjectileDamageResultSystem`：按 source 分区，不重叠

三个消费者读同一个组件，但 `ConsumeResults(EntityRuntimeHandle source)` 只摘除
`Request.Source` 匹配的条目（`CombatDamageResultQueueComponent.cs:47-67`），因此：
一个 source 的结果不会被两个消费者看到，NPC 与投射物的结果也不会被玩家系统吃掉（§3 第 4 条）。

所以**"结果消费"这一职责本身不重叠**；重叠的是**队列生命周期**（4.1）。但这个"不重叠"反过来
说明：本系统在自己的分区里既不消费也不产出，占着分区却没有分区内的规则。

### 4.5 附：它存在的唯一书面理由已被证伪

2026-11-02 审计 25.2 节写过：不跑本系统会让"玩家伤害结果永不排空——对局越久堆得越多"，并据此
在 25.3 节给玩家实体补挂死亡/重生组件。这条理由在今天的代码上不成立，而当时也不成立：
`git show 7e016cb^:src/NSSLC/Components/Share/Combat/CombatDamageRequestSystem.cs` 里已经有
`resultQueue.BeginResultTick(context.Tick)`，同一个父提交里的队列组件已经有
`_resultsTick` 清空逻辑。也就是说，那次"接线修复"修的是 `PlayerDeathRespawnSystem`（真实缺口），
顺带把一个**不需要修的队列**也归因成了缺口。

这条假不变量还留在了生产代码的注释里：`PlayerEntityDefinition.cs:46-48` 至今写着"后者永不排空
`CombatDamageResultQueue`，对局越久玩家伤害结果堆积越多"。注释不改不影响运行，但它会让下一个
读代码的人继续把"必须排空"当成前提。

这不是要翻旧账：它解释了为什么一个空系统会被当成"必须存在"——它背上了一条不存在的不变量。

## 5. 匹配键：把死亡簿记组件当玩家身份

`WithAll<PlayerDeathStateComponent>()` 在语义上表达的是"**有死亡簿记的实体**"，不是"玩家"。
两条推论都由探针证实（§3 第 4 条）：

- **过宽**：只挂 `PlayerDeathStateComponent`、既无生命也无身份的实体会被匹配，其名下的结果被
  静默丢弃。
- **过窄**：有生命、有运行时句柄、但没有死亡簿记的实体会被跳过；其名下的结果留到下一次 tick 清空。

今天没有导致缺陷：玩家实体只有一条创建路径（`PlayerEntityDefinition`，经
`Program.cs:759` / `794` 的 `entitySpawner.Spawn(PlayerEntityDefinition.Id)`），该定义必然挂这两项。
但这是**把排空的正确性绑在另一件事的组件组合上**：任何新增的玩家形态（旁观者、后续的客户端镜像实体、
分阶段装配）只要少挂一个死亡簿记，排空就静默失效——而失效没有任何可观察信号（§3 第 7 条）。

按 [ECS 文件组织设计约束](../../../Context/架构约束/ECS文件组织设计约束.md) §4，用"能力"组件做匹配键是
本仓的既定做法；死亡状态是**状态**，不是能力。

## 6. 备选方案对照

| 方案 | 内容 | 代价 | 适用前提 |
| --- | --- | --- | --- |
| **A 删除（推荐）** | 删系统与其注册；`PlayerDeathRespawn` 去掉假依赖边与假 reason | 一处注册、一个文件、一个未用的注册键；可观察行为零变化 | 玩家侧不需要"逐条命中反馈"这类结果级数据 |
| B 补齐为真实消费者 | 在排空时把 `Applied` 结果写成玩家事实（`CauseId`、受击方向、伤害数字），并排到生死系统之前 | 需要新的玩家事实载体；要把注册顺序与 `after` 边反过来；死亡原因才第一次非空 | 明确要做受击反馈/死亡原因 |
| C 保留现状 | 什么都不做 | 每 tick 一次全玩家查询 + N 次句柄解析，换零语义；继续占住结果队列的前置位置 | 无；若只为"对称"则不成立 |

**B 的最强理由**是"死亡原因"：`PlayerDeathStateComponent.CauseId` 与 `MarkDead(tick, causeId)`
已经存在，构造上也确实需要结果里的 `Request.Cause`。但 B 不是"保留现状"，它是**反向重排 + 新写入**，
比 A 大得多；而且它一旦成立，本系统的名字、查询键、位置全都要改。所以 A 与 B 是二选一，
"先留着以后再说"（C）两头都不占。

建议 **A**：在不清楚是否有受击反馈需求前，保留一个会吞掉数据的占位比不保留更贵——
它把"以后要不要读结果"变成"先改掉这个系统"。

## 7. 落地清单（执行 A 时）

| 步骤 | 目标 | 备注 |
| --- | --- | --- |
| 1 | 删除 `src/NSSLC/Player/PlayerCombatResultSystem.cs` | 唯一代码引用在 `CombatSystemRegistration.cs:116` |
| 1b | 修正 `PlayerEntityDefinition.cs:46-48` 的注释 | 它陈述的"结果越堆越多"已被 §3 证伪，留着会继续误导 |
| 2 | 删除 `CombatSystemRegistration.cs:113-116` 的注册块 | Commit 阶段无其它键依赖它 |
| 3 | `CombatPlayerDeathRespawn` 去掉 `after` 与 `reason` | 保留原位置即可；顺序不再有语义 |
| 4 | 删除 `EcsServerRegistrationKeys.CombatPlayerResult` | 无其它引用；它本来也不在 `EcsCoordinationRules.rules` 里 |
| 5 | 复核 `CombatResultDrain` 的调用方数量 | 从 2 降到 1，见下 |

第 5 步不阻塞 A：`CombatResultDrain` 仍是 `internal`，单调用方时属于可以保留的说明性转发，
但"抽出它的理由"（两处重复）已经消失。**不建议**在 A 的同一批里内联它——一次只验一件事；
留作下一次触碰该文件时顺带处理，并在提交说明里写明它已退化为单调用方。

A 的验收条件：`Terraria.Components` 与 `NSSLC.Server` 编译 0 错误；`PlayerCombatResultProbe`
的 `no-consumer-no-growth` 与 `death-respawn-does-not-read-results` 仍然 PASS；真实服务器启动到
`ecs.server.ready` 且无异常。

## 8. 未决与证据缺口

- **未决（影响 A/B 选择）**：玩家侧是否需要结果级数据（伤害数字、受击方向、死亡原因、成就统计）。
  本仓目前没有任何消费者，也没有写在计划里的需求；若需求出现，按 B 重新引入，而不是复活 A 删掉的形态。
- **缺口**：未在真实客户端联调中观察玩家受击链路（与 progress 里"原版客户端画面验收未完成"同一条
  长期未闭合），因此"玩家侧结果永远无人读"只有静态与单进程探针两级证据，没有客户端侧反证。
- **缺口**：`Build/playerprobe` 等历史探针已随目录重构失效（引用旧命名空间），本次没有复用它；
  `PlayerCombatResultProbe` 是新建的最小探针。
- **证据状态**：§2 静态事实为 confirmed；§3 七条为已运行且 PASS；真实服务器整包运行未做，
  故整体结论对"删除"是**建议**，不是"已迁移"。

## 9. 验证记录

工作目录 `D:\TRbackup\NLTX`，.NET SDK 由 `global.json` 选定，目标框架 `net10.0`。

```text
dotnet build Build/PlayerCombatResultProbe/PlayerCombatResultProbe.csproj --nologo --verbosity minimal
  退出码 0；0 警告 0 错误
  产物 Build/bin/PlayerCombatResultProbe/Debug/net10.0/PlayerCombatResultProbe.dll

dotnet run --project Build/PlayerCombatResultProbe/PlayerCombatResultProbe.csproj --no-build --no-restore
  退出码 0；7 PASS / 0 FAIL；末行 CONCLUSION PASS
```

探针是只读验证程序：只创建自己的 `ArchWorld`，不写仓库文件、不连网络、不改生产注册。
