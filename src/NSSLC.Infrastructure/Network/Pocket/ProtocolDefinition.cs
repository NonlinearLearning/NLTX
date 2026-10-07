using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public sealed record FrameLayout(int Capacity, int HeaderBytes);

public sealed record ProtocolPacketDefinition(string Name, PacketGraphManifest Packet);

public sealed record ProtocolPacketDescriptor(string Name, byte MessageId,
    WireDirection WireDirection, Type PacketType, PacketCodecDirections Directions);

public sealed class ProtocolManifest
{
    internal ProtocolManifest(string name, string version, ImmutableArray<ProtocolPacketDefinition> packets,
        FrameLayout? frame)
    {
        Name = name;
        Version = version;
        Packets = packets;
        Frame = frame;
    }

    public string Name { get; }
    public string Version { get; }
    public ImmutableArray<ProtocolPacketDefinition> Packets { get; }
    public FrameLayout? Frame { get; }

    public ProtocolPacketDefinition? Find(WireDirection direction, byte messageId)
    {
        if (direction is not (WireDirection.ClientToServer or WireDirection.ServerToClient))
            throw new ArgumentOutOfRangeException(nameof(direction));
        return Packets.SingleOrDefault(packet => packet.Packet.MessageId == messageId
            && packet.Packet.WireDirection.HasFlag(direction));
    }
}

public sealed class ProtocolDefinition
{
    private readonly List<ProtocolPacketDefinition> _packets = [];
    private ProtocolManifest? _manifest;

    public ProtocolDefinition(string name, string version = "", FrameLayout? frame = null)
    {
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(version);
        Name = name;
        Version = version;
        Frame = frame;
    }

    public string Name { get; }
    public string Version { get; }
    public FrameLayout? Frame { get; }

    public void Register(string name, PacketGraphManifest packet)
    {
        if (_manifest is not null) throw new InvalidOperationException("The protocol is frozen after Build().");
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(packet);
        if (name is "All" or "Find" or "Packets" or "CreateReader" or "CreateWriter"
            or "MessageId" or "Direction" or "Descriptor" || name == Name + "Protocol")
            throw new ArgumentException("The registration name conflicts with the catalog API.", nameof(name));
        if (_packets.Any(entry => entry.Name == name))
            throw new ArgumentException("Protocol packet names must be unique.", nameof(name));
        if (_packets.Any(entry => entry.Packet.MessageId == packet.MessageId
            && (entry.Packet.WireDirection & packet.WireDirection) != 0))
            throw new ArgumentException("A direction and message ID can identify only one packet.", nameof(packet));
        _packets.Add(new(name, packet));
    }

    public void Register(string name, PacketGraph packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Register(name, packet.Build());
    }

    public ProtocolManifest Build() => _manifest ??= new(Name, Version,
        _packets.OrderBy(static packet => packet.Name, StringComparer.Ordinal).ToImmutableArray(), Frame);

    private static void ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!SyntaxFacts.IsValidIdentifier(name) || name.StartsWith('@')
            || SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
            throw new ArgumentException("Names must be unescaped C# identifiers and cannot be keywords.", nameof(name));
    }
}
