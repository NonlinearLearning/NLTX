using NLTX.ClientPresentation.AudioParticlesCinematics.Animation;
using NLTX.ClientPresentation.AudioParticlesCinematics.Environment;
using NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

namespace NLTX.ClientPresentation.AudioParticlesCinematicsVerification;

internal static class Program
{
  private static int Main()
  {
    TestNpcFrameCatalogRejectsInvalidValues();
    TestAnimationAndChatValuesRejectInvalidValues();
    TestCageAnimationQueryWrapsFrames();
    TestCageAnimationSystemUpdatesStateAndProjection();
    TestClientPlayerAdapterDetaches();
    TestBackgroundSelectionIsPureAndBounded();
    TestAmbientAudioSystemEmitsThresholdCommands();
    TestChatMonitorAdapterOwnsExternalHandle();
    TestParticleRepelAndLifecycleBoundaries();
    TestParticleUpdateSystemAdvancesAndReclaimsParticles();
    Console.WriteLine("P19 verifier passed.");
    return 0;
  }

  private static void TestNpcFrameCatalogRejectsInvalidValues()
  {
    AssertThrows<ArgumentOutOfRangeException>(
      () => new NpcAnimationFrameVisualsComponent(new[] { 1, 0 }),
      "NPC frame catalogs must reject non-positive frame counts.");

    NpcAnimationFrameVisualsComponent catalog =
      new(new[] { 1, 3, 5 });
    Assert(catalog.TryGetFrameCount(1, out int frameCount),
      "A known NPC type should resolve a frame count.");
    Assert(frameCount == 3, "The NPC frame catalog should preserve the configured count.");
    Assert(!catalog.TryGetFrameCount(3, out _),
      "An unknown NPC type should not fabricate a frame count.");
  }

  private static void TestAnimationAndChatValuesRejectInvalidValues()
  {
    AssertThrows<ArgumentOutOfRangeException>(
      () => new CageAnimationKey(-1, 0),
      "Cage animation keys must reject negative container identifiers.");

    AssertThrows<ArgumentOutOfRangeException>(
      () => new CageAnimationFrameState(0, -1, 0),
      "Cage animation state must reject negative frame counters.");

    AssertThrows<ArgumentException>(
      () => new ChatMessage(" ", ChatMessageKind.System),
      "Chat messages must reject blank text.");
  }

  private static void TestCageAnimationQueryWrapsFrames()
  {
    CageAnimationStepResult result = CageAnimationQuery.AdvanceFrame(
      currentFrame: 2,
      frameCount: 3,
      ticksSinceAdvance: 4,
      ticksPerFrame: 4);

    Assert(result.Frame == 0, "A completed final frame should wrap to zero.");
    Assert(result.Advanced, "A frame should advance when the tick threshold is reached.");
    Assert(result.TicksSinceAdvance == 0, "The frame counter should reset after advancing.");
  }

  private static void TestCageAnimationSystemUpdatesStateAndProjection()
  {
    CageAnimationKey key = new(ContainerId: 4, SlotIndex: 2);
    CageBirdAnimationVisualsComponent component = new();
    component.Set(key, new CageAnimationFrameState(Frame: 2, TicksSinceAdvance: 4, Mode: 1));

    CageAnimationSystem system = new();
    CageAnimationStepResult result = system.Advance(
      component,
      key,
      frameCount: 3,
      ticksPerFrame: 4);

    Assert(result.Advanced, "The cage system should advance at the frame threshold.");
    Assert(component.TryGet(key, out CageAnimationFrameState state),
      "The cage component should retain the updated frame state.");
    Assert(state.Frame == 0 && state.TicksSinceAdvance == 0,
      "The cage system should wrap and reset the frame state.");

    CageAnimationProjection projection = system.Project(key, state);
    Assert(projection.Key == key && projection.Frame == 0 && projection.Mode == 1,
      "The cage projection should be a read-only view of the updated state.");
  }

  private static void TestClientPlayerAdapterDetaches()
  {
    ClientPlayerPresentationAdapterState adapter = new();
    object clientPlayer = new();
    adapter.Attach(clientPlayer);
    Assert(adapter.IsAttached && ReferenceEquals(adapter.ClientPlayer, clientPlayer),
      "The presentation adapter should retain the attached external player reference.");

    adapter.Detach();
    Assert(!adapter.IsAttached && adapter.ClientPlayer is null,
      "The presentation adapter should release the external player reference.");
  }

  private static void TestBackgroundSelectionIsPureAndBounded()
  {
    BackgroundLayerCatalogComponent catalog = new();
    catalog.SetLayerSet("forest", new[] { 4, 5, 6 });

    BackgroundSelectionResult result = BackgroundSelectionQuery.Select(
      catalog,
      "forest",
      cameraPosition: 12,
      parallaxScale: 0.5f);

    Assert(result.LayerIds.SequenceEqual(new[] { 4, 5, 6 }),
      "Background selection should preserve the selected layer catalog.");
    Assert(result.ParallaxOffset == 6,
      "Background selection should calculate a deterministic parallax offset.");

    AssertThrows<KeyNotFoundException>(
      () => BackgroundSelectionQuery.Select(catalog, "missing", 0, 1),
      "Background selection should reject an unknown layer set.");
  }

  private static void TestAmbientAudioSystemEmitsThresholdCommands()
  {
    AmbientAudioVisualsComponent state = new();
    AmbientAudioSystem system = new();
    IReadOnlyList<AmbientAudioCommand> commands = system.Update(
      state,
      new AmbientAudioInput(
        Wind: 0.5f,
        Rain: 0.6f,
        WaterfallPosition: new System.Numerics.Vector2(10, 20),
        WaterfallStrength: 0.75f,
        LavafallPosition: System.Numerics.Vector2.Zero,
        LavafallStrength: 0,
        LavaPosition: System.Numerics.Vector2.Zero,
        LavaStrength: 0));

    Assert(state.ShouldUseWindyDayMusic && state.ShouldUseStormMusic,
      "Ambient state should derive windy and storm music flags from thresholds.");
    Assert(commands.Any(command => command.Kind == AmbientAudioKind.Wind),
      "Ambient state should emit a windy-day command when the threshold is crossed.");
    Assert(commands.Any(command => command.Kind == AmbientAudioKind.Storm),
      "Ambient state should emit a storm command when the threshold is crossed.");
    Assert(commands.Any(command => command.Kind == AmbientAudioKind.Waterfall),
      "Ambient state should emit a waterfall command for an active waterfall.");
  }

  private static void TestChatMonitorAdapterOwnsExternalHandle()
  {
    RecordingChatMonitor monitor = new();
    ChatMonitorAdapterState adapter = new();
    adapter.Attach(monitor);
    adapter.Publish(new ChatMessage("hello", ChatMessageKind.System));
    Assert(monitor.Messages.Count == 1 && monitor.Messages[0].Text == "hello",
      "Chat projection should publish through the attached monitor.");

    adapter.Detach();
    AssertThrows<InvalidOperationException>(
      () => adapter.Publish(new ChatMessage("late", ChatMessageKind.System)),
      "Chat publishing should fail after the monitor is detached.");
  }

  private static void TestParticleRepelAndLifecycleBoundaries()
  {
    AssertThrows<ArgumentOutOfRangeException>(
      () => new ParticleRepelValue(
        System.Numerics.Vector2.Zero,
        System.Numerics.Vector2.Zero,
        -1,
        false),
      "Particle repel values must reject negative radii.");

    ParticleLayerRegistryAdapterState registry = new();
    object layer = new();
    registry.Attach("world", layer);
    Assert(registry.TryGet("world", out object? attachedLayer) &&
      ReferenceEquals(layer, attachedLayer),
      "The particle layer adapter should retain external layers behind its boundary.");

    ParticleLifecycleState lifecycle = new();
    lifecycle.MarkRemoved();
    lifecycle.ResetToPool();
    Assert(lifecycle.IsRestingInPool && !lifecycle.ShouldBeRemovedFromRenderer,
      "Particle lifecycle cleanup should be idempotent and pool-safe.");
  }

  private static void TestParticleUpdateSystemAdvancesAndReclaimsParticles()
  {
    StarVisualsComponent stars = new();
    stars.Set(1, new StarVisualState(
      Position: new System.Numerics.Vector2(2, 3),
      Scale: 1,
      Rotation: 0,
      Type: 2,
      Twinkle: 0.5f,
      TwinkleSpeed: 1,
      RotationSpeed: 2,
      Falling: true,
      Hidden: false,
      FallSpeed: new System.Numerics.Vector2(0, 4),
      FallTime: 0,
      Velocity: new System.Numerics.Vector2(1, 0),
      FadeIn: 0.5f));

    RainVisualsComponent rain = new();
    rain.Set(3, new RainVisualState(
      Position: System.Numerics.Vector2.Zero,
      Velocity: new System.Numerics.Vector2(0, 10),
      Alpha: 1,
      Active: true,
      Kill: false,
      Type: 1));

    ParticleUpdateSystem system = new();
    system.Update(stars, rain, deltaSeconds: 0.5f, worldBottom: 4);

    Assert(stars.TryGet(1, out StarVisualState star) &&
      star.Position == new System.Numerics.Vector2(2.5f, 5),
      "Particle updates should advance star position using velocity and fall speed.");
    Assert(!rain.TryGet(3, out _),
      "Particle updates should remove rain that crosses the configured world bound.");

    IReadOnlyList<StarVisualState> drawState =
      ParticleDrawProjection.Project(stars);
    Assert(drawState.Count == 1 && drawState[0].Type == 2,
      "Particle draw projection should expose a read-only star snapshot.");
  }

  private sealed class RecordingChatMonitor : IChatMonitorPort
  {
    public List<ChatMessage> Messages { get; } = new();

    public void Publish(ChatMessage message)
    {
      Messages.Add(message);
    }
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}
