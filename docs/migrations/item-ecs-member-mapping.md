# Item ECS 成员归属表

本表由 `Build/Tools/GenerateItemEcsBaseline.ps1` 从 Version4 `Terraria/Item.cs` 生成。分类是迁移边界初始归属，必须在后续批次中以实现或延期证据收敛。

| ID | 成员 | 类型 | 来源行 | 初始归属 | 目标/原因 |
| --- | --- | --- | ---: | --- | --- |
| `Item:Field:20:width:ED2BE84B7FF30CC1` | `width` | Field | 20 | Definition | `Items/Definitions` or immutable type metadata; `public int width;` |
| `Item:Field:22:height:4DA2DF6318E739CF` | `height` | Field | 22 | Definition | `Items/Definitions` or immutable type metadata; `public int height;` |
| `Item:Field:24:coinGrabRange:3C27BD31BF60E84A` | `coinGrabRange` | Field | 24 | Compatibility | `Items/Compatibility` boundary adapter; `public static int coinGrabRange = 350;` |
| `Item:Field:26:manaGrabRange:2E98AB09938B5BBF` | `manaGrabRange` | Field | 26 | Definition | `Items/Definitions` or immutable type metadata; `public static int manaGrabRange = 300;` |
| `Item:Field:28:lifeGrabRange:F0D9FBD040F40361` | `lifeGrabRange` | Field | 28 | Compatibility | `Items/Compatibility` boundary adapter; `public static int lifeGrabRange = 250;` |
| `Item:Field:30:treasureGrabRange:C40EC3C54C10B44D` | `treasureGrabRange` | Field | 30 | Compatibility | `Items/Compatibility` boundary adapter; `public static int treasureGrabRange = 150;` |
| `Item:Field:32:_nameOverride:2D2819A92326ED97` | `_nameOverride` | Field | 32 | Definition | `Items/Definitions` or immutable type metadata; `private string _nameOverride;` |
| `Item:Field:34:luckPotionDuration1:5F5DDEE94FA8E339` | `luckPotionDuration1` | Field | 34 | Definition | `Items/Definitions` or immutable type metadata; `public const int luckPotionDuration1 = 18000;` |
| `Item:Field:36:luckPotionDuration2:E66ABBB557070EB0` | `luckPotionDuration2` | Field | 36 | Definition | `Items/Definitions` or immutable type metadata; `public const int luckPotionDuration2 = 36000;` |
| `Item:Field:38:luckPotionDuration3:6B19D69563E5282E` | `luckPotionDuration3` | Field | 38 | Definition | `Items/Definitions` or immutable type metadata; `public const int luckPotionDuration3 = 54000;` |
| `Item:Field:40:flaskTime:53F86B55CE5F2329` | `flaskTime` | Field | 40 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public const int flaskTime = 72000;` |
| `Item:Field:42:copper:C10A897E8632919E` | `copper` | Field | 42 | Definition | `ItemPriceSystem.CopperValue` is the immutable copper-unit conversion constant; `public const int copper = 1;` |
| `Item:Field:44:silver:C0DDA1D9FE16FE7A` | `silver` | Field | 44 | Definition | `ItemPriceSystem.SilverValue` is the immutable copper-unit conversion constant; `public const int silver = 100;` |
| `Item:Field:46:gold:049299A1A94C257E` | `gold` | Field | 46 | Definition | `ItemPriceSystem.GoldValue` is the immutable copper-unit conversion constant; `public const int gold = 10000;` |
| `Item:Field:48:platinum:54D1D60D0B9E69AC` | `platinum` | Field | 48 | Definition | `ItemPriceSystem.PlatinumValue` is the immutable copper-unit conversion constant; `public const int platinum = 1000000;` |
| `Item:Field:50:goldCritterRarityColor:23D59873643C8D0B` | `goldCritterRarityColor` | Field | 50 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public const int goldCritterRarityColor = 3;` |
| `Item:Method:52:shadowOrbPrice:70C2C8AC19BA3D0E` | `shadowOrbPrice` | Method | 52 | Definition | `ItemPriceSystem.ShadowOrbPrice` preserves the immutable sell-price preset; `private readonly int shadowOrbPrice = sellPrice(0, 1, 50);` |
| `Item:Method:54:dungeonPrice:93E84375BEA24162` | `dungeonPrice` | Method | 54 | Definition | `ItemPriceSystem.DungeonPrice` preserves the immutable sell-price preset; `private readonly int dungeonPrice = sellPrice(0, 1, 75);` |
| `Item:Method:56:queenBeePrice:0DF01793EF37EA6D` | `queenBeePrice` | Method | 56 | Definition | `ItemPriceSystem.QueenBeePrice` preserves the immutable sell-price preset; `private readonly int queenBeePrice = sellPrice(0, 2);` |
| `Item:Method:58:hellPrice:C18B724FABAC9B79` | `hellPrice` | Method | 58 | Definition | `ItemPriceSystem.HellPrice` preserves the immutable sell-price preset; `private readonly int hellPrice = sellPrice(0, 2, 50);` |
| `Item:Method:60:eclipsePrice:FDFBD40D50D57DF0` | `eclipsePrice` | Method | 60 | Definition | `ItemPriceSystem.EclipsePrice` preserves the immutable sell-price preset; `private readonly int eclipsePrice = sellPrice(0, 7, 50);` |
| `Item:Method:62:eclipsePostPlanteraPrice:6BF590AF34D2501E` | `eclipsePostPlanteraPrice` | Method | 62 | Definition | `ItemPriceSystem.EclipsePostPlanteraPrice` preserves the immutable sell-price preset; `private readonly int eclipsePostPlanteraPrice = sellPrice(0, 10);` |
| `Item:Method:64:eclipseMothronPrice:1FD430DF6D46AC34` | `eclipseMothronPrice` | Method | 64 | Definition | `ItemPriceSystem.EclipseMothronPrice` preserves the immutable sell-price preset; `private readonly int eclipseMothronPrice = sellPrice(0, 12, 50);` |
| `Item:Field:66:CommonMaxStack:88CA1DB378405CF1` | `CommonMaxStack` | Field | 66 | Definition | `Items/Definitions` or immutable type metadata; `public static int CommonMaxStack = 9999;` |
| `Item:Method:68:cachedItemSpawnsByType:187E16B55808E4F2` | `cachedItemSpawnsByType` | Method | 68 | Definition | `Items/Definitions` or immutable type metadata; `public static int[] cachedItemSpawnsByType = ItemID.Sets.Factory.CreateIntSet(-1);` |
| `Item:Field:70:potionDelay:6938DA76C508B9E2` | `potionDelay` | Field | 70 | Definition | `Items/Definitions` or immutable type metadata; `public static int potionDelay = 3600;` |
| `Item:Field:72:restorationDelay:E532C1A44143B451` | `restorationDelay` | Field | 72 | Compatibility | `Items/Compatibility` boundary adapter; `public static int restorationDelay = 2700;` |
| `Item:Field:74:eggnogDelay:DF7E85B845A93253` | `eggnogDelay` | Field | 74 | Compatibility | `Items/Compatibility` boundary adapter; `public static int eggnogDelay = 2400;` |
| `Item:Field:76:mushroomDelay:7C827601ABE001FB` | `mushroomDelay` | Field | 76 | Compatibility | `Items/Compatibility` boundary adapter; `public static int mushroomDelay = 1800;` |
| `Item:Field:78:questItem:0D1627856AA4DD4B` | `questItem` | Field | 78 | Definition | `Items/Definitions` or immutable type metadata; `public bool questItem;` |
| `Item:Field:80:headType:A53DF5490BEAF305` | `headType` | Field | 80 | Definition | `Items/Definitions` or immutable type metadata; `public static int[] headType = new int[ArmorIDs.Head.Count];` |
| `Item:Field:82:bodyType:ACE07F615D60406B` | `bodyType` | Field | 82 | Definition | `Items/Definitions` or immutable type metadata; `public static int[] bodyType = new int[ArmorIDs.Body.Count];` |
| `Item:Field:84:legType:8DC03FF3CD35CAA5` | `legType` | Field | 84 | Definition | `Items/Definitions` or immutable type metadata; `public static int[] legType = new int[ArmorIDs.Legs.Count];` |
| `Item:Field:86:staff:2B55441EE4BCFD3F` | `staff` | Field | 86 | Compatibility | `Items/Compatibility` boundary adapter; `public static bool[] staff = new bool[ItemID.Count];` |
| `Item:Field:88:claw:DB8444FE0DDE5787` | `claw` | Field | 88 | Compatibility | `Items/Compatibility` boundary adapter; `public static bool[] claw = new bool[ItemID.Count];` |
| `Item:Field:90:flame:EF6510D4AD44CF8A` | `flame` | Field | 90 | Definition | `Items/Definitions` or immutable type metadata; `public bool flame;` |
| `Item:Field:92:mech:B03E6B8A6438DFB4` | `mech` | Field | 92 | Definition | `Items/Definitions` or immutable type metadata; `public bool mech;` |
| `Item:Field:94:tileWand:BCAAAEADE4376796` | `tileWand` | Field | 94 | Definition | `Items/Definitions` or immutable type metadata; `public int tileWand = -1;` |
| `Item:Field:96:wornArmor:0898D05AD12C7E38` | `wornArmor` | Field | 96 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public bool wornArmor;` |
| `Item:Field:98:tooltipContext:6F3F634E7C56296C` | `tooltipContext` | Field | 98 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public int tooltipContext = -1;` |
| `Item:Field:100:tooltipSlot:A37B40B83325E9C4` | `tooltipSlot` | Field | 100 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public int tooltipSlot = -1;` |
| `Item:Field:102:dye:38777BF53CA1CB34` | `dye` | Field | 102 | Definition | `Items/Definitions` or immutable type metadata; `public byte dye;` |
| `Item:Field:104:fishingPole:75829C5AEE9FFCC6` | `fishingPole` | Field | 104 | Definition | `Items/Definitions` or immutable type metadata; `public int fishingPole = 1;` |
| `Item:Field:106:bait:00A43B0E3FA1DBB3` | `bait` | Field | 106 | Definition | `Items/Definitions` or immutable type metadata; `public int bait;` |
| `Item:Field:108:makeNPC:3EEB64B30A051CF2` | `makeNPC` | Field | 108 | Definition | `Items/Definitions` or immutable type metadata; `public short makeNPC;` |
| `Item:Field:110:expertOnly:FC65E7A4189A7DF9` | `expertOnly` | Field | 110 | Definition | `Items/Definitions` or immutable type metadata; `public bool expertOnly;` |
| `Item:Field:112:expert:7E62F52C04050B4A` | `expert` | Field | 112 | Definition | `Items/Definitions` or immutable type metadata; `public bool expert;` |
| `Item:Field:114:isAShopItem:8CEECBB3A6B21457` | `isAShopItem` | Field | 114 | System | `Items/Systems` or deterministic command processing; `public bool isAShopItem;` |
| `Item:Field:116:hairDye:0286357EA3ED1D4F` | `hairDye` | Field | 116 | Definition | `Items/Definitions` or immutable type metadata; `public short hairDye = -1;` |
| `Item:Field:118:paint:4F66A5C624FB688D` | `paint` | Field | 118 | Definition | `Items/Definitions` or immutable type metadata; `public byte paint;` |
| `Item:Field:120:paintCoating:B8900D3BA0B31DCE` | `paintCoating` | Field | 120 | Definition | `Items/Definitions` or immutable type metadata; `public byte paintCoating;` |
| `Item:Field:122:type:B08745DD3A816197` | `type` | Field | 122 | Definition | `Items/Definitions` or immutable type metadata; `public int type;` |
| `Item:Field:124:favorited:8FBB3406E24699BF` | `favorited` | Field | 124 | Component | `ItemInstanceStateComponent.IsFavorited`; persisted account/world state and V1456 inventory/equipment projection; `public bool favorited;` |
| `Item:Field:126:holdStyle:D4CE6F7F1B5731BE` | `holdStyle` | Field | 126 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public int holdStyle;` |
| `Item:Field:128:useStyle:2543F3E32314571B` | `useStyle` | Field | 128 | System | `Items/Systems` or deterministic command processing; `public int useStyle;` |
| `Item:Field:130:channel:DC3BF9EBB049BB5D` | `channel` | Field | 130 | Definition | `Items/Definitions` or immutable type metadata; `public bool channel;` |
| `Item:Field:132:accessory:430DDD818FB268D6` | `accessory` | Field | 132 | Definition | `Items/Definitions` or immutable type metadata; `public bool accessory;` |
| `Item:Field:134:useAnimation:AB00037BDF34822F` | `useAnimation` | Field | 134 | System | `Items/Systems` or deterministic command processing; `public int useAnimation;` |
| `Item:Field:136:useTime:CE554D1D94817B02` | `useTime` | Field | 136 | System | `Items/Systems` or deterministic command processing; `public int useTime;` |
| `Item:Field:138:stack:A121A371291CE9E7` | `stack` | Field | 138 | Definition | `Items/Definitions` or immutable type metadata; `public int stack;` |
| `Item:Field:140:maxStack:B85B6A94ECF80A9A` | `maxStack` | Field | 140 | Definition | `Items/Definitions` or immutable type metadata; `public int maxStack;` |
| `Item:Field:142:pick:5A241C16B4C4104F` | `pick` | Field | 142 | System | `Items/Systems` or deterministic command processing; `public int pick;` |
| `Item:Field:144:axe:31952E9E9932DA87` | `axe` | Field | 144 | System | `Items/Systems` or deterministic command processing; `public int axe;` |
| `Item:Field:146:hammer:EEEAA9A9FCD9F919` | `hammer` | Field | 146 | System | `Items/Systems` or deterministic command processing; `public int hammer;` |
| `Item:Field:148:tileBoost:7845E832EDD6A3FC` | `tileBoost` | Field | 148 | Definition | `Items/Definitions` or immutable type metadata; `public int tileBoost;` |
| `Item:Field:150:createTile:4448F90A594872A1` | `createTile` | Field | 150 | System | `Items/Systems` or deterministic command processing; `public int createTile = -1;` |
| `Item:Field:152:createWall:60C7094B8D2D71EA` | `createWall` | Field | 152 | System | `Items/Systems` or deterministic command processing; `public int createWall = -1;` |
| `Item:Field:154:placeStyle:35FFD90B4B526179` | `placeStyle` | Field | 154 | System | `Items/Systems` or deterministic command processing; `public int placeStyle;` |
| `Item:Field:156:damage:7E9DB743700668E9` | `damage` | Field | 156 | Definition | `Items/Definitions` or immutable type metadata; `public int damage;` |
| `Item:Field:158:knockBack:F27747C7B51B49B5` | `knockBack` | Field | 158 | Definition | `Items/Definitions` or immutable type metadata; `public float knockBack;` |
| `Item:Field:160:healLife:D1CDE67946755C81` | `healLife` | Field | 160 | Definition | `Items/Definitions` or immutable type metadata; `public int healLife;` |
| `Item:Field:162:healMana:1B868BB5C986CEDC` | `healMana` | Field | 162 | Definition | `Items/Definitions` or immutable type metadata; `public int healMana;` |
| `Item:Field:164:potion:3C76D0CF421D34CB` | `potion` | Field | 164 | Definition | `Items/Definitions` or immutable type metadata; `public bool potion;` |
| `Item:Field:166:consumable:506E999B22C721AC` | `consumable` | Field | 166 | Definition | `Items/Definitions` or immutable type metadata; `public bool consumable;` |
| `Item:Field:168:autoReuse:79492CE378810FE4` | `autoReuse` | Field | 168 | Definition | `Items/Definitions` or immutable type metadata; `public bool autoReuse;` |
| `Item:Field:170:useTurn:22512A869BEB8534` | `useTurn` | Field | 170 | System | `Items/Systems` or deterministic command processing; `public bool useTurn;` |
| `Item:Field:172:color:8CA09E32792F7142` | `color` | Field | 172 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public Color color;` |
| `Item:Field:174:alpha:84DCDAC4A822B965` | `alpha` | Field | 174 | Definition | `Items/Definitions` or immutable type metadata; `public int alpha;` |
| `Item:Field:176:glowMask:EDBDE7022082EEDB` | `glowMask` | Field | 176 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public short glowMask;` |
| `Item:Field:178:scale:F67C888EAD9C9FD1` | `scale` | Field | 178 | Definition | `Items/Definitions` or immutable type metadata; `public float scale = 1f;` |
| `Item:Field:180:UseSound:569DDACBAD68E055` | `UseSound` | Field | 180 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public LegacySoundStyle UseSound;` |
| `Item:Field:182:useSoundPitch:6005703A1D0FB1C2` | `useSoundPitch` | Field | 182 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public float useSoundPitch;` |
| `Item:Field:184:defense:E9E90093A9217090` | `defense` | Field | 184 | Definition | `Items/Definitions` or immutable type metadata; `public int defense;` |
| `Item:Field:186:headSlot:EFC249382E2C7A32` | `headSlot` | Field | 186 | Definition | `Items/Definitions` or immutable type metadata; `public int headSlot = -1;` |
| `Item:Field:188:bodySlot:4982BD6BD096372D` | `bodySlot` | Field | 188 | Definition | `Items/Definitions` or immutable type metadata; `public int bodySlot = -1;` |
| `Item:Field:190:legSlot:728580B819765EC6` | `legSlot` | Field | 190 | Definition | `Items/Definitions` or immutable type metadata; `public int legSlot = -1;` |
| `Item:Field:192:handOnSlot:F61B0AA59918713A` | `handOnSlot` | Field | 192 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte handOnSlot = -1;` |
| `Item:Field:194:handOffSlot:036BEEBA8E66BD3F` | `handOffSlot` | Field | 194 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte handOffSlot = -1;` |
| `Item:Field:196:backSlot:7D0917A7EB353312` | `backSlot` | Field | 196 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte backSlot = -1;` |
| `Item:Field:198:frontSlot:90475A7780826224` | `frontSlot` | Field | 198 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte frontSlot = -1;` |
| `Item:Field:200:shoeSlot:8E67B5BB2DF58E72` | `shoeSlot` | Field | 200 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte shoeSlot = -1;` |
| `Item:Field:202:waistSlot:FC772B8DDADE4BFE` | `waistSlot` | Field | 202 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte waistSlot = -1;` |
| `Item:Field:204:wingSlot:8BE1A65A9609219A` | `wingSlot` | Field | 204 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte wingSlot = -1;` |
| `Item:Field:206:shieldSlot:98FC82F2034EF7CA` | `shieldSlot` | Field | 206 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte shieldSlot = -1;` |
| `Item:Field:208:neckSlot:2E923BAB75127AD1` | `neckSlot` | Field | 208 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte neckSlot = -1;` |
| `Item:Field:210:faceSlot:0630144BB2BB5CC4` | `faceSlot` | Field | 210 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte faceSlot = -1;` |
| `Item:Field:212:balloonSlot:15D2B1D38C980ADB` | `balloonSlot` | Field | 212 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte balloonSlot = -1;` |
| `Item:Field:214:beardSlot:D9EDB0A29F29FEC4` | `beardSlot` | Field | 214 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte beardSlot = -1;` |
| `Item:Field:216:voiceSlot:9DA6F8C070554F97` | `voiceSlot` | Field | 216 | Definition | `Items/Definitions` or immutable type metadata; `public sbyte voiceSlot;` |
| `Item:Field:218:stringColor:93C7F0DF7C377FB7` | `stringColor` | Field | 218 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public int stringColor;` |
| `Item:Field:220:ToolTip:89E1E73FA9CB4BE9` | `ToolTip` | Field | 220 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public ItemTooltip ToolTip;` |
| `Item:Field:222:BestiaryNotes:D71E4211E7BF82B9` | `BestiaryNotes` | Field | 222 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public string BestiaryNotes;` |
| `Item:Field:224:rare:9A51D305B9EAB7EE` | `rare` | Field | 224 | Definition | `Items/Definitions` or immutable type metadata; `public int rare;` |
| `Item:Field:226:shoot:5FAC0A2A3556CB82` | `shoot` | Field | 226 | System | `Items/Systems` or deterministic command processing; `public int shoot;` |
| `Item:Field:228:shootSpeed:98727CC0FE36F90F` | `shootSpeed` | Field | 228 | System | `Items/Systems` or deterministic command processing; `public float shootSpeed;` |
| `Item:Field:230:ammo:D4A62FCDC5FF960D` | `ammo` | Field | 230 | Definition | `Items/Definitions` or immutable type metadata; `public int ammo = AmmoID.None;` |
| `Item:Field:232:notAmmo:0129BB6FC3E0D79E` | `notAmmo` | Field | 232 | Definition | `Items/Definitions` or immutable type metadata; `public bool notAmmo;` |
| `Item:Field:234:useAmmo:411F12A358A7549C` | `useAmmo` | Field | 234 | System | `Items/Systems` or deterministic command processing; `public int useAmmo = AmmoID.None;` |
| `Item:Field:236:lifeRegen:6B2BBDB1FE584A6A` | `lifeRegen` | Field | 236 | Definition | `ItemEquipmentDefinition.LifeRegen` is immutable metadata; `EquipmentStatSystem` projects legacy regen units to the player entity's `HealthRegenerationComponent`; `public int lifeRegen;` |
| `Item:Field:238:manaIncrease:ABF4F220DB81BF78` | `manaIncrease` | Field | 238 | Definition | `Items/Definitions` or immutable type metadata; `public int manaIncrease;` |
| `Item:Field:240:buyOnce:7528522A68BCF638` | `buyOnce` | Field | 240 | Definition | `Items/Definitions` or immutable type metadata; `public bool buyOnce;` |
| `Item:Field:242:mana:AE7CF67886DA916A` | `mana` | Field | 242 | Definition | `Items/Definitions` or immutable type metadata; `public int mana;` |
| `Item:Field:244:noUseGraphic:0F7CCD81BF4B67E1` | `noUseGraphic` | Field | 244 | Definition | `Items/Definitions` or immutable type metadata; `public bool noUseGraphic;` |
| `Item:Field:246:noMelee:0475CE3510D015D6` | `noMelee` | Field | 246 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public bool noMelee;` |
| `Item:Field:248:value:97FDD81E514838E5` | `value` | Field | 248 | Definition | `Items/Definitions` or immutable type metadata; `public int value;` |
| `Item:Field:250:buy:64D713397D7C7988` | `buy` | Field | 250 | Definition | `Items/Definitions` or immutable type metadata; `public bool buy;` |
| `Item:Field:252:social:CA2B4F8BD59C72B3` | `social` | Field | 252 | Definition | `Items/Definitions` or immutable type metadata; `public bool social;` |
| `Item:Field:254:vanity:DB3EBEC5B49D1859` | `vanity` | Field | 254 | Definition | `Items/Definitions` or immutable type metadata; `public bool vanity;` |
| `Item:Field:256:material:53531FBB033132B2` | `material` | Field | 256 | Definition | `Items/Definitions` or immutable type metadata; `public bool material;` |
| `Item:Field:258:noWet:7DED287EAAC3FED3` | `noWet` | Field | 258 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public bool noWet;` |
| `Item:Field:260:buffType:80D928E2FBF58853` | `buffType` | Field | 260 | Definition | `Items/Definitions` or immutable type metadata; `public int buffType;` |
| `Item:Field:262:buffTime:057D9FD1B29509F2` | `buffTime` | Field | 262 | Definition | `ItemRecoveryDefinition.BuffDurationTicks`; validated atomic buff type/duration metadata and committed buff effect; `public int buffTime;` |
| `Item:Field:264:mountType:BB992F36940808E9` | `mountType` | Field | 264 | Definition | `Items/Definitions` or immutable type metadata; `public int mountType = -1;` |
| `Item:Field:266:cartTrack:98045FBF3CDB07E1` | `cartTrack` | Field | 266 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public bool cartTrack;` |
| `Item:Field:268:uniqueStack:D8F2468F52B77824` | `uniqueStack` | Field | 268 | Definition | `Items/Definitions` or immutable type metadata; `public bool uniqueStack;` |
| `Item:Field:270:shopSpecialCurrency:78DB082A47079EEB` | `shopSpecialCurrency` | Field | 270 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public int shopSpecialCurrency = -1;` |
| `Item:Field:272:shopCustomPrice:B7DD3FDB85F92C80` | `shopCustomPrice` | Field | 272 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public int? shopCustomPrice;` |
| `Item:Field:274:shootsEveryUse:EA52A5B44E7550DC` | `shootsEveryUse` | Field | 274 | System | `Items/Systems` or deterministic command processing; `public bool shootsEveryUse;` |
| `Item:Field:276:chlorophyteExtractinatorConsumable:3C9ACE6563602D04` | `chlorophyteExtractinatorConsumable` | Field | 276 | Definition | `Items/Definitions/ItemExtractinatorDefinition`; direct and Wiring/Chest Extractinator paths now use immutable extraction metadata. |
| `Item:Field:278:DD2Summon:273BB840A1EF9C9B` | `DD2Summon` | Field | 278 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public bool DD2Summon;` |
| `Item:Field:280:crit:B91D36FC6E9F5831` | `crit` | Field | 280 | Definition | `ItemCombatDefinition.CriticalChance`; compiler validates non-negative immutable combat metadata; `public int crit;` |
| `Item:Field:282:armorPenetration:6295942A91F72993` | `armorPenetration` | Field | 282 | Definition | `ItemCombatDefinition.ArmorPenetration`; compiler validates non-negative immutable combat metadata; `public int armorPenetration;` |
| `Item:Field:284:bonusTagDamage:B9E24AA90C08888A` | `bonusTagDamage` | Field | 284 | Definition | `Items/Definitions` or immutable type metadata; `public int bonusTagDamage;` |
| `Item:Field:286:prefix:01E328F534D439DD` | `prefix` | Field | 286 | System | `Items/Systems` or deterministic command processing; `public byte prefix;` |
| `Item:Field:288:melee:5B520D2703402022` | `melee` | Field | 288 | Definition | `ItemCombatDefinition.DamageClass = ItemDamageClass.Melee`; immutable enum is compiler-validated; `public bool melee;` |
| `Item:Field:290:magic:08D7C6C1EADB3781` | `magic` | Field | 290 | Definition | `ItemCombatDefinition.DamageClass = ItemDamageClass.Magic`; immutable enum is compiler-validated; `public bool magic;` |
| `Item:Field:292:ranged:C36D4E124A257BB1` | `ranged` | Field | 292 | Definition | `ItemCombatDefinition.DamageClass = ItemDamageClass.Ranged`; immutable enum is compiler-validated; `public bool ranged;` |
| `Item:Field:294:summon:5C250F867887ECC4` | `summon` | Field | 294 | Definition | `ItemCombatDefinition.DamageClass = ItemDamageClass.Summon`; immutable enum is compiler-validated; `public bool summon;` |
| `Item:Field:296:sentry:A1F36BB19B8188AB` | `sentry` | Field | 296 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public bool sentry;` |
| `Item:Field:298:reuseDelay:530DF5C45CDC9C6E` | `reuseDelay` | Field | 298 | Definition | `Items/Definitions` or immutable type metadata; `public int reuseDelay;` |
| `Item:Field:300:newAndShiny:B9DC48420242CEE3` | `newAndShiny` | Field | 300 | Component | `ItemInstanceStateComponent.IsNewAndShiny`; persisted account/world state and V1456 inventory/equipment projection; `public bool newAndShiny;` |
| `Item:Field:303:hasVanityEffects:75FA6F888F2D689E` | `hasVanityEffects` | Field | 303 | Definition | `Items/Definitions` or immutable type metadata; `public bool hasVanityEffects;` |
| `Item:Field:305:foodWidth:77EE57042A10997D` | `foodWidth` | Field | 305 | Definition | `Items/Definitions` or immutable type metadata; `private const int foodWidth = 22;` |
| `Item:Field:307:foodHeight:E0DB9C3F24AE00E1` | `foodHeight` | Field | 307 | Definition | `Items/Definitions` or immutable type metadata; `private const int foodHeight = 22;` |
| `Item:Field:309:WALL_PLACEMENT_USETIME:61A9DE6DEEF4ED78` | `WALL_PLACEMENT_USETIME` | Field | 309 | Definition | `Items/Definitions` or immutable type metadata; `public const int WALL_PLACEMENT_USETIME = 7;` |
| `Item:Field:311:_phaseColors:20837FAD8659E28F` | `_phaseColors` | Field | 311 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `private static Color[] _phaseColors = null;` |
| `Item:Field:313:PickupReplacementTime:5262ED26036EC3CA` | `PickupReplacementTime` | Field | 313 | System | `Items/Systems` or deterministic command processing; `public static readonly int PickupReplacementTime = 1200;` |
| `Item:Field:315:SlotsRemainingBeforeEmergencyStackingInMultiplayer:06422A470E66F8B1` | `SlotsRemainingBeforeEmergencyStackingInMultiplayer` | Field | 315 | Definition | `Items/Definitions` or immutable type metadata; `public static readonly int SlotsRemainingBeforeEmergencyStackingInMultiplayer = 40;` |
| `Item:Property:317:active:59A296ABC96144BC` | `active` | Property | 317 | Component | `ItemStackComponent` empty-stack and `ItemWorldStateComponent.IsActive` lifecycle invariants; `public bool active => type != 0;` |
| `Item:Method:319:Name:9572A077C2258FFB` | `Name` | Method | 319 | Definition | `Items/Definitions` or immutable type metadata; `public string Name => _nameOverride ?? Lang.GetItemNameValue(type);` |
| `Item:Property:322:PaintOrCoating:FF417E27E0175907` | `PaintOrCoating` | Property | 322 | Definition | `Items/Definitions` or immutable type metadata; `public bool PaintOrCoating {` |
| `Item:Property:333:OriginalRarity:032458F87CE7D467` | `OriginalRarity` | Property | 333 | Definition | `ItemDefinition.Rarity`; immutable definition registry retains the legacy base rarity; `public int OriginalRarity => ContentSamples.ItemsByType[type].rare;` |
| `Item:Property:335:OriginalDamage:1447A5509D94B007` | `OriginalDamage` | Property | 335 | Definition | `Items/Definitions` or immutable type metadata; `public int OriginalDamage => ContentSamples.ItemsByType[type].damage;` |
| `Item:Property:337:OriginalDefense:FB2FD0C412515A1B` | `OriginalDefense` | Property | 337 | Definition | `Items/Definitions` or immutable type metadata; `public int OriginalDefense => ContentSamples.ItemsByType[type].defense;` |
| `Item:Property:339:Variant:171879329E50AB75` | `Variant` | Property | 339 | System | `Items/Systems` or deterministic command processing; `public ItemVariant Variant { get; private set; }` |
| `Item:Property:342:IsACoin:1AA9A45548175A9A` | `IsACoin` | Property | 342 | System | `Items/Systems` or deterministic command processing; `public bool IsACoin {` |
| `Item:Property:355:IsAir:1FD868AF472B29BC` | `IsAir` | Property | 355 | System | `Items/Systems` or deterministic command processing; `public bool IsAir {` |
| `Item:Method:365:ToString:FC4ACE8CAF9461F4` | `ToString` | Method | 365 | System | `Items/Systems` or deterministic command processing; `public override string ToString(){` |
| `Item:Method:369:CanHavePrefixes:576F844978FDC569` | `CanHavePrefixes` | Method | 369 | System | `Items/Systems` or deterministic command processing; `public bool CanHavePrefixes() {` |
| `Item:Method:376:Prefix:2DC702937B9A05AC` | `Prefix` | Method | 376 | System | `Items/Systems` or deterministic command processing; `public bool Prefix(int prefixWeWant) {` |
| `Item:Method:384:Prefix:0B76EAA0F364CE5A` | `Prefix` | Method | 384 | System | `Items/Systems` or deterministic command processing; `public bool Prefix(int prefixWeWant, out bool rolledPrefixIsTopTier) {` |
| `Item:Method:502:CanRollPrefix:819F3C4F2AE1492B` | `CanRollPrefix` | Method | 502 | System | `Items/Systems` or deterministic command processing; `public bool CanRollPrefix(int prefix) {` |
| `Item:Method:521:TryGetPrefixStatMultipliersForItem:B42F9C6D640A8646` | `TryGetPrefixStatMultipliersForItem` | Method | 521 | Definition | `Items/Definitions` or immutable type metadata; `public bool TryGetPrefixStatMultipliersForItem(int rolledPrefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt, out int tagdmg, out int arpen, out float value) {` |
| `Item:Method:896:BestPrefixValue:57713C850D5095AB` | `BestPrefixValue` | Method | 896 | Definition | `Items/Definitions` or immutable type metadata; `public float BestPrefixValue() {` |
| `Item:Method:917:GetRollablePrefixes:89BD55F6C3F85574` | `GetRollablePrefixes` | Method | 917 | System | `Items/Systems` or deterministic command processing; `public int[] GetRollablePrefixes() {` |
| `Item:Method:956:RollAPrefix:3DC99C329162ACE5` | `RollAPrefix` | Method | 956 | Definition | `Items/Definitions` or immutable type metadata; `private bool RollAPrefix(UnifiedRandom random, ref int rolledPrefix){` |
| `Item:Method:960:IsAPrefixableAccessory:8DFC615B78219E6C` | `IsAPrefixableAccessory` | Method | 960 | System | `Items/Systems` or deterministic command processing; `public bool IsAPrefixableAccessory() {` |
| `Item:Method:971:CanBeEquipped:88D4403FAE2340FD` | `CanBeEquipped` | Method | 971 | System | `Items/Systems` or deterministic command processing; `public bool CanBeEquipped() {` |
| `Item:Method:990:OnlyNeedOneInInventory:90253A3C0E1ACC0C` | `OnlyNeedOneInInventory` | Method | 990 | System | `Items/Systems` or deterministic command processing; `public bool OnlyNeedOneInInventory() {` |
| `Item:Method:1010:AffixName:ECE1ACCC3B7770A7` | `AffixName` | Method | 1010 | Definition | `Items/Definitions` or immutable type metadata; `public string AffixName() {` |
| `Item:Method:1021:RebuildTooltip:C946CCF4B011CA72` | `RebuildTooltip` | Method | 1021 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public void RebuildTooltip() {` |
| `Item:Method:1031:netDefaults:EA6C71ACC200246E` | `netDefaults` | Method | 1031 | System | `Items/Systems` or deterministic command processing; `public void netDefaults(int type) {` |
| `Item:Method:1191:FitsAmmoSlot:CE882BFD58B2653F` | `FitsAmmoSlot` | Method | 1191 | Definition | `Items/Definitions` or immutable type metadata; `public bool FitsAmmoSlot() {` |
| `Item:Method:1202:CanFillEmptyAmmoSlot:C3303E727AE454A6` | `CanFillEmptyAmmoSlot` | Method | 1202 | System | `Items/Systems` or deterministic command processing; `public bool CanFillEmptyAmmoSlot() {` |
| `Item:Method:1213:SetDefaults1:825CB8496E33940A` | `SetDefaults1` | Method | 1213 | System | `Items/Systems` or deterministic command processing; `public void SetDefaults1(int type) {` |
| `Item:Method:12758:SetDefaults2:BB77B2D350B37F49` | `SetDefaults2` | Method | 12758 | System | `Items/Systems` or deterministic command processing; `public void SetDefaults2(int type) {` |
| `Item:Method:21203:SetDefaults3:2E0DE1C064A2C301` | `SetDefaults3` | Method | 21203 | System | `Items/Systems` or deterministic command processing; `public void SetDefaults3(int type) {` |
| `Item:Method:27475:DefaultToQuestFish:DB6107B05934F8E9` | `DefaultToQuestFish` | Method | 27475 | System | `Items/Systems` or deterministic command processing; `public void DefaultToQuestFish() {` |
| `Item:Method:27486:SetDefaults4:A16B7B85019B0964` | `SetDefaults4` | Method | 27486 | System | `Items/Systems` or deterministic command processing; `public void SetDefaults4(int type) {` |
| `Item:Method:35274:DefaultToGolfBall:BC59E29FA0EABEB0` | `DefaultToGolfBall` | Method | 35274 | System | `Items/Systems` or deterministic command processing; `public void DefaultToGolfBall(int projid) {` |
| `Item:Method:35294:SetDefaults5:2C804CAA488727FD` | `SetDefaults5` | Method | 35294 | System | `Items/Systems` or deterministic command processing; `public void SetDefaults5(int type) {` |
| `Item:Method:47240:DefaultToBanner:E5F3A95F51781206` | `DefaultToBanner` | Method | 47240 | System | `Items/Systems` or deterministic command processing; `public void DefaultToBanner(int tileStyleToPlace = 0) {` |
| `Item:Method:47250:DefaultToMonolith:A44862A847D92438` | `DefaultToMonolith` | Method | 47250 | System | `Items/Systems` or deterministic command processing; `public void DefaultToMonolith(int tileIDToPlace, int tileStyleToPlace = 0) {` |
| `Item:Method:47261:DefaultToBomb:1E399931D591430A` | `DefaultToBomb` | Method | 47261 | System | `Items/Systems` or deterministic command processing; `public void DefaultToBomb(int projectileID, float throwSpeed) {` |
| `Item:Method:47278:DefaultToVoiceOverrideAccessory:7F8885629A20EB6F` | `DefaultToVoiceOverrideAccessory` | Method | 47278 | System | `Items/Systems` or deterministic command processing; `public void DefaultToVoiceOverrideAccessory(sbyte voiceOverrideID) {` |
| `Item:Method:47294:DefaultToSolution:81728B95AADBD275` | `DefaultToSolution` | Method | 47294 | System | `Items/Systems` or deterministic command processing; `public void DefaultToSolution(int projectileId) {` |
| `Item:Method:47307:DefaultToWhip:00AC983FB2872678` | `DefaultToWhip` | Method | 47307 | System | `Items/Systems` or deterministic command processing; `public void DefaultToWhip(int projectileId, int dmg, float kb, float shootspeed, int animationTotalTime = 30) {` |
| `Item:Method:47327:DefaultToKite:DCA8BC03193283FE` | `DefaultToKite` | Method | 47327 | System | `Items/Systems` or deterministic command processing; `public void DefaultToKite(int projId) {` |
| `Item:Method:47341:DefaultToVanitypet:3B91524376274549` | `DefaultToVanitypet` | Method | 47341 | System | `Items/Systems` or deterministic command processing; `public void DefaultToVanitypet(int projId, int buffID) {` |
| `Item:Method:47359:IsAGolfingItem:B23A8D09F9ADF69A` | `IsAGolfingItem` | Method | 47359 | System | `Items/Systems` or deterministic command processing; `public static bool IsAGolfingItem(Item item) {` |
| `Item:Method:47375:DefaultToSeaShell:260BBC200C2DF6C2` | `DefaultToSeaShell` | Method | 47375 | System | `Items/Systems` or deterministic command processing; `private void DefaultToSeaShell() {` |
| `Item:Method:47411:DefaultToCapturedCritter:152792F41C511302` | `DefaultToCapturedCritter` | Method | 47411 | System | `Items/Systems` or deterministic command processing; `public void DefaultToCapturedCritter(short npcIdToSpawnOnUse) {` |
| `Item:Method:47428:DefaultToStaff:4C262B43931D89C1` | `DefaultToStaff` | Method | 47428 | System | `Items/Systems` or deterministic command processing; `public void DefaultToStaff(int projType, float pushForwardSpeed, int singleShotTime, int manaPerShot) {` |
| `Item:Method:47439:DefaultToSpear:C99165CD5388C9B0` | `DefaultToSpear` | Method | 47439 | System | `Items/Systems` or deterministic command processing; `public void DefaultToSpear(int projType, float pushForwardSpeed, int animationTime) {` |
| `Item:Method:47457:SetFoodDefaults:0E78CBC99D0189E8` | `SetFoodDefaults` | Method | 47457 | System | `Items/Systems` or deterministic command processing; `private void SetFoodDefaults(int type) {` |
| `Item:Method:47816:DefaultToMinecart:3644C297CE1E0D3A` | `DefaultToMinecart` | Method | 47816 | System | `Items/Systems` or deterministic command processing; `public void DefaultToMinecart(int mount) {` |
| `Item:Method:47825:DefaultToPlaceableWall:4DC004DE39C7D58A` | `DefaultToPlaceableWall` | Method | 47825 | System | `Items/Systems` or deterministic command processing; `public void DefaultToPlaceableWall(ushort wallToPlace) {` |
| `Item:Method:47840:SetWeaponValues:FB47374D59FF9116` | `SetWeaponValues` | Method | 47840 | System | `Items/Systems` or deterministic command processing; `public void SetWeaponValues(int dmg, float knockback, int bonusCritChance = 0) {` |
| `Item:Method:47849:DefaultToBow:D5462DA057B1B2F3` | `DefaultToBow` | Method | 47849 | System | `Items/Systems` or deterministic command processing; `public void DefaultToBow(int singleShotTime, float shotVelocity, bool hasAutoReuse = false) {` |
| `Item:Method:47859:DefaultToMagicWeapon:7E51081D85E5E718` | `DefaultToMagicWeapon` | Method | 47859 | System | `Items/Systems` or deterministic command processing; `public void DefaultToMagicWeapon(int projType, int singleShotTime, float shotVelocity, bool hasAutoReuse = false) {` |
| `Item:Method:47873:DefaultToRangedWeapon:E4E9576269023E88` | `DefaultToRangedWeapon` | Method | 47873 | System | `Items/Systems` or deterministic command processing; `public void DefaultToRangedWeapon(int baseProjType, int ammoID, int singleShotTime, float shotVelocity, bool hasAutoReuse = false) {` |
| `Item:Method:47888:DefaultToThrownWeapon:CE3DEF8745F64C76` | `DefaultToThrownWeapon` | Method | 47888 | System | `Items/Systems` or deterministic command processing; `public void DefaultToThrownWeapon(int baseProjType, int singleShotTime, float shotVelocity, bool hasAutoReuse = false) {` |
| `Item:Method:47903:DefaultToTorch:ABEB1D8A7B347AFA` | `DefaultToTorch` | Method | 47903 | System | `Items/Systems` or deterministic command processing; `private void DefaultToTorch(int tileStyleToPlace, bool allowWaterPlacement = false) {` |
| `Item:Method:47923:DefaultToPlaceableTile:D95C333D93EDBE03` | `DefaultToPlaceableTile` | Method | 47923 | System | `Items/Systems` or deterministic command processing; `public void DefaultToPlaceableTile(int tileIDToPlace, int tileStyleToPlace = 0) {` |
| `Item:Method:47930:DefaultToPlaceableTile:B8B32B7DF3995245` | `DefaultToPlaceableTile` | Method | 47930 | System | `Items/Systems` or deterministic command processing; `public void DefaultToPlaceableTile(ushort tileIDToPlace, int tileStyleToPlace = 0) {` |
| `Item:Method:47946:MakeUsableWithChlorophyteExtractinator:260E889120941D54` | `MakeUsableWithChlorophyteExtractinator` | Method | 47946 | System | `Items/Systems/ExtractinatorSystem` and `DomeSimulation` own target validation, deterministic output and atomic consumption; legacy method body is not imported. |
| `Item:Method:47958:DefaultToGolfClub:01A8886B78F97A84` | `DefaultToGolfClub` | Method | 47958 | System | `Items/Systems` or deterministic command processing; `public void DefaultToGolfClub(int newwidth, int newheight) {` |
| `Item:Method:47974:DefaultToLawnMower:66C35109A0F73C6E` | `DefaultToLawnMower` | Method | 47974 | System | `Items/Systems` or deterministic command processing; `public void DefaultToLawnMower(int newwidth, int newheight) {` |
| `Item:Method:47988:DefaultToFood:EE06CA39D48B655C` | `DefaultToFood` | Method | 47988 | System | `Items/Systems` or deterministic command processing; `public void DefaultToFood(int newwidth, int newheight, int foodbuff, int foodbuffduration, bool useGulpSound = false, int animationTime = 17) {` |
| `Item:Method:48019:DefaultToHealingPotion:A5C3F26D5DD4348E` | `DefaultToHealingPotion` | Method | 48019 | System | `Items/Systems` or deterministic command processing; `public void DefaultToHealingPotion(int newwidth, int newheight, int healingAmount, int animationTime = 17) {` |
| `Item:Method:48036:SetShopValues:31CBFFFFA0118679` | `SetShopValues` | Method | 48036 | System | `Items/Systems` or deterministic command processing; `public void SetShopValues(ItemRarityColor rarity, int coinValue) {` |
| `Item:Method:48044:DefaultToHeadgear:63ACA66186E32FE5` | `DefaultToHeadgear` | Method | 48044 | System | `Items/Systems` or deterministic command processing; `public void DefaultToHeadgear(int newwidth, int newheight, int helmetArtID) {` |
| `Item:Method:48053:DefaultToBody:9705773685539578` | `DefaultToBody` | Method | 48053 | System | `Items/Systems` or deterministic command processing; `public void DefaultToBody(int newwidth, int newheight, int bodySlotID) {` |
| `Item:Method:48062:DefaultToLegs:433057A597AEC9EE` | `DefaultToLegs` | Method | 48062 | System | `Items/Systems` or deterministic command processing; `public void DefaultToLegs(int newwidth, int newheight, int legSlotID) {` |
| `Item:Method:48071:DefaultToAccessory:CD66A05C09820CA5` | `DefaultToAccessory` | Method | 48071 | System | `Items/Systems` or deterministic command processing; `public void DefaultToAccessory(int newwidth = 24, int newheight = 24) {` |
| `Item:Method:48080:DefaultToInfoAccessory:9DE4D54AD7D93BBF` | `DefaultToInfoAccessory` | Method | 48080 | System | `Items/Systems` or deterministic command processing; `public void DefaultToInfoAccessory(int newwidth = 24, int newheight = 24) {` |
| `Item:Method:48088:DefaultToGuitar:19C9C40250C20DBE` | `DefaultToGuitar` | Method | 48088 | System | `Items/Systems` or deterministic command processing; `public void DefaultToGuitar(int newwidth = 24, int newheight = 24) {` |
| `Item:Method:48100:DefaultToMusicBox:003928CCF1917428` | `DefaultToMusicBox` | Method | 48100 | System | `Items/Systems` or deterministic command processing; `public void DefaultToMusicBox(int style) {` |
| `Item:Method:48121:SetDefaults:9B77E8B6D4D10E21` | `SetDefaults` | Method | 48121 | System | `Items/Systems` or deterministic command processing; `public void SetDefaults(int Type, ItemVariant variant = null) {` |
| `Item:Method:48388:ResetStats:54A9FD6AF6B0BBA4` | `ResetStats` | Method | 48388 | System | `Items/Systems` or deterministic command processing; `public void ResetStats(int Type) {` |
| `Item:Method:48499:GetPhaseColor:8F173D3FBCFE92E1` | `GetPhaseColor` | Method | 48499 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public static Color GetPhaseColor(int projectileType, bool drawColor = false) {` |
| `Item:Method:48559:GetPhaseColorDirect:EEB3B7D9C1A143D4` | `GetPhaseColorDirect` | Method | 48559 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `private static Color GetPhaseColorDirect(int projectileType){` |
| `Item:Method:48563:MechSpawn:106B65E512768490` | `MechSpawn` | Method | 48563 | Definition | `Items/Definitions` or immutable type metadata; `public static bool MechSpawn(float x, float y, int type) {` |
| `Item:Method:48597:buyPrice:534B8BDEF8A9B519` | `buyPrice` | Method | 48597 | Definition | `Items/Definitions` or immutable type metadata; `public static int buyPrice(int platinum = 0, int gold = 0, int silver = 0, int copper = 0) {` |
| `Item:Method:48604:sellPrice:51906EBD49629584` | `sellPrice` | Method | 48604 | System | `ItemPriceSystem.CalculateSellPrice` preserves the authoritative five-times buy-price conversion with overflow validation; `public static int sellPrice(int platinum = 0, int gold = 0, int silver = 0, int copper = 0) {` |
| `Item:Method:48611:GetRandomVoiceItem:062B756419E42457` | `GetRandomVoiceItem` | Method | 48611 | System | `Items/Systems` or deterministic command processing; `public static int GetRandomVoiceItem() {` |
| `Item:Method:48634:FixAgainstExploit:BCA10395AEE265BA` | `FixAgainstExploit` | Method | 48634 | System | `ItemInventorySanitizationSystem` normalizes restored/server-owned inventory stacks and definition-ineligible prefixes; `public void FixAgainstExploit() {` |
| `Item:Method:48653:CanPassivelyStackInWorld:B737B43AE81BBDDF` | `CanPassivelyStackInWorld` | Method | 48653 | System | `Items/Systems` or deterministic command processing; `public bool CanPassivelyStackInWorld() {` |
| `Item:Method:48677:GetDrawHitbox:B6CD1F1D80F315D5` | `GetDrawHitbox` | Method | 48677 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public static Rectangle GetDrawHitbox(int type, Player user) {` |
| `Item:Method:48687:NewItem:87A05540D9CBBFD2` | `NewItem` | Method | 48687 | Compatibility | `Items/Compatibility` boundary adapter; `public static int NewItem(IEntitySource source, Vector2 pos, Vector2 randomBox, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false) {` |
| `Item:Method:48694:NewItem:9C815598E2CE4387` | `NewItem` | Method | 48694 | Compatibility | `Items/Compatibility` boundary adapter; `public static int NewItem(IEntitySource source, int X, int Y, int Width, int Height, int Type, int Stack = 1, bool noBroadcast = false, int pfix = 0, bool noGrabDelay = false) {` |
| `Item:Method:48801:PickAnItemSlotToSpawnItemOn:A21CCD075A861202` | `PickAnItemSlotToSpawnItemOn` | Method | 48801 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `private static int PickAnItemSlotToSpawnItemOn() {` |
| `Item:Method:48863:Clone:8355CB935C6F6A72` | `Clone` | Method | 48863 | System | `Items/Systems` or deterministic command processing; `public Item Clone() {` |
| `Item:Method:48870:CanStack:E35EB0C993C69FD7` | `CanStack` | Method | 48870 | System | `Items/Systems` or deterministic command processing; `public static bool CanStack(Item item1, Item item2) {` |
| `Item:Method:48881:TurnToAir:FD4E43E45582AF98` | `TurnToAir` | Method | 48881 | System | `Items/Systems` or deterministic command processing; `public void TurnToAir(bool fullReset = false) {` |
| `Item:Method:48901:ResetPrefix:BD10AAACD7C9811E` | `ResetPrefix` | Method | 48901 | System | `Items/Systems` or deterministic command processing; `public void ResetPrefix() {` |
| `Item:Method:48912:Refresh:70900F59E371F577` | `Refresh` | Method | 48912 | Deferred | UI/rendering/legacy host concern; deferred until owner is assigned; `public void Refresh(bool onlyIfVariantChanged = true) {` |

## 排除与延期规则

- UI、纹理、Shader、音效、Tooltip、Bestiary 和依赖 `Main`/客户端全局状态的成员只能进入 `Deferred` 或 `Compatibility`。
- 任何尚未有实现或验证证据的成员不得标记为 `migrated`; 本表的初始归属不是完成声明。
- 成员索引和版本差异的重放输入分别为 `item-members.json` 和 `version3-version4-file-diff.txt`。
- 28 个 `Deferred` 成员的逐项 Version4 调用链、目标 owner 和排除理由见
  `Build/evidence/item-ecs/deferred-member-audit.md`；该审计不会将只有声明、没有当前
  Simulation 行为承载面的成员伪装为已迁移。

## Task 8 审计状态（2026-08-19）

成员表已经为每个成员分配了 `Definition`、`System`、`Compatibility` 或 `Deferred` 归属，
因此不存在未分类成员。归属不等于行为已经迁移：本表仍保持 `PARTIAL`，直到对应运行时路径和
最终证据完成。`Compatibility` 成员的当前边界实现位于：

- `src/Terraria.Dome.Simulation/Items/Compatibility/LegacyItemDefinitionAdapter.cs`
- `src/Terraria.Dome.Simulation/Items/Compatibility/LegacyItemImportAdapter.cs`
- `src/Terraria.Dome.Simulation/Items/Compatibility/LegacyItemDropAdapter.cs`

精确 legacy 引用查询及其零命中结果记录在
`Build/evidence/item-ecs/legacy-item-reference-audit.txt`。该审计不宣称 Version4 源文件已删除，
也不宣称 `Deferred` 的 UI、渲染和客户端成员已经实现。

`Build/evidence/item-ecs/deferred-member-audit.md` 为每个 Deferred 成员保留了具体 owner 和
原因，其中 `noMelee`、`cartTrack`、`DD2Summon`、`sentry` 和 Extractinator 明确属于尚未建立的
服务器行为域，而非 UI 标签。

