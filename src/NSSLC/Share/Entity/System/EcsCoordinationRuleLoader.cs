using System;
using System.Collections.Generic;
using System.Globalization;

namespace Terraria.Entity;

/// <summary>
/// Parses one collaboration-rule document into an <see cref="EcsCoordinationRuleSet"/>.
/// </summary>
/// <remarks>
/// The format is line oriented so a host can load it from a file, an embedded resource or a
/// literal without taking a serialization dependency:
/// <code>
/// # comment
/// before  combat.projectile.damage-result  combat.projectile.tile-resolve
/// phase   Commit  combat.projectile.tile-resolve
/// requires-dependency  combat.projectile.tile-resolve
/// requires-reason      combat.projectile.tile-resolve
/// </code>
/// Blank lines and lines starting with <c>#</c> are ignored. Every malformed line is reported
/// with its one-based line number; parsing never returns a partially applied rule set.
/// </remarks>
public static class EcsCoordinationRuleLoader
{
  private const string BeforeKeyword = "before";
  private const string PhaseKeyword = "phase";
  private const string RequiresDependencyKeyword = "requires-dependency";
  private const string RequiresReasonKeyword = "requires-reason";

  /// <summary>Parses a rule document. Returns <see cref="EcsCoordinationRuleSet.Empty"/> when empty.</summary>
  public static EcsCoordinationRuleSet Parse(string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    List<EcsCoordinationRule> rules = [];
    string[] lines = text.Split('\n');
    for (int index = 0; index < lines.Length; index++)
    {
      string line = lines[index].Trim();
      if (line.Length == 0 || line[0] == '#')
      {
        continue;
      }

      rules.Add(ParseLine(line, index + 1));
    }

    return EcsCoordinationRuleSet.Create(rules);
  }

  private static EcsCoordinationRule ParseLine(string line, int lineNumber)
  {
    string[] parts = line.Split(
        [' ', '\t'],
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    string keyword = parts[0];
    return keyword switch
    {
      BeforeKeyword => ParseBefore(parts, lineNumber),
      PhaseKeyword => ParsePhase(parts, lineNumber),
      RequiresDependencyKeyword => ParseRequiredKey(
          parts, lineNumber, EcsCoordinationRule.RequiresDependency),
      RequiresReasonKeyword => ParseRequiredKey(
          parts, lineNumber, EcsCoordinationRule.RequiresReason),
      _ => throw Invalid(lineNumber, $"Unknown coordination rule keyword '{keyword}'.")
    };
  }

  private static EcsCoordinationRule ParseBefore(string[] parts, int lineNumber)
  {
    if (parts.Length != 3)
    {
      throw Invalid(
          lineNumber,
          $"'{BeforeKeyword}' expects exactly two keys but found {parts.Length - 1}.");
    }

    return EcsCoordinationRule.Before(parts[1], parts[2]);
  }

  private static EcsCoordinationRule ParsePhase(string[] parts, int lineNumber)
  {
    if (parts.Length != 3)
    {
      throw Invalid(
          lineNumber,
          $"'{PhaseKeyword}' expects a phase name and one key but found {parts.Length - 1} " +
          "arguments.");
    }

    if (!Enum.TryParse(parts[1], ignoreCase: true, out EcsServerPhase phase) ||
        !Enum.IsDefined(phase))
    {
      throw Invalid(
          lineNumber,
          $"'{parts[1]}' is not a known {nameof(EcsServerPhase)} value. Known values: " +
          string.Join(", ", Enum.GetNames<EcsServerPhase>()) + ".");
    }

    return EcsCoordinationRule.RequiresPhase(phase, parts[2]);
  }

  private static EcsCoordinationRule ParseRequiredKey(
      string[] parts,
      int lineNumber,
      Func<string, EcsCoordinationRule> factory)
  {
    if (parts.Length != 2)
    {
      throw Invalid(lineNumber, $"'{parts[0]}' expects exactly one key.");
    }

    return factory(parts[1]);
  }

  private static EcsServerSchedulingException Invalid(int lineNumber, string detail)
  {
    return new EcsServerSchedulingException(
        EcsServerSchedulingFailureKind.CoordinationRuleInvalid,
        $"Invalid coordination rule at line {lineNumber.ToString(CultureInfo.InvariantCulture)}: " +
        detail);
  }
}
