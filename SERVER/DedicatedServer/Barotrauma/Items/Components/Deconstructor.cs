using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200049B RID: 1179
	internal class Deconstructor : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x060040AC RID: 16556 RVA: 0x0019DAEC File Offset: 0x0019BCEC
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			bool active = msg.ReadBoolean();
			this.item.CreateServerEvent<Deconstructor>(this);
			if (this.item.CanClientAccess(c))
			{
				this.SetActive(active, c.Character, false);
			}
		}

		// Token: 0x060040AD RID: 16557 RVA: 0x0019DB28 File Offset: 0x0019BD28
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Character character = this.user;
			msg.WriteUInt16((character != null) ? character.ID : 0);
			msg.WriteBoolean(this.IsActive);
			msg.WriteSingle(this.progressTimer);
		}

		// Token: 0x1700112E RID: 4398
		// (get) Token: 0x060040AE RID: 16558 RVA: 0x0019DB5A File Offset: 0x0019BD5A
		public ItemContainer InputContainer
		{
			get
			{
				return this.inputContainer;
			}
		}

		// Token: 0x1700112F RID: 4399
		// (get) Token: 0x060040AF RID: 16559 RVA: 0x0019DB62 File Offset: 0x0019BD62
		public ItemContainer OutputContainer
		{
			get
			{
				return this.outputContainer;
			}
		}

		// Token: 0x17001130 RID: 4400
		// (get) Token: 0x060040B0 RID: 16560 RVA: 0x0019DB6A File Offset: 0x0019BD6A
		// (set) Token: 0x060040B1 RID: 16561 RVA: 0x0019DB72 File Offset: 0x0019BD72
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DeconstructItemsSimultaneously { get; set; }

		// Token: 0x17001131 RID: 4401
		// (get) Token: 0x060040B2 RID: 16562 RVA: 0x0019DB7B File Offset: 0x0019BD7B
		// (set) Token: 0x060040B3 RID: 16563 RVA: 0x0019DB83 File Offset: 0x0019BD83
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 1000f)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float DeconstructionSpeed { get; set; }

		// Token: 0x060040B4 RID: 16564 RVA: 0x0019DB8C File Offset: 0x0019BD8C
		public Deconstructor(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060040B5 RID: 16565 RVA: 0x0019DBA4 File Offset: 0x0019BDA4
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<ItemContainer> containers = this.item.GetComponents<ItemContainer>().ToList<ItemContainer>();
			if (containers.Count < 2)
			{
				DebugConsole.ThrowError("Error in item \"" + this.item.Name + "\": Deconstructors must have two ItemContainer components!", null, null, false, false);
				return;
			}
			this.inputContainer = containers[0];
			this.outputContainer = containers[1];
		}

		// Token: 0x060040B6 RID: 16566 RVA: 0x0019DC10 File Offset: 0x0019BE10
		public override void Update(float deltaTime, Camera cam)
		{
			this.MoveInputQueue();
			if (this.inputContainer == null || this.inputContainer.Inventory.IsEmpty())
			{
				this.SetActive(false, null, false);
				return;
			}
			if (!this.HasPower)
			{
				return;
			}
			Repairable repairable = this.item.GetComponent<Repairable>();
			if (repairable != null)
			{
				repairable.LastActiveTime = (float)Timing.TotalTime + 10f;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			this.progressTimer += deltaTime * Math.Min((this.powerConsumption <= 0f) ? 1f : base.Voltage, 2f);
			float tinkeringStrength = 0f;
			if (repairable.IsTinkering)
			{
				tinkeringStrength = repairable.TinkeringStrength;
			}
			float deconstructionSpeedModifier = this.userDeconstructorSpeedMultiplier * (1f + tinkeringStrength * 2.5f);
			float deconstructionSpeed = this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.DeconstructorSpeed, this.DeconstructionSpeed);
			if (this.DeconstructItemsSimultaneously)
			{
				float deconstructTime = 0f;
				foreach (Item targetItem3 in this.inputContainer.Inventory.AllItems)
				{
					Submarine submarine = this.item.Submarine;
					if (submarine == null)
					{
						goto IL_143;
					}
					SubmarineInfo info = submarine.Info;
					if (info == null || info.Type != SubmarineType.Outpost)
					{
						goto IL_143;
					}
					float num = targetItem3.Prefab.DeconstructTimeInOutposts;
					IL_15D:
					float itemDeconstructTime = num;
					float targetDeconstructTime = itemDeconstructTime / (deconstructionSpeed * deconstructionSpeedModifier);
					LinkedControllerCharacterComponent linkedCharacter = targetItem3.GetComponent<LinkedControllerCharacterComponent>();
					if (linkedCharacter != null)
					{
						targetDeconstructTime *= linkedCharacter.DeconstructTimeMultiplier;
					}
					deconstructTime += targetDeconstructTime;
					this.ApplyDeconstructionStatusEffects(targetItem3, ActionType.OnDeconstructing, deltaTime);
					continue;
					IL_143:
					num = targetItem3.Prefab.DeconstructTime;
					goto IL_15D;
				}
				this.progressState = Math.Min(this.progressTimer / deconstructTime, 1f);
				if (this.progressTimer > deconstructTime)
				{
					List<Item> items = this.inputContainer.Inventory.AllItems.ToList<Item>();
					using (List<Item>.Enumerator enumerator2 = items.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Item targetItem = enumerator2.Current;
							EntitySpawner spawner = Entity.Spawner;
							if ((spawner == null || !spawner.IsInRemoveQueue(targetItem)) && this.inputContainer.Inventory.AllItems.Contains(targetItem))
							{
								Func<Identifier, bool> <>9__1;
								List<DeconstructItem> validDeconstructItems = targetItem.Prefab.DeconstructItems.Where(delegate(DeconstructItem it)
								{
									if (!it.IsValidDeconstructor(this.item))
									{
										return false;
									}
									if (it.RequiredOtherItem.Length != 0)
									{
										IEnumerable<Identifier> requiredOtherItem = it.RequiredOtherItem;
										Func<Identifier, bool> predicate;
										if ((predicate = <>9__1) == null)
										{
											predicate = (<>9__1 = ((Identifier r) => items.Any((Item it) => it != targetItem && (it.HasTag(r) || it.Prefab.Identifier == r))));
										}
										return requiredOtherItem.Any(predicate);
									}
									return true;
								}).ToList<DeconstructItem>();
								this.ProcessItem(targetItem, items, validDeconstructItems, validDeconstructItems.Any<DeconstructItem>() || !targetItem.Prefab.DeconstructItems.Any<DeconstructItem>());
							}
						}
					}
					this.item.CreateServerEvent<Deconstructor>(this);
					this.progressTimer = 0f;
					this.progressState = 0f;
					return;
				}
			}
			else
			{
				Item targetItem2 = this.inputContainer.Inventory.LastOrDefault();
				if (targetItem2 == null)
				{
					return;
				}
				this.ApplyDeconstructionStatusEffects(targetItem2, ActionType.OnDeconstructing, deltaTime);
				List<DeconstructItem> validDeconstructItems2 = (from it in targetItem2.Prefab.DeconstructItems
				where it.IsValidDeconstructor(this.item)
				select it).ToList<DeconstructItem>();
				Submarine submarine = this.item.Submarine;
				float num2;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null && info.Type == SubmarineType.Outpost)
					{
						num2 = targetItem2.Prefab.DeconstructTimeInOutposts;
						goto IL_39A;
					}
				}
				num2 = targetItem2.Prefab.DeconstructTime;
				IL_39A:
				float itemDeconstructTime2 = num2;
				float deconstructTime2 = (!targetItem2.Prefab.DeconstructItems.Any<DeconstructItem>() || validDeconstructItems2.Any<DeconstructItem>()) ? (itemDeconstructTime2 / (deconstructionSpeed * deconstructionSpeedModifier)) : 1f;
				LinkedControllerCharacterComponent linkedCharacter2 = targetItem2.GetComponent<LinkedControllerCharacterComponent>();
				if (linkedCharacter2 != null)
				{
					deconstructTime2 *= linkedCharacter2.DeconstructTimeMultiplier;
				}
				this.progressState = Math.Min(this.progressTimer / deconstructTime2, 1f);
				if (this.progressTimer > deconstructTime2)
				{
					this.ProcessItem(targetItem2, this.inputContainer.Inventory.AllItemsMod, validDeconstructItems2, validDeconstructItems2.Any<DeconstructItem>() || !targetItem2.Prefab.DeconstructItems.Any<DeconstructItem>());
					this.item.CreateServerEvent<Deconstructor>(this);
					this.progressTimer = 0f;
					this.progressState = 0f;
				}
			}
		}

		// Token: 0x060040B7 RID: 16567 RVA: 0x0019E098 File Offset: 0x0019C298
		private void ProcessItem(Item targetItem, IEnumerable<Item> inputItems, List<DeconstructItem> validDeconstructItems, bool allowRemove = true)
		{
			Deconstructor.<>c__DisplayClass28_0 CS$<>8__locals1 = new Deconstructor.<>c__DisplayClass28_0();
			CS$<>8__locals1.targetItem = targetItem;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.allowRemove = allowRemove;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			float amountMultiplier = 1f;
			if (this.user != null && !this.user.Removed)
			{
				AbilityDeconstructedItem abilityTargetItem = new AbilityDeconstructedItem(CS$<>8__locals1.targetItem, this.user);
				this.user.CheckTalents(AbilityEffectType.OnItemDeconstructed, abilityTargetItem);
				foreach (Character character in Character.GetFriendlyCrew(this.user))
				{
					character.CheckTalents(AbilityEffectType.OnItemDeconstructedByAlly, abilityTargetItem);
				}
				AbilityItemCreationMultiplier itemCreationMultiplier = new AbilityItemCreationMultiplier(CS$<>8__locals1.targetItem.Prefab, amountMultiplier);
				this.user.CheckTalents(AbilityEffectType.OnItemDeconstructedMaterial, itemCreationMultiplier);
				amountMultiplier = (float)((int)itemCreationMultiplier.Value);
			}
			if (CS$<>8__locals1.targetItem.Prefab.RandomDeconstructionOutput)
			{
				int amount = CS$<>8__locals1.targetItem.Prefab.RandomDeconstructionOutputAmount;
				List<int> deconstructItemIndexes = new List<int>();
				for (int k = 0; k < validDeconstructItems.Count; k++)
				{
					deconstructItemIndexes.Add(k);
				}
				List<float> commonness = (from i in validDeconstructItems
				select i.Commonness).ToList<float>();
				List<DeconstructItem> products = new List<DeconstructItem>();
				int j = 0;
				while (j < amount && deconstructItemIndexes.Count >= 1)
				{
					int itemIndex = ToolBox.SelectWeightedRandom<int>(deconstructItemIndexes, commonness, Rand.RandSync.Unsynced);
					products.Add(validDeconstructItems[itemIndex]);
					int removeIndex = deconstructItemIndexes.IndexOf(itemIndex);
					deconstructItemIndexes.RemoveAt(removeIndex);
					commonness.RemoveAt(removeIndex);
					j++;
				}
				using (List<DeconstructItem>.Enumerator enumerator2 = products.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						DeconstructItem deconstructProduct = enumerator2.Current;
						CS$<>8__locals1.<ProcessItem>g__CreateDeconstructProduct|0(deconstructProduct, inputItems, (int)(amountMultiplier * (float)deconstructProduct.Amount));
					}
					goto IL_22B;
				}
			}
			foreach (DeconstructItem deconstructProduct2 in validDeconstructItems)
			{
				CS$<>8__locals1.<ProcessItem>g__CreateDeconstructProduct|0(deconstructProduct2, inputItems, (int)(amountMultiplier * (float)deconstructProduct2.Amount));
			}
			IL_22B:
			if (CS$<>8__locals1.targetItem.Prefab.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
			{
				string str = "ItemDeconstructed:";
				GameSession gameSession = GameMain.GameSession;
				string text;
				if (gameSession == null)
				{
					text = null;
				}
				else
				{
					GameMode gameMode = gameSession.GameMode;
					text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
				}
				GameAnalyticsManager.AddDesignEvent(str + (text ?? "none") + ":" + CS$<>8__locals1.targetItem.Prefab.Identifier.ToString());
			}
			CS$<>8__locals1.should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventItemDeconstructed>(delegate(IEventItemDeconstructed x)
			{
				bool? flag = x.OnItemDeconstructed(CS$<>8__locals1.targetItem, CS$<>8__locals1.<>4__this, CS$<>8__locals1.<>4__this.user, CS$<>8__locals1.allowRemove);
				CS$<>8__locals1.should = ((flag != null) ? flag : CS$<>8__locals1.should);
			});
			if (CS$<>8__locals1.should.GetValueOrDefault())
			{
				return;
			}
			if (CS$<>8__locals1.targetItem.AllowDeconstruct & CS$<>8__locals1.allowRemove)
			{
				this.ApplyDeconstructionStatusEffects(CS$<>8__locals1.targetItem, ActionType.OnDeconstructed, 1f);
				foreach (ItemContainer ic in CS$<>8__locals1.targetItem.GetComponents<ItemContainer>())
				{
					if (((ic != null) ? ic.Inventory : null) != null && !ic.RemoveContainedItemsOnDeconstruct)
					{
						foreach (Item outputItem in ic.Inventory.AllItemsMod)
						{
							CS$<>8__locals1.<ProcessItem>g__tryPutInOutputSlots|2(outputItem);
							if (this.RelocateOutputToMainSub && this.user != null)
							{
								HumanAIController humanAi = this.user.AIController as HumanAIController;
								if (humanAi != null)
								{
									humanAi.HandleRelocation(outputItem);
								}
							}
						}
					}
				}
				this.inputContainer.Inventory.RemoveItem(CS$<>8__locals1.targetItem);
				Entity.Spawner.AddItemToRemoveQueue(CS$<>8__locals1.targetItem);
				this.MoveInputQueue();
				this.PutItemsToLinkedContainer();
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner != null && spawner.IsInRemoveQueue(CS$<>8__locals1.targetItem))
			{
				CS$<>8__locals1.targetItem.Drop(null, true, true);
				return;
			}
			CS$<>8__locals1.<ProcessItem>g__tryPutInOutputSlots|2(CS$<>8__locals1.targetItem);
		}

		// Token: 0x060040B8 RID: 16568 RVA: 0x0019E524 File Offset: 0x0019C724
		private void TryMoveItemToOutputContainers(Item spawnedItem)
		{
			for (int i = 0; i < this.outputContainer.Capacity; i++)
			{
				Item containedItem = this.outputContainer.Inventory.GetItemAt(i);
				bool combined = false;
				if (((containedItem != null) ? containedItem.OwnInventory : null) != null)
				{
					foreach (Item subItem in containedItem.ContainedItems.ToList<Item>())
					{
						if (subItem.Combine(spawnedItem, null))
						{
							combined = true;
							break;
						}
					}
				}
				if (!combined && containedItem != null && containedItem.Combine(spawnedItem, null))
				{
					break;
				}
			}
			this.PutItemsToLinkedContainer();
		}

		// Token: 0x060040B9 RID: 16569 RVA: 0x0019E5D8 File Offset: 0x0019C7D8
		private void PutItemsToLinkedContainer()
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.outputContainer.Inventory.IsEmpty())
			{
				return;
			}
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Item linkedItem = linkedTo as Item;
				if (linkedItem != null)
				{
					if (linkedItem.GetComponent<Fabricator>() == null)
					{
						ItemContainer itemContainer = linkedItem.GetComponent<ItemContainer>();
						if (itemContainer != null)
						{
							this.outputContainer.Inventory.AllItemsMod.ForEach(delegate(Item containedItem)
							{
								itemContainer.Inventory.TryPutItem(containedItem, null, null, true, false, true);
							});
						}
					}
				}
			}
		}

		// Token: 0x060040BA RID: 16570 RVA: 0x0019E6A4 File Offset: 0x0019C8A4
		private void ApplyDeconstructionStatusEffects(Item targetItem, ActionType type, float deltaTime)
		{
			Deconstructor.<>c__DisplayClass31_0 CS$<>8__locals1 = new Deconstructor.<>c__DisplayClass31_0();
			CS$<>8__locals1.<>4__this = this;
			LinkedControllerCharacterComponent linkedCharacterComponent = targetItem.GetComponent<LinkedControllerCharacterComponent>();
			CS$<>8__locals1.character = null;
			if (linkedCharacterComponent != null)
			{
				Character character = linkedCharacterComponent.Character;
				if (character != null && !character.Removed)
				{
					CS$<>8__locals1.character = linkedCharacterComponent.Character;
				}
			}
			Character character2 = CS$<>8__locals1.character;
			Limb limb = (character2 != null) ? character2.AnimController.Limbs.GetRandomUnsynced<Limb>() : null;
			if (this.user != null)
			{
				this.item.GetStatusEffectsOfType(type).ForEach(delegate(StatusEffect statusEffect)
				{
					statusEffect.SetUser(CS$<>8__locals1.<>4__this.user);
				});
				targetItem.GetStatusEffectsOfType(type).ForEach(delegate(StatusEffect statusEffect)
				{
					statusEffect.SetUser(CS$<>8__locals1.<>4__this.user);
				});
			}
			this.item.ApplyStatusEffects(type, deltaTime, CS$<>8__locals1.character, limb, targetItem, false, null);
			targetItem.ApplyStatusEffects(type, deltaTime, CS$<>8__locals1.character, limb, null, false, null);
			if (CS$<>8__locals1.character != null)
			{
				if (type == ActionType.OnDeconstructed)
				{
					CS$<>8__locals1.<ApplyDeconstructionStatusEffects>g__MoveItemsFromCharacterToOutput|3();
				}
				CS$<>8__locals1.character.ApplyStatusEffects(type, deltaTime);
				if (type == ActionType.OnDeconstructed)
				{
					CoroutineManager.Invoke(delegate
					{
						if (CS$<>8__locals1.character.Removed)
						{
							return;
						}
						base.<ApplyDeconstructionStatusEffects>g__MoveItemsFromCharacterToOutput|3();
						CS$<>8__locals1.character.Kill(CauseOfDeathType.Unknown, null, false, true);
						EntitySpawner spawner = Entity.Spawner;
						if (spawner == null)
						{
							return;
						}
						spawner.AddEntityToRemoveQueue(CS$<>8__locals1.character);
					}, 0.1f);
				}
			}
		}

		// Token: 0x060040BB RID: 16571 RVA: 0x0019E7C0 File Offset: 0x0019C9C0
		private void MoveInputQueue()
		{
			for (int i = this.inputContainer.Inventory.Capacity - 2; i >= 0; i--)
			{
				Item item;
				do
				{
					item = this.inputContainer.Inventory.GetItemAt(i);
				}
				while (item != null && this.inputContainer.Inventory.CanBePutInSlot(item, i + 1, false) && this.inputContainer.Inventory.TryPutItem(item, i + 1, false, false, null, true, false, true));
			}
		}

		// Token: 0x060040BC RID: 16572 RVA: 0x0019E832 File Offset: 0x0019CA32
		[return: TupleElementNames(new string[]
		{
			"item",
			"output"
		})]
		private IEnumerable<ValueTuple<Item, DeconstructItem>> GetAvailableOutputs(bool checkRequiredOtherItems = true)
		{
			Deconstructor.<GetAvailableOutputs>d__33 <GetAvailableOutputs>d__ = new Deconstructor.<GetAvailableOutputs>d__33(-2);
			<GetAvailableOutputs>d__.<>4__this = this;
			<GetAvailableOutputs>d__.<>3__checkRequiredOtherItems = checkRequiredOtherItems;
			return <GetAvailableOutputs>d__;
		}

		// Token: 0x060040BD RID: 16573 RVA: 0x0019E84C File Offset: 0x0019CA4C
		public void SetActive(bool active, Character user = null, bool createNetworkEvent = false)
		{
			this.PutItemsToLinkedContainer();
			this.user = user;
			this.RelocateOutputToMainSub = false;
			if (this.inputContainer.Inventory.IsEmpty())
			{
				active = false;
			}
			this.IsActive = active;
			this.userDeconstructorSpeedMultiplier = ((user != null) ? (1f + user.GetStatValue(StatTypes.DeconstructorSpeedMultiplier, true)) : 1f);
			if (user != null)
			{
				GameServer.Log(GameServer.CharacterLogName(user) + (this.IsActive ? " activated " : " deactivated ") + this.item.Name, ServerLog.MessageType.ItemInteraction);
			}
			if (createNetworkEvent)
			{
				this.item.CreateServerEvent<Deconstructor>(this);
			}
			if (!this.IsActive)
			{
				this.progressTimer = 0f;
				this.progressState = 0f;
			}
			this.inputContainer.Inventory.Locked = this.IsActive;
		}

		// Token: 0x04001EE5 RID: 7909
		private float progressTimer;

		// Token: 0x04001EE6 RID: 7910
		private float progressState;

		// Token: 0x04001EE7 RID: 7911
		private Character user;

		// Token: 0x04001EE8 RID: 7912
		private float userDeconstructorSpeedMultiplier = 1f;

		// Token: 0x04001EE9 RID: 7913
		private const float TinkeringSpeedIncrease = 2.5f;

		// Token: 0x04001EEA RID: 7914
		private ItemContainer inputContainer;

		// Token: 0x04001EEB RID: 7915
		private ItemContainer outputContainer;

		// Token: 0x04001EEC RID: 7916
		public bool RelocateOutputToMainSub;
	}
}
