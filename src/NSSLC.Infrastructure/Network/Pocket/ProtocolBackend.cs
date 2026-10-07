using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CSharpBackend
{
    public static GeneratedArtifact EmitProtocol(ProtocolManifest protocol, ImmutableArray<PacketIr> packets)
    {
        var builder = new StringBuilder();
        string name = protocol.Name + "Protocol";
        AppendLine(builder, "#nullable enable");
        AppendLine(builder, "using System;");
        AppendLine(builder, "using Terraria.NetWork.Prototype.PacketDesignCompiler;");
        AppendLine(builder, "namespace Terraria.NetWork.Generated;");
        AppendLine(builder, $"public static class {name}");
        AppendLine(builder, "{");
        AppendLine(builder, $"    public const string Name = {Literal(protocol.Name)};");
        AppendLine(builder, $"    public const string Version = {Literal(protocol.Version)};");
        AppendLine(builder, "    public static class Packets");
        AppendLine(builder, "    {");
        foreach (var registration in protocol.Packets)
        {
            var packet = packets.Single(packet => packet.GeneratedName == protocol.Name + "_" + registration.Name);
            string generatedName = GeneratedBaseName(packet) + "PacketCodec";
            AppendLine(builder, $"        public static class {registration.Name}");
            AppendLine(builder, "        {");
            AppendLine(builder, $"            public const byte MessageId = {packet.MessageId};");
            AppendLine(builder, $"            public const WireDirection Direction = (WireDirection){(int)packet.WireDirection};");
            AppendLine(builder, $"            public static ProtocolPacketDescriptor Descriptor {{ get; }} = new({Literal(registration.Name)}, MessageId, Direction, typeof({TypeName(packet.PacketType)}), (PacketCodecDirections){(int)packet.CodecDirections});");
            if (packet.CodecDirections.HasFlag(PacketCodecDirections.Decode))
            {
                var required = ProtocolConstructorInputs(packet, CodecDirection.Decode);
                string facts = required.IsEmpty ? string.Empty : ", " + ProtocolParameterList(required);
                string args = required.IsEmpty ? string.Empty : ", " + ProtocolArgumentList(required);
                AppendLine(builder, $"            public static {generatedName}Reader CreateReader(ReadOnlyMemory<byte> body{facts}) => new(body{args});");
                AppendLine(builder, $"            public static {generatedName}Reader CreateReader(ReadOnlyMemory<byte> buffer, int bodyLength{facts}) => new(buffer, bodyLength{args});");
            }
            if (packet.CodecDirections.HasFlag(PacketCodecDirections.Encode))
            {
                var required = ProtocolConstructorInputs(packet, CodecDirection.Encode);
                string facts = required.IsEmpty ? string.Empty : ProtocolParameterList(required);
                string args = required.IsEmpty ? string.Empty : ProtocolArgumentList(required);
                AppendLine(builder, $"            public static {generatedName}Writer CreateWriter({facts}) => new({args});");
            }
            AppendLine(builder, "        }");
        }
        AppendLine(builder, "    }");
        string entries = string.Join(", ", protocol.Packets.Select(entry => "Packets." + entry.Name + ".Descriptor"));
        AppendLine(builder, $"    public static global::System.Collections.Generic.IReadOnlyList<ProtocolPacketDescriptor> All {{ get; }} = Array.AsReadOnly(new ProtocolPacketDescriptor[] {{ {entries} }});");
        AppendLine(builder, "    public static ProtocolPacketDescriptor? Find(WireDirection direction, byte messageId)");
        AppendLine(builder, "    {");
        AppendLine(builder, "        if (direction is not (WireDirection.ClientToServer or WireDirection.ServerToClient)) throw new ArgumentOutOfRangeException(nameof(direction));");
        AppendLine(builder, "        foreach (var packet in All) if (packet.MessageId == messageId && (packet.WireDirection & direction) != 0) return packet;");
        AppendLine(builder, "        return null;");
        AppendLine(builder, "    }");
        AppendLine(builder, "}");
        string source = builder.ToString();
        return new(name + ".g.cs", "protocol:" + protocol.Name, source,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))));
    }
}
