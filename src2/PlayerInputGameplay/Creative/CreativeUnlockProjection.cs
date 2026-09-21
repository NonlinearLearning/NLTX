namespace NLTX.PlayerInputGameplay.Creative;

public readonly record struct CreativeUnlockNotification(int ItemId, int Revision);

public sealed class CreativeUnlockProjection
{
  public IReadOnlyList<CreativeUnlockNotification> Project(CreativeUnlockProgressComponent progress)
  {
    ArgumentNullException.ThrowIfNull(progress);
    return progress.DrainNewlyUnlocked()
      .Select(itemId => new CreativeUnlockNotification(itemId, progress.LastEditId))
      .ToArray();
  }
}
