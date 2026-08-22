using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200001F RID: 31
	internal class EventManager
	{
		// Token: 0x06000411 RID: 1041 RVA: 0x000213E8 File Offset: 0x0001F5E8
		public static void ServerWriteEventLog(Client client, EventManager.NetEventLogEntry entry)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(5);
			outmsg.WriteNetSerializableStruct(entry);
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			ServerPeer serverPeer = server.ServerPeer;
			if (serverPeer == null)
			{
				return;
			}
			serverPeer.Send(outmsg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00021434 File Offset: 0x0001F634
		public static void ServerWriteObjective(Client client, EventManager.NetEventObjective entry)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(6);
			outmsg.WriteNetSerializableStruct(entry);
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			ServerPeer serverPeer = server.ServerPeer;
			if (serverPeer == null)
			{
				return;
			}
			serverPeer.Send(outmsg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00021480 File Offset: 0x0001F680
		public void ServerRead(IReadMessage inc, Client sender)
		{
			ushort actionId = inc.ReadUInt16();
			byte selectedOption = inc.ReadByte();
			bool isIgnore = selectedOption == byte.MaxValue;
			foreach (Event ev in this.activeEvents)
			{
				ScriptedEvent scriptedEvent = ev as ScriptedEvent;
				if (scriptedEvent != null)
				{
					List<ValueTuple<int, EventAction>> actions = scriptedEvent.GetAllActions();
					foreach (EventAction action in from a in actions
					select a.Item2)
					{
						ConversationAction convAction = action as ConversationAction;
						if (convAction != null && convAction.Identifier == actionId)
						{
							if (!convAction.TargetClients.Contains(sender))
							{
								convAction.IgnoreClient(sender, 3f);
							}
							else
							{
								if (convAction.SelectedOption > -1)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 1);
									defaultInterpolatedStringHandler.AppendLiteral("Client replied to ");
									defaultInterpolatedStringHandler.AppendFormatted<Identifier>(ev.Prefab.Identifier);
									defaultInterpolatedStringHandler.AppendLiteral(", but option already selected for conversation, interrupt for the client");
									DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
									convAction.ServerWrite(convAction.Speaker, sender, true);
									return;
								}
								if (isIgnore)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 1);
									defaultInterpolatedStringHandler2.AppendLiteral("Client ignored ConversationAction (event ");
									defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(ev.Prefab.Identifier);
									defaultInterpolatedStringHandler2.AppendLiteral(").");
									DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
									convAction.IgnoreClient(sender, 3f);
									if (convAction.TargetClients.None(null))
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(42, 1);
										defaultInterpolatedStringHandler3.AppendLiteral("No target clients for event ");
										defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(ev.Prefab.Identifier);
										defaultInterpolatedStringHandler3.AppendLiteral(", retrying in ");
										DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear() + 4f.ToString(), null, false);
										convAction.RetriggerAfter(4f);
									}
								}
								else
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(57, 2);
									defaultInterpolatedStringHandler4.AppendLiteral("Client selected option ");
									defaultInterpolatedStringHandler4.AppendFormatted<byte>(selectedOption);
									defaultInterpolatedStringHandler4.AppendLiteral(" for ConversationAction in event ");
									defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(ev.Prefab.Identifier);
									defaultInterpolatedStringHandler4.AppendLiteral(".");
									DebugConsole.NewMessage(defaultInterpolatedStringHandler4.ToStringAndClear(), null, false);
									convAction.SelectedOption = (int)selectedOption;
									if (convAction.Options.Any<ConversationAction.OptionActionGroup>() && !convAction.GetEndingOptions().Contains((int)selectedOption))
									{
										ConversationAction.OptionActionGroup option = convAction.Options[(int)selectedOption];
										if (option.ForceSay && sender.Character != null)
										{
											sender.Character.ForceSay(option.ForceSayText.IsNullOrEmpty() ? TextManager.Get(option.Text).Fallback(option.Text, true) : TextManager.Get(option.ForceSayText).Fallback(option.ForceSayText, true), option.ForceSayInRadio, option.ForceSayRemoveQuotes, 0f);
										}
										foreach (Client c in convAction.TargetClients)
										{
											if (c != sender)
											{
												convAction.ServerWriteSelectedOption(c);
											}
										}
									}
								}
								return;
							}
						}
					}
				}
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000414 RID: 1044 RVA: 0x00021870 File Offset: 0x0001FA70
		public float CurrentIntensity
		{
			get
			{
				return this.currentIntensity;
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000415 RID: 1045 RVA: 0x00021878 File Offset: 0x0001FA78
		public float MusicIntensity
		{
			get
			{
				return this.musicIntensity;
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000416 RID: 1046 RVA: 0x00021880 File Offset: 0x0001FA80
		public IEnumerable<Event> ActiveEvents
		{
			get
			{
				return this.activeEvents;
			}
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x00021888 File Offset: 0x0001FA88
		public void AddTimeStamp(Event e)
		{
			this.timeStamps.Add(new EventManager.TimeStamp(e));
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0002189C File Offset: 0x0001FA9C
		public EventManager()
		{
			this.isClient = (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient);
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x00021949 File Offset: 0x0001FB49
		// (set) Token: 0x0600041A RID: 1050 RVA: 0x00021951 File Offset: 0x0001FB51
		public int RandomSeed { get; private set; }

		// Token: 0x0600041B RID: 1051 RVA: 0x0002195C File Offset: 0x0001FB5C
		public void StartRound(Level level)
		{
			EventManager.<>c__DisplayClass62_0 CS$<>8__locals1 = new EventManager.<>c__DisplayClass62_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.level = level;
			this.level = CS$<>8__locals1.level;
			if (this.isClient)
			{
				return;
			}
			this.timeStamps.Clear();
			this.pendingEventSets.Clear();
			this.selectedEvents.Clear();
			this.activeEvents.Clear();
			MissionAction.ResetMissionsUnlockedThisRound();
			UnlockPathAction.ResetPathsUnlockedThisRound();
			this.pathFinder = new PathFinder(WayPoint.WayPointList, false);
			this.totalPathLength = 0f;
			if (CS$<>8__locals1.level != null)
			{
				SteeringPath steeringPath = this.pathFinder.FindPath(ConvertUnits.ToSimUnits(CS$<>8__locals1.level.StartPosition), ConvertUnits.ToSimUnits(CS$<>8__locals1.level.EndPosition), null, null, 0f, null, null, null, true, 0f);
				this.totalPathLength = steeringPath.TotalLength;
			}
			this.SelectSettings();
			if (CS$<>8__locals1.level != null)
			{
				this.RandomSeed = ToolBox.StringToInt(CS$<>8__locals1.level.Seed);
				foreach (Identifier previousEvent in CS$<>8__locals1.level.LevelData.EventHistory)
				{
					this.RandomSeed ^= ToolBox.IdentifierToInt(previousEvent);
				}
			}
			this.random = new MTRandom(this.RandomSeed);
			GameSession gameSession = GameMain.GameSession;
			bool playingCampaign = ((gameSession != null) ? gameSession.GameMode : null) is CampaignMode;
			EventSet initialEventSet = null;
			EventSet additiveSet = null;
			IEnumerable<EventSet> selectAlwaysEventSets = from s in this.GetAllowedEventSets(EventSet.Prefabs.ToList<EventSet>(), new bool?(playingCampaign))
			where s.SelectAlways
			select s;
			foreach (EventSet eventSet in selectAlwaysEventSets)
			{
				if (eventSet.GetCommonness(CS$<>8__locals1.level) > 0f)
				{
					if (eventSet.Additive)
					{
						additiveSet = eventSet;
					}
					else
					{
						if (initialEventSet != null)
						{
							continue;
						}
						initialEventSet = eventSet;
					}
					CS$<>8__locals1.<StartRound>g__AddSet|2(eventSet);
				}
			}
			if (initialEventSet == null)
			{
				initialEventSet = this.SelectRandomEvents(EventSet.Prefabs.ToList<EventSet>(), new bool?(playingCampaign), this.random);
			}
			if (initialEventSet != null && initialEventSet.Additive)
			{
				additiveSet = initialEventSet;
				initialEventSet = this.SelectRandomEvents((from e in EventSet.Prefabs
				where !e.Additive
				select e).ToList<EventSet>(), new bool?(playingCampaign), this.random);
			}
			if (initialEventSet != null)
			{
				CS$<>8__locals1.<StartRound>g__AddSet|2(initialEventSet);
			}
			if (additiveSet != null)
			{
				CS$<>8__locals1.<StartRound>g__AddSet|2(additiveSet);
			}
			Level level2 = CS$<>8__locals1.level;
			LevelData levelData = (level2 != null) ? level2.LevelData : null;
			bool flag;
			if (levelData == null || levelData.Type != LevelData.LevelType.Outpost)
			{
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.GameMode : null) is TestGameMode)
				{
					Submarine mainSub = Submarine.MainSub;
					if (mainSub == null)
					{
						flag = false;
					}
					else
					{
						SubmarineInfo info = mainSub.Info;
						flag = (((info != null) ? new SubmarineType?(info.Type) : null).GetValueOrDefault() == SubmarineType.Outpost);
					}
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				flag = true;
			}
			bool isOutpostLevel = flag;
			if (isOutpostLevel)
			{
				Level level3 = CS$<>8__locals1.level;
				bool? flag2;
				if (level3 == null)
				{
					flag2 = null;
				}
				else
				{
					Location startLocation = level3.StartLocation;
					flag2 = ((startLocation != null) ? new bool?(startLocation.Connections.Any((LocationConnection c) => c.Locked && CS$<>8__locals1.level.StartLocation.MapPosition.X < c.OtherLocation(CS$<>8__locals1.level.StartLocation).MapPosition.X)) : null);
				}
				bool? flag3 = flag2;
				if (flag3.GetValueOrDefault())
				{
					EventPrefab unlockPathEventPrefab = EventPrefab.GetUnlockPathEvent(CS$<>8__locals1.level.LevelData.Biome.Identifier, CS$<>8__locals1.level.StartLocation.Faction);
					if (unlockPathEventPrefab != null)
					{
						Event newEvent = unlockPathEventPrefab.CreateInstance(this.RandomSeed);
						this.activeEvents.Add(newEvent);
					}
					else
					{
						CS$<>8__locals1.level.StartLocation.Connections.ForEach(delegate(LocationConnection c)
						{
							c.Locked = false;
						});
					}
				}
				Level level4 = CS$<>8__locals1.level;
				Submarine outpost = ((level4 != null) ? level4.StartOutpost : null) ?? Submarine.MainSub;
				NetworkMember networkMember = GameMain.NetworkMember;
				if ((networkMember == null || !networkMember.IsClient) && outpost != null)
				{
					foreach (Identifier eventTag in outpost.Info.TriggerOutpostMissionEvents)
					{
						EventPrefab eventPrefab = EventPrefab.FindEventPrefab(Identifier.Empty, eventTag, outpost.ContentPackage);
						if (eventPrefab == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Outpost ");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(outpost.Info.DisplayName);
							defaultInterpolatedStringHandler.AppendLiteral(" failed to trigger an event (tag: ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventTag);
							defaultInterpolatedStringHandler.AppendLiteral(").");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, outpost.ContentPackage, false, false);
						}
						else
						{
							Event newEvent2 = eventPrefab.CreateInstance(this.RandomSeed);
							this.ActivateEvent(newEvent2);
						}
					}
				}
			}
			Level level5 = CS$<>8__locals1.level;
			if (((level5 != null) ? level5.LevelData : null) != null)
			{
				CS$<>8__locals1.<StartRound>g__RegisterNonRepeatableChildEvents|3(initialEventSet);
			}
			for (;;)
			{
				Identifier id;
				if (!this.QueuedEventsForNextRound.TryDequeue(out id))
				{
					break;
				}
				EventPrefab eventPrefab2 = EventSet.GetEventPrefab(id) ?? (from e in EventSet.GetAllEventPrefabs()
				where e.Tags.Contains(id)
				select e).GetRandomUnsynced<EventPrefab>();
				if (eventPrefab2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(80, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in EventManager.StartRound - could not find an event with the identifier ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(id);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					Event ev = eventPrefab2.CreateInstance(this.RandomSeed);
					if (ev != null)
					{
						this.QueuedEvents.Enqueue(ev);
					}
				}
			}
			this.PreloadContent(this.GetFilesToPreload());
			this.roundDuration = 0f;
			this.eventsInitialized = false;
			this.isCrewAway = false;
			this.crewAwayDuration = 0f;
			this.crewAwayResetTimer = 0f;
			this.intensityUpdateTimer = 0f;
			this.CalculateCurrentIntensity(0f);
			this.currentIntensity = (this.musicIntensity = this.targetIntensity);
			this.eventCoolDown = 0f;
			this.CumulativeMonsterStrengthMain = 0f;
			this.CumulativeMonsterStrengthRuins = 0f;
			this.CumulativeMonsterStrengthWrecks = 0f;
			this.CumulativeMonsterStrengthCaves = 0f;
			this.distanceTraveled = 0f;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00021FFC File Offset: 0x000201FC
		public void ActivateEvent(Event newEvent)
		{
			this.activeEvents.Add(newEvent);
			newEvent.Init(null);
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00022011 File Offset: 0x00020211
		public void ClearEvents()
		{
			this.activeEvents.Clear();
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00022020 File Offset: 0x00020220
		private void SelectSettings()
		{
			if (!EventManagerSettings.Prefabs.Any<EventManagerSettings>())
			{
				throw new InvalidOperationException("Could not select EventManager settings (no settings loaded).");
			}
			EventManagerSettings[] orderedByDifficulty = EventManagerSettings.OrderedByDifficulty.ToArray<EventManagerSettings>();
			if (this.level == null)
			{
				throw new InvalidOperationException("Could not select EventManager settings (level not set).");
			}
			float extraDifficulty = 0f;
			CampaignMode campaign = GameMain.GameSession.Campaign;
			if (((campaign != null) ? campaign.Settings : null) != null)
			{
				extraDifficulty = GameMain.GameSession.Campaign.Settings.ExtraEventManagerDifficulty;
			}
			float modifiedDifficulty = Math.Clamp(this.level.Difficulty + extraDifficulty, 0f, 100f);
			EventManagerSettings[] suitableSettings = (from s in EventManagerSettings.OrderedByDifficulty
			where modifiedDifficulty >= s.MinLevelDifficulty && modifiedDifficulty <= s.MaxLevelDifficulty
			select s).ToArray<EventManagerSettings>();
			if (suitableSettings.Length == 0)
			{
				DebugConsole.ThrowError("No suitable event manager settings found for the selected level (difficulty " + this.level.Difficulty.ToString() + ")", null, null, false, false);
				this.settings = orderedByDifficulty.GetRandom(Rand.RandSync.ServerAndClient);
			}
			else
			{
				this.settings = suitableSettings.GetRandom(Rand.RandSync.ServerAndClient);
			}
			if (this.settings != null)
			{
				this.eventThreshold = this.settings.DefaultEventThreshold;
			}
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x0002213E File Offset: 0x0002033E
		public IEnumerable<ContentFile> GetFilesToPreload()
		{
			EventManager.<GetFilesToPreload>d__66 <GetFilesToPreload>d__ = new EventManager.<GetFilesToPreload>d__66(-2);
			<GetFilesToPreload>d__.<>4__this = this;
			return <GetFilesToPreload>d__;
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x00022150 File Offset: 0x00020350
		public void PreloadContent(IEnumerable<ContentFile> contentFiles)
		{
			List<ContentFile> filesToPreload = contentFiles.ToList<ContentFile>();
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.WreckAI != null)
				{
					if (!sub.WreckAI.Config.DefensiveAgent.IsEmpty)
					{
						CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(sub.WreckAI.Config.DefensiveAgent);
						if (prefab != null && !filesToPreload.Any((ContentFile f) => f.Path == prefab.FilePath))
						{
							filesToPreload.Add(prefab.ContentFile);
						}
					}
					foreach (Item item in Item.ItemList)
					{
						if (item.Submarine == sub)
						{
							foreach (ItemComponent component in item.Components)
							{
								if (component.statusEffectLists != null)
								{
									foreach (List<StatusEffect> statusEffectList in component.statusEffectLists.Values)
									{
										foreach (StatusEffect statusEffect in statusEffectList)
										{
											foreach (StatusEffect.CharacterSpawnInfo spawnInfo in statusEffect.SpawnCharacters)
											{
												CharacterPrefab prefab2 = CharacterPrefab.FindBySpeciesName(spawnInfo.SpeciesName);
												if (prefab2 != null && !filesToPreload.Contains(prefab2.ContentFile))
												{
													filesToPreload.Add(prefab2.ContentFile);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			foreach (ContentFile file in filesToPreload)
			{
				file.Preload(new Action<Sprite>(this.preloadedSprites.Add));
			}
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x00022450 File Offset: 0x00020650
		public void TriggerOnEndRoundActions()
		{
			foreach (Event ev in this.activeEvents)
			{
				ScriptedEvent scriptedEvent = ev as ScriptedEvent;
				if (scriptedEvent != null)
				{
					OnRoundEndAction onRoundEndAction = scriptedEvent.OnRoundEndAction;
					if (onRoundEndAction != null)
					{
						onRoundEndAction.Update(1f);
					}
				}
			}
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x000224C0 File Offset: 0x000206C0
		public void EndRound()
		{
			this.pendingEventSets.Clear();
			this.selectedEvents.Clear();
			this.activeEvents.Clear();
			this.QueuedEvents.Clear();
			this.finishedEvents.Clear();
			this.nonRepeatableEvents.Clear();
			this.preloadedSprites.ForEach(delegate(Sprite s)
			{
				s.Remove();
			});
			this.preloadedSprites.Clear();
			this.timeStamps.Clear();
			this.pathFinder = null;
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x00022558 File Offset: 0x00020758
		public void StoreEventDataAtRoundEnd(bool registerFinishedOnly = false)
		{
			Level level = this.level;
			if (((level != null) ? level.LevelData : null) == null)
			{
				return;
			}
			if (this.level.LevelData.Type == LevelData.LevelType.Outpost)
			{
				if (registerFinishedOnly)
				{
					foreach (Event finishedEvent in this.finishedEvents)
					{
						EventSet parentSet = finishedEvent.ParentSet;
						if (parentSet != null)
						{
							if (parentSet.Exhaustible)
							{
								this.level.LevelData.ExhaustEventSet(parentSet);
							}
							if (!this.level.LevelData.FinishedEvents.TryAdd(parentSet, 1))
							{
								Dictionary<EventSet, int> dictionary = this.level.LevelData.FinishedEvents;
								EventSet key = parentSet;
								dictionary[key]++;
							}
						}
					}
				}
				this.level.LevelData.EventHistory.AddRange(from e in this.selectedEvents.Values.SelectMany((List<Event> v) => v)
				select e.Prefab.Identifier into eventId
				where base.<StoreEventDataAtRoundEnd>g__Register|4(eventId) && !this.level.LevelData.EventHistory.Contains(eventId)
				select eventId);
				if (this.level.LevelData.EventHistory.Count > 20)
				{
					this.level.LevelData.EventHistory.RemoveRange(0, this.level.LevelData.EventHistory.Count - 20);
				}
			}
			this.level.LevelData.NonRepeatableEvents.AddRange(from eventId in this.nonRepeatableEvents
			where base.<StoreEventDataAtRoundEnd>g__Register|4(eventId) && !this.level.LevelData.NonRepeatableEvents.Contains(eventId)
			select eventId);
			if (!registerFinishedOnly)
			{
				this.level.LevelData.FinishedEvents.Clear();
			}
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x0002275C File Offset: 0x0002095C
		public void SkipEventCooldown()
		{
			this.eventCoolDown = 0f;
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x0002276C File Offset: 0x0002096C
		private float CalculateCommonness(EventPrefab eventPrefab, float baseCommonness)
		{
			if (this.level.LevelData.NonRepeatableEvents.Contains(eventPrefab.Identifier))
			{
				return 0f;
			}
			float retVal = baseCommonness;
			if (this.level.LevelData.EventHistory.Contains(eventPrefab.Identifier))
			{
				retVal *= 0.1f;
			}
			return retVal;
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x000227C4 File Offset: 0x000209C4
		private void CreateEvents(EventSet eventSet)
		{
			this.selectedEvents.Remove(eventSet);
			if (this.level == null)
			{
				return;
			}
			if (this.level.LevelData.HasHuntingGrounds && eventSet.DisableInHuntingGrounds)
			{
				return;
			}
			if (eventSet.Exhaustible && this.level.LevelData.IsEventSetExhausted(eventSet))
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Loading event set ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventSet.Identifier);
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.LightBlue), true);
			int applyCount = 1;
			List<Func<Level.InterestingPosition, bool>> spawnPosFilter = new List<Func<Level.InterestingPosition, bool>>();
			if (eventSet.PerRuin)
			{
				applyCount = this.level.Ruins.Count;
				using (List<Ruin>.Enumerator enumerator = this.level.Ruins.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Ruin ruin = enumerator.Current;
						spawnPosFilter.Add((Level.InterestingPosition pos) => pos.Ruin == ruin);
					}
					goto IL_1F6;
				}
			}
			if (eventSet.PerCave)
			{
				applyCount = this.level.Caves.Count;
				using (List<Level.Cave>.Enumerator enumerator2 = this.level.Caves.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Level.Cave cave = enumerator2.Current;
						spawnPosFilter.Add((Level.InterestingPosition pos) => pos.Cave == cave);
					}
					goto IL_1F6;
				}
			}
			if (eventSet.PerWreck)
			{
				IEnumerable<Submarine> wrecks = from s in Submarine.Loaded
				where s.Info.IsWreck && (s.WreckAI == null || !s.WreckAI.IsAlive)
				select s;
				applyCount = wrecks.Count<Submarine>();
				using (IEnumerator<Submarine> enumerator3 = wrecks.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Submarine wreck = enumerator3.Current;
						spawnPosFilter.Add((Level.InterestingPosition pos) => pos.Submarine == wreck);
					}
				}
			}
			IL_1F6:
			foreach (EventSet.SubEventPrefab subEventPrefab in eventSet.EventPrefabs)
			{
				foreach (Identifier missingId in subEventPrefab.GetMissingIdentifiers())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(81, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in event set \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(eventSet.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\" (");
					ContentFile contentFile = eventSet.ContentFile;
					string text;
					if (contentFile == null)
					{
						text = null;
					}
					else
					{
						ContentPackage contentPackage = contentFile.ContentPackage;
						text = ((contentPackage != null) ? contentPackage.Name : null);
					}
					defaultInterpolatedStringHandler2.AppendFormatted(text ?? "null");
					defaultInterpolatedStringHandler2.AppendLiteral(") - could not find an event prefab with the identifier \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(missingId);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, eventSet.ContentPackage, false, false);
				}
			}
			EventSet.SubEventPrefab[] suitablePrefabSubsets = (from e in eventSet.EventPrefabs
			where EventManager.IsFactionSuitable(e.Faction, this.level) && e.EventPrefabs.Any((EventPrefab ep) => EventManager.IsSuitable(ep, this.level))
			select e).ToArray<EventSet.SubEventPrefab>();
			for (int i = 0; i < applyCount; i++)
			{
				if (eventSet.ChooseRandom)
				{
					if (suitablePrefabSubsets.Any<EventSet.SubEventPrefab>())
					{
						List<EventSet.SubEventPrefab> unusedEvents = suitablePrefabSubsets.ToList<EventSet.SubEventPrefab>();
						int eventCount = eventSet.GetEventCount(this.level);
						int j = 0;
						while (j < eventCount && !unusedEvents.All((EventSet.SubEventPrefab e) => e.EventPrefabs.All((EventPrefab p) => this.CalculateCommonness(p, e.Commonness) <= 0f)))
						{
							EventSet.SubEventPrefab subEventPrefab2 = ToolBox.SelectWeightedRandom<EventSet.SubEventPrefab>(unusedEvents, (EventSet.SubEventPrefab e) => e.EventPrefabs.Max((EventPrefab p) => this.CalculateCommonness(p, e.Commonness)), this.random);
							EventSet.SubEventPrefab subEventPrefab3 = subEventPrefab2;
							IEnumerable<EventPrefab> enumerable;
							float num;
							float num2;
							subEventPrefab3.Deconstruct(out enumerable, out num, out num2);
							IEnumerable<EventPrefab> eventPrefabs = enumerable;
							float probability = num2;
							if (eventPrefabs != null && this.random.NextDouble() <= (double)probability)
							{
								EventPrefab eventPrefab = ToolBox.SelectWeightedRandom<EventPrefab>(from e in eventPrefabs
								where EventManager.IsSuitable(e, this.level)
								select e, (EventPrefab e) => e.Commonness, this.random);
								Event newEvent = eventPrefab.CreateInstance(this.RandomSeed);
								if (newEvent != null)
								{
									if (i < spawnPosFilter.Count)
									{
										newEvent.SpawnPosFilter = spawnPosFilter[i];
									}
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
									defaultInterpolatedStringHandler3.AppendLiteral("Initialized event ");
									defaultInterpolatedStringHandler3.AppendFormatted<Event>(newEvent);
									DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, true);
									if (!this.selectedEvents.ContainsKey(eventSet))
									{
										this.selectedEvents.Add(eventSet, new List<Event>());
									}
									this.selectedEvents[eventSet].Add(newEvent);
									unusedEvents.Remove(subEventPrefab2);
								}
							}
							j++;
						}
					}
					if (eventSet.ChildSets.Any<EventSet>())
					{
						int setCount = eventSet.SubSetCount;
						if (setCount > 1)
						{
							List<EventSet> unusedSets = eventSet.ChildSets.ToList<EventSet>();
							for (int k = 0; k < setCount; k++)
							{
								IReadOnlyList<EventSet> eventSets = unusedSets;
								Random random = this.random;
								EventSet newEventSet = this.SelectRandomEvents(eventSets, null, random);
								if (newEventSet == null)
								{
									break;
								}
								unusedSets.Remove(newEventSet);
								this.CreateEvents(newEventSet);
							}
						}
						else
						{
							IReadOnlyList<EventSet> eventSets2 = eventSet.ChildSets;
							Random random = this.random;
							EventSet newEventSet2 = this.SelectRandomEvents(eventSets2, null, random);
							if (newEventSet2 != null)
							{
								this.CreateEvents(newEventSet2);
							}
						}
					}
				}
				else
				{
					foreach (EventSet.SubEventPrefab subEventPrefab3 in suitablePrefabSubsets)
					{
						IEnumerable<EventPrefab> enumerable;
						float num;
						float num2;
						subEventPrefab3.Deconstruct(out enumerable, out num2, out num);
						IEnumerable<EventPrefab> eventPrefabs2 = enumerable;
						float probability2 = num;
						if (this.random.NextDouble() <= (double)probability2)
						{
							EventPrefab eventPrefab2 = ToolBox.SelectWeightedRandom<EventPrefab>(from e in eventPrefabs2
							where EventManager.IsSuitable(e, this.level)
							select e, (EventPrefab e) => e.Commonness, this.random);
							Event newEvent2 = eventPrefab2.CreateInstance(this.RandomSeed);
							if (newEvent2 != null)
							{
								if (i < spawnPosFilter.Count)
								{
									newEvent2.SpawnPosFilter = spawnPosFilter[i];
								}
								if (!this.selectedEvents.ContainsKey(eventSet))
								{
									this.selectedEvents.Add(eventSet, new List<Event>());
								}
								this.selectedEvents[eventSet].Add(newEvent2);
							}
						}
					}
					Location location = this.GetEventLocation();
					foreach (EventSet childEventSet in eventSet.ChildSets)
					{
						if (EventManager.IsValidForLevel(childEventSet, this.level) && this.IsValidForLocation(childEventSet, location))
						{
							this.CreateEvents(childEventSet);
						}
					}
				}
			}
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x00022EA0 File Offset: 0x000210A0
		private IEnumerable<EventSet> GetAllowedEventSets(IReadOnlyList<EventSet> eventSets, bool? requireCampaignSet = null)
		{
			EventManager.<>c__DisplayClass74_0 CS$<>8__locals1 = new EventManager.<>c__DisplayClass74_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.level == null)
			{
				return Enumerable.Empty<EventSet>();
			}
			IEnumerable<EventSet> allowedEventSets = from set in eventSets
			where EventManager.IsValidForLevel(set, CS$<>8__locals1.<>4__this.level)
			select set;
			if (requireCampaignSet != null)
			{
				if (requireCampaignSet.Value)
				{
					if (allowedEventSets.Any((EventSet es) => es.IsCampaignSet))
					{
						allowedEventSets = from es in allowedEventSets
						where es.IsCampaignSet
						select es;
					}
					else
					{
						DebugConsole.AddWarning("No campaign event sets available. Using a non-campaign-specific set instead.", null);
					}
				}
				else
				{
					allowedEventSets = from es in allowedEventSets
					where !es.IsCampaignSet
					select es;
				}
			}
			CS$<>8__locals1.location = this.GetEventLocation();
			allowedEventSets = from set in allowedEventSets
			where CS$<>8__locals1.<>4__this.IsValidForLocation(set, CS$<>8__locals1.location)
			select set;
			allowedEventSets = allowedEventSets.Where(delegate(EventSet set)
			{
				if (!set.CampaignTutorialOnly)
				{
					return true;
				}
				if (GameMain.IsSingleplayer)
				{
					GameSession gameSession3 = GameMain.GameSession;
					CampaignSettings campaignSettings;
					if (gameSession3 == null)
					{
						campaignSettings = null;
					}
					else
					{
						CampaignMode campaign = gameSession3.Campaign;
						campaignSettings = ((campaign != null) ? campaign.Settings : null);
					}
					CampaignSettings campaignSettings2 = campaignSettings;
					return campaignSettings2 != null && campaignSettings2.TutorialEnabled;
				}
				return false;
			});
			EventManager.<>c__DisplayClass74_0 CS$<>8__locals2 = CS$<>8__locals1;
			GameSession gameSession = GameMain.GameSession;
			int? discoveryIndex;
			if (gameSession == null)
			{
				discoveryIndex = null;
			}
			else
			{
				Map map = gameSession.Map;
				discoveryIndex = ((map != null) ? map.GetDiscoveryIndex(CS$<>8__locals1.location) : null);
			}
			CS$<>8__locals2.discoveryIndex = discoveryIndex;
			EventManager.<>c__DisplayClass74_0 CS$<>8__locals3 = CS$<>8__locals1;
			GameSession gameSession2 = GameMain.GameSession;
			int? visitIndex;
			if (gameSession2 == null)
			{
				visitIndex = null;
			}
			else
			{
				Map map2 = gameSession2.Map;
				visitIndex = ((map2 != null) ? map2.GetVisitIndex(CS$<>8__locals1.location, false) : null);
			}
			CS$<>8__locals3.visitIndex = visitIndex;
			if (CS$<>8__locals1.discoveryIndex != null)
			{
				int? num = CS$<>8__locals1.discoveryIndex;
				int num2 = 0;
				if ((num.GetValueOrDefault() >= num2 & num != null) && allowedEventSets.Any(delegate(EventSet set)
				{
					int forceAtDiscoveredNr = set.ForceAtDiscoveredNr;
					int? discoveryIndex2 = CS$<>8__locals1.discoveryIndex;
					return forceAtDiscoveredNr == discoveryIndex2.GetValueOrDefault() & discoveryIndex2 != null;
				}))
				{
					return allowedEventSets.Where(delegate(EventSet set)
					{
						int forceAtDiscoveredNr = set.ForceAtDiscoveredNr;
						int? discoveryIndex2 = CS$<>8__locals1.discoveryIndex;
						return forceAtDiscoveredNr == discoveryIndex2.GetValueOrDefault() & discoveryIndex2 != null;
					});
				}
			}
			if (CS$<>8__locals1.visitIndex != null)
			{
				int? num = CS$<>8__locals1.visitIndex;
				int num2 = 0;
				if ((num.GetValueOrDefault() >= num2 & num != null) && allowedEventSets.Any(delegate(EventSet set)
				{
					int forceAtVisitedNr = set.ForceAtVisitedNr;
					int? visitIndex2 = CS$<>8__locals1.visitIndex;
					return forceAtVisitedNr == visitIndex2.GetValueOrDefault() & visitIndex2 != null;
				}))
				{
					return allowedEventSets.Where(delegate(EventSet set)
					{
						int forceAtVisitedNr = set.ForceAtVisitedNr;
						int? visitIndex2 = CS$<>8__locals1.visitIndex;
						return forceAtVisitedNr == visitIndex2.GetValueOrDefault() & visitIndex2 != null;
					});
				}
			}
			return from set in allowedEventSets
			where set.ForceAtDiscoveredNr < 0 && set.ForceAtVisitedNr < 0
			select set;
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00023100 File Offset: 0x00021300
		private EventSet SelectRandomEvents(IReadOnlyList<EventSet> eventSets, bool? requireCampaignSet = null, Random random = null)
		{
			IEnumerable<EventSet> allowedEventSets = this.GetAllowedEventSets(eventSets, requireCampaignSet);
			if (allowedEventSets.Count<EventSet>() == 1)
			{
				return allowedEventSets.First<EventSet>();
			}
			Random rand = random ?? new MTRandom(ToolBox.StringToInt(this.level.Seed));
			float totalCommonness = allowedEventSets.Sum((EventSet e) => e.GetCommonness(this.level));
			float randomNumber = (float)rand.NextDouble();
			randomNumber *= totalCommonness;
			foreach (EventSet eventSet in allowedEventSets)
			{
				float commonness = eventSet.GetCommonness(this.level);
				if (randomNumber <= commonness)
				{
					return eventSet;
				}
				randomNumber -= commonness;
			}
			return null;
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x000231C0 File Offset: 0x000213C0
		public static bool IsSuitable(EventPrefab e, Level level)
		{
			return EventManager.IsLevelSuitable(e, level) && EventManager.IsFactionSuitable(e.Faction, level);
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x000231DC File Offset: 0x000213DC
		public static bool IsLevelSuitable(EventPrefab e, Level level)
		{
			if (!e.BiomeIdentifier.IsEmpty)
			{
				Identifier? identifier = new Identifier?(e.BiomeIdentifier);
				LevelData levelData = level.LevelData;
				Identifier? identifier2;
				Identifier? identifier3;
				if (levelData == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					Biome biome = levelData.Biome;
					if (biome == null)
					{
						identifier2 = null;
						identifier3 = identifier2;
					}
					else
					{
						identifier3 = new Identifier?(biome.Identifier);
					}
				}
				identifier2 = identifier3;
				if (!(identifier == identifier2))
				{
					return false;
				}
			}
			if ((e.RequiredLayer.IsEmpty || Submarine.LayerExistsInAnySub(e.RequiredLayer)) && (e.RequiredSpawnPointTag.IsEmpty || WayPoint.WayPointList.Any((WayPoint wp) => wp.Tags.Contains(e.RequiredSpawnPointTag))))
			{
				return !level.LevelData.NonRepeatableEvents.Contains(e.Identifier);
			}
			return false;
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x000232C8 File Offset: 0x000214C8
		private static bool IsFactionSuitable(Identifier factionId, Level level)
		{
			if (!factionId.IsEmpty)
			{
				Identifier? identifier = new Identifier?(factionId);
				Location startLocation = level.StartLocation;
				Identifier? identifier2;
				Identifier? identifier3;
				if (startLocation == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					Faction faction = startLocation.Faction;
					if (faction == null)
					{
						identifier2 = null;
						identifier3 = identifier2;
					}
					else
					{
						identifier3 = new Identifier?(faction.Prefab.Identifier);
					}
				}
				identifier2 = identifier3;
				if (!(identifier == identifier2))
				{
					Identifier? identifier4 = new Identifier?(factionId);
					Location startLocation2 = level.StartLocation;
					Identifier? identifier5;
					Identifier? identifier6;
					if (startLocation2 == null)
					{
						identifier5 = null;
						identifier6 = identifier5;
					}
					else
					{
						Faction secondaryFaction = startLocation2.SecondaryFaction;
						if (secondaryFaction == null)
						{
							identifier5 = null;
							identifier6 = identifier5;
						}
						else
						{
							identifier6 = new Identifier?(secondaryFaction.Prefab.Identifier);
						}
					}
					identifier5 = identifier6;
					return identifier4 == identifier5;
				}
			}
			return true;
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x00023378 File Offset: 0x00021578
		private static bool IsValidForLevel(EventSet eventSet, Level level)
		{
			return level.IsAllowedDifficulty(eventSet.MinLevelDifficulty, eventSet.MaxLevelDifficulty) && eventSet.LevelType.HasFlag(level.LevelData.Type) && (eventSet.RequiredLayer.IsEmpty || Submarine.LayerExistsInAnySub(eventSet.RequiredLayer)) && (eventSet.RequiredSpawnPointTag.IsEmpty || WayPoint.WayPointList.Any((WayPoint wp) => wp.Tags.Contains(eventSet.RequiredSpawnPointTag))) && (eventSet.BiomeIdentifier.IsEmpty || eventSet.BiomeIdentifier == level.LevelData.Biome.Identifier);
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00023464 File Offset: 0x00021664
		private bool IsValidForLocation(EventSet eventSet, Location location)
		{
			if (location == null)
			{
				return true;
			}
			if (!eventSet.Faction.IsEmpty)
			{
				Identifier? identifier = new Identifier?(eventSet.Faction);
				Faction faction = location.Faction;
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
					Identifier? identifier4 = new Identifier?(eventSet.Faction);
					Faction secondaryFaction = location.SecondaryFaction;
					Identifier? identifier5;
					Identifier? identifier6;
					if (secondaryFaction == null)
					{
						identifier5 = null;
						identifier6 = identifier5;
					}
					else
					{
						identifier6 = new Identifier?(secondaryFaction.Prefab.Identifier);
					}
					identifier5 = identifier6;
					if (identifier4 != identifier5)
					{
						return false;
					}
				}
			}
			LocationType locationType = location.GetLocationTypeToDisplay();
			bool includeGenericEvents = this.level.Type == LevelData.LevelType.LocationConnection || !locationType.IgnoreGenericEvents;
			if (includeGenericEvents && new ImmutableArray<Identifier>?(eventSet.LocationTypeIdentifiers) == null)
			{
				return true;
			}
			if (new ImmutableArray<Identifier>?(eventSet.LocationTypeIdentifiers) == null)
			{
				return false;
			}
			bool hasMatchingEventLocationId = !locationType.EventLocationType.IsEmpty && eventSet.LocationTypeIdentifiers.Contains(locationType.EventLocationType);
			bool hasMatchingLocationId = eventSet.LocationTypeIdentifiers.Contains(locationType.Identifier);
			return hasMatchingEventLocationId || hasMatchingLocationId;
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x000235A2 File Offset: 0x000217A2
		private Location GetEventLocation()
		{
			GameSession gameSession = GameMain.GameSession;
			Location location;
			if (gameSession == null)
			{
				location = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign == null)
				{
					location = null;
				}
				else
				{
					Map map = campaign.Map;
					location = ((map != null) ? map.CurrentLocation : null);
				}
			}
			Location result;
			if ((result = location) == null)
			{
				Level level = this.level;
				if (level == null)
				{
					return null;
				}
				result = level.StartLocation;
			}
			return result;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x000235E4 File Offset: 0x000217E4
		private bool CanStartEventSet(EventSet eventSet)
		{
			if (!eventSet.AllowAtStart)
			{
				ISpatialEntity refEntity = EventManager.GetRefEntity(false);
				float distFromStart = (float)Math.Sqrt(MathUtils.LineSegmentToPointDistanceSquared(this.level.StartExitPosition.ToPoint(), this.level.StartPosition.ToPoint(), refEntity.WorldPosition.ToPoint()));
				float distFromEnd = (float)Math.Sqrt(MathUtils.LineSegmentToPointDistanceSquared(this.level.EndExitPosition.ToPoint(), this.level.EndPosition.ToPoint(), refEntity.WorldPosition.ToPoint()));
				if (distFromStart * Physics.DisplayToRealWorldRatio < 50f || distFromEnd * Physics.DisplayToRealWorldRatio < 50f)
				{
					return false;
				}
			}
			return (!eventSet.DelayWhenCrewAway || ((!this.isCrewAway || this.crewAwayDuration >= this.settings.FreezeDurationWhenCrewAway) && this.crewAwayResetTimer <= 0f)) && ((Submarine.MainSub != null && this.distanceTraveled >= eventSet.MinDistanceTraveled) || this.roundDuration >= eventSet.MinMissionTime) && this.CurrentIntensity >= eventSet.MinIntensity && this.CurrentIntensity <= eventSet.MaxIntensity;
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x00023718 File Offset: 0x00021918
		public void Update(float deltaTime)
		{
			if (!this.Enabled)
			{
				return;
			}
			CampaignMode campaign = GameMain.GameSession.Campaign;
			if (campaign != null && campaign.DisableEvents)
			{
				return;
			}
			if (!this.eventsInitialized)
			{
				foreach (EventSet eventSet2 in this.selectedEvents.Keys)
				{
					foreach (Event ev3 in this.selectedEvents[eventSet2])
					{
						ev3.Init(eventSet2);
					}
				}
				this.eventsInitialized = true;
			}
			this.CalculateCurrentIntensity(deltaTime);
			if (this.isClient)
			{
				return;
			}
			this.roundDuration += deltaTime;
			if (this.settings == null)
			{
				DebugConsole.ThrowError("Event settings not set before updating EventManager. Attempting to select...", null, null, false, false);
				this.SelectSettings();
				if (this.settings == null)
				{
					DebugConsole.ThrowError("Could not select EventManager settings. Disabling EventManager for the round...", null, null, false, false);
					GameServer server = GameMain.Server;
					if (server != null)
					{
						server.SendChatMessage("Could not select EventManager settings. Disabling EventManager for the round...", new ChatMessageType?(ChatMessageType.Error), null, null, PlayerConnectionChangeType.None, ChatMode.None);
					}
					this.Enabled = false;
					return;
				}
			}
			if (this.IsCrewAway())
			{
				this.isCrewAway = true;
				this.crewAwayResetTimer = 60f;
				this.crewAwayDuration += deltaTime;
			}
			else if (this.crewAwayResetTimer > 0f)
			{
				this.isCrewAway = false;
				this.crewAwayResetTimer -= deltaTime;
			}
			else
			{
				this.isCrewAway = false;
				this.crewAwayDuration = 0f;
				this.eventThreshold += this.settings.EventThresholdIncrease * deltaTime;
				this.eventThreshold = Math.Min(this.eventThreshold, 1f);
				this.eventCoolDown -= deltaTime;
			}
			this.calculateDistanceTraveledTimer -= deltaTime;
			if (this.calculateDistanceTraveledTimer <= 0f)
			{
				this.distanceTraveled = this.CalculateDistanceTraveled();
				this.calculateDistanceTraveledTimer = 5f;
			}
			bool recheck = false;
			do
			{
				recheck = false;
				for (int i = this.pendingEventSets.Count - 1; i >= 0; i--)
				{
					EventSet eventSet = this.pendingEventSets[i];
					if ((this.eventCoolDown <= 0f || eventSet.IgnoreCoolDown) && (this.currentIntensity <= this.eventThreshold || eventSet.IgnoreIntensity) && this.CanStartEventSet(eventSet))
					{
						this.pendingEventSets.RemoveAt(i);
						if (this.selectedEvents.ContainsKey(eventSet))
						{
							Action <>9__1;
							foreach (Event ev2 in this.selectedEvents[eventSet])
							{
								this.activeEvents.Add(ev2);
								this.eventThreshold = this.settings.DefaultEventThreshold;
								if (eventSet.TriggerEventCooldown)
								{
									if (this.selectedEvents[eventSet].Any((Event e) => e.Prefab.TriggerEventCooldown))
									{
										this.eventCoolDown = this.settings.EventCooldown;
									}
								}
								if (eventSet.ResetTime > 0f)
								{
									Event @event = ev2;
									Action value;
									if ((value = <>9__1) == null)
									{
										value = (<>9__1 = delegate()
										{
											this.pendingEventSets.Add(eventSet);
											this.CreateEvents(eventSet);
											foreach (Event newEvent in this.selectedEvents[eventSet])
											{
												if (!newEvent.Initialized)
												{
													newEvent.Init(eventSet);
												}
											}
										});
									}
									@event.Finished += value;
								}
							}
						}
						foreach (EventSet childEventSet in eventSet.ChildSets)
						{
							this.pendingEventSets.Add(childEventSet);
							recheck = true;
						}
					}
				}
			}
			while (recheck);
			using (List<Event>.Enumerator enumerator5 = this.activeEvents.GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					Event ev = enumerator5.Current;
					if (!ev.IsFinished)
					{
						ev.Update(deltaTime);
					}
					else if (ev.Prefab != null && !this.finishedEvents.Any((Event e) => e.Prefab == ev.Prefab))
					{
						Level level = this.level;
						if (((level != null) ? level.LevelData : null) != null && this.level.LevelData.Type == LevelData.LevelType.Outpost && !this.level.LevelData.EventHistory.Contains(ev.Prefab.Identifier))
						{
							this.level.LevelData.EventHistory.Add(ev.Prefab.Identifier);
						}
						this.finishedEvents.Add(ev);
					}
				}
			}
			if (this.QueuedEvents.Count > 0)
			{
				this.activeEvents.Add(this.QueuedEvents.Dequeue());
			}
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x00023C70 File Offset: 0x00021E70
		public void EntitySpawned(Entity entity)
		{
			foreach (Event ev in this.activeEvents)
			{
				ScriptedEvent scriptedEvent = ev as ScriptedEvent;
				if (scriptedEvent != null)
				{
					scriptedEvent.EntitySpawned(entity);
				}
			}
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00023CD0 File Offset: 0x00021ED0
		private void CalculateCurrentIntensity(float deltaTime)
		{
			this.intensityUpdateTimer -= deltaTime;
			if (this.intensityUpdateTimer > 0f)
			{
				return;
			}
			this.intensityUpdateTimer = 5f;
			this.avgCrewHealth = 0f;
			int characterCount = 0;
			foreach (Character character in Character.CharacterList)
			{
				if (!character.IsDead && character.TeamID != CharacterTeamType.FriendlyNPC && (character.AIController is HumanAIController || character.IsRemotePlayer))
				{
					this.avgCrewHealth += character.Vitality / character.MaxVitality * (character.IsUnconscious ? 0.5f : 1f);
					characterCount++;
				}
			}
			if (characterCount > 0)
			{
				this.avgCrewHealth /= (float)characterCount;
			}
			else
			{
				this.avgCrewHealth = 0.5f;
			}
			this.enemyDanger = 0f;
			this.monsterStrength = 0f;
			foreach (Character character2 in Character.CharacterList)
			{
				if (!character2.IsIncapacitated && !character2.IsHandcuffed && character2.Enabled && !character2.IsPet)
				{
					EnemyAIController enemyAI = character2.AIController as EnemyAIController;
					if (enemyAI != null)
					{
						if (!enemyAI.AIParams.StayInAbyss)
						{
							this.monsterStrength += enemyAI.CombatStrength;
						}
						if (Submarine.MainSub != null)
						{
							Hull currentHull = character2.CurrentHull;
							SubmarineInfo submarineInfo = (currentHull != null) ? currentHull.Submarine.Info : null;
							if (submarineInfo != null && submarineInfo.Type == SubmarineType.Player && (character2.CurrentHull.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(character2.CurrentHull.Submarine)))
							{
								this.enemyDanger += enemyAI.CombatStrength / 500f;
								continue;
							}
						}
						AITarget selectedAiTarget = enemyAI.SelectedAiTarget;
						bool flag;
						if (selectedAiTarget == null)
						{
							flag = (null != null);
						}
						else
						{
							Entity entity = selectedAiTarget.Entity;
							flag = (((entity != null) ? entity.Submarine : null) != null);
						}
						if (flag)
						{
							this.enemyDanger += enemyAI.CombatStrength / 5000f;
						}
					}
					else
					{
						HumanAIController humanAi = character2.AIController as HumanAIController;
						if (humanAi != null && !character2.IsOnFriendlyTeam(CharacterTeamType.Team1) && character2.Submarine != null && Submarine.MainSub != null)
						{
							PhysicsBody physicsBody = character2.Submarine.PhysicsBody;
							if (physicsBody != null && physicsBody.BodyType == BodyType.Dynamic && Vector2.DistanceSquared(character2.Submarine.WorldPosition, Submarine.MainSub.WorldPosition) < 100000000f)
							{
								this.enemyDanger += 0.2f;
							}
						}
					}
				}
			}
			this.enemyDanger += this.monsterStrength / 5000f;
			this.enemyDanger = MathHelper.Clamp(this.enemyDanger, 0f, 1f);
			float holeCount = 0f;
			float waterAmount = 0f;
			float dryHullVolume = 0f;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine != null && hull.Submarine.Info.Type == SubmarineType.Player)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
					{
						if (hull.Submarine.TeamID != CharacterTeamType.Team1 && hull.Submarine.TeamID != CharacterTeamType.Team2)
						{
							continue;
						}
					}
					else if (hull.Submarine.TeamID != CharacterTeamType.Team1)
					{
						continue;
					}
					this.fireAmount += hull.FireSources.Sum((FireSource fs) => fs.Size.X);
					if (!hull.IsWetRoom)
					{
						foreach (Gap gap in hull.ConnectedGaps)
						{
							if (!gap.IsRoomToRoom)
							{
								holeCount += gap.Open;
							}
						}
						waterAmount += hull.WaterVolume;
						dryHullVolume += hull.Volume;
					}
				}
			}
			if (dryHullVolume > 0f)
			{
				this.floodingAmount = waterAmount / dryHullVolume;
			}
			this.avgHullIntegrity = MathHelper.Clamp(1f - holeCount / 10f, 0f, 1f);
			this.fireAmount = MathHelper.Clamp(this.fireAmount / 1000f, (this.fireAmount > 0f) ? 0.2f : 0f, 1f);
			if (this.floodingAmount < 0.1f)
			{
				this.floodingAmount = 0f;
			}
			else
			{
				this.floodingAmount *= 1.5f;
			}
			this.targetIntensity = (1f - this.avgCrewHealth + (1f - this.avgHullIntegrity) + this.floodingAmount) / 3f;
			this.targetIntensity += this.fireAmount * 0.5f;
			this.targetIntensity += this.enemyDanger;
			this.targetIntensity = MathHelper.Clamp(this.targetIntensity, 0f, 1f);
			if (this.targetIntensity > this.currentIntensity)
			{
				this.currentIntensity = Math.Min(this.currentIntensity + 0.19999999f, this.targetIntensity);
				this.musicIntensity = Math.Min(this.musicIntensity + 0.25f, this.targetIntensity);
				return;
			}
			this.currentIntensity = Math.Max(this.currentIntensity - 0.012499999f, this.targetIntensity);
			this.musicIntensity = Math.Max(this.musicIntensity - 0.25f, this.targetIntensity);
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x00024320 File Offset: 0x00022520
		private float CalculateDistanceTraveled()
		{
			if (this.level == null || this.pathFinder == null)
			{
				return 0f;
			}
			ISpatialEntity refEntity = EventManager.GetRefEntity(false);
			if (refEntity == null)
			{
				return 0f;
			}
			Vector2 target = ConvertUnits.ToSimUnits(this.level.EndPosition);
			SteeringPath steeringPath = this.pathFinder.FindPath(ConvertUnits.ToSimUnits(refEntity.WorldPosition), target, null, null, 0f, null, null, null, true, 0f);
			if (steeringPath.Unreachable || float.IsPositiveInfinity(this.totalPathLength))
			{
				return MathHelper.Clamp((refEntity.WorldPosition.X - this.level.StartPosition.X) / (this.level.EndPosition.X - this.level.StartPosition.X), 0f, 1f);
			}
			return MathHelper.Clamp(1f - steeringPath.TotalLength / this.totalPathLength, 0f, 1f);
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00024414 File Offset: 0x00022614
		public static ISpatialEntity GetRefEntity(bool acceptRemoteControlledSubs = false)
		{
			EventManager.<>c__DisplayClass88_0 CS$<>8__locals1;
			CS$<>8__locals1.acceptRemoteControlledSubs = acceptRemoteControlledSubs;
			CS$<>8__locals1.refEntity = Submarine.MainSub;
			if (CS$<>8__locals1.refEntity == null)
			{
				return null;
			}
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (client.Character != null)
				{
					EventManager.<GetRefEntity>g__GetRefSubForCharacter|88_0(client.Character, ref CS$<>8__locals1);
				}
			}
			return CS$<>8__locals1.refEntity;
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x00024498 File Offset: 0x00022698
		private bool IsCrewAway()
		{
			int playerCount = 0;
			int awayPlayerCount = 0;
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (client.Character != null && !client.Character.IsDead && !client.Character.IsIncapacitated)
				{
					playerCount++;
					if (this.IsCharacterAway(client.Character))
					{
						awayPlayerCount++;
					}
				}
			}
			return playerCount > 0 && (float)awayPlayerCount / (float)playerCount > 0.5f;
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00024530 File Offset: 0x00022730
		private bool IsCharacterAway(Character character)
		{
			if (character.Submarine != null)
			{
				switch (character.Submarine.Info.Type)
				{
				case SubmarineType.Player:
				case SubmarineType.Outpost:
				case SubmarineType.OutpostModule:
					return false;
				case SubmarineType.Wreck:
				case SubmarineType.BeaconStation:
				case SubmarineType.Ruin:
					return true;
				}
			}
			if (this.level != null && !this.level.Removed)
			{
				foreach (Ruin ruin in this.level.Ruins)
				{
					Rectangle area = ruin.Area;
					area.Inflate(1000, 1000);
					if (area.Contains(character.WorldPosition))
					{
						return true;
					}
				}
				foreach (Level.Cave cave in this.level.Caves)
				{
					Rectangle area2 = cave.Area;
					area2.Inflate(1000, 1000);
					if (area2.Contains(character.WorldPosition))
					{
						return true;
					}
				}
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.Info.Type == SubmarineType.BeaconStation || sub.Info.Type == SubmarineType.Wreck)
				{
					Rectangle worldBorders = new Rectangle(sub.Borders.X + (int)sub.WorldPosition.X - 1000, sub.Borders.Y + (int)sub.WorldPosition.Y + 1000, sub.Borders.Width + 2000, sub.Borders.Height + 2000);
					if (Submarine.RectContains(worldBorders, character.WorldPosition, false))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00024768 File Offset: 0x00022968
		public void Load(XElement element)
		{
			foreach (Identifier id in element.GetAttributeIdentifierArray("QueuedEventsForNextRound", Array.Empty<Identifier>(), true))
			{
				this.QueuedEventsForNextRound.Enqueue(id);
			}
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x000247A9 File Offset: 0x000229A9
		public XElement Save()
		{
			return new XElement("eventmanager", new XAttribute("QueuedEventsForNextRound", string.Join<Identifier>(',', this.QueuedEventsForNextRound)));
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x000248BC File Offset: 0x00022ABC
		[CompilerGenerated]
		internal static void <GetRefEntity>g__GetRefSubForCharacter|88_0(Character character, ref EventManager.<>c__DisplayClass88_0 A_1)
		{
			Submarine playerSub = character.Submarine;
			if (playerSub != null)
			{
				SubmarineInfo info = playerSub.Info;
				if (info != null && info.Type == SubmarineType.Player && playerSub.WorldPosition.X > A_1.refEntity.WorldPosition.X)
				{
					A_1.refEntity = playerSub;
				}
			}
			if (A_1.acceptRemoteControlledSubs)
			{
				Entity viewTarget = character.ViewTarget;
				Submarine viewedSub = (viewTarget != null) ? viewTarget.Submarine : null;
				if (viewedSub != null)
				{
					SubmarineInfo info = viewedSub.Info;
					if (info != null && info.Type == SubmarineType.Player && viewedSub.WorldPosition.X > A_1.refEntity.WorldPosition.X)
					{
						A_1.refEntity = viewedSub;
					}
				}
				Item selectedItem = character.SelectedItem;
				Submarine submarine;
				if (selectedItem == null)
				{
					submarine = null;
				}
				else
				{
					Steering component = selectedItem.GetComponent<Steering>();
					submarine = ((component != null) ? component.ControlledSub : null);
				}
				Submarine controlledSub = submarine;
				if (controlledSub != null && controlledSub.WorldPosition.X > A_1.refEntity.WorldPosition.X)
				{
					A_1.refEntity = controlledSub;
				}
			}
		}

		// Token: 0x040001DD RID: 477
		private const float IntensityUpdateInterval = 5f;

		// Token: 0x040001DE RID: 478
		private const float CalculateDistanceTraveledInterval = 5f;

		// Token: 0x040001DF RID: 479
		private const int MaxEventHistory = 20;

		// Token: 0x040001E0 RID: 480
		private Level level;

		// Token: 0x040001E1 RID: 481
		private readonly List<Sprite> preloadedSprites = new List<Sprite>();

		// Token: 0x040001E2 RID: 482
		private float currentIntensity;

		// Token: 0x040001E3 RID: 483
		private float targetIntensity;

		// Token: 0x040001E4 RID: 484
		private float musicIntensity;

		// Token: 0x040001E5 RID: 485
		private float eventThreshold = 0.2f;

		// Token: 0x040001E6 RID: 486
		private float eventCoolDown;

		// Token: 0x040001E7 RID: 487
		private float intensityUpdateTimer;

		// Token: 0x040001E8 RID: 488
		private PathFinder pathFinder;

		// Token: 0x040001E9 RID: 489
		private float totalPathLength;

		// Token: 0x040001EA RID: 490
		private float calculateDistanceTraveledTimer;

		// Token: 0x040001EB RID: 491
		private float distanceTraveled;

		// Token: 0x040001EC RID: 492
		private float avgCrewHealth;

		// Token: 0x040001ED RID: 493
		private float avgHullIntegrity;

		// Token: 0x040001EE RID: 494
		private float floodingAmount;

		// Token: 0x040001EF RID: 495
		private float fireAmount;

		// Token: 0x040001F0 RID: 496
		private float enemyDanger;

		// Token: 0x040001F1 RID: 497
		private float monsterStrength;

		// Token: 0x040001F2 RID: 498
		public float CumulativeMonsterStrengthMain;

		// Token: 0x040001F3 RID: 499
		public float CumulativeMonsterStrengthRuins;

		// Token: 0x040001F4 RID: 500
		public float CumulativeMonsterStrengthWrecks;

		// Token: 0x040001F5 RID: 501
		public float CumulativeMonsterStrengthCaves;

		// Token: 0x040001F6 RID: 502
		private float roundDuration;

		// Token: 0x040001F7 RID: 503
		private bool isCrewAway;

		// Token: 0x040001F8 RID: 504
		private const float CrewAwayResetDelay = 60f;

		// Token: 0x040001F9 RID: 505
		private float crewAwayResetTimer;

		// Token: 0x040001FA RID: 506
		private float crewAwayDuration;

		// Token: 0x040001FB RID: 507
		private readonly List<EventSet> pendingEventSets = new List<EventSet>();

		// Token: 0x040001FC RID: 508
		private readonly Dictionary<EventSet, List<Event>> selectedEvents = new Dictionary<EventSet, List<Event>>();

		// Token: 0x040001FD RID: 509
		private readonly List<Event> activeEvents = new List<Event>();

		// Token: 0x040001FE RID: 510
		private readonly HashSet<Event> finishedEvents = new HashSet<Event>();

		// Token: 0x040001FF RID: 511
		private readonly HashSet<Identifier> nonRepeatableEvents = new HashSet<Identifier>();

		// Token: 0x04000200 RID: 512
		private EventManagerSettings settings;

		// Token: 0x04000201 RID: 513
		private readonly bool isClient;

		// Token: 0x04000202 RID: 514
		public readonly Queue<Event> QueuedEvents = new Queue<Event>();

		// Token: 0x04000203 RID: 515
		public readonly Queue<Identifier> QueuedEventsForNextRound = new Queue<Identifier>();

		// Token: 0x04000204 RID: 516
		private readonly List<EventManager.TimeStamp> timeStamps = new List<EventManager.TimeStamp>();

		// Token: 0x04000205 RID: 517
		public readonly EventLog EventLog = new EventLog();

		// Token: 0x04000206 RID: 518
		public bool Enabled = true;

		// Token: 0x04000207 RID: 519
		private MTRandom random;

		// Token: 0x04000209 RID: 521
		private bool eventsInitialized;

		// Token: 0x020005B7 RID: 1463
		public enum NetworkEventType
		{
			// Token: 0x04002751 RID: 10065
			CONVERSATION,
			// Token: 0x04002752 RID: 10066
			CONVERSATION_SELECTED_OPTION,
			// Token: 0x04002753 RID: 10067
			STATUSEFFECT,
			// Token: 0x04002754 RID: 10068
			MISSION,
			// Token: 0x04002755 RID: 10069
			UNLOCKPATH,
			// Token: 0x04002756 RID: 10070
			EVENTLOG,
			// Token: 0x04002757 RID: 10071
			EVENTOBJECTIVE
		}

		// Token: 0x020005B8 RID: 1464
		[NetworkSerialize(25)]
		public readonly struct NetEventLogEntry : INetSerializableStruct, IEquatable<EventManager.NetEventLogEntry>
		{
			// Token: 0x06004BCE RID: 19406 RVA: 0x001DC09E File Offset: 0x001DA29E
			public NetEventLogEntry(Identifier EventPrefabId, Identifier LogEntryId, string Text)
			{
				this.EventPrefabId = EventPrefabId;
				this.LogEntryId = LogEntryId;
				this.Text = Text;
			}

			// Token: 0x170013C3 RID: 5059
			// (get) Token: 0x06004BCF RID: 19407 RVA: 0x001DC0B5 File Offset: 0x001DA2B5
			// (set) Token: 0x06004BD0 RID: 19408 RVA: 0x001DC0BD File Offset: 0x001DA2BD
			public Identifier EventPrefabId { get; set; }

			// Token: 0x170013C4 RID: 5060
			// (get) Token: 0x06004BD1 RID: 19409 RVA: 0x001DC0C6 File Offset: 0x001DA2C6
			// (set) Token: 0x06004BD2 RID: 19410 RVA: 0x001DC0CE File Offset: 0x001DA2CE
			public Identifier LogEntryId { get; set; }

			// Token: 0x170013C5 RID: 5061
			// (get) Token: 0x06004BD3 RID: 19411 RVA: 0x001DC0D7 File Offset: 0x001DA2D7
			// (set) Token: 0x06004BD4 RID: 19412 RVA: 0x001DC0DF File Offset: 0x001DA2DF
			public string Text { get; set; }

			// Token: 0x06004BD5 RID: 19413 RVA: 0x001DC0E8 File Offset: 0x001DA2E8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetEventLogEntry");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004BD6 RID: 19414 RVA: 0x001DC134 File Offset: 0x001DA334
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("EventPrefabId = ");
				builder.Append(this.EventPrefabId.ToString());
				builder.Append(", LogEntryId = ");
				builder.Append(this.LogEntryId.ToString());
				builder.Append(", Text = ");
				builder.Append(this.Text);
				return true;
			}

			// Token: 0x06004BD7 RID: 19415 RVA: 0x001DC1A9 File Offset: 0x001DA3A9
			[CompilerGenerated]
			public static bool operator !=(EventManager.NetEventLogEntry left, EventManager.NetEventLogEntry right)
			{
				return !(left == right);
			}

			// Token: 0x06004BD8 RID: 19416 RVA: 0x001DC1B5 File Offset: 0x001DA3B5
			[CompilerGenerated]
			public static bool operator ==(EventManager.NetEventLogEntry left, EventManager.NetEventLogEntry right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004BD9 RID: 19417 RVA: 0x001DC1BF File Offset: 0x001DA3BF
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Identifier>.Default.GetHashCode(this.<EventPrefabId>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<LogEntryId>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Text>k__BackingField);
			}

			// Token: 0x06004BDA RID: 19418 RVA: 0x001DC1FF File Offset: 0x001DA3FF
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EventManager.NetEventLogEntry && this.Equals((EventManager.NetEventLogEntry)obj);
			}

			// Token: 0x06004BDB RID: 19419 RVA: 0x001DC218 File Offset: 0x001DA418
			[CompilerGenerated]
			public bool Equals(EventManager.NetEventLogEntry other)
			{
				return EqualityComparer<Identifier>.Default.Equals(this.<EventPrefabId>k__BackingField, other.<EventPrefabId>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<LogEntryId>k__BackingField, other.<LogEntryId>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Text>k__BackingField, other.<Text>k__BackingField);
			}

			// Token: 0x06004BDC RID: 19420 RVA: 0x001DC26D File Offset: 0x001DA46D
			[CompilerGenerated]
			public void Deconstruct(out Identifier EventPrefabId, out Identifier LogEntryId, out string Text)
			{
				EventPrefabId = this.EventPrefabId;
				LogEntryId = this.LogEntryId;
				Text = this.Text;
			}
		}

		// Token: 0x020005B9 RID: 1465
		[NetworkSerialize(28)]
		public readonly struct NetEventObjective : INetSerializableStruct, IEquatable<EventManager.NetEventObjective>
		{
			// Token: 0x06004BDD RID: 19421 RVA: 0x001DC28F File Offset: 0x001DA48F
			public NetEventObjective(EventObjectiveAction.SegmentActionType Type, Identifier Identifier, Identifier ObjectiveTag, Identifier TextTag, Identifier ParentObjectiveId, bool CanBeCompleted)
			{
				this.Type = Type;
				this.Identifier = Identifier;
				this.ObjectiveTag = ObjectiveTag;
				this.TextTag = TextTag;
				this.ParentObjectiveId = ParentObjectiveId;
				this.CanBeCompleted = CanBeCompleted;
			}

			// Token: 0x170013C6 RID: 5062
			// (get) Token: 0x06004BDE RID: 19422 RVA: 0x001DC2BE File Offset: 0x001DA4BE
			// (set) Token: 0x06004BDF RID: 19423 RVA: 0x001DC2C6 File Offset: 0x001DA4C6
			public EventObjectiveAction.SegmentActionType Type { get; set; }

			// Token: 0x170013C7 RID: 5063
			// (get) Token: 0x06004BE0 RID: 19424 RVA: 0x001DC2CF File Offset: 0x001DA4CF
			// (set) Token: 0x06004BE1 RID: 19425 RVA: 0x001DC2D7 File Offset: 0x001DA4D7
			public Identifier Identifier { get; set; }

			// Token: 0x170013C8 RID: 5064
			// (get) Token: 0x06004BE2 RID: 19426 RVA: 0x001DC2E0 File Offset: 0x001DA4E0
			// (set) Token: 0x06004BE3 RID: 19427 RVA: 0x001DC2E8 File Offset: 0x001DA4E8
			public Identifier ObjectiveTag { get; set; }

			// Token: 0x170013C9 RID: 5065
			// (get) Token: 0x06004BE4 RID: 19428 RVA: 0x001DC2F1 File Offset: 0x001DA4F1
			// (set) Token: 0x06004BE5 RID: 19429 RVA: 0x001DC2F9 File Offset: 0x001DA4F9
			public Identifier TextTag { get; set; }

			// Token: 0x170013CA RID: 5066
			// (get) Token: 0x06004BE6 RID: 19430 RVA: 0x001DC302 File Offset: 0x001DA502
			// (set) Token: 0x06004BE7 RID: 19431 RVA: 0x001DC30A File Offset: 0x001DA50A
			public Identifier ParentObjectiveId { get; set; }

			// Token: 0x170013CB RID: 5067
			// (get) Token: 0x06004BE8 RID: 19432 RVA: 0x001DC313 File Offset: 0x001DA513
			// (set) Token: 0x06004BE9 RID: 19433 RVA: 0x001DC31B File Offset: 0x001DA51B
			public bool CanBeCompleted { get; set; }

			// Token: 0x06004BEA RID: 19434 RVA: 0x001DC324 File Offset: 0x001DA524
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetEventObjective");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004BEB RID: 19435 RVA: 0x001DC370 File Offset: 0x001DA570
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Type = ");
				builder.Append(this.Type.ToString());
				builder.Append(", Identifier = ");
				builder.Append(this.Identifier.ToString());
				builder.Append(", ObjectiveTag = ");
				builder.Append(this.ObjectiveTag.ToString());
				builder.Append(", TextTag = ");
				builder.Append(this.TextTag.ToString());
				builder.Append(", ParentObjectiveId = ");
				builder.Append(this.ParentObjectiveId.ToString());
				builder.Append(", CanBeCompleted = ");
				builder.Append(this.CanBeCompleted.ToString());
				return true;
			}

			// Token: 0x06004BEC RID: 19436 RVA: 0x001DC468 File Offset: 0x001DA668
			[CompilerGenerated]
			public static bool operator !=(EventManager.NetEventObjective left, EventManager.NetEventObjective right)
			{
				return !(left == right);
			}

			// Token: 0x06004BED RID: 19437 RVA: 0x001DC474 File Offset: 0x001DA674
			[CompilerGenerated]
			public static bool operator ==(EventManager.NetEventObjective left, EventManager.NetEventObjective right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004BEE RID: 19438 RVA: 0x001DC480 File Offset: 0x001DA680
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((EqualityComparer<EventObjectiveAction.SegmentActionType>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<Identifier>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<ObjectiveTag>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TextTag>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<ParentObjectiveId>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<CanBeCompleted>k__BackingField);
			}

			// Token: 0x06004BEF RID: 19439 RVA: 0x001DC510 File Offset: 0x001DA710
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EventManager.NetEventObjective && this.Equals((EventManager.NetEventObjective)obj);
			}

			// Token: 0x06004BF0 RID: 19440 RVA: 0x001DC528 File Offset: 0x001DA728
			[CompilerGenerated]
			public bool Equals(EventManager.NetEventObjective other)
			{
				return EqualityComparer<EventObjectiveAction.SegmentActionType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<ObjectiveTag>k__BackingField, other.<ObjectiveTag>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TextTag>k__BackingField, other.<TextTag>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<ParentObjectiveId>k__BackingField, other.<ParentObjectiveId>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<CanBeCompleted>k__BackingField, other.<CanBeCompleted>k__BackingField);
			}

			// Token: 0x06004BF1 RID: 19441 RVA: 0x001DC5C8 File Offset: 0x001DA7C8
			[CompilerGenerated]
			public void Deconstruct(out EventObjectiveAction.SegmentActionType Type, out Identifier Identifier, out Identifier ObjectiveTag, out Identifier TextTag, out Identifier ParentObjectiveId, out bool CanBeCompleted)
			{
				Type = this.Type;
				Identifier = this.Identifier;
				ObjectiveTag = this.ObjectiveTag;
				TextTag = this.TextTag;
				ParentObjectiveId = this.ParentObjectiveId;
				CanBeCompleted = this.CanBeCompleted;
			}
		}

		// Token: 0x020005BA RID: 1466
		private readonly struct TimeStamp
		{
			// Token: 0x06004BF2 RID: 19442 RVA: 0x001DC618 File Offset: 0x001DA818
			public TimeStamp(Event e)
			{
				this.Event = e;
				this.Time = Timing.TotalTime;
			}

			// Token: 0x04002761 RID: 10081
			public readonly double Time;

			// Token: 0x04002762 RID: 10082
			public readonly Event Event;
		}
	}
}
