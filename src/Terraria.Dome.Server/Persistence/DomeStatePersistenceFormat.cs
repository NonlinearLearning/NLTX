using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Persistence;

public static class DomeStatePersistenceFormat
{
  private const int Magic = 0x44535444;
  private const int CurrentFormatVersion = 10;
  private const int FirstFormatVersion = 1;
  private const int ObjectStateFormatVersion = 3;
  private const int WorldStateFormatVersion = 4;
  private const int ItemInstanceStateFormatVersion = 5;
  private const int ItemWorldStateFormatVersion = 8;
  private const int WorldEventStateFormatVersion = 7;
  private const int RainStateFormatVersion = 9;
  private const int SlimeRainStateFormatVersion = 10;
  private const int MaximumNpcCount = 4096;
  private const int MaximumPlayerAccountCount = 1024;
  private const int MaximumPlayerNameLength = 20;
  private const int MaximumItemNameOverrideLength = 200;
  private const int MaximumWorldItemCount = 8192;
  private const int MaximumWorldPayloadBytes = 256 * 1024 * 1024;
  private const int MaximumChestCount = 8192;
  private const int MaximumSignCount = 32000;
  private const int MaximumTileEntityCount = 8192;
  private const int MaximumOpaqueCompatibilityRecordCount = 16384;
  private const int MaximumCompatibilityPayloadBytes = 4 * 1024 * 1024;
  private const int MaximumCompatibilityTextLength = 4096;

  public static DomeSimulationSnapshot Read(Stream input)
  {
    ArgumentNullException.ThrowIfNull(input);
    using BinaryReader reader = new(input, Encoding.UTF8, leaveOpen: true);
    if (reader.ReadInt32() != Magic)
    {
      throw new InvalidDataException("The Dome state format is not supported.");
    }

    int formatVersion = reader.ReadInt32();
    if (formatVersion < FirstFormatVersion || formatVersion > CurrentFormatVersion)
    {
      throw new InvalidDataException("The Dome state format is not supported.");
    }

    int worldPayloadLength = reader.ReadInt32();
    if (worldPayloadLength <= 0 || worldPayloadLength > MaximumWorldPayloadBytes)
    {
      throw new InvalidDataException("The embedded world payload length is invalid.");
    }

    byte[] worldPayload = reader.ReadBytes(worldPayloadLength);
    if (worldPayload.Length != worldPayloadLength)
    {
      throw new EndOfStreamException("The embedded world payload was truncated.");
    }

    using MemoryStream worldStream = new(worldPayload, writable: false);
    WorldGridSnapshot world = WorldPersistenceFormat.Read(worldStream);
    long tickNumber = reader.ReadInt64();
    if (tickNumber < 0)
    {
      throw new InvalidDataException("The simulation tick cannot be negative.");
    }

    NpcReplicationSnapshot[] npcs = ReadNpcs(reader);
    ItemReplicationSnapshot[] worldItems = ReadWorldItems(reader, formatVersion);
    PlayerPersistentState[] playerAccounts = formatVersion == FirstFormatVersion ? [] :
      ReadPlayerAccounts(reader, formatVersion);
    ChestPersistentState[] chests = formatVersion < ObjectStateFormatVersion ? [] :
      ReadChests(reader);
    SignPersistentState[] signs = formatVersion < ObjectStateFormatVersion ? [] : ReadSigns(reader);
    TileEntityPersistentState[] tileEntities = formatVersion < ObjectStateFormatVersion ? [] :
      ReadTileEntities(reader);
    OpaqueCompatibilityRecord[] opaqueRecords = formatVersion < ObjectStateFormatVersion ? [] :
      ReadOpaqueCompatibilityRecords(reader);
    WorldClockSnapshot clock = new(tickNumber, 0, true, false, 1);
    WorldRuleState worldRules = new();
    WorldProgressionState progression = new();
    if (formatVersion >= WorldStateFormatVersion)
    {
      clock = ReadWorldClock(reader, tickNumber);
      WorldMetadata metadata = ReadWorldMetadata(reader, world.Metadata);
      world = WorldGrid.FromSnapshot(world).CreateSnapshot(metadata);
      worldRules = ReadWorldRules(reader, formatVersion);
      progression = ReadWorldProgression(reader, formatVersion);
    }

    if (reader.BaseStream.ReadByte() != -1)
    {
      throw new InvalidDataException("The Dome state contains trailing data.");
    }

    return new DomeSimulationSnapshot(
      world,
      npcs,
      worldItems,
      tickNumber,
      playerAccounts,
      chests,
      signs,
      tileEntities,
      opaqueRecords,
      worldClock: clock,
      worldRules: worldRules,
      progression: progression);
  }

  public static void Write(Stream output, DomeSimulationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(output);
    ArgumentNullException.ThrowIfNull(snapshot);
    using MemoryStream worldStream = new();
    WorldPersistenceFormat.Write(worldStream, snapshot.World);
    byte[] worldPayload = worldStream.ToArray();

    using BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true);
    writer.Write(Magic);
    writer.Write(CurrentFormatVersion);
    writer.Write(worldPayload.Length);
    writer.Write(worldPayload);
    writer.Write(snapshot.TickNumber);
    WriteNpcs(writer, snapshot.Npcs);
    WriteWorldItems(writer, snapshot.WorldItems);
    WritePlayerAccounts(writer, snapshot.PlayerAccounts);
    WriteChests(writer, snapshot.Chests);
    WriteSigns(writer, snapshot.Signs);
    WriteTileEntities(writer, snapshot.TileEntities);
    WriteOpaqueCompatibilityRecords(writer, snapshot.OpaqueCompatibilityRecords);
    WriteWorldClock(writer, snapshot.Clock);
    WriteWorldMetadata(writer, snapshot.World.Metadata);
    WriteWorldRules(writer, snapshot.WorldRules);
    WriteWorldProgression(writer, snapshot.Progression);
  }

  private static WorldClockSnapshot ReadWorldClock(BinaryReader reader, long tickNumber)
  {
    try
    {
      WorldClock clock = new(
        reader.ReadInt64(),
        reader.ReadInt32(),
        reader.ReadBoolean(),
        reader.ReadBoolean(),
        reader.ReadInt32(),
        reader.ReadInt32(),
        reader.ReadInt32());
      if (clock.TickNumber != tickNumber)
      {
        throw new InvalidDataException("The persisted clock tick does not match the snapshot tick.");
      }

      return clock.CreateSnapshot();
    }
    catch (ArgumentException exception)
    {
      throw new InvalidDataException("The persisted world clock is invalid.", exception);
    }
  }

  private static WorldMetadata ReadWorldMetadata(
    BinaryReader reader,
    WorldMetadata persistedWorldMetadata)
  {
    try
    {
      return new WorldMetadata(
        persistedWorldMetadata.Name,
        persistedWorldMetadata.Seed,
        persistedWorldMetadata.Width,
        persistedWorldMetadata.Height,
        reader.ReadInt32(),
        reader.ReadInt32(),
        reader.ReadInt32(),
        ReadBoundedString(reader, MaximumCompatibilityTextLength, "world seed variant"),
        reader.ReadInt32());
    }
    catch (ArgumentException exception)
    {
      throw new InvalidDataException("The persisted world metadata is invalid.", exception);
    }
  }

  private static WorldProgressionState ReadWorldProgression(BinaryReader reader, int formatVersion)
  {
    try
    {
      bool isHardMode = reader.ReadBoolean();
      bool defeatedEyeOfCthulhu = reader.ReadBoolean();
      bool defeatedEaterOrBrain = reader.ReadBoolean();
      bool defeatedSkeletron = reader.ReadBoolean();
      bool defeatedWallOfFlesh = reader.ReadBoolean();
      bool defeatedMechanicalBoss = formatVersion >= WorldEventStateFormatVersion &&
        reader.ReadBoolean();
      bool defeatedPlantera = reader.ReadBoolean();
      bool defeatedGolem = reader.ReadBoolean();
      bool isBloodMoon = reader.ReadBoolean();
      bool isEclipse = reader.ReadBoolean();
      int invasionType = reader.ReadInt32();
      int invasionSize = reader.ReadInt32();
      int slimeRainTimeTicks = formatVersion >= SlimeRainStateFormatVersion
        ? reader.ReadInt32()
        : 0;
      return new WorldProgressionState(
        isHardMode,
        defeatedEyeOfCthulhu,
        defeatedEaterOrBrain,
        defeatedSkeletron,
        defeatedWallOfFlesh,
        defeatedMechanicalBoss,
        defeatedPlantera,
        defeatedGolem,
        isBloodMoon,
        isEclipse,
        invasionType,
        invasionSize,
        slimeRainTimeTicks);
    }
    catch (ArgumentException exception)
    {
      throw new InvalidDataException("The persisted world progression is invalid.", exception);
    }
  }

  private static WorldRuleState ReadWorldRules(BinaryReader reader, int formatVersion)
  {
    try
    {
      int difficulty = reader.ReadInt32();
      bool isExpertMode = reader.ReadBoolean();
      bool isMasterMode = reader.ReadBoolean();
      bool isCrimsonWorld = reader.ReadBoolean();
      int rainTimeTicks = formatVersion >= RainStateFormatVersion ? reader.ReadInt32() : 0;
      float rainStrength = formatVersion >= RainStateFormatVersion ? reader.ReadSingle() : 0.0f;
      return new WorldRuleState(
        difficulty,
        isExpertMode,
        isMasterMode,
        isCrimsonWorld,
        rainTimeTicks,
        rainStrength);
    }
    catch (ArgumentException exception)
    {
      throw new InvalidDataException("The persisted world rules are invalid.", exception);
    }
  }

  private static PlayerPersistentState[] ReadPlayerAccounts(BinaryReader reader, int formatVersion)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumPlayerAccountCount)
    {
      throw new InvalidDataException("The player account count is invalid.");
    }

    HashSet<string> seenUuids = new(StringComparer.Ordinal);
    PlayerPersistentState[] accounts = new PlayerPersistentState[count];
    for (int index = 0; index < accounts.Length; index++)
    {
      string uuid = reader.ReadString();
      PlayerPersistentProfile profile = ReadPlayerProfile(reader);
      int life = reader.ReadInt32();
      int maximumLife = reader.ReadInt32();
      int mana = reader.ReadInt32();
      int maximumMana = reader.ReadInt32();
      int buffCount = reader.ReadInt32();
      if (buffCount < 0 || buffCount > 44)
      {
        throw new InvalidDataException("The player buff count is invalid.");
      }

      PlayerPersistentBuff[] buffs = new PlayerPersistentBuff[buffCount];
      for (int buffIndex = 0; buffIndex < buffs.Length; buffIndex++)
      {
        buffs[buffIndex] = new PlayerPersistentBuff(reader.ReadUInt16());
      }

      byte selectedLoadout = reader.ReadByte();
      ushort accessoryVisibility = reader.ReadUInt16();
      PlayerPersistentItem[] items = new PlayerPersistentItem[PlayerPersistentState.ItemSlotCount];
      for (int slotId = 0; slotId < items.Length; slotId++)
      {
        int itemSlotId = reader.ReadInt32();
        int itemStack = reader.ReadInt32();
        byte itemPrefix = reader.ReadByte();
        int itemType = reader.ReadInt32();
        bool isFavorited = reader.ReadBoolean();
        bool isNewAndShiny = reader.ReadBoolean();
        ushort variantId = formatVersion >= ItemWorldStateFormatVersion
          ? reader.ReadUInt16()
          : (ushort)0;
        byte dye = formatVersion >= ItemWorldStateFormatVersion ? reader.ReadByte() : (byte)0;
        byte paint = formatVersion >= ItemWorldStateFormatVersion ? reader.ReadByte() : (byte)0;
        string? nameOverride = null;
        if (formatVersion >= ItemWorldStateFormatVersion && reader.ReadBoolean())
        {
          nameOverride = ReadBoundedString(
            reader,
            MaximumItemNameOverrideLength,
            "player item name override");
        }

        items[slotId] = new PlayerPersistentItem(
          itemSlotId,
          itemStack,
          itemPrefix,
          itemType,
          isFavorited,
          isNewAndShiny,
          variantId,
          dye,
          paint,
          nameOverride);
      }

      if (!seenUuids.Add(uuid))
      {
        throw new InvalidDataException("The player account UUID is duplicated.");
      }

      try
      {
        accounts[index] = new PlayerPersistentState(
          uuid,
          profile,
          life,
          maximumLife,
          mana,
          maximumMana,
          buffs,
          selectedLoadout,
          accessoryVisibility,
          items);
      }
      catch (ArgumentException exception)
      {
        throw new InvalidDataException("The player account data is invalid.", exception);
      }
    }

    return accounts;
  }

  private static PlayerPersistentColor ReadPlayerColor(BinaryReader reader)
  {
    return new PlayerPersistentColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
  }

  private static PlayerPersistentProfile ReadPlayerProfile(BinaryReader reader)
  {
    string name = reader.ReadString();
    if (name.Length > MaximumPlayerNameLength)
    {
      throw new InvalidDataException("The player profile name is too long.");
    }

    return new PlayerPersistentProfile(
      name,
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadSingle(),
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadUInt16(),
      reader.ReadByte(),
      ReadPlayerColor(reader),
      ReadPlayerColor(reader),
      ReadPlayerColor(reader),
      ReadPlayerColor(reader),
      ReadPlayerColor(reader),
      ReadPlayerColor(reader),
      ReadPlayerColor(reader),
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadByte());
  }

  private static void WritePlayerAccounts(
    BinaryWriter writer,
    IReadOnlyList<PlayerPersistentState> playerAccounts)
  {
    if (playerAccounts.Count > MaximumPlayerAccountCount)
    {
      throw new InvalidDataException("The player account count is too large.");
    }

    PlayerPersistentState[] orderedAccounts = playerAccounts
      .OrderBy(account => account.Uuid, StringComparer.Ordinal)
      .ToArray();
    writer.Write(orderedAccounts.Length);
    string? previousUuid = null;
    for (int accountIndex = 0; accountIndex < orderedAccounts.Length; accountIndex++)
    {
      PlayerPersistentState account = orderedAccounts[accountIndex];
      if (account.Profile.Name.Length > MaximumPlayerNameLength ||
          string.Equals(previousUuid, account.Uuid, StringComparison.Ordinal))
      {
        throw new InvalidDataException("The player account data is invalid.");
      }

      previousUuid = account.Uuid;
      writer.Write(account.Uuid);
      WritePlayerProfile(writer, account.Profile);
      writer.Write(account.Life);
      writer.Write(account.MaximumLife);
      writer.Write(account.Mana);
      writer.Write(account.MaximumMana);
      writer.Write(account.Buffs.Count);
      for (int buffIndex = 0; buffIndex < account.Buffs.Count; buffIndex++)
      {
        writer.Write(account.Buffs[buffIndex].Type);
      }

      writer.Write(account.SelectedLoadout);
      writer.Write(account.AccessoryVisibility);
      for (int slotId = 0; slotId < account.Items.Count; slotId++)
      {
        PlayerPersistentItem item = account.Items[slotId];
        writer.Write(item.SlotId);
        writer.Write(item.Stack);
        writer.Write(item.Prefix);
        writer.Write(item.ItemType);
        writer.Write(item.IsFavorited);
        writer.Write(item.IsNewAndShiny);
        writer.Write(item.VariantId);
        writer.Write(item.Dye);
        writer.Write(item.Paint);
        bool hasNameOverride = item.NameOverride is not null;
        if (hasNameOverride && item.NameOverride!.Length > MaximumItemNameOverrideLength)
        {
          throw new InvalidDataException("The player item name override is too long.");
        }

        writer.Write(hasNameOverride);
        if (hasNameOverride)
        {
          writer.Write(item.NameOverride!);
        }
      }
    }
  }

  private static void WritePlayerColor(BinaryWriter writer, PlayerPersistentColor color)
  {
    writer.Write(color.Red);
    writer.Write(color.Green);
    writer.Write(color.Blue);
  }

  private static void WritePlayerProfile(BinaryWriter writer, PlayerPersistentProfile profile)
  {
    writer.Write(profile.Name);
    writer.Write(profile.SkinVariant);
    writer.Write(profile.VoiceVariant);
    writer.Write(profile.VoicePitchOffset);
    writer.Write(profile.Hair);
    writer.Write(profile.HairDye);
    writer.Write(profile.AccessoryVisibility);
    writer.Write(profile.HideMisc);
    WritePlayerColor(writer, profile.HairColor);
    WritePlayerColor(writer, profile.SkinColor);
    WritePlayerColor(writer, profile.EyeColor);
    WritePlayerColor(writer, profile.ShirtColor);
    WritePlayerColor(writer, profile.UnderShirtColor);
    WritePlayerColor(writer, profile.PantsColor);
    WritePlayerColor(writer, profile.ShoeColor);
    writer.Write(profile.DifficultyFlags);
    writer.Write(profile.BiomeTorchFlags);
    writer.Write(profile.ConsumableFlags);
  }

  private static NpcReplicationSnapshot[] ReadNpcs(BinaryReader reader)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumNpcCount)
    {
      throw new InvalidDataException("The NPC snapshot count is invalid.");
    }

    NpcReplicationSnapshot[] npcs = new NpcReplicationSnapshot[count];
    for (int index = 0; index < count; index++)
    {
      npcs[index] = new NpcReplicationSnapshot(
        reader.ReadInt32(),
        reader.ReadInt32(),
        new SimulationVector(reader.ReadSingle(), reader.ReadSingle()),
        new SimulationVector(reader.ReadSingle(), reader.ReadSingle()),
        reader.ReadInt32(),
        reader.ReadBoolean(),
        reader.ReadInt64(),
        new WorldSectionCoordinates(reader.ReadInt32(), reader.ReadInt32()));
    }

    return npcs;
  }

  private static ItemReplicationSnapshot[] ReadWorldItems(BinaryReader reader, int formatVersion)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumWorldItemCount)
    {
      throw new InvalidDataException("The world-item snapshot count is invalid.");
    }

    ItemReplicationSnapshot[] items = new ItemReplicationSnapshot[count];
    for (int index = 0; index < count; index++)
    {
      int replicationId = reader.ReadInt32();
      ItemStack stack = new(reader.ReadUInt16(), reader.ReadInt32());
      SimulationVector position = new(reader.ReadSingle(), reader.ReadSingle());
      bool isActive = reader.ReadBoolean();
      long revision = reader.ReadInt64();
      WorldSectionCoordinates section = new(reader.ReadInt32(), reader.ReadInt32());
      ItemInstanceStateComponent instanceState = default;
      if (formatVersion >= ItemInstanceStateFormatVersion)
      {
        bool hasNameOverride = reader.ReadBoolean();
        string? nameOverride = hasNameOverride ? reader.ReadString() : null;
        if (nameOverride is not null && nameOverride.Length > MaximumItemNameOverrideLength)
        {
          throw new InvalidDataException("The item name override is too long.");
        }

        instanceState = new ItemInstanceStateComponent(
          reader.ReadUInt16(),
          reader.ReadUInt16(),
          reader.ReadByte(),
          reader.ReadByte(),
          reader.ReadBoolean(),
          nameOverride);
      }

      ItemWorldStateComponent worldState =
        ItemWorldStateComponent.FromReplicationSnapshot(isActive, revision);
      if (formatVersion >= ItemWorldStateFormatVersion)
      {
        worldState = new ItemWorldStateComponent(
          reader.ReadBoolean(),
          reader.ReadInt32(),
          reader.ReadInt32(),
          reader.ReadInt64(),
          reader.ReadInt64(),
          reader.ReadInt64());
        if (worldState.PickupDelayTicks < 0 || worldState.SpawnSource < 0 ||
            worldState.LastOwnerRevision < 0 || worldState.LastMergeTick < -1 ||
            worldState.Revision < 0 || worldState.IsActive != isActive ||
            worldState.Revision != revision)
        {
          throw new InvalidDataException("The world-item runtime state is invalid.");
        }
      }

      items[index] = new ItemReplicationSnapshot(
        replicationId,
        stack,
        position,
        isActive,
        revision,
        section,
        instanceState,
        worldState);
    }

    return items;
  }

  private static void WriteNpcs(
    BinaryWriter writer,
    IReadOnlyList<NpcReplicationSnapshot> npcs)
  {
    if (npcs.Count > MaximumNpcCount)
    {
      throw new InvalidDataException("The NPC snapshot count is too large.");
    }

    writer.Write(npcs.Count);
    for (int index = 0; index < npcs.Count; index++)
    {
      NpcReplicationSnapshot npc = npcs[index];
      writer.Write(npc.ReplicationId);
      writer.Write(npc.NpcType);
      writer.Write(npc.Position.X);
      writer.Write(npc.Position.Y);
      writer.Write(npc.Velocity.X);
      writer.Write(npc.Velocity.Y);
      writer.Write(npc.Health);
      writer.Write(npc.IsActive);
      writer.Write(npc.Revision);
      writer.Write(npc.Section.X);
      writer.Write(npc.Section.Y);
    }
  }

  private static void WriteWorldItems(
    BinaryWriter writer,
    IReadOnlyList<ItemReplicationSnapshot> worldItems)
  {
    if (worldItems.Count > MaximumWorldItemCount)
    {
      throw new InvalidDataException("The world-item snapshot count is too large.");
    }

    writer.Write(worldItems.Count);
    for (int index = 0; index < worldItems.Count; index++)
    {
      ItemReplicationSnapshot item = worldItems[index];
      writer.Write(item.ReplicationId);
      writer.Write(item.Stack.ItemType);
      writer.Write(item.Stack.Quantity);
      writer.Write(item.Position.X);
      writer.Write(item.Position.Y);
      writer.Write(item.IsActive);
      writer.Write(item.Revision);
      writer.Write(item.Section.X);
      writer.Write(item.Section.Y);
      ItemInstanceStateComponent instanceState = item.InstanceState;
      bool hasNameOverride = instanceState.NameOverride is not null;
      if (hasNameOverride && instanceState.NameOverride!.Length > MaximumItemNameOverrideLength)
      {
        throw new InvalidDataException("The item name override is too long.");
      }

      writer.Write(hasNameOverride);
      if (hasNameOverride)
      {
        writer.Write(instanceState.NameOverride!);
      }

      writer.Write(instanceState.PrefixId);
      writer.Write(instanceState.VariantId);
      writer.Write(instanceState.Dye);
      writer.Write(instanceState.Paint);
      writer.Write(instanceState.IsFavorited);

      ItemWorldStateComponent worldState = item.WorldState;
      if (worldState.Revision == 0 && item.Revision > 0)
      {
        worldState = ItemWorldStateComponent.FromReplicationSnapshot(item.IsActive, item.Revision);
      }

      if (worldState.PickupDelayTicks < 0 || worldState.SpawnSource < 0 ||
          worldState.LastOwnerRevision < 0 || worldState.LastMergeTick < -1 ||
          worldState.Revision < 0 || worldState.IsActive != item.IsActive ||
          worldState.Revision != item.Revision)
      {
        throw new InvalidDataException("The world-item runtime state is invalid.");
      }

      writer.Write(worldState.IsActive);
      writer.Write(worldState.PickupDelayTicks);
      writer.Write(worldState.SpawnSource);
      writer.Write(worldState.LastOwnerRevision);
      writer.Write(worldState.LastMergeTick);
      writer.Write(worldState.Revision);
    }
  }

  private static ChestPersistentState[] ReadChests(BinaryReader reader)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumChestCount)
    {
      throw new InvalidDataException("The chest snapshot count is invalid.");
    }

    ChestPersistentState[] chests = new ChestPersistentState[count];
    HashSet<int> ids = [];
    for (int index = 0; index < count; index++)
    {
      int chestId = reader.ReadInt32();
      int tileX = reader.ReadInt32();
      int tileY = reader.ReadInt32();
      string name = ReadBoundedString(reader, MaximumCompatibilityTextLength, "chest name");
      long revision = reader.ReadInt64();
      int slotCount = reader.ReadInt32();
      if (slotCount != ChestComponent.SlotCount)
      {
        throw new InvalidDataException("The chest slot count is invalid.");
      }

      ItemStack[] slots = new ItemStack[slotCount];
      for (int slot = 0; slot < slots.Length; slot++)
      {
        slots[slot] = new ItemStack(reader.ReadUInt16(), reader.ReadInt32());
      }

      if (!ids.Add(chestId))
      {
        throw new InvalidDataException("The chest ID is duplicated.");
      }

      try
      {
        chests[index] = new ChestPersistentState(chestId, tileX, tileY, slots, revision, name);
      }
      catch (ArgumentException exception)
      {
        throw new InvalidDataException("The chest snapshot is invalid.", exception);
      }
    }

    return chests;
  }

  private static SignPersistentState[] ReadSigns(BinaryReader reader)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumSignCount)
    {
      throw new InvalidDataException("The sign snapshot count is invalid.");
    }

    SignPersistentState[] signs = new SignPersistentState[count];
    HashSet<int> ids = [];
    for (int index = 0; index < signs.Length; index++)
    {
      int signId = reader.ReadInt32();
      SignPersistentState sign = new(
        signId,
        reader.ReadInt32(),
        reader.ReadInt32(),
        ReadBoundedString(reader, MaximumCompatibilityTextLength, "sign text"),
        reader.ReadInt64());
      if (!ids.Add(signId) || signId < 0 || sign.Revision < 0)
      {
        throw new InvalidDataException("The sign snapshot is invalid.");
      }

      signs[index] = sign;
    }

    return signs;
  }

  private static TileEntityPersistentState[] ReadTileEntities(BinaryReader reader)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumTileEntityCount)
    {
      throw new InvalidDataException("The tile-entity snapshot count is invalid.");
    }

    TileEntityPersistentState[] entities = new TileEntityPersistentState[count];
    HashSet<int> ids = [];
    for (int index = 0; index < entities.Length; index++)
    {
      int id = reader.ReadInt32();
      byte type = reader.ReadByte();
      int tileX = reader.ReadInt32();
      int tileY = reader.ReadInt32();
      bool isOpaque = reader.ReadBoolean();
      byte[] payload = ReadBoundedPayload(reader);
      if (!ids.Add(id))
      {
        throw new InvalidDataException("The tile-entity ID is duplicated.");
      }

      try
      {
        entities[index] = new TileEntityPersistentState(
          id,
          type,
          tileX,
          tileY,
          payload,
          isOpaque);
      }
      catch (ArgumentException exception)
      {
        throw new InvalidDataException("The tile-entity snapshot is invalid.", exception);
      }
    }

    return entities;
  }

  private static OpaqueCompatibilityRecord[] ReadOpaqueCompatibilityRecords(BinaryReader reader)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumOpaqueCompatibilityRecordCount)
    {
      throw new InvalidDataException("The opaque compatibility record count is invalid.");
    }

    OpaqueCompatibilityRecord[] records = new OpaqueCompatibilityRecord[count];
    for (int index = 0; index < records.Length; index++)
    {
      int sectionIndex = reader.ReadInt32();
      long offset = reader.ReadInt64();
      string reason = ReadBoundedString(reader, MaximumCompatibilityTextLength, "compatibility reason");
      int? type = reader.ReadBoolean() ? reader.ReadInt32() : null;
      int? tileX = reader.ReadBoolean() ? reader.ReadInt32() : null;
      int? tileY = reader.ReadBoolean() ? reader.ReadInt32() : null;
      records[index] = new OpaqueCompatibilityRecord(
        sectionIndex,
        offset,
        reason,
        type,
        tileX,
        tileY,
        ReadBoundedPayload(reader),
        reader.ReadBoolean());
    }

    return records;
  }

  private static byte[] ReadBoundedPayload(BinaryReader reader)
  {
    int length = reader.ReadInt32();
    if (length < 0 || length > MaximumCompatibilityPayloadBytes)
    {
      throw new InvalidDataException("The compatibility payload length is invalid.");
    }

    byte[] payload = reader.ReadBytes(length);
    if (payload.Length != length)
    {
      throw new EndOfStreamException("The compatibility payload was truncated.");
    }

    return payload;
  }

  private static string ReadBoundedString(BinaryReader reader, int maximumLength, string fieldName)
  {
    string value = reader.ReadString();
    if (value.Length > maximumLength)
    {
      throw new InvalidDataException($"The {fieldName} is too long.");
    }

    return value;
  }

  private static void WriteChests(
    BinaryWriter writer,
    IReadOnlyList<ChestPersistentState> chests)
  {
    if (chests.Count > MaximumChestCount)
    {
      throw new InvalidDataException("The chest snapshot count is too large.");
    }

    writer.Write(chests.Count);
    for (int index = 0; index < chests.Count; index++)
    {
      ChestPersistentState chest = chests[index];
      writer.Write(chest.ChestId);
      writer.Write(chest.TileX);
      writer.Write(chest.TileY);
      writer.Write(chest.Name);
      writer.Write(chest.Revision);
      writer.Write(chest.Slots.Count);
      for (int slot = 0; slot < chest.Slots.Count; slot++)
      {
        writer.Write(chest.Slots[slot].ItemType);
        writer.Write(chest.Slots[slot].Quantity);
      }
    }
  }

  private static void WriteSigns(
    BinaryWriter writer,
    IReadOnlyList<SignPersistentState> signs)
  {
    if (signs.Count > MaximumSignCount)
    {
      throw new InvalidDataException("The sign snapshot count is too large.");
    }

    writer.Write(signs.Count);
    for (int index = 0; index < signs.Count; index++)
    {
      SignPersistentState sign = signs[index];
      writer.Write(sign.SignId);
      writer.Write(sign.TileX);
      writer.Write(sign.TileY);
      writer.Write(sign.Text);
      writer.Write(sign.Revision);
    }
  }

  private static void WriteTileEntities(
    BinaryWriter writer,
    IReadOnlyList<TileEntityPersistentState> entities)
  {
    if (entities.Count > MaximumTileEntityCount)
    {
      throw new InvalidDataException("The tile-entity snapshot count is too large.");
    }

    writer.Write(entities.Count);
    for (int index = 0; index < entities.Count; index++)
    {
      TileEntityPersistentState entity = entities[index];
      writer.Write(entity.Id);
      writer.Write(entity.Type);
      writer.Write(entity.TileX);
      writer.Write(entity.TileY);
      writer.Write(entity.IsOpaque);
      WriteBoundedPayload(writer, entity.Payload);
    }
  }

  private static void WriteOpaqueCompatibilityRecords(
    BinaryWriter writer,
    IReadOnlyList<OpaqueCompatibilityRecord> records)
  {
    if (records.Count > MaximumOpaqueCompatibilityRecordCount)
    {
      throw new InvalidDataException("The opaque compatibility record count is too large.");
    }

    writer.Write(records.Count);
    for (int index = 0; index < records.Count; index++)
    {
      OpaqueCompatibilityRecord record = records[index];
      writer.Write(record.SectionIndex);
      writer.Write(record.Offset);
      writer.Write(record.Reason);
      writer.Write(record.Type.HasValue);
      if (record.Type.HasValue)
      {
        writer.Write(record.Type.Value);
      }

      writer.Write(record.TileX.HasValue);
      if (record.TileX.HasValue)
      {
        writer.Write(record.TileX.Value);
      }

      writer.Write(record.TileY.HasValue);
      if (record.TileY.HasValue)
      {
        writer.Write(record.TileY.Value);
      }

      WriteBoundedPayload(writer, record.Payload);
      writer.Write(record.IsRequired);
    }
  }

  private static void WriteWorldClock(BinaryWriter writer, WorldClockSnapshot clock)
  {
    writer.Write(clock.TickNumber);
    writer.Write(clock.TimeOfDay);
    writer.Write(clock.IsDayTime);
    writer.Write(clock.IsPaused);
    writer.Write(clock.TicksPerUpdate);
    writer.Write(clock.DayLengthTicks);
    writer.Write(clock.NightLengthTicks);
  }

  private static void WriteWorldMetadata(BinaryWriter writer, WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    writer.Write(metadata.WorldId);
    writer.Write(metadata.SpawnX);
    writer.Write(metadata.SpawnY);
    WriteBoundedString(writer, metadata.SeedVariant, "world seed variant");
    writer.Write(metadata.RandomStreamVersion);
  }

  private static void WriteWorldProgression(
    BinaryWriter writer,
    WorldProgressionState progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    writer.Write(progression.IsHardMode);
    writer.Write(progression.DefeatedEyeOfCthulhu);
    writer.Write(progression.DefeatedEaterOrBrain);
    writer.Write(progression.DefeatedSkeletron);
    writer.Write(progression.DefeatedWallOfFlesh);
    writer.Write(progression.DefeatedMechanicalBoss);
    writer.Write(progression.DefeatedPlantera);
    writer.Write(progression.DefeatedGolem);
    writer.Write(progression.IsBloodMoon);
    writer.Write(progression.IsEclipse);
    writer.Write(progression.InvasionType);
    writer.Write(progression.InvasionSize);
    writer.Write(progression.SlimeRainTimeTicks);
  }

  private static void WriteWorldRules(BinaryWriter writer, WorldRuleState worldRules)
  {
    ArgumentNullException.ThrowIfNull(worldRules);
    writer.Write(worldRules.Difficulty);
    writer.Write(worldRules.IsExpertMode);
    writer.Write(worldRules.IsMasterMode);
    writer.Write(worldRules.IsCrimsonWorld);
    writer.Write(worldRules.RainTimeTicks);
    writer.Write(worldRules.RainStrength);
  }

  private static void WriteBoundedPayload(BinaryWriter writer, IReadOnlyList<byte> payload)
  {
    if (payload.Count > MaximumCompatibilityPayloadBytes)
    {
      throw new InvalidDataException("The compatibility payload is too large.");
    }

    writer.Write(payload.Count);
    for (int index = 0; index < payload.Count; index++)
    {
      writer.Write(payload[index]);
    }
  }

  private static void WriteBoundedString(BinaryWriter writer, string value, string fieldName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);
    if (value.Length > MaximumCompatibilityTextLength)
    {
      throw new InvalidDataException($"The {fieldName} is too long.");
    }

    writer.Write(value);
  }
}
