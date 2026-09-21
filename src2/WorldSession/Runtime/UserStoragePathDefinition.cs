namespace Terraria.WorldSession.Runtime;

public readonly record struct UserStoragePathDefinition
{
  public string SavePath { get; }

  public string SaveFolderName { get; }

  public UserStoragePathDefinition(string savePath, string saveFolderName)
  {
    SavePath = RequireText(savePath, nameof(savePath));
    SaveFolderName = RequireText(saveFolderName, nameof(saveFolderName));
  }

  private static string RequireText(string value, string parameterName)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("A storage path value is required.", parameterName);
    }

    return value;
  }
}
