# tModLoader 文档检索

## 适用范围

此流程只用于从 `D:\TRbackup\tmodloader-api-docs-stable\index.html` 获取 Terraria 或 tModLoader
公开类型和成员的已文档化语义。该目录是 tModLoader `v2026.07` stable API 的本地 Doxygen
镜像，不属于 NLTX 源码、构建输入或权威运行时实现。它不能确认目标仓库私有封装、实际调用顺序，
或文档未明确说明的持久化和网络行为。

## 检索流程

1. 先写下查询目标：声明类型、精确成员名，以及要确认的事实（类型、读写语义、生命周期、
   可空性或公开 API）。不要只按字段名猜含义。
2. 设定 `$apiRoot = 'D:\TRbackup\tmodloader-api-docs-stable'`，确认 `$apiRoot\index.html` 存在；
   读取首页的页面标题和页眉版本。`annotated.html` 是类型总表，`classes.html` 是类型索引，
   `namespaces.html` 用于命名空间消歧，`functions.html` 是成员索引。Doxygen 动态搜索框仅作辅助，
   空结果不是“该 API 不存在”的证据。
3. 在 `annotated.html` 或 `classes.html` 以精确类型名搜索，例如
   `rg -n -i 'href="[^"]*"[^>]*>Main<' "$apiRoot\annotated.html"`。读取命中项的**实际 href**，
   再打开对应本地文件；不得由 C# 类型名猜测 Doxygen 文件名。若同名类型不唯一，先使用
   `namespaces.html` 和类型页的命名空间/声明信息消歧。
4. 在已确认的类型页中以精确成员名搜索，例如
   `rg -n -i 'player' "$apiRoot\class_main.html"`。用声明类型、成员区段（数据、属性、方法等）、
   签名和摘要消除重名结果；读取成员链接的**实际 href 或 id 锚点**，将本地类型页路径与锚点组合为
   可复查 evidence path。成员页中找不到时，再查 `functions.html`，不能直接改用猜测文件名。
5. 只从该页明确的签名、摘要和说明推出结论。记录类型页标题、版本、查询词、命中数、本地完整路径
   与锚点、足以支持结论的原文摘要；没有明确说明的语义保持 `partial` 或 `missing`。

## 无法定位时

| 情况 | 处理 |
| --- | --- |
| 类型未在 `annotated.html`/`classes.html` 找到 | 查 `namespaces.html`，再查 `functions.html`；记录已查入口和精确查询词。 |
| 搜索框没有结果 | 改用可见索引，不把空结果当作缺失结论。 |
| 成员有多个同名命中 | 只接受声明类型、成员区段和签名都匹配的锚点；否则记录候选并保留 `partial`。 |
| 本地页面无法打开、链接缺失或版本不匹配 | 记录缺失路径或版本差异，按 `ecs-evidence-protocol.md` 的本地源码回退顺序继续；不得为补证据临时猜测在线 URL。 |
| 关键字段仍无法确认 | 记录 `missing`，停止最终拆分并向调用者说明需补充的类型、版本或源码位置。 |

## 证据记录

每次查询在成员清单或证据日志中保留下列字段：

| 字段 | 填写内容 |
| --- | --- |
| `source` | `D:\TRbackup\tmodloader-api-docs-stable` local stable API mirror |
| `version` | 本地首页页眉显示的版本，如 `tModLoader v2026.07` |
| `query` | 声明类型和精确成员名 |
| `hits` | 已核对的类型/成员命中数和消歧理由 |
| `evidence` | 带成员锚点的完整本地路径、页面标题、签名或摘要 |
| `gaps` | 文档未说明、版本不匹配或候选冲突的部分 |
| `stop_reason` | 已获得充分证据、转本地源码，或关键证据仍缺失 |

示例：查询 `Main.player` 时，先由 `annotated.html` 或 `classes.html` 取得 `Main` 的实际链接
`class_main.html`，再在 `Main Class Reference` 页取得 `player` 的成员锚点；证据必须记录为
`D:\TRbackup\tmodloader-api-docs-stable\class_main.html#a9687e7f510efb7175d2aa8c81cdcbf33`，而不是只记录首页。
