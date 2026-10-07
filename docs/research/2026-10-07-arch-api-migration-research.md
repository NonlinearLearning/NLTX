# Arch 2.1.0 官方 API 与迁移事实

文档 ID：DOC-2026-10-07-ARCH-API-MIGRATION-RESEARCH  
逻辑域：research  
产物类型：evidence  
状态：active；外部文档与源码研究，不是项目运行验收  
日期：2026-10-07  
范围：Arch 稳定包、原生实体/组件/查询、结构变化、世界释放及可选扩展  
证据入口：官方 GitBook、官方固定提交源码、NuGet 官方包与 `.nuspec`  
canonical 路径：`docs/research/2026-10-07-arch-api-migration-research.md`  
关联计划：[自定义 ECS 转向 Arch 执行计划](../plans/2026-10-07-custom-ecs-to-arch-execution-plan.md)

## 1. 研究边界与版本固定

用户允许采用 Arch 破坏当前自定义 ECS API 兼容性。本研究为直接采用 Arch 原生 API 提供事实，不要求模拟旧运行时、句柄、编辑委托或通用版本快照。

本次读取官方在线 API 文档与稳定源码，并下载官方 `.nupkg` 只读检查元数据和 XML API 文档。没有在项目中添加 PackageReference，没有 restore、编译或运行 API 探针。网络/源码读取结果不能代替 .NET 10 上的项目验证。

| 项目 | 本次取得的事实 |
| --- | --- |
| NuGet 稳定版本 | `Arch 2.1.0`；查询时官方版本索引中的最新稳定版本 |
| Git tag | `v2.1.0` |
| 固定提交 | `04d52e7268eb6f376ca4841a0c204334120c5e9d` |
| 包与源码对应 | `.nuspec` 的 repository commit 与 `git ls-remote --tags` 结果一致 |
| 包内目标资产 | `lib/net8.0/Arch.dll`、`lib/net6.0/Arch.dll`、`lib/netstandard2.1/Arch.dll` |
| 核心命名空间 | `Arch.Core`；命令缓冲命名空间为 `Arch.Buffer` |
| 核心许可 | Apache-2.0；完整依赖许可需在实施时记录 |

NuGet `.nuspec` 的三个目标框架组都声明以下依赖：

| 依赖 | 声明版本 |
| --- | --- |
| Arch.LowLevel | 1.1.5 |
| Collections.Pooled | 2.0.0-preview.27 |
| CommunityToolkit.HighPerformance | 8.2.2 |
| Microsoft.Extensions.ObjectPool | 7.0.0 |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 |
| ZeroAllocJobScheduler | 1.1.2 |

这些是包声明，不能冒充本仓库 restore 后的最终解析图。`net10.0` 消费 net8.0 资产的兼容性仍应由最小探针和实际项目构建确认；本次不修改 `global.json` 或目标框架。

来源：[NuGet 版本索引](https://api.nuget.org/v3-flatcontainer/arch/index.json)、[Arch 2.1.0 页面](https://www.nuget.org/packages/Arch/2.1.0)、[官方包](https://api.nuget.org/v3-flatcontainer/arch/2.1.0/arch.2.1.0.nupkg)、[固定源码](https://github.com/genaray/Arch/tree/04d52e7268eb6f376ca4841a0c204334120c5e9d)、[Arch.csproj](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Arch.csproj)。

## 2. 文档漂移：先纠正再设计

官方 README 和 GitBook 是阅读概念的入口，部分示例与 2.1.0 稳定源码不一致。

| 在线材料 | 固定版本事实 | 迁移影响 |
| --- | --- | --- |
| README 示例 `using Arch`，安装 `2.1.0-beta` | 稳定核心 namespace `Arch.Core`，版本 2.1.0 | 不直接复制安装命令与 using |
| 独立 `EntityReference` 页面 | 2.0.0 release 已宣布 Entity 自带 Version、移除 EntityReference；2.1.0 Entity 有 Id/WorldId/Version | 原生运行时句柄直接用完整 Entity，不新增已移除类型 |
| Query 示例 `.None<T>()` | 稳定 builder `.WithNone<T>()` | 查询示例须编译核对 |
| CommandBuffer 示例 `PlayBack`、`disposal` | `Playback(World world, bool dispose=true)` | 大小写和参数名按稳定源码 |
| Buffer `Create(new Signature(...))` 示例 | 稳定 `Create(ComponentType[] types)` | 临时实体与创建重载按稳定实现 |

本项目现有 `Terraria.Relationships.EntityReference` 是领域 UUID/runtime/scope 引用，不是 Arch 历史同名类型；框架移除自己的类型不要求删除项目的领域引用。

来源：[官方 README](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/README.md)、[2.0.0 Release](https://github.com/genaray/Arch/releases/tag/2.0.0)、[GitBook EntityReference](https://arch-ecs.gitbook.io/arch/documentation/utilities/entityreference)、[Entity.cs](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/Entity.cs)、[CommandBuffer.cs](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Buffer/CommandBuffer.cs)。

## 3. World 生命周期与跨世界失效

**源码观察：**

- `World.Create(...)` 创建并登记 World。默认普通包的 Entity 携带 WorldId。
- `World.Destroy(World world)` 直接调用 `world.Dispose()`；不是要求先 Dispose 再 Destroy 的两级清理。
- Dispose 有已释放保护并处理 World 登记/回收。应用仍负责清理其自己的 UUID、关系、槽位、订阅、排队请求与缓冲。
- Dispose 会允许 World.Id 被复用。Clear 清空 EntityInfo、回收队列及存储；后续新实体的初始版本可能与旧实例碰撞。

**对 NLTX 的推导：**世界实例 token 必须独立于 Arch World.Id。每次候选加载、切换或重建生成新 token；旧 token 的排队请求先拒绝，再检查 Arch Entity。本轮按会话创建新 World，不在原 token 下 Clear 后继续解析旧实体。

来源：[官方 World 文档](https://arch-ecs.gitbook.io/arch/documentation/world)、[World.cs](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/World.cs)。这一失效策略是项目设计要求，不是 Arch 替项目维护 epoch 的保证。

## 4. Entity、存活检查与组件查找

**稳定源码事实：**普通构建下 `Arch.Core.Entity` 包含 `Id`、`WorldId`、`Version`，相等比较三者。只比较 Id 会丢失实例代数与世界作用域。

`World.IsAlive(Entity)` 先拒绝非正 Version，再按 Id 查找 EntityData 并比较 Version；该实现没有检查 `entity.WorldId == world.Id`。`Get/Has/TryGet` 等高频方法也不能被当作自动检查世界和实体代数的安全边界。

因此，外部/缓存/排队入口的项目检查顺序应为：当前会话 token → Entity.WorldId 与目标 World.Id 一致 → World.IsAlive(Entity) → 领域生命周期与能力 → 获取组件。查询回调取得的当前 Entity 可以按本次查询合同使用，避免在可信热循环中重复所有边界查验。

| 稳定 API | 语义 |
| --- | --- |
| `world.Get<T>(entity)` | 返回组件 ref；访问期由调用者控制 |
| `world.TryGet<T>(entity, out T component)` | 获取值；struct 为副本，修改副本不提交 |
| `world.TryGetRef<T>(entity, out bool exists)` | 获取组件 ref 与存在标记；不存在时不可使用返回 ref |
| `world.Has<T>(entity)` | 组件存在检查，不能替代完整实体校验 |
| `world.Set<T>(entity, component)` | 写已有组件，不能当成自动 Add/Upsert |
| `world.Add<T>(entity, component)` / `Remove<T>(entity)` | 改变组件组合，属于结构变化 |
| `AddOrGet<T>` | 稳定版本存在的按需能力；具体重载在探针核对 |

class 组件和 struct 组件均可作为数据。class/数组/集合是否在两个实体间共享由创建 owner 决定，Arch 不提供领域深复制、不让同一个可变对象只附着一次的通用保证。

领域 UUID、账户、存档对象与协议槽位不能改成 Entity.Id/WorldId/Version；这三个数是当前框架运行时定位值。

来源：[Entity.cs](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/Entity.cs)、[World.cs](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/World.cs)、包内 `lib/net8.0/Arch.xml`、[官方 Entity 文档](https://arch-ecs.gitbook.io/arch/documentation/entity)。

## 5. 查询及确定性

`QueryDescription` 提供 `WithAll/WithAny/WithNone/WithExclusive`。All 要求列出的组件全部存在，允许额外组件；Any 至少命中一个；None 排除列出的组件；Exclusive 匹配精确组合。可复用固定 QueryDescription，避免每次构造。

原生 `World.Query` 可给回调传入 Entity 与组件 ref；无 Entity、多组件、InlineQuery 及低层 chunk 方式也存在。初轮统一普通 Query，性能数据证明有必要时才采用更低层方式。

**官方文档与源码观察：**Arch 按 archetype/chunk 布局遍历，删除可用另一个实体填补位置。查询顺序不能当作 NLTX 的旧局部槽位升序、创建顺序或随机数消费合同。依赖“第一个匹配”、扫描到新 NPC 是否同 tick 更新的路径，必须使用领域顺序或明确的新规则。

普通组件字段修改可以在查询 ref 回调中完成。持有的 ref 不跨 Add/Remove、实体移动、删除、存储增长或跨帧保留；对其他实体的结构变化也要检查影响范围。

来源：[Query](https://arch-ecs.gitbook.io/arch/documentation/query)、[Entities in Query](https://arch-ecs.gitbook.io/arch/examples-and-guidelines/page-2)、[查询模板](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Templates/World.Query.cs)、[Entity Query 模板](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Templates/World.EntityQuery.cs)。

## 6. 结构变化与 CommandBuffer

官方文档明确指出：不知道组件存在情况时，Add 前检查不存在、Remove 前检查存在；重复 Add 或缺失 Remove 不能依赖库自动拒绝。

查询内结构变化的官方说明区分具体情况：有些创建/当前元素删除方式在当前遍历实现下可工作，跨实体删除可能造成重复处理，Add/Remove 可能移动 archetype。文档推荐使用缓冲或在查询外执行。项目不必把这些有限布局行为扩大成“查询内任意结构变化都安全”的合同。

`Arch.Buffer.CommandBuffer` 位于普通 Arch 核心包/程序集，不是需要再安装的 `Arch.Buffer` NuGet 包。

**稳定源码观察：**

1. `Playback(World world, bool dispose=true)` 由主线程执行。
2. 回放按 Create → Add → Set → Remove → Destroy 分组，不是所有录入操作的 FIFO。
3. 重复 Set/Add 可能覆盖之前记录的值，不能用它替代领域效果顺序。
4. `Create(ComponentType[] types)` 返回负 Id 暂存 Entity；它在回放前不是真正的 World 实体。
5. Resolve 并非公开的通用生产映射协议。不能猜测最终 Id，把暂存实体发布成 UUID/网络/关系对象。
6. 该机制不是事务，也不能假定回放失败会撤销已经执行的组。

**对 NLTX 的推导：**已有 Kernel 业务命令队列继续承担请求、权限、epoch 与效果排序。CommandBuffer 只用于适合分组提交的存储变化；有身份/跨实体关系的 Spawn 优先在领域安全创建阶段直接创建。需要延迟时先保存领域意图，结束 ref/query 后由 owner 校验提交，并处理失败和取消。

来源：[Structural Changes](https://arch-ecs.gitbook.io/arch/examples-and-guidelines/structural-changes)、[CommandBuffer 文档](https://arch-ecs.gitbook.io/arch/documentation/utilities/commandbuffer)、[固定 CommandBuffer 源码](https://github.com/genaray/Arch/blob/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Buffer/CommandBuffer.cs)。

## 7. 单线程采用与可选扩展

官方提供 `World.SharedJobScheduler`、`ParallelQuery` 和 `InlineParallelQuery`，且查询等待工作完成后才继续主线程。并行并不自动隔离其他实体、随机源、关系图、平台对象、日志或网络输出；访问外部对象仍需项目自己处理同步。

首轮沿用会话/Kernel owner thread 和显式阶段。不要在换框架时同时启用并行、PURE_ECS 或事件生成。

| 可选能力 | 事实与采用决定 |
| --- | --- |
| Arch.System | 系统组织扩展；现有领域 System 可继续直接使用 World，非核心采用前置 |
| Arch.System.SourceGenerator | 生成查询的独立扩展；先核对其版本对核心包依赖再采用 |
| Arch-PureECS | 不含 WorldId 的独立变体；首轮不采用 |
| Arch-Events | 启用 EVENTS 的独立变体；普通 Arch 不自动拥有其全部事件 API |
| Arch.Relationships | 可选关系扩展；本项目有领域关系/UUID合同，不机械替换 |
| Arch.Persistence | 可选持久化扩展；不替换现有 WorldFile DTO/Codec/端口 |

来源：[Multithreading](https://arch-ecs.gitbook.io/arch/documentation/optimizations/multithreading)、[Arch.System](https://arch-ecs.gitbook.io/arch/extensions/page-3/arch.system)、[SourceGenerator](https://arch-ecs.gitbook.io/arch/extensions/page-3/arch.system.sourcegenerator)、[PURE_ECS](https://arch-ecs.gitbook.io/arch/documentation/optimizations/pure_ecs)、[Events](https://arch-ecs.gitbook.io/arch/documentation/utilities/events)、[官方扩展仓库](https://github.com/genaray/Arch.Extended)。

## 8. 实施时需要补的运行证据

以下是明确的待验证项，不是本次读取已经通过的结果：

- net10.0 实际消费稳定包及传递依赖；固定签名的最小探针编译。
- Entity ID 复用、跨 World 相同 Id/Version、Clear/World.Id 回收后旧 token 拒绝。
- 外部边界没有绕过世界/Version 校验直接 Get/Set。
- struct ref 与 TryGet 副本行为；独立 class/数组组合；结构变化后重新取 ref。
- CommandBuffer 分组、覆盖、临时实体、默认清空、重复回放与异常部分效果。
- 当前 NLTX 创建/删除、查询顺序、物品冲突、世界切换和真实加载 rollback/retry。
- 固定支持集和相同输入下的性能；Arch 的宣传/外部 benchmark 不证明本项目会更快。

运行证据应由关联执行计划 A0–A8 逐批产生；本研究的 API 事实不作为代码迁移完成证明。
