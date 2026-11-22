namespace Terraria.Network;

public readonly record struct CommitVersion(long Value)
{
  public bool IsValid => Value > 0;

  public static CommitVersion Initial => new(1);

  public CommitVersion Next()
  {
    if (Value == long.MaxValue)
    {
      throw new InvalidOperationException("The commit version reached its maximum value.");
    }

    return new CommitVersion(Value + 1);
  }
}
