# CR-2026-10-07：网络消息子分支集成

状态：scheduled / in_progress
提出人：用户
提出时间：2026-10-07（Asia/Shanghai）
变更等级：major
批准依据：用户明确要求执行消息子分支合并

## 用户原始要求

> 将这些消息子分支合并为一个分支然后再合并回主分支并处理冲突,注意ai系统也存在很多分支不要合并错了,

## 目标范围

只允许纳入以下 10 个分支：

- `codex/network-message-01-session`
- `codex/network-message-02-world-stream`
- `codex/network-message-03-player-state`
- `codex/network-message-04-entity-lifecycle`
- `codex/network-message-05-world-modification`
- `codex/network-message-06-inventory-commerce`
- `codex/network-message-07-combat-results`
- `codex/network-message-08-npc-world-events`
- `codex/network-message-09-ui-social`
- `codex/network-message-10-netmodules-legacy`

明确排除所有 `codex/npc-ai-*`、`codex/npc-ai-style-*` 及其他 AI 系统分支。主工作树已有的 AI 系统未提交文件也不得作为消息分支内容提交或合并。

## 预期流程

1. 恢复或获得 10 个消息分支的真实提交/快照。
2. 从当前主分支基线创建 `codex/network-message-integration`。
3. 仅按上述 10 个消息分支的提交合并到集成分支。
4. 逐文件处理冲突，优先保留消息 owner/方向/安全门，同时保留主分支中明确属于 AI 系统的变更。
5. 运行合并后允许的受影响项目构建和约 10% 核心 focused smoke。
6. 经主会话验收后，再将集成分支合并回 `main`。

## 预合并审计结果

截至本记录生成时：

- 10 个消息分支全部指向共同提交 `03ff677170879608b8f4771033989beb12400995`。
- 10 个消息分支没有可见的类别专属提交，不能通过 `git merge` 取得先前执行结果。
- 原执行 worktree 已经不存在。
- `mcp__codex_app__list_artifacts` 返回空列表，没有可由 Codex 恢复的 archived worktree。
- `git reflog --all` 只发现消息分支创建记录，没有消息类别提交或合并记录。
- `git fsck` 发现的 unreachable commit 仅是旧主项目 stash/index 或历史 prototype，不匹配本次 10 个消息类别。
- 当前主工作树包含大量 AI 系统和其他用户未提交变更；不能将其整体提交到消息集成分支。

## 影响评估

| 项目 | 结论 |
|---|---|
| 数据库 | 不涉及 |
| 代码合并 | 需要，但当前缺少可合并的消息提交 |
| 冲突风险 | 高；Program.cs、网络注册、方向 profile、验证入口和 ECS/NPC 文件存在共享修改 |
| AI 系统影响 | 必须排除；当前主工作树存在大量 NpcAi* 和 AI 文档/测试未提交文件 |
| 测试/构建 | 恢复后仍受约 10% 核心测试上限约束 |
| 当前决策 | 采用重新派发；不创建空合并，不提交主分支，不把 AI 文件纳入消息提交 |

## 2026-10-07 重新派发决策

用户列出的四种恢复路径中，当前机器无法提供 1、2、3：原 worktree、消息专属 commit 和消息 patch 均已丢失，只有 10 个共同基线分支引用仍存在。采用第 4 项重新派发：

- 保留旧基线 SHA `03ff677170879608b8f4771033989beb12400995` 作为审计依据，不将其误报为实现提交。
- 将 10 个无独有提交的消息分支推进到当前 `main` 已提交基线 `8e985b74a9bae603145c3a5f73f1d0e389e02056`；主工作树未提交的 AI 文件不进入分支。
- 每个子代理必须在自己的消息分支上形成真实 commit，并在最终报告中提供 commit SHA、文件清单、构建/测试命令和 exit code。
- 只有 10 个消息分支真实提交后，才创建 `codex/network-message-integration`，按类别逐个合并并处理冲突。

## 解除阻塞条件

重新派发完成后，必须满足：

1. 10 个消息分支各自存在不等于共同基线的真实 commit。
2. 每个 commit 只能包含本类别消息实现、测试和必要证据，不得包含 `NpcAi*` 或其他 AI 分支内容。
3. 每个 commit 都有独立的约 10% focused smoke 和受影响项目构建记录。
4. 集成分支完成冲突核对后，才允许合并回 `main`。

