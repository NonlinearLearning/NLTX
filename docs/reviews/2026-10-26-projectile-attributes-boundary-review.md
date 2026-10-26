# Projectile Attributes 命中边界验收记录

日期：2026-10-26  
变更记录：[CR-2026-10-26-projectile-attributes-boundary](../cr/CR-2026-10-26-projectile-attributes-boundary.md)

## DoD

| 项目 | 结果 | 证据 |
| --- | --- | --- |
| 功能完成 | ✅ | Projectile 实例复制 Attributes；穿透和免疫数值由 Attributes 计算；Projectile 保存 accepted hit、免疫 list 和终止请求 |
| 变更针对性测试 | ✅ | Smoke 覆盖多目标免疫、同目标刷新、generation 隔离、属性修饰、命中计数、穿透阈值和保护窗口拒绝 |
| 核心回归 | ✅ | `Terraria.Components.csproj` 构建通过；Buff/NPC 既有冒烟路径通过 |
| 文档同步 | ✅ | Projectile ECS 设计、Projectile 实现说明、Projectile/Attributes README 已同步 |
| 变更记录 | ✅ | CR、迭代任务、本文验收记录已落盘 |

## 验证

项目构建：

```text
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
```

结果：退出码 `0`，`0` 个警告，`0` 个错误。输出：
`src/NSSLC/Build/bin/Debug/net10.0/Terraria.Components.dll`

最小集成冒烟：

```text
dotnet run --project .agent-workplace/buff-smoke/Smoke.csproj --no-restore
```

结果：退出码 `0`，输出：

```text
Buff/NPC smoke verification passed.
Projectile ECS smoke verification passed.
```

代码卫生检查：`git diff --check` 通过。旧 `RemainingHits`、`IsUnlimited`、`Consume()` 和
Combat 系统级 `immunityTicks` API 未在本次相关代码中残留。

## 验收边界

本记录覆盖组件、命中提交、生命周期终止和最小 Arch/Combat 集成路径。生产宿主的完整 Tick
注册、真实 Tile sweep、AI 宿主绑定、NPC/Player 完整生命周期和旧客户端行为等价仍需后续
宿主级测试；网络、客户端和存档接入不属于本变更。

## 放量决策

本轮只完成服务器领域组件与系统的本地实现和验证，没有部署或发布动作，因此不执行灰度放量。
