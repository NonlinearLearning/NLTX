# World Clock Representation Decision

WLD stores `_tempTime` as a `Double` and `_tempDayTime` as a `Boolean`
(`Terraria.IO/WorldFile.cs:1312-1313`, `2125-2126`). The reference `Main.UpdateTime()` advances
the value using `time += dayRate` (`Terraria/Main.cs:13525-13527`), so the source type and update
surface do not establish an integral-only persistence contract.

The current authoritative ECS clock intentionally exposes `Int32 TimeOfDay`; its snapshot,
persistence and V1456 SetTime projection use that integer value. There is no source-backed rule
for truncating, rounding or rejecting arbitrary saved fractional WLD values. Therefore WLD
time/day-time import remains fail-closed. The next implementation must either prove the supported
server WLD contract cannot contain fractions, or introduce a versioned fractional representation
and explicitly verify packet, event-boundary and restart behavior.
