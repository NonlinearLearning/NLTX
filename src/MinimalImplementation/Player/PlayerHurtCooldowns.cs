namespace MinimalImplementation.Server.Player;

/// <summary>The six Version4 hurt cooldown slots; general immunity has its own timer.</summary>
public readonly record struct PlayerHurtCooldowns(
    int TileContactDamage,
    int BossNoCheese,
    int LegacyUnused2,
    int WrongBugNet,
    int Lava,
    int PaladinsShield) {
  public static PlayerHurtCooldowns ForAllTypes(int ticks) {
    return new PlayerHurtCooldowns(ticks, ticks, ticks, ticks, ticks, ticks);
  }

  public PlayerHurtCooldowns AdvanceTick() {
    return new PlayerHurtCooldowns(
      Decrement(TileContactDamage), Decrement(BossNoCheese), Decrement(LegacyUnused2),
      Decrement(WrongBugNet), Decrement(Lava), Decrement(PaladinsShield));
  }

  private static int Decrement(int ticks) {
    return ticks > 0 ? ticks - 1 : 0;
  }
}
