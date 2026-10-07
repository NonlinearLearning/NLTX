namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ProtocolInputs
{
    private static ProtocolInputs? s_instance;
    public static bool IsInitialized => Volatile.Read(ref s_instance)is not null;
    public static ProtocolInputs Instance => Volatile.Read(ref s_instance) ?? throw new InvalidOperationException("The packet external dependency model has not been initialized.");
    public bool[] FrameImportant { get; }
    public bool[] AllowsSaveCompressionBatching { get; }
    public PacketTileEntityCodecs TileEntityCodecs { get; }
    public bool IsServer { get; }
    public bool[] CatchableTypes { get; }
    public Func<short, byte?, float?, byte> LifeWidthResolver { get; }
    public Func<short, bool> NeedsUuid { get; }
    public int SlotCount { get; }
    public IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> ModuleCodecs { get; }
    public int TagEffectNpcSlotCount { get; }
    public Func<short, bool> TagEffectUsesProcTimes { get; }

    public ProtocolInputs(
        bool[] frameImportant,
        bool[] allowsSaveCompressionBatching,
        PacketTileEntityCodecs tileEntityCodecs,
        bool isServer,
        bool[] catchableTypes,
        Func<short, byte?, float?, byte> lifeWidthResolver,
        Func<short, bool> needsUuid,
        int slotCount,
        IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> moduleCodecs,
        int tagEffectNpcSlotCount,
        Func<short, bool> tagEffectUsesProcTimes)
    {
        if (IsInitialized)
            throw new InvalidOperationException("The packet external dependency model can only be initialized once.");
        ArgumentNullException.ThrowIfNull(frameImportant);
        ArgumentNullException.ThrowIfNull(allowsSaveCompressionBatching);
        ArgumentNullException.ThrowIfNull(tileEntityCodecs);
        ArgumentNullException.ThrowIfNull(catchableTypes);
        ArgumentNullException.ThrowIfNull(lifeWidthResolver);
        ArgumentNullException.ThrowIfNull(needsUuid);
        ArgumentNullException.ThrowIfNull(moduleCodecs);
        ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
        FrameImportant = frameImportant;
        AllowsSaveCompressionBatching = allowsSaveCompressionBatching;
        TileEntityCodecs = tileEntityCodecs;
        IsServer = isServer;
        CatchableTypes = catchableTypes;
        LifeWidthResolver = lifeWidthResolver;
        NeedsUuid = needsUuid;
        SlotCount = slotCount;
        ModuleCodecs = moduleCodecs;
        TagEffectNpcSlotCount = tagEffectNpcSlotCount;
        TagEffectUsesProcTimes = tagEffectUsesProcTimes;
        // Publish only a complete model; concurrent constructors must not replace it.
        if (Interlocked.CompareExchange(ref s_instance, this, null)is not null)
            throw new InvalidOperationException("The packet external dependency model can only be initialized once.");
    }
}
