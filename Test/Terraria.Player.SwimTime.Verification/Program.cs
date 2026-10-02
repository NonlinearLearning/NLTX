using Terraria.Player.Environment;

static void AssertEqual(int expected, int actual, string message)
{
  if (expected != actual)
  {
    throw new InvalidOperationException($"{message}: expected {expected}, got {actual}.");
  }
}

PlayerSwimTimeSystem system = new();
PlayerEnvironmentMobilityStateComponent state = new();

system.RefreshForMermanJump(
  state,
  new PlayerMermanJumpInput(
    HasActiveJumpCounter: false,
    IsAirborne: true,
    IsMerman: true,
    MountActive: false,
    CartMount: false));
AssertEqual(0, state.SwimTime, "Merman refresh should require an active jump counter");

system.RefreshForMermanJump(
  state,
  new PlayerMermanJumpInput(
    HasActiveJumpCounter: true,
    IsAirborne: true,
    IsMerman: true,
    MountActive: false,
    CartMount: false));
AssertEqual(30, state.SwimTime, "Merman jump should start swim time");

for (int frame = 0; frame < 19; frame++)
{
  system.TickFrame(state, isWet: true);
}

AssertEqual(11, state.SwimTime, "Wet frames should preserve the countdown above the threshold");
system.RefreshForMermanJump(
  state,
  new PlayerMermanJumpInput(
    HasActiveJumpCounter: true,
    IsAirborne: true,
    IsMerman: true,
    MountActive: false,
    CartMount: false));
AssertEqual(11, state.SwimTime, "Merman jump should preserve time above the threshold");

system.TickFrame(state, isWet: true);
AssertEqual(10, state.SwimTime, "Wet frame should reach the merman refresh threshold");
system.RefreshForMermanJump(
  state,
  new PlayerMermanJumpInput(
    HasActiveJumpCounter: true,
    IsAirborne: true,
    IsMerman: true,
    MountActive: true,
    CartMount: true));
AssertEqual(10, state.SwimTime, "Cart mount should suppress merman refresh");

system.RefreshForMermanJump(
  state,
  new PlayerMermanJumpInput(
    HasActiveJumpCounter: true,
    IsAirborne: true,
    IsMerman: true,
    MountActive: false,
    CartMount: false));
AssertEqual(30, state.SwimTime, "Merman jump should refresh at the threshold");

system.TickFrame(state, isWet: false);
AssertEqual(0, state.SwimTime, "Dry frame should clear swim time after decrement");
system.RefreshForFlipperJump(
  state,
  new PlayerFlipperJumpInput(CanStartNewJump: true, IsWet: true, HasFlippers: true));
AssertEqual(30, state.SwimTime, "Wet flipper jump should start swim time");

system.TickFrame(state, isWet: true);
AssertEqual(29, state.SwimTime, "Wet frame should decrement refreshed swim time");
system.RefreshForFlipperJump(
  state,
  new PlayerFlipperJumpInput(CanStartNewJump: true, IsWet: true, HasFlippers: true));
AssertEqual(29, state.SwimTime, "Flipper jump should not refresh nonzero time");

system.TickFrame(state, isWet: false);
AssertEqual(0, state.SwimTime, "Dry frame should clear swim time after decrement");

system.TickFrame(state, isWet: false);
AssertEqual(0, state.SwimTime, "Zero swim time should remain zero");

Console.WriteLine("PASS: player swim-time refresh thresholds and frame expiry");
