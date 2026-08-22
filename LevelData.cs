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
	// Token: 0x0200031C RID: 796
	internal class LevelData
	{
		// Token: 0x1700109D RID: 4253
		// (get) Token: 0x06003F3C RID: 16188 RVA: 0x00235FAF File Offset: 0x002341AF
		// (set) Token: 0x06003F3D RID: 16189 RVA: 0x00235FB7 File Offset: 0x002341B7
		public LevelGenerationParams GenerationParams { get; private set; }

		// Token: 0x1700109E RID: 4254
		// (get) Token: 0x06003F3E RID: 16190 RVA: 0x00235FC0 File Offset: 0x002341C0
		public float CrushDepth
		{
			get
			{
				return Math.Max((float)this.Size.Y, 3500f / Physics.DisplayToRealWorldRatio) - (float)this.InitialDepth;
			}
		}

		// Token: 0x1700109F RID: 4255
		// (get) Token: 0x06003F3F RID: 16191 RVA: 0x00235FE6 File Offset: 0x002341E6
		public float RealWorldCrushDepth
		{
			get
			{
				return Math.Max((float)this.Size.Y * Physics.DisplayToRealWorldRatio, 3500f);
			}
		}

		// Token: 0x06003F40 RID: 16192 RVA: 0x00236004 File Offset: 0x00234204
		public bool IsAllowedDifficulty(float minDifficulty, float maxDifficulty)
		{
			return this.Difficulty >= minDifficulty && this.Difficulty <= maxDifficulty;
		}

		// Token: 0x06003F41 RID: 16193 RVA: 0x00236020 File Offset: 0x00234220
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

		// Token: 0x06003F42 RID: 16194 RVA: 0x00236130 File Offset: 0x00234330
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

		// Token: 0x06003F43 RID: 16195 RVA: 0x00236560 File Offset: 0x00234760
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

		// Token: 0x06003F44 RID: 16196 RVA: 0x0023678C File Offset: 0x0023498C
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

		// Token: 0x06003F45 RID: 16197 RVA: 0x002368C8 File Offset: 0x00234AC8
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

		// Token: 0x06003F46 RID: 16198 RVA: 0x00236A78 File Offset: 0x00234C78
		public void ExhaustEventSet(EventSet eventSet)
		{
			this.exhaustedEventSets.Add(eventSet.Identifier);
		}

		// Token: 0x06003F47 RID: 16199 RVA: 0x00236A8C File Offset: 0x00234C8C
		public bool IsEventSetExhausted(EventSet eventSet)
		{
			return this.allEventsExhausted || this.exhaustedEventSets.Contains(eventSet.Identifier);
		}

		// Token: 0x06003F48 RID: 16200 RVA: 0x00236AA9 File Offset: 0x00234CA9
		public void ResetExhaustedEventSets()
		{
			this.allEventsExhausted = false;
			this.exhaustedEventSets.Clear();
		}

		// Token: 0x06003F49 RID: 16201 RVA: 0x00236ABD File Offset: 0x00234CBD
		public void ReassignGenerationParams(string seed)
		{
			this.GenerationParams = LevelGenerationParams.GetRandom(seed, this.Type, this.Difficulty, this.Biome.Identifier, false, false);
		}

		// Token: 0x170010A0 RID: 4256
		// (get) Token: 0x06003F4A RID: 16202 RVA: 0x00236AE4 File Offset: 0x00234CE4
		public bool OutpostGenerationParamsExist
		{
			get
			{
				return this.ForceOutpostGenerationParams != null || OutpostGenerationParams.OutpostParams.Any<OutpostGenerationParams>();
			}
		}

		// Token: 0x06003F4B RID: 16203 RVA: 0x00236AFC File Offset: 0x00234CFC
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

		// Token: 0x06003F4C RID: 16204 RVA: 0x00236C18 File Offset: 0x00234E18
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

		// Token: 0x06003F4E RID: 16206 RVA: 0x00236F54 File Offset: 0x00235154
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

		// Token: 0x040020D9 RID: 8409
		public readonly LevelData.LevelType Type;

		// Token: 0x040020DA RID: 8410
		public readonly string Seed;

		// Token: 0x040020DB RID: 8411
		public readonly float Difficulty;

		// Token: 0x040020DC RID: 8412
		public readonly Biome Biome;

		// Token: 0x040020DE RID: 8414
		public bool HasBeaconStation;

		// Token: 0x040020DF RID: 8415
		public bool IsBeaconActive;

		// Token: 0x040020E0 RID: 8416
		public bool HasHuntingGrounds;

		// Token: 0x040020E1 RID: 8417
		public bool OriginallyHadHuntingGrounds;

		// Token: 0x040020E2 RID: 8418
		public const float HuntingGroundsDifficultyThreshold = 25f;

		// Token: 0x040020E3 RID: 8419
		public const float MaxHuntingGroundsProbability = 0.3f;

		// Token: 0x040020E4 RID: 8420
		public OutpostGenerationParams ForceOutpostGenerationParams;

		// Token: 0x040020E5 RID: 8421
		public SubmarineInfo ForceBeaconStation;

		// Token: 0x040020E6 RID: 8422
		public SubmarineInfo ForceWreck;

		// Token: 0x040020E7 RID: 8423
		public RuinGenerationParams ForceRuinGenerationParams;

		// Token: 0x040020E8 RID: 8424
		public static SubmarineInfo ConsoleForceWreck;

		// Token: 0x040020E9 RID: 8425
		public static SubmarineInfo ConsoleForceBeaconStation;

		// Token: 0x040020EA RID: 8426
		public static LevelData.ThalamusSpawn ForceThalamus;

		// Token: 0x040020EB RID: 8427
		public bool AllowInvalidOutpost;

		// Token: 0x040020EC RID: 8428
		public readonly Point Size;

		// Token: 0x040020ED RID: 8429
		public readonly int InitialDepth;

		// Token: 0x040020EE RID: 8430
		public int? MinMainPathWidth;

		// Token: 0x040020EF RID: 8431
		public readonly List<Identifier> EventHistory = new List<Identifier>();

		// Token: 0x040020F0 RID: 8432
		public readonly List<Identifier> NonRepeatableEvents = new List<Identifier>();

		// Token: 0x040020F1 RID: 8433
		public readonly Dictionary<EventSet, int> FinishedEvents = new Dictionary<EventSet, int>();

		// Token: 0x040020F2 RID: 8434
		private bool allEventsExhausted;

		// Token: 0x040020F3 RID: 8435
		private HashSet<Identifier> exhaustedEventSets = new HashSet<Identifier>();

		// Token: 0x02000FE3 RID: 4067
		[Flags]
		public enum LevelType
		{
			// Token: 0x040056D8 RID: 22232
			LocationConnection = 1,
			// Token: 0x040056D9 RID: 22233
			Outpost = 2
		}

		// Token: 0x02000FE4 RID: 4068
		public enum ThalamusSpawn
		{
			// Token: 0x040056DB RID: 22235
			Random,
			// Token: 0x040056DC RID: 22236
			Forced,
			// Token: 0x040056DD RID: 22237
			Disabled
		}
	}
}
