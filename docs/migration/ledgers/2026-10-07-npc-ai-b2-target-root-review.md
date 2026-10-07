# NPC AI B2：目标选择的来源主验收

日期：2026-10-07（Asia/Shanghai）  
状态：真实来源反例 reproduced；生产目标选择修复与宿主验收 pending  
计划：[NPC AI 执行文档](../../plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md)

## 普通/WOF 距离反例

固定只读源码的 `NPC.cs:78861,78871,78878` 保留以下调用点表达式：

```text
abs(player.position.X + player.width / 2 - npc.position.X + npc.width / 2)
+ abs(player.position.Y + player.height / 2 - npc.position.Y + npc.height / 2)
```

半宽/半高先做整数除法；NPC 项的最后一项是加法。不能以一般中心点曼哈顿距离替代。
宠物评分也保留同样的源码表达式。是否符合常识不是行为迁移的验收依据。

反例设置 NPC `(100,100)`、尺寸 `24x18`；玩家 0 在 `(80,100)`，玩家 1 在 `(120,100)`，
二者尺寸 `20x40`，active/live/non-ghost、aggro 0、gross true、无 noAggro/宠物。
NPC 当前方向和旧方向为 `(1,1)`，current/old target 为 255，无混乱、Boss 或碰撞。

源码评分为玩家 0 的 31、玩家 1 的 71；当前中心点计算得到 33/29，因此会偏向不同玩家。
主会话随后直接执行来源方法确认选择结果，而不是用上述公式充当来源 oracle。

| 来源运行 | `TargetClosest` | `TargetClosest_WOF` | 方向 | netUpdate |
| --- | --- | --- | --- | --- |
| 原只读 `bin/Debug/net40/TerrariaServer.exe` | 玩家 0 | 玩家 0 | `(-1,1)` | true |
| 既有隔离固定源码 build | 玩家 0 | 玩家 0 | `(-1,1)` | true |

两条运行 exit 0；两个程序集运行前后不变。`TargetClosest`、`TargetClosest_WOF`、
`TryTrackingTarget` 和 `SetTargetTrackingValues` 四个方法的 IL SHA-256 分别在两边一致。
该身份关联只覆盖已比较方法与实际程序集，不扩展为完整 source parity。

## 执行证据

主会话生成 `Build/generated/NpcAiTargetSourceReview/TargetSourceProbe.cs`，复用既有
ReferenceHarness 的反射初始化、CallTracker timer 禁用与 sandbox。setup 使用零 AI 场景，
只为建立运行时；随后构造两个玩家并直接调用来源 TargetClosest/WOF。零 AI setup 不是
来源门禁的有效行为样本，也不被算作任何 AI coverage。

该 probe 不复制目标算法。它使用 .NET Framework 4 x86 编译器，兼容既有 net40 来源
程序集；生产 NPC 和普通 verifier 仍保持仓库 net10.0 目标。精确编译参数在
`compile-retry-args.json`，结果 exit 0、0 warnings / 0 errors，产物为
`Build/bin/NpcAiTargetSourceReview/TargetSourceProbe.exe`。首次相对路径编译失败的
`compile.log` 和 exit-code 保留；重试只改为绝对 Windows 路径。

证据目录：`Build/diagnostics/NpcAiRedesign/runs/b2-source-target-root-review-20261007/`。
包含两个实际调用的 args、log、exit-code、原报告、sandbox setup capture 和
`root-source-results.json`。原来源程序集 SHA-256 为
`CF30EBDA6839E9FF9556F3BA6CD84B18385CD3283E5462745BAA57D1548B669A`；
隔离来源 build SHA-256 为
`146D0FB79404BE1954D18ACB5FDC6BFB936D35BB4487DB79A6B02236966E1C1B`。

## B2 修复的必需复验

B2 独立会话拥有 target API 与适配器，已经收到来源报告。它须实际调用生产 Select 复现
旧结果，再完成普通/WOF 字面评分、整数半宽/目标矩形截断及宿主提交的修复；修复后的
domain/host 输出与本来源结果比较。来源 probe 通过不等于生产实现已经修复。

另外两项源码核对结果也需真实复验：普通/WOF 无候选仍调用 SetTargetTrackingValues，
保留有效 current target 或把越界 target 改 0，再取该玩家几何；Upgraded 的 NPC 目标使用
`WhoAmIToTargetingIndex = whoAmI + 300`（`NPC.cs:6735`），不能直接使用裸 NPC slot。
这些边界未在本次两玩家 probe 中执行，保持 pending。

不会因本反例、局部修复或 source helper IL 匹配提升任何 profile 的 coverage stage。
完整候选事实、宠物 CanHit、实时几何、authority/network 与 source/host 生命周期仍按 B2
长线任务独立验收。回退只撤销本反例的验收引用；不删除来源失败记录和并行 owner 改动。
