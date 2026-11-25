using System.Text;
using NSSLC.Infrastructure.Lvcs;
using NSSLC.Server.Content.ItemUse;
using NSSLC.Server.Content.Projectile;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Items;
using Terraria.Share.Lvcs;
using Terraria.Unshared.Projectiles;

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
    VerifyShootProjection();
    VerifyShootRejectsZeroVelocity();
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
    VerifyProjectileContent();
    VerifyProjectileRejectsBadEntries();
    VerifyProjectileRegistryDispatch();
    VerifySectionFamilyRegistration();

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

    LvcsLoadResult<ItemUseCatalog> result = Load(json);

    Check("happy-succeeded", result.Succeeded);
    Check("happy-count", result.Succeeded && result.Value!.Count == 2);
    Check("happy-sealed", result.Succeeded && result.Value!.IsSealed);
    Check("happy-lookup", result.Succeeded &&
        result.Value!.TryGet(ItemId.Torch, out ItemUse torch) &&
        torch.UseTime == 15);
    Check("happy-no-errors", result.Diagnostics.All(
        static d => d.Severity != LvcsDiagnosticSeverity.Error));
  }

  private static void VerifyInvalidJson()
  {
    LvcsLoadResult<ItemUseCatalog> result = Load("{ not json");

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

    LvcsLoadResult<ItemUseCatalog> result = Load(json);

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

    LvcsLoadResult<ItemUseCatalog> result = Load(json);

    Check("ammo-succeeded-false", !result.Succeeded);
    Check("ammo-names-field", result.Diagnostics.Any(
        static d => d.Field == "UseAmmo"));
  }

  /// <summary>
  /// shoot 块被投影到 <see cref="ItemShootProfile" />：投射物 id、速度、偏移都来自内容，
  /// 没声明 shoot 的物品保持 null（它只是"可以按"，不会发射）。
  /// </summary>
  private static void VerifyShootProjection()
  {
    string json = """
    [
      { "type": "itemUse", "id": "CopperBow", "useTime": 25, "useAnimation": 25,
        "autoReuse": false, "channel": false, "useAmmo": "WoodenArrow",
        "consumeAmmo": true, "manaCost": 0,
        "shoot": { "projectileId": "wooden-arrow", "velocityX": 8, "velocityY": 0,
                   "offsetX": 8, "offsetY": -8 } },
      { "type": "itemUse", "id": "Torch", "useTime": 15, "useAnimation": 15,
        "autoReuse": true, "channel": false, "useAmmo": "None",
        "consumeAmmo": false, "manaCost": 0 }
    ]
    """;

    LvcsLoadResult<ItemUseCatalog> result = Load(json);

    Check("shoot-projected", result.Succeeded &&
        result.Value!.TryGet(ItemId.CopperBow, out ItemUse bow) &&
        bow.Shoot is ItemShootProfile shoot &&
        shoot.ProjectileId == "wooden-arrow" &&
        shoot.Velocity == new System.Numerics.Vector2(8f, 0f) &&
        shoot.Offset == new System.Numerics.Vector2(8f, -8f));
    Check("shoot-absent-when-not-declared", result.Succeeded &&
        result.Value!.TryGet(ItemId.Torch, out ItemUse torch) &&
        torch.Shoot is null);
  }

  /// <summary>零速度的射击没有方向语义，必须在加载期拒绝。</summary>
  private static void VerifyShootRejectsZeroVelocity()
  {
    string json = """
    [
      { "type": "itemUse", "id": "CopperBow", "useTime": 25, "useAnimation": 25,
        "autoReuse": false, "channel": false, "useAmmo": "WoodenArrow",
        "consumeAmmo": true, "manaCost": 0,
        "shoot": { "projectileId": "wooden-arrow", "velocityX": 0, "velocityY": 0,
                   "offsetX": 0, "offsetY": 0 } }
    ]
    """;

    LvcsLoadResult<ItemUseCatalog> result = Load(json);

    Check("shoot-zero-velocity-rejected", !result.Succeeded);
    Check("shoot-zero-velocity-names-field", result.Diagnostics.Any(
        static d => d.Field == "Shoot"));
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
    LvcsLoadResult<ItemUseCatalog> result = Load("{ not json");
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

    LvcsLoadResult<ItemUseCatalog> result = new LvcsLoader().Load(
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

    bool ok = registry.TryResolve<ItemUseCatalog>(out var descriptor, out string? reason);
    Check("registry-resolve-ok", ok);
    Check("registry-resolve-reason-null", ok && reason is null);
    Check("registry-resolve-type", ok && descriptor!.ValueType == typeof(ItemUseCatalog));
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

    bool hit = registry.TryResolve<ItemUseCatalog>(
        ".json", out var descriptor, out string? reason);
    Check("registry-ext-hit", hit && descriptor is not null && reason is null);

    bool miss = registry.TryResolve<ItemUseCatalog>(
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

    bool resolved = registry.TryResolve<ItemUseCatalog>(
        ".json", out ILvcsCodecDescriptor? descriptor, out string? reason);
    Check("dispatch-resolved", resolved && reason is null);

    object? loaded = descriptor!.Load(
        new FileLvcsSource(descriptor.Family, directory, "*.json"),
        out IReadOnlyList<LvcsDiagnostic> diagnostics);
    Check("dispatch-loaded-type", loaded is ItemUseCatalog);
    Check("dispatch-loaded-sealed",
        loaded is ItemUseCatalog catalog && catalog.IsSealed);
    Check("dispatch-loaded-count",
        loaded is ItemUseCatalog counted && counted.Count >= 1);
    Check("dispatch-no-errors", diagnostics.All(
        static d => d.Severity != LvcsDiagnosticSeverity.Error));
    Check("dispatch-real-content-carries-shoot",
        loaded is ItemUseCatalog content &&
        content.TryGet(ItemId.CopperBow, out ItemUse bow) &&
        bow.Shoot is ItemShootProfile declared &&
        declared.ProjectileId == "wooden-arrow");
  }

  /// <summary>
  /// Real projectile content through the real file source: three entries load, project and seal,
  /// and every definition keeps the policy values the JSON declared.
  /// </summary>
  private static void VerifyProjectileContent()
  {
    string? directory = ResolveContentDirectory("Projectile");
    if (directory is null)
    {
      Console.WriteLine("SKIP projectile-content (content directory not found)");
      return;
    }

    LvcsLoadResult<ProjectileCatalog> result = new LvcsLoader().Load(
        new ProjectileCodec(ProjectileSectionReaders.Create()),
        new FileLvcsSource(ProjectileCodec.FamilyId, directory, "*.json"));

    Check("projectile-catalog-succeeded", result.Succeeded);
    Check("projectile-catalog-count", result.Succeeded && result.Value!.Count == 4);
    Check("projectile-catalog-sealed", result.Succeeded && result.Value!.IsSealed);
    if (!result.Succeeded)
    {
      return;
    }

    // 内容只以「配方 + 内容族 section」的形式存在：断言读 section 上的标量，
    // 不再经融合聚合或策略值对象转述（P2/P3 删除了这两层）。
    ProjectileCatalog catalog = result.Value!;
    bool arrowFound = catalog.TryGet(new ProjectileId("wooden-arrow"), out ProjectileRecipe arrow);
    Check("projectile-arrow-found", arrowFound);
    Check(
        "projectile-arrow-straight-and-single-hit",
        arrowFound &&
            arrow.TryGetSection(out ProjectileBasicsSection arrowBasics) &&
            arrowBasics.BehaviorId == new Terraria.Share.Ai.AiBehaviorId("Straight") &&
            arrow.TryGetSection(out ProjectilePenetrationSection arrowPenetration) &&
            arrowPenetration.MaximumHits == 1 &&
            !arrow.TryGetSection(out ProjectileBounceSection _) &&
            !arrow.TryGetSection(out ProjectileHomingSection _) &&
            arrowBasics.DamageType == DamageType.Ranged &&
            arrowBasics.BaseDamage == 5 &&
            arrowBasics.AttackDefinitionId == new CombatAttackId("projectile.wooden-arrow"));
    Check(
        "projectile-arrow-gravity-disabled",
        arrowFound &&
            arrow.TryGetSection(out ProjectileGravitySection arrowGravity) &&
            !arrowGravity.Enabled);
    // P3 起穿透上限只有预算组件一个来源，属性侧不再写入：断言改为钉住 section 值。
    Check(
        "projectile-arrow-penetration-budget-value",
        arrowFound &&
            arrow.TryGetSection(out ProjectilePenetrationSection arrowBudget) &&
            arrowBudget.MaximumHits == 1 &&
            !arrowBudget.IsUnlimited);

    bool sparkFound = catalog.TryGet(new ProjectileId("homing-spark"), out ProjectileRecipe spark);
    Check(
        "projectile-spark-homing",
        sparkFound &&
            spark.TryGetSection(out ProjectileHomingSection sparkHoming) &&
            sparkHoming.TurnRate == 6f &&
            sparkHoming.Range == 320f &&
            spark.TryGetSection(out ProjectilePenetrationSection sparkPenetration) &&
            sparkPenetration.MaximumHits == 3);

    bool orbFound = catalog.TryGet(new ProjectileId("bouncer-orb"), out ProjectileRecipe orb);
    Check(
        "projectile-orb-bounce",
        orbFound &&
            orb.TryGetSection(out ProjectileBounceSection orbBounce) &&
            orbBounce.MaximumBounces == 5 &&
            orbBounce.PenetrationCost == 1 &&
            orbBounce.SpeedScale == 0.8f &&
            orbBounce.ConsumeOrder == ProjectileBounceConsumeOrder.BeforeBounce &&
            orb.TryGetSection(out ProjectileBasicsSection orbBasics) &&
            orbBasics.ExtraUpdates == 1);

    Check("projectile-unknown-definition", !catalog.TryGet(new ProjectileId("missing"), out _));
  }

  /// <summary>Single-entry validation catches bad content before anything is registered.</summary>
  private static void VerifyProjectileRejectsBadEntries()
  {
    string unknownDamageType = ProjectileEntry.Replace(
        "damageType\": \"Generic", "damageType\": \"Nope", StringComparison.Ordinal);
    Check(
        "projectile-rejects-unknown-damage-type",
        FailsWithField(unknownDamageType, "DamageType"));

    string missingCollider = ProjectileEntry.Replace(
        "\"collider\": { \"width\": 8, \"height\": 8 },", string.Empty, StringComparison.Ordinal);
    Check(
        "projectile-rejects-missing-collider",
        FailsWithField(missingCollider, "Collider"));

    string unknownConsumeOrder = ProjectileEntry.Replace(
        "penetration\": { \"maximumHits\": 1 },",
        "penetration\": { \"maximumHits\": 1 },\n    \"bounce\": { \"maximumBounces\": 2, " +
            "\"penetrationCost\": 1, \"speedScale\": 0.5, \"consumeOrder\": \"Sideways\" },",
        StringComparison.Ordinal);
    Check(
        "projectile-rejects-unknown-consume-order",
        FailsWithField(unknownConsumeOrder, "Bounce"));

    Check(
        "projectile-rejects-duplicate-id",
        !LoadProjectile("[" + ProjectileEntry + "," + ProjectileEntry + "]").Succeeded);
    Check(
        "projectile-accepts-minimal-entry",
        LoadProjectile("[" + ProjectileEntry + "]").Succeeded);
  }

  /// <summary>Dispatch resolves the projectile family by produced type and extension.</summary>
  private static void VerifyProjectileRegistryDispatch()
  {
    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ProjectileCodec(ProjectileSectionReaders.Create()));

    bool ok = registry.TryResolve<ProjectileCatalog>(
        ".json", out ILvcsCodecDescriptor? descriptor, out string? reason);
    Check("projectile-registry-resolve", ok);
    Check("projectile-registry-family", ok && descriptor!.Family.Value == "projectile");
    Check("projectile-registry-reason-null", ok && reason is null);
  }

  private const string ProjectileEntry = """
  {
    "type": "projectile",
    "id": "probe",
    "behaviorId": "Straight",
    "profileId": "probe.default",
    "extraUpdates": 0,
    "lifetimeTicks": 60,
    "collider": { "width": 8, "height": 8 },
    "tileCollide": true,
    "ignoreOwner": true,
    "penetration": { "maximumHits": 1 },
    "attackId": "probe.attack",
    "damageType": "Generic",
    "baseDamage": 1
  }
  """;

  private static LvcsLoadResult<ProjectileCatalog> LoadProjectile(string json)
    => new LvcsLoader().Load(
        new ProjectileCodec(ProjectileSectionReaders.Create()),
        new InMemorySource(ProjectileCodec.FamilyId, json));

  private static bool FailsWithField(string entry, string field)
  {
    LvcsLoadResult<ProjectileCatalog> result = LoadProjectile("[" + entry + "]");

    return !result.Succeeded && result.Diagnostics.Any(
        diagnostic => diagnostic.Field == field &&
            diagnostic.Severity == LvcsDiagnosticSeverity.Error);
  }

  /// <summary>
  /// 判据 5：新增一个内容族不需要改 codec —— 只需要一个 section 加一个读取器。
  /// 这里用一个只存在于验证项目里的"探针族"证明：把它登记进读取器列表后，真实内容文件
  /// 仍然加载成功，且每条配方上都多出这个族的 section。
  /// </summary>
  private static void VerifySectionFamilyRegistration()
  {
    string? directory = ResolveContentDirectory("Projectile");
    if (directory is null)
    {
      Console.WriteLine("SKIP section-family-added-without-codec-edit (content directory not found)");
      return;
    }

    List<IProjectileSectionReader> readers =
        [.. ProjectileSectionReaders.Create(), new ProbeFamilyReader()];
    LvcsLoadResult<ProjectileCatalog> result = new LvcsLoader().Load(
        new ProjectileCodec(readers),
        new FileLvcsSource(ProjectileCodec.FamilyId, directory, "*.json"));

    bool probeOnEveryRecipe = result.Succeeded && result.Value!.Recipes.All(
        recipe => recipe.TryGetSection(out ProbeFamilySection probe) && probe.Label == "probe");
    Check(
        "section-family-added-without-codec-edit",
        result.Succeeded && result.Value!.Count == 4 && probeOnEveryRecipe);
  }

  /// <summary>只存在于验证项目里的内容族：带一个可断言的标签，不贡献任何组件。</summary>
  private sealed class ProbeFamilySection : IProjectileSection
  {
    public ProbeFamilySection(string label) => Label = label;

    public string Label { get; }

    public void Collect(
        in ProjectileSectionContext context,
        ProjectileCompositionParameters parameters)
    {
      // 探针族不写参数块：写任何键都会与真实内容族重复声明。
    }

    public void Contribute(
        in ProjectileSectionContext context,
        ProjectileCompositionParameters parameters,
        EntityBuilder builder)
    {
      // 也不贡献组件：本条用例要证明的是"新族接得进来"，不是"新族能加组件"。
    }
  }

  /// <summary>探针族的读取器：一个读取器就够，不需要碰 codec。</summary>
  private sealed class ProbeFamilyReader : IProjectileSectionReader
  {
    public string Family => "probe";

    public bool Validate(
        ProjectilePrototype prototype,
        LvcsFamilyId familyId,
        string location,
        LvcsDiagnosticSink diagnostics) => true;

    public IProjectileSection Read(ProjectilePrototype prototype) =>
        new ProbeFamilySection("probe");
  }

  private static string? ResolveContentDirectory() => ResolveContentDirectory("ItemUse");

  private static string? ResolveContentDirectory(string familyDirectory)
  {
    DirectoryInfo? current = new(AppContext.BaseDirectory);
    while (current is not null)
    {
      string candidate = Path.Combine(
          current.FullName, "src", "NSSLC.Server", "Content", "Prototypes", familyDirectory);
      if (Directory.Exists(candidate))
      {
        return candidate;
      }

      current = current.Parent;
    }

    return null;
  }

  private static LvcsLoadResult<ItemUseCatalog> Load(string json)
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
