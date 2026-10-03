using Terraria.Utilities;

namespace Terraria.IO;

public abstract class FileData
{
	protected string _path;

	public FileMetadata Metadata;

	public string Name;

	public readonly string Type;

	protected bool _isFavorite;

	public string Path => _path;

	public bool IsFavorite => _isFavorite;

	protected FileData(string type)
	{
		Type = type;
	}

	protected FileData(string type, string path)
	{
		Type = type;
		_path = path;
		_isFavorite = Main.LocalFavoriteData.IsFavorite(this);
	}

	public void ToggleFavorite()
{
	SetFavorite(!IsFavorite);
	}
	public string GetFileName(bool includeExtension = true)
{
	return FileUtilities.GetFileName(Path, includeExtension);
	}
	public void SetFavorite(bool favorite, bool saveChanges = true)
{
	_isFavorite = favorite;
	if (saveChanges)
	{
		Main.LocalFavoriteData.SaveFavorite(this);
	}
	}
	public abstract void SetAsActive();

}

