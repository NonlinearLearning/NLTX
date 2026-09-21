using System.Buffers.Binary;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class PreferencesStoreAdapter
{
  public delegate void TextProcessAction(ref string text);

  private readonly FilePlatformAdapter _platform;
  private readonly string _path;
  private readonly PreferencesSerializationPolicy _policy;
  private readonly object _lock = new();
  private Dictionary<string, object?> _data = new(StringComparer.Ordinal);

  public PreferencesStoreAdapter(
    FilePlatformAdapter platform,
    string path,
    bool parseAllTypes = false,
    bool useBson = false)
    : this(
      platform,
      path,
      new PreferencesSerializationPolicy(
        parseAllTypes,
        useBson ? PreferencesSerializationFormat.Bson : PreferencesSerializationFormat.Json))
  {
  }

  public PreferencesStoreAdapter(
    FilePlatformAdapter platform,
    string path,
    PreferencesSerializationPolicy policy)
  {
    _platform = platform ?? throw new ArgumentNullException(nameof(platform));
    _path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A preferences path is required.", nameof(path))
      : path;
    _policy = policy;
  }

  public bool AutoSave { get; set; }

  public bool UseBson => _policy.Format == PreferencesSerializationFormat.Bson;

  public string Path => _path;

  public event Action<PreferencesStoreAdapter>? OnSave;

  public event Action<PreferencesStoreAdapter>? OnLoad;

  public event TextProcessAction? OnProcessText;

  public PreferencesLoadResult Load()
  {
    lock (_lock)
    {
      FileExistenceResult existence = _platform.Exists(_path, isCloud: false);
      if (!existence.Exists)
      {
        return existence.Failure.Kind == FilePlatformFailureKind.None
          ? PreferencesLoadResult.MissingFile
          : PreferencesLoadResult.Failed(existence.Failure);
      }

      FileReadResult read = _platform.ReadAllBytes(_path, isCloud: false);
      if (!read.Succeeded || read.Data is null)
      {
        return PreferencesLoadResult.Failed(read.Failure);
      }

      try
      {
        Dictionary<string, object?> parsed = UseBson
          ? BsonDocumentCodec.Read(read.Data)
          : ReadJson(read.Data);
        _data = parsed;
        OnLoad?.Invoke(this);
        return PreferencesLoadResult.Success;
      }
      catch (Exception exception)
      {
        return PreferencesLoadResult.Failed(
          FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
      }
    }
  }

  public PreferencesSaveResult Save(bool canCreateFile = true)
  {
    lock (_lock)
    {
      try
      {
        OnSave?.Invoke(this);
      }
      catch (Exception exception)
      {
        return PreferencesSaveResult.Failed(
          FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
      }

      if (!canCreateFile)
      {
        FileExistenceResult existence = _platform.Exists(_path, isCloud: false);
        if (!existence.Exists)
        {
          return existence.Failure.Kind == FilePlatformFailureKind.None
            ? PreferencesSaveResult.SkippedMissingFile
            : PreferencesSaveResult.Failed(existence.Failure);
        }
      }

      try
      {
        byte[] bytes;
        if (UseBson)
        {
          bytes = BsonDocumentCodec.Write(_data);
        }
        else
        {
          string text = JsonSerializer.Serialize(
            _data,
            new JsonSerializerOptions { WriteIndented = true });
          OnProcessText?.Invoke(ref text);
          bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(text);
        }

        FilePlatformOperationResult write = _platform.WriteAllBytes(_path, bytes, isCloud: false);
        return write.Succeeded
          ? PreferencesSaveResult.Saved
          : PreferencesSaveResult.Failed(write.Failure);
      }
      catch (Exception exception)
      {
        return PreferencesSaveResult.Failed(
          FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure, exception.Message));
      }
    }
  }

  public PreferencesSaveResult Put(string name, object? value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    lock (_lock)
    {
      _data[name] = value;
      return AutoSave ? Save() : PreferencesSaveResult.Saved;
    }
  }

  public void Clear()
  {
    lock (_lock)
    {
      _data.Clear();
    }
  }

  public bool Contains(string name)
  {
    ArgumentNullException.ThrowIfNull(name);
    lock (_lock)
    {
      return _data.ContainsKey(name);
    }
  }

  public T Get<T>(string name, T defaultValue)
  {
    ArgumentNullException.ThrowIfNull(name);
    lock (_lock)
    {
      return _data.TryGetValue(name, out object? value) &&
        PreferencesValueConverter.TryConvert(value, out T converted)
        ? converted
        : defaultValue;
    }
  }

  public IReadOnlyList<string> GetAllKeys()
  {
    lock (_lock)
    {
      return Array.AsReadOnly(_data.Keys.ToArray());
    }
  }

  private static Dictionary<string, object?> ReadJson(byte[] bytes)
  {
    using JsonDocument document = JsonDocument.Parse(bytes);
    if (document.RootElement.ValueKind != JsonValueKind.Object)
    {
      throw new FormatException("Preferences root must be a JSON object.");
    }

    return (Dictionary<string, object?>)ReadJsonValue(document.RootElement)!;
  }

  private static object? ReadJsonValue(JsonElement element)
  {
    return element.ValueKind switch
    {
      JsonValueKind.Object => element.EnumerateObject().ToDictionary(
        property => property.Name,
        property => ReadJsonValue(property.Value),
        StringComparer.Ordinal),
      JsonValueKind.Array => element.EnumerateArray().Select(ReadJsonValue).ToList(),
      JsonValueKind.String => element.GetString(),
      JsonValueKind.True => true,
      JsonValueKind.False => false,
      JsonValueKind.Number when element.TryGetInt64(out long integer) => integer,
      JsonValueKind.Number => element.GetDouble(),
      JsonValueKind.Null => null,
      _ => throw new FormatException("Unsupported JSON preference value.")
    };
  }

  private static class BsonDocumentCodec
  {
    public static byte[] Write(IReadOnlyDictionary<string, object?> values)
    {
      using MemoryStream stream = new();
      WriteDocument(stream, values);
      return stream.ToArray();
    }

    public static Dictionary<string, object?> Read(byte[] bytes)
    {
      ArgumentNullException.ThrowIfNull(bytes);
      int offset = 0;
      object? value = ReadDocument(bytes, ref offset);
      if (offset != bytes.Length || value is not Dictionary<string, object?> document)
      {
        throw new FormatException("Invalid BSON preferences document.");
      }

      return document;
    }

    private static void WriteDocument(Stream stream, IReadOnlyDictionary<string, object?> values)
    {
      using MemoryStream body = new();
      foreach ((string key, object? value) in values)
      {
        WriteElement(body, key, value);
      }

      body.WriteByte(0);
      int length = checked((int)body.Length + 4);
      Span<byte> header = stackalloc byte[4];
      BinaryPrimitives.WriteInt32LittleEndian(header, length);
      stream.Write(header);
      body.Position = 0;
      body.CopyTo(stream);
    }

    private static void WriteElement(Stream stream, string key, object? value)
    {
      switch (value)
      {
        case null:
          stream.WriteByte(0x0A);
          WriteCString(stream, key);
          break;
        case string text:
          stream.WriteByte(0x02);
          WriteCString(stream, key);
          WriteString(stream, text);
          break;
        case bool boolean:
          stream.WriteByte(0x08);
          WriteCString(stream, key);
          stream.WriteByte(boolean ? (byte)1 : (byte)0);
          break;
        case byte number:
          WriteInt64(stream, key, number);
          break;
        case short number:
          WriteInt64(stream, key, number);
          break;
        case int number:
          stream.WriteByte(0x10);
          WriteCString(stream, key);
          WriteInt32(stream, number);
          break;
        case long number:
          WriteInt64(stream, key, number);
          break;
        case float number:
          WriteDouble(stream, key, number);
          break;
        case double number:
          WriteDouble(stream, key, number);
          break;
        case IReadOnlyDictionary<string, object?> document:
          stream.WriteByte(0x03);
          WriteCString(stream, key);
          WriteDocument(stream, document);
          break;
        case IDictionary dictionary:
          stream.WriteByte(0x03);
          WriteCString(stream, key);
          WriteDocument(stream, dictionary.Cast<DictionaryEntry>().ToDictionary(
            entry => Convert.ToString(entry.Key, CultureInfo.InvariantCulture)!,
            entry => entry.Value,
            StringComparer.Ordinal));
          break;
        case IEnumerable sequence:
          stream.WriteByte(0x04);
          WriteCString(stream, key);
          Dictionary<string, object?> array = sequence.Cast<object?>().Select((item, index) => (item, index)).ToDictionary(
            item => item.index.ToString(CultureInfo.InvariantCulture),
            item => item.item,
            StringComparer.Ordinal);
          WriteDocument(stream, array);
          break;
        default:
          JsonElement converted = JsonSerializer.SerializeToElement(value);
          WriteElement(stream, key, JsonElementToObject(converted));
          break;
      }
    }

    private static object? ReadDocument(byte[] bytes, ref int offset)
    {
      int start = offset;
      int length = ReadInt32(bytes, ref offset);
      if (length < 5 || start + length > bytes.Length)
      {
        throw new FormatException("Invalid BSON document length.");
      }

      int end = start + length;
      Dictionary<string, object?> values = new(StringComparer.Ordinal);
      while (offset < end - 1)
      {
        byte type = ReadByte(bytes, ref offset);
        string key = ReadCString(bytes, ref offset, end);
        values[key] = ReadValue(bytes, ref offset, type);
      }

      if (ReadByte(bytes, ref offset) != 0 || offset != end)
      {
        throw new FormatException("Invalid BSON document terminator.");
      }

      return values;
    }

    private static object? ReadValue(byte[] bytes, ref int offset, byte type)
    {
      return type switch
      {
        0x01 => ReadDouble(bytes, ref offset),
        0x02 => ReadString(bytes, ref offset),
        0x03 => ReadDocument(bytes, ref offset),
        0x04 => ReadArray(bytes, ref offset),
        0x08 => ReadByte(bytes, ref offset) != 0,
        0x0A => null,
        0x10 => ReadInt32(bytes, ref offset),
        0x12 => ReadInt64(bytes, ref offset),
        _ => throw new FormatException($"Unsupported BSON value type 0x{type:X2}.")
      };
    }

    private static List<object?> ReadArray(byte[] bytes, ref int offset)
    {
      if (ReadDocument(bytes, ref offset) is not Dictionary<string, object?> document)
      {
        throw new FormatException("Invalid BSON array.");
      }

      return document.OrderBy(item => int.Parse(item.Key, CultureInfo.InvariantCulture)).Select(item => item.Value).ToList();
    }

    private static void WriteCString(Stream stream, string value)
    {
      if (value.Contains('\0'))
      {
        throw new FormatException("BSON keys cannot contain null characters.");
      }

      byte[] bytes = Encoding.UTF8.GetBytes(value);
      stream.Write(bytes);
      stream.WriteByte(0);
    }

    private static void WriteString(Stream stream, string value)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(value);
      Span<byte> length = stackalloc byte[4];
      BinaryPrimitives.WriteInt32LittleEndian(length, checked(bytes.Length + 1));
      stream.Write(length);
      stream.Write(bytes);
      stream.WriteByte(0);
    }

    private static void WriteInt32(Stream stream, int value)
    {
      Span<byte> bytes = stackalloc byte[4];
      BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
      stream.Write(bytes);
    }

    private static void WriteInt64(Stream stream, string key, long value)
    {
      stream.WriteByte(0x12);
      WriteCString(stream, key);
      Span<byte> bytes = stackalloc byte[8];
      BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
      stream.Write(bytes);
    }

    private static void WriteDouble(Stream stream, string key, double value)
    {
      stream.WriteByte(0x01);
      WriteCString(stream, key);
      Span<byte> bytes = stackalloc byte[8];
      BinaryPrimitives.WriteInt64LittleEndian(bytes, BitConverter.DoubleToInt64Bits(value));
      stream.Write(bytes);
    }

    private static byte ReadByte(byte[] bytes, ref int offset)
    {
      if ((uint)offset >= (uint)bytes.Length)
      {
        throw new FormatException("BSON document is truncated.");
      }

      return bytes[offset++];
    }

    private static int ReadInt32(byte[] bytes, ref int offset)
    {
      if (offset + 4 > bytes.Length)
      {
        throw new FormatException("BSON document is truncated.");
      }

      int value = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset, 4));
      offset += 4;
      return value;
    }

    private static long ReadInt64(byte[] bytes, ref int offset)
    {
      if (offset + 8 > bytes.Length)
      {
        throw new FormatException("BSON document is truncated.");
      }

      long value = BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(offset, 8));
      offset += 8;
      return value;
    }

    private static double ReadDouble(byte[] bytes, ref int offset)
    {
      return BitConverter.Int64BitsToDouble(ReadInt64(bytes, ref offset));
    }

    private static string ReadString(byte[] bytes, ref int offset)
    {
      int length = ReadInt32(bytes, ref offset);
      if (length < 1 || offset + length > bytes.Length || bytes[offset + length - 1] != 0)
      {
        throw new FormatException("Invalid BSON string.");
      }

      string value = Encoding.UTF8.GetString(bytes, offset, length - 1);
      offset += length;
      return value;
    }

    private static string ReadCString(byte[] bytes, ref int offset, int end)
    {
      int start = offset;
      while (offset < end && bytes[offset] != 0)
      {
        offset++;
      }

      if (offset >= end)
      {
        throw new FormatException("Invalid BSON key.");
      }

      string value = Encoding.UTF8.GetString(bytes, start, offset - start);
      offset++;
      return value;
    }

    private static object? JsonElementToObject(JsonElement element)
    {
      return element.ValueKind switch
      {
        JsonValueKind.Object => element.EnumerateObject().ToDictionary(
          property => property.Name,
          property => JsonElementToObject(property.Value),
          StringComparer.Ordinal),
        JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number when element.TryGetInt64(out long integer) => integer,
        JsonValueKind.Number => element.GetDouble(),
        _ => null
      };
    }
  }
}
