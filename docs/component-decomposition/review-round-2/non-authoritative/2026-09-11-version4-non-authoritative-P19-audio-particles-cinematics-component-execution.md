# P19 非权威组件第二轮实施计划：音频、粒子与演出

> 本文件记录 proposed execution plan 及实际 Component checkpoint。未完成或依赖非 Component 边界的目标仍是 deferred，不表示已完成行为等价、网络/持久化闭合或最终 owner 裁决。

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

## 1. 实施边界与目标布局

本会话只允许把 Component 源码写入 src2，并只更新当前 P19 的两份第二轮文档；生产 src、测试代码、Version4、输入报告、ledger 和 lock 均未修改。项目文件和验证程序属于既有工作区内容，本 checkpoint 未修改它们。

| status | proposed path | proposed role | proposed single writer |
|---|---|---|---|
| proposed | src2/ClientPresentation/Animation/ | NPC/cage Components, Query, System, Projection | proposed CageAnimationSystem |
| proposed | src2/ClientPresentation/Environment/ | background, ambient, chat Components/Adapters/Projections | proposed BackgroundSelectionSystem and AmbientAudioSystem |
| proposed | src2/ClientPresentation/Particles/ | particle Components, renderer/pool Adapters | proposed ParticleUpdateSystem |
| proposed | src2/ClientPresentation/Popups/ | SceneDecoration and PopupText Components/Commands/Projection | proposed PopupTextLifecycleSystem |
| proposed | src2/ClientPresentation/Audio/ | audio values, Components, Query, Command, Adapter | proposed AudioPlaybackCoordinatorAdapterSystem |
| proposed | src2/ClientPresentation/Audio/Legacy/ | legacy audio catalog/instance/service Adapters | proposed LegacyAudioResourceAdapter |
| proposed | src2/ClientPresentation/Effects/ | effect/overlay/sky Adapters and Projections | proposed EffectLifecycleSystem |
| proposed | src2/ClientPresentation/Cinematics/ | timeline Component/Query/Command/System/Projection | proposed CinematicTimelineSystem |
| proposed | src2/ClientPresentation/Integration/ | cross-partition ports and snapshots | crossSubsystemOwner: integration-review |
| proposed | Test/ClientPresentation/ | focused verifier and test fixtures | proposed verifier harness |

Exact directories, namespaces and one-core-public-type-per-file choices must be rechecked against ECS文件组织设计约束.md and 组件命名设计约束.md before implementation. The NPC/cage target files listed below are now actual src2 implementation files for the current checkpoint; remaining target files remain planned until written.

## 2. Proposed target files and mapping

proposed target files include NpcAnimationFrameVisualsComponent.cs, ClientPlayerPresentationAdapterState.cs, CageAnimationControlState.cs, CageBirdAnimationVisualsComponent.cs, CageAquaticAnimationVisualsComponent.cs, CageMammalAnimationVisualsComponent.cs, CageSmallCritterAnimationVisualsComponent.cs, CageAnimationQuery.cs, CageAnimationSystem.cs, CageAnimationProjection.cs; BackgroundLayerCatalogComponent.cs, BackgroundParallaxVisualsComponent.cs, BackgroundVariantValue.cs, BackgroundVariantCatalogValue.cs, BackgroundSelectionQuery.cs, BackgroundSelectionSystem.cs, AmbientAudioVisualsComponent.cs, AmbientAudioSystem.cs, ChatMonitorAdapterState.cs, ChatProjection.cs; ParticleLayerRegistryAdapterState.cs, ParticleRendererAdapterState.cs, ParticleLifecycleAdapterContract.cs, ParticleRepelValue.cs, StarVisualsComponent.cs, StarfieldRuntimeControlState.cs, CloudVisualsComponent.cs, RainVisualsComponent.cs, DustVisualsComponent.cs, GoreVisualsComponent.cs, ParticleUpdateSystem.cs, ParticleDrawProjection.cs; SceneDecorationProjection.cs, PopupTextRegistryAdapterState.cs, PopupTextContentComponent.cs, PopupTextVisualsComponent.cs, PopupTextSpawnCommand.cs, PopupTextClearCommand.cs, PopupTextLifecycleSystem.cs; SoundPlayRequestValue.cs, AudioOverrideValue.cs, AudioDefinitionCatalogComponent.cs, ActiveSoundVisualsComponent.cs, AudioTrackAdapterState.cs, AudioPlaybackCoordinatorAdapterState.cs, AudioTrackedSoundRegistryAdapterState.cs, AudioPlaybackQuery.cs, AudioPlayCommand.cs, AudioPlaybackCoordinatorAdapterSystem.cs; LegacyAudioServiceAdapterState.cs, LegacyTrackedInstanceAdapterState.cs, LegacyGameplaySoundCatalogAdapterState.cs, LegacyInterfaceSoundCatalogAdapterState.cs, LegacyWorldEnvironmentAudioAdapterState.cs, LegacyEntityFeedbackAudioAdapterState.cs, LegacyPlayerInterfaceAudioAdapterState.cs, LegacyAudioResourceAdapter.cs; EffectRegistryAdapterState.cs, OverlayPresentationAdapterState.cs, SkyPresentationAdapterState.cs, CreditsRollProjection.cs, EffectLifecycleSystem.cs, OverlayProjection.cs, SkyProjection.cs; CinematicTimelineComponent.cs, CinematicTimelineAdapterState.cs, CinematicFrameEventQuery.cs, CinematicFrameEventCommand.cs, CinematicTimelineSystem.cs, CinematicProjection.cs. All are proposed paths only.

### Proposed source-to-target rule

- MainNpcFrameState maps to proposed NpcAnimationFrameVisualsComponent and proposed ClientPlayerPresentationAdapterState.
- The four MainCage leaves map by animal family to four proposed visual Components; cageFrames/critterCage remain in proposed CageAnimationControlState.
- MainBackground leaves and SharedBackgroundPresentationDefinitions map to proposed background catalog, parallax and value boundaries.
- MainAmbientEffectsAndChatState maps to proposed AmbientAudioVisualsComponent plus proposed ChatMonitorAdapterState.
- MainParticlePools, SharedParticlePresentation and Star/Cloud/Rain/Dust/Gore leaves map to proposed layer/renderer/pool Adapters and transient visual Components.
- SharedSceneDecorationAndAudioState maps to proposed SceneDecorationProjection; the two PopupText leaves split content, visual lifecycle and fixed-slot Adapter.
- SharedAudioAndSoundData, AudioDefinitionAndTrackState, AudioActiveSoundState, AudioPlaybackCoordinatorState and AudioTrackedSoundState map to proposed audio values, definitions, active state, track/coordinator Adapters and tracked registry.
- Seven SharedAudioLegacy leaves remain separate proposed legacy catalog, service, tracked-instance and family Adapters.
- SharedEffectAndSkyPresentation maps to proposed effect, overlay, sky and credits boundaries.
- CinematicTimelineState maps to proposed timeline Component, manager Adapter, pure frame Query, event Command, System and Projection.

## 3. Full proposed member mapping

The following mapping contains all 405 input members and is identical in both second-round documents.

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

## 4. Proposed implementation order and checkpoint gates

NPC/cage and background/ambient/chat implementation is started but not verified. Remaining proposed order:

1. proposed evidence fixtures and contract ports; gate on exact inventory and index/lifecycle evidence.
2. proposed NPC/cage animation; gate on array index and Entity/Tile/Cage scope.
3. proposed background/ambient/chat; gate on SceneMetrics/camera owner and client boundary.
4. proposed particle pools/instances; gate on renderer pool, shader conversion and teardown order.
5. proposed SceneDecoration/PopupText; gate on fixed-slot scope and npcNetID mapping.
6. proposed audio definitions/active playback; gate on stable sound keys, SlotId, audio support and random port.
7. proposed legacy audio Adapters; gate on Version4 LoadAll/PlaySound callers, resource failure and cleanup evidence.
8. proposed effects/skies; gate on render-layer order and lifecycle idempotence.
9. proposed cinematic timeline; gate on active-film policy, callback containment, frame inclusivity and cancellation.
10. proposed integration handoff and rollback gate.

At every gate, a proposed Component has one writer; Query is pure; Command commits; Adapter owns external I/O; Projection is one-way. Dual-read/dual-write is allowed only in a bounded proposed compatibility window with counters and an integration-approved rollback threshold.

## 5. Network, persistence, compatibility and failure plan

- Default proposed policy is no network replication or persistence for transient particles, PopupText, active sounds, effects, skies and cinematic frames.
- Stable content/definition IDs may be snapshotted only after crossSubsystemOwner: integration-review approves schema and migration.
- npcNetID remains a Network ID and must pass through a proposed network feedback Adapter before any Entity reference mapping.
- External handles and platform types never enter proposed snapshots; dedicated-server builds must omit client audio/graphics Adapters, subject to evidence.
- Unsupported audio, missing asset, invalid SlotId, pool exhaustion, monitor disposal and callback failure are local proposed Adapter errors with bounded retry/skip behavior.
- Cleanup is idempotent and owner-local. Legacy bridge removal, deletion and cutover require separate authorized implementation work.

## 6. Proposed rollback and focused verifier plan

Rollback conditions: ambiguous state ownership, identity loss in ID translation, leaked external handles, duplicated/dropped intents beyond the approved threshold, or focused verifier divergence. Preserve old inventory and compatibility Adapter as read-only evidence; enable one capability at a time; retain a bounded bridge; remove it only after integration-review acceptance.

Proposed verifiers: inventory 32/405/354/51 check; exact source-sequence comparison between report and both documents; metadata/checkpoint synchronization; static no-external-type leakage and one-writer checks; pure frame/threshold/background/audio qualification tests; pool/PopupText/tracked-sound/effect-sky/cinematic lifecycle tests; Adapter tests for unsupported audio, missing assets, invalid external IDs and dedicated-server guards.

Future compile-capable commands must run only through Build/Tools/Invoke-SerialDotnet.ps1 from repository root with -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false and verifiers with --no-build --no-restore. This checkpoint ran the Component project build and the existing P19 verifier recorded above; it did not run git diff --check against these documents.

## 7. Evidence gaps, blocking decisions and non-execution statement

- evidence-gap: missing first-round report D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P19-audio-particles-cinematics-public-decomposition.md.
- evidence-gap: Version4 stubs/deletions leave load, play, clear, spawn and callback behavior partial.
- evidence-gap: array/entity/tile scope, external teardown, final NLTX project boundary and current client runtime are not closed.
- blocking-decision: crossSubsystemOwner: integration-review for all cross-partition owners, interfaces, IDs, snapshots, value objects and system order.
- blocking-decision: no implementation, compatibility cutover, deletion or behavior-equivalence claim may proceed from this plan alone.

This is an incremental implementation record. NPC/cage, background/ambient/chat, particle, PopupText content/visual, audio definition, active sound, and cinematic timeline Component source files have been written under src2 and the affected Component project compiled with exit code 0. The existing P19 verifier also passed with exit code 0; that verifier does not establish behavior equivalence or full runtime integration. PopupText registry/commands/lifecycle, SceneDecoration projection, audio/cinematic adapters/queries/commands/systems/projections, legacy audio, and effects/skies remain deferred; no production src file was modified.
