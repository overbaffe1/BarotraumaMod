using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001C2 RID: 450
	internal sealed class EventSet : Prefab
	{
		// Token: 0x0600215E RID: 8542 RVA: 0x000DF81A File Offset: 0x000DDA1A
		public static IEnumerable<EventPrefab> GetAllEventPrefabs()
		{
			return EventSet.AllEventPrefabs.Values;
		}

		// Token: 0x0600215F RID: 8543 RVA: 0x000DF828 File Offset: 0x000DDA28
		public static void RefreshAllEventPrefabs()
		{
			EventSet.AllEventPrefabs.Clear();
			foreach (EventPrefab eventPrefab in EventPrefab.Prefabs)
			{
				EventSet.AllEventPrefabs.TryAdd(eventPrefab.Identifier, eventPrefab);
			}
			foreach (EventSet eventSet in EventSet.Prefabs)
			{
				EventSet.AddChildEventPrefabs(eventSet);
			}
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x000DF8C4 File Offset: 0x000DDAC4
		private static void AddChildEventPrefabs(EventSet set)
		{
			foreach (EventSet.SubEventPrefab subEventPrefabs in set.EventPrefabs)
			{
				foreach (EventPrefab eventPrefab in subEventPrefabs.EventPrefabs)
				{
					EventSet.AllEventPrefabs.TryAdd(eventPrefab.Identifier, eventPrefab);
				}
			}
			foreach (EventSet childSet in set.ChildSets)
			{
				EventSet.AddChildEventPrefabs(childSet);
			}
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x000DF964 File Offset: 0x000DDB64
		public static EventPrefab GetEventPrefab(Identifier identifier)
		{
			return EventSet.AllEventPrefabs.GetValueOrDefault(identifier);
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x000DF974 File Offset: 0x000DDB74
		private static Identifier DetermineIdentifier(EventSet parent, XElement element, RandomEventsFile file)
		{
			Identifier retVal = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (retVal.IsEmpty)
			{
				if (parent == null)
				{
					if (file.ContentPackage is CorePackage)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error in ");
						defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(file.Path);
						defaultInterpolatedStringHandler.AppendLiteral(": All root EventSets in a core package must have identifiers");
						throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(46, 1);
					defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(file.Path);
					defaultInterpolatedStringHandler2.AppendLiteral(": All root EventSets should have an identifier");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), file.ContentPackage);
				}
				XElement currElement = element;
				string siblingIndices = "";
				while (currElement.Parent != null)
				{
					int siblingIndex = currElement.ElementsBeforeSelf().Count<XElement>();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("-");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(siblingIndex);
					defaultInterpolatedStringHandler3.AppendFormatted(siblingIndices);
					siblingIndices = defaultInterpolatedStringHandler3.ToStringAndClear();
					if (parent != null)
					{
						break;
					}
					currElement = currElement.Parent;
				}
				string str;
				if (parent == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler4.AppendFormatted(file.ContentPackage.Name);
					defaultInterpolatedStringHandler4.AppendLiteral("-");
					defaultInterpolatedStringHandler4.AppendFormatted<ContentPath>(file.Path);
					str = defaultInterpolatedStringHandler4.ToStringAndClear();
				}
				else
				{
					str = parent.Identifier.Value;
				}
				retVal = (str + siblingIndices).ToIdentifier();
			}
			return retVal;
		}

		// Token: 0x06002163 RID: 8547 RVA: 0x000DFAD0 File Offset: 0x000DDCD0
		public EventSet(ContentXElement element, RandomEventsFile file, EventSet parentSet = null) : base(file, EventSet.DetermineIdentifier(parentSet, element, file))
		{
			List<EventSet.SubEventPrefab> eventPrefabs = new List<EventSet.SubEventPrefab>();
			List<EventSet> childSets = new List<EventSet>();
			Dictionary<Identifier, float> overrideCommonness = new Dictionary<Identifier, float>();
			this.BiomeIdentifier = element.GetAttributeIdentifier("biome", Identifier.Empty);
			this.MinLevelDifficulty = element.GetAttributeFloat("minleveldifficulty", 0f);
			this.MaxLevelDifficulty = Math.Max(element.GetAttributeFloat("maxleveldifficulty", 100f), this.MinLevelDifficulty);
			this.Additive = element.GetAttributeBool("Additive", false);
			this.SelectAlways = element.GetAttributeBool("SelectAlways", false);
			string levelTypeStr = element.GetAttributeString("leveltype", ((parentSet != null) ? parentSet.LevelType.ToString() : null) ?? "LocationConnection");
			if (!Enum.TryParse<LevelData.LevelType>(levelTypeStr, true, out this.LevelType))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event set \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". \"");
				defaultInterpolatedStringHandler.AppendFormatted(levelTypeStr);
				defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid level type.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			this.Faction = element.GetAttributeIdentifier("Faction", Identifier.Empty);
			Identifier[] locationTypeStr = element.GetAttributeIdentifierArray("locationtype", null, true);
			if (locationTypeStr != null)
			{
				this.LocationTypeIdentifiers = locationTypeStr.ToImmutableArray<Identifier>();
			}
			this.MinIntensity = element.GetAttributeFloat("minintensity", 0f);
			this.MaxIntensity = Math.Max(element.GetAttributeFloat("maxintensity", 100f), this.MinIntensity);
			this.ChooseRandom = element.GetAttributeBool("chooserandom", false);
			this.eventCount = element.GetAttributeInt("eventcount", 1);
			this.SubSetCount = element.GetAttributeInt("setcount", 1);
			this.Exhaustible = element.GetAttributeBool("exhaustible", parentSet != null && parentSet.Exhaustible);
			this.MinDistanceTraveled = element.GetAttributeFloat("mindistancetraveled", 0f);
			this.MinMissionTime = element.GetAttributeFloat("minmissiontime", 0f);
			this.AllowAtStart = element.GetAttributeBool("allowatstart", parentSet != null && parentSet.AllowAtStart);
			this.PerRuin = element.GetAttributeBool("perruin", false);
			this.PerCave = element.GetAttributeBool("percave", false);
			this.PerWreck = element.GetAttributeBool("perwreck", false);
			this.DisableInHuntingGrounds = element.GetAttributeBool("disableinhuntinggrounds", parentSet != null && parentSet.DisableInHuntingGrounds);
			this.IgnoreCoolDown = element.GetAttributeBool("ignorecooldown", (parentSet != null) ? parentSet.IgnoreCoolDown : (this.PerRuin || this.PerCave || this.PerWreck));
			this.IgnoreIntensity = element.GetAttributeBool("ignoreintensity", parentSet != null && parentSet.IgnoreIntensity);
			this.DelayWhenCrewAway = element.GetAttributeBool("delaywhencrewaway", (parentSet != null) ? parentSet.DelayWhenCrewAway : (!this.PerRuin && !this.PerCave && !this.PerWreck));
			this.OncePerLevel = element.GetAttributeBool("onceperlevel", element.GetAttributeBool("onceperoutpost", parentSet != null && parentSet.OncePerLevel));
			this.TriggerEventCooldown = element.GetAttributeBool("triggereventcooldown", parentSet == null || parentSet.TriggerEventCooldown);
			this.IsCampaignSet = element.GetAttributeBool("campaign", this.LevelType == LevelData.LevelType.Outpost || (parentSet != null && parentSet.IsCampaignSet));
			this.ResetTime = element.GetAttributeFloat("ResetTime", (parentSet != null) ? parentSet.ResetTime : 0f);
			this.CampaignTutorialOnly = element.GetAttributeBool("CampaignTutorialOnly", parentSet != null && parentSet.CampaignTutorialOnly);
			this.RequiredLayer = element.GetAttributeIdentifier("RequiredLayer", Identifier.Empty);
			this.RequiredSpawnPointTag = element.GetAttributeIdentifier("RequiredSpawnPointTag", Identifier.Empty);
			this.ForceAtDiscoveredNr = element.GetAttributeInt("ForceAtDiscoveredNr", -1);
			this.ForceAtVisitedNr = element.GetAttributeInt("ForceAtVisitedNr", -1);
			if (this.ForceAtDiscoveredNr >= 0 && this.ForceAtVisitedNr >= 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(123, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error with event set \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\" - both ForceAtDiscoveredNr and ForceAtVisitedNr are defined, this could lead to unexpected behavior");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			this.DefaultCommonness = element.GetAttributeFloat("commonness", 1f);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "commonness"))
				{
					if (!(a == "eventset"))
					{
						if (!(a == "overrideeventcount"))
						{
							if (!subElement.HasElements && subElement.Attributes().First<XAttribute>().Name.ToString().Equals("identifier", StringComparison.OrdinalIgnoreCase))
							{
								Identifier[] identifiers = subElement.GetAttributeIdentifierArray("identifier", Array.Empty<Identifier>(), true);
								float commonness = subElement.GetAttributeFloat("commonness", -1f);
								float probability = subElement.GetAttributeFloat("probability", -1f);
								Identifier factionId = subElement.GetAttributeIdentifier("Faction", Identifier.Empty);
								eventPrefabs.Add(new EventSet.SubEventPrefab(identifiers, (commonness >= 0f) ? new float?(commonness) : null, (probability >= 0f) ? new float?(probability) : null, factionId));
								continue;
							}
							ContentXElement element2 = subElement;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 2);
							defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
							defaultInterpolatedStringHandler3.AppendLiteral("-");
							defaultInterpolatedStringHandler3.AppendFormatted<int>(subElement.ElementsBeforeSelf().Count<ContentXElement>());
							EventPrefab prefab = new EventPrefab(element2, file, defaultInterpolatedStringHandler3.ToStringAndClear().ToIdentifier());
							eventPrefabs.Add(new EventSet.SubEventPrefab(prefab, new float?(prefab.Commonness), new float?(prefab.Probability), prefab.Faction));
							continue;
						}
						else
						{
							Identifier locationType = subElement.GetAttributeIdentifier("locationtype", "");
							if (!this.overrideEventCount.ContainsKey(locationType))
							{
								this.overrideEventCount.Add(locationType, subElement.GetAttributeInt("eventcount", this.eventCount));
								continue;
							}
							continue;
						}
					}
				}
				else
				{
					this.DefaultCommonness = subElement.GetAttributeFloat("commonness", this.DefaultCommonness);
					using (IEnumerator<ContentXElement> enumerator2 = subElement.Elements().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							ContentXElement cxe = enumerator2.Current;
							XElement overrideElement = cxe;
							Identifier identifier = overrideElement.NameAsIdentifier();
							if (identifier == "override")
							{
								Identifier levelType = overrideElement.GetAttributeIdentifier("leveltype", "");
								if (!overrideCommonness.ContainsKey(levelType))
								{
									overrideCommonness.Add(levelType, overrideElement.GetAttributeFloat("commonness", 0f));
								}
							}
						}
						continue;
					}
				}
				childSets.Add(new EventSet(subElement, file, this));
			}
			this.EventPrefabs = eventPrefabs.ToImmutableArray<EventSet.SubEventPrefab>();
			this.ChildSets = childSets.ToImmutableArray<EventSet>();
			this.OverrideCommonness = overrideCommonness.ToImmutableDictionary<Identifier, float>();
			if ((this.PerRuin && this.PerCave) || (this.PerWreck && this.PerCave) || (this.PerRuin && this.PerWreck))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(82, 4);
				defaultInterpolatedStringHandler4.AppendLiteral("Error in event set \"");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler4.AppendLiteral("\". Only one of the settings ");
				defaultInterpolatedStringHandler4.AppendFormatted("PerRuin");
				defaultInterpolatedStringHandler4.AppendLiteral(", ");
				defaultInterpolatedStringHandler4.AppendFormatted("PerCave");
				defaultInterpolatedStringHandler4.AppendLiteral(" or ");
				defaultInterpolatedStringHandler4.AppendFormatted("PerWreck");
				defaultInterpolatedStringHandler4.AppendLiteral(" can be enabled at the time.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler4.ToStringAndClear(), null);
			}
		}

		// Token: 0x06002164 RID: 8548 RVA: 0x000E034C File Offset: 0x000DE54C
		public void CheckLocationTypeErrors()
		{
			if (new ImmutableArray<Identifier>?(this.LocationTypeIdentifiers) == null)
			{
				return;
			}
			foreach (Identifier locationTypeId in this.LocationTypeIdentifiers)
			{
				if (!LocationType.Prefabs.ContainsKey(locationTypeId))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in event set \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". Location type \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(locationTypeId);
					defaultInterpolatedStringHandler.AppendLiteral("\" not found.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
		}

		// Token: 0x06002165 RID: 8549 RVA: 0x000E03F4 File Offset: 0x000DE5F4
		public float GetCommonness(Level level)
		{
			LevelGenerationParams generationParams = level.GenerationParams;
			Identifier? identifier = (generationParams != null) ? new Identifier?(generationParams.Identifier) : null;
			float generationParamsCommonness;
			if (identifier != null && !identifier.GetValueOrDefault().IsEmpty && this.OverrideCommonness.TryGetValue(level.GenerationParams.Identifier, out generationParamsCommonness))
			{
				return generationParamsCommonness;
			}
			Submarine startOutpost = level.StartOutpost;
			Identifier? identifier2;
			if (startOutpost == null)
			{
				identifier2 = null;
			}
			else
			{
				OutpostGenerationParams outpostGenerationParams = startOutpost.Info.OutpostGenerationParams;
				identifier2 = ((outpostGenerationParams != null) ? new Identifier?(outpostGenerationParams.Identifier) : null);
			}
			identifier = identifier2;
			float startOutpostParamsCommonness;
			if (identifier != null && !identifier.GetValueOrDefault().IsEmpty && this.OverrideCommonness.TryGetValue(level.StartOutpost.Info.OutpostGenerationParams.Identifier, out startOutpostParamsCommonness))
			{
				return startOutpostParamsCommonness;
			}
			Submarine endOutpost = level.EndOutpost;
			Identifier? identifier3;
			if (endOutpost == null)
			{
				identifier3 = null;
			}
			else
			{
				OutpostGenerationParams outpostGenerationParams2 = endOutpost.Info.OutpostGenerationParams;
				identifier3 = ((outpostGenerationParams2 != null) ? new Identifier?(outpostGenerationParams2.Identifier) : null);
			}
			identifier = identifier3;
			float endOutpostParamsCommonness;
			if (identifier != null && !identifier.GetValueOrDefault().IsEmpty && this.OverrideCommonness.TryGetValue(level.EndOutpost.Info.OutpostGenerationParams.Identifier, out endOutpostParamsCommonness))
			{
				return endOutpostParamsCommonness;
			}
			return this.DefaultCommonness;
		}

		// Token: 0x06002166 RID: 8550 RVA: 0x000E0554 File Offset: 0x000DE754
		public int GetEventCount(Level level)
		{
			int finishedEventCount = 0;
			if (level != null)
			{
				level.LevelData.FinishedEvents.TryGetValue(this, out finishedEventCount);
			}
			int count;
			if (level.StartLocation == null || !this.overrideEventCount.TryGetValue(level.StartLocation.Type.Identifier, out count))
			{
				return this.eventCount - finishedEventCount;
			}
			return count - finishedEventCount;
		}

		// Token: 0x06002167 RID: 8551 RVA: 0x000E05B0 File Offset: 0x000DE7B0
		public static List<string> GetDebugStatistics(int simulatedRoundCount = 100, Func<MonsterEvent, bool> filter = null, bool fullLog = false)
		{
			List<string> debugLines = new List<string>();
			foreach (EventSet eventSet in EventSet.Prefabs)
			{
				List<EventSet.EventDebugStats> stats = new List<EventSet.EventDebugStats>();
				for (int i = 0; i < simulatedRoundCount; i++)
				{
					EventSet.EventDebugStats newStats = new EventSet.EventDebugStats(eventSet);
					EventSet.<GetDebugStatistics>g__CheckEventSet|51_0(newStats, eventSet, filter);
					stats.Add(newStats);
				}
				List<string> list = debugLines;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Event stats (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventSet.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("): ");
				list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				EventSet.<GetDebugStatistics>g__LogEventStats|51_3(stats, debugLines, fullLog);
			}
			return debugLines;
		}

		// Token: 0x06002168 RID: 8552 RVA: 0x000E0674 File Offset: 0x000DE874
		public override string ToString()
		{
			return base.ToString() + " (" + this.Identifier.Value + ")";
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x000E0696 File Offset: 0x000DE896
		public override void Dispose()
		{
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x000E06B0 File Offset: 0x000DE8B0
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__CheckEventSet|51_0(EventSet.EventDebugStats stats, EventSet thisSet, Func<MonsterEvent, bool> filter = null)
		{
			if (thisSet.ChooseRandom)
			{
				List<EventSet.SubEventPrefab> unusedEvents = thisSet.EventPrefabs.ToList<EventSet.SubEventPrefab>();
				if (unusedEvents.Any<EventSet.SubEventPrefab>())
				{
					for (int i = 0; i < thisSet.eventCount; i++)
					{
						EventSet.SubEventPrefab eventPrefab = ToolBox.SelectWeightedRandom<EventSet.SubEventPrefab>(unusedEvents, (from e in unusedEvents
						select e.Commonness).ToList<float>(), Rand.RandSync.Unsynced);
						if (eventPrefab.EventPrefabs.Any((EventPrefab p) => p != null))
						{
							EventSet.<GetDebugStatistics>g__AddEvents|51_1(stats, eventPrefab.EventPrefabs, filter);
							unusedEvents.Remove(eventPrefab);
						}
					}
				}
				List<float> values = thisSet.ChildSets.SelectMany((EventSet s) => s.DefaultCommonness.ToEnumerable<float>().Concat(s.OverrideCommonness.Values)).ToList<float>();
				EventSet childSet = ToolBox.SelectWeightedRandom<EventSet>(thisSet.ChildSets, values, Rand.RandSync.Unsynced);
				if (childSet != null)
				{
					EventSet.<GetDebugStatistics>g__CheckEventSet|51_0(stats, childSet, filter);
					return;
				}
			}
			else
			{
				foreach (EventSet.SubEventPrefab eventPrefab2 in thisSet.EventPrefabs)
				{
					EventSet.<GetDebugStatistics>g__AddEvents|51_1(stats, eventPrefab2.EventPrefabs, filter);
				}
				foreach (EventSet childSet2 in thisSet.ChildSets)
				{
					EventSet.<GetDebugStatistics>g__CheckEventSet|51_0(stats, childSet2, filter);
				}
			}
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x000E081C File Offset: 0x000DEA1C
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__AddEvents|51_1(EventSet.EventDebugStats stats, IEnumerable<EventPrefab> eventPrefabs, Func<MonsterEvent, bool> filter = null)
		{
			eventPrefabs.ForEach(delegate(EventPrefab p)
			{
				EventSet.<GetDebugStatistics>g__AddEvent|51_2(stats, p, filter);
			});
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x000E0850 File Offset: 0x000DEA50
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__AddEvent|51_2(EventSet.EventDebugStats stats, EventPrefab eventPrefab, Func<MonsterEvent, bool> filter = null)
		{
			if (eventPrefab.EventType == typeof(MonsterEvent))
			{
				GameSession gameSession = GameMain.GameSession;
				int? num;
				if (gameSession == null)
				{
					num = null;
				}
				else
				{
					EventManager eventManager = gameSession.EventManager;
					num = ((eventManager != null) ? new int?(eventManager.RandomSeed) : null);
				}
				int? num2 = num;
				MonsterEvent monsterEvent;
				if (eventPrefab.TryCreateInstance<MonsterEvent>(num2.GetValueOrDefault(), out monsterEvent))
				{
					if (filter != null && !filter(monsterEvent))
					{
						return;
					}
					EventPrefab prefab = monsterEvent.Prefab;
					float spawnProbability = (prefab != null) ? prefab.Probability : 0f;
					if (Rand.Value(Rand.RandSync.Unsynced) > spawnProbability)
					{
						return;
					}
					int count = Rand.Range(monsterEvent.MinAmount, monsterEvent.MaxAmount + 1, Rand.RandSync.Unsynced);
					if (count <= 0)
					{
						return;
					}
					Identifier character = monsterEvent.SpeciesName;
					int currentCount;
					if (stats.MonsterCounts.TryGetValue(character, out currentCount))
					{
						if (currentCount >= monsterEvent.MaxAmountPerLevel)
						{
							return;
						}
					}
					else
					{
						stats.MonsterCounts[character] = 0;
					}
					Dictionary<Identifier, int> monsterCounts = stats.MonsterCounts;
					Identifier key = character;
					monsterCounts[key] += count;
					CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(character);
					ContentXElement contentXElement;
					if (characterPrefab == null)
					{
						contentXElement = null;
					}
					else
					{
						ContentXElement configElement = characterPrefab.ConfigElement;
						contentXElement = ((configElement != null) ? configElement.GetChildElement("ai") : null);
					}
					ContentXElement aiElement = contentXElement;
					ContentXElement contentXElement2 = null;
					if (aiElement != contentXElement2)
					{
						stats.MonsterStrength += aiElement.GetAttributeFloat("combatstrength", 0f) * (float)count;
					}
				}
			}
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x000E09B0 File Offset: 0x000DEBB0
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__LogEventStats|51_3(List<EventSet.EventDebugStats> stats, List<string> debugLines, bool fullLog)
		{
			if (stats.Count != 0)
			{
				if (!stats.All((EventSet.EventDebugStats s) => s.MonsterCounts.Values.Sum() == 0))
				{
					Dictionary<Identifier, int> allMonsters = new Dictionary<Identifier, int>();
					foreach (EventSet.EventDebugStats stat in stats)
					{
						foreach (KeyValuePair<Identifier, int> monster in stat.MonsterCounts)
						{
							if (!allMonsters.TryAdd(monster.Key, monster.Value))
							{
								Dictionary<Identifier, int> dictionary = allMonsters;
								Identifier key = monster.Key;
								dictionary[key] += monster.Value;
							}
						}
					}
					allMonsters = (from m in allMonsters
					orderby m.Key
					select m).ToDictionary((KeyValuePair<Identifier, int> m) => m.Key, (KeyValuePair<Identifier, int> m) => m.Value);
					stats.Sort((EventSet.EventDebugStats s1, EventSet.EventDebugStats s2) => s1.MonsterCounts.Values.Sum().CompareTo(s2.MonsterCounts.Values.Sum()));
					List<string> debugLines2 = debugLines;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 3);
					defaultInterpolatedStringHandler.AppendLiteral("  Average monster count: ");
					defaultInterpolatedStringHandler.AppendFormatted(((float)stats.Average((EventSet.EventDebugStats s) => s.MonsterCounts.Values.Sum())).FormatZeroDecimal());
					defaultInterpolatedStringHandler.AppendLiteral(" (Min: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(stats.First<EventSet.EventDebugStats>().MonsterCounts.Values.Sum());
					defaultInterpolatedStringHandler.AppendLiteral(", Max: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(stats.Last<EventSet.EventDebugStats>().MonsterCounts.Values.Sum());
					defaultInterpolatedStringHandler.AppendLiteral(")");
					debugLines2.Add(defaultInterpolatedStringHandler.ToStringAndClear());
					debugLines.Add("     " + EventSet.<GetDebugStatistics>g__LogMonsterCounts|51_4(allMonsters, (float)stats.Count));
					if (fullLog)
					{
						debugLines.Add("  All samples:");
						stats.ForEach(delegate(EventSet.EventDebugStats s)
						{
							debugLines.Add("     " + EventSet.<GetDebugStatistics>g__LogMonsterCounts|51_4(s.MonsterCounts, 0f));
						});
					}
					stats.Sort((EventSet.EventDebugStats s1, EventSet.EventDebugStats s2) => s1.MonsterStrength.CompareTo(s2.MonsterStrength));
					List<string> debugLines3 = debugLines;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("  Average monster strength: ");
					defaultInterpolatedStringHandler2.AppendFormatted(stats.Average((EventSet.EventDebugStats s) => s.MonsterStrength).FormatZeroDecimal());
					defaultInterpolatedStringHandler2.AppendLiteral(" (Min: ");
					defaultInterpolatedStringHandler2.AppendFormatted(stats.First<EventSet.EventDebugStats>().MonsterStrength.FormatZeroDecimal());
					defaultInterpolatedStringHandler2.AppendLiteral(", Max: ");
					defaultInterpolatedStringHandler2.AppendFormatted(stats.Last<EventSet.EventDebugStats>().MonsterStrength.FormatZeroDecimal());
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					debugLines3.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
					debugLines.Add(" ");
					return;
				}
			}
			debugLines.Add("  No monster spawns");
			debugLines.Add(" ");
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x000E0D40 File Offset: 0x000DEF40
		[CompilerGenerated]
		internal static string <GetDebugStatistics>g__LogMonsterCounts|51_4(Dictionary<Identifier, int> stats, float divider = 0f)
		{
			if (divider > 0f)
			{
				return string.Join("\n     ", from mc in stats
				select mc.Key.ToString() + " x " + ((float)mc.Value / divider).FormatSingleDecimal());
			}
			return string.Join(", ", from mc in stats
			select mc.Key.ToString() + " x " + mc.Value.ToString());
		}

		// Token: 0x04000FC9 RID: 4041
		public static readonly PrefabCollection<EventSet> Prefabs = new PrefabCollection<EventSet>();

		// Token: 0x04000FCA RID: 4042
		private static readonly Dictionary<Identifier, EventPrefab> AllEventPrefabs = new Dictionary<Identifier, EventPrefab>();

		// Token: 0x04000FCB RID: 4043
		public readonly bool IsCampaignSet;

		// Token: 0x04000FCC RID: 4044
		public readonly float MinLevelDifficulty;

		// Token: 0x04000FCD RID: 4045
		public readonly float MaxLevelDifficulty;

		// Token: 0x04000FCE RID: 4046
		public readonly Identifier BiomeIdentifier;

		// Token: 0x04000FCF RID: 4047
		public readonly LevelData.LevelType LevelType;

		// Token: 0x04000FD0 RID: 4048
		public readonly Identifier RequiredLayer;

		// Token: 0x04000FD1 RID: 4049
		public readonly Identifier RequiredSpawnPointTag;

		// Token: 0x04000FD2 RID: 4050
		public readonly ImmutableArray<Identifier> LocationTypeIdentifiers;

		// Token: 0x04000FD3 RID: 4051
		public readonly Identifier Faction;

		// Token: 0x04000FD4 RID: 4052
		public readonly bool ChooseRandom;

		// Token: 0x04000FD5 RID: 4053
		private readonly int eventCount = 1;

		// Token: 0x04000FD6 RID: 4054
		public readonly int SubSetCount = 1;

		// Token: 0x04000FD7 RID: 4055
		private readonly Dictionary<Identifier, int> overrideEventCount = new Dictionary<Identifier, int>();

		// Token: 0x04000FD8 RID: 4056
		public readonly bool Exhaustible;

		// Token: 0x04000FD9 RID: 4057
		public readonly float MinDistanceTraveled;

		// Token: 0x04000FDA RID: 4058
		public readonly float MinMissionTime;

		// Token: 0x04000FDB RID: 4059
		public readonly float MinIntensity;

		// Token: 0x04000FDC RID: 4060
		public readonly float MaxIntensity;

		// Token: 0x04000FDD RID: 4061
		public readonly bool AllowAtStart;

		// Token: 0x04000FDE RID: 4062
		public readonly bool IgnoreCoolDown;

		// Token: 0x04000FDF RID: 4063
		public readonly bool TriggerEventCooldown;

		// Token: 0x04000FE0 RID: 4064
		public readonly bool IgnoreIntensity;

		// Token: 0x04000FE1 RID: 4065
		public readonly bool PerRuin;

		// Token: 0x04000FE2 RID: 4066
		public readonly bool PerCave;

		// Token: 0x04000FE3 RID: 4067
		public readonly bool PerWreck;

		// Token: 0x04000FE4 RID: 4068
		public readonly bool DisableInHuntingGrounds;

		// Token: 0x04000FE5 RID: 4069
		public readonly bool OncePerLevel;

		// Token: 0x04000FE6 RID: 4070
		public readonly bool DelayWhenCrewAway;

		// Token: 0x04000FE7 RID: 4071
		public readonly bool Additive;

		// Token: 0x04000FE8 RID: 4072
		public readonly bool SelectAlways;

		// Token: 0x04000FE9 RID: 4073
		public readonly float DefaultCommonness;

		// Token: 0x04000FEA RID: 4074
		public readonly ImmutableDictionary<Identifier, float> OverrideCommonness;

		// Token: 0x04000FEB RID: 4075
		public readonly float ResetTime;

		// Token: 0x04000FEC RID: 4076
		public readonly int ForceAtDiscoveredNr;

		// Token: 0x04000FED RID: 4077
		public readonly int ForceAtVisitedNr;

		// Token: 0x04000FEE RID: 4078
		public readonly bool CampaignTutorialOnly;

		// Token: 0x04000FEF RID: 4079
		public readonly ImmutableArray<EventSet.SubEventPrefab> EventPrefabs;

		// Token: 0x04000FF0 RID: 4080
		public readonly ImmutableArray<EventSet> ChildSets;

		// Token: 0x02000945 RID: 2373
		internal class EventDebugStats
		{
			// Token: 0x06005917 RID: 22807 RVA: 0x001F8649 File Offset: 0x001F6849
			public EventDebugStats(EventSet rootSet)
			{
				this.RootSet = rootSet;
			}

			// Token: 0x040032A0 RID: 12960
			public readonly EventSet RootSet;

			// Token: 0x040032A1 RID: 12961
			public readonly Dictionary<Identifier, int> MonsterCounts = new Dictionary<Identifier, int>();

			// Token: 0x040032A2 RID: 12962
			public float MonsterStrength;
		}

		// Token: 0x02000946 RID: 2374
		public readonly struct SubEventPrefab
		{
			// Token: 0x06005918 RID: 22808 RVA: 0x001F8663 File Offset: 0x001F6863
			public SubEventPrefab(Either<Identifier[], EventPrefab> prefabOrIdentifiers, float? commonness, float? probability, Identifier factionId)
			{
				this.PrefabOrIdentifier = prefabOrIdentifiers;
				this.SelfCommonness = commonness;
				this.SelfProbability = probability;
				this.Faction = factionId;
			}

			// Token: 0x1700154C RID: 5452
			// (get) Token: 0x06005919 RID: 22809 RVA: 0x001F8684 File Offset: 0x001F6884
			public IEnumerable<EventPrefab> EventPrefabs
			{
				get
				{
					EventSet.SubEventPrefab.<get_EventPrefabs>d__3 <get_EventPrefabs>d__ = new EventSet.SubEventPrefab.<get_EventPrefabs>d__3(-2);
					<get_EventPrefabs>d__.<>3__<>4__this = this;
					return <get_EventPrefabs>d__;
				}
			}

			// Token: 0x1700154D RID: 5453
			// (get) Token: 0x0600591A RID: 22810 RVA: 0x001F86A8 File Offset: 0x001F68A8
			public float Commonness
			{
				get
				{
					float? selfCommonness = this.SelfCommonness;
					if (selfCommonness == null)
					{
						return this.EventPrefabs.MaxOrNull((EventPrefab p) => p.Commonness).GetValueOrDefault();
					}
					return selfCommonness.GetValueOrDefault();
				}
			}

			// Token: 0x1700154E RID: 5454
			// (get) Token: 0x0600591B RID: 22811 RVA: 0x001F8700 File Offset: 0x001F6900
			public float Probability
			{
				get
				{
					float? selfProbability = this.SelfProbability;
					if (selfProbability == null)
					{
						return this.EventPrefabs.MaxOrNull((EventPrefab p) => p.Probability).GetValueOrDefault();
					}
					return selfProbability.GetValueOrDefault();
				}
			}

			// Token: 0x0600591C RID: 22812 RVA: 0x001F8757 File Offset: 0x001F6957
			public void Deconstruct(out IEnumerable<EventPrefab> eventPrefabs, out float commonness, out float probability)
			{
				eventPrefabs = this.EventPrefabs;
				commonness = this.Commonness;
				probability = this.Probability;
			}

			// Token: 0x0600591D RID: 22813 RVA: 0x001F8771 File Offset: 0x001F6971
			public IEnumerable<Identifier> GetMissingIdentifiers()
			{
				EventSet.SubEventPrefab.<GetMissingIdentifiers>d__12 <GetMissingIdentifiers>d__ = new EventSet.SubEventPrefab.<GetMissingIdentifiers>d__12(-2);
				<GetMissingIdentifiers>d__.<>3__<>4__this = this;
				return <GetMissingIdentifiers>d__;
			}

			// Token: 0x040032A3 RID: 12963
			public readonly Either<Identifier[], EventPrefab> PrefabOrIdentifier;

			// Token: 0x040032A4 RID: 12964
			public readonly float? SelfCommonness;

			// Token: 0x040032A5 RID: 12965
			public readonly float? SelfProbability;

			// Token: 0x040032A6 RID: 12966
			public readonly Identifier Faction;
		}
	}
}
