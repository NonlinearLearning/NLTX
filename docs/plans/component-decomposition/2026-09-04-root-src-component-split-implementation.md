# 根 src ECS 组件拆分实施记录

**Goal:** 在根 `src` 交付组件设计报告确认的无副作用 ECS 状态边界。

**Architecture:** 每个有独立状态所有权的领域拥有一个非空 `net10.0` 类库；跨实体引用统一由
`Terraria.Relationships` 的 `EntityReference` 表达。现有共享 Entity 项目保持不变。

**Tech Stack:** C#、.NET `net10.0`，无 ECS 框架依赖。

---

### 实施批次

1. 创建 Relationships、Combat、Physics 项目及其最小状态组件。
2. 创建 Player 和 Npc 项目，分别保留输入/AI 与实体生命周期的领域边界。
3. 创建 Projectile 项目，分别表达定义、行为、寿命、归属、伤害、穿透和轨迹方向。
4. 创建 Items 与 StatusEffects 项目，分别表达物品定义/实例、库存关系和效果集合。
5. 静态审阅新增文件的命名、命名空间、项目引用方向、目录非空性及 diff 空白错误。

### 验证例外

用户明确要求“不做测试”。因此本实施记录不创建、修改或运行测试，也不运行 `dotnet restore`、
`dotnet build`、`dotnet test` 或 `dotnet run`。最终只报告源文件静态审阅结果；不得将其表述为
已编译或已通过测试。
