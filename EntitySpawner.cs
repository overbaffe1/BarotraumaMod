using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000F1 RID: 241
	internal class EntitySpawner : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x060022D2 RID: 8914 RVA: 0x001620D8 File Offset: 0x001602D8
		public void ClientEventRead(IReadMessage message, float sendingTime)
		{
			bool remove = message.ReadBoolean();
			if (remove)
			{
				ushort entityId = message.ReadUInt16();
				Entity entity = Entity.FindEntityByID(entityId);
				if (entity != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Received entity removal message for \"");
					defaultInterpolatedStringHandler.AppendFormatted<Entity>(entity);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					Item item = entity as Item;
					if (item != null)
					{
						Item container = item.Container;
						if (((container != null) ? container.GetComponent<Deconstructor>() : null) != null && item.Prefab.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
						{
							string str = "ItemDeconstructed:";
							GameSession gameSession = GameMain.GameSession;
							Identifier? identifier;
							if (gameSession == null)
							{
								identifier = null;
							}
							else
							{
								GameMode gameMode = gameSession.GameMode;
								identifier = ((gameMode != null) ? new Identifier?(gameMode.Preset.Identifier) : null);
							}
							GameAnalyticsManager.AddDesignEvent(str + (identifier ?? "none".ToIdentifier()).ToString() + ":" + item.Prefab.Identifier.ToString());
						}
					}
					entity.Remove();
				}
				else
				{
					DebugConsole.Log("Received entity removal message for ID " + entityId.ToString() + ". Entity with a matching ID not found.");
				}
				this.receivedEvents.Add(new ValueTuple<Entity, bool>(entity, true));
				return;
			}
			byte b = message.ReadByte();
			if (b != 0)
			{
				if (b != 1)
				{
					DebugConsole.ThrowError("Received invalid entity spawn message (unknown spawnable type)", null, null, false, false);
					return;
				}
				Character character = Character.ReadSpawnData(message);
				if (character == null)
				{
					DebugConsole.ThrowError("Received character spawn message, but spawning the character failed.", null, null, false, false);
					return;
				}
				this.receivedEvents.Add(new ValueTuple<Entity, bool>(character, false));
				return;
			}
			else
			{
				Item newItem = Item.ReadSpawnData(message, true);
				if (newItem == null)
				{
					DebugConsole.ThrowError("Received an item spawn message, but spawning the item failed.", null, null, false, false);
					return;
				}
				Item container2 = newItem.Container;
				if (((container2 != null) ? container2.GetComponent<Fabricator>() : null) != null && newItem.Prefab.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
				{
					string str2 = "ItemFabricated:";
					GameSession gameSession2 = GameMain.GameSession;
					Identifier? identifier2;
					if (gameSession2 == null)
					{
						identifier2 = null;
					}
					else
					{
						GameMode gameMode2 = gameSession2.GameMode;
						identifier2 = ((gameMode2 != null) ? new Identifier?(gameMode2.Preset.Identifier) : null);
					}
					GameAnalyticsManager.AddDesignEvent(str2 + (identifier2 ?? "none".ToIdentifier()).ToString() + ":" + newItem.Prefab.Identifier.ToString());
				}
				this.receivedEvents.Add(new ValueTuple<Entity, bool>(newItem, false));
				return;
			}
		}

		// Token: 0x060022D3 RID: 8915 RVA: 0x001623B4 File Offset: 0x001605B4
		public EntitySpawner() : base(null, ushort.MaxValue)
		{
			this.spawnOrRemoveQueue = new Queue<Either<EntitySpawner.IEntitySpawnInfo, Entity>>();
		}

		// Token: 0x060022D4 RID: 8916 RVA: 0x001623D8 File Offset: 0x001605D8
		public override string ToString()
		{
			return "EntitySpawner";
		}

		// Token: 0x060022D5 RID: 8917 RVA: 0x001623E0 File Offset: 0x001605E0
		public void AddItemToSpawnQueue(ItemPrefab itemPrefab, Vector2 worldPosition, float? condition = null, int? quality = null, Action<Item> onSpawned = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (itemPrefab == null)
			{
				string errorMsg = "Attempted to add a null item to entity spawn queue.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("EntitySpawner.AddToSpawnQueue1:ItemPrefabNull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(new EntitySpawner.ItemSpawnInfo(itemPrefab, worldPosition, onSpawned, condition, quality));
		}

		// Token: 0x060022D6 RID: 8918 RVA: 0x0016244C File Offset: 0x0016064C
		public void AddItemToSpawnQueue(ItemPrefab itemPrefab, Vector2 position, Submarine sub, float? condition = null, int? quality = null, Action<Item> onSpawned = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (itemPrefab == null)
			{
				string errorMsg = "Attempted to add a null item to entity spawn queue.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("EntitySpawner.AddToSpawnQueue2:ItemPrefabNull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(new EntitySpawner.ItemSpawnInfo(itemPrefab, position, sub, onSpawned, condition, quality));
		}

		// Token: 0x060022D7 RID: 8919 RVA: 0x001624BC File Offset: 0x001606BC
		public void AddItemToSpawnQueue(ItemPrefab itemPrefab, Inventory inventory, float? condition = null, int? quality = null, Action<Item> onSpawned = null, bool spawnIfInventoryFull = true, bool ignoreLimbSlots = false, InvSlotType slot = InvSlotType.None)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (itemPrefab == null)
			{
				string errorMsg = "Attempted to add a null item to entity spawn queue.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("EntitySpawner.AddToSpawnQueue3:ItemPrefabNull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(new EntitySpawner.ItemSpawnInfo(itemPrefab, inventory, onSpawned, condition, quality)
			{
				SpawnIfInventoryFull = spawnIfInventoryFull,
				IgnoreLimbSlots = ignoreLimbSlots,
				Slot = slot
			});
		}

		// Token: 0x060022D8 RID: 8920 RVA: 0x00162540 File Offset: 0x00160740
		public void AddCharacterToSpawnQueue(Identifier speciesName, Vector2 worldPosition, Action<Character> onSpawn = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (speciesName.IsEmpty)
			{
				string errorMsg = "Attempted to add an empty/null species name to entity spawn queue.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("EntitySpawner.AddToSpawnQueue4:SpeciesNameNullOrEmpty", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(new EntitySpawner.CharacterSpawnInfo(speciesName, worldPosition, onSpawn));
		}

		// Token: 0x060022D9 RID: 8921 RVA: 0x001625B0 File Offset: 0x001607B0
		public void AddCharacterToSpawnQueue(Identifier speciesName, Vector2 position, Submarine sub, Action<Character> onSpawn = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (speciesName.IsEmpty)
			{
				string errorMsg = "Attempted to add an empty/null species name to entity spawn queue.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("EntitySpawner.AddToSpawnQueue5:SpeciesNameNullOrEmpty", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(new EntitySpawner.CharacterSpawnInfo(speciesName, position, sub, onSpawn));
		}

		// Token: 0x060022DA RID: 8922 RVA: 0x00162620 File Offset: 0x00160820
		public void AddCharacterToSpawnQueue(Identifier speciesName, Vector2 worldPosition, CharacterInfo characterInfo, Action<Character> onSpawn = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (speciesName.IsEmpty)
			{
				string errorMsg = "Attempted to add an empty/null species name to entity spawn queue.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("EntitySpawner.AddToSpawnQueue4:SpeciesNameNullOrEmpty", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(new EntitySpawner.CharacterSpawnInfo(speciesName, worldPosition, characterInfo, onSpawn));
		}

		// Token: 0x060022DB RID: 8923 RVA: 0x00162690 File Offset: 0x00160890
		public void AddEntityToRemoveQueue(Entity entity)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (entity == null || this.IsInRemoveQueue(entity) || entity.Removed || entity.IdFreed)
			{
				return;
			}
			Item item = entity as Item;
			if (item != null)
			{
				this.AddItemToRemoveQueue(item);
				return;
			}
			if (entity is Character)
			{
				Character character = entity as Character;
			}
			this.spawnOrRemoveQueue.Enqueue(entity);
		}

		// Token: 0x060022DC RID: 8924 RVA: 0x00162700 File Offset: 0x00160900
		public void AddItemToRemoveQueue(Item item)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.IsInRemoveQueue(item) || item.Removed)
			{
				return;
			}
			this.spawnOrRemoveQueue.Enqueue(item);
			item.IsInRemoveQueue = true;
			foreach (Item containedItem in item.ContainedItems)
			{
				if (containedItem != null)
				{
					this.AddItemToRemoveQueue(containedItem);
				}
			}
		}

		// Token: 0x060022DD RID: 8925 RVA: 0x00162790 File Offset: 0x00160990
		public bool IsInSpawnQueue(Predicate<EntitySpawner.IEntitySpawnInfo> predicate)
		{
			foreach (Either<EntitySpawner.IEntitySpawnInfo, Entity> spawnOrRemove in this.spawnOrRemoveQueue)
			{
				EntitySpawner.IEntitySpawnInfo spawnInfo;
				if (spawnOrRemove.TryGet(out spawnInfo) && predicate(spawnInfo))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060022DE RID: 8926 RVA: 0x001627F8 File Offset: 0x001609F8
		public int CountSpawnQueue(Predicate<EntitySpawner.IEntitySpawnInfo> predicate)
		{
			int count = 0;
			foreach (Either<EntitySpawner.IEntitySpawnInfo, Entity> spawnOrRemove in this.spawnOrRemoveQueue)
			{
				EntitySpawner.IEntitySpawnInfo spawnInfo;
				if (spawnOrRemove.TryGet(out spawnInfo) && predicate(spawnInfo))
				{
					count++;
				}
			}
			return count;
		}

		// Token: 0x060022DF RID: 8927 RVA: 0x00162860 File Offset: 0x00160A60
		public bool IsInRemoveQueue(Entity entity)
		{
			foreach (Either<EntitySpawner.IEntitySpawnInfo, Entity> spawnOrRemove in this.spawnOrRemoveQueue)
			{
				Entity entityToRemove;
				if (spawnOrRemove.TryGet(out entityToRemove) && entityToRemove == entity)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060022E0 RID: 8928 RVA: 0x001628C4 File Offset: 0x00160AC4
		public void Update(bool createNetworkEvents = true)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			while (this.spawnOrRemoveQueue.Count > 0)
			{
				Either<EntitySpawner.IEntitySpawnInfo, Entity> spawnOrRemove = this.spawnOrRemoveQueue.Dequeue();
				Entity entityToRemove;
				EntitySpawner.IEntitySpawnInfo spawnInfo;
				if (spawnOrRemove.TryGet(out entityToRemove))
				{
					Item item = entityToRemove as Item;
					if (item != null)
					{
						item.SendPendingNetworkUpdates();
					}
					entityToRemove.Remove();
				}
				else if (spawnOrRemove.TryGet(out spawnInfo))
				{
					Entity spawnedEntity = spawnInfo.Spawn();
					if (spawnedEntity != null)
					{
						spawnInfo.OnSpawned(spawnedEntity);
						GameSession gameSession = GameMain.GameSession;
						if (gameSession != null)
						{
							EventManager eventManager = gameSession.EventManager;
							if (eventManager != null)
							{
								eventManager.EntitySpawned(spawnedEntity);
							}
						}
					}
				}
			}
		}

		// Token: 0x060022E1 RID: 8929 RVA: 0x00162960 File Offset: 0x00160B60
		public void Reset()
		{
			this.spawnOrRemoveQueue.Clear();
			this.receivedEvents.Clear();
		}

		// Token: 0x04001187 RID: 4487
		[TupleElementNames(new string[]
		{
			"entity",
			"isRemoval"
		})]
		public readonly List<ValueTuple<Entity, bool>> receivedEvents = new List<ValueTuple<Entity, bool>>();

		// Token: 0x04001188 RID: 4488
		private readonly Queue<Either<EntitySpawner.IEntitySpawnInfo, Entity>> spawnOrRemoveQueue;

		// Token: 0x02000BCC RID: 3020
		private enum SpawnableType
		{
			// Token: 0x040048F7 RID: 18679
			Item,
			// Token: 0x040048F8 RID: 18680
			Character
		}

		// Token: 0x02000BCD RID: 3021
		public interface IEntitySpawnInfo
		{
			// Token: 0x060079FD RID: 31229
			Entity Spawn();

			// Token: 0x060079FE RID: 31230
			void OnSpawned(Entity entity);
		}

		// Token: 0x02000BCE RID: 3022
		public class ItemSpawnInfo : EntitySpawner.IEntitySpawnInfo
		{
			// Token: 0x060079FF RID: 31231 RVA: 0x00380294 File Offset: 0x0037E494
			public ItemSpawnInfo(ItemPrefab prefab, Vector2 worldPosition, Action<Item> onSpawned, float? condition = null, int? quality = null) : this(prefab, onSpawned, condition, quality)
			{
				this.Position = worldPosition;
			}

			// Token: 0x06007A00 RID: 31232 RVA: 0x003802A9 File Offset: 0x0037E4A9
			public ItemSpawnInfo(ItemPrefab prefab, Vector2 position, Submarine sub, Action<Item> onSpawned, float? condition = null, int? quality = null) : this(prefab, onSpawned, condition, quality)
			{
				this.Position = position;
				this.Submarine = sub;
			}

			// Token: 0x06007A01 RID: 31233 RVA: 0x003802C6 File Offset: 0x0037E4C6
			public ItemSpawnInfo(ItemPrefab prefab, Inventory inventory, Action<Item> onSpawned, float? condition = null, int? quality = null) : this(prefab, onSpawned, condition, quality)
			{
				this.Inventory = inventory;
			}

			// Token: 0x06007A02 RID: 31234 RVA: 0x003802DC File Offset: 0x0037E4DC
			private ItemSpawnInfo(ItemPrefab prefab, Action<Item> onSpawned, float? condition = null, int? quality = null)
			{
				if (prefab == null)
				{
					throw new ArgumentException("ItemSpawnInfo prefab cannot be null.");
				}
				this.Prefab = prefab;
				this.Condition = ((condition != null) ? Option<float>.Some(condition.Value) : Option<float>.None());
				this.Quality = ((quality != null) ? Option<int>.Some(quality.Value) : Option<int>.None());
				this.onSpawned = onSpawned;
			}

			// Token: 0x06007A03 RID: 31235 RVA: 0x00380358 File Offset: 0x0037E558
			public Entity Spawn()
			{
				if (this.Prefab == null)
				{
					return null;
				}
				Inventory inventory = this.Inventory;
				Item spawnedItem;
				if (((inventory != null) ? inventory.Owner : null) != null)
				{
					if (!this.SpawnIfInventoryFull && !this.Inventory.CanProbablyBePut(this.Prefab, null, null))
					{
						return null;
					}
					spawnedItem = new Item(this.Prefab, this.Inventory.Owner.Position, this.Inventory.Owner.Submarine, 0, true);
					this.<Spawn>g__SetItemProperties|14_0(spawnedItem);
					IEnumerable<InvSlotType> slot = (this.Slot != InvSlotType.None) ? this.Slot.ToEnumerable<InvSlotType>() : spawnedItem.AllowedSlots;
					if (!this.Inventory.Owner.Removed && !this.Inventory.TryPutItem(spawnedItem, null, slot, true, false, true) && this.IgnoreLimbSlots)
					{
						for (int i = 0; i < this.Inventory.Capacity; i++)
						{
							if (this.Inventory.GetItemAt(i) == null)
							{
								this.Inventory.ForceToSlot(spawnedItem, i);
								break;
							}
						}
					}
					Character character = this.Inventory.Owner as Character;
					if (character != null && character.DisabledByEvent)
					{
						spawnedItem.IsActive = false;
					}
				}
				else
				{
					spawnedItem = new Item(this.Prefab, this.Position, this.Submarine, 0, true);
					this.<Spawn>g__SetItemProperties|14_0(spawnedItem);
				}
				return spawnedItem;
			}

			// Token: 0x06007A04 RID: 31236 RVA: 0x003804B8 File Offset: 0x0037E6B8
			public void OnSpawned(Entity spawnedItem)
			{
				Item item = spawnedItem as Item;
				if (item == null)
				{
					throw new ArgumentException("The entity passed to ItemSpawnInfo.OnSpawned must be an Item (value was " + (((spawnedItem != null) ? spawnedItem.ToString() : null) ?? "null") + ").");
				}
				Action<Item> action = this.onSpawned;
				if (action == null)
				{
					return;
				}
				action(item);
			}

			// Token: 0x06007A05 RID: 31237 RVA: 0x0038050C File Offset: 0x0037E70C
			[CompilerGenerated]
			private void <Spawn>g__SetItemProperties|14_0(Item spawnedItem)
			{
				float condition;
				if (this.Condition.TryUnwrap(out condition))
				{
					spawnedItem.Condition = condition;
				}
				int quality;
				if (this.Quality.TryUnwrap(out quality))
				{
					spawnedItem.Quality = quality;
				}
			}

			// Token: 0x040048F9 RID: 18681
			public readonly ItemPrefab Prefab;

			// Token: 0x040048FA RID: 18682
			public readonly Vector2 Position;

			// Token: 0x040048FB RID: 18683
			public readonly Inventory Inventory;

			// Token: 0x040048FC RID: 18684
			public readonly Submarine Submarine;

			// Token: 0x040048FD RID: 18685
			public readonly Option<float> Condition;

			// Token: 0x040048FE RID: 18686
			public readonly Option<int> Quality;

			// Token: 0x040048FF RID: 18687
			public bool SpawnIfInventoryFull = true;

			// Token: 0x04004900 RID: 18688
			public bool IgnoreLimbSlots;

			// Token: 0x04004901 RID: 18689
			public InvSlotType Slot;

			// Token: 0x04004902 RID: 18690
			private readonly Action<Item> onSpawned;
		}

		// Token: 0x02000BCF RID: 3023
		private class CharacterSpawnInfo : EntitySpawner.IEntitySpawnInfo
		{
			// Token: 0x06007A06 RID: 31238 RVA: 0x00380545 File Offset: 0x0037E745
			public CharacterSpawnInfo(Identifier identifier, Vector2 worldPosition, Action<Character> onSpawn = null)
			{
				this.Identifier = identifier;
				if (identifier.IsEmpty)
				{
					throw new ArgumentException("CharacterSpawnInfo identifier cannot be null.");
				}
				this.Position = worldPosition;
				this.onSpawned = onSpawn;
			}

			// Token: 0x06007A07 RID: 31239 RVA: 0x00380576 File Offset: 0x0037E776
			public CharacterSpawnInfo(Identifier identifier, Vector2 position, Submarine sub, Action<Character> onSpawn = null)
			{
				this.Identifier = identifier;
				if (identifier.IsEmpty)
				{
					throw new ArgumentException("CharacterSpawnInfo identifier cannot be null.");
				}
				this.Position = position;
				this.Submarine = sub;
				this.onSpawned = onSpawn;
			}

			// Token: 0x06007A08 RID: 31240 RVA: 0x003805AF File Offset: 0x0037E7AF
			public CharacterSpawnInfo(Identifier identifier, Vector2 position, CharacterInfo characterInfo, Action<Character> onSpawn = null) : this(identifier, position, onSpawn)
			{
				this.CharacterInfo = characterInfo;
			}

			// Token: 0x06007A09 RID: 31241 RVA: 0x003805C4 File Offset: 0x0037E7C4
			public Entity Spawn()
			{
				return this.Identifier.IsEmpty ? null : Character.Create(this.Identifier, (this.Submarine == null) ? this.Position : (this.Submarine.Position + this.Position), ToolBox.RandomSeed(8), this.CharacterInfo, 0, false, true, false, null, true, true);
			}

			// Token: 0x06007A0A RID: 31242 RVA: 0x00380628 File Offset: 0x0037E828
			public void OnSpawned(Entity spawnedCharacter)
			{
				Character character = spawnedCharacter as Character;
				if (character == null)
				{
					throw new ArgumentException("The entity passed to CharacterSpawnInfo.OnSpawned must be a Character (value was " + (((spawnedCharacter != null) ? spawnedCharacter.ToString() : null) ?? "null") + ").");
				}
				Action<Character> action = this.onSpawned;
				if (action == null)
				{
					return;
				}
				action(character);
			}

			// Token: 0x04004903 RID: 18691
			public readonly Identifier Identifier;

			// Token: 0x04004904 RID: 18692
			public readonly CharacterInfo CharacterInfo;

			// Token: 0x04004905 RID: 18693
			public readonly Vector2 Position;

			// Token: 0x04004906 RID: 18694
			public readonly Submarine Submarine;

			// Token: 0x04004907 RID: 18695
			private readonly Action<Character> onSpawned;
		}

		// Token: 0x02000BD0 RID: 3024
		private class SubmarineSpawnInfo : EntitySpawner.IEntitySpawnInfo
		{
			// Token: 0x06007A0B RID: 31243 RVA: 0x0038067A File Offset: 0x0037E87A
			public SubmarineSpawnInfo(string name, Vector2 worldPosition, Action<Character> onSpawn = null)
			{
				if (name == null)
				{
					throw new ArgumentException("ItemSpawnInfo prefab cannot be null.");
				}
				this.Name = name;
				this.Position = worldPosition;
				this.onSpawned = onSpawn;
			}

			// Token: 0x06007A0C RID: 31244 RVA: 0x003806A8 File Offset: 0x0037E8A8
			public Entity Spawn()
			{
				return string.IsNullOrEmpty(this.Name) ? null : new Submarine(SubmarineInfo.SavedSubmarines.First((SubmarineInfo s) => s.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase)), true, null, null);
			}

			// Token: 0x06007A0D RID: 31245 RVA: 0x003806E8 File Offset: 0x0037E8E8
			public void OnSpawned(Entity spawnedCharacter)
			{
				Character character = spawnedCharacter as Character;
				if (character == null)
				{
					throw new ArgumentException("The entity passed to CharacterSpawnInfo.OnSpawned must be a Character (value was " + (((spawnedCharacter != null) ? spawnedCharacter.ToString() : null) ?? "null") + ").");
				}
				Action<Character> action = this.onSpawned;
				if (action == null)
				{
					return;
				}
				action(character);
			}

			// Token: 0x04004908 RID: 18696
			public readonly string Name;

			// Token: 0x04004909 RID: 18697
			public readonly Vector2 Position;

			// Token: 0x0400490A RID: 18698
			private readonly Action<Character> onSpawned;
		}

		// Token: 0x02000BD1 RID: 3025
		public abstract class SpawnOrRemove : NetEntityEvent.IData
		{
			// Token: 0x17001AC3 RID: 6851
			// (get) Token: 0x06007A0F RID: 31247 RVA: 0x0038074E File Offset: 0x0037E94E
			public ushort ID
			{
				get
				{
					return this.Entity.ID;
				}
			}

			// Token: 0x06007A10 RID: 31248 RVA: 0x0038075C File Offset: 0x0037E95C
			public override string ToString()
			{
				string str = "(";
				MapEntity mapEntity = this.Entity as MapEntity;
				string str2 = ((mapEntity != null) ? mapEntity.Name : null) ?? "[NULL]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 3);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.InventoryID);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.SlotIndex);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return str + str2 + defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x06007A11 RID: 31249 RVA: 0x003807FC File Offset: 0x0037E9FC
			protected SpawnOrRemove(Entity entity)
			{
				this.Entity = entity;
				Item item = entity as Item;
				if (item != null)
				{
					Inventory parentInventory = item.ParentInventory;
					if (parentInventory != null && parentInventory.Owner != null)
					{
						this.InventoryID = item.ParentInventory.Owner.ID;
						this.SlotIndex = item.ParentInventory.FindIndex(item);
						if (item.Container == null)
						{
							return;
						}
						foreach (ItemComponent component in item.Container.Components)
						{
							ItemContainer container = component as ItemContainer;
							if (container != null && container.Inventory == item.ParentInventory)
							{
								this.ItemContainerIndex = (byte)item.Container.GetComponentIndex(component);
								break;
							}
						}
						return;
					}
				}
			}

			// Token: 0x0400490B RID: 18699
			public readonly Entity Entity;

			// Token: 0x0400490C RID: 18700
			public readonly ushort InventoryID;

			// Token: 0x0400490D RID: 18701
			public readonly byte ItemContainerIndex;

			// Token: 0x0400490E RID: 18702
			public readonly int SlotIndex;
		}

		// Token: 0x02000BD2 RID: 3026
		public sealed class SpawnEntity : EntitySpawner.SpawnOrRemove
		{
			// Token: 0x06007A12 RID: 31250 RVA: 0x003808D8 File Offset: 0x0037EAD8
			public SpawnEntity(Entity entity) : base(entity)
			{
			}

			// Token: 0x06007A13 RID: 31251 RVA: 0x003808E1 File Offset: 0x0037EAE1
			public override string ToString()
			{
				return "Spawn " + base.ToString();
			}
		}

		// Token: 0x02000BD3 RID: 3027
		public sealed class RemoveEntity : EntitySpawner.SpawnOrRemove
		{
			// Token: 0x06007A14 RID: 31252 RVA: 0x003808F3 File Offset: 0x0037EAF3
			public RemoveEntity(Entity entity) : base(entity)
			{
			}

			// Token: 0x06007A15 RID: 31253 RVA: 0x003808FC File Offset: 0x0037EAFC
			public override string ToString()
			{
				return "Remove " + base.ToString();
			}
		}
	}
}
