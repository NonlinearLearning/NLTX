namespace Terraria.Npc;

// status: partial
// sourceMembers: catchItem, releaseOwner
// crossSubsystemOwner: capture and release integration-review
public sealed class NpcCatchStateComponent
{
  public NpcCatchStateComponent(short catchItem = 0, short releaseOwner = -1)
  {
    CatchItem = catchItem;
    ReleaseOwner = releaseOwner;
  }

  public short CatchItem { get; }

  public short ReleaseOwner { get; }
}
