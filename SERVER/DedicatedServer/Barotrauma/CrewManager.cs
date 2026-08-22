using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200002F RID: 47
	internal class CrewManager
	{
		// Token: 0x0600059C RID: 1436 RVA: 0x000341AC File Offset: 0x000323AC
		public XElement SaveMultiplayer(XElement parentElement)
		{
			XElement element = new XElement("bots", new XAttribute("hasbots", this.HasBots));
			foreach (CharacterInfo info in this.GetCharacterInfos(true))
			{
				if (Level.Loaded != null && !info.IsNewHire && !info.IsOnReserveBench)
				{
					if (info.Character == null && !info.PendingSpawnToActiveService)
					{
						continue;
					}
					Character character = info.Character;
					if (character != null && character.IsDead)
					{
						continue;
					}
				}
				XElement characterElement = info.Save(element);
				if (info.InventoryData != null)
				{
					characterElement.Add(info.InventoryData);
				}
				if (info.HealthData != null)
				{
					characterElement.Add(info.HealthData);
				}
				if (info.OrderData != null)
				{
					characterElement.Add(info.OrderData);
				}
			}
			if (parentElement != null)
			{
				parentElement.Add(element);
			}
			return element;
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x000342B0 File Offset: 0x000324B0
		public void ServerWriteActiveOrders(IWriteMessage msg)
		{
			ushort count = (ushort)this.ActiveOrders.Count((CrewManager.ActiveOrder o) => o.Order != null && o.FadeOutTime == null);
			msg.WriteUInt16(count);
			if (count > 0)
			{
				foreach (CrewManager.ActiveOrder activeOrder in this.ActiveOrders)
				{
					Order order = (activeOrder != null) ? activeOrder.Order : null;
					if (order != null && activeOrder.FadeOutTime == null)
					{
						OrderChatMessage.WriteOrder(msg, order, null, true);
						bool hasOrderGiver = order.OrderGiver != null;
						msg.WriteBoolean(hasOrderGiver);
						if (hasOrderGiver)
						{
							msg.WriteUInt16(order.OrderGiver.ID);
						}
					}
				}
			}
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x00034384 File Offset: 0x00032584
		public void ReadToggleReserveBenchMessage(IReadMessage inc, Client sender)
		{
			ushort botId = inc.ReadUInt16();
			bool pendingHire = inc.ReadBoolean();
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
			if (mpCampaign == null)
			{
				return;
			}
			if (!CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.ManageHires))
			{
				DebugConsole.NewMessage("Client " + sender.Name + " is not allowed to modify the reserve bench status of bots (requires ManageHires)", null, false);
				return;
			}
			if (pendingHire)
			{
				Location currentLocation = mpCampaign.Map.CurrentLocation;
				CharacterInfo pendingCharacterInfo = (currentLocation != null) ? currentLocation.HireManager.PendingHires.FirstOrDefault((CharacterInfo ci) => ci.ID == botId) : null;
				if (pendingCharacterInfo != null)
				{
					this.ToggleReserveBenchStatus(pendingCharacterInfo, sender, true, false, true);
					return;
				}
			}
			CrewManager crewManager = GameMain.GameSession.CrewManager;
			CharacterInfo characterInfo2;
			if (crewManager == null)
			{
				characterInfo2 = null;
			}
			else
			{
				IEnumerable<CharacterInfo> enumerable = crewManager.GetCharacterInfos(true);
				characterInfo2 = ((enumerable != null) ? enumerable.FirstOrDefault((CharacterInfo i) => i.ID == botId) : null);
			}
			CharacterInfo characterInfo = characterInfo2;
			if (characterInfo != null)
			{
				this.ToggleReserveBenchStatus(characterInfo, sender, false, false, true);
			}
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0003447C File Offset: 0x0003267C
		public void ToggleReserveBenchStatus(CharacterInfo characterInfo, Client client, bool pendingHire = false, bool confirmPendingHire = false, bool sendUpdate = true)
		{
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
			if (mpCampaign == null)
			{
				return;
			}
			if (confirmPendingHire && !pendingHire)
			{
				DebugConsole.ThrowError("ToggleReserveBenchStatus: cannot confirm a hire that is not pending (bot " + characterInfo.DisplayName + ")", null, null, false, false);
			}
			BotStatus currentStatus = characterInfo.BotStatus;
			if (pendingHire && !confirmPendingHire)
			{
				Location currentLocation = mpCampaign.Map.CurrentLocation;
				if (currentLocation == null || !currentLocation.HireManager.PendingHires.Contains(characterInfo))
				{
					DebugConsole.ThrowError("ToggleReserveBenchStatus: bot " + characterInfo.DisplayName + " is supposed to be in the pending hires list, but can't be found there", null, null, false, false);
				}
				if (currentStatus == BotStatus.PendingHireToActiveService)
				{
					characterInfo.BotStatus = BotStatus.PendingHireToReserveBench;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" moved the pending hire \"");
					defaultInterpolatedStringHandler.AppendFormatted(characterInfo.DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral("\" to the reserve bench.");
					GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
				}
				else if (currentStatus == BotStatus.PendingHireToReserveBench)
				{
					if (this.GetCharacterInfos(false).Count<CharacterInfo>() >= 16)
					{
						DebugConsole.NewMessage("ToggleReserveBenchStatus: Tried moving pending hire " + characterInfo.DisplayName + " to active service, but MaxCrewSize has already been reached", null, false);
						return;
					}
					characterInfo.BotStatus = BotStatus.PendingHireToActiveService;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(77, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler2.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" moved the pending hire \"");
					defaultInterpolatedStringHandler2.AppendFormatted(characterInfo.DisplayName);
					defaultInterpolatedStringHandler2.AppendLiteral("\" from the reserve bench to active service.");
					GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
				}
			}
			else if (this.GetCharacterInfos(true).Contains(characterInfo) || confirmPendingHire)
			{
				if (currentStatus == BotStatus.ActiveService || (confirmPendingHire && currentStatus == BotStatus.PendingHireToReserveBench))
				{
					if (this.reserveBench.Contains(characterInfo))
					{
						DebugConsole.ThrowError("ToggleReserveBenchStatus: Tried to add the same CharacterInfo (" + characterInfo.DisplayName + ") to reserve bench twice", null, null, false, false);
					}
					this.RemoveCharacterInfo(characterInfo);
					characterInfo.BotStatus = BotStatus.ReserveBench;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(68, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler3.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler3.AppendLiteral("\" moved the bot \"");
					defaultInterpolatedStringHandler3.AppendFormatted(characterInfo.DisplayName);
					defaultInterpolatedStringHandler3.AppendLiteral("\" from active service to the reserve bench.");
					GameServer.Log(defaultInterpolatedStringHandler3.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
					this.reserveBench.Add(characterInfo);
				}
				else if (currentStatus == BotStatus.ReserveBench || (confirmPendingHire && currentStatus == BotStatus.PendingHireToActiveService))
				{
					if (this.GetCharacterInfos(false).Count<CharacterInfo>() >= 16)
					{
						DebugConsole.NewMessage("ToggleReserveBenchStatus: Tried moving " + characterInfo.DisplayName + " to active service, but MaxCrewSize has already been reached", null, false);
						return;
					}
					this.RemoveCharacterInfo(characterInfo);
					characterInfo.BotStatus = BotStatus.ActiveService;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(68, 2);
					defaultInterpolatedStringHandler4.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler4.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler4.AppendLiteral("\" moved the bot \"");
					defaultInterpolatedStringHandler4.AppendFormatted(characterInfo.DisplayName);
					defaultInterpolatedStringHandler4.AppendLiteral("\" from the reserve bench to active service.");
					GameServer.Log(defaultInterpolatedStringHandler4.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
					this.AddCharacterInfo(characterInfo);
				}
			}
			else
			{
				DebugConsole.ThrowError("ToggleReserveBenchStatus: bot " + characterInfo.DisplayName + " not found from CrewManager", null, null, false, false);
			}
			if (sendUpdate)
			{
				mpCampaign.SendCrewState(default(ValueTuple<ushort, string>), null, true);
			}
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x000347CD File Offset: 0x000329CD
		public IEnumerable<CharacterInfo> GetCharacterInfos(bool includeReserveBench = false)
		{
			if (includeReserveBench)
			{
				return this.characterInfos.Concat(this.reserveBench);
			}
			return this.characterInfos;
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x000347EA File Offset: 0x000329EA
		public IEnumerable<CharacterInfo> GetReserveBenchInfos()
		{
			return this.reserveBench;
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060005A2 RID: 1442 RVA: 0x000347F2 File Offset: 0x000329F2
		// (set) Token: 0x060005A3 RID: 1443 RVA: 0x000347FA File Offset: 0x000329FA
		public bool HasBots { get; set; }

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x00034803 File Offset: 0x00032A03
		public List<CrewManager.ActiveOrder> ActiveOrders { get; } = new List<CrewManager.ActiveOrder>();

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x060005A5 RID: 1445 RVA: 0x0003480B File Offset: 0x00032A0B
		// (set) Token: 0x060005A6 RID: 1446 RVA: 0x00034813 File Offset: 0x00032A13
		public bool IsSinglePlayer { get; private set; }

		// Token: 0x060005A7 RID: 1447 RVA: 0x0003481C File Offset: 0x00032A1C
		public CrewManager(bool isSinglePlayer)
		{
			this.IsSinglePlayer = isSinglePlayer;
			this.conversationTimer = 5f;
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x00034878 File Offset: 0x00032A78
		public bool AddOrder(Order order, float? fadeOutTime)
		{
			if (order.TargetEntity == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to add a \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(order.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" order with no target entity to CrewManager!\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				string message = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.AddWarning(message, null);
				GameAnalyticsManager.AddErrorEventOnce("CrewManager.AddOrder:OrderTargetEntityNull", GameAnalyticsManager.ErrorSeverity.Error, message);
				return false;
			}
			Identifier identifier = order.Identifier;
			bool isUnignoreOrder = identifier == Tags.UnignoreThis;
			identifier = order.Identifier;
			bool isIgnoreOrder = identifier == Tags.IgnoreThis;
			OrderPrefab orderPrefab = (!isUnignoreOrder) ? order.Prefab : OrderPrefab.Prefabs[Tags.IgnoreThis];
			CrewManager.ActiveOrder existingOrder = this.ActiveOrders.Find(delegate(CrewManager.ActiveOrder o)
			{
				if (o.Order.Prefab != orderPrefab || !CrewManager.<AddOrder>g__MatchesTarget|32_1(o.Order.TargetEntity, order.TargetEntity))
				{
					return false;
				}
				if (o.Order.TargetType == Order.OrderTargetType.WallSection)
				{
					int? wallSectionIndex = o.Order.WallSectionIndex;
					int? wallSectionIndex2 = order.WallSectionIndex;
					return wallSectionIndex.GetValueOrDefault() == wallSectionIndex2.GetValueOrDefault() & wallSectionIndex != null == (wallSectionIndex2 != null);
				}
				return true;
			});
			if (existingOrder != null)
			{
				if (!isUnignoreOrder)
				{
					existingOrder.FadeOutTime = fadeOutTime;
					return false;
				}
				this.ActiveOrders.Remove(existingOrder);
				if (isIgnoreOrder)
				{
					Item targetItem = order.TargetEntity as Item;
					if (targetItem != null)
					{
						using (IEnumerator<Item> enumerator = targetItem.GetStackedItems().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Item stackedItem = enumerator.Current;
								this.ActiveOrders.RemoveAll((CrewManager.ActiveOrder o) => o.Order.Prefab == orderPrefab && o.Order.TargetEntity == stackedItem);
								stackedItem.OrderedToBeIgnored = false;
							}
						}
					}
				}
				return true;
			}
			else
			{
				if (!isUnignoreOrder)
				{
					if (order.IsDeconstructOrder)
					{
						Item item = order.TargetEntity as Item;
						if (item != null)
						{
							identifier = order.Identifier;
							if (identifier == Tags.DeconstructThis)
							{
								using (IEnumerator<Item> enumerator2 = item.GetStackedItems().GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										Item stackedItem4 = enumerator2.Current;
										Item.DeconstructItems.Add(stackedItem4);
									}
									goto IL_25B;
								}
							}
							foreach (Item stackedItem2 in item.GetStackedItems())
							{
								Item.DeconstructItems.Remove(stackedItem2);
							}
						}
					}
					IL_25B:
					if (isIgnoreOrder)
					{
						Item targetItem2 = order.TargetEntity as Item;
						if (targetItem2 != null)
						{
							using (IEnumerator<Item> enumerator4 = targetItem2.GetStackedItems().GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									Item stackedItem3 = enumerator4.Current;
									this.ActiveOrders.Add(new CrewManager.ActiveOrder(order.WithTargetEntity(stackedItem3), fadeOutTime));
									stackedItem3.OrderedToBeIgnored = true;
								}
								return true;
							}
						}
					}
					this.ActiveOrders.Add(new CrewManager.ActiveOrder(order, fadeOutTime));
					return true;
				}
				return false;
			}
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x00034BA0 File Offset: 0x00032DA0
		public void AddCharacterElements(XElement element)
		{
			foreach (XElement characterElement in element.Elements())
			{
				if (characterElement.Name.ToString().Equals("character", StringComparison.OrdinalIgnoreCase))
				{
					CharacterInfo characterInfo = new CharacterInfo(new ContentXElement(null, characterElement), default(Identifier));
					if (characterElement.GetAttributeBool("IsOnReserveBench", false))
					{
						this.reserveBench.Add(characterInfo);
					}
					else
					{
						this.characterInfos.Add(characterInfo);
					}
					foreach (XElement subElement in characterElement.Elements())
					{
						string a = subElement.Name.ToString().ToLowerInvariant();
						if (!(a == "inventory"))
						{
							if (!(a == "health"))
							{
								if (a == "orders")
								{
									characterInfo.OrderData = subElement;
								}
							}
							else
							{
								characterInfo.HealthData = subElement;
							}
						}
						else
						{
							characterInfo.InventoryData = subElement;
						}
					}
				}
			}
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x00034CDC File Offset: 0x00032EDC
		public void RemoveCharacterInfo(CharacterInfo characterInfo)
		{
			if (characterInfo != null && characterInfo.IsOnReserveBench)
			{
				this.reserveBench.Remove(characterInfo);
			}
			this.characterInfos.Remove(characterInfo);
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x00034D04 File Offset: 0x00032F04
		public void AddCharacter(Character character)
		{
			if (character.Removed)
			{
				DebugConsole.ThrowError("Tried to add a removed character to CrewManager!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (character.IsDead)
			{
				DebugConsole.ThrowError("Tried to add a dead character to CrewManager!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (character.Info == null)
			{
				if (character.Prefab.ContentPackage == GameMain.VanillaContent)
				{
					DebugConsole.ThrowError("Added a character with no CharacterInfo to the crew." + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				}
				else
				{
					DebugConsole.ThrowError("Added add a character with no CharacterInfo to the crew. This may lead to issues: consider adding HasCharacterInfo=\"True\" to the character config.", null, null, false, false);
				}
			}
			if (!this.characters.Contains(character))
			{
				this.characters.Add(character);
			}
			if (!this.characterInfos.Contains(character.Info))
			{
				this.characterInfos.Add(character.Info);
			}
			HumanAIController humanAI = character.AIController as HumanAIController;
			if (humanAI != null)
			{
				AIObjectiveIdle idleObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveIdle>();
				if (idleObjective != null)
				{
					idleObjective.Behavior = character.Info.Job.Prefab.IdleBehavior;
				}
			}
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x00034E1B File Offset: 0x0003301B
		public bool IsFired(Character character)
		{
			return !this.GetCharacterInfos(false).Contains(character.Info);
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x00034E34 File Offset: 0x00033034
		public void RemoveCharacter(Character character, bool removeInfo = false, bool resetCrewListIndex = true)
		{
			if (character == null)
			{
				DebugConsole.ThrowError("Tried to remove a null character from CrewManager.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.characters.Remove(character);
			if (removeInfo)
			{
				this.characterInfos.Remove(character.Info);
			}
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x00034E84 File Offset: 0x00033084
		public void AddCharacterInfo(CharacterInfo characterInfo)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign && characterInfo.BotStatus != BotStatus.ActiveService)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("CrewManager.AddCharacterInfo called on a bot (");
				defaultInterpolatedStringHandler.AppendFormatted(characterInfo.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral(") with the wrong status (");
				defaultInterpolatedStringHandler.AppendFormatted<BotStatus>(characterInfo.BotStatus);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (this.characterInfos.Contains(characterInfo))
			{
				DebugConsole.ThrowError("Tried to add the same character info to CrewManager twice.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.characterInfos.Add(characterInfo);
		}

		// Token: 0x060005AF RID: 1455 RVA: 0x00034F42 File Offset: 0x00033142
		public void ClearCharacterInfos()
		{
			this.characterInfos.Clear();
			this.reserveBench.Clear();
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x00034F5C File Offset: 0x0003315C
		public void InitRound()
		{
			this.characters.Clear();
			List<WayPoint> spawnWaypoints = null;
			List<WayPoint> mainSubWaypoints = WayPoint.SelectCrewSpawnPoints(this.characterInfos, Submarine.MainSub).ToList<WayPoint>();
			if (Level.Loaded != null && Level.Loaded.ShouldSpawnCrewInsideOutpost())
			{
				spawnWaypoints = this.GetOutpostSpawnpoints();
				while (spawnWaypoints.Count > this.characterInfos.Count)
				{
					spawnWaypoints.RemoveAt(Rand.Int(spawnWaypoints.Count, Rand.RandSync.Unsynced));
				}
				while (spawnWaypoints.Any<WayPoint>() && spawnWaypoints.Count < this.characterInfos.Count)
				{
					spawnWaypoints.Add(spawnWaypoints[Rand.Int(spawnWaypoints.Count, Rand.RandSync.Unsynced)]);
				}
			}
			if (spawnWaypoints == null || !spawnWaypoints.Any<WayPoint>())
			{
				spawnWaypoints = mainSubWaypoints;
			}
			for (int i = 0; i < spawnWaypoints.Count; i++)
			{
				CharacterInfo info = this.characterInfos[i];
				info.TeamID = CharacterTeamType.Team1;
				Character character = Character.Create(info, spawnWaypoints[i].WorldPosition, info.Name, 0, false, true, null, true);
				this.InitializeCharacter(character, mainSubWaypoints[i], spawnWaypoints[i]);
				this.AddCharacter(character);
			}
			this.conversationTimer = (this.IsSinglePlayer ? Rand.Range(5f, 10f, Rand.RandSync.Unsynced) : Rand.Range(45f, 60f, Rand.RandSync.Unsynced));
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x000350A1 File Offset: 0x000332A1
		public List<WayPoint> GetOutpostSpawnpoints()
		{
			return WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Human && wp.Submarine == Level.Loaded.StartOutpost && wp.CurrentHull != null && wp.CurrentHull.OutpostModuleTags.Contains("airlock".ToIdentifier()));
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x000350CC File Offset: 0x000332CC
		public void InitializeCharacter(Character character, WayPoint mainSubWaypoint, WayPoint spawnWaypoint)
		{
			if (character.Info != null)
			{
				if (!character.Info.StartItemsGiven && character.Info.InventoryData != null)
				{
					DebugConsole.AddWarning("Error when initializing a round: character \"" + character.Name + "\" has not been given their initial items but has saved inventory data. Using the saved inventory data instead of giving the character new items.", null);
				}
				if (character.Info.InventoryData != null)
				{
					character.SpawnInventoryItems(character.Inventory, character.Info.InventoryData.FromPackage(null));
				}
				else if (!character.Info.StartItemsGiven)
				{
					GameSession gameSession = GameMain.GameSession;
					character.GiveJobItems(((gameSession != null) ? gameSession.GameMode : null) is PvPMode, mainSubWaypoint);
					foreach (Item item in character.Inventory.AllItems)
					{
						IdCard idCard = item.GetComponent<IdCard>();
						if (idCard != null)
						{
							idCard.SubmarineSpecificID = 0;
						}
					}
				}
				if (character.Info.HealthData != null)
				{
					CharacterInfo.ApplyHealthData(character, character.Info.HealthData, null);
				}
				character.LoadTalents();
				character.GiveIdCardTags(mainSubWaypoint, false);
				character.GiveIdCardTags(spawnWaypoint, false);
				character.Info.StartItemsGiven = true;
				if (character.Info.OrderData != null)
				{
					character.Info.ApplyOrderData();
				}
			}
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x0003521C File Offset: 0x0003341C
		public void RenameCharacter(CharacterInfo characterInfo, string newName)
		{
			characterInfo.Rename(newName);
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x00035225 File Offset: 0x00033425
		public void FireCharacter(CharacterInfo characterInfo)
		{
			this.RemoveCharacterInfo(characterInfo);
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00035230 File Offset: 0x00033430
		public void ClearCurrentOrders()
		{
			foreach (CharacterInfo characterInfo in this.characterInfos)
			{
				if (characterInfo != null)
				{
					characterInfo.ClearCurrentOrders();
				}
			}
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00035288 File Offset: 0x00033488
		public void Update(float deltaTime)
		{
			foreach (CrewManager.ActiveOrder order in this.ActiveOrders)
			{
				if (order.FadeOutTime != null)
				{
					order.FadeOutTime -= deltaTime;
				}
			}
			this.ActiveOrders.RemoveAll(delegate(CrewManager.ActiveOrder o)
			{
				if (o.FadeOutTime != null)
				{
					float? fadeOutTime = o.FadeOutTime;
					float num = 0f;
					if (fadeOutTime.GetValueOrDefault() <= num & fadeOutTime != null)
					{
						return true;
					}
				}
				return o.Order.TargetEntity != null && o.Order.TargetEntity.Removed;
			});
			this.UpdateConversations(deltaTime);
			ReadyCheck activeReadyCheck = this.ActiveReadyCheck;
			if (activeReadyCheck != null)
			{
				activeReadyCheck.Update(deltaTime);
			}
			if (this.ActiveReadyCheck != null && this.ActiveReadyCheck.IsFinished)
			{
				this.ActiveReadyCheck = null;
			}
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x00035374 File Offset: 0x00033574
		public void AddConversation([TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})] List<ValueTuple<Character, string>> conversationLines)
		{
			if (conversationLines == null || conversationLines.Count == 0)
			{
				return;
			}
			this.pendingConversationLines.AddRange(conversationLines);
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x00035390 File Offset: 0x00033590
		private void CreateRandomConversation()
		{
			List<Character> availableSpeakers = Character.CharacterList.FindAll((Character c) => c.AIController is HumanAIController && !c.IsDead && c.SpeechImpediment <= 100f);
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (client.Character != null)
				{
					availableSpeakers.Remove(client.Character);
				}
			}
			this.pendingConversationLines.AddRange(NPCConversation.CreateRandom(availableSpeakers));
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x0003542C File Offset: 0x0003362C
		private void UpdateConversations(float deltaTime)
		{
			GameSession gameSession = GameMain.GameSession;
			GameModePreset gameModePreset;
			if (gameSession == null)
			{
				gameModePreset = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				gameModePreset = ((gameMode != null) ? gameMode.Preset : null);
			}
			if (gameModePreset == GameModePreset.TestMode)
			{
				return;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.ServerSettings.DisableBotConversations)
			{
				return;
			}
			this.conversationTimer -= deltaTime;
			if (this.conversationTimer <= 0f)
			{
				this.CreateRandomConversation();
				this.conversationTimer = Rand.Range(100f, 180f, Rand.RandSync.Unsynced);
				if (GameMain.NetworkMember != null)
				{
					this.conversationTimer *= 5f;
				}
			}
			if (this.welcomeMessageNPC == null)
			{
				using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character npc = enumerator.Current;
						if ((npc.TeamID == CharacterTeamType.FriendlyNPC || npc.TeamID == CharacterTeamType.None) && npc.CurrentHull != null && !npc.IsIncapacitated)
						{
							HumanAIController humanAI = npc.AIController as HumanAIController;
							if (humanAI == null || (!humanAI.ObjectiveManager.IsCurrentObjective<AIObjectiveFindSafety>() && !humanAI.ObjectiveManager.IsCurrentObjective<AIObjectiveCombat>()))
							{
								foreach (Character player in Character.CharacterList)
								{
									if (player.TeamID != npc.TeamID && !player.IsIncapacitated && player.CurrentHull == npc.CurrentHull)
									{
										List<Character> availableSpeakers = new List<Character>
										{
											npc,
											player
										};
										List<Identifier> dialogFlags = new List<Identifier>
										{
											"OutpostNPC".ToIdentifier(),
											"EnterOutpost".ToIdentifier()
										};
										if (npc.HumanPrefab != null)
										{
											foreach (Identifier tag in npc.HumanPrefab.GetTags())
											{
												dialogFlags.Add(tag);
											}
										}
										GameSession gameSession2 = GameMain.GameSession;
										CampaignMode campaignMode = ((gameSession2 != null) ? gameSession2.GameMode : null) as CampaignMode;
										if (campaignMode != null)
										{
											Map map = campaignMode.Map;
											Identifier? identifier;
											Identifier? identifier2;
											if (map == null)
											{
												identifier = null;
												identifier2 = identifier;
											}
											else
											{
												Location currentLocation = map.CurrentLocation;
												if (currentLocation == null)
												{
													identifier = null;
													identifier2 = identifier;
												}
												else
												{
													LocationType type = currentLocation.Type;
													if (type == null)
													{
														identifier = null;
														identifier2 = identifier;
													}
													else
													{
														identifier2 = new Identifier?(type.Identifier);
													}
												}
											}
											identifier = identifier2;
											if (identifier == "abandoned")
											{
												dialogFlags.Remove("OutpostNPC".ToIdentifier());
											}
											else
											{
												Map map2 = campaignMode.Map;
												bool flag;
												if (map2 == null)
												{
													flag = (null != null);
												}
												else
												{
													Location currentLocation2 = map2.CurrentLocation;
													flag = (((currentLocation2 != null) ? currentLocation2.Reputation : null) != null);
												}
												if (flag)
												{
													float normalizedReputation = MathUtils.InverseLerp((float)campaignMode.Map.CurrentLocation.Reputation.MinReputation, (float)campaignMode.Map.CurrentLocation.Reputation.MaxReputation, campaignMode.Map.CurrentLocation.Reputation.Value);
													if (normalizedReputation < 0.2f)
													{
														dialogFlags.Add("LowReputation".ToIdentifier());
													}
													else if (normalizedReputation > 0.8f)
													{
														dialogFlags.Add("HighReputation".ToIdentifier());
													}
												}
											}
										}
										this.pendingConversationLines.AddRange(NPCConversation.CreateRandom(availableSpeakers, dialogFlags));
										this.welcomeMessageNPC = npc;
										break;
									}
								}
								if (this.welcomeMessageNPC != null)
								{
									break;
								}
							}
						}
					}
					goto IL_37E;
				}
			}
			if (this.welcomeMessageNPC.Removed)
			{
				this.welcomeMessageNPC = null;
			}
			IL_37E:
			if (this.pendingConversationLines.Count > 0)
			{
				this.conversationLineTimer -= deltaTime;
				if (this.conversationLineTimer <= 0f)
				{
					if (this.pendingConversationLines[0].Item1.SpeechImpediment >= 100f)
					{
						this.pendingConversationLines.Clear();
						return;
					}
					this.pendingConversationLines[0].Item1.Speak(this.pendingConversationLines[0].Item2, null, 0f, default(Identifier), 0f);
					if (this.pendingConversationLines.Count > 1)
					{
						this.conversationLineTimer = MathHelper.Clamp((float)this.pendingConversationLines[0].Item2.Length * 0.1f, 1f, 5f);
					}
					this.pendingConversationLines.RemoveAt(0);
				}
			}
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x000358EC File Offset: 0x00033AEC
		public static Character GetCharacterForQuickAssignment(Order order, Character controlledCharacter, IEnumerable<Character> characters, bool includeSelf = false)
		{
			bool isControlledCharacterNull = controlledCharacter == null;
			if (isControlledCharacterNull)
			{
				return null;
			}
			Character operatingCharacter;
			if (order.Category.GetValueOrDefault() == OrderCategory.Operate && HumanAIController.IsItemTargetedBySomeone(order.TargetItemComponent, (controlledCharacter != null) ? controlledCharacter.TeamID : CharacterTeamType.Team1, out operatingCharacter) && (isControlledCharacterNull || operatingCharacter.CanHearCharacter(controlledCharacter)))
			{
				return operatingCharacter;
			}
			return CrewManager.GetCharactersSortedForOrder(order, characters, controlledCharacter, includeSelf, null).FirstOrDefault((Character c) => isControlledCharacterNull || c.CanHearCharacter(controlledCharacter)) ?? controlledCharacter;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x00035998 File Offset: 0x00033B98
		public static IEnumerable<Character> GetCharactersSortedForOrder(Order order, IEnumerable<Character> characters, Character controlledCharacter, bool includeSelf, IEnumerable<Character> extraCharacters = null)
		{
			IEnumerable<Character> filteredCharacters = from c in characters
			where c.Info != null && (controlledCharacter == null || ((includeSelf || c != controlledCharacter) && c.TeamID == controlledCharacter.TeamID))
			select c;
			if (extraCharacters != null)
			{
				filteredCharacters = filteredCharacters.Union(extraCharacters);
			}
			Func<Order, bool> <>9__7;
			Func<Order, bool> <>9__8;
			return (from c in filteredCharacters
			orderby Character.Controlled == null || c.Submarine == Character.Controlled.Submarine descending
			select c).ThenByDescending(delegate(Character c)
			{
				if (order.Category.GetValueOrDefault() == OrderCategory.Operate)
				{
					IEnumerable<Order> currentOrders = c.CurrentOrders;
					Func<Order, bool> predicate;
					if ((predicate = <>9__7) == null)
					{
						predicate = (<>9__7 = delegate(Order o)
						{
							if (o != null)
							{
								Identifier identifier = o.Identifier;
								Identifier identifier2 = order.Identifier;
								if (identifier == identifier2)
								{
									return o.TargetEntity == order.TargetEntity;
								}
							}
							return false;
						});
					}
					return currentOrders.Any(predicate);
				}
				return false;
			}).ThenByDescending(new Func<Character, bool>(order.HasAppropriateJob)).ThenByDescending(delegate(Character c)
			{
				IEnumerable<Order> currentOrders = c.CurrentOrders;
				Func<Order, bool> predicate;
				if ((predicate = <>9__8) == null)
				{
					predicate = (<>9__8 = delegate(Order o)
					{
						if (o != null)
						{
							Identifier identifier = o.Identifier;
							Identifier identifier2 = order.Identifier;
							return identifier == identifier2;
						}
						return false;
					});
				}
				return currentOrders.None(predicate);
			}).ThenByDescending(new Func<Character, bool>(order.HasPreferredJob)).ThenByDescending((Character c) => c.IsBot).ThenBy(delegate(Character c)
			{
				HumanAIController humanAI = c.AIController as HumanAIController;
				if (humanAI == null)
				{
					return new float?(0f);
				}
				AIObjective currentObjective = humanAI.ObjectiveManager.CurrentObjective;
				if (currentObjective == null)
				{
					return null;
				}
				return new float?(currentObjective.Priority);
			}).ThenByDescending((Character c) => c.GetSkillLevel(order.AppropriateSkill));
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x00035AAC File Offset: 0x00033CAC
		public void SaveActiveOrders(XElement element)
		{
			List<Order> ordersToSave = new List<Order>();
			foreach (CrewManager.ActiveOrder activeOrder in this.ActiveOrders)
			{
				Order order = (activeOrder != null) ? activeOrder.Order : null;
				if (order != null && activeOrder.FadeOutTime == null)
				{
					ordersToSave.Add(order.WithManualPriority(CharacterInfo.HighestManualOrderPriority));
				}
			}
			CharacterInfo.SaveOrders(element, ordersToSave.ToArray());
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x00035B38 File Offset: 0x00033D38
		public void LoadActiveOrders(XElement element)
		{
			if (element == null)
			{
				return;
			}
			foreach (Order orderInfo in CharacterInfo.LoadOrders(element))
			{
				IIgnorable ignoreTarget = null;
				if (orderInfo.IsIgnoreOrder)
				{
					Order.OrderTargetType targetType = orderInfo.TargetType;
					if (targetType != Order.OrderTargetType.Entity)
					{
						if (targetType == Order.OrderTargetType.WallSection)
						{
							Structure s = orderInfo.TargetEntity as Structure;
							if (s != null && orderInfo.WallSectionIndex != null)
							{
								ignoreTarget = s.GetSection(orderInfo.WallSectionIndex.Value);
								goto IL_88;
							}
						}
						DebugConsole.ThrowError("Error loading an ignore order - can't find a proper ignore target", null, null, false, false);
						continue;
					}
					ignoreTarget = (orderInfo.TargetEntity as IIgnorable);
				}
				IL_88:
				if (orderInfo.TargetEntity != null && (!orderInfo.IsIgnoreOrder || ignoreTarget != null))
				{
					if (ignoreTarget != null)
					{
						ignoreTarget.OrderedToBeIgnored = true;
					}
					this.AddOrder(orderInfo, null);
				}
			}
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00035C28 File Offset: 0x00033E28
		[CompilerGenerated]
		internal static bool <AddOrder>g__MatchesTarget|32_1(Entity existingTarget, Entity newTarget)
		{
			if (existingTarget == newTarget)
			{
				return true;
			}
			Hull existingHullTarget = existingTarget as Hull;
			if (existingHullTarget != null)
			{
				Hull newHullTarget = newTarget as Hull;
				if (newHullTarget != null)
				{
					return existingHullTarget.linkedTo.Contains(newHullTarget);
				}
			}
			return false;
		}

		// Token: 0x040002CF RID: 719
		private const float ConversationIntervalMin = 100f;

		// Token: 0x040002D0 RID: 720
		private const float ConversationIntervalMax = 180f;

		// Token: 0x040002D1 RID: 721
		private const float ConversationIntervalMultiplierMultiplayer = 5f;

		// Token: 0x040002D2 RID: 722
		private float conversationTimer;

		// Token: 0x040002D3 RID: 723
		private float conversationLineTimer;

		// Token: 0x040002D4 RID: 724
		[TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})]
		private readonly List<ValueTuple<Character, string>> pendingConversationLines = new List<ValueTuple<Character, string>>();

		// Token: 0x040002D5 RID: 725
		public const int MaxCrewSize = 16;

		// Token: 0x040002D6 RID: 726
		private readonly List<CharacterInfo> characterInfos = new List<CharacterInfo>();

		// Token: 0x040002D7 RID: 727
		private readonly List<Character> characters = new List<Character>();

		// Token: 0x040002D8 RID: 728
		private readonly List<CharacterInfo> reserveBench = new List<CharacterInfo>();

		// Token: 0x040002D9 RID: 729
		private Character welcomeMessageNPC;

		// Token: 0x040002DD RID: 733
		public ReadyCheck ActiveReadyCheck;

		// Token: 0x02000625 RID: 1573
		public class ActiveOrder
		{
			// Token: 0x06004D57 RID: 19799 RVA: 0x001DF353 File Offset: 0x001DD553
			public ActiveOrder(Order order, float? fadeOutTime)
			{
				this.Order = order;
				this.FadeOutTime = fadeOutTime;
			}

			// Token: 0x04002895 RID: 10389
			public readonly Order Order;

			// Token: 0x04002896 RID: 10390
			public float? FadeOutTime;
		}
	}
}
