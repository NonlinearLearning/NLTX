using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Terraria.Npc;

internal static partial class Program
{
  private const int NpcWidth = 100;
  private const int NpcHeight = 110;
  private const int Life = 2800;
  private const int LifeMax = 2800;
  private const float Ai0 = 0f;
  private const float Ai1 = 2f;
  private const float Ai2 = 39f;
  private const float Ai3 = 0f;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
  };

  private static readonly string[] ProfileSourceFiles =
  [
    "INpcEyeOfCthulhuProfileEffectPort.cs",
    "INpcEyeOfCthulhuRandomPort.cs",
    "NpcEyeOfCthulhuAttackIntent.cs",
    "NpcEyeOfCthulhuExitReason.cs",
    "NpcEyeOfCthulhuProfile.cs",
    "NpcEyeOfCthulhuProfileInput.cs",
    "NpcEyeOfCthulhuProfileResult.cs",
    "NpcEyeOfCthulhuProfileState.cs",
    "INpcBlueSlimeProfileEffectPort.cs",
    "NpcBlueSlimeProfile.cs",
    "NpcBlueSlimeProfileInput.cs",
    "NpcBlueSlimeProfileResult.cs",
    "NpcBlueSlimeProfileState.cs",
    "NpcBlueSlimeSourceBranch.cs",
    "NpcBlueSlimeTypeOneSelectionResult.cs",
    "INpcMotherSlimeProfileEffectPort.cs",
    "NpcMotherSlimeProfile.cs",
    "NpcMotherSlimeProfileInput.cs",
    "NpcMotherSlimeProfileResult.cs",
    "NpcMotherSlimeProfileState.cs",
    "NpcMotherSlimeSourceBranch.cs",
  ];

  private static int Main(string[] args)
  {
    try
    {
      return Run(args);
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine($"NPC AI reference differential FAIL: {exception}");
      return 1;
    }
  }

  private static int Run(string[] args)
  {
    if (args.Length == 3 && args[0] == "--prepare-cases" && args[1] == "--output")
    {
      WriteCases(Path.GetFullPath(args[2]));
      Console.WriteLine($"Prepared {CreateCases().Cases.Count} differential scenarios: {Path.GetFullPath(args[2])}");
      return 0;
    }

    if (args.Length != 11 || args[0] != "--compare" || args[1] != "--cases" ||
        args[3] != "--original" || args[5] != "--source-build" ||
        args[7] != "--provenance" || args[9] != "--report")
    {
      throw new ArgumentException(
        "Usage: --compare --cases <json> --original <json> --source-build <json> --provenance <json> --report <json>");
    }

    string casesPath = Path.GetFullPath(args[2]);
    string originalPath = Path.GetFullPath(args[4]);
    string sourceBuildPath = Path.GetFullPath(args[6]);
    string provenancePath = Path.GetFullPath(args[8]);
    string reportPath = Path.GetFullPath(args[10]);
    CaseDocument cases = ReadJson<CaseDocument>(casesPath);
    ReferenceRun original = ReadJson<ReferenceRun>(originalPath);
    ReferenceRun sourceBuild = ReadJson<ReferenceRun>(sourceBuildPath);
    SourceBuildProvenance provenance = ReadJson<SourceBuildProvenance>(provenancePath);
    VerifierExecution verifierExecution = InspectVerifierExecution(provenance);
    List<string> evidenceErrors = ValidateSampleEvidence(cases, original, sourceBuild);
    evidenceErrors.AddRange(ValidateProvenance(provenance, original, sourceBuild, verifierExecution));
    if (evidenceErrors.Count > 0)
    {
      return WriteInvalidEvidenceReport(
        reportPath,
        cases,
        original,
        sourceBuild,
        provenance,
        verifierExecution,
        evidenceErrors);
    }

    List<ReferenceDifference> binaryVsBuildDifferences = CompareReferenceRuns(original, sourceBuild);
    bool aiIlMatches = string.Equals(
      original.NpcAiIlSha256,
      sourceBuild.NpcAiIlSha256,
      StringComparison.OrdinalIgnoreCase);
    bool slimeIlMatches = string.Equals(
      original.SlimeAiIlSha256,
      sourceBuild.SlimeAiIlSha256,
      StringComparison.OrdinalIgnoreCase);
    bool bothMethodIlHashesMatch = aiIlMatches && slimeIlMatches;
    bool originalMatchesSourceBuild = bothMethodIlHashesMatch && binaryVsBuildDifferences.Count == 0;
    bool sameIlOutputContradiction = bothMethodIlHashesMatch && binaryVsBuildDifferences.Count > 0;
    ReferenceRun selectedOracle = originalMatchesSourceBuild ? original : sourceBuild;
    string selectedOracleKind = originalMatchesSourceBuild
      ? "read-only TerrariaServer.exe; both sampled method IL hashes and captured outputs match the isolated pinned-source build"
      : sameIlOutputContradiction
        ? "isolated pinned-source build used for diagnostics only; matching method IL contradicts sampled outputs, so source trust fails"
        : "isolated pinned-source build fallback; original method IL differs, so the original binary is not associated with this source scope";

    ProfileComparison comparison = CompareProfile(cases, selectedOracle);
    Dictionary<string, string> profileSourceFilesAtRunTime = verifierExecution.ProfileSourceFilesAtRunTime;
    string profileSourceManifestSha256AtRunTime = verifierExecution.ProfileSourceManifestSha256AtRunTime;
    bool runtimeProfileSourcesMatchBuildManifest = verifierExecution.RuntimeProfileSourcesMatchBuildManifest;
    bool gateFailed = sameIlOutputContradiction ||
      !verifierExecution.MatchesBuildProvenance ||
      !runtimeProfileSourcesMatchBuildManifest ||
      comparison.Differences.Count > 0;
    string profileOutcome = comparison.Differences.Count == 0
      ? "no-profile-differences-in-sampled-scope"
      : "profile-differences";
    string sourceTrustStatus = originalMatchesSourceBuild
      ? "trusted-original-associated-with-pinned-source"
      : sameIlOutputContradiction
        ? "failed-same-il-sampled-output-contradiction"
        : "pinned-source-build-fallback-original-method-il-differs";
    string status = sameIlOutputContradiction
      ? "source-reference-inconsistency"
      : comparison.Differences.Count == 0
        ? originalMatchesSourceBuild
          ? "comparison-complete-no-difference-in-sampled-scope"
          : "comparison-complete-no-profile-differences-using-pinned-source-fallback"
        : "comparison-complete-with-profile-differences";
    var report = new
    {
      schemaVersion = 2,
      gate = "npc-ai-reference-differential",
      status,
      gateOutcome = gateFailed ? "failed" : "passed",
      profileOutcome,
      sourceTrust = new
      {
        status = sourceTrustStatus,
        originalMethodIlMatchesPinnedBuild = bothMethodIlHashesMatch,
        originalSampledOutputsMatchPinnedBuild = binaryVsBuildDifferences.Count == 0,
        originalBinaryAssociatedWithPinnedSource = originalMatchesSourceBuild,
        selectedOracle = selectedOracleKind,
        sampledSourceDifferenceCount = binaryVsBuildDifferences.Count,
        sameIlOutputContradiction,
      },
      createdUtc = DateTimeOffset.UtcNow,
      sourceIdentity = provenance,
      verifierExecution = CreateVerifierExecutionReport(provenance, verifierExecution),
      profile = new
      {
        project = "Test/Terraria.NpcAi.ReferenceVerification/Terraria.NpcAi.ReferenceVerification.csproj",
        productionProfiles = new[]
        {
          "src/NSSLC/Component/Npc/System/NpcEyeOfCthulhuProfile.cs",
          "src/NSSLC/Component/Npc/System/NpcBlueSlimeProfile.cs",
          "src/NSSLC/Component/Npc/System/NpcMotherSlimeProfile.cs",
        },
        productionProfileSourceFilesAtRunTime = profileSourceFilesAtRunTime,
        productionProfileSourceManifestSha256AtRunTime = profileSourceManifestSha256AtRunTime,
        algorithmCopiedIntoVerifier = false,
      },
      referenceBinaryAssociation = new
      {
        originalAssemblyPath = original.AssemblyPath,
        originalAssemblySha256 = original.AssemblySha256,
        originalNpcAiIlSha256 = original.NpcAiIlSha256,
        originalSlimeAiIlSha256 = original.SlimeAiIlSha256,
        isolatedSourceBuildAssemblyPath = sourceBuild.AssemblyPath,
        isolatedSourceBuildAssemblySha256 = sourceBuild.AssemblySha256,
        isolatedSourceBuildNpcAiIlSha256 = sourceBuild.NpcAiIlSha256,
        isolatedSourceBuildSlimeAiIlSha256 = sourceBuild.SlimeAiIlSha256,
        npcAiIlMatches = aiIlMatches,
        npcAi001SlimesIlMatches = slimeIlMatches,
        sampledReferenceOutputsMatch = binaryVsBuildDifferences.Count == 0,
        originalBinaryIsAssociatedWithPinnedSourceForNpcAi = originalMatchesSourceBuild,
        differences = binaryVsBuildDifferences,
      },
      oracle = new
      {
        kind = selectedOracleKind,
        assemblyPath = selectedOracle.AssemblyPath,
        assemblySha256 = selectedOracle.AssemblySha256,
        npcAiIlSha256 = selectedOracle.NpcAiIlSha256,
        npcAi001SlimesIlSha256 = selectedOracle.SlimeAiIlSha256,
      },
      captures = new
      {
        caseFile = provenance.CaseFilePath,
        originalRun = new
        {
          output = provenance.OriginalCapturePath,
          log = provenance.OriginalCaptureLogPath,
          runtimeSideEffectSandbox = original.RuntimeSideEffectSandbox,
        },
        isolatedSourceBuildRun = new
        {
          output = provenance.SourceBuildCapturePath,
          log = provenance.SourceBuildCaptureLogPath,
          runtimeSideEffectSandbox = sourceBuild.RuntimeSideEffectSandbox,
        },
      },
      inputs = cases.Cases,
      profileComparison = new
      {
        comparedScenarioCount = comparison.ScenarioCount,
        comparedTickCount = comparison.TickCount,
        identityGating = CreateIdentityGatingReport(),
        sourceBehaviorScope = new[]
        {
          new
          {
            profile = "blue-slime",
            identity = "type=1/netID=1/aiStyle=1",
            dispatchRange = "NPC.cs:20121-20125",
            helperRange = "NPC.cs:61133-62551; sampled regular wet/ground/counter/jump statements include 62269-62538",
            limit = "finite Blue Slime sample only; excludes contained-item selection because client authority suppresses it, plus full tile/collision, rendering, host integration, and other style-1 identities",
          },
          new
          {
            profile = "mother-slime",
            identity = "type=16/netID=16/aiStyle=1",
            dispatchRange = "NPC.cs:20121-20125",
            helperRange = "NPC.cs:61133-62551; sampled regular wet/ground/counter/jump statements include 62269-62538",
            limit = "finite Mother Slime sample only; excludes full tile/collision, rendering, host integration, death/split behavior, and other style-1 identities",
          },
        },
        stateFieldDifferences = comparison.StateFieldDifferences,
        velocityFieldDifferences = comparison.VelocityFieldDifferences,
        effectAndObservableDifferences = comparison.EffectAndObservableDifferences,
        maxAbsoluteVelocityDifference = comparison.MaxAbsoluteVelocityDifference,
        maxVelocityUlpDistance = comparison.MaxVelocityUlpDistance,
        totalDifferences = comparison.Differences.Count,
        differences = comparison.Differences,
        tickResults = comparison.TickResults,
        modeledScope = new[]
        {
          "Eye of Cthulhu: ai[0..3] and velocity damping in the sampled dash slice",
          "Blue Slime and Mother Slime: ai[0..3], velocity, direction, position where modeled, and sampled wet/ground/jump paths",
          "velocity X/Y, including every dash damping tick and the strict +/-0.1 clamp result",
          "the dust random roll supplied as explicit profile input",
          "profile network-update requests compared with source netUpdate; target-reacquire requests compared with TargetClosest presence in CallTracker's deduplicated first-entry order",
          "profile effect-port call order is recorded; source helper call counts and complete effect order are not inferred from CallTracker",
        },
        sourceObservationsOutsideProfileModel = new[]
        {
          "per-profile modeled fields are compared; unmodeled source outputs are retained in tick samples and explicitly listed with each profile scope",
          "NPC.rotation is recorded but not modeled by this profile slice",
          "NPC.reflectsProjectiles is initialized true then set false by NPC.AI; the profile has no corresponding state/effect",
          "host movement/integration after NPC.AI is not run",
          "world integration and rendering are not run; slime cases use fixed client authority and zero random draws",
        },
      },
    };

    WriteJson(reportPath, report);
    Console.WriteLine($"NPC AI reference differential captured {comparison.TickCount} ticks across {comparison.ScenarioCount} scenarios.");
    Console.WriteLine($"Original binary NPC.AI IL {(aiIlMatches ? "matches" : "differs from")} isolated source build.");
    Console.WriteLine($"Original binary NPC.AI_001_Slimes IL {(slimeIlMatches ? "matches" : "differs from")} isolated source build.");
    Console.WriteLine($"Selected oracle: {selectedOracleKind}.");
    Console.WriteLine($"Source trust: {sourceTrustStatus}; profile outcome: {profileOutcome}; gate outcome: {(gateFailed ? "failed" : "passed")}.");
    Console.WriteLine($"Profile state differences: {comparison.StateFieldDifferences}; velocity differences: {comparison.VelocityFieldDifferences}; effects/observables: {comparison.EffectAndObservableDifferences}.");
    Console.WriteLine($"Differential report: {reportPath}");
    if (gateFailed)
    {
      Console.Error.WriteLine($"Differential gate FAILED: source inconsistency={sameIlOutputContradiction}, verifier provenance mismatch={!verifierExecution.MatchesBuildProvenance || !runtimeProfileSourcesMatchBuildManifest}, profile differences={comparison.Differences.Count}.");
    }

    return gateFailed ? 2 : 0;
  }

  private static CaseDocument CreateCases()
  {
    List<Scenario> scenarios = [];
    AddScenario(scenarios, "normal-multi-tick", false, false, 2.75f, -1.125f, 5);
    AddScenario(scenarios, "expert-multi-tick", true, false, 2.75f, -1.125f, 5);
    AddScenario(scenarios, "expert-good-world-multi-tick", true, true, 2.75f, -1.125f, 5);

    (bool Expert, bool GoodWorld, string Label)[] modes =
    [
      (false, false, "normal"),
      (true, false, "expert"),
      (true, true, "expert-good-world"),
    ];
    foreach ((bool expert, bool goodWorld, string label) in modes)
    {
      float sourceDamping = 0.98f;
      if (expert)
      {
        sourceDamping *= 0.985f;
      }

      if (goodWorld)
      {
        sourceDamping *= 0.99f;
      }

      int boundaryBits = BitConverter.SingleToInt32Bits(0.1f / sourceDamping);
      for (int offset = -3; offset <= 3; offset++)
      {
        float magnitude = BitConverter.Int32BitsToSingle(boundaryBits + offset);
        AddScenario(
          scenarios,
          $"{label}-clamp-boundary-{offset:+0;-0;0}",
          expert,
          goodWorld,
          magnitude,
          -magnitude,
          1);
      }
    }

    AddSlimeScenarios(scenarios);

    return new CaseDocument(1, scenarios);
  }

  private static void AddScenario(
    List<Scenario> scenarios,
    string id,
    bool expertMode,
    bool getGoodWorld,
    float velocityX,
    float velocityY,
    int ticks)
  {
    scenarios.Add(new Scenario(
      id,
      expertMode,
      getGoodWorld,
      ticks,
      NpcWidth,
      NpcHeight,
      Life,
      LifeMax,
      Bits(1000.25f),
      Bits(1000.5f),
      Bits(1600f),
      Bits(900f),
      Bits(velocityX),
      Bits(velocityY),
      Bits(Ai0),
      Bits(Ai1),
      Bits(Ai2),
      Bits(Ai3),
      Bits(0.125f),
      [Bits(17.25f), Bits(-3.5f), Bits(4.75f), Bits(0f)]));
  }

  private static void WriteCases(string path)
  {
    WriteJson(path, CreateCases());
  }

  private static ProfileComparison CompareProfile(CaseDocument cases, ReferenceRun oracle)
  {
    ProfileComparison comparison = new();
    foreach (Scenario scenario in cases.Cases)
    {
      ReferenceCase referenceCase = FindCase(oracle, scenario.Id);
      if (scenario.Profile != "eye-of-cthulhu")
      {
        CompareSlimeProfile(scenario, referenceCase, comparison);
        continue;
      }

      NpcEyeOfCthulhuProfileState state = new(
        ToFloat(scenario.Ai0Bits),
        ToFloat(scenario.Ai1Bits),
        ToFloat(scenario.Ai2Bits),
        ToFloat(scenario.Ai3Bits));
      Vector2 velocity = new(ToFloat(scenario.VelocityXBits), ToFloat(scenario.VelocityYBits));
      Vector2 position = new(ToFloat(scenario.PositionXBits), ToFloat(scenario.PositionYBits));
      Vector2 targetCenter = new(
        ToFloat(scenario.TargetXBits) + 10f,
        ToFloat(scenario.TargetYBits) + 21f);
      int previousTarget = 0;

      for (int tickIndex = 0; tickIndex < referenceCase.Ticks.Count; tickIndex++)
      {
        ReferenceTick tick = referenceCase.Ticks[tickIndex];
        if (!tick.RandomAdvancedAsExpected || tick.RandomRoll == 0)
        {
          throw new InvalidOperationException(
            $"Reference scenario {scenario.Id} tick {tick.Tick} did not prove the one-draw no-dust setup.");
        }

        NpcEyeOfCthulhuProfileInput input = new(
          TypeId: 4,
          NetId: 4,
          AiStyle: 4,
          State: state,
          Position: position,
          Velocity: velocity,
          TargetCenter: targetCenter,
          Width: scenario.Width,
          Height: scenario.Height,
          Life: scenario.Life,
          LifeMax: scenario.LifeMax,
          TargetIsAvailable: true,
          TargetIsDead: false,
          DayTime: false,
          ExpertMode: scenario.ExpertMode,
          GetGoodWorld: scenario.GetGoodWorld,
          DustRoll: tick.RandomRoll);
        NpcEyeOfCthulhuProfileResult result = NpcEyeOfCthulhuProfile.Evaluate(in input);
        if (!result.IsSupported)
        {
          throw new InvalidOperationException($"Production profile rejected the valid scenario {scenario.Id}.");
        }

        RecordingEffectPort effectPort = new();
        NpcEyeOfCthulhuProfile.ApplyEffects(in input, in result, effectPort);
        comparison.TickResults.Add(new
        {
          scenario = scenario.Id,
          tick = tick.Tick,
          input = new
          {
            state = new
            {
              ai0 = FloatSample(state.Ai0),
              ai1 = FloatSample(state.Ai1),
              ai2 = FloatSample(state.Ai2),
              ai3 = FloatSample(state.Ai3),
            },
            velocity = new { x = FloatSample(velocity.X), y = FloatSample(velocity.Y) },
            dustRoll = tick.RandomRoll,
          },
          profile = new
          {
            state = new
            {
              ai0 = FloatSample(result.State.Ai0),
              ai1 = FloatSample(result.State.Ai1),
              ai2 = FloatSample(result.State.Ai2),
              ai3 = FloatSample(result.State.Ai3),
            },
            velocity = new { x = FloatSample(result.Velocity.X), y = FloatSample(result.Velocity.Y) },
            dustRequested = result.DustRequested,
            attackIntent = result.AttackIntent.ToString(),
            exitReason = result.ExitReason.ToString(),
            targetResetRequested = result.TargetResetRequested,
            networkUpdateRequested = result.NetworkUpdateRequested,
            orderedEffectPortCalls = effectPort.Events.ToArray(),
          },
          reference = new
          {
            state = new
            {
              ai0 = FloatSample(ToFloat(tick.Ai0Bits)),
              ai1 = FloatSample(ToFloat(tick.Ai1Bits)),
              ai2 = FloatSample(ToFloat(tick.Ai2Bits)),
              ai3 = FloatSample(ToFloat(tick.Ai3Bits)),
            },
            velocity = new
            {
              x = FloatSample(ToFloat(tick.VelocityXBits)),
              y = FloatSample(ToFloat(tick.VelocityYBits)),
            },
            position = new
            {
              x = FloatSample(ToFloat(tick.PositionXBits)),
              y = FloatSample(ToFloat(tick.PositionYBits)),
            },
            localAiBits = tick.LocalAiBits,
            rotation = FloatSample(ToFloat(tick.RotationBits)),
            randomRoll = tick.RandomRoll,
            randomAdvancedExactlyOnce = tick.RandomAdvancedExactlyOnce,
            target = tick.Target,
            netUpdate = tick.NetUpdate,
            reflectsProjectiles = tick.ReflectsProjectiles,
            life = tick.Life,
            lifeMax = tick.LifeMax,
            dayTime = tick.DayTime,
            expertMode = tick.ExpertMode,
            getGoodWorld = tick.GetGoodWorld,
          },
        });
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[0]", Bits(result.State.Ai0), tick.Ai0Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[1]", Bits(result.State.Ai1), tick.Ai1Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[2]", Bits(result.State.Ai2), tick.Ai2Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "ai[3]", Bits(result.State.Ai3), tick.Ai3Bits, true);
        CompareFloat(comparison, scenario.Id, tick.Tick, "velocity.X", Bits(result.Velocity.X), tick.VelocityXBits, false);
        CompareFloat(comparison, scenario.Id, tick.Tick, "velocity.Y", Bits(result.Velocity.Y), tick.VelocityYBits, false);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "dustRequested", result.DustRequested, tick.RandomRoll == 0, true);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "netUpdate", result.NetworkUpdateRequested, tick.NetUpdate, true);
        CompareBoolean(comparison, scenario.Id, tick.Tick, "targetReset", result.TargetResetRequested, tick.Target != previousTarget, true);

        bool referencePositionUnchanged =
          tick.PositionXBits == scenario.PositionXBits && tick.PositionYBits == scenario.PositionYBits;
        if (!referencePositionUnchanged)
        {
          AddDifference(comparison, scenario.Id, tick.Tick, "position", "profile has no position output", "reference position changed");
        }

        int[] expectedLocalAiBits = scenario.LocalAiBits;
        bool localAiUnchanged = tick.LocalAiBits.SequenceEqual(expectedLocalAiBits);
        if (!localAiUnchanged)
        {
          AddDifference(comparison, scenario.Id, tick.Tick, "localAI", "profile does not model localAI", "reference localAI changed");
        }

        if (tick.Target != previousTarget && result.TargetResetRequested != (tick.Target == 255))
        {
          AddDifference(comparison, scenario.Id, tick.Tick, "target", result.TargetResetRequested.ToString(), tick.Target.ToString());
        }

        if (effectPort.Events.Count != 0)
        {
          AddDifference(
            comparison,
            scenario.Id,
            tick.Tick,
            "effectPort",
            string.Join(" -> ", effectPort.Events),
            "no reference dust/dash/despawn/network effect requested in this slice");
        }

        state = result.State;
        velocity = result.Velocity;
        previousTarget = tick.Target;
        comparison.TickCount++;
      }

      comparison.ScenarioCount++;
    }

    return comparison;
  }

  private static void CompareFloat(
    ProfileComparison comparison,
    string scenarioId,
    int tick,
    string field,
    int profileBits,
    int referenceBits,
    bool stateField)
  {
    if (profileBits == referenceBits)
    {
      return;
    }

    if (stateField)
    {
      comparison.StateFieldDifferences++;
    }
    else
    {
      comparison.VelocityFieldDifferences++;
      double absoluteDifference = Math.Abs((double)ToFloat(profileBits) - ToFloat(referenceBits));
      comparison.MaxAbsoluteVelocityDifference = Math.Max(comparison.MaxAbsoluteVelocityDifference, absoluteDifference);
      ulong ulpDistance = GetUlpDistance(profileBits, referenceBits);
      comparison.MaxVelocityUlpDistance = Math.Max(comparison.MaxVelocityUlpDistance, ulpDistance);
    }

    AddDifference(
      comparison,
      scenarioId,
      tick,
      field,
      FormatFloat(profileBits),
      FormatFloat(referenceBits));
  }

  private static void CompareBoolean(
    ProfileComparison comparison,
    string scenarioId,
    int tick,
    string field,
    bool profileValue,
    bool referenceValue,
    bool effectOrObservable)
  {
    if (profileValue == referenceValue)
    {
      return;
    }

    if (effectOrObservable)
    {
      comparison.EffectAndObservableDifferences++;
    }

    AddDifference(comparison, scenarioId, tick, field, profileValue.ToString(), referenceValue.ToString());
  }

  private static List<ReferenceDifference> CompareReferenceRuns(
    ReferenceRun original,
    ReferenceRun sourceBuild)
  {
    List<ReferenceDifference> differences = [];
    foreach (ReferenceCase originalCase in original.Cases)
    {
      ReferenceCase buildCase = FindCase(sourceBuild, originalCase.Id);
      if (originalCase.Ticks.Count != buildCase.Ticks.Count)
      {
        differences.Add(new ReferenceDifference(originalCase.Id, 0, "tickCount", originalCase.Ticks.Count.ToString(), buildCase.Ticks.Count.ToString()));
        continue;
      }

      for (int index = 0; index < originalCase.Ticks.Count; index++)
      {
        ReferenceTick left = originalCase.Ticks[index];
        ReferenceTick right = buildCase.Ticks[index];
        CompareReferenceField(differences, originalCase.Id, left.Tick, "randomRoll", left.RandomRoll, right.RandomRoll);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "randomAdvancedAsExpected", left.RandomAdvancedAsExpected, right.RandomAdvancedAsExpected);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "expectedRandomCalls", left.ExpectedRandomCalls, right.ExpectedRandomCalls);
        CompareReferenceField(
          differences,
          originalCase.Id,
          left.Tick,
          "targetClosestCallPresence",
          HasTargetClosestFirstEntry(left),
          HasTargetClosestFirstEntry(right));
        CompareReferenceField(differences, originalCase.Id, left.Tick, "ai[0]", left.Ai0Bits, right.Ai0Bits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "ai[1]", left.Ai1Bits, right.Ai1Bits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "ai[2]", left.Ai2Bits, right.Ai2Bits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "ai[3]", left.Ai3Bits, right.Ai3Bits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "velocity.X", left.VelocityXBits, right.VelocityXBits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "velocity.Y", left.VelocityYBits, right.VelocityYBits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "position.X", left.PositionXBits, right.PositionXBits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "position.Y", left.PositionYBits, right.PositionYBits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "rotation", left.RotationBits, right.RotationBits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "target", left.Target, right.Target);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "identity", $"{left.TypeId}/{left.NetId}/{left.AiStyle}", $"{right.TypeId}/{right.NetId}/{right.AiStyle}");
        CompareReferenceField(differences, originalCase.Id, left.Tick, "direction", left.Direction, right.Direction);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "sourceCanContainItems", left.SourceCanContainItems, right.SourceCanContainItems);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "sourceGravity", left.SourceGravityBits, right.SourceGravityBits);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "netUpdate", left.NetUpdate, right.NetUpdate);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "reflectsProjectiles", left.ReflectsProjectiles, right.ReflectsProjectiles);
        CompareReferenceField(differences, originalCase.Id, left.Tick, "localAI", string.Join(",", left.LocalAiBits), string.Join(",", right.LocalAiBits));
      }
    }

    return differences;
  }

  private static void CompareReferenceField<T>(
    List<ReferenceDifference> differences,
    string scenarioId,
    int tick,
    string field,
    T originalValue,
    T sourceBuildValue)
  {
    if (!EqualityComparer<T>.Default.Equals(originalValue, sourceBuildValue))
    {
      differences.Add(new ReferenceDifference(
        scenarioId,
        tick,
        field,
        originalValue?.ToString() ?? "null",
        sourceBuildValue?.ToString() ?? "null"));
    }
  }

  private static bool HasTargetClosestFirstEntry(ReferenceTick tick)
  {
    return tick.DeduplicatedFirstEntryOrder.Any(static method =>
      method.Contains(".TargetClosest", StringComparison.Ordinal));
  }

  private static List<string> ValidateSampleEvidence(
    CaseDocument cases,
    ReferenceRun original,
    ReferenceRun sourceBuild)
  {
    List<string> errors = [];
    if (cases.SchemaVersion != 1)
    {
      errors.Add($"The requested case document uses unsupported schema version {cases.SchemaVersion}.");
    }
    if (cases.Cases is null || cases.Cases.Count == 0)
    {
      errors.Add("The requested case document must contain at least one scenario.");
      return errors;
    }

    string[] expectedIds = cases.Cases
      .Where(static scenario => scenario is not null)
      .Select(static scenario => scenario.Id)
      .ToArray();
    foreach (IGrouping<string, string> duplicate in expectedIds
               .Where(static id => !string.IsNullOrWhiteSpace(id))
               .GroupBy(static id => id, StringComparer.Ordinal)
               .Where(static group => group.Count() > 1))
    {
      errors.Add($"The requested case document contains duplicate scenario id '{duplicate.Key}'.");
    }

    foreach (Scenario scenario in cases.Cases)
    {
      if (scenario is null)
      {
        errors.Add("The requested case document contains a null scenario.");
        continue;
      }

      if (string.IsNullOrWhiteSpace(scenario.Id))
      {
        errors.Add("Every requested scenario must have a nonempty id.");
      }
      if (scenario.Ticks <= 0)
      {
        errors.Add($"Requested scenario '{scenario.Id}' must expect at least one tick.");
      }
    }

    ValidateReferenceRun(cases.Cases, original, "original executable", errors);
    ValidateReferenceRun(cases.Cases, sourceBuild, "isolated source build", errors);
    return errors;
  }

  private static void ValidateReferenceRun(
    List<Scenario> scenarios,
    ReferenceRun run,
    string runName,
    List<string> errors)
  {
    if (run.Cases is null || run.Cases.Count == 0)
    {
      errors.Add($"The {runName} capture must contain at least one scenario.");
      return;
    }

    if (!run.CallTrackerFlushTimerDisabled)
    {
      errors.Add($"The {runName} capture did not confirm that the CallTracker timer was disabled.");
    }

    string[] expectedIds = scenarios
      .Where(static scenario => scenario is not null)
      .Select(static scenario => scenario.Id)
      .Order(StringComparer.Ordinal)
      .ToArray();
    string[] capturedIds = run.Cases
      .Where(static captured => captured is not null)
      .Select(static captured => captured.Id)
      .Order(StringComparer.Ordinal)
      .ToArray();
    if (!expectedIds.SequenceEqual(capturedIds, StringComparer.Ordinal))
    {
      errors.Add($"Scenario identities in the {runName} capture do not match the request file.");
    }

    foreach (Scenario scenario in scenarios)
    {
      if (scenario is null || string.IsNullOrWhiteSpace(scenario.Id))
      {
        continue;
      }

      List<ReferenceCase> matches = run.Cases
        .Where(captured => captured is not null && string.Equals(captured.Id, scenario.Id, StringComparison.Ordinal))
        .ToList();
      if (matches.Count != 1)
      {
        errors.Add($"The {runName} capture must contain exactly one case '{scenario.Id}' (found {matches.Count}).");
        continue;
      }

      ReferenceCase captured = matches[0];
      if (captured.Ticks is null || captured.Ticks.Count == 0)
      {
        errors.Add($"The {runName} case '{scenario.Id}' must contain at least one tick.");
        continue;
      }
      if (scenario.Ticks <= 0 || captured.Ticks.Count != scenario.Ticks)
      {
        errors.Add($"The {runName} case '{scenario.Id}' has {captured.Ticks.Count} ticks; the request expects {scenario.Ticks}.");
      }

      int expectedTickCount = Math.Min(scenario.Ticks, captured.Ticks.Count);
      for (int index = 0; index < captured.Ticks.Count; index++)
      {
        ReferenceTick tick = captured.Ticks[index];
        if (tick is null)
        {
          errors.Add($"The {runName} case '{scenario.Id}' has a null tick at sequence position {index + 1}.");
          continue;
        }
        if (index >= expectedTickCount || tick.Tick != index + 1)
        {
          errors.Add($"The {runName} case '{scenario.Id}' tick sequence must be 1..N in order; position {index + 1} contains tick {tick.Tick}.");
        }

        if (tick.ExpertMode != scenario.ExpertMode ||
            tick.GetGoodWorld != scenario.GetGoodWorld ||
            tick.DayTime != scenario.DayTime)
        {
          errors.Add($"Difficulty or time flags differ in the {runName} case '{scenario.Id}' tick {index + 1}.");
        }
        if (tick.TypeId != scenario.TypeId ||
            tick.NetId != scenario.NetId ||
            tick.AiStyle != scenario.AiStyle ||
            tick.Wet != scenario.Wet ||
            tick.CollideX != scenario.CollideX ||
            tick.CollideY != scenario.CollideY ||
            tick.ExpectedRandomCalls != scenario.ExpectedRandomCalls)
        {
          errors.Add($"Identity, movement flags, or random draw budget differs in the {runName} case '{scenario.Id}' tick {index + 1}.");
        }
        if (tick.DeduplicatedFirstEntryOrder is null)
        {
          errors.Add($"The {runName} case '{scenario.Id}' tick {index + 1} has no CallTracker first-entry list.");
        }
      }

      if (captured.ExpertMode != scenario.ExpertMode ||
          captured.GetGoodWorld != scenario.GetGoodWorld ||
          captured.ExpectedRandomCalls != scenario.ExpectedRandomCalls ||
          captured.NetMode != scenario.NetMode ||
          captured.WorldSurface != scenario.WorldSurface ||
          captured.SourceGravity != SlimeGravity)
      {
        errors.Add($"Reference input setup differs for the {runName} case '{scenario.Id}'.");
      }
    }
  }

  private static List<string> ValidateProvenance(
    SourceBuildProvenance provenance,
    ReferenceRun original,
    ReferenceRun sourceBuild,
    VerifierExecution verifierExecution)
  {
    List<string> errors = [];
    if (!provenance.ReferenceTreeWasReadOnly || !provenance.OriginalServerExeUnchanged)
    {
      errors.Add("Reference-source provenance does not confirm a read-only tree and unchanged original executable.");
    }
    if (!string.Equals(original.AssemblySha256, provenance.OriginalServerExeSha256, StringComparison.OrdinalIgnoreCase))
    {
      errors.Add("The original capture assembly hash does not match the source-build provenance.");
    }
    if (!string.Equals(sourceBuild.AssemblySha256, provenance.BuiltServerExeSha256, StringComparison.OrdinalIgnoreCase) ||
        !PathsEqual(sourceBuild.AssemblyPath, provenance.BuiltExecutablePath))
    {
      errors.Add("The isolated source-build capture does not match the executable recorded in provenance.");
    }
    if (!verifierExecution.MatchesBuildProvenance)
    {
      errors.Add("The running verifier artifact or its profile source files do not match the verifier build provenance.");
    }

    return errors;
  }

  private static int WriteInvalidEvidenceReport(
    string reportPath,
    CaseDocument cases,
    ReferenceRun original,
    ReferenceRun sourceBuild,
    SourceBuildProvenance provenance,
    VerifierExecution verifierExecution,
    List<string> errors)
  {
    int scenarioCount = cases.Cases?.Count ?? 0;
    int expectedTickCount = cases.Cases?.Where(static scenario => scenario is not null).Sum(static scenario => scenario.Ticks) ?? 0;
    var report = new
    {
      schemaVersion = 2,
      gate = "npc-ai-reference-differential",
      status = "invalid-evidence",
      gateOutcome = "failed",
      profileOutcome = "not-evaluated-invalid-evidence",
      sourceTrust = new
      {
        status = "not-evaluated-invalid-evidence",
        originalBinaryAssociatedWithPinnedSource = false,
        sameIlOutputContradiction = false,
      },
      createdUtc = DateTimeOffset.UtcNow,
      sourceIdentity = provenance,
      verifierExecution = CreateVerifierExecutionReport(provenance, verifierExecution),
      evidenceValidation = new
      {
        valid = false,
        errors,
      },
      inputs = cases.Cases,
      captures = new
      {
        requestedScenarioCount = scenarioCount,
        requestedTickCount = expectedTickCount,
        originalScenarioCount = original.Cases?.Count ?? 0,
        sourceBuildScenarioCount = sourceBuild.Cases?.Count ?? 0,
      },
      profileComparison = new
      {
        comparedScenarioCount = 0,
        comparedTickCount = 0,
        totalDifferences = 0,
        differences = Array.Empty<object>(),
      },
    };
    WriteJson(reportPath, report);
    Console.Error.WriteLine($"Differential gate FAILED: invalid evidence ({errors.Count} validation errors).");
    Console.WriteLine($"Differential report: {reportPath}");
    return 2;
  }

  private static ReferenceCase FindCase(ReferenceRun run, string id)
  {
    ReferenceCase? result = run.Cases.SingleOrDefault(item => item.Id == id);
    return result ?? throw new InvalidDataException($"Reference case '{id}' is missing.");
  }

  private static void AddDifference(
    ProfileComparison comparison,
    string scenario,
    int tick,
    string field,
    string profileValue,
    string referenceValue)
  {
    if (field is "position" or "localAI" or "target" or "effectPort")
    {
      comparison.EffectAndObservableDifferences++;
    }

    comparison.Differences.Add(new DifferentialDifference(
      scenario,
      tick,
      field,
      profileValue,
      referenceValue));
  }

  private static string FormatFloat(int bits)
  {
    return $"{ToFloat(bits).ToString("R", System.Globalization.CultureInfo.InvariantCulture)} (0x{unchecked((uint)bits):x8})";
  }

  private static object FloatSample(float value)
  {
    return new
    {
      value,
      bits = $"0x{unchecked((uint)Bits(value)):x8}",
    };
  }

  private static ulong GetUlpDistance(int firstBits, int secondBits)
  {
    static uint ToOrdered(int bits)
    {
      uint value = unchecked((uint)bits);
      return (value & 0x80000000u) == 0 ? value | 0x80000000u : ~value;
    }

    uint first = ToOrdered(firstBits);
    uint second = ToOrdered(secondBits);
    return first >= second ? first - second : second - first;
  }

  private static Dictionary<string, string> HashProfileSourcesAtRunTime()
  {
    string projectRoot = FindProjectRoot();
    string profileDirectory = Path.Combine(projectRoot, "src", "NSSLC", "Component", "Npc", "System");
    Dictionary<string, string> result = new(StringComparer.Ordinal);
    foreach (string fileName in ProfileSourceFiles)
    {
      string path = Path.Combine(profileDirectory, fileName);
      result.Add($"src/NSSLC/Component/Npc/System/{fileName}", HashFile(path));
    }

    return result;
  }

  private static VerifierExecution InspectVerifierExecution(SourceBuildProvenance provenance)
  {
    string assemblyPath = Path.GetFullPath(typeof(Program).Assembly.Location);
    string assemblySha256 = HashFile(assemblyPath);
    Dictionary<string, string> profileSourceFilesAtRunTime = HashProfileSourcesAtRunTime();
    string profileSourceManifestSha256AtRunTime = HashMap(profileSourceFilesAtRunTime);
    Dictionary<string, string> buildSourceFiles = provenance.VerifierSourceFiles ?? [];
    bool sourceManifestMatchesRecordedFiles = buildSourceFiles.Count > 0 &&
      string.Equals(
        HashMap(buildSourceFiles),
        provenance.VerifierSourceManifestSha256,
        StringComparison.OrdinalIgnoreCase);
    bool runtimeProfileSourcesMatchBuildManifest = ProfileSourcesMatchBuildManifest(
      profileSourceFilesAtRunTime,
      buildSourceFiles);

    return new VerifierExecution(
      assemblyPath,
      assemblySha256,
      PathsEqual(assemblyPath, provenance.VerifierAssemblyPath),
      string.Equals(assemblySha256, provenance.VerifierAssemblySha256, StringComparison.OrdinalIgnoreCase),
      sourceManifestMatchesRecordedFiles,
      profileSourceFilesAtRunTime,
      profileSourceManifestSha256AtRunTime,
      runtimeProfileSourcesMatchBuildManifest);
  }

  private static bool ProfileSourcesMatchBuildManifest(
    Dictionary<string, string> profileSourceFilesAtRunTime,
    Dictionary<string, string> buildSourceFiles)
  {
    return profileSourceFilesAtRunTime.All(pair =>
      buildSourceFiles.TryGetValue(pair.Key, out string? buildHash) &&
      string.Equals(pair.Value, buildHash, StringComparison.OrdinalIgnoreCase));
  }

  private static bool PathsEqual(string firstPath, string secondPath)
  {
    if (string.IsNullOrWhiteSpace(firstPath) || string.IsNullOrWhiteSpace(secondPath))
    {
      return false;
    }

    try
    {
      return string.Equals(
        Path.GetFullPath(firstPath).TrimEnd(Path.DirectorySeparatorChar),
        Path.GetFullPath(secondPath).TrimEnd(Path.DirectorySeparatorChar),
        StringComparison.OrdinalIgnoreCase);
    }
    catch (ArgumentException)
    {
      return false;
    }
  }

  private static object CreateVerifierExecutionReport(
    SourceBuildProvenance provenance,
    VerifierExecution verifierExecution)
  {
    return new
    {
      assemblyPathAtRunTime = verifierExecution.AssemblyPath,
      assemblySha256AtRunTime = verifierExecution.AssemblySha256,
      assemblyPathMatchesBuildProvenance = verifierExecution.AssemblyPathMatchesBuildProvenance,
      assemblySha256MatchesBuildProvenance = verifierExecution.AssemblySha256MatchesBuildProvenance,
      sourceManifestSha256AtBuild = provenance.VerifierSourceManifestSha256,
      sourceFilesAtBuild = provenance.VerifierSourceFiles,
      sourceManifestMatchesRecordedFiles = verifierExecution.SourceManifestMatchesRecordedFiles,
      profileSourceFilesAtRunTime = verifierExecution.ProfileSourceFilesAtRunTime,
      profileSourceManifestSha256AtRunTime = verifierExecution.ProfileSourceManifestSha256AtRunTime,
      runtimeProfileSourcesMatchBuildManifest = verifierExecution.RuntimeProfileSourcesMatchBuildManifest,
      matchesBuildProvenance = verifierExecution.MatchesBuildProvenance,
    };
  }

  private static string FindProjectRoot()
  {
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
    {
      directory = directory.Parent;
    }

    return directory?.FullName ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
  }

  private static string HashMap(Dictionary<string, string> values)
  {
    using SHA256 algorithm = SHA256.Create();
    string canonical = string.Join("\n", values.OrderBy(static pair => pair.Key, StringComparer.Ordinal)
      .Select(static pair => pair.Key + ":" + pair.Value));
    return Convert.ToHexString(algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
  }

  private static string HashFile(string path)
  {
    using FileStream stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
  }

  private static int Bits(float value)
  {
    return BitConverter.SingleToInt32Bits(value);
  }

  private static float ToFloat(int bits)
  {
    return BitConverter.Int32BitsToSingle(bits);
  }

  private static T ReadJson<T>(string path)
  {
    return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)
      ?? throw new InvalidDataException($"Could not deserialize {path}.");
  }

  private static void WriteJson(string path, object value)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions));
  }

  private sealed record CaseDocument(int SchemaVersion, List<Scenario> Cases);

  private sealed record VerifierExecution(
    string AssemblyPath,
    string AssemblySha256,
    bool AssemblyPathMatchesBuildProvenance,
    bool AssemblySha256MatchesBuildProvenance,
    bool SourceManifestMatchesRecordedFiles,
    Dictionary<string, string> ProfileSourceFilesAtRunTime,
    string ProfileSourceManifestSha256AtRunTime,
    bool RuntimeProfileSourcesMatchBuildManifest)
  {
    public bool MatchesBuildProvenance => AssemblyPathMatchesBuildProvenance &&
      AssemblySha256MatchesBuildProvenance &&
      SourceManifestMatchesRecordedFiles &&
      RuntimeProfileSourcesMatchBuildManifest;
  }

  private sealed record Scenario(
    string Id,
    bool ExpertMode,
    bool GetGoodWorld,
    int Ticks,
    int Width,
    int Height,
    int Life,
    int LifeMax,
    int PositionXBits,
    int PositionYBits,
    int TargetXBits,
    int TargetYBits,
    int VelocityXBits,
    int VelocityYBits,
    int Ai0Bits,
    int Ai1Bits,
    int Ai2Bits,
    int Ai3Bits,
    int RotationBits,
      int[] LocalAiBits,
    string Profile = "eye-of-cthulhu",
    int TypeId = 4,
    int NetId = 4,
    int AiStyle = 4,
    int Direction = 1,
    int TargetSlot = 0,
    bool DayTime = false,
    bool Wet = false,
    bool CollideX = false,
    bool CollideY = false,
    bool IsDamaged = false,
    bool IsBelowSurface = true,
    bool SlimeRain = false,
    int OldVelocityXBits = 0,
    int OldVelocityYBits = 0,
    bool SolidCollision = false,
    bool IsClient = false,
    bool CanContainItems = true,
    int ValueBits = 0x3f800000,
    int BaseDefense = 0,
    int NetMode = 0,
    double WorldSurface = 1000.0,
    int RandomSeed = 0,
    int ExpectedRandomCalls = 1);

  private sealed class SourceBuildProvenance
  {
    public string ReferenceRoot { get; set; } = "";
    public int PinnedSourceFileCount { get; set; }
    public string OriginalServerExeSha256 { get; set; } = "";
    public string OriginalServerExeSha256Before { get; set; } = "";
    public string OriginalServerExePath { get; set; } = "";
    public bool OriginalServerExeUnchanged { get; set; }
    public string BuiltServerExeSha256 { get; set; } = "";
    public string BuiltServerPdbSha256 { get; set; } = "";
    public string PinnedSourceManifestSha256 { get; set; } = "";
    public Dictionary<string, string> PinnedSourceHashes { get; set; } = [];
    public string CopiedProjectPath { get; set; } = "";
    public string OriginalProjectSha256 { get; set; } = "";
    public string CopiedProjectBeforePatchSha256 { get; set; } = "";
    public string CopiedProjectAfterPatchSha256 { get; set; } = "";
    public string BuiltExecutablePath { get; set; } = "";
    public string BuildCommand { get; set; } = "";
    public int BuildExitCode { get; set; }
    public int BuildWarningCount { get; set; }
    public int BuildErrorCount { get; set; }
    public string BuildLogPath { get; set; } = "";
    public string VerifierBuildLogPath { get; set; } = "";
    public int VerifierBuildWarningCount { get; set; }
    public int VerifierBuildErrorCount { get; set; }
    public string VerifierAssemblyPath { get; set; } = "";
    public string VerifierAssemblySha256 { get; set; } = "";
    public string VerifierSourceManifestSha256 { get; set; } = "";
    public Dictionary<string, string> VerifierSourceFiles { get; set; } = [];
    public string BinaryLogPath { get; set; } = "";
    public string ResolvedPropertiesLogPath { get; set; } = "";
    public string DiagnosticsRoot { get; set; } = "";
    public string ReferenceCopyLogPath { get; set; } = "";
    public string ReferenceHarnessPath { get; set; } = "";
    public string ReferenceHarnessCompileLogPath { get; set; } = "";
    public string CaseFilePath { get; set; } = "";
    public string OriginalCapturePath { get; set; } = "";
    public string OriginalCaptureLogPath { get; set; } = "";
    public string OriginalRuntimeSandbox { get; set; } = "";
    public string SourceBuildCapturePath { get; set; } = "";
    public string SourceBuildCaptureLogPath { get; set; } = "";
    public string SourceBuildRuntimeSandbox { get; set; } = "";
    public string DifferentialReportPath { get; set; } = "";
    public string PatchedProjectExplanation { get; set; } = "";
    public bool ReferenceTreeWasReadOnly { get; set; }
    public string DotnetSdkVersion { get; set; } = "";
  }

  private sealed class ReferenceRun
  {
    public string AssemblyPath { get; set; } = "";
    public string AssemblyFullName { get; set; } = "";
    public string AssemblySha256 { get; set; } = "";
    public string NpcAiIlSha256 { get; set; } = "";
    public string SlimeAiIlSha256 { get; set; } = "";
    public int NpcAiIlByteCount { get; set; }
    public bool CallTrackerFlushTimerDisabled { get; set; }
    public string RuntimeSideEffectSandbox { get; set; } = "";
    public List<ReferenceCase> Cases { get; set; } = [];
  }

  private sealed class ReferenceCase
  {
    public string Id { get; set; } = "";
    public int Seed { get; set; }
    public bool ExpertMode { get; set; }
    public bool GetGoodWorld { get; set; }
    public int ExpectedRandomCalls { get; set; }
    public int NetMode { get; set; }
    public double WorldSurface { get; set; }
    public float SourceGravity { get; set; }
    public List<ReferenceTick> Ticks { get; set; } = [];
  }

  private sealed class ReferenceTick
  {
    public int Tick { get; set; }
    public int RandomRoll { get; set; }
    public bool RandomAdvancedExactlyOnce { get; set; }
    public bool RandomAdvancedAsExpected { get; set; }
    public int ExpectedRandomCalls { get; set; }
    public int RandomPeekBefore { get; set; }
    public int RandomPeekAfter { get; set; }
    public List<string> DeduplicatedFirstEntryOrder { get; set; } = [];
    public int TypeId { get; set; }
    public int NetId { get; set; }
    public int AiStyle { get; set; }
    public int Direction { get; set; }
    public int TargetSlot { get; set; }
    public bool Wet { get; set; }
    public bool CollideX { get; set; }
    public bool CollideY { get; set; }
    public int OldVelocityXBits { get; set; }
    public int OldVelocityYBits { get; set; }
    public bool SourceCanContainItems { get; set; }
    public int SourceGravityBits { get; set; }
    public int Defense { get; set; }
    public int ValueBits { get; set; }
    public int Ai0Bits { get; set; }
    public int Ai1Bits { get; set; }
    public int Ai2Bits { get; set; }
    public int Ai3Bits { get; set; }
    public int VelocityXBits { get; set; }
    public int VelocityYBits { get; set; }
    public int PositionXBits { get; set; }
    public int PositionYBits { get; set; }
    public int[] LocalAiBits { get; set; } = [];
    public int RotationBits { get; set; }
    public int Target { get; set; }
    public bool NetUpdate { get; set; }
    public bool ReflectsProjectiles { get; set; }
    public int Life { get; set; }
    public int LifeMax { get; set; }
    public bool DayTime { get; set; }
    public bool ExpertMode { get; set; }
    public bool GetGoodWorld { get; set; }
  }

  private sealed record DifferentialDifference(
    string Scenario,
    int Tick,
    string Field,
    string ProfileValue,
    string ReferenceValue);

  private sealed record ReferenceDifference(
    string Scenario,
    int Tick,
    string Field,
    string OriginalValue,
    string SourceBuildValue);

  private sealed class ProfileComparison
  {
    public int ScenarioCount { get; set; }
    public int TickCount { get; set; }
    public int StateFieldDifferences { get; set; }
    public int VelocityFieldDifferences { get; set; }
    public int EffectAndObservableDifferences { get; set; }
    public double MaxAbsoluteVelocityDifference { get; set; }
    public ulong MaxVelocityUlpDistance { get; set; }
    public List<DifferentialDifference> Differences { get; } = [];
    public List<object> TickResults { get; } = [];
  }

  private sealed class RecordingEffectPort : INpcEyeOfCthulhuProfileEffectPort
  {
    public List<string> Events { get; } = [];

    public void SpawnDust(Vector2 position, int width, int height, Vector2 velocity)
    {
      Events.Add("SpawnDust");
    }

    public bool TrySpawnServant(Vector2 position, Vector2 velocity)
    {
      Events.Add("SpawnServant");
      return true;
    }

    public void PlaySound(int soundId, Vector2 position)
    {
      Events.Add($"PlaySound({soundId})");
    }

    public void SpawnGore(int goreId, Vector2 position, Vector2 velocity)
    {
      Events.Add($"SpawnGore({goreId})");
    }

    public void SetReflectsProjectiles(bool reflectsProjectiles)
    {
      Events.Add($"ReflectsProjectiles({reflectsProjectiles})");
    }

    public void BeginDash(Vector2 velocity)
    {
      Events.Add("BeginDash");
    }

    public void EncourageDespawn(int ticks)
    {
      Events.Add($"EncourageDespawn({ticks})");
    }

    public void RequestNetworkUpdate()
    {
      Events.Add("RequestNetworkUpdate");
    }
  }
}
