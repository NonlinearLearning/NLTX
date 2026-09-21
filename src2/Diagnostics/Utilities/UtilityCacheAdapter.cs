using System.Text.RegularExpressions;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class UtilityCacheAdapter
{
  private static readonly Regex SubstitutionRegex = new(
    "{(\\?(?:!)?)?([a-zA-Z][\\w\\.]*)}",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  public Regex GetSubstitutionRegex()
  {
    return SubstitutionRegex;
  }
}
