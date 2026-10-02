# P11 玩家表现与派生视图 System 设计

- 文档状态：`proposed`
- 实现状态：`partial`
- 验证状态：`partial`（仅 overhead message/composite arm focused verifier）
- 分区：权威 P11，PlayerPresentationDerived
- 输入报告：[P11 System 拆分报告](../reports/2026-09-18-system-decomposition-authoritative-P11-player-presentation-derived.md)
- 配套执行文档：[P11 执行流程与迁移计划](../../architecture/execution/2026-09-30-authoritative-P11-player-presentation-derived-execution.md)

## 定位与范围

本设计覆盖输入报告中 18 个 P11 leaf group，共 91 个字段与 73 个属性。它是静态边界提案，不代表已迁移、已注册或行为等价。P11 inventory 约束本报告的成员范围；P01-P10、P12-P20 的状态所有者仍由对应分区或 integration-review 确认。

设计目标是保留 Player 表现与派生读取的可观察行为，同时让可独立维护的不变量有明确状态所有者。拆分单位是行为与写入协议，不按 18 个 inventory group 一组建一个 System。

不在本次设计中确认：生产 System 注册入口、全量调用闭包、完整调度顺序、唯一网络/环境/存档 owner、多人世界隔离、Version4 绘制消费者闭包，以及任何迁移完成状态。

## 证据基线

来源优先级：Version4 源码是目标行为事实；完整参考目录只用于补充同名同签名实现或 Version4 空体缺口。参考代码不能覆盖 Version4 源码，也不能单独证明迁移目标中的运行时关系。

| 来源 | 身份 | 用途与限制 |
| --- | --- | --- |
| Version4 | `D:/TRbackup/Version4`；`Terraria/Player.cs` SHA-256 `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`；`Terraria.GameContent/PlayerEyeHelper.cs` SHA-256 `0353B2AEF705FB3065674D95282132F2B4FDE331EB78E71096F4D5ABE9154E80` | P11 目标源码；已在 P11 输入报告及本次关键路径复读。未取得 Git revision。 |
| 完整参考 | `D:/TRbackup/无任何删减通过编译`；`Terraria/Player.cs` SHA-256 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`；眼睛和绘制文件哈希见下 | 用于比对眼睛状态机、绘制消费者及存档实现。该目录不是 Git 工作树；本次没有运行其构建，目录名不是本次验证证据。 |
| Version4 CPG | `D:/TRbackup/Version4-cpg-export/out-dop8-interproc.sqlite`；manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；project fingerprint `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`；967 shards | 通过 `.agents/skills/ecs-system/tools/CpgEvidence.ps1` 只读 Query API 查询。`SourceSnapshotId=null`，索引没有与本次源码快照绑定。 |
| NLTX 当前代码 | `D:/TRbackup/NLTX/src/NSSLC/Component/Player` | 可见眼睛动画、相机、外观、消息、阴影/手臂等隔离类型；搜索结果只证明类型和 focused verifier 引用存在，未找到生产注册或旧入口路由闭环。 |
| ECS 结构参考 | `C:/Users/shan/Downloads/ECS/space-station-14-master/Content.Server/Wires/WiresComponent.cs`、`WiresSystem.cs` | 仅用来校准 component state、System update/event 与输出协作的代码组织。没有从 SS14 推断 Terraria 语义或 owner。 |

完整参考中本次检查文件的 SHA-256：

- `Terraria.GameContent/PlayerEyeHelper.cs`: `D6D71E9D8A01CD63291BCADFE6E7142FB64F5C2507F3AF83FD8126C48439B2EA`
- `Terraria.DataStructures/PlayerDrawLayers.cs`: `76843DBC6BB6C13C0D7D004D2A48F4BDB3E358355C51473CE7367EF87678AE3C`
- `Terraria.DataStructures/PlayerDrawHeadLayers.cs`: `64C64C28A1A69294EC15360AFF9F04FD04F6C544197BA8B7B8E6F32246F92BD1`

Version4 与完整参考的 `PlayerEyeHelper.cs` 内容比对仅见文件头 BOM 差异；眼睛状态机在两份文件中相同。Version4 的 `PlayerDrawLayers.cs` 与 `PlayerDrawHeadLayers.cs` 是空类壳，而完整参考包含绘制实现，因此参考只能证明该参考版本怎样读取表现数据，不能证明 Version4 的绘制入口。

Version4 `Player.cs` 的 `Serialize`（约 26418 行）和 `Deserialize`（约 26455 行）为空体；完整参考有实际实现（约 55348、55750 行）。因此本设计不从参考代码直接宣称 Version4 的存档字段、顺序或兼容性。

## 关键路径与查询结果

CPG Query API 查询先解析符号，再按 `Player.cs`、`Main.cs`、`PlayerEyeHelper.cs`、`ChatHelper.cs`、`NetMessage.cs`、`MessageBuffer.cs` 这些选定 source path 查询调用点。查询状态只描述所选索引范围内是否完成；`complete` 不等于全调用闭包，零命中也不等于不存在。

| API 查询 | CPG 结果 | Version4 源码复核 | 设计影响 |
| --- | --- | --- | --- |
| `PlayerEyeHelper.Update(Player)` call-sites | `partial`，0 条 | `Player.cs:15728` 有 `eyeHelper.Update(this)`，在坐姿、睡眠更新之后 | 入口存在；CPG 不足以证明闭包，顺序必须保留。 |
| `BlinkBecausePlayerGotHurt()` call-sites | `complete`，1 条 | `Player.cs:22349` 伤害处理调用 | 支持该静态边；其他伤害入口仍未闭合。 |
| `OverheadMessage.NewMessage(string,int)` call-sites | `complete`，1 条 | `ChatHelper.cs:72` 调用；`Player.cs:14966-14968` 每 tick 递减计时 | 局部入站和计时路径确认，消息投影/渲染消费者未知。 |
| `UpdateSocialShadow()` call-sites | `complete`，2 条 | `Player.cs:15064`、`:21786`、`:21879` 有 3 处调用 | API 与源码数量不符；不得据 CPG 结果宣称闭包。 |
| `UpdateAdvancedShadows()` call-sites | `partial`，0 条 | `Player.cs:17679` 在 Player 更新路径调用 | 入口由源码确认；索引零命中不能当成没有调用。 |
| `SetCompositeArmFront/Back` call-sites | `complete`，分别 1 / 4 条（选中 `Player.cs`） | 去除两个方法声明后，源码有 front 1 处、back 4 处调用 | 此局部调用计数相符；不代表 setter 写入者和绘制消费者闭合。 |
| `UpdateNetOffset(bool,bool)` call-sites | `complete`，1 条 | `Player.cs:3971`、`:17556` 有 2 处调用 | API 结果少于源码；所有 caller 和调度仍 partial。 |

追加的 `Get-CpgMemberUses` 查询在选中 `Player.cs`、`Mount.cs` 和 Version4 `PlayerDrawLayers.cs` 时，对 `shadowPos` 返回 18 个 operation-level uses，访问模式全部为 `Unknown`；`chatOverhead` 在 `Player.cs`、`ChatHelper.cs`、`Main.cs` 范围返回 2 个 `ChatHelper.cs` uses，没有列出 Main reader；`compositeFrontArm` 返回 3 个 uses，其中 1 个被分类为写。结果不构成读写闭包，数组元素写入和索引遗漏仍需回源码核对。

完整参考 `Main.cs:64025-64033` 绘制 overhead 文本并读取消息尺寸、snippets、颜色和剩余时间；`LegacyMultiplayerClosePlayersOverlay.cs:51` 与 `NewMultiplayerClosePlayersOverlay.cs:225` 也按消息/表情计时调整 nameplate。完整参考 `PlayerDrawHeadLayers.cs:401`、`PlayerDrawLayers.cs:2470,2756` 读取眼睛帧，绘制层、`LegacyPlayerRenderer` 与 `LensFlareElement` 读取阴影/手臂相关状态；完整参考 `Mount.cs` 也会改写 social shadow 历史坐标。Version4 对应 draw 文件为空壳且当前 `Main.cs` 未找到 overhead reader，所以这些消费者只能补充参考版本证据，Version4/NLTX 的 renderer route 仍为 `unknown`。

## 行为边界提案

| 行为切片 | 边界决策 | 状态 / API 职责 | 证据状态 |
| --- | --- | --- | --- |
| 眼睛状态、计时与帧选择 | `PlayerEyeAnimationSystem` 作为独立 System 候选 | `PlayerEyeAnimationComponent` 保存 state、time、frame；System 接收不可变输入，负责转换、生成帧和 spawn/removal reset。hurt 是来自伤害 owner 的事件/方法输入。 | 转换逻辑和 Player 调用顺序 confirmed；全调用闭包、生命周期 wiring 与同 tick 先后 partial。 |
| Player pose 与每帧动画 | 保持 partial，不新建总控 System | 输入生产者仍由 movement、gravity、action、animation 等 owner 决定；本分区只定义一致的 frame snapshot 与显式提交边界。 | 成员和代表性写入 confirmed；写入者闭包与 phase order partial。 |
| 网络位移修正与摄像目标同步 | partial，状态与协议分开 | correction owner 更新 `netOffset`；camera query 只读取显式快照；Network Adapter 编解码并在约定发送后更新同步标记。 | 本地算法、部分 NetMessage/MessageBuffer 路径 confirmed；权威端、retry、delivery、同步确认与完整 caller partial。 |
| 阴影缓存、手臂与视觉输入 | partial，缓存/手臂/渲染输出保持可区分 | transient shadow cache 和 composite arm 值按独立不变量提交；Projection 只输出 draw snapshot，不写回 gameplay 或 equipment authority。 | Version4 有写入方法；绘制消费者仅由完整参考补充；重置、renderer closure 和全 producer list partial/unknown。 |
| 外观自定义 | 状态 + 显式修改 API + 边界 Adapter | customization 值由单一 owner 提交；网络/存档 Adapter 转换外部格式；绘制只消费 snapshot。 | Version4 字段及部分网络路径可见；保存格式在该快照 unknown。 |
| 区域标记和派生属性 | Query 与修改职责分开，owner 待 integration-review | 纯 Query 只吃 immutable snapshot；旧 setter 映射为相应环境/能力 owner 的同步写 API 或 Command。 | Zone packed-bit 读写及协议路径可见；zone producer、snapshot 新鲜度和共享 owner partial/unknown。 |
| overhead message | transient state System 候选 + Chat Adapter + 单向 Projection | 支持 replace、tick、reset、immutable draw data；Chat Adapter 负责协议文本与解析边界。手臂不并入消息 owner。 | `NewMessage` 和 timer tick confirmed；解析替换细节已见，消费者和 scope 未闭合。 |

这张表里的 System/Query/Command/Adapter/Projection 是责任候选，不是必须创建的类型清单。只有拥有独立不变量、写集、生命周期或外部效果契约的边界才可晋升为 System。

## API 组合契约

稳定映射采用 `LegacyEntryPoint -> ConceptId -> CompositionId -> CanonicalNewComposition`。API 签名可以变化，但必须同时保留返回值、错误、权威状态变化、事件/网络/渲染效果、顺序、可见时机、作用域和幂等语义。

| ConceptId / 旧入口 | 提议的 canonical composition | 必须保持的语义 / 未决点 |
| --- | --- | --- |
| `Player.EyeAnimation.Tick` / `PlayerEyeHelper.Update` | 建立眼睛输入 snapshot → `PlayerEyeAnimationSystem.Update` → `EyeFrame` snapshot → renderer projection | 状态优先级、frame-before-timer 顺序、sleep timer 覆盖、hurt hold、输入刷新时点和 Player tick 顺序；生产注册与 draw route unknown。 |
| `Player.EyeAnimation.Hurt` / `BlinkBecausePlayerGotHurt` | Damage owner 完成受伤判定 → 发送 hurt presentation event → eye state writer | 是否同 tick 生效、事件次数、death/removal/spawn reset 路径 partial。动画 owner 不取得伤害权威状态。 |
| `Player.Message.Replace` / `NewMessage` | Chat Adapter 形成 message input → message state replace/measure contract → tick advance → read-only projection | legacy 会替换文本、parse snippet、measure 并覆盖 display time；color 在 ChatHelper 后续写入。解析失败语义、显示线程和 renderer consumer unknown。 |
| `Player.Camera.CorrectAndSync` / `UpdateNetOffset`、`Main.UpdateCameraPan` | 明确 correction input → camera state update/query → Network Adapter 条件发送/解码 → 同步确认 | `netOffset` 碰撞输入、reset array scope、client/server authority、ack/retry 和 packet visibility 尚未封闭。 |
| `Player.Shadow.BuildFrame` / `UpdateSocialShadow`、`UpdateAdvancedShadows` | cache update/reset 与 arm state commit → draw snapshot → renderer | 绘制不可回写权威数据；数组长度、环形缓存时序、teleport reset、producer 顺序、无 renderer 路径均待核实。 |
| `Player.Derived.Read` 与旧属性 setter | Query over copied snapshot；setter 映射成所属 owner 的 mutation API/Command | `Male` 写 `skinVariant`；`MountedCenter` setter 可写 position；zone properties 写 packed bits；若 `HeldItem` 返回 live mutable `Item` 或 `SceneMetrics` 未复制，则不可标为纯 Query。 |

默认使用同步方法满足局部协作。只有 Version4 或目标运行机制证明需要 deferred visibility、排队、重试或跨 phase 冲突处理时，才引入 Command buffer、barrier 或额外调度节点。

## 读写与依赖关系

静态依赖提案如下，实线含义由源码确认，虚线含义是接入前置条件而不是已验证调度边：

```text
已提交的 movement / environment / status facts
  ..> EyeInput snapshot ..> EyeAnimationSystem ..> EyeFrame projection
        ^ damage owner --hurt event-->

ChatHelper --message input--> OverheadMessage state
  Player update --tick decrement--> OverheadMessage snapshot ..> draw consumer

movement/collision facts ..> NetworkCorrection state ..> Camera snapshot
  Main camera policy ..> Network Adapter <.. MessageBuffer decoder

shadow / arm producers ..> transient state ..> draw projection
zone & item / mount / interaction owners ..> copied query inputs ..> derived snapshots
```

点线关系不能用文件顺序、System 注册顺序或 CPG 的 `complete` 状态升级成 scheduler DAG。实际阶段、是否并行、读取可见时间和 `World/session` scope 仍需从目标运行时入口与调度器取证。

## 不变量与拒绝方案

- 每一项权威状态变化只有一个提交责任；两个 System 不得无协议写同一状态。
- Query 不懒加载、不修改权威值、不泄露可写对象引用；cache 若影响后续结果，仍需纳入状态 owner。
- Projection 只从已提交事实生成视图，不反向修正玩法或外观状态。
- Adapter 只翻译协议、参数和错误，不复制业务规则或累加不变量。
- packet、save、chat、audio、renderer 的效果不得被遗漏或因为拆分被无记录延迟。
- 不采用一个覆盖所有 P11 状态的 PlayerPresentationSystem；它会复制原 Player 的职责混合。
- 不按字段或 18 个 leaf group 建 System；没有独立不变量和协作协议的类型只是转发层。
- 不把带 setter 的旧属性或 mutable reference 自动包装成纯 Query。

## 待 integration-review 项

| 未决项 | 状态 | 解除条件 |
| --- | --- | --- |
| 所有 P11 字段 reader/writer、反射、配置、别名、扩展 hook 与运行时注册 | `unknown` / `partial` | 源码/注册索引和必要运行证据覆盖所有相关入口。 |
| eye 生命周期 reset、damage event 顺序、实际 draw consumer | `partial` / `unknown` | 追到 spawn、removal、hurt、update 与 render 的真实生产路径。 |
| camera 与网络的端权威、包序、retry 和 reset 范围 | `partial` | 逐方向闭合 NetMessage、MessageBuffer、Main camera 与 session owner。 |
| appearance save/load 字段与版本兼容 | `unknown` | 取得与 Version4 版本匹配的非 stub serializer 与 round-trip 契约。 |
| zone producer 与环境数据时点 | `partial` / `unknown` | 追清每个 packed bit owner 与 snapshot freshness/barrier。 |
| arm/shadow writer、renderer consumer、重置和缓存容量 | `partial` | 补全所有写入者与真实 draw/reset 路径。 |
| 当前 NLTX System 注册与旧 facade 调用路由 | `unknown` | 从生产启动、注册与业务入口证明 API composition 被实际调用。 |
| 多 world/session、卸载、异常、对象复用和 stale entity | `unknown` | 对照目标实体/服务生命周期建立作用域及清理协议。 |

未解除项只阻止其依赖的 owner 或兼容结论，不把不确定性推广成整个设计无效。
