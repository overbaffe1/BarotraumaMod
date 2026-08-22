using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000322 RID: 802
	internal class MapGenerationParams : Prefab, ISerializableEntity
	{
		// Token: 0x170010EF RID: 4335
		// (get) Token: 0x06004014 RID: 16404 RVA: 0x0023C72C File Offset: 0x0023A92C
		public static MapGenerationParams Instance
		{
			get
			{
				return MapGenerationParams.Params.ActivePrefab;
			}
		}

		// Token: 0x170010F0 RID: 4336
		// (get) Token: 0x06004015 RID: 16405 RVA: 0x0023C738 File Offset: 0x0023A938
		// (set) Token: 0x06004016 RID: 16406 RVA: 0x0023C740 File Offset: 0x0023A940
		[Serialize(6, IsPropertySaveable.Yes, "", "", false)]
		public int DifficultyZones { get; set; }

		// Token: 0x170010F1 RID: 4337
		// (get) Token: 0x06004017 RID: 16407 RVA: 0x0023C749 File Offset: 0x0023A949
		// (set) Token: 0x06004018 RID: 16408 RVA: 0x0023C751 File Offset: 0x0023A951
		[Serialize(8000, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int Width { get; set; }

		// Token: 0x170010F2 RID: 4338
		// (get) Token: 0x06004019 RID: 16409 RVA: 0x0023C75A File Offset: 0x0023A95A
		// (set) Token: 0x0600401A RID: 16410 RVA: 0x0023C762 File Offset: 0x0023A962
		[Serialize(500, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int Height { get; set; }

		// Token: 0x170010F3 RID: 4339
		// (get) Token: 0x0600401B RID: 16411 RVA: 0x0023C76B File Offset: 0x0023A96B
		// (set) Token: 0x0600401C RID: 16412 RVA: 0x0023C773 File Offset: 0x0023A973
		[Serialize(20f, IsPropertySaveable.Yes, "Connections with a length smaller or equal to this generate the smallest possible levels (using the MinWidth parameter in the level generation paramaters).", "", false)]
		[Editable(0f, 5000f, 1)]
		public float SmallLevelConnectionLength { get; set; }

		// Token: 0x170010F4 RID: 4340
		// (get) Token: 0x0600401D RID: 16413 RVA: 0x0023C77C File Offset: 0x0023A97C
		// (set) Token: 0x0600401E RID: 16414 RVA: 0x0023C784 File Offset: 0x0023A984
		[Serialize(200f, IsPropertySaveable.Yes, "Connections with a length larger or equal to this generate the largest possible levels (using the MaxWidth parameter in the level generation paramaters).", "", false)]
		[Editable(0f, 5000f, 1)]
		public float LargeLevelConnectionLength { get; set; }

		// Token: 0x170010F5 RID: 4341
		// (get) Token: 0x0600401F RID: 16415 RVA: 0x0023C78D File Offset: 0x0023A98D
		// (set) Token: 0x06004020 RID: 16416 RVA: 0x0023C795 File Offset: 0x0023A995
		[Serialize("20,20", IsPropertySaveable.Yes, "How far from each other voronoi sites are placed. Sites determine shape of the voronoi graph. Locations are placed at the vertices of the voronoi cells. (Decreasing this value causes the number of sites, and the complexity of the map, to increase exponentially - be careful when adjusting)", "", false)]
		[Editable]
		public Point VoronoiSiteInterval { get; set; }

		// Token: 0x170010F6 RID: 4342
		// (get) Token: 0x06004021 RID: 16417 RVA: 0x0023C79E File Offset: 0x0023A99E
		// (set) Token: 0x06004022 RID: 16418 RVA: 0x0023C7A6 File Offset: 0x0023A9A6
		[Serialize("5,5", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Point VoronoiSiteVariance { get; set; }

		// Token: 0x170010F7 RID: 4343
		// (get) Token: 0x06004023 RID: 16419 RVA: 0x0023C7AF File Offset: 0x0023A9AF
		// (set) Token: 0x06004024 RID: 16420 RVA: 0x0023C7B7 File Offset: 0x0023A9B7
		[Serialize(10f, IsPropertySaveable.Yes, "Connections smaller than this are removed.", "", false)]
		[Editable(0f, 500f, 1)]
		public float MinConnectionDistance { get; set; }

		// Token: 0x170010F8 RID: 4344
		// (get) Token: 0x06004025 RID: 16421 RVA: 0x0023C7C0 File Offset: 0x0023A9C0
		// (set) Token: 0x06004026 RID: 16422 RVA: 0x0023C7C8 File Offset: 0x0023A9C8
		[Serialize(5f, IsPropertySaveable.Yes, "Locations that are closer than this to another location are removed.", "", false)]
		[Editable(0f, 100f, 1)]
		public float MinLocationDistance { get; set; }

		// Token: 0x170010F9 RID: 4345
		// (get) Token: 0x06004027 RID: 16423 RVA: 0x0023C7D1 File Offset: 0x0023A9D1
		// (set) Token: 0x06004028 RID: 16424 RVA: 0x0023C7D9 File Offset: 0x0023A9D9
		[Serialize(0.1f, IsPropertySaveable.Yes, "ConnectionIterationMultiplier for the UI indicator lines between locations.", "", false)]
		[Editable(0f, 10f, 1, DecimalCount = 2)]
		public float ConnectionIndicatorIterationMultiplier { get; set; }

		// Token: 0x170010FA RID: 4346
		// (get) Token: 0x06004029 RID: 16425 RVA: 0x0023C7E2 File Offset: 0x0023A9E2
		// (set) Token: 0x0600402A RID: 16426 RVA: 0x0023C7EA File Offset: 0x0023A9EA
		[Serialize(0.1f, IsPropertySaveable.Yes, "ConnectionDisplacementMultiplier for the UI indicator lines between locations.", "", false)]
		[Editable(0f, 10f, 1, DecimalCount = 2)]
		public float ConnectionIndicatorDisplacementMultiplier { get; set; }

		// Token: 0x170010FB RID: 4347
		// (get) Token: 0x0600402B RID: 16427 RVA: 0x0023C7F3 File Offset: 0x0023A9F3
		// (set) Token: 0x0600402C RID: 16428 RVA: 0x0023C7FB File Offset: 0x0023A9FB
		[Serialize(0.75f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float MinZoom { get; set; }

		// Token: 0x170010FC RID: 4348
		// (get) Token: 0x0600402D RID: 16429 RVA: 0x0023C804 File Offset: 0x0023AA04
		// (set) Token: 0x0600402E RID: 16430 RVA: 0x0023C80C File Offset: 0x0023AA0C
		[Serialize(1.5f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float MaxZoom { get; set; }

		// Token: 0x170010FD RID: 4349
		// (get) Token: 0x0600402F RID: 16431 RVA: 0x0023C815 File Offset: 0x0023AA15
		// (set) Token: 0x06004030 RID: 16432 RVA: 0x0023C81D File Offset: 0x0023AA1D
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float MapTileScale { get; set; }

		// Token: 0x170010FE RID: 4350
		// (get) Token: 0x06004031 RID: 16433 RVA: 0x0023C826 File Offset: 0x0023AA26
		// (set) Token: 0x06004032 RID: 16434 RVA: 0x0023C82E File Offset: 0x0023AA2E
		[Serialize(15f, IsPropertySaveable.Yes, "Size of the location icons in pixels when at 100% zoom.", "", false)]
		[Editable(1f, 1000f, 1)]
		public float LocationIconSize { get; set; }

		// Token: 0x170010FF RID: 4351
		// (get) Token: 0x06004033 RID: 16435 RVA: 0x0023C837 File Offset: 0x0023AA37
		// (set) Token: 0x06004034 RID: 16436 RVA: 0x0023C83F File Offset: 0x0023AA3F
		[Serialize(5f, IsPropertySaveable.Yes, "Width of the connections between locations, in pixels when at 100% zoom.", "", false)]
		[Editable(1f, 1000f, 1)]
		public float LocationConnectionWidth { get; set; }

		// Token: 0x17001100 RID: 4352
		// (get) Token: 0x06004035 RID: 16437 RVA: 0x0023C848 File Offset: 0x0023AA48
		// (set) Token: 0x06004036 RID: 16438 RVA: 0x0023C850 File Offset: 0x0023AA50
		[Serialize("220,220,100,255", IsPropertySaveable.Yes, "The color used to display the indicators (current location, selected location, etc).", "", false)]
		[Editable]
		public Color IndicatorColor { get; set; }

		// Token: 0x17001101 RID: 4353
		// (get) Token: 0x06004037 RID: 16439 RVA: 0x0023C859 File Offset: 0x0023AA59
		// (set) Token: 0x06004038 RID: 16440 RVA: 0x0023C861 File Offset: 0x0023AA61
		[Serialize("150,150,150,255", IsPropertySaveable.Yes, "The color used to display the connections between locations.", "", false)]
		[Editable]
		public Color ConnectionColor { get; set; }

		// Token: 0x17001102 RID: 4354
		// (get) Token: 0x06004039 RID: 16441 RVA: 0x0023C86A File Offset: 0x0023AA6A
		// (set) Token: 0x0600403A RID: 16442 RVA: 0x0023C872 File Offset: 0x0023AA72
		[Serialize("150,150,150,255", IsPropertySaveable.Yes, "The color used to display the connections between locations when they're highlighted.", "", false)]
		[Editable]
		public Color HighlightedConnectionColor { get; set; }

		// Token: 0x17001103 RID: 4355
		// (get) Token: 0x0600403B RID: 16443 RVA: 0x0023C87B File Offset: 0x0023AA7B
		// (set) Token: 0x0600403C RID: 16444 RVA: 0x0023C883 File Offset: 0x0023AA83
		[Serialize("150,150,150,255", IsPropertySaveable.Yes, "The color used to display the connections the player hasn't travelled through.", "", false)]
		[Editable]
		public Color UnvisitedConnectionColor { get; set; }

		// Token: 0x17001104 RID: 4356
		// (get) Token: 0x0600403D RID: 16445 RVA: 0x0023C88C File Offset: 0x0023AA8C
		// (set) Token: 0x0600403E RID: 16446 RVA: 0x0023C894 File Offset: 0x0023AA94
		public Sprite ConnectionSprite { get; private set; }

		// Token: 0x17001105 RID: 4357
		// (get) Token: 0x0600403F RID: 16447 RVA: 0x0023C89D File Offset: 0x0023AA9D
		// (set) Token: 0x06004040 RID: 16448 RVA: 0x0023C8A5 File Offset: 0x0023AAA5
		public Sprite PassedConnectionSprite { get; private set; }

		// Token: 0x17001106 RID: 4358
		// (get) Token: 0x06004041 RID: 16449 RVA: 0x0023C8AE File Offset: 0x0023AAAE
		// (set) Token: 0x06004042 RID: 16450 RVA: 0x0023C8B6 File Offset: 0x0023AAB6
		public SpriteSheet DecorativeGraphSprite { get; private set; }

		// Token: 0x17001107 RID: 4359
		// (get) Token: 0x06004043 RID: 16451 RVA: 0x0023C8BF File Offset: 0x0023AABF
		// (set) Token: 0x06004044 RID: 16452 RVA: 0x0023C8C7 File Offset: 0x0023AAC7
		public Sprite MissionIcon { get; private set; }

		// Token: 0x17001108 RID: 4360
		// (get) Token: 0x06004045 RID: 16453 RVA: 0x0023C8D0 File Offset: 0x0023AAD0
		// (set) Token: 0x06004046 RID: 16454 RVA: 0x0023C8D8 File Offset: 0x0023AAD8
		public Sprite TypeChangeIcon { get; private set; }

		// Token: 0x17001109 RID: 4361
		// (get) Token: 0x06004047 RID: 16455 RVA: 0x0023C8E1 File Offset: 0x0023AAE1
		// (set) Token: 0x06004048 RID: 16456 RVA: 0x0023C8E9 File Offset: 0x0023AAE9
		public Sprite FogOfWarSprite { get; private set; }

		// Token: 0x1700110A RID: 4362
		// (get) Token: 0x06004049 RID: 16457 RVA: 0x0023C8F2 File Offset: 0x0023AAF2
		// (set) Token: 0x0600404A RID: 16458 RVA: 0x0023C8FA File Offset: 0x0023AAFA
		public Sprite CurrentLocationIndicator { get; private set; }

		// Token: 0x1700110B RID: 4363
		// (get) Token: 0x0600404B RID: 16459 RVA: 0x0023C903 File Offset: 0x0023AB03
		// (set) Token: 0x0600404C RID: 16460 RVA: 0x0023C90B File Offset: 0x0023AB0B
		public Sprite SelectedLocationIndicator { get; private set; }

		// Token: 0x1700110C RID: 4364
		// (get) Token: 0x0600404D RID: 16461 RVA: 0x0023C914 File Offset: 0x0023AB14
		public string Name
		{
			get
			{
				return base.GetType().ToString();
			}
		}

		// Token: 0x1700110D RID: 4365
		// (get) Token: 0x0600404E RID: 16462 RVA: 0x0023C921 File Offset: 0x0023AB21
		// (set) Token: 0x0600404F RID: 16463 RVA: 0x0023C929 File Offset: 0x0023AB29
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06004050 RID: 16464 RVA: 0x0023C934 File Offset: 0x0023AB34
		public MapGenerationParams(ContentXElement element, MapGenerationParametersFile file) : base(file, file.Path.Value.ToIdentifier())
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			int[] gateCount = element.GetAttributeIntArray("gatecount", null) ?? element.GetAttributeIntArray("GateCount", null);
			if (gateCount == null)
			{
				gateCount = new int[this.DifficultyZones];
				for (int i = 0; i < this.DifficultyZones; i++)
				{
					gateCount[i] = 1;
				}
			}
			this.GateCount = gateCount.ToImmutableArray<int>();
			Dictionary<Identifier, List<Sprite>> mapTiles = new Dictionary<Identifier, List<Sprite>>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 7:
					{
						if (!(text == "maptile"))
						{
							continue;
						}
						Identifier biome = subElement.GetAttributeIdentifier("biome", "");
						if (!mapTiles.ContainsKey(biome))
						{
							mapTiles[biome] = new List<Sprite>();
						}
						mapTiles[biome].Add(new Sprite(subElement, "", "", false, 1f));
						continue;
					}
					case 8:
					case 9:
					case 10:
					case 12:
					case 13:
					case 18:
					case 19:
					case 20:
					case 23:
						continue;
					case 11:
						if (!(text == "missionicon"))
						{
							continue;
						}
						this.MissionIcon = new Sprite(subElement, "", "", false, 1f);
						continue;
					case 14:
					{
						char c = text[0];
						if (c != 'f')
						{
							if (c != 't')
							{
								continue;
							}
							if (!(text == "typechangeicon"))
							{
								continue;
							}
							this.TypeChangeIcon = new Sprite(subElement, "", "", false, 1f);
							continue;
						}
						else
						{
							if (!(text == "fogofwarsprite"))
							{
								continue;
							}
							this.FogOfWarSprite = new Sprite(subElement, "", "", false, 1f);
							continue;
						}
						break;
					}
					case 15:
						if (!(text == "radiationparams"))
						{
							continue;
						}
						this.RadiationParams = new RadiationParams(subElement);
						continue;
					case 16:
						if (!(text == "connectionsprite"))
						{
							continue;
						}
						this.ConnectionSprite = new Sprite(subElement, "", "", false, 1f);
						continue;
					case 17:
						if (!(text == "locationindicator"))
						{
							continue;
						}
						break;
					case 21:
						if (!(text == "decorativegraphsprite"))
						{
							continue;
						}
						this.DecorativeGraphSprite = new SpriteSheet(subElement, "", "");
						continue;
					case 22:
						if (!(text == "passedconnectionsprite"))
						{
							continue;
						}
						this.PassedConnectionSprite = new Sprite(subElement, "", "", false, 1f);
						continue;
					case 24:
						if (!(text == "currentlocationindicator"))
						{
							continue;
						}
						break;
					case 25:
						if (!(text == "selectedlocationindicator"))
						{
							continue;
						}
						this.SelectedLocationIndicator = new Sprite(subElement, "", "", false, 1f);
						continue;
					default:
						continue;
					}
					this.CurrentLocationIndicator = new Sprite(subElement, "", "", false, 1f);
				}
			}
			this.MapTiles = (from kvp in mapTiles
			select new ValueTuple<Identifier, ImmutableArray<Sprite>>(kvp.Key, kvp.Value.ToImmutableArray<Sprite>())).ToImmutableDictionary<Identifier, ImmutableArray<Sprite>>();
		}

		// Token: 0x06004051 RID: 16465 RVA: 0x0023CD3C File Offset: 0x0023AF3C
		public override void Dispose()
		{
			Sprite connectionSprite = this.ConnectionSprite;
			if (connectionSprite != null)
			{
				connectionSprite.Remove();
			}
			Sprite passedConnectionSprite = this.PassedConnectionSprite;
			if (passedConnectionSprite != null)
			{
				passedConnectionSprite.Remove();
			}
			Sprite selectedLocationIndicator = this.SelectedLocationIndicator;
			if (selectedLocationIndicator != null)
			{
				selectedLocationIndicator.Remove();
			}
			Sprite currentLocationIndicator = this.CurrentLocationIndicator;
			if (currentLocationIndicator != null)
			{
				currentLocationIndicator.Remove();
			}
			SpriteSheet decorativeGraphSprite = this.DecorativeGraphSprite;
			if (decorativeGraphSprite != null)
			{
				decorativeGraphSprite.Remove();
			}
			Sprite missionIcon = this.MissionIcon;
			if (missionIcon != null)
			{
				missionIcon.Remove();
			}
			Sprite typeChangeIcon = this.TypeChangeIcon;
			if (typeChangeIcon != null)
			{
				typeChangeIcon.Remove();
			}
			Sprite fogOfWarSprite = this.FogOfWarSprite;
			if (fogOfWarSprite != null)
			{
				fogOfWarSprite.Remove();
			}
			foreach (ImmutableArray<Sprite> spriteList in this.MapTiles.Values)
			{
				foreach (Sprite sprite in spriteList)
				{
					sprite.Remove();
				}
			}
		}

		// Token: 0x04002161 RID: 8545
		public static readonly PrefabSelector<MapGenerationParams> Params = new PrefabSelector<MapGenerationParams>();

		// Token: 0x04002162 RID: 8546
		public readonly bool ShowLocations = true;

		// Token: 0x04002163 RID: 8547
		public readonly bool ShowLevelTypeNames;

		// Token: 0x04002164 RID: 8548
		public readonly bool ShowOverlay = true;

		// Token: 0x04002170 RID: 8560
		public readonly ImmutableArray<int> GateCount;

		// Token: 0x04002182 RID: 8578
		public readonly ImmutableDictionary<Identifier, ImmutableArray<Sprite>> MapTiles;

		// Token: 0x04002184 RID: 8580
		public RadiationParams RadiationParams;
	}
}
