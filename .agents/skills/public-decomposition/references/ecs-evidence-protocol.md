# ECS 证据记录协议

## 成员清单模板

| Member | Declaring Type | Visibility | Read By | Written By | Lifecycle | Access Pattern | State Kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FieldOrProperty` | `Type` | public/private | systems/callers | system/command | create/update/cleanup/persist | shared readers/writers | authoritative/derived/cache/snapshot/compat/presentation | URL or `file:line` | confirmed/partial/missing | Component/System/Query/Adapter/Projection |

## 来源记录

按以下字段记录每次检索：`source`、`version`、`query`、`hits`、`evidence`、`gaps`、`stop_reason`。目录和外部页面均只读。SS14 证据只能支持组织模式；tModLoader 和目标版本源码才可确认 Terraria 字段语义。

## 足够信息判定

- 必须确认：权威状态、公开 API、持久化/网络字段、生命周期字段。
- 可部分确认：纯局部缓存、临时计算值和明确暂缓的实现细节，但必须列入风险表。
- 关键字段为 `missing` 时不得输出最终拆分；输出缺口、已搜索来源和需要调用者补充的具体信息，并停止。
