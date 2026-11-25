using Terraria.Share.Lvcs;

namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Default <see cref="ILvcsCodecRegistry" />: type first, extension as narrowing, no ambiguity.
/// </summary>
/// <remarks>
/// Two deliberate departures from the asset systems this is modelled on:
/// <list type="number">
/// <item>
/// Registration fails on an overlapping (type, extension) pair instead of silently keeping the
/// newest loader. Silent shadowing would make the effective codec depend on the order assemblies
/// happen to register, which the project forbids for scheduling and loading alike.
/// </item>
/// <item>
/// Lookup returns the reason for failure. "No codec produces this type" and "two codecs produce
/// it" need different fixes, and a null alone cannot express that.
/// </item>
/// </list>
/// </remarks>
public sealed class LvcsCodecRegistry : ILvcsCodecRegistry
{
  private readonly List<ILvcsCodecDescriptor> _descriptors = [];
  private readonly ILvcsLoader _loader;

  public LvcsCodecRegistry(ILvcsLoader loader)
  {
    ArgumentNullException.ThrowIfNull(loader);
    _loader = loader;
  }

  public int Count => _descriptors.Count;

  public void Register<TRaw, TValue>(ILvcsCodec<TRaw, TValue> codec)
      where TRaw : class
      where TValue : class
  {
    ArgumentNullException.ThrowIfNull(codec);

    LvcsCodecDescriptor<TRaw, TValue> descriptor = new(codec, _loader);
    if (descriptor.Extensions.Count == 0)
    {
      throw new InvalidOperationException(
          $"Codec for {typeof(TValue).Name} declares no extensions; it could never be selected.");
    }

    foreach (ILvcsCodecDescriptor existing in _descriptors)
    {
      if (existing.ValueType != descriptor.ValueType)
      {
        continue;
      }

      foreach (string extension in descriptor.Extensions)
      {
        if (existing.Handles(extension))
        {
          throw new InvalidOperationException(
              $"A codec producing {descriptor.ValueType.Name} already handles '{extension}'. " +
              "Ambiguity is refused rather than resolved by registration order.");
        }
      }
    }

    _descriptors.Add(descriptor);
  }

  public bool TryResolve<TValue>(out ILvcsCodecDescriptor? descriptor, out string? reason)
      where TValue : class
  {
    List<ILvcsCodecDescriptor> candidates = Candidates(typeof(TValue));
    if (candidates.Count == 1)
    {
      descriptor = candidates[0];
      reason = null;
      return true;
    }

    descriptor = null;
    reason = candidates.Count == 0
        ? $"No codec produces {typeof(TValue).Name}."
        : $"{candidates.Count} codecs produce {typeof(TValue).Name}; " +
          "specify an extension to disambiguate.";
    return false;
  }

  public bool TryResolve<TValue>(
      string extension,
      out ILvcsCodecDescriptor? descriptor,
      out string? reason)
      where TValue : class
  {
    if (string.IsNullOrWhiteSpace(extension))
    {
      throw new ArgumentException("An extension is required.", nameof(extension));
    }

    List<ILvcsCodecDescriptor> candidates = Candidates(typeof(TValue));
    if (candidates.Count == 0)
    {
      descriptor = null;
      reason = $"No codec produces {typeof(TValue).Name}.";
      return false;
    }

    List<ILvcsCodecDescriptor> matching = [];
    foreach (ILvcsCodecDescriptor candidate in candidates)
    {
      if (candidate.Handles(extension))
      {
        matching.Add(candidate);
      }
    }

    if (matching.Count == 1)
    {
      descriptor = matching[0];
      reason = null;
      return true;
    }

    descriptor = null;
    reason = matching.Count == 0
        ? $"No codec producing {typeof(TValue).Name} reads '{extension}'."
        : $"{matching.Count} codecs producing {typeof(TValue).Name} read '{extension}'.";
    return false;
  }

  private List<ILvcsCodecDescriptor> Candidates(Type valueType)
  {
    List<ILvcsCodecDescriptor> candidates = [];
    foreach (ILvcsCodecDescriptor descriptor in _descriptors)
    {
      if (descriptor.ValueType == valueType)
      {
        candidates.Add(descriptor);
      }
    }

    return candidates;
  }
}
