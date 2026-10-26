# Liquid 组件

液体查询输出和液体规则输入分开保存：

- `LiquidContactComponent` 是空间系统提交的本帧接触事实；
- `LiquidInteractionCapabilityComponent` 表示实体是否受某类液体影响；
- `LiquidContactFlags` 用位标记表示接触类别。

玩家、NPC 和 Projectile 可以共享接触事实格式，但每个领域系统仍可按自己的能力组件
解释水、蜂蜜、岩浆和微光的后续效果。不要用一个公共 `Wet` 布尔值替代这些规则。
