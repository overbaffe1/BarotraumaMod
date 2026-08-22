using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001D0 RID: 464
	internal static class AutoItemPlacer
	{
		// Token: 0x0600225A RID: 8794 RVA: 0x000E6C28 File Offset: 0x000E4E28
		public static void SpawnItems(Identifier? startItemSet = null)
		{
			if (GameMain.NetworkMember != null && !GameMain.NetworkMember.IsServer)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.OwnedSubmarines : null) == null || GameMain.GameSession.OwnedSubmarines.Count <= 1)
			{
				for (int i = 0; i < Submarine.MainSubs.Length; i++)
				{
					Submarine sub = Submarine.MainSubs[i];
					if (sub != null && !sub.Info.InitialSuppliesSpawned && !sub.Info.IsManuallyOutfitted && sub.Info.IsPlayer)
					{
						AutoItemPlacer.SpawnStartItems(sub, startItemSet);
						IEnumerable<Submarine> subs = from s in sub.GetConnectedSubs()
						where s.TeamID == sub.TeamID
						select s;
						AutoItemPlacer.CreateAndPlace(subs, null, 0f);
						subs.ForEach(delegate(Submarine s)
						{
							s.Info.InitialSuppliesSpawned = true;
						});
						sub.CheckFuel();
					}
				}
			}
			foreach (Submarine sub3 in Submarine.Loaded)
			{
				SubmarineType type = sub3.Info.Type;
				bool flag = type <= SubmarineType.OutpostModule;
				if (!flag && !sub3.Info.InitialSuppliesSpawned)
				{
					AutoItemPlacer.CreateAndPlace(sub3.ToEnumerable<Submarine>(), null, 0f);
					sub3.Info.InitialSuppliesSpawned = true;
				}
			}
			Level loaded = Level.Loaded;
			if (((loaded != null) ? loaded.StartOutpost : null) != null && Level.Loaded.Type == LevelData.LevelType.Outpost)
			{
				Submarine sub2 = Level.Loaded.StartOutpost;
				if (!sub2.Info.InitialSuppliesSpawned)
				{
					Rand.SetSyncedSeed(ToolBox.StringToInt(sub2.Info.Name));
					AutoItemPlacer.CreateAndPlace(sub2.ToEnumerable<Submarine>(), null, 0f);
					sub2.Info.InitialSuppliesSpawned = true;
				}
			}
		}

		// Token: 0x0600225B RID: 8795 RVA: 0x000E6E4C File Offset: 0x000E504C
		public static IEnumerable<Item> RegenerateLoot(Submarine sub, ItemContainer regeneratedContainer, float skipItemProbability = 0f)
		{
			return AutoItemPlacer.CreateAndPlace(sub.ToEnumerable<Submarine>(), regeneratedContainer, skipItemProbability);
		}

		// Token: 0x0600225C RID: 8796 RVA: 0x000E6E5C File Offset: 0x000E505C
		private static void SpawnStartItems(Submarine sub, Identifier? startItemSet)
		{
			Identifier setIdentifier = startItemSet ?? AutoItemPlacer.DefaultStartItemSet;
			StartItemSet itemSet;
			if (!StartItemSet.Sets.TryGet(setIdentifier, out itemSet))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Couldn't find a start item set matching the identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(setIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\"!");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				StartItemSet defaultSet;
				if (!StartItemSet.Sets.TryGet(AutoItemPlacer.DefaultStartItemSet, out defaultSet))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Couldn't find the default start item set \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(AutoItemPlacer.DefaultStartItemSet);
					defaultInterpolatedStringHandler2.AppendLiteral("\"!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					return;
				}
				itemSet = defaultSet;
			}
			WayPoint wp = WayPoint.GetRandom(SpawnType.Cargo, null, sub, false, null, false);
			ISpatialEntity initialSpawnPos;
			if (((wp != null) ? wp.CurrentHull : null) == null)
			{
				Hull spawnHull = (from h in Hull.HullList
				where h.Submarine == sub && !h.IsWetRoom
				select h).GetRandomUnsynced<Hull>();
				if (spawnHull == null)
				{
					DebugConsole.AddWarning("Failed to spawn start items in the sub. No cargo waypoint or dry hulls found to spawn the items in.", null);
					return;
				}
				initialSpawnPos = spawnHull;
			}
			else
			{
				initialSpawnPos = wp;
			}
			List<Item> newItems = new List<Item>();
			foreach (StartItem startItem in itemSet.Items)
			{
				ItemPrefab itemPrefab;
				if (!ItemPrefab.Prefabs.TryGet(startItem.Item, out itemPrefab))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(52, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Cannot find a start item with with the identifier \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(startItem.Item);
					defaultInterpolatedStringHandler3.AppendLiteral("\"");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
				}
				else
				{
					if (startItem.MultiPlayerOnly)
					{
						GameSession gameSession = GameMain.GameSession;
						GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
						if (gameMode != null && gameMode.IsSinglePlayer)
						{
							continue;
						}
					}
					for (int i = 0; i < startItem.Amount; i++)
					{
						Item item = new Item(itemPrefab, initialSpawnPos.Position, sub, 0, false);
						foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
						{
							wifiComponent.TeamID = sub.TeamID;
						}
						newItems.Add(item);
					}
				}
			}
			List<ItemContainer> cargoContainers = new List<ItemContainer>();
			foreach (Item item2 in newItems)
			{
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item2));
				foreach (ItemComponent ic in item2.Components)
				{
					ic.OnItemLoaded();
				}
				Item container = sub.FindContainerFor(item2, true, false, false);
				if (container == null)
				{
					ItemContainer cargoContainer = CargoManager.GetOrCreateCargoContainerFor(item2.Prefab, initialSpawnPos, ref cargoContainers);
					container = ((cargoContainer != null) ? cargoContainer.Item : null);
				}
				if (container != null)
				{
					container.OwnInventory.TryPutItem(item2, null, null, true, false, true);
				}
			}
		}

		// Token: 0x0600225D RID: 8797 RVA: 0x000E71A8 File Offset: 0x000E53A8
		private static IEnumerable<Item> CreateAndPlace(IEnumerable<Submarine> subs, ItemContainer regeneratedContainer = null, float skipItemProbability = 0f)
		{
			AutoItemPlacer.<>c__DisplayClass5_0 CS$<>8__locals1 = new AutoItemPlacer.<>c__DisplayClass5_0();
			CS$<>8__locals1.skipItemProbability = skipItemProbability;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				DebugConsole.ThrowError("Clients are not allowed to use AutoItemPlacer.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return Enumerable.Empty<Item>();
			}
			CS$<>8__locals1.itemsToSpawn = new List<Item>(100);
			int itemCountApprox = MapEntityPrefab.List.Count<MapEntityPrefab>() / 3;
			CS$<>8__locals1.containers = new List<ItemContainer>(70 + 30 * subs.Count<Submarine>());
			List<ItemPrefab> prefabsItemsCanSpawnIn = new List<ItemPrefab>(itemCountApprox / 3);
			List<ItemPrefab> singlePrefabs = new List<ItemPrefab>(itemCountApprox);
			List<ItemPrefab> removals = new List<ItemPrefab>();
			if (regeneratedContainer != null)
			{
				CS$<>8__locals1.containers.Add(regeneratedContainer);
			}
			else
			{
				foreach (Item item in Item.ItemList)
				{
					if (subs.Contains(item.Submarine) && !(item.GetRootInventoryOwner() is Character) && !item.NonInteractable)
					{
						CS$<>8__locals1.containers.AddRange(item.GetComponents<ItemContainer>());
					}
				}
				CS$<>8__locals1.containers.Shuffle(Rand.RandSync.ServerAndClient);
			}
			IOrderedEnumerable<ItemPrefab> itemPrefabs = from p in ItemPrefab.Prefabs
			orderby p.UintIdentifier
			select p;
			using (IEnumerator<ItemPrefab> enumerator2 = itemPrefabs.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ItemPrefab ip = enumerator2.Current;
					if (!ip.PreferredContainers.None(null))
					{
						if (ip.ConfigElement.Elements().Any((ContentXElement e) => string.Equals(e.Name.ToString(), typeof(ItemContainer).Name.ToString(), StringComparison.OrdinalIgnoreCase)) && itemPrefabs.Any((ItemPrefab ip2) => AutoItemPlacer.<CreateAndPlace>g__CanSpawnIn|5_1(ip2, ip)))
						{
							prefabsItemsCanSpawnIn.Add(ip);
						}
						else
						{
							singlePrefabs.Add(ip);
						}
					}
				}
			}
			CS$<>8__locals1.validContainers = new Dictionary<ItemContainer, PreferredContainer>();
			prefabsItemsCanSpawnIn.Shuffle(Rand.RandSync.ServerAndClient);
			for (int j = 0; j < prefabsItemsCanSpawnIn.Count; j++)
			{
				ItemPrefab itemPrefab = prefabsItemsCanSpawnIn[j];
				if (itemPrefab != null)
				{
					CS$<>8__locals1.<CreateAndPlace>g__SpawnItems|3(itemPrefab, CS$<>8__locals1.skipItemProbability);
				}
			}
			singlePrefabs.Shuffle(Rand.RandSync.ServerAndClient);
			singlePrefabs.ForEach(delegate(ItemPrefab i)
			{
				base.<CreateAndPlace>g__SpawnItems|3(i, CS$<>8__locals1.skipItemProbability);
			});
			if (AutoItemPlacer.OutputDebugInfo)
			{
				List<string> subNames = (from s in subs
				select s.Info.Name).ToList<string>();
				DebugConsole.NewMessage("Automatically placed items in " + string.Join(", ", subNames) + ":", null, false);
				using (IEnumerator<string> enumerator3 = (from it in CS$<>8__locals1.itemsToSpawn
				select it.Name).Distinct<string>().GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						string itemName = enumerator3.Current;
						DebugConsole.NewMessage(" - " + itemName + " x" + CS$<>8__locals1.itemsToSpawn.Count((Item it) => it.Name == itemName).ToString(), null, false);
					}
				}
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Level : null) != null && GameMain.GameSession.Level.Type == LevelData.LevelType.Outpost)
			{
				Location startLocation = GameMain.GameSession.StartLocation;
				if (((startLocation != null) ? startLocation.TakenItems : null) != null)
				{
					using (IEnumerator<Location.TakenItem> enumerator4 = GameMain.GameSession.StartLocation.TakenItems.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							Location.TakenItem takenItem = enumerator4.Current;
							Item matchingItem = CS$<>8__locals1.itemsToSpawn.Find((Item it) => takenItem.Matches(it));
							if (matchingItem != null)
							{
								if (AutoItemPlacer.OutputDebugInfo)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
									defaultInterpolatedStringHandler.AppendLiteral("Removing the stolen item: ");
									defaultInterpolatedStringHandler.AppendFormatted<Identifier>(matchingItem.Prefab.Identifier);
									defaultInterpolatedStringHandler.AppendLiteral(" (");
									defaultInterpolatedStringHandler.AppendFormatted<ushort>(matchingItem.ID);
									defaultInterpolatedStringHandler.AppendLiteral(")");
									DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
								}
								List<Item> containedItems = CS$<>8__locals1.itemsToSpawn.FindAll(delegate(Item it)
								{
									Inventory parentInventory = it.ParentInventory;
									return ((parentInventory != null) ? parentInventory.Owner : null) == matchingItem;
								});
								matchingItem.Remove();
								CS$<>8__locals1.itemsToSpawn.Remove(matchingItem);
								foreach (Item containedItem in containedItems)
								{
									containedItem.Remove();
									CS$<>8__locals1.itemsToSpawn.Remove(containedItem);
								}
							}
						}
					}
				}
			}
			foreach (Item item2 in CS$<>8__locals1.itemsToSpawn)
			{
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item2));
				foreach (ItemComponent ic in item2.Components)
				{
					ic.OnItemLoaded();
				}
			}
			return CS$<>8__locals1.itemsToSpawn;
		}

		// Token: 0x0600225E RID: 8798 RVA: 0x000E77F8 File Offset: 0x000E59F8
		private static Dictionary<ItemContainer, PreferredContainer> GetValidContainers(PreferredContainer preferredContainer, IEnumerable<ItemContainer> allContainers, Dictionary<ItemContainer, PreferredContainer> validContainers, bool primary)
		{
			validContainers.Clear();
			foreach (ItemContainer container in allContainers)
			{
				if (container.AutoFill)
				{
					if (primary)
					{
						if (!ItemPrefab.IsContainerPreferred(preferredContainer.Primary, container))
						{
							continue;
						}
					}
					else if (!ItemPrefab.IsContainerPreferred(preferredContainer.Secondary, container))
					{
						continue;
					}
					if (!validContainers.ContainsKey(container))
					{
						validContainers.Add(container, preferredContainer);
					}
				}
			}
			return validContainers;
		}

		// Token: 0x0600225F RID: 8799 RVA: 0x000E787C File Offset: 0x000E5A7C
		private static List<Item> CreateItems(ItemPrefab itemPrefab, List<ItemContainer> containers, KeyValuePair<ItemContainer, PreferredContainer> validContainer)
		{
			List<Item> newItems = new List<Item>();
			if (Rand.Value(Rand.RandSync.ServerAndClient) > validContainer.Value.SpawnProbability)
			{
				return newItems;
			}
			if (validContainer.Key.Item.Submarine.WreckAI != null && itemPrefab.Tags.Contains("explodesinwater"))
			{
				return newItems;
			}
			int amount = validContainer.Value.Amount;
			if (amount == 0)
			{
				amount = Rand.Range(validContainer.Value.MinAmount, validContainer.Value.MaxAmount + 1, Rand.RandSync.ServerAndClient);
			}
			Func<Item, bool> <>9__0;
			Func<Item, bool> <>9__1;
			for (int i = 0; i < amount; i++)
			{
				if (validContainer.Key.Inventory.IsFull(true))
				{
					containers.Remove(validContainer.Key);
					break;
				}
				IEnumerable<Item> allItems = validContainer.Key.Inventory.AllItems;
				Func<Item, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((Item it) => it.Prefab == itemPrefab));
				}
				Item existingItem = allItems.FirstOrDefault(predicate);
				int quality = (existingItem != null) ? existingItem.Quality : Quality.GetSpawnedItemQuality(validContainer.Key.Item.Submarine, Level.Loaded, Rand.RandSync.ServerAndClient);
				Inventory inventory = validContainer.Key.Inventory;
				ItemPrefab itemPrefab2 = itemPrefab;
				int? quality2 = new int?(quality);
				if (!inventory.CanProbablyBePut(itemPrefab2, null, quality2))
				{
					break;
				}
				Item item2 = new Item(itemPrefab, validContainer.Key.Item.Position, validContainer.Key.Item.Submarine, 0, false);
				item2.SpawnedInCurrentOutpost = validContainer.Key.Item.SpawnedInCurrentOutpost;
				item2.AllowStealing = (validContainer.Key.Item.AllowStealing || validContainer.Key.Item.Prefab.AllowStealingContainedItems);
				item2.Quality = quality;
				item2.OriginalModuleIndex = validContainer.Key.Item.OriginalModuleIndex;
				IEnumerable<Item> itemList = Item.ItemList;
				Func<Item, bool> predicate2;
				if ((predicate2 = <>9__1) == null)
				{
					predicate2 = (<>9__1 = ((Item it) => it.Submarine == validContainer.Key.Item.Submarine && it.OriginalModuleIndex == validContainer.Key.Item.OriginalModuleIndex));
				}
				item2.OriginalContainerIndex = itemList.Where(predicate2).ToList<Item>().IndexOf(validContainer.Key.Item);
				Item item = item2;
				foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
				{
					wifiComponent.TeamID = validContainer.Key.Item.Submarine.TeamID;
				}
				newItems.Add(item);
				validContainer.Key.Inventory.TryPutItem(item, null, null, false, false, true);
				containers.AddRange(item.GetComponents<ItemContainer>());
			}
			return newItems;
		}

		// Token: 0x06002261 RID: 8801 RVA: 0x000E7BC0 File Offset: 0x000E5DC0
		[CompilerGenerated]
		internal static bool <CreateAndPlace>g__CanSpawnIn|5_1(ItemPrefab item, ItemPrefab container)
		{
			foreach (PreferredContainer preferredContainer in item.PreferredContainers)
			{
				if (ItemPrefab.IsContainerPreferred(preferredContainer.Primary, container.Identifier.ToEnumerable<Identifier>().Union(container.Tags)))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400106F RID: 4207
		public static bool OutputDebugInfo = false;

		// Token: 0x04001070 RID: 4208
		public static Identifier DefaultStartItemSet = new Identifier("normal");
	}
}
