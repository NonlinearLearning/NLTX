using System.Text.Json;
using System.Text.Json.Serialization;

namespace Terraria.NonAuthoritative.SimulationHost;

internal readonly record struct RuntimePlayerInput(
  int Horizontal,
  bool Jump,
  bool UseItem);

internal sealed class SimulationInputScript
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = false,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
  };

  private readonly Dictionary<(long Tick, int PlayerSlot), RuntimePlayerInput> _frames;
  private readonly Dictionary<int, bool> _magicQuiverByPlayer;

  private SimulationInputScript(
    string sourcePath,
    Dictionary<(long Tick, int PlayerSlot), RuntimePlayerInput> frames,
    Dictionary<int, bool> magicQuiverByPlayer)
  {
    SourcePath = sourcePath;
    _frames = frames;
    _magicQuiverByPlayer = magicQuiverByPlayer;
  }

  public string SourcePath { get; }

  public static SimulationInputScript Load(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    string fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
      throw new FileNotFoundException("The simulation input script does not exist.", fullPath);
    }

    ScriptDocument document;
    try
    {
      document = JsonSerializer.Deserialize<ScriptDocument>(
        File.ReadAllText(fullPath),
        JsonOptions) ?? throw new InvalidDataException("The input script is empty.");
    }
    catch (JsonException exception)
    {
      throw new InvalidDataException("The input script is not valid JSON.", exception);
    }

    if (document.Frames is null)
    {
      throw new InvalidDataException("The input script must contain a frames array.");
    }

    var frames = new Dictionary<(long Tick, int PlayerSlot), RuntimePlayerInput>();
    foreach (ScriptFrame frame in document.Frames)
    {
      if (frame.Tick <= 0 ||
          frame.PlayerSlot < 0 || frame.PlayerSlot >= byte.MaxValue ||
          frame.Horizontal is < -1 or > 1)
      {
        throw new InvalidDataException(
          "Input frames require a positive tick, player slot 0-254, and horizontal input -1, 0, or 1.");
      }

      var key = (frame.Tick, frame.PlayerSlot);
      if (!frames.TryAdd(key, new RuntimePlayerInput(
            frame.Horizontal,
            frame.Jump,
            frame.UseItem)))
      {
        throw new InvalidDataException(
          $"The input script contains duplicate input for player {frame.PlayerSlot} at tick {frame.Tick}.");
      }
    }

    var magicQuiverByPlayer = new Dictionary<int, bool>();
    if (document.Players is not null)
    {
      foreach (ScriptPlayer player in document.Players)
      {
        if ((uint)player.PlayerSlot >= byte.MaxValue)
        {
          throw new InvalidDataException(
            "Player capability entries require a player slot from 0 to 254.");
        }

        if (!magicQuiverByPlayer.TryAdd(player.PlayerSlot, player.MagicQuiver))
        {
          throw new InvalidDataException(
            $"The input script contains duplicate capabilities for player {player.PlayerSlot}.");
        }
      }
    }

    return new SimulationInputScript(fullPath, frames, magicQuiverByPlayer);
  }

  public void ValidatePlayerCount(int playerCount)
  {
    foreach ((long _, int playerSlot) in _frames.Keys)
    {
      if (playerSlot >= playerCount)
      {
        throw new InvalidDataException(
          $"The input script targets player slot {playerSlot}, but only {playerCount} players were created.");
      }
    }

    foreach (int playerSlot in _magicQuiverByPlayer.Keys)
    {
      if (playerSlot >= playerCount)
      {
        throw new InvalidDataException(
          $"The input script configures player slot {playerSlot}, but only {playerCount} players were created.");
      }
    }
  }

  public RuntimePlayerInput GetInput(long tick, int playerSlot)
  {
    return _frames.TryGetValue((tick, playerSlot), out RuntimePlayerInput input)
      ? input
      : default;
  }

  public bool HasMagicQuiver(int playerSlot)
  {
    return _magicQuiverByPlayer.TryGetValue(playerSlot, out bool enabled) && enabled;
  }

  private sealed class ScriptDocument
  {
    [JsonRequired]
    public List<ScriptFrame>? Frames { get; init; }

    public List<ScriptPlayer>? Players { get; init; }
  }

  private sealed class ScriptPlayer
  {
    [JsonRequired]
    public int PlayerSlot { get; init; }

    [JsonRequired]
    public bool MagicQuiver { get; init; }
  }

  private sealed class ScriptFrame
  {
    [JsonRequired]
    public long Tick { get; init; }

    [JsonRequired]
    public int PlayerSlot { get; init; }

    [JsonRequired]
    public int Horizontal { get; init; }

    [JsonRequired]
    public bool Jump { get; init; }

    [JsonRequired]
    public bool UseItem { get; init; }
  }
}
