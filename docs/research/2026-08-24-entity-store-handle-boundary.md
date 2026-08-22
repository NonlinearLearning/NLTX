# Entity Store Handle Boundary

`PlayerStore.Add` and `NpcStore.Add` now reject default or non-positive domain handles before
registering an Arch entity. This closes a direct ownership-map bypass: callers cannot create a
runtime entry that is unreachable through the authoritative positive identity domain. Duplicate
positive handles remain rejected, and entity mapping behavior is otherwise unchanged.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, player/NPC array capacities
around lines `647`, `657`, `1126` and active-entry initialization around `3682-3688`, `3878-3880`.
This card covers only typed identity ownership; it does not claim legacy array parity.

Accepted: positive `PlayerHandle`/`NpcHandle` and duplicate-free map ownership. Rejected: default or
non-positive handle before map mutation. Deferred: full slot allocation, lifecycle and client/network
behavior.
