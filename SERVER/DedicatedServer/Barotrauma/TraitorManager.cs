using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200004A RID: 74
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class TraitorManager
	{
		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06000BDA RID: 3034 RVA: 0x0007076A File Offset: 0x0006E96A
		public IEnumerable<TraitorManager.ActiveTraitorEvent> ActiveEvents
		{
			get
			{
				return this.activeEvents;
			}
		}

		// Token: 0x06000BDB RID: 3035 RVA: 0x00070774 File Offset: 0x0006E974
		public bool IsTraitor(Character character)
		{
			return this.activeEvents.Any((TraitorManager.ActiveTraitorEvent e) => e.Traitor.Character == character);
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x000707A5 File Offset: 0x0006E9A5
		public TraitorManager(GameServer server)
		{
			this.server = server;
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x000707CA File Offset: 0x0006E9CA
		public void Initialize(EventManager eventManager, Level level)
		{
			this.eventManager = eventManager;
			this.level = level;
			this.startTimer = (float)Rand.Range(60, 200, Rand.RandSync.Unsynced);
			this.started = false;
			this.results = null;
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x00070804 File Offset: 0x0006EA04
		private bool TryCreateTraitorEvents(EventManager eventManager, Level level)
		{
			TraitorManager.<>c__DisplayClass19_0 CS$<>8__locals1 = new TraitorManager.<>c__DisplayClass19_0();
			CS$<>8__locals1.level = level;
			CS$<>8__locals1.<>4__this = this;
			IEnumerable<TraitorEventPrefab> eventPrefabs = (from p in EventPrefab.Prefabs
			where p is TraitorEventPrefab
			select p).Cast<TraitorEventPrefab>();
			if (!eventPrefabs.Any<TraitorEventPrefab>())
			{
				DebugConsole.AddWarning("No traitor event available in any of the enabled content packages.", null);
				return false;
			}
			if (this.server.ConnectedClients.Count(new Func<Client, bool>(this.IsClientViableTraitor)) < this.server.ServerSettings.TraitorsMinPlayerCount)
			{
				DebugConsole.AddWarning("Not enough clients to create a traitor event. Active traitor events: " + this.activeEvents.Count.ToString(), null);
				return false;
			}
			CS$<>8__locals1.maxDangerLevel = this.server.ServerSettings.TraitorDangerLevel;
			CS$<>8__locals1.playerCount = this.server.ConnectedClients.Count((Client c) => c.Character != null && !c.Character.Removed);
			TraitorManager.<>c__DisplayClass19_0 CS$<>8__locals2 = CS$<>8__locals1;
			GameSession gameSession = GameMain.GameSession;
			CS$<>8__locals2.campaign = ((gameSession != null) ? gameSession.Campaign : null);
			List<TraitorEventPrefab> suitablePrefabs = (from e in eventPrefabs
			where EventManager.IsLevelSuitable(e, CS$<>8__locals1.level)
			where e.ReputationRequirementsMet(CS$<>8__locals1.campaign)
			where e.MissionRequirementsMet(GameMain.GameSession)
			where e.LevelRequirementsMet(CS$<>8__locals1.level)
			where e.DangerLevel <= CS$<>8__locals1.maxDangerLevel
			where CS$<>8__locals1.playerCount >= e.MinPlayerCount
			select e).ToList<TraitorEventPrefab>();
			if (!suitablePrefabs.Any<TraitorEventPrefab>())
			{
				DebugConsole.Log("No suitable traitor missions available for the level.");
				return false;
			}
			using (IEnumerator<TraitorManager.PreviousTraitorEvent> enumerator = this.previousTraitorEvents.Reverse<TraitorManager.PreviousTraitorEvent>().DistinctBy((TraitorManager.PreviousTraitorEvent e) => e.Traitor).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TraitorManager.PreviousTraitorEvent previousEvent = enumerator.Current;
					if (previousEvent.State == TraitorEvent.State.Completed && previousEvent.TraitorEvent.IsChainable && this.IsClientViableTraitor(previousEvent.Traitor))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(125, 2);
						defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(previousEvent.Traitor, null));
						defaultInterpolatedStringHandler.AppendLiteral(" successfully completed a traitor event (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(previousEvent.TraitorEvent.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(") on a previous round. Attempting to give choose them a new, more dangerous event...");
						GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Traitors);
						Func<Identifier, bool> <>9__11;
						TraitorEventPrefab suitablePrefab = suitablePrefabs.GetRandomUnsynced(delegate(TraitorEventPrefab p)
						{
							IEnumerable<Identifier> requiredCompletedTags = p.RequiredCompletedTags;
							Func<Identifier, bool> predicate;
							if ((predicate = <>9__11) == null)
							{
								predicate = (<>9__11 = ((Identifier t) => previousEvent.TraitorEvent.Identifier == t || previousEvent.TraitorEvent.Tags.Contains(t)));
							}
							return requiredCompletedTags.Any(predicate);
						});
						if (suitablePrefab == null)
						{
							suitablePrefab = suitablePrefabs.GetRandomUnsynced((TraitorEventPrefab p) => p.RequiredCompletedTags.None(null) && p.DangerLevel > previousEvent.TraitorEvent.DangerLevel && p.Faction == previousEvent.TraitorEvent.Faction);
						}
						if (suitablePrefab != null)
						{
							this.CreateTraitorEvent(eventManager, suitablePrefab, previousEvent.Traitor);
							return true;
						}
						GameServer.Log("Could not find a suitable, more difficult traitor event for " + NetworkMember.ClientLogName(previousEvent.Traitor, null) + ".", ServerLog.MessageType.Traitors);
					}
				}
			}
			GameSession gameSession2 = GameMain.GameSession;
			TraitorEventPrefab selectedPrefab;
			if (((gameSession2 != null) ? gameSession2.Campaign : null) == null)
			{
				selectedPrefab = suitablePrefabs.GetRandomByWeight(new Func<TraitorEventPrefab, float>(this.GetTraitorEventPrefabCommonness), Rand.RandSync.Unsynced);
			}
			else
			{
				List<TraitorEventPrefab> suitableInitialPrefabs = suitablePrefabs.FindAll((TraitorEventPrefab e) => e.RequiredCompletedTags.None(null) && base.<TryCreateTraitorEvents>g__IsSuitableDangerLevel|13(e));
				selectedPrefab = suitableInitialPrefabs.GetRandomByWeight(new Func<TraitorEventPrefab, float>(this.GetTraitorEventPrefabCommonness), Rand.RandSync.Unsynced);
				if (selectedPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(87, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not find a suitable danger level ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(1);
					defaultInterpolatedStringHandler2.AppendLiteral(" traitor event. Choosing a random event instead.");
					GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.Traitors);
					selectedPrefab = suitablePrefabs.GetRandomByWeight(new Func<TraitorEventPrefab, float>(this.GetTraitorEventPrefabCommonness), Rand.RandSync.Unsynced);
				}
			}
			if (selectedPrefab == null)
			{
				return false;
			}
			Client selectedTraitor = this.SelectRandomTraitor();
			if (selectedTraitor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Could not find a suitable traitor for the event \"");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(selectedPrefab.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, selectedPrefab.ContentPackage, false, false);
				return false;
			}
			this.CreateTraitorEvent(eventManager, selectedPrefab, selectedTraitor);
			return true;
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x00070C58 File Offset: 0x0006EE58
		[NullableContext(2)]
		private Client SelectRandomTraitor()
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				GameServer.Log("Choosing a random traitor... Available traitors:" + string.Concat(this.server.ConnectedClients.Where(new Func<Client, bool>(this.IsClientViableTraitor)).Select(delegate(Client c)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
					defaultInterpolatedStringHandler.AppendLiteral("\n  - ");
					defaultInterpolatedStringHandler.AppendFormatted(c.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)(this.GetTraitorProbability(c) * 100f));
					defaultInterpolatedStringHandler.AppendLiteral("%)");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				})), ServerLog.MessageType.Traitors);
			}
			return this.server.ConnectedClients.Where(new Func<Client, bool>(this.IsClientViableTraitor)).GetRandomByWeight(new Func<Client, float>(this.GetTraitorProbability), Rand.RandSync.Unsynced);
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x00070CE4 File Offset: 0x0006EEE4
		private IEnumerable<Client> SelectSecondaryTraitors(TraitorEvent traitorEvent, Client mainTraitor)
		{
			if (traitorEvent.Prefab.SecondaryTraitorPercentage <= 0f && traitorEvent.Prefab.SecondaryTraitorAmount <= 0)
			{
				return Enumerable.Empty<Client>();
			}
			List<Client> viableTraitors = (from c in this.server.ConnectedClients
			where c != mainTraitor && this.IsClientViableTraitor(c)
			select c).ToList<Client>();
			int amountToChoose = (int)Math.Ceiling((double)((float)viableTraitors.Count * (traitorEvent.Prefab.SecondaryTraitorPercentage / 100f)));
			amountToChoose = Math.Max(amountToChoose, traitorEvent.Prefab.SecondaryTraitorAmount);
			if (amountToChoose > viableTraitors.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(169, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in traitor event ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(traitorEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(". Not enough players to choose ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(amountToChoose);
				defaultInterpolatedStringHandler.AppendLiteral(" secondary traitors. ");
				defaultInterpolatedStringHandler.AppendLiteral("Make sure the ");
				defaultInterpolatedStringHandler.AppendFormatted("MinPlayerCount");
				defaultInterpolatedStringHandler.AppendLiteral(" of the event is high enough to support to desired amount of secondary traitors.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, traitorEvent.Prefab.ContentPackage, false, false);
				amountToChoose = viableTraitors.Count;
			}
			List<Client> traitors = new List<Client>();
			for (int i = 0; i < amountToChoose; i++)
			{
				Client traitor = viableTraitors.GetRandomUnsynced<Client>();
				if (traitor != null)
				{
					viableTraitors.Remove(traitor);
					traitors.Add(traitor);
				}
			}
			return traitors;
		}

		// Token: 0x06000BE1 RID: 3041 RVA: 0x00070E50 File Offset: 0x0006F050
		private bool IsClientViableTraitor(Client client)
		{
			return client != null && this.server.ConnectedClients.Contains(client) && client.Character != null && !client.Character.IsIncapacitated && !client.Character.Removed && this.activeEvents.None((TraitorManager.ActiveTraitorEvent e) => e.Traitor == client);
		}

		// Token: 0x06000BE2 RID: 3042 RVA: 0x00070ED4 File Offset: 0x0006F0D4
		private float GetTraitorEventPrefabCommonness(TraitorEventPrefab prefab)
		{
			int? roundsSinceLastSelected = this.GetRoundsSinceLastSelected((TraitorManager.PreviousTraitorEvent e) => e.TraitorEvent == prefab);
			if (roundsSinceLastSelected != null)
			{
				float normalizedRoundsSinceLastSelected = MathUtils.InverseLerp(0f, 10f, (float)roundsSinceLastSelected.Value);
				return prefab.Commonness * normalizedRoundsSinceLastSelected * normalizedRoundsSinceLastSelected;
			}
			return prefab.Commonness;
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x00070F40 File Offset: 0x0006F140
		private float GetTraitorProbability(Client client)
		{
			int? roundsSinceLastSelected = this.GetRoundsSinceLastSelected((TraitorManager.PreviousTraitorEvent e) => e.Traitor == client);
			if (roundsSinceLastSelected != null)
			{
				float normalizedRoundsSinceLastSelected = MathUtils.InverseLerp(0f, 10f, (float)roundsSinceLastSelected.Value);
				return normalizedRoundsSinceLastSelected * normalizedRoundsSinceLastSelected;
			}
			return 1f;
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x00070F98 File Offset: 0x0006F198
		private int? GetRoundsSinceLastSelected(Func<TraitorManager.PreviousTraitorEvent, bool> condition)
		{
			for (int i = this.previousTraitorEvents.Count - 1; i >= 0; i--)
			{
				if (condition(this.previousTraitorEvents[i]))
				{
					return new int?(this.previousTraitorEvents.Count - i);
				}
			}
			return null;
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x00070FF0 File Offset: 0x0006F1F0
		private void CreateTraitorEvent(EventManager eventManager, TraitorEventPrefab selectedPrefab, Client traitor)
		{
			TraitorEvent newEvent;
			if (selectedPrefab.TryCreateInstance<TraitorEvent>(eventManager.RandomSeed, out newEvent))
			{
				IEnumerable<Client> secondaryTraitors = this.SelectSecondaryTraitors(newEvent, traitor);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(traitor, null));
				defaultInterpolatedStringHandler.AppendLiteral(" was selected as a traitor. Selected event: ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(selectedPrefab.Name);
				string logMessage = defaultInterpolatedStringHandler.ToStringAndClear();
				if (secondaryTraitors.Any<Client>())
				{
					logMessage = logMessage + ", secondary traitors: " + string.Join(", ", from c in secondaryTraitors
					select NetworkMember.ClientLogName(c, null));
				}
				GameServer.Log(logMessage, ServerLog.MessageType.Traitors);
				TraitorEvent newEvent2 = newEvent;
				newEvent2.OnStateChanged = (Action)Delegate.Combine(newEvent2.OnStateChanged, new Action(delegate()
				{
					this.SendCurrentState(newEvent);
				}));
				this.activeEvents.Add(new TraitorManager.ActiveTraitorEvent(traitor, newEvent));
				newEvent.SetTraitor(traitor);
				newEvent.SetSecondaryTraitors(secondaryTraitors);
				eventManager.ActivateEvent(newEvent);
				this.SendCurrentState(newEvent);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(60, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Failed to create an instance of the traitor event prefab \"");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(selectedPrefab.Identifier);
			defaultInterpolatedStringHandler2.AppendLiteral("\"!");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, selectedPrefab.ContentPackage, false, false);
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x00071168 File Offset: 0x0006F368
		public void ForceTraitorEvent(TraitorEventPrefab traitorEventPrefab)
		{
			if (this.eventManager == null)
			{
				throw new InvalidOperationException("EventManager was null. TraitorManager may not have been initialized properly.");
			}
			Client traitor = this.SelectRandomTraitor();
			if (traitor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a suitable traitor for the event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(traitorEventPrefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, traitorEventPrefab.ContentPackage, false, false);
				return;
			}
			this.CreateTraitorEvent(this.eventManager, traitorEventPrefab, traitor);
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x000711E5 File Offset: 0x0006F3E5
		public void SkipStartDelay()
		{
			this.startTimer = 0f;
			if (this.activeEvents.Any<TraitorManager.ActiveTraitorEvent>())
			{
				this.started = true;
			}
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x00071208 File Offset: 0x0006F408
		public void Update(float deltaTime)
		{
			if (!this.Enabled)
			{
				return;
			}
			if (!this.started)
			{
				Level level = this.level;
				LevelData levelData = (level != null) ? level.LevelData : null;
				if (levelData != null && levelData.Type == LevelData.LevelType.LocationConnection && Submarine.MainSub != null && Submarine.MainSub.WorldPosition.X > (float)(this.level.Size.X / 2))
				{
					this.startTimer = Math.Min(this.startTimer, 10f);
				}
				this.startTimer -= deltaTime;
				if (this.startTimer >= 0f)
				{
					return;
				}
				if (this.eventManager == null)
				{
					throw new InvalidOperationException("EventManager was null. TraitorManager may not have been initialized properly.");
				}
				if (this.level == null)
				{
					throw new InvalidOperationException("Level was null. TraitorManager may not have been initialized properly.");
				}
				if (this.TryCreateTraitorEvents(this.eventManager, this.level))
				{
					this.started = true;
					return;
				}
				this.startTimer = (float)Rand.Range(60, 200, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06000BE9 RID: 3049 RVA: 0x000712FC File Offset: 0x0006F4FC
		public void EndRound()
		{
			Client votedAsTraitor = this.GetClientAccusedAsTraitor();
			foreach (TraitorManager.ActiveTraitorEvent activeEvent in this.activeEvents)
			{
				if (this.results != null)
				{
					DebugConsole.AddWarning("Multiple traitor events active during the round, only displaying the results for the last one.", null);
				}
				this.results = new TraitorManager.TraitorResults?(new TraitorManager.TraitorResults(votedAsTraitor, activeEvent.TraitorEvent));
				if (this.results.Value.MoneyPenalty > 0)
				{
					GameSession gameSession = GameMain.GameSession;
					if (gameSession != null)
					{
						CampaignMode campaign = gameSession.Campaign;
						if (campaign != null)
						{
							Wallet bank = campaign.Bank;
							if (bank != null)
							{
								bank.TryDeduct(this.results.Value.MoneyPenalty);
							}
						}
					}
				}
				if (activeEvent.TraitorEvent.CurrentState != TraitorEvent.State.Completed)
				{
					activeEvent.TraitorEvent.CurrentState = TraitorEvent.State.Failed;
				}
				GameServer.Log(NetworkMember.ClientLogName(activeEvent.Traitor, null) + ((activeEvent.TraitorEvent.CurrentState == TraitorEvent.State.Completed) ? " completed their traitor objective successfully." : " failed to complete their traitor objective."), ServerLog.MessageType.Traitors);
				if (this.results.Value.VotedCorrectTraitor)
				{
					GameServer.Log(NetworkMember.ClientLogName(activeEvent.Traitor, null) + " was correctly identified as the traitor, and will not receive any rewards.", ServerLog.MessageType.Traitors);
					activeEvent.TraitorEvent.CurrentState = TraitorEvent.State.Failed;
				}
				this.previousTraitorEvents.Add(new TraitorManager.PreviousTraitorEvent(activeEvent.TraitorEvent.Prefab, activeEvent.TraitorEvent.CurrentState, activeEvent.Traitor));
				if (activeEvent.TraitorEvent.CurrentState == TraitorEvent.State.Completed)
				{
					Client traitor = activeEvent.TraitorEvent.Traitor;
					AchievementManager.OnTraitorWin((traitor != null) ? traitor.Character : null);
					foreach (Client secondaryTraitor in activeEvent.TraitorEvent.SecondaryTraitors)
					{
						AchievementManager.OnTraitorWin((secondaryTraitor != null) ? secondaryTraitor.Character : null);
					}
				}
			}
			if (this.previousTraitorEvents.Count > 10)
			{
				this.previousTraitorEvents.RemoveRange(0, this.previousTraitorEvents.Count - 10);
			}
			this.activeEvents.Clear();
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x00071554 File Offset: 0x0006F754
		[NullableContext(2)]
		public Client GetClientAccusedAsTraitor()
		{
			int voteCount;
			Client votedAsTraitor = Voting.HighestVoted<Client>(VoteType.Traitor, this.server.ConnectedClients.Where(delegate(Client c)
			{
				Character character = c.Character;
				return character != null && !character.IsDead;
			}), out voteCount);
			if ((float)voteCount < (float)this.server.ConnectedClients.Count * this.server.ServerSettings.MinPercentageOfPlayersForTraitorAccusation / 100f)
			{
				votedAsTraitor = null;
			}
			return votedAsTraitor;
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x000715C9 File Offset: 0x0006F7C9
		public TraitorManager.TraitorResults? GetEndResults()
		{
			return this.results;
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x000715D4 File Offset: 0x0006F7D4
		public XElement Save()
		{
			XElement element = new XElement("TraitorManager", new XAttribute("version", GameMain.Version.ToString()));
			foreach (TraitorManager.PreviousTraitorEvent previousEvent in this.previousTraitorEvents)
			{
				previousEvent.Save(element);
			}
			return element;
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x00071654 File Offset: 0x0006F854
		public void Load(XElement traitorManagerElement)
		{
			this.previousTraitorEvents.Clear();
			foreach (XElement subElement in traitorManagerElement.Elements())
			{
				Identifier identifier = subElement.Name.ToIdentifier<XName>();
				if (identifier == "PreviousTraitorEvent")
				{
					TraitorManager.PreviousTraitorEvent previousTraitorEvent = TraitorManager.PreviousTraitorEvent.Load(subElement);
					if (previousTraitorEvent != null)
					{
						this.previousTraitorEvents.Add(previousTraitorEvent);
					}
				}
			}
		}

		// Token: 0x06000BEE RID: 3054 RVA: 0x000716D8 File Offset: 0x0006F8D8
		public void SendCurrentState(TraitorEvent ev)
		{
			if (((ev != null) ? ev.Traitor : null) == null)
			{
				return;
			}
			WriteOnlyMessage msg = new WriteOnlyMessage();
			msg.WriteByte(22);
			msg.WriteByte((byte)ev.CurrentState);
			msg.WriteIdentifier(ev.Prefab.Identifier);
			this.server.SendTraitorMessage(msg, ev.Traitor);
		}

		// Token: 0x04000512 RID: 1298
		private const int MaxPreviousEventHistory = 10;

		// Token: 0x04000513 RID: 1299
		private const int StartDelayMin = 60;

		// Token: 0x04000514 RID: 1300
		private const int StartDelayMax = 200;

		// Token: 0x04000515 RID: 1301
		private float startTimer;

		// Token: 0x04000516 RID: 1302
		private bool started;

		// Token: 0x04000517 RID: 1303
		private TraitorManager.TraitorResults? results;

		// Token: 0x04000518 RID: 1304
		private readonly List<TraitorManager.PreviousTraitorEvent> previousTraitorEvents = new List<TraitorManager.PreviousTraitorEvent>();

		// Token: 0x04000519 RID: 1305
		private readonly List<TraitorManager.ActiveTraitorEvent> activeEvents = new List<TraitorManager.ActiveTraitorEvent>();

		// Token: 0x0400051A RID: 1306
		private readonly GameServer server;

		// Token: 0x0400051B RID: 1307
		[Nullable(2)]
		private EventManager eventManager;

		// Token: 0x0400051C RID: 1308
		[Nullable(2)]
		private Level level;

		// Token: 0x0400051D RID: 1309
		public bool Enabled;

		// Token: 0x0200074E RID: 1870
		[Nullable(0)]
		public class PreviousTraitorEvent
		{
			// Token: 0x17001433 RID: 5171
			// (get) Token: 0x06005196 RID: 20886 RVA: 0x001E961A File Offset: 0x001E781A
			public TraitorEventPrefab TraitorEvent { get; }

			// Token: 0x17001434 RID: 5172
			// (get) Token: 0x06005197 RID: 20887 RVA: 0x001E9622 File Offset: 0x001E7822
			public TraitorEvent.State State { get; }

			// Token: 0x17001435 RID: 5173
			// (get) Token: 0x06005198 RID: 20888 RVA: 0x001E962A File Offset: 0x001E782A
			public Client Traitor
			{
				get
				{
					return GameMain.Server.ConnectedClients.Find((Client c) => this.traitorAccountId.IsSome() && this.traitorAccountId == c.AccountId) ?? GameMain.Server.ConnectedClients.Find((Client c) => this.traitorAddress == c.Connection.Endpoint.Address);
				}
			}

			// Token: 0x06005199 RID: 20889 RVA: 0x001E9666 File Offset: 0x001E7866
			public PreviousTraitorEvent(TraitorEventPrefab traitorEvent, TraitorEvent.State state, Client traitor)
			{
				this.TraitorEvent = traitorEvent;
				this.State = state;
				this.traitorAddress = traitor.Connection.Endpoint.Address;
				this.traitorAccountId = traitor.AccountId;
			}

			// Token: 0x0600519A RID: 20890 RVA: 0x001E969E File Offset: 0x001E789E
			private PreviousTraitorEvent(TraitorEventPrefab traitorEvent, TraitorEvent.State state, [Nullable(new byte[]
			{
				0,
				1
			})] Option<AccountId> accountId, Address address)
			{
				this.TraitorEvent = traitorEvent;
				this.State = state;
				this.traitorAddress = address;
				this.traitorAccountId = accountId;
			}

			// Token: 0x0600519B RID: 20891 RVA: 0x001E96C4 File Offset: 0x001E78C4
			public void Save(XElement parentElement)
			{
				parentElement.Add(new XElement("PreviousTraitorEvent", new object[]
				{
					new XAttribute("id", this.TraitorEvent.Identifier),
					new XAttribute("state", this.State),
					new XAttribute("accountid", this.traitorAccountId),
					new XAttribute("address", this.traitorAddress)
				}));
			}

			// Token: 0x0600519C RID: 20892 RVA: 0x001E9760 File Offset: 0x001E7960
			[return: Nullable(2)]
			public static TraitorManager.PreviousTraitorEvent Load(XElement subElement)
			{
				Identifier traitorEventId = subElement.GetAttributeIdentifier("id", Identifier.Empty);
				TraitorEvent.State state = subElement.GetAttributeEnum("state", Barotrauma.TraitorEvent.State.Failed);
				Option<AccountId> accountId = AccountId.Parse(subElement.GetAttributeString("accountid", null) ?? subElement.GetAttributeString("steamid", ""));
				Address address = Address.Parse(subElement.GetAttributeString("address", null) ?? subElement.GetAttributeString("endpoint", "")).Fallback(new UnknownAddress());
				EventPrefab prefab;
				if (EventPrefab.Prefabs.TryGet(traitorEventId, out prefab))
				{
					TraitorEventPrefab traitorEventPrefab = prefab as TraitorEventPrefab;
					if (traitorEventPrefab != null)
					{
						return new TraitorManager.PreviousTraitorEvent(traitorEventPrefab, state, accountId, address);
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error when loading ");
				defaultInterpolatedStringHandler.AppendFormatted("TraitorManager");
				defaultInterpolatedStringHandler.AppendLiteral(": could not find a traitor event prefab with the identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(traitorEventId);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return null;
			}

			// Token: 0x04002CA0 RID: 11424
			private readonly Address traitorAddress;

			// Token: 0x04002CA1 RID: 11425
			[Nullable(new byte[]
			{
				0,
				1
			})]
			private readonly Option<AccountId> traitorAccountId;
		}

		// Token: 0x0200074F RID: 1871
		[Nullable(0)]
		public readonly struct ActiveTraitorEvent : IEquatable<TraitorManager.ActiveTraitorEvent>
		{
			// Token: 0x0600519F RID: 20895 RVA: 0x001E989E File Offset: 0x001E7A9E
			public ActiveTraitorEvent(Client Traitor, TraitorEvent TraitorEvent)
			{
				this.Traitor = Traitor;
				this.TraitorEvent = TraitorEvent;
			}

			// Token: 0x17001436 RID: 5174
			// (get) Token: 0x060051A0 RID: 20896 RVA: 0x001E98AE File Offset: 0x001E7AAE
			// (set) Token: 0x060051A1 RID: 20897 RVA: 0x001E98B6 File Offset: 0x001E7AB6
			public Client Traitor { get; set; }

			// Token: 0x17001437 RID: 5175
			// (get) Token: 0x060051A2 RID: 20898 RVA: 0x001E98BF File Offset: 0x001E7ABF
			// (set) Token: 0x060051A3 RID: 20899 RVA: 0x001E98C7 File Offset: 0x001E7AC7
			public TraitorEvent TraitorEvent { get; set; }

			// Token: 0x060051A4 RID: 20900 RVA: 0x001E98D0 File Offset: 0x001E7AD0
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ActiveTraitorEvent");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060051A5 RID: 20901 RVA: 0x001E991C File Offset: 0x001E7B1C
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Traitor = ");
				builder.Append(this.Traitor);
				builder.Append(", TraitorEvent = ");
				builder.Append(this.TraitorEvent);
				return true;
			}

			// Token: 0x060051A6 RID: 20902 RVA: 0x001E9951 File Offset: 0x001E7B51
			[CompilerGenerated]
			public static bool operator !=(TraitorManager.ActiveTraitorEvent left, TraitorManager.ActiveTraitorEvent right)
			{
				return !(left == right);
			}

			// Token: 0x060051A7 RID: 20903 RVA: 0x001E995D File Offset: 0x001E7B5D
			[CompilerGenerated]
			public static bool operator ==(TraitorManager.ActiveTraitorEvent left, TraitorManager.ActiveTraitorEvent right)
			{
				return left.Equals(right);
			}

			// Token: 0x060051A8 RID: 20904 RVA: 0x001E9967 File Offset: 0x001E7B67
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<Client>.Default.GetHashCode(this.<Traitor>k__BackingField) * -1521134295 + EqualityComparer<TraitorEvent>.Default.GetHashCode(this.<TraitorEvent>k__BackingField);
			}

			// Token: 0x060051A9 RID: 20905 RVA: 0x001E9990 File Offset: 0x001E7B90
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is TraitorManager.ActiveTraitorEvent && this.Equals((TraitorManager.ActiveTraitorEvent)obj);
			}

			// Token: 0x060051AA RID: 20906 RVA: 0x001E99A8 File Offset: 0x001E7BA8
			[CompilerGenerated]
			public bool Equals(TraitorManager.ActiveTraitorEvent other)
			{
				return EqualityComparer<Client>.Default.Equals(this.<Traitor>k__BackingField, other.<Traitor>k__BackingField) && EqualityComparer<TraitorEvent>.Default.Equals(this.<TraitorEvent>k__BackingField, other.<TraitorEvent>k__BackingField);
			}

			// Token: 0x060051AB RID: 20907 RVA: 0x001E99DA File Offset: 0x001E7BDA
			[CompilerGenerated]
			public void Deconstruct(out Client Traitor, out TraitorEvent TraitorEvent)
			{
				Traitor = this.Traitor;
				TraitorEvent = this.TraitorEvent;
			}
		}

		// Token: 0x02000750 RID: 1872
		[NullableContext(0)]
		public struct TraitorResults : INetSerializableStruct
		{
			// Token: 0x060051AC RID: 20908 RVA: 0x001E99EC File Offset: 0x001E7BEC
			[NullableContext(1)]
			public TraitorResults([Nullable(2)] Client votedAsTraitor, TraitorEvent traitorEvent)
			{
				this.VotedAsTraitorClientSessionId = ((votedAsTraitor != null) ? votedAsTraitor.SessionId : 0);
				this.VotedCorrectTraitor = (votedAsTraitor == traitorEvent.Traitor);
				if (traitorEvent.Prefab.AllowAccusingSecondaryTraitor && !this.VotedCorrectTraitor)
				{
					this.VotedCorrectTraitor = traitorEvent.SecondaryTraitors.Contains(votedAsTraitor);
				}
				this.ObjectiveSuccessful = (traitorEvent.CurrentState == TraitorEvent.State.Completed);
				this.MoneyPenalty = ((votedAsTraitor != null && !this.VotedCorrectTraitor) ? traitorEvent.Prefab.MoneyPenaltyForUnfoundedTraitorAccusation : 0);
				this.TraitorEventIdentifier = traitorEvent.Prefab.Identifier;
			}

			// Token: 0x060051AD RID: 20909 RVA: 0x001E9A80 File Offset: 0x001E7C80
			[NullableContext(2)]
			public Client GetTraitorClient()
			{
				int sessionId = (int)this.VotedAsTraitorClientSessionId;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null)
				{
					return null;
				}
				IReadOnlyList<Client> connectedClients = networkMember.ConnectedClients;
				if (connectedClients == null)
				{
					return null;
				}
				return connectedClients.FirstOrDefault((Client c) => (int)c.SessionId == sessionId);
			}

			// Token: 0x04002CA4 RID: 11428
			[NetworkSerialize(12)]
			public byte VotedAsTraitorClientSessionId;

			// Token: 0x04002CA5 RID: 11429
			[NetworkSerialize(15)]
			public bool VotedCorrectTraitor;

			// Token: 0x04002CA6 RID: 11430
			[NetworkSerialize(18)]
			public bool ObjectiveSuccessful;

			// Token: 0x04002CA7 RID: 11431
			[NetworkSerialize(21)]
			public int MoneyPenalty;

			// Token: 0x04002CA8 RID: 11432
			[NetworkSerialize(24)]
			public Identifier TraitorEventIdentifier;
		}
	}
}
