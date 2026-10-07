using System.Buffers.Binary;
using System.Text;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

// Prototype wire-transport contract for whole-code-block codecs.
//
// A CodecRef node encloses an arbitrary run of wire bytes (loops, per-element
// scope, table lookups) in one hand-written method. The compiler records only the
// method signature and its position in the field sequence; it never inspects the
// body. These two types are the seam: the generated reader/writer constructs the
// transport over the byte range at the block's position and passes it as the
// codec's first argument.
//
// The codec returns the number of bytes it consumed (decode) or produced (encode),
// which is the single source of truth for how far the surrounding plan advances.

// Raised when a codec runs past the end of the packet. The generated reader catches
// exactly this type and returns null, so a truncated codec block fails the read the
// same way a truncated scalar field does.
public sealed class PacketWireTruncationException : Exception
{
    public PacketWireTruncationException(string message, PacketWireReader? reader = null)
        : base(message)
    {
        Reader = reader;
    }

    public PacketWireReader? Reader { get; }
}

// Raised when a codec encounters a value that cannot form a valid wire block.
public sealed class PacketWireFormatException : Exception
{
    public PacketWireFormatException(string message,
        PacketReadErrorCode code = PacketReadErrorCode.InvalidCodecData)
        : base(message)
    {
        Code = code;
    }

    public PacketReadErrorCode Code { get; }
}

// FRAME BOUNDING.
//
// A codec must only ever see the bytes of the frame it belongs to. Handing a codec the
// caller's whole receive buffer is not a convenience: it lets a compression domain, a
// length-prefixed body or a greedy loop walk straight into the NEXT frame, and the
// symptom is a plausible-looking decode of the wrong bytes rather than an error.
//
// So the transport is constructed over an explicit window. The parent buffer is never
// reachable through it: 'Frame' is the window, 'Remaining' counts down to the window end,
// and every read past that end raises PacketWireTruncationException -- the same failure a
// genuinely truncated frame produces. A codec therefore cannot distinguish "the frame
// ended" from "the buffer ended", which is exactly right: from inside a frame, that is
// the same event.
public sealed class PacketWireReader
{
    private readonly ReadOnlyMemory<byte> _frame;

    // The whole frame, with no parent buffer behind it.
    public PacketWireReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
    }

    // A window inside a larger buffer (typically one frame of a receive buffer). The
    // buffer stays private, so the window is all this reader can ever reach.
    public PacketWireReader(ReadOnlyMemory<byte> buffer, int offset, int length)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (offset > buffer.Length - length)
        {
            throw new ArgumentException(
                $"The window ({offset}..{offset + length}) does not fit in a {buffer.Length}-byte buffer.",
                nameof(length));
        }

        _frame = buffer.Slice(offset, length);
    }

    // The frame window. This is deliberately the ONLY view of the bytes a codec gets:
    // there is no property that returns the enclosing buffer.
    public ReadOnlyMemory<byte> Frame => _frame;

    public int Position { get; private set; }

    // Bytes left before the frame end -- not before the buffer end.
    public int Remaining => _frame.Length - Position;

    public ReadOnlySpan<byte> RemainingSpan => _frame.Span[Position..];

    // True once the whole frame has been consumed. A codec that owns a byte range can
    // use this to reject a frame with trailing bytes instead of silently ignoring them.
    public bool AtFrameEnd => Position == _frame.Length;

    // Fails unless the frame was consumed exactly. Callers that own a delimited region
    // should use this rather than trusting Remaining to be zero.
    public void RequireFrameEnd()
    {
        if (!AtFrameEnd)
        {
            throw new PacketWireFormatException(
                $"The wire reader stopped at {Position} of {_frame.Length} frame bytes; {Remaining} byte(s) were left unconsumed.",
                PacketReadErrorCode.TrailingBytes);
        }
    }

    public byte ReadByte()
    {
        Require(1);
        return _frame.Span[Position++];
    }

    public bool TryReadByte(out byte value)
    {
        if (Remaining < 1)
        {
            value = 0;
            return false;
        }

        value = ReadByte();
        return true;
    }

    public sbyte ReadSByte() => unchecked((sbyte)ReadByte());

    public bool ReadBoolean() => ReadByte() != 0;

    public string ReadString()
    {
        uint byteCount = 0;
        for (int shift = 0; shift <= 28; shift += 7)
        {
            byte part = ReadByte();
            if (shift == 28 && part > 7)
            {
                throw new PacketWireFormatException("String length prefix overflows Int32.", PacketReadErrorCode.InvalidLengthPrefix);
            }

            byteCount |= (uint)(part & 0x7F) << shift;
            if ((part & 0x80) == 0)
            {
                if (byteCount > int.MaxValue)
                {
                    throw new PacketWireFormatException("String length prefix overflows Int32.", PacketReadErrorCode.InvalidLengthPrefix);
                }

                return Encoding.UTF8.GetString(ReadBytes((int)byteCount).Span);
            }
        }

        throw new PacketWireFormatException("String length prefix is too long.", PacketReadErrorCode.InvalidLengthPrefix);
    }

    public short ReadInt16() => BinaryPrimitives.ReadInt16LittleEndian(Take(2));

    public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));

    public int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian(Take(4));

    public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(Take(4));

    public long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian(Take(8));

    public ulong ReadUInt64() => BinaryPrimitives.ReadUInt64LittleEndian(Take(8));

    public float ReadSingle() => BinaryPrimitives.ReadSingleLittleEndian(Take(4));

    public ReadOnlyMemory<byte> ReadBytes(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Require(count);
        var slice = _frame.Slice(Position, count);
        Position += count;
        return slice;
    }

    public void Skip(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Require(count);
        Position += count;
    }

    private ReadOnlySpan<byte> Take(int count)
    {
        Require(count);
        var slice = _frame.Span.Slice(Position, count);
        Position += count;
        return slice;
    }

    private void Require(int count)
    {
        if (Remaining < count)
        {
            throw new PacketWireTruncationException(
                $"The wire reader needs {count} more byte(s) but only {Remaining} remain.", this);
        }
    }
}

public sealed class PacketWireLimitException : Exception
{
    public PacketWireLimitException(string message)
        : base(message)
    {
    }
}

public sealed class PacketWireWriter
{
    private readonly MemoryStream _stream;
    private readonly int _maximumBytes;

    public PacketWireWriter(MemoryStream stream, int maximumBytes = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (maximumBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        }

        _stream = stream;
        if (stream.Position != stream.Length)
            throw new ArgumentException("A wire writer must start at the end of its stream.", nameof(stream));
        Origin = stream.Position;
        _maximumBytes = maximumBytes;
    }

    // A disposed view never owns or closes the underlying writer stream.
    public Stream OpenWriteStream() => new AppendWriteStream(this, _stream);

    // Compatibility access has the same append-only contract, regardless of bounds.
    public Stream Stream => OpenWriteStream();

    // Byte offset of this block inside the shared stream.
    public long Origin { get; }

    public int BytesWritten => (int)(_stream.Position - Origin);

    public void WriteByte(byte value)
    {
        EnsureCanWrite(1);
        _stream.WriteByte(value);
    }

    public void WriteSByte(sbyte value) => WriteByte(unchecked((byte)value));

    public void WriteBoolean(bool value) => WriteByte(value ? (byte)1 : (byte)0);

    public void WriteInt16(short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteUInt16(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteInt32(int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteUInt32(uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteInt64(long value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteUInt64(ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteSingle(float value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        Write(buffer);
    }

    public void WriteBytes(ReadOnlySpan<byte> value) => Write(value);

    public void WriteString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        uint remaining = (uint)bytes.Length;
        while (remaining >= 0x80)
        {
            WriteByte((byte)(remaining | 0x80));
            remaining >>= 7;
        }

        WriteByte((byte)remaining);
        WriteBytes(bytes);
    }

    private void Write(ReadOnlySpan<byte> value)
    {
        EnsureCanWrite(value.Length);
        _stream.Write(value);
    }

    private void EnsureCanWrite(int count)
    {
        if ((long)BytesWritten + count > _maximumBytes)
        {
            throw new PacketWireLimitException(
                $"The codec block exceeds its {_maximumBytes}-byte maximum.");
        }
    }

    private sealed class AppendWriteStream : Stream
    {
        private readonly PacketWireWriter _owner;
        private readonly MemoryStream _stream;
        private bool _disposed;

        public AppendWriteStream(PacketWireWriter owner, MemoryStream stream)
        {
            _owner = owner;
            _stream = stream;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => !_disposed;

        public override long Length => _owner.BytesWritten;

        public override long Position
        {
            get => _owner.BytesWritten;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _stream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (offset > buffer.Length - count)
            {
                throw new ArgumentException("The write range exceeds the supplied buffer.");
            }

            _owner.EnsureCanWrite(count);
            _stream.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _owner.EnsureCanWrite(buffer.Length);
            _stream.Write(buffer);
        }

        public override void WriteByte(byte value)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _owner.EnsureCanWrite(1);
            _stream.WriteByte(value);
        }
    }
}
