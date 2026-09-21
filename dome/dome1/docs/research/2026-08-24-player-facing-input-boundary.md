# Player Facing Input Boundary

The V1456 control projection represents facing as a boolean and the server converts it to the
direction domain `-1` or `1`. Simulation inputs also permit `0` to mean “retain current facing”.
Any other integer is forged/unrepresentable and must be rejected before the input component is
mutated.

The typed guard is implemented in `PlayerInputApplySystem`; validation occurs for the whole batch
before existing input state is cleared or applied.

Sources:

- `src/Terraria.Dome.Protocol.V1456/Packets/PlayerControlIntent.cs`, SHA256
  `C8444007AF03C90A087BE3EF7761F90585A9E19C44A74CB9EDD3CFA18E5038F6`.
- `src/Terraria.Dome.Simulation/Simulation/PlayerInput.cs` and
  `PlayerInputApplySystem` current authority route.

Accepted: `Facing` in `-1, 0, 1`; rejected: `Facing < -1` or `Facing > 1`. This does not claim
legacy client release/timing parity or a persisted replay envelope.
