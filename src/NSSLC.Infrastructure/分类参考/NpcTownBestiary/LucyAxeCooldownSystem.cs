namespace Terraria.NpcTownBestiary;

public sealed class LucyAxeCooldownSystem
{
  public void Tick(LucyAxeCooldownComponent cooldown)
  {
    ArgumentNullException.ThrowIfNull(cooldown);
    cooldown.Tick();
  }
}
