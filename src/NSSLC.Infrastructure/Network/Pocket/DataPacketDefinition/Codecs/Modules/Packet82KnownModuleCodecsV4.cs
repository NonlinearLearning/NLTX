using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class Packet82KnownModuleCodecsV4
{
    private const int MaximumBannerTypes = 293;

    public static IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> Create() => Create(
    [
        new ModuleCodec<Packet82LiquidData>(Packet82ModuleId.Liquid, WriteLiquid, ReadLiquid),
        new ModuleCodec<Packet82TextData>(Packet82ModuleId.Text, WriteText, ReadText),
        new ModuleCodec<Packet82PingData>(Packet82ModuleId.Ping, WritePing, ReadPing),
        new ModuleCodec<Packet82AmbienceData>(Packet82ModuleId.Ambience, WriteAmbience, ReadAmbience),
        new ModuleCodec<Packet82BestiaryData>(Packet82ModuleId.Bestiary, WriteBestiary, ReadBestiary),
        new ModuleCodec<Packet82CreativePowerData>(Packet82ModuleId.CreativePowers, WriteCreativePower, ReadCreativePower),
        new ModuleCodec<Packet82EmptyModuleData>(Packet82ModuleId.CreativeUnlocksPlayerReport, WriteEmpty, ReadEmpty),
        new ModuleCodec<Packet82TeleportPylonData>(Packet82ModuleId.TeleportPylon, WriteTeleportPylon, ReadTeleportPylon),
        new ModuleCodec<Packet82ParticlesData>(Packet82ModuleId.Particles, WriteParticles, ReadParticles),
        new ModuleCodec<Packet82PermissionData>(Packet82ModuleId.CreativePowerPermissions, WritePermission, ReadPermission),
        new ModuleCodec<Packet82BannerData>(Packet82ModuleId.Banners, WriteBanners, ReadBanners),
        new ModuleCodec<Packet82EmptyModuleData>(Packet82ModuleId.CraftingRequests, WriteEmpty, ReadEmpty),
        new ModuleCodec<Packet82TagEffectData>(Packet82ModuleId.TagEffectState, WriteTagEffect, ReadTagEffect),
        new ModuleCodec<Packet82LeashedData>(Packet82ModuleId.LeashedEntity, WriteLeashed, ReadLeashed),
        new ModuleCodec<Packet82EmptyModuleData>(Packet82ModuleId.UnbreakableWallScan, WriteEmpty, ReadEmpty)
    ]);

    public static IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> Create(
        IEnumerable<IPacket82ModuleWireCodec> codecs)
    {
        ArgumentNullException.ThrowIfNull(codecs);
        var map = new Dictionary<ushort, IPacket82ModuleWireCodec>();
        foreach (IPacket82ModuleWireCodec codec in codecs)
        {
            ArgumentNullException.ThrowIfNull(codec);
            if (!map.TryAdd(codec.ModuleId, codec))
                throw new ArgumentException($"Duplicate Packet 82 module id {codec.ModuleId}.", nameof(codecs));
        }
        if (map.Count != Enum.GetValues<Packet82ModuleId>().Length
            || Enum.GetValues<Packet82ModuleId>().Any(id => !map.ContainsKey((ushort)id)))
            throw new ArgumentException(
                "Packet 82 requires codecs for all fifteen modules in NetworkInitializer registration order.",
                nameof(codecs));
        return new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, IPacket82ModuleWireCodec>(map);
    }

    private static void WriteEmpty(PacketWireWriter writer, Packet82EmptyModuleData payload) { }

    public static IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> CreateSteam()
    {
        var codecs = new Dictionary<ushort, IPacket82ModuleWireCodec>();
        foreach (IPacket82ModuleWireCodec codec in Create().Values)
        {
            if (codec.ModuleId == (ushort)Packet82ModuleId.TagEffectState) continue;
            ushort wireId = codec.ModuleId is >= 5 and <= 11
                ? (ushort)(codec.ModuleId + 1) : codec.ModuleId;
            codecs.Add(wireId, new RemappedModuleCodec(wireId, codec));
        }
        codecs.Add(5, new ModuleCodec<Packet82CreativeUnlockData>((Packet82ModuleId)5,
            static (writer, data) => {
                writer.WriteInt16(data.ItemId);
                writer.WriteUInt16(data.SacrificeCount);
            }, static reader => new(reader.ReadInt16(), reader.ReadUInt16())));
        return Create(codecs.Values);
    }

    public static ushort ResolveLegacySteamModuleId(ushort wireId)
    {
        return wireId == 5 ? ushort.MaxValue
            : wireId is >= 6 and <= 12 ? (ushort)(wireId - 1) : wireId;
    }

    private static Packet82EmptyModuleData ReadEmpty(PacketWireReader reader) =>
        new Packet82EmptyModuleData();

    private static void WritePing(PacketWireWriter writer, Packet82PingData payload) {
      writer.WriteSingle(payload.Position.X);
      writer.WriteSingle(payload.Position.Y);
    }

    private static Packet82PingData ReadPing(PacketWireReader reader) {
      return new Packet82PingData(new PacketVector2(reader.ReadSingle(), reader.ReadSingle()));
    }

    private static void WriteTeleportPylon(PacketWireWriter writer, Packet82TeleportPylonData payload)
    {
        if (payload.Action > 2)
            throw new PacketWireFormatException($"Unknown Version4 teleport-pylon action {payload.Action}.");
        writer.WriteByte(payload.Action);
        writer.WriteInt16(payload.X);
        writer.WriteInt16(payload.Y);
        writer.WriteByte(payload.PylonType);
    }

    private static Packet82TeleportPylonData ReadTeleportPylon(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        if (action > 2)
            throw new PacketWireFormatException($"Unknown Version4 teleport-pylon action {action}.");
        var payload = new Packet82TeleportPylonData(action, reader.ReadInt16(), reader.ReadInt16(), reader.ReadByte());
        return payload;
    }

    private static void WriteTagEffect(
        PacketWireWriter writer,
        Packet82TagEffectData payload,
        int tagEffectNpcSlotCount,
        Func<short, bool> tagEffectUsesProcTimes)
    {
        bool hasProcTimes = false;
        switch (payload.Action)
        {
            case 0:
                if (payload.NpcTimes is null)
                    throw new PacketWireFormatException("TagEffect full-state messages require the NPC timer table.");
                RequireNpcSlotCount(payload.NpcTimes, tagEffectNpcSlotCount);
                hasProcTimes = tagEffectUsesProcTimes(payload.EffectType);
                if (hasProcTimes)
                {
                    if (payload.ProcTimes is null)
                        throw new PacketWireFormatException("This TagEffect full-state message requires the proc timer table.");
                    RequireNpcSlotCount(payload.ProcTimes, tagEffectNpcSlotCount);
                }
                else if (payload.ProcTimes is not null)
                {
                    throw new PacketWireFormatException("This TagEffect full-state message has no proc timer table.");
                }
                break;
            case 1:
                if (payload.NpcTimes is not null || payload.ProcTimes is not null)
                    throw new PacketWireFormatException("TagEffect active-effect changes carry no timer tables.");
                break;
            default:
                throw new PacketWireFormatException($"Unknown supported TagEffect action {payload.Action}.");
        }

        writer.WriteByte(payload.Player);
        writer.WriteByte(payload.Action);
        writer.WriteInt16(payload.EffectType);
        if (payload.Action == 0)
        {
            WriteSparseNpcTimes(writer, payload.NpcTimes!, tagEffectNpcSlotCount);
            if (hasProcTimes)
                WriteSparseNpcTimes(writer, payload.ProcTimes!, tagEffectNpcSlotCount);
        }
    }

    private static Packet82TagEffectData ReadTagEffect(
        PacketWireReader reader,
        int tagEffectNpcSlotCount,
        Func<short, bool> tagEffectUsesProcTimes)
    {
        byte player = reader.ReadByte();
        byte action = reader.ReadByte();
        short effectType = reader.ReadInt16();
        int[]? npcTimes = null;
        int[]? procTimes = null;
        switch (action)
        {
            case 0:
                npcTimes = ReadSparseNpcTimes(reader, tagEffectNpcSlotCount);
                if (tagEffectUsesProcTimes(effectType))
                    procTimes = ReadSparseNpcTimes(reader, tagEffectNpcSlotCount);
                break;
            case 1:
                break;
            default:
                throw new PacketWireFormatException($"Unknown supported TagEffect action {action}.");
        }
        return new Packet82TagEffectData(player, action, effectType, npcTimes, procTimes);
    }

    private static void WriteSparseNpcTimes(PacketWireWriter writer, int[] times, int slotCount)
    {
        RequireNpcSlotCount(times, slotCount);
        for (int index = 0; index < times.Length; index++)
        {
            if (times[index] == 0) continue;
            writer.WriteByte((byte)index);
            writer.WriteInt32(times[index]);
        }
        writer.WriteByte((byte)times.Length);
    }

    private static void RequireNpcSlotCount(int[] times, int slotCount)
    {
        if (times.Length != slotCount)
            throw new PacketWireFormatException($"TagEffect timer table must contain exactly {slotCount} NPC slots.");
    }

    private static int[] ReadSparseNpcTimes(PacketWireReader reader, int slotCount)
    {
        var times = new int[slotCount];
        int previousIndex = -1;
        while (true)
        {
            byte index = reader.ReadByte();
            if (index == slotCount) return times;
            if (index >= slotCount || index <= previousIndex)
                throw new PacketWireFormatException("TagEffect sparse timer indexes must be strictly increasing and within the NPC table.");
            int value = reader.ReadInt32();
            if (value == 0)
                throw new PacketWireFormatException("TagEffect sparse timer entries must be nonzero.");
            times[index] = value;
            previousIndex = index;
        }
    }

    private static void WriteLiquid(PacketWireWriter writer, Packet82LiquidData payload)
    {
        ArgumentNullException.ThrowIfNull(payload.Entries);
        if (payload.Entries.Count > ushort.MaxValue)
            throw new PacketWireFormatException("Packet 82 liquid entry count exceeds UInt16.");

        writer.WriteUInt16((ushort)payload.Entries.Count);
        foreach (Packet82LiquidEntry entry in payload.Entries)
        {
            writer.WriteInt32(entry.PackedTileCoordinate);
            writer.WriteByte(entry.Amount);
            writer.WriteByte(entry.LiquidType);
        }
    }

    private static Packet82LiquidData ReadLiquid(PacketWireReader reader)
    {
        int count = reader.ReadUInt16();
        var entries = new Packet82LiquidEntry[count];
        for (int index = 0; index < count; index++)
            entries[index] = new Packet82LiquidEntry(reader.ReadInt32(), reader.ReadByte(), reader.ReadByte());
        return new Packet82LiquidData(entries);
    }

    private static void WriteText(PacketWireWriter writer, Packet82TextData payload)
    {
        ArgumentNullException.ThrowIfNull(payload.Text);
        writer.WriteByte(payload.Author);
        NetworkTextCodec.Write(writer, payload.Text);
        writer.WriteByte(payload.Red);
        writer.WriteByte(payload.Green);
        writer.WriteByte(payload.Blue);
    }

    private static Packet82TextData ReadText(PacketWireReader reader)
    {
        byte author = reader.ReadByte();
        NetworkText text = NetworkTextCodec.Read(reader);
        byte red = reader.ReadByte();
        byte green = reader.ReadByte();
        byte blue = reader.ReadByte();
        return new Packet82TextData(author, text, red, green, blue);
    }

    private static void WriteAmbience(PacketWireWriter writer, Packet82AmbienceData payload)
    {
        writer.WriteByte(payload.Player);
        writer.WriteInt32(payload.Seed);
        writer.WriteByte(payload.SkyEntityType);
    }

    private static Packet82AmbienceData ReadAmbience(PacketWireReader reader)
    {
        var value = new Packet82AmbienceData(reader.ReadByte(), reader.ReadInt32(), reader.ReadByte());
        return value;
    }

    private static void WriteBestiary(PacketWireWriter writer, Packet82BestiaryData payload)
    {
        if (payload.Action > 2)
            throw new PacketWireFormatException($"Unknown Packet 82 bestiary action {payload.Action}.");
        if ((payload.Action == 0) != (payload.KillCount is not null))
            throw new PacketWireFormatException("Only bestiary kill-count messages carry a count.");

        writer.WriteByte(payload.Action);
        writer.WriteInt16(payload.NpcNetId);
        if (payload.KillCount is int count) Write7BitEncodedInt(writer, count);
    }

    private static Packet82BestiaryData ReadBestiary(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        if (action > 2)
            throw new PacketWireFormatException($"Unknown Packet 82 bestiary action {action}.");
        short npcNetId = reader.ReadInt16();
        int? killCount = action == 0 ? Read7BitEncodedInt(reader) : null;
        return new Packet82BestiaryData(action, npcNetId, killCount);
    }

    private static void WriteCreativePower(PacketWireWriter writer, Packet82CreativePowerData payload)
    {
        if (payload.PowerId > 14)
            throw new PacketWireFormatException($"Unknown Version4 creative power id {payload.PowerId}.");

        writer.WriteUInt16(payload.PowerId);
        switch (payload.PowerId)
        {
            case 0:
            case 9:
            case 10:
            case 13:
                writer.WriteBoolean(Require<Packet82SharedTogglePowerState>(payload.Value).Enabled);
                break;
            case 1:
            case 2:
            case 3:
            case 4:
                if (payload.Value is not null)
                    throw new PacketWireFormatException("Button powers have no payload bytes.");
                break;
            case 5:
            case 11:
                WritePerPlayerToggles(writer, Require<Packet82PerPlayerTogglePowerState>(payload.Value));
                break;
            case 6:
            case 7:
                if (payload.Value is not null)
                    throw new PacketWireFormatException("These Version4 slider powers have no registered network payload.");
                break;
            case 8:
            case 12:
                writer.WriteSingle(Require<Packet82SharedSliderPowerState>(payload.Value).Value);
                break;
            case 14:
                Packet82PerPlayerSliderPowerState perPlayer = Require<Packet82PerPlayerSliderPowerState>(payload.Value);
                writer.WriteByte(perPlayer.Player);
                writer.WriteSingle(perPlayer.Value);
                break;
            default:
                throw new PacketWireFormatException($"Unknown Version4 creative power id {payload.PowerId}.");
        }
    }

    private static Packet82CreativePowerData ReadCreativePower(PacketWireReader reader)
    {
        ushort powerId = reader.ReadUInt16();
        object? value = powerId switch
        {
            0 or 9 or 10 or 13 => new Packet82SharedTogglePowerState(reader.ReadBoolean()),
            1 or 2 or 3 or 4 => null,
            5 or 11 => ReadPerPlayerToggles(reader),
            6 or 7 => null,
            8 or 12 => new Packet82SharedSliderPowerState(reader.ReadSingle()),
            14 => new Packet82PerPlayerSliderPowerState(reader.ReadByte(), reader.ReadSingle()),
            _ => throw new PacketWireFormatException($"Unknown Version4 creative power id {powerId}.")
        };
        return new Packet82CreativePowerData(powerId, value);
    }

    private static void WritePerPlayerToggles(PacketWireWriter writer, Packet82PerPlayerTogglePowerState state)
    {
        if (state.EnabledPlayers is null || state.EnabledPlayers.Length != 255)
            throw new PacketWireFormatException("Per-player creative toggles require exactly 255 player states.");
        writer.WriteByte(0);
        for (int byteIndex = 0; byteIndex < 32; byteIndex++)
        {
            byte bits = 0;
            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                int playerIndex = byteIndex * 8 + bitIndex;
                if (playerIndex < state.EnabledPlayers.Length && state.EnabledPlayers[playerIndex])
                    bits |= (byte)(1 << bitIndex);
            }
            writer.WriteByte(bits);
        }
    }

    private static Packet82PerPlayerTogglePowerState ReadPerPlayerToggles(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        if (action != 0)
            throw new PacketWireFormatException($"Unknown per-player creative toggle action {action}.");
        var enabled = new bool[255];
        for (int byteIndex = 0; byteIndex < 32; byteIndex++)
        {
            byte bits = reader.ReadByte();
            int firstPlayer = byteIndex * 8;
            int validBits = Math.Min(8, enabled.Length - firstPlayer);
            if (validBits < 8 && (bits & ~((1 << validBits) - 1)) != 0)
                throw new PacketWireFormatException("Per-player creative toggle has bits outside the 255-player table.");
            for (int bitIndex = 0; bitIndex < validBits; bitIndex++)
                enabled[firstPlayer + bitIndex] = (bits & (1 << bitIndex)) != 0;
        }
        return new Packet82PerPlayerTogglePowerState(enabled);
    }

    private static void WriteParticles(PacketWireWriter writer, Packet82ParticlesData payload)
    {
        ArgumentNullException.ThrowIfNull(payload.Settings);
        writer.WriteByte(payload.ParticleType);
        writer.WriteSingle(payload.Settings.PositionX);
        writer.WriteSingle(payload.Settings.PositionY);
        writer.WriteSingle(payload.Settings.MovementX);
        writer.WriteSingle(payload.Settings.MovementY);
        writer.WriteInt32(payload.Settings.UniqueInfoPiece);
        writer.WriteByte(payload.Settings.InvokingPlayer);
    }

    private static Packet82ParticlesData ReadParticles(PacketWireReader reader)
    {
        byte type = reader.ReadByte();
        var settings = new Packet82ParticleSettings(
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadInt32(), reader.ReadByte());
        return new Packet82ParticlesData(type, settings);
    }

    private static void WritePermission(PacketWireWriter writer, Packet82PermissionData payload)
    {
        if (payload.Action != 0)
            throw new PacketWireFormatException($"Unknown Packet 82 permission action {payload.Action}.");
        writer.WriteByte(payload.Action);
        writer.WriteUInt16(payload.PowerId);
        writer.WriteByte(payload.Level);
    }

    private static Packet82PermissionData ReadPermission(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        if (action != 0)
            throw new PacketWireFormatException($"Unknown Packet 82 permission action {action}.");
        ushort powerId = reader.ReadUInt16();
        byte level = reader.ReadByte();
        return new Packet82PermissionData(action, powerId, level);
    }

    private static void WriteBanners(PacketWireWriter writer, Packet82BannerData payload)
    {
        if (payload.Action == 0
            && (payload.KillCounts?.Count != MaximumBannerTypes
                || payload.ClaimableCounts?.Count != MaximumBannerTypes))
        {
            throw new PacketWireFormatException(
                $"Version4 full banner state requires exactly {MaximumBannerTypes} entries in each table.");
        }

        writer.WriteByte(payload.Action);
        switch (payload.Action)
        {
            case 0:
                WriteCount(writer, payload.KillCounts?.Count ?? throw Missing("kill-count array"));
                foreach (int count in payload.KillCounts!) writer.WriteInt32(count);
                WriteCount(writer, payload.ClaimableCounts?.Count ?? throw Missing("claimable-count array"));
                foreach (ushort count in payload.ClaimableCounts!) writer.WriteUInt16(count);
                break;
            case 1:
                writer.WriteInt16(payload.BannerId ?? throw Missing("banner ID"));
                writer.WriteInt32(payload.KillCount ?? throw Missing("kill count"));
                break;
            case 2:
                writer.WriteInt16(payload.BannerId ?? throw Missing("banner ID"));
                writer.WriteUInt16(payload.ClaimableCount ?? throw Missing("claimable count"));
                break;
            default:
                throw new PacketWireFormatException($"Unknown Packet 82 banner action {payload.Action}.");
        }
    }

    private static Packet82BannerData ReadBanners(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        switch (action)
        {
            case 0:
            {
                int killCount = ReadCount(reader, "kill-count");
                if (killCount != MaximumBannerTypes)
                    throw new PacketWireFormatException($"Version4 banner kill table must contain exactly {MaximumBannerTypes} entries.");
                var kills = new int[killCount];
                for (int index = 0; index < kills.Length; index++) kills[index] = reader.ReadInt32();
                int claimCount = ReadCount(reader, "claimable-count");
                if (claimCount != MaximumBannerTypes)
                    throw new PacketWireFormatException($"Version4 banner claim table must contain exactly {MaximumBannerTypes} entries.");
                var claims = new ushort[claimCount];
                for (int index = 0; index < claims.Length; index++) claims[index] = reader.ReadUInt16();
                return new Packet82BannerData(action, kills, claims, null, null, null);
            }
            case 1:
            {
                short bannerId = reader.ReadInt16();
                int killCount = reader.ReadInt32();
                return new Packet82BannerData(action, null, null, bannerId, killCount, null);
            }
            case 2:
            {
                short bannerId = reader.ReadInt16();
                ushort claimableCount = reader.ReadUInt16();
                return new Packet82BannerData(action, null, null, bannerId, null, claimableCount);
            }
            default:
                throw new PacketWireFormatException($"Unknown Packet 82 banner action {action}.");
        }
    }

    private static void WriteLeashed(PacketWireWriter writer, Packet82LeashedData payload)
    {
        if (payload.Action is not (1 or 2))
            throw new PacketWireFormatException($"Unknown Version4 leashed-entity action {payload.Action}.");
        bool full = payload.Action == 1;
        bool hasAnchorX = payload.AnchorX is not null;
        bool hasAnchorY = payload.AnchorY is not null;
        if (full ? !hasAnchorX || !hasAnchorY : hasAnchorX || hasAnchorY)
            throw new PacketWireFormatException("Only full leashed-entity updates carry anchor coordinates.");

        writer.WriteByte((byte)payload.Action);
        Write7BitEncodedInt(writer, payload.EntityId);
        Write7BitEncodedInt(writer, payload.EntityType);
        if (full)
        {
            writer.WriteInt16(payload.AnchorX!.Value);
            writer.WriteInt16(payload.AnchorY!.Value);
        }
    }

    private static Packet82LeashedData ReadLeashed(PacketWireReader reader)
    {
        int action = reader.ReadByte();
        if (action is not (1 or 2))
            throw new PacketWireFormatException($"Unknown Version4 leashed-entity action {action}.");
        int entityId = Read7BitEncodedInt(reader);
        int entityType = Read7BitEncodedInt(reader);
        short? anchorX = null;
        short? anchorY = null;
        if (action == 1)
        {
            anchorX = reader.ReadInt16();
            anchorY = reader.ReadInt16();
        }
        return new Packet82LeashedData(action, entityId, entityType, anchorX, anchorY);
    }

    private static void WriteCount(PacketWireWriter writer, int count)
    {
        if (count > short.MaxValue)
            throw new PacketWireFormatException("Packet 82 banner table is too long for its Int16 count.");
        writer.WriteInt16((short)count);
    }

    private static int ReadCount(PacketWireReader reader, string name)
    {
        short count = reader.ReadInt16();
        if (count < 0)
            throw new PacketWireFormatException($"Packet 82 {name} table count {count} is invalid.");
        return count;
    }

    private static void Write7BitEncodedInt(PacketWireWriter writer, int value)
    {
        uint remaining = unchecked((uint)value);
        while (remaining >= 0x80)
        {
            writer.WriteByte((byte)(remaining | 0x80));
            remaining >>= 7;
        }
        writer.WriteByte((byte)remaining);
    }

    private static int Read7BitEncodedInt(PacketWireReader reader)
    {
        uint value = 0;
        for (int shift = 0; shift <= 28; shift += 7)
        {
            byte part = reader.ReadByte();
            if (shift == 28 && part > 15)
                throw new PacketWireFormatException("Packet 82 bestiary count overflows Int32.");
            value |= (uint)(part & 0x7F) << shift;
            if ((part & 0x80) == 0) return unchecked((int)value);
        }
        throw new PacketWireFormatException("Packet 82 bestiary count has an invalid 7-bit prefix.");
    }

    private static T Require<T>(object? value) => value is T typed
        ? typed
        : throw new PacketWireFormatException($"Packet 82 module requires {typeof(T).Name} payload.");

    private static ArgumentException Missing(string name) =>
        new($"Packet 82 banner update requires a {name}.");

    private sealed class RemappedModuleCodec(ushort wireId, IPacket82ModuleWireCodec inner)
        : IPacket82ModuleWireCodec
    {
        public ushort ModuleId { get; } = wireId;

        public void Write(PacketWireWriter writer, object? payload, int tagEffectNpcSlotCount,
            Func<short, bool> tagEffectUsesProcTimes)
        {
            inner.Write(writer, payload, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
        }

        public object? Read(PacketWireReader reader, int tagEffectNpcSlotCount,
            Func<short, bool> tagEffectUsesProcTimes)
        {
            return inner.Read(reader, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
        }
    }

    private sealed class ModuleCodec<TPayload> : IPacket82ModuleWireCodec
        where TPayload : notnull
    {
        private readonly Action<PacketWireWriter, TPayload, int, Func<short, bool>> _write;
        private readonly Func<PacketWireReader, int, Func<short, bool>, TPayload> _read;

        public ModuleCodec(
            Packet82ModuleId id,
            Action<PacketWireWriter, TPayload> write,
            Func<PacketWireReader, TPayload> read)
            : this(id, (writer, payload, _, _) => write(writer, payload), (reader, _, _) => read(reader))
        {
        }

        public ModuleCodec(
            Packet82ModuleId id,
            Action<PacketWireWriter, TPayload, int, Func<short, bool>> write,
            Func<PacketWireReader, int, Func<short, bool>, TPayload> read)
        {
            ModuleId = (ushort)id;
            _write = write;
            _read = read;
        }

        public ushort ModuleId { get; }

        public void Write(PacketWireWriter writer, object? payload, int tagEffectNpcSlotCount,
            Func<short, bool> tagEffectUsesProcTimes) =>
            _write(writer, Require<TPayload>(payload), tagEffectNpcSlotCount, tagEffectUsesProcTimes);

        public object? Read(PacketWireReader reader, int tagEffectNpcSlotCount,
            Func<short, bool> tagEffectUsesProcTimes)
        {
            return _read(reader, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
        }
    }
}
