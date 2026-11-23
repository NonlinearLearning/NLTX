using Terraria.Lvcs;

namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Default <see cref="ILvcsLoader" />: read, parse, project.
/// </summary>
/// <remarks>
/// The loader is the single place where exceptions become diagnostics. Codecs are free to throw
/// — <see cref="LvcsContentException" /> for rejected content, anything else for defects — because
/// most external loaders signal failure that way and forcing every codec to adapt would spread
/// the same try/catch everywhere.
/// <para>
/// Parsing stops before projection as soon as one unit was rejected: projecting a partial batch
/// would build domain values from content already known to be broken and bury the diagnostics
/// that explain why.
/// </para>
/// </remarks>
public sealed class LvcsLoader : ILvcsLoader
{
  public LvcsLoadResult<TValue> Load<TRaw, TValue>(
      ILvcsCodec<TRaw, TValue> codec,
      ILvcsSource source)
      where TRaw : class
      where TValue : class
  {
    ArgumentNullException.ThrowIfNull(codec);
    ArgumentNullException.ThrowIfNull(source);

    if (!string.Equals(codec.Family.Value, source.Family.Value, StringComparison.Ordinal))
    {
      throw new ArgumentException(
          $"Source family '{source.Family}' does not match codec family '{codec.Family}'.",
          nameof(source));
    }

    LvcsDiagnosticSink diagnostics = new();
    List<TRaw> raws = [];
    bool rejected = false;

    foreach (ILvcsUnit unit in source.ReadUnits())
    {
      ArgumentNullException.ThrowIfNull(unit);
      int before = diagnostics.Items.Count;
      try
      {
        IReadOnlyList<TRaw> parsed = codec.Parse(unit, diagnostics);
        if (parsed.Count == 0)
        {
          diagnostics.Error(
              codec.Family,
              id: null,
              field: null,
              message: "A unit produced no entries.",
              source: unit.Location);
          rejected = true;
          continue;
        }

        raws.AddRange(parsed);
      }
      catch (LvcsContentException exception)
      {
        // A codec may explain the failure in detail before throwing, but it is not required to.
        // When it did not, the exception message becomes the diagnostic so no failure is silent.
        if (diagnostics.Items.Count == before)
        {
          diagnostics.Error(
              codec.Family,
              id: null,
              field: null,
              message: exception.Message,
              source: unit.Location);
        }

        rejected = true;
      }
      catch (Exception exception)
      {
        // An exception type the codec did not intend as a content verdict. The type name is kept
        // in the message so a defect is not mistaken for bad data.
        diagnostics.Error(
            codec.Family,
            id: null,
            field: null,
            message: $"{exception.GetType().Name}: {exception.Message}",
            source: unit.Location);
        rejected = true;
      }
    }

    if (rejected || diagnostics.HasErrors)
    {
      return LvcsLoadResult<TValue>.Failure(diagnostics.Items);
    }

    int beforeProjection = diagnostics.Items.Count;
    TValue value;
    try
    {
      value = codec.Project(raws, diagnostics);
    }
    catch (LvcsContentException exception)
    {
      if (diagnostics.Items.Count == beforeProjection)
      {
        diagnostics.Error(
            codec.Family,
            id: null,
            field: null,
            message: exception.Message);
      }

      return LvcsLoadResult<TValue>.Failure(diagnostics.Items);
    }
    catch (Exception exception)
    {
      diagnostics.Error(
          codec.Family,
          id: null,
          field: null,
          message: $"{exception.GetType().Name}: {exception.Message}");
      return LvcsLoadResult<TValue>.Failure(diagnostics.Items);
    }

    if (diagnostics.HasErrors)
    {
      return LvcsLoadResult<TValue>.Failure(diagnostics.Items);
    }

    return LvcsLoadResult<TValue>.Success(value, diagnostics.Items);
  }
}
