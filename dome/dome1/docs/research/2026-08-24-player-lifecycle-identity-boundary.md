# Player Lifecycle Identity Boundary

`PlayerLifecycleSystem.Advance` now skips stale ownership entries whose Arch entity is absent or
destroyed. The guard runs before `PlayerLifecycleComponent` access, so a stale dictionary entry
cannot abort the lifecycle tick or schedule a forged respawn. Live player entries retain the
existing negative-timer normalization and respawn countdown behavior.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`, SHA256
`AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`, active/dead lifecycle
fields around lines `478`, `1134-1136` and active-player guards around `3501-3510`. This card
establishes the ECS identity precondition without claiming full `UpdateDead` parity.

Accepted: live entity, typed lifecycle access and existing countdown semantics. Rejected: stale or
destroyed entity mapping before component access. Deferred: full death/respawn presentation, network,
inventory and client branches.
