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
	// Token: 0x02000050 RID: 80
	internal class CargoMission : Mission
	{
		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000B48 RID: 2888 RVA: 0x0006952E File Offset: 0x0006772E
		public override bool DisplayAsCompleted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06000B49 RID: 2889 RVA: 0x00069531 File Offset: 0x00067731
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x00069534 File Offset: 0x00067734
		public override RichString GetMissionRewardText(Submarine sub)
		{
			LocalizedString rewardText = base.GetRewardAmountText(sub);
			LocalizedString retVal;
			if (this.rewardPerCrate != null)
			{
				LocalizedString rewardPerCrateText = TextManager.GetWithVariable("currencyformat", "[credits]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", this.rewardPerCrate.Value), FormatCapitals.No);
				string tag = "missionrewardcargopercrate";
				ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[4];
				array[0] = new ValueTuple<string, LocalizedString>("[rewardpercrate]", rewardPerCrateText);
				array[1] = new ValueTuple<string, LocalizedString>("[itemcount]", this.itemsToSpawn.Count.ToString());
				array[2] = new ValueTuple<string, LocalizedString>("[maxitemcount]", this.maxItemCount.ToString());
				int num = 3;
				string item = "[totalreward]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:gui.orange‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(rewardText);
				defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
				array[num] = new ValueTuple<string, LocalizedString>(item, defaultInterpolatedStringHandler.ToStringAndClear());
				retVal = TextManager.GetWithVariables(tag, array);
			}
			else
			{
				string tag2 = "missionrewardcargo";
				ValueTuple<string, string>[] array2 = new ValueTuple<string, string>[3];
				int num2 = 0;
				string item2 = "[totalreward]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:gui.orange‖");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(rewardText);
				defaultInterpolatedStringHandler2.AppendLiteral("‖end‖");
				array2[num2] = new ValueTuple<string, string>(item2, defaultInterpolatedStringHandler2.ToStringAndClear());
				array2[1] = new ValueTuple<string, string>("[itemcount]", this.itemsToSpawn.Count.ToString());
				array2[2] = new ValueTuple<string, string>("[maxitemcount]", this.maxItemCount.ToString());
				retVal = TextManager.GetWithVariables(tag2, array2);
			}
			return RichString.Rich(retVal, null);
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x000696E4 File Offset: 0x000678E4
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			this.items.Clear();
			ushort itemCount = msg.ReadUInt16();
			for (int i = 0; i < (int)itemCount; i++)
			{
				this.items.Add(Item.ReadSpawnData(msg, true));
			}
			if (this.items.Contains(null))
			{
				throw new Exception("Error in CargoMission.ClientReadInitial: item list contains null (mission: " + this.Prefab.Identifier.ToString() + ")");
			}
			if (this.items.Count != (int)itemCount)
			{
				throw new Exception(string.Concat(new string[]
				{
					"Error in CargoMission.ClientReadInitial: item count does not match the server count (",
					itemCount.ToString(),
					" != ",
					this.items.Count.ToString(),
					", mission: ",
					this.Prefab.Identifier.ToString(),
					")"
				}));
			}
			if (this.requiredDeliveryAmount == 0f)
			{
				this.requiredDeliveryAmount = (float)this.items.Count;
			}
			if (this.requiredDeliveryAmount > (float)this.items.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". Required delivery amount is ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.requiredDeliveryAmount);
				defaultInterpolatedStringHandler.AppendLiteral(" but there's only ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.items.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" items to deliver.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.Prefab.ContentPackage);
				this.requiredDeliveryAmount = (float)this.items.Count;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06000B4C RID: 2892 RVA: 0x000698A4 File Offset: 0x00067AA4
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

		// Token: 0x06000B4D RID: 2893 RVA: 0x0006994C File Offset: 0x00067B4C
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

		// Token: 0x06000B4E RID: 2894 RVA: 0x00069A28 File Offset: 0x00067C28
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

		// Token: 0x06000B4F RID: 2895 RVA: 0x0006A1F8 File Offset: 0x000683F8
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

		// Token: 0x06000B50 RID: 2896 RVA: 0x0006A338 File Offset: 0x00068538
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

		// Token: 0x06000B51 RID: 2897 RVA: 0x0006A424 File Offset: 0x00068624
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

		// Token: 0x06000B52 RID: 2898 RVA: 0x0006A5B4 File Offset: 0x000687B4
		protected override void StartMissionSpecific(Level level)
		{
			this.items.Clear();
			this.parentInventoryIDs.Clear();
			if (!Mission.IsClient)
			{
				this.InitItems();
			}
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x0006A5DC File Offset: 0x000687DC
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

		// Token: 0x06000B54 RID: 2900 RVA: 0x0006A644 File Offset: 0x00068844
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

		// Token: 0x06000B55 RID: 2901 RVA: 0x0006A6B4 File Offset: 0x000688B4
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

		// Token: 0x040005D4 RID: 1492
		private readonly ContentXElement itemConfig;

		// Token: 0x040005D5 RID: 1493
		private readonly List<Item> items = new List<Item>();

		// Token: 0x040005D6 RID: 1494
		private readonly Dictionary<Item, ushort> parentInventoryIDs = new Dictionary<Item, ushort>();

		// Token: 0x040005D7 RID: 1495
		private readonly Dictionary<Item, int> inventorySlotIndices = new Dictionary<Item, int>();

		// Token: 0x040005D8 RID: 1496
		private readonly Dictionary<Item, byte> parentItemContainerIndices = new Dictionary<Item, byte>();

		// Token: 0x040005D9 RID: 1497
		private float requiredDeliveryAmount;

		// Token: 0x040005DA RID: 1498
		[TupleElementNames(new string[]
		{
			"element",
			"container"
		})]
		private readonly List<ValueTuple<ContentXElement, ItemContainer>> itemsToSpawn = new List<ValueTuple<ContentXElement, ItemContainer>>();

		// Token: 0x040005DB RID: 1499
		private int? rewardPerCrate;

		// Token: 0x040005DC RID: 1500
		private int calculatedReward;

		// Token: 0x040005DD RID: 1501
		private int maxItemCount;

		// Token: 0x040005DE RID: 1502
		private Submarine currentSub;

		// Token: 0x040005DF RID: 1503
		private SubmarineInfo nextRoundSubInfo;

		// Token: 0x040005E0 RID: 1504
		private readonly List<CargoMission> previouslySelectedMissions = new List<CargoMission>();
	}
}
