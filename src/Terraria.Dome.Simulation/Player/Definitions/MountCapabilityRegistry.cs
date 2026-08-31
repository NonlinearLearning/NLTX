using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Player.Definitions;

public static class MountCapabilityRegistry
{
  private static readonly FrozenSet<int> CartTypes = new[]
  {
    6, 11, 13, 15, 16, 18, 19, 20, 21, 22, 24, 25, 26, 27,
    28, 29, 30, 31, 32, 33, 34, 35, 36, 38, 39, 51, 53
  }.ToFrozenSet();

  private static readonly FrozenSet<int> CanDashTypes = new[] { 56, 57, 58, 59, 60, 61, 62, 63 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> DoesNotHoldItemsTypes = new[] { 55, 56, 61 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> DismountsOnItemUseTypes = new[] { 55, 56, 61 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> TransformationMountTypes = new[] { 52, 54, 55, 56, 61 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> HiddenPlayerMountTypes = new[] { 52, 54, 55, 56, 61 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> HookCompatibleMountTypes = new[] { 54, 57, 58, 59, 60 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> CrowdControlDismountImmuneTypes = new[] { 55, 56, 61 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> FramePreservingMountTypes = new[] { 57, 58, 59, 60 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> RollerSkateMountTypes = new[] { 57, 58, 59, 60 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> WingEnabledMountTypes = new[] { 57, 58, 59, 60 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> MinecartTrackMountTypes = new[] { 57, 58, 59, 60 }
    .ToFrozenSet();

  private static readonly FrozenSet<int> ExtraJumpBlockingMountTypes = new[]
  {
    5, 6, 7, 8, 11, 12, 13, 16, 23, 44, 49, 56, 61
  }.ToFrozenSet();

  private static readonly FrozenSet<int> HoverMountTypes = new[]
  {
    5, 7, 8, 12, 23, 44, 48, 49, 56, 61
  }.ToFrozenSet();

  public static bool IsCart(int mountType)
  {
    return CartTypes.Contains(mountType);
  }

  public static bool CanDash(int mountType)
  {
    return CanDashTypes.Contains(mountType);
  }

  public static bool DoesNotHoldItems(int mountType)
  {
    return DoesNotHoldItemsTypes.Contains(mountType);
  }

  public static bool DismountsOnItemUse(int mountType)
  {
    return DismountsOnItemUseTypes.Contains(mountType);
  }

  public static bool IsTransformationMount(int mountType)
  {
    return TransformationMountTypes.Contains(mountType);
  }

  public static bool HidesPlayer(int mountType)
  {
    return HiddenPlayerMountTypes.Contains(mountType);
  }

  public static bool CanUseHooks(int mountType)
  {
    return HookCompatibleMountTypes.Contains(mountType);
  }

  public static bool DoesNotDismountWhenCrowdControlled(int mountType)
  {
    return CrowdControlDismountImmuneTypes.Contains(mountType);
  }

  public static bool DoesNotOverrideBodyFrames(int mountType)
  {
    return FramePreservingMountTypes.Contains(mountType);
  }

  public static bool DoesNotOverrideLegFrames(int mountType)
  {
    return FramePreservingMountTypes.Contains(mountType);
  }

  public static bool DoesNotOverrideBackpackDraw(int mountType)
  {
    return FramePreservingMountTypes.Contains(mountType);
  }

  public static bool IsRollerSkates(int mountType)
  {
    return RollerSkateMountTypes.Contains(mountType);
  }

  public static bool CanUseWings(int mountType)
  {
    return WingEnabledMountTypes.Contains(mountType);
  }

  public static bool CanRideMinecartTracks(int mountType)
  {
    return MinecartTrackMountTypes.Contains(mountType);
  }

  public static bool BlocksExtraJumps(int mountType)
  {
    return ExtraJumpBlockingMountTypes.Contains(mountType);
  }

  public static bool UsesHover(int mountType)
  {
    return HoverMountTypes.Contains(mountType);
  }

  public static bool CanUseHoverFrame(int mountType, int frameState)
  {
    if (!UsesHover(mountType))
    {
      return false;
    }

    return mountType != 49 || frameState == 4;
  }

  public static bool CanFly(int mountType)
  {
    return mountType != 48 && GetFlightTimeMax(mountType) > 0;
  }

  public static bool CanFly(int mountType, bool allowedToFly)
  {
    if (mountType == 54)
    {
      return allowedToFly;
    }

    return CanFly(mountType);
  }

  public static bool CanUseFlightFrame(int mountType, int frameState)
  {
    if (!CanFly(mountType))
    {
      return false;
    }

    return mountType switch
    {
      56 => frameState is 2 or 3,
      61 => frameState is 2 or 3 or 4,
      _ => frameState == 4
    };
  }

  public static int GetFlightTimeMax(int mountType)
  {
    return mountType switch
    {
      0 or 2 => 160,
      5 or 7 or 8 or 12 or 23 or 44 or 48 or 56 or 61 => 320,
      50 => 80,
      _ => 0
    };
  }

  public static int GetFatigueMax(int mountType)
  {
    return mountType switch
    {
      5 or 7 or 8 or 12 or 23 or 44 or 49 or 56 or 61 => 320,
      _ => 0
    };
  }

  public static float GetFallDamageMultiplier(int mountType)
  {
    return mountType switch
    {
      1 => 0.8f,
      3 => 0.5f,
      4 or 6 or 8 or 11 or 13 or 16 => 1.0f,
      10 or 14 or 17 or 37 or 47 => 0.2f,
      43 => 0.25f,
      45 => 0.1f,
      50 => 0.5f,
      52 or 54 or 55 => 0.1f,
      40 or 41 or 42 or 62 or 63 => 0.5f,
      57 or 58 or 59 or 60 => 1.0f,
      _ => 0.0f
    };
  }

  public static float GetRunSpeed(int mountType)
  {
    return mountType switch
    {
      0 => 5.5f,
      1 or 3 or 10 or 47 => 4.0f,
      2 => 5.0f,
      4 or 5 or 12 or 49 => 2.0f,
      6 or 11 or 15 or 16 or 18 or 19 or 20 or 21 or 22 or 24 or 25 or 26 or 27 or
        28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 38 or 51 or 53 => 13.0f,
      7 or 9 or 14 or 17 or 46 or 48 => 8.0f,
      8 or 37 or 39 => 6.0f,
      13 => 10.0f,
      23 => 9.0f,
      43 => 5.0f,
      44 => 3.0f,
      45 => 12.0f,
      50 => 5.5f,
      52 => 9.5f,
      40 or 41 or 42 => 3.0f,
      54 or 55 or 56 or 61 => 4.5f,
      57 or 58 or 59 or 60 => 7.5f,
      62 or 63 => 3.0f,
      _ => 0.0f
    };
  }

  public static float GetDashSpeed(int mountType)
  {
    return mountType switch
    {
      0 or 10 => 12.0f,
      1 => 7.8f,
      2 or 40 or 41 or 42 or 62 or 63 => 9.0f,
      3 => 4.0f,
      4 => 5.0f,
      5 => 2.0f,
      6 or 11 or 15 or 16 or 18 or 19 or 20 or 21 or 22 or 24 or 25 or 26 or 27 or
        28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 38 or 51 or 53 => 13.0f,
      7 or 9 or 46 or 48 => 8.0f,
      8 => 4.0f,
      12 or 49 => 1.0f,
      13 => 10.0f,
      23 => 9.0f,
      39 or 44 => 6.0f,
      45 => 16.0f,
      47 => 12.0f,
      50 => 5.5f,
      54 or 55 => 7.5f,
      56 or 61 => 4.5f,
      57 or 58 or 59 or 60 => 7.5f,
      _ => 0.0f
    };
  }

  public static float GetAcceleration(int mountType)
  {
    return mountType switch
    {
      0 => 0.09f,
      1 => 0.13f,
      2 or 4 => 0.08f,
      3 or 52 => 0.18f,
      5 or 7 or 8 or 23 => 0.16f,
      6 or 11 or 15 or 16 or 18 or 19 or 20 or 21 or 22 or 24 or 25 or 26 or 27 or
        28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 38 or 51 or 53 => 0.04f,
      9 or 46 => 0.4f,
      10 or 47 => 0.3f,
      12 or 48 => 0.2f,
      13 => 0.03f,
      14 or 17 => 0.25f,
      37 => 0.15f,
      39 => 0.02f,
      40 or 41 or 42 => 0.25f,
      43 => 0.1f,
      44 => 0.12f,
      45 => 0.5f,
      49 or 50 => 0.2f,
      54 or 55 => 0.15f,
      56 or 61 => 0.2f,
      57 or 58 or 59 or 60 => 0.3f,
      62 or 63 => 0.32f,
      _ => 0.0f
    };
  }

  public static int GetJumpHeight(int mountType)
  {
    return mountType switch
    {
      0 => 17,
      1 => 15,
      2 => 10,
      3 or 52 => 12,
      4 => 12,
      5 or 7 or 8 or 10 or 23 or 47 or 50 => 10,
      6 or 11 or 15 or 16 or 18 or 19 or 20 or 21 or 22 or 24 or 25 or 26 or 27 or
        28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 38 or 51 or 53 => 15,
      9 => 22,
      12 => 4,
      13 => 12,
      14 or 17 => 20,
      37 or 45 => 14,
      43 => 8,
      44 => 3,
      46 => 8,
      48 => 5,
      49 => 4,
      40 or 41 or 42 => 6,
      54 or 55 => 15,
      56 or 61 => 8,
      57 or 58 or 59 or 60 => 14,
      62 or 63 => 8,
      _ => 0
    };
  }

  public static float GetJumpSpeed(int mountType)
  {
    return mountType switch
    {
      0 => 5.31f,
      1 => 5.01f,
      2 or 54 or 55 => 6.01f,
      3 => 8.25f,
      4 => 3.7f,
      5 or 7 or 8 or 23 => 4.0f,
      6 or 11 or 13 or 15 or 16 or 18 or 19 or 20 or 21 or 22 or 24 or 25 or 26 or
        27 or 28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 38 or 51 or 53 => 5.15f,
      9 => 10.01f,
      10 or 14 or 17 or 47 or 52 => 8.01f,
      12 or 49 => 3.0f,
      37 => 6.01f,
      39 => 6.01f,
      43 => 8.0f,
      44 => 1.0f,
      45 => 7.0f,
      46 => 9.01f,
      48 => 6.0f,
      50 => 7.25f,
      56 or 61 => 5.0f,
      57 or 58 or 59 or 60 => 7.0f,
      62 or 63 => 8.01f,
      40 or 41 or 42 => 7.01f,
      _ => 0.0f
    };
  }

  public static float GetSwimSpeed(int mountType)
  {
    return mountType switch
    {
      4 => 10.0f,
      8 => 4.0f,
      12 => 16.0f,
      44 => 3.0f,
      48 => 8.0f,
      49 => 14.0f,
      _ => 0.0f
    };
  }

  public static int GetHeightBoost(int mountType)
  {
    return mountType switch
    {
      0 or 1 or 2 or 3 or 50 => 20,
      4 => 26,
      5 or 7 or 8 or 9 or 17 => 16,
      6 or 11 or 13 or 16 or 51 or 53 => 10,
      10 or 47 => 34,
      40 or 41 or 42 => 34,
      12 or 48 => 14,
      14 or 49 => 8,
      23 => 0,
      37 or 43 => 12,
      44 => 24,
      45 => 25,
      46 => 0,
      54 or 55 => 14,
      56 or 61 => 8,
      62 or 63 => 4,
      _ => 0
    };
  }
}
