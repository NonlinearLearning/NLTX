namespace Terraria.NonAuthoritative.ContentDefinitions;

public readonly record struct BuffAbilityFlags(
  bool IsPvp,
  bool IsPersistent,
  bool IsVanityPet,
  bool IsLightPet,
  bool IsMelee,
  bool IsDebuff,
  bool DoNotSave,
  bool HideTimeDisplay);

public sealed class ContentAbilityCatalog
{
  private readonly bool[] _projectileHostile;
  private readonly bool[] _projectileHook;
  private readonly bool[] _pvpBuff;
  private readonly bool[] _persistentBuff;
  private readonly bool[] _vanityPet;
  private readonly bool[] _lightPet;
  private readonly bool[] _meleeBuff;
  private readonly bool[] _debuff;
  private readonly bool[] _buffNoSave;
  private readonly bool[] _buffNoTimeDisplay;
  private float _musicPitch;

  internal ContentAbilityCatalog(ContentSize maxWorldViewSize, int projectileCount, int buffCount)
  {
    if (projectileCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileCount));
    }

    if (buffCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(buffCount));
    }

    MaxWorldViewSize = maxWorldViewSize;
    _projectileHostile = new bool[projectileCount];
    _projectileHook = new bool[projectileCount];
    _pvpBuff = new bool[buffCount];
    _persistentBuff = new bool[buffCount];
    _vanityPet = new bool[buffCount];
    _lightPet = new bool[buffCount];
    _meleeBuff = new bool[buffCount];
    _debuff = new bool[buffCount];
    _buffNoSave = new bool[buffCount];
    _buffNoTimeDisplay = new bool[buffCount];
  }

  public ContentSize MaxWorldViewSize { get; }

  public int ProjectileCount => _projectileHostile.Length;

  public int BuffCount => _pvpBuff.Length;

  internal void SetMusicPitch(float musicPitch)
  {
    if (float.IsNaN(musicPitch) || float.IsInfinity(musicPitch))
    {
      throw new ArgumentOutOfRangeException(nameof(musicPitch));
    }

    _musicPitch = musicPitch;
  }

  internal void SetProjectile(int projectileType, bool isHostile, bool isHook)
  {
    ValidateIndex(projectileType, _projectileHostile.Length, nameof(projectileType));
    _projectileHostile[projectileType] = isHostile;
    _projectileHook[projectileType] = isHook;
  }

  internal void SetBuff(int buffType, BuffAbilityFlags flags)
  {
    ValidateIndex(buffType, _pvpBuff.Length, nameof(buffType));
    _pvpBuff[buffType] = flags.IsPvp;
    _persistentBuff[buffType] = flags.IsPersistent;
    _vanityPet[buffType] = flags.IsVanityPet;
    _lightPet[buffType] = flags.IsLightPet;
    _meleeBuff[buffType] = flags.IsMelee;
    _debuff[buffType] = flags.IsDebuff;
    _buffNoSave[buffType] = flags.DoNotSave;
    _buffNoTimeDisplay[buffType] = flags.HideTimeDisplay;
  }

  internal ContentAbilitySnapshot CreateSnapshot()
  {
    return new ContentAbilitySnapshot(
      MaxWorldViewSize,
      _musicPitch,
      _projectileHostile,
      _projectileHook,
      _pvpBuff,
      _persistentBuff,
      _vanityPet,
      _lightPet,
      _meleeBuff,
      _debuff,
      _buffNoSave,
      _buffNoTimeDisplay);
  }

  private static void ValidateIndex(int index, int length, string parameterName)
  {
    if ((uint)index >= (uint)length)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}

public sealed class ContentAbilitySnapshot
{
  internal ContentAbilitySnapshot(
    ContentSize maxWorldViewSize,
    float musicPitch,
    IReadOnlyList<bool> projectileHostile,
    IReadOnlyList<bool> projectileHook,
    IReadOnlyList<bool> pvpBuff,
    IReadOnlyList<bool> persistentBuff,
    IReadOnlyList<bool> vanityPet,
    IReadOnlyList<bool> lightPet,
    IReadOnlyList<bool> meleeBuff,
    IReadOnlyList<bool> debuff,
    IReadOnlyList<bool> buffNoSave,
    IReadOnlyList<bool> buffNoTimeDisplay)
  {
    MaxWorldViewSize = maxWorldViewSize;
    MusicPitch = musicPitch;
    ProjectileHostile = Array.AsReadOnly(projectileHostile.ToArray());
    ProjectileHook = Array.AsReadOnly(projectileHook.ToArray());
    PvpBuff = Array.AsReadOnly(pvpBuff.ToArray());
    PersistentBuff = Array.AsReadOnly(persistentBuff.ToArray());
    VanityPet = Array.AsReadOnly(vanityPet.ToArray());
    LightPet = Array.AsReadOnly(lightPet.ToArray());
    MeleeBuff = Array.AsReadOnly(meleeBuff.ToArray());
    Debuff = Array.AsReadOnly(debuff.ToArray());
    BuffNoSave = Array.AsReadOnly(buffNoSave.ToArray());
    BuffNoTimeDisplay = Array.AsReadOnly(buffNoTimeDisplay.ToArray());
  }

  public ContentSize MaxWorldViewSize { get; }

  public float MusicPitch { get; }

  public IReadOnlyList<bool> ProjectileHostile { get; }

  public IReadOnlyList<bool> ProjectileHook { get; }

  public IReadOnlyList<bool> PvpBuff { get; }

  public IReadOnlyList<bool> PersistentBuff { get; }

  public IReadOnlyList<bool> VanityPet { get; }

  public IReadOnlyList<bool> LightPet { get; }

  public IReadOnlyList<bool> MeleeBuff { get; }

  public IReadOnlyList<bool> Debuff { get; }

  public IReadOnlyList<bool> BuffNoSave { get; }

  public IReadOnlyList<bool> BuffNoTimeDisplay { get; }

  public bool IsProjectileHostile(int projectileType)
  {
    return Read(ProjectileHostile, projectileType, nameof(projectileType));
  }

  public bool IsProjectileHook(int projectileType)
  {
    return Read(ProjectileHook, projectileType, nameof(projectileType));
  }

  public bool IsPvpBuff(int buffType)
  {
    return Read(PvpBuff, buffType, nameof(buffType));
  }

  public bool IsPersistentBuff(int buffType)
  {
    return Read(PersistentBuff, buffType, nameof(buffType));
  }

  public bool HideBuffTimeDisplay(int buffType)
  {
    return Read(BuffNoTimeDisplay, buffType, nameof(buffType));
  }

  private static bool Read(IReadOnlyList<bool> values, int index, string parameterName)
  {
    if ((uint)index >= (uint)values.Count)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return values[index];
  }
}

public static class ContentAbilityRegistrationSystem
{
  public static ContentAbilityCatalog CreateCatalog(
    ContentSize maxWorldViewSize,
    int projectileCount,
    int buffCount)
  {
    return new ContentAbilityCatalog(maxWorldViewSize, projectileCount, buffCount);
  }

  public static void RegisterProjectile(
    ContentAbilityCatalog catalog,
    int projectileType,
    bool isHostile,
    bool isHook)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.SetProjectile(projectileType, isHostile, isHook);
  }

  public static void RegisterBuff(
    ContentAbilityCatalog catalog,
    int buffType,
    BuffAbilityFlags flags)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.SetBuff(buffType, flags);
  }

  public static void SetMusicPitch(ContentAbilityCatalog catalog, float musicPitch)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.SetMusicPitch(musicPitch);
  }
}

public static class ContentAbilityQuery
{
  public static ContentAbilitySnapshot Snapshot(ContentAbilityCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.CreateSnapshot();
  }
}
