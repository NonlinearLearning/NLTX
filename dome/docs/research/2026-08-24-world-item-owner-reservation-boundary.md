# World-Item Owner Reservation Boundary

The legacy world-item route has one independently recoverable authorization predicate: a reserved
player may pick up the item, while another player may not. `255` is the unreserved sentinel.

Source evidence:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`, SHA256
  `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`, lines `13162-13194`.
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldItem.cs`, SHA256
  `A220C1C7AFDA67DDAFD296DA3E8EE14B8B8E1CC37492B01BF089080195D752CD`, lines `257-345`.

Accepted ECS predicate:

- `ReservedPlayerId == 255` permits pickup by any valid player;
- a reserved item permits only the matching `PlayerHandle.Value`;
- invalid reservation ids fail closed.

The owner state is represented by `ItemWorldStateComponent.ReservedPlayerId` and
`ReservationAgeTicks`; pickup consumes the predicate through `WorldItemPickupSystem` before any
inventory mutation.

Deferred because the current ECS has no source-backed equivalent for the full `FindOwner()` choice:

- nearest-player/item-space owner selection;
- inactive-owner re-evaluation and the 300-tick scheduling loop;
- network reservation messages and `timeItemSlotCannotBeReusedFor`;
- shimmer, enemy pickup and client presentation branches.

This card therefore accepts authorization only and does not claim complete item ownership parity.
