# 数据包代码目录

覆盖 Version4 消息号 0–161，共 165 个数据包定义。56、69、85 的请求和响应各自保留一个基础定义文件。

| 目录 | 内容 |
| --- | --- |
| `Packets/<类别>/` | 每个数据包一个 `.cs` 文件，字段和属性直接定义在包类中；专用枚举及真实子结构放在同一文件。 |
| `Codecs/<类别>/` | 对应类别的读写方法与 codec 注册；不放数据包基础结构或图声明。 |
| `Shared/` | 多包复用的线路值类型，按 Primitives、Text、Items、Tiles、Combat、TileEntities、Reserved 分类。 |
| `Definitions/` | 编译器图声明、协议目录、协议外部输入与帧容量。 |

全部 `PacketGraph`、`WireFormat` 及定义宿主集中在 [PacketDefinitions.cs](./Definitions/PacketDefinitions.cs)。既有宿主入口汇总各包定义，`PacketDefinitions` 类逐包声明直接成员。全协议入口仍为 `PacketAllDefinitions.CreateProtocol()`。

基础定义和读写实现使用相同的类别：

| 类别 | 内容 |
| --- | --- |
| Session | 握手、连接、状态文本、密码、主机身份。 |
| Players | 玩家信息、装备、移动、生命/魔力、状态、增益、目标与观察。 |
| World | 世界信息、区块内容、时间、世界状态、实体传送及微光动作。 |
| Tiles | 瓦片、墙体、液体、标牌、线路、区块请求与放置。 |
| Items | 物品同步、归属、属性、位置与消失。 |
| Npcs | NPC 同步、增益、名字、商店、坐标、复仇标记及捕获/释放。 |
| Projectiles | 弹幕同步、销毁、传送门与追踪。 |
| Chests | 箱子打开、物品、名称、容量、锁及快速堆叠。 |
| Combat | 伤害、闪避、受伤/死亡及战斗文字。 |
| Social | 聊天、表情、声音、特效与提示。 |
| Modules | `NetModulesPacket` 模块包及模块读写注册。 |
| TileEntities | 瓦片实体共享、交互与展示架/装备架等操作。 |
| Events | 世界事件、任务、成就、入侵进度及事件特效。 |
| Reserved | 未使用的空包与原样保留的 opaque 包。 |

例如，玩家控制结构位于 `Packets/Players/PlayerControlsPacket.cs`，读写方法位于 `Codecs/Players/PlayerControlsPacket.cs`，编译器声明位于 `Definitions/PacketDefinitions.cs`。`TileSectionPacket` 和 `AreaTileChangePacket` 的数据结构位于基础定义文件，partial 读写方法位于对应的 `Codecs/.../<MessageName>Packet.Codec.cs`。

数据包类不再使用 `Payload` 或 `Body` 包装属性，可直接创建和访问：

```csharp
var packet = new PlayerControlsPacket
{
    Player = 3,
    SelectedItem = 5,
    Position = new PacketVector2(1.25f, -2.5f)
};
packet.SelectedItem = 6;
```

成员保留原有类型和默认值，读写函数直接接收成员值，读函数返回单值或具名 tuple。`TileSectionPacket` 的压缩内容直接展开为 StartX、StartY、Width、Height、Tiles、Chests、Signs、TileEntities。空包没有数据成员；opaque 包直接暴露 Bytes。`NetModulesPacket` 直接暴露 ModuleId 和 Data，实际模块分支结构仍保留。颜色、坐标、物品记录、死亡原因和瓦片属于真实子结构，不作为整包外壳。

新增数据包时，将基础定义和读写实现放入对应类别，在 `Definitions/PacketDefinitions.cs` 中登记直接成员、函数和协议目录。包类使用 partial，以便生成成员声明。项目递归包含上述四个目录，无需逐项添加 `Compile Include`。文件名、数据包类型名和 codec 名称均使用 `MessageID` 的具体名称，消息编号只保留在 wire 图注册中。

```powershell
dotnet build .\src\DataPacketDefinition\PacketDesignCompiler.Packets.csproj
dotnet run --project .\tests\PacketDefinitions.Verification.csproj
```

验证入口编译全部 166 份生成源码，用展开前保存的 167 个线格式样例覆盖 165 项定义，比较编码字节并执行读写往返，同时检查成员直接声明和条件字段规则。样例来自本仓库展开前的实现，用于结构重构回归，不代表已完成所有原版协议分支的对照。
