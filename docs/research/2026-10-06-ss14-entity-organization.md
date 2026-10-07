# SS14 Entity 组织源码研究

## 1. 结论与交付边界

SS14/RobustToolbox 将运行时 Entity 表达为 **局部实体标识与附着组件的关联**。
实体能力由组件组合表达，系统按标识解析组件，或枚举所需组件组合；
`Entity<T...>` 是携带已解析组件的类型化访问对象，不是固定的实体类别聚合类。
这些结论来自身份定义、运行时组件表、原型加载和内容系统的具体实现。[RT-UID] [RT-VIEW] [RT-STORE]

本仓库采用这些组织原则时，保留已接受的 `EntityUuid` 身份根和 typed projection，
不照搬 SS14 的整数 UID、YAML、地图层级或全部生命周期事件。
正式规则见 [ECS Entity 组织设计约束](../../Context/架构设计/ECSEntity组织设计约束.md)；
文件目录继续使用 [ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md)。

本文是源码研究及目标适用性分析；不表示 NLTX 已完成迁移，也不表示运行了 SS14 或本项目。
`confirmed` 指所列源码中直接确认的结构/分支，不能代替运行时行为、性能或版本配套验证。

## 2. 来源、版本与缺口

研究日期：2026-10-06。证据分为两个来源：

| 来源 | 定位与版本 | 证据范围 |
| --- | --- | --- |
| SS14 内容层 | 本地 `C:/Users/shan/Downloads/ECS/space-station-14-master` | 下载快照，无可靠 Git 提交；以具体文件及 SHA256 固定 |
| RobustToolbox 引擎 | 官方仓库提交 `b2c22005744e8da8e74f69a8c8165f3839452ea6` | 核心源码单文件按该提交下载并阅读 |
| NLTX 目标 | 当前工作树，存在其他任务的未提交变更 | 仅对本文列出的当前文件作静态观察 |

本地内容快照的 `RobustToolbox/` 为空，因此不能仅靠内容目录证明引擎存储与生命周期。
引擎版本由官方 codeload 响应的 tar PAX 全局头
`comment=b2c22005744e8da8e74f69a8c8165f3839452ea6` 取得，随后通过官方
`raw.githubusercontent.com` 的固定提交 URL 下载完整单文件。
已读取文件保存在本仓库私有过程目录
`.agent-workplace/ss14-entity-reference-primary/`，正式证据入口采用下文固定提交链接。

**内容快照与该引擎提交的配套关系未确认。** 本文分别描述两层观察，不声称它们构成可编译、
可运行的同版本 checkout。过程目录里的 `RobustToolbox-header.tar.gz` 和
`RobustToolbox-master.zip` 均为未完成下载，不能作为完整源码档案或构建输入。

未覆盖引擎全部网络状态应用、预测回滚、地图反序列化及序列化复制内部实现。
这些部分只保留已读入口支持的结论；不据此证明所有关系自动修复、所有字段深拷贝安全或线程安全。

## 3. 身份与类型化实体访问

### 3.1 EntityUid 的范围

`EntityUid` 是 `readonly struct`，持有整数 `Id`。源码直接定义 Entity 是该标识及其附着
组件集合，并明确 UID 不保证跨游戏实例或跨 `IEntityManager` 唯一。向其他 manager 传递
局部 UID 可能命中无关实体；跨时间全局唯一身份需要另行设计。[RT-UID]

`EntityUid.IsValid()` 只判断 `Id > 0`，不检查实体是否存活。运行时另有
`EntityExists`、`Deleted`、`IsPaused` 和 `IsQueuedForDeletion` 等判断；
“值合法”“有登记”“暂停”“待删除”“已删除”不是同一状态。[RT-UID-VALID] [RT-EXISTS] [RT-QUEUE]

该提交的 `GenerateEntityUid` 与 `GenerateNetEntity` 使用各自递增计数器。
这里没有得出“所有 ECS 应使用整数”“所有框架都无需 generation”或“计数器跨重置永不复用”
的结论。NLTX 的可复用槽位需按自身协议处理旧引用失效。[RT-GENERATE]

### 3.2 NetEntity 不是 EntityUid 的另一个名称

`NetEntity` 的定义说明：服务器与客户端可以为同一实体分配不同局部 UID，
网络标识用来建立两端关联。它也不是账户身份或跨加载业务身份。[RT-NET]

本项目应借鉴标识作用域分离，继续遵守
[身份根决策](../architecture/decisions/0001-entity-identity-root-and-typed-projections.md)
及 [身份术语](../architecture/entity-identity-context.md)。不能把局部句柄、兼容槽位、
协议 identity、账户 UUID 和 `EntityUuid` 合并成可互换字段。

### 3.3 Entity<T...> 是访问视图

`Entity<T>` 包含 `Owner` 和 `Comp`；多参数形式携带多个已解析组件。
源码说明它用于让 API 表达所需组件，支持到八个类型参数，并标记
`[NotYamlSerializable]`。构造时调用 `DebugTools.AssertOwner`。[RT-VIEW]

它没有为实体创建另一份权威组件集合，也没有固定“玩家必须拥有的全部组件”。
构造断言不能证明组件在跨帧、事件回调或移除之后仍然有效，也不能据此声称 Release
自动持续验证引用。类型化视图解决接口表达和重复查找问题，生命周期协议仍需由调用方遵守。

## 4. 组件关联、注册与查询

### 4.1 类型侧与实体侧索引

`EntityManager.Components.cs` 同时维护：

- `_entTraitDict`：组件类型到 `Dictionary<EntityUid, IComponent>`。
- `_entTraitArray`：用组件注册索引访问相应组件表。
- `_entCompIndex`：实体到其组件集合的反向索引。

这些索引让“某实体的某组件”和“某组件涉及的实体”访问同一份对象；
不是为每个 `Mob`、玩家或物品创建固定字段容器。此提交可确认的是字典及反向索引，
不能将其描述成 archetype/chunk 存储或据此声称更快。[RT-STORE]

`AddComponentInternal` 检查注册身份是否已占用，处理显式覆盖，并更新组件表、实体索引、
适用的网络组件表和事件登记；然后执行该阶段需要的 add/init/start/map-init。
只给一个实体对象赋字段，不能替代这条关联和生命周期路径。[RT-ADD]

### 4.2 注册身份与 C# 继承不同

`ComponentFactory` 管理组件类型、注册名字和相关网络信息，检查注册冲突。
注册名字有框架转换规则，业务代码不应自行猜测类型名到键的映射。[RT-FACTORY]

`GetEntityQuery<T>` 使用 `T` 的组件索引取得对应表；另有内部
`GetEntityQuery<TShared,TConcrete>` 特殊入口显式取得具体类型表。
因此，不能从“组件继承了基类”推断普通基类查询会自动包含全部子类。[RT-QUERY-TABLE]

### 4.3 点查询、普通枚举与 All 枚举不同

| 实际入口 | 该提交的过滤与返回 |
| --- | --- |
| `EntityQuery<T>.TryGetComponent` / `GetComponent` | 查类型表并排除 `comp.Deleted`；返回组件对象；该分支未过滤暂停 |
| `EntityQueryEnumerator<T...>` | 排除已删除组件；检查 metadata 并跳过暂停实体；多组件取交集 |
| `AllEntityQueryEnumerator<T...>` | 排除已删除组件；多组件取交集；该枚举实现没有暂停过滤 |

点查询可直接持有组件表；它不是每次捕获的不可变快照。
`[Pure]` 或 query 名称不能让返回的可变组件变成只读数据。[RT-QUERY-POINT] [RT-QUERY-NORMAL] [RT-QUERY-ALL]

上述分支没有额外证明实体已完成初始化、组件正在 Running、实体未排队删除或未 Terminating。
特别是 `Component.Deleted` 从 `Removing` 开始为真，
延迟移除先到 `Stopped` 再等待 Cull，因此停止但尚未移除的组件仍可能被这些查询取得。
调用方必须检查本次行为所需阶段，不能仅以查询成功判断可参与正常模拟。[RT-COMP-STAGE] [RT-REMOVE]

`EntityQuery<T>.Resolve` 对已传入的非空组件先做 owner 断言并直接返回；
它不会在该分支重新查当前类型表。持有旧组件后调用 Resolve，也不是自动刷新引用。[RT-RESOLVE]

枚举器持有底层字典的 enumerator。结构变化能否同步执行、引用何时失效，
应按具体 API 和存储契约判断；本文未验证所有遍历中的添加/删除组合，也不声称框架统一安全。

内容层例子：

- `MobStateSystem.cs:22` 持有 `EntityQuery<MobStateComponent>`，按能力解析状态。
- `SharedJetpackSystem.cs:59` 枚举 `JetpackUserComponent, TransformComponent` 的组合。
- `PullingSystem.cs:171–176` 根据当前牵引关系 Ensure/Rem `ActivePullerComponent`。

来源均为第 10 节固定指纹的本地内容文件；这些例子说明能力组合及动态关联的使用方式，
不证明所有实体都使用同一组合，也不证明每个组件只被一个系统访问。

## 5. 原型与实体创建

### 5.1 原型描述组合

`EntityPrototype` 包含 `Parents`、`Abstract` 和 `ComponentRegistry`；
其默认组件定义含 MetaData 与 Transform。原型复用的是组件定义，
不等于建立运行时 `PlayerEntity -> MobEntity` 继承树。[RT-PROTOTYPE]

本地 `Resources/Prototypes/Entities/Mobs/base.yml` 中：

- `BaseControllable`（2–42 行）组合控制、物理、MindContainer 等能力。
- `BaseMob`（44–56 行）继承 BaseControllable 定义并增加 InputMover 等能力。
- `MobDamageable`（66–115 行）组合伤害、MobState、MobThresholds 等能力。

这些抽象定义让“可控制”“可移动”“可受伤”成为可组合的创建条件。
不需要把它们全部固化成某个长期实体对象的必有字段。

### 5.2 加载实例数据与创建阶段

`EntityPrototype.LoadEntity` 结合原型及加载上下文取得组件数据；
`EnsureCompExistsAndDeserialize` 复用已有必要组件或通过工厂创建新组件，
调用 `serManager.CopyTo`，然后对新组件执行挂载。[RT-PROTOTYPE-LOAD]

能确认的是独立组件获取/构造和序列化复制入口；本文没有展开全部 serializer、
`CopyByRef` 和自定义复制器，不能声称已证明每一种可变嵌套对象均无共享风险。
NLTX 的规范要求实例可变数据独立，应在实际采用的复制实现上验证。

`AllocEntity` 分配局部 UID 和网络标识，登记实体，并挂载必要 MetaData、Transform。
`CreateEntity` 加载原型组件；初始化/启动是后续入口。
地图已准备完成时，再执行 MapInit。原型加载或初始化/启动失败会调用删除清理，
而不是把分配过 UID 的半成品直接当成正常实体。[RT-CREATE] [RT-START]

本项目需要明确对应的创建可见边界及失败清理；不要求复制 SS14 的全部阶段名称，
也不因为借鉴它而新增数据库事务或固定两阶段提交类型。

## 6. 实体、组件与玩法生命周期

### 6.1 三层生命周期

`EntityLifeStage` 定义 PreInit、Initializing、Initialized、MapInitialized、
Terminating、Deleted；启动由独立入口执行，枚举本身没有额外的 Entity Running 项。
组件有自己的 PreAdd、Added、Initialized、Running、Stopped、Removing 等阶段。[RT-ENTITY-STAGE] [RT-COMP-STAGE]

组件阶段说明还明确：ComponentInit 时其他组件虽已存在，但不保证已正确初始化；
Startup/Shutdown 在实体生命期内可多次发生。因此，组件存在不等于依赖已准备好，
一次 Shutdown 也不能简单等同于实例最终删除。[RT-COMP-LIFECYCLE]

本地 `MobStateSystem.StateMachine.cs:103–133` 修改 MobState、发事件、Dirty，
并按新状态触发对应行为。这表达 Alive/Critical/Dead 的玩法状态。
该路径不能被解释为“死了必然从 EntityManager 删除”。

### 6.2 动态挂载、同步移除与延迟移除

挂载到已初始化或已 MapInitialized 的实体时，运行时会补齐适用的组件生命周期。
动态能力变化走运行时入口，不能只改引用字段。[RT-ADD]

`RemoveComponentImmediate` 执行适用 Shutdown、Remove 和表清理；
`RemoveComponentDeferred` 先将组件登记到 `_deleteSet` 并停止，
之后由 `CullRemovedComponents` 真正移除。MetaData/Transform 受保护，
普通组件移除路径不会随意删除它们。[RT-REMOVE]

因此，下列时点必须区分：请求移除、停止行为、查询不可见、对象关联清理。
“所有变更必须排队”与“删除字段立即等于注销组件”都不能从该实现推出。

### 6.3 实体删除及实际提交时点

`QueueDeleteEntity` 去重、入队并发出排队通知；它没有在该入口完成实体删除。
所读 Tick 路径顺序为：系统更新 → 事件队列处理 → 排队实体删除 → 组件 Cull。[RT-QUEUE] [RT-TICK]

引擎删除实现先沿 Transform 子树标记 Terminating，再递归删除，
Shutdown/Dispose 组件，登记 Deleted，清理实体集合和网络映射。
这是该空间关系的删除规则，不能据此推断目标关系、控制关系或所有权关系都级联删除。[RT-DELETE]

删除 API 的说明明确区分客户端预测：网络实体可能先脱离到 nullspace，
随后由状态处理恢复或最终删除。本文只确认该声明及已读基础实现，
没有展开整个客户端预测状态链，不把所有 Delete 调用概括为即时最终销毁。[RT-DELETE]

## 7. 跨实体关系及玩家控制

`TransformComponent` 存储局部坐标、空间父 UID、子实体集合和 Map/Grid 信息。
其矩阵属性存在惰性缓存更新，因此“组件只有字段、读取一律无副作用”也不符合全部源码。
空间父子同时影响坐标和删除，不能把它当成通用玩法 Parent。[RT-TRANSFORM]

内容层显示关系按领域组织：

- `MindComponent.cs:9–24` 明确 Mind 是独立实体上的组件。
- `UserId` 与 `OwnedEntity`、`VisitingEntity` 分离，CurrentEntity 从当前控制关系取得。
- `MindContainerComponent.cs:5–16` 让身体通过实体引用关联 Mind；名称中的 Container
  并不意味着把整个 Mind 对象嵌套存进身体。
- `PullingSystem.cs:553–555` 建立双方引用和活跃组件；347–393 行的 StopPulling
  协调关节、双方引用、标记、Dirty、提示及事件。

这些证据支持“账户/会话、控制者、身体、空间父子和牵引分别建模”。
它们不自动决定 NLTX 的玩家必须拆成 Mind/Body；独立实体应有实际生命周期和引用需求。

`MindComponent.OriginalOwnedEntity` 的源码还留下引用删除后的 TODO，
并用 NetEntity 提醒调用方检查存在性。参考项目同样可能保留历史引用和待完善路径；
不能将它描述为所有关系始终自动无悬空。

## 8. 系统行为与网络通知

本地 `SharedStackSystem.API.cs:239–265` 的 SetCount 同时进行组件解析、数值限制、
状态修改、UI 标记、Dirty、外观更新和事件通知；数量归零时调用 PredictedQueueDel。
它说明系统 API 可以协调一项能力的完整效果，Shared 层也可以包含修改与预测逻辑。

仅复制 Count 赋值会漏掉该路径的通知和删除语义。相应地，NLTX 迁移 Entity 组织时，
也应保留实际入口的状态、顺序、通知与错误协议，不能只证明组件“放进了表”。

组件的网络注册是有条件的：AddComponentInternal 依据注册 NetID、NetSyncEnabled 等
处理 metadata 的网络组件表。不能推断每个组件和字段自动复制，也不能推断复制字段
均允许客户端提交权威写入。[RT-ADD] [RT-FACTORY]

本地 `PrototypeSaveTest.cs:149` 的初始化事件可移除组件，是动态生命周期的额外源码例子。
本次未运行该测试，不将测试文件存在当成实际通过。

## 9. 对当前项目的适用性

### 9.1 2026-10-06 研究时的静态事实与问题范围

以下源码描述固定在本研究的 2026-10-06 阅读时点，不代表 B5-B9 迁移后的当前状态。当前实现、
caller 和验收范围见 [Entity 组织执行 ledger](../migration/ledgers/2026-10-06-entity-organization-execution-ledger.md)。

- 研究时的 `ProjectileEntityState.cs` 长期持有定义、生命周期、网络、碰撞、轨迹和可选能力等固定字段。
  当时的 `ProjectileLifecycleSystem.TryCreate` 构造它并使用分类槽位存储；B5 后续已删除该旧类型和对应聚合路径。
- [RuntimeNpcEntity](../../src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs) 与
  [RuntimePlayerEntity](../../src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs) 位于
  NonAuthoritative.SimulationHost；既持有能力状态，也承担装配或推进逻辑。
  这是研究时对宿主角色的判断；它们的角色不能仅靠 Entity 后缀判断，更不能将这个模拟宿主直接称为服务器权威 ECS。
- RuntimeNpcEntity.Hydrate 的 InstanceId 只组合 worldId 与 slot+1，未纳入 slotGeneration。
  因而相同世界、相同槽位重新 Hydrate 时，计算值相同；这是已确认的局部身份冲突风险。
  该值还进入 RuntimePlayerEntity 的伤害来源 Guid；本文没有证明由此已触发用户可见故障。
  该风险不代表当前状态：B1 后的实现通过进程递增值创建 `NpcInstanceId`，当前调用者与生命周期证据见 ledger。
- [EntitySlotStore](../../src/NSSLC/Component/WorldStorage/EntitySlotStore.cs) 已有 generation
  递增、匹配检查及耗尽处理；即使当时也不能把“全项目完全没有槽位失效保护”作为结论。

这些观察表明，仅把字段拆成 Component 类型，仍不足以建立按实体/能力解析的关联协议。
固定宿主容器可以是过渡实现；应明确哪些是创建组合、临时视图或边界数据，
哪些仍是长期权威状态容器，再逐路径迁移。本文不是整个代码库的最终缺陷审计。

### 9.2 借鉴与保留

| SS14 观察 | 本仓库组织要求 |
| --- | --- |
| EntityUid + 组件关联 | 沿用 EntityUuid 根身份，句柄及边界投影分别映射 |
| 类型表与实体反向索引 | 提供等价的点查询/组合查询一致性；不指定字典或 archetype |
| 原型组合与动态组件 | 表达实际能力存在性；支持现有创建方法或定义格式 |
| Entity<T...> | 可用临时类型化视图；明确归属与失效条件 |
| 独立生命周期及延迟操作 | 分开玩法死亡、组件移除和实例删除，说明可见时点 |
| Mind/Body、牵引、空间层级 | 按关系语义和实际需求组织，声明退出路径 |

本仓库已接受 `LocationComponent` 为公共位置名称；SS14 的 Transform 含层级坐标和地图语义，
不直接覆盖该决策。`WorldStorageRoot` 的名称也不证明文件 I/O 与 ECS 职责混杂，
应检查实际依赖及权威状态是否重复。

## 10. 内容快照指纹及复查入口

下列路径相对于 `C:/Users/shan/Downloads/ECS/space-station-14-master/`。
内容层引用仅代表此快照，不伪装成与固定引擎提交配套的上游链接。

| 文件 | SHA256 |
| --- | --- |
| `Resources/Prototypes/Entities/Mobs/base.yml` | `66BC8424103D331EB75A1E26E229128FB3CC4116241C114BC5128D365C4C3318` |
| `Content.Shared/Mobs/Systems/MobStateSystem.cs` | `A8FFC1A2BE35BB5E3FAC5E12D1949E11BC7D6461B22B1E54A10B2D441E4BD57C` |
| `Content.Shared/Mobs/Systems/MobStateSystem.StateMachine.cs` | `DA8FCD8A4BE373D728ED4CA2E835BE04603D89407C18E0B01ACFF74C7F079E6E` |
| `Content.Shared/Movement/Systems/SharedJetpackSystem.cs` | `3FB6A048244B35662E72A5D5C88C5C243A487828D98C07E5ACF9F740F089EEFC` |
| `Content.Shared/Movement/Pulling/Systems/PullingSystem.cs` | `ACA1F9F7B53141329FBCD243A0EC16A8C652FEBAE930ECD68CF05BB9E0152487` |
| `Content.Shared/Mind/MindComponent.cs` | `F0BE1F2AB81A6C075BCBA62AD5CCFA61607CAF00FDA39980018ED90B662338BA` |
| `Content.Shared/Mind/Components/MindContainerComponent.cs` | `B2CEBCC80CB6444B49A840801B5BCA5C79C317D732B8F5E266EE45E3129E4479` |
| `Content.Shared/Stacks/SharedStackSystem.API.cs` | `52DE7F7DCA3EA9EB7DCD3CD0FD0E854F6BB3AB3E5914890BDBD9629BD3197ECE` |
| `Content.IntegrationTests/Tests/PrototypeSaveTest.cs` | `0AF52F661F549A677477A339B7427FAD606DEC80FCC16315991D0C79302A02E2` |

本次交付为约束、研究及导航；验证范围为文档链接、Markdown 结构、manifest 及 scoped diff。
未编译、未运行 SS14/NLTX，也未修改生产源码或新增测试。

[RT-UID]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityUid.cs#L11-L30
[RT-UID-VALID]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityUid.cs#L78-L85
[RT-NET]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/NetEntity.cs#L10-L19
[RT-VIEW]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/Entity.cs#L8-L32
[RT-STORE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L39-L48
[RT-ADD]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L349-L434
[RT-FACTORY]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/ComponentFactory.cs#L87-L179
[RT-QUERY-TABLE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L1201-L1225
[RT-QUERY-POINT]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L1848-L1909
[RT-RESOLVE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L1969-L1987
[RT-QUERY-NORMAL]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L2381-L2414
[RT-QUERY-ALL]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L2797-L2825
[RT-PROTOTYPE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/Prototypes/EntityPrototype.cs#L145-L165
[RT-PROTOTYPE-LOAD]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/Prototypes/EntityPrototype.cs#L250-L334
[RT-CREATE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L931-L1029
[RT-START]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L1033-L1074
[RT-ENTITY-STAGE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityLifeStage.cs#L6-L35
[RT-COMP-STAGE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/Component.cs#L58-L80
[RT-COMP-LIFECYCLE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/Component.cs#L141-L226
[RT-REMOVE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.Components.cs#L583-L729
[RT-TICK]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L267-L290
[RT-DELETE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L576-L742
[RT-QUEUE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L745-L757
[RT-EXISTS]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L799-L826
[RT-GENERATE]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/EntityManager.cs#L1135-L1143
[RT-TRANSFORM]: https://github.com/space-wizards/RobustToolbox/blob/b2c22005744e8da8e74f69a8c8165f3839452ea6/Robust.Shared/GameObjects/Components/Transform/TransformComponent.cs#L19-L110
