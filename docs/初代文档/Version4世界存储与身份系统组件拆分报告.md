# Version4 世界存储与身份系统组件拆分报告

## 1. 结论与范围

本报告拆分《[Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)》第 3 项“世界存储与身份”。目标是让 Tile、液体待处理索引、固定实体槽位、容器、TileEntity、区段状态和各类 ID 各有一个明确的权威写入者；它不是把 `Main` 的静态数组机械包装成一个大型 `WorldStorage` 类。

本轮只读检查了 `D:\TRbackup\Version4`。核心证据来自 `Terraria/Main.cs`、`Tile.cs`、`Liquid.cs`、`LiquidBuffer.cs`、`WorldSections.cs`、`Chest.cs`、`Sign.cs`、`DataStructures/TileEntity.cs`、`DataStructures/TileEntitiesManager.cs`、`IO/WorldFile.cs`、`NetMessage.cs`、`MessageBuffer.cs`、`WorldGen.cs`、`Entity.cs`、`Player.cs`、`NPC.cs`、`Projectile.cs` 与 `WorldItem.cs`。

同时只读检查了 `C:\Users\shan\Downloads\ECS\space-station-14-master`。该目录的 `RobustToolbox` Git 子模块未初始化，因而底层 `EntityUid`、`NetEntity`、`IEntityManager`、地图和容器实现不能作为已验证源码证据。本报告只从已存在的上层调用方提取组织模式，不复制 SS14 代码、命名或领域语义。

不在本系统范围内：世界规则/进度（`WorldSession`）、Tile 定义（`ContentCatalog`）、碰撞/液体流算法、Tile 放置/电线等玩法规则、NPC/Player/Projectile 的内部玩法状态、网络连接会话、UI/渲染。它们通过 Query、Command 或 Projection 使用存储，不直接拥有存储的内部集合。

## 2. 真实代码盘点与证据状态

| 成员簇 | Version4 实现与证据 | 读/写者及生命周期 | 状态种类 | 候选归属 | 状态 |
| --- | --- | --- | --- | --- | --- |
| `Main.tile` 与 `Tile` 的 type/wall/liquid/header/frame | `Terraria/Main.cs:928`；`Terraria/Tile.cs:7-24,57-115` | WorldGen 初始化/重置（`WorldGen.cs:6412-6659`），物理、交互、液体、网络解码（`MessageBuffer.cs:801-1037,1903-1910`），存档（`WorldFile.cs:1481-1617,2584-2767`） | 世界权威、持久化、网络复制 | `TileMapStore` | confirmed |
| 液体活跃单元与缓冲队列 | `Liquid.cs:28,97,1003-1210,1525-1529`；`LiquidBuffer.cs:5-30`；坐标仍指向 `Main.tile` | 液体系统独占维护，世界重置归零 | 暂态调度/缓存，不是第二份液体真值 | `LiquidWorkQueueState` + `LiquidSimulationSystem` | confirmed |
| `Main.player/npc/projectile/item` 固定数组及 `whoAmI` | `Main.cs:932-944,982`；`Initialize_Entities` 在 `Main.cs:3452-3494` 将数组下标写入 `whoAmI`；基类字段在 `Entity.cs:8` | 初始化、WorldGen reset、领域生成/销毁、网络包读写 | 固定槽位协议；实例本体属各领域 | 四个 `EntitySlotStore<T>` | confirmed |
| 活动标记和槽位回收 | `Player.cs:478`、`NPC.cs:5897`、`Projectile.cs:90`、`WorldItem.cs:47`；投射物分配 `Projectile.cs:10220-10289`，NPC 分配 `NPC.cs:67051-67077` | 领域生命周期 System 写入，存储层负责原子占用/释放与 generation | 槽位生命周期；不能把 `active` 误当全局实体身份 | `EntitySlotStore<T>` + `EntityLifecycleCommand` | partial |
| `Projectile.owner + identity` 与 `Main.projectileIdentity[owner, identity]` | `Main.cs:946`；生成写入 `Projectile.cs:10272-10289`；查询 `Projectile.cs:18525-18545`；Kill 清理 `Projectile.cs:46420-46435`；网络接收写入 `MessageBuffer.cs:1357-1401` | 投射物生成、销毁、协议查找 | owner-scoped 网络/协议映射，非持久 ID | `ProjectileIdentityIndex` | confirmed |
| Chest 数组、位置索引、内容 | `Main.cs:950`；`Chest.cs:42-67,70-116`；存档 `WorldFile.cs:1653-1717` | 创建/销毁时同步数组和 `Point -> Chest` 索引；物品事务写 contents；存档在写前校验 Tile 结构 | 世界容器权威状态 | `WorldContainerStore` / `ChestIndex` | confirmed |
| Sign 槽位、位置和文本 | `Main.cs:952`；`Sign.cs:7-72`；网络写/读 `NetMessage.cs:975-977,1980-2004` 与 `MessageBuffer.cs:1848-1873`；存档 `WorldFile.cs:1727-1751` | Tile 破坏清理，读取时按 Tile 校验/分配槽位 | 世界标识牌状态 | `WorldSignStore` | confirmed |
| TileEntity ID、位置、类型、更新集合 | `DataStructures/TileEntity.cs:12-95,126-184`；类型注册 `TileEntitiesManager.cs:8-74`；逐 tick 调用 `WorldGen.cs:59400-59418` | `ByID` 与 `ByPosition` 一起写；`UpdateEntities` 随 `RequiresUpdates` 维护；WorldFile 加载后验证 Tile | 世界对象权威状态、持久化 ID、暂态更新索引 | `TileEntityStore` + `TileEntityUpdateSchedule` | confirmed |
| TileEntity 网络/存档序列化 | `TileEntity.cs:126-184`；`WorldFile.cs:3389-3445`；`NetMessage.cs:1211-1216,2226`；`MessageBuffer.cs:2551-2571` | Adapter 编解码，当前实现可直接改全局表 | 外部投影/输入适配 | `TileEntityPersistenceAdapter`、`TileEntityReplicationAdapter` | confirmed |
| section loaded/framed/map/refresh 位 | `WorldSections.cs:35-75`；初始化入队 `WorldGen.cs:10529` | `BitsByte[]` 保存四个 bit；当前 `SetSectionLoaded` 为空体，完整读写路径未闭合 | 区段生命周期/复制缓存 | `WorldSectionState` | partial |
| Tile/Chest/Sign/TileEntity 的持久化边界 | `WorldFile.cs:1481-1760,3389-3445` | 读取和保存同时观察并修复存储；加载存在裸 `catch`（`3418-3438`） | I/O 副作用，不得反向成为权威状态 | `WorldStoragePersistenceAdapter` | confirmed（行为风险存在） |
| Tile、实体、Chest、Sign、TileEntity 的网络边界 | `NetMessage.cs:579-619,771-983,1211-1216,1865-1866,1917-2226`；`MessageBuffer.cs:745-1037,1357-1401,1477-1504,1848-1910,2551-2571` | 编码器当前直接读，解码器当前直接写；包来源/授权不在存储层 | 协议适配与复制投影 | `WorldStorageReplicationAdapter` | confirmed（迁移前必须收口） |

### 已排除的相邻状态

- `Entity.position/velocity`（`Entity.cs:10-16`）是实体空间/移动状态；存储层只解析稳定实体引用，不拥有其运动规则。
- `NPC.netID`（`NPC.cs:6391`）是内容定义选择/网络协议字段，不可替换实例槽位、持久化 ID 或账户 ID。
- `Main.Map`、尘埃、雨、战斗文本和屏幕字段虽邻接 `Main.tile`（`Main.cs:926-980`），但分别为地图投影或表现对象，不并入权威存储。
- `Tile` 内部 bit/header 格式保持一个值对象。第一阶段不要把其每个 bit 拆成 ECS 组件，否则会破坏紧凑存档与网络格式，且让相邻 Tile 的高频查询变成多表拼接。

## 3. ID 与引用必须分轴建模

任何接口参数不得裸用 `int` 表示以下任意一个概念。至少应使用不同的值类型或带明确名称的记录，拒绝跨轴隐式转换。

| 轴 | 来源 | 作用域和失效条件 | 推荐值类型 | 禁止替代 |
| --- | --- | --- | --- | --- |
| 槽位 | `whoAmI == Main.*` 数组下标 | 固定容量；释放或复用后指向不同实例 | `PlayerSlot`、`NpcSlot`、`ProjectileSlot`、`WorldItemSlot` | 不作持久主键或泛用实体引用 |
| 权威实体身份 | Version4 当前没有独立通用 ID | 需要跨槽位复用、Command 去重、关系存储时才生成 | `EntityId { Slot, Generation }` | 不等同 `whoAmI` |
| 投射物协议身份 | `owner + Projectile.identity`、`projectileIdentity` | owner 局部；`Kill` 时撤销；不持久化 | `OwnerProjectileIdentity` | 不等同 `ProjectileSlot` 或 TileEntity ID |
| TileEntity 持久 ID | `TileEntity.ID` / `ByID` | 在世界存档内；加载时当前代码重新顺序分配（`WorldFile.cs:3405-3417`） | `TileEntityId` | 不与 Tile 坐标或网络实体 ID混用 |
| TileEntity 类型 ID | `TileEntity.type` 由 manager 注册 | 内容类型/工厂选择 | `TileEntityTypeId` | 不等同实例 ID |
| 空间定位 | `TileEntity.Position`、Chest/Sign `x,y` | 世界坐标；Tile 被移除即可能失效 | `TileCoordinate`、`SectionCoordinate` | 不等同存储对象身份 |
| 容器槽位 | `Main.chest[index]`、`Main.sign[index]` | 旧协议数组位置；可为空/可复用 | `ChestSlot`、`SignSlot` | 不用作对象长期 ID |
| 网络 ID | 包字段、SS14 上层的 `NetEntity` 使用模式 | 连接/复制域；断线或 entity despawn 后无效 | `NetworkEntityId` | 不让网络包直接拿到内部引用 |
| 玩家账户/会话 | 当前 `Main.myPlayer` 与 `RemoteClient` 邻接但语义不同 | 外部身份/连接会话 | `AccountId`、`ClientSessionId` | 不使用 PlayerSlot 代替 |

当前 `TileEntity` 读写呈现一个必须显式处理的兼容问题：`WriteInner` 在非网络存档中写 `ID`（`TileEntity.cs:148-177`），而 `LoadTileEntities` 又用顺序序号覆盖它（`WorldFile.cs:3405-3417`）。实施前要用旧存档夹具确认其版本兼容意图；在未确认前，`TileEntityId` 的“重载后稳定”只能标为 partial，不能宣称为永久 GUID。

## 4. 推荐边界、最小接口与 seam

建议 `WorldStorageRoot` 仅作为组合根，不公开内部数组、字典或可变 `Tile`：

```text
WorldStorageRoot
  TileMapStore -------------------- TileMapReadView
  LiquidWorkQueueState ----------- LiquidWorkQueuePort
  PlayerSlotStore / NpcSlotStore / ProjectileSlotStore / WorldItemSlotStore
  ProjectileIdentityIndex
  WorldContainerStore ------------ ContainerReadView
  WorldSignStore ----------------- SignReadView
  TileEntityStore ---------------- TileEntityReadView
  TileEntityUpdateSchedule
  WorldSectionState -------------- SectionReadView
```

| 模块 | Interface / 输入输出 | Implementation 与状态所有权 | Seam、Depth、Leverage、Locality |
| --- | --- | --- | --- |
| `TileMapStore` | `TryRead(TileCoordinate, out TileSnapshot)`；`Apply(TileMutation)`；`ReadSection(SectionCoordinate)` | 唯一拥有 Tile 网格、边界和 Tile 变异版本；`TileMutation` 一次更新 Tile 值、必要的 section dirty 位和结构索引失效 | 用 recording command sink 验证 mutation；深度在于隐藏二维数组和压缩格式；高杠杆，因为物理/世界交互/保存统一收口；空间局部性保留在 section 读取 |
| `LiquidWorkQueueState` | `Schedule(TileCoordinate)`、`TryTakeBatch(limit, out batch)` | 只拥有活跃液体和缓冲坐标及去重标记；不复制 `Tile.liquid` | 用确定性 batch 顺序和固定 TileMap fake 测试；与 `LiquidSimulationSystem` 分离，避免液体算法持有世界总状态 |
| `EntitySlotStore<T>` | `TryAllocate(out EntityHandle<T>)`、`TryResolve(EntityHandle<T>, out T)`、`Release(handle)`、`EnumerateActive()` | 保留固定容量/协议槽位；每槽增加 generation；玩家、NPC、投射物、世界物品各一实例 | 所有 spawn/despawn 经 Command；窄 `EntityHandle` 防止释放后的旧槽位误解析；高杠杆地消除 `active + int` 组合错误 |
| `ProjectileIdentityIndex` | `Bind(OwnerProjectileIdentity, ProjectileHandle)`、`TryResolve(...)`、`Unbind(handle)` | 单独维护 owner-local identity 到有 generation 的投射物句柄 | 对应 Version4 create/kill/packet 三路径；只由 Projectile lifecycle 写，禁止通用 EntityStore 代写 |
| `WorldContainerStore` | `TryOpen(ChestSlot)`、`TryFindAt(TileCoordinate)`、`Apply(ContainerTransaction)`、`Release(ChestSlot)` | Chest 槽位、坐标唯一性、物品集合和占用；银行/商店作为不同容器域或显式关联，不把 Player inventory 塞入 | 交易测试断言失败不扣物品、坐标/槽位索引同事务更新；存档和网络只通过快照 Adapter |
| `WorldSignStore` | `TryReadAt(TileCoordinate)`、`Upsert(SignMutation)`、`RemoveAt(TileCoordinate)` | Sign 槽位、坐标、文本；检查对应 Tile 是否仍是标牌由 `SignEligibilityQuery` 决定 | 用 TileMap fake 测试 Tile 移除后的清理；不把文本网络包解析直接放进 Store |
| `TileEntityStore` | `TryResolve(TileEntityId)`、`TryResolveAt(TileCoordinate)`、`Register(TileEntityRegistration)`、`Remove(TileEntityId)` | 原子维护 ID、位置、类型和实例；拒绝 ID/坐标冲突 | 注册命令先验证 Tile，再更新两个索引和更新计划；独立 fake 能验证冲突、重复包、删除后不可解析 |
| `TileEntityUpdateSchedule` | `Add/Remove(TileEntityId)`、`SnapshotForTick()` | 只保存需更新的实体集合与稳定 tick 快照；不拥有实例 | 先快照再更新，可避免迭代中删除集合；每种 TileEntity 规则在其领域 System 中执行 |
| `WorldSectionState` | `Read(SectionCoordinate)`、`MarkLoaded`、`MarkFramed`、`MarkDirtyForReplication` | 独立拥有 section 生命周期 bit 和版本；不能是 TileMap 的私有隐含缓存 | 与加载、网络和地图投影分别连接；`SetSectionLoaded` 空体说明现有语义未闭合，需先补 verifier |

推荐目录以能力优先而不是全局 `Components/` 分类：`src/WorldStorage/` 先平铺上述类型；只有 Tile、EntitySlots、Containers、TileEntities 或 Sections 达到独立迁移/测试规模时才建立同名责任子目录。每个公开核心类型一文件，且不依赖目录枚举表达系统顺序。

## 5. 调用方向与提交顺序

```text
Network/Persistence bytes
  -> Replication/Persistence Adapter (decode to typed request; no direct state write)
  -> Authorization + Tile/Section/Entity eligibility Query
  -> WorldStorage command buffer
  -> commit: TileMap -> Container/Sign/TileEntity indexes -> Slot/identity indexes -> Section dirty versions
  -> immutable snapshots
  -> Replication/Persistence/Presentation adapters

LiquidSimulationSystem: TileMapReadView + LiquidWorkQueueState -> TileMutation commands
TileEntityRuntimeSystem: TileEntityReadView + TileMapReadView -> typed gameplay/Tile commands
```

以下顺序是数据不变量，不是文件顺序：

1. 先解析外部字节为带来源和协议版本的 DTO；解析失败返回 typed error，绝不猜测槽位。
2. 在同一 tick 的提交前，用只读 Query 校验世界就绪、坐标边界、实体 handle generation、Tile 类型和权限。
3. 同一个结构变更事务先改变 Tile，再原子地创建/删除对应 Chest、Sign 或 TileEntity 索引，最后标记区段脏版本。这样不会出现 Tile 已消失而 TileEntity 仍可被 ID 查到。
4. 实体释放先撤销 `ProjectileIdentityIndex` 等反向索引，再增加 slot generation 并清空槽位。投射物的 owner/identity 映射不可由持久化 Adapter 恢复。
5. 成功提交后再取快照并发送网络包或写世界文件。发送失败、保存失败和客户端可见性变化不得回写世界真值。

## 6. SS14 参考：可采用的模式，而非代码移植

| 已读 SS14 文件 | 可借鉴的模式 | 对 Version4 的约束 |
| --- | --- | --- |
| `Content.Shared/Tools/Systems/SharedToolSystem.Tile.cs:24-103` | 工具交互先将 `NetEntity` 解析为本地实体，确认 grid/component、TileRef、阻挡与距离，再由 Tile system 产生实际变更；客户端不预测会导致错误实体生成的 TileEntity 创建 | 网络 ID 先经 Adapter 解析为强类型 handle；`TileMutationCommand` 通过资格 Query；不要让包处理器写 `Main.tile` 或 TileEntity 表 |
| `Content.Server/Containers/ThrowInsertContainerSystem.cs:13-55` | 调用方通过 `SharedContainerSystem.GetContainer/CanInsert/Insert` 修改容器，效果和审计在 System 边界执行 | 将 Chest 的数组、坐标索引与内容操作封装为事务接口；玩法规则不得直接操作内部 `Item[]` |
| `Content.Server/Maps/GameMapManager.cs:16-244` 与 `IGameMapManager.cs:8-70` | 地图选择/持久化路径配置是窄接口，不同于 Tile 存储本身 | 世界选择、世界文件路径和可玩地图目录放到 Session/Persistence 边界，不塞进 `TileMapStore` |

SS14 的底层类型源码无法读取，因为 `RobustToolbox` 是未初始化子模块（根 `.gitmodules` 声明了该子模块）。因此不能以本轮证据断言其 Entity UID、网络映射、容器或地图实现的确切生命周期；上述三项仅支持“窄服务、验证后变更、协议 ID 与本地引用分离”这一组织结论。

## 7. 不拆分项、兼容方案与风险

### 不拆分项

- 保留 `Tile` 紧凑值格式及固定数组的内部表现，第一阶段只加受控读取和 Command 写入接口。
- 不建立通用 `EntityStore` 把 Player/NPC/Projectile/WorldItem 生命周期合并。它们的创建、回收、网络和持久化语义不同；只复用 `EntitySlotStore<T>` 的机制。
- 不让 `WorldSectionState` 持有 Tile 内容，也不把 TileEntity 的 ID/位置/更新队列拆给三个互不协调的组件。
- 不将 `Chest`/`Sign`/TileEntity 的存档 DTO、网络包或 UI 开关视作权威状态。

### 单向兼容迁移

1. 先建立 `WorldStorageRoot` 的契约和只读视图；在尚未迁移的成员上，Legacy facade 继续直接读取旧数组，旧数组仍是唯一权威，不能建立会过期的第二份镜像。
2. 以 TileEntity 的注册/删除和 Projectile identity 为第一小批次。完成一个成员簇的迁移后，新 Store 成为唯一写者，Legacy facade 只单向转发到新 Store；对每批次移除一个旧写者，禁止新旧双写。
3. 接入 Chest/Sign 原子坐标索引，再迁移 Tile mutations 和 section 脏标记，最后把 WorldFile/NetMessage/MessageBuffer 限制为 Adapter。
4. 仅在每项 focused verifier 通过后删除 Legacy facade 的写路径。旧协议槽位可保留在 Adapter 层，但不可泄漏为领域 API。

### 已知风险

- `WorldSections.SetSectionLoaded` 为空体（`WorldSections.cs:68`），目前不足以确认 loaded 位和 `mapSectionsLeft` 的真实不变量；这是 `WorldSectionState` 的 P0 证据缺口。
- `TileEntity` 的存档 ID 重编行为尚未用真实旧世界夹具验证；不能承诺稳定 ID。
- `WorldFile.LoadTileEntities` 的空 `catch`（`WorldFile.cs:3430-3438`）会隐藏清理失败；迁移 Adapter 应返回明确诊断和部分恢复结果。
- `MessageBuffer` 和 `NetMessage` 直接读写静态存储的范围很大。先把它们改造成 DTO Adapter 时，必须针对每个包保留权限、容量、顺序和重复投递语义；不能以“能解析”代替服务器授权。
- `EntityCreationLock` 仅保护 TileEntity 的部分集合更新；WorldGen 同时重置全局状态。世界加载/生成与常规 tick 的单写者屏障尚未被代码证明，需在调度层先固定。

## 8. Focused verifier 计划与本轮结果

| 验证器 | 断言 | 本轮结果 |
| --- | --- | --- |
| `TileMapStoreMutationVerifier` | 边界拒绝；一次 Tile mutation 同步 section 脏版本；快照不能反写 Tile | 未实现 |
| `EntitySlotGenerationVerifier` | release 后旧 handle 不能解析到复用槽位；容量满返回显式结果 | 未实现 |
| `ProjectileIdentityVerifier` | spawn bind、packet replace、Kill unbind；owner/identity 绝不解析到别的 active 投射物 | 未实现 |
| `WorldContainerIndexVerifier` | 创建/删除同步 slot 与坐标索引；非法关联 Tile 不保存；失败事务不变更物品 | 未实现 |
| `TileEntityAtomicityVerifier` | ID/位置双索引冲突拒绝；删除清空 schedule；加载后无效 TileEntity 被清理且可诊断 | 未实现 |
| `SectionLifecycleVerifier` | loaded/framed/map/refresh bit 转换、初始化与 unload/replication 顺序 | 未实现，且现有 `SetSectionLoaded` 为空体 |
| `WorldStorageAdapterVerifier` | 存档/网络 decode 不直接写 Store；无效网络 ID、坐标和重复包返回 typed error | 未实现 |

本轮没有改动 Version4、NLTX 生产 C# 或测试，未执行编译。以上是基于源码与只读参考工程的组件拆分设计，不是迁移完成、构建成功或运行时行为已回归的声明。
