namespace Terraria.Npc;

/// <summary>
/// Captures a hit until the next NPC AI tick consumes it.
/// </summary>
public sealed class NpcHitStateComponent
{
  public bool JustHit { get; private set; }

  public void CommitHit()
  {
    JustHit = true;
  }

  public bool Consume()
  {
    bool justHit = JustHit;
    JustHit = false;
    return justHit;
  }
}
