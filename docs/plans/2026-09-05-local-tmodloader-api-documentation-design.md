# 本地 tModLoader API 文档检索实施计划

> **For Codex:** REQUIRED SUB-SKILL: Use writing-plans to implement this plan task-by-task.

**Goal:** 将 NLTX 的 tModLoader API 证据入口迁移到本地离线镜像，并定义可复查的 AI 检索流程。

**Architecture:** `D:\TRbackup\tmodloader-api-docs-stable` 是仓库外、只读的 Doxygen 文档镜像。项目内的公共拆分检索协议定义入口、类型页、成员锚点和证据记录规则；历史报告改为引用同一镜像，并如实记录早期在线 TLS 限制已被本地镜像替代。

**Tech Stack:** Markdown、Doxygen HTML、PowerShell、ripgrep。

---

### Task 1: 定义本地检索协议

**Files:**
- Modify: `.agents/skills/public-decomposition/references/tmodloader-documentation-retrieval.md`

**Step 1:** 将在线 stable 首页替换为 `D:\TRbackup\tmodloader-api-docs-stable\index.html`，并规定 `annotated.html`、`classes.html`、`functions.html` 为离线索引入口。

**Step 2:** 明确 AI 先用 `rg -n` 在索引和确认的类型页中读取实际 `href`；仅在已确认的页面内记录精确成员锚点，不从 C# 类型名推导 Doxygen 文件名。

**Step 3:** 明确 API 镜像只能证明公开签名、摘要和注释；私有实现、调用顺序、持久化与网络语义必须继续回退到本地参考源码。

### Task 2: 更新项目证据引用

**Files:**
- Modify: `docs/世界会话与进度系统组件化拆分报告.md`
- Modify: `docs/世界生成Biome生态与世界会话进度公共拆分审计报告.md`
- Modify: `docs/旧文件/Entity派生类同质组件审查报告.md`

**Step 1:** 将可访问的 tModLoader 文档 URL 改为绝对本地镜像路径。

**Step 2:** 将“当前 TLS 失败导致文档不可用”的现时结论修正为“历史在线请求失败；当前离线镜像可用”，保留不以 API 文档推断私有运行时行为的边界。

### Task 3: 验证

**Files:**
- Verify: `D:\TRbackup\tmodloader-api-docs-stable\index.html`
- Verify: `D:\TRbackup\tmodloader-api-docs-stable\annotated.html`
- Verify: `D:\TRbackup\tmodloader-api-docs-stable\class_main.html`

**Step 1:** 运行 `rg -n -i "https://docs\.tmodloader\.net/docs/stable"`，确认受控文档与协议不再将在线 URL 作为检索入口。

**Step 2:** 检查 `index.html` 的版本页眉、类型索引和 `Main.player` 的实际锚点存在。

**Step 3:** 检查修改后的 Markdown 段落包含本地入口、检索命令和证据边界。
