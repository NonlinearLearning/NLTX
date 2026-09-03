# Player Input Envelope Boundary

The current player input route already owns two narrow guards: unknown player handles are rejected
and one input per player is required in a tick batch. Active-player filtering prevents inactive
players from mutating movement/selection state.

The remaining legacy control surface is not a missing generic movement predicate. The source
`Player` control flags include client timing/release state and direction transitions; the V1456
packet projects those flags into a bounded `PlayerControlIntent`, while `SimulationInputBatch` is a
per-tick value list with no persisted sequence or replay envelope.

Sources:

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`, SHA256
  `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`.
- `src/Terraria.Dome.Protocol.V1456/Packets/PlayerControlIntent.cs`, SHA256
  `C8444007AF03C90A087BE3EF7761F90585A9E19C44A74CB9EDD3CFA18E5038F6`.
- `src/Terraria.Dome.Simulation/Simulation/SimulationInputBatch.cs`, SHA256
  `2DD0C70A59930518233A3A5D65D58A721256C4F241BECCD26ACFBE10E7DE2D38`.

Disposition: do not add a second input queue or fabricate a replay sequence inside Simulation.
Protocol command sequencing remains a Server concern; client-only release/timing behavior and a
standalone persisted input envelope remain deferred.
