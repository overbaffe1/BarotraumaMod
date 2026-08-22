using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000247 RID: 583
	internal class Map
	{
		// Token: 0x17000C35 RID: 3125
		// (get) Token: 0x0600295C RID: 10588 RVA: 0x0010CEEC File Offset: 0x0010B0EC
		// (set) Token: 0x0600295D RID: 10589 RVA: 0x0010CEF4 File Offset: 0x0010B0F4
		public int Width { get; private set; }

		// Token: 0x17000C36 RID: 3126
		// (get) Token: 0x0600295E RID: 10590 RVA: 0x0010CEFD File Offset: 0x0010B0FD
		// (set) Token: 0x0600295F RID: 10591 RVA: 0x0010CF05 File Offset: 0x0010B105
		public int Height { get; private set; }

		// Token: 0x17000C37 RID: 3127
		// (get) Token: 0x06002960 RID: 10592 RVA: 0x0010CF0E File Offset: 0x0010B10E
		public IReadOnlyList<Location> EndLocations
		{
			get
			{
				return this.endLocations;
			}
		}

		// Token: 0x17000C38 RID: 3128
		// (get) Token: 0x06002961 RID: 10593 RVA: 0x0010CF16 File Offset: 0x0010B116
		// (set) Token: 0x06002962 RID: 10594 RVA: 0x0010CF1E File Offset: 0x0010B11E
		public Location StartLocation { get; private set; }

		// Token: 0x17000C39 RID: 3129
		// (get) Token: 0x06002963 RID: 10595 RVA: 0x0010CF27 File Offset: 0x0010B127
		// (set) Token: 0x06002964 RID: 10596 RVA: 0x0010CF2F File Offset: 0x0010B12F
		public Location CurrentLocation { get; private set; }

		// Token: 0x17000C3A RID: 3130
		// (get) Token: 0x06002965 RID: 10597 RVA: 0x0010CF38 File Offset: 0x0010B138
		public int CurrentLocationIndex
		{
			get
			{
				return this.Locations.IndexOf(this.CurrentLocation);
			}
		}

		// Token: 0x17000C3B RID: 3131
		// (get) Token: 0x06002966 RID: 10598 RVA: 0x0010CF4B File Offset: 0x0010B14B
		// (set) Token: 0x06002967 RID: 10599 RVA: 0x0010CF53 File Offset: 0x0010B153
		public Location SelectedLocation { get; private set; }

		// Token: 0x17000C3C RID: 3132
		// (get) Token: 0x06002968 RID: 10600 RVA: 0x0010CF5C File Offset: 0x0010B15C
		public int SelectedLocationIndex
		{
			get
			{
				return this.Locations.IndexOf(this.SelectedLocation);
			}
		}

		// Token: 0x06002969 RID: 10601 RVA: 0x0010CF70 File Offset: 0x0010B170
		public IEnumerable<int> GetSelectedMissionIndices()
		{
			if (this.SelectedConnection != null)
			{
				return this.CurrentLocation.GetSelectedMissionIndices();
			}
			return Enumerable.Empty<int>();
		}

		// Token: 0x17000C3D RID: 3133
		// (get) Token: 0x0600296A RID: 10602 RVA: 0x0010CF98 File Offset: 0x0010B198
		// (set) Token: 0x0600296B RID: 10603 RVA: 0x0010CFA0 File Offset: 0x0010B1A0
		public LocationConnection SelectedConnection { get; private set; }

		// Token: 0x17000C3E RID: 3134
		// (get) Token: 0x0600296C RID: 10604 RVA: 0x0010CFA9 File Offset: 0x0010B1A9
		// (set) Token: 0x0600296D RID: 10605 RVA: 0x0010CFB1 File Offset: 0x0010B1B1
		public string Seed { get; private set; }

		// Token: 0x17000C3F RID: 3135
		// (get) Token: 0x0600296E RID: 10606 RVA: 0x0010CFBA File Offset: 0x0010B1BA
		// (set) Token: 0x0600296F RID: 10607 RVA: 0x0010CFC2 File Offset: 0x0010B1C2
		public List<Location> Locations { get; private set; }

		// Token: 0x17000C40 RID: 3136
		// (get) Token: 0x06002970 RID: 10608 RVA: 0x0010CFCB File Offset: 0x0010B1CB
		// (set) Token: 0x06002971 RID: 10609 RVA: 0x0010CFD3 File Offset: 0x0010B1D3
		public List<LocationConnection> Connections { get; private set; }

		// Token: 0x17000C41 RID: 3137
		// (get) Token: 0x06002972 RID: 10610 RVA: 0x0010CFDC File Offset: 0x0010B1DC
		public IOrderedEnumerable<Biome> OrderedBiomes
		{
			get
			{
				IOrderedEnumerable<Biome> result;
				if ((result = this._orderedBiomes) == null)
				{
					IOrderedEnumerable<Biome> orderedEnumerable = this._orderedBiomes = Biome.Prefabs.GetOrdered();
					result = orderedEnumerable;
				}
				return result;
			}
		}

		// Token: 0x06002973 RID: 10611 RVA: 0x0010D008 File Offset: 0x0010B208
		public Map(CampaignSettings settings)
		{
			this.generationParams = MapGenerationParams.Instance;
			this.Width = this.generationParams.Width;
			this.Height = this.generationParams.Height;
			this.Locations = new List<Location>();
			this.Connections = new List<LocationConnection>();
			if (this.generationParams.RadiationParams != null)
			{
				this.Radiation = new Radiation(this, this.generationParams.RadiationParams, null)
				{
					Enabled = settings.RadiationEnabled
				};
			}
		}

		// Token: 0x06002974 RID: 10612 RVA: 0x0010D0C4 File Offset: 0x0010B2C4
		private Map(CampaignMode campaign, XElement element) : this(campaign.Settings)
		{
			this.Seed = element.GetAttributeString("seed", "a");
			Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed));
			this.Width = element.GetAttributeInt("width", this.Width);
			this.Height = element.GetAttributeInt("height", this.Height);
			bool lairsFound = false;
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "location"))
				{
					if (a == "radiation")
					{
						this.Radiation = new Radiation(this, this.generationParams.RadiationParams, subElement)
						{
							Enabled = campaign.Settings.RadiationEnabled
						};
					}
				}
				else
				{
					int i = subElement.GetAttributeInt("i", 0);
					while (this.Locations.Count <= i)
					{
						this.Locations.Add(null);
					}
					lairsFound |= subElement.GetAttributeString("type", "").Equals("lair", StringComparison.OrdinalIgnoreCase);
					this.Locations[i] = new Location(campaign, subElement);
				}
			}
			List<XElement> connectionElements = new List<XElement>();
			foreach (XElement subElement2 in element.Elements())
			{
				string a2 = subElement2.Name.ToString().ToLowerInvariant();
				if (a2 == "connection")
				{
					Point locationIndices = subElement2.GetAttributePoint("locations", new Point(0, 1));
					if (locationIndices.X != locationIndices.Y)
					{
						LocationConnection connection = new LocationConnection(this.Locations[locationIndices.X], this.Locations[locationIndices.Y])
						{
							Passed = subElement2.GetAttributeBool("passed", false),
							Locked = subElement2.GetAttributeBool("locked", false),
							Difficulty = subElement2.GetAttributeFloat("difficulty", 0f)
						};
						this.Locations[locationIndices.X].Connections.Add(connection);
						this.Locations[locationIndices.Y].Connections.Add(connection);
						string biomeId = subElement2.GetAttributeString("biome", "");
						LocationConnection locationConnection = connection;
						Biome biome;
						if ((biome = Biome.Prefabs.FirstOrDefault((Biome b) => b.Identifier == biomeId)) == null)
						{
							biome = (Biome.Prefabs.FirstOrDefault((Biome b) => !b.OldIdentifier.IsEmpty && b.OldIdentifier == biomeId) ?? Biome.Prefabs.First<Biome>());
						}
						locationConnection.Biome = biome;
						connection.Difficulty = MathHelper.Clamp(connection.Difficulty, connection.Biome.MinDifficulty, connection.Biome.AdjustedMaxDifficulty);
						connection.LevelData = new LevelData(subElement2.Element("Level"), new float?(connection.Difficulty), false);
						this.Connections.Add(connection);
						connectionElements.Add(subElement2);
					}
				}
			}
			Random rand = new MTRandom(ToolBox.StringToInt(this.Seed));
			if (this.Locations.First<Location>().Biome == null)
			{
				this.AssignBiomes(rand);
			}
			int startLocationindex = element.GetAttributeInt("startlocation", -1);
			if (startLocationindex >= 0 && startLocationindex < this.Locations.Count)
			{
				this.StartLocation = this.Locations[startLocationindex];
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error while loading the map. Start location index out of bounds (index: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(startLocationindex);
				defaultInterpolatedStringHandler.AppendLiteral(", location count: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Locations.Count);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				foreach (Location location in this.Locations)
				{
					if (location.Type.HasOutpost && (this.StartLocation == null || location.MapPosition.X < this.StartLocation.MapPosition.X))
					{
						this.StartLocation = location;
					}
				}
			}
			if (element.GetAttribute("endlocation", StringComparison.OrdinalIgnoreCase) != null)
			{
				int endLocationIndex = element.GetAttributeInt("endlocation", -1);
				if (endLocationIndex >= 0 && endLocationIndex < this.Locations.Count)
				{
					this.endLocations.Add(this.Locations[endLocationIndex]);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(90, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Error while loading the map. End location index out of bounds (index: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(endLocationIndex);
					defaultInterpolatedStringHandler2.AppendLiteral(", location count: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(this.Locations.Count);
					defaultInterpolatedStringHandler2.AppendLiteral(").");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
			}
			else
			{
				int[] endLocationindices = element.GetAttributeIntArray("endlocations", Array.Empty<int>());
				foreach (int endLocationIndex2 in endLocationindices)
				{
					if (endLocationIndex2 >= 0 && endLocationIndex2 < this.Locations.Count)
					{
						this.endLocations.Add(this.Locations[endLocationIndex2]);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(90, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Error while loading the map. End location index out of bounds (index: ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(endLocationIndex2);
						defaultInterpolatedStringHandler3.AppendLiteral(", location count: ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(this.Locations.Count);
						defaultInterpolatedStringHandler3.AppendLiteral(").");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
					}
				}
			}
			if (!this.endLocations.Any<Location>())
			{
				DebugConsole.AddWarning("Error while loading the map. No end location(s) found. Choosing the rightmost location as the end location...", null);
				Location endLocation = null;
				foreach (Location location2 in this.Locations)
				{
					if (endLocation == null || location2.MapPosition.X > endLocation.MapPosition.X)
					{
						endLocation = location2;
					}
				}
				this.endLocations.Add(endLocation);
			}
			Location firstEndLocation = this.EndLocations[0];
			Biome endBiome = firstEndLocation.Biome;
			int missingOutpostCount = endBiome.EndBiomeLocationCount - this.endLocations.Count;
			for (int j = 0; j < missingOutpostCount; j++)
			{
				Vector2 mapPos = new Vector2(MathHelper.Lerp(firstEndLocation.MapPosition.X, (float)this.Width, MathHelper.Lerp(0.2f, 0.8f, (float)j / (float)missingOutpostCount)), (float)this.Height * MathHelper.Lerp(0.2f, 1f, (float)rand.NextDouble()));
				Location newEndLocation = new Location(mapPos, new int?(this.generationParams.DifficultyZones), new Identifier?(endBiome.Identifier), rand, false, firstEndLocation.Type, this.Locations);
				newEndLocation.Biome = endBiome;
				newEndLocation.LevelData = new LevelData(newEndLocation, this, 100f);
				this.Locations.Add(newEndLocation);
				this.endLocations.Add(newEndLocation);
			}
			if (lairsFound)
			{
				if (!this.Connections.Any((LocationConnection c) => c.LevelData.HasHuntingGrounds))
				{
					for (int k = 0; k < this.Connections.Count; k++)
					{
						this.Connections[k].LevelData.HasHuntingGrounds = (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.Connections[k].Difficulty / 100f * 0.3f);
						connectionElements[k].SetAttributeValue("hashuntinggrounds", true);
					}
				}
			}
			this.AssignEndLocationLevelData(campaign);
			float maxX = (from l in this.Locations
			select l.MapPosition.X).Max();
			if (maxX > (float)this.Width)
			{
				this.Width = (int)(maxX + 10f);
			}
			float maxY = (from l in this.Locations
			select l.MapPosition.Y).Max();
			if (maxY > (float)this.Height)
			{
				this.Height = (int)(maxY + 10f);
			}
		}

		// Token: 0x06002975 RID: 10613 RVA: 0x0010D9D8 File Offset: 0x0010BBD8
		public Map(CampaignMode campaign, string seed) : this(campaign.Settings)
		{
			this.Seed = seed;
			Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed));
			this.Generate(campaign);
			if (this.Locations.Count == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Generating a campaign map failed (no locations created). Width: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Width);
				defaultInterpolatedStringHandler.AppendLiteral(", height: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Height);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.<.ctor>g__FindStartLocation|60_2((Location l) => l.Type.Identifier == "outpost");
			if (this.CurrentLocation == null)
			{
				this.<.ctor>g__FindStartLocation|60_2((Location l) => l.Type.HasOutpost && l.Type.OutpostTeam == CharacterTeamType.FriendlyNPC);
			}
			this.StartLocation.SecondaryFaction = null;
			Faction faction;
			if (campaign == null)
			{
				faction = null;
			}
			else
			{
				faction = campaign.Factions.FirstOrDefault((Faction f) => f.Prefab.StartOutpost);
			}
			Faction startOutpostFaction = faction;
			if (startOutpostFaction != null)
			{
				this.StartLocation.Faction = startOutpostFaction;
			}
			foreach (LocationConnection connection in this.StartLocation.Connections)
			{
				Location otherLocation = connection.OtherLocation(this.StartLocation);
				LocationType outpostLocationType;
				if (!otherLocation.HasOutpost() && LocationType.Prefabs.TryGet("outpost".ToIdentifier(), out outpostLocationType))
				{
					otherLocation.ChangeType(campaign, outpostLocationType, false, true);
				}
				if (otherLocation.HasOutpost() && otherLocation.Type.OutpostTeam == CharacterTeamType.FriendlyNPC && otherLocation.Type.Faction.IsEmpty)
				{
					otherLocation.Faction = startOutpostFaction;
				}
			}
			if (campaign.CampaignMetadata.GetInt("campaign.endings".ToIdentifier(), new int?(0)) == 0 && (campaign.Settings.WorldHostility == WorldHostilityOption.Low || campaign.Settings.WorldHostility == WorldHostilityOption.Medium))
			{
				if (this.StartLocation != null)
				{
					this.StartLocation.LevelData = new LevelData(this.StartLocation, this, 0f);
				}
				foreach (LocationConnection locationConnection in this.StartLocation.Connections)
				{
					if (locationConnection.Difficulty > 0f)
					{
						locationConnection.Difficulty = 0f;
						locationConnection.LevelData = new LevelData(locationConnection);
					}
				}
			}
			LocationType tutorialOutpost;
			if (campaign.IsSinglePlayer && campaign.Settings.TutorialEnabled && LocationType.Prefabs.TryGet("tutorialoutpost", out tutorialOutpost))
			{
				this.CurrentLocation.ChangeType(campaign, tutorialOutpost, true, true);
			}
			else
			{
				LocationType forceStartOutpostType = (from lt in LocationType.Prefabs
				where lt.ForceAsStartOutpost
				select lt).GetRandom(Rand.RandSync.ServerAndClient);
				if (forceStartOutpostType != null)
				{
					this.CurrentLocation.ChangeType(campaign, forceStartOutpostType, true, true);
				}
			}
			this.Discover(this.CurrentLocation, true);
			this.Visit(this.CurrentLocation, true);
			this.CurrentLocation.CreateStores(false);
			foreach (Location location in this.Locations)
			{
				location.UnlockInitialMissions(Rand.RandSync.ServerAndClient);
			}
		}

		// Token: 0x06002976 RID: 10614 RVA: 0x0010DD78 File Offset: 0x0010BF78
		private void Generate(CampaignMode campaign)
		{
			Map.<>c__DisplayClass62_0 CS$<>8__locals1 = new Map.<>c__DisplayClass62_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.campaign = campaign;
			this.Connections.Clear();
			this.Locations.Clear();
			List<Vector2> voronoiSites = new List<Vector2>();
			for (float x = 10f; x < (float)this.Width - 10f; x += (float)this.generationParams.VoronoiSiteInterval.X)
			{
				for (float y = 10f; y < (float)this.Height - 10f; y += (float)this.generationParams.VoronoiSiteInterval.Y)
				{
					voronoiSites.Add(new Vector2(x + (float)this.generationParams.VoronoiSiteVariance.X * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient), y + (float)this.generationParams.VoronoiSiteVariance.Y * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient)));
				}
			}
			MapLocationTypeGenerator mapLocationTypeGenerator = new MapLocationTypeGenerator(CS$<>8__locals1.campaign, this);
			Voronoi voronoi = new Voronoi(0.5);
			List<GraphEdge> edges = voronoi.MakeVoronoiGraph(voronoiSites, this.Width, this.Height);
			Vector2 margin = new Vector2(Math.Min(10f, (float)this.Width * 0.1f), Math.Min(10f, (float)this.Height * 0.2f));
			float startX = margin.X;
			float endX = (float)this.Width - margin.X;
			float startY = margin.Y;
			float endY = (float)this.Height - margin.Y;
			if (!edges.Any<GraphEdge>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Generating a campaign map failed (no edges in the voronoi graph). Width: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Width);
				defaultInterpolatedStringHandler.AppendLiteral(", height: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Height);
				defaultInterpolatedStringHandler.AppendLiteral(", margin: ");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(margin);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			voronoiSites.Clear();
			using (List<GraphEdge>.Enumerator enumerator = edges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Map.<>c__DisplayClass62_1 CS$<>8__locals2 = new Map.<>c__DisplayClass62_1();
					CS$<>8__locals2.edge = enumerator.Current;
					if (!(CS$<>8__locals2.edge.Point1 == CS$<>8__locals2.edge.Point2) && CS$<>8__locals2.edge.Point1.X >= margin.X && CS$<>8__locals2.edge.Point1.X <= (float)this.Width - margin.X && CS$<>8__locals2.edge.Point1.Y >= startY && CS$<>8__locals2.edge.Point1.Y <= endY && CS$<>8__locals2.edge.Point2.X >= margin.X && CS$<>8__locals2.edge.Point2.X <= (float)this.Width - margin.X && CS$<>8__locals2.edge.Point2.Y >= startY && CS$<>8__locals2.edge.Point2.Y <= endY)
					{
						Location[] newLocations = new Location[2];
						newLocations[0] = this.Locations.Find((Location l) => l.MapPosition == CS$<>8__locals2.edge.Point1 || l.MapPosition == CS$<>8__locals2.edge.Point2);
						newLocations[1] = this.Locations.Find((Location l) => l != newLocations[0] && (l.MapPosition == CS$<>8__locals2.edge.Point1 || l.MapPosition == CS$<>8__locals2.edge.Point2));
						for (int i4 = 0; i4 < 2; i4++)
						{
							if (newLocations[i4] == null)
							{
								Vector2[] points = new Vector2[]
								{
									CS$<>8__locals2.edge.Point1,
									CS$<>8__locals2.edge.Point2
								};
								int positionIndex = Rand.Int(1, Rand.RandSync.ServerAndClient);
								Vector2 position = points[positionIndex];
								if (newLocations[1 - i4] != null && newLocations[1 - i4].MapPosition == position)
								{
									position = points[1 - positionIndex];
								}
								int zone = this.GetZoneIndex(position.X);
								Location[] newLocations2 = newLocations;
								int num = i4;
								Vector2 position2 = position;
								int? zone6 = new int?(zone);
								Biome biome = this.GetBiome(position.X);
								newLocations2[num] = Location.CreateRandom(position2, zone6, (biome != null) ? new Identifier?(biome.Identifier) : null, Rand.GetRNG(Rand.RandSync.ServerAndClient), false, null, this.Locations);
								mapLocationTypeGenerator.AddToLocationsPerZone(zone, newLocations[i4]);
								this.Locations.Add(newLocations[i4]);
							}
						}
						LocationConnection newConnection = new LocationConnection(newLocations[0], newLocations[1]);
						this.Connections.Add(newConnection);
					}
				}
			}
			float minConnectionDistanceSqr = this.generationParams.MinConnectionDistance * this.generationParams.MinConnectionDistance;
			for (int j2 = this.Connections.Count - 1; j2 >= 0; j2--)
			{
				LocationConnection connection9 = this.Connections[j2];
				if (Vector2.DistanceSquared(connection9.Locations[0].MapPosition, connection9.Locations[1].MapPosition) <= minConnectionDistanceSqr)
				{
					this.Connections.Remove(connection9);
					foreach (LocationConnection connection2 in this.Connections)
					{
						if (connection2.Locations[0] == connection9.Locations[0])
						{
							connection2.Locations[0] = connection9.Locations[1];
						}
						if (connection2.Locations[1] == connection9.Locations[0])
						{
							connection2.Locations[1] = connection9.Locations[1];
						}
					}
				}
			}
			foreach (LocationConnection connection3 in this.Connections)
			{
				connection3.Locations[0].Connections.Add(connection3);
				connection3.Locations[1].Connections.Add(connection3);
			}
			float minLocationDistanceSqr = this.generationParams.MinLocationDistance * this.generationParams.MinLocationDistance;
			int i;
			int num2;
			for (i = this.Locations.Count - 1; i >= 0; i = num2 - 1)
			{
				int j;
				for (j = this.Locations.Count - 1; j > i; j = num2 - 1)
				{
					float dist = Vector2.DistanceSquared(this.Locations[i].MapPosition, this.Locations[j].MapPosition);
					if (dist <= minLocationDistanceSqr)
					{
						foreach (LocationConnection connection4 in this.Locations[j].Connections)
						{
							if (connection4.Locations[0] == this.Locations[j])
							{
								connection4.Locations[0] = this.Locations[i];
							}
							else
							{
								connection4.Locations[1] = this.Locations[i];
							}
							if (connection4.Locations[0] != connection4.Locations[1])
							{
								this.Locations[i].Connections.Add(connection4);
							}
							else
							{
								this.Connections.Remove(connection4);
							}
						}
						this.Locations[i].Connections.RemoveAll((LocationConnection c) => c.OtherLocation(CS$<>8__locals1.<>4__this.Locations[i]) == CS$<>8__locals1.<>4__this.Locations[j]);
						this.Locations.RemoveAt(j);
					}
					num2 = j;
				}
				num2 = i;
			}
			foreach (Location location in this.Locations)
			{
				List<LocationConnection> connections = location.Connections;
				Comparison<LocationConnection> comparison;
				if ((comparison = CS$<>8__locals1.<>9__9) == null)
				{
					comparison = (CS$<>8__locals1.<>9__9 = ((LocationConnection c1, LocationConnection c2) => CS$<>8__locals1.<>4__this.Connections.IndexOf(c1).CompareTo(CS$<>8__locals1.<>4__this.Connections.IndexOf(c2))));
				}
				connections.Sort(comparison);
			}
			for (int k = this.Connections.Count - 1; k >= 0; k--)
			{
				k = Math.Min(k, this.Connections.Count - 1);
				LocationConnection connection5 = this.Connections[k];
				for (int l2 = Math.Min(k - 1, this.Connections.Count - 1); l2 >= 0; l2--)
				{
					if (connection5.Locations.Contains(this.Connections[l2].Locations[0]) && connection5.Locations.Contains(this.Connections[l2].Locations[1]))
					{
						this.Connections.RemoveAt(l2);
					}
				}
			}
			List<LocationConnection>[] connectionsBetweenZones = new List<LocationConnection>[this.generationParams.DifficultyZones];
			for (int m = 0; m < this.generationParams.DifficultyZones; m++)
			{
				connectionsBetweenZones[m] = new List<LocationConnection>();
			}
			List<LocationConnection> shuffledConnections = this.Connections.ToList<LocationConnection>();
			shuffledConnections.Shuffle(Rand.RandSync.ServerAndClient);
			using (List<LocationConnection>.Enumerator enumerator6 = shuffledConnections.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					LocationConnection connection = enumerator6.Current;
					int zone2 = this.GetZoneIndex(connection.Locations[0].MapPosition.X);
					int zone3 = this.GetZoneIndex(connection.Locations[1].MapPosition.X);
					if (zone2 != zone3)
					{
						if (zone2 > zone3)
						{
							int num3 = zone3;
							zone3 = zone2;
							zone2 = num3;
						}
						if (this.generationParams.GateCount[zone2] != 0)
						{
							if (!connectionsBetweenZones[zone2].Any<LocationConnection>())
							{
								connectionsBetweenZones[zone2].Add(connection);
							}
							else if (this.generationParams.GateCount[zone2] == 1)
							{
								if (Math.Abs(connection.CenterPos.Y - (float)(this.Height / 2)) < Math.Abs(connectionsBetweenZones[zone2].First<LocationConnection>().CenterPos.Y - (float)(this.Height / 2)))
								{
									connectionsBetweenZones[zone2].Clear();
									connectionsBetweenZones[zone2].Add(connection);
								}
							}
							else if (connectionsBetweenZones[zone2].Count<LocationConnection>() < this.generationParams.GateCount[zone2] && connectionsBetweenZones[zone2].None((LocationConnection c) => c.Locations.Contains(connection.Locations[0]) || c.Locations.Contains(connection.Locations[1])))
							{
								connectionsBetweenZones[zone2].Add(connection);
							}
							if (connectionsBetweenZones[zone2].None(null))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(140, 2);
								defaultInterpolatedStringHandler2.AppendLiteral("Potential error during map generation: no connections between zones ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(zone2);
								defaultInterpolatedStringHandler2.AppendLiteral(" and ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(zone3);
								defaultInterpolatedStringHandler2.AppendLiteral(" found. Traversing through to the end of the map may be impossible.");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							}
						}
					}
				}
			}
			IOrderedEnumerable<LocationType> orderedPrefabs = LocationType.Prefabs.GetOrdered();
			List<Location> forciblyReassignedGateLocations = new List<Location>();
			List<Faction> gateFactions = (from f in CS$<>8__locals1.campaign.Factions
			where f.Prefab.ControlledOutpostPercentage > 0f
			orderby f.Prefab.Identifier
			select f).ToList<Faction>();
			for (int n = this.Connections.Count - 1; n >= 0; n--)
			{
				int zone4 = this.GetZoneIndex(this.Connections[n].Locations[0].MapPosition.X);
				int zone5 = this.GetZoneIndex(this.Connections[n].Locations[1].MapPosition.X);
				if (zone4 != zone5 && zone4 != this.generationParams.DifficultyZones && zone5 != this.generationParams.DifficultyZones)
				{
					int leftZone = Math.Min(zone4, zone5);
					if (this.generationParams.GateCount[leftZone] != 0)
					{
						if (!connectionsBetweenZones[leftZone].Contains(this.Connections[n]))
						{
							this.Connections.RemoveAt(n);
						}
						else
						{
							Location leftMostLocation = (this.Connections[n].Locations[0].MapPosition.X < this.Connections[n].Locations[1].MapPosition.X) ? this.Connections[n].Locations[0] : this.Connections[n].Locations[1];
							if (!Map.<Generate>g__AllowAsBiomeGate|62_11(leftMostLocation.Type))
							{
								IEnumerable<LocationType> source = orderedPrefabs;
								Func<LocationType, bool> predicate;
								if ((predicate = Map.<>O.<0>__AllowAsBiomeGate) == null)
								{
									predicate = (Map.<>O.<0>__AllowAsBiomeGate = new Func<LocationType, bool>(Map.<Generate>g__AllowAsBiomeGate|62_11));
								}
								IEnumerable<LocationType> potentialGateLocationTypes = source.Where(predicate);
								LocationType random;
								Func<LocationType.AreaSettingData, bool> <>9__15;
								Func<LocationType.AreaSettingData, bool> <>9__16;
								if ((random = potentialGateLocationTypes.Where(delegate(LocationType lt)
								{
									IEnumerable<LocationType.AreaSettingData> areaSettings2 = lt.AreaSettings;
									Func<LocationType.AreaSettingData, bool> predicate2;
									if ((predicate2 = <>9__15) == null)
									{
										predicate2 = (<>9__15 = ((LocationType.AreaSettingData areaSettings) => areaSettings.MatchesLocation(CS$<>8__locals1.<>4__this, leftMostLocation) && areaSettings.Commonness > 0f));
									}
									return areaSettings2.Any(predicate2);
								}).GetRandom(Rand.RandSync.ServerAndClient)) == null && (random = potentialGateLocationTypes.Where(delegate(LocationType lt)
								{
									IEnumerable<LocationType.AreaSettingData> areaSettings2 = lt.AreaSettings;
									Func<LocationType.AreaSettingData, bool> predicate2;
									if ((predicate2 = <>9__16) == null)
									{
										predicate2 = (<>9__16 = delegate(LocationType.AreaSettingData areaSettings)
										{
											if (areaSettings.MatchesLocation(CS$<>8__locals1.<>4__this, leftMostLocation))
											{
												int? minCount = areaSettings.MinCount;
												int num4 = 0;
												return minCount.GetValueOrDefault() > num4 & minCount != null;
											}
											return false;
										});
									}
									return areaSettings2.Any(predicate2);
								}).GetRandom(Rand.RandSync.ServerAndClient)) == null)
								{
									random = (from lt in potentialGateLocationTypes
									where lt.AreaSettings.None((LocationType.AreaSettingData areaSettings) => areaSettings.Commonness > 0f || areaSettings.HasCounts)
									select lt).GetRandom(Rand.RandSync.ServerAndClient);
								}
								LocationType gateLocationType = random;
								if (gateLocationType == null)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(80, 2);
									defaultInterpolatedStringHandler3.AppendLiteral("Failed to find a suitable location type for a gate location between zones ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(zone4);
									defaultInterpolatedStringHandler3.AppendLiteral(" and ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(zone5);
									defaultInterpolatedStringHandler3.AppendLiteral(".");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
									goto IL_F59;
								}
								leftMostLocation.ChangeType(CS$<>8__locals1.campaign, gateLocationType, false, false);
								forciblyReassignedGateLocations.Add(leftMostLocation);
							}
							leftMostLocation.IsGateBetweenBiomes = true;
							this.Connections[n].Locked = true;
							if (leftMostLocation.Type.HasOutpost && CS$<>8__locals1.campaign != null && gateFactions.Any<Faction>())
							{
								leftMostLocation.Faction = gateFactions[connectionsBetweenZones[leftZone].IndexOf(this.Connections[n]) % gateFactions.Count];
							}
						}
					}
				}
				IL_F59:;
			}
			foreach (Location location2 in this.Locations)
			{
				for (int i2 = location2.Connections.Count - 1; i2 >= 0; i2--)
				{
					if (!this.Connections.Contains(location2.Connections[i2]))
					{
						location2.Connections.RemoveAt(i2);
					}
				}
			}
			for (int i3 = 0; i3 < this.Connections.Count; i3++)
			{
				LocationConnection connection6 = this.Connections[i3];
				if (connection6.Locked)
				{
					Location rightMostLocation = (connection6.Locations[0].MapPosition.X > connection6.Locations[1].MapPosition.X) ? connection6.Locations[0] : connection6.Locations[1];
					if (rightMostLocation.Connections.All((LocationConnection c) => c.OtherLocation(rightMostLocation).MapPosition.X < rightMostLocation.MapPosition.X))
					{
						Location closestLocation = null;
						float closestDist = float.PositiveInfinity;
						foreach (Location otherLocation in this.Locations)
						{
							if (otherLocation != rightMostLocation && otherLocation.MapPosition.X >= rightMostLocation.MapPosition.X)
							{
								float dist2 = Vector2.DistanceSquared(rightMostLocation.MapPosition, otherLocation.MapPosition);
								if (dist2 < closestDist || closestLocation == null)
								{
									closestLocation = otherLocation;
									closestDist = dist2;
								}
							}
						}
						LocationConnection newConnection2 = new LocationConnection(rightMostLocation, closestLocation);
						rightMostLocation.Connections.Add(newConnection2);
						closestLocation.Connections.Add(newConnection2);
						this.Connections.Add(newConnection2);
					}
				}
			}
			this.Locations.RemoveAll((Location l) => !CS$<>8__locals1.<>4__this.Connections.Any((LocationConnection c) => c.Locations.Contains(l)));
			this.AssignBiomes(Rand.GetRNG(Rand.RandSync.ServerAndClient));
			IEnumerable<Location> gateLocations = from l in this.Locations
			where l.IsGateBetweenBiomes
			select l;
			foreach (Location gateLocation in forciblyReassignedGateLocations)
			{
				mapLocationTypeGenerator.RemoveOneFromTotals(gateLocation.Type, gateLocation);
			}
			mapLocationTypeGenerator.AssignForcedBiomeGateTypes(gateLocations);
			foreach (LocationConnection connection7 in this.Connections)
			{
				if (connection7.Locations.Any((Location l) => l.IsGateBetweenBiomes))
				{
					connection7.Difficulty = Math.Min(connection7.Locations.Min((Location l) => l.Biome.ActualMaxDifficulty), connection7.Biome.AdjustedMaxDifficulty);
				}
				else
				{
					connection7.Difficulty = CS$<>8__locals1.<Generate>g__CalculateDifficulty|5(connection7.CenterPos.X, connection7.Biome);
				}
			}
			Location startLocation = this.Locations.MinBy((Location l) => l.MapPosition.X);
			LocationType startLocationType;
			if (LocationType.Prefabs.TryGet("outpost", out startLocationType))
			{
				mapLocationTypeGenerator.ChangeLocationTypeAndName(CS$<>8__locals1.campaign, startLocation, startLocationType);
				mapLocationTypeGenerator.AddToFilled(startLocation);
			}
			mapLocationTypeGenerator.AssignLocationTypesBasedOnDesiredPosition(gateLocations);
			foreach (Location location3 in this.Locations)
			{
				location3.LevelData = new LevelData(location3, this, CS$<>8__locals1.<Generate>g__CalculateDifficulty|5(location3.MapPosition.X, location3.Biome));
				location3.TryAssignFactionBasedOnLocationType(CS$<>8__locals1.campaign);
				if (location3.Type.HasOutpost && CS$<>8__locals1.campaign != null && location3.Type.OutpostTeam == CharacterTeamType.FriendlyNPC)
				{
					if (location3.Type.Faction.IsEmpty)
					{
						Location location4 = location3;
						if (location4.Faction == null)
						{
							location4.Faction = CS$<>8__locals1.campaign.GetRandomFaction(Rand.RandSync.ServerAndClient, true);
						}
					}
					if (location3.Type.SecondaryFaction.IsEmpty)
					{
						Location location4 = location3;
						if (location4.SecondaryFaction == null)
						{
							location4.SecondaryFaction = CS$<>8__locals1.campaign.GetRandomSecondaryFaction(Rand.RandSync.ServerAndClient, true);
						}
					}
				}
			}
			foreach (Location gateLocation2 in forciblyReassignedGateLocations)
			{
				gateLocation2.UnlockInitialMissions(Rand.RandSync.ServerAndClient);
			}
			List<Location> locationsToAssign = this.Locations.ToList<Location>();
			locationsToAssign.Remove(this.GetPreviousToEndLocation());
			mapLocationTypeGenerator.AssignLocationTypesBasedOnCount(gateLocations, locationsToAssign);
			foreach (LocationConnection connection8 in this.Connections)
			{
				connection8.LevelData = new LevelData(connection8);
			}
			this.CreateEndLocation(CS$<>8__locals1.campaign);
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x0010F384 File Offset: 0x0010D584
		public int GetZoneIndex(float xPos)
		{
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			return MathHelper.Clamp((int)Math.Floor((double)(xPos / zoneWidth)) + 1, 1, this.generationParams.DifficultyZones);
		}

		// Token: 0x06002978 RID: 10616 RVA: 0x0010F3C2 File Offset: 0x0010D5C2
		public Biome GetBiome(Vector2 mapPos)
		{
			return this.GetBiome(mapPos.X);
		}

		// Token: 0x06002979 RID: 10617 RVA: 0x0010F3D0 File Offset: 0x0010D5D0
		public Biome GetBiome(float xPos)
		{
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			int zoneIndex = (int)Math.Floor((double)(xPos / zoneWidth)) + 1;
			zoneIndex = Math.Clamp(zoneIndex, 1, this.generationParams.DifficultyZones - 1);
			return this.OrderedBiomes.FirstOrDefault((Biome b) => b.AllowedZones.Contains(zoneIndex));
		}

		// Token: 0x0600297A RID: 10618 RVA: 0x0010F440 File Offset: 0x0010D640
		private void AssignBiomes(Random rand)
		{
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			List<Biome> allowedBiomes = new List<Biome>(10);
			for (int i = 0; i < this.generationParams.DifficultyZones; i++)
			{
				int zoneIndex = i + 1;
				allowedBiomes.Clear();
				allowedBiomes.AddRange(from b in this.OrderedBiomes
				where b.AllowedZones.Contains(zoneIndex)
				select b);
				float zoneX = zoneWidth * (float)zoneIndex;
				foreach (Location location in this.Locations)
				{
					if (location.Biome == null && location.MapPosition.X < zoneX)
					{
						location.Biome = allowedBiomes[rand.Next() % allowedBiomes.Count];
					}
				}
			}
			foreach (LocationConnection connection in this.Connections)
			{
				if (connection.Biome == null)
				{
					connection.Biome = ((connection.Locations[0].MapPosition.X > connection.Locations[1].MapPosition.X) ? connection.Locations[0].Biome : connection.Locations[1].Biome);
				}
			}
		}

		// Token: 0x0600297B RID: 10619 RVA: 0x0010F5CC File Offset: 0x0010D7CC
		private Location GetPreviousToEndLocation()
		{
			Location previousToEndLocation = null;
			foreach (Location location in this.Locations)
			{
				if (!location.Biome.IsEndBiome && (previousToEndLocation == null || location.MapPosition.X > previousToEndLocation.MapPosition.X))
				{
					previousToEndLocation = location;
				}
			}
			return previousToEndLocation;
		}

		// Token: 0x0600297C RID: 10620 RVA: 0x0010F648 File Offset: 0x0010D848
		private void ForceLocationTypeToNone(CampaignMode campaign, Location location)
		{
			LocationType locationType;
			if (LocationType.Prefabs.TryGet("none", out locationType))
			{
				location.ChangeType(campaign, locationType, false, true);
			}
			location.DisallowLocationTypeChanges = true;
		}

		// Token: 0x0600297D RID: 10621 RVA: 0x0010F67C File Offset: 0x0010D87C
		private void CreateEndLocation(CampaignMode campaign)
		{
			Map.<>c__DisplayClass71_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			Vector2 endPos = new Vector2((float)this.Width - zoneWidth * 0.7f, (float)(this.Height / 2));
			float closestDist = float.MaxValue;
			CS$<>8__locals1.endLocation = this.Locations.First<Location>();
			foreach (Location location in this.Locations)
			{
				float dist = Vector2.DistanceSquared(endPos, location.MapPosition);
				if (location.Biome.IsEndBiome && dist < closestDist)
				{
					CS$<>8__locals1.endLocation = location;
					closestDist = dist;
				}
			}
			Location previousToEndLocation = this.GetPreviousToEndLocation();
			if (CS$<>8__locals1.endLocation == null || previousToEndLocation == null)
			{
				return;
			}
			this.endLocations = new List<Location>
			{
				CS$<>8__locals1.endLocation
			};
			if (CS$<>8__locals1.endLocation.Biome.EndBiomeLocationCount > 1)
			{
				this.<CreateEndLocation>g__FindConnectedEndLocations|71_0(CS$<>8__locals1.endLocation, ref CS$<>8__locals1);
			}
			this.ForceLocationTypeToNone(campaign, previousToEndLocation);
			for (int i = this.Locations.Count - 1; i >= 0; i--)
			{
				if (this.Locations[i].Biome.IsEndBiome)
				{
					for (int j = this.Locations[i].Connections.Count - 1; j >= 0; j--)
					{
						if (j < this.Locations[i].Connections.Count)
						{
							LocationConnection connection = this.Locations[i].Connections[j];
							Location otherLocation = connection.OtherLocation(this.Locations[i]);
							this.Locations[i].Connections.RemoveAt(j);
							if (otherLocation != null)
							{
								otherLocation.Connections.Remove(connection);
							}
							this.Connections.Remove(connection);
						}
					}
					if (!this.endLocations.Contains(this.Locations[i]))
					{
						this.Locations.RemoveAt(i);
					}
				}
			}
			if (previousToEndLocation.Connections.None(null))
			{
				Location connectTo = this.Locations.First<Location>();
				foreach (Location location2 in this.Locations)
				{
					if (!location2.Biome.IsEndBiome && location2 != previousToEndLocation && location2.MapPosition.X > connectTo.MapPosition.X)
					{
						connectTo = location2;
					}
				}
				LocationConnection newConnection = new LocationConnection(previousToEndLocation, connectTo)
				{
					Biome = CS$<>8__locals1.endLocation.Biome,
					Difficulty = 100f
				};
				newConnection.LevelData = new LevelData(newConnection);
				this.Connections.Add(newConnection);
				previousToEndLocation.Connections.Add(newConnection);
				connectTo.Connections.Add(newConnection);
			}
			LocationConnection endConnection = new LocationConnection(previousToEndLocation, CS$<>8__locals1.endLocation)
			{
				Biome = CS$<>8__locals1.endLocation.Biome,
				Difficulty = 100f
			};
			endConnection.LevelData = new LevelData(endConnection);
			this.Connections.Add(endConnection);
			previousToEndLocation.Connections.Add(endConnection);
			CS$<>8__locals1.endLocation.Connections.Add(endConnection);
			this.AssignEndLocationLevelData(campaign);
		}

		// Token: 0x0600297E RID: 10622 RVA: 0x0010FA14 File Offset: 0x0010DC14
		private void AssignEndLocationLevelData(CampaignMode campaign)
		{
			Map.<>c__DisplayClass72_0 CS$<>8__locals1 = new Map.<>c__DisplayClass72_0();
			CS$<>8__locals1.<>4__this = this;
			Map.<>c__DisplayClass72_0 CS$<>8__locals2 = CS$<>8__locals1;
			Biome biome = (from p in Biome.Prefabs
			orderby p.UintIdentifier
			select p).FirstOrDefault((Biome b) => b.IsEndBiome);
			if (biome == null)
			{
				throw new InvalidOperationException("Could not find an end biome to assign to the end locations.");
			}
			CS$<>8__locals2.endBiome = biome;
			LocationType locationType = (from p in LocationType.Prefabs
			orderby p.UintIdentifier
			select p).FirstOrDefault(new Func<LocationType, bool>(CS$<>8__locals1.<AssignEndLocationLevelData>g__IsSuitableEndLocationType|3));
			if (locationType == null)
			{
				throw new InvalidOperationException("Could not find an a location type to assign to the end locations.");
			}
			LocationType endLocationType = locationType;
			int i;
			int j;
			for (i = 0; i < this.endLocations.Count; i = j + 1)
			{
				if (this.endLocations[i].Biome != CS$<>8__locals1.endBiome)
				{
					this.endLocations[i].Biome = CS$<>8__locals1.endBiome;
					this.endLocations[i].LevelData = new LevelData(this.endLocations[i], this, this.endLocations[i].LevelData.Difficulty);
				}
				this.endLocations[i].ChangeType(campaign, endLocationType, true, true);
				if (!endLocationType.ForceLocationName.IsEmpty)
				{
					this.endLocations[i].ForceName(endLocationType.ForceLocationName);
				}
				this.endLocations[i].LevelData.ReassignGenerationParams(this.Seed);
				OutpostGenerationParams outpostParams = OutpostGenerationParams.OutpostParams.FirstOrDefault((OutpostGenerationParams p) => p.ForceToEndLocationIndex == i);
				if (outpostParams != null)
				{
					this.endLocations[i].LevelData.ForceOutpostGenerationParams = outpostParams;
				}
				j = i;
			}
		}

		// Token: 0x0600297F RID: 10623 RVA: 0x0010FC34 File Offset: 0x0010DE34
		private void ExpandBiomes(List<LocationConnection> seeds)
		{
			List<LocationConnection> nextSeeds = new List<LocationConnection>();
			foreach (LocationConnection connection in seeds)
			{
				foreach (Location location in connection.Locations)
				{
					foreach (LocationConnection otherConnection in location.Connections)
					{
						if (otherConnection != connection && otherConnection.Biome == null)
						{
							otherConnection.Biome = connection.Biome;
							nextSeeds.Add(otherConnection);
						}
					}
				}
			}
			if (nextSeeds.Count > 0)
			{
				this.ExpandBiomes(nextSeeds);
			}
		}

		// Token: 0x06002980 RID: 10624 RVA: 0x0010FD14 File Offset: 0x0010DF14
		public void MoveToNextLocation()
		{
			if (this.SelectedLocation == null)
			{
				Level loaded = Level.Loaded;
				if (((loaded != null) ? loaded.EndLocation : null) != null)
				{
					this.SelectLocation(Level.Loaded.EndLocation);
				}
			}
			if (this.SelectedConnection == null && !this.endLocations.Contains(this.CurrentLocation))
			{
				DebugConsole.ThrowError("Could not move to the next location (no connection selected).\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (this.SelectedLocation == null)
			{
				if (!this.endLocations.Contains(this.CurrentLocation))
				{
					DebugConsole.ThrowError("Could not move to the next location (no connection selected).\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
					return;
				}
				int currentEndLocationIndex = this.endLocations.IndexOf(this.CurrentLocation);
				if (currentEndLocationIndex < this.endLocations.Count - 1)
				{
					this.SelectedLocation = this.endLocations[currentEndLocationIndex + 1];
				}
				else
				{
					this.SelectedLocation = this.StartLocation;
				}
			}
			Location prevLocation = this.CurrentLocation;
			if (this.SelectedConnection != null)
			{
				this.SelectedConnection.Passed = true;
			}
			this.CurrentLocation = this.SelectedLocation;
			this.CurrentLocation.CreateStores(false);
			this.Discover(this.CurrentLocation, true);
			this.Visit(this.CurrentLocation, true);
			this.SelectedLocation = null;
			NamedEvent<Map.LocationChangeInfo> onLocationChanged = this.OnLocationChanged;
			if (onLocationChanged != null)
			{
				onLocationChanged.Invoke(new Map.LocationChangeInfo(prevLocation, this.CurrentLocation));
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign != null)
				{
					CampaignMetadata metadata = campaign.CampaignMetadata;
					if (metadata != null)
					{
						metadata.SetValue("campaign.location.id".ToIdentifier(), this.CurrentLocationIndex);
						metadata.SetValue("campaign.location.name".ToIdentifier(), this.CurrentLocation.NameIdentifier.Value);
						CampaignMetadata campaignMetadata = metadata;
						Identifier identifier = "campaign.location.biome".ToIdentifier();
						Biome biome = this.CurrentLocation.Biome;
						campaignMetadata.SetValue(identifier, (biome != null) ? biome.Identifier : "null".ToIdentifier());
						CampaignMetadata campaignMetadata2 = metadata;
						Identifier identifier2 = "campaign.location.type".ToIdentifier();
						LocationType type = this.CurrentLocation.Type;
						campaignMetadata2.SetValue(identifier2, (type != null) ? type.Identifier : "null".ToIdentifier());
					}
				}
			}
		}

		// Token: 0x06002981 RID: 10625 RVA: 0x0010FF48 File Offset: 0x0010E148
		public void SetLocation(int index)
		{
			if (index == -1)
			{
				this.CurrentLocation = null;
				return;
			}
			if (index < 0 || index >= this.Locations.Count)
			{
				DebugConsole.ThrowError("Location index out of bounds", null, null, false, false);
				return;
			}
			Location prevLocation = this.CurrentLocation;
			this.CurrentLocation = this.Locations[index];
			this.Discover(this.CurrentLocation, true);
			this.CurrentLocation.CreateStores(false);
			if (prevLocation != this.CurrentLocation)
			{
				LocationConnection connection = this.CurrentLocation.Connections.Find((LocationConnection c) => c.Locations.Contains(prevLocation));
				if (connection != null)
				{
					connection.Passed = true;
				}
				NamedEvent<Map.LocationChangeInfo> onLocationChanged = this.OnLocationChanged;
				if (onLocationChanged == null)
				{
					return;
				}
				onLocationChanged.Invoke(new Map.LocationChangeInfo(prevLocation, this.CurrentLocation));
			}
		}

		// Token: 0x06002982 RID: 10626 RVA: 0x00110018 File Offset: 0x0010E218
		public void SelectLocation(int index)
		{
			Map.<>c__DisplayClass76_0 CS$<>8__locals1 = new Map.<>c__DisplayClass76_0();
			CS$<>8__locals1.<>4__this = this;
			if (index == -1)
			{
				this.SelectedLocation = null;
				this.SelectedConnection = null;
				Action<Location, LocationConnection> onLocationSelected = this.OnLocationSelected;
				if (onLocationSelected == null)
				{
					return;
				}
				onLocationSelected(null, null);
				return;
			}
			else
			{
				if (index < 0 || index >= this.Locations.Count)
				{
					DebugConsole.ThrowError("Location index out of bounds", null, null, false, false);
					return;
				}
				Location prevSelected = this.SelectedLocation;
				this.SelectedLocation = this.Locations[index];
				Map.<>c__DisplayClass76_0 CS$<>8__locals2 = CS$<>8__locals1;
				GameSession gameSession = GameMain.GameSession;
				Location currentDisplayLocation;
				if (gameSession == null)
				{
					currentDisplayLocation = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					currentDisplayLocation = ((campaign != null) ? campaign.GetCurrentDisplayLocation() : null);
				}
				CS$<>8__locals2.currentDisplayLocation = currentDisplayLocation;
				if (CS$<>8__locals1.currentDisplayLocation == this.SelectedLocation)
				{
					this.SelectedConnection = this.Connections.Find((LocationConnection c) => c.Locations.Contains(CS$<>8__locals1.<>4__this.CurrentLocation) && c.Locations.Contains(CS$<>8__locals1.<>4__this.SelectedLocation));
				}
				else
				{
					this.SelectedConnection = (this.Connections.Find((LocationConnection c) => c.Locations.Contains(CS$<>8__locals1.currentDisplayLocation) && c.Locations.Contains(CS$<>8__locals1.<>4__this.SelectedLocation)) ?? this.Connections.Find((LocationConnection c) => c.Locations.Contains(CS$<>8__locals1.<>4__this.CurrentLocation) && c.Locations.Contains(CS$<>8__locals1.<>4__this.SelectedLocation)));
				}
				LocationConnection selectedConnection = this.SelectedConnection;
				if (selectedConnection != null && selectedConnection.Locked)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 4);
					defaultInterpolatedStringHandler.AppendLiteral("A locked connection was selected (");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.SelectedConnection.Locations[0].DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral(" -> ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.SelectedConnection.Locations[1].DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendLiteral(" Current location: ");
					defaultInterpolatedStringHandler.AppendFormatted<Location>(this.CurrentLocation);
					defaultInterpolatedStringHandler.AppendLiteral(", current display location: ");
					defaultInterpolatedStringHandler.AppendFormatted<Location>(CS$<>8__locals1.currentDisplayLocation);
					defaultInterpolatedStringHandler.AppendLiteral(").\n");
					string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("MapSelectLocation:LockedConnectionSelected", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				if (prevSelected != this.SelectedLocation)
				{
					Action<Location, LocationConnection> onLocationSelected2 = this.OnLocationSelected;
					if (onLocationSelected2 == null)
					{
						return;
					}
					onLocationSelected2(this.SelectedLocation, this.SelectedConnection);
				}
				return;
			}
		}

		// Token: 0x06002983 RID: 10627 RVA: 0x00110220 File Offset: 0x0010E420
		public void SelectLocation(Location location)
		{
			if (!this.Locations.Contains(location))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to select a location. ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(((location != null) ? location.DisplayName : null) ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral(" not found in the map.");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Map.SelectLocation:LocationNotFound", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			Location prevSelected = this.SelectedLocation;
			this.SelectedLocation = location;
			this.SelectedConnection = this.Connections.Find((LocationConnection c) => c.Locations.Contains(this.CurrentLocation) && c.Locations.Contains(this.SelectedLocation));
			LocationConnection selectedConnection = this.SelectedConnection;
			if (selectedConnection != null && selectedConnection.Locked)
			{
				DebugConsole.ThrowError("A locked connection was selected - this should not be possible.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
			}
			if (prevSelected != this.SelectedLocation)
			{
				Action<Location, LocationConnection> onLocationSelected = this.OnLocationSelected;
				if (onLocationSelected == null)
				{
					return;
				}
				onLocationSelected(this.SelectedLocation, this.SelectedConnection);
			}
		}

		// Token: 0x06002984 RID: 10628 RVA: 0x00110320 File Offset: 0x0010E520
		public void SelectMission(IEnumerable<int> missionIndices)
		{
			if (this.CurrentLocation == null)
			{
				string errorMsg = "Failed to select a mission (current location not set).";
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Map.SelectMission:CurrentLocationNotSet", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (!missionIndices.SequenceEqual(this.GetSelectedMissionIndices()))
			{
				this.CurrentLocation.SetSelectedMissionIndices(missionIndices);
				foreach (Mission selectedMission in this.CurrentLocation.SelectedMissions.ToList<Mission>())
				{
					if (selectedMission.Locations[0] != this.CurrentLocation || selectedMission.Locations[1] != this.CurrentLocation)
					{
						if (this.SelectedConnection == null)
						{
							return;
						}
						if (selectedMission.Locations[1] != this.SelectedLocation)
						{
							this.CurrentLocation.DeselectMission(selectedMission);
						}
					}
				}
				Action<LocationConnection, IEnumerable<Mission>> onMissionsSelected = this.OnMissionsSelected;
				if (onMissionsSelected == null)
				{
					return;
				}
				onMissionsSelected(this.SelectedConnection, this.CurrentLocation.SelectedMissions);
			}
		}

		// Token: 0x06002985 RID: 10629 RVA: 0x00110420 File Offset: 0x0010E620
		public void SelectRandomLocation(bool preferUndiscovered)
		{
			List<Location> nextLocations = (from c in this.CurrentLocation.Connections
			where !c.Locked
			select c.OtherLocation(this.CurrentLocation)).ToList<Location>();
			List<Location> undiscoveredLocations = nextLocations.FindAll((Location l) => !l.Discovered);
			if (undiscoveredLocations.Count > 0 && preferUndiscovered)
			{
				this.SelectLocation(undiscoveredLocations[Rand.Int(undiscoveredLocations.Count, Rand.RandSync.Unsynced)]);
				return;
			}
			this.SelectLocation(nextLocations[Rand.Int(nextLocations.Count, Rand.RandSync.Unsynced)]);
		}

		// Token: 0x06002986 RID: 10630 RVA: 0x001104D8 File Offset: 0x0010E6D8
		public void ProgressWorld(CampaignMode campaign, CampaignMode.TransitionType transitionType, float roundDuration)
		{
			int steps = (int)Math.Floor((double)(roundDuration / 600f));
			if (transitionType == CampaignMode.TransitionType.ProgressToNextLocation || transitionType == CampaignMode.TransitionType.ProgressToNextEmptyLocation)
			{
				steps = Math.Max(1, steps);
			}
			steps = Math.Min(steps, 5);
			for (int i = 0; i < steps; i++)
			{
				this.ProgressWorld(campaign);
			}
			for (int j = 0; j < Math.Max(1, steps); j++)
			{
				foreach (Location location in this.Locations)
				{
					if (location.Discovered)
					{
						location.UpdateSpecials();
					}
				}
			}
			Radiation radiation = this.Radiation;
			if (radiation == null)
			{
				return;
			}
			radiation.OnStep((float)steps);
		}

		// Token: 0x06002987 RID: 10631 RVA: 0x00110594 File Offset: 0x0010E794
		private void ProgressWorld(CampaignMode campaign)
		{
			foreach (Location location in this.Locations)
			{
				if (location.Visited)
				{
					location.WorldStepsSinceVisited++;
					if (location.WorldStepsSinceVisited > 10)
					{
						location.ClearStores();
					}
				}
				else
				{
					location.ClearStores();
				}
				location.LevelData.ResetExhaustedEventSets();
				if (location.Discovered && (this.furthestDiscoveredLocation == null || location.MapPosition.X > this.furthestDiscoveredLocation.MapPosition.X))
				{
					this.furthestDiscoveredLocation = location;
				}
			}
			foreach (LocationConnection connection in this.Connections)
			{
				connection.LevelData.ResetExhaustedEventSets();
			}
			foreach (Location location2 in this.Locations)
			{
				if (location2.MapPosition.X <= this.furthestDiscoveredLocation.MapPosition.X)
				{
					bool shouldUpdateStores = location2.Discovered;
					bool shouldProcessLocationTypeChanges = location2 != this.CurrentLocation && location2 != this.SelectedLocation && !location2.IsGateBetweenBiomes;
					if (shouldProcessLocationTypeChanges && this.ProgressLocationTypeChanges(campaign, location2))
					{
						shouldUpdateStores = false;
					}
					if (shouldUpdateStores)
					{
						location2.UpdateStores(false);
					}
				}
			}
			if (this.CurrentLocation != null)
			{
				this.CurrentLocation.UpdateStores(true);
				this.CurrentLocation.WorldStepsSinceVisited = 0;
			}
		}

		// Token: 0x06002988 RID: 10632 RVA: 0x00110758 File Offset: 0x0010E958
		private bool ProgressLocationTypeChanges(CampaignMode campaign, Location location)
		{
			location.TimeSinceLastTypeChange++;
			location.LocationTypeChangeCooldown--;
			if (location.PendingLocationTypeChange != null)
			{
				if (location.PendingLocationTypeChange.Value.Item1.DetermineProbability(location) <= 0f)
				{
					location.PendingLocationTypeChange = null;
				}
				else
				{
					location.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(location.PendingLocationTypeChange.Value.Item1, location.PendingLocationTypeChange.Value.Item2 - 1, location.PendingLocationTypeChange.Value.Item3));
					if (location.PendingLocationTypeChange.Value.Item2 <= 0)
					{
						return this.ChangeLocationType(campaign, location, location.PendingLocationTypeChange.Value.Item1);
					}
				}
			}
			Dictionary<LocationTypeChange, float> allowedTypeChanges = new Dictionary<LocationTypeChange, float>();
			foreach (LocationTypeChange typeChange in location.Type.CanChangeTo)
			{
				float probability = typeChange.DetermineProbability(location);
				if (probability > 0f)
				{
					allowedTypeChanges.Add(typeChange, probability);
				}
			}
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < allowedTypeChanges.Sum((KeyValuePair<LocationTypeChange, float> change) => change.Value))
			{
				LocationTypeChange selectedTypeChange = ToolBox.SelectWeightedRandom<LocationTypeChange>(allowedTypeChanges.Keys.ToList<LocationTypeChange>(), allowedTypeChanges.Values.ToList<float>(), Rand.RandSync.Unsynced);
				if (selectedTypeChange != null)
				{
					if (selectedTypeChange.RequiredDurationRange.X > 0)
					{
						location.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(selectedTypeChange, Rand.Range(selectedTypeChange.RequiredDurationRange.X, selectedTypeChange.RequiredDurationRange.Y, Rand.RandSync.Unsynced), null));
						return false;
					}
					return this.ChangeLocationType(campaign, location, selectedTypeChange);
				}
			}
			foreach (LocationTypeChange typeChange2 in location.Type.CanChangeTo)
			{
				foreach (LocationTypeChange.Requirement requirement in typeChange2.Requirements)
				{
					if (requirement.AnyWithinDistance(location, requirement.RequiredProximityForProbabilityIncrease))
					{
						if (!location.ProximityTimer.ContainsKey(requirement))
						{
							location.ProximityTimer[requirement] = 0;
						}
						Dictionary<LocationTypeChange.Requirement, int> proximityTimer = location.ProximityTimer;
						LocationTypeChange.Requirement key = requirement;
						proximityTimer[key]++;
					}
					else
					{
						location.ProximityTimer.Remove(requirement);
					}
				}
			}
			return false;
		}

		// Token: 0x06002989 RID: 10633 RVA: 0x00110A1C File Offset: 0x0010EC1C
		private bool ChangeLocationType(CampaignMode campaign, Location location, LocationTypeChange change)
		{
			LocalizedString prevName = location.DisplayName;
			LocationType newType;
			if (!LocationType.Prefabs.TryGet(change.ChangeToType, out newType))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to change the type of the location \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(location.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("\". Location type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(change.ChangeToType);
				defaultInterpolatedStringHandler.AppendLiteral("\" not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return false;
			}
			if (location.LocationTypeChangesBlocked)
			{
				return false;
			}
			if (newType.OutpostTeam != location.Type.OutpostTeam || newType.HasOutpost != location.Type.HasOutpost)
			{
				location.ClearMissions();
			}
			location.ChangeType(campaign, newType, false, true);
			foreach (LocationTypeChange.Requirement requirement in change.Requirements)
			{
				location.ProximityTimer.Remove(requirement);
			}
			location.TimeSinceLastTypeChange = 0;
			location.LocationTypeChangeCooldown = change.CooldownAfterChange;
			location.PendingLocationTypeChange = null;
			return true;
		}

		// Token: 0x0600298A RID: 10634 RVA: 0x00110B4C File Offset: 0x0010ED4C
		public static bool LocationOrConnectionWithinDistance(Location startLocation, int maxDistance, Func<Location, bool> criteria, Func<LocationConnection, bool> connectionCriteria = null)
		{
			return Map.GetDistanceToClosestLocationOrConnection(startLocation, maxDistance, criteria, connectionCriteria) <= maxDistance;
		}

		// Token: 0x0600298B RID: 10635 RVA: 0x00110B60 File Offset: 0x0010ED60
		public static int GetDistanceToClosestLocationOrConnection(Location startLocation, int maxDistance, Func<Location, bool> criteria, Func<LocationConnection, bool> connectionCriteria = null)
		{
			int distance = 0;
			List<Location> locationsToTest = new List<Location>
			{
				startLocation
			};
			HashSet<Location> nextBatchToTest = new HashSet<Location>();
			HashSet<Location> checkedLocations = new HashSet<Location>();
			while (locationsToTest.Any<Location>())
			{
				foreach (Location location in locationsToTest)
				{
					checkedLocations.Add(location);
					if (criteria(location))
					{
						return distance;
					}
					foreach (LocationConnection connection in location.Connections)
					{
						if (connectionCriteria != null && connectionCriteria(connection))
						{
							return distance;
						}
						Location otherLocation = connection.OtherLocation(location);
						if (!checkedLocations.Contains(otherLocation))
						{
							nextBatchToTest.Add(otherLocation);
						}
					}
					if (distance > maxDistance)
					{
						return int.MaxValue;
					}
				}
				distance++;
				locationsToTest.Clear();
				locationsToTest.AddRange(nextBatchToTest);
				nextBatchToTest.Clear();
			}
			return int.MaxValue;
		}

		// Token: 0x0600298C RID: 10636 RVA: 0x00110C94 File Offset: 0x0010EE94
		public void Discover(Location location, bool checkTalents = true)
		{
			if (location == null)
			{
				return;
			}
			if (this.locationsDiscovered.Contains(location))
			{
				return;
			}
			this.locationsDiscovered.Add(location);
			if (checkTalents)
			{
				GameSession.GetSessionCrewCharacters(CharacterType.Both).ForEach(delegate(Character c)
				{
					c.CheckTalents(AbilityEffectType.OnLocationDiscovered, new Location.AbilityLocation(location));
				});
			}
		}

		// Token: 0x0600298D RID: 10637 RVA: 0x00110CF6 File Offset: 0x0010EEF6
		public void Visit(Location location, bool resetTimeSinceVisited = true)
		{
			if (location == null)
			{
				return;
			}
			if (resetTimeSinceVisited)
			{
				location.WorldStepsSinceVisited = 0;
			}
			if (this.locationsVisited.Contains(location))
			{
				return;
			}
			this.locationsVisited.Add(location);
		}

		// Token: 0x0600298E RID: 10638 RVA: 0x00110D21 File Offset: 0x0010EF21
		public void ClearLocationHistory()
		{
			this.locationsDiscovered.Clear();
			this.locationsVisited.Clear();
		}

		// Token: 0x0600298F RID: 10639 RVA: 0x00110D3C File Offset: 0x0010EF3C
		public int? GetDiscoveryIndex(Location location)
		{
			if (!this.trackedLocationDiscoveryAndVisitOrder)
			{
				return null;
			}
			if (location == null)
			{
				return new int?(-1);
			}
			return new int?(this.locationsDiscovered.IndexOf(location));
		}

		// Token: 0x06002990 RID: 10640 RVA: 0x00110D78 File Offset: 0x0010EF78
		public int? GetVisitIndex(Location location, bool includeLocationsWithoutOutpost = false)
		{
			if (!this.trackedLocationDiscoveryAndVisitOrder)
			{
				return null;
			}
			if (location == null)
			{
				return new int?(-1);
			}
			int index = this.locationsVisited.IndexOf(location);
			if (includeLocationsWithoutOutpost)
			{
				return new int?(index);
			}
			int noOutpostLocations = 0;
			for (int i = 0; i < index; i++)
			{
				Location j = this.locationsVisited[i];
				if (j != null && !j.HasOutpost())
				{
					noOutpostLocations++;
				}
			}
			return new int?(index - noOutpostLocations);
		}

		// Token: 0x06002991 RID: 10641 RVA: 0x00110DED File Offset: 0x0010EFED
		public bool IsDiscovered(Location location)
		{
			return location != null && this.locationsDiscovered.Contains(location);
		}

		// Token: 0x06002992 RID: 10642 RVA: 0x00110E00 File Offset: 0x0010F000
		public bool IsVisited(Location location)
		{
			return location != null && this.locationsVisited.Contains(location);
		}

		// Token: 0x06002993 RID: 10643 RVA: 0x00110E14 File Offset: 0x0010F014
		public static Map Load(CampaignMode campaign, XElement element)
		{
			Map map = new Map(campaign, element);
			map.LoadState(campaign, element, false);
			return map;
		}

		// Token: 0x06002994 RID: 10644 RVA: 0x00110E34 File Offset: 0x0010F034
		public void LoadState(CampaignMode campaign, XElement element, bool showNotifications)
		{
			this.SetLocation(element.GetAttributeInt("currentlocation", 0));
			Version version;
			if (!Version.TryParse(element.GetAttributeString("version", ""), out version))
			{
				DebugConsole.ThrowError("Incompatible map save file, loading the game failed.", null, null, false, false);
				return;
			}
			this.ClearLocationHistory();
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "location"))
				{
					if (!(a == "connection"))
					{
						if (!(a == "radiation"))
						{
							if (!(a == "discovered"))
							{
								if (!(a == "visited"))
								{
									continue;
								}
							}
							else
							{
								bool trackedVisitedEmptyLocations = subElement.GetAttributeBool("trackedvisitedemptylocations", false);
								int[] discoveredIndices = subElement.GetAttributeIntArray("indices", Array.Empty<int>());
								foreach (int discoveredIndex in discoveredIndices)
								{
									this.<LoadState>g__Discover|97_0(this.Locations[discoveredIndex]);
								}
								using (IEnumerator<XElement> enumerator2 = subElement.GetChildElements("location", StringComparison.OrdinalIgnoreCase).GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										XElement childElement = enumerator2.Current;
										Location i = this.<LoadState>g__GetLocation|97_2(childElement);
										if (i != null)
										{
											this.<LoadState>g__Discover|97_0(i);
											if (!trackedVisitedEmptyLocations)
											{
												if (!i.HasOutpost())
												{
													this.Visit(i, false);
												}
												this.trackedLocationDiscoveryAndVisitOrder = false;
											}
										}
									}
									continue;
								}
							}
							int[] visitedIndices = subElement.GetAttributeIntArray("indices", Array.Empty<int>());
							foreach (int visitedIndex in visitedIndices)
							{
								this.Visit(this.Locations[visitedIndex], false);
							}
							foreach (XElement childElement2 in subElement.GetChildElements("location", StringComparison.OrdinalIgnoreCase))
							{
								Location j = this.<LoadState>g__GetLocation|97_2(childElement2);
								if (j != null)
								{
									this.Visit(j, false);
								}
							}
						}
						else
						{
							this.Radiation = new Radiation(this, this.generationParams.RadiationParams, subElement);
						}
					}
					else if (subElement.Attribute("i") != null)
					{
						int connectionIndex = subElement.GetAttributeInt("i", -1);
						if (connectionIndex < 0 || connectionIndex >= this.Connections.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Error while loading the campaign map: connection index out of bounds (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(connectionIndex);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							this.Connections[connectionIndex].Passed = subElement.GetAttributeBool("passed", false);
							this.Connections[connectionIndex].Locked = subElement.GetAttributeBool("locked", false);
						}
					}
				}
				else
				{
					int locationIndex = subElement.GetAttributeInt("i", -1);
					if (locationIndex < 0 || locationIndex >= this.Locations.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(69, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Error while loading the campaign map: location index out of bounds (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(locationIndex);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
					}
					else
					{
						Location location = this.Locations[locationIndex];
						location.ProximityTimer.Clear();
						for (int k = 0; k < location.Type.CanChangeTo.Count; k++)
						{
							for (int l2 = 0; l2 < location.Type.CanChangeTo[k].Requirements.Count; l2++)
							{
								location.ProximityTimer.Add(location.Type.CanChangeTo[k].Requirements[l2], subElement.GetAttributeInt("changetimer" + k.ToString() + "-" + l2.ToString(), 0));
							}
						}
						location.LoadLocationTypeChange(subElement);
						location.LoadChangingProperties(subElement, campaign);
						if (subElement.GetAttributeBool("discovered", false))
						{
							this.<LoadState>g__Discover|97_0(location);
							this.Visit(location, false);
							this.trackedLocationDiscoveryAndVisitOrder = false;
						}
						Identifier locationType = subElement.GetAttributeIdentifier("type", Identifier.Empty);
						LocalizedString prevLocationName = location.DisplayName;
						LocationType prevLocationType = location.Type;
						LocationType newLocationType = LocationType.Prefabs.Find((LocationType lt) => lt.Identifier == locationType) ?? LocationType.Prefabs.GetOrdered().First<LocationType>();
						location.ChangeType(campaign, newLocationType, true, true);
						if (showNotifications && prevLocationType != location.Type)
						{
							LocationTypeChange change = prevLocationType.CanChangeTo.Find((LocationTypeChange c) => c.ChangeToType == location.Type.Identifier);
							if (change != null)
							{
								location.TimeSinceLastTypeChange = 0;
							}
						}
						location.LoadStores(subElement);
						location.LoadMissions(subElement);
					}
				}
			}
			foreach (Location location3 in this.Locations)
			{
				if (location3 != null)
				{
					location3.InstantiateLoadedMissions(this);
				}
			}
			if (version < new Version(1, 0))
			{
				if (this.Locations.None((Location l) => l.Faction != null || l.SecondaryFaction != null))
				{
					Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed));
					foreach (Location location2 in this.Locations)
					{
						if (location2.Type.HasOutpost && campaign != null && location2.Type.OutpostTeam == CharacterTeamType.FriendlyNPC)
						{
							location2.Faction = campaign.GetRandomFaction(Rand.RandSync.ServerAndClient, true);
							if (location2 != this.StartLocation)
							{
								location2.SecondaryFaction = campaign.GetRandomSecondaryFaction(Rand.RandSync.ServerAndClient, true);
							}
						}
					}
				}
			}
			int currentLocationConnection = element.GetAttributeInt("currentlocationconnection", -1);
			if (currentLocationConnection >= 0)
			{
				this.Connections[currentLocationConnection].Locked = false;
				this.SelectLocation(this.Connections[currentLocationConnection].OtherLocation(this.CurrentLocation));
			}
			else if (this.CurrentLocation != null && !this.CurrentLocation.Type.HasOutpost && this.SelectedConnection == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(124, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Error while loading campaign map state. Submarine in a location with no outpost (");
				defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(this.CurrentLocation.DisplayName);
				defaultInterpolatedStringHandler3.AppendLiteral("). Loading the first adjacent connection...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
				this.SelectLocation(this.CurrentLocation.Connections[0].OtherLocation(this.CurrentLocation));
			}
			Location previousToEndLocation = this.GetPreviousToEndLocation();
			if (previousToEndLocation != null)
			{
				this.ForceLocationTypeToNone(campaign, previousToEndLocation);
			}
		}

		// Token: 0x06002995 RID: 10645 RVA: 0x00111608 File Offset: 0x0010F808
		public void Save(XElement element)
		{
			XElement mapElement = new XElement("map");
			mapElement.Add(new XAttribute("version", GameMain.Version.ToString()));
			mapElement.Add(new XAttribute("currentlocation", this.CurrentLocationIndex));
			GameMode gameMode = GameMain.GameSession.GameMode;
			CampaignMode campaign = gameMode as CampaignMode;
			if (campaign != null)
			{
				if (campaign.NextLevel != null && campaign.NextLevel.Type == LevelData.LevelType.LocationConnection)
				{
					mapElement.Add(new XAttribute("currentlocationconnection", this.Connections.IndexOf(this.CurrentLocation.Connections.Find((LocationConnection c) => c.LevelData == campaign.NextLevel))));
				}
				else if (Level.Loaded != null && Level.Loaded.Type == LevelData.LevelType.LocationConnection && !this.CurrentLocation.Type.HasOutpost)
				{
					mapElement.Add(new XAttribute("currentlocationconnection", this.Connections.IndexOf(this.Connections.Find((LocationConnection c) => c.LevelData == Level.Loaded.LevelData))));
				}
			}
			mapElement.Add(new XAttribute("width", this.Width));
			mapElement.Add(new XAttribute("height", this.Height));
			mapElement.Add(new XAttribute("selectedlocation", this.SelectedLocationIndex));
			mapElement.Add(new XAttribute("startlocation", this.Locations.IndexOf(this.StartLocation)));
			mapElement.Add(new XAttribute("endlocations", string.Join<int>(',', from e in this.EndLocations
			select this.Locations.IndexOf(e))));
			mapElement.Add(new XAttribute("seed", this.Seed));
			for (int i = 0; i < this.Locations.Count; i++)
			{
				Location location = this.Locations[i];
				XElement locationElement = location.Save(this, mapElement);
				locationElement.Add(new XAttribute("i", i));
			}
			for (int j = 0; j < this.Connections.Count; j++)
			{
				LocationConnection connection = this.Connections[j];
				XElement connectionElement = new XElement("connection", new object[]
				{
					new XAttribute("passed", connection.Passed),
					new XAttribute("locked", connection.Locked),
					new XAttribute("difficulty", connection.Difficulty),
					new XAttribute("biome", connection.Biome.Identifier),
					new XAttribute("i", j),
					new XAttribute("locations", this.Locations.IndexOf(connection.Locations[0]).ToString() + "," + this.Locations.IndexOf(connection.Locations[1]).ToString())
				});
				connection.LevelData.Save(connectionElement);
				mapElement.Add(connectionElement);
			}
			if (this.Radiation != null)
			{
				mapElement.Add(this.Radiation.Save());
			}
			if (this.locationsDiscovered.Any<Location>())
			{
				XElement discoveryElement = new XElement("discovered", new object[]
				{
					new XAttribute("trackedvisitedemptylocations", true),
					new XAttribute("indices", string.Join<int>(',', from l in this.locationsDiscovered
					select this.Locations.IndexOf(l)))
				});
				mapElement.Add(discoveryElement);
			}
			if (this.locationsVisited.Any<Location>())
			{
				XElement visitElement = new XElement("visited", new XAttribute("indices", string.Join<int>(',', from l in this.locationsVisited
				select this.Locations.IndexOf(l))));
				mapElement.Add(visitElement);
			}
			element.Add(mapElement);
		}

		// Token: 0x06002996 RID: 10646 RVA: 0x00111AC0 File Offset: 0x0010FCC0
		public void Remove()
		{
			foreach (Location location in this.Locations)
			{
				location.Remove();
			}
		}

		// Token: 0x06002997 RID: 10647 RVA: 0x00111B14 File Offset: 0x0010FD14
		[CompilerGenerated]
		private void <.ctor>g__FindStartLocation|60_2(Func<Location, bool> predicate)
		{
			foreach (Location location in this.Locations)
			{
				if (predicate(location) && (this.CurrentLocation == null || location.MapPosition.X < this.CurrentLocation.MapPosition.X))
				{
					this.CurrentLocation = (this.StartLocation = (this.furthestDiscoveredLocation = location));
				}
			}
		}

		// Token: 0x06002998 RID: 10648 RVA: 0x00111BA8 File Offset: 0x0010FDA8
		[CompilerGenerated]
		internal static bool <Generate>g__AllowAsBiomeGate|62_11(LocationType lt)
		{
			return lt.HasOutpost && lt.Identifier != "abandoned" && lt.BiomeGate != LocationType.BiomeGateSetting.Deny;
		}

		// Token: 0x06002999 RID: 10649 RVA: 0x00111BD4 File Offset: 0x0010FDD4
		[CompilerGenerated]
		private void <CreateEndLocation>g__FindConnectedEndLocations|71_0(Location currLocation, ref Map.<>c__DisplayClass71_0 A_2)
		{
			if (this.endLocations.Count >= A_2.endLocation.Biome.EndBiomeLocationCount)
			{
				return;
			}
			foreach (LocationConnection connection in currLocation.Connections)
			{
				if (connection.Biome == A_2.endLocation.Biome)
				{
					Location otherLocation = connection.OtherLocation(currLocation);
					if (otherLocation != null && !this.endLocations.Contains(otherLocation))
					{
						if (this.endLocations.Count >= A_2.endLocation.Biome.EndBiomeLocationCount)
						{
							break;
						}
						this.endLocations.Add(otherLocation);
						this.<CreateEndLocation>g__FindConnectedEndLocations|71_0(otherLocation, ref A_2);
					}
				}
			}
		}

		// Token: 0x0600299C RID: 10652 RVA: 0x00111CD8 File Offset: 0x0010FED8
		[CompilerGenerated]
		private Location <LoadState>g__GetLocation|97_2(XElement element)
		{
			int index = element.GetAttributeInt("i", -1);
			if (index < 0)
			{
				return null;
			}
			return this.Locations[index];
		}

		// Token: 0x0600299D RID: 10653 RVA: 0x00111D04 File Offset: 0x0010FF04
		[CompilerGenerated]
		private void <LoadState>g__Discover|97_0(Location location)
		{
			this.Discover(location, false);
			if (this.furthestDiscoveredLocation == null || location.MapPosition.X > this.furthestDiscoveredLocation.MapPosition.X)
			{
				this.furthestDiscoveredLocation = location;
			}
		}

		// Token: 0x04001466 RID: 5222
		public bool AllowDebugTeleport;

		// Token: 0x04001467 RID: 5223
		private readonly MapGenerationParams generationParams;

		// Token: 0x04001468 RID: 5224
		private Location furthestDiscoveredLocation;

		// Token: 0x0400146B RID: 5227
		public Action<Location, LocationConnection> OnLocationSelected;

		// Token: 0x0400146C RID: 5228
		public Action<LocationConnection, IEnumerable<Mission>> OnMissionsSelected;

		// Token: 0x0400146D RID: 5229
		public readonly NamedEvent<Map.LocationChangeInfo> OnLocationChanged = new NamedEvent<Map.LocationChangeInfo>();

		// Token: 0x0400146E RID: 5230
		private List<Location> endLocations = new List<Location>();

		// Token: 0x04001475 RID: 5237
		private readonly List<Location> locationsDiscovered = new List<Location>();

		// Token: 0x04001476 RID: 5238
		private readonly List<Location> locationsVisited = new List<Location>();

		// Token: 0x04001478 RID: 5240
		public Radiation Radiation;

		// Token: 0x04001479 RID: 5241
		private bool trackedLocationDiscoveryAndVisitOrder = true;

		// Token: 0x0400147A RID: 5242
		private IOrderedEnumerable<Biome> _orderedBiomes;

		// Token: 0x02000A5B RID: 2651
		public readonly struct LocationChangeInfo
		{
			// Token: 0x06005CD3 RID: 23763 RVA: 0x00201D66 File Offset: 0x001FFF66
			public LocationChangeInfo(Location prevLocation, Location newLocation)
			{
				this.PrevLocation = prevLocation;
				this.NewLocation = newLocation;
			}

			// Token: 0x040035EE RID: 13806
			public readonly Location PrevLocation;

			// Token: 0x040035EF RID: 13807
			public readonly Location NewLocation;
		}

		// Token: 0x02000A5C RID: 2652
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040035F0 RID: 13808
			public static Func<LocationType, bool> <0>__AllowAsBiomeGate;
		}
	}
}
