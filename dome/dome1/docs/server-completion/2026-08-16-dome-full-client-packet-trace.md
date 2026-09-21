# Dome 服务端与完整客户端数据包交流统计

日期：2026-08-16

## 会话范围

本报告统计完整 Terraria 客户端通过新的透明记录器访问正在运行的 Dome 服务端的
一次连接。运行中的 Dome 服务端监听 `127.0.0.1:7778`；记录器独立监听
`127.0.0.1:7779` 并逐字节转发到 `7778`。原有 `7777 -> 7778` 代理和 Dome 服务
进程均未停止或修改。

完整客户端使用：

```text
D:\TRbackup\客户端\bin\Debug\net40\Terraria.exe
```

客户端以 `join-stable` 自动化场景连接 `127.0.0.1:7779`。记录器、原始帧和自动
汇总均位于
[`Build/diagnostics/dome-full-client-20260816-145622`](../../Build/diagnostics/dome-full-client-20260816-145622)。

## 统计口径

- 仅统计 `trace.jsonl` 中唯一连接 `connectionId=1` 的 `frame` 事件。
- `C2S` 表示客户端到 Dome 服务端，`S2C` 表示 Dome 服务端到客户端。
- 每个完整帧按第一个载荷字节的 `messageId` 分类；帧长度包括 2 字节小端长度前缀。
- 不将 `listen`、`connect` 或双方关闭时的 `socket_error` 计为数据包。
- 汇总器逐帧验证了长度前缀、完整帧长度、`messageId` 和载荷；无效帧数为 0。

## 双向汇总

| 方向 | 完整数据包数量 | 去重后的 `messageId` 数量 | 占全部完整帧比例 |
| --- | ---: | ---: | ---: |
| C2S | 358 | 9 | 99.72% |
| S2C | 1 | 1 | 0.28% |
| 合计 | 359 | 10 | 100.00% |

两方向的观察到的 `messageId` 没有交集，因此方向类型数之和等于该会话的全局
去重类型数 10。

## 客户端到 Dome 服务端

| `messageId` | 协议名称 | 数量 | 帧长度范围（字节） |
| ---: | --- | ---: | ---: |
| 1 | `Hello` | 1 | 15..15 |
| 4 | `SyncPlayer` | 1 | 54..54 |
| 5 | `SyncEquipment` | 350 | 12..12 |
| 6 | `RequestWorldData` | 1 | 3..3 |
| 16 | `PlayerLifeMana` | 1 | 8..8 |
| 42 | `ItemRotationAndAnimation` | 1 | 8..8 |
| 50 | `PlayerBuffs` | 1 | 6..6 |
| 68 | `PlayerUuid` | 1 | 40..40 |
| 147 | `SyncLoadout` | 1 | 7..7 |
| **合计** | - | **358** | - |

其中 `SyncEquipment` 为 350 包，占 C2S 的 97.77%。

## Dome 服务端到客户端

| `messageId` | 协议名称 | 数量 | 帧长度范围（字节） |
| ---: | --- | ---: | ---: |
| 3 | `SetUserSlot` | 1 | 5..5 |
| **合计** | - | **1** | - |

因此本次观测中，Dome 在接收客户端的握手、玩家资料和装备同步后，仅向完整客户端
返回一包 `SetUserSlot`，没有观察到后续的 `WorldData`、`TileSection`、`InitialSpawn` 或
其他服务端初始化/复制帧。

## 会话结果和边界

客户端自动化结果为：

```json
{
  "scenario": "join-stable",
  "success": false,
  "message": "Timed out waiting for scenario completion.",
  "elapsedMilliseconds": 60008,
  "playerSlot": 4,
  "netMode": 1
}
```

这证明完整客户端已进入联网模式并收到分配的玩家槽位 `4`，但在 60,008 毫秒内未达到
`join-stable` 的完成条件。本报告是这一实际超时会话的包交流统计，不将其描述为完成
稳定入服。

轨迹共有 363 条事件：359 条完整帧、1 条 `listen`、1 条 `connect` 和 2 条
`socket_error` 终止事件。该连接以 `socket_error` 结束；客户端最后发送
`RequestWorldData` (`messageId=6`)。未发现不完整帧或无效长度事件。

## 可复查证据

- [原始逐帧记录](../../Build/diagnostics/dome-full-client-20260816-145622/trace.jsonl)
- [机器可读汇总](../../Build/diagnostics/dome-full-client-20260816-145622/summary.json)
- [自动生成的方向/ID 汇总](../../Build/diagnostics/dome-full-client-20260816-145622/summary.md)
- [完整客户端自动化结果](../../Build/diagnostics/dome-full-client-20260816-145622/client/join-stable.json)
- [Dome 的协议包目录](../../src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageCatalog.cs)
- [Dome 的消息 ID 枚举](../../src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageId.cs)

汇总命令如下，退出码为 `0`：

```powershell
python Build\diagnostics\summarize_full_session_trace.py `
  Build\diagnostics\dome-full-client-20260816-145622\trace.jsonl `
  --source-root D:\TRbackup\Version4物理删除了某些文件 `
  --json-out Build\diagnostics\dome-full-client-20260816-145622\summary.json `
  --markdown-out Build\diagnostics\dome-full-client-20260816-145622\summary.md
```
