using System;
using System.Text.Json;

namespace Terraria.Dome.Transport;

public static class DomeFrameCodec
{
  private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

  public static T Deserialize<T>(string frame)
  {
    ArgumentNullException.ThrowIfNull(frame);
    T? result = JsonSerializer.Deserialize<T>(frame, Options);
    if (result is null)
    {
      throw new InvalidOperationException("The frame payload is empty.");
    }

    return result;
  }

  public static string Serialize<T>(T frame)
  {
    return JsonSerializer.Serialize(frame, Options);
  }
}
