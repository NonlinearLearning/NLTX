using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using ArchWorld = Arch.Core.World;
using Terraria.Communication;
using Terraria.Entity;
using Terraria.Item;
using Terraria.World;

// Tile 实体（箱子 / 告示牌 / 传送塔）验证：
//   1. world.tile-entities 记录流按 Version4 TileEntity.Read 的布局解码。
//   2. 不支持的 kind 与截断流都明确失败，不会产生错位数据。
//   3. 箱子所有权、告示牌文本边界与请求系统的结果。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

// --- 1. 记录流解码 ---
byte[] records = BuildRecords();
bool decoded = WorldTileEntityRecordDecoder.TryDecode(
    records,
    entityCount: 4,
    out WorldTileEntityState[] entities,
    out string failure);
Check("decode-succeeds", decoded, $"failure={failure} count={entities.Length}");
Check(
    "decode-item-frame-payload",
    decoded && entities[0].Kind == WorldTileEntityKind.ItemFrame &&
        entities[0].Position == new WorldTileCoordinate(10, 20) &&
        entities[0].Item.ItemId == ItemId.Torch &&
        entities[0].Item.Quantity == 5 &&
        entities[0].Item.Prefix == 3,
    $"kind={(decoded ? entities[0].Kind.ToString() : "-")} item={(decoded ? entities[0].Item.ToString() : "-")}");
Check(
    "decode-training-dummy-npc",
    decoded && entities[1].Kind == WorldTileEntityKind.TrainingDummy &&
        entities[1].NpcIndex == 7,
    $"npc={(decoded ? entities[1].NpcIndex : -1)}");
Check(
    "decode-logic-sensor",
    decoded && entities[2].Kind == WorldTileEntityKind.LogicSensor && entities[2].SensorOn,
    $"on={(decoded ? entities[2].SensorOn : false)}");
Check(
    "decode-teleportation-pylon",
    decoded && entities[3].Kind == WorldTileEntityKind.TeleportationPylon &&
        entities[3].TileWidth == 3 && entities[3].TileHeight == 4,
    $"size={(decoded ? entities[3].TileWidth : 0)}x{(decoded ? entities[3].TileHeight : 0)}");

byte[] unsupported = BuildRecord(WorldTileEntityKind.DisplayDoll, id: 1, x: 1, y: 1);
bool dollDecoded = WorldTileEntityRecordDecoder.TryDecode(
    unsupported,
    entityCount: 1,
    out _,
    out string dollFailure);
Check(
    "decode-rejects-unsupported-kind",
    !dollDecoded && dollFailure.Contains("not ported", StringComparison.Ordinal),
    dollFailure);

bool truncatedDecoded = WorldTileEntityRecordDecoder.TryDecode(
    records.AsMemory(0, records.Length - 3),
    entityCount: 4,
    out _,
    out string truncatedFailure);
Check(
    "decode-rejects-truncated-stream",
    !truncatedDecoded && truncatedFailure.Contains("truncated", StringComparison.Ordinal),
    truncatedFailure);

// --- 2. 表与所有权 ---
WorldTileEntityStore store = new();
WorldChestState chest = new(0, new WorldTileCoordinate(100, 200), "Loot");
chest.TrySetItem(0, new ItemStack(ItemId.Gel, 12));
chest.TrySetItem(3, new ItemStack(ItemId.Torch, 5, 2));
WorldSignState sign = new(0, new WorldTileCoordinate(110, 200), "hello\r\nworld");
store.Load([chest], [sign], entities);

Check(
    "store-counts",
    store.ChestCount == 1 && store.SignCount == 1 && store.TileEntityCount == entities.Length,
    $"chests={store.ChestCount} signs={store.SignCount} entities={store.TileEntityCount}");
Check(
    "store-finds-chest-by-position",
    store.TryGetChest(new WorldTileCoordinate(100, 200), out WorldChestState? found) &&
        found is not null && found.StoredStackCount == 2,
    $"stacks={(found is null ? -1 : found.StoredStackCount)}");
Check(
    "store-finds-pylons",
    store.CountTileEntities(WorldTileEntityKind.TeleportationPylon) == 1,
    $"pylons={store.CountTileEntities(WorldTileEntityKind.TeleportationPylon)}");
Check(
    "sign-text-normalized",
    sign.Text == "hello\nworld",
    $"text={sign.Text.Replace("\n", "\\n")}");

SessionEpoch owner = SessionEpoch.Create(1);
SessionEpoch other = SessionEpoch.Create(2);
Check(
    "chest-open-first-wins",
    chest.TryOpen(owner) && chest.IsOpen && chest.OpenedBy == owner,
    $"openedBy={chest.OpenedBy}");
Check(
    "chest-open-second-rejected",
    !chest.TryOpen(other) && chest.OpenedBy == owner,
    "第二个会话不能抢占已打开的箱子");
Check(
    "chest-close-requires-owner",
    !chest.TryClose(other) && chest.TryClose(owner) && !chest.IsOpen,
    "只有持有者能关闭");

Check(
    "sign-edit-applied",
    sign.TryEdit("a\rb") && sign.Text == "a\nb",
    $"text={sign.Text.Replace("\n", "\\n")}");
Check(
    "sign-edit-bounded",
    !sign.TryEdit(new string('x', WorldSignState.MaximumTextLength + 1)) &&
        sign.Text == "a\nb",
    $"上限 {WorldSignState.MaximumTextLength} 字符");

// --- 3. 请求系统 ---
using ArchWorld ecsWorld = ArchWorld.Create();
_ = ecsWorld.Create(new WorldTileEntityRequestQueueComponent());
_ = ecsWorld.Create(new WorldTileEntityResultQueueComponent());
WorldTileEntityRequestSystem system = new(ecsWorld, store);
WorldTileEntityRequestQueueComponent requests = WorldTileEntityRequestQueue.Require(ecsWorld);

requests.Enqueue(new WorldTileEntityRequest(
    WorldTileEntityRequestKind.OpenChest,
    new WorldTileCoordinate(100, 200),
    owner));
requests.Enqueue(new WorldTileEntityRequest(
    WorldTileEntityRequestKind.OpenChest,
    new WorldTileCoordinate(100, 200),
    other));
requests.Enqueue(new WorldTileEntityRequest(
    WorldTileEntityRequestKind.CloseChest,
    new WorldTileCoordinate(100, 200),
    owner));
requests.Enqueue(new WorldTileEntityRequest(
    WorldTileEntityRequestKind.EditSign,
    new WorldTileCoordinate(110, 200),
    owner,
    "edited"));
requests.Enqueue(new WorldTileEntityRequest(
    WorldTileEntityRequestKind.OpenChest,
    new WorldTileCoordinate(999, 999),
    owner));
system.Update(new EcsServerTickContext(ecsWorld, 1, 1f / 60f));

IReadOnlyList<WorldTileEntityResult> results = WorldTileEntityResultQueue.Require(ecsWorld).Drain();
Check(
    "request-system-emits-one-result-per-request",
    results.Count == 5,
    $"results={results.Count}");
Check(
    "request-system-applies-open",
    results.Count > 0 && results[0].Outcome == WorldTileEntityResultKind.Applied,
    $"outcome={(results.Count > 0 ? results[0].Outcome.ToString() : "-")}");
Check(
    "request-system-rejects-second-open",
    results.Count > 1 && results[1].Outcome == WorldTileEntityResultKind.NotOwner,
    $"outcome={(results.Count > 1 ? results[1].Outcome.ToString() : "-")}");
Check(
    "request-system-applies-close",
    results.Count > 2 && results[2].Outcome == WorldTileEntityResultKind.Applied && !chest.IsOpen,
    $"isOpen={chest.IsOpen}");
Check(
    "request-system-applies-sign-edit",
    results.Count > 3 && results[3].Outcome == WorldTileEntityResultKind.Applied &&
        sign.Text == "edited",
    $"text={sign.Text}");
Check(
    "request-system-reports-missing-chest",
    results.Count > 4 && results[4].Outcome == WorldTileEntityResultKind.NotFound,
    $"outcome={(results.Count > 4 ? results[4].Outcome.ToString() : "-")}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: chest, sign and tile entity state decode and respond like the reference.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;

static byte[] BuildRecords()
{
  using MemoryStream stream = new();
  WriteRecord(stream, WorldTileEntityKind.ItemFrame, id: 0, x: 10, y: 20, writer =>
  {
    writer.Write((short)ItemIdRules.ToNetworkType(ItemId.Torch));
    writer.Write((byte)3);
    writer.Write((short)5);
  });
  WriteRecord(stream, WorldTileEntityKind.TrainingDummy, id: 1, x: 30, y: 40, writer =>
      writer.Write((short)7));
  WriteRecord(stream, WorldTileEntityKind.LogicSensor, id: 2, x: 50, y: 60, writer =>
  {
    writer.Write((byte)0);
    writer.Write(true);
  });
  WriteRecord(stream, WorldTileEntityKind.TeleportationPylon, id: 3, x: 70, y: 80, _ => { });
  return stream.ToArray();
}

static byte[] BuildRecord(WorldTileEntityKind kind, int id, short x, short y)
{
  using MemoryStream stream = new();
  WriteRecord(stream, kind, id, x, y, _ => { });
  return stream.ToArray();
}

static void WriteRecord(
    MemoryStream stream,
    WorldTileEntityKind kind,
    int id,
    short x,
    short y,
    Action<BinaryWriter> writeExtraData)
{
  using BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
  writer.Write((byte)kind);
  writer.Write(id);
  writer.Write(x);
  writer.Write(y);
  writeExtraData(writer);
  writer.Flush();
}
