using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000012 RID: 18
	internal class Character : Entity, IDamageable, ISerializableEntity, IClientSerializable, INetSerializable, IServerPositionSync, IServerSerializable
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600006B RID: 107 RVA: 0x000044EF File Offset: 0x000026EF
		public static Character Controlled
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x000044F4 File Offset: 0x000026F4
		private void SyncInGameEditables(Item item)
		{
			foreach (ItemComponent itemComponent in item.Components)
			{
				foreach (SerializableProperty serializableProperty in SerializableProperty.GetProperties<InGameEditable>(itemComponent))
				{
					GameMain.Server.CreateEntityEvent(item, new Item.ChangePropertyEventData(serializableProperty, itemComponent));
				}
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00004594 File Offset: 0x00002794
		public void SetOwnerClient(Client client)
		{
			if (client == null)
			{
				this.ownerClientAddress = null;
				this.ownerClientAccountId = Option<AccountId>.None();
				this.IsRemotePlayer = false;
				return;
			}
			this.ownerClientAddress = client.Connection.Endpoint.Address;
			this.ownerClientAccountId = client.AccountId;
			this.IsRemotePlayer = true;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x000045E8 File Offset: 0x000027E8
		public bool IsClientOwner(Client client)
		{
			AccountId accountId;
			AccountId clientId;
			if (this.ownerClientAccountId.TryUnwrap(out accountId) && client.AccountId.TryUnwrap(out clientId))
			{
				return accountId == clientId;
			}
			return this.ownerClientAddress == client.Connection.Endpoint.Address;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000463C File Offset: 0x0000283C
		public float GetPositionUpdateInterval(Client recipient)
		{
			if (!this.Enabled)
			{
				return 1000f;
			}
			Vector2 comparePosition = recipient.SpectatePos ?? recipient.Character.WorldPosition;
			float distance = Vector2.Distance(comparePosition, this.WorldPosition);
			Character character = recipient.Character;
			if (((character != null) ? character.ViewTarget : null) != null)
			{
				distance = Math.Min(distance, Vector2.Distance(recipient.Character.ViewTarget.WorldPosition, this.WorldPosition));
			}
			if (this.ViewTarget != null && this.ViewTarget != this)
			{
				distance = Math.Min(distance, Vector2.Distance(comparePosition, this.ViewTarget.WorldPosition));
			}
			float priority = 1f - MathUtils.InverseLerp(NetConfig.HighPrioCharacterPositionUpdateDistance, NetConfig.LowPrioCharacterPositionUpdateDistance, distance);
			float interval = MathHelper.Lerp(NetConfig.LowPrioCharacterPositionUpdateInterval, NetConfig.HighPrioCharacterPositionUpdateInterval, priority);
			if (this.IsDead && !this.AnimController.IsDraggedWithRope)
			{
				interval = Math.Max(interval * 2f, 0.1f);
			}
			return interval;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x0000473C File Offset: 0x0000293C
		public void ServerReadInput(IReadMessage msg, Client c)
		{
			if (c.Character != this)
			{
				return;
			}
			ushort networkUpdateID = msg.ReadUInt16();
			byte inputCount = msg.ReadByte();
			this.Enabled = true;
			for (int i = 0; i < (int)inputCount; i++)
			{
				Character.InputNetFlags newInput = (Character.InputNetFlags)msg.ReadRangedInteger(0, 65535);
				ushort newInteract = 0;
				if (newInput != Character.InputNetFlags.None && newInput != Character.InputNetFlags.FacingLeft)
				{
					c.KickAFKTimer = 0f;
				}
				else if (this.AnimController.Dir < 0f != newInput.HasFlag(Character.InputNetFlags.FacingLeft))
				{
					c.KickAFKTimer = 0f;
				}
				ushort newAim = msg.ReadUInt16();
				if (newInput.HasFlag(Character.InputNetFlags.Select) || newInput.HasFlag(Character.InputNetFlags.Deselect) || newInput.HasFlag(Character.InputNetFlags.Use) || newInput.HasFlag(Character.InputNetFlags.Health) || newInput.HasFlag(Character.InputNetFlags.Grab))
				{
					newInteract = msg.ReadUInt16();
				}
				if (NetIdUtils.IdMoreRecent((ushort)((int)networkUpdateID - i), this.LastNetworkUpdateID) && i < 60)
				{
					if (i > 0 && this.memInput[i - 1].intAim != newAim)
					{
						c.KickAFKTimer = 0f;
					}
					Character.NetInputMem newMem = new Character.NetInputMem
					{
						states = newInput,
						intAim = newAim,
						interact = newInteract,
						networkUpdateID = (ushort)((int)networkUpdateID - i)
					};
					this.memInput.Insert(i, newMem);
					this.LastInputTime = Timing.TotalTime;
				}
			}
			if (NetIdUtils.IdMoreRecent(networkUpdateID, this.LastNetworkUpdateID))
			{
				this.LastNetworkUpdateID = networkUpdateID;
			}
			else if (NetIdUtils.Difference(networkUpdateID, this.LastNetworkUpdateID) > 500)
			{
				this.LastNetworkUpdateID = (this.LastProcessedID = networkUpdateID);
			}
			if (this.memInput.Count > 60)
			{
				this.memInput.RemoveRange(30, this.memInput.Count - 30);
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004944 File Offset: 0x00002B44
		public virtual void ServerEventRead(IReadMessage msg, Client c)
		{
			Character.EventType eventType = (Character.EventType)msg.ReadRangedInteger(0, 18);
			switch (eventType)
			{
			case Character.EventType.InventoryState:
				this.Inventory.ServerEventRead(msg, c);
				return;
			case Character.EventType.Control:
				break;
			case Character.EventType.Status:
				if (c.Character != this)
				{
					return;
				}
				if (this.IsIncapacitated)
				{
					ValueTuple<CauseOfDeathType, Affliction> causeOfDeath = this.CharacterHealth.GetCauseOfDeath();
					this.Kill(causeOfDeath.Item1, causeOfDeath.Item2, false, true);
					return;
				}
				break;
			case Character.EventType.Treatment:
			{
				bool doingCPR = msg.ReadBoolean();
				if (c.Character != this)
				{
					return;
				}
				this.AnimController.Anim = (doingCPR ? AnimController.Animation.CPR : AnimController.Animation.None);
				return;
			}
			default:
				if (eventType != Character.EventType.UpdateTalents)
				{
					if (eventType != Character.EventType.ConfirmTalentRefund)
					{
						return;
					}
					if (!this.<ServerEventRead>g__CanManageTalents|14_0(c))
					{
						return;
					}
					CharacterInfo characterInfo = this.Info;
					if (characterInfo == null)
					{
						return;
					}
					characterInfo.RefundTalents();
					return;
				}
				else
				{
					if (!this.<ServerEventRead>g__CanManageTalents|14_0(c))
					{
						return;
					}
					ushort talentCount = msg.ReadUInt16();
					List<Identifier> talentSelection = new List<Identifier>();
					for (int i = 0; i < (int)talentCount; i++)
					{
						uint talentIdentifier = msg.ReadUInt32();
						TalentPrefab prefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab p) => p.UintIdentifier == talentIdentifier);
						if (prefab != null && TalentTree.IsViableTalentForCharacter(this, prefab.Identifier, talentSelection))
						{
							this.GiveTalent(prefab.Identifier, true);
							talentSelection.Add(prefab.Identifier);
						}
					}
					if (talentSelection.Count != (int)talentCount)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to unlock talents: the amount of unlocked talents doesn't match (client: ");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(talentCount);
						defaultInterpolatedStringHandler.AppendLiteral(", server: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(talentSelection.Count);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
				}
				break;
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00004AE4 File Offset: 0x00002CE4
		public void ServerWritePosition(ReadWriteMessage tempBuffer, Client c)
		{
			if (this == c.Character)
			{
				tempBuffer.WriteBoolean(true);
				if ((int)this.LastNetworkUpdateID < this.memInput.Count + 1)
				{
					tempBuffer.WriteUInt16(0);
				}
				else
				{
					tempBuffer.WriteUInt16((ushort)((int)this.LastNetworkUpdateID - this.memInput.Count - 1));
				}
			}
			else
			{
				tempBuffer.WriteBoolean(false);
				bool aiming = false;
				bool use = false;
				bool attack = false;
				bool shoot = false;
				if (this.IsRemotePlayer)
				{
					aiming = this.dequeuedInput.HasFlag(Character.InputNetFlags.Aim);
					use = this.dequeuedInput.HasFlag(Character.InputNetFlags.Use);
					attack = this.dequeuedInput.HasFlag(Character.InputNetFlags.Attack);
					shoot = this.dequeuedInput.HasFlag(Character.InputNetFlags.Shoot);
				}
				else if (this.keys != null)
				{
					aiming = this.keys[2].GetHeldQueue;
					use = this.keys[1].GetHeldQueue;
					attack = this.keys[7].GetHeldQueue;
					shoot = this.keys[25].GetHeldQueue;
					this.networkUpdateSent = true;
				}
				tempBuffer.WriteBoolean(aiming);
				tempBuffer.WriteBoolean(shoot);
				tempBuffer.WriteBoolean(use);
				HumanoidAnimController humanAnim = this.AnimController as HumanoidAnimController;
				if (humanAnim != null)
				{
					tempBuffer.WriteBoolean(humanAnim.Crouching);
				}
				else
				{
					FishAnimController fishAnim = this.AnimController as FishAnimController;
					if (fishAnim != null)
					{
						tempBuffer.WriteBoolean(fishAnim.Reverse);
					}
				}
				tempBuffer.WriteBoolean(attack);
				Vector2 relativeCursorPos = this.cursorPosition - this.AimRefPosition;
				tempBuffer.WriteUInt16((ushort)(65535.0 * Math.Atan2((double)relativeCursorPos.Y, (double)relativeCursorPos.X) / 6.283185307179586));
				tempBuffer.WriteBoolean(this.IsRagdolled || this.Stun > 0f || this.IsDead || this.IsIncapacitated);
				tempBuffer.WriteBoolean(this.AnimController.Dir > 0f);
			}
			if (this.SelectedCharacter != null || this.HasSelectedAnyItem)
			{
				tempBuffer.WriteBoolean(true);
				tempBuffer.WriteUInt16((this.SelectedCharacter != null) ? this.SelectedCharacter.ID : 0);
				tempBuffer.WriteUInt16((this.SelectedItem != null) ? this.SelectedItem.ID : 0);
				tempBuffer.WriteUInt16((this.SelectedSecondaryItem != null) ? this.SelectedSecondaryItem.ID : 0);
				if (this.SelectedCharacter != null)
				{
					tempBuffer.WriteBoolean(this.AnimController.Anim == AnimController.Animation.CPR);
				}
			}
			else
			{
				tempBuffer.WriteBoolean(false);
			}
			tempBuffer.WriteSingle(this.SimPosition.X);
			tempBuffer.WriteSingle(this.SimPosition.Y);
			float MaxVel = 64f;
			this.AnimController.Collider.LinearVelocity = new Vector2(NetConfig.Quantize(this.AnimController.Collider.LinearVelocity.X, -MaxVel, MaxVel, 12), NetConfig.Quantize(this.AnimController.Collider.LinearVelocity.Y, -MaxVel, MaxVel, 12));
			tempBuffer.WriteRangedSingle(this.AnimController.Collider.LinearVelocity.X, -MaxVel, MaxVel, 12);
			tempBuffer.WriteRangedSingle(this.AnimController.Collider.LinearVelocity.Y, -MaxVel, MaxVel, 12);
			this.AnimController.TargetMovement = new Vector2(NetConfig.Quantize(this.AnimController.TargetMovement.X, -20f, 20f, 12), NetConfig.Quantize(this.AnimController.TargetMovement.Y, -20f, 20f, 12));
			tempBuffer.WriteRangedSingle(this.AnimController.TargetMovement.X, -20f, 20f, 12);
			tempBuffer.WriteRangedSingle(this.AnimController.TargetMovement.Y, -20f, 20f, 12);
			bool fixedRotation = this.AnimController.Collider.FarseerBody.FixedRotation;
			tempBuffer.WriteBoolean(fixedRotation);
			if (!fixedRotation)
			{
				tempBuffer.WriteSingle(this.AnimController.Collider.Rotation);
				tempBuffer.WriteSingle(this.AnimController.Collider.AngularVelocity);
			}
			tempBuffer.WriteBoolean(this.AnimController.IgnorePlatforms);
			bool writeStatus = this.healthUpdateTimer <= 0f;
			tempBuffer.WriteBoolean(writeStatus);
			if (writeStatus)
			{
				this.WriteStatus(tempBuffer, false);
				tempBuffer.WriteBoolean(this.AIController is EnemyAIController);
				EnemyAIController enemyAi = this.AIController as EnemyAIController;
				if (enemyAi != null)
				{
					tempBuffer.WriteByte((byte)enemyAi.State);
					tempBuffer.WriteBoolean(enemyAi.PetBehavior != null);
					PetBehavior petBehavior = enemyAi.PetBehavior;
					if (petBehavior != null)
					{
						tempBuffer.WriteByte((byte)(petBehavior.Happiness / petBehavior.MaxHappiness * 255f));
						tempBuffer.WriteByte((byte)(petBehavior.Hunger / petBehavior.MaxHunger * 255f));
					}
				}
				this.HealthUpdatePending = false;
			}
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00004FF0 File Offset: 0x000031F0
		public virtual void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Character.IEventData eventData = extraData as Character.IEventData;
			if (eventData == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Malformed character event: expected ");
				defaultInterpolatedStringHandler.AppendFormatted("Character");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted("IEventData");
				defaultInterpolatedStringHandler.AppendLiteral(", got ");
				defaultInterpolatedStringHandler.AppendFormatted(((extraData != null) ? extraData.GetType().Name : null) ?? "[NULL]");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			msg.WriteRangedInteger((int)eventData.EventType, 0, 18);
			Character.IEventData eventData2 = eventData;
			if (eventData2 is Character.InventoryStateEventData)
			{
				Character.InventoryStateEventData inventoryData = (Character.InventoryStateEventData)eventData2;
				ServerEntityEvent serverEntityEvent = GameMain.Server.EntityEventManager.Events.Last<ServerEntityEvent>();
				msg.WriteUInt16((serverEntityEvent != null) ? serverEntityEvent.ID : 0);
				this.Inventory.ServerEventWrite(msg, c, inventoryData);
				return;
			}
			if (eventData2 is Character.ControlEventData)
			{
				Character.ControlEventData controlEventData = (Character.ControlEventData)eventData2;
				Client owner = controlEventData.Owner;
				msg.WriteBoolean(owner == c && owner.Character == this);
				msg.WriteByte((owner != null && owner.Character == this && GameMain.Server.ConnectedClients.Contains(owner)) ? owner.SessionId : 0);
				CharacterInfo characterInfo = this.info;
				msg.WriteBoolean(characterInfo != null && characterInfo.RenamingEnabled);
				return;
			}
			if (eventData2 is Character.CharacterStatusEventData)
			{
				Character.CharacterStatusEventData statusEventData = (Character.CharacterStatusEventData)eventData2;
				this.WriteStatus(msg, statusEventData.ForceAfflictionData);
				msg.WriteBoolean(this.GodMode);
				return;
			}
			if (eventData2 is Character.UpdateSkillsEventData)
			{
				Character.UpdateSkillsEventData updateSkillsData = (Character.UpdateSkillsEventData)eventData2;
				CharacterInfo characterInfo2 = this.Info;
				Job job = (characterInfo2 != null) ? characterInfo2.Job : null;
				if (job != null)
				{
					msg.WriteIdentifier(updateSkillsData.SkillIdentifier);
					msg.WriteBoolean(updateSkillsData.ForceNotification);
					msg.WriteSingle(job.GetSkillLevel(updateSkillsData.SkillIdentifier));
					return;
				}
				msg.WriteIdentifier(Identifier.Empty);
				return;
			}
			else
			{
				Character.IAttackEventData attackEventData = eventData2 as Character.IAttackEventData;
				if (attackEventData != null)
				{
					int attackLimbIndex = base.Removed ? -1 : Array.IndexOf<Limb>(this.AnimController.Limbs, attackEventData.AttackLimb);
					ushort targetEntityId = 0;
					int targetLimbIndex = -1;
					Entity targetEntity = attackEventData.TargetEntity as Entity;
					if (targetEntity != null && !targetEntity.Removed)
					{
						targetEntityId = targetEntity.ID;
						Character character = targetEntity as Character;
						if (character != null)
						{
							AnimController animController = character.AnimController;
							if (animController != null)
							{
								Limb[] targetLimbsArray = animController.Limbs;
								targetLimbIndex = targetLimbsArray.IndexOf(attackEventData.TargetLimb);
							}
						}
					}
					msg.WriteByte((byte)((attackLimbIndex < 0) ? 255 : attackLimbIndex));
					msg.WriteUInt16(targetEntityId);
					msg.WriteByte((byte)((targetLimbIndex < 0) ? 255 : targetLimbIndex));
					msg.WriteSingle(attackEventData.TargetSimPos.X);
					msg.WriteSingle(attackEventData.TargetSimPos.Y);
					return;
				}
				if (!(eventData2 is Character.AssignCampaignInteractionEventData))
				{
					if (eventData2 is Character.ObjectiveManagerStateEventData)
					{
						Character.ObjectiveManagerStateEventData objectiveManagerStateEventData = (Character.ObjectiveManagerStateEventData)eventData2;
						AIObjectiveManager.ObjectiveType type = objectiveManagerStateEventData.ObjectiveType;
						msg.WriteRangedInteger((int)type, 0, 2);
						HumanAIController controller = this.AIController as HumanAIController;
						if (controller == null)
						{
							msg.WriteBoolean(false);
							return;
						}
						if (type == AIObjectiveManager.ObjectiveType.Order)
						{
							Order currentOrderInfo = controller.ObjectiveManager.GetCurrentOrderInfo();
							bool validOrder = currentOrderInfo != null;
							msg.WriteBoolean(validOrder);
							if (validOrder)
							{
								OrderPrefab orderPrefab = currentOrderInfo.Prefab;
								msg.WriteUInt32(orderPrefab.UintIdentifier);
								if (orderPrefab.HasOptions)
								{
									int optionIndex = orderPrefab.AllOptions.IndexOf(currentOrderInfo.Option);
									if (optionIndex == -1)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(81, 2);
										defaultInterpolatedStringHandler2.AppendLiteral("Error while writing order data. Order option \"");
										defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(currentOrderInfo.Option);
										defaultInterpolatedStringHandler2.AppendLiteral("\" not found in the order prefab \"");
										defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(orderPrefab.Name);
										defaultInterpolatedStringHandler2.AppendLiteral("\".");
										DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
									}
									msg.WriteRangedInteger(optionIndex, -1, orderPrefab.AllOptions.Length);
									return;
								}
							}
						}
						else if (type == AIObjectiveManager.ObjectiveType.Objective)
						{
							AIObjective objective = controller.ObjectiveManager.CurrentObjective;
							Identifier? identifier = (objective != null) ? new Identifier?(objective.Identifier) : null;
							bool validObjective = identifier != null && !identifier.GetValueOrDefault().IsEmpty;
							msg.WriteBoolean(validObjective);
							if (validObjective)
							{
								msg.WriteIdentifier(objective.Identifier);
								msg.WriteIdentifier(objective.Option);
								ushort targetEntityId2 = 0;
								AIObjectiveOperateItem operateObjective = objective as AIObjectiveOperateItem;
								if (operateObjective != null && operateObjective.OperateTarget != null)
								{
									targetEntityId2 = operateObjective.OperateTarget.ID;
								}
								msg.WriteUInt16(targetEntityId2);
								return;
							}
						}
					}
					else
					{
						if (eventData2 is Character.TeamChangeEventData)
						{
							msg.WriteByte((byte)this.TeamID);
							return;
						}
						if (eventData2 is Character.AddToCrewEventData)
						{
							Character.AddToCrewEventData addToCrewEventData = (Character.AddToCrewEventData)eventData2;
							msg.WriteNetSerializableStruct(addToCrewEventData.ItemTeamChange);
							return;
						}
						if (eventData2 is Character.RemoveFromCrewEventData)
						{
							Character.RemoveFromCrewEventData removeFromCrewEventData = (Character.RemoveFromCrewEventData)eventData2;
							msg.WriteNetSerializableStruct(removeFromCrewEventData.ItemTeamChange);
							return;
						}
						if (!(eventData2 is Character.UpdateExperienceEventData))
						{
							if (!(eventData2 is Character.UpdateTalentsEventData))
							{
								if (!(eventData2 is Character.UpdateMoneyEventData))
								{
									if (!(eventData2 is Character.UpdateRefundPointsEventData))
									{
										if (eventData2 is Character.ConfirmRefundEventData)
										{
											return;
										}
										Character.LatchedOntoTargetEventData latchedOntoTargetEventData;
										if (eventData2 is Character.UpdatePermanentStatsEventData)
										{
											Character.UpdatePermanentStatsEventData updatePermanentStatsEventData = (Character.UpdatePermanentStatsEventData)eventData2;
											StatTypes statType = updatePermanentStatsEventData.StatType;
											if (this.Info == null)
											{
												msg.WriteByte(0);
												msg.WriteByte(0);
												return;
											}
											if (!this.Info.SavedStatValues.ContainsKey(statType))
											{
												msg.WriteByte(0);
												msg.WriteByte((byte)statType);
												return;
											}
											msg.WriteByte((byte)this.Info.SavedStatValues[statType].Count);
											msg.WriteByte((byte)statType);
											using (List<SavedStatValue>.Enumerator enumerator = this.Info.SavedStatValues[statType].GetEnumerator())
											{
												while (enumerator.MoveNext())
												{
													SavedStatValue savedStatValue = enumerator.Current;
													msg.WriteIdentifier(savedStatValue.StatIdentifier);
													msg.WriteSingle(savedStatValue.StatValue);
													msg.WriteBoolean(savedStatValue.RemoveOnDeath);
												}
												return;
											}
										}
										else
										{
											if (!(eventData2 is Character.LatchedOntoTargetEventData))
											{
												goto IL_81A;
											}
											latchedOntoTargetEventData = (Character.LatchedOntoTargetEventData)eventData2;
										}
										msg.WriteBoolean(latchedOntoTargetEventData.IsLatched);
										if (!latchedOntoTargetEventData.IsLatched)
										{
											return;
										}
										msg.WriteSingle(this.SimPosition.X);
										msg.WriteSingle(this.SimPosition.Y);
										msg.WriteSingle(latchedOntoTargetEventData.AttachSurfaceNormal.X);
										msg.WriteSingle(latchedOntoTargetEventData.AttachSurfaceNormal.Y);
										msg.WriteSingle(latchedOntoTargetEventData.AttachPos.X);
										msg.WriteSingle(latchedOntoTargetEventData.AttachPos.Y);
										msg.WriteInt32(latchedOntoTargetEventData.TargetLevelWallIndex);
										if (latchedOntoTargetEventData.TargetStructureID != 0)
										{
											msg.WriteUInt16(latchedOntoTargetEventData.TargetStructureID);
											return;
										}
										if (latchedOntoTargetEventData.TargetCharacterID != 0)
										{
											msg.WriteUInt16(latchedOntoTargetEventData.TargetCharacterID);
											return;
										}
										msg.WriteUInt16(0);
										return;
									}
									else
									{
										CharacterInfo i = this.Info;
										if (i != null)
										{
											msg.WriteInt32(i.TalentRefundPoints);
											return;
										}
									}
									IL_81A:
									throw new Exception("Malformed character event: did not expect " + eventData.GetType().Name);
								}
							}
							else
							{
								msg.WriteUInt16((ushort)this.characterTalents.Count);
								using (List<CharacterTalent>.Enumerator enumerator2 = this.characterTalents.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										CharacterTalent unlockedTalent = enumerator2.Current;
										msg.WriteBoolean(unlockedTalent.AddedThisRound);
										msg.WriteUInt32(unlockedTalent.Prefab.UintIdentifier);
									}
									return;
								}
							}
							Wallet wallet = this.Wallet;
							msg.WriteInt32((wallet != null) ? wallet.Balance : 0);
							return;
						}
						msg.WriteInt32(this.Info.ExperiencePoints);
						msg.WriteInt32(this.info.AdditionalTalentPoints);
						return;
					}
					return;
				}
				bool canClientInteract = true;
				if (this.CampaignInteractionType == CampaignMode.InteractionType.Talk && this.ActiveConversation != null)
				{
					canClientInteract = this.ActiveConversation.CanClientStartConversation(c);
				}
				msg.WriteByte((byte)(canClientInteract ? this.CampaignInteractionType : CampaignMode.InteractionType.None));
				msg.WriteBoolean(this.RequireConsciousnessForCustomInteract);
				return;
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00005850 File Offset: 0x00003A50
		private void WriteStatus(IWriteMessage msg, bool forceAfflictionData = false)
		{
			msg.WriteBoolean(this.IsDead);
			if (this.IsDead)
			{
				msg.WriteRangedInteger((int)this.CauseOfDeath.Type, 0, Enum.GetValues(typeof(CauseOfDeathType)).Length - 1);
				if (this.CauseOfDeath.Type == CauseOfDeathType.Affliction)
				{
					msg.WriteUInt32(this.CauseOfDeath.Affliction.UintIdentifier);
				}
				Character killer = this.CauseOfDeath.Killer;
				msg.WriteUInt16((killer != null) ? killer.ID : 0);
				msg.WriteBoolean(forceAfflictionData);
				if (forceAfflictionData)
				{
					this.CharacterHealth.ServerWrite(msg);
				}
			}
			else
			{
				this.CharacterHealth.ServerWrite(msg);
			}
			AnimController animController = this.AnimController;
			if (((animController != null) ? animController.LimbJoints : null) == null)
			{
				msg.WriteByte(0);
				return;
			}
			this.severedJointIndices.Clear();
			for (int i = 0; i < this.AnimController.LimbJoints.Length; i++)
			{
				if (this.AnimController.LimbJoints[i] != null && this.AnimController.LimbJoints[i].IsSevered)
				{
					this.severedJointIndices.Add(i);
				}
			}
			msg.WriteByte((byte)this.severedJointIndices.Count);
			foreach (int jointIndex in this.severedJointIndices)
			{
				msg.WriteByte((byte)jointIndex);
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000059C8 File Offset: 0x00003BC8
		public void WriteSpawnData(IWriteMessage msg, ushort entityId, bool restrictMessageSize)
		{
			Character.<>c__DisplayClass19_0 CS$<>8__locals1 = new Character.<>c__DisplayClass19_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.restrictMessageSize = restrictMessageSize;
			if (GameMain.Server == null)
			{
				return;
			}
			CS$<>8__locals1.initialMsgLength = msg.LengthBytes;
			msg.WriteBoolean(this.Info == null);
			msg.WriteUInt16(entityId);
			msg.WriteIdentifier(this.SpeciesName);
			msg.WriteString(this.Seed);
			if (base.Removed)
			{
				msg.WriteSingle(0f);
				msg.WriteSingle(0f);
			}
			else
			{
				msg.WriteSingle(this.WorldPosition.X);
				msg.WriteSingle(this.WorldPosition.Y);
			}
			msg.WriteBoolean(this.Enabled);
			msg.WriteBoolean(this.DisabledByEvent);
			if (this.Info == null)
			{
				CS$<>8__locals1.<WriteSpawnData>g__TryWriteStatus|2(msg);
				return;
			}
			Client ownerClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == CS$<>8__locals1.<>4__this && (!c.SpectateOnly || !GameMain.Server.ServerSettings.AllowSpectating));
			if (ownerClient != null)
			{
				msg.WriteBoolean(true);
				msg.WriteByte(ownerClient.SessionId);
			}
			else if (GameMain.Server.Character == this)
			{
				msg.WriteBoolean(true);
				msg.WriteByte(0);
			}
			else
			{
				msg.WriteBoolean(false);
			}
			msg.WriteSingle(this.HumanPrefabHealthMultiplier);
			msg.WriteInt32(this.Wallet.Balance);
			msg.WriteRangedInteger(this.Wallet.RewardDistribution, 0, 100);
			msg.WriteByte((byte)this.TeamID);
			msg.WriteBoolean(this is AICharacter);
			msg.WriteIdentifier(this.info.SpeciesName);
			int msgLengthBeforeInfo = msg.LengthBytes;
			this.info.ServerWrite(msg);
			int infoLength = msg.LengthBytes - msgLengthBeforeInfo;
			msg.WriteByte((byte)this.CampaignInteractionType);
			if (this.CampaignInteractionType == CampaignMode.InteractionType.Store)
			{
				msg.WriteIdentifier(this.MerchantIdentifier);
			}
			msg.WriteIdentifier(this.Faction);
			int msgLengthBeforeOrders = msg.LengthBytes;
			msg.WriteByte((byte)this.info.CurrentOrders.Count((Order o) => o != null));
			foreach (Order orderInfo in this.info.CurrentOrders)
			{
				if (orderInfo != null)
				{
					msg.WriteUInt32(orderInfo.Prefab.UintIdentifier);
					msg.WriteUInt16((orderInfo.TargetEntity == null) ? 0 : orderInfo.TargetEntity.ID);
					bool hasOrderGiver = orderInfo.OrderGiver != null;
					msg.WriteBoolean(hasOrderGiver);
					if (hasOrderGiver)
					{
						msg.WriteUInt16(orderInfo.OrderGiver.ID);
					}
					msg.WriteByte((byte)((orderInfo.Option == Identifier.Empty) ? 0 : orderInfo.Prefab.Options.IndexOf(orderInfo.Option)));
					msg.WriteByte((byte)orderInfo.ManualPriority);
					bool hasTargetPosition = orderInfo.TargetPosition != null;
					msg.WriteBoolean(hasTargetPosition);
					if (hasTargetPosition)
					{
						msg.WriteSingle(orderInfo.TargetPosition.Position.X);
						msg.WriteSingle(orderInfo.TargetPosition.Position.Y);
						msg.WriteUInt16((orderInfo.TargetPosition.Hull == null) ? 0 : orderInfo.TargetPosition.Hull.ID);
					}
				}
			}
			int ordersLength = msg.LengthBytes - msgLengthBeforeOrders;
			if (msg.LengthBytes - CS$<>8__locals1.initialMsgLength >= 255 & CS$<>8__locals1.restrictMessageSize)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Character spawn data for \"");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" exceeded 255 bytes (info: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(infoLength);
				defaultInterpolatedStringHandler.AppendLiteral(", orders: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(ordersLength);
				defaultInterpolatedStringHandler.AppendLiteral(", total: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(msg.LengthBytes - CS$<>8__locals1.initialMsgLength);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			CS$<>8__locals1.<WriteSpawnData>g__TryWriteStatus|2(msg);
			DebugConsole.Log("Character spawn message length: " + (msg.LengthBytes - CS$<>8__locals1.initialMsgLength).ToString());
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000076 RID: 118 RVA: 0x00005E1C File Offset: 0x0000401C
		public override ContentPackage ContentPackage
		{
			get
			{
				CharacterPrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.ContentPackage;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000077 RID: 119 RVA: 0x00005E2F File Offset: 0x0000402F
		// (set) Token: 0x06000078 RID: 120 RVA: 0x00005E44 File Offset: 0x00004044
		public bool Enabled
		{
			get
			{
				return this.enabled && !base.Removed;
			}
			set
			{
				if (this.initialized && value == this.enabled)
				{
					return;
				}
				this.initialized = true;
				if (base.Removed)
				{
					this.enabled = false;
					return;
				}
				this.enabled = value;
				foreach (Limb limb in this.AnimController.Limbs)
				{
					if (!limb.IsSevered && limb.body != null)
					{
						limb.body.Enabled = this.enabled;
					}
				}
				foreach (Item item in this.HeldItems)
				{
					if (item.body != null)
					{
						if (!this.enabled)
						{
							item.body.Enabled = false;
						}
						else
						{
							Holdable component = item.GetComponent<Holdable>();
							if (component != null && component.IsActive)
							{
								item.body.Enabled = true;
							}
						}
					}
				}
				this.AnimController.Collider.Enabled = value;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00005F50 File Offset: 0x00004150
		// (set) Token: 0x0600007A RID: 122 RVA: 0x00005F58 File Offset: 0x00004158
		public bool DisabledByEvent
		{
			get
			{
				return this.disabledByEvent;
			}
			set
			{
				if (value == this.disabledByEvent)
				{
					return;
				}
				this.disabledByEvent = value;
				if (this.disabledByEvent)
				{
					this.Enabled = false;
					Character.CharacterList.Remove(this);
					if (base.AiTarget != null)
					{
						AITarget.List.Remove(base.AiTarget);
					}
				}
				else
				{
					if (!Character.CharacterList.Contains(this))
					{
						Character.CharacterList.Add(this);
					}
					if (base.AiTarget != null && !AITarget.List.Contains(base.AiTarget))
					{
						AITarget.List.Add(base.AiTarget);
					}
				}
				if (this.Inventory != null)
				{
					foreach (Item item in this.Inventory.FindAllItems(null, true, null))
					{
						item.IsActive = !this.disabledByEvent;
					}
				}
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600007B RID: 123 RVA: 0x0000604C File Offset: 0x0000424C
		public bool IsRemotelyControlled
		{
			get
			{
				if (GameMain.NetworkMember == null)
				{
					return false;
				}
				if (GameMain.NetworkMember.IsClient)
				{
					return this != Character.Controlled;
				}
				return this.IsRemotePlayer;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600007C RID: 124 RVA: 0x00006075 File Offset: 0x00004275
		// (set) Token: 0x0600007D RID: 125 RVA: 0x0000607D File Offset: 0x0000427D
		public bool IsRemotePlayer { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600007E RID: 126 RVA: 0x00006086 File Offset: 0x00004286
		public bool IsLocalPlayer
		{
			get
			{
				return Character.Controlled == this;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600007F RID: 127 RVA: 0x00006090 File Offset: 0x00004290
		public bool IsPlayer
		{
			get
			{
				return this.IsLocalPlayer || this.IsRemotePlayer;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000080 RID: 128 RVA: 0x000060A4 File Offset: 0x000042A4
		public bool IsCommanding
		{
			get
			{
				if (!this.IsPlayer)
				{
					HumanAIController humanAIController = this.AIController as HumanAIController;
					if (humanAIController != null)
					{
						ShipCommandManager shipCommandManager = humanAIController.ShipCommandManager;
						if (shipCommandManager != null)
						{
							return shipCommandManager.Active;
						}
					}
					return false;
				}
				return true;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000081 RID: 129 RVA: 0x000060DC File Offset: 0x000042DC
		public bool IsBot
		{
			get
			{
				if (!this.IsPlayer)
				{
					AIController aicontroller = this.AIController;
					return aicontroller is HumanAIController && aicontroller.Enabled;
				}
				return false;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000082 RID: 130 RVA: 0x0000610C File Offset: 0x0000430C
		public bool IsAIControlled
		{
			get
			{
				if (!this.IsPlayer)
				{
					AIController aicontroller = this.AIController;
					return aicontroller != null && aicontroller.Enabled;
				}
				return false;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00006135 File Offset: 0x00004335
		// (set) Token: 0x06000084 RID: 132 RVA: 0x0000613D File Offset: 0x0000433D
		public bool IsEscorted { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000085 RID: 133 RVA: 0x00006148 File Offset: 0x00004348
		public Identifier JobIdentifier
		{
			get
			{
				CharacterInfo characterInfo = this.Info;
				Identifier? identifier;
				if (characterInfo == null)
				{
					identifier = null;
				}
				else
				{
					Job job = characterInfo.Job;
					identifier = ((job != null) ? new Identifier?(job.Prefab.Identifier) : null);
				}
				Identifier? identifier2 = identifier;
				if (identifier2 == null)
				{
					return Identifier.Empty;
				}
				return identifier2.GetValueOrDefault();
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000086 RID: 134 RVA: 0x000061A4 File Offset: 0x000043A4
		// (set) Token: 0x06000087 RID: 135 RVA: 0x000061B6 File Offset: 0x000043B6
		public bool DoesBleed
		{
			get
			{
				return this.Params.Health.DoesBleed;
			}
			set
			{
				this.Params.Health.DoesBleed = value;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000088 RID: 136 RVA: 0x000061C9 File Offset: 0x000043C9
		// (set) Token: 0x06000089 RID: 137 RVA: 0x000061D1 File Offset: 0x000043D1
		public bool IsContainable { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600008A RID: 138 RVA: 0x000061DA File Offset: 0x000043DA
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return this.Properties;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600008B RID: 139 RVA: 0x000061E2 File Offset: 0x000043E2
		public Key[] Keys
		{
			get
			{
				return this.keys;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600008C RID: 140 RVA: 0x000061EA File Offset: 0x000043EA
		// (set) Token: 0x0600008D RID: 141 RVA: 0x000061F4 File Offset: 0x000043F4
		public HumanPrefab HumanPrefab
		{
			get
			{
				return this.humanPrefab;
			}
			set
			{
				if (this.humanPrefab == value)
				{
					return;
				}
				this.humanPrefab = value;
				if (this.humanPrefab != null)
				{
					this.HumanPrefabHealthMultiplier = this.humanPrefab.HealthMultiplier;
					if (GameMain.NetworkMember != null)
					{
						this.HumanPrefabHealthMultiplier *= this.humanPrefab.HealthMultiplierInMultiplayer;
						return;
					}
				}
				else
				{
					this.HumanPrefabHealthMultiplier = 1f;
				}
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600008E RID: 142 RVA: 0x00006258 File Offset: 0x00004458
		// (set) Token: 0x0600008F RID: 143 RVA: 0x00006292 File Offset: 0x00004492
		public Identifier Faction
		{
			get
			{
				Identifier? identifier = this.faction;
				if (identifier != null)
				{
					return identifier.GetValueOrDefault();
				}
				HumanPrefab humanPrefab = this.HumanPrefab;
				if (humanPrefab == null)
				{
					return Identifier.Empty;
				}
				return humanPrefab.Faction;
			}
			set
			{
				this.faction = new Identifier?(value);
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000090 RID: 144 RVA: 0x000062A0 File Offset: 0x000044A0
		// (set) Token: 0x06000091 RID: 145 RVA: 0x000062A8 File Offset: 0x000044A8
		public CharacterTeamType TeamID
		{
			get
			{
				return this.teamID;
			}
			set
			{
				this.teamID = value;
				if (this.info != null)
				{
					this.info.TeamID = value;
				}
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000092 RID: 146 RVA: 0x000062C8 File Offset: 0x000044C8
		public CharacterTeamType OriginalTeamID
		{
			get
			{
				CharacterTeamType? characterTeamType = this.originalTeamID;
				if (characterTeamType == null)
				{
					return this.teamID;
				}
				return characterTeamType.GetValueOrDefault();
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000093 RID: 147 RVA: 0x000062F3 File Offset: 0x000044F3
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00006301 File Offset: 0x00004501
		public Wallet Wallet
		{
			get
			{
				this.ThrowIfAccessingWalletsInSingleplayer();
				return this.wallet;
			}
			set
			{
				this.ThrowIfAccessingWalletsInSingleplayer();
				this.wallet = value;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00006310 File Offset: 0x00004510
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00006318 File Offset: 0x00004518
		public bool AllowPlayDead { get; set; }

		// Token: 0x06000097 RID: 151 RVA: 0x00006324 File Offset: 0x00004524
		public void EvaluatePlayDeadProbability(float? probability = null)
		{
			CharacterParams.AIParams aiParams = this.Params.AI;
			if (aiParams != null)
			{
				if (probability != null)
				{
					aiParams.PlayDeadProbability = probability.Value;
				}
				this.AllowPlayDead = (Rand.Value(Rand.RandSync.Unsynced) <= aiParams.PlayDeadProbability);
				return;
			}
			if (probability != null)
			{
				this.AllowPlayDead = (Rand.Value(Rand.RandSync.Unsynced) <= probability.Value);
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x0000638F File Offset: 0x0000458F
		private void ThrowIfAccessingWalletsInSingleplayer()
		{
			if ((GameMain.NetworkMember == null || GameMain.IsSingleplayer) && this.IsPlayer)
			{
				throw new InvalidOperationException("Tried to access crew wallets in singleplayer. Use CampaignMode.Bank or CampaignMode.GetWallet instead.");
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x000063B2 File Offset: 0x000045B2
		public void SetOriginalTeamAndChangeTeam(CharacterTeamType newTeam, bool processImmediately = false)
		{
			this.TryRemoveTeamChange("original");
			this.currentTeamChange = new ActiveTeamChange(newTeam, ActiveTeamChange.TeamChangePriorities.Base, false);
			this.TryAddNewTeamChange("original", this.currentTeamChange);
			if (processImmediately)
			{
				this.UpdateTeam();
			}
		}

		// Token: 0x0600009A RID: 154 RVA: 0x000063EC File Offset: 0x000045EC
		private void ChangeTeam(CharacterTeamType newTeam)
		{
			if (newTeam == this.teamID)
			{
				return;
			}
			CharacterTeamType valueOrDefault = this.originalTeamID.GetValueOrDefault();
			if (this.originalTeamID == null)
			{
				valueOrDefault = this.teamID;
				this.originalTeamID = new CharacterTeamType?(valueOrDefault);
			}
			this.TeamID = newTeam;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.AIController is HumanAIController)
			{
				Order order = OrderPrefab.Dismissal.CreateInstance(OrderPrefab.OrderTargetType.Entity, this, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
				this.SetOrder(order, true, false, false);
			}
			GameMain.NetworkMember.CreateEntityEvent(this, default(Character.TeamChangeEventData));
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00006492 File Offset: 0x00004692
		public bool HasTeamChange(string identifier)
		{
			return this.activeTeamChanges.ContainsKey(identifier);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000064A0 File Offset: 0x000046A0
		public bool TryAddNewTeamChange(string identifier, ActiveTeamChange newTeamChange)
		{
			bool success = this.activeTeamChanges.TryAdd(identifier, newTeamChange);
			if (success && this.currentTeamChange == null)
			{
				this.SetOriginalTeamAndChangeTeam(this.TeamID, false);
			}
			return success;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x000064D4 File Offset: 0x000046D4
		public bool TryRemoveTeamChange(string identifier)
		{
			ActiveTeamChange removedTeamChange;
			if (this.activeTeamChanges.TryGetValue(identifier, out removedTeamChange) && this.currentTeamChange == removedTeamChange)
			{
				this.currentTeamChange = this.activeTeamChanges["original"];
			}
			return this.activeTeamChanges.Remove(identifier);
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0000651C File Offset: 0x0000471C
		public void UpdateTeam()
		{
			if (this.currentTeamChange == null)
			{
				return;
			}
			ActiveTeamChange bestTeamChange = this.currentTeamChange;
			foreach (KeyValuePair<string, ActiveTeamChange> desiredTeamChange in this.activeTeamChanges)
			{
				if (bestTeamChange.TeamChangePriority < desiredTeamChange.Value.TeamChangePriority)
				{
					bestTeamChange = desiredTeamChange.Value;
				}
			}
			if (this.TeamID != bestTeamChange.DesiredTeamId)
			{
				this.ChangeTeam(bestTeamChange.DesiredTeamId);
				this.currentTeamChange = bestTeamChange;
				if (bestTeamChange.AggressiveBehavior && this.AIController is HumanAIController)
				{
					Order order = OrderPrefab.Prefabs["fightintruders"].CreateInstance(OrderPrefab.OrderTargetType.Entity, this, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
					this.SetOrder(order, true, false, false);
				}
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600009F RID: 159 RVA: 0x000065F8 File Offset: 0x000047F8
		public bool IsOnPlayerTeam
		{
			get
			{
				return this.teamID == CharacterTeamType.Team1 || (this.teamID == CharacterTeamType.Team2 && !this.IsFriendlyNPCTurnedHostile);
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000A0 RID: 160 RVA: 0x0000661C File Offset: 0x0000481C
		public bool IsOriginallyOnPlayerTeam
		{
			get
			{
				CharacterTeamType? characterTeamType = this.originalTeamID;
				if (characterTeamType != null)
				{
					CharacterTeamType valueOrDefault = characterTeamType.GetValueOrDefault();
					if (valueOrDefault - CharacterTeamType.Team1 <= 1)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00006650 File Offset: 0x00004850
		public bool IsFriendlyNPCTurnedHostile
		{
			get
			{
				bool flag = this.originalTeamID.GetValueOrDefault() == CharacterTeamType.FriendlyNPC;
				bool flag2 = flag;
				if (flag2)
				{
					CharacterTeamType characterTeamType = this.teamID;
					bool flag3 = characterTeamType == CharacterTeamType.None || characterTeamType == CharacterTeamType.Team2;
					flag2 = flag3;
				}
				return flag2;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000A2 RID: 162 RVA: 0x00006688 File Offset: 0x00004888
		public bool IsInstigator
		{
			get
			{
				CombatAction combatAction = this.CombatAction;
				return combatAction != null && combatAction.IsInstigator;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x000066A7 File Offset: 0x000048A7
		public IEnumerable<Character.Attacker> LastAttackers
		{
			get
			{
				return this.lastAttackers;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x000066AF File Offset: 0x000048AF
		public Character LastAttacker
		{
			get
			{
				Character.Attacker attacker = this.lastAttackers.LastOrDefault<Character.Attacker>();
				if (attacker == null)
				{
					return null;
				}
				return attacker.Character;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x000066C7 File Offset: 0x000048C7
		// (set) Token: 0x060000A6 RID: 166 RVA: 0x000066CF File Offset: 0x000048CF
		public Character LastOrderedCharacter { get; private set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x000066D8 File Offset: 0x000048D8
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x000066E0 File Offset: 0x000048E0
		public Character SecondLastOrderedCharacter { get; private set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x000066E9 File Offset: 0x000048E9
		public Dictionary<ItemPrefab, double> ItemSelectedDurations
		{
			get
			{
				return this.itemSelectedDurations;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000AA RID: 170 RVA: 0x000066F1 File Offset: 0x000048F1
		// (set) Token: 0x060000AB RID: 171 RVA: 0x000066F9 File Offset: 0x000048F9
		public float InvisibleTimer { get; set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00006702 File Offset: 0x00004902
		public Identifier SpeciesName
		{
			get
			{
				CharacterParams @params = this.Params;
				if (@params == null)
				{
					return "null".ToIdentifier();
				}
				return @params.SpeciesName;
			}
		}

		// Token: 0x060000AD RID: 173 RVA: 0x0000671E File Offset: 0x0000491E
		public Identifier GetBaseCharacterSpeciesName()
		{
			return this.Prefab.GetBaseCharacterSpeciesName(this.SpeciesName);
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00006734 File Offset: 0x00004934
		public Identifier Group
		{
			get
			{
				HumanPrefab prefab = this.HumanPrefab;
				if (prefab == null || prefab.Group.IsEmpty)
				{
					return this.Params.Group;
				}
				return prefab.Group;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000AF RID: 175 RVA: 0x0000676D File Offset: 0x0000496D
		public bool IsHumanoid
		{
			get
			{
				return this.Params.Humanoid;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x0000677A File Offset: 0x0000497A
		public bool IsMachine
		{
			get
			{
				return this.Params.IsMachine;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00006787 File Offset: 0x00004987
		public bool IsHusk
		{
			get
			{
				return this.Params.Husk;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x00006794 File Offset: 0x00004994
		public bool IsDisguisedAsHusk
		{
			get
			{
				return this.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.DisguisedAsHuskType, true) > 0f;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x000067AE File Offset: 0x000049AE
		public bool IsHuskInfected
		{
			get
			{
				return this.CharacterHealth.GetActiveAfflictionTags().Contains(Tags.HuskInfected);
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x000067C5 File Offset: 0x000049C5
		public bool IsMale
		{
			get
			{
				CharacterInfo characterInfo = this.info;
				return characterInfo != null && characterInfo.IsMale;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x000067D8 File Offset: 0x000049D8
		public bool IsFemale
		{
			get
			{
				CharacterInfo characterInfo = this.info;
				return characterInfo != null && characterInfo.IsFemale;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x000067EB File Offset: 0x000049EB
		public string BloodDecalName
		{
			get
			{
				return this.Params.BloodDecal;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x000067F8 File Offset: 0x000049F8
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x00006805 File Offset: 0x00004A05
		public bool CanSpeak
		{
			get
			{
				return this.Params.CanSpeak;
			}
			set
			{
				this.Params.CanSpeak = value;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x00006813 File Offset: 0x00004A13
		// (set) Token: 0x060000BA RID: 186 RVA: 0x00006820 File Offset: 0x00004A20
		public bool NeedsAir
		{
			get
			{
				return this.Params.NeedsAir;
			}
			set
			{
				this.Params.NeedsAir = value;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000BB RID: 187 RVA: 0x0000682E File Offset: 0x00004A2E
		// (set) Token: 0x060000BC RID: 188 RVA: 0x0000683B File Offset: 0x00004A3B
		public bool NeedsWater
		{
			get
			{
				return this.Params.NeedsWater;
			}
			set
			{
				this.Params.NeedsWater = value;
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000BD RID: 189 RVA: 0x00006849 File Offset: 0x00004A49
		public bool NeedsOxygen
		{
			get
			{
				return this.NeedsAir || (this.NeedsWater && !this.AnimController.InWater);
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000BE RID: 190 RVA: 0x0000686D File Offset: 0x00004A6D
		// (set) Token: 0x060000BF RID: 191 RVA: 0x0000687A File Offset: 0x00004A7A
		public float Noise
		{
			get
			{
				return this.Params.Noise;
			}
			set
			{
				this.Params.Noise = value;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000C0 RID: 192 RVA: 0x00006888 File Offset: 0x00004A88
		// (set) Token: 0x060000C1 RID: 193 RVA: 0x00006895 File Offset: 0x00004A95
		public float Visibility
		{
			get
			{
				return this.Params.Visibility;
			}
			set
			{
				this.Params.Visibility = value;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x000068A3 File Offset: 0x00004AA3
		// (set) Token: 0x060000C3 RID: 195 RVA: 0x000068BF File Offset: 0x00004ABF
		public float MaxPerceptionDistance
		{
			get
			{
				CharacterParams.AIParams ai = this.Params.AI;
				if (ai == null)
				{
					return 0f;
				}
				return ai.MaxPerceptionDistance;
			}
			set
			{
				if (this.Params.AI != null)
				{
					this.Params.AI.MaxPerceptionDistance = value;
				}
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x000068DF File Offset: 0x00004ADF
		// (set) Token: 0x060000C5 RID: 197 RVA: 0x000068E7 File Offset: 0x00004AE7
		public bool IsTraitor { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000C6 RID: 198 RVA: 0x000068F0 File Offset: 0x00004AF0
		public bool IsHuman
		{
			get
			{
				Identifier speciesName = this.SpeciesName;
				return speciesName == CharacterPrefab.HumanSpeciesName;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x00006910 File Offset: 0x00004B10
		public List<Order> CurrentOrders
		{
			get
			{
				CharacterInfo characterInfo = this.Info;
				if (characterInfo == null)
				{
					return null;
				}
				return characterInfo.CurrentOrders;
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00006923 File Offset: 0x00004B23
		public bool IsDismissed
		{
			get
			{
				return this.GetCurrentOrderWithTopPriority() == null;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x0000692E File Offset: 0x00004B2E
		// (set) Token: 0x060000CA RID: 202 RVA: 0x00006936 File Offset: 0x00004B36
		public Entity ViewTarget { get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000CB RID: 203 RVA: 0x00006940 File Offset: 0x00004B40
		public Vector2 AimRefPosition
		{
			get
			{
				if (this.ViewTarget == null)
				{
					return this.AnimController.AimSourcePos;
				}
				Vector2 viewTargetWorldPos = this.ViewTarget.WorldPosition;
				Item targetItem = this.ViewTarget as Item;
				if (targetItem != null)
				{
					Turret turret = targetItem.GetComponent<Turret>();
					if (turret != null)
					{
						viewTargetWorldPos = new Vector2((float)targetItem.WorldRect.X + turret.TransformedBarrelPos.X, (float)targetItem.WorldRect.Y - turret.TransformedBarrelPos.Y);
					}
				}
				return this.Position + (viewTargetWorldPos - this.WorldPosition);
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000CC RID: 204 RVA: 0x000069D4 File Offset: 0x00004BD4
		// (set) Token: 0x060000CD RID: 205 RVA: 0x000069DC File Offset: 0x00004BDC
		public CharacterInfo Info
		{
			get
			{
				return this.info;
			}
			set
			{
				if (this.info != null && this.info != value)
				{
					this.info.Remove();
				}
				this.info = value;
				if (this.info != null)
				{
					this.info.Character = this;
				}
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000CE RID: 206 RVA: 0x00006A15 File Offset: 0x00004C15
		public Identifier VariantOf
		{
			get
			{
				return this.Prefab.VariantOf;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000CF RID: 207 RVA: 0x00006A24 File Offset: 0x00004C24
		public string Name
		{
			get
			{
				if (this.info == null || string.IsNullOrWhiteSpace(this.info.Name))
				{
					return this.SpeciesName.Value;
				}
				return this.info.Name;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x00006A68 File Offset: 0x00004C68
		public string DisplayName
		{
			get
			{
				if (this.IsPet)
				{
					EnemyAIController enemyAIController = this.AIController as EnemyAIController;
					if (enemyAIController != null)
					{
						PetBehavior petBehavior = enemyAIController.PetBehavior;
						if (petBehavior != null)
						{
							string petName = petBehavior.GetTagName();
							if (!string.IsNullOrEmpty(petName))
							{
								return petName;
							}
						}
					}
				}
				if (this.info != null && !string.IsNullOrWhiteSpace(this.info.Name))
				{
					return this.info.Name;
				}
				LocalizedString displayName = this.Params.DisplayName;
				if (displayName.IsNullOrWhiteSpace())
				{
					if (this.Params.SpeciesTranslationOverride.IsEmpty)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Character.");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpeciesName);
						displayName = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Character.");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Params.SpeciesTranslationOverride);
						displayName = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
				}
				if (!displayName.IsNullOrWhiteSpace())
				{
					return displayName.Value;
				}
				return this.Name;
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x00006B78 File Offset: 0x00004D78
		public string LogName
		{
			get
			{
				if (GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.AllowDisguises)
				{
					return this.Name;
				}
				if (this.info == null || string.IsNullOrWhiteSpace(this.info.Name))
				{
					return this.SpeciesName.Value;
				}
				return this.info.Name + ((this.info.DisplayName != this.info.Name) ? (" (as " + this.info.DisplayName + ")") : "");
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x00006C1B File Offset: 0x00004E1B
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x00006C2C File Offset: 0x00004E2C
		public bool HideFace
		{
			get
			{
				return this.hideFaceTimer > 0f;
			}
			set
			{
				bool wasHidden = this.HideFace;
				this.hideFaceTimer = MathHelper.Clamp(this.hideFaceTimer + (value ? 1f : -0.5f), 0f, 10f);
				bool isHidden = this.HideFace;
				if (isHidden != wasHidden && this.info != null && this.info.IsDisguisedAsAnother != isHidden)
				{
					this.info.CheckDisguiseStatus(true, null);
				}
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x00006C99 File Offset: 0x00004E99
		public string ConfigPath
		{
			get
			{
				return this.Params.File.Path.Value;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x00006CB0 File Offset: 0x00004EB0
		public float Mass
		{
			get
			{
				return this.AnimController.Mass;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00006CBD File Offset: 0x00004EBD
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x00006CC5 File Offset: 0x00004EC5
		public CharacterInventory Inventory { get; private set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x00006CCE File Offset: 0x00004ECE
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x00006CD6 File Offset: 0x00004ED6
		public bool DisableInteract { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060000DA RID: 218 RVA: 0x00006CDF File Offset: 0x00004EDF
		// (set) Token: 0x060000DB RID: 219 RVA: 0x00006CE7 File Offset: 0x00004EE7
		public bool DisableFocusingOnEntities { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00006CF0 File Offset: 0x00004EF0
		// (set) Token: 0x060000DD RID: 221 RVA: 0x00006CF8 File Offset: 0x00004EF8
		public LocalizedString CustomInteractHUDText { get; private set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00006D04 File Offset: 0x00004F04
		public bool AllowCustomInteract
		{
			get
			{
				if (CampaignMode.HostileFactionDisablesInteraction(this.CampaignInteractionType))
				{
					HumanAIController humanAi = this.AIController as HumanAIController;
					if (humanAi != null && humanAi.IsInHostileFaction())
					{
						return false;
					}
				}
				return (!this.RequireConsciousnessForCustomInteract || (!this.IsIncapacitated && this.Stun <= 0f)) && !base.Removed;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060000DF RID: 223 RVA: 0x00006D60 File Offset: 0x00004F60
		public bool ShouldShowCustomInteractText
		{
			get
			{
				if (!this.CustomInteractHUDText.IsNullOrEmpty() && this.AllowCustomInteract)
				{
					HumanAIController humanAi = this.AIController as HumanAIController;
					return humanAi == null || humanAi.AllowCampaignInteraction();
				}
				return false;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00006D9B File Offset: 0x00004F9B
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x00006DAA File Offset: 0x00004FAA
		public bool LockHands
		{
			get
			{
				return this.lockHandsTimer > 0f;
			}
			set
			{
				this.lockHandsTimer = MathHelper.Clamp(this.lockHandsTimer + (value ? 1f : -0.5f), 0f, 10f);
				if (value)
				{
					this.SelectedCharacter = null;
				}
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x00006DE1 File Offset: 0x00004FE1
		public bool AllowInput
		{
			get
			{
				return !base.Removed && !this.IsIncapacitated && this.Stun <= 0f;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x00006E05 File Offset: 0x00005005
		public bool CanMove
		{
			get
			{
				return (this.AnimController.InWater || this.AnimController.CanWalk) && this.AllowInput;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00006E2E File Offset: 0x0000502E
		public bool CanInteract
		{
			get
			{
				return this.AllowInput && this.Params.CanInteract && !this.LockHands;
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00006E50 File Offset: 0x00005050
		public bool CanEat
		{
			get
			{
				return !this.IsHumanoid && this.Params.CanEat && this.AllowInput && this.AnimController.GetLimb(LimbType.Head, true, false, false) != null;
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x00006E84 File Offset: 0x00005084
		public bool CanClimb
		{
			get
			{
				return this.Params.CanClimb && this.CanInteract;
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060000E7 RID: 231 RVA: 0x00006E9B File Offset: 0x0000509B
		// (set) Token: 0x060000E8 RID: 232 RVA: 0x00006EA3 File Offset: 0x000050A3
		public Vector2 CursorPosition
		{
			get
			{
				return this.cursorPosition;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.cursorPosition = value;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060000E9 RID: 233 RVA: 0x00006EB5 File Offset: 0x000050B5
		// (set) Token: 0x060000EA RID: 234 RVA: 0x00006EBD File Offset: 0x000050BD
		public Vector2 SmoothedCursorPosition { get; private set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00006EC6 File Offset: 0x000050C6
		public Vector2 CursorWorldPosition
		{
			get
			{
				if (base.Submarine != null)
				{
					return this.cursorPosition + base.Submarine.Position;
				}
				return this.cursorPosition;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060000EC RID: 236 RVA: 0x00006EED File Offset: 0x000050ED
		// (set) Token: 0x060000ED RID: 237 RVA: 0x00006EF5 File Offset: 0x000050F5
		public Character FocusedCharacter { get; set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060000EE RID: 238 RVA: 0x00006EFE File Offset: 0x000050FE
		// (set) Token: 0x060000EF RID: 239 RVA: 0x00006F08 File Offset: 0x00005108
		public Character SelectedCharacter
		{
			get
			{
				return this.selectedCharacter;
			}
			set
			{
				if (value == this.selectedCharacter)
				{
					return;
				}
				if (this.selectedCharacter != null)
				{
					this.selectedCharacter.selectedBy = null;
					foreach (Character otherCharacter in Character.CharacterList)
					{
						if (otherCharacter != this && otherCharacter.selectedCharacter == this.selectedCharacter)
						{
							this.selectedCharacter.selectedBy = otherCharacter;
							break;
						}
					}
				}
				CharacterHUD.RecreateHudTextsIfControlling(this);
				this.selectedCharacter = value;
				if (this.selectedCharacter != null)
				{
					this.selectedCharacter.selectedBy = this;
				}
				bool flag;
				if (!GameMain.IsSingleplayer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					flag = (networkMember != null && networkMember.IsServer);
				}
				else
				{
					flag = true;
				}
				bool isServerOrSingleplayer = flag;
				this.CheckTalents(AbilityEffectType.OnLootCharacter, new AbilityCharacterLoot(value));
				if (this.IsPlayer && isServerOrSingleplayer && value != null && value.IsDead)
				{
					Wallet grabbedWallet = value.Wallet;
					if (grabbedWallet != null)
					{
						int balance = grabbedWallet.Balance;
						if (balance > 0)
						{
							MultiPlayerCampaign mpCampaign = GameMain.GameSession.Campaign as MultiPlayerCampaign;
							if (mpCampaign != null)
							{
								GameServer server = GameMain.Server;
								if (server != null)
								{
									ServerSettings settings = server.ServerSettings;
									if (settings != null)
									{
										LootedMoneyDestination lootedMoneyDestination = settings.LootedMoneyDestination;
										if (lootedMoneyDestination == LootedMoneyDestination.Wallet && this.IsPlayer)
										{
											this.Wallet.Give(balance);
										}
										else
										{
											mpCampaign.Bank.Give(balance);
										}
									}
								}
							}
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
							defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(this));
							defaultInterpolatedStringHandler.AppendLiteral(" grabbed ");
							defaultInterpolatedStringHandler.AppendFormatted(value.Name);
							defaultInterpolatedStringHandler.AppendLiteral("'s body and received ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(grabbedWallet.Balance);
							defaultInterpolatedStringHandler.AppendLiteral(" mk.");
							GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Money);
							grabbedWallet.Deduct(balance);
							if (mpCampaign != null && this.selectedCharacter.Info != null)
							{
								CharacterCampaignData characterCampaignData = (mpCampaign != null) ? mpCampaign.GetCharacterData(this.selectedCharacter.Info) : null;
								if (characterCampaignData != null)
								{
									characterCampaignData.WalletData = grabbedWallet.Save();
									if (characterCampaignData != null)
									{
										characterCampaignData.ApplyWalletData(this.selectedCharacter);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x00007134 File Offset: 0x00005334
		// (set) Token: 0x060000F1 RID: 241 RVA: 0x0000713C File Offset: 0x0000533C
		public Character SelectedBy
		{
			get
			{
				return this.selectedBy;
			}
			set
			{
				if (this.selectedBy != null)
				{
					this.selectedBy.selectedCharacter = null;
				}
				this.selectedBy = value;
				if (this.selectedBy != null)
				{
					this.selectedBy.selectedCharacter = this;
				}
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x00007170 File Offset: 0x00005370
		public IEnumerable<Item> HeldItems
		{
			get
			{
				Character.<get_HeldItems>d__294 <get_HeldItems>d__ = new Character.<get_HeldItems>d__294(-2);
				<get_HeldItems>d__.<>4__this = this;
				return <get_HeldItems>d__;
			}
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00007190 File Offset: 0x00005390
		public bool IsDualWieldingRangedWeapons()
		{
			int rangedItemCount = 0;
			foreach (Item item in this.HeldItems)
			{
				if (item.GetComponent<RangedWeapon>() != null)
				{
					rangedItemCount++;
				}
				if (rangedItemCount > 1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060000F4 RID: 244 RVA: 0x000071F0 File Offset: 0x000053F0
		// (set) Token: 0x060000F5 RID: 245 RVA: 0x000071F8 File Offset: 0x000053F8
		public float LowPassMultiplier
		{
			get
			{
				return this.lowPassMultiplier;
			}
			set
			{
				this.lowPassMultiplier = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x00007210 File Offset: 0x00005410
		// (set) Token: 0x060000F7 RID: 247 RVA: 0x00007218 File Offset: 0x00005418
		public float ObstructVisionAmount
		{
			get
			{
				return this.obstructVisionAmount;
			}
			set
			{
				this.obstructVisionAmount = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x00007230 File Offset: 0x00005430
		// (set) Token: 0x060000F9 RID: 249 RVA: 0x0000723F File Offset: 0x0000543F
		public bool ObstructVision
		{
			get
			{
				return this.obstructVisionAmount > 0.01f;
			}
			set
			{
				this.obstructVisionAmount = (value ? 0.5f : 0f);
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060000FA RID: 250 RVA: 0x00007256 File Offset: 0x00005456
		// (set) Token: 0x060000FB RID: 251 RVA: 0x0000725E File Offset: 0x0000545E
		public float PressureProtection
		{
			get
			{
				return this.pressureProtection;
			}
			set
			{
				this.pressureProtection = Math.Max(value, this.pressureProtection);
				this.pressureProtectionLastSet = Timing.TotalTime;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060000FC RID: 252 RVA: 0x0000727D File Offset: 0x0000547D
		public bool InPressure
		{
			get
			{
				return this.CurrentHull == null || this.CurrentHull.LethalPressure > 0f;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x060000FD RID: 253 RVA: 0x0000729B File Offset: 0x0000549B
		public AnimController.Animation Anim
		{
			get
			{
				AnimController animController = this.AnimController;
				if (animController == null)
				{
					return AnimController.Animation.None;
				}
				return animController.Anim;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x060000FE RID: 254 RVA: 0x000072AE File Offset: 0x000054AE
		public bool IsIncapacitated
		{
			get
			{
				return this.IsUnconscious || this.CharacterHealth.IsParalyzed;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060000FF RID: 255 RVA: 0x000072C5 File Offset: 0x000054C5
		public bool IsUnconscious
		{
			get
			{
				return this.CharacterHealth.IsUnconscious;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000100 RID: 256 RVA: 0x000072D4 File Offset: 0x000054D4
		public bool IsHandcuffed
		{
			get
			{
				return this.IsHuman && this.HasEquippedItem(Tags.HandLockerItem, true, null);
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000101 RID: 257 RVA: 0x00007300 File Offset: 0x00005500
		public bool IsPet
		{
			get
			{
				return this.Params.IsPet;
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000102 RID: 258 RVA: 0x0000730D File Offset: 0x0000550D
		// (set) Token: 0x06000103 RID: 259 RVA: 0x0000731A File Offset: 0x0000551A
		public float Oxygen
		{
			get
			{
				return this.CharacterHealth.OxygenAmount;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.CharacterHealth.OxygenAmount = MathHelper.Clamp(value, -100f, 100f);
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000104 RID: 260 RVA: 0x00007340 File Offset: 0x00005540
		// (set) Token: 0x06000105 RID: 261 RVA: 0x00007348 File Offset: 0x00005548
		public float OxygenAvailable
		{
			get
			{
				return this.oxygenAvailable;
			}
			set
			{
				this.oxygenAvailable = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000106 RID: 262 RVA: 0x00007360 File Offset: 0x00005560
		public float HullOxygenPercentage
		{
			get
			{
				Hull currentHull = this.CurrentHull;
				if (currentHull == null)
				{
					return 0f;
				}
				return currentHull.OxygenPercentage;
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000107 RID: 263 RVA: 0x00007377 File Offset: 0x00005577
		// (set) Token: 0x06000108 RID: 264 RVA: 0x0000737F File Offset: 0x0000557F
		public bool UseHullOxygen { get; set; } = true;

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000109 RID: 265 RVA: 0x00007388 File Offset: 0x00005588
		// (set) Token: 0x0600010A RID: 266 RVA: 0x000073B0 File Offset: 0x000055B0
		public float Stun
		{
			get
			{
				if (!this.IsRagdolled || this.AnimController.IsHangingWithRope)
				{
					return this.CharacterHealth.Stun;
				}
				return 1f;
			}
			set
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				this.SetStun(value, true, false);
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600010B RID: 267 RVA: 0x000073CF File Offset: 0x000055CF
		// (set) Token: 0x0600010C RID: 268 RVA: 0x000073D7 File Offset: 0x000055D7
		public CharacterHealth CharacterHealth { get; private set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600010D RID: 269 RVA: 0x000073E0 File Offset: 0x000055E0
		public float Vitality
		{
			get
			{
				return this.CharacterHealth.Vitality;
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600010E RID: 270 RVA: 0x000073ED File Offset: 0x000055ED
		public float Health
		{
			get
			{
				return this.Vitality;
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600010F RID: 271 RVA: 0x000073F5 File Offset: 0x000055F5
		public float HealthPercentage
		{
			get
			{
				return this.CharacterHealth.HealthPercentage;
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000110 RID: 272 RVA: 0x00007402 File Offset: 0x00005602
		public float MaxVitality
		{
			get
			{
				return this.CharacterHealth.MaxVitality;
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000111 RID: 273 RVA: 0x0000740F File Offset: 0x0000560F
		public float MaxHealth
		{
			get
			{
				return this.MaxVitality;
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00007417 File Offset: 0x00005617
		public bool WasFullHealth
		{
			get
			{
				return this.CharacterHealth.WasInFullHealth;
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000113 RID: 275 RVA: 0x00007424 File Offset: 0x00005624
		public AIState AIState
		{
			get
			{
				EnemyAIController enemyAI = this.AIController as EnemyAIController;
				if (enemyAI == null)
				{
					return AIState.Idle;
				}
				return enemyAI.State;
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00007448 File Offset: 0x00005648
		public bool IsLatched
		{
			get
			{
				EnemyAIController enemyAI = this.AIController as EnemyAIController;
				return enemyAI != null && enemyAI.LatchOntoAI != null && enemyAI.LatchOntoAI.IsAttached;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000115 RID: 277 RVA: 0x00007479 File Offset: 0x00005679
		public float EmpVulnerability
		{
			get
			{
				return this.Params.Health.EmpVulnerability;
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000116 RID: 278 RVA: 0x0000748B File Offset: 0x0000568B
		public float PoisonVulnerability
		{
			get
			{
				return this.Params.Health.PoisonVulnerability;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000117 RID: 279 RVA: 0x0000749D File Offset: 0x0000569D
		public bool IsFlipped
		{
			get
			{
				return this.AnimController.IsFlipped;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000118 RID: 280 RVA: 0x000074AA File Offset: 0x000056AA
		// (set) Token: 0x06000119 RID: 281 RVA: 0x000074B7 File Offset: 0x000056B7
		public float Bloodloss
		{
			get
			{
				return this.CharacterHealth.BloodlossAmount;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.CharacterHealth.BloodlossAmount = value;
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x0600011A RID: 282 RVA: 0x000074CE File Offset: 0x000056CE
		public float Bleeding
		{
			get
			{
				return this.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.BleedingType, true);
			}
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x0600011B RID: 283 RVA: 0x000074E1 File Offset: 0x000056E1
		// (set) Token: 0x0600011C RID: 284 RVA: 0x00007507 File Offset: 0x00005707
		public float SpeechImpediment
		{
			get
			{
				if (!this.CanSpeak || this.IsUnconscious || this.IsKnockedDown)
				{
					return 100f;
				}
				return this.speechImpediment;
			}
			set
			{
				if (value < this.speechImpediment)
				{
					return;
				}
				this.speechImpedimentSet = true;
				this.speechImpediment = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x0600011D RID: 285 RVA: 0x00007530 File Offset: 0x00005730
		// (set) Token: 0x0600011E RID: 286 RVA: 0x00007538 File Offset: 0x00005738
		public float TextChatVolume
		{
			get
			{
				return this.textChatVolume;
			}
			set
			{
				this.textChatVolume = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00007550 File Offset: 0x00005750
		// (set) Token: 0x06000120 RID: 288 RVA: 0x00007558 File Offset: 0x00005758
		public float PressureTimer { get; private set; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00007561 File Offset: 0x00005761
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00007569 File Offset: 0x00005769
		public float DisableImpactDamageTimer { get; set; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000123 RID: 291 RVA: 0x00007572 File Offset: 0x00005772
		// (set) Token: 0x06000124 RID: 292 RVA: 0x0000757A File Offset: 0x0000577A
		public bool IgnoreMeleeWeapons { get; set; }

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000125 RID: 293 RVA: 0x00007584 File Offset: 0x00005784
		public float CurrentSpeed
		{
			get
			{
				AnimController animController = this.AnimController;
				float? num;
				if (animController == null)
				{
					num = null;
				}
				else
				{
					PhysicsBody collider = animController.Collider;
					num = ((collider != null) ? new float?(collider.LinearVelocity.Length()) : null);
				}
				float? num2 = num;
				return num2.GetValueOrDefault();
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000126 RID: 294 RVA: 0x000075D4 File Offset: 0x000057D4
		// (set) Token: 0x06000127 RID: 295 RVA: 0x000075DC File Offset: 0x000057DC
		public Item SelectedItem
		{
			get
			{
				return this._selectedItem;
			}
			set
			{
				Item prevSelectedItem = this._selectedItem;
				this._selectedItem = value;
				if (value != null)
				{
					this.CheckTalents(AbilityEffectType.OnItemSelected, new AbilityItemSelected(value));
				}
				if (prevSelectedItem != null && (this._selectedItem == null || this._selectedItem != prevSelectedItem) && this.itemSelectedTime > 0.0)
				{
					double selectedDuration = Timing.TotalTime - this.itemSelectedTime;
					if (this.itemSelectedDurations.ContainsKey(prevSelectedItem.Prefab))
					{
						Dictionary<ItemPrefab, double> dictionary = this.itemSelectedDurations;
						ItemPrefab prefab = prevSelectedItem.Prefab;
						dictionary[prefab] += selectedDuration;
					}
					else
					{
						this.itemSelectedDurations.Add(prevSelectedItem.Prefab, selectedDuration);
					}
					this.itemSelectedTime = 0.0;
				}
				if (this._selectedItem != null && (prevSelectedItem == null || prevSelectedItem != this._selectedItem))
				{
					this.itemSelectedTime = Timing.TotalTime;
				}
				if (prevSelectedItem != this._selectedItem && prevSelectedItem != null && prevSelectedItem.OnDeselect != null)
				{
					prevSelectedItem.OnDeselect(this);
				}
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000128 RID: 296 RVA: 0x000076D0 File Offset: 0x000058D0
		// (set) Token: 0x06000129 RID: 297 RVA: 0x000076D8 File Offset: 0x000058D8
		public Item SelectedSecondaryItem { get; set; }

		// Token: 0x0600012A RID: 298 RVA: 0x000076E1 File Offset: 0x000058E1
		public void ReleaseSecondaryItem()
		{
			this.SelectedSecondaryItem = null;
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x0600012B RID: 299 RVA: 0x000076EA File Offset: 0x000058EA
		public bool HasSelectedAnyItem
		{
			get
			{
				return this.SelectedItem != null || this.SelectedSecondaryItem != null;
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000076FF File Offset: 0x000058FF
		public bool IsAnySelectedItem(Item item)
		{
			return item == this.SelectedItem || item == this.SelectedSecondaryItem;
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00007715 File Offset: 0x00005915
		public bool HasSelectedAnotherSecondaryItem(Item item)
		{
			return this.SelectedSecondaryItem != null && this.SelectedSecondaryItem != item;
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x0600012E RID: 302 RVA: 0x0000772D File Offset: 0x0000592D
		// (set) Token: 0x0600012F RID: 303 RVA: 0x00007735 File Offset: 0x00005935
		public Item FocusedItem
		{
			get
			{
				return this.focusedItem;
			}
			set
			{
				this.focusedItem = value;
			}
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000130 RID: 304 RVA: 0x0000773E File Offset: 0x0000593E
		// (set) Token: 0x06000131 RID: 305 RVA: 0x00007746 File Offset: 0x00005946
		public Item PickingItem { get; set; }

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000132 RID: 306 RVA: 0x0000774F File Offset: 0x0000594F
		public virtual AIController AIController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000133 RID: 307 RVA: 0x00007752 File Offset: 0x00005952
		// (set) Token: 0x06000134 RID: 308 RVA: 0x0000775A File Offset: 0x0000595A
		public bool IsDead
		{
			get
			{
				return this.isDead;
			}
			set
			{
				if (this.isDead == value)
				{
					return;
				}
				if (value)
				{
					this.Kill(CauseOfDeathType.Unknown, null, false, true);
					return;
				}
				this.Revive(true, false);
			}
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000135 RID: 309 RVA: 0x0000777C File Offset: 0x0000597C
		// (set) Token: 0x06000136 RID: 310 RVA: 0x00007784 File Offset: 0x00005984
		public bool EnableDespawn { get; set; } = true;

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x06000137 RID: 311 RVA: 0x0000778D File Offset: 0x0000598D
		// (set) Token: 0x06000138 RID: 312 RVA: 0x00007795 File Offset: 0x00005995
		public CauseOfDeath CauseOfDeath { get; private set; }

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x06000139 RID: 313 RVA: 0x0000779E File Offset: 0x0000599E
		public CauseOfDeathType CauseOfDeathType
		{
			get
			{
				CauseOfDeath causeOfDeath = this.CauseOfDeath;
				if (causeOfDeath == null)
				{
					return CauseOfDeathType.None;
				}
				return causeOfDeath.Type;
			}
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x0600013A RID: 314 RVA: 0x000077B1 File Offset: 0x000059B1
		public bool CanBeSelected
		{
			get
			{
				return !base.Removed;
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x0600013B RID: 315 RVA: 0x000077BC File Offset: 0x000059BC
		public bool IsDraggable
		{
			get
			{
				return !base.Removed || this.AnimController.Draggable;
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x0600013C RID: 316 RVA: 0x000077D4 File Offset: 0x000059D4
		public bool CanAim
		{
			get
			{
				if (this.SelectedItem != null)
				{
					Controller component = this.SelectedItem.GetComponent<Controller>();
					if (component == null || !component.AllowAiming)
					{
						return false;
					}
				}
				if (!this.IsKnockedDownOrRagdolled)
				{
					return !this.IsRagdolled || this.AnimController.IsHoldingToRope;
				}
				return false;
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x0600013D RID: 317 RVA: 0x00007820 File Offset: 0x00005A20
		public bool InWater
		{
			get
			{
				AnimController animController = this.AnimController;
				return animController != null && animController.InWater;
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x0600013E RID: 318 RVA: 0x0000783F File Offset: 0x00005A3F
		public bool IsLowInOxygen
		{
			get
			{
				return this.CharacterHealth.OxygenAmount < 100f;
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x0600013F RID: 319 RVA: 0x00007853 File Offset: 0x00005A53
		// (set) Token: 0x06000140 RID: 320 RVA: 0x00007860 File Offset: 0x00005A60
		public bool Unkillable
		{
			get
			{
				return this.CharacterHealth.Unkillable;
			}
			set
			{
				this.CharacterHealth.Unkillable = value;
			}
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000141 RID: 321 RVA: 0x0000786E File Offset: 0x00005A6E
		// (set) Token: 0x06000142 RID: 322 RVA: 0x0000787B File Offset: 0x00005A7B
		public bool UseHealthWindow
		{
			get
			{
				return this.CharacterHealth.UseHealthWindow;
			}
			set
			{
				this.CharacterHealth.UseHealthWindow = value;
			}
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000143 RID: 323 RVA: 0x0000788C File Offset: 0x00005A8C
		public override Vector2 SimPosition
		{
			get
			{
				AnimController animController = this.AnimController;
				if (((animController != null) ? animController.Collider : null) == null)
				{
					if (!this.accessRemovedCharacterErrorShown)
					{
						string errorMsg = string.Concat(new string[]
						{
							"Attempted to access a potentially removed character. Character: [name], id: ",
							this.ID.ToString(),
							", removed: ",
							base.Removed.ToString(),
							"."
						});
						if (this.AnimController == null)
						{
							errorMsg += " AnimController == null";
						}
						else if (this.AnimController.Collider == null)
						{
							errorMsg += " AnimController.Collider == null";
						}
						errorMsg = errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace();
						DebugConsole.NewMessage(errorMsg.Replace("[name]", this.Name), new Color?(Color.Red), false);
						GameAnalyticsManager.AddErrorEventOnce("Character.SimPosition:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", this.SpeciesName.Value) + "\n" + Environment.StackTrace.CleanupStackTrace());
						this.accessRemovedCharacterErrorShown = true;
					}
					return Vector2.Zero;
				}
				return this.AnimController.Collider.SimPosition;
			}
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000144 RID: 324 RVA: 0x000079B9 File Offset: 0x00005BB9
		public override Vector2 Position
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.SimPosition);
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000145 RID: 325 RVA: 0x000079C6 File Offset: 0x00005BC6
		public override Vector2 DrawPosition
		{
			get
			{
				if (this.AnimController.MainLimb == null)
				{
					return Vector2.Zero;
				}
				return this.AnimController.MainLimb.body.DrawPosition;
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000146 RID: 326 RVA: 0x000079F0 File Offset: 0x00005BF0
		public bool IsInFriendlySub
		{
			get
			{
				return base.Submarine != null && base.Submarine.TeamID == this.TeamID;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000147 RID: 327 RVA: 0x00007A0F File Offset: 0x00005C0F
		public bool IsInPlayerSub
		{
			get
			{
				return base.Submarine != null && base.Submarine.Info.IsPlayer;
			}
		}

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000148 RID: 328 RVA: 0x00007A2B File Offset: 0x00005C2B
		public bool InPlayerSubmarine
		{
			get
			{
				return this.IsInPlayerSub;
			}
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000149 RID: 329 RVA: 0x00007A33 File Offset: 0x00005C33
		// (set) Token: 0x0600014A RID: 330 RVA: 0x00007A40 File Offset: 0x00005C40
		public float AITurretPriority
		{
			get
			{
				return this.Params.AITurretPriority;
			}
			private set
			{
				this.Params.AITurretPriority = value;
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00007A50 File Offset: 0x00005C50
		public static Character Create(CharacterInfo characterInfo, Vector2 position, string seed, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, RagdollParams ragdoll = null, bool spawnInitialItems = true)
		{
			return Character.Create(characterInfo.SpeciesName, position, seed, characterInfo, id, isRemotePlayer, hasAi, true, ragdoll, spawnInitialItems, true);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00007A78 File Offset: 0x00005C78
		public static Character Create(string speciesName, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, bool createNetworkEvent = true, RagdollParams ragdoll = null, bool throwErrorIfNotFound = true, bool spawnInitialItems = true)
		{
			if (speciesName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
			{
				speciesName = Path.GetFileNameWithoutExtension(speciesName);
			}
			return Character.Create(speciesName.ToIdentifier(), position, seed, characterInfo, id, isRemotePlayer, hasAi, createNetworkEvent, ragdoll, throwErrorIfNotFound, spawnInitialItems);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00007AB8 File Offset: 0x00005CB8
		public static Character Create(Identifier speciesName, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, bool createNetworkEvent = true, RagdollParams ragdoll = null, bool throwErrorIfNotFound = true, bool spawnInitialItems = true)
		{
			CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(speciesName);
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to create character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
				defaultInterpolatedStringHandler.AppendLiteral("\". Matching prefab not found.\n");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace;
				if (throwErrorIfNotFound)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				else
				{
					DebugConsole.AddWarning(errorMsg, null);
				}
				return null;
			}
			return Character.Create(prefab, position, seed, characterInfo, id, isRemotePlayer, hasAi, createNetworkEvent, ragdoll, spawnInitialItems);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00007B3C File Offset: 0x00005D3C
		public static Character Create(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, bool hasAi = true, bool createNetworkEvent = true, RagdollParams ragdoll = null, bool spawnInitialItems = true)
		{
			Character newCharacter;
			if (prefab.Identifier != CharacterPrefab.HumanSpeciesName || hasAi)
			{
				AICharacter aiCharacter = new AICharacter(prefab, position, seed, characterInfo, id, isRemotePlayer, ragdoll, spawnInitialItems);
				AIController ai = (prefab.Identifier == CharacterPrefab.HumanSpeciesName || aiCharacter.Params.UseHumanAI) ? new HumanAIController(aiCharacter) : new EnemyAIController(aiCharacter, seed);
				aiCharacter.SetAI(ai);
				newCharacter = aiCharacter;
			}
			else
			{
				newCharacter = new Character(prefab, position, seed, characterInfo, id, isRemotePlayer, ragdoll, spawnInitialItems);
			}
			if (GameMain.Server != null && Entity.Spawner != null && createNetworkEvent)
			{
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(newCharacter));
			}
			return newCharacter;
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00007BE8 File Offset: 0x00005DE8
		protected Character(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isRemotePlayer = false, RagdollParams ragdollParams = null, bool spawnInitialItems = true) : base(null, id)
		{
			this.wallet = new Wallet(Option<Character>.Some(this));
			GameSession gameSession = GameMain.GameSession;
			Wallet wallet;
			if (gameSession == null)
			{
				wallet = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				wallet = ((campaign != null) ? campaign.Bank : null);
			}
			Wallet bank = wallet;
			if (bank != null)
			{
				this.wallet.SetRewardDistribution(bank.RewardDistribution);
			}
			this.Seed = seed;
			this.Prefab = prefab;
			MTRandom random = new MTRandom(ToolBox.StringToInt(seed));
			this.IsRemotePlayer = isRemotePlayer;
			this.oxygenAvailable = 100f;
			this.aiTarget = new AITarget(this);
			this.lowPassMultiplier = 1f;
			this.Properties = SerializableProperty.GetProperties(this);
			this.Params = new CharacterParams(prefab.ContentFile as CharacterFile);
			this.Info = characterInfo;
			Identifier speciesName = prefab.Identifier;
			Identifier npcIdentifier = this.VariantOf;
			if (npcIdentifier == CharacterPrefab.HumanSpeciesName || speciesName == CharacterPrefab.HumanSpeciesName)
			{
				npcIdentifier = this.VariantOf;
				if (!npcIdentifier.IsEmpty)
				{
					DebugConsole.ThrowError("The variant system does not yet support humans, sorry. It does support other humanoids though!", null, this.Prefab.ContentPackage, false, false);
				}
				if (characterInfo == null)
				{
					Identifier humanSpeciesName = CharacterPrefab.HumanSpeciesName;
					string name2 = "";
					string originalName = "";
					Either<Job, JobPrefab> jobOrJobPrefab = null;
					int variant = 0;
					Rand.RandSync randSync = Rand.RandSync.Unsynced;
					npcIdentifier = default(Identifier);
					this.Info = new CharacterInfo(humanSpeciesName, name2, originalName, jobOrJobPrefab, variant, randSync, npcIdentifier);
				}
			}
			if (this.Info != null)
			{
				this.teamID = this.Info.TeamID;
				this.Info.IsNewHire = false;
			}
			ValueTuple<Identifier, Identifier>? valueTuple = (characterInfo != null) ? new ValueTuple<Identifier, Identifier>?(characterInfo.HumanPrefabIds) : null;
			if (valueTuple != null)
			{
				ValueTuple<Identifier, Identifier> valueOrDefault = valueTuple.GetValueOrDefault();
				npcIdentifier = valueOrDefault.Item1;
				if (!npcIdentifier.IsEmpty)
				{
					Identifier npcIdentifier2 = valueOrDefault.Item2;
					if (!npcIdentifier2.IsEmpty)
					{
						this.HumanPrefab = characterInfo.HumanPrefab;
					}
				}
			}
			this.keys = new Key[Enum.GetNames(typeof(InputType)).Length];
			for (int i = 0; i < Enum.GetNames(typeof(InputType)).Length; i++)
			{
				this.keys[i] = new Key((InputType)i);
			}
			ContentXElement mainElement = prefab.ConfigElement;
			List<ContentXElement> inventoryElements = new List<ContentXElement>();
			List<float> inventoryCommonness = new List<float>();
			List<ContentXElement> healthElements = new List<ContentXElement>();
			List<float> healthCommonness = new List<float>();
			foreach (ContentXElement subElement in mainElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "inventory"))
				{
					if (!(a == "health"))
					{
						if (a == "statuseffect")
						{
							StatusEffect statusEffect = StatusEffect.Load(subElement, this.Name);
							if (statusEffect != null)
							{
								if (!this.statusEffects.ContainsKey(statusEffect.type))
								{
									this.statusEffects.Add(statusEffect.type, new List<StatusEffect>());
								}
								this.statusEffects[statusEffect.type].Add(statusEffect);
							}
						}
					}
					else
					{
						healthElements.Add(subElement);
						healthCommonness.Add(subElement.GetAttributeFloat("commonness", 1f));
					}
				}
				else
				{
					inventoryElements.Add(subElement);
					inventoryCommonness.Add(subElement.GetAttributeFloat("commonness", 1f));
				}
			}
			if (this.Params.VariantFile != null)
			{
				ContentXElement paramsMainElement = this.Params.MainElement;
				if (paramsMainElement != null)
				{
					ContentXElement overrideElement = this.Params.VariantFile.GetRootExcludingOverride().FromPackage(paramsMainElement.ContentPackage);
					ContentXElement childElement = overrideElement.GetChildElement("inventory");
					ContentXElement contentXElement = null;
					if (childElement != contentXElement)
					{
						inventoryElements.Clear();
						inventoryCommonness.Clear();
						foreach (ContentXElement subElement2 in overrideElement.GetChildElements("inventory"))
						{
							string a2 = subElement2.Name.ToString().ToLowerInvariant();
							if (a2 == "inventory")
							{
								inventoryElements.Add(subElement2);
								inventoryCommonness.Add(subElement2.GetAttributeFloat("commonness", 1f));
							}
						}
					}
					childElement = overrideElement.GetChildElement("health");
					contentXElement = null;
					if (childElement != contentXElement)
					{
						healthElements.Clear();
						healthCommonness.Clear();
						foreach (ContentXElement subElement3 in overrideElement.GetChildElements("health"))
						{
							healthElements.Add(subElement3);
							healthCommonness.Add(subElement3.GetAttributeFloat("commonness", 1f));
						}
					}
				}
			}
			if (inventoryElements.Count > 0)
			{
				this.Inventory = new CharacterInventory((inventoryElements.Count == 1) ? inventoryElements[0] : ToolBox.SelectWeightedRandom<ContentXElement>(inventoryElements, inventoryCommonness, random), this, spawnInitialItems);
			}
			if (healthElements.Count == 0)
			{
				this.CharacterHealth = new CharacterHealth(this);
			}
			else
			{
				ContentXElement selectedHealthElement = (healthElements.Count == 1) ? healthElements[0] : ToolBox.SelectWeightedRandom<ContentXElement>(healthElements, healthCommonness, random);
				ContentXElement limbHealthElement = selectedHealthElement;
				if (this.Params.VariantFile != null)
				{
					ContentXElement childElement = limbHealthElement.GetChildElement("limb");
					ContentXElement contentXElement = null;
					if (childElement == contentXElement)
					{
						limbHealthElement = this.Params.OriginalElement.GetChildElement("health");
					}
				}
				this.CharacterHealth = new CharacterHealth(selectedHealthElement, this, limbHealthElement);
			}
			if (this.Params.Husk)
			{
				Identifier nonHuskedSpeciesName = this.Params.NonHuskedSpecies;
				if (!nonHuskedSpeciesName.IsEmpty || this.Params.UseHuskAppendage)
				{
					AfflictionPrefab matchingAffliction = null;
					foreach (AfflictionPrefabHusk huskPrefab in AfflictionPrefab.Prefabs.OfType<AfflictionPrefabHusk>())
					{
						if (!huskPrefab.HuskedSpeciesName.IsEmpty)
						{
							Identifier nonHuskedSpecies = nonHuskedSpeciesName;
							if (nonHuskedSpeciesName.IsEmpty)
							{
								nonHuskedSpecies = AfflictionHusk.GetNonHuskedSpeciesName(this.Params, huskPrefab);
							}
							if (huskPrefab.TargetSpecies.Contains(nonHuskedSpecies))
							{
								nonHuskedSpeciesName = nonHuskedSpecies;
								matchingAffliction = huskPrefab;
								break;
							}
						}
					}
					if (matchingAffliction == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(339, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Cannot find a husk infection that matches ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
						defaultInterpolatedStringHandler.AppendLiteral("! Please make sure that the speciesname is added as 'targets' in the husk affliction prefab definition! ");
						defaultInterpolatedStringHandler.AppendLiteral("If the name of the character doesn't match the default pattern ('Crawlerhusk', 'Humanhusk', etc), you'll also need to define the non-husked species with ");
						defaultInterpolatedStringHandler.AppendFormatted("NonHuskedSpecies");
						defaultInterpolatedStringHandler.AppendLiteral(" attribute in the character config file.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
						nonHuskedSpeciesName = (this.IsHumanoid ? CharacterPrefab.HumanSpeciesName : "crawler".ToIdentifier());
						speciesName = nonHuskedSpeciesName;
					}
				}
				if (ragdollParams == null)
				{
					Identifier npcIdentifier2 = prefab.VariantOf;
					if (npcIdentifier2 == null)
					{
						Identifier name = this.Params.UseHuskAppendage ? nonHuskedSpeciesName : speciesName;
						ragdollParams = (this.IsHumanoid ? RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(name, this.Params, this.Prefab.ContentPackage) : RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(name, this.Params, this.Prefab.ContentPackage));
					}
				}
				if (this.Params.HasInfo && this.info == null)
				{
					Identifier speciesName2 = nonHuskedSpeciesName;
					string name3 = "";
					string originalName2 = "";
					Either<Job, JobPrefab> jobOrJobPrefab2 = null;
					int variant2 = 0;
					Rand.RandSync randSync2 = Rand.RandSync.Unsynced;
					Identifier npcIdentifier2 = default(Identifier);
					this.info = new CharacterInfo(speciesName2, name3, originalName2, jobOrJobPrefab2, variant2, randSync2, npcIdentifier2);
				}
			}
			else if (this.Params.HasInfo && this.info == null)
			{
				Identifier speciesName3 = speciesName;
				string name4 = "";
				string originalName3 = "";
				Either<Job, JobPrefab> jobOrJobPrefab3 = null;
				int variant3 = 0;
				Rand.RandSync randSync3 = Rand.RandSync.Unsynced;
				Identifier npcIdentifier2 = default(Identifier);
				this.info = new CharacterInfo(speciesName3, name4, originalName3, jobOrJobPrefab3, variant3, randSync3, npcIdentifier2);
			}
			if (this.IsHumanoid)
			{
				this.AnimController = new HumanoidAnimController(this, seed, ragdollParams as HumanRagdollParams)
				{
					TargetDir = Direction.Right
				};
			}
			else
			{
				this.AnimController = new FishAnimController(this, seed, ragdollParams as FishRagdollParams);
				this.PressureProtection = 2.1474836E+09f;
			}
			this.CharacterHealth.CheckForErrors();
			this.AnimController.SetPosition(ConvertUnits.ToSimUnits(position), false, true, false, true);
			this.AnimController.FindHull(null, true, true);
			if (this.AnimController.CurrentHull != null)
			{
				base.Submarine = this.AnimController.CurrentHull.Submarine;
			}
			this.IsContainable = prefab.ConfigElement.GetAttributeBool("IsContainable", this.Mass < 35f);
			Character.CharacterList.Add(this);
			this.Enabled = (GameMain.NetworkMember == null);
			if (this.info != null)
			{
				this.LoadHeadAttachments();
			}
			this.ApplyStatusEffects(ActionType.OnSpawn, 1f);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00008634 File Offset: 0x00006834
		public void ReloadHead(int? headId = null, int hairIndex = -1, int beardIndex = -1, int moustacheIndex = -1, int faceAttachmentIndex = -1)
		{
			if (this.Info == null)
			{
				return;
			}
			if (this.AnimController.GetLimb(LimbType.Head, true, false, false) == null)
			{
				return;
			}
			HashSet<Identifier> tags = this.Info.Head.Preset.TagSet.ToHashSet<Identifier>();
			if (headId != null)
			{
				tags.RemoveWhere((Identifier t) => t.StartsWith("variant"));
				HashSet<Identifier> hashSet = tags;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("variant");
				defaultInterpolatedStringHandler.AppendFormatted<int>(headId.Value);
				hashSet.Add(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			}
			CharacterInfo.HeadInfo oldHeadInfo = this.Info.Head;
			this.Info.RecreateHead(tags.ToImmutableHashSet<Identifier>(), hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex);
			if (hairIndex == -1)
			{
				this.Info.Head.HairIndex = oldHeadInfo.HairIndex;
			}
			if (beardIndex == -1)
			{
				this.Info.Head.BeardIndex = oldHeadInfo.BeardIndex;
			}
			if (moustacheIndex == -1)
			{
				this.Info.Head.MoustacheIndex = oldHeadInfo.MoustacheIndex;
			}
			if (faceAttachmentIndex == -1)
			{
				this.Info.Head.FaceAttachmentIndex = oldHeadInfo.FaceAttachmentIndex;
			}
			this.Info.Head.SkinColor = oldHeadInfo.SkinColor;
			this.Info.Head.HairColor = oldHeadInfo.HairColor;
			this.Info.Head.FacialHairColor = oldHeadInfo.FacialHairColor;
			this.Info.CheckColors();
			this.LoadHeadAttachments();
		}

		// Token: 0x06000151 RID: 337 RVA: 0x000087C0 File Offset: 0x000069C0
		public void LoadHeadAttachments()
		{
			if (this.Info == null)
			{
				return;
			}
			if (this.AnimController == null)
			{
				return;
			}
			Limb head = this.AnimController.GetLimb(LimbType.Head, true, false, false);
			if (head == null)
			{
				return;
			}
			head.OtherWearables.ForEach(delegate(WearableSprite w)
			{
				Sprite sprite = w.Sprite;
				if (sprite == null)
				{
					return;
				}
				sprite.Remove();
			});
			head.OtherWearables.Clear();
			ContentXElement contentXElement = this.info.Head.FaceAttachment;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.FaceAttachmentIndex = 0;
			}
			ContentXElement faceAttachment = this.Info.Head.FaceAttachment;
			if (faceAttachment != null)
			{
				faceAttachment.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.FaceAttachment));
				});
			}
			contentXElement = this.info.Head.BeardElement;
			contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.BeardIndex = 0;
			}
			ContentXElement beardElement = this.Info.Head.BeardElement;
			if (beardElement != null)
			{
				beardElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.Beard));
				});
			}
			contentXElement = this.info.Head.MoustacheElement;
			contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.MoustacheIndex = 0;
			}
			ContentXElement moustacheElement = this.Info.Head.MoustacheElement;
			if (moustacheElement != null)
			{
				moustacheElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
				{
					head.OtherWearables.Add(new WearableSprite(s, WearableType.Moustache));
				});
			}
			contentXElement = this.info.Head.HairElement;
			contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				this.info.Head.HairIndex = 0;
			}
			ContentXElement hairElement = this.Info.Head.HairElement;
			if (hairElement == null)
			{
				return;
			}
			hairElement.GetChildElements("sprite").ForEach(delegate(ContentXElement s)
			{
				head.OtherWearables.Add(new WearableSprite(s, WearableType.Hair));
			});
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000089C4 File Offset: 0x00006BC4
		public bool IsKeyHit(InputType inputType)
		{
			if (GameMain.Server != null && this.IsRemotePlayer)
			{
				switch (inputType)
				{
				case InputType.Select:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Select);
				case InputType.Use:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Use) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Use);
				case InputType.Aim:
				case InputType.Attack:
				case InputType.ToggleRun:
				case InputType.InfoTab:
				case InputType.Chat:
				case InputType.RadioChat:
				case InputType.CrewOrders:
					break;
				case InputType.Up:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Up) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Up);
				case InputType.Down:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Down) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Down);
				case InputType.Left:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Left) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Left);
				case InputType.Right:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Right) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Right);
				case InputType.Run:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Run) && this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Run);
				case InputType.Crouch:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Crouch) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Crouch);
				case InputType.Ragdoll:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Ragdoll) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Ragdoll);
				case InputType.Health:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Health);
				case InputType.Grab:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Grab);
				default:
					if (inputType == InputType.Deselect)
					{
						return this.dequeuedInput.HasFlag(Character.InputNetFlags.Deselect);
					}
					if (inputType == InputType.Shoot)
					{
						return this.dequeuedInput.HasFlag(Character.InputNetFlags.Shoot) && !this.prevDequeuedInput.HasFlag(Character.InputNetFlags.Shoot);
					}
					break;
				}
				return false;
			}
			return this.keys[(int)inputType].Hit;
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00008CB4 File Offset: 0x00006EB4
		public bool IsKeyDown(InputType inputType)
		{
			if (GameMain.Server != null && this.IsRemotePlayer)
			{
				switch (inputType)
				{
				case InputType.Select:
					return false;
				case InputType.Use:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Use);
				case InputType.Aim:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Aim);
				case InputType.Up:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Up);
				case InputType.Down:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Down);
				case InputType.Left:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Left);
				case InputType.Right:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Right);
				case InputType.Attack:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Attack);
				case InputType.Run:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Run);
				case InputType.ToggleRun:
				case InputType.InfoTab:
				case InputType.Chat:
				case InputType.RadioChat:
				case InputType.CrewOrders:
					break;
				case InputType.Crouch:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Crouch);
				case InputType.Ragdoll:
					return this.dequeuedInput.HasFlag(Character.InputNetFlags.Ragdoll);
				default:
					if (inputType == InputType.Deselect)
					{
						return false;
					}
					if (inputType == InputType.Shoot)
					{
						return this.dequeuedInput.HasFlag(Character.InputNetFlags.Shoot);
					}
					break;
				}
				return false;
			}
			if (inputType == InputType.Up || inputType == InputType.Down || inputType == InputType.Left || inputType == InputType.Right)
			{
				Affliction invertControls = this.CharacterHealth.GetAfflictionOfType("invertcontrols".ToIdentifier(), true);
				if (invertControls != null)
				{
					switch (inputType)
					{
					case InputType.Up:
						inputType = InputType.Down;
						break;
					case InputType.Down:
						inputType = InputType.Up;
						break;
					case InputType.Left:
						inputType = InputType.Right;
						break;
					case InputType.Right:
						inputType = InputType.Left;
						break;
					}
				}
			}
			return (this == Character.Controlled && inputType == InputType.Run && this.ToggleRun) || this.keys[(int)inputType].Held;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00008EC3 File Offset: 0x000070C3
		public void SetInput(InputType inputType, bool hit, bool held)
		{
			this.keys[(int)inputType].Hit = hit;
			this.keys[(int)inputType].Held = held;
			this.keys[(int)inputType].SetState(hit, held);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x00008EF0 File Offset: 0x000070F0
		public void ClearInput(InputType inputType)
		{
			this.keys[(int)inputType].Hit = false;
			this.keys[(int)inputType].Held = false;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00008F10 File Offset: 0x00007110
		public void ClearInputs()
		{
			if (this.keys == null)
			{
				return;
			}
			foreach (Key key in this.keys)
			{
				key.Hit = false;
				key.Held = false;
			}
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00008F50 File Offset: 0x00007150
		public override string ToString()
		{
			return this.SpeciesName.Value;
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00008F6C File Offset: 0x0000716C
		public void GiveJobItems(bool isPvPMode, WayPoint spawnPoint = null)
		{
			if (this.info == null)
			{
				return;
			}
			ValueTuple<Identifier, Identifier> humanPrefabIds = this.info.HumanPrefabIds;
			Identifier identifier = default(Identifier);
			if (!(humanPrefabIds.Item1 != identifier))
			{
				Identifier identifier2 = default(Identifier);
				if (!(humanPrefabIds.Item2 != identifier2))
				{
					goto IL_102;
				}
			}
			HumanPrefab prefab = this.info.HumanPrefab;
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to give job items for the character \"");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" - could not find human prefab with the id \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.info.HumanPrefabIds.Item2);
				defaultInterpolatedStringHandler.AppendLiteral("\" from \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.info.HumanPrefabIds.Item1);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			else if (prefab.GiveItems(this, ((spawnPoint != null) ? spawnPoint.Submarine : null) ?? base.Submarine, spawnPoint, Rand.RandSync.Unsynced, true))
			{
				return;
			}
			IL_102:
			Job job = this.info.Job;
			if (job == null)
			{
				return;
			}
			job.GiveJobItems(this, isPvPMode, spawnPoint);
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00009094 File Offset: 0x00007294
		public void GiveIdCardTags(WayPoint spawnPoint, bool createNetworkEvent = false)
		{
			CharacterInfo characterInfo = this.info;
			if (((characterInfo != null) ? characterInfo.Job : null) == null || spawnPoint == null)
			{
				return;
			}
			foreach (Item item in this.Inventory.AllItems)
			{
				IdCard idCard = (item != null) ? item.GetComponent<IdCard>() : null;
				if (idCard != null && !(idCard.OwnerName != this.info.Name))
				{
					foreach (string s in spawnPoint.IdCardTags)
					{
						item.AddTag(s);
					}
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
					{
						Item item2 = item;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
						defaultInterpolatedStringHandler.AppendLiteral("id_");
						defaultInterpolatedStringHandler.AppendFormatted<CharacterTeamType>(this.TeamID);
						item2.AddTag(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
					}
					if (createNetworkEvent)
					{
						NetworkMember networkMember = GameMain.NetworkMember;
						if (networkMember != null && networkMember.IsServer)
						{
							GameMain.NetworkMember.CreateEntityEvent(item, new Item.ChangePropertyEventData(item.SerializableProperties["Tags".ToIdentifier()], item));
						}
					}
				}
			}
		}

		// Token: 0x0600015A RID: 346 RVA: 0x000091E0 File Offset: 0x000073E0
		public float GetSkillLevel(Identifier skillIdentifier)
		{
			CharacterInfo characterInfo = this.Info;
			if (((characterInfo != null) ? characterInfo.Job : null) == null)
			{
				return 0f;
			}
			float skillLevel = this.Info.Job.GetSkillLevel(skillIdentifier);
			StatTypes statType;
			if (Character.overrideStatTypes.TryGetValue(skillIdentifier, out statType))
			{
				float skillOverride = this.GetStatValue(statType, true);
				if (skillOverride > skillLevel)
				{
					skillLevel = skillOverride;
				}
			}
			foreach (Affliction affliction in this.CharacterHealth.GetAllAfflictions())
			{
				skillLevel *= affliction.GetSkillMultiplier();
			}
			float skillValue;
			if (skillIdentifier != null && this.wearableSkillModifiers.TryGetValue(skillIdentifier, out skillValue))
			{
				skillLevel += skillValue;
			}
			StatTypes skillStatType = Character.GetSkillStatType(skillIdentifier);
			if (skillStatType != StatTypes.None)
			{
				skillLevel += this.GetStatValue(skillStatType, true);
			}
			return Math.Max(skillLevel, 0f);
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x0600015B RID: 347 RVA: 0x000092C4 File Offset: 0x000074C4
		// (set) Token: 0x0600015C RID: 348 RVA: 0x000092CC File Offset: 0x000074CC
		public Vector2? OverrideMovement { get; set; }

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x0600015D RID: 349 RVA: 0x000092D5 File Offset: 0x000074D5
		// (set) Token: 0x0600015E RID: 350 RVA: 0x000092DD File Offset: 0x000074DD
		public bool ForceRun { get; set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x0600015F RID: 351 RVA: 0x000092E6 File Offset: 0x000074E6
		public bool IsClimbing
		{
			get
			{
				return this.AnimController.IsClimbing;
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x000092F4 File Offset: 0x000074F4
		public Vector2 GetTargetMovement()
		{
			Vector2 targetMovement = Vector2.Zero;
			if (this.OverrideMovement != null)
			{
				targetMovement = this.OverrideMovement.Value;
			}
			else
			{
				if (this.IsKeyDown(InputType.Left))
				{
					targetMovement.X -= 1f;
				}
				if (this.IsKeyDown(InputType.Right))
				{
					targetMovement.X += 1f;
				}
				if (this.IsKeyDown(InputType.Up))
				{
					targetMovement.Y += 1f;
				}
				if (this.IsKeyDown(InputType.Down))
				{
					targetMovement.Y -= 1f;
				}
			}
			bool run = false;
			if ((this.IsKeyDown(InputType.Run) && this.AnimController.ForceSelectAnimationType == AnimationType.NotDefined) || this.ForceRun)
			{
				run = this.CanRun;
			}
			return this.ApplyMovementLimits(targetMovement, this.AnimController.GetCurrentSpeed(run));
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000161 RID: 353 RVA: 0x000093C8 File Offset: 0x000075C8
		public bool CanRun
		{
			get
			{
				if (!this.DisableRunning && this.CanRunWhileDragging())
				{
					HumanoidAnimController humanoidAnimController = this.AnimController as HumanoidAnimController;
					if ((humanoidAnimController == null || !humanoidAnimController.Crouching) && !this.AnimController.IsMovingBackwards && !this.HasAbilityFlag(AbilityFlags.MustWalk))
					{
						return !this.AnimController.IsHoldingToRope;
					}
				}
				return false;
			}
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000162 RID: 354 RVA: 0x00009422 File Offset: 0x00007622
		// (set) Token: 0x06000163 RID: 355 RVA: 0x0000943B File Offset: 0x0000763B
		public bool DisableRunning
		{
			get
			{
				return this.disableRunningLastSet > Timing.TotalTime - 0.1;
			}
			set
			{
				if (value)
				{
					this.disableRunningLastSet = Timing.TotalTime;
				}
			}
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000944C File Offset: 0x0000764C
		public bool CanRunWhileDragging()
		{
			Character character = this.selectedCharacter;
			return character == null || !character.IsDraggable || ((this.selectedCharacter.IsIncapacitated || this.selectedCharacter.Stun > 0f) && this.HasAbilityFlag(AbilityFlags.MoveNormallyWhileDragging));
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00009498 File Offset: 0x00007698
		public Vector2 ApplyMovementLimits(Vector2 targetMovement, float currentSpeed)
		{
			if (this.AnimController.InWater)
			{
				float length = targetMovement.Length();
				if (length > 0f)
				{
					targetMovement /= length;
				}
			}
			targetMovement *= currentSpeed;
			float maxSpeed = this.ApplyTemporarySpeedLimits(currentSpeed);
			targetMovement.X = MathHelper.Clamp(targetMovement.X, -maxSpeed, maxSpeed);
			targetMovement.Y = MathHelper.Clamp(targetMovement.Y, -maxSpeed, maxSpeed);
			this.SpeedMultiplier = Math.Max(0f, this.greatestPositiveSpeedMultiplier - (1f - this.greatestNegativeSpeedMultiplier));
			targetMovement *= this.SpeedMultiplier;
			this.ResetSpeedMultiplier();
			return targetMovement;
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000166 RID: 358 RVA: 0x0000953E File Offset: 0x0000773E
		// (set) Token: 0x06000167 RID: 359 RVA: 0x00009546 File Offset: 0x00007746
		public float SpeedMultiplier { get; private set; } = 1f;

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000168 RID: 360 RVA: 0x0000954F File Offset: 0x0000774F
		// (set) Token: 0x06000169 RID: 361 RVA: 0x00009557 File Offset: 0x00007757
		public float PropulsionSpeedMultiplier
		{
			get
			{
				return this.propulsionSpeedMultiplier;
			}
			set
			{
				this.propulsionSpeedMultiplier = value;
				this.propulsionSpeedMultiplierLastSet = Timing.TotalTime;
			}
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000956B File Offset: 0x0000776B
		public void StackSpeedMultiplier(float val)
		{
			this.greatestNegativeSpeedMultiplier = Math.Min(val, this.greatestNegativeSpeedMultiplier);
			this.greatestPositiveSpeedMultiplier = Math.Max(val, this.greatestPositiveSpeedMultiplier);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00009591 File Offset: 0x00007791
		public void ResetSpeedMultiplier()
		{
			this.greatestPositiveSpeedMultiplier = 1f;
			this.greatestNegativeSpeedMultiplier = 1f;
			if (Timing.TotalTime > this.propulsionSpeedMultiplierLastSet + 0.1)
			{
				this.propulsionSpeedMultiplier = 1f;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600016C RID: 364 RVA: 0x000095CB File Offset: 0x000077CB
		// (set) Token: 0x0600016D RID: 365 RVA: 0x000095D3 File Offset: 0x000077D3
		public float HealthMultiplier { get; private set; } = 1f;

		// Token: 0x0600016E RID: 366 RVA: 0x000095DC File Offset: 0x000077DC
		public void StackHealthMultiplier(float val)
		{
			this.greatestNegativeHealthMultiplier = Math.Min(val, this.greatestNegativeHealthMultiplier);
			this.greatestPositiveHealthMultiplier = Math.Max(val, this.greatestPositiveHealthMultiplier);
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00009602 File Offset: 0x00007802
		private void CalculateHealthMultiplier()
		{
			this.HealthMultiplier = this.greatestPositiveHealthMultiplier - (1f - this.greatestNegativeHealthMultiplier);
			this.greatestPositiveHealthMultiplier = 1f;
			this.greatestNegativeHealthMultiplier = 1f;
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000170 RID: 368 RVA: 0x00009633 File Offset: 0x00007833
		// (set) Token: 0x06000171 RID: 369 RVA: 0x0000963B File Offset: 0x0000783B
		public float HumanPrefabHealthMultiplier { get; private set; } = 1f;

		// Token: 0x06000172 RID: 370 RVA: 0x00009644 File Offset: 0x00007844
		public float GetTemporarySpeedReduction()
		{
			if (!this.Params.Health.ApplyMovementPenalties)
			{
				return 0f;
			}
			float reduction = 0f;
			reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.RightFoot, false, false, false), reduction, 0.8f);
			reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.LeftFoot, false, false, false), reduction, 0.8f);
			if (this.AnimController is HumanoidAnimController)
			{
				if (this.AnimController.InWater)
				{
					reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.RightHand, false, false, false), reduction, 0.8f);
					reduction = this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.LeftHand, false, false, false), reduction, 0.8f);
				}
			}
			else
			{
				int totalTailLimbs = 0;
				int destroyedTailLimbs = 0;
				foreach (Limb limb in this.AnimController.Limbs)
				{
					if (limb.type == LimbType.Tail)
					{
						totalTailLimbs++;
						if (limb.IsSevered)
						{
							destroyedTailLimbs++;
						}
					}
				}
				if (destroyedTailLimbs > 0)
				{
					reduction += MathHelper.Lerp(0f, this.AnimController.InWater ? 1f : 0.5f, (float)destroyedTailLimbs / (float)totalTailLimbs);
				}
			}
			return Math.Clamp(reduction, 0f, 1f);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00009784 File Offset: 0x00007984
		private float CalculateMovementPenalty(Limb limb, float sum, float max = 0.8f)
		{
			if (!this.Params.Health.ApplyMovementPenalties)
			{
				return 0f;
			}
			if (limb != null)
			{
				sum += MathHelper.Lerp(0f, max, this.CharacterHealth.GetLimbDamage(limb, AfflictionPrefab.DamageType));
			}
			return Math.Clamp(sum, 0f, 1f);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x000097DC File Offset: 0x000079DC
		public float GetRightHandPenalty()
		{
			return this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.RightHand, false, false, false), 0f, 1f);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000097FD File Offset: 0x000079FD
		public float GetLeftHandPenalty()
		{
			return this.CalculateMovementPenalty(this.AnimController.GetLimb(LimbType.LeftHand, false, false, false), 0f, 1f);
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00009820 File Offset: 0x00007A20
		public float GetLegPenalty(float startSum = 0f)
		{
			float sum = startSum;
			foreach (Limb limb in this.AnimController.Limbs)
			{
				LimbType type = limb.type;
				if (type - LimbType.LeftFoot <= 1)
				{
					sum += this.CalculateMovementPenalty(limb, sum, 0.5f);
				}
			}
			return Math.Clamp(sum, 0f, 1f);
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00009880 File Offset: 0x00007A80
		public float ApplyTemporarySpeedLimits(float speed)
		{
			float max;
			if (this.AnimController is HumanoidAnimController)
			{
				max = (this.AnimController.InWater ? 0.5f : 0.8f);
			}
			else
			{
				max = (this.AnimController.InWater ? 0.9f : 0.5f);
			}
			speed *= 1f - MathHelper.Lerp(0f, max, this.GetTemporarySpeedReduction());
			return speed;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x000098EC File Offset: 0x00007AEC
		public void Control(float deltaTime, Camera cam)
		{
			this.ViewTarget = null;
			if (!this.AllowInput)
			{
				return;
			}
			if (Character.Controlled == this || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer))
			{
				this.SmoothedCursorPosition = this.cursorPosition;
			}
			else
			{
				Vector2 smoothedCursorDiff = this.cursorPosition - this.SmoothedCursorPosition;
				smoothedCursorDiff = NetConfig.InterpolateCursorPositionError(smoothedCursorDiff);
				this.SmoothedCursorPosition = this.cursorPosition - smoothedCursorDiff;
			}
			bool aiControlled = this is AICharacter && Character.Controlled != this && !this.IsRemotePlayer;
			NetworkMember networkMember = GameMain.NetworkMember;
			bool controlledByServer = networkMember != null && networkMember.IsClient && this.IsRemotelyControlled;
			if (!aiControlled && !controlledByServer)
			{
				Vector2 targetMovement = this.GetTargetMovement();
				this.AnimController.TargetMovement = targetMovement;
				Item selectedItem = this.SelectedItem;
				Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
				if (controller == null || !controller.ControlCharacterPose)
				{
					this.AnimController.IgnorePlatforms = (this.AnimController.TargetMovement.Y < -0.1f);
				}
			}
			HumanoidAnimController humanAnimController = this.AnimController as HumanoidAnimController;
			if (humanAnimController != null)
			{
				humanAnimController.Crouching = (humanAnimController.ForceSelectAnimationType == AnimationType.Crouch || this.IsKeyDown(InputType.Crouch));
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					humanAnimController.ForceSelectAnimationType = AnimationType.NotDefined;
				}
			}
			if (!aiControlled && !this.AnimController.IsUsingItem && this.AnimController.Anim != AnimController.Animation.CPR && (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient || Character.Controlled == this) && ((!this.IsClimbing && this.AnimController.OnGround) || (this.IsClimbing && this.IsKeyDown(InputType.Aim))) && !this.AnimController.InWater)
			{
				if (!this.FollowCursor)
				{
					this.AnimController.TargetDir = Direction.Right;
				}
				else if (this.AnimController is HumanoidAnimController)
				{
					if (this.CursorPosition.X < this.AnimController.Collider.Position.X - 40f)
					{
						this.AnimController.TargetDir = Direction.Left;
					}
					else if (this.CursorPosition.X > this.AnimController.Collider.Position.X + 40f)
					{
						this.AnimController.TargetDir = Direction.Right;
					}
				}
			}
			if (aiControlled && this.Stun <= 0f && !this.IsKnockedDownOrRagdolled && !this.LockHands && this.ShouldAvoidStayingAttachedToController())
			{
				this.SelectedItem = null;
			}
			if (GameMain.NetworkMember != null)
			{
				if (GameMain.NetworkMember.IsServer)
				{
					if (!aiControlled)
					{
						if (this.dequeuedInput.HasFlag(Character.InputNetFlags.FacingLeft))
						{
							this.AnimController.TargetDir = Direction.Left;
						}
						else
						{
							this.AnimController.TargetDir = Direction.Right;
						}
					}
				}
				else if (GameMain.NetworkMember.IsClient && Character.Controlled != this && this.memState.Count > 0)
				{
					this.AnimController.TargetDir = this.memState[0].Direction;
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && Character.Controlled != this && this.IsKeyDown(InputType.Aim))
			{
				Limb attackLimb2 = this.currentAttackTarget.AttackLimb;
				Attack attack = (attackLimb2 != null) ? attackLimb2.attack : null;
				if (attack != null && attack.Ranged)
				{
					EnemyAIController enemyAi = this.AIController as EnemyAIController;
					if (enemyAi != null)
					{
						enemyAi.AimRangedAttack(attack, this.currentAttackTarget.DamageTarget as Entity);
					}
				}
			}
			if (this.attackCoolDown > 0f)
			{
				this.attackCoolDown -= deltaTime;
			}
			else if (this.IsKeyDown(InputType.Attack) && !this.IsAttachedToController())
			{
				if (this.IsPlayer)
				{
					float dist = -1f;
					Vector2 attackPos = this.SimPosition + ConvertUnits.ToSimUnits(this.cursorPosition - this.Position);
					List<Body> ignoredBodies = (from l in this.AnimController.Limbs
					select l.body.FarseerBody).ToList<Body>();
					ignoredBodies.Add(this.AnimController.Collider.FarseerBody);
					Body body = Submarine.PickBody(this.SimPosition, attackPos, ignoredBodies, new Category?(Category.Cat1 | Category.Cat2), true, null, false);
					IDamageable attackTarget = null;
					if (body != null)
					{
						attackPos = Submarine.LastPickedPosition;
						Submarine sub = body.UserData as Submarine;
						if (sub != null)
						{
							body = Submarine.PickBody(this.SimPosition - ((Submarine)body.UserData).SimPosition, attackPos - ((Submarine)body.UserData).SimPosition, ignoredBodies, new Category?(Category.Cat1), true, null, false);
							if (body != null)
							{
								attackPos = Submarine.LastPickedPosition + sub.SimPosition;
								attackTarget = (body.UserData as IDamageable);
							}
						}
						else
						{
							IDamageable damageable = body.UserData as IDamageable;
							if (damageable != null)
							{
								attackTarget = damageable;
							}
							else
							{
								Limb limb = body.UserData as Limb;
								if (limb != null)
								{
									attackTarget = limb.character;
								}
							}
						}
					}
					IEnumerable<AttackContext> currentContexts = this.GetAttackContexts();
					IEnumerable<Limb> attackLimbs = from l in this.AnimController.Limbs
					where l.attack != null
					select l;
					bool hasAttacksWithoutRootForce = attackLimbs.Any((Limb l) => !l.attack.HasRootForce);
					IEnumerable<Limb> validLimbs = attackLimbs.Where(delegate(Limb l)
					{
						if (l.IsSevered || l.IsStuck)
						{
							return false;
						}
						if (l.Disabled)
						{
							return false;
						}
						Attack attack2 = l.attack;
						if (attack2.CoolDownTimer > 0f)
						{
							return false;
						}
						if (hasAttacksWithoutRootForce && attack2.HasRootForce)
						{
							return false;
						}
						if (!attack2.IsValidContext(currentContexts))
						{
							return false;
						}
						if (attackTarget != null)
						{
							if (!attack2.IsValidTarget(attackTarget as Entity))
							{
								return false;
							}
							ISerializableEntity se = attackTarget as ISerializableEntity;
							if (se != null && se is Character && attack2.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(se)))
							{
								return false;
							}
						}
						return !attack2.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(this));
					});
					IOrderedEnumerable<Limb> sortedLimbs = from l in validLimbs
					orderby Vector2.DistanceSquared(ConvertUnits.ToDisplayUnits(l.SimPosition), this.cursorPosition)
					select l;
					Limb attackLimb = sortedLimbs.FirstOrDefault<Limb>();
					if (attackLimb != null)
					{
						Character targetCharacter = attackTarget as Character;
						if (targetCharacter != null)
						{
							dist = ConvertUnits.ToDisplayUnits(Vector2.Distance(Submarine.LastPickedPosition, attackLimb.SimPosition));
							foreach (Limb limb2 in targetCharacter.AnimController.Limbs)
							{
								if (!limb2.IsSevered && !limb2.Removed)
								{
									float tempDist = ConvertUnits.ToDisplayUnits(Vector2.Distance(limb2.SimPosition, attackLimb.SimPosition));
									if (tempDist < dist)
									{
										dist = tempDist;
									}
								}
							}
						}
						AttackResult attackResult;
						attackLimb.UpdateAttack(deltaTime, attackPos, attackTarget, out attackResult, dist, null);
						if (!attackLimb.attack.IsRunning)
						{
							this.attackCoolDown = 1f;
						}
					}
				}
				else
				{
					networkMember = GameMain.NetworkMember;
					if (networkMember != null && networkMember.IsClient && Character.Controlled != this)
					{
						Entity entity = this.currentAttackTarget.DamageTarget as Entity;
						if (entity != null && entity.Removed)
						{
							this.currentAttackTarget = default(Character.AttackTargetData);
						}
						Limb attackLimb3 = this.currentAttackTarget.AttackLimb;
						if (attackLimb3 != null)
						{
							AttackResult attackResult2;
							attackLimb3.UpdateAttack(deltaTime, this.currentAttackTarget.AttackPos, this.currentAttackTarget.DamageTarget, out attackResult2, -1f, null);
						}
					}
				}
			}
			if (this.Inventory != null && Character.<Control>g__CanUseItemsWhenSelected|546_0(this.SelectedItem) && Character.<Control>g__CanUseItemsWhenSelected|546_0(this.SelectedSecondaryItem))
			{
				foreach (Item item in this.HeldItems)
				{
					this.<Control>g__tryUseItem|546_1(item, deltaTime);
				}
				foreach (Item item2 in this.Inventory.AllItems)
				{
					Wearable component = item2.GetComponent<Wearable>();
					if (component != null && component.AllowUseWhenWorn && this.HasEquippedItem(item2, null, null))
					{
						this.<Control>g__tryUseItem|546_1(item2, deltaTime);
					}
				}
			}
			if (this.SelectedItem != null)
			{
				this.<Control>g__tryUseItem|546_1(this.SelectedItem, deltaTime);
			}
			if (this.SelectedCharacter != null && (!this.SelectedCharacter.CanBeSelected || (Vector2.DistanceSquared(this.SelectedCharacter.WorldPosition, this.WorldPosition) > 40000f && this.SelectedCharacter.GetDistanceToClosestLimb(this.GetRelativeSimPosition(this.selectedCharacter, new Vector2?(this.WorldPosition))) > ConvertUnits.ToSimUnits(200f))))
			{
				this.DeselectCharacter();
			}
			if (this.IsRemotelyControlled && this.keys != null)
			{
				foreach (Key key in this.keys)
				{
					key.ResetHit();
				}
			}
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000A1AC File Offset: 0x000083AC
		public void SetAttackTarget(Limb attackLimb, IDamageable damageTarget, Vector2 attackPos)
		{
			this.currentAttackTarget = new Character.AttackTargetData
			{
				AttackLimb = attackLimb,
				DamageTarget = damageTarget,
				AttackPos = attackPos
			};
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000A1E0 File Offset: 0x000083E0
		private Limb GetSeeingLimb()
		{
			Limb result;
			if ((result = this.AnimController.GetLimb(LimbType.Head, true, false, false)) == null)
			{
				result = (this.AnimController.GetLimb(LimbType.Torso, true, false, false) ?? this.AnimController.MainLimb);
			}
			return result;
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000A218 File Offset: 0x00008418
		public bool CanSeeTarget(ISpatialEntity target, ISpatialEntity seeingEntity = null, bool seeThroughWindows = false, bool checkFacing = false)
		{
			if (seeingEntity == null)
			{
				ISpatialEntity spatialEntity;
				if (!this.AnimController.SimplePhysicsEnabled)
				{
					ISpatialEntity seeingLimb = this.GetSeeingLimb();
					spatialEntity = seeingLimb;
				}
				else
				{
					spatialEntity = this;
				}
				seeingEntity = spatialEntity;
			}
			Character targetCharacter = target as Character;
			if (targetCharacter != null)
			{
				return ISpatialEntity.IsCharacterVisible(targetCharacter, seeingEntity, seeThroughWindows, checkFacing);
			}
			return ISpatialEntity.CheckVisibility(target, seeingEntity, seeThroughWindows, checkFacing);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000A264 File Offset: 0x00008464
		public bool IsFacing(Vector2 targetWorldPos)
		{
			return (this.AnimController.Dir > 0f && targetWorldPos.X > this.WorldPosition.X) || (this.AnimController.Dir < 0f && targetWorldPos.X < this.WorldPosition.X);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0000A2BF File Offset: 0x000084BF
		public bool HasItem(Item item, bool requireEquipped = false, InvSlotType? slotType = null)
		{
			if (!requireEquipped)
			{
				return item.IsOwnedBy(this);
			}
			return this.HasEquippedItem(item, slotType, null);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000A2D8 File Offset: 0x000084D8
		public bool HasEquippedItem(Item item, InvSlotType? slotType = null, Func<InvSlotType, bool> predicate = null)
		{
			if (this.Inventory == null)
			{
				return false;
			}
			for (int i = 0; i < this.Inventory.Capacity; i++)
			{
				InvSlotType slot = this.Inventory.SlotTypes[i];
				if (predicate == null || predicate(slot))
				{
					if (slotType != null)
					{
						if (!slotType.Value.HasFlag(slot))
						{
							goto IL_61;
						}
					}
					else if (slot == InvSlotType.Any)
					{
						goto IL_61;
					}
					if (this.Inventory.GetItemAt(i) == item)
					{
						return true;
					}
				}
				IL_61:;
			}
			return false;
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000A35C File Offset: 0x0000855C
		public bool HasEquippedItem(Identifier tagOrIdentifier, bool allowBroken = true, InvSlotType? slotType = null)
		{
			if (this.Inventory == null)
			{
				return false;
			}
			int i = 0;
			while (i < this.Inventory.Capacity)
			{
				if (slotType != null)
				{
					if (slotType.Value.HasFlag(this.Inventory.SlotTypes[i]))
					{
						goto IL_51;
					}
				}
				else if (this.Inventory.SlotTypes[i] != InvSlotType.Any)
				{
					goto IL_51;
				}
				IL_90:
				i++;
				continue;
				IL_51:
				Item item = this.Inventory.GetItemAt(i);
				if (item != null && (allowBroken || item.Condition > 0f) && (item.Prefab.Identifier == tagOrIdentifier || item.HasTag(tagOrIdentifier)))
				{
					return true;
				}
				goto IL_90;
			}
			return false;
		}

		// Token: 0x06000180 RID: 384 RVA: 0x0000A410 File Offset: 0x00008610
		public Item GetEquippedItem(Identifier tagOrIdentifier = default(Identifier), InvSlotType? slotType = null)
		{
			if (this.Inventory == null)
			{
				return null;
			}
			int i = 0;
			while (i < this.Inventory.Capacity)
			{
				if (slotType != null)
				{
					if (slotType.Value.HasFlag(this.Inventory.SlotTypes[i]))
					{
						goto IL_4E;
					}
				}
				else if (this.Inventory.SlotTypes[i] != InvSlotType.Any)
				{
					goto IL_4E;
				}
				IL_86:
				i++;
				continue;
				IL_4E:
				Item item = this.Inventory.GetItemAt(i);
				if (item != null && (tagOrIdentifier.IsEmpty || item.Prefab.Identifier == tagOrIdentifier || item.HasTag(tagOrIdentifier)))
				{
					return item;
				}
				goto IL_86;
			}
			return null;
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0000A4BC File Offset: 0x000086BC
		public bool HasHandsFull([TupleElementNames(new string[]
		{
			"leftHandItem",
			"rightHandItem"
		})] out ValueTuple<Item, Item> items)
		{
			InvSlotType? slotType = new InvSlotType?(InvSlotType.LeftHand);
			Item leftHandItem = this.GetEquippedItem(default(Identifier), slotType);
			slotType = new InvSlotType?(InvSlotType.RightHand);
			Item rightHandItem = this.GetEquippedItem(default(Identifier), slotType);
			items = new ValueTuple<Item, Item>(leftHandItem, rightHandItem);
			return leftHandItem != null && rightHandItem != null;
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000A516 File Offset: 0x00008716
		public bool TryPutItem(Item item, IEnumerable<InvSlotType> allowedSlots)
		{
			return this.Inventory.TryPutItem(item, this, allowedSlots, true, false, true);
		}

		// Token: 0x06000183 RID: 387 RVA: 0x0000A529 File Offset: 0x00008729
		public bool TryPutItemInBag(Item item)
		{
			return item != null && item.AllowedSlots.Contains(InvSlotType.Bag) && this.TryPutItem(item, CharacterInventory.BagSlot);
		}

		// Token: 0x06000184 RID: 388 RVA: 0x0000A54E File Offset: 0x0000874E
		public bool TryPutItemInAnySlot(Item item)
		{
			return item != null && item.AllowedSlots.Contains(InvSlotType.Any) && this.TryPutItem(item, CharacterInventory.AnySlot);
		}

		// Token: 0x06000185 RID: 389 RVA: 0x0000A570 File Offset: 0x00008770
		public bool Unequip(Item item)
		{
			if (!this.HasEquippedItem(item, null, null))
			{
				return false;
			}
			if (!item.IsInteractable(this))
			{
				return false;
			}
			if (!this.TryPutItemInAnySlot(item) && !this.TryPutItemInBag(item))
			{
				item.Drop(this, true, true);
			}
			return true;
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000A5BC File Offset: 0x000087BC
		public bool CanAccessInventory(Inventory inventory, CharacterInventory.AccessLevel accessLevel = CharacterInventory.AccessLevel.AllowBotsAndPets)
		{
			if (!this.CanInteract || inventory.Locked)
			{
				return false;
			}
			Character inventoryOwner = inventory.Owner as Character;
			if (inventoryOwner != null)
			{
				return inventoryOwner.IsInventoryAccessibleTo(this, accessLevel) && (inventoryOwner == this || this.CanInteractWith(inventoryOwner, 200f, true, false));
			}
			Item item = inventory.Owner as Item;
			if (item != null)
			{
				if (!this.CanInteractWith(item, true))
				{
					foreach (MapEntity linkedEntity in item.linkedTo)
					{
						Item linkedItem = linkedEntity as Item;
						if (linkedItem != null && linkedItem.DisplaySideBySideWhenLinked && this.CanInteractWith(linkedItem, true))
						{
							return true;
						}
					}
					return false;
				}
				ItemInventory itemInventory = inventory as ItemInventory;
				ItemContainer container = (itemInventory != null) ? itemInventory.Container : null;
				if (container != null && !container.HasRequiredItems(this, false, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000A6B8 File Offset: 0x000088B8
		public bool CanBeHealedBy(Character character, bool checkFriendlyTeam = true)
		{
			return !character.IsClimbing && !this.DisableHealthWindow && this.UseHealthWindow && character.CanInteract && (!checkFriendlyTeam || this.IsFriendly(character) || this.CanBeDraggedBy(character)) && character.CanInteractWith(this, 160f, false, false);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000A70C File Offset: 0x0000890C
		public bool CanBeDraggedBy(Character character)
		{
			return this.IsDraggable && (this.IsKnockedDownOrRagdolled || this.LockHands || (this.IsPet && character.IsOnFriendlyTeam(this)) || (this.IsBot && character.TeamID == this.TeamID));
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000A760 File Offset: 0x00008960
		public bool IsInventoryAccessibleTo(Character character, CharacterInventory.AccessLevel accessLevel = CharacterInventory.AccessLevel.AllowBotsAndPets)
		{
			Character.<>c__DisplayClass565_0 CS$<>8__locals1;
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			if (base.Removed || this.Inventory == null)
			{
				return false;
			}
			if (!this.Inventory.AccessibleWhenAlive && !this.IsDead)
			{
				return CS$<>8__locals1.character == this && this.Inventory.AccessibleByOwner;
			}
			if (CS$<>8__locals1.character == this)
			{
				return true;
			}
			if (this.IsKnockedDownOrRagdolled || this.LockHands)
			{
				return true;
			}
			bool result;
			switch (accessLevel)
			{
			case CharacterInventory.AccessLevel.OnlyIfIncapacitated:
				result = false;
				break;
			case CharacterInventory.AccessLevel.AllowBotsAndPets:
				result = ((this.IsBot && this.<IsInventoryAccessibleTo>g__IsOnSameTeam|565_0(ref CS$<>8__locals1)) || this.<IsInventoryAccessibleTo>g__IsFriendlyPet|565_1(ref CS$<>8__locals1));
				break;
			case CharacterInventory.AccessLevel.AllowFriendly:
				result = (this.<IsInventoryAccessibleTo>g__IsOnSameTeam|565_0(ref CS$<>8__locals1) || this.<IsInventoryAccessibleTo>g__IsFriendlyPet|565_1(ref CS$<>8__locals1));
				break;
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x0600018A RID: 394 RVA: 0x0000A830 File Offset: 0x00008A30
		private Stopwatch StopWatch
		{
			get
			{
				Stopwatch result;
				if ((result = this.sw) == null)
				{
					result = (this.sw = new Stopwatch());
				}
				return result;
			}
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0000A858 File Offset: 0x00008A58
		public bool FindItem(ref int itemIndex, out Item targetItem, IEnumerable<Identifier> identifiers = null, bool ignoreBroken = true, IEnumerable<Item> ignoredItems = null, IEnumerable<Identifier> ignoredContainerIdentifiers = null, Func<Item, bool> customPredicate = null, Func<Item, float> customPriorityFunction = null, float maxItemDistance = 10000f, ISpatialEntity positionalReference = null)
		{
			if (HumanAIController.DebugAI)
			{
				this.StopWatch.Restart();
			}
			if (itemIndex == 0)
			{
				this._foundItem = null;
				this._selectedItemPriority = 0f;
			}
			int itemsPerFrame = this.IsOnPlayerTeam ? 100 : 10;
			int checkedItemCount = 0;
			int i = 0;
			while (i < itemsPerFrame && itemIndex < Item.ItemList.Count)
			{
				checkedItemCount++;
				Item item = Item.ItemList[itemIndex];
				if (item.IsInteractable(this) && (ignoredItems == null || !ignoredItems.Contains(item)) && item.Submarine != null && item.Submarine.TeamID == this.TeamID && item.CurrentHull != null && (!ignoreBroken || item.Condition > 0f) && (base.Submarine == null || base.Submarine.IsEntityFoundOnThisSub(item, true, false, false)) && (customPredicate == null || customPredicate(item)) && (identifiers == null || !identifiers.None((Identifier id) => item.Prefab.Identifier == id || item.HasTag(id))) && (ignoredContainerIdentifiers == null || item.Container == null || !ignoredContainerIdentifiers.Contains(item.ContainerIdentifier)) && !this.IsItemTakenBySomeoneElse(item))
				{
					Entity rootInventoryOwner = item.GetRootInventoryOwner();
					Item ownerItem = rootInventoryOwner as Item;
					if (ownerItem == null || ownerItem.IsInteractable(this))
					{
						float itemPriority = (customPriorityFunction != null) ? customPriorityFunction(item) : 1f;
						if (itemPriority > 0f)
						{
							Vector2 itemPos = (rootInventoryOwner ?? item).WorldPosition;
							Vector2 refPos = (positionalReference != null) ? positionalReference.WorldPosition : this.WorldPosition;
							float distanceFactor = AIObjective.GetDistanceFactor(refPos, itemPos, 0f, 5f, maxItemDistance, 1f);
							itemPriority *= distanceFactor;
							if (itemPriority > this._selectedItemPriority)
							{
								this._selectedItemPriority = itemPriority;
								this._foundItem = item;
							}
						}
					}
				}
				i++;
				itemIndex++;
			}
			targetItem = this._foundItem;
			bool completed = itemIndex >= Item.ItemList.Count - 1;
			if (HumanAIController.DebugAI && checkedItemCount > 0 && targetItem != null && this.StopWatch.ElapsedMilliseconds > 1L)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Went through ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(checkedItemCount);
				defaultInterpolatedStringHandler.AppendLiteral(" of total ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Item.ItemList.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" items. Found item ");
				defaultInterpolatedStringHandler.AppendFormatted(targetItem.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" in ");
				defaultInterpolatedStringHandler.AppendFormatted<long>(this.StopWatch.ElapsedMilliseconds);
				defaultInterpolatedStringHandler.AppendLiteral(" ms. Completed: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(completed);
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				if (this.StopWatch.ElapsedMilliseconds > 5L)
				{
					DebugConsole.ThrowError(msg, null, null, false, false);
				}
				else
				{
					DebugConsole.AddWarning(msg, null);
				}
			}
			return completed;
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000ABA5 File Offset: 0x00008DA5
		public bool IsItemTakenBySomeoneElse(Item item)
		{
			return item.FindParentInventory(delegate(Inventory i)
			{
				if (i.Owner != this)
				{
					Character owner = i.Owner as Character;
					if (owner != null && !owner.IsDead)
					{
						return !owner.Removed;
					}
				}
				return false;
			}) != null;
		}

		// Token: 0x0600018D RID: 397 RVA: 0x0000ABBC File Offset: 0x00008DBC
		public bool CanInteractWith(Character c, float maxDist = 200f, bool checkVisibility = true, bool skipDistanceCheck = false)
		{
			if (c == this || base.Removed || !c.Enabled || !c.CanBeSelected || c.InvisibleTimer > 0f)
			{
				return false;
			}
			if (!c.CharacterHealth.UseHealthWindow && !c.IsDraggable && (c.onCustomInteract == null || !c.AllowCustomInteract))
			{
				return false;
			}
			if (!skipDistanceCheck)
			{
				maxDist = Math.Max(ConvertUnits.ToSimUnits(maxDist), c.AnimController.Collider.GetMaxExtent());
				if (Vector2.DistanceSquared(this.SimPosition, c.SimPosition) > maxDist * maxDist && Vector2.DistanceSquared(this.SimPosition, c.AnimController.MainLimb.SimPosition) > maxDist * maxDist)
				{
					return false;
				}
			}
			return !checkVisibility || this.CanSeeTarget(c, null, false, false);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000AC84 File Offset: 0x00008E84
		public bool CanInteractWith(Item item, bool checkLinked = true)
		{
			float num;
			return this.CanInteractWith(item, out num, checkLinked);
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000AC9C File Offset: 0x00008E9C
		public bool CanInteractWith(Item item, out float distanceToItem, bool checkLinked)
		{
			distanceToItem = -1f;
			bool hidden = item.IsHidden;
			Controller controller = item.GetComponent<Controller>();
			if (controller != null && this.IsAnySelectedItem(item) && controller.IsAttachedUser(this))
			{
				return true;
			}
			if (!this.CanInteract || hidden || !item.IsInteractable(this))
			{
				return false;
			}
			if (item.ParentInventory != null)
			{
				return this.CanAccessInventory(item.ParentInventory, CharacterInventory.AccessLevel.AllowBotsAndPets);
			}
			Wire wire = item.GetComponent<Wire>();
			if (wire != null && item.GetComponent<ConnectionPanel>() == null)
			{
				if (wire.Locked)
				{
					return false;
				}
				if (wire.HiddenInGame && Screen.Selected == GameMain.GameScreen)
				{
					return false;
				}
				Connection connection = wire.Connections[0];
				if (((connection != null) ? connection.Item : null) != null && this.SelectedItem == wire.Connections[0].Item)
				{
					return wire.Connections[1] == null;
				}
				Connection connection2 = wire.Connections[1];
				if (((connection2 != null) ? connection2.Item : null) != null && this.SelectedItem == wire.Connections[1].Item)
				{
					return wire.Connections[0] == null;
				}
				Item selectedItem = this.SelectedItem;
				bool? flag;
				if (selectedItem == null)
				{
					flag = null;
				}
				else
				{
					ConnectionPanel component = selectedItem.GetComponent<ConnectionPanel>();
					flag = ((component != null) ? new bool?(component.DisconnectedWires.Contains(wire)) : null);
				}
				bool? flag2 = flag;
				if (flag2.GetValueOrDefault())
				{
					return wire.Connections[0] == null && wire.Connections[1] == null;
				}
			}
			if (checkLinked && item.DisplaySideBySideWhenLinked)
			{
				foreach (MapEntity linked in item.linkedTo)
				{
					Item linkedItem = linked as Item;
					if (linkedItem != null)
					{
						Inventory parentInventory = linkedItem.ParentInventory;
						float distToLinked;
						if (((parentInventory != null) ? parentInventory.Owner : null) != item && this.CanInteractWith(linkedItem, out distToLinked, false))
						{
							distanceToItem = distToLinked;
							return true;
						}
					}
				}
			}
			if (item.InteractDistance == 0f && !item.Prefab.Triggers.Any<Rectangle>())
			{
				return false;
			}
			Pickable pickableComponent = item.GetComponent<Pickable>();
			if (pickableComponent != null && pickableComponent.Picker != this && pickableComponent.Picker != null && !pickableComponent.Picker.IsDead)
			{
				return false;
			}
			Item selectedItem2 = this.SelectedItem;
			Item item2;
			if (selectedItem2 == null)
			{
				item2 = null;
			}
			else
			{
				RemoteController component2 = selectedItem2.GetComponent<RemoteController>();
				item2 = ((component2 != null) ? component2.TargetItem : null);
			}
			if (item2 == item)
			{
				return true;
			}
			CharacterInventory inventory = this.Inventory;
			Item heldItem = (inventory != null) ? inventory.GetItemInLimbSlot(InvSlotType.RightHand) : null;
			Item item3;
			if (heldItem == null)
			{
				item3 = null;
			}
			else
			{
				RemoteController component3 = heldItem.GetComponent<RemoteController>();
				item3 = ((component3 != null) ? component3.TargetItem : null);
			}
			if (item3 == item)
			{
				return true;
			}
			CharacterInventory inventory2 = this.Inventory;
			Item heldItem2 = (inventory2 != null) ? inventory2.GetItemInLimbSlot(InvSlotType.LeftHand) : null;
			Item item4;
			if (heldItem2 == null)
			{
				item4 = null;
			}
			else
			{
				RemoteController component4 = heldItem2.GetComponent<RemoteController>();
				item4 = ((component4 != null) ? component4.TargetItem : null);
			}
			if (item4 == item)
			{
				return true;
			}
			Vector2 characterDirection = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(this.AnimController.Collider.Rotation));
			Vector2 upperBodyPosition = this.Position + characterDirection * 20f;
			Vector2 lowerBodyPosition = this.Position - characterDirection * 60f;
			if (base.Submarine != null)
			{
				upperBodyPosition += base.Submarine.Position;
				lowerBodyPosition += base.Submarine.Position;
			}
			bool insideTrigger = item.IsInsideTrigger(upperBodyPosition) || item.IsInsideTrigger(lowerBodyPosition);
			if (item.Prefab.Triggers.Length > 0 && !insideTrigger && item.Prefab.RequireBodyInsideTrigger)
			{
				return false;
			}
			Rectangle itemDisplayRect = new Rectangle(item.InteractionRect.X, item.InteractionRect.Y - item.InteractionRect.Height, item.InteractionRect.Width, item.InteractionRect.Height);
			Vector2 playerDistanceCheckPosition = (lowerBodyPosition.Y < upperBodyPosition.Y) ? Vector2.Clamp(itemDisplayRect.Center.ToVector2(), lowerBodyPosition, upperBodyPosition) : Vector2.Clamp(itemDisplayRect.Center.ToVector2(), upperBodyPosition, lowerBodyPosition);
			if (itemDisplayRect.Contains(playerDistanceCheckPosition))
			{
				distanceToItem = 0f;
			}
			else
			{
				Vector2 rectIntersectionPoint = new Vector2(MathHelper.Clamp(playerDistanceCheckPosition.X, (float)itemDisplayRect.X, (float)itemDisplayRect.Right), MathHelper.Clamp(playerDistanceCheckPosition.Y, (float)itemDisplayRect.Y, (float)itemDisplayRect.Bottom));
				distanceToItem = Vector2.Distance(rectIntersectionPoint, playerDistanceCheckPosition);
			}
			float interactDistance = item.InteractDistance;
			if (this.SelectedSecondaryItem != null || item.IsSecondaryItem)
			{
				HumanoidAnimController c = this.AnimController as HumanoidAnimController;
				if (c != null)
				{
					float armLength = 0.75f * ConvertUnits.ToDisplayUnits(c.ArmLength);
					interactDistance = Math.Min(interactDistance, armLength);
				}
			}
			if (distanceToItem > interactDistance && item.InteractDistance > 0f)
			{
				return false;
			}
			Vector2 itemPosition = Character.<CanInteractWith>g__GetPosition|575_1(base.Submarine, item, item.SimPosition);
			if (this.SelectedSecondaryItem != null && !item.IsSecondaryItem)
			{
				if (controller != null && controller.Direction != Direction.None && controller.Direction != this.AnimController.Direction)
				{
					return false;
				}
				Controller selectedController = this.SelectedSecondaryItem.GetComponent<Controller>();
				if (selectedController != null && selectedController.ControlCharacterPose)
				{
					float threshold = ConvertUnits.ToSimUnits(40f);
					if (this.AnimController.Direction == Direction.Left && this.SimPosition.X + threshold < itemPosition.X)
					{
						return false;
					}
					if (this.AnimController.Direction == Direction.Right && this.SimPosition.X - threshold > itemPosition.X)
					{
						return false;
					}
				}
			}
			bool closeEnoughToIgnoreVisibilityCheck = distanceToItem <= 0.1f;
			if (!item.Prefab.InteractThroughWalls && Screen.Selected != GameMain.SubEditorScreen && !insideTrigger && !closeEnoughToIgnoreVisibilityCheck)
			{
				Body body = Submarine.CheckVisibility(this.SimPosition, itemPosition, true, false, true, true, true, null);
				bool itemCenterVisible = Character.<CanInteractWith>g__CheckBody|575_0(body, item);
				if (itemCenterVisible || !item.Prefab.RequireCursorInsideTrigger)
				{
					return itemCenterVisible;
				}
				foreach (Rectangle trigger in item.Prefab.Triggers)
				{
					Rectangle transformTrigger = item.TransformTrigger(trigger, false);
					RectangleF simRect = new RectangleF(ConvertUnits.ToSimUnits(transformTrigger.X), ConvertUnits.ToSimUnits(transformTrigger.Y - transformTrigger.Height), ConvertUnits.ToSimUnits(transformTrigger.Width), ConvertUnits.ToSimUnits(transformTrigger.Height));
					simRect.Location = Character.<CanInteractWith>g__GetPosition|575_1(base.Submarine, item, simRect.Location);
					Vector2 closest = ToolBox.GetClosestPointOnRectangle(simRect, this.SimPosition);
					Body triggerBody = Submarine.CheckVisibility(this.SimPosition, closest, true, false, true, true, true, null);
					if (Character.<CanInteractWith>g__CheckBody|575_0(triggerBody, item))
					{
						return true;
					}
				}
			}
			return true;
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000B368 File Offset: 0x00009568
		public void SetCustomInteract(Action<Character, Character> onCustomInteract, LocalizedString hudText)
		{
			this.onCustomInteract = onCustomInteract;
			this.CustomInteractHUDText = hudText;
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000B378 File Offset: 0x00009578
		public void SelectCharacter(Character character)
		{
			if (character == null || character == this)
			{
				return;
			}
			this.SelectedCharacter = character;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x0000B389 File Offset: 0x00009589
		public void DeselectCharacter()
		{
			if (this.SelectedCharacter == null)
			{
				return;
			}
			if (!this.SelectedCharacter.AllowInput)
			{
				AnimController animController = this.SelectedCharacter.AnimController;
				if (animController != null)
				{
					animController.ResetPullJoints(null);
				}
			}
			this.SelectedCharacter = null;
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000B3C0 File Offset: 0x000095C0
		public void DoInteractionUpdate(float deltaTime, Vector2 mouseSimPos)
		{
			if (this.IsAIControlled)
			{
				return;
			}
			if (this.DisableInteract)
			{
				this.DisableInteract = false;
				return;
			}
			if (!this.CanInteract)
			{
				if (!this.IsAttachedToController())
				{
					this.SelectedItem = null;
				}
				this.SelectedSecondaryItem = null;
				this.focusedItem = null;
				if (!this.AllowInput)
				{
					this.FocusedCharacter = null;
					if (this.SelectedCharacter != null)
					{
						this.DeselectCharacter();
					}
					return;
				}
			}
			Limb head = this.AnimController.GetLimb(LimbType.Head, true, false, false);
			bool headInWater = (head == null) ? this.AnimController.InWater : head.InWater;
			Item selectedSecondaryItem = this.SelectedSecondaryItem;
			Ladder currentLadder = (selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Ladder>() : null;
			if ((this.SelectedSecondaryItem == null || currentLadder != null) && !headInWater && Screen.Selected != GameMain.SubEditorScreen)
			{
				bool climbInput = this.IsKeyDown(InputType.Up) || this.IsKeyDown(InputType.Down);
				bool isControlled = Character.Controlled == this;
				Ladder nearbyLadder = null;
				if (isControlled || climbInput)
				{
					float minDist = float.PositiveInfinity;
					foreach (Ladder ladder in Ladder.List)
					{
						float dist;
						if (ladder != currentLadder && (currentLadder == null || ladder.Item.WorldPosition.Y > currentLadder.Item.WorldPosition.Y == this.IsKeyDown(InputType.Up)) && this.CanInteractWith(ladder.Item, out dist, false) && dist < minDist)
						{
							nearbyLadder = ladder;
							if (isControlled)
							{
								ladder.Item.IsHighlighted = true;
								break;
							}
							break;
						}
					}
				}
				if (nearbyLadder != null && climbInput && nearbyLadder.Select(this))
				{
					this.SelectedSecondaryItem = nearbyLadder.Item;
				}
			}
			bool selectInputSameAsDeselect = false;
			if (this.SelectedCharacter != null && (this.IsKeyHit(InputType.Grab) || this.IsKeyHit(InputType.Health)))
			{
				this.DeselectCharacter();
				return;
			}
			if (this.FocusedCharacter != null && this.IsKeyHit(InputType.Grab) && this.FocusedCharacter.CanBeDraggedBy(this) && (this.CanInteract || (this.FocusedCharacter.IsDead && this.CanEat)))
			{
				this.SelectCharacter(this.FocusedCharacter);
				return;
			}
			Character focusedCharacter = this.FocusedCharacter;
			if (focusedCharacter != null && !focusedCharacter.IsIncapacitated && this.IsKeyHit(InputType.Use) && this.FocusedCharacter.IsPet && this.CanInteract)
			{
				(this.FocusedCharacter.AIController as EnemyAIController).PetBehavior.Play(this);
				return;
			}
			if (this.FocusedCharacter != null && this.IsKeyHit(InputType.Health) && this.FocusedCharacter.CanBeHealedBy(this, true))
			{
				if (this.FocusedCharacter == this.SelectedCharacter)
				{
					this.DeselectCharacter();
					return;
				}
				this.SelectCharacter(this.FocusedCharacter);
				GameServer server = GameMain.Server;
				IReadOnlyList<Client> clients = (server != null) ? server.ConnectedClients : null;
				if (clients == null)
				{
					return;
				}
				using (IEnumerator<Client> enumerator2 = clients.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Client c = enumerator2.Current;
						if (c.Character == this)
						{
							HealingCooldown.SetCooldown(c);
							break;
						}
					}
					return;
				}
			}
			if (this.FocusedCharacter != null && this.IsKeyHit(InputType.Use) && this.FocusedCharacter.onCustomInteract != null && this.FocusedCharacter.AllowCustomInteract)
			{
				this.FocusedCharacter.onCustomInteract(this.FocusedCharacter, this);
				return;
			}
			if (this.IsKeyHit(InputType.Deselect) && this.SelectedItem != null && (this.focusedItem == null || this.focusedItem == this.SelectedItem || !selectInputSameAsDeselect))
			{
				this.SelectedItem = null;
				return;
			}
			if (this.IsKeyHit(InputType.Deselect) && this.SelectedSecondaryItem != null && this.SelectedSecondaryItem.GetComponent<Ladder>() == null && (this.focusedItem == null || this.focusedItem == this.SelectedSecondaryItem || !selectInputSameAsDeselect))
			{
				this.ReleaseSecondaryItem();
				return;
			}
			if (this.IsKeyHit(InputType.Health) && this.SelectedItem != null)
			{
				this.SelectedItem = null;
				return;
			}
			if (this.focusedItem != null)
			{
				bool canInteract = this.focusedItem.TryInteract(this, false, false, false);
			}
		}

		// Token: 0x06000194 RID: 404 RVA: 0x0000B7DC File Offset: 0x000099DC
		public static void UpdateAnimAll(float deltaTime)
		{
			foreach (Character c in Character.CharacterList)
			{
				if (c.Enabled && !c.AnimController.Frozen)
				{
					c.AnimController.UpdateAnimations(deltaTime);
				}
			}
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000B848 File Offset: 0x00009A48
		public static void UpdateAll(float deltaTime, Camera cam)
		{
			if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
			{
				foreach (Character c in Character.CharacterList)
				{
					if ((c is AICharacter || c.IsRemotePlayer) && !c.IsRemotePlayer)
					{
						if (c.IsLocalPlayer || (c.IsBot && !c.IsDead))
						{
							c.Enabled = true;
						}
						else if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
						{
							float closestPlayerDist = c.GetDistanceToClosestPlayer();
							if (closestPlayerDist > c.Params.DisableDistance)
							{
								c.Enabled = false;
								if (c.IsDead && c.AIController is EnemyAIController)
								{
									EntitySpawner spawner = Entity.Spawner;
									if (spawner != null)
									{
										spawner.AddEntityToRemoveQueue(c);
									}
								}
							}
							else if (closestPlayerDist < c.Params.DisableDistance * 0.9f)
							{
								c.Enabled = true;
							}
						}
						else if (Submarine.MainSub != null)
						{
							float distSqr = Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, c.WorldPosition);
							if (Character.Controlled != null)
							{
								distSqr = Math.Min(distSqr, Vector2.DistanceSquared(Character.Controlled.WorldPosition, c.WorldPosition));
							}
							else
							{
								distSqr = Math.Min(distSqr, Vector2.DistanceSquared(GameMain.GameScreen.Cam.GetPosition(), c.WorldPosition));
							}
							if (distSqr > MathUtils.Pow2(c.Params.DisableDistance))
							{
								c.Enabled = false;
								if (c.IsDead && c.AIController is EnemyAIController)
								{
									EntitySpawner spawner2 = Entity.Spawner;
									if (spawner2 != null)
									{
										spawner2.AddEntityToRemoveQueue(c);
									}
								}
							}
							else if (distSqr < MathUtils.Pow2(c.Params.DisableDistance * 0.9f))
							{
								c.Enabled = true;
							}
						}
					}
				}
			}
			Character.characterUpdateTick++;
			if (Character.characterUpdateTick % Character.CharacterUpdateInterval == 0)
			{
				for (int i = 0; i < Character.CharacterList.Count; i++)
				{
					if (!LuaCsSetup.Instance.Game.UpdatePriorityCharacters.Contains(Character.CharacterList[i]))
					{
						Character.CharacterList[i].Update(deltaTime * (float)Character.CharacterUpdateInterval, cam);
					}
				}
			}
			foreach (Character character in LuaCsSetup.Instance.Game.UpdatePriorityCharacters)
			{
				if (!character.Removed)
				{
					character.Update(deltaTime, cam);
				}
			}
		}

		// Token: 0x06000196 RID: 406 RVA: 0x0000BB20 File Offset: 0x00009D20
		public virtual void Update(float deltaTime, Camera cam)
		{
			if (this.TextChatVolume > 0f)
			{
				this.TextChatVolume -= 0.2f * deltaTime;
			}
			if (this.InvisibleTimer > 0f)
			{
				if (Character.Controlled != null && Character.Controlled != this)
				{
					Affliction affliction = Character.Controlled.CharacterHealth.GetAffliction("psychosis", true);
					if (((affliction != null) ? affliction.Strength : 0f) > 0f)
					{
						goto IL_7F;
					}
				}
				this.InvisibleTimer = Math.Min(this.InvisibleTimer, 1f);
				IL_7F:
				this.InvisibleTimer -= deltaTime;
			}
			this.KnockbackCooldownTimer -= deltaTime;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && this == Character.Controlled && !this.isSynced)
			{
				return;
			}
			this.UpdateDespawn(deltaTime, true);
			if (!this.Enabled)
			{
				return;
			}
			if (Level.Loaded != null && (this.WorldPosition.Y < -1000000f || (base.Submarine != null && base.Submarine.WorldPosition.Y < -1000000f)))
			{
				this.Enabled = false;
				this.Kill(CauseOfDeathType.Pressure, null, false, true);
				return;
			}
			this.ApplyStatusEffects(ActionType.Always, deltaTime);
			this.PreviousHull = this.CurrentHull;
			this.CurrentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
			this.obstructVisionAmount = Math.Max(this.obstructVisionAmount - deltaTime, 0f);
			if (this.Inventory != null && Vector2.DistanceSquared(this.lastInventoryItemSetTransformPosition, this.Position) > 0.1f)
			{
				foreach (Item item in this.Inventory.GetAllItems(false))
				{
					if (item.body != null && !item.body.Enabled)
					{
						item.SetTransform(this.SimPosition, 0f, true, true, base.Submarine);
					}
				}
				this.lastInventoryItemSetTransformPosition = this.Position;
			}
			this.HideFace = false;
			this.IgnoreMeleeWeapons = false;
			this.UpdateSightRange(deltaTime);
			this.UpdateSoundRange(deltaTime);
			this.UpdateAttackers(deltaTime);
			for (int i = 0; i < this.characterTalents.Count; i++)
			{
				this.characterTalents[i].UpdateTalent(deltaTime);
			}
			if (this.IsDead)
			{
				return;
			}
			if (GameMain.NetworkMember != null)
			{
				this.UpdateNetInput();
			}
			else
			{
				this.AnimController.Frozen = false;
			}
			this.DisableImpactDamageTimer -= deltaTime;
			if (!this.speechImpedimentSet)
			{
				this.speechImpediment = 0f;
			}
			this.speechImpedimentSet = false;
			if (this.NeedsAir)
			{
				if (!this.IsProtectedFromPressure && (this.AnimController.CurrentHull == null || this.AnimController.CurrentHull.LethalPressure >= 80f))
				{
					if (this.PressureTimer > this.CharacterHealth.PressureKillDelay * 0.1f)
					{
						this.CharacterHealth.ApplyAffliction(this.AnimController.MainLimb, new Affliction(AfflictionPrefab.OrganDamage, this.PressureTimer / 10f * deltaTime), true, false, true);
					}
					if (this.CharacterHealth.PressureKillDelay <= 0f)
					{
						this.PressureTimer = 100f;
					}
					else
					{
						this.PressureTimer += ((this.AnimController.CurrentHull == null) ? 100f : this.AnimController.CurrentHull.LethalPressure) / this.CharacterHealth.PressureKillDelay * deltaTime;
					}
					if (this.PressureTimer >= 100f)
					{
						if (Character.Controlled == this)
						{
							cam.Zoom = 5f;
						}
						if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
						{
							this.Implode(false);
							if (this.IsDead)
							{
								return;
							}
						}
					}
				}
				else
				{
					this.PressureTimer = 0f;
				}
			}
			else if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && !this.IsProtectedFromPressure)
			{
				Level loaded = Level.Loaded;
				float realWorldDepth = (loaded != null) ? loaded.GetRealWorldDepth(this.WorldPosition.Y) : 0f;
				if (this.PressureProtection < realWorldDepth && realWorldDepth > this.CharacterHealth.CrushDepth && (this.AnimController.CurrentHull == null || this.AnimController.CurrentHull.LethalPressure >= 80f))
				{
					this.Implode(false);
					if (this.IsDead)
					{
						return;
					}
				}
			}
			this.ApplyStatusEffects(this.AnimController.InWater ? ActionType.InWater : ActionType.NotInWater, deltaTime);
			this.ApplyStatusEffects(ActionType.OnActive, deltaTime);
			if (this.aiTarget != null && Timing.TotalTime > this.aiTarget.InDetectableSetTime + 0.10000000149011612)
			{
				this.aiTarget.InDetectable = false;
			}
			if (this.NeedsOxygen)
			{
				this.UpdateOxygen(deltaTime);
			}
			this.CalculateHealthMultiplier();
			this.CharacterHealth.Update(deltaTime);
			if (this.IsIncapacitated)
			{
				this.Stun = Math.Max(5f, this.Stun);
				this.AnimController.ResetPullJoints(null);
				this.SelectedItem = (this.SelectedSecondaryItem = null);
				return;
			}
			this.UpdateAIChatMessages(deltaTime);
			bool wasRagdolled = this.IsRagdolled;
			if (this.IsForceRagdolled)
			{
				this.IsRagdolled = this.IsForceRagdolled;
			}
			else if (this != Character.Controlled)
			{
				wasRagdolled = this.IsRagdolled;
				this.IsRagdolled = this.IsKeyDown(InputType.Ragdoll);
				if (this.IsRagdolled && !this.IsPlayer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember == null || !networkMember.IsClient)
					{
						this.ClearInput(InputType.Ragdoll);
					}
				}
			}
			else
			{
				bool tooFastToUnragdoll = this.<Update>g__bodyMovingTooFast|583_1(this.AnimController.Collider) || this.<Update>g__bodyMovingTooFast|583_1(this.AnimController.MainLimb.body);
				if (this.ragdollingLockTimer > 0f)
				{
					this.ragdollingLockTimer -= deltaTime;
				}
				else if (!tooFastToUnragdoll)
				{
					this.IsRagdolled = this.IsKeyDown(InputType.Ragdoll);
					if (wasRagdolled != this.IsRagdolled && !this.AnimController.IsHangingWithRope)
					{
						this.ragdollingLockTimer = 0.2f;
					}
				}
				this.SetInput(InputType.Ragdoll, false, this.IsRagdolled);
			}
			if (!wasRagdolled && this.IsRagdolled && !this.AnimController.IsHangingWithRope)
			{
				this.CheckTalents(AbilityEffectType.OnRagdoll);
			}
			this.lowPassMultiplier = MathHelper.Lerp(this.lowPassMultiplier, 1f, 0.1f);
			if (this.IsRagdolled || !this.CanMove)
			{
				HumanoidAnimController humanAnimController = this.AnimController as HumanoidAnimController;
				if (humanAnimController != null)
				{
					humanAnimController.Crouching = false;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				bool isControlledByRemotelyByServer = networkMember != null && networkMember.IsClient && this.IsRemotelyControlled;
				if (this.IsRagdolled && !isControlledByRemotelyByServer)
				{
					this.AnimController.IgnorePlatforms = true;
				}
				this.AnimController.ResetPullJoints(null);
				if (this.IsAttachedToController())
				{
					if (!this.IsKeyDown(InputType.Ragdoll))
					{
						goto IL_6E6;
					}
					if (GameMain.NetworkMember != null)
					{
						networkMember = GameMain.NetworkMember;
						if (networkMember == null || !networkMember.IsServer)
						{
							goto IL_6E6;
						}
					}
				}
				this.SelectedItem = null;
				IL_6E6:
				this.SelectedSecondaryItem = null;
				this.SelectedCharacter = null;
				return;
			}
			this.Control(deltaTime, cam);
			if (this.IsRemotePlayer)
			{
				Vector2 mouseSimPos = ConvertUnits.ToSimUnits(this.cursorPosition);
				this.DoInteractionUpdate(deltaTime, mouseSimPos);
			}
			if (this.<Update>g__MustDeselect|583_0(this.SelectedItem))
			{
				this.SelectedItem = null;
			}
			if (this.<Update>g__MustDeselect|583_0(this.SelectedSecondaryItem))
			{
				this.ReleaseSecondaryItem();
			}
			if (!this.IsDead)
			{
				this.LockHands = false;
			}
		}

		// Token: 0x06000197 RID: 407 RVA: 0x0000C290 File Offset: 0x0000A490
		public void AddAttacker(Character character, float damage)
		{
			Character.Attacker attacker = this.lastAttackers.FirstOrDefault((Character.Attacker a) => a.Character == character);
			if (attacker != null)
			{
				this.lastAttackers.Remove(attacker);
			}
			else
			{
				attacker = new Character.Attacker
				{
					Character = character
				};
			}
			if (this.lastAttackers.Count > 4)
			{
				this.lastAttackers.RemoveRange(0, this.lastAttackers.Count - 4);
			}
			attacker.Damage += damage;
			this.lastAttackers.Add(attacker);
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000C328 File Offset: 0x0000A528
		public void ForgiveAttacker(Character character)
		{
			int index;
			if ((index = this.lastAttackers.FindIndex((Character.Attacker a) => a.Character == character)) >= 0)
			{
				this.lastAttackers.RemoveAt(index);
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0000C36C File Offset: 0x0000A56C
		public float GetDamageDoneByAttacker(Character otherCharacter)
		{
			if (otherCharacter == null)
			{
				return 0f;
			}
			float dmg = 0f;
			Character.Attacker attacker = this.LastAttackers.LastOrDefault((Character.Attacker a) => a.Character == otherCharacter);
			if (attacker != null)
			{
				dmg = attacker.Damage;
			}
			return dmg;
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000C3C0 File Offset: 0x0000A5C0
		private void UpdateAttackers(float deltaTime)
		{
			foreach (Character.Attacker enemy in this.LastAttackers)
			{
				float cumulativeDamage = enemy.Damage;
				if (cumulativeDamage > 0f)
				{
					float reduction = deltaTime;
					if (cumulativeDamage < 2f)
					{
						reduction *= 0.5f;
					}
					enemy.Damage = Math.Max(0f, enemy.Damage - reduction);
				}
			}
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0000C440 File Offset: 0x0000A640
		private void UpdateOxygen(float deltaTime)
		{
			if (this.NeedsAir && Timing.TotalTime > this.pressureProtectionLastSet + 0.1)
			{
				this.pressureProtection = 0f;
			}
			if (this.NeedsWater)
			{
				float waterAvailable = 100f;
				if (!this.AnimController.InWater && this.CurrentHull != null)
				{
					waterAvailable = this.CurrentHull.WaterPercentage;
				}
				this.OxygenAvailable += MathHelper.Clamp(waterAvailable - this.oxygenAvailable, -deltaTime * 50f, deltaTime * 50f);
			}
			else
			{
				float hullAvailableOxygen = 0f;
				if (!this.AnimController.HeadInWater && this.AnimController.CurrentHull != null)
				{
					if (this.OxygenAvailable * 0.98f < this.AnimController.CurrentHull.OxygenPercentage && this.UseHullOxygen)
					{
						this.AnimController.CurrentHull.Oxygen -= 700f * deltaTime;
					}
					hullAvailableOxygen = this.AnimController.CurrentHull.OxygenPercentage;
				}
				this.OxygenAvailable += MathHelper.Clamp(hullAvailableOxygen - this.oxygenAvailable, -deltaTime * 50f, deltaTime * 50f);
			}
			this.UseHullOxygen = true;
		}

		// Token: 0x0600019C RID: 412 RVA: 0x0000C579 File Offset: 0x0000A779
		protected float GetDistanceToClosestPlayer()
		{
			return (float)Math.Sqrt((double)this.GetDistanceSqrToClosestPlayer());
		}

		// Token: 0x0600019D RID: 413 RVA: 0x0000C588 File Offset: 0x0000A788
		protected float GetDistanceSqrToClosestPlayer()
		{
			float distSqr = float.MaxValue;
			foreach (Character otherCharacter in Character.CharacterList)
			{
				if (otherCharacter != this && otherCharacter.IsRemotePlayer)
				{
					distSqr = Math.Min(distSqr, Vector2.DistanceSquared(otherCharacter.WorldPosition, this.WorldPosition));
					if (otherCharacter.ViewTarget != null)
					{
						distSqr = Math.Min(distSqr, Vector2.DistanceSquared(otherCharacter.ViewTarget.WorldPosition, this.WorldPosition));
					}
				}
			}
			for (int i = 0; i < GameMain.Server.ConnectedClients.Count; i++)
			{
				Vector2? spectatePos = GameMain.Server.ConnectedClients[i].SpectatePos;
				if (spectatePos != null)
				{
					distSqr = Math.Min(distSqr, Vector2.DistanceSquared(spectatePos.Value, this.WorldPosition));
				}
			}
			return distSqr;
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0000C678 File Offset: 0x0000A878
		public float GetDistanceToClosestLimb(Vector2 simPos)
		{
			float closestDist = float.MaxValue;
			foreach (Limb limb in this.AnimController.Limbs)
			{
				if (!limb.IsSevered)
				{
					float dist = Vector2.Distance(simPos, limb.SimPosition);
					dist -= limb.body.GetMaxExtent();
					closestDist = Math.Min(closestDist, dist);
					if (closestDist <= 0f)
					{
						return 0f;
					}
				}
			}
			return closestDist;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000C6E8 File Offset: 0x0000A8E8
		private void UpdateDespawn(float deltaTime, bool createNetworkEvents = true)
		{
			if (!this.EnableDespawn)
			{
				return;
			}
			if (GameMain.NetworkMember != null && !GameMain.NetworkMember.IsServer)
			{
				return;
			}
			if (this.IsDead)
			{
				CauseOfDeath causeOfDeath = this.CauseOfDeath;
				if (causeOfDeath != null && causeOfDeath.Type == CauseOfDeathType.Disconnected)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.Campaign : null) != null)
					{
						return;
					}
				}
				if (this.SelectedBy != null)
				{
					this.despawnTimer = 0f;
					return;
				}
				float despawnDelay = (float)GameSettings.CurrentConfig.CorpseDespawnDelay;
				float despawnPriority = 1f;
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.GameMode : null) is PvPMode)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (((networkMember != null) ? networkMember.RespawnManager : null) != null)
					{
						despawnDelay = (float)GameSettings.CurrentConfig.CorpseDespawnDelayPvP;
						goto IL_14C;
					}
				}
				int subCorpseCount = 0;
				if (base.Submarine != null)
				{
					subCorpseCount = Character.CharacterList.Count((Character c) => c.IsDead && c.Submarine == base.Submarine);
					if (subCorpseCount < GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold)
					{
						return;
					}
				}
				if (subCorpseCount > GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold)
				{
					despawnPriority += (float)(subCorpseCount - GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold) / (float)GameSettings.CurrentConfig.CorpsesPerSubDespawnThreshold;
				}
				float distToClosestPlayer = this.GetDistanceToClosestPlayer();
				if (distToClosestPlayer > this.Params.DisableDistance)
				{
					this.despawnTimer = Math.Max(this.despawnTimer, despawnDelay - 60f);
				}
				if (this.AIController is EnemyAIController)
				{
					despawnPriority *= 2f;
				}
				IL_14C:
				this.despawnTimer += deltaTime * despawnPriority;
				if (this.despawnTimer < despawnDelay)
				{
					return;
				}
				this.Despawn(true);
				return;
			}
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x0000C864 File Offset: 0x0000AA64
		private void Despawn(bool createNetworkEvents = true)
		{
			Character.<>c__DisplayClass597_0 CS$<>8__locals1 = new Character.<>c__DisplayClass597_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.createNetworkEvents = createNetworkEvents;
			if (!this.EnableDespawn)
			{
				return;
			}
			CS$<>8__locals1.despawnContainerId = (this.IsHuman ? Tags.DespawnContainer : this.Params.DespawnContainer);
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				flag = (((networkMember != null) ? networkMember.RespawnManager : null) != null);
			}
			else
			{
				flag = false;
			}
			bool pvpWithRespawning = flag;
			if (!CS$<>8__locals1.despawnContainerId.IsEmpty && !pvpWithRespawning)
			{
				CauseOfDeath causeOfDeath = this.CauseOfDeath;
				if (causeOfDeath == null || causeOfDeath.Type != CauseOfDeathType.Disconnected)
				{
					ItemPrefab itemPrefab;
					if ((itemPrefab = (MapEntityPrefab.FindByIdentifier(CS$<>8__locals1.despawnContainerId) as ItemPrefab)) == null)
					{
						itemPrefab = (ItemPrefab.Prefabs.Find((ItemPrefab me) => ((me != null) ? me.Tags : null) != null && me.Tags.Contains(CS$<>8__locals1.despawnContainerId)) ?? (MapEntityPrefab.FindByIdentifier("metalcrate".ToIdentifier()) as ItemPrefab));
					}
					ItemPrefab containerPrefab = itemPrefab;
					if (containerPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(124, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Could not spawn a container for a despawned character's items. No item with the tag \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.despawnContainerId);
						defaultInterpolatedStringHandler.AppendLiteral("\" or the identifier \"metalcrate\" found.");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
						return;
					}
					EntitySpawner spawner = Entity.Spawner;
					if (spawner == null)
					{
						return;
					}
					ItemPrefab itemPrefab2 = containerPrefab;
					Vector2 worldPosition = this.WorldPosition;
					Action<Item> onSpawned = new Action<Item>(CS$<>8__locals1.<Despawn>g__onItemContainerSpawned|1);
					spawner.AddItemToSpawnQueue(itemPrefab2, worldPosition, null, null, onSpawned);
					return;
				}
			}
			CauseOfDeath causeOfDeath2 = this.CauseOfDeath;
			if (causeOfDeath2 != null && causeOfDeath2.Type == CauseOfDeathType.Disconnected)
			{
				GameSession gameSession2 = GameMain.GameSession;
				MultiPlayerCampaign mpCampaign = ((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					mpCampaign.RefreshCharacterCampaignData(this, false);
				}
			}
			Entity.Spawner.AddEntityToRemoveQueue(this);
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x0000CA20 File Offset: 0x0000AC20
		public void DespawnNow(bool createNetworkEvents = true)
		{
			this.Despawn(createNetworkEvents);
			for (int i = 0; i < 2; i++)
			{
				Entity.Spawner.Update(createNetworkEvents);
			}
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x0000CA4C File Offset: 0x0000AC4C
		public static void RemoveByPrefab(CharacterPrefab prefab)
		{
			if (Character.CharacterList == null)
			{
				return;
			}
			List<Character> list = new List<Character>(Character.CharacterList);
			foreach (Character character in list)
			{
				if (character.Prefab == prefab)
				{
					character.Remove();
				}
			}
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000CAB8 File Offset: 0x0000ACB8
		private void UpdateSightRange(float deltaTime)
		{
			if (this.aiTarget == null)
			{
				return;
			}
			float minRange = Math.Clamp((float)Math.Sqrt((double)this.Mass) * this.Visibility, 250f, 1000f);
			float massFactor = (float)Math.Sqrt((double)(this.Mass / 20f));
			float targetRange = Math.Min(minRange + massFactor * this.AnimController.Collider.LinearVelocity.Length() * 2f * this.Visibility, this.maxAIRange);
			targetRange *= 1f + this.GetStatValue(StatTypes.SightRangeMultiplier, true);
			float newRange = MathHelper.SmoothStep(this.aiTarget.SightRange, targetRange, deltaTime * this.aiTargetChangeSpeed);
			if (!float.IsNaN(newRange))
			{
				this.aiTarget.SightRange = newRange;
			}
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x0000CB7C File Offset: 0x0000AD7C
		private void UpdateSoundRange(float deltaTime)
		{
			if (this.aiTarget == null)
			{
				return;
			}
			if (this.IsDead)
			{
				this.aiTarget.SoundRange = 0f;
				return;
			}
			float massFactor = (float)Math.Sqrt((double)(this.Mass / 10f));
			float targetRange = Math.Min(massFactor * this.AnimController.Collider.LinearVelocity.Length() * 2f * this.Noise, this.maxAIRange);
			float speechImpedimentMultiplier = 1f - this.SpeechImpediment / 100f;
			if (this.TextChatVolume > 0f)
			{
				targetRange = Math.Max(targetRange, this.TextChatVolume * 0.5f * 2000f * speechImpedimentMultiplier);
			}
			if (this.IsPlayer)
			{
				float voipAmplitude = 0f;
				foreach (Client c in GameMain.Server.ConnectedClients)
				{
					if (c.Character == this)
					{
						voipAmplitude = c.VoipServerDecoder.Amplitude;
						break;
					}
				}
				targetRange = Math.Max(targetRange, voipAmplitude * 1.5f * 2000f * speechImpedimentMultiplier);
			}
			targetRange *= 1f + this.GetStatValue(StatTypes.SoundRangeMultiplier, true);
			targetRange = Math.Min(targetRange, this.maxAIRange);
			float newRange = MathHelper.SmoothStep(this.aiTarget.SoundRange, targetRange, deltaTime * this.aiTargetChangeSpeed);
			if (!float.IsNaN(newRange))
			{
				this.aiTarget.SoundRange = newRange;
			}
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x0000CD00 File Offset: 0x0000AF00
		public bool CanHearCharacter(Character speaker)
		{
			if (speaker == null || speaker.SpeechImpediment > 100f)
			{
				return false;
			}
			if (speaker == this)
			{
				return true;
			}
			ChatMessageType messageType = (ChatMessage.CanUseRadio(speaker, false) && ChatMessage.CanUseRadio(this, false)) ? ChatMessageType.Radio : ChatMessageType.Default;
			return !string.IsNullOrEmpty(ChatMessage.ApplyDistanceEffect("message", messageType, speaker, this));
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0000CD54 File Offset: 0x0000AF54
		public void SetOrder(Order order, bool isNewOrder, bool speak = true, bool force = false)
		{
			Character orderGiver = (order != null) ? order.OrderGiver : null;
			if (!force && orderGiver != null && !this.CanHearCharacter(orderGiver))
			{
				return;
			}
			if (order != null && order.AutoDismiss)
			{
				OrderCategory? category = order.Category;
				if (category != null)
				{
					OrderCategory valueOrDefault = category.GetValueOrDefault();
					if (valueOrDefault != OrderCategory.Movement)
					{
						if (valueOrDefault != OrderCategory.Operate || order.TargetEntity == null)
						{
							goto IL_1E5;
						}
						using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Character character = enumerator.Current;
								if (character != this && character.TeamID == this.TeamID && character.AIController is HumanAIController && HumanAIController.IsActive(character) && character.Info != null)
								{
									foreach (Order currentOrder in character.CurrentOrders)
									{
										if (currentOrder != null && currentOrder.Category.GetValueOrDefault() == OrderCategory.Operate)
										{
											Identifier identifier = currentOrder.Identifier;
											Identifier identifier2 = order.Identifier;
											if (!(identifier != identifier2) && currentOrder.TargetEntity == order.TargetEntity && currentOrder.AutoDismiss)
											{
												character.SetOrder(currentOrder.GetDismissal(), isNewOrder, speak, force);
												break;
											}
										}
									}
								}
							}
							goto IL_1E5;
						}
					}
					Order orderToReplace = null;
					if (this.CurrentOrders != null)
					{
						foreach (Order currentOrder2 in this.CurrentOrders)
						{
							if (currentOrder2 != null && currentOrder2.Category.GetValueOrDefault() == OrderCategory.Movement)
							{
								orderToReplace = currentOrder2;
								break;
							}
						}
					}
					if (orderToReplace != null && orderToReplace.AutoDismiss)
					{
						this.SetOrder(orderToReplace.GetDismissal(), isNewOrder, speak, force);
					}
				}
			}
			IL_1E5:
			this.RemoveDuplicateOrders(order);
			this.AddCurrentOrder(order);
			bool flag;
			if (orderGiver != null)
			{
				Identifier identifier = order.Identifier;
				flag = (identifier != "dismissed");
			}
			else
			{
				flag = false;
			}
			if (flag && isNewOrder)
			{
				AbilityOrderedCharacter abilityOrderedCharacter = new AbilityOrderedCharacter(this);
				orderGiver.CheckTalents(AbilityEffectType.OnGiveOrder, abilityOrderedCharacter);
				if (order.OrderGiver.LastOrderedCharacter != this)
				{
					order.OrderGiver.SecondLastOrderedCharacter = order.OrderGiver.LastOrderedCharacter;
					order.OrderGiver.LastOrderedCharacter = this;
				}
			}
			HumanAIController humanAI = this.AIController as HumanAIController;
			if (humanAI != null)
			{
				humanAI.SetOrder(order, speak);
			}
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x0000CFF4 File Offset: 0x0000B1F4
		private void AddCurrentOrder(Order newOrder)
		{
			if (this.CurrentOrders == null)
			{
				return;
			}
			if (newOrder != null)
			{
				Identifier identifier = newOrder.Identifier;
				if (!(identifier == "dismissed"))
				{
					for (int i = 0; i < this.CurrentOrders.Count; i++)
					{
						Order orderInfo = this.CurrentOrders[i];
						if (orderInfo.ManualPriority <= newOrder.ManualPriority)
						{
							this.CurrentOrders[i] = orderInfo.WithManualPriority(orderInfo.ManualPriority - 1);
						}
					}
					this.CurrentOrders.RemoveAll((Order order) => order.ManualPriority <= 0);
					this.CurrentOrders.Add(newOrder);
					this.CurrentOrders.Sort((Order x, Order y) => y.ManualPriority.CompareTo(x.ManualPriority));
					return;
				}
			}
			if (!(newOrder.Option != Identifier.Empty))
			{
				this.CurrentOrders.Clear();
				return;
			}
			if (this.CurrentOrders.Any((Order o) => o.MatchesDismissedOrder(newOrder.Option)))
			{
				Order dismissedOrderInfo = this.CurrentOrders.First((Order o) => o.MatchesDismissedOrder(newOrder.Option));
				int dismissedOrderPriority = dismissedOrderInfo.ManualPriority;
				this.CurrentOrders.Remove(dismissedOrderInfo);
				for (int j = 0; j < this.CurrentOrders.Count; j++)
				{
					Order orderInfo2 = this.CurrentOrders[j];
					if (orderInfo2.ManualPriority < dismissedOrderPriority)
					{
						this.CurrentOrders[j] = orderInfo2.WithManualPriority(orderInfo2.ManualPriority + 1);
					}
				}
				return;
			}
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x0000D1BC File Offset: 0x0000B3BC
		private void RemoveDuplicateOrders(Order order)
		{
			if (this.CurrentOrders == null)
			{
				return;
			}
			int? priorityOfRemoved = null;
			for (int i = this.CurrentOrders.Count - 1; i >= 0; i--)
			{
				Order orderInfo = this.CurrentOrders[i];
				Identifier identifier = order.Identifier;
				Identifier identifier2 = orderInfo.Identifier;
				if (identifier == identifier2)
				{
					priorityOfRemoved = new int?(orderInfo.ManualPriority);
					this.CurrentOrders.RemoveAt(i);
					break;
				}
			}
			if (priorityOfRemoved == null)
			{
				return;
			}
			for (int j = 0; j < this.CurrentOrders.Count; j++)
			{
				Order orderInfo2 = this.CurrentOrders[j];
				if (orderInfo2.ManualPriority < priorityOfRemoved.Value)
				{
					this.CurrentOrders[j] = orderInfo2.WithManualPriority(orderInfo2.ManualPriority + 1);
				}
			}
			this.CurrentOrders.RemoveAll((Order o) => o.ManualPriority <= 0);
			this.CurrentOrders.Sort((Order x, Order y) => y.ManualPriority.CompareTo(x.ManualPriority));
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x0000D2E9 File Offset: 0x0000B4E9
		public Order GetCurrentOrderWithTopPriority()
		{
			return this.GetCurrentOrder(delegate(Order orderInfo)
			{
				if (orderInfo == null)
				{
					return false;
				}
				Identifier identifier = orderInfo.Identifier;
				return !(identifier == "dismissed") && orderInfo.ManualPriority >= 1;
			});
		}

		// Token: 0x060001AA RID: 426 RVA: 0x0000D310 File Offset: 0x0000B510
		public Order GetCurrentOrder(Order order)
		{
			return this.GetCurrentOrder((Order orderInfo) => orderInfo.MatchesOrder(order));
		}

		// Token: 0x060001AB RID: 427 RVA: 0x0000D33C File Offset: 0x0000B53C
		private Order GetCurrentOrder(Func<Order, bool> predicate)
		{
			if (this.CurrentOrders != null && this.CurrentOrders.Any(predicate))
			{
				return this.CurrentOrders.First(predicate);
			}
			return null;
		}

		// Token: 0x060001AC RID: 428 RVA: 0x0000D362 File Offset: 0x0000B562
		public void DisableLine(Identifier identifier)
		{
			if (identifier != Identifier.Empty)
			{
				this.prevAiChatMessages[identifier] = (float)Timing.TotalTime;
			}
		}

		// Token: 0x060001AD RID: 429 RVA: 0x0000D384 File Offset: 0x0000B584
		public void DisableLine(string identifier)
		{
			this.DisableLine(identifier.ToIdentifier());
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0000D394 File Offset: 0x0000B594
		public void Speak(string message, ChatMessageType? messageType = null, float delay = 0f, Identifier identifier = default(Identifier), float minDurationBetweenSimilar = 0f)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (string.IsNullOrEmpty(message))
			{
				return;
			}
			if (this.SpeechImpediment >= 100f)
			{
				return;
			}
			if (this.prevAiChatMessages.ContainsKey(identifier) && (double)this.prevAiChatMessages[identifier] < Timing.TotalTime - (double)minDurationBetweenSimilar)
			{
				this.prevAiChatMessages.Remove(identifier);
			}
			if (minDurationBetweenSimilar > 0f && !(identifier == Identifier.Empty) && (this.aiChatMessageQueue.Any((AIChatMessage m) => m.Identifier == identifier) || this.prevAiChatMessages.ContainsKey(identifier)))
			{
				return;
			}
			this.aiChatMessageQueue.Add(new AIChatMessage(message, messageType, identifier, delay));
		}

		// Token: 0x060001AF RID: 431 RVA: 0x0000D480 File Offset: 0x0000B680
		private void UpdateAIChatMessages(float deltaTime)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			List<AIChatMessage> sentMessages = new List<AIChatMessage>();
			foreach (AIChatMessage message in this.aiChatMessageQueue)
			{
				message.SendDelay -= deltaTime;
				if (message.SendDelay <= 0f)
				{
					WifiComponent radio;
					bool canUseRadio = ChatMessage.CanUseRadio(this, out radio, false);
					if (message.MessageType == null)
					{
						message.MessageType = new ChatMessageType?(canUseRadio ? ChatMessageType.Radio : ChatMessageType.Default);
					}
					if (GameMain.Server != null && message.MessageType.GetValueOrDefault() != ChatMessageType.Order)
					{
						GameMain.Server.SendChatMessage(message.Message, new ChatMessageType?(message.MessageType.Value), null, this, PlayerConnectionChangeType.None, ChatMode.None);
					}
					sentMessages.Add(message);
				}
			}
			foreach (AIChatMessage sent in sentMessages)
			{
				sent.SendTime = Timing.TotalTime;
				this.aiChatMessageQueue.Remove(sent);
				if (sent.Identifier != Identifier.Empty)
				{
					this.prevAiChatMessages[sent.Identifier] = (float)sent.SendTime;
				}
			}
			if (this.prevAiChatMessages.Count > 100)
			{
				HashSet<Identifier> toRemove = new HashSet<Identifier>();
				foreach (KeyValuePair<Identifier, float> prevMessage in this.prevAiChatMessages)
				{
					if ((double)prevMessage.Value < Timing.TotalTime - 60.0)
					{
						toRemove.Add(prevMessage.Key);
					}
				}
				foreach (Identifier identifier in toRemove)
				{
					this.prevAiChatMessages.Remove(identifier);
				}
			}
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000D6B4 File Offset: 0x0000B8B4
		public void ForceSay(LocalizedString messageToSay, bool sayInRadio, bool removeQuotes = false, float delay = 0f)
		{
			if (messageToSay.IsNullOrEmpty() || this.SpeechImpediment >= 100f || this.IsDead)
			{
				return;
			}
			if (removeQuotes)
			{
				messageToSay = new TrimLString(messageToSay, TrimLString.Mode.Both, new char[]
				{
					'"',
					'”',
					'“',
					' '
				});
			}
			ChatMessageType messageType = ChatMessageType.Default;
			WifiComponent radio;
			bool canUseRadio = ChatMessage.CanUseRadio(this, out radio, false);
			if (canUseRadio && sayInRadio)
			{
				messageType = ChatMessageType.Radio;
			}
			CoroutineManager.Invoke(delegate
			{
				GameServer server = GameMain.Server;
				if (server == null)
				{
					return;
				}
				server.SendChatMessage(messageToSay.Value, new ChatMessageType?(messageType), null, this, PlayerConnectionChangeType.None, ChatMode.None);
			}, delay);
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0000D74F File Offset: 0x0000B94F
		public void SetAllDamage(float damageAmount, float bleedingDamageAmount, float burnDamageAmount)
		{
			this.CharacterHealth.SetAllDamage(damageAmount, bleedingDamageAmount, burnDamageAmount);
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000D760 File Offset: 0x0000B960
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			return this.ApplyAttack(attacker, worldPosition, attack, deltaTime, impulseDirection, playSound, null, 0f);
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000D784 File Offset: 0x0000B984
		public AttackResult ApplyAttack(Character attacker, Vector2 worldPosition, Attack attack, float deltaTime, Vector2 impulseDirection, bool playSound = false, Limb targetLimb = null, float penetration = 0f)
		{
			if (base.Removed)
			{
				string errorMsg = "Tried to apply an attack to a removed character ([name]).\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg.Replace("[name]", this.Name), null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Character.ApplyAttack:RemovedCharacter", GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", this.SpeciesName.Value));
				return default(AttackResult);
			}
			Limb limbHit = targetLimb;
			float impulseMagnitude = (attack.TargetImpulse + attack.TargetForce) * attack.ImpactMultiplier * deltaTime;
			Vector2 attackImpulse = Vector2.Zero;
			if (Math.Abs(impulseMagnitude) > 0f)
			{
				impulseDirection = ((impulseDirection.LengthSquared() > 0.0001f) ? Vector2.Normalize(impulseDirection) : Vector2.UnitX);
				attackImpulse = impulseDirection * impulseMagnitude;
			}
			AbilityAttackData attackData = new AbilityAttackData(attack, this, attacker);
			IEnumerable<Affliction> attackAfflictions;
			if (attackData.Afflictions != null)
			{
				attackAfflictions = attackData.Afflictions.Union(attack.Afflictions.Keys);
			}
			else
			{
				attackAfflictions = attack.Afflictions.Keys;
			}
			float damageMultiplier = attack.DamageMultiplier * attackData.DamageMultiplier;
			AttackResult attackResult = (targetLimb == null) ? this.AddDamage(worldPosition, attackAfflictions, attack.Stun, playSound, attackImpulse, out limbHit, attacker, damageMultiplier) : this.DamageLimb(worldPosition, targetLimb, attackAfflictions, attack.Stun, playSound, attackImpulse, attacker, damageMultiplier, true, penetration + attackData.AddedPenetration, attackData.ShouldImplode, false, true);
			if (attacker != null)
			{
				AbilityAttackResult abilityAttackResult = new AbilityAttackResult(attackResult);
				attacker.CheckTalents(AbilityEffectType.OnAttackResult, abilityAttackResult);
				this.CheckTalents(AbilityEffectType.OnAttackedResult, abilityAttackResult);
			}
			if (limbHit == null)
			{
				return default(AttackResult);
			}
			Vector2 forceWorld = (attack.TargetImpulseWorld + attack.TargetForceWorld) * attack.ImpactMultiplier;
			if (attacker != null)
			{
				forceWorld.X *= attacker.AnimController.Dir;
			}
			PhysicsBody body = limbHit.body;
			if (body != null)
			{
				body.ApplyLinearImpulse(forceWorld * deltaTime, 64f);
			}
			Limb mainLimb = limbHit.character.AnimController.MainLimb;
			if (limbHit != mainLimb)
			{
				PhysicsBody body2 = mainLimb.body;
				if (body2 != null)
				{
					body2.ApplyLinearImpulse(forceWorld * deltaTime, 64f);
				}
			}
			if (attacker != null && attacker.AIController == null)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(GameServer.CharacterLogName(this) + " attacked by " + GameServer.CharacterLogName(attacker) + ".");
				if (attackResult.Afflictions != null)
				{
					foreach (Affliction affliction in attackResult.Afflictions)
					{
						if (Math.Abs(affliction.Strength) > 0.1f)
						{
							StringBuilder stringBuilder = sb;
							StringBuilder stringBuilder2 = stringBuilder;
							StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral(" ");
							appendInterpolatedStringHandler.AppendFormatted<LocalizedString>(affliction.Prefab.Name);
							appendInterpolatedStringHandler.AppendLiteral(": ");
							appendInterpolatedStringHandler.AppendFormatted(affliction.Strength.ToString("0.0"));
							stringBuilder2.Append(ref appendInterpolatedStringHandler);
						}
					}
				}
				GameServer.Log(sb.ToString(), ServerLog.MessageType.Attack);
			}
			this.TrySeverLimbJoints(limbHit, attack.SeverLimbsProbability, attackResult.Damage, attacker == null || attacker.IsHuman || attacker.IsPlayer, false, attacker);
			return attackResult;
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x0000DAD8 File Offset: 0x0000BCD8
		public void TrySeverLimbJoints(Limb targetLimb, float severLimbsProbability, float damage, bool allowBeheading, bool ignoreSeveranceProbabilityModifier = false, Character attacker = null)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (damage > 0f && damage < targetLimb.Params.MinSeveranceDamage)
			{
				return;
			}
			if (!this.IsDead)
			{
				if (!allowBeheading && targetLimb.type == LimbType.Head)
				{
					return;
				}
				if (!targetLimb.CanBeSeveredAlive)
				{
					return;
				}
			}
			bool wasSevered = false;
			float random = Rand.Value(Rand.RandSync.Unsynced);
			foreach (LimbJoint joint in this.AnimController.LimbJoints)
			{
				if (joint.CanBeSevered)
				{
					Limb referenceLimb = (targetLimb.type == LimbType.Head && targetLimb.Params.ID == 0) ? joint.LimbA : joint.LimbB;
					if (referenceLimb == targetLimb)
					{
						float probability = severLimbsProbability;
						if (!this.IsDead && !ignoreSeveranceProbabilityModifier)
						{
							probability *= joint.Params.SeveranceProbabilityModifier;
						}
						if (probability > 0f && random <= probability)
						{
							bool severed = this.AnimController.SeverLimbJoint(joint);
							if (!wasSevered)
							{
								wasSevered = severed;
							}
							if (severed)
							{
								Limb otherLimb = (joint.LimbA == targetLimb) ? joint.LimbB : joint.LimbA;
								otherLimb.body.ApplyLinearImpulse(targetLimb.LinearVelocity * targetLimb.Mass, 32f);
								if (attacker != null)
								{
									List<StatusEffect> statusEffectList;
									if (this.statusEffects.TryGetValue(ActionType.OnSevered, out statusEffectList))
									{
										foreach (StatusEffect statusEffect in statusEffectList)
										{
											statusEffect.SetUser(attacker);
										}
									}
									List<StatusEffect> limbStatusEffectList;
									if (targetLimb.StatusEffects.TryGetValue(ActionType.OnSevered, out limbStatusEffectList))
									{
										foreach (StatusEffect statusEffect2 in limbStatusEffectList)
										{
											statusEffect2.SetUser(attacker);
										}
									}
								}
								this.ApplyStatusEffects(ActionType.OnSevered, 1f);
								targetLimb.ApplyStatusEffects(ActionType.OnSevered, 1f);
							}
						}
					}
				}
			}
			if (wasSevered)
			{
				EnemyAIController enemyAI = targetLimb.character.AIController as EnemyAIController;
				if (enemyAI != null)
				{
					enemyAI.ReevaluateAttacks();
				}
			}
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000DD18 File Offset: 0x0000BF18
		public AttackResult AddDamage(Vector2 worldPosition, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2? attackImpulse = null, Character attacker = null, float damageMultiplier = 1f)
		{
			Limb limb;
			return this.AddDamage(worldPosition, afflictions, stun, playSound, attackImpulse ?? Vector2.Zero, out limb, attacker, damageMultiplier);
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0000DD50 File Offset: 0x0000BF50
		public AttackResult AddDamage(Vector2 worldPosition, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, out Limb hitLimb, Character attacker = null, float damageMultiplier = 1f)
		{
			hitLimb = null;
			if (base.Removed)
			{
				return default(AttackResult);
			}
			float closestDistance = 0f;
			foreach (Limb limb in this.AnimController.Limbs)
			{
				float distance = Vector2.DistanceSquared(worldPosition, limb.WorldPosition);
				if (hitLimb == null || distance < closestDistance)
				{
					hitLimb = limb;
					closestDistance = distance;
				}
			}
			return this.DamageLimb(worldPosition, hitLimb, afflictions, stun, playSound, attackImpulse, attacker, damageMultiplier, true, 0f, false, false, true);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x0000DDD8 File Offset: 0x0000BFD8
		public void RecordKill(Character target)
		{
			AbilityCharacterKill abilityCharacterKill = new AbilityCharacterKill(target, this);
			foreach (Character attackerCrewmember in Character.GetFriendlyCrew(this))
			{
				attackerCrewmember.CheckTalents(AbilityEffectType.OnCrewKillCharacter, abilityCharacterKill);
			}
			this.CheckTalents(AbilityEffectType.OnKillCharacter, abilityCharacterKill);
			if (!this.IsOnPlayerTeam)
			{
				return;
			}
			CreatureMetrics.RecordKill(target.SpeciesName);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x0000DE4C File Offset: 0x0000C04C
		public AttackResult DamageLimb(Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker = null, float damageMultiplier = 1f, bool allowStacking = true, float penetration = 0f, bool shouldImplode = false, bool ignoreDamageOverlay = false, bool recalculateVitality = true)
		{
			if (base.Removed)
			{
				return default(AttackResult);
			}
			this.SetStun(stun, false, false);
			if (attacker != null && attacker != this && attacker.IsOnPlayerTeam && GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.AllowFriendlyFire && attacker.TeamID == this.TeamID)
			{
				if (afflictions.None((Affliction a) => a.Prefab.IsBuff))
				{
					return default(AttackResult);
				}
			}
			Vector2 dir = hitLimb.WorldPosition - worldPosition;
			if (attackImpulse.LengthSquared() > 0f)
			{
				Vector2 diff = dir;
				if (diff == Vector2.Zero)
				{
					diff = Rand.Vector(1f, Rand.RandSync.Unsynced);
				}
				Vector2 hitPos = hitLimb.SimPosition + ConvertUnits.ToSimUnits(diff);
				hitLimb.body.ApplyLinearImpulse(attackImpulse, hitPos, 32f);
				Limb mainLimb = hitLimb.character.AnimController.MainLimb;
				if (hitLimb != mainLimb)
				{
					mainLimb.body.ApplyLinearImpulse(attackImpulse, hitPos, 64f);
				}
			}
			bool wasDead = this.IsDead;
			Vector2 simPos = hitLimb.SimPosition + ConvertUnits.ToSimUnits(dir);
			AttackResult attackResult = hitLimb.AddDamage(simPos, afflictions, playSound, damageMultiplier, penetration, attacker);
			this.CharacterHealth.ApplyDamage(hitLimb, attackResult, allowStacking, recalculateVitality);
			if (shouldImplode)
			{
				this.Implode(false);
			}
			if (attacker != this)
			{
				bool wasDamageOverlayVisible = this.CharacterHealth.ShowDamageOverlay;
				if (ignoreDamageOverlay)
				{
					this.CharacterHealth.ShowDamageOverlay = false;
				}
				Character.OnAttackedHandler onAttacked = this.OnAttacked;
				if (onAttacked != null)
				{
					onAttacked(attacker, attackResult);
				}
				this.OnAttackedProjSpecific(attacker, attackResult, stun);
				this.CharacterHealth.ShowDamageOverlay = wasDamageOverlayVisible;
				if (!wasDead)
				{
					this.TryAdjustAttackerSkill(attacker, attackResult);
				}
			}
			if (attackResult.Damage > 0f)
			{
				this.LastDamage = attackResult;
				if (attacker != null && attacker != this && !attacker.Removed)
				{
					this.AddAttacker(attacker, attackResult.Damage);
					if (this.IsOnPlayerTeam)
					{
						CreatureMetrics.AddEncounter(attacker.SpeciesName);
					}
					if (attacker.IsOnPlayerTeam)
					{
						CreatureMetrics.AddEncounter(this.SpeciesName);
					}
				}
				this.ApplyStatusEffects(ActionType.OnDamaged, 1f);
				hitLimb.ApplyStatusEffects(ActionType.OnDamaged, 1f);
			}
			return attackResult;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000E087 File Offset: 0x0000C287
		private void OnAttackedProjSpecific(Character attacker, AttackResult attackResult, float stun)
		{
			GameMain.Server.KarmaManager.OnCharacterHealthChanged(this, attacker, attackResult.Damage, stun, attackResult.Afflictions);
		}

		// Token: 0x060001BA RID: 442 RVA: 0x0000E0A8 File Offset: 0x0000C2A8
		public void TryAdjustAttackerSkill(Character attacker, AttackResult attackResult)
		{
			Character.<>c__DisplayClass627_0 CS$<>8__locals1;
			CS$<>8__locals1.attacker = attacker;
			if (CS$<>8__locals1.attacker == null)
			{
				return;
			}
			if (!CS$<>8__locals1.attacker.IsOnPlayerTeam)
			{
				return;
			}
			if (!(this.AIController is EnemyAIController) && this.TeamID == CS$<>8__locals1.attacker.TeamID)
			{
				return;
			}
			float weaponDamage = 0f;
			float medicalDamage = 0f;
			foreach (Affliction affliction in attackResult.Afflictions)
			{
				if (!affliction.Prefab.IsBuff && (!this.Params.IsMachine || affliction.Prefab.AffectMachines) && !this.Params.Health.ImmunityIdentifiers.Contains(affliction.Identifier))
				{
					if (affliction.Prefab.AfflictionType == AfflictionPrefab.PoisonType || affliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType)
					{
						if (!this.Params.Health.PoisonImmunity)
						{
							float relativeVitality = this.MaxVitality / 100f;
							float dmg = affliction.Strength;
							if (relativeVitality > 0f)
							{
								dmg /= relativeVitality;
							}
							if (this.PoisonVulnerability > 0f)
							{
								dmg /= this.PoisonVulnerability;
							}
							float strength = this.MaxVitality;
							if (this.Params.AI != null)
							{
								strength = this.Params.AI.CombatStrength;
							}
							float vitalityFactor = MathHelper.Lerp(0.5f, 2f, MathUtils.InverseLerp(0f, 1000f, strength));
							dmg *= vitalityFactor;
							medicalDamage += dmg * affliction.Prefab.MedicalSkillGain;
						}
					}
					else
					{
						medicalDamage += affliction.GetVitalityDecrease(null) * affliction.Prefab.MedicalSkillGain;
					}
					weaponDamage += affliction.GetVitalityDecrease(null) * affliction.Prefab.WeaponsSkillGain;
				}
			}
			if (medicalDamage > 0f)
			{
				Character.<TryAdjustAttackerSkill>g__IncreaseSkillLevel|627_0(Tags.MedicalSkill, medicalDamage, ref CS$<>8__locals1);
			}
			if (weaponDamage > 0f)
			{
				Character.<TryAdjustAttackerSkill>g__IncreaseSkillLevel|627_0(Tags.WeaponsSkill, weaponDamage, ref CS$<>8__locals1);
			}
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0000E2F4 File Offset: 0x0000C4F4
		public void TryAdjustHealerSkill(Character healer, float healthChange = 0f, Affliction affliction = null)
		{
			if (healer == null)
			{
				return;
			}
			bool isEnemy = this.AIController is EnemyAIController || this.TeamID != healer.TeamID;
			if (isEnemy)
			{
				return;
			}
			float medicalGain = healthChange;
			AfflictionPrefab afflictionPrefab = (affliction != null) ? affliction.Prefab : null;
			if (afflictionPrefab != null && afflictionPrefab.IsBuff && (!this.Params.IsMachine || affliction.Prefab.AffectMachines))
			{
				medicalGain += affliction.Strength * affliction.Prefab.MedicalSkillGain;
			}
			if (medicalGain > 0f)
			{
				CharacterInfo characterInfo = healer.Info;
				if (characterInfo == null)
				{
					return;
				}
				characterInfo.ApplySkillGain(Tags.MedicalItem, medicalGain * SkillSettings.Current.SkillIncreasePerFriendlyHealed, false, 2f, false);
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x060001BC RID: 444 RVA: 0x0000E3A4 File Offset: 0x0000C5A4
		public bool IsKnockedDownOrRagdolled
		{
			get
			{
				return (this.IsRagdolled && !this.AnimController.IsHangingWithRope) || this.IsKnockedDown;
			}
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x060001BD RID: 445 RVA: 0x0000E3C3 File Offset: 0x0000C5C3
		public bool IsKnockedDown
		{
			get
			{
				return this.CharacterHealth.StunTimer > 1f || this.IsIncapacitated;
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000E3E0 File Offset: 0x0000C5E0
		public void SetStun(float newStun, bool allowStunDecrease = false, bool isNetworkMessage = false)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && !isNetworkMessage)
			{
				return;
			}
			if (Screen.Selected != GameMain.GameScreen)
			{
				return;
			}
			if ((double)newStun < 0.016666666666666666 && this.Stun <= 0f)
			{
				return;
			}
			if (this.GodMode)
			{
				this.CharacterHealth.Stun = 0f;
				return;
			}
			if (newStun > 0f && this.Params.Health.StunImmunity && (this.EmpVulnerability <= 0f || this.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.EMPType, false) <= 0f))
			{
				return;
			}
			if (newStun > 0f)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode && this.IsHuman)
					{
						newStun = Math.Max(0f, newStun - newStun * networkMember.ServerSettings.PvPStunResist);
					}
				}
			}
			if ((newStun <= this.Stun && !allowStunDecrease) || !MathUtils.IsValid(newStun))
			{
				return;
			}
			if (Math.Sign(newStun) != Math.Sign(this.Stun))
			{
				this.AnimController.ResetPullJoints(null);
			}
			this.CharacterHealth.Stun = newStun;
			if (newStun > 0f)
			{
				if (!this.IsAttachedToController())
				{
					this.SelectedItem = null;
				}
				this.SelectedSecondaryItem = null;
				if (this.SelectedCharacter != null)
				{
					this.DeselectCharacter();
				}
			}
			this.HealthUpdateInterval = 0f;
		}

		// Token: 0x060001BF RID: 447 RVA: 0x0000E548 File Offset: 0x0000C748
		public void ApplyStatusEffects(ActionType actionType, float deltaTime)
		{
			if (actionType == ActionType.OnEating)
			{
				float eatingRegen = this.Params.Health.HealthRegenerationWhenEating;
				if (eatingRegen > 0f)
				{
					this.CharacterHealth.ReduceAfflictionOnAllLimbs(AfflictionPrefab.DamageType, eatingRegen * deltaTime, null, null);
				}
			}
			List<StatusEffect> statusEffectList;
			if (this.statusEffects.TryGetValue(actionType, out statusEffectList))
			{
				foreach (StatusEffect statusEffect in statusEffectList)
				{
					if (statusEffect.type != ActionType.OnDamaged || (statusEffect.HasRequiredAfflictions(this.LastDamage) && (!statusEffect.OnlyWhenDamagedByPlayer || (this.LastAttacker != null && this.LastAttacker.IsPlayer))))
					{
						if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems) || statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
						{
							this.targets.Clear();
							statusEffect.AddNearbyTargets(this.WorldPosition, this.targets);
							statusEffect.Apply(actionType, deltaTime, this, this.targets, null);
						}
						else if (statusEffect.targetLimbs != null)
						{
							LimbType[] targetLimbs = statusEffect.targetLimbs;
							for (int i = 0; i < targetLimbs.Length; i++)
							{
								LimbType limbType = targetLimbs[i];
								if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
								{
									foreach (Limb limb in this.AnimController.Limbs)
									{
										if (!limb.IsSevered && limb.type == limbType)
										{
											Character.<ApplyStatusEffects>g__ApplyToLimb|635_0(actionType, deltaTime, statusEffect, this, limb);
										}
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
								{
									Limb limb2 = this.AnimController.GetLimb(limbType, true, false, false);
									if (limb2 != null)
									{
										Character.<ApplyStatusEffects>g__ApplyToLimb|635_0(actionType, deltaTime, statusEffect, this, limb2);
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.LastLimb))
								{
									Limb limb3 = this.AnimController.Limbs.LastOrDefault((Limb l) => l.type == limbType && !l.IsSevered && !l.Hidden);
									if (limb3 != null)
									{
										Character.<ApplyStatusEffects>g__ApplyToLimb|635_0(actionType, deltaTime, statusEffect, this, limb3);
									}
								}
							}
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
						{
							foreach (Limb limb4 in this.AnimController.Limbs)
							{
								if (!limb4.IsSevered)
								{
									Character.<ApplyStatusEffects>g__ApplyToLimb|635_0(actionType, deltaTime, statusEffect, this, limb4);
								}
							}
						}
						if (statusEffect.HasTargetType(StatusEffect.TargetType.This) || statusEffect.HasTargetType(StatusEffect.TargetType.Character))
						{
							statusEffect.Apply(actionType, deltaTime, this, this, null);
						}
						if (statusEffect.HasTargetType(StatusEffect.TargetType.Hull) && this.CurrentHull != null)
						{
							statusEffect.Apply(actionType, deltaTime, this, this.CurrentHull, null);
						}
					}
				}
				if (actionType != ActionType.OnDamaged && actionType != ActionType.OnSevered)
				{
					foreach (Limb limb5 in this.AnimController.Limbs)
					{
						limb5.ApplyStatusEffects(actionType, deltaTime);
					}
				}
			}
			if (actionType != ActionType.OnActive)
			{
				this.CharacterHealth.ApplyAfflictionStatusEffects(actionType);
			}
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000E884 File Offset: 0x0000CA84
		private void Implode(bool isNetworkMessage = false)
		{
			if (this.CharacterHealth.Unkillable || this.GodMode || this.IsDead)
			{
				return;
			}
			NetworkMember networkMember;
			if (!isNetworkMessage)
			{
				networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			this.CharacterHealth.ApplyAffliction(null, new Affliction(AfflictionPrefab.Pressure, AfflictionPrefab.Pressure.MaxStrength), true, false, true);
			networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient || isNetworkMessage)
			{
				this.Kill(CauseOfDeathType.Pressure, null, true, true);
			}
			if (this.IsDead)
			{
				this.BreakJoints();
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000E918 File Offset: 0x0000CB18
		public void BreakJoints()
		{
			Vector2 centerOfMass = this.AnimController.GetCenterOfMass();
			foreach (Limb limb in this.AnimController.Limbs)
			{
				if (!limb.IsSevered)
				{
					limb.AddDamage(limb.SimPosition, 500f, 0f, 0f, false);
					Vector2 diff = centerOfMass - limb.SimPosition;
					if (!MathUtils.IsValid(diff))
					{
						string[] array = new string[7];
						array[0] = "Attempted to apply an invalid impulse to a limb in Character.BreakJoints (";
						int num = 1;
						Vector2 vector = diff;
						array[num] = vector.ToString();
						array[2] = "). Limb position: ";
						array[3] = limb.SimPosition.ToString();
						array[4] = ", center of mass: ";
						int num2 = 5;
						vector = centerOfMass;
						array[num2] = vector.ToString();
						array[6] = ".";
						string errorMsg = string.Concat(array);
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("Ragdoll.GetCenterOfMass", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						return;
					}
					if (!(diff == Vector2.Zero))
					{
						limb.body.ApplyLinearImpulse(diff * 50f, 64f);
					}
				}
			}
			foreach (LimbJoint joint in this.AnimController.LimbJoints)
			{
				if (joint.LimbA.type != LimbType.Head && joint.LimbB.type != LimbType.Head && joint.revoluteJoint != null)
				{
					joint.revoluteJoint.LimitEnabled = false;
				}
			}
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000EAA4 File Offset: 0x0000CCA4
		public void TurnIntoHusk(AfflictionPrefabHusk huskInfection = null, bool? playDead = null)
		{
			if (huskInfection == null)
			{
				huskInfection = (AfflictionPrefab.HuskInfection as AfflictionPrefabHusk);
			}
			if (huskInfection == null)
			{
				DebugConsole.ThrowError("Cannot turn " + this.Name + " into husk, because husk infection was not found!", null, AfflictionPrefab.Prefabs.First<AfflictionPrefab>().ContentPackage, false, false);
				return;
			}
			float startStrength = Rand.Range(Math.Max(huskInfection.MaxStrength - 2f, huskInfection.ActiveThreshold), huskInfection.MaxStrength, Rand.RandSync.Unsynced);
			startStrength *= this.MaxVitality / 100f;
			this.CharacterHealth.ApplyAffliction(this.AnimController.MainLimb, huskInfection.Instantiate(startStrength, null), true, false, true);
			if (playDead != null)
			{
				this.AllowPlayDead = playDead.Value;
			}
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000EB5C File Offset: 0x0000CD5C
		public bool IsAttachedToController()
		{
			if (this.SelectedItem == null)
			{
				return false;
			}
			Controller controller = this.SelectedItem.GetComponent<Controller>();
			return controller != null && controller.IsAttachedUser(this);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0000EB8C File Offset: 0x0000CD8C
		public bool ShouldAvoidStayingAttachedToController()
		{
			if (!this.IsAttachedToController())
			{
				return false;
			}
			Deconstructor deconstructor = this.SelectedItem.GetComponent<Deconstructor>();
			if (deconstructor != null)
			{
				return true;
			}
			if (this.IsHuman)
			{
				Character carryingCharacter = this.SelectedItem.GetRootInventoryOwner() as Character;
				if (carryingCharacter != null && this.TeamID != carryingCharacter.TeamID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000EBE4 File Offset: 0x0000CDE4
		public void Kill(CauseOfDeathType causeOfDeath, Affliction causeOfDeathAffliction, bool isNetworkMessage = false, bool log = true)
		{
			if (this.IsDead || this.CharacterHealth.Unkillable || this.GodMode || base.Removed)
			{
				return;
			}
			this.HealthUpdateInterval = 0f;
			NetworkMember networkMember;
			if (!isNetworkMessage)
			{
				networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				GameMain.NetworkMember.CreateEntityEvent(this, new Character.CharacterStatusEventData(true));
			}
			this.AnimController.Frozen = false;
			Character killer = (causeOfDeathAffliction != null) ? causeOfDeathAffliction.Source : null;
			if (this.IsBot)
			{
				foreach (Item item in this.Inventory.AllItems)
				{
					Character equipper = item.Equipper;
					if (equipper != null && equipper.IsPlayer)
					{
						if (item.GetComponents<ItemContainer>().Any((ItemContainer ic) => ic.BlameEquipperForDeath()))
						{
							killer = item.Equipper;
							HumanAIController humanAi = this.AIController as HumanAIController;
							if (humanAi != null)
							{
								humanAi.OnAttacked(killer, new AttackResult(this.MaxVitality, null));
								break;
							}
							break;
						}
					}
				}
			}
			this.CauseOfDeath = new CauseOfDeath(causeOfDeath, (causeOfDeathAffliction != null) ? causeOfDeathAffliction.Prefab : null, killer, this.LastDamageSource);
			if (this.info != null)
			{
				this.info.LastResistanceMultiplierSkillLossDeath = this.GetAbilityResistance(Tags.SkillLossDeathResistance);
				this.info.LastResistanceMultiplierSkillLossRespawn = this.GetAbilityResistance(Tags.SkillLossRespawnResistance);
			}
			this.isDead = true;
			this.ApplyStatusEffects(ActionType.OnBroken, 1f);
			if (this.Info != null)
			{
				this.Info.LastRewardDistribution = Option.Some<int>(this.Wallet.RewardDistribution);
			}
			if (GameAnalyticsManager.SendUserStatistics)
			{
				CharacterPrefab prefab = this.Prefab;
				if (((prefab != null) ? prefab.ContentPackage : null) == ContentPackageManager.VanillaCorePackage && GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
				{
					string causeOfDeathStr = (causeOfDeathAffliction == null) ? causeOfDeath.ToString() : causeOfDeathAffliction.Prefab.Identifier.Value.Replace(" ", "");
					string characterType = Character.<Kill>g__GetCharacterType|642_1(this);
					GameAnalyticsManager.AddDesignEvent("Kill:" + characterType + ":" + causeOfDeathStr);
					if (this.CauseOfDeath.Killer != null)
					{
						GameAnalyticsManager.AddDesignEvent("Kill:" + characterType + ":Killer:" + Character.<Kill>g__GetCharacterType|642_1(this.CauseOfDeath.Killer));
					}
					if (this.CauseOfDeath.DamageSource != null)
					{
						string damageSourceStr = this.CauseOfDeath.DamageSource.ToString();
						Item damageSourceItem = this.CauseOfDeath.DamageSource as Item;
						if (damageSourceItem != null)
						{
							damageSourceStr = damageSourceItem.ToString();
						}
						GameAnalyticsManager.AddDesignEvent("Kill:" + characterType + ":DamageSource:" + damageSourceStr);
					}
				}
			}
			Character.OnDeathHandler onDeath = this.OnDeath;
			if (onDeath != null)
			{
				onDeath(this, this.CauseOfDeath);
			}
			if (this.CauseOfDeath.Type != CauseOfDeathType.Disconnected)
			{
				AbilityCharacterKiller abilityCharacterKiller = new AbilityCharacterKiller(this.CauseOfDeath.Killer);
				this.CheckTalents(AbilityEffectType.OnDieToCharacter, abilityCharacterKiller);
				Character killer2 = this.CauseOfDeath.Killer;
				if (killer2 != null)
				{
					killer2.RecordKill(this);
				}
			}
			if (GameMain.GameSession != null && Screen.Selected == GameMain.GameScreen)
			{
				AchievementManager.OnCharacterKilled(this, this.CauseOfDeath);
			}
			this.KillProjSpecific(causeOfDeath, causeOfDeathAffliction, log);
			if (this.info != null)
			{
				this.info.CauseOfDeath = this.CauseOfDeath;
				this.info.MissionsCompletedSinceDeath = 0;
			}
			this.AnimController.movement = Vector2.Zero;
			this.AnimController.TargetMovement = Vector2.Zero;
			if (!this.LockHands && causeOfDeath != CauseOfDeathType.Disconnected)
			{
				foreach (Item heldItem in this.HeldItems.ToList<Item>())
				{
					Wearable wearable = heldItem.GetComponent<Wearable>();
					if (wearable == null || !wearable.IsActive)
					{
						heldItem.Drop(this, true, true);
					}
				}
			}
			this.SelectedItem = (this.SelectedSecondaryItem = null);
			this.SelectedCharacter = null;
			this.AnimController.ResetPullJoints(null);
			if (this.AnimController.LimbJoints != null)
			{
				foreach (LimbJoint joint in this.AnimController.LimbJoints)
				{
					if (joint.revoluteJoint != null)
					{
						joint.revoluteJoint.MotorEnabled = false;
					}
				}
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			gameSession.KillCharacter(this);
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000F07C File Offset: 0x0000D27C
		private void KillProjSpecific(CauseOfDeathType causeOfDeath, Affliction causeOfDeathAffliction, bool log)
		{
			if (log)
			{
				if (causeOfDeath == CauseOfDeathType.Affliction)
				{
					GameServer.Log(GameServer.CharacterLogName(this) + " has died (Cause of death: " + causeOfDeathAffliction.Prefab.Name.Value + ")", ServerLog.MessageType.Attack);
				}
				else
				{
					GameServer.Log(GameServer.CharacterLogName(this) + " has died (Cause of death: " + causeOfDeath.ToString() + ")", ServerLog.MessageType.Attack);
				}
			}
			GameServer server = GameMain.Server;
			if (server != null)
			{
				ServerSettings serverSettings = server.ServerSettings;
				if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath)
				{
					GameSession gameSession = GameMain.GameSession;
					MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
					if (mpCampaign != null && causeOfDeath != CauseOfDeathType.Disconnected)
					{
						Client ownerClient = GameMain.Server.ConnectedClients.FirstOrDefault((Client c) => c.Character == this);
						if (ownerClient != null)
						{
							ownerClient.SpectateOnly = true;
							CharacterCampaignData matchingData = mpCampaign.GetClientCharacterData(ownerClient);
							if (matchingData != null)
							{
								matchingData.ApplyPermadeath();
								GameServer server2 = GameMain.Server;
								serverSettings = ((server2 != null) ? server2.ServerSettings : null);
								if (serverSettings != null && serverSettings.IronmanModeActive)
								{
									mpCampaign.SaveSingleCharacter(matchingData, false);
								}
							}
						}
					}
				}
			}
			if (this.HasAbilityFlag(AbilityFlags.RetainExperienceForNewCharacter))
			{
				Client ownerClient2 = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == this);
				if (ownerClient2 != null)
				{
					GameSession gameSession2 = GameMain.GameSession;
					MultiPlayerCampaign multiPlayerCampaign = ((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign;
					if (multiPlayerCampaign != null)
					{
						multiPlayerCampaign.SaveExperiencePoints(ownerClient2);
					}
				}
			}
			this.healthUpdateTimer = 0f;
			if (this.CauseOfDeath.Killer != null && this.CauseOfDeath.Killer.IsTraitor && this.CauseOfDeath.Killer != this)
			{
				Client owner = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == this);
				if (owner != null && !LuaCsSetup.Instance.Game.overrideTraitors)
				{
					GameMain.Server.SendDirectChatMessage(TextManager.FormatServerMessage("KilledByTraitorNotification"), owner, ChatMessageType.ServerMessageBoxInGame);
				}
			}
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (client.InGame)
				{
					client.PendingPositionUpdates.Enqueue(this);
				}
			}
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0000F2B4 File Offset: 0x0000D4B4
		public void Revive(bool removeAfflictions = true, bool createNetworkEvent = false)
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Attempting to revive an already removed character\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			AITarget aiTarget = this.aiTarget;
			if (aiTarget != null)
			{
				aiTarget.Remove();
			}
			this.aiTarget = new AITarget(this);
			if (removeAfflictions)
			{
				this.CharacterHealth.RemoveAllAfflictions();
				this.SetAllDamage(0f, 0f, 0f);
				this.Bloodloss = 0f;
				this.SetStun(0f, true, false);
			}
			this.Oxygen = 100f;
			this.isDead = false;
			if (this.info != null)
			{
				this.info.CauseOfDeath = null;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath)
					{
						this.info.PermanentlyDead = false;
					}
				}
			}
			foreach (LimbJoint joint in this.AnimController.LimbJoints)
			{
				RevoluteJoint revoluteJoint = joint.revoluteJoint;
				if (revoluteJoint != null)
				{
					revoluteJoint.MotorEnabled = true;
				}
				joint.Enabled = true;
				joint.IsSevered = false;
			}
			foreach (Limb limb in this.AnimController.Limbs)
			{
				limb.body.Enabled = true;
				limb.IsSevered = false;
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.ReviveCharacter(this);
			}
			if (createNetworkEvent)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, default(Character.CharacterStatusEventData));
				}
			}
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000F44C File Offset: 0x0000D64C
		public override void Remove()
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Attempting to remove an already removed character\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			DebugConsole.Log(string.Concat(new string[]
			{
				"Removing character ",
				this.Name,
				" (ID: ",
				this.ID.ToString(),
				")"
			}));
			base.Remove();
			foreach (Item heldItem in this.HeldItems.ToList<Item>())
			{
				heldItem.Drop(this, true, true);
			}
			CharacterInfo characterInfo = this.info;
			if (characterInfo != null)
			{
				characterInfo.Remove();
			}
			Character.CharacterList.Remove(this);
			foreach (Projectile attachedProjectile in this.AttachedProjectiles.ToList<Projectile>())
			{
				attachedProjectile.Unstick();
			}
			this.Latchers.ForEachMod(delegate(LatchOntoAI l)
			{
				if (l != null)
				{
					l.DeattachFromBody(true, 0f);
				}
			});
			this.Latchers.Clear();
			if (this.Inventory != null)
			{
				foreach (Item item in this.Inventory.AllItems)
				{
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.AddItemToRemoveQueue(item);
					}
				}
			}
			this.itemSelectedDurations.Clear();
			AITarget aiTarget = this.aiTarget;
			if (aiTarget != null)
			{
				aiTarget.Remove();
			}
			AnimController animController = this.AnimController;
			if (animController != null)
			{
				animController.Remove();
			}
			CharacterHealth characterHealth = this.CharacterHealth;
			if (characterHealth != null)
			{
				characterHealth.Remove();
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.FocusedCharacter == this)
				{
					c.FocusedCharacter = null;
				}
				if (c.SelectedCharacter == this)
				{
					c.SelectedCharacter = null;
				}
			}
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000F6A4 File Offset: 0x0000D8A4
		public void TeleportTo(Vector2 worldPos)
		{
			this.CurrentHull = null;
			this.AnimController.CurrentHull = null;
			base.Submarine = null;
			this.AnimController.SetPosition(ConvertUnits.ToSimUnits(worldPos), false, true, false, true);
			this.AnimController.FindHull(new Vector2?(worldPos), true, false);
			this.CurrentHull = this.AnimController.CurrentHull;
			HumanAIController humanAI = this.AIController as HumanAIController;
			if (humanAI != null)
			{
				IndoorsSteeringManager pathSteering = humanAI.PathSteering;
				if (pathSteering == null)
				{
					return;
				}
				pathSteering.ResetPath();
			}
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000F724 File Offset: 0x0000D924
		public static void SaveInventory(Inventory inventory, XElement parentElement)
		{
			if (inventory == null || parentElement == null)
			{
				return;
			}
			IEnumerable<Item> items = inventory.AllItems.Distinct<Item>();
			foreach (Item item in items)
			{
				item.Submarine = inventory.Owner.Submarine;
				XElement itemElement = item.Save(parentElement);
				List<int> slotIndices = inventory.FindIndices(item);
				itemElement.Add(new XAttribute("i", string.Join<int>(",", slotIndices)));
				foreach (ItemContainer container in item.GetComponents<ItemContainer>())
				{
					XElement childInvElement = new XElement("inventory");
					itemElement.Add(childInvElement);
					Character.SaveInventory(container.Inventory, childInvElement);
				}
			}
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000F824 File Offset: 0x0000DA24
		public void SaveInventory()
		{
			Inventory inventory = this.Inventory;
			CharacterInfo characterInfo = this.Info;
			Character.SaveInventory(inventory, (characterInfo != null) ? characterInfo.InventoryData : null);
		}

		// Token: 0x060001CC RID: 460 RVA: 0x0000F843 File Offset: 0x0000DA43
		public void SpawnInventoryItems(Inventory inventory, ContentXElement itemData)
		{
			this.SpawnInventoryItemsRecursive(inventory, itemData, new List<Item>());
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000F854 File Offset: 0x0000DA54
		private void SpawnInventoryItemsRecursive(Inventory inventory, ContentXElement element, List<Item> extraDuffelBags)
		{
			foreach (ContentXElement itemElement in element.Elements())
			{
				Item newItem = Item.Load(itemElement, inventory.Owner.Submarine, true, IdRemap.DiscardId);
				if (newItem != null)
				{
					if (!MathUtils.NearlyEqual(newItem.Condition, newItem.MaxCondition, 0.0001f) && GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
					{
						newItem.CreateStatusEvent(true);
					}
					Terminal component = newItem.GetComponent<Terminal>();
					if (component != null)
					{
						component.SyncHistory();
					}
					WifiComponent wifiComponent = newItem.GetComponent<WifiComponent>();
					if (wifiComponent != null)
					{
						newItem.CreateServerEvent<WifiComponent>(wifiComponent);
					}
					GeneticMaterial geneticMaterial = newItem.GetComponent<GeneticMaterial>();
					if (geneticMaterial != null)
					{
						newItem.CreateServerEvent<GeneticMaterial>(geneticMaterial);
					}
					this.SyncInGameEditables(newItem);
					int[] slotIndices = itemElement.GetAttributeIntArray("i", new int[1]);
					if (!slotIndices.Any<int>())
					{
						DebugConsole.ThrowError("Invalid inventory data in character \"" + this.Name + "\" - no slot indices found", null, null, false, false);
					}
					else
					{
						bool canBePutInOriginalInventory;
						if (slotIndices[0] >= inventory.Capacity)
						{
							canBePutInOriginalInventory = false;
							for (int m = 0; m < inventory.Capacity; m++)
							{
								if (inventory.CanBePutInSlot(newItem, m, false))
								{
									slotIndices[0] = m;
									canBePutInOriginalInventory = true;
									break;
								}
							}
						}
						else
						{
							canBePutInOriginalInventory = inventory.CanBePutInSlot(newItem, slotIndices[0], true);
						}
						if (canBePutInOriginalInventory)
						{
							inventory.TryPutItem(newItem, slotIndices[0], false, false, null, true, false, true);
							newItem.ParentInventory = inventory;
							for (int j = 0; j < inventory.Capacity; j++)
							{
								if (slotIndices.Contains(j))
								{
									if (!inventory.GetItemsAt(j).Contains(newItem))
									{
										inventory.ForceToSlot(newItem, j);
									}
								}
								else if (inventory.FindIndices(newItem).Contains(j))
								{
									inventory.ForceRemoveFromSlot(newItem, j);
								}
							}
						}
						else
						{
							if (extraDuffelBags.None((Item i) => i.OwnInventory.CanBePut(newItem)))
							{
								ItemPrefab duffelBagPrefab = MapEntityPrefab.FindByIdentifier("duffelbag".ToIdentifier()) as ItemPrefab;
								if (duffelBagPrefab != null)
								{
									Hull hull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
									Submarine mainSub = Submarine.MainSubs.FirstOrDefault((Submarine s) => s.TeamID == this.TeamID);
									if ((hull == null || hull.Submarine != mainSub) && mainSub != null)
									{
										WayPoint wp = WayPoint.GetRandom(SpawnType.Cargo, null, mainSub, false, null, false) ?? WayPoint.GetRandom(SpawnType.Human, null, mainSub, false, null, false);
										if (wp != null)
										{
											hull = Hull.FindHull(wp.WorldPosition, null, true, true);
										}
									}
									Item newDuffelBag = new Item(duffelBagPrefab, (hull != null) ? CargoManager.GetCargoPos(hull, duffelBagPrefab) : this.Position, ((hull != null) ? hull.Submarine : null) ?? base.Submarine, 0, true);
									extraDuffelBags.Add(newDuffelBag);
									Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(newDuffelBag));
								}
							}
							for (int k = 0; k < extraDuffelBags.Count; k++)
							{
								Item duffelBag = extraDuffelBags[k];
								for (int l = 0; l < duffelBag.OwnInventory.Capacity; l++)
								{
									if (duffelBag.OwnInventory.TryPutItem(newItem, l, false, false, null, true, false, true))
									{
										newItem.ParentInventory = duffelBag.OwnInventory;
										break;
									}
								}
							}
						}
						foreach (CircuitBox circuitBox in newItem.GetComponents<CircuitBox>())
						{
							circuitBox.MarkServerRequiredInitialization();
						}
						int itemContainerIndex = 0;
						List<ItemContainer> itemContainers = newItem.GetComponents<ItemContainer>().ToList<ItemContainer>();
						foreach (ContentXElement childInvElement in itemElement.Elements())
						{
							if (itemContainerIndex >= itemContainers.Count)
							{
								break;
							}
							if (childInvElement.Name.ToString().Equals("inventory", StringComparison.OrdinalIgnoreCase))
							{
								this.SpawnInventoryItemsRecursive(itemContainers[itemContainerIndex].Inventory, childInvElement, extraDuffelBags);
								itemContainerIndex++;
							}
						}
					}
				}
			}
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000FD00 File Offset: 0x0000DF00
		public IEnumerable<AttackContext> GetAttackContexts()
		{
			this.currentContexts.Clear();
			if (this.AnimController.InWater)
			{
				this.currentContexts.Add(AttackContext.Water);
			}
			else
			{
				this.currentContexts.Add(AttackContext.Ground);
			}
			if (this.CurrentHull == null)
			{
				this.currentContexts.Add(AttackContext.Outside);
			}
			else
			{
				this.currentContexts.Add(AttackContext.Inside);
			}
			return this.currentContexts;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000FD6C File Offset: 0x0000DF6C
		public List<Hull> GetVisibleHulls()
		{
			this.visibleHulls.Clear();
			this.tempList.Clear();
			if (this.CurrentHull != null)
			{
				this.visibleHulls.Add(this.CurrentHull);
				IEnumerable<Hull> adjacentHulls = this.CurrentHull.GetConnectedHulls(true, new int?(1), false);
				float maxDistance = 1000f;
				Func<Gap, bool> <>9__1;
				foreach (Hull hull in adjacentHulls)
				{
					IEnumerable<Gap> connectedGaps = hull.ConnectedGaps;
					Func<Gap, bool> predicate;
					if ((predicate = <>9__1) == null)
					{
						predicate = (<>9__1 = ((Gap g) => g.Open > 0.9f && g.linkedTo.Contains(this.CurrentHull) && (double)Vector2.DistanceSquared(g.WorldPosition, this.WorldPosition) < Math.Pow((double)(maxDistance / 2f), 2.0)));
					}
					if (connectedGaps.Any(predicate) && (double)Vector2.DistanceSquared(hull.WorldPosition, this.WorldPosition) < Math.Pow((double)maxDistance, 2.0))
					{
						this.visibleHulls.Add(hull);
					}
				}
				Func<Gap, bool> <>9__2;
				this.visibleHulls.AddRange(this.CurrentHull.GetLinkedEntities<Hull>(this.tempList, null, delegate(Hull h)
				{
					if (adjacentHulls.Contains(h))
					{
						return false;
					}
					IEnumerable<Gap> connectedGaps2 = h.ConnectedGaps;
					Func<Gap, bool> predicate2;
					if ((predicate2 = <>9__2) == null)
					{
						predicate2 = (<>9__2 = ((Gap g) => g.Open > 0.9f && (double)Vector2.DistanceSquared(g.WorldPosition, this.WorldPosition) < Math.Pow((double)(maxDistance / 2f), 2.0) && this.CanSeeTarget(g, null, false, false)));
					}
					return connectedGaps2.Any(predicate2) && (double)Vector2.DistanceSquared(h.WorldPosition, this.WorldPosition) < Math.Pow((double)maxDistance, 2.0);
				}));
			}
			return this.visibleHulls;
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000FEB0 File Offset: 0x0000E0B0
		public Vector2 GetRelativeSimPosition(ISpatialEntity target, Vector2? worldPos = null)
		{
			return Submarine.GetRelativeSimPosition(this, target, worldPos);
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x0000FEBA File Offset: 0x0000E0BA
		public bool IsCaptain
		{
			get
			{
				return this.HasJob("captain");
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x060001D2 RID: 466 RVA: 0x0000FEC7 File Offset: 0x0000E0C7
		public bool IsEngineer
		{
			get
			{
				return this.HasJob("engineer");
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x060001D3 RID: 467 RVA: 0x0000FED4 File Offset: 0x0000E0D4
		public bool IsMechanic
		{
			get
			{
				return this.HasJob("mechanic");
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x0000FEE1 File Offset: 0x0000E0E1
		public bool IsMedic
		{
			get
			{
				return this.HasJob("medicaldoctor");
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x060001D5 RID: 469 RVA: 0x0000FEEE File Offset: 0x0000E0EE
		public bool IsSecurity
		{
			get
			{
				return this.HasJob("securityofficer") || this.HasJob("vipsecurityofficer") || this.HasJob("outpostsecurityofficer");
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x0000FF17 File Offset: 0x0000E117
		public bool IsAssistant
		{
			get
			{
				return this.HasJob("assistant");
			}
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x060001D7 RID: 471 RVA: 0x0000FF24 File Offset: 0x0000E124
		public bool IsWatchman
		{
			get
			{
				return this.HasJob("watchman");
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x060001D8 RID: 472 RVA: 0x0000FF31 File Offset: 0x0000E131
		public bool IsVip
		{
			get
			{
				return this.HasJob("prisoner");
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x060001D9 RID: 473 RVA: 0x0000FF3E File Offset: 0x0000E13E
		public bool IsPrisoner
		{
			get
			{
				return this.HasJob("prisoner");
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x060001DA RID: 474 RVA: 0x0000FF4B File Offset: 0x0000E14B
		public bool IsKiller
		{
			get
			{
				return this.HasJob("killer");
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x060001DB RID: 475 RVA: 0x0000FF58 File Offset: 0x0000E158
		// (set) Token: 0x060001DC RID: 476 RVA: 0x0000FF60 File Offset: 0x0000E160
		public Color? UniqueNameColor { get; set; }

		// Token: 0x060001DD RID: 477 RVA: 0x0000FF6C File Offset: 0x0000E16C
		public bool HasJob(string identifier)
		{
			CharacterInfo characterInfo = this.Info;
			Identifier? identifier2;
			Identifier? identifier3;
			if (characterInfo == null)
			{
				identifier2 = null;
				identifier3 = identifier2;
			}
			else
			{
				Job job = characterInfo.Job;
				if (job == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(job.Prefab.Identifier);
				}
			}
			identifier2 = identifier3;
			return identifier2 == identifier;
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000FFBC File Offset: 0x0000E1BC
		public bool HasJob(Identifier identifier)
		{
			CharacterInfo characterInfo = this.Info;
			Identifier? identifier2;
			Identifier? identifier3;
			if (characterInfo == null)
			{
				identifier2 = null;
				identifier3 = identifier2;
			}
			else
			{
				Job job = characterInfo.Job;
				if (job == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(job.Prefab.Identifier);
				}
			}
			identifier2 = identifier3;
			Identifier? identifier4 = new Identifier?(identifier);
			return identifier2 == identifier4;
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x060001DF RID: 479 RVA: 0x00010012 File Offset: 0x0000E212
		public bool IsProtectedFromPressure
		{
			get
			{
				if (!this.IsImmuneToPressure)
				{
					float num = this.PressureProtection;
					Level loaded = Level.Loaded;
					return num >= ((loaded != null) ? loaded.GetRealWorldDepth(this.WorldPosition.Y) : 1f);
				}
				return true;
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x060001E0 RID: 480 RVA: 0x00010049 File Offset: 0x0000E249
		public bool IsImmuneToPressure
		{
			get
			{
				return !this.NeedsAir || this.HasAbilityFlag(AbilityFlags.ImmuneToPressure) || this.GodMode;
			}
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x060001E1 RID: 481 RVA: 0x00010064 File Offset: 0x0000E264
		public IReadOnlyCollection<CharacterTalent> CharacterTalents
		{
			get
			{
				return this.characterTalents;
			}
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0001006C File Offset: 0x0000E26C
		public void ResetTalents(int talentPointReduction)
		{
			this.characterTalents.Clear();
			this.abilityResistances.Clear();
			this.abilityFlags = AbilityFlags.None;
			this.CharacterHealth.RemoveAfflictions((Affliction affliction) => affliction.Prefab.AfflictionType == Tags.AfflictionTypeTalentBuff);
			this.statValues.Clear();
			for (int i = 0; i < talentPointReduction; i++)
			{
				int currentLevel = this.info.GetCurrentLevel();
				if (currentLevel <= 0)
				{
					break;
				}
				this.info.SetExperience(this.info.ExperiencePoints - CharacterInfo.ExperienceRequiredPerLevel(currentLevel));
			}
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00010104 File Offset: 0x0000E304
		public void LoadTalents()
		{
			List<Identifier> toBeRemoved = null;
			foreach (Identifier talent in this.info.UnlockedTalents)
			{
				if (!this.GiveTalent(talent, false))
				{
					DebugConsole.AddWarning(this.Name + " had talent that did not exist! Removing talent from CharacterInfo.", null);
					if (toBeRemoved == null)
					{
						toBeRemoved = new List<Identifier>();
					}
					toBeRemoved.Add(talent);
				}
			}
			if (toBeRemoved != null)
			{
				foreach (Identifier removeTalent in toBeRemoved)
				{
					this.Info.UnlockedTalents.Remove(removeTalent);
				}
			}
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x000101D4 File Offset: 0x0000E3D4
		public bool GiveTalent(Identifier talentIdentifier, bool addingFirstTime = true)
		{
			TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.Identifier == talentIdentifier);
			if (talentPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to add talent by identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(talentIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" to character ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral(", but no such talent exists.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			return this.GiveTalent(talentPrefab, addingFirstTime);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x00010268 File Offset: 0x0000E468
		public bool GiveTalent(uint talentIdentifier, bool addingFirstTime = true)
		{
			TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.UintIdentifier == talentIdentifier);
			if (talentPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to add talent by identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(talentIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" to character ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral(", but no such talent exists.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			return this.GiveTalent(talentPrefab, addingFirstTime);
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x000102FC File Offset: 0x0000E4FC
		public bool GiveTalent(TalentPrefab talentPrefab, bool addingFirstTime = true)
		{
			if (this.info == null)
			{
				return false;
			}
			this.info.UnlockedTalents.Add(talentPrefab.Identifier);
			if (this.characterTalents.Any((CharacterTalent t) => t.Prefab == talentPrefab))
			{
				return false;
			}
			GameMain.NetworkMember.CreateEntityEvent(this, default(Character.UpdateTalentsEventData));
			CharacterTalent characterTalent = new CharacterTalent(talentPrefab, this);
			this.characterTalents.Add(characterTalent);
			characterTalent.ActivateTalent(addingFirstTime);
			characterTalent.AddedThisRound = addingFirstTime;
			if (addingFirstTime)
			{
				this.OnTalentGiven(talentPrefab);
				string str = "TalentUnlocked:";
				Job job = this.info.Job;
				string eventID = str + ((job != null) ? job.Prefab.Identifier : "None".ToIdentifier()).ToString() + ":" + talentPrefab.Identifier.ToString();
				GameSession gameSession = GameMain.GameSession;
				double? num;
				if (gameSession == null)
				{
					num = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					num = ((campaign != null) ? new double?(campaign.TotalPlayTime) : null);
				}
				double? num2 = num;
				GameAnalyticsManager.AddDesignEvent(eventID, num2.GetValueOrDefault());
			}
			return true;
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00010444 File Offset: 0x0000E644
		public bool HasTalent(Identifier identifier)
		{
			return this.info != null && this.info.UnlockedTalents.Contains(identifier);
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x00010461 File Offset: 0x0000E661
		public bool IsTalentLocked(Identifier talentIdentifier)
		{
			return this.info == null || this.Info.GetSavedStatValue(StatTypes.LockedTalents, talentIdentifier) >= 1f;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x00010488 File Offset: 0x0000E688
		public bool HasUnlockedAllTalents()
		{
			TalentTree talentTree;
			if (TalentTree.JobTalentTrees.TryGet(this.Info.Job.Prefab.Identifier, out talentTree))
			{
				foreach (TalentSubTree talentSubTree in talentTree.TalentSubTrees)
				{
					foreach (TalentOption talentOption in talentSubTree.TalentOptionStages)
					{
						if (!talentOption.HasMaxTalents(this.info.UnlockedTalents))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0001050E File Offset: 0x0000E70E
		public bool HasTalents()
		{
			return this.characterTalents.Any<CharacterTalent>();
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0001051C File Offset: 0x0000E71C
		public void CheckTalents(AbilityEffectType abilityEffectType, AbilityObject abilityObject)
		{
			foreach (CharacterTalent characterTalent in this.CharacterTalents)
			{
				characterTalent.CheckTalent(abilityEffectType, abilityObject);
			}
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0001056C File Offset: 0x0000E76C
		public void CheckTalents(AbilityEffectType abilityEffectType)
		{
			foreach (CharacterTalent characterTalent in this.characterTalents)
			{
				characterTalent.CheckTalent(abilityEffectType, null);
			}
		}

		// Token: 0x060001ED RID: 493 RVA: 0x000105C0 File Offset: 0x0000E7C0
		private void OnTalentGiven(TalentPrefab talentPrefab)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 2);
			defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(this));
			defaultInterpolatedStringHandler.AppendLiteral(" has gained the talent '");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Talent);
		}

		// Token: 0x060001EE RID: 494 RVA: 0x00010618 File Offset: 0x0000E818
		public bool IsInSameRoomAs(Character character)
		{
			if (character == this)
			{
				return true;
			}
			if (character.CurrentHull == null || this.CurrentHull == null)
			{
				return false;
			}
			if (character.Submarine != base.Submarine)
			{
				return false;
			}
			if (character.CurrentHull == this.CurrentHull)
			{
				return true;
			}
			this.sameRoomHulls.Clear();
			this.CurrentHull.GetLinkedEntities<Hull>(this.sameRoomHulls, null, null);
			this.sameRoomHulls.Add(this.CurrentHull);
			return this.sameRoomHulls.Contains(character.CurrentHull);
		}

		// Token: 0x060001EF RID: 495 RVA: 0x000106A8 File Offset: 0x0000E8A8
		public static IEnumerable<Character> GetFriendlyCrew(Character character)
		{
			if (character == null)
			{
				return Enumerable.Empty<Character>();
			}
			return from c in Character.CharacterList
			where c.Info != null && !c.IsDead && !c.IsPet && HumanAIController.IsFriendly(character, c, true, false)
			select c;
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x000106E8 File Offset: 0x0000E8E8
		public bool HasRecipeForItem(Identifier recipeIdentifier)
		{
			return (GameMain.GameSession != null && GameMain.GameSession.HasUnlockedRecipe(this, recipeIdentifier)) || this.characterTalents.Any((CharacterTalent t) => t.UnlockedRecipes.Contains(recipeIdentifier));
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00010738 File Offset: 0x0000E938
		public bool HasStoreAccessForItem(ItemPrefab prefab)
		{
			foreach (CharacterTalent talent in this.characterTalents)
			{
				foreach (Identifier unlockedItem in talent.UnlockedStoreItems)
				{
					if (prefab.Tags.Contains(unlockedItem))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x000107D8 File Offset: 0x0000E9D8
		public void GiveMoney(int amount)
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign == null)
			{
				return;
			}
			if (amount <= 0)
			{
				return;
			}
			MultiPlayerCampaign mpCampaign = campaign as MultiPlayerCampaign;
			if (mpCampaign == null)
			{
				throw new InvalidOperationException("Campaign on a server is not a multiplayer campaign");
			}
			Client targetClient = null;
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (client.Character == this)
				{
					targetClient = client;
					break;
				}
			}
			Wallet wallet = (targetClient == null) ? mpCampaign.Bank : mpCampaign.GetWallet(targetClient);
			int prevAmount = wallet.Balance;
			wallet.Give(amount);
			this.OnMoneyChanged(prevAmount, wallet.Balance);
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0001089C File Offset: 0x0000EA9C
		private void OnMoneyChanged(int prevAmount, int newAmount)
		{
			GameMain.NetworkMember.CreateEntityEvent(this, default(Character.UpdateMoneyEventData));
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x000108C4 File Offset: 0x0000EAC4
		public float GetStatValue(StatTypes statType, bool includeSaved = true)
		{
			if (this.Info == null)
			{
				return 0f;
			}
			float statValue = 0f;
			float value;
			if (this.statValues.TryGetValue(statType, out value))
			{
				statValue += value;
			}
			if (this.CharacterHealth != null)
			{
				statValue += this.CharacterHealth.GetStatValue(statType);
			}
			if (includeSaved)
			{
				statValue += this.Info.GetSavedStatValue(statType);
			}
			float wearableValue;
			if (this.wearableStatValues.TryGetValue(statType, out wearableValue))
			{
				statValue += wearableValue;
			}
			foreach (Item heldItem in this.HeldItems)
			{
				Holdable holdable = heldItem.GetComponent<Holdable>();
				float holdableValue;
				if (holdable != null && holdable.HoldableStatValues.TryGetValue(statType, out holdableValue))
				{
					statValue += holdableValue;
				}
			}
			return statValue;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00010994 File Offset: 0x0000EB94
		public void OnWearablesChanged()
		{
			HashSet<Wearable> handledWearables = new HashSet<Wearable>();
			this.wearableStatValues.Clear();
			this.wearableSkillModifiers.Clear();
			for (int i = 0; i < this.Inventory.Capacity; i++)
			{
				if (this.Inventory.SlotTypes[i] != InvSlotType.Any && this.Inventory.SlotTypes[i] != InvSlotType.LeftHand && this.Inventory.SlotTypes[i] != InvSlotType.RightHand)
				{
					Item itemAt = this.Inventory.GetItemAt(i);
					Wearable wearable = (itemAt != null) ? itemAt.GetComponent<Wearable>() : null;
					if (wearable != null && !handledWearables.Contains(wearable))
					{
						handledWearables.Add(wearable);
						foreach (KeyValuePair<StatTypes, float> statValuePair in wearable.WearableStatValues)
						{
							if (this.wearableStatValues.ContainsKey(statValuePair.Key))
							{
								Dictionary<StatTypes, float> dictionary = this.wearableStatValues;
								StatTypes key = statValuePair.Key;
								dictionary[key] += statValuePair.Value;
							}
							else
							{
								this.wearableStatValues.Add(statValuePair.Key, statValuePair.Value);
							}
						}
						foreach (KeyValuePair<Identifier, float> skillModifier in wearable.SkillModifiers)
						{
							if (this.wearableSkillModifiers.ContainsKey(skillModifier.Key))
							{
								Dictionary<Identifier, float> dictionary2 = this.wearableSkillModifiers;
								Identifier key2 = skillModifier.Key;
								dictionary2[key2] += skillModifier.Value;
							}
							else
							{
								this.wearableSkillModifiers.Add(skillModifier.Key, skillModifier.Value);
							}
						}
					}
				}
			}
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00010B78 File Offset: 0x0000ED78
		public void ChangeStat(StatTypes statType, float value)
		{
			if (this.statValues.ContainsKey(statType))
			{
				Dictionary<StatTypes, float> dictionary = this.statValues;
				dictionary[statType] += value;
				return;
			}
			this.statValues.Add(statType, value);
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00010BBC File Offset: 0x0000EDBC
		private static StatTypes GetSkillStatType(Identifier skillIdentifier)
		{
			string a = skillIdentifier.Value.ToLowerInvariant();
			if (a == "electrical")
			{
				return StatTypes.ElectricalSkillBonus;
			}
			if (a == "helm")
			{
				return StatTypes.HelmSkillBonus;
			}
			if (a == "mechanical")
			{
				return StatTypes.MechanicalSkillBonus;
			}
			if (a == "medical")
			{
				return StatTypes.MedicalSkillBonus;
			}
			if (!(a == "weapons"))
			{
				return StatTypes.None;
			}
			return StatTypes.WeaponsSkillBonus;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00010C24 File Offset: 0x0000EE24
		public void AddAbilityFlag(AbilityFlags abilityFlag)
		{
			this.abilityFlags |= abilityFlag;
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00010C34 File Offset: 0x0000EE34
		public void RemoveAbilityFlag(AbilityFlags abilityFlag)
		{
			this.abilityFlags &= ~abilityFlag;
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00010C45 File Offset: 0x0000EE45
		public bool HasAbilityFlag(AbilityFlags abilityFlag)
		{
			return this.abilityFlags.HasFlag(abilityFlag) || this.CharacterHealth.HasFlag(abilityFlag);
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00010C70 File Offset: 0x0000EE70
		public float GetAbilityResistance(Identifier resistanceId)
		{
			float resistance = 0f;
			bool hadResistance = false;
			foreach (KeyValuePair<TalentResistanceIdentifier, float> keyValuePair in this.abilityResistances)
			{
				TalentResistanceIdentifier talentResistanceIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentResistanceIdentifier, out num);
				TalentResistanceIdentifier key = talentResistanceIdentifier;
				float value = num;
				Identifier resistanceIdentifier = key.ResistanceIdentifier;
				if (resistanceIdentifier == resistanceId)
				{
					resistance += value;
					hadResistance = true;
				}
			}
			if (!hadResistance)
			{
				return 1f;
			}
			return Math.Max(0f, resistance);
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00010D08 File Offset: 0x0000EF08
		public float GetAbilityResistance(AfflictionPrefab affliction)
		{
			float resistance = 0f;
			bool hadResistance = false;
			foreach (KeyValuePair<TalentResistanceIdentifier, float> keyValuePair in this.abilityResistances)
			{
				TalentResistanceIdentifier talentResistanceIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentResistanceIdentifier, out num);
				TalentResistanceIdentifier key = talentResistanceIdentifier;
				float value = num;
				Identifier resistanceIdentifier = key.ResistanceIdentifier;
				if (!(resistanceIdentifier == affliction.AfflictionType))
				{
					Identifier resistanceIdentifier2 = key.ResistanceIdentifier;
					if (!(resistanceIdentifier2 == affliction.Identifier))
					{
						continue;
					}
				}
				resistance += value;
				hadResistance = true;
			}
			if (!hadResistance)
			{
				return 1f;
			}
			return Math.Max(0f, resistance);
		}

		// Token: 0x060001FD RID: 509 RVA: 0x00010DBC File Offset: 0x0000EFBC
		public void ChangeAbilityResistance(TalentResistanceIdentifier identifier, float value)
		{
			if (!MathUtils.IsValid(value))
			{
				return;
			}
			if (this.abilityResistances.ContainsKey(identifier))
			{
				Dictionary<TalentResistanceIdentifier, float> dictionary = this.abilityResistances;
				dictionary[identifier] *= value;
				return;
			}
			this.abilityResistances.Add(identifier, value);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x00010E07 File Offset: 0x0000F007
		public void RemoveAbilityResistance(TalentResistanceIdentifier identifier)
		{
			this.abilityResistances.Remove(identifier);
		}

		// Token: 0x060001FF RID: 511 RVA: 0x00010E16 File Offset: 0x0000F016
		public bool IsFriendly(Character other)
		{
			return Character.IsFriendly(this, other);
		}

		// Token: 0x06000200 RID: 512 RVA: 0x00010E1F File Offset: 0x0000F01F
		public static bool IsFriendly(Character me, Character other)
		{
			return Character.IsOnFriendlyTeam(me, other) && Character.IsSameSpeciesOrGroup(me, other);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x00010E34 File Offset: 0x0000F034
		public static bool IsOnFriendlyTeam(CharacterTeamType myTeam, CharacterTeamType otherTeam)
		{
			if (myTeam == otherTeam)
			{
				return true;
			}
			bool result;
			switch (myTeam)
			{
			case CharacterTeamType.None:
				result = (otherTeam == CharacterTeamType.FriendlyNPC);
				break;
			case CharacterTeamType.Team1:
			case CharacterTeamType.Team2:
				result = (otherTeam == CharacterTeamType.FriendlyNPC);
				break;
			case CharacterTeamType.FriendlyNPC:
			{
				bool flag = otherTeam - CharacterTeamType.Team1 <= 1;
				result = flag;
				break;
			}
			default:
				result = true;
				break;
			}
			return result;
		}

		// Token: 0x06000202 RID: 514 RVA: 0x00010E80 File Offset: 0x0000F080
		public static bool IsOnFriendlyTeam(Character me, Character other)
		{
			return Character.IsOnFriendlyTeam(me.TeamID, other.TeamID);
		}

		// Token: 0x06000203 RID: 515 RVA: 0x00010E93 File Offset: 0x0000F093
		public bool IsOnFriendlyTeam(Character other)
		{
			return Character.IsOnFriendlyTeam(this.TeamID, other.TeamID);
		}

		// Token: 0x06000204 RID: 516 RVA: 0x00010EA6 File Offset: 0x0000F0A6
		public bool IsOnFriendlyTeam(CharacterTeamType otherTeam)
		{
			return Character.IsOnFriendlyTeam(this.TeamID, otherTeam);
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00010EB4 File Offset: 0x0000F0B4
		public bool IsSameSpeciesOrGroup(Character other)
		{
			return Character.IsSameSpeciesOrGroup(this, other);
		}

		// Token: 0x06000206 RID: 518 RVA: 0x00010EC0 File Offset: 0x0000F0C0
		public static bool IsSameSpeciesOrGroup(Character me, Character other)
		{
			Identifier speciesName = other.SpeciesName;
			Identifier speciesName2 = me.SpeciesName;
			return speciesName == speciesName2 || CharacterParams.CompareGroup(me.Group, other.Group);
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00010EF9 File Offset: 0x0000F0F9
		public bool MatchesSpeciesNameOrGroup(Identifier speciesNameOrGroup)
		{
			return this.Prefab.MatchesSpeciesNameOrGroup(speciesNameOrGroup);
		}

		// Token: 0x06000208 RID: 520 RVA: 0x00010F07 File Offset: 0x0000F107
		public void StopClimbing()
		{
			this.AnimController.StopClimbing();
			this.ReleaseSecondaryItem();
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000209 RID: 521 RVA: 0x00010F1A File Offset: 0x0000F11A
		// (set) Token: 0x0600020A RID: 522 RVA: 0x00010F22 File Offset: 0x0000F122
		public float HealthUpdateInterval
		{
			get
			{
				return this.healthUpdateInterval;
			}
			set
			{
				this.healthUpdateInterval = MathHelper.Clamp(value, 0f, this.IsDead ? NetConfig.MaxHealthUpdateIntervalDead : NetConfig.MaxHealthUpdateInterval);
				this.healthUpdateTimer = Math.Min(this.healthUpdateTimer, this.healthUpdateInterval);
			}
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x0600020B RID: 523 RVA: 0x00010F60 File Offset: 0x0000F160
		public List<CharacterStateInfo> MemState
		{
			get
			{
				return this.memState;
			}
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x0600020C RID: 524 RVA: 0x00010F68 File Offset: 0x0000F168
		public List<CharacterStateInfo> MemLocalState
		{
			get
			{
				return this.memLocalState;
			}
		}

		// Token: 0x0600020D RID: 525 RVA: 0x00010F70 File Offset: 0x0000F170
		public void ResetNetState()
		{
			this.memInput.Clear();
			this.memState.Clear();
			this.memLocalState.Clear();
			this.LastNetworkUpdateID = 0;
			this.LastProcessedID = 0;
		}

		// Token: 0x0600020E RID: 526 RVA: 0x00010FA4 File Offset: 0x0000F1A4
		private void UpdateNetInput()
		{
			if (!(this is AICharacter) || this.IsRemotePlayer)
			{
				if (!this.CanMove)
				{
					this.AnimController.Frozen = false;
					if (this.memInput.Count > 0)
					{
						this.prevDequeuedInput = this.dequeuedInput;
						this.dequeuedInput = (this.memInput[this.memInput.Count - 1].states & Character.InputNetFlags.Ragdoll);
						this.memInput.RemoveAt(this.memInput.Count - 1);
					}
				}
				else if (this.memInput.Count == 0)
				{
					this.AnimController.Frozen = true;
					if (Timing.TotalTime > this.LastInputTime + 0.5)
					{
						this.prevDequeuedInput = (this.dequeuedInput = (this.dequeuedInput.HasFlag(Character.InputNetFlags.FacingLeft) ? Character.InputNetFlags.FacingLeft : Character.InputNetFlags.None));
					}
				}
				else
				{
					this.AnimController.Frozen = false;
					this.prevDequeuedInput = this.dequeuedInput;
					this.LastProcessedID = this.memInput[this.memInput.Count - 1].networkUpdateID;
					this.dequeuedInput = this.memInput[this.memInput.Count - 1].states;
					double aimAngle = (double)this.memInput[this.memInput.Count - 1].intAim / 65535.0 * 2.0 * 3.141592653589793;
					this.cursorPosition = this.AimRefPosition + new Vector2((float)Math.Cos(aimAngle), (float)Math.Sin(aimAngle)) * 500f;
					if (this.memInput[this.memInput.Count - 1].states.HasFlag(Character.InputNetFlags.Use) || this.memInput[this.memInput.Count - 1].states.HasFlag(Character.InputNetFlags.Select) || this.memInput[this.memInput.Count - 1].states.HasFlag(Character.InputNetFlags.Deselect) || this.memInput[this.memInput.Count - 1].states.HasFlag(Character.InputNetFlags.Health) || this.memInput[this.memInput.Count - 1].states.HasFlag(Character.InputNetFlags.Grab))
					{
						this.focusedItem = null;
						this.FocusedCharacter = null;
					}
					Entity closestEntity = Entity.FindEntityByID(this.memInput[this.memInput.Count - 1].interact);
					Item item = closestEntity as Item;
					if (item != null)
					{
						if (this.CanInteractWith(item, true))
						{
							this.focusedItem = item;
							this.FocusedCharacter = null;
						}
						else
						{
							item.PositionUpdateInterval = 0f;
							Holdable holdable = item.GetComponent<Holdable>();
							if (holdable != null)
							{
								Item item2 = holdable.Item;
								if (item2 != null)
								{
									item2.CreateServerEvent<Holdable>(holdable);
								}
							}
						}
					}
					else
					{
						Character character = closestEntity as Character;
						if (character != null && this.CanInteractWith(character, 250f, true, false))
						{
							this.FocusedCharacter = character;
							this.focusedItem = null;
						}
					}
					this.memInput.RemoveAt(this.memInput.Count - 1);
					if ((this.dequeuedInput == Character.InputNetFlags.None || this.dequeuedInput == Character.InputNetFlags.FacingLeft) && Math.Abs(this.AnimController.Collider.LinearVelocity.X) < 0.005f && Math.Abs(this.AnimController.Collider.LinearVelocity.Y) < 0.2f)
					{
						while (this.memInput.Count > 5 && this.memInput[this.memInput.Count - 1].states == this.dequeuedInput)
						{
							this.LastProcessedID = this.memInput[this.memInput.Count - 1].networkUpdateID;
							this.memInput.RemoveAt(this.memInput.Count - 1);
						}
					}
				}
			}
			this.AnimController.Frozen = false;
			if (this.networkUpdateSent)
			{
				foreach (Key key in this.keys)
				{
					key.DequeueHit();
					key.DequeueHeld();
				}
				this.networkUpdateSent = false;
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x000114D5 File Offset: 0x0000F6D5
		[CompilerGenerated]
		private bool <ServerEventRead>g__CanManageTalents|14_0(Client client)
		{
			return client.Character == this || (client.TeamID == this.TeamID && this.IsBot && client.HasPermission(ClientPermissions.ManageBotTalents) && !client.Spectating);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0001153C File Offset: 0x0000F73C
		[CompilerGenerated]
		internal static bool <Control>g__CanUseItemsWhenSelected|546_0(Item item)
		{
			return item == null || !item.Prefab.DisableItemUsageWhenSelected;
		}

		// Token: 0x06000214 RID: 532 RVA: 0x00011554 File Offset: 0x0000F754
		[CompilerGenerated]
		private void <Control>g__tryUseItem|546_1(Item item, float deltaTime)
		{
			if (this.IsKeyDown(InputType.Aim) || !item.RequireAimToSecondaryUse)
			{
				item.SecondaryUse(deltaTime, this);
			}
			if (this.IsKeyDown(InputType.Use) && !item.IsShootable && (!item.RequireAimToUse || this.IsKeyDown(InputType.Aim)))
			{
				item.Use(deltaTime, this, null, null, null);
			}
			if (this.IsKeyDown(InputType.Shoot) && item.IsShootable && (!item.RequireAimToUse || this.IsKeyDown(InputType.Aim)))
			{
				item.Use(deltaTime, this, null, null, null);
			}
		}

		// Token: 0x06000215 RID: 533 RVA: 0x000115D5 File Offset: 0x0000F7D5
		[CompilerGenerated]
		private bool <IsInventoryAccessibleTo>g__IsOnSameTeam|565_0(ref Character.<>c__DisplayClass565_0 A_1)
		{
			return A_1.character.TeamID == this.teamID;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x000115EA File Offset: 0x0000F7EA
		[CompilerGenerated]
		private bool <IsInventoryAccessibleTo>g__IsFriendlyPet|565_1(ref Character.<>c__DisplayClass565_0 A_1)
		{
			return this.IsPet && A_1.character.IsFriendly(this);
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0001163C File Offset: 0x0000F83C
		[CompilerGenerated]
		internal static bool <CanInteractWith>g__CheckBody|575_0(Body body, Item item)
		{
			if (body == null)
			{
				return true;
			}
			Item item2;
			if ((item2 = (body.UserData as Item)) == null)
			{
				ItemComponent itemComponent = body.UserData as ItemComponent;
				item2 = ((itemComponent != null) ? itemComponent.Item : null);
			}
			Item otherItem = item2;
			if (otherItem != item)
			{
				ItemComponent itemComponent2 = body.UserData as ItemComponent;
				if (((itemComponent2 != null) ? itemComponent2.Item : null) != item)
				{
					Door door = (otherItem != null) ? otherItem.GetComponent<Door>() : null;
					if (door == null || !door.IsOpen)
					{
						Fixture lastPickedFixture = Submarine.LastPickedFixture;
						if (((lastPickedFixture != null) ? lastPickedFixture.UserData : null) as Item != item)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06000219 RID: 537 RVA: 0x000116C8 File Offset: 0x0000F8C8
		[CompilerGenerated]
		internal static Vector2 <CanInteractWith>g__GetPosition|575_1(Submarine submarine, Item item, Vector2 simPosition)
		{
			Vector2 position = simPosition;
			Submarine submarine2 = item.Submarine;
			Vector2 itemSubPos = (submarine2 != null) ? submarine2.SimPosition : Vector2.Zero;
			Vector2 subPos = (submarine != null) ? submarine.SimPosition : Vector2.Zero;
			if (submarine == null && item.Submarine != null)
			{
				position += itemSubPos;
			}
			else if (submarine != null && item.Submarine == null)
			{
				position -= subPos;
			}
			else if (submarine != item.Submarine && submarine != null)
			{
				position += itemSubPos;
				position -= subPos;
			}
			return position;
		}

		// Token: 0x0600021A RID: 538 RVA: 0x00011748 File Offset: 0x0000F948
		[CompilerGenerated]
		private bool <Update>g__bodyMovingTooFast|583_1(PhysicsBody body)
		{
			return body.LinearVelocity.LengthSquared() > 64f || (!this.InWater && body.LinearVelocity.Y < -5f);
		}

		// Token: 0x0600021B RID: 539 RVA: 0x00011788 File Offset: 0x0000F988
		[CompilerGenerated]
		private bool <Update>g__MustDeselect|583_0(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (this.IsAIControlled && !this.CanInteract && this.IsAttachedToController())
			{
				return false;
			}
			if (!this.CanInteractWith(item, true))
			{
				return true;
			}
			bool hasSelectableComponent = false;
			foreach (ItemComponent component in item.Components)
			{
				if (component.CanBeSelected && component.HasRequiredItems(this, false, null))
				{
					hasSelectableComponent = true;
					break;
				}
			}
			return !hasSelectableComponent;
		}

		// Token: 0x0600021D RID: 541 RVA: 0x00011836 File Offset: 0x0000FA36
		[CompilerGenerated]
		internal static void <TryAdjustAttackerSkill>g__IncreaseSkillLevel|627_0(Identifier skill, float damage, ref Character.<>c__DisplayClass627_0 A_2)
		{
			CharacterInfo characterInfo = A_2.attacker.Info;
			if (characterInfo == null)
			{
				return;
			}
			characterInfo.ApplySkillGain(skill, damage * SkillSettings.Current.SkillIncreasePerHostileDamage, false, 1f, false);
		}

		// Token: 0x0600021E RID: 542 RVA: 0x00011864 File Offset: 0x0000FA64
		[CompilerGenerated]
		internal static void <ApplyStatusEffects>g__ApplyToLimb|635_0(ActionType actionType, float deltaTime, StatusEffect statusEffect, Character character, Limb limb)
		{
			statusEffect.sourceBody = limb.body;
			statusEffect.Apply(actionType, deltaTime, character, limb, null);
		}

		// Token: 0x0600021F RID: 543 RVA: 0x00011894 File Offset: 0x0000FA94
		[CompilerGenerated]
		internal static string <Kill>g__GetCharacterType|642_1(Character character)
		{
			if (character.IsPlayer)
			{
				return "Player";
			}
			if (character.AIController is EnemyAIController)
			{
				return "Enemy" + character.SpeciesName.ToString();
			}
			if (character.AIController is HumanAIController && character.TeamID == CharacterTeamType.Team2)
			{
				return "EnemyHuman";
			}
			if (character.Info != null && character.TeamID == CharacterTeamType.Team1)
			{
				return "AICrew";
			}
			if (character.Info != null && character.TeamID == CharacterTeamType.FriendlyNPC)
			{
				return "FriendlyNPC";
			}
			return "Unknown";
		}

		// Token: 0x04000070 RID: 112
		private Address ownerClientAddress;

		// Token: 0x04000071 RID: 113
		private Option<AccountId> ownerClientAccountId;

		// Token: 0x04000072 RID: 114
		public bool ClientDisconnected;

		// Token: 0x04000073 RID: 115
		public float KillDisconnectedTimer;

		// Token: 0x04000074 RID: 116
		private bool networkUpdateSent;

		// Token: 0x04000075 RID: 117
		private double LastInputTime;

		// Token: 0x04000076 RID: 118
		public bool HealthUpdatePending;

		// Token: 0x04000077 RID: 119
		private readonly List<int> severedJointIndices = new List<int>();

		// Token: 0x04000078 RID: 120
		public static readonly List<Character> CharacterList = new List<Character>();

		// Token: 0x04000079 RID: 121
		public static int CharacterUpdateInterval = 1;

		// Token: 0x0400007A RID: 122
		private static int characterUpdateTick = 1;

		// Token: 0x0400007B RID: 123
		public const float MaxHighlightDistance = 150f;

		// Token: 0x0400007C RID: 124
		public const float MaxDragDistance = 200f;

		// Token: 0x0400007D RID: 125
		private bool initialized;

		// Token: 0x0400007E RID: 126
		private bool enabled;

		// Token: 0x0400007F RID: 127
		private bool disabledByEvent;

		// Token: 0x04000080 RID: 128
		public Hull PreviousHull;

		// Token: 0x04000081 RID: 129
		public Hull CurrentHull;

		// Token: 0x04000085 RID: 133
		public readonly Dictionary<Identifier, SerializableProperty> Properties;

		// Token: 0x04000086 RID: 134
		protected Key[] keys;

		// Token: 0x04000087 RID: 135
		private HumanPrefab humanPrefab;

		// Token: 0x04000088 RID: 136
		private Identifier? faction;

		// Token: 0x04000089 RID: 137
		private CharacterTeamType teamID;

		// Token: 0x0400008A RID: 138
		private CharacterTeamType? originalTeamID;

		// Token: 0x0400008B RID: 139
		private Wallet wallet;

		// Token: 0x0400008C RID: 140
		public readonly HashSet<LatchOntoAI> Latchers = new HashSet<LatchOntoAI>();

		// Token: 0x0400008D RID: 141
		public readonly HashSet<Projectile> AttachedProjectiles = new HashSet<Projectile>();

		// Token: 0x0400008E RID: 142
		protected readonly Dictionary<string, ActiveTeamChange> activeTeamChanges = new Dictionary<string, ActiveTeamChange>();

		// Token: 0x0400008F RID: 143
		protected ActiveTeamChange currentTeamChange;

		// Token: 0x04000090 RID: 144
		private const string OriginalChangeTeamIdentifier = "original";

		// Token: 0x04000092 RID: 146
		public bool IsCriminal;

		// Token: 0x04000093 RID: 147
		public bool IsActingOffensively;

		// Token: 0x04000094 RID: 148
		public bool IsHostileEscortee;

		// Token: 0x04000095 RID: 149
		public CombatAction CombatAction;

		// Token: 0x04000096 RID: 150
		public readonly AnimController AnimController;

		// Token: 0x04000097 RID: 151
		private Vector2 cursorPosition;

		// Token: 0x04000098 RID: 152
		protected float oxygenAvailable;

		// Token: 0x04000099 RID: 153
		public readonly string Seed;

		// Token: 0x0400009A RID: 154
		protected Item focusedItem;

		// Token: 0x0400009B RID: 155
		private Character selectedCharacter;

		// Token: 0x0400009C RID: 156
		private Character selectedBy;

		// Token: 0x0400009D RID: 157
		private const int maxLastAttackerCount = 4;

		// Token: 0x0400009E RID: 158
		private readonly List<Character.Attacker> lastAttackers = new List<Character.Attacker>();

		// Token: 0x040000A1 RID: 161
		public Entity LastDamageSource;

		// Token: 0x040000A2 RID: 162
		public AttackResult LastDamage;

		// Token: 0x040000A3 RID: 163
		private readonly Dictionary<ItemPrefab, double> itemSelectedDurations = new Dictionary<ItemPrefab, double>();

		// Token: 0x040000A4 RID: 164
		private double itemSelectedTime;

		// Token: 0x040000A6 RID: 166
		public readonly CharacterPrefab Prefab;

		// Token: 0x040000A7 RID: 167
		public readonly CharacterParams Params;

		// Token: 0x040000A9 RID: 169
		public LocalizedString TraitorCurrentObjective = "";

		// Token: 0x040000AA RID: 170
		private float attackCoolDown;

		// Token: 0x040000AB RID: 171
		private readonly Dictionary<ActionType, List<StatusEffect>> statusEffects = new Dictionary<ActionType, List<StatusEffect>>();

		// Token: 0x040000AD RID: 173
		private CharacterInfo info;

		// Token: 0x040000AE RID: 174
		private float hideFaceTimer;

		// Token: 0x040000AF RID: 175
		private Vector2 lastInventoryItemSetTransformPosition;

		// Token: 0x040000B4 RID: 180
		private Action<Character, Character> onCustomInteract;

		// Token: 0x040000B5 RID: 181
		public ConversationAction ActiveConversation;

		// Token: 0x040000B6 RID: 182
		public bool RequireConsciousnessForCustomInteract = true;

		// Token: 0x040000B7 RID: 183
		private float lockHandsTimer;

		// Token: 0x040000BA RID: 186
		private float lowPassMultiplier;

		// Token: 0x040000BB RID: 187
		private float obstructVisionAmount;

		// Token: 0x040000BC RID: 188
		private double pressureProtectionLastSet;

		// Token: 0x040000BD RID: 189
		private float pressureProtection;

		// Token: 0x040000BE RID: 190
		public const float KnockbackCooldown = 5f;

		// Token: 0x040000BF RID: 191
		public float KnockbackCooldownTimer;

		// Token: 0x040000C0 RID: 192
		private float ragdollingLockTimer;

		// Token: 0x040000C1 RID: 193
		public bool IsRagdolled;

		// Token: 0x040000C2 RID: 194
		public bool IsForceRagdolled;

		// Token: 0x040000C3 RID: 195
		public bool FollowCursor = true;

		// Token: 0x040000C6 RID: 198
		public bool DisableHealthWindow;

		// Token: 0x040000C7 RID: 199
		private bool speechImpedimentSet;

		// Token: 0x040000C8 RID: 200
		private float speechImpediment;

		// Token: 0x040000C9 RID: 201
		private float textChatVolume;

		// Token: 0x040000CD RID: 205
		private Item _selectedItem;

		// Token: 0x040000D0 RID: 208
		private bool isDead;

		// Token: 0x040000D3 RID: 211
		public bool GodMode;

		// Token: 0x040000D4 RID: 212
		public CampaignMode.InteractionType CampaignInteractionType;

		// Token: 0x040000D5 RID: 213
		public Identifier MerchantIdentifier;

		// Token: 0x040000D6 RID: 214
		private bool accessRemovedCharacterErrorShown;

		// Token: 0x040000D7 RID: 215
		public HashSet<Identifier> MarkedAsLooted = new HashSet<Identifier>();

		// Token: 0x040000D8 RID: 216
		public Character.OnDeathHandler OnDeath;

		// Token: 0x040000D9 RID: 217
		public Character.OnAttackedHandler OnAttacked;

		// Token: 0x040000DA RID: 218
		private static readonly ImmutableDictionary<Identifier, StatTypes> overrideStatTypes = new Dictionary<Identifier, StatTypes>
		{
			{
				new Identifier("helm"),
				StatTypes.HelmSkillOverride
			},
			{
				new Identifier("medical"),
				StatTypes.MedicalSkillOverride
			},
			{
				new Identifier("weapons"),
				StatTypes.WeaponsSkillOverride
			},
			{
				new Identifier("electrical"),
				StatTypes.ElectricalSkillOverride
			},
			{
				new Identifier("mechanical"),
				StatTypes.MechanicalSkillOverride
			}
		}.ToImmutableDictionary<Identifier, StatTypes>();

		// Token: 0x040000DD RID: 221
		private double disableRunningLastSet;

		// Token: 0x040000DE RID: 222
		public bool ToggleRun;

		// Token: 0x040000DF RID: 223
		private float greatestNegativeSpeedMultiplier = 1f;

		// Token: 0x040000E0 RID: 224
		private float greatestPositiveSpeedMultiplier = 1f;

		// Token: 0x040000E2 RID: 226
		private double propulsionSpeedMultiplierLastSet;

		// Token: 0x040000E3 RID: 227
		private float propulsionSpeedMultiplier;

		// Token: 0x040000E4 RID: 228
		private float greatestNegativeHealthMultiplier = 1f;

		// Token: 0x040000E5 RID: 229
		private float greatestPositiveHealthMultiplier = 1f;

		// Token: 0x040000E8 RID: 232
		private const float cursorFollowMargin = 40f;

		// Token: 0x040000E9 RID: 233
		private Character.AttackTargetData currentAttackTarget;

		// Token: 0x040000EA RID: 234
		private Stopwatch sw;

		// Token: 0x040000EB RID: 235
		private float _selectedItemPriority;

		// Token: 0x040000EC RID: 236
		private Item _foundItem;

		// Token: 0x040000ED RID: 237
		private float despawnTimer;

		// Token: 0x040000EE RID: 238
		private readonly float maxAIRange = 20000f;

		// Token: 0x040000EF RID: 239
		private readonly float aiTargetChangeSpeed = 5f;

		// Token: 0x040000F0 RID: 240
		private readonly List<AIChatMessage> aiChatMessageQueue = new List<AIChatMessage>();

		// Token: 0x040000F1 RID: 241
		private readonly Dictionary<Identifier, float> prevAiChatMessages = new Dictionary<Identifier, float>();

		// Token: 0x040000F2 RID: 242
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x040000F3 RID: 243
		private readonly HashSet<AttackContext> currentContexts = new HashSet<AttackContext>();

		// Token: 0x040000F4 RID: 244
		private readonly List<Hull> visibleHulls = new List<Hull>();

		// Token: 0x040000F5 RID: 245
		private readonly HashSet<Hull> tempList = new HashSet<Hull>();

		// Token: 0x040000F7 RID: 247
		private readonly List<CharacterTalent> characterTalents = new List<CharacterTalent>();

		// Token: 0x040000F8 RID: 248
		private readonly HashSet<Hull> sameRoomHulls = new HashSet<Hull>();

		// Token: 0x040000F9 RID: 249
		private readonly Dictionary<StatTypes, float> statValues = new Dictionary<StatTypes, float>();

		// Token: 0x040000FA RID: 250
		private readonly Dictionary<StatTypes, float> wearableStatValues = new Dictionary<StatTypes, float>();

		// Token: 0x040000FB RID: 251
		private readonly Dictionary<Identifier, float> wearableSkillModifiers = new Dictionary<Identifier, float>();

		// Token: 0x040000FC RID: 252
		private AbilityFlags abilityFlags;

		// Token: 0x040000FD RID: 253
		private readonly Dictionary<TalentResistanceIdentifier, float> abilityResistances = new Dictionary<TalentResistanceIdentifier, float>();

		// Token: 0x040000FE RID: 254
		private Character.InputNetFlags dequeuedInput;

		// Token: 0x040000FF RID: 255
		private Character.InputNetFlags prevDequeuedInput;

		// Token: 0x04000100 RID: 256
		public ushort LastNetworkUpdateID;

		// Token: 0x04000101 RID: 257
		public ushort LastProcessedID;

		// Token: 0x04000102 RID: 258
		private readonly List<Character.NetInputMem> memInput = new List<Character.NetInputMem>();

		// Token: 0x04000103 RID: 259
		private readonly List<CharacterStateInfo> memState = new List<CharacterStateInfo>();

		// Token: 0x04000104 RID: 260
		private readonly List<CharacterStateInfo> memLocalState = new List<CharacterStateInfo>();

		// Token: 0x04000105 RID: 261
		public float healthUpdateTimer;

		// Token: 0x04000106 RID: 262
		private float healthUpdateInterval;

		// Token: 0x04000107 RID: 263
		public bool isSynced;

		// Token: 0x0200050B RID: 1291
		public class Attacker
		{
			// Token: 0x040024ED RID: 9453
			public Character Character;

			// Token: 0x040024EE RID: 9454
			public float Damage;
		}

		// Token: 0x0200050C RID: 1292
		// (Invoke) Token: 0x0600488A RID: 18570
		public delegate void OnDeathHandler(Character character, CauseOfDeath causeOfDeath);

		// Token: 0x0200050D RID: 1293
		// (Invoke) Token: 0x0600488E RID: 18574
		public delegate void OnAttackedHandler(Character attacker, AttackResult attackResult);

		// Token: 0x0200050E RID: 1294
		private struct AttackTargetData
		{
			// Token: 0x17001384 RID: 4996
			// (get) Token: 0x06004891 RID: 18577 RVA: 0x001CE042 File Offset: 0x001CC242
			// (set) Token: 0x06004892 RID: 18578 RVA: 0x001CE04A File Offset: 0x001CC24A
			public Limb AttackLimb { readonly get; set; }

			// Token: 0x17001385 RID: 4997
			// (get) Token: 0x06004893 RID: 18579 RVA: 0x001CE053 File Offset: 0x001CC253
			// (set) Token: 0x06004894 RID: 18580 RVA: 0x001CE05B File Offset: 0x001CC25B
			public IDamageable DamageTarget { readonly get; set; }

			// Token: 0x17001386 RID: 4998
			// (get) Token: 0x06004895 RID: 18581 RVA: 0x001CE064 File Offset: 0x001CC264
			// (set) Token: 0x06004896 RID: 18582 RVA: 0x001CE06C File Offset: 0x001CC26C
			public Vector2 AttackPos { readonly get; set; }
		}

		// Token: 0x0200050F RID: 1295
		public enum EventType
		{
			// Token: 0x040024F3 RID: 9459
			InventoryState,
			// Token: 0x040024F4 RID: 9460
			Control,
			// Token: 0x040024F5 RID: 9461
			Status,
			// Token: 0x040024F6 RID: 9462
			Treatment,
			// Token: 0x040024F7 RID: 9463
			SetAttackTarget,
			// Token: 0x040024F8 RID: 9464
			ExecuteAttack,
			// Token: 0x040024F9 RID: 9465
			AssignCampaignInteraction,
			// Token: 0x040024FA RID: 9466
			ObjectiveManagerState,
			// Token: 0x040024FB RID: 9467
			TeamChange,
			// Token: 0x040024FC RID: 9468
			AddToCrew,
			// Token: 0x040024FD RID: 9469
			UpdateExperience,
			// Token: 0x040024FE RID: 9470
			UpdateTalents,
			// Token: 0x040024FF RID: 9471
			UpdateSkills,
			// Token: 0x04002500 RID: 9472
			UpdateMoney,
			// Token: 0x04002501 RID: 9473
			UpdatePermanentStats,
			// Token: 0x04002502 RID: 9474
			RemoveFromCrew,
			// Token: 0x04002503 RID: 9475
			LatchOntoTarget,
			// Token: 0x04002504 RID: 9476
			UpdateTalentRefundPoints,
			// Token: 0x04002505 RID: 9477
			ConfirmTalentRefund,
			// Token: 0x04002506 RID: 9478
			MinValue = 0,
			// Token: 0x04002507 RID: 9479
			MaxValue = 18
		}

		// Token: 0x02000510 RID: 1296
		private interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x17001387 RID: 4999
			// (get) Token: 0x06004897 RID: 18583
			Character.EventType EventType { get; }
		}

		// Token: 0x02000511 RID: 1297
		public readonly struct InventoryStateEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001388 RID: 5000
			// (get) Token: 0x06004898 RID: 18584 RVA: 0x001CE075 File Offset: 0x001CC275
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.InventoryState;
				}
			}

			// Token: 0x06004899 RID: 18585 RVA: 0x001CE078 File Offset: 0x001CC278
			public InventoryStateEventData(Range slotRange)
			{
				this.SlotRange = slotRange;
			}

			// Token: 0x04002508 RID: 9480
			public readonly Range SlotRange;
		}

		// Token: 0x02000512 RID: 1298
		public readonly struct ControlEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001389 RID: 5001
			// (get) Token: 0x0600489A RID: 18586 RVA: 0x001CE081 File Offset: 0x001CC281
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.Control;
				}
			}

			// Token: 0x0600489B RID: 18587 RVA: 0x001CE084 File Offset: 0x001CC284
			public ControlEventData(Client owner)
			{
				this.Owner = owner;
			}

			// Token: 0x04002509 RID: 9481
			public readonly Client Owner;
		}

		// Token: 0x02000513 RID: 1299
		public struct CharacterStatusEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700138A RID: 5002
			// (get) Token: 0x0600489C RID: 18588 RVA: 0x001CE08D File Offset: 0x001CC28D
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.Status;
				}
			}

			// Token: 0x0600489D RID: 18589 RVA: 0x001CE090 File Offset: 0x001CC290
			public CharacterStatusEventData(bool forceAfflictionData)
			{
				this.ForceAfflictionData = forceAfflictionData;
			}

			// Token: 0x0400250A RID: 9482
			public bool ForceAfflictionData;
		}

		// Token: 0x02000514 RID: 1300
		public struct TreatmentEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700138B RID: 5003
			// (get) Token: 0x0600489E RID: 18590 RVA: 0x001CE099 File Offset: 0x001CC299
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.Treatment;
				}
			}
		}

		// Token: 0x02000515 RID: 1301
		private interface IAttackEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700138C RID: 5004
			// (get) Token: 0x0600489F RID: 18591
			Limb AttackLimb { get; }

			// Token: 0x1700138D RID: 5005
			// (get) Token: 0x060048A0 RID: 18592
			IDamageable TargetEntity { get; }

			// Token: 0x1700138E RID: 5006
			// (get) Token: 0x060048A1 RID: 18593
			Limb TargetLimb { get; }

			// Token: 0x1700138F RID: 5007
			// (get) Token: 0x060048A2 RID: 18594
			Vector2 TargetSimPos { get; }
		}

		// Token: 0x02000516 RID: 1302
		public struct SetAttackTargetEventData : Character.IAttackEventData, Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001390 RID: 5008
			// (get) Token: 0x060048A3 RID: 18595 RVA: 0x001CE09C File Offset: 0x001CC29C
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.SetAttackTarget;
				}
			}

			// Token: 0x17001391 RID: 5009
			// (get) Token: 0x060048A4 RID: 18596 RVA: 0x001CE09F File Offset: 0x001CC29F
			public readonly Limb AttackLimb { get; }

			// Token: 0x17001392 RID: 5010
			// (get) Token: 0x060048A5 RID: 18597 RVA: 0x001CE0A7 File Offset: 0x001CC2A7
			public readonly IDamageable TargetEntity { get; }

			// Token: 0x17001393 RID: 5011
			// (get) Token: 0x060048A6 RID: 18598 RVA: 0x001CE0AF File Offset: 0x001CC2AF
			public readonly Limb TargetLimb { get; }

			// Token: 0x17001394 RID: 5012
			// (get) Token: 0x060048A7 RID: 18599 RVA: 0x001CE0B7 File Offset: 0x001CC2B7
			public readonly Vector2 TargetSimPos { get; }

			// Token: 0x060048A8 RID: 18600 RVA: 0x001CE0BF File Offset: 0x001CC2BF
			public SetAttackTargetEventData(Limb attackLimb, IDamageable targetEntity, Limb targetLimb, Vector2 targetSimPos)
			{
				this.AttackLimb = attackLimb;
				this.TargetEntity = targetEntity;
				this.TargetLimb = targetLimb;
				this.TargetSimPos = targetSimPos;
			}
		}

		// Token: 0x02000517 RID: 1303
		public struct ExecuteAttackEventData : Character.IAttackEventData, Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001395 RID: 5013
			// (get) Token: 0x060048A9 RID: 18601 RVA: 0x001CE0DE File Offset: 0x001CC2DE
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.ExecuteAttack;
				}
			}

			// Token: 0x17001396 RID: 5014
			// (get) Token: 0x060048AA RID: 18602 RVA: 0x001CE0E1 File Offset: 0x001CC2E1
			public readonly Limb AttackLimb { get; }

			// Token: 0x17001397 RID: 5015
			// (get) Token: 0x060048AB RID: 18603 RVA: 0x001CE0E9 File Offset: 0x001CC2E9
			public readonly IDamageable TargetEntity { get; }

			// Token: 0x17001398 RID: 5016
			// (get) Token: 0x060048AC RID: 18604 RVA: 0x001CE0F1 File Offset: 0x001CC2F1
			public readonly Limb TargetLimb { get; }

			// Token: 0x17001399 RID: 5017
			// (get) Token: 0x060048AD RID: 18605 RVA: 0x001CE0F9 File Offset: 0x001CC2F9
			public readonly Vector2 TargetSimPos { get; }

			// Token: 0x060048AE RID: 18606 RVA: 0x001CE101 File Offset: 0x001CC301
			public ExecuteAttackEventData(Limb attackLimb, IDamageable targetEntity, Limb targetLimb, Vector2 targetSimPos)
			{
				this.AttackLimb = attackLimb;
				this.TargetEntity = targetEntity;
				this.TargetLimb = targetLimb;
				this.TargetSimPos = targetSimPos;
			}
		}

		// Token: 0x02000518 RID: 1304
		public struct AssignCampaignInteractionEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700139A RID: 5018
			// (get) Token: 0x060048AF RID: 18607 RVA: 0x001CE120 File Offset: 0x001CC320
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.AssignCampaignInteraction;
				}
			}
		}

		// Token: 0x02000519 RID: 1305
		public struct ObjectiveManagerStateEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700139B RID: 5019
			// (get) Token: 0x060048B0 RID: 18608 RVA: 0x001CE123 File Offset: 0x001CC323
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.ObjectiveManagerState;
				}
			}

			// Token: 0x060048B1 RID: 18609 RVA: 0x001CE126 File Offset: 0x001CC326
			public ObjectiveManagerStateEventData(AIObjectiveManager.ObjectiveType objectiveType)
			{
				this.ObjectiveType = objectiveType;
			}

			// Token: 0x04002513 RID: 9491
			public readonly AIObjectiveManager.ObjectiveType ObjectiveType;
		}

		// Token: 0x0200051A RID: 1306
		public readonly struct LatchedOntoTargetEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700139C RID: 5020
			// (get) Token: 0x060048B2 RID: 18610 RVA: 0x001CE12F File Offset: 0x001CC32F
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.LatchOntoTarget;
				}
			}

			// Token: 0x060048B3 RID: 18611 RVA: 0x001CE134 File Offset: 0x001CC334
			private LatchedOntoTargetEventData(Character character, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this.TargetCharacterID = 0;
				this.TargetStructureID = 0;
				this.TargetLevelWallIndex = -1;
				this.AttachSurfaceNormal = Vector2.Zero;
				this.AttachPos = Vector2.Zero;
				this.CharacterSimPos = character.SimPosition;
				this.IsLatched = true;
				this.AttachSurfaceNormal = attachSurfaceNormal;
				this.AttachPos = attachPos;
			}

			// Token: 0x060048B4 RID: 18612 RVA: 0x001CE18D File Offset: 0x001CC38D
			public LatchedOntoTargetEventData(Character character, Character targetCharacter, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this = new Character.LatchedOntoTargetEventData(character, attachSurfaceNormal, attachPos);
				this.TargetCharacterID = targetCharacter.ID;
			}

			// Token: 0x060048B5 RID: 18613 RVA: 0x001CE1A5 File Offset: 0x001CC3A5
			public LatchedOntoTargetEventData(Character character, Structure targetStructure, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this = new Character.LatchedOntoTargetEventData(character, attachSurfaceNormal, attachPos);
				this.TargetStructureID = targetStructure.ID;
			}

			// Token: 0x060048B6 RID: 18614 RVA: 0x001CE1BD File Offset: 0x001CC3BD
			public LatchedOntoTargetEventData(Character character, VoronoiCell levelWall, Vector2 attachSurfaceNormal, Vector2 attachPos)
			{
				this = new Character.LatchedOntoTargetEventData(character, attachSurfaceNormal, attachPos);
				this.TargetLevelWallIndex = Level.Loaded.GetAllCells().IndexOf(levelWall);
			}

			// Token: 0x060048B7 RID: 18615 RVA: 0x001CE1DF File Offset: 0x001CC3DF
			public LatchedOntoTargetEventData()
			{
				this.TargetCharacterID = 0;
				this.TargetStructureID = 0;
				this.TargetLevelWallIndex = -1;
				this.AttachSurfaceNormal = Vector2.Zero;
				this.AttachPos = Vector2.Zero;
				this.CharacterSimPos = Vector2.Zero;
				this.IsLatched = false;
			}

			// Token: 0x04002514 RID: 9492
			public readonly bool IsLatched;

			// Token: 0x04002515 RID: 9493
			public readonly ushort TargetCharacterID;

			// Token: 0x04002516 RID: 9494
			public readonly ushort TargetStructureID;

			// Token: 0x04002517 RID: 9495
			public readonly int TargetLevelWallIndex;

			// Token: 0x04002518 RID: 9496
			public readonly Vector2 AttachSurfaceNormal;

			// Token: 0x04002519 RID: 9497
			public readonly Vector2 AttachPos;

			// Token: 0x0400251A RID: 9498
			public readonly Vector2 CharacterSimPos;
		}

		// Token: 0x0200051B RID: 1307
		private struct TeamChangeEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700139D RID: 5021
			// (get) Token: 0x060048B8 RID: 18616 RVA: 0x001CE21E File Offset: 0x001CC41E
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.TeamChange;
				}
			}
		}

		// Token: 0x0200051C RID: 1308
		[NetworkSerialize(197)]
		public readonly struct ItemTeamChange : INetSerializableStruct, IEquatable<Character.ItemTeamChange>
		{
			// Token: 0x060048B9 RID: 18617 RVA: 0x001CE221 File Offset: 0x001CC421
			public ItemTeamChange(CharacterTeamType TeamId, ImmutableArray<ushort> ItemIds)
			{
				this.TeamId = TeamId;
				this.ItemIds = ItemIds;
			}

			// Token: 0x1700139E RID: 5022
			// (get) Token: 0x060048BA RID: 18618 RVA: 0x001CE231 File Offset: 0x001CC431
			// (set) Token: 0x060048BB RID: 18619 RVA: 0x001CE239 File Offset: 0x001CC439
			public CharacterTeamType TeamId { get; set; }

			// Token: 0x1700139F RID: 5023
			// (get) Token: 0x060048BC RID: 18620 RVA: 0x001CE242 File Offset: 0x001CC442
			// (set) Token: 0x060048BD RID: 18621 RVA: 0x001CE24A File Offset: 0x001CC44A
			public ImmutableArray<ushort> ItemIds { get; set; }

			// Token: 0x060048BE RID: 18622 RVA: 0x001CE254 File Offset: 0x001CC454
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ItemTeamChange");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060048BF RID: 18623 RVA: 0x001CE2A0 File Offset: 0x001CC4A0
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("TeamId = ");
				builder.Append(this.TeamId.ToString());
				builder.Append(", ItemIds = ");
				builder.Append(this.ItemIds.ToString());
				return true;
			}

			// Token: 0x060048C0 RID: 18624 RVA: 0x001CE2FC File Offset: 0x001CC4FC
			[CompilerGenerated]
			public static bool operator !=(Character.ItemTeamChange left, Character.ItemTeamChange right)
			{
				return !(left == right);
			}

			// Token: 0x060048C1 RID: 18625 RVA: 0x001CE308 File Offset: 0x001CC508
			[CompilerGenerated]
			public static bool operator ==(Character.ItemTeamChange left, Character.ItemTeamChange right)
			{
				return left.Equals(right);
			}

			// Token: 0x060048C2 RID: 18626 RVA: 0x001CE312 File Offset: 0x001CC512
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<CharacterTeamType>.Default.GetHashCode(this.<TeamId>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<ItemIds>k__BackingField);
			}

			// Token: 0x060048C3 RID: 18627 RVA: 0x001CE33B File Offset: 0x001CC53B
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Character.ItemTeamChange && this.Equals((Character.ItemTeamChange)obj);
			}

			// Token: 0x060048C4 RID: 18628 RVA: 0x001CE353 File Offset: 0x001CC553
			[CompilerGenerated]
			public bool Equals(Character.ItemTeamChange other)
			{
				return EqualityComparer<CharacterTeamType>.Default.Equals(this.<TeamId>k__BackingField, other.<TeamId>k__BackingField) && EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<ItemIds>k__BackingField, other.<ItemIds>k__BackingField);
			}

			// Token: 0x060048C5 RID: 18629 RVA: 0x001CE385 File Offset: 0x001CC585
			[CompilerGenerated]
			public void Deconstruct(out CharacterTeamType TeamId, out ImmutableArray<ushort> ItemIds)
			{
				TeamId = this.TeamId;
				ItemIds = this.ItemIds;
			}
		}

		// Token: 0x0200051D RID: 1309
		public struct AddToCrewEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A0 RID: 5024
			// (get) Token: 0x060048C6 RID: 18630 RVA: 0x001CE39B File Offset: 0x001CC59B
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.AddToCrew;
				}
			}

			// Token: 0x060048C7 RID: 18631 RVA: 0x001CE39F File Offset: 0x001CC59F
			public AddToCrewEventData(CharacterTeamType teamType, IEnumerable<Item> inventoryItems)
			{
				this.ItemTeamChange = new Character.ItemTeamChange(teamType, (from it in inventoryItems
				select it.ID).ToImmutableArray<ushort>());
			}

			// Token: 0x0400251D RID: 9501
			public readonly Character.ItemTeamChange ItemTeamChange;
		}

		// Token: 0x0200051E RID: 1310
		public struct RemoveFromCrewEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A1 RID: 5025
			// (get) Token: 0x060048C8 RID: 18632 RVA: 0x001CE3D7 File Offset: 0x001CC5D7
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.RemoveFromCrew;
				}
			}

			// Token: 0x060048C9 RID: 18633 RVA: 0x001CE3DB File Offset: 0x001CC5DB
			public RemoveFromCrewEventData(CharacterTeamType teamType, IEnumerable<Item> inventoryItems)
			{
				this.ItemTeamChange = new Character.ItemTeamChange(teamType, (from it in inventoryItems
				select it.ID).ToImmutableArray<ushort>());
			}

			// Token: 0x0400251E RID: 9502
			public readonly Character.ItemTeamChange ItemTeamChange;
		}

		// Token: 0x0200051F RID: 1311
		public struct UpdateExperienceEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A2 RID: 5026
			// (get) Token: 0x060048CA RID: 18634 RVA: 0x001CE413 File Offset: 0x001CC613
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateExperience;
				}
			}
		}

		// Token: 0x02000520 RID: 1312
		public struct UpdateTalentsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A3 RID: 5027
			// (get) Token: 0x060048CB RID: 18635 RVA: 0x001CE417 File Offset: 0x001CC617
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateTalents;
				}
			}
		}

		// Token: 0x02000521 RID: 1313
		public struct UpdateSkillsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A4 RID: 5028
			// (get) Token: 0x060048CC RID: 18636 RVA: 0x001CE41B File Offset: 0x001CC61B
			public readonly Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateSkills;
				}
			}

			// Token: 0x060048CD RID: 18637 RVA: 0x001CE41F File Offset: 0x001CC61F
			public UpdateSkillsEventData(Identifier skillIdentifier, bool forceNotification)
			{
				this.SkillIdentifier = skillIdentifier;
				this.ForceNotification = forceNotification;
			}

			// Token: 0x0400251F RID: 9503
			public readonly bool ForceNotification;

			// Token: 0x04002520 RID: 9504
			public readonly Identifier SkillIdentifier;
		}

		// Token: 0x02000522 RID: 1314
		private struct UpdateMoneyEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A5 RID: 5029
			// (get) Token: 0x060048CE RID: 18638 RVA: 0x001CE42F File Offset: 0x001CC62F
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateMoney;
				}
			}
		}

		// Token: 0x02000523 RID: 1315
		public struct UpdatePermanentStatsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A6 RID: 5030
			// (get) Token: 0x060048CF RID: 18639 RVA: 0x001CE433 File Offset: 0x001CC633
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdatePermanentStats;
				}
			}

			// Token: 0x060048D0 RID: 18640 RVA: 0x001CE437 File Offset: 0x001CC637
			public UpdatePermanentStatsEventData(StatTypes statType)
			{
				this.StatType = statType;
			}

			// Token: 0x04002521 RID: 9505
			public readonly StatTypes StatType;
		}

		// Token: 0x02000524 RID: 1316
		public struct UpdateRefundPointsEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A7 RID: 5031
			// (get) Token: 0x060048D1 RID: 18641 RVA: 0x001CE440 File Offset: 0x001CC640
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.UpdateTalentRefundPoints;
				}
			}
		}

		// Token: 0x02000525 RID: 1317
		public struct ConfirmRefundEventData : Character.IEventData, NetEntityEvent.IData
		{
			// Token: 0x170013A8 RID: 5032
			// (get) Token: 0x060048D2 RID: 18642 RVA: 0x001CE444 File Offset: 0x001CC644
			public Character.EventType EventType
			{
				get
				{
					return Character.EventType.ConfirmTalentRefund;
				}
			}
		}

		// Token: 0x02000526 RID: 1318
		[Flags]
		private enum InputNetFlags : ushort
		{
			// Token: 0x04002523 RID: 9507
			None = 0,
			// Token: 0x04002524 RID: 9508
			Left = 1,
			// Token: 0x04002525 RID: 9509
			Right = 2,
			// Token: 0x04002526 RID: 9510
			Up = 4,
			// Token: 0x04002527 RID: 9511
			Down = 8,
			// Token: 0x04002528 RID: 9512
			FacingLeft = 16,
			// Token: 0x04002529 RID: 9513
			Run = 32,
			// Token: 0x0400252A RID: 9514
			Crouch = 64,
			// Token: 0x0400252B RID: 9515
			Select = 128,
			// Token: 0x0400252C RID: 9516
			Use = 256,
			// Token: 0x0400252D RID: 9517
			Aim = 512,
			// Token: 0x0400252E RID: 9518
			Attack = 1024,
			// Token: 0x0400252F RID: 9519
			Ragdoll = 2048,
			// Token: 0x04002530 RID: 9520
			Health = 4096,
			// Token: 0x04002531 RID: 9521
			Grab = 8192,
			// Token: 0x04002532 RID: 9522
			Deselect = 16384,
			// Token: 0x04002533 RID: 9523
			Shoot = 32768,
			// Token: 0x04002534 RID: 9524
			MaxVal = 65535
		}

		// Token: 0x02000527 RID: 1319
		private struct NetInputMem
		{
			// Token: 0x04002535 RID: 9525
			public Character.InputNetFlags states;

			// Token: 0x04002536 RID: 9526
			public ushort intAim;

			// Token: 0x04002537 RID: 9527
			public ushort interact;

			// Token: 0x04002538 RID: 9528
			public ushort networkUpdateID;
		}
	}
}
