namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class DrawCommandWorksetComponent
{
  private readonly int _capacity;
  private readonly List<DrawCommand> _commands = new();
  private uint _nextLease;
  private uint _activeLease;

  public DrawCommandWorksetComponent(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _capacity = capacity;
  }

  public int Count => _commands.Count;

  public bool IsOpen { get; private set; }

  public uint Revision { get; private set; }

  internal uint BeginFrame()
  {
    if (IsOpen)
    {
      throw new InvalidOperationException("A draw frame is already open.");
    }

    _commands.Clear();
    _nextLease++;
    if (_nextLease == 0)
    {
      _nextLease = 1;
    }

    _activeLease = _nextLease;
    IsOpen = true;
    Revision++;
    return _activeLease;
  }

  internal void Add(DrawCommand command)
  {
    EnsureLease(command.FrameLease);
    if (_commands.Count >= _capacity)
    {
      throw new InvalidOperationException("The draw command workset capacity is exhausted.");
    }

    _commands.Add(command);
    Revision++;
  }

  internal IReadOnlyList<DrawCommand> Read(uint lease)
  {
    EnsureLease(lease);
    return _commands.ToArray();
  }

  internal void Clear(uint lease)
  {
    EnsureLease(lease);
    _commands.Clear();
    IsOpen = false;
    _activeLease = 0;
    Revision++;
  }

  private void EnsureLease(uint lease)
  {
    if (!IsOpen || lease == 0 || lease != _activeLease)
    {
      throw new InvalidOperationException("The draw frame lease is no longer valid.");
    }
  }
}
