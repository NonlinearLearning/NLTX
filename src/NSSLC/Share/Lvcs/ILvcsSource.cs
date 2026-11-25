namespace Terraria.Share.Lvcs;

/// <summary>
/// Supplies the raw units of one content family.
/// </summary>
/// <remarks>
/// This is the only I/O-shaped contract in the domain layer, and it carries no file system
/// types. Implementations live in infrastructure. Multiple sources may be read in order; the
/// loader does not define override semantics, a codec that needs merging performs it in
/// <see cref="ILvcsCodec{TRaw,TValue}.Project" />.
/// </remarks>
public interface ILvcsSource
{
  /// <summary>Family this source provides.</summary>
  LvcsFamilyId Family { get; }

  /// <summary>
  /// All units of the family. Order is not a semantic input: a codec that depends on order
  /// must express that dependency explicitly.
  /// </summary>
  IEnumerable<ILvcsUnit> ReadUnits();
}
