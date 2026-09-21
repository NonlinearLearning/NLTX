# P19 非权威组件第二轮设计：音频、粒子与演出

> 本文件是 proposed 架构设计和证据账本，不是已实现 ECS、迁移结果、行为等价证明或最终跨分区 owner 决议。

partitionId: P19
sessionId: 2819da679d824763a95ac6f5b225940b
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\19-audio-particles-cinematics.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-component-execution.md
designStatus: proposed
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: independently-verified
productionCodeModified: no
src2CodeModified: yes
evidenceStatus: existing-evidence-with-gaps
completedComponents: npc-cage-animation, background-ambient-chat, particle-pools-and-instances, popup-text-content-and-visuals, audio-definition-catalog, active-sound-visuals, cinematic-timeline
implementedComponents: npc-cage-animation, background-ambient-chat, particle-pools-and-instances, popup-text-content-and-visuals, audio-definition-catalog, active-sound-visuals, cinematic-timeline (src2 implementation saved; affected Component project serially compiled with exitCode 0; existing P19 verifier passed with exitCode 0)
currentComponent: none; all independently implementable P19 Components saved
pendingComponents: none
deferredComponents: scene-decoration-projection, popup-text-registry-adapter, popup-text-commands-and-lifecycle, legacy-audio-adapters, effects-and-skies, cinematic-adapters-queries-commands-systems-projections
lastCheckpointUtc: 2026-09-12T10:46:28.2769629Z
implementedSrc2Files: src2/ClientPresentation/AudioParticlesCinematics/Animation/CageAquaticAnimationVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Animation/CageBirdAnimationVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Animation/CageMammalAnimationVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Animation/CageSmallCritterAnimationVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Animation/NpcAnimationFrameVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Environment/AmbientAudioVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Environment/BackgroundLayerCatalogComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Environment/BackgroundParallaxVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Particles/CloudVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Particles/DustVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Particles/GoreVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Particles/RainVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Particles/StarVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Popups/PopupTextContentComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Popups/PopupTextVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Audio/AudioDefinitionCatalogComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Audio/ActiveSoundVisualsComponent.cs; src2/ClientPresentation/AudioParticlesCinematics/Cinematics/CinematicTimelineComponent.cs
verificationCommand: & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src2\ClientPresentation\AudioParticlesCinematics\Terraria.AudioParticlesCinematics.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
verificationProject: D:\TRbackup\NLTX\src2\ClientPresentation\AudioParticlesCinematics\Terraria.AudioParticlesCinematics.csproj
verificationExitCode: 0
verificationWarningCount: 0
verificationErrorCount: 0
verificationArtifact: D:\TRbackup\NLTX\Build\bin\Terraria.AudioParticlesCinematics\Debug\net10.0\Terraria.AudioParticlesCinematics.dll
focusedVerifierCommand: & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src2\ClientPresentation\AudioParticlesCinematicsVerification\Terraria.AudioParticlesCinematicsVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
focusedVerifierProject: D:\TRbackup\NLTX\src2\ClientPresentation\AudioParticlesCinematicsVerification\Terraria.AudioParticlesCinematicsVerification.csproj
focusedVerifierExitCode: 0
focusedVerifierWarningCount: 0
focusedVerifierErrorCount: 0
focusedVerifierOutput: P19 verifier passed.
evidence-gap: missing first-round P19 report; Version4 stubbed or deleted paths; current NLTX presentation runtime is partial; cage array index scope and final background ownership remain unresolved; particle renderer/pool teardown and shader ownership remain integration-review gaps; PopupTextContext and PopupEffectStyle enum semantics, fixed-slot ownership, global active-count ownership, packed Color mapping, and npcNetID-to-entity mapping remain unresolved; audio random source, SoundType enum ownership, SoundPlayOverrides ownership, IAudioTrack lifecycle, stable sound-key mapping, external SoundEffectInstance/SoundStyle handles, loop-condition semantics, tracked registry/SlotId ownership, cinematic FrameEvent callback semantics, and full runtime integration remain unresolved
blocking-decision: crossSubsystemOwner: integration-review for cross-partition owners, IDs, snapshots, value objects, interfaces and candidate ordering; do not serialize unresolved cage index scope; particle renderer handles, pool slots, shader/custom-data mapping and teardown order remain provisional; PopupText fixed-slot registry, context/effect enum ownership, packed color representation, and network feedback boundary remain deferred; audio randomization, external track handles, playback coordinator ownership, loop-condition evaluation, tracked registry ownership, and platform audio lifecycle remain deferred; cinematic callback sink, frame inclusivity/cancellation policy, active-film ownership and teardown order remain deferred

## 1. 范围、排除与证据状态

本分区包含 32 个 leaf subsystem、405 条成员记录（字段 354、属性 51），父级为 ClientPresentationAndTools、RuntimeComposition、SharedRuntimeMechanisms。领域覆盖音频、粒子、背景、NPC/笼舍动画、场景装饰、PopupText、Effect/Overlay/Sky 和 cinematic timeline。

排除：权威战斗、移动、世界生成、Tile 存储、库存经济、网络协议实现、持久化实现、客户端平台 SDK 和其他 P 分区。跨分区类型、owner、ID、快照、值对象、接口和顺序只提出候选，统一标记 crossSubsystemOwner: integration-review。

证据等级：P19 输入报告的逐成员库存已核对；Version4 直接源码提供生命周期线索；完整参考源码和本地 tModLoader v2026.07 文档只是 supplement/cross-check；当前 NLTX 仅有 partial 表现元数据。第一轮 P19 报告缺失，是 evidence-gap，不视为已完成分析。

## 2. Version4 事实与当前 NLTX 状态

直接 Version4 证据路径：

- D:\TRbackup\Version4\Terraria\Main.cs：背景、粒子层、风雨和环境音频、笼舍动画、聊天监视器及文本音频路径。
- D:\TRbackup\Version4\Terraria\Star.cs、Cloud.cs、Rain.cs、Dust.cs、Gore.cs：瞬态表现字段、生成、池化或清理线索。
- D:\TRbackup\Version4\Terraria\PopupText.cs、SceneMetrics.cs：PopupText 槽位、内容/渲染生命周期和场景扫描派生值。
- D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs、SoundPlayer.cs、SoundEngine.cs、LegacySoundPlayer.cs：活动音效、SlotVector 追踪、音频支持门、旧目录/实例、服务注入；Version4 部分 LoadAll/PlaySound 为 stub。
- D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs、LegacySoundStyle.cs、IAudioTrack.cs、SoundPlayOverrides.cs：音频定义、变体、随机 pitch、trackability、外部轨道生命周期。
- D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRenderer.cs、IParticle.cs、IPooledParticle.cs、ParticleRendererSettings.cs、ParticleRepelDetails.cs：粒子 add/update/remove/pool 边界。
- D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs、GameEffect.cs、Overlay.cs、OverlayManager.cs、SkyManager.cs、D:\TRbackup\Version4\Terraria.GameContent.Skies\CreditsRollSky.cs：效果、覆盖层和天空生命周期。
- D:\TRbackup\Version4\Terraria.Cinematics\Film.cs、CinematicManager.cs、FrameEventData.cs：帧窗口、序列、事件、begin/update/end 和完成移除线索。

补充证据 D:\TRbackup\无任何删减通过编译\ 仅用于对应 Audio/Cinematics 文件的完整参考，不替代 Version4；tModLoader 文档和 SS14 结构参考只用于语义/边界交叉检查。

当前 NLTX：src/Content/SoundReference.cs、ContentPresentationIndex.cs、ItemUseDefinition.cs、NpcPresentationDefinition.cs、ProjectilePresentationDefinition.cs 以及 WorldSession 天气文件为 partial 元数据；不存在已闭合的 P19 client presentation runtime。RuleOverride 文件属于 out-of-boundary。两份本输出文件在写入前不存在。

## 3. Proposed 边界、所有权和生命周期

| proposed boundary | responsibility and fields | writer/readers/lifecycle | persistence/network/UI/side-effect boundary |
|---|---|---|---|
| proposed NpcAnimationFrameVisualsComponent | npcFrameCount as content-keyed frame catalog | proposed catalog initializer writes; NPC presentation reads; reload/scene cleanup | no authority/persistence; crossSubsystemOwner: integration-review |
| proposed ClientPlayerPresentationAdapterState | clientPlayer external local-player seam | proposed client adapter owns reference and teardown | no shared gameplay state; crossSubsystemOwner: integration-review |
| proposed CageBirdAnimationVisualsComponent | bird/penguin/owl frames and counters | proposed CageAnimationSystem writes; draw projection reads; cull/teardown clears | transient, no network/save; crossSubsystemOwner: integration-review |
| proposed CageAquaticAnimationVisualsComponent | fish/frog/jellyfish/water-strider/seahorse/pufferfish arrays | proposed CageAnimationSystem writes; projection reads; container teardown clears | transient, no network/save; crossSubsystemOwner: integration-review |
| proposed CageMammalAnimationVisualsComponent | bunny/squirrel/snail/mouse/turtle/rat arrays | proposed CageAnimationSystem writes; projection reads; container teardown clears | transient, no network/save; crossSubsystemOwner: integration-review |
| proposed CageSmallCritterAnimationVisualsComponent | butterfly/dragonfly/scorpion/fairy/worm/maggot/ladybug/slug/grasshopper arrays | proposed CageAnimationSystem writes; projection reads; container teardown clears | transient, no network/save; crossSubsystemOwner: integration-review |
| proposed CageAnimationControlState | cageFrames and critterCage shared control inputs | proposed integration input writes; cage system reads; scope unresolved | no direct external side effect; crossSubsystemOwner: integration-review |
| proposed BackgroundLayerCatalogComponent | cloud alpha/active and background layer ID catalogs | proposed background init writes; selection/render reads; reload clears | definition/presentation only; crossSubsystemOwner: integration-review |
| proposed BackgroundParallaxVisualsComponent | tree/cave offsets and biome back styles | proposed selection system writes; background projection reads | client view only; crossSubsystemOwner: integration-review |
| proposed BackgroundVariantValue / proposed BackgroundVariantCatalogValue | Pure/Corrupt/Crimson/Hallow/desert definitions | proposed catalog adapter writes; background query reads | stable values only; crossSubsystemOwner: integration-review |
| proposed AmbientAudioVisualsComponent | wind/rain thresholds, waterfall/lava positions/strengths, counter and playing flags | proposed derivation system writes; audio intent query reads | derived client state; audio adapter is sole external side-effect owner; crossSubsystemOwner: integration-review |
| proposed ChatMonitorAdapterState | chatMonitor external UI handle | proposed UI adapter owns attach/detach; chat projection reads | UI side effect only; crossSubsystemOwner: integration-review |
| proposed ParticleLayerRegistryAdapterState | two Main ParticleRenderer layer references | proposed renderer registry owns init/teardown; particle systems request layers | external graphics state, not ECS authority; crossSubsystemOwner: integration-review |
| proposed ParticleRendererAdapterState / proposed ParticleRepelValue | Settings, Particles, anchor/source/radius/water inputs | renderer adapter maps values; particle system reads/writes transient state | external renderer/shader boundary; crossSubsystemOwner: integration-review |
| proposed StarVisualsComponent / proposed StarfieldRuntimeControlState | per-star transform/twinkle/falling/fade and static starfall controls | proposed particle system writes; draw projection reads; pool cleanup clears | transient client-only; crossSubsystemOwner: integration-review |
| proposed CloudVisualsComponent / proposed RainVisualsComponent | cloud/rain transform, velocity, alpha, type, active/kill state | proposed particle system writes; renderer reads; pool/scene cleanup clears | transient client-only; crossSubsystemOwner: integration-review |
| proposed DustVisualsComponent / proposed GoreVisualsComponent | transform, lifetime, light/frame/tile-layer and shader-independent fields | proposed visual systems write; draw projection reads | transient client-only; shader/customData behind adapter; crossSubsystemOwner: integration-review |
| proposed SceneDecorationProjection | SceneMetrics-derived music box, decoration, monolith, fountain and backwall view | SceneMetrics remains source reader; projection only emits view | no write-back, no persistence by default; crossSubsystemOwner: integration-review |
| proposed PopupTextRegistryAdapterState | fixed popup array, max count, reset/clear | popup adapter owns slots; commands and lifecycle system use it | UI/world presentation side effect; crossSubsystemOwner: integration-review |
| proposed PopupTextContentComponent / proposed PopupTextVisualsComponent | text/context/stack/coin/sonar/npcNetID and position/alpha/color/lifetime/effects | spawn command writes content; lifecycle system writes visuals; projections read | transient; npcNetID remains Network ID; crossSubsystemOwner: integration-review |
| proposed SoundPlayRequestValue / proposed AudioOverrideValue | cooldown/type/style and optional volume override | commands create immutable values; audio coordinator consumes | no external handle; crossSubsystemOwner: integration-review |
| proposed AudioDefinitionCatalogComponent | style/type/volume/pitch variance/variation/trackability/max instances | definition loader writes; playback query reads | stable content value; crossSubsystemOwner: integration-review |
| proposed ActiveSoundVisualsComponent | global flag, position, volume, pitch, condition and playing state | playback coordinator writes; projection/UI reads; stop cleanup clears | transient client state; external Sound behind adapter; crossSubsystemOwner: integration-review |
| proposed AudioTrackAdapterState / proposed AudioPlaybackCoordinatorAdapterState | IAudioTrack lifecycle and SoundEngine/players/audio support | coordinator is sole external audio writer; adapter init/stop/reuse owns handles | platform audio side effects only; crossSubsystemOwner: integration-review |
| proposed AudioTrackedSoundRegistryAdapterState | tracked ActiveSound SlotVector and SlotId boundary | coordinator owns register/update/remove; scene teardown drains | external registry, no persistence; crossSubsystemOwner: integration-review |
| proposed LegacyAudioServiceAdapterState / proposed LegacyTrackedInstanceAdapterState | IServiceProvider, assets, TrackableSoundInstances and tracked list | legacy resource adapter owns load/clear; coordinator routes play; teardown drains | external resource/instance lifecycle; crossSubsystemOwner: integration-review |
| proposed LegacyGameplaySoundCatalogAdapterState / proposed LegacyInterfaceSoundCatalogAdapterState | legacy Asset<SoundEffect> definition families | catalog adapters own load/reload; play adapters read | external assets never enter core; crossSubsystemOwner: integration-review |
| proposed LegacyWorldEnvironmentAudioAdapterState / proposed LegacyEntityFeedbackAudioAdapterState / proposed LegacyPlayerInterfaceAudioAdapterState | legacy SoundEffectInstance arrays for environment/entity/UI families | family adapters own reuse/stop/clear; coordinator routes intents | platform audio only; crossSubsystemOwner: integration-review |
| proposed EffectRegistryAdapterState / proposed OverlayPresentationAdapterState / proposed SkyPresentationAdapterState | loaded keyed effects, opacity/priority, overlay lists, active skies | lifecycle system coordinates; adapters own external lists; projections read | graphics lifecycle only; crossSubsystemOwner: integration-review |
| proposed CreditsRollProjection | end time and full-play duration | credits projection reads sky state; teardown clears view | client-only, no persistence; crossSubsystemOwner: integration-review |
| proposed CinematicTimelineComponent | film frame/count, append time, active flag, sequences and frame data | proposed CinematicTimelineSystem is sole writer; projection reads; completion removes | transient; callbacks behind command/sink; crossSubsystemOwner: integration-review |
| proposed CinematicTimelineAdapterState / proposed CinematicFrameEventQuery | manager/film collection boundary and pure frame window facts | adapter owns external collection; query has no writes | no external callback from query; crossSubsystemOwner: integration-review |
| proposed CinematicFrameEventCommand / proposed CinematicProjection | explicit event output and client camera/UI/effect view | timeline system emits command; integration sink/projection consumes | one-way output; crossSubsystemOwner: integration-review |

Invariant: proposed Components contain cohesive scalar/value state only. Asset, SoundEffectInstance, ParticleRenderer, IAudioTrack, CustomSky, IServiceProvider, SlotVector and similar lifecycle types stay behind proposed Adapters.

## 4. Proposed queries, commands, adapters, projections and ordering

| status | proposed object | reads/writes | explicit side effect |
|---|---|---|---|
| proposed | CageAnimationQuery | pure frame/index eligibility | none |
| proposed | CageAnimationSystem | writes frame/counter state | emits projection only |
| proposed | BackgroundSelectionQuery | pure biome/camera selection | none |
| proposed | AmbientAudioQuery | pure threshold/position qualification | none |
| proposed | AmbientAudioCommand | commits sound intent | proposed audio adapter only |
| proposed | ParticleUpdateSystem | writes transient particle/lifetime state | proposed renderer/pool adapter |
| proposed | ParticleDrawProjection | reads state | proposed graphics output only |
| proposed | SceneDecorationQuery | reads SceneMetrics-derived values | none |
| proposed | PopupTextSpawnCommand / PopupTextClearCommand | commit content/reset | popup registry adapter |
| proposed | PopupTextLifecycleSystem | writes motion/fade/active | no network/save |
| proposed | AudioPlaybackQuery | pure distance/trackability/max-instance check | none |
| proposed | AudioPlayCommand | commits play request | audio coordinator adapter |
| proposed | LegacyAudioResourceAdapter | load/reuse/clear resources | external asset lifecycle |
| proposed | EffectLifecycleSystem | activation/deactivation transition | effect/overlay/sky adapters |
| proposed | OverlayProjection / SkyProjection | read active registries | graphics output |
| proposed | CinematicFrameEventQuery | computes frame/first/last/remaining | none |
| proposed | CinematicTimelineSystem | writes frame/active and emits events | command sink only |

Candidate order, all provisional and crossSubsystemOwner: integration-review: (1) proposed SceneMetricsInputRefreshSystem; (2) proposed BackgroundSelectionSystem and CageAnimationSystem; (3) proposed ParticleUpdateSystem; (4) proposed PopupTextLifecycleSystem; (5) proposed CinematicTimelineSystem; (6) proposed AudioIntentSystem; (7) proposed AudioPlaybackCoordinatorAdapterSystem; (8) proposed EffectLifecycleSystem; (9) proposed OverlayProjection and SkyProjection; (10) UI, Camera, Network and WorldEvent projection consumers.

Query has no implicit writes. System writes are single-owner and explicit. Command is the commit boundary. Adapter owns I/O, clock/random/resource/audio/graphics lifecycle. Projection is one-way and cannot write Components.

## 5. ID、快照、网络、持久化和外部边界

- proposed Entity ID：一个瞬态视觉实例的 ECS 身份；不等于数组索引、Persistence ID、Network ID 或 external SlotId。
- proposed Persistence ID：内容/定义稳定键；瞬态星、云、雨、Dust、Gore、PopupText、活动音效、Effect、Sky 和 cinematic frame 默认不持久化。
- proposed Network ID：npcNetID 只能作为网络反馈标识；转为 ECS Entity reference 的映射由 crossSubsystemOwner: integration-review 决定。
- proposed External ID：音频资源键、SoundStyle/LegacySoundStyle 键、效果/覆盖层/天空键、SlotId 或平台句柄；不得进入权威 Component。
- proposed Snapshot 只携带稳定 ID 和标量/值对象，不携带 Asset、SoundEffectInstance、ParticleRenderer、IAudioTrack、IServiceProvider、SlotVector 或平台句柄。
- 网络消息、保存字段、快照版本、迁移策略和调度顺序都是 crossSubsystemOwner: integration-review 的 blocking-decision。

## 6. Complete 32-leaf / 405-member coverage ledger

下表逐条保留 P19 输入报告的 source sequence、kind、declaring type、Version4 路径、行列、member 和 declared type；最后一列是 proposed 归属，所有跨分区关系均标记 crossSubsystemOwner: integration-review。

### MainNpcFrameState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 445 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1084 | 2 | npcFrameCount | int[] | proposed NpcAnimationFrameVisualsComponent; crossSubsystemOwner: integration-review |
| 446 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1159 | 2 | clientPlayer | Terraria.Player | proposed ClientPlayerPresentationAdapterState; crossSubsystemOwner: integration-review |

### MainParticlePools

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 461 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1190 | 2 | ParticleSystem_World_OverPlayers | Terraria.Graphics.Renderers.ParticleRenderer | proposed ParticleLayerRegistryAdapterState; crossSubsystemOwner: integration-review |
| 462 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1192 | 2 | ParticleSystem_World_BehindPlayers | Terraria.Graphics.Renderers.ParticleRenderer | proposed ParticleLayerRegistryAdapterState; crossSubsystemOwner: integration-review |

### MainAmbientEffectsAndChatState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 474 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1218 | 2 | _shouldUseWindyDayMusic | bool | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 475 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1220 | 2 | _shouldUseStormMusic | bool | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 476 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1222 | 2 | _minWind | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 477 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1224 | 2 | _maxWind | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 478 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1226 | 2 | _minRain | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 479 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1228 | 2 | _maxRain | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 480 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1230 | 2 | ambientWaterfallX | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 481 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1232 | 2 | ambientWaterfallY | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 482 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1234 | 2 | ambientWaterfallStrength | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 483 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1236 | 2 | ambientLavafallX | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 484 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1238 | 2 | ambientLavafallY | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 485 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1240 | 2 | ambientLavafallStrength | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 486 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1242 | 2 | ambientLavaX | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 487 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1244 | 2 | ambientLavaY | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 488 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1246 | 2 | ambientLavaStrength | float | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 489 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1248 | 2 | ambientCounter | int | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 490 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1250 | 2 | _isWaterfallMusicPlaying | bool | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 491 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1252 | 2 | _isLavafallMusicPlaying | bool | proposed AmbientAudioVisualsComponent; crossSubsystemOwner: integration-review |
| 492 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 1254 | 2 | chatMonitor | Terraria.GameContent.UI.Chat.IChatMonitor | proposed ChatMonitorAdapterState; crossSubsystemOwner: integration-review |

### MainCageAquaticAndAmphibianAnimationState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 331 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 839 | 2 | fishBowlFrameMode | byte[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 332 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 841 | 2 | fishBowlFrame | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 333 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 843 | 2 | fishBowlFrameCounter | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 334 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 845 | 2 | lavaFishBowlFrame | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 335 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 847 | 2 | lavaFishBowlFrameCounter | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 336 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 849 | 2 | frogCageFrame | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 337 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 851 | 2 | frogCageFrameCounter | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 344 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 865 | 2 | jellyfishCageMode | byte[,] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 345 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 867 | 2 | jellyfishCageFrame | int[,] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 346 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 869 | 2 | jellyfishCageFrameCounter | int[,] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 357 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 891 | 2 | waterStriderCageFrame | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 358 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 893 | 2 | waterStriderCageFrameCounter | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 359 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 895 | 2 | seahorseCageFrame | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 360 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 897 | 2 | seahorseCageFrameCounter | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 367 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 911 | 2 | pufferfishCageFrame | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 368 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 913 | 2 | pufferfishCageFrameCounter | int[] | proposed CageAquaticAnimationVisualsComponent; crossSubsystemOwner: integration-review |

### MainCageBirdAnimationState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 304 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 785 | 2 | mallardCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 305 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 787 | 2 | mallardCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 306 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 789 | 2 | duckCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 307 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 791 | 2 | duckCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 308 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 793 | 2 | grebeCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 309 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 795 | 2 | grebeCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 310 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 797 | 2 | seagullCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 311 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 799 | 2 | seagullCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 312 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 801 | 2 | birdCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 313 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 803 | 2 | birdCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 314 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 805 | 2 | redBirdCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 315 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 807 | 2 | redBirdCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 316 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 809 | 2 | blueBirdCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 317 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 811 | 2 | blueBirdCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 318 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 813 | 2 | macawCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 319 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 815 | 2 | macawCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 355 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 887 | 2 | penguinCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 356 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 889 | 2 | penguinCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 363 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 903 | 2 | owlCageFrame | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 364 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 905 | 2 | owlCageFrameCounter | int[] | proposed CageBirdAnimationVisualsComponent; crossSubsystemOwner: integration-review |

### MainBackgroundLayerCatalogState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 82 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 324 | 2 | cloudBGAlpha | float | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 83 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 326 | 2 | cloudBGActive | float | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 84 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 328 | 2 | treeMntBGSet1 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 85 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 330 | 2 | treeMntBGSet2 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 86 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 332 | 2 | treeMntBGSet3 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 87 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 334 | 2 | treeMntBGSet4 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 88 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 336 | 2 | treeBGSet1 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 89 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 338 | 2 | treeBGSet2 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 90 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 340 | 2 | treeBGSet3 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 91 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 342 | 2 | treeBGSet4 | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 92 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 344 | 2 | corruptBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 93 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 346 | 2 | jungleBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 94 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 348 | 2 | snowMntBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 95 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 350 | 2 | snowBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 96 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 352 | 2 | hallowBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 97 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 354 | 2 | crimsonBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 98 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 356 | 2 | desertBackgroundSet | Terraria.DataStructures.BackgroundVariantSet | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 99 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 358 | 2 | mushroomBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 100 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 360 | 2 | oceanBG | int | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |
| 101 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 362 | 2 | underworldBG | int[] | proposed BackgroundLayerCatalogComponent; crossSubsystemOwner: integration-review |

### MainBackgroundParallaxAndStyleState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 80 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 320 | 2 | essScale | float | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 81 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 322 | 2 | essDir | int | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 102 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 364 | 2 | treeX | int[] | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 103 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 366 | 2 | treeStyle | int[] | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 104 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 368 | 2 | caveBackX | int[] | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 105 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 370 | 2 | caveBackStyle | int[] | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 106 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 372 | 2 | iceBackStyle | int | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 107 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 374 | 2 | hellBackStyle | int | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |
| 108 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 376 | 2 | jungleBackStyle | int | proposed BackgroundParallaxVisualsComponent; crossSubsystemOwner: integration-review |

### MainCageMammalAndReptileAnimationState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 296 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 769 | 2 | cageFrames | int | proposed CageAnimationControlState; crossSubsystemOwner: integration-review |
| 297 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 771 | 2 | critterCage | bool | proposed CageAnimationControlState; crossSubsystemOwner: integration-review |
| 298 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 773 | 2 | bunnyCageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 299 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 775 | 2 | bunnyCageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 300 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 777 | 2 | squirrelCageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 301 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 779 | 2 | squirrelCageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 302 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 781 | 2 | squirrelCageFrameOrange | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 303 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 783 | 2 | squirrelCageFrameCounterOrange | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 327 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 831 | 2 | snailCageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 328 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 833 | 2 | snailCageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 329 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 835 | 2 | snail2CageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 330 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 837 | 2 | snail2CageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 338 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 853 | 2 | mouseCageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 339 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 855 | 2 | mouseCageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 340 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 857 | 2 | turtleCageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 341 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 859 | 2 | turtleCageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 351 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 879 | 2 | ratCageFrame | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 352 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 881 | 2 | ratCageFrameCounter | int[] | proposed CageMammalAnimationVisualsComponent; crossSubsystemOwner: integration-review |

### MainCageInsectAndSmallCritterAnimationState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 320 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 817 | 2 | butterflyCageMode | byte[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 321 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 819 | 2 | butterflyCageFrame | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 322 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 821 | 2 | butterflyCageFrameCounter | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 323 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 823 | 2 | dragonflyJarFrameCounter | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 324 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 825 | 2 | dragonflyJarFrame | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 325 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 827 | 2 | scorpionCageFrame | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 326 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 829 | 2 | scorpionCageFrameCounter | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 342 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 861 | 2 | fairyJarFrame | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 343 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 863 | 2 | fairyJarFrameCounter | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 347 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 871 | 2 | wormCageFrame | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 348 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 873 | 2 | wormCageFrameCounter | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 349 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 875 | 2 | maggotCageFrame | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 350 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 877 | 2 | maggotCageFrameCounter | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 353 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 883 | 2 | ladybugCageFrame | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 354 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 885 | 2 | ladybugCageFrameCounter | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 361 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 899 | 2 | slugCageFrame | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 362 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 901 | 2 | slugCageFrameCounter | int[,] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 365 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 907 | 2 | grasshopperCageFrame | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |
| 366 | field | Terraria.Main | D:\TRbackup\Version4\Terraria\Main.cs | 909 | 2 | grasshopperCageFrameCounter | int[] | proposed CageSmallCritterAnimationVisualsComponent; crossSubsystemOwner: integration-review |

### SharedAudioAndSoundData

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 1283 | field | Terraria.DataStructures.SoundPlaySet | D:\TRbackup\Version4\Terraria.DataStructures\SoundPlaySet.cs | 5 | 2 | IntendedCooldown | int | proposed SoundPlayRequestValue; crossSubsystemOwner: integration-review |
| 1284 | field | Terraria.DataStructures.SoundPlaySet | D:\TRbackup\Version4\Terraria.DataStructures\SoundPlaySet.cs | 7 | 2 | SoundType | int | proposed SoundPlayRequestValue; crossSubsystemOwner: integration-review |
| 1285 | field | Terraria.DataStructures.SoundPlaySet | D:\TRbackup\Version4\Terraria.DataStructures\SoundPlaySet.cs | 9 | 2 | SoundStyle | int | proposed SoundPlayRequestValue; crossSubsystemOwner: integration-review |

### SharedEffectAndSkyPresentation

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 2188 | field | Terraria.GameContent.Skies.CreditsRollSky | D:\TRbackup\Version4\Terraria.GameContent.Skies\CreditsRollSky.cs | 12 | 2 | _endTime | int | proposed CreditsRollProjection; crossSubsystemOwner: integration-review |
| 2634 | field | Terraria.Graphics.Effects.EffectManager<T> | D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs | 8 | 2 | _isLoaded | bool | proposed EffectRegistryAdapterState; crossSubsystemOwner: integration-review |
| 2635 | field | Terraria.Graphics.Effects.EffectManager<T> | D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs | 10 | 2 | _effects | System.Collections.Generic.Dictionary<string, T> | proposed EffectRegistryAdapterState; crossSubsystemOwner: integration-review |
| 2636 | field | Terraria.Graphics.Effects.GameEffect | D:\TRbackup\Version4\Terraria.Graphics.Effects\GameEffect.cs | 7 | 2 | Opacity | float | proposed EffectRegistryAdapterState; crossSubsystemOwner: integration-review |
| 2637 | field | Terraria.Graphics.Effects.GameEffect | D:\TRbackup\Version4\Terraria.Graphics.Effects\GameEffect.cs | 9 | 2 | _isLoaded | bool | proposed EffectRegistryAdapterState; crossSubsystemOwner: integration-review |
| 2638 | field | Terraria.Graphics.Effects.GameEffect | D:\TRbackup\Version4\Terraria.Graphics.Effects\GameEffect.cs | 11 | 2 | _priority | Terraria.Graphics.Effects.EffectPriority | proposed EffectRegistryAdapterState; crossSubsystemOwner: integration-review |
| 2639 | field | Terraria.Graphics.Effects.Overlay | D:\TRbackup\Version4\Terraria.Graphics.Effects\Overlay.cs | 8 | 2 | Mode | Terraria.Graphics.Effects.OverlayMode | proposed OverlayPresentationAdapterState; crossSubsystemOwner: integration-review |
| 2640 | field | Terraria.Graphics.Effects.Overlay | D:\TRbackup\Version4\Terraria.Graphics.Effects\Overlay.cs | 10 | 2 | _layer | Terraria.Graphics.Effects.RenderLayers | proposed OverlayPresentationAdapterState; crossSubsystemOwner: integration-review |
| 2641 | field | Terraria.Graphics.Effects.OverlayManager | D:\TRbackup\Version4\Terraria.Graphics.Effects\OverlayManager.cs | 10 | 2 | OPACITY_RATE | float | proposed OverlayPresentationAdapterState; crossSubsystemOwner: integration-review |
| 2642 | field | Terraria.Graphics.Effects.OverlayManager | D:\TRbackup\Version4\Terraria.Graphics.Effects\OverlayManager.cs | 12 | 2 | _activeOverlays | System.Collections.Generic.LinkedList<Terraria.Graphics.Effects.Overlay>[] | proposed OverlayPresentationAdapterState; crossSubsystemOwner: integration-review |
| 2643 | field | Terraria.Graphics.Effects.SkyManager | D:\TRbackup\Version4\Terraria.Graphics.Effects\SkyManager.cs | 9 | 2 | Instance | Terraria.Graphics.Effects.SkyManager | proposed SkyPresentationAdapterState; crossSubsystemOwner: integration-review |
| 2644 | field | Terraria.Graphics.Effects.SkyManager | D:\TRbackup\Version4\Terraria.Graphics.Effects\SkyManager.cs | 11 | 2 | _activeSkies | System.Collections.Generic.LinkedList<Terraria.Graphics.Effects.CustomSky> | proposed SkyPresentationAdapterState; crossSubsystemOwner: integration-review |
| 3829 | property | Terraria.GameContent.Skies.CreditsRollSky | D:\TRbackup\Version4\Terraria.GameContent.Skies\CreditsRollSky.cs | 20 | 2 | AmountOfTimeNeededForFullPlay | int | proposed CreditsRollProjection; crossSubsystemOwner: integration-review |
| 3873 | property | Terraria.Graphics.Effects.EffectManager<T> | D:\TRbackup\Version4\Terraria.Graphics.Effects\EffectManager.cs | 12 | 2 | this[] | T | proposed EffectRegistryAdapterState; crossSubsystemOwner: integration-review |
| 3874 | property | Terraria.Graphics.Effects.Overlay | D:\TRbackup\Version4\Terraria.Graphics.Effects\Overlay.cs | 12 | 2 | Layer | Terraria.Graphics.Effects.RenderLayers | proposed OverlayPresentationAdapterState; crossSubsystemOwner: integration-review |

### SharedParticlePresentation

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 2679 | field | Terraria.Graphics.Renderers.ParticleRenderer | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRenderer.cs | 8 | 2 | Settings | Terraria.Graphics.Renderers.ParticleRendererSettings | proposed ParticleRendererAdapterState; crossSubsystemOwner: integration-review |
| 2680 | field | Terraria.Graphics.Renderers.ParticleRenderer | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRenderer.cs | 10 | 2 | Particles | System.Collections.Generic.List<Terraria.Graphics.Renderers.IParticle> | proposed ParticleRendererAdapterState; crossSubsystemOwner: integration-review |
| 2681 | field | Terraria.Graphics.Renderers.ParticleRendererSettings | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRendererSettings.cs | 7 | 2 | AnchorPosition | Vector2 | proposed ParticleRepelValue; crossSubsystemOwner: integration-review |
| 2682 | field | Terraria.Graphics.Renderers.ParticleRepelDetails | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRepelDetails.cs | 7 | 2 | SourcePosition | Vector2 | proposed ParticleRepelValue; crossSubsystemOwner: integration-review |
| 2683 | field | Terraria.Graphics.Renderers.ParticleRepelDetails | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRepelDetails.cs | 9 | 2 | Radius | float | proposed ParticleRepelValue; crossSubsystemOwner: integration-review |
| 2684 | field | Terraria.Graphics.Renderers.ParticleRepelDetails | D:\TRbackup\Version4\Terraria.Graphics.Renderers\ParticleRepelDetails.cs | 11 | 2 | IsInWater | bool | proposed ParticleRepelValue; crossSubsystemOwner: integration-review |
| 3884 | property | Terraria.Graphics.Renderers.IParticle | D:\TRbackup\Version4\Terraria.Graphics.Renderers\IParticle.cs | 7 | 2 | ShouldBeRemovedFromRenderer | bool | proposed ParticleLifecycleAdapterContract; crossSubsystemOwner: integration-review |
| 3885 | property | Terraria.Graphics.Renderers.IPooledParticle | D:\TRbackup\Version4\Terraria.Graphics.Renderers\IPooledParticle.cs | 5 | 2 | IsRestingInPool | bool | proposed ParticleLifecycleAdapterContract; crossSubsystemOwner: integration-review |

### SharedBackgroundPresentationDefinitions

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 1064 | field | Terraria.DataStructures.BackgroundVariant | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariant.cs | 5 | 2 | _backgrounds | int[] | proposed BackgroundVariantValue; crossSubsystemOwner: integration-review |
| 1065 | field | Terraria.DataStructures.BackgroundVariantSet | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 5 | 2 | Pure | Terraria.DataStructures.BackgroundVariant | proposed BackgroundVariantCatalogValue; crossSubsystemOwner: integration-review |
| 1066 | field | Terraria.DataStructures.BackgroundVariantSet | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 7 | 2 | Corrupt | Terraria.DataStructures.BackgroundVariant | proposed BackgroundVariantCatalogValue; crossSubsystemOwner: integration-review |
| 1067 | field | Terraria.DataStructures.BackgroundVariantSet | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 9 | 2 | Crimson | Terraria.DataStructures.BackgroundVariant | proposed BackgroundVariantCatalogValue; crossSubsystemOwner: integration-review |
| 1068 | field | Terraria.DataStructures.BackgroundVariantSet | D:\TRbackup\Version4\Terraria.DataStructures\BackgroundVariantSet.cs | 11 | 2 | Hallow | Terraria.DataStructures.BackgroundVariant | proposed BackgroundVariantCatalogValue; crossSubsystemOwner: integration-review |

### SharedStarParticleState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 3491 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 9 | 2 | position | Vector2 | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3492 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 11 | 2 | scale | float | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3493 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 13 | 2 | rotation | float | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3494 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 15 | 2 | type | int | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3495 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 17 | 2 | twinkle | float | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3496 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 19 | 2 | twinkleSpeed | float | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3497 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 21 | 2 | rotationSpeed | float | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3498 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 23 | 2 | falling | bool | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3499 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 25 | 2 | hidden | bool | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3500 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 27 | 2 | fallSpeed | Vector2 | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3501 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 29 | 2 | fallTime | int | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3502 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 31 | 2 | velocity | Vector2 | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |
| 3503 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 33 | 2 | starfallBoost | float | proposed StarfieldRuntimeControlState; crossSubsystemOwner: integration-review |
| 3504 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 35 | 2 | starFallCount | int | proposed StarfieldRuntimeControlState; crossSubsystemOwner: integration-review |
| 3505 | field | Terraria.Star | D:\TRbackup\Version4\Terraria\Star.cs | 37 | 2 | fadeIn | float | proposed StarVisualsComponent; crossSubsystemOwner: integration-review |

### SharedCloudAndRainParticleState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 2998 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 10 | 2 | position | Vector2 | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 2999 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 12 | 2 | scale | float | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3000 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 14 | 2 | rotation | float | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3001 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 16 | 2 | rSpeed | float | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3002 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 18 | 2 | sSpeed | float | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3003 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 20 | 2 | active | bool | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3004 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 22 | 2 | spriteDir | SpriteEffects | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3005 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 24 | 2 | type | int | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3006 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 26 | 2 | width | int | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3007 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 28 | 2 | height | int | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3008 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 30 | 2 | Alpha | float | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3009 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 32 | 2 | kill | bool | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3010 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 34 | 2 | rand | Terraria.Utilities.UnifiedRandom | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3011 | field | Terraria.Cloud | D:\TRbackup\Version4\Terraria\Cloud.cs | 36 | 2 | lastCameraCenter | Vector2? | proposed CloudVisualsComponent; crossSubsystemOwner: integration-review |
| 3391 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 8 | 2 | position | Vector2 | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |
| 3392 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 10 | 2 | velocity | Vector2 | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |
| 3393 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 12 | 2 | scale | float | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |
| 3394 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 14 | 2 | rotation | float | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |
| 3395 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 16 | 2 | alpha | int | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |
| 3396 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 18 | 2 | active | bool | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |
| 3397 | field | Terraria.Rain | D:\TRbackup\Version4\Terraria\Rain.cs | 20 | 2 | type | byte | proposed RainVisualsComponent; crossSubsystemOwner: integration-review |

### SharedDustParticleState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 3042 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 13 | 2 | dustIndex | int | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3043 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 15 | 2 | position | Vector2 | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3044 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 17 | 2 | velocity | Vector2 | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3045 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 19 | 2 | fadeIn | float | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3046 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 21 | 2 | noGravity | bool | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3047 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 23 | 2 | scale | float | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3048 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 25 | 2 | rotation | float | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3049 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 27 | 2 | noLight | bool | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3050 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 29 | 2 | noLightEmittance | bool | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3051 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 31 | 2 | fullBright | bool | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3052 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 33 | 2 | active | bool | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3053 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 35 | 2 | type | int | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3054 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 37 | 2 | color | Color | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3055 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 39 | 2 | alpha | int | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3056 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 41 | 2 | frame | Rectangle | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3057 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 43 | 2 | shader | Terraria.Graphics.Shaders.ArmorShaderData | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3058 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 45 | 2 | customData | object | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |
| 3059 | field | Terraria.Dust | D:\TRbackup\Version4\Terraria\Dust.cs | 47 | 2 | firstFrame | bool | proposed DustVisualsComponent; crossSubsystemOwner: integration-review |

### SharedGoreEffectState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 3106 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 16 | 2 | goreTime | int | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3107 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 18 | 2 | position | Vector2 | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3108 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 20 | 2 | velocity | Vector2 | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3109 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 22 | 2 | rotation | float | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3110 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 24 | 2 | scale | float | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3111 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 26 | 2 | alpha | int | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3112 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 28 | 2 | type | int | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3113 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 30 | 2 | light | float | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3114 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 32 | 2 | active | bool | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3115 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 34 | 2 | sticky | bool | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3116 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 36 | 2 | timeLeft | int | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3117 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 38 | 2 | behindTiles | bool | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3118 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 40 | 2 | frameCounter | byte | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |
| 3119 | field | Terraria.Gore | D:\TRbackup\Version4\Terraria\Gore.cs | 42 | 2 | Frame | Terraria.DataStructures.SpriteFrame | proposed GoreVisualsComponent; crossSubsystemOwner: integration-review |

### SharedSceneDecorationAndAudioState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 3987 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 120 | 2 | ActiveMusicBox | int | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 3988 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 122 | 2 | MusicBoxSilence | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4000 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 146 | 2 | HasSunflower | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4001 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 148 | 2 | HasGardenGnome | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4002 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 150 | 2 | HasClock | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4003 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 152 | 2 | HasCampfire | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4004 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 154 | 2 | HasStarInBottle | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4005 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 156 | 2 | HasHeartLantern | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4006 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 158 | 2 | ActiveFountainColor | int | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4007 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 160 | 2 | ActiveMonolithType | int | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4008 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 162 | 2 | BloodMoonMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4009 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 164 | 2 | MoonLordMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4010 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 166 | 2 | EchoMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4011 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 168 | 2 | ShimmerMonolithState | int | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4012 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 170 | 2 | CRTMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4013 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 172 | 2 | RetroMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4014 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 174 | 2 | NoirMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4015 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 176 | 2 | RadioThingMonolith | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4016 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 178 | 2 | HasCatBast | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |
| 4020 | property | Terraria.SceneMetrics | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 186 | 2 | BehindBackwall | bool | proposed SceneDecorationProjection; crossSubsystemOwner: integration-review |

### SharedPopupTextContentAndContextState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 3352 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 12 | 2 | maxItemText | int | proposed PopupTextRegistryAdapterState; crossSubsystemOwner: integration-review |
| 3353 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 14 | 2 | popupText | Terraria.PopupText[] | proposed PopupTextRegistryAdapterState; crossSubsystemOwner: integration-review |
| 3358 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 24 | 2 | name | string | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3359 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 26 | 2 | displayText | string | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3360 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 28 | 2 | stack | long | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3369 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 46 | 2 | coinText | bool | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3370 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 48 | 2 | coinValue | long | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3371 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 50 | 2 | sonarText | int | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3372 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 52 | 2 | expert | bool | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3373 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 54 | 2 | master | bool | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3374 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 56 | 2 | sonar | bool | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3375 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 58 | 2 | context | Terraria.PopupTextContext | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3376 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 60 | 2 | npcNetID | int | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |
| 3377 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 62 | 2 | freeAdvanced | bool | proposed PopupTextContentComponent; crossSubsystemOwner: integration-review |

### SharedPopupTextRenderLifecycleState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 3354 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 16 | 2 | position | Vector2 | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3355 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 18 | 2 | velocity | Vector2 | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3356 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 20 | 2 | alpha | float | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3357 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 22 | 2 | alphaDir | int | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3361 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 30 | 2 | scale | float | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3362 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 32 | 2 | rotation | float | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3363 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 34 | 2 | color | Color | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3364 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 36 | 2 | active | bool | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3365 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 38 | 2 | lifeTime | int | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3366 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 40 | 2 | framesSinceSpawn | int | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3367 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 42 | 2 | numActive | int | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3368 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 44 | 2 | NoStack | bool | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3378 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 64 | 2 | charOffsets | Vector2[] | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3379 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 66 | 2 | charColors | Color[] | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3380 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 68 | 2 | effectStyle | Terraria.GameContent.PopupEffectStyle | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |
| 3381 | field | Terraria.PopupText | D:\TRbackup\Version4\Terraria\PopupText.cs | 70 | 2 | effectIntensity | int | proposed PopupTextVisualsComponent; crossSubsystemOwner: integration-review |

### AudioDefinitionAndTrackState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4167 | field | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 8 | 2 | Random | Terraria.Utilities.UnifiedRandom | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4168 | field | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 10 | 2 | _style | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4169 | field | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 12 | 2 | Variations | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4170 | field | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 14 | 2 | SoundId | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4171 | field | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 16 | 2 | _maxTrackedInstances | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4175 | field | Terraria.Audio.SoundPlayOverrides | D:\TRbackup\Version4\Terraria.Audio\SoundPlayOverrides.cs | 5 | 2 | Volume | float? | proposed AudioOverrideValue; crossSubsystemOwner: integration-review |
| 4176 | field | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 8 | 2 | _random | Terraria.Utilities.UnifiedRandom | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4177 | field | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 10 | 2 | _volume | float | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4178 | field | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 12 | 2 | _pitchVariance | float | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4179 | field | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 14 | 2 | _type | Terraria.Audio.SoundType | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4500 | property | Terraria.Audio.IAudioTrack | D:\TRbackup\Version4\Terraria.Audio\IAudioTrack.cs | 8 | 2 | IsPlaying | bool | proposed AudioTrackAdapterState; crossSubsystemOwner: integration-review |
| 4501 | property | Terraria.Audio.IAudioTrack | D:\TRbackup\Version4\Terraria.Audio\IAudioTrack.cs | 10 | 2 | IsStopped | bool | proposed AudioTrackAdapterState; crossSubsystemOwner: integration-review |
| 4502 | property | Terraria.Audio.IAudioTrack | D:\TRbackup\Version4\Terraria.Audio\IAudioTrack.cs | 12 | 2 | IsPaused | bool | proposed AudioTrackAdapterState; crossSubsystemOwner: integration-review |
| 4503 | property | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 18 | 2 | Style | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4504 | property | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 30 | 2 | IsTrackable | bool | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4505 | property | Terraria.Audio.LegacySoundStyle | D:\TRbackup\Version4\Terraria.Audio\LegacySoundStyle.cs | 32 | 2 | MaxTrackedInstances | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4507 | property | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 16 | 2 | Volume | float | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4508 | property | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 18 | 2 | PitchVariance | float | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4509 | property | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 20 | 2 | Type | Terraria.Audio.SoundType | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4510 | property | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 22 | 2 | IsTrackable | bool | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |
| 4511 | property | Terraria.Audio.SoundStyle | D:\TRbackup\Version4\Terraria.Audio\SoundStyle.cs | 24 | 2 | MaxTrackedInstances | int | proposed AudioDefinitionCatalogComponent; crossSubsystemOwner: integration-review |

### AudioActiveSoundState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4089 | field | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 10 | 2 | IsGlobal | bool | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4090 | field | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 12 | 2 | Position | Vector2 | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4091 | field | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 14 | 2 | Volume | float | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4092 | field | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 16 | 2 | Pitch | float | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4093 | field | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 18 | 2 | Condition | Terraria.Audio.ActiveSound.LoopedPlayCondition | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4180 | field | Terraria.Audio.VampireSizzleTracker | D:\TRbackup\Version4\Terraria.Audio\VampireSizzleTracker.cs | 5 | 2 | _playerIndex | int | proposed ActiveSoundConditionAdapterState; crossSubsystemOwner: integration-review |
| 4497 | property | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 20 | 2 | Sound | SoundEffectInstance | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4498 | property | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 22 | 2 | Style | Terraria.Audio.SoundStyle | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |
| 4499 | property | Terraria.Audio.ActiveSound | D:\TRbackup\Version4\Terraria.Audio\ActiveSound.cs | 24 | 2 | IsPlaying | bool | proposed ActiveSoundVisualsComponent; crossSubsystemOwner: integration-review |

### CinematicTimelineState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4181 | field | Terraria.Cinematics.CinematicManager | D:\TRbackup\Version4\Terraria.Cinematics\CinematicManager.cs | 8 | 2 | Instance | Terraria.Cinematics.CinematicManager | proposed CinematicTimelineAdapterState; crossSubsystemOwner: integration-review |
| 4182 | field | Terraria.Cinematics.CinematicManager | D:\TRbackup\Version4\Terraria.Cinematics\CinematicManager.cs | 10 | 2 | _films | System.Collections.Generic.List<Terraria.Cinematics.Film> | proposed CinematicTimelineAdapterState; crossSubsystemOwner: integration-review |
| 4183 | field | Terraria.Cinematics.Film.Sequence | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 11 | 3 | _frameEvent | Terraria.Cinematics.FrameEvent | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4184 | field | Terraria.Cinematics.Film.Sequence | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 13 | 3 | _duration | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4185 | field | Terraria.Cinematics.Film.Sequence | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 15 | 3 | _start | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4186 | field | Terraria.Cinematics.Film | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 31 | 2 | _frame | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4187 | field | Terraria.Cinematics.Film | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 33 | 2 | _frameCount | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4188 | field | Terraria.Cinematics.Film | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 35 | 2 | _nextSequenceAppendTime | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4189 | field | Terraria.Cinematics.Film | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 37 | 2 | _isActive | bool | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4190 | field | Terraria.Cinematics.Film | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 39 | 2 | _sequences | System.Collections.Generic.List<Terraria.Cinematics.Film.Sequence> | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4191 | field | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 5 | 2 | _absoluteFrame | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4192 | field | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 7 | 2 | _start | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4193 | field | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 9 | 2 | _duration | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4512 | property | Terraria.Cinematics.Film.Sequence | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 17 | 3 | Event | Terraria.Cinematics.FrameEvent | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4513 | property | Terraria.Cinematics.Film.Sequence | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 19 | 3 | Duration | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4514 | property | Terraria.Cinematics.Film.Sequence | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 21 | 3 | Start | int | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4515 | property | Terraria.Cinematics.Film | D:\TRbackup\Version4\Terraria.Cinematics\Film.cs | 41 | 2 | IsActive | bool | proposed CinematicTimelineComponent; crossSubsystemOwner: integration-review |
| 4516 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 11 | 2 | AbsoluteFrame | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4517 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 13 | 2 | Start | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4518 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 15 | 2 | Duration | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4519 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 17 | 2 | Frame | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4520 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 19 | 2 | IsFirstFrame | bool | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4521 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 21 | 2 | IsLastFrame | bool | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |
| 4522 | property | Terraria.Cinematics.FrameEventData | D:\TRbackup\Version4\Terraria.Cinematics\FrameEventData.cs | 23 | 2 | Remaining | int | proposed CinematicFrameEventQuery; crossSubsystemOwner: integration-review |

### AudioPlaybackCoordinatorState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4172 | field | Terraria.Audio.SoundEngine | D:\TRbackup\Version4\Terraria.Audio\SoundEngine.cs | 11 | 2 | LegacySoundPlayer | Terraria.Audio.LegacySoundPlayer | proposed AudioPlaybackCoordinatorAdapterState; crossSubsystemOwner: integration-review |
| 4173 | field | Terraria.Audio.SoundEngine | D:\TRbackup\Version4\Terraria.Audio\SoundEngine.cs | 13 | 2 | SoundPlayer | Terraria.Audio.SoundPlayer | proposed AudioPlaybackCoordinatorAdapterState; crossSubsystemOwner: integration-review |
| 4506 | property | Terraria.Audio.SoundEngine | D:\TRbackup\Version4\Terraria.Audio\SoundEngine.cs | 15 | 2 | IsAudioSupported | bool | proposed AudioPlaybackCoordinatorAdapterState; crossSubsystemOwner: integration-review |

### AudioTrackedSoundState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4174 | field | Terraria.Audio.SoundPlayer | D:\TRbackup\Version4\Terraria.Audio\SoundPlayer.cs | 9 | 2 | _trackedSounds | SlotVector<Terraria.Audio.ActiveSound> | proposed AudioTrackedSoundRegistryAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacyTrackedInstanceState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4164 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 154 | 2 | TrackableSoundInstances | SoundEffectInstance[] | proposed LegacyTrackedInstanceAdapterState; crossSubsystemOwner: integration-review |
| 4166 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 158 | 2 | _trackedInstances | System.Collections.Generic.List<SoundEffectInstance> | proposed LegacyTrackedInstanceAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacySoundServicesState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4163 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 152 | 2 | TrackableSounds | Asset<SoundEffect>[] | proposed LegacyAudioServiceAdapterState; crossSubsystemOwner: integration-review |
| 4165 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 156 | 2 | _services | System.IServiceProvider | proposed LegacyAudioServiceAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacyPlayerAndInterfaceInstanceState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4111 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 48 | 2 | SoundInstancePlayerHit | SoundEffectInstance[] | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4113 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 52 | 2 | SoundInstanceFemaleHit | SoundEffectInstance[] | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4115 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 56 | 2 | SoundInstancePlayerKilled | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4134 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 94 | 2 | SoundInstanceMenuOpen | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4136 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 98 | 2 | SoundInstanceMenuClose | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4138 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 102 | 2 | SoundInstanceMenuTick | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4142 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 110 | 2 | SoundInstanceCamera | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4158 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 142 | 2 | SoundInstanceChat | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |
| 4160 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 146 | 2 | SoundInstanceMaxMana | SoundEffectInstance | proposed LegacyPlayerInterfaceAudioAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacyGameplaySoundDefinitionCatalogState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4094 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 14 | 2 | SoundDrip | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4096 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 18 | 2 | SoundLiquid | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4098 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 22 | 2 | SoundMech | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4100 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 26 | 2 | SoundDig | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4102 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 30 | 2 | SoundThunder | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4104 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 34 | 2 | SoundResearch | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4106 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 38 | 2 | SoundTink | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4108 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 42 | 2 | SoundCoin | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4116 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 58 | 2 | SoundGrass | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4118 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 62 | 2 | SoundGrab | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4120 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 66 | 2 | SoundPixie | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4122 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 70 | 2 | SoundItem | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4124 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 74 | 2 | SoundNpcHit | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4126 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 78 | 2 | SoundNpcKilled | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4129 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 84 | 2 | SoundDoorOpen | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4131 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 88 | 2 | SoundDoorClosed | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4139 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 104 | 2 | SoundShatter | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4143 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 112 | 2 | SoundZombie | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4145 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 116 | 2 | SoundRoar | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4147 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 120 | 2 | SoundSplash | Asset<SoundEffect>[] | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4149 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 124 | 2 | SoundDoubleJump | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4151 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 128 | 2 | SoundRun | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4153 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 132 | 2 | SoundCoins | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4155 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 136 | 2 | SoundUnlock | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4161 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 148 | 2 | SoundDrown | Asset<SoundEffect> | proposed LegacyGameplaySoundCatalogAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacyInterfaceSoundDefinitionCatalogState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4110 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 46 | 2 | SoundPlayerHit | Asset<SoundEffect>[] | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4112 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 50 | 2 | SoundFemaleHit | Asset<SoundEffect>[] | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4114 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 54 | 2 | SoundPlayerKilled | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4133 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 92 | 2 | SoundMenuOpen | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4135 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 96 | 2 | SoundMenuClose | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4137 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 100 | 2 | SoundMenuTick | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4141 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 108 | 2 | SoundCamera | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4157 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 140 | 2 | SoundChat | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |
| 4159 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 144 | 2 | SoundMaxMana | Asset<SoundEffect> | proposed LegacyInterfaceSoundCatalogAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacyWorldEnvironmentInstanceState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4095 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 16 | 2 | SoundInstanceDrip | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4097 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 20 | 2 | SoundInstanceLiquid | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4099 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 24 | 2 | SoundInstanceMech | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4101 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 28 | 2 | SoundInstanceDig | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4103 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 32 | 2 | SoundInstanceThunder | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4105 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 36 | 2 | SoundInstanceResearch | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4107 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 40 | 2 | SoundInstanceTink | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4109 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 44 | 2 | SoundInstanceCoin | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4117 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 60 | 2 | SoundInstanceGrass | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4121 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 68 | 2 | SoundInstancePixie | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4128 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 82 | 2 | SoundInstanceMoonlordCry | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4130 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 86 | 2 | SoundInstanceDoorOpen | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4132 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 90 | 2 | SoundInstanceDoorClosed | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4140 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 106 | 2 | SoundInstanceShatter | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4148 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 122 | 2 | SoundInstanceSplash | SoundEffectInstance[] | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |
| 4162 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 150 | 2 | SoundInstanceDrown | SoundEffectInstance | proposed LegacyWorldEnvironmentAudioAdapterState; crossSubsystemOwner: integration-review |

### SharedAudioLegacyEntityFeedbackInstanceState

| source sequence | kind | declaring type | Version4 source path | line | column | member | declared type | proposed target role |
|---:|---|---|---|---:|---:|---|---|---|
| 4119 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 64 | 2 | SoundInstanceGrab | SoundEffectInstance | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4123 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 72 | 2 | SoundInstanceItem | SoundEffectInstance[] | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4125 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 76 | 2 | SoundInstanceNpcHit | SoundEffectInstance[] | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4127 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 80 | 2 | SoundInstanceNpcKilled | SoundEffectInstance[] | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4144 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 114 | 2 | SoundInstanceZombie | SoundEffectInstance[] | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4146 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 118 | 2 | SoundInstanceRoar | SoundEffectInstance[] | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4150 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 126 | 2 | SoundInstanceDoubleJump | SoundEffectInstance | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4152 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 130 | 2 | SoundInstanceRun | SoundEffectInstance | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4154 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 134 | 2 | SoundInstanceCoins | SoundEffectInstance | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |
| 4156 | field | Terraria.Audio.LegacySoundPlayer | D:\TRbackup\Version4\Terraria.Audio\LegacySoundPlayer.cs | 138 | 2 | SoundInstanceUnlock | SoundEffectInstance | proposed LegacyEntityFeedbackAudioAdapterState; crossSubsystemOwner: integration-review |

## 7. Evidence gaps、blocking decisions 和 focused verifier plan

- evidence-gap: 缺失第一轮报告 D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-public-decomposition.md；不视为第一轮分析完成。
- evidence-gap: Version4 的 LoadAll、PlaySound、Clear、NewGore、部分 PopupText/callback 路径为 stub、删除或注释，不能补猜行为。
- evidence-gap: 笼舍数组、npcFrameCount、PopupText 固定槽位和 Main static 字段的 entity/tile/section/world scope 未闭合。
- evidence-gap: 当前 NLTX 的声音键、NPC frame count、weather/rain 等是 partial metadata，不是 P19 runtime 实现。
- evidence-gap: 外部资源卸载、音频设备失败、渲染池耗尽、UI monitor 释放和场景 teardown 行为缺少实现证据。
- blocking-decision: 所有跨分区 Component、System、Query、Command、Adapter、Projection、接口、ID、snapshot、value object 和顺序由 crossSubsystemOwner: integration-review 裁决。
- blocking-decision: 未确认 index/entity/tile owner 前不得实现或序列化动画和 PopupText 状态。
- blocking-decision: 未解决 Version4 stub 边界前不得宣称 behavior equivalence。
- focused verifier plan: 读取 P19 报告并断言 32/405/354/51；比较两份文档的同步元数据和完整 source sequence；静态检查 Query 无写入、Component 不泄露外部生命周期类型、每个外部类型只有一个 proposed Adapter；设计后再补 frame wrap、pool idempotence、PopupText reset、音频 unsupported/dedicated-server、tracked cleanup、effect/sky teardown、cinematic first/last/cancel 测试。
- focused verifier plan: 后续受影响项目才可通过 Build/Tools/Invoke-SerialDotnet.ps1 串行 restore/build/test，并使用 -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false；本 checkpoint 已完成受影响 Component 项目的串行 build，并以 --no-build --no-restore 运行现有 P19 verifier 且通过；未对这两份文档执行 git diff --check。

## 8. Implementation Evidence Checkpoint: particle-pools-and-instances

The `particle-pools-and-instances` component was implemented under `src2` and independently
verified. The implementation keeps renderer/layer handles behind adapters, stores particle
state as scalar/value records, lets `ParticleUpdateSystem` own lifecycle transitions, and lets
`ParticleDrawProjection` emit read-only snapshots. It does not claim renderer parity, pool
capacity parity, shader parity, network closure, persistence closure, or final ownership.

Focused verifier evidence:

- Build command: `pwsh -NoProfile -File .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src2\\ClientPresentation\\AudioParticlesCinematicsVerification\\Terraria.AudioParticlesCinematicsVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`.
- Build result: exit code `0`; `0` warnings; `0` errors.
- Verifier command: the same wrapper with `run --project .\\src2\\ClientPresentation\\AudioParticlesCinematicsVerification\\Terraria.AudioParticlesCinematicsVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verifier result: exit code `0`; output `P19 verifier passed.`
- Build outputs: `Build/bin/Terraria.AudioParticlesCinematics/Debug/net10.0/Terraria.AudioParticlesCinematics.dll` and `Build/bin/Terraria.AudioParticlesCinematicsVerification/Debug/net10.0/Terraria.AudioParticlesCinematicsVerification.dll`.
- Added `src2` surface: `Particles/ParticleRepelValue.cs`, `ParticleLayerRegistryAdapterState.cs`, `ParticleRendererAdapterState.cs`, `ParticleLifecycleState.cs`, `StarVisualState.cs`, `StarVisualsComponent.cs`, `StarfieldRuntimeControlState.cs`, `CloudVisualState.cs`, `CloudVisualsComponent.cs`, `RainVisualState.cs`, `RainVisualsComponent.cs`, `DustVisualState.cs`, `DustVisualsComponent.cs`, `GoreVisualState.cs`, `GoreVisualsComponent.cs`, `ParticleUpdateSystem.cs`, `ParticleDrawProjection.cs`, plus focused verifier assertions in `AudioParticlesCinematicsVerification/Program.cs`.

## 9. Integration Handoff

本文件现在同时记录实现进度：P19 所有当前证据允许独立实现的 Component 均已写入 src2，包括 PopupText 内容/视觉、音频定义、活动音效和 cinematic timeline 状态；受影响 Component 项目已完成串行编译并生成 Build/bin 产物，现有 P19 verifier 也已通过。本 checkpoint 的 verifier 只覆盖现有断言，不构成行为等价、网络/持久化闭合或运行时平台集成证明；PopupText registry/commands/lifecycle、SceneDecoration projection、音频与 cinematic adapters/queries/commands/systems/projections、legacy audio、effects/skies 均按任务边界 deferred。integration-review 仍需决定最终 owner、实体范围、稳定 ID、快照/网络/持久化策略、系统顺序、失败/重试策略、平台适配器、场景 teardown 以及 P19 是否落在 client presentation、shared content 或 world/session integration。
