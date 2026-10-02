namespace NLTX.PlayerInputGameplay.Movement;

public sealed class PlayerMovementCapabilityComponent
{
  public int RocketTime { get; private set; }

  public float WingTime { get; private set; }

  public int RocketDelay { get; private set; }

  public int RocketDelay2 { get; private set; }

  public bool JumpAgainCloud { get; private set; }

  public bool JumpAgainSandstorm { get; private set; }

  public bool JumpAgainBlizzard { get; private set; }

  public bool JumpAgainFart { get; private set; }

  public bool JumpAgainSail { get; private set; }

  public bool JumpAgainUnicorn { get; private set; }

  public bool MountPreventedFlight { get; private set; }

  public bool MountPreventedExtraJumps { get; private set; }

  public void Set(
    int rocketTime,
    float wingTime,
    int rocketDelay,
    int rocketDelay2,
    bool jumpAgainCloud,
    bool jumpAgainSandstorm,
    bool jumpAgainBlizzard,
    bool jumpAgainFart,
    bool jumpAgainSail,
    bool jumpAgainUnicorn,
    bool mountPreventedFlight,
    bool mountPreventedExtraJumps)
  {
    if (rocketTime < 0 || rocketDelay < 0 || rocketDelay2 < 0 || wingTime < 0f || float.IsNaN(wingTime) || float.IsInfinity(wingTime))
    {
      throw new ArgumentOutOfRangeException(nameof(rocketTime));
    }

    RocketTime = rocketTime;
    WingTime = wingTime;
    RocketDelay = rocketDelay;
    RocketDelay2 = rocketDelay2;
    JumpAgainCloud = jumpAgainCloud;
    JumpAgainSandstorm = jumpAgainSandstorm;
    JumpAgainBlizzard = jumpAgainBlizzard;
    JumpAgainFart = jumpAgainFart;
    JumpAgainSail = jumpAgainSail;
    JumpAgainUnicorn = jumpAgainUnicorn;
    MountPreventedFlight = mountPreventedFlight;
    MountPreventedExtraJumps = mountPreventedExtraJumps;
  }
}
