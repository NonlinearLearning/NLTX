# Entity 身份根与 typed projection

**Status: accepted**

服务器权威 ECS 实体使用由 Spawn/Commit 流程生成的 `EntityUuid` 作为单一运行时身份根；它只在一个实体实例生命周期内有效，实体销毁或重建即失效。`whoAmI`、Projectile protocol identity、`PlayerHandle`、`NpcHandle`、`ReplicationId`、账户 UUID 和持久化身份保留为各自作用域内的强类型投影，由 Registry/Adapter 映射到根身份，不能互相替代。该选择避免把协议槽位、账户归属、复制游标和运行时追踪耦合到一个 UUID，同时允许按域迁移旧字段并显式处理解析失败。
