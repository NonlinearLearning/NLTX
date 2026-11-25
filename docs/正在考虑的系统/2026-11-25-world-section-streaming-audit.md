# 参考客户端世界区段流式管理：机制事实与缺陷取证

| 项 | 值 |
| --- | --- |
| 文档 ID | `AUDIT-2026-11-25-WORLD-SECTION-STREAMING` |
| 状态 | `closed`（取证完成；未运行参考客户端，未做运行时一致性验证） |
| 触发 | 用户要求阅读 `D:\TRbackup\最新的客户端` 中「世界区段流式管理」相关代码并讨论 |
| 范围 | `D:\TRbackup\最新的客户端` 全部 `Section` 相关实现，对照本仓库 `src/`（NSSLC ECS 基线） |
| 判据 | 「流式」= 区段数据按需下发 / 按需驻留 / 按需释放三者至少成立其一；仅首次全量加载不算 |
| 日期 | 2026-11-25（Asia/Shanghai） |
| 参考客户端 | 反编译快照，1579 个 `.cs` 文件 |

## 1. 问题与结论速览

调查起点是「世界区段流式管理」这个说法。读码结论是：

1. **「区段」是 200 × 150 格的固定网格**，坐标换算只有两个纯函数 `Netplay.GetSectionX/Y`（`Netplay.cs:770-778`），常量在 `Main.cs:1091-1093`（`sectionWidth = 200`、`sectionHeight = 150`）。
2. **区段概念上挂着三个互不相同的子系统**：地形下发（`WorldSections` bit0/bit1）、活跃区段判定（`ActiveSections` + `RemoteClient.TileSectionsCheckTime`）、地图绘制与刷帧调度（`WorldSections` bit2/bit3）。三者共用网格，但**生命周期、属主、失效语义完全不同**。
3. **流式是真的**：服务端每 tick 只按玩家所在 3×3 九宫格下发，客户端撞到未加载格时用 159 号包反向补拉。
4. **但流式只覆盖「首次进入」**：全库搜不到任何卸载路径（`UnloadSection` / `SetSectionUnloaded` / 清理 `TileSections` 均 0 命中）。跑遍全图后两侧都退化为全量常驻。
5. **服务端不构造 `sectionManager`**：`WorldGen.cs:7091-7093` 的 `new WorldSections(...)` 被 `if (Main.netMode != 2)` 包住。服务端只有 `RemoteClient` 侧那份 `bool[,]` 账本。
6. 找到 **8 处可证缺陷**（§7）：其中 2 处会静默丢数据或静默漏判（§7.2、§7.5），1 处会在长期运行后翻转语义（§7.3），其余为开销与一致性隐患。

对 NLTX 的直接含义见 §8 / §9：NLTX 现有实现只接了「出生时一次性下发 + 移动补推 + 159 补拉」，**缺** 活跃区段判定、区段内箱子/NPC 补发、传送门区段补发，且 `_sections` 是**构造期快照**，后续地形改动不会进入已缓存载荷。

---

## 2. 证据基线与读取范围

| 项 | 内容 |
| --- | --- |
| 源码根 | `D:\TRbackup\最新的客户端` |
| 规模 | 1579 个 `.cs` 文件；`Section` 相关命中 368 处 / 35 个文件 |
| 命中集中度 | `LeashedEntity.cs`（45，实为无关的 `Section` 同名变量）、`WorldSections.cs`（41）、`MessageBuffer.cs`（31）、`WorldGen.cs`（31）、`RemoteClient.cs`（29）、`NetMessage.cs`（20）、`Main.cs`（19）、`ActiveSections.cs`（17） |
| 读取方式 | grep 定位 + 分段读；未整文件通读 `Main.cs`（约 66k 行）、`WorldGen.cs`（约 82k 行）、`NetMessage.cs` |
| 未读取 | 渲染层细节（`MapRenderer.DrawToMap_Section` 内部）、`PortalHelper` 的完整实现、测试夹具 `Terraria.Testing.*` 的全部用例 |

---

## 3. 坐标系与三方属主

### 3.1 网格参数

```csharp
// Terraria\Main.cs:1091-1097
public const int sectionWidth  = 200;
public const int sectionHeight = 150;
public static int maxSectionsX = maxTilesX / 200;   // 初值，随世界尺寸变
public static int maxSectionsY = maxTilesY / 150;
```

`maxSectionsX/Y` 在 `WorldGen.cs:6498-6499` 按实际世界尺寸重算（`Main.maxTilesX / 200`）。

换算（`Netplay.cs:770-778`）：

```csharp
public static int GetSectionX(int x) => x / 200;
public static int GetSectionY(int y) => y / 150;
```

注意二者**入参语义不同**：`GetSectionX` 同时被当作「格坐标」和「已除以 16 的像素坐标」使用——`RemoteClient.cs:155-156` 传 `(int)(position.X / 16f)`，而 `WorldSections.TileLoaded`（`WorldSections.cs:105-108`）直接传格坐标。调用方必须自己保证单位，函数本身不做区分。

### 3.2 三套子系统

| 子系统 | 状态载体 | 属主 | 失效语义 |
| --- | --- | --- | --- |
| ① 地形下发 | `RemoteClient.TileSections` (`bool[,]`) + 客户端 `WorldSections.data` bit0 | 服务端连接 / 客户端世界 | 无失效（只置 true） |
| ② 活跃判定 | `ActiveSections.LastActiveTime` + `RemoteClient.TileSectionsCheckTime` | 全局 / 每连接 | 60 tick 后自然失效 |
| ③ 绘制调度 | `WorldSections.data` bit1/bit2/bit3 | 客户端世界 | framed 后可被批量重置 |

三者的网格尺寸口径**并不一致**（§7.4）。

---

## 4. 地形流式下发全链路

### 4.1 服务端推送主循环

入口在专用服每 tick 的玩家循环（`Main.cs:65590-65593`）：

```csharp
if (player[k].active) {
    RemoteClient.CheckSection(k, player[k].position);
}
```

`RemoteClient.CheckSection`（`RemoteClient.cs:126-150`）有三层结构：

1. **重入保护**：`remoteClient.CheckingSections` 为真直接 return；
2. **本人区段**：`CheckSection_ForClient(remoteClient, position, fluff)`；
3. **旁观者级联**：遍历 255 个玩家，凡 `player.spectating == playerIndex` 的，递归以**同一 position** 再跑一遍。

第 3 步的语义值得注意：旁观者拿到的是**被旁观者的坐标**，而不是自己的。这是有意的（旁观者应该看见被旁观者的视野），但也意味着旁观者自己的位置从不参与区段计算。

### 4.2 九宫格判定

`CheckSection_ForClient`（`RemoteClient.cs:152-190`）：

```csharp
ActiveSections.CheckSection(position, fluff);          // 同时刷新全局活跃表
int sectionX = Netplay.GetSectionX((int)(position.X / 16f));
int sectionY = Netplay.GetSectionY((int)(position.Y / 16f));
int num = 0;
for (i = sectionX - fluff; i < sectionX + fluff + 1; i++)
  for (j = sectionY - fluff; j < sectionY + fluff + 1; j++)
    if (在界内) {
        bool num2 = client.IsSectionActive(new Point(i, j));
        client.TileSectionsCheckTime[i, j] = Main.GameUpdateCount;   // 无条件打时间戳
        if (!num2) NetSectionActivated(client.Id, new Point(i, j));  // 激活事件
        if (!client.TileSections[i, j]) num++;                       // 未下发计数
    }
if (num <= 0) return;
NetMessage.SendData(9, client.Id, -1, Lang.inter[44].ToNetworkText(), num);  // 进度条
for (k = sectionX - fluff; k < sectionX + fluff + 1; k++)
  for (l = sectionY - fluff; l < sectionY + fluff + 1; l++)
    NetMessage.SendSection(client.Id, k, l);      // 无界内检查，靠 SendSection 内部兜
```

两点观察：

- **`num` 只用于发进度条**，不影响实际发送。第二段循环对九宫格内**所有**区段调 `SendSection`，已下发的会被 `SendSection` 内部的 `!TileSections` 判断挡掉——是一个「靠下游幂等」的写法。
- **`TileSectionsCheckTime` 无条件写**，即使该区段从未下发。所以「活跃」和「已下发」是两个独立维度，可以出现「活跃但无地形」。

### 4.3 单区段发送

`NetMessage.SendSection`（`NetMessage.cs:2732-2757`）：

```csharp
if (Main.netMode != 2) return;
try {
    if (sectionX >= 0 && sectionY >= 0 && sectionX < Main.maxSectionsX && sectionY < Main.maxSectionsY
        && !Netplay.Clients[whoAmi].TileSections[sectionX, sectionY])
    {
        Netplay.Clients[whoAmi].TileSections[sectionX, sectionY] = true;   // ← 先置位
        int number = sectionX * 200;
        int num    = sectionY * 150;
        int num2   = 150;
        for (int i = num; i < num + 150; i += num2)
            SendData(10, whoAmi, -1, null, number, i, 200f, num2);         // 一条 200×150 条带
        SyncNPCsForSection(whoAmi, sectionX, sectionY);
        SyncChestContentsForSection(whoAmi, sectionX, sectionY);
    }
} catch { }                                                                // ← 空吞
```

虽然循环写成 `i += num2`，但 `num2` 恒为 150、上界 `num + 150`，**实际只发一条**，即整个 200×150 区段作为一个包。区段边界不裁剪：`number = sectionX * 200` 在最后一个区段时可能超出 `maxTilesX`，客户端解压侧靠 `DecompressTileBlock_Inner` 的 `Main.tile[j, i]` 访问自行兜（§4.4）。

### 4.4 客户端接收与解压

`MessageBuffer.cs:894-899` 分发到 `NetMessage.DecompressTileBlock`（`NetMessage.cs:2256-2270`）：

```csharp
DeflateStream val = new DeflateStream(stream, CompressionMode.Decompress, true);
try {
    BinaryReader binaryReader = new BinaryReader(val);
    DecompressTileBlock_Inner(binaryReader,
        binaryReader.ReadInt32(),   // xStart
        binaryReader.ReadInt32(),   // yStart
        binaryReader.ReadInt16(),   // width
        binaryReader.ReadInt16());  // height
} finally { ((IDisposable)val)?.Dispose(); }
```

`DecompressTileBlock_Inner`（`NetMessage.cs:2272-2492`）逐格写 `Main.tile`，支持 RLE（`num` 复制上一格）。收尾三件事：

```csharp
// NetMessage.cs:2490-2491
MapUpdateQueue.Add(new Rectangle(xStart, yStart, width, height));
Main.sectionManager.SetTilesLoaded(xStart, yStart, xStart + width - 1, yStart + height - 1);
```

**这里就是 `sectionManager` 在客户端侧唯一被写 bit0 的地方。**

### 4.5 客户端反向请求（159 号包）

全库只有两处主动请求，都是「操作撞到未加载格」：

| 位置 | 场景 | 载荷 |
| --- | --- | --- |
| `Player.cs:41844-41850` | 放绳子时下方格未加载 | `num4 / 200`, `(num3 + 1) / 150` |
| `WorldItem.cs:374-380` | 拾取判定撞到未加载格 | `point.X / 200`, `point.Y / 150` |

判断依据统一是 `WorldGen.IsTileLoaded`（`WorldGen.cs:9655-9662`）：

```csharp
public static bool IsTileLoaded(int x, int y) {
    if (InWorld(x, y)) return Main.tile[x, y] != null;
    return false;
}
```

即**「已加载」在客户端等价于「`Tile` 实例非 null」**，与 `WorldSections.data[i][0]` 是两套并行表示。

服务端处理在 `MessageBuffer.cs:4496-4503`：

```csharp
case 159:
    if (Main.netMode == 2) {
        int sectionX = reader.ReadUInt16();
        int sectionY = reader.ReadUInt16();
        NetMessage.SendSection(whoAmI, sectionX, sectionY);
    }
    break;
```

写入端 `NetMessage.cs:1675-1678` 是 `writer.Write((short)number); writer.Write((short)number2);` —— **写 `short`、读 `ushort`**，两者字节宽度一致，非负值等价。

### 4.6 进服时的批量下发

`MessageBuffer.cs:669` 的 `case 8`（客户端上报出生点）里构造 5×3 区段清单：

```csharp
int num197 = Netplay.GetSectionX(Main.spawnTileX) - 2;
int num198 = Netplay.GetSectionY(Main.spawnTileY) - 1;
int num199 = num197 + 5;   // X 方向 5 个
int num200 = num198 + 3;   // Y 方向 3 个
```

随后三段追加：

- `flag20`：队伍出生点（`num194..num204` × `num195..num205`），与主区不重叠才加；
- `flag21`：`Main.teamBasedSpawnsSeed` 下的额外队伍出生点，需 `ExtraSpawnPointManager.TryGetExtraSpawnPointForTeam` 成功；
- `PortalHelper.SyncPortalsOnPlayerJoin(whoAmI, 1, list, out var portalSections)`（`MessageBuffer.cs:810`）。

发完 `9` 号进度条（`num201` = 总区段数）后逐个 `SendSection`，再补发全部 `Main.item`（`21`/`22`）与 `Main.npc`（`23`/`54`）的初始快照（`MessageBuffer.cs:848-862`）。

**边界口径不一致（可证）**：主区段区间用 `< num199` 语义并把 `num199 = Main.maxSectionsX` 夹成**上界**；而队伍出生点区间用 `<= num204` 语义并把 `num204` 夹成 `Main.maxSectionsX - 1`，再以 `num206 <= num204` 作**闭区间上界**。两段代码对「`maxSectionsX` 是长度还是最大下标」采取了不同假设。

---

## 5. 活跃区段判定（ActiveSections）

`Terraria.DataStructures\ActiveSections.cs` 全文 62 行：

```csharp
public static readonly uint SectionInactiveTime = 60u;
private static uint[,] LastActiveTime =
    new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];
public static event Action<Point> SectionActivated;

public static void CheckSection(Vector2 position, int fluff = 1) { /* 3×3 打时间戳 */ }
public static bool IsSectionActive(Point p) =>
    LastActiveTime[p.X, p.Y] + SectionInactiveTime >= Main.GameUpdateCount;
public static int TimeTillInactive(Point p) =>
    Math.Max(0, LastActiveTime[p.X, p.Y] + SectionInactiveTime - Main.GameUpdateCount);
```

### 5.1 语义

「活跃」= 最近 60 个 `GameUpdateCount` 内被某个玩家的 3×3 窗口覆盖过。**不是几何视野，是时间窗口。**

`Main.cs:18064-18065` 在客户端每帧对 `LocalPlayer.position` 与 `LocalPlayer.SpectatingCameraPosition` 各调一次。

### 5.2 消费点

服务端用它裁剪三类广播：

| 消息 | 位置 | 条件 |
| --- | --- | --- |
| NPC 同步（23） | `NetMessage.cs:1753` / `1769` | `!IsSectionActive(npc.NetSectionCoordinates)` 时跳过，`skippedSyncs < 4` 才补 |
| 弹幕同步（27） | `NetMessage.cs:1803` | 同上，记 `netSyncSkippedForPlayer` |
| 区域方块改动（20） | `NetMessage.cs:1725` | 用 `SectionRange` 做四角判定，见 §7.5 |

### 5.3 一份隐藏的并行账本

`RemoteClient.cs:31-35`：

```csharp
public bool[,]  TileSections          = new bool[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];
public uint[,]  TileSectionsCheckTime = new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];
public bool     CheckingSections;
```

`RemoteClient.IsSectionActive`（`RemoteClient.cs:221-225`）**不复用 `ActiveSections`**，而是读自己这份 `TileSectionsCheckTime`。两份表在 `CheckSection_ForClient` 里被同时刷新（`ActiveSections.CheckSection(position, fluff)` 与逐个 `TileSectionsCheckTime[i, j] = Main.GameUpdateCount`），但因为 per-client 那份受 `fluff` 与远处/近处玩家差异影响，**两者可以不一致**。

---

## 6. 地图绘制与刷帧调度

### 6.1 WorldSections 的位图

`WorldSections.cs:45` 用 `BitsByte[] data`（`width * height` 个）承载四个位：

| bit | 常量（**死代码**，见 §7.1） | 含义 | 计数变量 |
| --- | --- | --- | --- |
| 0 | `BitIndex_SectionLoaded` | 收到过 `10` 号包 | —— |
| 1 | `BitIndex_SectionFramed` | 已完成一次 `SectionTileFrame` | `frameSectionsLeft` |
| 2 | `BitIndex_SectionMapDrawn` | 已烤进世界地图 | `mapSectionsLeft` |
| 3 | `BitIndex_SectionNeedsRefresh` | 需要重刷（隐形块开关等） | `_sectionsNeedingRefresh` |

### 6.2 刷帧（bit0 → bit1）

`SetTilesLoaded`（`WorldSections.cs:203-216`）把一块矩形映射到区段并调 `SetSectionLoaded`。后者语义是**双分支**（`WorldSections.cs:181-193`）：

```csharp
private void SetSectionLoaded(ref BitsByte section) {
    if (!section[0]) { section[0] = true;  frameSectionsLeft++; }   // 首次
    else if (section[1]) { section[1] = false; frameSectionsLeft++; } // 已 framed → 作废重刷
}
```

即「再次加载」被复用为「标记需要重新刷帧」。这是全模块里设计最紧凑的一处复用。

刷帧实际发生在渲染线程每帧（`TileDrawing.cs:6686-6691`），带 5ms 式预算的是地图那侧而不是这里：

```csharp
if (Main.sectionManager.AnyUnfinishedSections) {
    TimeLogger.StartTimestamp fromTimestamp = TimeLogger.Start();
    WorldGen.SectionTileFrameWithCheck(firstTileX, firstTileY, lastTileX, lastTileY);
    TimeLogger.SectionFraming.AddTime(fromTimestamp);
}
```

`SectionTileFrameWithCheck`（`WorldGen.cs:80633-80653`）遍历**屏内**区段，对「已加载且未 framed」的调 `SectionTileFrame`，后者在 `WorldGen.cs:80785` 置 bit1 并 `frameSectionsLeft--`（`WorldSections.cs:159-171`）。**没有时间上限**——一帧内若屏内有多块待刷就全刷完。

### 6.3 刷新（bit3）

`AnyNeedRefresh` 触发 `WorldGen.RefreshSections`（`WorldGen.cs:80655-80671`），后者对屏内每块 `RefreshSection`（`80673-80699`）：

- 世界生成期：全区逐格 `Reframe`；
- 运行期：按 `200 × 50` 条带切 `RefreshStrip`，顺带统计 `totalEcho`/`totalFramed`（隐形块相关）。

完成后 `SetSectionAsRefreshed`（`WorldSections.cs:71-78`）清 bit3 并 `_sectionsNeedingRefresh--`。

### 6.4 地图烤制：唯一做了时间片的路径

`Main.cs:62811-62828`：

```csharp
TimeLogger.StartTimestamp fromTimestamp4 = TimeLogger.Start();
int num3 = 0;
while (fromTimestamp4.Elapsed.TotalMilliseconds < 5.0
       && sectionManager.GetNextMapDraw(player[myPlayer].position, out x, out y))
{
    if (DebugOptions.unlockMap != 0 && sectionManager.SectionLoaded(x, y))
        Map.UnlockMapSection(x, y);
    MapRenderer.DrawToMap_Section(x, y);
    num3++;
}
if (num3 == 0 && !refreshMap) DebugOptions.unlockMap = 0;
```

`GetNextMapDraw`（`WorldSections.cs:218-358`，141 行）是本模块最复杂的一段：从玩家位置出发走**方螺旋**，`prevMap` 保存断点续走，并且留了显式护栏：

```csharp
// WorldSections.cs:264-269
if (num8 == 4) {
    if (flag2) throw new Exception("Infinite loop in WorldSections.GetNextMapDraw");
    flag2 = true; ...
}
```

这个 throw 说明该算法历史上真的死循环过。`mapSectionsLeft` 在 `ClearMapDraw`（`150-157`）重置为 `data.Length`。

**对比结论**：同一套区段概念，地图烤制有 5ms 预算、刷帧有屏内裁剪、地形下发两者皆无（§7.2）。

### 6.5 三处「全量重置刷帧」的调用

`SetAllFramedSectionsAsNeedingRefresh`（`WorldSections.cs:93-103`）共 3 个调用点：

| 位置 | 上下文 | 频次 |
| --- | --- | --- |
| `Main.cs:57839` | `DrawCapture`（`57556-57972`）内部；该方法由 `CaptureCamera.DrawTick` 按**截图分块**调用（`CaptureCamera.cs:190-193`） | 每截图分块一次 |
| `TileDrawing.cs:388` | `Update()` 内，仅当「显示隐形块」开关翻转时 | 事件驱动 |
| `WorldGenerator.cs:169` | `QueueMainThreadAction` | 事件驱动 |

§7.3 讨论其计数器缺陷。

---

## 7. 缺陷清单

### 7.1 BitIndex_Section* 四个常量是死代码

`WorldSections.cs:33-39` 定义了四个 `public const int`，全库 grep **只有定义、零引用**。所有实际代码使用魔数下标 `data[y * width + x][0]` … `[3]`。

影响：位语义只存在于作者记忆中；新增第 5 个位时无法依靠常量做一致性检查。

### 7.2 SendSection 先置位后发送，异常被空 catch 吞掉

`NetMessage.cs:2738-2756`（完整代码见 §4.3）。失败模式：

1. `TileSections[sectionX, sectionY] = true` 已经写入；
2. `SendData(10, ...)` 抛异常（发送缓冲区、断线、序列化）；
3. `catch { }` 静默吞掉，`SyncNPCsForSection` 与 `SyncChestContentsForSection` **永不执行**；
4. 因为第 1 步已置位，后续任何 `SendSection` 调用都会在 `!TileSections[...]` 处短路。

结果：**该区段地形永久缺失，且无任何重试路径**。客户端表现为地图缺块、可穿墙；该区段的箱子内容与城镇 NPC 一并丢失。

需要说明的是**触发概率**：`SendData`（`NetMessage.cs:81-1856`）内部对 `buffer[num]` 加了 `lock`，写入走固定长度 `writeBuffer`，正常路径下不易抛。因此这条更接近「错误处理设计缺陷」而非「高频故障」——但它把一个**本可恢复**的异常变成了**不可恢复**的状态，且空 `catch` 抹掉了全部可观测性。

### 7.3 _sectionsNeedingRefresh 只增不减导致的语义翻转

`WorldSections.cs:93-103`：

```csharp
public void SetAllFramedSectionsAsNeedingRefresh() {
    for (int i = 0; i < data.Length; i++)
        if (data[i][1]) { data[i][3] = true; _sectionsNeedingRefresh++; }   // 无条件 ++
}
```

位是幂等的（重复置 true 无害），**计数器不是**。`SetSectionAsRefreshed`（`71-78`）只在 `data[..][3]` 为 true 时才 `--`。因此：

- **第一次调用之后**，`_sectionsNeedingRefresh` 恒 > 0，`AnyNeedRefresh`（`59`）**永久为 true**，`TileDrawing.cs:6692` 的守卫从此失效，每帧都会进 `RefreshSections` 遍历一次屏内区段做 `SectionNeedsRefresh` 判定（每块一次数组访问，开销很小，但语义上是死的）；
- `int` 单调递增无上界检查。按最大世界 672 个已 framed 区段/次调用估算，约 **4800 次调用**后溢出为负；届时 `AnyNeedRefresh` 翻回 false，**刷新功能真正停摆**。

触发路径需要数千次调用，不属常规游玩路径；但 `AnyNeedRefresh` 从第一次调用起就恒真，这一条是**无条件成立**的。

### 7.4 四个区段数组的尺寸口径不一致

| 数组 | 尺寸表达式 | 位置 |
| --- | --- | --- |
| `WorldSections.data` | `maxTilesX / 200`（截断） | `WorldGen.cs:7093` |
| `ActiveSections.LastActiveTime` | `maxTilesX / 200 + 1` | `ActiveSections.cs:11` |
| `RemoteClient.TileSections` / `TileSectionsCheckTime` | `maxTilesX / 200 + 1` | `RemoteClient.cs:31-33` |
| `NetworkTestClient` 构造 | `(maxTilesX + 199) / 200`（向上取整） | `NetworkTestClient.cs:256` |

标准世界（4200/6400/8400 × 1200/1800/2400）均可整除，故常规情况下不可观测。但一旦世界尺寸非 200/150 整数倍，`WorldSections` 会**少一行一列**。

配套的越界语义也不统一：`ClampSectionCoords`（`ActiveSections.cs:53-56`）用**闭区间** `Utils.Clamp(point.X, 0, Main.maxSectionsX)`，**可以返回 `maxSectionsX` 这个越界下标**，靠的正是 `+1` 的数组尺寸兜住：

```csharp
return new Point(Utils.Clamp(point.X, 0, Main.maxSectionsX),
                 Utils.Clamp(point.Y, 0, Main.maxSectionsY));
```

而 `WorldSections` 内部的 `x < width` 是硬边界，不享受这个宽容。两套 clamp 语义并存是明确的踩坑面。

### 7.5 SectionRange 的四角采样会漏判

`RemoteClient.cs:192-219` 只检查方块矩形的**四个角**所在区段，任一已下发即返回 true：

```csharp
for (int i = 0; i < 4; i++) {
    int num = firstX; int num2 = firstY;
    if (i == 1) num += size;
    if (i == 2) num2 += size;
    if (i == 3) { num += size; num2 += size; }
    if (TileSections[GetSectionX(num), GetSectionY(num2)]) return true;
}
return false;
```

唯一调用点是 `NetMessage.cs:1725` 的 20 号包（`AreaTileChange`）广播：

```csharp
Netplay.Clients[num23].SectionRange((int)Math.Max(number3, number4), number, (int)number2)
```

参数实际语义：`size = Math.Max(width, height)`、`firstX = number`（X 起点）、`firstY = number2`（Y 起点）。当宽高不等时，用 `Max` 当尺寸会让「角的采样点」落在矩形之外——**采样点可能整体错位**。且四角全部未下发但矩形中部跨越了已下发区段时，会误判为不可见。

### 7.6 SyncChestContentsForSection / SyncNPCsForSection 是全量线性扫描

`NetMessage.cs:2759-2790`：

```csharp
private static void SyncChestContentsForSection(int whoAmi, int sectionX, int sectionY) {
    for (int i = 0; i < 8000; i++) {
        Chest chest = Main.chest[i];
        if (chest != null && sectionX == GetSectionX(chest.x) && sectionY == GetSectionY(chest.y))
            SendChestContentsTo(i, whoAmi);
    }
}
```

每个区段下发都要扫 **8000 个箱子槽位 + `Main.maxNPCs` 个 NPC**。九宫格 9 个区段即 9 × (8000 + maxNPCs) ≈ **7 万次遍历 / 玩家 / 跨区段**。没有空间索引。

### 7.7 服务端没有 sectionManager

`WorldGen.cs:7091-7093`：

```csharp
if (Main.netMode != 2)
{
    Main.sectionManager = new WorldSections(Main.maxTilesX / 200, Main.maxTilesY / 150);
}
```

专用服上 `Main.sectionManager` 保持 `null`。当前服务端确实不碰它——`SendSection` 只动 `RemoteClient.TileSections`。但这是**纯约定**而非类型保证：任何一处服务端代码误调 `Main.sectionManager.X` 就是 `NullReferenceException`。

（另一佐证：`WorldGen.cs:80643` 的 `SectionTileFrameWithCheck` 只要调用方是服务端就会 NRE，而它恰好只被客户端渲染路径调用。）

### 7.8 PortalHelper 是 159 号包之外唯一主动扩区段集合的地方

`MessageBuffer.cs:810-811`、`846`：进服时把传送门所在区段追加进清单并下发。`PortalHelper.cs:452` 自己也有一份带界内检查的筛选：

```csharp
if (j >= 0 && j < Main.maxSectionsX && k >= 0 && k < Main.maxSectionsY
    && !Netplay.Clients[plr].TileSections[j, k]
    && !dontInclude.Contains(new Point(j, k)))
```

含义：传送门可以让你**瞬移到从未下发的区段**，所以进服时预先把传送门区段推过去。这是流式模型的一个真实补丁——也说明「按玩家位置 3×3 下发」不足以覆盖瞬移。

---

## 8. NLTX 现状对照

| 能力 | NLTX 现状 | 证据 |
| --- | --- | --- |
| 区段常量 / 坐标换算 | **已具备** | `TerrariaProtocolAdmissionHandler.cs:33-35`（`SectionWidth = 200`、`SectionHeight = 150`、`TilePixelSize = 16`） |
| 区段载荷预压缩 | **已具备**（且优于原版） | `PrepareInitialSections`（`192-219`）在**构造期**一次性压缩全部区段，启动日志 `ecs.protocol.world_cache_ready` 输出 `sections`/`compressedBytes`/`elapsedMs` |
| 出生区段下发 | **已具备** | `SelectInitialSections`（`221-229`）：`abs(dx) <= 2 && abs(dy) <= 1` → **5×3 = 15 块**，与参考客户端一致 |
| 移动补推 | **已具备** | `CreateMovementSectionIntents`（`274-318`）：3×3 九宫格，触发条件为收到 `PlayerControlsMessageId` |
| 159 补拉 | **已具备** | `HandleSectionRequest`（`361-388`）：校验 `payload.Length != 4`，越界记 `ecs.protocol.section_rejected` 后丢弃 |
| 已下发账本 | **已具备** | `CommunicationSessionComponent` 的 `_sentWorldSections`（`10`）与 `HasSentWorldSection`/`MarkWorldSectionSent`（`35-44`） |
| 活跃区段判定 | **无** | `ActiveSection` 在 `src` 下仅 1 命中，且是 `LegacyApi.cs:12-15` 的 `dynamic` 空壳 |
| 区段内箱子/NPC 补发 | **无** | `SyncNPCsForSection` 0 命中；箱子仅有 `Packet_155` 编解码器，无「按区段补发」逻辑 |
| 传送门区段补发 | **无** | `SyncPortalsOnPlayerJoin` 0 命中 |
| 额外出生点区段 | **部分** | `ExtraSpawnPoint` 有 100 处命中（世界生成 + 包 7 字段），但无「按区段补发」 |
| 地图绘制 / 刷帧调度 | **无运行时实现** | `MapUpdateQueue` 有 3 处 `Add` 调用（`WorldGen.cs:80650`、`80707`、`81664`），但 `LegacyApi.cs:232` 是空壳 |
| `WorldSections` 本体 | **仅反编译副本** | `src\NSSLC.Infrastructure\WorldGeneration\Runtime\WorldSections.cs`，**原样保留了 §7.1 的死常量与 §7.3 的计数器缺陷** |

### 8.1 NLTX 侧的三处独立缺陷

**(a) HandleSectionRequest 与 CreateMovementSectionIntents 同样是「先记账后交付」**

`TerrariaProtocolAdmissionHandler.cs:383-387`：

```csharp
session.MarkWorldSectionSent(sectionX, sectionY);
return [
  CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(1), session),
  CreateIntent(TileSectionMessageId, section.Payload, session)
];
```

与 §7.2 同构：登记发生在返回 `Intent` 之前。若这组 `Intent` 在后续管线中被丢弃（背压、会话提前关闭、发送失败），该区段**永久不可再请求**。NLTX 的日志（`ecs.protocol.section_request`）至少留下了痕迹，但没有回滚手段。

**(b) _sections 是构造期快照，后续地形改动永不进入已缓存载荷**

`_sections` 在构造函数里由 `PrepareInitialSections` 生成一次（`51`、`192-219`），此后只读。这意味着：

- 玩家挖掉一块方块后再请求同一区段，拿到的仍是**压缩时的旧地形**；
- 参考客户端是靠「`TileSections` 只置 true 不置 false」规避了这个问题（永不重发），NLTX 继承了这个语义但没有继承它的合理性说明。

若 NLTX 未来要支持区段重发（比如卸载后重载），必须先解决载荷失效问题。

**(c) 移动补推只挂在 PlayerControls 包上**

`HandleActivePacket`（`244-272`）只在 `packet.Envelope.MessageId == GameCommunicationAdapter.PlayerControlsMessageId` 时才调 `CreateMovementSectionIntents`。后果：**传送、被击退、出生后的初始位置** 等不产生 `PlayerControls` 包的位移不会触发补推。参考客户端是靠服务端每 tick 主动 `CheckSection(player.position)`（`Main.cs:65592`），不依赖客户端上报。

---

## 9. 对 NLTX 的设计含义

### 9.1 「区段」应当拆成三个显式概念，不要继续共用一个 data 位图

参考客户端把「已下发」「已刷帧」「已烤地图」「需刷新」压进同一个 `BitsByte`，代价是 §7.1 的魔数下标与 §7.3 的计数器漂移。NLTX 应把它们拆成：

- **传输侧**：`SentSections`（每会话，已存在于 `CommunicationSessionComponent`）；
- **活跃侧**：`ActiveSections`（全局时间窗，需新增）；
- **表现侧**：`FrameState`（客户端，与传输完全解耦）。

三者网格相同、生命周期不同，混用一个类会在 ECS 化时立刻反噬。

### 9.2 服务端权威的「按位置驱动」应取代「按包驱动」

§8.1(c) 的根因是把区段补推挂在客户端包上。若 NLTX 要走向服务端权威（与其 ECS 定位一致），补推应由**服务端玩家位置系统**按 tick 驱动，而不是由通信层解析 `PlayerControls` 载荷里的坐标。这也符合 `Context/项目约束/ECS系统输入边界约束.md`——通信层不应反解玩法数据来决定世界状态。

### 9.3 区段载荷需要「版本」或「失效」语义

§8.1(b)。两个候选：

- **A：不可变快照 + 显式失效**。地形改动时把受影响区段的载荷置脏，下次请求重新压缩。代价是压缩开销移到了运行时。
- **B：区段载荷 + 增量包**。载荷保持不可变，改动通过已有的方块改动包（参考客户端的 20 号 `AreaTileChange`）补丁。

参考客户端的实际选择是 **B 的简化版**：载荷不可变、改动走 8/20/21 号包、且**从不重发区段**。NLTX 目前也是这样，但没有任何地方写明这是有意选择。

### 9.4 区段下发需要时间片

§6.4 的对比是明确的：参考客户端对地图烤制设了 5ms 预算，对地形下发没有。玩家高速跨区段时服务端会在一帧内同步压完 9 个包。NLTX 目前更糟——`PrepareInitialSections` 在**构造期**就把全部区段压完了，虽然躲开了运行时压缩，但换来的是构造期的一次长阻塞（有 `elapsedMs` 日志可观测）。

若 NLTX 要做运行时增量，应显式引入「每 tick 区段发送预算」而不必复用参考客户端的隐式行为。

### 9.5 活跃区段是 NPC/弹幕裁剪的前置能力

参考客户端用它裁掉 `23`/`27` 号包的发送（§5.2）。NLTX 若要做带裁剪的 NPC 同步（`CR-2026-11-22-npc-sync-outbound-priority.md` 已登记方向），必须先有活跃区段判定——现在没有。

### 9.6 优先级建议

- **低风险**：把 `_sentWorldSections` 的登记时机从「生成 Intent 前」改为「Intent 确认入队后」（§8.1(a)）。
- **次之**：显式定义区段载荷的失效语义（§8.1(b) / §9.3），哪怕结论是「永不失效」，也要写进文档。
- **再次**：新增 `ActiveSections`，先只做观测（日志）不接裁剪。
- **最后**：区段内 NPC/箱子补发。参考客户端用的是全量扫描（§7.6），NLTX 若照搬会把 `8000 + maxNPCs` 的遍历搬进 tick，建议先设计空间索引再实现。

---

## 10. 局限与未验证项

1. **纯静态读码**，未运行参考客户端，未做多人实测。§7.3 的「约 4800 次调用后溢出」是按最大世界的区段总数估算，未实测。
2. **未通读** `Main.cs`（约 66k 行）、`WorldGen.cs`（约 82k 行）、`NetMessage.cs`；结论建立在 grep 命中点及其上下文，可能存在本文未覆盖的、按世界类型或网络模式特化的区段分支。
3. **未核对** `MapRenderer.DrawToMap_Section` 的实际开销，因此 §6.4 的 5ms 预算只能证明「有预算」不能证明「预算合理」。
4. **未核对** `PortalHelper.SyncPortalsOnPlayerJoin` 的完整筛选逻辑，§7.8 只读到了 `PortalHelper.cs:452` 的边界判断。
5. **未验证** §9.3 的「参考客户端从不重发区段」这一负向断言。它是基于「全库 0 命中卸载/清理路径」的推断；理论上可能存在经由 `Reset()`（`RemoteClient.cs:227-230`，会 `Array.Clear(TileSections)`）的重置路径，但该重置只在连接建立/断开时发生，不构成流式重发。
6. **NLTX 侧**只读了 `TerrariaProtocolAdmissionHandler`、`CommunicationSessionComponent`、`WorldSections`（反编译副本）与 `LegacyApi`；未审计 `src\NSSLC.Infrastructure\Network` 下协议管线的 `Intent` 生命周期，因此 §8.1(a) 的「Intent 可能被丢弃」是设计层面的风险陈述，未定位到具体丢弃点。
