using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000243 RID: 579
	internal class Location
	{
		// Token: 0x17000BEE RID: 3054
		// (get) Token: 0x060028A1 RID: 10401 RVA: 0x0010786C File Offset: 0x00105A6C
		// (set) Token: 0x060028A2 RID: 10402 RVA: 0x00107874 File Offset: 0x00105A74
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x17000BEF RID: 3055
		// (get) Token: 0x060028A3 RID: 10403 RVA: 0x0010787D File Offset: 0x00105A7D
		public Identifier NameIdentifier
		{
			get
			{
				return this.nameIdentifier;
			}
		}

		// Token: 0x17000BF0 RID: 3056
		// (get) Token: 0x060028A4 RID: 10404 RVA: 0x00107885 File Offset: 0x00105A85
		public int NameFormatIndex
		{
			get
			{
				return this.nameFormatIndex;
			}
		}

		// Token: 0x17000BF1 RID: 3057
		// (get) Token: 0x060028A5 RID: 10405 RVA: 0x00107890 File Offset: 0x00105A90
		public bool Discovered
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				bool? flag;
				if (gameSession == null)
				{
					flag = null;
				}
				else
				{
					Map map = gameSession.Map;
					flag = ((map != null) ? new bool?(map.IsDiscovered(this)) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x17000BF2 RID: 3058
		// (get) Token: 0x060028A6 RID: 10406 RVA: 0x001078D8 File Offset: 0x00105AD8
		public bool Visited
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				bool? flag;
				if (gameSession == null)
				{
					flag = null;
				}
				else
				{
					Map map = gameSession.Map;
					flag = ((map != null) ? new bool?(map.IsVisited(this)) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x17000BF3 RID: 3059
		// (get) Token: 0x060028A7 RID: 10407 RVA: 0x00107920 File Offset: 0x00105B20
		public bool LocationTypeChangesBlocked
		{
			get
			{
				if (!this.DisallowLocationTypeChanges)
				{
					return this.availableMissions.Any((Mission m) => !m.Completed && m.Prefab.BlockLocationTypeChanges);
				}
				return true;
			}
		}

		// Token: 0x17000BF4 RID: 3060
		// (get) Token: 0x060028A8 RID: 10408 RVA: 0x00107956 File Offset: 0x00105B56
		// (set) Token: 0x060028A9 RID: 10409 RVA: 0x0010795E File Offset: 0x00105B5E
		public Biome Biome { get; set; }

		// Token: 0x17000BF5 RID: 3061
		// (get) Token: 0x060028AA RID: 10410 RVA: 0x00107967 File Offset: 0x00105B67
		// (set) Token: 0x060028AB RID: 10411 RVA: 0x0010796F File Offset: 0x00105B6F
		public Vector2 MapPosition { get; private set; }

		// Token: 0x17000BF6 RID: 3062
		// (get) Token: 0x060028AC RID: 10412 RVA: 0x00107978 File Offset: 0x00105B78
		// (set) Token: 0x060028AD RID: 10413 RVA: 0x00107980 File Offset: 0x00105B80
		public LocationType Type { get; private set; }

		// Token: 0x17000BF7 RID: 3063
		// (get) Token: 0x060028AE RID: 10414 RVA: 0x00107989 File Offset: 0x00105B89
		// (set) Token: 0x060028AF RID: 10415 RVA: 0x00107991 File Offset: 0x00105B91
		public LocationType OriginalType { get; private set; }

		// Token: 0x17000BF8 RID: 3064
		// (get) Token: 0x060028B0 RID: 10416 RVA: 0x0010799A File Offset: 0x00105B9A
		// (set) Token: 0x060028B1 RID: 10417 RVA: 0x001079A2 File Offset: 0x00105BA2
		public LevelData LevelData { get; set; }

		// Token: 0x17000BF9 RID: 3065
		// (get) Token: 0x060028B2 RID: 10418 RVA: 0x001079AB File Offset: 0x00105BAB
		// (set) Token: 0x060028B3 RID: 10419 RVA: 0x001079B3 File Offset: 0x00105BB3
		public int PortraitId { get; private set; }

		// Token: 0x17000BFA RID: 3066
		// (get) Token: 0x060028B4 RID: 10420 RVA: 0x001079BC File Offset: 0x00105BBC
		// (set) Token: 0x060028B5 RID: 10421 RVA: 0x001079C4 File Offset: 0x00105BC4
		public Faction Faction { get; set; }

		// Token: 0x17000BFB RID: 3067
		// (get) Token: 0x060028B6 RID: 10422 RVA: 0x001079CD File Offset: 0x00105BCD
		// (set) Token: 0x060028B7 RID: 10423 RVA: 0x001079D5 File Offset: 0x00105BD5
		public Faction SecondaryFaction { get; set; }

		// Token: 0x17000BFC RID: 3068
		// (get) Token: 0x060028B8 RID: 10424 RVA: 0x001079DE File Offset: 0x00105BDE
		public Reputation Reputation
		{
			get
			{
				Faction faction = this.Faction;
				if (faction == null)
				{
					return null;
				}
				return faction.Reputation;
			}
		}

		// Token: 0x17000BFD RID: 3069
		// (get) Token: 0x060028B9 RID: 10425 RVA: 0x001079F1 File Offset: 0x00105BF1
		public bool IsFactionHostile
		{
			get
			{
				Faction faction = this.Faction;
				return faction != null && faction.Reputation.NormalizedValue < 0.2f;
			}
		}

		// Token: 0x17000BFE RID: 3070
		// (get) Token: 0x060028BA RID: 10426 RVA: 0x00107A10 File Offset: 0x00105C10
		// (set) Token: 0x060028BB RID: 10427 RVA: 0x00107A18 File Offset: 0x00105C18
		public int TurnsInRadiation { get; set; }

		// Token: 0x17000BFF RID: 3071
		// (get) Token: 0x060028BC RID: 10428 RVA: 0x00107A21 File Offset: 0x00105C21
		// (set) Token: 0x060028BD RID: 10429 RVA: 0x00107A29 File Offset: 0x00105C29
		public Dictionary<Identifier, Location.StoreInfo> Stores { get; private set; }

		// Token: 0x17000C00 RID: 3072
		// (get) Token: 0x060028BE RID: 10430 RVA: 0x00107A32 File Offset: 0x00105C32
		private float StoreMaxReputationModifier
		{
			get
			{
				return this.Type.StoreMaxReputationModifier;
			}
		}

		// Token: 0x17000C01 RID: 3073
		// (get) Token: 0x060028BF RID: 10431 RVA: 0x00107A3F File Offset: 0x00105C3F
		private float StoreMinReputationModifier
		{
			get
			{
				return this.Type.StoreMinReputationModifier;
			}
		}

		// Token: 0x17000C02 RID: 3074
		// (get) Token: 0x060028C0 RID: 10432 RVA: 0x00107A4C File Offset: 0x00105C4C
		private float StoreSellPriceModifier
		{
			get
			{
				return this.Type.StoreSellPriceModifier;
			}
		}

		// Token: 0x17000C03 RID: 3075
		// (get) Token: 0x060028C1 RID: 10433 RVA: 0x00107A59 File Offset: 0x00105C59
		private float StoreBuyPriceModifier
		{
			get
			{
				return this.Type.StoreBuyPriceModifier;
			}
		}

		// Token: 0x17000C04 RID: 3076
		// (get) Token: 0x060028C2 RID: 10434 RVA: 0x00107A66 File Offset: 0x00105C66
		private float DailySpecialPriceModifier
		{
			get
			{
				return this.Type.DailySpecialPriceModifier;
			}
		}

		// Token: 0x17000C05 RID: 3077
		// (get) Token: 0x060028C3 RID: 10435 RVA: 0x00107A73 File Offset: 0x00105C73
		private float RequestGoodBuyPriceModifier
		{
			get
			{
				return this.Type.RequestGoodBuyPriceModifier;
			}
		}

		// Token: 0x17000C06 RID: 3078
		// (get) Token: 0x060028C4 RID: 10436 RVA: 0x00107A80 File Offset: 0x00105C80
		private float RequestGoodSellPriceModifier
		{
			get
			{
				return this.Type.RequestGoodPriceModifier;
			}
		}

		// Token: 0x17000C07 RID: 3079
		// (get) Token: 0x060028C5 RID: 10437 RVA: 0x00107A8D File Offset: 0x00105C8D
		public int StoreInitialBalance
		{
			get
			{
				return this.Type.StoreInitialBalance;
			}
		}

		// Token: 0x17000C08 RID: 3080
		// (get) Token: 0x060028C6 RID: 10438 RVA: 0x00107A9A File Offset: 0x00105C9A
		private int StorePriceModifierRange
		{
			get
			{
				return this.Type.StorePriceModifierRange;
			}
		}

		// Token: 0x17000C09 RID: 3081
		// (get) Token: 0x060028C7 RID: 10439 RVA: 0x00107AA7 File Offset: 0x00105CA7
		public int DailySpecialsCount
		{
			get
			{
				return this.Type.DailySpecialsCount;
			}
		}

		// Token: 0x17000C0A RID: 3082
		// (get) Token: 0x060028C8 RID: 10440 RVA: 0x00107AB4 File Offset: 0x00105CB4
		public int RequestedGoodsCount
		{
			get
			{
				return this.Type.RequestedGoodsCount;
			}
		}

		// Token: 0x17000C0B RID: 3083
		// (get) Token: 0x060028C9 RID: 10441 RVA: 0x00107AC1 File Offset: 0x00105CC1
		// (set) Token: 0x060028CA RID: 10442 RVA: 0x00107AC9 File Offset: 0x00105CC9
		private int StepsSinceSpecialsUpdated { get; set; }

		// Token: 0x17000C0C RID: 3084
		// (get) Token: 0x060028CB RID: 10443 RVA: 0x00107AD2 File Offset: 0x00105CD2
		public HashSet<Identifier> StoreIdentifiers { get; } = new HashSet<Identifier>();

		// Token: 0x17000C0D RID: 3085
		// (get) Token: 0x060028CC RID: 10444 RVA: 0x00107ADA File Offset: 0x00105CDA
		public IEnumerable<Location.TakenItem> TakenItems
		{
			get
			{
				return this.takenItems;
			}
		}

		// Token: 0x17000C0E RID: 3086
		// (get) Token: 0x060028CD RID: 10445 RVA: 0x00107AE2 File Offset: 0x00105CE2
		public IEnumerable<int> KilledCharacterIdentifiers
		{
			get
			{
				return this.killedCharacterIdentifiers;
			}
		}

		// Token: 0x17000C0F RID: 3087
		// (get) Token: 0x060028CE RID: 10446 RVA: 0x00107AEA File Offset: 0x00105CEA
		public IEnumerable<Mission> AvailableMissions
		{
			get
			{
				this.availableMissions.RemoveAll((Mission m) => m.Completed || (m.Failed && !m.Prefab.AllowRetry) || m.ForceFailure);
				return this.availableMissions;
			}
		}

		// Token: 0x17000C10 RID: 3088
		// (get) Token: 0x060028CF RID: 10447 RVA: 0x00107B1D File Offset: 0x00105D1D
		public IEnumerable<Mission> AvailableAndVisibleMissions
		{
			get
			{
				return from m in this.AvailableMissions
				where m.Prefab.ShowInMenus
				select m;
			}
		}

		// Token: 0x17000C11 RID: 3089
		// (get) Token: 0x060028D0 RID: 10448 RVA: 0x00107B49 File Offset: 0x00105D49
		public IEnumerable<Mission> SelectedMissions
		{
			get
			{
				this.selectedMissions.RemoveAll((Mission m) => !this.availableMissions.Contains(m));
				return this.selectedMissions;
			}
		}

		// Token: 0x060028D1 RID: 10449 RVA: 0x00107B69 File Offset: 0x00105D69
		public void SelectMission(Mission mission)
		{
			if (!this.SelectedMissions.Contains(mission) && mission != null)
			{
				this.selectedMissions.Add(mission);
				this.selectedMissions.Sort((Mission m1, Mission m2) => this.availableMissions.IndexOf(m1).CompareTo(this.availableMissions.IndexOf(m2)));
			}
		}

		// Token: 0x060028D2 RID: 10450 RVA: 0x00107B9F File Offset: 0x00105D9F
		public void DeselectMission(Mission mission)
		{
			this.selectedMissions.Remove(mission);
		}

		// Token: 0x060028D3 RID: 10451 RVA: 0x00107BB0 File Offset: 0x00105DB0
		public List<int> GetSelectedMissionIndices()
		{
			List<int> selectedMissionIndices = new List<int>();
			foreach (Mission mission in this.SelectedMissions)
			{
				if (this.availableMissions.Contains(mission))
				{
					selectedMissionIndices.Add(this.availableMissions.IndexOf(mission));
				}
			}
			return selectedMissionIndices;
		}

		// Token: 0x060028D4 RID: 10452 RVA: 0x00107C20 File Offset: 0x00105E20
		public void SetSelectedMissionIndices(IEnumerable<int> missionIndices)
		{
			this.selectedMissions.Clear();
			foreach (int missionIndex in missionIndices)
			{
				if (missionIndex < 0 || missionIndex >= this.availableMissions.Count)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(95, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to select a mission in location \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral("\". Mission index out of bounds (");
					defaultInterpolatedStringHandler.AppendFormatted<int>(missionIndex);
					defaultInterpolatedStringHandler.AppendLiteral(", available missions: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.availableMissions.Count);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					break;
				}
				this.selectedMissions.Add(this.availableMissions[missionIndex]);
			}
		}

		// Token: 0x17000C12 RID: 3090
		// (get) Token: 0x060028D5 RID: 10453 RVA: 0x00107D10 File Offset: 0x00105F10
		// (set) Token: 0x060028D6 RID: 10454 RVA: 0x00107D18 File Offset: 0x00105F18
		public float PriceMultiplier
		{
			get
			{
				return this.priceMultiplier;
			}
			set
			{
				this.priceMultiplier = MathHelper.Clamp(value, 0.1f, 10f);
			}
		}

		// Token: 0x17000C13 RID: 3091
		// (get) Token: 0x060028D7 RID: 10455 RVA: 0x00107D30 File Offset: 0x00105F30
		// (set) Token: 0x060028D8 RID: 10456 RVA: 0x00107D38 File Offset: 0x00105F38
		public float MechanicalPriceMultiplier
		{
			get
			{
				return this.mechanicalpriceMultiplier;
			}
			set
			{
				this.mechanicalpriceMultiplier = MathHelper.Clamp(value, 0.1f, 10f);
			}
		}

		// Token: 0x060028D9 RID: 10457 RVA: 0x00107D50 File Offset: 0x00105F50
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Location (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName ?? "null");
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060028DA RID: 10458 RVA: 0x00107DA4 File Offset: 0x00105FA4
		public Location(Vector2 mapPosition, int? zone, Identifier? biomeId, Random rand, bool requireOutpost = false, LocationType forceLocationType = null, IEnumerable<Location> existingLocations = null)
		{
			this.Type = (this.OriginalType = (forceLocationType ?? LocationType.Random(rand, zone, biomeId, requireOutpost, null)));
			this.AssignRandomName(this.Type, rand, existingLocations);
			this.MapPosition = mapPosition;
			this.PortraitId = ToolBox.StringToInt(this.nameIdentifier.Value);
			this.Connections = new List<LocationConnection>();
		}

		// Token: 0x060028DB RID: 10459 RVA: 0x00107E74 File Offset: 0x00106074
		public Location(CampaignMode campaign, XElement element)
		{
			Location.<>c__DisplayClass138_0 CS$<>8__locals1;
			CS$<>8__locals1.element = element;
			base..ctor();
			CS$<>8__locals1.<>4__this = this;
			Identifier locationTypeId = CS$<>8__locals1.element.GetAttributeIdentifier("type", "");
			LocationType type;
			bool typeNotFound = this.<.ctor>g__GetTypeOrFallback|138_0(locationTypeId, out type, ref CS$<>8__locals1);
			this.Type = type;
			Identifier originalLocationTypeId = CS$<>8__locals1.element.GetAttributeIdentifier("originaltype", locationTypeId);
			LocationType originalType;
			this.<.ctor>g__GetTypeOrFallback|138_0(originalLocationTypeId, out originalType, ref CS$<>8__locals1);
			this.OriginalType = originalType;
			this.nameIdentifier = CS$<>8__locals1.element.GetAttributeIdentifier("nameIdentifier", "");
			if (this.nameIdentifier.IsEmpty)
			{
				this.DisplayName = CS$<>8__locals1.element.GetAttributeString("name", "");
				this.rawName = CS$<>8__locals1.element.GetAttributeString("rawname", CS$<>8__locals1.element.GetAttributeString("basename", this.DisplayName.Value));
				this.nameIdentifier = this.rawName.ToIdentifier();
			}
			else
			{
				this.nameFormatIndex = CS$<>8__locals1.element.GetAttributeInt("nameFormatIndex", 0);
				this.DisplayName = Location.GetName(this.Type, this.nameFormatIndex, this.nameIdentifier);
			}
			this.LoadChangingProperties(CS$<>8__locals1.element, campaign);
			this.MapPosition = CS$<>8__locals1.element.GetAttributeVector2("position", Vector2.Zero);
			this.IsGateBetweenBiomes = CS$<>8__locals1.element.GetAttributeBool("isgatebetweenbiomes", false);
			Identifier biomeId = CS$<>8__locals1.element.GetAttributeIdentifier("biome", Identifier.Empty);
			if (biomeId != Identifier.Empty)
			{
				Biome biome;
				if (Biome.Prefabs.TryGet(biomeId, out biome))
				{
					this.Biome = biome;
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(84, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Error while loading the campaign map: could not find a biome with the identifier \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(biomeId);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
			if (!typeNotFound)
			{
				for (int i = 0; i < this.Type.CanChangeTo.Count; i++)
				{
					for (int j = 0; j < this.Type.CanChangeTo[i].Requirements.Count; j++)
					{
						this.ProximityTimer.Add(this.Type.CanChangeTo[i].Requirements[j], CS$<>8__locals1.element.GetAttributeInt("proximitytimer" + i.ToString() + "-" + j.ToString(), 0));
					}
				}
				this.LoadLocationTypeChange(CS$<>8__locals1.element);
			}
			string[] takenItemStr = CS$<>8__locals1.element.GetAttributeStringArray("takenitems", Array.Empty<string>(), true, false);
			foreach (string takenItem in takenItemStr)
			{
				string[] takenItemSplit = takenItem.Split(';', StringSplitOptions.None);
				ushort id;
				int containerIndex;
				ushort moduleIndex;
				if (takenItemSplit.Length != 4)
				{
					DebugConsole.ThrowError("Error in saved location: could not parse taken item data \"" + takenItem + "\"", null, null, false, false);
				}
				else if (!ushort.TryParse(takenItemSplit[1], out id))
				{
					DebugConsole.ThrowError("Error in saved location: could not parse taken item id \"" + takenItemSplit[1] + "\"", null, null, false, false);
				}
				else if (!int.TryParse(takenItemSplit[2], out containerIndex))
				{
					DebugConsole.ThrowError("Error in saved location: could not parse taken container index \"" + takenItemSplit[2] + "\"", null, null, false, false);
				}
				else if (!ushort.TryParse(takenItemSplit[3], out moduleIndex))
				{
					DebugConsole.ThrowError("Error in saved location: could not parse taken item module index \"" + takenItemSplit[3] + "\"", null, null, false, false);
				}
				else
				{
					this.takenItems.Add(new Location.TakenItem(takenItemSplit[0].ToIdentifier(), id, containerIndex, moduleIndex));
				}
			}
			this.killedCharacterIdentifiers = CS$<>8__locals1.element.GetAttributeIntArray("killedcharacters", Array.Empty<int>()).ToHashSet<int>();
			if (this.Type == null)
			{
				this.Type = LocationType.Prefabs.First<LocationType>();
			}
			this.LevelData = new LevelData(CS$<>8__locals1.element.GetChildElement("Level", StringComparison.OrdinalIgnoreCase), null, true);
			this.PortraitId = ToolBox.StringToInt((!this.rawName.IsNullOrEmpty()) ? this.rawName : this.nameIdentifier.Value);
			this.LoadStores(CS$<>8__locals1.element);
			this.LoadMissions(CS$<>8__locals1.element);
		}

		// Token: 0x060028DC RID: 10460 RVA: 0x0010832C File Offset: 0x0010652C
		public void LoadChangingProperties(XElement element, CampaignMode campaign)
		{
			this.PriceMultiplier = element.GetAttributeFloat("PriceMultiplier", 1f);
			this.MechanicalPriceMultiplier = element.GetAttributeFloat("MechanicalPriceMultiplier", 1f);
			this.TurnsInRadiation = element.GetAttributeInt("TurnsInRadiation".ToLower(), 0);
			this.StepsSinceSpecialsUpdated = element.GetAttributeInt("StepsSinceSpecialsUpdated", 0);
			this.WorldStepsSinceVisited = element.GetAttributeInt("WorldStepsSinceVisited", 0);
			Identifier factionIdentifier = element.GetAttributeIdentifier("faction", Identifier.Empty);
			this.Faction = (factionIdentifier.IsEmpty ? null : campaign.Factions.Find((Faction f) => f.Prefab.Identifier == factionIdentifier));
			Identifier secondaryFactionIdentifier = element.GetAttributeIdentifier("secondaryfaction", Identifier.Empty);
			this.SecondaryFaction = (secondaryFactionIdentifier.IsEmpty ? null : campaign.Factions.Find((Faction f) => f.Prefab.Identifier == secondaryFactionIdentifier));
		}

		// Token: 0x060028DD RID: 10461 RVA: 0x0010842C File Offset: 0x0010662C
		public void LoadLocationTypeChange(XElement locationElement)
		{
			this.TimeSinceLastTypeChange = locationElement.GetAttributeInt("timesincelasttypechange", 0);
			this.LocationTypeChangeCooldown = locationElement.GetAttributeInt("locationtypechangecooldown", 0);
			foreach (XElement subElement in locationElement.Elements())
			{
				string a = subElement.Name.ToString();
				if (a == "pendinglocationtypechange")
				{
					int timer = subElement.GetAttributeInt("timer", 0);
					if (subElement.Attribute("index") != null)
					{
						int locationTypeChangeIndex = subElement.GetAttributeInt("index", 0);
						if (locationTypeChangeIndex < 0 || locationTypeChangeIndex >= this.Type.CanChangeTo.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(94, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to activate a location type change in the location \"");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName);
							defaultInterpolatedStringHandler.AppendLiteral("\". Location index out of bounds (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(locationTypeChangeIndex);
							defaultInterpolatedStringHandler.AppendLiteral(").");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							this.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(this.Type.CanChangeTo[locationTypeChangeIndex], timer, null));
						}
					}
					else
					{
						Identifier missionIdentifier = subElement.GetAttributeIdentifier("missionidentifier", "");
						MissionPrefab mission = MissionPrefab.Prefabs[missionIdentifier];
						if (mission == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(105, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Failed to activate a location type change from the mission \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(missionIdentifier);
							defaultInterpolatedStringHandler2.AppendLiteral("\" in location \"");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(this.DisplayName);
							defaultInterpolatedStringHandler2.AppendLiteral("\". Matching mission not found.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
						}
						else
						{
							this.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(mission.LocationTypeChangeOnCompleted, timer, mission));
						}
					}
				}
			}
		}

		// Token: 0x060028DE RID: 10462 RVA: 0x00108620 File Offset: 0x00106820
		public void LoadMissions(XElement locationElement)
		{
			XElement missionsElement = locationElement.GetChildElement("missions", StringComparison.OrdinalIgnoreCase);
			if (missionsElement != null)
			{
				this.loadedMissions = new List<Location.LoadedMission>();
				foreach (XElement childElement in missionsElement.GetChildElements("mission", StringComparison.OrdinalIgnoreCase))
				{
					Location.LoadedMission loadedMission = new Location.LoadedMission(childElement);
					if (loadedMission.MissionPrefab != null)
					{
						this.loadedMissions.Add(loadedMission);
					}
				}
			}
		}

		// Token: 0x060028DF RID: 10463 RVA: 0x001086A4 File Offset: 0x001068A4
		public static Location CreateRandom(Vector2 position, int? zone, Identifier? biomeId, Random rand, bool requireOutpost, LocationType forceLocationType = null, IEnumerable<Location> existingLocations = null)
		{
			return new Location(position, zone, biomeId, rand, requireOutpost, forceLocationType, existingLocations);
		}

		// Token: 0x060028E0 RID: 10464 RVA: 0x001086B8 File Offset: 0x001068B8
		public void ChangeType(CampaignMode campaign, LocationType newType, bool createStores = true, bool unlockInitialMissions = true)
		{
			if (newType == this.Type)
			{
				return;
			}
			if (newType == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to change the type of the location \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("\" to null.\n");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.Type = newType;
			if (this.rawName != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(37, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Location ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.rawName);
				defaultInterpolatedStringHandler2.AppendLiteral(" changed it's type from ");
				defaultInterpolatedStringHandler2.AppendFormatted<LocationType>(this.Type);
				defaultInterpolatedStringHandler2.AppendLiteral(" to ");
				defaultInterpolatedStringHandler2.AppendFormatted<LocationType>(newType);
				DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
				this.DisplayName = ((this.Type.NameFormats == null || !this.Type.NameFormats.Any<string>()) ? this.rawName : this.Type.NameFormats[this.nameFormatIndex % this.Type.NameFormats.Count].Replace("[name]", this.rawName));
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(37, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Location ");
				defaultInterpolatedStringHandler3.AppendFormatted(this.DisplayName.Value);
				defaultInterpolatedStringHandler3.AppendLiteral(" changed it's type from ");
				defaultInterpolatedStringHandler3.AppendFormatted<LocationType>(this.Type);
				defaultInterpolatedStringHandler3.AppendLiteral(" to ");
				defaultInterpolatedStringHandler3.AppendFormatted<LocationType>(newType);
				DebugConsole.Log(defaultInterpolatedStringHandler3.ToStringAndClear());
				this.DisplayName = ((this.Type.NameFormats == null || !this.Type.NameFormats.Any<string>()) ? TextManager.Get(this.nameIdentifier) : this.Type.NameFormats[this.nameFormatIndex % this.Type.NameFormats.Count].Replace("[name]", TextManager.Get(this.nameIdentifier).Value));
			}
			this.TryAssignFactionBasedOnLocationType(campaign);
			if (this.Type.HasOutpost && this.Type.OutpostTeam == CharacterTeamType.FriendlyNPC)
			{
				Identifier identifier = this.Type.Faction;
				if (identifier == Identifier.Empty && this.Faction == null)
				{
					this.Faction = campaign.GetRandomFaction(Rand.RandSync.Unsynced, true);
				}
				identifier = this.Type.SecondaryFaction;
				if (identifier == Identifier.Empty && this.SecondaryFaction == null)
				{
					this.SecondaryFaction = campaign.GetRandomSecondaryFaction(Rand.RandSync.Unsynced, true);
				}
			}
			else
			{
				Identifier identifier = this.Type.Faction;
				if (identifier == Identifier.Empty)
				{
					this.Faction = null;
				}
				identifier = this.Type.SecondaryFaction;
				if (identifier == Identifier.Empty)
				{
					this.SecondaryFaction = null;
				}
			}
			if (unlockInitialMissions && !this.IsCriticallyRadiated())
			{
				this.UnlockInitialMissions(Rand.RandSync.Unsynced);
			}
			if (createStores)
			{
				this.CreateStores(true);
				return;
			}
			this.ClearStores();
		}

		// Token: 0x060028E1 RID: 10465 RVA: 0x001089C8 File Offset: 0x00106BC8
		public void TryAssignFactionBasedOnLocationType(CampaignMode campaign)
		{
			Location.<>c__DisplayClass144_0 CS$<>8__locals1;
			CS$<>8__locals1.campaign = campaign;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.campaign == null)
			{
				return;
			}
			Identifier identifier = this.Type.Faction;
			if (identifier != Identifier.Empty)
			{
				identifier = this.Type.Faction;
				this.Faction = ((identifier == "None") ? null : this.<TryAssignFactionBasedOnLocationType>g__TryFindFaction|144_0(this.Type.Faction, ref CS$<>8__locals1));
			}
			identifier = this.Type.SecondaryFaction;
			if (identifier != Identifier.Empty)
			{
				identifier = this.Type.SecondaryFaction;
				this.SecondaryFaction = ((identifier == "None") ? null : this.<TryAssignFactionBasedOnLocationType>g__TryFindFaction|144_0(this.Type.SecondaryFaction, ref CS$<>8__locals1));
			}
		}

		// Token: 0x060028E2 RID: 10466 RVA: 0x00108A90 File Offset: 0x00106C90
		public void UnlockInitialMissions(Rand.RandSync randSync = Rand.RandSync.ServerAndClient)
		{
			if (this.Type.MissionIdentifiers.Any<Identifier>())
			{
				this.UnlockMissionByIdentifier(this.Type.MissionIdentifiers.GetRandom(randSync), this.Type.ContentPackage);
			}
			if (this.Type.MissionTags.Any<Identifier>())
			{
				this.UnlockMissionByTag(this.Type.MissionTags.GetRandom(randSync), null, this.Type.ContentPackage);
			}
		}

		// Token: 0x060028E3 RID: 10467 RVA: 0x00108B14 File Offset: 0x00106D14
		public void UnlockMission(MissionPrefab missionPrefab, LocationConnection connection)
		{
			if (this.AvailableMissions.Any((Mission m) => m.Prefab == missionPrefab))
			{
				return;
			}
			if (this.AvailableMissions.Any((Mission m) => !m.Prefab.AllowOtherMissionsInLevel))
			{
				return;
			}
			this.AddMission(this.InstantiateMission(missionPrefab, connection));
		}

		// Token: 0x060028E4 RID: 10468 RVA: 0x00108B88 File Offset: 0x00106D88
		public void UnlockMission(MissionPrefab missionPrefab)
		{
			if (this.AvailableMissions.Any((Mission m) => m.Prefab == missionPrefab))
			{
				return;
			}
			if (this.AvailableMissions.Any((Mission m) => !m.Prefab.AllowOtherMissionsInLevel))
			{
				return;
			}
			this.AddMission(this.InstantiateMission(missionPrefab));
		}

		// Token: 0x060028E5 RID: 10469 RVA: 0x00108BFC File Offset: 0x00106DFC
		public Mission UnlockMissionByIdentifier(Identifier identifier, ContentPackage invokingContentPackage = null)
		{
			Location.<>c__DisplayClass148_0 CS$<>8__locals1 = new Location.<>c__DisplayClass148_0();
			CS$<>8__locals1.identifier = identifier;
			if (this.AvailableMissions.Any((Mission m) => m.Prefab.Identifier == CS$<>8__locals1.identifier))
			{
				return null;
			}
			if (this.AvailableMissions.Any((Mission m) => !m.Prefab.AllowOtherMissionsInLevel))
			{
				return null;
			}
			CS$<>8__locals1.missionPrefab = MissionPrefab.Prefabs.Find((MissionPrefab mp) => mp.Identifier == CS$<>8__locals1.identifier);
			if (CS$<>8__locals1.missionPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to unlock a mission with the identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": matching mission not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, invokingContentPackage, false, false);
				return null;
			}
			LocationConnection connection;
			Mission mission = this.InstantiateMission(CS$<>8__locals1.missionPrefab, out connection);
			if (this.AvailableMissions.Any((Mission m) => m.Prefab == CS$<>8__locals1.missionPrefab && m.Locations.Contains(mission.Locations[0]) && m.Locations.Contains(mission.Locations[1])))
			{
				return null;
			}
			this.AddMission(mission);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Unlocked a mission by \"");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(CS$<>8__locals1.identifier);
			defaultInterpolatedStringHandler2.AppendLiteral("\".");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, true);
			return mission;
		}

		// Token: 0x060028E6 RID: 10470 RVA: 0x00108D68 File Offset: 0x00106F68
		public Mission UnlockMissionByTag(Identifier tag, Random random = null, ContentPackage invokingContentPackage = null)
		{
			Location.<>c__DisplayClass149_0 CS$<>8__locals1 = new Location.<>c__DisplayClass149_0();
			CS$<>8__locals1.tag = tag;
			CS$<>8__locals1.<>4__this = this;
			if (this.AvailableMissions.Any((Mission m) => !m.Prefab.AllowOtherMissionsInLevel))
			{
				return null;
			}
			IEnumerable<MissionPrefab> matchingMissions = from mp in MissionPrefab.Prefabs
			where mp.Tags.Contains(CS$<>8__locals1.tag)
			select mp;
			if (matchingMissions.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to unlock a mission with the tag \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.tag);
				defaultInterpolatedStringHandler.AppendLiteral("\": no matching missions found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, invokingContentPackage, false, false);
			}
			else
			{
				IEnumerable<MissionPrefab> unusedMissions = from m in matchingMissions
				where CS$<>8__locals1.<>4__this.availableMissions.None((Mission mission) => mission.Prefab == m)
				select m;
				if (unusedMissions.Any<MissionPrefab>())
				{
					Location.<>c__DisplayClass149_2 CS$<>8__locals2 = new Location.<>c__DisplayClass149_2();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					IEnumerable<MissionPrefab> suitableMissions = from m in unusedMissions
					where CS$<>8__locals2.CS$<>8__locals1.<>4__this.Connections.Any((LocationConnection c) => m.IsAllowed(CS$<>8__locals2.CS$<>8__locals1.<>4__this, c.OtherLocation(CS$<>8__locals2.CS$<>8__locals1.<>4__this)) || m.IsAllowed(CS$<>8__locals2.CS$<>8__locals1.<>4__this, CS$<>8__locals2.CS$<>8__locals1.<>4__this))
					select m;
					if (suitableMissions.None(null))
					{
						suitableMissions = unusedMissions;
					}
					IEnumerable<MissionPrefab> filteredMissions = from m in suitableMissions
					where CS$<>8__locals2.CS$<>8__locals1.<>4__this.LevelData.Difficulty >= (float)m.MinLevelDifficulty && CS$<>8__locals2.CS$<>8__locals1.<>4__this.LevelData.Difficulty <= (float)m.MaxLevelDifficulty
					select m;
					if (filteredMissions.None(null))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(99, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("No suitable mission matching the level difficulty ");
						defaultInterpolatedStringHandler2.AppendFormatted<float>(this.LevelData.Difficulty);
						defaultInterpolatedStringHandler2.AppendLiteral(" found with the tag \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(CS$<>8__locals2.CS$<>8__locals1.tag);
						defaultInterpolatedStringHandler2.AppendLiteral("\". Ignoring the restriction.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), invokingContentPackage);
					}
					else
					{
						suitableMissions = filteredMissions;
					}
					Location.<>c__DisplayClass149_2 CS$<>8__locals3 = CS$<>8__locals2;
					MissionPrefab missionPrefab;
					if (random == null)
					{
						missionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(suitableMissions, (MissionPrefab m) => (float)m.Commonness, Rand.RandSync.Unsynced);
					}
					else
					{
						missionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(suitableMissions, (MissionPrefab m) => (float)m.Commonness, random);
					}
					CS$<>8__locals3.missionPrefab = missionPrefab;
					LocationConnection connection;
					CS$<>8__locals2.mission = this.InstantiateMission(CS$<>8__locals2.missionPrefab, out connection);
					if (this.AvailableMissions.Any((Mission m) => m.Prefab == CS$<>8__locals2.missionPrefab && m.Locations.Contains(CS$<>8__locals2.mission.Locations[0]) && m.Locations.Contains(CS$<>8__locals2.mission.Locations[1])))
					{
						return null;
					}
					this.AddMission(CS$<>8__locals2.mission);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(54, 3);
					defaultInterpolatedStringHandler3.AppendLiteral("Unlocked a random mission by \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(CS$<>8__locals2.CS$<>8__locals1.tag);
					defaultInterpolatedStringHandler3.AppendLiteral("\": ");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(CS$<>8__locals2.mission.Prefab.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(" (difficulty level: ");
					defaultInterpolatedStringHandler3.AppendFormatted<float>(this.LevelData.Difficulty);
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, true);
					return CS$<>8__locals2.mission;
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(94, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("Failed to unlock a mission with the tag \"");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(CS$<>8__locals1.tag);
					defaultInterpolatedStringHandler4.AppendLiteral("\": all available missions have already been unlocked.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler4.ToStringAndClear(), invokingContentPackage);
				}
			}
			return null;
		}

		// Token: 0x060028E7 RID: 10471 RVA: 0x00109068 File Offset: 0x00107268
		private void AddMission(Mission mission)
		{
			if (!mission.Prefab.AllowOtherMissionsInLevel)
			{
				this.availableMissions.Clear();
			}
			this.availableMissions.Add(mission);
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign multiPlayerCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (multiPlayerCampaign == null)
			{
				return;
			}
			multiPlayerCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
		}

		// Token: 0x060028E8 RID: 10472 RVA: 0x001090BC File Offset: 0x001072BC
		private Mission InstantiateMission(MissionPrefab prefab, out LocationConnection connection)
		{
			if (prefab.IsAllowed(this, this))
			{
				connection = null;
				return this.InstantiateMission(prefab);
			}
			IEnumerable<LocationConnection> suitableConnections = from c in this.Connections
			where prefab.IsAllowed(this, c.OtherLocation(this))
			select c;
			if (suitableConnections.None(null))
			{
				suitableConnections = this.Connections.ToList<LocationConnection>();
			}
			connection = ToolBox.SelectWeightedRandom<LocationConnection>(suitableConnections.ToList<LocationConnection>(), (from c in suitableConnections
			select Location.<InstantiateMission>g__GetConnectionWeight|151_2(this, c)).ToList<float>(), Rand.RandSync.Unsynced);
			return this.InstantiateMission(prefab, connection);
		}

		// Token: 0x060028E9 RID: 10473 RVA: 0x0010915C File Offset: 0x0010735C
		private Mission InstantiateMission(MissionPrefab prefab, LocationConnection connection)
		{
			Location destination = connection.OtherLocation(this);
			Mission mission = prefab.Instantiate(new Location[]
			{
				this,
				destination
			}, Submarine.MainSub);
			mission.AdjustLevelData(connection.LevelData);
			return mission;
		}

		// Token: 0x060028EA RID: 10474 RVA: 0x00109198 File Offset: 0x00107398
		private Mission InstantiateMission(MissionPrefab prefab)
		{
			Mission mission = prefab.Instantiate(new Location[]
			{
				this,
				this
			}, Submarine.MainSub);
			mission.AdjustLevelData(this.LevelData);
			return mission;
		}

		// Token: 0x060028EB RID: 10475 RVA: 0x001091CC File Offset: 0x001073CC
		public void InstantiateLoadedMissions(Map map)
		{
			this.availableMissions.Clear();
			this.selectedMissions.Clear();
			if (this.loadedMissions != null && this.loadedMissions.Any<Location.LoadedMission>())
			{
				foreach (Location.LoadedMission loadedMission in this.loadedMissions)
				{
					Location destination;
					if (loadedMission.DestinationIndex >= 0 && loadedMission.DestinationIndex < map.Locations.Count)
					{
						destination = map.Locations[loadedMission.DestinationIndex];
					}
					else
					{
						destination = this.Connections.First<LocationConnection>().OtherLocation(this);
					}
					Mission mission = loadedMission.MissionPrefab.Instantiate(new Location[]
					{
						this,
						destination
					}, Submarine.MainSub);
					if (loadedMission.OriginLocationIndex >= 0 && loadedMission.OriginLocationIndex < map.Locations.Count)
					{
						mission.OriginLocation = map.Locations[loadedMission.OriginLocationIndex];
					}
					mission.TimesAttempted = loadedMission.TimesAttempted;
					this.availableMissions.Add(mission);
					if (loadedMission.SelectedMission)
					{
						this.selectedMissions.Add(mission);
					}
					LevelData levelData2;
					if (destination != this)
					{
						LocationConnection locationConnection = this.Connections.FirstOrDefault((LocationConnection c) => c.OtherLocation(this) == destination);
						levelData2 = ((locationConnection != null) ? locationConnection.LevelData : null);
					}
					else
					{
						levelData2 = this.LevelData;
					}
					LevelData levelData = levelData2;
					if (levelData != null)
					{
						mission.AdjustLevelData(levelData);
					}
				}
				this.loadedMissions = null;
			}
			if (this.addInitialMissionsForType != null)
			{
				if (this.addInitialMissionsForType.MissionIdentifiers.Any<Identifier>())
				{
					this.UnlockMissionByIdentifier(this.addInitialMissionsForType.MissionIdentifiers.GetRandomUnsynced<Identifier>(), this.Type.ContentPackage);
				}
				if (this.addInitialMissionsForType.MissionTags.Any<Identifier>())
				{
					this.UnlockMissionByTag(this.addInitialMissionsForType.MissionTags.GetRandomUnsynced<Identifier>(), null, this.Type.ContentPackage);
				}
				this.addInitialMissionsForType = null;
			}
		}

		// Token: 0x060028EC RID: 10476 RVA: 0x00109404 File Offset: 0x00107604
		public void ClearMissions()
		{
			this.availableMissions.Clear();
			this.selectedMissions.Clear();
		}

		// Token: 0x060028ED RID: 10477 RVA: 0x0010941C File Offset: 0x0010761C
		public bool HasOutpost()
		{
			return this.Type.HasOutpost && !this.IsCriticallyRadiated();
		}

		// Token: 0x060028EE RID: 10478 RVA: 0x00109438 File Offset: 0x00107638
		public bool IsCriticallyRadiated()
		{
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (gameSession == null)
			{
				flag = (null != null);
			}
			else
			{
				Map map = gameSession.Map;
				flag = (((map != null) ? map.Radiation : null) != null);
			}
			return flag && this.TurnsInRadiation > GameMain.GameSession.Map.Radiation.Params.CriticalRadiationThreshold;
		}

		// Token: 0x060028EF RID: 10479 RVA: 0x00109488 File Offset: 0x00107688
		public LocationType GetLocationTypeToDisplay(out Identifier overrideDescriptionIdentifier)
		{
			overrideDescriptionIdentifier = Identifier.Empty;
			if (this.IsCriticallyRadiated() && !this.Type.ReplaceInRadiation.IsEmpty)
			{
				LocationType newLocationType;
				if (LocationType.Prefabs.TryGet(this.Type.ReplaceInRadiation, out newLocationType))
				{
					if (!newLocationType.DescriptionInRadiation.IsEmpty)
					{
						overrideDescriptionIdentifier = newLocationType.DescriptionInRadiation;
					}
					return newLocationType;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(101, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error when trying to get a new location type for an irradiated location - location type \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocationType>(newLocationType);
				defaultInterpolatedStringHandler.AppendLiteral("\" not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			return this.Type;
		}

		// Token: 0x060028F0 RID: 10480 RVA: 0x00109538 File Offset: 0x00107738
		public LocationType GetLocationTypeToDisplay()
		{
			Identifier identifier;
			return this.GetLocationTypeToDisplay(out identifier);
		}

		// Token: 0x060028F1 RID: 10481 RVA: 0x00109550 File Offset: 0x00107750
		public IEnumerable<Mission> GetMissionsInConnection(LocationConnection connection)
		{
			return from m in this.AvailableMissions
			where m.Locations[1] == connection.OtherLocation(this)
			select m;
		}

		// Token: 0x060028F2 RID: 10482 RVA: 0x00109588 File Offset: 0x00107788
		public void RemoveHireableCharacter(CharacterInfo character)
		{
			if (!this.Type.HasHireableCharacters)
			{
				DebugConsole.ThrowErrorLocalized("Cannot hire a character from location \"" + this.DisplayName + "\" - the location has no hireable characters.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (this.HireManager == null)
			{
				DebugConsole.ThrowErrorLocalized("Cannot hire a character from location \"" + this.DisplayName + "\" - hire manager has not been instantiated.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.HireManager.RemoveCharacter(character);
		}

		// Token: 0x060028F3 RID: 10483 RVA: 0x0010963C File Offset: 0x0010783C
		public IEnumerable<CharacterInfo> GetHireableCharacters()
		{
			if (!this.Type.HasHireableCharacters)
			{
				return Enumerable.Empty<CharacterInfo>();
			}
			if (this.HireManager == null)
			{
				this.HireManager = new HireManager();
			}
			if (!this.HireManager.AvailableCharacters.Any<CharacterInfo>())
			{
				this.HireManager.GenerateCharacters(this, 6);
			}
			return this.HireManager.AvailableCharacters;
		}

		// Token: 0x060028F4 RID: 10484 RVA: 0x00109699 File Offset: 0x00107899
		public void ForceHireableCharacters(IEnumerable<CharacterInfo> hireableCharacters)
		{
			if (this.HireManager == null)
			{
				this.HireManager = new HireManager();
			}
			this.HireManager.AvailableCharacters = hireableCharacters.ToList<CharacterInfo>();
		}

		// Token: 0x060028F5 RID: 10485 RVA: 0x001096C0 File Offset: 0x001078C0
		public void AssignRandomName(LocationType type, Random rand, IEnumerable<Location> existingLocations)
		{
			if (!type.ForceLocationName.IsEmpty)
			{
				this.nameIdentifier = type.ForceLocationName;
				this.DisplayName = TextManager.Get(this.nameIdentifier).Fallback(this.nameIdentifier.Value, true);
				return;
			}
			this.nameIdentifier = type.GetRandomNameId(rand, existingLocations);
			if (this.nameIdentifier.IsEmpty)
			{
				this.rawName = type.GetRandomRawName(rand, existingLocations);
				if (this.rawName.IsNullOrEmpty())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(110, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to generate a name for a location of the type ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(type.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(". No names found in localization files or the .txt files.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					this.rawName = "none";
				}
				this.nameIdentifier = this.rawName.ToIdentifier();
				if (type.NameFormats == null || !type.NameFormats.Any<string>())
				{
					this.DisplayName = this.rawName;
					return;
				}
				this.nameFormatIndex = rand.Next() % type.NameFormats.Count;
				this.DisplayName = type.NameFormats[this.nameFormatIndex].Replace("[name]", this.rawName);
				return;
			}
			else
			{
				if (type.NameFormats == null || !type.NameFormats.Any<string>())
				{
					this.DisplayName = TextManager.Get(this.nameIdentifier).Fallback(this.nameIdentifier.Value, true);
					return;
				}
				this.nameFormatIndex = rand.Next() % type.NameFormats.Count;
				this.DisplayName = Location.GetName(this.Type, this.nameFormatIndex, this.nameIdentifier);
				return;
			}
		}

		// Token: 0x060028F6 RID: 10486 RVA: 0x00109880 File Offset: 0x00107A80
		public static LocalizedString GetName(Identifier locationTypeIdentifier, int nameFormatIndex, Identifier nameId)
		{
			LocationType locationType;
			if (LocationType.Prefabs.TryGet(locationTypeIdentifier, out locationType))
			{
				return Location.GetName(locationType, nameFormatIndex, nameId);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Could not find the location type ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(locationTypeIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral(".\n");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace.CleanUpPath(), null, null, false, false);
			return new RawLString(nameId.Value);
		}

		// Token: 0x060028F7 RID: 10487 RVA: 0x001098FC File Offset: 0x00107AFC
		public static LocalizedString GetName(LocationType type, int nameFormatIndex, Identifier nameId)
		{
			if (((type != null) ? type.NameFormats : null) == null || !type.NameFormats.Any<string>() || nameFormatIndex < 0)
			{
				return TextManager.Get(nameId).Fallback(nameId.Value, true);
			}
			return type.NameFormats[nameFormatIndex % type.NameFormats.Count].Replace("[name]", TextManager.Get(nameId).Value);
		}

		// Token: 0x060028F8 RID: 10488 RVA: 0x00109973 File Offset: 0x00107B73
		public void ForceName(Identifier nameId)
		{
			this.rawName = string.Empty;
			this.nameIdentifier = nameId;
			this.DisplayName = TextManager.Get(nameId).Fallback(nameId.Value, true);
		}

		// Token: 0x060028F9 RID: 10489 RVA: 0x001099A8 File Offset: 0x00107BA8
		public void LoadStores(XElement locationElement)
		{
			this.UpdateStoreIdentifiers();
			Dictionary<Identifier, Location.StoreInfo> stores = this.Stores;
			if (stores != null)
			{
				stores.Clear();
			}
			bool hasStores = false;
			foreach (XElement storeElement in locationElement.GetChildElements("store", StringComparison.OrdinalIgnoreCase))
			{
				hasStores = true;
				if (this.Stores == null)
				{
					this.Stores = new Dictionary<Identifier, Location.StoreInfo>();
				}
				Identifier identifier = storeElement.GetAttributeIdentifier("identifier", "");
				if (!identifier.IsEmpty)
				{
					if (this.StoreIdentifiers.Contains(identifier))
					{
						if (!this.Stores.ContainsKey(identifier))
						{
							this.Stores.Add(identifier, new Location.StoreInfo(this, storeElement));
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Error loading store info for \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" at location ");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName);
							defaultInterpolatedStringHandler.AppendLiteral(" of type \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Type.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\": duplicate identifier.");
							string msg = defaultInterpolatedStringHandler.ToStringAndClear();
							DebugConsole.ThrowError(msg, null, null, false, false);
							GameAnalyticsManager.AddErrorEventOnce("Location.LoadStore:DuplicateStoreInfo", GameAnalyticsManager.ErrorSeverity.Error, msg);
						}
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(113, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("Error loading store info for \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\" at location ");
						defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(this.DisplayName);
						defaultInterpolatedStringHandler2.AppendLiteral(" of type \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Type.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\": location shouldn't contain a store with this identifier.");
						string msg2 = defaultInterpolatedStringHandler2.ToStringAndClear();
						DebugConsole.ThrowError(msg2, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("Location.LoadStore:IncorrectStoreIdentifier", GameAnalyticsManager.ErrorSeverity.Error, msg2);
					}
				}
			}
			if (hasStores)
			{
				foreach (Identifier id in this.StoreIdentifiers)
				{
					this.AddNewStore(id);
				}
			}
		}

		// Token: 0x060028FA RID: 10490 RVA: 0x00109BE8 File Offset: 0x00107DE8
		public bool IsRadiated()
		{
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (gameSession == null)
			{
				flag = (null != null);
			}
			else
			{
				Map map = gameSession.Map;
				flag = (((map != null) ? map.Radiation : null) != null);
			}
			return flag && GameMain.GameSession.Map.Radiation.Enabled && GameMain.GameSession.Map.Radiation.DepthInRadiation(this) > 0f;
		}

		// Token: 0x060028FB RID: 10491 RVA: 0x00109C48 File Offset: 0x00107E48
		public void RegisterTakenItems(IEnumerable<Item> items)
		{
			using (IEnumerator<Item> enumerator = items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					if (!this.takenItems.Any((Location.TakenItem it) => it.Matches(item) && it.OriginalID == item.ID) && !item.IsSalvageMissionItem)
					{
						if (item.OriginalModuleIndex < 0)
						{
							DebugConsole.ThrowError("Tried to register a non-outpost item as being taken from the outpost.", null, null, false, false);
						}
						else
						{
							this.takenItems.Add(new Location.TakenItem(item));
						}
					}
				}
			}
		}

		// Token: 0x060028FC RID: 10492 RVA: 0x00109CF0 File Offset: 0x00107EF0
		public void RegisterKilledCharacters(IEnumerable<Character> characters)
		{
			foreach (Character character in characters)
			{
				if (((character != null) ? character.Info : null) != null)
				{
					this.killedCharacterIdentifiers.Add(character.Info.GetIdentifier());
				}
			}
		}

		// Token: 0x060028FD RID: 10493 RVA: 0x00109D58 File Offset: 0x00107F58
		public void RemoveTakenItems()
		{
			using (List<Location.TakenItem>.Enumerator enumerator = this.takenItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Location.TakenItem takenItem = enumerator.Current;
					Item item = Item.ItemList.Find((Item it) => takenItem.Matches(it));
					if (item != null)
					{
						item.Remove();
					}
				}
			}
		}

		// Token: 0x060028FE RID: 10494 RVA: 0x00109DD0 File Offset: 0x00107FD0
		public int GetAdjustedMechanicalCost(int cost)
		{
			float discount = 0f;
			if (this.Reputation != null)
			{
				discount = this.Reputation.Value / (float)this.Reputation.MaxReputation * 0.5f;
			}
			return (int)Math.Ceiling((double)((1f - discount) * (float)cost * this.MechanicalPriceMultiplier));
		}

		// Token: 0x060028FF RID: 10495 RVA: 0x00109E24 File Offset: 0x00108024
		public int GetAdjustedHealCost(int cost)
		{
			float discount = 0f;
			if (this.Reputation != null)
			{
				discount = this.Reputation.Value / (float)this.Reputation.MaxReputation * 0.1f;
			}
			return (int)Math.Ceiling((double)((1f - discount) * (float)cost * this.PriceMultiplier));
		}

		// Token: 0x06002900 RID: 10496 RVA: 0x00109E78 File Offset: 0x00108078
		public Location.StoreInfo GetStore(Identifier identifier)
		{
			Location.StoreInfo store;
			if (this.Stores != null && this.Stores.TryGetValue(identifier, out store))
			{
				return store;
			}
			return null;
		}

		// Token: 0x06002901 RID: 10497 RVA: 0x00109EA0 File Offset: 0x001080A0
		public void CreateStores(bool force = false)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (!force && this.Stores != null)
			{
				return;
			}
			this.UpdateStoreIdentifiers();
			if (this.Stores != null)
			{
				foreach (Identifier storeIdentifier in this.Stores.Keys)
				{
					if (!this.StoreIdentifiers.Contains(storeIdentifier))
					{
						this.Stores.Remove(storeIdentifier);
					}
				}
				using (HashSet<Identifier>.Enumerator enumerator2 = this.StoreIdentifiers.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Identifier identifier = enumerator2.Current;
						Location.StoreInfo store;
						if (this.Stores.TryGetValue(identifier, out store))
						{
							store.Balance = Math.Max(store.Balance, this.StoreInitialBalance);
							List<PurchasedItem> newStock = store.CreateStock();
							using (List<PurchasedItem>.Enumerator enumerator3 = store.Stock.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									PurchasedItem oldStockItem = enumerator3.Current;
									PurchasedItem newStockItem = newStock.Find((PurchasedItem i) => i.ItemPrefab == oldStockItem.ItemPrefab);
									if (newStockItem != null && oldStockItem.Quantity > newStockItem.Quantity)
									{
										newStockItem.Quantity = oldStockItem.Quantity;
									}
								}
							}
							store.Stock.Clear();
							store.Stock.AddRange(newStock);
							store.GenerateSpecials();
							store.GeneratePriceModifier();
						}
						else
						{
							this.AddNewStore(identifier);
						}
					}
					return;
				}
			}
			foreach (Identifier identifier2 in this.StoreIdentifiers)
			{
				this.AddNewStore(identifier2);
			}
		}

		// Token: 0x06002902 RID: 10498 RVA: 0x0010A0E8 File Offset: 0x001082E8
		public void UpdateStores(bool createStoresIfNotCreated = true)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			if (this.Stores == null)
			{
				if (createStoresIfNotCreated)
				{
					this.CreateStores(false);
				}
				return;
			}
			HashSet<Identifier> storesToRemove = new HashSet<Identifier>();
			foreach (Location.StoreInfo store in this.Stores.Values)
			{
				if (!this.StoreIdentifiers.Contains(store.Identifier))
				{
					storesToRemove.Add(store.Identifier);
				}
				else
				{
					if (store.Balance < this.StoreInitialBalance)
					{
						store.Balance = Math.Min(store.Balance + (int)((float)this.StoreInitialBalance / 10f), this.StoreInitialBalance);
					}
					List<PurchasedItem> stock = new List<PurchasedItem>(store.Stock);
					List<PurchasedItem> stockToRemove = new List<PurchasedItem>();
					using (IEnumerator<ItemPrefab> enumerator2 = ItemPrefab.Prefabs.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							ItemPrefab itemPrefab = enumerator2.Current;
							PurchasedItem existingStock = stock.FirstOrDefault(delegate(PurchasedItem s)
							{
								Identifier itemPrefabIdentifier = s.ItemPrefabIdentifier;
								return itemPrefabIdentifier == itemPrefab.Identifier;
							});
							PriceInfo priceInfo;
							if (itemPrefab.CanBeBoughtFrom(store, out priceInfo))
							{
								if (existingStock == null)
								{
									stock.Add(Location.StoreInfo.CreateInitialStockItem(this, itemPrefab, priceInfo));
								}
								else
								{
									existingStock.Quantity = Math.Min(existingStock.Quantity + 1, priceInfo.MaxAvailableAmount);
								}
							}
							else if (existingStock != null)
							{
								stockToRemove.Add(existingStock);
							}
						}
					}
					stockToRemove.ForEach(delegate(PurchasedItem i)
					{
						stock.Remove(i);
					});
					store.Stock.Clear();
					store.Stock.AddRange(stock);
					store.GeneratePriceModifier();
				}
			}
			int stepsSinceSpecialsUpdated = this.StepsSinceSpecialsUpdated;
			this.StepsSinceSpecialsUpdated = stepsSinceSpecialsUpdated + 1;
			foreach (Identifier identifier in storesToRemove)
			{
				this.Stores.Remove(identifier);
			}
			foreach (Identifier identifier2 in this.StoreIdentifiers)
			{
				this.AddNewStore(identifier2);
			}
		}

		// Token: 0x06002903 RID: 10499 RVA: 0x0010A3AC File Offset: 0x001085AC
		public void UpdateSpecials()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if ((networkMember != null && networkMember.IsClient) || this.Stores == null)
			{
				return;
			}
			int extraSpecialSalesCount = Location.GetExtraSpecialSalesCount();
			foreach (Location.StoreInfo store in this.Stores.Values)
			{
				if (this.StepsSinceSpecialsUpdated >= 3 || store.DailySpecials.Count != this.DailySpecialsCount + extraSpecialSalesCount)
				{
					store.GenerateSpecials();
				}
			}
		}

		// Token: 0x06002904 RID: 10500 RVA: 0x0010A444 File Offset: 0x00108644
		private void UpdateStoreIdentifiers()
		{
			this.StoreIdentifiers.Clear();
			foreach (OutpostGenerationParams outpostParam in OutpostGenerationParams.OutpostParams)
			{
				if (outpostParam.AllowedLocationTypes.Contains(this.Type.Identifier))
				{
					foreach (Identifier identifier in outpostParam.GetStoreIdentifiers())
					{
						this.StoreIdentifiers.Add(identifier);
					}
				}
			}
		}

		// Token: 0x06002905 RID: 10501 RVA: 0x0010A4F4 File Offset: 0x001086F4
		private void AddNewStore(Identifier identifier)
		{
			if (this.Stores == null)
			{
				this.Stores = new Dictionary<Identifier, Location.StoreInfo>();
			}
			if (this.Stores.ContainsKey(identifier))
			{
				return;
			}
			Location.StoreInfo newStore = new Location.StoreInfo(this, identifier);
			this.Stores.Add(identifier, newStore);
		}

		// Token: 0x06002906 RID: 10502 RVA: 0x0010A53C File Offset: 0x0010873C
		public void AddStock(Dictionary<Identifier, List<SoldItem>> items)
		{
			if (items == null)
			{
				return;
			}
			foreach (KeyValuePair<Identifier, List<SoldItem>> storeItems in items)
			{
				Location.StoreInfo store = this.GetStore(storeItems.Key);
				if (store != null)
				{
					store.AddStock(storeItems.Value);
				}
			}
		}

		// Token: 0x06002907 RID: 10503 RVA: 0x0010A5A8 File Offset: 0x001087A8
		public void ClearStores()
		{
			this.Stores = null;
		}

		// Token: 0x06002908 RID: 10504 RVA: 0x0010A5B4 File Offset: 0x001087B4
		public void RemoveStock(Dictionary<Identifier, List<PurchasedItem>> items)
		{
			if (items == null)
			{
				return;
			}
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> storeItems in items)
			{
				Location.StoreInfo store = this.GetStore(storeItems.Key);
				if (store != null)
				{
					store.RemoveStock(storeItems.Value);
				}
			}
		}

		// Token: 0x06002909 RID: 10505 RVA: 0x0010A620 File Offset: 0x00108820
		public static int GetExtraSpecialSalesCount()
		{
			ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			if (!characters.Any<Character>())
			{
				return 0;
			}
			return characters.Max((Character c) => (int)c.GetStatValue(StatTypes.ExtraSpecialSalesCount, true));
		}

		// Token: 0x0600290A RID: 10506 RVA: 0x0010A663 File Offset: 0x00108863
		public bool CanHaveSubsForSale()
		{
			return this.HasOutpost() && this.CanHaveCampaignInteraction(CampaignMode.InteractionType.PurchaseSub);
		}

		// Token: 0x0600290B RID: 10507 RVA: 0x0010A676 File Offset: 0x00108876
		public int HighestSubmarineTierAvailable(SubmarineClass submarineClass = SubmarineClass.Undefined)
		{
			if (!this.CanHaveSubsForSale())
			{
				return 0;
			}
			Biome biome = this.Biome;
			if (biome == null)
			{
				return 3;
			}
			return biome.HighestSubmarineTierAvailable(submarineClass, this.Type.Identifier);
		}

		// Token: 0x0600290C RID: 10508 RVA: 0x0010A69F File Offset: 0x0010889F
		public bool IsSubmarineAvailable(SubmarineInfo info)
		{
			Biome biome = this.Biome;
			return biome == null || biome.IsSubmarineAvailable(info, this.Type.Identifier);
		}

		// Token: 0x0600290D RID: 10509 RVA: 0x0010A6C0 File Offset: 0x001088C0
		private bool CanHaveCampaignInteraction(CampaignMode.InteractionType interactionType)
		{
			return this.LevelData != null && this.LevelData.OutpostGenerationParamsExist && LevelData.GetSuitableOutpostGenerationParams(this, this.LevelData).Any((OutpostGenerationParams p) => p.CanHaveCampaignInteraction(interactionType));
		}

		// Token: 0x0600290E RID: 10510 RVA: 0x0010A710 File Offset: 0x00108910
		public void Reset(CampaignMode campaign)
		{
			if (this.Type != this.OriginalType && !this.DisallowLocationTypeChanges)
			{
				this.ChangeType(campaign, this.OriginalType, true, true);
				this.PendingLocationTypeChange = null;
			}
			this.ClearStores();
			this.ClearMissions();
			LevelData levelData = this.LevelData;
			if (levelData != null)
			{
				List<Identifier> eventHistory = levelData.EventHistory;
				if (eventHistory != null)
				{
					eventHistory.Clear();
				}
			}
			this.UnlockInitialMissions(Rand.RandSync.ServerAndClient);
		}

		// Token: 0x0600290F RID: 10511 RVA: 0x0010A780 File Offset: 0x00108980
		public XElement Save(Map map, XElement parentElement)
		{
			XName name = "location";
			object[] array = new object[12];
			array[0] = new XAttribute("type", this.Type.Identifier);
			array[1] = new XAttribute("originaltype", (this.Type ?? this.OriginalType).Identifier);
			array[2] = new XAttribute("name", this.DisplayName);
			int num = 3;
			XName name2 = "biome";
			Biome biome = this.Biome;
			array[num] = new XAttribute(name2, ((biome != null) ? biome.Identifier.Value : null) ?? string.Empty);
			array[4] = new XAttribute("position", XMLExtensions.Vector2ToString(this.MapPosition));
			array[5] = new XAttribute("pricemultiplier", this.PriceMultiplier);
			array[6] = new XAttribute("isgatebetweenbiomes", this.IsGateBetweenBiomes);
			array[7] = new XAttribute("mechanicalpricemultipler", this.MechanicalPriceMultiplier);
			array[8] = new XAttribute("timesincelasttypechange", this.TimeSinceLastTypeChange);
			array[9] = new XAttribute("TurnsInRadiation".ToLower(), this.TurnsInRadiation);
			array[10] = new XAttribute("StepsSinceSpecialsUpdated", this.StepsSinceSpecialsUpdated);
			array[11] = new XAttribute("WorldStepsSinceVisited", this.WorldStepsSinceVisited);
			XElement locationElement = new XElement(name, array);
			if (!this.rawName.IsNullOrEmpty())
			{
				locationElement.Add(new XAttribute("rawName", this.rawName));
			}
			else
			{
				locationElement.Add(new XAttribute("nameIdentifier", this.nameIdentifier));
				locationElement.Add(new XAttribute("nameFormatIndex", this.nameFormatIndex));
			}
			if (this.Faction != null)
			{
				locationElement.Add(new XAttribute("faction", this.Faction.Prefab.Identifier));
			}
			if (this.SecondaryFaction != null)
			{
				locationElement.Add(new XAttribute("secondaryfaction", this.SecondaryFaction.Prefab.Identifier));
			}
			this.LevelData.Save(locationElement);
			for (int i = 0; i < this.Type.CanChangeTo.Count; i++)
			{
				for (int j = 0; j < this.Type.CanChangeTo[i].Requirements.Count; j++)
				{
					if (this.ProximityTimer.ContainsKey(this.Type.CanChangeTo[i].Requirements[j]))
					{
						locationElement.Add(new XAttribute("proximitytimer" + i.ToString() + "-" + j.ToString(), this.ProximityTimer[this.Type.CanChangeTo[i].Requirements[j]]));
					}
				}
			}
			if (this.PendingLocationTypeChange != null)
			{
				XElement changeElement = new XElement("pendinglocationtypechange", new XAttribute("timer", this.PendingLocationTypeChange.Value.Item2));
				if (this.PendingLocationTypeChange.Value.Item3 != null)
				{
					changeElement.Add(new XAttribute("missionidentifier", this.PendingLocationTypeChange.Value.Item3.Identifier));
					locationElement.Add(changeElement);
				}
				else
				{
					int index = this.Type.CanChangeTo.IndexOf(this.PendingLocationTypeChange.Value.Item1);
					changeElement.Add(new XAttribute("index", index));
					if (index == -1)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Invalid location type change in the location \"");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName);
						defaultInterpolatedStringHandler.AppendLiteral("\". Unknown type change (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.PendingLocationTypeChange.Value.Item1.ChangeToType);
						defaultInterpolatedStringHandler.AppendLiteral(").");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					else
					{
						locationElement.Add(changeElement);
					}
				}
			}
			if (this.LocationTypeChangeCooldown > 0)
			{
				locationElement.Add(new XAttribute("locationtypechangecooldown", this.LocationTypeChangeCooldown));
			}
			if (this.takenItems.Any<Location.TakenItem>())
			{
				locationElement.Add(new XAttribute("takenitems", string.Join<string>(',', from it in this.takenItems
				select string.Concat(new string[]
				{
					it.Identifier.ToString(),
					";",
					it.OriginalID.ToString(),
					";",
					it.OriginalContainerIndex.ToString(),
					";",
					it.ModuleIndex.ToString()
				}))));
			}
			if (this.killedCharacterIdentifiers.Any<int>())
			{
				locationElement.Add(new XAttribute("killedcharacters", string.Join<int>(',', this.killedCharacterIdentifiers)));
			}
			if (this.Stores != null)
			{
				foreach (Location.StoreInfo store in this.Stores.Values)
				{
					XElement storeElement = new XElement("store", new object[]
					{
						new XAttribute("identifier", store.Identifier.Value),
						new XAttribute("MerchantFaction", store.MerchantFaction),
						new XAttribute("balance", store.Balance),
						new XAttribute("pricemodifier", store.PriceModifier)
					});
					foreach (PurchasedItem item in store.Stock)
					{
						if (((item != null) ? item.ItemPrefab : null) != null)
						{
							storeElement.Add(new XElement("stock", new object[]
							{
								new XAttribute("id", item.ItemPrefab.Identifier),
								new XAttribute("qty", item.Quantity)
							}));
						}
					}
					if (store.DailySpecials.Any<ItemPrefab>())
					{
						XElement dailySpecialElement = new XElement("dailyspecials");
						foreach (ItemPrefab item2 in store.DailySpecials)
						{
							dailySpecialElement.Add(new XElement("item", new XAttribute("id", item2.Identifier)));
						}
						storeElement.Add(dailySpecialElement);
					}
					if (store.RequestedGoods.Any<ItemPrefab>())
					{
						XElement requestedGoodsElement = new XElement("requestedgoods");
						foreach (ItemPrefab item3 in store.RequestedGoods)
						{
							requestedGoodsElement.Add(new XElement("item", new XAttribute("id", item3.Identifier)));
						}
						storeElement.Add(requestedGoodsElement);
					}
					locationElement.Add(storeElement);
				}
			}
			List<Mission> missions = this.AvailableMissions as List<Mission>;
			if (missions != null && missions.Any<Mission>())
			{
				XElement missionsElement = new XElement("missions");
				foreach (Mission mission in missions)
				{
					Location location = mission.Locations.All((Location l) => l == this) ? this : mission.Locations.FirstOrDefault((Location l) => l != this);
					int destinationIndex = map.Locations.IndexOf(location);
					int originIndex = map.Locations.IndexOf(mission.OriginLocation);
					missionsElement.Add(new XElement("mission", new object[]
					{
						new XAttribute("prefabid", mission.Prefab.Identifier),
						new XAttribute("destinationindex", destinationIndex),
						new XAttribute("TimesAttempted", mission.TimesAttempted),
						new XAttribute("origin", originIndex),
						new XAttribute("selected", this.selectedMissions.Contains(mission))
					}));
				}
				locationElement.Add(missionsElement);
			}
			parentElement.Add(locationElement);
			return locationElement;
		}

		// Token: 0x06002910 RID: 10512 RVA: 0x0010B158 File Offset: 0x00109358
		public void Remove()
		{
			this.RemoveProjSpecific();
		}

		// Token: 0x06002911 RID: 10513 RVA: 0x0010B160 File Offset: 0x00109360
		public void RemoveProjSpecific()
		{
			HireManager hireManager = this.HireManager;
			if (hireManager == null)
			{
				return;
			}
			hireManager.Remove();
		}

		// Token: 0x06002914 RID: 10516 RVA: 0x0010B1B4 File Offset: 0x001093B4
		[CompilerGenerated]
		private bool <.ctor>g__GetTypeOrFallback|138_0(Identifier identifier, out LocationType type, ref Location.<>c__DisplayClass138_0 A_3)
		{
			if (!LocationType.Prefabs.TryGet(identifier, out type))
			{
				if (identifier == "lair")
				{
					LocationType.Prefabs.TryGet("Abandoned".ToIdentifier(), out type);
					this.addInitialMissionsForType = this.Type;
				}
				if (type == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find location type \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". Using location type \"None\" instead.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					LocationType.Prefabs.TryGet("None".ToIdentifier(), out type);
					if (type == null)
					{
						type = LocationType.Prefabs.First<LocationType>();
					}
				}
				if (type != null)
				{
					A_3.element.SetAttributeValue("type", type.Identifier.ToString());
				}
				return false;
			}
			return true;
		}

		// Token: 0x06002915 RID: 10517 RVA: 0x0010B294 File Offset: 0x00109494
		[CompilerGenerated]
		private Faction <TryAssignFactionBasedOnLocationType>g__TryFindFaction|144_0(Identifier identifier, ref Location.<>c__DisplayClass144_0 A_2)
		{
			Faction faction = A_2.campaign.GetFaction(identifier);
			if (faction == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in location type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Type.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": failed to find a faction with the identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Type.ContentPackage, false, false);
			}
			return faction;
		}

		// Token: 0x06002916 RID: 10518 RVA: 0x0010B314 File Offset: 0x00109514
		[CompilerGenerated]
		internal static float <InstantiateMission>g__GetConnectionWeight|151_2(Location location, LocationConnection c)
		{
			Location destination = c.OtherLocation(location);
			if (destination == null)
			{
				return 0f;
			}
			float minWeight = 0.0001f;
			float lowWeight = 0.2f;
			float normalWeight = 1f;
			float maxWeight = 2f;
			float weight = c.Passed ? lowWeight : normalWeight;
			if (location.Biome.AllowedZones.Contains(1))
			{
				float diff = destination.MapPosition.X - location.MapPosition.X;
				if (diff < 0f)
				{
					weight *= 0.1f;
				}
				else
				{
					float maxRelevantDiff = 300f;
					weight = MathHelper.Lerp(weight, maxWeight, MathUtils.InverseLerp(0f, maxRelevantDiff, diff));
				}
			}
			else if (destination.MapPosition.X > location.MapPosition.X)
			{
				weight *= 2f;
			}
			int missionCount = location.availableMissions.Count((Mission m) => m.Locations.Contains(destination));
			if (missionCount > 0)
			{
				weight /= (float)(missionCount * 2);
			}
			if (destination.IsRadiated())
			{
				weight *= 0.001f;
			}
			return MathHelper.Clamp(weight, minWeight, maxWeight);
		}

		// Token: 0x040013FB RID: 5115
		public readonly List<LocationConnection> Connections = new List<LocationConnection>();

		// Token: 0x040013FD RID: 5117
		private int nameFormatIndex;

		// Token: 0x040013FE RID: 5118
		private Identifier nameIdentifier;

		// Token: 0x040013FF RID: 5119
		private string rawName;

		// Token: 0x04001400 RID: 5120
		private LocationType addInitialMissionsForType;

		// Token: 0x04001401 RID: 5121
		public const int ClearStoresDelay = 10;

		// Token: 0x04001402 RID: 5122
		public int WorldStepsSinceVisited;

		// Token: 0x04001403 RID: 5123
		public readonly Dictionary<LocationTypeChange.Requirement, int> ProximityTimer = new Dictionary<LocationTypeChange.Requirement, int>();

		// Token: 0x04001404 RID: 5124
		[TupleElementNames(new string[]
		{
			"typeChange",
			"delay",
			"parentMission"
		})]
		public ValueTuple<LocationTypeChange, int, MissionPrefab>? PendingLocationTypeChange;

		// Token: 0x04001405 RID: 5125
		public int LocationTypeChangeCooldown;

		// Token: 0x04001406 RID: 5126
		public bool DisallowLocationTypeChanges;

		// Token: 0x0400140F RID: 5135
		public Color? OverrideIconColor;

		// Token: 0x04001412 RID: 5138
		private const int SpecialsUpdateInterval = 3;

		// Token: 0x04001415 RID: 5141
		private const float MechanicalMaxDiscountPercentage = 50f;

		// Token: 0x04001416 RID: 5142
		private const float HealMaxDiscountPercentage = 10f;

		// Token: 0x04001417 RID: 5143
		private readonly List<Location.TakenItem> takenItems = new List<Location.TakenItem>();

		// Token: 0x04001418 RID: 5144
		private readonly HashSet<int> killedCharacterIdentifiers = new HashSet<int>();

		// Token: 0x04001419 RID: 5145
		private readonly List<Mission> availableMissions = new List<Mission>();

		// Token: 0x0400141A RID: 5146
		private readonly List<Mission> selectedMissions = new List<Mission>();

		// Token: 0x0400141B RID: 5147
		private float priceMultiplier = 1f;

		// Token: 0x0400141C RID: 5148
		private float mechanicalpriceMultiplier = 1f;

		// Token: 0x0400141D RID: 5149
		public string LastTypeChangeMessage;

		// Token: 0x0400141E RID: 5150
		public int TimeSinceLastTypeChange;

		// Token: 0x0400141F RID: 5151
		public bool IsGateBetweenBiomes;

		// Token: 0x04001420 RID: 5152
		private List<Location.LoadedMission> loadedMissions;

		// Token: 0x04001421 RID: 5153
		public HireManager HireManager;

		// Token: 0x02000A2F RID: 2607
		public class TakenItem
		{
			// Token: 0x06005C2D RID: 23597 RVA: 0x001FFDE2 File Offset: 0x001FDFE2
			public TakenItem(Identifier identifier, ushort originalID, int originalContainerIndex, ushort moduleIndex)
			{
				this.OriginalID = originalID;
				this.OriginalContainerIndex = originalContainerIndex;
				this.ModuleIndex = moduleIndex;
				this.Identifier = identifier;
			}

			// Token: 0x06005C2E RID: 23598 RVA: 0x001FFE07 File Offset: 0x001FE007
			public TakenItem(Item item)
			{
				this.OriginalContainerIndex = item.OriginalContainerIndex;
				this.OriginalID = item.ID;
				this.ModuleIndex = (ushort)item.OriginalModuleIndex;
				this.Identifier = item.Prefab.Identifier;
			}

			// Token: 0x06005C2F RID: 23599 RVA: 0x001FFE45 File Offset: 0x001FE045
			public bool IsEqual(Location.TakenItem obj)
			{
				return obj.OriginalID == this.OriginalID && obj.OriginalContainerIndex == this.OriginalContainerIndex && obj.ModuleIndex == this.ModuleIndex && obj.Identifier == this.Identifier;
			}

			// Token: 0x06005C30 RID: 23600 RVA: 0x001FFE84 File Offset: 0x001FE084
			public bool Matches(Item item)
			{
				if (item.OriginalContainerIndex != 0)
				{
					return item.OriginalContainerIndex == this.OriginalContainerIndex && item.OriginalModuleIndex == (int)this.ModuleIndex && item.Prefab.Identifier == this.Identifier;
				}
				return item.ID == this.OriginalID && item.OriginalModuleIndex == (int)this.ModuleIndex && item.Prefab.Identifier == this.Identifier;
			}

			// Token: 0x04003580 RID: 13696
			public readonly ushort OriginalID;

			// Token: 0x04003581 RID: 13697
			public readonly ushort ModuleIndex;

			// Token: 0x04003582 RID: 13698
			public readonly Identifier Identifier;

			// Token: 0x04003583 RID: 13699
			public readonly int OriginalContainerIndex;
		}

		// Token: 0x02000A30 RID: 2608
		public class StoreInfo
		{
			// Token: 0x1700157E RID: 5502
			// (get) Token: 0x06005C31 RID: 23601 RVA: 0x001FFF02 File Offset: 0x001FE102
			public Identifier Identifier { get; }

			// Token: 0x1700157F RID: 5503
			// (get) Token: 0x06005C32 RID: 23602 RVA: 0x001FFF0A File Offset: 0x001FE10A
			// (set) Token: 0x06005C33 RID: 23603 RVA: 0x001FFF12 File Offset: 0x001FE112
			public Identifier MerchantFaction { get; private set; }

			// Token: 0x17001580 RID: 5504
			// (get) Token: 0x06005C34 RID: 23604 RVA: 0x001FFF1B File Offset: 0x001FE11B
			// (set) Token: 0x06005C35 RID: 23605 RVA: 0x001FFF23 File Offset: 0x001FE123
			public int Balance { get; set; }

			// Token: 0x17001581 RID: 5505
			// (get) Token: 0x06005C36 RID: 23606 RVA: 0x001FFF2C File Offset: 0x001FE12C
			public List<PurchasedItem> Stock { get; } = new List<PurchasedItem>();

			// Token: 0x17001582 RID: 5506
			// (get) Token: 0x06005C37 RID: 23607 RVA: 0x001FFF34 File Offset: 0x001FE134
			public List<ItemPrefab> DailySpecials { get; } = new List<ItemPrefab>();

			// Token: 0x17001583 RID: 5507
			// (get) Token: 0x06005C38 RID: 23608 RVA: 0x001FFF3C File Offset: 0x001FE13C
			public List<ItemPrefab> RequestedGoods { get; } = new List<ItemPrefab>();

			// Token: 0x17001584 RID: 5508
			// (get) Token: 0x06005C39 RID: 23609 RVA: 0x001FFF44 File Offset: 0x001FE144
			// (set) Token: 0x06005C3A RID: 23610 RVA: 0x001FFF4C File Offset: 0x001FE14C
			public int PriceModifier { get; set; }

			// Token: 0x17001585 RID: 5509
			// (get) Token: 0x06005C3B RID: 23611 RVA: 0x001FFF55 File Offset: 0x001FE155
			public Location Location { get; }

			// Token: 0x17001586 RID: 5510
			// (get) Token: 0x06005C3C RID: 23612 RVA: 0x001FFF5D File Offset: 0x001FE15D
			private float MaxReputationModifier
			{
				get
				{
					return this.Location.StoreMaxReputationModifier;
				}
			}

			// Token: 0x17001587 RID: 5511
			// (get) Token: 0x06005C3D RID: 23613 RVA: 0x001FFF6A File Offset: 0x001FE16A
			private float MinReputationModifier
			{
				get
				{
					return this.Location.StoreMinReputationModifier;
				}
			}

			// Token: 0x06005C3E RID: 23614 RVA: 0x001FFF77 File Offset: 0x001FE177
			private StoreInfo(Location location)
			{
				this.Location = location;
			}

			// Token: 0x06005C3F RID: 23615 RVA: 0x001FFFA7 File Offset: 0x001FE1A7
			public StoreInfo(Location location, Identifier identifier) : this(location)
			{
				this.Identifier = identifier;
				this.Balance = location.StoreInitialBalance;
				this.Stock = this.CreateStock();
				this.GenerateSpecials();
				this.GeneratePriceModifier();
			}

			// Token: 0x06005C40 RID: 23616 RVA: 0x001FFFDC File Offset: 0x001FE1DC
			public StoreInfo(Location location, XElement storeElement) : this(location)
			{
				this.Identifier = storeElement.GetAttributeIdentifier("identifier", "");
				this.MerchantFaction = storeElement.GetAttributeIdentifier("MerchantFaction", "");
				this.Balance = storeElement.GetAttributeInt("balance", location.StoreInitialBalance);
				this.PriceModifier = storeElement.GetAttributeInt("pricemodifier", 0);
				if (storeElement.Attribute("stepssincespecialsupdated") != null)
				{
					location.StepsSinceSpecialsUpdated = storeElement.GetAttributeInt("stepssincespecialsupdated", 0);
				}
				foreach (XElement stockElement in storeElement.GetChildElements("stock", StringComparison.OrdinalIgnoreCase))
				{
					Identifier identifier = stockElement.GetAttributeIdentifier("id", Identifier.Empty);
					if (!identifier.IsEmpty)
					{
						ItemPrefab prefab = MapEntityPrefab.FindByIdentifier(identifier) as ItemPrefab;
						if (prefab != null)
						{
							int qty = stockElement.GetAttributeInt("qty", 0);
							if (qty >= 1)
							{
								this.Stock.Add(new PurchasedItem(prefab, qty, null));
							}
						}
					}
				}
				XElement specialsElement = storeElement.GetChildElement("dailyspecials", StringComparison.OrdinalIgnoreCase);
				if (specialsElement != null)
				{
					List<ItemPrefab> loadedDailySpecials = Location.StoreInfo.<.ctor>g__LoadStoreSpecials|33_0(specialsElement);
					this.DailySpecials.AddRange(loadedDailySpecials);
				}
				XElement goodsElement = storeElement.GetChildElement("requestedgoods", StringComparison.OrdinalIgnoreCase);
				if (goodsElement != null)
				{
					List<ItemPrefab> loadedRequestedGoods = Location.StoreInfo.<.ctor>g__LoadStoreSpecials|33_0(goodsElement);
					this.RequestedGoods.AddRange(loadedRequestedGoods);
				}
			}

			// Token: 0x06005C41 RID: 23617 RVA: 0x00200148 File Offset: 0x001FE348
			public static PurchasedItem CreateInitialStockItem(Location location, ItemPrefab itemPrefab, PriceInfo priceInfo)
			{
				int quantity = Rand.Range(priceInfo.MinAvailableAmount, priceInfo.MaxAvailableAmount + 1, Rand.RandSync.Unsynced);
				quantity = Math.Min(quantity + location.WorldStepsSinceVisited, priceInfo.MaxAvailableAmount);
				return new PurchasedItem(itemPrefab, quantity, null);
			}

			// Token: 0x06005C42 RID: 23618 RVA: 0x00200188 File Offset: 0x001FE388
			public List<PurchasedItem> CreateStock()
			{
				List<PurchasedItem> stock = new List<PurchasedItem>();
				foreach (ItemPrefab prefab in ItemPrefab.Prefabs)
				{
					PriceInfo priceInfo;
					if (prefab.CanBeBoughtFrom(this, out priceInfo))
					{
						stock.Add(Location.StoreInfo.CreateInitialStockItem(this.Location, prefab, priceInfo));
					}
				}
				return stock;
			}

			// Token: 0x06005C43 RID: 23619 RVA: 0x002001F4 File Offset: 0x001FE3F4
			public void AddStock(List<SoldItem> items)
			{
				if (items == null || items.None(null))
				{
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Adding items to stock for \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" at \"");
				defaultInterpolatedStringHandler.AppendFormatted<Location>(this.Location);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Purple), true);
				using (List<SoldItem>.Enumerator enumerator = items.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						SoldItem item = enumerator.Current;
						PurchasedItem stockItem = this.Stock.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == item.ItemPrefab);
						if (stockItem != null)
						{
							stockItem.Quantity++;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(22, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Added 1x ");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(item.ItemPrefab.Name);
							defaultInterpolatedStringHandler2.AppendLiteral(", new total: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(stockItem.Quantity);
							DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Cyan), true);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(32, 1);
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(item.ItemPrefab.Name);
							defaultInterpolatedStringHandler3.AppendLiteral(" not sold at location, can't add");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.Cyan), true);
						}
					}
				}
			}

			// Token: 0x06005C44 RID: 23620 RVA: 0x00200384 File Offset: 0x001FE584
			public void RemoveStock(List<PurchasedItem> items)
			{
				if (items == null || items.None(null))
				{
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Removing items from stock for \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" at \"");
				defaultInterpolatedStringHandler.AppendFormatted<Location>(this.Location);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Purple), true);
				using (List<PurchasedItem>.Enumerator enumerator = items.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PurchasedItem item = enumerator.Current;
						PurchasedItem stockItem = this.Stock.FirstOrDefault((PurchasedItem i) => i.ItemPrefab == item.ItemPrefab);
						if (stockItem != null)
						{
							stockItem.Quantity = Math.Max(stockItem.Quantity - item.Quantity, 0);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("Removed ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(item.Quantity);
							defaultInterpolatedStringHandler2.AppendLiteral("x ");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(item.ItemPrefab.Name);
							defaultInterpolatedStringHandler2.AppendLiteral(", new total: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(stockItem.Quantity);
							DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Cyan), true);
						}
					}
				}
			}

			// Token: 0x06005C45 RID: 23621 RVA: 0x00200500 File Offset: 0x001FE700
			public void GenerateSpecials()
			{
				Dictionary<ItemPrefab, float> availableStock = new Dictionary<ItemPrefab, float>();
				foreach (PurchasedItem stockItem in this.Stock)
				{
					if (stockItem.Quantity >= 1)
					{
						float weight = 1f;
						PriceInfo priceInfo = stockItem.ItemPrefab.GetPriceInfo(this);
						if (priceInfo != null)
						{
							if (!priceInfo.CanBeSpecial)
							{
								continue;
							}
							int baseQuantity = priceInfo.MinAvailableAmount;
							weight += (float)(stockItem.Quantity - baseQuantity) / (float)baseQuantity;
							if (weight < 0f)
							{
								continue;
							}
						}
						availableStock.Add(stockItem.ItemPrefab, weight);
					}
				}
				this.DailySpecials.Clear();
				int extraSpecialSalesCount = Location.GetExtraSpecialSalesCount();
				int i = 0;
				while (i < this.Location.DailySpecialsCount + extraSpecialSalesCount && !availableStock.None(null))
				{
					ItemPrefab item = ToolBox.SelectWeightedRandom<ItemPrefab>(availableStock.Keys.ToList<ItemPrefab>(), availableStock.Values.ToList<float>(), Rand.RandSync.Unsynced);
					if (item == null)
					{
						break;
					}
					this.DailySpecials.Add(item);
					availableStock.Remove(item);
					i++;
				}
				this.RequestedGoods.Clear();
				for (int j = 0; j < this.Location.RequestedGoodsCount; j++)
				{
					ItemPrefab selectedPrefab = ItemPrefab.Prefabs.GetRandom(delegate(ItemPrefab prefab)
					{
						if (prefab.CanBeSold && !this.RequestedGoods.Contains(prefab))
						{
							PriceInfo pi = prefab.GetPriceInfo(this);
							if (pi != null)
							{
								return pi.CanBeSpecial;
							}
						}
						return false;
					}, Rand.RandSync.Unsynced);
					if (selectedPrefab == null)
					{
						break;
					}
					this.RequestedGoods.Add(selectedPrefab);
				}
				this.Location.StepsSinceSpecialsUpdated = 0;
			}

			// Token: 0x06005C46 RID: 23622 RVA: 0x0020067C File Offset: 0x001FE87C
			public void GeneratePriceModifier()
			{
				this.PriceModifier = Rand.Range(-this.Location.StorePriceModifierRange, this.Location.StorePriceModifierRange + 1, Rand.RandSync.Unsynced);
			}

			// Token: 0x06005C47 RID: 23623 RVA: 0x002006A4 File Offset: 0x001FE8A4
			public int GetAdjustedItemBuyPrice(ItemPrefab item, PriceInfo priceInfo = null, bool considerDailySpecials = true)
			{
				if (priceInfo == null)
				{
					ItemPrefab item2 = item;
					priceInfo = ((item2 != null) ? item2.GetPriceInfo(this) : null);
				}
				if (priceInfo == null)
				{
					return 0;
				}
				float price = this.Location.StoreBuyPriceModifier * (float)priceInfo.Price;
				price = (float)(100 + this.PriceModifier) / 100f * price;
				price *= priceInfo.BuyingPriceMultiplier;
				if (considerDailySpecials && this.DailySpecials.Contains(item))
				{
					price = this.Location.DailySpecialPriceModifier * price;
				}
				if (this.RequestedGoods.Contains(item))
				{
					price = this.Location.RequestGoodBuyPriceModifier * price;
				}
				price *= this.GetReputationModifier(true);
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null)
				{
					price *= campaign.Settings.ShopPriceMultiplier;
				}
				ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
				if (characters.Any<Character>())
				{
					Identifier faction = this.GetMerchantOrLocationFactionIdentifier();
					if (!faction.IsEmpty && GameMain.GameSession.Campaign.GetFactionAffiliation(faction) == FactionAffiliation.Positive)
					{
						price *= 1f - characters.Max((Character c) => c.GetStatValue(StatTypes.StoreBuyMultiplierAffiliated, false));
						price *= 1f - characters.Max((Character c) => c.Info.GetSavedStatValue(StatTypes.StoreBuyMultiplierAffiliated, Tags.StatIdentifierTargetAll));
						price *= 1f - characters.Max((Character c) => Location.StoreInfo.<GetAdjustedItemBuyPrice>g__GetStatValuesForItem|40_0(c, item, StatTypes.StoreBuyMultiplierAffiliated));
					}
					price *= 1f - characters.Max((Character c) => c.GetStatValue(StatTypes.StoreBuyMultiplier, false));
					price *= 1f - characters.Max((Character c) => Location.StoreInfo.<GetAdjustedItemBuyPrice>g__GetStatValuesForItem|40_0(c, item, StatTypes.StoreBuyMultiplier));
				}
				return Math.Max((int)price, 1);
			}

			// Token: 0x06005C48 RID: 23624 RVA: 0x00200880 File Offset: 0x001FEA80
			public int GetAdjustedItemSellPrice(ItemPrefab item, PriceInfo priceInfo = null, bool considerRequestedGoods = true)
			{
				if (priceInfo == null)
				{
					ItemPrefab item2 = item;
					priceInfo = ((item2 != null) ? item2.GetPriceInfo(this) : null);
				}
				if (priceInfo == null)
				{
					return 0;
				}
				float price = this.Location.StoreSellPriceModifier * (float)priceInfo.Price;
				price = (float)(100 - this.PriceModifier) / 100f * price;
				if (considerRequestedGoods && this.RequestedGoods.Contains(item))
				{
					price = this.Location.RequestGoodSellPriceModifier * price;
				}
				price *= this.GetReputationModifier(false);
				ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
				if (characters.Any<Character>())
				{
					price *= 1f + characters.Max((Character c) => c.GetStatValue(StatTypes.StoreSellMultiplier, false));
					price *= 1f + characters.Max((Character c) => Location.StoreInfo.<GetAdjustedItemSellPrice>g__GetMultiplierForItem|41_2(c, item));
				}
				return Math.Max((int)price, 1);
			}

			// Token: 0x06005C49 RID: 23625 RVA: 0x0020096C File Offset: 0x001FEB6C
			public void SetMerchantFaction(Identifier factionIdentifier)
			{
				this.MerchantFaction = factionIdentifier;
			}

			// Token: 0x06005C4A RID: 23626 RVA: 0x00200978 File Offset: 0x001FEB78
			public Identifier GetMerchantOrLocationFactionIdentifier()
			{
				Identifier merchantFaction = this.MerchantFaction;
				Faction faction = this.Location.Faction;
				Identifier identifier = (faction != null) ? faction.Prefab.Identifier : Identifier.Empty;
				return merchantFaction.IfEmpty(identifier);
			}

			// Token: 0x06005C4B RID: 23627 RVA: 0x002009B8 File Offset: 0x001FEBB8
			public float GetReputationModifier(bool buying)
			{
				Identifier factionIdentifier = this.GetMerchantOrLocationFactionIdentifier();
				Faction faction = GameMain.GameSession.Campaign.GetFaction(factionIdentifier);
				Reputation reputation = (faction != null) ? faction.Reputation : null;
				if (reputation == null)
				{
					return 1f;
				}
				if (buying)
				{
					if (reputation.Value > 0f)
					{
						return MathHelper.Lerp(1f, 1f - this.MaxReputationModifier, reputation.Value / (float)reputation.MaxReputation);
					}
					return MathHelper.Lerp(1f, 1f + this.MinReputationModifier, reputation.Value / (float)reputation.MinReputation);
				}
				else
				{
					if (reputation.Value > 0f)
					{
						return MathHelper.Lerp(1f, 1f + this.MaxReputationModifier, reputation.Value / (float)reputation.MaxReputation);
					}
					return MathHelper.Lerp(1f, 1f - this.MinReputationModifier, reputation.Value / (float)reputation.MinReputation);
				}
			}

			// Token: 0x06005C4C RID: 23628 RVA: 0x00200AA4 File Offset: 0x001FECA4
			public override string ToString()
			{
				return this.Identifier.Value;
			}

			// Token: 0x06005C4D RID: 23629 RVA: 0x00200AC0 File Offset: 0x001FECC0
			[CompilerGenerated]
			internal static List<ItemPrefab> <.ctor>g__LoadStoreSpecials|33_0(XElement element)
			{
				List<ItemPrefab> specials = new List<ItemPrefab>();
				foreach (XElement childElement in element.GetChildElements("item", StringComparison.OrdinalIgnoreCase))
				{
					Identifier id = childElement.GetAttributeIdentifier("id", Identifier.Empty);
					if (!id.IsEmpty)
					{
						ItemPrefab prefab = MapEntityPrefab.FindByIdentifier(id) as ItemPrefab;
						if (prefab != null)
						{
							specials.Add(prefab);
						}
					}
				}
				return specials;
			}

			// Token: 0x06005C4F RID: 23631 RVA: 0x00200B80 File Offset: 0x001FED80
			[CompilerGenerated]
			internal static float <GetAdjustedItemBuyPrice>g__GetStatValuesForItem|40_0(Character character, ItemPrefab item, StatTypes statType)
			{
				float statValueSum = 0f;
				foreach (Identifier itemTag in item.Tags)
				{
					statValueSum += character.Info.GetSavedStatValue(statType, itemTag);
				}
				return statValueSum;
			}

			// Token: 0x06005C50 RID: 23632 RVA: 0x00200BE4 File Offset: 0x001FEDE4
			[CompilerGenerated]
			internal static float <GetAdjustedItemSellPrice>g__GetMultiplierForItem|41_2(Character character, ItemPrefab item)
			{
				return item.Tags.Sum((Identifier tag) => character.Info.GetSavedStatValue(StatTypes.StoreSellMultiplier, tag)) + character.Info.GetSavedStatValue(StatTypes.StoreSellMultiplier, Tags.StatIdentifierTargetAll);
			}
		}

		// Token: 0x02000A31 RID: 2609
		private readonly struct LoadedMission
		{
			// Token: 0x06005C51 RID: 23633 RVA: 0x00200C30 File Offset: 0x001FEE30
			public LoadedMission(XElement element)
			{
				Identifier id = element.GetAttributeIdentifier("prefabid", Identifier.Empty);
				MissionPrefab prefab;
				this.MissionPrefab = (MissionPrefab.Prefabs.TryGet(id, out prefab) ? prefab : null);
				this.TimesAttempted = element.GetAttributeInt("timesattempted", 0);
				this.OriginLocationIndex = element.GetAttributeInt("origin", -1);
				this.DestinationIndex = element.GetAttributeInt("destinationindex", -1);
				this.SelectedMission = element.GetAttributeBool("selected", false);
			}

			// Token: 0x0400358C RID: 13708
			public readonly MissionPrefab MissionPrefab;

			// Token: 0x0400358D RID: 13709
			public readonly int TimesAttempted;

			// Token: 0x0400358E RID: 13710
			public readonly int OriginLocationIndex;

			// Token: 0x0400358F RID: 13711
			public readonly int DestinationIndex;

			// Token: 0x04003590 RID: 13712
			public readonly bool SelectedMission;
		}

		// Token: 0x02000A32 RID: 2610
		public class AbilityLocation : AbilityObject, IAbilityLocation
		{
			// Token: 0x06005C52 RID: 23634 RVA: 0x00200CAF File Offset: 0x001FEEAF
			public AbilityLocation(Location location)
			{
				this.Location = location;
			}

			// Token: 0x17001588 RID: 5512
			// (get) Token: 0x06005C53 RID: 23635 RVA: 0x00200CBE File Offset: 0x001FEEBE
			// (set) Token: 0x06005C54 RID: 23636 RVA: 0x00200CC6 File Offset: 0x001FEEC6
			public Location Location { get; set; }
		}
	}
}
