using System.Numerics;
using NLTX.PlayerInputGameplay.Creative;
using NLTX.PlayerInputGameplay.Doors;
using NLTX.PlayerInputGameplay.Equipment;
using NLTX.PlayerInputGameplay.Focus;
using NLTX.PlayerInputGameplay.Golf;
using NLTX.PlayerInputGameplay.Input;
using NLTX.PlayerInputGameplay.Interaction;
using NLTX.PlayerInputGameplay.Intent;
using NLTX.PlayerInputGameplay.Movement;
using NLTX.PlayerInputGameplay.Player;
using NLTX.PlayerInputGameplay.Pickup;
using NLTX.PlayerInputGameplay.Presentation;
using NLTX.PlayerInputGameplay.PressurePlates;
using NLTX.PlayerInputGameplay.Runtime;
using NLTX.PlayerInputGameplay.SmartCursor;

namespace NLTX.PlayerInputGameplayVerification;

internal static class Program
{
  private static int Main()
  {
    TestMainInputFrameAndQueue();
    TestCursorWorldCoordinates();
    TestPlayerRegistryAndSpawn();
    TestInputSettingsAndEvents();
    TestGolfLocalOwnership();
    TestFocusQueries();
    TestCreativeContract();
    TestDoorSmartCursorAndPressurePlate();
    TestInputProfileAndEquipmentSwap();
    TestMovementAndIntent();
    TestPickupPreviewAndCursorSnapshots();
    TestCreativeRegistryUnlockAndPowerState();
    Console.WriteLine("P08 focused verifier passed.");
    return 0;
  }

  private static void TestMainInputFrameAndQueue()
  {
    var input = new MainInputFrameComponent();
    var timing = new MainFrameTimingComponent();
    var frameSystem = new MainInputFrameSystem();
    frameSystem.BeginFrame(input, timing, 12, 24, true, true, 0.25f);
    Assert(input.MouseX == 12 && input.MouseY == 24, "The input frame should capture pointer coordinates.");
    Assert(input.MouseRightRelease, "The input frame should capture the release edge.");

    var queue = new MainThreadActionQueue();
    var executed = 0;
    queue.Enqueue(() => executed++);
    Assert(queue.Drain() == 1 && executed == 1, "The main-thread queue should execute each action once.");
    frameSystem.EndFrame(input);
    Assert(!input.MouseRightRelease, "Transient input edges should clear at frame end.");
  }

  private static void TestCursorWorldCoordinates()
  {
    var input = new MainInputFrameComponent();
    input.Capture(10, 20, false, false);
    var viewport = new ScreenViewportComponent();
    viewport.Set(new Vector2(100, 200), 800, 600);
    var query = new CursorCoordinateQuery();
    Assert(query.MouseWorld(input, viewport, 1f) == new Vector2(110, 220), "Normal gravity should offset the cursor by the viewport.");
    Assert(query.MouseWorld(input, viewport, -1f) == new Vector2(110, 780), "Reverse gravity should invert the screen Y coordinate.");
  }

  private static void TestPlayerRegistryAndSpawn()
  {
    var registry = new PlayerRegistryAdapter(2);
    var player = new object();
    registry.Bind(1, player);
    Assert(registry.TryGet(1, out var value) && ReferenceEquals(player, value), "A bound player should be readable through the adapter.");
    var selection = new LocalPlayerSelectionComponent();
    selection.Select(1, registry.Capacity);
    var spawn = new PlayerSpawnPointComponent();
    spawn.Set(32, 64);
    Assert(selection.HasSelection && spawn.IsSet && spawn.TileX == 32, "Player selection and spawn state should be explicit.");
  }

  private static void TestInputSettingsAndEvents()
  {
    var preference = new InputPreferenceComponent();
    preference.SetInventoryCancelAction("Cancel");
    var smartCursor = new SmartCursorPreferenceComponent();
    smartCursor.Set(true, false);
    var stars = new FallingStarEventStateComponent();
    stars.Start();
    stars.RecordHit();
    Assert(preference.InventoryCancelAction == "Cancel" && smartCursor.WantedByMouse && stars.StarsHit == 1, "Input preferences and event state should retain their own values.");
    stars.Clear();
    Assert(!stars.StarGame && stars.StarsHit == 0, "Transient event state should clear explicitly.");
  }

  private static void TestGolfLocalOwnership()
  {
    var tracking = new GolfLocalTrackingComponent(2);
    var system = new GolfTrackingSystem();
    system.Observe(tracking, 1.0, 7, new Vector2(3, 4), 2, true, true, true);
    Assert(tracking.LastHitGolfBallId == 7 && tracking.WaitingForBallToSettle, "Only a local active golf ball should enter local tracking.");
    system.Observe(tracking, 2.0, 8, Vector2.Zero, 3, true, true, false);
    Assert(tracking.LastHitGolfBallId == 7, "A non-local golf ball must not overwrite local tracking.");
  }

  private static void TestFocusQueries()
  {
    var query = new GameplayActivityQuery();
    Assert(query.GameplayActive(true, false), "Focused unpaused gameplay should be active.");
    Assert(!query.AllowCountingPlayerTime(false, true, true, false), "Unfocused paused gameplay must not count player time.");
    Assert(!query.AllowCountingPlayerTime(true, false, true, true), "The game menu must disable player-time counting.");
  }

  private static void TestCreativeContract()
  {
    var contract = new CreativePowerContract(3, "time_rate", CreativePowerPermissionLevel.HostOnly, CreativePowerPermissionLevel.Everyone);
    Assert(contract.PowerId == 3 && contract.ServerConfigName == "time_rate", "Creative contract metadata should remain stable.");
  }

  private static void TestDoorSmartCursorAndPressurePlate()
  {
    var doorHandlers = new DoorOpeningHandlerAdapter();
    doorHandlers.Register(10, new AlwaysOpenDoorHandler());
    var doorState = new DoorOpeningStateComponent();
    var doorSystem = new DoorOpeningSystem();
    var request = new DoorOpeningRequest(
      new DoorTileCoordinate(4, 5),
      10,
      new DoorOpeningGeometry(0, 0, 16, 32, 0, 0, 1, 2),
      1,
      1);
    Assert(doorSystem.TryBeginOpening(doorState, doorHandlers, request), "A registered door handler should accept its request.");
    Assert(doorState.OngoingOpenDoors.Count == 1, "An accepted door request should enter ongoing state.");
    Assert(doorSystem.CompleteAll(doorState) == 1, "Completed doors should leave the ongoing state.");

    var smartCursor = new SmartCursorTargetBuffer();
    var usage = new SmartCursorUsageInfo(0, 1, Vector2.Zero, Vector2.Zero, Vector2.Zero, 0, 0, 0, 1, 0, 1, 0, 0);
    new SmartCursorSystem().Scan(usage, smartCursor, (x, y) => x == y);
    Assert(smartCursor.Targets.Count == 2, "SmartCursor should retain only reachable targets from its query.");

    var occupancy = new PressurePlateOccupancyComponent(2);
    var plateSystem = new PressurePlateSystem();
    var plate = new PressurePlateCoordinate(7, 8);
    Assert(plateSystem.UpdatePlayer(occupancy, plate, 0, Vector2.One, true)?.IsPressed == true, "Entering a pressure plate should create a pressed transition.");
    Assert(plateSystem.UpdatePlayer(occupancy, plate, 0, Vector2.One, true) is null, "Repeated occupancy should not emit a duplicate transition.");
    Assert(plateSystem.UpdatePlayer(occupancy, plate, 0, Vector2.Zero, false)?.IsPressed == false, "Leaving a pressure plate should create an unpressed transition.");
  }

  private static void TestInputProfileAndEquipmentSwap()
  {
    var profile = new PlayerInputProfileConfigurationComponent();
    profile.Configure("Keyboard", true, 16, 0.3f, 0.2f, 0.25f, 0.4f, 0.1f, 0.1f, false, false, false, false, 6);
    profile.InputModes[InputMode.Keyboard].Set("Jump", new[] { "Space" });
    var triggers = new InputTriggerFrameComponent();
    triggers.Capture(new[] { "Jump" }, true);
    Assert(triggers.JustPressed.Contains("Jump"), "The first trigger frame should expose a just-pressed action.");
    triggers.Capture(Array.Empty<string>(), false);
    Assert(triggers.JustReleased.Contains("Jump"), "The next trigger frame should expose a just-released action.");

    var runtime = new PlayerInputRuntimeComponent();
    runtime.SetOriginalScreenSize(1280, 720);
    Assert(runtime.OriginalScreenSize == new Vector2(1280, 720), "PlayerInput runtime should retain the original viewport.");

    var first = new EquipmentLoadoutComponent(1, 1, 1);
    var second = new EquipmentLoadoutComponent(1, 1, 1);
    first.Armor[0] = 11;
    second.Armor[0] = 22;
    first.Dye[0] = 31;
    second.Dye[0] = 42;
    first.Hide[0] = true;
    second.Hide[0] = false;
    first.SwapWith(second);
    Assert(first.Armor[0] == 22 && second.Armor[0] == 11, "Loadout swap should preserve both armor values.");
    Assert(first.Dye[0] == 42 && second.Dye[0] == 31, "Loadout swap should preserve both dye values.");
    Assert(!first.Hide[0] && second.Hide[0], "Loadout swap should preserve both hide flags.");
  }

  private static void TestMovementAndIntent()
  {
    var movement = new PlayerMovementCapabilityComponent();
    movement.Set(12, 3.5f, 2, 1, true, false, true, false, true, false, true, false);
    var snapshot = new MountMovementRestoreSnapshot();
    snapshot.Capture(movement);
    movement.Set(0, 0f, 0, 0, false, false, false, false, false, false, false, false);
    snapshot.PasteInto(movement);
    Assert(movement.RocketTime == 12 && movement.WingTime == 3.5f && movement.JumpAgainCloud, "Mount restore should restore the captured movement capability.");

    var stool = new PortableStoolUsageComponent();
    stool.SetStats(true, 4, -2, -3);
    stool.SetInUse(true);
    Assert(stool.IsInUse && stool.HeightBoost == 4, "Portable stool usage should retain geometry stats and use state.");
    stool.Reset();
    Assert(!stool.HasAStool && !stool.IsInUse, "Portable stool reset should clear its transient state.");

    var intention = new PlayerIntentionStateComponent();
    new IntentionTrackingSystem().Track(intention, 1, 2, Vector2.One, new Vector2(2, 3), Vector2.Zero, 1, 16, GuessedPlayerIntention.Move, 4);
    var anchor = new PlayerInteractionAnchorComponent();
    anchor.Set(9, 4, 5);
    Assert(intention.Intention == GuessedPlayerIntention.Move && anchor.InUse && anchor.X == 4, "Intention and interaction anchor state should remain separate.");
    anchor.Clear();
    Assert(!anchor.InUse, "Interaction anchor clear should remove the target snapshot.");
  }

  private static void TestPickupPreviewAndCursorSnapshots()
  {
    var respawn = new MinionRespawnComponent();
    respawn.Add(100, 2);
    Assert(respawn.Contains(100, 2) && !respawn.Contains(100, 3), "Minion respawn matching should include item type and prefix.");
    var log = new PlayerItemPickupLogComponent();
    log.Add(new PlayerItemPickupLogEntry(1, 2, 3, 4));
    Assert(log.Entries.Count == 0, "Disabled pickup logging should not append entries.");
    log.SetEnabled(true);
    log.Add(new PlayerItemPickupLogEntry(1, 2, 3, 4));
    Assert(log.Entries.Count == 1, "Enabled pickup logging should append diagnostics.");

    var rejection = new PlayerRejectionPresentationComponent();
    rejection.Show(ReturnFromRejectionMenuAction.Retry, "Rejected");
    Assert(rejection.IsVisible, "A rejection message should be visible after it is shown.");
    var preview = new CharacterPreviewSettingsComponent();
    preview.Set(Vector2.Zero, new CharacterPreviewSelectionSettings(0, 4, 2, true), new CharacterPreviewSelectionSettings(4, 2, 3, false), -1);
    Assert(preview.Direction == -1 && preview.Selected.FrameCount == 4, "Character preview settings should preserve selection animation data.");

    var cursor = new CursorItemPreviewComponent();
    cursor.SetPreview(7, 1, 2);
    cursor.SetCursorItem(new CursorItemSnapshot(8, 2, 3));
    Assert(cursor.GetCombinedPreview().Stack == 5, "Cursor item preview should combine the preview and cursor stacks.");
    var chest = new PositionedChestSnapshot(5, new Vector2(16, 32));
    Assert(chest.ChestId == 5 && chest.Position.Y == 32, "Positioned chest should be a value snapshot.");
  }

  private static void TestCreativeRegistryUnlockAndPowerState()
  {
    var registry = new CreativePowerRegistryComponent();
    var registrySystem = new CreativePowerRegistrySystem();
    registrySystem.Initialize(registry, new[]
    {
      new CreativePowerContract(1, "power_one", CreativePowerPermissionLevel.Everyone, CreativePowerPermissionLevel.Everyone)
    });
    Assert(registry.IsInitialized && registry.Count == 1 && registry.TryGet(1, out _), "Creative registry should initialize and index powers by ID.");

    var catalog = new CreativeSacrificeCatalogComponent();
    catalog.SetRequiredCount(10, 3);
    var progress = new CreativeUnlockProgressComponent();
    progress.RecordSacrifice("item-10", 10, 3, catalog.GetRequiredCount(10));
    Assert(progress.DrainNewlyUnlocked().SequenceEqual(new[] { 10 }), "Creative sacrifice progress should emit a newly unlocked item.");
    progress.RecordTeammateUnlock(11, "player-2");
    Assert(progress.AnyNewUnlocksFromTeammates && progress.TeammateUnlocks[11] == "player-2", "Teammate unlocks should remain a separate notification projection.");

    var perPlayer = new PerPlayerCreativePowerStateComponent(2);
    perPlayer.ConfigureSlider(0.5f);
    perPlayer.SetEnabled(1, true);
    perPlayer.SetSlider(1, 0.75f);
    Assert(perPlayer.IsEnabled(1) && perPlayer.GetSlider(1) == 0.75f, "Per-player creative power state should be indexed by player slot.");
    var shared = new SharedCreativePowerStateComponent();
    shared.SetEnabled(true);
    shared.SetTargetTimeRate(2);
    shared.SetStrengthMultiplier(1.25f);
    Assert(shared.Enabled && shared.TargetTimeRate == 2 && shared.StrengthMultiplierToGiveNpcs == 1.25f, "Shared creative power state should retain shared derived values.");
  }

  private sealed class AlwaysOpenDoorHandler : IDoorOpeningHandler
  {
    public bool CanOpen(DoorOpeningRequest request)
    {
      return request.HandlerTileType == 10;
    }
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
