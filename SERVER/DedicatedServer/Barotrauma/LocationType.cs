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
	// Token: 0x02000245 RID: 581
	internal class LocationType : PrefabWithUintIdentifier
	{
		// Token: 0x17000C19 RID: 3097
		// (get) Token: 0x06002923 RID: 10531 RVA: 0x0010B58E File Offset: 0x0010978E
		// (set) Token: 0x06002924 RID: 10532 RVA: 0x0010B596 File Offset: 0x00109796
		public LocationType.BiomeGateSetting BiomeGate { get; private set; }

		// Token: 0x17000C1A RID: 3098
		// (get) Token: 0x06002925 RID: 10533 RVA: 0x0010B59F File Offset: 0x0010979F
		// (set) Token: 0x06002926 RID: 10534 RVA: 0x0010B5A7 File Offset: 0x001097A7
		public bool ForceAsStartOutpost { get; private set; }

		// Token: 0x17000C1B RID: 3099
		// (get) Token: 0x06002927 RID: 10535 RVA: 0x0010B5B0 File Offset: 0x001097B0
		// (set) Token: 0x06002928 RID: 10536 RVA: 0x0010B5B8 File Offset: 0x001097B8
		public bool AllowInRandomLevels { get; private set; }

		// Token: 0x17000C1C RID: 3100
		// (get) Token: 0x06002929 RID: 10537 RVA: 0x0010B5C1 File Offset: 0x001097C1
		// (set) Token: 0x0600292A RID: 10538 RVA: 0x0010B5C9 File Offset: 0x001097C9
		public bool UsePortraitInRandomLoadingScreens { get; private set; }

		// Token: 0x17000C1D RID: 3101
		// (get) Token: 0x0600292B RID: 10539 RVA: 0x0010B5D4 File Offset: 0x001097D4
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

		// Token: 0x17000C1E RID: 3102
		// (get) Token: 0x0600292C RID: 10540 RVA: 0x0010B668 File Offset: 0x00109868
		public bool HasHireableCharacters
		{
			get
			{
				return this.hireableJobs.Any<ValueTuple<Identifier, float, bool>>();
			}
		}

		// Token: 0x17000C1F RID: 3103
		// (get) Token: 0x0600292D RID: 10541 RVA: 0x0010B675 File Offset: 0x00109875
		// (set) Token: 0x0600292E RID: 10542 RVA: 0x0010B67D File Offset: 0x0010987D
		public bool HasOutpost { get; private set; }

		// Token: 0x17000C20 RID: 3104
		// (get) Token: 0x0600292F RID: 10543 RVA: 0x0010B686 File Offset: 0x00109886
		public Identifier ReplaceInRadiation { get; }

		// Token: 0x17000C21 RID: 3105
		// (get) Token: 0x06002930 RID: 10544 RVA: 0x0010B68E File Offset: 0x0010988E
		public Identifier DescriptionInRadiation { get; }

		// Token: 0x17000C22 RID: 3106
		// (get) Token: 0x06002931 RID: 10545 RVA: 0x0010B696 File Offset: 0x00109896
		public Identifier Faction { get; }

		// Token: 0x17000C23 RID: 3107
		// (get) Token: 0x06002932 RID: 10546 RVA: 0x0010B69E File Offset: 0x0010989E
		public Identifier SecondaryFaction { get; }

		// Token: 0x17000C24 RID: 3108
		// (get) Token: 0x06002933 RID: 10547 RVA: 0x0010B6A6 File Offset: 0x001098A6
		// (set) Token: 0x06002934 RID: 10548 RVA: 0x0010B6AE File Offset: 0x001098AE
		public Sprite Sprite { get; private set; }

		// Token: 0x17000C25 RID: 3109
		// (get) Token: 0x06002935 RID: 10549 RVA: 0x0010B6B7 File Offset: 0x001098B7
		public Sprite RadiationSprite { get; }

		// Token: 0x17000C26 RID: 3110
		// (get) Token: 0x06002936 RID: 10550 RVA: 0x0010B6BF File Offset: 0x001098BF
		public bool IgnoreGenericEvents { get; }

		// Token: 0x17000C27 RID: 3111
		// (get) Token: 0x06002937 RID: 10551 RVA: 0x0010B6C7 File Offset: 0x001098C7
		// (set) Token: 0x06002938 RID: 10552 RVA: 0x0010B6CF File Offset: 0x001098CF
		public Identifier EventLocationType { get; private set; }

		// Token: 0x17000C28 RID: 3112
		// (get) Token: 0x06002939 RID: 10553 RVA: 0x0010B6D8 File Offset: 0x001098D8
		// (set) Token: 0x0600293A RID: 10554 RVA: 0x0010B6E0 File Offset: 0x001098E0
		public Identifier UseOutpostModulesOfLocationType { get; set; }

		// Token: 0x17000C29 RID: 3113
		// (get) Token: 0x0600293B RID: 10555 RVA: 0x0010B6E9 File Offset: 0x001098E9
		// (set) Token: 0x0600293C RID: 10556 RVA: 0x0010B6F1 File Offset: 0x001098F1
		public Color SpriteColor { get; private set; }

		// Token: 0x17000C2A RID: 3114
		// (get) Token: 0x0600293D RID: 10557 RVA: 0x0010B6FA File Offset: 0x001098FA
		public float StoreMaxReputationModifier { get; } = 0.1f;

		// Token: 0x17000C2B RID: 3115
		// (get) Token: 0x0600293E RID: 10558 RVA: 0x0010B702 File Offset: 0x00109902
		public float StoreMinReputationModifier { get; } = 1f;

		// Token: 0x17000C2C RID: 3116
		// (get) Token: 0x0600293F RID: 10559 RVA: 0x0010B70A File Offset: 0x0010990A
		public float StoreSellPriceModifier { get; } = 0.3f;

		// Token: 0x17000C2D RID: 3117
		// (get) Token: 0x06002940 RID: 10560 RVA: 0x0010B712 File Offset: 0x00109912
		public float StoreBuyPriceModifier { get; } = 1f;

		// Token: 0x17000C2E RID: 3118
		// (get) Token: 0x06002941 RID: 10561 RVA: 0x0010B71A File Offset: 0x0010991A
		public float DailySpecialPriceModifier { get; } = 0.5f;

		// Token: 0x17000C2F RID: 3119
		// (get) Token: 0x06002942 RID: 10562 RVA: 0x0010B722 File Offset: 0x00109922
		public float RequestGoodPriceModifier { get; } = 2f;

		// Token: 0x17000C30 RID: 3120
		// (get) Token: 0x06002943 RID: 10563 RVA: 0x0010B72A File Offset: 0x0010992A
		public float RequestGoodBuyPriceModifier { get; } = 5f;

		// Token: 0x17000C31 RID: 3121
		// (get) Token: 0x06002944 RID: 10564 RVA: 0x0010B732 File Offset: 0x00109932
		public int StoreInitialBalance { get; } = 5000;

		// Token: 0x17000C32 RID: 3122
		// (get) Token: 0x06002945 RID: 10565 RVA: 0x0010B73A File Offset: 0x0010993A
		public int StorePriceModifierRange { get; } = 5;

		// Token: 0x17000C33 RID: 3123
		// (get) Token: 0x06002946 RID: 10566 RVA: 0x0010B742 File Offset: 0x00109942
		public int DailySpecialsCount { get; } = 1;

		// Token: 0x17000C34 RID: 3124
		// (get) Token: 0x06002947 RID: 10567 RVA: 0x0010B74A File Offset: 0x0010994A
		public int RequestedGoodsCount { get; } = 1;

		// Token: 0x06002948 RID: 10568 RVA: 0x0010B752 File Offset: 0x00109952
		public override string ToString()
		{
			return "LocationType (" + this.Identifier.ToString() + ")";
		}

		// Token: 0x06002949 RID: 10569 RVA: 0x0010B774 File Offset: 0x00109974
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
											portraitsList.Add(portrait);
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

		// Token: 0x0600294A RID: 10570 RVA: 0x0010C2D0 File Offset: 0x0010A4D0
		public IEnumerable<JobPrefab> GetHireablesMissingFromCrew()
		{
			LocationType.<GetHireablesMissingFromCrew>d__119 <GetHireablesMissingFromCrew>d__ = new LocationType.<GetHireablesMissingFromCrew>d__119(-2);
			<GetHireablesMissingFromCrew>d__.<>4__this = this;
			return <GetHireablesMissingFromCrew>d__;
		}

		// Token: 0x0600294B RID: 10571 RVA: 0x0010C2E0 File Offset: 0x0010A4E0
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

		// Token: 0x0600294C RID: 10572 RVA: 0x0010C335 File Offset: 0x0010A535
		public Sprite GetPortrait(int randomSeed)
		{
			if (this.portraits.Length == 0)
			{
				return null;
			}
			return this.portraits[Math.Abs(randomSeed) % this.portraits.Length];
		}

		// Token: 0x0600294D RID: 10573 RVA: 0x0010C364 File Offset: 0x0010A564
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

		// Token: 0x0600294E RID: 10574 RVA: 0x0010C4DC File Offset: 0x0010A6DC
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

		// Token: 0x0600294F RID: 10575 RVA: 0x0010C58C File Offset: 0x0010A78C
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

		// Token: 0x06002950 RID: 10576 RVA: 0x0010C674 File Offset: 0x0010A874
		public bool IsValidForZoneOrBiome(int? zone, Identifier? biomeIdentifier)
		{
			return (zone != null || this.AllowInRandomLevels) && ((zone == null && biomeIdentifier == null) || this.AreaSettings.Any((LocationType.AreaSettingData setting) => setting.Matches(zone, biomeIdentifier)));
		}

		// Token: 0x06002951 RID: 10577 RVA: 0x0010C6E0 File Offset: 0x0010A8E0
		public OutpostGenerationParams GetForcedOutpostGenerationParams()
		{
			OutpostGenerationParams parameters;
			if (OutpostGenerationParams.OutpostParams.TryGet(this.forceOutpostGenerationParamsIdentifier, out parameters))
			{
				return parameters;
			}
			return null;
		}

		// Token: 0x06002952 RID: 10578 RVA: 0x0010C704 File Offset: 0x0010A904
		public bool HasCounts()
		{
			return this.AreaSettings.Any((LocationType.AreaSettingData setting) => setting.HasCounts);
		}

		// Token: 0x06002953 RID: 10579 RVA: 0x0010C730 File Offset: 0x0010A930
		public override void Dispose()
		{
		}

		// Token: 0x06002955 RID: 10581 RVA: 0x0010C740 File Offset: 0x0010A940
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

		// Token: 0x06002956 RID: 10582 RVA: 0x0010C828 File Offset: 0x0010AA28
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

		// Token: 0x06002957 RID: 10583 RVA: 0x0010CAA0 File Offset: 0x0010ACA0
		[CompilerGenerated]
		internal static bool <.ctor>g__HasComma|118_3(string intAttributeName, ref LocationType.<>c__DisplayClass118_2 A_1)
		{
			XAttribute attr = A_1.areaSettingsElement.GetAttribute(intAttributeName);
			return attr != null && attr.Value.Contains(',');
		}

		// Token: 0x0400142B RID: 5163
		public static readonly PrefabCollection<LocationType> Prefabs = new PrefabCollection<LocationType>();

		// Token: 0x0400142C RID: 5164
		private readonly ImmutableArray<string> rawNames;

		// Token: 0x0400142D RID: 5165
		private readonly ImmutableArray<Sprite> portraits;

		// Token: 0x0400142E RID: 5166
		[TupleElementNames(new string[]
		{
			"Identifier",
			"Commonness",
			"AlwaysAvailableIfMissingFromCrew"
		})]
		private readonly ImmutableArray<ValueTuple<Identifier, float, bool>> hireableJobs;

		// Token: 0x0400142F RID: 5167
		private readonly float totalHireableWeight;

		// Token: 0x04001430 RID: 5168
		public readonly LocalizedString Name;

		// Token: 0x04001431 RID: 5169
		public readonly LocalizedString Description;

		// Token: 0x04001432 RID: 5170
		public readonly Identifier ForceLocationName;

		// Token: 0x04001433 RID: 5171
		public readonly float BeaconStationChance;

		// Token: 0x04001434 RID: 5172
		public readonly CharacterTeamType OutpostTeam;

		// Token: 0x04001435 RID: 5173
		public bool IsAnyOutpost;

		// Token: 0x04001436 RID: 5174
		public readonly List<LocationTypeChange> CanChangeTo = new List<LocationTypeChange>();

		// Token: 0x04001437 RID: 5175
		public readonly ImmutableArray<Identifier> MissionIdentifiers;

		// Token: 0x04001438 RID: 5176
		public readonly ImmutableArray<Identifier> MissionTags;

		// Token: 0x04001439 RID: 5177
		public readonly List<LocationType.AreaSettingData> AreaSettings = new List<LocationType.AreaSettingData>();

		// Token: 0x0400143A RID: 5178
		public readonly List<string> HideEntitySubcategories;

		// Token: 0x0400143F RID: 5183
		private readonly ImmutableArray<Identifier>? nameIdentifiers;

		// Token: 0x04001440 RID: 5184
		private LanguageIdentifier nameFormatLanguage;

		// Token: 0x04001441 RID: 5185
		private ImmutableArray<string>? nameFormats;

		// Token: 0x04001449 RID: 5193
		private readonly Identifier forceOutpostGenerationParamsIdentifier;

		// Token: 0x0400144A RID: 5194
		public readonly Identifier BackgroundMusicLocationType;

		// Token: 0x0400145A RID: 5210
		public readonly bool ShowSonarMarker;

		// Token: 0x02000A4A RID: 2634
		public abstract class AreaSettingData
		{
			// Token: 0x17001589 RID: 5513
			// (get) Token: 0x06005C91 RID: 23697 RVA: 0x0020121B File Offset: 0x001FF41B
			public int? MinCount { get; }

			// Token: 0x1700158A RID: 5514
			// (get) Token: 0x06005C92 RID: 23698 RVA: 0x00201223 File Offset: 0x001FF423
			public int? MaxCount { get; }

			// Token: 0x1700158B RID: 5515
			// (get) Token: 0x06005C93 RID: 23699 RVA: 0x0020122B File Offset: 0x001FF42B
			public float Commonness { get; }

			// Token: 0x1700158C RID: 5516
			// (get) Token: 0x06005C94 RID: 23700 RVA: 0x00201233 File Offset: 0x001FF433
			public float? DesiredPosition { get; }

			// Token: 0x1700158D RID: 5517
			// (get) Token: 0x06005C95 RID: 23701 RVA: 0x0020123C File Offset: 0x001FF43C
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

			// Token: 0x1700158E RID: 5518
			// (get) Token: 0x06005C96 RID: 23702 RVA: 0x002012A5 File Offset: 0x001FF4A5
			public virtual bool HasValidData
			{
				get
				{
					return true;
				}
			}

			// Token: 0x06005C97 RID: 23703 RVA: 0x002012A8 File Offset: 0x001FF4A8
			internal AreaSettingData(int? minCount, int? maxCount, float commonness, float? desiredPosition)
			{
				this.MinCount = minCount;
				this.MaxCount = maxCount;
				this.Commonness = commonness;
				this.DesiredPosition = desiredPosition;
			}

			// Token: 0x06005C98 RID: 23704 RVA: 0x002012CD File Offset: 0x001FF4CD
			public virtual bool MatchesRemainingCount(MapLocationTypeGenerator.LocationTypeCount locationTypeCount)
			{
				return false;
			}

			// Token: 0x06005C99 RID: 23705 RVA: 0x002012D0 File Offset: 0x001FF4D0
			public virtual bool MatchesLocation(Map map, Location location)
			{
				return false;
			}

			// Token: 0x06005C9A RID: 23706 RVA: 0x002012D3 File Offset: 0x001FF4D3
			public virtual bool MatchesZone(int zoneIndex)
			{
				return false;
			}

			// Token: 0x06005C9B RID: 23707 RVA: 0x002012D6 File Offset: 0x001FF4D6
			public virtual bool MatchesBiome(Identifier biomeIdentifier)
			{
				return false;
			}

			// Token: 0x06005C9C RID: 23708 RVA: 0x002012D9 File Offset: 0x001FF4D9
			public virtual bool Matches(int? zone = null, Identifier? biomeId = null)
			{
				return false;
			}
		}

		// Token: 0x02000A4B RID: 2635
		public class BiomeSettingData : LocationType.AreaSettingData
		{
			// Token: 0x1700158F RID: 5519
			// (get) Token: 0x06005C9D RID: 23709 RVA: 0x002012DC File Offset: 0x001FF4DC
			public Identifier BiomeIdentifier { get; }

			// Token: 0x17001590 RID: 5520
			// (get) Token: 0x06005C9E RID: 23710 RVA: 0x002012E4 File Offset: 0x001FF4E4
			public override bool HasValidData
			{
				get
				{
					return Biome.Prefabs.ContainsKey(this.BiomeIdentifier);
				}
			}

			// Token: 0x06005C9F RID: 23711 RVA: 0x002012F8 File Offset: 0x001FF4F8
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

			// Token: 0x06005CA0 RID: 23712 RVA: 0x00201398 File Offset: 0x001FF598
			public override bool MatchesRemainingCount(MapLocationTypeGenerator.LocationTypeCount locationTypeCount)
			{
				Identifier? identifier = new Identifier?(this.BiomeIdentifier);
				return locationTypeCount.BiomeId == identifier;
			}

			// Token: 0x06005CA1 RID: 23713 RVA: 0x002013C0 File Offset: 0x001FF5C0
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

			// Token: 0x06005CA2 RID: 23714 RVA: 0x00201404 File Offset: 0x001FF604
			public override bool MatchesBiome(Identifier biomeIdentifier)
			{
				Identifier biomeIdentifier2 = this.BiomeIdentifier;
				return biomeIdentifier2 == biomeIdentifier;
			}

			// Token: 0x06005CA3 RID: 23715 RVA: 0x00201424 File Offset: 0x001FF624
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

		// Token: 0x02000A4C RID: 2636
		public class DifficultyZoneSettingData : LocationType.AreaSettingData
		{
			// Token: 0x17001591 RID: 5521
			// (get) Token: 0x06005CA4 RID: 23716 RVA: 0x00201451 File Offset: 0x001FF651
			public int DifficultyZone { get; }

			// Token: 0x06005CA5 RID: 23717 RVA: 0x0020145C File Offset: 0x001FF65C
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

			// Token: 0x06005CA6 RID: 23718 RVA: 0x002014FC File Offset: 0x001FF6FC
			public override bool MatchesRemainingCount(MapLocationTypeGenerator.LocationTypeCount locationTypeCount)
			{
				int? difficultyZone = locationTypeCount.DifficultyZone;
				int difficultyZone2 = this.DifficultyZone;
				return difficultyZone.GetValueOrDefault() == difficultyZone2 & difficultyZone != null;
			}

			// Token: 0x06005CA7 RID: 23719 RVA: 0x00201529 File Offset: 0x001FF729
			public override bool MatchesLocation(Map map, Location location)
			{
				return this.DifficultyZone == map.GetZoneIndex(location.MapPosition.X);
			}

			// Token: 0x06005CA8 RID: 23720 RVA: 0x00201544 File Offset: 0x001FF744
			public override bool MatchesZone(int zoneIndex)
			{
				return this.DifficultyZone == zoneIndex;
			}

			// Token: 0x06005CA9 RID: 23721 RVA: 0x00201550 File Offset: 0x001FF750
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

		// Token: 0x02000A4D RID: 2637
		public enum BiomeGateSetting
		{
			// Token: 0x040035C8 RID: 13768
			Allow,
			// Token: 0x040035C9 RID: 13769
			Deny,
			// Token: 0x040035CA RID: 13770
			Force
		}
	}
}
