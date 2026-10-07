using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class NetworkTextCodec
{
    private const int MaximumNodes = ushort.MaxValue / 2;
    private const int MaximumTextBytes = ushort.MaxValue;

    public static void Write(PacketWireWriter writer, NetworkText value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var budget = new Budget();
        WriteNode(writer, value, budget);
    }

    public static NetworkText Read(PacketWireReader reader)
    {
        var budget = new Budget();
        var ancestors = new Stack<NodeBuilder>();
        NodeBuilder current = ReadNodeHeader(reader, budget);
        while (true)
        {
            if (current.Children.Count < current.ExpectedChildren)
            {
                ancestors.Push(current);
                current = ReadNodeHeader(reader, budget);
                continue;
            }

            NetworkText completed = current.Build();
            if (ancestors.Count == 0)
                return completed;

            current = ancestors.Pop();
            current.Children.Add(completed);
        }
    }

    private static void WriteNode(PacketWireWriter writer, NetworkText value, Budget budget)
    {
        var pending = new Stack<NetworkText>();
        pending.Push(value);
        while (pending.Count != 0)
        {
            NetworkText current = pending.Pop();
            if (current is null)
                throw new PacketWireFormatException("NetworkText substitutions cannot contain null values.");
            budget.Visit();
            if (!Enum.IsDefined(current.Mode))
                throw new PacketWireFormatException($"Unknown NetworkText mode {(byte)current.Mode}.");
            if (current.Text is null || current.Substitutions is null)
                throw new PacketWireFormatException("NetworkText text and substitutions cannot be null.");

            writer.WriteByte((byte)current.Mode);
            WriteString(writer, current.Text, budget);
            if (current.Mode == NetworkTextMode.Literal)
            {
                if (current.Substitutions.Count != 0)
                    throw new PacketWireFormatException("Literal NetworkText cannot contain substitutions.");
                continue;
            }

            writer.WriteByte(unchecked((byte)current.Substitutions.Count));
            int serializedCount = current.Substitutions.Count & 0xFF;
            for (int index = serializedCount - 1; index >= 0; index--)
                pending.Push(current.Substitutions[index]);
        }
    }

    private static NodeBuilder ReadNodeHeader(PacketWireReader reader, Budget budget)
    {
        budget.Visit();
        byte rawMode = reader.ReadByte();
        if (rawMode > (byte)NetworkTextMode.LocalizationKey)
            throw new PacketWireFormatException($"Unknown NetworkText mode {rawMode}.");

        NetworkTextMode mode = (NetworkTextMode)rawMode;
        string text = ReadString(reader, budget);
        int childCount = mode == NetworkTextMode.Literal ? 0 : reader.ReadByte();
        return new NodeBuilder(mode, text, childCount);
    }

    private static void WriteString(PacketWireWriter writer, string value, Budget budget)
    {
        byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(value);
        budget.AddBytes(utf8.Length);
        uint length = (uint)utf8.Length;
        while (length >= 0x80)
        {
            writer.WriteByte((byte)(length | 0x80));
            length >>= 7;
        }
        writer.WriteByte((byte)length);
        writer.WriteBytes(utf8);
    }

    private static string ReadString(PacketWireReader reader, Budget budget)
    {
        uint length = 0;
        int shift = 0;
        for (; shift <= 28; shift += 7)
        {
            byte current = reader.ReadByte();
            if (shift == 28 && current > 7)
                throw new PacketWireFormatException("NetworkText string length overflows Int32.");
            length |= (uint)(current & 0x7F) << shift;
            if ((current & 0x80) == 0)
                break;
        }
        if (shift > 28 || length > int.MaxValue)
            throw new PacketWireFormatException("NetworkText string length prefix is invalid.");

        budget.AddBytes((int)length);
        try
        {
            return System.Text.Encoding.UTF8.GetString(reader.ReadBytes((int)length).Span);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new PacketWireFormatException("NetworkText string length is invalid: " + exception.Message);
        }
    }

    private sealed class Budget
    {
        private int _nodes;
        private int _bytes;

        public void Visit()
        {
            if (++_nodes > MaximumNodes)
                throw new PacketWireFormatException("NetworkText node budget exceeded.");
        }

        public void AddBytes(int count)
        {
            _bytes = checked(_bytes + count);
            if (_bytes > MaximumTextBytes)
                throw new PacketWireFormatException("NetworkText byte budget exceeded.");
        }
    }

    private sealed class NodeBuilder(NetworkTextMode mode, string text, int expectedChildren)
    {
        public int ExpectedChildren { get; } = expectedChildren;

        public List<NetworkText> Children { get; } = new(expectedChildren);

        public NetworkText Build() => mode switch
        {
            NetworkTextMode.Literal => NetworkText.Literal(text),
            NetworkTextMode.Formattable => NetworkText.Formattable(text, Children.ToArray()),
            NetworkTextMode.LocalizationKey => NetworkText.Key(text, Children.ToArray()),
            _ => throw new PacketWireFormatException($"Unknown NetworkText mode {(byte)mode}.")
        };
    }
}
