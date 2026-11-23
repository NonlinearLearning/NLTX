namespace Terraria.Lvcs;

/// <summary>
/// Stable identity of one content family. A family is one kind of domain value.
/// </summary>
/// <remarks>
/// The id is a plain string because families are open: a new family is added by writing a new
/// codec, not by editing an enum that every assembly must recompile.
/// </remarks>
public readonly record struct LvcsFamilyId
{
  public LvcsFamilyId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("A content family id is required.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public override string ToString() => Value;
}
