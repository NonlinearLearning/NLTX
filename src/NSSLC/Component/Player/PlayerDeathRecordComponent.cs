using System;

namespace Terraria.Player;

// Stores the committed facts from the most recent player death.
// Coin drops, messaging and persistence remain external effects.
public sealed class PlayerDeathRecordComponent
{
  public bool WasPvpDeath { get; internal set; }

  public long LostCoins { get; internal set; }

  public string LostCoinText { get; internal set; } = string.Empty;

  public int PveDeathCount { get; internal set; }

  public int PvpDeathCount { get; internal set; }

  public WorldPosition LastDeathPosition { get; internal set; }

  public DateTime LastDeathTime { get; internal set; }

  public bool ShowLastDeath { get; internal set; }
}
