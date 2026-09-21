# V1456 Unused67 Boundary

- Source anchor: `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:3347-3350`
  contains message `67` only in a no-op case; `NetMessage.cs` has no message `67` write branch.
- Current owner: no runtime owner by design. `TerrariaMessageCatalog` names the slot `Unused67`
  and keeps it `Unsupported`; no packet type or state transition is invented.
- State transition: none; inbound/outbound message 67 remains isolated at the catalog boundary.
- Verifier: `Test/Terraria.Dome.Protocol.Compatibility.Verification` checks the exact catalog name
  and explicit `Unsupported` status.
- Diagnostic: protocol build exit `0`, zero warnings/errors; compatibility verifier exit `0`.

This closes the evidence boundary for message 67 as a legacy no-op; it does not claim a wire
handler or complete 162-message parity.
