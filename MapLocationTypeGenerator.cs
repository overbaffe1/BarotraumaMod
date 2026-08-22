using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000323 RID: 803
	[NullableContext(1)]
	[Nullable(0)]
	internal class MapLocationTypeGenerator
	{
		// Token: 0x1700110E RID: 4366
		// (get) Token: 0x06004053 RID: 16467 RVA: 0x0023CE3C File Offset: 0x0023B03C
		private bool IsEveryLocationTypeAssigned
		{
			get
			{
				return this.locationTypeAmountsToAssign.SelectMany((KeyValuePair<LocationType, List<MapLocationTypeGenerator.LocationTypeCount>> kvp) => kvp.Value).None((MapLocationTypeGenerator.LocationTypeCount locationTypeCount) => locationTypeCount.AmountToAssign > 0);
			}
		}

		// Token: 0x06004054 RID: 16468 RVA: 0x0023CE98 File Offset: 0x0023B098
		public MapLocationTypeGenerator(CampaignMode campaign, Map map)
		{
			this.filledLocations = new List<Location>();
			this.map = map;
			this.campaign = campaign;
			this.locationTypeAmountsToAssign = new Dictionary<LocationType, List<MapLocationTypeGenerator.LocationTypeCount>>();
			this.GenerateTotalAmountsToAssign();
		}

		// Token: 0x06004055 RID: 16469 RVA: 0x0023CEF0 File Offset: 0x0023B0F0
		private void GenerateTotalAmountsToAssign()
		{
			foreach (LocationType locationTypePrefab in this.orderedLocationTypes)
			{
				foreach (LocationType.AreaSettingData areaSetting in locationTypePrefab.AreaSettings)
				{
					if (areaSetting.HasCounts)
					{
						if (!areaSetting.HasValidData)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Biome ID is invalid for AreaSetting in locationType '");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(locationTypePrefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("'. Skipping invalid setting.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), locationTypePrefab.ContentPackage);
						}
						else
						{
							int amountToAdd = Rand.GetRNG(Rand.RandSync.ServerAndClient).Next(areaSetting.MinCount.GetValueOrDefault(), areaSetting.MaxCount.GetValueOrDefault(1));
							if (amountToAdd > 0)
							{
								MapLocationTypeGenerator.LocationTypeCount existingCount = this.<GenerateTotalAmountsToAssign>g__GetExistingCount|10_1(locationTypePrefab, areaSetting);
								if (existingCount != null)
								{
									existingCount.AmountToAssign += amountToAdd;
								}
								else
								{
									if (!this.locationTypeAmountsToAssign.ContainsKey(locationTypePrefab))
									{
										this.locationTypeAmountsToAssign[locationTypePrefab] = new List<MapLocationTypeGenerator.LocationTypeCount>();
									}
									this.locationTypeAmountsToAssign[locationTypePrefab].Add(MapLocationTypeGenerator.<GenerateTotalAmountsToAssign>g__CreateNewCount|10_0(areaSetting, amountToAdd));
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06004056 RID: 16470 RVA: 0x0023D078 File Offset: 0x0023B278
		public void AddToLocationsPerZone(int zone, Location location)
		{
			if (!this.locationsPerZone.ContainsKey(zone))
			{
				this.locationsPerZone[zone] = new List<Location>();
			}
			this.locationsPerZone[zone].Add(location);
		}

		// Token: 0x06004057 RID: 16471 RVA: 0x0023D0AB File Offset: 0x0023B2AB
		public void AddToFilled(Location location)
		{
			if (this.filledLocations.Contains(location))
			{
				return;
			}
			this.filledLocations.Add(location);
		}

		// Token: 0x06004058 RID: 16472 RVA: 0x0023D0C8 File Offset: 0x0023B2C8
		public bool IsFilled(Location location)
		{
			return this.filledLocations.Contains(location);
		}

		// Token: 0x06004059 RID: 16473 RVA: 0x0023D0D8 File Offset: 0x0023B2D8
		public void ChangeLocationTypeAndName(CampaignMode campaign, Location location, LocationType suitableLocationType)
		{
			location.ChangeType(campaign, suitableLocationType, false, false);
			if (!suitableLocationType.ForceLocationName.IsEmpty)
			{
				location.ForceName(suitableLocationType.ForceLocationName);
				return;
			}
			location.AssignRandomName(location.Type, Rand.GetRNG(Rand.RandSync.ServerAndClient), this.map.Locations);
		}

		// Token: 0x0600405A RID: 16474 RVA: 0x0023D128 File Offset: 0x0023B328
		public void AssignForcedBiomeGateTypes(IEnumerable<Location> gateLocations)
		{
			foreach (Location gateLocation in gateLocations)
			{
				foreach (LocationType locationType in this.orderedLocationTypes)
				{
					if (locationType.BiomeGate == LocationType.BiomeGateSetting.Force)
					{
						int zone = this.map.GetZoneIndex(gateLocation.MapPosition.X);
						if (!locationType.HasCounts() || this.TypeHasRemainingCountForLocation(locationType, gateLocation))
						{
							if (!locationType.Faction.IsEmpty)
							{
								Identifier? identifier = new Identifier?(locationType.Faction);
								Faction faction = gateLocation.Faction;
								Identifier? identifier2;
								Identifier? identifier3;
								if (faction == null)
								{
									identifier2 = null;
									identifier3 = identifier2;
								}
								else
								{
									identifier3 = new Identifier?(faction.Prefab.Identifier);
								}
								identifier2 = identifier3;
								if (identifier != identifier2)
								{
									continue;
								}
							}
							if (gateLocation.Type == locationType)
							{
								this.AddToFilled(gateLocation);
								this.RemoveOneFromTotals(locationType, gateLocation);
								break;
							}
							if (!this.IsFilled(gateLocation) && locationType.IsValidForZoneOrBiome(new int?(zone), new Identifier?(gateLocation.Biome.Identifier)))
							{
								this.AddToFilled(gateLocation);
								this.ChangeLocationTypeAndName(this.campaign, gateLocation, locationType);
								this.RemoveOneFromTotals(locationType, gateLocation);
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600405B RID: 16475 RVA: 0x0023D2AC File Offset: 0x0023B4AC
		private bool TypeHasRemainingCountForLocation(LocationType countLocationType, Location location)
		{
			List<MapLocationTypeGenerator.LocationTypeCount> locationTypeCounts;
			if (!this.locationTypeAmountsToAssign.TryGetValue(countLocationType, out locationTypeCounts))
			{
				return false;
			}
			bool hasZoneCount = locationTypeCounts.Any(delegate(MapLocationTypeGenerator.LocationTypeCount ltc)
			{
				int? difficultyZone = ltc.DifficultyZone;
				int zoneIndex = this.map.GetZoneIndex(location.MapPosition.X);
				return (difficultyZone.GetValueOrDefault() == zoneIndex & difficultyZone != null) && ltc.AmountToAssign > 0;
			});
			bool hasBiomeCount = locationTypeCounts.Any(delegate(MapLocationTypeGenerator.LocationTypeCount ltc)
			{
				Identifier? identifier = new Identifier?(location.Biome.Identifier);
				return ltc.BiomeId == identifier && ltc.AmountToAssign > 0;
			});
			return hasZoneCount || hasBiomeCount;
		}

		// Token: 0x0600405C RID: 16476 RVA: 0x0023D308 File Offset: 0x0023B508
		private int GetRemainingCount(LocationType locationType, LocationType.AreaSettingData areaSetting)
		{
			List<MapLocationTypeGenerator.LocationTypeCount> locationTypeCounts;
			this.locationTypeAmountsToAssign.TryGetValue(locationType, out locationTypeCounts);
			if (locationTypeCounts == null || locationTypeCounts.None(null))
			{
				return 0;
			}
			MapLocationTypeGenerator.LocationTypeCount match = locationTypeCounts.FirstOrDefault((MapLocationTypeGenerator.LocationTypeCount ltc) => areaSetting.MatchesRemainingCount(ltc));
			if (match == null)
			{
				return 0;
			}
			return match.AmountToAssign;
		}

		// Token: 0x0600405D RID: 16477 RVA: 0x0023D360 File Offset: 0x0023B560
		public void RemoveOneFromTotals(LocationType locationType, Location location)
		{
			List<MapLocationTypeGenerator.LocationTypeCount> locationTypeCounts;
			if (!this.locationTypeAmountsToAssign.TryGetValue(locationType, out locationTypeCounts))
			{
				return;
			}
			MapLocationTypeGenerator.LocationTypeCount zoneMatch = locationTypeCounts.FirstOrDefault(delegate(MapLocationTypeGenerator.LocationTypeCount ltc)
			{
				if (ltc.AmountToAssign > 0)
				{
					int? difficultyZone = ltc.DifficultyZone;
					int zoneIndex = this.map.GetZoneIndex(location.MapPosition.X);
					return difficultyZone.GetValueOrDefault() == zoneIndex & difficultyZone != null;
				}
				return false;
			});
			if (zoneMatch != null)
			{
				zoneMatch.AmountToAssign--;
			}
			MapLocationTypeGenerator.LocationTypeCount biomeMatch = locationTypeCounts.FirstOrDefault(delegate(MapLocationTypeGenerator.LocationTypeCount ltc)
			{
				if (ltc.AmountToAssign > 0)
				{
					Identifier? identifier = new Identifier?(location.Biome.Identifier);
					return ltc.BiomeId == identifier;
				}
				return false;
			});
			if (biomeMatch != null)
			{
				biomeMatch.AmountToAssign--;
			}
		}

		// Token: 0x0600405E RID: 16478 RVA: 0x0023D3DC File Offset: 0x0023B5DC
		public void AssignLocationTypesBasedOnDesiredPosition(IEnumerable<Location> gateLocations)
		{
			MapLocationTypeGenerator.<>c__DisplayClass19_0 CS$<>8__locals1 = new MapLocationTypeGenerator.<>c__DisplayClass19_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.gateLocations = gateLocations;
			foreach (LocationType locationType in LocationType.Prefabs)
			{
				using (List<LocationType.AreaSettingData>.Enumerator enumerator2 = locationType.AreaSettings.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						LocationType.AreaSettingData areaSetting = enumerator2.Current;
						if (areaSetting.DesiredPosition != null)
						{
							int remainingCount = this.GetRemainingCount(locationType, areaSetting);
							if (remainingCount != 0)
							{
								IEnumerable<Location> source = from location in this.map.Locations
								where areaSetting.MatchesLocation(CS$<>8__locals1.<>4__this.map, location)
								select location;
								Func<Location, bool> predicate;
								if ((predicate = CS$<>8__locals1.<>9__2) == null)
								{
									predicate = (CS$<>8__locals1.<>9__2 = ((Location location) => !CS$<>8__locals1.gateLocations.Contains(location)));
								}
								IEnumerable<Location> locations = source.Where(predicate);
								if (!locations.None(null))
								{
									CS$<>8__locals1.<AssignLocationTypesBasedOnDesiredPosition>g__FillLocations|0(locations, areaSetting.DesiredPosition.Value, remainingCount, locationType);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600405F RID: 16479 RVA: 0x0023D564 File Offset: 0x0023B764
		public void AssignLocationTypesBasedOnCount(IEnumerable<Location> gateLocations, IEnumerable<Location> locations)
		{
			List<Location> shuffledLocations = locations.ToList<Location>();
			shuffledLocations.Shuffle(Rand.GetRNG(Rand.RandSync.ServerAndClient));
			foreach (Location location in shuffledLocations)
			{
				if (!this.IsFilled(location))
				{
					bool isBiomeGate = gateLocations.Contains(location);
					if (!isBiomeGate || location.Type.BiomeGate != LocationType.BiomeGateSetting.Force)
					{
						if (this.IsEveryLocationTypeAssigned)
						{
							break;
						}
						LocationType suitableLocationType = this.TryPickSuitableLocationTypeFromTotals(location, isBiomeGate);
						if (suitableLocationType != null)
						{
							this.ChangeLocationTypeAndName(this.campaign, location, suitableLocationType);
							this.AddToFilled(location);
						}
					}
				}
			}
			if (!this.IsEveryLocationTypeAssigned)
			{
				ContentPackage nonVanillaContentPackage = null;
				StringBuilder sb = new StringBuilder("Following location types could not be assigned to locations:\n");
				foreach (KeyValuePair<LocationType, List<MapLocationTypeGenerator.LocationTypeCount>> keyValuePair in this.locationTypeAmountsToAssign)
				{
					LocationType locationType2;
					List<MapLocationTypeGenerator.LocationTypeCount> list;
					keyValuePair.Deconstruct(out locationType2, out list);
					LocationType locationType = locationType2;
					List<MapLocationTypeGenerator.LocationTypeCount> locationTypeCounts = list;
					foreach (MapLocationTypeGenerator.LocationTypeCount locationTypeCount in locationTypeCounts)
					{
						if (locationTypeCount.AmountToAssign > 0)
						{
							if (locationType.ContentPackage != ContentPackageManager.VanillaCorePackage)
							{
								nonVanillaContentPackage = locationType.ContentPackage;
							}
							StringBuilder stringBuilder = sb;
							StringBuilder stringBuilder2 = stringBuilder;
							StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(8, 3, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("- ");
							appendInterpolatedStringHandler.AppendFormatted<Identifier>(locationType.Identifier);
							appendInterpolatedStringHandler.AppendLiteral(" - ");
							appendInterpolatedStringHandler.AppendFormatted<LocalizedString>(locationType.Name);
							appendInterpolatedStringHandler.AppendLiteral(" (");
							appendInterpolatedStringHandler.AppendFormatted(locationTypeCount.ToDebugString());
							appendInterpolatedStringHandler.AppendLiteral(")");
							stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
						}
					}
				}
				DebugConsole.AddWarning(sb.ToString(), nonVanillaContentPackage);
			}
		}

		// Token: 0x06004060 RID: 16480 RVA: 0x0023D764 File Offset: 0x0023B964
		[return: Nullable(2)]
		private LocationType TryPickSuitableLocationTypeFromTotals(Location location, bool isBiomeGate)
		{
			int locationZone = this.map.GetZoneIndex(location.MapPosition.X);
			Identifier locationBiomeId = location.Biome.Identifier;
			LocationType suitableLocationType = null;
			List<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>> potentialLocationTypeCounts = new List<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>>();
			foreach (KeyValuePair<LocationType, List<MapLocationTypeGenerator.LocationTypeCount>> keyValuePair in this.locationTypeAmountsToAssign)
			{
				LocationType locationType2;
				List<MapLocationTypeGenerator.LocationTypeCount> list;
				keyValuePair.Deconstruct(out locationType2, out list);
				LocationType locationType = locationType2;
				List<MapLocationTypeGenerator.LocationTypeCount> countList = list;
				if (!isBiomeGate || locationType.BiomeGate != LocationType.BiomeGateSetting.Deny)
				{
					foreach (MapLocationTypeGenerator.LocationTypeCount locationTypeCount2 in countList)
					{
						if (locationTypeCount2.AmountToAssign > 0)
						{
							potentialLocationTypeCounts.Add(new ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>(locationType, locationTypeCount2));
						}
					}
				}
			}
			IEnumerable<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>> zoneMatches = potentialLocationTypeCounts.Where(delegate([TupleElementNames(new string[]
			{
				"LocationType",
				"Count"
			})] ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount> locationTypeCount)
			{
				int? difficultyZone = locationTypeCount.Item2.DifficultyZone;
				int locationZone = locationZone;
				return difficultyZone.GetValueOrDefault() == locationZone & difficultyZone != null;
			});
			IEnumerable<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>> biomeMatches = potentialLocationTypeCounts.Where(delegate([TupleElementNames(new string[]
			{
				"LocationType",
				"Count"
			})] ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount> locationTypeCount)
			{
				MapLocationTypeGenerator.LocationTypeCount item = locationTypeCount.Item2;
				Identifier? identifier = new Identifier?(locationBiomeId);
				return item.BiomeId == identifier;
			});
			if (zoneMatches.None(null) && biomeMatches.None(null))
			{
				return null;
			}
			if (zoneMatches.Any<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>>() && biomeMatches.Any<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>>())
			{
				ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount> dualMatch = zoneMatches.FirstOrDefault(([TupleElementNames(new string[]
				{
					"LocationType",
					"Count"
				})] ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount> zoneCount) => biomeMatches.Any(([TupleElementNames(new string[]
				{
					"LocationType",
					"Count"
				})] ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount> biomeCount) => biomeCount.Item1 == zoneCount.Item1));
				if (dualMatch.Item1 != null)
				{
					suitableLocationType = dualMatch.Item1;
				}
			}
			if (suitableLocationType == null)
			{
				suitableLocationType = (zoneMatches.Any<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>>() ? zoneMatches.First<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>>().Item1 : biomeMatches.First<ValueTuple<LocationType, MapLocationTypeGenerator.LocationTypeCount>>().Item1);
			}
			if (suitableLocationType != null)
			{
				this.RemoveOneFromTotals(suitableLocationType, location);
			}
			return suitableLocationType;
		}

		// Token: 0x06004061 RID: 16481 RVA: 0x0023D914 File Offset: 0x0023BB14
		[CompilerGenerated]
		internal static MapLocationTypeGenerator.LocationTypeCount <GenerateTotalAmountsToAssign>g__CreateNewCount|10_0(LocationType.AreaSettingData areaSettingData, int amountToAdd)
		{
			LocationType.BiomeSettingData biomeSettingData = areaSettingData as LocationType.BiomeSettingData;
			if (biomeSettingData != null)
			{
				return new MapLocationTypeGenerator.LocationTypeCount(amountToAdd, biomeSettingData.BiomeIdentifier);
			}
			LocationType.DifficultyZoneSettingData difficultyZoneSettingData = areaSettingData as LocationType.DifficultyZoneSettingData;
			if (difficultyZoneSettingData != null)
			{
				return new MapLocationTypeGenerator.LocationTypeCount(amountToAdd, difficultyZoneSettingData.DifficultyZone);
			}
			throw new ArgumentException("Unrecognized areaSettingData");
		}

		// Token: 0x06004062 RID: 16482 RVA: 0x0023D95C File Offset: 0x0023BB5C
		[CompilerGenerated]
		[return: Nullable(2)]
		private MapLocationTypeGenerator.LocationTypeCount <GenerateTotalAmountsToAssign>g__GetExistingCount|10_1(LocationType locationTypePrefab, LocationType.AreaSettingData areaSettingData)
		{
			List<MapLocationTypeGenerator.LocationTypeCount> value;
			if (!this.locationTypeAmountsToAssign.TryGetValue(locationTypePrefab, out value))
			{
				return null;
			}
			return value.FirstOrDefault(new Func<MapLocationTypeGenerator.LocationTypeCount, bool>(areaSettingData.MatchesRemainingCount));
		}

		// Token: 0x04002185 RID: 8581
		private Dictionary<LocationType, List<MapLocationTypeGenerator.LocationTypeCount>> locationTypeAmountsToAssign;

		// Token: 0x04002186 RID: 8582
		private readonly Map map;

		// Token: 0x04002187 RID: 8583
		private readonly CampaignMode campaign;

		// Token: 0x04002188 RID: 8584
		private readonly List<Location> filledLocations;

		// Token: 0x04002189 RID: 8585
		private readonly Dictionary<int, List<Location>> locationsPerZone = new Dictionary<int, List<Location>>();

		// Token: 0x0400218A RID: 8586
		private readonly IOrderedEnumerable<LocationType> orderedLocationTypes = LocationType.Prefabs.GetOrdered();

		// Token: 0x02001018 RID: 4120
		[NullableContext(0)]
		internal class LocationTypeCount
		{
			// Token: 0x06008B47 RID: 35655 RVA: 0x003ACBE4 File Offset: 0x003AADE4
			public LocationTypeCount(int amountToAssign, int difficultyZone)
			{
				this.AmountToAssign = amountToAssign;
				this.DifficultyZone = new int?(difficultyZone);
			}

			// Token: 0x06008B48 RID: 35656 RVA: 0x003ACBFF File Offset: 0x003AADFF
			public LocationTypeCount(int amountToAssign, Identifier biomeId)
			{
				this.AmountToAssign = amountToAssign;
				this.BiomeId = new Identifier?(biomeId);
			}

			// Token: 0x06008B49 RID: 35657 RVA: 0x003ACC1C File Offset: 0x003AAE1C
			[NullableContext(1)]
			public string ToDebugString()
			{
				if (this.DifficultyZone != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 2);
					defaultInterpolatedStringHandler.AppendLiteral("x");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.AmountToAssign);
					defaultInterpolatedStringHandler.AppendLiteral(" in (zone ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.DifficultyZone.Value);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
				if (this.BiomeId != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("x");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(this.AmountToAssign);
					defaultInterpolatedStringHandler2.AppendLiteral(" in (biome ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.BiomeId.Value);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					return defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("x");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.AmountToAssign);
				return defaultInterpolatedStringHandler3.ToStringAndClear();
			}

			// Token: 0x0400575D RID: 22365
			public int AmountToAssign;

			// Token: 0x0400575E RID: 22366
			public int? DifficultyZone;

			// Token: 0x0400575F RID: 22367
			public Identifier? BiomeId;
		}
	}
}
