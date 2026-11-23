using System.Text;
using NSSLC.Infrastructure.Lvcs;
using NSSLC.Server.Content.ItemUse;
using Terraria.Item;
using Terraria.Lvcs;

namespace Terraria.Server.Lvcs;

/// <summary>
/// Verifies the LVCS loading contract end to end, including the exception based failure path.
/// </summary>
/// <remarks>
/// Fixed concerns:
/// 1. A well formed batch loads, projects and seals.
/// 2. Rejected content is reported as diagnostics, and codecs may throw for that verdict.
/// 3. A foreign exception type is still converted, and its type name is preserved.
/// 4. Cross entry rules (duplicate id, ammo without value) are caught at projection.
/// 5. Family mismatch between source and codec is rejected.
/// 6. A fill-in-place external loader is adapted without an adapter class.
/// Verification type: contract + chain.
/// </remarks>
internal static class Program
{
  private static int _failures;

  private static int Main()
  {
    VerifyHappyPath();
    VerifyInvalidJson();
    VerifyDuplicateId();
    VerifyAmmoWithoutValue();
    VerifyFamilyMismatch();
    VerifyDiagnosticShape();
    VerifyForeignExceptionConverted();
    VerifyFillInPlaceLoader();
    VerifyRealContentFile();
    VerifyRegistryResolution();
    VerifyRegistryRejectsDuplicate();
    VerifyRegistryExtensionNarrowing();
    VerifyRegistryAmbiguity();
    VerifyRegistryIsOrderIndependent();
    VerifyDispatchLoadsRealContent();

    Console.WriteLine();
    if (_failures == 0)
    {
      Console.WriteLine("RESULT: PASS");
      return 0;
    }

    Console.WriteLine($"RESULT: FAIL ({_failures})");
    return 1;
  }

  private static void VerifyHappyPath()
  {
    string json = """
    [
      { "type": "itemUse", "id": "Torch", "useTime": 15, "useAnimation": 15,
        "autoReuse": true, "channel": false, "useAmmo": "None",
        "consumeAmmo": false, "manaCost": 0 },
      { "type": "itemUse", "id": "CopperBow", "useTime": 25, "useAnimation": 25,
        "autoReuse": false, "channel": false, "useAmmo": "WoodenArrow",
        "consumeAmmo": true, "manaCost": 0 }
    ]
    """;

    LvcsLoadResult<ItemUseDefinitionCatalog> result = Load(json);

    Check("happy-succeeded", result.Succeeded);
    Check("happy-count", result.Succeeded && result.Value!.Count == 2);
    Check("happy-sealed", result.Succeeded && result.Value!.IsSealed);
    Check("happy-lookup", result.Succeeded &&
        result.Value!.TryGet(ItemId.Torch, out ItemUseDefinition torch) &&
        torch.UseTime == 15);
    Check("happy-no-errors", result.Diagnostics.All(
        static d => d.Severity != LvcsDiagnosticSeverity.Error));
  }

  private static void VerifyInvalidJson()
  {
    LvcsLoadResult<ItemUseDefinitionCatalog> result = Load("{ not json");

    Check("invalid-succeeded-false", !result.Succeeded);
    Check("invalid-has-error", result.Diagnostics.Any(
        static d => d.Severity == LvcsDiagnosticSeverity.Error));
  }

  private static void VerifyDuplicateId()
  {
    string json = """
    [
      { "type": "itemUse", "id": "Torch", "useTime": 15, "useAnimation": 15,
        "autoReuse": true, "channel": false, "useAmmo": "None",
        "consumeAmmo": false, "manaCost": 0 },
      { "type": "itemUse", "id": "Torch", "useTime": 20, "useAnimation": 20,
        "autoReuse": false, "channel": false, "useAmmo": "None",
        "consumeAmmo": false, "manaCost": 0 }
    ]
    """;

    LvcsLoadResult<ItemUseDefinitionCatalog> result = Load(json);

    Check("duplicate-succeeded-false", !result.Succeeded);
    Check("duplicate-names-entry", result.Diagnostics.Any(
        static d => d.Id == "Torch" && d.Field == "Id"));
  }

  private static void VerifyAmmoWithoutValue()
  {
    string json = """
    [
      { "type": "itemUse", "id": "CopperBow", "useTime": 25, "useAnimation": 25,
        "autoReuse": false, "channel": false, "useAmmo": "None",
        "consumeAmmo": true, "manaCost": 0 }
    ]
    """;

    LvcsLoadResult<ItemUseDefinitionCatalog> result = Load(json);

    Check("ammo-succeeded-false", !result.Succeeded);
    Check("ammo-names-field", result.Diagnostics.Any(
        static d => d.Field == "UseAmmo"));
  }

  private static void VerifyFamilyMismatch()
  {
    bool threw = false;
    try
    {
      new LvcsLoader().Load(
          new ItemUseCodec(),
          new InMemorySource(new LvcsFamilyId("otherFamily"), "[]"));
    }
    catch (ArgumentException)
    {
      threw = true;
    }

    Check("mismatch-throws", threw);
  }

  private static void VerifyDiagnosticShape()
  {
    LvcsLoadResult<ItemUseDefinitionCatalog> result = Load("{ not json");
    LvcsDiagnostic? first = result.Diagnostics
        .Where(static d => d.Severity == LvcsDiagnosticSeverity.Error)
        .Select(static d => (LvcsDiagnostic?)d)
        .FirstOrDefault();

    Check("diag-family", first is not null && first.Value.Family.Value == "itemUse");
    Check("diag-source", first is not null && first.Value.Source == "unit.json");
    Check("diag-message", first is not null && first.Value.Message.Length > 0);
  }

  /// <summary>
  /// A codec that throws a foreign exception must still produce a diagnostic, and the type name
  /// must survive so a defect is not mistaken for bad content.
  /// </summary>
  private static void VerifyForeignExceptionConverted()
  {
    LvcsLoadResult<ProbeResult> result = new LvcsLoader().Load(
        new ThrowingCodec(),
        new InMemorySource(ThrowingCodec.FamilyId, "irrelevant"));

    Check("foreign-succeeded-false", !result.Succeeded);
    Check("foreign-keeps-type-name", result.Diagnostics.Any(
        static d => d.Message.Contains(
            nameof(InvalidOperationException), StringComparison.Ordinal)));
  }

  /// <summary>
  /// Mirrors a third party loader that fills a caller supplied object and throws on bad format.
  /// The codec calls it directly; the loader converts the throw into a diagnostic.
  /// </summary>
  private static void VerifyFillInPlaceLoader()
  {
    LvcsLoadResult<ProbeResult> ok = new LvcsLoader().Load(
        new FillInPlaceCodec(shouldThrow: false),
        new InMemorySource(FillInPlaceCodec.FamilyId, "good"));

    Check("fill-succeeded", ok.Succeeded);
    Check("fill-target-populated", ok.Succeeded && ok.Value!.Label == "populated");

    LvcsLoadResult<ProbeResult> bad = new LvcsLoader().Load(
        new FillInPlaceCodec(shouldThrow: true),
        new InMemorySource(FillInPlaceCodec.FamilyId, "bad"));

    Check("fill-failure-succeeded-false", !bad.Succeeded);
    Check("fill-failure-has-error", bad.Diagnostics.Any(
        static d => d.Severity == LvcsDiagnosticSeverity.Error));
  }

  /// <summary>
  /// Loads the real content file through the real file source, proving the pipeline works on
  /// disk and not only on in-memory strings.
  /// </summary>
  private static void VerifyRealContentFile()
  {
    string? directory = ResolveContentDirectory();
    if (directory is null)
    {
      Console.WriteLine("SKIP real-content (content directory not found)");
      return;
    }

    LvcsLoadResult<ItemUseDefinitionCatalog> result = new LvcsLoader().Load(
        new ItemUseCodec(),
        new FileLvcsSource(ItemUseCodec.FamilyId, directory, "*.json"));

    Check("real-succeeded", result.Succeeded);
    Check("real-sealed", result.Succeeded && result.Value!.IsSealed);
    Check("real-count", result.Succeeded && result.Value!.Count >= 1);
  }

  /// <summary>Registry resolves a unique type to its single codec.</summary>
  private static void VerifyRegistryResolution()
  {
    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ItemUseCodec());

    bool ok = registry.TryResolve<ItemUseDefinitionCatalog>(out var descriptor, out string? reason);
    Check("registry-resolve-ok", ok);
    Check("registry-resolve-reason-null", ok && reason is null);
    Check("registry-resolve-type", ok && descriptor!.ValueType == typeof(ItemUseDefinitionCatalog));
    Check("registry-resolve-family", ok && descriptor!.Family.Value == "itemUse");

    bool missing = registry.TryResolve<ProbeResult>(out var none, out string? missingReason);
    Check("registry-resolve-missing", !missing && none is null);
    Check("registry-resolve-missing-reason",
        missingReason is not null && missingReason.Contains("No codec", StringComparison.Ordinal));
  }

  /// <summary>Registering two codecs for the same (type, extension) pair is refused.</summary>
  private static void VerifyRegistryRejectsDuplicate()
  {
    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ItemUseCodec());

    bool threw = false;
    try
    {
      registry.Register(new ItemUseCodec());
    }
    catch (InvalidOperationException)
    {
      threw = true;
    }

    Check("registry-duplicate-throws", threw);
    Check("registry-duplicate-count", registry.Count == 1);
  }

  /// <summary>An extension narrows the candidates for one type.</summary>
  private static void VerifyRegistryExtensionNarrowing()
  {
    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ItemUseCodec());

    bool hit = registry.TryResolve<ItemUseDefinitionCatalog>(
        ".json", out var descriptor, out string? reason);
    Check("registry-ext-hit", hit && descriptor is not null && reason is null);

    bool miss = registry.TryResolve<ItemUseDefinitionCatalog>(
        ".yaml", out var none, out string? missReason);
    Check("registry-ext-miss", !miss && none is null);
    Check("registry-ext-miss-reason", missReason is not null &&
        missReason.Contains(".yaml", StringComparison.Ordinal));
  }

  /// <summary>Two codecs for one type are ambiguous until an extension is supplied.</summary>
  private static void VerifyRegistryAmbiguity()
  {
    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ProbeCodec(ProbeCodec.JsonFamily, [".json2"]));
    registry.Register(new ProbeCodec(ProbeCodec.BinaryFamily, [".bin"]));

    bool ambiguous = registry.TryResolve<ProbeResult>(out var none, out string? reason);
    Check("registry-ambiguous-type", !ambiguous && none is null);
    Check("registry-ambiguous-type-reason", reason is not null &&
        reason.Contains("specify an extension", StringComparison.Ordinal));

    bool narrowed = registry.TryResolve<ProbeResult>(
        ".json2", out var descriptor, out string? narrowReason);
    Check("registry-ambiguous-narrowed", narrowed && descriptor is not null && narrowReason is null);
    Check("registry-ambiguous-narrowed-family",
        narrowed && descriptor!.Family.Value == ProbeCodec.JsonFamily.Value);
  }

  /// <summary>
  /// Resolution must not depend on registration order. The same pair of codecs registered in the
  /// opposite order must produce the same verdicts.
  /// </summary>
  private static void VerifyRegistryIsOrderIndependent()
  {
    LvcsCodecRegistry forward = new(new LvcsLoader());
    forward.Register(new ProbeCodec(ProbeCodec.JsonFamily, [".a"]));
    forward.Register(new ProbeCodec(ProbeCodec.BinaryFamily, [".b"]));

    LvcsCodecRegistry backward = new(new LvcsLoader());
    backward.Register(new ProbeCodec(ProbeCodec.BinaryFamily, [".b"]));
    backward.Register(new ProbeCodec(ProbeCodec.JsonFamily, [".a"]));

    bool forwardA = forward.TryResolve<ProbeResult>(".a", out var fa, out _);
    bool backwardA = backward.TryResolve<ProbeResult>(".a", out var ba, out _);
    Check("registry-order-a", forwardA && backwardA);
    Check("registry-order-a-same-family",
        fa!.Family.Value == ba!.Family.Value);

    bool forwardB = forward.TryResolve<ProbeResult>(".b", out var fb, out _);
    bool backwardB = backward.TryResolve<ProbeResult>(".b", out var bb, out _);
    Check("registry-order-b", forwardB && backwardB);
    Check("registry-order-b-same-family",
        fb!.Family.Value == bb!.Family.Value);

    bool forwardAmbiguous = forward.TryResolve<ProbeResult>(out _, out _);
    bool backwardAmbiguous = backward.TryResolve<ProbeResult>(out _, out _);
    Check("registry-order-ambiguous-both-fail",
        !forwardAmbiguous && !backwardAmbiguous);
  }

  /// <summary>
  /// End to end through dispatch: resolve by type, load the real file, get a sealed catalog.
  /// This is the path the server uses at startup.
  /// </summary>
  private static void VerifyDispatchLoadsRealContent()
  {
    string? directory = ResolveContentDirectory();
    if (directory is null)
    {
      Console.WriteLine("SKIP dispatch-real (content directory not found)");
      return;
    }

    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ItemUseCodec());

    bool resolved = registry.TryResolve<ItemUseDefinitionCatalog>(
        ".json", out ILvcsCodecDescriptor? descriptor, out string? reason);
    Check("dispatch-resolved", resolved && reason is null);

    object? loaded = descriptor!.Load(
        new FileLvcsSource(descriptor.Family, directory, "*.json"),
        out IReadOnlyList<LvcsDiagnostic> diagnostics);
    Check("dispatch-loaded-type", loaded is ItemUseDefinitionCatalog);
    Check("dispatch-loaded-sealed",
        loaded is ItemUseDefinitionCatalog catalog && catalog.IsSealed);
    Check("dispatch-loaded-count",
        loaded is ItemUseDefinitionCatalog counted && counted.Count >= 1);
    Check("dispatch-no-errors", diagnostics.All(
        static d => d.Severity != LvcsDiagnosticSeverity.Error));
  }

  private static string? ResolveContentDirectory()
  {
    DirectoryInfo? current = new(AppContext.BaseDirectory);
    while (current is not null)
    {
      string candidate = Path.Combine(
          current.FullName, "src", "NSSLC.Server", "Content", "Prototypes", "ItemUse");
      if (Directory.Exists(candidate))
      {
        return candidate;
      }

      current = current.Parent;
    }

    return null;
  }

  private static LvcsLoadResult<ItemUseDefinitionCatalog> Load(string json)
    => new LvcsLoader().Load(
        new ItemUseCodec(),
        new InMemorySource(ItemUseCodec.FamilyId, json));

  private static void Check(string name, bool condition)
  {
    if (condition)
    {
      Console.WriteLine($"PASS {name}");
      return;
    }

    Console.WriteLine($"FAIL {name}");
    _failures++;
  }

  /// <summary>In-memory source: the only thing this verification needs from I/O.</summary>
  private sealed class InMemorySource : ILvcsSource
  {
    private readonly string _text;

    public InMemorySource(LvcsFamilyId family, string text)
    {
      Family = family;
      _text = text;
    }

    public LvcsFamilyId Family { get; }

    public IEnumerable<ILvcsUnit> ReadUnits()
    {
      yield return new LvcsUnit("unit.json", Encoding.UTF8.GetBytes(_text));
    }
  }

  /// <summary>Minimal projected value used by the probe codecs.</summary>
  private sealed class ProbeResult
  {
    public ProbeResult(string label) => Label = label;

    public string Label { get; }
  }

  /// <summary>Codec that always throws a non content exception.</summary>
  private sealed class ThrowingCodec : ILvcsCodec<ProbeRaw, ProbeResult>
  {
    public static readonly LvcsFamilyId FamilyId = new("probeThrow");

    public LvcsFamilyId Family => FamilyId;

    public IReadOnlyList<string> Extensions { get; } = [".probe"];

    public IReadOnlyList<ProbeRaw> Parse(ILvcsUnit unit, LvcsDiagnosticSink diagnostics)
      => throw new InvalidOperationException("a defect, not bad content");

    public ProbeResult Project(IReadOnlyList<ProbeRaw> raws, LvcsDiagnosticSink diagnostics)
      => new("unreachable");
  }

  /// <summary>
  /// Codec shaped like a third party loader: it creates the target, hands it to a fill routine and
  /// lets the routine throw when the payload is rejected. No try/catch of its own.
  /// </summary>
  private sealed class FillInPlaceCodec : ILvcsCodec<ProbeRaw, ProbeResult>
  {
    public static readonly LvcsFamilyId FamilyId = new("probeFill");
    private readonly bool _shouldThrow;

    public FillInPlaceCodec(bool shouldThrow) => _shouldThrow = shouldThrow;

    public LvcsFamilyId Family => FamilyId;

    public IReadOnlyList<string> Extensions { get; } = [".fill"];

    public IReadOnlyList<ProbeRaw> Parse(ILvcsUnit unit, LvcsDiagnosticSink diagnostics)
    {
      ProbeRaw target = new();
      if (_shouldThrow)
      {
        throw new LvcsContentException("payload rejected by the external loader");
      }

      target.Label = "populated";
      return [target];
    }

    public ProbeResult Project(IReadOnlyList<ProbeRaw> raws, LvcsDiagnosticSink diagnostics)
      => new(raws[0].Label);
  }

  /// <summary>Mutable fill target, as an external loader would expect.</summary>
  private sealed class ProbeRaw
  {
    public string Label { get; set; } = string.Empty;
  }

  /// <summary>
  /// Codec used to exercise dispatch. It declares its own family and extensions so tests can
  /// build registry scenarios without touching real content.
  /// </summary>
  private sealed class ProbeCodec : ILvcsCodec<ProbeRaw, ProbeResult>
  {
    public static readonly LvcsFamilyId JsonFamily = new("probeJson");
    public static readonly LvcsFamilyId BinaryFamily = new("probeBinary");

    private readonly LvcsFamilyId _family;
    private readonly IReadOnlyList<string> _extensions;

    public ProbeCodec(LvcsFamilyId family, IReadOnlyList<string> extensions)
    {
      _family = family;
      _extensions = extensions;
    }

    public LvcsFamilyId Family => _family;

    public IReadOnlyList<string> Extensions => _extensions;

    public IReadOnlyList<ProbeRaw> Parse(ILvcsUnit unit, LvcsDiagnosticSink diagnostics)
      => [new ProbeRaw { Label = _family.Value }];

    public ProbeResult Project(IReadOnlyList<ProbeRaw> raws, LvcsDiagnosticSink diagnostics)
      => new(raws[0].Label);
  }
}
