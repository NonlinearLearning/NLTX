# P15 外部平台与协议边界：proposed 后续实施计划

partitionId: P15
sessionId: 24f54074853c473dacbcc4fa1438e4dd
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\15-external-platform-boundaries.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-component-execution.md
sessionRebind: explicit user-requested takeover from document session 149b070c9cf340c4b08c69797c32ce80; prior runner session 36c49a78c06d42b4aa2ccd18414094b0 was failed; current runner session 24f54074853c473dacbcc4fa1438e4dd
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: passed (focused src2 verifier re-run under current session)
completedComponents:
- reverified carried-forward MainPlatformExecutionBoundaryAdapter (source leaf: MainPlatformExecutionAdapter; focused verifier passed)
- reverified carried-forward SocialProviderRegistryBoundary (source leaf: SocialApiRegistry; focused verifier passed)
- reverified carried-forward WeGameIpcTransportBoundary (source leaf: SocialTransportIpc; focused verifier passed)
- reverified carried-forward WorkshopJoinBoundary (source leaf: WorkshopAndJoinBoundaryData; focused verifier passed)
- reverified carried-forward ResourcePackBoundaryAdapter (source leaf: SharedResourcePackAdapters; focused verifier passed)
- reverified carried-forward CryptoBoundaryAdapter (source leaf: CryptographicDependency; focused verifier passed through injected primitive)
- reverified carried-forward NatPortMappingBoundaryAdapter (source leaf: NatPortMappingInterop; focused verifier passed)
currentComponent: completed; evidence and integration review remain
pendingComponents: none (focused source verifier passed; broader host/provider/compatibility evidence remains open)
lastCheckpointUtc: 2026-09-12T07:29:40.951Z
evidence-gap: All proposed P15 boundary source was already present under src2 before this session and the focused build/verifier was re-run successfully. No P15 Component type is currently specified or present; the proposed Component candidates remain integration-review deferred. Native Windows return semantics, real provider registration, host integration, asynchronous pipe behavior, Workshop response ownership, resource-pack disposal under real asset services, NAT COM cleanup, and BCrypt compatibility vectors remain unproven. Version4 and complete-reference bodies are partial/version-drifted evidence and do not establish behavior equivalence.
blocking-decision: crossSubsystemOwner: integration-review is required for runtime/session entity, provider registry lifetime, JoinRequest acceptance owner, Workshop/resource-pack persistence and UI contracts, IPC scheduling, NAT mapping ownership and cleanup, and the exact secret-derivation compatibility policy.

> 本文件仍保留 proposed 设计与执行边界。对应的 `src2` C# 和 verifier 文件已经保存，串行编译与 focused verifier 已通过；真实宿主接入和行为等价仍待验证，不把这些源文件声明为当前 `src` 生产实现。

## 1. 实施边界和目标文件（全部 proposed）

目标目录按能力组织，避免 `Shared/Components/` 等收容目录：

| proposed target path | proposed role | source boundary |
|---|---|---|
| `src2/RuntimeComposition/Platform/MainPlatformExecutionBoundaryAdapter.cs` | proposed Adapter | `MainPlatformExecutionAdapter` |
| `src2/RuntimeComposition/Platform/IPlatformExecutionStatePort.cs` | proposed Port | `MainPlatformExecutionAdapter` |
| `src2/RuntimeComposition/Platform/PlatformExecutionLifecycleSystem.cs` | proposed System | host lifecycle |

上述路径均未创建。平台常量不会单独成为 Component 文件；一个 proposed public type 对应一个同名 PascalCase 文件。跨项目引用、项目归属和最终命名须在实现前由整合会话确认。

## 2. 第一组源成员映射和实施顺序

| source member | proposed target role/file | write owner | migration note |
|---|---|---|---|
| `Terraria.Main.NativeMethods.ES_CONTINUOUS` (`Main.cs:108`) | proposed constant inside `MainPlatformExecutionBoundaryAdapter.cs` | proposed Adapter only | retain numeric value behind port; no ECS registration key |
| `Terraria.Main.NativeMethods.ES_SYSTEM_REQUIRED` (`Main.cs:110`) | proposed constant inside `MainPlatformExecutionBoundaryAdapter.cs` | proposed Adapter only | retain numeric value behind port; no persistence/network field |

实施顺序：先添加 focused verifier seam（仍为后续实现动作），再实现 port contract，再实现 Windows adapter，再把 `NeverSleep`/`YouCanSleepNow` 的宿主调用改为 command flow，最后接入错误/退出路径。该计划在本会话未执行。

## 3. 平台边界实施规则

- `proposed IPlatformExecutionStatePort` 是唯一允许访问 OS API 的入口；System 不引用 `kernel32.dll`、P/Invoke 或平台第三方类型。
- previous execution state 归 Adapter 所有；不得双写到 ECS Component、存档或网络快照。
- 成功、unsupported、native failure、duplicate release 和 process shutdown 分别建模为可检查结果。
- 失败后不重试无限次；keep-awake 可在宿主启动阶段至多一次，release 在明确的 shutdown/error/failure path 执行，具体次数由 verifier 固定。
- 日志和诊断只读取结果，不作为下一次业务状态变更的触发器。

## 4. 后续六组实施顺序（planned）

1. `SocialApiRegistry`: 先确认 session/world owner，再把 `SocialAPI` 的 provider refs 收进 `proposed SocialProviderRegistryAdapter`；只把 mode/availability 等稳定值输出到 `proposed ExternalPlatformSessionComponent`。
2. `SocialTransportIpc`: 定义 bytes framing、BufferSize、cancel/close 和 callback thread contract；实现 Adapter 后再让 `proposed IpcTransportLifecycleSystem` 发布 decoded events。
3. `WorkshopAndJoinBoundaryData`: 先定义 external ID/path/value-object adapter DTO，再实现 join request validation/removal command 和 workshop projection；不能直接把第三方对象放入 Component。
4. `SharedResourcePackAdapters`: 先实现 path/file/zip/manifest/icon port 和 deterministic metadata projection，再接入 selection component 与 persistence/UI owner。
5. `CryptographicDependency`: 先锁定 Version4/完整参考算法版本、encoding、salt、work factor 和 secret-seed compatibility；通过 `proposed SecretDerivationAdapter` 隔离实现。
6. `NatPortMappingInterop`: 先定义 mapping key、ensure/release 幂等语义和 COM failure/unknown-result policy，再实现 Adapter，最后接入 Netplay integration owner。

## 5. 双写、兼容和提交边界

- 第一阶段不做 ECS/旧对象双写。若整合决定需要渐进迁移，旧静态入口只能成为 compatibility facade，所有写入必须汇聚到一个 proposed Adapter/System owner。
- `SocialAPI`、`CloudSocialModule`、`IPCBase`、`ResourcePack`、`BCrypt` 和 NAT COM 接口的外部 API/类型不得直接成为新 Component 字段。
- Workshop `workshopEntryId`、平台 user identifier、lobby/network ID、resource path 和 persistent save path 必须分开建模；不得以 `Id` 单字段复用。
- 网络快照只允许输出稳定值对象/Projection；持久化只保存经整合确认的选择和配置，不保存 PipeStream、ZipFile、Texture2D、CancellationTokenSource、COM handle 或 provider object。
- 提交边界：System 计算意图，Command 提交一次外部副作用；Adapter 返回成功/失败/unknown/取消结果；Projector 只读结果生成 UI、日志、网络或存档视图。

## 6. SocialApiRegistry 检查点（7/59）

目标路径和类型仍为 `status: proposed`，最终生产注册尚未决定；对应的 `src2` 实现文件已创建并保持按能力组织：

| source member(s) | proposed target role/file | single write owner | implementation note |
|---|---|---|---|
| `1000 _mode`, `1009 Mode` | `proposed SocialProviderRegistryAdapter.cs`; `proposed SocialModeQuery.cs` | proposed registry Adapter | keep mode private to Adapter; expose only immutable snapshot/query result; no static field in Component |
| `1001 Achievements` | `proposed IAchievementsPort.cs` plus Adapter-owned provider slot | proposed SocialProviderRegistryAdapter | typed port hides concrete `AchievementsSocialModule`; initialization and shutdown result explicit |
| `1002 Cloud` | `proposed ICloudStoragePort.cs` plus Adapter-owned provider slot | proposed SocialProviderRegistryAdapter | paths and file bytes cross as stable request/response values; provider object never crosses |
| `1003 Network` | `proposed ISocialNetworkPort.cs` plus Adapter-owned provider slot | proposed SocialProviderRegistryAdapter / integration owner | lobby/network ID remains external/network ID; listener callbacks become explicit commands/events |
| `1004 JoinRequests` | `proposed SocialJoinRequestBridge.cs`; later candidate `proposed JoinRequestInboxComponent.cs` | proposed bridge until integration assigns inbox owner | attach/detach host tick callback explicitly; do not expose `ReadOnlyCollection<UserJoinToServerRequest>` as core state |
| `1005 _modules` | `proposed SocialProviderRegistryAdapter.cs` private ordered registry | proposed Adapter only | preserve registration-order initialize and reverse-order shutdown; no mutable collection leak |

Planned sequence:

1. Freeze the proposed registry port and immutable capability snapshot, including typed unavailable/failed/unknown outcomes.
2. Add the proposed lifecycle verifier seam with fake modules and an explicit host tick subscription handle.
3. Implement provider selection and module registration behind the Adapter; keep Steam/WeGame loader details out of Systems.
4. Publish a snapshot to the proposed Query/Projection boundary, then allow Netplay and other consumers to issue typed commands.
5. Stop commands, detach JoinRequests, shut modules down in reverse order, and release Adapter-owned references.

Compatibility policy: a legacy static `SocialAPI` facade may remain only as a proposed compatibility facade during migration; it cannot become a second writer. No ECS/legacy double-write is planned. Mode and capability persistence/network serialization are deferred to `crossSubsystemOwner: integration-review`.

Proposed focused verifiers: registry order and reverse cleanup, duplicate lifecycle calls, provider failure classification, callback detachment, immutable snapshot purity, and static checks that no external provider type enters a Component. The current focused verifier covers the first lifecycle/snapshot paths; callback detachment and static ownership checks remain open.

## 7. SocialTransportIpc 检查点（9/59）

Version4 declares the nine inventoried members at `D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs:13-29`: producer and consumer packet lists, total partial-frame bytes, list lock, broken-pipe flag, pipe stream, cancellation source, data-arrival callback, and `BufferSize`; the constructor at `:43-46` sets the default buffer size to `256`. Version4 transport methods at `:47-59` are empty or stubbed. The same-path complete reference at `D:\TRbackup\无任何删减通过编译\Terraria.Social.WeGame\IPCBase.cs:50-223` is supplemental only: it shows lock-protected buffer swapping, async read/write callbacks, message-complete frame assembly, UTF-8 encoding, cancellation/disposal, and technical failure classification. Its `_haveDataToReadFlag` is version drift absent from the Version4 inventory and is not added to the P15 count.

| source member(s) | proposed target role/file | single write owner | migration/verification note |
|---|---|---|---|
| `992 _producer`, `993 _consumer`, `994 _totalData`, `995 _listLock` | `proposed WeGameIpcTransportAdapter.cs` private buffer/framing state | proposed Adapter | preserve callback-to-drain handoff; verify frame retention, FIFO order, lock scope, and no mutable collection leak |
| `996 _pipeBrokenFlag` | Adapter transport-status state and `proposed IpcTransportStatusProjection.cs` | proposed Adapter writes; Projection reads | map technical failures to stable categories; no Component authority or automatic retry |
| `997 _pipeStream`, `998 _cancelTokenSrc` | `proposed WeGameIpcTransportAdapter.cs` resource ownership | proposed Adapter | inject/create at composition boundary; cancel before dispose; close on success, failure, and shutdown |
| `999 _onDataArrive` | `proposed IpcTransportLifecycleSystem.cs` callback/queue seam | proposed Adapter owns registration; System owns dispatch | callback may enqueue only; detach before terminal state; handler invocation outside lock |
| `1008 BufferSize` | `proposed WeGameIpcTransportOptions.cs` or port value object | proposed Adapter configuration owner | retain default `256`, validate bounds, and require explicit session restart/rejection for changes |

Proposed sequence: freeze a stable frame/payload contract; add fake-pipe and callback seams; implement adapter-owned buffering and read/write lifecycle; add a designated-thread drain System; publish decoded events/commands; then connect protocol consumers. No target file is created in this session.

Compatibility and rollback: retain the old IPC entry point as a proposed facade only while the Adapter is the sole writer. If frame boundaries, callback order, or remote-result semantics cannot be proven, do not switch the default owner; keep the proposed seam unregistered and restore the previous caller path. A local send acknowledgement must not be used to infer remote processing; unknown outcomes require provider-specific confirmation or manual recovery, not blind retry.

## 8. WorkshopAndJoinBoundaryData 检查点（17/59）

目标路径和类型仍为 `status: proposed`，最终生产注册尚未决定；对应的 `src2` IPC adapter、port、frame 和 lifecycle System 已保存。

| source member(s) | proposed target role/file | single write owner | implementation note |
|---|---|---|---|
| `977 EnabledByDefault` | `proposed ICloudCapabilityPort.cs` plus `proposed CloudCapabilityProjection.cs` | proposed Cloud adapter | expose a stable capability result to world metadata; persistence owner is `crossSubsystemOwner: integration-review` |
| `978-982 FoundWorkshopEntryInfo.*` | `proposed WorkshopEntrySnapshot.cs` and `proposed WorkshopBoundaryAdapter.cs` | proposed Workshop adapter | copy external ID, visibility, tags, preview path, and published version into immutable DTO; keep external ID distinct from entity/network/persistence IDs |
| `983 RichPresenceState.GameMode` | `proposed RichPresenceProjection.cs` and `proposed RichPresenceQuery.cs` | proposed query computes; Projection emits | map explicit host/game snapshot to five modes; provider update is a side effect after projection change |
| `984-985 _requests`, `CurrentRequests` | `proposed JoinRequestInboxComponent.cs` candidate plus `proposed JoinRequestBoundaryAdapter.cs` | Adapter owns provider request; integration-assigned System owns approved snapshot | defensive copy only; define expiry, duplicate replacement, accept/reject removal, generation, and callback detachment before implementation |
| `986 WorkshopIssueReporter._reports` | `proposed WorkshopIssueProjection.cs` and Adapter-owned report store | proposed Workshop adapter | bounded/immutable snapshots; diagnostic clock and UI notification stay outside gameplay authority |
| `987-989 UsedTags`, `Publicity`, `PreviewImagePath` | `proposed WorkshopPublishRequest.cs` and `proposed WorkshopPublishCommand.cs` | proposed command boundary | validate/copy values; provider maps tag API names and visibility; path is external file input |
| `990-991 WorkshopTagOption.*` | `proposed WorkshopTagValue.cs` adapter DTO | proposed Workshop adapter | preserve localization key vs API name distinction; no generic content-ID reuse |
| `1006-1007 UserJoinToServerRequest.*` | fields in `proposed JoinRequestSnapshot.cs` | proposed boundary adapter | sanitize display name and preserve full external identifier separately; no third-party request object in Component |

Planned sequence:

1. Freeze provider-neutral DTOs and separate ExternalWorkshopId, ExternalUserIdentifier, entity ID, persistence ID, and network ID.
2. Add fake Workshop/Cloud ports, injected clock, and deterministic request-validity seam before any ECS registration.
3. Implement Adapter-owned provider calls and immutable result/error/unknown mapping; keep publish/download/import effects outside Systems.
4. If integration approves it, register only a third-party-free JoinRequest snapshot Component and process accept/reject through one explicit Command owner.
5. Add Rich Presence and issue-report projections after core state transitions, with defensive copies and bounded retention.

Compatibility policy: retain legacy DTOs only behind a proposed Adapter facade; do not double-write ECS and legacy request/report collections. Unknown Workshop publish/import results require provider lookup or manual recovery, never blind retry. If callback or cleanup semantics cannot be proven, leave the proposed Component unregistered and keep the existing external owner.

## 9. 回滚计划

1. 若 focused verifier 发现旧 API 调用顺序变化，停止新 Adapter 接入，保留未注册的 proposed seam，恢复调用者继续使用旧路径。
2. 若外部副作用已发出但结果未知（Workshop publish、IPC send、NAT Add），不得盲目重试；先通过 provider-specific query 或人工恢复策略确认，再决定补偿。
3. 若序列化/网络字段无法兼容，回滚 Component/Projection 注册，不删除旧外部字段；将迁移标记为 blocked 并交给 integration-review。
4. 若资源释放验证失败，禁止切换默认 owner；保留旧生命周期，修复 close/dispose verifier 后再重试。
5. 回滚不包括修改 Version4、输入报告、ledger、lock 或其他分区文档。

## 10. SharedResourcePackAdapters 检查点（10/59）

目标路径和类型仍为 `status: proposed`，最终生产注册尚未决定；对应的 `src2` Workshop/Join adapter、snapshot、query 和 projection 已保存。

| source member(s) | proposed target role/file | single write owner | implementation note |
|---|---|---|---|
| `2753 FullPath`, `2754 FileName` | `proposed ResourcePackMetadataSnapshot.cs`; `proposed ResourcePackBoundaryAdapter.cs` | proposed ResourcePack Adapter | validate and normalize external paths; keep FileName as discovery/display key, never as a universal ID |
| `2755 _services` | Adapter composition dependency in `proposed ResourcePackBoundaryAdapter.cs` | proposed Adapter | resolve asset/content services only at the boundary; no service locator in Components |
| `2756 IsCompressed`, `2757 Branding` | fields in `proposed ResourcePackMetadataSnapshot.cs` | proposed Adapter | derive storage format and origin from validated input; expose immutable metadata only |
| `2758 _zipFile`, `2759 _icon` | private resources in `proposed ResourcePackBoundaryAdapter.cs` | proposed Adapter | zip/texture handles remain private; close/release on parse failure, replacement, cancellation, and shutdown |
| `2760 ICON_FILE_NAME`, `2761 PACK_FILE_NAME` | Adapter-internal constants | proposed Adapter | preserve `icon.png` and `pack.json` compatibility names; no ECS registration keys |
| `2762 ResourcePackList._resourcePacks` | `proposed ResourcePackCollectionAdapter.cs` private list | proposed collection Adapter | copy input enumeration, deduplicate deterministically, and return defensive metadata snapshots |

Planned sequence: define file/zip/manifest/content/asset ports; build fake fixtures for directories, archives, invalid manifests, missing icons, and duplicate names; implement Adapter resource ownership and cleanup; add immutable metadata Projection; then ask integration review whether selection/persistence is in scope. Complete-reference `IsEnabled`, `SortingOrder`, manifest properties, content source, JSON serialization, and workshop discovery are version drift and remain deferred until their owner and format are confirmed.

Compatibility and rollback: leave legacy ResourcePack objects behind a proposed facade while the Adapter is verified; do not double-write selection state. If manifest parsing, path identity, ordering, or resource release differs from the established contract, do not register the proposed selection Component; retain the existing owner and fix the verifier first. Invalid external files must not be silently treated as valid empty packs.

## 11. CryptographicDependency 检查点（10/59）

目标路径和类型仍为 `status: proposed`，最终生产注册尚未决定；对应的 `src2` ResourcePack adapter、collection 和 metadata projection 已保存。

| source member(s) | proposed target role/file | single write owner | implementation note |
|---|---|---|---|
| `4054 DefaultRounds`, `4055 BCryptSaltLen`, `4057 BlowfishNumRounds`, `4059 EmptyString`, `4060 DefaultHashVersion`, `4061 Nul`, `4062 MinRounds`, `4063 MaxRounds` | `proposed SecretDerivationAdapter.cs` private compatibility constants | proposed SecretDerivationAdapter | preserve numeric/string values behind an explicit compatibility profile; no ECS registration or persistence field |
| `4056 SafeUTF8` | `proposed ISecretDerivationPort.cs` encoding seam plus Adapter implementation | proposed Adapter | define invalid input behavior and distinguish outer `Encoding.UTF8` behavior from BCrypt strict UTF-8 behavior |
| `4058 Index64` | Adapter-private immutable decode table | proposed Adapter only | do not expose mutable array; verify table and output vectors against selected complete reference |

The implementation sequence is: freeze the algorithm/encoding/salt/work-factor contract; add deterministic output-vector and validation seams; implement the Adapter with isolated work buffers; connect `Secrets.ToSecret`/WorldGen through the port; then run focused vector, determinism, invalid-input, and ownership checks. Version4 `CryptRaw` is currently stubbed, so no implementation or compatibility claim is made here.

The outer observed contract must remain ordered: UTF-8 input, first raw transform with fixed 16-byte salt and work factor 4, exactly 1000 deterministic swaps, second raw transform, standard base64 output. `DefaultRounds = 11` is not substituted for the observed value. A legacy static facade can remain as a proposed compatibility facade only; no ECS/legacy double write or secret persistence is planned.

Rollback: if output vectors or WorldGen secret-seed checks differ, keep the proposed Adapter unregistered, restore the existing caller path, and block on compatibility review. Do not “fix” a mismatch by changing the salt, round count, encoding, or swap loop without a new explicit contract and vectors. Do not log failed plaintext input or intermediate bytes.

## 12. 第一组 focused verifier 计划

以下均为后续计划，不是本会话已运行结果：

| verifier | scope | expected assertion |
|---|---|---|
| `PlatformExecutionPortVerifier` (proposed) | fake OS port | Windows/unsupported 分支、keep-awake/release 顺序、duplicate release、native failure 分类 |
| `MainLifecycleAdapterVerifier` (proposed) | host lifecycle harness | dedicated start、world-load failure、normal shutdown 都提交正确 command；System 不直接 I/O |
| `PlatformTargetLayoutVerifier` (proposed) | static scan | proposed paths follow domain-first layout; no Component owns native constants; no raw P/Invoke outside Adapter |

本轮不运行 `dotnet`、编译、测试或行为等价验证。若将来新增 C# 项目，必须遵守仓库的 serial dotnet wrapper、active process check、`Build/bin`/`Build/obj` 输出和 `--no-build --no-restore` verifier 规则。

## 13. NatPortMappingInterop 检查点（4/59）

目标路径和类型仍为 `status: proposed`，最终生产注册尚未决定；对应的 `src2` crypto port 和 ordered transform adapter 已保存。

| source member(s) | proposed target role/file | single write owner | implementation note |
|---|---|---|---|
| `4064 InternalPort`, `4065 Protocol`, `4066 InternalClient` | `proposed NatPortMappingKey.cs` value object populated by `proposed NatPortMappingAdapter.cs` | proposed NAT Adapter | materialize COM observations before comparison; keep internal port, protocol, and client distinct from external/network/entity IDs |
| `4067 StaticPortMappingCollection` | private COM handle in `proposed NatPortMappingAdapter.cs` | proposed NAT Adapter | activation, enumeration, Add/Remove, error mapping, and reference release stay at one boundary |

Planned sequence: define the mapping key and typed ensure/release results; add a fake COM collection seam; implement activation and enumeration matching the observed internal-port/client/TCP rule; add ownership-aware Add and Release policy; connect the lifecycle System after listener startup and on stop/failure; then emit a non-authoritative status projection. The collection's `Add` and `Remove` methods are related interface operations but are not additional P15 property members.

The legacy `Netplay.OpenPort` path may remain behind a proposed compatibility facade while the Adapter is verified. It must not be double-written by a new System. Because Version4 has no confirmed cleanup call, the proposed default is to release only mappings with an explicit session ownership record; if ownership cannot be proven, report `not-owned` or `unknown` and leave the mapping untouched. A COM exception after Add or Remove is not proof of no effect, so confirmation must precede any retry.

Rollback: if listener ordering, matching, ownership, or cleanup differs from the locked behavior, do not switch the default NAT owner; keep the proposed port unregistered and restore the prior path. If an external operation is unknown, do not issue repeated Add/Remove calls without a fresh query or integration-approved compensation. Release COM references and temporary enumerators on normal stop, failed startup, cancellation, and exceptional exit; the concrete Version4 cleanup path remains an evidence gap.

## 14. 第一组 focused verifier 计划

- `evidence-gap`: Version4 的 `Main` 已确认 call sites，但 native return/error semantics、所有 client shutdown paths 和 repeated lifecycle semantics 未完全闭合。
- `evidence-gap`: P15 first-round report 不存在；当前设计直接引用输入报告和 Version4 evidence，不把缺失的第一轮报告当作已完成研究。
- `blocking-decision`: `Main` 平台生命周期属于 RuntimeComposition 还是独立 host adapter，需要 integration-review；这会影响目标项目和调度入口。
- `blocking-decision`: 是否允许把平台状态快照暴露给诊断/客户端 projection，必须由 integration-review 决定；默认不持久化、不网络同步。

## 15. Checkpoint handoff

checkpointStatus: explicitly-rebound-and-reverified
completedComponents: reverified carried-forward MainPlatformExecutionBoundaryAdapter; reverified carried-forward SocialProviderRegistryBoundary; reverified carried-forward WeGameIpcTransportBoundary; reverified carried-forward WorkshopJoinBoundary; reverified carried-forward ResourcePackBoundaryAdapter; reverified carried-forward CryptoBoundaryAdapter; reverified carried-forward NatPortMappingBoundaryAdapter (focused verifier passed; no new Component file was authorized or added)
currentComponent: completed; evidence and integration review remain
pendingComponents: none (focused source verifier passed; broader host/provider/compatibility evidence remains open)
lastCheckpointUtc: 2026-09-12T07:29:40.951Z
executionStatus: completed
implementationStatus: completed
verificationStatus: passed (focused src2 verifier re-run under current session)

implementationFiles:
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/IPlatformExecutionStatePort.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/MainPlatformExecutionBoundaryAdapter.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/PlatformExecutionLifecycleSystem.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/PlatformExecutionResult.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/PlatformExecutionSnapshot.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/WindowsPlatformExecutionStatePort.cs
- src2/ExternalPlatformBoundaries/Terraria.ExternalPlatformBoundaries.csproj
- src2/ExternalPlatformBoundariesVerification/Program.cs
- src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderMode.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderCapabilities.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderLifecycleResult.cs
- src2/ExternalPlatformBoundaries/Social/Provider/ISocialProviderModule.cs
- src2/ExternalPlatformBoundaries/Social/Provider/ISocialJoinRequestTickSource.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderRegistrySnapshot.cs
- src2/ExternalPlatformBoundaries/Social/Provider/ISocialProviderRegistryPort.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderRegistryAdapter.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialRegistryLifecycleSystem.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialModeQuery.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcTransportOptions.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IWeGameIpcPipe.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcFrame.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IpcTransportResult.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcTransportStatus.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IWeGameIpcTransportPort.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcTransportAdapter.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IpcTransportLifecycleSystem.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopPublicity.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopOperationStatus.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopOperationResult.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopTagValue.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopEntrySnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopPublishRequest.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopLookupRequest.cs
- src2/ExternalPlatformBoundaries/Workshop/IWorkshopProviderPort.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopBoundaryAdapter.cs
- src2/ExternalPlatformBoundaries/Workshop/CloudCapabilitySnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/CloudCapabilityProjection.cs
- src2/ExternalPlatformBoundaries/Workshop/JoinRequestSnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/JoinRequestInboxAdapter.cs
- src2/ExternalPlatformBoundaries/Workshop/RichPresenceGameMode.cs
- src2/ExternalPlatformBoundaries/Workshop/RichPresenceGameSnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/RichPresenceQuery.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopIssueReportSnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopIssueReportStore.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackBranding.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackLoadStatus.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackMetadataSnapshot.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackLoadResult.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackBoundaryAdapter.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackCandidate.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackDiscoveryResult.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackCollectionAdapter.cs
- src2/ExternalPlatformBoundaries/Cryptography/ISecretDerivationPrimitive.cs
- src2/ExternalPlatformBoundaries/Cryptography/ISecretDerivationPort.cs
- src2/ExternalPlatformBoundaries/Cryptography/SecretDerivationAdapter.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingStatus.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingKey.cs
- src2/ExternalPlatformBoundaries/Nat/IStaticPortMappingSnapshot.cs
- src2/ExternalPlatformBoundaries/Nat/INatPortMappingCollectionPort.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingResult.cs
- src2/ExternalPlatformBoundaries/Nat/INatPortMappingPort.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingAdapter.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingLifecycleSystem.cs

verificationEvidence:
- command: pwsh -NoProfile -Command "$dotnetArgs = @('build', '.\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\Build\Tools\Invoke-SerialDotnet.ps1 @dotnetArgs"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  warnings: 0
  errors: 0
  output: Build/bin/Terraria.ExternalPlatformBoundaries/Debug/net10.0/Terraria.ExternalPlatformBoundaries.dll; Build/bin/Terraria.ExternalPlatformBoundariesVerification/Debug/net10.0/Terraria.ExternalPlatformBoundariesVerification.dll
- command: pwsh -NoProfile -Command "$dotnetArgs = @('run', '--project', '.\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj', '--no-build', '--no-restore'); & .\Build\Tools\Invoke-SerialDotnet.ps1 @dotnetArgs"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  output: P15 external-platform boundary verifier passed.
- command: pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments "build .\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  warnings: 0
  errors: 0
  output: Build/bin/Terraria.ExternalPlatformBoundaries/Debug/net10.0/Terraria.ExternalPlatformBoundaries.dll; Build/bin/Terraria.ExternalPlatformBoundariesVerification/Debug/net10.0/Terraria.ExternalPlatformBoundariesVerification.dll
- command: pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments "run --project .\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj --no-build --no-restore"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  output: P15 external-platform boundary verifier passed.
- scope: focused verifier passed for platform lease results, social lifecycle order/cleanup, IPC framing/UTF-8/close, Workshop defensive snapshots, Join generation/expiry, Rich Presence derivation, directory/zip ResourcePack metadata, ordered two-pass secret transform, and NAT matching/ownership.
- not proven: real provider registration, host lifecycle wiring, real PipeStream/COM/asset services, complete BCrypt raw primitive compatibility, production registration, behavior equivalence, network/persistence closure, and cross-partition ownership.
