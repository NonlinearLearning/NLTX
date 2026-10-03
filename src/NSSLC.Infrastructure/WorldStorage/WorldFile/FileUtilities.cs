using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ReLogic.OS;

namespace Terraria.Utilities;

public static class FileUtilities
{
	private static Regex FileNameRegex = new Regex("^(?<path>.*[\\\\\\/])?(?:$|(?<fileName>.+?)(?:(?<extension>\\.[^.]*$)|$))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public static bool Exists(string path)
{
	return File.Exists(path);
	}
	public static void Delete(string path, bool forceDeleteFile = false)
{
	if (forceDeleteFile)
	{
		File.Delete(path);
	}
	else
	{
		Platform.Get<IPathService>().MoveToRecycleBin(path);
	}
	}
	public static string GetFullPath(string path)
{
	return Path.GetFullPath(path);
	}
	public static void Copy(string source, string destination)
{
	try
	{
		File.Copy(source, destination, overwrite: true);
		return;
	}
	catch (IOException ex)
	{
		if (ex.GetType() != typeof(IOException))
		{
			throw;
		}
		using FileStream fileStream = File.OpenRead(source);
		using FileStream destination2 = File.Create(destination);
		fileStream.CopyTo(destination2);
		return;
	}
	}
	public static void Move(string source, string destination)
{
	try
	{
		if (File.Exists(destination))
		{
			File.Delete(destination);
		}
		File.Move(source, destination);
		return;
	}
	catch (IOException)
	{
	}
	Copy(source, destination);
	Delete(source, forceDeleteFile: true);
	}
	public static int GetFileSize(string path)
{
	return (int)new FileInfo(path).Length;
	}
	public static void Read(string path, byte[] buffer, int length)
{
	using FileStream fileStream = File.OpenRead(path);
	fileStream.Read(buffer, 0, length);
	}
	public static byte[] ReadAllBytes(string path)
{
	return File.ReadAllBytes(path);
	}
	public static bool WriteAllBytes(string path, byte[] data)
{
	return Write(path, data, data.Length);
	}
	public static bool Write(string path, byte[] data, int length)
{
	string parentFolderPath = GetParentFolderPath(path);
	if (parentFolderPath != "")
	{
		Utils.TryCreatingDirectory(parentFolderPath);
	}
	RemoveReadOnlyAttribute(path);
	using (FileStream fileStream = File.Open(path, FileMode.Create))
	{
		while (fileStream.Position < length)
		{
			fileStream.Write(data, (int)fileStream.Position, Math.Min(length - (int)fileStream.Position, 2048));
		}
	}
	return true;
	}
	public static void RemoveReadOnlyAttribute(string path)
{
	if (!File.Exists(path))
	{
		return;
	}
	try
	{
		FileAttributes attributes = File.GetAttributes(path);
		if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
		{
			attributes &= ~FileAttributes.ReadOnly;
			File.SetAttributes(path, attributes);
		}
	}
	catch (Exception)
	{
	}
	}
	public static string GetFileName(string path, bool includeExtension = true)
{
	Match match = FileNameRegex.Match(path);
	if (match == null || match.Groups["fileName"] == null)
	{
		return "";
	}
	includeExtension &= match.Groups["extension"] != null;
	return match.Groups["fileName"].Value + (includeExtension ? match.Groups["extension"].Value : "");
	}
	public static string GetParentFolderPath(string path, bool includeExtension = true)
{
	Match match = FileNameRegex.Match(path);
	if (match == null || match.Groups["path"] == null)
	{
		return "";
	}
	return match.Groups["path"].Value;
	}
	public static void CopyFolder(string sourcePath, string destinationPath)
{
	Directory.CreateDirectory(destinationPath);
	string[] directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
	for (int i = 0; i < directories.Length; i++)
	{
		Directory.CreateDirectory(directories[i].Replace(sourcePath, destinationPath));
	}
	directories = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
	foreach (string obj in directories)
	{
		File.Copy(obj, obj.Replace(sourcePath, destinationPath), overwrite: true);
	}
	}
	public static void ProtectedInvoke(Action action)
{
	bool isBackground = Thread.CurrentThread.IsBackground;
	try
	{
		Thread.CurrentThread.IsBackground = false;
		action();
	}
	finally
	{
		Thread.CurrentThread.IsBackground = isBackground;
	}
	}}

