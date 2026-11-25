using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSSLC.Infrastructure.Lvcs;
using Terraria.Unshared.Projectiles;

namespace NSSLC.Server.Content.Projectile;

/// <summary>
/// Loads projectile content entries from JSON files into an immutable catalog.
/// </summary>
/// <remarks>
/// 本类型不认识任何内容族：族字段的校验与转换都在 <see cref="IProjectileSectionReader" /> 里。
/// 它只负责三件与"批次"有关、单个条目自己回答不了的事——反序列化、逐条必填项（id / type）、
/// 重复 id 判定。新增一个内容族因此不需要改这里，只需新增一个读取器并登记到
/// <see cref="ProjectileSectionReaders" />。
/// <para>
/// Detail goes to the diagnostic sink; failure is signalled by throwing
/// <see cref="LvcsContentException" /> so that the loader owns the single conversion point.
/// </para>
/// </remarks>
internal sealed class ProjectileCodec : ILvcsCodec<ProjectilePrototype, ProjectileCatalog>
{
  /// <summary>Family id used by the projectile content family.</summary>
  public static readonly LvcsFamilyId FamilyId = new("projectile");

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
  };

  private readonly IProjectileSectionReader[] _readers;

  public ProjectileCodec(IEnumerable<IProjectileSectionReader> readers)
  {
    ArgumentNullException.ThrowIfNull(readers);
    _readers = [.. readers];
    if (_readers.Length == 0)
    {
      throw new ArgumentException(
          "A projectile codec needs at least one section reader.", nameof(readers));
    }
  }

  public LvcsFamilyId Family => FamilyId;

  public IReadOnlyList<string> Extensions { get; } = [".json"];

  public IReadOnlyList<ProjectilePrototype> Parse(
      ILvcsUnit unit,
      LvcsDiagnosticSink diagnostics)
  {
    ArgumentNullException.ThrowIfNull(unit);
    ArgumentNullException.ThrowIfNull(diagnostics);

    List<ProjectilePrototype>? prototypes;
    try
    {
      prototypes = JsonSerializer.Deserialize<List<ProjectilePrototype>>(
          unit.Payload.Span, JsonOptions);
    }
    catch (JsonException exception)
    {
      throw new LvcsContentException(
          $"Could not parse projectile JSON: {exception.Message}", exception);
    }

    if (prototypes is null || prototypes.Count == 0)
    {
      throw new LvcsContentException("File contains no projectile entries.");
    }

    List<ProjectilePrototype> accepted = [];
    bool rejected = false;
    for (int index = 0; index < prototypes.Count; index++)
    {
      ProjectilePrototype? prototype = prototypes[index];
      if (prototype is null)
      {
        diagnostics.Error(
            Family, id: null, field: null,
            message: $"Null entry at index {index}.", source: unit.Location);
        rejected = true;
        continue;
      }

      if (IsEntryValid(prototype, unit.Location, diagnostics))
      {
        accepted.Add(prototype);
      }
      else
      {
        rejected = true;
      }
    }

    if (rejected)
    {
      throw new LvcsContentException("One or more projectile entries were rejected.");
    }

    return accepted;
  }

  public ProjectileCatalog Project(
      IReadOnlyList<ProjectilePrototype> raws,
      LvcsDiagnosticSink diagnostics)
  {
    ArgumentNullException.ThrowIfNull(raws);
    ArgumentNullException.ThrowIfNull(diagnostics);

    ProjectileCatalog catalog = new();
    HashSet<ProjectileId> seen = [];
    bool rejected = false;

    foreach (ProjectilePrototype prototype in raws)
    {
      ProjectileId definitionId = new(prototype.Id);
      if (!seen.Add(definitionId))
      {
        diagnostics.Error(
            Family, prototype.Id, nameof(prototype.Id),
            "Projectile entry is declared twice.");
        rejected = true;
        continue;
      }

      try
      {
        // 读取器按登记顺序各给一个 section；族缺席时返回 null，因此这里不判断族名。
        List<IProjectileSection> sections = [];
        foreach (IProjectileSectionReader reader in _readers)
        {
          IProjectileSection? section = reader.Read(prototype);
          if (section is not null)
          {
            sections.Add(section);
          }
        }

        catalog.Register(new ProjectileRecipe(definitionId, sections));
      }
      catch (Exception exception) when (
          exception is ArgumentException or OverflowException or InvalidOperationException)
      {
        diagnostics.Error(
            Family, prototype.Id, field: null, message: exception.Message);
        rejected = true;
      }
    }

    if (rejected)
    {
      throw new LvcsContentException("One or more projectile entries were rejected.");
    }

    if (catalog.Count == 0)
    {
      throw new LvcsContentException("Family defines no entries.");
    }

    catalog.Seal();
    return catalog;
  }

  /// <summary>只校验条目级必填项，其余交给各内容族读取器。</summary>
  private bool IsEntryValid(
      ProjectilePrototype prototype,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (string.IsNullOrWhiteSpace(prototype.Id))
    {
      diagnostics.Error(
          Family, id: null, field: nameof(prototype.Id),
          message: "Entry has an empty id.", source: location);
      return false;
    }

    if (!string.Equals(prototype.Type, FamilyId.Value, StringComparison.Ordinal))
    {
      diagnostics.Error(
          Family,
          prototype.Id,
          nameof(prototype.Type),
          message: $"Entry has type '{prototype.Type}' but expected '{FamilyId.Value}'.",
          source: location);
      return false;
    }

    foreach (IProjectileSectionReader reader in _readers)
    {
      if (!reader.Validate(prototype, FamilyId, location, diagnostics))
      {
        return false;
      }
    }

    return true;
  }
}
