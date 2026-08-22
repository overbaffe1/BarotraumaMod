using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.RuinGeneration;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200023A RID: 570
	internal class LevelData
	{
		// Token: 0x17000B32 RID: 2866
		// (get) Token: 0x060026E7 RID: 9959 RVA: 0x000FF680 File Offset: 0x000FD880
		// (set) Token: 0x060026E8 RID: 9960 RVA: 0x000FF688 File Offset: 0x000FD888
		public LevelGenerationParams GenerationParams { get; private set; }

		// Token: 0x17000B33 RID: 2867
		// (get) Token: 0x060026E9 RID: 9961 RVA: 0x000FF691 File Offset: 0x000FD891
		public float CrushDepth
		{
			get
			{
				return Math.Max((float)this.Size.Y, 3500f / Physics.DisplayToRealWorldRatio) - (float)this.InitialDepth;
			}
		}

		// Token: 0x17000B34 RID: 2868
		// (get) Token: 0x060026EA RID: 9962 RVA: 0x000FF6B7 File Offset: 0x000FD8B7
		public float RealWorldCrushDepth
		{
			get
			{
				return Math.Max((float)this.Size.Y * Physics.DisplayToRealWorldRatio, 3500f);
			}
		}

		// Token: 0x060026EB RID: 9963 RVA: 0x000FF6D5 File Offset: 0x000FD8D5
		public bool IsAllowedDifficulty(float minDifficulty, float maxDifficulty)
		{
			return this.Difficulty >= minDifficulty && this.Difficulty <= maxDifficulty;
		}

		// Token: 0x060026EC RID: 9964 RVA: 0x000FF6F0 File Offset: 0x000FD8F0
		public LevelData(string seed, float difficulty, float sizeFactor, LevelGenerationParams generationParams, Biome biome)
		{
			if (seed == null)
			{
				throw new ArgumentException("Seed was null");
			}
			this.Seed = seed;
			if (biome == null)
			{
				throw new ArgumentException("Biome was null");
			}
			this.Biome = biome;
			if (generationParams == null)
			{
				throw new ArgumentException("Level generation parameters were null");
			}
			this.GenerationParams = generationParams;
			this.Type = this.GenerationParams.Type;
			this.Difficulty = difficulty;
			sizeFactor = MathHelper.Clamp(sizeFactor, 0f, 1f);
			int width = (int)MathHelper.Lerp((float)generationParams.MinWidth, (float)generationParams.MaxWidth, sizeFactor);
			this.InitialDepth = (int)MathHelper.Lerp((float)generationParams.InitialDepthMin, (float)generationParams.InitialDepthMax, sizeFactor);
			this.Size = new Point((int)MathUtils.Round((float)width, 2000f), (int)MathUtils.Round((float)generationParams.Height, 2000f));
		}

		// Token: 0x060026ED RID: 9965 RVA: 0x000FF800 File Offset: 0x000FDA00
		public LevelData(XElement element, float? forceDifficulty = null, bool clampDifficultyToBiome = false)
		{
			this.Seed = element.GetAttributeString("seed", "");
			this.Size = element.GetAttributePoint("size", new Point(1000));
			Enum.TryParse<LevelData.LevelType>(element.GetAttributeString("type", "LocationConnection"), out this.Type);
			this.HasBeaconStation = element.GetAttributeBool("hasbeaconstation", false);
			this.IsBeaconActive = element.GetAttributeBool("isbeaconactive", false);
			this.HasHuntingGrounds = element.GetAttributeBool("hashuntinggrounds", false);
			this.OriginallyHadHuntingGrounds = element.GetAttributeBool("originallyhadhuntinggrounds", this.HasHuntingGrounds);
			string generationParamsId = element.GetAttributeString("generationparams", "");
			this.GenerationParams = LevelGenerationParams.LevelParams.Find(delegate(LevelGenerationParams l)
			{
				if (l.Identifier == generationParamsId)
				{
					return true;
				}
				Identifier oldIdentifier = l.OldIdentifier;
				if (!oldIdentifier.IsEmpty)
				{
					oldIdentifier = l.OldIdentifier;
					return oldIdentifier == generationParamsId;
				}
				return false;
			});
			if (this.GenerationParams == null)
			{
				DebugConsole.ThrowError("Error while loading a level. Could not find level generation params with the ID \"" + generationParamsId + "\".", null, null, false, false);
				this.GenerationParams = LevelGenerationParams.LevelParams.FirstOrDefault((LevelGenerationParams l) => l.Type == this.Type);
				if (this.GenerationParams == null)
				{
					this.GenerationParams = LevelGenerationParams.LevelParams.First<LevelGenerationParams>();
				}
			}
			this.InitialDepth = element.GetAttributeInt("initialdepth", this.GenerationParams.InitialDepthMin);
			string biomeIdentifier = element.GetAttributeString("biome", "");
			this.Biome = Biome.Prefabs.FirstOrDefault((Biome b) => b.Identifier == biomeIdentifier || (!b.OldIdentifier.IsEmpty && b.OldIdentifier == biomeIdentifier));
			if (this.Biome == null)
			{
				DebugConsole.ThrowError("Error in level data: could not find the biome \"" + biomeIdentifier + "\".", null, null, false, false);
				this.Biome = Biome.Prefabs.First<Biome>();
			}
			this.Difficulty = (forceDifficulty ?? element.GetAttributeFloat("difficulty", 0f));
			if (clampDifficultyToBiome)
			{
				this.Difficulty = MathHelper.Clamp(this.Difficulty, this.Biome.MinDifficulty, this.Biome.AdjustedMaxDifficulty);
			}
			string[] prefabNames = element.GetAttributeStringArray("eventhistory", Array.Empty<string>(), true, false);
			this.EventHistory.AddRange(from p in EventPrefab.Prefabs
			where prefabNames.Any((string n) => p.Identifier == n)
			select p.Identifier);
			string[] nonRepeatablePrefabNames = element.GetAttributeStringArray("nonrepeatableevents", Array.Empty<string>(), true, false);
			this.NonRepeatableEvents.AddRange(from p in EventPrefab.Prefabs
			where nonRepeatablePrefabNames.Any((string n) => p.Identifier == n)
			select p.Identifier);
			string finishedEventsName = "FinishedEvents";
			XElement finishedEventsElement = element.GetChildElement(finishedEventsName, StringComparison.OrdinalIgnoreCase);
			if (finishedEventsElement != null)
			{
				foreach (XElement childElement in finishedEventsElement.GetChildElements(finishedEventsName, StringComparison.OrdinalIgnoreCase))
				{
					Identifier eventSetIdentifier = childElement.GetAttributeIdentifier("set", Identifier.Empty);
					if (!eventSetIdentifier.IsEmpty)
					{
						EventSet eventSet;
						if (!EventSet.Prefabs.TryGet(eventSetIdentifier, out eventSet))
						{
							foreach (EventSet prefab in EventSet.Prefabs)
							{
								EventSet foundSet = LevelData.<.ctor>g__FindSetRecursive|38_7(prefab, eventSetIdentifier);
								if (foundSet != null)
								{
									eventSet = foundSet;
									break;
								}
							}
						}
						if (eventSet != null)
						{
							int count = childElement.GetAttributeInt("count", 0);
							if (count >= 1)
							{
								this.FinishedEvents.TryAdd(eventSet, count);
							}
						}
					}
				}
			}
			this.exhaustedEventSets = element.GetAttributeIdentifierArray("exhaustedEventSets", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			this.allEventsExhausted = element.GetAttributeBool("EventsExhausted", false);
		}

		// Token: 0x060026EE RID: 9966 RVA: 0x000FFC30 File Offset: 0x000FDE30
		public LevelData(LocationConnection locationConnection)
		{
			this.Seed = locationConnection.Locations[0].LevelData.Seed + locationConnection.Locations[1].LevelData.Seed;
			bool connectionIsBiomeTransition = locationConnection.Locations[0].Biome.Identifier != locationConnection.Locations[1].Biome.Identifier;
			this.Biome = locationConnection.Biome;
			this.Type = LevelData.LevelType.LocationConnection;
			this.Difficulty = locationConnection.Difficulty;
			this.GenerationParams = LevelGenerationParams.GetRandom(this.Seed, LevelData.LevelType.LocationConnection, this.Difficulty, this.Biome.Identifier, false, connectionIsBiomeTransition);
			float sizeFactor = MathUtils.InverseLerp(MapGenerationParams.Instance.SmallLevelConnectionLength, MapGenerationParams.Instance.LargeLevelConnectionLength, locationConnection.Length);
			int width = (int)MathHelper.Lerp((float)this.GenerationParams.MinWidth, (float)this.GenerationParams.MaxWidth, sizeFactor);
			this.Size = new Point((int)MathUtils.Round((float)width, 2000f), (int)MathUtils.Round((float)this.GenerationParams.Height, 2000f));
			MTRandom rand = new MTRandom(ToolBox.StringToInt(this.Seed));
			this.InitialDepth = (int)MathHelper.Lerp((float)this.GenerationParams.InitialDepthMin, (float)this.GenerationParams.InitialDepthMax, (float)rand.NextDouble());
			if (this.Biome.IsEndBiome)
			{
				this.HasHuntingGrounds = false;
				this.HasBeaconStation = false;
			}
			else
			{
				this.HasHuntingGrounds = (this.OriginallyHadHuntingGrounds = (rand.NextDouble() < (double)(MathUtils.InverseLerp(25f, 100f, this.Difficulty) * 0.3f)));
				bool hasBeaconStation;
				if (!this.HasHuntingGrounds)
				{
					hasBeaconStation = (rand.NextDouble() < (double)(from l in locationConnection.Locations
					select l.Type.BeaconStationChance).Max());
				}
				else
				{
					hasBeaconStation = false;
				}
				this.HasBeaconStation = hasBeaconStation;
			}
			this.IsBeaconActive = false;
		}

		// Token: 0x060026EF RID: 9967 RVA: 0x000FFE5C File Offset: 0x000FE05C
		public LevelData(Location location, Map map, float difficulty)
		{
			this.Seed = location.NameIdentifier.Value + map.Locations.IndexOf(location).ToString();
			this.Biome = location.Biome;
			this.Type = LevelData.LevelType.Outpost;
			this.Difficulty = difficulty;
			this.GenerationParams = LevelGenerationParams.GetRandom(this.Seed, LevelData.LevelType.Outpost, this.Difficulty, this.Biome.Identifier, false, false);
			MTRandom rand = new MTRandom(ToolBox.StringToInt(this.Seed));
			int width = (int)MathHelper.Lerp((float)this.GenerationParams.MinWidth, (float)this.GenerationParams.MaxWidth, (float)rand.NextDouble());
			this.InitialDepth = (int)MathHelper.Lerp((float)this.GenerationParams.InitialDepthMin, (float)this.GenerationParams.InitialDepthMax, (float)rand.NextDouble());
			this.Size = new Point((int)MathUtils.Round((float)width, 2000f), (int)MathUtils.Round((float)this.GenerationParams.Height, 2000f));
		}

		// Token: 0x060026F0 RID: 9968 RVA: 0x000FFF98 File Offset: 0x000FE198
		public static LevelData CreateRandom(string seed = "", float? difficulty = null, LevelGenerationParams generationParams = null, Identifier biomeId = default(Identifier), bool requireOutpost = false, bool pvpOnly = false)
		{
			if (string.IsNullOrEmpty(seed))
			{
				seed = Rand.Range(0, int.MaxValue, Rand.RandSync.ServerAndClient).ToString();
			}
			Rand.SetSyncedSeed(ToolBox.StringToInt(seed));
			LevelGenerationParams generationParams2 = generationParams;
			LevelData.LevelType type = (generationParams2 != null) ? generationParams2.Type : (requireOutpost ? LevelData.LevelType.Outpost : LevelData.LevelType.LocationConnection);
			float selectedDifficulty = difficulty ?? Rand.Range(30f, 80f, Rand.RandSync.ServerAndClient);
			Biome biome = null;
			if (!biomeId.IsEmpty && biomeId != "Random")
			{
				Biome.Prefabs.TryGet(biomeId, out biome);
			}
			if (generationParams == null)
			{
				generationParams = LevelGenerationParams.GetRandom(seed, type, selectedDifficulty, biomeId, pvpOnly, false);
			}
			if (biome == null)
			{
				biome = (Biome.Prefabs.FirstOrDefault(delegate(Biome b)
				{
					LevelGenerationParams generationParams3 = generationParams;
					return generationParams3 != null && generationParams3.AllowedBiomeIdentifiers.Contains(b.Identifier);
				}) ?? Biome.Prefabs.GetRandom(Rand.RandSync.ServerAndClient));
			}
			LevelData levelData = new LevelData(seed, selectedDifficulty, Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient), generationParams, biome);
			if (type == LevelData.LevelType.LocationConnection)
			{
				float beaconRng = Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient);
				levelData.HasBeaconStation = (beaconRng < 0.5f);
				levelData.IsBeaconActive = (beaconRng > 0.25f);
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) != null)
			{
				foreach (Mission mission in GameMain.GameSession.GameMode.Missions)
				{
					mission.AdjustLevelData(levelData);
				}
			}
			return levelData;
		}

		// Token: 0x060026F1 RID: 9969 RVA: 0x00100148 File Offset: 0x000FE348
		public void ExhaustEventSet(EventSet eventSet)
		{
			this.exhaustedEventSets.Add(eventSet.Identifier);
		}

		// Token: 0x060026F2 RID: 9970 RVA: 0x0010015C File Offset: 0x000FE35C
		public bool IsEventSetExhausted(EventSet eventSet)
		{
			return this.allEventsExhausted || this.exhaustedEventSets.Contains(eventSet.Identifier);
		}

		// Token: 0x060026F3 RID: 9971 RVA: 0x00100179 File Offset: 0x000FE379
		public void ResetExhaustedEventSets()
		{
			this.allEventsExhausted = false;
			this.exhaustedEventSets.Clear();
		}

		// Token: 0x060026F4 RID: 9972 RVA: 0x0010018D File Offset: 0x000FE38D
		public void ReassignGenerationParams(string seed)
		{
			this.GenerationParams = LevelGenerationParams.GetRandom(seed, this.Type, this.Difficulty, this.Biome.Identifier, false, false);
		}

		// Token: 0x17000B35 RID: 2869
		// (get) Token: 0x060026F5 RID: 9973 RVA: 0x001001B4 File Offset: 0x000FE3B4
		public bool OutpostGenerationParamsExist
		{
			get
			{
				return this.ForceOutpostGenerationParams != null || OutpostGenerationParams.OutpostParams.Any<OutpostGenerationParams>();
			}
		}

		// Token: 0x060026F6 RID: 9974 RVA: 0x001001CC File Offset: 0x000FE3CC
		public static IEnumerable<OutpostGenerationParams> GetSuitableOutpostGenerationParams(Location location, LevelData levelData)
		{
			IEnumerable<OutpostGenerationParams> paramsForGameMode = OutpostGenerationParams.OutpostParams.Where(delegate(OutpostGenerationParams p)
			{
				if (!p.AllowedGameModeIdentifiers.None(null))
				{
					GameSession gameSession = GameMain.GameSession;
					GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
					if (gameMode != null)
					{
						return p.AllowedGameModeIdentifiers.Contains(gameMode.Preset.Identifier);
					}
				}
				return true;
			});
			IEnumerable<OutpostGenerationParams> paramsWithMatchingLevelType = paramsForGameMode.Where(delegate(OutpostGenerationParams p)
			{
				if (p.LevelType != null)
				{
					LevelData.LevelType type = levelData.Type;
					LevelData.LevelType? levelType = p.LevelType;
					return type == levelType.GetValueOrDefault() & levelType != null;
				}
				return true;
			});
			IEnumerable<OutpostGenerationParams> suitableParams = from p in paramsWithMatchingLevelType
			where location == null || p.AllowedLocationTypes.Contains(location.Type.Identifier)
			select p;
			if (!suitableParams.Any<OutpostGenerationParams>())
			{
				if (!location.Type.UseOutpostModulesOfLocationType.IsEmpty)
				{
					suitableParams = from p in paramsWithMatchingLevelType
					where p.AllowedLocationTypes.Contains(location.Type.UseOutpostModulesOfLocationType)
					select p;
				}
				if (!suitableParams.Any<OutpostGenerationParams>())
				{
					suitableParams = from p in paramsWithMatchingLevelType
					where location == null || !p.AllowedLocationTypes.Any<Identifier>()
					select p;
					if (!suitableParams.Any<OutpostGenerationParams>())
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(102, 1);
						defaultInterpolatedStringHandler.AppendLiteral("No suitable outpost generation parameters found for the location type \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(location.Type.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\". Selecting random parameters.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						suitableParams = paramsForGameMode;
					}
				}
			}
			return suitableParams;
		}

		// Token: 0x060026F7 RID: 9975 RVA: 0x001002E8 File Offset: 0x000FE4E8
		public void Save(XElement parentElement)
		{
			XElement newElement = new XElement("Level", new object[]
			{
				new XAttribute("seed", this.Seed),
				new XAttribute("biome", this.Biome.Identifier),
				new XAttribute("type", this.Type.ToString()),
				new XAttribute("difficulty", this.Difficulty.ToString("G", CultureInfo.InvariantCulture)),
				new XAttribute("size", XMLExtensions.PointToString(this.Size)),
				new XAttribute("generationparams", this.GenerationParams.Identifier),
				new XAttribute("initialdepth", this.InitialDepth)
			});
			newElement.Add(new XAttribute("exhaustedEventSets", string.Join<string>(',', from e in this.exhaustedEventSets
			select e.Value)));
			if (this.HasBeaconStation)
			{
				newElement.Add(new object[]
				{
					new XAttribute("hasbeaconstation", this.HasBeaconStation.ToString()),
					new XAttribute("isbeaconactive", this.IsBeaconActive.ToString())
				});
			}
			if (this.HasHuntingGrounds)
			{
				newElement.Add(new XAttribute("hashuntinggrounds", true));
			}
			if (this.HasHuntingGrounds || this.OriginallyHadHuntingGrounds)
			{
				newElement.Add(new XAttribute("originallyhadhuntinggrounds", true));
			}
			if (this.Type == LevelData.LevelType.Outpost)
			{
				if (this.EventHistory.Any<Identifier>())
				{
					newElement.Add(new XAttribute("eventhistory", string.Join<Identifier>(',', this.EventHistory)));
				}
				if (this.NonRepeatableEvents.Any<Identifier>())
				{
					newElement.Add(new XAttribute("nonrepeatableevents", string.Join<Identifier>(',', this.NonRepeatableEvents)));
				}
				if (this.FinishedEvents.Any<KeyValuePair<EventSet, int>>())
				{
					XElement finishedEventsElement = new XElement("FinishedEvents");
					foreach (KeyValuePair<EventSet, int> keyValuePair in this.FinishedEvents)
					{
						EventSet eventSet;
						int num;
						keyValuePair.Deconstruct(out eventSet, out num);
						EventSet set = eventSet;
						int count = num;
						XElement element = new XElement("FinishedEvents", new object[]
						{
							new XAttribute("set", set.Identifier),
							new XAttribute("count", count)
						});
						finishedEventsElement.Add(element);
					}
					newElement.Add(finishedEventsElement);
				}
			}
			parentElement.Add(newElement);
		}

		// Token: 0x060026F9 RID: 9977 RVA: 0x00100624 File Offset: 0x000FE824
		[CompilerGenerated]
		internal static EventSet <.ctor>g__FindSetRecursive|38_7(EventSet parentSet, Identifier setIdentifier)
		{
			foreach (EventSet childSet in parentSet.ChildSets)
			{
				if (childSet.Identifier == setIdentifier)
				{
					return childSet;
				}
				EventSet foundSet = LevelData.<.ctor>g__FindSetRecursive|38_7(childSet, setIdentifier);
				if (foundSet != null)
				{
					return foundSet;
				}
			}
			return null;
		}

		// Token: 0x04001305 RID: 4869
		public readonly LevelData.LevelType Type;

		// Token: 0x04001306 RID: 4870
		public readonly string Seed;

		// Token: 0x04001307 RID: 4871
		public readonly float Difficulty;

		// Token: 0x04001308 RID: 4872
		public readonly Biome Biome;

		// Token: 0x0400130A RID: 4874
		public bool HasBeaconStation;

		// Token: 0x0400130B RID: 4875
		public bool IsBeaconActive;

		// Token: 0x0400130C RID: 4876
		public bool HasHuntingGrounds;

		// Token: 0x0400130D RID: 4877
		public bool OriginallyHadHuntingGrounds;

		// Token: 0x0400130E RID: 4878
		public const float HuntingGroundsDifficultyThreshold = 25f;

		// Token: 0x0400130F RID: 4879
		public const float MaxHuntingGroundsProbability = 0.3f;

		// Token: 0x04001310 RID: 4880
		public OutpostGenerationParams ForceOutpostGenerationParams;

		// Token: 0x04001311 RID: 4881
		public SubmarineInfo ForceBeaconStation;

		// Token: 0x04001312 RID: 4882
		public SubmarineInfo ForceWreck;

		// Token: 0x04001313 RID: 4883
		public RuinGenerationParams ForceRuinGenerationParams;

		// Token: 0x04001314 RID: 4884
		public static SubmarineInfo ConsoleForceWreck;

		// Token: 0x04001315 RID: 4885
		public static SubmarineInfo ConsoleForceBeaconStation;

		// Token: 0x04001316 RID: 4886
		public static LevelData.ThalamusSpawn ForceThalamus;

		// Token: 0x04001317 RID: 4887
		public bool AllowInvalidOutpost;

		// Token: 0x04001318 RID: 4888
		public readonly Point Size;

		// Token: 0x04001319 RID: 4889
		public readonly int InitialDepth;

		// Token: 0x0400131A RID: 4890
		public int? MinMainPathWidth;

		// Token: 0x0400131B RID: 4891
		public readonly List<Identifier> EventHistory = new List<Identifier>();

		// Token: 0x0400131C RID: 4892
		public readonly List<Identifier> NonRepeatableEvents = new List<Identifier>();

		// Token: 0x0400131D RID: 4893
		public readonly Dictionary<EventSet, int> FinishedEvents = new Dictionary<EventSet, int>();

		// Token: 0x0400131E RID: 4894
		private bool allEventsExhausted;

		// Token: 0x0400131F RID: 4895
		private HashSet<Identifier> exhaustedEventSets = new HashSet<Identifier>();

		// Token: 0x02000A0B RID: 2571
		[Flags]
		public enum LevelType
		{
			// Token: 0x0400350A RID: 13578
			LocationConnection = 1,
			// Token: 0x0400350B RID: 13579
			Outpost = 2
		}

		// Token: 0x02000A0C RID: 2572
		public enum ThalamusSpawn
		{
			// Token: 0x0400350D RID: 13581
			Random,
			// Token: 0x0400350E RID: 13582
			Forced,
			// Token: 0x0400350F RID: 13583
			Disabled
		}
	}
}
