# World-Item Shimmer and Encumbrance Pickup Boundary

The remaining legacy `GrabItems` guard is not an independently implementable generic pickup
predicate in the current ECS.

Source:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`, SHA256
  `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`, lines `19833-19872`.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldItem.cs`, SHA256
  `A220C1C7AFDA67DDAFD296DA3E8EE14B8B8E1CC37492B01BF089080195D752CD`, lines `500-523`.

The source guard combines:

- `shimmerTime == 0` and `shimmered` velocity threshold;
- `noGrabDelay == 0`;
- `CanAcceptItemIntoInventory`, whose `preventAllItemPickups` branch depends on
  `ItemID.Sets.IgnoresEncumberingStone`;
- enemy pickup timers and item-type-specific `IsACoin`/special pickup rules.

Current ECS has the generic pickup delay and inventory capacity route, but has no authoritative
shimmer state/velocity contract, encumbrance definition table, or enemy pickup owner. Adding a
boolean or defaulting missing item definitions would be lossy. The branch is therefore deferred;
the accepted reservation authorization predicate remains valid independently.
