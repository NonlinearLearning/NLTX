namespace Terraria.WorldGeneration.Support;

public sealed class GenerationSupportWorkState
{
  public uint Revision { get; private set; }

  public bool IsActive { get; private set; }

  public void Begin(uint revision)
  {
    if (revision == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    if (IsActive)
    {
      throw new InvalidOperationException("Generation support work is already active.");
    }

    Revision = revision;
    IsActive = true;
  }

  public void Complete()
  {
    if (!IsActive)
    {
      throw new InvalidOperationException("Generation support work is not active.");
    }

    IsActive = false;
  }

  public void Clear()
  {
    Revision = 0;
    IsActive = false;
  }
}
