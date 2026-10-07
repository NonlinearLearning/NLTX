# 类别 07：战斗、伤害、死亡与实体结果消息执行文档

文档 ID：NETMSG-CAT-07
执行模型：gpt-6-luna，reasoning=max
分支：codex/network-message-07-combat-results
会话：network-message-07-combat-results
类别 owner：伤害请求、结算结果、死亡结果和表现投影

## 1. 目标

完成 12 个战斗/伤害/死亡/结果消息的方向准入和权威 projection。客户端报告永远不是
可信伤害结算；DamageRequestedEvent、DamageResolution、ApplyDamageCommand、Health/
Death owner 才是权威链。表现消息只播放结果，不能反向触发伤害。

## 2. 消息范围

24 UnusedMeleeStrike；28 DamageNPC；57 Unknown57；81 CombatTextInt；
97 AchievementMessageNPCKilled；98 AchievementMessageEventHappened；103 MoonlordHorror；
106 PoofOfSmoke；112 SpecialFX；119 CombatTextString；132 PlayLegacySound；
153 NPCDebuffDamage。

## 3. 基线与重叠处理

27/29 Projectile 由第 4 类负责，117/118/135 Player 结果由第 3 类负责；本类不得重复
注册这些 ID。132 在第 9 类也有表现语义，本类提供 CombatResult/Presentation 端口，
由单一注册 owner 负责实际 gateway key，必须在报告中明确最终归属。

24/57 是旧/未知路径，默认兼容读取或拒绝；28 是高风险 C2S/Relay，153/81/97/98/
103/106/112/119/132 是 S2C 结果或表现。不能把 S2C codec 的存在误当 C2S 可用。

## 4. 实施步骤

1. 建立 12 项 ID/方向/阶段/权限登记，核对已有 Combat/Projectile/NPC/Player owner。
2. 对 28 仅接收最小可信攻击声明，重算命中、目标、免疫、伤害、击退和来源；不允许
   客户端直接提交生命值、击杀或奖励。
3. 将 153 作为 Buff/Damage owner 已提交的 DoT 结果；81/119 作为数值/文本展示；
   97/98 作为成就/事件结果；103/106/112/132 作为视觉/音效结果。
4. 对结果快照使用 detached immutable payload，输出代次/目标复验，发送失败不重执行战斗。
5. 对 24/57 记录命中事实、字段和调用者，保留兼容隔离，不新建 ECS 入口。
6. 处理重复伤害包、旧实体引用、死亡后迟到消息和伪造受害者。

## 5. 非目标

不实现完整碰撞、NPC AI、Projectile physics、成就系统和客户端特效；不把 CombatText/
Sound/SpecialFX 当作规则触发；不做长时 DPS 或全量真实客户端战斗。

## 6. 10% 核心测试

只执行一组最小战斗链：

1. 一个 DamageNPC 请求的合法命中、伪造目标/伤害被拒绝，随后生成一个权威伤害或死亡
   projection；再验证重复/死亡后请求不重复结算。
2. 一个 NPCDebuffDamage 或 CombatText 结果只能由 owner 发布，客户端反向输入被拒绝。

优先使用 Test/Terraria.NpcDamageCombatVerification、ProjectileCombatVerification 或
现有 Network Verification 中的 focused combat case。只跑选定 case，构建受影响项目；
不要跑完整 solution 和全量网络验证。

## 7. 验收

必须证明 28 不直接写 Health/Death，结果消息方向受到保护，12 个 ID 的唯一 owner 明确，
并回报命令、exit code、warning/error、Build/bin、失败路径和与第 3/4/8/9 类的重复 key
风险。没有真实 owner 提交前后证据只能 partial。

