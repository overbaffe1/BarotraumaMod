using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000320 RID: 800
	internal class LocationType : PrefabWithUintIdentifier
	{
		// Token: 0x170010D3 RID: 4307
		// (get) Token: 0x06003FDB RID: 16347 RVA: 0x0023AD62 File Offset: 0x00238F62
		// (set) Token: 0x06003FDC RID: 16348 RVA: 0x0023AD6A File Offset: 0x00238F6A
		public LocationType.BiomeGateSetting BiomeGate { get; private set; }

		// Token: 0x170010D4 RID: 4308
		// (get) Token: 0x06003FDD RID: 16349 RVA: 0x0023AD73 File Offset: 0x00238F73
		// (set) Token: 0x06003FDE RID: 16350 RVA: 0x0023AD7B File Offset: 0x00238F7B
		public bool ForceAsStartOutpost { get; private set; }

		// Token: 0x170010D5 RID: 4309
		// (get) Token: 0x06003FDF RID: 16351 RVA: 0x0023AD84 File Offset: 0x00238F84
		// (set) Token: 0x06003FE0 RID: 16352 RVA: 0x0023AD8C File Offset: 0x00238F8C
		public bool AllowInRandomLevels { get; private set; }

		// Token: 0x170010D6 RID: 4310
		// (get) Token: 0x06003FE1 RID: 16353 RVA: 0x0023AD95 File Offset: 0x00238F95
		// (set) Token: 0x06003FE2 RID: 16354 RVA: 0x0023AD9D File Offset: 0x00238F9D
		public bool UsePortraitInRandomLoadingScreens { get; private set; }

		// Token: 0x170010D7 RID: 4311
		// (get) Token: 0x06003FE3 RID: 16355 RVA: 0x0023ADA8 File Offset: 0x00238FA8
		public IReadOnlyList<string> NameFormats
		{
			get
			{
				if (this.nameFormats == null || GameSettings.CurrentConfig.Language != this.nameFormatLanguage)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
					defaultInterpolatedStringHandler.AppendLiteral("LocationNameFormat.");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
					this.nameFormats = new ImmutableArray<string>?(TextManager.GetAll(defaultInterpolatedStringHandler.ToStringAndClear()).ToImmutableArray<string>());
					this.nameFormatLanguage = GameSettings.CurrentConfig.Language;
				}
				return this.nameFormats;
			}
		}

		// Token: 0x170010D8 RID: 4312
		// (get) Token: 0x06003FE4 RID: 16356 RVA: 0x0023AE3C File Offset: 0x0023903C
		public bool HasHireableCharacters
		{
			get
			{
				return this.hireableJobs.Any<ValueTuple<Identifier, float, bool>>();
			}
		}

		// Token: 0x170010D9 RID: 4313
		// (get) Token: 0x06003FE5 RID: 16357 RVA: 0x0023AE49 File Offset: 0x00239049
		// (set) Token: 0x06003FE6 RID: 16358 RVA: 0x0023AE51 File Offset: 0x00239051
		public bool HasOutpost { get; private set; }

		// Token: 0x170010DA RID: 4314
		// (get) Token: 0x06003FE7 RID: 16359 RVA: 0x0023AE5A File Offset: 0x0023905A
		public Identifier ReplaceInRadiation { get; }

		// Token: 0x170010DB RID: 4315
		// (get) Token: 0x06003FE8 RID: 16360 RVA: 0x0023AE62 File Offset: 0x00239062
		public Identifier DescriptionInRadiation { get; }

		// Token: 0x170010DC RID: 4316
		// (get) Token: 0x06003FE9 RID: 16361 RVA: 0x0023AE6A File Offset: 0x0023906A
		public Identifier Faction { get; }

		// Token: 0x170010DD RID: 4317
		// (get) Token: 0x06003FEA RID: 16362 RVA: 0x0023AE72 File Offset: 0x00239072
		public Identifier SecondaryFaction { get; }

		// Token: 0x170010DE RID: 4318
		// (get) Token: 0x06003FEB RID: 16363 RVA: 0x0023AE7A File Offset: 0x0023907A
		// (set) Token: 0x06003FEC RID: 16364 RVA: 0x0023AE82 File Offset: 0x00239082
		public Sprite Sprite { get; private set; }

		// Token: 0x170010DF RID: 4319
		// (get) Token: 0x06003FED RID: 16365 RVA: 0x0023AE8B File Offset: 0x0023908B
		public Sprite RadiationSprite { get; }

		// Token: 0x170010E0 RID: 4320
		// (get) Token: 0x06003FEE RID: 16366 RVA: 0x0023AE93 File Offset: 0x00239093
		public bool IgnoreGenericEvents { get; }

		// Token: 0x170010E1 RID: 4321
		// (get) Token: 0x06003FEF RID: 16367 RVA: 0x0023AE9B File Offset: 0x0023909B
		// (set) Token: 0x06003FF0 RID: 16368 RVA: 0x0023AEA3 File Offset: 0x002390A3
		public Identifier EventLocationType { get; private set; }

		// Token: 0x170010E2 RID: 4322
		// (get) Token: 0x06003FF1 RID: 16369 RVA: 0x0023AEAC File Offset: 0x002390AC
		// (set) Token: 0x06003FF2 RID: 16370 RVA: 0x0023AEB4 File Offset: 0x002390B4
		public Identifier UseOutpostModulesOfLocationType { get; set; }

		// Token: 0x170010E3 RID: 4323
		// (get) Token: 0x06003FF3 RID: 16371 RVA: 0x0023AEBD File Offset: 0x002390BD
		// (set) Token: 0x06003FF4 RID: 16372 RVA: 0x0023AEC5 File Offset: 0x002390C5
		public Color SpriteColor { get; private set; }

		// Token: 0x170010E4 RID: 4324
		// (get) Token: 0x06003FF5 RID: 16373 RVA: 0x0023AECE File Offset: 0x002390CE
		public float StoreMaxReputationModifier { get; } = 0.1f;

		// Token: 0x170010E5 RID: 4325
		// (get) Token: 0x06003FF6 RID: 16374 RVA: 0x0023AED6 File Offset: 0x002390D6
		public float StoreMinReputationModifier { get; } = 1f;

		// Token: 0x170010E6 RID: 4326
		// (get) Token: 0x06003FF7 RID: 16375 RVA: 0x0023AEDE File Offset: 0x002390DE
		public float StoreSellPriceModifier { get; } = 0.3f;

		// Token: 0x170010E7 RID: 4327
		// (get) Token: 0x06003FF8 RID: 16376 RVA: 0x0023AEE6 File Offset: 0x002390E6
		public float StoreBuyPriceModifier { get; } = 1f;

		// Token: 0x170010E8 RID: 4328
		// (get) Token: 0x06003FF9 RID: 16377 RVA: 0x0023AEEE File Offset: 0x002390EE
		public float DailySpecialPriceModifier { get; } = 0.5f;

		// Token: 0x170010E9 RID: 4329
		// (get) Token: 0x06003FFA RID: 16378 RVA: 0x0023AEF6 File Offset: 0x002390F6
		public float RequestGoodPriceModifier { get; } = 2f;

		// Token: 0x170010EA RID: 4330
		// (get) Token: 0x06003FFB RID: 16379 RVA: 0x0023AEFE File Offset: 0x002390FE
		public float RequestGoodBuyPriceModifier { get; } = 5f;

		// Token: 0x170010EB RID: 4331
		// (get) Token: 0x06003FFC RID: 16380 RVA: 0x0023AF06 File Offset: 0x00239106
		public int StoreInitialBalance { get; } = 5000;

		// Token: 0x170010EC RID: 4332
		// (get) Token: 0x06003FFD RID: 16381 RVA: 0x0023AF0E File Offset: 0x0023910E
		public int StorePriceModifierRange { get; } = 5;

		// Token: 0x170010ED RID: 4333
		// (get) Token: 0x06003FFE RID: 16382 RVA: 0x0023AF16 File Offset: 0x00239116
		public int DailySpecialsCount { get; } = 1;

		// Token: 0x170010EE RID: 4334
		// (get) Token: 0x06003FFF RID: 16383 RVA: 0x0023AF1E File Offset: 0x0023911E
		public int RequestedGoodsCount { get; } = 1;

		// Token: 0x06004000 RID: 16384 RVA: 0x0023AF26 File Offset: 0x00239126
		public override string ToString()
		{
			return "LocationType (" + this.Identifier.ToString() + ")";
		}

		// Token: 0x06004001 RID: 16385 RVA: 0x0023AF48 File Offset: 0x00239148
		public LocationType(ContentXElement element, LocationTypesFile file)
		{
			LocationType.<>c__DisplayClass118_0 CS$<>8__locals1;
			CS$<>8__locals1.element = element;
			base..ctor(file, CS$<>8__locals1.element.GetAttributeIdentifier("identifier", CS$<>8__locals1.element.Name.LocalName));
			CS$<>8__locals1.<>4__this = this;
			this.Name = TextManager.Get(new string[]
			{
				"LocationName." + this.Identifier.ToString(),
				"unknown"
			});
			this.Description = TextManager.Get(new string[]
			{
				"LocationDescription." + this.Identifier.ToString(),
				""
			});
			Identifier forceNameId = CS$<>8__locals1.element.GetAttributeIdentifier("ForceLocationTypeName", string.Empty);
			if (!forceNameId.IsEmpty)
			{
				LocalizedString forcedName = TextManager.Get("LocationName." + forceNameId.ToString());
				if (!forcedName.IsNullOrEmpty())
				{
					this.Name = forcedName;
				}
			}
			Identifier forceDescriptionId = CS$<>8__locals1.element.GetAttributeIdentifier("ForceLocationTypeDescription", string.Empty);
			if (!forceDescriptionId.IsEmpty)
			{
				LocalizedString forcedDescription = TextManager.Get("LocationDescription." + forceDescriptionId.ToString());
				if (!forcedDescription.IsNullOrEmpty())
				{
					this.Description = forcedDescription;
				}
			}
			this.BeaconStationChance = CS$<>8__locals1.element.GetAttributeFloat("beaconstationchance", 0f);
			this.UsePortraitInRandomLoadingScreens = CS$<>8__locals1.element.GetAttributeBool("UsePortraitInRandomLoadingScreens", true);
			this.HasOutpost = CS$<>8__locals1.element.GetAttributeBool("hasoutpost", true);
			bool allowAsBiomeGateLegacy = CS$<>8__locals1.element.GetAttributeBool("allowasbiomegate", true);
			ContentXElement element2 = CS$<>8__locals1.element;
			string key = "BiomeGate";
			LocationType.BiomeGateSetting biomeGateSetting = allowAsBiomeGateLegacy ? LocationType.BiomeGateSetting.Allow : LocationType.BiomeGateSetting.Deny;
			this.BiomeGate = element2.GetAttributeEnum<LocationType.BiomeGateSetting>(key, biomeGateSetting);
			if (this.BiomeGate != LocationType.BiomeGateSetting.Deny && !this.HasOutpost)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(144, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Potential error in location type ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(": the location is set to be allowed as a biome gate, but will never be chosen as one because it has no outpost.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), base.ContentPackage);
			}
			this.ForceAsStartOutpost = CS$<>8__locals1.element.GetAttributeBool("ForceAsStartOutpost", false);
			this.AllowInRandomLevels = CS$<>8__locals1.element.GetAttributeBool("AllowInRandomLevels", true);
			this.Faction = CS$<>8__locals1.element.GetAttributeIdentifier("Faction", Identifier.Empty);
			this.SecondaryFaction = CS$<>8__locals1.element.GetAttributeIdentifier("SecondaryFaction", Identifier.Empty);
			this.ShowSonarMarker = CS$<>8__locals1.element.GetAttributeBool("showsonarmarker", true);
			this.MissionIdentifiers = CS$<>8__locals1.element.GetAttributeIdentifierArray("missionidentifiers", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.MissionTags = CS$<>8__locals1.element.GetAttributeIdentifierArray("missiontags", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.HideEntitySubcategories = CS$<>8__locals1.element.GetAttributeStringArray("hideentitysubcategories", Array.Empty<string>(), false).ToList<string>();
			this.ReplaceInRadiation = CS$<>8__locals1.element.GetAttributeIdentifier("ReplaceInRadiation", Identifier.Empty);
			this.DescriptionInRadiation = CS$<>8__locals1.element.GetAttributeIdentifier("DescriptionInRadiation", "locationdescription.abandonedirradiated");
			this.forceOutpostGenerationParamsIdentifier = CS$<>8__locals1.element.GetAttributeIdentifier("forceoutpostgenerationparams", Identifier.Empty);
			this.BackgroundMusicLocationType = CS$<>8__locals1.element.GetAttributeIdentifier("BackgroundMusicLocationType", Identifier.Empty);
			this.IgnoreGenericEvents = CS$<>8__locals1.element.GetAttributeBool("IgnoreGenericEvents", false);
			this.EventLocationType = CS$<>8__locals1.element.GetAttributeIdentifier("EventLocationType", Identifier.Empty);
			this.UseOutpostModulesOfLocationType = CS$<>8__locals1.element.GetAttributeIdentifier("UseOutpostModulesOfLocationType", Identifier.Empty);
			this.IsAnyOutpost = CS$<>8__locals1.element.GetAttributeBool("IsAnyOutpost", this.HasOutpost);
			string teamStr = CS$<>8__locals1.element.GetAttributeString("outpostteam", "FriendlyNPC");
			Enum.TryParse<CharacterTeamType>(teamStr, out this.OutpostTeam);
			if (CS$<>8__locals1.element.GetAttribute("ForceLocationName") != null || CS$<>8__locals1.element.GetAttribute("name") != null)
			{
				this.ForceLocationName = CS$<>8__locals1.element.GetAttributeIdentifier("ForceLocationName", CS$<>8__locals1.element.GetAttributeIdentifier("name", string.Empty));
			}
			else
			{
				List<string> names = new List<string>();
				string[] rawNamePaths = CS$<>8__locals1.element.GetAttributeStringArray("namefile", Array.Empty<string>(), false);
				if (rawNamePaths.Any<string>())
				{
					foreach (string rawPath in rawNamePaths)
					{
						try
						{
							ContentPath path = ContentPath.FromRaw(CS$<>8__locals1.element.ContentPackage, rawPath.Trim());
							names.AddRange(File.ReadAllLines(path.Value, null, false).ToList<string>());
						}
						catch (Exception e)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("Failed to read name file \"rawPath\" for location type \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral("\"!");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e, null, false, false);
						}
					}
					if (!names.Any<string>())
					{
						names.Add("ERROR: No names found");
					}
					this.rawNames = names.ToImmutableArray<string>();
				}
				else
				{
					this.nameIdentifiers = new ImmutableArray<Identifier>?(CS$<>8__locals1.element.GetAttributeIdentifierArray("nameidentifiers", new Identifier[]
					{
						this.Identifier
					}, true).ToImmutableArray<Identifier>());
				}
			}
			string[] commonnessPerZoneStrs = CS$<>8__locals1.element.GetAttributeStringArray("commonnessperzone", Array.Empty<string>(), false);
			foreach (string commonnessPerZoneStr in commonnessPerZoneStrs)
			{
				string[] splitCommonnessPerZone = commonnessPerZoneStr.Split(':', StringSplitOptions.None);
				int zoneIndex;
				float zoneCommonness;
				if (splitCommonnessPerZone.Length != 2 || !int.TryParse(splitCommonnessPerZone[0].Trim(), out zoneIndex) || !float.TryParse(splitCommonnessPerZone[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out zoneCommonness))
				{
					DebugConsole.ThrowError("Failed to read commonness values for location type \"" + this.Identifier.ToString() + "\" - commonness should be given in the format \"zone1index: zone1commonness, zone2index: zone2commonness\"", null, CS$<>8__locals1.element.ContentPackage, false, false);
					break;
				}
				if (zoneCommonness > 0f)
				{
					this.<.ctor>g__AugmentDifficultyZoneSettings|118_0(zoneIndex, new float?(zoneCommonness), null, ref CS$<>8__locals1);
				}
			}
			string[] minCountPerZoneStrs = CS$<>8__locals1.element.GetAttributeStringArray("mincountperzone", Array.Empty<string>(), false);
			foreach (string minCountPerZoneStr in minCountPerZoneStrs)
			{
				string[] splitMinCountPerZone = minCountPerZoneStr.Split(':', StringSplitOptions.None);
				int zoneIndex2;
				int minCount;
				if (splitMinCountPerZone.Length != 2 || !int.TryParse(splitMinCountPerZone[0].Trim(), out zoneIndex2) || !int.TryParse(splitMinCountPerZone[1].Trim(), out minCount))
				{
					DebugConsole.ThrowError("Failed to read minimum zone count values for location type \"" + this.Identifier.ToString() + "\" - minimum zone counts should be given in the format \"zone1index: zone1mincount, zone2index: zone2mincount\"", null, CS$<>8__locals1.element.ContentPackage, false, false);
					break;
				}
				if (minCount > 0)
				{
					this.<.ctor>g__AugmentDifficultyZoneSettings|118_0(zoneIndex2, null, new int?(minCount), ref CS$<>8__locals1);
				}
			}
			List<Sprite> portraitsList = new List<Sprite>();
			List<ValueTuple<Identifier, float, bool>> hireableJobsList = new List<ValueTuple<Identifier, float, bool>>();
			foreach (ContentXElement subElement in CS$<>8__locals1.element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 5:
						if (text == "store")
						{
							this.StoreMaxReputationModifier = subElement.GetAttributeFloat("maxreputationmodifier", this.StoreMaxReputationModifier);
							this.StoreBuyPriceModifier = subElement.GetAttributeFloat("buypricemodifier", this.StoreBuyPriceModifier);
							this.StoreMinReputationModifier = subElement.GetAttributeFloat("minreputationmodifier", this.StoreMaxReputationModifier);
							this.StoreSellPriceModifier = subElement.GetAttributeFloat("sellpricemodifier", this.StoreSellPriceModifier);
							this.DailySpecialPriceModifier = subElement.GetAttributeFloat("dailyspecialpricemodifier", this.DailySpecialPriceModifier);
							this.RequestGoodPriceModifier = subElement.GetAttributeFloat("requestgoodpricemodifier", this.RequestGoodPriceModifier);
							this.RequestGoodBuyPriceModifier = subElement.GetAttributeFloat("requestgoodbuypricemodifier", this.RequestGoodBuyPriceModifier);
							this.StoreInitialBalance = subElement.GetAttributeInt("initialbalance", this.StoreInitialBalance);
							this.StorePriceModifierRange = subElement.GetAttributeInt("pricemodifierrange", this.StorePriceModifierRange);
							this.DailySpecialsCount = subElement.GetAttributeInt("dailyspecialscount", this.DailySpecialsCount);
							this.RequestedGoodsCount = subElement.GetAttributeInt("requestedgoodscount", this.RequestedGoodsCount);
						}
						break;
					case 6:
						if (text == "symbol")
						{
							this.Sprite = new Sprite(subElement, "", "", true, 1f);
							ContentXElement contentXElement = subElement;
							string key2 = "color";
							Color white = Color.White;
							this.SpriteColor = contentXElement.GetAttributeColor(key2, white);
						}
						break;
					case 7:
						break;
					case 8:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c != 'h')
							{
								if (c == 'p')
								{
									if (text == "portrait")
									{
										Sprite portrait = new Sprite(subElement, "", "", true, 1f);
										if (portrait != null)
										{
											if (!File.Exists(portrait.FilePath))
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(64, 2);
												defaultInterpolatedStringHandler3.AppendLiteral("Error in location type \"");
												defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
												defaultInterpolatedStringHandler3.AppendLiteral("\": cannot find the location portrait \"");
												defaultInterpolatedStringHandler3.AppendFormatted<ContentPath>(portrait.FilePath);
												defaultInterpolatedStringHandler3.AppendLiteral("\".");
												DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
											}
											else
											{
												portraitsList.Add(portrait);
											}
										}
									}
								}
							}
							else if (text == "hireable")
							{
								Identifier jobIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
								float jobCommonness = subElement.GetAttributeFloat("commonness", 1f);
								bool availableIfMissing = subElement.GetAttributeBool("AlwaysAvailableIfMissingFromCrew", false);
								this.totalHireableWeight += jobCommonness;
								hireableJobsList.Add(new ValueTuple<Identifier, float, bool>(jobIdentifier, jobCommonness, availableIfMissing));
							}
						}
						else if (text == "changeto")
						{
							this.CanChangeTo.Add(new LocationTypeChange(this.Identifier, subElement, true, 0f));
						}
						break;
					}
					default:
						if (length != 12)
						{
							if (length == 15)
							{
								if (text == "radiationsymbol")
								{
									this.RadiationSprite = new Sprite(subElement, "", "", true, 1f);
								}
							}
						}
						else if (text == "areasettings")
						{
							this.<.ctor>g__ParseAreaSettings|118_1(subElement, ref CS$<>8__locals1);
						}
						break;
					}
				}
			}
			this.portraits = portraitsList.ToImmutableArray<Sprite>();
			this.hireableJobs = hireableJobsList.ToImmutableArray<ValueTuple<Identifier, float, bool>>();
		}

		// Token: 0x06004002 RID: 16386 RVA: 0x0023BB10 File Offset: 0x00239D10
		public IEnumerable<JobPrefab> GetHireablesMissingFromCrew()
		{
			LocationType.<GetHireablesMissingFromCrew>d__119 <GetHireablesMissingFromCrew>d__ = new LocationType.<GetHireablesMissingFromCrew>d__119(-2);
			<GetHireablesMissingFromCrew>d__.<>4__this = this;
			return <GetHireablesMissingFromCrew>d__;
		}

		// Token: 0x06004003 RID: 16387 RVA: 0x0023BB20 File Offset: 0x00239D20
		public JobPrefab GetRandomHireable()
		{
			Identifier selectedJobId = this.hireableJobs.GetRandomByWeight(([TupleElementNames(new string[]
			{
				"Identifier",
				"Commonness",
				"AlwaysAvailableIfMissingFromCrew"
			})] ValueTuple<Identifier, float, bool> j) => j.Item2, Rand.RandSync.ServerAndClient).Item1;
			JobPrefab job;
			if (JobPrefab.Prefabs.TryGet(selectedJobId, out job))
			{
				return job;
			}
			return null;
		}

		// Token: 0x06004004 RID: 16388 RVA: 0x0023BB75 File Offset: 0x00239D75
		public Sprite GetPortrait(int randomSeed)
		{
			if (this.portraits.Length == 0)
			{
				return null;
			}
			return this.portraits[Math.Abs(randomSeed) % this.portraits.Length];
		}

		// Token: 0x06004005 RID: 16389 RVA: 0x0023BBA4 File Offset: 0x00239DA4
		public Identifier GetRandomNameId(Random rand, IEnumerable<Location> existingLocations)
		{
			if (this.nameIdentifiers == null)
			{
				return Identifier.Empty;
			}
			List<Identifier> nameIds = new List<Identifier>();
			foreach (Identifier nameId2 in this.nameIdentifiers.Value)
			{
				int index = 0;
				for (;;)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
					defaultInterpolatedStringHandler.AppendLiteral("LocationName.");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(nameId2);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendFormatted<int>(index);
					Identifier tag = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
					if (!TextManager.ContainsTag(tag, TextManager.DefaultLanguage))
					{
						break;
					}
					nameIds.Add(tag);
					index++;
				}
				if (index == 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not find any location names for the location type ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(". Name identifier: ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(nameId2);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				}
			}
			if (nameIds.None(null))
			{
				return Identifier.Empty;
			}
			if (existingLocations != null)
			{
				List<Identifier> unusedNameIds = nameIds.FindAll((Identifier nameId) => existingLocations.None(delegate(Location l)
				{
					Identifier nameIdentifier = l.NameIdentifier;
					return nameIdentifier == nameId;
				}));
				if (unusedNameIds.Count > 0)
				{
					return unusedNameIds[rand.Next() % unusedNameIds.Count];
				}
			}
			return nameIds[rand.Next() % nameIds.Count];
		}

		// Token: 0x06004006 RID: 16390 RVA: 0x0023BD1C File Offset: 0x00239F1C
		public string GetRandomRawName(Random rand, IEnumerable<Location> existingLocations)
		{
			if (new ImmutableArray<string>?(this.rawNames) == null || this.rawNames.None(null))
			{
				return string.Empty;
			}
			if (existingLocations != null)
			{
				List<string> unusedNames = (from name in this.rawNames
				where !existingLocations.Any((Location l) => l.DisplayName.Value == name)
				select name).ToList<string>();
				if (unusedNames.Count > 0)
				{
					return unusedNames[rand.Next() % unusedNames.Count];
				}
			}
			return this.rawNames[rand.Next() % this.rawNames.Length];
		}

		// Token: 0x06004007 RID: 16391 RVA: 0x0023BDCC File Offset: 0x00239FCC
		public static LocationType Random(Random rand, int? zone = null, Identifier? biomeId = null, bool requireOutpost = false, Func<LocationType, bool> predicate = null)
		{
			LocationType[] allowedLocationTypes = (from lt in LocationType.Prefabs
			where (predicate == null || predicate(lt)) && base.<Random>g__IsValid|2(lt)
			select lt into p
			orderby p.UintIdentifier
			select p).ToArray<LocationType>();
			if (allowedLocationTypes.Length == 0)
			{
				string str = "Could not generate a random location type - no location types for the zone ";
				int? zone2 = zone;
				DebugConsole.ThrowError(str + zone2.ToString() + " found!", null, null, false, false);
			}
			if (zone != null || biomeId != null)
			{
				Predicate<LocationType.AreaSettingData> <>9__6;
				return ToolBox.SelectWeightedRandom<LocationType>(allowedLocationTypes, allowedLocationTypes.Select(delegate(LocationType allowedType)
				{
					List<LocationType.AreaSettingData> areaSettings = allowedType.AreaSettings;
					Predicate<LocationType.AreaSettingData> match;
					if ((match = <>9__6) == null)
					{
						match = (<>9__6 = ((LocationType.AreaSettingData areaSetting) => areaSetting.MatchesZone(zone.Value) || areaSetting.MatchesBiome(biomeId.Value)));
					}
					LocationType.AreaSettingData areaSettingData = areaSettings.Find(match);
					if (areaSettingData == null)
					{
						return 0f;
					}
					return areaSettingData.Commonness;
				}).ToArray<float>(), rand);
			}
			return allowedLocationTypes[rand.Next() % allowedLocationTypes.Length];
		}

		// Token: 0x06004008 RID: 16392 RVA: 0x0023BEB4 File Offset: 0x0023A0B4
		public bool IsValidForZoneOrBiome(int? zone, Identifier? biomeIdentifier)
		{
			return (zone != null || this.AllowInRandomLevels) && ((zone == null && biomeIdentifier == null) || this.AreaSettings.Any((LocationType.AreaSettingData setting) => setting.Matches(zone, biomeIdentifier)));
		}

		// Token: 0x06004009 RID: 16393 RVA: 0x0023BF20 File Offset: 0x0023A120
		public OutpostGenerationParams GetForcedOutpostGenerationParams()
		{
			OutpostGenerationParams parameters;
			if (OutpostGenerationParams.OutpostParams.TryGet(this.forceOutpostGenerationParamsIdentifier, out parameters))
			{
				return parameters;
			}
			return null;
		}

		// Token: 0x0600400A RID: 16394 RVA: 0x0023BF44 File Offset: 0x0023A144
		public bool HasCounts()
		{
			return this.AreaSettings.Any((LocationType.AreaSettingData setting) => setting.HasCounts);
		}

		// Token: 0x0600400B RID: 16395 RVA: 0x0023BF70 File Offset: 0x0023A170
		public override void Dispose()
		{
		}

		// Token: 0x0600400D RID: 16397 RVA: 0x0023BF80 File Offset: 0x0023A180
		[CompilerGenerated]
		private void <.ctor>g__AugmentDifficultyZoneSettings|118_0(int zoneIndex, float? zoneCommonness, int? minCount, ref LocationType.<>c__DisplayClass118_0 A_4)
		{
			LocationType.AreaSettingData existingSettings = this.AreaSettings.Find(delegate(LocationType.AreaSettingData areaSettingData)
			{
				LocationType.DifficultyZoneSettingData difficultyZoneSettingData = areaSettingData as LocationType.DifficultyZoneSettingData;
				return difficultyZoneSettingData != null && difficultyZoneSettingData.DifficultyZone == zoneIndex;
			});
			if (existingSettings != null)
			{
				int index = this.AreaSettings.IndexOf(existingSettings);
				List<LocationType.AreaSettingData> areaSettings = this.AreaSettings;
				int index2 = index;
				int zoneIndex2 = zoneIndex;
				int? num = minCount;
				int? minCount2 = (num != null) ? num : existingSettings.MinCount;
				num = minCount;
				areaSettings[index2] = new LocationType.DifficultyZoneSettingData(zoneIndex2, minCount2, (num != null) ? num : existingSettings.MaxCount, zoneCommonness ?? existingSettings.Commonness, null, this);
				return;
			}
			this.AreaSettings.Add(new LocationType.DifficultyZoneSettingData(zoneIndex, new int?(minCount.GetValueOrDefault()), new int?(minCount.GetValueOrDefault()), zoneCommonness.GetValueOrDefault(), null, this));
		}

		// Token: 0x0600400E RID: 16398 RVA: 0x0023C068 File Offset: 0x0023A268
		[CompilerGenerated]
		private void <.ctor>g__ParseAreaSettings|118_1(ContentXElement areaSettingsElement, ref LocationType.<>c__DisplayClass118_0 A_2)
		{
			LocationType.<>c__DisplayClass118_2 CS$<>8__locals1;
			CS$<>8__locals1.areaSettingsElement = areaSettingsElement;
			Identifier biomeIdentifier = CS$<>8__locals1.areaSettingsElement.GetAttributeIdentifier("biome", Identifier.Empty);
			int zone = CS$<>8__locals1.areaSettingsElement.GetAttributeInt("zone", 0);
			if (biomeIdentifier == Identifier.Empty && zone == 0)
			{
				DebugConsole.ThrowError("Failed to read area settings for locationType \"" + this.Identifier.ToString() + "\" - biome identifier and zone are both missing.", null, A_2.element.ContentPackage, false, false);
				return;
			}
			if (biomeIdentifier != Identifier.Empty && zone != 0)
			{
				DebugConsole.ThrowError("Failed to read area settings for locationType \"" + this.Identifier.ToString() + "\" - both biome identifier and zone are defined. Must be one or the other.", null, A_2.element.ContentPackage, false, false);
				return;
			}
			if (LocationType.<.ctor>g__HasComma|118_3("mincount", ref CS$<>8__locals1) || LocationType.<.ctor>g__HasComma|118_3("maxcount", ref CS$<>8__locals1) || LocationType.<.ctor>g__HasComma|118_3("count", ref CS$<>8__locals1))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(168, 1);
				defaultInterpolatedStringHandler.AppendLiteral("AreaSettings for locationType ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" has comma inside int count attribute. This causes the resulting parse to combine the numbers, resulting in incorrect amount of locations.");
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				ContentPackage contentPackage = base.ContentPackage;
				DebugConsole.LogError(msg, null, contentPackage);
			}
			int? minCount = CS$<>8__locals1.areaSettingsElement.GetAttributeNullableInt("mincount");
			int? maxCount = CS$<>8__locals1.areaSettingsElement.GetAttributeNullableInt("maxcount");
			int? count = CS$<>8__locals1.areaSettingsElement.GetAttributeNullableInt("count");
			float? desiredPosition = CS$<>8__locals1.areaSettingsElement.GetAttributeNullableFloat("desiredposition");
			float commonness = CS$<>8__locals1.areaSettingsElement.GetAttributeFloat("commonness", 0f);
			if (count != null)
			{
				minCount = count;
				maxCount = count;
			}
			else if (minCount != null && maxCount != null)
			{
				int? num = minCount;
				int num2 = 0;
				if (num.GetValueOrDefault() <= num2 & num != null)
				{
					num = maxCount;
					num2 = 0;
					if (num.GetValueOrDefault() <= num2 & num != null)
					{
						DebugConsole.AddWarning("Failed to read count value for location type \"" + this.Identifier.ToString() + "\" - both min and max count are 0.", A_2.element.ContentPackage);
						return;
					}
				}
			}
			if (biomeIdentifier != Identifier.Empty)
			{
				this.AreaSettings.Add(new LocationType.BiomeSettingData(biomeIdentifier, minCount, maxCount, commonness, desiredPosition, this));
				return;
			}
			this.AreaSettings.Add(new LocationType.DifficultyZoneSettingData(zone, minCount, maxCount, commonness, desiredPosition, this));
		}

		// Token: 0x0600400F RID: 16399 RVA: 0x0023C2E0 File Offset: 0x0023A4E0
		[CompilerGenerated]
		internal static bool <.ctor>g__HasComma|118_3(string intAttributeName, ref LocationType.<>c__DisplayClass118_2 A_1)
		{
			XAttribute attr = A_1.areaSettingsElement.GetAttribute(intAttributeName);
			return attr != null && attr.Value.Contains(',');
		}

		// Token: 0x04002126 RID: 8486
		public static readonly PrefabCollection<LocationType> Prefabs = new PrefabCollection<LocationType>();

		// Token: 0x04002127 RID: 8487
		private readonly ImmutableArray<string> rawNames;

		// Token: 0x04002128 RID: 8488
		private readonly ImmutableArray<Sprite> portraits;

		// Token: 0x04002129 RID: 8489
		[TupleElementNames(new string[]
		{
			"Identifier",
			"Commonness",
			"AlwaysAvailableIfMissingFromCrew"
		})]
		private readonly ImmutableArray<ValueTuple<Identifier, float, bool>> hireableJobs;

		// Token: 0x0400212A RID: 8490
		private readonly float totalHireableWeight;

		// Token: 0x0400212B RID: 8491
		public readonly LocalizedString Name;

		// Token: 0x0400212C RID: 8492
		public readonly LocalizedString Description;

		// Token: 0x0400212D RID: 8493
		public readonly Identifier ForceLocationName;

		// Token: 0x0400212E RID: 8494
		public readonly float BeaconStationChance;

		// Token: 0x0400212F RID: 8495
		public readonly CharacterTeamType OutpostTeam;

		// Token: 0x04002130 RID: 8496
		public bool IsAnyOutpost;

		// Token: 0x04002131 RID: 8497
		public readonly List<LocationTypeChange> CanChangeTo = new List<LocationTypeChange>();

		// Token: 0x04002132 RID: 8498
		public readonly ImmutableArray<Identifier> MissionIdentifiers;

		// Token: 0x04002133 RID: 8499
		public readonly ImmutableArray<Identifier> MissionTags;

		// Token: 0x04002134 RID: 8500
		public readonly List<LocationType.AreaSettingData> AreaSettings = new List<LocationType.AreaSettingData>();

		// Token: 0x04002135 RID: 8501
		public readonly List<string> HideEntitySubcategories;

		// Token: 0x0400213A RID: 8506
		private readonly ImmutableArray<Identifier>? nameIdentifiers;

		// Token: 0x0400213B RID: 8507
		private LanguageIdentifier nameFormatLanguage;

		// Token: 0x0400213C RID: 8508
		private ImmutableArray<string>? nameFormats;

		// Token: 0x04002144 RID: 8516
		private readonly Identifier forceOutpostGenerationParamsIdentifier;

		// Token: 0x04002145 RID: 8517
		public readonly Identifier BackgroundMusicLocationType;

		// Token: 0x04002155 RID: 8533
		public readonly bool ShowSonarMarker;

		// Token: 0x02001006 RID: 4102
		public abstract class AreaSettingData
		{
			// Token: 0x17001C4A RID: 7242
			// (get) Token: 0x06008B02 RID: 35586 RVA: 0x003AC06B File Offset: 0x003AA26B
			public int? MinCount { get; }

			// Token: 0x17001C4B RID: 7243
			// (get) Token: 0x06008B03 RID: 35587 RVA: 0x003AC073 File Offset: 0x003AA273
			public int? MaxCount { get; }

			// Token: 0x17001C4C RID: 7244
			// (get) Token: 0x06008B04 RID: 35588 RVA: 0x003AC07B File Offset: 0x003AA27B
			public float Commonness { get; }

			// Token: 0x17001C4D RID: 7245
			// (get) Token: 0x06008B05 RID: 35589 RVA: 0x003AC083 File Offset: 0x003AA283
			public float? DesiredPosition { get; }

			// Token: 0x17001C4E RID: 7246
			// (get) Token: 0x06008B06 RID: 35590 RVA: 0x003AC08C File Offset: 0x003AA28C
			public bool HasCounts
			{
				get
				{
					if (this.MinCount == null || this.MaxCount == null)
					{
						return false;
					}
					int? num = this.MaxCount;
					int num2 = 0;
					if (!(num.GetValueOrDefault() > num2 & num != null))
					{
						num = this.MinCount;
						num2 = 0;
						return num.GetValueOrDefault() > num2 & num != null;
					}
					return true;
				}
			}

			// Token: 0x17001C4F RID: 7247
			// (get) Token: 0x06008B07 RID: 35591 RVA: 0x003AC0F5 File Offset: 0x003AA2F5
			public virtual bool HasValidData
			{
				get
				{
					return true;
				}
			}

			// Token: 0x06008B08 RID: 35592 RVA: 0x003AC0F8 File Offset: 0x003AA2F8
			internal AreaSettingData(int? minCount, int? maxCount, float commonness, float? desiredPosition)
			{
				this.MinCount = minCount;
				this.MaxCount = maxCount;
				this.Commonness = commonness;
				this.DesiredPosition = desiredPosition;
			}

			// Token: 0x06008B09 RID: 35593 RVA: 0x003AC11D File Offset: 0x003AA31D
			public virtual bool MatchesRemainingCount(MapLocationTypeGenerator.LocationTypeCount locationTypeCount)
			{
				return false;
			}

			// Token: 0x06008B0A RID: 35594 RVA: 0x003AC120 File Offset: 0x003AA320
			public virtual bool MatchesLocation(Map map, Location location)
			{
				return false;
			}

			// Token: 0x06008B0B RID: 35595 RVA: 0x003AC123 File Offset: 0x003AA323
			public virtual bool MatchesZone(int zoneIndex)
			{
				return false;
			}

			// Token: 0x06008B0C RID: 35596 RVA: 0x003AC126 File Offset: 0x003AA326
			public virtual bool MatchesBiome(Identifier biomeIdentifier)
			{
				return false;
			}

			// Token: 0x06008B0D RID: 35597 RVA: 0x003AC129 File Offset: 0x003AA329
			public virtual bool Matches(int? zone = null, Identifier? biomeId = null)
			{
				return false;
			}
		}

		// Token: 0x02001007 RID: 4103
		public class BiomeSettingData : LocationType.AreaSettingData
		{
			// Token: 0x17001C50 RID: 7248
			// (get) Token: 0x06008B0E RID: 35598 RVA: 0x003AC12C File Offset: 0x003AA32C
			public Identifier BiomeIdentifier { get; }

			// Token: 0x17001C51 RID: 7249
			// (get) Token: 0x06008B0F RID: 35599 RVA: 0x003AC134 File Offset: 0x003AA334
			public override bool HasValidData
			{
				get
				{
					return Biome.Prefabs.ContainsKey(this.BiomeIdentifier);
				}
			}

			// Token: 0x06008B10 RID: 35600 RVA: 0x003AC148 File Offset: 0x003AA348
			public BiomeSettingData(Identifier biomeIdentifier, int? minCount, int? maxCount, float commonness, float? desiredPosition, LocationType locationType) : base(minCount, maxCount, commonness, desiredPosition)
			{
				int? num = minCount;
				int? num2 = maxCount;
				if (num.GetValueOrDefault() > num2.GetValueOrDefault() & (num != null & num2 != null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in location type ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(locationType.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(": minimum count larger than maximum count in biome ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(biomeIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), locationType.ContentPackage);
				}
				this.BiomeIdentifier = biomeIdentifier;
			}

			// Token: 0x06008B11 RID: 35601 RVA: 0x003AC1E8 File Offset: 0x003AA3E8
			public override bool MatchesRemainingCount(MapLocationTypeGenerator.LocationTypeCount locationTypeCount)
			{
				Identifier? identifier = new Identifier?(this.BiomeIdentifier);
				return locationTypeCount.BiomeId == identifier;
			}

			// Token: 0x06008B12 RID: 35602 RVA: 0x003AC210 File Offset: 0x003AA410
			public override bool MatchesLocation(Map map, Location location)
			{
				Identifier? identifier = new Identifier?(this.BiomeIdentifier);
				Biome biome = location.Biome;
				Identifier? identifier2;
				Identifier? identifier3;
				if (biome == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(biome.Identifier);
				}
				identifier2 = identifier3;
				return identifier == identifier2;
			}

			// Token: 0x06008B13 RID: 35603 RVA: 0x003AC254 File Offset: 0x003AA454
			public override bool MatchesBiome(Identifier biomeIdentifier)
			{
				Identifier biomeIdentifier2 = this.BiomeIdentifier;
				return biomeIdentifier2 == biomeIdentifier;
			}

			// Token: 0x06008B14 RID: 35604 RVA: 0x003AC274 File Offset: 0x003AA474
			public override bool Matches(int? zone = null, Identifier? biomeId = null)
			{
				if (biomeId != null)
				{
					Identifier? identifier = new Identifier?(this.BiomeIdentifier);
					return biomeId == identifier;
				}
				return false;
			}
		}

		// Token: 0x02001008 RID: 4104
		public class DifficultyZoneSettingData : LocationType.AreaSettingData
		{
			// Token: 0x17001C52 RID: 7250
			// (get) Token: 0x06008B15 RID: 35605 RVA: 0x003AC2A1 File Offset: 0x003AA4A1
			public int DifficultyZone { get; }

			// Token: 0x06008B16 RID: 35606 RVA: 0x003AC2AC File Offset: 0x003AA4AC
			public DifficultyZoneSettingData(int difficultyZone, int? minCount, int? maxCount, float commonness, float? desiredPosition, LocationType locationType) : base(minCount, maxCount, commonness, desiredPosition)
			{
				int? num = minCount;
				int? num2 = maxCount;
				if (num.GetValueOrDefault() > num2.GetValueOrDefault() & (num != null & num2 != null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in location type ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(locationType.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(": minimum count larger than maximum count in difficulty zone ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(difficultyZone);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), locationType.ContentPackage);
				}
				this.DifficultyZone = difficultyZone;
			}

			// Token: 0x06008B17 RID: 35607 RVA: 0x003AC34C File Offset: 0x003AA54C
			public override bool MatchesRemainingCount(MapLocationTypeGenerator.LocationTypeCount locationTypeCount)
			{
				int? difficultyZone = locationTypeCount.DifficultyZone;
				int difficultyZone2 = this.DifficultyZone;
				return difficultyZone.GetValueOrDefault() == difficultyZone2 & difficultyZone != null;
			}

			// Token: 0x06008B18 RID: 35608 RVA: 0x003AC379 File Offset: 0x003AA579
			public override bool MatchesLocation(Map map, Location location)
			{
				return this.DifficultyZone == map.GetZoneIndex(location.MapPosition.X);
			}

			// Token: 0x06008B19 RID: 35609 RVA: 0x003AC394 File Offset: 0x003AA594
			public override bool MatchesZone(int zoneIndex)
			{
				return this.DifficultyZone == zoneIndex;
			}

			// Token: 0x06008B1A RID: 35610 RVA: 0x003AC3A0 File Offset: 0x003AA5A0
			public override bool Matches(int? zone = null, Identifier? biomeId = null)
			{
				if (zone != null)
				{
					int? num = zone;
					int difficultyZone = this.DifficultyZone;
					return num.GetValueOrDefault() == difficultyZone & num != null;
				}
				return false;
			}
		}

		// Token: 0x02001009 RID: 4105
		public enum BiomeGateSetting
		{
			// Token: 0x04005735 RID: 22325
			Allow,
			// Token: 0x04005736 RID: 22326
			Deny,
			// Token: 0x04005737 RID: 22327
			Force
		}
	}
}
