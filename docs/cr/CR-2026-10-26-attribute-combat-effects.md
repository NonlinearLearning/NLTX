# CR-2026-10-26：将高级伤害效果统一到 Attributes 计算

提出人：用户  
渠道：对话  
提出时间：2026-10-26（Asia/Shanghai）  
变更级别：重大  
状态：implemented

## 需求原文

> 元素抗性、护盾、反射、吸血、随机暴击全部移动到属性系统中进行计算

## 决策

`AttributeSetComponent` 是 Player、NPC、Projectile 共用的数值来源。新增元素、暴击、护盾、
反射和吸血属性键，并由 `AttributeCombatCalculator` 执行无副作用计算。Combat 仍负责请求
排序、准入和目标生命提交；护盾扣减由 `AttributeSystem.ConsumeShield` 在同一次提交边界执行。

随机暴击使用显式 `CriticalRollInput`，不读取全局随机源；同一请求重试得到同一结果。反射和
吸血只计算实际生命伤害对应的结果值，由后续效果 owner 消费，不在伤害提交系统中直接写来源
生命或递归生成请求。

## 规则

```text
EffectiveResistance = clamp(TargetResistance - SourceElementalPenetration
                             - Payload.ElementalPenetration, 0, 1)
ShieldAbsorbed = min(Target.Shield, DamageAfterReduction)
LifeDamage = DamageAfterReduction - ShieldAbsorbed
ReflectedDamage = floor(LifeDamage × Target.ReflectRatio)
LifeSteal = floor(LifeDamage × Source.LifeStealRatio)
```

`True` 伤害跳过防御、元素抗性和最终减伤，但仍被当前护盾吸收。护盾完全吸收时仍生成成功
回执，`LifeDamage` 为零而 `ShieldAbsorbedDamage` 大于零。反射默认不再次反射，反射和吸血的
实际二级效果系统不在本次修改中直接写入。

## 影响与验证

- 共享 `DamagePayload` 增加元素、元素穿透和显式暴击随机输入。
- `DamageMitigationResult`/`DamageAppliedReceipt` 暴露各阶段伤害、护盾、反射和吸血数值。
- 没有新增 Player/NPC/Projectile 专属防御或效果组件，也没有引入客户端或网络代码。
- `Terraria.Components`、Combat smoke、Buff smoke 均按仓库构建约束增量验证；输出位于
  `Build/bin/`。
