using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200003A RID: 58
	internal class Item : MapEntity, IDamageable, ISerializableEntity, IServerSerializable, INetSerializable, IClientSerializable, IIgnorable, ISpatialEntity, IServerPositionSync
	{
		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000737 RID: 1847 RVA: 0x00045BEE File Offset: 0x00043DEE
		public override Sprite Sprite
		{
			get
			{
				MapEntityPrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.Sprite;
			}
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x00045C04 File Offset: 0x00043E04
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			if (extraData == null)
			{
				throw this.<ServerEventWrite>g__error|4_0("event data was null");
			}
			Item.IEventData itemEventData = extraData as Item.IEventData;
			if (itemEventData == null)
			{
				throw this.<ServerEventWrite>g__error|4_0("event data was of the wrong type (\"" + extraData.GetType().Name + "\")");
			}
			msg.WriteRangedInteger((int)itemEventData.EventType, 0, 12);
			if (itemEventData is Item.ComponentStateEventData)
			{
				Item.ComponentStateEventData componentStateEventData = (Item.ComponentStateEventData)itemEventData;
				int componentIndex = this.components.IndexOf(componentStateEventData.Component);
				if (componentIndex < 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
					defaultInterpolatedStringHandler.AppendLiteral("component index out of range (");
					defaultInterpolatedStringHandler.AppendFormatted<int>(componentIndex);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					throw this.<ServerEventWrite>g__error|4_0(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				IServerSerializable serializableComponent = this.components[componentIndex] as IServerSerializable;
				if (serializableComponent == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(39, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("component \"");
					defaultInterpolatedStringHandler2.AppendFormatted<ItemComponent>(this.components[componentIndex]);
					defaultInterpolatedStringHandler2.AppendLiteral("\" is not server serializable");
					throw this.<ServerEventWrite>g__error|4_0(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				msg.WriteRangedInteger(componentIndex, 0, this.components.Count - 1);
				serializableComponent.ServerEventWrite(msg, c, extraData);
				return;
			}
			else if (itemEventData is Item.InventoryStateEventData)
			{
				Item.InventoryStateEventData inventoryStateEventData = (Item.InventoryStateEventData)itemEventData;
				int containerIndex = this.components.IndexOf(inventoryStateEventData.Component);
				if (containerIndex < 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(31, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("container index out of range (");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(containerIndex);
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					throw this.<ServerEventWrite>g__error|4_0(defaultInterpolatedStringHandler3.ToStringAndClear());
				}
				ItemContainer itemContainer = this.components[containerIndex] as ItemContainer;
				if (itemContainer == null)
				{
					string str = "component \"";
					ItemComponent itemComponent = this.components[containerIndex];
					throw this.<ServerEventWrite>g__error|4_0(str + ((itemComponent != null) ? itemComponent.ToString() : null) + "\" is not server serializable");
				}
				msg.WriteRangedInteger(containerIndex, 0, this.components.Count - 1);
				ServerEntityEvent serverEntityEvent = GameMain.Server.EntityEventManager.Events.Last<ServerEntityEvent>();
				msg.WriteUInt16((serverEntityEvent != null) ? serverEntityEvent.ID : 0);
				itemContainer.Inventory.ServerEventWrite(msg, c, inventoryStateEventData);
				return;
			}
			else
			{
				if (itemEventData is Item.ItemStatusEventData)
				{
					Item.ItemStatusEventData statusEvent = (Item.ItemStatusEventData)itemEventData;
					msg.WriteBoolean(statusEvent.LoadingRound);
					msg.WriteSingle(this.condition);
					return;
				}
				if (itemEventData is Item.AssignCampaignInteractionEventData)
				{
					Item.AssignCampaignInteractionEventData campaignInteractionData = (Item.AssignCampaignInteractionEventData)itemEventData;
					bool isVisibleToClient = new ImmutableArray<Client>?(campaignInteractionData.TargetClients) == null || campaignInteractionData.TargetClients.IsEmpty || campaignInteractionData.TargetClients.Contains(c);
					msg.WriteBoolean(isVisibleToClient);
					if (isVisibleToClient)
					{
						msg.WriteByte((byte)this.CampaignInteractionType);
						return;
					}
				}
				else
				{
					if (!(itemEventData is Item.ApplyStatusEffectEventData))
					{
						Item.SetItemStatEventData setItemStatEventData;
						Item.UpgradeEventData upgradeEventData;
						Item.DroppedStackEventData droppedStackEventData;
						if (itemEventData is Item.ChangePropertyEventData)
						{
							Item.ChangePropertyEventData changePropertyEventData = (Item.ChangePropertyEventData)itemEventData;
							try
							{
								this.WritePropertyChange(msg, changePropertyEventData, !GameMain.NetworkMember.IsServer);
								return;
							}
							catch (Exception e)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(65, 2);
								defaultInterpolatedStringHandler4.AppendLiteral("Failed to write a ChangeProperty network event for the item \"");
								defaultInterpolatedStringHandler4.AppendFormatted(this.Name);
								defaultInterpolatedStringHandler4.AppendLiteral("\" (");
								defaultInterpolatedStringHandler4.AppendFormatted(e.Message);
								defaultInterpolatedStringHandler4.AppendLiteral(")");
								throw new Exception(defaultInterpolatedStringHandler4.ToStringAndClear());
							}
						}
						else if (itemEventData is Item.SetItemStatEventData)
						{
							setItemStatEventData = (Item.SetItemStatEventData)itemEventData;
						}
						else
						{
							if (itemEventData is Item.UpgradeEventData)
							{
								upgradeEventData = (Item.UpgradeEventData)itemEventData;
								goto IL_544;
							}
							if (itemEventData is Item.DroppedStackEventData)
							{
								droppedStackEventData = (Item.DroppedStackEventData)itemEventData;
								goto IL_60C;
							}
							if (itemEventData is Item.SetHighlightEventData)
							{
								Item.SetHighlightEventData highlightEventData = (Item.SetHighlightEventData)itemEventData;
								bool isTargetedForClient = highlightEventData.TargetClients.IsEmpty || highlightEventData.TargetClients.Contains(c);
								msg.WriteBoolean(isTargetedForClient);
								if (!isTargetedForClient)
								{
									return;
								}
								msg.WriteBoolean(highlightEventData.Highlighted);
								if (highlightEventData.Highlighted)
								{
									msg.WriteColorR8G8B8A8(highlightEventData.Color);
									return;
								}
								return;
							}
							else
							{
								if (itemEventData is Item.SwapItemEventData)
								{
									Item.SwapItemEventData swapItemEventData = (Item.SwapItemEventData)itemEventData;
									msg.WriteUInt16(swapItemEventData.NewId);
									msg.WriteUInt32(swapItemEventData.NewItem.UintIdentifier);
									return;
								}
								throw this.<ServerEventWrite>g__error|4_0("Unsupported event type " + itemEventData.GetType().Name);
							}
						}
						msg.WriteByte((byte)setItemStatEventData.Stats.Count);
						using (Dictionary<TalentStatIdentifier, float>.Enumerator enumerator = setItemStatEventData.Stats.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								KeyValuePair<TalentStatIdentifier, float> keyValuePair = enumerator.Current;
								TalentStatIdentifier talentStatIdentifier;
								float num;
								keyValuePair.Deconstruct(out talentStatIdentifier, out num);
								TalentStatIdentifier key = talentStatIdentifier;
								float value = num;
								msg.WriteNetSerializableStruct(key);
								msg.WriteSingle(value);
							}
							return;
						}
						IL_544:
						Upgrade upgrade = upgradeEventData.Upgrade;
						Dictionary<ISerializableEntity, PropertyReference[]> upgradeTargets = upgrade.TargetComponents;
						msg.WriteIdentifier(upgrade.Identifier);
						msg.WriteByte((byte)upgrade.Level);
						msg.WriteByte((byte)upgradeTargets.Count);
						using (Dictionary<ISerializableEntity, PropertyReference[]>.Enumerator enumerator2 = upgrade.TargetComponents.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								KeyValuePair<ISerializableEntity, PropertyReference[]> keyValuePair2 = enumerator2.Current;
								ISerializableEntity serializableEntity;
								PropertyReference[] array;
								keyValuePair2.Deconstruct(out serializableEntity, out array);
								PropertyReference[] value2 = array;
								msg.WriteByte((byte)value2.Length);
								foreach (PropertyReference propertyReference in value2)
								{
									object originalValue = propertyReference.OriginalValue;
									msg.WriteSingle((float)(originalValue ?? -1));
								}
							}
							return;
						}
						IL_60C:
						msg.WriteRangedInteger(droppedStackEventData.Items.Length, 0, 63);
						foreach (Item droppedItem in droppedStackEventData.Items)
						{
							msg.WriteUInt16(droppedItem.ID);
						}
						return;
					}
					Item.ApplyStatusEffectEventData applyStatusEffectEventData = (Item.ApplyStatusEffectEventData)itemEventData;
					ActionType actionType = applyStatusEffectEventData.ActionType;
					ItemComponent targetComponent = applyStatusEffectEventData.TargetItemComponent;
					Limb targetLimb = applyStatusEffectEventData.TargetLimb;
					Vector2? worldPosition = applyStatusEffectEventData.WorldPosition;
					Character targetCharacter = applyStatusEffectEventData.TargetCharacter;
					if (targetCharacter != null && targetCharacter.Removed)
					{
						targetCharacter = null;
					}
					byte targetLimbIndex = (targetLimb != null && targetCharacter != null) ? ((byte)Array.IndexOf<Limb>(targetCharacter.AnimController.Limbs, targetLimb)) : byte.MaxValue;
					msg.WriteRangedInteger((int)actionType, 0, Enum.GetValues(typeof(ActionType)).Length - 1);
					msg.WriteByte((byte)((targetComponent == null) ? 255 : this.components.IndexOf(targetComponent)));
					Character targetCharacter2 = applyStatusEffectEventData.TargetCharacter;
					msg.WriteUInt16((targetCharacter2 != null) ? targetCharacter2.ID : 0);
					msg.WriteByte(targetLimbIndex);
					Entity useTarget = applyStatusEffectEventData.UseTarget;
					msg.WriteUInt16((useTarget != null) ? useTarget.ID : 0);
					msg.WriteBoolean(worldPosition != null);
					if (worldPosition != null)
					{
						msg.WriteSingle(worldPosition.Value.X);
						msg.WriteSingle(worldPosition.Value.Y);
						return;
					}
				}
				return;
			}
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x00046318 File Offset: 0x00044518
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			Item.EventType eventType = (Item.EventType)msg.ReadRangedInteger(0, 12);
			c.KickAFKTimer = 0f;
			switch (eventType)
			{
			case Item.EventType.ComponentState:
			{
				int componentIndex = msg.ReadRangedInteger(0, this.components.Count - 1);
				(this.components[componentIndex] as IClientSerializable).ServerEventRead(msg, c);
				return;
			}
			case Item.EventType.InventoryState:
			{
				int containerIndex = msg.ReadRangedInteger(0, this.components.Count - 1);
				(this.components[containerIndex] as ItemContainer).Inventory.ServerEventRead(msg, c);
				return;
			}
			case Item.EventType.Treatment:
			{
				if (c.Character == null || !c.Character.CanInteractWith(this, true))
				{
					return;
				}
				ushort characterID = msg.ReadUInt16();
				byte limbIndex = msg.ReadByte();
				if (HealingCooldown.IsOnCooldown(c))
				{
					return;
				}
				Character targetCharacter = Entity.FindEntityByID(characterID) as Character;
				if (targetCharacter != null && (targetCharacter == c.Character || c.Character.SelectedCharacter == targetCharacter))
				{
					HealingCooldown.SetCooldown(c);
					Limb targetLimb = ((int)limbIndex < targetCharacter.AnimController.Limbs.Length) ? targetCharacter.AnimController.Limbs[(int)limbIndex] : null;
					if (this.ContainedItems != null)
					{
						if (!this.ContainedItems.All((Item i) => i == null))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 3);
							defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(c.Character));
							defaultInterpolatedStringHandler.AppendLiteral(" used item ");
							defaultInterpolatedStringHandler.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" (contained items: ");
							defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", from i in this.ContainedItems
							select i.Name));
							defaultInterpolatedStringHandler.AppendLiteral(")");
							GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
							goto IL_20C;
						}
					}
					GameServer.Log(GameServer.CharacterLogName(c.Character) + " used item " + this.Name, ServerLog.MessageType.ItemInteraction);
					IL_20C:
					this.ApplyTreatment(c.Character, targetCharacter, targetLimb);
					return;
				}
				break;
			}
			case Item.EventType.ChangeProperty:
				this.ReadPropertyChange(msg, GameMain.NetworkMember.IsServer, c);
				return;
			case Item.EventType.Combine:
			{
				ushort combineTargetID = msg.ReadUInt16();
				Item combineTarget = Entity.FindEntityByID(combineTargetID) as Item;
				if (combineTarget == null || !c.Character.CanInteractWith(this, true) || !c.Character.CanInteractWith(combineTarget, true))
				{
					return;
				}
				this.Combine(combineTarget, c.Character);
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x000465A0 File Offset: 0x000447A0
		public void WriteSpawnData(IWriteMessage msg, ushort entityID, ushort originalInventoryID, byte originalItemContainerIndex, int originalSlotIndex)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			msg.WriteString(this.Prefab.OriginalName);
			msg.WriteIdentifier(this.Prefab.Identifier);
			msg.WriteBoolean(this.Description != this.Prefab.Description);
			if (this.Description != this.Prefab.Description)
			{
				msg.WriteString(this.Description);
			}
			msg.WriteUInt16(entityID);
			if (this.ParentInventory == null || this.ParentInventory.Owner == null || originalInventoryID == 0)
			{
				msg.WriteUInt16(0);
				msg.WriteSingle(this.Position.X);
				msg.WriteSingle(this.Position.Y);
				msg.WriteRangedSingle((this.body == null) ? 0f : MathUtils.WrapAngleTwoPi(this.body.Rotation), 0f, 6.2831855f, 8);
				msg.WriteUInt16((base.Submarine != null) ? base.Submarine.ID : 0);
			}
			else
			{
				msg.WriteUInt16(originalInventoryID);
				msg.WriteByte(originalItemContainerIndex);
				msg.WriteByte((originalSlotIndex < 0) ? byte.MaxValue : ((byte)originalSlotIndex));
			}
			msg.WriteBoolean(this.OnInsertedEffectsAppliedOnPreviousRound);
			msg.WriteByte((this.body == null) ? 0 : ((byte)this.body.BodyType));
			msg.WriteBoolean(this.SpawnedInCurrentOutpost);
			msg.WriteBoolean(this.AllowStealing);
			msg.WriteRangedInteger(this.Quality, 0, 3);
			byte teamID = 0;
			IdCard idCardComponent = null;
			using (IEnumerator<WifiComponent> enumerator = this.GetComponents<WifiComponent>().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					WifiComponent wifiComponent = enumerator.Current;
					teamID = (byte)wifiComponent.TeamID;
				}
			}
			if (teamID == 0)
			{
				using (IEnumerator<IdCard> enumerator2 = this.GetComponents<IdCard>().GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						IdCard idCard = enumerator2.Current;
						teamID = (byte)idCard.TeamID;
						idCardComponent = idCard;
					}
				}
			}
			msg.WriteByte(teamID);
			bool hasIdCard = idCardComponent != null;
			msg.WriteBoolean(hasIdCard);
			if (hasIdCard)
			{
				msg.WriteInt32(idCardComponent.SubmarineSpecificID);
				msg.WriteString(idCardComponent.OwnerName);
				msg.WriteString(idCardComponent.OwnerTags);
				msg.WriteByte((byte)Math.Max(0, idCardComponent.OwnerBeardIndex + 1));
				msg.WriteByte((byte)Math.Max(0, idCardComponent.OwnerHairIndex + 1));
				msg.WriteByte((byte)Math.Max(0, idCardComponent.OwnerMoustacheIndex + 1));
				msg.WriteByte((byte)Math.Max(0, idCardComponent.OwnerFaceAttachmentIndex + 1));
				msg.WriteColorR8G8B8(idCardComponent.OwnerHairColor);
				msg.WriteColorR8G8B8(idCardComponent.OwnerFacialHairColor);
				msg.WriteColorR8G8B8(idCardComponent.OwnerSkinColor);
				msg.WriteIdentifier(idCardComponent.OwnerJobId);
				msg.WriteByte((byte)idCardComponent.OwnerSheetIndex.X);
				msg.WriteByte((byte)idCardComponent.OwnerSheetIndex.Y);
			}
			bool tagsChanged = this.tags.Count != this.Prefab.Tags.Count || !this.tags.All((Identifier t) => this.Prefab.Tags.Contains(t));
			msg.WriteBoolean(tagsChanged);
			if (tagsChanged)
			{
				IEnumerable<Identifier> splitTags = this.Tags.ToIdentifiers(",");
				msg.WriteString(string.Join<Identifier>(',', from t in splitTags
				where !this.Prefab.Tags.Contains(t)
				select t));
				msg.WriteString(string.Join<Identifier>(',', from t in this.Prefab.Tags
				where !splitTags.Contains(t)
				select t));
			}
			NameTag nameTag = this.GetComponent<NameTag>();
			msg.WriteBoolean(nameTag != null);
			if (nameTag != null)
			{
				msg.WriteString(nameTag.WrittenName ?? "");
			}
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x00046988 File Offset: 0x00044B88
		public float GetPositionUpdateInterval(Client recipient)
		{
			if (this.PositionUpdateInterval == float.PositiveInfinity || this.body == null || this.parentInventory != null)
			{
				return float.PositiveInfinity;
			}
			if (recipient.Character == null || recipient.Character.IsDead)
			{
				return Math.Max(this.PositionUpdateInterval, 0.5f);
			}
			float distSqr = Vector2.DistanceSquared(recipient.Character.WorldPosition, this.WorldPosition);
			if (distSqr > 400000000f)
			{
				return float.PositiveInfinity;
			}
			if (distSqr > 100000000f)
			{
				return this.PositionUpdateInterval * 10f;
			}
			if (distSqr > 1000000f)
			{
				return this.PositionUpdateInterval * 2f;
			}
			return this.PositionUpdateInterval;
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x00046A33 File Offset: 0x00044C33
		public void ServerWritePosition(ReadWriteMessage tempBuffer, Client c)
		{
			this.body.ServerWrite(tempBuffer);
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x00046A41 File Offset: 0x00044C41
		public void CreateServerEvent<T>(T ic) where T : ItemComponent, IServerSerializable
		{
			this.CreateServerEvent<T>(ic, ic.ServerGetEventData());
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x00046A58 File Offset: 0x00044C58
		public void CreateServerEvent<T>(T ic, ItemComponent.IEventData extraData) where T : ItemComponent, IServerSerializable
		{
			if (GameMain.Server == null)
			{
				return;
			}
			if (!Item.ItemList.Contains(this))
			{
				string errorMsg = "Attempted to create a network event for an item (" + this.Name + ") that hasn't been fully initialized yet.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.CreateServerEvent:EventForUninitializedItem" + this.Name + this.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (!this.components.Contains(ic))
			{
				return;
			}
			Item.ComponentStateEventData eventData = new Item.ComponentStateEventData(ic, extraData);
			if (!ic.ValidateEventData(eventData))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Server-side component event creation for the item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" failed: ");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted("ValidateEventData");
				defaultInterpolatedStringHandler.AppendLiteral(" returned false. ");
				defaultInterpolatedStringHandler.AppendLiteral("Data: ");
				defaultInterpolatedStringHandler.AppendFormatted(((extraData != null) ? extraData.GetType().ToString() : null) ?? "null");
				string errorMsg2 = defaultInterpolatedStringHandler.ToStringAndClear();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Item.CreateServerEvent:ValidateEventData:");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				GameAnalyticsManager.AddErrorEventOnce(defaultInterpolatedStringHandler2.ToStringAndClear(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
				throw new Exception(errorMsg2);
			}
			GameMain.Server.CreateEntityEvent(this, eventData);
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x0600073F RID: 1855 RVA: 0x00046BF0 File Offset: 0x00044DF0
		public static IReadOnlyCollection<Item> DangerousItems
		{
			get
			{
				return Item._dangerousItems;
			}
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000740 RID: 1856 RVA: 0x00046BF7 File Offset: 0x00044DF7
		public static IReadOnlyCollection<Item> RepairableItems
		{
			get
			{
				return Item._repairableItems;
			}
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000741 RID: 1857 RVA: 0x00046BFE File Offset: 0x00044DFE
		public static IReadOnlyCollection<Item> CleanableItems
		{
			get
			{
				return Item._cleanableItems;
			}
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000742 RID: 1858 RVA: 0x00046C05 File Offset: 0x00044E05
		public static HashSet<Item> DeconstructItems
		{
			get
			{
				return Item._deconstructItems;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06000743 RID: 1859 RVA: 0x00046C0C File Offset: 0x00044E0C
		public static IReadOnlyCollection<Item> SonarVisibleItems
		{
			get
			{
				return Item._sonarVisibleItems;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000744 RID: 1860 RVA: 0x00046C13 File Offset: 0x00044E13
		public static IReadOnlyCollection<Item> TurretTargetItems
		{
			get
			{
				return Item._turretTargetItems;
			}
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000745 RID: 1861 RVA: 0x00046C1A File Offset: 0x00044E1A
		public static IReadOnlyCollection<Item> ChairItems
		{
			get
			{
				return Item._chairItems;
			}
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06000746 RID: 1862 RVA: 0x00046C21 File Offset: 0x00044E21
		public new ItemPrefab Prefab
		{
			get
			{
				return this.Prefab as ItemPrefab;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000747 RID: 1863 RVA: 0x00046C2E File Offset: 0x00044E2E
		public override ContentPackage ContentPackage
		{
			get
			{
				ItemPrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.ContentPackage;
			}
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000748 RID: 1864 RVA: 0x00046C41 File Offset: 0x00044E41
		// (set) Token: 0x06000749 RID: 1865 RVA: 0x00046C49 File Offset: 0x00044E49
		public Hull CurrentHull
		{
			get
			{
				return this.currentHull;
			}
			set
			{
				this.currentHull = value;
			}
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x0600074A RID: 1866 RVA: 0x00046C52 File Offset: 0x00044E52
		public float HullOxygenPercentage
		{
			get
			{
				Hull hull = this.CurrentHull;
				if (hull == null)
				{
					return 0f;
				}
				return hull.OxygenPercentage;
			}
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x0600074B RID: 1867 RVA: 0x00046C69 File Offset: 0x00044E69
		public CampaignMode.InteractionType CampaignInteractionType
		{
			get
			{
				return this.campaignInteractionType;
			}
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00046C71 File Offset: 0x00044E71
		public void AssignCampaignInteractionType(CampaignMode.InteractionType interactionType, IEnumerable<Client> targetClients = null)
		{
			if (this.campaignInteractionType == interactionType)
			{
				return;
			}
			this.campaignInteractionType = interactionType;
			this.AssignCampaignInteractionTypeProjSpecific(this.campaignInteractionType, targetClients);
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x00046C94 File Offset: 0x00044E94
		private void AssignCampaignInteractionTypeProjSpecific(CampaignMode.InteractionType interactionType, IEnumerable<Client> targetClients)
		{
			if (base.Removed)
			{
				return;
			}
			if (targetClients == null || targetClients.None(null))
			{
				this.campaignInteractionTypePerClient.Clear();
			}
			else
			{
				foreach (Client client in targetClients)
				{
					this.campaignInteractionTypePerClient[client] = interactionType;
				}
			}
			GameMain.NetworkMember.CreateEntityEvent(this, new Item.AssignCampaignInteractionEventData(targetClients));
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x0600074E RID: 1870 RVA: 0x00046D1C File Offset: 0x00044F1C
		// (set) Token: 0x0600074F RID: 1871 RVA: 0x00046D24 File Offset: 0x00044F24
		public bool FullyInitialized { get; private set; }

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000750 RID: 1872 RVA: 0x00046D30 File Offset: 0x00044F30
		// (set) Token: 0x06000751 RID: 1873 RVA: 0x00046D5B File Offset: 0x00044F5B
		public float WaterDragCoefficient
		{
			get
			{
				float? num = this.overrideWaterDragCoefficient;
				if (num == null)
				{
					return this.originalWaterDragCoefficient;
				}
				return num.GetValueOrDefault();
			}
			set
			{
				this.overrideWaterDragCoefficient = new float?(value);
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000752 RID: 1874 RVA: 0x00046D69 File Offset: 0x00044F69
		// (set) Token: 0x06000753 RID: 1875 RVA: 0x00046D7C File Offset: 0x00044F7C
		public BodyType BodyType
		{
			get
			{
				PhysicsBody physicsBody = this.body;
				if (physicsBody == null)
				{
					return BodyType.Dynamic;
				}
				return physicsBody.BodyType;
			}
			set
			{
				if (this.body != null)
				{
					this.body.BodyType = value;
				}
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00046D92 File Offset: 0x00044F92
		public void ResetWaterDragCoefficient()
		{
			this.overrideWaterDragCoefficient = null;
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000755 RID: 1877 RVA: 0x00046DA0 File Offset: 0x00044FA0
		// (set) Token: 0x06000756 RID: 1878 RVA: 0x00046DA8 File Offset: 0x00044FA8
		public Rectangle DefaultRect
		{
			get
			{
				return this.defaultRect;
			}
			set
			{
				this.defaultRect = value;
			}
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000757 RID: 1879 RVA: 0x00046DB1 File Offset: 0x00044FB1
		// (set) Token: 0x06000758 RID: 1880 RVA: 0x00046DB9 File Offset: 0x00044FB9
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000759 RID: 1881 RVA: 0x00046DC4 File Offset: 0x00044FC4
		private bool HasInGameEditableProperties
		{
			get
			{
				if (this.hasInGameEditableProperties == null)
				{
					this.hasInGameEditableProperties = new bool?(false);
					if (this.SerializableProperties.Values.Any((SerializableProperty p) => p.Attributes.OfType<InGameEditable>().Any<InGameEditable>()))
					{
						this.hasInGameEditableProperties = new bool?(true);
					}
					else
					{
						foreach (ItemComponent component in this.components)
						{
							if (component.AllowInGameEditing)
							{
								if (component.SerializableProperties.Values.Any((SerializableProperty p) => p.Attributes.OfType<InGameEditable>().Any<InGameEditable>()) || component.SerializableProperties.Values.Any((SerializableProperty p) => p.Attributes.OfType<ConditionallyEditable>().Any((ConditionallyEditable a) => a.IsEditable(this))))
								{
									this.hasInGameEditableProperties = new bool?(true);
									break;
								}
							}
						}
					}
				}
				return this.hasInGameEditableProperties.Value;
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x0600075A RID: 1882 RVA: 0x00046EE0 File Offset: 0x000450E0
		// (set) Token: 0x0600075B RID: 1883 RVA: 0x00046EE8 File Offset: 0x000450E8
		public bool EditableWhenEquipped { get; set; }

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x0600075C RID: 1884 RVA: 0x00046EF1 File Offset: 0x000450F1
		// (set) Token: 0x0600075D RID: 1885 RVA: 0x00046EF9 File Offset: 0x000450F9
		public Inventory PreviousParentInventory { get; set; }

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x0600075E RID: 1886 RVA: 0x00046F02 File Offset: 0x00045102
		// (set) Token: 0x0600075F RID: 1887 RVA: 0x00046F0A File Offset: 0x0004510A
		public Inventory ParentInventory
		{
			get
			{
				return this.parentInventory;
			}
			set
			{
				this.parentInventory = value;
				if (this.parentInventory != null)
				{
					this.Container = (this.parentInventory.Owner as Item);
					this.RemoveFromDroppedStack(false);
				}
				this.PreviousParentInventory = value;
			}
		}

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000760 RID: 1888 RVA: 0x00046F3F File Offset: 0x0004513F
		// (set) Token: 0x06000761 RID: 1889 RVA: 0x00046F48 File Offset: 0x00045148
		public Item RootContainer
		{
			get
			{
				return this.rootContainer;
			}
			private set
			{
				if (value == this)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Attempted to set the item \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" as it's own root container!\n");
					defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					this.rootContainer = null;
					return;
				}
				this.rootContainer = value;
			}
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000762 RID: 1890 RVA: 0x00046FBD File Offset: 0x000451BD
		// (set) Token: 0x06000763 RID: 1891 RVA: 0x00046FC5 File Offset: 0x000451C5
		public Item Container
		{
			get
			{
				return this.container;
			}
			private set
			{
				if (value != this.container)
				{
					this.container = value;
					this.CheckCleanable();
					this.SetActiveSprite();
					this.RefreshRootContainer();
				}
			}
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000764 RID: 1892 RVA: 0x00046FE9 File Offset: 0x000451E9
		public override string Name
		{
			get
			{
				return this.Prefab.Name.Value;
			}
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000765 RID: 1893 RVA: 0x00046FFB File Offset: 0x000451FB
		// (set) Token: 0x06000766 RID: 1894 RVA: 0x00047017 File Offset: 0x00045217
		public string Description
		{
			get
			{
				return this.description ?? this.Prefab.Description.Value;
			}
			set
			{
				this.description = value;
			}
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000767 RID: 1895 RVA: 0x00047020 File Offset: 0x00045220
		// (set) Token: 0x06000768 RID: 1896 RVA: 0x00047028 File Offset: 0x00045228
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork, true)]
		public string DescriptionTag
		{
			get
			{
				return this.descriptionTag;
			}
			set
			{
				if (value == this.descriptionTag)
				{
					return;
				}
				if (value.IsNullOrEmpty())
				{
					this.descriptionTag = null;
					this.description = null;
				}
				else
				{
					this.description = TextManager.Get(value).Value;
					this.descriptionTag = value;
				}
				SerializableProperty property;
				if (this.FullyInitialized && this.SerializableProperties != null && this.SerializableProperties.TryGetValue("DescriptionTag".ToIdentifier(), out property))
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember == null)
					{
						return;
					}
					networkMember.CreateEntityEvent(this, new Item.ChangePropertyEventData(property, this));
				}
			}
		}

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000769 RID: 1897 RVA: 0x000470B9 File Offset: 0x000452B9
		// (set) Token: 0x0600076A RID: 1898 RVA: 0x000470C1 File Offset: 0x000452C1
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool NonInteractable { get; set; }

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x0600076B RID: 1899 RVA: 0x000470CA File Offset: 0x000452CA
		// (set) Token: 0x0600076C RID: 1900 RVA: 0x000470D2 File Offset: 0x000452D2
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled, item is interactable only for characters on non-player teams.", "", true)]
		public bool NonPlayerTeamInteractable { get; set; }

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x0600076D RID: 1901 RVA: 0x000470DB File Offset: 0x000452DB
		// (set) Token: 0x0600076E RID: 1902 RVA: 0x000470E3 File Offset: 0x000452E3
		[ConditionallyEditable(ConditionallyEditable.ConditionType.IsSwappableItem, true)]
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool AllowSwapping { get; set; }

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x0600076F RID: 1903 RVA: 0x000470EC File Offset: 0x000452EC
		// (set) Token: 0x06000770 RID: 1904 RVA: 0x000470F4 File Offset: 0x000452F4
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PurchasedNewSwap { get; set; }

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000771 RID: 1905 RVA: 0x000470FD File Offset: 0x000452FD
		public bool IsPlayerTeamInteractable
		{
			get
			{
				return !this.NonInteractable && !this.NonPlayerTeamInteractable;
			}
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00047112 File Offset: 0x00045312
		public bool IsInteractable(Character character)
		{
			if (base.IsHidden)
			{
				return false;
			}
			if (character != null && character.IsOnPlayerTeam)
			{
				return this.IsPlayerTeamInteractable;
			}
			return !this.NonInteractable;
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000773 RID: 1907 RVA: 0x00047139 File Offset: 0x00045339
		// (set) Token: 0x06000774 RID: 1908 RVA: 0x00047146 File Offset: 0x00045346
		[ConditionallyEditable(ConditionallyEditable.ConditionType.AllowRotating, true, DecimalCount = 3, ForceShowPlusMinusButtons = true, ValueStep = 0.1f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Rotation
		{
			get
			{
				return MathHelper.ToDegrees(this.RotationRad);
			}
			set
			{
				if (!this.Prefab.AllowRotatingInEditor)
				{
					return;
				}
				this.RotationRad = MathUtils.WrapAnglePi(MathHelper.ToRadians(value));
			}
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000775 RID: 1909 RVA: 0x00047167 File Offset: 0x00045367
		// (set) Token: 0x06000776 RID: 1910 RVA: 0x0004716F File Offset: 0x0004536F
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts, true, MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float ImpactTolerance
		{
			get
			{
				return this.impactTolerance;
			}
			set
			{
				this.impactTolerance = Math.Max(value, 0f);
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000777 RID: 1911 RVA: 0x00047182 File Offset: 0x00045382
		// (set) Token: 0x06000778 RID: 1912 RVA: 0x0004718A File Offset: 0x0004538A
		[Serialize(0f, IsPropertySaveable.Yes, "The amount of damage the item takes from impacts. Acts as a multiplier on the strength of the impact. Note that ImpactTolerance must be set for impacts to register.", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts, true, MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float ImpactDamage { get; set; }

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000779 RID: 1913 RVA: 0x00047193 File Offset: 0x00045393
		// (set) Token: 0x0600077A RID: 1914 RVA: 0x0004719B File Offset: 0x0004539B
		[Serialize(1f, IsPropertySaveable.Yes, "Probability for impacts to register. Defaults to 1. Note that ImpactTolerance must also be set for impacts to register.", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts, true, MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float ImpactDamageProbability { get; set; }

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x0600077B RID: 1915 RVA: 0x000471A4 File Offset: 0x000453A4
		public float InteractDistance
		{
			get
			{
				return this.Prefab.InteractDistance;
			}
		}

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x0600077C RID: 1916 RVA: 0x000471B1 File Offset: 0x000453B1
		public float InteractPriority
		{
			get
			{
				return this.Prefab.InteractPriority;
			}
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x0600077D RID: 1917 RVA: 0x000471BE File Offset: 0x000453BE
		public override Vector2 Position
		{
			get
			{
				if (this.body != null)
				{
					return this.body.Position;
				}
				return base.Position;
			}
		}

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x0600077E RID: 1918 RVA: 0x000471DA File Offset: 0x000453DA
		public override Vector2 SimPosition
		{
			get
			{
				if (this.body != null)
				{
					return this.body.SimPosition;
				}
				return ConvertUnits.ToSimUnits(base.Position);
			}
		}

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x0600077F RID: 1919 RVA: 0x000471FB File Offset: 0x000453FB
		public Rectangle InteractionRect
		{
			get
			{
				return base.WorldRect;
			}
		}

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000780 RID: 1920 RVA: 0x00047203 File Offset: 0x00045403
		// (set) Token: 0x06000781 RID: 1921 RVA: 0x0004720C File Offset: 0x0004540C
		public override float Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				if (this.scale == value)
				{
					return;
				}
				this.scale = MathHelper.Clamp(value, this.Prefab.MinScale, this.Prefab.MaxScale);
				float relativeScale = this.scale / this.Prefab.Scale;
				if (!base.ResizeHorizontal || !base.ResizeVertical)
				{
					int newWidth = base.ResizeHorizontal ? this.rect.Width : ((int)((float)this.defaultRect.Width * relativeScale));
					int newHeight = base.ResizeVertical ? this.rect.Height : ((int)((float)this.defaultRect.Height * relativeScale));
					this.Rect = new Rectangle(this.rect.X, this.rect.Y, newWidth, newHeight);
				}
				if (this.body != null)
				{
					if (this.FullyInitialized)
					{
						Screen selected = Screen.Selected;
						if (selected != null && selected.IsEditor)
						{
							this.UpdateTransform();
						}
					}
					else
					{
						this.body.SetTransformIgnoreContacts(ConvertUnits.ToSimUnits(base.Position), this.body.Rotation, true);
					}
				}
				if (this.components != null)
				{
					foreach (ItemComponent component in this.components)
					{
						component.OnScaleChanged();
					}
				}
			}
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000782 RID: 1922 RVA: 0x00047374 File Offset: 0x00045574
		// (set) Token: 0x06000783 RID: 1923 RVA: 0x0004737C File Offset: 0x0004557C
		public float PositionUpdateInterval { get; set; } = float.PositiveInfinity;

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000784 RID: 1924 RVA: 0x00047385 File Offset: 0x00045585
		// (set) Token: 0x06000785 RID: 1925 RVA: 0x0004738D File Offset: 0x0004558D
		public Sprite OverrideInventorySprite { get; set; }

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000786 RID: 1926 RVA: 0x00047396 File Offset: 0x00045596
		// (set) Token: 0x06000787 RID: 1927 RVA: 0x0004739E File Offset: 0x0004559E
		[Editable]
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		public Color SpriteColor
		{
			get
			{
				return this.spriteColor;
			}
			set
			{
				this.spriteColor = value;
			}
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000788 RID: 1928 RVA: 0x000473A7 File Offset: 0x000455A7
		// (set) Token: 0x06000789 RID: 1929 RVA: 0x000473AF File Offset: 0x000455AF
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.Pickable, true)]
		public Color InventoryIconColor { get; protected set; }

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x0600078A RID: 1930 RVA: 0x000473B8 File Offset: 0x000455B8
		// (set) Token: 0x0600078B RID: 1931 RVA: 0x000473C0 File Offset: 0x000455C0
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Changes the color of the item this item is contained inside. Only has an effect if either of the UseContainedSpriteColor or UseContainedInventoryIconColor property of the container is set to true.", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.Pickable, true)]
		public Color ContainerColor { get; protected set; }

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x0600078C RID: 1932 RVA: 0x000473CC File Offset: 0x000455CC
		public Identifier ContainerIdentifier
		{
			get
			{
				Item item = this.Container;
				if (item != null)
				{
					return item.Prefab.Identifier;
				}
				Inventory inventory = this.ParentInventory;
				Identifier? identifier;
				if (inventory == null)
				{
					identifier = null;
				}
				else
				{
					Entity owner = inventory.Owner;
					identifier = ((owner != null) ? new Identifier?(owner.ToIdentifier<Entity>()) : null);
				}
				Identifier? identifier2 = identifier;
				if (identifier2 == null)
				{
					return Identifier.Empty;
				}
				return identifier2.GetValueOrDefault();
			}
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x0600078D RID: 1933 RVA: 0x00047438 File Offset: 0x00045638
		public bool IsContained
		{
			get
			{
				return this.parentInventory != null;
			}
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x0600078E RID: 1934 RVA: 0x00047444 File Offset: 0x00045644
		public float Speed
		{
			get
			{
				if (this.body != null && this.body.PhysEnabled)
				{
					return this.body.LinearVelocity.Length();
				}
				Inventory inventory = this.ParentInventory;
				Character character = ((inventory != null) ? inventory.Owner : null) as Character;
				if (character != null)
				{
					return character.AnimController.MainLimb.LinearVelocity.Length();
				}
				if (this.container != null)
				{
					return this.container.Speed;
				}
				return 0f;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x0600078F RID: 1935 RVA: 0x000474C7 File Offset: 0x000456C7
		// (set) Token: 0x06000790 RID: 1936 RVA: 0x000474F0 File Offset: 0x000456F0
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SonarLabel
		{
			get
			{
				AITarget aiTarget = base.AiTarget;
				string text;
				if (aiTarget == null)
				{
					text = null;
				}
				else
				{
					LocalizedString sonarLabel = aiTarget.SonarLabel;
					text = ((sonarLabel != null) ? sonarLabel.Value : null);
				}
				return text ?? "";
			}
			set
			{
				if (base.AiTarget != null)
				{
					string trimmedStr = (!string.IsNullOrEmpty(value) && value.Length > 250) ? value.Substring(250) : value;
					base.AiTarget.SonarLabel = TextManager.Get(trimmedStr).Fallback(trimmedStr, true);
				}
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000791 RID: 1937 RVA: 0x00047546 File Offset: 0x00045746
		public bool PhysicsBodyActive
		{
			get
			{
				return this.body != null && this.body.Enabled;
			}
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000792 RID: 1938 RVA: 0x0004755D File Offset: 0x0004575D
		// (set) Token: 0x06000793 RID: 1939 RVA: 0x00047578 File Offset: 0x00045778
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public new float SoundRange
		{
			get
			{
				if (this.aiTarget != null)
				{
					return this.aiTarget.SoundRange;
				}
				return 0f;
			}
			set
			{
				if (this.aiTarget != null)
				{
					this.aiTarget.SoundRange = Math.Max(0f, value);
				}
			}
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06000794 RID: 1940 RVA: 0x00047598 File Offset: 0x00045798
		// (set) Token: 0x06000795 RID: 1941 RVA: 0x000475B3 File Offset: 0x000457B3
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public new float SightRange
		{
			get
			{
				if (this.aiTarget != null)
				{
					return this.aiTarget.SightRange;
				}
				return 0f;
			}
			set
			{
				if (this.aiTarget != null)
				{
					this.aiTarget.SightRange = Math.Max(0f, value);
				}
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06000796 RID: 1942 RVA: 0x000475D3 File Offset: 0x000457D3
		// (set) Token: 0x06000797 RID: 1943 RVA: 0x000475DB File Offset: 0x000457DB
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool IsShootable { get; set; }

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000798 RID: 1944 RVA: 0x000475E4 File Offset: 0x000457E4
		// (set) Token: 0x06000799 RID: 1945 RVA: 0x000475EC File Offset: 0x000457EC
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RequireAimToUse { get; set; }

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x0600079A RID: 1946 RVA: 0x000475F5 File Offset: 0x000457F5
		// (set) Token: 0x0600079B RID: 1947 RVA: 0x000475FD File Offset: 0x000457FD
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool RequireAimToSecondaryUse { get; set; }

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x0600079C RID: 1948 RVA: 0x00047606 File Offset: 0x00045806
		// (set) Token: 0x0600079D RID: 1949 RVA: 0x0004760E File Offset: 0x0004580E
		public bool DontCleanUp { get; set; }

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x0600079E RID: 1950 RVA: 0x00047617 File Offset: 0x00045817
		// (set) Token: 0x0600079F RID: 1951 RVA: 0x0004761F File Offset: 0x0004581F
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool OnInsertedEffectsApplied { get; set; }

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x060007A0 RID: 1952 RVA: 0x00047628 File Offset: 0x00045828
		public Color Color
		{
			get
			{
				return this.spriteColor;
			}
		}

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x060007A1 RID: 1953 RVA: 0x00047630 File Offset: 0x00045830
		// (set) Token: 0x060007A2 RID: 1954 RVA: 0x00047638 File Offset: 0x00045838
		public bool IsFullCondition { get; private set; }

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x060007A3 RID: 1955 RVA: 0x00047641 File Offset: 0x00045841
		// (set) Token: 0x060007A4 RID: 1956 RVA: 0x00047649 File Offset: 0x00045849
		public float MaxCondition { get; private set; }

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x060007A5 RID: 1957 RVA: 0x00047652 File Offset: 0x00045852
		// (set) Token: 0x060007A6 RID: 1958 RVA: 0x0004765A File Offset: 0x0004585A
		public float ConditionPercentage { get; private set; }

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x060007A7 RID: 1959 RVA: 0x00047664 File Offset: 0x00045864
		public float ConditionPercentageRelativeToDefaultMaxCondition
		{
			get
			{
				float defaultMaxCondition = this.MaxCondition / this.MaxRepairConditionMultiplier;
				return MathUtils.Percentage(this.Condition, defaultMaxCondition);
			}
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x060007A8 RID: 1960 RVA: 0x0004768B File Offset: 0x0004588B
		// (set) Token: 0x060007A9 RID: 1961 RVA: 0x00047693 File Offset: 0x00045893
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float OffsetOnSelectedMultiplier
		{
			get
			{
				return this.offsetOnSelectedMultiplier;
			}
			set
			{
				this.offsetOnSelectedMultiplier = value;
			}
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x060007AA RID: 1962 RVA: 0x0004769C File Offset: 0x0004589C
		// (set) Token: 0x060007AB RID: 1963 RVA: 0x000476A4 File Offset: 0x000458A4
		[Serialize(1f, IsPropertySaveable.Yes, "Multiply the maximum condition by this value", "", false)]
		public float HealthMultiplier
		{
			get
			{
				return this.healthMultiplier;
			}
			set
			{
				float prevConditionPercentage = this.ConditionPercentage;
				this.healthMultiplier = MathHelper.Clamp(value, 0f, float.PositiveInfinity);
				this.RecalculateConditionValues();
				this.condition = this.MaxCondition * prevConditionPercentage / 100f;
				this.RecalculateConditionValues();
			}
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x060007AC RID: 1964 RVA: 0x000476EE File Offset: 0x000458EE
		// (set) Token: 0x060007AD RID: 1965 RVA: 0x000476F6 File Offset: 0x000458F6
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxRepairConditionMultiplier
		{
			get
			{
				return this.maxRepairConditionMultiplier;
			}
			set
			{
				this.maxRepairConditionMultiplier = MathHelper.Clamp(value, 0f, float.PositiveInfinity);
				this.RecalculateConditionValues();
			}
		}

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x060007AE RID: 1966 RVA: 0x00047714 File Offset: 0x00045914
		// (set) Token: 0x060007AF RID: 1967 RVA: 0x0004771C File Offset: 0x0004591C
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool HasBeenInstantiatedOnce { get; set; }

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x060007B0 RID: 1968 RVA: 0x00047725 File Offset: 0x00045925
		// (set) Token: 0x060007B1 RID: 1969 RVA: 0x0004772D File Offset: 0x0004592D
		[Serialize(float.NaN, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public float Condition
		{
			get
			{
				return this.condition;
			}
			set
			{
				this.SetCondition(value, false, true);
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x060007B2 RID: 1970 RVA: 0x00047738 File Offset: 0x00045938
		// (set) Token: 0x060007B3 RID: 1971 RVA: 0x00047740 File Offset: 0x00045940
		private double ConditionLastUpdated { get; set; }

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x060007B4 RID: 1972 RVA: 0x00047749 File Offset: 0x00045949
		// (set) Token: 0x060007B5 RID: 1973 RVA: 0x00047751 File Offset: 0x00045951
		private float LastConditionChange { get; set; }

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x060007B6 RID: 1974 RVA: 0x0004775A File Offset: 0x0004595A
		public bool ConditionIncreasedRecently
		{
			get
			{
				return Timing.TotalTime < this.ConditionLastUpdated + 1.0 && this.LastConditionChange > 0f;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x060007B7 RID: 1975 RVA: 0x00047782 File Offset: 0x00045982
		public float Health
		{
			get
			{
				return this.condition;
			}
		}

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x060007B8 RID: 1976 RVA: 0x0004778C File Offset: 0x0004598C
		// (set) Token: 0x060007B9 RID: 1977 RVA: 0x000477BC File Offset: 0x000459BC
		public bool Indestructible
		{
			get
			{
				bool? flag = this.indestructible;
				if (flag == null)
				{
					return this.Prefab.Indestructible;
				}
				return flag.GetValueOrDefault();
			}
			set
			{
				this.indestructible = new bool?(value);
			}
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x060007BA RID: 1978 RVA: 0x000477CA File Offset: 0x000459CA
		// (set) Token: 0x060007BB RID: 1979 RVA: 0x000477D2 File Offset: 0x000459D2
		public bool AllowDeconstruct { get; set; }

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x060007BC RID: 1980 RVA: 0x000477DC File Offset: 0x000459DC
		// (set) Token: 0x060007BD RID: 1981 RVA: 0x0004780C File Offset: 0x00045A0C
		public bool IsDangerous
		{
			get
			{
				bool? flag = this.isDangerous;
				if (flag == null)
				{
					return this.Prefab.IsDangerous;
				}
				return flag.GetValueOrDefault();
			}
			set
			{
				this.isDangerous = new bool?(value);
				if (!value)
				{
					Item._dangerousItems.Remove(this);
					return;
				}
				Item._dangerousItems.Add(this);
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x060007BE RID: 1982 RVA: 0x00047836 File Offset: 0x00045A36
		// (set) Token: 0x060007BF RID: 1983 RVA: 0x0004783E File Offset: 0x00045A3E
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled will prevent the item from taking damage from all sources", "", false)]
		public bool InvulnerableToDamage { get; set; }

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x060007C0 RID: 1984 RVA: 0x00047847 File Offset: 0x00045A47
		public bool Illegitimate
		{
			get
			{
				return !this.AllowStealing && this.SpawnedInCurrentOutpost;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x060007C1 RID: 1985 RVA: 0x00047859 File Offset: 0x00045A59
		// (set) Token: 0x060007C2 RID: 1986 RVA: 0x00047861 File Offset: 0x00045A61
		public bool SpawnedInCurrentOutpost
		{
			get
			{
				return this.spawnedInCurrentOutpost;
			}
			set
			{
				if (!this.spawnedInCurrentOutpost && value)
				{
					GameSession gameSession = GameMain.GameSession;
					string text;
					if (gameSession == null)
					{
						text = null;
					}
					else
					{
						LevelData levelData = gameSession.LevelData;
						text = ((levelData != null) ? levelData.Seed : null);
					}
					this.OriginalOutpost = text;
				}
				this.spawnedInCurrentOutpost = value;
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x060007C3 RID: 1987 RVA: 0x0004789A File Offset: 0x00045A9A
		// (set) Token: 0x060007C4 RID: 1988 RVA: 0x000478B1 File Offset: 0x00045AB1
		[Serialize(true, IsPropertySaveable.Yes, "Determined by where/how the item originally spawned. If ItemPrefab.AllowStealing is true, stealing the item is always allowed.", "", true)]
		public bool AllowStealing
		{
			get
			{
				return this.allowStealing || this.Prefab.AllowStealingAlways;
			}
			set
			{
				this.allowStealing = value;
			}
		}

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x060007C5 RID: 1989 RVA: 0x000478BA File Offset: 0x00045ABA
		// (set) Token: 0x060007C6 RID: 1990 RVA: 0x000478C4 File Offset: 0x00045AC4
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string OriginalOutpost
		{
			get
			{
				return this.originalOutpost;
			}
			set
			{
				this.originalOutpost = value;
				if (!string.IsNullOrEmpty(value))
				{
					GameSession gameSession = GameMain.GameSession;
					bool flag;
					if (gameSession == null)
					{
						flag = false;
					}
					else
					{
						LevelData levelData = gameSession.LevelData;
						flag = (((levelData != null) ? new LevelData.LevelType?(levelData.Type) : null).GetValueOrDefault() == LevelData.LevelType.Outpost);
					}
					if (flag)
					{
						GameSession gameSession2 = GameMain.GameSession;
						string a;
						if (gameSession2 == null)
						{
							a = null;
						}
						else
						{
							LevelData levelData2 = gameSession2.LevelData;
							a = ((levelData2 != null) ? levelData2.Seed : null);
						}
						if (a == value)
						{
							this.spawnedInCurrentOutpost = true;
						}
					}
				}
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x060007C7 RID: 1991 RVA: 0x00047943 File Offset: 0x00045B43
		// (set) Token: 0x060007C8 RID: 1992 RVA: 0x00047958 File Offset: 0x00045B58
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Tags
		{
			get
			{
				return this.tags.ConvertToString(",");
			}
			set
			{
				this.tags.Clear();
				this.Prefab.Tags.ForEach(delegate(Identifier t)
				{
					this.tags.Add(t);
				});
				this.tags = this.tags.Union(value.ToIdentifiers(",")).ToHashSet<Identifier>();
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x060007C9 RID: 1993 RVA: 0x000479AD File Offset: 0x00045BAD
		// (set) Token: 0x060007CA RID: 1994 RVA: 0x000479B5 File Offset: 0x00045BB5
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool FireProof { get; private set; }

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x060007CB RID: 1995 RVA: 0x000479BE File Offset: 0x00045BBE
		// (set) Token: 0x060007CC RID: 1996 RVA: 0x000479C8 File Offset: 0x00045BC8
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool WaterProof
		{
			get
			{
				return this.waterProof;
			}
			private set
			{
				if (this.waterProof == value)
				{
					return;
				}
				this.waterProof = value;
				foreach (Item containedItem in this.ContainedItems)
				{
					containedItem.RefreshInWaterProofContainer();
				}
			}
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x060007CD RID: 1997 RVA: 0x00047A28 File Offset: 0x00045C28
		public bool UseInHealthInterface
		{
			get
			{
				return this.Prefab.UseInHealthInterface;
			}
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x060007CE RID: 1998 RVA: 0x00047A35 File Offset: 0x00045C35
		// (set) Token: 0x060007CF RID: 1999 RVA: 0x00047A48 File Offset: 0x00045C48
		public int Quality
		{
			get
			{
				Quality quality = this.qualityComponent;
				if (quality == null)
				{
					return 0;
				}
				return quality.QualityLevel;
			}
			set
			{
				if (this.qualityComponent != null)
				{
					this.qualityComponent.QualityLevel = value;
				}
			}
		}

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x060007D0 RID: 2000 RVA: 0x00047A5E File Offset: 0x00045C5E
		public bool InWater
		{
			get
			{
				if (this.body != null && this.body.Enabled)
				{
					return this.inWater;
				}
				if (this.hasInWaterStatusEffects)
				{
					return this.inWater;
				}
				return this.IsInWater();
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x060007D1 RID: 2001 RVA: 0x00047A91 File Offset: 0x00045C91
		// (set) Token: 0x060007D2 RID: 2002 RVA: 0x00047A99 File Offset: 0x00045C99
		public List<Connection> LastSentSignalRecipients { get; private set; } = new List<Connection>(20);

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x060007D3 RID: 2003 RVA: 0x00047AA2 File Offset: 0x00045CA2
		public ContentPath ConfigFilePath
		{
			get
			{
				return this.Prefab.ContentFile.Path;
			}
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x060007D4 RID: 2004 RVA: 0x00047AB4 File Offset: 0x00045CB4
		public IEnumerable<InvSlotType> AllowedSlots
		{
			get
			{
				return this.allowedSlots;
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x060007D5 RID: 2005 RVA: 0x00047ABC File Offset: 0x00045CBC
		public List<Connection> Connections
		{
			get
			{
				ConnectionPanel panel = this.GetComponent<ConnectionPanel>();
				if (panel == null)
				{
					return null;
				}
				return panel.Connections;
			}
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x060007D6 RID: 2006 RVA: 0x00047ADC File Offset: 0x00045CDC
		public IEnumerable<Item> ContainedItems
		{
			get
			{
				Item.<get_ContainedItems>d__356 <get_ContainedItems>d__ = new Item.<get_ContainedItems>d__356(-2);
				<get_ContainedItems>d__.<>4__this = this;
				return <get_ContainedItems>d__;
			}
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x060007D7 RID: 2007 RVA: 0x00047AF9 File Offset: 0x00045CF9
		public ItemInventory OwnInventory
		{
			get
			{
				return this.ownInventory;
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x060007D8 RID: 2008 RVA: 0x00047B01 File Offset: 0x00045D01
		// (set) Token: 0x060007D9 RID: 2009 RVA: 0x00047B09 File Offset: 0x00045D09
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Enable if you want to display the item HUD side by side with another item's HUD, when linked together. Disclaimer: It's possible or even likely that the views block each other, if they were not designed to be viewed together!", "", false)]
		public bool DisplaySideBySideWhenLinked { get; set; }

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x060007DA RID: 2010 RVA: 0x00047B12 File Offset: 0x00045D12
		public List<Repairable> Repairables
		{
			get
			{
				return this.repairables;
			}
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x060007DB RID: 2011 RVA: 0x00047B1A File Offset: 0x00045D1A
		public List<ItemComponent> Components
		{
			get
			{
				return this.components;
			}
		}

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x060007DC RID: 2012 RVA: 0x00047B22 File Offset: 0x00045D22
		public override bool Linkable
		{
			get
			{
				return this.Prefab.Linkable;
			}
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x060007DD RID: 2013 RVA: 0x00047B2F File Offset: 0x00045D2F
		public float WorldPositionX
		{
			get
			{
				return this.WorldPosition.X;
			}
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x060007DE RID: 2014 RVA: 0x00047B3C File Offset: 0x00045D3C
		public float WorldPositionY
		{
			get
			{
				return this.WorldPosition.Y;
			}
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x060007DF RID: 2015 RVA: 0x00047B49 File Offset: 0x00045D49
		// (set) Token: 0x060007E0 RID: 2016 RVA: 0x00047B56 File Offset: 0x00045D56
		public float PositionX
		{
			get
			{
				return this.Position.X;
			}
			private set
			{
				this.Move(new Vector2(value * this.Scale, 0f), true);
			}
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x060007E1 RID: 2017 RVA: 0x00047B71 File Offset: 0x00045D71
		// (set) Token: 0x060007E2 RID: 2018 RVA: 0x00047B7E File Offset: 0x00045D7E
		public float PositionY
		{
			get
			{
				return this.Position.Y;
			}
			private set
			{
				this.Move(new Vector2(0f, value * this.Scale), true);
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x060007E3 RID: 2019 RVA: 0x00047B99 File Offset: 0x00045D99
		// (set) Token: 0x060007E4 RID: 2020 RVA: 0x00047BA1 File Offset: 0x00045DA1
		public BallastFloraBranch Infector { get; set; }

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x060007E5 RID: 2021 RVA: 0x00047BAA File Offset: 0x00045DAA
		// (set) Token: 0x060007E6 RID: 2022 RVA: 0x00047BB2 File Offset: 0x00045DB2
		public ItemPrefab PendingItemSwap { get; set; }

		// Token: 0x060007E7 RID: 2023 RVA: 0x00047BBC File Offset: 0x00045DBC
		public override string ToString()
		{
			return (this.Name.IsNullOrEmpty() ? this.Prefab.Identifier : this.Name).ToString() + " (ID: " + this.ID.ToString() + ")";
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x060007E8 RID: 2024 RVA: 0x00047C16 File Offset: 0x00045E16
		public IReadOnlyList<ISerializableEntity> AllPropertyObjects
		{
			get
			{
				return this.allPropertyObjects;
			}
		}

		// Token: 0x060007E9 RID: 2025 RVA: 0x00047C1E File Offset: 0x00045E1E
		public bool IgnoreByAI(Character character)
		{
			return this.HasTag(Barotrauma.Tags.IgnoredByAI) || (this.OrderedToBeIgnored && character.IsOnPlayerTeam);
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x060007EA RID: 2026 RVA: 0x00047C3F File Offset: 0x00045E3F
		// (set) Token: 0x060007EB RID: 2027 RVA: 0x00047C47 File Offset: 0x00045E47
		public bool OrderedToBeIgnored { get; set; }

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x060007EC RID: 2028 RVA: 0x00047C50 File Offset: 0x00045E50
		public bool HasBallastFloraInHull
		{
			get
			{
				Hull hull = this.CurrentHull;
				return ((hull != null) ? hull.BallastFlora : null) != null;
			}
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x060007ED RID: 2029 RVA: 0x00047C67 File Offset: 0x00045E67
		public bool IsClaimedByBallastFlora
		{
			get
			{
				Hull hull = this.CurrentHull;
				return ((hull != null) ? hull.BallastFlora : null) != null && this.CurrentHull.BallastFlora.ClaimedTargets.Contains(this);
			}
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x060007EE RID: 2030 RVA: 0x00047C98 File Offset: 0x00045E98
		public bool InPlayerSubmarine
		{
			get
			{
				Submarine submarine = base.Submarine;
				SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
				return submarineInfo != null && submarineInfo.IsPlayer;
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x060007EF RID: 2031 RVA: 0x00047CC4 File Offset: 0x00045EC4
		public bool InBeaconStation
		{
			get
			{
				Submarine submarine = base.Submarine;
				SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
				return submarineInfo != null && submarineInfo.Type == SubmarineType.BeaconStation;
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x060007F0 RID: 2032 RVA: 0x00047CF2 File Offset: 0x00045EF2
		public bool IsLadder { get; }

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x060007F1 RID: 2033 RVA: 0x00047CFA File Offset: 0x00045EFA
		public bool IsSecondaryItem { get; }

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x060007F2 RID: 2034 RVA: 0x00047D02 File Offset: 0x00045F02
		public ItemStatManager StatManager
		{
			get
			{
				if (this.statManager == null)
				{
					this.statManager = new ItemStatManager(this);
				}
				return this.statManager;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x060007F3 RID: 2035 RVA: 0x00047D1E File Offset: 0x00045F1E
		// (set) Token: 0x060007F4 RID: 2036 RVA: 0x00047D26 File Offset: 0x00045F26
		public float LastEatenTime { get; set; }

		// Token: 0x060007F5 RID: 2037 RVA: 0x00047D30 File Offset: 0x00045F30
		public Item(ItemPrefab itemPrefab, Vector2 position, Submarine submarine, ushort id = 0, bool callOnItemLoaded = true) : this(new Rectangle((int)(position.X - itemPrefab.Sprite.size.X / 2f * itemPrefab.Scale), (int)(position.Y + itemPrefab.Sprite.size.Y / 2f * itemPrefab.Scale), (int)(itemPrefab.Sprite.size.X * itemPrefab.Scale), (int)(itemPrefab.Sprite.size.Y * itemPrefab.Scale)), itemPrefab, submarine, callOnItemLoaded, id)
		{
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x00047DC8 File Offset: 0x00045FC8
		public Item(Rectangle newRect, ItemPrefab itemPrefab, Submarine submarine, bool callOnItemLoaded = true, ushort id = 0) : base(itemPrefab, submarine, id)
		{
			this.spriteColor = this.Prefab.SpriteColor;
			this.components = new List<ItemComponent>();
			this.drawableComponents = new List<IDrawableComponent>();
			this.hasComponentsToDraw = false;
			this.tags = new HashSet<Identifier>();
			this.repairables = new List<Repairable>();
			this.defaultRect = newRect;
			this.rect = newRect;
			this.condition = (this.MaxCondition = (this.prevCondition = this.Prefab.Health));
			this.ConditionPercentage = 100f;
			this.lastSentCondition = this.condition;
			this.AllowDeconstruct = itemPrefab.AllowDeconstruct;
			this.allPropertyObjects.Add(this);
			ContentXElement element = itemPrefab.ConfigElement;
			ContentXElement contentXElement = null;
			if (element == contentXElement)
			{
				return;
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (submarine == null || !submarine.Loading)
			{
				this.FindHull();
			}
			this.SetActiveSprite();
			ContentXElement bodyElement = null;
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 4:
						if (text == "body")
						{
							bodyElement = subElement;
							float density = subElement.GetAttributeFloat("density", 10f);
							float minDensity = subElement.GetAttributeFloat("mindensity", density);
							float maxDensity = subElement.GetAttributeFloat("maxdensity", density);
							if (minDensity < maxDensity)
							{
								Random rand = new Random((int)this.ID);
								density = MathHelper.Lerp(minDensity, maxDensity, (float)rand.NextDouble());
							}
							string collisionCategoryStr = subElement.GetAttributeString("collisioncategory", null);
							Category collisionCategory = Category.Cat5;
							Category collidesWith = Category.Cat1 | Category.Cat3 | Category.Cat8 | Category.Cat9;
							if ((this.Prefab.DamagedByProjectiles || this.Prefab.DamagedByMeleeWeapons || this.Prefab.DamagedByRepairTools) && this.Condition > 0f)
							{
								collisionCategory = Category.Cat2;
								collidesWith |= Category.Cat7;
							}
							if (collisionCategoryStr != null)
							{
								Category cat;
								if (!Physics.TryParseCollisionCategory(collisionCategoryStr, out cat))
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Invalid collision category in item \"",
										this.Name,
										"\" (",
										collisionCategoryStr,
										")"
									}), null, element.ContentPackage, false, false);
								}
								else
								{
									collisionCategory = cat;
									if (cat.HasFlag(Category.Cat2))
									{
										collisionCategory |= Category.Cat7;
									}
								}
							}
							this.body = new PhysicsBody(subElement, ConvertUnits.ToSimUnits(this.Position), this.Scale, new float?(density), collisionCategory, collidesWith, false);
							this.body.FarseerBody.AngularDamping = subElement.GetAttributeFloat("angulardamping", 0.2f);
							this.body.FarseerBody.LinearDamping = subElement.GetAttributeFloat("lineardamping", 0.1f);
							this.body.UserData = this;
							continue;
						}
						break;
					case 5:
						if (text == "price")
						{
							continue;
						}
						break;
					case 6:
						if (text == "sprite")
						{
							continue;
						}
						break;
					case 7:
					{
						char c2 = text[0];
						if (c2 != 't')
						{
							if (c2 == 'u')
							{
								if (text == "upgrade")
								{
									continue;
								}
							}
						}
						else if (text == "trigger")
						{
							continue;
						}
						break;
					}
					case 8:
						if (text == "aitarget")
						{
							this.aiTarget = new AITarget(this, subElement);
							continue;
						}
						break;
					case 9:
						if (text == "fabricate")
						{
							continue;
						}
						break;
					case 10:
					{
						char c2 = text[0];
						if (c2 != 'f')
						{
							if (c2 == 's')
							{
								if (text == "staticbody")
								{
									this.StaticBodyConfig = subElement;
									continue;
								}
							}
						}
						else if (text == "fabricable")
						{
							continue;
						}
						break;
					}
					case 11:
					{
						char c2 = text[0];
						if (c2 != 'd')
						{
							if (c2 == 'm')
							{
								if (text == "minimapicon")
								{
									continue;
								}
							}
						}
						else if (text == "deconstruct")
						{
							continue;
						}
						break;
					}
					case 12:
						if (text == "brokensprite")
						{
							continue;
						}
						break;
					case 13:
					{
						char c2 = text[0];
						if (c2 != 'i')
						{
							if (c2 != 's')
							{
								if (c2 == 'u')
								{
									if (text == "upgrademodule")
									{
										continue;
									}
								}
							}
							else if (text == "swappableitem")
							{
								continue;
							}
						}
						else if (text == "inventoryicon")
						{
							continue;
						}
						break;
					}
					case 14:
					{
						char c2 = text[0];
						if (c2 != 'f')
						{
							if (c2 == 'i')
							{
								if (text == "infectedsprite")
								{
									continue;
								}
							}
						}
						else if (text == "fabricableitem")
						{
							continue;
						}
						break;
					}
					case 15:
					{
						char c2 = text[0];
						if (c2 != 'c')
						{
							if (c2 != 'l')
							{
								if (c2 == 'u')
								{
									if (text == "upgradeoverride")
									{
										continue;
									}
								}
							}
							else if (text == "levelcommonness")
							{
								continue;
							}
						}
						else if (text == "containedsprite")
						{
							continue;
						}
						break;
					}
					case 16:
						if (text == "decorativesprite")
						{
							continue;
						}
						break;
					case 17:
						if (text == "suitabletreatment")
						{
							continue;
						}
						break;
					case 18:
						if (text == "preferredcontainer")
						{
							continue;
						}
						break;
					case 20:
					{
						char c2 = text[0];
						if (c2 != 's')
						{
							if (c2 == 'u')
							{
								if (text == "upgradepreviewsprite")
								{
									continue;
								}
							}
						}
						else if (text == "skillrequirementhint")
						{
							continue;
						}
						break;
					}
					case 21:
						if (text == "damagedinfectedsprite")
						{
							continue;
						}
						break;
					}
				}
				ItemComponent ic4 = ItemComponent.Load(subElement, this, true);
				if (ic4 != null)
				{
					this.AddComponent(ic4);
				}
			}
			foreach (ItemComponent ic2 in this.components)
			{
				Pickable pickable = ic2 as Pickable;
				if (pickable != null)
				{
					foreach (InvSlotType allowedSlot in pickable.AllowedSlots)
					{
						this.allowedSlots.Add(allowedSlot);
					}
				}
				Repairable repairable = ic2 as Repairable;
				if (repairable != null)
				{
					this.repairables.Add(repairable);
				}
				if (ic2 is IDrawableComponent && ic2.Drawable)
				{
					this.drawableComponents.Add(ic2 as IDrawableComponent);
					this.hasComponentsToDraw = true;
				}
				if (ic2.statusEffectLists != null && !ic2.InheritStatusEffects)
				{
					if (this.statusEffectLists == null)
					{
						this.statusEffectLists = new Dictionary<ActionType, List<StatusEffect>>();
					}
					foreach (List<StatusEffect> componentEffectList in ic2.statusEffectLists.Values)
					{
						ActionType actionType = componentEffectList.First<StatusEffect>().type;
						List<StatusEffect> statusEffectList;
						if (!this.statusEffectLists.TryGetValue(actionType, out statusEffectList))
						{
							statusEffectList = new List<StatusEffect>();
							this.statusEffectLists.Add(actionType, statusEffectList);
							this.hasStatusEffectsOfType[(int)actionType] = true;
						}
						foreach (StatusEffect effect in componentEffectList)
						{
							statusEffectList.Add(effect);
						}
					}
				}
			}
			this.hasInWaterStatusEffects = this.hasStatusEffectsOfType[12];
			this.hasNotInWaterStatusEffects = this.hasStatusEffectsOfType[13];
			if (this.body != null)
			{
				this.body.Submarine = submarine;
				this.originalWaterDragCoefficient = bodyElement.GetAttributeFloat("waterdragcoefficient", 5f);
			}
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel != null)
			{
				this.connections = new Dictionary<string, Connection>();
				foreach (Connection c3 in connectionPanel.Connections)
				{
					if (!this.connections.ContainsKey(c3.Name))
					{
						this.connections.Add(c3.Name, c3);
					}
				}
			}
			if (this.body != null)
			{
				this.body.FarseerBody.OnCollision += this.OnCollision;
			}
			ItemContainer itemContainer = this.GetComponent<ItemContainer>();
			if (itemContainer != null)
			{
				this.ownInventory = itemContainer.Inventory;
			}
			this.OwnInventories = (from ic in this.GetComponents<ItemContainer>()
			select ic.Inventory).ToImmutableArray<ItemInventory>();
			this.qualityComponent = this.GetComponent<Quality>();
			this.IsLadder = (this.GetComponent<Ladder>() != null);
			int num;
			if (!this.IsLadder)
			{
				Controller component = this.GetComponent<Controller>();
				num = ((component != null && component.IsSecondaryItem) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			this.IsSecondaryItem = num;
			if (callOnItemLoaded)
			{
				foreach (ItemComponent ic3 in this.components)
				{
					ic3.OnItemLoaded();
				}
			}
			IEnumerable<ItemComponent> holdables = from c in this.components
			where c is Holdable
			select c;
			if (holdables.Count<ItemComponent>() > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Item ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" has multiple ");
				defaultInterpolatedStringHandler.AppendFormatted("Holdable");
				defaultInterpolatedStringHandler.AppendLiteral(" components (");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", from h in holdables
				select h.GetType().Name));
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.Prefab.ContentPackage);
			}
			base.InsertToList();
			Item.ItemList.Add(this);
			if (this.Prefab.IsDangerous)
			{
				Item._dangerousItems.Add(this);
			}
			if (this.Repairables.Any<Repairable>())
			{
				Item._repairableItems.Add(this);
			}
			if (this.Prefab.SonarSize > 0f)
			{
				Item._sonarVisibleItems.Add(this);
			}
			if (this.Prefab.IsAITurretTarget)
			{
				Item._turretTargetItems.Add(this);
			}
			if (this.Prefab.Tags.Contains(Barotrauma.Tags.ChairItem))
			{
				Item._chairItems.Add(this);
			}
			this.CheckCleanable();
			DebugConsole.Log(string.Concat(new string[]
			{
				"Created ",
				this.Name,
				" (",
				this.ID.ToString(),
				")"
			}));
			if (this.Components.Any((ItemComponent ic) => ic is Wire))
			{
				if (this.Components.All((ItemComponent ic) => ic is Wire || ic is Holdable))
				{
					this.isWire = true;
				}
			}
			if (this.HasTag(Barotrauma.Tags.LogicItem))
			{
				this.isLogic = true;
			}
			this.ApplyStatusEffects(ActionType.OnSpawn, 1f, null, null, null, false, null);
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				if (this.HasTag(Barotrauma.Tags.OxygenSource))
				{
					this.conditionMultiplierCampaign *= campaign.Settings.OxygenMultiplier;
				}
				if (this.HasTag(Barotrauma.Tags.ReactorFuel))
				{
					this.conditionMultiplierCampaign *= campaign.Settings.FuelMultiplier;
				}
			}
			this.condition *= this.conditionMultiplierCampaign;
			this.RecalculateConditionValues();
			if (callOnItemLoaded)
			{
				this.FullyInitialized = true;
			}
			this.HasBeenInstantiatedOnce = true;
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x00048C4C File Offset: 0x00046E4C
		public bool IsContainerPreferred(ItemContainer container, out bool isPreferencesDefined, out bool isSecondary, bool requireConditionRestriction = false)
		{
			return this.Prefab.IsContainerPreferred(this, container, out isPreferencesDefined, out isSecondary, requireConditionRestriction, false);
		}

		// Token: 0x060007F8 RID: 2040 RVA: 0x00048C60 File Offset: 0x00046E60
		public override MapEntity Clone()
		{
			Item clone = new Item(this.rect, this.Prefab, base.Submarine, false, 0)
			{
				defaultRect = this.defaultRect
			};
			foreach (KeyValuePair<Identifier, SerializableProperty> property in this.SerializableProperties)
			{
				if (!property.Value.Attributes.OfType<Serialize>().None(null))
				{
					clone.SerializableProperties[property.Key].TrySetValue(clone, property.Value.GetValue(this));
				}
			}
			if (this.components.Count != clone.components.Count)
			{
				string errorMsg = "Error while cloning item \"" + this.Name + "\" - clone does not have the same number of components. ";
				errorMsg = errorMsg + "Original components: " + string.Join(", ", from c in this.components
				select c.GetType().ToString());
				errorMsg = errorMsg + ", cloned components: " + string.Join(", ", from c in clone.components
				select c.GetType().ToString());
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.Clone:" + this.Name, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			}
			int i = 0;
			while (i < this.components.Count && i < clone.components.Count)
			{
				foreach (KeyValuePair<Identifier, SerializableProperty> property2 in from s in this.components[i].SerializableProperties
				orderby s.Key
				select s)
				{
					if (!property2.Value.Attributes.OfType<Serialize>().None(null))
					{
						clone.components[i].SerializableProperties[property2.Key].TrySetValue(clone.components[i], property2.Value.GetValue(this.components[i]));
					}
				}
				foreach (KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp in this.components[i].RequiredItems)
				{
					for (int j = 0; j < kvp.Value.Count; j++)
					{
						if (clone.components[i].RequiredItems.ContainsKey(kvp.Key) && clone.components[i].RequiredItems[kvp.Key].Count > j)
						{
							clone.components[i].RequiredItems[kvp.Key][j].JoinedIdentifiers = kvp.Value[j].JoinedIdentifiers;
						}
					}
				}
				i++;
			}
			if (base.FlippedX)
			{
				clone.FlipX(false, false);
			}
			if (base.FlippedY)
			{
				clone.FlipY(false, false);
			}
			clone.Rotation = this.Rotation;
			foreach (ItemComponent component in clone.components)
			{
				component.OnItemLoaded();
			}
			Dictionary<ushort, Item> clonedContainedItems = new Dictionary<ushort, Item>();
			int k = 0;
			while (k < this.components.Count && k < clone.components.Count)
			{
				ItemComponent component2 = this.components[k];
				ItemComponent cloneComp = clone.components[k];
				ItemContainer origInv = component2 as ItemContainer;
				if (origInv != null)
				{
					ItemContainer cloneInv = cloneComp as ItemContainer;
					if (cloneInv != null)
					{
						foreach (Item containedItem in origInv.Inventory.AllItems)
						{
							Item containedClone = (Item)containedItem.Clone();
							cloneInv.Inventory.TryPutItem(containedClone, null, null, true, false, true);
							clonedContainedItems.Add(containedItem.ID, containedClone);
						}
					}
				}
				k++;
			}
			int l = 0;
			while (l < this.components.Count && l < clone.components.Count)
			{
				ItemComponent component3 = this.components[l];
				ItemComponent cloneComp2 = clone.components[l];
				if (component3.GetType() == cloneComp2.GetType())
				{
					cloneComp2.Clone(component3);
				}
				CircuitBox origBox = component3 as CircuitBox;
				if (origBox != null)
				{
					CircuitBox cloneBox = cloneComp2 as CircuitBox;
					if (cloneBox != null)
					{
						cloneBox.CloneFrom(origBox, clonedContainedItems);
					}
				}
				l++;
			}
			clone.FullyInitialized = true;
			return clone;
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x000491C0 File Offset: 0x000473C0
		public void AddComponent(ItemComponent component)
		{
			Item.<>c__DisplayClass425_0 CS$<>8__locals1 = new Item.<>c__DisplayClass425_0();
			CS$<>8__locals1.component = component;
			CS$<>8__locals1.<>4__this = this;
			this.allPropertyObjects.Add(CS$<>8__locals1.component);
			this.components.Add(CS$<>8__locals1.component);
			if (CS$<>8__locals1.component.IsActive || CS$<>8__locals1.component.UpdateWhenInactive || CS$<>8__locals1.component.Parent != null || (CS$<>8__locals1.component.IsActiveConditionals != null && CS$<>8__locals1.component.IsActiveConditionals.Any<PropertyConditional>()))
			{
				this.updateableComponents.Add(CS$<>8__locals1.component);
			}
			ItemComponent component2 = CS$<>8__locals1.component;
			component2.OnActiveStateChanged = (Action<bool>)Delegate.Combine(component2.OnActiveStateChanged, new Action<bool>(delegate(bool isActive)
			{
				bool needsSoundUpdate = false;
				if (!isActive && !CS$<>8__locals1.component.UpdateWhenInactive && !needsSoundUpdate && CS$<>8__locals1.component.Parent == null && (CS$<>8__locals1.component.IsActiveConditionals == null || !CS$<>8__locals1.component.IsActiveConditionals.Any<PropertyConditional>()))
				{
					if (CS$<>8__locals1.<>4__this.updateableComponents.Contains(CS$<>8__locals1.component))
					{
						CS$<>8__locals1.<>4__this.updateableComponents.Remove(CS$<>8__locals1.component);
						return;
					}
				}
				else if (!CS$<>8__locals1.<>4__this.updateableComponents.Contains(CS$<>8__locals1.component))
				{
					CS$<>8__locals1.<>4__this.updateableComponents.Add(CS$<>8__locals1.component);
					CS$<>8__locals1.<>4__this.IsActive = true;
				}
			}));
			Type type = CS$<>8__locals1.component.GetType();
			CS$<>8__locals1.<AddComponent>g__CacheComponent|0(type);
			Type baseType = type.BaseType;
			while (baseType != null)
			{
				CS$<>8__locals1.<AddComponent>g__CacheComponent|0(baseType);
				baseType = baseType.BaseType;
			}
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x000492B4 File Offset: 0x000474B4
		public void EnableDrawableComponent(IDrawableComponent drawable)
		{
			if (!this.drawableComponents.Contains(drawable))
			{
				this.drawableComponents.Add(drawable);
				this.hasComponentsToDraw = true;
			}
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x000492D7 File Offset: 0x000474D7
		public void DisableDrawableComponent(IDrawableComponent drawable)
		{
			if (this.drawableComponents.Contains(drawable))
			{
				this.drawableComponents.Remove(drawable);
				this.hasComponentsToDraw = (this.drawableComponents.Count > 0);
			}
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x00049308 File Offset: 0x00047508
		public int GetComponentIndex(ItemComponent component)
		{
			return this.components.IndexOf(component);
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x00049318 File Offset: 0x00047518
		public T GetComponent<T>() where T : ItemComponent
		{
			List<ItemComponent> matchingComponents;
			if (this.componentsByType.TryGetValue(typeof(T), out matchingComponents))
			{
				return (T)((object)matchingComponents.First<ItemComponent>());
			}
			return default(T);
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x00049354 File Offset: 0x00047554
		public IEnumerable<T> GetComponents<T>()
		{
			if (typeof(T) == typeof(ItemComponent))
			{
				return this.components.Cast<T>();
			}
			List<ItemComponent> matchingComponents;
			if (this.componentsByType.TryGetValue(typeof(T), out matchingComponents))
			{
				return matchingComponents.Cast<T>();
			}
			return Enumerable.Empty<T>();
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x000493AD File Offset: 0x000475AD
		public float GetQualityModifier(Quality.StatType statType)
		{
			Quality component = this.GetComponent<Quality>();
			if (component == null)
			{
				return 0f;
			}
			return component.GetValue(statType);
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x000493C5 File Offset: 0x000475C5
		public void RemoveContained(Item contained)
		{
			ItemInventory itemInventory = this.ownInventory;
			if (itemInventory != null)
			{
				itemInventory.RemoveItem(contained);
			}
			contained.Container = null;
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x000493E0 File Offset: 0x000475E0
		public void SetTransform(Vector2 simPosition, float rotation, bool findNewHull = true, bool setPrevTransform = true, Submarine forceSubmarine = null)
		{
			if (!MathUtils.IsValid(simPosition))
			{
				string[] array = new string[6];
				array[0] = "Attempted to move the item ";
				array[1] = this.Name;
				array[2] = " to an invalid position (";
				int num = 3;
				Vector2 vector = simPosition;
				array[num] = vector.ToString();
				array[4] = ")\n";
				array[5] = Environment.StackTrace.CleanupStackTrace();
				string errorMsg = string.Concat(array);
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.SetPosition:InvalidPosition" + this.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (this.body != null)
			{
				this.body.SetTransformIgnoreContacts(simPosition, rotation, setPrevTransform);
			}
			Vector2 displayPos = ConvertUnits.ToDisplayUnits(simPosition);
			this.rect.X = (int)MathF.Round(displayPos.X - (float)this.rect.Width / 2f);
			this.rect.Y = (int)MathF.Round(displayPos.Y + (float)this.rect.Height / 2f);
			if (findNewHull)
			{
				this.FindHull();
			}
			if (forceSubmarine != null)
			{
				base.Submarine = forceSubmarine;
			}
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x000494F4 File Offset: 0x000476F4
		public bool AllowDroppingOnSwapWith(Item otherItem)
		{
			if (!this.Prefab.AllowDroppingOnSwap || otherItem == null)
			{
				return false;
			}
			if (this.Prefab.AllowDroppingOnSwapWith.Any<Identifier>())
			{
				foreach (Identifier tagOrIdentifier in this.Prefab.AllowDroppingOnSwapWith)
				{
					if (otherItem.Prefab.Identifier == tagOrIdentifier)
					{
						return true;
					}
					if (otherItem.HasTag(tagOrIdentifier))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x00049594 File Offset: 0x00047794
		public void SetActiveSprite()
		{
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00049598 File Offset: 0x00047798
		public void CheckCleanable()
		{
			Pickable pickable = this.GetComponent<Pickable>();
			if (pickable != null && !pickable.IsAttached && this.Prefab.PreferredContainers.Any<PreferredContainer>() && (this.container == null || this.container.HasTag(Barotrauma.Tags.AllowCleanup)))
			{
				if (!Item._cleanableItems.Contains(this))
				{
					Item._cleanableItems.Add(this);
					return;
				}
			}
			else
			{
				Item._cleanableItems.Remove(this);
			}
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x00049608 File Offset: 0x00047808
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move an item by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			base.Move(amount, ignoreContacts);
			if (Item.ItemList != null && this.body != null)
			{
				if (ignoreContacts)
				{
					this.body.SetTransformIgnoreContacts(this.body.SimPosition + ConvertUnits.ToSimUnits(amount), this.body.Rotation, true);
				}
				else
				{
					this.body.SetTransform(this.body.SimPosition + ConvertUnits.ToSimUnits(amount), this.body.Rotation, true);
				}
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Move(amount, ignoreContacts);
			}
			if (this.body == null)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					return;
				}
			}
			Submarine submarine = base.Submarine;
			if (submarine == null || !submarine.Loading)
			{
				this.FindHull();
			}
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00049760 File Offset: 0x00047960
		public Rectangle TransformTrigger(Rectangle trigger, bool world = false)
		{
			Rectangle baseRect = world ? base.WorldRect : this.Rect;
			Rectangle transformedRect = new Rectangle((int)((float)baseRect.X + (float)trigger.X * this.Scale), (int)((float)baseRect.Y + (float)trigger.Y * this.Scale), (trigger.Width == 0) ? this.Rect.Width : ((int)((float)trigger.Width * this.Scale)), (trigger.Height == 0) ? this.Rect.Height : ((int)((float)trigger.Height * this.Scale)));
			if (base.FlippedX)
			{
				transformedRect.X = baseRect.X + (baseRect.Right - transformedRect.Right);
			}
			if (base.FlippedY)
			{
				transformedRect.Y = baseRect.Y + (baseRect.Y - baseRect.Height - (transformedRect.Y - transformedRect.Height));
			}
			return transformedRect;
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x00049854 File Offset: 0x00047A54
		public override Quad2D GetTransformedQuad()
		{
			return Quad2D.FromSubmarineRectangle(this.rect).Rotated(-this.RotationRad);
		}

		// Token: 0x06000808 RID: 2056 RVA: 0x00049880 File Offset: 0x00047A80
		public static void UpdateHulls()
		{
			foreach (Item item in Item.ItemList)
			{
				item.FindHull();
			}
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x000498D4 File Offset: 0x00047AD4
		public Hull FindHull()
		{
			if (this.parentInventory != null && this.parentInventory.Owner != null)
			{
				Character character = this.parentInventory.Owner as Character;
				if (character != null)
				{
					this.CurrentHull = character.AnimController.CurrentHull;
				}
				else
				{
					Item item = this.parentInventory.Owner as Item;
					if (item != null)
					{
						this.CurrentHull = item.CurrentHull;
					}
				}
				base.Submarine = this.parentInventory.Owner.Submarine;
				if (this.body != null)
				{
					this.body.Submarine = base.Submarine;
				}
				return this.CurrentHull;
			}
			this.CurrentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
			if (this.body != null && this.body.Enabled && (this.body.BodyType == BodyType.Dynamic || base.Submarine == null))
			{
				Hull hull = this.CurrentHull;
				base.Submarine = ((hull != null) ? hull.Submarine : null);
				this.body.Submarine = base.Submarine;
			}
			return this.CurrentHull;
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x000499EC File Offset: 0x00047BEC
		private void RefreshRootContainer()
		{
			Item newRootContainer = null;
			this.inWaterProofContainer = false;
			if (this.Container != null)
			{
				Item rootContainer = this.Container;
				this.inWaterProofContainer |= this.Container.WaterProof;
				while (rootContainer.Container != null)
				{
					rootContainer = rootContainer.Container;
					if (rootContainer == this)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Invalid container hierarchy: \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\" was contained inside itself!\n");
						defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						rootContainer = null;
						break;
					}
					this.inWaterProofContainer |= rootContainer.WaterProof;
				}
				newRootContainer = rootContainer;
			}
			if (newRootContainer != this.RootContainer)
			{
				this.RootContainer = newRootContainer;
				this.IsActive = true;
				foreach (Item containedItem in this.ContainedItems)
				{
					containedItem.RefreshRootContainer();
				}
			}
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x00049B08 File Offset: 0x00047D08
		private void RefreshInWaterProofContainer()
		{
			this.inWaterProofContainer = false;
			if (this.container == null)
			{
				return;
			}
			if (this.container.WaterProof || this.container.inWaterProofContainer)
			{
				this.inWaterProofContainer = true;
			}
			foreach (Item containedItem in this.ContainedItems)
			{
				containedItem.RefreshInWaterProofContainer();
			}
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00049B88 File Offset: 0x00047D88
		public bool HasAccess(Character character)
		{
			if (character.IsBot && this.IgnoreByAI(character))
			{
				return false;
			}
			if (!this.IsInteractable(character))
			{
				return false;
			}
			ItemContainer itemContainer = this.GetComponent<ItemContainer>();
			if (itemContainer != null && !itemContainer.HasAccess(character))
			{
				return false;
			}
			if (this.Container != null && !this.Container.HasAccess(character))
			{
				return false;
			}
			Pickable component = this.GetComponent<Pickable>();
			return component == null || component.CanBePicked;
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x00049BF8 File Offset: 0x00047DF8
		public bool IsOwnedBy(Entity entity)
		{
			return this.FindParentInventory((Inventory i) => i.Owner == entity) != null;
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x00049C28 File Offset: 0x00047E28
		public Entity GetRootInventoryOwner()
		{
			if (this.ParentInventory == null)
			{
				return this;
			}
			if (this.ParentInventory.Owner is Character)
			{
				return this.ParentInventory.Owner;
			}
			Item item = this.RootContainer;
			object obj;
			if (item == null)
			{
				obj = null;
			}
			else
			{
				Inventory inventory = item.ParentInventory;
				obj = ((inventory != null) ? inventory.Owner : null);
			}
			if (obj is Character)
			{
				return this.RootContainer.ParentInventory.Owner;
			}
			return this.RootContainer ?? this;
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x00049CA0 File Offset: 0x00047EA0
		public Inventory FindParentInventory(Func<Inventory, bool> predicate)
		{
			if (this.parentInventory != null)
			{
				if (predicate(this.parentInventory))
				{
					return this.parentInventory;
				}
				Item owner = this.parentInventory.Owner as Item;
				if (owner != null)
				{
					return owner.FindParentInventory(predicate);
				}
			}
			return null;
		}

		// Token: 0x06000810 RID: 2064 RVA: 0x00049CE8 File Offset: 0x00047EE8
		public void SetContainedItemPositions()
		{
			foreach (ItemInventory ownInventory in this.OwnInventories)
			{
				ownInventory.Container.SetContainedItemPositions();
			}
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x00049D1F File Offset: 0x00047F1F
		public void AddTag(string tag)
		{
			this.AddTag(tag.ToIdentifier());
		}

		// Token: 0x06000812 RID: 2066 RVA: 0x00049D2D File Offset: 0x00047F2D
		public void AddTag(Identifier tag)
		{
			this.tags.Add(tag);
		}

		// Token: 0x06000813 RID: 2067 RVA: 0x00049D3C File Offset: 0x00047F3C
		public void RemoveTag(Identifier tag)
		{
			if (!this.tags.Contains(tag))
			{
				return;
			}
			this.tags.Remove(tag);
		}

		// Token: 0x06000814 RID: 2068 RVA: 0x00049D5A File Offset: 0x00047F5A
		public bool HasTag(Identifier tag)
		{
			return this.tags.Contains(tag) || this.Prefab.Tags.Contains(tag);
		}

		// Token: 0x06000815 RID: 2069 RVA: 0x00049D7D File Offset: 0x00047F7D
		public bool HasIdentifierOrTags(IEnumerable<Identifier> identifiersOrTags)
		{
			return identifiersOrTags.Contains(this.Prefab.Identifier) || this.HasTag(identifiersOrTags);
		}

		// Token: 0x06000816 RID: 2070 RVA: 0x00049D9B File Offset: 0x00047F9B
		public void ReplaceTag(string tag, string newTag)
		{
			this.ReplaceTag(tag.ToIdentifier(), newTag.ToIdentifier());
		}

		// Token: 0x06000817 RID: 2071 RVA: 0x00049DAF File Offset: 0x00047FAF
		public void ReplaceTag(Identifier tag, Identifier newTag)
		{
			if (!this.tags.Contains(tag))
			{
				return;
			}
			this.tags.Remove(tag);
			this.tags.Add(newTag);
		}

		// Token: 0x06000818 RID: 2072 RVA: 0x00049DDA File Offset: 0x00047FDA
		public IReadOnlyCollection<Identifier> GetTags()
		{
			return this.tags;
		}

		// Token: 0x06000819 RID: 2073 RVA: 0x00049DE4 File Offset: 0x00047FE4
		public bool HasTag(IEnumerable<Identifier> allowedTags)
		{
			foreach (Identifier tag in allowedTags)
			{
				if (this.HasTag(tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x00049E38 File Offset: 0x00048038
		public bool ConditionalMatches(PropertyConditional conditional)
		{
			return this.ConditionalMatches(conditional, true);
		}

		// Token: 0x0600081B RID: 2075 RVA: 0x00049E44 File Offset: 0x00048044
		public bool ConditionalMatches(PropertyConditional conditional, bool checkContainer)
		{
			if (checkContainer && conditional.TargetContainer)
			{
				if (conditional.TargetGrandParent)
				{
					Item item = this.container;
					return ((item != null) ? item.container : null) != null && this.container.container.ConditionalMatches(conditional, false);
				}
				return this.container != null && this.container.ConditionalMatches(conditional, false);
			}
			else
			{
				if (string.IsNullOrEmpty(conditional.TargetItemComponent))
				{
					return conditional.Matches(this);
				}
				PropertyConditional.LogicalOperatorType itemComponentComparison = conditional.ItemComponentComparison;
				if (itemComponentComparison == PropertyConditional.LogicalOperatorType.And)
				{
					bool matchingComponentFound = false;
					foreach (ItemComponent c in this.components)
					{
						if (Item.<ConditionalMatches>g__MatchesComponent|460_0(c, conditional))
						{
							matchingComponentFound = true;
							if (!conditional.Matches(c))
							{
								return false;
							}
						}
					}
					return matchingComponentFound;
				}
				if (itemComponentComparison == PropertyConditional.LogicalOperatorType.Or)
				{
					foreach (ItemComponent c2 in this.components)
					{
						if (Item.<ConditionalMatches>g__MatchesComponent|460_0(c2, conditional) && conditional.Matches(c2))
						{
							return true;
						}
					}
					return false;
				}
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x00049F88 File Offset: 0x00048188
		public IEnumerable<StatusEffect> GetStatusEffectsOfType(ActionType type)
		{
			if (!this.hasStatusEffectsOfType[(int)type])
			{
				return Enumerable.Empty<StatusEffect>();
			}
			return this.statusEffectLists[type];
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00049FA8 File Offset: 0x000481A8
		public void ApplyStatusEffects(ActionType type, float deltaTime, Character character = null, Limb limb = null, Entity useTarget = null, bool isNetworkEvent = false, Vector2? worldPosition = null)
		{
			if (!this.hasStatusEffectsOfType[(int)type])
			{
				return;
			}
			foreach (StatusEffect effect in this.statusEffectLists[type])
			{
				this.ApplyStatusEffect(effect, type, deltaTime, character, limb, useTarget, isNetworkEvent, false, worldPosition);
			}
		}

		// Token: 0x0600081E RID: 2078 RVA: 0x0004A018 File Offset: 0x00048218
		public void ApplyStatusEffect(StatusEffect effect, ActionType type, float deltaTime, Character character = null, Limb limb = null, Entity useTarget = null, bool isNetworkEvent = false, bool checkCondition = true, Vector2? worldPosition = null)
		{
			if (effect.ShouldWaitForInterval(this, deltaTime))
			{
				return;
			}
			if (!isNetworkEvent && checkCondition && this.condition == 0f && !effect.AllowWhenBroken && effect.type != ActionType.OnBroken)
			{
				return;
			}
			if (effect.type != type)
			{
				return;
			}
			bool hasTargets = effect.TargetIdentifiers == null;
			this.targets.Clear();
			if (effect.HasTargetType(StatusEffect.TargetType.Contained))
			{
				using (IEnumerator<Item> enumerator = this.ContainedItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item containedItem = enumerator.Current;
						if ((effect.TargetIdentifiers == null || effect.TargetIdentifiers.Contains(containedItem.Prefab.Identifier) || effect.TargetIdentifiers.Any((Identifier id) => containedItem.HasTag(id))) && (effect.TargetSlot <= -1 || this.OwnInventory.GetItemsAt(effect.TargetSlot).Contains(containedItem)))
						{
							hasTargets = true;
							this.targets.AddRange(containedItem.AllPropertyObjects);
						}
					}
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters) || effect.HasTargetType(StatusEffect.TargetType.NearbyItems))
			{
				effect.AddNearbyTargets(this.WorldPosition, this.targets);
				if (this.targets.Count > 0)
				{
					hasTargets = true;
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.UseTarget))
			{
				ISerializableEntity serializableTarget = useTarget as ISerializableEntity;
				if (serializableTarget != null)
				{
					hasTargets = true;
					this.targets.Add(serializableTarget);
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.LinkedEntities))
			{
				foreach (MapEntity linkedEntity in this.linkedTo)
				{
					Item linkedItem = linkedEntity as Item;
					if (linkedItem != null)
					{
						this.targets.AddRange(linkedItem.AllPropertyObjects);
					}
					else
					{
						ISerializableEntity serializableEntity = linkedEntity as ISerializableEntity;
						if (serializableEntity != null)
						{
							this.targets.Add(serializableEntity);
						}
					}
				}
			}
			if (!hasTargets)
			{
				return;
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Hull) && this.CurrentHull != null)
			{
				this.targets.Add(this.CurrentHull);
			}
			if (effect.HasTargetType(StatusEffect.TargetType.This))
			{
				foreach (ISerializableEntity pobject in this.AllPropertyObjects)
				{
					this.targets.Add(pobject);
				}
			}
			if (character != null)
			{
				if (effect.HasTargetType(StatusEffect.TargetType.Character))
				{
					if (type == ActionType.OnContained)
					{
						CharacterInventory characterInventory = this.ParentInventory as CharacterInventory;
						if (characterInventory != null)
						{
							this.targets.Add(characterInventory.Owner as ISerializableEntity);
							goto IL_2A3;
						}
					}
					this.targets.Add(character);
				}
				IL_2A3:
				if (effect.HasTargetType(StatusEffect.TargetType.AllLimbs))
				{
					this.targets.AddRange(character.AnimController.Limbs);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Limb) && limb == null && effect.targetLimbs != null)
				{
					foreach (Limb characterLimb in character.AnimController.Limbs)
					{
						if (effect.targetLimbs.Contains(characterLimb.type))
						{
							this.targets.Add(characterLimb);
						}
					}
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Limb) && limb != null)
			{
				this.targets.Add(limb);
			}
			if (this.Container != null && effect.HasTargetType(StatusEffect.TargetType.Parent))
			{
				this.targets.AddRange(this.Container.AllPropertyObjects);
			}
			effect.Apply(type, deltaTime, this, this.targets, worldPosition);
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x0004A3CC File Offset: 0x000485CC
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			if (this.Indestructible || this.InvulnerableToDamage)
			{
				return default(AttackResult);
			}
			float damageAmount = attack.GetItemDamage(deltaTime, this.Prefab.ItemDamageMultiplier);
			this.Condition -= damageAmount;
			if (damageAmount >= this.Prefab.OnDamagedThreshold)
			{
				this.ApplyStatusEffects(ActionType.OnDamaged, 1f, null, null, null, false, null);
			}
			return new AttackResult(damageAmount, null);
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x0004A444 File Offset: 0x00048644
		private void SetCondition(float value, bool isNetworkEvent, bool executeEffects = true)
		{
			Item.<>c__DisplayClass466_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (!MathUtils.IsValid(value))
			{
				return;
			}
			if (this.Indestructible)
			{
				return;
			}
			if (this.InvulnerableToDamage && value <= this.condition)
			{
				return;
			}
			bool wasInFullCondition = this.IsFullCondition;
			float diff = value - this.condition;
			Door door = this.GetComponent<Door>();
			if (door != null && door.IsStuck && diff < 0f)
			{
				float dmg = -diff;
				float prevStuck = door.Stuck;
				door.Stuck -= dmg;
				if (door.IsStuck)
				{
					return;
				}
				float damageReduction = dmg - prevStuck;
				if (damageReduction < 0f)
				{
					return;
				}
				value -= damageReduction;
			}
			this.condition = MathHelper.Clamp(value, 0f, this.MaxCondition);
			if (MathUtils.NearlyEqual(this.prevCondition, value, 1E-06f))
			{
				return;
			}
			this.RecalculateConditionValues();
			CS$<>8__locals1.wasPreviousConditionChanged = false;
			if (this.condition == 0f && this.prevCondition > 0f)
			{
				Item.<SetCondition>g__flagChangedConnections|466_1(this.connections);
				this.<SetCondition>g__SetPreviousCondition|466_0(ref CS$<>8__locals1);
				if (executeEffects)
				{
					this.ApplyStatusEffects(ActionType.OnBroken, 1f, null, null, null, false, null);
				}
			}
			else if (this.condition > 0f && this.prevCondition <= 0f)
			{
				Item.<SetCondition>g__flagChangedConnections|466_1(this.connections);
			}
			this.SetActiveSprite();
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				bool needsConditionUpdate = false;
				if (!MathUtils.NearlyEqual(this.lastSentCondition, this.condition, 0.0001f) && (this.condition <= 0f || this.condition >= this.MaxCondition))
				{
					this.sendConditionUpdateTimer = 0f;
					needsConditionUpdate = true;
				}
				else if (Math.Abs(this.lastSentCondition - this.condition) > 1f || wasInFullCondition != this.IsFullCondition)
				{
					needsConditionUpdate = true;
				}
				if (needsConditionUpdate && !Item.itemsWithPendingConditionUpdates.Contains(this))
				{
					Item.itemsWithPendingConditionUpdates.Add(this);
				}
			}
			if (!CS$<>8__locals1.wasPreviousConditionChanged)
			{
				this.<SetCondition>g__SetPreviousCondition|466_0(ref CS$<>8__locals1);
			}
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x0004A65C File Offset: 0x0004885C
		public void RecalculateConditionValues()
		{
			this.MaxCondition = this.Prefab.Health * this.healthMultiplier * this.conditionMultiplierCampaign * this.maxRepairConditionMultiplier * (1f + this.GetQualityModifier(Barotrauma.Items.Components.Quality.StatType.Condition));
			this.IsFullCondition = MathUtils.NearlyEqual(this.Condition, this.MaxCondition, 0.0001f);
			this.ConditionPercentage = MathUtils.Percentage(this.Condition, this.MaxCondition);
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x0004A6D0 File Offset: 0x000488D0
		private bool IsInWater()
		{
			if (this.CurrentHull == null)
			{
				return true;
			}
			float surfaceY = this.CurrentHull.Surface;
			return this.CurrentHull.WaterVolume > 0f && this.Position.Y < surfaceY;
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x0004A718 File Offset: 0x00048918
		public void SendPendingNetworkUpdates()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsServer)
			{
				return;
			}
			if (!Item.itemsWithPendingConditionUpdates.Contains(this))
			{
				return;
			}
			this.SendPendingNetworkUpdatesInternal();
			Item.itemsWithPendingConditionUpdates.Remove(this);
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x0004A757 File Offset: 0x00048957
		private void SendPendingNetworkUpdatesInternal()
		{
			this.CreateStatusEvent(false);
			this.lastSentCondition = this.condition;
			this.sendConditionUpdateTimer = NetConfig.ItemConditionUpdateInterval;
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x0004A778 File Offset: 0x00048978
		public void CreateStatusEvent(bool loadingRound)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer && this.condition <= 0f && StatusEffect.DurationList.Any((DurationListElement d) => d.Targets.Contains(this) && d.Parent.HasTag(Barotrauma.Tags.OnFireStatusEffectTag)))
			{
				GameMain.NetworkMember.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(ActionType.OnFire, null, null, null, null, null));
			}
			GameMain.NetworkMember.CreateEntityEvent(this, new Item.ItemStatusEventData(loadingRound));
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x0004A7F8 File Offset: 0x000489F8
		public static void UpdatePendingConditionUpdates(float deltaTime)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsServer)
			{
				return;
			}
			for (int i = 0; i < Item.itemsWithPendingConditionUpdates.Count; i++)
			{
				Item item = Item.itemsWithPendingConditionUpdates[i];
				if (item == null || item.Removed)
				{
					Item.itemsWithPendingConditionUpdates.RemoveAt(i--);
				}
				else
				{
					Submarine submarine = item.Submarine;
					if (submarine == null || !submarine.Loading)
					{
						item.sendConditionUpdateTimer -= deltaTime;
						if (item.sendConditionUpdateTimer <= 0f)
						{
							item.SendPendingNetworkUpdatesInternal();
							Item.itemsWithPendingConditionUpdates.RemoveAt(i--);
						}
					}
				}
			}
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x0004A898 File Offset: 0x00048A98
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.IsActive || base.IsLayerHidden || this.IsInRemoveQueue)
			{
				return;
			}
			if (this.impactQueue != null)
			{
				float impact;
				while (this.impactQueue.TryDequeue(out impact))
				{
					this.ReceiveImpact(impact, true);
				}
			}
			if (this.isDroppedStackOwner && this.body != null)
			{
				foreach (Item item in this.droppedStack)
				{
					if (item != this)
					{
						item.body.Enabled = false;
						item.body.SetTransformIgnoreContacts(this.SimPosition, this.body.Rotation, true);
					}
				}
			}
			if (this.aiTarget != null && this.aiTarget.NeedsUpdate)
			{
				this.aiTarget.Update(deltaTime);
			}
			ActionType containedEffectType = (this.parentInventory == null) ? ActionType.OnNotContained : ActionType.OnContained;
			ActionType type = ActionType.Always;
			CharacterInventory characterInventory = this.parentInventory as CharacterInventory;
			this.ApplyStatusEffects(type, deltaTime, ((characterInventory != null) ? characterInventory.Owner : null) as Character, null, null, false, null);
			ActionType type2 = containedEffectType;
			CharacterInventory characterInventory2 = this.parentInventory as CharacterInventory;
			this.ApplyStatusEffects(type2, deltaTime, ((characterInventory2 != null) ? characterInventory2.Owner : null) as Character, null, null, false, null);
			for (int i = 0; i < this.updateableComponents.Count; i++)
			{
				ItemComponent ic = this.updateableComponents[i];
				bool flag;
				if (ic.InheritParentIsActive)
				{
					ItemComponent parent = ic.Parent;
					flag = (parent != null && !parent.IsActive);
				}
				else
				{
					flag = false;
				}
				bool isParentInActive = flag;
				if (ic.IsActiveConditionals != null && !isParentInActive)
				{
					if (ic.IsActiveConditionalComparison == PropertyConditional.LogicalOperatorType.And)
					{
						bool shouldBeActive = true;
						foreach (PropertyConditional conditional in ic.IsActiveConditionals)
						{
							if (!this.ConditionalMatches(conditional))
							{
								shouldBeActive = false;
								break;
							}
						}
						ic.IsActive = shouldBeActive;
					}
					else
					{
						bool shouldBeActive2 = false;
						foreach (PropertyConditional conditional2 in ic.IsActiveConditionals)
						{
							if (this.ConditionalMatches(conditional2))
							{
								shouldBeActive2 = true;
								break;
							}
						}
						ic.IsActive = shouldBeActive2;
					}
				}
				ic.WasUsed = false;
				ic.WasSecondaryUsed = false;
				if (ic.IsActive || ic.UpdateWhenInactive)
				{
					if (!ic.UpdateWhenBroken && this.condition <= 0f)
					{
						ic.UpdateBroken(deltaTime, cam);
					}
					else
					{
						ic.Update(deltaTime, cam);
					}
				}
			}
			if (base.Removed)
			{
				return;
			}
			bool needsWaterCheck = this.hasInWaterStatusEffects || this.hasNotInWaterStatusEffects;
			if (this.body != null && this.body.Enabled)
			{
				if (Math.Abs(this.body.LinearVelocity.X) > 0.01f || Math.Abs(this.body.LinearVelocity.Y) > 0.01f || this.transformDirty)
				{
					if (this.body.CollisionCategories != Category.None)
					{
						this.UpdateTransform();
					}
					if (this.CurrentHull == null && Level.Loaded != null && this.body.SimPosition.Y < ConvertUnits.ToSimUnits(-1000000))
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner == null)
						{
							return;
						}
						spawner.AddItemToRemoveQueue(this);
						return;
					}
				}
				needsWaterCheck = true;
				this.UpdateNetPosition(deltaTime);
				if (this.inWater)
				{
					this.ApplyWaterForces();
					Hull hull = this.CurrentHull;
					if (hull != null)
					{
						hull.ApplyFlowForces(deltaTime, this);
					}
				}
			}
			if (needsWaterCheck)
			{
				bool wasInWater = this.inWater;
				this.inWater = (!this.inWaterProofContainer && this.IsInWater());
				if (this.inWater && !wasInWater && this.CurrentHull != null && this.body != null && this.body.LinearVelocity.Y < -1f)
				{
					Projectile component = this.GetComponent<Projectile>();
					if (component == null || !component.IsActive)
					{
						this.body.LinearVelocity *= 0.2f;
					}
				}
				if ((this.hasInWaterStatusEffects || this.hasNotInWaterStatusEffects) && this.condition > 0f)
				{
					this.ApplyStatusEffects(this.inWater ? ActionType.InWater : ActionType.NotInWater, deltaTime, null, null, null, false, null);
				}
				if (this.inWaterProofContainer && !this.hasNotInWaterStatusEffects)
				{
					needsWaterCheck = false;
				}
			}
			if (!needsWaterCheck && this.updateableComponents.Count == 0 && (this.aiTarget == null || !this.aiTarget.NeedsUpdate) && !this.hasStatusEffectsOfType[0] && !this.hasStatusEffectsOfType[(int)containedEffectType] && (this.body == null || !this.body.Enabled))
			{
				this.IsActive = false;
			}
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x0004AD84 File Offset: 0x00048F84
		public void UpdateTransform()
		{
			if (this.body == null)
			{
				return;
			}
			Submarine prevSub = base.Submarine;
			Projectile projectile = this.GetComponent<Projectile>();
			if (((projectile != null) ? projectile.StickTarget : null) != null)
			{
				Limb limb = ((projectile != null) ? projectile.StickTarget.UserData : null) as Limb;
				if (limb != null && limb.character != null)
				{
					base.Submarine = (this.body.Submarine = limb.character.Submarine);
					this.currentHull = limb.character.CurrentHull;
				}
				else
				{
					Structure structure = projectile.StickTarget.UserData as Structure;
					if (structure != null)
					{
						base.Submarine = (this.body.Submarine = structure.Submarine);
						this.currentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
					}
					else
					{
						Item targetItem = projectile.StickTarget.UserData as Item;
						if (targetItem != null)
						{
							base.Submarine = (this.body.Submarine = targetItem.Submarine);
							this.currentHull = targetItem.CurrentHull;
						}
						else if (projectile.StickTarget.UserData is Submarine)
						{
							base.Submarine = (this.body.Submarine = null);
							this.currentHull = null;
						}
					}
				}
			}
			else
			{
				this.FindHull();
			}
			if (base.Submarine == null && prevSub != null)
			{
				this.body.SetTransformIgnoreContacts(this.body.SimPosition + prevSub.SimPosition, this.body.Rotation, true);
			}
			else if (base.Submarine != null && prevSub == null)
			{
				this.body.SetTransformIgnoreContacts(this.body.SimPosition - base.Submarine.SimPosition, this.body.Rotation, true);
			}
			else if (base.Submarine != null && prevSub != null && base.Submarine != prevSub)
			{
				this.body.SetTransformIgnoreContacts(this.body.SimPosition + prevSub.SimPosition - base.Submarine.SimPosition, this.body.Rotation, true);
			}
			if (base.Submarine != prevSub)
			{
				foreach (Item containedItem in this.ContainedItems)
				{
					if (containedItem != null)
					{
						containedItem.Submarine = base.Submarine;
					}
				}
			}
			Vector2 displayPos = ConvertUnits.ToDisplayUnits(this.body.SimPosition);
			this.rect.X = (int)(displayPos.X - (float)this.rect.Width / 2f);
			this.rect.Y = (int)(displayPos.Y + (float)this.rect.Height / 2f);
			if (Math.Abs(this.body.LinearVelocity.X) > 64f || Math.Abs(this.body.LinearVelocity.Y) > 64f)
			{
				this.body.LinearVelocity = new Vector2(MathHelper.Clamp(this.body.LinearVelocity.X, -64f, 64f), MathHelper.Clamp(this.body.LinearVelocity.Y, -64f, 64f));
			}
			this.transformDirty = false;
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x0004B0EC File Offset: 0x000492EC
		private void ApplyWaterForces()
		{
			if (this.body.Mass <= 0f || this.body.Density <= 0f || this.body.BodyType != BodyType.Dynamic)
			{
				return;
			}
			float forceFactor = 1f;
			if (this.CurrentHull != null)
			{
				float floor = (float)(this.CurrentHull.Rect.Y - this.CurrentHull.Rect.Height);
				float waterLevel = floor + this.CurrentHull.WaterVolume / (float)this.CurrentHull.Rect.Width;
				forceFactor = Math.Min((waterLevel - this.Position.Y) / (float)this.rect.Height, 1f);
				if (forceFactor <= 0f)
				{
					return;
				}
			}
			bool moving = this.body.LinearVelocity.LengthSquared() > 0.001f;
			float volume = this.body.Mass / this.body.Density;
			if (moving)
			{
				Vector2 localFront = this.body.GetLocalFront(null);
				Vector2 frontVel = this.body.FarseerBody.GetLinearVelocityFromLocalPoint(localFront);
				float speed = frontVel.Length();
				float drag = speed * speed * this.WaterDragCoefficient * volume * 10f;
				if (this.body.FarseerBody.IsBullet)
				{
					drag *= 0.1f;
				}
				Vector2 dragVec = -frontVel / speed * drag;
				Vector2 back = this.body.FarseerBody.GetWorldPoint(-localFront * 0.01f);
				this.body.ApplyForce(dragVec, back);
			}
			if (moving || this.body.Density <= 10f)
			{
				Vector2 buoyancy = -GameMain.World.Gravity * this.body.FarseerBody.GravityScale * forceFactor * volume * 10f;
				this.body.ApplyForce(buoyancy, 64f);
			}
			if (Math.Abs(this.body.AngularVelocity) > 0.0001f)
			{
				this.body.ApplyTorque(this.body.AngularVelocity * volume * -0.1f);
			}
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x0004B330 File Offset: 0x00049530
		private bool OnCollision(Fixture f1, Fixture f2, Contact contact)
		{
			if (this.transformDirty)
			{
				return false;
			}
			Projectile projectile = this.GetComponent<Projectile>();
			if (projectile != null)
			{
				if (f2.CollisionCategories == Category.Cat2)
				{
					return false;
				}
				if (projectile.IgnoredBodies != null && projectile.IgnoredBodies.Contains(f2.Body))
				{
					return false;
				}
				if (projectile.ShouldIgnoreSubmarineCollision(f2, contact))
				{
					return false;
				}
			}
			if (GameMain.GameSession == null || GameMain.GameSession.RoundDuration > 1f)
			{
				Vector2 normal;
				FixedArray2<Vector2> fixedArray;
				contact.GetWorldManifold(out normal, out fixedArray);
				if (contact.FixtureA.Body == f1.Body)
				{
					normal = -normal;
				}
				float impact = Vector2.Dot(f1.Body.LinearVelocity, -normal);
				if (this.impactQueue == null)
				{
					this.impactQueue = new ConcurrentQueue<float>();
				}
				this.impactQueue.Enqueue(impact);
			}
			this.IsActive = true;
			return true;
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x0004B400 File Offset: 0x00049600
		public void ReceiveImpact(float impactStrength, bool recursive = true)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			if (this.ImpactTolerance > 0f && Math.Abs(impactStrength) > this.ImpactTolerance && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.ImpactDamageProbability)
			{
				if (this.ImpactDamage != 0f)
				{
					this.Condition -= impactStrength * this.ImpactDamage;
				}
				if (this.hasStatusEffectsOfType[14])
				{
					foreach (StatusEffect effect in this.statusEffectLists[ActionType.OnImpact])
					{
						this.ApplyStatusEffect(effect, ActionType.OnImpact, 1f, null, null, null, false, true, null);
					}
					GameServer server = GameMain.Server;
					if (server != null)
					{
						server.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(ActionType.OnImpact, null, null, null, null, null));
					}
				}
			}
			if (!recursive)
			{
				return;
			}
			foreach (Item contained in this.ContainedItems)
			{
				if (contained.body != null)
				{
					contained.ReceiveImpact(impactStrength, true);
				}
			}
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x0004B568 File Offset: 0x00049768
		public override void FlipX(bool relativeToSub, bool force = false)
		{
			base.FlipX(relativeToSub, false);
			if (!this.Prefab.CanFlipX && !force)
			{
				base.FlippedX = false;
				return;
			}
			if (this.Prefab.AllowRotatingInEditor)
			{
				this.RotationRad = MathUtils.WrapAnglePi(-this.RotationRad);
			}
			foreach (ItemComponent component in this.components)
			{
				component.FlipX(relativeToSub);
			}
			this.SetContainedItemPositions();
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x0004B600 File Offset: 0x00049800
		public override void FlipY(bool relativeToSub, bool force = false)
		{
			base.FlipY(relativeToSub, false);
			if (!this.Prefab.CanFlipY && !force)
			{
				base.FlippedY = false;
				return;
			}
			if (this.Prefab.AllowRotatingInEditor)
			{
				this.RotationRad = MathUtils.WrapAngleTwoPi(-this.RotationRad);
			}
			foreach (ItemComponent component in this.components)
			{
				component.FlipY(relativeToSub);
			}
			this.SetContainedItemPositions();
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x0004B698 File Offset: 0x00049898
		public T GetDirectlyConnectedComponent<T>(Func<Connection, bool> connectionFilter = null) where T : ItemComponent
		{
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel == null)
			{
				return default(T);
			}
			foreach (Connection c in connectionPanel.Connections)
			{
				if (connectionFilter == null || connectionFilter(c))
				{
					foreach (Connection recipient in c.Recipients)
					{
						T component = recipient.Item.GetComponent<T>();
						if (component != null)
						{
							return component;
						}
					}
				}
			}
			return default(T);
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x0004B76C File Offset: 0x0004996C
		public List<T> GetConnectedComponents<T>(bool recursive = false, bool allowTraversingBackwards = true, Func<Connection, bool> connectionFilter = null) where T : ItemComponent
		{
			List<T> connectedComponents = new List<T>();
			if (recursive)
			{
				HashSet<Connection> alreadySearched = new HashSet<Connection>();
				this.GetConnectedComponentsRecursive<T>(alreadySearched, connectedComponents, false, allowTraversingBackwards);
				return connectedComponents;
			}
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel == null)
			{
				return connectedComponents;
			}
			foreach (Connection c in connectionPanel.Connections)
			{
				if (connectionFilter == null || connectionFilter(c))
				{
					foreach (Connection recipient in c.Recipients)
					{
						T component = recipient.Item.GetComponent<T>();
						if (component != null && !connectedComponents.Contains(component))
						{
							connectedComponents.Add(component);
						}
					}
				}
			}
			return connectedComponents;
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x0004B858 File Offset: 0x00049A58
		private void GetConnectedComponentsRecursive<T>(HashSet<Connection> alreadySearched, List<T> connectedComponents, bool ignoreInactiveRelays = false, bool allowTraversingBackwards = true) where T : ItemComponent
		{
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel == null)
			{
				return;
			}
			foreach (Connection c in connectionPanel.Connections)
			{
				if (alreadySearched.Add(c))
				{
					this.GetConnectedComponentsRecursive<T>(c, alreadySearched, connectedComponents, ignoreInactiveRelays, allowTraversingBackwards);
				}
			}
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x0004B8C4 File Offset: 0x00049AC4
		public List<T> GetConnectedComponentsRecursive<T>(Connection c, bool ignoreInactiveRelays = false, bool allowTraversingBackwards = true) where T : ItemComponent
		{
			List<T> connectedComponents = new List<T>();
			HashSet<Connection> alreadySearched = new HashSet<Connection>();
			this.GetConnectedComponentsRecursive<T>(c, alreadySearched, connectedComponents, ignoreInactiveRelays, allowTraversingBackwards);
			return connectedComponents;
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x0004B8EC File Offset: 0x00049AEC
		private void GetConnectedComponentsRecursive<T>(Connection c, HashSet<Connection> alreadySearched, List<T> connectedComponents, bool ignoreInactiveRelays, bool allowTraversingBackwards = true) where T : ItemComponent
		{
			Item.<>c__DisplayClass489_0<T> CS$<>8__locals1;
			CS$<>8__locals1.alreadySearched = alreadySearched;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.connectedComponents = connectedComponents;
			CS$<>8__locals1.ignoreInactiveRelays = ignoreInactiveRelays;
			CS$<>8__locals1.allowTraversingBackwards = allowTraversingBackwards;
			CS$<>8__locals1.c = c;
			CS$<>8__locals1.alreadySearched.Add(CS$<>8__locals1.c);
			foreach (Connection recipient in Item.<GetConnectedComponentsRecursive>g__GetRecipients|489_0<T>(CS$<>8__locals1.c))
			{
				if (!CS$<>8__locals1.alreadySearched.Contains(recipient))
				{
					T component = recipient.Item.GetComponent<T>();
					if (component != null && !CS$<>8__locals1.connectedComponents.Contains(component))
					{
						CS$<>8__locals1.connectedComponents.Add(component);
					}
					CircuitBox circuitBox = recipient.Item.GetComponent<CircuitBox>();
					CircuitBoxConnection cbConnection;
					if (circuitBox != null && circuitBox.FindInputOutputConnection(recipient).TryUnwrap(out cbConnection))
					{
						CircuitBoxInputConnection inputConnection = cbConnection as CircuitBoxInputConnection;
						if (inputConnection != null)
						{
							using (List<CircuitBoxConnection>.Enumerator enumerator2 = inputConnection.ExternallyConnectedTo.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									CircuitBoxConnection connectedTo = enumerator2.Current;
									if (!CS$<>8__locals1.alreadySearched.Contains(connectedTo.Connection))
									{
										this.<GetConnectedComponentsRecursive>g__CheckRecipient|489_1<T>(connectedTo.Connection, ref CS$<>8__locals1);
									}
								}
								goto IL_18B;
							}
						}
						foreach (CircuitBoxConnection connectedFrom in cbConnection.ExternallyConnectedFrom)
						{
							if (!CS$<>8__locals1.alreadySearched.Contains(connectedFrom.Connection) && CS$<>8__locals1.allowTraversingBackwards)
							{
								this.<GetConnectedComponentsRecursive>g__CheckRecipient|489_1<T>(connectedFrom.Connection, ref CS$<>8__locals1);
							}
						}
					}
					IL_18B:
					this.<GetConnectedComponentsRecursive>g__CheckRecipient|489_1<T>(recipient, ref CS$<>8__locals1);
				}
			}
			if (CS$<>8__locals1.ignoreInactiveRelays)
			{
				RelayComponent relay = this.GetComponent<RelayComponent>();
				if (relay != null && !relay.IsOn)
				{
					return;
				}
			}
			foreach (ValueTuple<Identifier, Identifier> valueTuple in Item.connectionPairs)
			{
				Identifier input = valueTuple.Item1;
				Identifier output = valueTuple.Item2;
				this.<GetConnectedComponentsRecursive>g__searchFromAToB|489_2<T>(input, output, ref CS$<>8__locals1);
				if (CS$<>8__locals1.allowTraversingBackwards)
				{
					this.<GetConnectedComponentsRecursive>g__searchFromAToB|489_2<T>(output, input, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x0004BB5C File Offset: 0x00049D5C
		public Controller FindController(ImmutableArray<Identifier>? tags = null)
		{
			List<Controller> controllers = this.GetConnectedComponents<Controller>(false, true, null);
			bool needsTag = tags != null && tags.Value.Length > 0;
			if (controllers.None(null) || (needsTag && controllers.None((Controller c) => c.Item.HasTag(tags))))
			{
				controllers = this.GetConnectedComponents<Controller>(true, true, null);
			}
			if (needsTag)
			{
				controllers.RemoveAll((Controller c) => !c.Item.HasTag(tags));
			}
			Controller result;
			if (controllers.Count >= 2)
			{
				if ((result = controllers.FirstOrDefault((Controller c) => c.GetFocusTarget() == this)) == null)
				{
					return controllers.FirstOrDefault<Controller>();
				}
			}
			else
			{
				result = controllers.FirstOrDefault<Controller>();
			}
			return result;
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x0004BC24 File Offset: 0x00049E24
		public bool TryFindController(out Controller controller, ImmutableArray<Identifier>? tags = null)
		{
			controller = this.FindController(tags);
			return controller != null;
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x0004BC34 File Offset: 0x00049E34
		public void SendSignal(string signal, string connectionName)
		{
			this.SendSignal(new Signal(signal, 0, null, null, 0f, 1f), connectionName);
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x0004BC50 File Offset: 0x00049E50
		public void SendSignal(Signal signal, string connectionName)
		{
			if (this.connections == null)
			{
				return;
			}
			Connection connection;
			if (!this.connections.TryGetValue(connectionName, out connection))
			{
				return;
			}
			ref Item ptr = ref signal.source;
			if (ptr == null)
			{
				ptr = this;
			}
			this.SendSignal(signal, connection);
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x0004BC90 File Offset: 0x00049E90
		public void SendSignal(Signal signal, Connection connection)
		{
			this.LastSentSignalRecipients.Clear();
			if (this.connections == null || connection == null)
			{
				return;
			}
			signal.stepsTaken++;
			if (signal.stepsTaken > 5 && signal.source != null)
			{
				int duplicateRecipients = 0;
				foreach (Connection recipient in signal.source.LastSentSignalRecipients)
				{
					if (recipient == connection)
					{
						duplicateRecipients++;
						if (duplicateRecipients > 2)
						{
							return;
						}
					}
				}
			}
			if (signal.stepsTaken > 10)
			{
				signal.stepsTaken = 0;
				bool duplicateFound = false;
				foreach (ValueTuple<Signal, Connection> s in this.delayedSignals)
				{
					if (s.Item2 == connection && s.Item1.source == signal.source && s.Item1.value == signal.value && s.Item1.sender == signal.sender)
					{
						duplicateFound = true;
						break;
					}
				}
				if (!duplicateFound)
				{
					this.delayedSignals.Add(new ValueTuple<Signal, Connection>(signal, connection));
					CoroutineManager.StartCoroutine(this.DelaySignal(signal, connection), "");
					return;
				}
			}
			else
			{
				if (connection.Effects != null && signal.value != "0" && !string.IsNullOrEmpty(signal.value))
				{
					foreach (StatusEffect effect in connection.Effects)
					{
						if (this.condition > 0f || effect.type == ActionType.OnBroken)
						{
							this.ApplyStatusEffect(effect, ActionType.OnUse, 0.016666668f, null, null, null, false, true, null);
						}
					}
				}
				ref Item ptr = ref signal.source;
				if (ptr == null)
				{
					ptr = this;
				}
				connection.SendSignal(signal);
			}
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x0004BEB0 File Offset: 0x0004A0B0
		private IEnumerable<CoroutineStatus> DelaySignal(Signal signal, Connection connection)
		{
			Item.<DelaySignal>d__496 <DelaySignal>d__ = new Item.<DelaySignal>d__496(-2);
			<DelaySignal>d__.<>4__this = this;
			<DelaySignal>d__.<>3__signal = signal;
			<DelaySignal>d__.<>3__connection = connection;
			return <DelaySignal>d__;
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0004BED0 File Offset: 0x0004A0D0
		public bool IsInsideTrigger(Vector2 worldPosition)
		{
			Rectangle rectangle;
			return this.IsInsideTrigger(worldPosition, out rectangle);
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x0004BEE8 File Offset: 0x0004A0E8
		public bool IsInsideTrigger(Vector2 worldPosition, out Rectangle transformedTrigger)
		{
			foreach (Rectangle trigger in this.Prefab.Triggers)
			{
				transformedTrigger = this.TransformTrigger(trigger, true);
				if (Submarine.RectContains(transformedTrigger, worldPosition, false))
				{
					return true;
				}
			}
			transformedTrigger = Rectangle.Empty;
			return false;
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x0004BF47 File Offset: 0x0004A147
		public bool CanClientAccess(Client c)
		{
			return c != null && c.Character != null && c.Character.CanInteractWith(this, true);
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0004BF64 File Offset: 0x0004A164
		public bool TryInteract(Character user, bool ignoreRequiredItems = false, bool forceSelectKey = false, bool forceUseKey = false)
		{
			CampaignMode.InteractionType campaignInteractionType = this.CampaignInteractionType;
			Client ownerClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == user);
			if (ownerClient != null && !this.campaignInteractionTypePerClient.TryGetValue(ownerClient, out campaignInteractionType))
			{
				campaignInteractionType = CampaignMode.InteractionType.None;
			}
			if (CampaignMode.BlocksInteraction(campaignInteractionType))
			{
				return false;
			}
			bool picked = false;
			bool selected = false;
			if (!this.IsInteractable(user))
			{
				return false;
			}
			foreach (ItemComponent ic in this.components)
			{
				bool pickHit = false;
				bool selectHit = false;
				if (!(ic is Ladder) && user.IsKeyDown(InputType.Aim))
				{
					pickHit = false;
					selectHit = false;
				}
				else if (forceSelectKey)
				{
					if (ic.PickKey == InputType.Select)
					{
						pickHit = true;
					}
					if (ic.SelectKey == InputType.Select)
					{
						selectHit = true;
					}
				}
				else if (forceUseKey)
				{
					if (ic.PickKey == InputType.Use)
					{
						pickHit = true;
					}
					if (ic.SelectKey == InputType.Use)
					{
						selectHit = true;
					}
				}
				else
				{
					pickHit = user.IsKeyHit(ic.PickKey);
					selectHit = user.IsKeyHit(ic.SelectKey);
				}
				if (pickHit || selectHit)
				{
					bool showUiMsg = false;
					if ((ignoreRequiredItems || ic.HasRequiredItems(user, showUiMsg, null)) && ((ic.CanBePicked && pickHit && ic.Pick(user)) || (ic.CanBeSelected && selectHit && ic.Select(user))))
					{
						picked = true;
						ic.ApplyStatusEffects(ActionType.OnPicked, 1f, user, null, null, null, null, 1f);
						if (ic.CanBeSelected && !(ic is Door))
						{
							selected = true;
						}
					}
				}
			}
			Inventory inventory = this.ParentInventory;
			if (((inventory != null) ? inventory.Owner : null) == user && this.GetComponent<ItemContainer>() != null)
			{
				selected = false;
			}
			if (!picked)
			{
				return false;
			}
			Action onInteract = this.OnInteract;
			if (onInteract != null)
			{
				onInteract();
			}
			if (user != null)
			{
				if (user.SelectedItem == this)
				{
					if (user.IsKeyHit(InputType.Select) || forceSelectKey)
					{
						user.SelectedItem = null;
					}
				}
				else if (user.SelectedSecondaryItem == this)
				{
					if (user.IsKeyHit(InputType.Select) || forceSelectKey)
					{
						user.SelectedSecondaryItem = null;
					}
				}
				else if (selected)
				{
					if (this.IsSecondaryItem)
					{
						user.SelectedSecondaryItem = this;
					}
					else
					{
						user.SelectedItem = this;
					}
				}
			}
			if (this.Container != null)
			{
				this.Container.RemoveContained(this);
			}
			return true;
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x0004C220 File Offset: 0x0004A420
		public float GetContainedItemConditionPercentage()
		{
			if (this.ownInventory == null)
			{
				return -1f;
			}
			float condition = 0f;
			float maxCondition = 0f;
			foreach (Item item in this.ContainedItems)
			{
				condition += item.condition;
				maxCondition += item.MaxCondition;
			}
			if (maxCondition > 0f)
			{
				return condition / maxCondition;
			}
			return -1f;
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x0004C2A4 File Offset: 0x0004A4A4
		public void Use(float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
		{
			if (this.RequireAimToUse && (user == null || !user.IsKeyDown(InputType.Aim)))
			{
				return;
			}
			if (this.condition <= 0f)
			{
				return;
			}
			bool remove = false;
			foreach (ItemComponent ic in this.components)
			{
				bool isControlled = false;
				if (ic.HasRequiredContainedItems(user, isControlled, null) && ic.Use(deltaTime, user))
				{
					ic.WasUsed = true;
					ic.ApplyStatusEffects(ActionType.OnUse, deltaTime, user, targetLimb, useTarget, user, null, 1f);
					ic.OnUsed.Invoke(new ItemComponent.ItemUseInfo(this, user ?? userForOnUsedEvent));
					if (ic.DeleteOnUse)
					{
						remove = true;
					}
				}
			}
			if (remove)
			{
				Entity.Spawner.AddItemToRemoveQueue(this);
			}
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0004C380 File Offset: 0x0004A580
		public void SecondaryUse(float deltaTime, Character character = null)
		{
			if (this.condition <= 0f)
			{
				return;
			}
			bool remove = false;
			foreach (ItemComponent ic in this.components)
			{
				bool isControlled = false;
				if (ic.HasRequiredContainedItems(character, isControlled, null) && ic.SecondaryUse(deltaTime, character))
				{
					ic.WasSecondaryUsed = true;
					ic.ApplyStatusEffects(ActionType.OnSecondaryUse, deltaTime, character, null, character, character, null, 1f);
					if (ic.DeleteOnUse)
					{
						remove = true;
					}
				}
			}
			if (remove)
			{
				Entity.Spawner.AddItemToRemoveQueue(this);
			}
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0004C434 File Offset: 0x0004A634
		public void ApplyTreatment(Character user, Character character, Limb targetLimb)
		{
			if (character.IsDead)
			{
				return;
			}
			if (!this.UseInHealthInterface)
			{
				return;
			}
			if (this.Prefab.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
			{
				GameAnalyticsManager.AddDesignEvent("ApplyTreatment:" + this.Prefab.Identifier.ToString());
			}
			bool remove = false;
			foreach (ItemComponent ic in this.components)
			{
				if (ic.HasRequiredContainedItems(user, user == Character.Controlled, null))
				{
					ActionType conditionalActionType = (Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) < ic.DegreeOfSuccess(user)) ? ActionType.OnSuccess : ActionType.OnFailure;
					ic.WasUsed = true;
					ic.ApplyStatusEffects(conditionalActionType, 1f, character, targetLimb, character, user, null, 1f);
					ic.ApplyStatusEffects(ActionType.OnUse, 1f, character, targetLimb, character, user, null, 1f);
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null && networkMember.IsServer)
					{
						GameMain.NetworkMember.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(conditionalActionType, ic, character, targetLimb, character, null));
						GameMain.NetworkMember.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(ActionType.OnUse, ic, character, targetLimb, character, null));
					}
					if (ic.DeleteOnUse)
					{
						remove = true;
					}
				}
			}
			if (user != null)
			{
				AbilityApplyTreatment abilityItem = new AbilityApplyTreatment(user, character, this, targetLimb);
				user.CheckTalents(AbilityEffectType.OnApplyTreatment, abilityItem);
			}
			if (remove)
			{
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.AddItemToRemoveQueue(this);
			}
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0004C608 File Offset: 0x0004A808
		public bool Combine(Item item, Character user)
		{
			if (item == this)
			{
				return false;
			}
			bool isCombined = false;
			foreach (ItemComponent ic in this.components)
			{
				if (ic.Combine(item, user))
				{
					isCombined = true;
				}
			}
			return isCombined;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0004C66C File Offset: 0x0004A86C
		public void Drop(Character dropper, bool createNetworkEvent = true, bool setTransform = true)
		{
			if (createNetworkEvent && this.parentInventory != null && !this.parentInventory.Owner.Removed && !base.Removed && GameMain.NetworkMember != null && (GameMain.NetworkMember.IsServer || Character.Controlled == dropper))
			{
				this.parentInventory.CreateNetworkEvent();
				this.PositionUpdateInterval = 0f;
			}
			if (this.body != null)
			{
				this.IsActive = true;
				this.body.Enabled = true;
				this.body.PhysEnabled = true;
				this.body.ResetDynamics();
				if (dropper != null)
				{
					if (this.body.Removed)
					{
						DebugConsole.ThrowError("Failed to drop the item \"" + this.Name + "\" (body has been removed" + (base.Removed ? ", item has been removed)" : ")"), null, null, false, false);
					}
					else if (setTransform)
					{
						this.body.SetTransformIgnoreContacts(dropper.SimPosition, 0f, true);
					}
				}
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Drop(dropper, setTransform);
			}
			if (this.Container != null)
			{
				if (setTransform)
				{
					this.SetTransform(this.Container.SimPosition, 0f, true, true, null);
				}
				this.Container.RemoveContained(this);
				this.Container = null;
			}
			if (this.ParentInventory != null)
			{
				this.ParentInventory.RemoveItem(this);
				this.ParentInventory = null;
			}
			this.transformDirty = true;
			this.SetContainedItemPositions();
		}

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x06000843 RID: 2115 RVA: 0x0004C80C File Offset: 0x0004AA0C
		public IEnumerable<Item> DroppedStack
		{
			get
			{
				IEnumerable<Item> enumerable = this.droppedStack;
				return enumerable ?? Enumerable.Empty<Item>();
			}
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0004C82C File Offset: 0x0004AA2C
		public void CreateDroppedStack(IEnumerable<Item> items, bool allowClientExecute)
		{
			if (!allowClientExecute)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			int itemCount = items.Count<Item>();
			if (itemCount == 1)
			{
				return;
			}
			if (items.DistinctBy((Item it) => it.Prefab).Count<Item>() > 1)
			{
				DebugConsole.ThrowError("Attempted to create a dropped stack of multiple different items (" + string.Join<Item>(", ", items.DistinctBy((Item it) => it.Prefab)) + ")\n" + Environment.StackTrace, null, null, false, false);
				return;
			}
			if (items.Any((Item it) => it.body == null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to create a dropped stack for an item with no body (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(items.First<Item>().Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (items.None(null))
			{
				DebugConsole.ThrowError("Attempted to create a dropped stack of an empty list of items.\n" + Environment.StackTrace, null, null, false, false);
				return;
			}
			int maxStackSize = items.First<Item>().Prefab.MaxStackSize;
			if (itemCount > maxStackSize)
			{
				int i = 0;
				while ((float)i < MathF.Ceiling((float)(itemCount / maxStackSize)))
				{
					int startIndex = i * maxStackSize;
					items.ElementAt(startIndex).CreateDroppedStack(items.Skip(startIndex).Take(maxStackSize), allowClientExecute);
					i++;
				}
				return;
			}
			if (this.droppedStack == null)
			{
				this.droppedStack = new List<Item>();
			}
			foreach (Item item in items)
			{
				if (!this.droppedStack.Contains(item))
				{
					this.droppedStack.Add(item);
				}
			}
			this.SetDroppedStackItemStates();
			NetworkMember server = GameMain.NetworkMember;
			if (server != null && server.IsServer)
			{
				server.CreateEntityEvent(this, new Item.DroppedStackEventData(this.droppedStack));
			}
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0004CA60 File Offset: 0x0004AC60
		private void RemoveFromDroppedStack(bool allowClientExecute)
		{
			if (!allowClientExecute)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			if (this.droppedStack == null)
			{
				return;
			}
			this.body.Enabled = (this.ParentInventory == null);
			this.isDroppedStackOwner = false;
			this.droppedStack.Remove(this);
			this.SetDroppedStackItemStates();
			this.droppedStack = null;
			NetworkMember server = GameMain.NetworkMember;
			if (server != null && server.IsServer && !base.Removed)
			{
				server.CreateEntityEvent(this, new Item.DroppedStackEventData(Enumerable.Empty<Item>()));
			}
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x0004CAF0 File Offset: 0x0004ACF0
		private void SetDroppedStackItemStates()
		{
			if (this.droppedStack == null)
			{
				return;
			}
			bool isFirst = true;
			foreach (Item item in this.droppedStack)
			{
				item.droppedStack = this.droppedStack;
				item.isDroppedStackOwner = isFirst;
				if (item.body != null)
				{
					item.body.Enabled = (item.body.PhysEnabled = isFirst);
					if (isFirst)
					{
						item.IsActive = true;
						item.body.ResetDynamics();
					}
				}
				isFirst = false;
			}
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x0004CB94 File Offset: 0x0004AD94
		public IEnumerable<Item> GetStackedItems()
		{
			Item.<GetStackedItems>d__514 <GetStackedItems>d__ = new Item.<GetStackedItems>d__514(-2);
			<GetStackedItems>d__.<>4__this = this;
			return <GetStackedItems>d__;
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x0004CBA4 File Offset: 0x0004ADA4
		public void Equip(Character character)
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Tried to equip a removed item (" + this.Name + ").\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Equip(character);
			}
			CharacterHUD.RecreateHudTextsIfControlling(character);
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x0004CC30 File Offset: 0x0004AE30
		public void Unequip(Character character)
		{
			foreach (ItemComponent ic in this.components)
			{
				ic.Unequip(character);
			}
			CharacterHUD.RecreateHudTextsIfControlling(character);
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x0004CC8C File Offset: 0x0004AE8C
		[return: TupleElementNames(new string[]
		{
			"obj",
			"property"
		})]
		public List<ValueTuple<object, SerializableProperty>> GetProperties<T>()
		{
			List<ValueTuple<object, SerializableProperty>> allProperties = new List<ValueTuple<object, SerializableProperty>>();
			List<SerializableProperty> itemProperties = SerializableProperty.GetProperties<T>(this);
			foreach (SerializableProperty itemProperty in itemProperties)
			{
				allProperties.Add(new ValueTuple<object, SerializableProperty>(this, itemProperty));
			}
			foreach (ItemComponent ic in this.components)
			{
				List<SerializableProperty> componentProperties = SerializableProperty.GetProperties<T>(ic);
				foreach (SerializableProperty componentProperty in componentProperties)
				{
					allProperties.Add(new ValueTuple<object, SerializableProperty>(ic, componentProperty));
				}
			}
			return allProperties;
		}

		// Token: 0x0600084B RID: 2123 RVA: 0x0004CD7C File Offset: 0x0004AF7C
		private void WritePropertyChange(IWriteMessage msg, Item.ChangePropertyEventData extraData, bool inGameEditableOnly)
		{
			List<ValueTuple<object, SerializableProperty>> allProperties = inGameEditableOnly ? this.GetInGameEditableProperties(true) : this.GetProperties<Editable>();
			SerializableProperty property = extraData.SerializableProperty;
			ISerializableEntity entity = extraData.Entity;
			if (property == null)
			{
				throw new ArgumentException("Failed to write propery value - property \"" + ((property == null) ? "null" : property.Name) + "\" is not serializable.");
			}
			if (allProperties.Count > 1)
			{
				if (allProperties.None(([TupleElementNames(new string[]
				{
					"obj",
					"property"
				})] ValueTuple<object, SerializableProperty> p) => p.Item2 == property && p.Item1 == entity))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find the property \"");
					defaultInterpolatedStringHandler.AppendFormatted(property.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" in \"");
					defaultInterpolatedStringHandler.AppendFormatted(entity.Name ?? "null");
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				msg.WriteIdentifier(property.Name.ToIdentifier());
			}
			object value = property.GetValue(entity);
			string stringVal = value as string;
			if (stringVal != null)
			{
				msg.WriteString(stringVal);
				return;
			}
			if (value is Identifier)
			{
				Identifier idValue = (Identifier)value;
				msg.WriteIdentifier(idValue);
				return;
			}
			if (value is float)
			{
				float floatVal = (float)value;
				msg.WriteSingle(floatVal);
				return;
			}
			if (value is int)
			{
				int intVal = (int)value;
				msg.WriteInt32(intVal);
				return;
			}
			if (value is bool)
			{
				bool boolVal = (bool)value;
				msg.WriteBoolean(boolVal);
				return;
			}
			if (value is Color)
			{
				Color color = (Color)value;
				msg.WriteByte(color.R);
				msg.WriteByte(color.G);
				msg.WriteByte(color.B);
				msg.WriteByte(color.A);
				return;
			}
			if (value is Vector2)
			{
				Vector2 vector2 = (Vector2)value;
				msg.WriteSingle(vector2.X);
				msg.WriteSingle(vector2.Y);
				return;
			}
			if (value is Vector3)
			{
				Vector3 vector3 = (Vector3)value;
				msg.WriteSingle(vector3.X);
				msg.WriteSingle(vector3.Y);
				msg.WriteSingle(vector3.Z);
				return;
			}
			if (value is Vector4)
			{
				Vector4 vector4 = (Vector4)value;
				msg.WriteSingle(vector4.X);
				msg.WriteSingle(vector4.Y);
				msg.WriteSingle(vector4.Z);
				msg.WriteSingle(vector4.W);
				return;
			}
			if (value is Point)
			{
				Point point = (Point)value;
				msg.WriteInt32(point.X);
				msg.WriteInt32(point.Y);
				return;
			}
			if (value is Rectangle)
			{
				Rectangle rect = (Rectangle)value;
				msg.WriteInt32(rect.X);
				msg.WriteInt32(rect.Y);
				msg.WriteInt32(rect.Width);
				msg.WriteInt32(rect.Height);
				return;
			}
			if (value is Enum)
			{
				msg.WriteInt32((int)value);
				return;
			}
			string[] a = value as string[];
			if (a != null)
			{
				msg.WriteInt32(a.Length);
				for (int i = 0; i < a.Length; i++)
				{
					msg.WriteString(a[i] ?? "");
				}
				return;
			}
			string str = "Serializing item properties of the type \"";
			Type type = value.GetType();
			throw new NotImplementedException(str + ((type != null) ? type.ToString() : null) + "\" not supported");
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x0004D0F0 File Offset: 0x0004B2F0
		[return: TupleElementNames(new string[]
		{
			"obj",
			"property"
		})]
		private List<ValueTuple<object, SerializableProperty>> GetInGameEditableProperties(bool ignoreConditions = false)
		{
			if (ignoreConditions)
			{
				return this.GetProperties<ConditionallyEditable>().Union(this.GetProperties<InGameEditable>()).ToList<ValueTuple<object, SerializableProperty>>();
			}
			return (from ce in this.GetProperties<ConditionallyEditable>()
			where ce.Item2.GetAttribute<ConditionallyEditable>().IsEditable(this)
			select ce).Union(this.GetProperties<InGameEditable>()).ToList<ValueTuple<object, SerializableProperty>>();
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x0004D140 File Offset: 0x0004B340
		private void ReadPropertyChange(IReadMessage msg, bool inGameEditableOnly, Client sender = null)
		{
			List<ValueTuple<object, SerializableProperty>> allProperties = inGameEditableOnly ? this.GetInGameEditableProperties(true) : this.GetProperties<Editable>();
			if (allProperties.Count == 0)
			{
				return;
			}
			Identifier propertyIdentifier = msg.ReadIdentifier();
			int propertyIndex = allProperties.IndexOf(([TupleElementNames(new string[]
			{
				"obj",
				"property"
			})] ValueTuple<object, SerializableProperty> p) => p.Item2.Name == propertyIdentifier);
			if (propertyIndex < 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(96, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("ReadPropertyChange");
				defaultInterpolatedStringHandler.AppendLiteral(". Could not find the property \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(propertyIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" in item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" (property count: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(allProperties.Count);
				defaultInterpolatedStringHandler.AppendLiteral(", in-game editable only: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(inGameEditableOnly);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			bool allowEditing = true;
			object parentObject = allProperties[propertyIndex].Item1;
			SerializableProperty property = allProperties[propertyIndex].Item2;
			if (inGameEditableOnly)
			{
				ItemComponent ic = parentObject as ItemComponent;
				if (ic != null && !ic.AllowInGameEditing)
				{
					allowEditing = false;
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				bool conditionAllowsEditing = true;
				ConditionallyEditable condition = property.GetAttribute<ConditionallyEditable>();
				if (condition != null)
				{
					conditionAllowsEditing = condition.IsEditable(this);
				}
				Item item = this.Container;
				CircuitBox cb = (item != null) ? item.GetComponent<CircuitBox>() : null;
				bool canAccess;
				if (cb != null && this.Container.CanClientAccess(sender))
				{
					canAccess = !cb.IsLocked();
				}
				else
				{
					canAccess = this.CanClientAccess(sender);
				}
				if (!canAccess || !conditionAllowsEditing)
				{
					allowEditing = false;
				}
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventItemReadPropertyChange>(delegate(IEventItemReadPropertyChange x)
			{
				bool? flag = x.OnItemReadPropertyChange(this, property, parentObject, allowEditing, sender);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			Type type = property.PropertyType;
			string logValue = "";
			if (type == typeof(string))
			{
				string val = msg.ReadString();
				Editable editableAttribute = property.GetAttribute<Editable>();
				if (editableAttribute != null && editableAttribute.MaxLength > 0 && val.Length > editableAttribute.MaxLength)
				{
					val = val.Substring(0, editableAttribute.MaxLength);
				}
				logValue = val;
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val);
				}
			}
			else if (type == typeof(Identifier))
			{
				Identifier val2 = msg.ReadIdentifier();
				logValue = val2.Value;
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val2);
				}
			}
			else if (type == typeof(float))
			{
				float val3 = msg.ReadSingle();
				logValue = val3.ToString("G", CultureInfo.InvariantCulture);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val3);
				}
			}
			else if (type == typeof(int))
			{
				int val4 = msg.ReadInt32();
				logValue = val4.ToString();
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val4);
				}
			}
			else if (type == typeof(bool))
			{
				bool val5 = msg.ReadBoolean();
				logValue = val5.ToString();
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val5);
				}
			}
			else if (type == typeof(Color))
			{
				Color val6 = new Color(msg.ReadByte(), msg.ReadByte(), msg.ReadByte(), msg.ReadByte());
				logValue = XMLExtensions.ColorToString(val6);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val6);
				}
			}
			else if (type == typeof(Vector2))
			{
				Vector2 val7 = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				logValue = XMLExtensions.Vector2ToString(val7);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val7);
				}
			}
			else if (type == typeof(Vector3))
			{
				Vector3 val8 = new Vector3(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
				logValue = XMLExtensions.Vector3ToString(val8, "G");
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val8);
				}
			}
			else if (type == typeof(Vector4))
			{
				Vector4 val9 = new Vector4(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
				logValue = XMLExtensions.Vector4ToString(val9, "G");
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val9);
				}
			}
			else if (type == typeof(Point))
			{
				Point val10 = new Point(msg.ReadInt32(), msg.ReadInt32());
				logValue = XMLExtensions.PointToString(val10);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val10);
				}
			}
			else if (type == typeof(Rectangle))
			{
				Rectangle val11 = new Rectangle(msg.ReadInt32(), msg.ReadInt32(), msg.ReadInt32(), msg.ReadInt32());
				logValue = XMLExtensions.RectToString(val11);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val11);
				}
			}
			else
			{
				if (!(type == typeof(string[])))
				{
					if (typeof(Enum).IsAssignableFrom(type))
					{
						int intVal = msg.ReadInt32();
						try
						{
							if (allowEditing)
							{
								property.TrySetValue(parentObject, Enum.ToObject(type, intVal));
								logValue = property.GetValue(parentObject).ToString();
							}
							goto IL_787;
						}
						catch (Exception e)
						{
							string str = "Item.ReadPropertyChange:";
							string name = this.Name;
							string str2 = ":";
							Type type2 = type;
							string identifier = str + name + str2 + ((type2 != null) ? type2.ToString() : null);
							GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Warning;
							string[] array = new string[7];
							array[0] = "Failed to convert the int value \"";
							array[1] = intVal.ToString();
							array[2] = "\" to ";
							int num = 3;
							Type type3 = type;
							array[num] = ((type3 != null) ? type3.ToString() : null);
							array[4] = " (item ";
							array[5] = this.Name;
							array[6] = ")";
							GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, string.Concat(array));
							goto IL_787;
						}
					}
					return;
				}
				int arrayLength = msg.ReadInt32();
				string[] val12 = new string[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					val12[i] = msg.ReadString();
				}
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val12);
				}
			}
			IL_787:
			if (allowEditing)
			{
				if (this.logPropertyChangeCoroutine != null)
				{
					CoroutineManager.StopCoroutines(this.logPropertyChangeCoroutine);
				}
				this.logPropertyChangeCoroutine = CoroutineManager.Invoke(delegate
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(39, 4);
					defaultInterpolatedStringHandler2.AppendFormatted(GameServer.CharacterLogName(sender.Character));
					defaultInterpolatedStringHandler2.AppendLiteral(" set the value \"");
					defaultInterpolatedStringHandler2.AppendFormatted(property.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" of the item \"");
					defaultInterpolatedStringHandler2.AppendFormatted(this.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
					defaultInterpolatedStringHandler2.AppendFormatted(logValue);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
				}, 1f);
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				ISerializableEntity entity = parentObject as ISerializableEntity;
				if (entity != null)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, new Item.ChangePropertyEventData(property, entity));
				}
			}
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x0004D960 File Offset: 0x0004BB60
		private void UpdateNetPosition(float deltaTime)
		{
			if (this.parentInventory != null || this.body == null || !this.body.Enabled || base.Removed)
			{
				this.PositionUpdateInterval = float.PositiveInfinity;
				return;
			}
			this.PositionUpdateInterval += deltaTime;
			float maxInterval = 30f;
			float velSqr = this.body.LinearVelocity.LengthSquared();
			if (velSqr > 100f)
			{
				maxInterval = 0.1f;
			}
			else if (velSqr > 1f)
			{
				maxInterval = 0.25f;
			}
			else if (velSqr > 0.0025000002f)
			{
				maxInterval = 1f;
			}
			this.PositionUpdateInterval = Math.Min(this.PositionUpdateInterval, maxInterval);
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x0004DA06 File Offset: 0x0004BC06
		public static Item Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			return Item.Load(element, submarine, false, idRemap);
		}

		// Token: 0x06000850 RID: 2128 RVA: 0x0004DA14 File Offset: 0x0004BC14
		public static Item Load(ContentXElement element, Submarine submarine, bool createNetworkEvent, IdRemap idRemap)
		{
			string name = element.GetAttribute("name").Value;
			Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (string.IsNullOrWhiteSpace(name) && identifier.IsEmpty)
			{
				string errorMessage = "Failed to load an item (both name and identifier were null):\n" + element.ToString();
				DebugConsole.ThrowError(errorMessage, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.Load:NameAndIdentifierNull", GameAnalyticsManager.ErrorSeverity.Error, errorMessage);
				return null;
			}
			Identifier pendingSwap = element.GetAttributeIdentifier("pendingswap", Identifier.Empty);
			ItemPrefab appliedSwap = null;
			ItemPrefab oldPrefab = null;
			if (!pendingSwap.IsEmpty)
			{
				Level loaded = Level.Loaded;
				if (loaded == null || loaded.Type != LevelData.LevelType.Outpost)
				{
					oldPrefab = ItemPrefab.Find(name, identifier);
					appliedSwap = ItemPrefab.Find(string.Empty, pendingSwap);
					identifier = pendingSwap;
					pendingSwap = Identifier.Empty;
				}
			}
			ItemPrefab prefab = ItemPrefab.Find(name, identifier);
			if (prefab == null)
			{
				return null;
			}
			string key = "rect";
			Rectangle empty = Rectangle.Empty;
			Rectangle rect = element.GetAttributeRect(key, empty);
			Vector2 centerPos = new Vector2((float)(rect.X + rect.Width / 2), (float)(rect.Y - rect.Height / 2));
			if (appliedSwap != null)
			{
				rect.Width = (int)(prefab.Sprite.size.X * prefab.Scale);
				rect.Height = (int)(prefab.Sprite.size.Y * prefab.Scale);
			}
			else if (rect.Width == 0 && rect.Height == 0)
			{
				rect.Width = (int)(prefab.Size.X * prefab.Scale);
				rect.Height = (int)(prefab.Size.Y * prefab.Scale);
			}
			Item item = new Item(rect, prefab, submarine, false, idRemap.GetOffsetId(element))
			{
				Submarine = submarine,
				linkedToID = new List<ushort>(),
				PendingItemSwap = (pendingSwap.IsEmpty ? null : (MapEntityPrefab.Find(pendingSwap.Value, null, true) as ItemPrefab))
			};
			if (createNetworkEvent)
			{
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item));
			}
			foreach (XAttribute attribute in (((appliedSwap != null) ? appliedSwap.ConfigElement : null) ?? element).Attributes())
			{
				SerializableProperty property;
				if (item.SerializableProperties.TryGetValue(attribute.NameAsIdentifier(), out property))
				{
					bool shouldBeLoaded = false;
					foreach (Serialize propertyAttribute in property.Attributes.OfType<Serialize>())
					{
						if (propertyAttribute.IsSaveable == IsPropertySaveable.Yes)
						{
							shouldBeLoaded = true;
							break;
						}
					}
					if (shouldBeLoaded)
					{
						object prevValue = property.GetValue(item);
						property.TrySetValue(item, attribute.Value);
						if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer && property.Attributes.OfType<Editable>().Any<Editable>() && (submarine == null || !submarine.Loading) && !(property.Name == "Tags") && !(property.Name == "Condition") && !(property.Name == "Description"))
						{
							object value = property.GetValue(item);
							if (value != null && !value.Equals(prevValue))
							{
								GameMain.NetworkMember.CreateEntityEvent(item, new Item.ChangePropertyEventData(property, item));
							}
						}
					}
				}
			}
			item.OnInsertedEffectsAppliedOnPreviousRound = item.OnInsertedEffectsApplied;
			item.ParseLinks(element, idRemap);
			bool thisIsOverride = element.GetAttributeBool("isoverride", false);
			bool isItemSwap = appliedSwap != null;
			bool usePrefabValues = thisIsOverride != ItemPrefab.Prefabs.IsOverride(prefab) || isItemSwap;
			List<ItemComponent> unloadedComponents = new List<ItemComponent>(item.components);
			using (IEnumerator<ContentXElement> enumerator3 = element.Elements().GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					ContentXElement subElement = enumerator3.Current;
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "upgrade"))
					{
						if (!(a == "itemstats"))
						{
							ItemComponent component = unloadedComponents.Find((ItemComponent x) => x.Name == subElement.Name.ToString());
							if (component != null)
							{
								component.Load(subElement, usePrefabValues, idRemap, isItemSwap);
								unloadedComponents.Remove(component);
							}
						}
						else
						{
							item.StatManager.Load(subElement);
						}
					}
					else
					{
						Identifier upgradeIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						UpgradePrefab upgradePrefab = UpgradePrefab.Find(upgradeIdentifier);
						int level = subElement.GetAttributeInt("level", 1);
						if (upgradePrefab != null)
						{
							item.AddUpgrade(new Upgrade(item, upgradePrefab, level, isItemSwap ? null : subElement), false);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
							defaultInterpolatedStringHandler.AppendLiteral("An upgrade with identifier \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(upgradeIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" on ");
							defaultInterpolatedStringHandler.AppendFormatted(item.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" was not found. ");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "It's effect will not be applied and won't be saved after the round ends.", null);
						}
					}
				}
			}
			if (usePrefabValues && !isItemSwap)
			{
				item.Scale = prefab.ConfigElement.GetAttributeFloat(item.scale, new string[]
				{
					"scale",
					"Scale"
				});
			}
			item.Upgrades.ForEach(delegate(Upgrade upgrade)
			{
				upgrade.ApplyUpgrade();
			});
			Identifier[] availableSwapIds = element.GetAttributeIdentifierArray("availableswaps", Array.Empty<Identifier>(), true);
			foreach (Identifier swapId in availableSwapIds)
			{
				ItemPrefab swapPrefab = ItemPrefab.Find(string.Empty, swapId);
				if (swapPrefab != null)
				{
					item.AvailableSwaps.Add(swapPrefab);
				}
			}
			if (element.GetAttributeBool("markedfordeconstruction", false))
			{
				Item._deconstructItems.Add(item);
			}
			float prevRotation = item.Rotation;
			if (element.GetAttributeBool("flippedx", false))
			{
				item.FlipX(false, true);
			}
			if (element.GetAttributeBool("flippedy", false))
			{
				item.FlipY(false, true);
			}
			item.Rotation = prevRotation;
			if (appliedSwap != null)
			{
				item.SpriteDepth = element.GetAttributeFloat("spritedepth", item.SpriteDepth);
				Item item2 = item;
				string key2 = "spritecolor";
				Color color = item.SpriteColor;
				item2.SpriteColor = element.GetAttributeColor(key2, color);
				item.Rotation = element.GetAttributeFloat("rotation", item.Rotation);
				item.PurchasedNewSwap = element.GetAttributeBool("purchasednewswap", false);
				float scaleRelativeToPrefab = element.GetAttributeFloat(item.scale, new string[]
				{
					"scale",
					"Scale"
				}) / oldPrefab.Scale;
				item.Scale *= scaleRelativeToPrefab;
				if (oldPrefab.SwappableItem != null && prefab.SwappableItem != null)
				{
					Vector2 oldRelativeOrigin = (oldPrefab.SwappableItem.SwapOrigin - oldPrefab.Size / 2f) * element.GetAttributeFloat(item.scale, new string[]
					{
						"scale",
						"Scale"
					});
					oldRelativeOrigin.Y = -oldRelativeOrigin.Y;
					oldRelativeOrigin = MathUtils.RotatePoint(oldRelativeOrigin, -item.RotationRad);
					Vector2 oldOrigin = centerPos + oldRelativeOrigin;
					Vector2 relativeOrigin = (prefab.SwappableItem.SwapOrigin - prefab.Size / 2f) * item.Scale;
					relativeOrigin.Y = -relativeOrigin.Y;
					relativeOrigin = MathUtils.RotatePoint(relativeOrigin, -item.RotationRad);
					Vector2 origin = new Vector2((float)(rect.X + rect.Width / 2), (float)(rect.Y - rect.Height / 2)) + relativeOrigin;
					Item item3 = item;
					item3.rect.Location = item3.rect.Location - (origin - oldOrigin).ToPoint();
				}
				if (item.PurchasedNewSwap)
				{
					SwappableItem swappableItem = appliedSwap.SwappableItem;
					if (!string.IsNullOrEmpty((swappableItem != null) ? swappableItem.SpawnWithId : null))
					{
						ItemContainer container = item.GetComponent<ItemContainer>();
						if (container != null)
						{
							container.SpawnWithId = appliedSwap.SwappableItem.SpawnWithId;
						}
					}
				}
				item.PurchasedNewSwap = false;
			}
			Version savedVersion = (submarine != null) ? submarine.Info.GameVersion : null;
			XDocument document = element.Document;
			if (((document != null) ? document.Root : null) != null && element.Document.Root.Name.ToString().Equals("gamesession", StringComparison.OrdinalIgnoreCase))
			{
				savedVersion = new Version(element.Document.Root.GetAttributeString("version", "0.0.0.0"));
			}
			float prevCondition = item.condition;
			if (savedVersion != null)
			{
				SerializableProperty.UpgradeGameVersion(item, item.Prefab.ConfigElement, savedVersion);
			}
			if (element.GetAttribute("conditionpercentage") != null)
			{
				item.condition = element.GetAttributeFloat("conditionpercentage", 100f) / 100f * item.MaxCondition;
			}
			else
			{
				item.condition = element.GetAttributeFloat("condition", item.condition);
				if (item.condition > 0f)
				{
					bool wasFullCondition = prevCondition >= item.Prefab.Health;
					if (wasFullCondition)
					{
						item.condition = item.MaxCondition;
					}
					item.condition = MathHelper.Clamp(item.condition, 0f, item.MaxCondition);
				}
			}
			item.lastSentCondition = (item.prevCondition = item.condition);
			item.RecalculateConditionValues();
			item.SetActiveSprite();
			foreach (ItemComponent component2 in item.components)
			{
				if (component2.Parent != null && component2.InheritParentIsActive)
				{
					component2.IsActive = component2.Parent.IsActive;
				}
				component2.OnItemLoaded();
			}
			item.FullyInitialized = true;
			return item;
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x0004E4D8 File Offset: 0x0004C6D8
		private void ReplaceFromNetwork(ItemPrefab replacement, ushort newId)
		{
			this.Replace(replacement, Option.Some<ushort>(newId), false);
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x0004E4E8 File Offset: 0x0004C6E8
		public void ReplaceWithLinkedItems(ItemPrefab replacement)
		{
			Option.UnspecifiedNone none = Option.None;
			this.Replace(replacement, none, true);
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x0004E50C File Offset: 0x0004C70C
		private void Replace(ItemPrefab replacement, Option<ushort> newId, bool createEntityEvent)
		{
			Vector2 centerPos = this.Position;
			Item newItem = new Item(replacement, this.Position, base.Submarine, newId.Fallback(0), true)
			{
				SpriteDepth = base.SpriteDepth,
				SpriteColor = this.SpriteColor,
				Rotation = this.Rotation
			};
			if (base.FlippedX)
			{
				newItem.FlipX(false, false);
			}
			if (base.FlippedY)
			{
				newItem.FlipY(false, false);
			}
			float scaleRelativeToPrefab = this.Scale / this.Prefab.Scale;
			newItem.Scale *= scaleRelativeToPrefab;
			if (this.Prefab.SwappableItem != null && replacement.SwappableItem != null)
			{
				Vector2 oldRelativeOrigin = (this.Prefab.SwappableItem.SwapOrigin - this.Prefab.Size / 2f) * this.scale;
				oldRelativeOrigin.Y = -oldRelativeOrigin.Y;
				oldRelativeOrigin = MathUtils.RotatePoint(oldRelativeOrigin, -this.RotationRad);
				Vector2 oldOrigin = centerPos + oldRelativeOrigin;
				Vector2 relativeOrigin = (this.Prefab.SwappableItem.SwapOrigin - this.Prefab.Size / 2f) * this.Scale;
				relativeOrigin.Y = -relativeOrigin.Y;
				relativeOrigin = MathUtils.RotatePoint(relativeOrigin, -this.RotationRad);
				Vector2 origin = new Vector2((float)this.rect.X + (float)this.rect.Width / 2f, (float)this.rect.Y - (float)this.rect.Height / 2f) + relativeOrigin;
				Item item = newItem;
				item.rect.Location = item.rect.Location - (origin - oldOrigin).ToPoint();
			}
			SwappableItem swappableItem = this.Prefab.SwappableItem;
			if (!string.IsNullOrEmpty((swappableItem != null) ? swappableItem.SpawnWithId : null))
			{
				ItemContainer newContainer = newItem.GetComponent<ItemContainer>();
				if (newContainer != null)
				{
					newContainer.SpawnWithId = this.Prefab.SwappableItem.SpawnWithId;
				}
			}
			using (List<ItemComponent>.Enumerator enumerator = this.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemComponent originalComponent = enumerator.Current;
					List<ItemComponent> originalComponents = (from c in this.components
					where c.GetType() == originalComponent.GetType()
					select c).ToList<ItemComponent>();
					List<ItemComponent> newComponents = (from c in newItem.components
					where c.GetType() == originalComponent.GetType()
					select c).ToList<ItemComponent>();
					int originalIndex = originalComponents.IndexOf(originalComponent);
					if (originalIndex < newComponents.Count)
					{
						ItemComponent newComponent = newComponents[originalIndex];
						foreach (KeyValuePair<Identifier, SerializableProperty> originalProperty in originalComponent.SerializableProperties)
						{
							if (!originalProperty.Value.OverridePrefabValues)
							{
								Editable attribute = originalProperty.Value.GetAttribute<Editable>();
								if (attribute == null || !attribute.TransferToSwappedItem)
								{
									continue;
								}
							}
							newComponent.SerializableProperties[originalProperty.Key].TrySetValue(newComponent, originalProperty.Value.GetValue(originalComponent));
						}
					}
				}
			}
			foreach (MapEntity linked in this.linkedTo)
			{
				newItem.linkedTo.Add(linked);
				if (linked.linkedTo.Contains(this))
				{
					linked.linkedTo.Add(newItem);
				}
			}
			ConnectionPanel thisConnectionPanel = this.GetComponent<ConnectionPanel>();
			ConnectionPanel newConnectionPanel = newItem.GetComponent<ConnectionPanel>();
			if (thisConnectionPanel != null && newConnectionPanel != null)
			{
				foreach (Connection connection in thisConnectionPanel.Connections)
				{
					foreach (Wire wire in connection.Wires)
					{
						int wireConnectionIndex = wire.Connections.IndexOf(connection);
						wire.RemoveConnection(this);
						int thisConnectionIndex = connection.ConnectionPanel.Connections.IndexOf(connection);
						if (thisConnectionIndex < 0 || thisConnectionIndex >= newConnectionPanel.Connections.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(161, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to move a wire from the connection ");
							defaultInterpolatedStringHandler.AppendFormatted(connection.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" when swapping the item ");
							defaultInterpolatedStringHandler.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" with ");
							defaultInterpolatedStringHandler.AppendFormatted(newItem.Name);
							defaultInterpolatedStringHandler.AppendLiteral(". The new item probably does not have the same number of connections as the previous one.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							Connection newConnection = newConnectionPanel.Connections[thisConnectionIndex];
							wire.Connect(newConnection, wireConnectionIndex, false, false);
							newConnection.ConnectWire(wire);
						}
					}
				}
			}
			if (newId.IsNone() && replacement.SwappableItem != null)
			{
				Dictionary<Item, ItemPrefab> connectedItemsToSwap = newItem.GetConnectedItemsToSwap(replacement.SwappableItem);
				foreach (KeyValuePair<Item, ItemPrefab> kvp in connectedItemsToSwap)
				{
					Item itemToSwap = kvp.Key;
					ItemPrefab swapTo = kvp.Value;
					Item item2 = itemToSwap;
					ItemPrefab replacement2 = swapTo;
					Option.UnspecifiedNone none = Option.None;
					item2.Replace(replacement2, none, createEntityEvent);
				}
			}
			if (createEntityEvent)
			{
				GameServer server = GameMain.Server;
				if (server != null)
				{
					server.CreateEntityEvent(this, new Item.SwapItemEventData(newItem.Prefab, newItem.ID));
				}
			}
			this.Remove();
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x0004EB5C File Offset: 0x0004CD5C
		public Dictionary<Item, ItemPrefab> GetConnectedItemsToSwap(SwappableItem swappingTo)
		{
			Dictionary<Item, ItemPrefab> itemsToSwap = new Dictionary<Item, ItemPrefab>();
			foreach (ValueTuple<Identifier, Identifier> valueTuple in swappingTo.ConnectedItemsToSwap)
			{
				Identifier requiredTag = valueTuple.Item1;
				Identifier swapTo = valueTuple.Item2;
				ItemPrefab replacement = MapEntityPrefab.FindByIdentifier(swapTo) as ItemPrefab;
				if (replacement != null)
				{
					foreach (MapEntity linked in this.linkedTo)
					{
						Item linkedItem = linked as Item;
						if (linkedItem != null && linkedItem.HasTag(requiredTag))
						{
							itemsToSwap.Add(linkedItem, replacement);
						}
					}
					ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
					if (connectionPanel != null)
					{
						foreach (Connection c in connectionPanel.Connections)
						{
							foreach (ItemComponent connectedComponent in this.GetConnectedComponentsRecursive<ItemComponent>(c, false, true))
							{
								if (!itemsToSwap.ContainsKey(connectedComponent.Item) && connectedComponent.Item.HasTag(requiredTag))
								{
									itemsToSwap.Add(connectedComponent.Item, replacement);
								}
							}
						}
					}
				}
			}
			return itemsToSwap;
		}

		// Token: 0x06000855 RID: 2133 RVA: 0x0004ED20 File Offset: 0x0004CF20
		public override XElement Save(XElement parentElement)
		{
			XElement element = new XElement("Item");
			element.Add(new object[]
			{
				new XAttribute("name", this.Prefab.OriginalName),
				new XAttribute("identifier", this.Prefab.Identifier),
				new XAttribute("ID", this.ID),
				new XAttribute("markedfordeconstruction", Item._deconstructItems.Contains(this))
			});
			if (this.PendingItemSwap != null)
			{
				element.Add(new XAttribute("pendingswap", this.PendingItemSwap.Identifier));
			}
			if (this.Rotation != 0f)
			{
				element.Add(new XAttribute("rotation", this.Rotation));
			}
			if (ItemPrefab.Prefabs.IsOverride(this.Prefab))
			{
				element.Add(new XAttribute("isoverride", "true"));
			}
			if (base.FlippedX)
			{
				element.Add(new XAttribute("flippedx", true));
			}
			if (base.FlippedY)
			{
				element.Add(new XAttribute("flippedy", true));
			}
			if (this.AvailableSwaps.Any<ItemPrefab>())
			{
				element.Add(new XAttribute("availableswaps", string.Join<Identifier>(',', from s in this.AvailableSwaps
				select s.Identifier)));
			}
			if (!MathUtils.NearlyEqual(this.healthMultiplier, 1f, 0.0001f))
			{
				element.Add(new XAttribute("healthmultiplier", this.HealthMultiplier.ToString("G", CultureInfo.InvariantCulture)));
			}
			Item item = this.RootContainer ?? this;
			Vector2 subPosition = (base.Submarine == null) ? Vector2.Zero : base.Submarine.HiddenSubPosition;
			int width = base.ResizeHorizontal ? this.rect.Width : this.defaultRect.Width;
			int height = base.ResizeVertical ? this.rect.Height : this.defaultRect.Height;
			element.Add(new XAttribute("rect", string.Concat(new string[]
			{
				((int)((float)this.rect.X - subPosition.X)).ToString(),
				",",
				((int)((float)this.rect.Y - subPosition.Y)).ToString(),
				",",
				width.ToString(),
				",",
				height.ToString()
			})));
			if (this.linkedTo != null && this.linkedTo.Count > 0)
			{
				bool isOutpost = base.Submarine != null && base.Submarine.Info.IsOutpost;
				IEnumerable<MapEntity> saveableLinked = from l in this.linkedTo
				where l.ShouldBeSaved && l.Removed == this.Removed && (l.Submarine == null || l.Submarine.Info.IsOutpost == isOutpost)
				select l;
				element.Add(new XAttribute("linked", string.Join(",", from l in saveableLinked
				select l.ID.ToString())));
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
			foreach (ItemComponent ic in this.components)
			{
				ic.Save(element);
			}
			foreach (Upgrade upgrade in this.Upgrades)
			{
				upgrade.Save(element);
			}
			ItemStatManager itemStatManager = this.statManager;
			if (itemStatManager != null)
			{
				itemStatManager.Save(element);
			}
			element.Add(new XAttribute("conditionpercentage", this.ConditionPercentage.ToString("G", CultureInfo.InvariantCulture)));
			XAttribute conditionAttribute = element.GetAttribute("condition", StringComparison.OrdinalIgnoreCase);
			if (conditionAttribute != null)
			{
				conditionAttribute.Remove();
			}
			parentElement.Add(element);
			return element;
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x0004F1C0 File Offset: 0x0004D3C0
		public virtual void Reset()
		{
			Holdable holdable = this.GetComponent<Holdable>();
			bool wasAttached = holdable != null && holdable.Attached;
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.Prefab.ConfigElement);
			this.Sprite.ReloadXML();
			base.SpriteDepth = this.Sprite.Depth;
			this.condition = this.MaxCondition;
			this.components.ForEach(delegate(ItemComponent c)
			{
				c.Reset();
			});
			if (wasAttached)
			{
				holdable.AttachToWall();
			}
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x0004F258 File Offset: 0x0004D458
		public override void OnMapLoaded()
		{
			this.FindHull();
			foreach (ItemComponent ic in this.components)
			{
				ic.OnMapLoaded();
			}
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x0004F2B4 File Offset: 0x0004D4B4
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			foreach (ItemComponent ic in this.components)
			{
				ic.ShallowRemove();
			}
			this.RemoveFromLists();
			if (this.body != null)
			{
				this.body.Remove();
				this.body = null;
			}
		}

		// Token: 0x06000859 RID: 2137 RVA: 0x0004F32C File Offset: 0x0004D52C
		public override void Remove()
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Attempting to remove an already removed item (" + this.Name + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			DebugConsole.Log(string.Concat(new string[]
			{
				"Removing item ",
				this.Name,
				" (ID: ",
				this.ID.ToString(),
				")"
			}));
			base.Remove();
			foreach (Character character in Character.CharacterList)
			{
				if (character.SelectedItem == this)
				{
					character.SelectedItem = null;
				}
				if (character.SelectedSecondaryItem == this)
				{
					character.SelectedSecondaryItem = null;
				}
			}
			Door door = this.GetComponent<Door>();
			Ladder ladder = this.GetComponent<Ladder>();
			if (door != null || ladder != null)
			{
				foreach (WayPoint wp in WayPoint.WayPointList)
				{
					if (door != null && wp.ConnectedDoor == door)
					{
						wp.ConnectedGap = null;
					}
					if (ladder != null && wp.Ladders == ladder)
					{
						wp.Ladders = null;
					}
				}
			}
			Dictionary<string, Connection> dictionary = this.connections;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			if (this.parentInventory != null)
			{
				CharacterInventory characterInventory = this.parentInventory as CharacterInventory;
				if (characterInventory != null)
				{
					characterInventory.RemoveItem(this, true);
				}
				else
				{
					this.parentInventory.RemoveItem(this);
				}
				this.parentInventory = null;
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Remove();
			}
			this.RemoveFromLists();
			if (this.body != null)
			{
				this.body.Remove();
				this.body = null;
			}
			this.CurrentHull = null;
			if (this.StaticFixtures != null)
			{
				foreach (Fixture fixture in this.StaticFixtures)
				{
					Body body = fixture.Body;
					if (((body != null) ? body.World : null) != null)
					{
						fixture.Body.Remove(fixture);
					}
				}
				this.StaticFixtures.Clear();
			}
			foreach (Item it in Item.ItemList)
			{
				if (it.linkedTo.Contains(this))
				{
					it.linkedTo.Remove(this);
				}
			}
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x0004F608 File Offset: 0x0004D808
		private void RemoveFromLists()
		{
			Item.ItemList.Remove(this);
			Item._dangerousItems.Remove(this);
			Item._repairableItems.Remove(this);
			Item._sonarVisibleItems.Remove(this);
			Item._cleanableItems.Remove(this);
			Item._deconstructItems.Remove(this);
			Item._turretTargetItems.Remove(this);
			Item._chairItems.Remove(this);
			this.RemoveFromDroppedStack(true);
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x0004F67C File Offset: 0x0004D87C
		public static void RemoveByPrefab(ItemPrefab prefab)
		{
			if (Item.ItemList == null)
			{
				return;
			}
			List<Item> list = new List<Item>(Item.ItemList);
			foreach (Item item in list)
			{
				if (item.Prefab == prefab)
				{
					item.Remove();
				}
			}
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x0004F86C File Offset: 0x0004DA6C
		[CompilerGenerated]
		private Exception <ServerEventWrite>g__error|4_0(string reason)
		{
			string errorMsg = "Failed to write a network event for the item \"" + this.Name + "\" - " + reason;
			GameAnalyticsManager.AddErrorEventOnce("Item.ServerWrite:" + this.Name, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			return new Exception(errorMsg);
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x0004F90C File Offset: 0x0004DB0C
		[CompilerGenerated]
		internal static bool <ConditionalMatches>g__MatchesComponent|460_0(ItemComponent comp, PropertyConditional cond)
		{
			return comp.Name == cond.TargetItemComponent;
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0004F91F File Offset: 0x0004DB1F
		[CompilerGenerated]
		private void <SetCondition>g__SetPreviousCondition|466_0(ref Item.<>c__DisplayClass466_0 A_1)
		{
			this.LastConditionChange = this.condition - this.prevCondition;
			this.ConditionLastUpdated = Timing.TotalTime;
			this.prevCondition = this.condition;
			A_1.wasPreviousConditionChanged = true;
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x0004F954 File Offset: 0x0004DB54
		[CompilerGenerated]
		internal static void <SetCondition>g__flagChangedConnections|466_1(Dictionary<string, Connection> connections)
		{
			if (connections == null)
			{
				return;
			}
			foreach (Connection c in connections.Values)
			{
				if (c.IsPower)
				{
					Powered.ChangedConnections.Add(c);
					foreach (Connection conn in c.Recipients)
					{
						Powered.ChangedConnections.Add(conn);
					}
				}
			}
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x0004FA24 File Offset: 0x0004DC24
		[CompilerGenerated]
		internal static IEnumerable<Connection> <GetConnectedComponentsRecursive>g__GetRecipients|489_0<T>(Connection c) where T : ItemComponent
		{
			Item.<<GetConnectedComponentsRecursive>g__GetRecipients|489_0>d<T> <<GetConnectedComponentsRecursive>g__GetRecipients|489_0>d = new Item.<<GetConnectedComponentsRecursive>g__GetRecipients|489_0>d<T>(-2);
			<<GetConnectedComponentsRecursive>g__GetRecipients|489_0>d.<>3__c = c;
			return <<GetConnectedComponentsRecursive>g__GetRecipients|489_0>d;
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x0004FA44 File Offset: 0x0004DC44
		[CompilerGenerated]
		private void <GetConnectedComponentsRecursive>g__CheckRecipient|489_1<T>(Connection recipient, ref Item.<>c__DisplayClass489_0<T> A_2) where T : ItemComponent
		{
			WifiComponent wifiComponent = recipient.Item.GetComponent<WifiComponent>();
			if (wifiComponent != null && wifiComponent.CanTransmit(false))
			{
				foreach (WifiComponent wifiReceiver in wifiComponent.GetTransmittersInRange())
				{
					List<Connection> receiverConnections = wifiReceiver.Item.Connections;
					if (receiverConnections != null)
					{
						foreach (Connection wifiOutput in receiverConnections)
						{
							if (wifiOutput.IsOutput != recipient.IsOutput && !A_2.alreadySearched.Contains(wifiOutput))
							{
								this.GetConnectedComponentsRecursive<T>(wifiOutput, A_2.alreadySearched, A_2.connectedComponents, A_2.ignoreInactiveRelays, A_2.allowTraversingBackwards);
							}
						}
					}
				}
			}
			recipient.Item.GetConnectedComponentsRecursive<T>(recipient, A_2.alreadySearched, A_2.connectedComponents, A_2.ignoreInactiveRelays, A_2.allowTraversingBackwards);
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0004FB58 File Offset: 0x0004DD58
		[CompilerGenerated]
		private void <GetConnectedComponentsRecursive>g__searchFromAToB|489_2<T>(Identifier connectionEndA, Identifier connectionEndB, ref Item.<>c__DisplayClass489_0<T> A_3) where T : ItemComponent
		{
			if (connectionEndA == A_3.c.Name)
			{
				Connection pairedConnection = A_3.c.Item.Connections.FirstOrDefault((Connection c2) => c2.Name == connectionEndB);
				if (pairedConnection != null)
				{
					if (A_3.alreadySearched.Contains(pairedConnection))
					{
						return;
					}
					this.GetConnectedComponentsRecursive<T>(pairedConnection, A_3.alreadySearched, A_3.connectedComponents, A_3.ignoreInactiveRelays, A_3.allowTraversingBackwards);
				}
			}
		}

		// Token: 0x0400035B RID: 859
		private CoroutineHandle logPropertyChangeCoroutine;

		// Token: 0x0400035C RID: 860
		private readonly Dictionary<Client, CampaignMode.InteractionType> campaignInteractionTypePerClient = new Dictionary<Client, CampaignMode.InteractionType>();

		// Token: 0x0400035D RID: 861
		public static readonly List<Item> ItemList = new List<Item>();

		// Token: 0x0400035E RID: 862
		private static readonly HashSet<Item> _dangerousItems = new HashSet<Item>();

		// Token: 0x0400035F RID: 863
		private static readonly List<Item> _repairableItems = new List<Item>();

		// Token: 0x04000360 RID: 864
		private static readonly List<Item> _cleanableItems = new List<Item>();

		// Token: 0x04000361 RID: 865
		private static readonly HashSet<Item> _deconstructItems = new HashSet<Item>();

		// Token: 0x04000362 RID: 866
		private static readonly List<Item> _sonarVisibleItems = new List<Item>();

		// Token: 0x04000363 RID: 867
		private static readonly List<Item> _turretTargetItems = new List<Item>();

		// Token: 0x04000364 RID: 868
		private static readonly List<Item> _chairItems = new List<Item>();

		// Token: 0x04000365 RID: 869
		public static bool ShowLinks = true;

		// Token: 0x04000366 RID: 870
		private HashSet<Identifier> tags;

		// Token: 0x04000367 RID: 871
		private readonly bool isWire;

		// Token: 0x04000368 RID: 872
		private readonly bool isLogic;

		// Token: 0x04000369 RID: 873
		private Hull currentHull;

		// Token: 0x0400036A RID: 874
		private CampaignMode.InteractionType campaignInteractionType;

		// Token: 0x0400036B RID: 875
		public bool Visible = true;

		// Token: 0x0400036C RID: 876
		private readonly Dictionary<Type, List<ItemComponent>> componentsByType = new Dictionary<Type, List<ItemComponent>>();

		// Token: 0x0400036D RID: 877
		private readonly List<ItemComponent> components;

		// Token: 0x0400036E RID: 878
		private readonly List<ItemComponent> updateableComponents = new List<ItemComponent>();

		// Token: 0x0400036F RID: 879
		private readonly List<IDrawableComponent> drawableComponents;

		// Token: 0x04000370 RID: 880
		private bool hasComponentsToDraw;

		// Token: 0x04000372 RID: 882
		public PhysicsBody body;

		// Token: 0x04000373 RID: 883
		private readonly float originalWaterDragCoefficient;

		// Token: 0x04000374 RID: 884
		private float? overrideWaterDragCoefficient;

		// Token: 0x04000375 RID: 885
		public readonly XElement StaticBodyConfig;

		// Token: 0x04000376 RID: 886
		public List<Fixture> StaticFixtures = new List<Fixture>();

		// Token: 0x04000377 RID: 887
		private bool transformDirty = true;

		// Token: 0x04000378 RID: 888
		private static readonly List<Item> itemsWithPendingConditionUpdates = new List<Item>();

		// Token: 0x04000379 RID: 889
		private float lastSentCondition;

		// Token: 0x0400037A RID: 890
		private float sendConditionUpdateTimer;

		// Token: 0x0400037B RID: 891
		private float prevCondition;

		// Token: 0x0400037C RID: 892
		private float condition;

		// Token: 0x0400037D RID: 893
		private bool inWater;

		// Token: 0x0400037E RID: 894
		private readonly bool hasInWaterStatusEffects;

		// Token: 0x0400037F RID: 895
		private readonly bool hasNotInWaterStatusEffects;

		// Token: 0x04000380 RID: 896
		private Inventory parentInventory;

		// Token: 0x04000381 RID: 897
		private readonly ItemInventory ownInventory;

		// Token: 0x04000382 RID: 898
		private Rectangle defaultRect;

		// Token: 0x04000383 RID: 899
		private readonly Dictionary<string, Connection> connections;

		// Token: 0x04000384 RID: 900
		private readonly List<Repairable> repairables;

		// Token: 0x04000385 RID: 901
		private readonly Quality qualityComponent;

		// Token: 0x04000386 RID: 902
		private ConcurrentQueue<float> impactQueue;

		// Token: 0x04000387 RID: 903
		private readonly bool[] hasStatusEffectsOfType = new bool[Enum.GetValues(typeof(ActionType)).Length];

		// Token: 0x04000388 RID: 904
		private readonly Dictionary<ActionType, List<StatusEffect>> statusEffectLists;

		// Token: 0x04000389 RID: 905
		private readonly float conditionMultiplierCampaign = 1f;

		// Token: 0x0400038A RID: 906
		public Action OnInteract;

		// Token: 0x0400038C RID: 908
		private bool? hasInGameEditableProperties;

		// Token: 0x0400038E RID: 910
		public Character Equipper;

		// Token: 0x04000390 RID: 912
		private Item rootContainer;

		// Token: 0x04000391 RID: 913
		private bool inWaterProofContainer;

		// Token: 0x04000392 RID: 914
		private Item container;

		// Token: 0x04000393 RID: 915
		private string description;

		// Token: 0x04000394 RID: 916
		private string descriptionTag;

		// Token: 0x04000399 RID: 921
		private float impactTolerance;

		// Token: 0x0400039C RID: 924
		public const float SubmarineImpactCooldown = 0.1f;

		// Token: 0x0400039D RID: 925
		public double LastSubmarineImpactTime;

		// Token: 0x0400039E RID: 926
		private float scale = 1f;

		// Token: 0x040003A1 RID: 929
		protected Color spriteColor;

		// Token: 0x040003A4 RID: 932
		public Color? HighlightColor;

		// Token: 0x040003AA RID: 938
		public bool OnInsertedEffectsAppliedOnPreviousRound;

		// Token: 0x040003AE RID: 942
		private float offsetOnSelectedMultiplier = 1f;

		// Token: 0x040003AF RID: 943
		private float healthMultiplier = 1f;

		// Token: 0x040003B0 RID: 944
		private float maxRepairConditionMultiplier = 1f;

		// Token: 0x040003B4 RID: 948
		private bool? indestructible;

		// Token: 0x040003B6 RID: 950
		private bool? isDangerous;

		// Token: 0x040003B8 RID: 952
		public bool UnequipAutomatically = true;

		// Token: 0x040003B9 RID: 953
		public bool StolenDuringRound;

		// Token: 0x040003BA RID: 954
		private bool spawnedInCurrentOutpost;

		// Token: 0x040003BB RID: 955
		private bool allowStealing;

		// Token: 0x040003BC RID: 956
		public bool IsSalvageMissionItem;

		// Token: 0x040003BD RID: 957
		private string originalOutpost;

		// Token: 0x040003BF RID: 959
		private bool waterProof;

		// Token: 0x040003C1 RID: 961
		private readonly HashSet<InvSlotType> allowedSlots = new HashSet<InvSlotType>();

		// Token: 0x040003C2 RID: 962
		public readonly ImmutableArray<ItemInventory> OwnInventories = ImmutableArray<ItemInventory>.Empty;

		// Token: 0x040003C6 RID: 966
		public readonly HashSet<ItemPrefab> AvailableSwaps = new HashSet<ItemPrefab>();

		// Token: 0x040003C7 RID: 967
		private readonly List<ISerializableEntity> allPropertyObjects = new List<ISerializableEntity>();

		// Token: 0x040003CB RID: 971
		private ItemStatManager statManager;

		// Token: 0x040003CD RID: 973
		public Action<Character> OnDeselect;

		// Token: 0x040003CE RID: 974
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x040003CF RID: 975
		public bool IsActive = true;

		// Token: 0x040003D0 RID: 976
		public bool IsInRemoveQueue;

		// Token: 0x040003D1 RID: 977
		[TupleElementNames(new string[]
		{
			"Input",
			"Output"
		})]
		public static readonly ImmutableArray<ValueTuple<Identifier, Identifier>> connectionPairs = new ValueTuple<Identifier, Identifier>[]
		{
			new ValueTuple<Identifier, Identifier>("power_in".ToIdentifier(), "power_out".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in1".ToIdentifier(), "signal_out1".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in2".ToIdentifier(), "signal_out2".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in3".ToIdentifier(), "signal_out3".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in4".ToIdentifier(), "signal_out4".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in".ToIdentifier(), "signal_out".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in1".ToIdentifier(), "signal_out".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in2".ToIdentifier(), "signal_out".ToIdentifier())
		}.ToImmutableArray<ValueTuple<Identifier, Identifier>>();

		// Token: 0x040003D2 RID: 978
		[TupleElementNames(new string[]
		{
			"Signal",
			"Connection"
		})]
		private readonly HashSet<ValueTuple<Signal, Connection>> delayedSignals = new HashSet<ValueTuple<Signal, Connection>>();

		// Token: 0x040003D3 RID: 979
		private List<Item> droppedStack;

		// Token: 0x040003D4 RID: 980
		private bool isDroppedStackOwner;

		// Token: 0x02000698 RID: 1688
		private readonly struct DroppedStackEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F2 RID: 5106
			// (get) Token: 0x06004F04 RID: 20228 RVA: 0x001E38C7 File Offset: 0x001E1AC7
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.DroppedStack;
				}
			}

			// Token: 0x06004F05 RID: 20229 RVA: 0x001E38CB File Offset: 0x001E1ACB
			[NullableContext(1)]
			public DroppedStackEventData(IEnumerable<Item> items)
			{
				this.Items = items.Distinct<Item>().ToImmutableArray<Item>();
			}

			// Token: 0x04002A08 RID: 10760
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public readonly ImmutableArray<Item> Items;
		}

		// Token: 0x02000699 RID: 1689
		public readonly struct SetHighlightEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F3 RID: 5107
			// (get) Token: 0x06004F06 RID: 20230 RVA: 0x001E38DE File Offset: 0x001E1ADE
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.SetHighlight;
				}
			}

			// Token: 0x06004F07 RID: 20231 RVA: 0x001E38E2 File Offset: 0x001E1AE2
			public SetHighlightEventData(bool highlighted, Color color, [Nullable(new byte[]
			{
				2,
				1
			})] IEnumerable<Client> targetClients)
			{
				this.Highlighted = highlighted;
				this.Color = color;
				this.TargetClients = (targetClients ?? Enumerable.Empty<Client>()).ToImmutableArray<Client>();
			}

			// Token: 0x04002A09 RID: 10761
			public readonly bool Highlighted;

			// Token: 0x04002A0A RID: 10762
			public readonly Color Color;

			// Token: 0x04002A0B RID: 10763
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public readonly ImmutableArray<Client> TargetClients;
		}

		// Token: 0x0200069A RID: 1690
		public enum EventType
		{
			// Token: 0x04002A0D RID: 10765
			ComponentState,
			// Token: 0x04002A0E RID: 10766
			InventoryState,
			// Token: 0x04002A0F RID: 10767
			Treatment,
			// Token: 0x04002A10 RID: 10768
			ChangeProperty,
			// Token: 0x04002A11 RID: 10769
			Combine,
			// Token: 0x04002A12 RID: 10770
			Status,
			// Token: 0x04002A13 RID: 10771
			AssignCampaignInteraction,
			// Token: 0x04002A14 RID: 10772
			ApplyStatusEffect,
			// Token: 0x04002A15 RID: 10773
			Upgrade,
			// Token: 0x04002A16 RID: 10774
			ItemStat,
			// Token: 0x04002A17 RID: 10775
			DroppedStack,
			// Token: 0x04002A18 RID: 10776
			SetHighlight,
			// Token: 0x04002A19 RID: 10777
			SwapItem,
			// Token: 0x04002A1A RID: 10778
			MinValue = 0,
			// Token: 0x04002A1B RID: 10779
			MaxValue = 12
		}

		// Token: 0x0200069B RID: 1691
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x170013F4 RID: 5108
			// (get) Token: 0x06004F08 RID: 20232
			Item.EventType EventType { get; }
		}

		// Token: 0x0200069C RID: 1692
		public readonly struct ComponentStateEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F5 RID: 5109
			// (get) Token: 0x06004F09 RID: 20233 RVA: 0x001E3907 File Offset: 0x001E1B07
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ComponentState;
				}
			}

			// Token: 0x06004F0A RID: 20234 RVA: 0x001E390A File Offset: 0x001E1B0A
			public ComponentStateEventData(ItemComponent component, ItemComponent.IEventData componentData)
			{
				this.Component = component;
				this.ComponentData = componentData;
			}

			// Token: 0x04002A1C RID: 10780
			public readonly ItemComponent Component;

			// Token: 0x04002A1D RID: 10781
			public readonly ItemComponent.IEventData ComponentData;
		}

		// Token: 0x0200069D RID: 1693
		public readonly struct InventoryStateEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F6 RID: 5110
			// (get) Token: 0x06004F0B RID: 20235 RVA: 0x001E391A File Offset: 0x001E1B1A
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.InventoryState;
				}
			}

			// Token: 0x06004F0C RID: 20236 RVA: 0x001E391D File Offset: 0x001E1B1D
			public InventoryStateEventData(ItemContainer component, Range slotRange)
			{
				this.Component = component;
				this.SlotRange = slotRange;
			}

			// Token: 0x04002A1E RID: 10782
			public readonly ItemContainer Component;

			// Token: 0x04002A1F RID: 10783
			public readonly Range SlotRange;
		}

		// Token: 0x0200069E RID: 1694
		public readonly struct ChangePropertyEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F7 RID: 5111
			// (get) Token: 0x06004F0D RID: 20237 RVA: 0x001E392D File Offset: 0x001E1B2D
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ChangeProperty;
				}
			}

			// Token: 0x06004F0E RID: 20238 RVA: 0x001E3930 File Offset: 0x001E1B30
			public ChangePropertyEventData(SerializableProperty serializableProperty, ISerializableEntity entity)
			{
				if (serializableProperty.GetAttribute<Editable>() == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Attempted to create ");
					defaultInterpolatedStringHandler.AppendFormatted("ChangePropertyEventData");
					defaultInterpolatedStringHandler.AppendLiteral(" for the non-editable property ");
					defaultInterpolatedStringHandler.AppendFormatted(serializableProperty.Name);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				this.SerializableProperty = serializableProperty;
				this.Entity = entity;
			}

			// Token: 0x04002A20 RID: 10784
			public readonly SerializableProperty SerializableProperty;

			// Token: 0x04002A21 RID: 10785
			public readonly ISerializableEntity Entity;
		}

		// Token: 0x0200069F RID: 1695
		public readonly struct SetItemStatEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F8 RID: 5112
			// (get) Token: 0x06004F0F RID: 20239 RVA: 0x001E39AA File Offset: 0x001E1BAA
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ItemStat;
				}
			}

			// Token: 0x06004F10 RID: 20240 RVA: 0x001E39AE File Offset: 0x001E1BAE
			public SetItemStatEventData(Dictionary<TalentStatIdentifier, float> stats)
			{
				this.Stats = stats;
			}

			// Token: 0x04002A22 RID: 10786
			public readonly Dictionary<TalentStatIdentifier, float> Stats;
		}

		// Token: 0x020006A0 RID: 1696
		private readonly struct ItemStatusEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013F9 RID: 5113
			// (get) Token: 0x06004F11 RID: 20241 RVA: 0x001E39B7 File Offset: 0x001E1BB7
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.Status;
				}
			}

			// Token: 0x06004F12 RID: 20242 RVA: 0x001E39BA File Offset: 0x001E1BBA
			public ItemStatusEventData(bool loadingRound)
			{
				this.LoadingRound = loadingRound;
			}

			// Token: 0x04002A23 RID: 10787
			public readonly bool LoadingRound;
		}

		// Token: 0x020006A1 RID: 1697
		private readonly struct AssignCampaignInteractionEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013FA RID: 5114
			// (get) Token: 0x06004F13 RID: 20243 RVA: 0x001E39C3 File Offset: 0x001E1BC3
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.AssignCampaignInteraction;
				}
			}

			// Token: 0x06004F14 RID: 20244 RVA: 0x001E39C6 File Offset: 0x001E1BC6
			public AssignCampaignInteractionEventData(IEnumerable<Client> targetClients)
			{
				this.TargetClients = (targetClients ?? Enumerable.Empty<Client>()).ToImmutableArray<Client>();
			}

			// Token: 0x04002A24 RID: 10788
			public readonly ImmutableArray<Client> TargetClients;
		}

		// Token: 0x020006A2 RID: 1698
		public readonly struct ApplyStatusEffectEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013FB RID: 5115
			// (get) Token: 0x06004F15 RID: 20245 RVA: 0x001E39DD File Offset: 0x001E1BDD
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ApplyStatusEffect;
				}
			}

			// Token: 0x06004F16 RID: 20246 RVA: 0x001E39E0 File Offset: 0x001E1BE0
			public ApplyStatusEffectEventData(ActionType actionType, ItemComponent targetItemComponent = null, Character targetCharacter = null, Limb targetLimb = null, Entity useTarget = null, Vector2? worldPosition = null)
			{
				this.ActionType = actionType;
				this.TargetItemComponent = targetItemComponent;
				this.TargetCharacter = targetCharacter;
				this.TargetLimb = targetLimb;
				this.UseTarget = useTarget;
				this.WorldPosition = worldPosition;
			}

			// Token: 0x04002A25 RID: 10789
			public readonly ActionType ActionType;

			// Token: 0x04002A26 RID: 10790
			public readonly ItemComponent TargetItemComponent;

			// Token: 0x04002A27 RID: 10791
			public readonly Character TargetCharacter;

			// Token: 0x04002A28 RID: 10792
			public readonly Limb TargetLimb;

			// Token: 0x04002A29 RID: 10793
			public readonly Entity UseTarget;

			// Token: 0x04002A2A RID: 10794
			public readonly Vector2? WorldPosition;
		}

		// Token: 0x020006A3 RID: 1699
		private readonly struct UpgradeEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013FC RID: 5116
			// (get) Token: 0x06004F17 RID: 20247 RVA: 0x001E3A0F File Offset: 0x001E1C0F
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.Upgrade;
				}
			}

			// Token: 0x06004F18 RID: 20248 RVA: 0x001E3A12 File Offset: 0x001E1C12
			public UpgradeEventData(Upgrade upgrade)
			{
				this.Upgrade = upgrade;
			}

			// Token: 0x04002A2B RID: 10795
			public readonly Upgrade Upgrade;
		}

		// Token: 0x020006A4 RID: 1700
		private readonly struct SwapItemEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013FD RID: 5117
			// (get) Token: 0x06004F19 RID: 20249 RVA: 0x001E3A1B File Offset: 0x001E1C1B
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.SwapItem;
				}
			}

			// Token: 0x06004F1A RID: 20250 RVA: 0x001E3A1F File Offset: 0x001E1C1F
			public SwapItemEventData(ItemPrefab newItem, ushort newId)
			{
				this.NewItem = newItem;
				this.NewId = newId;
			}

			// Token: 0x04002A2C RID: 10796
			public readonly ItemPrefab NewItem;

			// Token: 0x04002A2D RID: 10797
			public readonly ushort NewId;
		}
	}
}
