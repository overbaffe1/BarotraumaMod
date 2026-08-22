using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000021 RID: 33
	internal class CargoMission : Mission
	{
		// Token: 0x06000451 RID: 1105 RVA: 0x000253F0 File Offset: 0x000235F0
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteUInt16((ushort)this.items.Count);
			foreach (Item item in this.items)
			{
				item.WriteSpawnData(msg, item.ID, this.parentInventoryIDs.ContainsKey(item) ? this.parentInventoryIDs[item] : 0, this.parentItemContainerIndices.ContainsKey(item) ? this.parentItemContainerIndices[item] : 0, this.inventorySlotIndices.ContainsKey(item) ? this.inventorySlotIndices[item] : -1);
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000452 RID: 1106 RVA: 0x000254BC File Offset: 0x000236BC
		public override LocalizedString Description
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				SubmarineInfo submarineInfo;
				if (gameSession == null)
				{
					submarineInfo = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					submarineInfo = ((campaign != null) ? campaign.PendingSubmarineSwitch : null);
				}
				SubmarineInfo submarineInfo2;
				if ((submarineInfo2 = submarineInfo) == null)
				{
					Submarine mainSub = Submarine.MainSub;
					submarineInfo2 = ((mainSub != null) ? mainSub.Info : null);
				}
				if (submarineInfo2 != this.nextRoundSubInfo)
				{
					string rewardText = "‖color:gui.orange‖" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", base.GetReward(Submarine.MainSub)) + "‖end‖";
					if (this.descriptionWithoutReward != null)
					{
						this.description = this.descriptionWithoutReward.Replace("[reward]", rewardText, StringComparison.Ordinal);
					}
				}
				return this.description;
			}
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x00025564 File Offset: 0x00023764
		public CargoMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.currentSub = sub;
			this.nextRoundSubInfo = ((sub != null) ? sub.Info : null);
			this.itemConfig = prefab.ConfigElement.GetChildElement("Items");
			this.requiredDeliveryAmount = Math.Min(prefab.ConfigElement.GetAttributeFloat("requireddeliveryamount", 0.98f), 1f);
			if (sub == null || sub.Loading || sub.Removed || Submarine.Unloading || !Submarine.Loaded.Contains(sub))
			{
				return;
			}
			this.DetermineCargo();
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x00025640 File Offset: 0x00023840
		private void DetermineCargo()
		{
			if (this.currentSub != null)
			{
				ContentXElement contentXElement = null;
				if (!(this.itemConfig == contentXElement))
				{
					this.itemsToSpawn.Clear();
					this.maxItemCount = 0;
					foreach (ContentXElement subElement2 in this.itemConfig.Elements())
					{
						int maxCount = subElement2.GetAttributeInt("maxcount", 10);
						this.maxItemCount += maxCount;
					}
					GameSession gameSession = GameMain.GameSession;
					SubmarineInfo submarineInfo;
					if (gameSession == null)
					{
						submarineInfo = null;
					}
					else
					{
						CampaignMode campaign2 = gameSession.Campaign;
						submarineInfo = ((campaign2 != null) ? campaign2.PendingSubmarineSwitch : null);
					}
					SubmarineInfo pendingSubInfo = submarineInfo;
					if (pendingSubInfo != null && pendingSubInfo != this.currentSub.Info)
					{
						this.maxItemCount = Math.Min(this.maxItemCount, pendingSubInfo.CargoCapacity);
						this.previouslySelectedMissions.Clear();
						GameSession gameSession2 = GameMain.GameSession;
						bool flag;
						if (gameSession2 == null)
						{
							flag = (null != null);
						}
						else
						{
							Location startLocation = gameSession2.StartLocation;
							flag = (((startLocation != null) ? startLocation.SelectedMissions : null) != null);
						}
						if (flag)
						{
							bool isPriorMission = true;
							foreach (Mission mission in GameMain.GameSession.StartLocation.SelectedMissions)
							{
								CargoMission otherMission = mission as CargoMission;
								if (otherMission != null)
								{
									if (mission == this)
									{
										isPriorMission = false;
									}
									this.previouslySelectedMissions.Add(otherMission);
									if (isPriorMission)
									{
										this.maxItemCount -= otherMission.itemsToSpawn.Count;
									}
								}
							}
						}
						for (int i = 0; i < this.maxItemCount; i++)
						{
							using (IEnumerator<ContentXElement> enumerator3 = this.itemConfig.Elements().GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									ContentXElement subElement = enumerator3.Current;
									int maxCount2 = subElement.GetAttributeInt("maxcount", 10);
									if (this.itemsToSpawn.Count(([TupleElementNames(new string[]
									{
										"element",
										"container"
									})] ValueTuple<ContentXElement, ItemContainer> it) => it.Item1 == subElement) < maxCount2)
									{
										base.FindItemPrefab(subElement);
										Func<ValueTuple<ContentXElement, ItemContainer>, bool> <>9__1;
										while (this.itemsToSpawn.Count < this.maxItemCount)
										{
											this.itemsToSpawn.Add(new ValueTuple<ContentXElement, ItemContainer>(subElement, null));
											IEnumerable<ValueTuple<ContentXElement, ItemContainer>> source = this.itemsToSpawn;
											Func<ValueTuple<ContentXElement, ItemContainer>, bool> predicate;
											if ((predicate = <>9__1) == null)
											{
												predicate = (<>9__1 = (([TupleElementNames(new string[]
												{
													"element",
													"container"
												})] ValueTuple<ContentXElement, ItemContainer> it) => it.Item1 == subElement));
											}
											if (source.Count(predicate) >= maxCount2)
											{
												break;
											}
										}
									}
								}
							}
						}
						this.maxItemCount = Math.Max(0, this.maxItemCount);
						this.nextRoundSubInfo = pendingSubInfo;
					}
					else
					{
						List<ValueTuple<ItemContainer, int>> containers = this.currentSub.GetCargoContainers();
						containers.Sort(([TupleElementNames(new string[]
						{
							"container",
							"freeSlots"
						})] ValueTuple<ItemContainer, int> c1, [TupleElementNames(new string[]
						{
							"container",
							"freeSlots"
						})] ValueTuple<ItemContainer, int> c2) => c2.Item1.Capacity.CompareTo(c1.Item1.Capacity));
						this.previouslySelectedMissions.Clear();
						GameSession gameSession3 = GameMain.GameSession;
						bool flag2;
						if (gameSession3 == null)
						{
							flag2 = (null != null);
						}
						else
						{
							Location startLocation2 = gameSession3.StartLocation;
							flag2 = (((startLocation2 != null) ? startLocation2.SelectedMissions : null) != null);
						}
						if (flag2)
						{
							bool isPriorMission2 = true;
							foreach (Mission mission2 in GameMain.GameSession.StartLocation.SelectedMissions)
							{
								CargoMission otherMission2 = mission2 as CargoMission;
								if (otherMission2 != null)
								{
									if (mission2 == this)
									{
										isPriorMission2 = false;
									}
									this.previouslySelectedMissions.Add(otherMission2);
									if (isPriorMission2)
									{
										foreach (ValueTuple<ContentXElement, ItemContainer> valueTuple in otherMission2.itemsToSpawn)
										{
											ContentXElement element = valueTuple.Item1;
											ItemContainer container = valueTuple.Item2;
											for (int j = 0; j < containers.Count; j++)
											{
												if (containers[j].Item1 == container)
												{
													containers[j] = new ValueTuple<ItemContainer, int>(containers[j].Item1, containers[j].Item2 - 1);
													break;
												}
											}
										}
									}
								}
							}
						}
						for (int k = 0; k < containers.Count; k++)
						{
							using (IEnumerator<ContentXElement> enumerator6 = this.itemConfig.Elements().GetEnumerator())
							{
								while (enumerator6.MoveNext())
								{
									ContentXElement subElement = enumerator6.Current;
									int maxCount3 = subElement.GetAttributeInt("maxcount", 10);
									if (this.itemsToSpawn.Count(([TupleElementNames(new string[]
									{
										"element",
										"container"
									})] ValueTuple<ContentXElement, ItemContainer> it) => it.Item1 == subElement) < maxCount3)
									{
										ItemPrefab itemPrefab = base.FindItemPrefab(subElement);
										Func<ValueTuple<ContentXElement, ItemContainer>, bool> <>9__4;
										while (containers[k].Item2 > 0 && containers[k].Item1.Inventory.CanProbablyBePut(itemPrefab, null, null))
										{
											containers[k] = new ValueTuple<ItemContainer, int>(containers[k].Item1, containers[k].Item2 - 1);
											this.itemsToSpawn.Add(new ValueTuple<ContentXElement, ItemContainer>(subElement, containers[k].Item1));
											IEnumerable<ValueTuple<ContentXElement, ItemContainer>> source2 = this.itemsToSpawn;
											Func<ValueTuple<ContentXElement, ItemContainer>, bool> predicate2;
											if ((predicate2 = <>9__4) == null)
											{
												predicate2 = (<>9__4 = (([TupleElementNames(new string[]
												{
													"element",
													"container"
												})] ValueTuple<ContentXElement, ItemContainer> it) => it.Item1 == subElement));
											}
											if (source2.Count(predicate2) >= maxCount3)
											{
												break;
											}
										}
									}
								}
							}
						}
					}
					if (!this.itemsToSpawn.Any<ValueTuple<ContentXElement, ItemContainer>>())
					{
						this.itemsToSpawn.Add(new ValueTuple<ContentXElement, ItemContainer>(this.itemConfig.Elements().First<ContentXElement>(), null));
					}
					this.calculatedReward = 0;
					bool crateValuesUniform = true;
					int? prevCrateReward = null;
					foreach (ValueTuple<ContentXElement, ItemContainer> valueTuple2 in this.itemsToSpawn)
					{
						ContentXElement element2 = valueTuple2.Item1;
						ItemContainer container2 = valueTuple2.Item2;
						int currentCrateReward = element2.GetAttributeInt("reward", 0);
						this.calculatedReward += currentCrateReward;
						if (crateValuesUniform)
						{
							if (prevCrateReward != null && prevCrateReward.Value != currentCrateReward)
							{
								crateValuesUniform = false;
							}
							prevCrateReward = new int?(currentCrateReward);
						}
					}
					if (crateValuesUniform)
					{
						this.rewardPerCrate = new int?(this.calculatedReward / this.itemsToSpawn.Count);
					}
					else
					{
						this.rewardPerCrate = null;
					}
					GameSession gameSession4 = GameMain.GameSession;
					CampaignMode campaign = (gameSession4 != null) ? gameSession4.Campaign : null;
					if (campaign != null)
					{
						int? num = this.rewardPerCrate;
						if (num != null)
						{
							int confirmedRewardPerCrate = num.GetValueOrDefault();
							this.rewardPerCrate = new int?((int)Math.Round((double)((float)confirmedRewardPerCrate * campaign.Settings.MissionRewardMultiplier)));
						}
					}
					string rewardText = "‖color:gui.orange‖" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", base.GetReward(this.currentSub)) + "‖end‖";
					if (this.descriptionWithoutReward != null)
					{
						this.description = this.descriptionWithoutReward.Replace("[reward]", rewardText, StringComparison.Ordinal);
					}
					return;
				}
			}
			this.calculatedReward = this.Prefab.Reward;
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x00025E10 File Offset: 0x00024010
		public override float GetBaseReward(Submarine sub)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.StartLocation : null) != this.Locations[0])
			{
				return (float)this.calculatedReward;
			}
			bool missionsChanged = false;
			GameSession gameSession2 = GameMain.GameSession;
			bool flag;
			if (gameSession2 == null)
			{
				flag = (null != null);
			}
			else
			{
				Location startLocation = gameSession2.StartLocation;
				flag = (((startLocation != null) ? startLocation.SelectedMissions : null) != null);
			}
			if (flag)
			{
				List<Mission> currentMissions = (from m in GameMain.GameSession.StartLocation.SelectedMissions
				where m is CargoMission
				select m).ToList<Mission>();
				if (currentMissions.Count != this.previouslySelectedMissions.Count)
				{
					missionsChanged = true;
				}
				else
				{
					for (int i = 0; i < this.previouslySelectedMissions.Count; i++)
					{
						if (this.previouslySelectedMissions[i] != currentMissions[i])
						{
							missionsChanged = true;
							break;
						}
					}
				}
			}
			GameSession gameSession3 = GameMain.GameSession;
			SubmarineInfo submarineInfo;
			if (gameSession3 == null)
			{
				submarineInfo = null;
			}
			else
			{
				CampaignMode campaign = gameSession3.Campaign;
				submarineInfo = ((campaign != null) ? campaign.PendingSubmarineSwitch : null);
			}
			SubmarineInfo pendingSubInfo = submarineInfo;
			if (pendingSubInfo != null && this.nextRoundSubInfo != pendingSubInfo)
			{
				this.nextRoundSubInfo = pendingSubInfo;
				this.DetermineCargo();
			}
			else if (sub != this.currentSub || missionsChanged)
			{
				this.currentSub = sub;
				this.nextRoundSubInfo = ((sub != null) ? sub.Info : null);
				this.DetermineCargo();
			}
			return (float)this.calculatedReward;
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00025F50 File Offset: 0x00024150
		private void InitItems()
		{
			this.currentSub = Submarine.MainSub;
			this.DetermineCargo();
			this.items.Clear();
			this.parentInventoryIDs.Clear();
			this.parentItemContainerIndices.Clear();
			this.inventorySlotIndices.Clear();
			ContentXElement contentXElement = null;
			if (this.itemConfig == contentXElement)
			{
				DebugConsole.ThrowError("Failed to initialize items for cargo mission (itemConfig == null)", null, this.Prefab.ContentPackage, false, false);
				return;
			}
			foreach (ValueTuple<ContentXElement, ItemContainer> valueTuple in this.itemsToSpawn)
			{
				ContentXElement element = valueTuple.Item1;
				ItemContainer container = valueTuple.Item2;
				this.LoadItemAsChild(element, (container != null) ? container.Item : null);
			}
			if (this.requiredDeliveryAmount <= 0f)
			{
				this.requiredDeliveryAmount = 1f;
			}
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x0002603C File Offset: 0x0002423C
		private void LoadItemAsChild(ContentXElement element, Item parent)
		{
			ItemPrefab itemPrefab = base.FindItemPrefab(element);
			Submarine cargoRoomSub;
			Vector2? position = base.GetCargoSpawnPosition(itemPrefab, out cargoRoomSub);
			if (position == null)
			{
				return;
			}
			Item item = new Item(itemPrefab, position.Value, cargoRoomSub, 0, true)
			{
				SpawnedInCurrentOutpost = true,
				AllowStealing = false
			};
			item.AddTag(Tags.CargoMissionItem);
			item.AddTag(this.Prefab.Identifier);
			foreach (Identifier tag in this.Prefab.Tags)
			{
				item.AddTag(tag);
			}
			item.FindHull();
			this.items.Add(item);
			if (((parent != null) ? parent.GetComponent<ItemContainer>() : null) != null)
			{
				this.parentInventoryIDs.Add(item, parent.ID);
				this.parentItemContainerIndices.Add(item, (byte)parent.GetComponentIndex(parent.GetComponent<ItemContainer>()));
				parent.Combine(item, null);
				Dictionary<Item, int> dictionary = this.inventorySlotIndices;
				Item key = item;
				Inventory parentInventory = item.ParentInventory;
				dictionary.Add(key, (parentInventory != null) ? parentInventory.FindIndex(item) : -1);
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				int amount = subElement.GetAttributeInt("amount", 1);
				for (int i = 0; i < amount; i++)
				{
					this.LoadItemAsChild(subElement, item);
				}
			}
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x000261CC File Offset: 0x000243CC
		protected override void StartMissionSpecific(Level level)
		{
			this.items.Clear();
			this.parentInventoryIDs.Clear();
			if (!Mission.IsClient)
			{
				this.InitItems();
			}
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x000261F4 File Offset: 0x000243F4
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			if (Submarine.MainSub != null && Submarine.MainSub.AtEndExit)
			{
				int deliveredItemCount = this.items.Count((Item it) => CargoMission.IsItemDelivered(it));
				if ((float)deliveredItemCount / (float)this.items.Count >= this.requiredDeliveryAmount)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x0002625C File Offset: 0x0002445C
		protected override void EndMissionSpecific(bool completed)
		{
			foreach (Item item in this.items)
			{
				if (!item.Removed)
				{
					item.Remove();
				}
			}
			this.items.Clear();
			this.failed = !completed;
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x000262CC File Offset: 0x000244CC
		private static bool IsItemDelivered(Item item)
		{
			if (item.Removed || item.Condition <= 0f || Submarine.MainSub == null)
			{
				return false;
			}
			Submarine submarine2;
			if ((submarine2 = item.Submarine) == null)
			{
				Item rootContainer = item.RootContainer;
				submarine2 = ((rootContainer != null) ? rootContainer.Submarine : null);
			}
			Submarine submarine = submarine2;
			return submarine == Submarine.MainSub || Submarine.MainSub.GetConnectedSubs().Contains(submarine);
		}

		// Token: 0x04000217 RID: 535
		private readonly ContentXElement itemConfig;

		// Token: 0x04000218 RID: 536
		private readonly List<Item> items = new List<Item>();

		// Token: 0x04000219 RID: 537
		private readonly Dictionary<Item, ushort> parentInventoryIDs = new Dictionary<Item, ushort>();

		// Token: 0x0400021A RID: 538
		private readonly Dictionary<Item, int> inventorySlotIndices = new Dictionary<Item, int>();

		// Token: 0x0400021B RID: 539
		private readonly Dictionary<Item, byte> parentItemContainerIndices = new Dictionary<Item, byte>();

		// Token: 0x0400021C RID: 540
		private float requiredDeliveryAmount;

		// Token: 0x0400021D RID: 541
		[TupleElementNames(new string[]
		{
			"element",
			"container"
		})]
		private readonly List<ValueTuple<ContentXElement, ItemContainer>> itemsToSpawn = new List<ValueTuple<ContentXElement, ItemContainer>>();

		// Token: 0x0400021E RID: 542
		private int? rewardPerCrate;

		// Token: 0x0400021F RID: 543
		private int calculatedReward;

		// Token: 0x04000220 RID: 544
		private int maxItemCount;

		// Token: 0x04000221 RID: 545
		private Submarine currentSub;

		// Token: 0x04000222 RID: 546
		private SubmarineInfo nextRoundSubInfo;

		// Token: 0x04000223 RID: 547
		private readonly List<CargoMission> previouslySelectedMissions = new List<CargoMission>();
	}
}
