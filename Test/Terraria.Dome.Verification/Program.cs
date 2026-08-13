using System;
using Terraria.Dome.Client;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Transport;

DomeSimulation simulation = new();
PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
NpcHandle npc = simulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
SimulationSnapshot before = simulation.CreateSnapshot();

simulation.Tick(new SimulationInputBatch(
  new PlayerInput(player, MoveLeft: false, MoveRight: true, Jump: false, Fire: false)));

SimulationSnapshot after = simulation.CreateSnapshot();
PlayerSnapshot beforePlayer = before.FindPlayer(player);
PlayerSnapshot afterPlayer = after.FindPlayer(player);

if (afterPlayer.Position.X <= beforePlayer.Position.X)
{
  throw new InvalidOperationException("Right input must move the server-owned player right.");
}

Console.WriteLine("PASS: player movement");

DomeSimulation jumpSimulation = new();
PlayerHandle jumpingPlayer = jumpSimulation.CreatePlayer(new SimulationVector(0.0f, 0.0f));
jumpSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(jumpingPlayer, MoveLeft: false, MoveRight: false, Jump: true, Fire: false)));
PlayerSnapshot airbornePlayer = jumpSimulation.CreateSnapshot().FindPlayer(jumpingPlayer);

if (airbornePlayer.Position.Y <= 0.0f || airbornePlayer.IsGrounded)
{
  throw new InvalidOperationException("Jump input must move a grounded player into the air.");
}

for (int index = 0; index < 8; index++)
{
  jumpSimulation.Tick(new SimulationInputBatch());
}

PlayerSnapshot landedPlayer = jumpSimulation.CreateSnapshot().FindPlayer(jumpingPlayer);
if (landedPlayer.Position.Y != 0.0f || !landedPlayer.IsGrounded)
{
  throw new InvalidOperationException("Gravity must return a jumping player to the ground.");
}

Console.WriteLine("PASS: player jump and ground collision");

SimulationSnapshot beforeNpcTick = simulation.CreateSnapshot();
simulation.Tick(new SimulationInputBatch());
SimulationSnapshot afterNpcTick = simulation.CreateSnapshot();
NpcSnapshot beforeNpc = beforeNpcTick.FindNpc(npc);
NpcSnapshot afterNpc = afterNpcTick.FindNpc(npc);

if (afterNpc.Position.X >= beforeNpc.Position.X || !afterNpc.HasTarget)
{
  throw new InvalidOperationException("Npc must select and chase the active player.");
}

Console.WriteLine("PASS: npc targeting and chase");

DomeSimulation combatSimulation = new();
PlayerHandle combatPlayer = combatSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
NpcHandle combatNpc = combatSimulation.CreateNpc(new SimulationVector(16.0f, 0.0f));

combatSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(combatPlayer, MoveLeft: false, MoveRight: false, Jump: false, Fire: true)));
SimulationSnapshot afterFire = combatSimulation.CreateSnapshot();

if (afterFire.Projectiles.Count != 1)
{
  throw new InvalidOperationException("Fire input must create one projectile.");
}

combatSimulation.Tick(new SimulationInputBatch());

SimulationSnapshot afterHit = combatSimulation.CreateSnapshot();
NpcSnapshot hitNpc = afterHit.FindNpc(combatNpc);
if (hitNpc.Health != 90 || afterHit.Projectiles.Count != 0)
{
  throw new InvalidOperationException(
    "Projectile hit must resolve damage and despawn through the lifecycle commit. " +
    $"NpcX={hitNpc.Position.X}, Health={hitNpc.Health}, " +
    $"Projectiles={afterHit.Projectiles.Count}.");
}

Console.WriteLine("PASS: projectile damage and lifecycle");

using DomeServer server = new();
server.Start();
using DomeClient client = new();
await client.ConnectAsync(server.Port);
ServerSnapshot initialServerSnapshot = await client.SendAsync(new ClientInputFrame());
ServerSnapshot movedServerSnapshot = await client.SendAsync(new ClientInputFrame(MoveRight: true));

if (movedServerSnapshot.Player.X <= initialServerSnapshot.Player.X)
{
  throw new InvalidOperationException("TCP input must change the server-owned player snapshot.");
}

ServerSnapshot firedServerSnapshot = await client.SendAsync(new ClientInputFrame(Fire: true));
if (firedServerSnapshot.ProjectileCount != 1)
{
  throw new InvalidOperationException("TCP fire input must publish a projectile snapshot.");
}

Console.WriteLine("PASS: loopback client and server interaction");
