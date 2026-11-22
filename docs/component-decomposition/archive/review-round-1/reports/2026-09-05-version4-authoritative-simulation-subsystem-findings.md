# Version4 权威游戏模拟子系统：证据研究笔记

## 结论

本次材料的权威设计报告位于
[`docs/Version4权威游戏模拟系统拆分设计报告.md`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md)。
它的正确使用层级是**子系统架构**，不是把 `Main`、`Player`、`NPC` 等旧类逐字段翻译为
ECS 组件清单。报告明确以世界会话为根、实体交互为核心、Tick 调度为推进器，并将网络、存档
和客户端表现置于权威状态之外的投影/适配边界。

这是一个增量设计盘点，而不是已经验证的迁移或行为等价结论；报告自身声明未执行 Version4
编译或运行时回归，且存在未闭合的 `partial` 证据（报告 1.2、12、13 节）。

## 建议保留的子系统层级

以报告第 5 节的拓扑为主干，初始报告建议只保留下列五个层级及其粗粒度子系统，避免进入
字段、单个 AI style 或单个消息类型的粒度：

| 层级 | 子系统职责 | 边界判断 |
| --- | --- | --- |
| 运行时编排 | `RuntimeComposition` 负责 Tick、暂停、世界准备门控和统一提交 | 它定义阶段顺序，不拥有玩法实体或世界内容。 |
| 世界数据根 | `WorldSession`（世界规则、时间天气、事件进度）、`WorldStorage`（Tile、TileEntity、实体槽位、容器）、`ContentCatalog`（只读定义/规则） | 世界会话、可变世界存储和静态内容目录不可互相替代。 |
| 输入与世界交互 | `IntentAndInteraction`（输入/网络命令验证、Tile/线路交互） | 只产生已验证意图或命令；不能让入站包直接改写权威状态。 |
| 玩法模拟 | `Simulation` 下的移动物理、液体、玩家、NPC、投射物、战斗状态、物品玩法、生成/生命周期/掉落 | 用领域 System 执行状态转换，跨实体规则不重新聚成单一“实体系统”。 |
| 外部边界 | `NetworkSession/ReplicationProjection`、`PersistenceProjection`、`ClientPresentationProjection` | 从已提交权威快照读取；网络、存档和表现不得直接回写权威玩法状态。 |

**主证据：**报告拓扑和依赖方向见
[`Version4权威游戏模拟系统拆分设计报告.md:93`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:93)、
[`...:129`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:129)。最终的根/行为/跨实体规则/投影
归纳见 [`...:523`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:523)。

## 权威性、顺序与写入规则

1. **唯一权威写入路径。**内容目录供模拟读取；世界会话和世界存储供模拟读取；模拟以
   Command/Event 提交回世界存储；外部层只投影。这是报告的强制依赖方向
   （[`...:129`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:129)）。
2. **阶段顺序必须显式。**建议顺序为：命令入口、世界会话、刷怪/AI、玩家控制、移动物理、
   Tile/线路交互、投射物、战斗状态、物品、生命周期/掉落、提交/索引、网络复制、持久化。
   每阶段的读写约束见 [`...:424`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:424)。只有无共享
   写集且不破坏该依赖时才能并行（[`...:442`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:442)）。
3. **Version4 源码支持“先入站、再世界更新”的调度事实。**`Main.DoUpdate` 在进入世界更新前
   调用 `Netplay.UpdateInMainThread`，然后更新天气并以世界准备状态/生成状态决定是否调用
   `DoUpdateInWorld`：[`D:/TRbackup/Version4/Terraria/Main.cs:11244-11258`](D:/TRbackup/Version4/Terraria/Main.cs:11244)、
   [`Main.cs:11344-11355`](D:/TRbackup/Version4/Terraria/Main.cs:11344)、
   [`Main.cs:11400-11409`](D:/TRbackup/Version4/Terraria/Main.cs:11400)。世界更新中先更新玩家，
   再推进游戏计数、刷怪及后续实体路径（[`Main.cs:11420-11472`](D:/TRbackup/Version4/Terraria/Main.cs:11420)）。
   这证明需要一个显式调度器，但不证明现有循环已经满足目标阶段表的全部语义。
4. **网络是命令入口和快照出口，而非第二权威。**`MessageBuffer.GetData` 在处理消息前检查包 ID、
   连接状态与握手状态（[`D:/TRbackup/Version4/Terraria/MessageBuffer.cs:125-178`](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:125)）；
   `Main` 持有固定 Tile、WorldItem、NPC、Projectile 槽位（[`Main.cs:928-946`](D:/TRbackup/Version4/Terraria/Main.cs:928)），
   初始化时把 `whoAmI` 设为槽位索引（[`Main.cs:3465-3484`](D:/TRbackup/Version4/Terraria/Main.cs:3465)）。
   因而初期应保留槽位存储作为内部协议兼容实现，通过 `WorldStorage` 门面和提交队列访问；槽位
   不等同领域身份。

## tModLoader 本地 API 的有限交叉验证

本轮只使用本地稳定 API 文档校验外部边界，不将其当作 Version4 行为实现的替代来源。

- `ModSystem.PreUpdateEntities` 在玩家、NPC、投射物和 Tile 更新前运行，且只在完整更新帧调用，
  并在客户端和服务器均会调用。这支撑把“阶段编排”与“完整世界 Tick”分开建模：
  [`D:/TRbackup/tmodloader-api-docs-stable/class_mod_system.html:1020-1044`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_system.html:1020)。
- `ModSystem.NetSend` 只在服务器、世界数据发送时执行，`NetReceive` 只在客户端、世界数据接收
  成功后执行；文档将 Boss 击败等世界事实作为同步示例。这支持复制是从权威世界状态向客户端的
  单向投影：[`class_mod_system.html:839-896`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_system.html:839)。
- `SaveWorldData` 的用途是保存世界特有标志，如 Boss 击败后才能生成的内容，支持把长期进度置于
  `WorldSession` 而非某个 NPC 实例：
  [`class_mod_system.html:1078-1105`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_system.html:1078)。
- `NetmodeID` 将单机、多人客户端、服务器分别编码为 `0`、`1`、`2`，可作为 Adapter 的运行模式
  输入，不能作为玩法规则本身：
  [`D:/TRbackup/tmodloader-api-docs-stable/class_netmode_i_d.html:94-113`](D:/TRbackup/tmodloader-api-docs-stable/class_netmode_i_d.html:94)。

## 初始拆分的约束与风险

- 保持 `ItemDefinitionCatalog` 的紧凑 ID 表、`Tile` 位字段和固定槽位数组的内部表示；先加受控
  门面/命令提交，避免立即改变压缩和网络格式。
- 不要为每个 NPC AI style 创建组件；先保留行为状态容器及 typed AI System。
- 不得合并输入与速度、Tile 液体与实体接触、领域状态与复制游标、模拟状态与表现历史；这些关系
  在报告中被列为明确禁止合并项（[`...:444`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:444)）。
- 优先迁移世界会话、实体槽位门面和只读内容目录；再验证战斗、物理、物品与事件；网络/存档/表现
  投影放在后期接入，且迁移期间禁止新旧字段双写（[`...:485`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:485)）。
- 当前仍未闭合的关键链包括世界进度存档、投射物 identity 协议、初始化后定义写入、输入服务端
  验证、TileEntity/Chest ID 关系和 NPC AI 写集；因此它们只能标注 `partial`，不能据此冻结模型
  或删除旧字段（[`...:497`](../../baseline/Version4权威游戏模拟系统拆分设计报告.md:497)）。

## 对 NLTX 后续报告的影响

NLTX 的第一份整理报告应采用“**层级 -> 子系统 -> 责任、权威状态类别、输入/输出、相邻依赖、
证据状态**”的格式。只在某个子系统需要证明边界时点出少量代表性类型；不要逐一列出组件字段、
AI 变体、单包处理分支或每个 TileEntity 子类。任何更细粒度设计应另立后续报告，并以读写者、
生命周期、存档与网络协议证据闭合为前提。

