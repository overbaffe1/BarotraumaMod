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
	// Token: 0x02000037 RID: 55
	[NullableContext(1)]
	[Nullable(0)]
	internal class UpgradeManager
	{
		// Token: 0x170001AE RID: 430
		// (get) Token: 0x060006C5 RID: 1733 RVA: 0x00040F87 File Offset: 0x0003F187
		private CampaignMetadata Metadata
		{
			get
			{
				return this.Campaign.CampaignMetadata;
			}
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x00040F94 File Offset: 0x0003F194
		public UpgradeManager(CampaignMode campaign)
		{
			UpgradeCategory.Categories.ForEach(delegate(UpgradeCategory c)
			{
				c.DeterminePrefabsThatAllowUpgrades();
			});
			DebugConsole.Log("Created brand new upgrade manager.");
			this.Campaign = campaign;
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x00041010 File Offset: 0x0003F210
		public UpgradeManager(CampaignMode campaign, XElement element, bool isSingleplayer) : this(campaign)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Restored upgrade manager from save file, (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(element.Elements().Count<XElement>());
			defaultInterpolatedStringHandler.AppendLiteral(" pending upgrades).");
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			if (element.Name.LocalName.Equals("pendingupgrades", StringComparison.OrdinalIgnoreCase))
			{
				this.LoadPendingUpgrades(element, isSingleplayer);
				return;
			}
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "pendingupgrades")
				{
					this.LoadPendingUpgrades(subElement, isSingleplayer);
				}
			}
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x000410E4 File Offset: 0x0003F2E4
		public int DetermineItemSwapCost(Item item, [Nullable(2)] ItemPrefab replacement)
		{
			if (replacement == null)
			{
				replacement = ItemPrefab.Find("", item.Prefab.SwappableItem.ReplacementOnUninstall);
				if (replacement == null)
				{
					DebugConsole.ThrowError("Failed to determine swap cost for item \"{}\". Trying to uninstall the item but no replacement item found.", null, null, false, false);
					return 0;
				}
			}
			int price = 0;
			if (replacement == item.Prefab)
			{
				if (item.PendingItemSwap != null)
				{
					int num = price;
					SwappableItem swappableItem = item.PendingItemSwap.SwappableItem;
					CampaignMode campaign = this.Campaign;
					Location location;
					if (campaign == null)
					{
						location = null;
					}
					else
					{
						Map map = campaign.Map;
						location = ((map != null) ? map.CurrentLocation : null);
					}
					price = num - swappableItem.GetPrice(location);
					int num2 = price;
					SwappableItem swappableItem2 = item.Prefab.SwappableItem;
					CampaignMode campaign2 = this.Campaign;
					Location location2;
					if (campaign2 == null)
					{
						location2 = null;
					}
					else
					{
						Map map2 = campaign2.Map;
						location2 = ((map2 != null) ? map2.CurrentLocation : null);
					}
					price = num2 + swappableItem2.GetPrice(location2);
				}
			}
			else
			{
				SwappableItem swappableItem3 = replacement.SwappableItem;
				CampaignMode campaign3 = this.Campaign;
				Location location3;
				if (campaign3 == null)
				{
					location3 = null;
				}
				else
				{
					Map map3 = campaign3.Map;
					location3 = ((map3 != null) ? map3.CurrentLocation : null);
				}
				price = swappableItem3.GetPrice(location3);
				if (item.PendingItemSwap != null)
				{
					int num3 = price;
					SwappableItem swappableItem4 = item.PendingItemSwap.SwappableItem;
					CampaignMode campaign4 = this.Campaign;
					Location location4;
					if (campaign4 == null)
					{
						location4 = null;
					}
					else
					{
						Map map4 = campaign4.Map;
						location4 = ((map4 != null) ? map4.CurrentLocation : null);
					}
					price = num3 - swappableItem4.GetPrice(location4);
					int num4 = price;
					SwappableItem swappableItem5 = item.Prefab.SwappableItem;
					CampaignMode campaign5 = this.Campaign;
					Location location5;
					if (campaign5 == null)
					{
						location5 = null;
					}
					else
					{
						Map map5 = campaign5.Map;
						location5 = ((map5 != null) ? map5.CurrentLocation : null);
					}
					price = num4 + swappableItem5.GetPrice(location5);
				}
				if (replacement != item.Prefab)
				{
					int num5 = price;
					SwappableItem swappableItem6 = item.Prefab.SwappableItem;
					CampaignMode campaign6 = this.Campaign;
					Location location6;
					if (campaign6 == null)
					{
						location6 = null;
					}
					else
					{
						Map map6 = campaign6.Map;
						location6 = ((map6 != null) ? map6.CurrentLocation : null);
					}
					price = num5 - swappableItem6.GetPrice(location6);
				}
			}
			return price;
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x00041270 File Offset: 0x0003F470
		public bool TryPurchaseUpgrade(UpgradePrefab prefab, UpgradeCategory category, bool force = false, [Nullable(2)] Client client = null)
		{
			UpgradeManager.<>c__DisplayClass14_0 CS$<>8__locals1;
			CS$<>8__locals1.prefab = prefab;
			if (!this.HasPermissionToManageUpgrades(client))
			{
				return false;
			}
			if (!this.CanUpgradeSub())
			{
				DebugConsole.ThrowError("Cannot upgrade when switching to another submarine.", null, null, false, false);
				return false;
			}
			UpgradePrice price2 = CS$<>8__locals1.prefab.Price;
			UpgradePrefab prefab2 = CS$<>8__locals1.prefab;
			int upgradeLevel = this.GetUpgradeLevel(CS$<>8__locals1.prefab, category, null);
			Map map = this.Campaign.Map;
			int price = price2.GetBuyPrice(prefab2, upgradeLevel, (map != null) ? map.CurrentLocation : null, null);
			int currentLevel = this.GetUpgradeLevel(CS$<>8__locals1.prefab, category, null);
			CS$<>8__locals1.newLevel = currentLevel + 1;
			int maxLevel = CS$<>8__locals1.prefab.GetMaxLevelForCurrentSub();
			if (currentLevel + 1 > maxLevel)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to purchase \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CS$<>8__locals1.prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" over the max level! (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.newLevel);
				defaultInterpolatedStringHandler.AppendLiteral(" > ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(maxLevel);
				defaultInterpolatedStringHandler.AppendLiteral("). The transaction has been cancelled.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return false;
			}
			if (!force)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					if (!networkMember.IsClient)
					{
						if (networkMember.IsServer)
						{
							Character character = (client != null) ? client.Character : null;
							if (character != null)
							{
								if (!UpgradeManager.<TryPurchaseUpgrade>g__TryTakeResources|14_0(character, ref CS$<>8__locals1))
								{
									return false;
								}
								goto IL_1D4;
							}
						}
					}
					else
					{
						if (!CS$<>8__locals1.prefab.HasResourcesToUpgrade(Character.Controlled, CS$<>8__locals1.newLevel))
						{
							return false;
						}
						goto IL_1D4;
					}
				}
				else
				{
					Character controlled = Character.Controlled;
					if (controlled != null)
					{
						if (!UpgradeManager.<TryPurchaseUpgrade>g__TryTakeResources|14_0(controlled, ref CS$<>8__locals1))
						{
							return false;
						}
						goto IL_1D4;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(38, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Tried to purchase \"");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CS$<>8__locals1.prefab.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("\" without a player.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return false;
			}
			IL_1D4:
			if (price < 0)
			{
				Map map2 = this.Campaign.Map;
				Location location = (map2 != null) ? map2.CurrentLocation : null;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Upgrade price is less than 0! (");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(price);
				defaultInterpolatedStringHandler3.AppendLiteral(")");
				string text = defaultInterpolatedStringHandler3.ToStringAndClear();
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("Level", currentLevel);
				dictionary.Add("Saved Level", this.GetRealUpgradeLevel(CS$<>8__locals1.prefab, category));
				string key = "Upgrade";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(category.Identifier);
				defaultInterpolatedStringHandler4.AppendLiteral(".");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(CS$<>8__locals1.prefab.Identifier);
				dictionary.Add(key, defaultInterpolatedStringHandler4.ToStringAndClear());
				dictionary.Add("Location", (location != null) ? location.Type : null);
				string key2 = "Reputation";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(3, 2);
				float? value;
				if (location == null)
				{
					value = null;
				}
				else
				{
					Reputation reputation = location.Reputation;
					value = ((reputation != null) ? new float?(reputation.Value) : null);
				}
				defaultInterpolatedStringHandler5.AppendFormatted<float?>(value);
				defaultInterpolatedStringHandler5.AppendLiteral(" / ");
				int? value2;
				if (location == null)
				{
					value2 = null;
				}
				else
				{
					Reputation reputation2 = location.Reputation;
					value2 = ((reputation2 != null) ? new int?(reputation2.MaxReputation) : null);
				}
				defaultInterpolatedStringHandler5.AppendFormatted<int?>(value2);
				dictionary.Add(key2, defaultInterpolatedStringHandler5.ToStringAndClear());
				dictionary.Add("Base Price", CS$<>8__locals1.prefab.Price.BasePrice);
				UpgradeManager.LogError(text, dictionary, null);
			}
			if (force)
			{
				price = 0;
			}
			if (force || this.Campaign.TryPurchase(client, price))
			{
				if ((GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && (this.lastUpgradeSpeak == DateTime.MinValue || this.lastUpgradeSpeak.AddMinutes(5.0) < DateTime.Now))
				{
					this.UpgradeNPCSpeak(TextManager.Get("Dialog.UpgradePurchased").Value, this.Campaign.IsSinglePlayer, null);
					this.lastUpgradeSpeak = DateTime.Now;
				}
				GameAnalyticsManager.AddMoneySpentEvent(price, GameAnalyticsManager.MoneySink.SubmarineUpgrade, CS$<>8__locals1.prefab.Identifier.Value);
				PurchasedUpgrade upgrade = this.FindMatchingUpgrade(CS$<>8__locals1.prefab, category);
				if (upgrade == null)
				{
					this.PendingUpgrades.Add(new PurchasedUpgrade(CS$<>8__locals1.prefab, category, 1));
				}
				else
				{
					upgrade.Level++;
				}
				NamedEvent<UpgradeManager> onUpgradesChanged = this.OnUpgradesChanged;
				if (onUpgradesChanged != null)
				{
					onUpgradesChanged.Invoke(this);
				}
				return true;
			}
			string str = "Tried to purchase an upgrade with insufficient funds, the transaction has not been completed.\n";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(25, 3);
			defaultInterpolatedStringHandler6.AppendLiteral("Upgrade: ");
			defaultInterpolatedStringHandler6.AppendFormatted<LocalizedString>(CS$<>8__locals1.prefab.Name);
			defaultInterpolatedStringHandler6.AppendLiteral(", Cost: ");
			defaultInterpolatedStringHandler6.AppendFormatted<int>(price);
			defaultInterpolatedStringHandler6.AppendLiteral(", Have: ");
			defaultInterpolatedStringHandler6.AppendFormatted<int>(this.Campaign.GetWallet(client).Balance);
			DebugConsole.ThrowError(str + defaultInterpolatedStringHandler6.ToStringAndClear(), null, null, false, false);
			return false;
		}

		// Token: 0x060006CA RID: 1738 RVA: 0x00041760 File Offset: 0x0003F960
		public void AddUpgradeExternally(UpgradePrefab prefab, UpgradeCategory category, int level)
		{
			int maxLevel = prefab.GetMaxLevelForCurrentSub();
			int currentLevel = this.GetUpgradeLevel(prefab, category, null);
			if (currentLevel + 1 > maxLevel)
			{
				return;
			}
			this.PendingUpgrades.Add(new PurchasedUpgrade(prefab, category, level));
			NamedEvent<UpgradeManager> onUpgradesChanged = this.OnUpgradesChanged;
			if (onUpgradesChanged == null)
			{
				return;
			}
			onUpgradesChanged.Invoke(this);
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x000417AC File Offset: 0x0003F9AC
		public void PurchaseItemSwap(Item itemToRemove, ItemPrefab itemToInstall, bool isNetworkMessage = false, [Nullable(2)] Client client = null)
		{
			if (!this.HasPermissionToManageUpgrades(client))
			{
				return;
			}
			if (!this.CanUpgradeSub())
			{
				DebugConsole.ThrowError("Cannot swap items when switching to another submarine.", null, null, false, false);
				return;
			}
			if (itemToRemove == null)
			{
				DebugConsole.ThrowError("Cannot swap null item!", null, null, false, false);
				return;
			}
			if (itemToRemove.HiddenInGame)
			{
				DebugConsole.ThrowError("Cannot swap item \"" + itemToRemove.Name + "\" because it's set to be hidden in-game.", null, null, false, false);
				return;
			}
			if (!itemToRemove.AllowSwapping)
			{
				DebugConsole.ThrowError("Cannot swap item \"" + itemToRemove.Name + "\" because it's configured to be non-swappable.", null, null, false, false);
				return;
			}
			Func<Identifier, bool> <>9__2;
			Func<Identifier, bool> <>9__3;
			if (!UpgradeCategory.Categories.Any(delegate(UpgradeCategory c)
			{
				IEnumerable<Identifier> itemTags = c.ItemTags;
				Func<Identifier, bool> predicate;
				if ((predicate = <>9__2) == null)
				{
					predicate = (<>9__2 = ((Identifier t) => itemToRemove.HasTag(t)));
				}
				if (itemTags.Any(predicate))
				{
					IEnumerable<Identifier> itemTags2 = c.ItemTags;
					Func<Identifier, bool> predicate2;
					if ((predicate2 = <>9__3) == null)
					{
						predicate2 = (<>9__3 = ((Identifier t) => itemToInstall.Tags.Contains(t)));
					}
					return itemTags2.Any(predicate2);
				}
				return false;
			}))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to swap item \"");
				defaultInterpolatedStringHandler.AppendFormatted(itemToRemove.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" with \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemToInstall.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" (not in the same upgrade category).");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (itemToRemove.Prefab == itemToInstall)
			{
				DebugConsole.ThrowError("Failed to swap item \"" + itemToRemove.Name + "\" (trying to swap with the same item!).", null, null, false, false);
				return;
			}
			if (itemToRemove.Prefab.SwappableItem == null)
			{
				DebugConsole.ThrowError("Failed to swap item \"" + itemToRemove.Name + "\" (not configured as a swappable item).", null, null, false, false);
				return;
			}
			ICollection<Item> linkedItems = UpgradeManager.GetLinkedItemsToSwap(itemToRemove);
			int price = 0;
			if (!itemToRemove.AvailableSwaps.Contains(itemToInstall))
			{
				SwappableItem swappableItem = itemToInstall.SwappableItem;
				Map map = this.Campaign.Map;
				price = swappableItem.GetPrice((map != null) ? map.CurrentLocation : null) * linkedItems.Count;
			}
			if (isNetworkMessage)
			{
				price = 0;
			}
			if (!isNetworkMessage && !this.Campaign.TryPurchase(client, price))
			{
				string str = "Tried to swap an item with insufficient funds, the transaction has not been completed.\n";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(51, 4);
				defaultInterpolatedStringHandler2.AppendLiteral("Item to remove: ");
				defaultInterpolatedStringHandler2.AppendFormatted(itemToRemove.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(", Item to install: ");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(itemToInstall.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(", Cost: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(price);
				defaultInterpolatedStringHandler2.AppendLiteral(", Have: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.Campaign.GetWallet(client).Balance);
				DebugConsole.ThrowError(str + defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return;
			}
			this.PurchasedItemSwaps.RemoveAll((PurchasedItemSwap p) => linkedItems.Contains(p.ItemToRemove));
			if ((GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && (this.lastUpgradeSpeak == DateTime.MinValue || this.lastUpgradeSpeak.AddMinutes(5.0) < DateTime.Now))
			{
				this.UpgradeNPCSpeak(TextManager.Get("Dialog.UpgradePurchased").Value, this.Campaign.IsSinglePlayer, null);
				this.lastUpgradeSpeak = DateTime.Now;
			}
			GameAnalyticsManager.AddMoneySpentEvent(price, GameAnalyticsManager.MoneySink.SubmarineWeapon, itemToInstall.Identifier.Value);
			foreach (Item itemToSwap in linkedItems)
			{
				itemToSwap.AvailableSwaps.Add(itemToSwap.Prefab);
				if (itemToInstall != null && !itemToSwap.AvailableSwaps.Contains(itemToInstall))
				{
					itemToSwap.PurchasedNewSwap = true;
					itemToSwap.AvailableSwaps.Add(itemToInstall);
				}
				if (itemToSwap.Prefab != itemToInstall && itemToInstall != null)
				{
					itemToSwap.PendingItemSwap = itemToInstall;
					this.PurchasedItemSwaps.Add(new PurchasedItemSwap(itemToSwap, itemToInstall));
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(32, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("CLIENT: Swapped item \"");
					defaultInterpolatedStringHandler3.AppendFormatted(itemToSwap.Name);
					defaultInterpolatedStringHandler3.AppendLiteral("\" with \"");
					defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(itemToInstall.Name);
					defaultInterpolatedStringHandler3.AppendLiteral("\".");
					UpgradeManager.DebugLog(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.Orange));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(47, 2);
					defaultInterpolatedStringHandler4.AppendLiteral("CLIENT: Cancelled swapping the item \"");
					defaultInterpolatedStringHandler4.AppendFormatted(itemToSwap.Name);
					defaultInterpolatedStringHandler4.AppendLiteral("\" with \"");
					ItemPrefab pendingItemSwap = itemToSwap.PendingItemSwap;
					defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(((pendingItemSwap != null) ? pendingItemSwap.Name : null) ?? null);
					defaultInterpolatedStringHandler4.AppendLiteral("\".");
					UpgradeManager.DebugLog(defaultInterpolatedStringHandler4.ToStringAndClear(), new Color?(Color.Orange));
				}
			}
			NamedEvent<UpgradeManager> onUpgradesChanged = this.OnUpgradesChanged;
			if (onUpgradesChanged == null)
			{
				return;
			}
			onUpgradesChanged.Invoke(this);
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x00041CCC File Offset: 0x0003FECC
		public void CancelItemSwap(Item itemToRemove, bool force = false, [Nullable(2)] Client client = null)
		{
			if (!this.HasPermissionToManageUpgrades(client))
			{
				return;
			}
			if (!this.CanUpgradeSub())
			{
				DebugConsole.ThrowError("Cannot swap items when switching to another submarine.", null, null, false, false);
				return;
			}
			if (((itemToRemove != null) ? itemToRemove.PendingItemSwap : null) == null)
			{
				bool? flag;
				if (itemToRemove == null)
				{
					flag = null;
				}
				else
				{
					SwappableItem swappableItem2 = itemToRemove.Prefab.SwappableItem;
					flag = ((swappableItem2 != null) ? new bool?(swappableItem2.ReplacementOnUninstall.IsEmpty) : null);
				}
				bool? flag2 = flag;
				if (flag2.GetValueOrDefault(true))
				{
					DebugConsole.ThrowError("Cannot uninstall item \"" + ((itemToRemove != null) ? itemToRemove.Name : null) + "\" (no replacement item configured).", null, null, false, false);
					return;
				}
			}
			SwappableItem swappableItem = itemToRemove.Prefab.SwappableItem;
			if (swappableItem == null)
			{
				DebugConsole.ThrowError("Failed to uninstall item \"" + itemToRemove.Name + "\" (not configured as a swappable item).", null, null, false, false);
				return;
			}
			if ((GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && (this.lastUpgradeSpeak == DateTime.MinValue || this.lastUpgradeSpeak.AddMinutes(5.0) < DateTime.Now))
			{
				this.UpgradeNPCSpeak(TextManager.Get("Dialog.UpgradePurchased").Value, this.Campaign.IsSinglePlayer, null);
				this.lastUpgradeSpeak = DateTime.Now;
			}
			ICollection<Item> linkedItems = UpgradeManager.GetLinkedItemsToSwap(itemToRemove);
			using (IEnumerator<Item> enumerator = linkedItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item itemToCancel = enumerator.Current;
					if (itemToCancel.PendingItemSwap == null)
					{
						ItemPrefab replacement = MapEntityPrefab.FindByIdentifier(swappableItem.ReplacementOnUninstall) as ItemPrefab;
						if (replacement == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to uninstall item \"");
							defaultInterpolatedStringHandler.AppendFormatted(itemToCancel.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\". Could not find the replacement item \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(swappableItem.ReplacementOnUninstall);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
							break;
						}
						this.PurchasedItemSwaps.RemoveAll((PurchasedItemSwap p) => p.ItemToRemove == itemToCancel);
						this.PurchasedItemSwaps.Add(new PurchasedItemSwap(itemToCancel, replacement));
						UpgradeManager.DebugLog("Uninstalled item item \"" + itemToCancel.Name + "\".", new Color?(Color.Orange));
						itemToCancel.PendingItemSwap = replacement;
					}
					else
					{
						this.PurchasedItemSwaps.RemoveAll((PurchasedItemSwap p) => p.ItemToRemove == itemToCancel);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(39, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Cancelled swapping the item \"");
						defaultInterpolatedStringHandler2.AppendFormatted(itemToCancel.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\" with \"");
						defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(itemToCancel.PendingItemSwap.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\".");
						UpgradeManager.DebugLog(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Orange));
						itemToCancel.PendingItemSwap = null;
					}
				}
			}
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x00041FF8 File Offset: 0x000401F8
		public static ICollection<Item> GetLinkedItemsToSwap(Item item)
		{
			HashSet<Item> linkedItems = new HashSet<Item>
			{
				item
			};
			foreach (MapEntity linkedEntity in item.linkedTo)
			{
				foreach (MapEntity secondLinkedEntity in linkedEntity.linkedTo)
				{
					Item linkedItem = secondLinkedEntity as Item;
					if (linkedItem != null && linkedItem != item && linkedItem.AllowSwapping && linkedItem.Prefab.SwappableItem != null && (linkedItem.Prefab.SwappableItem.CanBeBought || item.Prefab.SwappableItem.ReplacementOnUninstall == linkedItem.Prefab.Identifier) && linkedItem.Prefab.SwappableItem.SwapIdentifier.Equals(item.Prefab.SwappableItem.SwapIdentifier, StringComparison.OrdinalIgnoreCase))
					{
						linkedItems.Add(linkedItem);
					}
				}
			}
			return linkedItems;
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x00042130 File Offset: 0x00040330
		public void ApplyUpgrades()
		{
			this.PurchasedUpgrades.Clear();
			this.PurchasedItemSwaps.Clear();
			if (Submarine.MainSub == null)
			{
				return;
			}
			List<PurchasedUpgrade> pendingUpgrades = this.PendingUpgrades;
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.Outpost)
			{
				return;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient && this.loadedUpgrades != null)
			{
				pendingUpgrades = this.loadedUpgrades;
			}
			DebugConsole.Log("Applying upgrades...");
			foreach (PurchasedUpgrade purchasedUpgrade in pendingUpgrades)
			{
				UpgradePrefab upgradePrefab;
				UpgradeCategory upgradeCategory;
				int num;
				purchasedUpgrade.Deconstruct(out upgradePrefab, out upgradeCategory, out num);
				UpgradePrefab prefab = upgradePrefab;
				UpgradeCategory category = upgradeCategory;
				int level = num;
				int newLevel = UpgradeManager.BuyUpgrade(prefab, category, Submarine.MainSub, level, null);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 4);
				defaultInterpolatedStringHandler.AppendLiteral("    - ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(category.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" lvl. ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(level);
				defaultInterpolatedStringHandler.AppendLiteral(", new: (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(newLevel);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				this.SetUpgradeLevel(prefab, category, this.GetRealUpgradeLevel(prefab, category) + level);
			}
			this.PendingUpgrades.Clear();
			List<PurchasedUpgrade> list = this.loadedUpgrades;
			if (list != null)
			{
				list.Clear();
			}
			this.loadedUpgrades = null;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x000422C4 File Offset: 0x000404C4
		public void CreateUpgradeErrorMessage(string text, bool isSinglePlayer, Character character)
		{
			if (this.lastErrorSpeak == DateTime.MinValue || this.lastErrorSpeak.AddSeconds(10.0) < DateTime.Now)
			{
				this.UpgradeNPCSpeak(text, isSinglePlayer, character);
				this.lastErrorSpeak = DateTime.Now;
			}
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x00042318 File Offset: 0x00040518
		private void UpgradeNPCSpeak(string text, bool isSinglePlayer, [Nullable(2)] Character character = null)
		{
			Level loaded = Level.Loaded;
			bool flag;
			if (loaded == null)
			{
				flag = (null != null);
			}
			else
			{
				Submarine startOutpost = loaded.StartOutpost;
				if (startOutpost == null)
				{
					flag = (null != null);
				}
				else
				{
					SubmarineInfo info = startOutpost.Info;
					flag = (((info != null) ? info.OutpostNPCs : null) != null);
				}
			}
			if (!flag)
			{
				return;
			}
			foreach (Character npc in Level.Loaded.StartOutpost.Info.OutpostNPCs.SelectMany((KeyValuePair<Identifier, List<Character>> kpv) => kpv.Value))
			{
				if (npc.CampaignInteractionType == CampaignMode.InteractionType.Upgrade)
				{
					npc.Speak(text, new ChatMessageType?(ChatMessageType.Default), 0f, default(Identifier), 0f);
					break;
				}
			}
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x000423E8 File Offset: 0x000405E8
		public void SanityCheckUpgrades()
		{
			UpgradeManager.<>c__DisplayClass22_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			GameSession gameSession = GameMain.GameSession;
			CS$<>8__locals1.submarine = (((gameSession != null) ? gameSession.Submarine : null) ?? Submarine.MainSub);
			if (CS$<>8__locals1.submarine == null)
			{
				return;
			}
			foreach (Structure wall in CS$<>8__locals1.submarine.GetWalls(true))
			{
				foreach (UpgradeCategory category in UpgradeCategory.Categories)
				{
					foreach (UpgradePrefab prefab in UpgradePrefab.Prefabs)
					{
						if (prefab.IsWallUpgrade)
						{
							this.<SanityCheckUpgrades>g__TryFixUpgrade|22_0(wall, category, prefab, ref CS$<>8__locals1);
						}
					}
				}
			}
			foreach (Item item in CS$<>8__locals1.submarine.GetItems(true))
			{
				foreach (UpgradeCategory category2 in UpgradeCategory.Categories)
				{
					foreach (UpgradePrefab prefab2 in UpgradePrefab.Prefabs)
					{
						this.<SanityCheckUpgrades>g__TryFixUpgrade|22_0(item, category2, prefab2, ref CS$<>8__locals1);
					}
				}
			}
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x000425B8 File Offset: 0x000407B8
		private static void FixUpgradeOnItem(ISerializableEntity target, UpgradePrefab prefab, int level)
		{
			MapEntity mapEntity = target as MapEntity;
			if (mapEntity != null)
			{
				if (level == 0)
				{
					return;
				}
				mapEntity.SetUpgrade(new Upgrade(target, prefab, level, null), false);
			}
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x000425E4 File Offset: 0x000407E4
		private static int BuyUpgrade(UpgradePrefab prefab, UpgradeCategory category, Submarine submarine, int level = 1, [Nullable(2)] Submarine parentSub = null)
		{
			if (parentSub == null)
			{
				UpgradeManager.upgradedSubs.Clear();
			}
			UpgradeManager.upgradedSubs.Add(submarine);
			UpgradeManager.<>c__DisplayClass25_0 CS$<>8__locals1;
			CS$<>8__locals1.newLevel = null;
			if (category.IsWallUpgrade)
			{
				using (List<Structure>.Enumerator enumerator = submarine.GetWalls(true).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Structure structure = enumerator.Current;
						Upgrade upgrade = new Upgrade(structure, prefab, level, null);
						structure.AddUpgrade(upgrade, false);
						Upgrade newUpgrade = structure.GetUpgrade(prefab.Identifier);
						if (newUpgrade != null)
						{
							UpgradeManager.<BuyUpgrade>g__SanityCheck|25_0(newUpgrade, structure, ref CS$<>8__locals1);
							int value = CS$<>8__locals1.newLevel.GetValueOrDefault();
							if (CS$<>8__locals1.newLevel == null)
							{
								value = newUpgrade.Level;
								CS$<>8__locals1.newLevel = new int?(value);
							}
						}
					}
					goto IL_16F;
				}
			}
			foreach (Item item2 in submarine.GetItems(true))
			{
				if (category.CanBeApplied(item2, prefab))
				{
					Upgrade upgrade2 = new Upgrade(item2, prefab, level, null);
					item2.AddUpgrade(upgrade2, false);
					Upgrade newUpgrade2 = item2.GetUpgrade(prefab.Identifier);
					if (newUpgrade2 != null)
					{
						UpgradeManager.<BuyUpgrade>g__SanityCheck|25_0(newUpgrade2, item2, ref CS$<>8__locals1);
						int value = CS$<>8__locals1.newLevel.GetValueOrDefault();
						if (CS$<>8__locals1.newLevel == null)
						{
							value = newUpgrade2.Level;
							CS$<>8__locals1.newLevel = new int?(value);
						}
					}
				}
			}
			IL_16F:
			foreach (Submarine loadedSub in Submarine.Loaded)
			{
				if (loadedSub != parentSub && loadedSub != submarine)
				{
					SubmarineInfo info = loadedSub.Info;
					if (info != null && info.Type <= SubmarineType.Player && !UpgradeManager.upgradedSubs.Contains(loadedSub))
					{
						SubmarineInfo info2 = loadedSub.Info;
						XElement root = (info2 != null) ? info2.SubmarineElement : null;
						if (root != null && root.Name.ToString().Equals("LinkedSubmarine", StringComparison.OrdinalIgnoreCase))
						{
							if (root.Attribute("location") != null)
							{
								ushort dockingPortID = (ushort)root.GetAttributeInt("originallinkedto", 0);
								if (dockingPortID > 0 && submarine.GetItems(true).Any((Item item) => item.ID == dockingPortID))
								{
									UpgradeManager.BuyUpgrade(prefab, category, loadedSub, level, submarine);
								}
							}
						}
					}
				}
			}
			return CS$<>8__locals1.newLevel.GetValueOrDefault(-1);
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0004289C File Offset: 0x00040A9C
		public int GetUpgradeLevel(UpgradePrefab prefab, UpgradeCategory category, [Nullable(2)] SubmarineInfo info = null)
		{
			UpgradeManager.<>c__DisplayClass26_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.prefab = prefab;
			CS$<>8__locals1.category = category;
			if (!this.Metadata.HasKey(UpgradeManager.FormatIdentifier(CS$<>8__locals1.prefab, CS$<>8__locals1.category)))
			{
				return this.<GetUpgradeLevel>g__GetPendingLevel|26_0(ref CS$<>8__locals1);
			}
			int maxLevel = (info == null) ? CS$<>8__locals1.prefab.GetMaxLevelForCurrentSub() : CS$<>8__locals1.prefab.GetMaxLevel(info);
			return Math.Min(this.GetRealUpgradeLevel(CS$<>8__locals1.prefab, CS$<>8__locals1.category) + this.<GetUpgradeLevel>g__GetPendingLevel|26_0(ref CS$<>8__locals1), maxLevel);
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x00042926 File Offset: 0x00040B26
		public int GetRealUpgradeLevel(UpgradePrefab prefab, UpgradeCategory category)
		{
			if (this.Metadata.HasKey(UpgradeManager.FormatIdentifier(prefab, category)))
			{
				return this.Metadata.GetInt(UpgradeManager.FormatIdentifier(prefab, category), new int?(0));
			}
			return 0;
		}

		// Token: 0x060006D6 RID: 1750 RVA: 0x00042956 File Offset: 0x00040B56
		public int GetRealUpgradeLevelForSub(UpgradePrefab prefab, UpgradeCategory category, SubmarineInfo info)
		{
			return Math.Min(this.GetRealUpgradeLevel(prefab, category), prefab.GetMaxLevel(info));
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x0004296C File Offset: 0x00040B6C
		private void SetUpgradeLevel(UpgradePrefab prefab, UpgradeCategory category, int level)
		{
			this.Metadata.SetValue(UpgradeManager.FormatIdentifier(prefab, category), level);
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x00042986 File Offset: 0x00040B86
		public bool CanUpgradeSub()
		{
			return this.Campaign.PendingSubmarineSwitch == null || this.Campaign.PendingSubmarineSwitch.Name == Submarine.MainSub.Info.Name;
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x000429BB File Offset: 0x00040BBB
		[NullableContext(2)]
		public bool HasPermissionToManageUpgrades(Client client = null)
		{
			return !GameMain.IsMultiplayer || (client != null && CampaignMode.AllowedToManageCampaign(client, ClientPermissions.ManageCampaign));
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x000429D4 File Offset: 0x00040BD4
		[NullableContext(2)]
		public void Save(XElement parent)
		{
			if (parent == null)
			{
				return;
			}
			XElement upgradeManagerElement = new XElement("upgrademanager");
			parent.Add(upgradeManagerElement);
			UpgradeManager.SavePendingUpgrades(upgradeManagerElement, this.PendingUpgrades);
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x00042A08 File Offset: 0x00040C08
		private static void SavePendingUpgrades([Nullable(2)] XElement parent, List<PurchasedUpgrade> upgrades)
		{
			if (parent == null)
			{
				return;
			}
			DebugConsole.Log("Saving pending upgrades to save file...");
			XElement upgradeElement = new XElement("PendingUpgrades");
			foreach (PurchasedUpgrade purchasedUpgrade in upgrades)
			{
				UpgradePrefab upgradePrefab;
				UpgradeCategory upgradeCategory;
				int num;
				purchasedUpgrade.Deconstruct(out upgradePrefab, out upgradeCategory, out num);
				UpgradePrefab prefab = upgradePrefab;
				UpgradeCategory category = upgradeCategory;
				int level = num;
				upgradeElement.Add(new XElement("PendingUpgrade", new object[]
				{
					new XAttribute("category", category.Identifier),
					new XAttribute("prefab", prefab.Identifier),
					new XAttribute("level", level)
				}));
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Saved ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(upgradeElement.Elements().Count<XElement>());
			defaultInterpolatedStringHandler.AppendLiteral(" pending upgrades.");
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			parent.Add(upgradeElement);
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x00042B3C File Offset: 0x00040D3C
		[NullableContext(2)]
		private void LoadPendingUpgrades(XElement element, bool isSingleplayer = true)
		{
			if (element == null || !element.HasElements)
			{
				return;
			}
			List<PurchasedUpgrade> pendingUpgrades = new List<PurchasedUpgrade>();
			foreach (XElement upgrade in element.Elements())
			{
				Identifier categoryIdentifier = upgrade.GetAttributeIdentifier("category", Identifier.Empty);
				UpgradeCategory category = UpgradeCategory.Find(categoryIdentifier);
				if (!categoryIdentifier.IsEmpty && category != null)
				{
					Identifier prefabIdentifier = upgrade.GetAttributeIdentifier("prefab", Identifier.Empty);
					UpgradePrefab prefab = UpgradePrefab.Find(prefabIdentifier);
					if (!prefabIdentifier.IsEmpty && prefab != null)
					{
						int level = upgrade.GetAttributeInt("level", -1);
						if (level >= 0)
						{
							pendingUpgrades.Add(new PurchasedUpgrade(prefab, category, level));
						}
					}
				}
			}
			this.SetPendingUpgrades(pendingUpgrades);
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x00042C10 File Offset: 0x00040E10
		public static void LogError(string text, [Nullable(new byte[]
		{
			1,
			1,
			2
		})] Dictionary<string, object> data, [Nullable(2)] Exception e = null)
		{
			string error = text + "\n";
			foreach (KeyValuePair<string, object> keyValuePair in data)
			{
				string text2;
				object obj;
				keyValuePair.Deconstruct(out text2, out obj);
				string label = text2;
				object value = obj;
				string str = error;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
				defaultInterpolatedStringHandler.AppendLiteral("    - ");
				defaultInterpolatedStringHandler.AppendFormatted(label);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<object>(value ?? "NULL");
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				error = str + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			DebugConsole.ThrowError(error.TrimEnd('\n'), e, null, false, false);
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x00042CDC File Offset: 0x00040EDC
		public void SetPendingUpgrades(List<PurchasedUpgrade> upgrades)
		{
			this.PendingUpgrades.Clear();
			this.PendingUpgrades.AddRange(upgrades);
			NamedEvent<UpgradeManager> onUpgradesChanged = this.OnUpgradesChanged;
			if (onUpgradesChanged == null)
			{
				return;
			}
			onUpgradesChanged.Invoke(this);
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x00042D06 File Offset: 0x00040F06
		public static void DebugLog(string msg, Color? color = null)
		{
			DebugConsole.Log(msg);
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x00042D10 File Offset: 0x00040F10
		[return: Nullable(2)]
		private PurchasedUpgrade FindMatchingUpgrade(UpgradePrefab prefab, UpgradeCategory category)
		{
			return this.PendingUpgrades.Find((PurchasedUpgrade u) => u.Prefab == prefab && u.Category == category);
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x00042D48 File Offset: 0x00040F48
		private static Identifier FormatIdentifier(UpgradePrefab prefab, UpgradeCategory category)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
			defaultInterpolatedStringHandler.AppendLiteral("upgrade.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(category.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
			return defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x00042DAC File Offset: 0x00040FAC
		[CompilerGenerated]
		internal static bool <TryPurchaseUpgrade>g__TryTakeResources|14_0(Character character, ref UpgradeManager.<>c__DisplayClass14_0 A_1)
		{
			bool result = A_1.prefab.TryTakeResources(character, A_1.newLevel);
			if (!result)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to purchase \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(A_1.prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" but the player does not have the required resources.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			return result;
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x00042E14 File Offset: 0x00041014
		[CompilerGenerated]
		private void <SanityCheckUpgrades>g__TryFixUpgrade|22_0(MapEntity entity, UpgradeCategory category, UpgradePrefab prefab, ref UpgradeManager.<>c__DisplayClass22_0 A_4)
		{
			if (!category.CanBeApplied(entity, prefab))
			{
				return;
			}
			int level = this.GetRealUpgradeLevel(prefab, category);
			SubmarineInfo info = A_4.submarine.Info;
			int maxLevel = (info != null) ? prefab.GetMaxLevel(info) : prefab.MaxLevel;
			if (maxLevel < level)
			{
				level = maxLevel;
			}
			if (level == 0)
			{
				return;
			}
			Upgrade upgrade = entity.GetUpgrade(prefab.Identifier);
			if (upgrade == null || upgrade.Level != level)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 4);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(entity.Prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" has incorrect \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" level! Expected ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(level);
				defaultInterpolatedStringHandler.AppendLiteral(" but got ");
				defaultInterpolatedStringHandler.AppendFormatted<int>((upgrade != null) ? upgrade.Level : 0);
				defaultInterpolatedStringHandler.AppendLiteral(". Fixing...");
				UpgradeManager.DebugLog(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				UpgradeManager.FixUpgradeOnItem((ISerializableEntity)entity, prefab, level);
			}
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x00042F14 File Offset: 0x00041114
		[CompilerGenerated]
		internal static void <BuyUpgrade>g__SanityCheck|25_0(Upgrade newUpgrade, MapEntity target, ref UpgradeManager.<>c__DisplayClass25_0 A_2)
		{
			if (A_2.newLevel != null)
			{
				int? newLevel = A_2.newLevel;
				int level = newUpgrade.Level;
				if (!(newLevel.GetValueOrDefault() == level & newLevel != null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(103, 4);
					defaultInterpolatedStringHandler.AppendLiteral("The upgrade ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(newUpgrade.Prefab.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted(target.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" has a different level compared to other items! \n");
					defaultInterpolatedStringHandler.AppendLiteral("Expected level was $");
					defaultInterpolatedStringHandler.AppendFormatted<int?>(A_2.newLevel);
					defaultInterpolatedStringHandler.AppendLiteral(" but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(newUpgrade.Level);
					defaultInterpolatedStringHandler.AppendLiteral(" instead.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), newUpgrade.Prefab.ContentPackage);
				}
			}
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x00042FF8 File Offset: 0x000411F8
		[CompilerGenerated]
		private int <GetUpgradeLevel>g__GetPendingLevel|26_0(ref UpgradeManager.<>c__DisplayClass26_0 A_1)
		{
			PurchasedUpgrade upgrade = this.FindMatchingUpgrade(A_1.prefab, A_1.category);
			if (upgrade == null)
			{
				return 0;
			}
			return upgrade.Level;
		}

		// Token: 0x0400033E RID: 830
		public const bool UpgradeAlsoConnectedSubs = true;

		// Token: 0x0400033F RID: 831
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private List<PurchasedUpgrade> loadedUpgrades;

		// Token: 0x04000340 RID: 832
		public readonly List<PurchasedUpgrade> PurchasedUpgrades = new List<PurchasedUpgrade>();

		// Token: 0x04000341 RID: 833
		public readonly List<PurchasedUpgrade> PendingUpgrades = new List<PurchasedUpgrade>();

		// Token: 0x04000342 RID: 834
		public readonly List<PurchasedItemSwap> PurchasedItemSwaps = new List<PurchasedItemSwap>();

		// Token: 0x04000343 RID: 835
		private readonly CampaignMode Campaign;

		// Token: 0x04000344 RID: 836
		public readonly NamedEvent<UpgradeManager> OnUpgradesChanged = new NamedEvent<UpgradeManager>();

		// Token: 0x04000345 RID: 837
		private DateTime lastUpgradeSpeak;

		// Token: 0x04000346 RID: 838
		private DateTime lastErrorSpeak;

		// Token: 0x04000347 RID: 839
		private static readonly HashSet<Submarine> upgradedSubs = new HashSet<Submarine>();
	}
}
