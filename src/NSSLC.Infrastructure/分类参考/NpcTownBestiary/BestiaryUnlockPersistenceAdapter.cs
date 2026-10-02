using System.IO;

namespace Terraria.NpcTownBestiary;

public sealed class BestiaryUnlockPersistenceAdapter
{
  public void Save(
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats,
    BinaryWriter writer)
  {
    ArgumentNullException.ThrowIfNull(kills);
    ArgumentNullException.ThrowIfNull(sights);
    ArgumentNullException.ThrowIfNull(chats);
    ArgumentNullException.ThrowIfNull(writer);

    writer.Write(kills.CountsByCredit.Count);
    foreach (KeyValuePair<BestiaryCreditId, int> entry in kills.CountsByCredit
      .OrderBy(entry => entry.Key.Value, StringComparer.Ordinal))
    {
      writer.Write(entry.Key.Value);
      writer.Write(entry.Value);
    }

    writer.Write(sights.DiscoveredCredits.Count);
    foreach (BestiaryCreditId creditId in sights.DiscoveredCredits
      .OrderBy(creditId => creditId.Value, StringComparer.Ordinal))
    {
      writer.Write(creditId.Value);
    }

    writer.Write(chats.DiscoveredCredits.Count);
    foreach (BestiaryCreditId creditId in chats.DiscoveredCredits
      .OrderBy(creditId => creditId.Value, StringComparer.Ordinal))
    {
      writer.Write(creditId.Value);
    }
  }

  public void Load(
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats,
    BinaryReader reader)
  {
    ArgumentNullException.ThrowIfNull(kills);
    ArgumentNullException.ThrowIfNull(sights);
    ArgumentNullException.ThrowIfNull(chats);
    ArgumentNullException.ThrowIfNull(reader);

    kills.Clear();
    sights.Clear();
    chats.Clear();

    int killCount = ReadCount(reader, "kill");
    for (int i = 0; i < killCount; i++)
    {
      kills.Restore(new BestiaryCreditId(reader.ReadString()), reader.ReadInt32());
    }

    int sightCount = ReadCount(reader, "sight");
    for (int i = 0; i < sightCount; i++)
    {
      sights.Restore(new BestiaryCreditId(reader.ReadString()));
    }

    int chatCount = ReadCount(reader, "chat");
    for (int i = 0; i < chatCount; i++)
    {
      chats.Restore(new BestiaryCreditId(reader.ReadString()));
    }
  }

  private static int ReadCount(BinaryReader reader, string kind)
  {
    int count = reader.ReadInt32();
    if (count < 0)
    {
      throw new InvalidDataException($"Bestiary {kind} count cannot be negative.");
    }

    return count;
  }
}
