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
	// Token: 0x02000042 RID: 66
	internal class EntitySpawner : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x06000AC5 RID: 2757 RVA: 0x00069D6C File Offset: 0x00067F6C
		public void CreateNetworkEvent(EntitySpawner.SpawnOrRemove spawnOrRemove)
		{
			this.CreateNetworkEventProjSpecific(spawnOrRemove);
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x00069D78 File Offset: 0x00067F78
		public void ServerEventWrite(IWriteMessage message, Client client, NetEntityEvent.IData extraData = null)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			EntitySpawner.SpawnOrRemove entities = extraData as EntitySpawner.SpawnOrRemove;
			if (entities == null)
			{
				throw new Exception("Malformed EntitySpawner event: expected SpawnOrRemove");
			}
			message.WriteBoolean(entities is EntitySpawner.RemoveEntity);
			if (entities is EntitySpawner.RemoveEntity)
			{
				message.WriteUInt16(entities.ID);
				return;
			}
			Entity entity = entities.Entity;
			Item item = entity as Item;
			if (item != null)
			{
				message.WriteByte(0);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Writing item spawn data ");
				defaultInterpolatedStringHandler.AppendFormatted<Item>(item);
				defaultInterpolatedStringHandler.AppendLiteral(" (ID: ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(entities.ID);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				item.WriteSpawnData(message, entities.ID, entities.InventoryID, entities.ItemContainerIndex, entities.SlotIndex);
				return;
			}
			Character character = entity as Character;
			if (character == null)
			{
				return;
			}
			message.WriteByte(1);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(37, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Writing character spawn data: ");
			defaultInterpolatedStringHandler2.AppendFormatted<Character>(character);
			defaultInterpolatedStringHandler2.AppendLiteral(" (ID: ");
			defaultInterpolatedStringHandler2.AppendFormatted<ushort>(entities.ID);
			defaultInterpolatedStringHandler2.AppendLiteral(")");
			DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
			character.WriteSpawnData(message, entities.ID, true);
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x00069EBC File Offset: 0x000680BC
		public EntitySpawner() : base(null, ushort.MaxValue)
		{
			this.spawnOrRemoveQueue = new Queue<Either<EntitySpawner.IEntitySpawnInfo, Entity>>();
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x00069ED5 File Offset: 0x000680D5
		public override string ToString()
		{
			return "EntitySpawner";
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x00069EDC File Offset: 0x000680DC
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

		// Token: 0x06000ACA RID: 2762 RVA: 0x00069F48 File Offset: 0x00068148
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

		// Token: 0x06000ACB RID: 2763 RVA: 0x00069FB8 File Offset: 0x000681B8
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

		// Token: 0x06000ACC RID: 2764 RVA: 0x0006A03C File Offset: 0x0006823C
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

		// Token: 0x06000ACD RID: 2765 RVA: 0x0006A0AC File Offset: 0x000682AC
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

		// Token: 0x06000ACE RID: 2766 RVA: 0x0006A11C File Offset: 0x0006831C
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

		// Token: 0x06000ACF RID: 2767 RVA: 0x0006A18C File Offset: 0x0006838C
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
				if (GameMain.Server != null)
				{
					Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
					if (client != null)
					{
						GameMain.Server.SetClientCharacter(client, null);
					}
				}
			}
			this.spawnOrRemoveQueue.Enqueue(entity);
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x0006A23C File Offset: 0x0006843C
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

		// Token: 0x06000AD1 RID: 2769 RVA: 0x0006A2CC File Offset: 0x000684CC
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

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0006A334 File Offset: 0x00068534
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

		// Token: 0x06000AD3 RID: 2771 RVA: 0x0006A39C File Offset: 0x0006859C
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

		// Token: 0x06000AD4 RID: 2772 RVA: 0x0006A400 File Offset: 0x00068600
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
					if (createNetworkEvents)
					{
						this.CreateNetworkEventProjSpecific(new EntitySpawner.RemoveEntity(entityToRemove));
					}
					entityToRemove.Remove();
				}
				else if (spawnOrRemove.TryGet(out spawnInfo))
				{
					Entity spawnedEntity = spawnInfo.Spawn();
					if (spawnedEntity != null)
					{
						if (createNetworkEvents)
						{
							this.CreateNetworkEventProjSpecific(new EntitySpawner.SpawnEntity(spawnedEntity));
						}
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

		// Token: 0x06000AD5 RID: 2773 RVA: 0x0006A4C0 File Offset: 0x000686C0
		private void CreateNetworkEventProjSpecific(EntitySpawner.SpawnOrRemove spawnOrRemove)
		{
			if (GameMain.Server == null || ((spawnOrRemove != null) ? spawnOrRemove.Entity : null) == null)
			{
				return;
			}
			GameMain.Server.CreateEntityEvent(this, spawnOrRemove);
			if (spawnOrRemove is EntitySpawner.SpawnEntity)
			{
				Character character = spawnOrRemove.Entity as Character;
				if (character != null && character.Info != null && !character.Removed)
				{
					foreach (StatTypes statKey in character.Info.SavedStatValues.Keys)
					{
						GameMain.NetworkMember.CreateEntityEvent(character, new Character.UpdatePermanentStatsEventData(statKey));
					}
				}
			}
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x0006A578 File Offset: 0x00068778
		public void Reset()
		{
			this.spawnOrRemoveQueue.Clear();
		}

		// Token: 0x040004B2 RID: 1202
		private readonly Queue<Either<EntitySpawner.IEntitySpawnInfo, Entity>> spawnOrRemoveQueue;

		// Token: 0x02000730 RID: 1840
		private enum SpawnableType
		{
			// Token: 0x04002C2F RID: 11311
			Item,
			// Token: 0x04002C30 RID: 11312
			Character
		}

		// Token: 0x02000731 RID: 1841
		public interface IEntitySpawnInfo
		{
			// Token: 0x0600511D RID: 20765
			Entity Spawn();

			// Token: 0x0600511E RID: 20766
			void OnSpawned(Entity entity);
		}

		// Token: 0x02000732 RID: 1842
		public class ItemSpawnInfo : EntitySpawner.IEntitySpawnInfo
		{
			// Token: 0x0600511F RID: 20767 RVA: 0x001E8983 File Offset: 0x001E6B83
			public ItemSpawnInfo(ItemPrefab prefab, Vector2 worldPosition, Action<Item> onSpawned, float? condition = null, int? quality = null) : this(prefab, onSpawned, condition, quality)
			{
				this.Position = worldPosition;
			}

			// Token: 0x06005120 RID: 20768 RVA: 0x001E8998 File Offset: 0x001E6B98
			public ItemSpawnInfo(ItemPrefab prefab, Vector2 position, Submarine sub, Action<Item> onSpawned, float? condition = null, int? quality = null) : this(prefab, onSpawned, condition, quality)
			{
				this.Position = position;
				this.Submarine = sub;
			}

			// Token: 0x06005121 RID: 20769 RVA: 0x001E89B5 File Offset: 0x001E6BB5
			public ItemSpawnInfo(ItemPrefab prefab, Inventory inventory, Action<Item> onSpawned, float? condition = null, int? quality = null) : this(prefab, onSpawned, condition, quality)
			{
				this.Inventory = inventory;
			}

			// Token: 0x06005122 RID: 20770 RVA: 0x001E89CC File Offset: 0x001E6BCC
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

			// Token: 0x06005123 RID: 20771 RVA: 0x001E8A48 File Offset: 0x001E6C48
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

			// Token: 0x06005124 RID: 20772 RVA: 0x001E8BA8 File Offset: 0x001E6DA8
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

			// Token: 0x06005125 RID: 20773 RVA: 0x001E8BFC File Offset: 0x001E6DFC
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

			// Token: 0x04002C31 RID: 11313
			public readonly ItemPrefab Prefab;

			// Token: 0x04002C32 RID: 11314
			public readonly Vector2 Position;

			// Token: 0x04002C33 RID: 11315
			public readonly Inventory Inventory;

			// Token: 0x04002C34 RID: 11316
			public readonly Submarine Submarine;

			// Token: 0x04002C35 RID: 11317
			public readonly Option<float> Condition;

			// Token: 0x04002C36 RID: 11318
			public readonly Option<int> Quality;

			// Token: 0x04002C37 RID: 11319
			public bool SpawnIfInventoryFull = true;

			// Token: 0x04002C38 RID: 11320
			public bool IgnoreLimbSlots;

			// Token: 0x04002C39 RID: 11321
			public InvSlotType Slot;

			// Token: 0x04002C3A RID: 11322
			private readonly Action<Item> onSpawned;
		}

		// Token: 0x02000733 RID: 1843
		private class CharacterSpawnInfo : EntitySpawner.IEntitySpawnInfo
		{
			// Token: 0x06005126 RID: 20774 RVA: 0x001E8C35 File Offset: 0x001E6E35
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

			// Token: 0x06005127 RID: 20775 RVA: 0x001E8C66 File Offset: 0x001E6E66
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

			// Token: 0x06005128 RID: 20776 RVA: 0x001E8C9F File Offset: 0x001E6E9F
			public CharacterSpawnInfo(Identifier identifier, Vector2 position, CharacterInfo characterInfo, Action<Character> onSpawn = null) : this(identifier, position, onSpawn)
			{
				this.CharacterInfo = characterInfo;
			}

			// Token: 0x06005129 RID: 20777 RVA: 0x001E8CB4 File Offset: 0x001E6EB4
			public Entity Spawn()
			{
				return this.Identifier.IsEmpty ? null : Character.Create(this.Identifier, (this.Submarine == null) ? this.Position : (this.Submarine.Position + this.Position), ToolBox.RandomSeed(8), this.CharacterInfo, 0, false, true, false, null, true, true);
			}

			// Token: 0x0600512A RID: 20778 RVA: 0x001E8D18 File Offset: 0x001E6F18
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

			// Token: 0x04002C3B RID: 11323
			public readonly Identifier Identifier;

			// Token: 0x04002C3C RID: 11324
			public readonly CharacterInfo CharacterInfo;

			// Token: 0x04002C3D RID: 11325
			public readonly Vector2 Position;

			// Token: 0x04002C3E RID: 11326
			public readonly Submarine Submarine;

			// Token: 0x04002C3F RID: 11327
			private readonly Action<Character> onSpawned;
		}

		// Token: 0x02000734 RID: 1844
		private class SubmarineSpawnInfo : EntitySpawner.IEntitySpawnInfo
		{
			// Token: 0x0600512B RID: 20779 RVA: 0x001E8D6A File Offset: 0x001E6F6A
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

			// Token: 0x0600512C RID: 20780 RVA: 0x001E8D98 File Offset: 0x001E6F98
			public Entity Spawn()
			{
				return string.IsNullOrEmpty(this.Name) ? null : new Submarine(SubmarineInfo.SavedSubmarines.First((SubmarineInfo s) => s.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase)), true, null, null);
			}

			// Token: 0x0600512D RID: 20781 RVA: 0x001E8DD8 File Offset: 0x001E6FD8
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

			// Token: 0x04002C40 RID: 11328
			public readonly string Name;

			// Token: 0x04002C41 RID: 11329
			public readonly Vector2 Position;

			// Token: 0x04002C42 RID: 11330
			private readonly Action<Character> onSpawned;
		}

		// Token: 0x02000735 RID: 1845
		public abstract class SpawnOrRemove : NetEntityEvent.IData
		{
			// Token: 0x17001423 RID: 5155
			// (get) Token: 0x0600512F RID: 20783 RVA: 0x001E8E3E File Offset: 0x001E703E
			public ushort ID
			{
				get
				{
					return this.Entity.ID;
				}
			}

			// Token: 0x06005130 RID: 20784 RVA: 0x001E8E4C File Offset: 0x001E704C
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

			// Token: 0x06005131 RID: 20785 RVA: 0x001E8EEC File Offset: 0x001E70EC
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

			// Token: 0x04002C43 RID: 11331
			public readonly Entity Entity;

			// Token: 0x04002C44 RID: 11332
			public readonly ushort InventoryID;

			// Token: 0x04002C45 RID: 11333
			public readonly byte ItemContainerIndex;

			// Token: 0x04002C46 RID: 11334
			public readonly int SlotIndex;
		}

		// Token: 0x02000736 RID: 1846
		public sealed class SpawnEntity : EntitySpawner.SpawnOrRemove
		{
			// Token: 0x06005132 RID: 20786 RVA: 0x001E8FC8 File Offset: 0x001E71C8
			public SpawnEntity(Entity entity) : base(entity)
			{
			}

			// Token: 0x06005133 RID: 20787 RVA: 0x001E8FD1 File Offset: 0x001E71D1
			public override string ToString()
			{
				return "Spawn " + base.ToString();
			}
		}

		// Token: 0x02000737 RID: 1847
		public sealed class RemoveEntity : EntitySpawner.SpawnOrRemove
		{
			// Token: 0x06005134 RID: 20788 RVA: 0x001E8FE3 File Offset: 0x001E71E3
			public RemoveEntity(Entity entity) : base(entity)
			{
			}

			// Token: 0x06005135 RID: 20789 RVA: 0x001E8FEC File Offset: 0x001E71EC
			public override string ToString()
			{
				return "Remove " + base.ToString();
			}
		}
	}
}
