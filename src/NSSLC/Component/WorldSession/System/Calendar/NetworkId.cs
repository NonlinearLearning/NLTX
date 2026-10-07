namespace Terraria.WorldSession.Calendar;

// Provisional boundary type; the final owner remains integration-review.
public readonly record struct NetworkId(int Value)
{
  public bool IsEmpty => Value == 0;
}
