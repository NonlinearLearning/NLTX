using System.Numerics;
using Terraria.Npc;

internal static partial class Program
{
  private const float SlimeGravity = 0.3f;
  private const int SlimeRandomSeed = 20261007;

  private static void AddSlimeScenarios(List<Scenario> scenarios)
  {
    AddSlimeScenario(scenarios, "c1-blue-slime-init-ground-jump", "blue-slime", 1, 1, 1,
      ai0: 0f, ai1: 0f, ai2: 0f, ai3: 0f, velocityX: 0f, velocityY: 0f, direction: 0,
      wet: false, ticks: 56);
    AddSlimeScenario(scenarios, "c1-blue-slime-ground-jump-launch", "blue-slime", 1, 1, 1,
      ai0: -2f, ai1: 0f, ai2: 1f, ai3: 0f, velocityX: 0f, velocityY: 0f, direction: 1,
      wet: false, ticks: 5);
    AddSlimeScenario(scenarios, "c1-blue-slime-wet-rise-turn", "blue-slime", 1, 1, 1,
      ai0: -200f, ai1: 0f, ai2: 1f, ai3: 1000.25f, velocityX: 0.7f, velocityY: 2.5f, direction: 1,
      wet: true, ticks: 8);
    AddSlimeScenario(scenarios, "c1-blue-slime-ai2-cooldown", "blue-slime", 1, 1, 1,
      ai0: -100f, ai1: 0f, ai2: 3f, ai3: 0f, velocityX: 0f, velocityY: 0f, direction: 1,
      wet: false, ticks: 4);

    AddSlimeScenario(scenarios, "c1-3-mother-slime-init-ground-jump", "mother-slime", 16, 16, 1,
      ai0: 0f, ai1: 0f, ai2: 0f, ai3: 0f, velocityX: 0f, velocityY: 0f, direction: 1,
      wet: false, ticks: 56);
    AddSlimeScenario(scenarios, "c1-3-mother-slime-ground-jump-launch", "mother-slime", 16, 16, 1,
      ai0: -2f, ai1: 0f, ai2: 1f, ai3: 0f, velocityX: 0f, velocityY: 0f, direction: 1,
      wet: false, ticks: 5);
    AddSlimeScenario(scenarios, "c1-3-mother-slime-wet-rise-turn", "mother-slime", 16, 16, 1,
      ai0: -200f, ai1: 0f, ai2: 1f, ai3: 1000.25f, velocityX: 0.7f, velocityY: 2.5f, direction: 1,
      wet: true, ticks: 8);
    AddSlimeScenario(scenarios, "c1-3-mother-slime-ai2-cooldown", "mother-slime", 16, 16, 1,
      ai0: -100f, ai1: 0f, ai2: 3f, ai3: 0f, velocityX: 0f, velocityY: 0f, direction: 1,
      wet: false, ticks: 4);
  }

  private static void AddSlimeScenario(
    List<Scenario> scenarios,
    string id,
    string profile,
    int typeId,
    int netId,
    int aiStyle,
    float ai0,
    float ai1,
    float ai2,
    float ai3,
    float velocityX,
    float velocityY,
    int direction,
    bool wet,
    int ticks)
  {
    bool isMotherSlime = profile == "mother-slime";
    scenarios.Add(new Scenario(
      id,
      ExpertMode: false,
      GetGoodWorld: false,
      Ticks: ticks,
      Width: isMotherSlime ? 36 : 24,
      Height: isMotherSlime ? 24 : 18,
      Life: isMotherSlime ? 90 : 25,
      LifeMax: isMotherSlime ? 90 : 25,
      PositionXBits: Bits(1000.25f),
      PositionYBits: Bits(1000.5f),
      TargetXBits: Bits(1200f),
      TargetYBits: Bits(900f),
      VelocityXBits: Bits(velocityX),
      VelocityYBits: Bits(velocityY),
      Ai0Bits: Bits(ai0),
      Ai1Bits: Bits(ai1),
      Ai2Bits: Bits(ai2),
      Ai3Bits: Bits(ai3),
      RotationBits: Bits(0f),
      LocalAiBits: [Bits(0f), Bits(0f), Bits(0f), Bits(0f)],
      Profile: profile,
      TypeId: typeId,
      NetId: netId,
      AiStyle: aiStyle,
      Direction: direction,
      TargetSlot: 0,
      DayTime: false,
      Wet: wet,
      IsDamaged: false,
      IsBelowSurface: true,
      SlimeRain: false,
      IsClient: true,
      CanContainItems: !isMotherSlime,
      ValueBits: Bits(isMotherSlime ? 0f : 25f),
      BaseDefense: isMotherSlime ? 6 : 0,
      NetMode: 1,
      RandomSeed: SlimeRandomSeed,
      ExpectedRandomCalls: 0));
  }

  private static void CompareSlimeProfile(
    Scenario scenario,
    ReferenceCase referenceCase,
    ProfileComparison comparison)
  {
    bool isBlueSlime = scenario.Profile == "blue-slime";
    bool isMotherSlime = scenario.Profile == "mother-slime";
    if (!isBlueSlime && !isMotherSlime)
    {
      throw new InvalidDataException($"Unknown profile kind '{scenario.Profile}' in {scenario.Id}.");
    }

    if (scenario.ExpectedRandomCalls != 0)
    {
      throw new InvalidDataException($"Slime scenario {scenario.Id} must use the zero-draw random fixture.");
    }

    bool identityAccepted = isBlueSlime
      ? NpcBlueSlimeProfile.CanHandle(scenario.TypeId, scenario.NetId, scenario.AiStyle)
      : NpcMotherSlimeProfile.CanHandle(scenario.TypeId, scenario.NetId, scenario.AiStyle);
    if (!identityAccepted)
    {
      AddDifference(comparison, scenario.Id, 0, "identityGate", "accepted", "rejected");
    }

    Vector2 position = new(ToFloat(scenario.PositionXBits), ToFloat(scenario.PositionYBits));
    Vector2 velocity = new(ToFloat(scenario.VelocityXBits), ToFloat(scenario.VelocityYBits));
    NpcBlueSlimeProfileState blueState = new(
      ToFloat(scenario.Ai0Bits),
      ToFloat(scenario.Ai1Bits),
      ToFloat(scenario.Ai2Bits),
      ToFloat(scenario.Ai3Bits));
    NpcMotherSlimeProfileState motherState = new(
      ToFloat(scenario.Ai0Bits),
      ToFloat(scenario.Ai1Bits),
      ToFloat(scenario.Ai2Bits),
      ToFloat(scenario.Ai3Bits));
    int direction = scenario.Direction;

    for (int tickIndex = 0; tickIndex < referenceCase.Ticks.Count; tickIndex++)
    {
      ReferenceTick tick = referenceCase.Ticks[tickIndex];
      if (!tick.RandomAdvancedAsExpected || tick.ExpectedRandomCalls != 0 || tick.RandomRoll != -1)
      {
        AddDifference(comparison, scenario.Id, tick.Tick, "randomConsumption", "0 draws", "unexpected random advancement");
      }
      CompareInt(comparison, scenario.Id, tick.Tick, "sourceGravity", Bits(SlimeGravity), tick.SourceGravityBits);

      int beforeAi0Bits = Bits(isBlueSlime ? blueState.Ai0 : motherState.Ai0);
      int beforeAi1Bits = Bits(isBlueSlime ? blueState.Ai1 : motherState.Ai1);
      int beforeAi2Bits = Bits(isBlueSlime ? blueState.Ai2 : motherState.Ai2);
      int beforeAi3Bits = Bits(isBlueSlime ? blueState.Ai3 : motherState.Ai3);
      int beforeVelocityXBits = Bits(velocity.X);
      int beforeVelocityYBits = Bits(velocity.Y);

      List<string> effectPortCalls = [];
      bool targetReacquireRequested;
      bool networkSyncRequested;
      bool containedItemRequested = false;
      bool containedItemRejected = false;
      int resultDirection;
      Vector2 resultPosition = position;
      Vector2 resultVelocity;
      NpcBlueSlimeProfileState resultingBlueState = blueState;
      NpcMotherSlimeProfileState resultingMotherState = motherState;
      string sourceBranches;

      if (isBlueSlime)
      {
        NpcBlueSlimeProfileInput input = new(
          Position: position,
          Velocity: velocity,
          State: blueState,
          Direction: direction,
          TargetSlot: scenario.TargetSlot,
          DayTime: scenario.DayTime,
          IsDamaged: scenario.IsDamaged,
          IsBelowSurface: scenario.IsBelowSurface,
          SlimeRain: scenario.SlimeRain,
          Wet: scenario.Wet,
          CollideX: scenario.CollideX,
          CollideY: scenario.CollideY,
          OldVelocity: new Vector2(ToFloat(scenario.OldVelocityXBits), ToFloat(scenario.OldVelocityYBits)),
          Gravity: SlimeGravity,
          IsClient: scenario.IsClient,
          CanContainItems: scenario.CanContainItems,
          Value: ToFloat(scenario.ValueBits),
          BaseDefense: scenario.BaseDefense);
        NpcBlueSlimeProfileResult result = NpcBlueSlimeProfile.Evaluate(in input);
        RecordingBlueSlimeEffectPort effectPort = new();
        NpcBlueSlimeProfile.ApplyEffects(in result, isBallooned: false, effectPort);
        effectPortCalls.AddRange(effectPort.Events);
        resultingBlueState = result.State;
        resultVelocity = result.Velocity;
        resultDirection = result.Direction;
        targetReacquireRequested = result.TargetClosestRequested;
        networkSyncRequested = result.NetUpdateRequested;
        containedItemRequested = result.ContainedItemGenerationRequested;
        sourceBranches = result.Branches.ToString();

        CompareInt(comparison, scenario.Id, tick.Tick, "direction", result.Direction, tick.Direction);
        CompareInt(comparison, scenario.Id, tick.Tick, "defense", result.Defense, tick.Defense);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[0]", Bits(result.State.Ai0), tick.Ai0Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[1]", Bits(result.State.Ai1), tick.Ai1Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[2]", Bits(result.State.Ai2), tick.Ai2Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[3]", Bits(result.State.Ai3), tick.Ai3Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "velocity.X", Bits(result.Velocity.X), tick.VelocityXBits, false);
        CompareFloat(comparison, scenario.Id, tick.Tick, "velocity.Y", Bits(result.Velocity.Y), tick.VelocityYBits, false);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "netUpdate", result.NetUpdateRequested, tick.NetUpdate, true);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "containedItemGeneration", result.ContainedItemGenerationRequested, false, true);
      }
      else
      {
        NpcMotherSlimeProfileInput input = new(
          TypeId: scenario.TypeId,
          NetId: scenario.NetId,
          AiStyle: scenario.AiStyle,
          Position: position,
          Velocity: velocity,
          State: motherState,
          Direction: direction,
          TargetSlot: scenario.TargetSlot,
          DayTime: scenario.DayTime,
          IsDamaged: scenario.IsDamaged,
          IsBelowSurface: scenario.IsBelowSurface,
          SlimeRain: scenario.SlimeRain,
          Wet: scenario.Wet,
          CollideX: scenario.CollideX,
          CollideY: scenario.CollideY,
          OldVelocity: new Vector2(ToFloat(scenario.OldVelocityXBits), ToFloat(scenario.OldVelocityYBits)),
          SolidCollision: scenario.SolidCollision);
        NpcMotherSlimeProfileResult result = NpcMotherSlimeProfile.Evaluate(in input);
        RecordingMotherSlimeEffectPort effectPort = new();
        NpcMotherSlimeProfile.ApplyEffects(in result, effectPort);
        effectPortCalls.AddRange(effectPort.Events);
        resultingMotherState = result.State;
        resultPosition = result.Position;
        resultVelocity = result.Velocity;
        resultDirection = result.Direction;
        targetReacquireRequested = result.TargetClosestRequested;
        networkSyncRequested = result.NetUpdateRequested;
        containedItemRejected = result.ContainedItemSelectionRejected;
        sourceBranches = result.Branches.ToString();

        CompareInt(comparison, scenario.Id, tick.Tick, "direction", result.Direction, tick.Direction);
        CompareFloat(comparison, scenario.Id, tick.Tick, "position.X", Bits(result.Position.X), tick.PositionXBits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "position.Y", Bits(result.Position.Y), tick.PositionYBits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[0]", Bits(result.State.Ai0), tick.Ai0Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[1]", Bits(result.State.Ai1), tick.Ai1Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[2]", Bits(result.State.Ai2), tick.Ai2Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[3]", Bits(result.State.Ai3), tick.Ai3Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "velocity.X", Bits(result.Velocity.X), tick.VelocityXBits, false);
        CompareFloat(comparison, scenario.Id, tick.Tick, "velocity.Y", Bits(result.Velocity.Y), tick.VelocityYBits, false);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "netUpdate", result.NetUpdateRequested, tick.NetUpdate, true);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "containedItemSelectionRejected", result.ContainedItemSelectionRejected, !tick.SourceCanContainItems, true);
      }

      bool sourceTargetClosestCalled = tick.DeduplicatedFirstEntryOrder.Any(static method =>
        method.Contains(".TargetClosest", StringComparison.Ordinal));
      CompareBoolean(comparison, scenario.Id, tick.Tick, "targetClosestCallPresence", targetReacquireRequested, sourceTargetClosestCalled, true);
      CompareBoolean(comparison, scenario.Id, tick.Tick, "sourceIdentity", true,
        tick.TypeId == scenario.TypeId && tick.NetId == scenario.NetId && tick.AiStyle == scenario.AiStyle, true);
      CompareBoolean(comparison, scenario.Id, tick.Tick, "sourceCanContainItems", scenario.CanContainItems, tick.SourceCanContainItems, true);
      CompareInt(comparison, scenario.Id, tick.Tick, "localAI[0]", scenario.LocalAiBits[0], tick.LocalAiBits[0]);
      CompareInt(comparison, scenario.Id, tick.Tick, "localAI[1]", scenario.LocalAiBits[1], tick.LocalAiBits[1]);
      CompareInt(comparison, scenario.Id, tick.Tick, "localAI[2]", scenario.LocalAiBits[2], tick.LocalAiBits[2]);
      CompareInt(comparison, scenario.Id, tick.Tick, "localAI[3]", scenario.LocalAiBits[3], tick.LocalAiBits[3]);

      comparison.TickResults.Add(new
      {
        profile = scenario.Profile,
        scenario = scenario.Id,
        tick = tick.Tick,
        fixedInput = new
        {
          typeId = scenario.TypeId,
          netId = scenario.NetId,
          aiStyle = scenario.AiStyle,
          direction = direction,
          targetSlot = scenario.TargetSlot,
          dayTime = scenario.DayTime,
          expertMode = scenario.ExpertMode,
          getGoodWorld = scenario.GetGoodWorld,
          netMode = scenario.NetMode,
          worldSurfaceTiles = scenario.WorldSurface,
          sourceAiGravity = SlimeGravity,
          isClient = scenario.IsClient,
          wet = scenario.Wet,
          collideX = scenario.CollideX,
          collideY = scenario.CollideY,
          oldVelocity = new { x = ToFloat(scenario.OldVelocityXBits), y = ToFloat(scenario.OldVelocityYBits) },
          randomSeed = referenceCase.Seed,
          expectedRandomCalls = scenario.ExpectedRandomCalls,
          randomPeekBefore = tick.RandomPeekBefore,
          randomPeekAfter = tick.RandomPeekAfter,
        },
        input = new
        {
          ai = new[] { beforeAi0Bits, beforeAi1Bits, beforeAi2Bits, beforeAi3Bits },
          velocity = new { x = beforeVelocityXBits, y = beforeVelocityYBits },
          position = new { x = Bits(position.X), y = Bits(position.Y) },
        },
        profileOutput = new
        {
          ai = isBlueSlime
            ? new[] { Bits(resultingBlueState.Ai0), Bits(resultingBlueState.Ai1), Bits(resultingBlueState.Ai2), Bits(resultingBlueState.Ai3) }
            : new[] { Bits(resultingMotherState.Ai0), Bits(resultingMotherState.Ai1), Bits(resultingMotherState.Ai2), Bits(resultingMotherState.Ai3) },
          velocity = new { x = Bits(resultVelocity.X), y = Bits(resultVelocity.Y) },
          position = new { x = Bits(resultPosition.X), y = Bits(resultPosition.Y) },
          direction = resultDirection,
          networkSyncRequested = networkSyncRequested,
          targetReacquireRequested = targetReacquireRequested,
          containedItemGenerationRequested = containedItemRequested,
          containedItemSelectionRejected = containedItemRejected,
          branches = sourceBranches,
          orderedEffectPortCalls = effectPortCalls,
        },
        sourceOutput = new
        {
          ai = new[] { tick.Ai0Bits, tick.Ai1Bits, tick.Ai2Bits, tick.Ai3Bits },
          velocity = new { x = tick.VelocityXBits, y = tick.VelocityYBits },
          position = new { x = tick.PositionXBits, y = tick.PositionYBits },
          direction = tick.Direction,
          target = tick.Target,
          netUpdate = tick.NetUpdate,
          sourceCanContainItems = tick.SourceCanContainItems,
          localAI = tick.LocalAiBits,
          randomAdvancedAsExpected = tick.RandomAdvancedAsExpected,
          deduplicatedFirstEntryOrder = tick.DeduplicatedFirstEntryOrder,
          targetClosestFirstEntryObserved = sourceTargetClosestCalled,
        },
        referenceEffectObservations = new
        {
          deduplicatedFirstEntryOrder = tick.DeduplicatedFirstEntryOrder,
          netUpdateObservedAfterAi = tick.NetUpdate,
          targetAfterTargetClosest = tick.Target,
        },
      });

      if (isBlueSlime)
      {
        blueState = resultingBlueState;
      }
      else
      {
        motherState = resultingMotherState;
        position = resultPosition;
      }

      velocity = resultVelocity;
      direction = resultDirection;
      comparison.TickCount++;
    }

    comparison.ScenarioCount++;
  }

  private static object CreateIdentityGatingReport()
  {
    bool motherMismatchThrows = false;
    try
    {
      NpcMotherSlimeProfileInput invalidInput = new(
        TypeId: 1,
        NetId: 1,
        AiStyle: 1,
        Position: Vector2.Zero,
        Velocity: Vector2.Zero,
        State: default,
        Direction: 0,
        TargetSlot: 0,
        DayTime: false,
        IsDamaged: false,
        IsBelowSurface: true,
        SlimeRain: false,
        Wet: false,
        CollideX: false,
        CollideY: false,
        OldVelocity: Vector2.Zero,
        SolidCollision: false);
      _ = NpcMotherSlimeProfile.Evaluate(in invalidInput);
    }
    catch (ArgumentException)
    {
      motherMismatchThrows = true;
    }

    return new
    {
      blueSlime = new
      {
        exactTypeNetIdStyleAccepted = NpcBlueSlimeProfile.CanHandle(1, 1, 1),
        wrongType = NpcBlueSlimeProfile.CanHandle(16, 1, 1) ? "accepted" : "rejected-before-evaluate",
        wrongNetId = NpcBlueSlimeProfile.CanHandle(1, 16, 1) ? "accepted" : "rejected-before-evaluate",
        wrongAiStyle = NpcBlueSlimeProfile.CanHandle(1, 1, 4) ? "accepted" : "rejected-before-evaluate",
      },
      motherSlime = new
      {
        exactTypeNetIdStyleAccepted = NpcMotherSlimeProfile.CanHandle(16, 16, 1),
        wrongType = NpcMotherSlimeProfile.CanHandle(1, 16, 1) ? "accepted" : "rejected-before-evaluate",
        wrongNetId = NpcMotherSlimeProfile.CanHandle(16, 1, 1) ? "accepted" : "rejected-before-evaluate",
        wrongAiStyle = NpcMotherSlimeProfile.CanHandle(16, 16, 4) ? "accepted" : "rejected-before-evaluate",
        evaluateWrongIdentity = motherMismatchThrows ? "rejected-with-ArgumentException" : "did-not-reject",
      },
    };
  }

  private static void CompareInt(ProfileComparison comparison, string scenarioId, int tick, string field, int profile, int source)
  {
    if (profile != source)
    {
      AddDifference(comparison, scenarioId, tick, field, profile.ToString(), source.ToString());
    }
  }

  private sealed class RecordingBlueSlimeEffectPort : INpcBlueSlimeProfileEffectPort
  {
    public List<string> Events { get; } = [];

    public NpcBlueSlimeTypeOneSelectionResult GenerateContainedItem(bool isBallooned)
    {
      Events.Add($"GenerateContainedItem({isBallooned})");
      return new NpcBlueSlimeTypeOneSelectionResult(true, -1f, true);
    }

    public void RequestNetworkSync()
    {
      Events.Add("RequestNetworkSync");
    }

    public void RequestTargetReacquire()
    {
      Events.Add("RequestTargetReacquire");
    }
  }

  private sealed class RecordingMotherSlimeEffectPort : INpcMotherSlimeProfileEffectPort
  {
    public List<string> Events { get; } = [];

    public void RequestNetworkSync()
    {
      Events.Add("RequestNetworkSync");
    }

    public void RequestTargetReacquire()
    {
      Events.Add("RequestTargetReacquire");
    }
  }
}
