# ArmorSetBonuses 资格审计执行提案

## 目标

为 M-001 的 `ArmorSetBonuses.Initialize/BuildLookup` 建立可复核的资格结论，判断它是否
可以作为独立 initializer responsibility family。该提案只推进审计和阻塞依据，不实现
armor-set gameplay，也不声称 M-001 完成。

## 执行步骤

1. 读取 Version4 `ArmorSetBonuses.cs`、`ArmorSetBonus.cs` 及 Main/Player 调用点，固定
   source hash、line scope 和注册/索引/查询/效果调用关系。
2. 检查唯一 Simulation owner、typed/replayable contract、persistence 和 protocol 边界。
3. 将结果分类为 `accepted`、`candidate` 或 `unknown/deferred`；禁止以 item triples
   代替 armor-set effect authority。
4. 若资格不足，写入明确 prerequisites，不添加 registry、`Queue<Action>` 或 aggregate
   initializer。
5. 运行约 40% 关键验证：MainBoundary、一个直接受影响的 Items verifier（若可用）和
   scoped `git diff --check`；不运行 full regression。

## 接受标准

- source path/hash/line scope 与结论一致；
- `Initialize` 的 effect delegates、`BuildLookup` 的 index semantics、`GetCompleteSet` 的
  Player dispatch 均有证据；
- `unknown/deferred` 不被描述为完成；
- 没有新增大 initializer 或未定义 caller/effect contract；
- MainBoundary 通过且 scoped diff check 为零。

## 下一步

优先建立 typed/replayable player capability/effect contract；在该 contract 之前，不推进
armor-set definition child。其它未完成的 entity/event lifecycle、NPC/projectile/item
tables、random starts 和 client/presentation 分支继续按主提案拆卡。
