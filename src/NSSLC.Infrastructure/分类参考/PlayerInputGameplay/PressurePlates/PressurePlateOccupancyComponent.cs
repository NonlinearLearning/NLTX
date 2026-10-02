using System.Numerics;

namespace NLTX.PlayerInputGameplay.PressurePlates;

public sealed class PressurePlateOccupancyComponent
{
  private readonly Dictionary<PressurePlateCoordinate, bool[]> _pressedByPlate = new();
  private Vector2[] _playerLastPosition;

  public PressurePlateOccupancyComponent(int playerCapacity = 255)
  {
    if (playerCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    _playerLastPosition = new Vector2[playerCapacity];
  }

  public bool NeedsFirstUpdate { get; private set; } = true;

  public IReadOnlyDictionary<PressurePlateCoordinate, bool[]> PressedByPlate =>
    _pressedByPlate.ToDictionary(pair => pair.Key, pair => (bool[])pair.Value.Clone());

  public void ResizePlayers(int playerCapacity)
  {
    if (playerCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    _playerLastPosition = new Vector2[playerCapacity];
    foreach (var plate in _pressedByPlate.Keys.ToArray())
    {
      _pressedByPlate[plate] = new bool[playerCapacity];
    }
  }

  public bool IsPressed(PressurePlateCoordinate plate, int playerSlot)
  {
    ValidatePlayer(playerSlot);
    return _pressedByPlate.TryGetValue(plate, out var players) && players[playerSlot];
  }

  internal bool SetPressed(PressurePlateCoordinate plate, int playerSlot, bool pressed)
  {
    ValidatePlayer(playerSlot);
    if (!_pressedByPlate.TryGetValue(plate, out var players))
    {
      players = new bool[_playerLastPosition.Length];
      _pressedByPlate.Add(plate, players);
    }

    var changed = players[playerSlot] != pressed;
    players[playerSlot] = pressed;
    return changed;
  }

  internal void SetPreviousPosition(int playerSlot, Vector2 position)
  {
    ValidatePlayer(playerSlot);
    _playerLastPosition[playerSlot] = position;
    NeedsFirstUpdate = false;
  }

  internal Vector2 GetPreviousPosition(int playerSlot)
  {
    ValidatePlayer(playerSlot);
    return _playerLastPosition[playerSlot];
  }

  private void ValidatePlayer(int playerSlot)
  {
    if (playerSlot < 0 || playerSlot >= _playerLastPosition.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }
  }
}
