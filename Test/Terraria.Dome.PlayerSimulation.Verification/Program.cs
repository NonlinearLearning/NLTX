using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Terraria.Dome.Simulation;
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
