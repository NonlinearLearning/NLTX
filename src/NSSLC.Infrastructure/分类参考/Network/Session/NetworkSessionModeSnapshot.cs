namespace Terraria.Network.Session;

public sealed record NetworkSessionModeSnapshot(
  NetworkSessionMode CurrentMode,
  NetworkSessionMode TargetMode,
  bool HasPendingTransition,
  bool GetIp,
  bool MenuMultiplayer,
  bool MenuServer,
  int NetPlayCounter,
  int LastItemUpdate,
  int MaxItemUpdates);
