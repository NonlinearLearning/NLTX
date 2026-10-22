using MinimalImplementation.Contracts;

namespace MinimalImplementation.Modules.Creative;

public sealed record CreativePowerEffect(
  ushort PowerId,
  byte RequestingPlayerId,
  CreativePowerWireMessage Message,
  byte AffectedPlayerId,
  CommitVersion CommitVersion) {
  /// <summary>
  /// Ordered server broadcasts. The original Message remains the committed owner operation.
  /// </summary>
  public IReadOnlyList<CreativePowerWireMessage> CaptureBroadcastMessages() {
    if (Message.ValueKind is CreativePowerValueKind.Button
        or CreativePowerValueKind.PerPlayerSlider) {
      return Array.Empty<CreativePowerWireMessage>();
    }
    if (Message.ValueKind != CreativePowerValueKind.PerPlayerToggleSync) {
      return Array.AsReadOnly(new[] { Message });
    }

    // Clients skip their own slot on action0; the server sends action1 for every slot instead.
    bool[] states = Message.CopyPlayerStates();
    CreativePowerWireMessage[] messages = new CreativePowerWireMessage[states.Length];
    for (int playerId = 0; playerId < states.Length; playerId++) {
      messages[playerId] = CreativePowerWireMessage.PerPlayerToggle(
          PowerId, (byte)playerId, states[playerId]);
    }
    return Array.AsReadOnly(messages);
  }
}
