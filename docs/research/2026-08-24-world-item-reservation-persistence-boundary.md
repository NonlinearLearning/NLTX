# World-Item Reservation Persistence Boundary

The owner reservation predicate is accepted as an in-memory pickup guard, but there is no
source-backed restart or WLD persistence contract for the reservation itself.

Evidence:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldItem.cs`, SHA256
  `A220C1C7AFDA67DDAFD296DA3E8EE14B8B8E1CC37492B01BF089080195D752CD`, lines `183-204`:
  `ResetStats` resets reservation age and `SetDefaultsBringOver` restores player id `255`.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
  `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, lines `13175-13194`:
  reservation is maintained by the runtime tick loop.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria.IO\WorldFile.cs`, SHA256
  `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289`, 4064 lines:
  the WLD item serialization surface contains chest `Item` values but no
  `playerIndexTheItemIsReservedFor` or `timeSinceTheItemHasBeenReservedForSomeone` field.

Disposition:

- Do not append reservation fields to the Dome persistence format as if they were durable world
  facts. That would invent restart semantics not present in the source save contract.
- New world-item spawn and restored legacy-compatible snapshots use the unreserved `255` state.
- A future persistence card requires an explicit server-session/restart contract and must not infer
  it from WLD files.

This is a qualified deferred branch, not a failure of the accepted in-memory authorization guard.
