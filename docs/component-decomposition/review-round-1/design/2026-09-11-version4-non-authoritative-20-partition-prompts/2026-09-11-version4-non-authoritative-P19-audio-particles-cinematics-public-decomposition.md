# Version4 非权威组件拆分分区 P19：音频、粒子与演出 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P19），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P19-non-authoritative-public-decomposition-20260911
- partitionId: P19
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\19-audio-particles-cinematics.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: ClientPresentationAndTools, RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 32
- fieldCount: 354
- propertyCount: 51
- memberCount: 405
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 32 个叶子子系统和 405 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainNpcFrameState` | `RuntimeComposition` | `4.1.32` | `presentation state` | 2 | 0 | 2 | NPC 帧计数和动画分类。 |
| `MainParticlePools` | `RuntimeComposition` | `4.1.35` | `presentation state` | 2 | 0 | 2 | 世界粒子渲染器引用。 |
| `MainAmbientEffectsAndChatState` | `RuntimeComposition` | `4.1.38` | `presentation state` | 19 | 0 | 19 | 风雨音乐、环境瀑布、聊天监视器和循环效果。 |
| `MainCageAquaticAndAmphibianAnimationState` | `RuntimeComposition` | `4.1.42` | `presentation state` | 16 | 0 | 16 | 水生和两栖捕获物的笼具、鱼缸与罐体帧。 |
| `MainCageBirdAnimationState` | `RuntimeComposition` | `4.1.47` | `presentation state` | 20 | 0 | 20 | 鸟类捕获物的笼具帧、鱼缸帧和动画计数。 |
| `MainBackgroundLayerCatalogState` | `RuntimeComposition` | `4.1.50` | `presentation state` | 20 | 0 | 20 | Main 背景图层、场景集合和背景目录状态。 |
| `MainBackgroundParallaxAndStyleState` | `RuntimeComposition` | `4.1.51` | `presentation state` | 9 | 0 | 9 | Main 背景偏移、视差、树木和风格参数状态。 |
| `MainCageMammalAndReptileAnimationState` | `RuntimeComposition` | `4.1.52` | `presentation state` | 18 | 0 | 18 | Main 中兔、松鼠、蜗牛、鼠、龟和大鼠捕获物动画状态。 |
| `MainCageInsectAndSmallCritterAnimationState` | `RuntimeComposition` | `4.1.53` | `presentation state` | 19 | 0 | 19 | Main 中蝴蝶、蜻蜓、蝎、仙女、蠕虫和其他小动物动画状态。 |
| `SharedAudioAndSoundData` | `SharedRuntimeMechanisms` | `4.9.11` | `presentation` | 3 | 0 | 3 | 共享声音播放载荷和音效数据结构。 |
| `SharedEffectAndSkyPresentation` | `SharedRuntimeMechanisms` | `4.9.43` | `presentation` | 12 | 3 | 15 | Effect、覆盖层和天空表现。 |
| `SharedParticlePresentation` | `SharedRuntimeMechanisms` | `4.9.44` | `presentation` | 6 | 2 | 8 | 粒子接口、池化粒子和粒子渲染器。 |
| `SharedBackgroundPresentationDefinitions` | `SharedRuntimeMechanisms` | `4.9.59` | `definition/presentation` | 5 | 0 | 5 | 背景变体和背景集合定义。 |
| `SharedStarParticleState` | `SharedRuntimeMechanisms` | `4.9.138` | `presentation state` | 15 | 0 | 15 | 星体位置、坠落、闪烁和淡入动画状态。 |
| `SharedCloudAndRainParticleState` | `SharedRuntimeMechanisms` | `4.9.139` | `presentation state` | 21 | 0 | 21 | 云层和雨滴位置、速度、透明度及回收状态。 |
| `SharedDustParticleState` | `SharedRuntimeMechanisms` | `4.9.146` | `presentation state` | 18 | 0 | 18 | Dust 粒子运动、光照、shader 和帧状态。 |
| `SharedGoreEffectState` | `SharedRuntimeMechanisms` | `4.9.147` | `presentation state` | 14 | 0 | 14 | Gore 运动、寿命、贴图帧和回收状态。 |
| `SharedSceneDecorationAndAudioState` | `SharedRuntimeMechanisms` | `4.9.176` | `presentation/query` | 0 | 20 | 20 | 场景音乐、蜡烛、纪念碑、装饰物和音频标记。 |
| `SharedPopupTextContentAndContextState` | `SharedRuntimeMechanisms` | `4.9.180` | `presentation` | 14 | 0 | 14 | 弹出文本内容、金币、声纳和来源上下文。 |
| `SharedPopupTextRenderLifecycleState` | `SharedRuntimeMechanisms` | `4.9.181` | `presentation state` | 16 | 0 | 16 | 弹出文本位置、透明度、生命周期和渲染缓存。 |
| `AudioDefinitionAndTrackState` | `ClientPresentationAndTools` | `4.11.2` | `definition/presentation` | 10 | 11 | 21 | 声音样式、音频轨道和播放覆盖定义。 |
| `AudioActiveSoundState` | `ClientPresentationAndTools` | `4.11.3` | `presentation` | 6 | 3 | 9 | 活动声音实例和特定环境声音跟踪。 |
| `CinematicTimelineState` | `ClientPresentationAndTools` | `4.11.4` | `presentation` | 13 | 11 | 24 | 电影序列、帧事件和播放状态。 |
| `AudioPlaybackCoordinatorState` | `ClientPresentationAndTools` | `4.11.7` | `presentation/adapter` | 2 | 1 | 3 | 音频引擎和播放协调器状态。 |
| `AudioTrackedSoundState` | `ClientPresentationAndTools` | `4.11.8` | `presentation/state` | 1 | 0 | 1 | 被跟踪声音集合和声音播放器状态。 |
| `SharedAudioLegacyTrackedInstanceState` | `ClientPresentationAndTools` | `4.11.26` | `presentation/adapter` | 2 | 0 | 2 | 旧音效可跟踪实例集合、回收和活动引用状态。 |
| `SharedAudioLegacySoundServicesState` | `ClientPresentationAndTools` | `4.11.27` | `presentation/adapter` | 2 | 0 | 2 | 旧音效 TrackableSounds、服务集合和外部播放适配。 |
| `SharedAudioLegacyPlayerAndInterfaceInstanceState` | `ClientPresentationAndTools` | `4.11.30` | `presentation/adapter` | 9 | 0 | 9 | 旧音效玩家反馈、菜单、相机和界面声音实例。 |
| `SharedAudioLegacyGameplaySoundDefinitionCatalogState` | `ClientPresentationAndTools` | `4.11.31` | `definition/catalog` | 25 | 0 | 25 | 旧音效环境、世界交互和实体反馈声音定义目录。 |
| `SharedAudioLegacyInterfaceSoundDefinitionCatalogState` | `ClientPresentationAndTools` | `4.11.32` | `definition/catalog` | 9 | 0 | 9 | 旧音效玩家反馈、菜单、相机和界面声音定义目录。 |
| `SharedAudioLegacyWorldEnvironmentInstanceState` | `ClientPresentationAndTools` | `4.11.39` | `presentation/adapter` | 16 | 0 | 16 | 旧音效液体、机械、天气、地形和世界交互声音实例。 |
| `SharedAudioLegacyEntityFeedbackInstanceState` | `ClientPresentationAndTools` | `4.11.40` | `presentation/adapter` | 10 | 0 | 10 | 旧音效物品、NPC、移动、跳跃和交互反馈声音实例。 |

来源成员的分区内序号线索范围：80..4522；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“音频、粒子与演出”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 NPC/捕获物动画帧、粒子池、背景/环境效果、音频定义/轨道/活动声音/旧音频服务、弹出文本和 Cinematic timeline，核对纯表现状态、池化生命周期和外部播放适配。
- 回到 Main、Audio、Cinematics、Particle、Background、PopupText 相关源码及触发调用者，确认创建/回收、帧更新、播放、停止、资源加载和客户端生命周期。
- 分别评估 presentation snapshot、particle pooled instance、audio definition vs active instance、legacy sound Adapter、cinematic timeline、popup text Projection 和背景目录。
- 核对音频/粒子/演出由世界事件、实体、战斗、UI、聊天触发时的单向事件输入、重复播放、取消、资源失败和帧顺序；不得把表现事实当模拟权威。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.Audio/ActiveSound.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/IAudioTrack.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/LegacySoundPlayer.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/LegacySoundStyle.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/SoundEngine.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/SoundPlayer.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/SoundPlayOverrides.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/SoundStyle.cs`
  - `D:\TRbackup\Version4\Terraria.Audio/VampireSizzleTracker.cs`
  - `D:\TRbackup\Version4\Terraria.Cinematics/CinematicManager.cs`
  - `D:\TRbackup\Version4\Terraria.Cinematics/Film.cs`
  - `D:\TRbackup\Version4\Terraria.Cinematics/FrameEventData.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/BackgroundVariant.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/BackgroundVariantSet.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/SoundPlaySet.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Skies/CreditsRollSky.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Effects/EffectManager.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Effects/GameEffect.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Effects/Overlay.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Effects/OverlayManager.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Effects/SkyManager.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Renderers/IParticle.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Renderers/IPooledParticle.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Renderers/ParticleRenderer.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Renderers/ParticleRendererSettings.cs`
  - `D:\TRbackup\Version4\Terraria.Graphics.Renderers/ParticleRepelDetails.cs`
  - `D:\TRbackup\Version4\Terraria/Cloud.cs`
  - `D:\TRbackup\Version4\Terraria/Dust.cs`
  - `D:\TRbackup\Version4\Terraria/Gore.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/PopupText.cs`
  - `D:\TRbackup\Version4\Terraria/Rain.cs`
  - `D:\TRbackup\Version4\Terraria/SceneMetrics.cs`
  - `D:\TRbackup\Version4\Terraria/Star.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.Audio.ActiveSound`
  - `Terraria.Audio.IAudioTrack`
  - `Terraria.Audio.LegacySoundPlayer`
  - `Terraria.Audio.LegacySoundStyle`
  - `Terraria.Audio.SoundEngine`
  - `Terraria.Audio.SoundPlayer`
  - `Terraria.Audio.SoundPlayOverrides`
  - `Terraria.Audio.SoundStyle`
  - `Terraria.Audio.VampireSizzleTracker`
  - `Terraria.Cinematics.CinematicManager`
  - `Terraria.Cinematics.Film`
  - `Terraria.Cinematics.Film.Sequence`
  - `Terraria.Cinematics.FrameEventData`
  - `Terraria.Cloud`
  - `Terraria.DataStructures.BackgroundVariant`
  - `Terraria.DataStructures.BackgroundVariantSet`
  - `Terraria.DataStructures.SoundPlaySet`
  - `Terraria.Dust`
  - `Terraria.GameContent.Skies.CreditsRollSky`
  - `Terraria.Gore`
  - `Terraria.Graphics.Effects.EffectManager<T>`
  - `Terraria.Graphics.Effects.GameEffect`
  - `Terraria.Graphics.Effects.Overlay`
  - `Terraria.Graphics.Effects.OverlayManager`
  - `Terraria.Graphics.Effects.SkyManager`
  - `Terraria.Graphics.Renderers.IParticle`
  - `Terraria.Graphics.Renderers.IPooledParticle`
  - `Terraria.Graphics.Renderers.ParticleRenderer`
  - `Terraria.Graphics.Renderers.ParticleRendererSettings`
  - `Terraria.Graphics.Renderers.ParticleRepelDetails`
  - `Terraria.Main`
  - `Terraria.PopupText`
  - `Terraria.Rain`
  - `Terraria.SceneMetrics`
  - `Terraria.Star`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 声音定义、轨道、活动实例、播放器集合和旧音频服务是否有不同生命周期与外部句柄；谁负责停止、回收和异常降级？
- 粒子、星体、云雨、Dust、Gore、动画帧和背景状态哪些是池化表现快照，哪些是可重建的定义/目录？
- Cinematic timeline、FrameEvent、PopupText 和聊天/战斗提示如何从事件输入生成单向 Projection，如何处理取消、跳帧和重复触发？
- 音频/粒子/演出与相机、地图、事件、实体、UI 和网络的共享 payload/事件 owner 哪些需 integration-review？

## 专属不拆分边界

- 不要把音频、粒子、动画、背景、弹出文本和电影序列合并为一个 PresentationComponent。
- 不要把活动音频句柄、粒子池槽位、单个帧事件、PopupText 条目或渲染缓存当作持久权威状态。
- 不要从音频/视觉输出反推模拟事件已提交；表现层只能消费快照/事件，不能反向写核心状态。

专属跨域提醒：重点记录与世界事件、实体/战斗、相机/地图、UI、聊天网络和资源平台的 integration-risk；AudioHandle、ParticleReference、CinematicEvent、PresentationSnapshot owner 标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 405 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P19
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
