using Terraria.Player.Interaction;

PlayerTileTargetCoordinate standard = PlayerTargetingSystem.ResolveCoordinate(
  new PlayerTileTargetCoordinateInput(
    MouseX: 96,
    MouseY: 160,
    ScreenPositionX: 800f,
    ScreenPositionY: 320f,
    ScreenHeight: 720,
    GravityDirection: 1f,
    MaxTilesX: 200,
    MaxTilesY: 120));
Assert(standard.TileTargetX == 56 && standard.TileTargetY == 30,
  "Normal gravity should project mouse and screen coordinates into tiles.");

PlayerTileTargetCoordinate invertedGravity = PlayerTargetingSystem.ResolveCoordinate(
  new PlayerTileTargetCoordinateInput(
    MouseX: 96,
    MouseY: 160,
    ScreenPositionX: 800f,
    ScreenPositionY: 320f,
    ScreenHeight: 720,
    GravityDirection: -1f,
    MaxTilesX: 200,
    MaxTilesY: 120));
Assert(invertedGravity.TileTargetX == 56 && invertedGravity.TileTargetY == 55,
  "Inverted gravity should mirror the mouse Y coordinate within the screen.");

PlayerTileTargetCoordinate clamped = PlayerTargetingSystem.ResolveCoordinate(
  new PlayerTileTargetCoordinateInput(
    MouseX: 5000,
    MouseY: 0,
    ScreenPositionX: 0f,
    ScreenPositionY: -100f,
    ScreenHeight: 720,
    GravityDirection: 1f,
    MaxTilesX: 200,
    MaxTilesY: 120));
Assert(clamped.TileTargetX == 195 && clamped.TileTargetY == 5,
  "Target coordinates should clamp to five tiles from the world edges.");

PlayerTileTargetCoordinate narrowWorld = PlayerTargetingSystem.ResolveCoordinate(
  new PlayerTileTargetCoordinateInput(
    MouseX: 160,
    MouseY: 160,
    ScreenPositionX: 0f,
    ScreenPositionY: 0f,
    ScreenHeight: 720,
    GravityDirection: 1f,
    MaxTilesX: 8,
    MaxTilesY: 8));
Assert(narrowWorld.TileTargetX == 5 && narrowWorld.TileTargetY == 5,
  "Lower-bound clamping should remain after upper-bound clamping in narrow worlds.");

var target = new PlayerTileTargetCoordinate(50, 20);
PlayerTileTargetCoordinate leftAxeTarget = PlayerTargetingSystem.ResolveAxeNeighborTarget(
  target,
  AxeInput(leftTileActive: true, leftTileType: 323, leftTileFrameY: 5));
Assert(leftAxeTarget.TileTargetX == 49,
  "A qualifying left neighbor should move the target one tile left.");

PlayerTileTargetCoordinate rightAxeTarget = PlayerTargetingSystem.ResolveAxeNeighborTarget(
  target,
  AxeInput(rightTileActive: true, rightTileType: 323, rightTileFrameY: -5));
Assert(rightAxeTarget.TileTargetX == 51,
  "A qualifying right neighbor should move the target one tile right.");

PlayerTileTargetCoordinate leftBoundary = PlayerTargetingSystem.ResolveAxeNeighborTarget(
  target,
  AxeInput(leftTileActive: true, leftTileType: 323, leftTileFrameY: 4));
Assert(leftBoundary == target,
  "The left neighbor frame boundary should remain strict.");

PlayerTileTargetCoordinate rightBoundary = PlayerTargetingSystem.ResolveAxeNeighborTarget(
  target,
  AxeInput(rightTileActive: true, rightTileType: 323, rightTileFrameY: -4));
Assert(rightBoundary == target,
  "The right neighbor frame boundary should remain strict.");

PlayerTileTargetCoordinate leftBranchPriority = PlayerTargetingSystem.ResolveAxeNeighborTarget(
  target,
  AxeInput(
    leftTileActive: true,
    leftTileType: 323,
    leftTileFrameY: 4,
    rightTileActive: true,
    rightTileType: 323,
    rightTileFrameY: -5));
Assert(leftBranchPriority == target,
  "A matching left neighbor should prevent evaluating the right fallback branch.");

PlayerTileTargetCoordinate disabledAxe = PlayerTargetingSystem.ResolveAxeNeighborTarget(
  target,
  AxeInput(axePower: 0, rightTileActive: true, rightTileType: 323, rightTileFrameY: -5));
Assert(disabledAxe == target,
  "Axe neighbor correction should not run without an axe.");

var tilePort = new RecordingTileTargetWorldPort();
tilePort.SetTile(49, 20, new PlayerTileTargetTileFacts(true, 323, 4));
tilePort.SetTile(51, 20, new PlayerTileTargetTileFacts(true, 323, -5));
PlayerTileTargetCoordinate composedTarget = PlayerTargetingSystem.ResolveTarget(
  new PlayerTileTargetCoordinateInput(
    MouseX: 0,
    MouseY: 0,
    ScreenPositionX: 800f,
    ScreenPositionY: 320f,
    ScreenHeight: 720,
    GravityDirection: 1f,
    MaxTilesX: 200,
    MaxTilesY: 120),
  new PlayerTileTargetToolInput(AxePower: 1, CreateWall: 0, HammerPower: 1),
  tilePort);
Assert(composedTarget == new PlayerTileTargetCoordinate(50, 20),
  "A qualifying left neighbor should keep priority when its frame does not correct the target.");
Assert(tilePort.Calls.SequenceEqual(new[]
  {
    "ensure:49,20",
    "ensure:51,20",
    "ensure:50,20",
    "read:50,20",
    "read:49,20"
  }),
  "Tile initialization and neighbor reads should preserve the legacy order " +
  "and left-branch short circuit.");

PlayerTileTargetRange defaultRange = PlayerTargetingSystem.ResolveRangeBaseline(
  new PlayerTileTargetRangeInput(
    IsLocalPlayer: true,
    IsDisplayDollOrInanimate: false,
    IsJourneyMode: false,
    FarPlacementRangePowerUnlocked: false,
    FarPlacementRangePowerEnabledForPlayer: false,
    CurrentTileRangeX: 12,
    CurrentTileRangeY: 9));
Assert(defaultRange == new PlayerTileTargetRange(5, 3),
  "The local player should reset to the Version4 default tile range.");

PlayerTileTargetRange journeyRange = PlayerTargetingSystem.ResolveRangeBaseline(
  new PlayerTileTargetRangeInput(
    IsLocalPlayer: true,
    IsDisplayDollOrInanimate: false,
    IsJourneyMode: true,
    FarPlacementRangePowerUnlocked: true,
    FarPlacementRangePowerEnabledForPlayer: true,
    CurrentTileRangeX: 12,
    CurrentTileRangeY: 9));
Assert(journeyRange == new PlayerTileTargetRange(18, 14),
  "Enabled and unlocked Journey range should apply the Version4 multiplier and bonus.");

PlayerTileTargetRange disabledJourneyRange = PlayerTargetingSystem.ResolveRangeBaseline(
  new PlayerTileTargetRangeInput(
    IsLocalPlayer: true,
    IsDisplayDollOrInanimate: false,
    IsJourneyMode: true,
    FarPlacementRangePowerUnlocked: true,
    FarPlacementRangePowerEnabledForPlayer: false,
    CurrentTileRangeX: 12,
    CurrentTileRangeY: 9));
Assert(disabledJourneyRange == new PlayerTileTargetRange(5, 3),
  "A disabled Journey power should leave the local baseline unchanged.");

PlayerTileTargetRange lockedJourneyRange = PlayerTargetingSystem.ResolveRangeBaseline(
  new PlayerTileTargetRangeInput(
    IsLocalPlayer: true,
    IsDisplayDollOrInanimate: false,
    IsJourneyMode: true,
    FarPlacementRangePowerUnlocked: false,
    FarPlacementRangePowerEnabledForPlayer: true,
    CurrentTileRangeX: 12,
    CurrentTileRangeY: 9));
Assert(lockedJourneyRange == new PlayerTileTargetRange(5, 3),
  "A locked Journey power should leave the local baseline unchanged.");

PlayerTileTargetRange sharedRange = PlayerTargetingSystem.ResolveRangeBaseline(
  new PlayerTileTargetRangeInput(
    IsLocalPlayer: false,
    IsDisplayDollOrInanimate: false,
    IsJourneyMode: true,
    FarPlacementRangePowerUnlocked: true,
    FarPlacementRangePowerEnabledForPlayer: true,
    CurrentTileRangeX: 12,
    CurrentTileRangeY: 9));
Assert(sharedRange == new PlayerTileTargetRange(12, 9),
  "Non-local updates should preserve the existing shared range workspace.");

PlayerTileTargetRange displayDollRange = PlayerTargetingSystem.ResolveRangeBaseline(
  new PlayerTileTargetRangeInput(
    IsLocalPlayer: true,
    IsDisplayDollOrInanimate: true,
    IsJourneyMode: true,
    FarPlacementRangePowerUnlocked: true,
    FarPlacementRangePowerEnabledForPlayer: true,
    CurrentTileRangeX: 12,
    CurrentTileRangeY: 9));
Assert(displayDollRange == new PlayerTileTargetRange(12, 9),
  "Display-doll and inanimate updates should preserve the existing range.");

var rangeState = new PlayerTileTargetingAndRangeStateComponent();
PlayerTargetingSystem.CaptureEffectiveRange(rangeState, journeyRange);
Assert(rangeState.LastTileRangeX == 18 && rangeState.LastTileRangeY == 14,
  "Effective range should be committed to the per-player compatibility snapshot.");

Console.WriteLine("PASS: player tile target, range baseline, axe correction and legacy clamp order");

static PlayerTileTargetAxeCorrectionInput AxeInput(
  int axePower = 1,
  bool centerTileActive = false,
  int createWall = 0,
  int hammerPower = 0,
  bool leftTileActive = false,
  int leftTileType = 0,
  int leftTileFrameY = 0,
  bool rightTileActive = false,
  int rightTileType = 0,
  int rightTileFrameY = 0)
{
  return new PlayerTileTargetAxeCorrectionInput(
    axePower,
    centerTileActive,
    createWall,
    hammerPower,
    leftTileActive,
    leftTileType,
    leftTileFrameY,
    rightTileActive,
    rightTileType,
    rightTileFrameY);
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class RecordingTileTargetWorldPort : IPlayerTileTargetWorldPort
{
  private readonly Dictionary<(int X, int Y), PlayerTileTargetTileFacts> _tiles = new();

  public List<string> Calls { get; } = new();

  public void SetTile(int tileX, int tileY, in PlayerTileTargetTileFacts tile)
  {
    _tiles[(tileX, tileY)] = tile;
  }

  public void EnsureTileExists(int tileX, int tileY)
  {
    Calls.Add($"ensure:{tileX},{tileY}");
    _tiles.TryAdd((tileX, tileY), default);
  }

  public PlayerTileTargetTileFacts ReadTile(int tileX, int tileY)
  {
    Calls.Add($"read:{tileX},{tileY}");
    return _tiles[(tileX, tileY)];
  }
}
