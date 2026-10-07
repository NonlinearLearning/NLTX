using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public sealed class PacketTileEntityCodecs
{
    private readonly IReadOnlyDictionary<byte, IPacketTileEntityWireCodec> _byType;

    public PacketTileEntityCodecs(IEnumerable<IPacketTileEntityWireCodec> codecs)
    {
        ArgumentNullException.ThrowIfNull(codecs);
        var map = new Dictionary<byte, IPacketTileEntityWireCodec>();
        foreach (IPacketTileEntityWireCodec codec in codecs)
        {
            if (!map.TryAdd(codec.Type, codec))
                throw new ArgumentException($"Duplicate tile-entity wire type {codec.Type}.", nameof(codecs));
        }
        byte[] expectedTypes = Enum.GetValues<PacketTileEntityType>()
            .Select(static type => (byte)type)
            .Order()
            .ToArray();
        if (!map.Keys.Order().SequenceEqual(expectedTypes))
        {
            throw new ArgumentException(
                "Version4 tile-entity codecs must cover registered type IDs 0 through 10.",
                nameof(codecs));
        }
        _byType = map;
    }

    public IPacketTileEntityWireCodec Get(byte type) => _byType.TryGetValue(type, out var codec)
        ? codec
        : throw new PacketWireFormatException($"No tile-entity wire codec is registered for type {type}.");
}
