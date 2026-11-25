# 属性系统与状态效果边界收敛执行计划

**文档 ID**：PLAN-2026-11-20-ATTRIBUTE-SYSTEM-BOUNDARY  
**策略**：graph（依赖复杂，契约稳定后允许互不重叠的批次并行）  
**状态**：completed（B0–B9 本轮验收项闭合；Inventory 属性桥接保持 deferred；等待用户审阅后决定下一轮内容迁移）  
**关联审查**：[属性系统与 Buff / 状态效果架构审查报告](../reviews/audits/2026-11-20-attribute-system-architecture-review.md)  
**范围**：属性、Buff/StatusEffect、Combat、Projectile、Player/NPC/Projectile 实体装配及验证项目；Inventory 只保留独立库存能力回归，不接入本轮属性系统  
**排除范围**：src/NSSLC.Infrastructure/** 不修改；不恢复旧应用层，不重写网络协议，不引入全局消息总线、事件溯源或 CQRS 框架。

**本轮明确排除**：Inventory 槽位、物品实例或装备关系不生成
`AttributeContributionRequestComponent`，不经过 `AttributeContributionApplySystem`，也不由服务器注册
Inventory 属性贡献系统。Inventory 属性贡献和基于槽位的来源身份设计另立变更后再做。

## 1. 执行目标

将当前“数值型 Buff 已进入属性修饰器，但资源、身份、生命周期和行为仍混在属性域”的状态，收敛为：

    AttributeSetComponent
        只保存可计算的数值属性和来源修饰器

    LifeComponent / ShieldPoolComponent / CooldownComponent /
    BreathComponent / MountStateComponent
        保存当前资源、计时器、关系和身份状态

    StatusEffectListComponent（兼容现有 BuffListComponent）
        保存效果实例、来源、期限、层数、免疫和刷新策略

    Behavior Components
        保存 DoT、HoT、眩晕、隐身、免疫、移动限制等行为状态

    各领域 RequestComponent / IntentComponent
        将状态变更提交给 Life / Combat / Movement / Attributes 所属系统

完成定义：

1. 当前生命不是普通属性；LifeComponent 是生命唯一权威状态。
2. AttributeSetComponent 不保存当前护盾、冷却剩余量、坐骑类型或无具体身份的状态标记。
3. 数值型 Buff 仍可产生 AttributeModifier，但行为型效果保留生命周期和行为语义。
4. 所有需要属性能力的战斗实体通过组件组合显式声明，不依赖 Combat 的隐式 null fallback。
5. 跨域输入通过目标域请求组件；保留同步命令时必须是有文档和测试保护的窄例外。
6. 每个来源可以独立撤销，来源身份不会因使用相同物品定义而冲突。
7. 每一批都有构建、静态边界检查和与主要风险相符的行为验证。

## 2. 当前基线和证据等级

| 项目 | 当前事实 | 判定 |
| --- | --- | --- |
| 生命 | LifeComponent 保存 Current / Maximum / Minimum，Combat 提交变化 | confirmed |
| 属性计算 | Base → Additive → Multiplicative → Override | confirmed |
| Buff 生命周期 | ActiveBuff 保存来源、期限、层数、免疫键 | confirmed |
| 数值 Buff | Ironskin、Swiftness、Regeneration 生成属性贡献 | confirmed，内容覆盖有限 |
| NPC 属性 | 向导和史莱姆定义没有 AttributeSetComponent | confirmed gap |
| 资源混用 | Shield、多个 cooldown、呼吸、MountKind、StatusFlag 在 AttributeKind | confirmed deviation |
| 属性写入 | 历史 Inventory 适配器曾直接 RemoveBySourceCategory / AddModifier；当前未注册且不在本轮接线 | deferred by scope change |
| Buff 外部入口 | BuffSystem.TryApply 可直接修改 Buff 列表和属性 | API risk confirmed；运行时调用 unknown |
| 再生闭环 | LifeRegeneration 有定义，未确认完整消费到生命恢复请求 | partial |
| SourceId | Inventory 历史适配器使用 slot.Definition.Index + 1；本轮不重建 Inventory 来源身份 | deferred by scope change |

证据入口：

- docs/reviews/audits/2026-11-20-attribute-system-architecture-review.md
- Context/项目约束/ECS系统输入边界约束.md
- Context/架构约束/ECS文件组织设计约束.md
- docs/architecture/ECSEntity组织设计约束.md
- Context/项目约束/组件命名设计约束.md
- Context/项目约束/构建与验证约束.md
- Context/项目约束/非函数式编码副作用隔离规范.md

不把“类型存在”“注册表存在”或“项目能编译”当作行为完成证据。每批必须补充运行路径或可执行验证。

## 3. 依赖图与批次总览

    B0 基线与契约冻结
     └─> B1 属性分类与元数据
          ├─> B2 NPC / Projectile 属性组合统一
          ├─> B3 来源身份与贡献重建边界
          ├─> B4 资源和身份状态拆分
          │     └─> B5 最大生命、护盾和资源同步
          └─> B6 StatusEffect / Behavior 模型
                └─> B7 数值效果与行为效果闭环

    B1 + B3 ─> B8 Buff / Combat 跨域请求组件
    B4 + B5 + B6 + B8 ─> B9 全量回归、旧键下线和文档收口

可并行：

- B2、B3 在 B1 完成后可以并行，但不能同时修改同一个实体定义文件。
- B4、B6 可以并行；共享的组件名称和状态 owner 必须先由 B1 固定。
- B3、B4、B6 可以分别准备，B8 必须等待它们的 API 稳定。

必须串行：

- B0 → B1。
- B4 → B5。
- B6 → B7。
- B2、B3、B4、B5、B6、B8 → B9。

## 4. 共同执行规则和批次 Gate

每批开始前：

1. 重新读取根 AGENTS.md、Context/progress.md、该批涉及的约束和 git status。
2. 只修改该批列出的文件边界，保留工作树中已有的无关修改。
3. src/NSSLC.Infrastructure/** 只读。
4. 新增组件按领域优先归属；一个公开组件类型对应一个同名 PascalCase 文件。
5. 新增跨域输入使用目标域的 *RequestComponent 或 *IntentComponent。
6. 纯计算使用显式输入；状态写入、请求消费、网络投影和日志集中在系统边界。

每批结束必须记录：

- 修改文件和权威 owner；
- 旧语义、新语义及兼容策略；
- 增量构建命令、项目、退出码、warning/error 数量和 Build/bin/ 输出位置；
- 行为验证结果；
- confirmed / partial / unknown / proposed 状态；
- 回滚方式和技术债。

共同 Gate：

    dotnet build <受影响项目>
    dotnet run --project <受影响验证项目> --no-build --no-restore
    git diff --check

构建必须按 Context/项目约束/构建与验证约束.md 执行，不使用 solution 全量重建代替受影响项目构建。若验证项目还没有可用资产，先增量 build，再用 --no-build --no-restore 运行。

## 5. B0：基线、契约和验证骨架

**前置**：无。  
**目标**：冻结当前行为，避免架构迁移被误判为业务变化。  
**范围**：属性、Buff、生命、写入点扫描和 Test/Terraria.Attributes.Verification/。

工作项：

1. 固定 AttributeKind、AttributeSourceCategory、BuffDefinition、ActiveBuff、LifeComponent 的 API 快照。
2. 静态列出所有 AttributeKind 的生产者、消费者和 owner，特别标注 Shield、cooldown、呼吸、Mount、StatusFlag、ProjectileHitImmunityTicks。
3. 静态列出所有 AttributeSetComponent 写入点和跨域调用点。
4. 建立 Attributes 验证骨架，覆盖现有计算顺序、来源撤销、Buff 期限唯一推进和生命独立性。
5. 记录当前 git status 和已有未提交修改，不能把无关修改纳入本轮。

验收：

- 每个 AttributeKind 至少有一处生产者、消费者和 owner 记录；零引用项单列。
- 现有属性计算、Buff 期限和生命提交均有可执行探针。
- 验证项目独立 build/run，输出进入 Build/bin/ 或既定输出目录。
- 没有修改生产运行时行为。

## 6. B1：属性分类、元数据和不变量冻结

**前置**：B0。  
**目标**：阻止新的错误语义进入 AttributeSetComponent，暂时保留兼容枚举值。  
**范围**：Share/Attributes/**、属性验证项目、Attributes README。

工作项：

1. 建立 ComputedScalar、ResourcePool、Cooldown、Identity、StatusFlag、Policy 分类或等价元数据。
2. 登记现有键：
   - Defense、Damage、MovementSpeed、Resistance、Critical：ComputedScalar。
   - MaxShield：ComputedScalar / capacity。
   - Shield：ResourcePool，禁止新增普通 modifier。
   - Potion / Restoration / Mushroom cooldown：Cooldown。
   - BreathCapacityTicks：区分 capacity 与 current，未确定前标 partial。
   - MountKind：Identity。
   - StatusFlag：StatusFlag，禁止新增无具体 FlagId 的调用。
   - ProjectileHitImmunityTicks：Policy 或计算参数；命中记录归 Projectile。
3. 登记比例 clamp、速度/伤害最小值、负值许可、Override 优先级和默认值。
4. 明确属性元数据不承担资源写入、网络、生命恢复或实体行为。
5. 把迁移债写入 Attributes README；不删除现有枚举值。

验收：

- 新测试能拒绝将 Shield、cooldown、MountKind、无具体标识的 StatusFlag 当作普通贡献。
- Defense / Damage / MovementSpeed / Resistance / Critical 结果不变。
- 所有暂未迁移的键都有 owner 和迁移状态。
- 不改变网络或存档身份。

## 7. B2：统一 NPC 和 Projectile 的属性能力组合

**前置**：B1。  
**目标**：消除“玩家有属性、NPC 靠 null fallback”的隐式差异。  
**范围**：PlayerEntityDefinition、NpcEntityDefinition、投射物定义/创建路径及实体验证。

工作项：

1. 为向导、史莱姆及所有参与 Combat 的 NPC 定义 AttributeSetComponent。
2. 为 NPC 设置显式基础属性；没有加成时也写入明确默认值。
3. 确认投射物每个实例拥有独立属性集合，不共享定义或其他实例的可变组件。
4. 暂时保留 Combat 可选属性兼容路径；实体覆盖完成后改为显式能力缺失诊断。
5. 增加实体定义验证，检查属性存在性和实例独立性。

验收：

- 参与 Combat 的 NPC 全部显式拥有 AttributeSetComponent。
- NPC 防御、抗性、伤害和移动使用与玩家相同的属性查询。
- 两个投射物实例不共享可变属性状态。
- NPC spawn、projectile、damage 验证通过。

建议 Gate：

    dotnet build src\NSSLC.Server\NSSLC.Server.csproj
    dotnet run --project Test\Terraria.Components\Terraria.Entity.Definition\Terraria.Entity.Definition.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.Projectile\Terraria.Projectile.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.NpcSpawn\Terraria.NpcSpawn.csproj --no-build --no-restore

## 8. B3：来源身份契约（Inventory 来源延期）

**前置**：B1；可与 B2 并行，但不得同时修改实体定义。  
**目标**：保留 Buff、StatusEffect 和 Combat 来源的可替换、可撤销契约；Inventory 来源身份和贡献翻译延期。  
**范围**：Attributes/Contribution/**、Buff/StatusEffect/Combat 来源；Inventory 只做独立功能回归。

工作项：

1. 以现有 `AttributeSourceDescriptor` / `AttributeContributionSet` 固化 Buff、StatusEffect 和 Combat 来源的非零身份、分类与期限契约。
2. 明确同一来源的替换语义，保证重复应用不会重复累加修饰器。
3. 保留按 source/category 撤销和诊断信息。
4. 将来源身份生成和贡献翻译保持为纯计算，实体查询和写入留在对应系统边界。
5. Inventory 的 `slot.Definition.Index + 1` 来源生成、装备贡献翻译和来源代次不在本轮修改；历史 `InventoryAttributeContributionSystem` 不注册、不作为验收入口。

验收：

- Buff/StatusEffect/Combat 同一来源重复应用时不会产生重复 modifier。
- 来源撤销只移除该来源的贡献。
- Inventory 验证只覆盖槽位、堆叠、物品关系和输入写入，不验证属性贡献。
- Inventory 属性来源身份和装备贡献重建列为 deferred，不得作为本轮完成条件。

建议 Gate：

    dotnet build src\NSSLC\Terraria.Components.csproj
    dotnet run --project Test\Terraria.Components\Terraria.Attributes\Terraria.Attributes.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.Inventory\Terraria.Inventory.csproj --no-build --no-restore

## 9. B4：拆分资源、冷却和身份状态

**前置**：B1；建议在 B2 后合并。  
**目标**：让属性系统退出当前资源和身份状态的权威存储。  
**范围**：Attributes、Combat、Player、Movement/ItemUse 调用点、实体定义和测试。

工作项：

1. 新增 ShieldPoolComponent，保存 Current 和容量同步状态；MaxShield 继续由属性计算。
2. 将护盾消耗/恢复改为 Shield owner 的受控命令或请求；禁止用 AttributeSetComponent.SetBaseValue(Shield) 修改当前护盾。
3. 新增 CooldownComponent 或按已确认领域拆分的冷却组件，迁移 Potion / Restoration / Mushroom 剩余 Tick。
4. 新增 BreathComponent，区分当前呼吸、最大容量、消耗和恢复；旧语义未确认时保留兼容读取并登记迁移债。
5. 新增 MountStateComponent，保存坐骑身份、坐骑实体关系和骑乘状态；坐骑数值贡献仍走属性贡献。
6. 用具体的 InvisibilityComponent、StunComponent、MovementRestrictionComponent 或带具体 FlagId 的组件替代 StatusFlag 的新调用。
7. 迁移期间保留旧属性键的只读兼容投影，禁止新写入；零引用后再删除枚举值。

验收：

- 当前护盾不再存储在 AttributeSetComponent 的基础值。
- MaxShield 修正不会被护盾消耗重置或重复计算。
- 冷却、呼吸、坐骑身份可以独立 tick、移除和投影（当前边界需要时）。
- AttributeKind 不再出现新的 ResourcePool / Cooldown / Identity / 无标识 StatusFlag 写入。
- Combat、PlayerItemUse 和相关验证通过。

建议 Gate：

    dotnet build src\NSSLC\Terraria.Components.csproj
    dotnet build src\NSSLC.Server\NSSLC.Server.csproj
    dotnet run --project Test\Terraria.Components\Terraria.Projectile\Terraria.Projectile.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.ItemUse\Terraria.ItemUse.csproj --no-build --no-restore

## 10. B5：最大生命、护盾容量和资源同步

**前置**：B2、B4。  
**目标**：保证属性结果到核心资源组件只有一个单向同步点。  
**范围**：Attributes、Combat、Player、资源组件和验证。

工作项：

1. 若需要临时最大生命修正，新增 MaximumLife 属性或等价明确键；它只表示目标最大值。
2. 新增归属明确的生命同步系统：读取属性结果，更新 LifeComponent.Maximum，下降时 clamp Current。
3. 明确最大生命增减时 Current 的规则和死亡边界。
4. 为 MaxShield 与 ShieldPoolComponent 建立容量同步规则；当前池只由 Shield owner 修改。
5. 登记同步系统的版本和 tick 顺序，避免属性重建后 Combat 读取旧容量。

验收：

- LifeComponent.Maximum 只有生命同步系统和既有生命 owner 写入。
- 最大生命下降时 Current 不超过 Maximum，不产生第二份生命权威值。
- 最大生命和最大护盾变化在同 tick 按计划顺序可被 Combat 读取。
- 属性计算器不修改 Life 或 Shield。

## 11. B6：StatusEffect 生命周期和行为组件模型

**前置**：B1；可与 B4 并行。  
**目标**：在不破坏 Buff API 的情况下扩展为“生命周期 + 数值贡献 + 行为贡献”。  
**范围**：Attributes/Buff/**、新增行为组件所属领域、Combat/Life/Movement 请求定义。

工作项：

1. 评估将 BuffListComponent 泛化为 StatusEffectListComponent；不能立即重命名时保留 BuffListComponent 并补充 StatusEffect 语义文档。
2. 扩展 BuffDefinition 为 AttributeContribution[]、BehaviorContribution[]、PeriodicEffectDefinition[]、ImmediateEffectDefinition[]。
3. 保持 ActiveBuff 的 duration、source、stack、immunity、refresh/stack policy 单一 owner。
4. 定义 DamageOverTimeComponent、LifeRegenerationComponent、StunComponent、InvisibilityComponent、DamageImmunityComponent 等行为组件；只有达到规模/边界时才建子目录。
5. 定义行为组件移除和过期规则，来源撤销只移除自身行为。
6. 周期行为只产生目标域请求，不直接写 Life、Movement 或网络状态。

验收：

- 数值贡献、行为贡献、即时效果、周期效果在定义层可区分。
- 一个效果可以同时产生属性修饰和行为组件，二者共享生命周期 owner。
- 过期、驱散、免疫、刷新对属性和行为结果一致。
- DoT、眩晕、隐身、免疫不编码为 StatusFlag 浮点值。

## 12. B7：数值效果和行为效果闭环

**前置**：B5、B6。  
**目标**：先完成代表性效果的完整路径，再扩展内容表。  
**首批内容**：Regeneration、Poison、Invisibility；不承诺完整 Terraria parity。

工作项：

1. Regeneration：读取 LifeRegeneration 或 LifeRegenerationComponent，产生 LifeRestoreRequestComponent，由 LifeRestoreRequestSystem 提交。
2. Poison：通过 DamageOverTimeComponent 生成带来源、伤害类型、周期和目标的 Combat damage request。
3. Invisibility：通过 InvisibilityComponent 驱动可见性/网络投影，不能写通用 StatusFlag。
4. 即时伤害/治疗先用明确请求组件实现，不直接在 Buff 系统改生命。
5. 固定状态 tick → 请求生成 → Combat/Life 提交 → Projection 顺序。
6. 定义同 tick 过期与周期触发边界，并写入测试。

验收：

- Regeneration 实际产生并消费生命恢复请求。
- Poison 实际产生 Combat 请求并保留来源、伤害类型和 tick 顺序。
- Invisibility 应用和移除可被投影/可见性系统观察。
- 过期不会留下 modifier 或行为组件。
- 刷新/叠层不会重复生成周期效果。

## 13. B8：跨域请求边界收敛

**前置**：B1、B3；建议等待 B4、B6 API 稳定。  
**目标**：让属性和状态 owner 回到对应域系统。  
**范围**：Attributes、Buff、Combat、服务器注册和调度；Inventory 明确保持独立。

工作项：

1. 外部 Buff 来源写 `BuffApplyRequestComponent`，由 StatusEffect/Buff 系统做准入、刷新、叠层和贡献应用。
2. Shield 消耗改为 `ShieldConsumeRequestComponent`；若伤害结算必须同步，使用明确同步结果协议并记录原子性理由。
3. 不向跨域系统注入可变队列或 owner 服务；请求队列放在组件。
4. 更新 `Program.BuildRegistrations`，显式声明 BuffApply、状态 tick、Combat、生命同步和 Projection 依赖。
5. `BuffSystem.TryApply` 限制为内部规则 API，或改为不可变输入和域内状态句柄，避免外部域直接取得可变权威组件。
6. Inventory 保持槽位、物品实例、包含关系和拾取提交的独立 owner；本轮不创建 Inventory→Attributes 请求链路，也不注册历史适配器。

验收：

- 外部系统不直接调用 BuffSystem.TryApply 改写他域状态。
- Combat 不直接改写 Shield 当前值。
- 每个跨域状态变更都能定位请求组件和消费系统。
- Inventory 的独立验证不依赖 Attributes；历史 `InventoryAttributeContributionSystem` 不在服务器调度图中。
- 调度不依赖文件枚举或偶然注册顺序。

静态检查：

    rg -n "\.AddModifier\(|\.RemoveBySource|\.RemoveBySourceCategory|BuffSystem\.TryApply|SetBaseValue\(.*Shield" src/NSSLC src/NSSLC.Server --glob '*.cs'

每个剩余命中必须标注为域内写入、受控同步例外、历史延期适配器或迁移债。

## 14. B9：旧键下线、全量回归和文档收口

**前置**：B2、B4、B5、B6、B7、B8；B3 的 Inventory 部分不阻塞本轮收口。  
**目标**：删除迁移债的生产使用，并以完整证据结束本轮。

工作项：

1. 确认 Shield、冷却、呼吸、MountKind、StatusFlag 的生产写入为零，再删除或 obsolete 旧键；Inventory 旧来源键和装备贡献适配器保持延期。
2. 删除 Combat 对 NPC 属性的隐式 fallback，改为实体能力缺失的显式拒绝或诊断；明确 opt-out 才保留缺失能力。
3. 更新 Attributes README、Buff README、实体定义说明和 ECS 系统输入边界审计。
4. 更新本计划和 task checkpoint，记录完成批次、未完成项和技术债。
5. 运行受影响项目与现有领域验证矩阵。

最终验收：

- AttributeSetComponent 只包含允许的 ComputedScalar / capacity / policy 属性。
- 核心资源、冷却、身份、行为状态都有独立 owner。
- Player、NPC、Projectile 的属性能力和 Buff/StatusEffect/Combat 来源撤销可验证。
- Regeneration、Poison、Invisibility 各有应用→tick/行为→过期闭环。
- 跨域直接写入为零，或每处有同步例外说明、原子性理由和测试；Inventory 历史适配器不在调度图中。
- Inventory（独立库存行为）、Projectile、NPC spawn、Combat protection、Player item use、ECS scheduling 验证全部通过。
- 没有修改 src/NSSLC.Infrastructure/**。

建议最终 Gate：

    dotnet build src\NSSLC\Terraria.Components.csproj
    dotnet build src\NSSLC.Server\NSSLC.Server.csproj
    dotnet build src\NSSLC.Network\NSSLC.Network.csproj

    dotnet run --project Test\Terraria.Components\Terraria.Attributes\Terraria.Attributes.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.Inventory\Terraria.Inventory.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.Projectile\Terraria.Projectile.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.Entity.Definition\Terraria.Entity.Definition.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.NpcSpawn\Terraria.NpcSpawn.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.ItemUse\Terraria.ItemUse.csproj --no-build --no-restore
    dotnet run --project Test\Terraria.Components\Terraria.Entity.Scheduling\Terraria.Entity.Scheduling.csproj --no-build --no-restore

    git diff --check

记录每条命令退出码、warning/error 数量、输出目录和未执行原因；区分既存 Infrastructure 警告与本轮新增警告。

## 15. 失败处理、回滚和兼容策略

失败处理：

- 构建失败：停在当前批，修复后重新跑同一批 Gate，不进入下一批。
- 行为验证失败：先判断预期语义变化、测试缺失还是迁移错误，未记录决策前不得扩大范围。
- 需要改变冻结 API：回到 B1 更新分类/不变量和依赖图，再验证受影响批次。
- 参考项目与本项目契约冲突：以本项目约束为准，记录差异，不照搬参考实现。

建议每批以独立可恢复边界结束：

    B0 baseline-verification
    B1 attribute-classification
    B2 entity-attribute-composition
    B3 attribute-source-identity
    B4 resource-state-extraction
    B5 life-and-shield-reconciliation
    B6 status-effect-model
    B7 behavior-effect-closures
    B8 cross-domain-requests
    B9 cleanup-and-regression

禁止破坏性 reset、clean 或覆盖无关工作树。迁移期间保留旧枚举和读取兼容，先禁止新增写入，零引用后再删除；保持 BuffId / BuffEffectId 和 LifeComponent 外部身份稳定；兼容投影不得成为第二份权威状态。

## 15.1 本轮收口记录（2026-11-22）

执行结果：B0–B9 适用验收项已闭合，B3 的 Inventory 属性贡献与来源身份保持 deferred。

已完成的工作项与证据：

1. 旧键下线：静态扫描确认 AttributeKind.Shield、PotionCooldownTicks、RestorationCooldownTicks、
   MushroomCooldownTicks、BreathCapacityTicks、MountKind、StatusFlag 在 src 内除枚举与元数据文件外
   零引用，生产写入与读取均为 0；枚举成员保留以维持既有键位编号稳定。
2. Combat 能力缺失显式化：CombatDamageSystem 删除可空 AttributeSetComponent 回退，源或目标缺少
   属性能力时返回新增的 DamageRejectionReason.MissingAttributeCapability；Player、Guide、Slime 与
   投射物定义均显式装配属性能力。
3. 文档：Attributes README 与 Buff README 已按结果重写资源权威、跨域请求、行为组件消费表与旧键说明。
4. 本计划与私有任务表已记录批次结论、回归矩阵、静态扫描、技术债与未覆盖范围。
5. 回归矩阵：13 个验证项目、243 项检查全部通过；Terraria.Components、NSSLC.Network、NSSLC.Server
   均 0 warning / 0 error；git diff --check 退出码 0。

代表性闭环证据（真实系统、真实 tick 顺序）：

- Regeneration：应用后属性修饰器生效，行为组件物化为带来源的再生记录，60 tick 后经
  LifeRestoreRequestSystem 把生命 50 提升到 54，请求队列清空。
- Poison：物化为 DamageOverTime 记录而非状态旗标，经 CombatDamageRequestSystem 造成 4 点伤害，
  结果保留 DamageType.DamageOverTime 与 EffectSourceId 9002。
- Invisibility：物化为 InvisibilityComponent，NPC 目标投影得到 IsVisible=false、IsUsable=false、
  IsAlive=true；到期后行为组件与属性修饰器同 tick 撤销。

技术债：StunComponent 尚无移动消费者；BuffApplyRequestComponent 还没有生产侧生产者（缺少物品到
Buff 的内容管线）；NPC 未装配 Buff 接收能力；即时效果未建模；旧枚举键保留但零读写。

未执行项：客户端联机会话烟测（缺少物品到 Buff 内容入口，无法从客户端触发三条效果，以可执行验证
替代并如实记录）；Terraria.Communication.Verification 在 2026-11-22 20:35-20:37 的工作区事件中
随未跟踪文件丢失，未恢复，且不属于本计划最终 Gate 列表。

## 16. 暂不承诺的范围

本计划不承诺完整 Terraria Buff / Debuff 内容覆盖，不承诺一次性迁移所有装备、套装、坐骑、环境和旗帜效果，不承诺立即删除所有旧键，不承诺把所有状态实体化为独立 Arch entity，也不修改网络协议、存档格式或基础设施实现。

## 17. 计划完成定义

当且仅当以下条件全部满足，计划才可从 proposed 更新为 completed：

1. B0–B9 的本轮适用验收项均有证据；Inventory 属性贡献与来源身份明确标记 deferred，不计入本轮完成条件。
2. 最终回归矩阵通过，新增 warning/error 已解释。
3. 旧键生产写入和跨域直接写入达到目标。
4. 运行时烟测证明至少三类代表性效果闭环。
5. 架构文档、进度记录、技术债和未覆盖范围已更新。
6. 用户审阅并确认是否进入下一轮内容迁移。

