using System;
using System.IO;
using Terraria.Dome.Server.Startup;

if (NpcStreamSpeedPolicy.DefaultTicks != 30 ||
    NpcStreamSpeedPolicy.Normalize(0) != 30 ||
    NpcStreamSpeedPolicy.Normalize(-10) != 30 ||
    NpcStreamSpeedPolicy.Normalize(1) != 1 ||
    NpcStreamSpeedPolicy.Normalize(601) != 600)
{
  throw new InvalidOperationException("NPC stream speed policy contract diverged.");
}

ServerLaunchOptions options = new("world.wld", 7777);
if (options.NpcStreamSpeed != NpcStreamSpeedPolicy.DefaultTicks)
{
  throw new InvalidOperationException("Server launch default NPC stream speed diverged.");
}

string worldPath = Path.Combine(Path.GetTempPath(), "npc-stream-speed.wld");
ServerLaunchOptions parsed = ServerLaunchOptions.Parse(
  ["--world", worldPath, "--port", "7777", "--npc-stream-speed", "601"]);
if (parsed.NpcStreamSpeed != NpcStreamSpeedPolicy.MaximumTicks)
{
  throw new InvalidOperationException("Server launch NPC stream speed upper bound diverged.");
}

ServerLaunchOptions fallback = ServerLaunchOptions.Parse(
  ["--world", worldPath, "--port", "7777", "--npc-stream-speed", "0"]);
if (fallback.NpcStreamSpeed != NpcStreamSpeedPolicy.DefaultTicks)
{
  throw new InvalidOperationException("Server launch NPC stream speed fallback diverged.");
}

AssertThrows(() => ServerLaunchOptions.Parse(
  ["--world", worldPath, "--port", "7777", "--npc-stream-speed", "30", "--npc-stream-speed", "60"]));
AssertThrows(() => ServerLaunchOptions.Parse(
  ["--world", worldPath, "--port", "7777", "--npc-stream-speed", "invalid"]));

Console.WriteLine("PASS: NPC stream speed default=30 bounded=1..600");
Console.WriteLine("DEFERRED: NPC streaming consumer and legacy network cadence parity remain outside this slice");

static void AssertThrows(Action action)
{
  try
  {
    action();
  }
  catch (ArgumentException)
  {
    return;
  }

  throw new InvalidOperationException("Expected invalid NPC stream speed arguments to be rejected.");
}
