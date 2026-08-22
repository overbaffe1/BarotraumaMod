using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200004E RID: 78
	internal class AbandonedOutpostMission : Mission
	{
		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x0006837A File Offset: 0x0006657A
		// (set) Token: 0x06000B2B RID: 2859 RVA: 0x00068382 File Offset: 0x00066582
		public override int State
		{
			get
			{
				return base.State;
			}
			set
			{
				if (this.state != value)
				{
					base.State = value;
					if (this.state == 5 && !this.hostagesKilledMessage.IsNullOrEmpty())
					{
						base.CreateMessageBox(string.Empty, this.hostagesKilledMessage);
					}
				}
			}
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000B2C RID: 2860 RVA: 0x000683C0 File Offset: 0x000665C0
		public override bool DisplayAsCompleted
		{
			get
			{
				return !this.DisplayAsFailed && this.State > 0 && this.requireRescue.None(null);
			}
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000B2D RID: 2861 RVA: 0x000683E1 File Offset: 0x000665E1
		public override bool DisplayAsFailed
		{
			get
			{
				return this.State == 5;
			}
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x000683EC File Offset: 0x000665EC
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			ushort targetItemCount = msg.ReadUInt16();
			for (int i = 0; i < (int)targetItemCount; i++)
			{
				Item item = Item.ReadSpawnData(msg, true);
				this.items.Add(item);
			}
			byte characterCount = msg.ReadByte();
			for (int j = 0; j < (int)characterCount; j++)
			{
				Character character = Character.ReadSpawnData(msg);
				this.characters.Add(character);
				if (msg.ReadBoolean())
				{
					this.requireKill.Add(character);
				}
				if (msg.ReadBoolean())
				{
					this.requireRescue.Add(character);
					if (this.allowOrderingRescuees)
					{
						CrewManager crewManager = GameMain.GameSession.CrewManager;
						if (crewManager != null)
						{
							crewManager.AddCharacterToCrewList(character);
						}
					}
				}
				ushort itemCount = msg.ReadUInt16();
				for (int k = 0; k < (int)itemCount; k++)
				{
					Item.ReadSpawnData(msg, true);
				}
				EnemyAIController enemyAi = character.AIController as EnemyAIController;
				if (enemyAi != null)
				{
					Submarine ownSub = character.Submarine;
					if (ownSub != null)
					{
						enemyAi.SetUnattackableSubmarines(ownSub, true, true, true);
					}
				}
			}
			if (this.characters.Contains(null))
			{
				throw new Exception("Error in AbandonedOutpostMission.ClientReadInitial: character list contains null (mission: " + this.Prefab.Identifier.ToString() + ")");
			}
			if (this.characters.Count != (int)characterCount)
			{
				string[] array = new string[7];
				array[0] = "Error in AbandonedOutpostMission.ClientReadInitial: character count does not match the server count (";
				int num = 1;
				List<Character> characters = this.characters;
				array[num] = ((characters != null) ? characters.ToString() : null);
				array[2] = " != ";
				array[3] = this.characters.Count.ToString();
				array[4] = "mission: ";
				array[5] = this.Prefab.Identifier.ToString();
				array[6] = ")";
				throw new Exception(string.Concat(array));
			}
		}

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06000B2F RID: 2863 RVA: 0x000685AE File Offset: 0x000667AE
		public override bool AllowRespawning
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06000B30 RID: 2864 RVA: 0x000685B1 File Offset: 0x000667B1
		public override bool AllowUndocking
		{
			get
			{
				return GameMain.GameSession.GameMode is CampaignMode || this.state > 0;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06000B31 RID: 2865 RVA: 0x000685CF File Offset: 0x000667CF
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				if (this.State == 0)
				{
					return from t in this.Targets
					select new ValueTuple<LocalizedString, Vector2>(this.Prefab.SonarLabel, t.WorldPosition);
				}
				return Enumerable.Empty<ValueTuple<LocalizedString, Vector2>>();
			}
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06000B32 RID: 2866 RVA: 0x000685F8 File Offset: 0x000667F8
		private IEnumerable<Entity> Targets
		{
			get
			{
				if (this.State > 0)
				{
					return Enumerable.Empty<Entity>();
				}
				if (this.items.Any<Item>())
				{
					return (from it in this.items
					where !it.Removed && it.Condition > 0f
					select it).Cast<Entity>().Concat(from c in this.requireKill
					where !c.Removed && !c.IsDead
					select c).Concat(this.requireRescue);
				}
				return this.requireKill.Concat(this.requireRescue);
			}
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x0006869C File Offset: 0x0006689C
		public AbandonedOutpostMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.allowOrderingRescuees = prefab.ConfigElement.GetAttributeBool("allowOrderingRescuees", true);
			string msgTag = prefab.ConfigElement.GetAttributeString("hostageskilledmessage", "");
			this.hostagesKilledMessage = TextManager.Get(msgTag).Fallback(msgTag, true);
			this.itemConfig = prefab.ConfigElement.GetChildElement("Items");
			this.itemTag = prefab.ConfigElement.GetAttributeIdentifier("targetitem", Identifier.Empty);
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x00068758 File Offset: 0x00066958
		protected override void StartMissionSpecific(Level level)
		{
			this.failed = false;
			this.endTimer = 0f;
			this.requireKill.Clear();
			this.requireRescue.Clear();
			this.items.Clear();
			Submarine submarine = Submarine.Loaded.Find((Submarine s) => s.Info.Type == SubmarineType.Outpost) ?? Submarine.MainSub;
			this.InitItems(submarine);
			if (!Mission.IsClient)
			{
				base.InitCharacters(submarine);
			}
			this.wasDocked = Submarine.MainSub.DockedTo.Contains(Level.Loaded.StartOutpost);
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x00068800 File Offset: 0x00066A00
		private void InitItems(Submarine submarine)
		{
			if (!this.itemTag.IsEmpty)
			{
				List<Item> itemsToDestroy = Item.ItemList.FindAll(delegate(Item it)
				{
					Submarine submarine2 = it.Submarine;
					return (submarine2 == null || submarine2.Info.Type > SubmarineType.Player) && it.HasTag(this.itemTag);
				});
				if (!itemsToDestroy.Any<Item>())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". Could not find an item with the tag \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.itemTag);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
				else
				{
					this.items.AddRange(itemsToDestroy);
				}
			}
			if (this.itemConfig != null && !Mission.IsClient)
			{
				foreach (XElement element in this.itemConfig.Elements())
				{
					Identifier itemIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
					ItemPrefab itemPrefab = MapEntityPrefab.FindByIdentifier(itemIdentifier) as ItemPrefab;
					if (itemPrefab == null)
					{
						DebugConsole.ThrowError("Couldn't spawn item for outpost destroy mission: item prefab \"" + itemIdentifier.ToString() + "\" not found", null, this.Prefab.ContentPackage, false, false);
					}
					else
					{
						Identifier[] moduleFlags = element.GetAttributeIdentifierArray("moduleflags", null, true);
						Identifier[] spawnPointTags = element.GetAttributeIdentifierArray("spawnpointtags", null, true);
						ISpatialEntity spawnPoint = SpawnAction.GetSpawnPos(SpawnAction.SpawnLocationType.Outpost, new SpawnType?(SpawnType.Human | SpawnType.Enemy), moduleFlags, spawnPointTags, element.GetAttributeBool("asfaraspossible", false), false, true);
						if (spawnPoint == null)
						{
							spawnPoint = submarine.GetHulls(false).GetRandomUnsynced<Hull>();
						}
						Vector2 spawnPos = spawnPoint.WorldPosition;
						WayPoint wp = spawnPoint as WayPoint;
						if (wp != null && wp.CurrentHull != null && wp.CurrentHull.Rect.Width > 100)
						{
							spawnPos = new Vector2(MathHelper.Clamp(wp.WorldPosition.X + (float)Rand.Range(-200, 201, Rand.RandSync.Unsynced), (float)(wp.CurrentHull.WorldRect.X + 50), (float)(wp.CurrentHull.WorldRect.Right - 50)), (float)(wp.CurrentHull.WorldRect.Y - wp.CurrentHull.Rect.Height) + 16f);
						}
						Item item = new Item(itemPrefab, spawnPos, null, 0, true);
						this.items.Add(item);
					}
				}
			}
		}

		// Token: 0x06000B36 RID: 2870 RVA: 0x00068A98 File Offset: 0x00066C98
		private void TrackKillTargetCount()
		{
			if (this.requireKill.Count == 0)
			{
				return;
			}
			if (this.previousKillTargetsRemaining == -1)
			{
				this.previousKillTargetsRemaining = this.requireKill.Count<Character>();
			}
			int killTargetsRemaining = this.requireKill.Count((Character c) => !c.Removed && !c.IsDead && (!c.LockHands || c.Submarine != Submarine.MainSub));
			if (killTargetsRemaining < this.previousKillTargetsRemaining)
			{
				SteamTimelineManager.OnOutpostTargetEliminated(this);
			}
			this.previousKillTargetsRemaining = killTargetsRemaining;
		}

		// Token: 0x06000B37 RID: 2871 RVA: 0x00068B10 File Offset: 0x00066D10
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			this.TrackKillTargetCount();
			if (this.State != 5)
			{
				if (this.requireRescue.Any((Character r) => r.Removed || r.IsDead))
				{
					this.State = 5;
					return;
				}
			}
			else
			{
				this.endTimer += deltaTime;
				float num = this.endTimer;
			}
			if (this.state == 0)
			{
				if (this.items.All((Item it) => it.Removed || it.Condition <= 0f))
				{
					if (this.requireKill.All((Character c) => c.Removed || c.IsDead || (c.LockHands && c.Submarine == Submarine.MainSub)))
					{
						if (this.requireRescue.All(delegate(Character c)
						{
							Submarine submarine = c.Submarine;
							return submarine != null && submarine.Info.Type == SubmarineType.Player;
						}))
						{
							this.State = 1;
						}
					}
				}
			}
		}

		// Token: 0x06000B38 RID: 2872 RVA: 0x00068C13 File Offset: 0x00066E13
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.State > 0 && this.State != 5;
		}

		// Token: 0x06000B39 RID: 2873 RVA: 0x00068C2C File Offset: 0x00066E2C
		protected override void EndMissionSpecific(bool completed)
		{
			bool failed;
			if (!completed)
			{
				failed = this.requireRescue.Any((Character r) => r.Removed || r.IsDead);
			}
			else
			{
				failed = false;
			}
			this.failed = failed;
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x00068C64 File Offset: 0x00066E64
		protected override void InitCharacter(Character character, XElement element)
		{
			base.InitCharacter(character, element);
			if (element.GetAttributeBool("requirekill", false))
			{
				this.requireKill.Add(character);
			}
		}

		// Token: 0x06000B3B RID: 2875 RVA: 0x00068C8C File Offset: 0x00066E8C
		protected override Character LoadHuman(HumanPrefab humanPrefab, XElement element, Submarine submarine)
		{
			Character spawnedCharacter = base.LoadHuman(humanPrefab, element, submarine);
			bool requiresRescue = element.GetAttributeBool("requirerescue", false);
			if (requiresRescue)
			{
				this.requireRescue.Add(spawnedCharacter);
				if (this.allowOrderingRescuees)
				{
					CrewManager crewManager = GameMain.GameSession.CrewManager;
					if (crewManager != null)
					{
						crewManager.AddCharacterToCrewList(spawnedCharacter);
					}
				}
			}
			else if (base.TimesAttempted > 0 && spawnedCharacter.AIController is HumanAIController)
			{
				Order order = OrderPrefab.Prefabs["fightintruders"].CreateInstance(OrderPrefab.OrderTargetType.Entity, spawnedCharacter, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
				spawnedCharacter.SetOrder(order, true, false, false);
			}
			CharacterTeamType teamId = element.GetAttributeEnum("teamid", requiresRescue ? CharacterTeamType.FriendlyNPC : CharacterTeamType.None);
			if (teamId != spawnedCharacter.TeamID)
			{
				spawnedCharacter.SetOriginalTeamAndChangeTeam(teamId, false);
			}
			return spawnedCharacter;
		}

		// Token: 0x040005C4 RID: 1476
		protected readonly HashSet<Character> requireKill = new HashSet<Character>();

		// Token: 0x040005C5 RID: 1477
		protected readonly HashSet<Character> requireRescue = new HashSet<Character>();

		// Token: 0x040005C6 RID: 1478
		private readonly Identifier itemTag;

		// Token: 0x040005C7 RID: 1479
		private readonly XElement itemConfig;

		// Token: 0x040005C8 RID: 1480
		private readonly List<Item> items = new List<Item>();

		// Token: 0x040005C9 RID: 1481
		protected const int HostagesKilledState = 5;

		// Token: 0x040005CA RID: 1482
		private readonly LocalizedString hostagesKilledMessage;

		// Token: 0x040005CB RID: 1483
		private const float EndDelay = 5f;

		// Token: 0x040005CC RID: 1484
		private float endTimer;

		// Token: 0x040005CD RID: 1485
		private readonly bool allowOrderingRescuees;

		// Token: 0x040005CE RID: 1486
		protected bool wasDocked;

		// Token: 0x040005CF RID: 1487
		private int previousKillTargetsRemaining = -1;
	}
}
