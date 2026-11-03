using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Communication;
using Terraria.Item;
using Terraria.World;

// 世界掉落物的物品身份改由共享 ItemStack 承载（见审计 §32 与 §34）。
// 本验证固定三件事：
//   1. 权威状态只接受 ItemId 里的已知类型；未知类型在入口被拒，不会先存下再投影。
//   2. 身份、数量、前缀与协议 itemType 之间双向无损（换模型不改变线上字节）。
//   3. 清除槽位用空 ItemStack 表达，与原来的 Stack = 0 语义等价。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

SessionEpoch actor = SessionEpoch.Create(7);
Vector2 position = new(10f, 20f);
Vector2 velocity = new(1f, -1f);

WorldItemStore store = new();

// --- 1. 已知类型创建成功，且身份进入共享 ItemStack ---
bool created = store.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Torch),
    stack: 5,
    prefix: 3,
    position,
    velocity,
    reservedFor: null,
    out WorldItemState torch);
Check("create-known-applied", created, $"created={created}");

Check(
    "create-carries-item-stack",
    torch.Item.ItemId == ItemId.Torch && torch.Item.Quantity == 5 && torch.Item.Prefix == 3,
    $"id={torch.Item.ItemId} quantity={torch.Item.Quantity} prefix={torch.Item.Prefix}");

Check(
    "network-type-round-trip",
    ItemIdRules.ToNetworkType(torch.Item.ItemId) == 8,
    $"itemType={ItemIdRules.ToNetworkType(torch.Item.ItemId)}（应为 8）");

IReadOnlyList<WorldItemMutation> spawnMutations = store.DrainMutations();
Check(
    "spawn-mutation-kind",
    spawnMutations.Count == 1 && spawnMutations[0].Kind == WorldItemChangeKind.Spawn,
    $"count={spawnMutations.Count} kind={(spawnMutations.Count > 0 ? spawnMutations[0].Kind.ToString() : "none")}");

// --- 2. 未知 / 非法类型在入口被拒 ---
const int UnknownItemType = 6196;  // ItemId 值域上界是 6195
Check(
    "create-unknown-type-rejected",
    !store.TryCreate(UnknownItemType, 5, 0, position, velocity, null, out _),
    $"itemType={UnknownItemType}（不在 ItemId 中）");

Check(
    "create-zero-stack-rejected",
    !store.TryCreate(8, 0, 0, position, velocity, null, out _),
    "stack=0");

Check(
    "create-negative-type-rejected",
    !store.TryCreate(-48, 5, 0, position, velocity, null, out _),
    "itemType=-48（ItemId 里有该常量，但世界掉落物要求 > 0）");

Check(
    "rejections-queue-nothing",
    store.DrainMutations().Count == 0,
    "被拒绝的创建不应产生变更");

// --- 3. 更新路径的校验 ---
short slot = torch.Slot;
Check(
    "update-unknown-type-rejected",
    !store.TryUpdate(slot, actor, UnknownItemType, 4, 3, position, velocity, out _),
    $"itemType={UnknownItemType}");

Check(
    "update-prefix-mismatch-rejected",
    !store.TryUpdate(slot, actor, 8, 4, prefix: 9, position, velocity, out _),
    "prefix 9 与原状态 3 不一致");

Check(
    "update-larger-stack-rejected",
    !store.TryUpdate(slot, actor, 8, 9, 3, position, velocity, out _),
    "stack 9 大于原数量 5");

Check(
    "update-applied",
    store.TryUpdate(slot, actor, 8, 4, 3, position, velocity, out WorldItemState? updated) &&
        updated is not null && updated.Item.Quantity == 4,
    $"quantity={(updated is null ? -1 : updated.Item.Quantity)}");

// --- 4. 清除槽位等价于空 ItemStack ---
bool retired = store.TryUpdate(
    slot,
    actor,
    itemType: 0,
    stack: 0,
    prefix: 0,
    position,
    velocity,
    out WorldItemState? retiredState);
Check(
    "update-zero-stack-retires",
    retired && retiredState is not null && retiredState.Item.IsEmpty,
    $"retired={retired} isEmpty={(retiredState is null ? false : retiredState.Item.IsEmpty)}");

bool sawRetire = false;
foreach (WorldItemMutation mutation in store.DrainMutations())
{
  sawRetire |= mutation.Kind == WorldItemChangeKind.Retire && mutation.Item.Item.IsEmpty;
}
Check("retire-mutation-carries-empty-item", sawRetire, "Retire 变更携带空 ItemStack");

Check(
    "retired-slot-is-free",
    store.TryCreate(ItemIdRules.ToNetworkType(ItemId.Gel), 2, 0, position, velocity, null, out WorldItemState gel) &&
        gel.Slot == slot,
    $"重新占用槽位 {slot}");

// --- 5. 直接退役同样产生空 ItemStack ---
bool directRetire = store.TryRetire(gel.Slot, actor, out WorldItemState? gone);
Check(
    "direct-retire-yields-empty-item",
    directRetire && gone is not null && gone.Item.IsEmpty,
    $"retired={directRetire} isEmpty={(gone is null ? false : gone.Item.IsEmpty)}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: world item identity is carried by the shared ItemStack.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;
