# Version4 非权威组件拆分分区 19/20：音频、粒子与演出

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：音频播放、旧音效实例、粒子、背景、天空表现、弹出文本和 cinematic timeline。
- 本分区组件化重点：表现层只读消费状态，隔离音频/图形副作用、资源生命周期和客户端清理。
- 本分区包含 32 个完整细分子系统、405 条成员记录（字段 354、属性 51）。来源序号覆盖区间 `80..4522`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `ClientPresentationAndTools` | 12 | 105 | 26 | 131 |
| `RuntimeComposition` | 9 | 125 | 0 | 125 |
| `SharedRuntimeMechanisms` | 11 | 124 | 25 | 149 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.32` | `RuntimeComposition` | `MainNpcFrameState` | presentation state | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.1.35` | `RuntimeComposition` | `MainParticlePools` | presentation state | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.1.38` | `RuntimeComposition` | `MainAmbientEffectsAndChatState` | presentation state | 19 | 0 | 19 | 待按成员访问模式拆分 |
| `4.1.42` | `RuntimeComposition` | `MainCageAquaticAndAmphibianAnimationState` | presentation state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.1.47` | `RuntimeComposition` | `MainCageBirdAnimationState` | presentation state | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.1.50` | `RuntimeComposition` | `MainBackgroundLayerCatalogState` | presentation state | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.1.51` | `RuntimeComposition` | `MainBackgroundParallaxAndStyleState` | presentation state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.1.52` | `RuntimeComposition` | `MainCageMammalAndReptileAnimationState` | presentation state | 18 | 0 | 18 | 待按成员访问模式拆分 |
| `4.1.53` | `RuntimeComposition` | `MainCageInsectAndSmallCritterAnimationState` | presentation state | 19 | 0 | 19 | 待按成员访问模式拆分 |
| `4.9.11` | `SharedRuntimeMechanisms` | `SharedAudioAndSoundData` | presentation | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.9.43` | `SharedRuntimeMechanisms` | `SharedEffectAndSkyPresentation` | presentation | 12 | 3 | 15 | 待按成员访问模式拆分 |
| `4.9.44` | `SharedRuntimeMechanisms` | `SharedParticlePresentation` | presentation | 6 | 2 | 8 | 待按成员访问模式拆分 |
| `4.9.59` | `SharedRuntimeMechanisms` | `SharedBackgroundPresentationDefinitions` | definition/presentation | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.9.138` | `SharedRuntimeMechanisms` | `SharedStarParticleState` | presentation state | 15 | 0 | 15 | 待按成员访问模式拆分 |
| `4.9.139` | `SharedRuntimeMechanisms` | `SharedCloudAndRainParticleState` | presentation state | 21 | 0 | 21 | 待按成员访问模式拆分 |
| `4.9.146` | `SharedRuntimeMechanisms` | `SharedDustParticleState` | presentation state | 18 | 0 | 18 | 待按成员访问模式拆分 |
| `4.9.147` | `SharedRuntimeMechanisms` | `SharedGoreEffectState` | presentation state | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.176` | `SharedRuntimeMechanisms` | `SharedSceneDecorationAndAudioState` | presentation/query | 0 | 20 | 20 | 待按成员访问模式拆分 |
| `4.9.180` | `SharedRuntimeMechanisms` | `SharedPopupTextContentAndContextState` | presentation | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.181` | `SharedRuntimeMechanisms` | `SharedPopupTextRenderLifecycleState` | presentation state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.11.2` | `ClientPresentationAndTools` | `AudioDefinitionAndTrackState` | definition/presentation | 10 | 11 | 21 | 待按成员访问模式拆分 |
| `4.11.3` | `ClientPresentationAndTools` | `AudioActiveSoundState` | presentation | 6 | 3 | 9 | 待按成员访问模式拆分 |
| `4.11.4` | `ClientPresentationAndTools` | `CinematicTimelineState` | presentation | 13 | 11 | 24 | 待按成员访问模式拆分 |
| `4.11.7` | `ClientPresentationAndTools` | `AudioPlaybackCoordinatorState` | presentation/adapter | 2 | 1 | 3 | 待按成员访问模式拆分 |
| `4.11.8` | `ClientPresentationAndTools` | `AudioTrackedSoundState` | presentation/state | 1 | 0 | 1 | 待按成员访问模式拆分 |
| `4.11.26` | `ClientPresentationAndTools` | `SharedAudioLegacyTrackedInstanceState` | presentation/adapter | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.11.27` | `ClientPresentationAndTools` | `SharedAudioLegacySoundServicesState` | presentation/adapter | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.11.30` | `ClientPresentationAndTools` | `SharedAudioLegacyPlayerAndInterfaceInstanceState` | presentation/adapter | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.11.31` | `ClientPresentationAndTools` | `SharedAudioLegacyGameplaySoundDefinitionCatalogState` | definition/catalog | 25 | 0 | 25 | 待按成员访问模式拆分 |
| `4.11.32` | `ClientPresentationAndTools` | `SharedAudioLegacyInterfaceSoundDefinitionCatalogState` | definition/catalog | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.11.39` | `ClientPresentationAndTools` | `SharedAudioLegacyWorldEnvironmentInstanceState` | presentation/adapter | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.11.40` | `ClientPresentationAndTools` | `SharedAudioLegacyEntityFeedbackInstanceState` | presentation/adapter | 10 | 0 | 10 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainNpcFrameState`

- 原报告章节：`4.1.32`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainNpcFrameState`
- 细分职责：NPC 帧计数和动画分类。
- 边界角色：`presentation state`；最小 seam：NPC frame view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 445 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1084 | 2 | npcFrameCount | int[] | `public static int[] npcFrameCount = new int[697]  	{  		1, 2, 2, 3, 6, 2, 2, 1, 1, 1,  		1, 1, 1, 1, 1, 1, 2, 25, 23, 25,  		21, 15, 26, 2, 10, 1, 16, 16, 16, 3,  		1, 15, 6, 1, 3, 2, 2, 21, 25, 1,  		1, 1, 3, 3, 15, 3, 7, 7, 6, 5,  		6, 5, 3, 3, 23, 6, 3, 6, 6, 2,  		5, 6, 5, 7, 7, 4, 5, 8, 1, 5,  		1, 2, 4, 16, 5, 4, 4, 15, 16, 16,  		16, 2, 4, 6, 6, 18, 16, 1, 1, 1,  		1, 1, 1, 4, 3, 1, 1, 1, 1, 1,  		1, 5, 6, 7, 16, 1, 1, 25, 23, 12,  		20, 21, 1, 2, 2, 3, 6, 1, 1, 1,  		15, 4, 11, 1, 23, 6, 6, 6, 1, 2,  		2, 1, 3, 4, 1, 2, 1, 4, 2, 1,  		15, 3, 25, 4, 5, 7, 3, 2, 12, 12,  		4, 4, 4, 8, 8, 13, 5, 6, 4, 15,  		23, 3, 15, 8, 5, 4, 13, 15, 12, 4,  		14, 14, 3, 2, 5, 3, 2, 3, 23, 5,  		14, 16, 5, 2, 2, 12, 3, 3, 3, 3,  		2, 2, 2, 2, 2, 7, 14, 15, 16, 8,  		3, 15, 15, 16, 2, 3, 20, 25, 23, 26,  		4, 4, 16, 16, 20, 20, 20, 2, 2, 2,  		2, 8, 12, 3, 4, 2, 4, 25, 26, 26,  		6, 3, 3, 3, 3, 3, 5, 4, 4, 5,  		4, 6, 7, 15, 4, 7, 6, 1, 1, 2,  		4, 3, 5, 3, 3, 3, 4, 5, 6, 4,  		2, 1, 8, 4, 4, 1, 8, 1, 4, 15,  		15, 15, 15, 15, 15, 16, 15, 15, 15, 15,  		15, 3, 3, 3, 3, 3, 3, 16, 3, 6,  		12, 21, 21, 20, 16, 15, 15, 5, 5, 6,  		6, 5, 2, 7, 2, 6, 6, 6, 6, 6,  		15, 15, 15, 15, 15, 11, 4, 2, 2, 3,  		3, 3, 16, 15, 16, 10, 14, 12, 1, 10,  		8, 3, 3, 2, 2, 2, 2, 7, 15, 15,  		15, 6, 3, 10, 10, 6, 9, 8, 9, 8,  		20, 10, 6, 23, 1, 4, 24, 2, 4, 6,  		6, 13, 15, 15, 15, 15, 4, 4, 26, 23,  		8, 2, 4, 4, 4, 4, 2, 2, 4, 12,  		12, 9, 9, 9, 1, 9, 11, 2, 2, 9,  		5, 6, 4, 18, 8, 11, 1, 4, 5, 8,  		4, 1, 1, 1, 1, 4, 2, 5, 4, 11,  		5, 11, 1, 1, 1, 10, 10, 15, 8, 17,  		6, 6, 1, 12, 12, 13, 15, 9, 5, 10,  		7, 7, 7, 7, 7, 7, 7, 4, 4, 16,  		16, 25, 5, 7, 3, 13, 2, 6, 2, 19,  		19, 19, 20, 26, 3, 1, 1, 1, 1, 1,  		16, 21, 9, 16, 7, 6, 18, 13, 20, 12,  		12, 20, 6, 14, 14, 14, 14, 6, 1, 3,  		25, 19, 20, 22, 2, 4, 4, 4, 11, 9,  		8, 1, 9, 1, 8, 8, 12, 12, 11, 11,  		11, 11, 11, 11, 11, 11, 11, 1, 6, 9,  		1, 1, 1, 1, 1, 1, 4, 1, 10, 1,  		8, 4, 1, 5, 8, 8, 8, 8, 9, 9,  		5, 4, 8, 16, 8, 2, 3, 3, 6, 6,  		7, 13, 4, 4, 4, 4, 1, 1, 1, 8,  		25, 11, 14, 14, 14, 17, 17, 17, 5, 5,  		5, 14, 14, 14, 9, 9, 9, 9, 17, 17,  		16, 16, 18, 18, 10, 10, 10, 10, 4, 1,  		6, 9, 6, 4, 4, 4, 14, 4, 25, 13,  		3, 7, 6, 6, 1, 4, 4, 4, 4, 4,  		4, 4, 15, 15, 8, 8, 2, 6, 15, 15,  		6, 13, 5, 5, 7, 5, 14, 14, 4, 6,  		21, 1, 1, 1, 11, 12, 6, 6, 17, 6,  		16, 21, 16, 23, 5, 16, 2, 28, 28, 6,  		6, 6, 6, 6, 6, 6, 7, 7, 7, 7,  		7, 7, 7, 3, 4, 6, 27, 16, 2, 2,  		4, 3, 4, 23, 6, 1, 1, 2, 8, 8,  		14, 6, 6, 6, 6, 6, 2, 4, 14, 14,  		14, 14, 14, 14, 14, 1, 1, 13, 6, 13,  		1, 3, 16, 3, 30, 3, 1  	};` | `public static int[] npcFrameCount = new int[697] { 1, 2, 2, 3, 6, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 25, 23, 25, 21, 15, 26, 2, 10, 1, 16, 16, 16, 3, 1, 15, 6, 1, 3, 2, 2, 21, 25, 1, 1, 1, 3, 3, 15, 3, 7, 7, 6, 5, 6, 5, 3, 3, 23, 6, 3, 6, 6, 2, 5, 6, 5, 7, 7, 4, 5, 8, 1, 5, 1, 2, 4, 16, 5, 4, 4, 15, 16, 16, 16, 2, 4, 6, 6, 18, 16, 1, 1, 1, 1, 1, 1, 4, 3, 1, 1, 1, 1, 1, 1, 5, 6, 7, 16, 1, 1, 25, 23, 12, 20, 21, 1, 2, 2, 3, 6, 1, 1, 1, 15, 4, 11, 1, 23, 6, 6, 6, 1, 2, 2, 1, 3, 4, 1, 2, 1, 4, 2, 1, 15, 3, 25, 4, 5, 7, 3, 2, 12, 12, 4, 4, 4, 8, 8, 13, 5, 6, 4, 15, 23, 3, 15, 8, 5, 4, 13, 15, 12, 4, 14, 14, 3, 2, 5, 3, 2, 3, 23, 5, 14, 16, 5, 2, 2, 12, 3, 3, 3, 3, 2, 2, 2, 2, 2, 7, 14, 15, 16, 8, 3, 15, 15, 16, 2, 3, 20, 25, 23, 26, 4, 4, 16, 16, 20, 20, 20, 2, 2, 2, 2, 8, 12, 3, 4, 2, 4, 25, 26, 26, 6, 3, 3, 3, 3, 3, 5, 4, 4, 5, 4, 6, 7, 15, 4, 7, 6, 1, 1, 2, 4, 3, 5, 3, 3, 3, 4, 5, 6, 4, 2, 1, 8, 4, 4, 1, 8, 1, 4, 15, 15, 15, 15, 15, 15, 16, 15, 15, 15, 15, 15, 3, 3, 3, 3, 3, 3, 16, 3, 6, 12, 21, 21, 20, 16, 15, 15, 5, 5, 6, 6, 5, 2, 7, 2, 6, 6, 6, 6, 6, 15, 15, 15, 15, 15, 11, 4, 2, 2, 3, 3, 3, 16, 15, 16, 10, 14, 12, 1, 10, 8, 3, 3, 2, 2, 2, 2, 7, 15, 15, 15, 6, 3, 10, 10, 6, 9, 8, 9, 8, 20, 10, 6, 23, 1, 4, 24, 2, 4, 6, 6, 13, 15, 15, 15, 15, 4, 4, 26, 23, 8, 2, 4, 4, 4, 4, 2, 2, 4, 12, 12, 9, 9, 9, 1, 9, 11, 2, 2, 9, 5, 6, 4, 18, 8, 11, 1, 4, 5, 8, 4, 1, 1, 1, 1, 4, 2, 5, 4, 11, 5, 11, 1, 1, 1, 10, 10, 15, 8, 17, 6, 6, 1, 12, 12, 13, 15, 9, 5, 10, 7, 7, 7, 7, 7, 7, 7, 4, 4, 16, 16, 25, 5, 7, 3, 13, 2, 6, 2, 19, 19, 19, 20, 26, 3, 1, 1, 1, 1, 1, 16, 21, 9, 16, 7, 6, 18, 13, 20, 12, 12, 20, 6, 14, 14, 14, 14, 6, 1, 3, 25, 19, 20, 22, 2, 4, 4, 4, 11, 9, 8, 1, 9, 1, 8, 8, 12, 12, 11, 11, 11, 11, 11, 11, 11, 11, 11, 1, 6, 9, 1, 1, 1, 1, 1, 1, 4, 1, 10, 1, 8, 4, 1, 5, 8, 8, 8, 8, 9, 9, 5, 4, 8, 16, 8, 2, 3, 3, 6, 6, 7, 13, 4, 4, 4, 4, 1, 1, 1, 8, 25, 11, 14, 14, 14, 17, 17, 17, 5, 5, 5, 14, 14, 14, 9, 9, 9, 9, 17, 17, 16, 16, 18, 18, 10, 10, 10, 10, 4, 1, 6, 9, 6, 4, 4, 4, 14, 4, 25, 13, 3, 7, 6, 6, 1, 4, 4, 4, 4, 4, 4, 4, 15, 15, 8, 8, 2, 6, 15, 15, 6, 13, 5, 5, 7, 5, 14, 14, 4, 6, 21, 1, 1, 1, 11, 12, 6, 6, 17, 6, 16, 21, 16, 23, 5, 16, 2, 28, 28, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 3, 4, 6, 27, 16, 2, 2, 4, 3, 4, 23, 6, 1, 1, 2, 8, 8, 14, 6, 6, 6, 6, 6, 2, 4, 14, 14, 14, 14, 14, 14, 14, 1, 1, 13, 6, 13, 1, 3, 16, 3, 30, 3, 1 };` |
| 446 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1159 | 2 | clientPlayer | Terraria.Player | `public static Player clientPlayer = new Player();` | `public static Player clientPlayer = new Player();` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainParticlePools`

- 原报告章节：`4.1.35`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainParticlePools`
- 细分职责：世界粒子渲染器引用。
- 边界角色：`presentation state`；最小 seam：particle pool view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 461 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1190 | 2 | ParticleSystem_World_OverPlayers | Terraria.Graphics.Renderers.ParticleRenderer | `public static ParticleRenderer ParticleSystem_World_OverPlayers = new ParticleRenderer();` | `public static ParticleRenderer ParticleSystem_World_OverPlayers = new ParticleRenderer();` |
| 462 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1192 | 2 | ParticleSystem_World_BehindPlayers | Terraria.Graphics.Renderers.ParticleRenderer | `public static ParticleRenderer ParticleSystem_World_BehindPlayers = new ParticleRenderer();` | `public static ParticleRenderer ParticleSystem_World_BehindPlayers = new ParticleRenderer();` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`MainAmbientEffectsAndChatState`

- 原报告章节：`4.1.38`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainAmbientEffectsAndChatState`
- 细分职责：风雨音乐、环境瀑布、聊天监视器和循环效果。
- 边界角色：`presentation state`；最小 seam：ambient/chat projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 474 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1218 | 2 | _shouldUseWindyDayMusic | bool | `public static bool _shouldUseWindyDayMusic = false;` | `public static bool _shouldUseWindyDayMusic = false;` |
| 475 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1220 | 2 | _shouldUseStormMusic | bool | `public static bool _shouldUseStormMusic = false;` | `public static bool _shouldUseStormMusic = false;` |
| 476 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1222 | 2 | _minWind | float | `private static float _minWind = 0.34f;` | `private static float _minWind = 0.34f;` |
| 477 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1224 | 2 | _maxWind | float | `private static float _maxWind = 0.4f;` | `private static float _maxWind = 0.4f;` |
| 478 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1226 | 2 | _minRain | float | `private static float _minRain = 0.4f;` | `private static float _minRain = 0.4f;` |
| 479 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1228 | 2 | _maxRain | float | `private static float _maxRain = 0.5f;` | `private static float _maxRain = 0.5f;` |
| 480 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1230 | 2 | ambientWaterfallX | float | `public static float ambientWaterfallX = -1f;` | `public static float ambientWaterfallX = -1f;` |
| 481 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1232 | 2 | ambientWaterfallY | float | `public static float ambientWaterfallY = -1f;` | `public static float ambientWaterfallY = -1f;` |
| 482 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1234 | 2 | ambientWaterfallStrength | float | `public static float ambientWaterfallStrength = 0f;` | `public static float ambientWaterfallStrength = 0f;` |
| 483 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1236 | 2 | ambientLavafallX | float | `public static float ambientLavafallX = -1f;` | `public static float ambientLavafallX = -1f;` |
| 484 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1238 | 2 | ambientLavafallY | float | `public static float ambientLavafallY = -1f;` | `public static float ambientLavafallY = -1f;` |
| 485 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1240 | 2 | ambientLavafallStrength | float | `public static float ambientLavafallStrength = 0f;` | `public static float ambientLavafallStrength = 0f;` |
| 486 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1242 | 2 | ambientLavaX | float | `public static float ambientLavaX = -1f;` | `public static float ambientLavaX = -1f;` |
| 487 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1244 | 2 | ambientLavaY | float | `public static float ambientLavaY = -1f;` | `public static float ambientLavaY = -1f;` |
| 488 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1246 | 2 | ambientLavaStrength | float | `public static float ambientLavaStrength;` | `public static float ambientLavaStrength;` |
| 489 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1248 | 2 | ambientCounter | int | `public static int ambientCounter;` | `public static int ambientCounter;` |
| 490 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1250 | 2 | _isWaterfallMusicPlaying | bool | `private static bool _isWaterfallMusicPlaying = false;` | `private static bool _isWaterfallMusicPlaying = false;` |
| 491 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1252 | 2 | _isLavafallMusicPlaying | bool | `private static bool _isLavafallMusicPlaying = false;` | `private static bool _isLavafallMusicPlaying = false;` |
| 492 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1254 | 2 | chatMonitor | Terraria.GameContent.UI.Chat.IChatMonitor | `public static IChatMonitor chatMonitor = new RemadeChatMonitor();` | `public static IChatMonitor chatMonitor = new RemadeChatMonitor();` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`MainCageAquaticAndAmphibianAnimationState`

- 原报告章节：`4.1.42`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainCageAnimationState`
- 细分职责：水生和两栖捕获物的笼具、鱼缸与罐体帧。
- 边界角色：`presentation state`；最小 seam：aquatic amphibian cage animation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 331 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 839 | 2 | fishBowlFrameMode | byte[] | `public static byte[] fishBowlFrameMode = new byte[cageFrames];` | `public static byte[] fishBowlFrameMode = new byte[cageFrames];` |
| 332 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 841 | 2 | fishBowlFrame | int[] | `public static int[] fishBowlFrame = new int[cageFrames];` | `public static int[] fishBowlFrame = new int[cageFrames];` |
| 333 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 843 | 2 | fishBowlFrameCounter | int[] | `public static int[] fishBowlFrameCounter = new int[cageFrames];` | `public static int[] fishBowlFrameCounter = new int[cageFrames];` |
| 334 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 845 | 2 | lavaFishBowlFrame | int[] | `public static int[] lavaFishBowlFrame = new int[cageFrames];` | `public static int[] lavaFishBowlFrame = new int[cageFrames];` |
| 335 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 847 | 2 | lavaFishBowlFrameCounter | int[] | `public static int[] lavaFishBowlFrameCounter = new int[cageFrames];` | `public static int[] lavaFishBowlFrameCounter = new int[cageFrames];` |
| 336 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 849 | 2 | frogCageFrame | int[] | `public static int[] frogCageFrame = new int[cageFrames];` | `public static int[] frogCageFrame = new int[cageFrames];` |
| 337 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 851 | 2 | frogCageFrameCounter | int[] | `public static int[] frogCageFrameCounter = new int[cageFrames];` | `public static int[] frogCageFrameCounter = new int[cageFrames];` |
| 344 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 865 | 2 | jellyfishCageMode | byte[,] | `public static byte[,] jellyfishCageMode = new byte[3, cageFrames];` | `public static byte[,] jellyfishCageMode = new byte[3, cageFrames];` |
| 345 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 867 | 2 | jellyfishCageFrame | int[,] | `public static int[,] jellyfishCageFrame = new int[3, cageFrames];` | `public static int[,] jellyfishCageFrame = new int[3, cageFrames];` |
| 346 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 869 | 2 | jellyfishCageFrameCounter | int[,] | `public static int[,] jellyfishCageFrameCounter = new int[3, cageFrames];` | `public static int[,] jellyfishCageFrameCounter = new int[3, cageFrames];` |
| 357 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 891 | 2 | waterStriderCageFrame | int[] | `public static int[] waterStriderCageFrame = new int[cageFrames];` | `public static int[] waterStriderCageFrame = new int[cageFrames];` |
| 358 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 893 | 2 | waterStriderCageFrameCounter | int[] | `public static int[] waterStriderCageFrameCounter = new int[cageFrames];` | `public static int[] waterStriderCageFrameCounter = new int[cageFrames];` |
| 359 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 895 | 2 | seahorseCageFrame | int[] | `public static int[] seahorseCageFrame = new int[cageFrames];` | `public static int[] seahorseCageFrame = new int[cageFrames];` |
| 360 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 897 | 2 | seahorseCageFrameCounter | int[] | `public static int[] seahorseCageFrameCounter = new int[cageFrames];` | `public static int[] seahorseCageFrameCounter = new int[cageFrames];` |
| 367 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 911 | 2 | pufferfishCageFrame | int[] | `public static int[] pufferfishCageFrame = new int[cageFrames];` | `public static int[] pufferfishCageFrame = new int[cageFrames];` |
| 368 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 913 | 2 | pufferfishCageFrameCounter | int[] | `public static int[] pufferfishCageFrameCounter = new int[cageFrames];` | `public static int[] pufferfishCageFrameCounter = new int[cageFrames];` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`MainCageBirdAnimationState`

- 原报告章节：`4.1.47`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainCageAnimationState`
- 上一级 peer 细分子系统：`MainCageBirdAndTerrestrialAnimationState`
- 细分职责：鸟类捕获物的笼具帧、鱼缸帧和动画计数。
- 边界角色：`presentation state`；最小 seam：bird cage animation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 304 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 785 | 2 | mallardCageFrame | int[] | `public static int[] mallardCageFrame = new int[cageFrames];` | `public static int[] mallardCageFrame = new int[cageFrames];` |
| 305 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 787 | 2 | mallardCageFrameCounter | int[] | `public static int[] mallardCageFrameCounter = new int[cageFrames];` | `public static int[] mallardCageFrameCounter = new int[cageFrames];` |
| 306 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 789 | 2 | duckCageFrame | int[] | `public static int[] duckCageFrame = new int[cageFrames];` | `public static int[] duckCageFrame = new int[cageFrames];` |
| 307 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 791 | 2 | duckCageFrameCounter | int[] | `public static int[] duckCageFrameCounter = new int[cageFrames];` | `public static int[] duckCageFrameCounter = new int[cageFrames];` |
| 308 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 793 | 2 | grebeCageFrame | int[] | `public static int[] grebeCageFrame = new int[cageFrames];` | `public static int[] grebeCageFrame = new int[cageFrames];` |
| 309 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 795 | 2 | grebeCageFrameCounter | int[] | `public static int[] grebeCageFrameCounter = new int[cageFrames];` | `public static int[] grebeCageFrameCounter = new int[cageFrames];` |
| 310 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 797 | 2 | seagullCageFrame | int[] | `public static int[] seagullCageFrame = new int[cageFrames];` | `public static int[] seagullCageFrame = new int[cageFrames];` |
| 311 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 799 | 2 | seagullCageFrameCounter | int[] | `public static int[] seagullCageFrameCounter = new int[cageFrames];` | `public static int[] seagullCageFrameCounter = new int[cageFrames];` |
| 312 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 801 | 2 | birdCageFrame | int[] | `public static int[] birdCageFrame = new int[cageFrames];` | `public static int[] birdCageFrame = new int[cageFrames];` |
| 313 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 803 | 2 | birdCageFrameCounter | int[] | `public static int[] birdCageFrameCounter = new int[cageFrames];` | `public static int[] birdCageFrameCounter = new int[cageFrames];` |
| 314 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 805 | 2 | redBirdCageFrame | int[] | `public static int[] redBirdCageFrame = new int[cageFrames];` | `public static int[] redBirdCageFrame = new int[cageFrames];` |
| 315 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 807 | 2 | redBirdCageFrameCounter | int[] | `public static int[] redBirdCageFrameCounter = new int[cageFrames];` | `public static int[] redBirdCageFrameCounter = new int[cageFrames];` |
| 316 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 809 | 2 | blueBirdCageFrame | int[] | `public static int[] blueBirdCageFrame = new int[cageFrames];` | `public static int[] blueBirdCageFrame = new int[cageFrames];` |
| 317 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 811 | 2 | blueBirdCageFrameCounter | int[] | `public static int[] blueBirdCageFrameCounter = new int[cageFrames];` | `public static int[] blueBirdCageFrameCounter = new int[cageFrames];` |
| 318 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 813 | 2 | macawCageFrame | int[] | `public static int[] macawCageFrame = new int[cageFrames];` | `public static int[] macawCageFrame = new int[cageFrames];` |
| 319 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 815 | 2 | macawCageFrameCounter | int[] | `public static int[] macawCageFrameCounter = new int[cageFrames];` | `public static int[] macawCageFrameCounter = new int[cageFrames];` |
| 355 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 887 | 2 | penguinCageFrame | int[] | `public static int[] penguinCageFrame = new int[cageFrames];` | `public static int[] penguinCageFrame = new int[cageFrames];` |
| 356 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 889 | 2 | penguinCageFrameCounter | int[] | `public static int[] penguinCageFrameCounter = new int[cageFrames];` | `public static int[] penguinCageFrameCounter = new int[cageFrames];` |
| 363 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 903 | 2 | owlCageFrame | int[] | `public static int[] owlCageFrame = new int[cageFrames];` | `public static int[] owlCageFrame = new int[cageFrames];` |
| 364 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 905 | 2 | owlCageFrameCounter | int[] | `public static int[] owlCageFrameCounter = new int[cageFrames];` | `public static int[] owlCageFrameCounter = new int[cageFrames];` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`MainBackgroundLayerCatalogState`

- 原报告章节：`4.1.50`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainBackgroundAndSeasonalState`
- 上一级 peer 细分子系统：`MainBackgroundLayerState`
- 细分职责：Main 背景图层、场景集合和背景目录状态。
- 边界角色：`presentation state`；最小 seam：main background layer catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 82 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 324 | 2 | cloudBGAlpha | float | `public static float cloudBGAlpha;` | `public static float cloudBGAlpha;` |
| 83 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 326 | 2 | cloudBGActive | float | `public static float cloudBGActive;` | `public static float cloudBGActive;` |
| 84 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 328 | 2 | treeMntBGSet1 | int[] | `public static int[] treeMntBGSet1 = new int[2];` | `public static int[] treeMntBGSet1 = new int[2];` |
| 85 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 330 | 2 | treeMntBGSet2 | int[] | `public static int[] treeMntBGSet2 = new int[2];` | `public static int[] treeMntBGSet2 = new int[2];` |
| 86 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 332 | 2 | treeMntBGSet3 | int[] | `public static int[] treeMntBGSet3 = new int[2];` | `public static int[] treeMntBGSet3 = new int[2];` |
| 87 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 334 | 2 | treeMntBGSet4 | int[] | `public static int[] treeMntBGSet4 = new int[2];` | `public static int[] treeMntBGSet4 = new int[2];` |
| 88 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 336 | 2 | treeBGSet1 | int[] | `public static int[] treeBGSet1 = new int[3];` | `public static int[] treeBGSet1 = new int[3];` |
| 89 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 338 | 2 | treeBGSet2 | int[] | `public static int[] treeBGSet2 = new int[3];` | `public static int[] treeBGSet2 = new int[3];` |
| 90 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 340 | 2 | treeBGSet3 | int[] | `public static int[] treeBGSet3 = new int[3];` | `public static int[] treeBGSet3 = new int[3];` |
| 91 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 342 | 2 | treeBGSet4 | int[] | `public static int[] treeBGSet4 = new int[3];` | `public static int[] treeBGSet4 = new int[3];` |
| 92 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 344 | 2 | corruptBG | int[] | `public static int[] corruptBG = new int[3];` | `public static int[] corruptBG = new int[3];` |
| 93 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 346 | 2 | jungleBG | int[] | `public static int[] jungleBG = new int[3];` | `public static int[] jungleBG = new int[3];` |
| 94 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 348 | 2 | snowMntBG | int[] | `public static int[] snowMntBG = new int[2];` | `public static int[] snowMntBG = new int[2];` |
| 95 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 350 | 2 | snowBG | int[] | `public static int[] snowBG = new int[3];` | `public static int[] snowBG = new int[3];` |
| 96 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 352 | 2 | hallowBG | int[] | `public static int[] hallowBG = new int[3];` | `public static int[] hallowBG = new int[3];` |
| 97 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 354 | 2 | crimsonBG | int[] | `public static int[] crimsonBG = new int[3];` | `public static int[] crimsonBG = new int[3];` |
| 98 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 356 | 2 | desertBackgroundSet | Terraria.DataStructures.BackgroundVariantSet | `public static BackgroundVariantSet desertBackgroundSet = new BackgroundVariantSet();` | `public static BackgroundVariantSet desertBackgroundSet = new BackgroundVariantSet();` |
| 99 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 358 | 2 | mushroomBG | int[] | `public static int[] mushroomBG = new int[3];` | `public static int[] mushroomBG = new int[3];` |
| 100 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 360 | 2 | oceanBG | int | `public static int oceanBG;` | `public static int oceanBG;` |
| 101 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 362 | 2 | underworldBG | int[] | `public static int[] underworldBG = new int[5];` | `public static int[] underworldBG = new int[5];` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`MainBackgroundParallaxAndStyleState`

- 原报告章节：`4.1.51`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainBackgroundAndSeasonalState`
- 上一级 peer 细分子系统：`MainBackgroundLayerState`
- 细分职责：Main 背景偏移、视差、树木和风格参数状态。
- 边界角色：`presentation state`；最小 seam：main background parallax style port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 80 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 320 | 2 | essScale | float | `public static float essScale = 1f;` | `public static float essScale = 1f;` |
| 81 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 322 | 2 | essDir | int | `public static int essDir = -1;` | `public static int essDir = -1;` |
| 102 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 364 | 2 | treeX | int[] | `public static int[] treeX = new int[4];` | `public static int[] treeX = new int[4];` |
| 103 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 366 | 2 | treeStyle | int[] | `public static int[] treeStyle = new int[4];` | `public static int[] treeStyle = new int[4];` |
| 104 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 368 | 2 | caveBackX | int[] | `public static int[] caveBackX = new int[4];` | `public static int[] caveBackX = new int[4];` |
| 105 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 370 | 2 | caveBackStyle | int[] | `public static int[] caveBackStyle = new int[4];` | `public static int[] caveBackStyle = new int[4];` |
| 106 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 372 | 2 | iceBackStyle | int | `public static int iceBackStyle;` | `public static int iceBackStyle;` |
| 107 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 374 | 2 | hellBackStyle | int | `public static int hellBackStyle;` | `public static int hellBackStyle;` |
| 108 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 376 | 2 | jungleBackStyle | int | `public static int jungleBackStyle;` | `public static int jungleBackStyle;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`MainCageMammalAndReptileAnimationState`

- 原报告章节：`4.1.52`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainCageAnimationState`
- 上一级 peer 细分子系统：`MainCageTerrestrialCritterAnimationState`
- 细分职责：Main 中兔、松鼠、蜗牛、鼠、龟和大鼠捕获物动画状态。
- 边界角色：`presentation state`；最小 seam：mammal reptile cage animation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 296 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 769 | 2 | cageFrames | int | `public static int cageFrames = 25;` | `public static int cageFrames = 25;` |
| 297 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 771 | 2 | critterCage | bool | `public static bool critterCage;` | `public static bool critterCage;` |
| 298 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 773 | 2 | bunnyCageFrame | int[] | `public static int[] bunnyCageFrame = new int[cageFrames];` | `public static int[] bunnyCageFrame = new int[cageFrames];` |
| 299 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 775 | 2 | bunnyCageFrameCounter | int[] | `public static int[] bunnyCageFrameCounter = new int[cageFrames];` | `public static int[] bunnyCageFrameCounter = new int[cageFrames];` |
| 300 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 777 | 2 | squirrelCageFrame | int[] | `public static int[] squirrelCageFrame = new int[cageFrames];` | `public static int[] squirrelCageFrame = new int[cageFrames];` |
| 301 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 779 | 2 | squirrelCageFrameCounter | int[] | `public static int[] squirrelCageFrameCounter = new int[cageFrames];` | `public static int[] squirrelCageFrameCounter = new int[cageFrames];` |
| 302 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 781 | 2 | squirrelCageFrameOrange | int[] | `public static int[] squirrelCageFrameOrange = new int[cageFrames];` | `public static int[] squirrelCageFrameOrange = new int[cageFrames];` |
| 303 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 783 | 2 | squirrelCageFrameCounterOrange | int[] | `public static int[] squirrelCageFrameCounterOrange = new int[cageFrames];` | `public static int[] squirrelCageFrameCounterOrange = new int[cageFrames];` |
| 327 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 831 | 2 | snailCageFrame | int[] | `public static int[] snailCageFrame = new int[cageFrames];` | `public static int[] snailCageFrame = new int[cageFrames];` |
| 328 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 833 | 2 | snailCageFrameCounter | int[] | `public static int[] snailCageFrameCounter = new int[cageFrames];` | `public static int[] snailCageFrameCounter = new int[cageFrames];` |
| 329 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 835 | 2 | snail2CageFrame | int[] | `public static int[] snail2CageFrame = new int[cageFrames];` | `public static int[] snail2CageFrame = new int[cageFrames];` |
| 330 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 837 | 2 | snail2CageFrameCounter | int[] | `public static int[] snail2CageFrameCounter = new int[cageFrames];` | `public static int[] snail2CageFrameCounter = new int[cageFrames];` |
| 338 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 853 | 2 | mouseCageFrame | int[] | `public static int[] mouseCageFrame = new int[cageFrames];` | `public static int[] mouseCageFrame = new int[cageFrames];` |
| 339 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 855 | 2 | mouseCageFrameCounter | int[] | `public static int[] mouseCageFrameCounter = new int[cageFrames];` | `public static int[] mouseCageFrameCounter = new int[cageFrames];` |
| 340 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 857 | 2 | turtleCageFrame | int[] | `public static int[] turtleCageFrame = new int[cageFrames];` | `public static int[] turtleCageFrame = new int[cageFrames];` |
| 341 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 859 | 2 | turtleCageFrameCounter | int[] | `public static int[] turtleCageFrameCounter = new int[cageFrames];` | `public static int[] turtleCageFrameCounter = new int[cageFrames];` |
| 351 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 879 | 2 | ratCageFrame | int[] | `public static int[] ratCageFrame = new int[cageFrames];` | `public static int[] ratCageFrame = new int[cageFrames];` |
| 352 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 881 | 2 | ratCageFrameCounter | int[] | `public static int[] ratCageFrameCounter = new int[cageFrames];` | `public static int[] ratCageFrameCounter = new int[cageFrames];` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`MainCageInsectAndSmallCritterAnimationState`

- 原报告章节：`4.1.53`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`MainCageAnimationState`
- 上一级 peer 细分子系统：`MainCageTerrestrialCritterAnimationState`
- 细分职责：Main 中蝴蝶、蜻蜓、蝎、仙女、蠕虫和其他小动物动画状态。
- 边界角色：`presentation state`；最小 seam：insect small critter cage animation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 320 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 817 | 2 | butterflyCageMode | byte[,] | `public static byte[,] butterflyCageMode = new byte[9, cageFrames];` | `public static byte[,] butterflyCageMode = new byte[9, cageFrames];` |
| 321 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 819 | 2 | butterflyCageFrame | int[,] | `public static int[,] butterflyCageFrame = new int[9, cageFrames];` | `public static int[,] butterflyCageFrame = new int[9, cageFrames];` |
| 322 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 821 | 2 | butterflyCageFrameCounter | int[,] | `public static int[,] butterflyCageFrameCounter = new int[9, cageFrames];` | `public static int[,] butterflyCageFrameCounter = new int[9, cageFrames];` |
| 323 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 823 | 2 | dragonflyJarFrameCounter | int[,] | `public static int[,] dragonflyJarFrameCounter = new int[7, cageFrames];` | `public static int[,] dragonflyJarFrameCounter = new int[7, cageFrames];` |
| 324 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 825 | 2 | dragonflyJarFrame | int[,] | `public static int[,] dragonflyJarFrame = new int[7, cageFrames];` | `public static int[,] dragonflyJarFrame = new int[7, cageFrames];` |
| 325 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 827 | 2 | scorpionCageFrame | int[,] | `public static int[,] scorpionCageFrame = new int[2, cageFrames];` | `public static int[,] scorpionCageFrame = new int[2, cageFrames];` |
| 326 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 829 | 2 | scorpionCageFrameCounter | int[,] | `public static int[,] scorpionCageFrameCounter = new int[2, cageFrames];` | `public static int[,] scorpionCageFrameCounter = new int[2, cageFrames];` |
| 342 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 861 | 2 | fairyJarFrame | int[] | `public static int[] fairyJarFrame = new int[cageFrames];` | `public static int[] fairyJarFrame = new int[cageFrames];` |
| 343 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 863 | 2 | fairyJarFrameCounter | int[] | `public static int[] fairyJarFrameCounter = new int[cageFrames];` | `public static int[] fairyJarFrameCounter = new int[cageFrames];` |
| 347 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 871 | 2 | wormCageFrame | int[] | `public static int[] wormCageFrame = new int[cageFrames];` | `public static int[] wormCageFrame = new int[cageFrames];` |
| 348 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 873 | 2 | wormCageFrameCounter | int[] | `public static int[] wormCageFrameCounter = new int[cageFrames];` | `public static int[] wormCageFrameCounter = new int[cageFrames];` |
| 349 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 875 | 2 | maggotCageFrame | int[] | `public static int[] maggotCageFrame = new int[cageFrames];` | `public static int[] maggotCageFrame = new int[cageFrames];` |
| 350 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 877 | 2 | maggotCageFrameCounter | int[] | `public static int[] maggotCageFrameCounter = new int[cageFrames];` | `public static int[] maggotCageFrameCounter = new int[cageFrames];` |
| 353 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 883 | 2 | ladybugCageFrame | int[] | `public static int[] ladybugCageFrame = new int[cageFrames];` | `public static int[] ladybugCageFrame = new int[cageFrames];` |
| 354 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 885 | 2 | ladybugCageFrameCounter | int[] | `public static int[] ladybugCageFrameCounter = new int[cageFrames];` | `public static int[] ladybugCageFrameCounter = new int[cageFrames];` |
| 361 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 899 | 2 | slugCageFrame | int[,] | `public static int[,] slugCageFrame = new int[3, cageFrames];` | `public static int[,] slugCageFrame = new int[3, cageFrames];` |
| 362 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 901 | 2 | slugCageFrameCounter | int[,] | `public static int[,] slugCageFrameCounter = new int[3, cageFrames];` | `public static int[,] slugCageFrameCounter = new int[3, cageFrames];` |
| 365 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 907 | 2 | grasshopperCageFrame | int[] | `public static int[] grasshopperCageFrame = new int[cageFrames];` | `public static int[] grasshopperCageFrame = new int[cageFrames];` |
| 366 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 909 | 2 | grasshopperCageFrameCounter | int[] | `public static int[] grasshopperCageFrameCounter = new int[cageFrames];` | `public static int[] grasshopperCageFrameCounter = new int[cageFrames];` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`SharedAudioAndSoundData`

- 原报告章节：`4.9.11`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedAudioAndSoundData`
- 细分职责：共享声音播放载荷和音效数据结构。
- 边界角色：`presentation`；最小 seam：sound data projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1283 | field | Terraria.DataStructures.SoundPlaySet | Terraria.DataStructures/SoundPlaySet.cs | D:\TRbackup\Version4\Terraria.DataStructures\SoundPlaySet.cs | 5 | 2 | IntendedCooldown | int | `public int IntendedCooldown;` | `public int IntendedCooldown;` |
| 1284 | field | Terraria.DataStructures.SoundPlaySet | Terraria.DataStructures/SoundPlaySet.cs | D:\TRbackup\Version4\Terraria.DataStructures\SoundPlaySet.cs | 7 | 2 | SoundType | int | `public int SoundType;` | `public int SoundType;` |
| 1285 | field | Terraria.DataStructures.SoundPlaySet | Terraria.DataStructures/SoundPlaySet.cs | D:\TRbackup\Version4\Terraria.DataStructures\SoundPlaySet.cs | 9 | 2 | SoundStyle | int | `public int SoundStyle;` | `public int SoundStyle;` |

#### 属性（0）

无该类型成员记录。


### 4.11 细分子系统：`SharedEffectAndSkyPresentation`

- 原报告章节：`4.9.43`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedEffectAndSkyPresentation`
- 细分职责：Effect、覆盖层和天空表现。
- 边界角色：`presentation`；最小 seam：effect/sky projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：6；声明类型数：6；字段：12；属性：3；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2188 | field | Terraria.GameContent.Skies.CreditsRollSky | Terraria.GameContent.Skies/CreditsRollSky.cs | D:\TRbackup\Version4\Terraria.GameContent.Skies\CreditsRollSky.cs | 12 | 2 | _endTime | int | `private int _endTime;` | `private int _endTime;` |
| 2634 | field | Terraria.Graphics.Effects.EffectManager<T> | Terraria.Graphics.Effects/EffectManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs | 8 | 2 | _isLoaded | bool | `protected bool _isLoaded;` | `protected bool _isLoaded;` |
| 2635 | field | Terraria.Graphics.Effects.EffectManager<T> | Terraria.Graphics.Effects/EffectManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs | 10 | 2 | _effects | System.Collections.Generic.Dictionary<string, T> | `protected Dictionary<string, T> _effects = new Dictionary<string, T>();` | `protected Dictionary<string, T> _effects = new Dictionary<string, T>();` |
| 2636 | field | Terraria.Graphics.Effects.GameEffect | Terraria.Graphics.Effects/GameEffect.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\GameEffect.cs | 7 | 2 | Opacity | float | `public float Opacity;` | `public float Opacity;` |
| 2637 | field | Terraria.Graphics.Effects.GameEffect | Terraria.Graphics.Effects/GameEffect.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\GameEffect.cs | 9 | 2 | _isLoaded | bool | `protected bool _isLoaded;` | `protected bool _isLoaded;` |
| 2638 | field | Terraria.Graphics.Effects.GameEffect | Terraria.Graphics.Effects/GameEffect.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\GameEffect.cs | 11 | 2 | _priority | Terraria.Graphics.Effects.EffectPriority | `protected EffectPriority _priority;` | `protected EffectPriority _priority;` |
| 2639 | field | Terraria.Graphics.Effects.Overlay | Terraria.Graphics.Effects/Overlay.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\Overlay.cs | 8 | 2 | Mode | Terraria.Graphics.Effects.OverlayMode | `public OverlayMode Mode = OverlayMode.Inactive;` | `public OverlayMode Mode = OverlayMode.Inactive;` |
| 2640 | field | Terraria.Graphics.Effects.Overlay | Terraria.Graphics.Effects/Overlay.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\Overlay.cs | 10 | 2 | _layer | Terraria.Graphics.Effects.RenderLayers | `private RenderLayers _layer = RenderLayers.All;` | `private RenderLayers _layer = RenderLayers.All;` |
| 2641 | field | Terraria.Graphics.Effects.OverlayManager | Terraria.Graphics.Effects/OverlayManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\OverlayManager.cs | 10 | 2 | OPACITY_RATE | float | `private const float OPACITY_RATE = 1f;` | `private const float OPACITY_RATE = 1f;` |
| 2642 | field | Terraria.Graphics.Effects.OverlayManager | Terraria.Graphics.Effects/OverlayManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\OverlayManager.cs | 12 | 2 | _activeOverlays | System.Collections.Generic.LinkedList<Terraria.Graphics.Effects.Overlay>[] | `private LinkedList<Overlay>[] _activeOverlays = new LinkedList<Overlay>[Enum.GetNames(typeof(EffectPriority)).Length];` | `private LinkedList<Overlay>[] _activeOverlays = new LinkedList<Overlay>[Enum.GetNames(typeof(EffectPriority)).Length];` |
| 2643 | field | Terraria.Graphics.Effects.SkyManager | Terraria.Graphics.Effects/SkyManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\SkyManager.cs | 9 | 2 | Instance | Terraria.Graphics.Effects.SkyManager | `public static SkyManager Instance = new SkyManager();` | `public static SkyManager Instance = new SkyManager();` |
| 2644 | field | Terraria.Graphics.Effects.SkyManager | Terraria.Graphics.Effects/SkyManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\SkyManager.cs | 11 | 2 | _activeSkies | System.Collections.Generic.LinkedList<Terraria.Graphics.Effects.CustomSky> | `private LinkedList<CustomSky> _activeSkies = new LinkedList<CustomSky>();` | `private LinkedList<CustomSky> _activeSkies = new LinkedList<CustomSky>();` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3829 | property | Terraria.GameContent.Skies.CreditsRollSky | Terraria.GameContent.Skies/CreditsRollSky.cs | D:\TRbackup\Version4\Terraria.GameContent.Skies\CreditsRollSky.cs | 20 | 2 | AmountOfTimeNeededForFullPlay | int | `public int AmountOfTimeNeededForFullPlay => _endTime;` | `public int AmountOfTimeNeededForFullPlay => _endTime;` |
| 3873 | property | Terraria.Graphics.Effects.EffectManager<T> | Terraria.Graphics.Effects/EffectManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs | 12 | 2 | this[] | T | `public T this[string key] { get { if (_effects.TryGetValue(key, out var value)) { return value; } return null; } set { Bind(key, value); } }` | `public T this[string key] { get { if (_effects.TryGetValue(key, out var value)) { return value; } return null; } set { Bind(key, value); } }` |
| 3874 | property | Terraria.Graphics.Effects.Overlay | Terraria.Graphics.Effects/Overlay.cs | D:\TRbackup\Version4\Terraria.Graphics.Effects\Overlay.cs | 12 | 2 | Layer | Terraria.Graphics.Effects.RenderLayers | `public RenderLayers Layer => _layer;` | `public RenderLayers Layer => _layer;` |


### 4.12 细分子系统：`SharedParticlePresentation`

- 原报告章节：`4.9.44`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedParticlePresentation`
- 细分职责：粒子接口、池化粒子和粒子渲染器。
- 边界角色：`presentation`；最小 seam：particle renderer port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：6；属性：2；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2679 | field | Terraria.Graphics.Renderers.ParticleRenderer | Terraria.Graphics.Renderers/ParticleRenderer.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRenderer.cs | 8 | 2 | Settings | Terraria.Graphics.Renderers.ParticleRendererSettings | `public ParticleRendererSettings Settings;` | `public ParticleRendererSettings Settings;` |
| 2680 | field | Terraria.Graphics.Renderers.ParticleRenderer | Terraria.Graphics.Renderers/ParticleRenderer.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRenderer.cs | 10 | 2 | Particles | System.Collections.Generic.List<Terraria.Graphics.Renderers.IParticle> | `public List<IParticle> Particles = new List<IParticle>();` | `public List<IParticle> Particles = new List<IParticle>();` |
| 2681 | field | Terraria.Graphics.Renderers.ParticleRendererSettings | Terraria.Graphics.Renderers/ParticleRendererSettings.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRendererSettings.cs | 7 | 2 | AnchorPosition | Vector2 | `public Vector2 AnchorPosition;` | `public Vector2 AnchorPosition;` |
| 2682 | field | Terraria.Graphics.Renderers.ParticleRepelDetails | Terraria.Graphics.Renderers/ParticleRepelDetails.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRepelDetails.cs | 7 | 2 | SourcePosition | Vector2 | `public Vector2 SourcePosition;` | `public Vector2 SourcePosition;` |
| 2683 | field | Terraria.Graphics.Renderers.ParticleRepelDetails | Terraria.Graphics.Renderers/ParticleRepelDetails.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRepelDetails.cs | 9 | 2 | Radius | float | `public float Radius;` | `public float Radius;` |
| 2684 | field | Terraria.Graphics.Renderers.ParticleRepelDetails | Terraria.Graphics.Renderers/ParticleRepelDetails.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRepelDetails.cs | 11 | 2 | IsInWater | bool | `public bool IsInWater;` | `public bool IsInWater;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3884 | property | Terraria.Graphics.Renderers.IParticle | Terraria.Graphics.Renderers/IParticle.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\IParticle.cs | 7 | 2 | ShouldBeRemovedFromRenderer | bool | `bool ShouldBeRemovedFromRenderer { get; }` | `bool ShouldBeRemovedFromRenderer { get; }` |
| 3885 | property | Terraria.Graphics.Renderers.IPooledParticle | Terraria.Graphics.Renderers/IPooledParticle.cs | D:\TRbackup\Version4\Terraria.Graphics.Renderers\IPooledParticle.cs | 5 | 2 | IsRestingInPool | bool | `bool IsRestingInPool { get; }` | `bool IsRestingInPool { get; }` |


### 4.13 细分子系统：`SharedBackgroundPresentationDefinitions`

- 原报告章节：`4.9.59`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedBackgroundPresentationDefinitions`
- 细分职责：背景变体和背景集合定义。
- 边界角色：`definition/presentation`；最小 seam：background definition view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1064 | field | Terraria.DataStructures.BackgroundVariant | Terraria.DataStructures/BackgroundVariant.cs | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariant.cs | 5 | 2 | _backgrounds | int[] | `private readonly int[] _backgrounds = new int[3] { -1, -1, -1 };` | `private readonly int[] _backgrounds = new int[3] { -1, -1, -1 };` |
| 1065 | field | Terraria.DataStructures.BackgroundVariantSet | Terraria.DataStructures/BackgroundVariantSet.cs | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 5 | 2 | Pure | Terraria.DataStructures.BackgroundVariant | `public BackgroundVariant Pure = new BackgroundVariant();` | `public BackgroundVariant Pure = new BackgroundVariant();` |
| 1066 | field | Terraria.DataStructures.BackgroundVariantSet | Terraria.DataStructures/BackgroundVariantSet.cs | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 7 | 2 | Corrupt | Terraria.DataStructures.BackgroundVariant | `public BackgroundVariant Corrupt = new BackgroundVariant();` | `public BackgroundVariant Corrupt = new BackgroundVariant();` |
| 1067 | field | Terraria.DataStructures.BackgroundVariantSet | Terraria.DataStructures/BackgroundVariantSet.cs | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 9 | 2 | Crimson | Terraria.DataStructures.BackgroundVariant | `public BackgroundVariant Crimson = new BackgroundVariant();` | `public BackgroundVariant Crimson = new BackgroundVariant();` |
| 1068 | field | Terraria.DataStructures.BackgroundVariantSet | Terraria.DataStructures/BackgroundVariantSet.cs | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 11 | 2 | Hallow | Terraria.DataStructures.BackgroundVariant | `public BackgroundVariant Hallow = new BackgroundVariant();` | `public BackgroundVariant Hallow = new BackgroundVariant();` |

#### 属性（0）

无该类型成员记录。


### 4.14 细分子系统：`SharedStarParticleState`

- 原报告章节：`4.9.138`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedWeatherParticleState`
- 细分职责：星体位置、坠落、闪烁和淡入动画状态。
- 边界角色：`presentation state`；最小 seam：star particle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3491 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 9 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3492 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 11 | 2 | scale | float | `public float scale;` | `public float scale;` |
| 3493 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 13 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3494 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 15 | 2 | type | int | `public int type;` | `public int type;` |
| 3495 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 17 | 2 | twinkle | float | `public float twinkle;` | `public float twinkle;` |
| 3496 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 19 | 2 | twinkleSpeed | float | `public float twinkleSpeed;` | `public float twinkleSpeed;` |
| 3497 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 21 | 2 | rotationSpeed | float | `public float rotationSpeed;` | `public float rotationSpeed;` |
| 3498 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 23 | 2 | falling | bool | `public bool falling;` | `public bool falling;` |
| 3499 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 25 | 2 | hidden | bool | `public bool hidden;` | `public bool hidden;` |
| 3500 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 27 | 2 | fallSpeed | Vector2 | `public Vector2 fallSpeed;` | `public Vector2 fallSpeed;` |
| 3501 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 29 | 2 | fallTime | int | `public int fallTime;` | `public int fallTime;` |
| 3502 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 31 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3503 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 33 | 2 | starfallBoost | float | `public static float starfallBoost = 1f;` | `public static float starfallBoost = 1f;` |
| 3504 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 35 | 2 | starFallCount | int | `public static int starFallCount = 0;` | `public static int starFallCount = 0;` |
| 3505 | field | Terraria.Star | Terraria/Star.cs | D:\TRbackup\Version4\Terraria\Star.cs | 37 | 2 | fadeIn | float | `public float fadeIn;` | `public float fadeIn;` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`SharedCloudAndRainParticleState`

- 原报告章节：`4.9.139`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedWeatherParticleState`
- 细分职责：云层和雨滴位置、速度、透明度及回收状态。
- 边界角色：`presentation state`；最小 seam：cloud rain particle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：21；属性：0；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2998 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 10 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 2999 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 12 | 2 | scale | float | `public float scale;` | `public float scale;` |
| 3000 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 14 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3001 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 16 | 2 | rSpeed | float | `public float rSpeed;` | `public float rSpeed;` |
| 3002 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 18 | 2 | sSpeed | float | `public float sSpeed;` | `public float sSpeed;` |
| 3003 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 20 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 3004 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 22 | 2 | spriteDir | SpriteEffects | `public SpriteEffects spriteDir;` | `public SpriteEffects spriteDir;` |
| 3005 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 24 | 2 | type | int | `public int type;` | `public int type;` |
| 3006 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 26 | 2 | width | int | `public int width;` | `public int width;` |
| 3007 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 28 | 2 | height | int | `public int height;` | `public int height;` |
| 3008 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 30 | 2 | Alpha | float | `public float Alpha;` | `public float Alpha;` |
| 3009 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 32 | 2 | kill | bool | `public bool kill;` | `public bool kill;` |
| 3010 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 34 | 2 | rand | Terraria.Utilities.UnifiedRandom | `private static UnifiedRandom rand = new UnifiedRandom();` | `private static UnifiedRandom rand = new UnifiedRandom();` |
| 3011 | field | Terraria.Cloud | Terraria/Cloud.cs | D:\TRbackup\Version4\Terraria\Cloud.cs | 36 | 2 | lastCameraCenter | Vector2? | `public static Vector2? lastCameraCenter;` | `public static Vector2? lastCameraCenter;` |
| 3391 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 8 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3392 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 10 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3393 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 12 | 2 | scale | float | `public float scale;` | `public float scale;` |
| 3394 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 14 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3395 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 16 | 2 | alpha | int | `public int alpha;` | `public int alpha;` |
| 3396 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 18 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 3397 | field | Terraria.Rain | Terraria/Rain.cs | D:\TRbackup\Version4\Terraria\Rain.cs | 20 | 2 | type | byte | `public byte type;` | `public byte type;` |

#### 属性（0）

无该类型成员记录。


### 4.16 细分子系统：`SharedDustParticleState`

- 原报告章节：`4.9.146`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedParticleAndGoreEffects`
- 细分职责：Dust 粒子运动、光照、shader 和帧状态。
- 边界角色：`presentation state`；最小 seam：dust particle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3042 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 13 | 2 | dustIndex | int | `public int dustIndex;` | `public int dustIndex;` |
| 3043 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 15 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3044 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 17 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3045 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 19 | 2 | fadeIn | float | `public float fadeIn;` | `public float fadeIn;` |
| 3046 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 21 | 2 | noGravity | bool | `public bool noGravity;` | `public bool noGravity;` |
| 3047 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 23 | 2 | scale | float | `public float scale;` | `public float scale;` |
| 3048 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 25 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3049 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 27 | 2 | noLight | bool | `public bool noLight;` | `public bool noLight;` |
| 3050 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 29 | 2 | noLightEmittance | bool | `public bool noLightEmittance;` | `public bool noLightEmittance;` |
| 3051 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 31 | 2 | fullBright | bool | `public bool fullBright;` | `public bool fullBright;` |
| 3052 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 33 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 3053 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 35 | 2 | type | int | `public int type;` | `public int type;` |
| 3054 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 37 | 2 | color | Color | `public Color color;` | `public Color color;` |
| 3055 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 39 | 2 | alpha | int | `public int alpha;` | `public int alpha;` |
| 3056 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 41 | 2 | frame | Rectangle | `public Rectangle frame;` | `public Rectangle frame;` |
| 3057 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 43 | 2 | shader | Terraria.Graphics.Shaders.ArmorShaderData | `public ArmorShaderData shader;` | `public ArmorShaderData shader;` |
| 3058 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 45 | 2 | customData | object | `public object customData;` | `public object customData;` |
| 3059 | field | Terraria.Dust | Terraria/Dust.cs | D:\TRbackup\Version4\Terraria\Dust.cs | 47 | 2 | firstFrame | bool | `public bool firstFrame;` | `public bool firstFrame;` |

#### 属性（0）

无该类型成员记录。


### 4.17 细分子系统：`SharedGoreEffectState`

- 原报告章节：`4.9.147`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedParticleAndGoreEffects`
- 细分职责：Gore 运动、寿命、贴图帧和回收状态。
- 边界角色：`presentation state`；最小 seam：gore effect port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3106 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 16 | 2 | goreTime | int | `public static int goreTime = 600;` | `public static int goreTime = 600;` |
| 3107 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 18 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3108 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 20 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3109 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 22 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3110 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 24 | 2 | scale | float | `public float scale;` | `public float scale;` |
| 3111 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 26 | 2 | alpha | int | `public int alpha;` | `public int alpha;` |
| 3112 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 28 | 2 | type | int | `public int type;` | `public int type;` |
| 3113 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 30 | 2 | light | float | `public float light;` | `public float light;` |
| 3114 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 32 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 3115 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 34 | 2 | sticky | bool | `public bool sticky = true;` | `public bool sticky = true;` |
| 3116 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 36 | 2 | timeLeft | int | `public int timeLeft = goreTime;` | `public int timeLeft = goreTime;` |
| 3117 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 38 | 2 | behindTiles | bool | `public bool behindTiles;` | `public bool behindTiles;` |
| 3118 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 40 | 2 | frameCounter | byte | `public byte frameCounter;` | `public byte frameCounter;` |
| 3119 | field | Terraria.Gore | Terraria/Gore.cs | D:\TRbackup\Version4\Terraria\Gore.cs | 42 | 2 | Frame | Terraria.DataStructures.SpriteFrame | `public SpriteFrame Frame = new SpriteFrame(1, 1);` | `public SpriteFrame Frame = new SpriteFrame(1, 1);` |

#### 属性（0）

无该类型成员记录。


### 4.18 细分子系统：`SharedSceneDecorationAndAudioState`

- 原报告章节：`4.9.176`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedSceneMetricsSnapshot`
- 上一级 peer 细分子系统：`SharedSceneAggregateAndDecorationState`
- 细分职责：场景音乐、蜡烛、纪念碑、装饰物和音频标记。
- 边界角色：`presentation/query`；最小 seam：scene decoration audio port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：20；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3987 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 120 | 2 | ActiveMusicBox | int | `public int ActiveMusicBox { get; set; }` | `public int ActiveMusicBox { get; set; }` |
| 3988 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 122 | 2 | MusicBoxSilence | bool | `public bool MusicBoxSilence { get; set; }` | `public bool MusicBoxSilence { get; set; }` |
| 4000 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 146 | 2 | HasSunflower | bool | `public bool HasSunflower { get; private set; }` | `public bool HasSunflower { get; private set; }` |
| 4001 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 148 | 2 | HasGardenGnome | bool | `public bool HasGardenGnome { get; private set; }` | `public bool HasGardenGnome { get; private set; }` |
| 4002 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 150 | 2 | HasClock | bool | `public bool HasClock { get; private set; }` | `public bool HasClock { get; private set; }` |
| 4003 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 152 | 2 | HasCampfire | bool | `public bool HasCampfire { get; private set; }` | `public bool HasCampfire { get; private set; }` |
| 4004 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 154 | 2 | HasStarInBottle | bool | `public bool HasStarInBottle { get; private set; }` | `public bool HasStarInBottle { get; private set; }` |
| 4005 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 156 | 2 | HasHeartLantern | bool | `public bool HasHeartLantern { get; private set; }` | `public bool HasHeartLantern { get; private set; }` |
| 4006 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 158 | 2 | ActiveFountainColor | int | `public int ActiveFountainColor { get; private set; }` | `public int ActiveFountainColor { get; private set; }` |
| 4007 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 160 | 2 | ActiveMonolithType | int | `public int ActiveMonolithType { get; private set; }` | `public int ActiveMonolithType { get; private set; }` |
| 4008 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 162 | 2 | BloodMoonMonolith | bool | `public bool BloodMoonMonolith { get; private set; }` | `public bool BloodMoonMonolith { get; private set; }` |
| 4009 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 164 | 2 | MoonLordMonolith | bool | `public bool MoonLordMonolith { get; private set; }` | `public bool MoonLordMonolith { get; private set; }` |
| 4010 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 166 | 2 | EchoMonolith | bool | `public bool EchoMonolith { get; private set; }` | `public bool EchoMonolith { get; private set; }` |
| 4011 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 168 | 2 | ShimmerMonolithState | int | `public int ShimmerMonolithState { get; private set; }` | `public int ShimmerMonolithState { get; private set; }` |
| 4012 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 170 | 2 | CRTMonolith | bool | `public bool CRTMonolith { get; private set; }` | `public bool CRTMonolith { get; private set; }` |
| 4013 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 172 | 2 | RetroMonolith | bool | `public bool RetroMonolith { get; private set; }` | `public bool RetroMonolith { get; private set; }` |
| 4014 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 174 | 2 | NoirMonolith | bool | `public bool NoirMonolith { get; private set; }` | `public bool NoirMonolith { get; private set; }` |
| 4015 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 176 | 2 | RadioThingMonolith | bool | `public bool RadioThingMonolith { get; private set; }` | `public bool RadioThingMonolith { get; private set; }` |
| 4016 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 178 | 2 | HasCatBast | bool | `public bool HasCatBast { get; private set; }` | `public bool HasCatBast { get; private set; }` |
| 4020 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 186 | 2 | BehindBackwall | bool | `public bool BehindBackwall { get; private set; }` | `public bool BehindBackwall { get; private set; }` |


### 4.19 细分子系统：`SharedPopupTextContentAndContextState`

- 原报告章节：`4.9.180`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedPopupAndCombatText`
- 上一级 peer 细分子系统：`SharedPopupTextState`
- 细分职责：弹出文本内容、金币、声纳和来源上下文。
- 边界角色：`presentation`；最小 seam：popup text content context port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3352 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 12 | 2 | maxItemText | int | `public const int maxItemText = 20;` | `public const int maxItemText = 20;` |
| 3353 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 14 | 2 | popupText | Terraria.PopupText[] | `public static PopupText[] popupText = new PopupText[20];` | `public static PopupText[] popupText = new PopupText[20];` |
| 3358 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 24 | 2 | name | string | `public string name;` | `public string name;` |
| 3359 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 26 | 2 | displayText | string | `public string displayText;` | `public string displayText;` |
| 3360 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 28 | 2 | stack | long | `public long stack;` | `public long stack;` |
| 3369 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 46 | 2 | coinText | bool | `public bool coinText;` | `public bool coinText;` |
| 3370 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 48 | 2 | coinValue | long | `public long coinValue;` | `public long coinValue;` |
| 3371 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 50 | 2 | sonarText | int | `public static int sonarText = -1;` | `public static int sonarText = -1;` |
| 3372 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 52 | 2 | expert | bool | `public bool expert;` | `public bool expert;` |
| 3373 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 54 | 2 | master | bool | `public bool master;` | `public bool master;` |
| 3374 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 56 | 2 | sonar | bool | `public bool sonar;` | `public bool sonar;` |
| 3375 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 58 | 2 | context | Terraria.PopupTextContext | `public PopupTextContext context;` | `public PopupTextContext context;` |
| 3376 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 60 | 2 | npcNetID | int | `public int npcNetID;` | `public int npcNetID;` |
| 3377 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 62 | 2 | freeAdvanced | bool | `public bool freeAdvanced;` | `public bool freeAdvanced;` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`SharedPopupTextRenderLifecycleState`

- 原报告章节：`4.9.181`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`SharedPopupAndCombatText`
- 上一级 peer 细分子系统：`SharedPopupTextState`
- 细分职责：弹出文本位置、透明度、生命周期和渲染缓存。
- 边界角色：`presentation state`；最小 seam：popup text render lifecycle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3354 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 16 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3355 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 18 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3356 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 20 | 2 | alpha | float | `public float alpha;` | `public float alpha;` |
| 3357 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 22 | 2 | alphaDir | int | `public int alphaDir = 1;` | `public int alphaDir = 1;` |
| 3361 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 30 | 2 | scale | float | `public float scale = 1f;` | `public float scale = 1f;` |
| 3362 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 32 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3363 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 34 | 2 | color | Color | `public Color color;` | `public Color color;` |
| 3364 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 36 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 3365 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 38 | 2 | lifeTime | int | `public int lifeTime;` | `public int lifeTime;` |
| 3366 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 40 | 2 | framesSinceSpawn | int | `public int framesSinceSpawn;` | `public int framesSinceSpawn;` |
| 3367 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 42 | 2 | numActive | int | `public static int numActive;` | `public static int numActive;` |
| 3368 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 44 | 2 | NoStack | bool | `public bool NoStack;` | `public bool NoStack;` |
| 3378 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 64 | 2 | charOffsets | Vector2[] | `public Vector2[] charOffsets;` | `public Vector2[] charOffsets;` |
| 3379 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 66 | 2 | charColors | Color[] | `public Color[] charColors;` | `public Color[] charColors;` |
| 3380 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 68 | 2 | effectStyle | Terraria.GameContent.PopupEffectStyle | `public PopupEffectStyle effectStyle;` | `public PopupEffectStyle effectStyle;` |
| 3381 | field | Terraria.PopupText | Terraria/PopupText.cs | D:\TRbackup\Version4\Terraria\PopupText.cs | 70 | 2 | effectIntensity | int | `public int effectIntensity;` | `public int effectIntensity;` |

#### 属性（0）

无该类型成员记录。


### 4.21 细分子系统：`AudioDefinitionAndTrackState`

- 原报告章节：`4.11.2`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioDefinitionAndTrackState`
- 细分职责：声音样式、音频轨道和播放覆盖定义。
- 边界角色：`definition/presentation`；最小 seam：audio definition/track view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：10；属性：11；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4167 | field | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 8 | 2 | Random | Terraria.Utilities.UnifiedRandom | `private static readonly UnifiedRandom Random = new UnifiedRandom();` | `private static readonly UnifiedRandom Random = new UnifiedRandom();` |
| 4168 | field | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 10 | 2 | _style | int | `private readonly int _style;` | `private readonly int _style;` |
| 4169 | field | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 12 | 2 | Variations | int | `public readonly int Variations;` | `public readonly int Variations;` |
| 4170 | field | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 14 | 2 | SoundId | int | `public readonly int SoundId;` | `public readonly int SoundId;` |
| 4171 | field | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 16 | 2 | _maxTrackedInstances | int | `public readonly int _maxTrackedInstances;` | `public readonly int _maxTrackedInstances;` |
| 4175 | field | Terraria.Audio.SoundPlayOverrides | Terraria.Audio/SoundPlayOverrides.cs | D:\TRbackup\Version4\Terraria.Audio\SoundPlayOverrides.cs | 5 | 2 | Volume | float? | `public float? Volume;` | `public float? Volume;` |
| 4176 | field | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 8 | 2 | _random | Terraria.Utilities.UnifiedRandom | `private static UnifiedRandom _random = new UnifiedRandom();` | `private static UnifiedRandom _random = new UnifiedRandom();` |
| 4177 | field | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 10 | 2 | _volume | float | `private float _volume;` | `private float _volume;` |
| 4178 | field | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 12 | 2 | _pitchVariance | float | `private float _pitchVariance;` | `private float _pitchVariance;` |
| 4179 | field | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 14 | 2 | _type | Terraria.Audio.SoundType | `private SoundType _type;` | `private SoundType _type;` |

#### 属性（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4500 | property | Terraria.Audio.IAudioTrack | Terraria.Audio/IAudioTrack.cs | D:\TRbackup\Version4\Terraria.Audio\IAudioTrack.cs | 8 | 2 | IsPlaying | bool | `bool IsPlaying { get; }` | `bool IsPlaying { get; }` |
| 4501 | property | Terraria.Audio.IAudioTrack | Terraria.Audio/IAudioTrack.cs | D:\TRbackup\Version4\Terraria.Audio\IAudioTrack.cs | 10 | 2 | IsStopped | bool | `bool IsStopped { get; }` | `bool IsStopped { get; }` |
| 4502 | property | Terraria.Audio.IAudioTrack | Terraria.Audio/IAudioTrack.cs | D:\TRbackup\Version4\Terraria.Audio\IAudioTrack.cs | 12 | 2 | IsPaused | bool | `bool IsPaused { get; }` | `bool IsPaused { get; }` |
| 4503 | property | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 18 | 2 | Style | int | `public int Style { get { if (Variations != 1) { return Random.Next(_style, _style + Variations); } return _style; } }` | `public int Style { get { if (Variations != 1) { return Random.Next(_style, _style + Variations); } return _style; } }` |
| 4504 | property | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 30 | 2 | IsTrackable | bool | `public override bool IsTrackable => SoundId == 42;` | `public override bool IsTrackable => SoundId == 42;` |
| 4505 | property | Terraria.Audio.LegacySoundStyle | Terraria.Audio/LegacySoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 32 | 2 | MaxTrackedInstances | int | `public override int MaxTrackedInstances => _maxTrackedInstances;` | `public override int MaxTrackedInstances => _maxTrackedInstances;` |
| 4507 | property | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 16 | 2 | Volume | float | `public float Volume => _volume;` | `public float Volume => _volume;` |
| 4508 | property | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 18 | 2 | PitchVariance | float | `public float PitchVariance => _pitchVariance;` | `public float PitchVariance => _pitchVariance;` |
| 4509 | property | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 20 | 2 | Type | Terraria.Audio.SoundType | `public SoundType Type => _type;` | `public SoundType Type => _type;` |
| 4510 | property | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 22 | 2 | IsTrackable | bool | `public abstract bool IsTrackable { get; }` | `public abstract bool IsTrackable { get; }` |
| 4511 | property | Terraria.Audio.SoundStyle | Terraria.Audio/SoundStyle.cs | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 24 | 2 | MaxTrackedInstances | int | `public abstract int MaxTrackedInstances { get; }` | `public abstract int MaxTrackedInstances { get; }` |


### 4.22 细分子系统：`AudioActiveSoundState`

- 原报告章节：`4.11.3`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioActiveSoundState`
- 细分职责：活动声音实例和特定环境声音跟踪。
- 边界角色：`presentation`；最小 seam：active sound projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：6；属性：3；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4089 | field | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 10 | 2 | IsGlobal | bool | `public readonly bool IsGlobal;` | `public readonly bool IsGlobal;` |
| 4090 | field | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 12 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 4091 | field | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 14 | 2 | Volume | float | `public float Volume;` | `public float Volume;` |
| 4092 | field | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 16 | 2 | Pitch | float | `public float Pitch;` | `public float Pitch;` |
| 4093 | field | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 18 | 2 | Condition | Terraria.Audio.ActiveSound.LoopedPlayCondition | `public LoopedPlayCondition Condition;` | `public LoopedPlayCondition Condition;` |
| 4180 | field | Terraria.Audio.VampireSizzleTracker | Terraria.Audio/VampireSizzleTracker.cs | D:\TRbackup\Version4\Terraria.Audio\VampireSizzleTracker.cs | 5 | 2 | _playerIndex | int | `private int _playerIndex;` | `private int _playerIndex;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4497 | property | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 20 | 2 | Sound | SoundEffectInstance | `public SoundEffectInstance Sound { get; private set; }` | `public SoundEffectInstance Sound { get; private set; }` |
| 4498 | property | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 22 | 2 | Style | Terraria.Audio.SoundStyle | `public SoundStyle Style { get; private set; }` | `public SoundStyle Style { get; private set; }` |
| 4499 | property | Terraria.Audio.ActiveSound | Terraria.Audio/ActiveSound.cs | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 24 | 2 | IsPlaying | bool | `public bool IsPlaying { get { if (Sound != null) { return Sound.State == SoundState.Playing; } return false; } }` | `public bool IsPlaying { get { if (Sound != null) { return Sound.State == SoundState.Playing; } return false; } }` |


### 4.23 细分子系统：`CinematicTimelineState`

- 原报告章节：`4.11.4`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`CinematicTimelineState`
- 细分职责：电影序列、帧事件和播放状态。
- 边界角色：`presentation`；最小 seam：cinematic projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：13；属性：11；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4181 | field | Terraria.Cinematics.CinematicManager | Terraria.Cinematics/CinematicManager.cs | D:\TRbackup\Version4\Terraria.Cinematics\CinematicManager.cs | 8 | 2 | Instance | Terraria.Cinematics.CinematicManager | `public static CinematicManager Instance = new CinematicManager();` | `public static CinematicManager Instance = new CinematicManager();` |
| 4182 | field | Terraria.Cinematics.CinematicManager | Terraria.Cinematics/CinematicManager.cs | D:\TRbackup\Version4\Terraria.Cinematics\CinematicManager.cs | 10 | 2 | _films | System.Collections.Generic.List<Terraria.Cinematics.Film> | `private List<Film> _films = new List<Film>();` | `private List<Film> _films = new List<Film>();` |
| 4183 | field | Terraria.Cinematics.Film.Sequence | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 11 | 3 | _frameEvent | Terraria.Cinematics.FrameEvent | `private FrameEvent _frameEvent;` | `private FrameEvent _frameEvent;` |
| 4184 | field | Terraria.Cinematics.Film.Sequence | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 13 | 3 | _duration | int | `private int _duration;` | `private int _duration;` |
| 4185 | field | Terraria.Cinematics.Film.Sequence | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 15 | 3 | _start | int | `private int _start;` | `private int _start;` |
| 4186 | field | Terraria.Cinematics.Film | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 31 | 2 | _frame | int | `private int _frame;` | `private int _frame;` |
| 4187 | field | Terraria.Cinematics.Film | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 33 | 2 | _frameCount | int | `private int _frameCount;` | `private int _frameCount;` |
| 4188 | field | Terraria.Cinematics.Film | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 35 | 2 | _nextSequenceAppendTime | int | `private int _nextSequenceAppendTime;` | `private int _nextSequenceAppendTime;` |
| 4189 | field | Terraria.Cinematics.Film | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 37 | 2 | _isActive | bool | `private bool _isActive;` | `private bool _isActive;` |
| 4190 | field | Terraria.Cinematics.Film | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 39 | 2 | _sequences | System.Collections.Generic.List<Terraria.Cinematics.Film.Sequence> | `private List<Sequence> _sequences = new List<Sequence>();` | `private List<Sequence> _sequences = new List<Sequence>();` |
| 4191 | field | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 5 | 2 | _absoluteFrame | int | `private int _absoluteFrame;` | `private int _absoluteFrame;` |
| 4192 | field | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 7 | 2 | _start | int | `private int _start;` | `private int _start;` |
| 4193 | field | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 9 | 2 | _duration | int | `private int _duration;` | `private int _duration;` |

#### 属性（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4512 | property | Terraria.Cinematics.Film.Sequence | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 17 | 3 | Event | Terraria.Cinematics.FrameEvent | `public FrameEvent Event => _frameEvent;` | `public FrameEvent Event => _frameEvent;` |
| 4513 | property | Terraria.Cinematics.Film.Sequence | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 19 | 3 | Duration | int | `public int Duration => _duration;` | `public int Duration => _duration;` |
| 4514 | property | Terraria.Cinematics.Film.Sequence | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 21 | 3 | Start | int | `public int Start => _start;` | `public int Start => _start;` |
| 4515 | property | Terraria.Cinematics.Film | Terraria.Cinematics/Film.cs | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 41 | 2 | IsActive | bool | `public bool IsActive => _isActive;` | `public bool IsActive => _isActive;` |
| 4516 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 11 | 2 | AbsoluteFrame | int | `public int AbsoluteFrame => _absoluteFrame;` | `public int AbsoluteFrame => _absoluteFrame;` |
| 4517 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 13 | 2 | Start | int | `public int Start => _start;` | `public int Start => _start;` |
| 4518 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 15 | 2 | Duration | int | `public int Duration => _duration;` | `public int Duration => _duration;` |
| 4519 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 17 | 2 | Frame | int | `public int Frame => _absoluteFrame - _start;` | `public int Frame => _absoluteFrame - _start;` |
| 4520 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 19 | 2 | IsFirstFrame | bool | `public bool IsFirstFrame => _start == _absoluteFrame;` | `public bool IsFirstFrame => _start == _absoluteFrame;` |
| 4521 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 21 | 2 | IsLastFrame | bool | `public bool IsLastFrame => Remaining == 0;` | `public bool IsLastFrame => Remaining == 0;` |
| 4522 | property | Terraria.Cinematics.FrameEventData | Terraria.Cinematics/FrameEventData.cs | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 23 | 2 | Remaining | int | `public int Remaining => _start + _duration - _absoluteFrame - 1;` | `public int Remaining => _start + _duration - _absoluteFrame - 1;` |


### 4.24 细分子系统：`AudioPlaybackCoordinatorState`

- 原报告章节：`4.11.7`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioPlaybackCoordinatorState`
- 细分职责：音频引擎和播放协调器状态。
- 边界角色：`presentation/adapter`；最小 seam：audio playback coordinator；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：1；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4172 | field | Terraria.Audio.SoundEngine | Terraria.Audio/SoundEngine.cs | D:\TRbackup\Version4\Terraria.Audio\SoundEngine.cs | 11 | 2 | LegacySoundPlayer | Terraria.Audio.LegacySoundPlayer | `public static LegacySoundPlayer LegacySoundPlayer;` | `public static LegacySoundPlayer LegacySoundPlayer;` |
| 4173 | field | Terraria.Audio.SoundEngine | Terraria.Audio/SoundEngine.cs | D:\TRbackup\Version4\Terraria.Audio\SoundEngine.cs | 13 | 2 | SoundPlayer | Terraria.Audio.SoundPlayer | `public static SoundPlayer SoundPlayer;` | `public static SoundPlayer SoundPlayer;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4506 | property | Terraria.Audio.SoundEngine | Terraria.Audio/SoundEngine.cs | D:\TRbackup\Version4\Terraria.Audio\SoundEngine.cs | 15 | 2 | IsAudioSupported | bool | `public static bool IsAudioSupported { get; private set; }` | `public static bool IsAudioSupported { get; private set; }` |


### 4.25 细分子系统：`AudioTrackedSoundState`

- 原报告章节：`4.11.8`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioTrackedSoundState`
- 细分职责：被跟踪声音集合和声音播放器状态。
- 边界角色：`presentation/state`；最小 seam：tracked sound port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：1；属性：0；合计：1。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4174 | field | Terraria.Audio.SoundPlayer | Terraria.Audio/SoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\SoundPlayer.cs | 9 | 2 | _trackedSounds | SlotVector<Terraria.Audio.ActiveSound> | `private readonly SlotVector<ActiveSound> _trackedSounds = new SlotVector<ActiveSound>(4096);` | `private readonly SlotVector<ActiveSound> _trackedSounds = new SlotVector<ActiveSound>(4096);` |

#### 属性（0）

无该类型成员记录。


### 4.26 细分子系统：`SharedAudioLegacyTrackedInstanceState`

- 原报告章节：`4.11.26`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`AudioLegacySoundInstanceState`
- 细分职责：旧音效可跟踪实例集合、回收和活动引用状态。
- 边界角色：`presentation/adapter`；最小 seam：legacy tracked instance port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4164 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 154 | 2 | TrackableSoundInstances | SoundEffectInstance[] | `public SoundEffectInstance[] TrackableSoundInstances;` | `public SoundEffectInstance[] TrackableSoundInstances;` |
| 4166 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 158 | 2 | _trackedInstances | System.Collections.Generic.List<SoundEffectInstance> | `private List<SoundEffectInstance> _trackedInstances;` | `private List<SoundEffectInstance> _trackedInstances;` |

#### 属性（0）

无该类型成员记录。


### 4.27 细分子系统：`SharedAudioLegacySoundServicesState`

- 原报告章节：`4.11.27`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`AudioLegacySoundCatalogState`
- 细分职责：旧音效 TrackableSounds、服务集合和外部播放适配。
- 边界角色：`presentation/adapter`；最小 seam：legacy sound services port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4163 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 152 | 2 | TrackableSounds | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] TrackableSounds;` | `public Asset<SoundEffect>[] TrackableSounds;` |
| 4165 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 156 | 2 | _services | System.IServiceProvider | `private readonly IServiceProvider _services;` | `private readonly IServiceProvider _services;` |

#### 属性（0）

无该类型成员记录。


### 4.28 细分子系统：`SharedAudioLegacyPlayerAndInterfaceInstanceState`

- 原报告章节：`4.11.30`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`SharedAudioLegacySoundCatalogInstanceState`
- 细分职责：旧音效玩家反馈、菜单、相机和界面声音实例。
- 边界角色：`presentation/adapter`；最小 seam：legacy player interface instance port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4111 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 48 | 2 | SoundInstancePlayerHit | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstancePlayerHit = new SoundEffectInstance[3];` | `public SoundEffectInstance[] SoundInstancePlayerHit = new SoundEffectInstance[3];` |
| 4113 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 52 | 2 | SoundInstanceFemaleHit | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceFemaleHit = new SoundEffectInstance[3];` | `public SoundEffectInstance[] SoundInstanceFemaleHit = new SoundEffectInstance[3];` |
| 4115 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 56 | 2 | SoundInstancePlayerKilled | SoundEffectInstance | `public SoundEffectInstance SoundInstancePlayerKilled;` | `public SoundEffectInstance SoundInstancePlayerKilled;` |
| 4134 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 94 | 2 | SoundInstanceMenuOpen | SoundEffectInstance | `public SoundEffectInstance SoundInstanceMenuOpen;` | `public SoundEffectInstance SoundInstanceMenuOpen;` |
| 4136 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 98 | 2 | SoundInstanceMenuClose | SoundEffectInstance | `public SoundEffectInstance SoundInstanceMenuClose;` | `public SoundEffectInstance SoundInstanceMenuClose;` |
| 4138 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 102 | 2 | SoundInstanceMenuTick | SoundEffectInstance | `public SoundEffectInstance SoundInstanceMenuTick;` | `public SoundEffectInstance SoundInstanceMenuTick;` |
| 4142 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 110 | 2 | SoundInstanceCamera | SoundEffectInstance | `public SoundEffectInstance SoundInstanceCamera;` | `public SoundEffectInstance SoundInstanceCamera;` |
| 4158 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 142 | 2 | SoundInstanceChat | SoundEffectInstance | `public SoundEffectInstance SoundInstanceChat;` | `public SoundEffectInstance SoundInstanceChat;` |
| 4160 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 146 | 2 | SoundInstanceMaxMana | SoundEffectInstance | `public SoundEffectInstance SoundInstanceMaxMana;` | `public SoundEffectInstance SoundInstanceMaxMana;` |

#### 属性（0）

无该类型成员记录。


### 4.29 细分子系统：`SharedAudioLegacyGameplaySoundDefinitionCatalogState`

- 原报告章节：`4.11.31`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`SharedAudioLegacySoundDefinitionCatalogState`
- 细分职责：旧音效环境、世界交互和实体反馈声音定义目录。
- 边界角色：`definition/catalog`；最小 seam：legacy gameplay sound definition port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：25；属性：0；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（25）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4094 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 14 | 2 | SoundDrip | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundDrip = new Asset<SoundEffect>[3];` | `public Asset<SoundEffect>[] SoundDrip = new Asset<SoundEffect>[3];` |
| 4096 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 18 | 2 | SoundLiquid | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundLiquid = new Asset<SoundEffect>[2];` | `public Asset<SoundEffect>[] SoundLiquid = new Asset<SoundEffect>[2];` |
| 4098 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 22 | 2 | SoundMech | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundMech = new Asset<SoundEffect>[1];` | `public Asset<SoundEffect>[] SoundMech = new Asset<SoundEffect>[1];` |
| 4100 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 26 | 2 | SoundDig | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundDig = new Asset<SoundEffect>[3];` | `public Asset<SoundEffect>[] SoundDig = new Asset<SoundEffect>[3];` |
| 4102 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 30 | 2 | SoundThunder | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundThunder = new Asset<SoundEffect>[6];` | `public Asset<SoundEffect>[] SoundThunder = new Asset<SoundEffect>[6];` |
| 4104 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 34 | 2 | SoundResearch | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundResearch = new Asset<SoundEffect>[4];` | `public Asset<SoundEffect>[] SoundResearch = new Asset<SoundEffect>[4];` |
| 4106 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 38 | 2 | SoundTink | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundTink = new Asset<SoundEffect>[3];` | `public Asset<SoundEffect>[] SoundTink = new Asset<SoundEffect>[3];` |
| 4108 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 42 | 2 | SoundCoin | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundCoin = new Asset<SoundEffect>[5];` | `public Asset<SoundEffect>[] SoundCoin = new Asset<SoundEffect>[5];` |
| 4116 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 58 | 2 | SoundGrass | Asset<SoundEffect> | `public Asset<SoundEffect> SoundGrass;` | `public Asset<SoundEffect> SoundGrass;` |
| 4118 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 62 | 2 | SoundGrab | Asset<SoundEffect> | `public Asset<SoundEffect> SoundGrab;` | `public Asset<SoundEffect> SoundGrab;` |
| 4120 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 66 | 2 | SoundPixie | Asset<SoundEffect> | `public Asset<SoundEffect> SoundPixie;` | `public Asset<SoundEffect> SoundPixie;` |
| 4122 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 70 | 2 | SoundItem | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundItem = new Asset<SoundEffect>[SoundID.ItemSoundCount];` | `public Asset<SoundEffect>[] SoundItem = new Asset<SoundEffect>[SoundID.ItemSoundCount];` |
| 4124 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 74 | 2 | SoundNpcHit | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundNpcHit = new Asset<SoundEffect>[59];` | `public Asset<SoundEffect>[] SoundNpcHit = new Asset<SoundEffect>[59];` |
| 4126 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 78 | 2 | SoundNpcKilled | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundNpcKilled = new Asset<SoundEffect>[SoundID.NPCDeathCount];` | `public Asset<SoundEffect>[] SoundNpcKilled = new Asset<SoundEffect>[SoundID.NPCDeathCount];` |
| 4129 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 84 | 2 | SoundDoorOpen | Asset<SoundEffect> | `public Asset<SoundEffect> SoundDoorOpen;` | `public Asset<SoundEffect> SoundDoorOpen;` |
| 4131 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 88 | 2 | SoundDoorClosed | Asset<SoundEffect> | `public Asset<SoundEffect> SoundDoorClosed;` | `public Asset<SoundEffect> SoundDoorClosed;` |
| 4139 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 104 | 2 | SoundShatter | Asset<SoundEffect> | `public Asset<SoundEffect> SoundShatter;` | `public Asset<SoundEffect> SoundShatter;` |
| 4143 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 112 | 2 | SoundZombie | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundZombie = new Asset<SoundEffect>[131];` | `public Asset<SoundEffect>[] SoundZombie = new Asset<SoundEffect>[131];` |
| 4145 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 116 | 2 | SoundRoar | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundRoar = new Asset<SoundEffect>[3];` | `public Asset<SoundEffect>[] SoundRoar = new Asset<SoundEffect>[3];` |
| 4147 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 120 | 2 | SoundSplash | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundSplash = new Asset<SoundEffect>[6];` | `public Asset<SoundEffect>[] SoundSplash = new Asset<SoundEffect>[6];` |
| 4149 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 124 | 2 | SoundDoubleJump | Asset<SoundEffect> | `public Asset<SoundEffect> SoundDoubleJump;` | `public Asset<SoundEffect> SoundDoubleJump;` |
| 4151 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 128 | 2 | SoundRun | Asset<SoundEffect> | `public Asset<SoundEffect> SoundRun;` | `public Asset<SoundEffect> SoundRun;` |
| 4153 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 132 | 2 | SoundCoins | Asset<SoundEffect> | `public Asset<SoundEffect> SoundCoins;` | `public Asset<SoundEffect> SoundCoins;` |
| 4155 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 136 | 2 | SoundUnlock | Asset<SoundEffect> | `public Asset<SoundEffect> SoundUnlock;` | `public Asset<SoundEffect> SoundUnlock;` |
| 4161 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 148 | 2 | SoundDrown | Asset<SoundEffect> | `public Asset<SoundEffect> SoundDrown;` | `public Asset<SoundEffect> SoundDrown;` |

#### 属性（0）

无该类型成员记录。


### 4.30 细分子系统：`SharedAudioLegacyInterfaceSoundDefinitionCatalogState`

- 原报告章节：`4.11.32`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`SharedAudioLegacySoundDefinitionCatalogState`
- 细分职责：旧音效玩家反馈、菜单、相机和界面声音定义目录。
- 边界角色：`definition/catalog`；最小 seam：legacy interface sound definition port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4110 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 46 | 2 | SoundPlayerHit | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundPlayerHit = new Asset<SoundEffect>[3];` | `public Asset<SoundEffect>[] SoundPlayerHit = new Asset<SoundEffect>[3];` |
| 4112 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 50 | 2 | SoundFemaleHit | Asset<SoundEffect>[] | `public Asset<SoundEffect>[] SoundFemaleHit = new Asset<SoundEffect>[3];` | `public Asset<SoundEffect>[] SoundFemaleHit = new Asset<SoundEffect>[3];` |
| 4114 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 54 | 2 | SoundPlayerKilled | Asset<SoundEffect> | `public Asset<SoundEffect> SoundPlayerKilled;` | `public Asset<SoundEffect> SoundPlayerKilled;` |
| 4133 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 92 | 2 | SoundMenuOpen | Asset<SoundEffect> | `public Asset<SoundEffect> SoundMenuOpen;` | `public Asset<SoundEffect> SoundMenuOpen;` |
| 4135 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 96 | 2 | SoundMenuClose | Asset<SoundEffect> | `public Asset<SoundEffect> SoundMenuClose;` | `public Asset<SoundEffect> SoundMenuClose;` |
| 4137 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 100 | 2 | SoundMenuTick | Asset<SoundEffect> | `public Asset<SoundEffect> SoundMenuTick;` | `public Asset<SoundEffect> SoundMenuTick;` |
| 4141 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 108 | 2 | SoundCamera | Asset<SoundEffect> | `public Asset<SoundEffect> SoundCamera;` | `public Asset<SoundEffect> SoundCamera;` |
| 4157 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 140 | 2 | SoundChat | Asset<SoundEffect> | `public Asset<SoundEffect> SoundChat;` | `public Asset<SoundEffect> SoundChat;` |
| 4159 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 144 | 2 | SoundMaxMana | Asset<SoundEffect> | `public Asset<SoundEffect> SoundMaxMana;` | `public Asset<SoundEffect> SoundMaxMana;` |

#### 属性（0）

无该类型成员记录。


### 4.31 细分子系统：`SharedAudioLegacyWorldEnvironmentInstanceState`

- 原报告章节：`4.11.39`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`SharedAudioLegacyEnvironmentalInstanceState`
- 细分职责：旧音效液体、机械、天气、地形和世界交互声音实例。
- 边界角色：`presentation/adapter`；最小 seam：legacy world environment instance port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4095 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 16 | 2 | SoundInstanceDrip | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceDrip = new SoundEffectInstance[3];` | `public SoundEffectInstance[] SoundInstanceDrip = new SoundEffectInstance[3];` |
| 4097 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 20 | 2 | SoundInstanceLiquid | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceLiquid = new SoundEffectInstance[2];` | `public SoundEffectInstance[] SoundInstanceLiquid = new SoundEffectInstance[2];` |
| 4099 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 24 | 2 | SoundInstanceMech | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceMech = new SoundEffectInstance[1];` | `public SoundEffectInstance[] SoundInstanceMech = new SoundEffectInstance[1];` |
| 4101 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 28 | 2 | SoundInstanceDig | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceDig = new SoundEffectInstance[3];` | `public SoundEffectInstance[] SoundInstanceDig = new SoundEffectInstance[3];` |
| 4103 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 32 | 2 | SoundInstanceThunder | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceThunder = new SoundEffectInstance[6];` | `public SoundEffectInstance[] SoundInstanceThunder = new SoundEffectInstance[6];` |
| 4105 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 36 | 2 | SoundInstanceResearch | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceResearch = new SoundEffectInstance[4];` | `public SoundEffectInstance[] SoundInstanceResearch = new SoundEffectInstance[4];` |
| 4107 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 40 | 2 | SoundInstanceTink | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceTink = new SoundEffectInstance[3];` | `public SoundEffectInstance[] SoundInstanceTink = new SoundEffectInstance[3];` |
| 4109 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 44 | 2 | SoundInstanceCoin | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceCoin = new SoundEffectInstance[5];` | `public SoundEffectInstance[] SoundInstanceCoin = new SoundEffectInstance[5];` |
| 4117 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 60 | 2 | SoundInstanceGrass | SoundEffectInstance | `public SoundEffectInstance SoundInstanceGrass;` | `public SoundEffectInstance SoundInstanceGrass;` |
| 4121 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 68 | 2 | SoundInstancePixie | SoundEffectInstance | `public SoundEffectInstance SoundInstancePixie;` | `public SoundEffectInstance SoundInstancePixie;` |
| 4128 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 82 | 2 | SoundInstanceMoonlordCry | SoundEffectInstance | `public SoundEffectInstance SoundInstanceMoonlordCry;` | `public SoundEffectInstance SoundInstanceMoonlordCry;` |
| 4130 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 86 | 2 | SoundInstanceDoorOpen | SoundEffectInstance | `public SoundEffectInstance SoundInstanceDoorOpen;` | `public SoundEffectInstance SoundInstanceDoorOpen;` |
| 4132 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 90 | 2 | SoundInstanceDoorClosed | SoundEffectInstance | `public SoundEffectInstance SoundInstanceDoorClosed;` | `public SoundEffectInstance SoundInstanceDoorClosed;` |
| 4140 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 106 | 2 | SoundInstanceShatter | SoundEffectInstance | `public SoundEffectInstance SoundInstanceShatter;` | `public SoundEffectInstance SoundInstanceShatter;` |
| 4148 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 122 | 2 | SoundInstanceSplash | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceSplash = new SoundEffectInstance[6];` | `public SoundEffectInstance[] SoundInstanceSplash = new SoundEffectInstance[6];` |
| 4162 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 150 | 2 | SoundInstanceDrown | SoundEffectInstance | `public SoundEffectInstance SoundInstanceDrown;` | `public SoundEffectInstance SoundInstanceDrown;` |

#### 属性（0）

无该类型成员记录。


### 4.32 细分子系统：`SharedAudioLegacyEntityFeedbackInstanceState`

- 原报告章节：`4.11.40`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`19` / `音频、粒子与演出`

- 上一级基线细分子系统：`AudioLegacySoundAdapterState`
- 上一级 peer 细分子系统：`SharedAudioLegacyEnvironmentalInstanceState`
- 细分职责：旧音效物品、NPC、移动、跳跃和交互反馈声音实例。
- 边界角色：`presentation/adapter`；最小 seam：legacy entity feedback instance port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4119 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 64 | 2 | SoundInstanceGrab | SoundEffectInstance | `public SoundEffectInstance SoundInstanceGrab;` | `public SoundEffectInstance SoundInstanceGrab;` |
| 4123 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 72 | 2 | SoundInstanceItem | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceItem = new SoundEffectInstance[SoundID.ItemSoundCount];` | `public SoundEffectInstance[] SoundInstanceItem = new SoundEffectInstance[SoundID.ItemSoundCount];` |
| 4125 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 76 | 2 | SoundInstanceNpcHit | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceNpcHit = new SoundEffectInstance[59];` | `public SoundEffectInstance[] SoundInstanceNpcHit = new SoundEffectInstance[59];` |
| 4127 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 80 | 2 | SoundInstanceNpcKilled | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceNpcKilled = new SoundEffectInstance[SoundID.NPCDeathCount];` | `public SoundEffectInstance[] SoundInstanceNpcKilled = new SoundEffectInstance[SoundID.NPCDeathCount];` |
| 4144 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 114 | 2 | SoundInstanceZombie | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceZombie = new SoundEffectInstance[131];` | `public SoundEffectInstance[] SoundInstanceZombie = new SoundEffectInstance[131];` |
| 4146 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 118 | 2 | SoundInstanceRoar | SoundEffectInstance[] | `public SoundEffectInstance[] SoundInstanceRoar = new SoundEffectInstance[3];` | `public SoundEffectInstance[] SoundInstanceRoar = new SoundEffectInstance[3];` |
| 4150 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 126 | 2 | SoundInstanceDoubleJump | SoundEffectInstance | `public SoundEffectInstance SoundInstanceDoubleJump;` | `public SoundEffectInstance SoundInstanceDoubleJump;` |
| 4152 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 130 | 2 | SoundInstanceRun | SoundEffectInstance | `public SoundEffectInstance SoundInstanceRun;` | `public SoundEffectInstance SoundInstanceRun;` |
| 4154 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 134 | 2 | SoundInstanceCoins | SoundEffectInstance | `public SoundEffectInstance SoundInstanceCoins;` | `public SoundEffectInstance SoundInstanceCoins;` |
| 4156 | field | Terraria.Audio.LegacySoundPlayer | Terraria.Audio/LegacySoundPlayer.cs | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 138 | 2 | SoundInstanceUnlock | SoundEffectInstance | `public SoundEffectInstance SoundInstanceUnlock;` | `public SoundEffectInstance SoundInstanceUnlock;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：32；成员数：405；字段：354；属性：51。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
