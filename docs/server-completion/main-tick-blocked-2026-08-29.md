# Main Tick 阻塞记录 2026-08-29

## 状态

- Flowstate：`N6`
- 决定：`blocked`
- 范围：当前只接受有 source-backed contract 的 server-owned bounded slice。
- 结论：不宣称 full release，也不宣称完整 Terraria parity。

## 阻塞决定

剩余 deferred responsibility 缺少恢复后的源码锚点和可验证契约。当前证据不足以确定
caller identity、执行 phase、取消语义、重启/持久化语义或 client projection owner。继续
添加实现将需要猜测行为，超出本轮计划边界，因此暂停代码实现。

## 已完成的 bounded scope

- `WorldClock.Restore` 在状态变更前拒绝与实例 `TicksPerUpdate` 不一致的快照。
- weather、progression、invasion、meteor、Slime Rain、Lantern Night event fact 的 identity
  和 transition sequence 已修复；无效前导 command 不再污染 sequence。
- `WorldProgressionState.With*` 保留 Lantern Night schedule sequence 和 invasion clear flags。
- Persistence V36 已保存和恢复 chest、liquid、wiring 的 mutation sequence cursor；V35 及更早
  版本默认 cursor 为 `0`，缺失 V36 cursor tail 或越界值会被拒绝。

## 验证证据

最新 root recheck：

`Build/diagnostics/main-tick/current-root-recheck/20260829-170000/`

- `Terraria.Dome.sln` serial Release build：exit `0`
- warnings/errors：`0/0`
- WorldRules focused verifier：exit `0`
- Persistence focused verifier：exit `0`
- MainBoundary verifier：exit `0`
- `git diff --check`：exit `0`

相关专项证据：

- `Build/diagnostics/main-tick/task-5-environment-sequence-filter/20260829-140000/`
- `Build/diagnostics/main-tick/task-6-8-event-sequence-filter/20260829-150000/`
- `Build/diagnostics/main-tick/task-11-mutation-cursors/20260829-160000/`
- `Build/diagnostics/main-tick/task-11-mutation-cursors/20260829-180000/`

这些结果证明已完成范围可重现，不证明延期责任族已经完成。

## 明确延期的责任族

- random trigger 和完整 weather/event parity；
- NPC spawn table、AI family、NPC-driven invasion spawn；
- client UI、chat、sound、ambience 及其他 presentation projection；
- 尚未恢复 source-backed contract 的 static tables 和 arbitrary orchestration；
- 需要完整 legacy identity ordering、restart/persistence 或 network projection 语义的行为。

## 已尝试并排除的路径

- 仅依据现有 Simulation 类型补写缺失行为：缺少 caller、phase 和 projection provenance，不能
  区分合法默认值与错误推测。
- 添加通用 `Action` queue、通用 coroutine storage 或无 contract 的 delayed process：无法建立
  所有权、取消、顺序和持久化边界，已排除。
- 以 focused verifier 通过替代 parity 证明：验证器只覆盖 bounded predicates，不能证明完整
  Terraria 行为，因此不作为解除阻塞的依据。

## 解除条件

重新开启 deferred implementation 前，必须同时满足：

1. 恢复对应 legacy/source anchor，并记录责任族与调用者身份。
2. 明确 `Simulation`、`Server`、protocol projection 和 persistence 的 owner 边界。
3. 定义 typed command/state/fact/projection contract，包括 phase、sequence、cancellation 和
   restart semantics。
4. 为该责任族增加 focused verifier，覆盖有效路径、拒绝路径和持久化/重启路径（适用时）。
5. 使用新鲜 `Build/diagnostics/` 输出重新运行相关 verifier、MainBoundary 和 serial Release
   build，并更新评审与完成清单。

在满足上述条件前，保持 `N6 / blocked`，不扩大实现范围。
