namespace Terraria.Npc;

/// <summary>
/// Defines which host modes may execute authoritative NPC AI.
/// </summary>
public static class NpcAiAuthorityGate
{
  public const int SinglePlayerNetMode = 0;
  public const int ClientNetMode = 1;
  public const int ServerNetMode = 2;

  public static bool IsAuthoritative(int netMode)
  {
    return netMode == SinglePlayerNetMode || netMode == ServerNetMode;
  }

  public static bool IsClient(int netMode)
  {
    return netMode == ClientNetMode;
  }
}
