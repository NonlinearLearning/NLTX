# WLD Clock Fraction Boundary

## Source Audit

WLD stores `_tempTime` as `Double` and `_tempDayTime` as `Boolean`
(`Terraria.IO/WorldFile.cs:1312-1313, 2125-2126`). Legacy `Main.UpdateTime` advances time using
`time += dayRate` (`Main.cs:13525-13527`), so fractional values are part of the source state
surface.

## Decision

The fractional-clock child has now been implemented and accepted. `WorldClock` and
`WorldClockSnapshot` retain `Double TimeOfDay`; compatibility projection validates the WLD value
without truncation, and persistence format version 28 appends the exact fractional value. The
V1456 `SetTime` packet remains a projection boundary and deliberately quantizes only at protocol
encoding, where the legacy payload is integral.

This does not claim complete `Main.UpdateTime` parity. Global update ordering, legacy random
consumers, client presentation and unsupported event branches remain separate deferred work.

## Focused Evidence

WorldClock focused verification passes fixed rate, day/night boundaries, full cycle, pause,
simulation tick phase, snapshot continuation and fractional persistence. WLD V319 verification
passes the historical version matrix and recorded differential oracle. Scoped diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-wld-clock-fraction-boundary/20260822-002106/`

The source audit and original RED fixture are recorded at
`Build/diagnostics/main-migration/task-9-wld-clock-fraction-boundary/20260823-123000/`.
The current focused rerun passes the WorldClock and WLD V319 verifiers. MainBoundary and root
Release remain separate reduced-scope gates; broad suites remain outside this card.
