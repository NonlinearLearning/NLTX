using System.Numerics;
using Terraria.Npc;

namespace Terraria.NpcAi.Verification;

internal static class NpcServantOfCthulhuProfileVerification
{
  public static void Run()
  {
    Require(NpcServantOfCthulhuProfile.CanHandle(5, 5, 5) &&
      !NpcServantOfCthulhuProfile.CanHandle(5, 5, 4),
      "Servant of Cthulhu must bind only to type=5, netID=5, and aiStyle=5.");

    NpcServantOfCthulhuProfileInput input = new(
      TypeId: 5,
      NetId: 5,
      AiStyle: 5,
      Position: Vector2.Zero,
      Velocity: new Vector2(-1f, 0f),
      TargetCenter: new Vector2(160f, 0f),
      Width: 20,
      Height: 20);
    NpcServantOfCthulhuProfileResult result =
      NpcServantOfCthulhuProfile.Evaluate(in input);
    Require(result.IsSupported &&
      Vector2.DistanceSquared(result.Velocity, new Vector2(-0.94f, -0.03f)) < 1e-12f,
      "Servant steering must grid-align its target and apply source acceleration with reversal " +
      $"help. Actual velocity: {result.Velocity}.");

    NpcServantOfCthulhuProfileInput zeroDistance = input with
    {
      Velocity = new Vector2(1.25f, -0.75f),
      TargetCenter = new Vector2(10f, 10f),
    };
    NpcServantOfCthulhuProfileResult zeroDistanceResult =
      NpcServantOfCthulhuProfile.Evaluate(in zeroDistance);
    Require(zeroDistanceResult.Velocity == zeroDistance.Velocity,
      "A target at the same eight-pixel grid cell must preserve the current velocity.");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
