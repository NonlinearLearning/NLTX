using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation.WorldModel;

WorldGrid world = new(width: 400, height: 300);
WorldSectionCoordinates first = new(0, 0);
WorldSectionCoordinates second = new(1, 0);
SessionSectionVisibility visibility = new();

WorldSectionSnapshot firstSnapshot = world.CreateSectionSnapshot(first);
WorldSectionSnapshot secondSnapshot = world.CreateSectionSnapshot(second);
IReadOnlyList<WorldSectionSnapshot> initial = visibility.CollectChangedSections(
  [firstSnapshot, secondSnapshot]);
if (initial.Count != 2 || initial[0].Coordinates != first || initial[1].Coordinates != second)
{
  throw new InvalidOperationException("A new session did not receive each subscribed section.");
}

if (visibility.CollectChangedSections([firstSnapshot, secondSnapshot]).Count != 0)
{
  throw new InvalidOperationException("A session received an unchanged section twice.");
}

_ = world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
WorldSectionSnapshot changedFirstSnapshot = world.CreateSectionSnapshot(first);
IReadOnlyList<WorldSectionSnapshot> dirty = visibility.CollectChangedSections(
  [changedFirstSnapshot, secondSnapshot]);
if (dirty.Count != 1 || dirty[0].Coordinates != first || dirty[0].Version != 1)
{
  throw new InvalidOperationException("A session did not receive only the dirty section version.");
}

visibility.Clear();
if (visibility.CollectChangedSections([changedFirstSnapshot, secondSnapshot]).Count != 2)
{
  throw new InvalidOperationException("Clearing a disconnected session did not release section versions.");
}

Console.WriteLine("PASS: server-owned section visibility versions");

WorldGrid streamedWorld = new(width: 4200, height: 1200);
_ = streamedWorld.TrySetTile(2100, 300, new WorldTile(IsActive: true, Type: 1));
WorldSectionReplication replication = new(streamedWorld);
SessionSectionVisibility initialVisibility = new();
IReadOnlyList<byte[]> initialStream = replication.CreateInitialWorldStream(
  new SpawnTileDataRequestPacket(2100, 300, 0),
  initialVisibility);
if (initialStream.Count != 17 ||
    TerrariaFrameCodec.Decode(initialStream[0]).MessageId != TerrariaMessageId.StatusTextSize ||
    TerrariaFrameCodec.Decode(initialStream[16]).MessageId != TerrariaMessageId.InitialSpawn)
{
  throw new InvalidOperationException("Server world replication did not produce the initial V1456 stream.");
}

TerrariaFrame centerSectionFrame = TerrariaFrameCodec.Decode(initialStream[8]);
using MemoryStream compressedStream = new(centerSectionFrame.Payload.ToArray(), writable: false);
using DeflateStream decompressor = new(compressedStream, CompressionMode.Decompress);
using BinaryReader reader = new(decompressor);
int originX = reader.ReadInt32();
int originY = reader.ReadInt32();
if (originX != 2000 || originY != 300)
{
  throw new InvalidOperationException("Server world replication selected the wrong spawn-center section.");
}

Console.WriteLine("PASS: server world section selection and initial replication");
