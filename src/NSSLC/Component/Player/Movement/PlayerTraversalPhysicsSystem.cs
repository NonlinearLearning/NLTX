namespace Terraria.Player.Movement;

// status: implemented-partial
// crossSubsystemOwner: P03 Mount and P07 Movement/Collision need integration review
// frameSchedulingOwner: integration-review
public sealed class PlayerTraversalPhysicsSystem
{
  private const int InitialJumpHeight = 15;
  private const float InitialJumpSpeed = 5.01f;
  private const float InitialMaxFallSpeed = 10f;
  private const float PortalPhysicsMaxFallSpeed = 35f;
  private const float InitialMaxRunSpeed = 3f;
  private const float InitialRunAcceleration = 0.08f;
  private const float InitialRunSlowdown = 0.2f;
  private const float InitialWetDownDashMultiplier = 0.85f;
  private const float InitialShimmerMultiplier = 0.9f;
  private const float MaxFallSpeedPerFrameIncrease = 0.01f;

  public PlayerTraversalPhysicsResult Resolve(in PlayerTraversalPhysicsInput input)
  {
    float gravity = input.DefaultGravity;
    float maxFallSpeed = InitialMaxFallSpeed;
    float maxRunSpeed = InitialMaxRunSpeed;
    float runAcceleration = InitialRunAcceleration;
    float runSlowdown = InitialRunSlowdown;
    int jumpHeight = InitialJumpHeight;
    float jumpSpeed = InitialJumpSpeed;

    if (input.PortalPhysicsEnabled)
    {
      maxFallSpeed = PortalPhysicsMaxFallSpeed;
    }

    if (!input.Shimmering && input.Wet && input.IsPerformingJumpDownDash)
    {
      gravity *= InitialWetDownDashMultiplier;
      maxFallSpeed *= InitialWetDownDashMultiplier;
    }
    else if (input.ShimmerWet || input.Shimmering)
    {
      if (input.Shimmering)
      {
        gravity *= InitialShimmerMultiplier;
        maxFallSpeed *= InitialShimmerMultiplier;
      }
      else
      {
        gravity = 0.15f;
        jumpHeight = 23;
        jumpSpeed = 5.51f;
      }
    }
    else if (input.Wet)
    {
      if (input.HoneyWet)
      {
        gravity = 0.1f;
        maxFallSpeed = 3f;
      }
      else if (input.Merman)
      {
        gravity = 0.3f;
        maxFallSpeed = 7f;
      }
      else if (input.Trident && !input.LavaWet)
      {
        gravity = 0.25f;
        maxFallSpeed = 6f;
        jumpHeight = 25;
        jumpSpeed = 5.51f;
        if (input.ControlUp)
        {
          gravity = 0.1f;
          maxFallSpeed = 2f;
        }
      }
      else
      {
        gravity = 0.2f;
        maxFallSpeed = 5f;
        jumpHeight = 30;
        jumpSpeed = 6.01f;
      }
    }

    if (input.VortexDebuff)
    {
      gravity = 0f;
    }

    maxFallSpeed += MaxFallSpeedPerFrameIncrease;

    return new PlayerTraversalPhysicsResult(
      gravity,
      maxFallSpeed,
      maxRunSpeed,
      runAcceleration,
      runSlowdown,
      jumpHeight,
      jumpSpeed);
  }

  public void CommitMovementState(
    PlayerMovementPhysicsStateComponent state,
    in PlayerTraversalPhysicsResult result)
  {
    ArgumentNullException.ThrowIfNull(state);

    state.Gravity = result.Gravity;
    state.MaxFallSpeed = result.MaxFallSpeed;
    state.MaxRunSpeed = result.MaxRunSpeed;
    state.RunAcceleration = result.RunAcceleration;
    state.RunSlowdown = result.RunSlowdown;
  }
}
