using System;

namespace Terraria.Projectile;

public struct ProjectileNetworkStateComponent
{
  public ProjectileNetworkStateComponent()
  {
    NetworkImportant = false;
    PrimaryUpdatePending = false;
    SecondaryUpdatePending = false;
    NetSpam = 0;
    SectionSyncSkippedForPlayer = new bool[255];
    SendRequested = false;
  }

  public ProjectileNetworkStateComponent(
    int playerCapacity = 255,
    bool networkImportant = false,
    bool primaryUpdatePending = false,
    bool secondaryUpdatePending = false,
    int netSpam = 0,
    bool sendRequested = false)
  {
    if (playerCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    if (netSpam < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(netSpam));
    }

    NetworkImportant = networkImportant;
    PrimaryUpdatePending = primaryUpdatePending;
    SecondaryUpdatePending = secondaryUpdatePending;
    NetSpam = netSpam;
    SectionSyncSkippedForPlayer = new bool[playerCapacity];
    SendRequested = sendRequested;
  }

  public bool NetworkImportant;
  public bool PrimaryUpdatePending;
  public bool SecondaryUpdatePending;
  public int NetSpam;
  public bool[] SectionSyncSkippedForPlayer;
  public bool SendRequested;
}
