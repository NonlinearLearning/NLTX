namespace Terraria.Presentation.CombatText;

public static class CombatTextNetworkProjection
{
  public readonly record struct NetworkPayload(
    long EventId,
    int NetworkSourceId,
    string Text,
    CombatTextColorRole ColorRole,
    int? PresentationSlotIndex);

  public static NetworkPayload CreatePayload(
    CombatTextSpawnCommand command,
    int networkSourceId)
  {
    return new NetworkPayload(
      command.EventId,
      networkSourceId,
      command.Text,
      command.ColorRole,
      null);
  }
}
