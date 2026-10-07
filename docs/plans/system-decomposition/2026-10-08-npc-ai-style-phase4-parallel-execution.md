# NPC AI `aiStyle 0–127` Phase 4 并行执行文档

日期：2026-10-08  
上游设计：[NPC AI 系统设计](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md)  
上游执行计划：[NPC AI 系统执行计划](2026-10-06-npc-ai-system-execution-plan.md)  
上一阶段验收：[Phase 3 并行验收](2026-10-07-npc-ai-style-parallel-acceptance.md)  
状态：active / all-category parallel continuation  
执行方式：复用现有 7 个类别会话和统一前缀分支；主会话负责独立验收，不串行等待类别完成

## 1. Phase 4 目标

Phase 3 已交付 7 个类别的窄 profile、owner seam、生命周期契约和 client negative-path 证据，
但 coverage 仍为 `124 indexed / 4 mapped / 0 implemented / 0 verified`。Phase 4 不得把这些
局部 seam 当成完整迁移，而是继续沿来源证据向完整 `aiStyle 0–127` 推进：

```text
source NPC identity
  -> source behavior branch / helper closure
  -> profile input and result
  -> server-only owner commit
  -> host dispatcher / spawn / update seam
  -> network/save/load/unload/deletion boundary
  -> source differential or explicit evidence-gap
  -> independently reviewable Markdown checkpoint
```

本阶段每个类别至少要把一组尚未闭合的 style 从 `indexed/open` 推进到有精确 identity、来源锚点、
行为模块和 host 接线合同的 `mapped/open`；只有实际 server owner、生命周期闭包和允许范围内的
行为验证全部存在时，主会话才考虑 `implemented` 或 `verified`。子会话不得修改 central coverage
ledger 的 promotion 字段。

## 2. 不可变硬约束

1. 七个类别必须同一阶段并行推进，不能变成“一个类别完成后再进入下一个类别”。继续使用原有
   类别会话、原有 worktree 和 `codex/npc-ai-style-` 分支前缀，不创建同类别重复分支。
2. 每个子会话继续以 `/goal` 管理本类别长线目标，并完整读取/遵守 `pua` skill。工具失败、路径
   猜测、脚本错误和证据不足必须记录失败计数、已排除假设和下一条路径，不得静默吞掉。
3. `netMode=0/2` 才能执行目标选择、随机、AI/profile、任务、spawn、transform、reward、movement、
   physics、collision、contact damage、关系修改以及 network/save/load/unload/delete owner。
   `netMode=1` 只能消费复制状态、网络同步意图和呈现数据。
4. 不得编写客户端运行的 NPC AI。client 不得调用目标选择、随机端口、profile decision、
   `_aiSystem.Evaluate`、movement/physics/collision、natural despawn、task commit 或 authority spawn。
   发现 client side effect 时，停止 promotion，记录为 `open`，并优先修复唯一共享入口的 gate。
5. 子会话不得运行 solution/full build、restore、TargetHost 程序、完整测试全集或其他运行型验证。
   主会话唯一允许的增量构建仍是：

   ```powershell
   dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
   ```

   只执行约 10% 的核心验证；源级检查必须记录实际命令和 exit code，不能把“文件存在”写成行为通过。
6. 不使用 `git reset --hard`、`git clean` 或广泛删除。类别回退必须是文件级、可恢复，并且不能回退
   主树既有 `NpcAiAuthorityGate`、TargetHost client/server 断言或其他类别改动。

## 3. 七类并行工作包

所有工作包在同一时间窗口启动；类别之间不互相等待，也不共享未审查的代码文件。

### 3.1 ground-jump（24 styles）

范围：`1,3,8,10,13,19,20,22,23,25,26,39,40,41,50,56,66,67,98,100,101,102,103,107`。

已有窄切片：Wolf `155/155/26`。Phase 4 必须并行完成：

- 为尚未闭合的 style 逐条核对 `type/netId/aiStyle`、来源 `NPCID.cs`/`NPC.cs` 锚点和专属 helper；
- 继续禁止用 generic `aiStyle` fallback 代替 type-specific 分支；
- 核对 `ActiveNpcTickPhase`、natural spawn、contact damage、Store direct entry 的同一 authority gate；
- 将 jump/ground intent 与唯一 movement owner 的提交点分开；不得在 profile 内保存第二份跨 tick 状态；
- 对每个新 identity 给出 registry/catalog 可达性、client early return、死亡/卸载/删除和回退合同；
- 不能证明来源闭包的 style 保持 `indexed/open`，不得为了数字好看提升 coverage。

### 3.2 air-water（26 styles）

范围：`2,5,9,14,16,17,18,21,24,44,49,63,70,72,74,80,85,86,91,95,96,99,108,113,118,119`。

已有窄切片：Eater `6/6/5`。Phase 4 必须并行完成：

- 逐条补来源 identity、目标/转向/湿态/碰撞/随机 helper 证据；不得把 type 6 不可达 catalog 当成运行闭环；
- 继续保留 `oldVelocity`、`collideX/collideY`、速度下限和反弹来源顺序；
- WaterStrider 只能在真实水线 Query 可用时接入，Query 失败必须返回 unknown，不填默认水线；
- 明确 profile → velocity/AI state → network intent → physics/collision 的 server-only 顺序；
- client 只观察复制速度/意图，不预测水上移动、跳跃或碰撞。

### 3.3 town-rescue（6 styles）

范围：`0,7,42,124,125,127`。

已有窄切片：Guide `22/22/7` 回家 task owner。Phase 4 必须并行完成：

- 为 style 0、42、124、125、127 找到真实来源 identity 和城镇/救援/变换分支；
- 保留 task reference、generation、完成/失败/中断、过期引用和 cleanup 顺序；
- Guide source profile 不能因为 local catalog 的 `aiStyle=0` 而被伪装成来源 style 7；
- server-only 执行传送/变换/奖励，client 只消费复制结果和同步意图；
- housing、town history、save/load/unload 与 NPC 实例生命周期的 owner 必须分别登记。

### 3.4 bio-environment（7 styles）

范围：`64,65,68,112,114,115,116`。

已有窄切片：WaterStrider `612/612/116` 与 `613/613/116`。Phase 4 必须并行完成：

- 不合并 612 与 613 的 identity；为其余六个 style 找到实际 environment query 和来源分支；
- 将液体/湿态/水线 Query、随机 port、profile decision、velocity/AI state 和 network intent 标成逐项读写集；
- random seed/消费位置必须仅在 server/single-player 发生，client 不消费随机；
- 环境事实无来源时写 `unknown/open`，不得用常数或 catalog 默认值伪造水线；
- 增加环境缓存失效、world unload、自然 despawn、删除和复制边界说明。

### 3.5 single-boss-special（19 styles）

范围：`4,15,43,45,51,54,57,58,60,61,69,93,97,110,117,120,121,122,123`。

已有窄切片：Eye `4/4/4` Servant spawn claim。Phase 4 必须并行完成：

- 逐条核对阶段、阈值、目标、随机、变身、Servant/攻击者和网络效果的来源顺序；
- 证明同 tick/re-entry 幂等、阶段转换、死亡、Reset、Dispose、unload 和 Servant cleanup；
- 所有 boss phase、target、random 和 effect commit 必须在 authority gate 后；
- client 不预测 Boss 阶段、冲刺、仆从、变身或奖励，只观察复制状态；
- 已存在 Servant 与 parent death 的联动语义若未有来源证据，必须保持 open，不自行补造。

### 3.6 segmented-multipart（33 styles）

范围：`6,11,12,27,28,29,30,31,32,33,34,35,36,37,46,47,48,52,53,55,59,71,75,76,77,78,79,81,82,84,88,89,90`。

已有窄切片：Wyvern style 6 的 15 节 projection/lifecycle seam。Phase 4 必须并行完成：

- 逐条登记 root/segment type、netId、aiStyle、`realLife`、`ai[0]/ai[1]/ai[3]` 和邻接关系；
- 将 `SpawnNow → Attach → immediate read → shared-life/update → Detach/Release` 映射到真实 owner；
- 继续证明容量失败逆序回滚、根死亡整链释放、中段 detach、stale reference 和 slot generation；
- 明确默认 catalog、dispatcher、自然 spawn、正常 tick、网络 serializer、save/load/hydrate 和 unload seam；
- client 只能观察已复制链，不能修改关系、预测段位置或执行释放。

### 3.7 encounter-event（13 styles）

范围：`38,62,73,83,87,92,94,104,105,106,109,111,126`。

已有窄切片：TargetDummy 来源 `488/488/92` admission/lifecycle seam。Phase 4 必须并行完成：

- 保持本地 `488/488/0` 与来源 `488/488/92` mismatch，除非有独立来源证据支持 catalog 修复；
- 逐条核对 event/wave/crystal/portal/NPC/TileEntity 的 admission、幂等、重复请求、部分失败、journal/outbox 和 cleanup；
- 事件 owner、NPC owner、TileEntity owner、network owner 必须分开记录最终写入者；
- client 不提交 wave/reward/event state，不创建 TileEntity/NPC，只消费复制状态和同步意图；
- 不把 event/wave/reward 算法塞进通用 NPC profile。

## 4. 每个类别的 Phase 4 Markdown 交付契约

每个类别必须在唯一 Markdown 中追加 Phase 4 checkpoint，至少包含：

1. 本阶段实际推进的 style 集合、精确 identity、来源文件和行号/指纹；
2. profile input/state/result、Query/Command 读写集、最终 owner 和 API 组合；
3. server-only 更新顺序、提交点、错误/重试/幂等和 client negative path；
4. 真实 host call site 或明确的 integration seam；若没有，写 `open` 和下一步所需输入；
5. network/save/load/unload/despawn/deletion/transform/reward/task 生命周期状态；
6. 每条 focused source check 的实际命令、exit code、报告路径和证据等级；
7. build/restore/run/test 是否执行；未执行必须写 `not-run`，不能写“通过”；
8. 精确文件级 rollback；不能回退主树 authority gate 或其他类别；
9. PUA 失败计数、七项自检和下一阶段建议。

## 5. 主会话并行验收策略

类别会话运行期间，主会话只做不修改类别 worktree 的只读检查：

- 重新计算 128 分类的互斥/完整性；
- 检查每个类别的 `CanHandle` 三元 identity predicate 是否精确；
- 检查 client 是否在 owner、AI、目标、随机、物理、生成、任务和生命周期写入前返回；
- 检查类别改动没有修改 TargetHost、central ledger 或其他类别文件；
- 检查 Markdown 是否记录真实 exit code、open gaps 和 rollback；
- 所有类别 Phase 4 checkpoint 收齐后，主会话才可再次执行唯一 TargetHost 增量构建；
- 主会话不运行 TargetHost 程序，不运行全量测试，不把源级审计提升为 runtime verified。

## 6. 完成门禁

Phase 4 仍不自动关闭完整 goal。只有同时满足以下条件，主会话才可进入 completion audit：

- 128 个 style 全部具有唯一来源 identity、来源分支/辅助方法和 profile/handler 映射；
- 每个被提升的 profile 都有真实 server owner、状态提交、效果/网络/生命周期闭包；
- client 全部只观察复制状态，且没有 client AI、随机、目标、物理或 authority side effect；
- source differential、network、save/load、unload、natural despawn、deletion、transform、reward、关系和任务门禁均有相称证据；
- coverage ledger 的 `implemented/verified` 与实际 evidence 一致，不能只靠静态映射晋升；
- 唯一 TargetHost 增量构建通过，且 client/server authority 断言仍存在；
- 所有类别均有可执行文件级回退，并且回退不会影响其它类别或主树已有修改。

本阶段完成后，若仍有任一条件缺失，目标继续保持 `active`，并在下一阶段继续并行推进，不得以“七类文档已完成”替代完整 NPC AI 迁移。
