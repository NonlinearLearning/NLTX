# 非权威细分子系统同级拆分实施计划

> **For Codex:** 按本计划修改生成器和校验器，重新生成清单并完成验证。

**目标：** 固定上一轮 239 个细分子系统的前 50 个排行榜基线，将每个基线拆为至少两个可追溯的同级导航组，同时保持成员事实和父级统计不变。

**架构：** 使用生成器中的确定性映射作为唯一分组来源；报告只保存输入成员的投影。校验器
同时验证成员闭合、父级/细分摘要和来源追溯。

**技术栈：** PowerShell、Markdown 成员清单、SHA-256 文件追溯。

---

### Task 1: 更新细分映射和定义

**Files:**
- Modify: `Build/Tools/Generate-Version4NonAuthoritativeFineSubsystemReport.ps1`

按设计文档增加 `Main.cs` 语义行段和高密度共享能力的映射；为每个新组登记角色、seam
和职责，保留未命中时的异常；保留基线组到最终 peer 组的反向归属。

### Task 2: 强化摘要统计校验

**Files:**
- Modify: `Build/Tools/Test-Version4NonAuthoritativeFineSubsystemReport.ps1`

解析报告的父级统计、最终细分统计和基线聚合统计，与逐成员重新计算的结果逐项比较，覆盖有记录和零记录父级；确认 50 个基线各有至少两个 peer。

### Task 3: 重新生成报告

**Files:**
- Modify: `docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

运行生成器，确认细分数量增加但父级和成员总量不变。

### Task 4: 运行验证

运行生成器、完整校验器和 focused split verifier，另外执行独立摘要交叉检查；记录命令、退出码和关键统计（最终 289 组，4,017 字段，525 属性）。
