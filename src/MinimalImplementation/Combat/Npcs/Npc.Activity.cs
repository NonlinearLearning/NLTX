using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class Npc {
  internal const int InitialActivityTime = 750;

  public int ActivityTimeLeft { get; private set; } = InitialActivityTime;

  internal void EncourageDespawn() {
    ActivityTimeLeft = Math.Min(ActivityTimeLeft, 10);
  }

  internal void ApplyActivity(int timeLeft, bool despawn, CommitVersion version) {
    ActivityTimeLeft = timeLeft;
    Revision++;
    CommitVersion = version;
    if (despawn) {
      Life = 0;
      IsDirty = true;
    }
    ClearJustHit();
  }
}
