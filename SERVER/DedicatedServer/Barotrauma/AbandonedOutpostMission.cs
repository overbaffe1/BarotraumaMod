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
	// Token: 0x02000020 RID: 32
	internal class AbandonedOutpostMission : Mission
	{
		// Token: 0x06000441 RID: 1089 RVA: 0x000249A8 File Offset: 0x00022BA8
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteUInt16((ushort)this.spawnedItems.Count);
			foreach (Item item in this.spawnedItems)
			{
				item.WriteSpawnData(msg, item.ID, 0, 0, -1);
			}
			msg.WriteByte((byte)this.characters.Count);
			foreach (Character character in this.characters)
			{
				character.WriteSpawnData(msg, character.ID, false);
				msg.WriteBoolean(this.requireKill.Contains(character));
				msg.WriteBoolean(this.requireRescue.Contains(character));
				msg.WriteUInt16((ushort)this.characterItems[character].Count);
				foreach (Item item2 in this.characterItems[character])
				{
					Item item3 = item2;
					ushort id = item2.ID;
					Inventory parentInventory = item2.ParentInventory;
					ushort? num;
					if (parentInventory == null)
					{
						num = null;
					}
					else
					{
						Entity owner = parentInventory.Owner;
						num = ((owner != null) ? new ushort?(owner.ID) : null);
					}
					ushort? num2 = num;
					ushort valueOrDefault = num2.GetValueOrDefault();
					byte originalItemContainerIndex = 0;
					Inventory parentInventory2 = item2.ParentInventory;
					item3.WriteSpawnData(msg, id, valueOrDefault, originalItemContainerIndex, (parentInventory2 != null) ? parentInventory2.FindIndex(item2) : -1);
				}
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000442 RID: 1090 RVA: 0x00024B88 File Offset: 0x00022D88
		public override bool AllowRespawning
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000443 RID: 1091 RVA: 0x00024B8B File Offset: 0x00022D8B
		public override bool AllowUndocking
		{
			get
			{
				return GameMain.GameSession.GameMode is CampaignMode || this.state > 0;
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000444 RID: 1092 RVA: 0x00024BA9 File Offset: 0x00022DA9
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

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000445 RID: 1093 RVA: 0x00024BD0 File Offset: 0x00022DD0
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

		// Token: 0x06000446 RID: 1094 RVA: 0x00024C74 File Offset: 0x00022E74
		public AbandonedOutpostMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.allowOrderingRescuees = prefab.ConfigElement.GetAttributeBool("allowOrderingRescuees", true);
			string msgTag = prefab.ConfigElement.GetAttributeString("hostageskilledmessage", "");
			this.hostagesKilledMessage = TextManager.Get(msgTag).Fallback(msgTag, true);
			this.itemConfig = prefab.ConfigElement.GetChildElement("Items");
			this.itemTag = prefab.ConfigElement.GetAttributeIdentifier("targetitem", Identifier.Empty);
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x00024D38 File Offset: 0x00022F38
		protected override void StartMissionSpecific(Level level)
		{
			this.failed = false;
			this.endTimer = 0f;
			this.requireKill.Clear();
			this.requireRescue.Clear();
			this.items.Clear();
			this.spawnedItems.Clear();
			Submarine submarine = Submarine.Loaded.Find((Submarine s) => s.Info.Type == SubmarineType.Outpost) ?? Submarine.MainSub;
			this.InitItems(submarine);
			if (!Mission.IsClient)
			{
				base.InitCharacters(submarine);
			}
			this.wasDocked = Submarine.MainSub.DockedTo.Contains(Level.Loaded.StartOutpost);
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00024DEC File Offset: 0x00022FEC
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
						this.spawnedItems.Add(item);
					}
				}
			}
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00025094 File Offset: 0x00023294
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
			int num = this.previousKillTargetsRemaining;
			this.previousKillTargetsRemaining = killTargetsRemaining;
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00025104 File Offset: 0x00023304
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
				if (this.endTimer > 5f && !(GameMain.GameSession.GameMode is CampaignMode) && GameMain.Server != null)
				{
					GameMain.Server.EndGame(CampaignMode.TransitionType.None, false, null);
				}
			}
			int state = this.state;
			if (state != 0)
			{
				if (state != 1)
				{
					return;
				}
				if (!(GameMain.GameSession.GameMode is CampaignMode) && GameMain.Server != null && (!Submarine.MainSub.AtStartExit || (this.wasDocked && !Submarine.MainSub.DockedTo.Contains(Level.Loaded.StartOutpost))))
				{
					GameMain.Server.EndGame(CampaignMode.TransitionType.None, false, null);
					this.State = 2;
				}
			}
			else if (this.items.All((Item it) => it.Removed || it.Condition <= 0f))
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
						return;
					}
				}
			}
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x00025293 File Offset: 0x00023493
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.State > 0 && this.State != 5;
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x000252AC File Offset: 0x000234AC
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

		// Token: 0x0600044D RID: 1101 RVA: 0x000252E4 File Offset: 0x000234E4
		protected override void InitCharacter(Character character, XElement element)
		{
			base.InitCharacter(character, element);
			if (element.GetAttributeBool("requirekill", false))
			{
				this.requireKill.Add(character);
			}
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x0002530C File Offset: 0x0002350C
		protected override Character LoadHuman(HumanPrefab humanPrefab, XElement element, Submarine submarine)
		{
			Character spawnedCharacter = base.LoadHuman(humanPrefab, element, submarine);
			bool requiresRescue = element.GetAttributeBool("requirerescue", false);
			if (requiresRescue)
			{
				this.requireRescue.Add(spawnedCharacter);
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

		// Token: 0x0400020A RID: 522
		private readonly List<Item> spawnedItems = new List<Item>();

		// Token: 0x0400020B RID: 523
		protected readonly HashSet<Character> requireKill = new HashSet<Character>();

		// Token: 0x0400020C RID: 524
		protected readonly HashSet<Character> requireRescue = new HashSet<Character>();

		// Token: 0x0400020D RID: 525
		private readonly Identifier itemTag;

		// Token: 0x0400020E RID: 526
		private readonly XElement itemConfig;

		// Token: 0x0400020F RID: 527
		private readonly List<Item> items = new List<Item>();

		// Token: 0x04000210 RID: 528
		protected const int HostagesKilledState = 5;

		// Token: 0x04000211 RID: 529
		private readonly LocalizedString hostagesKilledMessage;

		// Token: 0x04000212 RID: 530
		private const float EndDelay = 5f;

		// Token: 0x04000213 RID: 531
		private float endTimer;

		// Token: 0x04000214 RID: 532
		private readonly bool allowOrderingRescuees;

		// Token: 0x04000215 RID: 533
		protected bool wasDocked;

		// Token: 0x04000216 RID: 534
		private int previousKillTargetsRemaining = -1;
	}
}
