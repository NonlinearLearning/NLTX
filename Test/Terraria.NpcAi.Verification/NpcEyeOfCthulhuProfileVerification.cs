using System.Numerics;
using Terraria.Npc;

namespace Terraria.NpcAi.Verification;

internal static class NpcEyeOfCthulhuProfileVerification
{
  public static void Run()
  {
    VerifyIdentityAndPhaseCounters();
    VerifyLifeThreshold();
    VerifyTargetLossAndExitOrder();
    VerifyDashAndEffectOrder();
    VerifyDashCycle();
    VerifyOpeningServantSpawn();
    VerifyTransformationAndExpertServants();
  }

  private static void VerifyIdentityAndPhaseCounters()
  {
    Require(NpcEyeOfCthulhuProfile.CanHandle(4, 4, 4),
      "Eye of Cthulhu type 4 must bind to style 4.");
    Require(!NpcEyeOfCthulhuProfile.CanHandle(5, 4, 4) &&
      !NpcEyeOfCthulhuProfile.CanHandle(4, 5, 4) &&
      !NpcEyeOfCthulhuProfile.CanHandle(4, 4, 15),
      "The profile must reject nearby identities and other styles.");

    NpcEyeOfCthulhuProfileResult beforeBoundary = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(state: new NpcEyeOfCthulhuProfileState(0f, 0f, 598f, 0f)));
    Require(beforeBoundary.State.Ai2 == 599f && beforeBoundary.State.Ai1 == 0f &&
      !beforeBoundary.NetworkUpdateRequested,
      "The normal opening counter must remain in hover one tick before 600.");

    NpcEyeOfCthulhuProfileResult atBoundary = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(state: new NpcEyeOfCthulhuProfileState(0f, 0f, 599f, 0f)));
    Require(atBoundary.State.Ai1 == 1f && atBoundary.State.Ai2 == 0f &&
      atBoundary.State.Ai3 == 0f && atBoundary.TargetResetRequested &&
      atBoundary.NetworkUpdateRequested,
      "The opening counter must enter its first dash at ai[2] >= 600.");

    NpcEyeOfCthulhuProfileResult expertBoundary = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 0f, 209f, 0f),
        expertMode: true));
    Require(expertBoundary.State.Ai1 == 1f && expertBoundary.State.Ai2 == 0f,
      "Expert opening timing must use the 210 tick boundary.");
  }

  private static void VerifyLifeThreshold()
  {
    NpcEyeOfCthulhuProfileResult threshold = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 0f, 10f, 0f),
        life: 500,
        lifeMax: 1_000));
    Require(threshold.State.Ai0 == 0f,
      "The normal phase transition must not fire at exactly half life.");

    NpcEyeOfCthulhuProfileResult crossed = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 0f, 10f, 0f),
        life: 499,
        lifeMax: 1_000));
    Require(crossed.State == new NpcEyeOfCthulhuProfileState(1f, 0f, 0f, 0f) &&
      crossed.NetworkUpdateRequested,
      "Crossing below half life must expose the phase transition and synchronization intent.");

    NpcEyeOfCthulhuProfileResult expertThreshold = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 0f, 10f, 0f),
        life: 650,
        lifeMax: 1_000,
        expertMode: true));
    Require(expertThreshold.State.Ai0 == 0f,
      "The expert phase transition must use a strict 65 percent threshold.");

    NpcEyeOfCthulhuProfileResult expertCrossed = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 0f, 10f, 0f),
        life: 649,
        lifeMax: 1_000,
        expertMode: true));
    Require(expertCrossed.State == new NpcEyeOfCthulhuProfileState(1f, 0f, 0f, 0f) &&
      expertCrossed.NetworkUpdateRequested,
      "Crossing below 65 percent life must request the source phase update.");
  }

  private static void VerifyTargetLossAndExitOrder()
  {
    var events = new List<string>();
    NpcEyeOfCthulhuProfileInput input = CreateInput(
      targetIsAvailable: false,
      dustRoll: 0);
    NpcEyeOfCthulhuProfileResult result = NpcEyeOfCthulhuProfile.Evaluate(in input);
    NpcEyeOfCthulhuProfile.ApplyEffects(in input, in result, new RecordingEffectPort(events));
    Require(result.ExitReason == NpcEyeOfCthulhuExitReason.TargetUnavailable &&
      result.Velocity.Y == input.Velocity.Y - 0.04f &&
      events.SequenceEqual(["Dust", "EncourageDespawn:10"]),
      "A missing target must preserve the source dust-before-exit order.");

    var deadTargetEvents = new List<string>();
    NpcEyeOfCthulhuProfileInput deadTargetInput = CreateInput(
      targetIsAvailable: false,
      targetIsDead: true,
      dustRoll: 1);
    NpcEyeOfCthulhuProfileResult deadTargetResult = NpcEyeOfCthulhuProfile.Evaluate(
      in deadTargetInput);
    NpcEyeOfCthulhuProfile.ApplyEffects(
      in deadTargetInput,
      in deadTargetResult,
      new RecordingEffectPort(deadTargetEvents));
    Require(deadTargetResult.ExitReason == NpcEyeOfCthulhuExitReason.TargetDead &&
      deadTargetEvents.SequenceEqual(["EncourageDespawn:10"]),
      "A dead target must take the explicit exit path without emitting dust when the roll misses.");
  }

  private static void VerifyDashAndEffectOrder()
  {
    var events = new List<string>();
    NpcEyeOfCthulhuProfileInput input = CreateInput(
      state: new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 0f),
      dustRoll: 0);
    NpcEyeOfCthulhuProfileResult result = NpcEyeOfCthulhuProfile.EvaluateWithRandom(
      in input,
      new RecordingRandomPort(events, result: 0));
    NpcEyeOfCthulhuProfile.ApplyEffects(in input, in result, new RecordingEffectPort(events));
    Require(result.AttackIntent == NpcEyeOfCthulhuAttackIntent.Dash &&
      result.State.Ai1 == 2f && MathF.Abs(result.Velocity.Length() - 6f) < 0.001f &&
      result.NetworkUpdateRequested &&
      events.SequenceEqual(["Random.Next(5)=0", "Dust", "Attack:Dash", "NetworkUpdate"]),
      "The dash attack must consume random before dust and publish its network effect last.");

    NpcEyeOfCthulhuProfileResult expertResult = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 0f),
        expertMode: true));
    Require(MathF.Abs(expertResult.Velocity.Length() - 7f) < 0.001f,
      "The Expert first dash must use the source speed of seven.");

    NpcEyeOfCthulhuProfileResult goodWorldResult = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 0f),
        getGoodWorld: true));
    NpcEyeOfCthulhuProfileResult expertGoodWorldResult = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 0f),
        expertMode: true,
        getGoodWorld: true));
    Require(MathF.Abs(goodWorldResult.Velocity.Length() - 7f) < 0.001f &&
      MathF.Abs(expertGoodWorldResult.Velocity.Length() - 8f) < 0.001f,
      "For the Worthy must add one to the ordinary and Expert first dash speeds.");

    Vector2 initialVelocity = new(2.75f, -1.125f);
    Vector2 sourceOrderedVelocity = initialVelocity;
    sourceOrderedVelocity *= 0.98f;
    sourceOrderedVelocity *= 0.985f;
    sourceOrderedVelocity *= 0.99f;
    NpcEyeOfCthulhuProfileInput dampInput = CreateInput(
      state: new NpcEyeOfCthulhuProfileState(0f, 2f, 39f, 0f),
      expertMode: true,
      getGoodWorld: true) with
    {
      Velocity = initialVelocity,
    };
    NpcEyeOfCthulhuProfileResult dampResult = NpcEyeOfCthulhuProfile.Evaluate(in dampInput);
    Require(dampResult.Velocity == sourceOrderedVelocity,
      "Expert+GetGoodWorld dash damping must preserve the source's sequential float operations.");
  }

  private static void VerifyDashCycle()
  {
    NpcEyeOfCthulhuProfileResult firstDashEnd = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(state: new NpcEyeOfCthulhuProfileState(0f, 2f, 149f, 0f)));
    Require(firstDashEnd.State == new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 1f) &&
      firstDashEnd.TargetResetRequested,
      "The first 150-tick dash must return to the aim-and-launch state with one completed dash.");

    NpcEyeOfCthulhuProfileResult secondDashEnd = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(state: new NpcEyeOfCthulhuProfileState(0f, 2f, 149f, 1f)));
    Require(secondDashEnd.State == new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 2f) &&
      secondDashEnd.TargetResetRequested,
      "The second ordinary dash must preserve the two-dash cycle counter.");

    NpcEyeOfCthulhuProfileResult thirdDashEnd = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(state: new NpcEyeOfCthulhuProfileState(0f, 2f, 149f, 2f)));
    Require(thirdDashEnd.State == new NpcEyeOfCthulhuProfileState(0f, 0f, 0f, 0f) &&
      thirdDashEnd.TargetResetRequested,
      "The third ordinary dash must reset the attack cycle to hover.");

    NpcEyeOfCthulhuProfileResult expertGoodWorldDashEnd = NpcEyeOfCthulhuProfile.Evaluate(
      CreateInput(
        state: new NpcEyeOfCthulhuProfileState(0f, 2f, 84f, 1f),
        expertMode: true,
        getGoodWorld: true));
    Require(expertGoodWorldDashEnd.State ==
        new NpcEyeOfCthulhuProfileState(0f, 1f, 0f, 2f) &&
      expertGoodWorldDashEnd.TargetResetRequested,
      "Expert For the Worthy must use its 85-tick dash boundary before the third dash.");
  }

  private static void VerifyOpeningServantSpawn()
  {
    var events = new List<string>();
    NpcEyeOfCthulhuProfileInput input = CreateInput(
      state: new NpcEyeOfCthulhuProfileState(0f, 0f, 0f, 43f),
      expertMode: true);
    NpcEyeOfCthulhuProfileResult result = NpcEyeOfCthulhuProfile.Evaluate(in input);
    NpcEyeOfCthulhuProfile.ApplyEffects(
      in input,
      in result,
      new RecordingEffectPort(events));
    Require(result.ServantSummonRequested && result.State.Ai3 == 0f &&
      result.ServantSummonDustCount == 10 && result.ServantSummonSoundRequested &&
      events.Count(effect => effect == "Servant") == 1 &&
      events.Count(effect => effect == "Dust") == 10 &&
      events[0] == "Servant" && events[1] == "Sound:3",
      "The opening hover threshold must reset ai[3], spawn a Servant, and emit its " +
      "sound and dust.");
  }

  private static void VerifyTransformationAndExpertServants()
  {
    var events = new List<string>();
    NpcEyeOfCthulhuProfileInput input = CreateInput(
      state: new NpcEyeOfCthulhuProfileState(1f, 99f, 0.495f, 0f),
      expertMode: true,
      getGoodWorld: true,
      life: 900,
      lifeMax: 2_800);
    var randomPort = new RecordingRandomPort(events, result: 0);
    NpcEyeOfCthulhuProfileResult result = NpcEyeOfCthulhuProfile.EvaluateWithRandom(
      in input,
      randomPort);
    NpcEyeOfCthulhuProfile.ApplyEffects(
      in input,
      in result,
      new RecordingEffectPort(events),
      randomPort);
    int servantCount = events.Count(effect => effect == "Servant");
    int goreCount = events.Count(effect => effect.StartsWith("Gore:"));
    int dustCount = events.Count(effect => effect == "Dust");
    int servantIndex = events.IndexOf("Servant");
    int transformationSoundIndex = events.IndexOf("Sound:3");
    Require(result.State == new NpcEyeOfCthulhuProfileState(2f, 0f, 0.5f, 0f) &&
      result.ServantSummonRequested &&
      servantCount == 1 &&
      result.TransformationBurstRequested &&
      result.ReflectsProjectiles && goreCount == 6 &&
      dustCount == 32 &&
      servantIndex < transformationSoundIndex &&
      events.Contains("Sound:3") && events.Contains("Sound:15"),
      "The first transformation boundary must preserve its phase change, Expert summon, " +
      $"reflection, and effects. Actual: state={result.State}, " +
      $"summon={result.ServantSummonRequested}, servants={servantCount}, " +
      $"burst={result.TransformationBurstRequested}, reflects={result.ReflectsProjectiles}, " +
      $"gore={goreCount}, dust={dustCount}, effect-order={servantIndex}<{transformationSoundIndex}.");

    events.Clear();
    NpcEyeOfCthulhuProfileInput finalPhaseInput = CreateInput(
      state: new NpcEyeOfCthulhuProfileState(2f, 99f, 0.005f, 0f),
      expertMode: true,
      getGoodWorld: true);
    var finalPhaseRandom = new RecordingRandomPort(events, result: 0);
    NpcEyeOfCthulhuProfileResult finalPhase = NpcEyeOfCthulhuProfile.EvaluateWithRandom(
      in finalPhaseInput,
      finalPhaseRandom);
    NpcEyeOfCthulhuProfile.ApplyEffects(
      in finalPhaseInput,
      in finalPhase,
      new RecordingEffectPort(events),
      finalPhaseRandom);
    Require(finalPhase.State == new NpcEyeOfCthulhuProfileState(3f, 0f, 0f, 0f) &&
      finalPhase.ServantSummonRequested &&
      events.Count(effect => effect == "Servant") == 1 &&
      !finalPhase.TransformationBurstRequested &&
      events.Count(effect => effect == "Dust") == 12 &&
      events.All(effect => effect != "Sound:3") &&
      events.All(effect => !effect.StartsWith("Gore:")),
      "The second transformation boundary must enter phase three without replaying " +
      "phase-one visuals.");
  }

  private static NpcEyeOfCthulhuProfileInput CreateInput(
    NpcEyeOfCthulhuProfileState? state = null,
    bool targetIsAvailable = true,
    bool targetIsDead = false,
    bool dayTime = false,
    bool expertMode = false,
    bool getGoodWorld = false,
    int life = 2_800,
    int lifeMax = 2_800,
    int dustRoll = 1)
  {
    return new NpcEyeOfCthulhuProfileInput(
      TypeId: 4,
      NetId: 4,
      AiStyle: 4,
      State: state ?? new NpcEyeOfCthulhuProfileState(0f, 0f, 0f, 0f),
      Position: Vector2.Zero,
      Velocity: Vector2.Zero,
      TargetCenter: new Vector2(300f, 100f),
      Width: 100,
      Height: 100,
      Life: life,
      LifeMax: lifeMax,
      TargetIsAvailable: targetIsAvailable,
      TargetIsDead: targetIsDead,
      DayTime: dayTime,
      ExpertMode: expertMode,
      GetGoodWorld: getGoodWorld,
      DustRoll: dustRoll)
    {
      TargetPosition = new Vector2(300f, 100f),
    };
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class RecordingEffectPort(List<string> events)
    : INpcEyeOfCthulhuProfileEffectPort
  {
    public void SpawnDust(Vector2 position, int width, int height, Vector2 velocity)
    {
      events.Add("Dust");
    }

    public bool TrySpawnServant(
      Vector2 position,
      Vector2 velocity)
    {
      events.Add("Servant");
      return true;
    }

    public void PlaySound(int soundId, Vector2 position)
    {
      events.Add($"Sound:{soundId}");
    }

    public void SpawnGore(int goreId, Vector2 position, Vector2 velocity)
    {
      events.Add($"Gore:{goreId}");
    }

    public void SetReflectsProjectiles(bool reflectsProjectiles)
    {
      events.Add($"Reflect:{reflectsProjectiles}");
    }

    public void BeginDash(Vector2 velocity)
    {
      events.Add("Attack:Dash");
    }

    public void EncourageDespawn(int ticks)
    {
      events.Add($"EncourageDespawn:{ticks}");
    }

    public void RequestNetworkUpdate()
    {
      events.Add("NetworkUpdate");
    }
  }

  private sealed class RecordingRandomPort(List<string> events, int result)
    : INpcEyeOfCthulhuRandomPort
  {
    public int Next(int maxExclusive)
    {
      events.Add($"Random.Next({maxExclusive})={result}");
      return result;
    }
  }
}
