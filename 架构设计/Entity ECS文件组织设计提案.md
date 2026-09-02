# Entity ECS 文件组织设计提案

## 1. 文档信息

| 项目 | 内容 |
| --- | --- |
| 适用范围 | `src/Share/Entity` ECS 样本线 |
| 参考项目 | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| 文档状态 | 已确认设计 |
| 设计原则 | 领域优先、文件直放、按需建目录、渐进迁移 |

本提案只规定 Entity ECS 的组件与相关查询文件组织。它不规定 System、Event、Command、
Snapshot、Definition、ECS 运行时、网络、渲染、UI、持久化、构建输出或 `dome/` 下其他
项目的目录。

## 2. 设计背景

当前样本是单一项目 `src/Share/Entity/Terraria.EntityEcs.csproj`，源码按技术类别集中在
`Components/` 和 `Queries/`。这种结构在文件较少时简单，但会把不同游戏能力混在同一
技术目录中；随着组件增加，维护者需要从类型名而不是目录判断领域归属。

Space Station 14 的可采纳经验是：第一层优先表达玩法领域，相关代码在领域边界内聚合；
领域内部的子目录则按规模和稳定职责按需出现，而不是每个领域套用同一份固定模板。其
`Content.Shared/Doors/` 同时包含根目录文件、`Components/`、`Systems/` 和
`Electronics/`；`Movement/` 使用 `Components/` 与 `Events/`；`EntityTable/` 使用
`Conditions/` 与 `EntitySelectors/`。这说明“领域优先”与“职责子目录”是两个独立决策。

本项目当前只有一个轻量共享 ECS 项目，不照搬 SS14 的大规模 `Shared/Server/Client`
项目树，也不预先创建空的职责目录。

## 3. 目标与非目标

### 3.1 目标

- 通过领域目录表达组件和查询服务的游戏语义。
- 跨实体共享能力按能力归属，而不是按首次使用的实体类别归属。
- 小型领域目录直接放置实际文件，不增加无实际价值的 `Components/` 或 `Queries/` 子目录。
- 子目录只在领域内部形成稳定的职责聚合、规模边界或依赖边界后按需引入。
- 组件与查询文件的迁移不强制改变 C# 命名空间。
- 为未来扩展系统或端别实现保留清晰边界，但不在本提案中规定其位置。

### 3.2 非目标

- 不在本轮建立测试项目；查询边界测试作为后续质量待办。
- 不在本轮实现文件移动、组件重构或公共 API 变更。
- 不为 `Systems/`、`Events/`、`Commands/`、`Snapshots/`、`Definitions/` 创建目录规范。
- 不提前创建 `Shared/Server/Client` 三套目录或项目。
- 不触碰 `dome/` 下的源码和测试项目。

## 4. 目标目录结构

```text
src/
  Share/
    Entity/
      Terraria.EntityEcs.csproj

      Entity/
        EntityIdentityComponent.cs
        LocationComponent.cs
        DirectionComponent.cs

      Movement/
        VelocityComponent.cs

      Physics/
        ColliderComponent.cs
        EntityGeometryQuery.cs
        EntityHitbox.cs
        EntitySpatialQuery.cs

      Liquid/
        LiquidComponent.cs
```

### 4.1 目录规则

1. 先按游戏能力或规则选择领域目录，再决定文件属于组件还是查询。
2. 小型领域目录直接承载文件；当前不创建 `Entity/Components/`、`Movement/Components/`
   或 `Physics/Queries/` 等固定模板子目录。
3. 只有领域中确实存在文件时才创建目录，不为未来可能出现的职责预留空目录。
4. 当领域内部出现稳定的职责聚合、规模增长或依赖边界时，才按需增加子目录；该决定
   必须以实际文件和不变量为依据，并单独记录迁移影响。
5. 本提案的领域目录只涵盖组件和相关查询；系统等其他职责若未来出现，另行提出组织
   决策，不自动塞入当前领域目录。
6. 目录路径变化不自动要求命名空间变化；命名空间迁移是后续独立变更。

## 5. 领域归属

### 5.1 `Entity`

放置所有实体都可能拥有、且不表达特定玩法的基础状态。当前包括：

- `EntityIdentityComponent`：ECS 权威实体身份根。
- `LocationComponent`：实体世界坐标。
- `DirectionComponent`：当前样本的基础水平朝向。

`Entity` 是领域名称，不是实体继承树；不建立 `PlayerEntity`、`NpcEntity` 或
`ProjectileEntity` 派生目录。

### 5.2 `Movement`

放置跨实体共享的移动能力状态。当前包括：

- `VelocityComponent`：实体速度。

当未来出现明确的移动意图、移动规则或移动查询时，仍先评估其是否属于 `Movement`，
而不是按实体类别复制一份组件。

### 5.3 `Physics`

放置碰撞、命中体、几何和空间访问相关文件。当前包括：

- `ColliderComponent`：碰撞尺寸状态。
- `EntityGeometryQuery`：中心点、边缘点和命中体等派生几何计算。
- `EntityHitbox`：几何查询使用的命中体值类型。
- `EntitySpatialQuery`：距离、角度、方向和范围等空间查询。

查询文件直接放在 `Physics/`，不再增加 `Physics/Queries/` 子目录。

### 5.4 `Liquid`

液体作为独立能力领域，组件文件直接放在该领域目录：

- `LiquidComponent` → `Liquid/LiquidComponent.cs`。

Player、NPC 和 Projectile 的液体特有规则不因共享 `LiquidComponent` 而合并；本提案不
规定这些规则文件的位置。

## 6. 当前文件迁移映射

本节只定义目标路径，不在本提案中执行移动。

| 当前路径 | 目标路径 | 归属依据 |
| --- | --- | --- |
| `Components/EntityIdentityComponent.cs` | `Entity/EntityIdentityComponent.cs` | 基础实体身份 |
| `Components/LocationComponent.cs` | `Entity/LocationComponent.cs` | 基础世界位置 |
| `Components/DirectionComponent.cs` | `Entity/DirectionComponent.cs` | 当前为基础水平朝向；未来有独立移动语义时再评估 |
| `Components/VelocityComponent.cs` | `Movement/VelocityComponent.cs` | 跨实体移动能力 |
| `Components/ColliderComponent.cs` | `Physics/ColliderComponent.cs` | 碰撞能力 |
| `Components/LiquidComponent.cs` | `Liquid/LiquidComponent.cs` | 独立液体能力领域 |
| `Queries/EntityGeometryQuery.cs` | `Physics/EntityGeometryQuery.cs` | 几何查询 |
| `Queries/EntityHitbox.cs` | `Physics/EntityHitbox.cs` | 命中体查询数据 |
| `Queries/EntitySpatialQuery.cs` | `Physics/EntitySpatialQuery.cs` | 空间查询 |

目录移动期间可以暂时保留现有 `EntityEcs.Components`、`EntityEcs.Queries` 命名空间；
命名空间是否按领域细分，留给后续独立变更决定。

## 7. 命名和基线门禁

这些门禁属于文件迁移的前置条件，但不扩大本提案的字段设计范围。

### 7.1 位置命名统一

查询链和相关引用必须统一使用 `LocationComponent`。不得同时保留公共
`TransformComponent` 与 `LocationComponent` 两套位置组件名称。当前源码中引用
`TransformComponent` 的地方，应在目录迁移前完成基线修复。

### 7.2 身份命名统一

`EntityIdentityComponent` 的权威身份语义使用 `EntityUuid`。`UUID` → `EntityUuid` 是
命名门禁；网络标识、账户 UUID、复制 ID 和 `whoAmI` 仍属于各自边界，不进入该身份根
命名。

### 7.3 朝向类型门禁

`DirectionComponent` 的公共朝向表示使用 `DirectionKind`，不再以可任意赋值的裸 `int`
作为公共契约。本提案只记录类型门禁，不规定枚举字段实现细节；表现方向、NPC 垂直方向
和 Projectile 特殊方向继续保持独立语义。

### 7.4 编译验证门禁

目录迁移前后分别验证 `Terraria.EntityEcs.csproj`：

1. 先修复 `LocationComponent` 命名基线。
2. 通过仓库规定的串行脚本执行项目编译。
3. 完成目录移动后，用相同命令再次编译。
4. 记录命令、项目、退出码、警告/错误数量和 `Build/bin/` 输出路径。

当前不建立测试项目；几何/空间查询的零尺寸、零距离和退化向量边界测试列为后续质量
待办，不作为本轮目录设计的阻塞条件。

## 8. 渐进迁移策略

### 阶段 A：先修复基线

- 统一查询参数和引用中的 `TransformComponent` → `LocationComponent`。
- 将 `UUID` → `EntityUuid`、`int` 朝向 → `DirectionKind` 记录为实现门禁。
- 在串行构建空闲且所有者明确后，验证项目基线可编译。

### 阶段 B：按领域移动文件

- 创建实际需要的 `Entity/`、`Movement/`、`Physics/`、`Liquid/` 目录。
- 按第 6 节映射移动组件和查询文件。
- 默认保持命名空间不变，只修正路径相关的项目引用或文档链接。
- 用相同串行命令重新编译，确认目录变化没有引入错误。

### 阶段 C：后续独立决策

以下内容不由本提案自动触发：

- 命名空间是否按领域细分；
- System、Event、Command、Snapshot、Definition 的文件组织；
- 是否新增测试项目；
- 是否拆分 Shared/Server/Client 项目；
- 是否将某个领域进一步拆成更细的子领域。

## 9. 验收清单

- [ ] 文件按游戏领域归属，而不是继续放入全局 `Components/` 或 `Queries/`。
- [ ] 当前小型领域直接放文件，没有为模板完整性增加 `Components/` 或 `Queries/` 子目录。
- [ ] 未来新增子目录有明确的稳定职责、规模或依赖边界依据。
- [ ] `LiquidComponent` 位于独立 `Liquid/` 领域。
- [ ] 查询文件位于 `Physics/`，不建立 `Physics/Queries/`。
- [ ] `LocationComponent` 是唯一公共位置组件名称。
- [ ] `EntityUuid`、`DirectionKind` 作为后续实现门禁记录在案。
- [ ] 命名空间迁移未被误当成本轮目录迁移的一部分。
- [ ] 未新增测试项目，查询边界测试已记录为后续质量待办。
- [ ] 未触碰 `dome/` 或无关项目。
- [ ] 迁移前后均按仓库串行构建规则验证，并记录 `Build/bin/` 输出证据。

## 10. 结论

本项目的 Entity ECS 样本采用轻量的领域优先布局：`Entity`、`Movement`、`Physics` 和
`Liquid` 作为领域目录，组件与相关查询文件直接放在领域目录内。该方案吸收 SS14 的
领域内聚经验，同时采用其“子目录按需出现”的实践，而不复制其大规模端别项目和固定
职责模板，适合当前只有一个共享 ECS 项目的阶段。

字段模型、系统组织、测试项目和命名空间迁移均明确留给后续独立决策，避免文件结构设计
文档越界为实现或数据模型设计。
