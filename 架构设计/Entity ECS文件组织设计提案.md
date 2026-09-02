# Entity ECS 文件组织设计提案

## 1. 文档信息

| 项目 | 内容 |
| --- | --- |
| 适用范围 | `src/Share/Entity` ECS 样本线 |
| 参考项目 | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| 文档状态 | 已确认设计 |
| 设计原则 | 领域优先、职责次级、按需建目录、渐进迁移 |

本提案只讨论 Entity ECS 样本线的源码文件组织，不改变 `dome/` 下的世界文件、协议、
服务器、传输层或验证项目，也不规定 ECS 运行时、网络、渲染、UI、持久化和构建输出的
目录。

## 2. 背景与问题

当前样本位于单一项目 `src/Share/Entity/Terraria.EntityEcs.csproj`，已有文件按技术类别
集中在以下目录：

```text
src/Share/Entity/
  Components/
  Queries/
```

这种布局适合文件数量较少的起步阶段，但随着系统、事件、命令和定义加入，会产生两个
问题：

1. 同一项游戏能力的状态、行为和查询被横向拆散，维护者需要跨多个全局目录理解一个
   规则。
2. 共享组件容易按首次使用者归类，导致 `Velocity`、`Collider` 等跨实体能力被错误地
   放入 `Player`、`Npc` 或 `Projectile` 目录。

Space Station 14 的组织方式提供了可复用的启发：先按玩法领域切片（例如 `Doors`），
再在领域内部按组件、系统和事件分类；当共享、服务端和客户端实现并存时，保持领域名
一致并在端别目录下提供对应实现。本项目当前只有一个共享 ECS 样本项目，因此只采用
“领域内聚”原则，暂不提前创建 `Server`、`Client` 空壳。

## 3. 设计目标与非目标

### 3.1 目标

- 维护者能够从领域目录理解一项游戏规则的完整边界。
- 共享组件按其表达的游戏能力归属，而不是按实体类别归属。
- `Component`、`System`、`Query`、`Event`、`Command`、`Snapshot` 和 `Definition` 的
  生命周期与职责清晰可辨。
- 新领域可以渐进增加，不要求一次性迁移整个样本。
- 未来拆分共享、服务端和客户端实现时，不需要重新设计领域名称。
- 目录组织不承担系统调度职责；执行顺序由显式代码契约表达。

### 3.2 非目标

- 本提案不引入新的 ECS 框架、注册机制或运行时抽象。
- 本提案不要求立即移动现有文件。
- 本提案不创建没有实际文件的空目录。
- 本提案不重构 `dome/` 下已经存在的多项目结构。
- 本提案不把协议 DTO、世界文件模型、数据库模型或构建脚本混入 Entity ECS 领域。

## 4. 采用的总体结构

### 4.1 当前阶段：单项目、领域优先

目标目录如下。目录仅在拥有实际文件时创建：

```text
src/
  Share/
    Entity/
      Terraria.EntityEcs.csproj

      Entity/
        Components/
          EntityIdentityComponent.cs
          LocationComponent.cs
          DirectionComponent.cs

      Movement/
        Components/
          VelocityComponent.cs

      Physics/
        Components/
          ColliderComponent.cs
        Queries/
          EntityGeometryQuery.cs
          EntityHitbox.cs
          EntitySpatialQuery.cs

      Environment/
        Components/
          LiquidComponent.cs
```

这里的 `Entity` 是基础实体领域，不是实体继承树。它表达实体身份和基础空间状态；
玩家、NPC、投射物等实体应通过组合领域组件构成，而不是创建 `PlayerEntity`、
`NpcEntity` 或 `ProjectileEntity` 派生目录。

### 4.2 未来阶段：出现端别实现后的外壳

只有在共享、服务端和客户端出现真实的独立代码与项目引用后，才引入端别外壳：

```text
src/
  Shared/
    Entity/
      Components/
      Queries/
      Systems/

  Server/
    Entity/
      Systems/

  Client/
    Entity/
      Systems/
```

端别拆分时，各端仍应围绕相同领域命名；例如服务端和客户端的门系统都应位于 `Doors`
领域，而不是建立全局的 `Server/Systems` 和 `Client/Systems` 技术堆栈。若端别实现
仍然很少，继续放在单一共享项目中更合适。

## 5. 领域与职责契约

### 5.1 领域命名

第一层目录回答“这是哪项游戏能力或规则”，使用 PascalCase 的游戏语义名称，例如：

- `Entity`：实体身份与最低层空间状态。
- `Movement`：速度、移动意图和移动规则。
- `Physics`：碰撞、命中体、空间查询和几何约束。
- `Combat`：生命、伤害和伤害修正。
- `Player`、`Npc`、`Projectile`：仅放实体类别独有的状态或规则。
- `Environment`：液体等世界环境能力；当液体规则稳定并具有独立系统时，可进一步
  提取为 `Liquid` 领域。

不要建立以下全局技术目录作为长期导航入口：

```text
src/Share/Entity/Components/
src/Share/Entity/Systems/
src/Share/Entity/Events/
```

也不要建立语义不明的 `Shared/Components/` 收容目录。无法归属的类型应先澄清其业务
语义，再决定是否形成新领域。

### 5.2 领域内职责目录

| 目录 | 放置内容 | 语义与生命周期 |
| --- | --- | --- |
| `Components/` | 实体或世界的持续状态 | 随模拟 tick 变化，可被系统读写 |
| `Systems/` | 规则计算、事件处理和状态提交逻辑 | 代码固定，按显式调度执行 |
| `Queries/` | 只读访问、几何计算和筛选逻辑 | 不直接拥有持续状态 |
| `Events/` | 已经发生的一次性事实 | 发布后短暂存在，不替代组件状态 |
| `Commands/` | 尚待提交的确定性变更意图 | 由系统产生，提交后失效 |
| `Snapshots/` | tick 边界的稳定只读输入视图 | 创建、替换或失效由边界控制 |
| `Definitions/` | 跨实例复用的静态规则或原型 | 通常不可变，不随 tick 改变 |

`Queries/` 是当前样本已经存在的职责分类，因此保留在领域内部。查询与快照不能混用：
查询描述“如何读取或计算”，快照描述“某个 tick 使用的稳定输入”。

### 5.3 共享组件归属规则

放置一个新组件时依次判断：

1. 是否只属于一种实体类别？是则放入该实体类别领域，例如 `PlayerInputComponent` 放入
   `Player/Components/`。
2. 是否表达多个实体共享的一项明确能力？是则放入能力领域，例如 `HealthComponent`
   放入 `Combat/Components/`，`VelocityComponent` 放入 `Movement/Components/`。
3. 是否几乎适用于所有实体且不表达特定玩法？是则放入 `Entity/Components/`。

组件归属应由其不变量和主要读写者决定，而不是由第一个调用它的系统决定。

## 6. 当前文件迁移映射

本节只定义目标归属，不在本提案中执行移动。

| 当前文件 | 目标位置 | 归属理由 |
| --- | --- | --- |
| `Components/EntityIdentityComponent.cs` | `Entity/Components/` | 实体身份是基础实体状态 |
| `Components/LocationComponent.cs` | `Entity/Components/` | 表达实体在世界中的基础位置 |
| `Components/DirectionComponent.cs` | `Entity/Components/` | 当前作为基础空间朝向使用；若未来只服务移动意图，再迁入 `Movement` |
| `Components/VelocityComponent.cs` | `Movement/Components/` | 表达跨实体共享的移动能力 |
| `Components/ColliderComponent.cs` | `Physics/Components/` | 表达碰撞能力，不属于某一实体类别 |
| `Components/LiquidComponent.cs` | `Environment/Components/` | 表达环境液体状态；液体规则独立后可提取为 `Liquid` 领域 |
| `Queries/EntityGeometryQuery.cs` | `Physics/Queries/` | 几何约束和空间计算属于物理能力 |
| `Queries/EntityHitbox.cs` | `Physics/Queries/` | 命中体是碰撞/命中查询数据 |
| `Queries/EntitySpatialQuery.cs` | `Physics/Queries/` | 空间筛选依赖物理空间和碰撞边界 |

如果实现细节表明某个查询只服务于实体索引而不涉及物理约束，可以在迁移评审时将其
放入 `Entity/Queries/`；目录归属以实际不变量为准，不以文件名中的 `Entity` 前缀
机械决定。

## 7. 依赖与调度规则

目录层级只表达归属，不自动表达依赖。代码应遵守以下边界：

- `Components` 和 `Definitions` 保持数据与静态规则纯净，不反向依赖具体系统。
- `Queries` 可以读取组件或快照，但不应隐式修改组件。
- `Systems` 可以组合查询、读取组件、消费事件并产生命令。
- `Commands` 由明确的提交系统消费；提交完成后发布对应事件或生成新快照。
- 跨领域调用通过稳定的查询、命令、事件或接口完成，避免直接访问对方的私有状态。
- 系统执行顺序必须通过调度器、阶段声明或注册代码显式表达，禁止依赖文件枚举顺序、
  目录顺序或项目文件顺序。

推荐的确定性数据流为：

```text
Snapshot / Component
        |
        v
      Query
        |
        v
      System -----> Command
        |              |
        |              v
        +--------> Commit System
                       |
                       v
                 Component / Snapshot
                       |
                       v
                     Event
```

该流程不是强制的运行时实现，而是用于区分“当前状态”“读取计算”“变更意图”和“已
发生事实”的文件职责。

## 8. 渐进迁移策略

迁移应以小批次进行，每批保持项目可编译且不改变行为。

### 阶段 A：冻结归属规则

- 将本提案作为 `src/Share/Entity` 的组织基线。
- 新增类型必须先选择领域，再选择职责目录。
- 暂不为不存在的系统、事件或端别创建空目录。

### 阶段 B：按领域移动现有文件

建议按以下顺序迁移，避免一次性大范围改动：

1. 创建 `Entity`、`Movement`、`Physics`、`Environment` 中实际需要的目录。
2. 先移动无行为变化的组件文件。
3. 再移动查询文件并修正命名空间或项目内引用。
4. 每批完成后运行受影响项目的编译验证。

### 阶段 C：引入新职责

- 首次出现系统时，在对应领域创建 `Systems/`。
- 首次出现一次性事实时创建 `Events/`；不要把事件塞入组件目录。
- 首次出现待提交变更时创建 `Commands/`，并同时明确消费它的提交系统。
- 只有存在稳定跨实例静态规则时才创建 `Definitions/`。

### 阶段 D：评估端别拆分

满足以下条件后再考虑 `Shared/Server/Client` 外壳：

- 至少有一项领域同时存在共享规则和端别专有规则。
- 端别代码拥有独立项目边界或独立依赖约束。
- 不拆分会导致共享项目引用明显膨胀或出现反向依赖。

## 9. 验收清单

每次新增或迁移 Entity ECS 文件时，检查：

- [ ] 文件位于表达游戏语义的领域目录，而不是全局技术目录。
- [ ] 组件、查询、系统、事件、命令、快照和定义的职责没有混淆。
- [ ] 跨实体共享能力按能力领域归属。
- [ ] 没有为了完整性创建空目录或泛化 `Shared/Components/`。
- [ ] 一个核心公开类型对应一个 PascalCase 文件名。
- [ ] 目录移动没有改变命名空间、公共 API 或运行时行为（除非变更另有批准）。
- [ ] 系统顺序由显式调度/注册代码保证，而不是由文件枚举顺序推断。
- [ ] 受影响的 `Terraria.EntityEcs.csproj` 使用仓库规定的串行脚本完成编译验证，输出
      位于 `Build/bin/`。
- [ ] 迁移没有触碰 `dome/` 或无关测试项目。

## 10. 结论

`src/Share/Entity` 采用“领域优先、职责次级”的单项目布局：先用 `Entity`、`Movement`、
`Physics` 和 `Environment` 表达能力边界，再在领域内部按 ECS 文件职责分类。该布局
保留当前样本的轻量性，避免照搬 SS14 的大规模端别项目结构，同时为未来出现服务端和
客户端实现保留稳定的领域命名和迁移路径。

本提案只锁定文件组织规则；具体文件移动和新系统实现应作为后续独立变更，逐批提交、
逐批验证。
