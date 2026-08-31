using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Player;
using Terraria.Dome.Simulation.WorldModel;

using DomeSimulation first = new(new WorldGrid(400, 300));
using DomeSimulation second = new(new WorldGrid(400, 300));
PlayerHandle firstPlayer = first.CreatePlayer(new SimulationVector(10.0f, 0.0f));
PlayerHandle secondPlayer = second.CreatePlayer(new SimulationVector(10.0f, 0.0f));
PlayerInput[] inputs =
[
  new PlayerInput(firstPlayer, false, true, false, false),
  new PlayerInput(firstPlayer, false, false, true, false),
  new PlayerInput(firstPlayer, true, false, false, false)
];

for (int index = 0; index < inputs.Length; index++)
{
  PlayerInput input = inputs[index] with { Player = secondPlayer };
  first.Tick(new SimulationInputBatch(inputs[index]));
  second.Tick(new SimulationInputBatch(input));
}

PlayerSnapshot firstSnapshot = first.CreateSnapshot().FindPlayer(firstPlayer);
PlayerSnapshot secondSnapshot = second.CreateSnapshot().FindPlayer(secondPlayer);
if (firstSnapshot != secondSnapshot with { Player = firstPlayer })
{
  throw new InvalidOperationException("Equivalent player replays produced different snapshots.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  UseItem: true)));
if (!first.TryGetPlayerInputEdges(firstPlayer, out PlayerInputEdges pressed) ||
    !pressed.WasUseItemPressed || pressed.WasUseItemReleased)
{
  throw new InvalidOperationException("Player UseItem press edge was not exposed for its tick.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  UseItem: false)));
if (!first.TryGetPlayerInputEdges(firstPlayer, out PlayerInputEdges released) ||
    released.WasUseItemPressed || !released.WasUseItemReleased)
{
  throw new InvalidOperationException("Player UseItem release edge was not exposed for its tick.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  Up: true,
  UseTile: true)));
if (!first.TryGetPlayerInputState(firstPlayer, out PlayerInputState inputState) ||
    !inputState.Up || !inputState.UseTile)
{
  throw new InvalidOperationException("Player Up and UseTile inputs were not preserved.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  UseTile: false)));
first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  UseTile: true)));
if (!first.TryGetPlayerInputEdges(firstPlayer, out PlayerInputEdges tilePressed) ||
    !tilePressed.WasUseTilePressed)
{
  throw new InvalidOperationException("Player UseTile press edge was not exposed.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  UseTile: false)));
if (!first.TryGetPlayerInputEdges(firstPlayer, out PlayerInputEdges tileReleased) ||
    !tileReleased.WasUseTileReleased)
{
  throw new InvalidOperationException("Player UseTile release edge was not exposed.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  Dash: true)));
if (!first.TryGetPlayerInputState(firstPlayer, out PlayerInputState dashState) ||
    !dashState.Dash)
{
  throw new InvalidOperationException("Player Dash input was not preserved.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  MoveLeft: true,
  MoveRight: false,
  Jump: true,
  Fire: true,
  UseItem: true)));
if (!first.TryGetPlayerInputState(firstPlayer, out PlayerInputState completeState) ||
    !completeState.MoveLeft || !completeState.Jump || !completeState.Fire ||
    !completeState.UseItem)
{
  throw new InvalidOperationException("Player input state projection omitted core control flags.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  SelectedSlot: 2,
  Facing: -1)));
if (!first.TryGetPlayerInputState(firstPlayer, out PlayerInputState selectionState) ||
    selectionState.Facing != -1 || selectionState.SelectedSlot != 2)
{
  throw new InvalidOperationException("Player input state projection omitted facing or selection.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  MoveLeft: true,
  MoveRight: true,
  Jump: true,
  Fire: true,
  Down: true,
  Up: true,
  UseTile: true,
  Dash: true)));
PlayerSnapshot inputSnapshot = first.CreateSnapshot().FindPlayer(firstPlayer);
PlayerStateSnapshot inputStateSnapshot = first.CreatePlayerStateSnapshot(firstPlayer);
if (!inputSnapshot.MoveLeft || !inputSnapshot.MoveRight || !inputSnapshot.Jump ||
    !inputSnapshot.Fire || !inputSnapshot.Down || !inputSnapshot.Up ||
    !inputSnapshot.UseTile || !inputSnapshot.Dash ||
    !inputStateSnapshot.MoveLeft || !inputStateSnapshot.MoveRight ||
    !inputStateSnapshot.Jump || !inputStateSnapshot.Fire ||
    !inputStateSnapshot.Down || !inputStateSnapshot.Up ||
    !inputStateSnapshot.UseTile || !inputStateSnapshot.Dash)
{
  throw new InvalidOperationException("Player snapshot input projection omitted typed controls.");
}
Console.WriteLine("PASS: typed Player controls project through both snapshots");

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  Dash: false)));
first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  Dash: true)));
if (!first.TryGetPlayerInputEdges(firstPlayer, out PlayerInputEdges dashPressed) ||
    !dashPressed.WasDashPressed)
{
  throw new InvalidOperationException("Player Dash press edge was not exposed.");
}

first.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  false,
  false,
  false,
  false,
  Dash: false)));
if (!first.TryGetPlayerInputEdges(firstPlayer, out PlayerInputEdges dashReleased) ||
    !dashReleased.WasDashReleased)
{
  throw new InvalidOperationException("Player Dash release edge was not exposed.");
}

try
{
  first.Tick(new SimulationInputBatch(
    new PlayerInput(firstPlayer, false, false, false, false),
    new PlayerInput(firstPlayer, true, false, false, false)));
  throw new InvalidOperationException("Duplicate player input was silently accepted.");
}
catch (ArgumentException)
{
}

try
{
  first.Tick(new SimulationInputBatch(
    new PlayerInput(firstPlayer, false, false, false, false, Facing: 2)));
  throw new InvalidOperationException("An invalid player facing was silently accepted.");
}
catch (ArgumentException)
{
}

string replayText = string.Join(
  "|",
  firstSnapshot.Position.X.ToString("R", CultureInfo.InvariantCulture),
  firstSnapshot.Position.Y.ToString("R", CultureInfo.InvariantCulture),
  firstSnapshot.Velocity.X.ToString("R", CultureInfo.InvariantCulture),
  firstSnapshot.Velocity.Y.ToString("R", CultureInfo.InvariantCulture),
  firstSnapshot.Facing,
  firstSnapshot.IsGrounded,
  firstSnapshot.Health,
  firstSnapshot.IsActive,
  firstSnapshot.RespawnTicks,
  firstSnapshot.Mana,
  firstSnapshot.MaximumMana);
string replayHash = Convert.ToHexString(
  SHA256.HashData(Encoding.UTF8.GetBytes(replayText)));
Console.WriteLine(
  $"PASS: player input replay is deterministic and duplicate inputs are rejected; " +
  $"REPLAY_HASH={replayHash}");
