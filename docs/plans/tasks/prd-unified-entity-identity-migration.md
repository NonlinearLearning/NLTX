# PRD：统一 Entity 身份根与 typed projection 迁移

状态：2026-10-07 已按 accepted ADR 与当前运行时生命周期语义校准。本文保留原始需求历史，
并以本版本取代先前要求 Player 普通重生换根、强制 Arch 类型/目录的措辞。本 PRD 定义身份
行为与边界，不指定 ECS 框架或运行时句柄实现。

## 1. 简介与概述

当前运行时同时使用 `whoAmI`、Projectile identity/UUID、`PlayerHandle`、`NpcHandle`、
`ReplicationId`、账户 UUID、持久化标识和运行时实体句柄（原草案曾将 Arch `Entity` 列为一种实现）。这些值在不同作用域内各自有意义，
却经常被当作“实体唯一身份”使用，导致协议槽位、运行时引用、复制游标、账户归属和存档
引用之间存在串用风险。

本 PRD 定义一套完整迁移架构：以服务器生成的 `EntityUuid` 作为权威 ECS 实体实例的唯一
身份根；以 Registry 维护根身份与当前运行时实体的登记；以强类型 projection/handle 和
Adapter 连接协议、复制、账户、持久化和 Terraria 兼容边界。首批范围覆盖 Player、NPC、
玩家 Projectile 和 NPC Projectile，保持现有 Terraria 网络包格式不变。

本文是需求文档，不是实现计划。它不授权立即修改 C#，也不规定提交顺序。

## 2. 目标

- 为每个持续参与服务器权威模拟的首批实体提供一个不可混淆的 `EntityUuid` 根身份。
- 消除核心系统把 `whoAmI`、Projectile UUID、ReplicationId、账户 UUID 或运行时实体句柄
  当作全局主键的用法。
- 保留现有协议字段和客户端兼容行为，不修改当前 Terraria 网络包格式。
- 让 Player/NPC/Projectile 的作用域标识通过强类型 Adapter 映射到根身份。
- 明确实体创建、销毁、重生、重建、断线重连和预测确认时的身份生命周期。
- 使身份解析失败可分类、可观测、可测试，禁止因槽位复用而静默绑定错误实体。
- 为未来 WorldItem、TileEntity、Chest 等服务器权威对象接入预留一致规则，但不纳入首批迁移。

## 3. 用户故事

### US-001：服务器为权威实体分配身份根

**描述：**作为服务器模拟系统开发者，我希望每个权威 Player、NPC 和 Projectile 实例都有
一个唯一的 `EntityUuid`，以便跨系统日志、事件和诊断时不会混淆实体。

**验收标准：**

- [ ] Player、NPC、玩家 Projectile、NPC Projectile 在正式 Spawn/Commit 后均拥有非零 `EntityUuid`。
- [ ] `EntityUuid` 只能由服务器权威创建流程生成，客户端或协议输入不能指定、覆盖或复用。
- [ ] 同一实体实例在其生命周期内 UUID 不变；销毁后该 UUID 不再解析到任何新实体。
- [ ] Registry 拒绝零值和重复 UUID 注册。

### US-002：实体生命周期与身份失效

**描述：**作为生命周期系统开发者，我希望区分玩法复活与实例重建：仍存活的 Player 实例在
死亡后普通复活时保留身份根；真正销毁后重建的 Player、NPC 或 Projectile 使用新根，以免旧引用
解析到新实例。

**验收标准：**

- [ ] Player 普通死亡/复活期间 `EntityUuid` 不变；销毁并重建 Player、重建 NPC、重新发射
  Projectile 或断线后新建 Player 实例取得新 UUID。
- [ ] 旧 UUID 的解析结果为 `Expired` 或 `NotFound`，不能指向复用槽位中的新实体。
- [ ] 实体销毁时 Registry 清理当前 ECS Entity 与全部运行时 projection 映射。
- [ ] 账户身份和持久化身份不因运行时实体重建而改变。

### US-003：Registry 管理根身份关系

**描述：**作为运行时系统开发者，我希望通过统一 Registry 将根身份解析到所属 runtime 中的
当前实体句柄，而不让 Registry 重新承载所有实体领域状态。

**验收标准：**

- [ ] Registry 登记 `EntityUuid`、所属 runtime 中的当前实体句柄和有效 typed projection。
- [ ] Registry 只负责登记、解析、冲突检测和生命周期清理，不保存 Player/NPC/Projectile 领域字段。
- [ ] 解析接口返回明确成功结果或 typed 失败结果，不返回可猜测的默认实体。
- [ ] 并发或重复注册不会产生两个当前实体绑定同一根 UUID。

### US-004：保留并强化 typed Handle 与协议 identity

**描述：**作为领域系统开发者，我希望继续使用 `PlayerHandle`、`NpcHandle` 和 Projectile 协议
identity 进行高频查找，同时保证它们不能跨域互换。

**验收标准：**

- [ ] `PlayerHandle`、`NpcHandle` 保持强类型，不能通过裸 `int` 隐式跨域转换。
- [ ] Projectile 协议 identity 明确包含其作用域（至少所有者/协议会话所需范围）。
- [ ] 每种 projection 都能解析到 `EntityUuid` 或当前 runtime 实体句柄。
- [ ] 槽位复用、会话结束或作用域不匹配时解析失败，不静默指向新实体。

### US-005：保持 Terraria 网络协议兼容

**描述：**作为协议维护者，我希望现有 Terraria 客户端无需升级即可继续通信，同时服务器内部
使用统一身份根。

**验收标准：**

- [ ] 现有 Terraria 网络包格式和字段编码不变。
- [ ] `EntityUuid` 不直接加入现有协议包。
- [ ] `whoAmI`、Projectile identity 等协议字段只在协议 Adapter 中编码/解码。
- [ ] 协议 Adapter 能将有效 projection 映射到服务器实体，无法解析时拒绝或请求重同步。
- [ ] 未认证客户端不能通过网络提交 UUID 访问任意服务器实体。

### US-006：分离持久化身份与运行时身份

**描述：**作为存档系统开发者，我希望跨会话对象保持业务身份，但不把运行时 UUID 当存档主键。

**验收标准：**

- [ ] `EntityUuid` 不作为默认持久化主键。
- [ ] 需要跨会话保留的对象使用独立 typed `PersistentEntityId`。
- [ ] 恢复时先读取持久化身份，再创建新运行时实体并生成新的 `EntityUuid`。
- [ ] 恢复流程登记 `PersistentEntityId -> EntityUuid` 映射，并能处理引用修复失败。
- [ ] 首批临时 Projectile 不写入持久化身份。

### US-007：支持预测实体和纯表现对象

**描述：**作为客户端/表现系统开发者，我希望预测和渲染对象可以独立追踪，而不会冒充服务器
权威实体。

**验收标准：**

- [ ] 客户端预测实体使用独立 `PredictionEntityId` 或等价临时标识。
- [ ] 预测/表现/插值/协议解码临时对象不挂载服务器 `EntityIdentityComponent`。
- [ ] 权威确认通过协议 projection 或命令关联，而不是接受客户端提供的 `EntityUuid`。
- [ ] 预测对象与权威对象的关联失败不会写入错误的根身份。

### US-008：单向迁移与旧字段弃用

**描述：**作为迁移负责人，我希望逐域替换旧身份调用，避免新旧字段双写造成不一致。

**验收标准：**

- [ ] 新核心代码只依赖 `EntityUuid`、typed projection 和 Registry/Adapter 接口。
- [ ] 旧字段只作为 Adapter 的输入/输出投影，不再作为第二权威存储。
- [ ] 迁移路径明确为单向投影，禁止新旧身份字段隐式双向同步。
- [ ] 每个域完成 focused verifier 后，才移除旧核心写路径。
- [ ] 旧 API 的弃用、解析失败和冲突均有日志或指标记录。

### US-009：身份解析失败可观测且安全

**描述：**作为安全和运维人员，我希望身份错误不会产生错误副作用，并能区分失败原因。

**验收标准：**

- [ ] Registry/Adapter 至少区分 `NotFound`、`Expired`、`ScopeMismatch`、`Conflict`。
- [ ] 客户端命令解析失败时拒绝命令并记录安全审计事件。
- [ ] 服务器内部事件解析失败时进入隔离/死信路径并暴露诊断指标。
- [ ] 网络快照解析失败时丢弃投影并等待完整快照或重同步。
- [ ] 持久化恢复解析失败时标记引用修复失败，禁止自动绑定到新实体。

## 4. 功能需求

- **FR-1：根身份组件**：系统必须提供一个共享的规范 `EntityIdentityComponent`，其运行时字段
  语义为 `EntityUuid`，内部可使用 128 位 `Guid` 承载；其程序集和存储位置由已接受的项目依赖边界决定。
- **FR-2：生成权**：系统必须只允许服务器权威 Spawn/Commit 流程生成根身份；零值和外部指定值必须被拒绝。
- **FR-3：生命周期**：系统必须保证一个 `EntityUuid` 只绑定一个实体实例；实体销毁或重建后旧身份失效。
- **FR-4：Registry**：系统必须提供身份登记、解析、冲突检测和清理能力，将根解析到当前 runtime
  实体句柄；Registry 不得拥有领域状态。
- **FR-5：投影索引**：系统必须为 Player、NPC、玩家 Projectile、NPC Projectile 的 Handle、协议 identity
  和复制 projection 提供带类型、作用域和生命周期的映射。
- **FR-6：账户边界**：系统必须保留 `AccountUuid` 作为跨连接账户身份，并映射到当前 Player 运行时实体，不得以 `EntityUuid` 替代。
- **FR-7：持久化边界**：需要跨会话的对象必须使用独立 `PersistentEntityId`；恢复必须创建新运行时根身份。
- **FR-8：协议兼容**：系统必须保持现有 Terraria 网络包格式；`EntityUuid` 不得成为现有包的通用实体字段。
- **FR-9：兼容索引**：`whoAmI` 必须限制在 Compatibility Adapter 中，不得写入 ECS 权威状态。
- **FR-10：Projectile 兼容**：Projectile 原有 UUID 不得继续作为第二权威根；协议 identity 仍可作为作用域 projection。
- **FR-11：单向迁移**：所有旧身份字段迁移必须通过单向 Adapter projection 完成，禁止隐式双写。
- **FR-12：失败契约**：所有身份解析必须返回显式 typed 结果，并按命令、内部事件、网络快照、持久化恢复分别处理。
- **FR-13：预测隔离**：预测、表现和临时解码对象不得写入服务器 `EntityIdentityComponent`。
- **FR-14：未来扩展**：WorldItem、TileEntity、Chest 等对象只需记录接入规则，不纳入本次首批迁移交付。

## 5. 非目标

- 不修改现有 Terraria 网络包格式，不要求客户端立即支持 `EntityUuid`。
- 不用一个 UUID 直接替换 `PlayerHandle`、`NpcHandle`、`ReplicationId`、账户 UUID、持久化 ID 或运行时实体句柄。
- 不把 Registry 设计为包含所有实体字段的全局 Manager。
- 不在首批迁移中重构 WorldItem、TileEntity、Chest、掉落物等非 Entity 派生对象。
- 不改变 Player、NPC、Projectile 的生命周期、AI、所有者、伤害、复制顺序或表现语义。
- 不把预测实体或纯表现对象提升为服务器权威实体。
- 不在本 PRD 中指定具体提交顺序、分支策略或立即执行 C# 拆分。

## 6. 设计考虑

### 6.1 身份层次

```text
服务器权威 ECS 实体
        │
        └── EntityIdentityComponent.EntityUuid  （唯一运行时根身份）
                │
                ├── Identity Registry ── 当前 runtime 实体句柄
                ├── PlayerHandle / NpcHandle projection
                ├── ProjectileProtocolIdentity projection
                ├── ReplicationId projection
                ├── AccountUuid / PersistentEntityId projection
                └── whoAmI Compatibility Adapter projection
```

根身份不编码实体类型、时间或槽位。projection 必须声明类型、作用域、有效期和失效行为。

### 6.2 生命周期与所有权

- Spawn/Commit 生成并登记根身份；领域 Spawn System 继续拥有 Player/NPC/Projectile 专属状态。
- Registry 拥有身份关系，Adapter 拥有外部投影；两者都不拥有 Health、AI、Lifetime 或 Combat 状态。
- 普通玩法死亡/复活不改变仍存活实例的根。实际销毁和重建触发旧根失效并生成新根；账户/
  持久化域可独立保留自己的身份。
- 所有跨实体关系通过强类型引用和 Registry 解析，不允许裸整数或裸 Guid。

### 6.3 网络与持久化

- 现有 Terraria 协议仍使用槽位、Projectile identity 等既有字段；Adapter 在边界转换。
- `EntityUuid` 仅供服务器内部日志、诊断、事件关联和跨系统追踪。
- 持久化对象使用 `PersistentEntityId`；恢复建立到新 `EntityUuid` 的映射。
- 新协议如需实体引用，必须版本化并声明作用域、权限和有效期。

## 7. 技术考虑

- 身份组件、Registry、runtime handle 和 projection 契约位于共享领域边界；不得要求某个 ECS
  框架类型成为跨领域身份根。具体目录和项目引用遵循仓库已接受的依赖方向。
- 根类型使用 `EntityUuid` 语义并拒绝零值；底层值表示不改变其实例身份含义。
- Registry 必须提供高频 typed lookup，避免每次通过全局扫描 ECS 查询实体。
- Projection 索引必须在槽位复用、会话结束和实体销毁时原子失效，避免 ABA 式错误绑定。
- 日志可输出规范化 UUID 字符串，核心组件保留值类型而非字符串。
- 任何新的公共组件必须避免重新聚合生命周期、网络、账户和领域状态。

## 8. 成功指标

- 首批四类权威实体的根身份覆盖率达到 100%。
- 核心 Simulation 代码中不再将 `whoAmI`、Projectile UUID、ReplicationId、账户 UUID 或运行时实体句柄作为全局主键。
- 现有 Terraria 协议兼容验证保持通过，包格式和客户端行为无回归。
- 所有身份解析失败均可按 typed 原因统计，且不存在静默槽位复用绑定。
- Registry 查询不需要扫描整个 ECS 世界；关键路径使用有界 typed 索引。
- 身份生命周期测试覆盖创建、销毁、普通死亡/复活保根、销毁重建换根、断线重连、槽位复用和预测确认。
- 迁移后的新旧身份字段不存在双向隐式写入。

## 9. 验收与验证要求

实现阶段至少需要以下 focused verifier；本 PRD 创建时不执行这些测试：

1. **Identity lifecycle verifier**：验证四类实体创建、销毁、重生/重建和 UUID 失效。
2. **Registry conflict verifier**：验证零值、重复注册、并发登记、槽位复用和显式失败原因。
3. **Projection mapping verifier**：验证 Player/NPC Handle、Projectile protocol identity、ReplicationId 的类型和作用域隔离。
4. **Protocol compatibility verifier**：验证现有包字节布局不变，`whoAmI` 和 Projectile identity 只经 Adapter 转换。
5. **Persistence remap verifier**：验证 `PersistentEntityId` 恢复到新 `EntityUuid`，并拒绝错误引用自动绑定。
6. **Prediction isolation verifier**：验证预测/表现对象没有服务器根身份，权威确认不会接受客户端 UUID。
7. **Migration guard verifier**：扫描或静态断言新核心代码不存在旧身份字段双写和裸 ID 跨域关系。
8. **Regression suite**：运行受影响的 Player、NPC、Projectile、Replication、Protocol 和 Persistence 验证项目。

## 10. 开放问题

- Registry 的具体并发模型和跨线程访问契约需要在实现计划阶段确定。
- 是否需要为新协议定义版本化 `EntityReference`，取决于未来协议功能，不属于现有协议迁移。
- WorldItem、TileEntity、Chest 等对象接入持久化身份的具体格式和生命周期仍待独立 PRD。
- B9 已关闭本次旧生产声明出口：共享 `EntityIdentityState` / `EntityId` 声明已删除；literal/config/alias source audit 限于生产源码与 `Test`，排除只读 `src/NSSLC.Infrastructure/分类参考/`。该目录存在同名参考类型，不是生产声明。独立 audit evidence 明确记录排除项；主 receipt 引用的 final summary 未复述该字段，因此 0-hit 结论不得扩大到整棵 `src`。删除后的七个 Projectile mode 与 current Simulation build 有通过证据。`WorldSession.Calendar.EntityId` 是独立领域类型，合法保留；具体 caller/write-owner 与清理点见 Entity 组织执行 ledger。

## 11. 2026-10-07 需求校准记录

本次校准遵循已接受的 [Entity 身份根与 typed projection 决策](../../architecture/decisions/0001-entity-identity-root-and-typed-projections.md)
和当前 [Entity 组织约束](../../../Context/架构设计/ECSEntity组织设计约束.md)：根只在实体实例实际创建时产生，实例删除/重建时旧根失效；玩法生命周期变化本身不等同实例重建。原 PRD 的“Player 重生后重新分配 UUID”与此语义冲突，已由 US-002、FR-3 和 §6.2 明确替代，历史要求由本段留档，不继续作为验收标准。

原草案把 Arch `Entity`、特定 `Share/Entity/Components` 路径和 Arch 项目引用写成目标架构。本次改为 runtime-neutral 的实体句柄和共享身份契约；这些 Arch 字样仅说明草案当时使用的实现设想，不是项目对该 ECS 的采纳，也不是当前实现验收要求。

本 PRD 中 Projectile UUID 的身份边界仍按 FR-10 作为作用域协议 projection；B5 已交付 typed Projectile source，`ProjectileEntityState` 已删除，src/Test C# 与 csproj scoped static search 无引用；删除旧 identity 后 fresh DLL `5C631F…487B3` 上七个 domain mode 全通过。真实 Simulation 仅支持 Projectile type 1 ordinary arrow，NetworkServer packet-27/29 ingress 仍 disabled；各 mode 与实际 host 的证据范围见[Entity 组织执行 ledger](../../migration/ledgers/2026-10-06-entity-organization-execution-ledger.md)。

依据当前 Simulation 源码，`RuntimePlayerEntity.AdvanceLifecycle` 在同一 handle 上提交普通 respawn 生命周期、生命、免疫和 spawn position；`RuntimePlayerStore.TryDestroyPlayer` 与 `TryCreatePlayerAtSlot` 是销毁/新建路径。对应观察见 `Build/diagnostics/EntityOrganization/B4/simulation/npc-contact-deaths-3600.json`（3 次 PvE death，初始与最终 `EntityReference` 相同；该 tick 限制结束时玩家处于 Dead）和 `Build/diagnostics/EntityOrganization/B4/player-root-probe-final.json`（显式 destroy/recreate 后旧引用失效、新引用解析、root 和 handle 改变）。38 场景 current Simulation host matrix 已覆盖当前支持集 NetId 1,2,3,4,16,22,37,488 的 0/1/2 Player spatial/cleanup、正常 respawn、destroy/recreate 与两次 world switch；真实 WorldFile 和 Simulation actual-owner/static-immunity late-failure rollback/retry 也已通过。3600 tick exact comparison 仍有 237 项中的 11 项 Mother Slime / NetId 16 差异，保留为具名行为限制，不写为 parity pass。
