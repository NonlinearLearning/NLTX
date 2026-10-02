using System.Text.RegularExpressions;

namespace Terraria.NonAuthoritative.Platform;

public static class FilePathParsingAdapter
{
  private static readonly Regex FileNameRegex = new(
    "^(?<path>.*[\\\\/])?(?:$|(?<fileName>.+?)(?:(?<extension>\\.[^.]*$)|$))",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  public static string GetFileName(string path, bool includeExtension = true)
  {
    ArgumentNullException.ThrowIfNull(path);

    Match match = FileNameRegex.Match(path);
    if (!match.Success)
    {
      return string.Empty;
    }

    Group fileName = match.Groups["fileName"];
    Group extension = match.Groups["extension"];
    return fileName.Value + (includeExtension && extension.Success ? extension.Value : string.Empty);
  }

  public static string GetParentFolderPath(string path)
  {
    ArgumentNullException.ThrowIfNull(path);

    Match match = FileNameRegex.Match(path);
    return match.Success ? match.Groups["path"].Value : string.Empty;
  }

  public static string GetFullPath(string path, bool isCloud)
  {
    ArgumentNullException.ThrowIfNull(path);
    return isCloud ? path : Path.GetFullPath(path);
  }
}
