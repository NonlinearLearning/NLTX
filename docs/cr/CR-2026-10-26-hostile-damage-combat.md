# CR-2026-10-26：实现通用敌对伤害 Combat 系统

提出人：用户  
渠道：对话  
提出时间：2026-10-26（Asia/Shanghai）  
变更级别：重大  
状态：accepted；用户明确要求执行既有设计计划

## 需求原文

> 执行设计计划,实现代码

对应设计计划：

- [通用敌对伤害 Combat 系统设计与执行计划](../plans/2026-10-26-hostile-damage-combat-system-plan.md)

## 范围

将当前 Projectile 专用伤害链逐步收敛为 Player、NPC、Projectile 可复用的本地 Combat 伤害
入口，包含伤害类型、敌我关系、通用 DamageRequest、生命提交和 DamageAppliedReceipt。

## 明确排除

- 网络、客户端表现、客户端预测和出站复制；
- `src/NSSLC.Infrastructure/`；
- 恢复已删除的客户端组件、聚合实体或旧数组架构；
- 在本变更中完整实现连续 Projectile Sweep、复杂元素抗性、护盾、反射和 Boss 特殊规则。

## 影响评估

- 组件/API：新增共享 Combat 值类型，迁移 Projectile 伤害载荷和命中回执。
- 系统边界：`DamageAdmissionSystem`、`DamageMitigationSystem`、`DamageCommitSystem` 从
  Projectile 专用输入转为通用请求；新增敌我关系和请求协调边界。
- 生命写入：保持 `DamageCommitSystem` 为伤害路径唯一常规 `LifeComponent` 写入者；既有复活
  系统仍保留独立职责。
- 验证：每批增量构建、Combat 冒烟、Projectile 回归和静态 owner 扫描。
- 风险：旧 Projectile API 迁移、同 Tick 请求竞争、数值公式尚未完整覆盖高级规则。

## 排期与决策

用户的“执行设计计划，实现代码”作为本次重大变更的实施授权。实现按计划的 Phase 0–6
分批执行；未决定的高级伤害规则不通过隐式默认值扩展范围，而记录为后续技术债。
