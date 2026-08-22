using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002B5 RID: 693
	internal sealed class EventSet : Prefab
	{
		// Token: 0x06003BFF RID: 15359 RVA: 0x00225704 File Offset: 0x00223904
		public static Sprite GetEventSprite(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return null;
			}
			EventSprite sprite;
			if (EventSprite.Prefabs.TryGet(identifier.ToIdentifier(), out sprite))
			{
				return sprite.Sprite;
			}
			DebugConsole.AddWarning("Could not find the event sprite \"" + identifier + "\"", null);
			return null;
		}

		// Token: 0x06003C00 RID: 15360 RVA: 0x0022574D File Offset: 0x0022394D
		public static IEnumerable<EventPrefab> GetAllEventPrefabs()
		{
			return EventSet.AllEventPrefabs.Values;
		}

		// Token: 0x06003C01 RID: 15361 RVA: 0x0022575C File Offset: 0x0022395C
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

		// Token: 0x06003C02 RID: 15362 RVA: 0x002257F8 File Offset: 0x002239F8
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

		// Token: 0x06003C03 RID: 15363 RVA: 0x00225898 File Offset: 0x00223A98
		public static EventPrefab GetEventPrefab(Identifier identifier)
		{
			return EventSet.AllEventPrefabs.GetValueOrDefault(identifier);
		}

		// Token: 0x06003C04 RID: 15364 RVA: 0x002258A8 File Offset: 0x00223AA8
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

		// Token: 0x06003C05 RID: 15365 RVA: 0x00225A04 File Offset: 0x00223C04
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

		// Token: 0x06003C06 RID: 15366 RVA: 0x00226280 File Offset: 0x00224480
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

		// Token: 0x06003C07 RID: 15367 RVA: 0x00226328 File Offset: 0x00224528
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

		// Token: 0x06003C08 RID: 15368 RVA: 0x00226488 File Offset: 0x00224688
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

		// Token: 0x06003C09 RID: 15369 RVA: 0x002264E4 File Offset: 0x002246E4
		public static List<string> GetDebugStatistics(int simulatedRoundCount = 100, Func<MonsterEvent, bool> filter = null, bool fullLog = false)
		{
			List<string> debugLines = new List<string>();
			foreach (EventSet eventSet in EventSet.Prefabs)
			{
				List<EventSet.EventDebugStats> stats = new List<EventSet.EventDebugStats>();
				for (int i = 0; i < simulatedRoundCount; i++)
				{
					EventSet.EventDebugStats newStats = new EventSet.EventDebugStats(eventSet);
					EventSet.<GetDebugStatistics>g__CheckEventSet|52_0(newStats, eventSet, filter);
					stats.Add(newStats);
				}
				List<string> list = debugLines;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Event stats (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventSet.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("): ");
				list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				EventSet.<GetDebugStatistics>g__LogEventStats|52_3(stats, debugLines, fullLog);
			}
			return debugLines;
		}

		// Token: 0x06003C0A RID: 15370 RVA: 0x002265A8 File Offset: 0x002247A8
		public override string ToString()
		{
			return base.ToString() + " (" + this.Identifier.Value + ")";
		}

		// Token: 0x06003C0B RID: 15371 RVA: 0x002265CA File Offset: 0x002247CA
		public override void Dispose()
		{
		}

		// Token: 0x06003C0D RID: 15373 RVA: 0x002265E4 File Offset: 0x002247E4
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__CheckEventSet|52_0(EventSet.EventDebugStats stats, EventSet thisSet, Func<MonsterEvent, bool> filter = null)
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
							EventSet.<GetDebugStatistics>g__AddEvents|52_1(stats, eventPrefab.EventPrefabs, filter);
							unusedEvents.Remove(eventPrefab);
						}
					}
				}
				List<float> values = thisSet.ChildSets.SelectMany((EventSet s) => s.DefaultCommonness.ToEnumerable<float>().Concat(s.OverrideCommonness.Values)).ToList<float>();
				EventSet childSet = ToolBox.SelectWeightedRandom<EventSet>(thisSet.ChildSets, values, Rand.RandSync.Unsynced);
				if (childSet != null)
				{
					EventSet.<GetDebugStatistics>g__CheckEventSet|52_0(stats, childSet, filter);
					return;
				}
			}
			else
			{
				foreach (EventSet.SubEventPrefab eventPrefab2 in thisSet.EventPrefabs)
				{
					EventSet.<GetDebugStatistics>g__AddEvents|52_1(stats, eventPrefab2.EventPrefabs, filter);
				}
				foreach (EventSet childSet2 in thisSet.ChildSets)
				{
					EventSet.<GetDebugStatistics>g__CheckEventSet|52_0(stats, childSet2, filter);
				}
			}
		}

		// Token: 0x06003C0E RID: 15374 RVA: 0x00226750 File Offset: 0x00224950
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__AddEvents|52_1(EventSet.EventDebugStats stats, IEnumerable<EventPrefab> eventPrefabs, Func<MonsterEvent, bool> filter = null)
		{
			eventPrefabs.ForEach(delegate(EventPrefab p)
			{
				EventSet.<GetDebugStatistics>g__AddEvent|52_2(stats, p, filter);
			});
		}

		// Token: 0x06003C0F RID: 15375 RVA: 0x00226784 File Offset: 0x00224984
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__AddEvent|52_2(EventSet.EventDebugStats stats, EventPrefab eventPrefab, Func<MonsterEvent, bool> filter = null)
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

		// Token: 0x06003C10 RID: 15376 RVA: 0x002268E4 File Offset: 0x00224AE4
		[CompilerGenerated]
		internal static void <GetDebugStatistics>g__LogEventStats|52_3(List<EventSet.EventDebugStats> stats, List<string> debugLines, bool fullLog)
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
					debugLines.Add("     " + EventSet.<GetDebugStatistics>g__LogMonsterCounts|52_4(allMonsters, (float)stats.Count));
					if (fullLog)
					{
						debugLines.Add("  All samples:");
						stats.ForEach(delegate(EventSet.EventDebugStats s)
						{
							debugLines.Add("     " + EventSet.<GetDebugStatistics>g__LogMonsterCounts|52_4(s.MonsterCounts, 0f));
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

		// Token: 0x06003C11 RID: 15377 RVA: 0x00226C74 File Offset: 0x00224E74
		[CompilerGenerated]
		internal static string <GetDebugStatistics>g__LogMonsterCounts|52_4(Dictionary<Identifier, int> stats, float divider = 0f)
		{
			if (divider > 0f)
			{
				return string.Join("\n     ", from mc in stats
				select mc.Key.ToString() + " x " + ((float)mc.Value / divider).FormatSingleDecimal());
			}
			return string.Join(", ", from mc in stats
			select mc.Key.ToString() + " x " + mc.Value.ToString());
		}

		// Token: 0x04001EAF RID: 7855
		public static readonly PrefabCollection<EventSet> Prefabs = new PrefabCollection<EventSet>();

		// Token: 0x04001EB0 RID: 7856
		private static readonly Dictionary<Identifier, EventPrefab> AllEventPrefabs = new Dictionary<Identifier, EventPrefab>();

		// Token: 0x04001EB1 RID: 7857
		public readonly bool IsCampaignSet;

		// Token: 0x04001EB2 RID: 7858
		public readonly float MinLevelDifficulty;

		// Token: 0x04001EB3 RID: 7859
		public readonly float MaxLevelDifficulty;

		// Token: 0x04001EB4 RID: 7860
		public readonly Identifier BiomeIdentifier;

		// Token: 0x04001EB5 RID: 7861
		public readonly LevelData.LevelType LevelType;

		// Token: 0x04001EB6 RID: 7862
		public readonly Identifier RequiredLayer;

		// Token: 0x04001EB7 RID: 7863
		public readonly Identifier RequiredSpawnPointTag;

		// Token: 0x04001EB8 RID: 7864
		public readonly ImmutableArray<Identifier> LocationTypeIdentifiers;

		// Token: 0x04001EB9 RID: 7865
		public readonly Identifier Faction;

		// Token: 0x04001EBA RID: 7866
		public readonly bool ChooseRandom;

		// Token: 0x04001EBB RID: 7867
		private readonly int eventCount = 1;

		// Token: 0x04001EBC RID: 7868
		public readonly int SubSetCount = 1;

		// Token: 0x04001EBD RID: 7869
		private readonly Dictionary<Identifier, int> overrideEventCount = new Dictionary<Identifier, int>();

		// Token: 0x04001EBE RID: 7870
		public readonly bool Exhaustible;

		// Token: 0x04001EBF RID: 7871
		public readonly float MinDistanceTraveled;

		// Token: 0x04001EC0 RID: 7872
		public readonly float MinMissionTime;

		// Token: 0x04001EC1 RID: 7873
		public readonly float MinIntensity;

		// Token: 0x04001EC2 RID: 7874
		public readonly float MaxIntensity;

		// Token: 0x04001EC3 RID: 7875
		public readonly bool AllowAtStart;

		// Token: 0x04001EC4 RID: 7876
		public readonly bool IgnoreCoolDown;

		// Token: 0x04001EC5 RID: 7877
		public readonly bool TriggerEventCooldown;

		// Token: 0x04001EC6 RID: 7878
		public readonly bool IgnoreIntensity;

		// Token: 0x04001EC7 RID: 7879
		public readonly bool PerRuin;

		// Token: 0x04001EC8 RID: 7880
		public readonly bool PerCave;

		// Token: 0x04001EC9 RID: 7881
		public readonly bool PerWreck;

		// Token: 0x04001ECA RID: 7882
		public readonly bool DisableInHuntingGrounds;

		// Token: 0x04001ECB RID: 7883
		public readonly bool OncePerLevel;

		// Token: 0x04001ECC RID: 7884
		public readonly bool DelayWhenCrewAway;

		// Token: 0x04001ECD RID: 7885
		public readonly bool Additive;

		// Token: 0x04001ECE RID: 7886
		public readonly bool SelectAlways;

		// Token: 0x04001ECF RID: 7887
		public readonly float DefaultCommonness;

		// Token: 0x04001ED0 RID: 7888
		public readonly ImmutableDictionary<Identifier, float> OverrideCommonness;

		// Token: 0x04001ED1 RID: 7889
		public readonly float ResetTime;

		// Token: 0x04001ED2 RID: 7890
		public readonly int ForceAtDiscoveredNr;

		// Token: 0x04001ED3 RID: 7891
		public readonly int ForceAtVisitedNr;

		// Token: 0x04001ED4 RID: 7892
		public readonly bool CampaignTutorialOnly;

		// Token: 0x04001ED5 RID: 7893
		public readonly ImmutableArray<EventSet.SubEventPrefab> EventPrefabs;

		// Token: 0x04001ED6 RID: 7894
		public readonly ImmutableArray<EventSet> ChildSets;

		// Token: 0x02000F54 RID: 3924
		internal class EventDebugStats
		{
			// Token: 0x060088AE RID: 34990 RVA: 0x003A65F6 File Offset: 0x003A47F6
			public EventDebugStats(EventSet rootSet)
			{
				this.RootSet = rootSet;
			}

			// Token: 0x04005556 RID: 21846
			public readonly EventSet RootSet;

			// Token: 0x04005557 RID: 21847
			public readonly Dictionary<Identifier, int> MonsterCounts = new Dictionary<Identifier, int>();

			// Token: 0x04005558 RID: 21848
			public float MonsterStrength;
		}

		// Token: 0x02000F55 RID: 3925
		public readonly struct SubEventPrefab
		{
			// Token: 0x060088AF RID: 34991 RVA: 0x003A6610 File Offset: 0x003A4810
			public SubEventPrefab(Either<Identifier[], EventPrefab> prefabOrIdentifiers, float? commonness, float? probability, Identifier factionId)
			{
				this.PrefabOrIdentifier = prefabOrIdentifiers;
				this.SelfCommonness = commonness;
				this.SelfProbability = probability;
				this.Faction = factionId;
			}

			// Token: 0x17001C1B RID: 7195
			// (get) Token: 0x060088B0 RID: 34992 RVA: 0x003A6630 File Offset: 0x003A4830
			public IEnumerable<EventPrefab> EventPrefabs
			{
				get
				{
					EventSet.SubEventPrefab.<get_EventPrefabs>d__3 <get_EventPrefabs>d__ = new EventSet.SubEventPrefab.<get_EventPrefabs>d__3(-2);
					<get_EventPrefabs>d__.<>3__<>4__this = this;
					return <get_EventPrefabs>d__;
				}
			}

			// Token: 0x17001C1C RID: 7196
			// (get) Token: 0x060088B1 RID: 34993 RVA: 0x003A6654 File Offset: 0x003A4854
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

			// Token: 0x17001C1D RID: 7197
			// (get) Token: 0x060088B2 RID: 34994 RVA: 0x003A66AC File Offset: 0x003A48AC
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

			// Token: 0x060088B3 RID: 34995 RVA: 0x003A6703 File Offset: 0x003A4903
			public void Deconstruct(out IEnumerable<EventPrefab> eventPrefabs, out float commonness, out float probability)
			{
				eventPrefabs = this.EventPrefabs;
				commonness = this.Commonness;
				probability = this.Probability;
			}

			// Token: 0x060088B4 RID: 34996 RVA: 0x003A671D File Offset: 0x003A491D
			public IEnumerable<Identifier> GetMissingIdentifiers()
			{
				EventSet.SubEventPrefab.<GetMissingIdentifiers>d__12 <GetMissingIdentifiers>d__ = new EventSet.SubEventPrefab.<GetMissingIdentifiers>d__12(-2);
				<GetMissingIdentifiers>d__.<>3__<>4__this = this;
				return <GetMissingIdentifiers>d__;
			}

			// Token: 0x04005559 RID: 21849
			public readonly Either<Identifier[], EventPrefab> PrefabOrIdentifier;

			// Token: 0x0400555A RID: 21850
			public readonly float? SelfCommonness;

			// Token: 0x0400555B RID: 21851
			public readonly float? SelfProbability;

			// Token: 0x0400555C RID: 21852
			public readonly Identifier Faction;
		}
	}
}
