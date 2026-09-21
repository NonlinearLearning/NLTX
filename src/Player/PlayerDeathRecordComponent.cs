using System;

namespace Terraria.Player;

// Owns the committed facts from the most recent player death.
// Death resolution, coin drops, messaging and persistence remain external effects.
public sealed class PlayerDeathRecordComponent
{
  public long LostCoins { get; set; }

  public string LostCoinText { get; set; } = string.Empty;

  public int PveDeathCount { get; set; }

  public int PvpDeathCount { get; set; }

  public WorldPosition LastDeathPosition { get; set; }

  public DateTime LastDeathTime { get; set; }

  public bool ShowLastDeath { get; set; }
}
