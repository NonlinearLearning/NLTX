# NPC AI 来源门禁：主会话反向验收

日期：2026-10-07（Asia/Shanghai）  
状态：门禁退出语义验收 rejected，等待修复后复验；来源 profile parity 仍 failed  
关联：[来源差分 owner 检查点](2026-10-07-npc-ai-reference-differential-checkpoint.md)

主会话复核了固定来源的 32 场景/182 tick capture、8 个 Slime 状态差异和 11 个 observable
差异，以及正常 Windows PowerShell `-CompareOnly` 返回 exit 2 的结果。源码/二进制关联、
采样状态和未闭合效果的证据仍保留。这些事实不等于门禁的所有拒绝条件都经过验收。

## 独立反向夹具

先从真实 capture 批次 `Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/`
抽出原本无 profile 差异的 24 个 Eye 场景/36 tick，并保存抽取后的 request/original/source。
随后只修改 source 首 tick 的 `rotationBits` 为 1。另设 request/original/source 三份
`cases=[]` 的空夹具。所有夹具在独立 diagnostics 路径，不修改原 capture 或只读来源。

| 夹具 | profile 差异 | source-to-source 差异 | 场景 / tick | 比较器退出码 | 结论 |
| --- | ---: | ---: | --- | ---: | --- |
| Eye control | 0 | 0 | 24 / 36 | 0 | 零差异控制样本成立 |
| Eye source rotation mutation | 0 | 1 | 24 / 36 | 0 | 已记录 `sampledReferenceOutputsMatch=false`，顶层仍标 no-difference 并成功退出 |
| Empty request/captures | 0 | 0 | 0 / 0 | 0 | 没有任何样本仍成功退出 |

执行的是已构建 `Build/bin/Terraria.NpcAi.ReferenceVerification/Debug/net10.0/` 中的比较器，
没有重新构建来源树。该 verifier 程序集 SHA-256 为
`D94B1E3D18AD8940B2F55BA340E2C51E76F106133FA22592D67944DB1D17FEE0`。
每条精确参数、报告和日志位于
`Build/diagnostics/NpcAiRedesign/runs/source-gate-negative-root-review-20261007/`，
汇总为 `root-negative-review.json`。

owner 较早的 Slime rotation mutation 夹具本来就有其他 profile 差异，返回 exit 2
不能单独证明 source mismatch 会触发非零退出。它证明差异记录存在，但退出码受既有
失败项混淆。本次 Eye control 排除了这项混淆。

## 修复与重验条件

修复由来源会话独占 ReferenceVerification 文件，主会话独立复验：

- request/captures 至少有一个有效场景和实际 tick；tick 编号和期望序列必须有效。
- 原 EXE 与隔离源码 build 的相关 AI IL 都相同、相同输入却出现来源输出矛盾时，
  门禁必须非零；顶层结果必须区分来源可信性与 profile 差异。
- 若保留原二进制 IL 不关联时的 pinned-source fallback，明确它的来源证据要求；
  它不能掩盖相同 IL、相同输入的矛盾输出。
- 用零差异 Eye control、Eye mutation、empty 和编号异常夹具检查 direct comparator
  及 PowerShell `-CompareOnly`，确保没有其他失败项替代被检查条件。
- verifier 实际执行程序集与 build-source 指纹要有明确关联。运行时读取 current source
  得到的哈希只能称为运行时文件指纹，不能自动证明 linked binary 编译自这些内容。
- 修复后重跑真实 32/182 comparison；cooldown/target-helper 差异须按生产修复和组合
  验收继续闭合，不能删除、豁免或重命名后算通过。

本记录不改 coverage stage，也不推翻已观察的来源文件指纹与真实 capture。它拒绝的是
当前门禁实现的完成声明。修复前保留失败的所有输出；修复后的新证据在本记录续写，
不能覆盖历史复现。全量 source/authority/persistence/unload/deletion 门禁仍 open。
