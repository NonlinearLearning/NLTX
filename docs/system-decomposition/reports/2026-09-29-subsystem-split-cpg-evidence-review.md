# 历史子系统拆分的 CPG 证据复核

审查日期：2026-09-29  
审查状态：`partial`  
复核边界：`LiquidSimulation` 与 `SpatialSimulation`

## 1. 结论

`LiquidSimulation` 从 `SpatialSimulation` 拆出，作为能力边界是**合理的，局部证据支持但闭包仍为 partial**。液体求解器维护独立队列并执行液体状态转换和 Tile 写入；空间碰撞代码读取相同的 Tile 液体事实，计算接触与移动结果。共享 Tile 状态不要求共享行为 owner。

这是对液体/空间边界的定向复核，不能据此批准历史清单中的全部 32 个子系统边界。字段/属性报告适合作为成员声明库存，但不能单独证明 System owner、唯一写入者、运行时入口闭包或函数行为。旧报告的网络描述也需要更精确：液体复制既有 `NetMessage` 路径，也有独立的 `NetLiquidModule` 批量路径。

## 2. 范围

本次读取的文档：

- [Version4 权威子系统全量审查报告（2026-09-05）](../../component-decomposition/baseline/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md)，重点复核 `LiquidSimulation` 和 `SpatialSimulation`。
- [字段和属性按系统及子系统拆分报告](../../migration/ledgers/Version4字段属性按系统子系统拆分报告-去除ID类文件.md)，重点复核两个子系统的字段/属性库存。
- [子系统索引](../../migration/ledgers/Version4子系统索引.json)与[源码覆盖表](../../migration/ledgers/Version4源码覆盖.tsv)。

`docs/system-decomposition/reports/` 之前没有报告。因此，本报告复核已有的基线拆分和派生字段/属性库存；未审查所有权威或非权威分区、其他历史子系统边界，也未核验 NLTX 中的迁移行为。

## 3. 证据身份

只读 CPG API 使用以下数据根目录初始化：

```text
D:\TRbackup\Version4-cpg-export\out-dop8-interproc
```

manifest 报告 schema `1`、状态 `complete`、967 个 shard 和 1,317 条诊断。CPG artifact manifest SHA-256 为 `6D6BDF09A70E7B9EC3E22E5C4F32AA8D5F447F1A9BDE5CD444B9D427A715A364`；project fingerprint 为 `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`。`SourceSnapshotId` 缺失，因此 CPG 身份没有绑定到特定源码快照。

`Get-CpgSourceExcerpt` 返回了 `UpdateLiquid` 附近的源码文本，状态为 `unknown`，源码 SHA-256 为 `23C27E5B669B99FE225ECFACEBD6F5254A2BA63239B6906CEF2C070010E709B6`。直接计算 `D:\TRbackup\Version4\Terraria\Liquid.cs` 的哈希得到相同值。这确认了本次读取的文件内容，但不能补足 CPG manifest 缺少的源码绑定。

## 4. CPG 查询结果

以下结果都只覆盖列出的 source shard。`complete` 表示选定 shard 在预算内扫描完毕，不代表调用、别名、副作用或运行时入口闭包完整。

| API 与范围 | 结果 | 可支持的判断 |
| --- | --- | --- |
| `Get-CpgTypeSurface`，`Terraria/Liquid.cs` | `complete`，1 个 shard，46,751,194 字节；43 个直接成员：21 字段、22 方法 | 液体状态与运算方法集中在同一源码类型中。 |
| `Get-CpgTypeSurface`，`Terraria/LiquidBuffer.cs` | `complete`，5 个成员：3 字段，以及 `AddBuffer`、`DelBuffer` | 存在小型队列数据结构和对应的队列操作。 |
| `Get-CpgMemberUses`，`_netChangeSet`；`Liquid.cs`、`Tile.cs`、`Collision.cs`、`WaterfallManager.cs` | `partial`，4/4 个选定 shard，183,306,461 字节，有效并行度 2；4 条使用事实，均在 `Liquid.cs`；2 个别名/accessor 缺口 | 选定的其他 shard 没有增加已解析的使用点；缺口禁止把它解释为无使用或副作用闭包。 |
| `Get-CpgMemberUses`，`LiquidBuffer.numLiquidBuffer`；`LiquidBuffer.cs`、`Liquid.cs`、`WorldFile.cs` | 对 3/3 个选定 shard 为 `complete`，145,236,008 字节，有效并行度 2；15 条使用事实 | 三个文件均有使用事实：`LiquidBuffer.cs` 7 条、`Liquid.cs` 4 条、`WorldFile.cs` 4 条。14 条访问模式为 `Unknown`，1 条为 `Write`；结果不能证明唯一写入者。 |
| `Get-CpgCallableFacts`，`Liquid.UpdateLiquid` | `partial`；55 个操作节点、2 个直接调用目标，缺口为 `CalleeEffectsNotExpanded` | 支持局部控制流/运算表面，但未展开 helper 副作用或跨文件行为。 |
| `Find-CpgCallSites`，`UpdateLiquid`；`WorldFile.cs`、`Liquid.cs` | 对 2/2 个选定 shard 为 `complete`，144,547,146 字节，有效并行度 2；在 `WorldFile.cs` 命中 1 个调用点 | 确认所选加载路径中的调用；不能推出未扫描 shard 中不存在其他调用。 |
| `Find-CpgCallSites`，`NetLiquidModule.CreateAndBroadcastByChunk`；`Liquid.cs` 与模块 shard | 对 2/2 个选定 shard 为 `complete`，49,224,422 字节，有效并行度 2；在 `Liquid.cs` 命中 1 个调用点 | 确认本范围内液体循环向网络模块直接交接。 |
| `Find-CpgCallSites`，`UpdateLiquid`；`WorldGen.cs` | `partial`，0/1 个 shard 扫描；缺口 `ShardTooLarge` | `WorldGen.cs.json` 约 9.3 GB，超过 reader 的 512 MiB 单 shard 硬上限。无调用事实不能作为负向证明。 |
| `Trace-CpgValueFlow`，`wetCounter`，深度 2 | `partial`，无值流结果；缺口 `NoValueFlowEdgeInShard` | 此节点没有可用值流边，必须回源码理解计数计算。 |
| `Get-CpgTypeSurface`，`Terraria.Collision`，`Collision.cs` | `complete`，1 个 shard；59 个直接成员：10 字段、49 方法 | 空间规则表面含 `WetCollision`、`LavaCollision`、`WaterCollision` 和 Tile/坡面碰撞方法。 |

`numLiquidBuffer` 查询只对列出的三个 shard 扫描完整。大部分访问模式为 `Unknown`，符合 API 已说明的限制：`OpChild` 事实并不总能确定操作数是读还是写。

## 5. 源码复核

`D:\TRbackup\Version4\Terraria\Liquid.cs:1015` 的 `UpdateLiquid` 会调整每轮处理预算，执行 `quickFall`/`quickSettle` 分支，消费 `LiquidBuffer`，移除已稳定或过期的队列项，更新 Tile 液体，并批量发布变更坐标。源码中可以看到 `wetCounter`/`cycles` 的分段计算，尽管 `Trace-CpgValueFlow` 没有返回值流边。

字段报告把 24 个字段归到 `LiquidSimulation`：`Liquid.cs` 21 个，`LiquidBuffer.cs` 3 个。源码显示 `LiquidBuffer` 保存队列计数和坐标，`Liquid.UpdateLiquid` 消费该缓冲区。`WorldFile.cs:775` 在报告生成进度时读取 `numLiquidBuffer`，随后调用 `Liquid.UpdateLiquid`。这是跨子系统消费者，不足以说明 `WorldFile` 拥有队列。

在 Version4 源码中搜索到 `Liquid.UpdateLiquid()` 的位置为 `WorldGen.cs:15306`、`:20118`、`:59433` 和 `WorldFile.cs:775`。CPG 只能确认所选 `WorldFile` shard 的调用，无法扫描 `WorldGen.cs`；三个 WorldGen 调用属于手工源码证据。动态调用和实际运行时调度仍未核实。

`Collision.WetCollision`、`LavaCollision` 和 `WaterCollision` 读取 `Main.tile` 的液体量/类型，用于计算潮湿状态、危险接触或移动结果。它们处理空间对象与液体的交互；`Liquid` 求解器则改变液体量/类型并推进液体工作队列。这支持能力拆分，同时也说明 Tile 是共享状态。

网络交接同样跨越明确边界。`Liquid.NetSendLiquid` 将打包坐标加入 `_netChangeSet`；`UpdateLiquid` 将批次交给 `NetLiquidModule.CreateAndBroadcastByChunk`。`NetLiquidModule.SerializeForPlayer` 按客户端可见 chunk 分组变更坐标，并把 Tile 液体量和类型写入 `NetPacket`。`NetMessage.cs` 仍在其他包路径中序列化液体，因此旧报告提到它并非全错；但它没有单独描述 `UpdateLiquid` 的批量发布路径。

## 6. 评审发现

### F1. 保留 Liquid 与 Spatial 的能力拆分

**判断：支持，闭包仍为 partial。** 液体子系统有独立可变队列、求解计数、批量提交/发布路径，以及世界生成/加载入口。空间碰撞方法读取液体事实以计算接触和移动。共享 Tile 组件或数据模型，不等于必须共享 System owner。

建议将边界表述为“液体流动/反应状态转换”与“空间接触/碰撞行为”。网络应作为外发适配/投影协作，世界生成与加载应作为调用者或编排方，不应因此并入求解器的状态 owner。

### F2. 字段/属性报告应继续定位为成员库存

**判断：符合其声明用途。** 报告说明它按每个文件的 `primary_owner` 分组声明，保留成员行，且不证明迁移或行为等价。`7,201` 是成员库存总数，不包括方法体，也没有建立写入闭包。CPG 查询与源码复核确认，函数和使用点需要单独分析。

子系统索引把 `SpatialSimulation`、`WorldStorage`、`ExternalBoundaries` 列为 `LiquidSimulation` 的相关子系统，但字段报告中 Liquid 的“跨域关联”显示为空。这不一定是数据矛盾，因为字段报告只复制源码覆盖表里的文件级关系；不过读者可能因此忽略子系统级关系。建议保留唯一 primary owner 和原成员计数，同时另列来自子系统索引的相关子系统，不参与重复计数。

### F3. 细化历史网络描述

**判断：需要澄清。** 旧拆分报告写明 `NetMessage.cs` 负责液体序列化路径。当前源码显示 `UpdateLiquid` 批次通过 `NetLiquidModule` 发布；其他液体序列化仍在 `NetMessage`。后续文档应分别标出两条路径，并指出网络模块是批量发布的交接点。

### F4. 不将单个样本升级为全量拆分验收

**判断：复核范围之外为 unknown。** CPG 未扫描 9.3 GB 的 `WorldGen` shard；值流以及别名/accessor 副作用仍不完整；manifest 没有源码快照 ID。其他历史拆分，尤其 `SharedRuntimeMechanisms` 这类成员数和文件数都较大的分组，需要分别抽取状态、函数、使用点和调用者进行核对，之后才能给出全局判断。

## 7. 后续建议

1. 保留 `LiquidSimulation` 与 `SpatialSimulation` 的独立候选边界，并明确记录共享 Tile 状态、求解 API 和网络交接。
2. 在后续子系统文档中区分两条液体复制路径；字段库存的 primary-owner 规则与消费者/相关子系统关系分开表示。
3. 在宣布历史拆分整体合理之前，对其他高风险边界执行同样的限范围 API 查询和源码回读。超大 shard 未扫描或查询零命中，都不能写成“没有依赖”。

本次没有修改源码、CPG 数据、查询脚本或历史拆分文档。未运行构建或测试；本报告是静态证据复核，不是实现验证。
