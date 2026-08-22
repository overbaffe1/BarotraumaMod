using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200049D RID: 1181
	internal class Fabricator : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x060040D7 RID: 16599 RVA: 0x0019F238 File Offset: 0x0019D438
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			uint recipeHash = msg.ReadUInt32();
			int amountToFabricate = msg.ReadRangedInteger(1, 99);
			this.item.CreateServerEvent<Fabricator>(this);
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			this.AmountToFabricate = amountToFabricate;
			if (recipeHash == 0U)
			{
				this.CancelFabricating(c.Character);
				return;
			}
			if (this.fabricatedItem != null && this.fabricatedItem.RecipeHash == recipeHash)
			{
				return;
			}
			if (recipeHash == 0U)
			{
				return;
			}
			this.amountRemaining = this.AmountToFabricate;
			this.StartFabricating(this.fabricationRecipes[recipeHash], c.Character, true);
		}

		// Token: 0x060040D8 RID: 16600 RVA: 0x0019F2C8 File Offset: 0x0019D4C8
		public override ItemComponent.IEventData ServerGetEventData()
		{
			return new Fabricator.EventData(this.serverEventId, this.State);
		}

		// Token: 0x060040D9 RID: 16601 RVA: 0x0019F2E0 File Offset: 0x0019D4E0
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			Fabricator.EventData eventData;
			return base.TryExtractEventData<Fabricator.EventData>(data, out eventData);
		}

		// Token: 0x060040DA RID: 16602 RVA: 0x0019F2F8 File Offset: 0x0019D4F8
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Fabricator.EventData componentData = base.ExtractEventData<Fabricator.EventData>(extraData);
			msg.WriteByte((byte)componentData.State);
			msg.WriteRangedInteger(this.AmountToFabricate, 0, 99);
			msg.WriteRangedInteger(this.amountRemaining, 0, 99);
			msg.WriteSingle(this.timeUntilReady);
			FabricationRecipe fabricationRecipe = this.fabricatedItem;
			uint recipeHash = (fabricationRecipe != null) ? fabricationRecipe.RecipeHash : 0U;
			msg.WriteUInt32(recipeHash);
			ushort userId = (this.fabricatedItem == null || this.user == null) ? 0 : this.user.ID;
			msg.WriteUInt16(userId);
			IEnumerable<KeyValuePair<uint, int>> reachedLimits = from kvp in this.fabricationLimits
			where kvp.Value <= 0
			select kvp;
			msg.WriteUInt16((ushort)reachedLimits.Count<KeyValuePair<uint, int>>());
			foreach (KeyValuePair<uint, int> kvp2 in reachedLimits)
			{
				msg.WriteUInt32(kvp2.Key);
			}
		}

		// Token: 0x17001139 RID: 4409
		// (get) Token: 0x060040DB RID: 16603 RVA: 0x0019F404 File Offset: 0x0019D604
		// (set) Token: 0x060040DC RID: 16604 RVA: 0x0019F40C File Offset: 0x0019D60C
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 1000f)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float FabricationSpeed { get; set; }

		// Token: 0x1700113A RID: 4410
		// (get) Token: 0x060040DD RID: 16605 RVA: 0x0019F415 File Offset: 0x0019D615
		// (set) Token: 0x060040DE RID: 16606 RVA: 0x0019F41D File Offset: 0x0019D61D
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillRequirementMultiplier { get; set; }

		// Token: 0x1700113B RID: 4411
		// (get) Token: 0x060040DF RID: 16607 RVA: 0x0019F426 File Offset: 0x0019D626
		// (set) Token: 0x060040E0 RID: 16608 RVA: 0x0019F42E File Offset: 0x0019D62E
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int AmountToFabricate
		{
			get
			{
				return this.amountToFabricate;
			}
			set
			{
				this.amountToFabricate = MathHelper.Clamp(value, 1, 99);
			}
		}

		// Token: 0x1700113C RID: 4412
		// (get) Token: 0x060040E1 RID: 16609 RVA: 0x0019F43F File Offset: 0x0019D63F
		// (set) Token: 0x060040E2 RID: 16610 RVA: 0x0019F447 File Offset: 0x0019D647
		private Fabricator.FabricatorState State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state == value)
				{
					return;
				}
				this.state = value;
				this.serverEventId += 1UL;
				this.item.CreateServerEvent<Fabricator>(this);
			}
		}

		// Token: 0x1700113D RID: 4413
		// (get) Token: 0x060040E3 RID: 16611 RVA: 0x0019F475 File Offset: 0x0019D675
		public ItemContainer InputContainer
		{
			get
			{
				return this.inputContainer;
			}
		}

		// Token: 0x1700113E RID: 4414
		// (get) Token: 0x060040E4 RID: 16612 RVA: 0x0019F47D File Offset: 0x0019D67D
		public ItemContainer OutputContainer
		{
			get
			{
				return this.outputContainer;
			}
		}

		// Token: 0x060040E5 RID: 16613 RVA: 0x0019F488 File Offset: 0x0019D688
		public Fabricator(Item item, ContentXElement element) : base(item, element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("fabricableitem", StringComparison.OrdinalIgnoreCase))
				{
					DebugConsole.ThrowError("Error in item " + item.Name + "! Fabrication recipes should be defined in the craftable item's xml, not in the fabricator.", null, element.ContentPackage, false, false);
					break;
				}
			}
			Dictionary<uint, FabricationRecipe> fabricationRecipes = new Dictionary<uint, FabricationRecipe>();
			Func<Identifier, bool> <>9__0;
			foreach (ItemPrefab itemPrefab in ItemPrefab.Prefabs)
			{
				foreach (FabricationRecipe recipe in itemPrefab.FabricationRecipes.Values)
				{
					if (recipe.SuitableFabricatorIdentifiers.Length > 0)
					{
						ImmutableArray<Identifier> suitableFabricatorIdentifiers = recipe.SuitableFabricatorIdentifiers;
						Func<Identifier, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((Identifier i) => item.Prefab.Identifier == i || item.HasTag(i)));
						}
						if (!suitableFabricatorIdentifiers.Any(predicate))
						{
							continue;
						}
					}
					ContentPackage packageToLog = itemPrefab.GetParentModPackageOrThisPackage();
					bool recipeInvalid = false;
					foreach (FabricationRecipe.RequiredItem requiredItem in recipe.RequiredItems)
					{
						if (requiredItem.ItemPrefabs.None(null))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Error in the fabrication recipe for \"");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemPrefab.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\". Could not find the ingredient \"");
							defaultInterpolatedStringHandler.AppendFormatted<FabricationRecipe.RequiredItem>(requiredItem);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, packageToLog, false, false);
							recipeInvalid = true;
						}
					}
					if (!recipeInvalid)
					{
						FabricationRecipe duplicateRecipe;
						if (fabricationRecipes.TryGetValue(recipe.RecipeHash, out duplicateRecipe))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(63, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Error in the fabrication recipe for \"");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(itemPrefab.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\". Duplicate recipe in \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(duplicateRecipe.TargetItem.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, packageToLog, false, false);
						}
						else
						{
							fabricationRecipes.Add(recipe.RecipeHash, recipe);
							if (recipe.FabricationLimitMax >= 0)
							{
								this.fabricationLimits.Add(recipe.RecipeHash, Rand.Range(recipe.FabricationLimitMin, recipe.FabricationLimitMax + 1, Rand.RandSync.Unsynced));
							}
						}
					}
				}
			}
			this.fabricationRecipes = fabricationRecipes.ToImmutableDictionary<uint, FabricationRecipe>();
			this.state = Fabricator.FabricatorState.Stopped;
		}

		// Token: 0x060040E6 RID: 16614 RVA: 0x0019F7B8 File Offset: 0x0019D9B8
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<ItemContainer> containers = this.item.GetComponents<ItemContainer>().ToList<ItemContainer>();
			if (containers.Count < 2)
			{
				DebugConsole.ThrowError("Error in item \"" + this.item.Name + "\": Fabricators must have two ItemContainer components!", null, null, false, false);
				return;
			}
			this.inputContainer = containers[0];
			this.outputContainer = containers[1];
			foreach (FabricationRecipe recipe in this.fabricationRecipes.Values)
			{
				if (recipe.RequiredItems.Length > this.inputContainer.Capacity)
				{
					DebugConsole.ThrowErrorLocalized("Error in item \"" + this.item.Name + "\": There's not enough room in the input inventory for the ingredients of \"" + recipe.TargetItem.Name + "\"!", null, null, false, false);
				}
			}
		}

		// Token: 0x060040E7 RID: 16615 RVA: 0x0019F8C0 File Offset: 0x0019DAC0
		public override bool Select(Character character)
		{
			return base.Select(character);
		}

		// Token: 0x060040E8 RID: 16616 RVA: 0x0019F8C9 File Offset: 0x0019DAC9
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x060040E9 RID: 16617 RVA: 0x0019F8D0 File Offset: 0x0019DAD0
		public void RemoveFabricationRecipes(IEnumerable<Identifier> allowedIdentifiers)
		{
			this.fabricationRecipes = (from kvp in this.fabricationRecipes
			where allowedIdentifiers.Contains(kvp.Value.TargetItemPrefabIdentifier)
			select kvp).ToImmutableDictionary<uint, FabricationRecipe>();
		}

		// Token: 0x060040EA RID: 16618 RVA: 0x0019F90C File Offset: 0x0019DB0C
		private void StartFabricating(FabricationRecipe selectedItem, Character user, bool addToServerLog = true)
		{
			if (selectedItem == null)
			{
				return;
			}
			if (!this.outputContainer.Inventory.CanProbablyBePut(selectedItem.TargetItem, new float?(selectedItem.OutCondition * selectedItem.TargetItem.Health), null))
			{
				return;
			}
			this.IsActive = true;
			this.user = user;
			this.fabricatedItem = selectedItem;
			this.RefreshAvailableIngredients();
			if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
			{
				this.MoveIngredientsToInputContainer(selectedItem);
			}
			this.requiredTime = this.GetRequiredTime(this.fabricatedItem, user);
			this.timeUntilReady = this.requiredTime;
			this.inputContainer.Inventory.Locked = true;
			this.outputContainer.Inventory.Locked = true;
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || networkMember.IsServer)
			{
				this.State = Fabricator.FabricatorState.Active;
			}
			if (user != null && addToServerLog && selectedItem.RequiredMoney == 0)
			{
				if (selectedItem.RequiredMoney > 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 4);
					defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(user));
					defaultInterpolatedStringHandler.AppendLiteral(" bought ");
					defaultInterpolatedStringHandler.AppendFormatted(selectedItem.DisplayName.Value);
					defaultInterpolatedStringHandler.AppendLiteral(" for ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(selectedItem.RequiredMoney);
					defaultInterpolatedStringHandler.AppendLiteral(" mk from ");
					defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
					GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Money);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 3);
				defaultInterpolatedStringHandler2.AppendFormatted(GameServer.CharacterLogName(user));
				defaultInterpolatedStringHandler2.AppendLiteral(" started fabricating ");
				defaultInterpolatedStringHandler2.AppendFormatted(selectedItem.DisplayName.Value);
				defaultInterpolatedStringHandler2.AppendLiteral(" in ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.item.Name);
				GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
			}
		}

		// Token: 0x060040EB RID: 16619 RVA: 0x0019FAE4 File Offset: 0x0019DCE4
		private void CancelFabricating(Character user = null)
		{
			this.IsActive = false;
			this.user = null;
			this.currPowerConsumption = 0f;
			this.progressState = 0f;
			this.timeUntilReady = 0f;
			this.inputContainer.Inventory.Locked = false;
			this.outputContainer.Inventory.Locked = false;
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || networkMember.IsServer)
			{
				this.State = Fabricator.FabricatorState.Stopped;
			}
			if (this.fabricatedItem == null)
			{
				return;
			}
			if (user != null)
			{
				GameServer.Log(string.Concat(new string[]
				{
					GameServer.CharacterLogName(user),
					" cancelled the fabrication of ",
					this.fabricatedItem.DisplayName.Value,
					" in ",
					this.item.Name
				}), ServerLog.MessageType.ItemInteraction);
			}
			this.fabricatedItem = null;
		}

		// Token: 0x060040EC RID: 16620 RVA: 0x0019FBBC File Offset: 0x0019DDBC
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.refreshIngredientsTimer <= 0f)
			{
				this.RefreshAvailableIngredients();
				this.refreshIngredientsTimer = 1f;
			}
			this.refreshIngredientsTimer -= deltaTime;
			NetworkMember networkMember = GameMain.NetworkMember;
			bool isClient = networkMember != null && networkMember.IsClient;
			if (!isClient && (this.fabricatedItem == null || !this.CanBeFabricated(this.fabricatedItem, this.availableIngredients, this.user)))
			{
				this.CancelFabricating(null);
				return;
			}
			this.progressState = ((this.fabricatedItem == null) ? 0f : ((this.requiredTime - this.timeUntilReady) / this.requiredTime));
			if (isClient)
			{
				this.hasPower = (this.State != Fabricator.FabricatorState.Paused);
				if (!this.hasPower)
				{
					return;
				}
			}
			else
			{
				this.hasPower = this.HasPower;
				if (!this.hasPower)
				{
					this.State = Fabricator.FabricatorState.Paused;
					return;
				}
				this.State = Fabricator.FabricatorState.Active;
			}
			float tinkeringStrength = 0f;
			Repairable repairable = this.item.GetComponent<Repairable>();
			if (repairable != null)
			{
				repairable.LastActiveTime = (float)Timing.TotalTime + 10f;
				if (repairable.IsTinkering)
				{
					tinkeringStrength = repairable.TinkeringStrength;
				}
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			float fabricationSpeedIncrease = 1f + tinkeringStrength * 2.5f;
			this.timeUntilReady -= deltaTime * fabricationSpeedIncrease * Math.Min((this.powerConsumption <= 0f) ? 1f : base.Voltage, 2f);
			if (this.timeUntilReady <= 0f)
			{
				this.Fabricate();
			}
		}

		// Token: 0x060040ED RID: 16621 RVA: 0x0019FD45 File Offset: 0x0019DF45
		private Client GetUsingClient()
		{
			return GameMain.Server.ConnectedClients.Find((Client c) => c.Character == this.user);
		}

		// Token: 0x060040EE RID: 16622 RVA: 0x0019FD64 File Offset: 0x0019DF64
		private void Fabricate()
		{
			Fabricator.<>c__DisplayClass59_0 CS$<>8__locals1 = new Fabricator.<>c__DisplayClass59_0();
			CS$<>8__locals1.<>4__this = this;
			this.RefreshAvailableIngredients();
			if (this.fabricatedItem == null || !this.CanBeFabricated(this.fabricatedItem, this.availableIngredients, this.user))
			{
				this.CancelFabricating(null);
				return;
			}
			if (this.fabricatedItem.RequiredMoney > 0)
			{
				if (this.user == null)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					Client client = this.GetUsingClient();
					if (client != null)
					{
						mpCampaign.TryPurchase(client, this.fabricatedItem.RequiredMoney);
					}
					else
					{
						this.user.Wallet.Deduct(this.fabricatedItem.RequiredMoney);
					}
				}
				else
				{
					GameSession gameSession2 = GameMain.GameSession;
					CampaignMode campaign = ((gameSession2 != null) ? gameSession2.GameMode : null) as CampaignMode;
					if (campaign != null)
					{
						campaign.Bank.Deduct(this.fabricatedItem.RequiredMoney);
					}
				}
			}
			CS$<>8__locals1.ingredientsStolen = false;
			CS$<>8__locals1.ingredientsAllowStealing = true;
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				List<Item> chosenIngredients = new List<Item>();
				IEnumerable<Item> suitableIngredients = this.GetSortedSuitableIngredients();
				foreach (FabricationRecipe.RequiredItem requiredItem in this.fabricatedItem.RequiredItems)
				{
					for (int i = 0; i < requiredItem.Amount; i++)
					{
						foreach (Item suitableIngredient in suitableIngredients)
						{
							if (requiredItem.MatchesItem(suitableIngredient) && !chosenIngredients.Contains(suitableIngredient))
							{
								CS$<>8__locals1.ingredientsStolen |= suitableIngredient.StolenDuringRound;
								if (!suitableIngredient.AllowStealing)
								{
									CS$<>8__locals1.ingredientsAllowStealing = false;
								}
								if (requiredItem.UseCondition && suitableIngredient.ConditionPercentage - requiredItem.MinCondition * 100f > 0f)
								{
									suitableIngredient.Condition -= suitableIngredient.Prefab.Health * requiredItem.MinCondition;
									break;
								}
								if (suitableIngredient.OwnInventory != null)
								{
									foreach (Item containedItem in suitableIngredient.OwnInventory.AllItemsMod)
									{
										ItemContainer component = suitableIngredient.GetComponent<ItemContainer>();
										if (component != null && component.RemoveContainedItemsOnDeconstruct)
										{
											Entity.Spawner.AddItemToRemoveQueue(containedItem);
										}
										else
										{
											containedItem.Drop(null, true, true);
										}
									}
								}
								chosenIngredients.Add(suitableIngredient);
								break;
							}
						}
					}
				}
				Fabricator.AbilityFabricationItemIngredients fabricationIngredients = new Fabricator.AbilityFabricationItemIngredients(chosenIngredients);
				Character character2 = this.user;
				if (character2 != null)
				{
					character2.CheckTalents(AbilityEffectType.OnItemFabricatedIngredients, fabricationIngredients);
				}
				foreach (Item availableItem in fabricationIngredients.Items)
				{
					Entity.Spawner.AddItemToRemoveQueue(availableItem);
					this.inputContainer.Inventory.RemoveItem(availableItem);
				}
				int amountFittingContainer = this.outputContainer.Inventory.HowManyCanBePut(this.fabricatedItem.TargetItem, new float?(this.fabricatedItem.OutCondition * this.fabricatedItem.TargetItem.Health));
				Fabricator.AbilityFabricationItemAmount fabricationitemAmount = new Fabricator.AbilityFabricationItemAmount(this.fabricatedItem.TargetItem, (float)this.fabricatedItem.Amount);
				int quality = 0;
				if (this.fabricatedItem.Quality != null)
				{
					quality = this.fabricatedItem.Quality.Value;
				}
				else
				{
					Character character3 = this.user;
					if (((character3 != null) ? character3.Info : null) != null)
					{
						foreach (Character character in Character.GetFriendlyCrew(this.user))
						{
							character.CheckTalents(AbilityEffectType.OnAllyItemFabricatedAmount, fabricationitemAmount);
						}
						this.user.CheckTalents(AbilityEffectType.OnItemFabricatedAmount, fabricationitemAmount);
						quality = ((this.fabricatedItem.TargetItem.MaxStackSize > 1) ? Fabricator.GetFabricatedItemQuality(this.fabricatedItem, this.user).Quality : Fabricator.GetFabricatedItemQuality(this.fabricatedItem, this.user).RollQuality());
					}
				}
				int amount = (int)fabricationitemAmount.Value;
				if (this.fabricationLimits.ContainsKey(this.fabricatedItem.RecipeHash))
				{
					if (amount > this.fabricationLimits[this.fabricatedItem.RecipeHash])
					{
						amount = this.fabricationLimits[this.fabricatedItem.RecipeHash];
						this.fabricationLimits[this.fabricatedItem.RecipeHash] = 0;
					}
					else
					{
						Dictionary<uint, int> dictionary = this.fabricationLimits;
						uint recipeHash = this.fabricatedItem.RecipeHash;
						dictionary[recipeHash] -= amount;
					}
				}
				Character tempUser = this.user;
				for (int j = 0; j < amount; j++)
				{
					float outCondition = this.fabricatedItem.OutCondition;
					if (this.fabricatedItem.TargetItem.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
					{
						string str = "ItemFabricated:";
						GameSession gameSession3 = GameMain.GameSession;
						string text;
						if (gameSession3 == null)
						{
							text = null;
						}
						else
						{
							GameMode gameMode = gameSession3.GameMode;
							text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
						}
						GameAnalyticsManager.AddDesignEvent(str + (text ?? "none") + ":" + this.fabricatedItem.TargetItem.Identifier.ToString());
					}
					InvSlotType invSlot = this.fabricatedItem.MoveToSlot;
					if (j < amountFittingContainer)
					{
						Entity.Spawner.AddItemToSpawnQueue(this.fabricatedItem.TargetItem, this.outputContainer.Inventory, new float?(this.fabricatedItem.TargetItem.Health * outCondition), new int?(quality), delegate(Item spawnedItem)
						{
							CS$<>8__locals1.<Fabricate>g__onItemSpawned|0(spawnedItem, tempUser, invSlot);
							spawnedItem.Quality = quality;
							spawnedItem.StolenDuringRound = CS$<>8__locals1.ingredientsStolen;
							spawnedItem.AllowStealing = CS$<>8__locals1.ingredientsAllowStealing;
							spawnedItem.Condition = spawnedItem.MaxCondition * outCondition;
						}, true, false, InvSlotType.None);
					}
					else
					{
						Entity.Spawner.AddItemToSpawnQueue(this.fabricatedItem.TargetItem, this.item.Position, this.item.Submarine, new float?(this.fabricatedItem.TargetItem.Health * outCondition), new int?(quality), delegate(Item spawnedItem)
						{
							CS$<>8__locals1.<Fabricate>g__onItemSpawned|0(spawnedItem, tempUser, invSlot);
							spawnedItem.Quality = quality;
							spawnedItem.StolenDuringRound = CS$<>8__locals1.ingredientsStolen;
							spawnedItem.AllowStealing = CS$<>8__locals1.ingredientsAllowStealing;
							spawnedItem.Condition = spawnedItem.MaxCondition * outCondition;
						});
					}
				}
				Character character4 = this.user;
				if (((character4 != null) ? character4.Info : null) != null && !this.user.Removed)
				{
					foreach (Skill skill in this.fabricatedItem.RequiredSkills)
					{
						float addedSkill = skill.Level * SkillSettings.Current.SkillIncreasePerFabricatorRequiredSkill;
						Fabricator.AbilityFabricatorSkillGain addedSkillValue = new Fabricator.AbilityFabricatorSkillGain(skill.Identifier, addedSkill);
						this.user.CheckTalents(AbilityEffectType.OnItemFabricationSkillGain, addedSkillValue);
						this.user.Info.ApplySkillGain(skill.Identifier, addedSkillValue.Value, false, 2f, false);
					}
				}
				FabricationRecipe prevFabricatedItem = this.fabricatedItem;
				Character prevUser = this.user;
				this.CancelFabricating(null);
				this.amountRemaining--;
				if (this.amountRemaining > 0 && this.CanBeFabricated(prevFabricatedItem, this.availableIngredients, prevUser))
				{
					this.StartFabricating(prevFabricatedItem, prevUser, false);
				}
			}
		}

		// Token: 0x060040EF RID: 16623 RVA: 0x001A056C File Offset: 0x0019E76C
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			this.currPowerConsumption = base.PowerConsumption;
			Repairable component = this.item.GetComponent<Repairable>();
			if (component != null)
			{
				component.AdjustPowerConsumption(ref this.currPowerConsumption);
			}
			return this.currPowerConsumption;
		}

		// Token: 0x060040F0 RID: 16624 RVA: 0x001A05BE File Offset: 0x0019E7BE
		public static float CalculateBonusRollPercentage(float skillLevel, float target)
		{
			return Math.Clamp((skillLevel - target) / (100f - target) * 100f, 0f, 100f);
		}

		// Token: 0x060040F1 RID: 16625 RVA: 0x001A05E0 File Offset: 0x0019E7E0
		private static Fabricator.QualityResult GetFabricatedItemQuality(FabricationRecipe fabricatedItem, Character user)
		{
			if (((user != null) ? user.Info : null) == null)
			{
				return Fabricator.QualityResult.Empty;
			}
			ContentXElement childElement = fabricatedItem.TargetItem.ConfigElement.GetChildElement("Quality");
			ContentXElement contentXElement = null;
			if (childElement == contentXElement)
			{
				return Fabricator.QualityResult.Empty;
			}
			float floatQuality = 0f;
			floatQuality += user.GetStatValue(StatTypes.IncreaseFabricationQuality, false);
			foreach (Identifier tag in fabricatedItem.TargetItem.Tags)
			{
				floatQuality += user.Info.GetSavedStatValue(StatTypes.IncreaseFabricationQuality, tag);
			}
			if (!fabricatedItem.TargetItem.Tags.Contains(fabricatedItem.TargetItem.Identifier))
			{
				floatQuality += user.Info.GetSavedStatValue(StatTypes.IncreaseFabricationQuality, fabricatedItem.TargetItem.Identifier);
			}
			int quality = (int)floatQuality;
			Option.UnspecifiedNone none = Option.None;
			Option<float> plusOne = none;
			none = Option.None;
			Option<float> plusTwo = none;
			foreach (Skill skill in fabricatedItem.RequiredSkills)
			{
				float skillLevel = user.GetSkillLevel(skill.Identifier);
				if (skillLevel < 50f)
				{
					break;
				}
				float bonusChance = Fabricator.CalculateBonusRollPercentage(skillLevel, MathHelper.Lerp(skill.Level, 100f, 0.2f));
				plusOne = Fabricator.<GetFabricatedItemQuality>g__OverrideChanceIfLess|69_4(plusOne, bonusChance);
				if (skillLevel < 75f)
				{
					break;
				}
				float bonusChance2 = Fabricator.CalculateBonusRollPercentage(skillLevel, MathHelper.Lerp(skill.Level, 125f, 0.4f));
				plusTwo = Fabricator.<GetFabricatedItemQuality>g__OverrideChanceIfLess|69_4(plusTwo, bonusChance2);
			}
			bool hasRandomQuality = fabricatedItem.TargetItem.MaxStackSize <= 1;
			float PlusOnePercentage = plusOne.Match((float f) => f, () => 0f);
			float PlusTwoPercentage = plusTwo.Match((float f) => f, () => 0f);
			if (!hasRandomQuality && PlusOnePercentage > 0f)
			{
				quality++;
				if (PlusTwoPercentage > 0f)
				{
					quality++;
				}
			}
			return new Fabricator.QualityResult(quality, hasRandomQuality, PlusOnePercentage, PlusTwoPercentage);
		}

		// Token: 0x060040F2 RID: 16626 RVA: 0x001A0854 File Offset: 0x0019EA54
		private static bool AnyOneHasRecipeForItem(Character user, ItemPrefab item)
		{
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			CharacterType mustHaveRecipe = (gameMode != null && gameMode.IsSinglePlayer) ? CharacterType.Both : CharacterType.Bot;
			return (user != null && user.HasRecipeForItem(item.Identifier)) || GameSession.GetSessionCrewCharacters(mustHaveRecipe).Any((Character c) => c.HasRecipeForItem(item.Identifier));
		}

		// Token: 0x060040F3 RID: 16627 RVA: 0x001A08BF File Offset: 0x0019EABF
		public bool MissingRequiredRecipe(FabricationRecipe fabricableItem, Character character)
		{
			if (fabricableItem.RequiresRecipe)
			{
				if (character == null)
				{
					return false;
				}
				if (!Fabricator.AnyOneHasRecipeForItem(character, fabricableItem.TargetItem))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060040F4 RID: 16628 RVA: 0x001A08E0 File Offset: 0x0019EAE0
		private bool CanBeFabricated(FabricationRecipe fabricableItem, IReadOnlyDictionary<Identifier, List<Item>> availableIngredients, Character character)
		{
			if (fabricableItem == null)
			{
				return false;
			}
			if (this.MissingRequiredRecipe(fabricableItem, character))
			{
				return false;
			}
			if (fabricableItem.HideForNonTraitors && (character == null || !character.IsTraitor))
			{
				return false;
			}
			if (fabricableItem.RequiredMoney > 0)
			{
				GameSession gameSession = GameMain.GameSession;
				GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
				MultiPlayerCampaign mpCampaign = gameMode as MultiPlayerCampaign;
				if (mpCampaign == null)
				{
					CampaignMode campaign = gameMode as CampaignMode;
					if (campaign == null)
					{
						return false;
					}
					if (campaign.Bank.Balance < fabricableItem.RequiredMoney)
					{
						return false;
					}
				}
				else if (!mpCampaign.CanAfford(fabricableItem.RequiredMoney, this.GetUsingClient()))
				{
					return false;
				}
			}
			int amount;
			if (this.fabricationLimits.TryGetValue(fabricableItem.RecipeHash, out amount) && amount <= 0)
			{
				return false;
			}
			this.usedIngredients.Clear();
			this.ingredientFlexibilityCache.Clear();
			using (IEnumerator<ItemPrefab> enumerator = fabricableItem.RequiredItems.SelectMany((FabricationRecipe.RequiredItem r) => r.ItemPrefabs).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemPrefab prefab = enumerator.Current;
					this.ingredientFlexibilityCache[prefab] = fabricableItem.RequiredItems.Count((FabricationRecipe.RequiredItem r) => r.ItemPrefabs.Contains(prefab));
				}
			}
			return (from r in fabricableItem.RequiredItems
			orderby r.ItemPrefabs.Count<ItemPrefab>()
			select r).ThenByDescending((FabricationRecipe.RequiredItem requiredItem) => requiredItem.Amount).All(delegate(FabricationRecipe.RequiredItem requiredItem)
			{
				int availableItemsAmount = 0;
				foreach (ItemPrefab requiredPrefab in requiredItem.ItemPrefabs.OrderBy(new Func<ItemPrefab, int>(base.<CanBeFabricated>g__GetItemFlexibility|4)).ThenByDescending(new Func<ItemPrefab, int>(base.<CanBeFabricated>g__GetAvailableItemsCount|3)))
				{
					List<Item> availableItems;
					if (availableIngredients.TryGetValue(requiredPrefab.Identifier, out availableItems))
					{
						foreach (Item availableItem in availableItems)
						{
							if (!this.usedIngredients.Contains(availableItem))
							{
								if (requiredItem.IsConditionSuitable(availableItem.ConditionPercentage))
								{
									this.usedIngredients.Add(availableItem);
									availableItemsAmount++;
								}
								if (availableItemsAmount >= requiredItem.Amount)
								{
									return true;
								}
							}
						}
					}
				}
				return false;
			});
		}

		// Token: 0x060040F5 RID: 16629 RVA: 0x001A0AC0 File Offset: 0x0019ECC0
		private float GetRequiredTime(FabricationRecipe fabricableItem, Character user)
		{
			float degreeOfSuccess = this.FabricationDegreeOfSuccess(user, fabricableItem.RequiredSkills);
			float t = (degreeOfSuccess < 0.5f) ? (degreeOfSuccess * degreeOfSuccess) : (degreeOfSuccess * 2f);
			float time = fabricableItem.RequiredTime / this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.FabricationSpeed, this.FabricationSpeed) / MathHelper.Clamp(t, 0.01f, 2f);
			CharacterInfo info = (user != null) ? user.Info : null;
			if (info != null)
			{
				ItemPrefab it = fabricableItem.TargetItem;
				if (it != null)
				{
					time /= 1f + it.Tags.Sum((Identifier tag) => info.GetSavedStatValue(StatTypes.FabricationSpeed, tag));
				}
			}
			return time;
		}

		// Token: 0x060040F6 RID: 16630 RVA: 0x001A0B70 File Offset: 0x0019ED70
		public float FabricationDegreeOfSuccess(Character character, ImmutableArray<Skill> skills)
		{
			if (skills.Length == 0)
			{
				return 0.5f;
			}
			if (character == null)
			{
				return 0f;
			}
			float minDegreeOfSuccess = 1f;
			foreach (Skill skill in skills)
			{
				float characterLevel = character.GetSkillLevel(skill.Identifier);
				minDegreeOfSuccess = Math.Min(minDegreeOfSuccess, (characterLevel - skill.Level * this.SkillRequirementMultiplier + 100f) / 2f / 100f);
			}
			return minDegreeOfSuccess;
		}

		// Token: 0x060040F7 RID: 16631 RVA: 0x001A0BEC File Offset: 0x0019EDEC
		public override float GetSkillMultiplier()
		{
			return this.SkillRequirementMultiplier;
		}

		// Token: 0x060040F8 RID: 16632 RVA: 0x001A0BF4 File Offset: 0x0019EDF4
		private void RefreshAvailableIngredients()
		{
			Character user = this.user;
			this.linkedInventories.Clear();
			List<Item> itemList = new List<Item>();
			itemList.AddRange(this.inputContainer.Inventory.AllItems);
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Item linkedItem = linkedTo as Item;
				if (linkedItem != null)
				{
					ItemContainer itemContainer = linkedItem.GetComponent<ItemContainer>();
					if (itemContainer != null && (user == null || itemContainer.HasRequiredItems(user, false, null)))
					{
						Deconstructor deconstructor = linkedItem.GetComponent<Deconstructor>();
						if (deconstructor != null)
						{
							itemContainer = deconstructor.OutputContainer;
						}
						this.linkedInventories.Add(itemContainer.Inventory);
						itemList.AddRange(itemContainer.Inventory.AllItems);
					}
				}
			}
			for (int i = 0; i < itemList.Count; i++)
			{
				ItemContainer container = itemList[i].GetComponent<ItemContainer>();
				if (container != null)
				{
					itemList.AddRange(container.Inventory.AllItems);
				}
			}
			if (((user != null) ? user.Inventory : null) != null && user.SelectedItem == this.item)
			{
				itemList.AddRange(user.Inventory.AllItems);
				this.linkedInventories.Add(user.Inventory);
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.SelectedItem != null && c.Inventory != null && this.linkedInventories.Contains(c.SelectedItem.OwnInventory) && !this.linkedInventories.Contains(c.Inventory))
				{
					itemList.AddRange(c.Inventory.AllItems);
					this.linkedInventories.Add(c.Inventory);
				}
			}
			this.availableIngredients.Clear();
			foreach (Item item in itemList)
			{
				Identifier itemIdentifier = item.Prefab.Identifier;
				if (!this.availableIngredients.ContainsKey(itemIdentifier))
				{
					this.availableIngredients[itemIdentifier] = new List<Item>(itemList.Count);
				}
				this.availableIngredients[itemIdentifier].Add(item);
			}
			foreach (Identifier itemId in this.availableIngredients.Keys)
			{
				this.availableIngredients[itemId] = this.SortIngredients(this.availableIngredients[itemId]).ToList<Item>();
			}
		}

		// Token: 0x060040F9 RID: 16633 RVA: 0x001A0EE4 File Offset: 0x0019F0E4
		private IEnumerable<Item> SortIngredients(IEnumerable<Item> items)
		{
			return items.OrderByDescending(new Func<Item, int>(this.<SortIngredients>g__getIngredientContainerPriority|81_3)).ThenBy(delegate(Item it)
			{
				PriceInfo defaultPrice = it.Prefab.DefaultPrice;
				if (defaultPrice == null)
				{
					return 0;
				}
				return defaultPrice.Price;
			}).ThenBy(delegate(Item it)
			{
				if (!MathUtils.IsValid(it.Condition))
				{
					return 0f;
				}
				return it.Condition;
			}).ThenByDescending(delegate(Item it)
			{
				Inventory parentInventory = it.ParentInventory;
				if (parentInventory == null)
				{
					return 0;
				}
				return parentInventory.FindIndex(it);
			});
		}

		// Token: 0x060040FA RID: 16634 RVA: 0x001A0F70 File Offset: 0x0019F170
		private IEnumerable<Item> GetSortedSuitableIngredients()
		{
			List<Item> suitableIngredients = new List<Item>();
			ImmutableArray<FabricationRecipe.RequiredItem>.Enumerator enumerator = this.fabricatedItem.RequiredItems.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FabricationRecipe.RequiredItem requiredItem = enumerator.Current;
				Func<Item, bool> <>9__0;
				foreach (ItemPrefab requiredPrefab in requiredItem.ItemPrefabs)
				{
					if (this.availableIngredients.ContainsKey(requiredPrefab.Identifier))
					{
						List<Item> availableItems = this.availableIngredients[requiredPrefab.Identifier];
						List<Item> list = suitableIngredients;
						IEnumerable<Item> source = availableItems;
						Func<Item, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((Item potentialItem) => requiredItem.IsConditionSuitable(potentialItem.ConditionPercentage)));
						}
						list.AddRange(source.Where(predicate));
					}
				}
			}
			return this.SortIngredients(suitableIngredients);
		}

		// Token: 0x060040FB RID: 16635 RVA: 0x001A1054 File Offset: 0x0019F254
		private void MoveIngredientsToInputContainer(FabricationRecipe targetItem)
		{
			List<Item> chosenIngredients = new List<Item>();
			IEnumerable<Item> suitableIngredients = this.GetSortedSuitableIngredients();
			Func<Item, bool> <>9__0;
			foreach (FabricationRecipe.RequiredItem requiredItem in targetItem.RequiredItems)
			{
				for (int i = 0; i < requiredItem.Amount; i++)
				{
					foreach (Item suitableIngredient in suitableIngredients)
					{
						if (requiredItem.MatchesItem(suitableIngredient) && !chosenIngredients.Contains(suitableIngredient))
						{
							if (suitableIngredient.ParentInventory != this.inputContainer.Inventory)
							{
								if (!this.inputContainer.Inventory.CanBePut(suitableIngredient))
								{
									IEnumerable<Item> allItems = this.inputContainer.Inventory.AllItems;
									Func<Item, bool> predicate;
									if ((predicate = <>9__0) == null)
									{
										predicate = (<>9__0 = ((Item it) => !chosenIngredients.Contains(it)));
									}
									Item unneededItem = allItems.FirstOrDefault(predicate);
									if (unneededItem != null)
									{
										unneededItem.Drop(null, true, true);
									}
								}
								this.inputContainer.Inventory.TryPutItem(suitableIngredient, null, null, true, false, true);
							}
							chosenIngredients.Add(suitableIngredient);
							break;
						}
					}
				}
			}
			this.RefreshAvailableIngredients();
		}

		// Token: 0x060040FC RID: 16636 RVA: 0x001A11B4 File Offset: 0x0019F3B4
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			if (this.fabricatedItem != null)
			{
				componentElement.Add(new XAttribute("fabricateditemidentifier", this.fabricatedItem.TargetItem.Identifier));
				componentElement.Add(new XAttribute("savedtimeuntilready", this.timeUntilReady.ToString("G", CultureInfo.InvariantCulture)));
				componentElement.Add(new XAttribute("savedrequiredtime", this.requiredTime.ToString("G", CultureInfo.InvariantCulture)));
			}
			return componentElement;
		}

		// Token: 0x060040FD RID: 16637 RVA: 0x001A1250 File Offset: 0x0019F450
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.savedFabricatedItem = componentElement.GetAttributeString("fabricateditemidentifier", "");
			this.savedTimeUntilReady = componentElement.GetAttributeFloat("savedtimeuntilready", 0f);
			this.savedRequiredTime = componentElement.GetAttributeFloat("savedrequiredtime", 0f);
		}

		// Token: 0x060040FE RID: 16638 RVA: 0x001A12AC File Offset: 0x0019F4AC
		public override void OnMapLoaded()
		{
			if (string.IsNullOrEmpty(this.savedFabricatedItem))
			{
				return;
			}
			ItemContainer itemContainer = this.inputContainer;
			if (itemContainer != null)
			{
				itemContainer.OnMapLoaded();
			}
			ItemContainer itemContainer2 = this.outputContainer;
			if (itemContainer2 != null)
			{
				itemContainer2.OnMapLoaded();
			}
			FabricationRecipe recipe = this.fabricationRecipes.Values.FirstOrDefault((FabricationRecipe r) => r.TargetItem.Identifier == this.savedFabricatedItem);
			if (recipe == null)
			{
				DebugConsole.ThrowError("Error while loading a fabricator. Can't continue fabricating \"" + this.savedFabricatedItem + "\" (matching recipe not found).", null, null, false, false);
			}
			else
			{
				this.StartFabricating(recipe, null, true);
				this.timeUntilReady = this.savedTimeUntilReady;
				this.requiredTime = this.savedRequiredTime;
			}
			this.savedFabricatedItem = null;
		}

		// Token: 0x060040FF RID: 16639 RVA: 0x001A1351 File Offset: 0x0019F551
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.OnItemFabricated = null;
		}

		// Token: 0x06004101 RID: 16641 RVA: 0x001A1370 File Offset: 0x0019F570
		[CompilerGenerated]
		internal static Option<float> <GetFabricatedItemQuality>g__OverrideChanceIfLess|69_4(Option<float> original, float bonusChance)
		{
			float originalChance;
			if (!original.TryUnwrap(out originalChance))
			{
				return Option.Some<float>(bonusChance);
			}
			if (originalChance <= bonusChance)
			{
				return original;
			}
			return Option.Some<float>(bonusChance);
		}

		// Token: 0x06004102 RID: 16642 RVA: 0x001A139B File Offset: 0x0019F59B
		[CompilerGenerated]
		private int <SortIngredients>g__getIngredientContainerPriority|81_3(Item item)
		{
			if (item.ParentInventory == this.InputContainer.Inventory)
			{
				return 3;
			}
			if (item.ParentInventory is CharacterInventory)
			{
				return 2;
			}
			return 1;
		}

		// Token: 0x04001EFE RID: 7934
		private ulong serverEventId;

		// Token: 0x04001EFF RID: 7935
		private ImmutableDictionary<uint, FabricationRecipe> fabricationRecipes;

		// Token: 0x04001F00 RID: 7936
		private const int MaxAmountToFabricate = 99;

		// Token: 0x04001F01 RID: 7937
		private FabricationRecipe fabricatedItem;

		// Token: 0x04001F02 RID: 7938
		private float timeUntilReady;

		// Token: 0x04001F03 RID: 7939
		private float requiredTime;

		// Token: 0x04001F04 RID: 7940
		private string savedFabricatedItem;

		// Token: 0x04001F05 RID: 7941
		private float savedTimeUntilReady;

		// Token: 0x04001F06 RID: 7942
		private float savedRequiredTime;

		// Token: 0x04001F07 RID: 7943
		private readonly Dictionary<Identifier, List<Item>> availableIngredients = new Dictionary<Identifier, List<Item>>();

		// Token: 0x04001F08 RID: 7944
		private const float RefreshIngredientsInterval = 1f;

		// Token: 0x04001F09 RID: 7945
		private float refreshIngredientsTimer;

		// Token: 0x04001F0A RID: 7946
		private bool hasPower;

		// Token: 0x04001F0B RID: 7947
		private Character user;

		// Token: 0x04001F0C RID: 7948
		private ItemContainer inputContainer;

		// Token: 0x04001F0D RID: 7949
		private ItemContainer outputContainer;

		// Token: 0x04001F10 RID: 7952
		private int amountToFabricate;

		// Token: 0x04001F11 RID: 7953
		private int amountRemaining;

		// Token: 0x04001F12 RID: 7954
		private const float TinkeringSpeedIncrease = 2.5f;

		// Token: 0x04001F13 RID: 7955
		private Fabricator.FabricatorState state;

		// Token: 0x04001F14 RID: 7956
		private float progressState;

		// Token: 0x04001F15 RID: 7957
		private readonly Dictionary<uint, int> fabricationLimits = new Dictionary<uint, int>();

		// Token: 0x04001F16 RID: 7958
		public Action<Item, Character> OnItemFabricated;

		// Token: 0x04001F17 RID: 7959
		public const int PlusOneQualityBonusThreshold = 50;

		// Token: 0x04001F18 RID: 7960
		public const int PlusTwoQualityBonusThreshold = 75;

		// Token: 0x04001F19 RID: 7961
		public const int PlusOneTarget = 100;

		// Token: 0x04001F1A RID: 7962
		public const int PlusTwoTarget = 125;

		// Token: 0x04001F1B RID: 7963
		public const float PlusOneLerp = 0.2f;

		// Token: 0x04001F1C RID: 7964
		public const float PlusTwoLerp = 0.4f;

		// Token: 0x04001F1D RID: 7965
		private readonly HashSet<Item> usedIngredients = new HashSet<Item>();

		// Token: 0x04001F1E RID: 7966
		private readonly Dictionary<ItemPrefab, int> ingredientFlexibilityCache = new Dictionary<ItemPrefab, int>();

		// Token: 0x04001F1F RID: 7967
		private readonly HashSet<Inventory> linkedInventories = new HashSet<Inventory>();

		// Token: 0x02000DA1 RID: 3489
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x060067C1 RID: 26561 RVA: 0x00221343 File Offset: 0x0021F543
			public EventData(ulong serverEventId, Fabricator.FabricatorState state)
			{
				this.ServerEventId = serverEventId;
				this.State = state;
			}

			// Token: 0x04004040 RID: 16448
			public readonly ulong ServerEventId;

			// Token: 0x04004041 RID: 16449
			public readonly Fabricator.FabricatorState State;
		}

		// Token: 0x02000DA2 RID: 3490
		private enum FabricatorState
		{
			// Token: 0x04004043 RID: 16451
			Active = 1,
			// Token: 0x04004044 RID: 16452
			Paused,
			// Token: 0x04004045 RID: 16453
			Stopped = 0
		}

		// Token: 0x02000DA3 RID: 3491
		public readonly struct QualityResult : IEquatable<Fabricator.QualityResult>
		{
			// Token: 0x060067C2 RID: 26562 RVA: 0x00221353 File Offset: 0x0021F553
			public QualityResult(int Quality, bool HasRandomQuality, float PlusOnePercentage, float PlusTwoPercentage)
			{
				this.Quality = Quality;
				this.HasRandomQuality = HasRandomQuality;
				this.PlusOnePercentage = PlusOnePercentage;
				this.PlusTwoPercentage = PlusTwoPercentage;
			}

			// Token: 0x17001665 RID: 5733
			// (get) Token: 0x060067C3 RID: 26563 RVA: 0x00221372 File Offset: 0x0021F572
			// (set) Token: 0x060067C4 RID: 26564 RVA: 0x0022137A File Offset: 0x0021F57A
			public int Quality { get; set; }

			// Token: 0x17001666 RID: 5734
			// (get) Token: 0x060067C5 RID: 26565 RVA: 0x00221383 File Offset: 0x0021F583
			// (set) Token: 0x060067C6 RID: 26566 RVA: 0x0022138B File Offset: 0x0021F58B
			public bool HasRandomQuality { get; set; }

			// Token: 0x17001667 RID: 5735
			// (get) Token: 0x060067C7 RID: 26567 RVA: 0x00221394 File Offset: 0x0021F594
			// (set) Token: 0x060067C8 RID: 26568 RVA: 0x0022139C File Offset: 0x0021F59C
			public float PlusOnePercentage { get; set; }

			// Token: 0x17001668 RID: 5736
			// (get) Token: 0x060067C9 RID: 26569 RVA: 0x002213A5 File Offset: 0x0021F5A5
			// (set) Token: 0x060067CA RID: 26570 RVA: 0x002213AD File Offset: 0x0021F5AD
			public float PlusTwoPercentage { get; set; }

			// Token: 0x17001669 RID: 5737
			// (get) Token: 0x060067CB RID: 26571 RVA: 0x002213B6 File Offset: 0x0021F5B6
			public bool HasRandomQualityRollChance
			{
				get
				{
					return this.HasRandomQuality && (this.PlusOnePercentage > 0f || this.PlusTwoPercentage > 0f);
				}
			}

			// Token: 0x1700166A RID: 5738
			// (get) Token: 0x060067CC RID: 26572 RVA: 0x002213DE File Offset: 0x0021F5DE
			public float TotalPlusOnePercentage
			{
				get
				{
					return Math.Clamp(this.PlusOnePercentage * (100f - this.PlusTwoPercentage) / 100f, 0f, 100f);
				}
			}

			// Token: 0x1700166B RID: 5739
			// (get) Token: 0x060067CD RID: 26573 RVA: 0x00221408 File Offset: 0x0021F608
			public float TotalPlusTwoPercentage
			{
				get
				{
					return Math.Clamp(this.PlusOnePercentage * this.PlusTwoPercentage / 100f, 0f, 100f);
				}
			}

			// Token: 0x060067CE RID: 26574 RVA: 0x0022142C File Offset: 0x0021F62C
			public int RollQuality()
			{
				int additionalQuality = 0;
				if (Fabricator.QualityResult.<RollQuality>g__Roll|24_0(this.PlusOnePercentage))
				{
					additionalQuality++;
					if (Fabricator.QualityResult.<RollQuality>g__Roll|24_0(this.PlusTwoPercentage))
					{
						additionalQuality++;
					}
				}
				return this.Quality + additionalQuality;
			}

			// Token: 0x060067CF RID: 26575 RVA: 0x00221468 File Offset: 0x0021F668
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("QualityResult");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060067D0 RID: 26576 RVA: 0x002214B4 File Offset: 0x0021F6B4
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Quality = ");
				builder.Append(this.Quality.ToString());
				builder.Append(", HasRandomQuality = ");
				builder.Append(this.HasRandomQuality.ToString());
				builder.Append(", PlusOnePercentage = ");
				builder.Append(this.PlusOnePercentage.ToString());
				builder.Append(", PlusTwoPercentage = ");
				builder.Append(this.PlusTwoPercentage.ToString());
				builder.Append(", HasRandomQualityRollChance = ");
				builder.Append(this.HasRandomQualityRollChance.ToString());
				builder.Append(", TotalPlusOnePercentage = ");
				builder.Append(this.TotalPlusOnePercentage.ToString());
				builder.Append(", TotalPlusTwoPercentage = ");
				builder.Append(this.TotalPlusTwoPercentage.ToString());
				return true;
			}

			// Token: 0x060067D1 RID: 26577 RVA: 0x002215D3 File Offset: 0x0021F7D3
			[CompilerGenerated]
			public static bool operator !=(Fabricator.QualityResult left, Fabricator.QualityResult right)
			{
				return !(left == right);
			}

			// Token: 0x060067D2 RID: 26578 RVA: 0x002215DF File Offset: 0x0021F7DF
			[CompilerGenerated]
			public static bool operator ==(Fabricator.QualityResult left, Fabricator.QualityResult right)
			{
				return left.Equals(right);
			}

			// Token: 0x060067D3 RID: 26579 RVA: 0x002215EC File Offset: 0x0021F7EC
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<int>.Default.GetHashCode(this.<Quality>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<HasRandomQuality>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<PlusOnePercentage>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<PlusTwoPercentage>k__BackingField);
			}

			// Token: 0x060067D4 RID: 26580 RVA: 0x0022164E File Offset: 0x0021F84E
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Fabricator.QualityResult && this.Equals((Fabricator.QualityResult)obj);
			}

			// Token: 0x060067D5 RID: 26581 RVA: 0x00221668 File Offset: 0x0021F868
			[CompilerGenerated]
			public bool Equals(Fabricator.QualityResult other)
			{
				return EqualityComparer<int>.Default.Equals(this.<Quality>k__BackingField, other.<Quality>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<HasRandomQuality>k__BackingField, other.<HasRandomQuality>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<PlusOnePercentage>k__BackingField, other.<PlusOnePercentage>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<PlusTwoPercentage>k__BackingField, other.<PlusTwoPercentage>k__BackingField);
			}

			// Token: 0x060067D6 RID: 26582 RVA: 0x002216D5 File Offset: 0x0021F8D5
			[CompilerGenerated]
			public void Deconstruct(out int Quality, out bool HasRandomQuality, out float PlusOnePercentage, out float PlusTwoPercentage)
			{
				Quality = this.Quality;
				HasRandomQuality = this.HasRandomQuality;
				PlusOnePercentage = this.PlusOnePercentage;
				PlusTwoPercentage = this.PlusTwoPercentage;
			}

			// Token: 0x060067D8 RID: 26584 RVA: 0x00221710 File Offset: 0x0021F910
			[CompilerGenerated]
			internal static bool <RollQuality>g__Roll|24_0(float percentage)
			{
				return percentage >= (float)Rand.Range(0, 100, Rand.RandSync.Unsynced);
			}

			// Token: 0x0400404A RID: 16458
			public static readonly Fabricator.QualityResult Empty = new Fabricator.QualityResult(0, true, 0f, 0f);
		}

		// Token: 0x02000DA4 RID: 3492
		private class AbilityFabricatorSkillGain : AbilityObject, IAbilityValue, IAbilitySkillIdentifier
		{
			// Token: 0x060067D9 RID: 26585 RVA: 0x00221722 File Offset: 0x0021F922
			public AbilityFabricatorSkillGain(Identifier skillIdentifier, float skillAmount)
			{
				this.SkillIdentifier = skillIdentifier;
				this.Value = skillAmount;
			}

			// Token: 0x1700166C RID: 5740
			// (get) Token: 0x060067DA RID: 26586 RVA: 0x00221738 File Offset: 0x0021F938
			// (set) Token: 0x060067DB RID: 26587 RVA: 0x00221740 File Offset: 0x0021F940
			public float Value { get; set; }

			// Token: 0x1700166D RID: 5741
			// (get) Token: 0x060067DC RID: 26588 RVA: 0x00221749 File Offset: 0x0021F949
			// (set) Token: 0x060067DD RID: 26589 RVA: 0x00221751 File Offset: 0x0021F951
			public Identifier SkillIdentifier { get; set; }
		}

		// Token: 0x02000DA5 RID: 3493
		private class AbilityFabricationItemAmount : AbilityObject, IAbilityValue, IAbilityItemPrefab
		{
			// Token: 0x060067DE RID: 26590 RVA: 0x0022175A File Offset: 0x0021F95A
			public AbilityFabricationItemAmount(ItemPrefab itemPrefab, float itemAmount)
			{
				this.ItemPrefab = itemPrefab;
				this.Value = itemAmount;
			}

			// Token: 0x1700166E RID: 5742
			// (get) Token: 0x060067DF RID: 26591 RVA: 0x00221770 File Offset: 0x0021F970
			// (set) Token: 0x060067E0 RID: 26592 RVA: 0x00221778 File Offset: 0x0021F978
			public float Value { get; set; }

			// Token: 0x1700166F RID: 5743
			// (get) Token: 0x060067E1 RID: 26593 RVA: 0x00221781 File Offset: 0x0021F981
			// (set) Token: 0x060067E2 RID: 26594 RVA: 0x00221789 File Offset: 0x0021F989
			public ItemPrefab ItemPrefab { get; set; }
		}

		// Token: 0x02000DA6 RID: 3494
		internal sealed class AbilityFabricationItemIngredients : AbilityObject
		{
			// Token: 0x17001670 RID: 5744
			// (get) Token: 0x060067E3 RID: 26595 RVA: 0x00221792 File Offset: 0x0021F992
			// (set) Token: 0x060067E4 RID: 26596 RVA: 0x0022179A File Offset: 0x0021F99A
			public List<Item> Items { get; set; }

			// Token: 0x060067E5 RID: 26597 RVA: 0x002217A3 File Offset: 0x0021F9A3
			public AbilityFabricationItemIngredients(List<Item> items)
			{
				this.Items = items;
			}
		}
	}
}
