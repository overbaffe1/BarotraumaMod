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
	// Token: 0x0200031E RID: 798
	internal class Location
	{
		// Token: 0x170010A8 RID: 4264
		// (get) Token: 0x06003F59 RID: 16217 RVA: 0x00237042 File Offset: 0x00235242
		// (set) Token: 0x06003F5A RID: 16218 RVA: 0x0023704A File Offset: 0x0023524A
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x170010A9 RID: 4265
		// (get) Token: 0x06003F5B RID: 16219 RVA: 0x00237053 File Offset: 0x00235253
		public Identifier NameIdentifier
		{
			get
			{
				return this.nameIdentifier;
			}
		}

		// Token: 0x170010AA RID: 4266
		// (get) Token: 0x06003F5C RID: 16220 RVA: 0x0023705B File Offset: 0x0023525B
		public int NameFormatIndex
		{
			get
			{
				return this.nameFormatIndex;
			}
		}

		// Token: 0x170010AB RID: 4267
		// (get) Token: 0x06003F5D RID: 16221 RVA: 0x00237064 File Offset: 0x00235264
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

		// Token: 0x170010AC RID: 4268
		// (get) Token: 0x06003F5E RID: 16222 RVA: 0x002370AC File Offset: 0x002352AC
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

		// Token: 0x170010AD RID: 4269
		// (get) Token: 0x06003F5F RID: 16223 RVA: 0x002370F4 File Offset: 0x002352F4
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

		// Token: 0x170010AE RID: 4270
		// (get) Token: 0x06003F60 RID: 16224 RVA: 0x0023712A File Offset: 0x0023532A
		// (set) Token: 0x06003F61 RID: 16225 RVA: 0x00237132 File Offset: 0x00235332
		public Biome Biome { get; set; }

		// Token: 0x170010AF RID: 4271
		// (get) Token: 0x06003F62 RID: 16226 RVA: 0x0023713B File Offset: 0x0023533B
		// (set) Token: 0x06003F63 RID: 16227 RVA: 0x00237143 File Offset: 0x00235343
		public Vector2 MapPosition { get; private set; }

		// Token: 0x170010B0 RID: 4272
		// (get) Token: 0x06003F64 RID: 16228 RVA: 0x0023714C File Offset: 0x0023534C
		// (set) Token: 0x06003F65 RID: 16229 RVA: 0x00237154 File Offset: 0x00235354
		public LocationType Type { get; private set; }

		// Token: 0x170010B1 RID: 4273
		// (get) Token: 0x06003F66 RID: 16230 RVA: 0x0023715D File Offset: 0x0023535D
		// (set) Token: 0x06003F67 RID: 16231 RVA: 0x00237165 File Offset: 0x00235365
		public LocationType OriginalType { get; private set; }

		// Token: 0x170010B2 RID: 4274
		// (get) Token: 0x06003F68 RID: 16232 RVA: 0x0023716E File Offset: 0x0023536E
		// (set) Token: 0x06003F69 RID: 16233 RVA: 0x00237176 File Offset: 0x00235376
		public LevelData LevelData { get; set; }

		// Token: 0x170010B3 RID: 4275
		// (get) Token: 0x06003F6A RID: 16234 RVA: 0x0023717F File Offset: 0x0023537F
		// (set) Token: 0x06003F6B RID: 16235 RVA: 0x00237187 File Offset: 0x00235387
		public int PortraitId { get; private set; }

		// Token: 0x170010B4 RID: 4276
		// (get) Token: 0x06003F6C RID: 16236 RVA: 0x00237190 File Offset: 0x00235390
		// (set) Token: 0x06003F6D RID: 16237 RVA: 0x00237198 File Offset: 0x00235398
		public Faction Faction { get; set; }

		// Token: 0x170010B5 RID: 4277
		// (get) Token: 0x06003F6E RID: 16238 RVA: 0x002371A1 File Offset: 0x002353A1
		// (set) Token: 0x06003F6F RID: 16239 RVA: 0x002371A9 File Offset: 0x002353A9
		public Faction SecondaryFaction { get; set; }

		// Token: 0x170010B6 RID: 4278
		// (get) Token: 0x06003F70 RID: 16240 RVA: 0x002371B2 File Offset: 0x002353B2
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

		// Token: 0x170010B7 RID: 4279
		// (get) Token: 0x06003F71 RID: 16241 RVA: 0x002371C5 File Offset: 0x002353C5
		public bool IsFactionHostile
		{
			get
			{
				Faction faction = this.Faction;
				return faction != null && faction.Reputation.NormalizedValue < 0.2f;
			}
		}

		// Token: 0x170010B8 RID: 4280
		// (get) Token: 0x06003F72 RID: 16242 RVA: 0x002371E4 File Offset: 0x002353E4
		// (set) Token: 0x06003F73 RID: 16243 RVA: 0x002371EC File Offset: 0x002353EC
		public int TurnsInRadiation { get; set; }

		// Token: 0x170010B9 RID: 4281
		// (get) Token: 0x06003F74 RID: 16244 RVA: 0x002371F5 File Offset: 0x002353F5
		// (set) Token: 0x06003F75 RID: 16245 RVA: 0x002371FD File Offset: 0x002353FD
		public Dictionary<Identifier, Location.StoreInfo> Stores { get; private set; }

		// Token: 0x170010BA RID: 4282
		// (get) Token: 0x06003F76 RID: 16246 RVA: 0x00237206 File Offset: 0x00235406
		private float StoreMaxReputationModifier
		{
			get
			{
				return this.Type.StoreMaxReputationModifier;
			}
		}

		// Token: 0x170010BB RID: 4283
		// (get) Token: 0x06003F77 RID: 16247 RVA: 0x00237213 File Offset: 0x00235413
		private float StoreMinReputationModifier
		{
			get
			{
				return this.Type.StoreMinReputationModifier;
			}
		}

		// Token: 0x170010BC RID: 4284
		// (get) Token: 0x06003F78 RID: 16248 RVA: 0x00237220 File Offset: 0x00235420
		private float StoreSellPriceModifier
		{
			get
			{
				return this.Type.StoreSellPriceModifier;
			}
		}

		// Token: 0x170010BD RID: 4285
		// (get) Token: 0x06003F79 RID: 16249 RVA: 0x0023722D File Offset: 0x0023542D
		private float StoreBuyPriceModifier
		{
			get
			{
				return this.Type.StoreBuyPriceModifier;
			}
		}

		// Token: 0x170010BE RID: 4286
		// (get) Token: 0x06003F7A RID: 16250 RVA: 0x0023723A File Offset: 0x0023543A
		private float DailySpecialPriceModifier
		{
			get
			{
				return this.Type.DailySpecialPriceModifier;
			}
		}

		// Token: 0x170010BF RID: 4287
		// (get) Token: 0x06003F7B RID: 16251 RVA: 0x00237247 File Offset: 0x00235447
		private float RequestGoodBuyPriceModifier
		{
			get
			{
				return this.Type.RequestGoodBuyPriceModifier;
			}
		}

		// Token: 0x170010C0 RID: 4288
		// (get) Token: 0x06003F7C RID: 16252 RVA: 0x00237254 File Offset: 0x00235454
		private float RequestGoodSellPriceModifier
		{
			get
			{
				return this.Type.RequestGoodPriceModifier;
			}
		}

		// Token: 0x170010C1 RID: 4289
		// (get) Token: 0x06003F7D RID: 16253 RVA: 0x00237261 File Offset: 0x00235461
		public int StoreInitialBalance
		{
			get
			{
				return this.Type.StoreInitialBalance;
			}
		}

		// Token: 0x170010C2 RID: 4290
		// (get) Token: 0x06003F7E RID: 16254 RVA: 0x0023726E File Offset: 0x0023546E
		private int StorePriceModifierRange
		{
			get
			{
				return this.Type.StorePriceModifierRange;
			}
		}

		// Token: 0x170010C3 RID: 4291
		// (get) Token: 0x06003F7F RID: 16255 RVA: 0x0023727B File Offset: 0x0023547B
		public int DailySpecialsCount
		{
			get
			{
				return this.Type.DailySpecialsCount;
			}
		}

		// Token: 0x170010C4 RID: 4292
		// (get) Token: 0x06003F80 RID: 16256 RVA: 0x00237288 File Offset: 0x00235488
		public int RequestedGoodsCount
		{
			get
			{
				return this.Type.RequestedGoodsCount;
			}
		}

		// Token: 0x170010C5 RID: 4293
		// (get) Token: 0x06003F81 RID: 16257 RVA: 0x00237295 File Offset: 0x00235495
		// (set) Token: 0x06003F82 RID: 16258 RVA: 0x0023729D File Offset: 0x0023549D
		private int StepsSinceSpecialsUpdated { get; set; }

		// Token: 0x170010C6 RID: 4294
		// (get) Token: 0x06003F83 RID: 16259 RVA: 0x002372A6 File Offset: 0x002354A6
		public HashSet<Identifier> StoreIdentifiers { get; } = new HashSet<Identifier>();

		// Token: 0x170010C7 RID: 4295
		// (get) Token: 0x06003F84 RID: 16260 RVA: 0x002372AE File Offset: 0x002354AE
		public IEnumerable<Location.TakenItem> TakenItems
		{
			get
			{
				return this.takenItems;
			}
		}

		// Token: 0x170010C8 RID: 4296
		// (get) Token: 0x06003F85 RID: 16261 RVA: 0x002372B6 File Offset: 0x002354B6
		public IEnumerable<int> KilledCharacterIdentifiers
		{
			get
			{
				return this.killedCharacterIdentifiers;
			}
		}

		// Token: 0x170010C9 RID: 4297
		// (get) Token: 0x06003F86 RID: 16262 RVA: 0x002372BE File Offset: 0x002354BE
		public IEnumerable<Mission> AvailableMissions
		{
			get
			{
				this.availableMissions.RemoveAll((Mission m) => m.Completed || (m.Failed && !m.Prefab.AllowRetry) || m.ForceFailure);
				return this.availableMissions;
			}
		}

		// Token: 0x170010CA RID: 4298
		// (get) Token: 0x06003F87 RID: 16263 RVA: 0x002372F1 File Offset: 0x002354F1
		public IEnumerable<Mission> AvailableAndVisibleMissions
		{
			get
			{
				return from m in this.AvailableMissions
				where m.Prefab.ShowInMenus
				select m;
			}
		}

		// Token: 0x170010CB RID: 4299
		// (get) Token: 0x06003F88 RID: 16264 RVA: 0x0023731D File Offset: 0x0023551D
		public IEnumerable<Mission> SelectedMissions
		{
			get
			{
				this.selectedMissions.RemoveAll((Mission m) => !this.availableMissions.Contains(m));
				return this.selectedMissions;
			}
		}

		// Token: 0x06003F89 RID: 16265 RVA: 0x0023733D File Offset: 0x0023553D
		public void SelectMission(Mission mission)
		{
			if (!this.SelectedMissions.Contains(mission) && mission != null)
			{
				this.selectedMissions.Add(mission);
				this.selectedMissions.Sort((Mission m1, Mission m2) => this.availableMissions.IndexOf(m1).CompareTo(this.availableMissions.IndexOf(m2)));
			}
		}

		// Token: 0x06003F8A RID: 16266 RVA: 0x00237373 File Offset: 0x00235573
		public void DeselectMission(Mission mission)
		{
			this.selectedMissions.Remove(mission);
		}

		// Token: 0x06003F8B RID: 16267 RVA: 0x00237384 File Offset: 0x00235584
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

		// Token: 0x06003F8C RID: 16268 RVA: 0x002373F4 File Offset: 0x002355F4
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

		// Token: 0x170010CC RID: 4300
		// (get) Token: 0x06003F8D RID: 16269 RVA: 0x002374E4 File Offset: 0x002356E4
		// (set) Token: 0x06003F8E RID: 16270 RVA: 0x002374EC File Offset: 0x002356EC
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

		// Token: 0x170010CD RID: 4301
		// (get) Token: 0x06003F8F RID: 16271 RVA: 0x00237504 File Offset: 0x00235704
		// (set) Token: 0x06003F90 RID: 16272 RVA: 0x0023750C File Offset: 0x0023570C
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

		// Token: 0x06003F91 RID: 16273 RVA: 0x00237524 File Offset: 0x00235724
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Location (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName ?? "null");
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003F92 RID: 16274 RVA: 0x00237578 File Offset: 0x00235778
		public Location(Vector2 mapPosition, int? zone, Identifier? biomeId, Random rand, bool requireOutpost = false, LocationType forceLocationType = null, IEnumerable<Location> existingLocations = null)
		{
			this.Type = (this.OriginalType = (forceLocationType ?? LocationType.Random(rand, zone, biomeId, requireOutpost, null)));
			this.AssignRandomName(this.Type, rand, existingLocations);
			this.MapPosition = mapPosition;
			this.PortraitId = ToolBox.StringToInt(this.nameIdentifier.Value);
			this.Connections = new List<LocationConnection>();
		}

		// Token: 0x06003F93 RID: 16275 RVA: 0x00237648 File Offset: 0x00235848
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

		// Token: 0x06003F94 RID: 16276 RVA: 0x00237B00 File Offset: 0x00235D00
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

		// Token: 0x06003F95 RID: 16277 RVA: 0x00237C00 File Offset: 0x00235E00
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

		// Token: 0x06003F96 RID: 16278 RVA: 0x00237DF4 File Offset: 0x00235FF4
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

		// Token: 0x06003F97 RID: 16279 RVA: 0x00237E78 File Offset: 0x00236078
		public static Location CreateRandom(Vector2 position, int? zone, Identifier? biomeId, Random rand, bool requireOutpost, LocationType forceLocationType = null, IEnumerable<Location> existingLocations = null)
		{
			return new Location(position, zone, biomeId, rand, requireOutpost, forceLocationType, existingLocations);
		}

		// Token: 0x06003F98 RID: 16280 RVA: 0x00237E8C File Offset: 0x0023608C
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

		// Token: 0x06003F99 RID: 16281 RVA: 0x0023819C File Offset: 0x0023639C
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

		// Token: 0x06003F9A RID: 16282 RVA: 0x00238264 File Offset: 0x00236464
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

		// Token: 0x06003F9B RID: 16283 RVA: 0x002382E8 File Offset: 0x002364E8
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

		// Token: 0x06003F9C RID: 16284 RVA: 0x0023835C File Offset: 0x0023655C
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

		// Token: 0x06003F9D RID: 16285 RVA: 0x002383D0 File Offset: 0x002365D0
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

		// Token: 0x06003F9E RID: 16286 RVA: 0x0023853C File Offset: 0x0023673C
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

		// Token: 0x06003F9F RID: 16287 RVA: 0x0023883C File Offset: 0x00236A3C
		private void AddMission(Mission mission)
		{
			if (!mission.Prefab.AllowOtherMissionsInLevel)
			{
				this.availableMissions.Clear();
			}
			this.availableMissions.Add(mission);
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			CampaignMode campaign = gameSession.Campaign;
			if (campaign == null)
			{
				return;
			}
			CampaignUI campaignUI = campaign.CampaignUI;
			if (campaignUI == null)
			{
				return;
			}
			campaignUI.RefreshLocationInfo();
		}

		// Token: 0x06003FA0 RID: 16288 RVA: 0x00238890 File Offset: 0x00236A90
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

		// Token: 0x06003FA1 RID: 16289 RVA: 0x00238930 File Offset: 0x00236B30
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

		// Token: 0x06003FA2 RID: 16290 RVA: 0x0023896C File Offset: 0x00236B6C
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

		// Token: 0x06003FA3 RID: 16291 RVA: 0x002389A0 File Offset: 0x00236BA0
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

		// Token: 0x06003FA4 RID: 16292 RVA: 0x00238BD8 File Offset: 0x00236DD8
		public void ClearMissions()
		{
			this.availableMissions.Clear();
			this.selectedMissions.Clear();
		}

		// Token: 0x06003FA5 RID: 16293 RVA: 0x00238BF0 File Offset: 0x00236DF0
		public bool HasOutpost()
		{
			return this.Type.HasOutpost && !this.IsCriticallyRadiated();
		}

		// Token: 0x06003FA6 RID: 16294 RVA: 0x00238C0C File Offset: 0x00236E0C
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

		// Token: 0x06003FA7 RID: 16295 RVA: 0x00238C5C File Offset: 0x00236E5C
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

		// Token: 0x06003FA8 RID: 16296 RVA: 0x00238D0C File Offset: 0x00236F0C
		public LocationType GetLocationTypeToDisplay()
		{
			Identifier identifier;
			return this.GetLocationTypeToDisplay(out identifier);
		}

		// Token: 0x06003FA9 RID: 16297 RVA: 0x00238D24 File Offset: 0x00236F24
		public IEnumerable<Mission> GetMissionsInConnection(LocationConnection connection)
		{
			return from m in this.AvailableMissions
			where m.Locations[1] == connection.OtherLocation(this)
			select m;
		}

		// Token: 0x06003FAA RID: 16298 RVA: 0x00238D5C File Offset: 0x00236F5C
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

		// Token: 0x06003FAB RID: 16299 RVA: 0x00238E10 File Offset: 0x00237010
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

		// Token: 0x06003FAC RID: 16300 RVA: 0x00238E6D File Offset: 0x0023706D
		public void ForceHireableCharacters(IEnumerable<CharacterInfo> hireableCharacters)
		{
			if (this.HireManager == null)
			{
				this.HireManager = new HireManager();
			}
			this.HireManager.AvailableCharacters = hireableCharacters.ToList<CharacterInfo>();
		}

		// Token: 0x06003FAD RID: 16301 RVA: 0x00238E94 File Offset: 0x00237094
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

		// Token: 0x06003FAE RID: 16302 RVA: 0x00239054 File Offset: 0x00237254
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

		// Token: 0x06003FAF RID: 16303 RVA: 0x002390D0 File Offset: 0x002372D0
		public static LocalizedString GetName(LocationType type, int nameFormatIndex, Identifier nameId)
		{
			if (((type != null) ? type.NameFormats : null) == null || !type.NameFormats.Any<string>() || nameFormatIndex < 0)
			{
				return TextManager.Get(nameId).Fallback(nameId.Value, true);
			}
			return type.NameFormats[nameFormatIndex % type.NameFormats.Count].Replace("[name]", TextManager.Get(nameId).Value);
		}

		// Token: 0x06003FB0 RID: 16304 RVA: 0x00239147 File Offset: 0x00237347
		public void ForceName(Identifier nameId)
		{
			this.rawName = string.Empty;
			this.nameIdentifier = nameId;
			this.DisplayName = TextManager.Get(nameId).Fallback(nameId.Value, true);
		}

		// Token: 0x06003FB1 RID: 16305 RVA: 0x0023917C File Offset: 0x0023737C
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

		// Token: 0x06003FB2 RID: 16306 RVA: 0x002393BC File Offset: 0x002375BC
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

		// Token: 0x06003FB3 RID: 16307 RVA: 0x0023941C File Offset: 0x0023761C
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

		// Token: 0x06003FB4 RID: 16308 RVA: 0x002394C4 File Offset: 0x002376C4
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

		// Token: 0x06003FB5 RID: 16309 RVA: 0x0023952C File Offset: 0x0023772C
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

		// Token: 0x06003FB6 RID: 16310 RVA: 0x002395A4 File Offset: 0x002377A4
		public int GetAdjustedMechanicalCost(int cost)
		{
			float discount = 0f;
			if (this.Reputation != null)
			{
				discount = this.Reputation.Value / (float)this.Reputation.MaxReputation * 0.5f;
			}
			return (int)Math.Ceiling((double)((1f - discount) * (float)cost * this.MechanicalPriceMultiplier));
		}

		// Token: 0x06003FB7 RID: 16311 RVA: 0x002395F8 File Offset: 0x002377F8
		public int GetAdjustedHealCost(int cost)
		{
			float discount = 0f;
			if (this.Reputation != null)
			{
				discount = this.Reputation.Value / (float)this.Reputation.MaxReputation * 0.1f;
			}
			return (int)Math.Ceiling((double)((1f - discount) * (float)cost * this.PriceMultiplier));
		}

		// Token: 0x06003FB8 RID: 16312 RVA: 0x0023964C File Offset: 0x0023784C
		public Location.StoreInfo GetStore(Identifier identifier)
		{
			Location.StoreInfo store;
			if (this.Stores != null && this.Stores.TryGetValue(identifier, out store))
			{
				return store;
			}
			return null;
		}

		// Token: 0x06003FB9 RID: 16313 RVA: 0x00239674 File Offset: 0x00237874
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

		// Token: 0x06003FBA RID: 16314 RVA: 0x002398BC File Offset: 0x00237ABC
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

		// Token: 0x06003FBB RID: 16315 RVA: 0x00239B80 File Offset: 0x00237D80
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

		// Token: 0x06003FBC RID: 16316 RVA: 0x00239C18 File Offset: 0x00237E18
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

		// Token: 0x06003FBD RID: 16317 RVA: 0x00239CC8 File Offset: 0x00237EC8
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

		// Token: 0x06003FBE RID: 16318 RVA: 0x00239D10 File Offset: 0x00237F10
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

		// Token: 0x06003FBF RID: 16319 RVA: 0x00239D7C File Offset: 0x00237F7C
		public void ClearStores()
		{
			this.Stores = null;
		}

		// Token: 0x06003FC0 RID: 16320 RVA: 0x00239D88 File Offset: 0x00237F88
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

		// Token: 0x06003FC1 RID: 16321 RVA: 0x00239DF4 File Offset: 0x00237FF4
		public static int GetExtraSpecialSalesCount()
		{
			ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			if (!characters.Any<Character>())
			{
				return 0;
			}
			return characters.Max((Character c) => (int)c.GetStatValue(StatTypes.ExtraSpecialSalesCount, true));
		}

		// Token: 0x06003FC2 RID: 16322 RVA: 0x00239E37 File Offset: 0x00238037
		public bool CanHaveSubsForSale()
		{
			return this.HasOutpost() && this.CanHaveCampaignInteraction(CampaignMode.InteractionType.PurchaseSub);
		}

		// Token: 0x06003FC3 RID: 16323 RVA: 0x00239E4A File Offset: 0x0023804A
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

		// Token: 0x06003FC4 RID: 16324 RVA: 0x00239E73 File Offset: 0x00238073
		public bool IsSubmarineAvailable(SubmarineInfo info)
		{
			Biome biome = this.Biome;
			return biome == null || biome.IsSubmarineAvailable(info, this.Type.Identifier);
		}

		// Token: 0x06003FC5 RID: 16325 RVA: 0x00239E94 File Offset: 0x00238094
		private bool CanHaveCampaignInteraction(CampaignMode.InteractionType interactionType)
		{
			return this.LevelData != null && this.LevelData.OutpostGenerationParamsExist && LevelData.GetSuitableOutpostGenerationParams(this, this.LevelData).Any((OutpostGenerationParams p) => p.CanHaveCampaignInteraction(interactionType));
		}

		// Token: 0x06003FC6 RID: 16326 RVA: 0x00239EE4 File Offset: 0x002380E4
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

		// Token: 0x06003FC7 RID: 16327 RVA: 0x00239F54 File Offset: 0x00238154
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

		// Token: 0x06003FC8 RID: 16328 RVA: 0x0023A92C File Offset: 0x00238B2C
		public void Remove()
		{
			this.RemoveProjSpecific();
		}

		// Token: 0x06003FC9 RID: 16329 RVA: 0x0023A934 File Offset: 0x00238B34
		public void RemoveProjSpecific()
		{
			HireManager hireManager = this.HireManager;
			if (hireManager == null)
			{
				return;
			}
			hireManager.Remove();
		}

		// Token: 0x06003FCC RID: 16332 RVA: 0x0023A988 File Offset: 0x00238B88
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

		// Token: 0x06003FCD RID: 16333 RVA: 0x0023AA68 File Offset: 0x00238C68
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

		// Token: 0x06003FCE RID: 16334 RVA: 0x0023AAE8 File Offset: 0x00238CE8
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

		// Token: 0x040020F6 RID: 8438
		public readonly List<LocationConnection> Connections = new List<LocationConnection>();

		// Token: 0x040020F8 RID: 8440
		private int nameFormatIndex;

		// Token: 0x040020F9 RID: 8441
		private Identifier nameIdentifier;

		// Token: 0x040020FA RID: 8442
		private string rawName;

		// Token: 0x040020FB RID: 8443
		private LocationType addInitialMissionsForType;

		// Token: 0x040020FC RID: 8444
		public const int ClearStoresDelay = 10;

		// Token: 0x040020FD RID: 8445
		public int WorldStepsSinceVisited;

		// Token: 0x040020FE RID: 8446
		public readonly Dictionary<LocationTypeChange.Requirement, int> ProximityTimer = new Dictionary<LocationTypeChange.Requirement, int>();

		// Token: 0x040020FF RID: 8447
		[TupleElementNames(new string[]
		{
			"typeChange",
			"delay",
			"parentMission"
		})]
		public ValueTuple<LocationTypeChange, int, MissionPrefab>? PendingLocationTypeChange;

		// Token: 0x04002100 RID: 8448
		public int LocationTypeChangeCooldown;

		// Token: 0x04002101 RID: 8449
		public bool DisallowLocationTypeChanges;

		// Token: 0x0400210A RID: 8458
		public Color? OverrideIconColor;

		// Token: 0x0400210D RID: 8461
		private const int SpecialsUpdateInterval = 3;

		// Token: 0x04002110 RID: 8464
		private const float MechanicalMaxDiscountPercentage = 50f;

		// Token: 0x04002111 RID: 8465
		private const float HealMaxDiscountPercentage = 10f;

		// Token: 0x04002112 RID: 8466
		private readonly List<Location.TakenItem> takenItems = new List<Location.TakenItem>();

		// Token: 0x04002113 RID: 8467
		private readonly HashSet<int> killedCharacterIdentifiers = new HashSet<int>();

		// Token: 0x04002114 RID: 8468
		private readonly List<Mission> availableMissions = new List<Mission>();

		// Token: 0x04002115 RID: 8469
		private readonly List<Mission> selectedMissions = new List<Mission>();

		// Token: 0x04002116 RID: 8470
		private float priceMultiplier = 1f;

		// Token: 0x04002117 RID: 8471
		private float mechanicalpriceMultiplier = 1f;

		// Token: 0x04002118 RID: 8472
		public string LastTypeChangeMessage;

		// Token: 0x04002119 RID: 8473
		public int TimeSinceLastTypeChange;

		// Token: 0x0400211A RID: 8474
		public bool IsGateBetweenBiomes;

		// Token: 0x0400211B RID: 8475
		private List<Location.LoadedMission> loadedMissions;

		// Token: 0x0400211C RID: 8476
		public HireManager HireManager;

		// Token: 0x02000FEB RID: 4075
		public class TakenItem
		{
			// Token: 0x06008A9E RID: 35486 RVA: 0x003AAC30 File Offset: 0x003A8E30
			public TakenItem(Identifier identifier, ushort originalID, int originalContainerIndex, ushort moduleIndex)
			{
				this.OriginalID = originalID;
				this.OriginalContainerIndex = originalContainerIndex;
				this.ModuleIndex = moduleIndex;
				this.Identifier = identifier;
			}

			// Token: 0x06008A9F RID: 35487 RVA: 0x003AAC55 File Offset: 0x003A8E55
			public TakenItem(Item item)
			{
				this.OriginalContainerIndex = item.OriginalContainerIndex;
				this.OriginalID = item.ID;
				this.ModuleIndex = (ushort)item.OriginalModuleIndex;
				this.Identifier = item.Prefab.Identifier;
			}

			// Token: 0x06008AA0 RID: 35488 RVA: 0x003AAC93 File Offset: 0x003A8E93
			public bool IsEqual(Location.TakenItem obj)
			{
				return obj.OriginalID == this.OriginalID && obj.OriginalContainerIndex == this.OriginalContainerIndex && obj.ModuleIndex == this.ModuleIndex && obj.Identifier == this.Identifier;
			}

			// Token: 0x06008AA1 RID: 35489 RVA: 0x003AACD4 File Offset: 0x003A8ED4
			public bool Matches(Item item)
			{
				if (item.OriginalContainerIndex != 0)
				{
					return item.OriginalContainerIndex == this.OriginalContainerIndex && item.OriginalModuleIndex == (int)this.ModuleIndex && item.Prefab.Identifier == this.Identifier;
				}
				return item.ID == this.OriginalID && item.OriginalModuleIndex == (int)this.ModuleIndex && item.Prefab.Identifier == this.Identifier;
			}

			// Token: 0x040056ED RID: 22253
			public readonly ushort OriginalID;

			// Token: 0x040056EE RID: 22254
			public readonly ushort ModuleIndex;

			// Token: 0x040056EF RID: 22255
			public readonly Identifier Identifier;

			// Token: 0x040056F0 RID: 22256
			public readonly int OriginalContainerIndex;
		}

		// Token: 0x02000FEC RID: 4076
		public class StoreInfo
		{
			// Token: 0x17001C3F RID: 7231
			// (get) Token: 0x06008AA2 RID: 35490 RVA: 0x003AAD52 File Offset: 0x003A8F52
			public Identifier Identifier { get; }

			// Token: 0x17001C40 RID: 7232
			// (get) Token: 0x06008AA3 RID: 35491 RVA: 0x003AAD5A File Offset: 0x003A8F5A
			// (set) Token: 0x06008AA4 RID: 35492 RVA: 0x003AAD62 File Offset: 0x003A8F62
			public Identifier MerchantFaction { get; private set; }

			// Token: 0x17001C41 RID: 7233
			// (get) Token: 0x06008AA5 RID: 35493 RVA: 0x003AAD6B File Offset: 0x003A8F6B
			// (set) Token: 0x06008AA6 RID: 35494 RVA: 0x003AAD73 File Offset: 0x003A8F73
			public int Balance { get; set; }

			// Token: 0x17001C42 RID: 7234
			// (get) Token: 0x06008AA7 RID: 35495 RVA: 0x003AAD7C File Offset: 0x003A8F7C
			public List<PurchasedItem> Stock { get; } = new List<PurchasedItem>();

			// Token: 0x17001C43 RID: 7235
			// (get) Token: 0x06008AA8 RID: 35496 RVA: 0x003AAD84 File Offset: 0x003A8F84
			public List<ItemPrefab> DailySpecials { get; } = new List<ItemPrefab>();

			// Token: 0x17001C44 RID: 7236
			// (get) Token: 0x06008AA9 RID: 35497 RVA: 0x003AAD8C File Offset: 0x003A8F8C
			public List<ItemPrefab> RequestedGoods { get; } = new List<ItemPrefab>();

			// Token: 0x17001C45 RID: 7237
			// (get) Token: 0x06008AAA RID: 35498 RVA: 0x003AAD94 File Offset: 0x003A8F94
			// (set) Token: 0x06008AAB RID: 35499 RVA: 0x003AAD9C File Offset: 0x003A8F9C
			public int PriceModifier { get; set; }

			// Token: 0x17001C46 RID: 7238
			// (get) Token: 0x06008AAC RID: 35500 RVA: 0x003AADA5 File Offset: 0x003A8FA5
			public Location Location { get; }

			// Token: 0x17001C47 RID: 7239
			// (get) Token: 0x06008AAD RID: 35501 RVA: 0x003AADAD File Offset: 0x003A8FAD
			private float MaxReputationModifier
			{
				get
				{
					return this.Location.StoreMaxReputationModifier;
				}
			}

			// Token: 0x17001C48 RID: 7240
			// (get) Token: 0x06008AAE RID: 35502 RVA: 0x003AADBA File Offset: 0x003A8FBA
			private float MinReputationModifier
			{
				get
				{
					return this.Location.StoreMinReputationModifier;
				}
			}

			// Token: 0x06008AAF RID: 35503 RVA: 0x003AADC7 File Offset: 0x003A8FC7
			private StoreInfo(Location location)
			{
				this.Location = location;
			}

			// Token: 0x06008AB0 RID: 35504 RVA: 0x003AADF7 File Offset: 0x003A8FF7
			public StoreInfo(Location location, Identifier identifier) : this(location)
			{
				this.Identifier = identifier;
				this.Balance = location.StoreInitialBalance;
				this.Stock = this.CreateStock();
				this.GenerateSpecials();
				this.GeneratePriceModifier();
			}

			// Token: 0x06008AB1 RID: 35505 RVA: 0x003AAE2C File Offset: 0x003A902C
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

			// Token: 0x06008AB2 RID: 35506 RVA: 0x003AAF98 File Offset: 0x003A9198
			public static PurchasedItem CreateInitialStockItem(Location location, ItemPrefab itemPrefab, PriceInfo priceInfo)
			{
				int quantity = Rand.Range(priceInfo.MinAvailableAmount, priceInfo.MaxAvailableAmount + 1, Rand.RandSync.Unsynced);
				quantity = Math.Min(quantity + location.WorldStepsSinceVisited, priceInfo.MaxAvailableAmount);
				return new PurchasedItem(itemPrefab, quantity, null);
			}

			// Token: 0x06008AB3 RID: 35507 RVA: 0x003AAFD8 File Offset: 0x003A91D8
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

			// Token: 0x06008AB4 RID: 35508 RVA: 0x003AB044 File Offset: 0x003A9244
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

			// Token: 0x06008AB5 RID: 35509 RVA: 0x003AB1D4 File Offset: 0x003A93D4
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

			// Token: 0x06008AB6 RID: 35510 RVA: 0x003AB350 File Offset: 0x003A9550
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

			// Token: 0x06008AB7 RID: 35511 RVA: 0x003AB4CC File Offset: 0x003A96CC
			public void GeneratePriceModifier()
			{
				this.PriceModifier = Rand.Range(-this.Location.StorePriceModifierRange, this.Location.StorePriceModifierRange + 1, Rand.RandSync.Unsynced);
			}

			// Token: 0x06008AB8 RID: 35512 RVA: 0x003AB4F4 File Offset: 0x003A96F4
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

			// Token: 0x06008AB9 RID: 35513 RVA: 0x003AB6D0 File Offset: 0x003A98D0
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

			// Token: 0x06008ABA RID: 35514 RVA: 0x003AB7BC File Offset: 0x003A99BC
			public void SetMerchantFaction(Identifier factionIdentifier)
			{
				this.MerchantFaction = factionIdentifier;
			}

			// Token: 0x06008ABB RID: 35515 RVA: 0x003AB7C8 File Offset: 0x003A99C8
			public Identifier GetMerchantOrLocationFactionIdentifier()
			{
				Identifier merchantFaction = this.MerchantFaction;
				Faction faction = this.Location.Faction;
				Identifier identifier = (faction != null) ? faction.Prefab.Identifier : Identifier.Empty;
				return merchantFaction.IfEmpty(identifier);
			}

			// Token: 0x06008ABC RID: 35516 RVA: 0x003AB808 File Offset: 0x003A9A08
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

			// Token: 0x06008ABD RID: 35517 RVA: 0x003AB8F4 File Offset: 0x003A9AF4
			public override string ToString()
			{
				return this.Identifier.Value;
			}

			// Token: 0x06008ABE RID: 35518 RVA: 0x003AB910 File Offset: 0x003A9B10
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

			// Token: 0x06008AC0 RID: 35520 RVA: 0x003AB9D0 File Offset: 0x003A9BD0
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

			// Token: 0x06008AC1 RID: 35521 RVA: 0x003ABA34 File Offset: 0x003A9C34
			[CompilerGenerated]
			internal static float <GetAdjustedItemSellPrice>g__GetMultiplierForItem|41_2(Character character, ItemPrefab item)
			{
				return item.Tags.Sum((Identifier tag) => character.Info.GetSavedStatValue(StatTypes.StoreSellMultiplier, tag)) + character.Info.GetSavedStatValue(StatTypes.StoreSellMultiplier, Tags.StatIdentifierTargetAll);
			}
		}

		// Token: 0x02000FED RID: 4077
		private readonly struct LoadedMission
		{
			// Token: 0x06008AC2 RID: 35522 RVA: 0x003ABA80 File Offset: 0x003A9C80
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

			// Token: 0x040056F9 RID: 22265
			public readonly MissionPrefab MissionPrefab;

			// Token: 0x040056FA RID: 22266
			public readonly int TimesAttempted;

			// Token: 0x040056FB RID: 22267
			public readonly int OriginLocationIndex;

			// Token: 0x040056FC RID: 22268
			public readonly int DestinationIndex;

			// Token: 0x040056FD RID: 22269
			public readonly bool SelectedMission;
		}

		// Token: 0x02000FEE RID: 4078
		public class AbilityLocation : AbilityObject, IAbilityLocation
		{
			// Token: 0x06008AC3 RID: 35523 RVA: 0x003ABAFF File Offset: 0x003A9CFF
			public AbilityLocation(Location location)
			{
				this.Location = location;
			}

			// Token: 0x17001C49 RID: 7241
			// (get) Token: 0x06008AC4 RID: 35524 RVA: 0x003ABB0E File Offset: 0x003A9D0E
			// (set) Token: 0x06008AC5 RID: 35525 RVA: 0x003ABB16 File Offset: 0x003A9D16
			public Location Location { get; set; }
		}
	}
}
