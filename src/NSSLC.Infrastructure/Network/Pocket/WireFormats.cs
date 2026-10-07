using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public enum WireFormatKind : byte { Scalar, Structure, Codec }

public abstract class WireFormat
{
    internal WireFormat(string name, Type valueType, WireFormatKind kind,
        PacketCodecDirections directions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        ValueType = valueType;
        Kind = kind;
        Directions = directions;
    }

    public string Name { get; }
    public Type ValueType { get; }
    public WireFormatKind Kind { get; }
    public PacketCodecDirections Directions { get; }
    internal ScalarKind Scalar { get; init; }
    internal PacketGraphManifest? Definition { get; init; }
    internal MethodRef? ReadMethod { get; init; }
    internal MethodRef? WriteMethod { get; init; }
    internal ByteRange? LengthRange { get; init; }
}

public sealed class WireFormat<T> : WireFormat
{
    internal WireFormat(string name, WireFormatKind kind,
        PacketCodecDirections directions = PacketCodecDirections.Bidirectional)
        : base(name, typeof(T), kind, directions) { }

    public static WireFormat<T> FromCodec(string name,
        Delegate write, Delegate read,
        ByteRange? lengthRange = null)
    {
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(read);
        return new WireFormat<T>(name, WireFormatKind.Codec)
        {
            WriteMethod = CompilerMethodCatalog.Find(write) ?? CompilerMethodCatalog.Unresolved,
            ReadMethod = CompilerMethodCatalog.Find(read) ?? CompilerMethodCatalog.Unresolved,
            LengthRange = lengthRange
        };
    }
}

// A format has a field/dependency scope but no message identity in the protocol catalog.
public sealed class WireFormatBuilder<T> : PacketGraph<T>
{
    private readonly string _name;
    private readonly ByteRange? _lengthRange;
    private WireFormat<T>? _format;

    public WireFormatBuilder(string name,
        PacketCodecDirections directions = PacketCodecDirections.Bidirectional,
        ByteRange? lengthRange = null)
        : base(0, codecDirections: directions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        _lengthRange = lengthRange;
    }

    public new WireFormat<T> Build() => _format ??= new WireFormat<T>(
        _name, WireFormatKind.Structure, base.Build().CodecDirections)
    {
        Definition = base.Build(),
        LengthRange = _lengthRange
    };
}

public static class WireFormats
{
    private static WireFormat<T> Scalar<T>(string name, ScalarKind kind) =>
        new(name, WireFormatKind.Scalar) { Scalar = kind };

    public static WireFormat<byte> Byte { get; } = Scalar<byte>("Byte", ScalarKind.Byte);
    public static WireFormat<sbyte> SByte { get; } = Scalar<sbyte>("SByte", ScalarKind.SByte);
    public static WireFormat<short> Int16 { get; } = Scalar<short>("Int16", ScalarKind.Int16);
    public static WireFormat<ushort> UInt16 { get; } = Scalar<ushort>("UInt16", ScalarKind.UInt16);
    public static WireFormat<int> Int32 { get; } = Scalar<int>("Int32", ScalarKind.Int32);
    public static WireFormat<uint> UInt32 { get; } = Scalar<uint>("UInt32", ScalarKind.UInt32);
    public static WireFormat<long> Int64 { get; } = Scalar<long>("Int64", ScalarKind.Int64);
    public static WireFormat<ulong> UInt64 { get; } = Scalar<ulong>("UInt64", ScalarKind.UInt64);
    public static WireFormat<bool> Boolean { get; } = Scalar<bool>("Boolean", ScalarKind.Bool);
    public static WireFormat<float> Single { get; } = Scalar<float>("Single", ScalarKind.Float32);
}

public sealed record FieldFormatBinding(PacketFieldNode Field, WireFormat Format);

public sealed record WireFormatIr(WireFormat Format, PacketIr? Structure = null);

public sealed record FieldFormatIr(PacketFieldNode Field, WireFormatIr Format);
