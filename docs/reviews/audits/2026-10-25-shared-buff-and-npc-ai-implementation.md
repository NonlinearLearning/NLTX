# 共享 Buff 与 NPC AI 实现说明

- 日期：2026-10-25
- 范围：`src/NSSLC/Components/Share/Attributes/Buff`、`src/NSSLC/Components/Npc`
- 状态：implemented-foundation
- 运行接入：组件和纯领域规则已实现；宿主仍需显式注册 Tick 系统、AI 绑定和快照提供者

## 目标

本切片把 NPC 的 Buff 和行为状态从实体类别专属收容器改成共享能力和共享规则：

```text
实体组合
  → BuffReceptionCapabilityComponent / BuffImmunityComponent / BuffListComponent
  → BuffAdmissionSystem
  → BuffSystem
  → AttributeSetComponent / AttributeSystem
```

NPC 行为则使用：

```text
AiComponent
  + AiBehaviorSelectionComponent
  + NpcBehaviorStateComponent
  → AiSystem<NpcAiIntent>
  → NpcAiIntentAdapter
  → MovementIntent / 后续 Combat intent
```

实现不依赖客户端的 `Main` 数组、实体大对象、静态免疫表、网络字段或客户端命名。

## Buff 组件和规则

### 接收能力

`BuffReceptionCapabilityComponent` 只声明接收资格：

- `Disabled`：拒绝所有 Buff；
- `AllowListed`：只接受 `AllowedBuffs` 中的免疫键；
- `AllowAll`：接受所有已注册定义，仍受免疫表约束。

它不保存当前 Buff。`BuffListComponent` 是共享的可变状态组件，保存 `ActiveBuff`：效果 ID、
免疫键、来源 ID、剩余 Tick 和层数。`BuffImmunityComponent` 只保存免疫键集合。

### 定义和应用

`BuffDefinitionRegistry` 保存不可变 `BuffDefinition`。定义包括：

- 效果 ID和免疫键；
- 初始持续 Tick；
- 最大层数；
- 对 `AttributeSetComponent` 的 `BuffAttributeModifierDefinition`。

`BuffSystem.TryApply` 的顺序固定为：

1. 检查请求层数和来源 ID；
2. 解析不可变定义；
3. 由 `BuffAdmissionSystem.Check` 检查接收能力和免疫；
4. 以“效果 ID + 来源 ID”定位已有实例；
5. 计算刷新和层数上限；
6. 移除旧来源的属性修饰器；
7. 写入新的 `ActiveBuff`；
8. 按当前层数重新加入 `AttributeModifier`。

`BuffSystem.Advance` 每次推进一个逻辑 Tick，移除到期实例并按来源 ID清理属性修饰器。
周期伤害、治疗、击退和召唤物维护不在本系统内直接执行；它们必须由后续领域产生意图，
再交给 Combat、Movement 或生命周期系统提交。

### Arch 适配

`BuffTickSystem` 查询所有带 `BuffListComponent` 的实体；属性组件是可选的，只在实体同时
拥有 `AttributeSetComponent` 时同步修饰器。该系统不创建或删除实体，不执行网络、文件或
客户端表现副作用。

## NPC 行为组件和适配器

`AiBehaviorSelectionComponent` 只保存两个稳定内容键：

- `BehaviorId`：选择内容行为；
- `ProfileId`：选择不可变参数目录。

`NpcBehaviorStateComponent` 只保存可观察运行态：当前阶段、阶段进入 Tick、上次决策 Tick、
决策冷却、动作阶段和两个有限行为槽位。Planner、GOAP Agent、HTN Context、World、Arch
Entity 和服务引用由 `AiBinding` 管理，不进入组件。

`NpcAiIntent` 是 `unmanaged` AI 输出，允许：

- 更新 NPC 行为阶段；
- 产生移动意图；
- 标记战斗动作开始或取消。

`NpcAiIntentAdapter` 要求实体同时拥有：

```text
AiComponent
AiBehaviorSelectionComponent
NpcBehaviorStateComponent
MovementIntent
```

它调用宿主提供的 `IAiSnapshotProvider<Arch.Entity>`，消费共享 `AiEntitySystem` 的结果，
更新行为状态或覆盖本 Tick 的 `MovementIntent`。它不直接写位置、速度、生命、属性或 Buff。

## 生命周期和接入边界

宿主负责：

1. 注册 `BuffDefinition`；
2. 为需要 Buff 的实体附加接收能力、Buff 列表和可选免疫/属性组件；
3. 以明确阶段注册 `BuffTickSystem`；
4. 创建 `AiSystem<NpcAiIntent>`、注册行为工厂并为实体 Attach binding；
5. 提供只读快照，并在 NPC 删除、AI 禁用或世界卸载时执行 `Stop → 消费清理意图 → Dispose`；
6. 在空间和 Combat 系统中消费 `MovementIntent` 和后续战斗意图。

这些代码已经可以独立构建，但不宣称完整 NPC 生成、感知、目标选择、Combat、死亡掉落或
Projectile 命中闭环已经接入。

## 验证

已执行：

```text
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore
```

结果：0 警告、0 错误；输出位于 `Build/bin/Terraria.Components/Debug/net10.0/`。

尚未完成的行为验证包括：真实 Arch 世界中 BuffTickSystem 的多实体查询、NPC 快照提供者与
行为绑定的完整生命周期、目标关系解析、Combat intent 提交和世界卸载清理。

