using Terraria.Content;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class SimulationContentBootstrap
{
  public static ContentCatalog Build()
  {
    const int woodenBow = 39;
    const int gel = 23;
    const int woodenArrowProjectile = 1;
    const int blueSlime = SimulationContentSupportManifest.BlueSlimeNetId;
    const int demonEye = SimulationContentSupportManifest.DemonEyeNetId;
    const int greenSlime = SimulationContentSupportManifest.GreenSlimeNetId;
    const int guide = SimulationContentSupportManifest.GuideNetId;
    const int oldMan = SimulationContentSupportManifest.OldManNetId;
    const int eyeOfCthulhu = SimulationContentSupportManifest.EyeOfCthulhuNetId;
    const int servantOfCthulhu = SimulationContentSupportManifest.ServantOfCthulhuNetId;
    const int zombie = 3;
    const int trainingDummy = 488;

    ItemDefinition bow = new(
      new ItemIdentityDefinition(
        woodenBow,
        "item.wooden-bow",
        SimulationContentSupportManifest.Version),
      new ItemUseDefinition(
        HoldStyle: 0,
        UseStyle: 5,
        Channel: false,
        UseAnimationTicks: 30,
        UseTimeTicks: 30,
        AutoReuse: false,
        UseTurn: true,
        NoUseGraphic: false,
        NoMeleeGraphic: false,
        ShootsEveryUse: true,
        UseSoundKey: null,
        UseSoundPitch: 0),
      new ItemStackDefinition(MaxStack: 1, UniqueStack: true, IsMaterial: false, DefaultStack: 1),
      new ItemCombatDefinition(
        Damage: 4,
        KnockBack: 2,
        CritChance: 4,
        ArmorPenetration: 0,
        BonusTagDamage: 0,
        ShootTypeId: woodenArrowProjectile,
        ShootSpeed: 8,
        AmmoTypeId: null,
        UseAmmoTypeId: 40,
        IsNotAmmo: true,
        IsMelee: false,
        IsRanged: true,
        IsMagic: false,
        IsSummon: false),
      new ItemPlacementDefinition(null, null, 0, false),
      new ItemEffectDefinition(0, 0, 0, 0, 0, null, 0, false, false, false),
      new ItemEquipmentDefinition(false, null, null, null),
      new ItemEconomyDefinition(50, null),
      new ItemCapabilitiesDefinition(false, false, false));

    ItemDefinition arrow = new(
      new ItemIdentityDefinition(
        40,
        "item.wooden-arrow",
        SimulationContentSupportManifest.Version),
      new ItemUseDefinition(0, 0, false, 0, 0, false, false, false, false, false, null, 0),
      new ItemStackDefinition(MaxStack: 9999, UniqueStack: false, IsMaterial: true, DefaultStack: 1),
      new ItemCombatDefinition(0, 0, 0, 0, 0, null, 0, null, null, false, false, false, false, false),
      new ItemPlacementDefinition(null, null, 0, false),
      new ItemEffectDefinition(0, 0, 0, 0, 0, null, 0, false, false, false),
      new ItemEquipmentDefinition(false, null, null, null),
      new ItemEconomyDefinition(0, null),
      new ItemCapabilitiesDefinition(false, false, false));

    ItemDefinition gelItem = new(
      new ItemIdentityDefinition(
        gel,
        "item.gel",
        SimulationContentSupportManifest.Version),
      new ItemUseDefinition(0, 0, false, 0, 0, false, false, false, false, false, null, 0),
      new ItemStackDefinition(MaxStack: 9999, UniqueStack: false, IsMaterial: true, DefaultStack: 1),
      new ItemCombatDefinition(0, 0, 0, 0, 0, null, 0, null, null, true, false, false, false, false),
      new ItemPlacementDefinition(null, null, 0, false),
      new ItemEffectDefinition(0, 0, 0, 0, 0, null, 0, false, false, false),
      new ItemEquipmentDefinition(false, null, null, null),
      new ItemEconomyDefinition(0, null),
      new ItemCapabilitiesDefinition(false, false, false));

    NpcDefinition guideDefinition = new(
      typeId: guide,
      netId: guide,
      persistentId: "npc.guide",
      defaultLifeMax: 250,
      defaultDamage: 10,
      defaultDefense: 15,
      width: 18,
      height: 40,
      aiStyle: 0,
      isTownNpc: true,
      friendly: true,
      hostile: false);
    NpcDefinition oldManDefinition = new(
      typeId: oldMan,
      netId: oldMan,
      persistentId: "npc.old-man",
      defaultLifeMax: 250,
      defaultDamage: 10,
      defaultDefense: 15,
      width: 18,
      height: 40,
      aiStyle: 0,
      isTownNpc: true,
      friendly: true,
      hostile: false);
    NpcDefinition zombieDefinition = new(
      typeId: zombie,
      netId: zombie,
      persistentId: "npc.zombie",
      defaultLifeMax: 45,
      defaultDamage: 14,
      defaultDefense: 6,
      width: 18,
      height: 40,
      aiStyle: 3,
      isTownNpc: false,
      friendly: false,
      hostile: true);
    NpcDefinition blueSlimeDefinition = new(
      typeId: blueSlime,
      netId: blueSlime,
      persistentId: "npc.blue-slime",
      defaultLifeMax: 25,
      defaultDamage: 7,
      defaultDefense: 2,
      width: 24,
      height: 18,
      aiStyle: 1,
      isTownNpc: false,
      friendly: false,
      hostile: true)
    {
      Core = new NpcCoreDefinition(25, 7, 2) { Value = 25 },
    };
    NpcDefinition greenSlimeDefinition = new(
      typeId: greenSlime,
      netId: greenSlime,
      persistentId: "npc.green-slime",
      defaultLifeMax: 14,
      defaultDamage: 7,
      defaultDefense: 0,
      width: 32,
      height: 24,
      aiStyle: 1,
      isTownNpc: false,
      friendly: false,
      hostile: true);
    NpcDefinition demonEyeDefinition = new(
      typeId: demonEye,
      netId: demonEye,
      persistentId: "npc.demon-eye",
      defaultLifeMax: 60,
      defaultDamage: 18,
      defaultDefense: 2,
      width: 32,
      height: 32,
      aiStyle: 2,
      isTownNpc: false,
      friendly: false,
      hostile: true);
    NpcDefinition trainingDummyDefinition = new(
      typeId: trainingDummy,
      netId: trainingDummy,
      persistentId: "npc.training-dummy",
      defaultLifeMax: 1_000_000,
      defaultDamage: 0,
      defaultDefense: 0,
      width: 32,
      height: 48,
      aiStyle: 0,
      isTownNpc: false,
      friendly: false,
      hostile: false);
    NpcDefinition eyeOfCthulhuDefinition = new(
      typeId: eyeOfCthulhu,
      netId: eyeOfCthulhu,
      persistentId: "npc.eye-of-cthulhu",
      defaultLifeMax: 2800,
      defaultDamage: 15,
      defaultDefense: 12,
      width: 100,
      height: 110,
      aiStyle: 4,
      isTownNpc: false,
      friendly: false,
      hostile: true)
    {
      Core = new NpcCoreDefinition(2800, 15, 12) { Value = 30_000 },
      Capabilities = new NpcCapabilitiesDefinition(false, true, IsBoss: true),
    };
    NpcDefinition servantOfCthulhuDefinition = new(
      typeId: servantOfCthulhu,
      netId: servantOfCthulhu,
      persistentId: "npc.servant-of-cthulhu",
      defaultLifeMax: 8,
      defaultDamage: 12,
      defaultDefense: 0,
      width: 20,
      height: 20,
      aiStyle: 5,
      isTownNpc: false,
      friendly: false,
      hostile: true)
    {
      Core = new NpcCoreDefinition(8, 12, 0),
    };

    ProjectileDefinition arrowProjectile = new(
      typeId: woodenArrowProjectile,
      persistentId: "projectile.wooden-arrow",
      width: 10,
      height: 10,
      aiStyle: 1,
      defaultTimeLeft: SimulationContentSupportManifest.SupportedArrowLifetimeTicks,
      penetrate: 1,
      friendly: true,
      hostile: false)
    {
      Identity = new ProjectileIdentityDefinition(
        woodenArrowProjectile,
        "projectile.wooden-arrow",
        NeedsUuid: false),
      Combat = new ProjectileCombatDefinition(0, 0f, true, false)
      {
        Ranged = true,
      },
      Capabilities = new ProjectileCapabilitiesDefinition(
        IsArrow: true,
        IsBobber: false,
        IsMinion: false,
        IsSentry: false),
    };

    var items = new ItemDefinitionCatalog(new[] { bow, arrow, gelItem });
    var npcs = new NpcDefinitionCatalog(
      new[]
      {
        blueSlimeDefinition,
        demonEyeDefinition,
        zombieDefinition,
        greenSlimeDefinition,
        guideDefinition,
        oldManDefinition,
        eyeOfCthulhuDefinition,
        servantOfCthulhuDefinition,
        trainingDummyDefinition,
      });
    var projectiles = new ProjectileDefinitionCatalog(new[] { arrowProjectile });
    var identities = new ContentIdentityCatalog(
      new Dictionary<int, string>
      {
        [woodenBow] = bow.Identity.PersistentId,
        [40] = arrow.Identity.PersistentId,
        [gel] = gelItem.Identity.PersistentId,
      },
      new Dictionary<int, string>
      {
        [greenSlime] = greenSlimeDefinition.PersistentId,
        [blueSlime] = blueSlimeDefinition.PersistentId,
        [demonEye] = demonEyeDefinition.PersistentId,
        [guide] = guideDefinition.PersistentId,
        [oldMan] = oldManDefinition.PersistentId,
        [zombie] = zombieDefinition.PersistentId,
        [eyeOfCthulhu] = eyeOfCthulhuDefinition.PersistentId,
        [servantOfCthulhu] = servantOfCthulhuDefinition.PersistentId,
        [trainingDummy] = trainingDummyDefinition.PersistentId,
      },
      new Dictionary<int, string>
      {
        [woodenArrowProjectile] = arrowProjectile.PersistentId!,
      });
    var snapshot = new ContentCatalogSnapshot(
      catalogRevision: 1,
      sourceKey: SimulationContentSupportManifest.Version,
      items,
      npcs,
      projectiles,
      identities);
    ContentCatalog catalog = ContentCatalogBuildSystem.Build(snapshot);
    SimulationContentSupportManifest.Validate(catalog);
    return catalog;
  }
}
