# P11 玩家表现与派生视图执行流程与迁移计划

- 文档状态：`proposed`
- 文档类型：静态运行流程说明 + 分段实现记录
- 实现状态：`partial`
- 验证状态：`partial`（仅 overhead message/composite arm focused verifier）
- 设计文档：[P11 System 设计](../../system-decomposition/authoritative/2026-09-30-authoritative-P11-player-presentation-derived-design.md)
- 权威输入报告：[P11 System 拆分报告](../../system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P11-player-presentation-derived.md)

## 目的与证据边界

本文件把已确认的 Version4 行为顺序、分段代码现状和后续接入门槛并列记录。`src/NSSLC` 已有 overhead message 状态与只读快照、独立 composite-arm state/System、shadow cache/System 等局部代码；本批修正了三个 mount 类型引用文件的命名空间导入以解除 focused verifier 编译错误。生产注册、Chat/Player 旧入口路由和实际 renderer consumer 仍未接通，其他阶段仍是计划。

Version4 `D:/TRbackup/Version4` 是行为事实基线。完整参考 `D:/TRbackup/无任何删减通过编译` 只用于补读同名实现与绘制消费者；其项目名、源码或可构建性都没有在本次重新构建验证。CPG 数据库 manifest 与 project fingerprint 见设计文档；其 `SourceSnapshotId=null`，且局部 call-sites 结果与源码有漏项，故只作定位线索。

## 运行时路径

### 眼睛动画

1. `Player` 受伤处理在播放受伤声音后调用 `eyeHelper.BlinkBecausePlayerGotHurt()`（Version4 `Terraria/Player.cs:22349`）；它把眼睛状态强制置为 `JustTookDamage` 并重置状态时间。
2. Player update 先执行 `sitting.UpdateSitting(this)`、`sleeping.UpdateState(this)`，再调用 `eyeHelper.Update(this)`（Version4 `Terraria/Player.cs:15726-15728`）。
3. `PlayerEyeHelper.Update` 先根据 Player 输入选择状态，再按当前状态时间算帧，最后递增计时器。睡眠帧计算会把内部计时器覆盖为 `sleeping.timeSleeping`。blind/blackout 优先级、受伤后保持窗口、生命阈值、中毒/醉酒/沙尘暴等输入都属于行为观察范围。
4. 完整参考 `Terraria.DataStructures/PlayerDrawHeadLayers.cs:401`、`PlayerDrawLayers.cs:2470,2756` 消费 `EyeFrameToShow`。Version4 对应 draw 文件为空类，当前目标绘制入口与帧可见时点仍为 `unknown`。

拟迁移组合：显式创建 eye input snapshot，由唯一 `PlayerEyeAnimationSystem` 写 eye component 并返回 frame snapshot；hurt 来自伤害 owner；draw adapter 只读输出。不得在 eye System 里重新推导/拥有伤害、环境、zone 或睡眠 authority。

### Overhead message

1. `ChatHelper.DisplayMessage` 对 `messageAuthor < byte.MaxValue` 的消息调用 `chatOverhead.NewMessage(text, displayTime)`，随后设置颜色，并继续 name tag、缓存或全局文本输出（Version4 `Terraria.Chat/ChatHelper.cs:72-80`）。这些后续效果留在 Chat/消息边界，不随 state owner 一起吞并。
2. `NewMessage` 覆盖 `chatText`，解析 snippet、测量文本并重置 `timeLeft`（Version4 `Terraria/Player.cs:467-480`）。这是替换语义；重复消息不是自动追加。
3. Player update 仅在 `timeLeft > 0` 时递减（Version4 `Terraria/Player.cs:14966-14968`）。
4. Version4 当前源码搜索未发现 `chatOverhead` 的绘制 reader，故目标 renderer route 仍为 `unknown`。完整参考在 `Terraria/Main.cs:64025-64033` 绘制 snippets，并在 `LegacyMultiplayerClosePlayersOverlay.cs:51`、`NewMultiplayerClosePlayersOverlay.cs:225` 按 overhead/emote 时间调整 nameplate；这些只证明参考版本的消费者位置，不证明 Version4 或 NLTX 的接线路径。

拟迁移组合：Chat Adapter 形成消息输入并保留 parse/measure 错误语义 → transient message owner 替换内容并按旧 tick 约定计时 → renderer projection 只读。颜色设置的时点、线程/作用域与消息绘制入口必须先闭合。

### 阴影、手臂与绘制

- Version4 Player update 调用 `UpdateSocialShadow()`（`:15064`）与 `UpdateAdvancedShadows()`（`:17679`）；传送相关路径还会先 reset，再多次重建 social shadow（`:21786`、`:21879`）。`SetCompositeArmFront/Back` 写每侧的 arm 值，action/item 路径还可能重置 arm enabled。
- Version4 中 `shadowPos`、shadow rotation/origin/direction、advanced shadow 缓存和 composite arm 的 writer 可读；`PlayerDrawLayers.cs` 与 `PlayerDrawHeadLayers.cs` 本身没有消费者实现。`Mount.cs` 也会改写 shadow 历史位置。
- 完整参考绘制层、`LegacyPlayerRenderer`、`LensFlareElement` 读取 shadow/arm 数据；这些关系只对参考版本成立，Version4 的真实 renderer/扩展入口仍待查证。完整参考 `Mount.cs` 的历史位置变换还说明 shadow state 不能只迁移 `Player.UpdateSocialShadow` 写入。
- CPG `SetCompositeArmFront/Back` call-sites 返回 1 / 4 条，与 Version4 源码在扣除方法声明后的直接调用数相符；`UpdateSocialShadow` 返回 2 条而源码有 3 处，`UpdateAdvancedShadows` 返回 `partial/0` 而源码有入口。CPG `Get-CpgMemberUses` 对 `shadowPos` 返回 18 个 use 且访问模式为 `Unknown`，对选中 `Player.cs`、`ChatHelper.cs`、`Main.cs` 的 `chatOverhead` 查询只返回 ChatHelper 的 2 个 use。索引状态不替代源码/运行闭包。

拟迁移组合：缓存状态和 arm pose 使用各自清晰写 API；frame projection 构造后交 renderer 消费，禁止 projection 回写权威外观/装备/动作状态。容量、清零时机、teleport 后历史长度、空 renderer 路径和作用域待确认。

### 摄像修正、网络与派生查询

- `UpdateNetOffset` 将 fake offset、collision adjusted velocity 与 offset decay 合并到 `netOffset`；`ResetNetOffsets` 清理 player array。其 scope、完整调用者与 camera target 的同步协议仍需审查。
- 主摄像目标由 `Main.UpdateCameraPan` 与 reported target 路径参与；`NetMessage` / `MessageBuffer` 编解码并更新同步缓存。client/server authority、包确认、失败和 retry 未确认。
- zone 属性访问 packed zone values；多个旧属性 setter 会直接写 packed zone、`skinVariant`、`position`、selection 或 capability bits。query 只接受冻结副本；写 setter 变成所属 authority 的方法/Command。
- `HeldItem` 可暴露 mutable `Item`，`SceneMetrics` 等外部值的快照边界未闭合，不能作为无副作用 Query 合约。

## 接入顺序与迁移阶段

每个阶段的输入、输出、依赖与门槛如下。后续实现必须按实际调用边界调整阶段；表格不是必须的类型流水线。

| 阶段 | 输入与依赖 | 预期输出 | 进入下一阶段的门槛 |
| --- | --- | --- | --- |
| 0. 锁定行为和入口 | P11 claim/report、Version4 源码快照、完整参考对照、只读 CPG API；先厘清 caller、writer、reader、lifecycle、注册、serializer 与 renderer | 每个候选 Concept 的旧入口映射和观察向量；标注来源版本、query scope、`partial`/`unknown` | CPG 关系和目标源码差异被记录；依赖 owner 的关键结论均有直接来源，其他项保持 `unknown`。 |
| 1. 眼睛动画切片 | 阶段 0 完成 hurt、update、spawn/removal 路由；status/zone/sleep 输入以明确 snapshot 传入 | 单一 eye state writer、frame snapshot、旧入口 adapter | tick 次数与状态优先级、受伤保持、睡眠计时、frame-before-increment、同 tick 顺序及真实 draw route 可核对；不得有第二 writer。 |
| 2. Overhead message | 阶段 0 闭合 ChatHelper 输入、parse/measure、颜色赋值、renderer reader 和线程作用域 | transient message state、replace/tick API、只读 projection；Chat Adapter 保留原输出效果 | 替换次数、timeLeft tick 边界、颜色、文本尺寸/snippet、cache/global chat 效果与失败行为明确；render 消费路由已接入。 |
| 3. 阴影、手臂及 draw snapshot | 确认全部 cache/arm writers、reset/teleport 路径和 Version4 renderer/extension consumers；参考绘制层只能提示检索位置 | 彼此边界明确的 transient cache / arm state API 和不可回写的 draw projection | 更新顺序、缓存容量/环绕、reset 范围、gravity/direction 转换、draw 可见性及消费者路由均有证据。 |
| 4. 派生 Query 与 setter 写 API | 按 zone/spatial/identity/interaction/ability/item/mount 等真实 owner 分组；依赖各 owner 提供 immutable snapshot | 纯 Query over copies；旧 setter 映射到所属 owner 的同步写 API 或明确 Command；adapter 复制 mutable inputs | 每个 Query 可重复读且不产生写入/外部效果；所有原 setter 的 authority、bit index、返回/错误语义保持；不泄漏 mutable alias。 |
| 5. camera / packet / appearance / save | 确认 client/server/session owner、NetMessage 与 MessageBuffer 双向协议、camera sync condition、目标版本 serializer | camera correction 与 camera target API、协议 Adapter、外观状态入口；必要时版本化存档映射 | 包布局、默认值、条件发送、错误/重试、ack、authority 和存档 round-trip 有目标版本证据。Version4 serializer 空体未解除前，不得声称 save 兼容。 |
| 6. 生产路由切换与旧入口清理 | 前 1-5 阶段所需的行为与集成门槛全部达到；检查注册、反射、配置和扩展入口 | 唯一实际 composition，旧入口仅保留为显式兼容 adapter 或按批准范围移除 | 生产调用真实进入新 owner；Observation 含 return/error、state delta、events/effects、order/visibility、lifecycle/scope 与 retry/idempotency；required verification 全部执行并记录。此门槛当前未满足，旧入口删除不在本计划授权范围内。 |

同一旧入口可组合多个 API，但每个概念只能有一个 canonical authority composition。跨 owner 组合默认非原子；没有事务或补偿证据时不得声称原子、幂等或可回滚。只有目标调度证据要求延迟提交时才增加 phase/barrier。

## 验收观察向量

各切片按其适用部分比较：

```text
Observation = (return/error, authoritative state delta, emitted events and external effects,
               order and visibility, lifecycle and scope, retry and idempotency)
```

返回值相同不代表事件、包、渲染、默认值、异常时机、tick 数或 state visibility 相同。旧 API compatibility、wire compatibility 和 semantic compatibility 分别验收。未知或未运行的项明确留空，不从静态设计推导通过。

## 本次验证记录

本次只运行一个 focused verifier，覆盖 overhead message state、不可变 snippets、replace/tick/reset 和 composite arm 独立写入、重力反转。未运行其他 P11 verifier、集成运行或完整行为比较。

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build','.\src\NSSLC\Component\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj','--no-restore','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')
```

构建结果：exit code `0`，0 warnings、0 errors。产物为 `Build/bin/Terraria.PlayerPresentationMessageVerification/Debug/net10.0/Terraria.PlayerPresentationMessageVerification.dll`。

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run','--project','.\src\NSSLC\Component\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj','--no-build','--no-restore','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')
```

运行结果：exit code `0`，stdout 为 `PASS: player overhead message state and composite arm ownership`。

## 尚未验证与风险

| 项目 | 当前状态 |
| --- | --- |
| System 运行注册、生产入口到新组合的真实调用链 | `unknown`；当前 NLTX 检索仅证明新类型和 focused verifier 引用存在。 |
| Version4 CPG 到源码快照绑定 | `unknown`；`SourceSnapshotId=null`，且眼睛、阴影、netOffset 调用点查询与源码不完全匹配。 |
| P11 全 writer/reader/reflection/serializer/lifecycle 闭包 | `partial` / `unknown`。 |
| draw consumer 在 Version4 的路径 | `unknown`；完整参考代码只能补充候选消费者。 |
| save/load round-trip | `unknown`；Version4 Player serializer 为 stub。 |
| camera/network authority、delivery/retry、zone freshness、multi-world/session scope | `partial` / `unknown`。 |
| 其他 P11 verifiers、生产入口/renderer 路由、运行时与完整行为等价 | `not-run` / `unknown`；本次只验证上面一个 focused verifier。 |

设计状态仍为 `proposed`，实现仅为 `partial`。这个 focused verifier 只证明局部代码路径，不证明生产入口命中新组合、Version4 行为等价或 P11 迁移成功。
